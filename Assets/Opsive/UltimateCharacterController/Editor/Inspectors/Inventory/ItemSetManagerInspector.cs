/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the ItemSetManager component.
    /// </summary>
    [CustomEditor(typeof(ItemSetManager))]
    public class ItemSetManagerInspector : ItemSetManagerBaseInspector
    {
        protected override List<string> ExcludedFields => new List<string>() { "m_ItemSetGroups", "m_ItemCollection" };
        
        public override Type ItemCategoryType => typeof(CategoryBase);

        private ItemSetManager m_ItemSetManager;
        private ObjectField m_ItemCollectionField;
        
        /// <summary>
        /// Adds the custom UIElements to the bottom of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_ItemSetManager = target as ItemSetManager;
        
            m_ItemCollectionField = new ObjectField("Item Collection");
            m_ItemCollectionField.objectType = typeof(ItemCollection);
            m_ItemCollectionField.value = m_ItemSetManager.ItemCollection;
            m_ItemCollectionField.RegisterValueChangedCallback(ctx =>
            {
                // Change all the Categories too, both on the ItemSetGroups and the ItemSetAbilities.
                m_ItemSetManager.SetItemCollection(ctx.newValue as ItemCollection, true, false);
                
                EditorUtility.SetDirty(m_ItemSetManager);
                var locomotion = m_ItemSetManager.GetComponent<UltimateCharacterLocomotion>();
                if (locomotion != null) {
                    EditorUtility.SetDirty(locomotion);
                }
                EditorUtility.SetDirty(m_ItemSetManager.gameObject);
                
                Refresh();
            });
            container.Add(m_ItemCollectionField);
            
            base.ShowFooterElements(container);
        }
    }
}