/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Inventory;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The ammo data contains information about the each individual ammo.
    /// </summary>
    [Serializable]
    public struct ShootableAmmoData
    {
        private bool m_Valid;
        private ShootableAmmoModule m_AmmoModule;
        private IItemIdentifier m_ItemIdentifier;
        private int m_Index;
        private int m_Value;
        private object m_UserData;
        
        public bool Valid => m_Valid;
        public ShootableAmmoModule AmmoModule => m_AmmoModule;
        public IItemIdentifier ItemIdentifier => m_ItemIdentifier;
        public int Index => m_Index;
        public int Value => m_Value;
        public object UserData => m_UserData;
        public static ShootableAmmoData None => new ShootableAmmoData();

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="ammoModule">The ammo module.</param>
        /// <param name="index">The index of the ammo within a clip.</param>
        /// <param name="value">A value associated to the ammo to differentiate it from others.</param>
        /// <param name="itemIdentifier">The item Identifier associated to the ammo if it has one.</param>
        /// <param name="userData">Use data for custom functionality.</param>
        public ShootableAmmoData(ShootableAmmoModule ammoModule, int index, int value, IItemIdentifier itemIdentifier,  object userData)
        {
            m_Valid = true;
            m_AmmoModule = ammoModule;
            m_ItemIdentifier = itemIdentifier;
            m_Index = index;
            m_Value = value;
            m_UserData = userData;
        }
        
        /// <summary>
        /// Copy this data and change only the index.
        /// </summary>
        /// <param name="index">The new index.</param>
        /// <returns>Returns a copy of the data with the index changed.</returns>
        public ShootableAmmoData CopyWithIndex(int index)
        {
            return new ShootableAmmoData(
                m_AmmoModule,
                index,
                m_Value,
                m_ItemIdentifier,
                m_UserData);
        }
    }
    
    /// <summary>
    /// This module is used to define how the ammo connected to the Inventory.
    /// </summary>
    [Serializable]
    public abstract class ShootableAmmoModule : ShootableActionModule
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;
        public virtual ItemDefinitionBase AmmoItemDefinition { get { return null; } set { } }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);
            
            // Notify the ammo change as the ammo module could have been switched at runtime.
            if (register) {
                NotifyAmmoChange();
            }
        }

        /// <summary>
        /// Is there any ammo left to be used?
        /// </summary>
        /// <returns>True if there is still ammo left.</returns>
        public abstract bool HasAmmoRemaining();

        /// <summary>
        /// Is the ammo shared between multiple items.
        /// </summary>
        /// <returns>True if the ammo is shared with other items.</returns>
        public abstract bool IsAmmoShared();
        
        /// <summary>
        /// Check if the same ammo is shared between the two modules.
        /// </summary>
        /// <param name="otherAmmoModule">The other Ammo Module to compare if the ammo match.</param>
        /// <returns>True if both modules share the same ammo.</returns>
        public abstract bool DoesAmmoSharedMatch(ShootableAmmoModule otherAmmoModule);

        /// <summary>
        /// Returns the remaining ammo count.
        /// </summary>
        /// <returns>The remaining ammo count.</returns>
        public abstract int GetAmmoRemainingCount();

        /// <summary>
        /// Load the ammo within the clip ammo list.
        /// </summary>
        /// <param name="clipRemaining">The clip remaining list of ammo.</param>
        /// <param name="reloadAmount">The number of ammo to reload inside the clip list.</param>
        /// <param name="removeAmmoWhenLoaded">Remove the ammo when it is loaded in the clip list?</param>
        public abstract void LoadAmmoIntoList(List<ShootableAmmoData> clipRemaining, int reloadAmount, bool removeAmmoWhenLoaded);
        
        /// <summary>
        /// Adjust the ammo amount by adding the amount (negative to remove ammo).
        /// </summary>
        /// <param name="amount">The amount to adjust the ammo by.</param>
        public abstract void AdjustAmmoAmount(int amount);

        /// <summary>
        /// Notify, with an event, that the ammo amount has changed.
        /// </summary>
        public void NotifyAmmoChange()
        {
            Shared.Events.EventHandler.ExecuteEvent<CharacterItem, ShootableAmmoModule>(Character, "OnShootableItemAmmoChange", CharacterItem, this);
        }

        /// <summary>
        /// Create an ammo data which can be cached.
        /// </summary>
        /// <returns>The new shootable ammo data to cache.</returns>
        public virtual ShootableAmmoData CreateAmmoData()
        {
            return new ShootableAmmoData(this, -1, -1, null, null);
        }
    }

    /// <summary>
    /// Infinite ammo when a shootable item does not require anything to fire projectiles.
    /// </summary>
    [Serializable]
    public class InfiniteAmmo : ShootableAmmoModule
    {
        /// <summary>
        /// Is there any ammo left to be used?
        /// </summary>
        /// <returns>True if there is still ammo left.</returns>
        public override bool HasAmmoRemaining()
        {
            return true;
        }

        /// <summary>
        /// Is the ammo shared between multiple items.
        /// </summary>
        /// <returns>True if the ammo is shared with other items.</returns>
        public override bool IsAmmoShared()
        {
            return false;
        }

        /// <summary>
        /// Check if the same ammo is shared between the two modules.
        /// </summary>
        /// <param name="otherAmmoModule">The other Ammo Module to compare if the ammo match.</param>
        /// <returns>True if both modules share the same ammo.</returns>
        public override bool DoesAmmoSharedMatch(ShootableAmmoModule otherAmmoModule)
        {
            return false;
        }

        /// <summary>
        /// Get the amount of unloaded ammo remaining.
        /// </summary>
        /// <returns>The amount of unloaded ammo remaining.</returns>
        public override int GetAmmoRemainingCount()
        {
            return int.MaxValue;
        }

        /// <summary>
        /// Load the ammo within the clip ammo list.
        /// </summary>
        /// <param name="clipRemaining">The clip remaining list of ammo.</param>
        /// <param name="reloadAmount">The number of ammo to reload inside the clip list.</param>
        /// <param name="removeAmmoWhenLoaded">Remove the ammo when it is loaded in the clip list?</param>
        public override void LoadAmmoIntoList(List<ShootableAmmoData> clipRemaining, int reloadAmount, bool removeAmmoWhenLoaded)
        {
            for (int i = 0; i < reloadAmount; i++) {
                var ammoData = CreateAmmoData();
                clipRemaining.Add(ammoData);
            }
        }

        /// <summary>
        /// Adjust the ammo amount by adding the amount (negative to remove ammo).
        /// </summary>
        /// <param name="amount">The amount to adjust the ammo by.</param>
        public override void AdjustAmmoAmount(int amount)
        {
            // Do nothing.
        }
    }

    /// <summary>
    /// Uses Items inside the Inventory to get ammo for the shootable action.
    /// </summary>
    [Serializable]
    public class ItemAmmo : ShootableAmmoModule, IModuleGetItemsToDrop, IModuleItemDefinitionConsumer
    {
        /// <summary>
        /// Specifies the quantity of ammo that gets dropped when the character item is dropped.
        /// </summary>
        public enum DropOptions
        {
            Nothing,    // No ammo is dropped.
            Clip,       // The ammo that is within the clip.
            AmmoLeft,   // The ammo that hasn't been loaded yet.
            All         // The ammo that is within the clip and the ammo that hasn't been loaded yet.
        }
        
        [Tooltip("The ItemDefinition that is consumed by the item.")]
        [SerializeField] protected ItemDefinitionBase m_AmmoItemDefinition;
        [Tooltip("Choose how what quantity of ammo item gets dropped when the character item is dropped.")]
        [SerializeField] protected DropOptions m_DropOption = DropOptions.AmmoLeft; 
        [Tooltip("Is the ammo shared with other items?")]
        [SerializeField] protected bool m_SharedAmmoItemIdentifier = false; 

        public override ItemDefinitionBase AmmoItemDefinition { get { return m_AmmoItemDefinition; } set { m_AmmoItemDefinition = value; } }
        public DropOptions DropOption { get { return m_DropOption; } set { m_DropOption = value; } }

        protected IItemIdentifier m_AmmoItemIdentifier;

        public IItemIdentifier AmmoItemIdentifier { get { return m_AmmoItemIdentifier; } }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            if (m_AmmoItemDefinition != null) {
                m_AmmoItemIdentifier = m_AmmoItemDefinition.CreateItemIdentifier();
            }
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);
            Shared.Events.EventHandler.RegisterUnregisterEvent<IItemIdentifier, int, int>(register,
                Character, "OnInventoryAdjustItemIdentifierAmount", OnAdjustItemIdentifierAmount);
        }

        /// <summary>
        /// The specified ItemIdentifier amount has been adjusted.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="previousAmount">The previous amount of ItemIdentifier to adjust.</param>
        /// <param name="newAmount">The new amount of ItemIdentifier to adjust.</param>
        protected virtual void OnAdjustItemIdentifierAmount(IItemIdentifier itemIdentifier, int previousAmount, int newAmount)
        {
            NotifyAmmoChange();
        }

        /// <summary>
        /// Set the ItemIdentifier which can be consumed by the item.
        /// </summary>
        /// <param name="itemIdentifier">The new ItemIdentifier which can be consumed by the item.</param>
        public virtual void SetConsumableItemIdentifier(IItemIdentifier itemIdentifier)
        {
            if (m_AmmoItemIdentifier == itemIdentifier) { return; }

            var previousAmmoIdentifier = m_AmmoItemIdentifier;
            m_AmmoItemIdentifier = itemIdentifier;
            m_AmmoItemDefinition = itemIdentifier.GetItemDefinition();
            
            // Add back the previous consumable item to the inventory.
            Inventory.AdjustItemIdentifierAmount(previousAmmoIdentifier, ShootableAction.ClipRemainingCount);
            // Set the ClipRemaining to 0 so the new consumable item can be loaded from the inventory.
            ShootableAction.MainClipModule.EmptyClip(false);

            NotifyAmmoChange();

            ShootableAction.ReloadItem(false);
        }

        /// <summary>
        /// Is there any ammo left to be used?
        /// </summary>
        /// <returns>True if there is still ammo left.</returns>
        public override bool HasAmmoRemaining()
        {
            if (Inventory.GetItemIdentifierAmount(m_AmmoItemIdentifier, true) == 0) { return false; }

            return true;
        }

        /// <summary>
        /// Is the ammo shared between multiple items.
        /// </summary>
        /// <returns>True if the ammo is shared with other items.</returns>
        public override bool IsAmmoShared()
        {
            return m_SharedAmmoItemIdentifier;
        }
        
        /// <summary>
        /// Check if the same ammo is shared between the two modules.
        /// </summary>
        /// <param name="otherAmmoModule">The other Ammo Module to compare if the ammo match.</param>
        /// <returns>True if both modules share the same ammo.</returns>
        public override bool DoesAmmoSharedMatch(ShootableAmmoModule otherAmmoModule)
        {
            if (!m_SharedAmmoItemIdentifier || otherAmmoModule == null || !otherAmmoModule.IsAmmoShared()) {
                return false;
            }

            if (!(otherAmmoModule is ItemAmmo itemAmmo)) {
                return false;
            }

            return m_AmmoItemDefinition == itemAmmo.AmmoItemDefinition;
        }

        /// <summary>
        /// Get the number of unloaded ammo left.
        /// </summary>
        /// <returns></returns>
        public override int GetAmmoRemainingCount()
        {
            return Inventory.GetItemIdentifierAmount(m_AmmoItemIdentifier, true);
        }

        /// <summary>
        /// Create an ammo data which can be cached.
        /// </summary>
        /// <returns>The new shootable ammo data to cache.</returns>
        public override ShootableAmmoData CreateAmmoData()
        {
            return new ShootableAmmoData(this, -1, -1, m_AmmoItemIdentifier, null);
        }

        /// <summary>
        /// Load the ammo within the clip ammo list.
        /// </summary>
        /// <param name="clipRemaining">The clip remaining list of ammo.</param>
        /// <param name="reloadAmount">The number of ammo to reload inside the clip list.</param>
        /// <param name="removeAmmoWhenLoaded">Remove the ammo when it is loaded in the clip list?</param>
        public override void LoadAmmoIntoList(List<ShootableAmmoData> clipRemaining, int reloadAmount, bool removeAmmoWhenLoaded)
        {
            if (reloadAmount <= 0) { return; }

            for (int i = 0; i < reloadAmount; i++) {
                var ammoData = CreateAmmoData();
                clipRemaining.Add(ammoData);
            }

            if (removeAmmoWhenLoaded) {
                Inventory.AdjustItemIdentifierAmount(m_AmmoItemIdentifier, -reloadAmount);
                NotifyAmmoChange();
            }
        }

        /// <summary>
        /// Adjust the ammo amount by adding the amount (negative to remove ammo).
        /// </summary>
        /// <param name="amount">The amount to adjust the ammo by.</param>
        public override void AdjustAmmoAmount(int amount)
        {
            Inventory.AdjustItemIdentifierAmount(m_AmmoItemIdentifier, amount);
            NotifyAmmoChange();
        }

        /// <summary>
        /// Get the items to drop by adding it to the list.
        /// </summary>
        /// <param name="itemsToDrop">The list of items to drop, the item to drop will be added to this list.</param>
        public void GetItemsToDrop(List<ItemIdentifierAmount> itemsToDrop)
        {
            var amount = -1;
            switch (m_DropOption) {
                case DropOptions.Nothing:
                    return;
                case DropOptions.All:
                    amount = GetAmmoRemainingCount() + ShootableAction.ClipRemainingCount;
                    break;
                case DropOptions.Clip:
                    amount = ShootableAction.ClipRemainingCount;
                    break;
                case DropOptions.AmmoLeft:
                    amount = GetAmmoRemainingCount();
                    break;
            }

            if (amount == -1) {
                return;
            }
            
            itemsToDrop.Add(new ItemIdentifierAmount(m_AmmoItemDefinition, amount));
        }

        // IModuleItemDefinitionConsumer implementation:
        public ItemDefinitionBase ItemDefinition { get => m_AmmoItemDefinition; set => m_AmmoItemDefinition = value; }
        public int GetItemDefinitionRemainingCount() { return GetAmmoRemainingCount(); }
        public void SetItemDefinitionRemainingCount(int count) { var diff = count - GetAmmoRemainingCount(); AdjustAmmoAmount(diff); }
    }
}