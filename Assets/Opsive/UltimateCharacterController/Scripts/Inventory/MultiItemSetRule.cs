/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The Multi Item Set Rule allows you to group multiple Item Set Rules under a single object.
    /// </summary>
    [CreateAssetMenu(menuName = "Opsive/Ultimate Character Controller/Inventory/Multi Item Set Rule", fileName = "MyMultiItemSetRule", order = 22)]
    [Serializable]
    public class MultiItemSetRule : ItemSetRuleBase
    {
        [Tooltip("All the Item Set Rules to group. One one needs to pass to allow the item set to be created.")]
        [SerializeField] private ItemSetRuleBase[] m_ItemSetRules;

        private List<ItemSetStateInfo> m_CachedItemSetStateInfos;

        /// <summary>
        /// From the Item Set Rule Stream Data return the next item set state info.
        /// </summary>
        /// <param name="itemSetRuleStreamData">The item set rule stream data.</param>
        /// <returns>Return the item set state info.</returns>
        public override ListSlice<ItemSetStateInfo> GetNextItemSetsStateInfo(ItemSetRuleStreamData itemSetRuleStreamData)
        {
            if (m_CachedItemSetStateInfos == null) {
                m_CachedItemSetStateInfos = new List<ItemSetStateInfo>();
            } else {
                m_CachedItemSetStateInfos.Clear();
            }
            
            for (int i = 0; i < m_ItemSetRules.Length; i++) {
                var result = m_ItemSetRules[i].GetNextItemSetsStateInfo(itemSetRuleStreamData);
                m_CachedItemSetStateInfos.AddRange(result);
            }

            return m_CachedItemSetStateInfos;
        }

        /// <summary>
        /// Returns if an item set is valid for the allowed slots mask.
        /// </summary>
        /// <param name="itemSet">The item set to check.</param>
        /// <param name="allowedSlotsMask">The allowed slots mask.</param>
        /// <returns>Returns true if the item set is valid.</returns>
        public override bool IsItemSetValid(ItemSet itemSet, int allowedSlotsMask)
        {
            //If just one is valid return true.
            for (int i = 0; i < m_ItemSetRules.Length; i++) {
                var result = m_ItemSetRules[i].IsItemSetValid(itemSet,allowedSlotsMask);
                if (result) {
                    return true;
                }
            }

            return false;
        }
    }
}