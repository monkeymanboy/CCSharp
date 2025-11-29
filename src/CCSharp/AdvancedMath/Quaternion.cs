using CCSharp.Attributes;
using CCSharp.ComputerCraft;

namespace CCSharp.AdvancedMath;

/// <summary>
/// A basic quaternion type and some common quaternion operations. This may be useful when working with rotation in regards to physics (such as those from the Ship API provided by CC: VS).
/// </summary>
public class Quaternion
{
    [LuaProperty("vec")]
    public Vector3 Imaginary { get; set; }
    [LuaProperty("a")]
    public double Real { get; set; }

    /// <summary>
    /// Constructs a new Quaternion from a Vector and a w parameter. Similarly to fromComponents, this method will not produce a normalized Quaternion.
    /// </summary>
    /// <param name="vec">The imaginary component of the Vector, stored in a Vector.</param>
    /// <param name="w">The real component of the Quaternion.</param>
    [LuaConstructor("quaternion.new")]
    public Quaternion(Vector3 vec, double w) { }

    /// <summary>
    /// Constructs a new Quaternion from the provided axis - angle parameters. The resulting Quaternion is already normalized.
    /// </summary>
    /// <param name="axis">The rotation axis that will be used for the Quaternion. The axis does not need to be normalized.</param>
    /// <param name="angle">The angle to rotate by, in radians.</param>
    [LuaConstructor("quaternion.fromAxisAngle")]
    public Quaternion(Vector3 axis, double angle) { }

    /// <summary>
    /// Constructs a new quaternion using the provided pitch, yaw and roll. Uses the YXZ reference frame
    /// </summary>
    /// <param name="pitch">The pitch in radians.</param>
    /// <param name="yaw">The yaw in radians.</param>
    /// <param name="roll">The roll in radians.</param>
    [LuaConstructor("quaternion.fromEuler")]
    public Quaternion(double pitch, double yaw, double roll) { }

    /// <summary>
    /// Constructs a new Quaternion from its components. Note that this will not produce a normalized Quaternion.
    /// </summary>
    /// <param name="x">The X component of the imaginary part.</param>
    /// <param name="y">The Y component of the imaginary part.</param>
    /// <param name="z">The Z component of the imaginary part.</param>
    /// <param name="w">The real component of the Quaternion.</param>
    [LuaConstructor("quaternion.fromComponents")]
    public Quaternion(double x, double y, double z, double w) { }

    /// <summary>
    /// Constructs a new quaternion from a 3x3 rotation matrix or 4x4 transformation matrix.
    /// </summary>
    /// <param name="matrix">The rotation matrix to convert to a Quaternion.</param>
    [LuaConstructor("quaternion.fromMatrix")]
    public Quaternion(Matrix matrix) { }

    /// <summary>
    /// Constructs a new identity Quaternion (0, 0, 0, 1).
    /// </summary>
    [LuaConstructor("quaternion.identity")]
    public Quaternion() { }

    /// <summary>
    /// Get the length (also referred to as magnitude) of this Quaternion.
    /// </summary>
    /// <returns>The length of this quaternion.</returns>
    [LuaMethod("length")]
    public double Length() => default;

    /// <summary>
    /// Finds the conjugate of the Quaternion.
    /// </summary>
    /// <returns>The conjugated Quaternion.</returns>
    [LuaMethod("conjugate")]
    public Quaternion Conjugate() => default;

    /// <summary>
    /// Normalizes the Quaternion, producing a unit Quaternion with the same direction.
    /// </summary>
    /// <returns>The normalized Quaternion.</returns>
    [LuaMethod("normalize")]
    public Quaternion Normalize() => default;

    /// <summary>
    /// Inverts the Quaternion.
    /// </summary>
    /// <returns>The inverted Quaternion.</returns>
    [LuaMethod("inverse")]
    public Quaternion Inverse() => default;

    /// <summary>
    /// Performs spherical linear interpolation between this Quaternion and another Quaternion.
    /// </summary>
    /// <param name="b">The target Quaternion to interpolate towards.</param>
    /// <param name="alpha">The interpolation factor, between 0 and 1.</param>
    /// <returns>The interpolated Quaternion.</returns>
    [LuaMethod("slerp")]
    public Quaternion Slerp(Quaternion b, double alpha) => default;

    /// <summary>
    /// Gets the angle of rotation represented by this Quaternion, in radians.
    /// </summary>
    /// <returns>The angle of rotation in radians.</returns>
    [LuaMethod("getAngle")]
    public double GetAngle() => default;

    /// <summary>
    /// Gets the axis of rotation represented by this Quaternion.
    /// </summary>
    /// <returns>The axis of rotation as a Vector3.</returns>
    [LuaMethod("getAxis")]
    public Vector3 GetAxis() => default;

    /// <summary>
    /// Converts this Quaternion to pitch, yaw and roll angles, using the YXZ reference frame
    /// </summary>
    /// <returns>A tuple containing the pitch, yaw and roll in radians.</returns>
    [LuaMethod("toEuler", CallMethodFlags.WrapAsTable)]
    public (double pitch, double yaw, double roll) ToEuler() => default;

    /// <summary>
    /// Converts this Quaternion to a 3x3 rotation matrix.
    /// </summary>
    /// <returns>The rotation matrix.</returns>
    [LuaMethod("toMatrix")]
    public Matrix ToMatrix() => default;

    /// <summary>
    /// Checks if any component of this Quaternion is NaN.
    /// </summary>
    /// <returns>True if any component is NaN, false otherwise.</returns>
    [LuaMethod("isNan")]
    public bool IsNaN() => default;

    /// <summary>
    /// Checks if any component of this Quaternion is infinite.
    /// </summary>
    /// <returns>True if any component is infinite, false otherwise.</returns>
    [LuaMethod("isInf")]
    public bool IsInfinite() => default;

    /// <summary>
    /// Creates a copy of this Quaternion.
    /// </summary>
    /// <returns>The copied Quaternion.</returns>
    [LuaMethod("copy")]
    public Quaternion Copy() => default;

    public static Quaternion operator +(Quaternion a, Quaternion b) => default;
    public static Quaternion operator -(Quaternion a, Quaternion b) => default;
    public static Quaternion operator *(Quaternion a, Quaternion b) => default;
    public static Vector3 operator *(Quaternion q, Vector3 s) => default;
    public static Quaternion operator *(Quaternion q, double s) => default;
    public static Quaternion operator /(Quaternion q, Quaternion s) => default;
    public static Quaternion operator /(Quaternion q, double s) => default;
    public static Quaternion operator -(Quaternion q) => default;
}