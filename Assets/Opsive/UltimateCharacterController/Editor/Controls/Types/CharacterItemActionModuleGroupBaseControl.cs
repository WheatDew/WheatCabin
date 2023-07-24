/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.Shared.Editor.Utility;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    [ControlType(typeof(ActionModuleGroupBase))]
    public class CharacterItemActionModuleGroupBaseControl : TypeControlBase
    {
        public static string c_MissingIconGuid = "b34170890b9b9d1469d9b451fddc01dd";

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
            var moduleGroupBase = input.Value as ActionModuleGroupBase;
            if (moduleGroupBase == null) {
                return new Label("The Module Group is null.");
            }

            // Get the Icon.
            var icon = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(c_MissingIconGuid);
            if (input.Field != null) {
                var groupAttribute = input.Field.GetCustomAttribute<ActionModuleGroupAttribute>();
                if (groupAttribute != null) {
                    var groupIcon = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(groupAttribute.IconGuid);
                    if (groupIcon != null) {
                        icon = groupIcon;
                    }
                }
            }

            var moduleGroupField = new CharacterItemActionModuleGroupField(ObjectNames.NicifyVariableName(input.Field?.Name ?? "Module Group"), 
                                                        icon, input.UnityObject, moduleGroupBase, input.SerializedProperty);
            moduleGroupField.OnValueChange += newValue =>
            {
                input.OnChangeEvent?.Invoke(newValue);
            };

            return moduleGroupField;
        }
    }
    
    /// <summary>
    /// A property drawer used to draw an Action Module Group.
    /// </summary>
    [CustomPropertyDrawer(typeof(ActionModuleGroupBase), true)]
    public class CharacterItemActionModuleGroupBaseDrawer : PropertyDrawer
    {
        /// <summary>
        /// Override this method to make your own UIElements based GUI for the property.
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>The element containing the custom GUI.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var moduleGroup = (ActionModuleGroupBase) ControlUtility.GetTargetObjectOfProperty(property);

            // Get the Icon.
            var icon = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(CharacterItemActionModuleGroupBaseControl.c_MissingIconGuid);
            var groupAttribute = property.GetCustomAttribute<ActionModuleGroupAttribute>(true);
            if (groupAttribute != null) {
                var groupIcon = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(groupAttribute.IconGuid);
                if (groupIcon != null) {
                    icon = groupIcon;
                }
            }

            var moduleGroupField = new CharacterItemActionModuleGroupField(ObjectNames.NicifyVariableName(property.name), 
                                        icon, property.serializedObject.targetObject, moduleGroup, property);
            
            return moduleGroupField;
        }
    }
}