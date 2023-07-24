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
    using UnityEngine;

    /// <summary>
    /// The Individual Item Set Rule allows you to create one item set per item slot.
    /// </summary>
    [CreateAssetMenu(menuName = "Opsive/Ultimate Character Controller/Inventory/Individual Item Set Rule", fileName = "MyIndividualItemSetRule", order = 23)]
    [Serializable]
    public class IndividualItemSetRule : ItemSetRule
    {
        [Tooltip("The Item Definitions that should not occupy the item slots.")]
        [ReorderableObjectList] [SerializeField] protected ItemType[] m_ItemTypeExceptions;
        [Tooltip("The Item Definitions that should not occupy the item slots.")]
        [ReorderableObjectList] [SerializeField] protected CategoryBase[] m_ItemCategoryExceptions;
        
        [Shared.Utility.NonSerialized] public ItemType[] ItemTypeExceptions { get { return m_ItemTypeExceptions; } set { m_ItemTypeExceptions = value; } }
        [Shared.Utility.NonSerialized] public CategoryBase[] ItemCategoryExceptions { get { return m_ItemCategoryExceptions; } set { m_ItemCategoryExceptions = value; } }
        
        /// <summary>
        /// Returns if an item set is valid for the allowed slots mask.
        /// </summary>
        /// <param name="itemSet">The item set to check.</param>
        /// <param name="allowedSlotsMask">The allowed slots mask.</param>
        /// <returns>Returns true if the item set is valid.</returns>
        public override bool IsItemSetValid(ItemSet itemSet, int allowedSlotsMask)
        {
            return true;
        }

        /// <summary>
        /// Can the slot be empty for this rule.
        /// </summary>
        /// <param name="slotID">The slot ID to check.</param>
        /// <returns>True if it can be empty.</returns>
        protected override bool CanSlotBeNull(int slotID)
        {
            return true;
        }

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

            //Only allow item sets with a single item per set.
            for (int i = 0; i < currentPermutation.Count; i++) {
                if (currentPermutation[i] != null) {
                    return false;
                }
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
    }
}