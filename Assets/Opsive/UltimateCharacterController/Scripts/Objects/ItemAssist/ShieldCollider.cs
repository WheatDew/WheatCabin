/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items;
    using UnityEngine;

    /// <summary>
    /// The ShieldCollider component specifies the object that acts as a collider for the shield.
    /// </summary>
    public class ShieldCollider : MonoBehaviour
    {
        [Tooltip("A reference to the Shield item action.")]
        [SerializeField] protected ShieldAction m_ShieldAction;
        [Tooltip("Should the shield collider be disabled when unequipped (for example in the holster)?")]
        [SerializeField] protected bool m_DisableOnUnequip = true;

        [Shared.Utility.NonSerialized] public ShieldAction ShieldAction { get { return m_ShieldAction; } set { m_ShieldAction = value; } }

        private bool m_FirstPersonPerspective;
        private Collider m_Collider;
        private GameObject m_Character;
        private UltimateCharacterLocomotion m_CharacterLocomotion;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Start()
        {
            if (m_ShieldAction == null) {
                Debug.LogError("Error: The shield is not assigned. Ensure the shield is created from the Item Manager.", this);
                return;
            }

            var firstPersonPerspectiveItem = m_ShieldAction.CharacterItem.FirstPersonPerspectiveItem?.GetVisibleObject()?.transform;
            if (firstPersonPerspectiveItem != null && (transform == firstPersonPerspectiveItem || transform.IsChildOf(firstPersonPerspectiveItem))) {
                m_FirstPersonPerspective = true;
            } else {
                m_FirstPersonPerspective = false;
            }

            m_CharacterLocomotion = m_ShieldAction.gameObject.GetComponentInParent<UltimateCharacterLocomotion>();
            m_Character = m_CharacterLocomotion.gameObject;
            m_Collider = GetComponent<Collider>();
            m_Collider.enabled = m_CharacterLocomotion.FirstPersonPerspective == m_FirstPersonPerspective;

            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
            EventHandler.RegisterEvent<CharacterItem, int>(m_Character, "OnInventoryUnequipItem", OnUnequipItem);
            EventHandler.RegisterEvent<CharacterItem, int>(m_Character, "OnInventoryEquipItem", OnEquipItem);
        }

        /// <summary>
        /// The camera perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the camera in a first person view?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            if (m_ShieldAction == null) { return; }
            // The collider should only be enabled for the corresponding perspective and if it is active.
            m_Collider.enabled = (m_FirstPersonPerspective == firstPersonPerspective);
            if (m_DisableOnUnequip && m_Collider.enabled) {
                m_Collider.enabled = m_ShieldAction.CharacterItem.IsActive();
            }

        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="characterItem">The item that was unequipped.</param>
        /// <param name="slotID">The slot that the item was unequipped from.</param>
        private void OnUnequipItem(CharacterItem characterItem, int slotID)
        {
            if (m_ShieldAction == null) { return; }

            if (characterItem != m_ShieldAction.CharacterItem) {
                return;
            }

            if (m_DisableOnUnequip) {
                m_Collider.enabled = false;
            }
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="characterItem">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(CharacterItem characterItem, int slotID)
        {
            if (m_ShieldAction == null) { return; }

            if (characterItem != m_ShieldAction.CharacterItem) {
                return;
            }

            if (m_DisableOnUnequip) {
                // Re-enable the collider only if the perspective is correct.
                m_Collider.enabled = m_CharacterLocomotion.FirstPersonPerspective == m_FirstPersonPerspective;
            }
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_Character == null) {
                return;
            }

            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterChangePerspectives", OnChangePerspectives);
            EventHandler.UnregisterEvent<CharacterItem, int>(m_Character, "OnInventoryUnequipItem", OnUnequipItem);
            EventHandler.UnregisterEvent<CharacterItem, int>(m_Character, "OnInventoryEquipItem", OnEquipItem);
        }
    }
}