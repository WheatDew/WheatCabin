using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityWeld.Binding.Internal;
using UnityWeld_Editor;
using static Battlehub.UIControls.Binding.ControlBinding;

namespace Battlehub.UIControls.Binding
{

    [CustomEditor(typeof(ControlBinding), true)]
    public class ControlBindingEditor : BaseBindingEditor
    {
        private const float k_Width = 0.7f;

        private ControlBinding targetControlBinding;
        private ReorderableListEditor m_eventBindingsEditor = new ReorderableListEditor();
        private ReorderableListEditor m_oneWayPropertyBindingsEditor = new ReorderableListEditor();
        private ReorderableListEditor m_twoWayPropertyBindingsEditor = new ReorderableListEditor();

        #region ReorderableListEditor
        private class ReorderableListEditor
        {
            private SerializedProperty m_property;
            private ReorderableList m_reordableList;

            private SerializedObject m_serializedObject;
            private string m_headerName;
            private Action<Rect, SerializedProperty> m_drawElementCallback;
            private Func<int, float> m_elementHeightCallback;

            public void OnEnable(SerializedObject so, string propertyName, string headerName,
                Action<Rect, SerializedProperty> drawElementCallback,
                Func<int, float> elementHeightCallback)
            {
                m_serializedObject = so;
                m_headerName = headerName;
                m_drawElementCallback = drawElementCallback;
                m_elementHeightCallback = elementHeightCallback;

                m_property = m_serializedObject.FindProperty(propertyName);
                m_reordableList = new ReorderableList(serializedObject: m_serializedObject, elements: m_property, draggable: true, displayHeader: true,
                    displayAddButton: true, displayRemoveButton: true);

                m_reordableList.drawHeaderCallback = DrawHeaderCallback;
                m_reordableList.drawElementCallback = DrawElementCallback;
                m_reordableList.elementHeightCallback += ElementHeightCallback;
                m_reordableList.onAddCallback += OnAddCallback;
            }

            public void OnDisable()
            {
                m_reordableList.elementHeightCallback -= ElementHeightCallback;
                m_reordableList.onAddCallback -= OnAddCallback;

                m_serializedObject = null;
                m_headerName = null;
                m_drawElementCallback = null;
                m_elementHeightCallback = null;
            }


            public void OnInspectorGUI()
            {
                m_serializedObject.Update();

                m_reordableList.DoLayoutList();

                m_serializedObject.ApplyModifiedProperties();
            }

            private void DrawHeaderCallback(Rect rect)
            {
                EditorGUI.LabelField(rect, m_headerName);
            }

            private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
            {
                SerializedProperty element = m_reordableList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 4;
                
                m_drawElementCallback(rect, element);
            }

            private float ElementHeightCallback(int index)
            {
                if (m_elementHeightCallback != null)
                {
                    return m_elementHeightCallback(index);
                }

                float propertyHeight = EditorGUIUtility.singleLineHeight * 2;
                float spacing = EditorGUIUtility.singleLineHeight / 2.0f;
                return propertyHeight + spacing;
            }

            private void OnAddCallback(ReorderableList list)
            {
                var index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;
            }
        }
        #endregion

        protected virtual void OnEnable()
        {
            // Initialise everything
            targetControlBinding = (ControlBinding)target;

            m_eventBindingsEditor.OnEnable(serializedObject, "m_eventBindings", "Events", DrawEvent, null);

            m_oneWayPropertyBindingsEditor.OnEnable(serializedObject, "m_oneWayPropertyBindings", "One Way Property Bindings", DrawOneWayPropertyBinding, GetOneWayPropertyBindingHeight);

            m_twoWayPropertyBindingsEditor.OnEnable(serializedObject, "m_twoWayPropertyBindings", "Two Way Property Bindings", DrawTwoWayPropertyBinding, GetTwoWayPropertyBindingHeight);
        }

        protected virtual void OnDisable()
        {
            m_eventBindingsEditor.OnDisable();
            m_oneWayPropertyBindingsEditor.OnDisable();
            m_twoWayPropertyBindingsEditor.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            m_eventBindingsEditor.OnInspectorGUI();
            m_oneWayPropertyBindingsEditor.OnInspectorGUI();
            m_twoWayPropertyBindingsEditor.OnInspectorGUI();
        }

        private void DrawTwoWayPropertyBinding(Rect rect, SerializedProperty element)
        {
            SerializedProperty elementName = element.FindPropertyRelative("m_viewPropertyName");
            string elementTitle = string.IsNullOrEmpty(elementName.stringValue) ? "New One Way Property Binding" : elementName.stringValue;

            TwoWayPropertyBindingSlim twoWayBinding = element.GetSerializedValue<TwoWayPropertyBindingSlim>();

            ShowEventMenu(
                new Rect(rect.x += 10, rect.y, Screen.width * k_Width, height: EditorGUIUtility.singleLineHeight),
                GetBindableEvents()
                    .OrderBy(evt => evt.Name)
                    .ToArray(),
                updatedValue => twoWayBinding.ViewEventName = updatedValue,
                twoWayBinding.ViewEventName
            );

            Type viewPropertyType;
            ShowViewPropertyMenu(
                new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 1, Screen.width * k_Width, height: EditorGUIUtility.singleLineHeight),
                new GUIContent("View property", "Property on the view to bind to"),
                PropertyFinder.GetBindableProperties(targetControlBinding.gameObject)
                    .OrderBy(prop => prop.ViewModelTypeName)
                    .ThenBy(prop => prop.MemberName)
                    .ToArray(),
                updatedValue => twoWayBinding.ViewPropertyName = updatedValue,
                twoWayBinding.ViewPropertyName,
                out viewPropertyType
            );

            var guiPreviouslyEnabled = GUI.enabled;
            if (string.IsNullOrEmpty(twoWayBinding.ViewPropertyName))
            {
                GUI.enabled = false;
            }

            var viewAdapterTypeNames = GetAdapterTypeNames(
                type => viewPropertyType == null ||
                TypeResolver.FindAdapterAttribute(type).OutputType == viewPropertyType
            );

            ShowAdapterMenu(
                 new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 2 + 1, Screen.width * k_Width, height: EditorGUIUtility.singleLineHeight),
                 new GUIContent(
                     "View adapter",
                     "Adapter that converts values sent from the view-model to the view."
                 ),
                 viewAdapterTypeNames,
                 twoWayBinding.ViewAdapterTypeName,
                 newValue =>
                 {
                     // Get rid of old adapter options if we changed the type of the adapter.
                     if (newValue != twoWayBinding.ViewAdapterTypeName)
                     {
                         Undo.RecordObject(targetControlBinding, "Set view adapter options");
                         twoWayBinding.ViewAdapterOptions = null;
                     }

                     UpdateProperty(
                         updatedValue => twoWayBinding.ViewAdapterTypeName = updatedValue,
                         twoWayBinding.ViewAdapterTypeName,
                         newValue,
                         "Set view adapter"
                     );
                 }
             );

            var adaptedViewPropertyType = AdaptTypeBackward(
                viewPropertyType,
                twoWayBinding.ViewAdapterTypeName
            );

            ShowViewModelPropertyMenuWithNone(
                new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 3 + 1, Screen.width * k_Width, height: EditorGUIUtility.singleLineHeight),
                new GUIContent(
                    "View-model property",
                    "Property on the view-model to bind to."
                ),
                TypeResolver.FindBindableProperties(targetControlBinding),
                updatedValue => twoWayBinding.ViewModelPropertyName = updatedValue,
                twoWayBinding.ViewModelPropertyName,
                property => property.PropertyType == adaptedViewPropertyType
            );

            var viewModelAdapterTypeNames = GetAdapterTypeNames(
                type => adaptedViewPropertyType == null ||
                    TypeResolver.FindAdapterAttribute(type).OutputType == adaptedViewPropertyType
            );

            ShowAdapterMenu(
                new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 4 + 1, Screen.width * k_Width, height: EditorGUIUtility.singleLineHeight),
                new GUIContent(
                   "View-model adapter",
                   "Adapter that converts from the view back to the view-model"
                ),
                viewModelAdapterTypeNames,
                twoWayBinding.ViewModelAdapterTypeName,
                newValue =>
                {
                    if (newValue != twoWayBinding.ViewModelAdapterTypeName)
                    {
                        Undo.RecordObject(targetControlBinding, "Set view-model adapter options");
                        twoWayBinding.ViewModelAdapterOptions = null;
                    }

                    UpdateProperty(
                        updatedValue => twoWayBinding.ViewModelAdapterTypeName = updatedValue,
                        twoWayBinding.ViewModelAdapterTypeName,
                        newValue,
                        "Set view-model adapter"
                    );
                }
           );

            GUI.enabled = guiPreviouslyEnabled;
        }

        private float GetTwoWayPropertyBindingHeight(int index)
        {
            float propertyHeight = EditorGUIUtility.singleLineHeight * 5;
            float spacing = EditorGUIUtility.singleLineHeight / 2.0f;
            return propertyHeight + spacing;
        }


        private void DrawOneWayPropertyBinding(Rect rect, SerializedProperty element)
        {
            SerializedProperty elementName = element.FindPropertyRelative("m_viewPropertyName");
            string elementTitle = string.IsNullOrEmpty(elementName.stringValue) ? "New One Way Property Binding" : elementName.stringValue;

            OneWayPropertyBindingSlim onewWayBinding = element.GetSerializedValue<OneWayPropertyBindingSlim>();

            Type viewPropertyType;
            ShowViewPropertyMenu(
                new Rect(rect.x, rect.y, Screen.width * k_Width, height: EditorGUIUtility.singleLineHeight),
                new GUIContent("View property", "Property on the view to bind to"),
                PropertyFinder.GetBindableProperties(targetControlBinding.gameObject)
                    .OrderBy(prop => prop.ViewModelTypeName)
                    .ThenBy(prop => prop.MemberName)
                    .ToArray(),
                updatedValue => onewWayBinding.ViewPropertyName = updatedValue,
                onewWayBinding.ViewPropertyName,
                out viewPropertyType
            );

            var guiPreviouslyEnabled = GUI.enabled;
            if (string.IsNullOrEmpty(onewWayBinding.ViewPropertyName))
            {
                GUI.enabled = false;
            }

            var viewAdapterTypeNames = GetAdapterTypeNames(
                type => viewPropertyType == null ||
                TypeResolver.FindAdapterAttribute(type).OutputType == viewPropertyType
            );

            ShowAdapterMenu(
                 new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 1, Screen.width * k_Width, height: EditorGUIUtility.singleLineHeight),
                 new GUIContent(
                     "View adapter",
                     "Adapter that converts values sent from the view-model to the view."
                 ),
                 viewAdapterTypeNames,
                 onewWayBinding.ViewAdapterTypeName,
                 newValue =>
                 {
                        // Get rid of old adapter options if we changed the type of the adapter.
                     if (newValue != onewWayBinding.ViewAdapterTypeName)
                     {
                         Undo.RecordObject(targetControlBinding, "Set view adapter options");
                         onewWayBinding.ViewAdapterOptions = null;
                     }

                     UpdateProperty(
                         updatedValue => onewWayBinding.ViewAdapterTypeName = updatedValue,
                         onewWayBinding.ViewAdapterTypeName,
                         newValue,
                         "Set view adapter"
                     );
                 }
             );

            var adaptedViewPropertyType = AdaptTypeBackward(
                viewPropertyType,
                onewWayBinding.ViewAdapterTypeName
            );

            ShowViewModelPropertyMenuWithNone(
                new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 2 + 1, Screen.width * k_Width, height: EditorGUIUtility.singleLineHeight),
                new GUIContent(
                    "View-model property",
                    "Property on the view-model to bind to."
                ),
                TypeResolver.FindBindableProperties(targetControlBinding),
                updatedValue => onewWayBinding.ViewModelPropertyName = updatedValue,
                onewWayBinding.ViewModelPropertyName,
                property => property.PropertyType == adaptedViewPropertyType
            );

            GUI.enabled = guiPreviouslyEnabled;
        }

        private float GetOneWayPropertyBindingHeight(int index)
        {
            float propertyHeight = EditorGUIUtility.singleLineHeight * 3;
            float spacing = EditorGUIUtility.singleLineHeight / 2.0f;
            return propertyHeight + spacing;
        }


        protected virtual IEnumerable<BindableEvent> GetBindableEvents()
        {
            return TypeResolverEx.GetBindableEvents(targetControlBinding.TargetControl);
        }

        private void DrawEvent(Rect rect, SerializedProperty element)
        {
            SerializedProperty elementName = element.FindPropertyRelative("viewEventName");
            string elementTitle = string.IsNullOrEmpty(elementName.stringValue) ? "New Event" : elementName.stringValue;

            EventBindingSlim eventBinding = element.GetSerializedValue<EventBindingSlim>();

            ShowEventMenu(
                new Rect(rect.x += 10, rect.y, Screen.width * k_Width, height: EditorGUIUtility.singleLineHeight),
                GetBindableEvents()
                    .OrderBy(evt => evt.Name)
                    .ToArray(),
                updatedValue => eventBinding.ViewEventName = updatedValue,
                eventBinding.ViewEventName
            );

            ShowMethodMenu(
                new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 1, Screen.width * k_Width, height: EditorGUIUtility.singleLineHeight),
                eventBinding, TypeResolverEx.FindBindableMethods(targetControlBinding));
        }

        private void ShowEventMenu(
            Rect position,
            BindableEvent[] events,
            Action<string> propertyValueSetter,
            string curPropertyValue
            )
        {
            var eventNames = events
                .Select(TypeResolverEx.BindableEventToString)
                .ToArray();
            var selectedIndex = Array.IndexOf(eventNames, curPropertyValue);
            var content = events
                .Select(evt => new GUIContent(evt.Name))
                .ToArray();

            var newSelectedIndex = EditorGUI.Popup(position,
                new GUIContent("View event", "Event on the view to bind to."),
                selectedIndex,
                content
            );

            if (newSelectedIndex == selectedIndex)
            {
                return;
            }

            var selectedEvent = events[newSelectedIndex];
            UpdateProperty(
                propertyValueSetter,
                curPropertyValue,
                TypeResolverEx.BindableEventToString(selectedEvent),
                "Set bound event"
            );
        }

        private void ShowMethodMenu(
            Rect position,
            EventBindingSlim targetScript,
            BindableMember<MethodInfo>[] bindableMethods
        )
        {
            var tooltip = "Method on the view-model to bind to.";

            InspectorUtils.DoPopup(
                position,
                new GUIContent(targetScript.ViewModelMethodName),
                new GUIContent("View-model method", tooltip),
                m => m.ViewModelType + "/" + m.MemberName,
                m => true,
                m => m.ToString() == targetScript.ViewModelMethodName,
                m => UpdateProperty(
                    updatedValue => targetScript.ViewModelMethodName = updatedValue,
                    targetScript.ViewModelMethodName,
                    m.ToString(),
                    "Set bound view-model method"
                ),
                bindableMethods
                    .OrderBy(m => m.ViewModelTypeName)
                    .ThenBy(m => m.MemberName)
                    .ToArray()
            );
        }

        protected void ShowViewPropertyMenu(
            Rect position,
            GUIContent label,
            BindableMember<PropertyInfo>[] properties,
            Action<string> propertyValueSetter,
            string curPropertyValue,
             out Type selectedPropertyType
      )
        {
            var propertyNames = properties
                .Select(m => m.ToString())
                .ToArray();
            var selectedIndex = Array.IndexOf(propertyNames, curPropertyValue);
            var content = properties.Select(prop => new GUIContent(string.Concat(
                    prop.ViewModelTypeName,
                    "/",
                    prop.MemberName,
                    " : ",
                    prop.Member.PropertyType.Name
                )))
                .ToArray();

            var newSelectedIndex = EditorGUI.Popup(position, label, selectedIndex, content);
            if (newSelectedIndex != selectedIndex)
            {
                var newSelectedProperty = properties[newSelectedIndex];

                UpdateProperty(
                    propertyValueSetter,
                    curPropertyValue,
                    newSelectedProperty.ToString(),
                    "Set view property"
                );

                selectedPropertyType = newSelectedProperty.Member.PropertyType;
            }
            else
            {
                if (selectedIndex < 0)
                {
                    selectedPropertyType = null;
                    return;
                }

                selectedPropertyType = properties[selectedIndex].Member.PropertyType;
            }
        }

        private class OptionInfo
        {
            public OptionInfo(string menuName, BindableMember<PropertyInfo> property)
            {
                this.MenuName = menuName;
                this.Property = property;
            }

            public string MenuName { get; private set; }

            public BindableMember<PropertyInfo> Property { get; private set; }
        }


        /// <summary>
        /// The string used to show that no option is selected in the property menu.
        /// </summary>
        private static readonly string NoneOptionString = "None";

        /// <summary>
        /// Display a popup menu for selecting a property from a view-model.
        /// </summary>
        protected new void ShowViewModelPropertyMenuWithNone(
            GUIContent label,
            BindableMember<PropertyInfo>[] bindableProperties,
            Action<string> propertyValueSetter,
            string curPropertyValue,
            Func<PropertyInfo, bool> menuEnabled
        )
        {
            var labelRect = EditorGUILayout.GetControlRect(false, 16f, EditorStyles.popup);

            ShowViewModelPropertyMenuWithNone(labelRect, label, bindableProperties, propertyValueSetter, curPropertyValue, menuEnabled);
        }

        protected void ShowViewModelPropertyMenuWithNone(Rect labelRect, GUIContent label, BindableMember<PropertyInfo>[] bindableProperties, Action<string> propertyValueSetter, string curPropertyValue, Func<PropertyInfo, bool> menuEnabled)
        {
            var options = bindableProperties
                .Select(prop => new OptionInfo(
                    string.Concat(prop.ViewModelType, "/", prop.MemberName, " : ", prop.Member.PropertyType.Name),
                    prop
                ))
                .OrderBy(option => option.Property.ViewModelTypeName)
                .ThenBy(option => option.Property.MemberName);

            var noneOption = new OptionInfo(NoneOptionString, null);

            InspectorUtils.DoPopup(
                labelRect,
                new GUIContent(string.IsNullOrEmpty(curPropertyValue) ? NoneOptionString : curPropertyValue),
                label,
                option => option.MenuName,
                option => option.MenuName == NoneOptionString ? true : menuEnabled(option.Property.Member),
                option =>
                {
                    if (option == noneOption)
                    {
                        return string.IsNullOrEmpty(curPropertyValue);
                    }

                    return option.Property.ToString() == curPropertyValue;
                },
                option => UpdateProperty(
                    propertyValueSetter,
                    curPropertyValue,
                    option.Property == null ? string.Empty : option.Property.ToString(),
                    "Set view-model property"
                ),
                new[] { noneOption }
                    .Concat(options)
                    .ToArray()
            );
        }

        protected static void ShowAdapterMenu(
            Rect position,
            GUIContent label,
            string[] adapterTypeNames,
            string curValue,
            Action<string> valueUpdated
        )
        {
            var adapterMenu = new[] { "None" }
                .Concat(adapterTypeNames)
                .Select(typeName => new GUIContent(typeName))
                .ToArray();

            var curSelectionIndex = Array.IndexOf(adapterTypeNames, curValue) + 1; // +1 to account for 'None'.
            var newSelectionIndex = EditorGUI.Popup(
                    position,
                    label,
                    curSelectionIndex,
                    adapterMenu
                );

            if (newSelectionIndex == curSelectionIndex)
            {
                return;
            }

            if (newSelectionIndex == 0)
            {
                valueUpdated(null); // No adapter selected.
            }
            else
            {
                valueUpdated(adapterTypeNames[newSelectionIndex - 1]); // -1 to account for 'None'.
            }
        }
    }
}
