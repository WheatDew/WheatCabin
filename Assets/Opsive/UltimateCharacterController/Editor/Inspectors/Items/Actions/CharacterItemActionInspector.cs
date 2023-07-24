/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Items.Actions
{
    using Opsive.Shared.Editor.Inspectors;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    [CustomEditor(typeof(CharacterItemAction), true)]
    public class CharacterItemActionInspector : UIStateBehaviorInspector
    {
        protected CharacterItemAction m_CharacterItemAction;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_CharacterItemAction = target as CharacterItemAction;
            base.InitializeInspector();
        }

        /// <summary>
        /// Method after the UI elements have been shown.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowElementsEnd(VisualElement container)
        {
            base.ShowElementsEnd(container);

            Refresh();
        }

        /// <summary>
        /// Refresh The  inspector.
        /// </summary>
        public virtual void Refresh()
        {
        }
    }

    /// <summary>
    /// A visual element used to show a character item action module group as a reorderable list.
    /// </summary>
    public class CharacterItemActionModuleGroupField : VisualElement
    {
        public event Action<ActionModuleGroupBase> OnValueChange;
        protected ReorderableList m_ReorderableList;

        private ActionModuleGroupBase m_ItemActionModuleGroupBase;
        private SerializedProperty m_SerializedProperty;

        protected readonly string[] m_InLineFields = new[] { "m_Delay", "m_Enabled", "m_IsEnabled" };
        protected Object m_Target;

        protected List<ActionModule> m_List;
        protected VisualElement m_SelectedModuleContainer;

        protected ActionModule m_DrawnModule;
        protected GameObject m_TargetGameObject;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="title">The title/label of the field.</param>
        /// <param name="icon">The icon to display next to the title.</param>
        /// <param name="target">The target Unity Object.</param>
        /// <param name="actionModuleGroupBase">The action module group to assign to the field.</param>
        /// <param name="serializedProperty">The SerializedProperty that belongs to the group..</param>
        public CharacterItemActionModuleGroupField(string title, Texture2D icon, Object target, ActionModuleGroupBase actionModuleGroupBase, SerializedProperty serializedProperty)
        {
            m_Target = target;
            var componentTarget = target as Component;
            m_TargetGameObject = componentTarget == null ? target as GameObject : componentTarget.gameObject;
            m_ItemActionModuleGroupBase = actionModuleGroupBase;
            m_SerializedProperty = serializedProperty.FindPropertyRelative("m_Modules");

            var headerFoldout = new Foldout();
            headerFoldout.text = title;
            headerFoldout.contentContainer.AddToClassList("contained-list");
            Add(headerFoldout);

            if (icon != null) {
                var headerIcon = new VisualElement();
                headerIcon.AddToClassList("foldout-icon");
                headerIcon.style.backgroundImage = new StyleBackground(icon);
                // Position the icon to the left of the foldout text.
                var headerLabel = headerFoldout.Q<Label>();
                headerLabel.parent.Insert(1, headerIcon);
            }

            m_List = new List<ActionModule>();
            var value = m_ItemActionModuleGroupBase.BaseModules;
            if (value != null) {
                m_List.AddRange(value);
            }

            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var itemSetGroupVisualElement = new CharacterItemActionModuleListElement(
                        actionModuleGroupBase.GetModuleType(),
                        m_Target,
                        (changedIndex, module) =>
                        {
                            if (index < 0 || index >= m_List.Count) {
                                Debug.LogWarning($"Index out of range {index}/{m_List.Count}");
                                return;
                            }

                            m_List[changedIndex] = module;
                            m_ItemActionModuleGroupBase.SetModuleAsBase(index, module);
                            InvokeValueChanged();
                        },
                        m_InLineFields);

                    parent.Add(itemSetGroupVisualElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as CharacterItemActionModuleListElement;
                    listElement.Index = index;
                    listElement.Refresh(m_ReorderableList.ItemsSource[index] as ActionModule);
                }, (parent) =>
                {
                    var horizontalLayout = new VisualElement();
                    horizontalLayout.AddToClassList("horizontal-layout");
                    parent.Add(horizontalLayout);

                    var label = new Label("Modules");
                    label.AddToClassList("flex-grow");
                    label.RegisterCallback<MouseDownEvent>(evt =>
                    {
                        m_ReorderableList.SelectedIndex = -1;
                        DrawModule(-1);
                    });
                    horizontalLayout.Add(label);

                    for (int i = 0; i < m_InLineFields.Length; i++) {
                        var fieldName = m_InLineFields[i];
                        if (actionModuleGroupBase?.GetModuleType()?.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public) == null) { continue; }

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
                    var moduleType = actionModuleGroupBase.GetModuleType();
                    ReorderableListSerializationHelper.AddObjectType(moduleType, true, null, AddModule);
                }, (index) =>
                {
                    Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(m_Target, "Change Value");
                    if (index < 0 || index >= m_List.Count) { return; }

                    // Call on module removed.
                    // The list is a copy of the array, so the module must also be removed from the list.
                    m_ItemActionModuleGroupBase.RemoveModuleAt(index, m_TargetGameObject);
                    for (int i = index; i < m_ItemActionModuleGroupBase.ModuleCount; ++i) {
                        m_ItemActionModuleGroupBase.BaseModules[i].ID -= 1;
                    }
                    m_List.RemoveAt(index);

                    InvokeValueChanged();
                }, (i1, i2) =>
                {
                    Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(m_Target, "Change Value");
                    var element1 = m_ReorderableList.ListItems[i1].ItemContents.ElementAt(0) as CharacterItemActionModuleListElement;
                    element1.Index = i1;
                    var element2 = m_ReorderableList.ListItems[i2].ItemContents.ElementAt(0) as CharacterItemActionModuleListElement;
                    element2.Index = i2;

                    InvokeValueChanged();
                });

            headerFoldout.Add(m_ReorderableList);

            m_SelectedModuleContainer = new VisualElement();
            headerFoldout.Add(m_SelectedModuleContainer);
        }

        /// <summary>
        /// Draw the module.
        /// </summary>
        /// <param name="index">The index of the module within the list.</param>
        private void DrawModule(int index)
        {
            if (index <= -1 || index >= m_ItemActionModuleGroupBase.ModuleCount) {
                m_SelectedModuleContainer.Clear();
                m_DrawnModule = null;
                return;
            }

            var module = m_ItemActionModuleGroupBase.GetBaseModuleAt(index);
            // Don't redraw if it is already drawn, because it cause fields to be unselected.
            if (m_DrawnModule == module) {
                return;
            }

            m_SelectedModuleContainer.Clear();
            m_DrawnModule = module;
            m_SerializedProperty.serializedObject.Update();

            var label = new Label(ObjectNames.NicifyVariableName(module.ToString()));
            label.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            m_SelectedModuleContainer.Add(label);
            
            FieldInspectorView.AddFields(m_Target,module, Shared.Utility.MemberVisibility.Public, m_SelectedModuleContainer,
                (object obj) =>
                {
                    m_List[index] = obj as ActionModule;
                    m_ItemActionModuleGroupBase.SetModuleAsBase(index, obj as ActionModule);
                    InvokeValueChanged();
                }, m_SerializedProperty.GetArrayElementAtIndex(index), null, true);
        }

        /// <summary>
        /// Add a new module of any type.
        /// </summary>
        /// <param name="obj">The module to add.</param>
        private void AddModule(object obj)
        {
            var moduleType = obj as Type;
            var module = Activator.CreateInstance(moduleType) as ActionModule;

            // OnModuleAdded will be called allowing the module to set things up.
            m_List.Add(module);

            // Invoke change before OnModuleAdded, because it checks that the module really is in the list.
            InvokeValueChanged(false);

            m_ItemActionModuleGroupBase.OnModuleAdded(module, m_TargetGameObject);
            module.ID = m_ItemActionModuleGroupBase.ModuleCount - 1;

            // Invoke value changed again in case the OnModuleAdded function made some changes.
            InvokeValueChanged(true);

            m_ReorderableList.SelectedIndex = m_List.Count - 1;
            Refresh();
        }

        /// <summary>
        /// Serialize and update the visuals.
        /// </summary>
        /// <param name="refresh">Refresh the editor?</param>
        private void InvokeValueChanged(bool refresh = true)
        {
            // Do not set modules while in play mode, this causes issues with the state system and more.
            if (Application.isPlaying == false) {
                m_ItemActionModuleGroupBase.SetModulesAsBase(m_List);
            }

            OnValueChange?.Invoke(m_ItemActionModuleGroupBase);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            m_SerializedProperty.serializedObject.ApplyModifiedProperties();

            if (refresh) {
                Refresh();
            }
        }

        /// <summary>
        /// Refresh the field to show the updated data.
        /// </summary>
        public void Refresh()
        {
            var array = m_ItemActionModuleGroupBase.BaseModules;
            m_List.Clear();
            if (array != null) {
                m_List.AddRange(array);
            }
            m_ReorderableList.Refresh(m_List);

            DrawModule(m_ReorderableList.SelectedIndex);
        }
        
        /// <summary>
        /// Refresh the field to show the updated data.
        /// </summary>
        public void Refresh(ActionModuleGroupBase group)
        {
            m_ItemActionModuleGroupBase = group;
            Refresh();
        }

        /// <summary>
        /// A elment within the list view.
        /// </summary>
        public class CharacterItemActionModuleListElement : VisualElement
        {
            protected Action<int, ActionModule> m_OnChange;

            protected Object m_Target;
            protected Type m_BaseModuleType;

            protected VisualElement m_StateIcon;
            protected Label m_Label;
            protected ActionModule m_Module;

            protected string[] m_InLineFields;
            protected VisualElement m_OtherFieldsContainer;

            public int Index { get; set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public CharacterItemActionModuleListElement(Type baseModuleType, Object target, Action<int, ActionModule> onChange, string[] inLineFields)
            {
                m_BaseModuleType = baseModuleType;
                m_Target = target;
                m_OnChange = onChange;
                m_InLineFields = inLineFields;

                AddToClassList("horizontal-layout");

                if (Application.isPlaying) {
                    m_StateIcon = new VisualElement();
                    m_StateIcon.name = "CharacterItemActionModule_StateIcon";
                    Add(m_StateIcon);
                }

                m_Label = new Label();
                Add(m_Label);

                if (m_InLineFields == null) { return; }

                m_OtherFieldsContainer = new VisualElement();
                m_OtherFieldsContainer.style.flexGrow = 0;
                m_OtherFieldsContainer.AddToClassList("horizontal-layout");
                Add(m_OtherFieldsContainer);
            }

            /// <summary>
            /// Refresh the view.
            /// </summary>
            /// <param name="index">THe index within the list.</param>
            /// <param name="module">The module to show.</param>
            public virtual void Refresh(int index, ActionModule module)
            {
                Index = index;
                m_Module = module;
                Refresh();
            }

            /// <summary>
            /// Refresh the module.
            /// </summary>
            /// <param name="module">The new module to show.</param>
            public virtual void Refresh(ActionModule module)
            {
                m_Module = module;
                Refresh();
            }

            /// <summary>
            /// Refresh and change the index.
            /// </summary>
            /// <param name="index">The new index of the view.</param>
            public virtual void Refresh(int index)
            {
                Index = index;
                Refresh();
            }

            /// <summary>
            /// Refresh the view showing the updated data.
            /// </summary>
            public virtual void Refresh()
            {
                if (m_StateIcon != null) {
                    var stateIconAndTooltip = GetStateIconAndTooltip(m_Module);
                    m_StateIcon.style.backgroundImage = new StyleBackground(stateIconAndTooltip.Icon);
                    m_StateIcon.tooltip = stateIconAndTooltip.Tooltip;
                }

                m_Label.text = m_Module == null ? "(null)" : ObjectNames.NicifyVariableName(m_Module.ToString());

                if (m_InLineFields == null) { return; }

                m_OtherFieldsContainer.Clear();

                if (m_Module == null) { return; }

                var extraFieldCount = 0;
                for (int i = 0; i < m_InLineFields.Length; i++) {
                    var fieldName = m_InLineFields[i];
                    var fieldInfo = m_BaseModuleType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo == null) { continue; }

                    var value = fieldInfo.GetValue(m_Module);
                    FieldInspectorView.AddField(m_Target, m_Module, fieldInfo, null, -1, fieldInfo.FieldType,
                        null, null, false, value,
                        m_OtherFieldsContainer, (newValue) =>
                        {
                            if (m_Target != null) {
                                Undo.RecordObject(m_Target,
                                    "Change " + ObjectNames.NicifyVariableName(fieldInfo.Name));
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

            /// <summary>
            /// Get the state icon and the tooltip for the module.
            /// </summary>
            /// <param name="module">The module to get the state icon and tooltipe from.</param>
            /// <returns>The tooltip and icon associated with the state of the module.</returns>
            public (string Tooltip, Texture2D Icon) GetStateIconAndTooltip(ActionModule module)
            {
                if (module != null) {
                    if (module.IsActive && module.Enabled) {
                        return ("Active and Enabled", Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("defe707fac256444598e3f8a297992f7"));
                    }

                    if (module.Enabled && !module.IsActive) {
                        return ("Enabled but Inactive", Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("4c21462617efc944b9d80c3e9f237ad6"));
                    }

                    if (module.IsActive && !module.Enabled) {
                        return ("Active but Disabled", Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("97fbf97b141837d42a7ba59bce21101b"));
                    }
                }

                return ("Disabled && Inactive", Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("479498807a425664db202c18464e8ff0"));
            }
        }
    }
}