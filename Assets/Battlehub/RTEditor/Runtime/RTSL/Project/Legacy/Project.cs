using Battlehub.RTCommon;
using Battlehub.RTCommon.EditorTreeView;
using Battlehub.RTSL.Interface;
using UnityEngine.Battlehub.SL2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;
using Battlehub.RTSL.Battlehub.SL2;
using Battlehub.Utils;

namespace Battlehub.RTSL
{
    [Obsolete]
    /// <summary>
    /// Legacy (use ProjectAsync instead)
    /// </summary>
    public abstract class Project<TID> : MonoBehaviour
    {
        public event ProjectEventHandler NewSceneCreating;
        public event ProjectEventHandler NewSceneCreated;
        public event ProjectEventHandler<ProjectInfo> CreateProjectCompleted;
        public event ProjectEventHandler<ProjectInfo> OpenProjectCompleted;
        public event ProjectEventHandler<string> CopyProjectCompleted;
        public event ProjectEventHandler<string> DeleteProjectCompleted;
        public event ProjectEventHandler<string> ExportProjectCompleted;
        public event ProjectEventHandler<string> ImportProjectCompleted;

        public event ProjectEventHandler<ProjectInfo[]> ListProjectsCompleted;
        public event ProjectEventHandler CloseProjectCompleted;

        public event ProjectEventHandler<ProjectItem[]> GetAssetItemsCompleted;
        public event ProjectEventHandler<object[]> BeginSave;
        public event ProjectEventHandler<AssetItem[], bool> SaveCompleted;
        public event ProjectEventHandler<AssetItem[]> BeginLoad;
        public event ProjectEventHandler<AssetItem[], UnityObject[]> LoadCompleted;
        public event ProjectEventHandler<AssetItem[]> ImportCompleted;
        protected void RaiseImportCompleted(Error error, AssetItem[] assetItems)
        {
            if (ImportCompleted != null)
            {
                ImportCompleted(error, assetItems);
            }
        }

        [Obsolete]
        public event ProjectEventHandler<AssetItem[]> DuplicateCompleted;
        public event ProjectEventHandler<ProjectItem[]> DuplicateItemsCompleted;

        public event ProjectEventHandler UnloadCompleted;
        public event ProjectEventHandler<ProjectItem[]> BeforeDeleteCompleted;
        public event ProjectEventHandler<ProjectItem[]> DeleteCompleted;
        public event ProjectEventHandler<ProjectItem[], ProjectItem[]> MoveCompleted;
        public event ProjectEventHandler<ProjectItem> RenameCompleted;
        public event ProjectEventHandler<ProjectItem[]> CreateCompleted;

        //private IProjectAsyncToProjectEvents m_eventForwarder;
        protected IStorage<TID> m_storage;
        private IAssetDB<TID> m_assetDB;

        protected ITypeMap m_typeMap;
        private IUnityObjectFactory m_factory;
        private IProjectAsyncState<TID> m_state;
        internal IProjectAsyncState<TID> State
        {
            get { return m_state; }
        }

        public bool IsOpened
        {
            get { return ProjectInfo != null; }
        }

        protected string ProjectPath
        {
            get { return m_state.ProjectPath; }
        }
        public ProjectInfo ProjectInfo
        {
            get { return m_state.ProjectInfo; }
        }
        public ProjectItem Root
        {
            get { return m_state.RootFolder; }
        }

        public AssetItem LoadedScene
        {
            get { return (AssetItem)m_state.LoadedScene; }
            set { m_state.LoadedScene = value; }
        }

        public virtual AssetBundle[] LoadedAssetBundles
        {
            get { return new AssetBundle[0]; }
        }

        /// <summary>
        /// only one operation can be active at a time
        /// </summary>
        private bool m_isBusy;
        public bool IsBusy
        {
            get { return m_isBusy; }
            protected set 
            {
                if (m_isBusy != value)
                {
                    if (value)
                    {
                        m_isBusy = value;
                        Application.logMessageReceived += OnApplicationLogMessageReceived;
                    }
                    else
                    {
                        if (m_actionsQueue.Count > 0)
                        {
                            Action workItem = m_actionsQueue.Dequeue();
                            workItem();
                        }
                        else
                        {
                            m_isBusy = value;
                            Application.logMessageReceived -= OnApplicationLogMessageReceived;
                        }
                    }
                }
            }
        }

        private void OnApplicationLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                IsBusy = false;
            }
        }

        private readonly Queue<Action> m_actionsQueue = new Queue<Action>();

        private IEnumerator CoCallback(Action cb)
        {
            yield return new WaitForEndOfFrame();
            if (cb != null)
            {
                cb();
            }
        }

        protected virtual void Awake()
        {
            Awake_Internal();
        }

        public virtual void Awake_Internal()
        {
            m_assetDB = IOC.Resolve<IAssetDB<TID>>();
            m_storage = IOC.Resolve<IStorage<TID>>();
            m_typeMap = IOC.Resolve<ITypeMap>();
            m_factory = IOC.Resolve<IUnityObjectFactory>();
            m_state = IOC.Resolve<IProjectAsyncState<TID>>();
           
        }

        protected virtual void Start()
        {
        }

        protected virtual void OnDestroy()
        {
            StopAllCoroutines();
            UnloadUnregisterDestroy();
            m_actionsQueue.Clear();
        }

        protected virtual void UnloadUnregister()
        {
            m_assetDB.UnregisterSceneObjects();
            m_assetDB.UnregisterDynamicResources();
            m_state.ClearProjectItemsCache();
        }

        private void UnloadUnregisterDestroy()
        {
            UnityObject[] dynamicResources = m_assetDB.GetDynamicResources();
            AssetBundle[] loadedAssetBundles = LoadedAssetBundles;

            UnloadUnregister();

            foreach (UnityObject dynamicResource in dynamicResources)
            {
                if (dynamicResource is Transform)
                {
                    continue;
                }
                Destroy(dynamicResource);
            }

            foreach (AssetBundle assetBundle in loadedAssetBundles)
            {
                assetBundle.Unload(true);
            }
        }

        protected abstract bool IsEqual(TID id1, TID id2);
        protected abstract TID GetID(ProjectItem projectItem);
        protected abstract void SetID(ProjectItem projectItem, TID id);
        protected abstract TID GetID(PrefabPart prefabPart);
        protected abstract void SetID(PrefabPart prefabPart, TID id);
        protected abstract void SetParentID(PrefabPart prefabPart, TID id);
        protected abstract TID GetID(Preview preview);
        protected abstract void SetID(Preview preview, TID id);
        protected abstract TID[] GetDependencies(AssetItem assetItem);
        protected abstract void SetDependencies(AssetItem assetItem, TID[] dependencies);
        protected abstract void GenerateIdentifiers(int count, Action<Error, TID[]> callback);

        protected virtual void ResolveDepenencies(Action callback, HashSet<TID> unresolvedDependencies)
        {
            callback();
        }

        protected virtual void LoadLibraryWithSceneDependencies(Action callback)
        {
            callback();
        }
        protected virtual void LoadAllAssetLibraries(TID[] deps, Action callback)
        {
            callback();
        }

        protected virtual void _LoadImportItems(string libraryName, bool isBuiltIn, ProjectEventHandler<ProjectItem> callback, ProjectAsyncOperation<ProjectItem> ao)
        {
            throw new NotImplementedException();
        }

        public virtual void UnloadImportItems(ProjectItem importItemsRoot)
        {

        }
        protected virtual void _Import(ImportItem[] importItems, ProjectEventHandler<AssetItem[]> callback, ProjectAsyncOperation<AssetItem[]> ao)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<int, string> GetStaticAssetLibraries()
        {
            return new Dictionary<int, string>();
        }

        public bool IsStatic(ProjectItem projectItem)
        {
            return m_assetDB.IsStaticResourceID(GetID(projectItem));
        }

        public bool IsScene(ProjectItem projectItem)
        {
            if (projectItem.IsFolder)
            {
                return false;
            }

            AssetItem assetItem = (AssetItem)projectItem;
            return ToType(assetItem) == typeof(Scene);
        }

        public Type ToType(AssetItem assetItem)
        {
            return m_typeMap.ToType(assetItem.TypeGuid);
        }

        public Guid ToGuid(Type type)
        {
            return m_typeMap.ToGuid(type);
        }
        public object ToPersistentID(UnityObject obj)
        {
            return m_assetDB.ToID(obj);
        }

        public object ToPersistentID(ProjectItem projectItem)
        {
            return GetID(projectItem);
        }

        public object ToPersistentID(Preview preview)
        {
            return GetID(preview);
        }

        public T FromPersistentID<T>(object id) where T : UnityObject
        {
            return m_assetDB.FromID<T>((TID)id);
        }

        public T FromPersistentID<T>(ProjectItem projectItem) where T : UnityObject
        {
            return m_assetDB.FromID<T>(GetID(projectItem));
        }

        public void SetPersistentID(ProjectItem projectItem, object id)
        {
            SetID(projectItem, (TID)id);
        }

        public void SetPersistentID(Preview preview, object id)
        {
            SetID(preview, (TID)id);
        }

        [Obsolete]
        public virtual long ToID(UnityObject obj)
        {
            throw new NotSupportedException();
        }

        [Obsolete]
        public T FromID<T>(long id) where T : UnityObject
        {
            throw new NotSupportedException();
        }

        public AssetItem ToAssetItem(UnityObject obj)
        {
            ProjectItem result;
            TID id = m_assetDB.ToID(obj);
            if (!m_state.TryGetProjectItemFromCache(id, out result))
            {
                return null;
            }
            return (AssetItem)result;
        }

        public string GetExt(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is RuntimeTextAsset)
            {
                RuntimeTextAsset textAsset = (RuntimeTextAsset)obj;
                return textAsset.Ext;
            }
            else if (obj is RuntimeBinaryAsset)
            {
                RuntimeBinaryAsset binaryAsset = (RuntimeBinaryAsset)obj;
                return binaryAsset.Ext;
            }

            return GetExt(obj.GetType());
        }

        public string GetExt(Type type)
        {
            if (type == null)
            {
                return null;
            }
            if (type == typeof(Scene))
            {
                return ".rtscene";
            }
            if (type == typeof(GameObject))
            {
                return ".rtprefab";
            }
            if (type == typeof(ScriptableObject))
            {
                return ".rtasset";
            }
            if (type == typeof(Material))
            {
                return ".rtmat";
            }
            if (type == typeof(Mesh))
            {
                return ".rtmesh";
            }
            if (type == typeof(Shader))
            {
                return ".rtshader";
            }
            if (type == typeof(TerrainData))
            {
                return ".rtterdata";
            }
            if (type == typeof(TerrainLayer))
            {
                return ".rtterlayer";
            }
            if (type == typeof(RuntimeTextAsset))
            {
                return ".txt";
            }
            if (type == typeof(RuntimeBinaryAsset))
            {
                return ".bin";
            }
            return ".rt" + type.Name.ToLower().Substring(0, 3);
        }

        public string GetUniqueName(string name, string[] names)
        {
            return PathHelper.GetUniqueName(name, names.ToList());
        }

        public string GetUniqueName(string name, Type type, ProjectItem folder, bool noSpace = false)
        {
            if (folder.Children == null)
            {
                return name;
            }

            string ext = GetExt(type);
            List<string> existingNames = folder.Children.Where(c => !c.IsFolder).Select(c => c.NameExt).ToList();
            return PathHelper.GetUniqueName(name, ext, existingNames, noSpace);
        }

        public string GetUniqueName(string name, string ext, ProjectItem folder, bool noSpace = false)
        {
            if (folder == null || folder.Children == null)
            {
                return name;
            }

            List<string> existingNames = folder.Children.Where(c => !c.IsFolder).Select(c => c.NameExt).ToList();
            return PathHelper.GetUniqueName(name, ext, existingNames, noSpace);
        }

        public string GetUniquePath(string path, Type type, ProjectItem folder, bool noSpace = false)
        {
            string name = Path.GetFileName(path);
            name = GetUniqueName(name, type, folder, noSpace);

            path = Path.GetDirectoryName(path).Replace(@"\", "/");

            return path + (path.EndsWith("/") ? name : "/" + name);
        }

        public void CreateNewScene()
        {
            if (NewSceneCreating != null)
            {
                NewSceneCreating(new Error(Error.OK));
            }

            ClearScene();

            m_state.LoadedScene = null;

            if (NewSceneCreated != null)
            {
                NewSceneCreated(new Error(Error.OK));
            }
        }

        public void ClearScene()
        {
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; ++i)
            {
                GameObject rootGO = rootGameObjects[i];
                if (rootGO.GetComponent<RTSLIgnore>() || (rootGO.hideFlags & HideFlags.DontSave) != 0)
                {
                    continue;
                }

                if (RTSLSettings.SaveIncludedObjectsOnly)
                {
                    if (rootGO.GetComponent<RTSLInclude>() == null)
                    {
                        continue;
                    }
                }


                Destroy(rootGO);
            }
        }


        /// <summary>
        /// List all projects
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<ProjectInfo[]> GetProjects(ProjectEventHandler<ProjectInfo[]> callback = null)
        {
            ProjectAsyncOperation<ProjectInfo[]> ao = new ProjectAsyncOperation<ProjectInfo[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _GetProjects(callback, ao));
            }
            else
            {
                IsBusy = true;
                _GetProjects(callback, ao);
            }
            return ao;
        }

        private void _GetProjects(ProjectEventHandler<ProjectInfo[]> callback, ProjectAsyncOperation<ProjectInfo[]> ao)
        {
            m_storage.GetProjects((error, projects) =>
            {
                if (callback != null)
                {
                    callback(error, projects);
                }

                ao.Error = error;
                ao.Result = projects;
                ao.IsCompleted = true;

                if (ListProjectsCompleted != null)
                {
                    ListProjectsCompleted(error, projects);
                }

                IsBusy = false;
            });
        }

        /// <summary>
        /// Create Project
        /// </summary>
        /// <param name="project"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<ProjectInfo> CreateProject(string project, ProjectEventHandler<ProjectInfo> callback = null)
        {
            ProjectAsyncOperation<ProjectInfo> ao = new ProjectAsyncOperation<ProjectInfo>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _CreateProject(project, callback, ao));
            }
            else
            {
                IsBusy = true;
                _CreateProject(project, callback, ao);
            }
            return ao;
        }

        private void _CreateProject(string project, ProjectEventHandler<ProjectInfo> callback, ProjectAsyncOperation<ProjectInfo> ao)
        {
            m_storage.CreateProject(project, (error, projectInfo) =>
            {
                if (callback != null)
                {
                    callback(error, projectInfo);
                }
                if (CreateProjectCompleted != null)
                {
                    CreateProjectCompleted(error, projectInfo);
                }
                ao.Error = error;
                ao.Result = projectInfo;
                ao.IsCompleted = true;

                IsBusy = false;
            });
        }

        public YieldLock Lock()
        {
            return new YieldLock(m_state.LockAsync());
        }

        /// <summary>
        /// Open Project
        /// </summary>
        /// <param name="project"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<ProjectInfo> OpenProject(string project, ProjectEventHandler<ProjectInfo> callback)
        {
            return OpenProject(project, OpenProjectFlags.Default, callback);
        }

        public ProjectAsyncOperation<ProjectInfo> OpenProject(string project, OpenProjectFlags flags, ProjectEventHandler<ProjectInfo> callback)
        {
            ProjectAsyncOperation<ProjectInfo> ao = new ProjectAsyncOperation<ProjectInfo>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _OpenProject(project, flags, callback, ao));
            }
            else
            {
                IsBusy = true;
                _OpenProject(project, flags, callback, ao);
            }
            return ao;
        }


        private void _OpenProject(string project, OpenProjectFlags flags, ProjectEventHandler<ProjectInfo> callback, ProjectAsyncOperation<ProjectInfo> ao)
        {
            if ((flags & OpenProjectFlags.DestroyObjects) != 0)
            {
                UnloadUnregisterDestroy();
            }
            else
            {
                UnloadUnregister();
            }

            if (m_state.ProjectInfo != null)
            {
                if ((flags & OpenProjectFlags.CreateNewScene) != 0)
                {
                    CreateNewScene();
                }
                else
                {
                    if ((flags & OpenProjectFlags.ClearScene) != 0)
                    {
                        ClearScene();
                    }
                    m_state.LoadedScene = null;
                }
            }

            m_state.ProjectInfo = null;
            m_state.RootFolder = null;

            m_storage.GetProject(project, (error, projectInfo, assetBundleInfo) =>
            {
                if (error.HasError)
                {
                    RaiseOpenProjectCompleted(callback, ao, error, projectInfo);
                    return;
                }

                OnGetProjectCompleted(project, projectInfo, assetBundleInfo, ao, callback);
            });
        }

        private void RaiseOpenProjectCompleted(ProjectEventHandler<ProjectInfo> callback, ProjectAsyncOperation<ProjectInfo> ao, Error error, ProjectInfo projectInfo)
        {
            if (callback != null)
            {
                callback(error, projectInfo);
            }

            ao.Result = projectInfo;
            ao.Error = error;
            ao.IsCompleted = true;

            if (OpenProjectCompleted != null)
            {
                OpenProjectCompleted(error, projectInfo);
            }

            IsBusy = false;
        }

        protected virtual void OnGetProjectCompleted(string project, ProjectInfo projectInfo, AssetBundleInfo[] assetBundleInfo, ProjectAsyncOperation<ProjectInfo> ao, ProjectEventHandler<ProjectInfo> callback)
        {
            if (projectInfo == null)
            {
                projectInfo = new ProjectInfo();
            }

            m_state.ProjectPath = project;
            m_state.ProjectInfo = projectInfo;
            GetProjectTree(project, ao, callback);
        }

        private void GetProjectTree(string project, ProjectAsyncOperation<ProjectInfo> ao, ProjectEventHandler<ProjectInfo> callback)
        {
            m_storage.GetProjectTree(project, (error, rootFolder) =>
            {
                if (error.HasError)
                {
                    if (callback != null)
                    {
                        callback(error, m_state.ProjectInfo);
                    }

                    ao.Result = m_state.ProjectInfo;
                    ao.Error = error;
                    ao.IsCompleted = true;

                    if (OpenProjectCompleted != null)
                    {
                        OpenProjectCompleted(error, m_state.ProjectInfo);
                    }

                    IsBusy = false;
                    return;
                }

                OnGetProjectTreeCompleted(error, rootFolder, ao, callback);
            });
        }

        private void OnGetProjectTreeCompleted(Error error, ProjectItem rootFolder, ProjectAsyncOperation<ProjectInfo> ao, ProjectEventHandler<ProjectInfo> callback)
        {
            m_state.RootFolder = rootFolder;

            AssetItem[] assetItems = m_state.FlattenHierarchy(true).OfType<AssetItem>().ToArray();
            for(int i = 0; i < assetItems.Length; ++i)
            {
                AssetItem assetItem = assetItems[i];
                m_state.AddProjectItemToCache(GetID(assetItem), assetItem);
                if (assetItem.Parts != null)
                {
                    for (int j = 0; j < assetItem.Parts.Length; ++j)
                    {
                        PrefabPart prefabPart = assetItem.Parts[j];
                        if (prefabPart != null)
                        {
                            m_state.AddProjectItemToCache(GetID(prefabPart), assetItem);
                        }
                    }
                }
            }

            if (callback != null)
            {
                callback(error, m_state.ProjectInfo);
            }

            ao.Result = m_state.ProjectInfo;
            ao.Error = error;
            ao.IsCompleted = true;

            if (OpenProjectCompleted != null)
            {
                OpenProjectCompleted(error, m_state.ProjectInfo);
            }

            IsBusy = false;
        }

        public ProjectAsyncOperation<string> CopyProject(string project, string targetProject, ProjectEventHandler<string> callback = null)
        {
            ProjectAsyncOperation<string> pao = new ProjectAsyncOperation<string>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _CopyProject(project, targetProject, callback, pao));
            }
            else
            {
                IsBusy = true;
                _CopyProject(project, targetProject, callback, pao);
            }

            return pao;
        }

        private void _CopyProject(string project, string targetProject, ProjectEventHandler<string> callback, ProjectAsyncOperation<string> ao)
        {
            m_storage.CopyProject(project, targetProject, error =>
            {
                if (callback != null)
                {
                    callback(error, targetProject);
                }

                ao.Error = error;
                ao.Result = project;
                ao.IsCompleted = true;

                if (CopyProjectCompleted != null)
                {
                    CopyProjectCompleted(error, targetProject);
                }

                IsBusy = false;
            });
        }

        /// <summary>
        /// Delete Project
        /// </summary>
        /// <param name="project"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<string> DeleteProject(string project, ProjectEventHandler<string> callback = null)
        {
            ProjectAsyncOperation<string> pao = new ProjectAsyncOperation<string>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _DeleteProject(project, callback, pao));
            }
            else
            {
                IsBusy = true;
                _DeleteProject(project, callback, pao);
            }

            return pao;
        }

        private void _DeleteProject(string project, ProjectEventHandler<string> callback, ProjectAsyncOperation<string> ao)
        {
            if (m_state.ProjectInfo != null && project == m_state.ProjectInfo.Name)
            {
                CloseProject();
            }
            m_storage.DeleteProject(project, error =>
            {
                if (callback != null)
                {
                    callback(error, project);
                }

                ao.Error = error;
                ao.Result = project;
                ao.IsCompleted = true;

                if (DeleteProjectCompleted != null)
                {
                    DeleteProjectCompleted(error, project);
                }

                IsBusy = false;
            });
        }

        public ProjectAsyncOperation<string> ExportProject(string project, string targetPath, ProjectEventHandler<string> callback = null)
        {
            ProjectAsyncOperation<string> pao = new ProjectAsyncOperation<string>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _ExportProject(project, targetPath, callback, pao));
            }
            else
            {
                IsBusy = true;
                _ExportProject(project, targetPath, callback, pao);
            }

            return pao;
        }

        private void _ExportProject(string project, string targetPath, ProjectEventHandler<string> callback, ProjectAsyncOperation<string> ao)
        {
            m_storage.ExportProject(project, targetPath, error =>
            {
                if (callback != null)
                {
                    callback(error, project);
                }

                ao.Error = error;
                ao.Result = project;
                ao.IsCompleted = true;

                if (!error.HasError)
                {
                    if (ExportProjectCompleted != null)
                    {
                        ExportProjectCompleted(error, project);
                    }
                }

                IsBusy = false;
            });
        }

        public ProjectAsyncOperation<string> ImportProject(string project, string sourcePath, bool overwrite = false, ProjectEventHandler<string> callback = null)
        {
            ProjectAsyncOperation<string> pao = new ProjectAsyncOperation<string>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _ImportProject(project, sourcePath, overwrite, callback, pao));
            }
            else
            {
                IsBusy = true;
                _ImportProject(project, sourcePath, overwrite, callback, pao);
            }

            return pao;
        }

        private void _ImportProject(string project, string sourcePath, bool overwrite, ProjectEventHandler<string> callback, ProjectAsyncOperation<string> ao)
        {
            if (overwrite)
            {
                m_storage.DeleteProject(project, error =>
                {

                    ao.Error = error;
                    if (ao.HasError)
                    {
                        if (callback != null)
                        {
                            callback(error, project);
                        }

                        ao.Result = project;
                        ao.IsCompleted = true;
                    }
                    else
                    {
                        _ImportProject(project, sourcePath, callback, ao);
                    }
                });
            }
            else
            {
                _ImportProject(project, sourcePath, callback, ao);
            }

        }

        private void _ImportProject(string project, string sourcePath, ProjectEventHandler<string> callback, ProjectAsyncOperation<string> ao)
        {
            m_storage.ImportProject(project, sourcePath, error =>
            {
                if (callback != null)
                {
                    callback(error, project);
                }

                ao.Error = error;
                ao.Result = project;
                ao.IsCompleted = true;

                if (!error.HasError)
                {
                    if (ImportProjectCompleted != null)
                    {
                        ImportProjectCompleted(error, project);
                    }
                }

                IsBusy = false;
            });
        }

        /// <summary>
        /// Close Project
        /// </summary>
        public void CloseProject()
        {
            if (m_state.ProjectInfo != null)
            {
                UnloadUnregisterDestroy();
            }
            m_state.ProjectInfo = null;
            m_state.RootFolder = null;
            m_state.ProjectPath = null;

            ClearScene();
            m_state.LoadedScene = null;
            
            if (CloseProjectCompleted != null)
            {
                CloseProjectCompleted(new Error(Error.OK));
            }
        }

        public ProjectAsyncOperation<Preview[]> GetPreviews(AssetItem[] assetItems, ProjectEventHandler<Preview[]> callback)
        {
            ProjectAsyncOperation<Preview[]> ao = new ProjectAsyncOperation<Preview[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _GetPreviews(assetItems, callback, ao));
            }
            else
            {
                IsBusy = true;
                _GetPreviews(assetItems, callback, ao);
            }
            return ao;
        }

        private void _GetPreviews(AssetItem[] assetItems, ProjectEventHandler<Preview[]> callback, ProjectAsyncOperation<Preview[]> ao)
        {
            m_storage.GetPreviews(m_state.ProjectPath, assetItems.Select(f => f.ToString()).ToArray(), (error, previews) =>
            {
                if (error.HasError)
                {
                    callback(error, new Preview[0]);
                }
                else
                {
                    if (callback != null)
                    {
                        callback(error, previews);
                    }
                }

                ao.Error = error;
                ao.Result = previews;
                ao.IsCompleted = true;
                IsBusy = false;
            });
        }


        [Obsolete("Use GetPreviews method instead")]
        public ProjectAsyncOperation<AssetItem[]> GetAssetItems(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback)
        {
            ProjectAsyncOperation<AssetItem[]> ao = new ProjectAsyncOperation<AssetItem[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _GetAssetItems(assetItems, callback, ao));
            }
            else
            {
                IsBusy = true;
                _GetAssetItems(assetItems, callback, ao);
            }
            return ao;
        }

        [Obsolete("Use GetPreviews method instead")]
        private void _GetAssetItems(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback, ProjectAsyncOperation<AssetItem[]> ao)
        {
            m_storage.GetPreviews(m_state.ProjectPath, assetItems.Select(f => f.ToString()).ToArray(), (error, previews) =>
            {
                AssetItem[] result = null;
                if (error.HasError)
                {
                    callback(error, new AssetItem[0]);
                }
                else
                {
                    result = assetItems.ToArray();
                    for (int i = 0; i < result.Length; ++i)
                    {
                        AssetItem assetItem = result[i];
                        assetItem.Preview = previews[i];
                    }

                    if (callback != null)
                    {
                        callback(error, result);
                    }
                }

                ao.Error = error;
                ao.Result = result;
                ao.IsCompleted = true;

                IsBusy = false;
            });
        }

        /// <summary>
        /// Get Asset Items with previews
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<ProjectItem[]> GetAssetItems(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback)
        {
            ProjectAsyncOperation<ProjectItem[]> ao = new ProjectAsyncOperation<ProjectItem[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _GetAssetItems(folders, string.Empty, callback, ao));
            }
            else
            {
                IsBusy = true;
                _GetAssetItems(folders, string.Empty, callback, ao);
            }

            return ao;
        }

        public ProjectAsyncOperation<ProjectItem[]> GetAssetItems(ProjectItem[] folders, string searchPattern, ProjectEventHandler<ProjectItem[]> callback)
        {
            ProjectAsyncOperation<ProjectItem[]> ao = new ProjectAsyncOperation<ProjectItem[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _GetAssetItems(folders, searchPattern, callback, ao));
            }
            else
            {
                IsBusy = true;
                _GetAssetItems(folders, searchPattern, callback, ao);
            }

            return ao;
        }

        private void _GetAssetItems(ProjectItem[] folders, string searchPattern, ProjectEventHandler<ProjectItem[]> callback, ProjectAsyncOperation<ProjectItem[]> ao)
        {
            m_storage.GetPreviews(m_state.ProjectPath, folders.Select(f => f.ToString()).ToArray(), (error, result) =>
            {
                if (error.HasError)
                {
                    if (callback != null)
                    {
                        callback(error, new AssetItem[0]);
                    }

                    ao.Result = new AssetItem[0];
                    ao.Error = error;
                    ao.IsCompleted = true;

                    if (GetAssetItemsCompleted != null)
                    {
                        GetAssetItemsCompleted(error, new AssetItem[0]);
                    }

                    IsBusy = false;
                    return;
                }
                OnGetPreviewsCompleted(folders, searchPattern, ao, callback, error, result);
            });
        }

        private void OnGetPreviewsCompleted(ProjectItem[] folders, string searchPattern, ProjectAsyncOperation<ProjectItem[]> ao, ProjectEventHandler<ProjectItem[]> callback, Error error, Preview[][] result)
        {
            for (int i = 0; i < result.Length; ++i)
            {
                ProjectItem folder = folders[i];
                Preview[] previews = result[i];
                if (previews != null && previews.Length > 0)
                {
                    for (int j = 0; j < previews.Length; ++j)
                    {
                        Preview preview = previews[j];
                        ProjectItem assetItem;

                        if (m_state.TryGetProjectItemFromCache(GetID(preview), out assetItem))
                        {
                            if (assetItem.Parent == null)
                            {
                                Debug.LogErrorFormat("asset item {0} parent is null", assetItem.ToString());
                                continue;
                            }

                            if (assetItem.Parent.ItemID != folder.ItemID)
                            {
                                Debug.LogErrorFormat("asset item {0} with wrong parent selected. Expected parent {1}. Actual parent {2}", folder.ToString(), assetItem.Parent.ToString());
                                continue;
                            }

                            assetItem.SetPreview(preview != null ? preview.PreviewData : null);
                        }
                        else
                        {
                            Debug.LogWarningFormat("AssetItem with ItemID {0} does not exist", GetID(preview));
                        }
                    }
                }
            }

            if (searchPattern == null)
            {
                searchPattern = string.Empty;
            }

            ProjectItem[] projectItems = folders.Where(f => f.Children != null).SelectMany(f => f.Children).Where(item => item.Name.ToLower().Contains(searchPattern.ToLower())).ToArray();
            if (callback != null)
            {
                callback(error, projectItems);
            }

            ao.Error = error;
            ao.Result = projectItems;
            ao.IsCompleted = true;

            if (GetAssetItemsCompleted != null)
            {
                GetAssetItemsCompleted(error, projectItems);
            }

            IsBusy = false;
        }

        private void PersistentDescriptorsToPrefabPartItems(PersistentDescriptor<TID>[] descriptors, List<PrefabPart> prefabParts, bool includeRoot = false)
        {
            if (descriptors == null)
            {
                return;
            }

            for (int i = 0; i < descriptors.Length; ++i)
            {
                PersistentDescriptor<TID> descriptor = descriptors[i];

                if (descriptor != null)
                {
                    bool checkPassed = true;
                    Guid typeGuid = Guid.Empty;
                    Type persistentType = m_typeMap.ToType(descriptor.PersistentTypeGuid);
                    if (persistentType == null)
                    {
                        Debug.LogWarningFormat("Unable to resolve type with guid {0}", descriptor.PersistentTypeGuid);
                        checkPassed = false;
                    }
                    else
                    {
                        Type type;
                        if (persistentType.GetGenericTypeDefinition() != typeof(PersistentRuntimeSerializableObject<>))
                        {
                            type = m_typeMap.ToUnityType(persistentType);
                        }
                        else
                        {
                            type = m_typeMap.ToType(descriptor.RuntimeTypeGuid);
                        }

                        if (type == null)
                        {
                            Debug.LogWarningFormat("Unable to get unity type from persistent type {1}", persistentType.FullName);
                            checkPassed = false;
                        }
                        else
                        {
                            typeGuid = m_typeMap.ToGuid(type);
                            if (typeGuid == Guid.Empty)
                            {
                                Debug.LogWarningFormat("Unable convert type {0} to guid", type.FullName);
                                checkPassed = false;
                            }
                        }
                    }

                    if (checkPassed && includeRoot)
                    {
                        PrefabPart prefabPartItem = new PrefabPart();
                        SetID(prefabPartItem, descriptor.PersistentID);
                        SetParentID(prefabPartItem, descriptor.Parent != null ? descriptor.Parent.PersistentID : m_assetDB.NullID);
                        prefabPartItem.Name = descriptor.Name;
                        prefabPartItem.TypeGuid = typeGuid;
                        prefabParts.Add(prefabPartItem);
                    }

                    PersistentDescriptorsToPrefabPartItems(descriptor.Children, prefabParts, true);
                    PersistentDescriptorsToPrefabPartItems(descriptor.Components, prefabParts, true);
                }
            }
        }

        /// <summary>
        /// Get Dependecies from object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<object[]> GetDependencies(object obj, bool exceptMappedObjects, ProjectEventHandler<object[]> callback)
        {
            ProjectAsyncOperation<object[]> ao = new ProjectAsyncOperation<object[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _GetDependencies(obj, exceptMappedObjects, callback, ao));
            }
            else
            {
                IsBusy = true;
                _GetDependencies(obj, exceptMappedObjects, callback, ao);
            }

            return ao;
        }

        private void _GetDependencies(object obj, bool exceptMappedObject, ProjectEventHandler<object[]> callback, ProjectAsyncOperation<object[]> ao)
        {
            Type objType = obj.GetType();
            Type persistentType = m_typeMap.ToPersistentType(objType);
            if (persistentType == null)
            {
                Debug.LogWarningFormat(string.Format("PersistentClass for {0} does not exist. To create or edit persistent classes click Tools->Runtime SaveLoad->Persistent Classes->Edit.", obj.GetType()), "obj");
                RaiseGetDependenciesCompleted(callback, ao, new object[0]);
                return;
            }

            LoadLibraryWithSceneDependencies(() =>
            {
                object[] dependencies = FindDeepDependencies(obj, exceptMappedObject);
                RaiseGetDependenciesCompleted(callback, ao, dependencies);
            });
        }

        private void RaiseGetDependenciesCompleted(ProjectEventHandler<object[]> callback, ProjectAsyncOperation<object[]> ao, object[] dependencies)
        {
            Error error = new Error(Error.OK);
            if (callback != null)
            {
                callback(error, dependencies);
            }

            ao.Error = error;
            ao.Result = dependencies;
            ao.IsCompleted = true;

            IsBusy = false;
        }

        public object[] FindDeepDependencies(object obj)
        {
            return FindDeepDependencies(obj, false);
        }

        private object[] FindDeepDependencies(object obj, bool exceptMappedObject)
        {
            Type objType = obj.GetType();
            Type persistentType = m_typeMap.ToPersistentType(objType);
            if (persistentType == null)
            {
                return null;
            }

            if (persistentType.GetGenericTypeDefinition() == typeof(PersistentGameObject<>))
            {
                persistentType = typeof(PersistentRuntimePrefab<TID>);
            }

            IPersistentSurrogate persistentObject = (IPersistentSurrogate)Activator.CreateInstance(persistentType);
            GetDepsFromContext ctx = new GetDepsFromContext();
            persistentObject.GetDepsFrom(obj, ctx);

            object[] deps = ctx.Dependencies.ToArray();
            ctx.Dependencies.Clear();

            for (int i = 0; i < deps.Length; ++i)
            {
                object dep = deps[i];

                if (dep is GameObject)
                {
                    continue;
                }
                else if (dep is Component)
                {
                    continue;
                }
                else if (dep is UnityObject)
                {
                    if (exceptMappedObject && m_assetDB.IsMapped((UnityObject)dep))
                    {
                        continue;
                    }
                    ctx.Dependencies.Add(dep);
                }
            }

            Queue<UnityObject> depsQueue = new Queue<UnityObject>(deps.OfType<UnityObject>());
            FindDeepDependencies(depsQueue, exceptMappedObject, ctx);

            object[] dependencies;
            if (exceptMappedObject)
            {
                dependencies = ctx.Dependencies.Where(d => d is UnityObject && !m_assetDB.IsMapped((UnityObject)d)).ToArray();
            }
            else
            {
                dependencies = ctx.Dependencies.ToArray();
            }

            return dependencies;
        }

        private void FindDeepDependencies(Queue<UnityObject> depsQueue, bool exceptMappedObject, GetDepsFromContext ctx)
        {
            GetDepsFromContext getDepsCtx = new GetDepsFromContext();
            while (depsQueue.Count > 0)
            {
                UnityObject uo = depsQueue.Dequeue();
                if (!uo)
                {
                    continue;
                }

                if (exceptMappedObject && m_assetDB.IsMapped(uo))
                {
                    continue;
                }

                if (!(uo is GameObject) && !(uo is Component))
                {
                    Type persistentType = m_typeMap.ToPersistentType(uo.GetType());
                    if (persistentType != null)
                    {
                        getDepsCtx.Clear();

                        IPersistentSurrogate persistentObject = (IPersistentSurrogate)Activator.CreateInstance(persistentType);
                        persistentObject.ReadFrom(uo);
                        persistentObject.GetDepsFrom(uo, getDepsCtx);

                        foreach (UnityObject dep in getDepsCtx.Dependencies)
                        {
                            if (!ctx.Dependencies.Contains(dep))
                            {
                                if (dep is GameObject)
                                {
                                    continue;
                                }
                                else if (dep is Component)
                                {
                                    continue;
                                }
                                else
                                {
                                    ctx.Dependencies.Add(dep);
                                }

                                depsQueue.Enqueue(dep);
                            }
                        }
                    }
                }
            }
        }

        public AssetItem[] GetDependantAssetItems(AssetItem[] assetItems)
        {
            HashSet<AssetItem> resultHs = new HashSet<AssetItem>();
            Queue<AssetItem> queue = new Queue<AssetItem>();
            for (int i = 0; i < assetItems.Length; ++i)
            {
                queue.Enqueue(assetItems[i]);
            }

            while (queue.Count > 0)
            {
                AssetItem assetItem = queue.Dequeue();
                if (resultHs.Contains(assetItem))
                {
                    continue;
                }
                resultHs.Add(assetItem);

                TID assetItemID = GetID(assetItem);

                foreach (AssetItem item in m_state.ProjectItemsCache)
                {
                    if (item.Dependencies != null)
                    {
                        TID[] dependencies = GetDependencies(item);
                        for (int i = 0; i < dependencies.Length; ++i)
                        {
                            if (IsEqual(assetItemID, dependencies[i]))
                            {
                                queue.Enqueue(item);
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < assetItems.Length; ++i)
            {
                AssetItem assetItem = assetItems[i];
                resultHs.Remove(assetItem);
            }
            return resultHs.ToArray();
        }

        public ProjectAsyncOperation<AssetItem[]> SaveScene(string name, ProjectEventHandler<AssetItem[]> callback = null)
        {
            return Save(new[] { Root }, new[] { new byte[0] }, new object[] { SceneManager.GetActiveScene() }, new[] { name }, callback);
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="assetItems"></param>
        /// <param name="obj"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<AssetItem[]> Save(AssetItem[] assetItems, object[] obj, ProjectEventHandler<AssetItem[]> callback = null)
        {
            return Save(assetItems, null, null, obj, null, true, callback);
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="parents"></param>
        /// <param name="previewData"></param>
        /// <param name="obj"></param>
        /// <param name="nameOverrides"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<AssetItem[]> Save(ProjectItem[] parents, byte[][] previewData, object[] obj, string[] nameOverrides, ProjectEventHandler<AssetItem[]> callback = null)
        {
            return Save(null, parents, previewData, obj, nameOverrides, true, callback);
        }

        public ProjectAsyncOperation<AssetItem[]> Save(ProjectItem[] parents, byte[][] previewData, object[] obj, string[] nameOverrides, bool isUserAction, ProjectEventHandler<AssetItem[]> callback = null)
        {
            return Save(null, parents, previewData, obj, nameOverrides, isUserAction, callback);
        }

        private ProjectAsyncOperation<AssetItem[]> Save(AssetItem[] existingAssetItems, ProjectItem[] parents, byte[][] previewData, object[] objects, string[] nameOverrides, bool isUserAction, ProjectEventHandler<AssetItem[]> callback)
        {
            ProjectAsyncOperation<AssetItem[]> ao = new ProjectAsyncOperation<AssetItem[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => { _Save(existingAssetItems, parents, previewData, objects, nameOverrides, isUserAction, callback, ao); });
            }
            else
            {
                IsBusy = true;
                _Save(existingAssetItems, parents, previewData, objects, nameOverrides, isUserAction, callback, ao);
            }

            return ao;
        }

        private void _Save(AssetItem[] existingAssetItems, ProjectItem[] parents, byte[][] previewData, object[] objects, string[] nameOverrides, bool isUserAction, ProjectEventHandler<AssetItem[]> callback, ProjectAsyncOperation<AssetItem[]> ao)
        {
            if (BeginSave != null)
            {
                BeginSave(new Error(), objects);
            }
            _Save(parents, previewData, objects, nameOverrides, existingAssetItems, callback, ao, () =>
            {
                if (gameObject.activeInHierarchy)
                {
                    if (SaveCompleted != null)
                    {
                        bool isNew = existingAssetItems == null;
                        SaveCompleted(ao.Error, ao.Result, isNew && isUserAction);
                    }
                    IsBusy = false;
                }
                else
                {
                    StartCoroutine(CoCallback(() =>
                    {
                        if (SaveCompleted != null)
                        {
                            bool isNew = existingAssetItems == null;
                            SaveCompleted(ao.Error, ao.Result, isNew);
                        }
                        IsBusy = false;
                    }));
                }
            });
        }

        protected void _Save(ProjectItem[] parents, byte[][] previewData, object[] objects, string[] nameOverrides, AssetItem[] existingAssetItems, ProjectEventHandler<AssetItem[]> callback, ProjectAsyncOperation<AssetItem[]> ao, Action done)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            if (objects == null)
            {
                throw new ArgumentNullException("objects");
            }

            if (existingAssetItems != null)
            {
                if (existingAssetItems.Length != objects.Length)
                {
                    throw new ArgumentException("existingAssetItems.Length != objects.Length", "existingAssetItems");
                }

                for (int i = 0; i < existingAssetItems.Length; ++i)
                {
                    object obj = objects[i];
                    if (!(obj is Scene))
                    {
                        if (!IsEqual(GetID(existingAssetItems[i]), m_assetDB.ToID((UnityObject)obj)))
                        {
                            throw new ArgumentException("Unable to override item with different object:  assetItem.ItemID != obj persistent id. Either delete the Asset Item, or load the Object before updating.");
                        }
                    }
                }
            }

            LoadLibraryWithSceneDependencies(() => DoSave(ao, parents, previewData, objects, nameOverrides, existingAssetItems, callback, done));
        }

        private void DoSave(ProjectAsyncOperation<AssetItem[]> ao, ProjectItem[] parents, byte[][] previewData, object[] objects, string[] nameOverrides, AssetItem[] existingAssetItems, ProjectEventHandler<AssetItem[]> callback, Action done)
        {
            if (parents == null)
            {
                if (existingAssetItems == null)
                {
                    parents = new ProjectItem[objects.Length];
                    for (int i = 0; i < parents.Length; ++i)
                    {
                        parents[i] = Root;
                    }
                }
            }
            else
            {
                for (int i = 0; i < parents.Length; ++i)
                {
                    if (!parents[i].IsFolder)
                    {
                        throw new ArgumentException("parent is not folder", "parent");
                    }
                }

                if (parents.Length == 0)
                {
                    Error error = new Error(Error.OK);

                    if (callback != null)
                    {
                        callback(error, new AssetItem[0]);
                    }

                    if (!ao.IsCompleted)
                    {
                        ao.Error = error;
                        ao.Result = new AssetItem[0];
                        ao.IsCompleted = true;
                    }

                    done();
                    return;
                }

                int parentIndex = parents.Length - 1;
                ProjectItem lastParent = parents[parentIndex];
                Array.Resize(ref parents, objects.Length);
                for (int i = parentIndex + 1; i < parents.Length; ++i)
                {
                    parents[i] = lastParent;
                }
            }

            List<UnityObject> notMapped = new List<UnityObject>();
            for (int o = 0; o < objects.Length; ++o)
            {
                object obj = objects[o];
                Type objType = obj.GetType();
                Type persistentType = m_typeMap.ToPersistentType(objType);
                if (persistentType == null)
                {
                    Debug.LogWarningFormat(string.Format("PersistentClass for {0} does not exist. Tools->Runtime SaveLoad->Persistent Classes->Edit", obj.GetType()), "obj");
                    continue;
                }

                AssetItem existingAssetItem = existingAssetItems != null ? existingAssetItems[o] : null;
                if (existingAssetItem == null || m_assetDB.IsDynamicResourceID(GetID(existingAssetItem)))
                {
                    if (existingAssetItem != null && obj is UnityObject)
                    {
                        /* object with correct identifier already exists. Only new prefab parts should be processed */
                        Debug.Assert(IsEqual(GetID(existingAssetItem), m_assetDB.ToID((UnityObject)obj)));
                    }

                    if (obj is GameObject)
                    {
                        GetUnmappedObjects((GameObject)obj, notMapped);
                    }
                    else if (obj is UnityObject)
                    {
                        if (!m_assetDB.IsMapped((UnityObject)obj))
                        {
                            notMapped.Add((UnityObject)obj);
                        }
                    }
                    else if (obj is Scene)
                    {
                        notMapped.Add(ScriptableObject.CreateInstance<ScriptableObject>());
                    }
                }
            }

            GenerateIdentifiers(notMapped.Count, (error, ids) =>
            {
                if (error.HasError)
                {
                    if (callback != null)
                    {
                        callback(error, null);
                    }

                    ao.Error = error;
                    ao.Result = null;
                    ao.IsCompleted = true;
                    done();
                    return;
                }

                for (int i = 0; i < notMapped.Count; ++i)
                {
                    m_assetDB.RegisterDynamicResource(ids[i], notMapped[i]);
                }

                AssetItem[] assetItems = existingAssetItems == null ? new AssetItem[objects.Length] : existingAssetItems;
                PersistentObject<TID>[] persistentObjects = new PersistentObject<TID>[objects.Length];
                Dictionary<ProjectItem, List<ProjectItem>> parentToPotentialChildren = null;
                if (parents != null)
                {
                    parentToPotentialChildren = new Dictionary<ProjectItem, List<ProjectItem>>();
                    for (int i = 0; i < parents.Length; ++i)
                    {
                        if (!parentToPotentialChildren.ContainsKey(parents[i]))
                        {
                            if (parents[i].Children == null)
                            {
                                parents[i].Children = new List<ProjectItem>();
                            }
                            parentToPotentialChildren.Add(parents[i], parents[i].Children.ToList());
                        }
                    }
                }

                for (int objIndex = 0; objIndex < objects.Length; ++objIndex)
                {
                    object obj = objects[objIndex];
                    Type objType = obj.GetType();
                    Type persistentType = m_typeMap.ToPersistentType(objType);
                    if (persistentType == null)
                    {
                        Debug.LogWarningFormat(string.Format("PersistentClass for {0} does not exist. Tools->Runtime SaveLoad->Persistent Classes->Edit", obj.GetType()), "obj");
                        continue;
                    }

                    if (persistentType.GetGenericTypeDefinition() == typeof(PersistentGameObject<>))
                    {
                        persistentType = typeof(PersistentRuntimePrefab<TID>);
                    }

                    string nameOverride = nameOverrides != null ? nameOverrides[objIndex] : null;
                    PersistentObject<TID> persistentObject = (PersistentObject<TID>)Activator.CreateInstance(persistentType);
                    persistentObject.ReadFrom(obj);

                    AssetItem assetItem = assetItems[objIndex];
                    if (assetItem == null)
                    {
                        if (!string.IsNullOrEmpty(nameOverride))
                        {
                            persistentObject.name = nameOverride;
                        }

                        List<ProjectItem> potentialChildren = parentToPotentialChildren[parents[objIndex]];
                        persistentObject.name = PathHelper.RemoveInvalidFileNameCharacters(persistentObject.name);
                        persistentObject.name = PathHelper.GetUniqueName(persistentObject.name, GetExt(obj), potentialChildren.Select(c => c.NameExt).ToList());
                        assetItem = new AssetItem<TID>();
                        if (obj is Scene)
                        {
                            SetID(assetItem, m_assetDB.ToID(notMapped[objIndex]));
                            Destroy(notMapped[objIndex]);
                        }
                        else
                        {
                            SetID(assetItem, m_assetDB.ToID((UnityObject)obj));
                        }

                        assetItem.Parent = parents[objIndex];
                        assetItem.Name = persistentObject.name;

                        assetItem.Ext = GetExt(obj);
                        assetItem.TypeGuid = m_typeMap.ToGuid(obj.GetType());
                        potentialChildren.Add(assetItem);
                    }
                    if (previewData != null)
                    {
                        Preview preview = new Preview
                        {
                            PreviewData = previewData[objIndex]
                        };
                        SetID(preview, GetID(assetItem));
                        assetItem.Preview = preview;
                    }

                    if (persistentObject is PersistentRuntimePrefab<TID> && !(persistentObject is PersistentRuntimeScene<TID>))
                    {
                        PersistentRuntimePrefab<TID> persistentPrefab = (PersistentRuntimePrefab<TID>)persistentObject;
                        if (persistentPrefab.Descriptors != null)
                        {
                            List<PrefabPart> prefabParts = new List<PrefabPart>();
                            PersistentDescriptorsToPrefabPartItems(persistentPrefab.Descriptors, prefabParts);
                            assetItem.Parts = prefabParts.ToArray();
                        }
                    }

                    GetDepsContext<TID> getDepsCtx = new GetDepsContext<TID>();
                    persistentObject.GetDeps(getDepsCtx);
                    SetDependencies(assetItem, getDepsCtx.Dependencies.ToArray());

                    persistentObjects[objIndex] = persistentObject;
                    assetItems[objIndex] = assetItem;
                }

                for (int i = 0; i < notMapped.Count; ++i)
                {
                    m_assetDB.UnregisterDynamicResource(ids[i]);
                }

                assetItems = assetItems.Where(ai => ai != null).ToArray();
                string[] path = assetItems.Select(ai => ai.Parent.ToString()).ToArray();
                persistentObjects = persistentObjects.Where(p => p != null).ToArray();

                m_storage.Save(m_state.ProjectPath, path, assetItems, persistentObjects, m_state.ProjectInfo, false, saveError =>
                {
                    if (!saveError.HasError)
                    {
                        for (int objIndex = 0; objIndex < assetItems.Length; ++objIndex)
                        {
                            AssetItem assetItem = assetItems[objIndex];
                            if (assetItem.Parts != null)
                            {
                                for (int i = 0; i < assetItem.Parts.Length; ++i)
                                {
                                    TID partID = GetID(assetItem.Parts[i]);
                                    if (!m_state.ContainsProjectItemWithKey(partID))
                                    {
                                        m_state.AddProjectItemToCache(partID, assetItem);
                                    }
                                }
                            }

                            TID assetItemID = GetID(assetItem);
                            if (!m_state.ContainsProjectItemWithKey(assetItemID))
                            {
                                m_state.AddProjectItemToCache(assetItemID, assetItem);
                            }

                            if (parents != null)
                            {
                                parents[objIndex].AddChild(assetItem);
                            }
                        }

                        if (callback != null)
                        {
                            callback(saveError, assetItems);
                        }

                        if (!ao.IsCompleted)
                        {
                            ao.Error = saveError;
                            ao.Result = assetItems;
                            ao.IsCompleted = true;
                        }
                        done();
                    }
                });

            });
        }

        protected void GetUnmappedObjects(GameObject go, List<UnityObject> notMapped)
        {
            if (go.GetComponent<RTSLIgnore>())
            {
                return;
            }

            if (!m_assetDB.IsMapped(go))
            {
                notMapped.Add(go);
            }

            Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; ++i)
            {
                Transform tf = transforms[i];
                if (tf.gameObject != go && !m_assetDB.IsMapped(tf.gameObject))
                {
                    notMapped.Add(tf.gameObject);
                }

                Component[] components = tf.GetComponents<Component>();
                for (int j = 0; j < components.Length; ++j)
                {
                    Component comp = components[j];
                    if (!m_assetDB.IsMapped(comp))
                    {
                        notMapped.Add(comp);
                    }
                }
            }
        }

        /// <summary>
        /// Save Preview
        /// </summary>
        /// <param name="assetItems"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<AssetItem[]> SavePreview(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null)
        {
            ProjectAsyncOperation<AssetItem[]> ao = new ProjectAsyncOperation<AssetItem[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _SavePreview(assetItems, callback, ao));
            }
            else
            {
                IsBusy = true;
                _SavePreview(assetItems, callback, ao);
            }
            return ao;
        }

        private void _SavePreview(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback, ProjectAsyncOperation<AssetItem[]> ao)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            m_storage.Save(m_state.ProjectPath, assetItems.Select(ai => ai.Parent.ToString()).ToArray(), assetItems, null, m_state.ProjectInfo, true, error =>
            {
                if (callback != null)
                {
                    callback(error, assetItems);
                }

                ao.Result = assetItems;
                ao.Error = error;
                ao.IsCompleted = true;

                if (SaveCompleted != null)
                {
                    const bool isNew = false;
                    SaveCompleted(error, assetItems, isNew);
                }

                IsBusy = false;
            });
        }


        [Obsolete]
        public ProjectAsyncOperation<AssetItem[]> Duplicate(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null)
        {
            ProjectAsyncOperation<AssetItem[]> ao = new ProjectAsyncOperation<AssetItem[]>();
            Duplicate((ProjectItem[])assetItems, (error, result) =>
            {
                if (result != null)
                {
                    assetItems = result.OfType<AssetItem>().ToArray();
                }

                if (callback != null)
                {
                    callback(error, assetItems);
                }

                ao.Error = error;
                ao.Result = assetItems;
                ao.IsCompleted = true;
            });
            return ao;
        }

        /// <summary>
        /// Duplicate
        /// </summary>
        /// <param name="projectItems"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<ProjectItem[]> Duplicate(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback = null)
        {
            ProjectAsyncOperation<ProjectItem[]> ao = new ProjectAsyncOperation<ProjectItem[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Duplicate(projectItems, callback, ao));
            }
            else
            {
                IsBusy = true;
                _Duplicate(projectItems, callback, ao);
            }

            return ao;
        }

        private void _Duplicate(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback, ProjectAsyncOperation<ProjectItem[]> ao)
        {
            List<ProjectItem> extendedProjectItems = new List<ProjectItem>();
            ProjectItem[] folders = projectItems.Where(item => item.IsFolder).ToArray();
            Dictionary<ProjectItem, List<string>> toSubfolderNames = new Dictionary<ProjectItem, List<string>>();
            foreach (ProjectItem folder in folders)
            {
                bool isRoot = true;
                foreach (ProjectItem parent in folders)
                {
                    if (folder != parent && folder.IsDescendantOf(parent))
                    {
                        isRoot = false;
                        break;
                    }
                }

                if (isRoot)
                {
                    foreach (ProjectItem projectItem in folder.Flatten(false))
                    {
                        extendedProjectItems.Add(projectItem);
                    }

                    if (!toSubfolderNames.ContainsKey(folder.Parent))
                    {
                        toSubfolderNames.Add(folder.Parent, new List<string>(folder.Parent.Children.Where(item => item.IsFolder).Select(item => item.Name)));
                    }
                }
            }

            foreach (ProjectItem item in projectItems.Where(item => !item.IsFolder))
            {
                if (!extendedProjectItems.Contains(item))
                {
                    extendedProjectItems.Add(item);
                }
            }

            folders = extendedProjectItems.Where(item => item.IsFolder).ToArray();
            int[] folderIndices = new int[folders.Length];
            for (int i = 0; i < folders.Length; ++i)
            {
                folderIndices[i] = extendedProjectItems.IndexOf(folders[i]);
            }
            DuplicateFolders(folders, toSubfolderNames, ao, callback, folderDuplicates =>
            {
                Dictionary<ProjectItem, ProjectItem> folderToDuplicate = new Dictionary<ProjectItem, ProjectItem>();
                for (int i = 0; i < folders.Length; ++i)
                {
                    folderToDuplicate.Add(folders[i], folderDuplicates[i]);
                }

                AssetItem[] scenes = extendedProjectItems.Where(item => IsScene(item)).OfType<AssetItem>().ToArray();
                int[] sceneIndices = new int[scenes.Length];
                for (int i = 0; i < scenes.Length; ++i)
                {
                    sceneIndices[i] = extendedProjectItems.IndexOf(scenes[i]);
                }

                DuplicateScenes(scenes, folderToDuplicate, ao, callback, (sceneDuplicates) =>
                {
                    AssetItem[] nonScenes = extendedProjectItems.Where(item => !item.IsFolder && !IsScene(item)).OfType<AssetItem>().ToArray();
                    m_storage.GetPreviews(m_state.ProjectPath, nonScenes.Select(item => item.ToString()).ToArray(), (Error getPreviewsError, Preview[] preivews) =>
                    {
                        if (getPreviewsError.HasError)
                        {
                            if (callback != null)
                            {
                                callback(getPreviewsError, null);
                            }
                            ao.Error = getPreviewsError;
                            ao.IsCompleted = true;
                            RaiseDuplicateCompleted(ao);
                            return;
                        }

                        int[] nonSceneIndices = new int[nonScenes.Length];
                        for (int i = 0; i < nonScenes.Length; ++i)
                        {
                            nonScenes[i].Preview = preivews[i];
                            nonSceneIndices[i] = extendedProjectItems.IndexOf(nonScenes[i]);
                        }

                        ProjectAsyncOperation<UnityObject[]> loadAo = new ProjectAsyncOperation<UnityObject[]>();
                        _Load(nonScenes, (loadError, loadedObjects) =>
                        {
                            if (loadError.HasError)
                            {
                                if (callback != null)
                                {
                                    callback(loadError, null);
                                }
                                ao.Error = loadError;
                                ao.IsCompleted = true;
                                return;
                            }

                            for (int i = 0; i < loadedObjects.Length; ++i)
                            {
                                string name = loadedObjects[i].name;
                                GameObject go = loadedObjects[i] as GameObject;
                                bool wasActive = false;
                                if (go != null)
                                {
                                    wasActive = go.activeSelf;
                                    go.SetActive(false);
                                }

                                loadedObjects[i] = Instantiate(loadedObjects[i]);
                                loadedObjects[i].name = name;

                                if (go != null)
                                {
                                    go.SetActive(wasActive);
                                }
                            }

                            ProjectItem[] nonSceneParents = nonScenes.Select(ai => (folderToDuplicate.ContainsKey(ai.Parent) ? folderToDuplicate[ai.Parent] : ai.Parent)).ToArray();
                            ProjectAsyncOperation<AssetItem[]> saveAo = new ProjectAsyncOperation<AssetItem[]>();
                            _Save(nonSceneParents, nonScenes.Select(ai => ai.Preview.PreviewData).ToArray(), loadedObjects, null, null, (saveError, nonSceneDuplicates) =>
                            {
                                for (int i = 0; i < loadedObjects.Length; ++i)
                                {
                                    Destroy(loadedObjects[i]);
                                }

                                ProjectItem[] result = null;
                                if (!saveError.HasError)
                                {
                                    result = new ProjectItem[folders.Length + sceneDuplicates.Length + nonSceneDuplicates.Length];
                                    for (int i = 0; i < folderIndices.Length; ++i)
                                    {
                                        result[folderIndices[i]] = folderDuplicates[i];
                                    }
                                    for (int i = 0; i < sceneIndices.Length; ++i)
                                    {
                                        result[sceneIndices[i]] = sceneDuplicates[i];
                                    }
                                    for (int i = 0; i < nonSceneIndices.Length; ++i)
                                    {
                                        result[nonSceneIndices[i]] = nonSceneDuplicates[i];
                                    }
                                }

                                if (callback != null)
                                {
                                    callback(saveError, result);
                                }

                                ao.Result = result;
                                ao.Error = saveError;
                                ao.IsCompleted = true;
                            },
                            saveAo,
                            () =>
                            {
                                RaiseDuplicateCompleted(ao);
                            });
                        },
                        loadAo, () =>
                        {
                            if (ao.Error != null && ao.HasError)
                            {
                                RaiseDuplicateCompleted(ao);
                            }
                        });
                    });
                });
            });
        }

        private void RaiseDuplicateCompleted(ProjectAsyncOperation<ProjectItem[]> ao)
        {
            if (DuplicateCompleted != null)
            {
                DuplicateCompleted(ao.Error, ao.Result?.OfType<AssetItem>().ToArray());
            }
            if (DuplicateItemsCompleted != null)
            {
                DuplicateItemsCompleted(ao.Error, ao.Result);
            }
            IsBusy = false;
        }

        private void DuplicateFolders(ProjectItem[] folders, Dictionary<ProjectItem, List<string>> toSubfolderName, ProjectAsyncOperation<ProjectItem[]> ao, ProjectEventHandler<ProjectItem[]> callback, Action<ProjectItem[]> done)
        {
            if (folders.Length == 0)
            {
                done(new ProjectItem[0]);
                return;
            }

            string[] paths = new string[folders.Length];
            string[] names = new string[folders.Length];
            ProjectItem[] result = new ProjectItem[folders.Length];
            Dictionary<ProjectItem, ProjectItem> m_toDuplicate = new Dictionary<ProjectItem, ProjectItem>();
            for (int i = 0; i < folders.Length; ++i)
            {
                ProjectItem folder = folders[i];
                ProjectItem parentFolder = folder.Parent;

                List<string> uniqueNames;
                if (toSubfolderName.TryGetValue(parentFolder, out uniqueNames))
                {
                    names[i] = PathHelper.GetUniqueName(folder.Name, uniqueNames);
                    uniqueNames.Add(names[i]);
                }
                else
                {
                    names[i] = folder.Name;
                }

                ProjectItem duplicatedFolder = new ProjectItem
                {
                    Name = names[i],
                    Parent = m_toDuplicate.ContainsKey(parentFolder) ? m_toDuplicate[parentFolder] : parentFolder,
                    Children = new List<ProjectItem>()
                };
                m_toDuplicate.Add(folder, duplicatedFolder);

                paths[i] = duplicatedFolder.Parent.ToString();
                result[i] = duplicatedFolder;
            }

            m_storage.Create(m_state.ProjectPath, paths, names, error =>
            {
                ao.Error = error;
                if (error.HasError)
                {
                    if (callback != null)
                    {
                        callback(ao.Error, result);
                    }

                    ao.IsCompleted = true;
                    RaiseDuplicateCompleted(ao);
                    return;
                }

                for (int i = 0; i < folders.Length; ++i)
                {
                    ProjectItem duplicatedFolder = result[i];
                    ProjectItem parentFolder = duplicatedFolder.Parent;

                    parentFolder.Children.Add(duplicatedFolder);
                    parentFolder.Children.Sort((item0, item1) => item0.Name.ToUpper().CompareTo(item1.Name.ToUpper()));
                }

                done(result);
            });
        }


        private AssetItem DuplicateScene(AssetItem assetItem, TID copyID)
        {
            AssetItem copy = new AssetItem<TID>();
            copy.TypeGuid = assetItem.TypeGuid;
            copy.Name = assetItem.Name;
            copy.Ext = assetItem.Ext;
            if (assetItem.Dependencies != null)
            {
                copy.Dependencies = assetItem.Dependencies.ToArray();
            }

            if (assetItem.Preview != null)
            {
                copy.Preview = new Preview
                {
                    PreviewData = assetItem.Preview.PreviewData != null ? assetItem.Preview.PreviewData.ToArray() : null
                };
            }

            SetID(copy, copyID);
            if (copy.Preview != null)
            {
                SetID(copy.Preview, copyID);
            }

            if (!m_state.ContainsProjectItemWithKey(copyID))
            {
                m_state.AddProjectItemToCache(copyID, copy);
            }

            return copy;
        }

        private void DuplicateScenes(AssetItem[] scenes, Dictionary<ProjectItem, ProjectItem> folderToDuplicate, ProjectAsyncOperation<ProjectItem[]> ao, ProjectEventHandler<ProjectItem[]> callback, Action<AssetItem[]> done)
        {
            if (scenes.Length == 0)
            {
                done(new AssetItem[0]);
                return;
            }

            GenerateIdentifiers(scenes.Length, (error, ids) =>
            {
                if (error.HasError)
                {
                    if (error.HasError)
                    {
                        if (callback != null)
                        {
                            callback(error, null);
                        }

                        ao.Error = error;
                        ao.Result = null;
                        ao.IsCompleted = true;
                        RaiseDuplicateCompleted(ao);
                        return;
                    }
                }

                AssetItem[] duplicates = new AssetItem[scenes.Length];
                List<string>[] names = scenes.Select(s => s.Parent.Children.Where(c => IsScene(c)).Select(c => c.Name).ToList()).ToArray();
                for (int i = 0; i < duplicates.Length; ++i)
                {
                    AssetItem copy = DuplicateScene(scenes[i], ids[i]);
                    if (copy == null)
                    {
                        return;
                    }

                    if (!folderToDuplicate.ContainsKey(scenes[i].Parent))
                    {
                        copy.Name = PathHelper.GetUniqueName(copy.Name, names[i]);
                        names[i].Add(copy.Name);
                    }

                    duplicates[i] = copy;
                }

                m_storage.Load(m_state.ProjectPath, scenes, scenes.Select(s => typeof(PersistentRuntimeScene<TID>)).ToArray(), (loadError, persistentObjects) =>
                {
                    if (loadError.HasError)
                    {
                        if (callback != null)
                        {
                            callback(loadError, null);
                        }
                        ao.Error = loadError;
                        ao.Result = null;
                        ao.IsCompleted = true;
                        RaiseDuplicateCompleted(ao);
                        return;
                    }

                    m_storage.Save(m_state.ProjectPath, scenes.Select(ai => (folderToDuplicate.ContainsKey(ai.Parent) ? folderToDuplicate[ai.Parent] : ai.Parent).ToString()).ToArray(), duplicates, persistentObjects, m_state.ProjectInfo, false, saveError =>
                    {
                        if (saveError.HasError)
                        {
                            if (callback != null)
                            {
                                callback(saveError, null);
                            }
                            ao.Error = saveError;
                            ao.Result = null;
                            ao.IsCompleted = true;
                            RaiseDuplicateCompleted(ao);
                            return;
                        }

                        for (int i = 0; i < scenes.Length; ++i)
                        {
                            (folderToDuplicate.ContainsKey(scenes[i].Parent) ? folderToDuplicate[scenes[i].Parent] : scenes[i].Parent).AddChild(duplicates[i]);
                        }

                        done(duplicates);
                    });
                });
            });
        }

        /// <summary>
        /// Load asset
        /// </summary>
        /// <param name="assetItem"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<UnityObject[]> Load(AssetItem[] assetItems, ProjectEventHandler<UnityObject[]> callback)
        {
            ProjectAsyncOperation<UnityObject[]> ao = new ProjectAsyncOperation<UnityObject[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Load(assetItems, callback, ao));
            }
            else
            {
                IsBusy = true;
                _Load(assetItems, callback, ao);
            }

            return ao;
        }

        private void _Load(AssetItem[] assetItems, ProjectEventHandler<UnityObject[]> callback, ProjectAsyncOperation<UnityObject[]> ao)
        {
            if (BeginLoad != null)
            {
                BeginLoad(new Error(), assetItems);
            }

            _Load(assetItems, callback, ao, () =>
            {
                if (Application.isPlaying)
                {
                    StartCoroutine(CoCallback(() =>
                    {
                        if (!ao.Error.HasError)
                        {
                            if (assetItems.Any(assetItem => ToType(assetItem) == typeof(Scene)))
                            {
                                m_state.LoadedScene = assetItems.First(assetItem => ToType(assetItem) == typeof(Scene));
                            }
                        }

                        if (LoadCompleted != null)
                        {
                            LoadCompleted(ao.Error, assetItems, ao.Result);
                        }

                        IsBusy = false;
                    }));
                }
                else
                {
                    if (!ao.Error.HasError)
                    {
                        if (assetItems.Any(assetItem => ToType(assetItem) == typeof(Scene)))
                        {
                            m_state.LoadedScene = assetItems.First(assetItem => ToType(assetItem) == typeof(Scene));
                        }
                    }

                    if (LoadCompleted != null)
                    {
                        LoadCompleted(ao.Error, assetItems, ao.Result);
                    }

                    IsBusy = false;
                }
            });
        }

        private void _Load(AssetItem[] assetItems, ProjectEventHandler<UnityObject[]> callback, ProjectAsyncOperation<UnityObject[]> ao, Action done)
        {
            HashSet<AssetItem> loadAssetItemsHs = new HashSet<AssetItem>();
            BeginResolveDependencies(assetItems, loadAssetItemsHs, () =>
            {
                OnDependenciesResolved(assetItems, loadAssetItemsHs, callback, ao, done);
            });
        }

        public void BeginResolveDependencies(AssetItem[] assetItems, HashSet<AssetItem> loadHs, Action callback)
        {
            HashSet<TID> unresolvedDependencies = new HashSet<TID>();
            GetAssetItemsToLoad(assetItems, loadHs, unresolvedDependencies);

            if (unresolvedDependencies.Count > 0)
            {
                ResolveDepenencies(callback, unresolvedDependencies);
            }
            else
            {
                callback();
            }
        }

        private void GetAssetItemsToLoad(AssetItem[] assetItems, HashSet<AssetItem> loadHs, HashSet<TID> unresolvedDependencies)
        {
            for (int a = 0; a < assetItems.Length; ++a)
            {
                AssetItem assetItem = assetItems[a];
                Type type = m_typeMap.ToType(assetItem.TypeGuid);
                if (type == null)
                {
                    continue;
                }
                Type persistentType = m_typeMap.ToPersistentType(type);
                if (persistentType == null)
                {
                    continue;
                }

                if (!loadHs.Contains(assetItem) && !m_assetDB.IsMapped(GetID(assetItem)))
                {
                    loadHs.Add(assetItem);
                    TID[] dependencies = GetDependencies(assetItem);
                    if (dependencies != null)
                    {
                        List<ProjectItem> deps = new List<ProjectItem>();

                        for (int i = 0; i < dependencies.Length; ++i)
                        {
                            TID dep = dependencies[i];

                            ProjectItem dependencyAssetItem;
                            if (m_state.TryGetProjectItemFromCache(dep, out dependencyAssetItem))
                            {
                                deps.Add(dependencyAssetItem);
                            }
                            else
                            {
                                if (!unresolvedDependencies.Contains(dep))
                                {
                                    unresolvedDependencies.Add(dep);
                                }
                            }
                        }

                        if (deps.Count > 0)
                        {
                            GetAssetItemsToLoad(deps.Cast<AssetItem>().ToArray(), loadHs, unresolvedDependencies);
                        }
                    }
                }
            }
        }

        private void OnDependenciesResolved(AssetItem[] rootAssetItems, HashSet<AssetItem> loadAssetItemsHs, ProjectEventHandler<UnityObject[]> callback, ProjectAsyncOperation<UnityObject[]> ao, Action done)
        {
            Type[] persistentTypes = loadAssetItemsHs.Select(item => m_typeMap.ToPersistentType(m_typeMap.ToType(item.TypeGuid))).ToArray();
            for (int i = 0; i < persistentTypes.Length; ++i)
            {
                Type type = persistentTypes[i];
                if (type == null)
                {
                    continue;
                }

                if (type.GetGenericTypeDefinition() == typeof(PersistentGameObject<>))
                {
                    persistentTypes[i] = typeof(PersistentRuntimePrefab<TID>);
                }
            }

            m_storage.Load(m_state.ProjectPath, loadAssetItemsHs.ToArray(), persistentTypes, (error, persistentObjects) =>
            {
                if (error.HasError)
                {
                    if (callback != null)
                    {
                        callback(error, null);
                    }

                    ao.Error = error;
                    ao.Result = new UnityObject[0];
                    ao.IsCompleted = true;

                    done();
                    return;
                }

                AssetItem[] assetItems = loadAssetItemsHs.ToArray();
                LoadAllAssetLibraries(assetItems.Select(ai => GetID(ai)).ToArray(), () =>
                {
                    OnLoadCompleted(rootAssetItems, assetItems, persistentObjects, ao, callback, done);
                });
            });
        }

        private void OnLoadCompleted(AssetItem[] rootAssetItems, AssetItem[] assetItems, PersistentObject<TID>[] persistentObjects, ProjectAsyncOperation<UnityObject[]> ao, ProjectEventHandler<UnityObject[]> callback, Action done)
        {
            for (int i = 0; i < assetItems.Length; ++i)
            {
                AssetItem assetItem = assetItems[i];
                TID assetItemId = GetID(assetItem);
                if (!m_assetDB.IsMapped(assetItemId))
                {
                    if (m_assetDB.IsDynamicResourceID(assetItemId))
                    {
                        PersistentObject<TID> persistentObject = persistentObjects[i];
                        if (persistentObject != null)
                        {
                            if (persistentObject is PersistentRuntimeScene<TID>)
                            {
                                continue;
                            }
                            else if (persistentObject is PersistentRuntimePrefab<TID>)
                            {
                                PersistentRuntimePrefab<TID> persistentPrefab = (PersistentRuntimePrefab<TID>)persistentObject;
                                Dictionary<TID, UnityObject> idToObj = new Dictionary<TID, UnityObject>();
                                List<GameObject> createdGameObjects = new List<GameObject>();
                                persistentPrefab.CreateGameObjectWithComponents(m_typeMap, persistentPrefab.Descriptors[0], idToObj, m_state.RuntimePrefabsRoot, createdGameObjects);
                                m_assetDB.RegisterDynamicResources(idToObj);
                                for (int j = 0; j < createdGameObjects.Count; ++j)
                                {
                                    GameObject createdGO = createdGameObjects[j];
                                    createdGO.hideFlags = HideFlags.HideAndDontSave;
                                }
                            }
                            else
                            {
                                Type type = m_typeMap.ToType(assetItem.TypeGuid);
                                if (type != null)
                                {
                                    if (m_factory.CanCreateInstance(type, persistentObject))
                                    {
                                        UnityObject instance = m_factory.CreateInstance(type, persistentObject);
                                        if (instance != null)
                                        {
                                            m_assetDB.RegisterDynamicResource(GetID(assetItem), instance);
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarning("Unable to create object of type " + type.ToString());
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning("Unable to resolve type by guid " + assetItem.TypeGuid);
                                }
                            }
                        }
                    }
                }
            }

            IPersistentSurrogate sceneObject = null;
            for (int i = 0; i < persistentObjects.Length; ++i)
            {
                AssetItem assetItem = assetItems[i];
                IPersistentSurrogate persistentObject = persistentObjects[i];
                if (persistentObject != null)
                {
                    if (persistentObject is PersistentRuntimeScene<TID>)
                    {
                        sceneObject = persistentObject;
                    }
                    else
                    {
                        TID assetItemID = GetID(assetItem);
                        UnityObject obj = m_assetDB.FromID<UnityObject>(assetItemID);
                        if (obj != null)
                        {
                            persistentObject.WriteTo(obj);
                            obj.name = assetItem.Name;

                            if (m_assetDB.IsDynamicResourceID(assetItemID))
                            {
                                obj.hideFlags = HideFlags.HideAndDontSave;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Unable to find UnityEngine.Object for " + assetItems[i].ToString() + " with id: " + GetID(assetItems[i]) + ". This typically means that asset or corresponding asset library was removed (or its ordinal was changed).");
                        }
                    }
                }
            }

            if (sceneObject != null)
            {
                sceneObject.WriteTo(SceneManager.GetActiveScene());
            }

            Error error = new Error(Error.OK);
            UnityObject[] result = rootAssetItems.Select(rootItem => m_assetDB.FromID<UnityObject>(GetID(rootItem))).ToArray();
            if (callback != null)
            {
                callback(error, result);
            }
            ao.Error = error;
            ao.Result = result;
            ao.IsCompleted = true;

            done();
        }

        public void Unload(AssetItem[] assetItems)
        {
            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Unload(assetItems));
            }
            else
            {
                IsBusy = true;
                _Unload(assetItems);
            }
        }

        private void _Unload(AssetItem[] assetItems)
        {
            for (int i = 0; i < assetItems.Length; ++i)
            {
                AssetItem assetItem = assetItems[i];
                TID assetItemID = GetID(assetItem);
                if (!m_assetDB.IsDynamicResourceID(assetItemID))
                {
                    Debug.LogWarning("Unable to unload " + assetItem.ToString() + ". It is possible to unload dynamic resources only"); ;
                    continue;
                }

                if (assetItem.Parts != null)
                {
                    for (int p = 0; p < assetItem.Parts.Length; ++p)
                    {
                        TID partID = GetID(assetItem.Parts[p]);
                        m_assetDB.UnregisterDynamicResource(partID);
                    }
                }

                if (m_assetDB.IsDynamicResourceID(assetItemID))
                {
                    UnityObject obj = m_assetDB.FromID<UnityObject>(assetItemID);
                    m_assetDB.UnregisterDynamicResource(assetItemID);
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }
            IsBusy = false;
        }

        /// <summary>
        /// Unload everything
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation Unload(ProjectEventHandler callback = null)
        {
            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Unload(callback, ao));
            }
            else
            {
                IsBusy = true;
                _Unload(callback, ao);
            }

            return ao;
        }

        private void _Unload(ProjectEventHandler callback, ProjectAsyncOperation ao)
        {
            UnloadUnregisterDestroy();
            m_assetDB.UnloadUnusedAssets(op =>
            {
                if (callback != null)
                {
                    callback(new Error());
                }

                ao.IsCompleted = true;
                ao.Error = new Error();

                if (UnloadCompleted != null)
                {
                    UnloadCompleted(new Error());
                }

                IsBusy = false;
            });
        }

        /// <summary>
        /// Load asset library to import
        /// </summary>
        /// <param name="libraryName"></param>
        /// <param name="isBuiltIn"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<ProjectItem> LoadImportItems(string libraryName, bool isBuiltIn, ProjectEventHandler<ProjectItem> callback = null)
        {
            ProjectAsyncOperation<ProjectItem> ao = new ProjectAsyncOperation<ProjectItem>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _LoadImportItems(libraryName, isBuiltIn, callback, ao));
            }
            else
            {
                IsBusy = true;
                _LoadImportItems(libraryName, isBuiltIn, callback, ao);
            }

            return ao;
        }


        /// <summary>
        /// Import assets
        /// </summary>
        /// <param name="importItems"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<AssetItem[]> Import(ProjectItem[] importItems, ProjectEventHandler<AssetItem[]> callback)
        {
            for (int i = 0; i < importItems.Length; ++i)
            {
                if (importItems[i].GetPreview() == null)
                {
                    Debug.LogWarning("Preview is null. Import item: " + importItems[i].Name + " Id: " + importItems[i].ItemID);
                }

                //Debug.Assert(importItems[i].Object == null);
            }

            ProjectAsyncOperation<AssetItem[]> ao = new ProjectAsyncOperation<AssetItem[]>();

            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Import(importItems.OfType<ImportItem>().ToArray(), callback, ao));
            }
            else
            {
                IsBusy = true;
                _Import(importItems.OfType<ImportItem>().ToArray(), callback, ao);
            }

            return ao;
        }

        protected void RemoveAssetItem(AssetItem assetItem)
        {
            if (assetItem.Parent != null)
            {
                assetItem.Parent.RemoveChild(assetItem);
            }
            TID assetItemID = GetID(assetItem);
            m_state.RemoveProjectItemFromCache(assetItemID);
            if (assetItem.Parts != null)
            {
                for (int p = 0; p < assetItem.Parts.Length; ++p)
                {
                    TID partID = GetID(assetItem.Parts[p]);
                    m_assetDB.UnregisterDynamicResource(partID);

                    ProjectItem partAssetItem;
                    if (m_state.TryGetProjectItemFromCache(partID, out partAssetItem))
                    {
                        Debug.Assert(assetItem == partAssetItem);
                        m_state.RemoveProjectItemFromCache(partID);
                    }
                }
            }

            if (m_assetDB.IsDynamicResourceID(assetItemID))
            {
                UnityObject obj = m_assetDB.FromID<UnityObject>(assetItemID);
                m_assetDB.UnregisterDynamicResource(assetItemID);
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }


        protected void RemoveFolder(ProjectItem projectItem)
        {
            if (projectItem.Children != null)
            {
                for (int i = projectItem.Children.Count - 1; i >= 0; --i)
                {
                    ProjectItem child = projectItem.Children[i];
                    if (child is AssetItem)
                    {
                        RemoveAssetItem((AssetItem)child);
                    }
                    else
                    {
                        RemoveFolder(child);
                    }
                }
            }

            if (projectItem.Parent != null)
            {
                projectItem.Parent.RemoveChild(projectItem);
            }
        }

        
        public ProjectAsyncOperation CreatePrefab(string folderPath, GameObject prefab, bool includeDeps, Func<UnityObject, byte[]> createPreview = null)
        {
            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            if(string.IsNullOrEmpty(folderPath))
            {
                folderPath = "/";
            }

            ProjectItem folder = Root.GetOrCreateFolder(folderPath);
            if (folder is AssetItem || folder == null)
            {
                throw new ArgumentException("folderPath");
            }

            if (includeDeps)
            {
                GetDependencies(prefab, true, (error, deps) =>
                {
                    object[] objects;
                    if (!deps.Contains(prefab))
                    {
                        objects = new object[deps.Length + 1];
                        objects[deps.Length] = prefab;
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
                    objects = objects.Where(obj => uoFactory.CanCreateInstance(obj.GetType())).ToArray();

                    byte[][] previewData = new byte[objects.Length][];
                    if (createPreview != null)
                    {
                        for (int i = 0; i < objects.Length; ++i)
                        {
                            if (objects[i] is UnityObject)
                            {
                                previewData[i] = createPreview((UnityObject)objects[i]);
                            }
                        }
                    }

                    Save(new[] { folder }, previewData, objects, null, (saveErr, assetItems) =>
                    {
                        ao.Error = saveErr;
                        ao.IsCompleted = true;
                    });
                });
            }
            else
            {
                byte[][] previewData = new byte[1][];
                if (createPreview != null)
                {
                    previewData[0] = createPreview(prefab);
                }

                Save(new[] { folder }, previewData, new[] { prefab }, null, (saveErr, assetItems) =>
                {
                    ao.Error = saveErr;
                    ao.IsCompleted = true;
                });
            }

            return ao;
        }

        /// <summary>
        /// Create folder
        /// </summary>
        /// <param name="projectItem"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<ProjectItem> CreateFolder(ProjectItem projectItem, ProjectEventHandler<ProjectItem> callback = null)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }
            if (!projectItem.IsFolder)
            {
                throw new InvalidOperationException("is not a folder");
            }

            ProjectAsyncOperation<ProjectItem> ao = new ProjectAsyncOperation<ProjectItem>();
            StartCoroutine(CoCreate(projectItem, ao, callback));
            return ao;
        }

        private IEnumerator CoCreate(ProjectItem projectItem, ProjectAsyncOperation<ProjectItem> ao, ProjectEventHandler<ProjectItem> callback = null)
        {
            ProjectAsyncOperation<ProjectItem[]> createAo = new ProjectAsyncOperation<ProjectItem[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Create(new[] { projectItem }, (err, projectItems) => { callback?.Invoke(err, projectItems.FirstOrDefault()); }, createAo));
            }
            else
            {
                IsBusy = true;
                _Create(new[] { projectItem }, (err, projectItems) => { callback?.Invoke(err, projectItems.FirstOrDefault()); }, createAo);
            };

            yield return createAo;
            ao.Error = createAo.Error;
            if (createAo.Result != null)
            {
                ao.Result = createAo.Result.FirstOrDefault();
            }
            ao.IsCompleted = true;
        }

        private void _Create(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback, ProjectAsyncOperation<ProjectItem[]> ao)
        {
            m_storage.Create(m_state.ProjectPath, projectItems.Select(projectItem => projectItem.Parent.ToString()).ToArray(), projectItems.Select(projectItem => projectItem.NameExt).ToArray(), error =>
            {
                if (callback != null)
                {
                    callback(error, projectItems);
                }

                ao.Result = projectItems;
                ao.Error = error;
                ao.IsCompleted = true;

                if (CreateCompleted != null)
                {
                    CreateCompleted(error, projectItems);
                }

                IsBusy = false;
            });
        }

        public ProjectAsyncOperation<ProjectItem[]> CreateFolders(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback = null)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }
            if (projectItems.Any(projectItem => !projectItem.IsFolder))
            {
                throw new InvalidOperationException("is not a folder");
            }

            ProjectAsyncOperation<ProjectItem[]> ao = new ProjectAsyncOperation<ProjectItem[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Create(projectItems, callback, ao));
            }
            else
            {
                IsBusy = true;
                _Create(projectItems, callback, ao);
            }
            return ao;
        }

        /// <summary>
        /// Rename asset or folder
        /// </summary>
        /// <param name="projectItem"></param>
        /// <param name="oldName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<ProjectItem> Rename(ProjectItem projectItem, string oldName, ProjectEventHandler<ProjectItem> callback)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            ProjectAsyncOperation<ProjectItem> ao = new ProjectAsyncOperation<ProjectItem>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Rename(projectItem, oldName, callback, ao));
            }
            else
            {
                IsBusy = true;
                _Rename(projectItem, oldName, callback, ao);
            }
            return ao;
        }

        private void _Rename(ProjectItem projectItem, string oldName, ProjectEventHandler<ProjectItem> callback, ProjectAsyncOperation<ProjectItem> ao)
        {
            UnityObject obj = m_assetDB.FromID<UnityObject>(GetID(projectItem));
            if (obj != null)
            {
                obj.name = projectItem.Name;
            }

            m_storage.Rename(m_state.ProjectPath, new[] { projectItem.Parent.ToString() }, new[] { oldName + projectItem.Ext }, new[] { projectItem.NameExt }, error =>
            {
                if (callback != null)
                {
                    callback(error, projectItem);
                }

                ao.Result = projectItem;
                ao.Error = error;
                ao.IsCompleted = true;

                if (RenameCompleted != null)
                {
                    RenameCompleted(error, projectItem);
                }

                IsBusy = false;
            });
        }

        /// <summary>
        /// Move assets and folders
        /// </summary>
        /// <param name="projectItems"></param>
        /// <param name="target"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<ProjectItem[], ProjectItem[]> Move(ProjectItem[] projectItems, ProjectItem target, ProjectEventHandler<ProjectItem[], ProjectItem[]> callback)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            ProjectAsyncOperation<ProjectItem[], ProjectItem[]> ao = new ProjectAsyncOperation<ProjectItem[], ProjectItem[]>();

            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Move(projectItems, target, callback, ao));
            }
            else
            {
                IsBusy = true;
                _Move(projectItems, target, callback, ao);
            }
            return ao;
        }

        private void _Move(ProjectItem[] projectItems, ProjectItem target, ProjectEventHandler<ProjectItem[], ProjectItem[]> callback, ProjectAsyncOperation<ProjectItem[], ProjectItem[]> ao)
        {
            ProjectItem[] oldParents = projectItems.Select(item => item.Parent).ToArray();
            m_storage.Move(m_state.ProjectPath, projectItems.Select(p => p.Parent.ToString()).ToArray(), projectItems.Select(p => p.NameExt).ToArray(), target.ToString(), error =>
            {
                if (!error.HasError)
                {
                    ProjectItem targetFolder = m_state.RootFolder.Get(target.ToString());

                    foreach (ProjectItem item in projectItems)
                    {
                        targetFolder.AddChild(item);
                    }
                }

                if (callback != null)
                {
                    callback(error, projectItems, oldParents);
                }

                ao.Result = projectItems;
                ao.Result2 = oldParents;
                ao.Error = error;
                ao.IsCompleted = true;

                if (MoveCompleted != null)
                {
                    MoveCompleted(error, projectItems, oldParents);
                }

                IsBusy = false;
            });
        }

        public ProjectAsyncOperation Delete(string projectPath, string[] projectItemsPath, ProjectEventHandler callback)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }
            ProjectAsyncOperation<ProjectItem[]> ao = new ProjectAsyncOperation<ProjectItem[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Delete(projectPath, projectItemsPath, callback, ao));
            }
            else
            {
                IsBusy = true;
                _Delete(projectPath, projectItemsPath, callback, ao);
            }
            return ao;
        }

        private void _Delete(string projectPath, string[] projectItemsPath, ProjectEventHandler callback, ProjectAsyncOperation ao)
        {
            m_storage.Delete(projectPath, projectItemsPath, error =>
            {
                if (callback != null)
                {
                    callback(error);
                }

                ao.Error = error;
                ao.IsCompleted = true;

                IsBusy = false;
            });
        }

        /// <summary>
        /// Delete assets and folders
        /// </summary>
        /// <param name="projectItems"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<ProjectItem[]> Delete(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }
            ProjectAsyncOperation<ProjectItem[]> ao = new ProjectAsyncOperation<ProjectItem[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _Delete(projectItems, callback, ao));
            }
            else
            {
                IsBusy = true;
                _Delete(projectItems, callback, ao);
            }
            return ao;
        }

        private void _Delete(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback, ProjectAsyncOperation<ProjectItem[]> ao)
        {
            m_storage.Delete(m_state.ProjectPath, projectItems.Select(p => p.ToString()).ToArray(), error =>
            {
                if (BeforeDeleteCompleted != null)
                {
                    BeforeDeleteCompleted(error, projectItems);
                }

                if (!error.HasError)
                {
                    foreach (ProjectItem item in projectItems)
                    {
                        if (item is AssetItem)
                        {
                            RemoveAssetItem((AssetItem)item);
                        }
                        else
                        {
                            RemoveFolder(item);
                        }
                    }
                }

                if (callback != null)
                {
                    callback(error, projectItems);
                }

                ao.Error = error;
                ao.Result = projectItems;
                ao.IsCompleted = true;

                if (DeleteCompleted != null)
                {
                    DeleteCompleted(error, projectItems);
                }

                IsBusy = false;
            });
        }

        /// <summary>
        /// Get List of all asset bundles could be imported and loaded
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ProjectAsyncOperation<string[]> GetAssetBundles(ProjectEventHandler<string[]> callback = null)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            ProjectAsyncOperation<string[]> ao = new ProjectAsyncOperation<string[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _GetAssetBundles(callback, ao));
            }
            else
            {
                IsBusy = true;
                _GetAssetBundles(callback, ao);
            }

            return ao;
        }

        private void _GetAssetBundles(ProjectEventHandler<string[]> callback, ProjectAsyncOperation<string[]> ao)
        {
            IOC.Resolve<IAssetBundleLoader>().GetAssetBundles(result =>
            {
                Error error = new Error(Error.OK);
                if (callback != null)
                {
                    callback(error, result);
                }

                ao.Error = error;
                ao.Result = result;
                ao.IsCompleted = true;

                IsBusy = false;
            });
        }
        
        public ProjectAsyncOperation<T[]> GetValues<T>(string searchPattern, ProjectEventHandler<T[]> callback = null)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            ProjectAsyncOperation<T[]> ao = new ProjectAsyncOperation<T[]>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _GetValues(searchPattern, callback, ao));
            }
            else
            {
                IsBusy = true;
                _GetValues(searchPattern, callback, ao);
            }

            return ao;
        }

        private void _GetValues<T>(string searchPattern, ProjectEventHandler<T[]> callback, ProjectAsyncOperation<T[]> ao)
        {
            Type objType = typeof(T);
            Type persistentType = m_typeMap.ToPersistentType(objType);
                 
            m_storage.GetValues(m_state.ProjectPath, searchPattern, persistentType != null ? persistentType : objType, (getValuesError, values) =>
            {
                if (getValuesError.HasError)
                {
                    RaiseGetValuesCompleted(new T[0], callback, ao, getValuesError);
                    return;
                }

                T[] result = new T[values.Length];
                if (persistentType != null)
                {
                    if (objType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        for (int i = 0; i < values.Length; ++i)
                        {
                            IPersistentSurrogate persistentObj = (IPersistentSurrogate)values[i];
                            result[i] = (T)Convert.ChangeType(ScriptableObject.CreateInstance(objType), objType);
                            persistentObj.WriteTo(result[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < values.Length; ++i)
                        {
                            IPersistentSurrogate persistentObj = (IPersistentSurrogate)values[i];
                            result[i] = (T)persistentObj.Instantiate(typeof(T));
                            persistentObj.WriteTo(result[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < values.Length; ++i)
                    {
                        result[i] = (T)values[i];
                    }
                }

                RaiseGetValuesCompleted(result, callback, ao, getValuesError);
            });
        }

        private void RaiseGetValuesCompleted<T>(T[] result, ProjectEventHandler<T[]> callback, ProjectAsyncOperation<T[]> ao, Error error)
        {
            if (callback != null)
            {
                callback(error, result);
            }

            ao.Result = result;
            ao.Error = error;
            ao.IsCompleted = true;
            IsBusy = false;
        }


        public ProjectAsyncOperation<T> GetValue<T>(string key, ProjectEventHandler<T> callback = null)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            ProjectAsyncOperation<T> ao = new ProjectAsyncOperation<T>();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _GetValue(key, callback, ao));
            }
            else
            {
                IsBusy = true;
                _GetValue(key, callback, ao);
            }

            return ao;
        }

        private void _GetValue<T>(string key, ProjectEventHandler<T> callback, ProjectAsyncOperation<T> ao)
        {
            Type objType = typeof(T);
            Type persistentType = m_typeMap.ToPersistentType(objType);
            if (persistentType == null)
            {
                persistentType = objType;
            }

            m_storage.GetValue(m_state.ProjectPath, key + GetExt(objType), persistentType, (getValueError, value) =>
            {
                if (getValueError.HasError)
                {
                    RaiseGetValueCompleted(default(T), callback, ao, getValueError);
                    return;
                }

                object result = null;
                IPersistentSurrogate persistentObj = value as IPersistentSurrogate;
                if (persistentObj != null)
                {
                    if (objType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        result = ScriptableObject.CreateInstance(objType);
                    }
                    else
                    {
                        result = persistentObj.Instantiate(objType);
                    }
                    persistentObj.WriteTo(result);
                }
                else
                {
                    result = value;
                }
                
                RaiseGetValueCompleted((T)result, callback, ao, getValueError);
            });
        }

        private void RaiseGetValueCompleted<T>(T result, ProjectEventHandler<T> callback, ProjectAsyncOperation<T> ao, Error error)
        {
            if (callback != null)
            {
                callback(error, result);
            }

            ao.Result = result;
            ao.Error = error;
            ao.IsCompleted = true;
            IsBusy = false;
        }

        public ProjectAsyncOperation SetValue<T>(string key, T obj, ProjectEventHandler callback = null)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _SetValue(key, obj, callback, ao));
            }
            else
            {
                IsBusy = true;
                _SetValue(key, obj, callback, ao);
            }

            return ao;
        }

        private void _SetValue<T>(string key, T obj, ProjectEventHandler callback, ProjectAsyncOperation ao)
        {
            object value = obj;

            Type objType = obj.GetType();
            Type persistentType = m_typeMap.ToPersistentType(objType);
            if (persistentType != null)
            {
                IPersistentSurrogate persistentObj = (IPersistentSurrogate)Activator.CreateInstance(persistentType);
                persistentObj.ReadFrom(obj);
                value = persistentObj;
            }
                        
            m_storage.SetValue(m_state.ProjectPath, key + GetExt(objType), value, (setValueError) =>
            {
                if (setValueError.HasError)
                {
                    RaiseSetValueCompleted(callback, ao, setValueError);
                    return;
                }

                RaiseSetValueCompleted(callback, ao, new Error(Error.OK));
            });
        }

        private void RaiseSetValueCompleted(ProjectEventHandler callback, ProjectAsyncOperation ao, Error error)
        {
            if (callback != null)
            {
                callback(error);
            }

            ao.Error = error;
            ao.IsCompleted = true;

            IsBusy = false;
        }

        public ProjectAsyncOperation DeleteValue<T>(string key, ProjectEventHandler callback = null)
        {
            if (m_state.RootFolder == null)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProject method");
            }

            ProjectAsyncOperation ao = new ProjectAsyncOperation();
            if (IsBusy)
            {
                m_actionsQueue.Enqueue(() => _DeleteValue<T>(key, callback, ao));
            }
            else
            {
                IsBusy = true;
                _DeleteValue<T>(key, callback, ao);
            }
            return ao;
        }


        private void _DeleteValue<T>(string key, ProjectEventHandler callback, ProjectAsyncOperation ao)
        {
            Type objType = typeof(T);
            m_storage.DeleteValue(m_state.ProjectPath, key + GetExt(objType), (deleteValueError) =>
            {
                if (deleteValueError.HasError)
                {
                    RaiseDeleteValueCompleted(callback, ao, deleteValueError);
                    return;
                }

                RaiseDeleteValueCompleted(callback, ao, new Error(Error.OK));
            });
        }

        private void RaiseDeleteValueCompleted(ProjectEventHandler callback, ProjectAsyncOperation ao, Error error)
        {
            if (callback != null)
            {
                callback(error);
            }

            ao.Error = error;
            ao.IsCompleted = true;
            IsBusy = false;
        }
    }

    [Obsolete]
    public class Project : Project<long>, IProject
    {
        [SerializeField]
        private AssetLibrariesListAsset m_staticAssetLibraries = null;
        private Dictionary<int, string> m_ordinalToStaticAssetLibrary;
     
        [SerializeField]
        private string m_sceneDepsLibrary = null;

        [SerializeField]
        private string m_builtInLibrary = null;

        public override AssetBundle[] LoadedAssetBundles
        {
            get { return m_ordinalToAssetBundle.Values.ToArray(); }
        }

        private Dictionary<int, AssetBundleInfo> m_ordinalToAssetBundleInfo = new Dictionary<int, AssetBundleInfo>();
        private Dictionary<int, AssetBundle> m_ordinalToAssetBundle = new Dictionary<int, AssetBundle>();

        private IAssetDB m_assetDB;
        
        public override void Awake_Internal()
        {
            base.Awake_Internal();

            m_assetDB = IOC.Resolve<IAssetDB>();
            
            if (string.IsNullOrEmpty(m_sceneDepsLibrary))
            {
                m_sceneDepsLibrary = "Scenes/" + SceneManager.GetActiveScene().name + "/SceneAssetLibrary";
            }

            if (string.IsNullOrEmpty(m_builtInLibrary))
            {
                m_builtInLibrary = "BuiltInAssets/BuiltInAssetLibrary";
            }
        }

        protected override void UnloadUnregister()
        {
            m_assetDB.UnloadLibraries();

            base.UnloadUnregister();
 
            m_ordinalToAssetBundleInfo.Clear();
            m_ordinalToAssetBundle.Clear();
        }

        protected override bool IsEqual(long id1, long id2)
        {
            return id1 == id2;
        }

        protected override long GetID(ProjectItem projectItem)
        {
            return projectItem.ItemID;
        }

        protected override void SetID(ProjectItem projectItem, long id)
        {
            projectItem.ItemID = id;
        }

        protected override long GetID(PrefabPart prefabPart)
        {
            return prefabPart.PartID;
        }

        protected override void SetID(PrefabPart prefabPart, long id)
        {
            prefabPart.PartID = id;
        }

        protected override void SetParentID(PrefabPart prefabPart, long id)
        {
            prefabPart.ParentID = id;
        }

        protected override long GetID(Preview preview)
        {
            return preview.ItemID;
        }

        protected override void SetID(Preview preview, long id)
        {
            preview.ItemID = id;
        }

        protected override long[] GetDependencies(AssetItem assetItem)
        {
            return assetItem.Dependencies;
        }

        protected override void SetDependencies(AssetItem assetItem, long[] dependencies)
        {
            assetItem.Dependencies = dependencies;
        }

        protected override void GenerateIdentifiers(int count, Action<Error, long[]> callback)
        {
            int assetIdentifier = ProjectInfo.AssetIdentifier;
            long[] result = new long[count];
            for (int i = 0; i < count; ++i)
            {
                int ordinal;
                int id;
                if(GetOrdinalAndId(ref ProjectInfo.AssetIdentifier, out ordinal, out id))
                {
                    result[i] = m_assetDB.ToDynamicResourceID(ordinal, id);
                }
                else
                {
                    ProjectInfo.AssetIdentifier = assetIdentifier;
                    callback(new Error(Error.E_InvalidOperation), new long[0]);
                    return;
                }
            }

            callback(Error.NoError, result);
        }

        private bool GetOrdinalAndId(ref int identifier, out int ordinal, out int id)
        {
            ordinal = AssetLibraryInfo.DYNAMICLIB_FIRST + m_assetDB.ToOrdinal(identifier);
            if (ordinal > AssetLibraryInfo.DYNAMICLIB_LAST)
            {
                Debug.LogError("Unable to generate identifier. Allotted Identifiers range was exhausted");
                id = 0;
                return false;
            }

            id = identifier & AssetLibraryInfo.ORDINAL_MASK;
            identifier++;
            return true;
        }

        protected override void OnGetProjectCompleted(string project, ProjectInfo projectInfo, AssetBundleInfo[] assetBundleInfo, ProjectAsyncOperation<ProjectInfo> ao, ProjectEventHandler<ProjectInfo> callback)
        {
            m_ordinalToAssetBundleInfo = assetBundleInfo.ToDictionary(info => info.Ordinal);

            base.OnGetProjectCompleted(project, projectInfo, assetBundleInfo, ao, callback);
        }

        protected override void ResolveDepenencies(Action callback, HashSet<long> unresolvedDependencies)
        {
            HashSet<int> assetLibrariesToLoad = new HashSet<int>();
            foreach (long unresolvedDependency in unresolvedDependencies)
            {
                if (m_assetDB.IsStaticResourceID(unresolvedDependency))
                {
                    int ordinal = m_assetDB.ToOrdinal(unresolvedDependency);
                    if (!assetLibrariesToLoad.Contains(ordinal) && !m_assetDB.IsLibraryLoaded(ordinal))
                    {
                        assetLibrariesToLoad.Add(ordinal);
                    }
                }
            }

            DoLoadAssetLibraries(assetLibrariesToLoad, () =>
            {
                foreach (long unresolvedDependency in unresolvedDependencies)
                {
                    UnityObject obj = m_assetDB.FromID<UnityObject>(unresolvedDependency);
                    if (obj != null)
                    {
                        Guid typeGuid = m_typeMap.ToGuid(obj.GetType());
                        if (typeGuid != Guid.Empty)
                        {
                            AssetItem resolvedAssetItem = new AssetItem<long>();
                            SetID(resolvedAssetItem, unresolvedDependency);
                            resolvedAssetItem.Name = obj.name;
                            resolvedAssetItem.TypeGuid = typeGuid;

                            State.AddProjectItemToCache(unresolvedDependency, resolvedAssetItem);
                        }
                    }
                }
                callback();
            });
        }

        private void DoLoadAssetLibraries(HashSet<int> assetLibrariesToLoad, Action callback)
        {
            if (assetLibrariesToLoad.Count == 0)
            {
                callback();
            }
            else
            {
                Dictionary<int, string> ordinalToStaticLib = GetStaticAssetLibraries();

                int loadedLibrariesCount = 0;
                foreach (int ordinal in assetLibrariesToLoad)
                {
                    string assetLibraryName = null;
                    if (ordinalToStaticLib.ContainsKey(ordinal))
                    {
                        assetLibraryName = ordinalToStaticLib[ordinal];
                    }
                    else
                    {
                        if (m_assetDB.IsBuiltinLibrary(ordinal))
                        {
                            if (ordinal != AssetLibraryInfo.BUILTIN_FIRST)
                            {
                                assetLibraryName = m_builtInLibrary + ((ordinal - AssetLibraryInfo.BUILTIN_FIRST) + 1);
                            }
                            else
                            {
                                assetLibraryName = m_builtInLibrary;
                            }
                        }
                        else if (m_assetDB.IsSceneLibrary(ordinal))
                        {
                            if (ordinal != AssetLibraryInfo.SCENELIB_FIRST)
                            {
                                assetLibraryName = m_sceneDepsLibrary + ((ordinal - AssetLibraryInfo.SCENELIB_FIRST) + 1);
                            }
                            else
                            {
                                assetLibraryName = m_sceneDepsLibrary;
                            }
                        }
                        else if (m_assetDB.IsBundledLibrary(ordinal))
                        {
                            AssetBundleInfo assetBundleInfo = m_ordinalToAssetBundleInfo[ordinal];
                            assetLibraryName = assetBundleInfo.UniqueName;
                        }
                    }

                    if (!string.IsNullOrEmpty(assetLibraryName))
                    {
                        LoadLibrary(ordinal, true, true, done =>
                        {
                            if (!done)
                            {
                                Debug.LogWarning("Asset Library '" + assetLibraryName + "' was not loaded");
                            }
                            loadedLibrariesCount++;
                            if (assetLibrariesToLoad.Count == loadedLibrariesCount)
                            {
                                callback();
                            }
                        });
                    }
                    else
                    {
                        loadedLibrariesCount++;
                        if (assetLibrariesToLoad.Count == loadedLibrariesCount)
                        {
                            callback();
                        }
                    }
                }
            }
        }

        private void LoadLibrary(int ordinal, bool loadIIDtoPID, bool loadPIDtoObj, Action<bool> callback)
        {
            if (m_assetDB.IsLibraryLoaded(ordinal))
            {
                Debug.LogError("Already loaded");
                callback(false);
            }

            if (m_assetDB.IsStaticLibrary(ordinal))
            {
                Dictionary<int, string> staticAssetLibraries = GetStaticAssetLibraries();
                if (!staticAssetLibraries.ContainsKey(ordinal))
                {
                    Debug.LogError("Unable to load static library " + ordinal);
                    callback(false);
                    return;
                }
                m_assetDB.LoadLibrary(staticAssetLibraries[ordinal], ordinal, loadIIDtoPID, loadPIDtoObj, callback);
            }
            else if (m_assetDB.IsBuiltinLibrary(ordinal))
            {
                int num = ordinal - AssetLibraryInfo.BUILTIN_FIRST;
                string builtinLibraryName = m_builtInLibrary;
                if (num > 0)
                {
                    builtinLibraryName += (num + 1);
                }
                m_assetDB.LoadLibrary(builtinLibraryName, ordinal, loadIIDtoPID, loadPIDtoObj, callback);
            }
            else if (m_assetDB.IsSceneLibrary(ordinal))
            {
                int num = ordinal - AssetLibraryInfo.SCENELIB_FIRST;
                string sceneLibraryName = m_sceneDepsLibrary;
                if (num > 0)
                {
                    sceneLibraryName += (num + 1);
                }
                m_assetDB.LoadLibrary(sceneLibraryName, ordinal, loadIIDtoPID, loadPIDtoObj, callback);
            }
            else if (m_assetDB.IsBundledLibrary(ordinal))
            {
                AssetBundleInfo assetBundleInfo;
                if (!m_ordinalToAssetBundleInfo.TryGetValue(ordinal, out assetBundleInfo))
                {
                    throw new InvalidOperationException("asset bundle with ordinal = " + ordinal + " was not imported");
                }

                IAssetBundleLoader loader = IOC.Resolve<IAssetBundleLoader>();
                loader.Load(assetBundleInfo.UniqueName, assetBundle =>
                {
                    if (assetBundle == null)
                    {
                        Debug.LogError("Unable to load asset bundle " + assetBundleInfo.UniqueName);
                        callback(false);
                        return;
                    }

                    m_ordinalToAssetBundle.Add(ordinal, assetBundle);

                    AssetLibraryAsset assetLibraryAsset = ToAssetLibraryAsset(assetBundle, assetBundleInfo);
                    m_assetDB.AddLibrary(assetLibraryAsset, ordinal, loadIIDtoPID, loadPIDtoObj);
                    callback(true);
                });
            }
            else
            {
                throw new ArgumentException("could load static or bundled library", "ordinal");
            }
        }

        private AssetLibraryAsset ToAssetLibraryAsset(AssetBundle bundle, AssetBundleInfo info)
        {
            UnityObject[] allAssets = bundle.LoadAllAssets();
            for (int i = 0; i < allAssets.Length; ++i)
            {
                UnityObject asset = allAssets[i];
                if (asset is AssetLibraryAsset)
                {
                    AssetLibraryAsset assetLibraryAsset = (AssetLibraryAsset)asset;
                    assetLibraryAsset.Ordinal = info.Ordinal;

                }
            }

            AssetLibraryAsset result = ScriptableObject.CreateInstance<AssetLibraryAsset>();
            result.Ordinal = info.Ordinal;

            AssetLibraryInfo assetLib = result.AssetLibrary;
            AssetFolderInfo assetsFolder = assetLib.Folders[1];
            if (assetsFolder.children == null)
            {
                assetsFolder.children = new List<TreeElement>();
            }
            if (assetsFolder.Assets == null)
            {
                assetsFolder.Assets = new List<AssetInfo>();
            }
            int folderId = assetsFolder.id + 1;
            AssetBundleItemInfo[] assetBundleItems = info.AssetBundleItems.OrderBy(i => i.Path.Length).ToArray(); //components will have greater indices
            List<AssetInfo> assetsList = new List<AssetInfo>();
            for (int i = 0; i < assetBundleItems.Length; ++i)
            {
                AssetFolderInfo folder = assetsFolder;
                AssetBundleItemInfo bundleItem = assetBundleItems[i];
                string[] pathParts = bundleItem.Path.Split('/');
                int p = 1;
                for (; p < pathParts.Length; ++p)
                {
                    string pathPart = pathParts[p];
                    if (pathPart.Contains("."))
                    {
                        break;
                    }

                    AssetFolderInfo childFolder = (AssetFolderInfo)folder.children.FirstOrDefault(f => f.name == pathPart);
                    if (childFolder == null)
                    {
                        childFolder = new AssetFolderInfo(pathPart, folder.depth + 1, folderId);
                        childFolder.children = new List<TreeElement>();
                        childFolder.Assets = new List<AssetInfo>();
                        folderId++;
                        folder.children.Add(childFolder);
                    }
                    folder = childFolder;
                }

                if (pathParts.Length > 1)
                {
                    AssetInfo assetInfo = folder.Assets != null ? folder.Assets.Where(a => a.name == pathParts[p]).FirstOrDefault() : null;
                    if (assetInfo == null)
                    {
                        assetInfo = new AssetInfo(pathParts[p], 0, bundleItem.Id);
                        assetInfo.PrefabParts = new List<PrefabPartInfo>();

                        Debug.Assert(p == pathParts.Length - 1);
                        assetInfo.Object = bundle.LoadAsset(bundleItem.Path);

                        folder.Assets.Add(assetInfo);
                        assetsList.Add(assetInfo);
                    }
                    else
                    {
                        UnityObject prefab = assetInfo.Object;
                        if (prefab is GameObject)
                        {
                            GameObject go = (GameObject)prefab;
                            PrefabPartInfo prefabPart = new PrefabPartInfo();

                            string pathPart = pathParts[p + 1];
                            if (pathPart.Contains("@@@"))
                            {
                                string parentPath = string.Join("/", pathParts, 0, p + 1);

                                UnityObject[] subAssets = bundle.LoadAssetWithSubAssets(parentPath);

                                string[] subAssetPathParts = pathPart.Split(new[] { "@@@" }, StringSplitOptions.None);
                                string subAssetName = subAssetPathParts[0];
                                string subAssetTypeName = subAssetPathParts[1];

                                UnityObject subAsset = subAssets.Where(sa => sa.name == subAssetName && sa.GetType().FullName == subAssetTypeName).FirstOrDefault();
                                if (subAsset != null)
                                {
                                    prefabPart.Object = subAsset;
                                    prefabPart.PersistentID = bundleItem.Id;
                                    prefabPart.ParentPersistentID = bundleItem.ParentId;
                                    prefabPart.Depth = pathParts.Length - p;
                                    assetInfo.PrefabParts.Add(prefabPart);
                                }
                            }
                            else
                            {
                                prefabPart.Object = GetPrefabPartAtPath(go, pathParts, p + 1);
                                prefabPart.PersistentID = bundleItem.Id;
                                prefabPart.ParentPersistentID = bundleItem.ParentId;
                                prefabPart.Depth = pathParts.Length - p;
                                assetInfo.PrefabParts.Add(prefabPart);
                            }
                        }
                    }
                }
            }

            //fix names
            for (int i = 0; i < assetsList.Count; ++i)
            {
                assetsList[i].name = Path.GetFileNameWithoutExtension(assetsList[i].name);
            }

            //convert folders tree to assetLibraryInfo folders array;
            if (assetsFolder.hasChildren)
            {
                for (int i = 0; i < assetsFolder.children.Count; ++i)
                {
                    FoldersTreeToArray(assetLib, (AssetFolderInfo)assetsFolder.children[i]);
                }
            }

            return result;
        }

        private UnityObject GetPrefabPartAtPath(GameObject go, string[] path, int pathPartIndex)
        {
            string pathPart = path[pathPartIndex];
            if (pathPart.Contains("###"))
            {
                string[] nameAndNumber = pathPart.Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries);
                string name = nameAndNumber[0];
                int number;

                GameObject childGo = null;
                if (nameAndNumber.Length > 1 && int.TryParse(nameAndNumber[1], out number))
                {
                    int n = 1;
                    foreach (Transform child in go.transform)
                    {
                        if (child.name == name)
                        {
                            if (n == number)
                            {
                                childGo = child.gameObject;
                                break;
                            }
                            else
                            {
                                n++;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Transform child in go.transform)
                    {
                        if (child.name == name)
                        {
                            childGo = child.gameObject;
                            break;
                        }
                    }
                }

                if (childGo != null)
                {
                    if (pathPartIndex < path.Length - 1)
                    {
                        return GetPrefabPartAtPath(childGo, path, pathPartIndex + 1);
                    }
                }

                return childGo;
            }

            Debug.Assert(pathPartIndex == path.Length - 1);

            Component component = go.GetComponents<Component>().Where(c => c != null && c.GetType().FullName == path[pathPartIndex]).FirstOrDefault();
            return component;
        }

        private void FoldersTreeToArray(AssetLibraryInfo assetLibraryInfo, AssetFolderInfo folder)
        {
            assetLibraryInfo.Folders.Add(folder);
            if (folder.hasChildren)
            {
                for (int i = 0; i < folder.children.Count; ++i)
                {
                    FoldersTreeToArray(assetLibraryInfo, (AssetFolderInfo)folder.children[i]);
                }
            }
        }

        protected override void LoadLibraryWithSceneDependencies(Action callback)
        {
            LoadBuiltinLibrary(() =>
            {
                if (!m_assetDB.IsLibraryLoaded(AssetLibraryInfo.SCENELIB_FIRST))
                {
                    LoadLibraryWithSceneDependencies(m_sceneDepsLibrary, AssetLibraryInfo.SCENELIB_FIRST, callback);
                }
                else
                {
                    callback();
                }
            });
        }

        private void LoadBuiltinLibrary(Action callback)
        {
            if (!m_assetDB.IsLibraryLoaded(AssetLibraryInfo.BUILTIN_FIRST))
            {
                LoadBuiltinLibrary(m_builtInLibrary, AssetLibraryInfo.BUILTIN_FIRST, callback);
            }
            else
            {
                callback();
            }
        }

        private void LoadBuiltinLibrary(string name, int ordinal, Action callback)
        {
            string libraryName = ordinal == AssetLibraryInfo.BUILTIN_FIRST ? name : name + ((ordinal - AssetLibraryInfo.BUILTIN_FIRST) + 1);
            m_assetDB.LoadLibrary(libraryName, ordinal, true, true, done =>
            {
                if (!done)
                {
                    if (ordinal == AssetLibraryInfo.BUILTIN_FIRST)
                    {
                        Debug.LogWarning("Builtin library was not loaded");
                    }
                    callback();
                    return;
                }

                ordinal++;
                LoadBuiltinLibrary(name, ordinal, callback);
            });
        }

        private void LoadLibraryWithSceneDependencies(string name, int ordinal, Action callback)
        {
            string libraryName = ordinal == AssetLibraryInfo.SCENELIB_FIRST ? name : name + ((ordinal - AssetLibraryInfo.SCENELIB_FIRST) + 1);
            m_assetDB.LoadLibrary(libraryName, ordinal, true, true, done =>
            {
                if (!done)
                {
                    if (ordinal == AssetLibraryInfo.SCENELIB_FIRST)
                    {
                        Debug.LogWarning("Library with scene dependencies was not loaded");
                    }
                    callback();
                    return;
                }

                ordinal++;
                LoadLibraryWithSceneDependencies(name, ordinal, callback);
            });
        }

        protected override void LoadAllAssetLibraries(long[] deps, Action callback)
        {
            HashSet<int> assetLibrariesToLoad = new HashSet<int>();
            for (int i = 0; i < deps.Length; ++i)
            {
                long id = deps[i];
                if (!m_assetDB.IsMapped(id))
                {
                    if (m_assetDB.IsStaticResourceID(id))
                    {
                        int ordinal = m_assetDB.ToOrdinal(id);
                        if (!assetLibrariesToLoad.Contains(ordinal) && !m_assetDB.IsLibraryLoaded(ordinal))
                        {
                            assetLibrariesToLoad.Add(ordinal);
                        }
                    }
                }
            }

            DoLoadAssetLibraries(assetLibrariesToLoad, callback);
        }

        protected override void _LoadImportItems(string libraryName, bool isBuiltIn, ProjectEventHandler<ProjectItem> callback, ProjectAsyncOperation<ProjectItem> ao)
        {
            if (Root == null)
            {
                Error error = new Error(Error.E_InvalidOperation);
                error.ErrorText = "Unable to load asset library. Open project first";
                RaiseLoadAssetLibraryCallback(callback, ao, error);
                return;
            }

            if (isBuiltIn)
            {
                int ordinal = -1;

                Dictionary<int, string> ordinalToStaticAssetLibrary = GetStaticAssetLibraries();
                foreach (KeyValuePair<int, string> kvp in m_ordinalToStaticAssetLibrary)
                {
                    if (kvp.Value == libraryName)
                    {
                        ordinal = kvp.Key;
                        break;
                    }
                }

                if (ordinal < 0)
                {
                    Error error = new Error(Error.E_NotFound);
                    error.ErrorText = "Built-in asset library " + libraryName + " not found";
                    RaiseLoadAssetLibraryCallback(callback, ao, error);
                    return;
                }

                ResourceRequest request = Resources.LoadAsync<AssetLibraryAsset>(libraryName);
                Action<AsyncOperation> completed = null;
                completed = op =>
                {
                    request.completed -= completed;

                    AssetLibraryAsset asset = (AssetLibraryAsset)request.asset;

                    CompleteLoadAssetLibrary(libraryName, callback, ao, ordinal, asset);

                    IsBusy = false;
                };
                request.completed += completed;
                return;
            }
            else
            {
                if (ProjectInfo.BundleIdentifier >= AssetLibraryInfo.MAX_BUNDLEDLIBS - 1)
                {
                    Error error = new Error(Error.E_NotFound);
                    error.ErrorText = "Unable to load asset bundle. Bundle identifiers exhausted";
                    RaiseLoadAssetLibraryCallback(callback, ao, error);
                    return;
                }
                m_storage.Load(ProjectPath, libraryName, (loadError, assetBundleInfo) =>
                {
                    if (loadError.HasError && loadError.ErrorCode != Error.E_NotFound)
                    {
                        RaiseLoadAssetLibraryCallback(callback, ao, loadError);
                        return;
                    }

                    if (assetBundleInfo == null)
                    {
                        assetBundleInfo = new AssetBundleInfo();
                        assetBundleInfo.UniqueName = libraryName;
                        assetBundleInfo.Ordinal = AssetLibraryInfo.BUNDLEDLIB_FIRST + ProjectInfo.BundleIdentifier;
                        ProjectInfo.BundleIdentifier++;
                        m_ordinalToAssetBundleInfo.Add(assetBundleInfo.Ordinal, assetBundleInfo);
                    }

                    AssetBundle loadedAssetBundle;
                    if (m_ordinalToAssetBundle.TryGetValue(assetBundleInfo.Ordinal, out loadedAssetBundle))
                    {
                        Debug.Assert(m_assetDB.IsLibraryLoaded(assetBundleInfo.Ordinal));
                        LoadImportItemsFromAssetBundle(libraryName, callback, assetBundleInfo, loadedAssetBundle, ao);
                    }
                    else
                    {
                        IAssetBundleLoader loader = IOC.Resolve<IAssetBundleLoader>();
                        loader.Load(libraryName, assetBundle =>
                        {
                            LoadImportItemsFromAssetBundle(libraryName, callback, assetBundleInfo, assetBundle, ao);
                        });
                    }

                });
            }
        }

        private void LoadImportItemsFromAssetBundle(string libraryName, ProjectEventHandler<ProjectItem> callback, AssetBundleInfo assetBundleInfo, AssetBundle assetBundle, ProjectAsyncOperation<ProjectItem> ao)
        {
            if (assetBundle == null)
            {
                Error error = new Error(Error.E_NotFound);
                error.ErrorText = "Unable to load asset bundle";
                RaiseLoadAssetLibraryCallback(callback, ao, error);
                return;
            }

            GenerateIdentifiers(assetBundle, assetBundleInfo);
            if (assetBundleInfo.Identifier >= AssetLibraryInfo.MAX_ASSETS)
            {
                Error error = new Error(Error.E_NotFound);
                error.ErrorText = "Unable to load asset bundle. Asset identifier exhausted";
                RaiseLoadAssetLibraryCallback(callback, ao, error);
                return;
            }

            m_storage.Save(ProjectPath, assetBundleInfo, ProjectInfo, saveError =>
            {
                AssetLibraryAsset asset = ToAssetLibraryAsset(assetBundle, assetBundleInfo);
                CompleteLoadAssetLibrary(libraryName, callback, ao, assetBundleInfo.Ordinal, asset);

                if (!m_assetDB.IsLibraryLoaded(assetBundleInfo.Ordinal))
                {
                    assetBundle.Unload(false);
                }

                IsBusy = false;
            });
        }

        private void GenerateIdentifiers(AssetBundle bundle, AssetBundleInfo info)
        {
            Dictionary<string, AssetBundleItemInfo> pathToBundleItem = info.AssetBundleItems != null ? info.AssetBundleItems.ToDictionary(i => i.Path) : new Dictionary<string, AssetBundleItemInfo>();

            string[] assetNames = bundle.GetAllAssetNames();
            for (int i = 0; i < assetNames.Length; ++i)
            {
                string assetName = assetNames[i];
                AssetBundleItemInfo bundleItem;
                if (!pathToBundleItem.TryGetValue(assetName, out bundleItem))
                {
                    bundleItem = new AssetBundleItemInfo
                    {
                        Path = assetName,
                        Id = info.Identifier,
                    };
                    info.Identifier++;
                    pathToBundleItem.Add(bundleItem.Path, bundleItem);
                }

                UnityObject obj = bundle.LoadAsset<UnityObject>(assetName);
                if (obj is GameObject)
                {

                    GenerateIdentifiersForPrefab(assetName, (GameObject)obj, info, pathToBundleItem);

                    UnityObject[] subAssets = bundle.LoadAssetWithSubAssets<UnityObject>(assetName);
                    foreach (UnityObject subAsset in subAssets)
                    {
                        if (obj == subAsset)
                        {
                            continue;
                        }

                        //Add avatar or mesh as prefab part
                        if (subAsset is Avatar || subAsset is Mesh || subAsset is Material || subAsset is AnimationClip)
                        {
                            AssetBundleItemInfo subItem = new AssetBundleItemInfo
                            {
                                Id = info.Identifier,
                                ParentId = pathToBundleItem[bundleItem.Path].Id,
                                Path = assetName + "/" + subAsset.name + "@@@" + subAsset.GetType().FullName,
                            };
                            info.Identifier++;
                            pathToBundleItem[subItem.Path] = subItem;
                        }
                    }
                }
            }

            info.AssetBundleItems = pathToBundleItem.Values.ToArray();
        }

        private void GenerateIdentifiersForPrefab(string assetName, GameObject go, AssetBundleInfo info, Dictionary<string, AssetBundleItemInfo> pathToBundleItem)
        {
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i)
            {
                Component component = components[i];
                if (component != null)
                {
                    string componentName = assetName + "/" + component.GetType().FullName;
                    AssetBundleItemInfo bundleItem;
                    if (!pathToBundleItem.TryGetValue(componentName, out bundleItem)) //Multiple components of same type are not supported
                    {
                        bundleItem = new AssetBundleItemInfo
                        {
                            Path = componentName,
                            Id = info.Identifier,
                            ParentId = pathToBundleItem[assetName].Id
                        };
                        info.Identifier++;
                        pathToBundleItem.Add(bundleItem.Path, bundleItem);
                    }
                }
            }

            Dictionary<string, int> nameToIndex = new Dictionary<string, int>();
            foreach (Transform child in go.transform)
            {
                GameObject childGo = child.gameObject;
                string childName = assetName + "/" + childGo.name + "###";  //Children re-arrangement will lead to new (wrong) identifiers will be generated
                if (!nameToIndex.TryGetValue(childName, out int index))
                {
                    nameToIndex.Add(childName, 2);
                }

                if (index > 0)
                {
                    nameToIndex[childName]++;
                    childName += index;
                }

                AssetBundleItemInfo bundleItem;
                if (!pathToBundleItem.TryGetValue(childName, out bundleItem))
                {
                    bundleItem = new AssetBundleItemInfo
                    {
                        Path = childName,
                        Id = info.Identifier,
                        ParentId = pathToBundleItem[assetName].Id
                    };
                    info.Identifier++;
                    pathToBundleItem.Add(bundleItem.Path, bundleItem);
                }
                
                GenerateIdentifiersForPrefab(childName, child.gameObject, info, pathToBundleItem);
            }
        }

        private void RaiseLoadAssetLibraryCallback(ProjectEventHandler<ProjectItem> callback, ProjectAsyncOperation<ProjectItem> ao, Error error)
        {
            if (callback != null)
            {
                callback(error, null);
            }
            ao.Error = error;
            ao.IsCompleted = true;

            IsBusy = false;
        }

        private void CompleteLoadAssetLibrary(string name, ProjectEventHandler<ProjectItem> callback, ProjectAsyncOperation<ProjectItem> pao, int ordinal, AssetLibraryAsset asset)
        {
            ProjectItem result = new ProjectItem();
            Error error = new Error(Error.OK);
            if (asset == null)
            {
                error.ErrorCode = Error.E_NotFound;
                error.ErrorText = "Asset Library " + name + " does not exist";
                if (callback != null)
                {
                    callback(error, result);
                }

                pao.Error = error;
                pao.Result = null;
                pao.IsCompleted = true;
                return;
            }

            TreeModel<AssetFolderInfo> model = new TreeModel<AssetFolderInfo>(asset.AssetLibrary.Folders);
            BuildImportItemsTree(result, (AssetFolderInfo)model.root.children[0], ordinal);

            if (callback != null)
            {
                callback(error, result);
            }

            pao.Result = result;
            pao.IsCompleted = true;
            pao.Error = error;

            if (!m_assetDB.IsLibraryLoaded(ordinal))
            {
                if (!m_assetDB.IsBundledLibrary(asset.Ordinal))
                {
                    Resources.UnloadAsset(asset);
                }
            }
        }

        private void BuildImportItemsTree(ProjectItem projectItem, AssetFolderInfo folder, int ordinal)
        {
            projectItem.Name = folder.name;

            if (folder.hasChildren)
            {
                projectItem.Children = new List<ProjectItem>();
                for (int i = 0; i < folder.children.Count; ++i)
                {
                    ProjectItem child = new ProjectItem();
                    projectItem.AddChild(child);
                    BuildImportItemsTree(child, (AssetFolderInfo)folder.children[i], ordinal);
                }
            }

            if (folder.Assets != null && folder.Assets.Count > 0)
            {
                if (projectItem.Children == null)
                {
                    projectItem.Children = new List<ProjectItem>();
                }

                List<string> existingNames = new List<string>();
                for (int i = 0; i < folder.Assets.Count; ++i)
                {
                    AssetInfo assetInfo = folder.Assets[i];
                    if (assetInfo.Object != null)
                    {
                        ImportStatus status = ImportStatus.New;
                        string ext = GetExt(assetInfo.Object);
                        string name = PathHelper.GetUniqueName(assetInfo.name, ext, existingNames);
                        long itemID = m_assetDB.ToStaticResourceID(ordinal, assetInfo.PersistentID);
                        Guid typeGuid = m_typeMap.ToGuid(assetInfo.Object.GetType());
                        if (typeGuid == Guid.Empty)
                        {
                            continue;
                        }

                        ImportItem importItem = new ImportItem
                        {
                            Name = name,
                            Ext = ext,
                            Object = assetInfo.Object,
                            TypeGuid = typeGuid,
                            ItemID = itemID
                        };

                        if (assetInfo.PrefabParts != null)
                        {
                            List<PrefabPart> parts = new List<PrefabPart>();
                            for (int j = 0; j < assetInfo.PrefabParts.Count; ++j)
                            {
                                PrefabPartInfo partInfo = assetInfo.PrefabParts[j];

                                if (partInfo.Object != null)
                                {
                                    Guid partTypeGuid = m_typeMap.ToGuid(partInfo.Object.GetType());
                                    if (partTypeGuid == Guid.Empty)
                                    {
                                        continue;
                                    }
                                    PrefabPart part = new PrefabPart
                                    {
                                        Name = partInfo.Object.name,
                                        PartID = m_assetDB.ToStaticResourceID(ordinal, partInfo.PersistentID),
                                        ParentID = m_assetDB.ToStaticResourceID(ordinal, partInfo.ParentPersistentID),
                                        TypeGuid = partTypeGuid,
                                    };

                                    ProjectItem partAssetItem;
                                    if (State.TryGetProjectItemFromCache(part.PartID, out partAssetItem))
                                    {
                                        if (partAssetItem.ItemID != itemID || partAssetItem.GetTypeGuid() != typeGuid)
                                        {
                                            status = ImportStatus.Conflict;
                                        }
                                    }

                                    parts.Add(part);
                                }
                            }
                            importItem.Parts = parts.ToArray();
                        }

                        if (status != ImportStatus.Conflict)
                        {
                            ProjectItem exisitingItem;
                            if (State.TryGetProjectItemFromCache(itemID, out exisitingItem))
                            {
                                if (exisitingItem.GetTypeGuid() == typeGuid)
                                {
                                    status = ImportStatus.Overwrite;
                                }
                                else
                                {
                                    status = ImportStatus.Conflict;
                                }
                                importItem.Name = exisitingItem.Name;
                            }
                            else
                            {
                                status = ImportStatus.New;
                            }
                        }

                        const bool doNotOverwriteItems = true;
                        if(!doNotOverwriteItems || status != ImportStatus.Overwrite)
                        {
                            importItem.Status = status;
                            projectItem.AddChild(importItem);
                            existingNames.Add(importItem.NameExt);
                        }
                    }
                }
            }

            RemoveEmptyFolders(projectItem);
        }

        private void RemoveEmptyFolders(ProjectItem item)
        {
            if(item.Children != null)
            {
                for (int i = item.Children.Count - 1; i >= 0; --i)
                {
                    RemoveEmptyFolders(item.Children[i]);
                    if (item.Children[i].IsFolder && (item.Children[i].Children == null || item.Children[i].Children.Count == 0))
                    {
                        item.RemoveChild(item.Children[i]);
                    }
                }
            }
        }

        public override void UnloadImportItems(ProjectItem importItemsRoot)
        {
            if (importItemsRoot == null)
            {
                Debug.LogWarning("importItemsRoot == null");
                return;
            }

            ImportItem[] importItems = importItemsRoot.Flatten(true).OfType<ImportItem>().ToArray();
            for (int i = 0; i < importItems.Length; ++i)
            {
                if (importItems[i].Object != null)
                {
                    int ordinal = m_assetDB.ToOrdinal(importItems[i].ItemID);
                    if (!m_assetDB.IsLibraryLoaded(ordinal))
                    {
                        if (m_assetDB.IsBundledLibrary(ordinal))
                        {
                            DestroyImmediate(importItems[i].Object, true);
                            importItems[i].Object = null;
                        }
                        else if (m_assetDB.IsBuiltinLibrary(ordinal) || m_assetDB.IsSceneLibrary(ordinal) || m_assetDB.IsStaticLibrary(ordinal))
                        {
                            UnityObject uo = importItems[i].Object;
                            if (!(uo is GameObject) && !(uo is Component))
                            {
                                Resources.UnloadAsset(uo);
                            }
                            importItems[i].Object = null;
                        }
                    }
                    else
                    {
                        importItems[i].Object = null;
                    }
                }
            }
        }

        protected override void _Import(ImportItem[] importItems, ProjectEventHandler<AssetItem[]> callback, ProjectAsyncOperation<AssetItem[]> ao)
        {
            HashSet<int> assetLibraryIds = new HashSet<int>();

            if (Root == null)
            {
                Error error = new Error(Error.E_InvalidOperation);
                error.ErrorText = "Unable to load asset library. Open project first";
                RaiseImportAssetsCompletedCallback(error, null, callback, ao);
                return;
            }

            for (int i = 0; i < importItems.Length; ++i)
            {
                ImportItem importItem = importItems[i];

                if (m_typeMap.ToType(importItem.TypeGuid) == null)
                {
                    Error error = new Error(Error.E_InvalidOperation);
                    error.ErrorText = "Type of import item is invalid";
                    RaiseImportAssetsCompletedCallback(error, null, callback, ao);
                    return;
                }

                if (!assetLibraryIds.Contains(m_assetDB.ToOrdinal(importItem.ItemID)))
                {
                    assetLibraryIds.Add(m_assetDB.ToOrdinal(importItem.ItemID));
                }

                if (importItem.Parts != null)
                {
                    for (int p = 0; p < importItem.Parts.Length; ++p)
                    {
                        PrefabPart part = importItem.Parts[p];
                        if (m_typeMap.ToType(part.TypeGuid) == null)
                        {
                            Error error = new Error(Error.E_InvalidOperation);
                            error.ErrorText = "Type of import item part is invalid";
                            RaiseImportAssetsCompletedCallback(error, null, callback, ao);
                            return;
                        }

                        if (!assetLibraryIds.Contains(m_assetDB.ToOrdinal(part.PartID)))
                        {
                            assetLibraryIds.Add(m_assetDB.ToOrdinal(part.PartID));
                        }
                    }
                }
            }

            if (assetLibraryIds.Count == 0)
            {
                RaiseImportAssetsCompletedCallback(new Error(Error.OK), null, callback, ao);
                return;
            }

            if (assetLibraryIds.Count > 1)
            {
                Error error = new Error(Error.E_InvalidOperation);
                error.ErrorText = "Unable to import more then one AssetLibrary";
                RaiseImportAssetsCompletedCallback(error, null, callback, ao);
                return;
            }

            int ordinal = assetLibraryIds.First();

            if (m_assetDB.IsLibraryLoaded(ordinal))
            {
                CompleteImportAssets(importItems, ordinal, ao, false, callback);
            }
            else
            {
                LoadLibrary(ordinal, true, true, loaded =>
                {
                    if (!loaded)
                    {
                        Error error = new Error(Error.E_NotFound);
                        error.ErrorText = "Unable to load library with ordinal " + ordinal;
                        RaiseImportAssetsCompletedCallback(error, null, callback, ao);
                        return;
                    }

                    CompleteImportAssets(importItems, ordinal, ao, true, callback);
                });
            }
        }

        private void CompleteImportAssets(ImportItem[] importItems, int ordinal, ProjectAsyncOperation<AssetItem[]> ao, bool unloadWhenDone, ProjectEventHandler<AssetItem[]> callback)
        {
            AssetItem[] assetItems = new AssetItem[importItems.Length];
            object[] objects = new object[importItems.Length];

            HashSet<string> removePathHs = new HashSet<string>();
            for (int i = 0; i < importItems.Length; ++i)
            {
                ImportItem importItem = importItems[i];
                ProjectItem parent = null;
                AssetItem assetItem = null;
                if (State.TryGetProjectItemFromCache(importItem.ItemID, out ProjectItem item))
                {
                    assetItem = (AssetItem)item;
                    parent = assetItem.Parent;

                    string path = assetItem.ToString();
                    if (!removePathHs.Contains(path))
                    {
                        removePathHs.Add(path);
                    }
                }

                if (importItem.Parts != null)
                {
                    for (int p = 0; p < importItem.Parts.Length; ++p)
                    {
                        PrefabPart part = importItem.Parts[p];
                        ProjectItem partAssetItem;
                        if (State.TryGetProjectItemFromCache(part.PartID, out partAssetItem))
                        {
                            string path = partAssetItem.ToString();
                            if (!removePathHs.Contains(path))
                            {
                                removePathHs.Add(path);
                            }
                            if (assetItem != partAssetItem)
                            {
                                RemoveAssetItem((AssetItem)partAssetItem);
                            }
                        }
                    }
                }

                if (assetItem == null)
                {
                    assetItem = new AssetItem<long>();
                    assetItem.ItemID = importItem.ItemID;
                    State.AddProjectItemToCache(assetItem.ItemID, assetItem);
                }
                else
                {
                    assetItem.ItemID = importItem.ItemID;
                }

                assetItem.Name = PathHelper.GetUniqueName(importItem.Name, importItem.Ext, importItem.Parent.Children.Where(child => child != importItem).Select(child => child.NameExt).ToList());
                assetItem.Ext = importItem.Ext;
                assetItem.Parts = importItem.Parts;
                assetItem.TypeGuid = importItem.TypeGuid;
                assetItem.Preview = importItem.Preview;

                if (assetItem.Parts != null)
                {
                    for (int p = 0; p < assetItem.Parts.Length; ++p)
                    {
                        PrefabPart part = assetItem.Parts[p];
                        if (!State.ContainsProjectItemWithKey(part.PartID))
                        {
                            State.AddProjectItemToCache(part.PartID, assetItem);
                        }
                    }
                }

                if (parent == null)
                {
                    parent = Root.GetOrCreateFolder(importItem.Parent.ToString());
                }

                parent.AddChild(assetItem);
                assetItems[i] = assetItem;

                UnityObject obj = m_assetDB.FromID<UnityObject>(importItem.ItemID);
                objects[i] = obj;
                if (obj != null)
                {
                    if (m_assetDB.TryToReplaceID(obj, importItem.ItemID))
                    {
                        Debug.Log("Object  " + obj + " is present in asset db. This means that it was already loaded from different asset library (SceneAssetLibrary for example). -> PersistentID replaced with " + importItem.ItemID);
                    }
                }
            }

            m_storage.Delete(ProjectPath, removePathHs.ToArray(), deleteError =>
            {
                if (deleteError.HasError)
                {
                    RaiseImportAssetsCompletedCallback(deleteError, null, callback, ao);
                }
                else
                {
                    ProjectAsyncOperation<AssetItem[]> saveAo = new ProjectAsyncOperation<AssetItem[]>();
                    _Save(null, null, objects, null, assetItems, (saveError, savedAssetItems) =>
                    {
                        if (unloadWhenDone)
                        {
                            m_assetDB.UnloadLibrary(ordinal);

                            AssetBundle assetBundle;
                            if (m_ordinalToAssetBundle.TryGetValue(ordinal, out assetBundle))
                            {
                                assetBundle.Unload(true);
                                m_ordinalToAssetBundle.Remove(ordinal);
                            }
                        }
                        RaiseImportAssetsCompletedCallback(saveError, savedAssetItems, callback, ao);
                    },
                    saveAo, () => { });
                }
            });
        }

        private void RaiseImportAssetsCompletedCallback(Error error, AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback, ProjectAsyncOperation<AssetItem[]> ao)
        {
            if (callback != null)
            {
                callback(error, assetItems);
            }

            ao.Result = assetItems;
            ao.Error = error;
            ao.IsCompleted = true;

            RaiseImportCompleted(error, assetItems);
            
            IsBusy = false;
        }

        public override Dictionary<int, string> GetStaticAssetLibraries()
        {
            if (m_ordinalToStaticAssetLibrary != null)
            {
                return m_ordinalToStaticAssetLibrary;
            }

            AssetLibrariesListAsset staticAssetLibraries;
            if (m_staticAssetLibraries != null)
            {
                staticAssetLibraries = m_staticAssetLibraries;
            }
            else
            {
                staticAssetLibraries = Resources.Load<AssetLibrariesListAsset>("Lists/AssetLibrariesList");
            }

            if (staticAssetLibraries == null)
            {
                return new Dictionary<int, string>();
            }

            m_ordinalToStaticAssetLibrary = new Dictionary<int, string>();
            for (int i = 0; i < staticAssetLibraries.List.Count; ++i)
            {
                AssetLibraryListEntry entry = staticAssetLibraries.List[i];
                if (!m_ordinalToStaticAssetLibrary.ContainsKey(entry.Ordinal))
                {
                    m_ordinalToStaticAssetLibrary.Add(entry.Ordinal, entry.Library.Remove(entry.Library.LastIndexOf(".asset")));
                }
            }

            return m_ordinalToStaticAssetLibrary;
        }
    }
}
