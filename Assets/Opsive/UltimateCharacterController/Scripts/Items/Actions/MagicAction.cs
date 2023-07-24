/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Events;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Magic;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The Magic Use Data stream used by the Magic Character Item Action to keep track of the data.
    /// </summary>
    public class MagicUseDataStream
    {
        protected MagicAction m_MagicAction;
        protected TriggerData m_TriggerData;
        protected MagicCastData m_CastData;

        public virtual MagicAction MagicAction { get => m_MagicAction; set => m_MagicAction = value; }
        public virtual TriggerData TriggerData { get => m_TriggerData; set => m_TriggerData = value; }
        public virtual MagicCastData CastData { get => m_CastData; set => m_CastData = value; }
        
        /// <summary>
        /// Intialize the data stream.
        /// </summary>
        /// <param name="action">The magic action.</param>
        public virtual void Initialize(MagicAction action)
        {
            m_MagicAction = action;
        }
        
        /// <summary>
        /// Reset the 
        /// </summary>
        public virtual void Reset()
        {
            TriggerData = null;
            CastData = null;
        }
    }

    /// <summary>
    /// The base class for the Magic Character Item Action Modules.
    /// </summary>
    [Serializable]
    public abstract class MagicActionModule : ActionModule
    {
        private MagicAction m_MagicAction;
        public MagicAction MagicAction => m_MagicAction;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent item action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            if (itemAction is MagicAction MagicCharacterItemAction) {
                m_MagicAction = MagicCharacterItemAction;
            } else {
                Debug.LogError($"The Module Type {GetType()} does not match the character item action type {itemAction?.GetType()}.");
            }

            base.Initialize(itemAction);
        }
    }

    /// <summary>
    /// The Magic Character Item Action is used for weapons such as an assault rifle or Bow/Arrow.
    /// </summary>
    public class MagicAction : UsableAction
    {
        /// <summary>
        /// The state the magic action is in.
        /// </summary>
        public enum CastState
        {
            None,       // No state means the cast has not started or has finished.
            Begin,      // The magic item is in the Begin phase before casting.
            Casting,    // The magic item is in the Casting phase which can loop multiple times.
            End         // The magic item is in the End phase which happens after finishing casting.
        }

        // Editor Icons
        public const string c_CasterIconGuid = "30db3c82fdb5e484198ede32bd0ddbb9";
        public const string c_StartIconGuid = "bad628101ee5c9943abd3c7698cc1bb4";
        public const string c_StopIconGuid = "e67c115206aa31c4a9a8679e9c84426a";
        public const string c_CastEffectIconGuid = "6879a65286b100b4690b3be311a6a06f";

        // Info keys for debugging.
        public const string c_InfoKey_CastState  = "Magic/CastState";
        public const string c_InfoKey_CastData  = "Magic/CastData";
        public const string c_InfoKey_ActionsCasted  = "Magic/ActionsCasted";
        public const string c_InfoKey_AllActionsCasted  = "Magic/AllActionsCasted";
        public const string c_InfoKey_ImpactData  = "Magic/ImpactData";

        [ActionModuleGroup(c_CasterIconGuid)]
        [Tooltip("The Magic Caster modules defines how the magic is cast.")]
        [SerializeField] protected ActionModuleGroup<MagicCasterModule> m_CasterModuleGroup = new ActionModuleGroup<MagicCasterModule>();
        [ActionModuleGroup(c_StartIconGuid)]
        [Tooltip("The Begining module group defines what effects happen before starting the cast effects.")]
        [SerializeField] protected ActionModuleGroup<MagicStartStopModule> m_BeginModuleGroup = new ActionModuleGroup<MagicStartStopModule>();
        [ActionModuleGroup(c_CastEffectIconGuid)]
        [Tooltip("The cast effects happen repeateadly over the magic action life time.")]
        [SerializeField] protected ActionModuleGroup<MagicCastEffectModule> m_CastEffectsModuleGroup = new ActionModuleGroup<MagicCastEffectModule>();
        [ActionModuleGroup(ImpactIconGuid)]
        [Tooltip("The impact which get invoked by some of the cast effects.")]
        [SerializeField] protected ActionModuleGroup<MagicImpactModule> m_ImpactModuleGroup = new ActionModuleGroup<MagicImpactModule>();
        [ActionModuleGroup(c_StopIconGuid)]
        [Tooltip("The end effects invoked after all the casting effects are done and before ending the ability.")]
        [SerializeField] protected ActionModuleGroup<MagicStartStopModule> m_EndModuleGroup = new ActionModuleGroup<MagicStartStopModule>();
        [ActionModuleGroup(ExtraIconGuid)]
        [Tooltip("The modules containing extra functionality specific to the magic ation.")]
        [SerializeField] protected ActionModuleGroup<MagicExtraModule> m_ExtraModuleGroup = new ActionModuleGroup<MagicExtraModule>();
        [Tooltip("Unity event invoked when the begin or end actions are started or stopped.")]
        [SerializeField] protected UnityItemBoolBoolEvent m_OnStartStopBeginEndActionsEvent;
        [Tooltip("Unity event invoked when the magic item casts its actions.")]
        [SerializeField] protected UnityItemEvent m_OnCastEvent;
        
        protected MagicUseDataStream m_MagicUseDataStream;
        public MagicUseDataStream MagicUseDataStream
        {
            get { return m_MagicUseDataStream; }
            set { m_MagicUseDataStream = value; }
        }

        protected ImpactCallbackContext m_MagicImpactCallbackContext;
        public ImpactCallbackContext MagicImpactCallbackContext
        {
            get { return m_MagicImpactCallbackContext; }
            set { m_MagicImpactCallbackContext = value; }
        }

        protected CastState m_CastState;

        public ActionModuleGroup<MagicStartStopModule> BeginModuleGroup { get => m_BeginModuleGroup; set => m_BeginModuleGroup = value; }
        public ActionModuleGroup<MagicCasterModule> CasterModuleGroup { get => m_CasterModuleGroup; set => m_CasterModuleGroup = value; }
        public ActionModuleGroup<MagicCastEffectModule> CastEffectsModuleGroup { get => m_CastEffectsModuleGroup; set => m_CastEffectsModuleGroup = value; }
        public ActionModuleGroup<MagicImpactModule> ImpactModuleGroup { get => m_ImpactModuleGroup; set => m_ImpactModuleGroup = value; }
        public ActionModuleGroup<MagicStartStopModule> EndModuleGroup { get => m_EndModuleGroup; set => m_EndModuleGroup = value; }
        public ActionModuleGroup<MagicExtraModule> ExtraModuleGroup { get => m_ExtraModuleGroup; set => m_ExtraModuleGroup = value; }

        public MagicCasterModule MainMagicCaster => m_CasterModuleGroup.FirstEnabledModule;

        /// <summary>
        /// Initialize the item action.
        /// </summary>
        /// <param name="force">Force initialize the action?</param>
        protected override void InitializeActionInternal(bool force)
        {
            base.InitializeActionInternal(force);
            
            m_MagicUseDataStream = CreateMagicUseDataStream();
            m_MagicImpactCallbackContext = new ImpactCallbackContext();
            m_MagicImpactCallbackContext.SetCharacterItemAction(this);
            m_CastState = CastState.None;
        }

        /// <summary>
        /// Get all the module groups and add them to the list.
        /// </summary>
        /// <param name="groupsResult">The module group list where the groups will be added.</param>
        public override void GetAllModuleGroups(List<ActionModuleGroupBase> groupsResult)
        {
            base.GetAllModuleGroups(groupsResult);

            groupsResult.Add(m_BeginModuleGroup);
            groupsResult.Add(m_CasterModuleGroup);
            groupsResult.Add(m_CastEffectsModuleGroup);
            groupsResult.Add(m_ImpactModuleGroup);
            groupsResult.Add(m_EndModuleGroup);
            groupsResult.Add(m_ExtraModuleGroup);
        }
        
        /// <summary>
        /// Check if the item action is valid.
        /// </summary>
        /// <returns>Returns a tuple containing if the action is valid and a string warning message.</returns>
        public override (bool isValid, string message) CheckIfValidInternal()
        {
            var (isValid, message) = base.CheckIfValidInternal();

            if (MainMagicCaster == null) {
                isValid = false;
                message += "At least one Caster Module should be active.\n";
            }

            return (isValid, message);
        }

        /// <summary>
        /// Create the Magic Use Data Stream.
        /// </summary>
        /// <returns>The new Magic Use Data Stream.</returns>
        public virtual MagicUseDataStream CreateMagicUseDataStream()
        {
            var useDataStream = new MagicUseDataStream();
            useDataStream.Initialize(this);
            return useDataStream;
        }
        
        /// <summary>
        /// Create the Magic Use Data Stream.
        /// </summary>
        /// <returns>The new Magic Use Data Stream.</returns>
        public virtual ImpactCallbackContext CreateMagicImpactCallbackData()
        {
            var useDataStream = new ImpactCallbackContext();
            useDataStream.SetCharacterItemAction(this);
            return useDataStream;
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="useAbility">The item ability that is using the item.</param>
        public override void StartItemUse(Use useAbility)
        {
            base.StartItemUse(useAbility);
            
            if (IsDebugging) {
                DebugLogger.SetInfo(c_InfoKey_ImpactData, "Nothing since Start");
            }

            m_CastState = CastState.Begin;
            // The Begin Actions allows the effect to play any starting effect.
            StartStopBeginEndActions(true, true, false);
        }

        /// <summary>
        /// Uses the item.
        /// </summary>
        public override void UseItem()
        {
            // The Use Item starts here, reset the use data stream
            m_MagicUseDataStream.Reset();
            
            if (IsDebugging) {
                DebugLogger.SetInfo(c_InfoKey_CastState, "Use Item Trigger");
            }
            
            base.UseItem();
        }

        /// <summary>
        /// The trigger is trying to cast the weapon.
        /// </summary>
        /// <param name="triggerData">The data associated with the cast.</param>
        public override void TriggerItemAction(TriggerData triggerData)
        {
            DebugLogger.SetInfo(c_InfoKey_CastState, "Trigger Cast");
            DebugLogger.Log("Trigger Cast");

            // The item has started to be used.
            TriggeredUseItemAction();
            
            // Set the trigger data first in case it is used by the Begin stop actions.
            m_MagicUseDataStream.TriggerData = triggerData;

            // The shooter will take care of removing the mana when getting the projectile data.
            // It will also call OnCast when it has done firing.
            MainMagicCaster.Cast(m_MagicUseDataStream);

            // The item can complete its use.
            TriggeredUseItemActionComplete();
        }

        /// <summary>
        /// Start casting.
        /// </summary>
        /// <param name="castData">The data for casting.</param>
        public virtual void OnStartCasting(MagicCastData castData)
        {
            m_MagicUseDataStream.CastData = castData;
            
            DebugLogger.SetInfo(c_InfoKey_CastState, "On Start Casting");
            DebugLogger.SetInfo(c_InfoKey_ActionsCasted, "");
            DebugLogger.SetInfo(c_InfoKey_AllActionsCasted, "");
            DebugLogger.SetInfo(c_InfoKey_CastData, castData?.ToString());

            m_CastState = CastState.Casting;
            StartStopBeginEndActions(true, false, true);
        }
        
        /// <summary>
        /// Allows the item to update while it is being used.
        /// </summary>
        public override void UseItemUpdate(Use useItemAbility)
        {
            base.UseItemUpdate(useItemAbility);

            if (m_CastState == CastState.Begin || m_CastState == CastState.End) {
                var actionModuleGroup = m_CastState == CastState.Begin ? m_BeginModuleGroup : m_EndModuleGroup;
                if (actionModuleGroup != null) {
                    var enabledActionModules = actionModuleGroup.EnabledModules;
                    for (int i = 0; i < enabledActionModules.Count; ++i) {
                        enabledActionModules[i].Update(m_MagicUseDataStream);
                    }
                }
            } else if (m_CastState == CastState.Casting) {
                MainMagicCaster.CastUpdate();
            }
        }

        /// <summary>
        /// All actions have been casted. This can happen multiple times in a single cast
        /// </summary>
        /// <param name="individualCastCount">The individual cast count.</param>
        /// <param name="allCastCount">The all cast count.</param>
        public void OnAllActionsCasted(int individualCastCount, int allCastCount)
        {
            if (IsDebugging) {
                DebugLogger.SetInfo(c_InfoKey_AllActionsCasted, $"Individual Cast Count: {individualCastCount} | All Casted Count: {allCastCount}");
            }
            
            Shared.Events.EventHandler.ExecuteEvent(m_Character, "OnMagicItemCast", m_CharacterItem);
        }

        /// <summary>
        /// Reset the impact modules for the specified source ID.
        /// </summary>
        /// <param name="sourceID">The source id for the impact modules to reset.</param>
        public void ResetImpactModules(uint sourceID)
        {
            for (int i = 0; i < m_ImpactModuleGroup.Modules.Count; ++i) {
                m_ImpactModuleGroup.Modules[i].Reset(sourceID);
            }
        }

        /// <summary>
        /// Stop cast when the use is complete.
        /// </summary>
        public override void ItemUseComplete()
        {
            base.ItemUseComplete();

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            int invokedBitmask = 0;
#endif
            for (int i = 0; i < m_CastEffectsModuleGroup.EnabledModules.Count; ++i) {
                m_CastEffectsModuleGroup.EnabledModules[i].StopCast();
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                invokedBitmask |= 1 << m_CastEffectsModuleGroup.EnabledModules[i].ID;
#endif
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.InvokeMagicCastEffectsModules(this, m_CastEffectsModuleGroup, invokedBitmask, Networking.Character.INetworkCharacter.CastEffectState.End, m_MagicUseDataStream);
            }
#endif
        }

        /// <summary>
        /// Start the process of stopping the cast.
        /// </summary>
        /// <param name="castData">the cast data.</param>
        public virtual void OnStopCasting(MagicCastData castData)
        {
            DebugLogger.SetInfo(c_InfoKey_CastState, "On Stop Casting");
            DebugLogger.SetInfo(c_InfoKey_CastData, castData?.ToString());
            
            m_CastState = CastState.End;
            StartStopBeginEndActions(false, true, true);
        }
        
        /// <summary>
        /// A cast has caused a collision. Perform the impact actions.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hit">The raycast that caused the impact.</param>
        public void PerformImpact(uint castID, GameObject source, GameObject target, RaycastHit hit)
        {
            var impactCollisionData = m_MagicImpactCallbackContext.ImpactCollisionData;
            if (impactCollisionData == null) {
                impactCollisionData = new ImpactCollisionData();
                m_MagicImpactCallbackContext.ImpactCollisionData = impactCollisionData;
            }
            impactCollisionData.Reset();
            impactCollisionData.Initialize();
            impactCollisionData.SourceID = castID;
            impactCollisionData.SetImpactSource(source, Character);
            impactCollisionData.SourceItemAction = this;
            impactCollisionData.SetRaycast(hit);
            if (target != null) {
                impactCollisionData.SetImpactTarget(hit.collider, target);
            }

            OnCastImpact(m_MagicImpactCallbackContext);
        }

        /// <summary>
        /// A cast has caused a collision. Perform the impact actions.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="source">The object that caused the cast.</param>
        /// <param name="target">The object that was hit by the cast.</param>
        /// <param name="hitCollider">The collider hit.</param>
        /// <param name="position">The position of the hit.</param>
        /// <param name="direction">The direction of the hit</param>
        public void PerformImpact(uint castID, GameObject source, GameObject target, Collider hitCollider, Vector3 position, Vector3 direction)
        {
            var impactCollisionData = m_MagicImpactCallbackContext.ImpactCollisionData;
            if (impactCollisionData == null) {
                impactCollisionData = new ImpactCollisionData();
                m_MagicImpactCallbackContext.ImpactCollisionData = impactCollisionData;
            }
            impactCollisionData.Reset();
            impactCollisionData.Initialize();
            impactCollisionData.SourceID = castID;
            impactCollisionData.SetImpactSource(source, Character);
            impactCollisionData.SourceItemAction = this;
            impactCollisionData.ImpactPosition = position;
            impactCollisionData.ImpactDirection = direction;
            impactCollisionData.SetImpactTarget(hitCollider, target);

            OnCastImpact(m_MagicImpactCallbackContext);
        }
        
        /// <summary>
        /// A cast has caused a collision. Perform the impact actions.
        /// </summary>
        /// <param name="castID">The ID of the cast.</param>
        /// <param name="impactCallbackContext">The callback context.</param>
        public void PerformImpact(uint castID, ImpactCallbackContext impactCallbackContext)
        {
            var impactCollisionData = impactCallbackContext.ImpactCollisionData;
            if (impactCollisionData == null) {
                impactCollisionData = new ImpactCollisionData();
            }
            m_MagicImpactCallbackContext.ImpactCollisionData = impactCollisionData;
            m_MagicImpactCallbackContext.ImpactDamageData = impactCallbackContext.ImpactDamageData;

            impactCollisionData.SourceID = castID;
            impactCollisionData.SourceItemAction = this;

            OnCastImpact(m_MagicImpactCallbackContext);
        }

        /// <summary>
        /// Returns true if the object can be collided with.
        /// </summary>
        /// <param name="other">The object that may be able to be collided with.</param>
        /// <returns>True if the object can be collided with.</returns>
        private bool IsValidCollisionObject(GameObject other)
        {
            return true;
        }
        
        /// <summary>
        /// A callback when an impact happens.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback.</param>
        public virtual void OnCastImpact(ImpactCallbackContext impactCallbackContext)
        {
            if (!IsValidCollisionObject(impactCallbackContext.ImpactCollisionData.ImpactGameObject)) {
                return;
            }
            
            if (IsDebugging) {
                DebugLogger.SetInfo(c_InfoKey_ImpactData, impactCallbackContext?.ToString());
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            int invokedBitmask = 0;
#endif
            for (int i = 0; i < m_ImpactModuleGroup.EnabledModules.Count; i++) {
                m_ImpactModuleGroup.EnabledModules[i].OnImpact(impactCallbackContext);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                invokedBitmask |= 1 << m_ImpactModuleGroup.EnabledModules[i].ID;
#endif
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.InvokeMagicImpactModules(this, m_ImpactModuleGroup, invokedBitmask, impactCallbackContext);
            }
#endif
        }

        /// <summary>
        /// Stops the item use.
        /// </summary>
        public override void StopItemUse()
        {
            base.StopItemUse();
            
            // If Force Stop is true then the cast was interrupted. Reset the objects.
            if (m_CastState == CastState.Casting) {
                // Some cast effects might still be casting if it was forced to stop.
                var enabledCastModules = CastEffectsModuleGroup.EnabledModules;
                for (int i = 0; i < enabledCastModules.Count; i++) {
                    enabledCastModules[i].CastWillStop();
                    enabledCastModules[i].StopCast();
                }
                ItemUseComplete();
            }
            
            // If Force Stop is true then the cast was interrupted. Reset the objects.
            if (m_CastState == CastState.Begin) {
                StartStopBeginEndActions(true, false, false);
                ItemUseComplete();
            }

            StartStopBeginEndActions(false, false, false);
        }

        /// <summary>
        /// Get the cast preview data before the cast even starts.
        /// </summary>
        /// <returns>The cast data.</returns>
        public MagicCastData GetCastPreviewData()
        {
            return CasterModuleGroup.FirstEnabledModule.GetCastPreviewData();
        }

        /// <summary>
        /// Starts or stops the begin or end actions.
        /// </summary>
        /// <param name="beginActions">Should the begin actions be started?</param>
        /// <param name="start">Should the actions be started?</param>
        /// <param name="networkEvent">Should the event be sent over the network?</param>
        public virtual void StartStopBeginEndActions(bool beginActions, bool start, bool networkEvent)
        {
            // On start preview the cast data since it does not exist yet.
            if (start) {
                m_MagicUseDataStream.CastData = GetCastPreviewData();
            }

            var actionModuleGroup = beginActions ? m_BeginModuleGroup : m_EndModuleGroup;
            if (actionModuleGroup != null) {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                int invokedBitmask = 0;
#endif
                for (int i = 0; i < actionModuleGroup.EnabledModules.Count; ++i) {
                    if (start) {
                        actionModuleGroup.EnabledModules[i].Start(m_MagicUseDataStream);
                    } else {
                        actionModuleGroup.EnabledModules[i].Stop(m_MagicUseDataStream);
                    }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                    invokedBitmask |= 1 << actionModuleGroup.EnabledModules[i].ID;
#endif
                }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                    m_NetworkCharacter.InvokeMagicBeginEndModules(this, actionModuleGroup, invokedBitmask, start, m_MagicUseDataStream);
                }
#endif
            }

            // Notify those interested that the actions have been started or stopped.
            EventHandler.ExecuteEvent(m_Character, "OnMagicItemStartStopBeginEndActions", m_CharacterItem, beginActions, start);
            if (m_OnStartStopBeginEndActionsEvent != null) {
                m_OnStartStopBeginEndActionsEvent.Invoke(m_CharacterItem, beginActions, start);
            }
        }
    }
}