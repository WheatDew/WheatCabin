/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Motion;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Allows the character to stand on uneven surfaces and rotates and positions the character's limbs to face in the look direction.
    /// </summary>
    public class CharacterIK : CharacterIKBase
    {
#if UNITY_EDITOR
        [Tooltip("Draw a debug line to see the direction that the character is facing (editor only).")]
        [SerializeField] protected bool m_DebugDrawLookRay;
#endif
        [Tooltip("The index of the base layer within the Animator Controller.")]
        [SerializeField] protected int m_BaseLayerIndex;
        [Tooltip("The index of the upper body layer within the Animator Controller.")]
        [SerializeField] protected int m_UpperBodyLayerIndex = 4;
        [Tooltip("The layers that the component should use when determining the objects to test against.")]
        [SerializeField] protected LayerMask m_LayerMask = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.TransparentFX | 1 << LayerManager.Overlay | 
                                                                1 << LayerManager.Water | 1 << LayerManager.UI | 1 << LayerManager.VisualEffect |
                                                                1 << LayerManager.SubCharacter | 1 << LayerManager.Character);
        [Tooltip("An offset to apply to the look at direction for the body and arms.")]
        [SerializeField] protected Vector3 m_LookAtOffset;
        [Shared.Utility.InspectorFoldout("Body")]
        [Tooltip("Determines how much weight is applied to the body when looking at the target. (0-1).")]
        [Range(0, 1)] [SerializeField] protected float m_LookAtBodyWeight = 0.05f;
        [Tooltip("Determines how much weight is applied to the head when looking at the target. (0-1).")]
        [Range(0, 1)] [SerializeField] protected float m_LookAtHeadWeight = 0.425f;
        [Tooltip("Determines how much weight is applied to the eyes when looking at the target. (0-1).")]
        [Range(0, 1)] [SerializeField] protected float m_LookAtEyesWeight = 1;
        [Tooltip("A value of 0 means the character is completely unrestrained in motion, 1 means the character motion completely clamped (look at becomes impossible) (0-1).")]
        [Range(0, 1)] [SerializeField] protected float m_LookAtClampWeight = 0.35f;
        [Tooltip("The adjustment speed to apply to the Look At position.")]
        [SerializeField] protected float m_LookAtAdjustmentSpeed = 0.5f;
        [Tooltip("The name of the state when the character is actively looking at a target.")]
        [SerializeField] protected string m_ActiveLookAtStateName = "LookAt";
        [Tooltip("The speed at which the look at weight should adjust.")]
        [SerializeField] protected float m_LookAtWeightAdjustmentSpeed = 0.2f;
        [Tooltip("The speed at which the hips position should adjust to using IK and not using IK.")]
        [SerializeField] protected float m_HipsPositionAdjustmentSpeed = 4;
        [Tooltip("An extra offset applied to the hips position.")]
        [SerializeField] protected float m_HipsPositionOffset;
        [Shared.Utility.InspectorFoldout("Feet")]
        [Tooltip("The offset of the foot between the foot bone and the base of the foot.")]
        [SerializeField] protected float m_FootOffsetAdjustment = 0.005f;
        [Tooltip("The speed at which the foot weight should adjust to when foot IK is active.")]
        [SerializeField] protected float m_FootWeightActiveAdjustmentSpeed = 10;
        [Tooltip("The speed at which the foot weight should adjust to when foot IK is inactive.")]
        [SerializeField] protected float m_FootWeightInactiveAdjustmentSpeed = 2;
        [Tooltip("The overridden weight for foot IK. Set to -1 to disable.")]
        [SerializeField] protected float m_OverrideFootIKWeight = -1f;
        [Shared.Utility.InspectorFoldout("Upper Arm")]
        [Tooltip("Determines how much weight is applied to the upper arms when looking at the target (0-1).")]
        [Range(0, 1)] [SerializeField] protected float m_UpperArmWeight = 1;
        [Tooltip("The speed at which the upper arm rotation should adjust to using IK and not using IK.")]
        [SerializeField] protected float m_UpperArmAdjustmentSpeed = 10;
        [Shared.Utility.InspectorFoldout("Hands")]
        [Tooltip("Determines how much weight is applied to the hands when looking at the target (0-1).")]
        [Range(0, 1)] [SerializeField] protected float m_HandWeight = 1;
        [Tooltip("The speed at which the hand position/rotation should adjust to using IK and not using IK.")]
        [SerializeField] protected float m_HandAdjustmentSpeed = 10;
        [Tooltip("Specifies a local offset to add to the position of the hands.")]
        [SerializeField] protected Vector3 m_HandPositionOffset;
        [Tooltip("The left hand positional spring used for IK movement.")]
        [SerializeField] protected Spring m_LeftHandPositionSpring = new Spring();
        [Tooltip("The left hand rotational spring used for IK movement.")]
        [SerializeField] protected Spring m_LeftHandRotationSpring = new Spring(0.2f, 0.05f);
        [Tooltip("The right hand positional spring used for IK movement.")]
        [SerializeField] protected Spring m_RightHandPositionSpring = new Spring();
        [Tooltip("The right hand rotational spring used for IK movement.")]
        [SerializeField] protected Spring m_RightHandRotationSpring = new Spring(0.2f, 0.05f);

        public LayerMask LayerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }
        public Vector3 LookAtOffset { get { return m_LookAtOffset; } set { m_LookAtOffset = value; } }
        public float LookAtBodyWeight { get { return m_LookAtBodyWeight; } set { m_LookAtBodyWeight = value; } }
        public float LookAtHeadWeight { get { return m_LookAtHeadWeight; } set { m_LookAtHeadWeight = value; } }
        public float LookAtEyesWeight { get { return m_LookAtEyesWeight; } set { m_LookAtEyesWeight = value; } }
        public float LookAtClampWeight { get { return m_LookAtClampWeight; } set { m_LookAtClampWeight = value; } }
        public float LookAtAdjustmentSpeed { get { return m_LookAtAdjustmentSpeed; } set { m_LookAtAdjustmentSpeed = value; } }
        public string ActiveLookAtStateName { get { return m_ActiveLookAtStateName; } set { m_ActiveLookAtStateName = value; } }
        public float LookAtWeightAdjustmentSpeed { get { return m_LookAtWeightAdjustmentSpeed; } set { m_LookAtWeightAdjustmentSpeed = value; } }
        public float HipsPositionAdjustmentSpeed
        {
            get { return m_HipsPositionAdjustmentSpeed; }
            set {
                m_HipsPositionAdjustmentSpeed = value;
                if (m_HipsPositionAdjustmentSpeed == 0) { m_HipsOffset = 0; }
            }
        }
        public float FootOffsetAdjustment { get { return m_FootOffsetAdjustment; } set { m_FootOffsetAdjustment = value; } }
        public float FootWeightActiveAdjustmentSpeed { get { return m_FootWeightActiveAdjustmentSpeed; } set { m_FootWeightActiveAdjustmentSpeed = value; } }
        public float FootWeightInactiveAdjustmentSpeed { get { return m_FootWeightInactiveAdjustmentSpeed; } set { m_FootWeightInactiveAdjustmentSpeed = value; } }
        public float OverrideFootIKWeight { get { return m_OverrideFootIKWeight; } set { m_OverrideFootIKWeight = value; } }
        public float UpperArmWeight { get { return m_UpperArmWeight; } set { m_UpperArmWeight = value; } }
        public float UpperArmAdjustmentSpeed { get { return m_UpperArmAdjustmentSpeed; } set { m_UpperArmAdjustmentSpeed = value; } }
        public float HandWeight { get { return m_HandWeight; } set { m_HandWeight = value; } }
        public float HandAdjustmentSpeed { get { return m_HandAdjustmentSpeed; } set { m_HandAdjustmentSpeed = value; } }
        public Vector3 HandPositionOffset { get { return m_HandPositionOffset; } set { m_HandPositionOffset = value; } }
        public Spring LeftHandPositionSpring
        {
            get { return m_LeftHandPositionSpring; }
            set {
                m_LeftHandPositionSpring = value;
                if (m_LeftHandPositionSpring != null) { m_LeftHandPositionSpring.Initialize(false, false); }
            }
        }
        public Spring LeftHandRotationSpring
        {
            get { return m_LeftHandRotationSpring; }
            set {
                m_LeftHandRotationSpring = value;
                if (m_LeftHandRotationSpring != null) { m_LeftHandRotationSpring.Initialize(true, false); }
            }
        }
        public Spring RightHandPositionSpring
        {
            get { return m_RightHandPositionSpring; }
            set {
                m_RightHandPositionSpring = value;
                if (m_RightHandPositionSpring != null) { m_RightHandPositionSpring.Initialize(false, false); }
            }
        }
        public Spring RightHandRotationSpring
        {
            get { return m_RightHandPositionSpring; }
            set {
                m_RightHandPositionSpring = value;
                if (m_RightHandPositionSpring != null) { m_RightHandPositionSpring.Initialize(true, false); }
            }
        }

        private Transform m_Transform;
        private Animator m_Animator;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private GameObject m_CharacterGameObject;
        private ILookSource m_LookSource;
        private InventoryBase m_Inventory;
        private RaycastHit m_RaycastHit;

        private Transform m_Head;
        private Transform m_Hips;
        private Transform m_LeftFoot;
        private Transform m_RightFoot;
        private Transform m_LeftToes;
        private Transform m_RightToes;
        private Transform m_LeftLowerLeg;
        private Transform m_RightLowerLeg;
        private Transform m_LeftHand;
        private Transform m_RightHand;
        private Transform m_LeftUpperArm;
        private Transform m_RightUpperArm;

        private bool m_ImmediatePosition;
        private bool m_IKPass;
        private bool m_Aiming;
        private bool m_ItemInUse;

        private Vector3 m_LookAtPosition;
        private float m_LookAtBodyIKWeight;
        private float m_LookAtHeadIKWeight;
        private float m_LookAtEyesIKWeight;

        private Vector3 m_LastCharacterPosition;
        private Vector3 m_CharacterMovement;

        private Vector3 m_HipsPosition;
        private float m_HipsOffset;
        private float[] m_FootOffset = new float[2];
        private float[] m_FootIKWeight = new float[2];
        private float[] m_MaxLegLength = new float[2];
        private float[] m_RaycastDistance = new float[2];
        private float[] m_GroundDistance = new float[2];
        private Vector3[] m_GroundPoint = new Vector3[2];
        private Vector3[] m_GroundNormal = new Vector3[2];

        private float[] m_HandRotationIKWeight = new float[2];
        private float[] m_HandPositionIKWeight = new float[2];
        private int[] m_HandSlotID = new int[2];
        private Transform m_DominantHand;
        private Transform m_NonDominantHand;

        private float m_DominantUpperArmWeight;
        private Transform m_DominantUpperArm;
        private Vector3 m_DominantHandPosition;
        private Vector3 m_NonDominantHandOffset;
        private Vector3 m_NonDominantHandPosition;
        private int m_DominantSlotID;
        private Vector3 m_HandOffset;
        private bool m_Unequipping;

        private Transform m_LeftHandItemIKTarget;
        private Transform m_RightHandItemIKTarget;
        private Transform m_LeftHandItemIKHintTarget;
        private Transform m_RightHandItemIKHintTarget;

        private bool m_ActiveLookAtTarget;
        private Vector3 m_LookAtTargetPosition;
        private Transform[] m_IKTarget;
        private Transform[] m_AbilityIKTarget;
        private Transform[] m_InterpolationTarget;
        private float[] m_StartInterpolation;
        private float[] m_InterpolationDuration;
        private bool m_InterpolateIKTargets;
        private bool m_RequireSecondHandPositioning;

        private Vector3 m_PrevLeftHandPositionSpringValue;
        private Vector3 m_PrevLeftHandPositionSpringVelocity;
        private Vector3 m_PrevLeftHandRotationSpringValue;
        private Vector3 m_PrevLeftHandRotationSpringVelocity;
        private Vector3 m_PrevRightHandPositionSpringValue;
        private Vector3 m_PrevRightHandPositionSpringVelocity;
        private Vector3 m_PrevRightHandRotationSpringValue;
        private Vector3 m_PrevRightHandRotationSpringVelocity;

        private bool m_Enable;

        public override bool UseOnAnimatorIK => true;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_Animator = gameObject.GetCachedComponent<Animator>();
            m_CharacterLocomotion = gameObject.GetComponentInParent<UltimateCharacterLocomotion>();
            m_CharacterGameObject = m_CharacterLocomotion.gameObject;
            m_Transform = m_CharacterLocomotion.transform;

            if (m_Animator == null) {
                Debug.LogError("The Character Animator could not be found.", gameObject);
                return;
            }

            InitializeBones(true);

            // Initialize the target ik arrays.
            var count = (int)IKGoal.Last;
            m_IKTarget = new Transform[count];
            m_AbilityIKTarget = new Transform[count];
            m_InterpolationTarget = new Transform[count];
            m_StartInterpolation = new float[count];
            m_InterpolationDuration = new float[count];
            for (int i = 0; i < m_StartInterpolation.Length; ++i) {
                m_StartInterpolation[i] = -1;
            }

            m_LeftHandPositionSpring.Initialize(false, false);
            m_LeftHandRotationSpring.Initialize(true, false);
            m_RightHandPositionSpring.Initialize(false, false);
            m_RightHandRotationSpring.Initialize(true, false);

            EventHandler.RegisterEvent<ILookSource>(m_CharacterGameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.RegisterEvent<CharacterItem, int>(m_CharacterGameObject, "OnInventoryEquipItem", OnEquipItem);
            EventHandler.RegisterEvent<CharacterItem, int>(m_CharacterGameObject, "OnInventoryUnequipItem", OnUnequipItem);
            EventHandler.RegisterEvent<CharacterItem, int>(m_CharacterGameObject, "OnInventoryRemoveItem", OnUnequipItem);
            EventHandler.RegisterEvent<bool>(m_CharacterGameObject, "OnAimAbilityAim", OnAim);
            EventHandler.RegisterEvent<bool, Abilities.Items.Use>(m_CharacterGameObject, "OnUseAbilityStart", OnUseStart);
            EventHandler.RegisterEvent<int, Vector3, Vector3, bool>(m_CharacterGameObject, "OnAddSecondaryForce", OnAddForce);
            EventHandler.RegisterEvent(m_CharacterGameObject, "OnAnimatorWillSnap", ImmediatePosition);
            EventHandler.RegisterEvent(m_CharacterGameObject, "OnAnimatorSnapped", AnimatorSnapped);
            EventHandler.RegisterEvent(m_CharacterGameObject, "OnCameraImmediatePosition", ImmediatePosition);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_CharacterGameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_CharacterGameObject, "OnRespawn", OnRespawn);

            m_Enable = enabled;
            // Disable the component until the LookSource has been attached.
            enabled = false;
        }

        /// <summary>
        /// Initializes the humanoid bones.
        /// </summary>
        /// <param name="fromAwake">Are the bones being initalized from awake?</param>
        /// <returns>True if the bones aare initialized.</returns>
        private bool InitializeBones(bool fromAwake)
        {
            if (m_Head != null && !fromAwake) {
                return true;
            }

            // Assign the humanoid limbs. If the character is not a humanoid then the component will stay disabled because Unity's IK system
            // only works with humanoids.
            if (!m_Animator.isHuman) {
                if (!fromAwake) {
                    Debug.LogError("Error: The CharacterIK component only works with humanoid models.");
                }
                return false;
            }

            m_Head = m_Animator.GetBoneTransform(HumanBodyBones.Head);
            if (m_Head == null) {
                if (!fromAwake) {
                    Debug.LogError($"Error: The Head bone is not assigned to the character {gameObject.name}.");
                }
                return false;
            }
            m_Hips = m_Animator.GetBoneTransform(HumanBodyBones.Hips);
            m_LeftFoot = m_Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            m_RightFoot = m_Animator.GetBoneTransform(HumanBodyBones.RightFoot);
            if (m_LeftFoot == null || m_RightFoot == null) {
                if (!fromAwake) {
                    Debug.LogError($"Error: The Left or Right foot bone is not assigned to the character {gameObject.name}.");
                }
                return false;
            }
            m_LeftToes = m_Animator.GetBoneTransform(HumanBodyBones.LeftToes);
            m_RightToes = m_Animator.GetBoneTransform(HumanBodyBones.RightToes);
            m_LeftLowerLeg = m_Animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            m_RightLowerLeg = m_Animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            m_LeftHand = m_Animator.GetBoneTransform(HumanBodyBones.LeftHand);
            m_RightHand = m_Animator.GetBoneTransform(HumanBodyBones.RightHand);
            if (m_LeftHand == null || m_RightHand == null) {
                if (!fromAwake) {
                    Debug.LogError($"Error: The Left or Right hand bone is not assigned to the character {gameObject.name}.");
                }
                return false;
            }
            m_LeftUpperArm = m_Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            m_RightUpperArm = m_Animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

            // The limbs should snap into position at the start.
            m_ImmediatePosition = true;
            m_IKPass = false;
            m_LastCharacterPosition = m_Transform.position;

            // Perform measurements during initialization while in a T-Pose so they can be compared against during the IK pass.
            for (int i = 0; i < 2; ++i) {
                var foot = i == 0 ? m_LeftFoot : m_RightFoot;
                m_FootOffset[i] = m_Transform.InverseTransformPoint(foot.position).y - m_FootOffsetAdjustment;
                m_MaxLegLength[i] = m_Transform.InverseTransformPoint(i == 0 ? m_LeftLowerLeg.position : m_RightLowerLeg.position).y - m_FootOffsetAdjustment;
            }
            m_HipsPosition = m_Transform.InverseTransformPoint(m_Hips.position);

            // The slot IDs can be populated programmatically by finding a reference to the ItemSlot component.
            var itemSlot = m_LeftHand.GetComponentInChildren<CharacterItemSlot>();
            if (itemSlot != null) {
                m_HandSlotID[0] = itemSlot.ID;
            }
            itemSlot = m_RightHand.GetComponentInChildren<CharacterItemSlot>();
            if (itemSlot != null) {
                m_HandSlotID[1] = itemSlot.ID;
            }

            return true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Implement start so the enable/disable toggle appears up in the inspector.
        /// </summary>
        private void Start() { }
#endif

        /// <summary>
        /// The component has been enabled.
        /// </summary>
        private void OnEnable()
        {
            m_LastCharacterPosition = m_Transform.position;
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            var prevEnabled = enabled;
            m_LookSource = lookSource;
            enabled = InitializeBones(false) && m_LookSource != null;
            if (m_LookSource != null) {
                m_LookAtPosition = GetDefaultLookAtPosition();
                m_LookAtPosition = m_Head.position + (m_LookAtPosition - m_Head.position).normalized;
            }
            // If the component is enabled update the animator so OnAnimatorIK will run.
            if (enabled && !prevEnabled && m_Animator.isActiveAndEnabled) {
                m_Animator.Update(0);
            }
        }

        /// <summary>
        /// Sets the target that the character should look at.
        /// </summary>
        /// <param name="active">Should the character look at the target position?</param>
        /// <param name="position">The position that the character should look at.</param>
        public override void SetLookAtPosition(bool active, Vector3 position)
        {
            if (active) {
                m_LookAtTargetPosition = position;
            }
            if (m_ActiveLookAtTarget != active) {
                if (!string.IsNullOrEmpty(m_ActiveLookAtStateName)) {
                    Shared.StateSystem.StateManager.SetState(m_CharacterGameObject, m_ActiveLookAtStateName, active);
                }
                m_ActiveLookAtTarget = active;
            }
        }

        /// <summary>
        /// Returns the default position that the character should look at.
        /// </summary>
        /// <returns>The default position that the character should look at.</returns>
        public override Vector3 GetDefaultLookAtPosition()
        {
            var headPosition = m_Head.position;
            var lookDirection = m_LookSource.LookDirection(headPosition, false, 0, true, false);
            // Multiply the local offset by the distance so the same relative offset will be applied for both the upper body and head.
            var localOffset = m_LookAtOffset * m_LookSource.LookDirectionDistance;
            localOffset.z += m_LookSource.LookDirectionDistance;
            return MathUtility.TransformPoint(headPosition, Quaternion.LookRotation(lookDirection), localOffset);
        }

        /// <summary>
        /// Specifies the location of the left or right hand IK target and IK hint target.
        /// </summary>
        /// <param name="itemTransform">The transform of the item.</param>
        /// <param name="itemHand">The hand that the item is parented to.</param>
        /// <param name="nonDominantHandTarget">The target of the left or right hand. Can be null.</param>
        /// <param name="nonDominantHandElbowTarget">The target of the left or right elbow. Can be null.</param>
        public override void SetItemIKTargets(Transform itemTransform, Transform itemHand, Transform nonDominantHandTarget, Transform nonDominantHandElbowTarget)
        {
            if (itemHand == null) {
                return;
            }

            // If the item is parented to the right hand, then the left hand should use the IK target (and visa-versa).
            if (itemHand.IsChildOf(m_RightHand) || itemHand == m_RightHand) {
                m_LeftHandItemIKTarget = nonDominantHandTarget;
                m_LeftHandItemIKHintTarget = nonDominantHandElbowTarget;
            } else {
                m_RightHandItemIKTarget = nonDominantHandTarget;
                m_RightHandItemIKHintTarget = nonDominantHandElbowTarget;
            }

            UpdateIKTargets();
        }

        /// <summary>
        /// Specifies the target location of the limb.
        /// </summary>
        /// <param name="target">The target location of the limb.</param>
        /// <param name="ikGoal">The limb affected by the target location.</param>
        /// <param name="duration">The amount of time it takes to reach the goal.</param>
        public override void SetAbilityIKTarget(Transform target, IKGoal ikGoal, float duration)
        {
            if (m_InterpolationTarget[(int)ikGoal] == null) {
                var interpTarget = new GameObject("IKInterpolation" + ikGoal);
                m_InterpolationTarget[(int)ikGoal] = interpTarget.transform;
                m_InterpolationTarget[(int)ikGoal].SetParentOrigin(m_Transform);
            }
            m_StartInterpolation[(int)ikGoal] = Time.time;
            m_InterpolationDuration[(int)ikGoal] = duration;

            SetAbilityIKTarget(target, ikGoal);
        }

        /// <summary>
        /// Specifies the target location of the limb.
        /// </summary>
        /// <param name="target">The target location of the limb.</param>
        /// <param name="ikGoal">The limb affected by the target location.</param>
        private void SetAbilityIKTarget(Transform target, IKGoal ikGoal)
        {
            m_AbilityIKTarget[(int)ikGoal] = target;
            m_InterpolateIKTargets = true;
            UpdateIKTargets();
        }

        /// <summary>
        /// Updates the IK target references.
        /// </summary>
        private void UpdateIKTargets()
        {
            // The ability ik targets override the item ik targets.
            m_IKTarget[(int)IKGoal.LeftHand] = m_StartInterpolation[(int)IKGoal.LeftHand] != -1 ? m_InterpolationTarget[(int)IKGoal.LeftHand] :
                                                        (m_AbilityIKTarget[(int)IKGoal.LeftHand] != null ? m_AbilityIKTarget[(int)IKGoal.LeftHand] : m_LeftHandItemIKTarget);
            m_IKTarget[(int)IKGoal.LeftElbow] = m_StartInterpolation[(int)IKGoal.LeftElbow] != -1 ? m_InterpolationTarget[(int)IKGoal.LeftElbow] :
                                                        (m_AbilityIKTarget[(int)IKGoal.LeftElbow] != null ? m_AbilityIKTarget[(int)IKGoal.LeftElbow] : m_LeftHandItemIKHintTarget);
            m_IKTarget[(int)IKGoal.RightHand] = m_StartInterpolation[(int)IKGoal.RightHand] != -1 ? m_InterpolationTarget[(int)IKGoal.RightHand] :
                                                        (m_AbilityIKTarget[(int)IKGoal.RightHand] != null ? m_AbilityIKTarget[(int)IKGoal.RightHand] : m_RightHandItemIKTarget);
            m_IKTarget[(int)IKGoal.RightElbow] = m_StartInterpolation[(int)IKGoal.RightElbow] != -1 ? m_InterpolationTarget[(int)IKGoal.RightElbow] :
                                                        (m_AbilityIKTarget[(int)IKGoal.RightElbow] != null ? m_AbilityIKTarget[(int)IKGoal.RightElbow] : m_RightHandItemIKHintTarget);
            for (int i = 0; i < (int)IKGoal.LeftFoot; ++i) {
                m_RequireSecondHandPositioning = m_IKTarget[i] != null;
                if (m_RequireSecondHandPositioning) {
                    break;
                }
            }

            // The feet targets are not affected by items so they can easily be iterated.
            for (int i = (int)IKGoal.LeftFoot; i < (int)IKGoal.Last; ++i) {
                m_IKTarget[i] = m_StartInterpolation[i] != -1 ? m_InterpolationTarget[i] : m_AbilityIKTarget[i];
            }
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="characterItem">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(CharacterItem characterItem, int slotID)
        {
            DetermineDominantHand();
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="characterItem">The item that was unequipped.</param>
        /// <param name="slotID">The slot that the item was unequipped from.</param>
        private void OnUnequipItem(CharacterItem characterItem, int slotID)
        {
            DetermineDominantHand();
        }

        /// <summary>
        /// An item was equipepd or unequipped. Determine the new dominant hand.
        /// </summary>
        private void DetermineDominantHand()
        {
            if (m_Inventory == null) {
                m_Inventory = m_CharacterGameObject.GetCachedComponent<InventoryBase>();
            }
            CharacterItem dominantCharacterItem = null;
            for (int i = 0; i < m_Inventory.SlotCount; ++i) {
                var item = m_Inventory.GetActiveCharacterItem(i);
                if (item != null && item.DominantItem) {
                    dominantCharacterItem = item;
                    break;
                }
            }

            // The hands should act independently if there are no items.
            if (dominantCharacterItem == null) {
                m_Unequipping = true;
                // Do not reset the variables immediately - the upper arm weight first needs to interpolate back to 0 for a smooth unequip.
                if (m_DominantUpperArmWeight == 0) {
                    m_DominantHand = null;
                    m_NonDominantHand = null;
                    m_DominantUpperArm = null;
                    m_DominantSlotID = -1;
                }
            } else {
                m_Unequipping = false;
                if (dominantCharacterItem.SlotID == m_HandSlotID[0]) { // Left Hand.
                    m_DominantHand = m_LeftHand;
                    m_NonDominantHand = m_RightHand;
                    m_DominantUpperArm = m_LeftUpperArm;
                    m_DominantSlotID = dominantCharacterItem.SlotID;
                } else if (dominantCharacterItem.SlotID == m_HandSlotID[1]) {  // Right Hand.
                    m_DominantHand = m_RightHand;
                    m_NonDominantHand = m_LeftHand;
                    m_DominantUpperArm = m_RightUpperArm;
                    m_DominantSlotID = dominantCharacterItem.SlotID;
                }
            }
        }

        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="aim">Has the Aim ability started?</param>
        private void OnAim(bool aim)
        {
            m_Aiming = aim;
        }

        /// <summary>
        /// The Use ability has started or stopped using an item.
        /// </summary>
        /// <param name="start">Has the Use ability started?</param>
        /// <param name="useAbility">The Use ability that has started or stopped.</param>
        private void OnUseStart(bool start, Abilities.Items.Use useAbility)
        {
            if (useAbility.SlotID == -1 || useAbility.SlotID == m_DominantSlotID) {
                m_ItemInUse = start;
            }
        }

        /// <summary>
        /// Adds a positional and rotational force to the ik target.
        /// </summary>
        /// <param name="slotID">The Slot ID that is adding the secondary force.</param>
        /// <param name="positionalForce">The positional force to add.</param>
        /// <param name="rotationalForce">The rotational force to add.</param>
        /// <param name="globalForce">Is the force applied to the entire character?</param>
        private void OnAddForce(int slotID, Vector3 positionalForce, Vector3 rotationalForce, bool globalForce)
        {
            if (globalForce || slotID == m_HandSlotID[0]) { // Left Hand.
                m_LeftHandPositionSpring.AddForce(positionalForce);
                m_LeftHandRotationSpring.AddForce(rotationalForce);
            }

            if (globalForce || slotID == m_HandSlotID[1]) { // Right Hand.
                m_RightHandPositionSpring.AddForce(positionalForce);
                m_RightHandRotationSpring.AddForce(rotationalForce);
            }
        }

        /// <summary>
        /// Update the IK position and weights.
        /// </summary>
        /// <param name="layerIndex">The animator layer that is affected by IK.</param>
        private void OnAnimatorIK(int layerIndex)
        {
            UpdateSolvers(layerIndex);
        }

        /// <summary>
        /// Updates the IK solvers.
        /// </summary>
        /// <param name="index">The index of the animation layer.</param>
        public override void UpdateSolvers(int layerIndex)
        {
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);

            if (layerIndex == m_BaseLayerIndex) { // Base layer.
                m_CharacterMovement = m_Transform.position - m_LastCharacterPosition;
                m_LastCharacterPosition = m_Transform.position;
                m_IKPass = true;

                // Any target interpolations should first be updated before ik is run.
                UpdateTargetInterpolations();
                // Position the legs to stand on the ground.
                PositionLowerBody();
                // The upper body should look in the direction of the LookSource.
                LookAtTarget();
            } else if (layerIndex == m_UpperBodyLayerIndex) { // Upper body.
                // If the character is aiming the hands should be rotated towards the target.
                RotateHands();
                // The upper arms should look in the direction of the target.
                RotateUpperArms();
                // If the character is aiming the hands should be positioned towards the target.
                PositionHands();
            } else if (m_RequireSecondHandPositioning) { // Full body layer.
                // If an IK target is set the hands need to be positioned again so they match the upper body rotation.
                PositionHands();
            }

            m_CharacterLocomotion.EnableColliderCollisionLayer(true);
        }

        /// <summary>
        /// Positions the lower body so the legs are always on the ground.
        /// </summary>
        private void PositionLowerBody()
        {
            var hipsOffset = m_CharacterLocomotion.ColliderSpacing;
            if (m_OverrideFootIKWeight > 0 || (m_CharacterLocomotion.Grounded && m_CharacterLocomotion.UsingVerticalCollisionDetection)) {
                // There are two passes for positioning the feet. The hips need to be positioned first and then the feet can be positioned.
                for (int i = 0; i < 2; ++i) {
                    // If a foot ik target is set then the feet are positioned manually.
                    if (m_IKTarget[(int)(i == 0 ? IKGoal.LeftFoot : IKGoal.RightFoot)] != null) {
                        m_GroundDistance[i] = float.MaxValue;
                        continue;
                    }

                    // Fire the first raycast from the foot.
                    var target = (i == 0 ? m_LeftFoot : m_RightFoot);
                    var lowerLeg = (i == 0 ? m_LeftLowerLeg : m_RightLowerLeg);
                    if (Physics.Raycast(GetFootRaycastPosition(target, lowerLeg, out var distance), -m_CharacterLocomotion.Up, out m_RaycastHit,
                                            distance + m_FootOffset[i] + m_MaxLegLength[i], m_LayerMask, QueryTriggerInteraction.Ignore) &&
                                            m_Transform.InverseTransformPoint(m_RaycastHit.point).y < m_CharacterLocomotion.Radius) {
                        m_RaycastDistance[i] = distance * m_Transform.lossyScale.y;
                        m_GroundDistance[i] = m_RaycastHit.distance;
                        m_GroundPoint[i] = m_RaycastHit.point;
                        m_GroundNormal[i] = m_RaycastHit.normal;
                    } else {
                        m_GroundDistance[i] = float.MaxValue;
                    }

                    // Fire the second raycast from the toe. If a closer object is hit then the toe raycast results should be used. This prevent the toe from clipping objects
                    // if the object isn't at the same height as the foot.
                    target = (i == 0 ? m_LeftToes : m_RightToes);
                    if (target != null && Physics.Raycast(GetFootRaycastPosition(target, lowerLeg, out distance), -m_CharacterLocomotion.Up, out m_RaycastHit,
                                                            distance + m_FootOffset[i] + m_MaxLegLength[i], m_LayerMask, QueryTriggerInteraction.Ignore) &&
                                                            m_Transform.InverseTransformPoint(m_RaycastHit.point).y < m_CharacterLocomotion.Radius) {
                        // In addition to checking the distance also ensure the normal is the same as the up direction as the character. This will prevent the toes from
                        // positioning the IK while on a slope.
                        if (m_RaycastHit.distance + m_CharacterLocomotion.ColliderSpacing < m_GroundDistance[i] && m_RaycastHit.normal == m_CharacterLocomotion.Up) {
                            m_RaycastDistance[i] = distance * m_Transform.lossyScale.y;
                            m_GroundDistance[i] = m_RaycastHit.distance;
                            m_GroundPoint[i] = m_RaycastHit.point;
                            m_GroundNormal[i] = m_RaycastHit.normal;
                        }
                    }

                    if (m_GroundDistance[i] != float.MaxValue) {
                        // If the foot is at the same relative height then the hip offset should be set. This is most useful when the character is standing on uneven ground.
                        // As an example, imagine that the character is standing on a set of stairs. The stairs have two sets of colliders: one collider which covers each step 
                        // and another plane collider at the same slope as the stairs. The character’s collider is going to be resting on the plane collider while standing on the 
                        // stairs and the IK system will be trying to ensure the feet are resting on the stairs collider. In some cases the plane collider may be relatively far 
                        // above the stair collider so the hip needs to be moved down to allow the character’s foot to hit the stair collider.
                        float offset;
                        var foot = (i == 0 ? m_LeftFoot : m_RightFoot);
                        if ((offset = m_GroundDistance[i] - m_RaycastDistance[i] - m_Transform.InverseTransformPoint(foot.position).y) > hipsOffset) {
                            hipsOffset = offset;
                        }
                    }
                }
            }

            // Smoothly position the hips.
            m_HipsOffset = m_ImmediatePosition ? hipsOffset : Mathf.Lerp(m_HipsOffset, hipsOffset, m_HipsPositionAdjustmentSpeed * Time.deltaTime);
            m_HipsPosition = m_Transform.InverseTransformPoint(m_Hips.position);
            m_HipsPosition.y -= m_HipsOffset + m_HipsPositionOffset;

            // Move the feet into the correct position/rotation.
            for (int i = 0; i < 2; ++i) {
                var ikGoal = (i == 0 ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot);
                var position = m_Animator.GetIKPosition(ikGoal);
                var rotation = m_Animator.GetIKRotation(ikGoal);
                var targetWeight = m_OverrideFootIKWeight != -1 ? m_OverrideFootIKWeight : 0f;

                var adjustmentSpeed = m_FootWeightInactiveAdjustmentSpeed;
                Transform target;
                // If an IK target is specified then the target should be used.
                if ((target = m_IKTarget[(int)(i == 0 ? IKGoal.LeftFoot : IKGoal.RightFoot)]) != null) {
                    position = target.position;
                    rotation = target.rotation;
                    targetWeight = 1f;
                    adjustmentSpeed = m_FootWeightActiveAdjustmentSpeed;
                } else {
                    // Determine the position and rotation of the foot if on the ground.
                    if (m_CharacterLocomotion.Grounded) {
                        // IK should only be used if the foot position would be underneath the ground position.
                        if (m_GroundDistance[i] != float.MaxValue && m_GroundDistance[i] > 0 && m_Transform.InverseTransformDirection(position - m_GroundPoint[i]).y - m_FootOffset[i] - m_HipsOffset < 0) {
                            var localFootPosition = m_Transform.InverseTransformPoint(position);
                            localFootPosition.y = m_Transform.InverseTransformPoint(m_GroundPoint[i]).y;
                            position = m_Transform.TransformPoint(localFootPosition) + m_CharacterLocomotion.Up * (m_FootOffset[i] + m_HipsOffset);
                            rotation = Quaternion.LookRotation(Vector3.Cross(m_GroundNormal[i], rotation * -Vector3.right), m_CharacterLocomotion.Up);
                            targetWeight = 1f;
                            adjustmentSpeed = m_FootWeightActiveAdjustmentSpeed;
                        }
                    }
                }

                if (adjustmentSpeed == 0) {
                    m_FootIKWeight[i] = 0;
                } else {
                    m_FootIKWeight[i] = Mathf.Clamp01(m_ImmediatePosition ? targetWeight : Mathf.MoveTowards(m_FootIKWeight[i], targetWeight, adjustmentSpeed * Time.deltaTime));
                }

                // Other objects have the chance of modifying the final position and rotation value.
                if (m_OnUpdateIKPosition != null) {
                    position = m_OnUpdateIKPosition(i == 0 ? IKGoal.LeftFoot : IKGoal.RightFoot, position, rotation);
                }
                if (m_OnUpdateIKRotation != null) {
                    rotation = m_OnUpdateIKRotation(i == 0 ? IKGoal.LeftFoot : IKGoal.RightFoot, rotation, position);
                }

                // Apply the IK position and rotation.
                m_Animator.SetIKPosition(ikGoal, position);
                m_Animator.SetIKRotation(ikGoal, rotation);
                m_Animator.SetIKPositionWeight(ikGoal, m_FootIKWeight[i]);
                m_Animator.SetIKRotationWeight(ikGoal, m_FootIKWeight[i]);
            }

            // The knees can be positioned manually.
            if (m_IKTarget[(int)IKGoal.LeftKnee] != null) {
                m_Animator.SetIKHintPosition(AvatarIKHint.LeftKnee, m_IKTarget[(int)IKGoal.LeftKnee].position);
                m_Animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, m_FootIKWeight[0]);
            }
            if (m_IKTarget[(int)IKGoal.RightKnee] != null) {
                m_Animator.SetIKHintPosition(AvatarIKHint.RightKnee, m_IKTarget[(int)IKGoal.RightKnee].position);
                m_Animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, m_FootIKWeight[1]);
            }
        }

        /// <summary>
        /// Returns the position that the raycast should start at when determining if the foot is near the ground.
        /// </summary>
        /// <param name="targetTransform">The Transform of the foot or toe.</param>
        /// <param name="lowerLeg">The Transform of the lower leg.</param>
        /// <param name="distance">The vertical distance between the hip and target Transform.</param>
        /// <returns>The position that the raycast should start at when determining if the foot is near the ground.</returns>
        private Vector3 GetFootRaycastPosition(Transform targetTransform, Transform lowerLeg, out float distance)
        {
            // The relative y position should be the same as the lower leg so the raycast can detect any objects between the lower leg position and current foot position.
            var raycastPosition = m_Transform.InverseTransformPoint(targetTransform.position);
            var localHipPosition = m_Transform.InverseTransformPoint(lowerLeg.position);
            distance = (localHipPosition.y - raycastPosition.y);
            raycastPosition.y = localHipPosition.y;
            return m_Transform.TransformPoint(raycastPosition);
        }

        /// <summary>
        /// Rotates the upper body to look at the target specified by the LookSource.
        /// </summary>
        private void LookAtTarget()
        {
            var headPosition = m_Head.position;
            var lookAtPosition = m_ActiveLookAtTarget ? m_LookAtTargetPosition : GetDefaultLookAtPosition();
            lookAtPosition = headPosition + (lookAtPosition - headPosition).normalized;
            m_LookAtPosition = m_ImmediatePosition ? lookAtPosition : Vector3.Lerp(m_LookAtPosition + m_CharacterMovement, lookAtPosition, m_LookAtAdjustmentSpeed);

            // If the target position has a positive y value then the character should not look down betwen the lerp.
            var localDirection = m_Transform.InverseTransformPoint(lookAtPosition);
            if (localDirection.y > 0) {
                var localLookAtPosition = m_Transform.InverseTransformPoint(m_LookAtPosition);
                if (localLookAtPosition.y < 0) {
                    localLookAtPosition.y = 0;
                    m_LookAtPosition = m_Transform.TransformPoint(localLookAtPosition);
                }
            }
            m_Animator.SetLookAtPosition(m_LookAtPosition);

            m_LookAtBodyIKWeight = m_ImmediatePosition ? m_LookAtBodyWeight :
                                    Mathf.Lerp(m_LookAtBodyIKWeight, m_LookAtBodyWeight, m_LookAtWeightAdjustmentSpeed);
            m_LookAtHeadIKWeight = m_ImmediatePosition ? m_LookAtHeadWeight :
                                        Mathf.Lerp(m_LookAtHeadIKWeight, m_LookAtHeadWeight, m_LookAtWeightAdjustmentSpeed);
            m_LookAtEyesIKWeight = m_ImmediatePosition ? m_LookAtHeadWeight :
                                        Mathf.Lerp(m_LookAtEyesIKWeight, m_LookAtHeadWeight, m_LookAtWeightAdjustmentSpeed);
            m_Animator.SetLookAtWeight(1, m_LookAtBodyIKWeight, m_LookAtHeadIKWeight, m_LookAtEyesIKWeight, m_LookAtClampWeight);

#if UNITY_EDITOR
            // Visualize the direction of the target look position.
            if (m_DebugDrawLookRay) {
                var direction = (m_LookAtPosition - headPosition).normalized * m_LookSource.LookDirectionDistance;
                Debug.DrawRay(headPosition, direction, Color.green);
            }
#endif
        }

        /// <summary>
        /// Rotates the hands to look at the target specified by the LookSource.
        /// </summary>
        private void RotateHands()
        {
            var dominantHandGoal = m_DominantHand == m_RightHand ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;
            var nonDominantHandGoal = m_DominantHand == m_RightHand ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;
            m_NonDominantHandOffset = MathUtility.InverseTransformPoint(m_Animator.GetIKPosition(dominantHandGoal), m_Animator.GetIKRotation(dominantHandGoal), m_Animator.GetIKPosition(nonDominantHandGoal));
            Transform distantHand = null;
            for (int i = 0; i < 2; ++i) {
                var ikGoal = (i == 0 ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand);
                var ikTarget = (i == 0 ? m_IKTarget[(int)IKGoal.LeftHand] : m_IKTarget[(int)IKGoal.RightHand]);

                // If an IK target is specified for the hand then it should use the Transform for the location. This for example allows an item to specify
                // the location of the non-dominant hand. The hands should also rotate towards the look direction if the character is aiming or the item
                // is being used.
                var targetWeight = 0f;
                if (m_HandWeight > 0 && (ikTarget != null || m_Aiming || m_ItemInUse || m_CharacterLocomotion.FirstPersonPerspective || m_OnUpdateIKRotation != null)) {
                    targetWeight = m_HandWeight;
                }

                m_HandRotationIKWeight[i] = Mathf.Clamp01(m_ImmediatePosition ? targetWeight :
                                                Mathf.MoveTowards(m_HandRotationIKWeight[i], targetWeight, m_HandAdjustmentSpeed * Time.deltaTime));

                // Set the IK rotation after the weight has been set. This is done after the weight is set because the rotation should be set at any time 
                // the weight is greater than zero (such as when the hands are transitioning from aiming to no aiming).
                Quaternion targetRotation;
                if (ikTarget != null) {
                    targetRotation = ikTarget.rotation;
                } else {
                    targetRotation = GetTargetHandRotation(ref distantHand, i == 0, ikGoal);
                }
                // Other objects have the chance of modifying the final rotation value.
                targetWeight = m_HandRotationIKWeight[i];
                if (m_OnUpdateIKRotation != null) {
                    targetWeight = 1;
                    targetRotation = m_OnUpdateIKRotation(i == 0 ? IKGoal.LeftHand : IKGoal.RightHand, targetRotation, m_Animator.GetIKPosition(ikGoal));
                }
                m_Animator.SetIKRotation(ikGoal, targetRotation);
                m_Animator.SetIKRotationWeight(ikGoal, targetWeight);
            }
        }

        /// <summary>
        /// Returns the target rotation of the hand.
        /// </summary>
        /// <param name="distantHand">A reference to the hand that is further away from the character.</param>
        /// <param name="leftHand">Is the hand the left hand?</param>
        /// <param name="ikGoal">The AvatarIKGoal of the hand.</param>
        /// <returns>The target rotation of the hand.</returns>
        private Quaternion GetTargetHandRotation(ref Transform distantHand, bool leftHand, AvatarIKGoal ikGoal)
        {
            if (m_HandRotationIKWeight[leftHand ? 0 : 1] == 0) {
                return m_Animator.GetIKRotation(ikGoal);
            }

            // Use the distant hand so the hands are always pointing in the same direction.
            if (distantHand == null) {
                if (m_Transform.InverseTransformPoint(m_RightHand.position).z < m_Transform.InverseTransformPoint(m_LeftHand.position).z) {
                    distantHand = m_LeftHand;
                } else {
                    distantHand = m_RightHand;
                }
            }
            var lookDirection = (m_LookSource.LookDirection(distantHand.position, false, 0, true, false) + m_Transform.TransformDirection(m_LookAtOffset)).normalized;
            return Quaternion.LookRotation(lookDirection, m_CharacterLocomotion.Up) * Quaternion.Inverse(m_Transform.rotation) *
                                Quaternion.Euler(m_Transform.TransformDirection(leftHand ? m_LeftHandRotationSpring.Value : m_RightHandRotationSpring.Value)) *
                                m_Animator.GetIKRotation(ikGoal);
        }

        /// <summary>
        /// Rotates the upper arms to face the target.
        /// </summary>
        private void RotateUpperArms()
        {
            var targetWeight = 0f;
            if (m_DominantUpperArm != null && m_UpperArmWeight > 0 && !m_Unequipping) {
                targetWeight = m_UpperArmWeight;
            }

            var prevUpperArmWeight = m_DominantUpperArmWeight;
            m_DominantUpperArmWeight = Mathf.Clamp01(m_ImmediatePosition ? targetWeight : Mathf.MoveTowards(m_DominantUpperArmWeight, targetWeight, m_UpperArmAdjustmentSpeed * Time.deltaTime));
            if (prevUpperArmWeight > 0 && m_DominantUpperArmWeight == 0) {
                DetermineDominantHand();
            }
            if (m_DominantUpperArm != null) {
                if (m_DominantUpperArmWeight > 0) {
                    // The dominant upper arm should rotate to face the target.
                    var localLookDirection = m_Transform.InverseTransformDirection(m_LookSource.LookDirection(m_DominantUpperArm.position, false, 0, true, false));
                    var lookDirection = m_Transform.InverseTransformDirection(m_Transform.forward);
                    lookDirection.y = localLookDirection.y;
                    lookDirection = m_Transform.TransformDirection(lookDirection).normalized;
                    // Prevent the upper arm from moving too far behind the character.
                    if (localLookDirection.y < 0) {
                        lookDirection = Vector3.Lerp(m_Transform.forward, lookDirection, 1 - Mathf.Abs(localLookDirection.y));
                    }

                    var dominantUpperArmRotation = m_DominantUpperArm.rotation;
                    var targetRotation = Quaternion.FromToRotation(m_Transform.forward, lookDirection) * dominantUpperArmRotation;
                    targetRotation = Quaternion.Slerp(dominantUpperArmRotation, targetRotation, m_DominantUpperArmWeight);

                    // When the hand IK positions are set they should use the updated rotation.
                    var offset = Vector3.Scale(m_DominantUpperArm.InverseTransformPoint(m_DominantHand.position), m_DominantHand.lossyScale);
                    m_DominantHandPosition = MathUtility.TransformPoint(m_DominantUpperArm.position, targetRotation, offset);

                    // The non-dominant hand position is determined by the dominant hand's rotation as well as the upper arm's rotation.
                    if (m_HandRotationIKWeight[m_DominantHand == m_RightHand ? 0 : 1] > 0) {
                        m_NonDominantHandPosition = MathUtility.TransformPoint(m_DominantHandPosition, m_Animator.GetIKRotation(m_DominantHand == m_RightHand ?
                                                            AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand), m_NonDominantHandOffset);
                    } else {
                        offset = Vector3.Scale(m_DominantUpperArm.InverseTransformPoint(m_NonDominantHand.position), m_NonDominantHand.lossyScale);
                        m_NonDominantHandPosition = MathUtility.TransformPoint(m_DominantUpperArm.position, targetRotation, offset);
                    }
                } else if (m_DominantHand != null) {
                    // If the upper arm does not rotate at all then the hand positions can be determined based off of the original upper arm rotation.
                    m_HandOffset = Vector3.Scale(m_DominantHand.InverseTransformPoint(m_NonDominantHand.position), m_NonDominantHand.lossyScale);
                }
            }
        }

        /// <summary>
        /// Updates the interpolation transform to move closer towards the target.
        /// </summary>
        private void UpdateTargetInterpolations()
        {
            if (!m_InterpolateIKTargets) {
                return;
            }

            m_InterpolateIKTargets = false;
            var updateIKTargets = false;
            Transform distantHand = null;
            for (int i = 0; i < (int)IKGoal.Last; ++i) {
                if (m_StartInterpolation[i] != -1) {
                    Vector3 ikPosition;
                    var ikRotation = Quaternion.identity;
                    // Convert the IKGoal to an AvatarIKGoal/AvatarIKHint.
                    if (i == (int)IKGoal.LeftHand) {
                        ikPosition = GetTargetHandPosition(m_LeftHand, true);
                        ikRotation = GetTargetHandRotation(ref distantHand, true, AvatarIKGoal.LeftHand);
                    } else if (i == (int)IKGoal.LeftElbow) {
                        ikPosition = m_Animator.GetIKHintPosition(AvatarIKHint.LeftElbow);
                    } else if (i == (int)IKGoal.RightHand) {
                        ikPosition = GetTargetHandPosition(m_RightHand, false);
                        ikRotation = GetTargetHandRotation(ref distantHand, false, AvatarIKGoal.RightHand);
                    } else if (i == (int)IKGoal.RightElbow) {
                        ikPosition = m_Animator.GetIKHintPosition(AvatarIKHint.RightElbow);
                    } else if (i == (int)IKGoal.LeftFoot) {
                        ikPosition = m_Animator.GetIKPosition(AvatarIKGoal.LeftFoot);
                        ikRotation = m_Animator.GetIKRotation(AvatarIKGoal.LeftFoot);
                    } else if (i == (int)IKGoal.LeftKnee) {
                        ikPosition = m_Animator.GetIKHintPosition(AvatarIKHint.LeftKnee);
                    } else if (i == (int)IKGoal.RightFoot) {
                        ikPosition = m_Animator.GetIKPosition(AvatarIKGoal.RightFoot);
                        ikRotation = m_Animator.GetIKRotation(AvatarIKGoal.RightFoot);
                    } else { // Right Knee.
                        ikPosition = m_Animator.GetIKHintPosition(AvatarIKHint.RightKnee);
                    }
                    // If the target is not null then the transform should interpolate towards the target. If the target is null then
                    // the interpolation should move back towards the original ik position.
                    var time = m_InterpolationDuration[i] > 0 ? Mathf.Clamp01((Time.time - m_StartInterpolation[i]) / m_InterpolationDuration[i]) : 1;
                    if (m_AbilityIKTarget[i] == null) {
                        m_InterpolationTarget[i].SetPositionAndRotation(Vector3.Lerp(m_InterpolationTarget[i].position, ikPosition, time), Quaternion.Slerp(m_InterpolationTarget[i].rotation, ikRotation, time));
                        if (time == 1) {
                            m_StartInterpolation[i] = -1;
                            updateIKTargets = true;
                        }
                    } else {
                        m_InterpolationTarget[i].SetPositionAndRotation(Vector3.Lerp(ikPosition, m_AbilityIKTarget[i].position, time), Quaternion.Slerp(ikRotation, m_AbilityIKTarget[i].rotation, time));
                    }
                    m_InterpolateIKTargets = true;
                }
            }
            if (updateIKTargets) {
                UpdateIKTargets();
            }
        }

        /// <summary>
        /// Position the hands to face the look direction.
        /// </summary>
        private void PositionHands()
        {
            for (int i = 0; i < 2; ++i) {
                var ikGoal = (i == 0 ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand);
                var targetWeight = 0f;
                var hand = (i == 0 ? m_LeftHand : m_RightHand);
                var ikTarget = (i == 0 ? m_IKTarget[(int)IKGoal.LeftHand] : m_IKTarget[(int)IKGoal.RightHand]);
                var hintTarget = (i == 0 ? m_IKTarget[(int)IKGoal.LeftElbow] : m_IKTarget[(int)IKGoal.RightElbow]);
                var hintGoal = (i == 0 ? AvatarIKHint.LeftElbow : AvatarIKHint.RightElbow);

                // If an IK target is specified for the hand then it should use the Transform for the location. This for example allows an item to specify
                // the location of the non-dominant hand. The hands should also be positioned towards the look direction if the character is aiming or the item
                // is being used.
                if (ikTarget != null || m_Aiming || m_ItemInUse || m_CharacterLocomotion.FirstPersonPerspective || m_UpperArmWeight > 0 || m_OnUpdateIKPosition != null) {
                    targetWeight = m_HandWeight;
                }

                m_HandPositionIKWeight[i] = Mathf.Clamp01(m_ImmediatePosition ? targetWeight :
                                                Mathf.MoveTowards(m_HandPositionIKWeight[i], targetWeight, m_HandAdjustmentSpeed * Time.deltaTime));

                // Set the IK position after the weight has been set. This is done after the weight is set because the position should be set at any time 
                // the weight is greater than zero (such as when the hands are transitioning from aiming to no aiming).
                var targetPosition = m_Animator.GetIKPosition(ikGoal);
                if (m_HandPositionIKWeight[i] > 0) {
                    if (ikTarget != null) {
                        targetPosition = ikTarget.position;
                        if (hintTarget != null) {
                            m_Animator.SetIKHintPosition(hintGoal, hintTarget.position);
                        }
                    } else {
                        targetPosition = GetTargetHandPosition(hand, i == 0);
                    }
                }
                // Other objects have the chance of modifying the final position value.
                targetWeight = m_HandPositionIKWeight[i];
                if (m_OnUpdateIKPosition != null) {
                    targetWeight = 1;
                    targetPosition = m_OnUpdateIKPosition(i == 0 ? IKGoal.LeftHand : IKGoal.RightHand, targetPosition, m_Animator.GetIKRotation(ikGoal));
                }
                m_Animator.SetIKPosition(ikGoal, targetPosition);
                m_Animator.SetIKPositionWeight(ikGoal, targetWeight);
                m_Animator.SetIKHintPositionWeight(hintGoal, hintTarget != null ? m_HandPositionIKWeight[i] : 0);
            }
        }

        /// <summary>
        /// Returns the target position of the hand.
        /// </summary>
        /// <param name="hand">The hand to retrieve the position of.</param>
        /// <param name="leftHand">Is the hand the left hand?</param>
        /// <returns>The target position of the hand.</returns>
        private Vector3 GetTargetHandPosition(Transform hand, bool leftHand)
        {
            // The RotateUpperArms method will set the dominant and nondominant hand positions if it is being used. Otherwise the offset is set of the nondominant hand.
            Vector3 handPosition;
            if (m_DominantUpperArmWeight > 0) {
                handPosition = (hand == m_DominantHand ? m_DominantHandPosition : m_NonDominantHandPosition);
            } else {
                handPosition = ((hand == m_DominantHand || m_DominantHand == null) ? hand.position : m_DominantHand.TransformPoint(m_HandOffset));
            }
            return handPosition + m_Transform.TransformDirection(leftHand ? m_LeftHandPositionSpring.Value : m_RightHandPositionSpring.Value) +
                                            m_Transform.TransformDirection(m_HandPositionOffset);
        }

        /// <summary>
        /// Updates the IK component after the animator has updated.
        /// </summary>
        public void LateUpdate()
        {
            m_Hips.position = m_Transform.TransformPoint(m_HipsPosition);

            // After the IK has finished positioning the limbs for the first time reset the immediate position. It should smoothly blend
            // during runtime.
            if (m_ImmediatePosition && m_IKPass) {
                m_ImmediatePosition = false;
            }
        }

        /// <summary>
        /// Immediately position the IK limbs.
        /// </summary>
        private void ImmediatePosition()
        {
            m_ImmediatePosition = true;
            m_IKPass = false;
        }

        /// <summary>
        /// The animator has snapped into position.
        /// </summary>
        private void AnimatorSnapped()
        {
            if (!enabled) {
                return;
            }

            LateUpdate();
        }

        /// <summary>
        /// The character has died. Disable the component.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_Enable = enabled;
            enabled = false;
        }

        /// <summary>
        /// The character has respawned. Enable the component.
        /// </summary>
        private void OnRespawn()
        {
            enabled = m_Enable;
            m_ImmediatePosition = true;
            m_IKPass = false;
        }

        /// <summary>
        /// Callback when the StateManager will change the active state on the current object.
        /// </summary>
        public override void StateWillChange()
        {
            // Remember the interal spring values so they can be restored if a new spring is applied during the state change.
            m_PrevLeftHandPositionSpringValue = m_LeftHandPositionSpring.Value;
            m_PrevLeftHandPositionSpringVelocity = m_LeftHandPositionSpring.Velocity;
            m_PrevLeftHandRotationSpringValue = m_LeftHandRotationSpring.Value;
            m_PrevLeftHandRotationSpringVelocity = m_LeftHandRotationSpring.Velocity;
            m_PrevRightHandPositionSpringValue = m_RightHandPositionSpring.Value;
            m_PrevRightHandPositionSpringVelocity = m_RightHandPositionSpring.Velocity;
            m_PrevRightHandRotationSpringValue = m_RightHandRotationSpring.Value;
            m_PrevRightHandRotationSpringVelocity = m_RightHandRotationSpring.Velocity;
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            m_LeftHandPositionSpring.Value = m_PrevLeftHandPositionSpringValue;
            m_LeftHandPositionSpring.Velocity = m_PrevLeftHandPositionSpringVelocity;
            m_LeftHandRotationSpring.Value = m_PrevLeftHandRotationSpringValue;
            m_LeftHandRotationSpring.Velocity = m_PrevLeftHandRotationSpringVelocity;
            m_RightHandPositionSpring.Value = m_PrevRightHandPositionSpringValue;
            m_RightHandPositionSpring.Velocity = m_PrevRightHandPositionSpringVelocity;
            m_RightHandRotationSpring.Value = m_PrevRightHandRotationSpringValue;
            m_RightHandRotationSpring.Velocity = m_PrevRightHandRotationSpringVelocity;
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            m_LeftHandPositionSpring.Destroy();
            m_LeftHandRotationSpring.Destroy();
            m_RightHandPositionSpring.Destroy();
            m_RightHandRotationSpring.Destroy();

            EventHandler.UnregisterEvent<ILookSource>(m_CharacterGameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.UnregisterEvent<CharacterItem, int>(m_CharacterGameObject, "OnInventoryEquipItem", OnEquipItem);
            EventHandler.UnregisterEvent<CharacterItem, int>(m_CharacterGameObject, "OnInventoryUnequipItem", OnUnequipItem);
            EventHandler.UnregisterEvent<CharacterItem, int>(m_CharacterGameObject, "OnInventoryRemoveItem", OnUnequipItem);
            EventHandler.UnregisterEvent<bool>(m_CharacterGameObject, "OnAimAbilityAim", OnAim);
            EventHandler.UnregisterEvent<bool, Abilities.Items.Use>(m_CharacterGameObject, "OnUseAbilityStart", OnUseStart);
            EventHandler.UnregisterEvent<int, Vector3, Vector3, bool>(m_CharacterGameObject, "OnAddSecondaryForce", OnAddForce);
            EventHandler.UnregisterEvent(m_CharacterGameObject, "OnAnimatorWillSnap", ImmediatePosition);
            EventHandler.UnregisterEvent(m_CharacterGameObject, "OnAnimatorSnapped", AnimatorSnapped);
            EventHandler.UnregisterEvent(m_CharacterGameObject, "OnAnimatorWillSnap", ImmediatePosition);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_CharacterGameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_CharacterGameObject, "OnRespawn", OnRespawn);
        }
    }
}