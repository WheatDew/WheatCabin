/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Utility
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Searchable list allowing for searching and sorting a ReorderableList.
    /// </summary>
    public class SearchableList<T> : VisualElement
    {
        protected IList<T> m_ItemSource;
        protected List<T> m_SearchedItemSource;
        protected T m_PreviousSelectedObject;
        protected string m_PreviousSearch;
        protected Action<int> m_OnSelection;

        protected ToolbarSearchField m_SearchField;
        protected ReorderableList m_ReorderableList;

        public List<T> ItemList => m_SearchedItemSource;

        public bool selectOnRefresh = true;

        public int SelectedIndex {
            get => m_ReorderableList.SelectedIndex;
            set => m_ReorderableList.SelectedIndex = value;
        }

        public T SelectedObject {
            get => SelectedIndex < 0 || SelectedIndex >= m_SearchedItemSource.Count ? default : m_SearchedItemSource[SelectedIndex];
            set => SelectObject(value);
        }

        /// <summary>
        /// SearchableList constructor.
        /// </summary>
        /// <param name="itemsSource">The list of items.</param>
        /// <param name="makeItem">The callback when each element is being created.</param>
        /// <param name="bindItem">the callback when the element is binding to the data.</param>
        /// <param name="header">The list header (can be null).</param>
        /// <param name="onSelection">The callback when an element is selected (can be null).</param>
        /// <param name="onAdd">The callback when the add button is pressed (can be null).</param>
        /// <param name="onRemove">The callback when the remove button is pressed (can be null).</param>
        /// <param name="onReorder">The callback when elements are reordered.</param>
        public SearchableList(IList<T> itemsSource, Action<VisualElement, int> makeItem, Action<VisualElement, int> bindItem, Action<VisualElement> header,
                                Action<int> onSelection, Action onAdd, Action<int> onRemove, Action<int, int> onReorder)
        {
            m_ItemSource = itemsSource;
            m_SearchedItemSource = new List<T>();
            if (m_ItemSource != null) {
                for (int i = 0; i < m_ItemSource.Count; i++) { m_SearchedItemSource.Add(m_ItemSource[i]); }
            }

            m_OnSelection = onSelection;

            m_SearchField = new ToolbarSearchField();
            m_SearchField.style.flexShrink = 1;
            m_SearchField.style.width = new StyleLength(StyleKeyword.Auto);
            m_SearchField.style.marginRight = 4;
            m_SearchField.RegisterValueChangedCallback(evt =>
            {
                Search(evt.newValue);
            });
            Add(m_SearchField);

            m_ReorderableList = new ReorderableList(m_SearchedItemSource, makeItem, bindItem, header,
                index =>
                {
                    m_OnSelection?.Invoke(index);
                    m_PreviousSelectedObject = (index < 0 || index >= m_SearchedItemSource.Count) ? default : m_SearchedItemSource[index];
                }, onAdd, onRemove, onReorder);
            Add(m_ReorderableList);
        }

        /// <summary>
        /// Clears the search and refreshes.
        /// </summary>
        public void ClearSearch()
        {
            m_SearchField.SetValueWithoutNotify((string)null);
            
            Search(null);
        }

        /// <summary>
        /// Refresh and select the object that was selected before the search sort.
        /// </summary>
        /// <param name="searchString">The search text.</param>
        private void SearchRefresh(string searchString)
        {
            var previousSelectedObject = m_PreviousSelectedObject;
            var previousSelectedIndex = m_ReorderableList.SelectedIndex;

            if (!selectOnRefresh) { m_ReorderableList.SelectedIndex = -1; }

            m_ReorderableList.Refresh(m_SearchedItemSource);

            if (selectOnRefresh) {
                // Select the object that was selected before the search change if possible.
                if (m_PreviousSearch != searchString) {
                    if (m_SearchedItemSource.Contains(previousSelectedObject)) {
                        SelectObject(previousSelectedObject);
                    } else if (m_SearchedItemSource != null && previousSelectedIndex != -1) {

                        if (m_SearchedItemSource.Count <= 0) {
                            m_ReorderableList.SelectedIndex = -1;
                            m_OnSelection?.Invoke(-1);
                        } else if (!Equals(previousSelectedObject, m_SearchedItemSource[0])) {
                            m_ReorderableList.SelectedIndex = -1;
                            m_OnSelection?.Invoke(-1);
                            m_PreviousSelectedObject = m_SearchedItemSource[0];
                            m_ReorderableList.SelectedIndex = 0;
                        }
                    }
                } else if (previousSelectedIndex >= m_SearchedItemSource.Count) {
                    m_ReorderableList.SelectedIndex = m_SearchedItemSource.Count - 1;
                }
            }

            m_PreviousSearch = searchString;
        }

        /// <summary>
        /// Searches the list.
        /// </summary>
        /// <param name="searchString">The search text.</param>
        protected void Search(string searchString)
        {
            m_SearchedItemSource.Clear();
            if (string.IsNullOrWhiteSpace(searchString)) {
                m_SearchedItemSource = new List<T>(m_ItemSource);
            } else {
                searchString = searchString.ToLowerInvariant();
                for (int i = 0; i < m_ItemSource.Count; ++i) {
                    var elementValue = m_ItemSource[i].ToString().ToLowerInvariant();
                    if (elementValue.Contains(searchString)) {
                        m_SearchedItemSource.Add(m_ItemSource[i]);
                    }
                }
            }
            SearchRefresh(searchString);
        }

        /// <summary>
        /// Refresh after assigning a new itemSource.
        /// </summary>
        /// <param name="itemSource">The item source.</param>
        public void Refresh(IList<T> itemSource)
        {
            m_ItemSource = itemSource;
            Refresh();
        }

        /// <summary>
        /// Refresh the list.
        /// </summary>
        public void Refresh()
        {
            var previousSelected = SelectedObject;
            Search(m_SearchField.value);
            SelectObject(previousSelected);
        }

        /// <summary>
        /// Select an object in the list.
        /// </summary>
        /// <param name="obj">The object to select.</param>
        public virtual void SelectObject(T obj)
        {
            if (obj == null) { return; }

            var index = -1;
            for (int i = 0; i < m_SearchedItemSource.Count; i++) {
                if (!ReferenceEquals(obj, m_SearchedItemSource[i])) { continue; }

                index = i;
                break;
            }

            m_ReorderableList.HighlightSelectedItem = true;
            if (index == -1 || (index == SelectedIndex && Equals(obj,SelectedObject))) { return; }
            m_ReorderableList.SelectedIndex = index;
        }

        /// <summary>
        /// Focuses the search field.
        /// </summary>
        public void FocusSearchField()
        {
            m_SearchField.Focus();
            m_SearchField.Q("unity-text-input").Focus();
        }
    }
    
    /// <summary>
    /// Window that contains the SearchableList.
    /// </summary>
    public abstract class SearchableListWindow<T>
    {
        public event Action OnClose;
        public event Action OnActionComplete;

        protected bool m_IsInitialized;
        protected PopupWindow m_PopupWindow;
        protected VisualElement m_PopupWindowContent;
        protected SearchableList<T> m_SearchableList;

        protected IList<(string, Action<T>)> m_Actions;
        protected T m_SelectedValue;

        public bool CloseOnActionComplete { get; set; }

        /// <summary>
        /// Three parameter constructor.
        /// </summary>
        /// <param name="actions">The actions that can be performed on the selected object.</param>
        /// <param name="closeOnActionComplete">Should the window be closed when the action is performed?</param>
        public SearchableListWindow(IList<(string, Action<T>)> actions,bool closeOnActionComplete)
        {
            m_Actions = actions;
            CloseOnActionComplete = closeOnActionComplete;
        }

        /// <summary>
        /// Three parameter constructor.
        /// </summary>
        /// <param name="action">The actions that can be performed on the selected object.</param>
        /// <param name="closeOnActionComplete">Should the window be closed when the action is performed?</param>
        public SearchableListWindow(Action<T> action, bool closeOnActionComplete) : 
                                    this(new[] { ("Select", action) }, closeOnActionComplete) { }

        /// <summary>
        /// Opens a new popup window.
        /// </summary>
        /// <param name="button">The button that opened the popup.</param>
        public void OpenPopupWindow(Button button)
        {
            var buttonPosition = button.worldBound.position;
            var buttonSize = button.worldBound.size;
            
            var size = new Vector2(250, 300);
            var position = EditorWindow.focusedWindow.position.position + buttonPosition + new Vector2(buttonSize.x - size.x, 0);
            
            OpenPopupWindow(position,size);
        }

        /// <summary>
        /// Opens a new popup window.
        /// </summary>
        public void OpenPopupWindow()
        {
            var size = new Vector2(250, 300);
            var point = GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - new Vector2(size.x/2f, 0);
            
            OpenPopupWindow(point,size);
        }
        
        /// <summary>
        /// Opens a new popup window.
        /// </summary>
        /// <param name="position">The position of the window.</param>
        /// <param name="size">The size of the window.</param>
        public void OpenPopupWindow(Vector2 position, Vector2 size)
        {
            BuildVisualElements(false);
            Refresh();

            m_PopupWindow = PopupWindow.OpenWindow(new Rect(position, size), size, m_PopupWindowContent);
            AddStyling(m_PopupWindow.rootVisualElement);
            m_SearchableList.FocusSearchField();
        }

        /// <summary>
        /// Adds the styles to the VisualElement.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected virtual void AddStyling(VisualElement container)
        {
            container.styleSheets.Add(
                Opsive.Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("e70f56fae2d84394b861a2013cb384d0")); // Shared stylesheet.
            container.styleSheets.Add(StylesForInspector.StyleSheet);
            var reorderableStateListStyleSheet =
                Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("a243b2a2fb9cc0d45a2aac464a7a3ba3");
            if (reorderableStateListStyleSheet != null) {
                container.styleSheets.Add(reorderableStateListStyleSheet); // ReorderableStateList stylesheet.
            }

            var controllerStyleSheet =
                Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("a46bc2a887de04846a522116cc71dd3b");
            if (controllerStyleSheet != null) {
                container.styleSheets.Add(controllerStyleSheet); // Controller Stylesheet
            }
        }

        /// <summary>
        /// Build the popup window content.
        /// </summary>
        public virtual void BuildVisualElements(bool force)
        {
            if (m_IsInitialized && !force) { return; }

            m_IsInitialized = true;

            m_PopupWindowContent = new VisualElement();
            m_PopupWindowContent.name = "PopupSearchableList";
            BuildVisualElements();
        }
        
        /// <summary>
        /// Adds the VisualElements to the window.
        /// </summary>
        protected virtual void BuildVisualElements()
        {
            var genericMenu = new GenericMenu();

            for (int i = 0; i < m_Actions.Count; i++) {
                var localI = i;
                genericMenu.AddItem(new GUIContent(m_Actions[localI].Item1), false, () => InvokeAction(localI));
            }
            
            m_SearchableList = new SearchableList<T>(GetDataSource(), MakeItem, BindItem, null,
                (index) =>
                {
                    if (index == -1) { return; }

                    m_SelectedValue = m_SearchableList.SelectedObject;
                    if (m_Actions.Count == 1) { 
                        InvokeAction(0); 
                    } else {
                        genericMenu.ShowAsContext();
                    }

                    m_SearchableList.SelectedIndex = -1;
                },
                null, null, null);
            m_SearchableList.selectOnRefresh = false;
            m_PopupWindowContent.Add(m_SearchableList);
        }

        /// <summary>
        /// Selects the element.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        protected virtual void InvokeAction(int index)
        {
            m_Actions[index].Item2?.Invoke(m_SelectedValue);
            m_SearchableList.SelectedIndex = -1;
            OnActionComplete?.Invoke();
            if (CloseOnActionComplete) {
                ClosePopup();
            }
        }

        /// <summary>
        /// Bind the list item.
        /// </summary>
        /// <param name="parent">The parent visual element.</param>
        /// <param name="index">The index.</param>
        protected abstract void BindItem(VisualElement parent, int index);

        /// <summary>
        /// Make the list item.
        /// </summary>
        /// <param name="parent">The parent visual element.</param>
        /// <param name="index">The index.</param>
        protected abstract void MakeItem(VisualElement parent, int index);

        /// <summary>
        /// Return the data source of the list.
        /// </summary>
        /// <returns>The list source.</returns>
        protected abstract IList<T> GetDataSource();

        /// <summary>
        /// Refresh the ObjectField.
        /// </summary>
        public void Refresh()
        {
            m_SearchableList.Refresh(GetDataSource());
        }

        /// <summary>
        /// Close the popup window.
        /// </summary>
        protected void ClosePopup()
        {
            m_PopupWindow.Close();
            OnClose?.Invoke();
        }

        /// <summary>
        /// Returns the name of the object.
        /// </summary>
        /// <param name="obj">A reference to the object.</param>
        /// <returns>The name of the specified object.</returns>
        public abstract string GetObjectName(T obj);
        
        /// <summary>
        /// Search filter for the ItemCategory list
        /// </summary>
        /// <param name="list">The list to search.</param>
        /// <param name="searchValue">The search string.</param>
        /// <param name="searchOptions">The search options.</param>
        /// <returns>A new filtered list.</returns>
        public virtual IList<T> SearchFilter(IList<T> list, string searchValue, (string prefix, Func<string, T, bool>)[] searchOptions)
        {
            var searchWords = searchValue.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            var compareInfo = CultureInfo.CurrentCulture.CompareInfo;
            var newList = new List<T>();
            for (int i = 0; i < list.Count; ++i) {
                var element = list[i];
                if (element == null) { continue; }

                var addElement = false;
                var additiveTags = false;

                // Match search tags additively.
                for (int j = 0; j < searchWords.Length; j++) {

                    var searchWord = searchWords[j];
                    addElement = false;

                    // Search by name.
                    if (searchWord.Contains(":") == false) {
                        // Case insensitive Contains(string).
                        if (compareInfo.IndexOf(GetObjectName(element), searchWord, CompareOptions.IgnoreCase) >= 0) {
                            addElement = true;
                            if (additiveTags) { break; }
                        }
                        if (addElement == false) { break; } else { continue; }
                    }

                    // Search options.
                    if (searchOptions != null) {
                        for (int k = 0; k < searchOptions.Length; k++) {
                            var searchPrefix = searchOptions[k].prefix;
                            var searchFunction = searchOptions[k].Item2;
                            if (searchWord.StartsWith(searchPrefix)) {
                                var optionSearchWord = searchWord.Remove(0, 2);
                                var optionAddElement = false;

                                optionAddElement = searchFunction(optionSearchWord, element);

                                if (optionAddElement) {
                                    addElement = true;
                                    if (additiveTags) { break; }
                                }

                                if (addElement == false) { break; } else { continue; }
                            }
                        }
                    }
                }

                if (addElement) {
                    newList.Add(list[i]);
                }
            }

            return newList;
        }
    }
    
    /// <summary>
    /// An editor popup menu.
    /// </summary>
    public class PopupWindow : EditorWindow
    {
        protected VisualElement m_Content;
        private static PopupWindow s_Instance;

        public VisualElement Content => m_Content;

        /// <summary>
        /// The popup instance.
        /// </summary>
        private static PopupWindow Instance {
            get {
                if (s_Instance == null) {
                    s_Instance = CreateInstance<PopupWindow>();
                    s_Instance.hideFlags = HideFlags.HideAndDontSave;
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// Opens a popup window.
        /// </summary>
        /// <param name="rect">The position.</param>
        /// <param name="size">The size.</param>
        /// <param name="content">The popup content.</param>
        /// <returns>The popup window.</returns>
        public static PopupWindow OpenWindow(Rect rect, Vector2 size, VisualElement content)
        {
            var window = Instance;
            window.m_Content = content;
            window.ShowAsDropDown(rect, size);
            //window.Show();

            if (rect.position.y + rect.size.y > Screen.currentResolution.height) {
                rect.position -= new Vector2(0, rect.size.y);
            }

            window.position = rect;
            window.Refresh();
            return window;
        }

        /// <summary>
        /// Redraw the content.
        /// </summary>
        public virtual void Refresh()
        {
            rootVisualElement.Clear();
            rootVisualElement.Add(m_Content);
        }
    }
}