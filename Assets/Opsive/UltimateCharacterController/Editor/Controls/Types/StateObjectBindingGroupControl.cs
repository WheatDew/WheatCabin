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
    using Opsive.UltimateCharacterController.Items.Actions.Bindings;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the StateObjectBindingGroup ControlType.
    /// </summary>
    [ControlType(typeof(StateObjectBindingGroup))]
    public class StateObjectBindingGroupControlType : TypeControlBase
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
            var group = input.Value as StateObjectBindingGroup;
            if (group == null) {
                return new Label("The State Object Binding Group is null.");
            }
            
            var moduleGroupField = new StateObjectBindingGroupField(ObjectNames.NicifyVariableName(input.Field?.Name ?? "Bindings"), input.UnityObject, group, input.SerializedProperty);
            moduleGroupField.OnValueChange += (newValue) =>
            {
                input.OnChangeEvent?.Invoke(newValue);
            };
            
            if (input.Field != null) {
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    moduleGroupField.Refresh(newValue as StateObjectBindingGroup);
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
    /// Property Drawer for the StateObjectBinding.
    /// </summary>
    [CustomPropertyDrawer(typeof(StateObjectBindingGroup), true)]
    public class StateObjectBindingGroupDrawer : PropertyDrawer
    {
        /// <summary>
        /// Creates a new Property Drawer.
        /// </summary>
        /// <param name="serializedProperty">The property to create the drawer for.</param>
        /// <returns>The new Property Drawer.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            var group = (StateObjectBindingGroup) ControlUtility.GetTargetObjectOfProperty(serializedProperty);
            var moduleGroupField = new StateObjectBindingGroupField(ObjectNames.NicifyVariableName(serializedProperty.name),
                                        serializedProperty.serializedObject.targetObject, group, serializedProperty);
            return moduleGroupField;
        }
    }
    
    /// <summary>
    /// Reorderable List which binds the StateObjectBindingGroup.
    /// </summary>
    public class StateObjectBindingGroupField : GenericReorderableList<StateObjectBinding>
    {
        public event Action<StateObjectBindingGroup> OnValueChange;
        protected StateObjectBindingGroup m_Group;

        /// <summary>
        /// Three parameter constructor.
        /// </summary>
        /// <param name="title">The title of the ReorderableList.</param>
        /// <param name="target">The object that is being inspected.</param>
        /// <param name="group">The group that is being inspected</param>
        /// <param name="serializedProperty">The SerializedProperty that the object belongs to.</param>
        public StateObjectBindingGroupField(string title, UnityEngine.Object target, StateObjectBindingGroup group, SerializedProperty serializedProperty)
        {
            m_Group = group;
            Setup(title, target, Get, Set, serializedProperty?.FindPropertyRelative("m_Bindings"));
        }

        /// <summary>
        /// Returns the ReorderableList source.
        /// </summary>
        /// <returns>The ReorderableList source.</returns>
        protected virtual IList<StateObjectBinding> Get()
        {
            return m_Group.Bindings;
        }

        /// <summary>
        /// The ReorderableList has changed.
        /// </summary>
        /// <param name="bindings">The updated source values.</param>
        protected virtual void Set(IList<StateObjectBinding> bindings)
        {
            m_Group.Bindings = bindings.ToArray();
            m_Group.Initialize(m_Group.StateObject, m_Group.BoundGameObject);
            OnValueChange?.Invoke(m_Group);
        }
        
        /// <summary>
        /// Refresh the Impact Action Group.
        /// </summary>
        /// <param name="group">The Impact Action Group</param>
        public virtual void Refresh(StateObjectBindingGroup @group)
        {
            m_Group = @group;
            Refresh();
        }
    }
}