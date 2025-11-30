using CCSharp.Attributes;
using CCSharp.ComputerCraft;
using CCSharp.AdvancedMath;

namespace CCSharp.CCVS;
/// <summary>
/// This API is added by CC: VS and allows CCSharp to access information from Valkyrien Skies Ships.
/// </summary>
public class Ship
{
    /// <summary>
    /// Gets the Ship's unique ID
    /// </summary>
    /// <returns>The Ship's unique ID</returns>
    [LuaMethod("ship.getId")]
    public static Long GetId() => default;

    /// <summary>
    /// Gets the Ship's unique Slug
    /// </summary>
    /// <returns>The Ship's unique Slug</returns>
    [LuaMethod("ship.getSlug")]
    public static string GetSlug() => default;

    /// <summary>
    /// Sets the Ship's Slug
    /// </summary>
    /// <param name="slug">The new Slug</returns>
    [LuaMethod("ship.setSlug")]
    public static void SetSlug(string slug) { }

    /// <summary>
    /// Gets the Ship's Mass
    /// </summary>
    /// <returns>The Ship's Mass</returns>
    [LuaMethod("ship.getMass")]
    public static double GetMass() => default;

    /// <summary>
    /// Determines whether the Ship is a static or dynamic object
    /// </summary>
    /// <returns>True if the Ship is a static object, otherwise false</returns>
    [LuaMethod("ship.isStatic")]
    public static bool IsStatic() => default;

    /// <summary>
    /// Converts the Ship to and from a static object
    /// </summary>
    /// <param name="s">Whether the Ship should be static</returns>
    [LuaMethod("ship.setStatic")]
    public static void SetStatic(bool s) { }

    /// <summary>
    /// Gets all Constraints affecting the Ship
    /// </summary>
    /// <returns>A dictionary of all Constraints and their respective IDs</returns>
    [LuaMethod("ship.getConstraints")]
    public static Dictionary<Long, Constraint> GetConstraints() => default;

    /// <summary>
    /// Gets the Ship's Center of Mass in the Shipyard
    /// </summary>
    /// <returns>The Ship's Center of Mass in the Shipyard</returns>
    [LuaMethod("ship.getShipyardPosition")]
    public static Vector3 GetShipyardPosition() => default;

    /// <summary>
    /// Gets the Ship's Center of Mass in Worldspace
    /// </summary>
    /// <returns>The Ship's Center of Mass in Worldspace</returns>
    [LuaMethod("ship.getWorldspacePosition")]
    public static Vector3 GetWorldspacePosition() => default;

    /// <summary>
    /// Gets the Ship's linear velocity
    /// </summary>
    /// <returns>The Ship's linear velocity</returns>
    [LuaMethod("ship.getVelocity")]
    public static Vector3 GetLinearVelocity() => default;

    /// <summary>
    /// Gets the Ship's angular velocity
    /// </summary>
    /// <returns>The Ship's angular velocity</returns>
    [LuaMethod("ship.getOmega")]
    public static Vector3 GetAngularVelocity() => default;

    /// <summary>
    /// Gets the Ship's scale. Note that, despite returning a Vector, the scale is uniform.
    /// </summary>
    /// <returns>The Ship's scale</returns>
    [LuaMethod("ship.getScale")]
    public static Vector3 GetScale() => default;

    /// <summary>
    /// Sets the Ship's scale
    /// </summary>
    /// <param name="scale">The new scale of the Ship</param>
    [LuaMethod("ship.setScale")]
    public static void SetScale(double scale) { }

    /// <summary>
    /// Gets the Ship's orientation as a Quaternion
    /// </summary>
    /// <returns>The Ship's Quaternion</returns>
    [LuaMethod("ship.getQuaternion")]
    public static Quaternion GetQuaternion() => default;

    /// <summary>
    /// Gets the Ship's transformation matrix
    /// </summary>
    /// <returns>The Ship's transformation matrix</returns>
    [LuaMethod("ship.getTransformationMatrix")]
    public static Matrix GetTransformationMatrix() => default;

    /// <summary>
    /// Gets the Ship's current Moment of Inertia Tensor as a Matrix
    /// </summary>
    /// <returns>The Ship's current Moment of Inertia Tensor</returns>
    [LuaMethod("ship.getMomentOfInertiaTensorToSave")]
    public static Matrix GetMomentOfInertiaTensorToSave() => default;

    /// <summary>
    /// Gets the Ship's previous Moment of Inertia Tensor as a Matrix
    /// </summary>
    /// <returns>The Ship's previous Moment of Inertia Tensor</returns>
    [LuaMethod("ship.getMomentOfInertiaTensor")]
    public static Matrix GetMomentOfInertiaTensor() => default;

    /// <summary>
    /// Transform the given Shipyard position to Worldspace if it is on the same Ship
    /// </summary>
    /// <param name="pos">The Shipyard position on the Ship</param>
    /// <returns>The Worldspace position</returns>
    [LuaMethod("ship.transformPositionToWorld")]
    public static Vector3 transformPositionToWorld(Vector3 pos) => default;

    /// <summary>
    /// Holds the thread while awaiting the "physics_ticks" event
    /// </summary>
    /// <returns>The event name and PhysicsTick for every previous physics tick since last gametick</returns>
    [LuaMethod("ship.pullPhysicsTicks")]
    public static (string name, PhysicsTick[] ticks) PullPhysicsTicks() => default;

    /// <summary>
    /// Contains the output data from "physics_ticks" regarding the individual tick
    /// </summary>
    class PhysicsTick
    {
        /// <summary>
        /// Gets the buoyant factor during this physics tick
        /// </summary>
        /// <returns>The Ship's buoyant factor</returns>
        [LuaMethod("getBuoyantFactor")]
        public double GetBuoyantFactor() => default;

        /// <summary>
        /// Determines whether the Ship was static or dynamic during this physics tick
        /// </summary>
        /// <returns>True if it was static dyuring this physics tick, otherwise false</returns>
        [LuaMethod("isStatic")]
        public bool IsStatic() => default;

        /// <summary>
        /// Determines whether the Ship was affected by fluid drag during this physics tick
        /// </summary>
        /// <returns>True if it was affected by fluid drag during this physics tick</returns>
        [LuaMethod("doFluidDrag")]
        public bool FluidDrag() => default;

        /// <summary>
        /// Gets the inertia data during this physics tick
        /// </summary>
        /// <returns>The Ship's inertia data during this physics tick</returns>
        [LuaMethod("getInertia")]
        public Inertia GetInertia() => default;

        /// <summary>
        /// Gets the pose vel data during this physics tick
        /// </summary>
        /// <returns>The Ship's pose vel data during this physics tick</returns>
        [LuaMethod("getPoseVel")]
        public PoseVel GetPoseVel() => default;

        /// <summary>
        /// Gets the force inducers acting on the Ship during this physics tick
        /// </summary>
        /// <returns>The Ship's force inducers during this physics tick</returns>
        [LuaMethod("getForceInducers")]
        public string[] GetForceInducers() => default;

        /// <summary>
        /// The inertia data provided by the physics tick
        /// </summary>
        class Inertia
        {
            [LuaProperty("momentOfInertiaTensor")] public Matrix MomentOfInertiaTensor { get; set; }
            [LuaProperty("mass")] public double Mass { get; set; }
        }

        /// <summary>
        /// The pose vel data provided by the physics tick
        /// </summary>
        class PoseVel
        {
            [LuaProperty("vel")] public Vector3 LinearVelocity { get; set; }
            [LuaProperty("omega")] public Vector3 AngularVelocity { get; set; }
            [LuaProperty("pos")] public Vector3 CenterOfMass { get; set; }
            [LuaProperty("rot")] public Quaternion Rotation { get; set; }
        }

    }

    /// <summary>
    /// Teleports the Ship using the input data. Highly recommend not using.
    /// </summary>
    /// <param name="data">The data to use during teleportation</param>
    [LuaMethod("ship.teleport")]
    public static void Teleport(object data) { }

    /// <summary>
    /// Applies an invariant force to the Ship on the next physics tick
    /// </summary>
    /// <param name="force">The force to be applied</param>
    [LuaMethod("ship.applyInvariantForce")]
    public static void ApplyInvariantForce(Vector3 force) { }
    /// <summary>
    /// Applies an invariant force to the Ship on the next physics tick
    /// </summary>
    /// <param name="forceX">The force to be applied on the X axis</param>
    /// <param name="forceY">The force to be applied on the Y axis</param>
    /// <param name="forceZ">The force to be applied on the Z axis</param>
    [LuaMethod("ship.applyInvariantForce")]
    public static void ApplyInvariantForce(double forceX, double forceY, double forceZ) { }

    /// <summary>
    /// Applies an invariant torque to the Ship on the next physics tick
    /// </summary>
    /// <param name="torque">The torque to be applied</param>
    [LuaMethod("ship.applyInvariantTorque")]
    public static void ApplyInvariantTorque(Vector3 torque) { }
    /// <summary>
    /// Applies an invariant torque to the Ship on the next physics tick
    /// </summary>
    /// <param name="torqueX">The torque to be applied on the X axis</param>
    /// <param name="torqueY">The torque to be applied on the Y axis</param>
    /// <param name="torqueZ">The torque to be applied on the Z axis</param>
    [LuaMethod("ship.applyInvariantTorque")]
    public static void ApplyInvariantTorque(double torqueX, double torqueY, double torqueZ) { }

    /// <summary>
    /// Applies an invariant force to the Ship at the given local offset on the next physics tick
    /// </summary>
    /// <param name="force">The force to be applied</param>
    /// <param name="pos">The local offset ot apply the force to</param>
    [LuaMethod("ship.applyInvariantForceToPos")]
    public static void ApplyInvariantForceToPosition(Vector3 force, Vector3 pos) { }
    /// <summary>
    /// Applies an invariant force to the Ship at the given local offset on the next physics tick
    /// </summary>
    /// <param name="forceX">The force to be applied on the X axis</param>
    /// <param name="forceY">The force to be applied on the Y axis</param>
    /// <param name="forceZ">The force to be applied on the Z axis</param>
    /// <param name="posX">The local X-axis offset to apply the force to</param>
    /// <param name="posY">The local Y-axis offset to apply the force to</param>
    /// <param name="posZ">The local Z-axis offset to apply the force to</param>
    [LuaMethod("ship.applyInvariantForceToPos")]
    public static void ApplyInvariantForceToPosition(double forceX, double forceY, double forceZ, double posX, double posY, double posZ) { }

    /// <summary>
    /// Applies a rotation-dependent force to the Ship on the next physics tick
    /// </summary>
    /// <param name="force">The force to be applied</param>
    [LuaMethod("ship.applyRotDependentForce")]
    public static void ApplyRotDependentForce(Vector3 force) { }
    /// <summary>
    /// Applies a rotation-dependent force to the Ship on the next physics tick
    /// </summary>
    /// <param name="forceX">The force to be applied on the X axis</param>
    /// <param name="forceY">The force to be applied on the Y axis</param>
    /// <param name="forceZ">The force to be applied on the Z axis</param>
    [LuaMethod("ship.applyRotDependentForce")]
    public static void ApplyRotDependentForce(double forceX, double forceY, double forceZ) { }

    /// <summary>
    /// Applies a rotation-dependent torque to the Ship on the next physics tick
    /// </summary>
    /// <param name="torque">The torque to be applied</param>
    [LuaMethod("ship.applyRotDependentTorque")]
    public static void ApplyRotDependentTorque(Vector3 torque) { }
    /// <summary>
    /// Applies a rotation-dependent torque to the Ship on the next physics tick
    /// </summary>
    /// <param name="torqueX">The torque to be applied on the X axis</param>
    /// <param name="torqueY">The torque to be applied on the Y axis</param>
    /// <param name="torqueZ">The torque to be applied on the Z axis</param>
    [LuaMethod("ship.applyRotDependentTorque")]
    public static void ApplyRotDependentTorque(double torqueX, double torqueY, double torqueZ) { }

    /// <summary>
    /// Applies a rotation-dependent force to the Ship at the given local offset on the next physics tick
    /// </summary>
    /// <param name="force">The force to be applied</param>
    /// <param name="pos">The local offset ot apply the force to</param>
    [LuaMethod("ship.applyRotDependentForceToPos")]
    public static void ApplyRotDependentForceToPosition(Vector3 force, Vector3 pos) { }
    /// <summary>
    /// Applies a rotation-dependent force to the Ship at the given local offset on the next physics tick
    /// </summary>
    /// <param name="forceX">The force to be applied on the X axis</param>
    /// <param name="forceY">The force to be applied on the Y axis</param>
    /// <param name="forceZ">The force to be applied on the Z axis</param>
    /// <param name="posX">The local X-axis offset to apply the force to</param>
    /// <param name="posY">The local Y-axis offset to apply the force to</param>
    /// <param name="posZ">The local Z-axis offset to apply the force to</param>
    [LuaMethod("ship.applyRotDependentForceToPos")]
    public static void ApplyRotDependentForceToPosition(double forceX, double forceY, double forceZ, double posX, double posY, double posZ) { }

    /// <summary>
    /// The base constraint data class
    /// </summary>
    [LuaTableTypeCheck(TableAccessor = "type")]
    class Constraint
    {
        [LuaEnum(typeof(ConstraintType))]
        public enum ConstraintType
        {
            [LuaEnumValue("attachment")] Attachment,
            [LuaEnumValue("fixed_attachment_orientation")] FixedAttachmentOrientation,
            [LuaEnumValue("fixed_orientation")] FixedOrientation,
            [LuaEnumValue("hinge_orientation")] HingeOrientation,
            [LuaEnumValue("hinge_swing_limits")] HingeSwingLimits,
            [LuaEnumValue("hinge_target_angle")] HingeTargetAngle,
            [LuaEnumValue("pos_damping")] PosDamping,
            [LuaEnumValue("rope")] Rope,
            [LuaEnumValue("rot_damping")] RotDamping,
            [LuaEnumValue("Slide")] Slide,
            [LuaEnumValue("spherical_swing_limits")] SphericalSwingLimits,
            [LuaEnumValue("spherical_twist_limits")] SphericalTwistLimits
        }

        [LuaProperty("shipId0")] public Long FirstShipID { get; set; }
        [LuaProperty("shipId1")] public Long SecondShipID { get; set; }
        [LuaProperty("type")] public ConstraintType Type { get; set; }
        [LuaProperty("compliance")] public double Compliance { get; set; }
    }

    /// <summary>
    /// The attachment constraint data class
    /// </summary>
    [LuaImplicitTypeArgument("attachment")]
    class AttachmentConstraint : Constraint
    {
        [LuaProperty("localPos0")] public Vector3 FirstPosition { get; set; }
        [LuaProperty("localPos1")] public Vector3 SecondPosition { get; set; }
        [LuaProperty("maxForce")] public double MaxForce { get; set; }
        [LuaProperty("fixedDistance")] public double FixedDistance { get; set; }
    }


    /// <summary>
    /// The hinge swing limits constraint data class
    /// </summary>
    [LuaImplicitTypeArgument("hinge_swing_limits")]
    class HingeSwingLimitsConstraint : Constraint
    {
        [LuaProperty("localRot0")] public Quaternion FirstPosition { get; set; }
        [LuaProperty("localRot1")] public Quaternion SecondPosition { get; set; }
        [LuaProperty("maxTorque")] public double MaxTorque { get; set; }
        [LuaProperty("minSwingAngle")] public double MinSwingAngle { get; set; }
        [LuaProperty("maxSwingAngle")] public double MaxSwingAngle { get; set; }
    }


    /// <summary>
    /// The hinge target angle constraint data class
    /// </summary>
    [LuaImplicitTypeArgument("thinge_target_angle")]
    class HingeTargetAngleConstraint : Constraint
    {
        [LuaProperty("localRot0")] public Quaternion FirstPosition { get; set; }
        [LuaProperty("localRot1")] public Quaternion SecondPosition { get; set; }
        [LuaProperty("maxTorque")] public double MaxTorque { get; set; }
        [LuaProperty("targetAngle")] public double TargetAngle { get; set; }
        [LuaProperty("nextTickTargetAngle")] public double NextTickTargetAngle { get; set; }
    }


    /// <summary>
    /// The position damping constraint data class
    /// </summary>
    [LuaImplicitTypeArgument("pos_damping")]
    class PosDampingConstraint : Constraint
    {
        [LuaProperty("localPos0")] public Vector3 FirstPosition { get; set; }
        [LuaProperty("localPos1")] public Vector3 SecondPosition { get; set; }
        [LuaProperty("maxForce")] public double MaxForce { get; set; }
        [LuaProperty("posDamping")] public double PosDamping { get; set; }
    }

    /// <summary>
    /// The rope constraint data class
    /// </summary>
    [LuaImplicitTypeArgument("rope")]
    class RopeConstraint : Constraint
    {
        [LuaProperty("localPos0")] public Vector3 FirstPosition { get; set; }
        [LuaProperty("localPos1")] public Vector3 SecondPosition { get; set; }
        [LuaProperty("maxForce")] public double MaxForce { get; set; }
        [LuaProperty("ropeLength")] public double RopeLength { get; set; }
    }


    /// <summary>
    /// The rotation damping constraint data class
    /// </summary>
    [LuaImplicitTypeArgument("rot_damping")]
    class RotDampingConstraint : Constraint
    {
        [LuaEnum(typeof(RotDampingAxes))]
        public enum RotDampingAxes
        {
            [LuaEnumValue("parallel")] Parallel,
            [LuaEnumValue("perpendicular")] Perpendicular,
            [LuaEnumValue("all_axes")] AllAxes
        }

        [LuaProperty("localPos0")] public Vector3 FirstPosition { get; set; }
        [LuaProperty("localPos1")] public Vector3 SecondPosition { get; set; }
        [LuaProperty("maxForce")] public double MaxForce { get; set; }
        [LuaProperty("rotDamping")] public double RotDamping { get; set; }
        [LuaProperty("rotDampingAxes")] public RotDampingAxes RotDampingAxis { get; set; }
    }

    /// <summary>
    /// The slide constraint data class
    /// </summary>
    [LuaImplicitTypeArgument("slide")]
    class SlideConstraint : Constraint
    {
        [LuaProperty("localPos0")] public Vector3 FirstPosition { get; set; }
        [LuaProperty("localPos1")] public Vector3 SecondPosition { get; set; }
        [LuaProperty("maxForce")] public double MaxForce { get; set; }
        [LuaProperty("localSlideAxis0")] public Vector3 LocalSlideAxis { get; set; }
        [LuaProperty("maxDistBetweenPoints")] public double MaxDistanceBetweenPoints { get; set; }
    }

    /// <summary>
    /// The spherical swing limits constraint data class
    /// </summary>
    [LuaImplicitTypeArgument("spherical_swing_limits")]
    class SphericalSwingLimitsConstraint : Constraint
    {
        [LuaProperty("localRot0")] public Quaternion FirstPosition { get; set; }
        [LuaProperty("localRot1")] public Quaternion SecondPosition { get; set; }
        [LuaProperty("maxTorque")] public double MaxTorque { get; set; }
        [LuaProperty("minSwingAngle")] public double MinSwingAngle { get; set; }
        [LuaProperty("maxSwingAngle")] public double MaxSwingAngle { get; set; }
    }

    /// <summary>
    /// The spherical twist limits constraint data class
    /// </summary>
    [LuaImplicitTypeArgument("spherical_twist_limits")]
    class SphericalTwistLimitsConstraint : Constraint
    {
        [LuaProperty("localRot0")] public Quaternion FirstPosition { get; set; }
        [LuaProperty("localRot1")] public Quaternion SecondPosition { get; set; }
        [LuaProperty("maxTorque")] public double MaxTorque { get; set; }
        [LuaProperty("minTwistAngle")] public double MinTwistAngle { get; set; }
        [LuaProperty("maxTwistAngle")] public double MaxTwistAngle { get; set; }
    }
}