/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Character.Effects;
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using UnityEngine;

    /// <summary>
    /// Stores the version 2 legacy data for updating.
    /// </summary>
    public class LegacyCharacterLocomotion : UltimateCharacterLocomotion
    {
        [Tooltip("The serialization data for the MovementTypes.")]
        [SerializeField] protected Serialization[] m_MovementTypeData;
        [Tooltip("The serialization data for the Abilities.")]
        [SerializeField] protected Serialization[] m_AbilityData;
        [Tooltip("The serialization data for the Item Abilities.")]
        [SerializeField] protected Serialization[] m_ItemAbilityData;
        [Tooltip("The serialization data for the Effects.")]
        [SerializeField] protected Serialization[] m_EffectData;

        public bool HasMovementTypeData => (m_MovementTypeData != null && m_MovementTypeData.Length > 0);
        public bool HasAbilityData => (m_AbilityData != null && m_AbilityData.Length > 0);
        public bool HasItemAbilityData => (m_ItemAbilityData != null && m_ItemAbilityData.Length > 0);
        public bool HasEffectData => (m_EffectData != null && m_EffectData.Length > 0);

        private MovementType[] m_LegacyMovementTypes;
        private Ability[] m_LegacyAbilities;
        private ItemAbility[] m_LegacyItemAbilities;
        private Effect[] m_LegacyEffects;

        /// <summary>
        /// Returns the deserialized the movement types.
        /// </summary>
        /// <returns>The deserialized movement types.</returns>
        public MovementType[] GetDeserializedMovementTypes()
        {
            if (m_LegacyMovementTypes == null && m_MovementTypeData != null && m_MovementTypeData.Length > 0) {
                m_LegacyMovementTypes = new MovementType[m_MovementTypeData.Length];
                for (int i = 0; i < m_MovementTypeData.Length; ++i) {
                    m_LegacyMovementTypes[i] = m_MovementTypeData[i].DeserializeFields(MemberVisibility.Public) as MovementType;
                }
            }
            return m_LegacyMovementTypes;
        }

        /// <summary>
        /// Returns the deserialized abilities.
        /// </summary>
        /// <returns>The deserialized abilities.</returns>
        public Ability[] GetDeserializedAbilities()
        {
            if (m_LegacyAbilities == null && m_AbilityData != null && m_AbilityData.Length > 0) {
                m_LegacyAbilities = new Ability[m_AbilityData.Length];
                for (int i = 0; i < m_AbilityData.Length; ++i) {
                    m_LegacyAbilities[i] = m_AbilityData[i].DeserializeFields(MemberVisibility.Public) as Ability;
                }
            }
            return m_LegacyAbilities;
        }

        /// <summary>
        /// Returns the deserialized item abilities.
        /// </summary>
        /// <returns>The deserialized item abilities.</returns>
        public ItemAbility[] GetDeserializedItemAbilities()
        {
            if (m_LegacyItemAbilities == null && m_ItemAbilityData != null && m_ItemAbilityData.Length > 0) {
                m_LegacyItemAbilities = new ItemAbility[m_ItemAbilityData.Length];
                for (int i = 0; i < m_ItemAbilityData.Length; ++i) {
                    m_LegacyItemAbilities[i] = m_ItemAbilityData[i].DeserializeFields(MemberVisibility.Public) as ItemAbility;
                }
            }
            return m_LegacyItemAbilities;
        }

        /// <summary>
        /// Returns the deserialize effects.
        /// </summary>
        /// <returns>The deserialized effects.</returns>
        public Effect[] GetDeserializedEffects()
        {
            if (m_LegacyEffects == null && m_EffectData != null && m_EffectData.Length > 0) {
                m_LegacyEffects = new Effect[m_EffectData.Length];
                for (int i = 0; i < m_EffectData.Length; ++i) {
                    m_LegacyEffects[i] = m_EffectData[i].DeserializeFields(MemberVisibility.Public) as Effect;
                }
            }
            return m_LegacyEffects;
        }
    }
}