/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types
{
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.Shared.Utility;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the StateObject ControlType.
    /// </summary>
    [ControlType(typeof(StateObject))]
    public class StateObjectControlType : TypeControlBase
    {
        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get => false; }

        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(TypeControlInput input)
        {
            var container = new VisualElement();
            var topControl = GetHeaderControl(input.UnityObject, input.Target, input.Field, input.SerializedProperty, input.ArrayIndex, input.Type, input.Value, input.OnChangeEvent, input.UserData);
            if (topControl != null) {
                container.Add(topControl);
            }
            FieldInspectorView.AddField(input.UnityObject, input.Value, "m_States", container, (o) => { input.OnChangeEvent(o); });
            return container;
        }

        /// <summary>
        /// Returns the header control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="field">The field responsible for the control (can be null).</param>
        /// <param name="serializedProperty">The SerializedProperty bound to the field (can be null).</param>
        /// <param name="arrayIndex">The index of the object within the array (-1 indicates no array).</param>
        /// <param name="type">The type of control being retrieved.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="userData">Optional data which can be used by the controls.</param>
        /// <returns>The created control.</returns>
        public virtual VisualElement GetHeaderControl(UnityEngine.Object unityObject, object target, System.Reflection.FieldInfo field, UnityEditor.SerializedProperty serializedProperty,
                                                         int arrayIndex, System.Type type, object value, System.Func<object, bool> onChangeEvent, object userData)
        {
            var container = new VisualElement();

            /// By default GetFields will return the inherited fields before the base class fields. Reverse the order so the base class fields are
            /// shown before the inherited fields.
            var allBaseTypes = new List<Type>();
            var baseType = type;
            while (baseType != typeof(Opsive.Shared.StateSystem.StateObject)) {
                allBaseTypes.Add(baseType);
                baseType = baseType.BaseType;
            }
            
            // All of the types have been determined. Get the fields.
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            for (int i = allBaseTypes.Count - 1; i >= 0; --i) {
                var fields = allBaseTypes[i].GetFields(flags);
                for (int j = 0; j < fields.Length; ++j) {
                    var localField = fields[j];

                    // Don't show the field if:
                    // - The visibility is public but the field is private/protected without the SerializeField attribute.
                    // - The field has the HideInInspector attribute.
                    if (((localField.IsPrivate || localField.IsFamily) && TypeUtility.GetAttribute(localField, typeof(SerializeField)) == null) ||
                        TypeUtility.GetAttribute(localField, typeof(HideInInspector)) != null) {
                        continue;
                    }

                    FieldInspectorView.AddField(unityObject, value, localField, serializedProperty?.FindPropertyRelative(localField.Name), (VisualElement element, FieldInfo addField) => { container.Add(element); }, (object o) => { onChangeEvent(o); });
                }
            }

            return container;
        }
    }
}