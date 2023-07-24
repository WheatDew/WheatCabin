/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable;
    using Opsive.UltimateCharacterController.Objects;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// The Throwable Use Data stream used by the Throwable Character Item Action to keep track of the data.
    /// </summary>
    [Serializable]
    public class ThrowableUseDataStream
    {
        protected ThrowableAction m_ThrowableAction;
        protected TriggerData m_TriggerData;
        protected ThrowableThrowData m_ThrowData;

        public virtual ThrowableAction ThrowableAction { get => m_ThrowableAction; set => m_ThrowableAction = value; }
        public virtual TriggerData TriggerData { get => m_TriggerData; set => m_TriggerData = value; }
        public virtual ThrowableThrowData ThrowData { get => m_ThrowData; set => m_ThrowData = value; }
        public virtual ThrowableProjectileData ThrowableProjectileData { get => m_ThrowData?.ProjectileData; set => m_ThrowData.ProjectileData = value; }
        public virtual ThrowableAmmoData AmmoData { get => m_ThrowData?.ProjectileData?.AmmoData ?? ThrowableAmmoData.None; set => m_ThrowData.ProjectileData.AmmoData = value; }

        /// <summary>
        /// Initialize the use data stream.
        /// </summary>
        /// <param name="action">The item action.</param>
        public virtual void Initialize(ThrowableAction action)
        {
            m_ThrowableAction = action;
        }

        /// <summary>
        /// Reset the use data stream.
        /// </summary>
        public virtual void Reset()
        {
            TriggerData = null;
            ThrowData = null;
        }
    }

    /// <summary>
    /// The base class for the Throwable Character Item Action Modules.
    /// </summary>
    [Serializable]
    public abstract class ThrowableActionModule : ActionModule
    {
        private ThrowableAction m_ThrowableAction;
        public ThrowableAction ThrowableAction => m_ThrowableAction;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            if (itemAction is ThrowableAction throwableCharacterItemAction) {
                m_ThrowableAction = throwableCharacterItemAction;
            } else {
                Debug.LogError(
                    $"The Module Type {GetType()} does not match the character item action type {itemAction?.GetType()} ");
            }

            base.Initialize(itemAction);
        }
    }

    /// <summary>
    /// The Throwable Character Item Action is used for weapons such as an assault rifle or Bow/Arrow.
    /// </summary>
    public class ThrowableAction : UsableAction
    {
        public event Action<ThrowableThrowData> OnThrowE;
        public event Action OnReequipThrowableItemE;

        public const string ThrowerIconGuid = "27acb4e9fb8863142bac8bc75c2eedec";
        public const string ThrowableAmmoIconGuid = "04d6ba270b317ce478f47932bd0573fc";
        public const string ThrowableProjectileIconGuid = "fd204ca1148e9e049b8e9a0a9bda7727";
        public const string ThrowableEffectIconGuid = "79b9c243f4e1971488cccd45da2082ca";
        public const string ThrowableReequipperIconGuid = "7b6c80a8bffd9484a8e15898b85a2166";

        // Info keys for debugging.
        public const string InfoKey_ThrowState = "Throwable/ThrowState";
        public const string InfoKey_ThrowData = "Throwable/ThrowData";
        public const string InfoKey_AmmoDataToThrow = "Throwable/AmmoData";
        public const string InfoKey_ProjectileData = "Throwable/ProjectileData";
        public const string InfoKey_ImpactData = "Throwable/ImpactData";

        [ActionModuleGroup(ThrowerIconGuid)]
        [SerializeField] protected ActionModuleGroup<ThrowableThrowerModule> m_ThrowerModuleGroup = new ActionModuleGroup<ThrowableThrowerModule>();
        [ActionModuleGroup(ThrowableAmmoIconGuid)]
        [SerializeField] protected ActionModuleGroup<ThrowableAmmoModule> m_AmmoModuleGroup = new ActionModuleGroup<ThrowableAmmoModule>();
        [ActionModuleGroup(ThrowableProjectileIconGuid)]
        [SerializeField] protected ActionModuleGroup<ThrowableProjectileModule> m_ProjectileModuleGroup = new ActionModuleGroup<ThrowableProjectileModule>();
        [ActionModuleGroup(ThrowableEffectIconGuid)]
        [SerializeField] protected ActionModuleGroup<ThrowableThrowEffectModule> m_ThrowEffectGroup = new ActionModuleGroup<ThrowableThrowEffectModule>();
        [ActionModuleGroup(ImpactIconGuid)]
        [SerializeField] protected ActionModuleGroup<ThrowableImpactModule> m_ImpactModuleGroup = new ActionModuleGroup<ThrowableImpactModule>();
        [ActionModuleGroup(ThrowableReequipperIconGuid)]
        [SerializeField] protected ActionModuleGroup<ThrowableReequipperModule> m_ReequiperModuleGroup = new ActionModuleGroup<ThrowableReequipperModule>();
        [ActionModuleGroup(ExtraIconGuid)]
        [SerializeField] protected ActionModuleGroup<ThrowableExtraModule> m_ExtraModuleGroup = new ActionModuleGroup<ThrowableExtraModule>();
        [SerializeField] protected UnityEvent OnThrowUnityEvent;

        protected ThrowableUseDataStream m_ThrowableUseDataStream;
        public ThrowableUseDataStream ThrowableUseDataStream
        {
            get { return m_ThrowableUseDataStream; }
            set { m_ThrowableUseDataStream = value; }
        }

        protected ItemSubstateIndexStreamData m_ReloadItemSubstateIndexStreamData;

        public ActionModuleGroup<ThrowableThrowerModule> ThrowerModuleGroup { get => m_ThrowerModuleGroup; set => m_ThrowerModuleGroup = value; }
        public ActionModuleGroup<ThrowableProjectileModule> ProjectileModuleGroup { get => m_ProjectileModuleGroup; set => m_ProjectileModuleGroup = value; }
        public ActionModuleGroup<ThrowableAmmoModule> AmmoModuleGroup { get => m_AmmoModuleGroup; set => m_AmmoModuleGroup = value; }
        public ActionModuleGroup<ThrowableThrowEffectModule> ThrowEffectGroup { get => m_ThrowEffectGroup; set => m_ThrowEffectGroup = value; }
        public ActionModuleGroup<ThrowableReequipperModule> ReequiperModuleGroup { get => m_ReequiperModuleGroup; set => m_ReequiperModuleGroup = value; }
        public ActionModuleGroup<ThrowableImpactModule> ImpactModuleGroup { get => m_ImpactModuleGroup; set => m_ImpactModuleGroup = value; }
        public ActionModuleGroup<ThrowableExtraModule> ExtraModuleGroup { get => m_ExtraModuleGroup; set => m_ExtraModuleGroup = value; }

        public ThrowableThrowerModule MainThrowerModule => ThrowerModuleGroup.FirstEnabledModule;
        public ThrowableAmmoModule MainAmmoModule => m_AmmoModuleGroup.FirstEnabledModule;
        public ThrowableProjectileModule MainProjectileModule => ProjectileModuleGroup.FirstEnabledModule;
        public ThrowableReequipperModule MainReequiperModule => ReequiperModuleGroup.FirstEnabledModule;

        public virtual bool IsThrowing => IsTriggering;
        public virtual bool WasThrown => WasTriggered;
        public virtual int RemainingAmmoCount { get => AmmoModuleGroup.FirstEnabledModule.GetAmmoRemainingCount(); }
        public virtual bool IsReequipping => m_ReequiperModuleGroup.FirstEnabledModule.IsReequipping;
        public TrajectoryObject InstantiatedTrajectoryObject => m_ProjectileModuleGroup.FirstEnabledModule.InstantiatedTrajectoryObject;
        public bool ThrowableObjectIsVisible => m_ProjectileModuleGroup.FirstEnabledModule.ObjectIsVisible;
        public virtual int AmmoRemainingCount { get => MainAmmoModule.GetAmmoRemainingCount(); }

        /// <summary>
        /// Initialize the item action.
        /// </summary>
        /// <param name="force">Force initialize the action?</param>
        protected override void InitializeActionInternal(bool force)
        {
            base.InitializeActionInternal(force);

            m_ThrowableUseDataStream = CreateThrowableUseDataStream();
            m_ReloadItemSubstateIndexStreamData = CreateReloadItemSubstateIndexStreamData();
        }

        /// <summary>
        /// Get all the module groups and add them to the list.
        /// </summary>
        /// <param name="groupsResult">The module group list where the groups will be added.</param>
        public override void GetAllModuleGroups(List<ActionModuleGroupBase> groupsResult)
        {
            base.GetAllModuleGroups(groupsResult);

            groupsResult.Add(m_ThrowerModuleGroup);
            groupsResult.Add(m_AmmoModuleGroup);
            groupsResult.Add(m_ProjectileModuleGroup);
            groupsResult.Add(m_ThrowEffectGroup);
            groupsResult.Add(m_ReequiperModuleGroup);
            groupsResult.Add(m_ImpactModuleGroup);
            groupsResult.Add(m_ExtraModuleGroup);
        }

        /// <summary>
        /// Check if the item action is valid.
        /// </summary>
        /// <returns>Returns a tuple containing if the action is valid and a string warning message.</returns>
        public override (bool isValid, string message) CheckIfValidInternal()
        {
            var (isValid, message) = base.CheckIfValidInternal();

            if (MainThrowerModule == null) {
                isValid = false;
                message += "At least one Thrower Module should be active.\n";
            }

            if (MainAmmoModule == null) {
                isValid = false;
                message += "At least one Ammo Module should be active.\n";
            }

            if (MainProjectileModule == null) {
                isValid = false;
                message += "At least one Projectile Module should be active.\n";
            }

            if (MainReequiperModule == null) {
                isValid = false;
                message += "At least one Reequipper Module should be active.\n";
            }

            return (isValid, message);
        }

        /// <summary>
        /// Create the Throwable Use Data Stream.
        /// </summary>
        /// <returns>The new Throwable Use Data Stream.</returns>
        public virtual ThrowableUseDataStream CreateThrowableUseDataStream()
        {
            var useDataStream = new ThrowableUseDataStream();
            useDataStream.Initialize(this);
            return useDataStream;
        }

        /// <summary>
        /// Create the Item Substate Index Stream Data.
        /// </summary>
        /// <returns>The new Substate Index Stream Data.</returns>
        protected virtual ItemSubstateIndexStreamData CreateReloadItemSubstateIndexStreamData()
        {
            return new ItemSubstateIndexStreamData();
        }

        /// <summary>
        /// Uses the item.
        /// </summary>
        public override void UseItem()
        {
            // The Use Item starts here, reset the use data stream
            m_ThrowableUseDataStream.Reset();

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_ThrowState, "Use Item Trigger");
            }

            base.UseItem();
        }

        /// <summary>
        /// The trigger is trying to cast the weapon.
        /// </summary>
        /// <param name="triggerData">The data associated with the cast.</param>
        public override void TriggerItemAction(TriggerData triggerData)
        {
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_ThrowState, "Trigger Throw");
                DebugLogger.Log("Trigger Throw");
            }

            // The item has started to be used.
            TriggeredUseItemAction();

            m_ThrowableUseDataStream.TriggerData = triggerData;

            // The Shooter will take care of removing the ammo when getting the projectile data.
            // It will also call OnThrow when it has done firing.
            m_ThrowerModuleGroup.FirstEnabledModule.Throw(m_ThrowableUseDataStream);

            // The item can complete its use.
            TriggeredUseItemActionComplete();
        }

        /// <summary>
        /// Invoke events when the the throwable item was thrown. 
        /// </summary>
        /// <param name="throwData">The throw data.</param>
        public virtual void OnThrow(ThrowableThrowData throwData)
        {
            //The Throw data contains the Projectile and Ammo Data, so the apply Throw effects has all the information about the Throw.
            m_ThrowableUseDataStream.ThrowData = throwData;

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_ThrowState, "On Throw");
                DebugLogger.SetInfo(InfoKey_ThrowData, throwData?.ToString());
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            int invokedBitmask = 0;
#endif
            for (int i = 0; i < m_ThrowEffectGroup.EnabledModules.Count; i++) {
                m_ThrowEffectGroup.EnabledModules[i].InvokeEffect(m_ThrowableUseDataStream);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                invokedBitmask |= 1 << m_ThrowEffectGroup.EnabledModules[i].ID;
#endif
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.InvokeThrowableEffectModules(this, m_ThrowEffectGroup, invokedBitmask, m_ThrowableUseDataStream);
            }
#endif

            OnThrowE?.Invoke(throwData);

            OnThrowUnityEvent?.Invoke();
        }

        /// <summary>
        /// Get the projectile data to throw.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        /// <param name="throwPoint">The origin throw point.</param>
        /// <param name="throwDirection">The throw direction.</param>
        /// <param name="index">The index of the projectile to throw.</param>
        /// <param name="remove">Remove the projectile?</param>
        /// <returns>The throwable projectile data.</returns>
        public virtual ThrowableProjectileData GetProjectileDataToThrow(ThrowableUseDataStream dataStream, Vector3 throwPoint, Vector3 throwDirection, int index, bool remove)
        {
            var module = m_ProjectileModuleGroup.FirstEnabledModule;
            if (module == null) {
                Debug.LogError("The Throwable weapon requires a projectile module, use the BasicProjectile module if unsure which one to use");
                return null;
            }

            var projectileDataToThrow = module.GetProjectileDataToThrow(dataStream, throwPoint, throwDirection, index, remove);

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_ProjectileData, projectileDataToThrow?.ToString());
            }

            return projectileDataToThrow;
        }

        /// <summary>
        /// Get the next ammo data.
        /// </summary>
        /// <returns>The new ammo data.</returns>
        public virtual ThrowableAmmoData GetNextAmmoData()
        {
            //Index 0 is the ammo closest to getting fired
            var ammoModule = m_AmmoModuleGroup.FirstEnabledModule;
            if (ammoModule == null) {
                Debug.LogError("The Ammo Module Group should always have at least one active module.");

                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_AmmoDataToThrow, "Error, no ammo module.");
                }

                return ThrowableAmmoData.None;
            }

            var ammoDataInClip = ammoModule.GetNextAmmoData();

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_AmmoDataToThrow, ammoDataInClip.ToString());
            }

            return ammoDataInClip;
        }

        /// <summary>
        /// Get the throw preview data.
        /// </summary>
        /// <returns>Get the throw preview data.</returns>
        public ThrowableThrowData GetThrowPreviewData()
        {
            return ThrowerModuleGroup.FirstEnabledModule.GetThrowPreviewData();
        }

        /// <summary>
        /// Should the item be unequipped?
        /// </summary>
        /// <returns>True if the item should be unequipped.</returns>
        public override bool ShouldUnequip()
        {
            return RemainingAmmoCount == 0;
        }

        /// <summary>
        /// Notify that the item was re-equipped.
        /// </summary>
        public void OnReequipThrowableItem()
        {
            OnReequipThrowableItemE?.Invoke();
        }
    }
}