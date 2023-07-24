/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using System;

    /// <summary>
    /// The ammo data contains information about the each individual ammo.
    /// </summary>
    [Serializable]
    public struct ThrowableAmmoData
    {
        private bool m_Valid;
        private ThrowableAmmoModule m_AmmoModule;
        private IItemIdentifier m_ItemIdentifier;
        private int m_Index;
        private int m_Value;
        private object m_UserData;
        
        public bool Valid => m_Valid;
        public ThrowableAmmoModule AmmoModule => m_AmmoModule;
        public IItemIdentifier ItemIdentifier => m_ItemIdentifier;
        public int Index => m_Index;
        public int Value => m_Value;
        public object UserData => m_UserData;
        public static ThrowableAmmoData None => new ThrowableAmmoData();

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="ammoModule">The ammo module.</param>
        /// <param name="index">The index of the ammo within a clip.</param>
        /// <param name="value">A value associated to the ammo to differentiate it from others.</param>
        /// <param name="itemIdentifier">The item Identifier associated to the ammo if it has one.</param>
        /// <param name="userData">Use data for custom functionality.</param>
        public ThrowableAmmoData(ThrowableAmmoModule ammoModule, int index, int value, IItemIdentifier itemIdentifier,  object userData)
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
        public ThrowableAmmoData CopyWithIndex(int index)
        {
            return new ThrowableAmmoData(
                m_AmmoModule,
                index,
                m_Value,
                m_ItemIdentifier,
                m_UserData);
        }
    }
    
    /// <summary>
    /// The base class for the ammo module of throwable actions.
    /// </summary>
    [Serializable]
    public abstract class ThrowableAmmoModule : ThrowableActionModule
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;
        
        /// <summary>
        /// Are there still ammo left.
        /// </summary>
        /// <returns>True if there are still available ammo.</returns>
        public virtual bool HasAmmoRemaining()
        {
            return GetAmmoRemainingCount() > 0;
        }
        
        /// <summary>
        /// Is the ammo shared with other items.
        /// </summary>
        /// <returns>True if the ammo is shared.</returns>
        public abstract bool IsAmmoShared();
        
        /// <summary>
        /// Get the number of ammo left.
        /// </summary>
        /// <returns>The ammo quantity left.</returns>
        public abstract int GetAmmoRemainingCount();
        
        /// <summary>
        /// Get the next available ammo.
        /// </summary>
        /// <returns>The next available ammo.</returns>
        public abstract ThrowableAmmoData GetNextAmmoData();
        
        /// <summary>
        /// Load the next available ammo.
        /// </summary>
        /// <returns>Load the next available ammo.</returns>
        public abstract ThrowableAmmoData LoadNextAmmoData();
        
        /// <summary>
        /// Adjust the ammo amount by adding the amount (negative to remove ammo).
        /// </summary>
        /// <param name="amount">The amount to adjust the ammo by.</param>
        public abstract void AdjustAmmoAmount(int amount);

        /// <summary>
        /// Notify a change in the ammo type or quantity.
        /// </summary>
        public void NotifyAmmoChange()
        {
            Shared.Events.EventHandler.ExecuteEvent<CharacterItem, ThrowableAmmoModule>(Character, "OnThrowableItemAmmoChange", CharacterItem, this);
        }
        
        /// <summary>
        /// Create a new ammo data.
        /// </summary>
        /// <returns>The new ammo data.</returns>
        public virtual ThrowableAmmoData CreateAmmoData()
        {
            var ammoData = new ThrowableAmmoData(
                this,
                -1,
                -1,
                null,
                null);
            return ammoData;
        }
    }
    
    /// <summary>
    /// Use items from the Inventory as ammo to throw.
    /// </summary>
    [Serializable]
    public class ItemAmmo : ThrowableAmmoModule, IModuleCanStartUseItem
    {
        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register) {
            base.UpdateRegisteredEventsInternal(register);
            Shared.Events.EventHandler.RegisterUnregisterEvent<IItemIdentifier, int, int>(register, Character, "OnInventoryAdjustItemIdentifierAmount",
                OnAdjustItemIdentifierAmount);
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
        /// Is the ammo shared with other items.
        /// </summary>
        /// <returns>True if the ammo is shared.</returns>
        public override bool IsAmmoShared()
        {
            return false;
        }

        /// <summary>
        /// Get the number of ammo left.
        /// </summary>
        /// <returns>The ammo quantity that is left.</returns>
        public override int GetAmmoRemainingCount()
        {
            return Inventory.GetItemIdentifierAmount(CharacterItem.ItemIdentifier);
        }

        /// <summary>
        /// Create a new ammo data.
        /// </summary>
        /// <returns>The new ammo data.</returns>
        public override ThrowableAmmoData CreateAmmoData()
        {
            return new ThrowableAmmoData(
                this,
                -1,
                -1,
                CharacterItem.ItemIdentifier,
                null);
        }

        /// <summary>
        /// Get the next available ammo.
        /// </summary>
        /// <returns>The next available ammo.</returns>
        public override ThrowableAmmoData GetNextAmmoData()
        {
            var itemIdentifierAmount = Inventory.GetItemIdentifierAmount(CharacterItem.ItemIdentifier);
            if (itemIdentifierAmount > 0) {
                return CreateAmmoData();
            }

            // If there is no item amount left retun an invalid ammo data.
            return new ThrowableAmmoData();
        }

        /// <summary>
        /// Load the next available ammo.
        /// </summary>
        /// <returns>Load the next available ammo.</returns>
        public override ThrowableAmmoData LoadNextAmmoData()
        {
            var ammoData = CreateAmmoData();
            Inventory.AdjustItemIdentifierAmount(CharacterItem.ItemIdentifier, -1);
            NotifyAmmoChange();
            
            return ammoData;
        }

        /// <summary>
        /// Adjust the ammo amount by adding the amount (negative to remove ammo).
        /// </summary>
        /// <param name="amount">The amount to adjust the ammo by.</param>
        public override void AdjustAmmoAmount(int amount)
        {
            Inventory.AdjustItemIdentifierAmount(CharacterItem.ItemIdentifier, amount);
            NotifyAmmoChange();
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            // The item can't be used if there aren't any items left.
            if (!HasAmmoRemaining()) {
                return false;
            }

            return true;
        }
    }
}