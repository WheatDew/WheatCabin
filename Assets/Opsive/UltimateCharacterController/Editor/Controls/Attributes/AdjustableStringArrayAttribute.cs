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
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using System;
    using System.Reflection;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Implements AttributeControlBase for the AdjustableStringArray attribute.
    /// </summary>
    [ControlType(typeof(Opsive.UltimateCharacterController.Utility.AdjustableStringArrayAttribute))]
    public class AdjustableStringArrayAttributeControl : AttributeControlBase
    {
        /// <summary>
        /// Does the attribute override the type control?
        /// </summary>
        public override bool OverrideTypeControl { get { return true; } }

        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get { return false; } }

        /// <summary>
        /// Returns the attribute control that should be used for the specified AttributeControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(AttributeControlInput input)
        {
            var container = new VisualElement();
            var stringArray = input.Value as string[];
            if (stringArray == null || stringArray.Length == 0) {
                stringArray = new string[1];
            }
            ShowStringElements(input.Target, input.Field, stringArray, container, input.OnChangeEvent);
            return container;
        }

        /// <summary>
        /// Shows a textfield for each string element.
        /// </summary>
        /// <param name="target">The object that the field belongs to.</param>
        /// <param name="field">The field being retrieved.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="container">The UIElement that the textfields should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        private void ShowStringElements(object target, FieldInfo field, string[] value, VisualElement container, Func<object, bool> onChangeEvent)
        {
            for (int i = 0; i < value.Length; ++i) {
                var index = i;
                var fieldContainer = new VisualElement();
                var fieldName = " ";
                if (i == 0) {
                    fieldName = UnityEditor.ObjectNames.NicifyVariableName(field.Name);
                }
                var labelControl = new LabelControl(fieldName, InspectorUtility.GetFieldTooltip(field), fieldContainer, true);
                var textField = new TextField();
                textField.style.marginLeft = 0;
                textField.SetValueWithoutNotify(value[i]);
                System.Action<object> onBindingUpdateEvent = (object newValue) => textField.SetValueWithoutNotify(newValue as string);
                textField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(field, i, target, onBindingUpdateEvent);
                });
                textField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                textField.RegisterValueChangedCallback(c =>
                {
                    value[index] = c.newValue;
                    onChangeEvent(value);
                    c.StopPropagation();
                });
                textField.AddToClassList("flex-grow");
                fieldContainer.AddToClassList("horizontal-layout");
                fieldContainer.Add(textField);

                if (value.Length > 1) {
                    var removeButton = new Button();
                    removeButton.name = "remove-button";
                    removeButton.text = "-";
                    removeButton.clicked += () =>
                    {
                        UnityEditor.ArrayUtility.RemoveAt(ref value, index);
                        onChangeEvent(value);
                        container.Clear();
                        ShowStringElements(target, field, value, container, onChangeEvent);
                    };
                    fieldContainer.Add(removeButton);
                }

                if (i == value.Length - 1) {
                    var addButton = new Button();
                    addButton.name = "add-button";
                    addButton.text = "+";
                    addButton.clicked += () =>
                    {
                        Array.Resize(ref value, value.Length + 1);
                        onChangeEvent(value);
                        container.Clear();
                        ShowStringElements(target, field, value, container, onChangeEvent);
                    };
                    fieldContainer.Add(addButton);
                }
                container.Add(labelControl);
            }
        }
    }
}