/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Melee;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The Melee Use Data stream used by the Melee Character Item Action to keep track of the data.
    /// </summary>
    public class MeleeUseDataStream
    {
        protected MeleeAction m_MeleeAction;
        protected TriggerData m_TriggerData;
        protected MeleeAttackData m_AttackData;
        protected MeleeCollisionData m_CollisionData;

        public virtual MeleeAction MeleeAction { get => m_MeleeAction; set => m_MeleeAction = value; }
        public virtual TriggerData TriggerData { get => m_TriggerData; set => m_TriggerData = value; }
        public virtual MeleeAttackData AttackData { get => m_AttackData; set => m_AttackData = value; }
        public virtual MeleeCollisionData CollisionData { get => m_CollisionData; set => m_CollisionData = value; }

        /// <summary>
        /// Initialize the melee use data stream.
        /// </summary>
        /// <param name="action">The melee action.</param>
        public virtual void Initialize(MeleeAction action)
        {
            m_MeleeAction = action;
        }
        
        /// <summary>
        /// Reset the data stream.
        /// </summary>
        public virtual void Reset()
        {
            m_TriggerData = null;
            m_AttackData = null;
            m_CollisionData = null;
        }
    }
    
    /// <summary>
    /// The base class for the Melee Character Item Action Modules.
    /// </summary>
    [Serializable]
    public abstract class MeleeActionModule : ActionModule
    {
        private MeleeAction m_MeleeAction;
        public MeleeAction MeleeAction => m_MeleeAction;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent item action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            if (itemAction is MeleeAction meleeCharacterItemAction) {
                m_MeleeAction =meleeCharacterItemAction;
            } else {
                Debug.LogError($"The Module Type {GetType()} does not match the character item action type {itemAction?.GetType()}.");
            }
            
            base.Initialize(itemAction);
        }

        /// <summary>
        /// Called when the Attack starts its active state.
        /// </summary>
        /// <param name="meleeUseDataStream">The use data stream.</param>
        public virtual void OnActiveAttackStart(MeleeUseDataStream meleeUseDataStream)
        {
            // To override.
        }

        /// <summary>
        /// The item has completed its active attack state.
        /// </summary>
        /// <param name="meleeUseDataStream">The use data stream.</param>
        public virtual void OnActiveAttackComplete(MeleeUseDataStream meleeUseDataStream)
        {
            // To override.
        }
    }
    
    /// <summary>
    /// The Melee Character Item Action is used for weapons such as a sword and also the Body Weapon.
    /// </summary>
    public class MeleeAction : UsableAction
    {
        public const string MeleeAttackIconGuid = "3fcb0e20017b26741b3fd57b740984a6";
        public const string CollisionIconGuid = "9991e77ffb5c1cb42a2eeacf26a14405";
        public const string AttackEffectIconGuid = "64d82b7236857c842af10865f24c3915";
        public const string RecoilIconGuid = "8cd90db6e143b404b8ec3904883f8899";
        
        // Info keys for debugging.
        public const string InfoKey_AttackState  = "Melee/AttackState";
        public const string InfoKey_CheckCollisionCount  = "Melee/CheckCollisionCountSinceAttackStart";
        public const string InfoKey_OnHitCount  = "Melee/HitCountSinceAttackStart";
        public const string InfoKey_ImpactData  = "Melee/ImpactData";
        public const string InfoKey_Recoil  = "Melee/DoRecoil";

        [ActionModuleGroup(MeleeAttackIconGuid)]
        [SerializeField] protected ActionModuleGroup<MeleeAttackModule> m_AttackModuleGroup = new ActionModuleGroup<MeleeAttackModule>();
        [ActionModuleGroup(CollisionIconGuid)]
        [SerializeField] protected ActionModuleGroup<MeleeCollisionModule> m_CollisionModuleGroup = new ActionModuleGroup<MeleeCollisionModule>();
        [ActionModuleGroup(AttackEffectIconGuid)]
        [SerializeField] protected ActionModuleGroup<MeleeAttackEffectModule> m_AttackEffectsModuleGroup = new ActionModuleGroup<MeleeAttackEffectModule>();
        [ActionModuleGroup(ImpactIconGuid)]
        [SerializeField] protected ActionModuleGroup<MeleeImpactModule> m_ImpactModuleGroup = new ActionModuleGroup<MeleeImpactModule>();
        [ActionModuleGroup(RecoilIconGuid)]
        [SerializeField] protected ActionModuleGroup<MeleeRecoilModule> m_RecoilModuleGroup = new ActionModuleGroup<MeleeRecoilModule>();
        [ActionModuleGroup(ExtraIconGuid)]
        [SerializeField] protected ActionModuleGroup<MeleeExtraModule> m_ExtraModuleGroup = new ActionModuleGroup<MeleeExtraModule>();

        protected MeleeUseDataStream m_MeleeUseDataStream;
        public MeleeUseDataStream MeleeUseDataStream
        {
            get { return m_MeleeUseDataStream; }
            set { m_MeleeUseDataStream = value; }
        }
        
        protected int m_CheckCollisionCountSinceAttackStart = 0;
        protected int m_OnHitCountSinceAttackStart = 0;

        public ActionModuleGroup<MeleeAttackModule> AttackModuleGroup { get => m_AttackModuleGroup; set => m_AttackModuleGroup = value; }
        public ActionModuleGroup<MeleeCollisionModule> CollisionModuleGroup { get => m_CollisionModuleGroup; set => m_CollisionModuleGroup = value; }
        public ActionModuleGroup<MeleeRecoilModule> RecoilModuleGroup { get => m_RecoilModuleGroup; set => m_RecoilModuleGroup = value; }
        public ActionModuleGroup<MeleeImpactModule> ImpactModuleGroup { get => m_ImpactModuleGroup; set => m_ImpactModuleGroup = value; }
        public ActionModuleGroup<MeleeAttackEffectModule> AttackEffectsModuleGroup { get => m_AttackEffectsModuleGroup; set => m_AttackEffectsModuleGroup = value; }
        public ActionModuleGroup<MeleeExtraModule> ExtraModuleGroup { get => m_ExtraModuleGroup; set => m_ExtraModuleGroup = value; }

        public MeleeAttackModule MainMeleeAttackModule => m_AttackModuleGroup.FirstEnabledModule;
        public MeleeCollisionModule MainMeleeCollisionModule => m_CollisionModuleGroup.FirstEnabledModule;
        public MeleeRecoilModule MainMeleeRecoilModule => RecoilModuleGroup.FirstEnabledModule;
        
        public bool IsAttacking => m_AttackModuleGroup.FirstEnabledModule.IsActiveAttacking;

        /// <summary>
        /// Initialize the item action.
        /// </summary>
        /// <param name="force">Force initialize the action?</param>
        protected override void InitializeActionInternal(bool force)
        {
            base.InitializeActionInternal(force);
            
            m_MeleeUseDataStream = CreateMeleeUseDataStream();
        }

        /// <summary>
        /// Get all the module groups and add them to the list.
        /// </summary>
        /// <param name="groupsResult">The module group list where the groups will be added.</param>
        public override void GetAllModuleGroups(List<ActionModuleGroupBase> groupsResult)
        {
            base.GetAllModuleGroups(groupsResult);

            groupsResult.Add(m_AttackModuleGroup);
            groupsResult.Add(m_CollisionModuleGroup);
            groupsResult.Add(m_RecoilModuleGroup);
            groupsResult.Add(m_ImpactModuleGroup);
            groupsResult.Add(m_AttackEffectsModuleGroup);
            groupsResult.Add(m_ExtraModuleGroup);
        }
        
        /// <summary>
        /// Check if the item action is valid.
        /// </summary>
        /// <returns>Returns a tuple containing if the action is valid and a string warning message.</returns>
        public override (bool isValid, string message) CheckIfValidInternal()
        {
            var (isValid, message) = base.CheckIfValidInternal();

            if (MainMeleeAttackModule == null) {
                isValid = false;
                message += "At least one Attack Module should be active.\n";
            }
            
            if (MainMeleeCollisionModule == null) {
                isValid = false;
                message += "At least one Collision Module should be active.\n";
            }

            return (isValid, message);
        }

        /// <summary>
        /// Create the Melee Use Data Stream.
        /// </summary>
        /// <returns>The new Melee Use Data Stream.</returns>
        public virtual MeleeUseDataStream CreateMeleeUseDataStream()
        {
            var useDataStream = new MeleeUseDataStream();
            useDataStream.Initialize(this);
            return useDataStream;
        }

        /// <summary>
        /// Uses the item.
        /// </summary>
        public override void UseItem()
        {
            // The Use Item starts here, reset the use data stream
            m_MeleeUseDataStream.Reset();

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_AttackState, "Use Item Trigger");
            }
            
            base.UseItem();
        }

        /// <summary>
        /// The trigger is trying to cast the weapon.
        /// </summary>
        /// <param name="triggerData">The data associated with the cast.</param>
        public override void TriggerItemAction(TriggerData triggerData)
        {
            m_CheckCollisionCountSinceAttackStart = 0;
            m_OnHitCountSinceAttackStart = 0;
            
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_AttackState, "Trigger Attack Start");
                DebugLogger.SetInfo(InfoKey_OnHitCount, m_OnHitCountSinceAttackStart.ToString());
                DebugLogger.SetInfo(InfoKey_CheckCollisionCount, m_CheckCollisionCountSinceAttackStart.ToString());
                DebugLogger.SetInfo(InfoKey_ImpactData, "No Impact Since Attack Start");
            }
            
            // The item has started to be used.
            TriggeredUseItemAction();
            
            m_MeleeUseDataStream.TriggerData = triggerData;
            MainMeleeAttackModule.AttackStart(m_MeleeUseDataStream);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.InvokeMeleeAttackModule(MainMeleeAttackModule, m_MeleeUseDataStream);
            }
#endif

            // The item can complete its use.
            TriggeredUseItemActionComplete();
        }

        /// <summary>
        /// Called when the Attack starts its active state.
        /// </summary>
        /// <param name="attackData">The attack data.</param>
        public virtual void OnActiveAttackStart(MeleeAttackData attackData)
        {
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_OnHitCount, m_OnHitCountSinceAttackStart.ToString());
                DebugLogger.SetInfo(InfoKey_CheckCollisionCount, m_CheckCollisionCountSinceAttackStart.ToString());
                DebugLogger.SetInfo(InfoKey_ImpactData, "No Impact Since Active Attack Start");
                DebugLogger.SetInfo(InfoKey_AttackState, "Active Attack Started");
                DebugLogger.Log("On Active Attack Start");
            }
            
            // Setting the data lets us differentiate the attack when using multi attacks
            m_MeleeUseDataStream.AttackData = attackData;
            if (string.IsNullOrWhiteSpace(attackData.StateName) == false) {
                StateManager.SetState(m_Character, attackData.StateName, true);
                StateManager.SetState(m_CharacterItem.gameObject, attackData.StateName, true);
            }

            for (int i = 0; i < AllModuleGroups.Count; i++) {
                var allModules = AllModuleGroups[i].EnabledBaseModules;
                for (int j = 0; j < allModules.Count; j++) {
                    if (allModules[j] is MeleeActionModule meleeActionModule) {
                        meleeActionModule.OnActiveAttackStart(m_MeleeUseDataStream);
                    }
                }
            }
        }

        /// <summary>
        /// Check for collisions using a module.
        /// </summary>
        /// <param name="attackData">The attack data.</param>
        public void CheckForCollision(MeleeAttackData attackData)
        {
            m_CheckCollisionCountSinceAttackStart++;
            
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_CheckCollisionCount, m_CheckCollisionCountSinceAttackStart.ToString());
                DebugLogger.Log("Check For Collision ");
            }
            
            m_MeleeUseDataStream.AttackData = attackData;
            
            if (MainMeleeCollisionModule == null) {
                Debug.LogError("Collision Module is missing.", this);
                return;
            }
            MainMeleeCollisionModule.CheckCollisions(m_MeleeUseDataStream);
        }
        
        /// <summary>
        /// Should the attack continue to check for collisions after this collision.
        /// Return false to interrupt or stop the attack. 
        /// </summary>
        /// <param name="collisionData">The collision data so far.</param>
        /// <returns>Returns true to continue the attack.</returns>
        public bool OnHitColliderContinue(MeleeCollisionData collisionData)
        {
            m_MeleeUseDataStream.CollisionData = collisionData;

            var recoilModule = MainMeleeRecoilModule;
            // If there is no recoil, simply continue.
            if (recoilModule == null) { return true;}

            // Stop checking collisions and recoil if something solid is hit.
            var shouldRecoil = recoilModule.CheckForSolidObject(m_MeleeUseDataStream);

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_Recoil, shouldRecoil.ToString());
            }
            
            if (shouldRecoil) {
                recoilModule.DoRecoil(m_MeleeUseDataStream);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Invoke the impact actions for the attack impact.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback data.</param>
        public virtual void OnAttackImpact(MeleeImpactCallbackContext impactCallbackContext)
        {
            m_OnHitCountSinceAttackStart++;
            
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_OnHitCount, m_OnHitCountSinceAttackStart.ToString());
                DebugLogger.SetInfo(InfoKey_ImpactData, impactCallbackContext.ToString());
                DebugLogger.Log("On Attack impact");
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
                m_NetworkCharacter.InvokeMeleeImpactModules(this, m_ImpactModuleGroup, invokedBitmask, impactCallbackContext);
            }
#endif
        }

        /// <summary>
        /// The item has been used.
        /// </summary>
        public override void ItemUseComplete()
        {
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_AttackState, "Trigger Attack Complete");
                DebugLogger.Log("On Trigger Attack Complete");
            }
            MainMeleeAttackModule.AttackComplete(m_MeleeUseDataStream);
            
            base.ItemUseComplete();
        }

        /// <summary>
        /// The item has completed its active attack state.
        /// </summary>
        public virtual void OnActiveAttackComplete(MeleeAttackData attackData)
        {
            m_MeleeUseDataStream.AttackData = attackData;
            
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_AttackState, "Active Attack Completed");
                DebugLogger.Log("On Active Attack Complete");
            }
            
            if (string.IsNullOrWhiteSpace(attackData.StateName) == false) {
                StateManager.SetState(m_Character, attackData.StateName, false);
                StateManager.SetState(m_CharacterItem.gameObject, attackData.StateName, false);
            }
            
            for (int i = 0; i < AllModuleGroups.Count; i++) {
                var allModules = AllModuleGroups[i].EnabledBaseModules;
                for (int j = 0; j < allModules.Count; j++) {
                    if (allModules[j] is MeleeActionModule meleeActionModule) {
                        meleeActionModule.OnActiveAttackComplete(m_MeleeUseDataStream);
                    }
                }
            }
        }
    }
}