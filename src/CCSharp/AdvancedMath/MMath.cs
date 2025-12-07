using System;
using CCSharp.Attributes;
using CCSharp.ComputerCraft;

namespace CCSharp.AdvancedMath;

/// <summary>
/// A grab bag of linear equation solvers. Useful for finding solution using linear algebra.
/// </summary>
[LuaRequireModule("AdvancedMath.mmath", "mmath")]
public class MMath
{
    /// <summary>
    /// Solves f(x) = g(x) over an interval by probing many starting 
    /// points and using Newton-Raphson.
    /// </summary>
    /// <param name="f">The first function in the system of equations</param>
    /// <param name="g">The second function in the system of equations</param>
    /// <param name="min">The lower bound of the search interval</param>
    /// <param name="max">The upper bound of the search interval</param>
    /// <param name="steps">The number of probe seeds across the interval Needs to be greater than or equal to one.</param>
    /// <param name="tol">The tolerance forwarded to the root finder</param>
    /// <param name="closeThresh">The distance threshold  used to consider two roots identical</param>
    /// <returns>An array of root approximations found within [min, max]</returns>
    [LuaMethod("mmath.solveSysEq")]
    public static double[] SolveSystemOfEquations(Func<double, double> f, Func<double, double> g, double min, double max, double steps, double tol, double closeThresh) => default;
    [LuaMethod("mmath.solveSysEq")]
    public static double[] SolveSystemOfEquations(Func<double, double> f, Func<double, double> g, double min, double max, double steps, double tol) => default;
    [LuaMethod("mmath.solveSysEq")]
    public static double[] SolveSystemOfEquations(Func<double, double> f, Func<double, double> g, double min, double max, double steps) => default;

    /// <summary>
    /// Expand a set of values according to integer weights.
    /// 
    /// For each index i in values, values[i] is inserted into the 
    /// returned array weights[i] times.
    /// </summary>
    /// <param name="values">Array of values to repeat</param>
    /// <param name="weights">Array of integer weights</param>
    /// <returns>Array with values repeated according to weights</returns>
    [LuaMethod("mmath.weightedTable")]
    public static double[] WeightedTable(double[] values, double[] weights) => default;

    /// <summary>
    /// Shuffle/randomize the array and return the new scrambled array.
    /// 
    /// This function mutates the input array by repeatedly removing each
    /// element and inserting it at a random position. It is a simple shuffle and
    /// not an in-place Fisher–Yates implementation.
    /// </summary>
    /// <param name="t">The array to scramble</param>
    /// <returns>The new scrambled array</returns>
    [LuaMethod("mmath.scamble")]
    public static double[] Scramble(double[] t) => default;

    /// <summary>
    /// Numerically integrate f(x) on [a, b] using the trapezoid rule.
    /// </summary>
    /// <param name="f">The function to integrate</param>
    /// <param name="n">The number of subdivision. Must be greater than zero.</param>
    /// <param name="a">The lower bound</param>
    /// <param name="b">The upper bound</param>
    /// <param name="init">The optional initial accumulator</param>
    /// <returns>The approximation of the integral of f(x) on [a, b]</returns>
    [LuaMethod("mmath.integrateSimple")]
    public static double IntegrateSimple(Func<double, double> f, double n, double a, double b, double init) => default;
    [LuaMethod("mmath.integrateSimple")]
    public static double IntegrateSimple(Func<double, double> f, double n, double a, double b) => default;

    /// <summary>
    /// Numerically integrate f(x) on [a, b] using Simpson-like parabolic rule.
    /// 
    /// If n is odd it is decremented to make it even. This method is generally
    /// more accurate than the trapezoid rule for smooth functions.
    /// </summary>
    /// <param name="f">The function to integrate</param>
    /// <param name="n">The number of subdivision. Must be greater than zero.</param>
    /// <param name="a">The lower bound</param>
    /// <param name="b">The upper bound</param>
    /// <param name="init">The optional initial accumulator</param>
    /// <returns>The approximation of the integral of f(x) on [a, b]</returns>
    [LuaMethod("mmath.integrateComplex")]
    public static double IntegrateComplex(Func<double, double> f, double n, double a, double b, double init) => default;
    [LuaMethod("mmath.integrateComplex")]
    public static double IntegrateComplex(Func<double, double> f, double n, double a, double b) => default;

    /// <summary>
    /// Approximate the derivative (central difference) of a single-variable function.
    /// 
    /// Computes (f(x + h) - f(x - h)) / (2 * h). Use smaller h to approach the analytical
    /// derivative but beware of numerical round-off for extremely small h.
    /// </summary>
    /// <param name="f">The function to differentiate</param>
    /// <param name="x">The point at which to evalluate the derivative</param>
    /// <param name="h">The step size</param>
    /// <returns>The numerical derivative approximation</returns>
    [LuaMethod("mmath.ARC")]
    public static double ApproximateRateOfChange(Func<double, double> f, double x, double h) => default;

    [LuaMethod("mmath.ARC")]
    public static double ApproximateRateOfChange(Func<double, double> f, double x) => default;

    /// <summary>
    /// Find a root using Newton–Raphson starting from x0.
    /// 
    /// Attempts up to 500 iterations. The derivative is approximated using ApproximateRateOfChange.
    /// </summary>
    /// <param name="f">The function for which to find a root</param>
    /// <param name="x0">The initial guess</param>
    /// <param name="tol">The convergence tolerance for changes in x</param>
    /// <returns>On success returns the root (a number), otherwise returns null to indicate failure.</returns>
    [LuaMethod("mmath.solveRoot")]
    public static double SolveRoot(Func<double, double> f, double x0, double tol) => default;
}