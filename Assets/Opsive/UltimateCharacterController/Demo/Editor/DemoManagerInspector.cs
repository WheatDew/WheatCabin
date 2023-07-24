/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Demo
{
    using Opsive.Shared.Editor.Inspectors;
    using Opsive.UltimateCharacterController.Demo;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Shows a custom inspector for the DemoManager component.
    /// </summary>
    [CustomEditor(typeof(DemoManager), true)]
    public class DemoManagerInspector : InspectorBase
    {
        private const string c_EditorPrefsSelectedDemoZoneIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Demo.SelectedDemoZoneIndex";
        private string SelectedViewTypeIndexKey { get { return c_EditorPrefsSelectedDemoZoneIndexKey + "." + target.GetType() + "." + target.name; } }

        private ReorderableList m_DemoZonesList;

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            DrawCharacterField();
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            if (PropertyFromName("m_PerspectiveToggle").objectReferenceValue == null) {
                EditorGUILayout.PropertyField(PropertyFromName("m_DefaultFirstPersonStart"));
            }
#endif
            if (Foldout("Free Roam")) {
                EditorGUI.indentLevel++;
                DrawFreeRoamFields();
                EditorGUI.indentLevel--;
            }
            if (Foldout("UI")) {
                EditorGUI.indentLevel++;
                DrawUIFields();
                EditorGUI.indentLevel--;
            }

            if (m_DemoZonesList == null) {
                m_DemoZonesList = new ReorderableList(serializedObject, PropertyFromName("m_DemoZones"), true, true, true, true);
                m_DemoZonesList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width - 12, EditorGUIUtility.singleLineHeight), "Demo Zones");
                };
                m_DemoZonesList.onSelectCallback += (ReorderableList list) =>
                {
                    EditorPrefs.SetInt(SelectedViewTypeIndexKey, list.index);
                };
                m_DemoZonesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.LabelField(rect, (target as DemoManager).DemoZones[index].Header);
                };
                if (EditorPrefs.GetInt(SelectedViewTypeIndexKey, -1) != -1) {
                    m_DemoZonesList.index = EditorPrefs.GetInt(SelectedViewTypeIndexKey, -1);
                }
            }

            m_DemoZonesList.DoLayoutList();
            DrawSelectedDemoZone();
            
            EditorGUILayout.PropertyField(PropertyFromName("m_OnCharacterInitialized"));

            if (EditorGUI.EndChangeCheck()) {
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draws the inspected character field.
        /// </summary>
        protected virtual void DrawCharacterField()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_Character"));
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            EditorGUILayout.PropertyField(PropertyFromName("m_DefaultFirstPersonStart"));
#endif
        }

        /// <summary>
        /// Draws the free roam fields.
        /// </summary>
        protected virtual void DrawFreeRoamFields()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_FreeRoam"));
            EditorGUILayout.PropertyField(PropertyFromName("m_FreeRoamSpawnLocation"));
        }

        /// <summary>
        /// Draws the UI fields.
        /// </summary>
        protected virtual void DrawUIFields()
        {
            EditorGUILayout.PropertyField(PropertyFromName("m_ZoneSelection"));
            EditorGUILayout.PropertyField(PropertyFromName("m_PerspectiveToggle"));
            EditorGUILayout.PropertyField(PropertyFromName("m_Header"));
            EditorGUILayout.PropertyField(PropertyFromName("m_Description"));
            EditorGUILayout.PropertyField(PropertyFromName("m_ControlsParent"));
            EditorGUILayout.PropertyField(PropertyFromName("m_KeyboardMapping"));
            EditorGUILayout.PropertyField(PropertyFromName("m_ControllerMapping"));
            EditorGUILayout.PropertyField(PropertyFromName("m_InGameZoneContent"));
            EditorGUILayout.PropertyField(PropertyFromName("m_InGameZoneDescription"));
            EditorGUILayout.PropertyField(PropertyFromName("m_AddAllItemsToCharacter"));
            EditorGUILayout.PropertyField(PropertyFromName("m_AddOnDemoManager"));
        }

        /// <summary>
        /// Draws the fields for the selected demo zone.
        /// </summary>
        private void DrawSelectedDemoZone()
        {
            var demoZonesProperty = PropertyFromName("m_DemoZones");
            if (m_DemoZonesList.index == -1 || m_DemoZonesList.index >= demoZonesProperty.arraySize) {
                return;
            }

            var demoZoneProperty = demoZonesProperty.GetArrayElementAtIndex(m_DemoZonesList.index);
            EditorGUILayout.PropertyField(demoZoneProperty.FindPropertyRelative("m_Zone"));
            EditorGUILayout.LabelField(demoZoneProperty.FindPropertyRelative("m_Header").stringValue + " Demo Zone", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(demoZoneProperty.FindPropertyRelative("m_Header"));
            EditorGUILayout.PropertyField(demoZoneProperty.FindPropertyRelative("m_Sprite"));
            EditorGUILayout.LabelField("Description");
            demoZoneProperty.FindPropertyRelative("m_Description").stringValue = InspectorUtility.DrawEditorWithoutSelectAll(() => 
                            EditorGUILayout.TextArea(demoZoneProperty.FindPropertyRelative("m_Description").stringValue, Shared.Editor.Inspectors.Utility.InspectorStyles.WordWrapTextArea));
        }
    }
}