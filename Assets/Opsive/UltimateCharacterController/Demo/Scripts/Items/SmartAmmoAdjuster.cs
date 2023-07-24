/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Items
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.UI;
    using UnityEngine;

    /// <summary>
    /// A simple character monitor which has a function to add ammo for the currently equipped item
    /// Works on both Throwable and Shootable Item Actions
    /// </summary>
    public class SmartAmmoAdjuster : CharacterMonitor
    {
        [Tooltip("The quantity of ammo to add (can be negative to remove).")]
        [SerializeField] private int m_AmountToAdjust = 1;

        private InventoryBase m_Inventory;
        private ItemSetManagerBase m_ItemSetManager;

        protected override void OnAttachCharacter(GameObject character)
        {
            base.OnAttachCharacter(character);
            if (character != null) {
                m_Inventory = character.GetCachedComponent<InventoryBase>();
                m_ItemSetManager = character.GetCachedComponent<ItemSetManagerBase>();
            }
        }

        /// <summary>
        /// Adjust the amount of ammo.
        /// </summary>
        [ContextMenu("Adjust Ammo")]
        public void AdjustAmmo()
        {
            AdjustAmmo(m_AmountToAdjust);
        }

        /// <summary>
        /// Adjust the amount of ammo.
        /// </summary>
        /// <param name="amount">The amount to adjust.</param>
        public void AdjustAmmo(int amount)
        {
            var activeItemSet = m_ItemSetManager.GetActiveItemSet(0);
            if(activeItemSet == null){ return; }

            var characterItemRight = activeItemSet.GetCharacterItem(0);
            AdjustAmmo(characterItemRight, amount);
            var characterItemLeft = activeItemSet.GetCharacterItem(1);
            AdjustAmmo(characterItemLeft, amount);
        }

        /// <summary>
        /// Adjust the amount of ammo.
        /// </summary>
        /// <param name="characterItem">The character item to add ammo for.</param>
        /// <param name="amount">The amount to adjust.</param>
        public void AdjustAmmo(CharacterItem characterItem, int amount)
        {
            if (characterItem == null) { return; }

            var itemActions = characterItem.ItemActions;
            for (int i = 0; i < itemActions.Length; i++) {
                if (itemActions[i] is ShootableAction shootableAction) {
                    shootableAction.MainAmmoModule.AdjustAmmoAmount(amount);
                }else if (itemActions[i] is ThrowableAction throwableAction) {
                    throwableAction.MainAmmoModule.AdjustAmmoAmount(amount);
                }
            }
        }
    }
}
