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
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Implements TypeControlBase for the ImpactActionConditionGroup ControlType.
    /// </summary>
    [ControlType(typeof(ImpactActionConditionGroup))]
    public class ImpactActionConditionGroupControlType : TypeControlBase
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
            ImpactActionConditionGroup ImpactActionConditionGroup = input.Value as ImpactActionConditionGroup;
            if (ImpactActionConditionGroup == null) {
                return new Label("The Impact Action Group is null.");
            }
            
            var moduleGroupField = new ImpactActionConditionGroupField(
                ObjectNames.NicifyVariableName(input.Field?.Name ?? "Impact Action Group"), input.UnityObject, ImpactActionConditionGroup, input.SerializedProperty);
            moduleGroupField.OnValueChange += (newValue) =>
            {
                input.OnChangeEvent?.Invoke(newValue);
            };
            
            if (input.Field != null) {
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    moduleGroupField.Refresh(newValue as ImpactActionConditionGroup);
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
    /// A property drawer used to draw an Impact action group.
    /// </summary>
    [CustomPropertyDrawer(typeof(ImpactActionConditionGroup), true)]
    public class ImpactActionConditionGroupDrawer : PropertyDrawer
    {
        /// <summary>
        /// Override this method to make your own UIElements based GUI for the property.
        /// </summary>
        /// <param name="serializedProperty">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>The element containing the custom GUI.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            var moduleGroup = (ImpactActionConditionGroup) ControlUtility.GetTargetObjectOfProperty(serializedProperty);
            var moduleGroupField = new ImpactActionConditionGroupField(ObjectNames.NicifyVariableName(serializedProperty.name),
                                            serializedProperty.serializedObject.targetObject, moduleGroup, serializedProperty);
            return moduleGroupField;
        }
    }
    
    /// <summary>
    /// The Impact action group reorderable list.
    /// </summary>
    public class ImpactActionConditionGroupField : GenericReorderableList<ImpactActionCondition>
    {
        public event Action<ImpactActionConditionGroup> OnValueChange;
        protected ImpactActionConditionGroup m_ImpactActionConditionGroup;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="title">The field title.</param>
        /// <param name="target">The target object.</param>
        /// <param name="ImpactActionConditionGroup">The impact action group object.</param>
        /// <param name="serializedProperty">The SerializedProperty that the object belongs to.</param>
        public ImpactActionConditionGroupField(string title, Object target, ImpactActionConditionGroup ImpactActionConditionGroup, SerializedProperty serializedProperty)
        {
            m_ImpactActionConditionGroup = ImpactActionConditionGroup;
            Setup(title, target, Get, Set, serializedProperty?.FindPropertyRelative("m_ImpactActionConditions"));
        }

        /// <summary>
        /// Get the impact actions.
        /// </summary>
        /// <returns>The impact actions.</returns>
        protected virtual IList<ImpactActionCondition> Get()
        {
            return m_ImpactActionConditionGroup.ImpactActionConditions;
        }

        /// <summary>
        /// Set the impact actions.
        /// </summary>
        /// <param name="actions">The impact actions to set.</param>
        protected virtual void Set(IList<ImpactActionCondition> actions)
        {
            m_ImpactActionConditionGroup.ImpactActionConditions = actions.ToArray();
            OnValueChange?.Invoke(m_ImpactActionConditionGroup);
        }
        
        /// <summary>
        /// Refresh the Impact Action Group.
        /// </summary>
        /// <param name="ImpactActionConditionGroup">The Impact Action Group</param>
        public virtual void Refresh(ImpactActionConditionGroup ImpactActionConditionGroup)
        {
            m_ImpactActionConditionGroup = ImpactActionConditionGroup;
            Refresh();
        }
    }
}