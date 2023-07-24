/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The Shootable Use Data stream used by the Shootable Character Item Action to keep track of the data.
    /// </summary>
    public class ShootableUseDataStream
    {
        protected ShootableAction m_ShootableAction;
        protected TriggerData m_TriggerData;
        protected ShootableFireData m_FireData;

        public virtual ShootableAction ShootableAction { get => m_ShootableAction; set => m_ShootableAction = value; }
        public virtual TriggerData TriggerData { get => m_TriggerData; set => m_TriggerData = value; }
        public virtual ShootableFireData FireData { get => m_FireData; set => m_FireData = value; }

        /// <summary>
        /// Initialize the use data stream.
        /// </summary>
        /// <param name="action">The shootable action.</param>
        public virtual void Initialize(ShootableAction action)
        {
            m_ShootableAction = action;
        }

        /// <summary>
        /// Reset the data stream.
        /// </summary>
        public virtual void Reset()
        {
            TriggerData = null;
            FireData = null;
        }
    }

    /// <summary>
    /// The base class for the Shootable Character Item Action Modules.
    /// </summary>
    [Serializable]
    public abstract class ShootableActionModule : ActionModule
    {
        private ShootableAction m_ShootableAction;
        public ShootableAction ShootableAction => m_ShootableAction;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent item action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            if (itemAction is ShootableAction shootableCharacterItemAction) {
                m_ShootableAction = shootableCharacterItemAction;
            } else {
                Debug.LogError($"The Module Type {GetType()} does not match the character item action type {itemAction?.GetType()}.");
            }

            base.Initialize(itemAction);
        }
    }

    /// <summary>
    /// The Shootable Character Item Action is used for weapons such as an assault rifle or Bow/Arrow.
    /// </summary>
    public class ShootableAction : UsableAction, IReloadableItem
    {
        // Editor Icons.
        public const string ShooterIconGuid = "70960e2cc317fc8459bd34760700e24a";
        public const string ShootableAmmoIconGuid = "8d62be6a10c8de04c9d293573d56d924";
        public const string SootableProjectileIconGuid = "5eac81c05eadeef4ca4c97be76e8b307";
        public const string ShootableClipIconGuid = "d43f6771d4bde2d4f9664fc1d362a7e7";
        public const string ShootableFireEffectIconGuid = "42992282312c39d44b5c5d4a03ce3a37";
        public const string ShootableDryFireEffectIconGuid = "dfc32949868d6a649bdbe6d2ad4b66fe";
        public const string ShootableReloaderIconGuid = "e81d5f5b655d33f4a89f7231f0b4b02e";

        // Info keys for debugging.
        public const string InfoKey_ReloadItemSubstateIndex = "Reload/ReloadItemSubstateIndex";
        public const string InfoKey_CanReload = "Reload/CanReload";
        public const string InfoKey_ShouldReload = "Reload/ShouldReload";
        public const string InfoKey_FireState = "Shootable/FireState";
        public const string InfoKey_FireData = "Shootable/FireData";
        public const string InfoKey_AmmoDataToFire = "Shootable/AmmoData";
        public const string InfoKey_ProjectileData = "Shootable/ProjectileData";
        public const string InfoKey_ImpactData = "Shootable/ImpactData";

        [ActionModuleGroup(ShooterIconGuid)]
        [SerializeField] protected ActionModuleGroup<ShootableShooterModule> m_ShooterModuleGroup = new ActionModuleGroup<ShootableShooterModule>();
        [ActionModuleGroup(ShootableAmmoIconGuid)]
        [SerializeField] protected ActionModuleGroup<ShootableAmmoModule> m_AmmoModuleGroup = new ActionModuleGroup<ShootableAmmoModule>();
        [ActionModuleGroup(ShootableClipIconGuid)]
        [SerializeField] protected ActionModuleGroup<ShootableClipModule> m_ClipModuleGroup = new ActionModuleGroup<ShootableClipModule>();
        [ActionModuleGroup(SootableProjectileIconGuid)]
        [SerializeField] protected ActionModuleGroup<ShootableProjectileModule> m_ProjectileModuleGroup = new ActionModuleGroup<ShootableProjectileModule>();
        [ActionModuleGroup(ShootableFireEffectIconGuid)]
        [SerializeField] protected ActionModuleGroup<ShootableFireEffectModule> m_FireEffectsModuleGroup = new ActionModuleGroup<ShootableFireEffectModule>();
        [ActionModuleGroup(ShootableDryFireEffectIconGuid)]
        [SerializeField] protected ActionModuleGroup<ShootableFireEffectModule> m_DryFireEffectsModuleGroup = new ActionModuleGroup<ShootableFireEffectModule>();
        [ActionModuleGroup(ImpactIconGuid)]
        [SerializeField] protected ActionModuleGroup<ShootableImpactModule> m_ImpactModuleGroup = new ActionModuleGroup<ShootableImpactModule>();
        [ActionModuleGroup(ShootableReloaderIconGuid)]
        [SerializeField] protected ActionModuleGroup<ShootableReloaderModule> m_ReloaderModuleGroup = new ActionModuleGroup<ShootableReloaderModule>();
        [ActionModuleGroup(ExtraIconGuid)]
        [SerializeField] protected ActionModuleGroup<ShootableExtraModule> m_ExtraModuleGroup = new ActionModuleGroup<ShootableExtraModule>();

        protected ShootableUseDataStream m_ShootableUseDataStream;
        public ShootableUseDataStream ShootableUseDataStream
        {
            get { return m_ShootableUseDataStream; }
            set { m_ShootableUseDataStream = value; }
        }

        protected ItemSubstateIndexStreamData m_ReloadItemSubstateIndexStreamData;

        public ActionModuleGroup<ShootableShooterModule> ShooterModuleGroup { get => m_ShooterModuleGroup; set => m_ShooterModuleGroup = value; }
        public ActionModuleGroup<ShootableAmmoModule> AmmoModuleGroup { get => m_AmmoModuleGroup; set => m_AmmoModuleGroup = value; }
        public ActionModuleGroup<ShootableClipModule> ClipModuleGroup { get => m_ClipModuleGroup; set => m_ClipModuleGroup = value; }
        public ActionModuleGroup<ShootableProjectileModule> ProjectileModuleGroup { get => m_ProjectileModuleGroup; set => m_ProjectileModuleGroup = value; }
        public ActionModuleGroup<ShootableImpactModule> ImpactModuleGroup { get => m_ImpactModuleGroup; set => m_ImpactModuleGroup = value; }
        public ActionModuleGroup<ShootableFireEffectModule> FireEffectsModuleGroup { get => m_FireEffectsModuleGroup; set => m_FireEffectsModuleGroup = value; }
        public ActionModuleGroup<ShootableFireEffectModule> DryFireEffectsModuleGroup { get => m_DryFireEffectsModuleGroup; set => m_DryFireEffectsModuleGroup = value; }
        public ActionModuleGroup<ShootableReloaderModule> ReloaderModuleGroup { get => m_ReloaderModuleGroup; set => m_ReloaderModuleGroup = value; }
        public ActionModuleGroup<ShootableExtraModule> ExtraModuleGroup { get => m_ExtraModuleGroup; set => m_ExtraModuleGroup = value; }

        public ShootableShooterModule MainShooterModule => m_ShooterModuleGroup.FirstEnabledModule;
        public ShootableAmmoModule MainAmmoModule => m_AmmoModuleGroup.FirstEnabledModule;
        public ShootableClipModule MainClipModule => m_ClipModuleGroup.FirstEnabledModule;
        public ShootableProjectileModule MainProjectileModule => m_ProjectileModuleGroup.FirstEnabledModule;
        public ShootableReloaderModule MainReloaderModule => m_ReloaderModuleGroup.FirstEnabledModule;

        public int ClipSize { get => MainClipModule.ClipSize; }
        public virtual int ClipRemainingCount { get => m_IsInitialized ? MainClipModule.ClipRemainingCount : 0; }
        public virtual int AmmoRemainingCount { get => MainAmmoModule.GetAmmoRemainingCount(); }

        /// <summary>
        /// Initialize the item action.
        /// </summary>
        /// <param name="force">Force initialize the action?</param>
        protected override void InitializeActionInternal(bool force)
        {
            base.InitializeActionInternal(force);

            m_ShootableUseDataStream = CreateShootableUseDataStream();
            m_ReloadItemSubstateIndexStreamData = CreateReloadItemSubstateIndexStreamData();
        }

        /// <summary>
        /// Get all the module groups and add them to the list.
        /// </summary>
        /// <param name="groupsResult">The module group list where the groups will be added.</param>
        public override void GetAllModuleGroups(List<ActionModuleGroupBase> groupsResult)
        {
            base.GetAllModuleGroups(groupsResult);

            groupsResult.Add(m_ShooterModuleGroup);
            groupsResult.Add(m_AmmoModuleGroup);
            groupsResult.Add(m_ClipModuleGroup);
            groupsResult.Add(m_ProjectileModuleGroup);
            groupsResult.Add(m_FireEffectsModuleGroup);
            groupsResult.Add(m_DryFireEffectsModuleGroup);
            groupsResult.Add(m_ImpactModuleGroup);
            groupsResult.Add(m_ReloaderModuleGroup);
            groupsResult.Add(m_ExtraModuleGroup);
        }

        /// <summary>
        /// Check if the item action is valid.
        /// </summary>
        /// <returns>Returns a tuple containing if the action is valid and a string warning message.</returns>
        public override (bool isValid, string message) CheckIfValidInternal()
        {
            var (isValid, message) = base.CheckIfValidInternal();

            if (MainShooterModule == null) {
                isValid = false;
                message += "At least one Shooter Module should be active.\n";
            }

            if (MainAmmoModule == null) {
                isValid = false;
                message += "At least one Ammo Module should be active.\n";
            }

            if (MainClipModule == null) {
                isValid = false;
                message += "At least one Clip Module should be active.\n";
            }

            if (MainProjectileModule == null) {
                isValid = false;
                message += "At least one Projectile Module should be active.\n";
            }

            if (MainReloaderModule == null) {
                isValid = false;
                message += "At least one Reloader Module should be active.\n";
            }

            return (isValid, message);
        }

        /// <summary>
        /// Create the Shootable Use Data Stream.
        /// </summary>
        /// <returns>The new Shootable Use Data Stream.</returns>
        public virtual ShootableUseDataStream CreateShootableUseDataStream()
        {
            var useDataStream = new ShootableUseDataStream();
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
            // The Use Item starts here, reset the use data stream.
            m_ShootableUseDataStream.Reset();

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_FireState, "Use Item Trigger");
            }

            base.UseItem();
        }

        /// <summary>
        /// The trigger is trying to cast the weapon.
        /// </summary>
        /// <param name="triggerData"></param>
        public override void TriggerItemAction(TriggerData triggerData)
        {
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_FireState, "Trigger Fire");
                DebugLogger.Log("Trigger Fire");
            }

            // The item has started to be used.
            TriggeredUseItemAction();

            m_ShootableUseDataStream.TriggerData = triggerData;

            // The shooter will take care of removing the ammo when getting the projectile data.
            // It will also call OnFire when it has done firing.
            m_ShooterModuleGroup.FirstEnabledModule.Fire(m_ShootableUseDataStream);

            // The item can complete its use.
            TriggeredUseItemActionComplete();
        }

        /// <summary>
        /// On fire, invoke the fire effects.
        /// </summary>
        /// <param name="fireData">The fire data.</param>
        public virtual void OnFire(ShootableFireData fireData)
        {
            // The fire data contains the Projectile and Ammo Data, so the apply fire effects has all the information about the fire.
            m_ShootableUseDataStream.FireData = fireData;

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_FireState, "On Fire");
                DebugLogger.SetInfo(InfoKey_FireData, fireData?.ToString());
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            int invokedBitmask = 0;
#endif
            for (int i = 0; i < m_FireEffectsModuleGroup.EnabledModules.Count; i++) {
                m_FireEffectsModuleGroup.EnabledModules[i].InvokeEffects(m_ShootableUseDataStream);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                invokedBitmask |= 1 << m_FireEffectsModuleGroup.EnabledModules[i].ID;
#endif
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (invokedBitmask > 0 && m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.InvokeShootableFireEffectModules(this, m_FireEffectsModuleGroup, invokedBitmask, m_ShootableUseDataStream);
            }
#endif
        }

        /// <summary>
        /// On Dry Fire, invoke the dry fire effects.
        /// </summary>
        /// <param name="fireData">The fire data.</param>
        public virtual void OnDryFire(ShootableFireData fireData)
        {
            // The fire data contains the Projectile and Ammo Data, so the apply fire effects has all the information about the fire.
            m_ShootableUseDataStream.FireData = fireData;

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_FireState, "On Dry Fire");
                DebugLogger.SetInfo(InfoKey_FireData, fireData?.ToString());
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            int invokedBitmask = 0;
#endif
            for (int i = 0; i < m_DryFireEffectsModuleGroup.EnabledModules.Count; i++) {
                m_DryFireEffectsModuleGroup.EnabledModules[i].InvokeEffects(m_ShootableUseDataStream);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                invokedBitmask |= 1 << m_DryFireEffectsModuleGroup.EnabledModules[i].ID;
#endif
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.InvokeShootableDryFireEffectModules(this, m_DryFireEffectsModuleGroup, invokedBitmask, m_ShootableUseDataStream);
            }
#endif
        }

        /// <summary>
        /// On fire impact, invoke the impact actions.
        /// </summary>
        /// <param name="shootableImpactCallbackContext">The shootable impact callback data.</param>
        public virtual void OnFireImpact(ShootableImpactCallbackContext shootableImpactCallbackContext)
        {
            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_ImpactData, shootableImpactCallbackContext?.ToString());
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            int invokedBitmask = 0;
#endif
            for (int i = 0; i < m_ImpactModuleGroup.EnabledModules.Count; i++) {
                m_ImpactModuleGroup.EnabledModules[i].OnImpact(shootableImpactCallbackContext);
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                invokedBitmask |= 1 << m_ImpactModuleGroup.EnabledModules[i].ID;
#endif
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.InvokeShootableImpactModules(this, m_ImpactModuleGroup, invokedBitmask, shootableImpactCallbackContext);
            }
#endif
        }

        /// <summary>
        /// Get the Fire preview data from the shooter module.
        /// </summary>
        /// <returns>The preview fire data.</returns>
        public ShootableFireData GetFirePreviewData()
        {
            return ShooterModuleGroup.FirstEnabledModule.GetFirePreviewData();
        }

        /// <summary>
        /// Get the preview projectile data to fire next.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="position">The origin position.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="index">The projectile index.</param>
        /// <returns>The preview shootable projectile data.</returns>
        public virtual ShootableProjectileData GetPreviewProjectileDataToFire(ShootableUseDataStream dataStream, Vector3 position, Vector3 direction, int index)
        {
            var module = m_ProjectileModuleGroup.FirstEnabledModule;
            if (module == null) {
                Debug.LogError("The shootable weapon requires a projectile module. Use the BasicProjectile module if unsure which one to use.");
                return null;
            }

            var projectileDataToFire = module.GetPreviewProjectileData(dataStream, position, direction, index);

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_ProjectileData, projectileDataToFire?.ToString());
            }

            return projectileDataToFire;
        }

        /// <summary>
        /// Get the projectile data to fire next.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="firePoint">The origin fire position.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <param name="ammoData">The ammo data of the projectile to fire.</param>
        /// <param name="remove">Remove the projectile?</param>
        /// <param name="destroy">Destroy the projectile?</param>
        /// <returns>The shootable projectile data to fire next.</returns>
        public virtual ShootableProjectileData GetProjectileDataToFire(ShootableUseDataStream dataStream, Vector3 firePoint, Vector3 fireDirection, ShootableAmmoData ammoData, bool remove, bool destroy)
        {
            var module = MainProjectileModule;
            if (module == null) {
                Debug.LogError("The shootable weapon requires a projectile module. Use the BasicProjectile module if unsure which one to use.");
                return null;
            }

            var projectileDataToFire = module.GetProjectileDataToFire(dataStream, firePoint, fireDirection, ammoData, remove, destroy);

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_ProjectileData, projectileDataToFire?.ToString());
            }

            return projectileDataToFire;
        }

        /// <summary>
        /// Get the ammo data within the clip at the specified index.
        /// </summary>
        /// <param name="index">The ammo data index. Index 0 is the next ammo to be used.</param>
        /// <returns>The ammo data at the index specified within the clip.</returns>
        public virtual ShootableAmmoData GetAmmoDataInClip(int index)
        {
            // Index 0 is the ammo closest to getting fired.
            var clipModule = m_ClipModuleGroup.FirstEnabledModule;
            if (clipModule == null) {
                Debug.LogError("The Clip Module Group should always have at least one active module.");

                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_AmmoDataToFire, "Error, no clip module.");
                }

                return ShootableAmmoData.None;
            }

            var ammoDataInClip = clipModule.GetAmmoDataInClip(index);

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_AmmoDataToFire, ammoDataInClip.ToString());
            }

            return ammoDataInClip;
        }

        /// <summary>
        /// Should the item be unequipped?
        /// </summary>
        /// <returns>True if the item should be unequipped.</returns>
        public override bool ShouldUnequip()
        {
            return ClipRemainingCount == 0;
        }

        /// <summary>
        /// Reload the clip.
        /// </summary>
        /// <param name="instantly">Instantly reload or wait for an animation event?</param>
        /// <param name="fullClip">Reload the entire clip or one by one?</param>
        public virtual void ReloadClip(bool instantly, bool fullClip)
        {
            if (instantly) {
                InvokeOnModulesWithType(fullClip, (IModuleReloadClip module, bool i1) => module.ReloadClip(i1));
            } else {
                var reloadAbility = CharacterLocomotion.GetItemAbility<Reload>(m_CharacterItem.SlotID, m_ID);
                CharacterLocomotion.TryStartAbility(reloadAbility);
            }
        }

        /// <summary>
        /// Is the shootable action reloading?
        /// </summary>
        /// <returns>True if it is currently reloading the item.</returns>
        public virtual bool IsReloading()
        {
            return MainReloaderModule.IsReloading();
        }

        /// <summary>
        /// Has the shootable action reloaded the clip?
        /// </summary>
        /// <returns>True if the item has added the ammo to the clip.</returns>
        public virtual bool HasReloaded()
        {
            return MainReloaderModule.HasReloaded();
        }

        public virtual IReloadableItemModule ReloadableItemModule => m_ReloaderModuleGroup.FirstEnabledModule;

        /// <summary>
        /// Returns the reload item substate index used to animate the item.
        /// </summary>
        /// <returns>The reload item substate index.</returns>
        public virtual int GetReloadItemSubstateIndex()
        {
            m_ReloadItemSubstateIndexStreamData.Clear();

            InvokeOnModulesWithType(m_ReloadItemSubstateIndexStreamData, (IModuleGetReloadItemSubstateIndex module, ItemSubstateIndexStreamData i1) => module.GetReloadItemSubstateIndex(i1));

            var substateIndex = m_ReloadItemSubstateIndexStreamData.SubstateIndex;
            if (m_ReloadItemSubstateIndexStreamData.Priority > -1) {
                if (IsDebugging) {
                    var dataList = m_ReloadItemSubstateIndexStreamData.SubstateIndexModuleDataList;
                    var message = $"{substateIndex} from modules:";

                    for (int i = 0; i < dataList.Count; i++) {
                        message += "\n\t" + dataList[i];
                    }

                    DebugLogger.SetInfo(InfoKey_ReloadItemSubstateIndex, message);
                }

                return substateIndex;
            }

            substateIndex = -1;

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_ReloadItemSubstateIndex, $"{substateIndex} due to no modules with substate index.");
            }

            return substateIndex;
        }

        /// <summary>
        /// Start reloading the item.
        /// </summary>
        public virtual void StartItemReload()
        {
            InvokeOnModulesWithType((IModuleStartItemReload module) => module.StartItemReload());
        }

        /// <summary>
        /// Can the item reload?
        /// </summary>
        /// <param name="checkEquipStatus">Check if the item is equipped?</param>
        /// <returns>True if it can be reloaded.</returns>
        public virtual bool CanReloadItem(bool checkEquipStatus)
        {
            var (modulesCanStart, moduleThatStopped) = InvokeOnModulesWithTypeConditional(checkEquipStatus,
                (IModuleCanReloadItem module, bool i1) =>
                    module.CanReloadItem(i1), false);

            if (!modulesCanStart) {
                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_CanReload, "(No) because of module: " + moduleThatStopped);
                    DebugLogger.Log("Cannot Reload because of module: " + moduleThatStopped);
                }

                return false;
            }

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_CanReload, "(Yes)");
            }

            return true;
        }

        /// <summary>
        /// Reload the item.
        /// </summary>
        /// <param name="fullClip">Reload the full clip or just one ammo?</param>
        public virtual void ReloadItem(bool fullClip)
        {
            if (IsDebugging) {
                DebugLogger.Log("Reload Item with fullClip " + fullClip);
            }

            InvokeOnModulesWithType(fullClip, (IModuleReloadItem module, bool i1) => module.ReloadItem(i1));
        }

        /// <summary>
        /// The item reload has completed.
        /// </summary>
        /// <param name="success">Did it complete successfully?</param>
        /// <param name="immediateReload">Was it an immediate reload?</param>
        public virtual void ItemReloadComplete(bool success, bool immediateReload)
        {
            InvokeOnModulesWithType(success, immediateReload,
                (IModuleItemReloadComplete module, bool i1, bool i2) => module.ItemReloadComplete(i1, i2));
        }

        /// <summary>
        /// Should the item be reloaded?
        /// </summary>
        /// <param name="characterItem">The character item to check for reload.</param>
        /// <param name="ammoItemIdentifier">The item identifier that could be the ammo.</param>
        /// <param name="fromPickup">Was the item identifier added from pickup?</param>
        /// <returns>True if the item should reload.</returns>
        public virtual bool ShouldReload(CharacterItem characterItem, IItemIdentifier ammoItemIdentifier, bool fromPickup)
        {
            var (modulesCanStart, moduleThatStopped) = InvokeOnModulesWithTypeConditional(fromPickup,
                (IModuleShouldReload module, bool i1) => module.ShouldReload(ammoItemIdentifier, i1), false);

            if (!modulesCanStart) {
                if (IsDebugging) {
                    DebugLogger.SetInfo(InfoKey_ShouldReload, "(No) because of module: " + moduleThatStopped);
                    DebugLogger.Log("Should not Reload because of module: " + moduleThatStopped);
                }

                return false;
            }

            if (IsDebugging) {
                DebugLogger.SetInfo(InfoKey_ShouldReload, "(Yes)");
            }

            return true;
        }
    }
}