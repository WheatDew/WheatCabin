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
    using Opsive.Shared.Editor.Utility;
    using Opsive.UltimateCharacterController.Traits;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the AttributeModifier ControlType.
    /// </summary>
    [ControlType(typeof(AttributeModifier))]
    public class AttributeModifierControl : TypeControlBase
    {
        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get { return false; } }

        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(TypeControlInput input)
        {
            var container = new VisualElement();
            var attributeContainer = new VisualElement();
            attributeContainer.AddToClassList("indent");
            var attributeModifier = input.Value as AttributeModifier;
            AttributeManager attributeManager = null;
            if (input.UnityObject is Component) {
                var unityComponent = input.UnityObject as Component;
                attributeManager = unityComponent.GetComponent<AttributeManager>();
            }
            var label = ObjectNames.NicifyVariableName(input.Field.Name);
            if (attributeManager != null) {
                var attributeNames = new List<string>();
                attributeNames.Add("(None)");
                var attributeIndex = 0;
                for (int i = 0; i < attributeManager.Attributes.Length; ++i) {
                    attributeNames.Add(attributeManager.Attributes[i].Name);
                    if (attributeModifier.AttributeName == attributeNames[i + 1]) {
                        attributeIndex = i + 1;
                    }
                }
                var dropdownField = new DropdownField(attributeNames, attributeIndex);
                var labelControl = new LabelControl(label, Shared.Editor.Utility.EditorUtility.GetTooltip(input.Field), dropdownField, true);
                container.Add(labelControl);
                dropdownField.tooltip = Shared.Editor.Utility.EditorUtility.GetTooltip(input.Field);
                System.Action<object> onBindingUpdateEvent = (object newValue) => dropdownField.SetValueWithoutNotify(newValue as string);
                dropdownField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(input.Field, -1, input.Target, onBindingUpdateEvent);
                });
                dropdownField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                dropdownField.RegisterValueChangedCallback(c =>
                {
                    // Clear out the old.
                    dropdownField.SetValueWithoutNotify(c.newValue);
                    attributeModifier.AttributeName = c.newValue;
                    c.StopPropagation();
                    ShowAttribute(input.UnityObject, input.Target, input.Field, attributeModifier, attributeContainer, input.OnChangeEvent);
                    input.OnChangeEvent(input.Value);
                });
            } else {
                var textField = new TextField();
                var labelControl = new LabelControl(label, Shared.Editor.Utility.EditorUtility.GetTooltip(input.Field), textField, true);
                container.Add(labelControl);
                textField.tooltip = Shared.Editor.Utility.EditorUtility.GetTooltip(input.Field);
                textField.SetValueWithoutNotify(attributeModifier.AttributeName);
                System.Action<object> onBindingUpdateEvent = (object newValue) => textField.SetValueWithoutNotify(newValue as string);
                textField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(input.Field, -1, input.Target, onBindingUpdateEvent);
                });
                textField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                textField.RegisterValueChangedCallback(c =>
                {
                    textField.SetValueWithoutNotify(c.newValue);
                    attributeModifier.AttributeName = c.newValue;
                    ShowAttribute(input.UnityObject, input.Target, input.Field, attributeModifier, attributeContainer, input.OnChangeEvent);
                    input.OnChangeEvent(input.Value);
                    c.StopPropagation();
                });
            }
            container.Add(attributeContainer);
            ShowAttribute(input.UnityObject, input.Target, input.Field, attributeModifier, attributeContainer, input.OnChangeEvent);
            return container;
        }

        /// <summary>
        /// Shows the attribute fields.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that the field belongs to.</param>
        /// <param name="field">The field being retrieved.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="container">The UIElement that the textfields should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        private void ShowAttribute(UnityEngine.Object unityObject, object target, System.Reflection.FieldInfo field, AttributeModifier value, VisualElement container, System.Func<object, bool> onChangeEvent)
        {
            container.Clear();
            if (string.IsNullOrEmpty(value.AttributeName) || value.AttributeName == "(None)") {
                return;
            }

            var autoUpdateContainer = new VisualElement();
            autoUpdateContainer.AddToClassList("indent");
            FieldInspectorView.AddField(unityObject, value, "m_Amount", container, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, value, "m_AutoUpdate", container, (o) => { 
                ShowAutoUpdate(unityObject, target, field, value, autoUpdateContainer, onChangeEvent);
                onChangeEvent(o); 
            });
            container.Add(autoUpdateContainer);
            ShowAutoUpdate(unityObject, target, field, value, autoUpdateContainer, onChangeEvent);
        }

        /// <summary>
        /// Shows the Auto Update attribute fields.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that the field belongs to.</param>
        /// <param name="field">The field being retrieved.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="container">The UIElement that the textfields should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        private void ShowAutoUpdate(UnityEngine.Object unityObject, object target, System.Reflection.FieldInfo field, AttributeModifier value, VisualElement container, System.Func<object, bool> onChangeEvent)
        {
            container.Clear();
            if (!value.AutoUpdate) {
                return;
            }

            FieldInspectorView.AddField(unityObject, value, "m_AutoUpdateStartDelay", container, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, value, "m_AutoUpdateInterval", container, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, value, "m_AutoUpdateDuration", container, (o) => { onChangeEvent(o); });
        }
    }
    
    [CustomPropertyDrawer(typeof(AttributeModifier), true)]
    public class AttributeModifierDrawer : PropertyDrawer
    {
        /// <summary>
        /// Override this method to make your own UIElements based GUI for the property.
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>The element containing the custom GUI.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var targetObject = property.serializedObject.targetObject;
            var targetGameObject = (targetObject as Component)?.gameObject;
            var field = property.GetFieldInfo();
            var attributeModifier = (AttributeModifier) ControlUtility.GetTargetObjectOfProperty(property);
            
            var attributeModifierControl = new AttributeModifierControl();
            var control = attributeModifierControl.GetTypeControl(new TypeControlBase.TypeControlInput {
                UnityObject = targetGameObject,
                Target = targetObject,
                Field = field,
                SerializedProperty = property,
                ArrayIndex = -1,
                Type = attributeModifier?.GetType() ?? typeof(AttributeModifier),
                Value = attributeModifier,
                OnChangeEvent = (newValue) =>
                {
                    property.serializedObject.ApplyModifiedProperties();
                    return true;
                }
            });
            
            return control;
        }
    }
}