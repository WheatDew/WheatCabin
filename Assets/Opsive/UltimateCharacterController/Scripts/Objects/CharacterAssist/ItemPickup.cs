/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;

    /// <summary>
    /// Extends ItemPickupBase to allow for ItemIdentifier pickups.
    /// </summary>
    public class ItemPickup : ItemPickupBase
    {
        [Tooltip("Should the item be equipped on pickup?")]
        [SerializeField] protected bool m_Equip = true;
        [Tooltip("The Item Set Group to equip on pickup.")]
        [SerializeField] protected int m_ItemSetGroup = -1;
        [Tooltip("If an item set exist with the matching state name, that one will be equipped.")]
        [StateName] [SerializeField] protected string m_ItemSetName;
        [Tooltip("An array of ItemIdentifiers to be picked up.")]
        [SerializeField] protected ItemIdentifierAmount[] m_ItemDefinitionAmounts;
        
        /// <summary>
        /// Returns the ItemDefinitionAmount that the ItemPickup contains.
        /// </summary>
        /// <returns>The ItemDefinitionAmount that the ItemPickup contains.</returns>
        public override ItemIdentifierAmount[] GetItemDefinitionAmounts()
        {
            return m_ItemDefinitionAmounts;
        }

        /// <summary>
        /// Sets the ItemPickup ItemDefinitionAmounts value.
        /// </summary>
        /// <param name="itemDefinitionAmounts">The ItemDefinitionAmount that should be set.</param>
        public override void SetItemDefinitionAmounts(ItemIdentifierAmount[] itemDefinitionAmounts)
        {
            m_ItemDefinitionAmounts = itemDefinitionAmounts;
        }

        /// <summary>
        /// Internal method which picks up the ItemIdentifier.
        /// </summary>
        /// <param name="character">The character that should pick up the ItemIdentifier.</param>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <returns>True if an ItemIdentifier was picked up.</returns>
        protected override bool DoItemIdentifierPickupInternal(GameObject character, InventoryBase inventory, int slotID, bool immediatePickup, bool forceEquip)
        {
            // Add the ItemIdentifiers to the Inventory. This allows the character to pick up the actual item and any consumable ItemIdentifier (such as ammo).
            if (m_ItemDefinitionAmounts == null) {
                return false;
            }

            var pickedUp = false;
            for (int i = 0; i < m_ItemDefinitionAmounts.Length; ++i) {
                var itemIdentifier = m_ItemDefinitionAmounts[i].ItemIdentifier;
                if (itemIdentifier == null) {
#if UNITY_EDITOR
                    Debug.LogWarning("Empty items cannot be picked up.", gameObject);
#endif
                    continue;
                    
                }
                
                if (inventory.PickupItem(itemIdentifier, slotID, m_ItemDefinitionAmounts[i].Amount, immediatePickup, forceEquip) >= 0) {
                    pickedUp = true;
                }
            }

            // If the item was picked up the pickup events should equip/unequip the item.
            if (m_Equip) {
                // The item can't be equipped if the use or reload abilities are active.
                var characterLocomotion = inventory.gameObject.GetCachedComponent<Character.UltimateCharacterLocomotion>();
                if (characterLocomotion.IsAbilityTypeActive<Character.Abilities.Items.Use>() || characterLocomotion.IsAbilityTypeActive<Character.Abilities.Items.Reload>()) {
                    return pickedUp;
                }

                var itemSetManager = inventory.gameObject.GetCachedComponent<ItemSetManagerBase>();
                itemSetManager.UpdateItemSets();

                if (string.IsNullOrWhiteSpace(m_ItemSetName) == false) {
                    if (itemSetManager.TryEquipItemSet(m_ItemSetName, m_ItemSetGroup, forceEquip, false)) {
                        return true;
                    } else {
                        Debug.LogWarning($"Cannot equip item set '{m_ItemSetName}' it might be invalid, disabled or might not exist.");
                    }
                }
                
                for (int i = 0; i < m_ItemDefinitionAmounts.Length; i++) {
                    var itemIdentifierAmount = m_ItemDefinitionAmounts[i];
                    for (int j = inventory.SlotCount - 1; j >= 0; j--) {
                        var characterItem = inventory.GetCharacterItem(itemIdentifierAmount.ItemIdentifier, j);
                        if (characterItem == null) { continue; }

                        // Only equip if the item is not already equipped or about to be equipped.
                        // It can sometimes be equipping from the Pickup event.
                        if (itemSetManager.IsItemContainedInActiveItemSet(m_ItemSetGroup, characterItem.ItemIdentifier) ||
                                        itemSetManager.IsItemContainedInNextItemSet(m_ItemSetGroup, characterItem.ItemIdentifier)) {
                            continue;
                        }

                        var itemSet = itemSetManager.GetItemSet(characterItem.ItemIdentifier, m_ItemSetGroup, true);
                        if (itemSet == null) { continue; }

                        itemSetManager.TryEquipItemSet(itemSet, forceEquip, false);
                    }
                }
            }

            return pickedUp;
        }
    }
}