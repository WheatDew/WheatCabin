/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The ToggleEquip ability will equip or unequip the current ItemSet. ToggleEquip just specifies which ItemSet should be equipped/unequipped and then will let
    /// the EquipUnequip ability to do the actual equip/unequip.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Toggle Item Equip")]
    [AllowDuplicateTypes]
    public class ToggleEquip : EquipSwitcher
    {
        [Tooltip("Should the default ItemSet be toggled upon start?")]
        [SerializeField] protected bool m_ToggleDefaultItemSetOnStart;

        public bool ToggleDefaultItemSetOnStart { get { return m_ToggleDefaultItemSetOnStart; } set { m_ToggleDefaultItemSetOnStart = value; } }

        private ItemSet m_PreviousItemSet;
        private int m_PrevItemSetIndex = -1;
        private int m_NextItemSetIndex = -1;
        private bool m_ShouldEquipItem = true;

        /// <summary>
        /// Start the ability if the default ItemSet should be equipped.
        /// </summary>
        public override void Start()
        {
            if (m_ToggleDefaultItemSetOnStart) {
                var itemSetIndex = m_ItemSetManager.GetActiveItemSetIndex(m_ItemSetGroupIndex);
                if (itemSetIndex == -1) {
                    m_ShouldEquipItem = false;
                    StartAbility();
                }
            }
        }

        /// <summary>
        /// The EquipUnequip ability has changed the active ItemSet. Store this value so ToggleEquip knows which ItemSet to equip after the unequip.
        /// </summary>
        /// <param name="itemSetIndex">The updated active ItemSet index value.</param>
        protected override void OnItemSetIndexChange(int itemSetIndex)
        {
            if (!Enabled) {
                return;
            }

            var defaultItemSetIndex = m_ItemSetManager.GetDefaultItemSetIndex(m_ItemSetGroupIndex);
            m_ShouldEquipItem = itemSetIndex == defaultItemSetIndex;
            if (itemSetIndex == defaultItemSetIndex) {
                // The previous ItemSet may have been removed.
                if (!m_ItemSetManager.IsItemSetValid(m_ItemSetGroupIndex, m_PrevItemSetIndex, false)) {
                    m_PrevItemSetIndex = -1;
                    m_PreviousItemSet = null;
                }
                return;
            }
            m_PrevItemSetIndex = itemSetIndex;
            m_PreviousItemSet = m_ItemSetManager.GetItemSet(m_PrevItemSetIndex, m_ItemSetGroupIndex);
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            // Check if the previous Item Set is still active.
            var isPreviousValid = m_PreviousItemSet?.IsValid ?? false;

            m_NextItemSetIndex = -1;
            // PrevItemSetIndex will equal -1 if no non-default items have been equipped.
            if (isPreviousValid == false || m_PrevItemSetIndex == -1) {
                // Equip the Next valid item if there is one
                m_NextItemSetIndex = m_ItemSetManager.NextActiveItemSetIndex(m_ItemSetGroupIndex, m_EquipUnequipItemAbility.ActiveItemSetIndex, true);

                return m_NextItemSetIndex != -1 && m_NextItemSetIndex != m_EquipUnequipItemAbility.ActiveItemSetIndex;
            }

            // If the previous ItemSet does not match the index it means the ItemSet was reordered.
            // Get the new updated index.
            if (m_PreviousItemSet != m_ItemSetManager.GetItemSet(m_PrevItemSetIndex, m_ItemSetGroupIndex)) {
                m_PrevItemSetIndex = m_PreviousItemSet.Index;
            }

            return m_PrevItemSetIndex != m_ItemSetManager.GetDefaultItemSetIndex(m_ItemSetGroupIndex);
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            // Equip the Next valid item if there is one.
            if (m_NextItemSetIndex != -1) {
                m_EquipUnequipItemAbility.StartEquipUnequip(m_NextItemSetIndex);

                // It is up to the EquipUnequip ability to do the actual equip - stop the current ability.
                StopAbility();
                return;
            }

            // Start the EquipUnequip ability and then stop the ability. The EquipUnequip ability will do the actual work of equipping or unequipping the items.
            var defaultItemSetIndex = m_ItemSetManager.GetDefaultItemSetIndex(m_ItemSetGroupIndex);
            var itemSetIndex = m_ShouldEquipItem ? m_PrevItemSetIndex : defaultItemSetIndex;
            m_EquipUnequipItemAbility.StartEquipUnequip(itemSetIndex, false, false);
            m_ShouldEquipItem = itemSetIndex == defaultItemSetIndex;
            StopAbility();
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        protected override void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            if (m_Inventory.UnequipAllOnDeath) {
                m_PrevItemSetIndex = -1;
                m_PreviousItemSet = null;
            }
        }
    }
}