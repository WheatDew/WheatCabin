/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI.Inventory
{
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A simple component that adds a button to equip unequip an item set from an ItemSetView.
    /// </summary>
    public class ItemSetEquipButton : MonoBehaviour
    {
        [Tooltip("The Equip Unequip Button.")]
        [SerializeField] protected Button m_EquipUnequipButton;
        [Tooltip("The Item Set View with the Item Set to equip or unequip.")]
        [SerializeField] protected ItemSetView m_ItemSetView;

        public Button EquipUnequipButton { get => m_EquipUnequipButton; set => m_EquipUnequipButton = value; }
        public ItemSetView ItemSetView { get => m_ItemSetView; set => m_ItemSetView = value; }

        /// <summary>
        /// Listen to the click event.
        /// </summary>
        private void Awake()
        {
            m_EquipUnequipButton.onClick.AddListener(HandleEquipUnequipButtonClick);
        }

        /// <summary>
        /// Start equip unequip the item set.
        /// </summary>
        private void HandleEquipUnequipButtonClick()
        {
            var itemSet = m_ItemSetView.ItemSet;
            if (itemSet.IsValid == false) {
                Debug.LogWarning("The item set cannot be equipped if it is not valid: "+itemSet.State);
            }
            
            var equipUnequipAbility = itemSet.EquipUnequip;
            if (equipUnequipAbility == null) {
                Debug.LogError("The Equip Unequip Ability is missing for the ItemSet: "+itemSet.State);
                return;
            }
            equipUnequipAbility.StartEquipUnequip(itemSet.Index);
        }

        /// <summary>
        /// Stop listening to the click event.
        /// </summary>
        private void OnDestroy()
        {
            if(m_EquipUnequipButton == null){ return; }
            m_EquipUnequipButton.onClick.RemoveAllListeners();
        }
    }
}