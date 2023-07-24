/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types.AbilityDrawers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Starters;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Editor.Utility;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the Ability ControlType.
    /// </summary>
    [ControlType(typeof(Ability))]
    public class AbilityControlType : StateObjectControlType
    {
        private const string c_EditorPrefsLastLastAnimatorCodePathKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Ability.LastAnimatorCodePath";

        private VisualElement m_AbilityStarterContainer;
        private VisualElement m_InputNamesContainer;

        private static List<System.Type> s_AbilityStarterTypes;
        private static List<string> s_AbilityStarterTypeNames;

        /// <summary>
        /// Returns the header control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="field">The field responsible for the control (can be null).</param>
        /// <param name="serializedProperty">The SerializedProperty bound to the field (can be null).</param>
        /// <param name="arrayIndex">The index of the object within the array (-1 indicates no array).</param>
        /// <param name="type">The type of control being retrieved.</param>
        /// <param name="value">The value of the control.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="userData">Optional data which can be used by the controls.</param>
        /// <returns>The created control.</returns>
        public override VisualElement GetHeaderControl(UnityEngine.Object unityObject, object target, FieldInfo field, UnityEditor.SerializedProperty serializedProperty,
                                                        int arrayIndex, System.Type type, object value, System.Func<object, bool> onChangeEvent, object userData)
        {
            var container = new VisualElement();
            ShowInputFieldsFields(unityObject, target, container, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_AttributeModifier", container, (o) => { onChangeEvent(o); });
            var stateNameContainer = new VisualElement();
            stateNameContainer.AddToClassList("horizontal-layout");
            VisualElement appendItemElement = null;
            FieldInspectorView.AddField(unityObject, target, "m_State", stateNameContainer, (o) => {
                onChangeEvent(o);
                appendItemElement.SetEnabled(!string.IsNullOrEmpty((value as Ability).State));
            });
            FieldInspectorView.AddField(unityObject, target, "m_StateAppendItemIdentifierName", stateNameContainer, (o) => { onChangeEvent(o); }, null, false, null, false);
            stateNameContainer.Query<Label>().ForEach((label) =>
            {
                if (label.text.Contains("Append")) {
                    label.style.minWidth = 0;
                    appendItemElement = label.parent; // LabelControl.
                    appendItemElement.RemoveFromClassList("flex-grow");
                    appendItemElement.style.flexGrow = 0;
                    appendItemElement.AddToClassList("flex-shrink");
                    appendItemElement.SetEnabled(!string.IsNullOrEmpty((value as Ability).State));
                }
            });
            container.Add(stateNameContainer);
            FieldInspectorView.AddField(unityObject, target, "m_AbilityIndexParameter", container, (o) => { onChangeEvent(o); });

            var abilityDrawer = AbilityDrawerUtility.FindAbilityDrawer(type, true);
            if (abilityDrawer != null) {
                abilityDrawer.CreateDrawer(unityObject, target, container, null, (o) =>
                {
                    onChangeEvent(o);
                });
            } else {
                FieldInspectorView.AddFields(unityObject, target, Shared.Utility.MemberVisibility.Public, container, (o) => { onChangeEvent(o); }, null, null, false, null, true);
            }
            var effectContainer = new VisualElement();
            FieldInspectorView.AddField(unityObject, target, "m_StartEffectName", container, (o) => {
                effectContainer.style.display = (string.IsNullOrEmpty((target as Ability).StartEffectName) ? DisplayStyle.None : DisplayStyle.Flex);
                onChangeEvent(o);
            });
            FieldInspectorView.AddField(unityObject, target, "m_StartEffectIndex", effectContainer, (o) => { onChangeEvent(o); });
            effectContainer.style.display = (string.IsNullOrEmpty((target as Ability).StartEffectName) ? DisplayStyle.None : DisplayStyle.Flex);
            container.Add(effectContainer);

            // Show the audio settings.
            var foldout = new Foldout() { text = "Audio" };
            FieldInspectorView.AddField(unityObject, target, "m_StartAudioClipSet", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_StopAudioClipSet", foldout, (o) => { onChangeEvent(o); });
            container.Add(foldout);

            // General settings.
            foldout = new Foldout() { text = "General" };
            FieldInspectorView.AddField(unityObject, target, "m_InspectorDescription", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_AllowPositionalInput", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_AllowRotationalInput", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_UseGravity", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_UseRootMotionPosition", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_UseRootMotionRotation", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_DetectHorizontalCollisions", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_DetectVerticalCollisions", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_DetectGroundCollisions", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_AnimatorMotion", foldout, (o) => { onChangeEvent(o); });
            var equipmentContainer = new VisualElement();
            ShowEquipmentFields(unityObject, target, equipmentContainer, (o) => onChangeEvent(o));
            foldout.Add(equipmentContainer);
            container.Add(foldout);

            // UI settings.
            foldout = new Foldout() { text = "UI" };
            FieldInspectorView.AddField(unityObject, target, "m_AbilityMessageText", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_AbilityMessageIcon", foldout, (o) => { onChangeEvent(o); });
            container.Add(foldout);

            // Editor settings.
            foldout = new Foldout() { text = "Editor" };
            ShowEditorFields(unityObject, target, foldout, (o) => { onChangeEvent(o); }, (Inspectors.Character.UltimateCharacterLocomotionInspector)userData);
            container.Add(foldout);

            return container;
        }

        /// <summary>
        /// Draws the Ability fields related to input.
        /// </summary>
        /// <param name="unityObject">The Unity Object that the target belongs to.</param>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="container">The container UIElement.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes.</param>
        protected void ShowInputFieldsFields(Object unityObject, object target, VisualElement container, System.Action<object> onChangeEvent)
        {
            var ability = target as Ability;
            FieldInspectorView.AddField(unityObject, ability, "m_StartType", container, (o) =>
            {
                m_AbilityStarterContainer.Clear();
                if (ability.StartType == Ability.AbilityStartType.Custom) {
                    ShowAbilityStarter(unityObject, target, m_AbilityStarterContainer, onChangeEvent);
                } else {
                    ability.AbilityStarter = null;
                }
                m_InputNamesContainer.Clear();
                ShowInputNames(unityObject, target, m_InputNamesContainer, onChangeEvent);
                onChangeEvent(o);
            });

            m_AbilityStarterContainer = new VisualElement();
            ShowAbilityStarter(unityObject, target, m_AbilityStarterContainer, onChangeEvent);
            container.Add(m_AbilityStarterContainer);

            FieldInspectorView.AddField(unityObject, ability, "m_StopType", container, (o) =>
            {
                m_InputNamesContainer.Clear();
                ShowInputNames(unityObject, target, m_InputNamesContainer, onChangeEvent);
            });

            m_InputNamesContainer = new VisualElement();
            ShowInputNames(unityObject, target, m_InputNamesContainer, onChangeEvent);
            container.Add(m_InputNamesContainer);
        }

        /// <summary>
        /// Shows the UIElements for the ability starter.
        /// </summary>
        /// <param name="unityObject">The Unity Object that the target belongs to.</param>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="container">The container UIElement.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes.</param>
        private void ShowAbilityStarter(Object unityObject, object target, VisualElement container, System.Action<object> onChangeEvent)
        {
            var ability = target as Ability;

            if (ability.StartType != Ability.AbilityStartType.Custom) {
                return;
            }

            PopulateAbilityStarterTypes();
            if (s_AbilityStarterTypes != null && s_AbilityStarterTypes.Count > 0) {
                var abilityStarterContainer = new VisualElement();
                abilityStarterContainer.AddToClassList("indent");
                var selected = 0;
                var emptyStarter = true;
                if (ability.AbilityStarter != null) {
                    for (int i = 0; i < s_AbilityStarterTypes.Count; ++i) {
                        if (s_AbilityStarterTypes[i].FullName == ability.AbilityStarter.GetType().FullName) {
                            selected = i;
                            emptyStarter = false;
                            break;
                        }
                    }
                }
                if (emptyStarter) {
                    ability.AbilityStarter = System.Activator.CreateInstance(s_AbilityStarterTypes[0]) as AbilityStarter;
                }
                var dropdownField = new DropdownField(s_AbilityStarterTypeNames, selected);
                // Ensure the control is kept up to date as the value changes.
                var starterField = InspectorUtility.GetField(target, "m_AbilityStarter");
                if (starterField != null) {
                    System.Action<object> onBindingUpdateEvent = (object newValue) => dropdownField.SetValueWithoutNotify(newValue as string);
                    dropdownField.RegisterCallback<AttachToPanelEvent>(c =>
                    {
                        BindingUpdater.AddBinding(starterField, -1, target, onBindingUpdateEvent);
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
                    ability.AbilityStarter = System.Activator.CreateInstance(s_AbilityStarterTypes[dropdownField.index]) as AbilityStarter;
                    FieldInspectorView.AddFields(unityObject, ability.AbilityStarter, Shared.Utility.MemberVisibility.Public, abilityStarterContainer, (object obj) =>
                    {
                        ability.AbilityStarter = obj as AbilityStarter;
                        onChangeEvent(ability);
                    });
                    onChangeEvent(ability);
                });
                var labelControl = new LabelControl("Starter", Opsive.Shared.Editor.Utility.EditorUtility.GetTooltip(typeof(Ability).GetField("m_AbilityStarter", BindingFlags.NonPublic | BindingFlags.Instance)), dropdownField, true);
                labelControl.Q<Label>().AddToClassList("indent");
                container.Add(labelControl);
                container.Add(abilityStarterContainer);

                if (ability.AbilityStarter != null) {
                    FieldInspectorView.AddFields(unityObject, ability.AbilityStarter, Shared.Utility.MemberVisibility.Public, abilityStarterContainer, (object obj) =>
                    {
                        ability.AbilityStarter = obj as AbilityStarter;
                        onChangeEvent(ability);
                    });
                }
            }
        }

        /// <summary>
        /// Searches for an adds any Starter Types available in the project.
        /// </summary>
        private static void PopulateAbilityStarterTypes()
        {
            if (s_AbilityStarterTypes != null) {
                return;
            }

            s_AbilityStarterTypes = new List<System.Type>();
            s_AbilityStarterTypeNames = new List<string>();
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must derive from AbilityStarter.
                    if (!typeof(AbilityStarter).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    s_AbilityStarterTypes.Add(assemblyTypes[j]);
                    s_AbilityStarterTypeNames.Add(InspectorUtility.DisplayTypeName(assemblyTypes[j], false));
                }
            }
        }

        /// <summary>
        /// Shows the UIElements for the input names.
        /// </summary>
        /// <param name="unityObject">The Unity Object that the target belongs to.</param>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="container">The container UIElement.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes.</param>
        private void ShowInputNames(Object unityObject, object target, VisualElement container, System.Action<object> onChangeEvent)
        {
            var ability = target as Ability;

            if ((ability.StartType == Ability.AbilityStartType.Automatic || ability.StartType == Ability.AbilityStartType.Manual || ability.StartType == Ability.AbilityStartType.Custom) &&
                (ability.StopType == Ability.AbilityStopType.Automatic || ability.StopType == Ability.AbilityStopType.Manual)) {
                return;
            }

            FieldInspectorView.AddField(unityObject, ability, "m_InputNames", container, onChangeEvent);

            // Only show the duration and wait for release options with a LongPress start/stop type.
            if (ability.StartType == Ability.AbilityStartType.LongPress || ability.StopType == Ability.AbilityStopType.LongPress) {
                FieldInspectorView.AddField(unityObject, target, "m_LongPressDuration", container, onChangeEvent);
                FieldInspectorView.AddField(unityObject, target, "m_WaitForLongPressRelease", container, onChangeEvent);
            }
        }

        /// <summary>
        /// Shows the UIElements for the equipment fields.
        /// </summary>
        /// <param name="unityObject">The Unity Object that the target belongs to.</param>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="container">The container UIElement.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes.</param>
        private void ShowEquipmentFields(Object unityObject, object target, VisualElement container, System.Action<object> onChangeEvent)
        {
            container.Clear();

            var inventory = (unityObject as Component).GetComponent<Inventory.InventoryBase>();
            if (inventory == null || (unityObject as Component).GetComponent<Inventory.ItemSetManagerBase>() == null) {
                return;
            }

            var foldout = new Foldout() { text = "Allow Equipped Items" };
            var slotCount = inventory.SlotCount;

            var mask = InspectorUtility.GetFieldValue<int>(target, "m_AllowEquippedSlotsMask");
            var field = InspectorUtility.GetField(target, "m_AllowEquippedSlotsMask");
            for (int i = 0; i < slotCount; ++i) {
                var index = i;
                var toggle = new Toggle();
                toggle.text = "Slot " + index;
                toggle.value = (mask & (1 << index)) == (1 << index);
                toggle.RegisterValueChangedCallback(c =>
                {
                    toggle.SetValueWithoutNotify(c.newValue);
                    if (c.newValue) {
                        (target as Ability).AllowEquippedSlotsMask |= 1 << index;
                    } else {
                        (target as Ability).AllowEquippedSlotsMask &= ~(1 << index);
                    }
                    onChangeEvent(target);
                });
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    var enabled = ((target as Ability).AllowEquippedSlotsMask  & (1 << index)) == (1 << index);
                    toggle.SetValueWithoutNotify(enabled);
                };
                toggle.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(field, -1, target, onBindingUpdateEvent);
                });
                toggle.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                foldout.Add(toggle);
            }

            container.Add(foldout);
            FieldInspectorView.AddField(unityObject, target, "m_AllowItemDefinitions", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_ImmediateUnequip", foldout, (o) => { onChangeEvent(o); });
            FieldInspectorView.AddField(unityObject, target, "m_ReequipSlots", foldout, (o) => { onChangeEvent(o); });
        }

        /// <summary>
        /// Shows the editor fields for trhe ability.
        /// </summary>
        /// <param name="unityObject">The Unity Object that the target belongs to.</param>
        /// <param name="target">The object that is being drawn.</param>
        /// <param name="container">The container UIElement.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes.</param>
        protected virtual void ShowEditorFields(Object unityObject, object target, VisualElement container, System.Action<object> onChangeEvent, 
                                                    Inspectors.Character.UltimateCharacterLocomotionInspector inspector)
        {
            var ability = target as Ability;
            var abilityDrawer = AbilityDrawerUtility.FindAbilityDrawer(ability.GetType(), true);
            if (abilityDrawer != null) {
                abilityDrawer.CreateEditorDrawer(unityObject, ability, container, null, (o) =>
                {
                    onChangeEvent(o);
                });
            }

            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            container.Add(horizontalLayout);

            var generateButton = new Button();
            generateButton.text = "Generate Animator Controller";
            generateButton.clicked += () =>
            {
                UnityEditor.Animations.AnimatorController[] animatorControllers = null;
                UnityEditor.Animations.AnimatorController[] firstPersonAnimatorControllers = null;
                GetAnimatorControllers(unityObject as UltimateCharacterLocomotion, ref animatorControllers, ref firstPersonAnimatorControllers);

                if (animatorControllers != null || firstPersonAnimatorControllers != null) {
                    var baseDirectory = EditorPrefs.GetString(c_EditorPrefsLastLastAnimatorCodePathKey, "Assets");
                    var path = AnimatorBuilder.GenerateAnimatorCode(animatorControllers, firstPersonAnimatorControllers, "AbilityIndex", ability.AbilityIndexParameter, ability, baseDirectory);
                    if (!string.IsNullOrEmpty(path)) {
                        EditorPrefs.SetString(c_EditorPrefsLastLastAnimatorCodePathKey, System.IO.Path.GetFullPath(path.Replace(Application.dataPath, "Assets")));
                    }
                }
            };
            generateButton.style.flexGrow = 1;
            generateButton.SetEnabled(ability.AbilityIndexParameter != -1);
            horizontalLayout.Add(generateButton);

            var buildAnimatorButton = new Button();
            buildAnimatorButton.text = "Build Animator";
            buildAnimatorButton.clicked += () =>
            {
                UnityEditor.Animations.AnimatorController[] animatorControllers = null;
                UnityEditor.Animations.AnimatorController[] firstPersonAnimatorControllers = null;
                GetAnimatorControllers(unityObject as UltimateCharacterLocomotion, ref animatorControllers, ref firstPersonAnimatorControllers);

                if (animatorControllers != null || firstPersonAnimatorControllers != null) {
                    abilityDrawer.BuildAnimator(animatorControllers, firstPersonAnimatorControllers);
                }
            };
            buildAnimatorButton.style.flexGrow = 1;
            buildAnimatorButton.SetEnabled(abilityDrawer != null && abilityDrawer.CanBuildAnimator);
            horizontalLayout.Add(buildAnimatorButton);
        }

        /// <summary>
        /// Retrieves the Animator Controllers on the specified Ultimate Character Locomotion.
        /// </summary>
        /// <param name="characterLocomotion">The Ultimate Character Locomotion that the Animator Controllers should be retrieved from.</param>
        /// <param name="animatorControllers">The character Animator Controllers.</param>
        /// <param name="firstPersonAnimatorControllers">The first person Animator Controllers.</param>
        private void GetAnimatorControllers(UltimateCharacterLocomotion characterLocomotion, ref UnityEditor.Animations.AnimatorController[] animatorControllers, ref UnityEditor.Animations.AnimatorController[] firstPersonAnimatorControllers)
        {
            var animatorMonitors = characterLocomotion.GetComponentsInChildren<AnimatorMonitor>();
            if (animatorMonitors != null) {
                animatorControllers = new UnityEditor.Animations.AnimatorController[animatorMonitors.Length];
                for (int i = 0; i < animatorMonitors.Length; ++i) {
                    var animator = animatorMonitors[i].GetComponent<Animator>();
                    if (animator == null) {
                        continue;
                    }

                    animatorControllers[i] = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                }
            }

#if FIRST_PERSON_CONTROLLER
            var firstPersonBaseObjects = characterLocomotion.GetComponentsInChildren<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>();
            if (firstPersonBaseObjects != null) {
                firstPersonAnimatorControllers = new UnityEditor.Animations.AnimatorController[firstPersonBaseObjects.Length];
                for (int i = 0; i < firstPersonBaseObjects.Length; ++i) {
                    var animator = firstPersonBaseObjects[i].GetComponent<Animator>();
                    if (animator == null) {
                        continue;
                    }

                    firstPersonAnimatorControllers[i] = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                }
            }
#endif
        }
    }
}