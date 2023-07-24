/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using Opsive.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// An ItemType is a static representation of an item. Each item that interacts with the inventory must have an ItemType.
    /// </summary>
    public class ItemType : ItemDefinitionBase, IItemIdentifier
    {
        [Tooltip("The unique ID of the object within the collection.")]
        [SerializeField] protected uint m_ID;
        [Tooltip("The ID of the categories that the ItemType belongs to.")]
        [SerializeField] protected uint[] m_CategoryIDs = new uint[] { };
        [Tooltip("The maximum number of ItemTypes the character can hold.")]
        [SerializeField] protected int m_Capacity = int.MaxValue;
        [Tooltip("The Character ItemPrefabs (optional).")]
        [SerializeField] protected GameObject[] m_Prefabs;
        
        /// <summary>
        /// Definition ID, new is used to allow for a internal setter.
        /// </summary>
        public new uint ID { get { return m_ID; } set { m_ID = value; } }
        protected override uint IDGetter => ID;
        public uint[] CategoryIDs { get { return m_CategoryIDs; } set { m_CategoryIDs = value; } }
        public int Capacity { get { return m_Capacity; } set { m_Capacity = value; } }
        public GameObject[] Prefabs { get { return m_Prefabs; } set { m_Prefabs = value; } }

        private IItemCategoryIdentifier m_Category;

        /// <summary>
        /// Creates an IItemIdentifier based off of the ItemDefinitionBase.
        /// </summary>
        /// <returns>An IItemIdentifier based off of the ItemDefinitionBase.</returns>
        public override IItemIdentifier CreateItemIdentifier()
        {
            return this; // ItemTypes act as both definitions and identifiers.
        }

        /// <summary>
        /// Initializes the categories which belong to the ItemCollection.
        /// </summary>
        /// <param name="itemCollection">The ItemCollection that contain the categories.</param>
        public void Initialize(ItemCollection itemCollection)
        {
            if (CategoryIDs == null || CategoryIDs.Length == 0) {
                return;
            }

            if (CategoryIDs.Length > 1) {
                // Multiple categories are specified. Create a new runtime category that has multiple parents.
                var categoryParents = new Category[CategoryIDs.Length];
                for (int i = 0; i < CategoryIDs.Length; ++i) {
                    categoryParents[i] = itemCollection.GetCategory(CategoryIDs[i]);
                }
                var category = ScriptableObject.CreateInstance<Category>();
                category.hideFlags = HideFlags.HideAndDontSave;
                category.Parents = categoryParents;
                m_Category = category;
            } else {
                m_Category = itemCollection.GetCategory(CategoryIDs[0]);
            }
        }

        /// <summary>
        /// Returns the parent of the current identifier.
        /// </summary>
        /// <returns>The parent of the current identifier. Can be null.</returns>
        public override ItemDefinitionBase GetParent() { return null; }

        /// <summary>
        /// Check if the definition contains an Item Identifier.
        /// </summary>
        /// <returns>True if the item identifier is inherits the definition.</returns>
        public override bool InherentlyContains(IItemIdentifier itemIdentifier)
        {
            if (itemIdentifier == null) {
                return false;
            }

            return itemIdentifier.GetItemDefinition() == this;
        }

        /// <summary>
        /// Returns the category of the current identifier.
        /// </summary>
        /// <returns>The category of the current identifier. Can be null.</returns>
        public override IItemCategoryIdentifier GetItemCategory() { return m_Category; }

        /// <summary>
        /// Returns true if this item is part of the category with the matching ID.
        /// </summary>
        /// <param name="categoryID">The category ID.</param>
        /// <returns>True if the item is a child of the category with the matching ID.</returns>
        public bool InherentlyContainedByCategory(uint categoryID)
        {
            for (int i = 0; i < m_CategoryIDs.Length; i++) {
                if (m_CategoryIDs[i] == categoryID) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the ItemDefinition that the identifier uses.
        /// </summary>
        /// <returns>The ItemDefinition that the identifier uses.</returns>
        public ItemDefinitionBase GetItemDefinition()
        {
            return this; // ItemTypes act as both definitions and identifiers.
        }

        /// <summary>
        /// The Prefabs for this ItemDefinition, usually contains Character Item Prefabs.
        /// </summary>
        /// <returns>The Prefabs for this ItemDefinition.</returns>
        public ListSlice<GameObject> GetPrefabs()
        {
            return m_Prefabs;
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString() { return name; }
    }
}