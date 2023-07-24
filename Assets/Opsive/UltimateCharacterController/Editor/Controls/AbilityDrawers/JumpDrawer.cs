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
    /// Implements AbilityDrawer for the Jump ControlType.
    /// </summary>
    [ControlType(typeof(Jump))]
    public class JumpDrawer : AbilityDrawer
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
            var jump = target as Jump;

            FieldInspectorView.AddField(unityObject, target, "m_MinCeilingJumpHeight", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_GroundedGracePeriod", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_PreventSlopeLimitJump", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_Force", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_SidewaysForceMultiplier", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_BackwardsForceMultiplier", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_Frames", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ForceDamping", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_JumpEvent", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_JumpSurfaceImpact", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ForceHold", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_ForceDampingHold", container, onChangeEvent, onValidateChange);
            var airborneJumpContainer = new VisualElement();
            FieldInspectorView.AddField(unityObject, target, "m_MaxAirborneJumpCount", container, (o) =>
            {
                ShowAirborneJumpFields(unityObject, target, airborneJumpContainer, onChangeEvent, onValidateChange);
                onChangeEvent(o);
            }, onValidateChange);
            ShowAirborneJumpFields(unityObject, target, airborneJumpContainer, onChangeEvent, onValidateChange);
            container.Add(airborneJumpContainer);
            FieldInspectorView.AddField(unityObject, target, "m_VerticalVelocityStopThreshold", container, null, null);
            FieldInspectorView.AddField(unityObject, target, "m_RecurrenceDelay", container, null, null);
        }

        /// <summary>
        /// Shows the fields for the airborne jump options.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        private void ShowAirborneJumpFields(UnityEngine.Object unityObject, object target, VisualElement container, System.Action<object> onChangeEvent, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange)
        {
            var jump = target as Jump;
            container.Clear();
            if (jump.MaxAirborneJumpCount == 0) {
                return;
            }

            FieldInspectorView.AddField(unityObject, target, "m_AirborneJumpForce", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_AirborneJumpFrames", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, target, "m_AirborneJumpAudioClipSet", container, onChangeEvent, onValidateChange);
        }
    }
}