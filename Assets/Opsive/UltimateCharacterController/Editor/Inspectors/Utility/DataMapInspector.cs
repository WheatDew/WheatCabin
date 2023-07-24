/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Utility
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Base inspector for the DataMap class.
    /// </summary>
    public abstract class DataMapInspector<T> : UIElementsInspector
    {
        public static string StyleClassName => ListElement.c_StyleClassName;

        private string m_CurrentName;

        protected override List<string> ExcludedFields { get => new List<string>() { "m_EditableObjects", "m_ReadOnlyObjects" }; }

        protected DataMap<T> m_DataMap;
        protected List<T> m_DataMapList;

        private ReorderableList m_ReorderableList;
        private VisualElement m_WarningHelpBox;

        /// <summary>
        /// Returns the title of the ReorderableList.
        /// </summary>
        protected abstract string ReorderableListTitle { get; }

        /// <summary>
        /// Returns a new ListElement for the implementation class.
        /// </summary>
        /// <param name="onChange">Callback when the element has changed.</param>
        /// <returns>A new ListElement for the implementation class.</returns>
        protected abstract ListElement GetListElement(Action<int, T> onChange);

        /// <summary>
        /// Shows the sorting elements.
        /// </summary>
        /// <param name="container">The parentContainer.</param>
        protected abstract void ShowSortingElements(VisualElement container);

        /// <summary>
        /// Refreshes the objects.
        /// </summary>
        protected abstract void DoRefresh();

        /// <summary>
        /// Initialzies the inspector.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_DataMap = target as DataMap<T>;
            m_DataMap.OnValidate();

            base.InitializeInspector();
        }

        /// <summary>
        /// Adds elements to the bottom of the inspector.
        /// </summary>
        /// <param name="container">The container that the elements should be added to.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            if (m_DataMap.EditableObjects == null) {
                m_DataMap.EditableObjects = new T[0];
            }
            m_DataMapList = new List<T>(m_DataMap.AllObjects);

            m_WarningHelpBox = new HelpBox("The data in the file is invalid. Please ensure there are no duplicate values.", HelpBoxMessageType.Warning);
            m_WarningHelpBox.style.flexShrink = 0;
            m_WarningHelpBox.style.display = DisplayStyle.None;
            container.Add(m_WarningHelpBox);

            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            horizontalLayout.style.height = horizontalLayout.style.minHeight = horizontalLayout.style.maxHeight = 20;
            container.Add(horizontalLayout);

            var addField = new TextField("New Item");
            var addButton = new Button();
            addField.value = m_CurrentName != null ? m_CurrentName.ToString() : string.Empty;
            addField.style.flexGrow = 1;
            addField.RegisterValueChangedCallback(c =>
            {
                m_CurrentName = c.newValue;
                addButton.SetEnabled(m_DataMap.IsNameValid(m_CurrentName));
            });
            horizontalLayout.Add(addField);

            addButton.text = "Add";
            addButton.SetEnabled(m_DataMap.IsNameValid(m_CurrentName));
            addButton.clicked += () =>
            {
                m_DataMap.AddName(m_CurrentName);
                Refresh();
                addField.value = string.Empty;
            };
            horizontalLayout.Add(addButton);

            ShowSortingElements(container);

            m_ReorderableList = new ReorderableList(
                m_DataMapList, 
                (VisualElement container, int index) => // Make item.
                {
                    container.Add(GetListElement((index, newValue) =>
                    {
                        var editableIndex = Array.IndexOf(m_DataMap.EditableObjects, m_DataMapList[index]);
                        var editableNameIDs = m_DataMap.EditableObjects;
                        m_DataMapList[index] = editableNameIDs[editableIndex] = newValue;
                        m_DataMap.EditableObjects = editableNameIDs;
                        Shared.Editor.Utility.EditorUtility.SetDirty(target);
                        m_ReorderableList.Refresh();
                    }));
                }, (VisualElement container, int index) => // Bind item.
                {
                    var listElement = container.ElementAt(0) as ListElement;
                    listElement.SetEnabled(Array.IndexOf(m_DataMap.EditableObjects, m_DataMapList[index]) != -1);
                    listElement.Bind(m_DataMapList[index], index);
                }, (container) => // Header.
                {
                    container.Add(new Label(ReorderableListTitle));
                }, (int index) => // Selection.
                {
                    // Don't let the read only elements be selected.
                    if (m_DataMap.EditableObjects == null || index >= m_DataMap.EditableObjects.Length) {
                        m_ReorderableList.SelectedIndex = -1;
                    }
                }, null, (int index) => // Remove.
                {
                    m_DataMap.Remove(index);
                }, null);

            container.Add(m_ReorderableList);

            m_DataMap.OnValidateEvent += OnValidate;

            Refresh();
        }

        /// <summary>
        /// Refreshes the objects when the editor is validated.
        /// </summary>
        private void OnValidate()
        {
            Refresh();
        }

        /// <summary>
        /// Refreshes the objects.
        /// </summary>
        protected virtual void Refresh()
        {
            if (m_WarningHelpBox == null) {
                return;
            }

            m_DataMapList.Clear();
            m_DataMapList.AddRange(m_DataMap.AllObjects);

            DoRefresh();

            if (m_DataMap.IsDataValid()) {
                m_WarningHelpBox.style.display = DisplayStyle.None;
            } else {
                m_WarningHelpBox.style.display = DisplayStyle.Flex;
            }

            m_ReorderableList.Refresh();
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            if (m_DataMap == null) {
                return;
            }
            m_DataMap.OnValidateEvent -= OnValidate;
        }

        /// <summary>
        /// Represents an element within the ReorderableList.
        /// </summary>
        public abstract class ListElement : VisualElement
        {
            public const string c_StyleClassName = "data-map-list-element";

            protected DataMapInspector<T> m_Inspector;
            protected Action<int, T> m_OnChange;

            protected int m_Index;
            protected VisualElement m_WarningIndicator;

            /// <summary>
            /// Makes a new element.
            /// </summary>
            /// <param name="inspector">The parent inspector.</param>
            public ListElement(DataMapInspector<T> inspector, Action<int, T> onChange)
            {
                m_Inspector = inspector;
                m_OnChange = onChange;

                m_WarningIndicator = new VisualElement();
                m_WarningIndicator.AddToClassList(c_StyleClassName + "_warning");
                m_WarningIndicator.style.backgroundImage = new StyleBackground(
                    Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("479498807a425664db202c18464e8ff0"));

                AddToClassList(c_StyleClassName);
                Add(m_WarningIndicator);
            }

            /// <summary>
            /// Binds the list to the new NameID.
            /// </summary>
            /// <param name="obj">A reference to the NameID.</param>
            /// <param name="index">The index of the element.</param>
            public virtual void Bind(T obj, int index)
            {
                m_Index = index;

                m_WarningIndicator.style.display = DisplayStyle.None;
                m_WarningIndicator.tooltip = string.Empty;
            }
        }
    }
}