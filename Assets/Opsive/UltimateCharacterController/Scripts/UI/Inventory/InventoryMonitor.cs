/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI.Inventory
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Inventory;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A UI Panel used most times for debugging, To know what items are in the Inventory and adding, removing items
    /// </summary>
    public class InventoryMonitor : CharacterMonitor
    {
        [Tooltip("Should all of the items be drawn, or just the items that are currently in the Inventory?")]
        [SerializeField] protected bool m_DrawAllItemsInCollection = true;
        [Tooltip("The content in which the item View instances will be spawned.")]
        [SerializeField] protected RectTransform m_Content;
        [Tooltip("The item view prefab to instantiate for every item to show.")]
        [SerializeField] protected ItemView m_ItemViewPrefab;

        protected InventoryBase m_Inventory;
        protected ItemSetManager m_ItemSetManager;

        protected List<ItemView> m_ItemViews = new List<ItemView>();

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            base.OnAttachCharacter(character);

            if (character == null) {
                return;
            }

            m_Inventory = character.GetCachedComponent<InventoryBase>();
            m_ItemSetManager = character.GetCachedComponent<ItemSetManager>();

            Shared.Events.EventHandler.RegisterEvent<IItemIdentifier, int, int>(character, "OnInventoryAdjustItemIdentifierAmount", HandleItemAmountChange);
            Draw();
        }

        /// <summary>
        /// Handle an Item amount change in the Inventory by drawing the monitor.
        /// </summary>
        /// <param name="item">The item that changed amount.</param>
        /// <param name="previousAmount">The previous amount.</param>
        /// <param name="newAmount">The new amount.</param>
        private void HandleItemAmountChange(IItemIdentifier item, int previousAmount, int newAmount)
        {
            Draw();
        }

        /// <summary>
        /// Draw the contents of the Inventory.
        /// </summary>
        private void Draw()
        {
            var itemCount = 0;
            
            if (m_DrawAllItemsInCollection) {
                
                //Draw all the items in the item collection, even if the inventory does not contain them.
                var allItems = m_ItemSetManager.ItemCollection.ItemTypes;
                itemCount = allItems.Length;
                
                for (int i = 0; i < allItems.Length; i++) {
                    var item = allItems[i];
                    var amount = m_Inventory.GetItemIdentifierAmount(item);
                    Draw(i, new ItemIdentifierAmount(item.GetItemDefinition(), amount));
                }
            } else {
                // Draw only the items that are currently in the Inventory.
                var allItems = m_Inventory.GetAllItemIdentifiers();
                itemCount = allItems.Count;
                
                for (int i = 0; i < allItems.Count; i++) {
                    var item = allItems[i];
                    var amount = m_Inventory.GetItemIdentifierAmount(item);
                    Draw(i, new ItemIdentifierAmount(item.GetItemDefinition(), amount));
                }
            }

            // Return unused views to the pool.
            if (itemCount < m_ItemViews.Count) {
                for (int i = m_ItemViews.Count - 1; i >= itemCount; i--) {
                    ObjectPool.Destroy(m_ItemViews[i].gameObject);
                    m_ItemViews.RemoveAt(i);
                }
            }

            // Resize the content since it is most likely part of a Scroll View.
            m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x,
                m_ItemViewPrefab.gameObject.GetCachedComponent<RectTransform>().sizeDelta.y * itemCount);
        }

        /// <summary>
        /// Draw the item at a specific index.
        /// </summary>
        /// <param name="index">The index in which to draw the item.</param>
        /// <param name="itemAmount">The item to draw.</param>
        protected void Draw(int index, ItemIdentifierAmount itemAmount)
        {
            if (m_ItemViews.Count <= index) {
                var count = m_ItemViews.Count;
                for (int i = count; i < index+1; i++) {
                    var instance = ObjectPool.Instantiate(m_ItemViewPrefab.gameObject, m_Content);
                    
                    m_ItemViews.Add(instance.GetComponent<ItemView>());
                }
            }
            
            m_ItemViews[index].SetItemAmount(itemAmount, m_Inventory);
        }
    }
}