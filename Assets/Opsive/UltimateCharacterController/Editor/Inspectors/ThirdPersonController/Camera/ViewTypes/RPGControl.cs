/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types.ViewTypeDrawers.ThirdPerson
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements ViewTypeDrawer for the RPG ViewType.
    /// </summary>
    [ControlType(typeof(RPG))]
    public class RPGControl : ThirdPersonControl
    {
        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        public override void CreateDrawer(UnityEngine.Object unityObject, object target, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
        {
            base.CreateDrawer(unityObject, target, container, onValidateChange, onChangeEvent);

            FieldInspectorView.AddField(unityObject, target, "m_YawSnapDamping", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_AllowFreeMovement", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_CameraFreeMovementInputName", container, onChangeEvent, onValidateChange);
        }
    }
}