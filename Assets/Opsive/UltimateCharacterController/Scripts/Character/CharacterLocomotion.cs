/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The CharacterLocomotion class serves as the base character controller class. It handles the base locomotion with the following features:
    /// - Movement
    /// - Collision Detection
    /// - Physic Materials
    /// - Slopes
    /// - Stairs
    /// - Push Rigidbodies
    /// - Gravity Direction
    /// - Root Motion
    /// - Variable Time Scale
    /// - CapsuleCollider, SphereCollider and BoxCollider support (for generic characters)
    /// - Moving Platforms
    /// </summary>
    public class CharacterLocomotion : StateBehavior
    {
        // Padding value used to prevent the character's collider from overlapping the environment collider. Overlapped colliders don't work well with ray casts.
        private const float c_ColliderSpacing = 0.01f;

        [Tooltip("Should the motor be interpolated between FixedUpdate ticks?")]
        [SerializeField] protected bool m_Interpolate = true;
        [Tooltip("Should root motion be used to move the character?")]
        [SerializeField] protected bool m_UseRootMotionPosition;
        [Tooltip("If using root motion, applies a multiplier to the root motion delta position.")]
        [SerializeField] protected float m_RootMotionSpeedMultiplier = 1;
        [Tooltip("The rate at which the character's motor force accelerates. Only used by non-root motion characters.")]
        [SerializeField] protected Vector3 m_MotorAcceleration = new Vector3(3.2f, 0, 3.2f);
        [Tooltip("The rate at which the character's motor force decelerates. Only used by non-root motion characters.")]
        [SerializeField] protected float m_MotorDamping = 5.4f;
        [Tooltip("A multiplier which is applied to the motor while moving backwards.")]
        [SerializeField] protected float m_MotorBackwardsMultiplier = 0.7f;
        [Tooltip("A (0-1) value specifying the amount of influence the previous acceleration direction has on the current velocity.")]
        [Range(0, 1)] [SerializeField] protected float m_PreviousAccelerationInfluence = 1;
        [Tooltip("Should the motor force be adjusted while on a slope?")]
        [SerializeField] protected bool m_AdjustMotorForceOnSlope = true;
        [Tooltip("If adjusting the motor force on a slope, the force multiplier when on an upward slope.")]
        [SerializeField] protected float m_MotorSlopeForceUp = 1f;
        [Tooltip("If adjusting the motor force on a slope, the force multiplier when on a downward slope.")]
        [SerializeField] protected float m_MotorSlopeForceDown = 1.25f;

        [Tooltip("Should root motion be used to rotate the character?")]
        [SerializeField] protected bool m_UseRootMotionRotation;
        [Tooltip("If using root motion, applies a multiplier to the root motion delta rotation.")]
        [SerializeField] protected float m_RootMotionRotationMultiplier = 1;
        [Tooltip("The rate at which the character can rotate. Only used by non-root motion characters.")]
        [SerializeField] protected float m_MotorRotationSpeed = 0.15f;
        [Tooltip("The rate at which the character adjusts to the new up direction.")]
        [SerializeField] protected float m_UpAlignmentRotationSpeed = 0.2f;

        [Tooltip("The up direction of the character.")]
        [SerializeField] protected Vector3 m_Up = Vector3.up;
        [Tooltip("Should gravity be applied?")]
        [SerializeField] protected bool m_UseGravity = true;
        [Tooltip("The amount of gravity that should be accumulated each move.")]
        [SerializeField] protected float m_GravityAmount = 0.2f;
        [Tooltip("The direction of the gravity.")]
        [SerializeField] protected Vector3 m_GravityDirection = Vector3.down;
        [Tooltip("Specifies the width of the characters skin, use for ground detection.")]
        [SerializeField] protected float m_SkinWidth = 0.08f;
        [Tooltip("Should the character stick to the ground?")]
        [SerializeField] protected bool m_StickToGround = true;
        [Tooltip("The distance that should be used when the character is sticking to the ground.")]
        [SerializeField] protected float m_StickToGroundDistance = 0.3f;
        [Tooltip("The rate at which the character's external force decelerates.")]
        [SerializeField] protected float m_ExternalForceDamping = 0.1f;
        [Tooltip("A curve specifying the amount to move when gliding along a wall. The x variable represents the dot product between the character look direction and wall normal. " +
                 "An x value of 0 means the character is looking directly at the wall. An x value of 1 indicates the character is looking parallel to the wall.")]
        [SerializeField] protected AnimationCurve m_WallGlideCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(0.1f, 0.5f, 0, 0), new Keyframe(1, 0.5f) });
        [Tooltip("A multiplier to apply to the physic material friction value when colliding with the wall.")]
        [SerializeField] protected float m_WallFrictionModifier = 1f;
        [Tooltip("A multiplier to apply to the physic material bounce value when colliding with the wall. Allows for the character to bounce off the wall.")]
        [SerializeField] protected float m_WallBounceModifier = 2;
        [Tooltip("A multiplier to apply to the physic material friction value when colliding with the ground.")]
        [SerializeField] protected float m_GroundFrictionModifier = 10f;
        [Tooltip("A multiplier to apply to the physic material bounce value when colliding with the ground. Allows for the character to bounce off of the ground.")]
        [SerializeField] protected float m_GroundBounceModifier = 1;

        [Tooltip("The maximum object slope angle that the character can traverse (in degrees).")]
        [SerializeField] protected float m_SlopeLimit = 50f;
        [Tooltip("The maximum height that the character can step on top of.")]
        [SerializeField] protected float m_MaxStepHeight = 0.35f;
        [Tooltip("The local time scale of the character.")]
        [Range(0, 4)] [SerializeField] protected float m_TimeScale = 1;
        [Tooltip("Can the character detect horizontal collisions?")]
        [SerializeField] protected bool m_DetectHorizontalCollisions = true;
        [Tooltip("Can the character detect vertical collisions?")]
        [SerializeField] protected bool m_DetectVerticalCollisions = true;
        [Tooltip("Can the character detect the ground?")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_GroundDetection")] // 3.0.9.
        [SerializeField] protected bool m_DetectGround = true;
        [Tooltip("Should the character check for collisions even when not moving?")]
        [SerializeField] protected bool m_ContinuousCollisionDetection = true;
        [Tooltip("The layers that can act as colliders for the character. This does not affect the collision detection. For collision detection use the Character Layer Manager.")]
        [SerializeField] protected LayerMask m_ColliderLayerMask = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.SubCharacter |
                                                                     1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect);
        [Tooltip("The maximum number of colliders that the character can detect.")]
        [SerializeField] protected int m_MaxCollisionCount = 100;
        [Tooltip("The maximum number of frames that the soft force can be distributed by.")]
        [SerializeField] protected int m_MaxSoftForceFrames = 100;
        [Tooltip("The maximum number of collision checks that should be performed when moving.")]
        [SerializeField] protected int m_MaxMovementCollisionChecks = 5;
        [Tooltip("The maximum number of penetration checks against a single collider.")]
        [SerializeField] protected int m_MaxPenetrationChecks = 5;
        [Tooltip("The maximum number of collision checks that should be performed when rotating.")]
        [SerializeField] protected int m_MaxRotationCollisionChecks = 10;

        [Tooltip("Should the character stick to the moving platform? If false the character will inherit the moving platform's momentum when the platform stops quickly.")]
        [SerializeField] protected bool m_StickToMovingPlatform = true;
        [Tooltip("The velocity magnitude required for the character to separate from the moving platform due to a sudden moving platform stop.")]
        [SerializeField] protected float m_MovingPlatformSeperationVelocity = 5;
        [Tooltip("The rate at which the character's moving platform force decelerates when the character is no longer on the platform.")]
        [SerializeField] protected float m_MovingPlatformForceDamping = 0.1f;

        public float ColliderSpacing => c_ColliderSpacing;
        public bool Interpolate { get { return m_Interpolate; } set { m_Interpolate = value; } }
        public bool UseRootMotionPosition { get => m_UseRootMotionPosition; set => m_UseRootMotionPosition = value; }
        public float RootMotionSpeedMultiplier { get => m_RootMotionSpeedMultiplier; set => m_RootMotionSpeedMultiplier = value; }
        public Vector3 MotorAcceleration { get => m_MotorAcceleration; set => m_MotorAcceleration = value; }
        public float MotorDamping { get => m_MotorDamping; set => m_MotorDamping = value; }
        public float MotorBackwardsMultiplier { get => m_MotorBackwardsMultiplier; set => m_MotorBackwardsMultiplier = value; }
        public float PreviousAccelerationInfluence { get => m_PreviousAccelerationInfluence; set => m_PreviousAccelerationInfluence = value; }
        public bool AdjustMotorForceOnSlope { get => m_AdjustMotorForceOnSlope; set => m_AdjustMotorForceOnSlope = value; }
        public float MotorSlopeForceUp { get => m_MotorSlopeForceUp; set => m_MotorSlopeForceUp = value; }
        public float MotorSlopeForceDown { get => m_MotorSlopeForceDown; set => m_MotorSlopeForceDown = value; }
        public bool UseRootMotionRotation { get => m_UseRootMotionRotation; set => m_UseRootMotionRotation = value; }
        public float RootMotionRotationMultiplier { get => m_RootMotionRotationMultiplier; set => m_RootMotionRotationMultiplier = value; }
        public float MotorRotationSpeed { get => m_MotorRotationSpeed; set => m_MotorRotationSpeed = value; }
        public float UpAlignmentRotationSpeed { get => m_UpAlignmentRotationSpeed; set => m_UpAlignmentRotationSpeed = value; }
        public Vector3 Up { get => m_Up; set => m_Up = value; }
        public bool UseGravity { get => m_UseGravity; set => m_UseGravity = value; }
        public float GravityAmount { get => m_GravityAmount; set => m_GravityAmount = value; }
        public Vector3 GravityDirection { get => m_GravityDirection; set => m_GravityDirection = value; }
        public float SkinWidth { get => m_SkinWidth; set => m_SkinWidth = value; }
        public bool StickToGround { get => m_StickToGround; set => m_StickToGround = value; }
        public float StickToGroundDistance { get => m_StickToGroundDistance; set => m_StickToGroundDistance = value; }
        public float ExternalForceDamping { get => m_ExternalForceDamping; set => m_ExternalForceDamping = value; }
        public AnimationCurve WallGlideCurve { get => m_WallGlideCurve; set => m_WallGlideCurve = value; }
        public float WallFrictionModifier { get => m_WallFrictionModifier; set => m_WallFrictionModifier = value; }
        public float WallBounceModifier { get => m_WallBounceModifier; set => m_WallBounceModifier = value; }
        public float GroundFrictionModifier { get => m_GroundFrictionModifier; set => m_GroundFrictionModifier = value; }
        public float GroundBounceModifier { get => m_GroundBounceModifier; set => m_GroundBounceModifier = value; }
        public float SlopeLimit { get => m_SlopeLimit; set => m_SlopeLimit = value; }
        public float MaxStepHeight { get => m_MaxStepHeight; set => m_MaxStepHeight = value; }
        public virtual float TimeScale { get => m_TimeScale; set => m_TimeScale = value; }
        public bool DetectHorizontalCollisions { get => m_DetectHorizontalCollisions; set => m_DetectHorizontalCollisions = value; }
        public bool DetectVerticalCollisions { get => m_DetectVerticalCollisions; set => m_DetectVerticalCollisions = value; }
        public bool DetectGround { get => m_DetectGround; set => m_DetectGround = value; }
        public bool ContinuousCollisionDetection { get => m_ContinuousCollisionDetection; set => m_ContinuousCollisionDetection = value; }
        public LayerMask ColliderLayerMask { get => m_ColliderLayerMask; set => m_ColliderLayerMask = value; }
        public int MaxCollisionCount => m_MaxCollisionCount;
        public int MaxSoftForceFrames => m_MaxSoftForceFrames;
        public int MaxMovementCollisionChecks => m_MaxMovementCollisionChecks;
        public int MaxPenetrationChecks => m_MaxPenetrationChecks;
        public int MaxRotationCollisionChecks => m_MaxRotationCollisionChecks;
        public bool StickToMovingPlatform { get => m_StickToMovingPlatform; set => m_StickToMovingPlatform = value; }
        public float MovingPlatformSeperationVelocity { get => m_MovingPlatformSeperationVelocity; set => m_MovingPlatformSeperationVelocity = value; }
        public float MovingPlatformForceDamping { get => m_MovingPlatformForceDamping; set => m_MovingPlatformForceDamping = value; }

        protected Transform m_Transform;
        protected Rigidbody m_Rigidbody;
        protected CharacterLayerManager m_CharacterLayerManager;

        protected Vector2 m_InputVector;
        protected Vector3 m_DeltaRotation;
        private Vector3 m_DesiredMovement;
        private Quaternion m_DesiredRotation = Quaternion.identity;

        private float m_Height;
        private float m_Radius = float.MaxValue;
        private bool m_CollisionLayerEnabled = true;
        protected Collider[] m_Colliders;
        private Collider[] m_IgnoredColliders;
        private GameObject[] m_ColliderGameObjects;
        private GameObject[] m_IgnoredColliderGameObjects;
        protected int m_ColliderCount;
        private int m_IgnoredColliderCount;
        private int[] m_ColliderLayers;
        private int[] m_IgnoredColliderLayers;
        private bool m_CheckRotationCollision;
        private bool m_AllowUseGravity = true;
        private bool m_ForceUseGravity;
        private bool m_AllowRootMotionPosition = true;
        private bool m_AllowRootMotionRotation = true;
        private bool m_ForceRootMotionPosition;
        private bool m_ForceRootMotionRotation;
        private bool m_AllowHorizontalCollisionDetection = true;
        private bool m_ForceHorizontalCollisionDetection;
        private bool m_AllowVerticalCollisionDetection = true;
        private bool m_ForceVerticalCollisionDetection;
        private bool m_AllowGroundCollisionDetection = true;
        private bool m_ForceGroundCollisionDetection;
        private bool m_AlignToUpDirection;

        protected RaycastHit[] m_CastResults;
        protected UnityEngineUtility.RaycastHitComparer m_CastHitComparer = new UnityEngineUtility.RaycastHitComparer();
        private int m_ColliderIndex;
        private Dictionary<RaycastHit, int> m_ColliderIndexMap;
        private RaycastHit[] m_CombinedCastResults;
        private Collider[] m_OverlapCastResults;

        private bool m_Grounded;
        private RaycastHit m_GroundedRaycastHit;
        private Collider m_CharacterGroundedCollider;
        private RaycastHit m_EmptyRaycastHit = new RaycastHit();
        private bool m_ForceUngrounded = false;
        private float m_GravityAccumulation;
        private bool m_ForceStickToGround;
        private bool m_SlopedGround;

        private Vector3 m_MotorThrottle;
        private float m_SlopeFactor = 1;
        private Vector3 m_RootMotionDeltaPosition;
        private Quaternion m_RootMotionDeltaRotation = Quaternion.identity;
        private Quaternion m_PrevMotorRotation;

        private Vector3 m_ExternalForce;
        private Vector3[] m_SoftForceFrames;

        protected Transform m_MovingPlatform;
        protected Vector3 m_MovingPlatformRelativePosition;
        protected Quaternion m_MovingPlatformRotationOffset;
        private Vector3 m_PreMovingPlatformMovement;
        private Vector3 m_MovingPlatformMovement;
        protected Quaternion m_MovingPlatformRotation = Quaternion.identity;
        private Quaternion m_PrevMovingPlatformRotation;
        private bool m_MovingPlatformOverride;

        protected Vector3 m_MovingPlatformDisconnectMovement;
        protected Quaternion m_MovingPlatformDisconnectRotation = Quaternion.identity;
        private bool m_ApplyMovingPlatformDisconnectMovement;
        protected float m_MovingPlatformDisconnectVelocityMaxMagnitude = 0.01f;
        protected float m_MovingPlatformDisconnectTorqueMaxMagnitude = 0.01f;

        private bool m_InstantRigidbodyMove;
        private Vector3 m_Velocity;
        private Quaternion m_Torque;
        private Action<RaycastHit> m_OnCollision;

        private int m_SimulationIndex = -1;

        [Shared.Utility.NonSerialized] public Vector2 InputVector { get { return m_InputVector; } set { m_InputVector = value; } }
        [Shared.Utility.NonSerialized] public Vector3 DeltaRotation { get { return m_DeltaRotation; } set { m_DeltaRotation = value; } }
        [Shared.Utility.NonSerialized] public Quaternion DesiredRotation { get { return m_DesiredRotation; } set { m_DesiredRotation = value; } }
        [Shared.Utility.NonSerialized] public Vector3 DesiredMovement { get { return m_DesiredMovement; } set { m_DesiredMovement = value; } }
        public float Height => m_Height;
        public float Radius => m_Radius;
        public bool CollisionLayerEnabled => m_CollisionLayerEnabled;
        public Vector3 Center => m_Rigidbody.InverseTransformPoint(m_Rigidbody.position + (m_Up * m_Height / 2));
        public Collider[] Colliders => m_Colliders;
        public int ColliderCount => m_ColliderCount;
        public Collider[] IgnoredColliders => m_IgnoredColliders;
        public int IgnoredColliderCount => m_IgnoredColliderCount;
        [Shared.Utility.NonSerialized] public Vector3 LocalDesiredMovement
        {
            get { return m_Rigidbody.InverseTransformDirection(m_DesiredMovement); }
            set {
                if (Math.Abs(value.y) > LocalDesiredMovement.y && m_Grounded) { m_ForceUngrounded = true; }
                m_DesiredMovement = m_Rigidbody.TransformDirection(value);
            }
        }
        [Shared.Utility.NonSerialized] public bool ForceStickToGround { get { return m_ForceStickToGround; } set { m_ForceStickToGround = value; } }
        public bool StickingToGround => m_StickToGround || m_ForceStickToGround;
        [Shared.Utility.NonSerialized] public Vector3 RootMotionDeltaPosition { get { return m_RootMotionDeltaPosition; } set { m_RootMotionDeltaPosition = value; } }
        [Shared.Utility.NonSerialized] public Quaternion RootMotionDeltaRotation { get { return m_RootMotionDeltaRotation; } set { m_RootMotionDeltaRotation = value; } }
        [Shared.Utility.NonSerialized] public Transform MovingPlatform { get { return m_MovingPlatform; } set { SetMovingPlatform(value); } }
        public Vector3 MovingPlatformMovement { get { return m_MovingPlatformMovement; } }
        public Quaternion MovingPlatformRotation { get { return m_MovingPlatformRotation; } }
        [Shared.Utility.NonSerialized] public bool InstantRigidbodyMove { get { return m_InstantRigidbodyMove; } set { m_InstantRigidbodyMove = value; } }
        public Vector3 Velocity => m_Velocity;
        public Vector3 LocalVelocity => MathUtility.InverseTransformDirection(m_Velocity, m_Rigidbody.rotation * m_DesiredRotation);
        public Quaternion Torque => m_Torque;
        public Quaternion LocalTorque => MathUtility.InverseTransformQuaternion(m_Torque, m_Rigidbody.rotation * m_DesiredRotation);
        public bool Grounded { get { return m_Grounded; } set { m_Grounded = value; } }
        public RaycastHit GroundedRaycastHit => m_GroundedRaycastHit;
        [Shared.Utility.NonSerialized] public float GravityAccumulation { get { return m_GravityAccumulation; } set { m_GravityAccumulation = value; } }
        [Shared.Utility.NonSerialized] public Action<RaycastHit> OnCollision { get { return m_OnCollision; } set { m_OnCollision = value; } }

        public bool AllowUseGravity { set { m_AllowUseGravity = value; } }
        public bool ForceUseGravity { set { m_ForceUseGravity = value; } }
        public bool UsingGravity { get { return (m_UseGravity || m_ForceUseGravity) && m_AllowUseGravity; } }
        public bool AllowRootMotionPosition { set { m_AllowRootMotionPosition = value; } }
        public bool ForceRootMotionPosition { set { m_ForceRootMotionPosition = value; } }
        public bool UsingRootMotionPosition { get { return (m_UseRootMotionPosition || m_ForceRootMotionPosition) && m_AllowRootMotionPosition; } }
        public bool AllowRootMotionRotation { set { m_AllowRootMotionRotation = value; } }
        public bool ForceRootMotionRotation { set { m_ForceRootMotionRotation = value; } }
        public bool UsingRootMotionRotation { get { return (m_UseRootMotionRotation || m_ForceRootMotionRotation) && m_AllowRootMotionRotation; } }
        public bool AllowHorizontalCollisionDetection { set { m_AllowHorizontalCollisionDetection = value; } }
        public bool ForceHorizontalCollisionDetection { set { m_ForceHorizontalCollisionDetection = value; } }
        public bool UsingHorizontalCollisionDetection { get { return (m_DetectHorizontalCollisions || m_ForceHorizontalCollisionDetection) && m_AllowHorizontalCollisionDetection; } }
        public bool AllowVerticalCollisionDetection { set { m_AllowVerticalCollisionDetection = value; } }
        public bool ForceVerticalCollisionDetection { set { m_ForceVerticalCollisionDetection = value; } }
        public bool UsingVerticalCollisionDetection { get { return (m_DetectVerticalCollisions || m_ForceVerticalCollisionDetection) && m_AllowVerticalCollisionDetection; } }
        public bool AllowGroundCollisionDetection { set { m_AllowGroundCollisionDetection = value; } }
        public bool ForceGroundCollisionDetection { set { m_ForceGroundCollisionDetection = value; } }
        public bool UsingGroundCollisionDetection { get { return (m_DetectGround || m_ForceGroundCollisionDetection) && m_AllowGroundCollisionDetection; } }
        public bool AlignToUpDirection { get { return m_AlignToUpDirection; } set { m_AlignToUpDirection = value; } }

        public int SimulationIndex { get { return m_SimulationIndex; } set { m_SimulationIndex = value; } }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        protected override void Awake()
        {
            if (!CharacterInitializer.AutoInitialization) {
                CharacterInitializer.Instance.OnAwake += AwakeInternal;
                return;
            }

            AwakeInternal();
        }

        /// <summary>
        /// Internal method which initializes the default values.
        /// </summary>
        protected virtual void AwakeInternal()
        {
            if (!CharacterInitializer.AutoInitialization) {
                CharacterInitializer.Instance.OnAwake -= AwakeInternal;
            }

            m_Transform = transform;
            m_Rigidbody = gameObject.GetCachedComponent<Rigidbody>();
            m_Rigidbody.drag = m_Rigidbody.angularDrag = 0;
            m_Rigidbody.isKinematic = true;
            m_CharacterLayerManager = gameObject.GetCachedComponent<CharacterLayerManager>();

            base.Awake();

            // Initialize the colliders.
            var colliders = GetComponentsInChildren<Collider>(true);
            m_Colliders = new Collider[colliders.Length];
            m_IgnoredColliders = new Collider[colliders.Length];
            for (int i = 0; i < colliders.Length; ++i) {
                // There are a variety of colliders which should be ignored.
                if (!colliders[i].enabled || colliders[i].isTrigger) {
                    continue;
                }
                // Sub colliders are parented to the character but they are not used for collision detection.
                if (!MathUtility.InLayerMask(colliders[i].gameObject.layer, m_ColliderLayerMask) || !(colliders[i] is CapsuleCollider || colliders[i] is SphereCollider || colliders[i] is BoxCollider)) {
                    m_IgnoredColliders[m_IgnoredColliderCount] = colliders[i];
                    m_IgnoredColliderCount++;
                    continue;
                }

                // Determine the mim radius of the character.
                float radius;
                if (colliders[i] is CapsuleCollider) {
                    radius = (colliders[i] as CapsuleCollider).radius;
                } else if (colliders[i] is SphereCollider) {
                    radius = (colliders[i] as SphereCollider).radius;
                } else { // BoxCollider.
                    radius = (colliders[i] as BoxCollider).size.x / 2; // Not a true radius, but serves the purpose.
                }
                if (radius < m_Radius) {
                    m_Radius = radius;
                }

                m_Colliders[m_ColliderCount] = colliders[i];
                m_ColliderCount++;

                // Determine the max height of the character based on the collider.
                var height = MathUtility.LocalColliderHeight(m_Transform, colliders[i]);
                if (height > m_Height) {
                    m_Height = height;
                }

                // The rotation collider check only needs to be checked if the collider rotates on an axis other than the relative-y axis.
                if (!m_CheckRotationCollision) {
                    m_CheckRotationCollision = CanCauseRotationCollision(colliders[i]);
                }
            }

            // Resize the array depending on the number of valid colliders.
            if (m_Colliders.Length != m_ColliderCount) {
                Array.Resize(ref m_Colliders, m_ColliderCount);
            }
            if (m_Colliders.Length == 0) {
                Debug.LogWarning($"Warning: The character {name} doesn't contain any colliders. Capsule, sphere and box colliders are supported.", this);
            }
            if (m_IgnoredColliders.Length != m_IgnoredColliderCount) {
                Array.Resize(ref m_IgnoredColliders, m_IgnoredColliderCount);
            }
            m_ColliderLayers = new int[m_Colliders.Length];
            m_IgnoredColliderLayers = new int[m_IgnoredColliders.Length];

            // Cache the collider GameObjects for best performance.
            m_ColliderGameObjects = new GameObject[m_Colliders.Length];
            for (int i = 0; i < m_ColliderGameObjects.Length; ++i) {
                m_ColliderGameObjects[i] = m_Colliders[i].gameObject;
            }
            m_IgnoredColliderGameObjects = new GameObject[m_IgnoredColliders.Length];
            for (int i = 0; i < m_IgnoredColliderGameObjects.Length; ++i) {
                m_IgnoredColliderGameObjects[i] = m_IgnoredColliders[i].gameObject;
            }

            // Physics initialization.
            m_CastResults = new RaycastHit[m_MaxCollisionCount];
            m_OverlapCastResults = new Collider[m_MaxCollisionCount];
            m_SoftForceFrames = new Vector3[m_MaxSoftForceFrames];

            // If there are multiple colliders then save a mapping between the raycast hit and the collider index.
            if (m_Colliders.Length > 1) {
                m_ColliderIndexMap = new Dictionary<RaycastHit, int>(new UnityEngineUtility.RaycastHitEqualityComparer());
                m_CombinedCastResults = new RaycastHit[m_CastResults.Length * m_Colliders.Length];
            }

            // Final initialization.
            m_Up = m_Rigidbody.rotation * Vector3.up;
            m_PrevMotorRotation = m_Rigidbody.rotation;
        }

        /// <summary>
        /// The character has been enabled.
        /// </summary>
        private void OnEnable()
        {
            if (!CharacterInitializer.AutoInitialization) {
                CharacterInitializer.Instance.OnEnable += OnEnableInternal;
                return;
            }

            OnEnableInternal();
        }

        /// <summary>
        /// Internal method which enables the character.
        /// </summary>
        protected virtual void OnEnableInternal()
        {
            if (!CharacterInitializer.AutoInitialization) {
                CharacterInitializer.Instance.OnEnable -= OnEnableInternal;
            }

            m_SimulationIndex = SimulationManager.RegisterCharacter(this);
            ResetRotationPosition();

            EnableColliderCollisionLayer(false);
            DetectGroundCollision();
            EnableColliderCollisionLayer(true);
        }

        /// <summary>
        /// Can the specified collider cause a collision when the character is rotating? The collider can cause a rotation collision when it would rotate on
        /// an axis other than the relative-y axis.
        /// </summary>
        /// <param name="rotationCollider">The collider to check against.</param>
        /// <returns>True if the collider could cause a rotation collision.</returns>
        private bool CanCauseRotationCollision(Collider rotationCollider)
        {
            Vector3 direction;
            if (rotationCollider is CapsuleCollider) {
                Vector3 startEndCap, endEndCap;
                var capsuleCollider = rotationCollider as CapsuleCollider;
                var colliderTransform = rotationCollider.transform;
                // The CapsuleCollider's end caps and the center position must be on the same relative-y axis.
                direction = m_Rigidbody.InverseTransformDirection(colliderTransform.TransformPoint(capsuleCollider.center) - m_Rigidbody.position);
                if (Mathf.Abs(direction.x) > 0.0001f || Mathf.Abs(direction.z) > 0.0001f) {
                    return true;
                }
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, colliderTransform.position, colliderTransform.rotation, out startEndCap, out endEndCap);
                direction = m_Rigidbody.InverseTransformDirection(startEndCap - endEndCap);
            } else if (rotationCollider is SphereCollider) {
                direction = m_Rigidbody.InverseTransformDirection(rotationCollider.transform.TransformPoint((rotationCollider as SphereCollider).center) - m_Rigidbody.position);
            } else {
                return true; // Box Colliders will always cause a rotation collision.
            }
            if (Mathf.Abs(direction.x) > 0.0001f || Mathf.Abs(direction.z) > 0.0001f) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets the locomotion for the next movement input.
        /// </summary>
        public void PreMove()
        {
            m_PreMovingPlatformMovement = m_MovingPlatformMovement = Vector3.zero;
            m_MovingPlatformRotation = Quaternion.identity;

            if (m_MovingPlatform != null) {
                // Update the rotation changes.
                if (m_AlignToUpDirection) {
                    var movingPlatformRotation = m_MovingPlatform.rotation;
                    var deltaRotation = Quaternion.Inverse(m_PrevMovingPlatformRotation) * movingPlatformRotation;
                    var target = (deltaRotation * m_MovingPlatformRotationOffset);
                    m_MovingPlatformRotation = Quaternion.Inverse(m_MovingPlatformRotationOffset * Quaternion.Inverse(m_MovingPlatformRotation)) * target;
                } else {
                    var characterRotation = m_Rigidbody.rotation;
                    var localRotation = MathUtility.InverseTransformDirection(m_MovingPlatform.eulerAngles - m_PrevMovingPlatformRotation.eulerAngles, characterRotation);
                    localRotation.x = localRotation.z = 0;
                    m_MovingPlatformRotation = Quaternion.Euler(MathUtility.TransformDirection(localRotation, characterRotation));
                }

                // Update the position changes.
                m_PreMovingPlatformMovement = m_MovingPlatformMovement;
                m_MovingPlatformMovement = m_MovingPlatform.TransformPoint(m_MovingPlatformRelativePosition) - m_Rigidbody.position;
            }
        }

        /// <summary>
        /// Moves the character according to the input. This method exists to allow AI to easily move the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        /// <param name="deltaYawRotation">Value specifying the number of degrees changed on the local yaw axis.</param>
        public virtual void Move(float horizontalMovement, float forwardMovement, float deltaYawRotation)
        {
            if (m_TimeScale == 0 || Time.deltaTime == 0) {
                return;
            }

            // Assign the inputs.
            m_InputVector.x = horizontalMovement;
            m_InputVector.y = forwardMovement;
            m_DeltaRotation.Set(0, deltaYawRotation, 0);
            //m_InputVector.x = 1;
            //m_InputVector.y = 1;
            //m_DeltaRotation.Set(0, 5, 0);

            // Reset the previous values.
            m_DesiredMovement = Vector3.zero;
            m_DesiredRotation = Quaternion.identity;

            EnableColliderCollisionLayer(false);

            UpdateCharacter();

            EnableColliderCollisionLayer(true);
        }

        /// <summary>
        /// Moves and rotates the character.
        /// </summary>
        protected virtual void UpdateCharacter()
        {
            // Before any other movements are done the character should first stay aligned to any moving platforms.
            UpdateMovingPlatformMovement();

            // Update the rotation before the position so the forces will be applied in the correct direction.
            UpdateRotation();

            // Set the new rotation.
            ApplyRotation();

            // Update the position values based on the input.
            UpdatePosition();

            // The movement values have been determined. Update the DesiredMovement before the collision and ground checks.
            UpdateDesiredMovement();

            // Update the external forces after the movement has been applied.
            // This should be done after the movement is applied so the full force value has been applied within UpdatePosition.
            UpdateExternalForces();

            // Check for collisions.
            DetectCollisions();

            // Is the character still on the ground?
            DetectGroundCollision();

            // Set the new position.
            ApplyPosition();
        }

        /// <summary>
        /// Updates the position and rotation changes while on a moving platform.
        /// </summary>
        private void UpdateMovingPlatformMovement()
        {
            if (m_MovingPlatform == null) {
                if (m_Grounded) {
                    return;
                }
                // The character may have previously been on a platform and should inherit the platform movement
                if (m_ApplyMovingPlatformDisconnectMovement) {
                    m_ApplyMovingPlatformDisconnectMovement = !m_Grounded && UpdateMovingPlaformDisconnectMovement();
                }
                return;
            }

            m_DesiredRotation *= m_MovingPlatformRotation;
            m_DesiredMovement += m_MovingPlatformMovement;

            // If the character doesn't stick to the moving platform and the platform slows down more than the separation velocity then the
            // moving platform momentum should be transferred to the character.
            if (!m_StickToMovingPlatform) {
                var prevMovingPlatformVelocity = m_PreMovingPlatformMovement / (m_TimeScale * Time.deltaTime);
                var movingPlatformVelocity = m_MovingPlatformMovement / (m_TimeScale * Time.deltaTime);
                if (movingPlatformVelocity.sqrMagnitude < prevMovingPlatformVelocity.sqrMagnitude && (movingPlatformVelocity - prevMovingPlatformVelocity).magnitude > m_MovingPlatformSeperationVelocity) {
                    AddForce(prevMovingPlatformVelocity, 1, false);
                }
            }
        }

        /// <summary>
        /// Updates the velocity and torque of the character when the character leaves the platform.
        /// </summary>
        /// <returns>True if the velocity or torque disconnect values are greater than the specified max magnitude values.</returns>
        protected virtual bool UpdateMovingPlaformDisconnectMovement()
        {
            var damping = m_MovingPlatformForceDamping * m_TimeScale;
            m_MovingPlatformDisconnectMovement /= (1 + damping);
            m_DesiredMovement += m_MovingPlatformDisconnectMovement;

            m_MovingPlatformDisconnectRotation = Quaternion.RotateTowards(m_MovingPlatformDisconnectRotation, Quaternion.identity, damping);
            m_DesiredRotation *= m_MovingPlatformDisconnectRotation;

            return m_MovingPlatformDisconnectMovement.magnitude > m_MovingPlatformDisconnectVelocityMaxMagnitude ||
                   Quaternion.Angle(m_MovingPlatformDisconnectRotation, Quaternion.identity) > m_MovingPlatformDisconnectTorqueMaxMagnitude;
        }

        /// <summary>
        /// Updates the character's rotation. The DesiredRotation will be set based on the root motion/input values.
        /// </summary>
        protected virtual void UpdateRotation()
        {
            // Rotate according to the root motion rotation or target rotation.
            Quaternion rotationDelta, targetRotation;
            var rotation = m_Rigidbody.rotation * m_DesiredRotation;
            if (UsingRootMotionRotation) {
                targetRotation = rotation * m_RootMotionDeltaRotation;
            } else {
                targetRotation = Quaternion.Slerp(rotation, rotation * Quaternion.Euler(m_DeltaRotation), m_MotorRotationSpeed * m_TimeScale * Time.timeScale);
            }

            // Rotate to match the desired up direction.
            if (m_AlignToUpDirection) {
                var proj = (targetRotation * Vector3.forward) - Vector3.Dot(targetRotation * Vector3.forward, m_Up) * m_Up;
                if (proj.sqrMagnitude > 0.0001f) {
                    if (m_MovingPlatform == null) {
                        targetRotation = Quaternion.Slerp(targetRotation, Quaternion.LookRotation(proj, m_Up), m_UpAlignmentRotationSpeed * m_TimeScale * Time.timeScale);
                    } else {
                        targetRotation = Quaternion.LookRotation(proj, m_Up);
                    }
                } else {
                    // Prevents locking the rotation if proj magnitude is close to 0 when character forward is close or equal to the up vector.
                    var right = targetRotation * Vector3.right;
                    var forward = Vector3.Cross(right, m_Up);
                    targetRotation = Quaternion.LookRotation(forward, m_Up);
                }
            }

            rotationDelta = CheckRotation(Quaternion.Inverse(rotation) * targetRotation, false);

            // Apply the delta rotation.
            m_DesiredRotation *= rotationDelta;
        }

        /// <summary>
        /// Checks the rotation to ensure the character's colliders won't collide with any other objects.
        /// </summary>
        /// <param name="rotationDelta">The delta to apply to the rotation.</param>
        /// <param name="forceCheck">Should the rotation be force checked? This is used when the character is aligning to the ground.</param>
        /// <returns>A valid rotation delta.</returns>
        public Quaternion CheckRotation(Quaternion rotationDelta, bool forceCheck)
        {
            // The rotation only needs to be checked if a collider could cause a collision when rotating. For example, a vertical CapsuleCollider centered 
            // in the origin doesn't need to be checked because it can't collide with anything else when rotating.
            if (m_CheckRotationCollision && rotationDelta != Quaternion.identity) {
                if (m_ColliderCount > 1) {
                    // Clear the index map to start it off fresh.
                    m_ColliderIndexMap.Clear();
                }

                // There is no "rotation capsule/sphere/box cast" so the collisions must be detected manually. Loop through all of the colliders checking for a collision using
                // the Physics Overlap method. If there is a collision then the penetration must be determined to detect slopes. If the collision is not on a slope then
                // the rotation would overlap another collider so a smaller rotation must be used. Do this for the maximum number of collision checks, with the last one
                // being no rotation at all.
                for (int i = 0; i < m_ColliderCount; ++i) {
                    // The collider doesn't need to be checked if the rotation doesn't cause a change on the relative x or z axis. It will always be checked if the character
                    // is being realigned to the ground because the colliders will always change on the relative x or z axis.
                    if (!m_Colliders[i].gameObject.activeInHierarchy || (!forceCheck && !CanCauseRotationCollision(m_Colliders[i]))) {
                        continue;
                    }

                    // Prevent the character from intersecting with another object while rotating.
                    var targetRotationDelta = rotationDelta;
                    var hitCount = 0;
                    var rigidbodyRotation = m_Rigidbody.rotation;
                    var rigidbodyPosition = m_Rigidbody.position;
                    for (int j = 0; j < m_MaxRotationCollisionChecks; ++j) {
                        // Slerp towards Quaternion.identity which will not add any rotation at all.
                        rotationDelta = Quaternion.Slerp(targetRotationDelta, Quaternion.identity, j / (float)(m_MaxRotationCollisionChecks - 1));
                        // Calculate what the matrix for the child collider will be based on the rotation delta. This is done instead of setting the rotation
                        // direction on the Transform to reduce the calls to the Unity API.
                        var matrix = MathUtility.ApplyRotationToChildMatrices(m_Colliders[i].transform, m_Transform, rotationDelta);
                        // Store the position and rotation from the matrix for future use.
                        var targetPosition = MathUtility.TransformPoint(rigidbodyPosition, rigidbodyRotation, MathUtility.PositionFromMatrix(matrix));
                        var targetRotation = MathUtility.TransformQuaternion(rigidbodyRotation, MathUtility.QuaternionFromMatrix(matrix));

                        var overlap = false;
                        hitCount = OverlapColliders(m_Colliders[i], targetPosition, targetRotation, c_ColliderSpacing);
                        if (hitCount > 0) {
                            Vector3 direction;
                            float distance;

                            // Slightly increase the size of the collider so the penetration test will correctly detect the other collider.
                            if (m_Colliders[i] is CapsuleCollider) {
                                (m_Colliders[i] as CapsuleCollider).radius += c_ColliderSpacing;
                            } else if (m_Colliders[i] is SphereCollider) {
                                (m_Colliders[i] as SphereCollider).radius += c_ColliderSpacing;
                            } else { // BoxCollider.
                                var boxCollider = m_Colliders[i] as BoxCollider;
                                boxCollider.size = boxCollider.size + new Vector3(c_ColliderSpacing, c_ColliderSpacing, c_ColliderSpacing);
                            }

                            // If there is a collision ensure that collision is with a non-sloped object.
                            for (int k = 0; k < hitCount; ++k) {
                                if (Physics.ComputePenetration(m_Colliders[i], targetPosition, targetRotation,
                                    m_OverlapCastResults[k], m_OverlapCastResults[k].transform.position, m_OverlapCastResults[k].transform.rotation, out direction, out distance)) {

                                    // If the hit object is less then the slope limit then the character should rotate with the slope rather then stopping immediately.
                                    var slope = Vector3.Angle((m_Rigidbody.rotation * rotationDelta) * Vector3.up, direction);
                                    if (slope <= m_SlopeLimit) {
                                        continue;
                                    }

                                    overlap = true;
                                    break;
                                }
                            }

                            // The size of the collider has been increased ever so slightly so the collider bounds won't overlap other colliders. If this was not done
                            // the character would be able to rotate in a position that would allow them to move through other colliders.
                            if (m_Colliders[i] is CapsuleCollider) {
                                (m_Colliders[i] as CapsuleCollider).radius -= c_ColliderSpacing;
                            } else if (m_Colliders[i] is SphereCollider) {
                                (m_Colliders[i] as SphereCollider).radius -= c_ColliderSpacing;
                            } else { // BoxCollider.
                                var boxCollider = m_Colliders[i] as BoxCollider;
                                boxCollider.size = boxCollider.size - new Vector3(c_ColliderSpacing, c_ColliderSpacing, c_ColliderSpacing);
                            }
                        }

                        // If there is no overlap then the rotation is valid and can be used.
                        if (!overlap) {
                            break;
                        }
                    }
                }
            }
            return rotationDelta;
        }

        /// <summary>
        /// Performs on overlap cast on the specified collider.
        /// </summary>
        /// <param name="overlapCollider">The collider to check against.</param>
        /// <param name="targetPosition">The position of the collider.</param>
        /// <param name="targetRotation">The rotation of the collider.</param>
        /// <param name="sizeBuffer">The size of the collider that should be modified by when performing the check.</param>
        /// <returns>The number of objects which overlap the collider. These objects will be populated within m_OverlapCastResults.</returns>
        public int OverlapColliders(Collider overlapCollider, Vector3 targetPosition, Quaternion targetRotation, float sizeBuffer = 0)
        {
            int hitCount;
            if (overlapCollider is CapsuleCollider) {
                var capsuleCollider = overlapCollider as CapsuleCollider;
                capsuleCollider.radius += sizeBuffer;
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, targetPosition, targetRotation, out var capsuleEndCap1, out var capsuleEndCap2);
                hitCount = Physics.OverlapCapsuleNonAlloc(capsuleEndCap1, capsuleEndCap2, capsuleCollider.radius * MathUtility.ColliderScaleMultiplier(capsuleCollider),
                                        m_OverlapCastResults, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                capsuleCollider.radius -= sizeBuffer;
            } else if (overlapCollider is SphereCollider) {
                var sphereCollider = overlapCollider as SphereCollider;
                sphereCollider.radius += sizeBuffer;
                hitCount = Physics.OverlapSphereNonAlloc(targetPosition, sphereCollider.radius * MathUtility.ColliderScaleMultiplier(sphereCollider),
                                        m_OverlapCastResults, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                sphereCollider.radius -= sizeBuffer;
            } else { // BoxCollider.
                var boxCollider = overlapCollider as BoxCollider;
                boxCollider.size = boxCollider.size + new Vector3(sizeBuffer, sizeBuffer, sizeBuffer);
                var extents = (MathUtility.ColliderScaleMultiplier(boxCollider) - c_ColliderSpacing) * boxCollider.size / 2;
                hitCount = Physics.OverlapBoxNonAlloc(targetPosition, extents, m_OverlapCastResults, targetRotation, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                boxCollider.size = boxCollider.size - new Vector3(sizeBuffer, sizeBuffer, sizeBuffer);
            }
            return hitCount;
        }

        /// <summary>
        /// Applies the desired rotation to the transform.
        /// </summary>
        protected virtual void ApplyRotation()
        {
            m_Torque = m_DesiredRotation * Quaternion.Inverse(m_MovingPlatformRotation);

            if (m_MovingPlatform != null) {
                var movingPlatformRotation = m_MovingPlatform.rotation;
                m_MovingPlatformRotationOffset = MathUtility.InverseTransformQuaternion(m_Rigidbody.rotation, movingPlatformRotation);
                if (m_AlignToUpDirection) {
                    var localRotation = m_MovingPlatformRotationOffset.eulerAngles;
                    localRotation.x = localRotation.y = 0;
                    m_MovingPlatformRotationOffset.eulerAngles = localRotation;
                }
                m_PrevMovingPlatformRotation = movingPlatformRotation;
            }

            m_Rigidbody.rotation *= m_DesiredRotation;
            m_Transform.rotation = m_Rigidbody.rotation;

            m_RootMotionDeltaRotation = Quaternion.identity;
        }

        /// <summary>
        /// Updates the character's position. The DesiredMovement will be set based on the root motion/input values.
        /// </summary>
        protected virtual void UpdatePosition()
        {
            var frictionValue = 1f;
            // The collider may be destroyed before the grounded check runs again.
            if (m_Grounded && m_GroundedRaycastHit.collider != null) {
                frictionValue = (1 - Mathf.Clamp01(MathUtility.FrictionValue(m_CharacterGroundedCollider.material, m_GroundedRaycastHit.collider.material, true)));
            }
            if (UsingRootMotionPosition) {
                var localDeltaPosition = m_Rigidbody.InverseTransformDirection(m_RootMotionDeltaPosition);
                localDeltaPosition.x *= m_SlopeFactor * frictionValue;
                localDeltaPosition.z *= m_SlopeFactor * frictionValue;
                m_MotorThrottle = m_Rigidbody.TransformDirection(localDeltaPosition) / Time.deltaTime;
            } else {
                // Dampen motor forces.
                m_MotorThrottle /= (1 + m_MotorDamping * m_TimeScale * Time.timeScale);

                // Apply a multiplier if the character is moving backwards.
                var backwardsMultiplier = 1f;
                if (m_InputVector.y < 0) {
                    backwardsMultiplier *= Mathf.Lerp(1, m_MotorBackwardsMultiplier, Mathf.Abs(m_InputVector.y));
                }
                // As the character changes rotation the same local motor throttle force should be applied. This is most apparent when the character is being aligned to the ground
                // and the local y direction changes.
                var prevLocalMotorThrottle = MathUtility.InverseTransformDirection(m_MotorThrottle, m_PrevMotorRotation) * m_PreviousAccelerationInfluence;
                var rotation = Quaternion.Slerp(m_PrevMotorRotation, m_Rigidbody.rotation, m_PreviousAccelerationInfluence);
                var acceleration = m_SlopeFactor * backwardsMultiplier * m_MotorAcceleration * m_TimeScale * Time.timeScale;
                // Convert input into motor forces. Normalize the input vector to prevent the diagonal from moving faster.
                var normalizedInputVector = m_InputVector.normalized * Mathf.Max(Mathf.Abs(m_InputVector.x), Mathf.Abs(m_InputVector.y));
                m_MotorThrottle = MathUtility.TransformDirection(new Vector3(prevLocalMotorThrottle.x + normalizedInputVector.x * acceleration.x,
                                            prevLocalMotorThrottle.y, prevLocalMotorThrottle.z + normalizedInputVector.y * acceleration.z), rotation) * frictionValue;
            }
            m_PrevMotorRotation = m_Rigidbody.rotation;
        }

        /// <summary>
        /// Updates the desired movement value.
        /// </summary>
        protected virtual void UpdateDesiredMovement()
        {
            m_DesiredMovement += (m_MotorThrottle + m_ExternalForce) * Time.deltaTime;

            if (!m_Grounded && UsingGravity) {
                m_GravityAccumulation += m_GravityAmount * m_TimeScale * Time.timeScale * Time.deltaTime;
                m_DesiredMovement += m_GravityDirection * (m_GravityAccumulation * m_TimeScale * Time.timeScale);
            }
        }

        /// <summary>
        /// Updates any external forces.
        /// </summary>
        private void UpdateExternalForces()
        {
            // Apply a soft force (forces applied over several frames).
            if (m_SoftForceFrames[0].sqrMagnitude != 0) {
                AddExternalForce(m_SoftForceFrames[0]);
                for (int i = 0; i < m_MaxSoftForceFrames; ++i) {
                    m_SoftForceFrames[i] = (i < m_MaxSoftForceFrames - 1) ? m_SoftForceFrames[i + 1] : Vector3.zero;
                    if (m_SoftForceFrames[i].sqrMagnitude == 0) {
                        break;
                    }
                }
            }

            // Dampen external forces.
            m_ExternalForce /= (1 + m_ExternalForceDamping * m_TimeScale * Time.timeScale);
        }

        /// <summary>
        /// Check for a horizontal collision.
        /// </summary>
        private void DetectCollisions()
        {
            if (!UsingHorizontalCollisionDetection) {
                return;
            }

            if (m_DesiredMovement.sqrMagnitude == 0) {
                // There may be other objects that intersect with the character even if the character isn't moving.
                if (m_ContinuousCollisionDetection) {
                    for (int i = 0; i < m_ColliderCount; ++i) {
                        if (!m_Colliders[i].gameObject.activeInHierarchy) {
                            continue;
                        }

                        if (OverlapColliders(m_Colliders[i], m_Rigidbody.position + m_Up * c_ColliderSpacing, m_Rigidbody.rotation * m_DesiredRotation, -c_ColliderSpacing * 2) > 0) {
                            if (ResolvePenetrations(m_Colliders[i], Vector3.zero, out var offset)) {
                                var localOffset = m_Rigidbody.InverseTransformDirection(offset);
                                localOffset.y = 0;
                                m_DesiredMovement += m_Rigidbody.TransformDirection(offset);
                            }
                        }

                    }
                }
                if (m_DesiredMovement.sqrMagnitude == 0) {
                    return;
                }
            }

            var targetPosition = m_Rigidbody.position + m_DesiredMovement;
            var lastPosition = m_Rigidbody.position + m_MovingPlatformMovement;
            var iterations = 0;
            bool settled;
            var slideDirection = Vector3.zero;
            var slide = false;
            do {
                settled = true;
                var targetMovement = targetPosition - lastPosition;
                var movementMagnitude = targetMovement.magnitude;
                var normalizedTargetMovement = targetMovement.normalized;
                var hitCount = CombinedCast(lastPosition, normalizedTargetMovement, Mathf.Max(movementMagnitude, c_ColliderSpacing));
                if (hitCount > 0) {
                    for (int i = 0; i < hitCount; ++i) {
                        // Determine which collider caused the intersection.
                        var closestRaycastHit = QuickSelect.SmallestK(m_CombinedCastResults, hitCount, i, m_CastHitComparer);
                        var activeCollider = m_ColliderCount > 1 ? m_Colliders[m_ColliderIndexMap[closestRaycastHit]] : m_Colliders[m_ColliderIndex];

                        if (closestRaycastHit.distance == 0) {
                            if (ResolvePenetrations(activeCollider, targetPosition - m_Rigidbody.position, out var offset)) {
                                // Stop moving if two characters are running directly into each other.
                                if (closestRaycastHit.rigidbody != null && closestRaycastHit.collider.gameObject.GetCachedParentComponent<CharacterLocomotion>() != null) {
                                    if (Vector3.Dot(offset.normalized, normalizedTargetMovement) < -0.99f) {
                                        var localLastPosition = m_Rigidbody.InverseTransformPoint(lastPosition);
                                        localLastPosition.y = m_Rigidbody.InverseTransformPoint(targetPosition).y;
                                        targetPosition = m_Rigidbody.TransformPoint(localLastPosition);
                                        break;
                                    }
                                }

                                // Ground check will handle vertical collisions.
                                if (!UsingGroundCollisionDetection || !UsingVerticalCollisionDetection || Mathf.Abs(Vector3.Dot(m_Up, m_Rigidbody.InverseTransformDirection(closestRaycastHit.normal))) < 0.9995f) {
                                    targetPosition += offset * 0.1f;
                                    settled = false;
                                }
                            } else {
                                // The vertical direction should not be affected.
                                var localLastPosition = m_Rigidbody.InverseTransformPoint(lastPosition);
                                localLastPosition.y = m_Rigidbody.InverseTransformPoint(targetPosition).y;
                                targetPosition = m_Rigidbody.TransformPoint(localLastPosition);
                            }
                            break;
                        }

                        // If the object is directly beneath the character then DetectGround will handle it.
                        if (Vector3.Dot(closestRaycastHit.normal, m_Up) == 1) {
                            continue;
                        }

                        // If the object is a rigidbody then the character should try to push it.
                        PushRigidbody(closestRaycastHit.rigidbody, closestRaycastHit.point, closestRaycastHit.normal, movementMagnitude);

                        // A CapsuleCast/SphereCast normal isn't always the true normal: http://answers.unity3d.com/questions/50825/raycasthitnormal-what-does-it-really-return.html.
                        // Cast a regular raycast in order to determine the true normal.
                        var ray = new Ray(closestRaycastHit.point + normalizedTargetMovement * c_ColliderSpacing + m_Up * (m_MaxStepHeight + c_ColliderSpacing), -m_Up);
                        if (!Physics.Raycast(ray, out var slopeRaycastHit, m_MaxStepHeight + c_ColliderSpacing * 2, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore)) {
                            slopeRaycastHit = closestRaycastHit;
                        }

                        // Only check for steps if the hit point is lower than the max step height.
                        var slopeAngle = Vector3.Angle(m_Up, slopeRaycastHit.normal);
                        var directionalSlope = false;
                        var groundPoint = MathUtility.InverseTransformPoint(m_Rigidbody.position, m_Rigidbody.rotation * m_DesiredRotation, closestRaycastHit.point - m_MovingPlatformMovement);
                        if (groundPoint.y > 0 && (groundPoint.y <= m_MaxStepHeight || groundPoint.y < movementMagnitude)) {
                            if (slopeAngle >= 89.999f) {
                                // Cast a ray directly in front of the character. If it doesn't hit an object then the object is shorter than the step height and should be stepped on.
                                // Continue out of the loop to prevent the character from stopping in front of the object.
                                if (!SingleCast(activeCollider, (targetPosition - m_Rigidbody.position).normalized, m_MovingPlatformMovement + m_Up * (m_MaxStepHeight - c_ColliderSpacing),
                                                                    (m_Radius + movementMagnitude), m_CharacterLayerManager.SolidObjectLayers)) {
                                    continue;
                                }
                            } else if (slopeAngle <= m_SlopeLimit) {
                                directionalSlope = true;
                                m_SlopedGround = true;
                                continue;
                            }
                        }

                        var minDistance = Mathf.Max(Mathf.Min(movementMagnitude, closestRaycastHit.distance - c_ColliderSpacing), 0);
                        // Stop moving if a ceiling is near the character.
                        if (closestRaycastHit.distance < movementMagnitude && Vector3.Dot(closestRaycastHit.normal, -m_Up) > 0.995) {
                            minDistance = 0;
                        } else if (!directionalSlope) {
                            // The character may bounce away from the object. This bounce is applied to the external force so it'll be checked next frame.
                            if (m_WallBounceModifier > 0) {
                                var bouncinessValue = MathUtility.BouncinessValue(activeCollider.material, closestRaycastHit.collider.material);
                                if (bouncinessValue > 0.0f) {
                                    var magnitude = m_DesiredMovement.magnitude * bouncinessValue * m_WallBounceModifier / Time.deltaTime;
                                    AddForce(Vector3.Reflect(normalizedTargetMovement, closestRaycastHit.normal).normalized * magnitude, 1, false);
                                }
                            }

                            // The movement isn't settled until the character has moved the full potential distance. The character can glide across walls and slopes.
                            float movementDot;
                            var projectedNormal = Vector3.ProjectOnPlane(closestRaycastHit.normal, m_Up);
                            if (closestRaycastHit.distance < movementMagnitude && (movementDot = Vector3.Dot(normalizedTargetMovement, projectedNormal)) > -0.99f) {
                                var cross = Vector3.Cross(slopeAngle <= m_SlopeLimit + c_ColliderSpacing ? closestRaycastHit.normal : projectedNormal, normalizedTargetMovement);
                                var forward = Vector3.Cross(cross, closestRaycastHit.normal).normalized;
                                if (forward.sqrMagnitude > 0) {
                                    // Prevent root motion from causing the character to slide in the wrong direction.
                                    if (UsingRootMotionPosition && Vector3.Dot(forward, m_RootMotionDeltaPosition.normalized) < 0.01f) {
                                        forward = Vector3.Cross(Vector3.Cross(closestRaycastHit.normal, normalizedTargetMovement), closestRaycastHit.normal).normalized;
                                    }
                                    // The vertical movement should not change.
                                    var localVerticalPosition = m_Rigidbody.InverseTransformPoint(targetPosition).y;

                                    // Prevent the character from jittering between positions by ensuring the character slides in the same direction.
                                    if (slideDirection.sqrMagnitude > 0 && Vector3.Dot(slideDirection, forward) > 0 && m_Rigidbody.InverseTransformPoint(closestRaycastHit.point).y > 0) {
                                        var localLastPosition = m_Rigidbody.InverseTransformPoint(lastPosition);
                                        targetPosition = m_Rigidbody.TransformPoint(new Vector3(localLastPosition.x, localVerticalPosition, localLastPosition.z));
                                        break;
                                    }
                                    settled = false;
                                    slideDirection = forward;
                                    slide = true;

                                    var dynamicFrictionValue = (1 - Mathf.Clamp01(MathUtility.FrictionValue(activeCollider.material, closestRaycastHit.collider.material, true))) * m_WallFrictionModifier;
                                    lastPosition = (lastPosition + normalizedTargetMovement * minDistance);
                                    targetPosition = lastPosition + (movementMagnitude - closestRaycastHit.distance) * m_WallGlideCurve.Evaluate(1 - movementDot) * dynamicFrictionValue * forward;

                                    // Keep the same vertical movement.
                                    var localTargetPosition = m_Rigidbody.InverseTransformPoint(targetPosition);
                                    targetPosition = m_Rigidbody.TransformPoint(new Vector3(localTargetPosition.x, localVerticalPosition, localTargetPosition.z));

                                    break;
                                }
                            }
                        }

#if UNITY_EDITOR
                        Debug.DrawRay(closestRaycastHit.point, closestRaycastHit.normal, Color.red);
#endif
                        targetPosition = lastPosition + normalizedTargetMovement * minDistance;

                        if (Vector3.Dot(m_ExternalForce.normalized, m_Up) > 0.99f) {
                            // The vertical external force should be cancelled out.
                            var localExternalForce = m_Rigidbody.InverseTransformDirection(m_ExternalForce);
                            localExternalForce.y = 0;
                            m_ExternalForce = m_Rigidbody.TransformDirection(localExternalForce);
                        }

                        // Others may be interested in the collision.
                        if (m_OnCollision != null) {
                            m_OnCollision(closestRaycastHit);
                        }

                        break;
                    }
                }
                iterations++;
            } while (!settled && iterations < m_MaxMovementCollisionChecks);

            m_DesiredMovement = targetPosition - m_Rigidbody.position;

            // If the character is sliding along the wall the resulting vector may cause a collision. Ensure the method doesn't end in a collision state.
            if (slide) {
                var targetDesiredMovement = m_DesiredMovement;
                for (int i = 0; i < m_ColliderCount; ++i) {
                    // The collider may not be enabled.
                    if (!m_Colliders[i].gameObject.activeInHierarchy) {
                        continue;
                    }

                    if (ResolvePenetrations(m_Colliders[i], m_DesiredMovement, out var offset)) {
                        m_DesiredMovement += offset;
                    }
                }

                // Rest the desired movement if the target direction is going in the wrong direction.
                if (Vector3.Dot(m_DesiredMovement.normalized, targetDesiredMovement.normalized) < 0.5f) {
                    m_DesiredMovement = targetDesiredMovement;
                }
            }
        }

        /// <summary>
        /// Returns true if the penetrations have been resolved with the specified collider.
        /// </summary>
        /// <param name="activeCollider">The collider that caused the collision.</param>
        /// <param name="moveDirection">The move direction of the collider.</param>
        /// <param name="offset">The resulting offset of the penetrations.</param>
        /// <returns>True if the penetrations have been resolved with the specified collider.</returns>
        private bool ResolvePenetrations(Collider activeCollider, Vector3 moveDirection, out Vector3 offset)
        {
            offset = Vector3.zero;
            var resolved = false;
            var iterations = 0;

            while (iterations < m_MaxPenetrationChecks && !resolved) {
                resolved = true;
                var hitCount = OverlapColliders(activeCollider, activeCollider.transform.position + moveDirection + offset, activeCollider.transform.rotation);
                if (hitCount > 0) {
                    for (int i = 0; i < hitCount; ++i) {
                        if (Physics.ComputePenetration(activeCollider, activeCollider.transform.position + moveDirection + offset, activeCollider.transform.rotation, m_OverlapCastResults[i],
                            m_OverlapCastResults[i].transform.position, m_OverlapCastResults[i].transform.rotation, out var direction, out var distance)) {
                            offset += direction * (distance + c_ColliderSpacing);
                            resolved = false;
                            break;
                        }
                    }
                }
                iterations++;
            }

            return resolved;
        }

        /// <summary>
        /// Pushes the target Rigidbody in the specified direction.
        /// </summary>
        /// <param name="targetRigidbody">The Rigidbody to push.</param>
        /// <param name="point">The point at which to apply the push force.</param>
        /// <param name="normal">The normal of the hit Rigidbody.</param>
        /// <param name="movementMagnitude">The magnitude of the character's movement.</param>
        private void PushRigidbody(Rigidbody targetRigidbody, Vector3 point, Vector3 normal, float movementMagnitude)
        {
            if (targetRigidbody == null || targetRigidbody.isKinematic) {
                return;
            }

            targetRigidbody.AddForceAtPosition((m_Rigidbody.mass / targetRigidbody.mass) * movementMagnitude * -normal, point);
        }

        /// <summary>
        /// Determine if the character is on the ground.
        /// </summary>
        /// <param name="sendGroundedEvents">Should the events be sent if the grounded status changes?</param>
        private void DetectGroundCollision(bool sendGroundedEvents = true)
        {
            if (!UsingGroundCollisionDetection) {
                return;
            }

            m_GroundedRaycastHit = m_EmptyRaycastHit;
            m_CharacterGroundedCollider = null;

            var grounded = false;
            var rigidbodyRotation = m_Rigidbody.rotation;
            var localDesiredMovement = MathUtility.InverseTransformDirection(m_DesiredMovement - m_MovingPlatformMovement, rigidbodyRotation);
            var localMovingPlatformMovement = MathUtility.InverseTransformDirection(m_MovingPlatformMovement, rigidbodyRotation);
            // The target position should be above the current position to account for slopes.
            var castOffset = m_Radius + m_MaxStepHeight + (m_SlopedGround ? localDesiredMovement.magnitude : 0) + Mathf.Abs(localMovingPlatformMovement.y);
            var targetPosition = m_Rigidbody.position + MathUtility.TransformDirection(
                                new Vector3(localDesiredMovement.x, (localDesiredMovement.y < 0 ? 0 : localDesiredMovement.y) + castOffset, localDesiredMovement.z),
                                rigidbodyRotation) + m_MovingPlatformMovement;
            var stickyGround = StickingToGround && m_Grounded;
            var hitCount = CombinedCast(targetPosition, m_GravityDirection, 
                                        (stickyGround ? m_StickToGroundDistance : 0) +
                                        m_SkinWidth + castOffset + c_ColliderSpacing);
            if (hitCount > 0) {
                for (int i = 0; i < hitCount; ++i) {
                    var closestRaycastHit = QuickSelect.SmallestK(m_CombinedCastResults, hitCount, i, m_CastHitComparer);
                    if (closestRaycastHit.distance == 0) {
                        if (closestRaycastHit.collider.Raycast(new Ray(targetPosition + m_Up * castOffset, m_GravityDirection), out var hit, Mathf.Infinity) &&
                                    (closestRaycastHit.distance == 0 || MathUtility.InverseTransformPoint(m_Rigidbody.position + m_MovingPlatformMovement, m_Rigidbody.rotation * m_MovingPlatformRotation, closestRaycastHit.point).y > m_MaxStepHeight)) {
                            var colliderIndex = 0;
                            if (m_ColliderCount > 1) {
                                colliderIndex = m_ColliderIndexMap[closestRaycastHit];
                            }
                            closestRaycastHit = hit;
                            closestRaycastHit.distance = MathUtility.InverseTransformDirection(targetPosition - hit.point, m_Rigidbody.rotation).y;

                            // The raycast result may already exist if there are multiple m_CombinedCastReults with a 0 distance.
                            if (m_ColliderCount > 1 && !m_ColliderIndexMap.ContainsKey(closestRaycastHit)) {
                                m_ColliderIndexMap.Add(closestRaycastHit, colliderIndex);
                            }
                        } else {
                            continue;
                        }
                    }

                    if (m_Rigidbody.InverseTransformPoint(closestRaycastHit.point).y > m_Radius + m_MaxStepHeight + (m_SlopedGround ? localDesiredMovement.y : 0) + localMovingPlatformMovement.y) {
                        continue;
                    }

                    if (closestRaycastHit.rigidbody != null && !closestRaycastHit.rigidbody.isKinematic) {
                        var groundPoint = MathUtility.InverseTransformPoint(m_Rigidbody.position, rigidbodyRotation, closestRaycastHit.point - m_MovingPlatformMovement);
                        if (groundPoint.y > m_MaxStepHeight) {
                            continue;
                        }
                    }

                    // Determine which collider caused the intersection.
                    var distance = closestRaycastHit.distance - castOffset;

                    // The character is grounded if:
                    // - They should not be force ungrounded OR the desired movement is down.
                    // - The distance to the ground is less than the skin width. If the desired movement is up then the character can still be grounded if the ground is near.
                    grounded = !m_ForceUngrounded && ((Mathf.Abs(localDesiredMovement.y) + (m_StickToGroundDistance + c_ColliderSpacing) > distance) || distance < m_SkinWidth) && (distance + Mathf.Min(localDesiredMovement.y, 0) <= m_SkinWidth || stickyGround);
                    if (grounded) {
                        m_GravityAccumulation = 0;
                        if (!m_MovingPlatformOverride && m_GroundedRaycastHit.transform != closestRaycastHit.transform) {
                            UpdateMovingPlatformTransform(closestRaycastHit.transform);
                        }
                        m_GroundedRaycastHit = closestRaycastHit;
                        var activeCollider = m_ColliderCount > 1 ? m_Colliders[m_ColliderIndexMap[closestRaycastHit]] : m_Colliders[m_ColliderIndex];
                        m_CharacterGroundedCollider = activeCollider;

                        if (UsingVerticalCollisionDetection) {
                            var desiredMagnitude = m_DesiredMovement.magnitude;
                            var bouncinessValue = MathUtility.BouncinessValue(activeCollider.material, closestRaycastHit.collider.material);
                            if (bouncinessValue > 0) {
                                var magnitude = desiredMagnitude * bouncinessValue * m_GroundBounceModifier / Time.deltaTime;
                                AddForce(closestRaycastHit.normal * magnitude, 1, false);
                            }
                            // Do not slide on the ground unless the ground friction material is explicitly set.
                            var frictionValue = (Mathf.Clamp01(MathUtility.FrictionValue(m_CharacterGroundedCollider.material, closestRaycastHit.collider.material, true)));
                            if (frictionValue > 0) {
                                var magnitude = desiredMagnitude * frictionValue * m_GroundFrictionModifier;
                                var direction = Vector3.Cross(Vector3.Cross(closestRaycastHit.normal, -m_Up), closestRaycastHit.normal);
                                AddForce(direction * magnitude, 1, false);
                            }

                            PushRigidbody(closestRaycastHit.rigidbody, closestRaycastHit.point, closestRaycastHit.normal, desiredMagnitude);

                            // The character should be snapped to the ground if the ground is sticky or the character would go through the ground (the distance is negative).
                            localDesiredMovement.y -= (stickyGround || distance < 0 || localDesiredMovement.y - distance < 0) ? (distance + Mathf.Min(localDesiredMovement.y, 0) - c_ColliderSpacing) : 0;
                            m_DesiredMovement = MathUtility.TransformDirection(localDesiredMovement, rigidbodyRotation) + m_MovingPlatformMovement;

                            // Others may be interested in the collision.
                            if (m_OnCollision != null) {
                                m_OnCollision(closestRaycastHit);
                            }
                        }
                    }
                    break;
                }
            }

            if (UpdateGroundState(grounded, sendGroundedEvents)) {
                if (grounded) {
                    if (!m_MovingPlatformOverride) {
                        UpdateMovingPlatformTransform(m_GroundedRaycastHit.transform);
                    }
                    // The vertical external force should be cancelled out.
                    if (!m_ForceUngrounded) {
                        var localExternalForce = m_Rigidbody.InverseTransformDirection(m_ExternalForce);
                        if (localExternalForce.y < 0) {
                            localExternalForce.y = 0;
                            m_ExternalForce = m_Rigidbody.TransformDirection(localExternalForce);
                        }
                    }
                } else {
                    m_ForceUngrounded = false;

                    if (m_MovingPlatform != null && !m_MovingPlatformOverride) {
                        UpdateMovingPlatformTransform(null);
                    }
                }
            }
            m_SlopedGround = false;
            UpdateSlopeFactor();
        }

        /// <summary>
        /// Updates the grounded state.
        /// </summary>
        /// <param name="grounded">Is the character grounded?</param>
        /// <param name="sendGroundedEvents">Should the events be sent if the grounded status changes?</param>
        /// <returns>True if the grounded state changed.</returns>
        protected virtual bool UpdateGroundState(bool grounded, bool sendGroundedEvents)
        {
            // Update the grounded state. Allows for cleanup when the character hits the ground or moves into the air.
            if (m_Grounded != grounded) {
                m_Grounded = grounded;
                if (m_Grounded) {
                    m_ApplyMovingPlatformDisconnectMovement = false;
                    m_MovingPlatformMovement = m_MovingPlatformDisconnectMovement = Vector3.zero;
                    m_MovingPlatformRotation = m_MovingPlatformDisconnectRotation = Quaternion.identity;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the slope factor. This gives the option of slowing the character down while moving up a slope or increasing the speed while moving down.
        /// </summary>
        protected virtual void UpdateSlopeFactor()
        {
            // The character isn't on a slope while in the air or not moving.
            if (!m_AdjustMotorForceOnSlope || !m_Grounded || m_MotorThrottle.sqrMagnitude == 0 || !UsingVerticalCollisionDetection) {
                m_SlopeFactor = 1;
                return;
            }

            // Determine if the slope is uphill or downhill.
            m_SlopeFactor = 1 + (1 - (Vector3.Angle(m_GroundedRaycastHit.normal, m_MotorThrottle.normalized) / 90));

            if (Mathf.Abs(1 - m_SlopeFactor) < 0.01f) { // Moving on flat ground or moving perpendicular to a slope.
                m_SlopeFactor = 1;
            } else if (m_SlopeFactor > 1) { // Downhill.
                m_SlopeFactor = m_MotorSlopeForceDown / m_SlopeFactor;
            } else { // Uphill.
                m_SlopeFactor *= m_MotorSlopeForceUp;
            }
        }

        /// <summary>
        /// Casts a ray in the specified direction. If the character has multiple colliders added then a ray will be cast from each collider.
        /// A CapsuleCast, SphereCast or BoxCast is used depending on the type of collider that has been added. The result is stored in the m_CombinedCastResults array.
        /// </summary>
        /// <param name="position">The position that the cast should be started from.</param>
        /// <param name="direction">The direction of the cast. This value should be normalized.</param>
        /// <param name="distance">The distance of the cast.</param>
        /// <param name="drawDebugLine">Should the debug lines be drawn?</param>
        /// <returns>The number of objects hit from the cast.</returns>
        private int CombinedCast(Vector3 position, Vector3 direction, float distance
#if UNITY_EDITOR
            , bool drawDebugLine = false
#endif
            )
        {
            if (m_ColliderCount > 1) {
                // Clear the index map to start it off fresh.
                m_ColliderIndexMap.Clear();
            }

            var rigidbodyRotation = m_Rigidbody.rotation * m_MovingPlatformRotation;
            var positionOffset = MathUtility.InverseTransformPoint(m_Rigidbody.position, rigidbodyRotation, position);
            var hitCount = 0;
            for (int i = 0; i < m_ColliderCount; ++i) {
                // The collider may not be enabled.
                if (!m_Colliders[i].gameObject.activeInHierarchy) {
                    continue;
                }

                int localHitCount;
                var colliderTransform = m_Colliders[i].transform;
                var castPosition = MathUtility.TransformPoint(colliderTransform.position, rigidbodyRotation, positionOffset);
                // Determine if the collider would intersect any objects.
                if (m_Colliders[i] is CapsuleCollider) {
                    var capsuleCollider = m_Colliders[i] as CapsuleCollider;
                    MathUtility.CapsuleColliderEndCaps(capsuleCollider, castPosition, colliderTransform.rotation, out var capsuleEndCap1, out var capsuleEndCap2);
                    var radius = capsuleCollider.radius * MathUtility.ColliderScaleMultiplier(capsuleCollider) - c_ColliderSpacing;
                    localHitCount = Physics.CapsuleCastNonAlloc(capsuleEndCap1, capsuleEndCap2, radius, direction, m_CastResults, distance, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
#if UNITY_EDITOR
                    if (drawDebugLine) {
                        Debug.DrawLine(capsuleEndCap1, capsuleEndCap2, Color.green);
                        Debug.DrawRay(capsuleEndCap1, direction * Mathf.Min(distance, 100), Color.blue);
                    }
#endif
                } else if (m_Colliders[i] is SphereCollider) {
                    var sphereCollider = m_Colliders[i] as SphereCollider;
                    var radius = sphereCollider.radius * MathUtility.ColliderScaleMultiplier(sphereCollider) - c_ColliderSpacing;
                    localHitCount = Physics.SphereCastNonAlloc(castPosition + colliderTransform.TransformDirection(sphereCollider.center), radius, direction,
                                                                    m_CastResults, distance, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
#if UNITY_EDITOR
                    if (drawDebugLine) {
                        Debug.DrawRay(position + sphereCollider.transform.TransformDirection(sphereCollider.center) + positionOffset, direction * Mathf.Min(distance, 100), Color.blue);
                    }
#endif
                } else { // BoxCollider.
                    var boxCollider = m_Colliders[i] as BoxCollider;
                    var extents = (MathUtility.ColliderScaleMultiplier(boxCollider) - c_ColliderSpacing) * boxCollider.size / 2;
                    localHitCount = Physics.BoxCastNonAlloc(castPosition + colliderTransform.TransformDirection(boxCollider.center), extents, direction,
                                                                    m_CastResults, colliderTransform.rotation, distance, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
#if UNITY_EDITOR
                    if (drawDebugLine) {
                        Debug.DrawRay(position + boxCollider.transform.TransformDirection(boxCollider.center) + positionOffset, direction * Mathf.Min(distance, 100), Color.blue);
                    }
#endif
                }

                if (localHitCount > 0) {
                    // The mapping needs to be saved if there are multiple colliders.
                    if (m_ColliderCount > 1) {
                        int validHitCount = 0;
                        for (int j = 0; j < localHitCount; ++j) {
                            if (m_ColliderIndexMap.ContainsKey(m_CastResults[j])) {
                                continue;
                            }
                            // Ensure the array is large enough.
                            if (hitCount + validHitCount >= m_CombinedCastResults.Length) {
                                Debug.LogWarning("Warning: The maximum number of collisions has been reached. Consider increasing the CharacterLocomotion MaxCollisionCount value.");
                                continue;
                            }

                            m_ColliderIndexMap.Add(m_CastResults[j], i);
                            m_CombinedCastResults[hitCount + validHitCount] = m_CastResults[j];
                            validHitCount += 1;
                        }
                        hitCount += validHitCount;
                    } else {
                        m_CombinedCastResults = m_CastResults;
                        hitCount += localHitCount;
                        m_ColliderIndex = i;
                    }
                }
            }

            return hitCount;
        }

        /// <summary>
        /// Casts a ray using in the specified direction.  A CapsuleCast or SphereCast is used depending on the type of collider that has been added.
        /// The result is stored in the m_RaycastHit object.
        /// </summary>
        /// <param name="castCollider">The collider which is performing the cast.</param>
        /// <param name="direction">The direction to perform the cast.</param>
        /// <param name="offset">Any offset to apply to the cast.</param>
        /// <param name="distance">The distance of the cast.</param>
        /// <param name="layers">The layers to perform the cast on.</param>
        /// <returns>Did the cast hit an object?</returns>
        protected bool SingleCast(Collider castCollider, Vector3 direction, Vector3 offset, float distance, int layers
#if UNITY_EDITOR
            , bool drawDebugLine = false
#endif
            )
        {
            return SingleCast(castCollider, direction, offset, distance, layers, out var raycastHit
#if UNITY_EDITOR
                , drawDebugLine
#endif
                );
        }

        /// <summary>
        /// Casts a ray using in the specified direction.  A CapsuleCast or SphereCast is used depending on the type of collider that has been added.
        /// The result is stored in the m_RaycastHit object.
        /// </summary>
        /// <param name="castCollider">The collider which is performing the cast.</param>
        /// <param name="direction">The direction to perform the cast.</param>
        /// <param name="offset">Any offset to apply to the cast.</param>
        /// <param name="distance">The distance of the cast.</param>
        /// <param name="layers">The layers to perform the cast on.</param>
        /// <param name="raycastHit">The resulting RaycastHit.</param>
        /// <returns>Did the cast hit an object?</returns>
        protected bool SingleCast(Collider castCollider, Vector3 direction, Vector3 offset, float distance, int layers, out RaycastHit raycastHit
#if UNITY_EDITOR
            , bool drawDebugLine = false
#endif
            )
        {
            // Determine if the collider would intersect any objects.
            bool hit;
            if (castCollider is CapsuleCollider) {
                Vector3 capsuleEndCap1, capsuleEndCap2;
                var capsuleCollider = castCollider as CapsuleCollider;
                var colliderTransform = capsuleCollider.transform;
                MathUtility.CapsuleColliderEndCaps(capsuleCollider, colliderTransform.position + offset, colliderTransform.rotation, out capsuleEndCap1, out capsuleEndCap2);
                var radius = capsuleCollider.radius * MathUtility.ColliderScaleMultiplier(capsuleCollider) - c_ColliderSpacing;
                hit = Physics.CapsuleCast(capsuleEndCap1, capsuleEndCap2, radius, direction.normalized, out raycastHit, distance + c_ColliderSpacing, layers, QueryTriggerInteraction.Ignore);
#if UNITY_EDITOR
                if (drawDebugLine) {
                    Debug.DrawLine(capsuleEndCap1, capsuleEndCap2, Color.green);
                    Debug.DrawRay(capsuleEndCap1, direction.normalized * Mathf.Min(distance, 100), Color.blue);
                }
#endif
                return hit;
            } else if (castCollider is SphereCollider) {
                var sphereCollider = castCollider as SphereCollider;
                var radius = sphereCollider.radius * MathUtility.ColliderScaleMultiplier(sphereCollider) - c_ColliderSpacing;
                hit = Physics.SphereCast(sphereCollider.transform.TransformPoint(sphereCollider.center) + offset, radius, direction.normalized,
                                                                out raycastHit, distance + c_ColliderSpacing, layers, QueryTriggerInteraction.Ignore);
#if UNITY_EDITOR
                if (drawDebugLine) {
                    Debug.DrawRay(sphereCollider.transform.TransformPoint(sphereCollider.center) + offset, direction.normalized * Mathf.Min(distance, 100), Color.blue);
                }
#endif
            } else { // BoxCollider.
                var boxCollider = castCollider as BoxCollider;
                var extents = (MathUtility.ColliderScaleMultiplier(boxCollider) - ColliderSpacing) * boxCollider.size / 2;
                hit = Physics.BoxCast(boxCollider.transform.TransformPoint(boxCollider.center) + offset, extents, direction, out raycastHit,
                                                            boxCollider.transform.rotation, distance + ColliderSpacing, layers, QueryTriggerInteraction.Ignore);
#if UNITY_EDITOR
                if (drawDebugLine) {
                    Debug.DrawRay(boxCollider.transform.TransformPoint(boxCollider.center) + offset, direction.normalized * Mathf.Min(distance, 100), Color.blue);
                }
#endif
            }
            return hit;
        }

        /// <summary>
        /// Applies the desired movement to the transform.
        /// </summary>
        protected virtual void ApplyPosition()
        {
            m_Velocity = (m_DesiredMovement - m_MovingPlatformMovement) / Time.deltaTime;
            m_Rigidbody.position += m_DesiredMovement;
            m_Transform.position = m_Rigidbody.position;

            if (m_MovingPlatform != null) {
                m_MovingPlatformRelativePosition = m_MovingPlatform.InverseTransformPoint(m_Rigidbody.position);
            }

            // Cancel out the vertical external force if the character changes directions.
            if (LocalDesiredMovement.y > 0) {
                var localExternalForce = m_Transform.InverseTransformDirection(m_ExternalForce);
                if (localExternalForce.y < 0) {
                    localExternalForce.y = 0;
                    m_ExternalForce = m_Transform.TransformDirection(localExternalForce);
                }
            }

            m_RootMotionDeltaPosition = Vector3.zero;
        }

        /// <summary>
        /// Updates the root motion position and rotation.
        /// </summary>
        /// <param name="deltaPosition">The delta root motion position.</param>
        /// <param name="deltaRotation">The delta root motion rotation.</param>
        public virtual void UpdateRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
        {
            m_RootMotionDeltaPosition += deltaPosition * m_RootMotionSpeedMultiplier;
            m_RootMotionDeltaRotation *= deltaRotation;
            m_RootMotionDeltaRotation.ToAngleAxis(out var angle, out var axis);
            angle *= m_RootMotionRotationMultiplier;
            m_RootMotionDeltaRotation = Quaternion.AngleAxis(angle, axis);
        }

        /// <summary>
        /// When the character changes grounded state the moving platform should also be updated. This
        /// allows the character to always reference the correct moving platform (if one exists at all).
        /// </summary>
        /// <param name="hitTransform">The name of the possible moving platform transform.</param>
        /// <returns>True if the platform changed.</returns>
        private bool UpdateMovingPlatformTransform(Transform hitTransform)
        {
            // Update the moving platform if on the ground and the ground transform is a moving platform.
            if (hitTransform != null) {
                // The character may not be on the ground if the character is teleported to a location that overlaps the moving platform.
                if (hitTransform.gameObject.layer == LayerManager.MovingPlatform) {
                    if (hitTransform != m_MovingPlatform) {
                        SetMovingPlatform(hitTransform, false);
                        return true;
                    }
                    return false;
                } else if (m_MovingPlatform != null) {
                    SetMovingPlatform(null, false);
                    return true;
                }
            } else if (m_MovingPlatform != null && hitTransform == null) { // The character is no longer on a moving platform.
                SetMovingPlatform(null, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the moving platform to the specified transform.
        /// </summary>
        /// <param name="movingPlatform">The platform transform that should be set. Can be null.</param>
        /// <param name="platformOverride">Is the default moving platform logic being overridden?</param>
        /// <returns>True if the platform was changed.</returns>
        public virtual bool SetMovingPlatform(Transform movingPlatform, bool platformOverride = true)
        {
            if (m_MovingPlatform == movingPlatform) {
                return false;
            }

            m_MovingPlatform = movingPlatform;
            m_MovingPlatformOverride = m_MovingPlatform != null && platformOverride;
            if (m_MovingPlatform != null) {
                m_MovingPlatformRelativePosition = m_MovingPlatform.InverseTransformPoint(m_Rigidbody.position);
                var movingPlatformRotation = m_MovingPlatform.rotation;
                m_MovingPlatformRotationOffset = MathUtility.InverseTransformQuaternion(m_Rigidbody.rotation, movingPlatformRotation);
                if (m_AlignToUpDirection) {
                    var localRotation = m_MovingPlatformRotationOffset.eulerAngles;
                    localRotation.x = localRotation.y = 0;
                    m_MovingPlatformRotationOffset.eulerAngles = localRotation;
                }

                m_PrevMovingPlatformRotation = m_MovingPlatform.rotation;
            } else {
                m_MovingPlatformDisconnectMovement = GetMovingPlatformDisconnectMovement();
                m_MovingPlatformDisconnectRotation = GetMovingPlatformDisconnectRotation();
                m_MovingPlatformMovement = Vector3.zero;
                m_MovingPlatformRotation = Quaternion.identity;
                m_ApplyMovingPlatformDisconnectMovement = true;
            }
            return true;
        }

        /// <summary>
        /// Returns the movement when the character is disconnecting from the platform.
        /// </summary>
        /// <returns>The movement when the character is disconnecting from the platform.</returns>
        protected virtual Vector3 GetMovingPlatformDisconnectMovement()
        {
            return m_MovingPlatformMovement;
        }

        /// <summary>
        /// Returns the rotation when the character is disconnecting from the platform.
        /// </summary>
        /// <returns>The rotation when the character is disconnecting from the platform.</returns>
        protected virtual Quaternion GetMovingPlatformDisconnectRotation()
        {
            return m_MovingPlatformRotation;
        }

        /// <summary>
        /// Adds a force to the character in the specified number of frames. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="scaleByMass">Should the force be scaled by the character's mass?</param>
        public void AddForce(Vector3 force, int frames = 1, bool scaleByMass = true)
        {
            if (force.sqrMagnitude == 0) {
                return;
            }

            if (scaleByMass) {
                force /= m_Rigidbody.mass;
            }
            m_ForceUngrounded = m_Grounded && m_Rigidbody.InverseTransformDirection(force).y > 0;
            if (frames > 1) {
                AddSoftForce(force, frames);
            } else {
                AddExternalForce(force);
            }
        }

        /// <summary>
        /// Adds an external force to add.
        /// </summary>
        /// <param name="force">The force to add.</param>
        private void AddExternalForce(Vector3 force)
        {
            m_ExternalForce += force;
        }

        /// <summary>
        /// Adds a soft force to the character. A soft force is spread out through up to c_MaxSoftForceFrames frames.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        private void AddSoftForce(Vector3 force, float frames)
        {
            frames = Mathf.Clamp(frames, 1, m_MaxSoftForceFrames);
            AddExternalForce(force / frames);
            for (int i = 0; i < (Mathf.RoundToInt(frames) - 1); i++) {
                m_SoftForceFrames[i] += (force / frames);
            }
        }

        /// <summary>
        /// Adds a force relative to the character in the specified number of frames. The force will either be an external or soft force.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="frames">The number of frames to add the force to.</param>
        /// <param name="scaleByMass">Should the force be scaled by the character's mass?</param>
        public void AddRelativeForce(Vector3 force, int frames = 1, bool scaleByMass = true)
        {
            // Convert the force into a relative force.
            AddForce(m_Rigidbody.InverseTransformDirection(force), frames, scaleByMass);
        }

        /// <summary>
        /// If the collision layer is disabled then all of the character's colliders will be set to an IgnoreRaycast layer. This
        /// prevents any CapsuleCast or SphereCasts from returning a collider added to the character itself.
        /// </summary>
        /// <param name="enable">Should the layers be enabled?</param>
        public void EnableColliderCollisionLayer(bool enable)
        {
            // Protect against duplicate enabled values changing the collider layer.
            if (m_CollisionLayerEnabled == enable) {
                return;
            }
            m_CollisionLayerEnabled = enable;

            if (enable) {
                for (int i = 0; i < m_ColliderCount; ++i) {
                    m_ColliderGameObjects[i].layer = m_ColliderLayers[i];
                }
                for (int i = 0; i < m_IgnoredColliderCount; ++i) {
                    m_IgnoredColliderGameObjects[i].layer = m_IgnoredColliderLayers[i];
                }
            } else {
                for (int i = 0; i < m_ColliderCount; ++i) {
                    m_ColliderLayers[i] = m_ColliderGameObjects[i].layer;
                    m_ColliderGameObjects[i].layer = LayerManager.IgnoreRaycast;
                }
                for (int i = 0; i < m_IgnoredColliderCount; ++i) {
                    m_IgnoredColliderLayers[i] = m_IgnoredColliderGameObjects[i].layer;
                    m_IgnoredColliderGameObjects[i].layer = LayerManager.IgnoreRaycast;
                }
            }
        }

        /// <summary>
        /// Sets the layer of the colliders.
        /// </summary>
        /// <param name="layer">The layer that should be set.</param>
        public void SetCollisionLayer(int layer)
        {
            for (int i = 0; i < m_ColliderCount; ++i) {
                m_ColliderLayers[i] = m_ColliderGameObjects[i].layer = layer;
            }
        }

        /// <summary>
        /// Adds a collider to the existing collider array.
        /// </summary>
        /// <param name="addCollider">The collider that should be added to the array.</param>
        public void AddCollider(Collider addCollider)
        {
            m_ColliderCount = AddCollider(addCollider, ref m_Colliders, ref m_ColliderLayers, ref m_ColliderGameObjects, m_ColliderCount);
            if (m_ColliderCount > 0 && m_ColliderIndexMap == null && m_Colliders.Length > 1) {
                m_ColliderIndexMap = new Dictionary<RaycastHit, int>(new UnityEngineUtility.RaycastHitEqualityComparer());
            }
        }

        /// <summary>
        /// Adds a collider to the existing collider array.
        /// </summary>
        /// <param name="addCollider">The collider that should be added to the array.</param>
        /// <param name="existingColliders">An array of colliders that the colliders array should be added to.</param>
        /// <param name="existingColliderLayers">An array of existing collider layers that may need to be resized.</param>
        /// <param name="existingColliderGameObjects">An array of existing collider GameObjects that should be updated.</param>
        /// <param name="existingColliderCount">The count of the existing colliders array.</param>
        /// <returns>The new count of the existing colliders array.</returns>
        private int AddCollider(Collider addCollider, ref Collider[] existingColliders, ref int[] existingColliderLayers, ref GameObject[] existingColliderGameObjects, int existingColliderCount)
        {
            // Don't add an already added collider.
            for (int i = 0; i < existingColliderCount; ++i) {
                if (existingColliders[i] == addCollider) {
                    return existingColliderCount;
                }
            }

            // The collider should be added.
            if (existingColliderCount == existingColliders.Length) {
                Array.Resize(ref existingColliders, existingColliders.Length + 1);
                Array.Resize(ref existingColliderLayers, existingColliderLayers.Length + 1);
                Array.Resize(ref existingColliderGameObjects, existingColliderGameObjects.Length + 1);
            }
            existingColliders[existingColliderCount] = addCollider;
            existingColliderGameObjects[existingColliderCount] = addCollider.gameObject;
            if (!m_CollisionLayerEnabled) {
                existingColliderLayers[existingColliderCount] = existingColliderGameObjects[existingColliderCount].layer;
                existingColliderGameObjects[existingColliderCount].layer = LayerManager.IgnoreRaycast;
            }
            return existingColliderCount + 1;
        }

        /// <summary>
        /// Removes the specified collider from the collider array.
        /// </summary>
        /// <param name="removeCollider">The collider which should be removed from the array.</param>
        public void RemoveCollider(Collider removeCollider)
        {
            m_ColliderCount = RemoveCollider(removeCollider, ref m_Colliders, ref m_ColliderLayers, ref m_ColliderGameObjects, m_ColliderCount);
        }

        /// <summary>
        /// Removes the specified collider from the collider array.
        /// </summary>
        /// <param name="removeCollider">The collider which should be removed from the array.</param>
        /// <param name="existingColliders">An array of colliders that the colliders array should be removed from.</param>
        /// <param name="existingColliderLayers">An array of existing collider layers that may need to be resized.</param>
        /// <param name="existingColliderGameObjects">An array of existing collider GameObjects that should be updated.</param>
        /// <param name="existingColliderCount">The count of the existing colliders array.</param>
        /// <returns>The number of colliders within the existing colliders array.</returns>
        public int RemoveCollider(Collider removeCollider, ref Collider[] existingColliders, ref int[] existingColliderLayers, ref GameObject[] existingColliderGameObjects, int existingColliderCount)
        {
            for (int i = existingColliders.Length - 1; i > -1; --i) {
                if (existingColliders[i] != removeCollider) {
                    continue;
                }
                // The collider may be removed when collisions are disabled. The layer should be reverted back to its original.
                if (!m_CollisionLayerEnabled) {
                    existingColliderGameObjects[i].layer = existingColliderLayers[i];
                }

                // Do not resize the array for performance reasons. Move all of the next colliders back a slot instead.
                for (int j = i; j < existingColliderCount - 1; ++j) {
                    existingColliders[j] = existingColliders[j + 1];
                    existingColliderLayers[j] = existingColliderLayers[j + 1];
                    existingColliderGameObjects[j] = existingColliderGameObjects[j + 1];
                }
                existingColliderCount--;
                existingColliders[i] = null;
                existingColliderGameObjects[i] = null;
            }
            return existingColliderCount;
        }

        /// <summary>
        /// Adds an array to the collider array.
        /// </summary>
        /// <param name="colliders">The colliders which should be added to the array.</param>
        public void AddColliders(Collider[] colliders)
        {
            m_ColliderCount = AddColliders(colliders, ref m_Colliders, ref m_ColliderLayers, ref m_ColliderGameObjects, m_ColliderCount);
            if (m_ColliderIndexMap == null && m_Colliders.Length > 1) {
                m_ColliderIndexMap = new Dictionary<RaycastHit, int>(new UnityEngineUtility.RaycastHitEqualityComparer());
            }
        }

        /// <summary>
        /// Adds the colliders array to the existing colliders array. The existing colliders array length will be resized if the new
        /// set of colliders won't fit.
        /// </summary>
        /// <param name="colliders">The colliders that should be added to the existing colliders.</param>
        /// <param name="existingColliders">An array of colliders that the colliders array should be added to.</param>
        /// <param name="existingColliderLayers">An array of existing collider layers that may need to be resized.</param>
        /// <param name="existingColliderGameObjects">An array of existing collider GameObjects that should be updated.</param>
        /// <param name="existingColliderCount">The count of the existing colliders array.</param>
        /// <returns>The new count of the existing colliders array.</returns>
        private int AddColliders(Collider[] colliders, ref Collider[] existingColliders, ref int[] existingColliderLayers, ref GameObject[] existingColliderGameObjects, int existingColliderCount)
        {
            // The array may need to be increased with the new colliders.
            if (existingColliders.Length < existingColliderCount + colliders.Length) {
                var diff = (existingColliderCount + colliders.Length) - existingColliders.Length;
                Array.Resize(ref existingColliders, existingColliders.Length + diff);
                Array.Resize(ref existingColliderLayers, existingColliderLayers.Length + diff);
                Array.Resize(ref existingColliderGameObjects, existingColliderGameObjects.Length + diff);
            }
            var startCount = existingColliderCount;
            for (int i = 0; i < colliders.Length; ++i) {
                if (colliders[i] == null) {
                    continue;
                }

                // Don't add an already added collider.
                var addCollider = true;
                for (int j = 0; j < startCount; ++j) {
                    if (colliders[i] == existingColliders[j]) {
                        addCollider = false;
                        break;
                    }
                }

                // The collider is new - add it to the array.
                if (addCollider) {
                    existingColliders[existingColliderCount] = colliders[i];
                    existingColliderGameObjects[existingColliderCount] = colliders[i].gameObject;
                    if (!m_CollisionLayerEnabled) {
                        existingColliderLayers[existingColliderCount] = existingColliderGameObjects[existingColliderCount].layer;
                        existingColliderGameObjects[existingColliderCount].layer = LayerManager.IgnoreRaycast;
                    }
                    existingColliderCount++;
                }
            }

            // Return the new collider count.
            return existingColliderCount;
        }

        /// <summary>
        /// Removes the specified colliders from the collider array.
        /// </summary>
        /// <param name="colliders">The colliders that should be removed.</param>
        public void RemoveColliders(Collider[] colliders)
        {
            m_ColliderCount = RemoveColliders(colliders, ref m_Colliders, ref m_ColliderLayers, ref m_ColliderGameObjects, m_ColliderCount);
        }

        /// <summary>
        /// Removes the colliders from the collider array.
        /// </summary>
        /// <param name="colliders">The colliders that should be removed.</param>
        /// <param name="existingColliders">An array of colliders that the colliders array should be removed from.</param>
        /// <param name="existingColliderLayers">An array of existing collider layers that may need to be resized.</param>
        /// <param name="existingColliderGameObjects">An array of existing collider GameObjects that should be updated.</param>
        /// <param name="existingColliderCount">The count of the existing colliders array.</param>
        /// <returns>The number of colliders within the existing colliders array.</returns>
        private int RemoveColliders(Collider[] colliders, ref Collider[] existingColliders, ref int[] existingColliderLayers, ref GameObject[] existingColliderGameObjects, int existingColliderCount)
        {
            for (int i = existingColliderCount - 1; i > -1; --i) {
                for (int j = colliders.Length - 1; j > -1; --j) {
                    if (existingColliders[i] != colliders[j]) {
                        continue;
                    }
                    // The collider may be removed when collisions are disabled. The layer should be reverted back to its original.
                    if (!m_CollisionLayerEnabled) {
                        existingColliderGameObjects[i].layer = existingColliderLayers[i];
                    }

                    // Do not resize the array for performance reasons. Move all of the next colliders back a slot instead.
                    for (int k = i; k < existingColliderCount - 1; ++k) {
                        existingColliders[k] = existingColliders[k + 1];
                        existingColliderLayers[k] = existingColliderLayers[k + 1];
                        existingColliderGameObjects[k] = existingColliderGameObjects[k + 1];
                    }
                    existingColliderCount--;
                    existingColliders[existingColliderCount] = null;
                    existingColliderGameObjects[existingColliderCount] = null;
                    break;
                }
            }
            return existingColliderCount;
        }

        /// <summary>
        /// Adds an element to the ignored collider array.
        /// </summary>
        /// <param name="addCollider">The collider which should be added to the array.</param>
        public void AddIgnoredCollider(Collider addCollider)
        {
            m_IgnoredColliderCount = AddCollider(addCollider, ref m_IgnoredColliders, ref m_IgnoredColliderLayers, ref m_IgnoredColliderGameObjects, m_IgnoredColliderCount);
        }

        /// <summary>
        /// Removes the specified collider from the ignored collider array.
        /// </summary>
        /// <param name="removeCollider">The collider which should be removed from the array.</param>
        public void RemoveIgnoredCollider(Collider removeCollider)
        {
            m_IgnoredColliderCount = RemoveCollider(removeCollider, ref m_IgnoredColliders, ref m_IgnoredColliderLayers, ref m_IgnoredColliderGameObjects, m_IgnoredColliderCount);
        }

        /// <summary>
        /// Adds an array to the ignored collider array.
        /// </summary>
        /// <param name="colliders">The colliders which should be added to the array.</param>
        public void AddIgnoredColliders(Collider[] colliders)
        {
            m_IgnoredColliderCount = AddColliders(colliders, ref m_IgnoredColliders, ref m_IgnoredColliderLayers, ref m_IgnoredColliderGameObjects, m_IgnoredColliderCount);
        }

        /// <summary>
        /// Removes the specified colliders from the ignored collider array.
        /// </summary>
        /// <param name="colliders">The colliders that should be removed.</param>
        public void RemoveIgnoredColliders(Collider[] colliders)
        {
            m_IgnoredColliderCount = RemoveColliders(colliders, ref m_IgnoredColliders, ref m_IgnoredColliderLayers, ref m_IgnoredColliderGameObjects, m_IgnoredColliderCount);
        }

        /// <summary>
        /// Sets the rotation of the character.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        public virtual void SetRotation(Quaternion rotation)
        {
            m_Transform.rotation = m_Rigidbody.rotation = m_PrevMotorRotation = rotation;
            m_Up = rotation * Vector3.up;
            m_SlopedGround = m_ApplyMovingPlatformDisconnectMovement = false;

            SimulationManager.SetCharacterRotation(m_SimulationIndex, rotation);
        }

        /// <summary>
        /// Sets the position of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        public virtual void SetPosition(Vector3 position)
        {
            m_Transform.position = m_Rigidbody.position = position;
            m_MotorThrottle = m_DesiredMovement = m_ExternalForce = Vector3.zero;
            m_GravityAccumulation = 0;
            Grounded = false;
            m_SlopedGround = m_ApplyMovingPlatformDisconnectMovement = false;

            EnableColliderCollisionLayer(false);
            DetectGroundCollision(false);
            EnableColliderCollisionLayer(true);

            SimulationManager.SetCharacterPosition(m_SimulationIndex, position);
        }

        /// <summary>
        /// Resets the rotation and position to their default values.
        /// </summary>
        public virtual void ResetRotationPosition()
        {
            m_PrevMotorRotation = m_Rigidbody.rotation;
            m_Up = m_Rigidbody.rotation * Vector3.up;
            m_GravityDirection = -m_Up;
            m_DesiredRotation = Quaternion.identity;
            m_MotorThrottle = m_DesiredMovement = m_ExternalForce = Vector3.zero;
            m_SlopedGround = m_ApplyMovingPlatformDisconnectMovement = false;

            ResetRootMotion();
        }

        /// <summary>
        /// Resets the root motion delta values.
        /// </summary>
        public void ResetRootMotion()
        {
            m_RootMotionDeltaPosition = Vector3.zero;
            m_RootMotionDeltaRotation = Quaternion.identity;
        }

        /// <summary>
        /// The character has been disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            SimulationManager.UnregisterCharacter(m_SimulationIndex);
        }
    }
}