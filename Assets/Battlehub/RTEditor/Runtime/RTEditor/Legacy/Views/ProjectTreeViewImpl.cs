using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using Battlehub.UIControls.MenuControl;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Battlehub.RTEditor.ViewModels;

namespace Battlehub.RTEditor
{
    public class SelectionChangedArgs<T> : EventArgs
    {
        /// <summary>
        /// Unselected Items
        /// </summary>
        public T[] OldItems
        {
            get;
            private set;
        }

        /// <summary>
        /// Selected Items
        /// </summary>
        public T[] NewItems
        {
            get;
            private set;
        }

        /// <summary>
        /// First Unselected Item
        /// </summary>
        public T OldItem
        {
            get
            {
                if (OldItems == null)
                {
                    return default(T);
                }
                if (OldItems.Length == 0)
                {
                    return default(T);
                }
                return OldItems[0];
            }
        }

        /// <summary>
        /// First Selected Item
        /// </summary>
        public T NewItem
        {
            get
            {
                if (NewItems == null)
                {
                    return default(T);
                }
                if (NewItems.Length == 0)
                {
                    return default(T);
                }
                return NewItems[0];
            }
        }

        public bool IsUserAction
        {
            get;
            private set;
        }

        public SelectionChangedArgs(T[] oldItems, T[] newItems, bool isUserAction)
        {
            OldItems = oldItems;
            NewItems = newItems;
            IsUserAction = isUserAction;
        }

        public SelectionChangedArgs(T oldItem, T newItem, bool isUserAction)
        {
            OldItems = new[] { oldItem };
            NewItems = new[] { newItem };
            IsUserAction = isUserAction;
        }

        public SelectionChangedArgs(IEnumerable oldItems, IEnumerable newItems, bool isUserAction)
        {
            if (oldItems != null)
            {
                OldItems = oldItems.OfType<T>().ToArray();
            }

            if (newItems != null)
            {
                NewItems = newItems.OfType<T>().ToArray();
            }
            IsUserAction = isUserAction;
        }

        public SelectionChangedArgs(SelectionChangedArgs args, bool isUserAction) : this(args.OldItems, args.NewItems, isUserAction)
        {
        }
    }


    public class ProjectTreeContextMenuEventArgs : ProjectTreeEventArgs
    {
        public List<MenuItemInfo> MenuItems
        {
            get;
            private set;
        }

        public ProjectTreeContextMenuEventArgs(ProjectItem[] projectItems, List<MenuItemInfo> menuItems)
            : base(projectItems)
        {
            MenuItems = menuItems;
        }
    }

    public class ProjectTreeEventArgs : EventArgs
    {
        public ProjectItem[] ProjectItems
        {
            get;
            private set;
        }

        public ProjectItem ProjectItem
        {
            get
            {
                if (ProjectItems == null || ProjectItems.Length == 0)
                {
                    return null;
                }
                return ProjectItems[0];
            }
        }

        public ProjectTreeEventArgs(ProjectItem[] projectItems)
        {
            ProjectItems = projectItems;
        }
    }

    public class ProjectTreeRenamedEventArgs : ProjectTreeEventArgs
    {
        public string[] OldNames
        {
            get;
            private set;
        }

        public string OldName
        {
            get
            {
                if (OldNames == null || OldNames.Length == 0)
                {
                    return null;
                }
                return OldNames[0];
            }
        }

        public ProjectTreeRenamedEventArgs(ProjectItem[] projectItems, string[] oldNames)
            : base(projectItems)
        {
            OldNames = oldNames;
        }
    }

    public class ProjectTreeCancelEventArgs : ProjectTreeEventArgs
    {
        public bool Cancel;

        public ProjectTreeCancelEventArgs(ProjectItem[] items) : base(items)
        {
        }
    }

    //[Obsolete]
    public interface IProjectTree 
    {
        event EventHandler<ItemDataBindingArgs> ItemDataBinding;
        event EventHandler<SelectionChangedArgs<ProjectItem>> SelectionChanged;
        event EventHandler<ProjectTreeCancelEventArgs> ItemsDeleting;
        event EventHandler<ProjectTreeRenamedEventArgs> ItemRenamed;
        event EventHandler<ProjectTreeEventArgs> ItemsDeleted;
        event EventHandler<ProjectTreeContextMenuEventArgs> ContextMenu;
        event EventHandler Destroyed;

        ProjectItem SelectedItem
        {
            get;
            set;
        }

        void DeleteSelectedItems();
    }
}

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*Obsolete*/]
    #pragma warning disable CS0612
    public class ProjectTreeViewImpl : MonoBehaviour, IProjectTree
    #pragma warning restore CS0612
    {
        public event EventHandler<ItemDataBindingArgs> ItemDataBinding;
        public event EventHandler<SelectionChangedArgs<ProjectItem>> SelectionChanged;
        public event EventHandler<ProjectTreeRenamedEventArgs> ItemRenamed;
        public event EventHandler<ProjectTreeCancelEventArgs> ItemsDeleting;
        public event EventHandler<ProjectTreeEventArgs> ItemsDeleted;
        public event EventHandler<ProjectTreeContextMenuEventArgs> ContextMenu;
        public event EventHandler Destroyed;

        public ProjectItem[] SelectedFolders
        {
            get
            {
                return m_treeView.SelectedItemsCount > 0 ?
                    m_treeView.SelectedItems.OfType<ProjectItem>().ToArray() :
                    new ProjectItem[0];
            }
        }

        public ProjectItem SelectedItem
        {
            get
            {
                return (ProjectItem)m_treeView.SelectedItem;
            }
            set
            {
                if (value == null)
                {
                    m_treeView.SelectedIndex = -1;
                }
                else
                {
                    ProjectItem folder = value;
                    string path = folder.ToString();
                    folder = m_root.Get(path);

                    if (folder != null)
                    {
                        if (folder.Parent == null)
                        {
                            Expand(folder);
                        }
                        else
                        {
                            Expand(folder.Parent);
                        }
                    }

                    if (m_treeView.IndexOf(folder) >= 0)
                    {
                        m_treeView.ScrollIntoView(folder);
                        m_treeView.SelectedItem = folder;
                    }
                }
            }
        }

        private void Expand(ProjectItem item)
        {
            if (item == null)
            {
                return;
            }
            if (item.Parent != null && !m_treeView.IsExpanded(item.Parent))
            {
                Expand(item.Parent);
            }
            m_treeView.Expand(item);
        }


        private void Toggle(ProjectItem projectItem)
        {
            VirtualizingTreeViewItem treeViewItem = m_treeView.GetTreeViewItem(projectItem);
            if (treeViewItem == null)
            {
                Toggle(projectItem.Parent);
                treeViewItem = m_treeView.GetTreeViewItem(projectItem);
            }
            else
            {
                treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
            }
        }

        private IProjectAsync m_project;
        protected IProjectAsync Project
        {
            get { return m_project; }
        }

        private IRuntimeEditor m_editor;
        protected IRuntimeEditor Editor
        {
            get { return m_editor; }
        }

        private ProjectTreeView m_projectTreeView;
        protected RuntimeWindow Window
        {
            get { return m_projectTreeView; }
        }

        private VirtualizingTreeView m_treeView;
        protected VirtualizingTreeView TreeView
        {
            get { return m_treeView; }
        }

        private bool m_showRootFolder;
        protected bool ShowRootFolder
        {
            get { return m_showRootFolder; }
        }

        private Sprite m_folderIcon;
        protected Sprite FolderIcon
        {
            get { return m_folderIcon; }
        }

        [NonSerialized]
        private ProjectItem m_root;
        private IWindowManager m_wm;
        private ILocalization m_localization;

        protected virtual void Awake()
        {
            m_projectTreeView = GetComponent<ProjectTreeView>();
            m_showRootFolder = m_projectTreeView.ShowRootFolder;
            m_folderIcon = m_projectTreeView.FolderIcon;

            m_editor = IOC.Resolve<IRuntimeEditor>();
            if (Editor == null)
            {
                Debug.LogError("Editor is null");
                return;
            }

            m_project = IOC.Resolve<IProjectAsync>();
            m_wm = IOC.Resolve<IWindowManager>();
            m_localization = IOC.Resolve<ILocalization>();

            m_treeView = Instantiate(m_projectTreeView.TreeViewPrefab, transform).GetComponent<VirtualizingTreeView>();
            m_treeView.name = "ProjectTreeView";

            m_treeView.CanReorder = false;
            m_treeView.CanReparent = ShowRootFolder;
            m_treeView.CanUnselectAll = false;
            m_treeView.CanDrag = ShowRootFolder;
            m_treeView.CanRemove = false;
            m_treeView.CanSelectAll = false;

            m_treeView.SelectionChanged += OnSelectionChanged;
            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.ItemExpanding += OnItemExpanding;
            m_treeView.ItemsRemoving += OnItemsRemoving;
            m_treeView.ItemsRemoved += OnItemsRemoved;
            m_treeView.ItemBeginEdit += OnItemBeginEdit;
            m_treeView.ItemEndEdit += OnItemEndEdit;
            m_treeView.ItemBeginDrag += OnItemBeginDrag;
            m_treeView.ItemBeginDrop += OnItemBeginDrop;
            m_treeView.ItemDragEnter += OnItemDragEnter;
            m_treeView.ItemDrag += OnItemDrag;
            m_treeView.ItemDragExit += OnItemDragExit;
            m_treeView.ItemDrop += OnItemDrop;
            m_treeView.ItemEndDrag += OnItemEndDrag;
            m_treeView.ItemDoubleClick += OnItemDoubleClick;
            m_treeView.ItemClick += OnItemClick;
            m_treeView.ItemHold += OnItemHold;

            m_projectTreeView.DragEnterEvent += OnDragEnter;
            m_projectTreeView.DragLeaveEvent += OnDragLeave;
            m_projectTreeView.DragEvent += OnDrag;
            m_projectTreeView.DropEvent += OnDrop;

            IOC.RegisterFallback<IProjectTree>(this);
        }

        protected virtual void OnDestroy()
        {
            if (m_treeView != null)
            {
                m_treeView.SelectionChanged -= OnSelectionChanged;
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.ItemExpanding -= OnItemExpanding;
                m_treeView.ItemsRemoving -= OnItemsRemoving;
                m_treeView.ItemsRemoved -= OnItemsRemoved;
                m_treeView.ItemBeginEdit -= OnItemBeginEdit;
                m_treeView.ItemEndEdit -= OnItemEndEdit;
                m_treeView.ItemBeginDrag -= OnItemBeginDrag;
                m_treeView.ItemBeginDrop -= OnItemBeginDrop;
                m_treeView.ItemDragEnter -= OnItemDragEnter;
                m_treeView.ItemDrag -= OnItemDrag;
                m_treeView.ItemDragExit -= OnItemDragExit;
                m_treeView.ItemDrop -= OnItemDrop;
                m_treeView.ItemEndDrag -= OnItemEndDrag;
                m_treeView.ItemDoubleClick -= OnItemDoubleClick;
                m_treeView.ItemClick -= OnItemClick;
                m_treeView.ItemHold -= OnItemHold;
            }

            if (m_projectTreeView != null)
            {
                m_projectTreeView.DragEnterEvent -= OnDragEnter;
                m_projectTreeView.DragLeaveEvent -= OnDragLeave;
                m_projectTreeView.DragEvent -= OnDrag;
                m_projectTreeView.DropEvent -= OnDrop;
            }

            if (Destroyed != null)
            {
                Destroyed(this, EventArgs.Empty);
            }

            IOC.UnregisterFallback<IProjectTree>(this);
        }

        public void LoadProject(ProjectItem root)
        {
            if (root == null)
            {
                m_treeView.Items = null;
            }
            else
            {
                if (ShowRootFolder)
                {
                    m_treeView.Items = new[] { root };
                }
                else
                {
                    if (root.Children != null)
                    {
                        m_root.Children = root.Children.OrderBy(projectItem => projectItem.NameExt).ToList();
                        m_treeView.Items = m_root.Children.Where(projectItem => CanDisplayFolder(projectItem)).ToArray();
                    }
                }
            }

            m_root = root;
        }

        public void ChangeParent(ProjectItem projectItem, ProjectItem oldParent)
        {
            if (!m_treeView.IsDropInProgress && m_treeView.GetItemContainerData(projectItem) != null)
            {
                m_treeView.ChangeParent(projectItem.Parent, projectItem);
            }

            VirtualizingTreeViewItem tvOldParent = m_treeView.GetTreeViewItem(oldParent);
            if (tvOldParent != null)
            {
                tvOldParent.CanExpand = oldParent.Children != null && oldParent.Children.Any(c => c.IsFolder);
            }

            VirtualizingTreeViewItem tvNewParent = m_treeView.GetTreeViewItem(projectItem.Parent);
            if (tvNewParent != null)
            {
                tvNewParent.CanExpand = true;
            }
        }

        public void UpdateProjectItem(ProjectItem item)
        {
            VirtualizingItemContainer itemContainer = m_treeView.GetItemContainer(item);
            if (itemContainer != null)
            {
                m_treeView.DataBindItem(item, itemContainer);
            }
        }

        public void RemoveProjectItemsFromTree(ProjectItem[] projectItems)
        {
            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
                if (projectItem.IsFolder)
                {
                    m_treeView.RemoveChild(projectItem.Parent, projectItem);
                }
            }
        }

        public void SelectRootIfNothingSelected()
        {
            if (m_treeView.SelectedIndex < 0)
            {
                m_treeView.SelectedIndex = 0;
            }
        }

        private void OnItemClick(object sender, ItemArgs e)
        {
            if (e.PointerEventData.button == PointerEventData.InputButton.Right)
            {
                ProjectItem[] projectItems = e.Items.OfType<ProjectItem>().ToArray();
                OpenContextMenu(projectItems);
            }
        }

        private void OnItemHold(object sender, ItemArgs e)
        {
            if (e.PointerEventData.pointerId == 0)
            {
                ProjectItem[] projectItems = e.Items.OfType<ProjectItem>().ToArray();
                OpenContextMenu(projectItems);
            }
        }

        private void OpenContextMenu(ProjectItem[] projectItems)
        {
            IContextMenu menu = IOC.Resolve<IContextMenu>();

            List<MenuItemInfo> menuItems = new List<MenuItemInfo>();
            OnContextMenu(menuItems);

            if (ContextMenu != null)
            {
                ProjectTreeContextMenuEventArgs args = new ProjectTreeContextMenuEventArgs(projectItems, menuItems);
                ContextMenu(this, args);
            }

            if (menuItems.Count > 0)
            {
                menu.Open(menuItems.ToArray());
            }
        }

        private void OnItemDoubleClick(object sender, ItemArgs e)
        {
            if (e.PointerEventData.button == PointerEventData.InputButton.Left)
            {
                ProjectItem projectItem = (ProjectItem)e.Items[0];
                Toggle(projectItem);
            }
        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseBeginDrag(this, e.Items, e.PointerEventData);
        }

        private bool FolderContainsItemWithSameName(object dropTarget, object[] dragItems)
        {
            ProjectItem folder = (ProjectItem)dropTarget;
            if (folder.Children == null || folder.Children.Count == 0)
            {
                return false;
            }

            foreach (ProjectItem projectItem in dragItems)
            {
                if (folder.Children.Any(child => child.NameExt == projectItem.NameExt))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnItemDragEnter(object sender, ItemDropCancelArgs e)
        {
            if (e.DropTarget == null || ProjectItem.IsAssetItem(e.DropTarget) || e.DragItems != null && e.DragItems.Contains(e.DropTarget) || FolderContainsItemWithSameName(e.DropTarget, e.DragItems))
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
                e.Cancel = true;
            }
            else
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
        }

        private void OnItemDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrag(e.PointerEventData);
        }

        private void OnItemDragExit(object sender, EventArgs e)
        {
            Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
        }

        private void OnItemBeginDrop(object sender, ItemDropCancelArgs e)
        {
            if (!e.IsExternal)
            {
                ProjectItem dropFolder = (ProjectItem)e.DropTarget;
                e.Cancel = !CanDrop(dropFolder, e.DragItems.OfType<ProjectItem>().ToArray());
            }
        }

        private async void OnItemDrop(object sender, ItemDropArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);

            if (e.Action == ItemDropAction.SetLastChild)
            {
                Editor.IsBusy = true;
                await m_project.Safe.MoveAsync(e.DragItems.OfType<ProjectItem>().ToArray(), (ProjectItem)e.DropTarget);
                Editor.IsBusy = false;
            }
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);
        }

        private void OnItemBeginEdit(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ProjectItem item = e.Item as ProjectItem;
            if (item != null)
            {
                TMP_InputField inputField = e.EditorPresenter.GetComponentInChildren<TMP_InputField>(true);
                inputField.text = item.Name;
                inputField.ActivateInputField();
                inputField.Select();

                Image image = e.EditorPresenter.GetComponentInChildren<Image>(true);
                image.sprite = FolderIcon;
                image.gameObject.SetActive(true);

                LayoutElement layout = inputField.GetComponent<LayoutElement>();

                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = item.Name;

                RectTransform rt = text.GetComponent<RectTransform>();
                layout.preferredWidth = rt.rect.width;
            }
        }

        private void OnItemEndEdit(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            TMP_InputField inputField = e.EditorPresenter.GetComponentInChildren<TMP_InputField>(true);
            TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);

            ProjectItem projectItem = (ProjectItem)e.Item;
            string oldName = projectItem.Name;
            if (projectItem.Parent != null)
            {
                ProjectItem parentItem = projectItem.Parent;
                string newNameExt = inputField.text.Trim() + projectItem.Ext;
                if (!string.IsNullOrEmpty(inputField.text.Trim()) && ProjectItem.IsValidName(inputField.text.Trim()) && !parentItem.Children.Any(p => p.NameExt == newNameExt))
                {
                    projectItem.Name = inputField.text.Trim();
                }
            }

            if (projectItem.Name != oldName)
            {
                if (ItemRenamed != null)
                {
                    ItemRenamed(this, new ProjectTreeRenamedEventArgs(new[] { projectItem }, new[] { oldName }));
                }
            }

            text.text = projectItem.Name;

            //Following code is required to unfocus inputfield if focused and release InputManager
            if (EventSystem.current != null && !EventSystem.current.alreadySelecting)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void OnItemsRemoving(object sender, ItemsCancelArgs e)
        {
            if (e.Items == null)
            {
                return;
            }

            if (!Editor.ActiveWindow == this)
            {
                e.Items.Clear();
                return;
            }

            for (int i = e.Items.Count - 1; i >= 0; i--)
            {
                ProjectItem item = (ProjectItem)e.Items[i];
                if (m_project.Utils.IsStatic(item))
                {
                    e.Items.Remove(item);
                }
            }

            if (e.Items.Count == 0)
            {
                m_wm.MessageBox("Can't remove folder", "Unable to remove folders exposed from editor");
            }
        }

        private void OnItemsRemoved(object sender, ItemsRemovedArgs e)
        {
            if (ItemsDeleted != null)
            {
                ItemsDeleted(this, new ProjectTreeEventArgs(e.Items.OfType<ProjectItem>().ToArray()));
            }
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            ProjectItem item = e.Item as ProjectItem;
            if (item != null)
            {
                item.Children = item.Children
                    .OrderBy(projectItem => projectItem.NameExt).ToList();
                e.Children = item.Children
                    .Where(projectItem => CanDisplayFolder(projectItem))
                    .OrderBy(projectItem => projectItem.NameExt);
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ProjectItem item = e.Item as ProjectItem;
            if (item != null)
            {
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = item.Name;

                Image image = e.ItemPresenter.GetComponentInChildren<Image>(true);
                image.sprite = FolderIcon;
                image.gameObject.SetActive(true);
                e.CanEdit = !m_project.Utils.IsStatic(item) && item.Parent != null;
                e.CanDrag = !m_project.Utils.IsStatic(item) && item.Parent != null;
                e.HasChildren = item.Children != null && item.Children.Count(projectItem => CanDisplayFolder(projectItem)) > 0;
            }

            if (ItemDataBinding != null)
            {
                ItemDataBinding(this, e);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new SelectionChangedArgs<ProjectItem>(e.OldItems, e.NewItems, true));
            }
        }

        protected virtual bool CanDisplayFolder(ProjectItem projectItem)
        {
            return projectItem.IsFolder;
        }

        private bool CanCreatePrefab(ProjectItem dropTarget, object[] dragItems)
        {
            ExposeToEditor[] objects = dragItems.OfType<ExposeToEditor>().ToArray();
            if (objects.Length == 0)
            {
                return false;
            }

            if (!objects.All(o => o.CanCreatePrefab))
            {
                return false;
            }

            return true;
        }

        private bool CanDrop(ProjectItem dropFolder, object[] dragItems)
        {
            if (dropFolder == null)
            {
                return false;
            }

            ProjectItem[] dragProjectItems = dragItems.OfType<ProjectItem>().ToArray();
            if (dragProjectItems.Length == 0)
            {
                return false;
            }

            if (dropFolder.Children == null)
            {
                return true;
            }

            for (int i = 0; i < dragProjectItems.Length; ++i)
            {
                ProjectItem dragItem = dragProjectItems[i];
                if (dropFolder.IsDescendantOf(dragItem))
                {
                    return false;
                }

                if (dropFolder.Children.Any(childItem => childItem.NameExt == dragItem.NameExt))
                {
                    return false;
                }
            }
            return true;
        }


        protected virtual void OnDragEnter(PointerEventData pointerEventData)
        {
            m_treeView.ExternalBeginDrag(pointerEventData.position);
        }

        protected virtual void OnDragLeave(PointerEventData pointerEventData)
        {
            m_treeView.ExternalDrop();
            Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
        }

        protected virtual void OnDrag(PointerEventData pointerEventData)
        {
            m_treeView.ExternalDrag(pointerEventData.position);
            object[] dragObjects = Editor.DragDrop.DragObjects;
            bool canCreatePrefab = CanCreatePrefab((ProjectItem)m_treeView.DropTarget, dragObjects);
            bool canDrop = CanDrop((ProjectItem)m_treeView.DropTarget, dragObjects);
            if (!canCreatePrefab && !canDrop)
            {
                m_treeView.ClearTarget();

                Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            }
            else
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
        }

        protected async virtual void OnDrop(PointerEventData pointerEventData)
        {
            ProjectItem dropTarget = (ProjectItem)m_treeView.DropTarget;
            object[] dragObjects = Editor.DragDrop.DragObjects;
            if (CanDrop(dropTarget, dragObjects))
            {
                Editor.IsBusy = true;
                await m_project.MoveAsync(dragObjects.OfType<ProjectItem>().ToArray(), dropTarget);
                Editor.IsBusy = false;
            }
            else if (dropTarget != null && dropTarget.IsFolder && CanCreatePrefab(dropTarget, dragObjects))
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                ExposeToEditor dragObject = (ExposeToEditor)dragObjects[0];
                if (dropTarget.IsFolder)
                {
                    editor.CreatePrefab(dropTarget, dragObject, null, assetItem =>
                    {
                    });
                }
            }
            m_treeView.ExternalDrop();
        }

        protected virtual void OnContextMenu(List<MenuItemInfo> menuItems)
        {
            MenuItemInfo createFolder = new MenuItemInfo { Path = m_localization.GetString("ID_RTEditor_ProjectTreeView_CreateFolder", "Create Folder") };
            createFolder.Validate = new MenuItemValidationEvent();
            createFolder.Validate.AddListener(CreateFolderValidateContextMenuCmd);
            createFolder.Action = new MenuItemEvent();
            createFolder.Action.AddListener(CreateFolderContextMenuCmd);
            menuItems.Add(createFolder);

            MenuItemInfo duplicateFolder = new MenuItemInfo { Path = m_localization.GetString("ID_RTEditor_ProjectTreeView_Duplicate", "Duplicate") };
            duplicateFolder.Validate = new MenuItemValidationEvent();
            duplicateFolder.Validate.AddListener(DuplicateValidateContextMenuCmd);
            duplicateFolder.Action = new MenuItemEvent();
            duplicateFolder.Action.AddListener(DuplicateContextMenuCmd);
            menuItems.Add(duplicateFolder);

            MenuItemInfo deleteFolder = new MenuItemInfo { Path = m_localization.GetString("ID_RTEditor_ProjectTreeView_Delete", "Delete") };
            deleteFolder.Validate = new MenuItemValidationEvent();
            deleteFolder.Validate.AddListener(DeleteFolderValidateContextMenuCmd);
            deleteFolder.Action = new MenuItemEvent();
            deleteFolder.Action.AddListener(DeleteFolderContextMenuCmd);
            menuItems.Add(deleteFolder);

            MenuItemInfo renameFolder = new MenuItemInfo { Path = m_localization.GetString("ID_RTEditor_ProjectTreeView_Rename", "Rename") };
            renameFolder.Validate = new MenuItemValidationEvent();
            renameFolder.Validate.AddListener(RenameValidateContextMenuCmd);
            renameFolder.Action = new MenuItemEvent();
            renameFolder.Action.AddListener(RenameFolderContextMenuCmd);
            menuItems.Add(renameFolder);
        }

        protected virtual void CreateFolderValidateContextMenuCmd(MenuItemValidationArgs args)
        {   
        }

        protected async virtual void CreateFolderContextMenuCmd(string arg)
        {
            ProjectItem parentFolder = (ProjectItem)m_treeView.SelectedItem;
            
            string[] existingNames = parentFolder.Children.Where(c => c.IsFolder).Select(c => c.Name).ToArray();
            
            string name = m_project.Utils.GetUniqueName(m_localization.GetString("ID_RTEditor_ProjectTreeView_Folder", "Folder"), parentFolder.Children == null ? new string[0] : existingNames);

            Editor.IsBusy = true;
            ProjectItem[] folders = await m_project.CreateFoldersAsync(new[] { parentFolder }, new[] { name });
            parentFolder.AddChild(folders[0]);
            AddItem(parentFolder, folders[0], existingNames);
            Editor.IsBusy = false;
        }

        protected virtual void DuplicateValidateContextMenuCmd(MenuItemValidationArgs args)
        {
            ProjectItem projectItem = (ProjectItem)TreeView.SelectedItem;

            if (projectItem == null || projectItem.Parent == null)
            {
                args.IsValid = false;
            }
        }

        protected async virtual void DuplicateContextMenuCmd(string arg)
        {
            await m_project.DuplicateAsync(TreeView.SelectedItems.OfType<ProjectItem>().ToArray());
        }

        protected virtual void DeleteFolderValidateContextMenuCmd(MenuItemValidationArgs args)
        {
            ProjectItem projectItem = (ProjectItem)TreeView.SelectedItem;

            if (projectItem == null || projectItem.Parent == null)
            {
                args.IsValid = false;
            }
        }

        protected virtual void DeleteFolderContextMenuCmd(string arg)
        {
            DeleteSelectedItems();
        }

        protected virtual void RenameValidateContextMenuCmd(MenuItemValidationArgs args)
        {
            VirtualizingTreeViewItem treeViewItem = m_treeView.GetTreeViewItem(m_treeView.SelectedItem);
            args.IsValid = treeViewItem != null && treeViewItem.CanEdit;
        }

        protected virtual void RenameFolderContextMenuCmd(string arg)
        {
            VirtualizingTreeViewItem treeViewItem = m_treeView.GetTreeViewItem(m_treeView.SelectedItem);
            if (treeViewItem != null && treeViewItem.CanEdit)
            {
                treeViewItem.IsEditing = true;
            }
        }

        public virtual void DeleteSelectedItems()
        {
            if (m_treeView.SelectedItem != null)
            {
                ProjectItem[] projectItems = m_treeView.SelectedItems.OfType<ProjectItem>().ToArray();
                if (ItemsDeleting != null)
                {
                    ProjectTreeCancelEventArgs args = new ProjectTreeCancelEventArgs(projectItems);
                    ItemsDeleting(this, args);
                    if (args.Cancel)
                    {
                        return;
                    }
                }

                if (projectItems.Any(p => p.Parent == null))
                {
                    m_wm.MessageBox(
                        m_localization.GetString("ID_RTEditor_ProjectTreeView_UnableToRemove", "Unable to remove"),
                        m_localization.GetString("ID_RTEditor_ProjectTreeView_UnableToRemoveRootFolder", "Unable to remove root folder"));
                }
                else
                {
                    m_wm.Confirmation(
                        m_localization.GetString("ID_RTEditor_ProjectTreeView_DeleteSelectedAssets", "Delete selected assets"),
                        m_localization.GetString("ID_RTEditor_ProjectTreeView_YouCannotUndoThisAction", "You cannot undo this action"), (dialog, arg) =>
                        {
                            m_treeView.RemoveSelectedItems();
                        },
                    (dialog, arg) => { },
                        m_localization.GetString("ID_RTEditor_ProjectTreeView_Btn_Delete", "Delete"),
                        m_localization.GetString("ID_RTEditor_ProjectTreeView_Btn_Cancel", "Cancel"));
                }
            }
        }

        public virtual void SelectAll()
        {
            m_treeView.SelectedItems = m_treeView.Items;
        }

        public virtual void AddItem(ProjectItem parentFolder, ProjectItem folder)
        {
            AddItem(parentFolder, folder, true, true);
        }

        public virtual void AddItem(ProjectItem parentFolder, ProjectItem folder, bool select, bool expand)
        {
            string[] existingNames = parentFolder.Children.Where(c => c != folder && c.IsFolder).Select(c => c.Name).ToArray();
            AddItem(parentFolder, folder, existingNames, select, expand);
        }

        protected virtual void AddItem(ProjectItem parentFolder, ProjectItem folder, string[] existingNames)
        {
            AddItem(parentFolder, folder, existingNames, true, true);
        }

        protected virtual void AddItem(ProjectItem parentFolder, ProjectItem folder, string[] existingNames, bool select, bool expand)
        {
            m_treeView.AddChild(parentFolder, folder);

            if (existingNames.Length > 0)
            {
                int index = Array.IndexOf(existingNames.Union(new[] { folder.Name }).OrderBy(n => n).ToArray(), folder.Name);
                if (index > 0)
                {
                    m_treeView.SetNextSibling(parentFolder.Children.Where(c => c.IsFolder).OrderBy(c => c.Name).ElementAt(index - 1), folder);
                }
                else
                {
                    m_treeView.SetPrevSibling(parentFolder.Children.Where(c => c.IsFolder).OrderBy(c => c.Name).ElementAt(index + 1), folder);
                }
            }

            if(expand)
            {
                ProjectItem projectItem = parentFolder;
                Expand(parentFolder);
            }

            if(select)
            {
                if(m_treeView.GetTreeViewItem(folder) != null)
                {
                    m_treeView.ScrollIntoView(folder);
                }
                m_treeView.SelectedItem = folder;
            }  
        }
    }
}