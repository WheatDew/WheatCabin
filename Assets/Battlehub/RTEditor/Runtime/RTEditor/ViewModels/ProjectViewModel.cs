using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [DefaultExecutionOrder(1)]
    [Binding]
    public class ProjectViewModel : ViewModel
    {
        #region ProjectItemViewModel
        /// <summary>
        /// This class is never instantiated. 
        /// It is used in the Template to specify the binding properties of ProjectItem without modifying the ProjectItem itself.
        /// </summary>
        [Binding]
        internal class ProjectItemViewModel
        {
            [Binding]
            public string Name
            {
                get;
                set;
            }

            [Binding]
            public ProjectItem Self
            {
                get;
            }

            private ProjectItemViewModel() { Debug.Assert(false); }
        }
        #endregion

        [SerializeField]
        private ProjectTreeViewModel m_projectTree = null;
        [Binding]
        public ProjectTreeViewModel ProjectTree
        {
            get { return m_projectTree; }
        }

        [SerializeField]
        private ProjectFolderViewModel m_projectFolder = null;
        [Binding]
        public ProjectFolderViewModel ProjectFolder
        {
            get { return m_projectFolder; }
        }

        private bool m_tryToChangeSelectedFolder;
        private string m_filterText;
        [Binding]
        public string FilterText
        {
            get { return m_filterText; }
            set
            {
                if (m_filterText != value)
                {
                    m_tryToChangeSelectedFolder = !string.IsNullOrWhiteSpace(m_filterText) && string.IsNullOrWhiteSpace(value);
                    m_filterText = value;
                    RaisePropertyChanged(nameof(FilterText));
                    ApplyFilter(false);
                }
            }
        }


        private IProjectAsync m_project;
        protected IProjectAsync Project
        {
            get { return m_project; }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (m_isStarted)
            {
                Init();
            }
        }

        private bool m_isStarted;
        protected override void Start()
        {
            base.Start();
            m_isStarted = true;

            if (m_projectTree == null)
            {
                m_projectTree = GetComponentInChildren<ProjectTreeViewModel>();
            }

            if (m_projectFolder == null)
            {
                m_projectFolder = GetComponentInChildren<ProjectFolderViewModel>();
            }

            Init();

            if (m_project.State.IsOpened)
            {
                OnOpenProjectCompleted();
                m_projectTree.RootFolder = m_project.State.RootFolder;
                m_projectTree.SelectedItem = m_project.State.RootFolder;
            }
            ApplyFilter(false);
        }

        private void Init()
        {
            m_project = IOC.Resolve<IProjectAsync>();
            if (m_project == null)
            {
                Debug.LogWarning("RTSLDeps.Project is null");
                Destroy(gameObject);
                return;
            }

            if (m_projectTree != null)
            {
                m_projectTree.SelectionChanged += OnProjectTreeSelectionChanged;
                m_projectTree.ItemRenamed += OnProjectTreeItemRenamed;
                m_projectTree.ItemsRemoved += OnProjectTreeItemsRemoved;
            }

            if (m_projectFolder != null)
            {
                m_projectFolder.SelectionChanged += OnProjectFolderSelectionChanged;
                m_projectFolder.ItemRenamed += OnProjectFolderItemRenamed;
                m_projectFolder.ItemsRemoved += OnProjectFolderItemsRemoved;
                m_projectFolder.ItemOpen += OnProjectFolderOpen;
            }

            m_project.Events.OpenProjectCompleted += OnOpenProjectCompleted;
            m_project.Events.CloseProjectCompleted += OnCloseProjectCompleted;
            m_project.Events.ImportCompleted += OnImportCompleted;
            m_project.Events.DeleteCompleted += OnDeleteCompleted;
            m_project.Events.CreateFoldersCompleted += OnCreateFoldersCompleted;
            m_project.Events.MoveCompleted += OnMoveCompleted;
            m_project.Events.SaveCompleted += OnSaveCompleted;
            m_project.Events.DuplicateCompleted += OnDuplicateCompleted;

            if (m_projectTree.RootFolder != null)
            {
                if (m_projectTree.SelectedItem == null || m_project.State.RootFolder.Get(m_projectTree.SelectedItem.ToString()) == null)
                {
                    m_projectTree.SelectedItem = m_project.State.RootFolder;
                }

                ApplyFilter(false);
            }
        }

        protected override void OnDisable()
        {
            if (m_projectTree != null)
            {
                m_projectTree.SelectionChanged -= OnProjectTreeSelectionChanged;
                m_projectTree.ItemRenamed -= OnProjectTreeItemRenamed;
                m_projectTree.ItemsRemoved -= OnProjectTreeItemsRemoved;
            }

            if (m_projectFolder != null)
            {
                m_projectFolder.SelectionChanged -= OnProjectFolderSelectionChanged;
                m_projectFolder.ItemRenamed -= OnProjectFolderItemRenamed;
                m_projectFolder.ItemsRemoved -= OnProjectFolderItemsRemoved;
                m_projectFolder.ItemOpen -= OnProjectFolderOpen;
            }

            if (m_project != null)
            {
                m_project.Events.OpenProjectCompleted -= OnOpenProjectCompleted;
                m_project.Events.CloseProjectCompleted -= OnCloseProjectCompleted;
                m_project.Events.ImportCompleted -= OnImportCompleted;
                m_project.Events.DeleteCompleted -= OnDeleteCompleted;
                m_project.Events.CreateFoldersCompleted -= OnCreateFoldersCompleted;
                m_project.Events.MoveCompleted -= OnMoveCompleted;
                m_project.Events.SaveCompleted -= OnSaveCompleted;
                m_project.Events.DuplicateCompleted -= OnDuplicateCompleted;
            }

            m_project = null;

            base.OnDisable();
        }

        #region Project Tree Event Handlers
        protected virtual void OnProjectTreeSelectionChanged(IEnumerable<ProjectItem> unslectedItems, IEnumerable<ProjectItem> selectedItems)
        {
            bool clear = selectedItems != null && selectedItems.Count() == 1 && (selectedItems.First().Children == null || selectedItems.First().Children.Count == 0);

            if (FilterText != null)
            {
                m_filterText = null;
                m_tryToChangeSelectedFolder = false;
                RaisePropertyChanged(nameof(FilterText));
                ApplyFilter(clear);
            }
            else
            {
                ApplyFilter(clear);
            }
        }

        protected virtual async void OnProjectTreeItemRenamed(ProjectItem projectItem, string oldName)
        {
            Editor.IsBusy = true;

            string newName = projectItem.Name;
            projectItem.Name = oldName;

            await m_project.Safe.RenameAsync(new[] { projectItem }, new[] { newName });

            Editor.IsBusy = false;
        }

        protected virtual void OnProjectTreeItemsRemoved(IEnumerable<ProjectItem> projectItems)
        {
            Editor.DeleteAssets(projectItems.ToArray());
        }

        #endregion

        #region Project Folder Event Handlers
        protected virtual async void OnProjectFolderSelectionChanged(IEnumerable<ProjectItem> unslectedItems, IEnumerable<ProjectItem> selectedItems)
        {
            if (m_projectFolder.SelectedItems == null)
            {
                Selection.activeObject = null;
            }
            else
            {
                ProjectItem[] assetItems = m_projectFolder.SelectedItems.Where(o => !o.IsFolder && !m_project.Utils.IsScene(o)).ToArray();
                if (assetItems.Length == 0)
                {
                    Selection.activeObject = null;
                }
                else
                {
                    Editor.IsBusy = true;
                    Object[] objects = await m_project.Safe.LoadAsync(assetItems);
                    Editor.IsBusy = false;

                    m_projectFolder.HandleEditorSelectionChange = false;
                    Selection.objects = objects;
                    m_projectFolder.HandleEditorSelectionChange = true;
                }
            }
        }

        protected virtual async void OnProjectFolderItemRenamed(ProjectItem projectItem, string oldName)
        {
            m_projectTree.UpdateProjectItem(projectItem);
            Editor.IsBusy = true;

            string newName = projectItem.Name;
            projectItem.Name = oldName;

            await m_project.Safe.RenameAsync(new[] { projectItem }, new[] { newName });

            Editor.IsBusy = false;
        }

        protected virtual void OnProjectFolderItemsRemoved(IEnumerable<ProjectItem> projectItems)
        {
            Editor.DeleteAssets(projectItems.ToArray());
        }

        protected virtual async void OnProjectFolderOpen(ProjectItem projectItem)
        {
            if (projectItem == null)
            {
                return;
            }

            if (projectItem.IsFolder)
            {
                m_projectTree.SelectedItem = projectItem;
            }
            else
            {
                if (m_project.Utils.IsScene(projectItem))
                {
                    Editor.IsPlaying = false;
                    Editor.IsBusy = true;
                    try
                    {
                        await m_project.Safe.LoadAsync(new[] { projectItem });
                    }
                    catch (System.Exception exc)
                    {
                        WindowManager.MessageBox(Localization.GetString("ID_RTEditor_ProjectView_UnableToLoadScene", "Unable to load scene") + " " + projectItem.ToString(), exc.Message);
                        Debug.LogError(exc);
                    }
                    finally
                    {
                        Editor.IsBusy = false;
                    }
                }
            }
        }


        #endregion

        #region Project Event Handlers
        protected virtual void OnOpenProjectCompleted(object sender, ProjectEventArgs<ProjectInfo> e)
        {
            OnOpenProjectCompleted();
        }

        private void OnOpenProjectCompleted()
        {
            m_projectTree.RootFolder = m_project.State.RootFolder;
            m_projectTree.SelectedItem = null;
            m_projectTree.SelectedItem = m_project.State.RootFolder;
        }

        protected virtual void OnCloseProjectCompleted(object sender, ProjectEventArgs<string> e)
        {
            m_projectTree.RootFolder = null;
            m_projectTree.SelectedItem = null;
            m_projectFolder.SetItems(null, null, true);
        }
        protected virtual void OnImportCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            string path = string.Empty;
            if (m_projectTree.SelectedItem != null)
            {
                path = m_projectTree.SelectedItem.ToString();
            }

            //m_projectTree.RootFolder = m_project.State.RootFolder;
            m_projectTree.BindData();

            if (!string.IsNullOrEmpty(path))
            {
                if (m_projectTree.SelectedItem == m_project.State.RootFolder)
                {
                    m_projectTree.SelectedItem = null;
                }

                m_projectTree.SelectedItem = m_project.State.RootFolder.Get(path);
            }
            else
            {
                m_projectTree.SelectedItem = m_project.State.RootFolder;
            }
        }

        protected virtual void OnDeleteCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            m_projectTree.RemoveProjectItemsFromTree(e.Payload);
            //m_projectFolder.OnRemoved(e.Payload.Where(item => !item.IsFolder).ToArray());
            m_projectFolder.OnRemoved(e.Payload);

            if (Selection.activeObject != null)
            {
                object selectedObjectId = m_project.Utils.ToPersistentID(Selection.activeObject);
                if (e.Payload.Any(r => !r.IsFolder && m_project.Utils.ToPersistentID(r).Equals(selectedObjectId)))
                {
                    bool wasEnabled = Undo.Enabled;
                    Undo.Enabled = false;
                    Selection.activeObject = null;
                    Undo.Enabled = wasEnabled;
                }
            }
        }

        protected virtual void OnCreateFoldersCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            ProjectItem folder = e.Payload.Last();
            bool select = folder.Parent != m_projectTree.SelectedItem;
            foreach (ProjectItem item in e.Payload)
            {
                m_projectTree.AddItem(item.Parent, item, false, true);
                if (!select)
                {
                    m_projectFolder.InsertItems(new[] { item }, true);
                }
            }

            if (select)
            {
                m_projectTree.SelectedItem = e.Payload.Last();
            }
        }

        protected virtual void OnMoveCompleted(object sender, ProjectEventArgs<(ProjectItem[] OriginalParentItems, ProjectItem[] MovedItems)> e)
        {
            m_projectFolder.Remove(e.Payload.MovedItems);

            for (int i = 0; i < e.Payload.MovedItems.Length; ++i)
            {
                ProjectItem projectItem = e.Payload.MovedItems[i];
                ProjectItem oldParent = e.Payload.OriginalParentItems[i];
                if (projectItem.IsFolder)
                {
                    m_projectTree.ChangeParent(projectItem, oldParent);
                }
            }
        }

        protected virtual void OnSaveCompleted(object sender, ProjectEventArgs<(ProjectItem[] SavedItems, bool IsUserAction)> e)
        {
            if (!e.HasError)
            {
                m_projectFolder.InsertItems(e.Payload.SavedItems, e.Payload.IsUserAction);
            }
        }

        protected virtual void OnDuplicateCompleted(object sender, ProjectEventArgs<(ProjectItem[] OriginalItems, ProjectItem[] DuplicatedItems)> e)
        {
            m_projectFolder.InsertItems(e.Payload.DuplicatedItems, true);
            foreach (ProjectItem item in e.Payload.DuplicatedItems.Where(item => item.IsFolder))
            {
                m_projectTree.AddItem(item.Parent, item, false, false);
            }

            m_projectTree.SelectedItem = e.Payload.DuplicatedItems.FirstOrDefault();
            m_projectFolder.SelectedItems = e.Payload.DuplicatedItems;
        }

        #endregion

        #region Methods
        private void ApplyFilter(bool clearProjectFolder)
        {
            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                DataBind(m_project.State.RootFolder.Flatten(false, true), FilterText, clearProjectFolder);
            }
            else
            {
                if (m_tryToChangeSelectedFolder)
                {
                    ProjectItem selectedFolder = m_projectTree.SelectedItem;
                    IEnumerable<ProjectItem> selectedItems = m_projectFolder.SelectedItems;
                    if (selectedFolder != null && selectedItems != null && selectedItems.Any())
                    {
                        if (!selectedItems.Any(item => item.Parent == selectedFolder))
                        {
                            m_projectTree.SelectedItem = selectedItems.First().Parent;
                            return;
                        }
                    }
                }


                if (m_projectTree.SelectedItems != null)
                {
                    DataBind(m_projectTree.SelectedItems.ToArray(), FilterText, clearProjectFolder);
                }
            }
        }

        private async void DataBind(ProjectItem[] projectItems, string searchPattern, bool clear)
        {
            if (clear)
            {
                m_projectFolder.SetItems(new ProjectItem[0], new ProjectItem[0], true);
            }

            ProjectItem[] assetItems = await m_project.Safe.GetAssetItemsAsync(projectItems, searchPattern);
            IResourcePreviewUtility resourcePreviewUtil = IOC.Resolve<IResourcePreviewUtility>();
            if (resourcePreviewUtil != null)
            {
                StartCoroutine(resourcePreviewUtil.CoCreatePreviews(assetItems));
            }

            SetItems(projectItems, assetItems);
        }

        private void SetItems(ProjectItem[] projectItems, ProjectItem[] assets)
        {
            Editor.Selection.Enabled = false;
            m_projectFolder.SetItems(projectItems.ToArray(), assets, true);
            Editor.Selection.Enabled = true;
        }

        #endregion

    }
}
