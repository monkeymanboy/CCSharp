using CCSharp.Attributes;
using CCSharp.ComputerCraft;

namespace CCSharp.AdvancedMath;

[LuaRequiredModule("AdvancedMath.mmath", "mmath")]
public class MMath
{
    [LuaMethod("mmath.solveSysEq")]
    public static double[] SolveSystemOfEquations(Func<double, double> f, Func<double, double> g, double min, double max, double steps, double tol, double closeThresh) => default;

    [LuaMethod("mmath.weightedTable")]
    public static double[] WeightedTable(double[] values, double[] weights) => default;

    [LuaMethod("mmath.scamble")]
    public static double[] Scramble(double[] t) => default;

    [LuaMethod("mmath.integrateSimple")]
    public static double IntegrateSimple(Func<double, double> f, double n, double a, double b, double init) => default;

    [LuaMethod("mmath.integrateComplex")]
    public static double IntegrateComplex(Func<double, double> f, double n, double a, double b, double init) => default;

    [LuaMethod("mmath.ARC")]
    public static double ApproximateRateOfChange(Func<double, double> f, double x, double h) => default;

    [LuaMethod("mmath.solveRoot")]
    public static double SolveRoot(Func<double, double> f, double x0, double tol) => default;
}