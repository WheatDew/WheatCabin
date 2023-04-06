using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityWeld.Binding.Internal;

namespace Battlehub.UIControls.Binding
{
    [CustomEditor(typeof(VirtualizingTreeViewBinding))]
    class VirtualizingTreeViewBindingEditor : ControlBindingEditor
    {
        private VirtualizingTreeViewBinding targetScript;

        private bool m_viewModelPrefabModified;
       
        private bool m_viewModelSourceItemsPrefabModified;
        private bool m_viewModelSourceItemsAdapterPrefabModified;

        private bool m_viewModelTargetPrefabModified;
        
        protected override void OnEnable()
        {
            base.OnEnable();

            targetScript = (VirtualizingTreeViewBinding)target;
        }

        public override void OnInspectorGUI()
        {
            if (CannotModifyInPlayMode())
            {
                GUI.enabled = false;
            }

            UpdatePrefabModifiedProperties();

            var defaultLabelStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = m_viewModelPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent("View-model items source property", "Items source property on the view-model to bind to."),
                TypeResolverEx.FindBindableIHierarchicalDataProperties(targetScript),
                updatedValue => targetScript.ViewModelPropertyName = updatedValue,
                targetScript.ViewModelPropertyName,
                property => true
            );

            EditorGUILayout.Space();

            EditorStyles.label.fontStyle = m_viewModelSourceItemsPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowViewModelPropertyMenuWithNone(
                new GUIContent("View-model source items property", "Source items property on the view-model to bind to."),
                TypeResolverEx.FindBindableIEnumerableProperties(targetScript),
                updatedValue => targetScript.ViewModelSourceItemsPropertyName = updatedValue,
                targetScript.ViewModelSourceItemsPropertyName,
                property => true
            );

            var viewModelAdapterTypeNames = GetAdapterTypeNames(
                type => TypeResolver.FindAdapterAttribute(type).InputType == typeof(IEnumerable)
            );

            EditorStyles.label.fontStyle = m_viewModelSourceItemsAdapterPrefabModified
                ? FontStyle.Bold
                : defaultLabelStyle;

            ShowAdapterMenu(
                new GUIContent(
                    "Source items view-model adapter",
                    "Adapter that converts from the view back to the view-model"
                ),
                viewModelAdapterTypeNames,
                targetScript.SourceItemsUIToViewModelAdapter,
                newValue =>
                {
                    UpdateProperty(
                        updatedValue => targetScript.SourceItemsUIToViewModelAdapter = updatedValue,
                        targetScript.SourceItemsUIToViewModelAdapter,
                        newValue,
                        "Set source items view-model adapter"
                    );
                }
            );

            EditorGUILayout.Space();

            EditorStyles.label.fontStyle = m_viewModelTargetPrefabModified ? FontStyle.Bold : defaultLabelStyle;
            ShowViewModelPropertyMenuWithNone(
                new GUIContent("View-model target property", "Target property on the view-model to bind to."),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.ViewModelTargetPropertyName = updatedValue,
                targetScript.ViewModelTargetPropertyName,
                property => true
            );

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }

        /// <summary>
        /// Check whether each of the properties on the object have been changed from the value in the prefab.
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
                    case "m_viewModelPropertyName":
                        m_viewModelPrefabModified = property.prefabOverride;
                        break;
                    case "m_viewModelSourceItemsPropertyName":
                        m_viewModelSourceItemsPrefabModified = property.prefabOverride;
                        break;
                    case "m_sourceItemsUIToViewModelAdapter":
                        m_viewModelSourceItemsAdapterPrefabModified = property.prefabOverride;
                        break;
                    case "m_viewModelTargetPropertyName":
                        m_viewModelTargetPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }

        protected override IEnumerable<BindableEvent> GetBindableEvents()
        {
            return TypeResolverEx.GetBindableEvents(targetScript);
        }
    }
}
