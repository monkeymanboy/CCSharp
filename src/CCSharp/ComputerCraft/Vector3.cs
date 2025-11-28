using CCSharp.Attributes;

namespace CCSharp.ComputerCraft;

/// <summary>
/// A basic 3D vector type and some common vector operations. This may be useful when working with coordinates in Minecraft's world (such as those from the gps API).
/// </summary>
public class Vector3
{
    [LuaProperty("x")]
    public double X { get; set; }
    [LuaProperty("y")]
    public double Y { get; set; }
    [LuaProperty("z")]
    public double Z { get; set; }

    /// <summary>
    /// Construct a new Vector with the given coordinates.
    /// </summary>
    /// <param name="x">The X coordinate or direction of the vector.</param>
    /// <param name="y">The Y coordinate or direction of the vector.</param>
    /// <param name="z">The Z coordinate or direction of the vector.</param>
    [LuaConstructor("vector.new")]
    public Vector3(double x, double y, double z) { }

    /// <summary>
    /// Construct a new 0 length vector
    /// </summary>
    [LuaConstructor("vector.new")]
    public Vector3() { }
    
    /// <summary>
    /// Get the length (also referred to as magnitude) of this vector.
    /// </summary>
    /// <returns>The length of this vector.</returns>
    [LuaMethod("length")]
    public double Length() => default;
    
    /// <summary>
    /// Divide this vector by its length, producing with the same direction, but of length 1.
    /// </summary>
    /// <returns>The normalised vector.</returns>
    [LuaMethod("normalize")]
    public Vector3 Normalize() => default;
    
    /// <summary>
    /// Construct a vector with each dimension rounded to the nearest integer value.
    /// </summary>
    /// <returns>The rounded vector.</returns>
    [LuaMethod("round")]
    public Vector3 Round() => default;
    
    /// <summary>
    /// Construct a vector with each dimension rounded to the nearest value.
    /// </summary>
    /// <param name="tolerance">The tolerance that we should round to, defaulting to 1. For instance, a tolerance of 0.5 will round to the nearest 0.5.</param>
    /// <returns>The normalised vector.</returns>
    [LuaMethod("round")]
    public Vector3 Round(float tolerance) => default;
    
    /// <summary>
    /// Compute the cross product of two vectors
    /// </summary>
    /// <param name="other">The second vector to compute the cross product of.</param>
    /// <returns>The cross product of self and other.</returns>
    [LuaMethod("cross")]
    public Vector3 Cross(Vector3 other) => default;
    
    /// <summary>
    /// Compute the dot product of two vectors
    /// </summary>
    /// <param name="other">The second vector to compute the dot product of.</param>
    /// <returns>The dot product of self and other.</returns>
    [LuaMethod("dot")]
    public Vector3 Dot(Vector3 other) => default;
    
    public static Vector3 operator +(Vector3 a, Vector3 b) => default;
    public static Vector3 operator -(Vector3 a, Vector3 b) => default;
    public static Vector3 operator *(Vector3 a, double scalar) => default;
    public static Vector3 operator /(Vector3 a, double scalar) => default;
    public static Vector3 operator -(Vector3 a) => default;
}