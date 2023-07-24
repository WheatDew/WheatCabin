/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable
{
    using Opsive.Shared.Game;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The shootable clip module contains information about the clip containing ammo for the weapon.
    /// </summary>
    [Serializable]
    public abstract class ShootableClipModule : ShootableActionModule, IModuleReloadClip
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;

        /// <summary>
        /// The number of ammo remaining in the clip.
        /// </summary>
        public abstract int ClipRemainingCount { get; }
        
        /// <summary>
        /// The maximum amount of ammo the clip can contain.
        /// </summary>
        public abstract int ClipSize { get; set; }

        /// <summary>
        /// Is the clip full
        /// </summary>
        /// <returns></returns>
        public virtual bool IsClipFull()
        {
            // Don't reload if the clip size is infinitely large or at capacity.
            if (ClipSize == int.MaxValue || ClipRemainingCount == ClipSize) { return true; }

            return false;
        }

        /// <summary>
        /// Is the clip empty?
        /// </summary>
        /// <returns>True if the clip is empty.</returns>
        public virtual bool IsClipEmpty()
        {
            return ClipRemainingCount == 0;
        }

        /// <summary>
        /// Set the clip remaining manually.
        /// </summary>
        /// <param name="targetClipRemainingCount">The clip remaining count to set.</param>
        public abstract void SetClipRemaining(int targetClipRemainingCount);

        /// <summary>
        /// Reload the clip.
        /// </summary>
        /// <param name="fullClip">Reload the clip completely?</param>
        public abstract void ReloadClip(bool fullClip);

        /// <summary>
        /// Get the ammo within the clip at the specified index.
        /// </summary>
        /// <param name="index">The ammo at the specific index.</param>
        /// <returns>The shootable ammo data at the index specified.</returns>
        public abstract ShootableAmmoData GetAmmoDataInClip(int index);

        /// <summary>
        /// Notify that the ammo was used.
        /// </summary>
        /// <param name="amountUsed">The number of ammo that was used.</param>
        /// <param name="startIndex">The ammo index at which the ammo started to be used.</param>
        public abstract void AmmoUsed(int amountUsed, int startIndex = 0);

        /// <summary>
        /// Notify that the ammo was removed.
        /// </summary>
        /// <param name="amountToRemove">The number of ammo that was removed.</param>
        /// <param name="startIndex">The ammo index at which the ammo started to be removed.</param>
        public abstract void RemoveAmmo(int amountToRemove, int startIndex = 0);

        /// <summary>
        /// Empty the clip completely.
        /// </summary>
        /// <param name="notify">Notify that the clip changed.</param>
        public abstract void EmptyClip(bool notify);

        /// <summary>
        /// Notify that the ammo within the clip has changed.
        /// </summary>
        public virtual void NotifyClipChange()
        {
            Shared.Events.EventHandler.ExecuteEvent<CharacterItem, ShootableClipModule>(Character, "OnShootableItemClipChange", CharacterItem, this);
        }
    }

    /// <summary>
    /// A simple clip that can contain a fixed amount of ammo.
    /// </summary>
    [Serializable]
    public class SimpleClip : ShootableClipModule
    {
        [Tooltip("The number of rounds in the clip.")]
        [SerializeField] protected int m_ClipSize = 50;
        
        protected List<ShootableAmmoData> m_ClipRemaining;

        public List<ShootableAmmoData> ClipRemaining
        {
            get { return m_ClipRemaining; }
            private set
            {
                m_ClipRemaining = value;
                NotifyClipChange();
            }
        }

        public override int ClipSize { get { return m_ClipSize; } set { m_ClipSize = value; } }
        public override int ClipRemainingCount => m_ClipRemaining.Count;
        
        protected int m_TotalReloadAmount;
        protected int m_TotalClipAmount;
        protected int m_TotalAmmoAmount;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);

            m_ClipRemaining = new List<ShootableAmmoData>();
        }

        /// <summary>
        /// Get the ammo within the clip at the specified index.
        /// </summary>
        /// <param name="index">The ammo at the specific index.</param>
        /// <returns>The shootable ammo data at the index specified.</returns>
        public override ShootableAmmoData GetAmmoDataInClip(int index)
        {
            if (index < 0 || index >= m_ClipRemaining.Count) { return ShootableAmmoData.None; }

            if (m_ClipRemaining[index].Valid == false) { return ShootableAmmoData.None; }

            m_ClipRemaining[index] = m_ClipRemaining[index].CopyWithIndex(index);

            return m_ClipRemaining[index];
        }

        /// <summary>
        /// Notify that the ammo was used.
        /// </summary>
        /// <param name="amountUsed">The number of ammo that was used.</param>
        /// <param name="startIndex">The ammo index at which the ammo started to be used.</param>
        public override void AmmoUsed(int amountUsed, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex >= m_ClipRemaining.Count) { return; }

            m_ClipRemaining.RemoveRange(startIndex, amountUsed);
            NotifyClipChange();
        }

        /// <summary>
        /// Notify that the ammo was removed.
        /// </summary>
        /// <param name="amountToRemove">The number of ammo that was removed.</param>
        /// <param name="startIndex">The ammo index at which the ammo started to be removed.</param>
        public override void RemoveAmmo(int amountToRemove, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex >= m_ClipRemaining.Count) { return; }

            m_ClipRemaining.RemoveRange(startIndex, amountToRemove);
            NotifyClipChange();
        }

        /// <summary>
        /// Empty the clip completely.
        /// </summary>
        /// <param name="notify">Notify that the clip changed.</param>
        public override void EmptyClip(bool notify)
        {
            if(m_ClipRemaining.Count == 0){ return; }
            m_ClipRemaining.Clear();
            if (notify) {
                NotifyClipChange();
            }
        }

        /// <summary>
        /// Set the clip remaining manually.
        /// </summary>
        /// <param name="targetClipRemainingCount">The clip remaining count to set.</param>
        public override void SetClipRemaining(int targetClipRemainingCount)
        {
            var currentRemainingCount = ClipRemaining.Count;
            if (currentRemainingCount == targetClipRemainingCount) {
                return;
            }

            if (currentRemainingCount > targetClipRemainingCount) {
                ClipRemaining.RemoveRange(targetClipRemainingCount, currentRemainingCount - targetClipRemainingCount);
                NotifyClipChange();
                return;
            }
            
            // Add more.
            var reloadAmount = targetClipRemainingCount - currentRemainingCount;
            var ammoModule = ShootableAction.MainAmmoModule;
            if (ammoModule == null) {
                for (int i = 0; i < reloadAmount; i++) {
                    ClipRemaining.Add(new ShootableAmmoData(null, i +currentRemainingCount,0,null,null));
                }
            } else {
                ShootableAction.MainAmmoModule.LoadAmmoIntoList(ClipRemaining, reloadAmount, false);
            }
            
            NotifyClipChange();
        }

        /// <summary>
        /// Reload the clip.
        /// </summary>
        /// <param name="fullClip">Reload the clip completely?</param>
        public override void ReloadClip(bool fullClip)
        {
            var mainAmmoModule = ShootableAction.MainAmmoModule;
            if (mainAmmoModule == null) {
                // The weapon can not be reloaded without an ammo module
                return;
            }
            
            var remainingAmmo = mainAmmoModule.GetAmmoRemainingCount();
            int reloadAmount;
            var clipRemainingCount = ClipRemainingCount;

            if (mainAmmoModule.IsAmmoShared()) {
                DetermineTotalReloadAmount();

                if (m_TotalReloadAmount > m_TotalAmmoAmount) {
                    var totalAmount = m_TotalAmmoAmount + m_TotalClipAmount;

                    var activeSharedInstances = GetNumberOfSharedInstances();

                    // If there are multiple active consumable ItemIdentifiers then the reloaded count is shared across all of the ItemIdentifiers.
                    var targetAmount = fullClip
                        ? Mathf.CeilToInt(totalAmount / (float)activeSharedInstances)
                        : clipRemainingCount + 1;
                    reloadAmount = Mathf.Min(remainingAmmo, Mathf.Min(m_ClipSize - clipRemainingCount, targetAmount - clipRemainingCount));
                    
                } else {
                    // The Consumable ItemIdentifier doesn't need to be shared if there is plenty of ammo for all weapons.
                    reloadAmount = Mathf.Min(m_TotalAmmoAmount,
                        (fullClip ? (m_ClipSize - clipRemainingCount) : 1));
                }
            } else {
                // The consumable ItemIdentifier doesn't share with any other objects.
                reloadAmount = Mathf.Min(remainingAmmo,
                    (fullClip ? (m_ClipSize - clipRemainingCount) : 1));
            }

            if (reloadAmount <= 0) {
                // Reloading negative amounts of ammo is not possible
                return;
            }
            
            mainAmmoModule.LoadAmmoIntoList(ClipRemaining, reloadAmount, true);
            NotifyClipChange();
        }

        /// <summary>
        /// Determine the total amount of ammo to reload.
        /// </summary>
        protected virtual void DetermineTotalReloadAmount()
        {
            var mainAmmoModule = ShootableAction.MainAmmoModule;
            m_TotalReloadAmount = m_ClipSize - ClipRemainingCount;
            m_TotalClipAmount = ClipRemainingCount;
            if (mainAmmoModule?.IsAmmoShared() ?? false) {
                for (int i = 0; i < Inventory.SlotCount; ++i) {
                    var item = Inventory.GetActiveCharacterItem(i);
                    if (item != null) {
                        var shootableWeapons = item.gameObject.GetCachedComponents<ShootableAction>();
                        for (int j = 0; j < shootableWeapons.Length; ++j) {
                            if (shootableWeapons[j] == ShootableAction) { continue; }
                            
                            // Ignore if it was already reloaded.
                            if(shootableWeapons[j].HasReloaded()){ continue; }

                            m_TotalReloadAmount +=
                                shootableWeapons[j].ClipSize - shootableWeapons[j].ClipRemainingCount;
                            m_TotalClipAmount += shootableWeapons[j].ClipRemainingCount;
                        }
                    }
                }
            }

            m_TotalAmmoAmount = mainAmmoModule?.GetAmmoRemainingCount() ?? 0;
        }
        
        /// <summary>
        /// Get the number of items or modules sharing the ammo.
        /// </summary>
        /// <returns>The number of items or modules sharing the ammo.</returns>
        public int GetNumberOfSharedInstances()
        {
            var ammoModule = ShootableAction.MainAmmoModule;
            if (ammoModule == null) { return 1; }
            var isAmmoShared = ammoModule.IsAmmoShared();
            if (isAmmoShared == false) { return 1; }
            
            var inventory = Inventory;
            var sharedCount = 0;

            // Find other modules/item that use the same ammo.
            for (int i = 0; i < inventory.SlotCount; ++i) {
                var item = inventory.GetActiveCharacterItem(i);
                if (item == null) { 
                    continue;
                }

                // Find any ShootableWeapons that may be sharing the same Consumable ItemIdentifier.
                var itemActions = item.ItemActions;
                for (int j = 0; j < itemActions.Length; ++j) {
                    var otherShootableAction = itemActions[j] as ShootableAction;
                    if (otherShootableAction == null || otherShootableAction.IsInitialized == false || ammoModule.DoesAmmoSharedMatch(otherShootableAction.MainAmmoModule) == false) {
                        continue;
                    }

                    // The Consumable ItemIdentifier doesn't need to be shared if there is plenty of ammo for all weapons.
                    var totalInventoryAmount = ammoModule.GetAmmoRemainingCount();
                    if (otherShootableAction.ClipSize + ShootableAction.ClipSize < totalInventoryAmount) {
                        continue;
                    }

                    sharedCount++;
                }
            }

            return sharedCount;
        }
    }
}