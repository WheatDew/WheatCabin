/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types
{
    using Opsive.Shared.Audio;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UnityEditor;
    using UnityEditor.UIElements;
    using System.Reflection;

    /// <summary>
    /// Implements TypeControlBase for the AudioClipSet ControlType.
    /// </summary>
    [ControlType(typeof(AudioClipSet))]
    public class AudioClipSetControl : TypeControlBase
    {
        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get { return false; } }

        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(TypeControlInput input)
        {
            var audioClipSet = input.Value as AudioClipSet;

            var foldout = new Foldout();
            var label = string.Empty;
            var labelOverride = input.Field.GetCustomAttribute<Opsive.Shared.Utility.LabelOverride>();
            if (labelOverride != null) {
                label = labelOverride.Text;
            } else {
                label = ObjectNames.NicifyVariableName(input.Field.Name);
            }
            foldout.text = label;
            foldout.tooltip = Inspectors.Utility.InspectorUtility.GetFieldTooltip(input.Field);
            FieldInspectorView.AddField(input.UnityObject, input.Value, "m_AudioConfig", foldout, (o) => { input.OnChangeEvent(o); });

            ReorderableList audioClips = null ;
            audioClips = new ReorderableList(audioClipSet.AudioClips, (VisualElement container, int index) =>
            {
                var objectField = new ObjectField();
                objectField.objectType = typeof(AudioClip);
                container.Add(objectField);

            }, (VisualElement container, int index) =>
            {
                var clips = audioClipSet.AudioClips;
                var objectField = container.Q<ObjectField>();
                System.Action<object> onBindingUpdateEvent = (object newValue) => objectField.SetValueWithoutNotify(newValue as AudioClip);
                objectField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(input.Field, index, input.Target, onBindingUpdateEvent);
                });
                objectField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                objectField.RegisterValueChangedCallback(c =>
                {
                    objectField.SetValueWithoutNotify(c.newValue);
                    clips[index] = c.newValue as AudioClip;
                    input.OnChangeEvent(input.Value);
                    c.StopPropagation();
                });
                objectField.SetValueWithoutNotify(audioClipSet.AudioClips[index]);
            }, (VisualElement container) =>
            {
                container.Add(new Label("Audio Clips"));
            }, (int index) =>
            {
            }, () =>
            {
                var clips = audioClipSet.AudioClips;
                System.Array.Resize(ref clips, (clips != null ? clips.Length + 1 : 1));
                audioClips.ItemsSource = audioClipSet.AudioClips = clips;
                input.OnChangeEvent(input.Value);
            }, (int index) =>
            {
                var clips = audioClipSet.AudioClips;
                ArrayUtility.RemoveAt(ref clips, index);
                audioClips.ItemsSource = audioClipSet.AudioClips = clips;
                input.OnChangeEvent(input.Value);
            }, (int fromIndex, int toIndex) => {
                input.OnChangeEvent(input.Value);
            });
            foldout.Add(audioClips);

            return foldout;
        }
    }
}