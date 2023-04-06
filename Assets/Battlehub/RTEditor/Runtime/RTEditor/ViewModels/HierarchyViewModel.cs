using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTHandles;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Binding;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class HierarchyViewModel : HierarchicalDataViewModel<ExposeToEditor>
    {
        #region ExposeToEditorViewModel
        /// <summary>
        /// This class is never instantiated. 
        /// It is used in the Template to specify the binding properties of ExposeToEditor without modifying the ExposeToEditor itself.
        /// </summary>
        [Binding]
        internal class ExposeToEditorViewModel
        {
            [Binding]
            public bool ActiveInHierarchy
            {
                get;
                set;
            }

            [Binding]
            public bool ActiveSelf
            {
                get;
                set;
            }

            [Binding]
            public string Name
            {
                get;
                set;
            }

            private ExposeToEditorViewModel() { Debug.Assert(false); }
        }
        #endregion

        protected GameObject[] SelectedGameObjects
        {
            get
            {
                if (SelectedItems == null)
                {
                    return new GameObject[0];
                }

                return SelectedItems.Select(item => item.gameObject).ToArray();
            }
        }

        protected bool IsFilterTextEmpty
        {
            get { return string.IsNullOrWhiteSpace(m_filterText); }
        }

        private bool m_forceUseCache;
        private string m_filterText;
        [Binding]
        public string FilterText
        {
            get { return m_filterText; }
            set
            {
                if (m_filterText != value)
                {
                    m_filterText = value;
                    m_forceUseCache = true;
                    RaisePropertyChanged(nameof(FilterText));
                    BindData();
                }
            }
        }

        protected virtual bool Filter(ExposeToEditor go)
        {
            return go.name.ToLower().Contains(FilterText.ToLower());
        }

         private IRuntimeSelectionComponent m_selectionComponent;
        protected IRuntimeSelectionComponent SelectionComponent
        {
            get { return m_selectionComponent; }
        }

        private IProjectAsync m_project;
        protected IProjectAsync Project
        {
            get { return m_project; }
        }

        private IPlacementModel m_placementModel;
        private IGroupingModel m_groupingModel;
     
        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_project = IOC.Resolve<IProjectAsync>();
            m_placementModel = IOC.Resolve<IPlacementModel>();
            m_groupingModel = IOC.Resolve<IGroupingModel>();
            
            Enable();

            Editor.SceneLoading += OnSceneLoading;
            Editor.SceneLoaded += OnSceneLoaded;
        }

        protected override void Start()
        {
            base.Start();
            EditorSelectionToSelectedObjects();
        }

        protected override void OnDisable()
        {
            Editor.SceneLoading -= OnSceneLoading;
            Editor.SceneLoaded -= OnSceneLoaded;

            Disable();

            m_project = null;
            m_placementModel = null;
            m_groupingModel = null;

            base.OnDisable();
        }

        protected virtual void LateUpdate()
        {
            m_rootGameObjects = null;
        }

        protected virtual void Enable()
        {
            BindData();

            EditorSelectionToSelectedObjects();

            Editor.PlaymodeStateChanged += OnPlaymodeStateChanged;

            Selection.SelectionChanged += OnEditorSelectionChanged;

            Editor.Object.Awaked += OnObjectAwaked;
            Editor.Object.Enabled += OnObjectEnabled;
            Editor.Object.Disabled += OnObjectDisabled;
            Editor.Object.Destroying += OnObjectDestroying;
            Editor.Object.MarkAsDestroyedChanging += OnObjectMarkAsDestroyedChanged;
            Editor.Object.ParentChanged += OnObjectParentChanged;
            Editor.Object.NameChanged += OnObjectNameChanged;
        }

        protected virtual void Disable()
        {
            if (Selection != null)
            {
                Selection.SelectionChanged -= OnEditorSelectionChanged;
            }

            Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;

            if (Editor.Object != null)
            {
                Editor.Object.Awaked -= OnObjectAwaked;
                Editor.Object.Enabled -= OnObjectEnabled;
                Editor.Object.Disabled -= OnObjectDisabled;
                Editor.Object.Destroying -= OnObjectDestroying;
                Editor.Object.MarkAsDestroyedChanged -= OnObjectMarkAsDestroyedChanged;
                Editor.Object.ParentChanged -= OnObjectParentChanged;
                Editor.Object.NameChanged -= OnObjectNameChanged;
            }

        }

        #region Editor EventHandlers
        protected virtual void OnSceneLoading()
        {
            Disable();
        }

        protected virtual void OnSceneLoaded()
        {
            Enable();
        }

        protected virtual void OnEditorSelectionChanged(UnityObject[] unselectedObjects)
        {
            EditorSelectionToSelectedObjects();
        }

        protected virtual void OnPlaymodeStateChanged()
        {
            BindData();
        }

        protected virtual void OnObjectAwaked(ExposeToEditor obj)
        {
            if(!obj.MarkAsDestroyed)
            {
                if (IsFilterTextEmpty)
                {
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(obj.GetParent(), obj));
                }
                else
                {
                    if(Filter(obj))
                    {
                        RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(null, obj));
                    }                    
                }
            }
        }

        protected virtual void OnObjectEnabled(ExposeToEditor obj)
        {
            obj.RaisePropertyChanged(nameof(ExposeToEditor.ActiveSelf));
            obj.RaisePropertyChanged(nameof(ExposeToEditor.ActiveInHierarchy));
        }

        protected virtual void OnObjectDisabled(ExposeToEditor obj)
        {
            obj.RaisePropertyChanged(nameof(ExposeToEditor.ActiveSelf));
            obj.RaisePropertyChanged(nameof(ExposeToEditor.ActiveInHierarchy));
        }

        protected virtual void OnObjectDestroying(ExposeToEditor obj)
        {
            if (IsFilterTextEmpty)
            {
                ExposeToEditor parent = obj.GetParent();
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemRemoved(parent, obj));
            }
            else
            {
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemRemoved(null, obj));
            }
        }

        protected virtual void OnObjectMarkAsDestroyedChanged(ExposeToEditor obj)
        {
            if (obj.MarkAsDestroyed)
            {
                ExposeToEditor parent = obj.GetParent();
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemRemoved(parent, obj));
            }
            else
            {
                if (IsFilterTextEmpty)
                {
                    ExposeToEditor parent = obj.GetParent();
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(parent, obj));
                    SetSiblingIndex(obj);
                }
                else
                {
                    if (Filter(obj))
                    {
                        AddSortedByName(obj);
                    }
                }
            }
        }

        protected virtual void OnObjectParentChanged(ExposeToEditor obj, ExposeToEditor oldValue, ExposeToEditor newValue)
        {
            if (Editor.IsPlaymodeStateChanging)
            {
                return;
            }

            if (!IsFilterTextEmpty)
            {
                return;
            }

            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ParentChanged(oldValue, obj));
        }

        protected virtual void OnObjectNameChanged(ExposeToEditor obj)
        {
            if(IsFilterTextEmpty)
            {
                return;
            }

            if (Filter(obj))
            {
                AddSortedByName(obj);
                SelectedItems = Selection.gameObjects.Select(go => go.GetComponent<ExposeToEditor>()).Where(exposed => exposed != null);
            }
            else
            {
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ParentChanged(null, obj));
            }
        }

        #endregion

        #region IHierarchicalData
        public override ExposeToEditor GetParent(ExposeToEditor item)
        {
            return item.GetParent();
        }

        public override bool HasChildren(ExposeToEditor parent)
        {
            if(!IsFilterTextEmpty)
            {
                return false;
            }

            if (parent == null)
            {
                return Editor.Object.Get(true).Any(obj => !obj.MarkAsDestroyed);
            }

            return parent.HasChildren();
        }
        public override IEnumerable<ExposeToEditor> GetChildren(ExposeToEditor parent)
        {
            if (parent == null)
            {
                bool useCache = Editor.IsPlaying || m_forceUseCache;
                m_forceUseCache = false;

                IEnumerable<ExposeToEditor> objects = Editor.Object.Get(IsFilterTextEmpty, useCache);
                if (IsFilterTextEmpty)
                {
                    if (objects.Any())
                    {
                        Transform commonParent = objects.First().transform.parent;
                        foreach (ExposeToEditor obj in objects)
                        {
                            if (obj.transform.parent != commonParent)
                            {
                                Debug.LogWarning("ExposeToEditor objects have different parents, hierarchy may not work correctly.");
                                break;
                            }
                        }
                    }
                    return objects.OrderBy(g => g.transform.GetSiblingIndex());
                }

                return objects.Where(Filter).OrderBy(g => g.name);

            }

            return parent.GetChildren();
        }

        public override int IndexOf(ExposeToEditor parent, ExposeToEditor item)
        {
            if (parent == null)
            {
                return Editor.Object.Get(true).Where(obj => !obj.MarkAsDestroyed).TakeWhile(x => x != item).Count();
            }

            return parent.GetChildren().IndexOf(item);
        }

        public override void Add(ExposeToEditor parent, ExposeToEditor item)
        {
            item.MarkAsDestroyed = false;
            item.transform.SetParent(parent != null ? parent.transform : null, true);
        }

        public override void Insert(ExposeToEditor parent, ExposeToEditor item, int index)
        {
            item.MarkAsDestroyed = false;
            item.transform.SetParent(parent != null ? parent.transform : null, true);
            item.transform.SetSiblingIndex(index);
        }

        public override void Remove(ExposeToEditor parent, ExposeToEditor item)
        {
            item.MarkAsDestroyed = true;
        }

        public override HierarchicalDataFlags GetFlags()
        {
            HierarchicalDataFlags flags = HierarchicalDataFlags.Default;
            return flags;
        }

        public override HierarchicalDataItemFlags GetItemFlags(ExposeToEditor item)
        {
            HierarchicalDataItemFlags flags = HierarchicalDataItemFlags.Default;

            if(!item.CanDelete)
            {
                flags &= ~HierarchicalDataItemFlags.CanRemove;
            }
            
            if(!item.CanRename || !IsFilterTextEmpty)
            {
                flags &= ~HierarchicalDataItemFlags.CanEdit;
            }

            if(!IsFilterTextEmpty)
            {
                flags &= ~HierarchicalDataItemFlags.CanDrag;
            }

            return flags;
        }

  
        #endregion

        #region Bound UnityEvent Handlers
        public override void OnSelectAll()
        {
            SelectedItems = GetExpandedItems();
        }
    
        public override void OnDelete()
        {
            Editor.Delete(Editor.Selection.gameObjects);
        }

        public override void OnDuplicate()
        {
            Editor.Duplicate(Editor.Selection.gameObjects);
        }

        public override async void OnExternalObjectDrop()
        {
            if (!AllowDropExternalObjects())
            {
                CanDropExternalObjects = false;
                return;
            }

            ProjectItem[] loadAssetItems = ExternalDragObjects.Where(o => o is ProjectItem && m_project.Utils.ToType((ProjectItem)o) == typeof(GameObject)).Select(o => (ProjectItem)o).ToArray();
            if (loadAssetItems.Length > 0)
            {
                //m_isSpawningPrefab = true;
                Editor.IsBusy = true;
                UnityObject[] objects;
                try
                {
                    objects = await m_project.Safe.LoadAsync(loadAssetItems);
                }
                catch (Exception e)
                {
                    IWindowManager wm = IOC.Resolve<IWindowManager>();
                    wm.MessageBox(Localization.GetString("ID_RTEditor_HierarchyView_UnableToLoadAssetItems", "Unable to load asset items"), e.Message);
                    Debug.LogException(e);
                    return;
                }
                finally
                {
                    Editor.IsBusy = false;
                }

                OnProjectItemsLoaded(objects, TargetItem);
            }
            else
            {
                CanDropExternalObjects = false;
            }
        }

        public override void OnItemsBeginDrop()
        {
            base.OnItemsBeginDrop();

            Undo.BeginRecord();
            Undo.CreateRecord(null, null, false,
                record => RefreshTree(record, true),
                record => RefreshTree(record, false));

            IEnumerable<ExposeToEditor> dragItems = SourceItems.OfType<ExposeToEditor>();

            if (LastDragDropAction == DragDropAction.SetLastChild || dragItems.Any(d => (object)d.GetParent() != TargetItem))
            {
                foreach (ExposeToEditor exposed in dragItems.Reverse())
                {
                    Transform dragT = exposed.transform;
                    int siblingIndex = dragT.GetSiblingIndex();
                    Undo.BeginRecordTransform(dragT, dragT.parent, siblingIndex);
                }
            }
            else
            {
                Transform dropT = TargetItem.transform;
                int dropTIndex = dropT.GetSiblingIndex();

                foreach (ExposeToEditor exposed in dragItems
                    .Where(o => o.transform.GetSiblingIndex() > dropTIndex)
                    .OrderBy(o => o.transform.GetSiblingIndex())
                    .Union(dragItems
                        .Where(o => o.transform.GetSiblingIndex() < dropTIndex)
                        .OrderByDescending(o => o.transform.GetSiblingIndex())))
                {
                    Transform dragT = exposed.transform;
                    int siblingIndex = dragT.GetSiblingIndex();
                    Undo.BeginRecordTransform(dragT, dragT.parent, siblingIndex);
                }
            }

            Undo.EndRecord();
        }

        public override void OnItemsDrop()
        {
            base.OnItemsDrop();

            Transform dropT = TargetItem.transform;
            if (LastDragDropAction == DragDropAction.SetLastChild)
            {
                Undo.BeginRecord();
                foreach(ExposeToEditor dragObject in SourceItems)
                {
                    Transform dragT = dragObject.transform;
                    dragT.SetParent(dropT, true);
                    dragT.SetAsLastSibling();

                    Undo.EndRecordTransform(dragT, dropT, dragT.GetSiblingIndex());
                }
                Undo.CreateRecord(null, null, true,
                   record => RefreshTree(record, true),
                   record => RefreshTree(record, false));
                Undo.EndRecord();
            }
            else if (LastDragDropAction == DragDropAction.SetNextSibling)
            {
                Undo.BeginRecord();

                ExposeToEditor[] dragObjects = SourceItems.ToArray();
                for (int i = dragObjects.Length - 1; i >= 0; --i)
                {
                    ExposeToEditor dragObject = dragObjects[i];
                    Transform dragT = dragObject.transform;

                    int dropTIndex = dropT.GetSiblingIndex();
                    if (dragT.parent != dropT.parent)
                    {
                        dragT.SetParent(dropT.parent, true);
                        dragT.SetSiblingIndex(dropTIndex + 1);
                    }
                    else
                    {
                        int dragTIndex = dragT.GetSiblingIndex();
                        if (dropTIndex < dragTIndex)
                        {
                            dragT.SetSiblingIndex(dropTIndex + 1);
                        }
                        else
                        {
                            dragT.SetSiblingIndex(dropTIndex);
                        }
                    }
                    Undo.EndRecordTransform(dragT, dragT.parent, dragT.GetSiblingIndex());
                }
                Undo.CreateRecord(null, null, true,
                    record => RefreshTree(record, true),
                    record => RefreshTree(record, false));
                Undo.EndRecord();

            }
            else if (LastDragDropAction == DragDropAction.SetPrevSilbling)
            {
                Undo.BeginRecord();
                foreach (ExposeToEditor dragObject in SourceItems)
                { 
                    Transform dragT = dragObject.transform;
                    if (dragT.parent != dropT.parent)
                    {
                        dragT.SetParent(dropT.parent, true);
                    }

                    int dropTIndex = dropT.GetSiblingIndex();
                    int dragTIndex = dragT.GetSiblingIndex();
                    if (dropTIndex > dragTIndex)
                    {
                        dragT.SetSiblingIndex(dropTIndex - 1);
                    }
                    else
                    {
                        dragT.SetSiblingIndex(dropTIndex);
                    }

                    Undo.EndRecordTransform(dragT, dragT.parent, dragT.GetSiblingIndex());
                }
                Undo.CreateRecord(null, null, true,
                    record => RefreshTree(record, true),
                    record => RefreshTree(record, false));
                Undo.EndRecord();
            }
        }

        public override void OnItemHold()
        {
            Selection.activeObject = TargetItem.gameObject;
            OpenContextMenu();
        }

        public override void OnItemBeginEdit()
        {
            base.OnItemBeginEdit();
            Undo.BeginRecordValue(Selection.activeGameObject.GetComponent<ExposeToEditor>(), Strong.MemberInfo((ExposeToEditor x) => x.Name));
        }

        public override void OnItemEndEdit()
        {
            base.OnItemEndEdit();
            Undo.EndRecordValue(Selection.activeGameObject.GetComponent<ExposeToEditor>(), Strong.MemberInfo((ExposeToEditor x) => x.Name));
        }

        #endregion

        #region Context Menu
        protected override void OnContextMenu(List<MenuItemViewModel> menuItems)
        {
            MenuItemViewModel createPrefab = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_CreatePrefab", "Create Prefab") };
            createPrefab.Action = CreatePrefabContextMenuCmd;
            createPrefab.Validate = CreatePrefabValidateContextMenuCmd;
            menuItems.Add(createPrefab);

            MenuItemViewModel duplicate = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewImpl_Duplicate", "Duplicate") };
            duplicate.Action = DuplicateContextMenuCmd;
            duplicate.Validate = DuplicateValidateContextMenuCmd;
            menuItems.Add(duplicate);

            MenuItemViewModel delete = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewImpl_Delete", "Delete") };
            delete.Action = DeleteContextMenuCmd;
            delete.Validate = DeleteValidateContextMenuCmd;
            menuItems.Add(delete);

            MenuItemViewModel rename = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewImpl_Rename", "Rename") };
            rename.Action = RenameContextMenuCmd;
            rename.Validate = RenameValidateContextMenuCmd;
            menuItems.Add(rename);

            MenuItemViewModel group = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_Group", "Grouping/Group") };
            group.Action = GroupContextMenuCmd;
            group.Validate = GroupValidateContextMenuCmd;
            menuItems.Add(group);

            MenuItemViewModel groupLocal = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_GroupLocal", "Grouping/Group (Local)"),  Command = "Local" };
            groupLocal.Action = GroupContextMenuCmd;
            groupLocal.Validate = GroupValidateContextMenuCmd;
            menuItems.Add(groupLocal);

            MenuItemViewModel ungroup = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_HierarchyViewModel_Ungroup", "Grouping/Ungroup") };
            ungroup.Action = UngroupContextMenuCmd;
            ungroup.Validate = UngroupValidateContextMenuCmd;
            menuItems.Add(ungroup);
        }

        protected virtual void CreatePrefabValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            if (!HasSelectedItems)
            {
                args.IsValid = false;
            }
        }

        protected virtual void CreatePrefabContextMenuCmd(string arg)
        {
            IProjectTreeModel projectTree = IOC.Resolve<IProjectTreeModel>();
            ProjectItem projectItem = projectTree.SelectedItem;
            if(projectItem == null || !projectItem.IsFolder)
            {
                projectItem = m_project.State.RootFolder;
            }
            Editor.CreatePrefabAsync(projectItem, SelectedItem, null);
        }

        protected virtual void DuplicateValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            if (!HasSelectedItems || !SelectedItems.Any(o => o.CanDuplicate))
            {
                args.IsValid = false;
            }
        }

        protected virtual void DuplicateContextMenuCmd(string arg)
        {
            GameObject[] gameObjects = SelectedItems.Select(o => o.gameObject).ToArray();
            Editor.Duplicate(gameObjects);
        }

        protected virtual void DeleteValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            if (!HasSelectedItems || !SelectedItems.Any(o => o.CanDelete))
            {
                args.IsValid = false;
            }
        }

        protected virtual void DeleteContextMenuCmd(string arg)
        {
            GameObject[] gameObjects = SelectedItems.Select(o => o.gameObject).ToArray();
            Editor.Delete(gameObjects);
        }

        protected virtual void RenameValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            if (!HasSelectedItems || !SelectedItems.First().CanRename)
            {
                args.IsValid = false;
            }
        }

        protected virtual void RenameContextMenuCmd(string arg)
        {
            IsEditing = true;
        }

        protected virtual void GroupValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            if (!HasSelectedItems || !m_groupingModel.CanGroup(SelectedGameObjects))
            {
                args.IsValid = false;
            }
        }

        protected virtual void GroupContextMenuCmd(string arg)
        {
            GameObject[] gameObjects = SelectedGameObjects;
            WindowManager.Prompt(
                Localization.GetString("ID_RTEditor_HierarchyViewModel_EnterGroupName", "Enter Group Name"),
                Localization.GetString("ID_RTEditor_HierarchyViewModel_DefaultGroupName", "Group"),
                (sender, args) =>
                {
                    string groupName = args.Text;
                    m_groupingModel.GroupAndRecord(gameObjects, groupName, arg == "Local");
                },
                (sender, args) => { });;
        }

        protected virtual void UngroupValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            if (!HasSelectedItems || !m_groupingModel.CanUngroup(SelectedGameObjects))
            {
                args.IsValid = false;
            }
        }

        protected virtual void UngroupContextMenuCmd(string arg)
        {
            GameObject[] gameObjects = SelectedGameObjects;
            m_groupingModel.UngroupAndRecord(gameObjects);
        }

        #endregion

        #region Methods

        protected override bool AllowDropExternalObjects()
        {
            if (TargetItem == null)
            {
                return true;
            }

            if (!IsFilterTextEmpty)
            {
                return false;
            }

            IEnumerable<ProjectItem> projectItems = ExternalDragObjects.OfType<ProjectItem>();
            return projectItems.Count() > 0 && projectItems.Any(projectItem => !projectItem.IsFolder && m_project.Utils.ToType(projectItem) == typeof(GameObject));
        }

        protected GameObject[] GetGameObjects(IEnumerable<ExposeToEditor> exposedToEditor)
        {
            if (exposedToEditor == null)
            {
                return null;
            }

            return exposedToEditor.Select(e => e.gameObject).ToArray();
        }

        protected ExposeToEditor[] GetExposedToEditor(GameObject[] gameObjects)
        {
            if (gameObjects == null)
            {
                return null;
            }

            return gameObjects.Select(g => g.GetComponent<ExposeToEditor>()).Where(e => e != null).ToArray();
        }

        protected override void OnSelectedItemsChanged(IEnumerable<ExposeToEditor> unselectedObjects, IEnumerable<ExposeToEditor> selectedObjects)
        {
            Selection.objects = GetGameObjects(selectedObjects);
        }

        protected void EditorSelectionToSelectedObjects()
        {
            m_selectedItems = GetExposedToEditor(Selection.gameObjects);
            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.Select(m_selectedItems));

            if (SelectedItems != null)
            {
                foreach (ExposeToEditor selectedObject in SelectedItems)
                {
                    if (!IsExpanded(GetParent(selectedObject)))
                    {
                        ExpandTo(selectedObject);
                    }
                }
            }
       }

        private List<GameObject> m_rootGameObjects;
        protected void SetSiblingIndex(ExposeToEditor obj)
        {
            if (obj.transform.parent == null && m_rootGameObjects == null)
            {
                m_rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects().OrderBy(g => g.transform.GetSiblingIndex()).ToList();
            }

            ExposeToEditor nextSibling = obj.NextSibling(m_rootGameObjects);
            if (nextSibling != null)
            {
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.PrevSiblingsChanged(obj, nextSibling));
            }
        }

        protected virtual void AddSortedByName(ExposeToEditor obj)
        {
            IEnumerable<ExposeToEditor> items = DataSource.GetChildren(null);
            string[] names = items.Select(go => go.name).Union(new[] { obj.name }).OrderBy(k => k).ToArray();
            int index = Array.IndexOf(names, obj.name);
            ExposeToEditor sibling;
            if (index == 0)
            {
                sibling = items.FirstOrDefault();
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(null, obj));
                if (sibling != null)
                {
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.PrevSiblingsChanged(obj, sibling));
                }
            }
            else
            {
                sibling = items.ElementAt(index - 1);
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(null, obj));
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.NextSiblingsChanged(obj, sibling));
            }
        }
        protected virtual void OnProjectItemsLoaded(UnityObject[] objects, ExposeToEditor dropTarget)
        {
            m_selectionComponent = m_placementModel.GetSelectionComponent();

            GameObject[] createdObjects = new GameObject[objects.Length];
            for (int i = 0; i < objects.Length; ++i)
            {
                GameObject prefab = (GameObject)objects[i];
                bool wasPrefabEnabled = prefab.activeSelf;
                prefab.SetActive(false);

                GameObject prefabInstance = InstantiatePrefab(prefab);
                Editor.AddGameObjectToHierarchy(prefabInstance);
                prefab.SetActive(wasPrefabEnabled);

                ExposeToEditor exposeToEditor = ExposePrefabInstance(prefabInstance);
                exposeToEditor.SetName(prefab.name);

                if (dropTarget == null)
                {
                    exposeToEditor.transform.SetParent(null);
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(null, exposeToEditor));
                }
                else
                {
                    if (LastDragDropAction == DragDropAction.SetLastChild)
                    {
                        exposeToEditor.transform.SetParent(dropTarget.transform);
                        RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(dropTarget, exposeToEditor));
                        
                        Expand(dropTarget);
                    }
                    else if (LastDragDropAction == DragDropAction.SetNextSibling)
                    {
                        ExposeToEditor dropTargetParent = dropTarget.GetParent();

                        exposeToEditor.transform.SetParent(dropTargetParent != null ? dropTargetParent.transform : null, false);
                        exposeToEditor.transform.SetSiblingIndex(dropTarget.transform.GetSiblingIndex() + 1);

                        RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(dropTarget.GetParent(), exposeToEditor));
                        RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.NextSiblingsChanged(exposeToEditor, dropTarget));
                    }
                    else if (LastDragDropAction == DragDropAction.SetPrevSilbling)
                    {
                        ExposeToEditor dropTargetParent = dropTarget.GetParent();

                        exposeToEditor.transform.SetParent(dropTargetParent != null ? dropTargetParent.transform : null, false);
                        exposeToEditor.transform.SetSiblingIndex(dropTarget.transform.GetSiblingIndex());

                        RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(dropTarget.GetParent(), exposeToEditor));
                        RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.PrevSiblingsChanged(exposeToEditor, dropTarget));
                    }
                }

                OnActivatePrefabInstance(prefabInstance);
                createdObjects[i] = prefabInstance;
            }

            if (createdObjects.Length > 0)
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                editor.RegisterCreatedObjects(createdObjects, m_selectionComponent != null ? m_selectionComponent.CanSelect : true);
            }

            CanDropExternalObjects = false;
        }

        protected virtual ExposeToEditor ExposePrefabInstance(GameObject prefabInstance)
        {
            Transform[] transforms = prefabInstance.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in transforms)
            {
                if (transform.GetComponent<ExposeToEditor>() == null)
                {
                    transform.gameObject.AddComponent<ExposeToEditor>();
                }
            }

            ExposeToEditor exposeToEditor = prefabInstance.GetComponent<ExposeToEditor>();
            if (exposeToEditor == null)
            {
                exposeToEditor = prefabInstance.AddComponent<ExposeToEditor>();
            }

            return exposeToEditor;
        }

        protected virtual void OnActivatePrefabInstance(GameObject prefabInstance)
        {
            prefabInstance.SetActive(true);
        }

        protected virtual GameObject InstantiatePrefab(GameObject prefab)
        {
            Vector3 pivot = Vector3.zero;
            if (m_selectionComponent != null)
            {
                pivot = m_selectionComponent.SecondaryPivot;
            }

            return Instantiate(prefab, pivot, Quaternion.identity);
        }

        protected override void ExpandTo(ExposeToEditor item)
        {
            if(!IsFilterTextEmpty)
            {
                return;
            }

            base.ExpandTo(item);
        }

        protected virtual bool RefreshTree(Record record, bool isRedo)
        {
            bool applyOnRedo = (bool)record.OldState;
            if (applyOnRedo != isRedo)
            {
                return false;
            }

            BindData();
            EditorSelectionToSelectedObjects();
            
            if (SelectedItems != null)
            {
                foreach (ExposeToEditor obj in SelectedItems.OfType<ExposeToEditor>().OrderBy(o => o.transform.GetSiblingIndex()))
                {
                    ExpandTo(obj);
                }
            }

            return false;
        }

        #endregion
    }

}
