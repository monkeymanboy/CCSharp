using CCSharp.Attributes;
using CCSharp.ComputerCraft;

namespace CCSharp.AdvancedMath;

/// <summary>
/// A basic PID type and common PID operations. This may be useful when working with control systems.
/// </summary>
[LuaRequiredModule("AdvancedMath.pid", "pid")]
public abstract class PID<T, Y>
{
    /// <summary>
    /// The setpoint for the PID to reach
    /// </summary>
    [LuaProperty("sp")] public T SetPoint { get; set; }
    /// <summary>
    /// The proportional gain - how aggressively to respond to the current error
    /// </summary>
    [LuaProperty("kp")] public double KP { get; set; }
    /// <summary>
    /// The integral gain - how aggressively to eliminate accumulated error
    /// </summary>
    [LuaProperty("ki")] public double KI { get; set; }
    /// <summary>
    /// The derivative gain - how aggressively to dampen the rate of change
    /// </summary>
    [LuaProperty("kd")] public double KD { get; set; }
    /// <summary>
    /// Whether to treat the PID as discrete or continuous
    /// </summary>
    [LuaProperty("discrete")] public bool Discrete { get; set; }
    /// <summary>
    /// The internal Integral being influenced each step
    /// </summary>
    [LuaProperty("integral")] public T Integral { get; set; }
    /// <summary>
    /// The error from the previous step
    /// </summary>
    [LuaProperty("prev_error")] public T PreviousError { get; set; }

    private PID(T target, double p, double i, double d, bool discrete) { }

    /// <summary>
    /// Performs a PID control step
    /// </summary>
    /// <param name="value">The current value being measured</param>
    /// <param name="dt">The time since the last step</param>
    /// <returns>The control output</returns>
    [LuaMethod("step")]
    public Y Step(T value, double dt) => default;
    /// <summary>
    /// Performs a PID control step
    /// </summary>
    /// <param name="value">The current value being measured</param>
    [LuaMethod("step")]
    public Y Step(T value) => default;

    /// <summary>
    /// Assigns the clamps on the control output
    /// </summary>
    /// <param name="min">The minimum control output</param>
    /// <param name="max">The maximum control output</param>
    [LuaMethod("clampOutput")]
    public void ClampOutput(double min, double max) => default;
    /// <summary>
    /// Disables the clamps on the control output
    /// </summary>
    [LuaMethod("clampOutput")]
    public void ClampOutput() => default;

    /// <summary>
    /// Assigns the limits on the Integral
    /// </summary>
    /// <param name="min">The minimum integral limit</param>
    /// <param name="max">The maximum integral limit</param>
    [LuaMethod("limitIntegral")]
    public void LimitIntegral(double min, double max) => default;
    /// <summary>
    /// Disables the limits on the Integral
    /// </summary>
    [LuaMethod("limitIntegral")]
    public void LimitIntegral() => default;

    /// <summary>
    /// A Scalar PID for handling doubles
    /// </summary>
    public class Scalar : PID<double, double> 
    {
        /// <summary>
        /// Constructs a new Scalar PID Controller
        /// </summary>
        /// <param name="target">The setpoint to reach</param>
        /// <param name="p">The proporional gain</param>
        /// <param name="i">The integral gain</param>
        /// <param name="d">The derivative gain</param>
        /// <param name="discrete">Whether the PID controller is discrete or continuous</param>
        [LuaConstructor("pid.new")]
        public Scalar(T target, double p, double i, double d, bool discrete) : base(target, p, i, d, discrete) { }
    }
    /// <summary>
    /// A Vector PID for handling Vectors
    /// </summary>
    public class Vector : PID<Vector3, Vector3> 
    {
        /// <summary>
        /// Constructs a new Vector PID Controller
        /// </summary>
        /// <param name="target">The setpoint to reach</param>
        /// <param name="p">The proporional gain</param>
        /// <param name="i">The integral gain</param>
        /// <param name="d">The derivative gain</param>
        /// <param name="discrete">Whether the PID controller is discrete or continuous</param>
        [LuaConstructor("pid.new")]
        public Vector(T target, double p, double i, double d, bool discrete) : base(target, p, i, d, discrete) { }
    }
    /// <summary>
    /// A Quaternion PID for handling Quaternion. Step outputs the angular velocity as the control output.
    /// </summary>
    public class Quat : PID<Quaternion, Vector3>
    {
        /// <summary>
        /// Constructs a new Quaternion PID Controller
        /// </summary>
        /// <param name="target">The setpoint to reach</param>
        /// <param name="p">The proporional gain</param>
        /// <param name="i">The integral gain</param>
        /// <param name="d">The derivative gain</param>
        /// <param name="discrete">Whether the PID controller is discrete or continuous</param>
        [LuaConstructor("pid.new")]
        public Quat(T target, double p, double i, double d, bool discrete) : base(target, p, i, d, discrete) { }
    }
}