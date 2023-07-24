/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using UnityEngine;

    /// <summary>
    /// The ItemSetManager manages the ItemSets belonging to the character.
    /// </summary>
    public class ItemSetManager : ItemSetManagerBase
    {
        [Tooltip("A reference to the ItemCollection that the inventory is using.")]
        [SerializeField] protected ItemCollection m_ItemCollection;

        public ItemCollection ItemCollection { 
            get { return m_ItemCollection; } 
            set { SetItemCollection(value, true, true); } 
        }

        /// <summary>
        /// Initializes the ItemSetManager.
        /// </summary>
        /// <param name="force">Should the ItemSet be force initialized?</param>
        public override void Initialize(bool force)
        {
            if (!Application.isPlaying) {
                base.Initialize(force);
                return;
            }

            if (m_Initialized && !force) {
                return;
            }

            // The ItemTypes get their categories from the ItemCollection.
            for (int i = 0; i < m_ItemCollection.ItemTypes.Length; ++i) {
                m_ItemCollection.ItemTypes[i].Initialize(m_ItemCollection);
            }

            base.Initialize(force);
        }

        /// <summary>
        /// Get the default ItemCategory to use when an ItemSetGroup does not have a category assigned.
        /// </summary>
        /// <returns>The default ItemCategory.</returns>
        public override CategoryBase GetDefaultCategory()
        {
            return m_ItemCollection.Categories[0];
        }

        /// <summary>
        /// Set the ItemCollection
        /// </summary>
        /// <param name="itemCollection">The new ItemCollection.</param>
        /// <param name="validate">Ensure both ItemSetGroups and ItemSetAbilities use that ItemCollection?</param>
        /// <param name="reinitialize">Re initialize if the ItemCollection is not the same as before?</param>
        public void SetItemCollection(ItemCollection itemCollection, bool validate, bool reinitialize)
        {
            if (m_ItemCollection == itemCollection) {
                return;
            }
            
            m_ItemCollection = itemCollection;

            // If the new itemCollection is null return early.
            if (m_ItemCollection == null) {
                return;
            }

            if (validate) {
                if (m_ItemSetGroups == null) {
                    m_ItemSetGroups = new ItemSetGroup[1];
                    m_ItemSetGroups[0] = new ItemSetGroup();
                }
                
                // Replace the ItemSetGroup categories.
                for (int i = 0; i < m_ItemSetGroups.Length; i++) {
                    var newCategory = m_ItemCollection.GetCategory(m_ItemSetGroups[i].CategoryID);

                    // Try getting by name if no matching ID is found.
                    if (newCategory == null) {
                        newCategory = m_ItemCollection.GetCategory(m_ItemSetGroups[i].SerializedItemCategory?.name);
                    }

                    // The first set should not be null.
                    if (i == 0 && newCategory == null && m_ItemCollection.Categories != null && m_ItemCollection.Categories.Length > 0) {
                        m_ItemSetGroups[i].SerializedItemCategory = m_ItemCollection.Categories[0];
                    } else {
                        m_ItemSetGroups[i].SerializedItemCategory = newCategory;
                    }
                }
                
                // Replace the ItemSetAbility categories.
                var abilities = GetComponent<UltimateCharacterLocomotion>()?.GetAbilities<ItemSetAbilityBase>();
                if (abilities != null) {
                    for (int i = 0; i < abilities.Length; i++) {
                        var newCategory = m_ItemCollection.GetCategory(abilities[i].ItemSetCategoryID);

                        // Try getting by name if no matching ID is found.
                        if (newCategory == null) {
                            newCategory = m_ItemCollection.GetCategory(abilities[i].ItemCategory?.name);
                        }
                        
                        abilities[i].ItemCategory = newCategory;
                    }
                }
            }

            if (reinitialize) {
                Initialize(true); 
            }
        }
    }
}