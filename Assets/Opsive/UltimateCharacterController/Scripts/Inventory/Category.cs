/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using System;
    using Opsive.Shared.Inventory;
    using Opsive.Shared.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A Category contains a grouping of ItemTypes.
    /// </summary>
    [System.Serializable]
    public class Category : CategoryBase, IItemCategoryIdentifier
    {
        [Tooltip("The ID of the category.")]
        [SerializeField] protected uint m_ID;

        /// <summary>
        /// Create a new category with the name provided.
        /// </summary>
        /// <param name="name">The name of the new category.</param>
        /// <returns>The new category.</returns>
        public static Category Create(string name)
        {
            var newCategory = ScriptableObject.CreateInstance<Category>();
            newCategory.ID = GenerateID();
            newCategory.name = name;

            return newCategory;
        }

        /// <summary>
        /// Category ID, new is used to allow for a internal setter.
        /// </summary>
        public new uint ID
        {
            get { 
                if (RandomID.IsIDEmpty(m_ID)) {
                    m_ID = GenerateID();
                }
                return m_ID; 
            }
            internal set => m_ID = value;
        }
        protected override uint IDGetter => ID;

        [System.NonSerialized] private Category[] m_Parents = new Category[0];
        public Category[] Parents { set { m_Parents = value; } }

        /// <summary>
        /// Returns a read only array of the direct parents of the current category.
        /// </summary>
        /// <returns>The direct parents of the current category.</returns>
        public virtual IReadOnlyList<IItemCategoryIdentifier> GetDirectParents()
        {
            return m_Parents;
        }

        /// <summary>
        /// Check if the category contains another category.
        /// </summary>
        /// <param name="other">The category to determine if it's within the category.</param>
        /// <param name="includeThis">Should the current category be included?</param>
        /// <returns>True if the category is inherits the category.</returns>
        public virtual bool InherentlyContains(IItemCategoryIdentifier other, bool includeThis = true)
        {
            if (other == null) { return false; }

            if (includeThis && ReferenceEquals(this, other)) {
                return true;
            }

            var otherParents = other.GetDirectParents();
            for (int i = 0; i < otherParents.Count; i++) {
                if (otherParents[i].InherentlyContains(this, true)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the category contains the specified item definition.
        /// </summary>
        /// <param name="itemDefinition">The definition to determine if it's within the category.</param>
        /// <param name="includeThis">Should the current category be included?</param>
        /// <returns>True if the category is inherits the category.</returns>
        public virtual bool InherentlyContains(ItemDefinitionBase itemDefinition, bool includeThis = true)
        {
            if (itemDefinition == null) {
                return false;
            }

            return InherentlyContains(itemDefinition.GetItemCategory(), true);
        }

        /// <summary>
        /// Check if the category contains the specified item.
        /// </summary>
        /// <param name="item">The item to determine if it's within the category.</param>
        /// <param name="includeThis">Should the current category be included?</param>
        /// <returns>True if the category is inherits the category.</returns>
        public virtual bool InherentlyContains(IItemIdentifier item, bool includeThis = true)
        {
            if (item == null) {
                return false;
            }

            return InherentlyContains(item.GetItemCategory(), true);
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString() { return name; }

        /// <summary>
        /// Returns a new ID for the category.
        /// </summary>
        /// <returns>The new cateogry ID.</returns>
        public static uint GenerateID() 
        {
            uint id;
            // The category ID is stored as a uint. Inspector fields aren't able to cast to a uint so keep generating a new ID for as long as 
            // the value is greater than the max int value.
            do {
                id = RandomID.Generate();
            } while (id > int.MaxValue);
            return id;
        }
    }
}