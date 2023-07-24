/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the AnimationEventTrigger ControlType.
    /// </summary>
    [ControlType(typeof(AnimationEventTrigger))]
    [ControlType(typeof(AnimationSlotEventTrigger))]
    public class AnimationEventTriggerControlType : TypeControlBase
    {
        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get => false; }

        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(TypeControlInput input)
        {
            var animationEventTrigger = input.Value as AnimationEventTrigger;
            if (animationEventTrigger == null) {
                input.Value = animationEventTrigger = new AnimationEventTrigger();
                input.OnChangeEvent(animationEventTrigger);
            }
            var container = new Foldout() { text = UnityEditor.ObjectNames.NicifyVariableName(input.Field.Name) };
            var durationContainer = new VisualElement();
            VisualElement slotContainer = null;
            FieldInspectorView.AddField(input.UnityObject, input.Value, "m_WaitForAnimationEvent", container, c => { input.OnChangeEvent(input.Value);
                durationContainer.style.display = animationEventTrigger.WaitForAnimationEvent ? DisplayStyle.None : DisplayStyle.Flex; 
                if (slotContainer != null) {
                    slotContainer.style.display = animationEventTrigger.WaitForAnimationEvent ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }, null, true);
            durationContainer.style.display = animationEventTrigger.WaitForAnimationEvent ? DisplayStyle.None : DisplayStyle.Flex;
            container.Add(durationContainer);
            FieldInspectorView.AddField(input.UnityObject, input.Value, "m_Duration", durationContainer, c => { input.OnChangeEvent(c); }, null, true, null, false);
            if (animationEventTrigger is AnimationSlotEventTrigger) {
                slotContainer = new VisualElement();
                slotContainer.style.display = animationEventTrigger.WaitForAnimationEvent ? DisplayStyle.Flex : DisplayStyle.None;
                container.Add(slotContainer);
                FieldInspectorView.AddField(input.UnityObject, input.Value, "m_WaitForSlotEvent", slotContainer, c => { input.OnChangeEvent(input.Value); });
            }
            return container;
        }
    }
}