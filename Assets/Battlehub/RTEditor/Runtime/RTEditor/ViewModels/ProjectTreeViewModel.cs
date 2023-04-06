using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityWeld.Binding;


namespace Battlehub.RTEditor.ViewModels
{
    public interface IProjectTreeModel
    {
        event EventHandler<ContextMenuEventArgs> ContextMenuOpened;

        bool HasSelectedItems
        {
            get;
        }

        ProjectItem SelectedItem
        {
            get;
            set;
        }

        IEnumerable<ProjectItem> SelectedItems
        {
            get;
        }
    }

    [Binding]
    public class ProjectTreeViewModel : HierarchicalDataViewModel<ProjectItem>, IProjectTreeModel
    {
        public event Action<IEnumerable<ProjectItem>, IEnumerable<ProjectItem>> SelectionChanged;
        public event Action<ProjectItem, string> ItemRenamed;
        public event Action<IEnumerable<ProjectItem>> ItemsRemoved;

        private ProjectItem m_root;
        public ProjectItem RootFolder
        {
            get { return m_root; }
            set
            {
                if (m_root != value)
                {
                    m_root = value;
                    if (m_root != null && m_root.Children != null)
                    {
                        m_root.Children = RootFolder.Children.OrderBy(projectItem => projectItem.NameExt).ToList();
                    }
                    BindData();
                }
            }
        }

        public override ProjectItem SelectedItem
        {
            get { return base.SelectedItem; }
            set
            {
                if (value == null)
                {
                    base.SelectedItem = null;
                }
                else
                {
                    ProjectItem selectedItem = value;
                    string path = selectedItem.ToString();
                    selectedItem = m_root.Get(path);

                    if (CanDisplay(selectedItem))
                    {
                        if (selectedItem != null)
                        {
                            if (selectedItem.Parent == null)
                            {
                                Expand(selectedItem);
                            }
                            else
                            {
                                Expand(selectedItem.Parent);
                            }
                        }

                        ScrollIntoView = true;
                        base.SelectedItem = selectedItem;
                        ScrollIntoView = false;
                    }
                }
            }
        }

        private IProjectAsync m_project;
        protected IProjectAsync Project
        {
            get { return m_project; }
        }

        protected override void Start()
        {
            base.Start();
            IOC.RegisterFallback<IProjectTreeModel>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            IOC.UnregisterFallback<IProjectTreeModel>(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_project = IOC.Resolve<IProjectAsync>();
        }

        protected override void OnDisable()
        {
            m_project = null;
            base.OnDisable();
        }

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            HierarchicalDataFlags flags = HierarchicalDataFlags.Default;

            flags &= ~HierarchicalDataFlags.CanReorder;
            flags &= ~HierarchicalDataFlags.CanSelectAll;
            flags &= ~HierarchicalDataFlags.CanUnselectAll;
            flags &= ~HierarchicalDataFlags.CanRemove;

            return flags;
        }

        public override HierarchicalDataItemFlags GetItemFlags(ProjectItem item)
        {
            HierarchicalDataItemFlags flags = HierarchicalDataItemFlags.Default;
            bool isStatic = m_project.Utils.IsStatic(item);
            if (isStatic)
            {
                flags &= ~HierarchicalDataItemFlags.CanRemove;
            }

            if (isStatic || item.Parent == null)
            {
                flags &= ~HierarchicalDataItemFlags.CanEdit;
                flags &= ~HierarchicalDataItemFlags.CanDrag;
            }

            if (!CanDrop(item, SourceItems))
            {
                flags &= ~HierarchicalDataItemFlags.CanBeParent;
            }

            return flags;
        }

        public override bool HasChildren(ProjectItem parent)
        {
            return parent.Children != null && parent.Children.Where(item => CanDisplay(item)).Any();
        }

        public override IEnumerable<ProjectItem> GetChildren(ProjectItem parent)
        {
            if (parent == null)
            {
                return new[] { m_root };
            }

            parent.Children = parent.Children.OrderBy(projectItem => projectItem.NameExt).ToList();
            return parent.Children.Where(item => CanDisplay(item));
        }

        public override ProjectItem GetParent(ProjectItem item)
        {
            return item.Parent;
        }

        public override int IndexOf(ProjectItem parent, ProjectItem item)
        {
            return parent.Children.IndexOf(item);
        }

        #endregion

        #region Bound Unity Event Handlers

        protected override void OnSelectedItemsChanged(IEnumerable<ProjectItem> unselectedObjects, IEnumerable<ProjectItem> selectedObjects)
        {
            SelectionChanged?.Invoke(unselectedObjects, selectedObjects);
        }

        public override async void OnExternalObjectDrop()
        {
            if (CanDrop(TargetItem, ExternalDragObjects))
            {
                Editor.IsBusy = true;
                await m_project.MoveAsync(ExternalDragObjects.OfType<ProjectItem>().ToArray(), TargetItem);
                Editor.IsBusy = false;
            }
            else if (CanCreatePrefab(TargetItem, ExternalDragObjects))
            {
                CreatePrefab(TargetItem, ExternalDragObjects);
            }

            CanDropExternalObjects = false;
        }

        public override void OnItemDragEnter()
        {
            if (!CanDrop(TargetItem, SourceItems))
            {
                TargetItem = null;
            }
        }

        public override async void OnItemsDrop()
        {
            CanDropItems = false;

            if (LastDragDropAction == DragDropAction.SetLastChild)
            {
                Editor.IsBusy = true;
                await m_project.Safe.MoveAsync(SourceItems.ToArray(), TargetItem);
                Editor.IsBusy = false;
            }
        }

        public override void OnItemsRemoved()
        {
            ItemsRemoved?.Invoke(SourceItems);
        }

        public override void OnItemDoubleClick()
        {
            base.OnItemDoubleClick();

            if (IsExpanded(TargetItem))
            {
                Collapse(TargetItem);
            }
            else
            {
                Expand(TargetItem);
            }
        }

        private string oldName;
        public override void OnItemBeginEdit()
        {
            oldName = TargetItem.Name;
        }

        public override void OnItemEndEdit()
        {
            string newName = TargetItem.Name;
            TargetItem.Name = oldName;
            if (CanRename(TargetItem, newName))
            {
                TargetItem.Name = newName;
                if (oldName != newName)
                {
                    ItemRenamed?.Invoke(TargetItem, oldName);
                }
            }
        }


        public override void OnSelectAll()
        {
            SelectedItems = GetExpandedItems();
        }

        public override async void OnDuplicate()
        {
            await DuplicateSelectedItems();
        }

        public override void OnDelete()
        {
            RemoveSelectedItems();
        }

        #endregion

        #region Context Menu

        protected override void OnContextMenu(List<MenuItemViewModel> menuItems)
        {
            base.OnContextMenu(menuItems);

            MenuItemViewModel createFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_ProjectTreeView_CreateFolder", "Create Folder") };
            createFolder.Validate = CreateFolderValidateContextMenuCmd;
            createFolder.Action = CreateFolderContextMenuCmd;
            menuItems.Add(createFolder);

            MenuItemViewModel duplicateFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_ProjectTreeView_Duplicate", "Duplicate") };
            duplicateFolder.Validate = DuplicateValidateContextMenuCmd;
            duplicateFolder.Action = DuplicateContextMenuCmd;
            menuItems.Add(duplicateFolder);

            MenuItemViewModel deleteFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_ProjectTreeView_Delete", "Delete") };
            deleteFolder.Validate = DeleteFolderValidateContextMenuCmd;
            deleteFolder.Action = DeleteFolderContextMenuCmd;
            menuItems.Add(deleteFolder);

            MenuItemViewModel renameFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_ProjectTreeView_Rename", "Rename") };
            renameFolder.Validate = RenameValidateContextMenuCmd;
            renameFolder.Action = RenameFolderContextMenuCmd;
            menuItems.Add(renameFolder);
        }

        protected virtual void CreateFolderValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
        }

        protected async virtual void CreateFolderContextMenuCmd(string arg)
        {
            ProjectItem parentFolder = SelectedItems.First();

            string[] existingNames = parentFolder.Children.Where(c => c.IsFolder).Select(c => c.Name).ToArray();

            string name = m_project.Utils.GetUniqueName(Localization.GetString("ID_RTEditor_ProjectTreeView_Folder", "Folder"), parentFolder.Children == null ? new string[0] : existingNames);

            Editor.IsBusy = true;
            ProjectItem[] folders = await m_project.CreateFoldersAsync(new[] { parentFolder }, new[] { name });
            parentFolder.AddChild(folders[0]);
            AddItem(parentFolder, folders[0], existingNames);
            Editor.IsBusy = false;
        }

        protected virtual void DuplicateValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            ProjectItem projectItem = SelectedItems.First();
            if (projectItem == null || projectItem.Parent == null)
            {
                args.IsValid = false;
            }
        }

        protected async virtual void DuplicateContextMenuCmd(string arg)
        {
            await DuplicateSelectedItems();
        }

        protected virtual void DeleteFolderValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            ProjectItem projectItem = SelectedItems.First();
            if (projectItem == null || projectItem.Parent == null)
            {
                args.IsValid = false;
            }
        }

        protected virtual void DeleteFolderContextMenuCmd(string arg)
        {
            RemoveSelectedItems();
        }

        protected virtual void RenameValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            args.IsValid = (GetItemFlags(SelectedItems.First()) & HierarchicalDataItemFlags.CanEdit) != 0;
        }

        protected virtual void RenameFolderContextMenuCmd(string arg)
        {
            IsEditing = true;
        }

        #endregion

        #region Methods

        protected override bool AllowDropExternalObjects()
        {
            bool canCreatePrefab = CanCreatePrefab(TargetItem, ExternalDragObjects);
            bool canDrop = CanDrop(TargetItem, ExternalDragObjects);
            return canCreatePrefab || canDrop;
        }

        public void UpdateProjectItem(ProjectItem item)
        {
            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.Reset(item));
        }

        protected virtual bool CanRename(ProjectItem projectItem, string newName)
        {
            bool result = false;

            if (projectItem.Parent != null)
            {
                ProjectItem parentItem = projectItem.Parent;
                string newNameExt = newName + projectItem.Ext;
                if (!string.IsNullOrEmpty(newName)
                    && ProjectItem.IsValidName(newName)
                    && !parentItem.Children.Any(p => p.NameExt.ToLower() == newNameExt.ToLower())
                    && newName != projectItem.Name)
                {
                    result = true;
                }
            }

            return result;
        }

        public virtual void ChangeParent(ProjectItem projectItem, ProjectItem oldParent)
        {
            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ParentChanged(oldParent, projectItem));
        }

        public void RemoveProjectItemsFromTree(ProjectItem[] projectItems)
        {
            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
                if (projectItem.IsFolder)
                {
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemRemoved(projectItem.Parent, projectItem));
                }
            }

            if (m_selectedItems != null && m_selectedItems.Except(projectItems).Count() != m_selectedItems.Count())
            {
                SelectedItems = m_selectedItems.Except(projectItems);
                if (SelectedItem == null)
                {
                    SelectedItem = GetChildren(null).FirstOrDefault();
                }
            }
        }

        protected virtual bool CanDisplay(ProjectItem projectItem)
        {
            return projectItem.IsFolder;
        }

        protected virtual bool CanCreatePrefab(ProjectItem dropTarget, IEnumerable<object> dragItems)
        {
            if (dropTarget == null || !dropTarget.IsFolder)
            {
                return false;
            }

            ExposeToEditor[] objects = dragItems.OfType<ExposeToEditor>().ToArray();
            if (objects.Length == 0)
            {
                return false;
            }

            return objects.All(o => o.CanCreatePrefab);
        }

        protected virtual void CreatePrefab(ProjectItem dropTarget, IEnumerable<object> dragItems)
        {
            ExposeToEditor dragObject = (ExposeToEditor)ExternalDragObjects.First();
            Editor.CreatePrefab(TargetItem, dragObject, null, result =>
            {

            });
        }

        protected virtual bool CanDrop(ProjectItem dropTarget, IEnumerable<object> dragItems)
        {
            if (dropTarget == null || !dropTarget.IsFolder)
            {
                return false;
            }

            if (dragItems == null)
            {
                return true;
            }

            ProjectItem[] dragProjectItems = dragItems.OfType<ProjectItem>().ToArray();
            if (dragProjectItems.Length == 0)
            {
                return false;
            }

            if (dropTarget.Children == null)
            {
                return true;
            }

            for (int i = 0; i < dragProjectItems.Length; ++i)
            {
                ProjectItem dragItem = dragProjectItems[i];
                if (dropTarget.IsDescendantOf(dragItem))
                {
                    return false;
                }

                if (dropTarget.Children.Any(childItem => childItem.NameExt == dragItem.NameExt))
                {
                    return false;
                }
            }
            return true;
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

            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(parentFolder, folder));

            if (existingNames.Length > 0)
            {
                int index = Array.IndexOf(existingNames.Union(new[] { folder.Name }).OrderBy(n => n).ToArray(), folder.Name);
                if (index > 0)
                {
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.NextSiblingsChanged(folder, parentFolder.Children.Where(c => c.IsFolder).OrderBy(c => c.Name).ElementAt(index - 1)));
                }
                else
                {
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.PrevSiblingsChanged(folder, parentFolder.Children.Where(c => c.IsFolder).OrderBy(c => c.Name).ElementAt(index + 1)));
                }
            }

            if (expand)
            {
                Expand(parentFolder);
            }

            if (select)
            {
                ScrollIntoView = true;
                SelectedItems = new[] { folder };
                ScrollIntoView = false;
            }
        }

        private async Task DuplicateSelectedItems()
        {
            await m_project.DuplicateAsync(SelectedItems.ToArray());
        }

        public virtual void RemoveSelectedItems()
        {
            if (SelectedItem != null)
            {
                ProjectItem[] projectItems = SelectedItems.ToArray();

                if (projectItems.Any(p => p.Parent == null))
                {
                    WindowManager.MessageBox(
                        Localization.GetString("ID_RTEditor_ProjectTreeView_UnableToRemove", "Unable to remove"),
                        Localization.GetString("ID_RTEditor_ProjectTreeView_UnableToRemoveRootFolder", "Unable to remove root folder"));
                }
                else
                {
                    WindowManager.Confirmation(
                        Localization.GetString("ID_RTEditor_ProjectTreeView_DeleteSelectedAssets", "Delete selected assets"),
                        Localization.GetString("ID_RTEditor_ProjectTreeView_YouCannotUndoThisAction", "You cannot undo this action"), (dialog, arg) =>
                        {
                            RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.RemoveSelected());
                            SelectedItem = m_project.State.RootFolder;
                        },
                    (dialog, arg) => { },
                        Localization.GetString("ID_RTEditor_ProjectTreeView_Btn_Delete", "Delete"),
                        Localization.GetString("ID_RTEditor_ProjectTreeView_Btn_Cancel", "Cancel"));
                }
            }
        }

        #endregion
    }
}
