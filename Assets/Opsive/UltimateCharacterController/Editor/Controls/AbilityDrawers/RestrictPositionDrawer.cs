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
    /// Implements AbilityDrawer for the RestrictPosition ControlType.
    /// </summary>
    [ControlType(typeof(RestrictPosition))]
    public class RestrictPositionDrawer : AbilityDrawer
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
            var restrictPosition = target as RestrictPosition;

            var xContainer = new VisualElement();
            var zContainer = new VisualElement();
            
            FieldInspectorView.AddField(unityObject, target, "m_Restriction", container, (o) =>
            {
                onChangeEvent(o);
                ShowXRestriction(unityObject, restrictPosition, xContainer, onValidateChange, onChangeEvent);
                ShowZRestriction(unityObject, restrictPosition, zContainer, onValidateChange, onChangeEvent);
            }, onValidateChange);

            ShowXRestriction(unityObject, restrictPosition, xContainer, onValidateChange, onChangeEvent);
            ShowZRestriction(unityObject, restrictPosition, zContainer, onValidateChange, onChangeEvent);

            container.Add(xContainer);
            container.Add(zContainer);
        }

        /// <summary>
        /// Shows the x positional restriction elements.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="restrictPosition">The RestrictPosition ability reference.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        private void ShowXRestriction(UnityEngine.Object unityObject, RestrictPosition restrictPosition, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
        {
            container.Clear();
            if (restrictPosition.Restiction == RestrictPosition.RestrictionType.RestrictZ) {
                return;
            }

            FieldInspectorView.AddField(unityObject, restrictPosition, "m_MinXPosition", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, restrictPosition, "m_MaxXPosition", container, onChangeEvent, onValidateChange);
        }

        /// <summary>
        /// Adds the z positional restriction elements.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="restrictPosition">The RestrictPosition ability reference.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        private void ShowZRestriction(UnityEngine.Object unityObject, RestrictPosition restrictPosition, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
        {
            container.Clear();
            if (restrictPosition.Restiction == RestrictPosition.RestrictionType.RestrictX) {
                return;
            }

            FieldInspectorView.AddField(unityObject, restrictPosition, "m_MinZPosition", container, onChangeEvent, onValidateChange);
            FieldInspectorView.AddField(unityObject, restrictPosition, "m_MaxZPosition", container, onChangeEvent, onValidateChange);
        }
    }
}