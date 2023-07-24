/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types.ViewTypeDrawers.ThirdPerson
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements ViewTypeDrawer for the LookAt ViewType.
    /// </summary>
    [ControlType(typeof(LookAt))]
    public class LookAtControl : ViewTypeDrawer
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
            FieldInspectorView.AddField(unityObject, target, "m_FieldOfView", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FieldOfViewDamping", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_Target", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_Offset", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_LookDistanceLimit", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_PositionSmoothing", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotationalLerpSpeed", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_CollisionRadius", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotationSpring", container, onChangeEvent, onValidateChange);
        }

        /// <summary>
        /// The ability has been added to the camera. Perform any initialization.
        /// </summary>
        /// <param name="viewType">The view type that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public override void ViewTypeAdded(ViewType viewType, Object parent)
        {
            var cameraController = parent as CameraController;
            if (cameraController.Character == null) {
                return;
            }

            var animator = cameraController.Character.GetComponent<Animator>();
            if (animator == null || !animator.isHuman) {
                return;
            }

            // Automatically set the Transform variables if the character is a humanoid.
            var lookAt = viewType as LookAt;
            lookAt.Target = animator.GetBoneTransform(HumanBodyBones.Head);
        }
    }
}