/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using Opsive.UltimateCharacterController.Traits.Damage;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The Usable Character Item Action is the base class for item that can be used by the Use ability.
    /// </summary>
    public class UsableAction : CharacterItemAction, IUsableItem, IDamageSource
    {
        public enum UseAbilityState
        {
            Start,
            Update,
            None
        }
        
        // Editor Icons.
        public const string TriggerIconGuid = "1ba39e786747bb04f8c1641e11556c1a";
        public const string UsableIconGuid = "6369f09220d42d6449b0d8271b58c6cb";
        public const string ImpactIconGuid = "7ba8c971c8e384f4697856e1ed4902f4";
        public const string ExtraIconGuid = "402bf7ad581d6d747a8712e4abd96f0d";

        // Info keys used for debugging.
        public const string InfoKey_IsAiming  = "Aim/Aiming";
        public const string InfoKey_UseInputActive  = "Use/UseInputIsActive";
        public const string InfoKey_IsTryingToStop  = "Use/IsTryingToStop";
        public const string InfoKey_ItemUseState  = "Use/ItemUseState";
        public const string InfoKey_UseAbilityActive  = "Use/UseAbilityActive";
        public const string InfoKey_CanStartUseAbility  = "Use/CanStartUseAbility";
        public const string InfoKey_CanStartUseItem  = "Use/CanStartUseItem";
        public const string InfoKey_CanUseItem  = "Use/CanUseItem";
        public const string InfoKey_IsItemUsePending  = "Use/IsItemUsePending";
        public const string InfoKey_CanStopItemUse  = "Use/CanStopItemUse";
        public const string InfoKey_CanStopAbility  = "Use/CanStopAbility";
        public const string InfoKey_UseItemSubstateIndex  = "Use/UseItemSubstateIndex";
        public const string InfoKey_StartUseCountSinceAbilityStart  = "Use/StartUseCountSinceAbilityStart";
        public const string InfoKey_UseCountSinceAbilityStart  = "Use/UseCountSinceAbilityStart";
        public const string InfoKey_UseCompleteCountSinceAbilityStart  = "Use/UseCompleteCountSinceAbilityStart";

        [Tooltip("The amount of time that must elapse before the item can be used again.")]
        [SerializeField] protected float m_UseRate = 0.1f;
        [Tooltip("Should the character rotate to face the target during use?")]
        [SerializeField] protected bool m_FaceTarget = true;
        [Tooltip("The amount of extra time it takes for the ability to stop after use.\n" +
                 "Negative values will not stop the ability on complete, 0 will stop it instantly, position value will stop it after some delay.")]
        [SerializeField] protected float m_StopUseAbilityOnCompleteDelay = 1f;
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemUse animation event or wait for the specified duration before being used.")]
        [SerializeField] protected AnimationSlotEventTrigger m_UseEvent = new AnimationSlotEventTrigger(true, 0.2f);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemUseComplete animation event or wait for the specified duration before completing the use.")]
        [SerializeField] protected AnimationSlotEventTrigger m_UseCompleteEvent = new AnimationSlotEventTrigger(false, 0.05f);
        [Tooltip("Does the item require root motion position during use?")]
        [SerializeField] protected bool m_ForceRootMotionPosition;
        [Tooltip("Does the item require root motion rotation during use?")]
        [SerializeField] protected bool m_ForceRootMotionRotation;
        [ActionModuleGroup(TriggerIconGuid)]
        [Tooltip("The module group containing the trigger modules, used for starting an action.")]
        [SerializeField] protected ActionModuleGroup<TriggerModule> m_TriggerActionModuleGroup = new ActionModuleGroup<TriggerModule>();
        [Tooltip("The module group containing the usable modules, these are generic modules which can invoke function at different stages of the usable action.")]
        [ActionModuleGroup(UsableIconGuid)]
        [SerializeField] protected ActionModuleGroup<UsableActionModule> m_UsableActionModuleGroup = new ActionModuleGroup<UsableActionModule>();

        public float UseRate { get { return m_UseRate; } set { m_UseRate = value; } }
        public bool FaceTarget { get { return m_FaceTarget; } set { m_FaceTarget = value; } }
        public AnimationSlotEventTrigger UseEvent { get { return m_UseEvent; } set { m_UseEvent.CopyFrom(value); } }
        public AnimationSlotEventTrigger UseCompleteEvent { get { return m_UseCompleteEvent; } set { m_UseCompleteEvent.CopyFrom(value); } }
        public bool ForceRootMotionPosition { get { return m_ForceRootMotionPosition; } set { m_ForceRootMotionPosition = value; } }
        public bool ForceRootMotionRotation { get { return m_ForceRootMotionRotation; } set { m_ForceRootMotionRotation = value; } }
        public ActionModuleGroup<UsableActionModule> UsableActionModuleGroup
        {
            get => m_UsableActionModuleGroup;
            set => m_UsableActionModuleGroup = value;
        }
        public ActionModuleGroup<TriggerModule> TriggerActionModuleGroup
        {
            get => m_TriggerActionModuleGroup;
            set => m_TriggerActionModuleGroup = value;
        }
        public TriggerModule MainTriggerModule => TriggerActionModuleGroup.FirstEnabledModule;
        public ILookSource LookSource => m_LookSource;

        protected Use m_UseItemAbility;
        protected ILookSource m_LookSource;
        protected CharacterItem m_FaceTargetCharacterItem;
        protected CharacterLayerManager m_CharacterLayerManager;
        protected ScheduledEventBase m_CanStopEvent;
        protected ItemSubstateIndexStreamData m_UseItemSubstateIndexStreamData;

        protected bool m_WaitForUseBeforeCanStartAgain = true;
        protected bool m_WaitForUseCompleteBeforeCanStartAgain = true;
        
        protected float m_NextAllowedUseTime;
        protected float m_LastStartUseTime;
        protected float m_LastUseTime;
        protected float m_LastUseCompleteTime;
        protected bool m_InUse;
        protected bool m_UseCompleted;
        protected bool m_UseCompleteWaitForAnimatorUpdate;

        protected int m_StartUseCountSinceAbilityStart;
        protected int m_UseCountSinceAbilityStart;
        protected int m_UseCompleteCountSinceAbilityStart;
        
        protected bool m_Aiming;
        protected bool m_RegisteredToEvents;

        public bool WaitForUseBeforeCanStartAgain { get => m_WaitForUseBeforeCanStartAgain; set => m_WaitForUseBeforeCanStartAgain = value; }
        public bool WaitForUseCompleteBeforeCanStartAgain { get => m_WaitForUseCompleteBeforeCanStartAgain; set => m_WaitForUseCompleteBeforeCanStartAgain = value; }

        public float NextAllowedUseTime => m_NextAllowedUseTime;
        public float LastStartUseTime => m_LastStartUseTime;
        public float LastUseTime => m_LastUseTime;
        public float LastUseCompleteTime => m_LastUseCompleteTime;
        public bool InUse => m_InUse;
        public bool WaitingForUseEvent => m_UseEvent.IsWaiting;
        public bool WaitingForUseCompleteEvent => m_UseCompleteEvent.IsWaiting;
        public bool UseCompleted => m_UseCompleted;
        public bool Aiming => m_Aiming;

        public bool IsTriggering => MainTriggerModule?.IsTriggering ?? false;
        public bool WasTriggered => MainTriggerModule?.WasTriggered ?? false;
        
        IDamageSource IDamageSource.OwnerDamageSource => null;
        GameObject IDamageSource.SourceOwner => m_Character;
        GameObject IDamageSource.SourceGameObject => gameObject;
        Component IDamageSource.SourceComponent => this;

        public virtual CharacterItem FaceTargetCharacterItem => m_FaceTargetCharacterItem;

        /// <summary>
        /// Initialize the item action.
        /// </summary>
        /// <param name="force">Force initialize the action?</param>
        protected override void InitializeActionInternal(bool force)
        {
            base.InitializeActionInternal(force);
            
            m_LastUseTime = -1;
            m_LastStartUseTime = -1;
            m_LastUseCompleteTime = -1;
            m_InUse = false;
            m_UseCompleted = true;
            m_UseItemSubstateIndexStreamData = CreateUseItemSubstateIndexStreamData();

            m_CharacterLayerManager = m_Character.GetCachedComponent<CharacterLayerManager>();
            
            // The item may have been added at runtime in which case the look source has already been populated.
            m_LookSource = m_CharacterLocomotion.LookSource;

            m_NextAllowedUseTime = Time.time;
            EventHandler.RegisterEvent<ILookSource>(m_Character, "OnCharacterAttachLookSource", OnAttachLookSource);
            Shared.Events.EventHandler.RegisterEvent<bool, bool>(Character, "OnAimAbilityStart", OnAim);

            if (IsDebugging) {
                m_DebugLogger.SetInfo(InfoKey_ItemUseState, "Initialized");
            }
        }
        
        /// <summary>
        /// Get all the module groups and add them to the list.
        /// </summary>
        /// <param name="groupsResult">The module group list where the groups will be added.</param>
        public override void GetAllModuleGroups(List<ActionModuleGroupBase> groupsResult)
        {
            base.GetAllModuleGroups(groupsResult);

            groupsResult.Add(m_TriggerActionModuleGroup);
            groupsResult.Add(m_UsableActionModuleGroup);
        }
        
        /// <summary>
        /// Check if the item action is valid.
        /// </summary>
        /// <returns>Returns a tuple containing if the action is valid and a string warning message.</returns>
        public override (bool isValid, string message) CheckIfValidInternal()
        {
            var (isValid, message) = base.CheckIfValidInternal();

            if (MainTriggerModule == null) {
                isValid = false;
                message += "At least one Trigger Action Module should be active.\n";
            }

            return (isValid, message);
        }

        /// <summary>
        /// Create the Item Substate Index Stream Data.
        /// </summary>
        /// <returns>The new Substate Index Stream Data.</returns>
        protected virtual ItemSubstateIndexStreamData CreateUseItemSubstateIndexStreamData()
        {
            return new ItemSubstateIndexStreamData();
        }

        /// <summary>
        /// Notifiy that item was used.
        /// </summary>
        public void NotifyItemUse()
        {
            EventHandler.ExecuteEvent<IUsableItem>(m_Character, "OnItemUse", this);
        }

        /// <summary>
        /// Notify the item use completed.
        /// </summary>
        public void NotifyItemUseComplete()
        {
            EventHandler.ExecuteEvent<IUsableItem>(m_Character, "OnItemUseComplete", this);
        }

        /// <summary>
        /// Register or unregister the use event.
        /// </summary>
        /// <param name="register">Register or Unregister the event?</param>
        public virtual void RegisterUnregisterUseEvents(bool register)
        {
            if (m_RegisteredToEvents == register) {
                return;
            }

            m_RegisteredToEvents = register;
            var eventTarget = m_Character;
            
            m_UseEvent.RegisterUnregisterEvent(register, eventTarget,"OnAnimatorItemUse",m_CharacterItem.SlotID, HandleItemUseAnimationSlotEvent);
            m_UseCompleteEvent.RegisterUnregisterEvent(register, eventTarget,"OnAnimatorItemUseComplete",m_CharacterItem.SlotID, HandleItemUseCompleteAnimationSlotEvent);
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;
        }

        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="start">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        public void OnAim(bool aim, bool inputStart)
        {
            DebugLogger.SetInfo(InfoKey_IsAiming, aim.ToString());

            m_Aiming = aim;
            InvokeOnModulesWithType(aim, inputStart, (IModuleOnAim module, bool i1, bool i2)=> module.OnAim(i1,i2));
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public virtual bool CanStartUseItem(Use useAbility, UseAbilityState abilityState)
        {
            if (WaitForUseBeforeCanStartAgain && m_UseEvent.IsWaiting) {
                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_CanStartUseItem, "(No) Waiting for Use Event");
                }
                return false;
            }
            
            if (WaitForUseCompleteBeforeCanStartAgain && m_UseCompleteEvent.IsWaiting) {
                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_CanStartUseItem, "(No) Waiting for Use Complete Event");
                }
                return false;
            }

            // Prevent the item from being used too soon.
            if (Time.time < m_NextAllowedUseTime) {
                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_CanStartUseItem, "(No) Waiting for Next Allowed Use Time");
                }
                return false;
            }
            
            var (modulesCanStart, moduleThatStopped) = InvokeOnModulesWithTypeConditional(useAbility, abilityState,
                (IModuleCanStartUseItem module, Use i1, UseAbilityState i2) => module.CanStartUseItem(i1, i2), false);

            if (modulesCanStart == false) {
                if (IsDebugging) {
                    var message = "Cannot start use item because of module " + moduleThatStopped;
                    DebugLogger.SetInfo(InfoKey_CanStartUseItem, "(No) "+message);
                    DebugLogger.Log(message);
                }

                // The item can no longer be used. Stop the ability.
                if (abilityState != UseAbilityState.Start) { useAbility.StopAbility(); }
                
                return false;
            }

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_CanStartUseItem, "(Yes)");
            }

            return true;
        }

        /// <summary>
        /// Can the Item be used?
        /// </summary>
        /// <returns>True if the item can be used.</returns>
        public virtual bool CanUseItem()
        {
            if (!m_InUse) {
                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_CanUseItem, "(No) Has Not Started Use.");
                }
                return false;
            }

            if (m_UseCompleted) {
                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_CanUseItem, "(No) Already Use Completed.");
                }
                return false;
            }

            if (m_UseEvent.IsWaiting) {
                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_CanUseItem, "(No) Waiting For Use Event.");
                }
                return false;
            }

            if (m_UseCompleteEvent.IsWaiting) {
                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_CanUseItem, "(No) Waiting For Use Complete Event.");
                }
                return false;
            }

            var (modulesCanUse, moduleThatStopped) = InvokeOnModulesWithTypeConditional(
                (IModuleCanUseItem module) => module.CanUseItem(), false);

            if (!modulesCanUse) {
                if (IsDebugging) {
                    var message = "Cannot Use Item because of module "+moduleThatStopped;
                    DebugLogger.SetInfo(InfoKey_CanUseItem, "(No) "+message);
                    DebugLogger.Log(message);
                }
                
                return false;
            }
            
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_CanUseItem, "(Yes)");
            }
            return true;
        }

        /// <summary>
        /// Can the ability be started?
        /// </summary>
        /// <param name="ability">The ability that is trying to start.</param>
        /// <returns>True if the ability can be started.</returns>
        public virtual bool CanStartAbility(Ability ability)
        {
            var (modulesCanUse, moduleThatStopped) = InvokeOnModulesWithTypeConditional(ability,
                (IModuleCanStartAbility module, Ability i1) => module.CanStartAbility(i1), false);

            if (!modulesCanUse) {
                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_CanStartUseAbility, "(No) Cannot Start Ability because of module "+moduleThatStopped);
                    DebugLogger.Log("Cannot Start Ability because of module "+moduleThatStopped);
                }
                return false;
            }
            
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_CanStartUseAbility, "(Yes)");
            }
            return true;
        }

        /// <summary>
        /// The item ability has started
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        public virtual void UseItemAbilityStarted(Use useAbility)
        {
            m_StartUseCountSinceAbilityStart = 0;
            m_UseCountSinceAbilityStart = 0;
            m_UseCompleteCountSinceAbilityStart = 0;
            
            m_LastUseTime = -1;
            m_LastStartUseTime = -1;
            m_LastUseCompleteTime = -1;
            
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_StartUseCountSinceAbilityStart, m_StartUseCountSinceAbilityStart.ToString());
                DebugLogger.SetInfo(InfoKey_UseCountSinceAbilityStart, m_UseCountSinceAbilityStart.ToString());
                DebugLogger.SetInfo(InfoKey_UseCompleteCountSinceAbilityStart, m_UseCompleteCountSinceAbilityStart.ToString());
                DebugLogger.SetInfo(InfoKey_UseAbilityActive, "(True)");
                DebugLogger.SetInfo(InfoKey_ItemUseState, "Use Ability Started");
            }
            
            m_UseItemAbility = useAbility;
            RegisterUnregisterUseEvents(true);
            m_FaceTargetCharacterItem = null;

            InvokeOnModulesWithType(useAbility, (IModuleUseItemAbilityStarted module, Use i1) => module.UseItemAbilityStarted(i1));
            
            // Start Item Use right away.
            StartItemUse(useAbility);
            
            // An Animator Audio State Set may prevent the item from being used.
            if (!IsItemInUse()) {
                SetCanStopAbility(true);
                return;
            }

            ResetCanStopEvent();
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="useAbility">The item ability that is using the item.</param>
        public virtual void StartItemUse(Use useAbility)
        {
            m_LastStartUseTime = Time.time;
            m_StartUseCountSinceAbilityStart++;
            
            if (IsDebugging) {
                DebugLogger.Log("Started Item Use");
                DebugLogger.SetInfo(InfoKey_StartUseCountSinceAbilityStart, m_StartUseCountSinceAbilityStart.ToString());
                DebugLogger.SetInfo(InfoKey_ItemUseState, "Started Item Use");
            }

            m_InUse = true;
            m_UseCompleted = false;

            if (ForceRootMotionPosition) {
                m_CharacterLocomotion.ForceRootMotionPosition = true;
            }
            if (ForceRootMotionRotation) {
                m_CharacterLocomotion.ForceRootMotionRotation = true;
            }
            if (FaceTarget && !m_CharacterLocomotion.ActiveMovementType.UseIndependentLook(true)) {
                m_FaceTargetCharacterItem = CharacterItem;
            }
            
            m_UseEvent.WaitForEvent(true);
            m_UseCompleteEvent.CancelWaitForEvent();
            
            ResetCanStopEvent();
            
            if (IsItemInUse()) {
                InvokeOnModulesWithType(useAbility, (IModuleStartItemUse module, Use i1) => module.StartItemUse(i1));
            }

            EventHandler.ExecuteEvent<IUsableItem, bool>(m_Character, "OnItemStartUse", this, true);
        }

        /// <summary>
        /// Update the character rotation.
        /// </summary>
        /// <param name="itemAbility">The item ability rotating the character.</param>
        /// <param name="rotateTowardLookAtTarget">Rotate towards the look at target?</param>
        public virtual void UpdateRotation(ItemAbility itemAbility, bool rotateTowardLookAtTarget)
        {
            // The rotation doesn't need to be updated if the item doesn't need to face the target.
            if (m_FaceTargetCharacterItem == null) {
                return;
            }

            // The look source may be null if a remote player is still being initialized.
            if (m_LookSource == null || !rotateTowardLookAtTarget) {
                return;
            }

            // Determine the direction that the character should be facing.
            var transformRotation =  m_CharacterTransform.rotation;
            var lookDirection = Vector3.ProjectOnPlane(m_LookSource.LookDirection(m_LookSource.LookPosition(true), true, m_CharacterLayerManager.IgnoreInvisibleCharacterLayers, false, false), m_CharacterLocomotion.Up);
            var rotation = transformRotation * m_CharacterLocomotion.DesiredRotation;
            var targetRotation = Quaternion.LookRotation(lookDirection, rotation * Vector3.up);
            m_CharacterLocomotion.DesiredRotation = (Quaternion.Inverse(transformRotation) * targetRotation);
        }

        /// <summary>
        /// Handle the Item Use Animation Slot Event.
        /// </summary>
        public void HandleItemUseAnimationSlotEvent()
        {
            if (IsDebugging) {
                var message = "Handling the Item Use Animation/Schedule Event";
                DebugLogger.SetInfo(InfoKey_ItemUseState, message);
                DebugLogger.Log(message);
            }
            
            m_UseEvent.CancelWaitForEvent();
            NotifyItemUse();
        }

        /// <summary>
        /// Uses the item.
        /// </summary>
        public virtual void UseItem()
        {
            if (IsDebugging) {
                DebugLogger.Log( "Use Item");
                DebugLogger.SetInfo(InfoKey_ItemUseState, "Use Item Trigger");
            }

            m_TriggerActionModuleGroup.FirstEnabledModule.UseItemTrigger();
        }
        
        /// <summary>
        /// Uses the item.
        /// </summary>
        public virtual void TriggeredUseItemAction()
        {
            m_UseCountSinceAbilityStart++;
            
            if (IsDebugging) {
                DebugLogger.Log( "Use Item");
                DebugLogger.SetInfo(InfoKey_UseCountSinceAbilityStart, m_UseCountSinceAbilityStart.ToString());
                DebugLogger.SetInfo(InfoKey_ItemUseState, "Use Item");
            }
            
            m_UseEvent.CancelWaitForEvent();
            m_LastUseTime = Time.time;

            InvokeOnModulesWithType<IModuleUseItem>(module => module.UseItem());
        }

        /// <summary>
        /// The item has been used.
        /// </summary>
        public virtual void TriggeredUseItemActionComplete()
        {
            var isItemUsePending = IsItemUsePending();
            
            if (IsDebugging) {
                DebugLogger.Log( $"Triggered Use Item Action Complete | use pending : {isItemUsePending}");
            }

            // The item needs to be used before the complete event can be called.
            
            if (!isItemUsePending) {
                m_UseCompleteEvent.WaitForEvent(true);
            }
        }

        /// <summary>
        /// Is the item in use? Includes start use.
        /// </summary>
        /// <returns>True if the item is in use.</returns>
        public virtual bool IsItemInUse() { return m_InUse; }

        /// <summary>
        /// Is the item waiting to be used? This will return true if the item is waiting to be charged or pulled back.
        /// </summary>
        /// <returns>Returns true if the item is waiting to be used.</returns>
        public virtual bool IsItemUsePending()
        {
            var (modulesUsePending, moduleThatStopped) = InvokeOnModulesWithTypeConditional((IModuleIsItemUsePending module) => module.IsItemUsePending(), true);

            if (modulesUsePending) {
                if (IsDebugging) {
                    var message = "The item is Use pending because of module "+moduleThatStopped;
                    DebugLogger.SetInfo(InfoKey_IsItemUsePending, "(Yes) "+message);
                    DebugLogger.Log(message);
                }
                
                return true;
            }

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_IsItemUsePending, "(No)");
            }

            return false;
        }

        /// <summary>
        /// Returns the substate index that the item should be in.
        /// </summary>
        /// <returns>the substate index that the item should be in.</returns>
        public virtual int GetUseItemSubstateIndex()
        {
            m_UseItemSubstateIndexStreamData.Clear();
           
            InvokeOnModulesWithType(m_UseItemSubstateIndexStreamData,
                (IModuleGetUseItemSubstateIndex module, ItemSubstateIndexStreamData i1) => module.GetUseItemSubstateIndex(i1));

            var substateIndex = m_UseItemSubstateIndexStreamData.SubstateIndex;
            if (m_UseItemSubstateIndexStreamData.Priority > -1) {
                if (IsDebugging) {
                    var dataList = m_UseItemSubstateIndexStreamData.SubstateIndexModuleDataList;
                    var message = $"{substateIndex} from modules:";

                    for (int i = 0; i < dataList.Count; i++) {
                        message += "\n\t"+dataList[i];
                    }
                    
                    DebugLogger.SetInfo(InfoKey_UseItemSubstateIndex, message);
                }
                
                return substateIndex;
            }

            substateIndex = -1;
            
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_UseItemSubstateIndex, $"{substateIndex} due to no modules with a substate index");
            }

            return substateIndex;
        }

        /// <summary>
        /// Allows the item to update while it is being used.
        /// </summary>
        public virtual void UseItemUpdate(Use useItemAbility)
        {
            var isNotTryingToStop = UseItemUpdateInternal(useItemAbility);
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_IsTryingToStop, (!isNotTryingToStop).ToString());
            }

            InvokeOnModulesWithType<IModuleUseItemUpdate>(module => module.UseItemUpdate());
        }

        /// <summary>
        /// Checks if the item should start being used or be used.
        /// </summary>
        /// <param name="useItemAbility">The use item ability.</param>
        /// <returns>Return false if the item is is trying to stop.</returns>
        private bool UseItemUpdateInternal(Use useItemAbility)
        {
            var useInputIsTryingToStop = useItemAbility.IsUseInputTryingToStop();
            
            DebugLogger.SetInfo(InfoKey_UseInputActive, (!useInputIsTryingToStop).ToString());
            
            // The Animator monitor updates with the simulation manager.
            // Give it an extra frame to avoid preventing transitions that are required for animation events.
            if (m_UseCompleteWaitForAnimatorUpdate) {
                m_UseCompleteWaitForAnimatorUpdate = false;
                return true;
            }

            if (useInputIsTryingToStop && CanStopItemUse()) {
                return false;
            }

            // The item may not be able to be used.
            if (CanUseItem()) {
                UseItem();
                EventHandler.ExecuteEvent<IUsableItem>(m_Character, "OnUseAbilityUsedItem", this);

                // Using the item may have killed the character and stopped the ability.
                if (!useItemAbility.IsActive) { return false; }

                // The ability may have been stopped immediately after use. This will happen if for example a shootable weapon automatically reloads when it
                // is out of ammo.
                // A custom use animation should be played.
                UpdateItemAbilityAnimatorParameters();
            }
            
            // Check again if the item can be stopped in case complete is instant.
            if (useInputIsTryingToStop && CanStopItemUse()) {
                return false;
            }
            
            if (CanStartUseItem(useItemAbility, UseAbilityState.Update)) {
                // In some cases the item can start without having finished complete (such as the melee combo).
                // In that case complete early and wait for the next frame.
                if (m_UseCompleteEvent.IsWaiting) {
                    ItemUseComplete();
                    return true;
                }

                StartItemUse(useItemAbility);
            }

            return true;
        }

        /// <summary>
        /// Handle the Item Use Complete Animation Slot Event.
        /// </summary>
        public void HandleItemUseCompleteAnimationSlotEvent()
        {
            if (IsDebugging) {
                var message = "Handling the Item Use Complete Animation/Schedule Event";
                DebugLogger.SetInfo(InfoKey_ItemUseState, message);
                DebugLogger.Log(message);
            }
            
            ItemUseComplete();
            NotifyItemUseComplete();
        }

        /// <summary>
        /// The item has been used.
        /// </summary>
        public virtual void ItemUseComplete()
        {
            m_UseCompleteCountSinceAbilityStart++;
            
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_UseCompleteCountSinceAbilityStart, m_UseCompleteCountSinceAbilityStart.ToString());
                DebugLogger.SetInfo(InfoKey_ItemUseState, "Item Use Complete");
            }

            // Determine next allowed use time.
            var normalizeUseRate = (m_UseRate / m_CharacterLocomotion.TimeScale);
            // Account for being able to use quicker than the framerate.
            var errorMitigation = Mathf.Max(Time.time - (m_LastUseCompleteTime != -1 ? m_LastUseCompleteTime : Time.time) - normalizeUseRate, 0);
            m_NextAllowedUseTime = Time.time + normalizeUseRate - errorMitigation;
            m_LastUseCompleteTime = Time.time;

            m_UseCompleteWaitForAnimatorUpdate = true;
            
            UpdateItemAbilityAnimatorParameters();
            m_UseCompleteEvent.CancelWaitForEvent();
            m_UseCompleted = true;

            if (m_StopUseAbilityOnCompleteDelay < 0) {
                // Don't schedule stop ability.
            } else if (m_StopUseAbilityOnCompleteDelay == 0) {
                SetCanStopAbility(true);
            } else {
                ResetCanStopEvent();
                m_CanStopEvent = Scheduler.ScheduleFixed(m_StopUseAbilityOnCompleteDelay, SetCanStopAbility, true);
            }
            
            InvokeOnModulesWithType<IModuleItemUseComplete>(module => module.ItemUseComplete());
        }
        
        /// <summary>
        /// Resets the CanStop event back to its default value.
        /// </summary>
        public virtual void ResetCanStopEvent()
        {
            // Melee weapons will not have a stop use delay so should not reset the event.
            if (m_StopUseAbilityOnCompleteDelay == 0) {
                SetCanStopAbility(true);
                return;
            }

            var stop = m_UseItemAbility.StopType == Ability.AbilityStopType.Manual && m_UseItemAbility.AIAgent;
            SetCanStopAbility(stop);
            if (m_CanStopEvent != null) {
                Scheduler.Cancel(m_CanStopEvent);
                m_CanStopEvent = null;
            }
        }

        /// <summary>
        /// Set that the ability can be stopped.
        /// </summary>
        /// <param name="canStop">True if you which the let the ability know that it can be stopped.</param>
        public virtual void SetCanStopAbility(bool canStop)
        {
            DebugLogger.SetInfo(InfoKey_CanStopAbility,$"({canStop})");

            m_UseItemAbility.SetCanStopAbility(this, canStop);
        }

        /// <summary>
        /// Tries to stop the item use.
        /// </summary>
        public bool TryStopItemUse()
        {
            TryStopItemUseInternal();
            
            // The UsableItem may not be able to be stopped (for example, if a throwable item should be used when the button press is released).
            if (!CanStopItemUse()) {
                return false;
            }

            if (!m_UseCompleted) {
                // The complete event may not have been called if the item use was still pending.
                m_UseCompleteEvent.WaitForEvent(false);
                
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Tries to stop the item use.
        /// </summary>
        public virtual void TryStopItemUseInternal()
        {
            InvokeOnModulesWithType<IModuleTryStopItemUse>(module => module.TryStopItemUse());
        }

        /// <summary>
        /// Can the item use be stopped?
        /// </summary>
        /// <returns>True if the item use can be stopped.</returns>
        public virtual bool CanStopItemUse()
        {
            var (modulesCanStopItemUse, moduleThatStopped) = InvokeOnModulesWithTypeConditional(
                (IModuleCanStopItemUse module) => module.CanStopItemUse(), false);

            if (!modulesCanStopItemUse) {
                DebugLogger.SetInfo(InfoKey_CanStopItemUse, "(No) because of "+moduleThatStopped);
                DebugLogger.Log("The Item cannot be stopped because of module " + moduleThatStopped + ".");
                return false;
            }
            
            DebugLogger.SetInfo(InfoKey_CanStopItemUse, "(Yes)");
            return true;
        }

        /// <summary>
        /// Stops the item use.
        /// </summary>
        public virtual void StopItemUse()
        {
            DebugLogger.SetInfo(InfoKey_ItemUseState, "Stop Item Use");
            
            m_UseCompleteWaitForAnimatorUpdate = false;
            m_InUse = false;
            m_UseEvent.CancelWaitForEvent();
            m_UseCompleteEvent.CancelWaitForEvent();
            
            InvokeOnModulesWithType<IModuleStopItemUse>(module => module.StopItemUse());
        }

        /// <summary>
        /// The Item ability has stopped.
        /// </summary>
        /// <param name="useAbility">The use item ability that stopped.</param>
        public virtual void ItemAbilityStopped(ItemAbility useAbility)
        {
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_UseAbilityActive, "(False)");
                DebugLogger.SetInfo(InfoKey_ItemUseState, "Use Ability Stopped");
            }
            
            if (ForceRootMotionPosition) {
                m_CharacterLocomotion.ForceRootMotionPosition = false;
            }
            if (ForceRootMotionRotation) {
                m_CharacterLocomotion.ForceRootMotionRotation = false;
            }
            
            m_UseCompleteWaitForAnimatorUpdate = false;
            RegisterUnregisterUseEvents(false);
            StopItemUse();
            EventHandler.ExecuteEvent<IUsableItem, bool>(m_GameObject, "OnItemStartUse", this, false);
            m_UseCompleted = true;
            UseEvent.CancelWaitForEvent();
            ResetCanStopEvent();
            
            InvokeOnModulesWithType(useAbility, (IModuleItemAbilityStopped module, ItemAbility i1) => module.ItemAbilityStopped(i1));
        }

        /// <summary>
        /// Should the item be unequipped?
        /// </summary>
        /// <returns>True if the item should be unequipped.</returns>
        public virtual bool ShouldUnequip()
        {
            return false;
        }

        /// <summary>
        /// Trigger the item action to be used.
        /// </summary>
        /// <param name="triggerData">The trigger data.</param>
        public virtual void TriggerItemAction(TriggerData triggerData)
        {
            TriggeredUseItemAction();
            TriggeredUseItemActionComplete();
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ILookSource>(m_Character, "OnCharacterAttachLookSource", OnAttachLookSource);
            Shared.Events.EventHandler.UnregisterEvent<bool, bool>(Character, "OnAimAbilityStart", OnAim);
        }
    }
}