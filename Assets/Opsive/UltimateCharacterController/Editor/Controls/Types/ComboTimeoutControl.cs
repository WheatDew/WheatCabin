/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.UltimateCharacterController.Character.Abilities.Starters;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the ComboTimeout ControlType.
    /// </summary>
    [ControlType(typeof(ComboTimeout))]
    public class ComboTimeoutControl : TypeControlBase
    {
        /// <summary>                                                                                                                                                                                                                                                                                                                                
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get { return false; } }

        /// <summary>
        /// Returns the header control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(TypeControlInput input)
        {
            var comboTimeout = input.Value as ComboTimeout;
            if (comboTimeout == null) {
                return null;
            }

            var container = new VisualElement();
            // Draw a custom array inspector for the input names.
            var elements = comboTimeout.ComboInputElements;
            if (elements == null || elements.Length == 0) {
                comboTimeout.ComboInputElements = new ComboTimeout.ComboInputElement[1];
            }
            ShowElements(input.Target, input.Field, comboTimeout, container, input.OnChangeEvent);

            return container;
        }

        /// <summary>
        /// Shows a row for each combo element.
        /// </summary>
        /// <param name="target">The object that the field belongs to.</param>
        /// <param name="field">The field being retrieved.</param>
        /// <param name="elements">The value of the combo elements.</param>
        /// <param name="container">The UIElement that the textfields should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        private void ShowElements(object target, FieldInfo field, ComboTimeout comboTimeout, VisualElement container, Func<object, bool> onChangeEvent)
        {
            container.Clear();

            for (int i = 0; i < comboTimeout.ComboInputElements.Length; ++i) {
                var element = comboTimeout.ComboInputElements[i];
                var index = i;

                // Initialization.
                var horizontalLayout = new VisualElement();
                horizontalLayout.AddToClassList("horizontal-layout");
                var labelControl = new LabelControl("  Combo " + (i + 1), string.Empty, horizontalLayout);
                container.Add(labelControl);
                var inputNameField = new TextField();
                var startTypeField = new DropdownField(new List<string>(new string[] { "Button Down", "Axis" }), element.AxisInput ? 1 : 0);
                var timeoutField = new FloatField();

                // Input name.
                inputNameField.style.marginLeft = 0;
                inputNameField.SetValueWithoutNotify(element.InputName);
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    var newComboValue = (ComboTimeout.ComboInputElement)newValue;
                    inputNameField.SetValueWithoutNotify(newComboValue.InputName);
                    startTypeField.index = element.AxisInput ? 1 : 0;
                    timeoutField.SetValueWithoutNotify(newComboValue.Timeout);
                };
                inputNameField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(field, i, target, onBindingUpdateEvent);
                });
                inputNameField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                inputNameField.RegisterValueChangedCallback(c =>
                {
                    element.InputName = c.newValue;
                    comboTimeout.ComboInputElements[index] = element;
                    onChangeEvent(comboTimeout);
                    c.StopPropagation();
                });
                inputNameField.AddToClassList("flex-grow");
                horizontalLayout.Add(inputNameField);

                // Start type.
                startTypeField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(field, i, target, onBindingUpdateEvent);
                });
                startTypeField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                startTypeField.RegisterValueChangedCallback(c =>
                {
                    element.AxisInput = startTypeField.index == 1;
                    onChangeEvent(comboTimeout);
                    c.StopPropagation();
                });
                startTypeField.style.width = 100;
                horizontalLayout.Add(startTypeField);

                // Timeout.
                timeoutField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(field, i, target, onBindingUpdateEvent);
                });
                timeoutField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                timeoutField.RegisterValueChangedCallback(c =>
                {
                    element.Timeout = c.newValue;
                    onChangeEvent(comboTimeout);
                    c.StopPropagation();
                });
                horizontalLayout.Add(timeoutField);

                // Remove.
                if (comboTimeout.ComboInputElements.Length > 1) {
                    var removeButton = new Button();
                    removeButton.name = "remove-button";
                    removeButton.text = "-";
                    removeButton.clicked += () =>
                    {
                        var elements = comboTimeout.ComboInputElements;
                        UnityEditor.ArrayUtility.RemoveAt(ref elements, index);
                        comboTimeout.ComboInputElements = elements;
                        onChangeEvent(comboTimeout);
                        ShowElements(target, field, comboTimeout, container, onChangeEvent);
                    };
                    horizontalLayout.Add(removeButton);
                }

                // Add.
                if (i == comboTimeout.ComboInputElements.Length - 1) {
                    var addButton = new Button();
                    addButton.name = "add-button";
                    addButton.text = "+";
                    addButton.clicked += () =>
                    {
                        var elements = comboTimeout.ComboInputElements;
                        Array.Resize(ref elements, elements.Length + 1);
                        comboTimeout.ComboInputElements = elements;
                        onChangeEvent(comboTimeout);
                        ShowElements(target, field, comboTimeout, container, onChangeEvent);
                    };
                    horizontalLayout.Add(addButton);
                    if (comboTimeout.ComboInputElements.Length > 1) {
                        timeoutField.style.width = 35;
                    } else {
                        timeoutField.style.width = 62;
                    }
                } else {
                    timeoutField.style.width = 62;
                }
            }
        }
    }
}