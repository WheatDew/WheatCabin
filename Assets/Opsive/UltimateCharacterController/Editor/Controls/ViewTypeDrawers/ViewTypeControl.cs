/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types.ViewTypeDrawers
{
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the ViewType ControlType.
    /// </summary>
    [ControlType(typeof(ViewType))]
    public class ViewTypeControl : StateObjectControlType
    {
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
        public override VisualElement GetHeaderControl(UnityEngine.Object unityObject, object target, System.Reflection.FieldInfo field, UnityEditor.SerializedProperty serializedProperty,
                                                        int arrayIndex, System.Type type, object value, System.Func<object, bool> onChangeEvent, object userData)
        {
            var container = new VisualElement();

            var viewTypeDrawer = ViewTypeDrawerUtility.FindViewTypeDrawer(type, true);
            if (viewTypeDrawer != null) {
                viewTypeDrawer.CreateDrawer(unityObject, target, container, null, (o) =>
                {
                    onChangeEvent(o);
                });
            } else {
                FieldInspectorView.AddFields(unityObject, target, Shared.Utility.MemberVisibility.Public, container, (o) => { onChangeEvent(o); }, null, null, false, null, true);
            }

            return container;
        }
    }
}