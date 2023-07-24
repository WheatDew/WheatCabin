/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    using System;
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using UnityEngine.Serialization;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Base class which allows an object with the Inventory component to pickup items when a character enters the trigger.
    /// </summary>
    public abstract class ItemPickupBase : ObjectPickup
    {
        [Tooltip("Should the object be picked up even if the inventory cannot hold any more of the ItemIdentifier?")]
        [SerializeField] protected bool m_AlwaysPickup;

        public bool AlwaysPickup { get { return m_AlwaysPickup; } set { m_AlwaysPickup = value; } }

        private bool m_PickedUp;

        /// <summary>
        /// A GameObject has entered the trigger.
        /// </summary>
        /// <param name="other">The GameObject that entered the trigger.</param>
        public override void TriggerEnter(GameObject other)
        {
            DoPickup(other);
        }

        /// <summary>
        /// Picks up the object.
        /// </summary>
        /// <param name="target">The object doing the pickup.</param>
        public override void DoPickup(GameObject target)
        {
            // The object must have an enabled inventory in order for the item to be picked up.
            var inventory = target.GetCachedParentComponent<InventoryBase>();
            if (inventory == null || !inventory.enabled) {
                return;
            }

            // The collider must be a main character collider. Items or ragdoll colliders don't count.
            var layerManager = inventory.gameObject.GetCachedComponent<CharacterLayerManager>();
            if (layerManager == null || !MathUtility.InLayerMask(target.layer, layerManager.CharacterLayer)) {
                return;
            }

            TryItemPickup(inventory, -1);
        }

        /// <summary>
        /// Tries to pickup the item.
        /// </summary>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        private void TryItemPickup(InventoryBase inventory, int slotID)
        {
            if (m_PickupOnTriggerEnter) {
                DoItemPickup(inventory.gameObject, inventory, slotID, false, true);
            } else {
                // If the object is a character that has a disabled pickup item ability then the item should be picked up immediately, 
                // even if the pickup on trigger enter is disabled.
                var character = inventory.gameObject.GetCachedComponent<Character.UltimateCharacterLocomotion>();
                if (character != null) {
                    var pickup = character.GetAbility<Character.Abilities.Pickup>();
                    if (pickup != null && pickup.CanPickupItem()) {
                        DoItemPickup(inventory.gameObject, inventory, slotID, false, true);
                    }
                }
            }
        }

        /// <summary>
        /// Picks up the item.
        /// </summary>
        /// <param name="character">The character that should pick up the item.</param>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="pickupItemIdentifier">Should the ItemIdentifier be picked up? This should be false if the ItemIdentifier will later be picked up.</param>
        public void DoItemPickup(GameObject character, InventoryBase inventory, int slotID, bool immediatePickup, bool pickupItemIdentifier)
        {
            m_PickedUp = m_AlwaysPickup;
            if (pickupItemIdentifier) {
                // Even if the ItemIdentifier doesn't have space it may be equipped by the inventory. The object should be considered as picked up in this situation.
                EventHandler.RegisterEvent<CharacterItem, int>(character, "OnAbilityWillEquipItem", OnWillEquipItem);
                if (DoItemIdentifierPickup(character, inventory, slotID, immediatePickup, false)) {
                    m_PickedUp = true;
                }
                EventHandler.UnregisterEvent<CharacterItem, int>(character, "OnAbilityWillEquipItem", OnWillEquipItem);
            } else {
                // If pickup ItemIdentifier is false then the PickupItem ability will pick up the ItemIdentifier.
                m_PickedUp = true;
            }

            if (m_PickedUp) {
                ObjectPickedUp(character);
            }
        }

        /// <summary>
        /// Returns the ItemDefinitionAmount that the ItemPickup contains.
        /// </summary>
        /// <returns>The ItemDefinitionAmount that the ItemPickup contains.</returns>
        public abstract ItemIdentifierAmount[] GetItemDefinitionAmounts();

        /// <summary>
        /// Sets the ItemPickup ItemDefinitionAmounts value.
        /// </summary>
        /// <param name="itemDefinitionAmounts">The ItemDefinitionAmount that should be set.</param>
        public abstract void SetItemDefinitionAmounts(ItemIdentifierAmount[] itemDefinitionAmounts);

        /// <summary>
        /// Picks up the ItemIdentifier.
        /// </summary>
        /// <param name="character">The character that should pick up the ItemIdentifier.</param>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <returns>True if an ItemIdentifier was picked up.</returns>
        public bool DoItemIdentifierPickup(GameObject character, InventoryBase inventory, int slotID, bool immediatePickup, bool forceEquip)
        {
            EventHandler.ExecuteEvent(character, "OnItemPickupStartPickup");
            var result = DoItemIdentifierPickupInternal(character, inventory, slotID, immediatePickup, forceEquip);
            EventHandler.ExecuteEvent(character, "OnItemPickupStopPickup");
            return result;
        }

        /// <summary>
        /// Internal method which picks up the ItemIdentifier.
        /// </summary>
        /// <param name="character">The character that should pick up the ItemIdentifier.</param>
        /// <param name="inventory">The inventory belonging to the character.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <returns>True if an ItemIdentifier was picked up.</returns>
        protected abstract bool DoItemIdentifierPickupInternal(GameObject character, InventoryBase inventory, int slotID, bool immediatePickup, bool forceEquip);

        /// <summary>
        /// The specified item will be equipped.
        /// </summary>
        /// <param name="characterItem">The item that will be equipped.</param>
        /// <param name="slotID">The slot that the item will occupy.</param>
        private void OnWillEquipItem(CharacterItem characterItem, int slotID)
        {
            m_PickedUp = true;
        }
    }
}