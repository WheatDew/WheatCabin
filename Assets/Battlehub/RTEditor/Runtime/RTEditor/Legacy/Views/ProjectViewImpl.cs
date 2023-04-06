using UnityEngine;
using Battlehub.RTCommon;
using System.Linq;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.DockPanels;
using System.Collections;
using TMPro;
using Battlehub.UIControls;
using Battlehub.RTEditor.ViewModels;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*Obsolete*/]
    public class ProjectViewImpl : MonoBehaviour
    {
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

        private ProjectView m_projectView;
        protected RuntimeWindow Window
        {
            get { return m_projectView; }
        }

        private ProjectTreeViewImpl m_projectTree;
        protected ProjectTreeViewImpl ProjectTree
        {
            get { return m_projectTree; }
        }

        private ProjectFolderViewImpl m_projectFolder;
        protected ProjectFolderViewImpl ProjectFolder
        {
            get { return m_projectFolder; }
        }

        private TMP_InputField m_filterInput;
        protected TMP_InputField FilterInput
        {
            get { return m_filterInput; }
        }

        private IWindowManager m_windowManager;
        private ILocalization m_localization;
        private IResourcePreviewUtility m_resourcePreview;

        private string m_filter = string.Empty;
        private bool m_tryToChangeSelectedFolder;
        private float m_applyFilterTime;

        protected virtual void Start()
        {
            m_projectView = GetComponent<ProjectView>();
            m_projectTree = m_projectView.ProjectTree.GetComponent<ProjectTreeViewImpl>();
            m_projectFolder = m_projectView.ProjectFolder.GetComponent<ProjectFolderViewImpl>();
            m_filterInput = m_projectView.FilterInput;

            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_windowManager = IOC.Resolve<IWindowManager>();
            m_localization = IOC.Resolve<ILocalization>();
            m_project = IOC.Resolve<IProjectAsync>();
            if (m_project == null)
            {
                Debug.LogWarning("RTSLDeps.Get.Project is null");
                Destroy(gameObject);
                return;
            }

            m_resourcePreview = IOC.Resolve<IResourcePreviewUtility>();
            if(m_resourcePreview == null)
            {
                Debug.LogWarning("RTEDeps.Get.ResourcePreview is null");
            }

            DockPanel dockPanelsRoot = GetComponent<DockPanel>();
            if (dockPanelsRoot != null)
            {
                dockPanelsRoot.CursorHelper = Editor.CursorHelper;
            }

            UnityEventHelper.AddListener(m_filterInput, inputField => inputField.onValueChanged, OnFiltering);

            m_projectFolder.ItemDoubleClick += OnProjectFolderItemDoubleClick;
            m_projectFolder.ItemRenamed += OnProjectFolderItemRenamed;
            m_projectFolder.ItemsDeleted += OnProjectFolderItemDeleted;
            m_projectFolder.SelectionChanged += OnProjectFolderSelectionChanged;

            m_projectTree.SelectionChanged += OnProjectTreeSelectionChanged;
            m_projectTree.ItemRenamed += OnProjectTreeItemRenamed;
            m_projectTree.ItemsDeleted += OnProjectTreeItemDeleted;

            m_project.Events.OpenProjectCompleted += OnOpenProjectCompleted;
            m_project.Events.CloseProjectCompleted += OnCloseProjectCompleted;
            m_project.Events.ImportCompleted += OnImportCompleted;
            //BeforeDelteCompleted missing
            //m_project.Events.BeforeDeleteCompleted += OnBeforeDeleteCompleted;
            m_project.Events.DeleteCompleted += OnDeleteCompleted;
            m_project.Events.RenameCompleted += OnRenameCompleted;
            m_project.Events.CreateFoldersCompleted += OnCreateFoldersCompleted;
            m_project.Events.MoveCompleted += OnMoveCompleted;
            m_project.Events.SaveCompleted += OnSaveCompleted;
            m_project.Events.DuplicateCompleted += OnDuplicateCompleted;

            if (m_project.State.IsOpened)
            {
                m_projectTree.LoadProject(m_project.State.RootFolder);
                StartCoroutine(CoSetSelectedItem());
            }
        }

        private IEnumerator CoSetSelectedItem()
        {
            yield return new WaitForEndOfFrame();
            m_projectTree.SelectedItem = m_project.State.RootFolder;
        }

        protected virtual void OnDestroy()
        {
            if (m_projectFolder != null)
            {
                m_projectFolder.ItemDoubleClick -= OnProjectFolderItemDoubleClick;
                m_projectFolder.ItemRenamed -= OnProjectFolderItemRenamed;
                m_projectFolder.ItemsDeleted -= OnProjectFolderItemDeleted;
                m_projectFolder.SelectionChanged -= OnProjectFolderSelectionChanged;
            }

            if(m_projectTree != null)
            {
                m_projectTree.SelectionChanged -= OnProjectTreeSelectionChanged;
                m_projectTree.ItemsDeleted -= OnProjectTreeItemDeleted;
                m_projectTree.ItemRenamed -= OnProjectTreeItemRenamed;
            }

            if (m_project != null)
            {
                m_project.Events.OpenProjectCompleted -= OnOpenProjectCompleted;
                m_project.Events.CloseProjectCompleted -= OnCloseProjectCompleted;
                m_project.Events.ImportCompleted -= OnImportCompleted;
                //BeforeDelteCompleted missing
                //m_project.Events.BeforeDeleteCompleted -= OnBeforeDeleteCompleted;
                m_project.Events.DeleteCompleted -= OnDeleteCompleted;
                m_project.Events.RenameCompleted -= OnRenameCompleted;
                m_project.Events.CreateFoldersCompleted -= OnCreateFoldersCompleted;
                m_project.Events.MoveCompleted -= OnMoveCompleted;
                m_project.Events.SaveCompleted -= OnSaveCompleted;
                m_project.Events.DuplicateCompleted -= OnDuplicateCompleted;
            }

            UnityEventHelper.RemoveListener(m_filterInput, inputField => inputField.onValueChanged, OnFiltering);
        }

        protected virtual void Update()
        {
            if (Time.time > m_applyFilterTime)
            {
                m_applyFilterTime = float.PositiveInfinity;
                ApplyFilter();
            }
        }

        private void OnOpenProjectCompleted(object sender, ProjectEventArgs<ProjectInfo> e)
        {
            m_projectTree.LoadProject(m_project.State.RootFolder);
            m_projectTree.SelectedItem = null;
            m_projectTree.SelectedItem = m_project.State.RootFolder;
        }

        private void OnCloseProjectCompleted(object sender, ProjectEventArgs<string> e)
        {
            m_projectTree.LoadProject(null);
            m_projectTree.SelectedItem = null;
            m_projectFolder.SetItems(null, null, true);
        }

        private void OnImportCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            string path = string.Empty;
            if (m_projectTree.SelectedItem != null)
            {
                path = m_projectTree.SelectedItem.ToString();
            }

            m_projectTree.LoadProject(m_project.State.RootFolder);

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

        private void OnDeleteCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            m_projectTree.RemoveProjectItemsFromTree(e.Payload);
            m_projectTree.SelectRootIfNothingSelected();
            m_projectFolder.OnDeleted(e.Payload.Where(item => !item.IsFolder).ToArray());

            if (Editor.Selection.activeObject != null)
            {
                object selectedObjectId = m_project.Utils.ToPersistentID(Editor.Selection.activeObject);
                if (e.Payload.Any(r => !r.IsFolder && m_project.Utils.ToPersistentID(r).Equals(selectedObjectId)))
                {
                    bool wasEnabled = Editor.Undo.Enabled;
                    Editor.Undo.Enabled = false;
                    Editor.Selection.activeObject = null;
                    Editor.Undo.Enabled = wasEnabled;
                }
            }
        }

        private void OnRenameCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {   
        }

        private void OnCreateFoldersCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            foreach (ProjectItem item in e.Payload)
            {
                m_projectTree.AddItem(item.Parent, item);
            }
            m_projectTree.SelectedItem = e.Payload.Last();
        }

        private void OnMoveCompleted(object sender, ProjectEventArgs<(ProjectItem[] OriginalParentItems, ProjectItem[] MovedItems)> e)
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

        private void OnSaveCompleted(object sender, ProjectEventArgs<(ProjectItem[] SavedItems, bool IsUserAction)> e)
        {
            if(!e.HasError)
            {
                m_projectFolder.InsertItems(e.Payload.SavedItems, e.Payload.IsUserAction);
            }
        }

        private void OnDuplicateCompleted(object sender, ProjectEventArgs<(ProjectItem[] OriginalItems, ProjectItem[] DuplicatedItems)> e)
        {
            m_projectFolder.InsertItems(e.Payload.DuplicatedItems, true);
            foreach (ProjectItem item in e.Payload.DuplicatedItems.Where(item => item.IsFolder))
            {
                m_projectTree.AddItem(item.Parent, item, false, false);
            }

            m_projectTree.SelectedItem = e.Payload.DuplicatedItems.FirstOrDefault();
        }

        private void OnProjectTreeSelectionChanged(object sender, SelectionChangedArgs<ProjectItem> e)
        {
            if (m_filterInput != null)
            {
                m_filterInput.SetTextWithoutNotify(string.Empty);
            }
            m_filter = string.Empty;
            m_tryToChangeSelectedFolder = false;
            ApplyFilter();
        }

        private async void DataBind(ProjectItem[] projectItems, string searchPattern)
        {
            ProjectItem[] assetItems = await m_project.Safe.GetAssetItemsAsync(projectItems, searchPattern);
            StartCoroutine(m_resourcePreview.CoCreatePreviews(assetItems));
            StartCoroutine(CoSetItems(projectItems, assetItems));
        }

        private void SetItems(ProjectItem[] projectItems, ProjectItem[] assets)
        {
            Editor.Selection.Enabled = false;
            m_projectFolder.SetItems(projectItems.ToArray(), assets, true);
            Editor.Selection.Enabled = true;
        }

        private IEnumerator CoSetItems(ProjectItem[] projectItems, ProjectItem[] assets)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            SetItems(projectItems, assets);
        }

        private void OnProjectFolderItemDeleted(object sender, ProjectTreeEventArgs e)
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            editor.DeleteAssets(e.ProjectItems);
        }

        private async void OnProjectFolderItemDoubleClick(object sender, ProjectTreeEventArgs e)
        {
            if(e.ProjectItem == null)
            {
                return;
            }

            if(e.ProjectItem.IsFolder)
            {
                m_projectTree.SelectedItem = e.ProjectItem;
            }
            else
            {
                if(m_project.Utils.IsScene(e.ProjectItem))
                {
                    Editor.IsPlaying = false;
                    Editor.IsBusy = true;
                    try
                    {
                        await m_project.Safe.LoadAsync(new[] { e.ProjectItem });
                    }
                    catch(System.Exception exc)
                    {
                        m_windowManager.MessageBox(m_localization.GetString("ID_RTEditor_ProjectView_UnableToLoadScene", "Unable to load scene") + " " + e.ProjectItem.ToString(), exc.Message);
                        Debug.LogError(exc);
                    }
                    finally
                    {
                        Editor.IsBusy = false;
                    }
                }
            }
        }

        private async void OnProjectFolderItemRenamed(object sender, ProjectTreeRenamedEventArgs e)
        {
            m_projectTree.UpdateProjectItem(e.ProjectItem);            
            Editor.IsBusy = true;

            string newName = e.ProjectItem.Name;
            e.ProjectItem.Name = e.OldName;

            await m_project.Safe.RenameAsync(new[] { e.ProjectItem }, new[] { newName });

            Editor.IsBusy = false;
        }

        private async void OnProjectFolderSelectionChanged(object sender, ProjectTreeEventArgs e)
        {
            if(m_projectFolder.SelectedItems == null)
            {
                Editor.Selection.activeObject = null;
            }
            else
            {
                ProjectItem[] assetItems = m_projectFolder.SelectedItems.Where(o => !o.IsFolder && !m_project.Utils.IsScene(o)).ToArray();
                if (assetItems.Length == 0)
                {
                    Editor.Selection.activeObject = null;
                }
                else
                {
                    Editor.IsBusy = true;
                    Object[] objects = await m_project.Safe.LoadAsync(assetItems);
                    Editor.IsBusy = false;

                    m_projectFolder.HandleEditorSelectionChange = false;
                    Editor.Selection.objects = objects;
                    m_projectFolder.HandleEditorSelectionChange = true;

                }
            }
        }

        private async void OnProjectTreeItemRenamed(object sender, ProjectTreeRenamedEventArgs e)
        {
            Editor.IsBusy = true;
            
            string newName = e.ProjectItem.Name;
            e.ProjectItem.Name = e.OldName;

            await m_project.Safe.RenameAsync(new[] { e.ProjectItem }, new[] { newName });
            Editor.IsBusy = false;
        }

        private void OnProjectTreeItemDeleted(object sender, ProjectTreeEventArgs e)
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            editor.DeleteAssets(e.ProjectItems);
        }

        private void OnFiltering(string value)
        {
            m_tryToChangeSelectedFolder = !string.IsNullOrWhiteSpace(m_filter) && string.IsNullOrWhiteSpace(value);
            m_filter = value;
            m_applyFilterTime = Time.time + 0.3f;
        }

        private void ApplyFilter()
        {
            if (!string.IsNullOrWhiteSpace(m_filter))
            {
                DataBind(m_project.State.RootFolder.Flatten(false, true), m_filter);
            }
            else
            {
                if(m_tryToChangeSelectedFolder)
                {
                    ProjectItem selectedFolder = m_projectTree.SelectedItem;
                    ProjectItem[] selectedItems = m_projectFolder.SelectedItems;
                    if (selectedFolder != null && selectedItems != null && selectedItems.Length > 0)
                    {
                        if (!selectedItems.Any(item => item.Parent == selectedFolder))
                        {
                            m_projectTree.SelectedItem = selectedItems[0].Parent;
                            return;
                        }
                    }
                }

                DataBind(m_projectTree.SelectedFolders, m_filter);
            }
        }
    }
}
