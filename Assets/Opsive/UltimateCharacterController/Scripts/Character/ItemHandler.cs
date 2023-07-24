/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Events;
    using Opsive.Shared.Input;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The ItemHandler manages the movement for each equipped item.
    /// </summary>
    public class ItemHandler : MonoBehaviour
    {
        private InventoryBase m_Inventory;
        private IPlayerInput m_PlayerInput;

        /// <summary>
        /// Registers for the CharacterInitializer callbacks if necessary.
        /// </summary>
        private void Awake()
        {
            if (!Game.CharacterInitializer.AutoInitialization) {
                Game.CharacterInitializer.Instance.OnStart += StartInternal;
                return;
            }
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Start()
        {
            if (!Game.CharacterInitializer.AutoInitialization) {
                return;
            }

            StartInternal();
        }

        /// <summary>
        /// Internal method which initializes the default values.
        /// </summary>
        private void StartInternal()
        {
            if (Game.CharacterInitializer.Instance != null) {
                Game.CharacterInitializer.Instance.OnStart -= StartInternal;
            }

            m_PlayerInput = gameObject.GetCachedComponent<IPlayerInput>();
            m_Inventory = gameObject.GetCachedComponent<InventoryBase>();

            EventHandler.RegisterEvent<Items.CharacterItem, int>(gameObject, "OnInventoryEquipItem", OnEquipItem);
        }

        /// <summary>
        /// Moves the item in each slot.
        /// </summary>
        private void Update()
        {
            var lookVectorMovement = m_PlayerInput.GetLookVector(true);
            for (int i = 0; i < m_Inventory.SlotCount; ++i) {
                var item = m_Inventory.GetActiveCharacterItem(i);
                if (item != null && item.IsActive() && item.DominantItem) {
                    item.Move(lookVectorMovement.x, lookVectorMovement.y);
                }
            }

            // Each object should only be updated once. Clear the frame after execution to allow the objects to be updated again.
            UnityEngineUtility.ClearUpdatedObjects();
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="characterItem">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(Items.CharacterItem characterItem, int slotID)
        {
            if (characterItem.IsActive() && characterItem.DominantItem) {
                UnityEngineUtility.ClearUpdatedObjects();
                characterItem.Move(0, 0);
            }
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Items.CharacterItem, int>(gameObject, "OnInventoryEquipItem", OnEquipItem);
        }
    }
}