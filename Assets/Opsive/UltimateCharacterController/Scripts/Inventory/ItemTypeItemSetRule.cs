/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The Item Type Item Set Rule define a rules which creates item sets for matching item type in each slot.
    /// </summary>
    [CreateAssetMenu(menuName = "Opsive/Ultimate Character Controller/Inventory/Item Type Item Set Rule", fileName = "MyItemTypeItemSetRule", order = 20)]
    [Serializable]
    public class ItemTypeItemSetRule : ItemSetRule
    {
        [Tooltip("If true, checks that the number of items in the Inventory matches exactly the number of slots used by said item.")]
        [SerializeField] protected bool m_ExactAmountValidation = false;
        [Tooltip("The Item Definitions that occupy the inventory slots.")]
        [ReorderableObjectList] [SerializeField] protected ItemType[] m_ItemTypeSlots;

        protected Dictionary<IItemIdentifier, int> m_CachedItemAmount;
        
        [Shared.Utility.NonSerialized] public ItemType[] ItemTypeSlots { get { return m_ItemTypeSlots; } set { m_ItemTypeSlots = value; } }


        /// <summary>
        /// Does the character item match this rule.
        /// </summary>
        /// <param name="itemSetRuleStreamData">The item set rule stream data.</param>
        /// <param name="currentPermutation">The current item permutation so far.</param>
        /// <param name="characterItem">The character item to check.</param>
        /// <returns>True if the character item matches this rule.</returns>
        public override bool DoesCharacterItemMatchRule(ItemSetRuleStreamData itemSetRuleStreamData,
            ListSlice<IItemIdentifier> currentPermutation, CharacterItem characterItem)
        {
            var item = characterItem.ItemIdentifier;
            var slotID = characterItem.SlotID;
            
            if (slotID < 0 || slotID >= m_ItemTypeSlots.Length) {
                return false;
            }
            if (m_ItemTypeSlots[slotID] == null) {
                return false;
            }
            if (m_ItemTypeSlots[slotID] != null && m_ItemTypeSlots[slotID].InherentlyContains(item) == false) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Can the slot be empty for this rule.
        /// </summary>
        /// <param name="slotID">The slot ID to check.</param>
        /// <returns>True if it can be empty.</returns>
        protected override bool CanSlotBeNull(int slotID)
        {
            if (slotID < 0 || slotID >= m_ItemTypeSlots.Length) {
                return true;
            }

            return m_ItemTypeSlots[slotID] == null;
        }

        /// <summary>
        /// Returns if an item set is valid for the allowed slots mask.
        /// </summary>
        /// <param name="itemSet">The item set to check.</param>
        /// <param name="allowedSlotsMask">The allowed slots mask.</param>
        /// <returns>Returns true if the item set is valid.</returns>
        public override bool IsItemSetValid(ItemSet itemSet, int allowedSlotsMask)
        {
            if (!m_ExactAmountValidation) {
                return true;
            }

            if (m_CachedItemAmount == null) {
                m_CachedItemAmount = new Dictionary<IItemIdentifier, int>();
            } else {
                m_CachedItemAmount.Clear();
            }

            var characterInventory = itemSet.ItemSetGroup.ItemSetManager.CharacterInventory;
                
            for (int i = 0; i < itemSet.ItemIdentifiers.Length; i++) {
                var item = itemSet.ItemIdentifiers[i];
                if(item == null){ continue; }

                if (m_CachedItemAmount.TryGetValue(item, out var value)) {
                    m_CachedItemAmount[item] = value + 1;
                } else {
                    m_CachedItemAmount[item] = 1;
                }

                var count = characterInventory.GetItemIdentifierAmount(item);
            }

            foreach (var itemAmount in m_CachedItemAmount) {
                if (characterInventory.GetItemIdentifierAmount(itemAmount.Key) != itemAmount.Value) {
                    return false;
                }
            }

            return true;
        }
    }
}