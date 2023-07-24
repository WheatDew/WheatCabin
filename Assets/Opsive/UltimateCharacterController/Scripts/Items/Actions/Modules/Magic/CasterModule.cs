/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// The magic cast data contains information about the origin of the cast and more.
    /// </summary>
    public class MagicCastData
    {
        private Transform m_CastOrigin;
        private Vector3 m_Direction;
        private Vector3 m_CastPosition;
        private Vector3 m_CastTargetPosition;
        protected float m_StartCastTime;        // The start of other casts if the caster is continuous.
        
        public Transform CastOrigin { get => m_CastOrigin; set => m_CastOrigin = value; }
        public Vector3 CastPosition { get => m_CastPosition; set => m_CastPosition = value; }
        
        public Vector3 Direction { get => m_Direction; set => m_Direction = value; }
        public Vector3 CastTargetPosition { get => m_CastTargetPosition; set => m_CastTargetPosition = value; }
        public float StartCastTime { get=> m_StartCastTime; set=> m_StartCastTime = value; }
        public int TargetIndex { get; set; }
        public ListSlice<Collider> Targets { get; set; }
        public uint CastID { get; set; }
        public Vector3 CastNormal { get; set; }
        public LayerMask DetectLayers { get; set; }
    }
    
    /// <summary>
    /// The caster module is used to detect the targets initiate all the casting effects.
    /// </summary>
    [Serializable]
    public abstract class MagicCasterModule : MagicActionModule
    {
        protected MagicCastData m_MagicCastData;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_MagicCastData = CreateNewMagicCastData();
        }

        /// <summary>
        /// Create the magic cast data which will be cached. 
        /// </summary>
        /// <returns>The new magic cast data.</returns>
        public virtual MagicCastData CreateNewMagicCastData()
        {
            return new MagicCastData();
        }
        
        public override bool IsActiveOnlyIfFirstEnabled => true;

        /// <summary>
        /// Start casting the effects.
        /// </summary>
        /// <param name="magicUseDataStream">The use data stream.</param>
        public abstract void Cast(MagicUseDataStream magicUseDataStream);

        /// <summary>
        /// Immediate cast effects bypasses the conditions and waiting time and directly casts the effects.
        /// Useful for chain reactions.
        /// </summary>
        /// <param name="magicUseDataStream">The use data stream.</param>
        public abstract void ImmediateCastEffects(MagicUseDataStream magicUseDataStream);
        
        /// <summary>
        /// Get the cast data preview to know potentially where a cast will originate and more.
        /// </summary>
        /// <returns>The preview cast data.</returns>
        public abstract MagicCastData GetCastPreviewData();
        
        /// <summary>
        /// Tick the update function for casting.
        /// </summary>
        public abstract void CastUpdate();
    }
    
    /// <summary>
    /// A caster module with basic functionality. The caster module is used to detect the targets initiate all the casting effects.
    /// </summary>
    [Serializable]
    public class SimpleCaster : MagicCasterModule,
        IModuleCanStartUseItem, IModuleCanUseItem, IModuleIsItemUsePending, IModuleCanStopItemUse, IModuleTryStopItemUse, IModuleStartItemUse, IModuleStopItemUse
    {
        /// <summary>
        /// Specifies the direction of the cast.
        /// </summary>
        public enum CastDirection
        {
            None,       // The cast has no movement.
            Forward,    // The cast should move in the forward direction.
            Target,     // The cast should move towards a target.
            Indicate    // The cast should move towards an indicated position.
        }

        /// <summary>
        /// Specifies how often the magic item does its cast.
        /// </summary>
        public enum CastUseType
        {
            Single,     // The cast should occur once per use.
            Continuous  // The cast should occur every use update.
        }

        /// <summary>
        /// Specifies if the cast should interrupted.
        /// </summary>
        [Flags]
        public enum CastInterruptSource
        {
            None = 0,       // The cast should not be interrupted.
            Movement = 1,   // The cast should be interrupted when the character moves.
            Damage = 2      // The cast should be interrupted when the character takes damage.
        }
        
        [Tooltip("Start Cast event Trigger defines the delay between Begin cast and the casting effect.")]
        [SerializeField] protected AnimationSlotEventTrigger m_StartCastEventTrigger = new AnimationSlotEventTrigger(false, 0);
        [Tooltip("Repeat Cast Event Trigger defines the time between repeating the casting effects when continuous.")]
        [SerializeField] protected AnimationSlotEventTrigger m_RepeatCastEventTrigger = new AnimationSlotEventTrigger(false, 0);
        [Tooltip("End Cast Event Trigger defines the time between the last cast effect and the End cast.")]
        [SerializeField] protected AnimationSlotEventTrigger m_EndCastEventTrigger = new AnimationSlotEventTrigger(false, 0);
        [Tooltip("Use the Character Transform as the Cast Origin?")]
        [SerializeField] protected bool m_CharacterAsCastOrigin = false;
        [Tooltip("The Cast Origin transform.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_CastOrigin;
        [Tooltip("Is the character required to be on the ground?")]
        [SerializeField] protected bool m_RequireGrounded = true;
        [Tooltip("The direction of the cast.")]
        [SerializeField] protected CastDirection m_Direction = CastDirection.Forward;
        [Tooltip("Should the look source be used when determining the cast direction?")]
        [SerializeField] protected bool m_UseLookSource = true;
        [Tooltip("The maximum distance of the movement cast direction.")]
        [SerializeField] protected float m_MaxDistance = 100;
        [Tooltip("The radius of the movement cast direction.")]
        [SerializeField] protected float m_Radius = 0.1f;
        [Tooltip("The layers that the movement directions can collide with.")]
        [SerializeField] protected LayerMask m_DetectLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.UI | 1 << LayerManager.SubCharacter | 1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect);
        [Tooltip("The maximum angle that the target object can be compared to the character's forward direction.")]
        [SerializeField] protected float m_MaxAngle = 30;
        [Tooltip("The maximum number of colliders that can be detected by the target cast.")]
        [SerializeField] protected int m_MaxCollisionCount = 100;
        [UnityEngine.Serialization.FormerlySerializedAs("m_TargetCount")]
        [Tooltip("The number of objects that a single cast should cast.")]
        [SerializeField] protected int m_MaxTargetCount = 1;
        [Tooltip("The transform used to indicate the surface. Can be null.")]
        [SerializeField] protected Transform m_SurfaceIndicator;
        [Tooltip("The offset when positioning the surface indicator.")]
        [SerializeField] protected Vector3 m_SurfaceIndicatorOffset = new Vector3(0, 0.1f, 0);
        [Tooltip("Specifies how often the cast is used.")]
        [SerializeField] protected CastUseType m_UseType;
        [Tooltip("The minimum duration of the continuous use type. If a value of -1 is set then the item will be stopped when the stop is requested.")]
        [SerializeField] protected float m_MinContinuousUseDuration = 1;
        [Tooltip("Should the continuous use type cast every update?")]
        [SerializeField] protected bool m_ContinuousCast;
        [Tooltip("Specifies when the cast should be interrupted.")]
        [SerializeField] protected CastInterruptSource m_InterruptSource;
        [Tooltip("Should cast update be called the moment the cast happens or wait until UpdateItem is called?")]
        [SerializeField] protected bool m_CastUpdateOnCast;

        private CharacterLayerManager m_CharacterLayerManager;
        private Use m_UseStartAbility;
        
        private Collider[] m_TargetColliders;
        private float[] m_TargetAngles;
        private int m_TargetsFoundCount;
        private float m_StartUseTime;
        private float m_StartCastTime;

        private Vector3 m_CastDirection;
        private Vector3 m_CastTargetPosition;
        private Vector3 m_CastNormal;
        private bool m_Used;

        protected bool m_Casting;
        
        private bool m_StopRequested;
        private bool m_Stopping;
        private bool m_ForceStop;

        protected int m_IndividualCastStartCount;
        protected int m_AllCastStartCount;

        protected int TargetCount => Mathf.Min(m_TargetsFoundCount, m_MaxTargetCount);

        public virtual ILookSource LookSource => MagicAction.LookSource;
        public Transform CastOriginLocation
        {
            get
            {
                if (m_CharacterAsCastOrigin) {
                    return CharacterTransform;
                }
                var origin = m_CastOrigin.GetValue();
                if (origin == null) {
                    return CharacterTransform;
                }

                return origin;
            }
        }

        [Shared.Utility.NonSerialized] public ItemPerspectiveIDObjectProperty<Transform> PerspectiveCastOrigin { get => m_CastOrigin; set => m_CastOrigin = value; }
        public bool RequireGrounded { get => m_RequireGrounded; set => m_RequireGrounded = value; }
        public CastDirection Direction { get => m_Direction; set => m_Direction = value; }
        public bool UseLookSource { get => m_UseLookSource; set => m_UseLookSource = value; }
        public float MaxDistance { get => m_MaxDistance; set => m_MaxDistance = value; }
        public float Radius { get => m_Radius; set => m_Radius = value; }
        public LayerMask DetectLayers { get => m_DetectLayers; set => m_DetectLayers = value; }
        public float MaxAngle { get => m_MaxAngle; set => m_MaxAngle = value; }
        public int MaxCollisionCount { get => m_MaxCollisionCount; set => m_MaxCollisionCount = value; }
        public int MaxTargetCount { get => m_MaxTargetCount; set => m_MaxTargetCount = value; }
        public Transform SurfaceIndicator { get => m_SurfaceIndicator; set => m_SurfaceIndicator = value; }
        public Vector3 SurfaceIndicatorOffset { get => m_SurfaceIndicatorOffset; set => m_SurfaceIndicatorOffset = value; }
        public CastUseType UseType { get => m_UseType; set => m_UseType = value; }
        public float MinContinuousUseDuration { get => m_MinContinuousUseDuration; set => m_MinContinuousUseDuration = value; }
        public bool ContinuousCast { get => m_ContinuousCast; set => m_ContinuousCast = value; }
        public CastInterruptSource InterruptSource { get => m_InterruptSource; set => m_InterruptSource = value; }
        public bool CastUpdateOnCast { get => m_CastUpdateOnCast; set => m_CastUpdateOnCast = value; }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_CharacterLayerManager = Character.GetCachedComponent<CharacterLayerManager>();
            m_CastOrigin.Initialize(itemAction);
            
            if (m_SurfaceIndicator != null) {
                m_SurfaceIndicator.gameObject.SetActive(false);
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // The local surface indicator should not show for remote players.
            if (NetworkInfo != null && !NetworkInfo.HasAuthority()) {
                m_SurfaceIndicator = null;
            }
#endif
            MagicAction.OnLateUpdateE += LateUpdate;
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);

            var target = Character;
            
            m_StartCastEventTrigger.RegisterUnregisterEvent(register, target, "OnAnimatorStartCast", SlotID, OnAnimatorStartCast);
            m_RepeatCastEventTrigger.RegisterUnregisterEvent(register, target, "OnAnimatorRepeatCast", SlotID, OnAnimatorRepeatCast);
            m_EndCastEventTrigger.RegisterUnregisterEvent(register, target, "OnAnimatorEndCast", SlotID, OnAnimatorEndCast);
            
            if (register) {
                Opsive.Shared.Events.EventHandler.RegisterEvent<bool>(target, "OnCharacterMoving", OnMoving);
                Opsive.Shared.Events.EventHandler.RegisterEvent<Ability, bool>(target, "OnCharacterAbilityActive", OnAbilityActive);
                Opsive.Shared.Events.EventHandler.RegisterEvent<float, Vector3, Vector3, GameObject, Collider>(target, "OnHealthDamage", OnDamage);
            } else {
                Opsive.Shared.Events.EventHandler.UnregisterEvent<bool>(target, "OnCharacterMoving", OnMoving);
                Opsive.Shared.Events.EventHandler.UnregisterEvent<Ability, bool>(target, "OnCharacterAbilityActive", OnAbilityActive);
                Opsive.Shared.Events.EventHandler.UnregisterEvent<float, Vector3, Vector3, GameObject, Collider>(target, "OnHealthDamage", OnDamage);
            }
        }

        /// <summary>
        /// The animation event when it is time to repeat the cast.
        /// </summary>
        public void OnAnimatorStartCast()
        {
            StartCastEffects();
            
            if (m_CastUpdateOnCast) {
                CastUpdate();
            }
        }
        
        /// <summary>
        /// The animation event when it is time to repeat the cast.
        /// </summary>
        public void OnAnimatorRepeatCast()
        {
            // Do nothing.
        }
        
        /// <summary>
        /// The animation event when it is time to repeat the cast.
        /// </summary>
        public void OnAnimatorEndCast()
        {
            m_Casting = false;
            MagicAction.OnStopCasting(MagicAction.MagicUseDataStream.CastData);
        }
        
        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public void StartItemUse(Use useAbility)
        {
            m_StartUseTime = Time.time;
            m_StartCastTime = 0;
            m_UseStartAbility = useAbility;
            m_ForceStop = false;
            m_Used = false;
            m_Casting = false;

            m_AllCastStartCount = 0;
            m_IndividualCastStartCount = 0;
        }

        /// <summary>
        /// Start casting the effects.
        /// </summary>
        /// <param name="magicUseDataStream">The use data stream.</param>
        public override void Cast(MagicUseDataStream magicUseDataStream)
        {
            
            //Start the casting right away.
            var magicCastData = GetCastData();
            m_StartCastTime =  Time.time;
            magicCastData.StartCastTime = m_StartCastTime;
            
            MagicAction.OnStartCasting(magicCastData);

            m_Casting = true;
            m_StartCastEventTrigger.WaitForEvent(true);
        }

        /// <summary>
        /// Start the cast effects.
        /// </summary>
        protected virtual void StartCastEffects()
        {
            m_AllCastStartCount++;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            int invokedBitmask = 0;
#endif
            for (int i = 0; i < MagicAction.CastEffectsModuleGroup.EnabledModules.Count; i++) {
                MagicAction.CastEffectsModuleGroup.EnabledModules[i].StartCast(MagicAction.MagicUseDataStream);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                invokedBitmask |= 1 << MagicAction.CastEffectsModuleGroup.EnabledModules[i].ID;
#endif
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (MagicAction.NetworkInfo != null && MagicAction.NetworkInfo.HasAuthority()) {
                MagicAction.NetworkCharacter.InvokeMagicCastEffectsModules(MagicAction, MagicAction.CastEffectsModuleGroup, invokedBitmask, 
                    Networking.Character.INetworkCharacter.CastEffectState.Start, MagicAction.MagicUseDataStream);
            }
#endif
        }

        /// <summary>
        /// Tick the update function for casting.
        /// </summary>
        public override void CastUpdate()
        {
            if (m_RepeatCastEventTrigger.IsWaiting){ return;}
            
            var magicCastData = GetCastData();
            magicCastData.TargetIndex = -1;
            MagicAction.MagicUseDataStream.CastData = magicCastData;

            // Only cast the actions that haven't been casted yet.
            var castEffectModules = MagicAction.CastEffectsModuleGroup.EnabledModules;
            var allActionsCasted = true;
            if (!m_ContinuousCast) {
                for (int i = 0; i < castEffectModules.Count; ++i) {
                    if (castEffectModules[i].IsCastComplete(MagicAction.MagicUseDataStream) == false) {
                        allActionsCasted = false;
                        break;
                    }
                }
            }

            if (allActionsCasted && !m_ContinuousCast) {
                return;
            }
            
            if (!allActionsCasted || m_ContinuousCast) {
                // Use the cast effect for each targets.
                var useCount = m_Direction == CastDirection.Target ? TargetCount : 1;
                for (int i = 0; i < useCount; ++i) {
                    var previousCastDirection = m_CastDirection;
                    var previousCastTarget = m_CastTargetPosition;
                    var previousCastNormal = m_CastNormal;
                    if (!DetermineCastValues(i, ref m_CastDirection, ref m_CastTargetPosition, ref m_CastNormal)) {
                        // If the cast no longer finds a value, but it is the first cast use the previous values
                        // Otherwise ignore the cast
                        if (m_Used || i > 0) {
                            continue;
                        }
                        
                        m_CastDirection = previousCastDirection;
                        m_CastTargetPosition = previousCastTarget;
                        m_CastNormal = previousCastNormal;
                    }
                    
                    magicCastData = GetCastData();

                    // Update the cast data for each target.
                    magicCastData.TargetIndex = i;
                    MagicAction.MagicUseDataStream.CastData = magicCastData;

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                    int invokedBitmask = 0;
#endif
                    for (int j = 0; j < castEffectModules.Count; j++) {
                        castEffectModules[j].OnCastUpdate(MagicAction.MagicUseDataStream);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                        invokedBitmask |= 1 << castEffectModules[i].ID;
#endif
                    }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                    if (MagicAction.NetworkInfo != null && MagicAction.NetworkInfo.HasAuthority()) {
                        MagicAction.NetworkCharacter.InvokeMagicCastEffectsModules(MagicAction, MagicAction.CastEffectsModuleGroup, invokedBitmask,
                            Networking.Character.INetworkCharacter.CastEffectState.Update, MagicAction.MagicUseDataStream);
                    }
#endif
                }
            }

            magicCastData.TargetIndex = -1;
            MagicAction.MagicUseDataStream.CastData = magicCastData;

            allActionsCasted = true;
            for (int i = 0; i < castEffectModules.Count; ++i) {
                if (castEffectModules[i].IsCastComplete(MagicAction.MagicUseDataStream) == false) {
                    allActionsCasted = false;
                }
            }

            if (allActionsCasted) { AllActionCasted(); }

            if (!m_Used) {
                // The item isn't done being used until all actions have been used.
                if (!m_ContinuousCast) {
                    for (int i = 0; i < castEffectModules.Count; ++i) {
                        if (castEffectModules[i].IsCastComplete(MagicAction.MagicUseDataStream) == false) {
                            return;
                        }
                    }
                }

                // If the item was just used the end actions should start.
                if (m_UseType == CastUseType.Single) {
                    // Notify the cast modules that they will stop.
                    var enabledCastModules = MagicAction.CastEffectsModuleGroup.EnabledModules;
                    for (int i = 0; i < enabledCastModules.Count; i++) {
                        enabledCastModules[i].CastWillStop();
                    }
                    
                    m_EndCastEventTrigger.WaitForEvent(false);
                }

                m_Used = true;
            }
        }

        /// <summary>
        /// All actions were casted.
        /// </summary>
        protected virtual void AllActionCasted()
        {
            MagicAction.OnAllActionsCasted(m_IndividualCastStartCount, m_AllCastStartCount);
            
            // Start waiting for a repeat.
            m_RepeatCastEventTrigger.WaitForEvent(true);
            
            var useCount = m_Direction == CastDirection.Target ? TargetCount : 1;
            for (int i = 0; i < useCount; i++) {
                MagicAction.ResetImpactModules((uint)i);
            }
        }

        /// <summary>
        /// Immediate cast effects bypasses the conditions and waiting time and directly casts the effects.
        /// Useful for chain reactions.
        /// </summary>
        /// <param name="magicUseDataStream">The use data stream.</param>
        public override void ImmediateCastEffects(MagicUseDataStream magicUseDataStream)
        {
            var castEffectModules = MagicAction.CastEffectsModuleGroup.EnabledModules;
            for (int j = 0; j < castEffectModules.Count; j++) {
                var castEffectModule = castEffectModules[j];
                
                // Do not check if can cast.
                castEffectModule.DoCast(magicUseDataStream);
                m_IndividualCastStartCount++;
            }
        }

        /// <summary>
        /// Initializes the cast data before returning it.
        /// </summary>
        /// <returns>The updated cast data.</returns>
        public MagicCastData GetCastData()
        {
            m_MagicCastData.CastOrigin = CastOriginLocation;
            m_MagicCastData.CastPosition = CastOriginLocation.position;
            m_MagicCastData.Direction = m_CastDirection;
            m_MagicCastData.CastTargetPosition = m_CastTargetPosition;
            m_MagicCastData.CastNormal = m_CastNormal;
            m_MagicCastData.Targets = new ListSlice<Collider>(m_TargetColliders, 0, Mathf.Min(m_TargetsFoundCount, m_MaxTargetCount));
            m_MagicCastData.TargetIndex = -1;
            m_MagicCastData.DetectLayers = m_DetectLayers;

            return m_MagicCastData;
        }

        /// <summary>
        /// Get the cast data preview to know potentially where a cast will originate and more.
        /// </summary>
        /// <returns>The preview cast data.</returns>
        public override MagicCastData GetCastPreviewData()
        {
            m_MagicCastData.CastOrigin = CastOriginLocation;
            m_MagicCastData.CastPosition = CastOriginLocation.position;
            m_MagicCastData.Direction = m_CastDirection;
            m_MagicCastData.CastTargetPosition = m_CastTargetPosition;
            m_MagicCastData.CastNormal = m_CastNormal;
            m_MagicCastData.Targets = new ListSlice<Collider>(m_TargetColliders, 0, Mathf.Min(m_TargetsFoundCount, m_MaxTargetCount));
            m_MagicCastData.TargetIndex = -1;
            m_MagicCastData.DetectLayers = m_DetectLayers;
            
            return m_MagicCastData;
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            if (useAbility != null && useAbility.IsUseInputTryingToStop()) { return false; }
            
            // The item cannot be used while it is already casting.
            // Casting effects must stop before they can be cast again by the trigger.
            if (m_Casting) { return false; }

            if (abilityState == UsableAction.UseAbilityState.Start) {
                // Certain items require the character to be grounded.
                if (m_RequireGrounded && !CharacterLocomotion.Grounded) {
                    return false;
                }
                // If the cast isn't valid then the item shouldn't start.
                if (m_Direction == CastDirection.Target) {
                    DetermineTargetColliders();
                }
                if (!DetermineCastValues(0, ref m_CastDirection, ref m_CastTargetPosition, ref m_CastNormal)) {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <returns>True if the item can be used.</returns>
        public bool CanUseItem()
        {
            // The item cannot be used while it is already casting.
            // Casting effects must stop before they can be cast again by the trigger.
            if (m_Casting) { return false; }

            return true;
        }
        
         /// <summary>
        /// Determines the colliders that are hit by the target direction.
        /// </summary>
        private void DetermineTargetColliders()
        {
            if (m_TargetColliders == null) {
                m_TargetColliders = new Collider[m_MaxCollisionCount];
                m_TargetAngles = new float[m_MaxCollisionCount];
                // Initialize to the max value.
                for (int i = 0; i < m_TargetAngles.Length; ++i) {
                    m_TargetAngles[i] = int.MaxValue;
                }
            }
            var hitCount = Physics.OverlapSphereNonAlloc(CharacterTransform.position, m_MaxDistance, m_TargetColliders, m_DetectLayers, QueryTriggerInteraction.Ignore);
            if (hitCount == 0) {
                return;
            }

#if UNITY_EDITOR
            if (hitCount >= m_TargetColliders.Length) {
                Debug.LogWarning("Warning: The hit count is equal to the max collider array size. This will cause objects to be missed. Consider increasing the max collision count size.");
            }
#endif
            m_TargetsFoundCount = 0;
            for (int i = 0; i < hitCount; ++i) {
                if (m_TargetColliders[i].transform.IsChildOf(CharacterTransform)) {
                    m_TargetAngles[i] = int.MaxValue;
                    continue;
                }

                // The target object needs to be within the field of view of the current object
                var direction = m_TargetColliders[i].transform.position - CharacterTransform.position;
                var angle = Vector3.Angle(direction, CharacterTransform.forward);
                if (angle < m_MaxAngle * 0.5f) {
                    // The target must be within sight.
                    var hitTransform = false;
                    Vector3 position;
                    PivotOffset pivotOffset;
                    if ((pivotOffset = m_TargetColliders[i].transform.gameObject.GetCachedComponent<PivotOffset>()) != null) {
                        position = m_TargetColliders[i].transform.TransformPoint(pivotOffset.Offset);
                    } else {
                        position = m_TargetColliders[i].transform.position;
                    }
                    if (Physics.Linecast(CharacterTransform.position, position, out var raycastHit, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                        var raycastTransform = raycastHit.collider.transform;
                        if (raycastTransform.IsChildOf(m_TargetColliders[i].transform) || raycastTransform.IsChildOf(CharacterTransform)) {
                            hitTransform = true;
                        }
                    }
                    
                    // Find the target that is most in front of the character.
                    m_TargetAngles[i] = hitTransform ? angle : int.MaxValue;
                    m_TargetsFoundCount++;
                } else {
                    m_TargetAngles[i] = int.MaxValue;
                }
            }

            // Sort by the angle. Return the min angle.
            System.Array.Sort(m_TargetAngles, m_TargetColliders);
        }

        /// <summary>
        /// Determines the values of the cast.
        /// </summary>
        /// <param name="index">The index of the target position to retrieve.</param>
        /// <param name="direction">A reference to the target direction.</param>
        /// <param name="position">A reference to the target position.</param>
        /// <param name="normal">A reference to the target normal.</param>
        /// <returns>True if the cast is valid.</returns>
        private bool DetermineCastValues(int index, ref Vector3 direction, ref Vector3 position, ref Vector3 normal)
        {
            var castPosition = CastOriginLocation.position;
            if (m_Direction == CastDirection.Forward || m_Direction == CastDirection.Indicate) {
                if (m_Direction == CastDirection.Forward) {
                    direction = m_UseLookSource ? LookSource.LookDirection(CastOriginLocation.position, false, m_CharacterLayerManager.SolidObjectLayers, true, true) : CharacterTransform.forward;
                    castPosition = m_UseLookSource ? LookSource.LookPosition(true) : CharacterTransform.position;
                } else { // Indicate.
                    direction = LookSource.LookDirection(false);
                    castPosition = LookSource.LookPosition(false);
                }

                var collisionEnabled = CharacterLocomotion.CollisionLayerEnabled;
                CharacterLocomotion.EnableColliderCollisionLayer(false);
                if (Physics.SphereCast(castPosition - direction * m_Radius, m_Radius, direction, out var raycastHit, m_MaxDistance, m_DetectLayers, QueryTriggerInteraction.Ignore)) {
                    // The Cast Actions may indicate that the position is invalid.
                    if (IsValidTargetPosition(raycastHit.point, raycastHit.normal) == false) {
                        return false;
                    }
                    
                    position = raycastHit.point;
                    normal = raycastHit.normal;
                }else if (m_Direction == CastDirection.Forward) {
                    var possiblePosition =castPosition + (direction*m_MaxDistance);
                    var possibleNormal =  -direction;
                    
                    // The Cast Actions may indicate that the position is invalid.
                    if (IsValidTargetPosition(possiblePosition, possibleNormal) == false) {
                        return false;
                    }
                    
                    position = possiblePosition;
                    normal = possibleNormal;
                }
                CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);
                var hasCastValue = m_Direction == CastDirection.Indicate ? (raycastHit.distance > 0) : true;

                if (hasCastValue) {
                    if (m_CharacterItemAction.IsDebugging) {
                        Debug.DrawRay(castPosition, direction *m_MaxDistance, Color.blue, 0.1f);
                    }
                }
                
                return hasCastValue;
            } else if (m_Direction == CastDirection.Target) {
                if (index >= m_TargetsFoundCount || index >= m_TargetColliders.Length) {
                    return false;
                }

                var targetTransform = m_TargetColliders[index].transform;
                PivotOffset pivotOffset;
                if ((pivotOffset = targetTransform.gameObject.GetCachedComponent<PivotOffset>()) != null) {
                    position = targetTransform.TransformPoint(pivotOffset.Offset);
                } else {
                    position = targetTransform.position;
                }
                direction = (position - castPosition).normalized;
                normal = targetTransform.up;
                
                if (m_CharacterItemAction.IsDebugging) {
                    Debug.DrawRay(castPosition, direction *m_MaxDistance, Color.blue, 0.1f);
                }
                
                return true;
            }

            // None direction.
            direction = CharacterTransform.forward;
            position = CharacterTransform.position;
            normal = CharacterTransform.up;
            
            if (m_CharacterItemAction.IsDebugging) {
                Debug.DrawRay(castPosition, direction *m_MaxDistance, Color.blue, 0.1f);
            }
            
            return true;
        }

        /// <summary>
        /// Is the raycast hit target position valid.
        /// </summary>
        /// <param name="point">The point to check for.</param>
        /// <param name="normal">The normal to check for.</param>
        /// <returns>True if the target position is valid.</returns>
        protected virtual bool IsValidTargetPosition(Vector3 point, Vector3 normal)
        {
            var castEffectModules = MagicAction.CastEffectsModuleGroup.EnabledModules;
            
            for (int i = 0; i < castEffectModules.Count; ++i) {
                if (castEffectModules[i].IsValidTargetPosition(MagicAction.MagicUseDataStream, point, normal) == false) {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Is the item waiting to be used? This will return true if the item is waiting to be charged or pulled back.
        /// </summary>
        /// <returns>Returns true if the item is waiting to be used.</returns>
        public virtual bool IsItemUsePending()
        {
            return !CanStopItemUse();
        }

        /// <summary>
        /// Tries to stop the item use.
        /// </summary>
        public virtual void TryStopItemUse()
        {
            // The end actions aren't called until the continuous use item stops.
            m_StopRequested = m_UseStartAbility?.IsUseInputTryingToStop() ?? true;

            if (m_Stopping || (!m_ForceStop && !CanStopItemUse())) {
                UpdateItemAbilityAnimatorParameters();
                return;
            }

            m_Stopping = true;
            UpdateItemAbilityAnimatorParameters();

            if (m_UseType == CastUseType.Continuous) {
                //Notify the cast modules that they will stop.
                var enabledCastModules = MagicAction.CastEffectsModuleGroup.EnabledModules;
                for (int i = 0; i < enabledCastModules.Count; i++) {
                    enabledCastModules[i].CastWillStop();
                }
                
                m_EndCastEventTrigger.WaitForEvent(false);
            }
        }

        /// <summary>
        /// Can the item use be stopped?
        /// </summary>
        /// <returns>True if the item use can be stopped.</returns>
        public virtual bool CanStopItemUse()
        {
            return m_UseType == CastUseType.Single || 
                   (m_StopRequested && (m_MinContinuousUseDuration == -1 || (m_MinContinuousUseDuration > 0 && m_StartUseTime + m_MinContinuousUseDuration < Time.time)));
        }
        
        /// <summary>
        /// Stop the item use.
        /// </summary>
        public void StopItemUse()
        {
            // If the item is forced to stop for whatever reason, make sure it is reset properly.
            m_Casting = false;
        }
        
        /// <summary>
        /// The item has started to be unequipped by the character.
        /// </summary>
        public override void StartUnequip()
        {
            base.StartUnequip();

            if (m_SurfaceIndicator != null && m_SurfaceIndicator.gameObject.activeSelf) {
                m_SurfaceIndicator.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The character has started to or stopped moving.
        /// </summary>
        /// <param name="moving">Is the character moving?</param>
        private void OnMoving(bool moving)
        {
            // Stop the item if the character starts to move and the cast should be interrupted on movement.
            if (moving && (m_InterruptSource & CastInterruptSource.Movement) != 0 && MagicAction.IsItemInUse()) {
                m_ForceStop = true;
                m_UseStartAbility.StopAbility(true);
            }
        }

        /// <summary>
        /// The character's ability has been started or stopped.
        /// </summary>
        /// <param name="ability">The ability which was started or stopped.</param>
        /// <param name="active">True if the ability was started, false if it was stopped.</param>
        private void OnAbilityActive(Ability ability, bool active)
        {
            if (!active || (!(ability is Jump) && !(ability is Fall))) {
                return;
            }

            // Stop the item if the character starts to jump or fall and the cast should be interrupted on movement.
            if ((m_InterruptSource & CastInterruptSource.Movement) != 0 && MagicAction.IsItemInUse()) {
                m_ForceStop = true;
                m_UseStartAbility.StopAbility(true);
            }
        }

        /// <summary>
        /// The character has taken damage.
        /// </summary>
        /// <param name="amount">The amount of damage taken.</param>
        /// <param name="position">The position of the damage.</param>
        /// <param name="force">The amount of force applied to the object while taking the damage.</param>
        /// <param name="attacker">The GameObject that did the damage.</param>
        /// <param name="hitCollider">The Collider that was hit.</param>
        private void OnDamage(float amount, Vector3 position, Vector3 force, GameObject attacker, Collider hitCollider)
        {
            // The item can stop the use ability when the character takes damage.
            if ((m_InterruptSource & CastInterruptSource.Damage) != 0 && MagicAction.IsItemInUse()) {
                m_ForceStop = true;
                m_UseStartAbility.StopAbility(true);
            }
        }
        
        /// <summary>
        /// Draws an indicator when the direction is indicate.
        /// </summary>
        public void LateUpdate()
        {
            var direction = Vector3.zero;
            var position = Vector3.zero;
            var normal = Vector3.zero;
            if (m_Direction != CastDirection.Indicate || !DetermineCastValues(-1, ref direction, ref position, ref normal)) {
                if (m_SurfaceIndicator != null && m_SurfaceIndicator.gameObject.activeSelf) {
                    m_SurfaceIndicator.gameObject.SetActive(false);
                }
                return;
            }

            // The position is valid. Show an optional indicator.
            if (m_SurfaceIndicator != null) {
                m_SurfaceIndicator.SetPositionAndRotation(position + m_SurfaceIndicator.TransformDirection(m_SurfaceIndicatorOffset), Quaternion.LookRotation(Vector3.ProjectOnPlane(direction, normal)));
                if (!m_SurfaceIndicator.gameObject.activeSelf) {
                    m_SurfaceIndicator.gameObject.SetActive(true);
                }
            }
        }
    }
}