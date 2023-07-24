/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Custom inspector for the Inventory component.
    /// </summary>
    [CustomEditor(typeof(UltimateCharacterController.Inventory.Inventory))]
    public class InventoryInspector : InventoryBaseInspector
    {
        private ReorderableList m_DefaultLoadoutReordableList;
        private ReorderableList m_RemoveExceptionReorderableList;

        /// <summary>
        /// Draws the properties for the inventory subclass.
        /// </summary>
        protected override void DrawInventoryProperties()
        {
            if (Foldout("Default Loadout")) {
                EditorGUI.indentLevel++;
                if (m_DefaultLoadoutReordableList == null) {
                    var itemListProperty = PropertyFromName("m_DefaultLoadout");
                    m_DefaultLoadoutReordableList = new ReorderableList(serializedObject, itemListProperty, true, true, true, true);
                    m_DefaultLoadoutReordableList.drawHeaderCallback = OnDefaultLoadoutHeaderDraw;
                    m_DefaultLoadoutReordableList.drawElementCallback = OnDefaultLoadoutElementDraw;
                }
                var listRect = GUILayoutUtility.GetRect(0, m_DefaultLoadoutReordableList.GetHeight());
                listRect.x += EditorGUI.indentLevel * Shared.Editor.Inspectors.Utility.InspectorUtility.IndentWidth;
                listRect.xMax -= EditorGUI.indentLevel * Shared.Editor.Inspectors.Utility.InspectorUtility.IndentWidth;
                m_DefaultLoadoutReordableList.DoList(listRect);

                // Ensure the Item Types are valid.
                if (!Application.isPlaying) {
                    var itemSetManager = (target as Inventory).GetComponent<ItemSetManager>();
                    if (itemSetManager != null) {
                        var itemCollection = itemSetManager.ItemCollection;
                        if (itemCollection != null) {
                            var defaultLoadout = (target as Inventory).DefaultLoadout;
                            if (defaultLoadout != null) {
                                for (int i = 0; i < defaultLoadout.Length; ++i) {
                                    if (defaultLoadout[i].ItemDefinition == null) {
                                        continue;
                                    }

                                    var validItemType = false;
                                    if (itemCollection.ItemTypes != null) {
                                        for (int j = 0; j < itemCollection.ItemTypes.Length; ++j) {
                                            if (defaultLoadout[i].ItemDefinition == itemCollection.ItemTypes[j]) {
                                                validItemType = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (!validItemType) {
                                        EditorGUILayout.HelpBox($"The ItemType {defaultLoadout[i].ItemDefinition} does not belong to the Item Collection specified on the Item Set Manager.", MessageType.Error);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.PropertyField(PropertyFromName("m_RemoveAllOnDeath"));
            
            if (Foldout("Remove Exceptions")) {
                EditorGUI.indentLevel++;
                if (m_RemoveExceptionReorderableList == null) {
                    var itemListProperty = PropertyFromName("m_RemoveExceptions");
                    m_RemoveExceptionReorderableList = new ReorderableList(serializedObject, itemListProperty, true, true, true, true);
                    m_RemoveExceptionReorderableList.drawHeaderCallback = OnRemoveExceptionHeaderDraw;
                    m_RemoveExceptionReorderableList.drawElementCallback = OnRemoveExceptionElementDraw;
                }
                var listRect = GUILayoutUtility.GetRect(0, m_RemoveExceptionReorderableList.GetHeight());
                listRect.x += EditorGUI.indentLevel * Shared.Editor.Inspectors.Utility.InspectorUtility.IndentWidth;
                listRect.xMax -= EditorGUI.indentLevel * Shared.Editor.Inspectors.Utility.InspectorUtility.IndentWidth;
                m_RemoveExceptionReorderableList.DoList(listRect);

                // Ensure the Item Definitions are valid.
                if (!Application.isPlaying) {
                    var itemSetManager = (target as Inventory).GetComponent<ItemSetManager>();
                    if (itemSetManager != null) {
                        var itemCollection = itemSetManager.ItemCollection;
                        if (itemCollection != null) {
                            var removeExceptions = (target as Inventory).RemoveExceptions;
                            if (removeExceptions != null) {
                                for (int i = 0; i < removeExceptions.Length; ++i) {
                                    if (removeExceptions[i] == null) {
                                        continue;
                                    }

                                    var validItemType = false;
                                    if (itemCollection.ItemTypes != null) {
                                        for (int j = 0; j < itemCollection.ItemTypes.Length; ++j) {
                                            if (removeExceptions[i] == itemCollection.ItemTypes[j].GetItemDefinition()) {
                                                validItemType = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (!validItemType) {
                                        EditorGUILayout.HelpBox($"The Item Definition {removeExceptions[i]} does not belong to the Item Collection specified on the Item Set Manager.", MessageType.Error);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }

            var startEquipSetting = PropertyFromName("m_LoadoutEquip");
            var optionsRect = GUILayoutUtility.GetRect(0, 18);
            var position = EditorGUI.PrefixLabel(optionsRect, new GUIContent("Loadout Equip", startEquipSetting.tooltip));

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var optionsProperty = startEquipSetting.FindPropertyRelative("m_Option");
            optionsRect = new Rect(position.x, position.y, position.width, 18);
            EditorGUI.PropertyField(optionsRect, optionsProperty, GUIContent.none);
            var value = (Inventory.LoadoutEquipSettings)ControlUtility.GetTargetObjectOfProperty(startEquipSetting);
            if (value.Option == Inventory.LoadoutEquipSettings.Options.ItemSetName) {
                var nameRect = GUILayoutUtility.GetRect(0, 18);
                nameRect = new Rect(position.x, nameRect.y, position.width, 18);
                var nameProperty = startEquipSetting.FindPropertyRelative("m_ItemSetName");
                EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
            } else if (value.Option == Inventory.LoadoutEquipSettings.Options.ItemSetIndex) {
                var indexRect = GUILayoutUtility.GetRect(0, 18);
                indexRect = new Rect(position.x, indexRect.y, position.width, 18);
                var indexProperty = startEquipSetting.FindPropertyRelative("m_Index");
                EditorGUI.PropertyField(indexRect, indexProperty, GUIContent.none);
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
        }

        /// <summary>
        /// Draws the DefaultLoadout ReordableList header.
        /// </summary>
        private void OnDefaultLoadoutHeaderDraw(Rect rect)
        {
            ItemDefinitionAmountInspector.OnItemDefinitionAmountHeaderDraw(rect);
        }

        /// <summary>
        /// Draws the DefaultLoadout ReordableList element.
        /// </summary>
        private void OnDefaultLoadoutElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            ItemDefinitionAmountInspector.OnItemDefinitionAmountElementDraw(PropertyFromName("m_DefaultLoadout"), rect, index, isActive, isFocused);
        }
        
        /// <summary>
        /// Draws the RemoveExceptions ReordableList header.
        /// </summary>
        private void OnRemoveExceptionHeaderDraw(Rect rect)
        {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Item Definition");
        }

        /// <summary>
        /// Draws the RemoveExceptions ReordableList element.
        /// </summary>
        private void OnRemoveExceptionElementDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.height = 16;
            EditorGUI.PropertyField(rect, PropertyFromName("m_RemoveExceptions").GetArrayElementAtIndex(index), new GUIContent());
        }
    }
}