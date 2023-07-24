/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI.Inventory
{
    using Opsive.Shared.UI;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;

    /// <summary>
    /// The item view is used to show an Item Amount in the UI.
    /// </summary>
    public class ItemView : MonoBehaviour
    {
        [Tooltip("The Item View amount text.")]
        [SerializeField] protected Shared.UI.Text m_Text;
        
        private InventoryBase m_Inventory;
        private ItemIdentifierAmount m_ItemIdentifierAmount;
        
        public Text Text { get => m_Text; set => m_Text = value; }

        public ItemIdentifierAmount ItemIdentifierAmount
        {
            get => m_ItemIdentifierAmount;
        }
        public InventoryBase Inventory
        {
            get => m_Inventory;
        }

        /// <summary>
        /// Set the item amount to show in the UI.
        /// </summary>
        /// <param name="itemIdentifierAmount">The item amount to view in the UI.</param>
        /// <param name="inventoryBase">The Inventory where the item comes from.</param>
        public virtual void SetItemAmount(ItemIdentifierAmount itemIdentifierAmount, InventoryBase inventoryBase)
        {
            m_ItemIdentifierAmount = itemIdentifierAmount;
            m_Inventory = inventoryBase;
            var itemName = itemIdentifierAmount.ItemDefinition?.name ?? "(null)";
            m_Text.text = $"{itemIdentifierAmount.Amount} {itemName}";
        }
    }
}