/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types.AbilityDrawers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements AbilityDrawer for the DetectObjectAbilityBase ControlType.
    /// </summary>
    [ControlType(typeof(DetectObjectAbilityBase))]
    public class DetectObjectAbilityBaseDrawer : AbilityDrawer
    {
        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        public override void CreateDrawer(UnityEngine.Object unityObject, object target, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
        {
            FieldInspectorView.AddFields(unityObject, target, Shared.Utility.MemberVisibility.Public, container, onChangeEvent, null, onValidateChange, false, null, true);

            var detectObject = target as DetectObjectAbilityBase;
            var detectionContainer = new VisualElement();
            var castContainer = new VisualElement();
            var sphereCastContainer = new VisualElement();
            var triggerContainer = new VisualElement();

            FieldInspectorView.AddField(unityObject, target, "m_ObjectDetection", container, (o) =>
            {
                onChangeEvent(o);
                ShowDetectionFields(unityObject, detectObject, detectionContainer, onValidateChange, onChangeEvent);
                ShowCastFields(unityObject, detectObject, castContainer, onValidateChange, onChangeEvent);
                ShowSphereCastFields(unityObject, detectObject, sphereCastContainer, onValidateChange, onChangeEvent);
                ShowTriggerFields(unityObject, detectObject, triggerContainer, onValidateChange, onChangeEvent);
            }, onValidateChange);
            ShowDetectionFields(unityObject, detectObject, detectionContainer, onValidateChange, onChangeEvent);
            ShowCastFields(unityObject, detectObject, castContainer, onValidateChange, onChangeEvent);
            ShowSphereCastFields(unityObject, detectObject, sphereCastContainer, onValidateChange, onChangeEvent);
            ShowTriggerFields(unityObject, detectObject, triggerContainer, onValidateChange, onChangeEvent);

            container.Add(detectionContainer);
            container.Add(castContainer);
            container.Add(sphereCastContainer);
            container.Add(triggerContainer);

            FieldInspectorView.AddField(unityObject, target, "m_MoveWithObject", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_DetectedObject", (VisualElement element, System.Reflection.FieldInfo addField) => 
                                        { element.SetEnabled(false); container.Add(element); }, onChangeEvent, onValidateChange);
        }

        /// <summary>
        /// Shows the detection elements.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="detectObject">The DetectObjectAbilityBase ability reference.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        private void ShowDetectionFields(UnityEngine.Object unityObject, DetectObjectAbilityBase detectObject, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
        {
            container.Clear();
            if (detectObject.ObjectDetection == 0) {
                return;
            }

            container.AddToClassList("indent");
            FieldInspectorView.AddField(unityObject, detectObject, "m_DetectLayers", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, detectObject, "m_UseLookPosition", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, detectObject, "m_UseLookDirection", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, detectObject, "m_AngleThreshold", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, detectObject, "m_ObjectID", container, onChangeEvent, onValidateChange);
        }

        /// <summary>
        /// Shows the cast elements.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="detectObject">The DetectObjectAbilityBase ability reference.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        private void ShowCastFields(UnityEngine.Object unityObject, DetectObjectAbilityBase detectObject, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
        {
            container.Clear();
            if (detectObject.ObjectDetection == 0 || detectObject.ObjectDetection == DetectObjectAbilityBase.ObjectDetectionMode.Trigger) {
                return;
            }

            container.AddToClassList("indent");
            FieldInspectorView.AddField(unityObject, detectObject, "m_CastDistance", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, detectObject, "m_CastFrameInterval", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, detectObject, "m_CastOffset", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, detectObject, "m_TriggerInteraction", container, onChangeEvent, onValidateChange);
        }

        /// <summary>
        /// Shows the spherecast elements.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="detectObject">The DetectObjectAbilityBase ability reference.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        private void ShowSphereCastFields(UnityEngine.Object unityObject, DetectObjectAbilityBase detectObject, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
        {
            container.Clear();
            if (detectObject.ObjectDetection != DetectObjectAbilityBase.ObjectDetectionMode.Spherecast) {
                return;
            }

            container.AddToClassList("indent");
            FieldInspectorView.AddField(unityObject, detectObject, "m_SpherecastRadius", container, onChangeEvent, onValidateChange);
        }

        /// <summary>
        /// Shows the trigger elements.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="detectObject">The DetectObjectAbilityBase ability reference.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        private void ShowTriggerFields(UnityEngine.Object unityObject, DetectObjectAbilityBase detectObject, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
        {
            container.Clear();
            if (detectObject.ObjectDetection != DetectObjectAbilityBase.ObjectDetectionMode.Trigger) {
                return;
            }

            container.AddToClassList("indent");
            FieldInspectorView.AddField(unityObject, detectObject, "m_MaxTriggerObjectCount", container, onChangeEvent, onValidateChange);
        }
    }
}