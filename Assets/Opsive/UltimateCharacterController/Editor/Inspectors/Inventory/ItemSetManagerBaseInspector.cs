/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.StateSystem;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    using EventHandler = Opsive.Shared.Events.EventHandler;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Custom inspector for the ItemSetManagerBase component.
    /// </summary>
    [CustomEditor(typeof(ItemSetManagerBase))]
    public abstract class ItemSetManagerBaseInspector : UIElementsInspector
    {
        protected override List<string> ExcludedFields => new List<string>() { "m_ItemSetGroups" };

        private ItemSetManagerBase m_ItemSetManagerBase;
        private ItemSetGroupListVisualElement m_ItemSetGroupListVisualElement;

        public abstract Type ItemCategoryType { get; }

        /// <summary>
        /// Create a custom inspector by overriding the base one.
        /// </summary>
        /// <returns>The custom inspector.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            if (m_ItemSetManagerBase == null) {
                m_ItemSetManagerBase = target as ItemSetManagerBase;
                if (m_ItemSetManagerBase.ItemSetGroups == null) {
                    m_ItemSetManagerBase.ItemSetGroups = new ItemSetGroup[0];
                }

                if (Application.isPlaying) {
                    EventHandler.RegisterEvent<ItemSetGroup>(m_ItemSetManagerBase.gameObject, "OnItemSetGroupUpdated", HandleUpdate);
                    EventHandler.RegisterEvent<int, int, int>(m_ItemSetManagerBase.gameObject, "OnItemSetManagerUpdateNextItemSet", HandleUpdate);
                    EventHandler.RegisterEvent<int, int>(m_ItemSetManagerBase.gameObject, "OnItemSetManagerUpdateItemSet", HandleUpdate);

                }
            }

            return base.CreateInspectorGUI();
        }

        /// <summary>
        /// Handles the manager updating.
        /// </summary>
        /// <param name="groupIndex">The item set group index.</param>
        /// <param name="prevItemSetIndex">The previous active item set index.</param>
        /// <param name="itemSetIndex">The new active item set index.</param>
        private void HandleUpdate(int groupIndex, int prevItemSetIndex, int itemSetIndex)
        {
            Refresh();
        }
        
        /// <summary>
        /// Handles the manager updating.
        /// </summary>
        /// <param name="groupIndex">The item set group index.</param>
        /// <param name="newItemSetIndex">The new active item set index.</param>
        private void HandleUpdate(int groupIndex, int newItemSetIndex)
        {
            Refresh();
        }

        /// <summary>
        /// Handles the manager updating.
        /// </summary>
        /// <param name="itemSetGroup">The item set group that changed.</param>
        private void HandleUpdate(ItemSetGroup itemSetGroup)
        {
            Refresh();
        }

        /// <summary>
        /// Adds the custom UIElements to the bottom of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_ItemSetGroupListVisualElement = new ItemSetGroupListVisualElement(
                target,
                "Item Set Groups",
                () => m_ItemSetManagerBase.ItemSetGroups,
                (newValue) =>
                {
                    Undo.RegisterCompleteObjectUndo(m_ItemSetManagerBase, "Item Set Manager Change");
                    m_ItemSetManagerBase.ItemSetGroups = newValue;
                    EditorUtility.SetDirty(m_ItemSetManagerBase);
                },ItemCategoryType);

            container.Add(m_ItemSetGroupListVisualElement);

            Refresh();
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public void Refresh()
        {
            m_ItemSetGroupListVisualElement.Refresh();
        }
    }

    /// <summary>
    /// The item set group list view shows the contents of multiple Item Set Groups.
    /// </summary>
    public class ItemSetGroupListVisualElement : VisualElement
    {
        private Func<ItemSetGroup[]> m_Getter;
        private Action<ItemSetGroup[]> m_Setter;
        private List<ItemSetGroup> m_List;
        private FoldoutListView m_FoldoutListView;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">The object that the element belongs to.</param>
        /// <param name="title">The view title.</param>
        /// <param name="getter">A getter for getting the array of item set groups when refreshing the view.</param>
        /// <param name="setter">The setter for setting the item set group array when it changes.</param>
        public ItemSetGroupListVisualElement(Object target, string title, Func<ItemSetGroup[]> getter, Action<ItemSetGroup[]> setter, Type categoryType)
        {
            m_Getter = getter;
            m_Setter = setter;

            m_List = new List<ItemSetGroup>();
            m_List.AddRange(m_Getter.Invoke());
            m_FoldoutListView = new FoldoutListView(
                m_List,
                (parent, index) =>
                {
                    parent.Header.text = "new";

                    var itemSetGroupVisualElement = new ItemSetGroupVisualElement(target, categoryType);
                    itemSetGroupVisualElement.OnValueChanged += HandleElementValueChanged;

                    parent.Content.Add(itemSetGroupVisualElement);
                }, (parent, index) =>
                {
                    var itemSetGroup = m_List[index];
                    var ruleCount = Application.isPlaying ? itemSetGroup?.ItemSetRules?.Count ?? 0
                                        : (ReflectionUtility.GetNonPublicField(itemSetGroup, "m_StartingItemSetRules") as ItemSetRuleBase[])?.Length ?? 0;
                    var setCount = itemSetGroup?.ItemSetList?.Count ?? 0;

                    var isCategoryNull = itemSetGroup == null || itemSetGroup.SerializedItemCategory == null;
                    var categoryName = isCategoryNull ? string.Empty : itemSetGroup.SerializedItemCategory.name;
                    var groupTitle = !string.IsNullOrWhiteSpace(categoryName) ? $"{itemSetGroup.SerializedItemCategory.name} Group" : $"Group {index}";
                    parent.Header.text = $"{groupTitle} [Rules: {ruleCount}, Sets: {setCount}]";
                    var listElement = parent.Content.ElementAt(0) as ItemSetGroupVisualElement;
                    listElement.Refresh(itemSetGroup);
                }, (parent) =>
                {
                    var titleLabel = new Label(title);
                    titleLabel.AddToClassList("flex-grow");
                    parent.Add(titleLabel);
                },
                () =>
                {
                    m_List.Add(new ItemSetGroup());
                    m_Setter.Invoke(m_List.ToArray());
                    Refresh();
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);
                    m_Setter.Invoke(m_List.ToArray());
                    Refresh();
                }, (i1, i2) =>
                {
                    var temp = m_List[i1];
                    m_List[i1] = m_List[i2];
                    m_List[i2] = temp;

                    m_Setter.Invoke(m_List.ToArray());
                });

            Add(m_FoldoutListView);
        }

        /// <summary>
        /// Serialize and update the visuals.
        /// </summary>
        /// <param name="value">The value.</param>
        private void HandleElementValueChanged(ItemSetGroup value)
        {
            var index = m_List.IndexOf(value);
            if (index == -1) {
                Debug.LogWarning("The value that was changed is not part of the list.");
                return;
            }

            m_Setter.Invoke(m_List.ToArray());
            m_FoldoutListView.Refresh(m_List);
        }

        /// <summary>
        /// Refresh the view to show the updated data.
        /// </summary>
        public void Refresh()
        {
            m_List.Clear();
            var value = m_Getter.Invoke();
            if (value != null) {
                m_List.AddRange(value);
            }
            m_FoldoutListView.Refresh(m_List);
        }
    }

    /// <summary>
    /// The view to show the contents of an Item Set Group.
    /// </summary>
    public class ItemSetGroupVisualElement : VisualElement
    {
        public event Action<ItemSetGroup> OnValueChanged;

        private Object m_Target;
        private ItemSetGroup m_ItemSetGroup;
        private ObjectField m_ItemCategoryField;
        private EditTimeItemSetRuleListVisualElement m_EditTimeItemSetRuleListVisualElement;
        private RuntimeTimeItemSetRuleListVisualElement m_RunTimeItemSetRuleListVisualElement;
        private ItemSetListVisualElement m_ItemSetListVisualElement;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">The object that the element belongs to.</param>
        public ItemSetGroupVisualElement(Object target, Type categoryType)
        {
            m_Target = target;
            m_ItemCategoryField = new ObjectField("Item Category");
            m_ItemCategoryField.objectType = categoryType;
            m_ItemCategoryField.RegisterValueChangedCallback(evt =>
            {
                ReflectionUtility.SetNonPublicField(m_ItemSetGroup, "m_SerializedItemCategory", evt.newValue);
                OnValueChanged?.Invoke(m_ItemSetGroup);
            });
            Add(m_ItemCategoryField);

            if (Application.isPlaying && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target))) {
                m_RunTimeItemSetRuleListVisualElement = new RuntimeTimeItemSetRuleListVisualElement(
                    m_Target,
                    "Item Set Rules",
                    () =>
                    {
                        if (m_ItemSetGroup == null) {
                            return null;
                        }
                        var itemSetRules = m_ItemSetGroup.ItemSetRules;
                        return itemSetRules;
                    });
                Add(m_RunTimeItemSetRuleListVisualElement);

                m_ItemSetListVisualElement = new ItemSetListVisualElement(
                    m_Target,
                    "Item Sets",
                    () =>
                    {
                        return m_ItemSetGroup?.ItemSetList?.ToArray();
                    },
                    (newValue) =>
                    {
                        m_ItemSetGroup?.ItemSetList?.Clear();
                        m_ItemSetGroup?.ItemSetList?.AddRange(newValue);
                    });
                Add(m_ItemSetListVisualElement);
            } else {
                m_EditTimeItemSetRuleListVisualElement = new EditTimeItemSetRuleListVisualElement(
                    "Item Set Rules",
                    () =>
                    {
                        if (m_ItemSetGroup == null) {
                            return null;
                        }
                        var itemSetRules = ReflectionUtility.GetNonPublicField(m_ItemSetGroup, "m_StartingItemSetRules") as ItemSetRuleBase[];
                        return itemSetRules;
                    },
                    (newValue) =>
                    {
                        ReflectionUtility.SetNonPublicField(m_ItemSetGroup, "m_StartingItemSetRules", newValue);
                        OnValueChanged?.Invoke(m_ItemSetGroup);
                    });
                Add(m_EditTimeItemSetRuleListVisualElement);
            }
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        /// <param name="itemSetGroup">The item set group to show.</param>
        public void Refresh(ItemSetGroup itemSetGroup)
        {
            m_ItemSetGroup = itemSetGroup;
            if (itemSetGroup == null) {
                return;
            }

            m_ItemCategoryField.SetValueWithoutNotify(m_ItemSetGroup.SerializedItemCategory);

            if (m_RunTimeItemSetRuleListVisualElement != null) {
                m_RunTimeItemSetRuleListVisualElement.Refresh();
                m_ItemSetListVisualElement.Refresh();
            } else {
                m_EditTimeItemSetRuleListVisualElement.Refresh();
            }
        }
    }

    /// <summary>
    /// The Item Set Rule visual element element when showing the rules during edit time.
    /// (Some rules only appear during runtime due to not being scriptable objects).
    /// </summary>
    public class EditTimeItemSetRuleListVisualElement : VisualElement
    {
        private ReorderableList m_ReorderableList;

        private Func<ItemSetRuleBase[]> m_Getter;
        private Action<ItemSetRuleBase[]> m_Setter;
        private Foldout m_Header;

        private List<ItemSetRuleBase> m_List;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="getter">A getter to get the item set rules to show when refreshing.</param>
        /// <param name="setter">A setter to set the item set rules if they have changed.</param>
        public EditTimeItemSetRuleListVisualElement(string title, Func<ItemSetRuleBase[]> getter, Action<ItemSetRuleBase[]> setter)
        {
            m_Getter = getter;
            m_Setter = setter;

            m_Header = new Foldout();
            m_Header.text = title;
            m_Header.Q<Toggle>().style.marginLeft = 0;
            m_Header.RegisterValueChangedCallback(evt =>
            {
                if (m_ReorderableList == null) {
                    return;
                }

                var opened = evt.newValue;
                var body = m_ReorderableList.Q("body");
                var footer = m_ReorderableList.Q("footer");

                if (body != null) {
                    body.style.display = opened ? DisplayStyle.Flex : DisplayStyle.None;
                }

                if (footer != null) {
                    footer.style.display = opened ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });

            m_List = new List<ItemSetRuleBase>();
            var value = m_Getter.Invoke();
            if (value != null) {
                m_List.AddRange(value);
            }

            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var itemSetGroupVisualElement = new ItemSetRuleVisualElement();
                    itemSetGroupVisualElement.LabelMinWidth = 20;
                    itemSetGroupVisualElement.OnValueChanged += HandleElementValueChanged;
                    parent.Add(itemSetGroupVisualElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ItemSetRuleVisualElement;
                    listElement.Index = index;
                    listElement.Refresh(m_ReorderableList.ItemsSource[index] as ItemSetRuleBase);
                }, (parent) =>
                {
                    parent.Add(m_Header);
                }, null,
                () =>
                {
                    m_List.Add(null);
                    m_Setter.Invoke(m_List.ToArray());
                    Refresh();

                    m_ReorderableList.SelectedIndex = m_List.Count - 1;
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) { return; }

                    m_List.RemoveAt(index);
                    m_Setter.Invoke(m_List.ToArray());
                    Refresh();
                }, (i1, i2) =>
                {
                    var element1 = m_ReorderableList.ListItems[i1].ItemContents.ElementAt(0) as ItemSetRuleVisualElement;
                    element1.Index = i1;
                    var element2 = m_ReorderableList.ListItems[i2].ItemContents.ElementAt(0) as ItemSetRuleVisualElement;
                    element2.Index = i2;
                    m_Setter.Invoke(m_List.ToArray());
                    Refresh();
                });
            m_ReorderableList.HighlightSelectedItem = false;

            Add(m_ReorderableList);
        }

        /// <summary>
        /// Serialize and update the visuals.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        private void HandleElementValueChanged(int index, ItemSetRuleBase value)
        {
            if (index < 0 || index >= m_List.Count) {
                Debug.LogWarning($"Index is out of range {index}/{m_List.Count}.");
                return;
            }

            m_List[index] = value;

            m_Setter.Invoke(m_List.ToArray());
            m_ReorderableList.Refresh(m_List);
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public void Refresh()
        {
            m_ReorderableList.SelectedIndex = -1;

            m_List.Clear();
            var value = m_Getter.Invoke();
            if (value != null) {
                m_List.AddRange(value);
            }
            m_ReorderableList.Refresh(m_List);
        }
    }

    /// <summary>
    /// The Item Set Rule Visual Element is used when showing the rules during runtime time (some rules only appear during runtime due to not being scriptable objects).
    /// </summary>
    public class RuntimeTimeItemSetRuleListVisualElement : VisualElement
    {
        private Object m_Target;
        private Func<List<IItemSetRule>> m_Getter;
        private ReorderableList m_ReorderableList;
        private Foldout m_Header;
        private List<IItemSetRule> m_List;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="getter">A getter to show the item rules when refreshing.</param>
        public RuntimeTimeItemSetRuleListVisualElement(Object target, string title, Func<List<IItemSetRule>> getter)
        {
            m_Target = target;
            m_Getter = getter;

            m_Header = new Foldout();
            m_Header.text = title;
            m_Header.Q<Toggle>().style.marginLeft = 0;
            m_Header.RegisterValueChangedCallback(evt =>
            {
                if (m_ReorderableList == null) {
                    return;
                }

                var opened = evt.newValue;

                var body = m_ReorderableList.Q("body");
                var footer = m_ReorderableList.Q("footer");

                if (body != null) {
                    body.style.display = opened ? DisplayStyle.Flex : DisplayStyle.None;
                }

                if (footer != null) {
                    footer.style.display = opened ? DisplayStyle.Flex : DisplayStyle.None;
                }

                m_ReorderableList.SelectedIndex = -1;
            });

            m_List = new List<IItemSetRule>();
            var value = m_Getter.Invoke();
            if (value != null) {
                m_List.AddRange(value);
            }

            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var itemSetGroupVisualElement = new ListItem();

                    parent.Add(itemSetGroupVisualElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ListItem;
                    listElement.Index = index;
                    listElement.Refresh(m_ReorderableList.ItemsSource[index] as IItemSetRule);
                }, (parent) =>
                {
                    parent.Add(m_Header);
                }, null, null, null, null);

            Add(m_ReorderableList);
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public void Refresh()
        {
            m_ReorderableList.SelectedIndex = -1;

            m_List.Clear();
            var value = m_Getter.Invoke();
            if (value != null) {
                m_List.AddRange(value);
            }
            m_ReorderableList.Refresh(m_List);
        }

        /// <summary>
        /// The list items for item set rules, shows a single item set rule in the list.
        /// </summary>
        public class ListItem : VisualElement
        {
            private InspectableObjectField<Object> m_ObjectField;
            private Label m_Label;

            public int Index { get => m_ObjectField.Index; set => m_ObjectField.Index = value; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public ListItem()
            {
                m_ObjectField = new InspectableObjectField<Object>();
                m_ObjectField.LabelMinWidth = 20;
                m_Label = new Label();
            }

            /// <summary>
            /// Refreesh the view with a new item set rule.
            /// </summary>
            /// <param name="itemSetRule">The item set rule to show.</param>
            public void Refresh(IItemSetRule itemSetRule)
            {
                Clear();
                if (itemSetRule is Object obj) {
                    m_ObjectField.Refresh(obj);
                    Add(m_ObjectField);
                } else {
                    m_Label.text = Index + "   " + itemSetRule.name;
                    Add(m_Label);
                }
            }
        }
    }

    /// <summary>
    /// A simple Item Set Rule view by using an InspectableObjectField.
    /// </summary>
    public class ItemSetRuleVisualElement : InspectableObjectField<ItemSetRuleBase>
    { }

    /// <summary>
    /// The item set list, shows a list of the item sets.
    /// </summary>
    public class ItemSetListVisualElement : VisualElement
    {
        private Object m_Target;
        private Func<ItemSet[]> m_Getter;
        private Action<ItemSet[]> m_Setter;
        private ReorderableList m_ReorderableList;
        private Foldout m_Header;
        private List<ItemSet> m_List;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="getter">A getter to get the item sets to show when refreshing.</param>
        /// <param name="setter">A setter to set the item sets when there is a change.</param>
        public ItemSetListVisualElement(Object target, string title, Func<ItemSet[]> getter, Action<ItemSet[]> setter)
        {
            m_Target = target;
            m_Getter = getter;
            m_Setter = setter;

            m_Header = new Foldout();
            m_Header.text = title;
            m_Header.Q<Toggle>().style.marginLeft = 0;
            m_Header.RegisterValueChangedCallback(evt =>
            {
                if (m_ReorderableList == null) {
                    return;
                }

                var opened = evt.newValue;
                var body = m_ReorderableList.Q("body");
                var footer = m_ReorderableList.Q("footer");

                if (body != null) {
                    body.style.display = opened ? DisplayStyle.Flex : DisplayStyle.None;
                }

                if (footer != null) {
                    footer.style.display = opened ? DisplayStyle.Flex : DisplayStyle.None;
                }

                m_ReorderableList.SelectedIndex = -1;
            });

            m_List = new List<ItemSet>();
            var value = m_Getter.Invoke();
            if (value != null) {
                m_List.AddRange(value);
            }

            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var itemSetGroupVisualElement = new ItemSetVisualElement(m_Target);
                    itemSetGroupVisualElement.OnValueChanged += HandleElementValueChanged;

                    parent.Add(itemSetGroupVisualElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ItemSetVisualElement;
                    listElement.Index = index;
                    listElement.Refresh(m_ReorderableList.ItemsSource[index] as ItemSet, m_ReorderableList.SelectedIndex == index);
                }, (parent) =>
                {
                    parent.Add(m_Header);
                }, (index) =>
                {
                    if (index < 0 || index >= m_List.Count) {
                        return ReorderableList.DefaultElementHeight;
                    }
                    return ItemSetVisualElement.GetHeight(m_List[index], m_ReorderableList.SelectedIndex == index);
                },
                (index) =>
                {
                    for (int i = 0; i < m_ReorderableList.ListItems.Count; i++) {
                        var element1 = m_ReorderableList.ListItems[i].ItemContents.ElementAt(0) as ItemSetVisualElement;
                        element1.Select(i == index);
                    }
                    // Refresh to update the element size.
                    m_ReorderableList.Refresh();
                }, null, null, null);
            m_ReorderableList.HighlightSelectedItem = false;

            Add(m_ReorderableList);
        }

        /// <summary>
        /// Serialize and update the visuals.
        /// </summary>
        /// <param name="value">The value.</param>
        private void HandleElementValueChanged(ItemSet value)
        {
            var index = m_List.IndexOf(value);
            if (index == -1) {
                Debug.LogWarning("The value that was changed is not part of the list.");
                return;
            }

            m_Setter.Invoke(m_List.ToArray());
            m_ReorderableList.Refresh(m_List);
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public void Refresh()
        {
            m_ReorderableList.SelectedIndex = -1;
            m_List.Clear();
            var value = m_Getter.Invoke();
            if (value != null) {
                m_List.AddRange(value);
            }
            m_ReorderableList.Refresh(m_List);
        }
    }

    /// <summary>
    /// The visual element for an item set.
    /// </summary>
    public class ItemSetVisualElement : VisualElement
    {
        public event Action<ItemSet> OnValueChanged;

        public const float c_SlotHeight = 22;
        public const float c_HeaderHeight = 22;
        public const float c_StateHeight = 22;
        public const float c_DetailHeight = 150;

        private Object m_Target;
        private bool m_Selected;
        private IItemSetRule m_ItemSetRule;
        private ItemSet m_ItemSet;

        private VisualElement m_Container;
        private List<GameObject> m_CharacterItemList;

        private VisualElement m_Header;
        private VisualElement m_StateIcon;
        private Label m_SetLabel;

        private ReorderableList m_SlotsReorderableList;

        private VisualElement m_DetailContainer;
        private VisualElement m_StateContainer;

        public IItemSetRule ItemSetRule => m_ItemSetRule;
        public int SlotCount => m_ItemSet.ItemIdentifiers.Length;

        public string Title
        {
            get {
                if (m_ItemSet == null) {
                    return "";
                }

                var stateText = string.IsNullOrWhiteSpace(m_ItemSet.State) ? "" : $" ('{m_ItemSet.State}')";
                return stateText;
            }
        }

        public int Index { get; set; }

        /// <summary>
        /// Select or unselect the item set.
        /// </summary>
        /// <param name="select">Is the item set selected?</param>
        public void Select(bool select)
        {
            if (m_Selected == select) { return; }

            m_Selected = select;

            Refresh(m_ItemSet, m_Selected);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ItemSetVisualElement(Object target)
        {
            m_Target = target;
            m_Container = new VisualElement();
            Add(m_Container);

            m_Header = new VisualElement();
            m_Header.name = "itemSetHeader";

            m_StateIcon = new VisualElement();
            m_StateIcon.name = "itemSetHeader_StateIcon";
            m_Header.Add(m_StateIcon);

            m_SetLabel = new Label();
            m_Header.Add(m_SetLabel);

            m_CharacterItemList = new List<GameObject>();
            m_SlotsReorderableList = new ReorderableList(
                m_CharacterItemList,
                (parent, index) =>
                {
                    var itemSetGroupVisualElement = new ItemSetSlotVisualElement();
                    parent.Add(itemSetGroupVisualElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ItemSetSlotVisualElement;
                    listElement.LabelText = "Slot " + index;
                    listElement.LabelMinWidth = 40;
                    listElement.Index = index;
                    listElement.Refresh(m_CharacterItemList[index]);
                }, null, (index) =>
                {
                    // Not implemented.
                }, null, null, null);
            m_SlotsReorderableList.name = "ItemSet_SlotList";
            m_SlotsReorderableList.HighlightSelectedItem = false;

            m_DetailContainer = new VisualElement();
            m_DetailContainer.name = "ItemSet_DetailContainer";
            m_StateContainer = new VisualElement();
            m_StateContainer.AddToClassList("indent");
        }

        /// <summary>
        /// Get the height since it can depend on the content of the item set.
        /// </summary>
        /// <param name="itemSet">The item set to get the visual element height for.</param>
        /// <param name="selected">Is the element selected?</param>
        /// <returns>The height that the visual element should have.</returns>
        public static float GetHeight(ItemSet itemSet, bool selected)
        {
            if (itemSet == null) {
                return c_HeaderHeight;
            }

            if (selected) {
                return c_SlotHeight * itemSet.SlotCount
                       + c_StateHeight * itemSet.States.Length
                       + c_HeaderHeight
                       + c_DetailHeight;
            } else {
                return c_SlotHeight * itemSet.SlotCount + c_HeaderHeight;
            }
        }

        /// <summary>
        /// Update the visuals.
        /// </summary>
        /// <param name="newItemSet">The new value.</param>
        /// <param name="selected">Is the item set selected?</param>
        public virtual void Refresh(ItemSet newItemSet, bool selected)
        {
            m_Selected = selected;
            m_Container.Clear();

            m_ItemSet = newItemSet;
            m_ItemSetRule = m_ItemSet.ItemSetRule;

            if (m_ItemSet == null) {
                m_Container.Add(new Label("The Item Set is null."));
                return;
            }

            // Header.
            var stateIconAndTooltip = GetStateIconAndTooltip(m_ItemSet);
            m_Header.tooltip = stateIconAndTooltip.Tooltip;
            var selectedStyle = $"itemSetHeader-{(EditorGUIUtility.isProSkin ? "dark" : "light")}-selected";
            if (selected) {
                m_Header.AddToClassList(selectedStyle);
            } else {
                m_Header.RemoveFromClassList(selectedStyle);
            }

            m_SetLabel.text = $"Item Set {Index} [" +
                              $"{(m_ItemSet.Default ? "Default, " : "")}" +
                              $"{(string.IsNullOrEmpty(m_ItemSet.State) ? "" : (m_ItemSet.State + ", "))}" +
                              $"{m_ItemSet.ItemSetRule.name}{(m_ItemSet.ItemSetRuleIndex != -1 ? (" (Index " + m_ItemSet.ItemSetRuleIndex.ToString() + ")"): "")}]";
            m_StateIcon.style.backgroundImage = new StyleBackground(stateIconAndTooltip.Icon);
            m_StateIcon.tooltip = stateIconAndTooltip.Tooltip;

            // Slots.
            m_CharacterItemList.Clear();
            for (int i = 0; i < m_ItemSet.SlotCount; i++) {
                var characterItem = m_ItemSet.GetCharacterItem(i)?.gameObject;
                m_CharacterItemList.Add(characterItem);
            }
            m_SlotsReorderableList.Refresh(m_CharacterItemList);

            m_Container.Add(m_Header);
            m_Container.Add(m_SlotsReorderableList);

            if (!selected) {
                return;
            }

            m_DetailContainer.Clear();
            m_StateContainer.Clear();
            m_Container.Add(m_DetailContainer);

            FieldInspectorView.AddField(m_Target, m_ItemSet, "m_Enabled", m_DetailContainer, (object obj) =>
            {
                m_ItemSet.Enabled = (bool)obj;
                HandleValueChanged();
            });
            FieldInspectorView.AddField(m_Target, m_ItemSet, "m_State", m_DetailContainer, (object obj) =>
            {
                m_ItemSet.State = (string)obj;
                HandleValueChanged();
            });
            FieldInspectorView.AddField(m_Target, m_ItemSet, "m_CanSwitchTo", m_DetailContainer, (object obj) =>
            {
                m_ItemSet.CanSwitchTo = (bool)obj;
                HandleValueChanged();
            });
            FieldInspectorView.AddField(m_Target, m_ItemSet, "m_DisabledIndex", m_DetailContainer, (object obj) =>
            {
                m_ItemSet.DisabledIndex = (int)obj;
                HandleValueChanged();
            });
            FieldInspectorView.AddField(m_Target, m_ItemSet, "m_States", m_StateContainer, (object obj) =>
            {
                m_ItemSet.States = (State[])obj;
                HandleValueChanged();
            });
            m_DetailContainer.Add(m_StateContainer);
        }

        /// <summary>
        /// Returns the state icon and the state tooltip for an item set.
        /// </summary>
        /// <param name="itemSet">The item set to get the state icon and tooltip for.</param>
        /// <returns>The tooltip and the Icon for the item set state.</returns>
        public (string Tooltip, Texture2D Icon) GetStateIconAndTooltip(ItemSet itemSet)
        {
            if (itemSet.Active) {
                return ("Active", Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("defe707fac256444598e3f8a297992f7"));
            }

            if (!itemSet.IsValid) {
                return ("Invalid", Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("97fbf97b141837d42a7ba59bce21101b"));
            }

            if (itemSet.Enabled) {
                return ("Enabled", Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("4c21462617efc944b9d80c3e9f237ad6"));
            }

            return ("Disabled", Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>("479498807a425664db202c18464e8ff0"));
        }

        /// <summary>
        /// Notifgy that an itemset has changed.
        /// </summary>
        protected void HandleValueChanged()
        {
            OnValueChanged?.Invoke(m_ItemSet);
        }
    }

    /// <summary>
    /// A simple inspectable object field for GameObject.
    /// </summary>
    public class ItemSetSlotVisualElement : InspectableObjectField<GameObject>
    {

    }
}