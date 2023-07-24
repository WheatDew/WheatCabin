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
    /// The Item Category Item Set Rule allow you to create item sets for any item matching the item category and the slot.
    /// </summary>
    [CreateAssetMenu(menuName = "Opsive/Ultimate Character Controller/Inventory/Category Item Set Rule", fileName = "MyCategoryItemSetRule", order = 21)]
    [Serializable]
    public class CategoryItemSetRule : ItemSetRule
    {
        [Tooltip("If set to true, check that the number of Items in the Inventory exactly match the number of slots used by that item.")]
        [SerializeField] protected bool m_ExactAmountValidation = false;
        [Tooltip("The Item Categories that occupy the item slots.")]
        [ReorderableObjectList] [SerializeField] protected CategoryBase[] m_ItemCategorySlots;
        [Tooltip("The Item Definitions that should not occupy the item slots.")]
        [ReorderableObjectList] [SerializeField] protected ItemType[] m_ItemTypeExceptions;
        [Tooltip("The Item Definitions that should not occupy the item slots.")]
        [ReorderableObjectList] [SerializeField] protected CategoryBase[] m_ItemCategoryExceptions;

        protected Dictionary<IItemIdentifier, int> m_ItemAmountByItemIdentifier;

        [Shared.Utility.NonSerialized] public CategoryBase[] ItemCategorySlots { get { return m_ItemCategorySlots; } set { m_ItemCategorySlots = value; } }
        [Shared.Utility.NonSerialized] public ItemType[] ItemTypeExceptions { get { return m_ItemTypeExceptions; } set { m_ItemTypeExceptions = value; } }
        [Shared.Utility.NonSerialized] public CategoryBase[] ItemCategoryExceptions { get { return m_ItemCategoryExceptions; } set { m_ItemCategoryExceptions = value; } }

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

            if (slotID < 0 || slotID >= m_ItemCategorySlots.Length) {
                return false;
            }
            if (m_ItemCategorySlots[slotID] == null) {
                return false;
            }
            if (item.InherentlyContainedByCategory(m_ItemCategorySlots[slotID].ID) == false) {
                return false;
            }

            if (m_ItemTypeExceptions != null) {
                for (int i = 0; i < m_ItemTypeExceptions.Length; i++) {
                    if (m_ItemTypeExceptions[i].InherentlyContains(item)) {
                        return false;
                    }
                }
            }

            if (m_ItemCategoryExceptions != null) {
                for (int i = 0; i < m_ItemCategoryExceptions.Length; i++) {
                    if (item.InherentlyContainedByCategory(m_ItemCategoryExceptions[i].ID)) {
                        return false;
                    }
                }
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
            if (slotID < 0 || slotID >= m_ItemCategorySlots.Length) {
                return true;
            }

            return m_ItemCategorySlots[slotID] == null;
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

            if (m_ItemAmountByItemIdentifier == null) {
                m_ItemAmountByItemIdentifier = new Dictionary<IItemIdentifier, int>();
            } else {
                m_ItemAmountByItemIdentifier.Clear();
            }

            var characterInventory = itemSet.ItemSetGroup.ItemSetManager.CharacterInventory;
            for (int i = 0; i < itemSet.ItemIdentifiers.Length; i++) {
                var item = itemSet.ItemIdentifiers[i];
                if (item == null) { continue; }

                if (m_ItemAmountByItemIdentifier.TryGetValue(item, out var value)) {
                    m_ItemAmountByItemIdentifier[item] = value + 1;
                } else {
                    m_ItemAmountByItemIdentifier[item] = 1;
                }
            }

            foreach (var itemAmount in m_ItemAmountByItemIdentifier) {
                if (characterInventory.GetItemIdentifierAmount(itemAmount.Key) != itemAmount.Value) {
                    return false;
                }
            }

            return true;
        }
    }
}