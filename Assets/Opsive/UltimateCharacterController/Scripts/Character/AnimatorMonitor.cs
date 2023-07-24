/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// The AnimatorMonitor acts as a bridge for the parameters on the Animator component.
    /// If an Animator component is not attached to the character (such as for first person view) then the updates will be forwarded to the item's Animator.
    /// </summary>
    public class AnimatorMonitor : StateBehavior
    {
#if UNITY_EDITOR
        [Tooltip("Should the Animator log any changes to the item parameters?")]
        [SerializeField] protected bool m_LogAbilityParameterChanges;
        [Tooltip("Should the Animator log any changes to the item parameters?")]
        [SerializeField] protected bool m_LogItemParameterChanges;
        [Tooltip("Should the Animator log any events that it sends?")]
        [SerializeField] protected bool m_LogEvents;
#endif
        [Tooltip("The damping time for the Horizontal Movement parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField] protected float m_HorizontalMovementDampingTime = 0.1f;
        [Tooltip("The damping time for the Forward Movement parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField] protected float m_ForwardMovementDampingTime = 0.1f;
        [Tooltip("The damping time for the Pitch parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField] protected float m_PitchDampingTime = 0.1f;
        [Tooltip("The damping time for the Yaw parameter. The higher the value the slower the parameter value changes.")]
        [SerializeField] protected float m_YawDampingTime = 0.1f;
        [Tooltip("The runtime speed of the Animator.")]
        [SerializeField] protected float m_AnimatorSpeed = 1;
        [Tooltip("Specifies how much to multiply the yaw parameter by when turning in place.")]
        [SerializeField] protected float m_YawMultiplier = 7;
        [Tooltip("Specifies the value of the Speed Parameter when the character is moving.")]
        [SerializeField] protected float m_MovingSpeedParameterValue = 1;

#if UNITY_EDITOR
        public bool LogEvents { get { return m_LogEvents; } }
#endif
        public float HorizontalMovementDampingTime { get { return m_HorizontalMovementDampingTime; } set { m_HorizontalMovementDampingTime = value; } }
        public float ForwardMovementDampingTime { get { return m_ForwardMovementDampingTime; } set { m_ForwardMovementDampingTime = value; } }
        public float PitchDampingTime { get { return m_PitchDampingTime; } set { m_PitchDampingTime = value; } }
        public float YawDampingTime { get { return m_YawDampingTime; } set { m_YawDampingTime = value; } }
        public float AnimatorSpeed { get { return m_AnimatorSpeed; } set { m_AnimatorSpeed = value; if (m_Animator != null) { m_Animator.speed = m_AnimatorSpeed; } } }

        private static int s_HorizontalMovementHash = Animator.StringToHash("HorizontalMovement");
        private static int s_ForwardMovementHash = Animator.StringToHash("ForwardMovement");
        private static int s_PitchHash = Animator.StringToHash("Pitch");
        private static int s_YawHash = Animator.StringToHash("Yaw");
        private static int s_SpeedHash = Animator.StringToHash("Speed");
        private static int s_HeightHash = Animator.StringToHash("Height");
        private static int s_MovingHash = Animator.StringToHash("Moving");
        private static int s_AimingHash = Animator.StringToHash("Aiming");
        private static int s_MovementSetIDHash = Animator.StringToHash("MovementSetID");
        private static int s_AbilityIndexHash = Animator.StringToHash("AbilityIndex");
        private static int s_AbilityChangeHash = Animator.StringToHash("AbilityChange");
        private static int s_AbilityIntDataHash = Animator.StringToHash("AbilityIntData");
        private static int s_AbilityFloatDataHash = Animator.StringToHash("AbilityFloatData");
        private static int[] s_ItemSlotIDHash;
        private static int[] s_ItemSlotStateIndexHash;
        private static int[] s_ItemSlotStateIndexChangeHash;
        private static int[] s_ItemSlotSubstateIndexHash;

        protected GameObject m_GameObject;
        protected Transform m_Transform;
        protected Animator m_Animator;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private CharacterIK m_CharacterIK;

        private float m_HorizontalMovement;
        private float m_ForwardMovement;
        private float m_Pitch;
        private float m_Yaw;
        private float m_Speed;
        private float m_Height;
        private bool m_Moving;
        private bool m_Aiming;
        private int m_MovementSetID;
        private int m_AbilityIndex;
        private int m_AbilityIntData;
        private float m_AbilityFloatData;
        private bool m_HasItemParameters;
        private int[] m_ItemSlotID;
        private int[] m_ItemSlotStateIndex;
        private int[] m_ItemSlotSubstateIndex;
        private CharacterItem[] m_EquippedItems;
        private bool m_DirtyAbilityParameters;
        private bool m_DirtyItemAbilityParameters;
        private bool m_DirtyItemAbilityParametersForceChange;
        private bool m_DirtyEquippedItems;
        private bool[] m_DirtyItemStateIndexParameters;
        private bool[] m_DirtyItemSubstateIndexParameters;
        private HashSet<int> m_ItemParameterExists;
        private bool m_SpeedParameterOverride;

        public bool AnimatorEnabled { get { return m_Animator != null && m_Animator.enabled; } }
        public float HorizontalMovement { get { return m_HorizontalMovement; } }
        public float ForwardMovement { get { return m_ForwardMovement; } }
        public float Pitch { get { return m_Pitch; } }
        public float Yaw { get { return m_Yaw; } }
        public float Speed { get { return m_Speed; } }
        public float Height { get { return m_Height; } }
        public bool Moving { get { return m_Moving; } }
        public bool Aiming { get { return m_Aiming; } }
        public int MovementSetID { get { return m_MovementSetID; } }
        public int AbilityIndex { get { return m_AbilityIndex; } }
        public bool AbilityChange { get { return (m_Animator != null) && m_Animator.GetBool(s_AbilityChangeHash); } }
        public int AbilityIntData { get { return m_AbilityIntData; } }
        public float AbilityFloatData { get { return m_AbilityFloatData; } }
        public bool HasItemParameters { get { return m_HasItemParameters; } }
        public int ParameterSlotCount { get { return m_ItemSlotID.Length; } }
        public int[] ItemSlotID { get { return m_ItemSlotID; } }
        public int[] ItemSlotStateIndex { get { return m_ItemSlotStateIndex; } }
        public int[] ItemSlotSubstateIndex { get { return m_ItemSlotSubstateIndex; } }
        [Snapshot] protected CharacterItem[] EquippedItems { get { return m_EquippedItems; } set { m_EquippedItems = value; } }
        [NonSerialized] public bool SpeedParameterOverride { get { return m_SpeedParameterOverride; } set { m_SpeedParameterOverride = value; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            if (!Game.CharacterInitializer.AutoInitialization) {
                Game.CharacterInitializer.Instance.OnAwake += AwakeInternal;
                return;
            }

            AwakeInternal();
        }

        /// <summary>
        /// Internal method which initializes the default values.
        /// </summary>
        private void AwakeInternal()
        {
            if (!Game.CharacterInitializer.AutoInitialization) {
                Game.CharacterInitializer.Instance.OnAwake -= AwakeInternal;
            }

            base.Awake();

            m_CharacterLocomotion = gameObject.GetComponentInParent<UltimateCharacterLocomotion>();
            m_GameObject = m_CharacterLocomotion.gameObject;
            m_Transform = m_GameObject.transform;
            m_Animator = gameObject.GetComponent<Animator>(); // The Animator does not have to exist on the same GameObject as the CharacterLocomotion.
            m_CharacterIK = gameObject.GetComponent<CharacterIK>();

#if UNITY_EDITOR
            // If the animator doesn't have the required parameters then it's not a valid animator.
            if (m_Animator != null) {
                if (!HasParameter(s_HorizontalMovementHash) || !HasParameter(s_ForwardMovementHash) || !HasParameter(s_AbilityChangeHash)) {
                    Debug.LogError($"Error: The animator {m_Animator.name} is not designed to work with the Ultimate Character Controller. " +
                                   "Ensure the animator has all of the required parameters.");
                    return;
                }
            }
#endif
            InitializeItemParameters();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterMoving", OnMoving);
            EventHandler.RegisterEvent<Abilities.Ability, bool>(m_GameObject, "OnCharacterAbilityActive", OnAbilityActive);
            EventHandler.RegisterEvent<Abilities.Items.ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterUpdateAbilityParameters", UpdateAbilityAnimatorParameters);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterUpdateItemAbilityParameters", ExternalUpdateItemAbilityAnimatorParameters);
            EventHandler.RegisterEvent<CharacterItem, int>(m_GameObject, "OnAbilityWillEquipItem", OnWillEquipItem);
            EventHandler.RegisterEvent<CharacterItem, int>(m_GameObject, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.RegisterEvent<CharacterItem, int>(m_GameObject, "OnInventoryRemoveItem", OnUnequipItem);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnAimAbilityAim", OnAiming);
            if (m_Animator != null) {
                var modelManager = m_CharacterLocomotion.gameObject.GetCachedComponent<ModelManager>();
                if (modelManager == null || modelManager.ActiveModel == gameObject) {
                    m_CharacterLocomotion.OnAnimationUpdate += UpdateAnimatorParameters;
                }
                m_Animator.speed = m_AnimatorSpeed;
                EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterSnapAnimator", SnapAnimator);
                EventHandler.RegisterEvent<GameObject>(m_GameObject, "OnCharacterSwitchModels", OnSwitchModels);
                EventHandler.RegisterEvent<float>(m_GameObject, "OnCharacterChangeTimeScale", OnChangeTimeScale);
            }

        }

        /// <summary>
        /// Does the animator have the specified parameter?
        /// </summary>
        /// <param name="parameterHash">The hash of the parameter.</param>
        /// <returns>True if the animator has the specified parameter.</returns>
        private bool HasParameter(int parameterHash)
        {
            for (int i = 0; i < m_Animator.parameterCount; ++i) {
                if (m_Animator.parameters[i].nameHash == parameterHash) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Initializes the item parameters.
        /// </summary>
        public void InitializeItemParameters()
        {
            if (m_HasItemParameters) {
                return;
            }
            // The Animator Controller may not have the item parameters if the character can never equip an item.
            m_HasItemParameters = m_GameObject.GetComponentInChildren<ItemPlacement>(true) != null;
            var inventory = m_GameObject.GetComponent<InventoryBase>();
            if (inventory == null) {
                return;
            }

            var slotCount = inventory.SlotCount;
            m_EquippedItems = new CharacterItem[slotCount];

            m_ItemSlotID = new int[slotCount];
            m_ItemSlotStateIndex = new int[slotCount];
            m_ItemSlotSubstateIndex = new int[slotCount];
            m_DirtyItemStateIndexParameters = new bool[slotCount];
            m_DirtyItemSubstateIndexParameters = new bool[slotCount];
            m_ItemParameterExists = new HashSet<int>();

            if (m_Animator == null) {
                return;
            }

            // As of version 3.0.10 the Animator should be updated within the normal Update loop.
            m_Animator.updateMode = AnimatorUpdateMode.Normal;

            if (s_ItemSlotIDHash == null || s_ItemSlotIDHash.Length < slotCount) {
                s_ItemSlotIDHash = new int[slotCount];
                s_ItemSlotStateIndexHash = new int[slotCount];
                s_ItemSlotStateIndexChangeHash = new int[slotCount];
                s_ItemSlotSubstateIndexHash = new int[slotCount];
            }

            for (int i = 0; i < slotCount; ++i) {
                // Animators do not need to contain every slot index.
                var slotIDHash = Animator.StringToHash(string.Format("Slot{0}ItemID", i));
                if (!HasParameter(slotIDHash)) {
                    continue;
                }
                m_ItemParameterExists.Add(i);

                if (s_ItemSlotIDHash[i] == 0) {
                    s_ItemSlotIDHash[i] = slotIDHash;
                    s_ItemSlotStateIndexHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemStateIndex", i));
                    s_ItemSlotStateIndexChangeHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemStateIndexChange", i));
                    s_ItemSlotSubstateIndexHash[i] = Animator.StringToHash(string.Format("Slot{0}ItemSubstateIndex", i));
                }
            }
        }

        /// <summary>
        /// Prepares the Animator parameters for start.
        /// </summary>
        protected virtual void Start()
        {
            SnapAnimator(false);

            if (m_Animator != null) {
                var characterLocomotion = m_GameObject.GetCachedComponent<UltimateCharacterLocomotion>();
                if (characterLocomotion != null) {
                    OnChangeTimeScale(characterLocomotion.TimeScale);
                }
            }
        }

        /// <summary>
        /// Snaps the animator to the default values.
        /// </summary>
        protected virtual void SnapAnimator()
        {
            SnapAnimator(true);
        }

        /// <summary>
        /// Snaps the animator to the default values.
        /// </summary>
        /// <param name="executeEvent">Should the animator snapped event be executed?</param>
        protected virtual void SnapAnimator(bool executeEvent)
        {
            // A first person view may not use an Animator.
            if (m_Animator != null) {
                // The values should be reset enabled so the animator will snap to the correct animation.
                m_Animator.SetFloat(s_HorizontalMovementHash, m_HorizontalMovement, 0, 0);
                m_Animator.SetFloat(s_ForwardMovementHash, m_ForwardMovement, 0, 0);
                m_Animator.SetFloat(s_PitchHash, m_Pitch, 0, 0);
                m_Animator.SetFloat(s_YawHash, m_Yaw, 0, 0);
                m_Animator.SetFloat(s_SpeedHash, m_Speed, 0, 0);
                m_Animator.SetFloat(s_HeightHash, m_Height, 0, 0);
                m_Animator.SetBool(s_MovingHash, m_Moving);
                m_Animator.SetBool(s_AimingHash, m_Aiming);
                m_Animator.SetInteger(s_MovementSetIDHash, m_MovementSetID);
                m_Animator.SetInteger(s_AbilityIndexHash, m_AbilityIndex);
                m_Animator.SetTrigger(s_AbilityChangeHash);
                m_Animator.SetInteger(s_AbilityIntDataHash, m_AbilityIntData);
                m_Animator.SetFloat(s_AbilityFloatDataHash, m_AbilityFloatData, 0, 0);

                if (m_HasItemParameters) {
                    UpdateItemIDParameters();
                    for (int i = 0; i < m_EquippedItems.Length; ++i) {
                        if (!m_ItemParameterExists.Contains(i)) {
                            continue;
                        }
                        m_Animator.SetInteger(s_ItemSlotIDHash[i], m_ItemSlotID[i]);
                        m_Animator.SetTrigger(s_ItemSlotStateIndexChangeHash[i]);
                        m_Animator.SetInteger(s_ItemSlotStateIndexHash[i], m_ItemSlotStateIndex[i]);
                        m_Animator.SetInteger(s_ItemSlotSubstateIndexHash[i], m_ItemSlotSubstateIndex[i]);
                    }
                }

                if (executeEvent) {
                    EventHandler.ExecuteEvent(m_GameObject, "OnAnimatorWillSnap");
                }

                // Keep the IK component disabled until the animator is snapped. This will prevent the OnAnimatorIK callback from occurring.
                var ikEnabled = false;
                if (m_CharacterIK != null) {
                    ikEnabled = m_CharacterIK.enabled;
                    m_CharacterIK.enabled = false;
                }

                // Root motion should not move the character when snapping.
                var position = m_Transform.position;
                var rotation = m_Transform.rotation;

                // Update 0 will force the changes.
                if (m_Animator.isActiveAndEnabled) {
                    m_Animator.Update(0);
                }
#if UNITY_EDITOR
                var count = 0;
#endif
                // Keep updating the Animator until it is no longer in a transition. This will snap the animator to the correct state immediately.
                while (IsInTrasition()) {
#if UNITY_EDITOR
                    count++;
                    if (count > TimeUtility.TargetFramerate * 2) {
                        Debug.LogError("Error: The animator is not leaving a transition. Ensure your Animator Controller does not have any infinite loops.");
                        return;
                    }
#endif
                    m_Animator.Update(Time.fixedDeltaTime * 2);
                }

                // The animator should be positioned at the start of each state.
                for (int i = 0; i < m_Animator.layerCount; ++i) {
                    m_Animator.Play(m_Animator.GetCurrentAnimatorStateInfo(i).fullPathHash, i, 0);
                }

                if (m_Animator.isActiveAndEnabled) {
                    m_Animator.Update(Time.fixedDeltaTime);
                }
                
                // Prevent the change parameters from staying triggered when the animator is on the idle state.
                SetAbilityChangeParameter(false);

                m_Transform.SetPositionAndRotation(position, rotation);
                if (ikEnabled) {
                    m_CharacterIK.enabled = true;
                }
            }

            // The item animators should also snap.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    SetItemStateIndexChangeParameter(i, false);
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SnapAnimator();
                    }
                }
            }

            if (executeEvent) {
                EventHandler.ExecuteEvent(m_GameObject, "OnAnimatorSnapped");
            }
        }

        /// <summary>
        /// Is the Animator Controller currently in a transition?
        /// </summary>
        /// <returns>True if any layer within the Animator Controller is within a transition.</returns>
        private bool IsInTrasition()
        {
            for (int i = 0; i < m_Animator.layerCount; ++i) {
                if (m_Animator.IsInTransition(i)) {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Returns true if the specified layer is in transition.
        /// </summary>
        /// <param name="layerIndex">The layer to determine if it is in transition.</param>
        /// <returns>True if the specified layer is in transition.</returns>
        public bool IsInTransition(int layerIndex)
        {
            if (m_Animator == null) {
                return false;
            }

            return m_Animator.IsInTransition(layerIndex);
        }

        /// <summary>
        /// Updates the Animator paremters.
        /// </summary>
        protected virtual void UpdateAnimatorParameters()
        {
            SetHorizontalMovementParameter(m_CharacterLocomotion.InputVector.x, m_CharacterLocomotion.TimeScale, m_HorizontalMovementDampingTime);
            SetForwardMovementParameter(m_CharacterLocomotion.InputVector.y, m_CharacterLocomotion.TimeScale, m_ForwardMovementDampingTime);
            if (m_CharacterLocomotion.LookSource != null) {
                SetPitchParameter(m_CharacterLocomotion.LookSource.Pitch, m_CharacterLocomotion.TimeScale, m_PitchDampingTime);
            }
            float yawAngle;
            if (m_CharacterLocomotion.UsingRootMotionRotation) {
                yawAngle = MathUtility.ClampInnerAngle(m_CharacterLocomotion.DeltaRotation.y);
            } else {
                yawAngle = MathUtility.ClampInnerAngle((m_CharacterLocomotion.DesiredRotation * Quaternion.Inverse(m_CharacterLocomotion.MovingPlatformRotation)).eulerAngles.y);
            }
            SetYawParameter(yawAngle * m_YawMultiplier, m_CharacterLocomotion.TimeScale, m_YawDampingTime);
            if (!m_SpeedParameterOverride) {
                SetSpeedParameter(m_CharacterLocomotion.Moving ? m_MovingSpeedParameterValue : 0, m_CharacterLocomotion.TimeScale);
            }

            UpdateDirtyAbilityAnimatorParameters();
            UpdateItemIDParameters();
        }

        /// <summary>
        /// Sets the Horizontal Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetHorizontalMovementParameter(float value, float timeScale)
        {
            SetHorizontalMovementParameter(value, timeScale, m_HorizontalMovementDampingTime);
        }

        /// <summary>
        /// Sets the Horizontal Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetHorizontalMovementParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_HorizontalMovement != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_HorizontalMovementHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_HorizontalMovement = m_Animator.GetFloat(s_HorizontalMovementHash);
                    if (Mathf.Abs(m_HorizontalMovement) < 0.001f) {
                        m_HorizontalMovement = 0;
                        m_Animator.SetFloat(s_HorizontalMovementHash, 0);
                    }
                } else {
                    m_HorizontalMovement = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetHorizontalMovementParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Forward Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetForwardMovementParameter(float value, float timeScale)
        {
            SetForwardMovementParameter(value, timeScale, m_ForwardMovementDampingTime);
        }

        /// <summary>
        /// Sets the Forward Movement parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetForwardMovementParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_ForwardMovement != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_ForwardMovementHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_ForwardMovement = m_Animator.GetFloat(s_ForwardMovementHash);
                    if (Mathf.Abs(m_ForwardMovement) < 0.001f) {
                        m_ForwardMovement = 0;
                        m_Animator.SetFloat(s_ForwardMovementHash, 0);
                    }
                } else {
                    m_ForwardMovement = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetForwardMovementParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Pitch parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <returns>True if the parameter was changed.</returns>
        public void SetPitchParameter(float value, float timeScale)
        {
            SetPitchParameter(value, timeScale, m_PitchDampingTime);
        }

        /// <summary>
        /// Sets the Pitch parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetPitchParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_Pitch != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_PitchHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_Pitch = m_Animator.GetFloat(s_PitchHash);
                    if (Mathf.Abs(m_Pitch) < 0.001f) {
                        m_Pitch = 0;
                        m_Animator.SetFloat(s_PitchHash, 0);
                    }
                } else {
                    m_Pitch = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetPitchParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Yaw parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <returns>True if the parameter was changed.</returns>
        public void SetYawParameter(float value, float timeScale)
        {
            SetYawParameter(value, timeScale, m_YawDampingTime);
        }

        /// <summary>
        /// Sets the Yaw parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetYawParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_Yaw != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_YawHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_Yaw = m_Animator.GetFloat(s_YawHash);
                    if (Mathf.Abs(m_Yaw) < 0.001f) {
                        m_Yaw = 0;
                        m_Animator.SetFloat(s_YawHash, 0);
                    }
                } else {
                    m_Yaw = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetYawParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Speed parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetSpeedParameter(float value, float timeScale)
        {
            SetSpeedParameter(value, timeScale, 0);
        }

        /// <summary>
        /// Sets the Speed parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetSpeedParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_Speed != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_SpeedHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_Speed = m_Animator.GetFloat(s_SpeedHash);
                    if (Mathf.Abs(m_Speed) < 0.001f) {
                        m_Speed = 0;
                        m_Animator.SetFloat(s_SpeedHash, 0);
                    }
                } else {
                    m_Speed = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetSpeedParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Height parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetHeightParameter(float value)
        {
            var change = m_Height != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_HeightHash, value, 0, 0);
                    m_Height = (int)m_Animator.GetFloat(s_HeightHash);
                    if (Mathf.Abs(m_Height) < 0.001f) {
                        m_Height = 0;
                        m_Animator.SetFloat(s_HeightHash, 0);
                    }
                } else {
                    m_Height = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetHeightParameter(value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Moving parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetMovingParameter(bool value)
        {
            var change = m_Moving != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetBool(s_MovingHash, value);
                }
                m_Moving = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetMovingParameter(value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Aiming parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAimingParameter(bool value)
        {
            var change = m_Aiming != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetBool(s_AimingHash, value);
                }
                m_Aiming = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetAimingParameter(value);
                    }
                }
            }
            return change;
        }

        /// <summary>
        /// Sets the Movement Set ID parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetMovementSetIDParameter(int value)
        {
            var change = m_MovementSetID != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetInteger(s_MovementSetIDHash, value);
                }
                m_MovementSetID = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetMovementSetIDParameter(value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Ability Index parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityIndexParameter(int value)
        {
            var change = m_AbilityIndex != value;
            if (change) {
#if UNITY_EDITOR
                if (m_LogAbilityParameterChanges) {
                    Debug.Log($"{Time.frameCount} Changed AbilityIndex to {value} on GameObject {m_GameObject.name}.");
                }
#endif
                if (m_Animator != null) {
                    m_Animator.SetInteger(s_AbilityIndexHash, value);
                    SetAbilityChangeParameter(true);
                }
                m_AbilityIndex = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetAbilityIndexParameter(value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Ability Change parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityChangeParameter(bool value)
        {
            if (m_Animator != null && m_Animator.GetBool(s_AbilityChangeHash) != value) {
                if (value) {
                    m_Animator.SetTrigger(s_AbilityChangeHash);
                } else {
                    m_Animator.ResetTrigger(s_AbilityChangeHash);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the Int Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityIntDataParameter(int value)
        {
            var change = m_AbilityIntData != value;
            if (change) {
#if UNITY_EDITOR
                if (m_LogAbilityParameterChanges) {
                    Debug.Log($"{Time.frameCount} Changed AbilityIntData to {value} on GameObject {m_GameObject.name}.");
                }
#endif
                if (m_Animator != null) {
                    m_Animator.SetInteger(s_AbilityIntDataHash, value);
                }
                m_AbilityIntData = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetAbilityIntDataParameter(value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        public void SetAbilityFloatDataParameter(float value, float timeScale)
        {
            SetAbilityFloatDataParameter(value, timeScale, 0);
        }

        /// <summary>
        /// Sets the Float Data parameter to the specified value.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <param name="timeScale">The time scale of the character.</param>
        /// <param name="dampingTime">The time allowed for the parameter to reach the value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetAbilityFloatDataParameter(float value, float timeScale, float dampingTime)
        {
            var change = m_AbilityFloatData != value;
            if (change) {
                if (m_Animator != null) {
                    m_Animator.SetFloat(s_AbilityFloatDataHash, value, dampingTime, TimeUtility.DeltaTimeScaled / timeScale);
                    m_AbilityFloatData = m_Animator.GetFloat(s_AbilityFloatDataHash);
                } else {
                    m_AbilityFloatData = value;
                }
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetAbilityFloatDataParameter(value, timeScale, dampingTime);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Item ID parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        public virtual bool SetItemIDParameter(int slotID, int value)
        {
            var change = m_ItemSlotID[slotID] != value;
            if (change) {
#if UNITY_EDITOR
                if (m_LogItemParameterChanges) {
                    Debug.Log($"{Time.frameCount} Changed Slot{slotID}ItemID to {value} on GameObject {m_GameObject.name}.");
                }
#endif
                if (m_Animator != null && m_ItemParameterExists.Contains(slotID)) {
                    m_Animator.SetInteger(s_ItemSlotIDHash[slotID], value);
                    // Even though no state index was changed the trigger should be set to true so the animator can transition to the new item id.
                    SetItemStateIndexChangeParameter(slotID, value != 0);
                }
                m_ItemSlotID[slotID] = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetItemIDParameter(slotID, value);
                    }
                }
            }

            return change;
        }
        
        /// <summary>
        /// Sets the Primary Item State Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        /// <param name="forceChange">Force the change the new value?</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetItemStateIndexParameter(int slotID, int value, bool forceChange)
        {
            var change = forceChange || m_ItemSlotStateIndex[slotID] != value;
            if (change) {
#if UNITY_EDITOR
                if (m_LogItemParameterChanges) {
                    Debug.Log($"{Time.frameCount} Changed Slot{slotID}ItemStateIndex to {value} on GameObject {m_GameObject.name}.");
                }
#endif
                if (m_Animator != null && m_ItemParameterExists.Contains(slotID)) {
                    m_Animator.SetInteger(s_ItemSlotStateIndexHash[slotID], value);
                    SetItemStateIndexChangeParameter(slotID, value != 0);
                }
                m_ItemSlotStateIndex[slotID] = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetItemStateIndexParameter(slotID, value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Sets the Item State Index Change parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot of that item that should be set.</param>
        /// <param name="value">The new value.</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetItemStateIndexChangeParameter(int slotID, bool value)
        {
            if (!m_ItemParameterExists.Contains(slotID)) {
                return false;
            }

            if (m_Animator != null && m_Animator.GetBool(s_ItemSlotStateIndexChangeHash[slotID]) != value) {
                if (value) {
                    m_Animator.SetTrigger(s_ItemSlotStateIndexChangeHash[slotID]);
                } else {
                    m_Animator.ResetTrigger(s_ItemSlotStateIndexChangeHash[slotID]);
                }
                
#if UNITY_EDITOR
                if (m_LogItemParameterChanges) {
                    Debug.Log($"{Time.frameCount} Changed Slot{slotID}ItemStateIndexChange Trigger to {value} on GameObject {m_GameObject.name}.");
                }
#endif
                
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the Item Substate Index parameter with the indicated slot to the specified value.
        /// </summary>
        /// <param name="slotID">The slot that the item occupies.</param>
        /// <param name="value">The new value.</param>
        /// <param name="forceChange">Force the change the new value?</param>
        /// <returns>True if the parameter was changed.</returns>
        public virtual bool SetItemSubstateIndexParameter(int slotID, int value, bool forceChange)
        {
            var change = forceChange || m_ItemSlotSubstateIndex[slotID] != value;
            if (change) {
#if UNITY_EDITOR
                if (m_LogItemParameterChanges) {
                    Debug.Log($"{Time.frameCount} Changed Slot{slotID}ItemSubstateIndex to {value} on GameObject {m_GameObject.name}.");
                }
#endif
                if (m_Animator != null && m_ItemParameterExists.Contains(slotID)) {
                    m_Animator.SetInteger(s_ItemSlotSubstateIndexHash[slotID], value);
                }
                m_ItemSlotSubstateIndex[slotID] = value;
            }

            // The item's Animator should also be aware of the updated parameter value.
            if (m_EquippedItems != null) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (m_EquippedItems[i] != null) {
                        m_EquippedItems[i].SetItemSubstateIndexParameter(slotID, value);
                    }
                }
            }

            return change;
        }

        /// <summary>
        /// Executes an event on the EventHandler.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        public virtual void ExecuteEvent(string eventName)
        {
#if UNITY_EDITOR
            if (m_LogEvents) {
                Debug.Log($"{Time.frameCount} Execute {eventName} on GameObject {m_GameObject.name}.");
            }
#endif
            EventHandler.ExecuteEvent(m_GameObject, eventName);
        }

        /// <summary>
        /// The character's ability has been started or stopped.
        /// </summary>
        /// <param name="ability">The ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnAbilityActive(Abilities.Ability ability, bool active)
        {
            UpdateAbilityAnimatorParameters();
        }

        /// <summary>
        /// The character's item ability has been started or stopped.
        /// </summary>
        /// <param name="itemAbility">The ItemAbility activated or deactivated.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnItemAbilityActive(Abilities.Items.ItemAbility itemAbility, bool active)
        {
            UpdateItemAbilityAnimatorParameters(false);
        }

        /// <summary>
        /// The character has started or stopped moving.
        /// </summary>
        /// <param name="moving">True if the character has started to move.</param>
        private void OnMoving(bool moving)
        {
            SetMovingParameter(moving);
        }
        
        /// <summary>
        /// The character has started or stopped aiming.
        /// </summary>
        /// <param name="aiming">Has the character started to aim?</param>
        private void OnAiming(bool aiming)
        {
            SetAimingParameter(aiming);
        }

        /// <summary>
        /// Updates the ability and item ability parameters if they are dirty.
        /// </summary>
        private void UpdateDirtyAbilityAnimatorParameters()
        {
            if (m_DirtyAbilityParameters) {
                UpdateAbilityAnimatorParameters(true);
            }
            if (m_DirtyItemAbilityParameters) {
                UpdateItemAbilityAnimatorParameters(true);
            }
        }

        /// <summary>
        /// Sets the ability animator parameters to the ability with the highest priority.
        /// </summary>
        public void UpdateAbilityAnimatorParameters()
        {
            UpdateAbilityAnimatorParameters(false);
        }

        /// <summary>
        /// Sets the ability animator parameters to the ability with the highest priority.
        /// </summary>
        /// <param name="immediateUpdate">Should the parameters be updated immediately?</param>
        public void UpdateAbilityAnimatorParameters(bool immediateUpdate = false)
        {
            // Wait to update until the proper time so the animator synchronizes properly.
            if (!immediateUpdate) {
                m_DirtyAbilityParameters = true;
                return;
            }
            m_DirtyAbilityParameters = false;

            int abilityIndex = 0, intData = 0;
            var floatData = 0f;
            bool setAbilityIndex = true, setStateIndex = true, setAbilityFloatData = true;
            for (int i = 0; i < m_CharacterLocomotion.ActiveAbilityCount; ++i) {
                if (setAbilityIndex && m_CharacterLocomotion.ActiveAbilities[i].AbilityIndexParameter != -1) {
                    abilityIndex = m_CharacterLocomotion.ActiveAbilities[i].AbilityIndexParameter;
                    setAbilityIndex = false;
                }
                if (setStateIndex && m_CharacterLocomotion.ActiveAbilities[i].AbilityIntData != -1) {
                    intData = m_CharacterLocomotion.ActiveAbilities[i].AbilityIntData;
                    setStateIndex = false;
                }
                if (setAbilityFloatData && m_CharacterLocomotion.ActiveAbilities[i].AbilityFloatData != -1) {
                    floatData = m_CharacterLocomotion.ActiveAbilities[i].AbilityFloatData;
                    setAbilityFloatData = false;
                }
            }
            SetAbilityIndexParameter(abilityIndex);
            SetAbilityIntDataParameter(intData);
            SetAbilityFloatDataParameter(floatData, m_CharacterLocomotion.TimeScale);
        }

        /// <summary>
        /// Sets the item animator parameters to the item ability with the highest priority.
        /// </summary>
        /// <param name="forceChange">Should the parameters be forced to be updated?</param>
        public void ExternalUpdateItemAbilityAnimatorParameters(bool forceChange)
        {
            UpdateItemAbilityAnimatorParameters(false, forceChange);
        }

        /// <summary>
        /// Sets the item animator parameters to the item ability with the highest priority.
        /// </summary>
        /// <param name="immediateUpdate">Should the parameters be updated immediately?</param>
        /// <param name="forceChange">Force the trigger to be changed?</param>
        public void UpdateItemAbilityAnimatorParameters(bool immediateUpdate = false, bool forceChange = false)
        {
            if (!m_HasItemParameters) {
                return;
            }

            // Wait to update until the proper time so the animator synchronizes properly.
            if (!immediateUpdate) {
                m_DirtyItemAbilityParameters = true;
                if (forceChange) {
                    m_DirtyItemAbilityParametersForceChange = true;
                }
                return;
            }

            forceChange = forceChange | m_DirtyItemAbilityParametersForceChange;
            m_DirtyItemAbilityParameters = false;
            m_DirtyItemAbilityParametersForceChange = false;

            // Reset the dirty parmaeters for the next use.
            for (int i = 0; i < m_ItemSlotSubstateIndex.Length; ++i) {
                m_DirtyItemStateIndexParameters[i] = m_DirtyItemSubstateIndexParameters[i] = false;
            }

            // The value can only be assigned if it hasn't already been assigned.
            int value;
            for (int i = 0; i < m_CharacterLocomotion.ActiveItemAbilityCount; ++i) {
                for (int j = 0; j < m_ItemSlotSubstateIndex.Length; ++j) {
                    if (!m_DirtyItemStateIndexParameters[j] && (value = m_CharacterLocomotion.ActiveItemAbilities[i].GetItemStateIndex(j)) != -1) {
                        m_DirtyItemStateIndexParameters[j] = true;
                        SetItemStateIndexParameter(j, value, forceChange);
                    }
                    if (!m_DirtyItemSubstateIndexParameters[j] && (value = m_CharacterLocomotion.ActiveItemAbilities[i].GetItemSubstateIndex(j)) != -1) {
                        m_DirtyItemSubstateIndexParameters[j] = true;
                        SetItemSubstateIndexParameter(j, value, forceChange);
                    }
                }
            }

            // The parameter may need to be reset to the default value.
            for (int i = 0; i < m_ItemSlotSubstateIndex.Length; ++i) {
                if (!m_DirtyItemStateIndexParameters[i]) { 
                    SetItemStateIndexParameter(i, 0, forceChange);
                }

                if (!m_DirtyItemSubstateIndexParameters[i]) {
                    SetItemSubstateIndexParameter(i, 0, forceChange);
                }
            }

            SetAimingParameter(m_Aiming);
        }

        /// <summary>
        /// Updates the ItemID and MovementSetID parameters to the equipped items.
        /// </summary>
        public void UpdateItemIDParameters()
        {
            if (m_DirtyEquippedItems) {
                var movementSetID = 0;
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    var itemID = 0;
                    if (m_EquippedItems[i] != null) {
                        if (m_EquippedItems[i].DominantItem) {
                            movementSetID = m_EquippedItems[i].AnimatorMovementSetID;
                        }
                        itemID = m_EquippedItems[i].AnimatorItemID;
                    }
                    SetItemIDParameter(i, itemID);
                }
                SetMovementSetIDParameter(movementSetID);
                m_DirtyEquippedItems = false;
            }
        }

        /// <summary>
        /// The specified item will be equipped.
        /// </summary>
        /// <param name="characterItem">The item that will be equipped.</param>
        /// <param name="slotID">The slot that the item will occupy.</param>
        private void OnWillEquipItem(CharacterItem characterItem, int slotID)
        {
            m_EquippedItems[slotID] = characterItem;
            m_DirtyEquippedItems = true;
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="characterItem">The item that was unequipped.</param>
        /// <param name="slotID">The slot that the item was unequipped from.</param>
        private void OnUnequipItem(CharacterItem characterItem, int slotID)
        {
            if (characterItem != m_EquippedItems[slotID]) {
                return;
            }

            SetItemIDParameter(slotID, 0);
            m_EquippedItems[slotID] = null;
            m_DirtyEquippedItems = true;
        }

        /// <summary>
        /// The character's model has switched.
        /// </summary>
        /// <param name="activeModel">The active character model.</param>
        private void OnSwitchModels(GameObject activeModel)
        {
            if (activeModel == gameObject) {
                m_CharacterLocomotion.OnAnimationUpdate += UpdateAnimatorParameters;
            } else {
                m_CharacterLocomotion.OnAnimationUpdate -= UpdateAnimatorParameters;
            }
        }

        /// <summary>
        /// The character's local timescale has changed.
        /// </summary>
        /// <param name="timeScale">The new timescale.</param>
        private void OnChangeTimeScale(float timeScale)
        {
            m_Animator.speed = timeScale;
        }

        /// <summary>
        /// Enables or disables the Animator.
        /// </summary>
        /// <param name="enable">Should the animator be enabled?</param>
        public void EnableAnimator(bool enable)
        {
            m_Animator.enabled = enable;
        }

        /// <summary>
        /// Copies the Animator parameters from the target Animator Monitor.
        /// </summary>
        /// <param name="targetAnimatorMonitor">The Aniator Monitor whose values should be copied.</param>
        public void CopyParameters(AnimatorMonitor targetAnimatorMonitor)
        {
            m_HorizontalMovement = targetAnimatorMonitor.HorizontalMovement;
            m_ForwardMovement = targetAnimatorMonitor.ForwardMovement;
            m_Pitch = targetAnimatorMonitor.Pitch;
            m_Yaw = targetAnimatorMonitor.Yaw;
            m_Speed = targetAnimatorMonitor.Speed;
            m_Height = targetAnimatorMonitor.Height;
            m_Moving = targetAnimatorMonitor.Moving;
            m_Aiming = targetAnimatorMonitor.Aiming;
            m_MovementSetID = targetAnimatorMonitor.MovementSetID;
            m_AbilityIndex = targetAnimatorMonitor.AbilityIndex;
            m_AbilityIntData = targetAnimatorMonitor.AbilityIntData;
            m_AbilityFloatData = targetAnimatorMonitor.AbilityFloatData;

            if (m_HasItemParameters && targetAnimatorMonitor.HasItemParameters) {
                for (int i = 0; i < m_EquippedItems.Length; ++i) {
                    if (!m_ItemParameterExists.Contains(i) || targetAnimatorMonitor.ItemSlotID.Length <= i) {
                        continue;
                    }
                    m_ItemSlotID[i] = targetAnimatorMonitor.ItemSlotID[i];
                    m_ItemSlotStateIndex[i] = targetAnimatorMonitor.ItemSlotStateIndex[i];
                    m_ItemSlotSubstateIndex[i] = targetAnimatorMonitor.ItemSlotSubstateIndex[i];
                }
            }

            SnapAnimator(false);
        }

        /// <summary>
        /// Root motion has moved the character.
        /// </summary>
        private void OnAnimatorMove()
        {
            m_CharacterLocomotion.UpdateRootMotion(m_Animator.deltaPosition, m_Animator.deltaRotation);
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            m_CharacterLocomotion.OnAnimationUpdate -= UpdateAnimatorParameters;

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterMoving", OnMoving);
            EventHandler.UnregisterEvent<Abilities.Ability, bool>(m_GameObject, "OnCharacterAbilityActive", OnAbilityActive);
            EventHandler.UnregisterEvent<Abilities.Items.ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterUpdateAbilityParameters", UpdateAbilityAnimatorParameters);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterUpdateItemAbilityParameters", ExternalUpdateItemAbilityAnimatorParameters);
            EventHandler.UnregisterEvent<CharacterItem, int>(m_GameObject, "OnAbilityWillEquipItem", OnWillEquipItem);
            EventHandler.UnregisterEvent<CharacterItem, int>(m_GameObject, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.UnregisterEvent<CharacterItem, int>(m_GameObject, "OnInventoryRemoveItem", OnUnequipItem);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnAimAbilityAim", OnAiming);
            if (m_Animator != null) {
                EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterSnapAnimator", SnapAnimator);
                EventHandler.UnregisterEvent<GameObject>(m_GameObject, "OnCharacterSwitchModels", OnSwitchModels);
                EventHandler.UnregisterEvent<float>(m_GameObject, "OnCharacterChangeTimeScale", OnChangeTimeScale);
            }
        }

        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void DomainReset()
        {
            s_ItemSlotIDHash = null;
            s_ItemSlotStateIndexHash = null;
            s_ItemSlotStateIndexChangeHash = null;
            s_ItemSlotSubstateIndexHash = null;
        }
    }
}