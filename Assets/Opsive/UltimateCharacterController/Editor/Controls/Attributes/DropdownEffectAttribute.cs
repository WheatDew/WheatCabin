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
    using Opsive.UltimateCharacterController.Character.Effects;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Implements AttributeControlBase for the DropdownEffect attribute.
    /// </summary>
    [ControlType(typeof(Opsive.UltimateCharacterController.Utility.DropdownEffectAttribute))]
    public class DropdownEffectAttributeControl : AttributeControlBase
    {
        public static List<Type> s_EffectTypes;
        public static List<string> s_EffectNames;

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
            PopulateEffectTypes();
            var stringValue = input.Value as string;
            var index = 0;
            if (!string.IsNullOrEmpty(stringValue)) {
                for (int i = 0; i < s_EffectTypes.Count; ++i) {
                    if (s_EffectTypes[i].FullName.Equals(stringValue)) {
                        index = i + 1;
                        break;
                    }
                }
            }
            var dropdownField = new DropdownField(s_EffectNames, index);
            var labelControl = new LabelControl(UnityEditor.ObjectNames.NicifyVariableName(input.Field.Name), Shared.Editor.Utility.EditorUtility.GetTooltip(input.Field), dropdownField, true);
            System.Action<object> onBindingUpdateEvent = (object newValue) => { 
                var stringValue = newValue as string;
                if (string.IsNullOrEmpty(stringValue)) {
                    stringValue = "(none)";
                } else {
                    stringValue = InspectorUtility.DisplayTypeName(Shared.Utility.TypeUtility.GetType(stringValue), false);
                }
                dropdownField.SetValueWithoutNotify(stringValue); 
            };
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
                dropdownField.SetValueWithoutNotify(c.newValue);
                c.StopPropagation();
                input.OnChangeEvent(dropdownField.index > 0 ? s_EffectTypes[dropdownField.index - 1].FullName : string.Empty);
            });
            return labelControl;
        }

        /// <summary>
        /// Populates the effect types.
        /// </summary>
        private void PopulateEffectTypes()
        {
            if (s_EffectTypes != null) {
                return;
            }

            s_EffectTypes = new List<Type>();
            s_EffectNames = new List<string>();
            s_EffectNames.Add("(none)");
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    if (typeof(Effect).IsAssignableFrom(assemblyTypes[j]) && !assemblyTypes[j].IsAbstract) {
                        s_EffectTypes.Add(assemblyTypes[j]);
                        s_EffectNames.Add(InspectorUtility.DisplayTypeName(assemblyTypes[j], true));
                    }
                }
            }
        }
    }
}