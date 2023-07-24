/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI.Inventory
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The Item Set View shows an Item Set in the UI.
    /// </summary>
    public class ItemSetView : MonoBehaviour
    {
        public Action<ItemSet> OnItemSetChange;
        
        [Tooltip("The Content in which to spawn the Item View instance for each item per slot.")]
        [SerializeField] protected RectTransform m_Content;
        [Tooltip("The Item View prefab used to show the Items in each slot.")]
        [SerializeField] protected ItemView m_ItemViewPrefab;
        [Tooltip("The item set state name text.")]
        [SerializeField] protected Shared.UI.Text m_StateName;
        [Tooltip("A gameObject to activate if the item set is active.")]
        [SerializeField] protected GameObject m_SetActive;
        [Tooltip("A gameObject to activate if the item set is not active.")]
        [SerializeField] protected GameObject m_SetInactive;
        [Tooltip("A gameObject to activate if the item set is enabled.")]
        [SerializeField] protected GameObject m_SetEnabled;
        [Tooltip("A gameObject to activate if the item set is not enabled.")]
        [SerializeField] protected GameObject m_SetDisabled;
 
        protected List<ItemView> m_ItemViewList = new List<ItemView>();
        private ItemSet m_ItemSet;
        
        public ItemSet ItemSet
        {
            get => m_ItemSet;
            set => SetItemSet(value);
        }

        /// <summary>
        /// Set the item set to view.
        /// </summary>
        /// <param name="itemSet">The item set to view.</param>
        public virtual void SetItemSet(ItemSet itemSet)
        {
            m_ItemSet = itemSet;
            OnItemSetChange?.Invoke(itemSet);

            if (m_ItemSet == null) {
                m_StateName.text = "(null)";
                for (int i = m_ItemViewList.Count - 1; i >= 0; i--) {
                    ObjectPool.Destroy(m_ItemViewList[i].gameObject);
                }
                m_ItemViewList.Clear();
                return;
            }
            
            m_StateName.text = itemSet.State;
            
            if (m_SetActive != null) {
                m_SetActive.SetActive(itemSet.Active);
            }
            if (m_SetInactive != null) {
                m_SetInactive.SetActive(!itemSet.Active);
            }
            if (m_SetEnabled != null) {
                m_SetEnabled.SetActive(itemSet.Enabled);
            }
            if (m_SetDisabled != null) {
                m_SetDisabled.SetActive(!itemSet.Enabled);
            }

            var itemCount = itemSet?.ItemIdentifiers?.Length ?? 0;
            for (int i = 0; i < itemCount; i++) {
                if (m_ItemViewList.Count <= i || m_ItemViewList[i] != null) {
                    var instance = ObjectPool.Instantiate(m_ItemViewPrefab.gameObject, m_Content);
                    
                    m_ItemViewList.Add(instance.GetComponent<ItemView>());
                }

                m_ItemViewList[i].SetItemAmount(new ItemIdentifierAmount(itemSet.ItemIdentifiers[i], i), itemSet.ItemSetGroup.ItemSetManager.CharacterInventory);
            }

            // Return to the pool the unused Item Views.
            for (int i = m_ItemViewList.Count - 1; i >= itemCount; i--) {
                ObjectPool.Destroy(m_ItemViewList[i].gameObject);
                m_ItemViewList.RemoveAt(i);
            }
        }
    }
}