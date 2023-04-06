using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [CustomEditor(typeof(BuiltInWindows))]
    public class BuiltInWindowEditor : Editor
    {
        private SerializedProperty m_windowsProperty;
        private ReorderableList m_ReorderableList;

        private void OnEnable()
        {    
            m_windowsProperty = serializedObject.FindProperty("m_windows");

            m_ReorderableList = new ReorderableList(serializedObject: serializedObject, elements: m_windowsProperty, draggable: true, displayHeader: true,
                displayAddButton: true, displayRemoveButton: true);

            m_ReorderableList.drawHeaderCallback = DrawHeaderCallback;
            m_ReorderableList.drawElementCallback = DrawElementCallback;
            m_ReorderableList.elementHeightCallback += ElementHeightCallback;
            m_ReorderableList.onAddCallback += OnAddCallback;
        }

        private void OnDisable()
        {
            m_ReorderableList.elementHeightCallback -= ElementHeightCallback;
            m_ReorderableList.onAddCallback -= OnAddCallback;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Windows");
        }

        private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            SerializedProperty element = m_ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            SerializedProperty elementName = element.FindPropertyRelative("TypeName");
            string elementTitle = string.IsNullOrEmpty(elementName.stringValue) ? "New Window" : elementName.stringValue;
            
            EditorGUI.PropertyField(position:
                new Rect(rect.x += 10, rect.y, Screen.width * .8f, height: EditorGUIUtility.singleLineHeight), property:
                element, label: new GUIContent(elementTitle), includeChildren: true);
        }

        private float ElementHeightCallback(int index)
        {
            float propertyHeight =
                EditorGUI.GetPropertyHeight(m_ReorderableList.serializedProperty.GetArrayElementAtIndex(index), true);

            float spacing = EditorGUIUtility.singleLineHeight / 2;

            return propertyHeight + spacing;
        }

        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_ReorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}

