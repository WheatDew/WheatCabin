using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.MenuControl;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene33
{
    /// <summary>
    /// This is example on how to create, save, load, delete assets and folders using IProjectAsync interface
    /// </summary>
    [MenuDefinition]
    public class FolderAndAssetExampleMenu : EditorExtension
    {
        private IProjectAsync m_project;
        private IWindowManager m_wm;

        protected override void OnInit()
        {
            base.OnInit();

            m_wm = IOC.Resolve<IWindowManager>();

            m_project = IOC.Resolve<IProjectAsync>();
            m_project.Events.BeginSave += OnBeginSave;
            m_project.Events.SaveCompleted += OnSaveCompleted;
            m_project.Events.BeginLoad += OnBeginLoad;
            m_project.Events.LoadCompleted += OnLoadCompleted;
            m_project.Events.DeleteCompleted += OnDeleteCompleted;
            m_project.Events.DuplicateCompleted += OnDuplicateCompleted;
            m_project.Events.CreateFoldersCompleted += OnCreateFoldersCompleted;
            m_project.Events.CreatePrefabsCompleted += OnCreatePrefabsCompleted;
        }


        protected override void OnCleanup()
        {
            base.OnCleanup();

            if (m_project != null)
            {
                m_project.Events.BeginSave -= OnBeginSave;
                m_project.Events.SaveCompleted -= OnSaveCompleted;
                m_project.Events.BeginLoad -= OnBeginLoad;
                m_project.Events.LoadCompleted -= OnLoadCompleted;
                m_project.Events.DeleteCompleted -= OnDeleteCompleted;
                m_project.Events.DuplicateCompleted -= OnDuplicateCompleted;
                m_project.Events.CreateFoldersCompleted -= OnCreateFoldersCompleted;
                m_project.Events.CreatePrefabsCompleted -= OnCreatePrefabsCompleted;

                m_project = null;
            }

            m_wm = null;

        }

        [MenuCommand("Example/Create Folder")]
        public void SaveScene()
        {
            m_wm.Prompt("Enter Folder Name", "My Folder", async (sender, args) =>
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;

                IProjectAsync project = m_project;
                using (await project.LockAsync())
                {
                    if (project.Utils.GetFolder(args.Text) == null)
                    {
                        await project.CreateFolderAsync(args.Text);
                    }
                }

                editor.IsBusy = false;
            });
        }

        [MenuCommand("Example/Delete Folder")]
        public void DeleteFolder()
        {
            m_wm.Prompt("Enter Folder Name", "My Folder", async (sender, args) =>
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;

                IProjectAsync project = m_project;
                using (await project.LockAsync())
                {
                    await project.DeleteFolderAsync(args.Text);
                }

                editor.IsBusy = false;
            });
        }

        [MenuCommand("Example/Delete Selected Folders", validate: true)]
        public bool CanDeleteSelectedFolders()
        {
            IProjectTreeModel projectTree = IOC.Resolve<IProjectTreeModel>();
            return projectTree != null && projectTree.HasSelectedItems && !projectTree.SelectedItems.Any(item => item.Parent == null);
        }

        [MenuCommand("Example/Delete Selected Folders")]
        public async void DeleteSelectedFolders()
        {
            IProjectTreeModel projectTree = IOC.Resolve<IProjectTreeModel>();

            IRTE editor = IOC.Resolve<IRTE>();
            editor.IsBusy = true;

            IProjectAsync project = m_project;
            using (await project.LockAsync())
            {
                await project.DeleteAsync(projectTree.SelectedItems.ToArray());
            }

            editor.IsBusy = false;
        }

        [MenuCommand("Example/Create Prefab", validate: true)]
        public bool CanCreatePrefabIProjectAsync()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            GameObject go = editor.Selection.activeGameObject;
            return go != null && !go.IsPrefab();
        }

        /// <summary>
        /// see FolderAndAssetExampleIRuntimeEditorMenu.cs on how to implement this operation when using the Runtime Editor
        /// </summary>
        [MenuCommand("Example/Create Prefab")]
        public async void CreatePrefab()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            GameObject go = editor.Selection.activeGameObject;

            editor.IsBusy = true;

            IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            using (await project.LockAsync())
            {
                ProjectItem folder = GetFolder();
                await project.CreatePrefabsAsync(new[] { folder }, new[] { go }, true, obj => previewUtil.CreatePreviewData(obj));
            }
            editor.IsBusy = false;
        }

        [MenuCommand("Example/Create Material")]
        public async void CreateMaterial()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.IsBusy = true;

            Material material = Instantiate(RenderPipelineInfo.DefaultMaterial);
            material.name = "Material";
            material.Color(Color.green);
            material.MainTexture(Resources.Load<Texture2D>("Duck"));
           
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            using (await project.LockAsync())
            {
                ProjectItem folder = GetFolder();
                await Save(folder, material);
            }

            editor.IsBusy = false;
        }

        private static async Task Save(ProjectItem folder, Object asset)
        {
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
            
            object[] deps = await project.GetDependenciesAsync(asset, true);

            HashSet<Object> hs = new HashSet<Object>(deps.OfType<Object>());
            hs.Add(asset);

            Object[] objects = hs.OrderBy(o => o == asset).ToArray();

            await project.SaveAsync(
                Enumerable.Repeat(folder, objects.Length).ToArray(),
                objects.Select(o => previewUtil.CreatePreviewData(o)).ToArray(),
                objects);
        }

        [MenuCommand("Example/Update Material", validate: true)]
        public bool CanUpdateMaterial()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            return editor.Selection.activeObject is Material;
        }

        [MenuCommand("Example/Update Material")]
        public async void UpdateMaterial()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.IsBusy = true;

            Material material = (Material)editor.Selection.activeObject;
            material.Color(Random.ColorHSV());

            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            using (await project.LockAsync())
            {
                ProjectItem projectItem = project.Utils.ToProjectItem(material);
                await project.SaveAsync(new[] { projectItem }, new[] { material });

                IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
                projectItem.SetPreview(previewUtil.CreatePreviewData(material));

                await m_project.SavePreviewsAsync(new[] { projectItem });
            }

            editor.IsBusy = false;
        }

        [MenuCommand("Example/Delete Selected Assets", validate: true)]
        public bool CanDeleteSelectedAssets()
        {
            IProjectFolderViewModel projectFolder = IOC.Resolve<IProjectFolderViewModel>();
            return projectFolder != null && projectFolder.HasSelectedItems;
        }

        /// <summary>
        /// see FolderAndAssetExampleIRuntimeEditorMenu.cs on how to implement this operation when using the Runtime Editor
        /// </summary>
        [MenuCommand("Example/Delete Selected Assets")]
        public async void DeleteSelectedAssets()
        {
            IProjectFolderViewModel projectFolder = IOC.Resolve<IProjectFolderViewModel>();

            IRTE editor = IOC.Resolve<IRTE>();
            editor.IsBusy = true;

            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            using (await project.LockAsync())
            {
                await project.DeleteAsync(projectFolder.SelectedItems.ToArray());
            }

            editor.IsBusy = false;
        }
     
        private void OnNewSceneCreating(object sender, System.EventArgs e)
        {
            Debug.Log("New Scene Creating");
        }

        private void OnNewSceneCreated(object sender, System.EventArgs e)
        {
            Debug.Log("New Scene Created");
        }

        private void OnBeginSave(object sender, ProjectEventArgs<object[]> e)
        {
            Debug.Log("Scene saving");
        }

        private void OnSaveCompleted(object sender, ProjectEventArgs<(ProjectItem[] SavedItems, bool IsUserAction)> e)
        {
            if(!e.HasError && e.Payload.SavedItems.Length > 0)
            {
                Debug.Log($"{e.Payload.SavedItems[0].Name} saved");
            }
        }

        private void OnBeginLoad(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            Debug.Log($"{e.Payload[0].Name} loading");
        }

        private void OnLoadCompleted(object sender, ProjectEventArgs<(ProjectItem[] LoadedItems, Object[] LoadedObjects)> e)
        {
            if (!e.HasError && e.Payload.LoadedItems.Length > 0)
            {
                Debug.Log($"{e.Payload.LoadedItems[0].Name} loaded");
            }
        }

        private void OnDeleteCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            Debug.Log($"{e.Payload[0].Name} deleted");
        }

        private void OnDuplicateCompleted(object sender, ProjectEventArgs<(ProjectItem[] OriginalItems, ProjectItem[] DuplicatedItems)> e)
        {
            Debug.Log($"{e.Payload.OriginalItems[0].Name} duplicated");
        }

        private void OnCreateFoldersCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            Debug.Log($"{e.Payload[0].Name} folder created");
        }

        private void OnCreatePrefabsCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            Debug.Log($"{e.Payload[0].Name} prefab created");
        }

        private static ProjectItem GetFolder()
        {
            IProjectAsync project = IOC.Resolve<IProjectAsync>();
            ProjectItem folder = project.State.RootFolder;
            IProjectTreeModel projectTree = IOC.Resolve<IProjectTreeModel>();
            if (projectTree != null && projectTree.SelectedItem != null)
            {
                folder = projectTree.SelectedItem;
            }

            return folder;
        }
    }
}
