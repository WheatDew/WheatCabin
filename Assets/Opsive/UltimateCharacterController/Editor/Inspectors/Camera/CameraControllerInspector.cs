/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Camera
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.Inspectors;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shows a custom inspector for the CameraController.
    /// </summary>
    [CustomEditor(typeof(CameraController))]
    public class CameraControllerInspector : UIStateBehaviorInspector
    {
        private const string c_EditorPrefsSelectedViewTypeIndexKey = "Opsive.UltimateCharacterController.Editor.Inspectors.Camera.SelectedViewTypeIndex";
        private string SelectedViewTypeIndexKey { get { return c_EditorPrefsSelectedViewTypeIndexKey + "." + target.GetType() + "." + target.name; } }

        private VisualElement m_DualPerspectiveContainer;
        private DropdownField m_FirstPersonViewTypeDropdown;
        private DropdownField m_ThirdPersonViewTypeDropdown;
        private List<string> m_FirstPersonViewTypeNames;
        private List<string> m_ThirdPersonViewTypeNames;
        private Type[] m_FirstPersonViewTypes;
        private Type[] m_ThirdPersonViewTypes;

        private CameraController m_CameraController;
        private ReorderableList m_ViewTypeReorderableList;
        private VisualElement m_SelectedViewTypeContainer;

        protected override bool ExcludeAllFields => true;

        /// <summary>
        /// Initialize the inspector.
        /// </summary>
        protected override void InitializeInspector()
        {
            base.InitializeInspector();

            m_CameraController = target as CameraController;
        }

        /// <summary>
        /// Adds the custom UIElements to the top of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowHeaderElements(VisualElement container)
        {
            base.ShowHeaderElements(container);

            var foldout = new Foldout() { text = "Character" };
            container.Add(foldout);
            FieldInspectorView.AddField(target, "m_InitCharacterOnAwake", foldout);
            FieldInspectorView.AddField(target, "m_Character", foldout);
            FieldInspectorView.AddField(target, "m_Anchor", foldout);
            FieldInspectorView.AddField(target, "m_AutoAnchor", foldout);
            FieldInspectorView.AddField(target, "m_AutoAnchorBone", foldout);
            FieldInspectorView.AddField(target, "m_AnchorOffset", foldout);

            AddViewTypeList(container);

            foldout = new Foldout() { text = "Zoom" };
            container.Add(foldout);
            FieldInspectorView.AddField(target, "m_CanZoom", foldout);
            FieldInspectorView.AddField(target, "m_ZoomState", foldout);
            FieldInspectorView.AddField(target, "m_StateAppendItemIdentifierName", foldout);

            foldout = new Foldout() { text = "Events" };
            container.Add(foldout);
            foldout.Add(GetPropertyField("m_OnChangeViewTypesEvent"));
            foldout.Add(GetPropertyField("m_OnChangePerspectivesEvent"));
            foldout.Add(GetPropertyField("m_OnZoomEvent"));
        }

        /// <summary>
        /// Adds the ViewType ReorderableList.
        /// </summary>
        /// <param name="container">The container of the list.</param>
        private void AddViewTypeList(VisualElement container)
        {
            if (target == null) {
                return;
            }

            var foldout = new Foldout() { text = "View Types" };
            foldout.contentContainer.AddToClassList("contained-list");
            container.Add(foldout);

            m_DualPerspectiveContainer = new VisualElement();
            foldout.Add(m_DualPerspectiveContainer);

            m_FirstPersonViewTypeDropdown = new DropdownField("First Person View Type");
            // Ensure the control is kept up to date as the value changes.
            var firstPersonViewTypeField = InspectorUtility.GetField(target, "m_FirstPersonViewTypeFullName");
            if (firstPersonViewTypeField != null) {
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    m_FirstPersonViewTypeDropdown.SetValueWithoutNotify(InspectorUtility.DisplayTypeName(TypeUtility.GetType(newValue as string), false));
                };
                m_FirstPersonViewTypeDropdown.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(firstPersonViewTypeField, -1, target, onBindingUpdateEvent);
                });
                m_FirstPersonViewTypeDropdown.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
            }
            m_FirstPersonViewTypeDropdown.RegisterValueChangedCallback(c =>
            {
                // Clear out the old.
                m_FirstPersonViewTypeDropdown.SetValueWithoutNotify(c.newValue);
                c.StopPropagation();
                if (m_FirstPersonViewTypeDropdown.index > -1) {
                    m_CameraController.FirstPersonViewTypeFullName = m_FirstPersonViewTypes[m_FirstPersonViewTypeDropdown.index].FullName;
                    EditorUtility.SetDirty(m_CameraController);
                }
            });
            m_DualPerspectiveContainer.Add(m_FirstPersonViewTypeDropdown);

            m_ThirdPersonViewTypeDropdown = new DropdownField("Third Person View Type");
            // Ensure the control is kept up to date as the value changes.
            var thirdPersonViewTypeField = InspectorUtility.GetField(target, "m_ThirdPersonViewTypeFullName");
            if (thirdPersonViewTypeField != null) {
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    m_ThirdPersonViewTypeDropdown.SetValueWithoutNotify(InspectorUtility.DisplayTypeName(TypeUtility.GetType(newValue as string), false));
                };
                m_ThirdPersonViewTypeDropdown.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(thirdPersonViewTypeField, -1, target, onBindingUpdateEvent);
                });
                m_ThirdPersonViewTypeDropdown.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
            }
            m_ThirdPersonViewTypeDropdown.RegisterValueChangedCallback(c =>
            {
                // Clear out the old.
                m_ThirdPersonViewTypeDropdown.SetValueWithoutNotify(c.newValue);
                c.StopPropagation();
                if (m_ThirdPersonViewTypeDropdown.index > -1) {
                    m_CameraController.ThirdPersonViewTypeFullName = m_ThirdPersonViewTypes[m_ThirdPersonViewTypeDropdown.index].FullName;
                    EditorUtility.SetDirty(m_CameraController);
                }
            });
            m_DualPerspectiveContainer.Add(m_ThirdPersonViewTypeDropdown);

            FieldInspectorView.AddField(target, "m_CanChangePerspectives", m_DualPerspectiveContainer);

            m_ViewTypeReorderableList = new ReorderableList(m_CameraController.ViewTypes, (VisualElement container, int index) => // Add.
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
                if (m_CameraController.ViewTypes[index] == null) {
                    label.text = "(Unknown View Type)";
                    return;
                }
                label.text = InspectorUtility.DisplayTypeName(m_CameraController.ViewTypes[index].GetType(), true);

                // Map the radio button to the active view type.
                var radioButton = container.Q<RadioButton>();
                radioButton.SetValueWithoutNotify(m_CameraController.ViewTypeFullName == m_CameraController.ViewTypes[index].GetType().FullName);
                var activeMovementTypeField = InspectorUtility.GetField(m_CameraController, "m_ActiveViewType");
                Action<object> movementTypeBindingUpdateEvent = (object newValue) =>
                {
                    radioButton.SetValueWithoutNotify(m_CameraController.ViewTypeFullName == m_CameraController.ViewTypes[index].GetType().FullName);
                };
                radioButton.RegisterValueChangedCallback<bool>((c) =>
                {
                    if (c.newValue == false) {
                        return;
                    }

                    m_CameraController.ViewTypeFullName = m_CameraController.ViewTypes[index].GetType().FullName;
                    if (m_CameraController.ViewTypes[index].GetType().FullName.Contains("FirstPerson")) {
                        m_CameraController.FirstPersonViewTypeFullName = m_CameraController.ViewTypeFullName;
                    } else if (m_CameraController.ViewTypes[index].GetType().FullName.Contains("ThirdPerson")) {
                        m_CameraController.ThirdPersonViewTypeFullName = m_CameraController.ViewTypeFullName;
                    }
                    Shared.Editor.Utility.EditorUtility.SetDirty(m_CameraController);
                });
                radioButton.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(activeMovementTypeField, -1, m_CameraController, movementTypeBindingUpdateEvent);
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

                var label = new Label("View Types");
                label.AddToClassList("flex-grow");
                horizontalLayout.Add(label);
                label = new Label("Active");
                label.name = "serialized-reference-list-right-label";
                horizontalLayout.Add(label);
            }, (int index) => // Select.
            {
                if (ShowViewType(index)) {
                    EditorPrefs.SetInt(SelectedViewTypeIndexKey, m_ViewTypeReorderableList.SelectedIndex);
                }
            }, () => // Add.
            {
                ReorderableListSerializationHelper.AddObjectType(typeof(ViewType), true, m_CameraController.ViewTypes, AddViewType);
            }, (int index) => // Remove.
            {
                var removeViewType = m_CameraController.ViewTypes[m_ViewTypeReorderableList.SelectedIndex];
                var firstPersonPerspective = removeViewType.FirstPersonPerspective;
                var selected = m_CameraController.ViewTypeFullName == removeViewType.GetType().FullName;

                // Allow the view type to perform any destruction.
                var viewTypeDrawer = Controls.Types.ViewTypeDrawers.ViewTypeDrawerUtility.FindViewTypeDrawer(removeViewType.GetType(), true);
                if (viewTypeDrawer != null) {
                    viewTypeDrawer.ViewTypeRemoved(removeViewType, target);
                }

                var viewTypesList = new List<ViewType>(m_CameraController.ViewTypes);
                viewTypesList.RemoveAt(m_ViewTypeReorderableList.SelectedIndex);
                m_ViewTypeReorderableList.ItemsSource = m_CameraController.ViewTypes = viewTypesList.ToArray();
                if (firstPersonPerspective) {
                    m_CameraController.FirstPersonViewTypeFullName = string.Empty;
                } else {
                    m_CameraController.ThirdPersonViewTypeFullName = string.Empty;
                }

                // Select the next available ViewType.
                for (int i = 0; i < viewTypesList.Count; ++i) {
                    if (viewTypesList[i].FirstPersonPerspective == firstPersonPerspective) {
                        if (firstPersonPerspective) {
                            m_CameraController.FirstPersonViewTypeFullName = viewTypesList[i].GetType().FullName;
                            if (selected) {
                                m_CameraController.ViewTypeFullName = m_CameraController.FirstPersonViewTypeFullName;
                            }
                        } else {
                            m_CameraController.ThirdPersonViewTypeFullName = viewTypesList[i].GetType().FullName;
                            if (selected) {
                                m_CameraController.ViewTypeFullName = m_CameraController.ThirdPersonViewTypeFullName;
                            }
                        }
                        break;
                    }
                }

                Shared.Editor.Utility.EditorUtility.SetDirty(m_CameraController);
                m_SelectedViewTypeContainer.Clear();

                // Update the index to point to no longer point to the now deleted ability.
                var selectedIndex = index - 1;
                if (selectedIndex == -1 && m_CameraController.ViewTypes.Length > 0) {
                    selectedIndex = 0;
                }
                m_ViewTypeReorderableList.SelectedIndex = selectedIndex;
                EditorPrefs.SetInt(SelectedViewTypeIndexKey, m_ViewTypeReorderableList.SelectedIndex);
                m_ViewTypeReorderableList.EnableRemove = viewTypesList.Count > 1;

                UpdateDefaultViewTypes();
            }, (int fromIndex, int toIndex) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(m_CameraController);
            });
            m_ViewTypeReorderableList.EnableRemove = m_CameraController.ViewTypes != null && m_CameraController.ViewTypes.Length > 1;
            foldout.Add(m_ViewTypeReorderableList);

            m_SelectedViewTypeContainer = new VisualElement();
            foldout.Add(m_SelectedViewTypeContainer);

            if (m_CameraController.ViewTypes != null) {
                var selectedIndex = EditorPrefs.GetInt(SelectedViewTypeIndexKey, -1);
                if (selectedIndex != -1 && selectedIndex < m_CameraController.ViewTypes.Length) {
                    m_ViewTypeReorderableList.SelectedIndex = selectedIndex;
                }
            }

            UpdateDefaultViewTypes();
        }

        /// <summary>
        /// Shows the selected view type.
        /// </summary>
        /// <param name="index">The index of the view type.</param>
        /// <returns>True if the view type was shown.</returns>
        private bool ShowViewType(int index)
        {
            m_SelectedViewTypeContainer.Clear();
            var viewType = m_CameraController.ViewTypes[index];
            if (viewType == null) {
                return false;
            }

            var label = new Label(InspectorUtility.DisplayTypeName(viewType.GetType(), true));
            label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            m_SelectedViewTypeContainer.Add(label);
            FieldInspectorView.AddFields(m_CameraController, viewType, Shared.Utility.MemberVisibility.Public, m_SelectedViewTypeContainer, (obj) =>
            {
                m_CameraController.ViewTypes[index] = obj as ViewType; // The view type reference may have been updated.
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(target, "Change Value");
            }, null, null, true);
            return true;
        }

        /// <summary>
        /// Adds the view type with the specified type.
        /// </summary>
        private void AddViewType(object obj)
        {
            var viewTypes = m_CameraController.ViewTypes;
            if (viewTypes == null) {
                viewTypes = new ViewType[1];
            } else {
                Array.Resize(ref viewTypes, viewTypes.Length + 1);
            }
            var viewType = Activator.CreateInstance(obj as Type) as ViewType;
            viewTypes[viewTypes.Length - 1] = viewType;
            m_ViewTypeReorderableList.ItemsSource = m_CameraController.ViewTypes = viewTypes;
            if (viewTypes.Length == 1) {
                m_CameraController.ViewTypeFullName = viewTypes[0].GetType().FullName;
            }
            Shared.Editor.Utility.EditorUtility.SetDirty(m_CameraController);
            m_ViewTypeReorderableList.EnableRemove = viewTypes.Length > 1;

            UpdateDefaultViewTypes();

            m_ViewTypeReorderableList.SelectedIndex = viewTypes.Length - 1;
            EditorPrefs.SetInt(SelectedViewTypeIndexKey, m_ViewTypeReorderableList.SelectedIndex);

            // Allow the view type to perform any initialization.
            var viewTypeDrawer = Controls.Types.ViewTypeDrawers.ViewTypeDrawerUtility.FindViewTypeDrawer(viewType.GetType(), true);
            if (viewTypeDrawer != null) {
                viewTypeDrawer.ViewTypeAdded(viewType, target);
            }
        }

        /// <summary>
        /// Updates the default first/third view type based on the view types availabe on the camera controller.
        /// </summary>
        private void UpdateDefaultViewTypes()
        {
            // The view type may not exist anymore.
            if (Shared.Utility.TypeUtility.GetType(m_CameraController.FirstPersonViewTypeFullName) == null) {
                m_CameraController.FirstPersonViewTypeFullName = string.Empty;
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
            }
            if (Shared.Utility.TypeUtility.GetType(m_CameraController.ThirdPersonViewTypeFullName) == null) {
                m_CameraController.ThirdPersonViewTypeFullName = string.Empty;
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
            }

            var hasSelectedViewType = false;
            m_FirstPersonViewTypeNames = new List<string>();
            m_ThirdPersonViewTypeNames = new List<string>();
            var firstPersonViewTypes = new List<Type>();
            var thirdPersonViewTypes = new List<Type>();
            var selectedFirstPersonIndex = -1;
            var selectedThirdPersonIndex = -1;
            var viewTypes = m_CameraController.ViewTypes;
            if (viewTypes != null) {
                for (int i = 0; i < viewTypes.Length; ++i) {
                    if (viewTypes[i] == null) {
                        continue;
                    }
                    // Transition view types are not limited to one perspective.
                    if (viewTypes[i] is UltimateCharacterController.Camera.ViewTypes.Transition) {
                        continue;
                    }
                    if (viewTypes[i].FirstPersonPerspective) {
                        // Use the view type if the type is currently empty.
                        if (string.IsNullOrEmpty(m_CameraController.FirstPersonViewTypeFullName)) {
                            m_CameraController.FirstPersonViewTypeFullName = viewTypes[i].GetType().FullName;
                        }
                        firstPersonViewTypes.Add(viewTypes[i].GetType());
                        m_FirstPersonViewTypeNames.Add(InspectorUtility.DisplayTypeName(viewTypes[i].GetType(), false));
                        if (viewTypes[i].GetType().FullName == m_CameraController.FirstPersonViewTypeFullName) {
                            selectedFirstPersonIndex = m_FirstPersonViewTypeNames.Count - 1;
                        }
                    } else { // Third Person.
                        // Use the view type if the type is currently empty.
                        if (string.IsNullOrEmpty(m_CameraController.ThirdPersonViewTypeFullName)) {
                            m_CameraController.ThirdPersonViewTypeFullName = viewTypes[i].GetType().FullName;
                        }
                        thirdPersonViewTypes.Add(viewTypes[i].GetType());
                        m_ThirdPersonViewTypeNames.Add(InspectorUtility.DisplayTypeName(viewTypes[i].GetType(), false));
                        if (viewTypes[i].GetType().FullName == m_CameraController.ThirdPersonViewTypeFullName) {
                            selectedThirdPersonIndex = m_ThirdPersonViewTypeNames.Count - 1;
                        }
                    }

                    if (m_CameraController.ViewTypeFullName == viewTypes[i].GetType().FullName) {
                        hasSelectedViewType = true;
                    }
                }
            }
            m_FirstPersonViewTypes = firstPersonViewTypes.ToArray();
            m_ThirdPersonViewTypes = thirdPersonViewTypes.ToArray();

            // If the selected ViewType no longer exists in the list then select the next view type.
            if (!hasSelectedViewType) {
                m_CameraController.ViewTypeFullName = string.Empty;
                if (viewTypes != null && viewTypes.Length > 0) {
                    for (int i = 0; i < viewTypes.Length; ++i) {
                        // Transition ViewTypes cannot be selected.
                        if (viewTypes[i] is UltimateCharacterController.Camera.ViewTypes.Transition) {
                            continue;
                        }

                        m_CameraController.ViewTypeFullName = viewTypes[i].GetType().FullName;
                        break;
                    }
                }
            }

            var dualPerspectives = !string.IsNullOrEmpty(m_CameraController.FirstPersonViewTypeFullName) && !string.IsNullOrEmpty(m_CameraController.ThirdPersonViewTypeFullName);
            m_DualPerspectiveContainer.style.display = dualPerspectives ? DisplayStyle.Flex : DisplayStyle.None;
            if (!dualPerspectives) {
                return;
            }

            UIElementsUtility.SetDropdownFieldChoices(m_FirstPersonViewTypeDropdown, m_FirstPersonViewTypeNames);
            m_FirstPersonViewTypeDropdown.index = selectedFirstPersonIndex;
            UIElementsUtility.SetDropdownFieldChoices(m_ThirdPersonViewTypeDropdown, m_ThirdPersonViewTypeNames);
            m_ThirdPersonViewTypeDropdown.index = selectedThirdPersonIndex;
        }
    }
}