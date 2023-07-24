/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using UnityEngine;

    /// <summary>
    /// A simple class to add or remove items in an inventory.
    /// </summary>
    public class AddRemoveItem : MonoBehaviour
    {
        [Tooltip("The Inventory in which to add or remove items.")]
        [SerializeField] protected InventoryBase m_Inventory;
        [Tooltip("The Item Set Manager linked to the Inventory.")]
        [SerializeField] protected ItemSetManager m_ItemSetManager;
        [Tooltip("The item to add/remove.")]
        [SerializeField] protected ItemDefinitionBase m_Item;
        [Tooltip("The amount to add/remove.")]
        [SerializeField] protected int m_Amount;

        /// <summary>
        /// Add the item specified in the fields.
        /// </summary>
        public void AddItem()
        {
            m_Inventory.AddItemIdentifierAmount(m_Item.CreateItemIdentifier(), m_Amount);
            m_ItemSetManager.UpdateItemSets();
        }
        
        /// <summary>
        /// Remove the item specified in the fields.
        /// </summary>
        public void RemoveItem()
        {
            m_Inventory.RemoveItemIdentifierAmount(m_Item.CreateItemIdentifier(), m_Amount);
            m_ItemSetManager.UpdateItemSets();
        }
    }
}