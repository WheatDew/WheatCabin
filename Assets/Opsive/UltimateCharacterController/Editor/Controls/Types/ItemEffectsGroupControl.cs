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
    using Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions;
    using Opsive.UltimateCharacterController.Items.Actions.Effect;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Implements TypeControlBase for the ItemEffectGroup ControlType.
    /// </summary>
    [ControlType(typeof(ItemEffectGroup))]
    public class ItemEffectsGroupControlType : TypeControlBase
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
            ItemEffectGroup effectGroup = input.Value as ItemEffectGroup;
            if (effectGroup == null) {
                return new Label("The Effect Group is null.");
            }

            var moduleGroupField = new ItemEffectGroupField(
                ObjectNames.NicifyVariableName(input.Field?.Name ?? "Impact Action Group"), input.UnityObject, effectGroup, input.SerializedProperty);
            moduleGroupField.OnValueChange += (newValue) =>
            {
                input.OnChangeEvent?.Invoke(newValue);
            };
            
            if (input.Field != null) {
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    moduleGroupField.Refresh(newValue as ItemEffectGroup);
                };
                moduleGroupField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(input.Field, input.ArrayIndex, input.Target, onBindingUpdateEvent);
                });
                moduleGroupField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
            }

            return moduleGroupField;
        }
    }
    
    /// <summary>
    /// A property drawer used to draw an Item Effect group.
    /// </summary>
    [CustomPropertyDrawer(typeof(ItemEffectGroup), true)]
    public class ItemEffectGroupDrawer : PropertyDrawer
    {
        /// <summary>
        /// Override this method to make your own UIElements based GUI for the property.
        /// </summary>
        /// <param name="serializedProperty">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>The element containing the custom GUI.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            var moduleGroup = (ItemEffectGroup) ControlUtility.GetTargetObjectOfProperty(serializedProperty);
            var moduleGroupField = new ItemEffectGroupField(ObjectNames.NicifyVariableName(serializedProperty.name), 
                                            serializedProperty.serializedObject.targetObject, moduleGroup, serializedProperty);
            return moduleGroupField;
        }
    }
    
    /// <summary>
    /// The Item Effect group reorderable list.
    /// </summary>
    public class ItemEffectGroupField : GenericReorderableList<ItemEffect>
    {
        public event Action<ItemEffectGroup> OnValueChange;
        protected ItemEffectGroup m_EffectGroup;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="title">The field title.</param>
        /// <param name="target">The target object.</param>
        /// <param name="effectGroup">The item effect group object.</param>
        /// <param name="serializedProperty">The SerializedProperty that the object belongs to.</param>
        public ItemEffectGroupField(string title, Object target, ItemEffectGroup effectGroup, SerializedProperty serializedProperty)
        {
            m_EffectGroup = effectGroup;
            Setup(title, target, Get, Set, serializedProperty?.FindPropertyRelative("m_Effects"));
        }

        /// <summary>
        /// Get the item effects.
        /// </summary>
        /// <returns>The item effects.</returns>
        protected virtual IList<ItemEffect> Get()
        {
            return m_EffectGroup.Effects;
        }

        /// <summary>
        /// Set the item effects.
        /// </summary>
        /// <param name="effects">The item effects to set.</param>
        protected virtual void Set(IList<ItemEffect> effects)
        {
            m_EffectGroup.Effects = effects.ToArray();
            OnValueChange?.Invoke(m_EffectGroup);
        }

        /// <summary>
        /// Refresh the ItemEffectGroup.
        /// </summary>
        /// <param name="itemEffectGroup">The Item Effect Group</param>
        public virtual void Refresh(ItemEffectGroup itemEffectGroup)
        {
            m_EffectGroup = itemEffectGroup;
            Refresh();
        }
    }
}