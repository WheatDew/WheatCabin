using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTEditor.ViewModels;
using Battlehub.RTEditor.Views;
using Battlehub.RTHandles;
using Battlehub.RTSL;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls;
using Battlehub.UIControls.MenuControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public interface IRuntimeEditor : IRTE
    {
        event RTEEvent<CancelArgs> BeforeSceneSave;
        event RTEEvent SceneSaving;
        event RTEEvent SceneSaved;

        event RTEEvent SceneLoading;
        event RTEEvent SceneLoaded;

        void NewScene(bool confirm = true);
        void SaveScene();
        void SaveSceneAs();

        [Obsolete] //12.11.2020
        void OverwriteScene(AssetItem scene, Action<Error> callback = null);
        Task OverwriteSceneAsync(ProjectItem scene);
        void SaveSceneToFolder(ProjectItem folder, string name, Action<Error> callback = null);

        void CreateWindow(string window);
        void CreateOrActivateWindow(string window);
        void ResetToDefaultLayout();

        Task<ProjectItem[]> CreatePrefabAsync(ProjectItem folder, ExposeToEditor obj, bool? includeDependencies);
        Task<ProjectItem[]> SaveAssetsAsync(UnityObject[] assets);
        Task<ProjectItem[]> DeleteAssetsAsync(ProjectItem[] projectItems);
        Task<ProjectItem> UpdatePreviewAsync(UnityObject obj);

        ProjectAsyncOperation<AssetItem[]> CreatePrefab(ProjectItem folder, ExposeToEditor obj, bool? includeDependencies = null, Action<AssetItem[]> done = null);
        ProjectAsyncOperation<AssetItem[]> SaveAssets(UnityObject[] assets, Action<AssetItem[]> done = null);
        ProjectAsyncOperation<ProjectItem[]> DeleteAssets(ProjectItem[] projectItems, Action<ProjectItem[]> done = null);
        [Obsolete("User UpdatePreviewAsync instead")] //16.11.2020
        ProjectAsyncOperation<AssetItem> UpdatePreview(UnityObject obj, Action<AssetItem> done = null);
        [Obsolete("Use SaveAssets")]
        ProjectAsyncOperation<AssetItem> SaveAsset(UnityObject obj, Action<AssetItem> done = null);
        [Obsolete]
        bool CmdGameObjectValidate(string cmd);
        [Obsolete]
        void CmdGameObject(string cmd);
        [Obsolete]
        bool CmdEditValidate(string cmd);
        [Obsolete]
        void CmdEdit(string cmd);
    }


    [Obsolete("Use IPlacementModel instead")] //14.07.2021
    public static class IRuntimEditorExt
    {
        [Obsolete("Use IPlacementModel.GetSelectionComponent instead")]
        public static IRuntimeSelectionComponent GetScenePivot(this IRTE editor)
        {
            IPlacementModel placement = IOC.Resolve<IPlacementModel>();
            return placement.GetSelectionComponent();
        }

        [Obsolete("Use IPlacementModel.AddGameObjectToScene instead")]
        public static void AddGameObjectToScene(this IRTE editor, GameObject go, bool select = true)
        {
            IPlacementModel placement = IOC.Resolve<IPlacementModel>();
            placement.AddGameObjectToScene(go, select);
        }

        [Obsolete("Use IPlacementModel.AddGameObjectToScene instead")]
        public static void AddGameObjectToScene(this IRTE editor, GameObject go, Vector3 pivot, bool select = true)
        {
            IPlacementModel placement = IOC.Resolve<IPlacementModel>();
            placement.AddGameObjectToScene(go, pivot, select);
        }
    }

    [DefaultExecutionOrder(-92)]
    [RequireComponent(typeof(RuntimeObjects))]
    public class RuntimeEditor : RTEBase, IRuntimeEditor
    {
        public event RTEEvent<CancelArgs> BeforeSceneSave;
        public event RTEEvent SceneSaving;
        public event RTEEvent SceneSaved;

        public event RTEEvent SceneLoading;
        public event RTEEvent SceneLoaded;

        private IRuntimeSceneManager m_sceneManager;
        private IProjectAsync m_project;
        private IWindowManager m_wm;
        private ILocalization m_localization;
        private IPlacementModel m_placement;
        private IGroupingModel m_grouping;
        private IInspectorModel m_inspector;
        private IImporterModel m_importer;
        private ILayoutStorageModel m_layoutStorage;

        [SerializeField]
        private GameObject m_progressIndicator = null;

        [Serializable]
        public class Settings
        {
            public bool OpenDefaultProject = true;
            public string DefaultProjectName = null;
            public bool CreateCamera = true;
            public bool CreateLight = true;
            public bool LoadLayers = true;
        }

        [SerializeField]
        private Settings m_extraSettings;

        public override bool IsBusy
        {
            get { return base.IsBusy; }
            set
            {
                base.IsBusy = value;

                if (m_progressIndicator != null)
                {
                    m_progressIndicator.gameObject.SetActive(base.IsBusy);
                }

            }
        }


        public override bool IsPlaying
        {
            get
            {
                return base.IsPlaying;
            }
            set
            {
                if (value != base.IsPlaying)
                {
                    if (!IsPlaying)
                    {
                        RuntimeWindow gameView = GetWindow(RuntimeWindowType.Game);
                        if (gameView != null)
                        {
                            ActivateWindow(gameView);
                        }
                    }

                    base.IsPlaying = value;

                    if (!IsPlaying)
                    {
                        if (ActiveWindow == null || ActiveWindow.WindowType != RuntimeWindowType.Scene)
                        {
                            RuntimeWindow sceneView = GetWindow(RuntimeWindowType.Scene);
                            if (sceneView != null)
                            {
                                ActivateWindow(sceneView);
                            }
                        }
                    }
                }
            }
        }

        protected override void Awake()
        {
            if (!RenderPipelineInfo.UseRenderTextures)
            {
                CameraLayerSettings layerSettings = CameraLayerSettings;
                Transform uiBgCameraTransform = transform.Find("UIBackgroundCamera");
                Transform uiCameraTransform = transform.Find("UICamera");
                Transform uiBgTransform = transform.Find("UIBackground");
                if (uiBgCameraTransform != null && uiCameraTransform != null && uiBgTransform != null)
                {
                    Camera uiBgCamera = uiBgCameraTransform.GetComponent<Camera>();
                    Camera uiCamera = uiCameraTransform.GetComponent<Camera>();
                    Canvas uiBg = uiBgTransform.GetComponent<Canvas>();
                    if (uiBgCamera != null && uiCamera != null && uiBg != null)
                    {
                        uiBgCamera.enabled = true;
                        uiBg.worldCamera = uiBgCamera;
                        uiBgCamera.gameObject.SetActive(true);

                        uiCamera.clearFlags = CameraClearFlags.Depth;
                        uiCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
                    }
                }
            }
            else
            {
                Transform uiBgCameraTransform = transform.Find("UIBackgroundCamera");
                if (uiBgCameraTransform != null)
                {
                    Destroy(uiBgCameraTransform.gameObject);
                }
            }

            base.Awake();

            IOC.Resolve<IRTEAppearance>();

            m_project = IOC.Resolve<IProjectAsync>();
            m_sceneManager = IOC.Resolve<IRuntimeSceneManager>();
            m_wm = IOC.Resolve<IWindowManager>();
            m_localization = IOC.Resolve<ILocalization>();
            RegisterModels();

            if (m_sceneManager != null)
            {
                m_sceneManager.NewSceneCreating += OnNewSceneCreating;
                m_sceneManager.NewSceneCreated += OnNewSceneCreated;
            }

            if (m_project != null)
            {
                m_project.Events.BeginSave += OnBeginSave;
                m_project.Events.BeginLoad += OnBeginLoad;
                m_project.Events.SaveCompleted += OnSaveCompleted;
                m_project.Events.LoadCompleted += OnLoadCompleted;
                m_project.Events.OpenProjectCompleted += OnOpenProjectCompleted;
                m_project.Events.DeleteProjectCompleted += OnDeleteProjectCompleted;

                if (m_extraSettings == null)
                {
                    m_extraSettings = new Settings();
                }

                ApplyExtraSettingsAsync();
            }
        }

        protected override void Start()
        {
            if (GetComponent<RuntimeEditorInput>() == null)
            {
                gameObject.AddComponent<RuntimeEditorInput>();
            }
            base.Start();
            if (EventSystem != null)
            {
                if (!EventSystem.GetComponent<RTSLIgnore>() && EventSystem.transform.parent == null)
                {
                    EventSystem.gameObject.AddComponent<RTSLIgnore>();
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            StopAllCoroutines();

            if (m_sceneManager != null)
            {
                m_sceneManager.NewSceneCreating -= OnNewSceneCreating;
                m_sceneManager.NewSceneCreated -= OnNewSceneCreated;
                m_sceneManager = null;
            }

            if (m_project != null)
            {
                m_project.Events.BeginSave -= OnBeginSave;
                m_project.Events.BeginLoad -= OnBeginLoad;
                m_project.Events.SaveCompleted -= OnSaveCompleted;
                m_project.Events.LoadCompleted -= OnLoadCompleted;
                m_project.Events.OpenProjectCompleted -= OnOpenProjectCompleted;
                m_project.Events.DeleteProjectCompleted -= OnDeleteProjectCompleted;
                m_project = null;
            }

            UnregisterModels();

            m_wm = null;
            m_localization = null;
        }

        protected override void Update()
        {

        }

        protected virtual void RegisterModels()
        {
            if (!IOC.IsFallbackRegistered<IPlacementModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_placement == null)
                    {
                        GameObject modelsRoot = GetModelsRoot();
                        m_placement = modelsRoot.AddComponent<PlacementModel>();
                    }
                    return m_placement;
                });
            }

            if (!IOC.IsFallbackRegistered<IGroupingModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_grouping == null)
                    {
                        GameObject modelsRoot = GetModelsRoot();
                        m_grouping = modelsRoot.AddComponent<GroupingModel>();
                    }

                    return m_grouping;
                });
            }

            if (!IOC.IsFallbackRegistered<IInspectorModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_inspector == null)
                    {
                        GameObject modelsRoot = GetModelsRoot();
                        m_inspector = modelsRoot.AddComponent<InspectorModel>();
                    }

                    return m_inspector;
                });
            }

            if (!IOC.IsFallbackRegistered<IImporterModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_importer == null)
                    {
                        GameObject modelsRoot = GetModelsRoot();
                        m_importer = modelsRoot.AddComponent<ImporterModel>();
                    }

                    return m_importer;
                });
            }

            if (!IOC.IsFallbackRegistered<ILayoutStorageModel>())
            {
                IOC.RegisterFallback(() =>
                {
                    if (m_layoutStorage == null)
                    {
                        GameObject modelsRoot = GetModelsRoot();
                        m_layoutStorage = modelsRoot.AddComponent<PlayerPrefsLayoutStorageModel>();
                    }

                    return m_layoutStorage;
                });
            }
        }
        protected virtual void UnregisterModels()
        {
            if (m_placement != null)
            {
                IOC.UnregisterFallback<IPlacementModel>();
                m_placement = null;
            }

            if (m_grouping != null)
            {
                IOC.UnregisterFallback<IGroupingModel>();
                m_grouping = null;
            }

            if (m_inspector != null)
            {
                IOC.UnregisterFallback<IInspectorModel>();
                m_inspector = null;
            }

            if (m_importer != null)
            {
                IOC.UnregisterFallback<IImporterModel>();
                m_importer = null;
            }

            if (m_layoutStorage != null)
            {
                IOC.UnregisterFallback<ILayoutStorageModel>();
                m_layoutStorage = null;
            }
        }

        protected GameObject GetModelsRoot()
        {
            Transform models = transform.Find("Models");
            if (models == null)
            {
                models = transform;
            }
            return models.gameObject;
        }


        private async void ApplyExtraSettingsAsync()
        {
            if (m_extraSettings.OpenDefaultProject)
            {
                if (string.IsNullOrEmpty(m_extraSettings.DefaultProjectName))
                {
                    m_extraSettings.DefaultProjectName = PlayerPrefs.GetString("RuntimeEditor.DefaultProject", "DefaultProject");
                }

                using (await m_project.LockAsync())
                {
                    if (!m_project.State.IsOpened)
                    {
                        IsBusy = true;

                        await m_project.OpenProjectAsync(m_extraSettings.DefaultProjectName);

                        IsBusy = false;
                    }
                }
            }
        }

        public void ResetToDefaultLayout()
        {
            ILayoutStorageModel layoutStorage = IOC.Resolve<ILayoutStorageModel>();
            bool layoutExist = layoutStorage.LayoutExists(layoutStorage.DefaultLayoutName);
            if (layoutExist)
            {
                layoutStorage.DeleteLayout(layoutStorage.DefaultLayoutName);
            }

            m_wm.SetDefaultLayout();
        }

        public void CmdCreateWindowValidate(MenuItemValidationArgs args)
        {
#if !(UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_WSA)
            if (args.Command.ToLower() == RuntimeWindowType.ImportFile.ToString().ToLower())
            {
                args.IsVisible = false;          
            }
#endif
        }

        public virtual void CreateWindow(string windowTypeName)
        {
            m_wm.CreateWindow(windowTypeName);
        }

        public virtual void CreateOrActivateWindow(string windowTypeName)
        {
            if (!m_wm.CreateWindow(windowTypeName))
            {
                if (m_wm.Exists(windowTypeName))
                {
                    if (!m_wm.IsActive(windowTypeName))
                    {
                        m_wm.ActivateWindow(windowTypeName);

                        Transform windowTransform = m_wm.GetWindow(windowTypeName);

                        RuntimeWindow window = windowTransform.GetComponentInChildren<RuntimeWindow>();
                        if (window != null)
                        {
                            base.ActivateWindow(window);
                        }
                    }
                }
            }
        }

        public override void ActivateWindow(RuntimeWindow windowToActivate)
        {
            base.ActivateWindow(windowToActivate);
            if (windowToActivate != null)
            {
                m_wm.ActivateWindow(windowToActivate.transform);
                windowToActivate.EnableRaycasts();
            }
            else
            {
                if (Windows != null)
                {
                    foreach (RuntimeWindow window in Windows)
                    {
                        window.EnableRaycasts();
                    }
                }
            }
        }

        //[Obsolete("Use NewSceneAsync instead")] //12.11.2020
        public virtual async void NewScene(bool confirm)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            await NewSceneAsync(confirm);
        }

        private async Task NewSceneAsync(bool confirm)
        {
            if (confirm)
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                m_wm.Confirmation(m_localization.GetString("ID_RTEditor_CreateNewScene", "Create New Scene"),
                    m_localization.GetString("ID_RTEditor_DoYouWantCreateNewScene", "Do you want to create new scene?") + System.Environment.NewLine +
                    m_localization.GetString("ID_RTEditor_UnsavedChangesWillBeLost", "All unsaved changes will be lost"), async (dialog, args) =>
                    {
                        using (await m_project.LockAsync())
                        {
                            try
                            {
                                m_sceneManager.CreateNewScene();
                                tcs.SetResult(null);
                            }
                            catch (Exception e)
                            {
                                tcs.SetException(e);
                            }

                        }
                    },
                    (dialog, args) => { },
                    m_localization.GetString("ID_RTEditor_Create", "Create"),
                    m_localization.GetString("ID_RTEditor_Cancel", "Cancel"));

                await tcs.Task;
            }
            else
            {
                using (await m_project.LockAsync())
                {
                    m_sceneManager.CreateNewScene();
                }
            }
        }

        //[Obsolete("Use SaveSceneAsync instead")] //12.11.2020
        public virtual async void SaveScene()
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            await SaveSceneAsync();
        }

        private async Task SaveSceneAsync()
        {

            if (m_project.State.LoadedScene == null)
            {
                m_wm.CreateWindow(RuntimeWindowType.SaveScene.ToString());
            }
            else
            {
                if (IsPlaying)
                {
                    m_wm.MessageBox(
                        m_localization.GetString("ID_RTEditor_UnableToSaveScene", "Unable to save scene"),
                        m_localization.GetString("ID_RTEditor_UnableToSaveSceneInPlayMode", "Unable to save scene in play mode"));
                    return;
                }

                ProjectItem scene = m_project.State.LoadedScene;
                try
                {
                    await OverwriteSceneAsync(scene);
                }
                catch (Exception e)
                {
                    m_wm.MessageBox(m_localization.GetString("ID_RTEditor_UnableToSaveScene", "Unable to save scene"), e.ToString());
                }
            }

        }

        [Obsolete("Use OverwriteSceneAsync instead")] //12.11.2020
        public async void OverwriteScene(AssetItem scene, Action<Error> callback)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            try
            {
                await OverwriteSceneAsync(scene);
                callback?.Invoke(Error.NoError);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                callback?.Invoke(new Error(Error.E_Failed));
            }
        }

        public async Task OverwriteSceneAsync(ProjectItem scene)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            if (BeforeSceneSave != null)
            {
                CancelArgs args = new CancelArgs();
                BeforeSceneSave(args);
                if (args.Cancel)
                {
                    return;
                }
            }

            Undo.Purge();

            IsBusy = true;

            await m_project.SaveAsync(new[] { scene }, new[] { (object)SceneManager.GetActiveScene() });

            m_project.State.LoadedScene = scene;

            IsBusy = false;
        }

        //[Obsolete("Use SaveSceneToFolderAsync instead")] //12.11.2020
        public async void SaveSceneToFolder(ProjectItem folder, string name, Action<Error> callback)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            try
            {
                await SaveSceneToFolderAsync(folder, name);
                callback?.Invoke(Error.NoError);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                callback?.Invoke(new Error(Error.E_Failed));
            }
        }

        public async Task SaveSceneToFolderAsync(ProjectItem folder, string name)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            if (BeforeSceneSave != null)
            {
                CancelArgs args = new CancelArgs();
                BeforeSceneSave(args);
                if (args.Cancel)
                {
                    return;
                }
            }

            Undo.Purge();

            IsBusy = true;

            ProjectItem[] result = await m_project.SaveAsync(new[] { folder }, new[] { new byte[0] }, new[] { (object)SceneManager.GetActiveScene() }, new[] { name });

            IsBusy = false;

            m_project.State.LoadedScene = result[0];
        }

        public virtual void SaveSceneAs()
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            if (IsPlaying)
            {
                m_wm.MessageBox(
                    m_localization.GetString("ID_RTEditor_UnableToSaveScene", "Unable to save scene"),
                    m_localization.GetString("ID_RTEditor_UnableToSaveSceneInPlayMode", "Unable to save scene in play mode"));
                return;
            }

            CreateOrActivateWindow("SaveScene");
        }

        public Task<ProjectItem[]> CreatePrefabAsync(ProjectItem folder, ExposeToEditor obj, bool? includeDependencies)
        {
            if (m_project == null)
            {
                throw new InvalidOperationException("Project is not initialized");
            }

            if (folder == null)
            {
                folder = m_project.State.RootFolder;
            }

            TaskCompletionSource<ProjectItem[]> completionSource = new TaskCompletionSource<ProjectItem[]>();
            if (!includeDependencies.HasValue)
            {
                m_wm.Confirmation(
                    m_localization.GetString("ID_RTEditor_CreatePrefab", "Create Prefab"),
                    m_localization.GetString("ID_RTEditor_IncludeDependencies", "Include dependencies?"),
                    async (sender, args) =>
                    {
                        try
                        {
                            ProjectItem[] result = await CreatePrefabWithDependenciesAsync(folder, obj);
                            completionSource.SetResult(result);
                        }
                        catch (Exception e)
                        {
                            completionSource.SetException(e);
                        }
                    },
                    async (sender, args) =>
                    {
                        try
                        {
                            ProjectItem[] result = await CreatePrefabWithoutDependenciesAsync(folder, obj);
                            completionSource.SetResult(result);
                        }
                        catch (Exception e)
                        {
                            completionSource.SetException(e);
                        }

                    },
                    m_localization.GetString("ID_RTEditor_Yes", "Yes"),
                    m_localization.GetString("ID_RTEditor_No", "No"));
            }
            else
            {
                if (includeDependencies.Value)
                {
                    return CreatePrefabWithDependenciesAsync(folder, obj);
                }
                else
                {
                    return CreatePrefabWithoutDependenciesAsync(folder, obj);
                }
            }

            return completionSource.Task;
        }


        private async Task<ProjectItem[]> CreatePrefabWithoutDependenciesAsync(ProjectItem folder, ExposeToEditor obj)
        {
            IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();
            byte[] previewData = previewUtility.CreatePreviewData(obj.gameObject);
            return await CreatePrefabAsync(folder, new[] { previewData }, new[] { obj.gameObject });
        }

        private async Task<ProjectItem[]> CreatePrefabWithDependenciesAsync(ProjectItem folder, ExposeToEditor obj)
        {
            IResourcePreviewUtility previewUtility = IOC.Resolve<IResourcePreviewUtility>();

            object[] deps = await m_project.GetDependenciesAsync(obj.gameObject, true);
            object[] objects;
            if (!deps.Contains(obj.gameObject))
            {
                objects = new object[deps.Length + 1];
                objects[deps.Length] = obj.gameObject;
                for (int i = 0; i < deps.Length; ++i)
                {
                    objects[i] = deps[i];
                }
            }
            else
            {
                objects = deps;
            }

            IUnityObjectFactory uoFactory = IOC.Resolve<IUnityObjectFactory>();
            objects = objects.Where(o => uoFactory.CanCreateInstance(o.GetType())).ToArray();

            byte[][] previewData = new byte[objects.Length][];
            for (int i = 0; i < objects.Length; ++i)
            {
                if (objects[i] is UnityObject)
                {
                    previewData[i] = previewUtility.CreatePreviewData((UnityObject)objects[i]);
                }
            }

            ProjectItem[] result = await CreatePrefabAsync(folder, previewData, objects);
            return result;
        }

        private async Task<ProjectItem[]> CreatePrefabAsync(ProjectItem folder, byte[][] previewData, object[] objects)
        {
            if (objects.Any(o => !(o is GameObject)))
            {
                if (folder.Children == null || folder.Get("Data") == null)
                {
                    await m_project.CreateFoldersAsync(new[] { folder }, new[] { "Data" });
                }

#pragma warning disable CS0612
                IProjectTree projectTree = IOC.Resolve<IProjectTree>();
#pragma warning restore CS0612
                if (projectTree != null)
                {
                    projectTree.SelectedItem = folder;
                }
                else
                {
                    IProjectTreeModel projectTreeViewModel = IOC.Resolve<IProjectTreeModel>();
                    if (projectTreeViewModel != null)
                    {
                        projectTreeViewModel.SelectedItem = folder;
                    }
                }
            }

            ProjectItem dataFolder = folder.Get("Data");
            List<ProjectItem> parents = new List<ProjectItem>();
            for (int i = 0; i < objects.Length; ++i)
            {
                object obj = objects[i];
                if (obj is GameObject)
                {
                    parents.Add(folder);
                }
                else
                {
                    parents.Add(dataFolder);
                }
            }

            ProjectItem[] result = await m_project.SaveAsync(parents.ToArray(), previewData, objects, null);
            return result;
        }

        public async Task<ProjectItem[]> SaveAssetsAsync(UnityObject[] assets)
        {
            List<UnityObject> assetsToSave = new List<UnityObject>();
            List<ProjectItem> projectItems = new List<ProjectItem>();

            for (int i = 0; i < assets.Length; ++i)
            {
                UnityObject asset = assets[i];
                ProjectItem projectItem = m_project.Utils.ToProjectItem(asset);
                if (projectItem == null)
                {
                    continue;
                }

                assetsToSave.Add(asset);
                projectItems.Add(projectItem);
            }

            if (assetsToSave.Count == 0)
            {
                return new ProjectItem[0];
            }

            ProjectItem[] items = projectItems.ToArray();
            await m_project.SaveAsync(items, assets.ToArray(), false);

            IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
            for (int i = 0; i < items.Length; ++i)
            {
                byte[] preview = previewUtil.CreatePreviewData(assets[i]);
                items[i].SetPreview(preview);
            }

            await m_project.SavePreviewsAsync(items);
            await UpdatePreviewsDependentOn(items);
            return items;
        }

        public async Task<ProjectItem[]> DeleteAssetsAsync(ProjectItem[] projectItems)
        {
            ProjectItem[] assetItems = projectItems.Where(pi => !pi.IsFolder).ToArray();
            for (int i = 0; i < assetItems.Length; ++i)
            {
                ProjectItem assetItem = assetItems[i];
                UnityObject obj = m_project.Utils.FromProjectItem<UnityObject>(assetItem);

                if (obj != null)
                {
                    if (obj is GameObject)
                    {
                        GameObject go = (GameObject)obj;
                        Component[] components = go.GetComponentsInChildren<Component>(true);
                        for (int j = 0; j < components.Length; ++j)
                        {
                            Component component = components[j];
                            Undo.Erase(component, null);
                            if (component is Transform)
                            {
                                Undo.Erase(component.gameObject, null);
                            }
                        }
                    }
                    else
                    {
                        Undo.Erase(obj, null);
                    }
                }
            }

            ProjectItem[] folders = projectItems.Where(pi => pi.IsFolder).ToArray();
            ProjectItem[] result = assetItems.Union(folders).ToArray();
            await m_project.DeleteAsync(result);
            await Task.Yield();
            await UpdatePreviewsDependentOn(assetItems);
            return projectItems;
        }

        private async Task UpdatePreviewsDependentOn(ProjectItem[] assetItems)
        {
            IResourcePreviewUtility previewUtil = IOC.Resolve<IResourcePreviewUtility>();
            ProjectItem[] dependentItems = m_project.Utils.GetProjectItemsDependentOn(assetItems).Where(item => !m_project.Utils.IsScene(item)).ToArray();
            if (dependentItems.Length == 0)
            {
                return;
            }

            UnityObject[] loadedObjects = await m_project.LoadAsync(dependentItems);

            for (int i = 0; i < loadedObjects.Length; ++i)
            {
                UnityObject loadedObject = loadedObjects[i];
                ProjectItem dependentItem = dependentItems[i];
                if (loadedObject != null)
                {
                    byte[] previewData = previewUtil.CreatePreviewData(loadedObject);
                    dependentItem.SetPreview(previewData);
                }
                else
                {
                    dependentItem.SetPreview(null);
                }
            }

            await m_project.SavePreviewsAsync(dependentItems);
        }

        public async Task<ProjectItem> UpdatePreviewAsync(UnityObject obj)
        {
            using (await m_project.LockAsync())
            {
                ProjectItem projectItem = m_project.Utils.ToProjectItem(obj);
                if (projectItem != null)
                {
                    IResourcePreviewUtility resourcePreviewUtility = IOC.Resolve<IResourcePreviewUtility>();
                    byte[] preview = resourcePreviewUtility.CreatePreviewData(obj);
                    projectItem.SetPreview(preview);
                }
                return projectItem;
            }
        }

        private void OnNewSceneCreating(object sender, EventArgs e)
        {
            IsPlaying = false;

            SceneLoading?.Invoke();
        }

        private async void OnNewSceneCreated(object sender, EventArgs e)
        {
            if (m_extraSettings.CreateLight)
            {
                if (m_project.Utils.ToGuid(typeof(Light)) != Guid.Empty)
                {
                    GameObject lightGO = new GameObject(m_localization.GetString("ID_RTEditor_DirectionalLight", "Directional Light"));
                    lightGO.transform.position = Vector3.up * 3;
                    lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

                    Light light = lightGO.AddComponent<Light>();
                    light.type = LightType.Directional;
                    light.shadows = LightShadows.Soft;
                    lightGO.AddComponent<ExposeToEditor>();

                    if (RTSLSettings.SaveIncludedObjectsOnly)
                    {
                        lightGO.AddComponent<RTSLInclude>();
                    }

                    if (RenderPipelineInfo.Type == RPType.HDRP)
                    {
                        light.intensity = 10000;
                    }
                }
            }

            if (m_extraSettings.CreateCamera)
            {
                if (m_project.Utils.ToGuid(typeof(Camera)) != Guid.Empty)
                {
                    GameObject cameraGO = new GameObject(m_localization.GetString("ID_RTEditor_Camera", "Camera"));
                    cameraGO.transform.position = new Vector3(0, 1, -10);

                    cameraGO.gameObject.SetActive(false);
                    cameraGO.AddComponent<Camera>();
                    cameraGO.AddComponent<ExposeToEditor>();
                    cameraGO.gameObject.SetActive(true);

                    if (RTSLSettings.SaveIncludedObjectsOnly)
                    {
                        cameraGO.AddComponent<RTSLInclude>();
                    }

                    cameraGO.AddComponent<GameViewCamera>();
                }
            }

            Selection.objects = null;
            Undo.Purge();

            await Task.Yield();
            await Task.Yield();

            SceneLoaded?.Invoke();
        }

        private async void OnOpenProjectCompleted(object sender, ProjectEventArgs<ProjectInfo> e)
        {
            PlayerPrefs.SetString("RuntimeEditor.DefaultProject", e.Payload.Name);

            if (m_extraSettings != null && m_extraSettings.LoadLayers)
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;
                await LayersEditor.LoadLayersAsync(layers =>
                {
                    editor.IsBusy = false;
                });
            }
        }

        private void OnDeleteProjectCompleted(object sender, ProjectEventArgs<string> e)
        {
            if (e.Payload == PlayerPrefs.GetString("RuntimeEditor.DefaultProject"))
            {
                PlayerPrefs.DeleteKey("RuntimeEditor.DefaultProject");
            }
        }

        private void OnBeginLoad(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            RaiseIfIsScene(e.Payload, () =>
            {
                IsPlaying = false;

                Selection.objects = null;
                Undo.Purge();

                SceneLoading?.Invoke();
            });
        }

        private void OnLoadCompleted(object sender, ProjectEventArgs<(ProjectItem[] LoadedItems, UnityObject[] LoadedObjects)> e)
        {
            RaiseIfIsScene(e.Payload.LoadedItems, () =>
            {
                SceneLoaded?.Invoke();
            });
        }

        private void OnBeginSave(object sender, ProjectEventArgs<object[]> e)
        {
            object[] result = e.Payload;
            if (result != null && result.Length > 0)
            {
                IsPlaying = false;

                object obj = result[0];
                if (obj != null && obj is Scene)
                {
                    SceneSaving?.Invoke();
                }
            }
        }

        private void OnSaveCompleted(object sender, ProjectEventArgs<(ProjectItem[] SavedItems, bool IsUserAction)> e)
        {
            RaiseIfIsScene(e.Payload.SavedItems, () =>
            {
                SceneSaved?.Invoke();
            });
        }

        private void RaiseIfIsScene(ProjectItem[] projectItems, Action callback)
        {
            if (projectItems != null && projectItems.Length > 0)
            {
                ProjectItem projectItem = projectItems[0];
                if (projectItem != null && m_project.Utils.IsScene(projectItem))
                {
                    callback();
                }
            }
        }


        #region Obsolete


        [Obsolete("Use CreatePrefabAsync instead")]
        public ProjectAsyncOperation<AssetItem[]> CreatePrefab(ProjectItem folder, ExposeToEditor obj, bool? includeDependencies, Action<AssetItem[]> done)
        {
            ProjectAsyncOperation<AssetItem[]> ao = new ProjectAsyncOperation<AssetItem[]>();
            CreatePrefabAsync(folder, obj, includeDependencies, ao, done);
            return ao;
        }


        [Obsolete("Use CreatePrefabAsync instead")]
        private async void CreatePrefabAsync(ProjectItem folder, ExposeToEditor obj, bool? includeDependencies, ProjectAsyncOperation<AssetItem[]> ao, Action<AssetItem[]> callback)
        {
            ProjectItem[] result = await CreatePrefabAsync(folder, obj, includeDependencies);
            callback?.Invoke(result.OfType<AssetItem>().ToArray());
            ao.Result = result.OfType<AssetItem>().ToArray();
            ao.IsCompleted = true;
        }


        [Obsolete("Use SaveAssetsAsync")] //12.11.2020
        public ProjectAsyncOperation<AssetItem[]> SaveAssets(UnityObject[] assets, Action<AssetItem[]> done)
        {
            ProjectAsyncOperation<AssetItem[]> ao = new ProjectAsyncOperation<AssetItem[]>();

            SaveAssetsAsync(assets, ao, done);

            return ao;
        }

        [Obsolete("Use SaveAssetsAsync")] //12.11.2020
        private async void SaveAssetsAsync(UnityObject[] assets, ProjectAsyncOperation<AssetItem[]> ao, Action<AssetItem[]> done)
        {
            ProjectItem[] result = await SaveAssetsAsync(assets);
            done?.Invoke(result.OfType<AssetItem>().ToArray());
            ao.Result = result.OfType<AssetItem>().ToArray();
            ao.IsCompleted = true;
        }


        [Obsolete("Use SaveAssetsAsync")]
        public ProjectAsyncOperation<AssetItem> SaveAsset(UnityObject obj, Action<AssetItem> done)
        {
            ProjectAsyncOperation<AssetItem> ao = new ProjectAsyncOperation<AssetItem>();

            IProject project = IOC.Resolve<IProject>();
            AssetItem assetItem = project.ToAssetItem(obj);
            if (assetItem == null)
            {
                if (done != null)
                {
                    done(null);
                }

                ao.Error = new Error();
                ao.IsCompleted = true;
                return ao;
            }

            IsBusy = true;
            project.Save(new[] { assetItem }, new[] { obj }, (saveError, saveResult) =>
            {
                if (saveError.HasError)
                {
                    IsBusy = false;
                    m_wm.MessageBox(m_localization.GetString("ID_RTEditor_UnableToSaveAsset", "Unable to save asset"), saveError.ErrorText);

                    if (done != null)
                    {
                        done(null);
                    }

                    ao.Error = saveError;
                    ao.IsCompleted = true;
                    return;
                }

                UpdateDependentPreview(saveResult, () =>
                {
                    IsBusy = false;
                    if (done != null)
                    {
                        done(saveResult[0]);
                    }
                    ao.Error = new Error();
                    ao.Result = saveResult[0];
                    ao.IsCompleted = true;
                });
            });

            return ao;
        }

        //[Obsolete("Use DeleteAssetsAsync instead")] //12.11.2020
        public ProjectAsyncOperation<ProjectItem[]> DeleteAssets(ProjectItem[] projectItems, Action<ProjectItem[]> done)
        {
            ProjectAsyncOperation<ProjectItem[]> ao = new ProjectAsyncOperation<ProjectItem[]>();
            DeleteAssetsAsync(projectItems, ao, done);
            return ao;
        }

        //[Obsolete("Use DeleteAssetsAsync instead")] //12.11.2020
        private async void DeleteAssetsAsync(ProjectItem[] projectItems, ProjectAsyncOperation<ProjectItem[]> ao, Action<ProjectItem[]> callback)
        {
            ProjectItem[] result = await DeleteAssetsAsync(projectItems);
            callback?.Invoke(result);
            ao.Result = result;
            ao.IsCompleted = true;
        }

        [Obsolete("Use UpdatePreviewAsync instead")] //12.11.2020
        public ProjectAsyncOperation<AssetItem> UpdatePreview(UnityObject obj, Action<AssetItem> done)
        {
            ProjectAsyncOperation<AssetItem> ao = new ProjectAsyncOperation<AssetItem>();

            IProject project = IOC.Resolve<IProject>();
            AssetItem assetItem = project.ToAssetItem(obj);
            if (assetItem != null)
            {
                IResourcePreviewUtility resourcePreviewUtility = IOC.Resolve<IResourcePreviewUtility>();
                byte[] preview = resourcePreviewUtility.CreatePreviewData(obj);
                assetItem.Preview = new Preview { PreviewData = preview };
                project.SetPersistentID(assetItem.Preview, project.ToPersistentID(assetItem));
            }

            if (done != null)
            {
                done(assetItem);
            }

            ao.Error = new Error();
            ao.Result = assetItem;
            ao.IsCompleted = true;
            return ao;
        }

        [Obsolete("Use UpdateDependentPreviewsAsync instead")] //12.11.2020
        private async void UpdateDependentPreview(ProjectItem[] projectItems, Action callback)
        {
            await UpdatePreviewsDependentOn(projectItems);
            callback?.Invoke();
        }


        [Obsolete]
        public void CmdGameObjectValidate(MenuItemValidationArgs args)
        {
            args.IsValid = CmdGameObjectValidate(args.Command);
        }

        [Obsolete]
        public bool CmdGameObjectValidate(string cmd)
        {
            IGameObjectCmd goCmd = IOC.Resolve<IGameObjectCmd>();
            if (goCmd != null)
            {
                return goCmd.CanExec(cmd);
            }
            return false;
        }

        [Obsolete]
        public void CmdGameObject(string cmd)
        {
            IGameObjectCmd goCmd = IOC.Resolve<IGameObjectCmd>();
            if (goCmd != null)
            {
                goCmd.Exec(cmd);
            }
        }

        [Obsolete]
        public void CmdEditValidate(MenuItemValidationArgs args)
        {
            args.IsValid = CmdEditValidate(args.Command);
        }

        [Obsolete]
        public bool CmdEditValidate(string cmd)
        {
            IEditCmd editCmd = IOC.Resolve<IEditCmd>();
            if (editCmd != null)
            {
                return editCmd.CanExec(cmd);
            }
            return false;
        }

        [Obsolete]
        public void CmdEdit(string cmd)
        {
            IEditCmd editCmd = IOC.Resolve<IEditCmd>();
            if (editCmd != null)
            {
                editCmd.Exec(cmd);
            }
        }

        #endregion
    }
}
