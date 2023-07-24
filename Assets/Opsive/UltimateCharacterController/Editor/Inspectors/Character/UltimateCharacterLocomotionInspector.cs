/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Character.Effects;
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Utility.Builders;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shows a custom inspector for the UltimateCharacterLocomotion component.
    /// </summary>
    [CustomEditor(typeof(UltimateCharacterLocomotion), true)]
    public class UltimateCharacterLocomotionInspector : CharacterLocomotionInspector
    {
        private const string c_EditorPrefsSelectedMovementTypeIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedMovementTypeIndex";
        private const string c_EditorPrefsSelectedAbilityIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Character.SelectedAbilityIndex";
        private const string c_EditorPrefsSelectedItemAbilityIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Character.SelectedItemAbilityIndex";
        private const string c_EditorPrefsSelectedEffectIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.SelectedEffectIndex";
        private string SelectedMovementTypeIndexKey { get { return c_EditorPrefsSelectedMovementTypeIndexKey + "." + target.GetType() + "." + target.name; } }
        private string SelectedAbilityIndexKey { get { return c_EditorPrefsSelectedAbilityIndexKey + "." + target.GetType() + "." + target.name; } }
        private string SelectedItemAbilityIndexKey { get { return c_EditorPrefsSelectedItemAbilityIndexKey + "." + target.GetType() + "." + target.name; } }
        private string SelectedEffectIndexKey { get { return c_EditorPrefsSelectedEffectIndexKey + "." + target.GetType() + "." + target.name; } }

        private UltimateCharacterLocomotion m_CharacterLocomotion;

        private VisualElement m_FirstPersonDropdownContainer;
        private VisualElement m_ThirdPersonDropdownContainer;
        private ReorderableList m_MovementTypeReorderableList;
        private ReorderableList m_AbilityReorderableList;
        private ReorderableList m_ItemAbilityReorderableList;
        private ReorderableList m_EffectsReorderableList;
        private VisualElement m_SelectedMovementTypeContainer;
        private VisualElement m_SelectedAbilityContainer;
        private VisualElement m_SelectedItemAbilityContainer;
        private VisualElement m_SelectedEffectContainer;

        private List<string> m_FirstPersonMovementTypeNames;
        private List<string> m_ThirdPersonMovementTypeNames;
        private Type[] m_FirstPersonMovementTypes;
        private Type[] m_ThirdPersonMovementTypes;

        private Dictionary<Ability, Label> m_LabelByAbilityMap = new Dictionary<Ability, Label>();
        private Dictionary<Effect, Label> m_LabelByEffectMap = new Dictionary<Effect, Label>();
        private float m_LastUpdateTime;

        /// <summary>
        /// Initialize the inspector.
        /// </summary>
        protected override void InitializeInspector()
        {
            base.InitializeInspector();

            m_CharacterLocomotion = target as UltimateCharacterLocomotion;
        }

        /// <summary>
        /// Draws the custom UIElements to the top of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowHeaderElements(VisualElement container)
        {
            m_FirstPersonDropdownContainer = new VisualElement();
            container.Add(m_FirstPersonDropdownContainer);
            m_ThirdPersonDropdownContainer = new VisualElement();
            container.Add(m_ThirdPersonDropdownContainer);
            AddMovementTypeDropdowns();
            AddMovementTypeList(container);

            base.ShowHeaderElements(container);
        }

        /// <summary>
        /// Adds the first or third person perspective dropdowns.
        /// </summary>
        private void AddMovementTypeDropdowns()
        {
            UpdateDefaultMovementTypes();

            // Only show the first/third person movement type popup if that movement type is available.
            m_FirstPersonDropdownContainer.Clear();
            if (m_FirstPersonMovementTypeNames != null && m_FirstPersonMovementTypeNames.Count > 0) {
                var selectedIndex = 0;
                for (int i = 0; i < m_FirstPersonMovementTypes.Length; ++i) {
                    if (m_FirstPersonMovementTypes[i].FullName == m_CharacterLocomotion.FirstPersonMovementTypeFullName) {
                        selectedIndex = i;
                        break;
                    }
                }
                var dropdownField = new DropdownField(m_FirstPersonMovementTypeNames, selectedIndex);
                dropdownField.RegisterValueChangedCallback(c =>
                {
                    m_CharacterLocomotion.FirstPersonMovementTypeFullName = m_FirstPersonMovementTypes[dropdownField.index].FullName;
                    // Update the default movement type if the current movement type is first person. Do not update when playing because the first person property will update the current type.
                    if (Application.isPlaying && m_CharacterLocomotion.ActiveMovementType.GetType().FullName.Contains("FirstPerson")) {
                        m_CharacterLocomotion.MovementTypeFullName = m_CharacterLocomotion.FirstPersonMovementTypeFullName;
                    }
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);
                });
                var field = InspectorUtility.GetField(target, "m_FirstPersonMovementTypeFullName");
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    dropdownField.SetValueWithoutNotify(InspectorUtility.DisplayTypeName(Shared.Utility.TypeUtility.GetType(m_CharacterLocomotion.FirstPersonMovementTypeFullName), false));
                };
                dropdownField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(field, -1, target, onBindingUpdateEvent);
                });
                dropdownField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                var labelControl = new LabelControl("First Person Movement Type", InspectorUtility.GetFieldTooltip(field), dropdownField, true);
                m_FirstPersonDropdownContainer.Add(labelControl);
            }

            m_ThirdPersonDropdownContainer.Clear();
            if (m_ThirdPersonMovementTypeNames != null && m_ThirdPersonMovementTypeNames.Count > 0) {
                var selectedIndex = 0;
                for (int i = 0; i < m_ThirdPersonMovementTypes.Length; ++i) {
                    if (m_ThirdPersonMovementTypes[i].FullName == m_CharacterLocomotion.ThirdPersonMovementTypeFullName) {
                        selectedIndex = i;
                        break;
                    }
                }
                var dropdownField = new DropdownField(m_ThirdPersonMovementTypeNames, selectedIndex);
                dropdownField.RegisterValueChangedCallback(c =>
                {
                    m_CharacterLocomotion.ThirdPersonMovementTypeFullName = m_ThirdPersonMovementTypes[dropdownField.index].FullName;
                    // Update the default movement type if the current movement type is third person. Do not update when playing because the third person property will update the current type.
                    if (Application.isPlaying && m_CharacterLocomotion.ActiveMovementType.GetType().FullName.Contains("ThirdPerson")) {
                        m_CharacterLocomotion.MovementTypeFullName = m_CharacterLocomotion.ThirdPersonMovementTypeFullName;
                    }
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);
                });
                var field = InspectorUtility.GetField(target, "m_ThirdPersonMovementTypeFullName");
                System.Action<object> onBindingUpdateEvent = (object newValue) => {
                    dropdownField.SetValueWithoutNotify(InspectorUtility.DisplayTypeName(Shared.Utility.TypeUtility.GetType(m_CharacterLocomotion.ThirdPersonMovementTypeFullName), false));
                };
                dropdownField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(field, -1, target, onBindingUpdateEvent);
                });
                dropdownField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                var labelControl = new LabelControl("Third Person Movement Type", InspectorUtility.GetFieldTooltip(field), dropdownField, true);
                m_ThirdPersonDropdownContainer.Add(labelControl);
            }
        }

        /// <summary>
        /// Adds the movement type ReorderableList.
        /// </summary>
        /// <param name="container">The parent container.</param>
        private void AddMovementTypeList(VisualElement container)
        {
            var foldout = PersistentFoldout("Movement Types");
            foldout.contentContainer.AddToClassList("contained-list");
            container.Add(foldout);

            m_MovementTypeReorderableList = new ReorderableList(m_CharacterLocomotion.MovementTypes, (VisualElement container, int index) => // Add.
            {
                var element = new VisualElement();
                element.AddToClassList("horizontal-layout");
                var label = new Label();
                label.AddToClassList("flex-grow");
                element.Add(label);
                var radioButton = new RadioButton();
                radioButton.name = "serialized-reference-list-right-action-object";
                element.Add(radioButton);
                container.Add(element);
            }, (VisualElement container, int index) => // Bind.
            {
                var label = container.Q<Label>();
                if (m_CharacterLocomotion.MovementTypes[index] == null) {
                    label.text = "(Unknown Movement Type)";
                    return;
                }
                label.text = InspectorUtility.DisplayTypeName(m_CharacterLocomotion.MovementTypes[index].GetType(), true);

                // Map the radio button to the active movement type.
                var radioButton = container.Q<RadioButton>();
                radioButton.SetValueWithoutNotify(m_CharacterLocomotion.MovementTypeFullName == m_CharacterLocomotion.MovementTypes[index].GetType().FullName);
                var activeMovementTypeField = InspectorUtility.GetField(m_CharacterLocomotion, "m_MovementType");
                Action<object> movementTypeBindingUpdateEvent = (object newValue) =>
                {
                    radioButton.SetValueWithoutNotify(m_CharacterLocomotion.MovementTypeFullName == m_CharacterLocomotion.MovementTypes[index].GetType().FullName);
                };
                radioButton.RegisterValueChangedCallback<bool>((c) =>
                {
                    if (c.newValue == false) {
                        return;
                    }

                    m_CharacterLocomotion.MovementTypeFullName = m_CharacterLocomotion.MovementTypes[index].GetType().FullName;
                    if (m_CharacterLocomotion.MovementTypes[index].GetType().FullName.Contains("FirstPerson")) {
                        m_CharacterLocomotion.FirstPersonMovementTypeFullName = m_CharacterLocomotion.MovementTypeFullName;
                    } else if (m_CharacterLocomotion.MovementTypes[index].GetType().FullName.Contains("ThirdPerson")) {
                        m_CharacterLocomotion.ThirdPersonMovementTypeFullName = m_CharacterLocomotion.MovementTypeFullName;
                    }
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);
                });
                radioButton.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(activeMovementTypeField, -1, m_CharacterLocomotion, movementTypeBindingUpdateEvent);
                });
                radioButton.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(movementTypeBindingUpdateEvent);
                });
            }, (VisualElement container) => // Header.
            {
                var horizontalLayout = new VisualElement();
                horizontalLayout.AddToClassList("horizontal-layout");
                container.Add(horizontalLayout);

                var label = new Label("Movement Types");
                label.AddToClassList("flex-grow");
                horizontalLayout.Add(label);
                label = new Label("Active");
                label.name = "serialized-reference-list-right-label";
                horizontalLayout.Add(label);
            }, (int index) => // Select.
            {
                if (ShowMovementType(index)) {
                    EditorPrefs.SetInt(SelectedMovementTypeIndexKey, index);
                }
            }, () => // Add.
            {
                ReorderableListSerializationHelper.AddObjectType(typeof(MovementType), true, m_CharacterLocomotion.MovementTypes, AddMovementType);
            }, (int index) => // Remove.
            {
                var movementTypes = new List<MovementType>(m_CharacterLocomotion.MovementTypes);
                // Select a new movement type if the currently selected movement type is being removed.
                var removedMovementType = movementTypes[index].GetType().FullName;
                var removedSelected = movementTypes[index].GetType().FullName == m_CharacterLocomotion.MovementTypeFullName;

                // Remove the element.
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");
                movementTypes.RemoveAt(index);
                m_MovementTypeReorderableList.ItemsSource = m_CharacterLocomotion.MovementTypes = movementTypes.ToArray();
                Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);

                // Don't allow remove if there is only one movement type left.
                m_MovementTypeReorderableList.EnableRemove = m_CharacterLocomotion.MovementTypes.Length > 1;

                // Update the index to point to no longer point to the now deleted movement type.
                m_MovementTypeReorderableList.SelectedIndex = index - 1;
                if (m_MovementTypeReorderableList.SelectedIndex == -1 && movementTypes.Count > 0) {
                    m_MovementTypeReorderableList.SelectedIndex = 0;
                }
                if (removedSelected) {
                    if (m_CharacterLocomotion.FirstPersonMovementTypeFullName == removedMovementType) {
                        for (int i = 0; i < m_CharacterLocomotion.MovementTypes.Length; ++i) {
                            if (m_CharacterLocomotion.MovementTypes[i].GetType().FullName.Contains("FirstPerson")) {
                                m_CharacterLocomotion.FirstPersonMovementTypeFullName = m_CharacterLocomotion.MovementTypes[i].GetType().FullName;
                                break;
                            }
                        }
                    } else if (m_CharacterLocomotion.ThirdPersonMovementTypeFullName == removedMovementType) {
                        for (int i = 0; i < m_CharacterLocomotion.MovementTypes.Length; ++i) {
                            if (m_CharacterLocomotion.MovementTypes[i].GetType().FullName.Contains("ThirdPerson")) {
                                m_CharacterLocomotion.ThirdPersonMovementTypeFullName = m_CharacterLocomotion.MovementTypes[i].GetType().FullName;
                                break;
                            }
                        }
                    }
                    m_CharacterLocomotion.MovementTypeFullName = movementTypes[m_MovementTypeReorderableList.SelectedIndex].GetType().FullName;
                }
                EditorPrefs.SetInt(SelectedMovementTypeIndexKey, index);
                AddMovementTypeDropdowns();
            }, (int fromIndex, int toIndex) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);
            });
            m_MovementTypeReorderableList.EnableRemove = m_CharacterLocomotion.MovementTypes != null && m_CharacterLocomotion.MovementTypes.Length > 1;
            foldout.Add(m_MovementTypeReorderableList);

            m_SelectedMovementTypeContainer = new VisualElement();
            foldout.Add(m_SelectedMovementTypeContainer);

            if (m_CharacterLocomotion.MovementTypes != null) {
                var selectedIndex = EditorPrefs.GetInt(SelectedMovementTypeIndexKey, -1);
                if (selectedIndex != -1 && selectedIndex < m_CharacterLocomotion.MovementTypes.Length) {
                    m_MovementTypeReorderableList.SelectedIndex = selectedIndex;
                }
            }
        }

        /// <summary>
        /// Shows the selected movement type.
        /// </summary>
        /// <param name="index">The index of the movement type.</param>
        /// <returns>True if the movement type was shown.</returns>
        private bool ShowMovementType(int index)
        {
            m_SelectedMovementTypeContainer.Clear();
            var movementType = m_CharacterLocomotion.MovementTypes[index];
            if (movementType == null) {
                return false;
            }

            var label = new Label(InspectorUtility.DisplayTypeName(movementType.GetType(), true));
            label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            m_SelectedMovementTypeContainer.Add(label);
            FieldInspectorView.AddFields(m_CharacterLocomotion, movementType, Shared.Utility.MemberVisibility.Public, m_SelectedMovementTypeContainer, (obj) =>
            {
                m_CharacterLocomotion.MovementTypes[index] = obj as MovementType; // The movement type reference may have been updated.
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");
            }, null, null, true);
            return true;
        }

        /// <summary>
        /// Adds the movement type with the specified type.
        /// </summary>
        private void AddMovementType(object obj)
        {
            var movementTypes = m_CharacterLocomotion.MovementTypes;
            if (movementTypes == null) {
                movementTypes = new MovementType[1];
            } else {
                Array.Resize(ref movementTypes, movementTypes.Length + 1);
            }
            movementTypes[movementTypes.Length - 1] = Activator.CreateInstance(obj as Type) as MovementType;
            if (movementTypes.Length == 1) {
                m_CharacterLocomotion.MovementTypeFullName = movementTypes[0].GetType().FullName;
            }
            m_MovementTypeReorderableList.ItemsSource = m_CharacterLocomotion.MovementTypes = movementTypes;
            Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);

#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            // If both a first and third person movement type exists then the PerspectiveMonitor should also be added.
            if (m_CharacterLocomotion.GetComponent<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>() == null) {
                var hasFirstPersonMovementType = false;
                var hasThirdPersonMovementType = false;
                for (int i = 0; i < m_CharacterLocomotion.MovementTypes.Length; ++i) {
                    if (!hasFirstPersonMovementType && m_CharacterLocomotion.MovementTypes[i].GetType().FullName.Contains("FirstPersonController")) {
                        hasFirstPersonMovementType = true;
                    } else if (!hasThirdPersonMovementType && m_CharacterLocomotion.MovementTypes[i].GetType().FullName.Contains("ThirdPersonController")) {
                        hasThirdPersonMovementType = true;
                    }
                    if (hasFirstPersonMovementType && hasThirdPersonMovementType) {
                        break;
                    }
                }

                // If a first and third person movement type exists then the component should be added.
                if (hasFirstPersonMovementType && hasThirdPersonMovementType) {
                    m_CharacterLocomotion.gameObject.AddComponent<UltimateCharacterController.ThirdPersonController.Character.PerspectiveMonitor>();
                }
            }
#endif

            m_MovementTypeReorderableList.EnableRemove = movementTypes.Length > 1;

            // Select the newly added movement type.
            m_MovementTypeReorderableList.SelectedIndex = movementTypes.Length - 1;
            EditorPrefs.SetInt(SelectedMovementTypeIndexKey, m_MovementTypeReorderableList.SelectedIndex);

            AddMovementTypeDropdowns();
        }

        /// <summary>
        /// Draws the custom UIElements to the bottom of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            AddStateNames(container);
            AddAbilityList(container, true);
            AddAbilityList(container, false);
            AddEffectList(container);
            AddUnityEvents(container);

            // Ensure the ability description stays accurate. The BindingUpdater cannot be used because it doesn't operate on properties.
            container.RegisterCallback<AttachToPanelEvent>(c =>
            {
                EditorApplication.update += UpdateDescriptions;
            });
            container.RegisterCallback<DetachFromPanelEvent>(c =>
            {
                m_LabelByAbilityMap.Clear();
                m_LabelByEffectMap.Clear();
                EditorApplication.update -= UpdateDescriptions;
            });

            base.ShowFooterElements(container);
        }

        /// <summary>
        /// Adds the state names that can be set by the locomotion component.
        /// </summary>
        /// <param name="container">The parent container.</param>
        private void AddStateNames(VisualElement container)
        {
            var foldout = PersistentFoldout("State Names");
            container.Add(foldout);

            if (m_FirstPersonMovementTypeNames != null && m_FirstPersonMovementTypeNames.Count > 0) {
                FieldInspectorView.AddField(target, "m_FirstPersonStateName", foldout);
            }
            if (m_ThirdPersonMovementTypeNames != null && m_ThirdPersonMovementTypeNames.Count > 0) {
                FieldInspectorView.AddField(target, "m_ThirdPersonStateName", foldout);
            }
            FieldInspectorView.AddField(target, "m_MovingStateName", foldout);
            FieldInspectorView.AddField(target, "m_AirborneStateName", foldout);
        }

        /// <summary>
        /// Adds the ability ReorderableList.
        /// </summary>
        /// <param name="container">The parent container.</param>
        /// <param name="addAbilities">Should the abilities be added? If false the item abilities will be added.</param>
        private void AddAbilityList(VisualElement container, bool addAbilities)
        {
            var abilityFoldout = PersistentFoldout(addAbilities ? "Abilities" : "Item Abilities");
            abilityFoldout.contentContainer.AddToClassList("contained-list");
            container.Add(abilityFoldout);

            ReorderableList reorderableList = null;
            reorderableList = new ReorderableList((addAbilities ? m_CharacterLocomotion.Abilities : m_CharacterLocomotion.ItemAbilities), (VisualElement container, int index) => // Element.
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
                var ability = (addAbilities ? m_CharacterLocomotion.Abilities : m_CharacterLocomotion.ItemAbilities)[index];

                var label = container.Q<Label>();
                if (ability == null) {
                    label.text = "(Unknown Ability)";
                    return;
                }

                UpdateAbilityLabel(ability, label);
                if (m_LabelByAbilityMap.ContainsKey(ability)) {
                    m_LabelByAbilityMap.Remove(ability);
                }
                m_LabelByAbilityMap.Add(ability, label);

                // Map the toggle to the enabled bool.
                var toggle = container.Q<Toggle>();
                toggle.SetValueWithoutNotify(ability.Enabled);
                var enableField = InspectorUtility.GetField(ability, "m_Enabled");
                Action<object> enabledBindingUpdateEvent = (object newValue) =>
                {
                    toggle.SetValueWithoutNotify((bool)newValue);
                };
                toggle.RegisterValueChangedCallback<bool>((c) =>
                {
                    ability = (addAbilities ? m_CharacterLocomotion.Abilities : m_CharacterLocomotion.ItemAbilities)[index];
                    if (ability.Enabled == c.newValue) {
                        return;
                    }
                    ability.Enabled = c.newValue;
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);
                });
                toggle.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(enableField, -1, ability, enabledBindingUpdateEvent);
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

                var label = new Label((addAbilities ? "Abilities" : "Item Abilities"));
                label.AddToClassList("flex-grow");
                horizontalLayout.Add(label);
                label = new Label("Enabled");
                label.name = "serialized-reference-list-right-label";
                horizontalLayout.Add(label);
            }, (int index) => // Select.
            {
                if (ShowAbility(addAbilities, index)) {
                    EditorPrefs.SetInt((addAbilities ? SelectedAbilityIndexKey : SelectedItemAbilityIndexKey), reorderableList.SelectedIndex);
                }
            }, () => // Add.
            {
                if (addAbilities) {
                    ReorderableListSerializationHelper.AddObjectType(typeof(Ability), true, m_CharacterLocomotion.Abilities, AddAbility);
                } else {
                    ReorderableListSerializationHelper.AddObjectType(typeof(ItemAbility), true, m_CharacterLocomotion.ItemAbilities, AddItemAbility);
                }
            }, (int index) => // Remove.
            {
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");

                // Allow the ability to perform any destruction.
                var ability = (addAbilities ? m_CharacterLocomotion.Abilities : m_CharacterLocomotion.ItemAbilities)[reorderableList.SelectedIndex];
                if (ability != null) {
                    var abilityDrawer = Controls.Types.AbilityDrawers.AbilityDrawerUtility.FindAbilityDrawer(ability.GetType(), true);
                    if (abilityDrawer != null) {
                        abilityDrawer.AbilityRemoved(ability, target);
                    }
                }

                if (addAbilities) {
                    var abilityList = new List<Ability>(m_CharacterLocomotion.Abilities);
                    abilityList.RemoveAt(reorderableList.SelectedIndex);
                    reorderableList.ItemsSource = m_CharacterLocomotion.Abilities = abilityList.ToArray();
                } else {
                    var abilityList = new List<ItemAbility>(m_CharacterLocomotion.ItemAbilities);
                    abilityList.RemoveAt(reorderableList.SelectedIndex);
                    reorderableList.ItemsSource = m_CharacterLocomotion.ItemAbilities = abilityList.ToArray();

                }
                Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);
                (addAbilities ? m_SelectedAbilityContainer : m_SelectedItemAbilityContainer).Clear();

                // Update the index to point to no longer point to the now deleted ability.
                var selectedIndex = index - 1;
                if (selectedIndex == -1 && (addAbilities ? m_CharacterLocomotion.Abilities : m_CharacterLocomotion.ItemAbilities).Length > 0) {
                    selectedIndex = 0;
                }
                reorderableList.SelectedIndex = selectedIndex;
                EditorPrefs.SetInt((addAbilities ? SelectedAbilityIndexKey : SelectedItemAbilityIndexKey), reorderableList.SelectedIndex);
            }, (int fromIndex, int toIndex) => // Reorder.
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);
            });
            abilityFoldout.Add(reorderableList);
            if (addAbilities) {
                m_AbilityReorderableList = reorderableList;
                m_SelectedAbilityContainer = new VisualElement();
                abilityFoldout.Add(m_SelectedAbilityContainer);

                if (m_CharacterLocomotion.Abilities != null) {
                    var selectedAbilityIndex = EditorPrefs.GetInt(SelectedAbilityIndexKey, -1);
                    if (selectedAbilityIndex != -1 && selectedAbilityIndex < m_CharacterLocomotion.Abilities.Length) {
                        m_AbilityReorderableList.SelectedIndex = selectedAbilityIndex;
                    }
                }
            } else {
                m_ItemAbilityReorderableList = reorderableList;
                m_SelectedItemAbilityContainer = new VisualElement();
                abilityFoldout.Add(m_SelectedItemAbilityContainer);

                if (m_CharacterLocomotion.ItemAbilities != null) {
                    var selectedAbilityIndex = EditorPrefs.GetInt(SelectedItemAbilityIndexKey, -1);
                    if (selectedAbilityIndex != -1 && selectedAbilityIndex < m_CharacterLocomotion.ItemAbilities.Length) {
                        m_ItemAbilityReorderableList.SelectedIndex = selectedAbilityIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the ability label in the ReorderableList row.
        /// </summary>
        /// <param name="ability">The ability that should be updated.</param>
        /// <param name="label">The label of the ability.</param>
        private void UpdateAbilityLabel(Ability ability, Label label)
        {
            var displayText = InspectorUtility.DisplayTypeName(ability.GetType(), true);
            var description = ability.AbilityDescription;
            var inspectorDescription = ability.InspectorDescription;
            if (!string.IsNullOrEmpty(inspectorDescription)) {
                if (string.IsNullOrEmpty(description)) {
                    description = inspectorDescription;
                } else {
                    description += ", " + inspectorDescription;
                }
            }
            if (!string.IsNullOrEmpty(description)) {
                displayText = string.Format("{0} ({1})", displayText, description);
            }
            if (ability.IsActive) {
                displayText += " (Active)";
            }
            label.text = displayText;
        }

        /// <summary>
        /// Shows the selected ability.
        /// </summary>
        /// <param name="abilities">Should the abilities be shown? If false the item abilities will be shown.</param>
        /// <param name="index">The index of the ability.</param>
        /// <returns>True if the ability was shown.</returns>
        private bool ShowAbility(bool abilities, int index)
        {
            var abilityContainer = (abilities ? m_SelectedAbilityContainer : m_SelectedItemAbilityContainer);
            abilityContainer.Clear();
            var ability = (abilities ? m_CharacterLocomotion.Abilities[index] : m_CharacterLocomotion.ItemAbilities[index]);
            if (ability == null) {
                return false;
            }

            var label = new Label(InspectorUtility.DisplayTypeName(ability.GetType(), true));
            label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            abilityContainer.Add(label);
            FieldInspectorView.AddFields(m_CharacterLocomotion, ability, Shared.Utility.MemberVisibility.Public, abilityContainer, (obj) =>
            {
                // The ability reference may have been updated.
                if (abilities) {
                    m_CharacterLocomotion.Abilities[index] = obj as Ability;
                } else {
                    m_CharacterLocomotion.ItemAbilities[index] = obj as ItemAbility;
                }
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");
            }, null, null, true, null, false, null, this);
            return true;
        }

        /// <summary>
        /// Adds the ability with the specified type.
        /// </summary>
        private void AddAbility(object obj)
        {
            var ability = AbilityBuilder.AddAbility(m_CharacterLocomotion, obj as Type);
            m_AbilityReorderableList.ItemsSource = m_CharacterLocomotion.Abilities;

            // Select the newly added ability.
            m_AbilityReorderableList.SelectedIndex = m_CharacterLocomotion.Abilities.Length - 1;
            EditorPrefs.SetInt(SelectedAbilityIndexKey, m_AbilityReorderableList.SelectedIndex);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);

            // Allow the ability to perform any initialization.
            var abilityDrawer = Controls.Types.AbilityDrawers.AbilityDrawerUtility.FindAbilityDrawer(ability.GetType(), true);
            if (abilityDrawer != null) {
                abilityDrawer.AbilityAdded(ability, target);
            }
        }

        /// <summary>
        /// Adds the item ability with the specified type.
        /// </summary>
        private void AddItemAbility(object obj)
        {
            var ability = AbilityBuilder.AddItemAbility(m_CharacterLocomotion, obj as Type);
            m_ItemAbilityReorderableList.ItemsSource = m_CharacterLocomotion.ItemAbilities;

            // Select the newly added ability.
            m_ItemAbilityReorderableList.SelectedIndex = m_CharacterLocomotion.ItemAbilities.Length - 1;
            EditorPrefs.SetInt(SelectedItemAbilityIndexKey, m_ItemAbilityReorderableList.SelectedIndex);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);

            // Allow the ability to perform any initialization.
            var abilityDrawer = Controls.Types.AbilityDrawers.AbilityDrawerUtility.FindAbilityDrawer(ability.GetType(), true);
            if (abilityDrawer != null) {
                abilityDrawer.AbilityAdded(ability, target);
            }
        }

        /// <summary>
        /// Adds the effect ReorderableList.
        /// </summary>
        /// <param name="container">The parent container.</param>
        private void AddEffectList(VisualElement container)
        {
            var foldout = PersistentFoldout("Effects");
            foldout.contentContainer.AddToClassList("contained-list");
            container.Add(foldout);

            m_EffectsReorderableList = new ReorderableList(m_CharacterLocomotion.Effects, (VisualElement container, int index) => // Add.
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
                if (m_CharacterLocomotion.Effects[index] == null) {
                    label.text = "(Unknown Effect)";
                    return;
                }
                var effect = m_CharacterLocomotion.Effects[index];
                label.text = InspectorUtility.DisplayTypeName(effect.GetType(), true);

                UpdateEffectLabel(effect, label);
                if (m_LabelByEffectMap.ContainsKey(effect)) {
                    m_LabelByEffectMap.Remove(effect);
                }
                m_LabelByEffectMap.Add(effect, label);

                // Map the toggle to the enabled bool.
                var toggle = container.Q<Toggle>();
                toggle.SetValueWithoutNotify(effect.Enabled);
                var enableField = InspectorUtility.GetField(effect, "m_Enabled");
                Action<object> enabledBindingUpdateEvent = (object newValue) =>
                {
                    toggle.SetValueWithoutNotify((bool)newValue);
                };
                toggle.RegisterValueChangedCallback<bool>((c) =>
                {
                    effect = m_CharacterLocomotion.Effects[index];
                    if (effect.Enabled == c.newValue) {
                        return;
                    }
                    effect.Enabled = c.newValue;
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);
                });
                toggle.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(enableField, -1, effect, enabledBindingUpdateEvent);
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

                var label = new Label("Effects");
                label.AddToClassList("flex-grow");
                horizontalLayout.Add(label);
                label = new Label("Enabled");
                label.name = "serialized-reference-list-right-label";
                horizontalLayout.Add(label);
            }, (int index) => // Select.
            {
                if (ShowEffect(index)) {
                    EditorPrefs.SetInt(SelectedEffectIndexKey, m_EffectsReorderableList.SelectedIndex);
                }
            }, () => // Add.
            {
                ReorderableListSerializationHelper.AddObjectType(typeof(Effect), true, m_CharacterLocomotion.Effects, AddEffect);
            }, (int index) => // Remove.
            {
                var effectList = new List<Effect>(m_CharacterLocomotion.Effects);
                effectList.RemoveAt(m_EffectsReorderableList.SelectedIndex);
                m_EffectsReorderableList.ItemsSource = m_CharacterLocomotion.Effects = effectList.ToArray();
                Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);
                m_SelectedEffectContainer.Clear();

                // Update the index to point to no longer point to the now deleted ability.
                var selectedIndex = index - 1;
                if (selectedIndex == -1 && m_CharacterLocomotion.Effects.Length > 0) {
                    selectedIndex = 0;
                }
                m_EffectsReorderableList.SelectedIndex = selectedIndex;
                EditorPrefs.SetInt(SelectedEffectIndexKey, m_EffectsReorderableList.SelectedIndex);
            }, (int fromIndex, int toIndex) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);
            });
            foldout.Add(m_EffectsReorderableList);

            m_SelectedEffectContainer = new VisualElement();
            foldout.Add(m_SelectedEffectContainer);

            if (m_CharacterLocomotion.Effects != null) {
                var selectedIndex = EditorPrefs.GetInt(SelectedEffectIndexKey, -1);
                if (selectedIndex != -1 && selectedIndex < m_CharacterLocomotion.Effects.Length) {
                    m_EffectsReorderableList.SelectedIndex = selectedIndex;
                }
            }
        }

        /// <summary>
        /// Updates the effect label in the ReorderableList row.
        /// </summary>
        /// <param name="effect">The effect that should be updated.</param>
        /// <param name="label">The label of the ability.</param>
        private void UpdateEffectLabel(Effect effect, Label label)
        {
            var displayText = InspectorUtility.DisplayTypeName(effect.GetType(), true);
            if (!string.IsNullOrEmpty(effect.InspectorDescription)) {
                displayText = string.Format("{0} ({1})", displayText, effect.InspectorDescription);
            }
            if (effect.IsActive) {
                displayText += " (Active)";
            }
            label.text = displayText;
        }

        /// <summary>
        /// Shows the selected effect.
        /// </summary>
        /// <param name="index">The index of the effect.</param>
        /// <returns>True if the effect was shown.</returns>
        private bool ShowEffect(int index)
        {
            m_SelectedEffectContainer.Clear();
            var effect = m_CharacterLocomotion.Effects[index];
            if (effect == null) {
                return false;
            }

            var label = new Label(InspectorUtility.DisplayTypeName(effect.GetType(), true));
            label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            m_SelectedEffectContainer.Add(label);
            FieldInspectorView.AddFields(m_CharacterLocomotion, effect, Shared.Utility.MemberVisibility.Public, m_SelectedEffectContainer, (obj) =>
            {
                m_CharacterLocomotion.Effects[index] = obj as Effect; // The effect reference may have been updated.
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");
            }, null, null, true);
            return true;
        }

        /// <summary>
        /// Adds the effect with the specified type.
        /// </summary>
        private void AddEffect(object obj)
        {
            EffectBuilder.AddEffect(m_CharacterLocomotion, obj as Type);
            m_EffectsReorderableList.ItemsSource = m_CharacterLocomotion.Effects;
            Shared.Editor.Utility.EditorUtility.SetDirty(m_CharacterLocomotion);

            // Select the newly added effect.
            m_EffectsReorderableList.SelectedIndex = m_CharacterLocomotion.Effects.Length - 1;
            EditorPrefs.SetInt(SelectedEffectIndexKey, m_EffectsReorderableList.SelectedIndex);
        }

        /// <summary>
        /// Adds the unity events.
        /// </summary>
        /// <param name="container">The parent container.</param>
        private void AddUnityEvents(VisualElement container)
        {
            var foldout = PersistentFoldout("Events");
            container.Add(foldout);
            FieldInspectorView.AddField(target, "m_OnMovementTypeActiveEvent", foldout);
            FieldInspectorView.AddField(target, "m_OnAbilityActiveEvent", foldout);
            FieldInspectorView.AddField(target, "m_OnItemAbilityActiveEvent", foldout);
            FieldInspectorView.AddField(target, "m_OnGroundedEvent", foldout);
            FieldInspectorView.AddField(target, "m_OnLandEvent", foldout);
            FieldInspectorView.AddField(target, "m_OnChangeTimeScaleEvent", foldout);
            FieldInspectorView.AddField(target, "m_OnChangeMovingPlatformsEvent", foldout);
        }

        /// <summary>
        /// Updates the default first/third movement type based on the movement types availabe on the character controller.
        /// </summary>
        private void UpdateDefaultMovementTypes()
        {
            // The movement type may not exist anymore.
            if (!string.IsNullOrEmpty(m_CharacterLocomotion.FirstPersonMovementTypeFullName) && Shared.Utility.TypeUtility.GetType(m_CharacterLocomotion.FirstPersonMovementTypeFullName) == null) {
                m_CharacterLocomotion.FirstPersonMovementTypeFullName = string.Empty;
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
            }
            if (!string.IsNullOrEmpty(m_CharacterLocomotion.ThirdPersonMovementTypeFullName) && Shared.Utility.TypeUtility.GetType(m_CharacterLocomotion.ThirdPersonMovementTypeFullName) == null) {
                m_CharacterLocomotion.ThirdPersonMovementTypeFullName = string.Empty;
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
            }

            var hasSelectedMovementType = false;
            var firstPersonMovementTypes = new List<Type>();
            var thirdPersonMovementTypes = new List<Type>();
            m_FirstPersonMovementTypeNames = new List<string>();
            m_ThirdPersonMovementTypeNames = new List<string>();
            var movementTypes = m_CharacterLocomotion.MovementTypes;
            if (movementTypes != null) {
                for (int i = 0; i < movementTypes.Length; ++i) {
                    if (movementTypes[i] == null) {
                        continue;
                    }
                    if (movementTypes[i].GetType().FullName.Contains("FirstPerson")) {
                        // Use the movement type if the type is currently empty.
                        if (string.IsNullOrEmpty(m_CharacterLocomotion.FirstPersonMovementTypeFullName)) {
                            m_CharacterLocomotion.FirstPersonMovementTypeFullName = movementTypes[i].GetType().FullName;
                        }
                        firstPersonMovementTypes.Add(movementTypes[i].GetType());
                        m_FirstPersonMovementTypeNames.Add(InspectorUtility.DisplayTypeName(movementTypes[i].GetType(), false));
                    } else { // Third Person.
                        // Use the movement type if the type is currently empty.
                        if (string.IsNullOrEmpty(m_CharacterLocomotion.ThirdPersonMovementTypeFullName)) {
                            m_CharacterLocomotion.ThirdPersonMovementTypeFullName = movementTypes[i].GetType().FullName;
                        }
                        thirdPersonMovementTypes.Add(movementTypes[i].GetType());
                        m_ThirdPersonMovementTypeNames.Add(InspectorUtility.DisplayTypeName(movementTypes[i].GetType(), false));
                    }

                    if (m_CharacterLocomotion.MovementTypeFullName == movementTypes[i].GetType().FullName) {
                        hasSelectedMovementType = true;
                    }
                }
            }
            m_FirstPersonMovementTypes = firstPersonMovementTypes.ToArray();
            m_ThirdPersonMovementTypes = thirdPersonMovementTypes.ToArray();

            // If the selected MovementType no longer exists in the list then select the first movement type.
            if (!hasSelectedMovementType) {
                m_CharacterLocomotion.MovementTypeFullName = string.Empty;
                if (movementTypes != null && movementTypes.Length > 0) {
                    m_CharacterLocomotion.MovementTypeFullName = movementTypes[0].GetType().FullName;
                }
            }
        }

        /// <summary>
        /// Ensures the ability and effect descriptions stay updated.
        /// </summary>
        private void UpdateDescriptions()
        {
            if (m_LastUpdateTime + BindingUpdater.UpdateRate > Time.realtimeSinceStartup) {
                return;
            }
            m_LastUpdateTime = Time.realtimeSinceStartup;

            foreach (var abilityLabelPair in m_LabelByAbilityMap) {
                UpdateAbilityLabel(abilityLabelPair.Key, abilityLabelPair.Value);
            }

            foreach (var effectLabelPair in m_LabelByEffectMap) {
                UpdateEffectLabel(effectLabelPair.Key, effectLabelPair.Value);
            }
        }
    }
}