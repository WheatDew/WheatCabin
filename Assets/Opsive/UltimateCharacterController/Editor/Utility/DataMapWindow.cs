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
    using UnityEngine.UIElements;
    using UnityEditor;

    /// <summary>
    /// Window allowing inspection of the DataMap object.
    /// </summary>
    public abstract class DataMapWindow<T1, T2> : EditorWindow where T1 : UnityEngine.ScriptableObject where T2 : UIElementsInspector
    {
        /// <summary>
        /// The label of the header.
        /// </summary>
        protected abstract string HeaderLabel { get; }

        /// <summary>
        /// The default GUID of the inspected ScriptableObject.
        /// </summary>
        protected abstract string DefaultFileGUID { get; }

        /// <summary>
        /// The EditorPrefs key for the last inspected ScriptableObject.
        /// </summary>
        protected abstract string DefaultFileKey { get; }

        private VisualElement m_ContentContainer;
        private T1 m_DataMap;

        protected T1 DataMap
        {
            get {
                if (m_DataMap != null) {
                    return m_DataMap;
                }

                var guid = EditorPrefs.GetString(DefaultFileKey, DefaultFileGUID);
                m_DataMap = AssetDatabase.LoadAssetAtPath<T1>(AssetDatabase.GUIDToAssetPath(guid));
                return m_DataMap;
            }
            set {
                m_DataMap = value;
                EditorPrefs.SetString(DefaultFileKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_DataMap)));
            }
        }

        /// <summary>
        /// The window has been enabled.
        /// </summary>
        public void OnEnable()
        {
            minSize = new UnityEngine.Vector2(300, 400);

            rootVisualElement.styleSheets.Add(Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("e70f56fae2d84394b861a2013cb384d0"));
            rootVisualElement.AddToClassList("vertical-layout");

            var headerBackground = new VisualElement();
            headerBackground.AddToClassList("header");
            headerBackground.AddToClassList(EditorGUIUtility.isProSkin ? "header-dark" : "header-light");
            var label = new Label(HeaderLabel);
            label.AddToClassList("header-text");
            headerBackground.Add(label);
            rootVisualElement.Add(headerBackground);

            m_ContentContainer = new VisualElement();
            rootVisualElement.Add(m_ContentContainer);

            ShowMappingElements();
        }

        /// <summary>
        /// Shows the DataMap elements.
        /// </summary>
        private void ShowMappingElements()
        {
            m_ContentContainer.Clear();

            var nameIDField = new NestedInspectedObjectField<T1, T2>("Name Map", DataMap, "The mapping used by the inspector.",
                                                                        (newValue) => // OnChange.
                                                                        {
                                                                            m_DataMap = newValue;

                                                                            if (newValue == null) {
                                                                                EditorPrefs.SetString(DefaultFileKey, DefaultFileGUID);
                                                                            } else {
                                                                                EditorPrefs.SetString(DefaultFileKey, AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(newValue)).ToString());
                                                                            }
                                                                        });
            m_ContentContainer.Add(nameIDField);
        }
    }

    /// <summary>
    /// Creates a popup that has a search field.
    /// </summary>
    public abstract class DataMapSearchableWindow<T1, T2, T3> : SearchableListWindow<T2> where T1 : DataMap<T2> where T3 : EditorWindow
    {
        private string m_Title;
        protected T1 m_DataMap;
        protected string m_CurrentName;

        /// <summary>
        /// Five parameter constructor.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="dataMap">The data source.</param>
        /// <param name="currentName">The current name of the object.</param>
        /// <param name="actions">The actions to perform when a value is selected.</param>
        /// <param name="closeOnActionComplete">Should the window be closed when the action is performed?</param>
        public DataMapSearchableWindow(string title, T1 dataMap, string currentName, IList<(string, Action<T2>)> actions, bool closeOnActionComplete) : base(actions, closeOnActionComplete)
        {
            m_Title = title;
            m_DataMap = dataMap;
            m_CurrentName = currentName;
        }

        /// <summary>
        /// Five parameter constructor.
        /// </summary>
        /// <param name="title">The title of the window.</param>
        /// <param name="dataMap">The data source.</param>
        /// <param name="currentName">The current name of the object.</param>
        /// <param name="action">Action to perform when a value is selected.</param>
        /// <param name="closeOnActionComplete">Should the window be closed when the action is performed?</param>
        public DataMapSearchableWindow(string title, T1 dataMap, string currentName, Action<T2> action, bool closeOnActionComplete) : base(action, closeOnActionComplete)
        {
            m_Title = title;
            m_DataMap = dataMap;
            m_CurrentName = currentName;
        }

        /// <summary>
        /// Adds the VisualElements to the window.
        /// </summary>
        protected override void BuildVisualElements()
        {
            var label = new Label(m_Title);
            label.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            m_PopupWindowContent.Add(label);

            var openWindowButton = new Button();
            openWindowButton.text = "Open Editor";
            openWindowButton.clicked += () =>
            {
                ClosePopup();

                var window = UnityEngine.ScriptableObject.CreateInstance<T3>();
                window.titleContent = new UnityEngine.GUIContent(m_Title);
                window.ShowAuxWindow();
            };
            m_PopupWindowContent.Add(openWindowButton);

            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            horizontalLayout.style.flexGrow = 1;
            horizontalLayout.style.height = horizontalLayout.style.minHeight = horizontalLayout.style.maxHeight = 20;
            m_PopupWindowContent.Add(horizontalLayout);

            var addButton = new Button();
            var addField = new TextField();
            addField.value = m_CurrentName;
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
                Array.Sort(m_DataMap.AllObjects);
                m_SearchableList.Refresh(m_DataMap.AllObjects);
                addField.SetValueWithoutNotify(string.Empty);
            };
            horizontalLayout.Add(addButton);

            base.BuildVisualElements();
        }

        /// <summary>
        /// Callback when a new list element is added.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <param name="index">The index of the element.</param>
        protected override void MakeItem(VisualElement parent, int index)
        {
            parent.Add(new Label());
        }

        /// <summary>
        /// Binds the list element to the data.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <param name="index">The index of the element.</param>
        protected override void BindItem(VisualElement parent, int index)
        {
            if (index < 0 || index >= m_SearchableList.ItemList.Count) {
                return;
            }
            var listLabel = parent.ElementAt(0) as Label;
            var element = m_SearchableList.ItemList[index];
            listLabel.text = GetObjectName(element);
        }

        /// <summary>
        /// Returns the data source of the list.
        /// </summary>
        /// <returns>The data source of the list.</returns>
        protected override IList<T2> GetDataSource()
        {
            var allObjects = m_DataMap.AllObjects;
            Array.Sort(allObjects);
            return allObjects;
        }

        /// <summary>
        /// Returns the name of the specified object.
        /// </summary>
        /// <param name="obj">The interested object.</param>
        /// <returns>The name of the specified object.</returns>
        public override string GetObjectName(T2 obj)
        {
            return m_DataMap.GetStringValue(obj, false);
        }
    }
}