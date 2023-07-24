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
    /// Implements TypeControlBase for the ImpactActionGroup ControlType.
    /// </summary>
    [ControlType(typeof(ImpactActionGroup))]
    public class ImpactActionGroupControlType : TypeControlBase
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
            ImpactActionGroup impactActionGroup = input.Value as ImpactActionGroup;
            if (impactActionGroup == null) {
                return new Label("The Impact Action Group is null.");
            }
            
            var moduleGroupField = new ImpactActionGroupField(
                ObjectNames.NicifyVariableName(input.Field?.Name ?? "Impact Action Group"), input.UnityObject, impactActionGroup, input.SerializedProperty);
            moduleGroupField.OnValueChange += (newValue) =>
            {
                input.OnChangeEvent?.Invoke(newValue);
            };
            
            if (input.Field != null) {
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    moduleGroupField.Refresh(newValue as ImpactActionGroup);
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
    [CustomPropertyDrawer(typeof(ImpactActionGroup), true)]
    public class ImpactActionGroupDrawer : PropertyDrawer
    {
        /// <summary>
        /// Override this method to make your own UIElements based GUI for the property.
        /// </summary>
        /// <param name="serializedProperty">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>The element containing the custom GUI.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            var moduleGroup = (ImpactActionGroup) ControlUtility.GetTargetObjectOfProperty(serializedProperty);
            var moduleGroupField = new ImpactActionGroupField(ObjectNames.NicifyVariableName(serializedProperty.name),
                                            serializedProperty.serializedObject.targetObject, moduleGroup, serializedProperty);
            return moduleGroupField;
        }
    }
    
    /// <summary>
    /// The Impact action group reorderable list.
    /// </summary>
    public class ImpactActionGroupField : GenericReorderableList<ImpactAction>
    {
        public event Action<ImpactActionGroup> OnValueChange;
        protected ImpactActionGroup m_ImpactActionGroup;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="title">The field title.</param>
        /// <param name="target">The target object.</param>
        /// <param name="impactActionGroup">The impact action group object.</param>
        /// <param name="serializedProperty">The SerializedProperty that the object belongs to.</param>
        public ImpactActionGroupField(string title, Object target, ImpactActionGroup impactActionGroup, SerializedProperty serializedProperty)
        {
            m_ImpactActionGroup = impactActionGroup;
            Setup(title, target, Get, Set, serializedProperty?.FindPropertyRelative("m_ImpactActions"));
        }

        /// <summary>
        /// Get the impact actions.
        /// </summary>
        /// <returns>The impact actions.</returns>
        protected virtual IList<ImpactAction> Get()
        {
            return m_ImpactActionGroup.ImpactActions;
        }

        /// <summary>
        /// Set the impact actions.
        /// </summary>
        /// <param name="actions">The impact actions to set.</param>
        protected virtual void Set(IList<ImpactAction> actions)
        {
            m_ImpactActionGroup.ImpactActions = actions.ToArray();
            OnValueChange?.Invoke(m_ImpactActionGroup);
        }
        
        /// <summary>
        /// Refresh the Impact Action Group.
        /// </summary>
        /// <param name="impactActionGroup">The Impact Action Group</param>
        public virtual void Refresh(ImpactActionGroup impactActionGroup)
        {
            m_ImpactActionGroup = impactActionGroup;
            Refresh();
        }
    }
}