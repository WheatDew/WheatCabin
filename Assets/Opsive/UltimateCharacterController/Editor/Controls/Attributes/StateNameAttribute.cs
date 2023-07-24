/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Attributes
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Attributes;
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Editor.Utility;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEditor;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Implements AttributeControlBase for the StateName attribute.
    /// </summary>
    [ControlType(typeof(StateNameAttribute))]
    public class StateNameAttributeControl : AttributeControlBase
    {
        /// <summary>
        /// Does the attribute override the type control?
        /// </summary>
        public override bool OverrideTypeControl { get => true; }

        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get => m_UseLabel; }

        private bool m_UseLabel;

        /// <summary>
        /// Returns the attribute control that should be used for the specified AttributeControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(AttributeControlInput input)
        {
            var fieldType = input.Field.FieldType;
            if (fieldType.IsAssignableFrom(typeof(IList<string>)) || (fieldType.IsArray && fieldType.GetElementType() == typeof(string))) {
                // The object is a list/array of state names
                m_UseLabel = false;
                
                var inputReadOnlyList = input.Value as IList<string>;
                List<string> list = null;
                if (inputReadOnlyList == null) {
                    list = new List<string>();
                } else {
                    list = new List<string>(inputReadOnlyList);
                }
                
                var reorderableList = new ReorderableList(
                    list,
                    (VisualElement container, int index) =>
                    {
                        var stateNameField = new StateNameField("", index);
                        stateNameField.OnChangeE += (newStateName, changeIndex) =>
                        {
                            list[changeIndex] = newStateName;
                            
                            if (fieldType.IsArray) {
                                input.OnChangeEvent(list.ToArray());
                            } else {
                                input.OnChangeEvent(list);
                            }
                        };
                        SetBindingUpdateEvent(stateNameField, input.Target, input.Field, index);
                        container.Add(stateNameField);

                    }, (VisualElement container, int index) =>
                    {
                        var stateNameField = container.Q<StateNameField>();

                        stateNameField.Refresh(list[index]);
                    }, (VisualElement container) =>
                    {
                        var titleLabel = ObjectNames.NicifyVariableName(input.Field.Name);
                        container.Add(new Label(titleLabel));
                    }, (int index) =>
                    {
                    }, () =>
                    {
                        list.Add("New State Name");

                        if (fieldType.IsArray) {
                            input.OnChangeEvent(list.ToArray());
                        } else {
                            input.OnChangeEvent(list);
                        }
                    }, (int index) =>
                    {
                        list.RemoveAt(index);
                        
                        if (fieldType.IsArray) {
                            input.OnChangeEvent(list.ToArray());
                        } else {
                            input.OnChangeEvent(list);
                        }
                    }, (int fromIndex, int toIndex) => { 
                        if (fieldType.IsArray) {
                            input.OnChangeEvent(list.ToArray());
                        } else {
                            input.OnChangeEvent(list);
                        }
                    });
                return reorderableList;
            } else if (fieldType.IsAssignableFrom(typeof(string))) {
                // The state name is a string.
                m_UseLabel = true;
                
                var stateName = input.Value as string;
                var stateNameField = new StateNameField(stateName);
                stateNameField.OnChangeE += (newStateName, changeIndex) => { input.OnChangeEvent(newStateName); };
                SetBindingUpdateEvent(stateNameField, input.Target, input.Field, input.ArrayIndex);

                return stateNameField;
            } else {
                return new Label("The Attribute [StateName] can only be used on strings or string list/arrays.");
            }
        }

        /// <summary>
        /// Set the Binding Update event for panel attach and detach.
        /// </summary>
        /// <param name="stateNameField">The state name field.</param>
        /// <param name="target">The target object.</param>
        /// <param name="field">The field info.</param>
        /// <param name="arrayIndex">The array index.</param>
        private void SetBindingUpdateEvent(StateNameField stateNameField, object target, FieldInfo field, int arrayIndex)
        {
            if (field != null) {
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    stateNameField.Refresh((string)newValue, false);
                };
                stateNameField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(field, arrayIndex, target, onBindingUpdateEvent);
                });
                stateNameField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
            }
        }
    }

    /// <summary>
    /// Implements a custom PropertyDrawer for the StateName attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(StateNameAttribute))]
    public class StateNameDrawer : PropertyDrawer
    {
        /// <summary>
        /// Creates a new VisualElements for the property field.
        /// </summary>
        /// <param name="property">The property that is being referenced.</param>
        /// <returns>The new VisualElements for the property field.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.String) {
                return new Label("The StateName attribute only works on strings.");
            }

            var stateNameAttribute = attribute as StateNameAttribute;
            var stateName = property.stringValue;

            var nameField = new TextField();
            nameField.name = "name-field";
            nameField.style.flexDirection = FlexDirection.Row;
            nameField.RegisterValueChangedCallback(c =>
            {
                property.stringValue = c.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            // The dropdown button overlaps the text within the textfield. Set the padding on the TextInput in order to
            // allow the far right text to be visible.
            foreach (var child in nameField.Children()) {
                child.style.paddingRight = 16;
                break;
            }

            var stateIDSearchButton = new Button();
            stateIDSearchButton.AddToClassList(DataMapInspector<string>.StyleClassName + "_button");
            stateIDSearchButton.text = "▼";
            stateIDSearchButton.clicked += () =>
            {
                StateNamesSearchableWindow.OpenWindow("States", nameField.value, (newValue) =>
                {
                    property.stringValue = (string)newValue;
                    property.serializedObject.ApplyModifiedProperties();
                }, true);
            };
            nameField.Add(stateIDSearchButton);

            return nameField;
        }

        /// <summary>
        ///  Override this method to make your own IMGUI based GUI for the property.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label.
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't allow child fields to be indented.
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects.
            var button = new Rect(position.x + position.width -20, position.y, 20, position.height);
            var textFieldRect = new Rect(position.x, position.y, position.width - 20, position.height);
            
            EditorGUI.PropertyField(textFieldRect, property, GUIContent.none);

            if (EditorGUI.LinkButton(button, "▼")) {
                StateNamesSearchableWindow.OpenWindow("States", property.stringValue, (newValue) =>
                {
                    property.stringValue = (string)newValue;
                    property.serializedObject.ApplyModifiedProperties();
                }, true);
            }

            // Set indent back to what it was.
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    /// <summary>
    /// The field for a state Name text field with a button to open the StateNamesSearchableWindow.
    /// </summary>
    public class StateNameField : VisualElement
    {
        public Action<string, int> OnChangeE;

        private int m_Index;
        private TextField m_NameField;
        private Button m_StateIDSearchButton;

        public string StateName => m_NameField.value;
        public int Index { get => m_Index; set => m_Index = value; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stateName">The starting state name.</param>
        /// <param name="index">The index of this field if used in a list.</param>
        public StateNameField(string stateName = "", int index = -1)
        {
            m_Index = index;
            style.height = 18;

            m_NameField = new TextField();
            m_NameField.name = "name-field";
            m_NameField.style.flexDirection = FlexDirection.Row;
            m_NameField.style.marginLeft = 0;
            m_NameField.value = stateName;
            m_NameField.RegisterValueChangedCallback(c => { OnChangeE?.Invoke(c.newValue, m_Index); });

            m_StateIDSearchButton = new Button();
            m_StateIDSearchButton.AddToClassList(DataMapInspector<string>.StyleClassName + "_button");
            m_StateIDSearchButton.text = "▼";
            m_StateIDSearchButton.clicked += () =>
            {
                StateNamesSearchableWindow.OpenWindow("States", m_NameField.value,
                    (newValue) => { OnChangeE?.Invoke(newValue, m_Index); }, true);
            };
            m_NameField.Add(m_StateIDSearchButton);
            
            Add(m_NameField);
        }

        /// <summary>
        /// Refresh the state name.
        /// </summary>
        /// <param name="stateName">The new state name.</param>
        /// <param name="notify">Notify that the state name has changed?</param>
        public void Refresh(string stateName, bool notify = false)
        {
            if (notify) {
                m_NameField.value =stateName;
            } else {
                m_NameField.SetValueWithoutNotify(stateName);
            }
            
        }
    }
}