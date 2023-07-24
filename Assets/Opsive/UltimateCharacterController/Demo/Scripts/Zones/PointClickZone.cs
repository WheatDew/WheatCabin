/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Prepares the character for a point & click zone.
    /// </summary>
    public class PointClickZone : MonoBehaviour, IZoneTrigger
    {
        private int[] m_ItemSetIndex;
        private float m_OriginalSpeedChangeMultiplier;
        private float m_OriginalSpeedChangeMaxValue;

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            // Ensure no items are equipped.
            var equipUnequipAbilities = characterLocomotion.GetAbilities<EquipUnequip>();
            m_ItemSetIndex = new int[equipUnequipAbilities.Length];
            for (int i = 0; i < equipUnequipAbilities.Length; ++i) {
                m_ItemSetIndex[i] = equipUnequipAbilities[i].ActiveItemSetIndex;
                equipUnequipAbilities[i].StartEquipUnequip(-1, true);
            }

            // Remember the initial speed change values so they can be reverted.
            var speedChange = characterLocomotion.GetAbility<SpeedChange>();
            m_OriginalSpeedChangeMultiplier = speedChange.SpeedChangeMultiplier;
            m_OriginalSpeedChangeMaxValue = speedChange.MaxSpeedChangeValue;
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The object that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            ExitZone(characterLocomotion.gameObject);
        }

        /// <summary>
        /// Resets the zone after the character exits.
        /// </summary>
        /// <param name="character">The character that exited the zone.</param>
        public void ExitZone(GameObject character)
        {
            if (m_ItemSetIndex == null) {
                return;
            }

            var characterLocomotion = character.GetComponent<UltimateCharacterLocomotion>();

            // Equip the original items.
            var equipUnequipAbilities = characterLocomotion.GetAbilities<EquipUnequip>();
            for (int i = 0; i < equipUnequipAbilities.Length; ++i) {
                equipUnequipAbilities[i].StartEquipUnequip(m_ItemSetIndex[i], true);
            }

            // The character should no longer move towards a destination outside of the trigger.
            characterLocomotion.MoveTowardsAbility.StopAbility();

            // Revert the speed change values.
            var speedChange = characterLocomotion.GetAbility<SpeedChange>();
            speedChange.SpeedChangeMultiplier = m_OriginalSpeedChangeMultiplier;
            speedChange.MaxSpeedChangeValue = m_OriginalSpeedChangeMaxValue;
            speedChange.MinSpeedChangeValue = -m_OriginalSpeedChangeMaxValue;

            m_ItemSetIndex = null;
        }
    }
}