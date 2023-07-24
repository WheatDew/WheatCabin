/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using UnityEngine;

    /// <summary>
    /// Specifies the amount of each ItemDefinitionBase that the character can pickup or is loaded with the default inventory.
    /// </summary>
    [System.Serializable]
    public struct ItemIdentifierAmount
    {
        [Tooltip("The type of item.")]
        [SerializeField] public ItemDefinitionBase ItemDefinition;
        [Tooltip("The number of ItemIdentifier units to pickup.")]
        [SerializeField] public int Amount;

        private IItemIdentifier m_ItemIdentifier;

        public IItemIdentifier ItemIdentifier { 
            get {
                if (Application.isPlaying && m_ItemIdentifier == null) {
                    m_ItemIdentifier = ItemDefinition?.CreateItemIdentifier();
                }
                return m_ItemIdentifier;
            } 
        }

        /// <summary>
        /// ItemDefinitionAmount constructor with two parameters.
        /// </summary>
        /// <param name="itemDefinition">The definition of item.</param>
        /// <param name="amount">The amount of ItemDefinitionBase.</param>
        public ItemIdentifierAmount(ItemDefinitionBase itemDefinition, int amount)
        {
            ItemDefinition = itemDefinition;
            Amount = amount;
            m_ItemIdentifier = null;
        }
        
        /// <summary>
        /// ItemDefinitionAmount constructor with two parameters.
        /// </summary>
        /// <param name="itemDefinition">The definition of item.</param>
        /// <param name="amount">The amount of ItemDefinitionBase.</param>
        public ItemIdentifierAmount(IItemIdentifier itemIdentifier, int amount)
        {
            ItemDefinition = itemIdentifier?.GetItemDefinition();
            Amount = amount;
            m_ItemIdentifier = itemIdentifier;
        }
    }
}