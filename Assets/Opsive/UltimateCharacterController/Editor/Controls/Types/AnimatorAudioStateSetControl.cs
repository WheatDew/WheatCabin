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
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Items.AnimatorAudioStates;
    using System;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the AnimatorAudioStateSet ControlType.
    /// </summary>
    [ControlType(typeof(AnimatorAudioStateSet))]
    public class AnimatorAudioStateSetControlType : TypeControlBase
    {
        private static List<Type> s_SelectorTypes;
        private static List<string> s_SelectorTypeNames;

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
            var container = new Foldout() { text = "Animator Audio" };
            AddSelector(input.UnityObject, input.Value, container, (o) => { input.OnChangeEvent(o); });
            AddAudioStateSetList(input.UnityObject, input.Value, container, (o) => { input.OnChangeEvent(o); });
            return container;
        }

        /// <summary>
        /// Adds the selector controls.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="container">The VisualElement that the object should be parented to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes.</param>
        private void AddSelector(UnityEngine.Object unityObject, object value, VisualElement container, System.Action<object> onChangeEvent)
        {
            PopulateAnimatorAudioStateSelectorTypes();

            var animatorAudioStateSet = value as AnimatorAudioStateSet;
            if (animatorAudioStateSet == null) {
                animatorAudioStateSet = new AnimatorAudioStateSet();
                onChangeEvent(animatorAudioStateSet);
            }

            if (s_SelectorTypes != null && s_SelectorTypes.Count > 0) {
                var abilityStarterContainer = new VisualElement();
                var selected = 0;
                var emptySelector = true;
                if (animatorAudioStateSet.AnimatorAudioStateSelector != null) {
                    for (int i = 0; i < s_SelectorTypes.Count; ++i) {
                        if (s_SelectorTypes[i].FullName == animatorAudioStateSet.AnimatorAudioStateSelector.GetType().FullName) {
                            selected = i;
                            emptySelector = false;
                            break;
                        }
                    }
                }
                if (emptySelector) {
                    animatorAudioStateSet.AnimatorAudioStateSelector = System.Activator.CreateInstance(s_SelectorTypes[0]) as AnimatorAudioStateSelector;
                }
                var dropdownField = new DropdownField("Selector", s_SelectorTypeNames, selected);
                // Ensure the control is kept up to date as the value changes.
                var selectorField = InspectorUtility.GetField(value, "m_AnimatorAudioStateSetSelector");
                if (selectorField != null) {
                    System.Action<object> onBindingUpdateEvent = (object newValue) => dropdownField.SetValueWithoutNotify(newValue as string);
                    dropdownField.RegisterCallback<AttachToPanelEvent>(c =>
                    {
                        BindingUpdater.AddBinding(selectorField, -1, value, onBindingUpdateEvent);
                    });
                    dropdownField.RegisterCallback<DetachFromPanelEvent>(c =>
                    {
                        BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                    });
                }
                dropdownField.RegisterValueChangedCallback(c =>
                {
                    // Clear out the old.
                    dropdownField.SetValueWithoutNotify(c.newValue);
                    c.StopPropagation();
                    abilityStarterContainer.Clear();

                    // Create the new starter.
                    animatorAudioStateSet.AnimatorAudioStateSelector = System.Activator.CreateInstance(s_SelectorTypes[dropdownField.index]) as AnimatorAudioStateSelector;
                    FieldInspectorView.AddFields(unityObject, animatorAudioStateSet.AnimatorAudioStateSelector, Shared.Utility.MemberVisibility.Public, abilityStarterContainer, o => { onChangeEvent(animatorAudioStateSet); });
                    onChangeEvent(animatorAudioStateSet);
                });
                container.Add(dropdownField);
                container.Add(abilityStarterContainer);

                if (animatorAudioStateSet.AnimatorAudioStateSelector != null) {
                    FieldInspectorView.AddFields(unityObject, animatorAudioStateSet.AnimatorAudioStateSelector, Shared.Utility.MemberVisibility.Public, abilityStarterContainer, o => { onChangeEvent(animatorAudioStateSet); });
                }
            }
        }

        /// <summary>
        /// Searches for and adds any AnimatorAudioStateSelectors available in the project.
        /// </summary>
        private static void PopulateAnimatorAudioStateSelectorTypes()
        {
            if (s_SelectorTypes != null) {
                return;
            }

            s_SelectorTypes = new List<Type>();
            s_SelectorTypeNames = new List<string>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must derive from AnimatorAudioStateSelector.
                    if (!typeof(AnimatorAudioStateSelector).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    s_SelectorTypes.Add(assemblyTypes[j]);
                    s_SelectorTypeNames.Add(InspectorUtility.DisplayTypeName(assemblyTypes[j], false));
                }
            }
        }

        /// <summary>
        /// Adds the ReorderableList for the AudioStateSet.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="container">The VisualElement that the object should be parented to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes.</param>
        private void AddAudioStateSetList(UnityEngine.Object unityObject, object value, VisualElement container, System.Action<object> onChangeEvent)
        {
            var animatorAudioStateSet = value as AnimatorAudioStateSet;

            var selectedStateContainer = new VisualElement();
            ReorderableList reorderableList = null;
            reorderableList = new ReorderableList(animatorAudioStateSet.States, (VisualElement container, int index) => // Add.
            {
                var element = new VisualElement();
                element.AddToClassList("horizontal-layout");
                var label = new Label();
                label.AddToClassList("flex-grow");
                element.Add(label);
                var toggle = new Toggle();
                toggle.name = "serialized-reference-list-right-action-object";
                element.Add(toggle);
                container.Add(element);
            }, (VisualElement container, int index) => // Bind.
            {
                var label = container.Q<Label>();
                var stateName = animatorAudioStateSet.States[index].StateName;
                var substateIndex = animatorAudioStateSet.States[index].ItemSubstateIndex;
                if (string.IsNullOrWhiteSpace(stateName)) {
                    label.text = substateIndex.ToString();  
                } else {
                    label.text = substateIndex.ToString() + $" ({stateName})";
                }
                var indexField = InspectorUtility.GetField(animatorAudioStateSet.States[index], "m_ItemSubstateIndex");
                Action<object> indexBindingUpdateEvent = (object newValue) =>
                {
                    label.text = animatorAudioStateSet.States[index].ItemSubstateIndex.ToString();
                };
                label.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(indexField, -1, animatorAudioStateSet.States[index], indexBindingUpdateEvent);
                });
                label.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(indexBindingUpdateEvent);
                });

                // Map the toggle to the enabled bool.
                var toggle = container.Q<Toggle>();
                toggle.SetValueWithoutNotify(animatorAudioStateSet.States[index].Enabled);
                var enableField = InspectorUtility.GetField(animatorAudioStateSet.States[index], "m_Enabled");
                Action<object> enabledBindingUpdateEvent = (object newValue) =>
                {
                    toggle.SetValueWithoutNotify((bool)newValue);
                };
                toggle.RegisterValueChangedCallback<bool>((c) =>
                {
                    animatorAudioStateSet.States[index].Enabled = c.newValue;
                    Shared.Editor.Utility.EditorUtility.SetDirty(unityObject);
                });
                toggle.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(enableField, -1, animatorAudioStateSet.States[index], enabledBindingUpdateEvent);
                });
                toggle.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(enabledBindingUpdateEvent);
                });
            }, (VisualElement container) => // Header.
            {
                var horizontalLayout = new VisualElement();
                horizontalLayout.AddToClassList("horizontal-layout");
                container.Add(horizontalLayout);

                var label = new Label("Animator Audio States");
                label.AddToClassList("flex-grow");
                horizontalLayout.Add(label);
                label = new Label("Enabled");
                label.name = "serialized-reference-list-right-label";
                horizontalLayout.Add(label);
            }, (int index) => // Select.
            {
                selectedStateContainer.Clear();
                var state = animatorAudioStateSet.States[index];
                if (state == null) {
                    return;
                }

                FieldInspectorView.AddFields(unityObject, state, Shared.Utility.MemberVisibility.Public, selectedStateContainer, (obj) =>
                {
                    Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(unityObject, "Change Value");
                    onChangeEvent(animatorAudioStateSet);
                }, null, null, true);
            }, () => // Add.
            {
                var animatorAudioStateSet = value as AnimatorAudioStateSet;
                var states = animatorAudioStateSet.States;
                if (states == null) {
                    states = new AnimatorAudioStateSet.AnimatorAudioState[1];
                } else {
                    Array.Resize(ref states, states.Length + 1);
                }
                states[states.Length - 1] = Activator.CreateInstance(typeof(AnimatorAudioStateSet.AnimatorAudioState)) as AnimatorAudioStateSet.AnimatorAudioState;
                reorderableList.ItemsSource = animatorAudioStateSet.States = states;
                Shared.Editor.Utility.EditorUtility.SetDirty(unityObject);

                // Select the newly added effect.
                reorderableList.SelectedIndex = states.Length - 1;
            }, (int index) => // Remove.
            {
                var stateList = new List<AnimatorAudioStateSet.AnimatorAudioState>(animatorAudioStateSet.States);
                stateList.RemoveAt(reorderableList.SelectedIndex);
                reorderableList.ItemsSource = animatorAudioStateSet.States = stateList.ToArray();
                Shared.Editor.Utility.EditorUtility.SetDirty(unityObject);
                selectedStateContainer.Clear();

                // Update the index to point to no longer point to the now deleted ability.
                var selectedIndex = index - 1;
                if (selectedIndex == -1 && animatorAudioStateSet.States.Length > 0) {
                    selectedIndex = 0;
                }
                reorderableList.SelectedIndex = selectedIndex;
            }, (int fromIndex, int toIndex) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(unityObject);
            });
            container.Add(reorderableList);
            container.Add(selectedStateContainer);
        }
    }
}