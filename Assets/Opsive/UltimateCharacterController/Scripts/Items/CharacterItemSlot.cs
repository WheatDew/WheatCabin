/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items
{
    using UnityEngine;

    /// <summary>
    /// Identifier class used to determine where the item GameObjects are located.
    /// </summary>
    public class CharacterItemSlot : MonoBehaviour, System.IComparable<CharacterItemSlot>
    {
        [Tooltip("An identifier for the CharacterItemSlot component.")]
        [SerializeField] protected int m_ID;

        public int ID { get { return m_ID; } set { m_ID = value; } }

        /// <summary>
        /// Compares to CharacterItemSlots.
        /// </summary>
        /// <param name="obj">The other CharacterItemSlot.</param>
        /// <returns>The CharacterItemSlot ID comparison.</returns>
        public int CompareTo(CharacterItemSlot obj)
        {
            return m_ID.CompareTo(obj.ID);
        }
    }
}