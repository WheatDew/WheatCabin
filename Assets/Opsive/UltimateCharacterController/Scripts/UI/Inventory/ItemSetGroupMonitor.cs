/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI.Inventory
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// This monitor will show the state of an Item Set Group within the Item Set Manager.
    /// </summary>
    public class ItemSetGroupMonitor : CharacterMonitor
    {
        [Tooltip("The Index of the Item Set Group to monitor.")]
        [SerializeField] protected int m_GroupIndex;
        [Tooltip("The Content in which to add the instantiated Item Set Views.")]
        [SerializeField] protected RectTransform m_Content;
        [Tooltip("The ITem Set View prefab to be instantiated for each ItemSet.")]
        [SerializeField] protected ItemSetView m_ItemSetViewPrefab;

        protected InventoryBase m_Inventory;
        protected ItemSetManager m_ItemSetManager;

        protected List<ItemSetView> m_ItemSetViews = new List<ItemSetView>();

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            base.OnAttachCharacter(character);

            if (m_Character != character) {
                Shared.Events.EventHandler.UnregisterEvent<ItemSetGroup, List<ItemSetStateInfo>>(character, "OnItemSetGroupWillUpdate", HandleItemSetsUpdated);
                Shared.Events.EventHandler.UnregisterEvent<ItemSetGroup>(character, "OnItemSetGroupUpdated", HandleItemSetsUpdated);
                Shared.Events.EventHandler.UnregisterEvent<int, int>(character, "OnItemSetManagerUpdateItemSet", HandleItemSetsUpdated);
            }

            if (character == null) {
                return;
            }

            m_Inventory = character.GetCachedComponent<InventoryBase>();
            m_ItemSetManager = character.GetCachedComponent<ItemSetManager>();

            Shared.Events.EventHandler.RegisterEvent<ItemSetGroup, List<ItemSetStateInfo>>(character, "OnItemSetGroupWillUpdate", HandleItemSetsUpdated);
            Shared.Events.EventHandler.RegisterEvent<ItemSetGroup>(character, "OnItemSetGroupUpdated", HandleItemSetsUpdated);
            Shared.Events.EventHandler.RegisterEvent<int, int>(character, "OnItemSetManagerUpdateItemSet", HandleItemSetsUpdated);

            Draw();
        }

        /// <summary>
        /// Draw on event.
        /// </summary>
        /// <param name="itemSetGroup">The item set group.</param>
        /// <param name="newItemSetStateInfo">The new item set state info.</param>
        private void HandleItemSetsUpdated(ItemSetGroup itemSetGroup, List<ItemSetStateInfo> newItemSetStateInfo)
        {
            if (itemSetGroup.GroupIndex != m_GroupIndex) { return; }
            Draw();
        }

        /// <summary>
        /// Draw on event.
        /// </summary>
        /// <param name="itemSetGroup">The item set group.</param>
        private void HandleItemSetsUpdated(ItemSetGroup itemSetGroup)
        {
            if (itemSetGroup.GroupIndex != m_GroupIndex) { return; }
            Draw();
        }

        /// <summary>
        /// Draw on event.
        /// </summary>
        /// <param name="groupIndex">The item set group index.</param>
        /// <param name="itemSetIndex">The item set index.</param>
        private void HandleItemSetsUpdated(int groupIndex, int itemSetIndex)
        {
            if (groupIndex != m_GroupIndex) { return; }
            Draw();
        }

        /// <summary>
        /// Draw the item sets within the monitored group.
        /// </summary>
        private void Draw()
        {
            var itemSetGroup = m_ItemSetManager.ItemSetGroups[m_GroupIndex];

            var itemSets = itemSetGroup.ItemSetList;
            var itemSetsCount = itemSets.Count;
            for (int i = 0; i < itemSetsCount; i++) {
                var itemSet = itemSets[i];
                if (m_ItemSetViews.Count <= i) {
                    var instance = ObjectPool.Instantiate(m_ItemSetViewPrefab.gameObject, m_Content);

                    m_ItemSetViews.Add(instance.GetComponent<ItemSetView>());
                }

                m_ItemSetViews[i].SetItemSet(itemSet);
            }

            // Return unused views to the pool.
            if (itemSetsCount < m_ItemSetViews.Count) {
                for (int i = m_ItemSetViews.Count - 1; i >= itemSetsCount; i--) {
                    ObjectPool.Destroy(m_ItemSetViews[i].gameObject);
                    m_ItemSetViews.RemoveAt(i);
                }
            }

            // Resize the content since it is most likely part of a Scroll View.
            m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x, itemSetsCount == 0 ? 0 :
                m_ItemSetViews[0].gameObject.GetCachedComponent<RectTransform>().sizeDelta.y * itemSetsCount);
        }
    }
}