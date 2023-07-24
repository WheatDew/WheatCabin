/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects.CharacterAssist
{
    using System.Collections.Generic;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Inventory;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Editor.Managers;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the ItemPickup component.
    /// </summary>
    [CustomEditor(typeof(ItemPickup), true)]
    public class ItemPickupInspector : ObjectPickupInspector
    {
        private ItemCollection m_ItemCollection;
        private ReorderableList m_ReordableItemAmount;

        protected List<string> m_ItemPickupExcludeField;

        protected override List<string> ExcludedFields => m_ItemPickupExcludeField;
        
        /// <summary>
        /// Finds the Item Collection.
        /// </summary>
        protected override void InitializeInspector()
        {
            base.InitializeInspector();
            m_ItemPickupExcludeField = new List<string>(){"m_ItemDefinitionAmounts"};
            m_ItemPickupExcludeField.AddRange(m_ObjectPickupExcludeField);
            
            m_ItemCollection = ManagerUtility.FindItemCollection(this);
        }

        /// <summary>
        /// Adds the custom UIElements to the bottom of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            var imguiContainer = new IMGUIContainer(() =>
            {
                EditorGUI.BeginChangeCheck();
                
                if (m_ReordableItemAmount == null) {
                    var itemListProperty = PropertyFromName("m_ItemDefinitionAmounts");
                    m_ReordableItemAmount = new ReorderableList(serializedObject, itemListProperty, true, true, true, true);
                    m_ReordableItemAmount.drawHeaderCallback = OnItemIdentifierAmountHeaderDraw;
                    m_ReordableItemAmount.drawElementCallback = OnItemIdentifierAmountElementDraw;
                    m_ReordableItemAmount.elementHeight = 18;
                }
                var listRect = GUILayoutUtility.GetRect(0, m_ReordableItemAmount.GetHeight());
                listRect.x += EditorGUI.indentLevel * Shared.Editor.Inspectors.Utility.InspectorUtility.IndentWidth;
                listRect.xMax -= EditorGUI.indentLevel * Shared.Editor.Inspectors.Utility.InspectorUtility.IndentWidth;
                m_ReordableItemAmount.DoList(listRect);
                
                if (EditorGUI.EndChangeCheck()) {
                    Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");
                    serializedObject.ApplyModifiedProperties();
                }
            });

            container.Add(imguiContainer);
            
            base.ShowFooterElements(container);
        }
        
        /// <summary>
        /// Draws the ItemIdentifierAmount ReordableList header.
        /// </summary>
        private void OnItemIdentifierAmountHeaderDraw(Rect rect)
        {
            ItemDefinitionAmountInspector.OnItemDefinitionAmountHeaderDraw(rect);
        }

        /// <summary>
        /// Draws the ItemIdentifierAmount ReordableList element.
        /// </summary>
        private void OnItemIdentifierAmountElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            ItemDefinitionAmountInspector.OnItemDefinitionAmountElementDraw(PropertyFromName("m_ItemDefinitionAmounts"), rect, index, isActive, isFocused);
        }
    }
}