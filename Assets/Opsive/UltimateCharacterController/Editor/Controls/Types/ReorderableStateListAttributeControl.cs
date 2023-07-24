/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Attributes;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Editor.Utility;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Implements AttributeControlBase for reorderable lists.
    /// </summary>
    [ControlType(typeof(Opsive.Shared.StateSystem.ReorderableStateListAttribute))]
    public class ReorderableStateListAttributeControl : AttributeControlBase
    {
        private const string c_EditorPrefsLastPresetPathKey = "Opsive.Shared.Editor.Inspectors.LastPresetPath";
        private const string c_EditorPrefsSelectedIndexKey = "Opsive.Shared.Editor.Inspectors.SelectedStateIndex";

        /// <summary>
        /// Does the attribute override the type control?
        /// </summary>
        public override bool OverrideTypeControl { get { return true; } }

        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get { return false; } }

        /// <summary>
        /// Returns the attribute control that should be used for the specified AttributeControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(AttributeControlInput input)
        {
            var stateOwner = input.Target as IStateOwner;

            var foldout = new Foldout() { text = "States" };
            foldout.name = "ReorderableStateListAttributeControl";
            ReorderableList reorderableList = null;
            reorderableList = new ReorderableList(stateOwner.States, (VisualElement container, int index) => // Add Row.
            {
                var stateElementContainer = new StateElementContainer(stateOwner, reorderableList);
                stateElementContainer.OnStateChange += () =>
                {
                    // The value is an array with object references inside. Therefore simply changed data is already set. 
                    input.OnChangeEvent?.Invoke(stateOwner.States);
                };
                container.Add(stateElementContainer);
            }, (VisualElement container, int index) => // Bind.
            {
                var state = stateOwner.States[index];
                var elementContainer = container.Q<StateElementContainer>();
                elementContainer.BindToState(state);
            }, (VisualElement container) => // Header.
            {
                container.AddToClassList("horizontal-layout");
                var stateLabel = new Label("State");
                stateLabel.name = "name-label";
                container.Add(stateLabel);
                var presetLabel = new Label("Preset");
                presetLabel.name = "preset-label";
                container.Add(presetLabel);
                var blockedByLabel = new Label("Blocked By");
                blockedByLabel.name = "blocked-by-label";
                container.Add(blockedByLabel);
                var persistLabel = new Label("Persist");
                persistLabel.name = "persist-label";
                container.Add(persistLabel);
                var activateLabel = new Label("Activate");
                activateLabel.name = "activate-label";
                container.Add(activateLabel);
            }, (int index) => // Select.
            {
            }, () => // Add
            {
                var addMenu = new GenericMenu();
                addMenu.AddItem(new GUIContent("Add Existing Preset"), false, () =>
                {
                    AddExistingPreset(stateOwner, reorderableList, GetSelectedIndexKey(input.Target));
                    input.OnChangeEvent?.Invoke(stateOwner.States);
                });
                addMenu.AddItem(new GUIContent("Create New Preset"), false, () =>
                {
                    CreatePreset(stateOwner, reorderableList, GetSelectedIndexKey(input.Target));
                    input.OnChangeEvent?.Invoke(stateOwner.States);
                });
                addMenu.ShowAsContext();
            }, (int index) => // Remove.
            {
                var states = stateOwner.States;

                // The last state cannot be removed.
                if (reorderableList.SelectedIndex == states.Length - 1) {
                    EditorUtility.DisplayDialog("Unable to Remove", "The Default State cannot be removed.", "OK");
                    return;
                }

                // The block lists must be updated to account for the state removal.
                for (int i = 0; i < states.Length; ++i) {
                    if (i == reorderableList.SelectedIndex) {
                        continue;
                    }

                    var state = states[i];
                    if (state.BlockList != null && state.BlockList.Length > 0) {
                        var blockList = new List<string>(state.BlockList);
                        for (int j = blockList.Count - 1; j > -1; --j) {
                            if (blockList[j] == states[reorderableList.SelectedIndex].Name) {
                                blockList.RemoveAt(j);
                            }
                        }
                        state.BlockList = blockList.ToArray();
                    }
                }

                var stateList = new List<Opsive.Shared.StateSystem.State>(states);
                stateList.RemoveAt(reorderableList.SelectedIndex);
                reorderableList.ItemsSource = stateOwner.States = stateList.ToArray();
                reorderableList.SelectedIndex = reorderableList.SelectedIndex - 1;
                if (reorderableList.SelectedIndex == -1 && stateList.Count > 0) {
                    reorderableList.SelectedIndex = 0;
                }
                reorderableList.EnableRemove = stateList.Count > 1;
                input.OnChangeEvent?.Invoke(stateOwner.States);
            }, (int fromIndex, int toIndex) => {
                // The last state has to be the default state.
                if (!stateOwner.States[stateOwner.States.Length - 1].Default) {
                    var tmpState = stateOwner.States[stateOwner.States.Length - 2];
                    stateOwner.States[stateOwner.States.Length - 2] = stateOwner.States[stateOwner.States.Length - 1];
                    stateOwner.States[stateOwner.States.Length - 1] = tmpState;
                    reorderableList.Refresh();
                    reorderableList.SelectedIndex = stateOwner.States.Length - 2;
                }
                input.OnChangeEvent?.Invoke(stateOwner.States);
            });
            reorderableList.EnableAdd = !Application.isPlaying;
            reorderableList.EnableRemove = !Application.isPlaying && stateOwner.States.Length > 1;
            foldout.Add(reorderableList);
            return foldout;
        }
        private string GetSelectedIndexKey(object target) { return c_EditorPrefsSelectedIndexKey + "." + target.GetType() + (target is Component ? ("." + (target as Component).name) : String.Empty); }

        /// <summary>
        /// VisualElement that represents a row within the state ReorderableList.
        /// </summary>
        private class StateElementContainer : VisualElement
        {
            public Action OnStateChange;
            
            private IStateOwner m_StateOwner;
            private State m_State;
            private List<string> m_AllStates;

            private TextField m_NameField;
            private ObjectField m_PresetField;
            private MaskField m_BlockedByField;
            private Button m_PersistButton;
            private Button m_ActivateButton;

            /// <summary>
            /// Two parameter constructor.
            /// </summary>
            /// <param name="stateOwner">The owner of the states.</param>
            /// <param name="reorderableList">The list that the element container belongs to.</param>
            public StateElementContainer(IStateOwner stateOwner, ReorderableList reorderableList)
            {
                m_StateOwner = stateOwner;
                AddToClassList("horizontal-layout");

                m_NameField = new TextField();
                m_NameField.name = "name-field";
                m_NameField.isDelayed = true;

                m_NameField.RegisterValueChangedCallback(c =>
                {
                    if (c.newValue == m_State.Name) {
                        return;
                    }

                    if (!IsUniqueName(m_StateOwner.States, c.newValue)) {
                        m_NameField.SetValueWithoutNotify(c.previousValue);
                        return;
                    }

                    m_State.Name = c.newValue;
                    reorderableList.Refresh();
                    OnStateChange?.Invoke();
                });
                Add(m_NameField);

                // The dropdown button overlaps the text within the textfield. Set the padding on the TextInput in order to
                // allow the far right text to be visible.
                foreach (var child in m_NameField.Children()) {
                    child.style.paddingRight = 16;
                    break;
                }

                var stateIDSearchButton = new Button();
                stateIDSearchButton.AddToClassList(DataMapInspector<string>.StyleClassName + "_reorderable_state_button");
                stateIDSearchButton.text = "▼";
                stateIDSearchButton.clicked += () => {
                    reorderableList.Refresh();
                    StateNamesSearchableWindow.OpenWindow("States",  m_NameField.value, (newValue) =>
                    {
                        m_NameField.SetValueWithoutNotify(newValue);
                        m_State.Name = newValue;
                        reorderableList.Refresh();
                        OnStateChange?.Invoke();
                    }, true);
                };
                m_NameField.Add(stateIDSearchButton);

                m_PresetField = new ObjectField();
                m_PresetField.name = "preset-field";
                m_PresetField.objectType = typeof(Preset);
                m_PresetField.RegisterValueChangedCallback(c =>
                {
                    var desiredPreset = c.newValue as PersistablePreset;
                    if (desiredPreset != null) {
                        var objType = TypeUtility.GetType(desiredPreset.Data.ObjectType);
                        if (objType != null && objType.IsInstanceOfType(m_StateOwner)) {
                            m_State.Preset = desiredPreset;
                            m_PresetField.SetValueWithoutNotify(c.newValue);
                        } else {
                            Debug.LogError($"Error: Unable to change preset. {desiredPreset.name} ({desiredPreset.Data.ObjectType}) doesn't use the same object type ({m_StateOwner.GetType().FullName}).");
                            m_PresetField.SetValueWithoutNotify(c.previousValue);
                        }
                    }
                    OnStateChange?.Invoke();
                });
                Add(m_PresetField);

                m_BlockedByField = new MaskField();
                m_BlockedByField.name = "blocked-by-field";
                m_BlockedByField.RegisterValueChangedCallback<int>((c) =>
                {
                    var stateNames = new List<string>();
                    for (int i = 0; i < m_AllStates.Count; ++i) {
                        // If the state index is within the block mask then that state should be added to the list. A blockMask of -1 indicates Everything.
                        if (((1 << i) & c.newValue) != 0 || c.newValue == -1) {
                            stateNames.Add(m_AllStates[i]);
                        }
                    }
                    m_State.BlockList = stateNames.ToArray();
                    OnStateChange?.Invoke();
                });
                Add(m_BlockedByField);

                m_PersistButton = new Button();
                m_PersistButton.name = "persist-button";
                m_PersistButton.AddToClassList(EditorGUIUtility.isProSkin ? "persist-dark-icon" : "persist-light-icon");
                m_PersistButton.clicked += () =>
                {
                    var preset = m_State.Preset as PersistablePreset;
                    Dictionary<long, int> valuePositionMap;
                    if (preset.Data.LongValueHashes != null && preset.Data.LongValueHashes.Length > 0) {
                        valuePositionMap = new Dictionary<long, int>(preset.Data.LongValueHashes.Length);
                        for (int i = 0; i < preset.Data.LongValueHashes.Length; ++i) {
                            valuePositionMap.Add(preset.Data.LongValueHashes[i], i);
                        }
                    } else if (preset.Data.ValueHashes != null) {
                        valuePositionMap = new Dictionary<long, int>(preset.Data.ValueHashes.Length);
                        for (int i = 0; i < preset.Data.ValueHashes.Length; ++i) {
                            valuePositionMap.Add(preset.Data.ValueHashes[i], i);
                        }
                    } else {
                        valuePositionMap = new Dictionary<long, int>();
                    }

                    // Loop through all of the properties on the object.
                    var properties = Serialization.GetSerializedProperties(m_StateOwner.GetType(), MemberVisibility.Public);
                    var hashType = Serialization.GetHashType(preset.Data.Version);
                    // Remove and add the properties that are being serialized.
                    for (int i = 0; i < properties.Length; ++i) {
                        var hash = Serialization.StringHash(properties[i].PropertyType.FullName) + Serialization.StringHash(properties[i].Name);
                        // The property is currently being serialized.
                        if (valuePositionMap.ContainsKey(hash)) {
                            // Add the new property to the serialization.
                            object value = null;
                            var property = properties[i];
                            if (!typeof(UnityEngine.Object).IsAssignableFrom(property.PropertyType)) {
                                long[] valueHashes;
                                if (preset.Data.LongValueHashes != null && preset.Data.LongValueHashes.Length > 0) {
                                    valueHashes = preset.Data.LongValueHashes;
                                } else {
                                    valueHashes = new long[preset.Data.ValueHashes.Length];
                                    for (int j = 0; j < valueHashes.Length; ++j) {
                                        valueHashes[j] = preset.Data.ValueHashes[j];
                                    }
                                }

                                var unityObjectIndexes = new List<int>();
                                Serialization.GetUnityObjectIndexes(ref unityObjectIndexes, property.PropertyType, property.Name, 0, valuePositionMap, valueHashes, preset.Data.ValuePositions,
                                                                    preset.Data.Values, false, MemberVisibility.Public, hashType);

                                Serialization.RemoveProperty(i, unityObjectIndexes, preset.Data, MemberVisibility.Public, hashType);

                                // Get the current value of the active object.
                                var getMethod = property.GetGetMethod();
                                if (getMethod != null) {
                                    value = getMethod.Invoke(m_StateOwner, null);
                                }
                                // Add the property back with the updated value.
                                Serialization.AddProperty(property, value, unityObjectIndexes, preset.Data, MemberVisibility.Public);
                            }
                        }
                    }
                    OnStateChange?.Invoke();
                };
                Add(m_PersistButton);

                m_ActivateButton = new Button();
                m_ActivateButton.name = "activate-button";
                m_ActivateButton.AddToClassList(EditorGUIUtility.isProSkin ? "activate-dark-icon" : "activate-light-icon"); 
                m_ActivateButton.clicked += () =>
                {
                    StateManager.ActivateState(m_State, !m_State.Active, m_StateOwner.States);
                    OnStateChange?.Invoke();
                };
                m_ActivateButton.SetEnabled(EditorApplication.isPlayingOrWillChangePlaymode);
                Add(m_ActivateButton);

                // Do not allow modifications when the game is active.
                EditorApplication.playModeStateChanged += (s) =>
                {
                    m_NameField.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
                    m_PresetField.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
                    m_BlockedByField.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
                    m_ActivateButton.SetEnabled(EditorApplication.isPlayingOrWillChangePlaymode);
                };
            }

            /// <summary>
            /// Binds the state to the row elements.
            /// </summary>
            /// <param name="state">The state that the row represents.</param>
            public void BindToState(State state)
            {
                m_State = state;

                parent.parent.SetEnabled(!state.Default);

                var active = state.Active && !state.IsBlocked();
                var stateName = state.Name + (active ? " (Active)" : string.Empty);
                m_NameField.value = stateName;
                m_NameField.style.unityFontStyleAndWeight = active ? FontStyle.Bold : FontStyle.Normal;
                Action<object> activeBindingUpdateEvent = (object newValue) =>
                {
                    if ((bool)newValue) {
                        m_NameField.SetValueWithoutNotify(m_NameField.value + " (Active)");
                        m_NameField.style.unityFontStyleAndWeight = FontStyle.Bold;
                    } else {
                        m_NameField.SetValueWithoutNotify(m_NameField.value.Replace(" (Active)", ""));
                        m_NameField.style.unityFontStyleAndWeight = FontStyle.Normal;
                    }
                };
                m_NameField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(state.GetType().GetField("m_Active", BindingFlags.Instance | BindingFlags.NonPublic), -1, state, activeBindingUpdateEvent);
                });
                m_NameField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(activeBindingUpdateEvent);
                });
                m_PresetField.SetValueWithoutNotify(state.Preset);

                // Determine the fields for the BlockedBy field.
                m_AllStates = new List<string>();
                var blockList = m_State.BlockList;
                var selected = 0;
                for (int i = 0; i < m_StateOwner.States.Length; ++i) {
                    var currentState = m_StateOwner.States[i];
                    if (currentState == null) {
                        m_StateOwner.States[i] = currentState = new Opsive.Shared.StateSystem.State();
                    }
                    // The current state cannot block the default state.
                    if (currentState.Default) {
                        continue;
                    }
                    string name;
                    // The current state cannot block itself.
                    if ((name = currentState.Name) == m_State.Name) {
                        continue;
                    }
                    // The selected state cannot block the current state if the current state blocks the selected state.
                    var currentStateBlockList = currentState.BlockList;
                    var canAdd = true;
                    if (currentStateBlockList != null) {
                        for (int j = 0; j < currentStateBlockList.Length; ++j) {
                            if (m_State.Name == currentStateBlockList[j]) {
                                canAdd = false;
                                break;
                            }
                        }
                    }

                    // canAdd will be false if the current state is blocking the selected state.
                    if (!canAdd) {
                        continue;
                    }

                    // The current state can block the selected state. Add the name to the popup and determine if the state is selected. A mask is used
                    // to allow multiple selected states.
                    m_AllStates.Add(name);
                    if (blockList != null) {
                        for (int j = 0; j < blockList.Length; ++j) {
                            if (m_AllStates[m_AllStates.Count - 1] == blockList[j]) {
                                selected |= 1 << (m_AllStates.Count - 1);
                                break;
                            }
                        }
                    }
                }
                // At least one value needs to exist.
                if (m_AllStates.Count == 0) {
                    m_AllStates.Add("Nothing");
                }
                UIElementsUtility.SetMaskFieldChoices(m_BlockedByField, m_AllStates);
                m_BlockedByField.SetValueWithoutNotify(selected);
            }
        }

        /// <summary>
        /// Adds a new element to the state list which uses an existing preset.
        /// </summary>
        public static void AddExistingPreset(IStateOwner stateOwner, ReorderableList reorderableList, string selectedIndexKey)
        {
            // A state must have a preset - open the file panel to select it.
            var path = EditorPrefs.GetString(c_EditorPrefsLastPresetPathKey, Opsive.Shared.Editor.Inspectors.Utility.InspectorUtility.GetSaveFilePath());
            // The directory name may saved incorrectly so correct it by replacing '\' with '/'.
            path = path.Replace('\\', '/');
            if (!path.Contains(Application.dataPath)) {
                path = Application.dataPath;
            }
            path = EditorUtility.OpenFilePanelWithFilters("Select Preset", path, new[] { "Preset", "asset" });
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                EditorPrefs.SetString(c_EditorPrefsLastPresetPathKey, System.IO.Path.GetDirectoryName(path));
                // The path is relative to the project.
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                var preset = AssetDatabase.LoadAssetAtPath<PersistablePreset>(path);
                if (preset == null) {
                    Debug.LogError($"Error: Unable to add preset. {System.IO.Path.GetFileName(path)} isn't located within the same project directory.");
                    return;
                }
                // The preset object type has to belong to the same object type.
                var type = stateOwner.GetType();
                if (preset.Data.ObjectType == type.FullName) {
                    var startName = type.Name + "Preset";
                    var name = preset.name;
                    if (!string.IsNullOrEmpty(name.Replace(startName, ""))) {
                        name = name.Replace(startName, "");
                    }
                    InsertStateElement(stateOwner, reorderableList, selectedIndexKey, name, preset);
                } else {
                    Debug.LogError($"Error: Unable to add preset. {preset.name} ({preset.Data.ObjectType}) doesn't use the same object type ({type.FullName}).");
                }
            }
        }

        /// <summary>
        /// Creates a new preset and adds it to a new state in the list.
        /// </summary>
        public static void CreatePreset(IStateOwner stateOwner, ReorderableList reorderableList, string selectedIndexKey)
        {
            var preset = PersistablePreset.CreatePreset(stateOwner);
            if (preset != null) {
                var startName = stateOwner.GetType().Name + "Preset.asset";
                var path = EditorPrefs.GetString(c_EditorPrefsLastPresetPathKey, Opsive.Shared.Editor.Inspectors.Utility.InspectorUtility.GetSaveFilePath());
                // The directory name may saved incorrectly so correct it by replacing '\' with '/'.
                path = path.Replace('\\', '/');
                if (!path.Contains(Application.dataPath)) {
                    path = Application.dataPath;
                }
                path = EditorUtility.SaveFilePanel("Save Preset", path, startName, "asset");
                if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                    EditorPrefs.SetString(c_EditorPrefsLastPresetPathKey, System.IO.Path.GetDirectoryName(path));
                    // The path is relative to the project.
                    path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));
                    // Do not delete/add if an existing preset already exists to prevent the references from being destroyed.
                    var existingPreset = AssetDatabase.LoadAssetAtPath<Preset>(path);
                    if (existingPreset != null) {
                        EditorUtility.DisplayDialog("Unable to Save Preset", "The preset must reference a unique file name.", "Okay");
                        return;
                    }

                    var name = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (!string.IsNullOrEmpty(name.Replace(stateOwner.GetType().Name + "Preset", ""))) {
                        name = name.Replace(stateOwner.GetType().Name + "Preset", "");
                    }

                    AssetDatabase.CreateAsset(preset, path);
                    AssetDatabase.ImportAsset(path);
                    EditorGUIUtility.PingObject(preset);
                    if (!Application.isPlaying) {
                        InsertStateElement(stateOwner, reorderableList, selectedIndexKey, name, preset);
                    }
                }
            }
        }

        /// <summary>
        /// Inserts a new state element in the state array.
        /// </summary>
        private static void InsertStateElement(IStateOwner stateOwner, ReorderableList reorderableList, string selectedIndexKey, string name, PersistablePreset preset)
        {
            // The name has to be unique to prevent it from interferring with other state names.
            if (!IsUniqueName(stateOwner.States, name)) {
                var postfixIndex = 1;
                while (!IsUniqueName(stateOwner.States, name + " " + postfixIndex)) {
                    postfixIndex++;
                }
                name += " " + postfixIndex;
            }

            // Create the element.
            var state = new Opsive.Shared.StateSystem.State(name, false);
            state.Preset = preset;
            var stateList = new List<Opsive.Shared.StateSystem.State>(stateOwner.States);
            stateList.Insert(0, state);
            reorderableList.ItemsSource = stateOwner.States = stateList.ToArray();

            // Select the new element.
            reorderableList.SelectedIndex = stateList.Count - 1;
            reorderableList.EnableRemove = true;
            EditorPrefs.SetInt(selectedIndexKey, reorderableList.SelectedIndex);
        }

        /// <summary>
        /// Is the state name unique compared to the other states?
        /// </summary>
        private static bool IsUniqueName(Opsive.Shared.StateSystem.State[] states, string name)
        {
            // A blank string is not unique.
            if (string.IsNullOrEmpty(name)) {
                return false;
            }

            // A name is not unique if it is equal to any other state name.
            for (int i = 0; i < states.Length; ++i) {
                if (states[i].Name == name) {
                    return false;
                }
            }

            return true;
        }
    }
}