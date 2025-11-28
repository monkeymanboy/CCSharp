using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CCSharp.Attributes;
using CCSharp.Enums;
using CCSharp.RedIL.Enums;
using CCSharp.RedIL.Nodes;
using CCSharp.RedIL.Nodes.Internal;
using CCSharp.RedIL.Resolving;
using CCSharp.RedIL.Utilities;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp.Resolver;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Syntax.PatternMatching;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using CCSharp.RedIL.Extensions;
using CCSharp.RedIL.Resolving.Attributes;
using CCSharp.RedIL.Resolving.CommonResolvers;
using ICSharpCode.Decompiler.CSharp;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;
using PrimitiveType = ICSharpCode.Decompiler.CSharp.Syntax.PrimitiveType;
using SwitchSection = ICSharpCode.Decompiler.CSharp.Syntax.SwitchSection;

namespace CCSharp.RedIL;

public class CSharpCompiler
{
    public MainResolver MainResolver { get; }
    public string LuaClass { get; set; }
    public DictionaryTableDefinitionNode ClassInitializerTableNode { get; set; }
    public RedILNode PostConstructorObjectInitializerNode { get; set; }

    class AstVisitor : IAstVisitor<RedILNode>
    {
        private CSharpCompiler _compiler;

        private MainResolver _resolver;

        private HashSet<string> _identifiers;

        private RootNode _root;

        private Stack<BlockNode> _blockStack;

        private IType currentType;

        private IType currentBaseType;

        public AstVisitor(CSharpCompiler compiler)
        {
            _compiler = compiler;
            _resolver = _compiler.MainResolver;
            _identifiers = new HashSet<string>();
            /*
            _identifiers.Add(csharp.ArgumentsVariableName);
            _identifiers.Add(csharp.CursorVariableName);
            _identifiers.Add(csharp.KeysVariableName);*/
            _blockStack = new Stack<BlockNode>();
        }

        /*
         * Not all of C#'s syntax tree is compiled
         * So the class is divided into `Used` and `Unused` sections
         */

        #region Used

        public RedILNode VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
        {
            return VisitArrayInitializerExpression(arrayCreateExpression.Initializer);
        }

        public RedILNode VisitAsExpression(AsExpression asExpression)
        {
            ResolveResult resolveResult = asExpression.Type.Annotation<ResolveResult>();
            if (resolveResult != null)
            {
                //TODO there's a bit of duplication here across this and VisitIsExpression that can be reduced
                Type targetType = Assembly.Load(resolveResult.Type.GetDefinition().ParentModule.FullAssemblyName).GetType(resolveResult.Type.ReflectionName);
                LuaTableTypeCheckAttribute typeCheckAttribute = targetType.GetCustomAttribute<LuaTableTypeCheckAttribute>();
                if (typeCheckAttribute != null)
                {
                    LuaImplicitTypeArgumentAttribute implicitTypeArgument = targetType.GetCustomAttribute<LuaImplicitTypeArgumentAttribute>();
                    ConstantValueNode typeNode = new ConstantValueNode(DataValueType.String, implicitTypeArgument.Argument);
                    ExpressionNode asExpressionNode = asExpression.Expression.AcceptVisitor(this) as ExpressionNode;
                    if (typeCheckAttribute.Method != null)
                    {
                        return new BinaryExpressionNode(DataValueType.Dictionary, BinaryExpressionOperator.Or, new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.And,
                            new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.And,
                                new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.NotEqual, asExpressionNode, new NilNode()),
                                new CallCustomMethodNode(typeCheckAttribute.Method, null, null, false,
                                new List<ExpressionNode>
                                {
                                    asExpressionNode,
                                    typeNode
                                })), asExpressionNode), new NilNode());
                    }
                    else
                    {
                        return new BinaryExpressionNode(DataValueType.Dictionary, BinaryExpressionOperator.Or, new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.And,
                            new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.And,
                                new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.NotEqual, asExpressionNode, new NilNode()),
                            new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.Equal,
                                new TableKeyAccessNode(asExpressionNode, new ConstantValueNode(typeCheckAttribute.TableAccessor is string ? DataValueType.String : DataValueType.Integer, typeCheckAttribute.TableAccessor), DataValueType.Array),
                                typeNode)), asExpressionNode), new NilNode());
                    }
                }
                return new BinaryExpressionNode(DataValueType.Dictionary, BinaryExpressionOperator.Or, new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.And,
                    new CallLuaFunctionNode(LuaFunction.TableArrayContains, DataValueType.Boolean,
                        new List<ExpressionNode>
                        {
                            new TableKeyAccessNode(asExpression.Expression.AcceptVisitor(this) as ExpressionNode, new ConstantValueNode(DataValueType.String, "_CSharpTypes"), DataValueType.Array),
                            targetType.GetTypeValueNode(_resolver.Flags)
                        }), asExpression.Expression.AcceptVisitor(this) as ExpressionNode), new NilNode());
            }
            return asExpression.Expression.AcceptVisitor(this);
        }

        public RedILNode VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            if (assignmentExpression.Parent.NodeType != NodeType.Statement)
            {
                throw new RedILException("Assigment is only possible within a statement");
            }

            var left = CastUtilities.CastRedILNode<ExpressionNode>(assignmentExpression.Left.AcceptVisitor(this));
            var right = CastUtilities.CastRedILNode<ExpressionNode>(assignmentExpression.Right.AcceptVisitor(this));

            if (assignmentExpression.Operator != AssignmentOperatorType.Assign)
            {
                var op = OperatorUtilities.BinaryOperator(assignmentExpression.Operator);
                right = CreateBinaryExpression(op, left, right);
            }

            return new AssignNode(left, right);
        }

        public RedILNode VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
        {
            var op = OperatorUtilities.BinaryOperator(binaryOperatorExpression.Operator);
            var left = CastUtilities.CastRedILNode<ExpressionNode>(
                binaryOperatorExpression.Left.AcceptVisitor(this));
            var right = CastUtilities.CastRedILNode<ExpressionNode>(
                binaryOperatorExpression.Right.AcceptVisitor(this));

            return CreateBinaryExpression(op, left, right);
        }

        public RedILNode VisitBlockStatement(BlockStatement blockStatement)
        {
            var block = new BlockNode();

            bool init = true;
            if (_root is null)
            {
                init = false;
                _root = new RootNode(block) { Identifiers = _identifiers };
            }

            /* No need to flatten implicit blocks for now */
            /*
            var children = blockStatement.Children
                .SelectMany(child => FlattenImplicitBlocks(child.AcceptVisitor(this)))
                .Where(child => child.Type != RedILNodeType.Empty);*/

            _blockStack.Push(block);
            foreach (var child in blockStatement.Children)
            {
                var visited = child.AcceptVisitor(this);
                if (visited.Type == RedILNodeType.Block)
                {
                    foreach (var innerChild in ((BlockNode)visited).Children)
                    {
                        if (innerChild.Type != RedILNodeType.Empty)
                        {
                            block.Children.Add(innerChild);
                        }
                    }
                }
                else if (visited.Type != RedILNodeType.Empty)
                {
                    block.Children.Add(visited);
                }
            }

            _blockStack.Pop();

            if (!init)
            {
                return _root;
            }

            return block;
        }

        public RedILNode VisitBreakStatement(BreakStatement breakStatement)
        {
            return new BreakNode();
        }

        public RedILNode VisitCastExpression(CastExpression castExpression)
        {
            RedILNode FromPrimitive(PrimitiveType primitiveType)
            {
                var resType = primitiveType is null
                    ? DataValueType.Unknown
                    : TypeUtilities.GetValueType(primitiveType.KnownTypeCode);
                var argument =
                    CastUtilities.CastRedILNode<ExpressionNode>(castExpression.Expression.AcceptVisitor(this));
                if (resType == DataValueType.Unknown || argument.Type == RedILNodeType.Nil)
                {
                    return argument;
                }

                return new CastNode(resType, argument);
            }

            if (castExpression.Type is ComposedType && ((ComposedType)castExpression.Type).HasNullableSpecifier)
            {
                return FromPrimitive(((ComposedType)castExpression.Type).BaseType as PrimitiveType);
            }

            return FromPrimitive(castExpression.Type as PrimitiveType);
        }

        public RedILNode VisitComment(Comment comment)
        {
            return new EmptyNode();
        }

        public RedILNode VisitConditionalExpression(ConditionalExpression conditionalExpression)
        {
            return new ConditionalExpressionNode(
                CastUtilities.CastRedILNode<ExpressionNode>(conditionalExpression.Condition.AcceptVisitor(this)),
                CastUtilities.CastRedILNode<ExpressionNode>(
                    conditionalExpression.TrueExpression.AcceptVisitor(this)),
                CastUtilities.CastRedILNode<ExpressionNode>(
                    conditionalExpression.FalseExpression.AcceptVisitor(this)));
        }

        public RedILNode VisitContinueStatement(ContinueStatement continueStatement)
        {
            return new ContinueNode();
        }

        public RedILNode VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression)
        {
            return new NilNode();
        }

        public RedILNode VisitDocumentationReference(DocumentationReference documentationReference)
        {
            return new EmptyNode();
        }

        public RedILNode VisitDoWhileStatement(DoWhileStatement doWhileStatement)
        {
            return new DoWhileNode(
                CastUtilities.CastRedILNode<ExpressionNode>(doWhileStatement.Condition.AcceptVisitor(this)),
                RemoveFirstLevelContinue(
                    CastUtilities.CastRedILNode<BlockNode>(doWhileStatement.EmbeddedStatement
                        .AcceptVisitor(this))));
        }

        public RedILNode VisitEmptyStatement(EmptyStatement emptyStatement)
        {
            return new EmptyNode();
        }

        public RedILNode VisitExpressionStatement(ExpressionStatement expressionStatement)
        {
            return expressionStatement.Expression.AcceptVisitor(this);
        }

        public RedILNode VisitForeachStatement(ForeachStatement foreachStatement)
        {
            var over = CastUtilities.CastRedILNode<ExpressionNode>(
                foreachStatement.InExpression.AcceptVisitor(this));
            var body = CastUtilities.CastRedILNode<BlockNode>(
                foreachStatement.EmbeddedStatement.AcceptVisitor(this));

            return new IteratorLoopNode(foreachStatement.VariableName, over, body);
        }

        public RedILNode VisitForStatement(ForStatement forStatement)
        {
            var blockNode = new BlockNode()
            {
                Explicit = false
            };

            _blockStack.Push(blockNode);
            foreach (var initializer in forStatement.Initializers)
            {
                var visited = initializer.AcceptVisitor(this);
                blockNode.Children.Add(visited);
            }

            var whileNode = new WhileNode();
            whileNode.Condition =
                CastUtilities.CastRedILNode<ExpressionNode>(forStatement.Condition.AcceptVisitor(this));
            whileNode.Body = RemoveFirstLevelContinue(CastUtilities.CastRedILNode<BlockNode>(
                forStatement.EmbeddedStatement.AcceptVisitor(this)));

            foreach (var iterator in forStatement.Iterators)
            {
                var visited = iterator.AcceptVisitor(this);
                whileNode.Body.Children.Add(visited);
            }

            blockNode.Children.Add(whileNode);
            _blockStack.Pop();

            return blockNode;
        }

        public RedILNode VisitIdentifierExpression(IdentifierExpression identifierExpression)
        {
            string identifier = identifierExpression.Identifier;
            MemberResolveResult memberResolveResult = identifierExpression.Annotation<MemberResolveResult>();
            if (memberResolveResult != null)
            {
                bool isStatic = memberResolveResult.Member.IsStatic;
                var resolver = _resolver.ResolveMember(isStatic, currentType, identifierExpression.Identifier);
                if (resolver is TableAccessMemberResolver tableAccessMemberResolver)
                {
                    if (tableAccessMemberResolver.Key is string customMemberName)
                        identifier = customMemberName;
                }

                if (_compiler.LuaClass != null)
                {
                    if (isStatic)
                        identifier = $"{_compiler.LuaClass}.{identifier}";
                    else
                        identifier = $"self.{identifier}";
                }
            }
            var resType = _compiler.ResolveExpressionType(identifierExpression);
            return new IdentifierNode(identifier, resType);
        }

        public RedILNode VisitIfElseStatement(IfElseStatement ifElseStatement)
        {
            //TODO This is to get around an issue where ICSharpCode.Decompiler adds these surrounding switch expressions, I assume once I update it this won't be necessary
            if (ifElseStatement.Condition.ToString() == "1 == 0") return new EmptyNode();
            
            var ifNode = new IfNode();
            ifNode.Ifs = new[]
            {
                new KeyValuePair<ExpressionNode, RedILNode>(
                    CastUtilities.CastRedILNode<ExpressionNode>(ifElseStatement.Condition.AcceptVisitor(this)),
                    NullIfNil(ifElseStatement.TrueStatement.AcceptVisitor(this)))
            }.Where(p => !(p.Value is null)).Where(p => !p.Key.EqualOrNull(ExpressionNode.False)).ToArray();
            var truth = ifNode.Ifs.FirstOrDefault(p => p.Key.EqualOrNull(ExpressionNode.True));
            if (!(truth.Key is null))
            {
                return truth.Value;
            }

            ifNode.IfElse = NullIfNil(ifElseStatement.FalseStatement.AcceptVisitor(this));
            if (ifNode.Ifs.Count == 0 && ifNode.IfElse is null)
            {
                return new EmptyNode();
            }
            else if (ifNode.Ifs.Count == 0)
            {
                return ifNode.IfElse;
            }

            return ifNode;
        }

        public RedILNode VisitIndexerExpression(IndexerExpression indexerExpression)
        {
            var target = CastUtilities.CastRedILNode<ExpressionNode>(indexerExpression.Target.AcceptVisitor(this));
            var type = _compiler.ResolveExpressionType(indexerExpression);
            foreach (var arg in indexerExpression.Arguments)
            {
                var argVisited = CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this));

                // In LUA, array indices start at 1
                if ((target.DataType == DataValueType.Array || target.DataType == DataValueType.String) &&
                    argVisited.DataType == DataValueType.Integer)
                {
                    if (argVisited.Type == RedILNodeType.Constant)
                    {
                        argVisited = new ConstantValueNode(DataValueType.Integer,
                            int.Parse(((ConstantValueNode)argVisited).Value.ToString()) + 1);
                    }
                    else
                    {
                        argVisited = new BinaryExpressionNode(DataValueType.Integer, BinaryExpressionOperator.Add,
                            argVisited, new ConstantValueNode(DataValueType.Integer, 1));
                    }
                }


                if (target.DataType == DataValueType.Array || target.DataType == DataValueType.Dictionary || target.DataType == DataValueType.Unknown)
                {
                    target = new TableKeyAccessNode(target, argVisited, type);
                }
                else if (target.DataType == DataValueType.String)
                {
                    target = new CallBuiltinLuaMethodNode(LuaBuiltinMethod.StringSub,
                        new List<ExpressionNode>() { target, argVisited, argVisited });
                }
            }

            return target;
        }

        public RedILNode VisitInterpolatedStringExpression(
            InterpolatedStringExpression interpolatedStringExpression)
        {
            //TODO: set parent node
            var strings = new List<ExpressionNode>();
            foreach (var str in interpolatedStringExpression.Children)
            {
                var child = CastUtilities.CastRedILNode<ExpressionNode>(str.AcceptVisitor(this));
                strings.Add(child);
            }

            return new UniformOperatorNode(DataValueType.String, BinaryExpressionOperator.StringConcat, strings);
        }

        public RedILNode VisitInterpolatedStringText(InterpolatedStringText interpolatedStringText)
        {
            return new ConstantValueNode(DataValueType.String, interpolatedStringText.Text);
        }

        public RedILNode VisitInterpolation(Interpolation interpolation)
        {
            var expr = interpolation.Expression.AcceptVisitor(this);
            return expr;
        }

        public RedILNode VisitInvocationExpression(InvocationExpression invocationExpression)
        {
            var memberReference = invocationExpression.Target as MemberReferenceExpression;
            var invocRes = _compiler.GetInvocationResolveResult(invocationExpression);
            var arguments = invocationExpression.Arguments
                .Select(arg => CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this))).ToArray();
            ExpressionNode caller = null;
            if (memberReference is null)
            {
                if (invocationExpression.Target is IdentifierExpression identifierExpression)
                {
                    try
                    {
                        bool isStaticIdentifier = invocationExpression.Annotation<CSharpInvocationResolveResult>().Member.IsStatic;
                        var selfResolver = _resolver.ResolveMethod(isStaticIdentifier, currentType,
                            identifierExpression.Identifier, invocRes.Parameters.ToArray());
                        if (selfResolver is CallCustomMethodResolver customMethodResolver)
                        {
                            if (customMethodResolver.SourceLuaClass != null)
                            {
                                caller = new IdentifierNode(isStaticIdentifier ? customMethodResolver.SourceLuaClass : "self", DataValueType.Class);
                            }
                        }
                            
                        return selfResolver.Resolve(GetContext(invocationExpression), caller, arguments);
                    } catch { }

                    var customMethodArgs = invocationExpression.Arguments.Select(arg =>
                        CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this))).ToArray();
                    return new CallCustomMethodNode(identifierExpression.Identifier, null, null, false, customMethodArgs);
                }

                throw new RedILException($"Invocation is only possible by a member reference");
            }

            var isStatic = memberReference.Target is TypeReferenceExpression;

            var resolver = _resolver.ResolveMethod(isStatic, invocRes.DeclaringType,
                memberReference.MemberName, invocRes.Parameters.ToArray());

            caller = isStatic
                ? null
                : CastUtilities.CastRedILNode<ExpressionNode>(memberReference.Target.AcceptVisitor(this));
            if (arguments.Length < invocRes.Parameters.Count)
            {
                var optional = EvaluateOptionalArguments(invocationExpression,
                    invocRes.Parameters.Skip(arguments.Length).SkipWhile(x => !x.IsOptional).ToArray());
                arguments = arguments.Concat(optional).ToArray();
            }
            
            var invocationResolveResult = invocationExpression.Annotation<CSharpInvocationResolveResult>();
            if (resolver is CallCustomMethodResolver callCustomMethodResolver && invocationResolveResult != null)
            {
                if ((callCustomMethodResolver.Flags & CallMethodFlags.ImplicitReturnTypeArgument) != 0)
                {
                    IType type = invocationResolveResult.Type;
                    if (type is ArrayType arrayType)
                        type = arrayType.ElementType;
                    Type targetType = Assembly.Load(type.GetDefinition().ParentModule.FullAssemblyName).GetType(type.GetDefinition().ReflectionName);
                    
                    LuaImplicitTypeArgumentAttribute argumentAttribute = targetType.GetCustomAttribute<LuaImplicitTypeArgumentAttribute>();
                    if (argumentAttribute == null)
                        throw new Exception($"Return type must have LuaImplicitTypeArgumentAttribute to use with ImplicitReturnTypeArgument flag for expression '{invocationExpression}'");
                    arguments = arguments.Prepend(new ConstantValueNode(DataValueType.String, argumentAttribute.Argument)).ToArray();
                }
            }

            return resolver.Resolve(GetContext(invocationExpression), caller, arguments);
        }

        public RedILNode VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
        {
            var target = memberReferenceExpression.Target;
            var isStatic = target is TypeReferenceExpression;
            /*
            var resolveResult =
                memberReferenceExpression.Annotations.FirstOrDefault(annot => annot is MemberResolveResult) as
                    ResolveResult;

            if (resolveResult is null)
            {
                resolveResult = target.Annotations.FirstOrDefault(annot => annot is ResolveResult) as ResolveResult;
                if (resolveResult is null)
                {
                    throw new RedILException("Unable to find member resolve annotation");
                }
            }*/

            IType type;
            var resolveResult = target.Annotations.FirstOrDefault(annot => annot is ResolveResult) as ResolveResult;
            if (resolveResult is null)
            {
                var memberResolveResult =
                    memberReferenceExpression.Annotations.FirstOrDefault(annot => annot is MemberResolveResult) as
                        MemberResolveResult;
                type = memberResolveResult.Member.DeclaringType;
            }
            else
            {
                type = resolveResult.Type;
            }

            if (type.IsAnonymousType())
            {
                var dataType = _compiler.ResolveExpressionType(memberReferenceExpression);
                return new TableKeyAccessNode(CastUtilities.CastRedILNode<ExpressionNode>(target.AcceptVisitor(this)),
                    (ConstantValueNode)memberReferenceExpression.MemberName, dataType);
            }

            var resolver = _resolver.ResolveMember(isStatic, type,
                memberReferenceExpression.MemberName);

            var caller = isStatic ? null : CastUtilities.CastRedILNode<ExpressionNode>(target.AcceptVisitor(this));
            if (isStatic)
            {
                //TODO This is a band aid fix for referring to static class members with the class name, think it can be handled better higher up the chain
                ExpressionNode expressionNode = resolver.Resolve(GetContext(memberReferenceExpression), caller);
                if (expressionNode is TableKeyAccessNode tableKeyAccessNode &&
                    tableKeyAccessNode.Key is ConstantValueNode identifierValueNode &&
                    identifierValueNode.DataType == DataValueType.String)
                    return new IdentifierNode(identifierValueNode.Value as string, tableKeyAccessNode.DataType);
            }
            return resolver.Resolve(GetContext(memberReferenceExpression), caller);
        }

        public RedILNode VisitNullNode(AstNode nullNode)
        {
            return new NilNode();
        }

        public RedILNode VisitNullReferenceExpression(NullReferenceExpression nullReferenceExpression)
        {
            return new NilNode();
        }

        public RedILNode VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression)
        {
            return parenthesizedExpression.Expression.AcceptVisitor(this);
        }

        public RedILNode VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
        {
            var type = TypeUtilities.GetValueType(primitiveExpression.Value);
            return new ConstantValueNode(type, primitiveExpression.Value);
        }

        public RedILNode VisitReturnStatement(ReturnStatement returnStatement)
        {
            return new ReturnNode(
                CastUtilities.CastRedILNode<ExpressionNode>(returnStatement.Expression.AcceptVisitor(this)));
        }

        public RedILNode VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        {
            var operand =
                CastUtilities.CastRedILNode<ExpressionNode>(unaryOperatorExpression.Expression.AcceptVisitor(this));
            if (OperatorUtilities.IsIncrement(unaryOperatorExpression.Operator))
            {
                if (unaryOperatorExpression.Parent.NodeType != NodeType.Statement)
                {
                    throw new RedILException($"Incremental operators can only be used within statements");
                }

                BinaryExpressionOperator binaryOp = default;
                switch (unaryOperatorExpression.Operator)
                {
                    case UnaryOperatorType.Increment:
                    case UnaryOperatorType.PostIncrement:
                        binaryOp = BinaryExpressionOperator.Add;
                        break;
                    case UnaryOperatorType.Decrement:
                    case UnaryOperatorType.PostDecrement:
                        binaryOp = BinaryExpressionOperator.Subtract;
                        break;
                }

                var constantOne = new ConstantValueNode(DataValueType.Integer, 1);
                return new AssignNode(operand, CreateBinaryExpression(binaryOp, operand, constantOne));
            }

            var op = OperatorUtilities.UnaryOperator(unaryOperatorExpression.Operator);

            return new UnaryExpressionNode(op, operand);
        }

        public RedILNode VisitVariableDeclarationStatement(
            VariableDeclarationStatement variableDeclarationStatement)
        {
            var block = new BlockNode()
            {
                Explicit = false
            };

            _blockStack.Push(block);
            foreach (var variable in variableDeclarationStatement.Variables)
            {
                var decl = CastUtilities.CastRedILNode<VariableDeclareNode>(
                    variable.AcceptVisitor(this));
                block.Children.Add(decl);
            }

            _blockStack.Pop();

            return block;
        }

        public RedILNode VisitLocalFunctionDeclarationStatement(LocalFunctionDeclarationStatement localFunctionDeclarationStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitVariableInitializer(VariableInitializer variableInitializer)
        {
            _identifiers.Add(variableInitializer.Name);
            return new VariableDeclareNode(variableInitializer.Name,
                !(variableInitializer.Initializer is null)
                    ? CastUtilities.CastRedILNode<ExpressionNode>(
                        variableInitializer.Initializer.AcceptVisitor(this))
                    : null);
        }

        public RedILNode VisitWhileStatement(WhileStatement whileStatement)
        {
            var whileNode = new WhileNode();

            whileNode.Condition =
                CastUtilities.CastRedILNode<ExpressionNode>(
                    whileStatement.Condition.AcceptVisitor(this));
            whileNode.Body = RemoveFirstLevelContinue(CastUtilities.CastRedILNode<BlockNode>(
                whileStatement.EmbeddedStatement.AcceptVisitor(this)));

            return whileNode;
        }

        public RedILNode VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
        {
            TypeResolveResult typeResolveResult = objectCreateExpression.Type.Annotations.First() as TypeResolveResult;
            var invocRes = _compiler.GetInvocationResolveResult(objectCreateExpression);
            var resolver = _resolver.ResolveConstructor(invocRes.DeclaringType, invocRes.Parameters.ToArray());

            var arguments = objectCreateExpression.Arguments.Select(arg =>
                CastUtilities.CastRedILNode<ExpressionNode>(arg.AcceptVisitor(this))).ToArray();
            
            if (arguments.Length < invocRes.Parameters.Count)
            {
                var optional = EvaluateOptionalArguments(objectCreateExpression,
                    invocRes.Parameters.Skip(arguments.Length).ToArray());
                arguments = arguments.Concat(optional).ToArray();
            }

            var initializerElements = objectCreateExpression.Initializer.Elements
                .Select(elem => CastUtilities.CastRedILNode<ExpressionNode>(elem.AcceptVisitor(this))).ToList();
            
            IType objectType = (objectCreateExpression.Annotations.FirstOrDefault() as CSharpInvocationResolveResult).Type;
            
            //Update initializer elements with their resolver keys
            List<string> definedValues = new();
            foreach (ExpressionNode initializerElement in initializerElements)
            {
                if (initializerElement is not ArrayTableDefinitionNode arrayTableDefinition)
                    continue;
                if(arrayTableDefinition.Elements.First() is not ConstantValueNode keyValue)
                    continue;
                try
                {
                    var memberResolver = _resolver.ResolveMember(false, objectType,keyValue.Value.ToString());
                    if(memberResolver is not TableAccessMemberResolver tableAccessMemberResolver)
                        continue;
                    keyValue.Value = tableAccessMemberResolver.Key;
                    keyValue.DataType = tableAccessMemberResolver.DataType;
                } catch { }
                definedValues.Add(keyValue.Value as string);
            }
            
            var type = typeResolveResult.Type;
            var asm = Assembly.Load(type.GetDefinition().ParentModule.FullAssemblyName);
            var loadedType = asm.GetType(type.GetDefinition().ReflectionName);
            if (loadedType.GetCustomAttribute<LuaTableAttribute>() != null)
            {
                var typeRootNode = (CompiledTypeCache.GetRootNode(loadedType) as BlockNode).Children.First() as BlockNode;
                foreach (VariableDeclareNode variableDeclareNode in typeRootNode.Children.Where(x => x is VariableDeclareNode))
                {
                    if(definedValues.Contains(variableDeclareNode.Name.ToString())) continue;
                    if (variableDeclareNode.Value is NilNode) continue;
                    initializerElements.Add(new ArrayTableDefinitionNode(new List<ExpressionNode>()
                    {
                        variableDeclareNode.Name,
                        variableDeclareNode.Value
                    }));
                }
            }

            return resolver.Resolve(GetContext(objectCreateExpression), arguments, initializerElements.ToArray());
        }

        public RedILNode VisitIsExpression(IsExpression isExpression)
        {
            var exprVisited = CastUtilities.CastRedILNode<ExpressionNode>(isExpression.Expression.AcceptVisitor(this));
            if (isExpression.Type.IsNull)
            {
                return CreateBinaryExpression(BinaryExpressionOperator.Equal, exprVisited, ExpressionNode.Nil);
            }

            DataValueType type;
            if (isExpression.Type is PrimitiveType)
            {
                type = TypeUtilities.GetValueType(((PrimitiveType)isExpression.Type).KnownTypeCode);
            }
            else
            {
                type = DataValueType.Unknown;
            }

            if (type == DataValueType.Unknown)
            {
                ResolveResult resolveResult = isExpression.Type.Annotation<ResolveResult>();
                if (resolveResult != null)
                {
                    Type targetType = Assembly.Load(resolveResult.Type.GetDefinition().ParentModule.FullAssemblyName).GetType(resolveResult.Type.ReflectionName);
                    
                    LuaTableTypeCheckAttribute typeCheckAttribute = targetType.GetCustomAttribute<LuaTableTypeCheckAttribute>();
                    if (typeCheckAttribute != null)
                    {
                        LuaImplicitTypeArgumentAttribute implicitTypeArgument = targetType.GetCustomAttribute<LuaImplicitTypeArgumentAttribute>();
                        ConstantValueNode typeNode = new ConstantValueNode(DataValueType.String, implicitTypeArgument.Argument);
                        var isExpressionNode = isExpression.Expression.AcceptVisitor(this) as ExpressionNode;
                        if (typeCheckAttribute.Method != null)
                        {
                            return new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.And,
                                new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.NotEqual, isExpressionNode, new NilNode()),
                                new CallCustomMethodNode(
                                    typeCheckAttribute.Method, null, null, false,
                                    new List<ExpressionNode>
                                    {
                                        isExpressionNode,
                                        typeNode
                                    }));
                        }
                        else
                        {
                            return new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.And,
                                new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.NotEqual, isExpressionNode, new NilNode()),
                                new BinaryExpressionNode(DataValueType.Boolean, BinaryExpressionOperator.Equal,
                                    new TableKeyAccessNode(isExpression.Expression.AcceptVisitor(this) as ExpressionNode,
                                        new ConstantValueNode(
                                            typeCheckAttribute.TableAccessor is string
                                                ? DataValueType.String
                                                : DataValueType.Integer, typeCheckAttribute.TableAccessor),
                                        DataValueType.Array),
                                    typeNode));
                        }
                    }
                    return new CallLuaFunctionNode(LuaFunction.TableArrayContains, DataValueType.Boolean,
                        new List<ExpressionNode>
                        {
                            new TableKeyAccessNode(isExpression.Expression.AcceptVisitor(this) as ExpressionNode,
                                new ConstantValueNode(DataValueType.String, "_CSharpTypes"), DataValueType.Array),
                            targetType.GetTypeValueNode(_resolver.Flags)
                        });
                }
                return ExpressionNode.False;
            }

            if (exprVisited.DataType == DataValueType.Unknown)
            {
                return CreateBinaryExpression(BinaryExpressionOperator.Equal,
                    new CallBuiltinLuaMethodNode(LuaBuiltinMethod.Type, new[] { exprVisited }),
                    (ConstantValueNode)LuaTypeNameFromDataValueType(type));
            }

            return exprVisited.DataType == type ? ExpressionNode.True : ExpressionNode.False;
        }

        public RedILNode VisitSwitchStatement(SwitchStatement switchStatement)
        {
            BlockNode GetFromSection(SwitchSection section)
            {
                BlockNode block;
                if (section.Statements.FirstOrDefault() is not BlockStatement)
                {
                    block = new();
                    foreach (Statement statement in section.Statements)
                    {
                        block.Children.Add(statement.AcceptVisitor(this));
                    }
                }
                else
                {
                    if (section.Statements.Count != 1)
                    {
                        throw new RedILException("Expected statements inside switch section to be of length 1");
                    }
                    block = CastUtilities.CastRedILNode<BlockNode>(section.Statements.First().AcceptVisitor(this));
                }

                var last = block.Children.LastOrDefault();
                if (!(last is null) && last.Type == RedILNodeType.Break)
                {
                    block.Children.Remove(last);
                }

                return block;
            }

            var pivot = CastUtilities.CastRedILNode<ExpressionNode>(switchStatement.Expression.AcceptVisitor(this));
            var ifNode = new IfNode();
            var defaultCase =
                switchStatement.SwitchSections.FirstOrDefault(s =>
                    s.CaseLabels.Any(cl => cl.Expression is null || cl.Expression.IsNull));
            if (!(defaultCase is null))
            {
                switchStatement.SwitchSections.Remove(defaultCase);
            }

            foreach (var section in switchStatement.SwitchSections)
            {
                if (section.CaseLabels.Count == 0) continue;
                var condition = CreateBinaryExpression(BinaryExpressionOperator.Equal, pivot,
                    CastUtilities.CastRedILNode<ExpressionNode>(section.CaseLabels.First().Expression
                        .AcceptVisitor(this)));
                foreach (var or in section.CaseLabels.Skip(1))
                {
                    condition = CreateBinaryExpression(BinaryExpressionOperator.Or, condition,
                        CreateBinaryExpression(BinaryExpressionOperator.Equal, pivot,
                            CastUtilities.CastRedILNode<ExpressionNode>(or.Expression.AcceptVisitor(this))));
                }

                if (condition.EqualOrNull(ExpressionNode.True))
                {
                    return GetFromSection(section);
                }
                else if (!condition.EqualOrNull(ExpressionNode.False))
                {
                    var block = GetFromSection(section);
                    ifNode.Ifs.Add(new KeyValuePair<ExpressionNode, RedILNode>(condition, block));
                }
            }

            ifNode.IfElse = defaultCase is null ? null : GetFromSection(defaultCase);
            if (ifNode.Ifs.Count == 0 && ifNode.IfElse is null)
            {
                return new EmptyNode();
            }
            else if (ifNode.Ifs.Count == 0)
            {
                return ifNode.IfElse;
            }

            return ifNode;
        }

        public RedILNode VisitAnonymousTypeCreateExpression(
            AnonymousTypeCreateExpression anonymousTypeCreateExpression)
        {
            var kvs = new List<KeyValuePair<ExpressionNode, ExpressionNode>>();
            foreach (var init in anonymousTypeCreateExpression.Initializers)
            {
                var named = init as NamedExpression;
                if (named is null)
                {
                    throw new RedILException(
                        $"Anonymous object declaration can only be done via named expressions");
                }

                var expr = CastUtilities.CastRedILNode<ExpressionNode>(named.Expression.AcceptVisitor(this));
                kvs.Add(new KeyValuePair<ExpressionNode, ExpressionNode>((ConstantValueNode)named.Name, expr));
            }

            return new DictionaryTableDefinitionNode(kvs);
        }

        #endregion
        public RedILNode VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression)
        {
            return new FunctionNode(anonymousMethodExpression.Parameters.Select(VisitParameterDeclaration).ToArray(),
                VisitBlockStatement(anonymousMethodExpression.Body));
        }

        public RedILNode VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
        {
            ExpressionNode classInitializerNode = _compiler.ClassInitializerTableNode;
            if (constructorDeclaration.Initializer != null && currentBaseType != null)
            {
                var baseClassResolver = _resolver.ResolveConstructor(currentBaseType, constructorDeclaration.Initializer.Arguments.Select(p => p.Annotation<ResolveResult>().Type).ToArray());
                classInitializerNode = baseClassResolver.Resolve(null, constructorDeclaration.Initializer.Arguments.Select(x => x.AcceptVisitor(this)).Append(_compiler.ClassInitializerTableNode).Cast<ExpressionNode>().ToArray(), null);
            }
            var resolver = _resolver.ResolveConstructor(currentType, constructorDeclaration.Parameters.Select(p => p.Annotation<ResolveResult>().Type).ToArray());
            if (resolver is LuaClassConstructorResolver classConstructor)
            {
                var body = VisitBlockStatement(constructorDeclaration.Body);
                var bodyBlock = body as BlockNode;
                if (bodyBlock == null && body is RootNode rootNode)
                    bodyBlock = rootNode.Body as BlockNode;
                var newBody = bodyBlock.Children.Prepend(new VariableDeclareNode("self",
                    new CallCustomMethodNode("setmetatable", null, null, false, new List<ExpressionNode>()
                    {
                        classInitializerNode,
                        new IdentifierNode(_compiler.LuaClass, DataValueType.String)
                    })));
                newBody = newBody.Append(_compiler.PostConstructorObjectInitializerNode);
                newBody = newBody.Append(new ReturnNode(new IdentifierNode("self", DataValueType.String)));
                bodyBlock.Children = newBody.ToList();
                return new FunctionNode($"{classConstructor.SourceLuaClass}.{classConstructor.Name}",
                    constructorDeclaration.Parameters.Select(VisitParameterDeclaration).Append(new IdentifierNode("_initializer", DataValueType.String)).ToArray(), body);
            }
            throw new NotImplementedException();
        }

        public RedILNode CreateDefaultConstructor()
        {
            ExpressionNode classInitializerNode = _compiler.ClassInitializerTableNode;
            if (currentBaseType != null)
            {
                var baseClassResolver = _resolver.ResolveConstructor(currentBaseType, Array.Empty<IParameter>());
                classInitializerNode = baseClassResolver.Resolve(null, new []{_compiler.ClassInitializerTableNode}, null);
            }
            BlockNode bodyBlock = new();
            var newBody = bodyBlock.Children.Prepend(new VariableDeclareNode("self",
                new CallCustomMethodNode("setmetatable", null, null, false, new List<ExpressionNode>()
                {
                    classInitializerNode,
                    new IdentifierNode(_compiler.LuaClass, DataValueType.String)
                })));
            newBody = newBody.Append(_compiler.PostConstructorObjectInitializerNode);
            newBody = newBody.Append(new ReturnNode(new IdentifierNode("self", DataValueType.String)));
            bodyBlock.Children = newBody.ToList();
            return new FunctionNode($"{_compiler.LuaClass}.new", new List<RedILNode>() { new IdentifierNode("_initializer", DataValueType.String) }, bodyBlock);
        }

        public RedILNode VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
        {
            if (fieldDeclaration.Variables.Count > 1)
            {
                throw new Exception("Unhandled more than 1 variable initializer in field declaration"); //Don't think this can even happen
            }

            foreach (VariableInitializer variableInitializer in fieldDeclaration.Variables)
            {
                string declarationName = variableInitializer.Name;
                var memberResolver = _resolver.ResolveMember(fieldDeclaration.HasModifier(Modifiers.Static), currentType, declarationName);
                if (memberResolver is TableAccessMemberResolver tableAccessMemberResolver)
                {
                    if (tableAccessMemberResolver.Key is string customName)
                        declarationName = customName;
                }
                
                if (variableInitializer.Initializer == Expression.Null)
                {
                    if (fieldDeclaration.ReturnType is SimpleType simpleType &&
                        simpleType.Annotations.First() is TypeResolveResult typeResolveResult)
                    {
                        if (_resolver.ResolveValue(typeResolveResult.Type) is EnumValueResolver enumResolver)
                        {
                            return enumResolver.DefaultValue switch
                            {
                                string => new VariableDeclareNode(declarationName,
                                    new ConstantValueNode(DataValueType.String, enumResolver.DefaultValue)),
                                bool => new VariableDeclareNode(declarationName,
                                    new ConstantValueNode(DataValueType.Boolean, enumResolver.DefaultValue)),
                                _ => new VariableDeclareNode(declarationName,
                                    new ConstantValueNode(DataValueType.Integer, enumResolver.DefaultValue))
                            };
                        }
                    }

                    if (fieldDeclaration.ReturnType is PrimitiveType primitiveType)
                    {
                        switch (primitiveType.KnownTypeCode)
                        {
                            case KnownTypeCode.Boolean:
                                return new VariableDeclareNode(declarationName,
                                    new ConstantValueNode(DataValueType.Boolean, default(bool)));
                            case KnownTypeCode.Char:
                                return new VariableDeclareNode(declarationName,
                                    new ConstantValueNode(DataValueType.Unknown, default(char)));
                            case KnownTypeCode.SByte:
                            case KnownTypeCode.Byte:
                            case KnownTypeCode.Int16:
                            case KnownTypeCode.UInt16:
                            case KnownTypeCode.Int32:
                            case KnownTypeCode.UInt32:
                            case KnownTypeCode.Int64:
                            case KnownTypeCode.UInt64:
                                return new VariableDeclareNode(declarationName,
                                    new ConstantValueNode(DataValueType.Integer, 0));
                            case KnownTypeCode.Single:
                            case KnownTypeCode.Double:
                            case KnownTypeCode.Decimal:
                                return new VariableDeclareNode(declarationName,
                                    new ConstantValueNode(DataValueType.Float, 0));
                        }
                    }

                    return new EmptyNode();
                }

                return new VariableDeclareNode(declarationName,
                    !(variableInitializer.Initializer is null)
                        ? CastUtilities.CastRedILNode<ExpressionNode>(
                            variableInitializer.Initializer.AcceptVisitor(this))
                        : null);
            }

            throw new Exception("No Variable initializer for FieldDeclaration");
        }

        public RedILNode VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        {
            string methodName = methodDeclaration.Name;
            if (methodName == "ToString" && methodDeclaration.HasModifier(Modifiers.Override))
            {
                return new AssignNode(new IdentifierNode($"{_compiler.LuaClass}.__tostring", DataValueType.Unknown),
                    new FunctionNode(new List<RedILNode>{ new IdentifierNode("self", DataValueType.Dictionary)},
                        VisitBlockStatement(methodDeclaration.Body)));
            }
            try
            {
                bool isStatic = methodDeclaration.HasModifier(Modifiers.Static);
                var resolver = _resolver.ResolveMethod(isStatic, currentType, methodDeclaration.Name, methodDeclaration.Parameters.Select(p => p.Annotation<ILVariableResolveResult>().Type).ToArray());
                if (resolver is CallCustomMethodResolver methodResolver)
                {
                    methodName = methodResolver.Method;
                    if (methodResolver.SourceLuaClass != null)
                    {
                        methodName = $"{methodResolver.SourceLuaClass}{(isStatic ? "." : ":")}{methodName}";
                    }
                }
            } catch { }
            
            return new FunctionNode(methodName,
                methodDeclaration.Parameters.Select(VisitParameterDeclaration).ToArray(),
                VisitBlockStatement(methodDeclaration.Body));
        }

        public RedILNode VisitParameterDeclaration(ParameterDeclaration parameterDeclaration)
        {
            return new IdentifierNode(parameterDeclaration.Name, DataValueType.Unknown);
        }

        public RedILNode VisitSyntaxTree(SyntaxTree syntaxTree)
        {
            List<RedILNode> children = new();
            foreach (var child in syntaxTree.Children)
            {
                if (child is NamespaceDeclaration namespaceDeclaration)
                {
                    foreach (var nameSpaceChild in namespaceDeclaration.Children)
                    {
                        if (nameSpaceChild is TypeDeclaration typeDeclaration)
                        {
                            children.Add(VisitTypeDeclaration(typeDeclaration));
                        }
                    }
                }

                if (child is TypeDeclaration noNameSpaceTypeDeclaration)
                {
                    children.Add(VisitTypeDeclaration(noNameSpaceTypeDeclaration));
                }
            }

            return new BlockNode(children);
        }

        public RedILNode VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
        {
            string declarationName = propertyDeclaration.Name;
            var memberResolver = _resolver.ResolveMember(propertyDeclaration.HasModifier(Modifiers.Static), currentType, declarationName);
            if (memberResolver is TableAccessMemberResolver tableAccessMemberResolver)
            {
                if (tableAccessMemberResolver.Key is string customName)
                    declarationName = customName;
            }
            
            if (propertyDeclaration.Initializer == Expression.Null)
            {
                if (propertyDeclaration.ReturnType is SimpleType simpleType &&
                    simpleType.Annotations.First() is TypeResolveResult typeResolveResult)
                {
                    if (_resolver.ResolveValue(typeResolveResult.Type) is EnumValueResolver enumResolver)
                    {
                        return enumResolver.DefaultValue switch
                        {
                            string => new VariableDeclareNode(declarationName,
                                new ConstantValueNode(DataValueType.String, enumResolver.DefaultValue)),
                            bool => new VariableDeclareNode(declarationName,
                                new ConstantValueNode(DataValueType.Boolean, enumResolver.DefaultValue)),
                            _ => new VariableDeclareNode(declarationName,
                                new ConstantValueNode(DataValueType.Integer, enumResolver.DefaultValue))
                        };
                    }
                }

                if (propertyDeclaration.ReturnType is PrimitiveType primitiveType)
                {
                    switch (primitiveType.KnownTypeCode)
                    {
                        case KnownTypeCode.Boolean:
                            return new VariableDeclareNode(declarationName,
                                new ConstantValueNode(DataValueType.Boolean, default(bool)));
                        case KnownTypeCode.Char:
                            return new VariableDeclareNode(declarationName,
                                new ConstantValueNode(DataValueType.Unknown, default(char)));
                        case KnownTypeCode.SByte:
                        case KnownTypeCode.Byte:
                        case KnownTypeCode.Int16:
                        case KnownTypeCode.UInt16:
                        case KnownTypeCode.Int32:
                        case KnownTypeCode.UInt32:
                        case KnownTypeCode.Int64:
                        case KnownTypeCode.UInt64:
                            return new VariableDeclareNode(declarationName,
                                new ConstantValueNode(DataValueType.Integer, 0));
                        case KnownTypeCode.Single:
                        case KnownTypeCode.Double:
                        case KnownTypeCode.Decimal:
                            return new VariableDeclareNode(declarationName,
                                new ConstantValueNode(DataValueType.Float, 0));
                    }
                }
                return new EmptyNode();
            }

            return new VariableDeclareNode(declarationName,
                !(propertyDeclaration.Initializer is null)
                    ? CastUtilities.CastRedILNode<ExpressionNode>(
                        propertyDeclaration.Initializer.AcceptVisitor(this))
                    : null);
        }

        public RedILNode VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        {
            if (typeDeclaration.Annotations.FirstOrDefault() is TypeResolveResult typeResolveResult)
                currentType = typeResolveResult.Type;
            if (typeDeclaration.BaseTypes.Count > 0 && typeDeclaration.BaseTypes.First().Annotations.FirstOrDefault() is TypeResolveResult baseClassType)
            {
                if(baseClassType.Type.Kind != TypeKind.Interface) //We don't care if only inherits from interfaces, interfaces aren't relevant for us in lua land 
                    currentBaseType = baseClassType.Type;
            }
            BlockNode currentBlock = new BlockNode();
            _blockStack.Push(currentBlock);
            if (_compiler.LuaClass != null)
            {
                if (currentBaseType == null)
                    currentBlock.Children.Add(new AssignNode(new IdentifierNode(_compiler.LuaClass, DataValueType.Dictionary), new DictionaryTableDefinitionNode()));
                else
                {
                    
                    currentBlock.Children.Add(new AssignNode(new IdentifierNode(_compiler.LuaClass, DataValueType.Dictionary), new CallCustomMethodNode("setmetatable", null, null, false, new List<ExpressionNode>()
                    {
                        new DictionaryTableDefinitionNode(),
                        new IdentifierNode(_resolver.ResolveLuaClassName(currentBaseType), DataValueType.Dictionary)
                    })));
                }
                currentBlock.Children.Add(new AssignNode(new IdentifierNode($"{_compiler.LuaClass}.__index", DataValueType.String), new IdentifierNode(_compiler.LuaClass, DataValueType.String)));
                if ((_compiler.MainResolver.Flags & LuaCompileFlags.StripCSharpTypes) == 0)
                {
                    Type type = Assembly.Load(currentType.GetDefinition().ParentModule.FullAssemblyName).GetType(currentType.GetDefinition().ReflectionName);
                    currentBlock.Children.Add(new AssignNode(new IdentifierNode($"{_compiler.LuaClass}._CSharpTypes", DataValueType.String), new ArrayTableDefinitionNode(type.GetAllBaseTypesAndInterfaces().Select(x => x.GetTypeValueNode(_resolver.Flags)).Cast<ExpressionNode>().ToList())));
                }
            }
            bool needsDefaultConstructor = _compiler.LuaClass != null;
            foreach (var childNode in typeDeclaration.Children)
            {
                if (_compiler.LuaClass == null)
                {
                    //For [LuaProgram] we can handle properties and fields normally
                    if (childNode is PropertyDeclaration propertyDeclaration)
                    {
                        currentBlock.Children.Add(VisitPropertyDeclaration(propertyDeclaration));
                    }

                    if (childNode is FieldDeclaration fieldDeclaration)
                    {
                        currentBlock.Children.Add(VisitFieldDeclaration(fieldDeclaration));
                    }
                }
                else
                {
                    //For [LuaClass] we need to tweak properties and fields so they modify the class table if they are static and get added to the initializer node if they are not
                    if (childNode is PropertyDeclaration propertyDeclaration)
                    {
                        if(propertyDeclaration.HasModifier(Modifiers.Static))
                            currentBlock.Children.Add(ConvertToClassTableAssignmentNode(VisitPropertyDeclaration(propertyDeclaration)));
                        else
                            AddToClassInitializerTableNode(VisitPropertyDeclaration(propertyDeclaration));
                            
                    }

                    if (childNode is FieldDeclaration fieldDeclaration)
                    {
                        if(fieldDeclaration.HasModifier(Modifiers.Static)) 
                            currentBlock.Children.Add(ConvertToClassTableAssignmentNode(VisitFieldDeclaration(fieldDeclaration)));
                        else
                            AddToClassInitializerTableNode(VisitFieldDeclaration(fieldDeclaration));
                    }
                    
                    //only need to do constructors for [LuaClass] not for [LuaProgram]
                    if (childNode is ConstructorDeclaration constructorDeclaration)
                    {
                        currentBlock.Children.Add(VisitConstructorDeclaration(constructorDeclaration));
                        needsDefaultConstructor = false;
                    }
                    
                    //only need to do deconstructors for [LuaClass] not for [LuaProgram]
                    if (childNode is DestructorDeclaration destructorDeclaration)
                    {
                        currentBlock.Children.Add(VisitDestructorDeclaration(destructorDeclaration));
                    }
                    
                    //only need to do OperatorDeclaration for [LuaClass] not for [LuaProgram]
                    if (childNode is OperatorDeclaration op)
                    {
                        currentBlock.Children.Add(VisitOperatorDeclaration(op));
                    }
                }

                if (childNode is MethodDeclaration methodDeclaration)
                {
                    currentBlock.Children.Add(VisitMethodDeclaration(methodDeclaration));
                }
            }
            
            if(needsDefaultConstructor)
                currentBlock.Children.Add(CreateDefaultConstructor());

            return currentBlock;
        }
        
        private RedILNode ConvertToClassTableAssignmentNode(RedILNode baseNode)
        {
            if (baseNode is not VariableDeclareNode variableDeclareNode)
                return baseNode;
            return new AssignNode(
                new TableKeyAccessNode(new IdentifierNode(_compiler.LuaClass, DataValueType.String), variableDeclareNode.Name, DataValueType.Unknown), variableDeclareNode.Value);
        }
        
        private void AddToClassInitializerTableNode(RedILNode baseNode)
        {
            if (baseNode is not VariableDeclareNode variableDeclareNode)
                return;
            _compiler.ClassInitializerTableNode.Elements.Add(new KeyValuePair<ExpressionNode, ExpressionNode>(variableDeclareNode.Name,variableDeclareNode.Value));
        }

        public RedILNode VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression)
        {
            if (_compiler.LuaClass != null)
                return new IdentifierNode("self", DataValueType.Dictionary);
            else
                return null;
        }

        public RedILNode VisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression)
        {
            if (namedArgumentExpression.Expression is PrimitiveExpression primitiveExpression)
            {
                if (primitiveExpression.Value is bool boolValue)
                    return new ConstantValueNode(DataValueType.Boolean, boolValue);
                if (primitiveExpression.Value is string stringValue)
                    return new ConstantValueNode(DataValueType.String, stringValue);
                if (primitiveExpression.Value is int intValue)
                    return new ConstantValueNode(DataValueType.Integer, intValue);
                if (primitiveExpression.Value is float floatValue)
                    return new ConstantValueNode(DataValueType.Float, floatValue);
            }

            throw new NotImplementedException();
        }

        public RedILNode VisitNamedExpression(NamedExpression namedExpression)
        {
            return new ArrayTableDefinitionNode(new List<ExpressionNode>
                {
                    new ConstantValueNode(DataValueType.String, namedExpression.Name),
                    namedExpression.Expression.AcceptVisitor(this) as ExpressionNode
                }
            );
        }

        public RedILNode VisitLambdaExpression(LambdaExpression lambdaExpression)
        {
            return new FunctionNode(lambdaExpression.Parameters.Select(VisitParameterDeclaration).ToArray(), new ReturnNode(lambdaExpression.Body.AcceptVisitor(this) as ExpressionNode));
        }

        public RedILNode VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression)
        {
            var elements =
                arrayInitializerExpression.Elements.Select(
                    elem => CastUtilities.CastRedILNode<ExpressionNode>(elem.AcceptVisitor(this)));

            return new ArrayTableDefinitionNode(elements.ToList());
        }

        public RedILNode VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression)
        {
            if (_compiler.LuaClass != null)
                return new IdentifierNode("self", DataValueType.Dictionary);
            else
                return null;
        }

        public RedILNode VisitTypeOfExpression(TypeOfExpression typeOfExpression)
        {
            ResolveResult resolveResult = typeOfExpression.Type.Annotation<ResolveResult>();
            if(resolveResult != null)
                return Assembly.Load(resolveResult.Type.GetDefinition().ParentModule.FullAssemblyName).GetType(resolveResult.Type.ReflectionName).GetTypeValueNode(_resolver.Flags);
            throw new NotImplementedException();
        }

        public RedILNode VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
        {
            //TODO More of these could be supported in the future by giving them custom methods that the transpiler can convert to call those methods when necessary
            switch (operatorDeclaration.OperatorType)
            {
                //We don't need to transpile these due to how lua overloads work
                case OperatorType.Inequality:
                    return new EmptyNode();
            }
            string identifier = $"{_compiler.LuaClass}.{operatorDeclaration.OperatorType switch
            {
                OperatorType.LogicalNot => "__bnot",
                OperatorType.OnesComplement =>  throw new Exception("Unsupported operator overload OnesComplement"),
                OperatorType.Increment =>  throw new Exception("Unsupported operator overload Increment"),
                OperatorType.Decrement =>  throw new Exception("Unsupported operator overload Decrement"),
                OperatorType.True => throw new Exception("Unsupported operator overload True"),
                OperatorType.False => throw new Exception("Unsupported operator overload False"),
                OperatorType.Addition => "__add",
                OperatorType.Subtraction => "__sub",
                OperatorType.UnaryPlus => throw new Exception("Unsupported operator overload UnaryPlus"),
                OperatorType.UnaryNegation => "__unm",
                OperatorType.Multiply => "__mul",
                OperatorType.Division => "__div",
                OperatorType.Modulus => "__mod",
                OperatorType.BitwiseAnd => "__band",
                OperatorType.BitwiseOr => "__bor",
                OperatorType.ExclusiveOr => "__bxor",
                OperatorType.LeftShift => throw new Exception("Unsupported operator overload LeftShift"),
                OperatorType.RightShift => throw new Exception("Unsupported operator overload RightShift"),
                OperatorType.Equality => "__eq",
                OperatorType.Inequality => throw new Exception("Unsupported operator overload Inequality"),
                OperatorType.GreaterThan => throw new Exception("Unsupported operator overload GreaterThan (use LessThan)"),
                OperatorType.LessThan => "__lt",
                OperatorType.GreaterThanOrEqual => throw new Exception("Unsupported operator overload GreaterThanOrEqual (use LessThanOrEqual)"),
                OperatorType.LessThanOrEqual => "__le",
                OperatorType.Implicit =>  throw new Exception("Unsupported operator overload Implicit"),
                OperatorType.Explicit =>  throw new Exception("Unsupported operator overload Explicit"),
                _ => throw new ArgumentOutOfRangeException()
            }}";
            return new AssignNode(new IdentifierNode(identifier, DataValueType.Unknown), new FunctionNode(
                operatorDeclaration.Parameters.Select(VisitParameterDeclaration).ToArray(),
                VisitBlockStatement(operatorDeclaration.Body)));
        }

        public RedILNode VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration)
        {
            return new AssignNode(new IdentifierNode($"{_compiler.LuaClass}.__gc", DataValueType.Unknown), new FunctionNode(
                new List<RedILNode>(),
                VisitBlockStatement(destructorDeclaration.Body)));
        }

        #region Private

        private Context GetContext(Expression currentExpr)
        {
            return new Context(_compiler, _root, currentExpr, _blockStack.Peek());
        }

        private BinaryExpressionNode CreateBinaryExpression(BinaryExpressionOperator op, ExpressionNode left,
            ExpressionNode right)
        {
            if (OperatorUtilities.IsBoolean(op))
            {
                return new BinaryExpressionNode(DataValueType.Boolean, op, left, right);
            }
            else if (OperatorUtilities.IsArithmatic(op))
            {
                if (left.DataType == DataValueType.String ||
                    right.DataType == DataValueType.String)
                {
                    //Call tostring on any non string operands
                    if (left.DataType != DataValueType.String)
                    {
                        left = new CallCustomMethodNode("tostring", null, null, false, new List<ExpressionNode> { left });
                    }
                    if (right.DataType != DataValueType.String)
                    {
                        right = new CallCustomMethodNode("tostring", null, null, false, new List<ExpressionNode> { right });
                    }
                    return new BinaryExpressionNode(DataValueType.String, BinaryExpressionOperator.StringConcat,
                        left, right);
                }
                else if (left.DataType == DataValueType.Float ||
                         right.DataType == DataValueType.Float)
                {
                    return new BinaryExpressionNode(DataValueType.Float, op, left, right);
                }
                else if (left.DataType == DataValueType.Integer &&
                         right.DataType == DataValueType.Integer)
                {
                    return new BinaryExpressionNode(DataValueType.Integer, op, left, right);
                }
            }
            else if (op == BinaryExpressionOperator.NullCoalescing)
            {
                return new BinaryExpressionNode(left.DataType, op, left, right);
            }

            return new BinaryExpressionNode(DataValueType.Unknown, op, left, right);
            //TODO Handle more elegantly
            //throw new RedILException($"Unsupported operator '{op}' with data types '{left.DataType}' and '{right.DataType}'");
        }

        //TODO: This covers the cases I've seen so far, might have to rewrite it to a more general version that would remove all instances of `continue`
        private BlockNode RemoveFirstLevelContinue(BlockNode node)
        {
            var newBlock = new BlockNode();
            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                if (child.Type == RedILNodeType.If)
                {
                    var ifNode = child as IfNode;
                    if (!(ifNode.Ifs is null) && ifNode.Ifs.Count == 1 && ifNode.IfElse is null)
                    {
                        var truthBlock = ifNode.Ifs.First().Value as BlockNode;
                        if (truthBlock.Children.Count == 1 &&
                            truthBlock.Children.First().Type == RedILNodeType.Continue)
                        {
                            var innerIfBlock = new BlockNode();
                            for (int j = i + 1; j < node.Children.Count; j++)
                            {
                                innerIfBlock.Children.Add(node.Children[j]);
                            }

                            /*newBlock.Children.Add(new IfNode(OptimizedNot(ifNode.Ifs.First().Key), innerIfBlock, null));*/
                            newBlock.Children.Add(new IfNode(
                                new KeyValuePair<ExpressionNode, RedILNode>[]
                                {
                                    new KeyValuePair<ExpressionNode, RedILNode>(
                                        OptimizedNot(ifNode.Ifs.First().Key), innerIfBlock)
                                }, null));
                            break;
                        }
                    }
                }

                newBlock.Children.Add(child);
            }

            return newBlock;
        }

        private ExpressionNode OptimizedNot(ExpressionNode expr)
        {
            if (expr.Type == RedILNodeType.UnaryExpression &&
                (expr as UnaryExpressionNode).Operator == UnaryExpressionOperator.Not)
            {
                return (expr as UnaryExpressionNode).Operand;
            }

            return new UnaryExpressionNode(UnaryExpressionOperator.Not, expr);
        }

        private IEnumerable<ExpressionNode> EvaluateOptionalArguments(Expression currentExpr, IList<IParameter> args)
        {
            foreach (var arg in args)
            {
                if (!arg.IsOptional || !arg.HasConstantValueInSignature)
                {
                    throw new RedILException(
                        $"Optional argument must be declared optional and have a constant value in its signature");
                }

                var constant = arg.GetConstantValue();
                if (constant is null)
                {
                    yield return new NilNode();
                }
                else if (arg.Type is PrimitiveType)
                {
                    var typeCode = ((PrimitiveType)arg).KnownTypeCode;
                    if (typeCode == KnownTypeCode.Double)
                    {
                        //TODO: Find a way to handle it in the double value resolver
                        double num = (double)constant;
                        if (double.IsPositiveInfinity(num))
                        {
                            yield return (ConstantValueNode)"+inf";
                            continue;
                        }
                        else if (double.IsNegativeInfinity(num))
                        {
                            yield return (ConstantValueNode)"-inf";
                            continue;
                        }
                    }

                    var primitiveType = TypeUtilities.GetValueType(typeCode);
                    yield return new ConstantValueNode(primitiveType, constant);
                }
                else if (arg.Type.Kind == TypeKind.Enum || arg.Type.Kind == TypeKind.Struct)
                {
                    var resolver = _resolver.ResolveValue(arg.Type);
                    yield return resolver is null
                        ? ExpressionNode.Nil
                        : resolver.Resolve(GetContext(currentExpr), constant);
                }
                else
                {
                    throw new RedILException($"Unsupported type for optional argument '{arg.Type}'");
                }
            }
        }

        private string LuaTypeNameFromDataValueType(DataValueType type)
        {
            switch (type)
            {
                case DataValueType.Array:
                    return "table";
                case DataValueType.Boolean:
                    return "boolean";
                case DataValueType.Dictionary:
                    return "table";
                case DataValueType.Float:
                    return "number";
                case DataValueType.Integer:
                    return "number";
                case DataValueType.String:
                    return "string";
                case DataValueType.KVPair:
                    return "table";
                default: return "nil";
            }
        }

        private RedILNode NullIfNil(RedILNode node)
        {
            return node.Type == RedILNodeType.Nil ||
                   (node.Type == RedILNodeType.Block &&
                    ((BlockNode)node).Children.SequenceEqual(new[] { ExpressionNode.Nil }))
                ? null
                : node;
        }

        private IEnumerable<RedILNode> FlattenImplicitBlocks(RedILNode node)
        {
            var stack = new Stack<RedILNode>();
            stack.Push(node);
            while (stack.Count > 0)
            {
                var top = stack.Pop();
                var topAsBlock = top as BlockNode;
                if (topAsBlock is null || topAsBlock.Explicit)
                {
                    yield return top;
                }
                else
                {
                    var children = topAsBlock.Children.Reverse();
                    foreach (var child in children)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        #endregion

        #region Unused

        public RedILNode VisitYieldBreakStatement(YieldBreakStatement yieldBreakStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitYieldReturnStatement(YieldReturnStatement yieldReturnStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitAccessor(Accessor accessor)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitConstructorInitializer(ConstructorInitializer constructorInitializer)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitEnumMemberDeclaration(EnumMemberDeclaration enumMemberDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitEventDeclaration(EventDeclaration eventDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitCustomEventDeclaration(CustomEventDeclaration customEventDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitFixedFieldDeclaration(FixedFieldDeclaration fixedFieldDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitFixedVariableInitializer(FixedVariableInitializer fixedVariableInitializer)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitSimpleType(SimpleType simpleType)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitMemberType(MemberType memberType)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitTupleType(TupleAstType tupleType)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitTupleTypeElement(TupleTypeElement tupleTypeElement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitFunctionPointerType(FunctionPointerType functionPointerType)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitComposedType(ComposedType composedType)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitArraySpecifier(ArraySpecifier arraySpecifier)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitUncheckedExpression(UncheckedExpression uncheckedExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitUndocumentedExpression(UndocumentedExpression undocumentedExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitQueryExpression(QueryExpression queryExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitQueryContinuationClause(QueryContinuationClause queryContinuationClause)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitQueryFromClause(QueryFromClause queryFromClause)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitQueryLetClause(QueryLetClause queryLetClause)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitQueryWhereClause(QueryWhereClause queryWhereClause)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitQueryJoinClause(QueryJoinClause queryJoinClause)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitQueryOrderClause(QueryOrderClause queryOrderClause)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitQueryOrdering(QueryOrdering queryOrdering)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitQuerySelectClause(QuerySelectClause querySelectClause)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitQueryGroupClause(QueryGroupClause queryGroupClause)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitAttribute(Attribute attribute)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitAttributeSection(AttributeSection attributeSection)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitDelegateDeclaration(DelegateDeclaration delegateDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitUsingAliasDeclaration(UsingAliasDeclaration usingAliasDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitUsingDeclaration(UsingDeclaration usingDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitSwitchSection(SwitchSection switchSection)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitCaseLabel(CaseLabel caseLabel)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitThrowStatement(ThrowStatement throwStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitTryCatchStatement(TryCatchStatement tryCatchStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitCatchClause(CatchClause catchClause)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitUncheckedStatement(UncheckedStatement uncheckedStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitUnsafeStatement(UnsafeStatement unsafeStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitUsingStatement(UsingStatement usingStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitSizeOfExpression(SizeOfExpression sizeOfExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitStackAllocExpression(StackAllocExpression stackAllocExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitThrowExpression(ThrowExpression throwExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitTupleExpression(TupleExpression tupleExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitDirectionExpression(DirectionExpression directionExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitPreProcessorDirective(PreProcessorDirective preProcessorDirective)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitOutVarDeclarationExpression(OutVarDeclarationExpression outVarDeclarationExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitErrorNode(AstNode errorNode)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitPatternPlaceholder(AstNode placeholder, Pattern pattern)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitGotoDefaultStatement(GotoDefaultStatement gotoDefaultStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitGotoStatement(GotoStatement gotoStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitTypeParameterDeclaration(TypeParameterDeclaration typeParameterDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitConstraint(Constraint constraint)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitCSharpTokenNode(CSharpTokenNode cSharpTokenNode)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitIdentifier(Identifier identifier)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitExternAliasDeclaration(ExternAliasDeclaration externAliasDeclaration)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitFixedStatement(FixedStatement fixedStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitCheckedExpression(CheckedExpression checkedExpression)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitPrimitiveType(PrimitiveType primitiveType)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitLabelStatement(LabelStatement labelStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitLockStatement(LockStatement lockStatement)
        {
            throw new NotImplementedException();
        }

        public RedILNode VisitCheckedStatement(CheckedStatement checkedStatement)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    #region Public Utilities

    public DataValueType ResolveExpressionType(Expression expr)
        => ExtractTypeFromAnnontations(expr.Annotations);

    public DataValueType ExtractTypeFromAnnontations(IEnumerable<object> annontations)
    {
        var resType = DataValueType.Unknown;
        var ilResolveResult =
            annontations.FirstOrDefault(annot => annot is ResolveResult) as ResolveResult;

        if (!(ilResolveResult is null))
        {
            if (ilResolveResult.Type.Kind == TypeKind.Array)
            {
                resType = DataValueType.Array;
            }
            else
            {
                var systemType = Type.GetType(ilResolveResult.Type.ReflectionName);
                resType = systemType is null ? DataValueType.Unknown : TypeUtilities.GetValueType(systemType);

                if (resType == DataValueType.Unknown)
                {
                    resType = MainResolver.ResolveDataType(ilResolveResult.Type);
                }
            }
        }

        return resType;
    }

    public IParameterizedMember GetInvocationResolveResult(Expression expr)
    {
        var invocResult = expr.Annotations
            .FirstOrDefault(annot => annot is CSharpInvocationResolveResult) as CSharpInvocationResolveResult;

        if (invocResult is null)
        {
            throw new RedILException($"Unable to get invocation resolve from '{expr}'");
        }

        return invocResult.Member;
    }

    public RedILNode IfTable(Context context, DataValueType type,
        IList<KeyValuePair<ExpressionNode, ExpressionNode>> table)
    {
        table = table.Where(kv => !kv.Key.EqualOrNull(ExpressionNode.False)).ToList();
        var truth = table.SingleOrDefault(kv => kv.Key.EqualOrNull(ExpressionNode.True));
        if (!(truth.Key is null))
        {
            return truth.Value;
        }

        if (context.IsPartOfBlock())
        {
            var ifNode = new IfNode();
            ifNode.Ifs = table.Select(kv =>
                    new KeyValuePair<ExpressionNode, RedILNode>(kv.Key,
                        new BlockNode() { Children = new[] { kv.Value } }))
                .ToList();
            return ifNode;
        }
        else
        {
            var temp = new TemporaryIdentifierNode(type);
            var ifNode = new IfNode();
            ifNode.Ifs = table.Select(kv => new KeyValuePair<ExpressionNode, RedILNode>(kv.Key,
                new BlockNode() { Children = new[] { new AssignNode(temp, kv.Value) } })).ToList();

            context.CurrentBlock.Children.Add(new VariableDeclareNode(temp, null));
            context.CurrentBlock.Children.Add(ifNode);

            return temp;
        }
    }

    public ExpressionNode Dictionary(Context context, string dictName, DictionaryTableDefinitionNode dict,
        ExpressionNode key)
    {
        if (key.Type == RedILNodeType.Constant)
        {
            var constKey = (ConstantValueNode)key;
            var val = dict.Elements.FirstOrDefault(kv => kv.Key.Equals(constKey));
            if (val.Key is null)
            {
                throw new RedILException($"Could not find key '{constKey.Value}' in dictionary");
            }

            return val.Value;
        }

        var temp = new TemporaryIdentifierNode("_s_" + dictName, DataValueType.Dictionary);
        context.Root.GlobalVariables.Add(
            new VariableDeclareNode(temp, dict));
        return new TableKeyAccessNode(temp, key, DataValueType.Unknown);
    }

    #endregion

    public CSharpCompiler(LuaCompileFlags flags)
    {
        MainResolver = new MainResolver(flags);
    }

    public CSharpCompiler(LuaCompileFlags flags, string luaClass)
    {
        MainResolver = new MainResolver(flags);
        LuaClass = luaClass;
        ClassInitializerTableNode = new DictionaryTableDefinitionNode();
        PostConstructorObjectInitializerNode = new IfNode(new List<KeyValuePair<ExpressionNode, RedILNode>>()
        {
            new(
                new IdentifierNode("_initializer", DataValueType.Dictionary),
                new IteratorLoopNode(("_initializerKey", "_initializerValue"),
                    new IdentifierNode("_initializer", DataValueType.Dictionary),
                    new BlockNode(new List<RedILNode>()
                    {
                        new AssignNode(
                            new TableKeyAccessNode(new IdentifierNode("self", DataValueType.Dictionary),
                                new IdentifierNode("_initializerKey", DataValueType.String),
                                DataValueType.Unknown),
                            new IdentifierNode("_initializerValue", DataValueType.Unknown))
                    })))
        }, null);
    }

    public RootNode Compile(DecompilationResult csharp)
    {
        var visitor = new AstVisitor(this);
        var node = csharp.Body.AcceptVisitor(visitor);
        return node as RootNode;
    }

    public RedILNode CompileNode(DecompilationResult csharp)
    {
        var visitor = new AstVisitor(this);
        return csharp.Body.AcceptVisitor(visitor);
    }
}