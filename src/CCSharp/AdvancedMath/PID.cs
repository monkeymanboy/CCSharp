using CCSharp.Attributes;
using CCSharp.ComputerCraft;

namespace CCSharp.AdvancedMath;

[LuaRequiredModule("AdvancedMath.pid", "pid")]
public class PID<T, Y>
{
    [LuaProperty("sp")] public T SetPoint { get; set; }
    [LuaProperty("kp")] public double KP { get; set; }
    [LuaProperty("ki")] public double KI { get; set; }
    [LuaProperty("kd")] public double KD { get; set; }
    [LuaProperty("discrete")] public bool Discrete { get; set; }
    [LuaProperty("integral")] public T Integral { get; set; }
    [LuaProperty("prev_error")] public T PreviousError { get; set; }

    private PID(T target, double p, double i, double d, bool discrete) { }

    [LuaMethod("step")]
    public Y Step(T value, double dt) => default;
    [LuaMethod("step")]
    public Y Step(T value) => default;

    [LuaMethod("clampOutput")]
    public void ClampOutput(double min, double max) => default;
    [LuaMethod("clampOutput")]
    public void ClampOutput() => default;

    [LuaMethod("limitIntegral")]
    public void LimitIntegral(double min, double max) => default;
    [LuaMethod("limitIntegral")]
    public void LimitIntegral() => default;

    public class Scalar : PID<double, double> 
    {
        [LuaConstructor("pid.new")]
        public Scalar(T target, double p, double i, double d, bool discrete) : base(target, p, i, d, discrete) { }
    }
    public class Vector : PID<Vector3, Vector3> 
    {
        [LuaConstructor("pid.new")]
        public Vector(T target, double p, double i, double d, bool discrete) : base(target, p, i, d, discrete) { }
    }
    public class Quat : PID<Quaternion, Vector3>
    {
        [LuaConstructor("pid.new")]
        public Quat(T target, double p, double i, double d, bool discrete) : base(target, p, i, d, discrete) { }
    }
}