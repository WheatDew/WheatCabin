/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.Shared.Networking;
#endif
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Character.Effects;
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using Opsive.UltimateCharacterController.Events;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking.Character;
#endif
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The UltimateCharacterLocomotion component extends the CharacterLocomotion functionality by handling the following features:
    /// - Movement Types
    /// - Abilities
    /// - Effects
    /// </summary>
    public class UltimateCharacterLocomotion : CharacterLocomotion, Shared.Character.ICharacter
    {
        [Tooltip("The name of the state that should be activated when the character is in a first person perspective.")]
        [SerializeField] [StateName] protected string m_FirstPersonStateName = "FirstPerson";
        [Tooltip("The name of the state that should be activated when the character is in a third person perspective.")]
        [SerializeField] [StateName] protected string m_ThirdPersonStateName = "ThirdPerson";
        [Tooltip("The name of the state that should be activated when the character is moving.")]
        [SerializeField] [StateName] protected string m_MovingStateName = "Moving";
        [Tooltip("The name of the state that should be activated when the character is airborne.")]
        [SerializeField] [StateName] protected string m_AirborneStateName = "Airborne";

        [Tooltip("The full name of the active movement type.")]
        [SerializeField] protected string m_MovementTypeFullName;
        [Tooltip("The name of the active first person movement type.")]
        [SerializeField] protected string m_FirstPersonMovementTypeFullName;
        [Tooltip("The name of the active third person movement type.")]
        [SerializeField] protected string m_ThirdPersonMovementTypeFullName;

        [Tooltip("A reference to the Movement Types that the character can use for move direction and rotation.")]
        [SerializeReference] protected MovementType[] m_MovementTypes;
        [Tooltip("A reference to the Abilities that provide extra motor functionality.")]
        [SerializeReference] [Shared.Utility.IgnoreReferences] protected Ability[] m_Abilities;
        [Tooltip("A reference to the Item Abilities that provide extra item functionality.")]
        [SerializeReference] [Shared.Utility.IgnoreReferences] protected ItemAbility[] m_ItemAbilities;
        [Tooltip("A reference to the Effects that provide extra functionality.")]
        [SerializeReference] [Shared.Utility.IgnoreReferences] protected Effect[] m_Effects;

        [Tooltip("Unity event invoked when an ability has been started or stopped.")]
        [SerializeField] protected UnityMovementTypeBoolEvent m_OnMovementTypeActiveEvent;
        [Tooltip("Unity event invoked when an movement type has been started or stopped.")]
        [SerializeField] protected UnityAbilityBoolEvent m_OnAbilityActiveEvent;
        [Tooltip("Unity event invoked when an item ability has been started or stopped.")]
        [SerializeField] protected UnityItemAbilityBoolEvent m_OnItemAbilityActiveEvent;
        [Tooltip("Unity event invoked when the character has changed grounded state.")]
        [SerializeField] protected UnityBoolEvent m_OnGroundedEvent;
        [Tooltip("Unity event invoked when the character has landed on the ground.")]
        [SerializeField] protected UnityFloatEvent m_OnLandEvent;
        [Tooltip("Unity event invoked when the time scale has changed.")]
        [SerializeField] protected UnityFloatEvent m_OnChangeTimeScaleEvent;
        [Tooltip("Unity event invoked when the moving platforms have changed.")]
        [SerializeField] protected UnityTransformEvent m_OnChangeMovingPlatformsEvent;

        public GameObject GameObject => gameObject;
        public string FirstPersonStateName { get { return m_FirstPersonStateName; } set { m_FirstPersonStateName = value; } }
        public string ThirdPersonStateName { get { return m_ThirdPersonStateName; } set { m_ThirdPersonStateName = value; } }
        public string MovingStateName { get { return m_MovingStateName; } set { m_MovingStateName = value; } }
        public string AirborneStateName { get { return m_AirborneStateName; } set { m_AirborneStateName = value; } }

        public string MovementTypeFullName { get { return m_MovementTypeFullName; } set { SetMovementType(value); } }
        public string FirstPersonMovementTypeFullName
        {
            get { return m_FirstPersonMovementTypeFullName; }
            set
            {
                if (m_FirstPersonMovementTypeFullName != value) {
                    if (!string.IsNullOrEmpty(value) && Application.isPlaying && m_FirstPersonPerspective) {
                        SetMovementType(value);
                    } else {
                        m_FirstPersonMovementTypeFullName = value;
                    }
                }
            }
        }
        public string ThirdPersonMovementTypeFullName
        {
            get { return m_ThirdPersonMovementTypeFullName; }
            set
            {
                if (m_ThirdPersonMovementTypeFullName != value) {
                    if (!string.IsNullOrEmpty(value) && Application.isPlaying && !m_FirstPersonPerspective) {
                        SetMovementType(value);
                    } else {
                        m_ThirdPersonMovementTypeFullName = value;
                    }
                }
            }
        }
        public MovementType[] MovementTypes
        {
            get { return m_MovementTypes; }
            set
            {
                m_MovementTypes = value;
                if (Application.isPlaying && m_MovementTypes != null) {
                    m_MovementTypeNameMap.Clear();
                    UnityEngineUtility.RemoveNullElements(ref m_MovementTypes);
                    for (int i = 0; i < m_MovementTypes.Length; ++i) {
                        m_MovementTypeNameMap.Add(m_MovementTypes[i].GetType().FullName, i);
                    }
                }
            }
        }
        public Ability[] Abilities
        {
            get { return m_Abilities; }
            set
            {
                m_Abilities = value;
                if (Application.isPlaying && m_Abilities != null) {
                    if (m_ActiveAbilities == null) {
                        m_ActiveAbilities = new Ability[m_Abilities.Length];
                    } else {
                        System.Array.Resize(ref m_ActiveAbilities, m_Abilities.Length);
                    }

                    // The ability can be added after the character has already been initialized.
                    for (int i = 0; i < m_Abilities.Length; ++i) {
                        if (m_Abilities[i].Index == -1) {
                            m_Abilities[i].Initialize(this, i);
                            m_Abilities[i].Awake();

                            // The MoveTowards and ItemEquipVerifier abilities are a special type of ability in that it is started by the controller.
                            if (m_Abilities[i] is MoveTowards) {
                                m_MoveTowardsAbility = m_Abilities[i] as MoveTowards;
                            } else if (m_Abilities[i] is ItemEquipVerifier) {
                                m_ItemEquipVerifierAbility = m_Abilities[i] as ItemEquipVerifier;
                            }
                        }
                    }
                }
            }
        }
        public ItemAbility[] ItemAbilities
        {
            get { return m_ItemAbilities; }
            set
            {
                m_ItemAbilities = value;
                if (Application.isPlaying && m_ItemAbilities != null) {
                    if (m_ActiveItemAbilities == null) {
                        m_ActiveItemAbilities = new ItemAbility[m_ItemAbilities.Length];
                    } else {
                        System.Array.Resize(ref m_ActiveItemAbilities, m_ItemAbilities.Length);
                    }

                    // The ability can be added after the character has already been initialized.
                    for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                        if (m_ItemAbilities[i].Index == -1) {
                            m_ItemAbilities[i].Initialize(this, i);
                            m_ItemAbilities[i].Awake();
                        }
                    }
                }
            }
        }
        public Effect[] Effects
        {
            get { return m_Effects; }
            set
            {
                m_Effects = value;
                if (Application.isPlaying && m_Effects != null) {
                    if (m_ActiveEffects == null) {
                        m_ActiveEffects = new Effect[m_Effects.Length];
                    } else {
                        System.Array.Resize(ref m_ActiveEffects, m_Effects.Length);
                    }

                    // The effect can be added after the character has already been initialized.
                    for (int i = 0; i < m_Effects.Length; ++i) {
                        if (m_Effects[i].Index == -1) {
                            m_Effects[i].Initialize(this, i);
                            m_Effects[i].Awake();
                        }
                    }
                }
            }
        }
        public UnityMovementTypeBoolEvent OnMovementTypeActiveEvent { get { return m_OnMovementTypeActiveEvent; } set { m_OnMovementTypeActiveEvent = value; } }
        public UnityAbilityBoolEvent OnAbilityActiveEvent { get { return m_OnAbilityActiveEvent; } set { m_OnAbilityActiveEvent = value; } }
        public UnityItemAbilityBoolEvent OnItemAbilityActiveEvent { get { return m_OnItemAbilityActiveEvent; } set { m_OnItemAbilityActiveEvent = value; } }
        public UnityBoolEvent OnGroundedEvent { get { return m_OnGroundedEvent; } set { m_OnGroundedEvent = value; } }
        public UnityFloatEvent OnLandEvent { get { return m_OnLandEvent; } set { m_OnLandEvent = value; } }
        public UnityFloatEvent OnChangeTimeScaleEvent { get { return m_OnChangeTimeScaleEvent; } set { m_OnChangeTimeScaleEvent = value; } }
        public UnityTransformEvent OnChangeMovingPlatformsEvent { get { return m_OnChangeMovingPlatformsEvent; } set { m_OnChangeMovingPlatformsEvent = value; } }

        [System.NonSerialized] private GameObject m_GameObject;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private INetworkInfo m_NetworkInfo;
        private INetworkCharacter m_NetworkCharacter;
#endif

        private MovementType m_ActiveMovementType;
        private Dictionary<string, int> m_MovementTypeNameMap = new Dictionary<string, int>();
        private bool m_FirstPersonPerspective;
        private ILookSource m_LookSource;

        private Ability[] m_ActiveAbilities;
        private int m_ActiveAbilityCount;
        private ItemAbility[] m_ActiveItemAbilities;
        private int m_ActiveItemAbilityCount;
        private Effect[] m_ActiveEffects;
        private int m_ActiveEffectsCount;

        private MoveTowards m_MoveTowardsAbility;
        private ItemEquipVerifier m_ItemEquipVerifierAbility;

        private Vector2 m_RawInputVector;
        private bool m_Moving;
        private float m_MaxHeight;
        private Vector3 m_MaxHeightPosition;
        private bool m_Alive;

        private System.Action m_OnAnimationUpdate;
        public System.Action OnAnimationUpdate { get => m_OnAnimationUpdate; set => m_OnAnimationUpdate = value; }

        public Ability[] ActiveAbilities => m_ActiveAbilities;
        public int ActiveAbilityCount => m_ActiveAbilityCount;
        public ItemAbility[] ActiveItemAbilities => m_ActiveItemAbilities;
        public int ActiveItemAbilityCount => m_ActiveItemAbilityCount;
        public Effect[] ActiveEffects => m_ActiveEffects;
        public int ActiveEffectsCount => m_ActiveEffectsCount;
        public MovementType ActiveMovementType { get => m_ActiveMovementType; set => m_ActiveMovementType = value; }
        [Shared.Utility.NonSerialized] public bool FirstPersonPerspective { get { return m_FirstPersonPerspective; } set { m_FirstPersonPerspective = value; } }
        public ILookSource LookSource { get => m_LookSource; }

        public MoveTowards MoveTowardsAbility { get { return m_MoveTowardsAbility; } }
        public ItemEquipVerifier ItemEquipVerifierAbility { get { return m_ItemEquipVerifierAbility; } }
        public Vector2 RawInputVector { get => m_RawInputVector; set => m_RawInputVector = value; }
        public override float TimeScale
        {
            get { return base.TimeScale; }
            set {
                // Override the TimeScale setter to allow an event to be sent when the time scale changes.
                if (base.TimeScale != value) {
                    if (base.TimeScale == 0 && value != 0) {
                        EventHandler.ExecuteEvent(m_GameObject, "OnEnableGameplayInput", true);
                    } else if (base.TimeScale != 0 && value == 0) {
                        EventHandler.ExecuteEvent(m_GameObject, "OnEnableGameplayInput", false);
                    }
                    EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangeTimeScale", value);
                    if (m_OnChangeTimeScaleEvent != null) {
                        m_OnChangeTimeScaleEvent.Invoke(value);
                    }
                }
                base.TimeScale = value;
            }
        }
        [Shared.Utility.NonSerialized]
        public bool Moving
        {
            get { return m_Moving || m_RawInputVector.sqrMagnitude > 0.001f || m_InputVector.sqrMagnitude > 0.001f; }
            set
            {
                if (m_Moving != value) {
                    m_Moving = value;
                    EventHandler.ExecuteEvent(m_GameObject, "OnCharacterMoving", m_Moving);
                    if (!string.IsNullOrEmpty(m_MovingStateName)) {
                        StateManager.SetState(m_GameObject, m_MovingStateName, m_Moving);
                    }
                }
            }
        }
        public bool Alive { get => m_Alive; }

        /// <summary>
        /// Cache the component references and initialize the default values.
        /// </summary>
        protected override void AwakeInternal()
        {
            m_GameObject = gameObject;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = m_GameObject.GetCachedComponent<INetworkInfo>();
            m_NetworkCharacter = m_GameObject.GetCachedComponent<INetworkCharacter>();
#endif

            base.AwakeInternal();

            m_MaxHeight = float.NegativeInfinity;
            m_MaxHeightPosition = m_Rigidbody.position;
            m_Alive = true;

            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterChangePerspectives", OnChangePerspectives);
            EventHandler.RegisterEvent<GameObject>(m_GameObject, "OnCharacterSwitchModels", OnSwitchModels);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);

            // Create any abilities, effects, and movement types from the serialized data.
            InitializeEffects();
            InitializeAbilities();
            InitializeItemAbilities();
            InitializeMovementTypes();
        }

        /// <summary>
        /// Initialize the effects.
        /// </summary>
        public void InitializeEffects()
        {
            if (m_Effects == null) {
                return;
            }

            // Do a sanity check to ensure none of the effects are null.
            UnityEngineUtility.RemoveNullElements(ref m_Effects);

            m_ActiveEffects = new Effect[m_Effects.Length];
            for (int i = 0; i < m_Effects.Length; ++i) {
                m_Effects[i].Initialize(this, i);
                m_Effects[i].Awake();
            }
        }

        /// <summary>
        /// Initialize the abilities.
        /// </summary>
        public void InitializeAbilities()
        {
            if (m_Abilities == null) {
                return;
            }

            // Do a sanity check to ensure none of the abilities are null.
            UnityEngineUtility.RemoveNullElements(ref m_Abilities);

            m_ActiveAbilities = new Ability[m_Abilities.Length];
            for (int i = 0; i < m_Abilities.Length; ++i) {
                m_Abilities[i].Initialize(this, i);
                m_Abilities[i].Awake();

                // The MoveTowards and ItemEquipVerifier abilities are a special type of ability in that it is started by the controller.
                if (m_Abilities[i] is MoveTowards) {
                    m_MoveTowardsAbility = m_Abilities[i] as MoveTowards;
                } else if (m_Abilities[i] is ItemEquipVerifier) {
                    m_ItemEquipVerifierAbility = m_Abilities[i] as ItemEquipVerifier;
                }
            }
        }

        /// <summary>
        /// Initialize the item abilities.
        /// </summary>
        public void InitializeItemAbilities()
        {
            if (m_ItemAbilities == null) {
                return;
            }

            // Do a sanity check to ensure none of the item abilities are null.
            UnityEngineUtility.RemoveNullElements(ref m_ItemAbilities);

            m_ActiveItemAbilities = new ItemAbility[m_ItemAbilities.Length];
            for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                m_ItemAbilities[i].Initialize(this, i);
                m_ItemAbilities[i].Awake();
            }
        }

        /// <summary>
        /// Initialize the movement types.
        /// </summary>
        public void InitializeMovementTypes()
        {
            if (m_MovementTypes == null) {
                return;
            }

            m_MovementTypeNameMap.Clear();

            // Do a sanity check to ensure none of the movement types are null.
            UnityEngineUtility.RemoveNullElements(ref m_MovementTypes);

            for (int i = 0; i < m_MovementTypes.Length; ++i) {
                m_MovementTypeNameMap.Add(m_MovementTypes[i].GetType().FullName, i);
                m_MovementTypes[i].Initialize(this);
                m_MovementTypes[i].Awake();
            }

            SetMovementType(m_MovementTypeFullName);
        }

        /// <summary>
        /// Sets the movement type to the object with the specified type which should be set.
        /// </summary>
        /// <param name="typeName">The type name of the MovementType which should be set.</param>
        private void SetMovementType(string typeName)
        {
            SetMovementType(Shared.Utility.TypeUtility.GetType(typeName));
        }

        /// <summary>
        /// Sets the movement type to the object with the specified type.
        /// </summary>
        /// <param name="type">The type of the MovementType which should be set.</param>
        public void SetMovementType(System.Type type)
        {
            if (type == null || (m_ActiveMovementType != null && m_ActiveMovementType.GetType() == type)) {
                return;
            }

            m_MovementTypeFullName = type.FullName;

            if (!Application.isPlaying) {
                return;
            }

            int index;
            if (!m_MovementTypeNameMap.TryGetValue(type.FullName, out index)) {
                Debug.LogError($"Error: Unable to find the movement type with name {type.FullName}.", this);
                return;
            }

            // Notify the previous movement type that it is no longer active.
            if (m_ActiveMovementType != null) {
                m_ActiveMovementType.ChangeMovementType(false);

                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangeMovementType", m_ActiveMovementType, false);

                if (m_OnMovementTypeActiveEvent != null) {
                    m_OnMovementTypeActiveEvent.Invoke(m_ActiveMovementType, false);
                }
            }

            m_ActiveMovementType = m_MovementTypes[index];

            // Notify the current movement type that is now active.
            if (m_ActiveMovementType.FirstPersonPerspective) {
                m_FirstPersonMovementTypeFullName = m_MovementTypeFullName;
            } else {
                m_ThirdPersonMovementTypeFullName = m_MovementTypeFullName;
            }
            m_ActiveMovementType.ChangeMovementType(true);

            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangeMovementType", m_ActiveMovementType, true);

            if (m_OnMovementTypeActiveEvent != null) {
                m_OnMovementTypeActiveEvent.Invoke(m_ActiveMovementType, true);
            }
        }

        /// <summary>
        /// Internal method which enables the character.
        /// </summary>
        protected override void OnEnableInternal()
        {
            base.OnEnableInternal();

            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterMoving", false);
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", true);
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterActivate", true);
        }

        /// <summary>
        /// Starts the abilities.
        /// </summary>
        public virtual void Start()
        {
            if (m_Abilities != null) {
                for (int i = 0; i < m_Abilities.Length; ++i) {
                    m_Abilities[i].Start();
                }
            }
            if (m_ItemAbilities != null) {
                for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                    m_ItemAbilities[i].Start();
                }
            }
            if (m_Effects != null) {
                for (int i = 0; i < m_Effects.Length; ++i) {
                    m_Effects[i].Start();
                }
            }

            // Do a pass on trying to start any abilities in case they should be started on the first frame.
            EnableColliderCollisionLayer(false);
            UpdateAbilities(m_Abilities);
            UpdateAbilities(m_ItemAbilities);
            EnableColliderCollisionLayer(true);
            if (m_OnAnimationUpdate != null) {
                m_OnAnimationUpdate();
            }

            // The abilities may have updated the animator.
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterSnapAnimator", true);

            // The character isn't moving at the start.
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterMoving", false);
            // Notify those interested in the time scale isn't set to 1 at the start. The TimeScale property will notify those interested of the change during runtime.
            if (m_TimeScale != 1) {
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangeTimeScale", m_TimeScale);
                if (m_OnChangeTimeScaleEvent != null) {
                    m_OnChangeTimeScaleEvent.Invoke(m_TimeScale);
                }
            }
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;

            if (m_LookSource != null && enabled) {
                // Do a pass on trying to start any abilities in case they should be started immediately when the look source is attached.
                UpdateAbilities(m_Abilities);
                UpdateAbilities(m_ItemAbilities);
            }

#if THIRD_PERSON_CONTROLLER
            var hasPerspectiveMonitor = m_GameObject.GetComponent<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>() != null;
#else
            var hasPerspectiveMonitor = false;
#endif
            // If the character doesn't have the PerspectiveMonitor then the perspective depends on the look source.
            if (!hasPerspectiveMonitor) {
                if (lookSource != null) {
                    var cameraController = lookSource as UltimateCharacterController.Camera.CameraController;
                    if (cameraController != null) {
                        m_FirstPersonPerspective = cameraController.ActiveViewType.FirstPersonPerspective;
                    } else {
                        m_FirstPersonPerspective = false;
                    }
                }
                EventHandler.ExecuteEvent<bool>(m_GameObject, "OnCharacterChangePerspectives", m_FirstPersonPerspective);
            }
        }

        /// <summary>
        /// Moves and rotates the character.
        /// </summary>
        protected override void UpdateCharacter()
        {
            // The MovementType may change the InputVector.
            m_RawInputVector = m_InputVector;

            // Abilities may disallow input.
            bool allowPositionalInput, allowRotationalInput;
            AbilitiesAllowInput(out allowPositionalInput, out allowRotationalInput);
            if (allowPositionalInput) {
                // Positional input is allowed - use the movement type to determine how the character should move.
                m_InputVector = m_ActiveMovementType.GetInputVector(m_InputVector);
            } else {
                m_InputVector = Vector2.zero;
            }
            if (!allowRotationalInput) {
                m_DeltaRotation = Vector3.zero;
            }

            // Start and update the abilities.
            UpdateAbilities(m_Abilities);
            UpdateAbilities(m_ItemAbilities);

            // Update the effects.
            for (int i = 0; i < m_ActiveEffectsCount; ++i) {
                m_ActiveEffects[i].Update();
            }

            if (m_Moving != (m_InputVector.sqrMagnitude > 0.001f)) {
                Moving = !m_Moving;
            }

            base.UpdateCharacter();

            // Update the animations.
            if (OnAnimationUpdate != null) {
                m_OnAnimationUpdate();
            }

            // Allow the abilities to update after the character has been moved.
            LateUpdateActiveAbilities(m_ActiveAbilities, ref m_ActiveAbilityCount);
            LateUpdateActiveAbilities(m_ActiveItemAbilities, ref m_ActiveItemAbilityCount);
        }

        /// <summary>
        /// Do the abilities allow positional and rotational input?
        /// </summary>
        /// <param name="allowPositionalInput">A reference to a bool which indicates if the abilities allow positional input.</param>
        /// <param name="allowRotationalInput">A reference to a bool which indicates if the abilities allow rotational input.</param>
        private void AbilitiesAllowInput(out bool allowPositionalInput, out bool allowRotationalInput)
        {
            allowPositionalInput = allowRotationalInput = true;
            // Check the abilities to see if any disallow input.
            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                if (!m_ActiveAbilities[i].AllowPositionalInput) {
                    allowPositionalInput = false;
                }
                if (!m_ActiveAbilities[i].AllowRotationalInput) {
                    allowRotationalInput = false;
                }
            }

            // If neither the positional or rotational input is allowed then the item abilities don't need to be checked.
            if (!allowPositionalInput && !allowRotationalInput) {
                return;
            }

            // Check the item abilities to see if any disallow input.
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                if (!m_ActiveItemAbilities[i].AllowPositionalInput) {
                    allowPositionalInput = false;
                }
                if (!m_ActiveItemAbilities[i].AllowRotationalInput) {
                    allowRotationalInput = false;
                }
            }
        }

        /// <summary>
        /// Try to start an automatic inactive ability and also try to stop an automatic active ability. The Update or InactiveUpdate will also be called.
        /// </summary>
        /// <param name="abilities">An array of all of the abilities.</param>
        private void UpdateAbilities(Ability[] abilities)
        {
            if (abilities != null) {
                for (int i = 0; i < abilities.Length; ++i) {
                    if (!abilities[i].IsActive) {
                        if (abilities[i].StartType == Ability.AbilityStartType.Automatic) {
                            TryStartAbility(abilities[i]);
                        } else if (abilities[i].StartType != Ability.AbilityStartType.Manual && abilities[i].CheckForAbilityMessage &&
                                    (m_MoveTowardsAbility == null || !m_MoveTowardsAbility.IsActive)) {
                            // The ability message can show if the non-automatic/manual ability can start.
                            abilities[i].AbilityMessageCanStart = abilities[i].Enabled && abilities[i].CanStartAbility();
                        }
                    } else if (abilities[i].IsActive && abilities[i].StopType == Ability.AbilityStopType.Automatic) {
                        TryStopAbility(abilities[i]);
                    }
                    if (abilities[i].IsActive) {
                        abilities[i].Update();
                    } else if (abilities[i].Enabled) {
                        abilities[i].InactiveUpdate();
                    }
                }
            }
        }

        /// <summary>
        /// Calls LateUpdate on the active abilities.
        /// </summary>
        /// <param name="abilities">An array of all of the abilities.</param>
        /// <param name="abilityCount">The number of active abilities.</param>
        private void LateUpdateActiveAbilities(Ability[] abilities, ref int abilityCount)
        {
            if (abilities != null) {
                for (int i = 0; i < abilityCount; ++i) {
                    abilities[i].LateUpdate();
                }
            }
        }

        /// <summary>
        /// Updates the character's rotation. The DesiredRotation will be set based on the root motion/input values.
        /// </summary>
        protected override void UpdateRotation()
        {
            base.UpdateRotation();

            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                m_ActiveAbilities[i].UpdateRotation();
            }
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                m_ActiveItemAbilities[i].UpdateRotation();
            }
        }

        /// <summary>
        /// Applies the desired rotation to the transform.
        /// </summary>
        protected override void ApplyRotation()
        {
            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                m_ActiveAbilities[i].ApplyRotation();
            }
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                m_ActiveItemAbilities[i].ApplyRotation();
            }

            base.ApplyRotation();
        }

        /// <summary>
        /// Updates the character's position. The DesiredMovement will be set based on the root motion/input values.
        /// </summary>
        protected override void UpdatePosition()
        {
            base.UpdatePosition();

            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                m_ActiveAbilities[i].UpdatePosition();
            }
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                m_ActiveItemAbilities[i].UpdatePosition();
            }
        }

        /// <summary>
        /// Applies the desired movement to the transform.
        /// </summary>
        protected override void ApplyPosition()
        {
            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                m_ActiveAbilities[i].ApplyPosition();
            }
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                m_ActiveItemAbilities[i].ApplyPosition();
            }

            base.ApplyPosition();
        }

        /// <summary>
        /// Updates the desired movement value.
        /// </summary>
        protected override void UpdateDesiredMovement()
        {
            base.UpdateDesiredMovement();

            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                m_ActiveAbilities[i].UpdateDesiredMovement();
            }
            for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                m_ActiveItemAbilities[i].UpdateDesiredMovement();
            }
        }

        /// <summary>
        /// Updates the grounded state.
        /// </summary>
        /// <param name="grounded">Is the character grounded?</param>
        /// <param name="sendEvents">Should the events be sent if the grounded status changes?</param>
        /// <returns>True if the grounded state changed.</returns>
        protected override bool UpdateGroundState(bool grounded, bool sendEvents)
        {
            var groundedStatusChanged = base.UpdateGroundState(grounded, sendEvents);
            if (groundedStatusChanged) {
                // Notify interested objects of the ground change.
                if (sendEvents) {
                    EventHandler.ExecuteEvent<bool>(m_GameObject, "OnCharacterGrounded", grounded);
                    if (m_OnGroundedEvent != null) {
                        m_OnGroundedEvent.Invoke(grounded);
                    }
                }
                if (grounded) {
                    if (sendEvents && !float.IsNegativeInfinity(m_MaxHeight) && UsingGravity) {
                        var height = m_MaxHeight - m_Rigidbody.InverseTransformDirection(m_Rigidbody.position - m_MaxHeightPosition).y;
                        EventHandler.ExecuteEvent<float>(m_GameObject, "OnCharacterLand", height);
                        if (m_OnLandEvent != null) {
                            m_OnLandEvent.Invoke(height);
                        }
                    }
                } else {
                    m_MaxHeightPosition = m_Rigidbody.position;
                    m_MaxHeight = float.NegativeInfinity;
                }
            } else if (!grounded) {
                // Save out the max height of the character in the air so the fall height can be calculated.
                var height = m_Rigidbody.InverseTransformDirection(m_Rigidbody.position - m_MaxHeightPosition).y;
                if (height > m_MaxHeight) {
                    m_MaxHeightPosition = m_Rigidbody.position;
                    m_MaxHeight = height;
                }
            }
            // Set the airborne state if the grounded status has changed or no events are being sent. No events will be sent when the grounded status is initially checked.
            if ((groundedStatusChanged || !sendEvents) && !string.IsNullOrEmpty(m_AirborneStateName)) {
                StateManager.SetState(m_GameObject, m_AirborneStateName, !grounded);
            }
            return groundedStatusChanged;
        }

        /// <summary>
        /// Sets the moving platform to the specified transform.
        /// </summary>
        /// <param name="platform">The platform transform that should be set. Can be null.</param>
        /// <param name="platformOverride">Is the default moving platform logic being overridden?</param>
        /// <returns>True if the platform was changed.</returns>
        public override bool SetMovingPlatform(Transform movingPlatform, bool platformOverride = true)
        {
            var movingPlatformChanged = base.SetMovingPlatform(movingPlatform, platformOverride);
            if (movingPlatformChanged) {
                // Notify interested objects of the platform change.
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterChangeMovingPlatforms", m_MovingPlatform);
                if (m_OnChangeMovingPlatformsEvent != null) {
                    m_OnChangeMovingPlatformsEvent.Invoke(m_MovingPlatform);
                }
            }
            return movingPlatformChanged;
        }

        /// <summary>
        /// Tries to start the specified ability.
        /// </summary>
        /// <param name="ability">The ability to try to start.</param>
        /// <param name="ignorePriority">Should the ability priority be ignored?</param>
        /// <param name="ignoreCanStartCheck">Should the CanStartAbility check be ignored?</param>
        /// <returns>True if the ability was started.</returns>
        public bool TryStartAbility(Ability ability, bool ignorePriority = false, bool ignoreCanStartCheck = false)
        {
            if (ability == null) {
                return false;
            }

            // ItemAbilities have a different startup process than regular abilities.
            if (ability is ItemAbility) {
                return TryStartAbility(ability as ItemAbility, ignoreCanStartCheck);
            }

            // Start the ability if it is not active or can be started multiple times, enabled, and can be started.
            if ((!ability.IsActive || ability.CanReceiveMultipleStarts) && ability.Enabled && (ignoreCanStartCheck || ability.CanStartAbility())) {
                // The ability may already be active if the ability can receive multiple starts. Multiple starts are useful for item abilities that need to be active
                // over a period of time but can be updated with the input start type while active. A good example of this is the Toggle Equip Item ability. When
                // this ability starts it sets an Animator parameter to equip or unequip the item. The ability continues to run while equipping or unequipping the item
                // but it should trigger the reverse of the equip or unequip when another start is triggered.
                int index;
                if (!ability.IsActive) {
                    // The priority can be ignored if the ability should be force started.
                    if (!ignorePriority) {
                        // If the ability is not a concurrent ability then it can only be started if it has a lower index than any other active abilities.
                        if (!ability.IsConcurrent) {
                            for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                                var ignoreLocalPriority = m_ActiveAbilities[i].IgnorePriority && ability.IgnorePriority;
                                if (m_ActiveAbilities[i].IsConcurrent) {
                                    // The ability cannot be started if a concurrent ability is active and has a lower index.
                                    if (((!ignoreLocalPriority && m_ActiveAbilities[i].Index < ability.Index) || ignoreLocalPriority) && m_ActiveAbilities[i].ShouldBlockAbilityStart(ability)) {
                                        return false;
                                    }
                                } else {
                                    // The ability cannot be started if another ability is already active and has a lower index or if the active ability says the current ability cannot be started.
                                    if ((m_ActiveAbilities[i].Index < ability.Index && !ignoreLocalPriority) || m_ActiveAbilities[i].ShouldBlockAbilityStart(ability)) {
                                        return false;
                                    } else {
                                        // Stop any abilities that have a higher index to prevent two non-concurrent abilities from running at the same time.
                                        TryStopAbility(m_ActiveAbilities[i], true);
                                    }
                                }
                            }
                        }
                        // The ability cannot be started if the active ability says the current ability cannot be started.
                        for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                            if (m_ActiveAbilities[i].ShouldBlockAbilityStart(ability)) {
                                return false;
                            }
                        }
                        for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                            if (m_ActiveItemAbilities[i].ShouldBlockAbilityStart(ability)) {
                                return false;
                            }
                        }
                    }

                    // The ability can start. Stop any currently active abilities that should not be started because the current ability has started.
                    for (int i = m_ActiveAbilityCount - 1; i >= 0; --i) {
                        if (ability.ShouldStopActiveAbility(m_ActiveAbilities[i])) {
                            TryStopAbility(m_ActiveAbilities[i], true);
                        }
                    }
                    for (int i = m_ActiveItemAbilityCount - 1; i >= 0; --i) {
                        if (ability.ShouldStopActiveAbility(m_ActiveItemAbilities[i])) {
                            TryStopAbility(m_ActiveItemAbilities[i], true);
                        }
                    }

                    // Give the ability one more chance to initialize any variables or stop from starting. This is important for abilities that are being started
                    // after a period of time such as from the Move Towards ability or the Item Equip Verifier.
                    if (!ability.AbilityWillStart()) {
                        return false;
                    }

                    var moveEquipStarted = false;
                    // The ability may require the character to first move to a specific location before it can start.
                    if (!(ability is MoveTowards) && m_MoveTowardsAbility != null) {
                        // If StartMoving returns true then the MoveTowards ability has started and it will start the original
                        // ability after the character has arrived at the destination.
                        if (m_MoveTowardsAbility.StartMoving(ability.GetMoveTowardsLocations(), ability)) {
                            moveEquipStarted = true;
                        }
                    }

                    // The ability may first need to unequip any equipped items before it can start.
                    if (!(ability is ItemEquipVerifier) && m_ItemEquipVerifierAbility != null) {
                        // If TryToggleItem returns true then the ItemEquipVerifier ability has started and it will start the original ability after
                        // the character has finished unequipping the equipped items.
                        if (m_ItemEquipVerifierAbility.TryToggleItem(ability, true) && !ability.ImmediateStartItemVerifier) {
                            moveEquipStarted = true;
                        }
                    }

                    // Wait for the MoveTowards and ItemEquipVerifier abilities to end before starting the new ability.
                    if (moveEquipStarted) {
                        return true;
                    }

                    // Insert in the active abilities array according to priority.
                    index = m_ActiveAbilityCount;
                    for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                        if (m_ActiveAbilities[i].Index > ability.Index) {
                            index = i;
                            break;
                        }
                    }
                    // Make space for the new ability.
                    for (int i = m_ActiveAbilityCount; i > index; --i) {
                        m_ActiveAbilities[i] = m_ActiveAbilities[i - 1];
                        m_ActiveAbilities[i].ActiveIndex = i;
                    }

                    m_ActiveAbilities[index] = ability;
                    m_ActiveAbilityCount++;
                } else {
                    // The ability is already active - start it again for a multiple start.
                    index = ability.ActiveIndex;
                }

                // Execute the event before the ability is started in case the ability is stopped within the start.
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterAbilityActive", ability, true);
                if (m_OnAbilityActiveEvent != null) {
                    m_OnAbilityActiveEvent.Invoke(ability, true);
                }

                ability.StartAbility(index);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to start the specified item ability.
        /// </summary>
        /// <param name="itemAbility">The item ability to try to start.</param>
        /// <param name="ignoreCanStartCheck">Should the CanStartAbility check be ignored?</param>
        /// <returns>True if the ability was started.</returns>
        public bool TryStartAbility(ItemAbility itemAbility, bool ignoreCanStartCheck)
        {
            if (itemAbility == null) {
                return false;
            }

            // Start the ability if it is not active or can be started multiple times, enabled, and can be started.
            if ((!itemAbility.IsActive || itemAbility.CanReceiveMultipleStarts) && itemAbility.Enabled && (ignoreCanStartCheck || itemAbility.CanStartAbility())) {
                // The ability cannot be started if the active ability says the current ability cannot be started.
                for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                    if (m_ActiveItemAbilities[i].ShouldBlockAbilityStart(itemAbility)) {
                        return false;
                    }
                }
                for (int i = 0; i < m_ActiveAbilityCount; ++i) {
                    if (m_ActiveAbilities[i].ShouldBlockAbilityStart(itemAbility)) {
                        return false;
                    }
                }

                // The ability can start. Stop any currently active abilities that should not be started because the current ability has started.
                for (int i = m_ActiveItemAbilityCount - 1; i >= 0; --i) {
                    if (itemAbility.ShouldStopActiveAbility(m_ActiveItemAbilities[i])) {
                        TryStopAbility(m_ActiveItemAbilities[i], true);
                    }
                }
                for (int i = m_ActiveAbilityCount - 1; i >= 0; --i) {
                    if (itemAbility.ShouldStopActiveAbility(m_ActiveAbilities[i])) {
                        TryStopAbility(m_ActiveAbilities[i], true);
                    }
                }

                // The ability may already be active if the ability can receive multiple starts. Multiple starts are useful for item abilities that need to be active
                // over a period of time but can be updated with the input start type while active. A good example of this is the Toggle Equip Item ability. When
                // this ability starts it sets an Animator parameter to equip or unequip the item. The ability continues to run while equipping or unequipping the item
                // but it should trigger the reverse of the equip or unequip when another start is triggered.
                int index;
                if (!itemAbility.IsActive) {
                    // Notify the ability that it will start. This method isn't as useful for ItemAbilities because the ability will always be immediately started after this,
                    // but it is added for consistency with the ability system. 
                    if (!itemAbility.AbilityWillStart()) {
                        return false;
                    }

                    // Insert in the active abilities array according to priority.
                    index = m_ActiveItemAbilityCount;
                    for (int i = 0; i < m_ActiveItemAbilityCount; ++i) {
                        if (m_ActiveItemAbilities[i].Index > itemAbility.Index) {
                            index = i;
                            break;
                        }
                    }
                    // Make space for the new item ability.
                    for (int i = m_ActiveItemAbilityCount; i > index; --i) {
                        m_ActiveItemAbilities[i] = m_ActiveItemAbilities[i - 1];
                        m_ActiveItemAbilities[i].ActiveIndex = i;
                    }

                    m_ActiveItemAbilities[index] = itemAbility;
                    m_ActiveItemAbilityCount++;
                } else {
                    // The ability is already active - start it again for a multiple start.
                    index = itemAbility.ActiveIndex;
                }

                // Execute the event before the ability is started in case the ability is stopped within the start.
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterItemAbilityActive", itemAbility, true);
                if (m_OnItemAbilityActiveEvent != null) {
                    m_OnItemAbilityActiveEvent.Invoke(itemAbility, true);
                }

                itemAbility.StartAbility(index);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to stop the specified ability.
        /// </summary>
        /// <param name="ability">The ability to try to stop.</param>
        /// <param name="force">Should the ability be force stopped?</param>
        public bool TryStopAbility(Ability ability, bool force = false)
        {
            // The ability can't be stopped if it isn't active.
            if (ability == null || !ability.IsActive) {
                return false;
            }

            ability.WillTryStopAbility();

            // CanStopAbility can prevent the ability from stopping.
            if (!ability.CanStopAbility(force)) {
                return false;
            }

            // Update the active ability array by removing the stopped ability.
            if (ability is ItemAbility) {
                for (int i = ability.ActiveIndex; i < m_ActiveItemAbilityCount - 1; ++i) {
                    m_ActiveItemAbilities[i] = m_ActiveItemAbilities[i + 1];
                    m_ActiveItemAbilities[i].ActiveIndex = i;
                }
                m_ActiveItemAbilityCount--;
                m_ActiveItemAbilities[m_ActiveItemAbilityCount] = null;

                ability.StopAbility(force, true);

                // Let others know that the ability has stopped.
                var itemAbility = ability as ItemAbility;
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterItemAbilityActive", itemAbility, false);
                if (m_OnItemAbilityActiveEvent != null) {
                    m_OnItemAbilityActiveEvent.Invoke(itemAbility, false);
                }
            } else {
                for (int i = ability.ActiveIndex; i < m_ActiveAbilityCount - 1; ++i) {
                    m_ActiveAbilities[i] = m_ActiveAbilities[i + 1];
                    m_ActiveAbilities[i].ActiveIndex = i;
                }
                m_ActiveAbilityCount--;
                m_ActiveAbilities[m_ActiveAbilityCount] = null;

                ability.StopAbility(force, true);

                // Let others know that the ability has stopped.
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterAbilityActive", ability, false);
                if (m_OnAbilityActiveEvent != null) {
                    m_OnAbilityActiveEvent.Invoke(ability, false);
                }

                // After the ability has stopped it may need to equip the unequipped items again.
                if (!(ability is ItemEquipVerifier) && !(ability is MoveTowards) && m_ItemEquipVerifierAbility != null) {
                    m_ItemEquipVerifierAbility.TryToggleItem(ability, false);
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the ability of type T with the specified index.
        /// </summary>
        /// <typeparam name="T">The type of ability to return.</typeparam>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The ability of type T with the specified index. Can be null.</returns>
        public T GetAbility<T>(int index = -1) where T : Ability
        {
            return GetAbility(typeof(T), index) as T;
        }

        /// <summary>
        /// Returns the ability of the specified type with the specified index.
        /// </summary>
        /// <param name="type">The type of ability to return.</param>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The ability of the specified type with the specified index. Can be null.</returns>
        public Ability GetAbility(System.Type type, int index = -1)
        {
            var allAbilities = (typeof(ItemAbility).IsAssignableFrom(type) ? m_ItemAbilities : m_Abilities);
            if (allAbilities != null) {
                for (int i = 0; i < allAbilities.Length; ++i) {
                    if (type.IsInstanceOfType(allAbilities[i]) && (index == -1 || index == allAbilities[i].Index)) {
                        return allAbilities[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the item ability of type T with the specified slotID and actionID.
        /// </summary>
        /// <typeparam name="T">The type of item ability to return.</typeparam>
        /// <param name="slotID">The slot ID of the item ability. -1 will ignore the slotID.</param>
        /// <param name="actionID">The action ID of the item ability. -1 will ignore the actionID.</param>
        /// <returns>The item ability of type T with the specified slotID and actionID. Can be null.</returns>
        public T GetItemAbility<T>(int slotID = -1, int actionID = -1) where T : ItemAbility
        {
            return GetItemAbility(typeof(T), slotID, actionID) as T;
        }
        
        /// <summary>
        /// Returns the item ability of type T with the specified slotID and actionID.
        /// </summary>
        /// <param name="type">The type of item ability to return.</param>
        /// <param name="slotID">The slot ID of the item ability. -1 will ignore the slotID.</param>
        /// <param name="actionID">The action ID of the item ability. -1 will ignore the actionID.</param>
        /// <returns>The item ability of type T with the specified slotID and actionID. Can be null.</returns>
        public ItemAbility GetItemAbility(System.Type type, int slotID = -1, int actionID = -1)
        {
            var itemAbilities = m_ItemAbilities;
            if (itemAbilities != null) {
                for (int i = 0; i < itemAbilities.Length; ++i) {
                    if (type.IsInstanceOfType(itemAbilities[i]) 
                        && (slotID == -1 || slotID == itemAbilities[i].SlotID)
                        && (actionID == -1 || actionID == itemAbilities[i].ActionID)) {
                        return itemAbilities[i];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the abilities of type T with the specified index.
        /// </summary>
        /// <typeparam name="T">The type of ability to return.</typeparam>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The abilities of type T with the specified index. Can be null.</returns>
        public T[] GetAbilities<T>(int index = -1) where T : Ability
        {
            var allAbilities = (typeof(ItemAbility).IsAssignableFrom(typeof(T)) ? m_ItemAbilities : m_Abilities);
            var count = 0;
            if (allAbilities != null) {
                // Determine the total number of abilities first so only one allocation is made.
                for (int i = 0; i < allAbilities.Length; ++i) {
                    if (allAbilities[i] is T && (index == -1 || index == allAbilities[i].Index)) {
                        count++;
                    }
                }

                if (count > 0) {
                    var abilities = new T[count];
                    count = 0;
                    for (int i = 0; i < allAbilities.Length; ++i) {
                        if (allAbilities[i] is T && (index == -1 || index == allAbilities[i].Index)) {
                            abilities[count] = allAbilities[i] as T;
                            count++;
                            if (count == abilities.Length) {
                                break;
                            }
                        }
                    }
                    return abilities;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the abilities of the specified type with the specified index.
        /// </summary>
        /// <param name="type">The type of ability.</param>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The abilities of the specified type with the specified index. Can be null.</returns>
        public Ability[] GetAbilities(System.Type type, int index = -1)
        {
            if (type == null) { return null; }

            var allAbilities = (typeof(ItemAbility).IsAssignableFrom(type) ? m_ItemAbilities : m_Abilities);
            var count = 0;
            if (allAbilities != null) {
                // Determine the total number of abilities first so only one allocation is made.
                for (int i = 0; i < allAbilities.Length; ++i) {
                    if (type.IsInstanceOfType(allAbilities[i]) && (index == -1 || index == allAbilities[i].Index)) {
                        count++;
                    }
                }

                if (count > 0) {
                    var abilities = new Ability[count];
                    count = 0;
                    for (int i = 0; i < allAbilities.Length; ++i) {
                        if (type.IsInstanceOfType(allAbilities[i]) && (index == -1 || index == allAbilities[i].Index)) {
                            abilities[count] = allAbilities[i];
                            count++;
                            if (count == abilities.Length) {
                                break;
                            }
                        }
                    }
                    return abilities;
                }
            }

            return null;
        }

        /// <summary>
        /// Is the ability of the specified type active?
        /// </summary>
        /// <typeparam name="T">The type of ability.</typeparam>
        /// <returns>True if the ability is active.</returns>
        public bool IsAbilityTypeActive<T>() where T : Ability
        {
            var isItemAbility = typeof(ItemAbility).IsAssignableFrom(typeof(T));
            var activeAbilities = isItemAbility ? m_ActiveItemAbilities : m_ActiveAbilities;
            var count = isItemAbility ? m_ActiveItemAbilityCount : m_ActiveAbilityCount;
            if (activeAbilities != null) {
                for (int i = 0; i < count; ++i) {
                    if (typeof(T).IsInstanceOfType(activeAbilities[i])) {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Stops all of the active abilities.
        /// </summary>
        /// <param name="fromDeath">Are the abilities being stopped from death callback?</param>
        private void StopAllAbilities(bool fromDeath)
        {
            for (int i = m_ActiveAbilityCount - 1; i >= 0; --i) {
                // Another ability may have already stopped the active ability.
                if (m_ActiveAbilities[i] == null) {
                    continue;
                }
                if (!fromDeath || !m_ActiveAbilities[i].CanStayActivatedOnDeath) {
                    TryStopAbility(m_ActiveAbilities[i], true);
                }
            }
            for (int i = m_ActiveItemAbilityCount - 1; i >= 0; --i) {
                // Another ability may have already stopped the active ability.
                if (m_ActiveItemAbilities[i] == null) {
                    continue;
                }
                if (!fromDeath || !m_ActiveItemAbilities[i].CanStayActivatedOnDeath) {
                    TryStopAbility(m_ActiveItemAbilities[i], true);
                }
            }
        }

        /// <summary>
        /// Tries to start the specified effect.
        /// </summary>
        /// <param name="effect">The effect to try to start.</param>
        /// <returns>True if the effect was started.</returns>
        public bool TryStartEffect(Effect effect)
        {
            // The effect can't be started if it is already active, isn't enabled, or can't be started.
            if (effect.IsActive || !effect.Enabled || !effect.CanStartEffect()) {
                return false;
            }

            m_ActiveEffects[m_ActiveEffectsCount] = effect;
            m_ActiveEffectsCount++;
            effect.StartEffect(m_ActiveEffectsCount);
            return true;
        }

        /// <summary>
        /// Tries to stop the specified effect.
        /// </summary>
        /// <param name="effect">The effect to try to stop.</param>
        /// <returns>True if the effect was stopped.</returns>
        public bool TryStopEffect(Effect effect)
        {
            // The effect can't be stopped if it isn't active.
            if (!effect.IsActive) {
                return false;
            }

            // Store the active index ahead of time because StopEffect will reset the value.
            var index = effect.ActiveIndex;
            effect.StopEffect(true);

            // Update the active effect array by removing the stopped ability.
            for (int i = index; i < m_ActiveEffectsCount - 1; ++i) {
                m_ActiveEffects[i] = m_ActiveEffects[i + 1];
            }
            m_ActiveEffectsCount--;
            m_ActiveEffects[m_ActiveEffectsCount] = null;
            return true;
        }

        /// <summary>
        /// Returns the effect of type T.
        /// </summary>
        /// <typeparam name="T">The type of effect to return.</typeparam>
        /// <returns>The effect of type T. Can be null.</returns>
        public T GetEffect<T>() where T : Effect
        {
            return GetEffect<T>(-1);
        }

        /// <summary>
        /// Returns the effect of type T at the specified index.
        /// </summary>
        /// <typeparam name="T">The type of effect to return.</typeparam>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The effect of type T. Can be null.</returns>
        public T GetEffect<T>(int index) where T : Effect
        {
            if (m_Effects == null) { return null; }

            var type = typeof(T);
            for (int i = 0; i < m_Effects.Length; ++i) {
                if (type.IsInstanceOfType(m_Effects[i]) && (index == -1 || index == m_Effects[i].Index)) {
                    return m_Effects[i] as T;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the effect of the specified type.
        /// </summary>
        /// <param name="type">The type of effect to retrieve.</param>
        /// <returns>The effect of the specified type. Can be null.</returns>
        public Effect GetEffect(System.Type type)
        {
            return GetEffect(type, -1);
        }

        /// <summary>
        /// Returns the effect of the specified type at the specified index.
        /// </summary>
        /// <param name="type">The type of effect to retrieve.</param>
        /// <param name="index">The index of the ability. -1 will ignore the index.</param>
        /// <returns>The effect of the specified type. Can be null.</returns>
        public Effect GetEffect(System.Type type, int index)
        {
            if (type == null) { return null; }
            if (m_Effects == null) { return null; }

            for (int i = 0; i < m_Effects.Length; ++i) {
                if (type.IsInstanceOfType(m_Effects[i]) && (index == -1 || index == m_Effects[i].Index)) {
                    return m_Effects[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Casts a ray using in the specified direction. If the character has multiple colliders added then a ray will be cast from each collider.
        /// A CapsuleCast or SphereCast is used depending on the type of collider that has been added.
        /// </summary>
        /// <param name="direction">The direction to perform the cast.</param>
        /// <param name="layers">The layers to perform the cast on.</param>
        /// <param name="offset">The offset of the cast.</param>
        /// <param name="distance">The distance of the cast.</param>
        /// <param name="result">The hit RaycastHit.</param>
        /// <returns>Did the cast hit an object?</returns>
        public bool SingleCast(Vector3 direction, Vector3 offset, float distance, int layers, ref RaycastHit result
#if UNITY_EDITOR
            , bool drawDebugLine = false
#endif
            )
        {
            for (int i = 0; i < m_ColliderCount; ++i) {
                // The collider may not be active.
                if (!m_Colliders[i].gameObject.activeInHierarchy) {
                    continue;
                }
                if (SingleCast(m_Colliders[i], direction, offset, distance, layers, out result
#if UNITY_EDITOR
                    , drawDebugLine
#endif
                )) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Casts a ray using in the specified direction. If the character has multiple colliders added then a ray will be cast from each collider.
        /// A CapsuleCast, SphereCast, or BoxCast is used depending on the type of collider that has been added. The result is stored in the combinedRaycastHits array.
        /// </summary>
        /// <param name="direction">The direction to perform the cast.</param>
        /// <param name="offset">Any offset to apply to the cast.</param>
        /// <param name="combinedCastReults">A mapping between the raycast hit and collider index.</param>
        /// <param name="colliderIndex">The collider index of the hit raycast.</param>
        /// <param name="colliderIndexMap">The found raycast hits.</param>
        /// <returns>The number of objects hit from the cast.</returns>
        public int Cast(Vector3 direction, Vector3 offset, ref RaycastHit[] combinedCastReults, ref int colliderIndex, ref Dictionary<RaycastHit, int> colliderIndexMap)
        {
            if (m_ColliderCount > 1) {
                if (combinedCastReults == null) {
                    combinedCastReults = new RaycastHit[m_ColliderCount * m_CastResults.Length];
                    colliderIndexMap = new Dictionary<RaycastHit, int>();
                }
                // Clear the index map to start it off fresh.
                colliderIndexMap.Clear();
            }

            var hitCount = 0;
            for (int i = 0; i < m_ColliderCount; ++i) {
                // The collider may not be active.
                if (!m_Colliders[i].gameObject.activeInHierarchy) {
                    continue;
                }

                int localHitCount;
                // Determine if the collider would intersect any objects.
                if (m_Colliders[i] is CapsuleCollider) {
                    Vector3 startEndCap, endEndCap;
                    var capsuleCollider = m_Colliders[i] as CapsuleCollider;
                    var colliderTransform = capsuleCollider.transform;
                    MathUtility.CapsuleColliderEndCaps(capsuleCollider, colliderTransform.position + offset, colliderTransform.rotation, out startEndCap, out endEndCap);
                    var radius = capsuleCollider.radius * MathUtility.ColliderScaleMultiplier(capsuleCollider) - ColliderSpacing;
                    localHitCount = Physics.CapsuleCastNonAlloc(startEndCap, endEndCap, radius, direction.normalized, m_CastResults, direction.magnitude + ColliderSpacing, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                } else if (m_Colliders[i] is SphereCollider) {
                    var sphereCollider = m_Colliders[i] as SphereCollider;
                    var radius = sphereCollider.radius * MathUtility.ColliderScaleMultiplier(sphereCollider) - ColliderSpacing;
                    localHitCount = Physics.SphereCastNonAlloc(sphereCollider.transform.TransformPoint(sphereCollider.center) + offset, radius, direction.normalized,
                                                                    m_CastResults, direction.magnitude + ColliderSpacing, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                } else { // BoxCollider.
                    var boxCollider = m_Colliders[i] as BoxCollider;
                    var extents = (MathUtility.ColliderScaleMultiplier(boxCollider) - ColliderSpacing) * boxCollider.size / 2;
                    localHitCount = Physics.BoxCastNonAlloc(boxCollider.transform.TransformPoint(boxCollider.center) + offset, extents, direction.normalized,
                                                                    m_CastResults, boxCollider.transform.rotation, direction.magnitude + ColliderSpacing, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                }

                if (localHitCount > 0) {
                    // The mapping needs to be saved if there are multiple colliders.
                    if (m_ColliderCount > 1) {
                        int validHitCount = 0;
                        for (int j = 0; j < localHitCount; ++j) {
                            if (colliderIndexMap.ContainsKey(m_CastResults[j])) {
                                continue;
                            }
                            // Ensure the array is large enough.
                            if (hitCount + j >= m_CastResults.Length) {
                                Debug.LogWarning("Warning: The maximum number of collisions has been reached. Consider increasing the CharacterLocomotion MaxCollisionCount value.");
                                continue;
                            }

                            colliderIndexMap.Add(m_CastResults[j], i);
                            combinedCastReults[hitCount + j] = m_CastResults[j];
                            validHitCount += 1;
                        }
                        hitCount += validHitCount;
                    } else {
                        combinedCastReults = m_CastResults;
                        hitCount += localHitCount;
                        colliderIndex = i;
                    }
                }
            }

            return hitCount;
        }

        /// <summary>
        /// Returns the collider which contains the point within its bounding box.
        /// </summary>
        /// <param name="point">The point to determine if it is within the bounding box of the character.</param>
        /// <returns>The collider which contains the point within its bounding box. Can be null.</returns>
        public Collider BoundsCountains(Vector3 point)
        {
            for (int i = 0; i < m_ColliderCount; ++i) {
                if (m_Colliders[i].bounds.Contains(point)) {
                    return m_Colliders[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Sets the rotation of the character.
        /// </summary>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        public void SetRotation(Quaternion rotation, bool snapAnimator = true)
        {
            // If the character isn't active then only the transform needs to be set.
            if (m_GameObject == null) {
                transform.rotation = rotation;
                return;
            }

            base.SetRotation(rotation);

            if (snapAnimator) {
                StopAllAbilities(false);
            }

            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", snapAnimator);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.SetRotation(rotation, snapAnimator);
            }
#endif
        }

        /// <summary>
        /// Sets the position of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        public void SetPosition(Vector3 position, bool snapAnimator = true)
        {
            // If the character isn't active then only the transform needs to be set.
            if (m_GameObject == null) {
                transform.position = position;
                GetComponent<Rigidbody>().position = position;
                return;
            }

            m_MaxHeight = float.NegativeInfinity;
            m_MaxHeightPosition = position;
            base.SetPosition(position);
            if (snapAnimator) {
                StopAllAbilities(false);
            }
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", snapAnimator);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.SetPosition(position, snapAnimator);
            }
#endif
        }

        /// <summary>
        /// Resets the rotation and position to their default values.
        /// </summary>
        public override void ResetRotationPosition()
        {
            if (m_GameObject == null) {
                return;
            }

            base.ResetRotationPosition();

            Moving = false;
            m_MaxHeight = float.NegativeInfinity;
            m_MaxHeightPosition = m_Rigidbody.position;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.ResetRotationPosition();
            }
#endif
        }

        /// <summary>
        /// Sets the position and rotation of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            SetPositionAndRotation(position, rotation, true, true);
        }

        /// <summary>
        /// Sets the position and rotation of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool snapAnimator)
        {
            SetPositionAndRotation(position, rotation, snapAnimator, true);
        }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        /// <summary>
        /// Sets the position and rotation of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        /// <param name="stopAllAbilities">Should all abilities be stopped?</param>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool snapAnimator, bool stopAllAbilities)
        {
            SetPositionAndRotation(position, rotation, snapAnimator, stopAllAbilities, true);
        }
#endif

        /// <summary>
        /// Sets the position and rotation of the character.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="snapAnimator">Should the animator be snapped into position?</param>
        /// <param name="stopAllAbilities">Should all abilities be stopped?</param>
        /// <param name="sendOverNetwork">Should the position and rotation be sent over the network?</param>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool snapAnimator, bool stopAllAbilities
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                                            , bool sendOverNetwork
#endif
            )
        {
            if (m_GameObject == null) {
                return;
            }

            if (stopAllAbilities) {
                StopAllAbilities(false);
            }

            m_MaxHeight = float.NegativeInfinity;
            m_MaxHeightPosition = position;

            base.SetRotation(rotation);
            base.SetPosition(position);

            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", snapAnimator);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (sendOverNetwork && m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.SetPositionAndRotation(position, rotation, snapAnimator, stopAllAbilities);
            }
#endif
        }

        /// <summary>
        /// Completely resets the character to its default values.
        /// </summary>
        public void ResetCharacter()
        {
            ResetRotationPosition();
            SetMovingPlatform(null);
            StopAllAbilities(false);
            m_GravityAmount = 0;
        }


        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            m_FirstPersonPerspective = firstPersonPerspective;
            if (firstPersonPerspective) {
                if (!string.IsNullOrEmpty(m_ThirdPersonStateName)) {
                    StateManager.SetState(m_GameObject, m_ThirdPersonStateName, false);
                }
                if (!string.IsNullOrEmpty(m_FirstPersonStateName)) {
                    StateManager.SetState(m_GameObject, m_FirstPersonStateName, true);
                }
                if (!string.IsNullOrEmpty(m_FirstPersonMovementTypeFullName)) {
                    SetMovementType(m_FirstPersonMovementTypeFullName);
                }
            } else {
                if (!string.IsNullOrEmpty(m_FirstPersonStateName)) {
                    StateManager.SetState(m_GameObject, m_FirstPersonStateName, false);
                }
                if (!string.IsNullOrEmpty(m_ThirdPersonStateName)) {
                    StateManager.SetState(m_GameObject, m_ThirdPersonStateName, true);
                }
                if (!string.IsNullOrEmpty(m_ThirdPersonMovementTypeFullName)) {
                    SetMovementType(m_ThirdPersonMovementTypeFullName);
                }
            }

            if (m_ActiveMovementType == null) {
                SetMovementType(m_MovementTypeFullName);
            }
        }

        /// <summary>
        /// The character's model has switched.
        /// </summary>
        /// <param name="activeModel">The active character model.</param>
        protected virtual void OnSwitchModels(GameObject activeModel)
        {
            ResetRootMotion();
        }

        /// <summary>
        /// The character has entered a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character entered.</param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            // Forward the enter to the abilities.
            if (m_Abilities != null) {
                for (int i = 0; i < m_Abilities.Length; ++i) {
                    if (!m_Abilities[i].Enabled) {
                        continue;
                    }

                    m_Abilities[i].OnTriggerEnter(other);
                }
            }
            if (m_ItemAbilities != null) {
                for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                    if (!m_ItemAbilities[i].Enabled) {
                        continue;
                    }

                    m_ItemAbilities[i].OnTriggerEnter(other);
                }
            }
        }

        /// <summary>
        /// The character has exited a trigger.
        /// </summary>
        /// <param name="other">The trigger collider that the character exited.</param>
        protected virtual void OnTriggerExit(Collider other)
        {
            // Forward the exit to the abilities.
            if (m_Abilities != null) {
                for (int i = 0; i < m_Abilities.Length; ++i) {
                    if (!m_Abilities[i].Enabled) {
                        continue;
                    }

                    m_Abilities[i].OnTriggerExit(other);
                }
            }
            if (m_ItemAbilities != null) {
                for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                    if (!m_ItemAbilities[i].Enabled) {
                        continue;
                    }

                    m_ItemAbilities[i].OnTriggerExit(other);
                }
            }
        }

        /// <summary>
        /// The character's position or rotation has been teleported.
        /// </summary>
        /// <param name="snapAnimator">Should the animator be snapped?</param>
        private void OnImmediateTransformChange(bool snapAnimator)
        {
            // Do a pass on trying to start any abilities and items to ensure they are in sync.
            UpdateAbilities(m_Abilities);
            UpdateAbilities(m_ItemAbilities);
            if (m_OnAnimationUpdate != null) {
                m_OnAnimationUpdate();
            }

            // Snap the animator after the abilities have updated.
            if (snapAnimator) {
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterSnapAnimator", true);
            }
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_Alive = false;

            // All of the abilities should stop.
            StopAllAbilities(true);

            // The animator values should reset.
            m_InputVector = Vector3.zero;
            Moving = false;
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            m_Alive = true;
            ResetRotationPosition();

            // Do a pass on trying to start any abilities and items to ensure they are in sync.
            UpdateAbilities(m_Abilities);
            UpdateAbilities(m_ItemAbilities);
        }

        /// <summary>
        /// The character has been disabled.
        /// </summary>
        protected override void OnDisable()
        {
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterActivate", false);

            base.OnDisable();
        }

        /// <summary>
        /// Callback that should draw the editor gizmos.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (m_Abilities != null) {
                for (int i = 0; i < m_Abilities.Length; ++i) {
                    m_Abilities[i].OnDrawGizmos();
                }
            }
            if (m_ItemAbilities != null) {
                for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                    m_ItemAbilities[i].OnDrawGizmos();
                }
            }
        }

        /// <summary>
        /// Callback that should draw the editor gizmos when the character is selected.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (m_Abilities != null) {
                for (int i = 0; i < m_Abilities.Length; ++i) {
                    m_Abilities[i].OnDrawGizmosSelected();
                }
            }
            if (m_ItemAbilities != null) {
                for (int i = 0; i < m_ItemAbilities.Length; ++i) {
                    m_ItemAbilities[i].OnDrawGizmosSelected();
                }
            }
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.UnregisterEvent<bool>(gameObject, "OnCharacterChangePerspectives", OnChangePerspectives);
            EventHandler.UnregisterEvent<GameObject>(m_GameObject, "OnCharacterSwitchModels", OnSwitchModels);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }
    }
}