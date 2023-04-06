using Battlehub.UIControls.Binding;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityWeld.Binding.Internal;

namespace Battlehub.RTEditor.Binding
{
    [CustomEditor(typeof(ViewBinding))]
    public class ViewBindingEditor : ControlBindingEditor
    {
        private bool m_viewModelDragObjectsPrefabModified;
        private bool m_viewModelCanDropObjectsPrefabModified;
        private ViewBinding m_targetScript;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_targetScript = (ViewBinding)target;
        }

        public override void OnInspectorGUI()
        {
            UpdateModifiedProperties();

            var defaultLabelStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = m_viewModelDragObjectsPrefabModified
              ? FontStyle.Bold
              : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent(
                    "Drag External Objects Property",
                    "Property on the view-model to bind to."
                ),
                TypeResolver.FindBindableProperties(m_targetScript),
                updatedValue => m_targetScript.ViewModelDragObjectsPropertyName = updatedValue,
                m_targetScript.ViewModelDragObjectsPropertyName,
                property => property.PropertyType == typeof(IEnumerable<object>)
            );

            EditorGUILayout.Space();

            EditorStyles.label.fontStyle = m_viewModelCanDropObjectsPrefabModified
                ? FontStyle.Bold
                : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent(
                    "Can Drop External Objects Property",
                    "Property on the view-model to bind to."
                ),
                TypeResolver.FindBindableProperties(m_targetScript),
                updatedValue => m_targetScript.ViewModelCanDropObjectsPropertyName = updatedValue,
                m_targetScript.ViewModelCanDropObjectsPropertyName,
                property => property.PropertyType == typeof(bool)
            );

            EditorStyles.label.fontStyle = EditorStyles.label.fontStyle;

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }

        private void UpdateModifiedProperties()
        {
            var property = serializedObject.GetIterator();
            property.Next(true);
            do
            {
                switch (property.name)
                {
                    case "m_viewModelDragObjectsPropertyName":
                        m_viewModelDragObjectsPrefabModified = property.prefabOverride;
                        break;

                    case "m_viewModelCanDropObjectsPropertyName":
                        m_viewModelCanDropObjectsPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }


    }
}
