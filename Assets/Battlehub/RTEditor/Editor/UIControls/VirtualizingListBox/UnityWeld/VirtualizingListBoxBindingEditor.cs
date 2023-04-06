using Battlehub.UIControls.Binding;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityWeld.Binding.Internal;

namespace Battlehub.UIControls
{
    [CustomEditor(typeof(VirtualizingListBoxBinding))]
    class VirtualizingListBoxBindingEditor : ControlBindingEditor
    {
        private VirtualizingListBoxBinding targetScript;
        private SerializedProperty m_canSelectProperty;
        private SerializedProperty m_canReorderProperty;
        private SerializedProperty m_canRemoveProperty;
        private SerializedProperty m_selectOnPointerUpProperty;
        private SerializedProperty m_canUnselectAllProperty;
        private SerializedProperty m_scrollSelectedIntoView;

        private bool m_viewModelItemsPrefabModified;
        private bool m_viewModelItemsAdapterPrefabModified;
        private bool m_viewModelSelectedItemsPrefabModified;
        private bool m_viewModelSelectedItemsAdapterPrefabModified;
        private bool m_viewModelTargetPrefabModified;

        protected override void OnEnable()
        {
            base.OnEnable();
            targetScript = (VirtualizingListBoxBinding)target;

            m_canSelectProperty = serializedObject.FindProperty("m_canSelect");
            m_canReorderProperty = serializedObject.FindProperty("m_canReorder");
            m_canRemoveProperty = serializedObject.FindProperty("m_canRemove");
            m_selectOnPointerUpProperty = serializedObject.FindProperty("m_selectOnPointerUp");
            m_canUnselectAllProperty = serializedObject.FindProperty("m_canUnselectAll");
            m_scrollSelectedIntoView = serializedObject.FindProperty("m_scrollSelectedIntoView");
        }

        public override void OnInspectorGUI()
        {
            if (CannotModifyInPlayMode())
            {
                GUI.enabled = false;
            }

            UpdatePrefabModifiedProperties();

            var defaultLabelStyle = EditorStyles.label.fontStyle;

            EditorStyles.label.fontStyle = m_viewModelItemsPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent("View-model items property", "items property on the view-model to bind to."),
                FindBindableIEnumerableProperties(targetScript),
                updatedValue => targetScript.ViewModelItemsPropertyName = updatedValue,
                targetScript.ViewModelItemsPropertyName,
                property => true
            );

            EditorStyles.label.fontStyle = m_viewModelItemsAdapterPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            var viewModelAdapterTypeNames = GetAdapterTypeNames(
                type => TypeResolver.FindAdapterAttribute(type).InputType == typeof(IEnumerable)
            );

            EditorStyles.label.fontStyle = m_viewModelItemsAdapterPrefabModified
                ? FontStyle.Bold
                : defaultLabelStyle;

            ShowAdapterMenu(
                new GUIContent(
                    "items view-model adapter",
                    "Adapter that converts from the view back to the view-model"
                ),
                viewModelAdapterTypeNames,
                targetScript.ItemsUIToViewModelAdapter,
                newValue =>
                {
                    UpdateProperty(
                        updatedValue => targetScript.ItemsUIToViewModelAdapter = updatedValue,
                        targetScript.ItemsUIToViewModelAdapter,
                        newValue,
                        "Set items view-model adapter"
                    );
                }
            );

            EditorGUILayout.Space();

            EditorStyles.label.fontStyle = m_viewModelSelectedItemsPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent("View-model selected items property", "Selected items property on the view-model to bind to."),
                FindBindableIEnumerableProperties(targetScript),
                updatedValue => targetScript.ViewModelSelectedItemsPropertyName = updatedValue,
                targetScript.ViewModelSelectedItemsPropertyName,
                property => true
            );

            EditorStyles.label.fontStyle = m_viewModelSelectedItemsAdapterPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            viewModelAdapterTypeNames = GetAdapterTypeNames(
                type => TypeResolver.FindAdapterAttribute(type).InputType == typeof(IEnumerable)
            );

            EditorStyles.label.fontStyle = m_viewModelSelectedItemsAdapterPrefabModified
                ? FontStyle.Bold
                : defaultLabelStyle;

            ShowAdapterMenu(
                new GUIContent(
                    "Selected items view-model adapter",
                    "Adapter that converts from the view back to the view-model"
                ),
                viewModelAdapterTypeNames,
                targetScript.SelectedItemsUIToViewModelAdapter,
                newValue =>
                {
                    UpdateProperty(
                        updatedValue => targetScript.SelectedItemsUIToViewModelAdapter = updatedValue,
                        targetScript.SelectedItemsUIToViewModelAdapter,
                        newValue,
                        "Set selected items view-model adapter"
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

            EditorStyles.label.fontStyle = defaultLabelStyle;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_canRemoveProperty, new GUIContent("Can Remove"));
            EditorGUILayout.PropertyField(m_canSelectProperty, new GUIContent("Can Select"));
            EditorGUILayout.PropertyField(m_canReorderProperty, new GUIContent("Can Reorder"));
            EditorGUILayout.PropertyField(m_selectOnPointerUpProperty, new GUIContent("Select On Pointer Up"));
            EditorGUILayout.PropertyField(m_canUnselectAllProperty, new GUIContent("Can Unselect All"));
            EditorGUILayout.PropertyField(m_scrollSelectedIntoView, new GUIContent("Scroll Selected Into View"));
            serializedObject.ApplyModifiedProperties();

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
                    case "m_viewModelItemsPropertyName":
                        m_viewModelItemsPrefabModified = property.prefabOverride;
                        break;
                    case "m_itemsUIToViewModelAdapter":
                        m_viewModelItemsAdapterPrefabModified = property.prefabOverride;
                        break;
                    case "m_viewModelSelectedItemsPropertyName":
                        m_viewModelSelectedItemsPrefabModified = property.prefabOverride;
                        break;
                    case "m_selectedItemsUIToViewModelAdapter":
                        m_viewModelSelectedItemsAdapterPrefabModified = property.prefabOverride;
                        break;
                    case "m_viewModelTargetPropertyName":
                        m_viewModelTargetPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }

        public static BindableMember<PropertyInfo>[] FindBindableIEnumerableProperties(VirtualizingListBoxBinding target)
        {
            return TypeResolver.FindBindableProperties(target)
                .Where(p => typeof(IEnumerable).IsAssignableFrom(p.Member.PropertyType))
                .Where(p => !typeof(string).IsAssignableFrom(p.Member.PropertyType))
                .ToArray();
        }
    }
}
