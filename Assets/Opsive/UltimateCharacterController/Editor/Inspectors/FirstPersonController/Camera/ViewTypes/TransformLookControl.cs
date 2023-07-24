/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types.ViewTypeDrawers.FirstPerson
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements ViewTypeDrawer for the TransformLook ViewType.
    /// </summary>
    [ControlType(typeof(TransformLook))]
    public class TransformLookControl : ViewTypeDrawer
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
            FieldInspectorView.AddFields(unityObject, target, Shared.Utility.MemberVisibility.Public, container, onChangeEvent, null, onValidateChange);
        }

        /// <summary>
        /// The ability has been added to the camera. Perform any initialization.
        /// </summary>
        /// <param name="viewType">The view type that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public override void ViewTypeAdded(ViewType viewType, Object parent)
        {
            var cameraController = parent as CameraController;
            if (cameraController == null || cameraController.Character == null) {
                return;
            }

            Animator animator = null;
            var modelManager = cameraController.Character.GetComponent<ModelManager>();
            if (modelManager != null) {
                animator = modelManager.ActiveModel.GetComponent<Animator>();
            } else {
                var animatorMonitor = cameraController.Character.GetComponentInChildren<AnimatorMonitor>();
                if (animatorMonitor != null) {
                    animator = animatorMonitor.gameObject.GetComponent<Animator>();
                }
            }
            if (animator == null || !animator.isHuman) {
                return;
            }

            // Automatically set the Transform variables if the character is a humanoid.
            var transformLook = viewType as TransformLook;
            transformLook.MoveTarget = animator.GetBoneTransform(HumanBodyBones.Head);
            transformLook.RotationTarget = animator.GetBoneTransform(HumanBodyBones.Hips);
        }
    }
}