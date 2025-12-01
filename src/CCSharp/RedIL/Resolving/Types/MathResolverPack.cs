using System;
using System.Collections.Generic;
using System.IO.Compression;
using CCSharp.Attributes;
using CCSharp.Enums;
using CCSharp.RedIL.Resolving.Attributes;
using CCSharp.RedIL.Resolving.CommonResolvers;

namespace CCSharp.RedIL.Resolving.Types;

class MathResolverPack
{
    class MathProxy
    {
        #region Abs
            
        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.MathAbs)]
        public static double Abs(double num) => default;

        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.MathAbs)]
        public static decimal Abs(decimal num) => default;

        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.MathAbs)]
        public static float Abs(float num) => default;
            
        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.MathAbs)]
        public static int Abs(int num) => default;

        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.MathAbs)]
        public static long Abs(long num) => default;

        #endregion
            
        #region Min
            
        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.MathMin)]
        public static double Min(double a, double b) => default;

        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.MathMin)]
        public static decimal Min(decimal a, decimal b) => default;

        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.MathMin)]
        public static float Min(float a, float b) => default;
            
        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.MathMin)]
        public static int Min(int a, int b) => default;

        [RedILResolve(typeof(CallLuaBuiltinStaticMethodResolver), LuaBuiltinMethod.MathMin)]
        public static long Min(long a, long b) => default;
            
        #endregion
            
        #region Pow
            
        [LuaMethod("math.pow")]
        public static double Pow(double a, double b) => default;
            
        #endregion
    }
        
    public static Dictionary<Type, Type> GetMapToProxy()
    {
        return new Dictionary<Type, Type>()
        {
            {typeof(Math), typeof(MathProxy)}
        };
    }
}