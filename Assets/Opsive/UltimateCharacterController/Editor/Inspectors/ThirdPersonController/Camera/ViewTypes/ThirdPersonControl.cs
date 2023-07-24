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
    /// Implements RPG for the ThirdPerson ViewType.
    /// </summary>
    [ControlType(typeof(ThirdPerson))]
    public class ThirdPersonControl : ViewTypeDrawer
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
            FieldInspectorView.AddField(unityObject, target, "m_LookDirectionDistance", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ForwardAxis", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_LookOffset", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_LookOffsetSmoothing", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotationSpeed", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_SecondaryRotationSpeed", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_HorizontalPivotFreedom", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ObstructionPositionSmoothing", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FieldOfView", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FieldOfViewDamping", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_CollisionRadius", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_CollisionAnchorOffset", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_PitchLimit", container, onChangeEvent, onValidateChange);
            if (target is Adventure) {
                FieldInspectorView.AddField(unityObject, target, "m_YawLimit", container, onChangeEvent, onValidateChange);
                FieldInspectorView.AddField(unityObject, target, "m_YawLimitLerpSpeed", container, onChangeEvent, onValidateChange);
            }

            var foldout = new Foldout();
            foldout.text = "Primary Spring";
            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_PositionSpring", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotationSpring", foldout, onChangeEvent, onValidateChange);

            foldout = new Foldout();
            foldout.text = "Secondary Spring";
            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_SecondaryPositionSpring", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_SecondaryRotationSpring", foldout, onChangeEvent, onValidateChange);

            foldout = new Foldout();
            foldout.text = "Step Zoom";
            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_StepZoomInputName", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_StepZoomSensitivity", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_StepZoomLimit", foldout, onChangeEvent, onValidateChange);
        }
    }
}