/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// This class allows for easy generic use of a reorderable list.
    /// Extremely useful for SerializedReferenced lists as it creates a list with elements with just the name
    /// And when selected shows a container with the full serialized data below the list.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the list.</typeparam>
    public class GenericReorderableList<T> : VisualElement where T : class
    {
        protected ReorderableList m_ReorderableList;
        protected SerializedProperty m_SerializedProperty;

        protected readonly string[] m_InLineFields = new[] { "m_Delay", "m_Enabled" };
        protected Func<IList<T>> m_Get;
        protected Action<IList<T>> m_Set;

        protected Foldout m_Header;
        protected Object m_Target;
        
        protected T m_DrawnModule;

        protected List<T> m_List;
        protected VisualElement m_SelectedModuleContainer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public GenericReorderableList()
        {
            // Do nothing.
        }

        /// <summary>
        /// Setup or initialize the field.
        /// </summary>
        /// <param name="title">The title of the reorderable list.</param>
        /// <param name="target">The target Unity Object.</param>
        /// <param name="get">A getter for getting the list of objects to display.</param>
        /// <param name="set">A setter for setting the list of elements when a change occurs.</param>
        /// <param name="serializedProperty">The SerializedProperty that the object belongs to.</param>
        public virtual void Setup(string title, Object target, Func<IList<T>> get, Action<IList<T>> set, SerializedProperty serializedProperty)
        {
            m_Target = target;
            m_Get = get;
            m_Set = set;
            m_SerializedProperty = serializedProperty;

            m_Header = new Foldout();
            m_Header.text = title;
            Add(m_Header);

            m_List = new List<T>();
            var value = get?.Invoke();
            if (value != null) {
                m_List.AddRange(value);
            }

            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var itemSetGroupVisualElement = new GenericListElement(m_Target, (changedIndex, module) =>
                    {
                        if (index < 0 || index >= m_List.Count) {
                            Debug.LogWarning($"Index out of range {index}/{m_List.Count}.");
                            return;
                        }

                        m_List[changedIndex] = module;
                        InvokeValueChanged();
                    }, m_InLineFields);

                    parent.Add(itemSetGroupVisualElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as GenericListElement;
                    listElement.Index = index;
                    listElement.Refresh(m_ReorderableList.ItemsSource[index] as T);
                }, (parent) =>
                {
                    var horizontalLayout = new VisualElement();
                    horizontalLayout.AddToClassList("horizontal-layout");
                    parent.Add(horizontalLayout);

                    var label = new Label(title);
                    label.AddToClassList("flex-grow");
                    label.RegisterCallback<MouseDownEvent>(evt =>
                    {
                        m_ReorderableList.SelectedIndex = -1;
                        DrawModule(-1);
                    });
                    horizontalLayout.Add(label);

                    for (int i = 0; i < m_InLineFields.Length; i++) {
                        var fieldName = m_InLineFields[i];
                        if (typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public) == null) { continue; }

                        var additionalFieldLabel = new Label(ObjectNames.NicifyVariableName(fieldName));
                        additionalFieldLabel.name = "serialized-reference-list-right-label";
                        additionalFieldLabel.style.width = 50;
                        horizontalLayout.Add(additionalFieldLabel);
                    }

                }, (index) =>
                {
                    DrawModule(index);
                },
                () =>
                {
                    Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(m_Target, "Change Value");
                    var moduleType = typeof(T);
                    ReorderableListSerializationHelper.AddObjectType(moduleType, true, null, AddModule);
                }, (index) =>
                {
                    Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(m_Target, "Change Value");
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);

                    InvokeValueChanged();
                    Refresh();
                }, (i1, i2) =>
                {
                    Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(m_Target, "Change Value");
                    var element1 = m_ReorderableList.ListItems[i1].ItemContents.ElementAt(0) as GenericListElement;
                    element1.Index = i1;
                    var element2 = m_ReorderableList.ListItems[i2].ItemContents.ElementAt(0) as GenericListElement;
                    element2.Index = i2;

                    InvokeValueChanged();
                    Refresh();
                });

            m_Header.Add(m_ReorderableList);

            m_SelectedModuleContainer = new VisualElement();
            m_Header.Add(m_SelectedModuleContainer);
        }

        /// <summary>
        /// Draw the element at the index.
        /// </summary>
        /// <param name="index">The index of the element to draw.</param>
        private void DrawModule(int index)
        {
            if (index == -1) {
                m_DrawnModule = null;
                m_SelectedModuleContainer.Clear();
                return;
            }

            var elementList = m_Get.Invoke();
            if (elementList == null) {
                m_DrawnModule = null;
                m_SelectedModuleContainer.Clear();
                return;
            }

            if (index >= elementList.Count) {
                m_DrawnModule = null;
                m_SelectedModuleContainer.Clear();
                return;
            }

            var module = elementList[index];
            // Don't redraw if it is already drawn, because it cause fields to be unselected.
            if (m_DrawnModule == module) {
                return;
            }

            m_SelectedModuleContainer.Clear();
            m_DrawnModule = module;
            m_SerializedProperty.serializedObject.Update();

            var titleLabel = new Label(ObjectNames.NicifyVariableName(module?.ToString()));
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            m_SelectedModuleContainer.Add(titleLabel);

            FieldInspectorView.AddFields(
                m_Target,
                module, Shared.Utility.MemberVisibility.Public,
                m_SelectedModuleContainer,
                (object obj) =>
                {
                    m_List[index] = obj as T;
                    InvokeValueChanged();
                }, m_SerializedProperty.GetArrayElementAtIndex(index), null, true);
        }

        /// <summary>
        /// Add a element to the list.
        /// </summary>
        /// <param name="obj">The element to add to the list.</param>
        private void AddModule(object obj)
        {
            m_List.Add(Activator.CreateInstance(obj as Type) as T);

            InvokeValueChanged();
            m_ReorderableList.SelectedIndex = m_List.Count - 1;
            Refresh();
        }

        /// <summary>
        /// Serialize and update the visuals.
        /// </summary>
        private void InvokeValueChanged()
        {
            m_Set(m_List);

            Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            m_SerializedProperty.serializedObject.ApplyModifiedProperties();
            Refresh();
        }

        /// <summary>
        /// Refresh the field to show the updated data.
        /// </summary>
        public void Refresh()
        {
            var array = m_Get.Invoke();
            m_List.Clear();
            if (array != null) {
                m_List.AddRange(array);
            }
            m_ReorderableList.Refresh(m_List);

            DrawModule(m_ReorderableList.SelectedIndex);
        }

        /// <summary>
        /// The visual elment for the objects showed inside the list.
        /// </summary>
        public class GenericListElement : VisualElement
        {
            protected Label m_Label;
            protected Object m_Target;
            protected T m_Module;
            protected Action<int, T> m_OnChange;

            protected string[] m_InLineFields;
            protected VisualElement m_OtherFieldsContainer;

            public int Index { get; set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public GenericListElement(Object target, Action<int, T> onChange, string[] inLineFields)
            {
                m_Target = target;
                m_OnChange = onChange;
                m_InLineFields = inLineFields;

                AddToClassList("horizontal-layout");

                m_Label = new Label();
                m_Label.style.flexGrow = 1;
                Add(m_Label);

                if (m_InLineFields == null) { return; }

                m_OtherFieldsContainer = new VisualElement();
                m_OtherFieldsContainer.style.flexGrow = 0;
                m_OtherFieldsContainer.AddToClassList("horizontal-layout");
                Add(m_OtherFieldsContainer);
            }

            /// <summary>
            /// Refresh the view to show the updated data.
            /// </summary>
            /// <param name="index">The index of the element in the list.</param>
            /// <param name="module">The element at the index.</param>
            public virtual void Refresh(int index, T module)
            {
                Index = index;
                m_Module = module;
                Refresh();
            }

            /// <summary>
            /// Refresh the view to show the updated data.
            /// </summary>
            /// <param name="module">The element to display.</param>
            public virtual void Refresh(T module)
            {
                m_Module = module;
                Refresh();
            }

            /// <summary>
            /// Refresh the view to show the updated data.
            /// </summary>
            /// <param name="index">The index of the element to display.</param>
            public virtual void Refresh(int index)
            {
                Index = index;
                Refresh();
            }

            /// <summary>
            /// Show the updated data.
            /// </summary>
            public virtual void Refresh()
            {
                m_Label.text = m_Module == null ? "(null)" : ObjectNames.NicifyVariableName(m_Module.ToString());// +" | "+m_Module.GetHashCode();

                if (m_InLineFields == null) { return; }

                m_OtherFieldsContainer.Clear();

                if (m_Module == null) { return; }

                var extraFieldCount = 0;
                for (int i = 0; i < m_InLineFields.Length; i++) {
                    var fieldName = m_InLineFields[i];
                    var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo == null) { continue; }

                    var value = fieldInfo.GetValue(m_Module);
                    FieldInspectorView.AddField(m_Target, m_Module, fieldInfo, null, -1, fieldInfo.FieldType,
                        null, null, false, value,
                        m_OtherFieldsContainer, (newValue) =>
                        {
                            if (m_Target != null) {
                                Undo.RecordObject(m_Target, "Change " + ObjectNames.NicifyVariableName(fieldInfo.Name));
                            }

                            fieldInfo.SetValue(m_Module, newValue);
                            InvokeChange(newValue);
                        });
                    var fieldElement = m_OtherFieldsContainer.ElementAt(extraFieldCount);
                    fieldElement.name = "serialized-reference-list-right-label";

                    if (fieldElement is Toggle toggle) {
                        toggle.style.marginLeft = 22;
                        toggle.style.marginRight = 22;
                        toggle.style.justifyContent = Justify.Center;
                        toggle.ElementAt(0).style.justifyContent = Justify.Center;
                    } else {
                        fieldElement.style.width = 50;
                        fieldElement.style.justifyContent = Justify.Center;
                    }

                    extraFieldCount++;
                }
            }

            /// <summary>
            /// Invoke that the field was changed.
            /// </summary>
            /// <param name="obj">The object that changed.</param>
            private void InvokeChange(object obj)
            {
                m_OnChange?.Invoke(Index, m_Module);
            }
        }
    }
}