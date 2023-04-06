using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.ViewModels
{
    public interface IProjectFolderViewModel
    {
        event EventHandler<ContextMenuEventArgs> ContextMenuOpened;
        event Action<ProjectItem> ItemOpen;

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

        ProjectItem[] Folders
        {
            get;
        }
    }

    [Binding]
    public class ProjectFolderViewModel : HierarchicalDataViewModel<ProjectItem>, IProjectFolderViewModel
    {
        public event Action<IEnumerable<ProjectItem>, IEnumerable<ProjectItem>> SelectionChanged;
        public event Action<IEnumerable<ProjectItem>> ItemsRemoved;
        public event Action<ProjectItem> ItemOpen;
        public event Action<ProjectItem, string> ItemRenamed;
        
        public bool HandleEditorSelectionChange
        {
            get;
            set;
        }


        private bool m_raiseSelectionChange = true;
        protected bool RaiseSelectionChange
        {
            get { return m_raiseSelectionChange; }
            set { m_raiseSelectionChange = value; }
        }

        private bool m_raiseItemDeleted = true;
        protected bool RaiseItemDeleted
        {
            get { return m_raiseItemDeleted; }
            set { m_raiseItemDeleted = value; }
        }

        private Dictionary<object, ProjectItem> m_idToItem = new Dictionary<object, ProjectItem>();
        protected Dictionary<object, ProjectItem> IdToItem
        {
            get { return m_idToItem; }
        }

        private List<ProjectItem> m_items;
        protected List<ProjectItem> Items
        {
            get { return m_items; }
        }

        private ProjectItem[] m_folders;
        public ProjectItem[] Folders
        {
            get { return m_folders; }
        }

        private IProjectAsync m_project;
        protected IProjectAsync Project
        {
            get { return m_project; }
        }

        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<IProjectFolderViewModel>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            IOC.UnregisterFallback<IProjectFolderViewModel>(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_project = IOC.Resolve<IProjectAsync>();

            Selection.SelectionChanged += EditorSelectionChanged;
            Editor.Object.NameChanged += OnObjectNameChanged;
        }

        protected override void OnDisable()
        {
            m_project = null;

            if(Selection != null)
            {
                Selection.SelectionChanged -= EditorSelectionChanged;
            }

            if(Editor.Object != null)
            {
                Editor.Object.NameChanged -= OnObjectNameChanged;
            }
            
            base.OnDisable();
        }

        #region Editor EventHandlers
        protected virtual void EditorSelectionChanged(UnityObject[] unselectedObjects)
        {
            EditorSelectionToSelectedObjects();
        }

        protected virtual  void OnObjectNameChanged(ExposeToEditor obj)
        {
            ProjectItem projectItem = m_project.Utils.ToProjectItem(obj);
            if (projectItem == null)
            {
                return;
            }

            if (CanRename(projectItem, obj.name))
            {
                projectItem.Name = obj.name;
            }
            else
            {
                obj.Name = projectItem.Name;
            }
        }

        #endregion

        #region IHierarchicalData
        public override HierarchicalDataFlags GetFlags()
        {
            HierarchicalDataFlags flags = HierarchicalDataFlags.Default;

            flags &= ~HierarchicalDataFlags.CanReorder;
            flags &= ~HierarchicalDataFlags.CanSelectAll;
            flags &= ~HierarchicalDataFlags.CanRemove;

            return flags;
        }

        public override HierarchicalDataItemFlags GetItemFlags(ProjectItem item)
        {
            return HierarchicalDataItemFlags.Default;
        }

        public override IEnumerable<ProjectItem> GetChildren(ProjectItem parent)
        {
            return m_items != null ? m_items.ToList() : null;
        }

        #endregion

        #region Bound UnityEvent Handlers

        protected override void OnSelectedItemsChanged(IEnumerable<ProjectItem> unselectedObjects, IEnumerable<ProjectItem> selectedObjects)
        {
            if (!RaiseSelectionChange)
            {
                return;
            }

            SelectionChanged?.Invoke(unselectedObjects, selectedObjects);
        }

        public override void OnExternalObjectDrop()
        {
            if (!AllowDropExternalObjects())
            {
                CanDropExternalObjects = false;
                return;
            }

            ProjectItem targetItem = TargetItem;
            if (targetItem == null)
            {
                targetItem = Folders[0];
            }


            ExposeToEditor dragObject = ExternalDragObjects.First() as ExposeToEditor;
            if (dragObject != null && dragObject.CanCreatePrefab)
            {
                Editor.CreatePrefab(targetItem, dragObject, null, assetItem => { });
            }
           
            CanDropExternalObjects = false;
        }

        public override void OnItemDragEnter()
        {
            if (TargetItem == null || !TargetItem.IsFolder || SourceItems == null ||
              SourceItems.Contains(TargetItem) || FolderContainsItemWithSameName(TargetItem, SourceItems))
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
                await m_project.MoveAsync(SourceItems.ToArray(), TargetItem);
                Editor.IsBusy = false;
            }
            
        }

        public override void OnItemsEndDrag()
        {
            Remove(SourceItems);
        }

        public override void OnItemsRemoved()
        {
            foreach(ProjectItem item in SourceItems)
            {
                m_items.Remove(item);

                if (!item.IsFolder)
                {
                    m_idToItem.Remove(m_project.Utils.ToPersistentID(item));
                }

            }

            if (RaiseItemDeleted)
            {
                ItemsRemoved?.Invoke(SourceItems);
            }
        }

        public override void OnItemDoubleClick()
        {
            ItemOpen?.Invoke(TargetItem);
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
            Rename(TargetItem, newName);
        }

        public override void OnSelectAll()
        {
            SelectedItems = GetChildren(null);
        }

        public override async void OnDuplicate()
        {
            await Duplicate();
        }

        public override void OnDelete()
        {
            DeleteSelectedItems();
        }

        #endregion

        #region Context Menu
    
        protected override void OnContextMenu(List<MenuItemViewModel> menuItems)
        {
            if(TargetItem == null)
            {
                MenuItemViewModel createFolder = new MenuItemViewModel
                {
                    Path = string.Format("{0}/{1}",
                        Localization.GetString("ID_RTEditor_ProjectFolderView_Create", "Create"),
                        Localization.GetString("ID_RTEditor_ProjectFolderView_Folder", "Folder"))
                };
                createFolder.Command = "CurrentFolder";
                createFolder.Action = CreateFolderContextMenuCmd;
                menuItems.Add(createFolder);

                string materialStr = Localization.GetString("ID_RTEditor_ProjectFolderView_Material", "Material");
                string animationClipStr = Localization.GetString("ID_RTEditor_ProjectFolderView_AnimationClip", "Animation Clip");
                CreateMenuItem(materialStr, materialStr, typeof(Material), menuItems);
                CreateMenuItem(animationClipStr, animationClipStr.Replace(" ", ""), typeof(RuntimeAnimationClip), menuItems);
            }
            else
            {
                MenuItemViewModel createFolder = new MenuItemViewModel
                {
                    Path = string.Format("{0}/{1}",
                    Localization.GetString("ID_RTEditor_ProjectFolderView_Create", "Create"),
                    Localization.GetString("ID_RTEditor_ProjectFolderView_Folder", "Folder"))
                };

                createFolder.Action = CreateFolderContextMenuCmd;
                createFolder.Validate = CreateValidateContextMenuCmd;
                menuItems.Add(createFolder);

                string materialStr = Localization.GetString("ID_RTEditor_ProjectFolderView_Material", "Material");
                string animationClipStr = Localization.GetString("ID_RTEditor_ProjectFolderView_AnimationClip", "Animation Clip");
                CreateMenuItem(materialStr, materialStr, typeof(Material), menuItems);
                CreateMenuItem(animationClipStr, animationClipStr.Replace(" ", ""), typeof(RuntimeAnimationClip), menuItems);

                MenuItemViewModel open = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_ProjectFolderView_Open", "Open") };
                open.Action = OpenContextMenuCmd;
                open.Validate = OpenValidateContextMenuCmd;
                menuItems.Add(open);

                MenuItemViewModel duplicate = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_ProjectFolderView_Duplicate", "Duplicate") };
                duplicate.Action = DuplicateContextMenuCmd;
                duplicate.Validate = DuplicateValidateContextMenuCmd;
                menuItems.Add(duplicate);

                MenuItemViewModel deleteFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_ProjectFolderView_Delete", "Delete") };
                deleteFolder.Action = DeleteContextMenuCmd;
                deleteFolder.Validate = DeleteValidateContextMenuCmd;
                menuItems.Add(deleteFolder);

                MenuItemViewModel renameFolder = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_ProjectFolderView_Rename", "Rename") };
                renameFolder.Action = RenameContextMenuCmd;
                renameFolder.Validate = RenameValidateContextMenuCmd;
                menuItems.Add(renameFolder);
            }
        }

        private void CreateMenuItem(string text, string defaultName, Type type, List<MenuItemViewModel> menuItems)
        {
            if (m_project.Utils.ToGuid(type) != Guid.Empty)
            {
                MenuItemViewModel createAsset = new MenuItemViewModel { Path = Localization.GetString("ID_RTEditor_ProjectFolderView_Create", "Create") + "/" + text };
                createAsset.Command = "CurrentFolder";
                createAsset.Action = arg => CreateAsset(arg, type, defaultName);
                createAsset.Validate = CreateValidateContextMenuCmd;
                menuItems.Add(createAsset);
            }
        }

        protected virtual void CreateValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            ProjectItem selectedItem = SelectedItem;
            if (selectedItem != null && !selectedItem.IsFolder)
            {
                args.IsValid = false;
            }
        }

        protected async virtual void CreateFolderContextMenuCmd(string arg)
        {
            if (m_folders == null)
            {
                return;
            }

            bool currentFolder = !string.IsNullOrEmpty(arg);

            ProjectItem parentFolder = currentFolder ? m_folders.FirstOrDefault() : SelectedItem;
            if (parentFolder == null)
            {
                return;
            }

            Editor.IsBusy = true;

            string[] existingNames = parentFolder.Children.Where(c => c.IsFolder).Select(c => c.Name).ToArray();
            string name = m_project.Utils.GetUniqueName(Localization.GetString("ID_RTEditor_ProjectFolderView_Folder", "Folder"), parentFolder.Children == null ? new string[0] : existingNames);
            ProjectItem[] folders = await m_project.Safe.CreateFoldersAsync(new[] { parentFolder }, new[] { name });
            parentFolder.AddChild(folders[0]);

            if (currentFolder)
            {
                InsertItems(new[] { folders[0] }, true);
            }

            Editor.IsBusy = false;
        }

        protected virtual void OpenValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            ProjectItem selectedItem = SelectedItem;
            ProjectTreeCancelEventArgs cancelArgs = new ProjectTreeCancelEventArgs(new[] { selectedItem });

            if (SelectedItems.Count() != 1 || !selectedItem.IsFolder && !m_project.Utils.IsScene(selectedItem))
            {
                cancelArgs.Cancel = true;
            }

            args.IsValid = !cancelArgs.Cancel;
        }

        protected virtual void OpenContextMenuCmd(string arg)
        {
            ItemOpen?.Invoke(SelectedItem);
        }

        protected virtual void DuplicateValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            ProjectItem projectItem = SelectedItem;

            if (projectItem == null || projectItem.Parent == null)
            {
                args.IsValid = false;
            }
        }

        protected async virtual void DuplicateContextMenuCmd(string arg)
        {
            await Duplicate();
        }

        protected virtual void DeleteValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
            ProjectItem projectItem = SelectedItem;

            if (projectItem == null || projectItem.Parent == null)
            {
                args.IsValid = false;
            }
        }

        protected virtual void DeleteContextMenuCmd(string arg)
        {
            DeleteSelectedItems();
        }

        protected virtual void RenameValidateContextMenuCmd(MenuItemViewModel.ValidationArgs args)
        {
        }

        protected virtual void RenameContextMenuCmd(string arg)
        {
            IsEditing = true;
        }


        #endregion

        #region Methods
        protected override bool AllowDropExternalObjects()
        {
            ProjectItem targetItem = TargetItem;
            if(targetItem == null)
            {
                if (Folders.Length > 1)
                {
                    return false;
                }
                targetItem = Folders[0];
            }

            if (!targetItem.IsFolder)
            {
                return false;
            }

            IEnumerable<ProjectItem> projectItems = ExternalDragObjects.OfType<ProjectItem>();
            if(projectItems.Contains(targetItem))
            {
                return false;
            }

            if (FolderContainsItemWithSameName(targetItem, projectItems))
            {
                return false;
            }

            bool result = projectItems.Any() || CanCreatePrefab(targetItem, ExternalDragObjects);
            return result;
        }

        public virtual void DeleteSelectedItems()
        {
            WindowManager.Confirmation(
                Localization.GetString("ID_RTEditor_ProjectFolderView_DeleteSelectedAssets", "Delete Selected Assets"),
                Localization.GetString("ID_RTEditor_ProjectFolderView_YouCanNotUndoThisAction", "You cannot undo this action"),
                (sender, arg) =>
                {
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.RemoveSelected());

                    bool wasEnabled = Editor.Undo.Enabled;
                    Undo.Enabled = false;
                    Selection.objects = null;
                    Undo.Enabled = wasEnabled;
                },
            (sender, arg) => { },
            Localization.GetString("ID_RTEditor_ProjectFolderView_BtnDelete", "Delete"),
            Localization.GetString("ID_RTEditor_ProjectFolderView_BtnCancel", "Cancel"));
        }

        private async Task Duplicate()
        {
            ProjectItem[] projectItems = SelectedItems.OfType<ProjectItem>().ToArray();
            Editor.IsBusy = true;
            await m_project.DuplicateAsync(projectItems);
            Editor.IsBusy = false;
        }

        private void CreateAsset(string arg, Type type, string defaultName)
        {
            if (m_folders == null)
            {
                return;
            }

            bool currentFolder = !string.IsNullOrEmpty(arg);

            ProjectItem parentFolder = currentFolder ? m_folders.FirstOrDefault() : SelectedItem;
            if (parentFolder == null)
            {
                return;
            }
            CreateAsset(type, defaultName, parentFolder);
        }

        private void CreateAsset(Type type, string defaultName, ProjectItem parentFolder)
        {
            IUnityObjectFactory objectFactory = IOC.Resolve<IUnityObjectFactory>();
            UnityObject asset = objectFactory.CreateInstance(type, null);
            asset.name = defaultName;

            CreateAsset(asset, parentFolder);
        }

        public async void CreateAsset(UnityObject asset, ProjectItem parentFolder)
        {
            if (m_folders == null)
            {
                return;
            }

            IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();
            byte[] preview = resourcePreview.CreatePreviewData(asset);
            Editor.IsBusy = true;
            ProjectItem[] assetItems = await m_project.Safe.SaveAsync(new[] { parentFolder }, new[] { preview }, new[] { asset }, null);
            Editor.IsBusy = false;

            if (parentFolder != m_folders.FirstOrDefault())
            {
                ItemOpen?.Invoke(parentFolder);
            }

            Destroy(asset);
        }


        public virtual void InsertItems(ProjectItem[] items, bool selectAndScrollIntoView)
        {
            if (m_folders == null)
            {
                return;
            }

            items = items.Where(item => m_folders.Contains(item.Parent) && CanDisplay(item)).ToArray();
            if (items.Length == 0)
            {
                return;
            }

            m_items = m_items.Union(items).ToList();
            List<ProjectItem> sorted = m_items.Where(item => item.IsFolder).OrderBy(item => item.Name).Union(m_items.Where(item => !item.IsFolder).OrderBy(item => item.Name)).ToList();
            ProjectItem selectItem = null;
            for (int i = 0; i < sorted.Count; ++i)
            {
                if (items.Contains(sorted[i]))
                {
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemInserted(i, sorted[i]));
                    selectItem = sorted[i];
                }
                else
                {
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.Reset(sorted[i]));
                }

                if (!sorted[i].IsFolder && !m_idToItem.ContainsKey(m_project.Utils.ToPersistentID(sorted[i])))
                {
                    m_idToItem.Add(m_project.Utils.ToPersistentID(sorted[i]), sorted[i]);
                }
            }
            m_items = sorted;

            if (selectItem != null)
            {
                if (selectAndScrollIntoView)
                {
                    ScrollIntoView = true;
                    SelectedItem = selectItem;
                    ScrollIntoView = false;
                }
            }
        }

        public virtual void Rename(ProjectItem projectItem, string newName)
        {
            if(CanRename(projectItem, newName))
            {
                string oldName = projectItem.Name;
                projectItem.Name = newName;
                if(oldName != newName)
                {
                    ItemRenamed?.Invoke(projectItem, oldName);
                }
            }
        }

        public virtual void OnRemoved(ProjectItem[] projectItems)
        {
            for (int i = 0; i < projectItems.Length; ++i)
            {
                m_items.Remove(projectItems[i]);
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemRemoved(null, projectItems[i]));
            }
        }

        public virtual void Remove(IEnumerable<ProjectItem> items)
        {
            if(m_folders == null)
            {
                return;
            }

            RaiseItemDeleted = false;
            foreach (ProjectItem item in items)
            {
                m_items.Remove(item);
                if (m_folders.All(f => f.Children == null || !f.Children.Contains(item)))
                {
                    RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemRemoved(item.Parent, item));   
                }
            }
            RaiseItemDeleted = true;
        }

        public virtual void SetItems(ProjectItem[] folders, ProjectItem[] items, bool reload)
        {
            if (folders == null || items == null)
            {
                m_folders = null;
                m_items = null;
                m_idToItem = new Dictionary<object, ProjectItem>();
            }
            else
            {
                m_folders = folders;
                m_items = new List<ProjectItem>(items.Where(item => CanDisplay(item)));
                m_idToItem = m_items.Where(item => !item.IsFolder).ToDictionary(item => m_project.Utils.ToPersistentID(item));
                if (m_items != null)
                {
                    m_items = m_items.Where(item => item.IsFolder).OrderBy(item => item.Name).Union(m_items.Where(item => !item.IsFolder).OrderBy(item => item.Name)).ToList();
                }
                BindData(reload);
                EditorSelectionChanged(null);
            }
        }

        private void BindData(bool clearItems)
        {
            if (m_items == null)
            {
                SelectedItems = null;
            }
            else
            {
                
                if (clearItems)
                {
                    List<ProjectItem> items = m_items;
                    m_items = null;
                    BindData();
                    m_items = items;
                }

                SelectedItems = null;
            }

            BindData();
        }

        protected virtual bool CanDisplay(ProjectItem projectItem)
        {
            return true;
        }

        protected virtual void EditorSelectionToSelectedObjects()
        {
            if (!HandleEditorSelectionChange)
            {
                return;
            }

            RaiseSelectionChange = false;

            UnityObject[] selectedObjects = Selection.objects;
            if (selectedObjects != null)
            {
                List<ProjectItem> selectedItems = new List<ProjectItem>();
                for (int i = 0; i < selectedObjects.Length; ++i)
                {
                    UnityObject selectedObject = selectedObjects[i];
                    object id = m_project.Utils.ToPersistentID(selectedObject);
                    if (m_idToItem.ContainsKey(id))
                    {
                        ProjectItem item = m_idToItem[id];
                        if (item != null)
                        {
                            selectedItems.Add(item);
                        }
                    }
                }
                if (selectedItems.Count > 0)
                {
                    SelectedItems = selectedItems;
                }
                else if(SelectedItem != null && SelectedItems.Any(item => !m_project.Utils.IsUnityObject(item)))
                {
                    SelectedItems = SelectedItems.Where(item => !m_project.Utils.IsUnityObject(item));
                }
            }
            else if (SelectedItem != null && SelectedItems.Any(item => !m_project.Utils.IsUnityObject(item)))
            {
                SelectedItems = SelectedItems.Where(item => !m_project.Utils.IsUnityObject(item));
            }

            RaiseSelectionChange = true;
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

        protected virtual bool CanCreatePrefab(ProjectItem projectItem, IEnumerable<object> dragObjects)
        {
            ExposeToEditor[] objects = dragObjects.OfType<ExposeToEditor>().ToArray();
            if (objects.Length == 0)
            {
                return false;
            }
            return objects.All(o => o.CanCreatePrefab);
        }

        protected virtual bool FolderContainsItemWithSameName(ProjectItem folder, IEnumerable<ProjectItem> dragItems)
        {
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

        #endregion
    }
}
