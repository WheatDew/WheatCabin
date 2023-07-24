/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.UIElements.Managers
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// A utility window designed to show the configuration for the character item slots.
    /// </summary>
    public class CharacterItemSlotWindow : EditorWindow
    {
        /// <summary>
        /// Specifies the location of the Item Slot.
        /// </summary>
        private enum SlotID
        {
            Right,
            Left,
            Other
        }

        private GameObject[] m_Models;
        private GameObject[][] m_SlotParents;
        private int[][] m_IDs;
        private Action<GameObject[][]> m_OnSlotParentChange;
        private Action<int[][]> m_OnIDChange;

        private bool m_CanBuild = true;
        private bool m_Close;

        public bool CanBuild { get => m_CanBuild; }

        /// <summary>
        /// The window should be closed so it displays the accurate fields.
        /// </summary>
        public void OnDisable()
        {
            m_Close = true;
        }

        /// <summary>
        /// Closes the window after the project has been compiled.
        /// </summary>
        public void OnEnable()
        {
            if (m_Close) {
                EditorApplication.update += CloseWindow;
            }
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        private void CloseWindow()
        {
            EditorApplication.update -= CloseWindow;

            Close();
        }

        /// <summary>
        /// Initializes the window.
        /// </summary>
        public void Initialize(GameObject model, GameObject[] arms, GameObject[][] slotParents, Action<GameObject[][]> onSlotParentChange, int[][] ids, Action<int[][]> onIDChange)
        {
            m_Models = new GameObject[(model != null ? 1 : 0) + (arms != null ? arms.Length : 0)];
            var offset = 0;
            if (model != null) {
                m_Models[0] = model;
                offset = 1;
            }
            if (arms != null) {
                for (int i = 0; i < arms.Length; ++i) {
                    m_Models[i + offset] = arms[i];
                }
            }
            m_SlotParents = slotParents;
            m_OnSlotParentChange = onSlotParentChange;
            m_IDs = ids;
            m_OnIDChange = onIDChange;
            m_CanBuild = true;

            rootVisualElement.styleSheets.Add(Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("e70f56fae2d84394b861a2013cb384d0"));//shared uss
            rootVisualElement.styleSheets.Add(ManagerStyles.StyleSheet);

            BuildVisualElements();
        }

        /// <summary>
        /// Builds the Visual Elements for the window.
        /// </summary>
        public void BuildVisualElements()
        {
            rootVisualElement.styleSheets.Add(Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("a46bc2a887de04846a522116cc71dd3b")); // Controller stylesheet.

            rootVisualElement.Clear();
            m_CanBuild = true;

            var scrollView = new ScrollView();
            rootVisualElement.Add(scrollView);

            if (m_SlotParents == null) {
                m_SlotParents = new GameObject[m_Models.Length][];
                m_IDs = new int[m_Models.Length][];
                m_OnSlotParentChange(m_SlotParents);
                m_OnIDChange(m_IDs);
            } else if (m_SlotParents.Length != m_Models.Length) {
                Array.Resize(ref m_SlotParents, m_Models.Length);
                Array.Resize(ref m_IDs, m_Models.Length);
            }

            for (int i = 0; i < m_Models.Length; ++i) {
                if (m_Models[i] == null) {
                    continue;
                }

                var objectField = new ObjectField(i == 0 ? "Model" : "First Person Arms");
                objectField.objectType = typeof(GameObject);
                objectField.value = m_Models[i];
                objectField.SetEnabled(false);
                scrollView.Add(objectField);

                if (m_SlotParents[i] == null) {
                    m_SlotParents[i] = new GameObject[0];
                    m_IDs[i] = new int[0];
                    m_OnSlotParentChange(m_SlotParents);
                    m_OnIDChange(m_IDs);
                }

                // Sets for error checking.
                var registeredParents = new HashSet<GameObject>();
                var registeredIDs = new HashSet<int>();

                // The model can have any number of slots.
                for (int j = 0; j < m_SlotParents[i].Length + 1; ++j) {
                    ShowSlot(scrollView, i, j, registeredParents, registeredIDs);
                }
            }

            var closeButton = new Button();
            closeButton.text = "Close";
            closeButton.clicked += () =>
            {
                Close();
            };
            scrollView.Add(closeButton);
        }

        /// <summary>
        /// Shows the slot corresponding to the specified model and slot index.
        /// </summary>
        private void ShowSlot(VisualElement container, int modelIndex, int slotIndex, HashSet<GameObject> registeredParents, HashSet<int> registeredIDs)
        {
            var horizontalContainer = new VisualElement();
            horizontalContainer.AddToClassList("horizontal-layout");
            var objectField = new ObjectField(slotIndex == 0 ? "Item Slots" : " ");
            objectField.Q<Label>().AddToClassList("indent");
            objectField.AddToClassList("flex-grow");
            objectField.AddToClassList("flex-shrink");
            objectField.objectType = typeof(GameObject);
            objectField.value = (slotIndex < m_SlotParents[modelIndex].Length ? m_SlotParents[modelIndex][slotIndex] : null);
            objectField.RegisterValueChangedCallback(c =>
            {
                if (slotIndex < m_SlotParents[modelIndex].Length) {
                    m_SlotParents[modelIndex][slotIndex] = (GameObject)c.newValue;
                    if (m_SlotParents[modelIndex][slotIndex] != null && slotIndex == m_SlotParents[modelIndex].Length) {
                        Array.Resize(ref m_SlotParents[modelIndex], m_SlotParents[modelIndex].Length + 1);
                        Array.Resize(ref m_IDs[modelIndex], m_IDs[modelIndex].Length + 1);
                    }
                } else { // The object doesn't exist yet.
                    Array.Resize(ref m_SlotParents[modelIndex], m_SlotParents[modelIndex].Length + 1);
                    Array.Resize(ref m_IDs[modelIndex], m_IDs[modelIndex].Length + 1);
                    m_SlotParents[modelIndex][slotIndex] = (GameObject)c.newValue;
                    if (slotIndex > 0) {
                        m_IDs[modelIndex][slotIndex] = m_IDs[modelIndex][slotIndex - 1] + 1;
                    }
                }

                // Try to automatically assign the correct ID.
                if (m_SlotParents[modelIndex][slotIndex] != null) {
                    var parentName = m_SlotParents[modelIndex][slotIndex].name.ToLowerInvariant();
                    if (parentName.Contains("left") || parentName.EndsWith("-l") || parentName.EndsWith(".l")) {
                        var hasLeftID = false;
                        for (int i = 0; i < m_IDs[modelIndex].Length; ++i) {
                            if (i == slotIndex) {
                                continue;
                            }
                            if (m_IDs[modelIndex][i] == (int)SlotID.Left) {
                                hasLeftID = true;
                                break;
                            }
                        }
                        if (!hasLeftID) {
                            m_IDs[modelIndex][slotIndex] = (int)SlotID.Left;
                        }
                    } else if (parentName.Contains("right") || parentName.EndsWith("-r") || parentName.EndsWith(".r")) {
                        var hasRightID = false;
                        for (int i = 0; i < m_IDs[modelIndex].Length; ++i) {
                            if (i == slotIndex) {
                                continue;
                            }
                            if (m_IDs[modelIndex][i] == (int)SlotID.Right) {
                                hasRightID = true;
                                break;
                            }
                        }
                        if (!hasRightID) {
                            m_IDs[modelIndex][slotIndex] = (int)SlotID.Right;
                        }
                    }
                }

                BuildVisualElements();
                m_OnSlotParentChange(m_SlotParents);
                m_OnIDChange(m_IDs);
            });
            horizontalContainer.Add(objectField);

            if (slotIndex < m_SlotParents[modelIndex].Length && m_SlotParents[modelIndex][slotIndex] != null) {
                var idType = (SlotID)(m_IDs[modelIndex][slotIndex] < (int)SlotID.Other ? m_IDs[modelIndex][slotIndex] : (int)SlotID.Other);
                var idTypeField = new EnumField(idType);
                var idIntField = new IntegerField();
                idTypeField.RegisterValueChangedCallback(c =>
                {
                    m_IDs[modelIndex][slotIndex] = (int)(SlotID)c.newValue;
                    idIntField.value = m_IDs[modelIndex][slotIndex];
                    idIntField.SetEnabled(m_IDs[modelIndex][slotIndex] > 1);
                    BuildVisualElements();
                    m_OnIDChange(m_IDs);
                });
                idTypeField.style.width = 65;
                var idLabel = new Label("ID");
                idLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                idLabel.style.marginLeft = 8;
                horizontalContainer.Add(idLabel);
                horizontalContainer.Add(idTypeField);

                idIntField.value = m_IDs[modelIndex][slotIndex];
                idIntField.RegisterValueChangedCallback(c =>
                {
                    m_IDs[modelIndex][slotIndex] = c.newValue;
                    BuildVisualElements();
                    m_OnIDChange(m_IDs);
                });
                idIntField.SetEnabled(m_IDs[modelIndex][slotIndex] > 1);
                idIntField.style.width = 45;
                horizontalContainer.Add(idIntField);
            }

            if (slotIndex < m_SlotParents[modelIndex].Length) {
                var removeButton = new Button();
                removeButton.text = "-";
                removeButton.clicked += () =>
                {
                    ArrayUtility.RemoveAt(ref m_IDs[modelIndex], slotIndex);
                    ArrayUtility.RemoveAt(ref m_SlotParents[modelIndex], slotIndex);
                    BuildVisualElements();
                    m_OnSlotParentChange(m_SlotParents);
                    m_OnIDChange(m_IDs);
                };
                horizontalContainer.Add(removeButton);
            }

            container.Add(horizontalContainer);

            if (m_CanBuild && slotIndex < m_SlotParents[modelIndex].Length) {
                var errorMessage = string.Empty;
                if (m_SlotParents[modelIndex] != null && (m_SlotParents[modelIndex][slotIndex] == m_Models[modelIndex] || 
                        !m_SlotParents[modelIndex][slotIndex].transform.IsChildOf(m_Models[modelIndex].transform))) {
                    errorMessage = $"The slot parent must be a child of the {(modelIndex == 0 ? "model" : "first person arms")}.";
                } else if (registeredParents.Contains(m_SlotParents[modelIndex][slotIndex])) {
                    errorMessage = "The Item Slot parents must be unique.";
                } else if (registeredIDs.Contains(m_IDs[modelIndex][slotIndex])) {
                    errorMessage = "The Item Slot ID must be unique.";
                } else {
                    registeredParents.Add(m_SlotParents[modelIndex][slotIndex]);
                    registeredIDs.Add(m_IDs[modelIndex][slotIndex]);
                }
                if (!string.IsNullOrEmpty(errorMessage)) {
                    m_CanBuild = false;
                    var helpBox = new HelpBox(errorMessage, HelpBoxMessageType.Error);
                    container.Add(helpBox);
                }
            }
        }
    }
}