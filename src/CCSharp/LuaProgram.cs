using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using CCSharp.Attributes;
using CCSharp.Lua;
using CCSharp.RedIL;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

namespace CCSharp;

public class LuaProgram
{
    public static DecompilerSettings DecompilerSettings =
        new()
        {
            ExtensionMethods = false,
            NamedArguments = false,
            QueryExpressions = false,
            NullPropagation = false
        };

    public string Filename { get; set; }
    public HashSet<LuaRequireModuleAttribute> Modules = new();
    public string CompiledCode { get; set; }
    public List<(string,bool)> MainMethods = new();
    
    public LuaCompileFlags Flags { get; set; }

    public string FullCode => string.Join("\n",
        Modules.Select(module => $"local {module.Variable} = require(\"{module.Module}\")")
            .Append(CompiledCode)
            .Union(MainMethods.Select(((method) => method.Item2 ? $"{method.Item1}({{...}})" : $"{method.Item1}()"))));

    public static LuaProgram FromType(Type type)
    {
        LuaProgramAttribute luaProgramAttribute = type.GetCustomAttribute<LuaProgramAttribute>();
        LuaProgram program = new()
        {
            Filename = luaProgramAttribute?.Filename ?? $"{type.Name}.lua",
            Flags = luaProgramAttribute?.Flags ?? LuaCompileFlags.None
        };
        var decompiler = new CSharpDecompiler(type.Assembly.Location, DecompilerSettings);
        var syntaxTree = decompiler.Decompile(new List<EntityHandle> { MetadataTokens.EntityHandle(type.GetTypeInfo().MetadataToken) });
        Console.WriteLine(syntaxTree);
        var compiler = new CSharpCompiler(program.Flags);
        var rootNode = compiler.CompileNode(new DecompilationResult(syntaxTree));
        program.Modules = compiler.MainResolver.RequiredModules;
        program.CompiledCode = new CompilationInstance(rootNode).Compile();
        HashSet<Type> compiledDependencies = new();
        program.CompiledCode=$"{CompileDependencies(program, compiler.MainResolver.LuaClassDependencies, compiledDependencies)}\n{program.CompiledCode}";
        foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
        {
            if(methodInfo.GetCustomAttribute<LuaMainAttribute>() != null)
                program.MainMethods.Add((methodInfo.Name, methodInfo.GetParameters().Length == 1));
        }
        program.CompiledCode = program.CompiledCode.Trim('\n');
        return program;
    }

    private static string CompileDependencies(LuaProgram program, HashSet<Type> dependencies, HashSet<Type> compiledDependencies)
    {
        string compiledDependenciesString = "";
        foreach (Type type in dependencies)
        {
            if (!compiledDependencies.Add(type)) continue;
            var decompiler = new CSharpDecompiler(type.Assembly.Location, DecompilerSettings);
            var syntaxTree = decompiler.Decompile(new List<EntityHandle>
                { MetadataTokens.EntityHandle(type.GetTypeInfo().MetadataToken) });
            //Console.WriteLine(syntaxTree);
            var compiler = new CSharpCompiler(program.Flags, type.GetLuaClassName(program.Flags));
            var rootNode = compiler.CompileNode(new DecompilationResult(syntaxTree));
            foreach (LuaRequireModuleAttribute module in compiler.MainResolver.RequiredModules)
            {
                program.Modules.Add(module);
            }
            string subDependenciesString = CompileDependencies(program, compiler.MainResolver.LuaClassDependencies, compiledDependencies);
            if(!string.IsNullOrEmpty(subDependenciesString)) 
                compiledDependenciesString = $"{subDependenciesString}\n{compiledDependenciesString}".Trim('\n');;
            compiledDependenciesString = $"{compiledDependenciesString}\n{new CompilationInstance(rootNode).Compile()}";
        }
        return compiledDependenciesString.Trim('\n');
    }

    public void Export(string directory = "", bool includeDependencies = true)
    {
        if (includeDependencies)
        {
            foreach (LuaRequireModuleAttribute module in Modules)
            {
                if (module.Module == "linq")
                    File.WriteAllText(Path.Combine(directory, "linq.lua"), ModuleDependencies.LINQ);
            }
        }

        File.WriteAllText(Path.Combine(directory, Filename), FullCode);
    }
}