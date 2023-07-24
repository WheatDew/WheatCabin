/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;

    /// <summary>
    /// The ItemMonitor will update the UI for the character's items.
    /// </summary>
    public abstract class ItemMonitor : CharacterMonitor
    {
        protected InventoryBase m_CharacterInventory;
        protected CharacterItem m_MonitoredCharacterItem;

        public CharacterItem MonitoredCharacterItem => m_MonitoredCharacterItem;

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<IItemIdentifier, int, bool, bool>(m_Character, "OnInventoryPickupItemIdentifier", OnPickupItemIdentifier);
                EventHandler.UnregisterEvent<CharacterItem, bool>(m_Character, "OnItemUpdateDominantItem", OnUpdateDominantItem);
                
                EventHandler.UnregisterEvent<IItemIdentifier, int, int>(m_Character, "OnInventoryAdjustItemIdentifierAmount", OnAdjustItemIdentifierAmount);
            }

            base.OnAttachCharacter(character);

            if (m_Character == null) {
                return;
            }
            
            // The character must have an inventory.
            m_CharacterInventory = m_Character.GetCachedComponent<InventoryBase>();
            if (m_CharacterInventory == null) {
                return;
            }
            gameObject.SetActive(CanShowUI());

            EventHandler.RegisterEvent<IItemIdentifier, int, bool, bool>(m_Character, "OnInventoryPickupItemIdentifier", OnPickupItemIdentifier);
            EventHandler.RegisterEvent<CharacterItem, bool>(m_Character, "OnItemUpdateDominantItem", OnUpdateDominantItem);
            
            EventHandler.RegisterEvent<IItemIdentifier, int, int>(m_Character, "OnInventoryAdjustItemIdentifierAmount", OnAdjustItemIdentifierAmount);
        }

        /// <summary>
        /// An ItemIdentifier has been picked up within the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier that has been picked up.</param>
        /// <param name="amount">The amount of item picked up.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        protected virtual void OnPickupItemIdentifier(IItemIdentifier itemIdentifier, int amount, bool immediatePickup, bool forceEquip) { }

        /// <summary>
        /// The DominantItem field has been updated for the specified item.
        /// </summary>
        /// <param name="characterItem">The Item whose DominantItem field was updated.</param>
        /// <param name="dominantItem">True if the item is now a dominant item.</param>
        protected virtual void OnUpdateDominantItem(CharacterItem characterItem, bool dominantItem) { }

        /// <summary>
        /// The specified ItemIdentifier amount has been adjusted.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="previousAmount">The previous amount of ItemIdentifier to adjust.</param>
        /// <param name="newAmount">The new amount of ItemIdentifier to adjust.</param>
        protected virtual void OnAdjustItemIdentifierAmount(IItemIdentifier itemIdentifier, int previousAmount, int newAmount) { }

        /// <summary>
        /// The specified ItemIdentifier amount has been adjusted.
        /// </summary>
        /// <param name="newMonitoredCharacterItem">The new Monitored Character Item.</param>
        protected virtual void OnMonitoredCharacterItemChanged(CharacterItem newMonitoredCharacterItem)
        {
            var previousMonitoredItem = m_MonitoredCharacterItem;
            m_MonitoredCharacterItem = newMonitoredCharacterItem;
            EventHandler.ExecuteEvent<CharacterItem, CharacterItem>(gameObject, 
                "OnMonitoredCharacterItemChanged", previousMonitoredItem,newMonitoredCharacterItem);
        }
    }
}
