/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types.ViewTypeDrawers.FirstPerson
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes;
#if ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP || ULTIMATE_CHARACTER_CONTROLLER_HDRP
    using Opsive.UltimateCharacterController.Camera;
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements ViewTypeDrawer for the FirstPerson ViewType.
    /// </summary>
    [ControlType(typeof(FirstPerson))]
    public class FirstPersonControl : ViewTypeDrawer
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
            var foldout = new Foldout();
            foldout.text = "Rendering";
            container.Add(foldout);

            FieldInspectorView.AddField(unityObject, target, "m_LookDirectionDistance", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_LookOffset", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_LookDownOffset", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_CullingMask", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FieldOfView", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FieldOfViewDamping", foldout, onChangeEvent, onValidateChange);

            var firstPersonCameraFoldout = new Foldout();
            firstPersonCameraFoldout.text = "First Person Camera";

#if ULTIMATE_CHARACTER_CONTROLLER_UNIVERSALRP || ULTIMATE_CHARACTER_CONTROLLER_HDRP
            var renderType = ((FirstPerson)target).OverlayRenderType;
            var drawFirstPersonCamera = renderType == FirstPerson.ObjectOverlayRenderType.SecondCamera;

            var renderPipelineContainer = new VisualElement();
            foldout.Add(renderPipelineContainer);
            renderPipelineContainer.style.display = renderType == FirstPerson.ObjectOverlayRenderType.RenderPipeline ? DisplayStyle.Flex : DisplayStyle.None;
            
            FieldInspectorView.AddField(unityObject, target, "m_OverlayRenderType", foldout, (object obj) =>
            {
                onChangeEvent(obj);

                renderType = (obj as FirstPerson).OverlayRenderType;
                if (renderType == FirstPerson.ObjectOverlayRenderType.SecondCamera) {
#if FIRST_PERSON_CONTROLLER
                    UltimateCharacterController.Utility.Builders.ViewTypeBuilder.AddFirstPersonCamera(unityObject as CameraController, target as FirstPerson);
#endif
                } else {
                    var firstPersonCamera = (target as FirstPerson).FirstPersonCamera;
                    if (firstPersonCamera != null) {
                        if (PrefabUtility.IsPartOfPrefabInstance(firstPersonCamera.gameObject)) {
                            PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(firstPersonCamera.gameObject), PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                        }
                        GameObject.DestroyImmediate(firstPersonCamera.gameObject, true);
                        (target as FirstPerson).FirstPersonCamera = null;
                    }
                }
                firstPersonCameraFoldout.style.display = renderType == FirstPerson.ObjectOverlayRenderType.SecondCamera ? DisplayStyle.Flex : DisplayStyle.None;

                // All first person view types need to be switched to the currently active render type.
                var viewTypes = (unityObject as CameraController).ViewTypes;
                for (int i = 0; i < viewTypes.Length; ++i) {
                    var firstPersonViewType = viewTypes[i] as FirstPerson;
                    if (firstPersonViewType == null) {
                        continue;
                    }

                    if (renderType == FirstPerson.ObjectOverlayRenderType.RenderPipeline && firstPersonViewType.OverlayRenderType == FirstPerson.ObjectOverlayRenderType.SecondCamera) {
                        firstPersonViewType.FirstPersonCamera = null;
                        firstPersonViewType.OverlayRenderType = renderType;
                    } else if (renderType == FirstPerson.ObjectOverlayRenderType.SecondCamera && firstPersonViewType.OverlayRenderType == FirstPerson.ObjectOverlayRenderType.RenderPipeline) {
                        firstPersonViewType.FirstPersonCamera = (target as FirstPerson).FirstPersonCamera;
                        firstPersonViewType.OverlayRenderType = renderType;
                    }
                }
            }, onValidateChange);
            
            foldout.Add(firstPersonCameraFoldout);
            FieldInspectorView.AddField(unityObject, target, "m_FirstPersonCullingMask", renderPipelineContainer, onChangeEvent, onValidateChange);
#else
            FieldInspectorView.AddField(unityObject, target, "m_UseFirstPersonCamera", foldout, onChangeEvent, onValidateChange);
            var drawFirstPersonCamera = ((FirstPerson)target).UseFirstPersonCamera;
#endif
            firstPersonCameraFoldout.style.display = drawFirstPersonCamera ? DisplayStyle.Flex : DisplayStyle.None;

            FieldInspectorView.AddField(unityObject, target, "m_FirstPersonCamera", firstPersonCameraFoldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FirstPersonCullingMask", firstPersonCameraFoldout, onChangeEvent, onValidateChange);

            var firstPersonFieldOfViewContainer = new VisualElement();
            firstPersonCameraFoldout.Add(firstPersonFieldOfViewContainer);
            firstPersonFieldOfViewContainer.style.display = ((FirstPerson)target).SynchronizeFieldOfView ? DisplayStyle.Flex : DisplayStyle.None;
            FieldInspectorView.AddField(unityObject, target, "m_SynchronizeFieldOfView", firstPersonCameraFoldout, (object obj) =>
            {
                onChangeEvent(obj);
                firstPersonFieldOfViewContainer.style.display = (bool)obj ? DisplayStyle.Flex : DisplayStyle.None;
            }, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FirstPersonFieldOfView", firstPersonFieldOfViewContainer, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FirstPersonFieldOfViewDamping", firstPersonFieldOfViewContainer, onChangeEvent, onValidateChange);

            FieldInspectorView.AddField(unityObject, target, "m_FirstPersonPositionOffset", firstPersonCameraFoldout, (object obj) =>
            {
                onChangeEvent(obj);
                // Set the property if the game is playing so the camera will update.
                (target as FirstPerson).FirstPersonPositionOffset = (Vector3)obj;
            }, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_FirstPersonRotationOffset", firstPersonCameraFoldout, (object obj) =>
            {
                onChangeEvent(obj);
                // Set the property if the game is playing so the camera will update.
                (target as FirstPerson).FirstPersonRotationOffset = (Vector3)obj;
            }, onValidateChange);

            foldout = new Foldout();
            foldout.text = "Primary Spring";
            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_PositionSpring", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_PositionLowerVerticalLimit", foldout, (object obj) =>
            {
                onChangeEvent(obj);
                // Set the property if the game is playing so the camera will update.
                (target as FirstPerson).PositionLowerVerticalLimit = (float)obj;
            }, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_PositionFallImpact", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_PositionFallImpactSoftness", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotationStrafeRoll", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotationSpring", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotationFallImpact", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotationFallImpactSoftness", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_SecondaryRotationSpeed", foldout, onChangeEvent, onValidateChange);

            foldout = new Foldout();
            foldout.text = "Secondary Spring";
            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_SecondaryPositionSpring", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_SecondaryRotationSpring", foldout, onChangeEvent, onValidateChange);

            foldout = new Foldout();
            foldout.text = "Shake";
            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_ShakeSpeed", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ShakeAmplitude", foldout, onChangeEvent, onValidateChange);

            foldout = new Foldout();
            foldout.text = "Bob";
            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_BobPositionalRate", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_BobPositionalAmplitude", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_BobRollRate", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_BobRollAmplitude", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_BobInputVelocityScale", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_BobMaxInputVelocity", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_BobMinTroughVerticalOffset", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_BobTroughForce", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_BobRequireGroundContact", foldout, onChangeEvent, onValidateChange);

            foldout = new Foldout();
            foldout.text = "Limits";
            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_PitchLimit", foldout, onChangeEvent, onValidateChange);
            if (target is FreeLook) {
                FieldInspectorView.AddField(unityObject, target, "m_YawLimit", foldout, onChangeEvent, onValidateChange);
                FieldInspectorView.AddField(unityObject, target, "m_YawLimitLerpSpeed", foldout, onChangeEvent, onValidateChange);
                FieldInspectorView.AddField(unityObject, target, "m_RotateWithCharacter", foldout, onChangeEvent, onValidateChange);
            }
            FieldInspectorView.AddField(unityObject, target, "m_LookDirectionDistance", foldout, onChangeEvent, onValidateChange);

            foldout = new Foldout();
            foldout.text = "Head Tracking";
            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_SmoothHeadOffsetSteps", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_CollisionRadius", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_RotateWithHead", foldout, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_VerticalOffsetLerpSpeed", foldout, onChangeEvent, onValidateChange);
        }
    }
}