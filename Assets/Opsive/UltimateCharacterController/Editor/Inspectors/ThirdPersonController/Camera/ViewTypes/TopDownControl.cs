/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types.ViewTypeDrawers.ThirdPerson
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements ViewTypeDrawer for the TopDown ViewType.
    /// </summary>
    [ControlType(typeof(TopDown))]
    public class TopDownControl : ViewTypeDrawer
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
            FieldInspectorView.AddField(unityObject, target, "m_UpAxis", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FieldOfView", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FieldOfViewDamping", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotationSpeed", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_CollisionRadius", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ViewDistance", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ViewStep", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_MoveSmoothing", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_VerticalLookDirection", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_PitchLimit", container, onChangeEvent, onValidateChange);

            var cameraRotationContainer = new VisualElement();
            FieldInspectorView.AddField(unityObject, target, "m_AllowDynamicCameraRotation", container, (object obj) => {
                onChangeEvent(obj);
                cameraRotationContainer.style.display = (obj as TopDown).AllowDynamicCameraRotation ? DisplayStyle.Flex : DisplayStyle.None;
                }, onValidateChange);
            container.Add(cameraRotationContainer);
            cameraRotationContainer.AddToClassList("indent");
            cameraRotationContainer.style.display = InspectorUtility.GetFieldValue<bool>(target, "m_AllowDynamicCameraRotation") ? DisplayStyle.Flex : DisplayStyle.None;
            FieldInspectorView.AddField(unityObject, target, "m_DesiredAngle", cameraRotationContainer, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ChangeAngleSpeed", cameraRotationContainer, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotationTransitionCurve", cameraRotationContainer, onChangeEvent, onValidateChange);

            var dynamicPitchContainer = new VisualElement();
            FieldInspectorView.AddField(unityObject, target, "m_AllowDynamicPitchAdjustment", container, (object obj) => {
                onChangeEvent(obj);
                dynamicPitchContainer.style.display = (obj as TopDown).AllowDynamicPitchAdjustment ? DisplayStyle.Flex : DisplayStyle.None;
            }, onValidateChange);
            container.Add(dynamicPitchContainer);
            dynamicPitchContainer.AddToClassList("indent");
            dynamicPitchContainer.style.display = InspectorUtility.GetFieldValue<bool>(target, "m_AllowDynamicPitchAdjustment") ? DisplayStyle.Flex : DisplayStyle.None;
            FieldInspectorView.AddField(unityObject, target, "m_DesiredPitch", dynamicPitchContainer, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ChangePitchSpeed", dynamicPitchContainer, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_UseIndependentPitchTransition", dynamicPitchContainer, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_PitchTransitionCurve", dynamicPitchContainer, onChangeEvent, onValidateChange);

            var dynamicDistanceContainer = new VisualElement();
            FieldInspectorView.AddField(unityObject, target, "m_AllowDynamicDistanceAdjustment", container, (object obj) => {
                onChangeEvent(obj);
                dynamicDistanceContainer.style.display = (obj as TopDown).AllowDynamicDistanceAdjustment ? DisplayStyle.Flex : DisplayStyle.None;
            }, onValidateChange);
            container.Add(dynamicDistanceContainer);
            dynamicDistanceContainer.AddToClassList("indent");
            dynamicDistanceContainer.style.display = InspectorUtility.GetFieldValue<bool>(target, "m_AllowDynamicDistanceAdjustment") ? DisplayStyle.Flex : DisplayStyle.None;
            FieldInspectorView.AddField(unityObject, target, "m_DesiredDistance", dynamicDistanceContainer, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ChangeDistanceSpeed", dynamicDistanceContainer, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_UseIndependentDistanceTransition", dynamicDistanceContainer, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_DistanceTransitionCurve", dynamicDistanceContainer, onChangeEvent, onValidateChange);

            var foldout = new Foldout();
            foldout.text = "Secondary Spring";
            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_SecondaryPositionSpring", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_SecondaryRotationSpring", foldout, onChangeEvent, onValidateChange);
        }
    }
}