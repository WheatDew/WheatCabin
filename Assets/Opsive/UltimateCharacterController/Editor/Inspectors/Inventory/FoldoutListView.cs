/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The styling for the foldout list.
    /// </summary>
    public static class FoldoutListViewStyles
    {
        public static StyleSheet StyleSheet => Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("b62c5fc864a82e9458d17c82a2eea394");
        
        public static string StyleBackground => EditorGUIUtility.isProSkin ? "foldout-background-dark" : "foldout-background-light";
        public static string StyleButtonBorder => EditorGUIUtility.isProSkin ? "button-border-dark" : "button-border-light";

        public static string FoldoutListViewElement_Header => "foldout-list-view-element_header";
        public static string ListElementTitle => "foldout-list-view-element_foldout-header_title";
        public static string FoldoutListViewElement_Content => "foldout-list-view-element_content";
        public static string ListElementOptions => "foldout-list-view-element_foldout-header_options";
    }
    
    /// <summary>
    /// The foldout list header is a simple foldout with a different styling.
    /// </summary>
    public class FoldoutHeader : Foldout
    {
        
    }

    /// <summary>
    /// The Element of a foldout list contains a header and a content.
    /// </summary>
    public class FoldoutListViewElement : VisualElement
    {
        protected int m_Index;
        private FoldoutHeader m_Header;
        protected VisualElement m_Content;
        
        public int Index { get => m_Index; set => m_Index = value; }
        public FoldoutHeader Header { get => m_Header; set => m_Header = value; }
        public VisualElement Content { get => m_Content; set => m_Content = value; }

        public FoldoutListViewElement()
        {
            m_Index = -1;
            m_Header = new FoldoutHeader();
            m_Header.AddToClassList(FoldoutListViewStyles.FoldoutListViewElement_Header);
            m_Header.RegisterValueChangedCallback(evt =>
            {
                m_Content.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            Add(m_Header);

            m_Content = new VisualElement();
            m_Content.AddToClassList(FoldoutListViewStyles.FoldoutListViewElement_Content);
            Add(m_Content);
        }
    }
    
    /// <summary>
    /// A foldout list view is similar in functionality as a Reorderable list but in addition it has a foldout title.
    /// This makes it more suitable when having many nested lists.
    /// </summary>
    public class FoldoutListView: VisualElement
    {
        protected IList m_List;
        protected List<FoldoutListViewElement> m_ElementList;
        
        private VisualElement m_Header;
        private VisualElement m_Content;
        private Label m_EmptyListNotice;
        private ScrollView m_ScrollView;
        public ScrollView ScrollView => m_ScrollView;

        private Action<FoldoutListViewElement, int> m_OnMakeItem;
        private Action<FoldoutListViewElement, int> m_OnBindItem;
        private Action<VisualElement> m_OnMakeHeader;
        private Action m_OnAdd;
        private Action<int> m_OnRemove;
        private Action<int, int> m_OnReorder;

        /// <summary>
        /// FoldoutListView constructor.
        /// </summary>
        /// <param name="itemsSource">The list of items.</param>
        /// <param name="makeItem">The callback when each element is being created.</param>
        /// <param name="bindItem">the callback when the element is binding to the data.</param>
        /// <param name="makeHeader">The list header (can be null).</param>
        /// <param name="onAdd">The callback when the add button is pressed (can be null).</param>
        /// <param name="onRemove">The callback when the remove button is pressed (can be null).</param>
        /// <param name="onReorder">The callback when elements are reordered.</param>
        public FoldoutListView(IList itemsSource, Action<FoldoutListViewElement, int> makeItem, Action<FoldoutListViewElement, int> bindItem, Action<VisualElement> makeHeader,
            Action onAdd, Action<int> onRemove, Action<int, int> onReorder)
        {
            styleSheets.Add(FoldoutListViewStyles.StyleSheet);
            
            m_List = itemsSource;
            
            m_ElementList = new List<FoldoutListViewElement>();

            m_OnMakeItem = makeItem;
            m_OnBindItem = bindItem;
            m_OnMakeHeader = makeHeader;
            m_OnRemove = onRemove;
            m_OnAdd = onAdd;
            m_OnReorder = onReorder;

            m_Header = new VisualElement();
            m_Header.name = "foldout-header";
            m_Header.AddToClassList(FoldoutListViewStyles.StyleBackground);
            Add(m_Header);

            m_Content = new VisualElement();
            m_Content.name = "foldout-content";
            m_Content.AddToClassList(FoldoutListViewStyles.StyleBackground);
            Add(m_Content);

            m_EmptyListNotice = new Label("The list is empty");
            
            Refresh();
        }

        /// <summary>
        /// Refresh the list showing the updated data.
        /// </summary>
        public void Refresh()
        {
            m_Header.Clear();
            m_Content.Clear();

            m_OnMakeHeader?.Invoke(m_Header);

            if (m_List == null || m_List.Count == 0) {
                if (m_OnAdd != null) {
                    var addButton = new Button();
                    addButton.text = "+";
                    addButton.clicked += () =>
                    {
                        m_OnAdd.Invoke();
                    };
                    addButton.SetEnabled(!Application.isPlaying);
                    m_Header.Add(addButton);
                }

                m_Content.Add(m_EmptyListNotice);
                return;
            }
            
            if (m_ElementList.Count < m_List.Count) {
                for (int i = m_ElementList.Count; i < m_List.Count; i++) {
                    var listElement = new FoldoutListViewElement();
                    listElement.Index = i;
                    m_OnMakeItem.Invoke(listElement, i);

                    var listElementOptions = new VisualElement();
                    listElementOptions.AddToClassList(FoldoutListViewStyles.ListElementOptions);
                    listElement.Header.Q<Toggle>().ElementAt(0).AddToClassList(FoldoutListViewStyles.ListElementTitle);
                    listElement.Header.Q<Toggle>().Add(listElementOptions);

                    if (m_OnReorder != null) {
                        var moveDownButton = new Button();
                        moveDownButton.text = "↓";
                        moveDownButton.clicked += () =>
                        {
                            m_OnReorder.Invoke(listElement.Index, listElement.Index + 1);
                        };
                        moveDownButton.SetEnabled(!Application.isPlaying);
                        listElementOptions.Add(moveDownButton);
                        var moveUpButton = new Button();
                        moveUpButton.text = "↑";
                        moveUpButton.clicked += () =>
                        {
                            m_OnReorder.Invoke(listElement.Index, listElement.Index - 1);
                        };
                        moveUpButton.SetEnabled(!Application.isPlaying);
                        listElementOptions.Add(moveUpButton);
                    }

                    if (m_OnRemove != null) {
                        var removeButton = new Button();
                        removeButton.text = "-";
                        removeButton.clicked += () =>
                        {
                            m_OnRemove.Invoke(listElement.Index);
                        };
                        removeButton.SetEnabled(!Application.isPlaying);
                        listElementOptions.Add(removeButton);
                    }

                    if (m_OnAdd != null) {
                        var addButton = new Button();
                        addButton.text = "+";
                        addButton.clicked += () =>
                        {
                            m_OnAdd.Invoke();
                        };
                        addButton.SetEnabled(!Application.isPlaying);
                        listElementOptions.Add(addButton);
                    }

                    m_ElementList.Add(listElement);
                }
            }

            for (int i = 0; i < m_List.Count; i++) {
                var listElement = m_ElementList[i];
                m_OnBindItem.Invoke(listElement, i);
                m_Content.Add(listElement);
            }
        }

        /// <summary>
        /// Refresh the list showing the updated data.
        /// </summary>
        /// <param name="list">The new list to show.</param>
        public void Refresh(IList list)
        {
            m_List = list;
            Refresh();
        }
    }
}