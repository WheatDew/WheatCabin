using UnityEditor;
using UnityEngine;
using UnityWeld.Binding.Internal;
using UnityWeld_Editor;

namespace Battlehub.RTEditor.Binding
{
    [CustomEditor(typeof(PropertyEditorBinding))]
    class PropertyEditorBindingEditor : BaseBindingEditor
    {
        private PropertyEditorBinding targetScript;
        private bool viewModelPropertyPrefabModified;

        private SerializedProperty m_labelProperty;
        private SerializedProperty m_isLabelVisibleProperty;
        private SerializedProperty m_enableUndoProperty;

        private void OnEnable()
        {
            targetScript = (PropertyEditorBinding)target;

            m_labelProperty = serializedObject.FindProperty("m_label");
            m_isLabelVisibleProperty = serializedObject.FindProperty("m_isLabelVisible");
            m_enableUndoProperty = serializedObject.FindProperty("m_enableUndo");
        }

        public override void OnInspectorGUI()
        {
            if (CannotModifyInPlayMode())
            {
                GUI.enabled = false;
            }

            UpdatePrefabModifiedProperties();

            var defaultLabelStyle = EditorStyles.label.fontStyle;
      
            EditorStyles.label.fontStyle = viewModelPropertyPrefabModified
                ? FontStyle.Bold
                : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent(
                    "View-model property",
                    "Property on the view-model to bind to."
                ),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.ViewModelPropertyName = updatedValue,
                targetScript.ViewModelPropertyName,
                property => true
            );

            EditorStyles.label.fontStyle = defaultLabelStyle;

            EditorGUILayout.PropertyField(m_labelProperty);
            EditorGUILayout.PropertyField(m_isLabelVisibleProperty);
            EditorGUILayout.PropertyField(m_enableUndoProperty);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Check whether each of the properties on the object have been changed 
        /// from the value in the prefab.
        /// </summary>
        private void UpdatePrefabModifiedProperties()
        {
            var property = serializedObject.GetIterator();
            // Need to call Next(true) to get the first child. Once we have it, Next(false)
            // will iterate through the properties.
            property.Next(true);
            do
            {
                switch (property.name)
                {
  
                    case "viewModelPropertyName":
                        viewModelPropertyPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
