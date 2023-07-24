/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI.Inventory
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// This simple component allows to add and remove an item from an ItemView to/from an Inventory.
    /// </summary>
    public class ItemAddRemoveButton : MonoBehaviour
    {
        [Tooltip("The button to add the item.")]
        [SerializeField] protected Button m_AddButton;
        [Tooltip("The button to remove the item.")]
        [SerializeField] protected Button m_RemoveButton;
        [Tooltip("The item view containing the item to add or remove.")]
        [SerializeField] protected ItemView m_ItemView;

        /// <summary>
        /// Listen to the click events.
        /// </summary>
        private void Awake()
        {
            if (m_AddButton != null) {
                m_AddButton.onClick.AddListener(HandleAddButtonClick);
            }
            if (m_RemoveButton != null) {
                m_RemoveButton.onClick.AddListener(HandleRemoveButtonClick);
            }
        }

        /// <summary>
        /// Handle the add item button click.
        /// </summary>
        private void HandleAddButtonClick()
        {
            var itemAmount = m_ItemView.ItemIdentifierAmount;
            m_ItemView.Inventory.AdjustItemIdentifierAmount(itemAmount.ItemIdentifier, 1);
        }

        /// <summary>
        /// Handle the remove item button click.
        /// </summary>
        private void HandleRemoveButtonClick()
        {
            var itemAmount = m_ItemView.ItemIdentifierAmount;
            m_ItemView.Inventory.AdjustItemIdentifierAmount(itemAmount.ItemIdentifier, -1);
        }

        /// <summary>
        /// Stop listening to the click events.
        /// </summary>
        private void OnDestroy()
        {
            if (m_AddButton != null) {
                m_AddButton.onClick.RemoveAllListeners();
            }
            
            if (m_RemoveButton != null) {
                m_RemoveButton.onClick.RemoveAllListeners();
            }
            
        }
    }
}