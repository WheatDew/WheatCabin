using Battlehub.RTCommon;
using Battlehub.RTCommon.EditorTreeView;
using Battlehub.RTSL.Battlehub.SL2;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL
{
    public interface IProjectAsyncState<TID> : IDisposable
    {
        string ProjectPath { get; set; }
        ProjectInfo ProjectInfo { get; set; }
        ProjectItem RootFolder { get; set; }
        ProjectItem LoadedScene { get; set; }

        Transform RuntimePrefabsRoot { get; }
        CancellationToken CancellationToken { get; }
        Task<Lock.LockReleaser> LockAsync(Action lockedCallback = null, Action releasedCallback = null, CancellationToken ct = default);
        void ThrowIfCancellationRequested();
        void Cancel();

        ProjectItem[] FlattenHierarchy(bool excludeFolders, bool excludeAssets = false);
        IEnumerable<ProjectItem> ProjectItemsCache { get; }
        ProjectItem this[TID key] { get; set; }
        void AddProjectItemToCache(TID key, ProjectItem value);
        bool ContainsProjectItemWithKey(TID key);
        bool RemoveProjectItemFromCache(TID key);
        bool TryGetProjectItemFromCache(TID key, out ProjectItem value);
        void ClearProjectItemsCache();
    }

    public class ProjectAsyncState<TID> : IProjectAsyncState<TID>
    {
        public string ProjectPath { get; set; }
        public ProjectInfo ProjectInfo { get; set; }
        public ProjectItem RootFolder { get; set; }
        public ProjectItem LoadedScene { get; set; }

        public Transform RuntimePrefabsRoot { get; private set; }

        public CancellationToken CancellationToken
        {
            get { return CTS.Token; }
        }

        private CancellationTokenSource CTS { get; set; }
        private Lock Lock { get; set; }
        private Dictionary<TID, ProjectItem> IDToProjectItem { get; set; }

        public ProjectAsyncState(GameObject root)
        {
            GameObject go = new GameObject();
            go.name = "DynamicResourceRoot";
            go.transform.SetParent(root.transform, false);

            RuntimePrefabsRoot = go.transform;
            RuntimePrefabsRoot.gameObject.SetActive(false);

            IDToProjectItem = new Dictionary<TID, ProjectItem>();

            Lock = new Lock();

            CTS = new CancellationTokenSource();
        }

        public void Dispose()
        {
            ProjectPath = null;
            ProjectInfo = null;
            RootFolder = null;
            LoadedScene = null;
            IDToProjectItem.Clear();

            CTS.Cancel();
            CTS = null;
            Lock = null;

            if (RuntimePrefabsRoot != null)
            {
                UnityObject.Destroy(RuntimePrefabsRoot.gameObject);
                RuntimePrefabsRoot = null;
            }
        }

        public ProjectItem[] FlattenHierarchy(bool excludeFolders, bool excludeAssets = false)
        {
            return RootFolder.Flatten(excludeFolders, excludeAssets);
        }

        public IEnumerable<ProjectItem> ProjectItemsCache
        {
            get { return IDToProjectItem.Values; }
        }

        public ProjectItem this[TID key]
        {
            get { return IDToProjectItem[key]; }
            set { IDToProjectItem[key] = value; }
        }

        public void AddProjectItemToCache(TID key, ProjectItem value)
        {
            IDToProjectItem.Add(key, value);
        }

        public bool ContainsProjectItemWithKey(TID key)
        {
            return IDToProjectItem.ContainsKey(key);
        }

        public bool RemoveProjectItemFromCache(TID key)
        {
            return IDToProjectItem.Remove(key);
        }

        public bool TryGetProjectItemFromCache(TID key, out ProjectItem value)
        {
            return IDToProjectItem.TryGetValue(key, out value);
        }

        public void ClearProjectItemsCache()
        {
            IDToProjectItem.Clear();
        }

        public Task<Lock.LockReleaser> LockAsync(Action lockedCallback = null, Action releasedCallback = null, CancellationToken ct = default)
        {
            return Lock.Wait(lockedCallback, releasedCallback, ct);
        }

        public void ThrowIfCancellationRequested()
        {
            CTS.Token.ThrowIfCancellationRequested();
        }

        public void Cancel()
        {
            if(CTS != null)
            {
                CTS.Cancel();
            }
            
            CTS = new CancellationTokenSource();
        }
    }

    internal class ProjectAsyncSafe : IProjectAsyncSafe
    {
        public IProjectEvents Events { get { return m_wrappedProject.Events; } }

        private IProjectAsync m_wrappedProject;
        public ProjectAsyncSafe(IProjectAsync wrappedObj)
        {
            m_wrappedProject = wrappedObj;
        }

        public async Task<ProjectInfo[]> GetProjectsAsync(CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.GetProjectsAsync(ct);
            }
        }
        public async Task<ProjectInfo> CreateProjectAsync(string project, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.CreateProjectAsync(project, ct);
            }
        }

        public async Task CopyProjectAsync(string project, string targetProject, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.CopyProjectAsync(project, targetProject, ct);
            }
        }

        public async Task DeleteProjectAsync(string project, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.DeleteProjectAsync(project, ct);
            }
        }

        public async Task ExportProjectAsync(string project, string targetPath, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.ExportProjectAsync(project, targetPath, ct);
            }
        }
        public async Task ImportProjectAsync(string project, string sourcePath, bool overwrite, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.ImportProjectAsync(project, sourcePath, overwrite, ct);
            }
        }

        public async Task<ProjectInfo> OpenProjectAsync(string project, OpenProjectFlags flags, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.OpenProjectAsync(project, flags, ct);
            }
        }

        public async Task CloseProjectAsync(CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.CloseProjectAsync(ct);
            }
        }

        public async Task<byte[][]> GetPreviewsAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.GetPreviewsAsync(projectItems, ct);
            }
        }
        public async Task<ProjectItem[]> GetAssetItemsAsync(ProjectItem[] folders, string searchPattern, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.GetAssetItemsAsync(folders, searchPattern, ct);
            }
        }

        public async Task SaveAsync(ProjectItem[] projectItems, object[] obj, bool isUserAction, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.SaveAsync(projectItems, obj, isUserAction, ct);
            }
        }

        public async Task SaveAsync(ProjectItem[] projectItems, object[] obj, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.SaveAsync(projectItems, obj, ct);
            }
        }

        public async Task<ProjectItem[]> SaveAsync(ProjectItem[] folders, byte[][] previewData, object[] obj, string[] nameOverrides, bool isUserAction, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.SaveAsync(folders, previewData, obj, nameOverrides, isUserAction, ct);
            }
        }

        public async Task SavePreviewsAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.SavePreviewsAsync(projectItems, ct);
            }
        }

        public async Task<UnityObject[]> LoadAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.LoadAsync(projectItems, ct);
            }
        }

        public async Task UnloadAllAsync(CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.UnloadAllAsync(ct);
            }
        }

        public async Task UnloadAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.UnloadAsync(projectItems, ct);
            }
        }

        public async Task<ProjectItem[]> CreatePrefabsAsync(ProjectItem[] parentFolders, GameObject[] prefabs, bool includeDeps, Func<UnityObject, byte[]> createPreview, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.CreatePrefabsAsync(parentFolders, prefabs, includeDeps, createPreview, ct);
            }
        }

        public async Task<ProjectItem[]> CreateFoldersAsync(ProjectItem[] parentFolders, string[] names, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.CreateFoldersAsync(parentFolders, names, ct);
            }
        }
        public async Task<ProjectItem[]> DuplicateAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.DuplicateAsync(projectItems, ct);
            }
        }

        public async Task MoveAsync(ProjectItem[] projectItems, ProjectItem targetFolder, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.MoveAsync(projectItems, targetFolder, ct);
            }
        }

        public async Task RenameAsync(ProjectItem[] projectItem, string[] newNames, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.RenameAsync(projectItem, newNames, ct);
            }
        }

        public async Task DeleteAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.DeleteAsync(projectItems, ct);
            }
        }

        public async Task<T> GetValueAsync<T>(string key, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.GetValueAsync<T>(key, ct);
            }
        }

        public async Task<T[]> GetValuesAsync<T>(string searchPattern, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.GetValuesAsync<T>(searchPattern, ct);
            }
        }

        public async Task SetValueAsync<T>(string key, T obj, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.SetValueAsync(key, obj, ct);
            }
        }

        public async Task DeleteValueAsync<T>(string key, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                await m_wrappedProject.DeleteValueAsync<T>(key, ct);
            }
        }

        public async Task<string[]> GetAssetBundlesAsync(CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.GetAssetBundlesAsync(ct);
            }
        }

        public async Task<object[]> GetDependenciesAsync(object obj, bool exceptMappedObjects, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.GetDependenciesAsync(obj, exceptMappedObjects, ct);
            }
        }

        public async Task<string[]> GetStaticAssetLibrariesAsync(CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.GetStaticAssetLibrariesAsync(ct);
            }
        }

        public async Task<ProjectItem> LoadImportItemsAsync(string path, bool isBuiltIn, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.LoadImportItemsAsync(path, isBuiltIn, ct);
            }
        }

        public void UnloadImportItems(ProjectItem importItemsRoot)
        {
            m_wrappedProject.UnloadImportItems(importItemsRoot);
        }

        public async Task<ProjectItem[]> ImportAsync(ProjectItem[] importItems, CancellationToken ct)
        {
            using (await m_wrappedProject.LockAsync())
            {
                return await m_wrappedProject.ImportAsync(importItems, ct);
            }
        }
    }

    public class ProjectAsyncImpl : ProjectAsyncWithAssetLibraries<long>
    {
        private const long m_staticResourceIDMask = 1L << 34;
        private const long m_dynamicResourceIDMask = 1L << 36;

        public long ToStaticResourceID(int assetLibraryID, int id)
        {
            id = (assetLibraryID << AssetLibraryInfo.ORDINAL_OFFSET) | (AssetLibraryInfo.ORDINAL_MASK & id);
            return m_staticResourceIDMask | (0x00000000FFFFFFFFL & id);
        }

        public long ToDynamicResourceID(int assetLibraryID, int id)
        {
            id = (assetLibraryID << AssetLibraryInfo.ORDINAL_OFFSET) | (AssetLibraryInfo.ORDINAL_MASK & id);
            return m_dynamicResourceIDMask | (0x00000000FFFFFFFFL & id);
        }


        protected override async Task<Dictionary<int, long>> LoadIDsForAssetLibraryAsync(int assetLibraryID, AssetLibraryAsset assetLibrary, CancellationToken ct)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            MappingInfo mapping = new MappingInfo();
            assetLibrary.LoadIDMappingTo(mapping, true, false);

            Dictionary<int, long> ids = new Dictionary<int, long>();
            foreach(int id in mapping.InstanceIDtoPID.Values)
            {
                ids.Add(id, ToStaticResourceID(assetLibraryID, id));
            }

            return ids;
        }


        protected override async Task SaveIDsForAssetLibraryAsync(int assetLibraryID, Dictionary<int, long> pidToTID, CancellationToken ct)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();
        }

        protected override Task<long[]> GenerateIdentifiersAsync(int count, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<long[]>();
            int assetIdentifier = ProjectInfo.AssetIdentifier;
            long[] result = new long[count];
            for (int i = 0; i < count; ++i)
            {
                int ordinal;
                int id;
                if (GetOrdinalAndId(ref ProjectInfo.AssetIdentifier, out ordinal, out id))
                {
                    result[i] = ToDynamicResourceID(ordinal, id);
                }
                else
                {
                    ProjectInfo.AssetIdentifier = assetIdentifier;
                    throw new InvalidOperationException("Identifiers Exhausted");
                }
            }

            tcs.TrySetResult(result);
            return tcs.Task;
        }

        private bool GetOrdinalAndId(ref int identifier, out int ordinal, out int id)
        {
            ordinal = AssetLibraryInfo.DYNAMICLIB_FIRST + ToOrdinal(identifier);
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

        public int ToOrdinal(int id)
        {
            return (id >> AssetLibraryInfo.ORDINAL_OFFSET) & AssetLibraryInfo.ORDINAL_MASK;
        }

        protected override Task ResolveDependenciesAsync(ProjectItem[] projectItems, HashSet<long> unresolvedDependencies, CancellationToken ct)
        {
            #region compatibility with 2.11 - 2.26 files
            for (int i = 0; i < projectItems.Length; ++i)
            {
                AssetItem assetItem = projectItems[i] as AssetItem;
                if (assetItem != null && assetItem.Dependencies != null) 
                {
                    if(assetItem.Dependencies != null && assetItem.Dependencies.Length > 0)
                    {
                        int[] assetLibraryIDs = assetItem.Dependencies.Select(d => (int)(0x00000000FFFFFFFFL & d)).Select(d => ToOrdinal(d)).Distinct().ToArray();
                        assetItem.SetAssetLibraryIDs(assetLibraryIDs);
                    }
                }
            }
            #endregion

            return base.ResolveDependenciesAsync(projectItems, unresolvedDependencies, ct);
        }
    }



    public class ProjectAsyncWithAssetLibraries<TID> : ProjectAsyncImpl<TID>, IProjectAsyncWithAssetLibraries where TID : IEquatable<TID>
    {
        private string m_builtInLibraryName = null;
        public string BuiltInLibraryName
        {
            get { return string.IsNullOrWhiteSpace(m_builtInLibraryName) ? "BuiltInAssets/BuiltInAssetLibrary" : m_builtInLibraryName; }
            set { m_builtInLibraryName = value; }
        }

        private string m_sceneDepsLibraryName = null;
        public string SceneDepsAssetLibraryName
        {
            get { return string.IsNullOrWhiteSpace(m_sceneDepsLibraryName) ? "Scenes/" + SceneManager.GetActiveScene().name + "/SceneAssetLibrary" : m_sceneDepsLibraryName; }
            set { m_sceneDepsLibraryName = value; }
        }

        private readonly Dictionary<int, AssetLibraryAsset> m_loadedAssetLibraries = new Dictionary<int, AssetLibraryAsset>();
        private Dictionary<int, AssetBundleInfo> m_assetLibraryIDToAssetBundleInfo = new Dictionary<int, AssetBundleInfo>();
        private readonly Dictionary<int, AssetBundle> m_assetLibraryIDToAssetBundle = new Dictionary<int, AssetBundle>();
      
        public override AssetBundle[] LoadedAssetBundles
        {
            get { return m_assetLibraryIDToAssetBundle.Values.ToArray(); }
        }

        private AssetLibrariesListAsset m_staticAssetLibraries = null;
        public AssetLibrariesListAsset StaticAssetLibraries
        {
            get { return m_staticAssetLibraries; }
            set { m_staticAssetLibraries = value; }
        }

        private Dictionary<int, string> m_assetLibraryIDToLibraryName;
        public Dictionary<int, string> AssetLibraryIDToLibraryName
        {
            get { return m_assetLibraryIDToLibraryName; }
        }

        private bool IsLibraryLoaded(int assetLibraryId)
        {
            return m_loadedAssetLibraries.ContainsKey(assetLibraryId);
        }

        private bool IsStaticLibrary(int assetLibraryID)
        {
            return AssetLibraryInfo.STATICLIB_FIRST <= assetLibraryID && assetLibraryID <= AssetLibraryInfo.STATICLIB_LAST;
        }

        private bool IsSceneLibrary(int assetLibraryID)
        {
            return AssetLibraryInfo.SCENELIB_FIRST <= assetLibraryID && assetLibraryID <= AssetLibraryInfo.SCENELIB_LAST;
        }

        private bool IsBuiltinLibrary(int assetLibraryID)
        {
            return AssetLibraryInfo.BUILTIN_FIRST <= assetLibraryID && assetLibraryID <= AssetLibraryInfo.BUILTIN_LAST;
        }

        private bool IsBundledLibrary(int assetLibraryID)
        {
            return AssetLibraryInfo.BUNDLEDLIB_FIRST <= assetLibraryID && assetLibraryID <= AssetLibraryInfo.BUNDLEDLIB_LAST;
        }

        private Dictionary<int, string> GetStaticAssetLibraries()
        {
            if (m_assetLibraryIDToLibraryName != null)
            {
                return m_assetLibraryIDToLibraryName;
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

            m_assetLibraryIDToLibraryName = new Dictionary<int, string>();
            for (int i = 0; i < staticAssetLibraries.List.Count; ++i)
            {
                AssetLibraryListEntry entry = staticAssetLibraries.List[i];
                if (!m_assetLibraryIDToLibraryName.ContainsKey(entry.Ordinal))
                {
                    m_assetLibraryIDToLibraryName.Add(entry.Ordinal, entry.Library.Remove(entry.Library.LastIndexOf(".asset")));
                }
            }

            return m_assetLibraryIDToLibraryName;
        }

        protected virtual async Task<Dictionary<int, TID>> LoadIDsForAssetLibraryAsync(int assetLibraryID, AssetLibraryAsset asset, CancellationToken ct)
        {
            string key = $"AssetLibrary_{assetLibraryID}";

            Dictionary<int, TID> pidToTID;
            try
            {
                pidToTID = await GetValueAsync<Dictionary<int, TID>>(key, ct);
            }
            catch (StorageException e)
            {
                if (e.ErrorCode == Error.E_NotFound)
                {
                    pidToTID = new Dictionary<int, TID>();
                }
                else
                {
                    throw;
                }
            }
            return pidToTID;
        }

        protected virtual async Task SaveIDsForAssetLibraryAsync(int assetLibraryID, Dictionary<int, TID> pidToTID, CancellationToken ct)
        {
            string key = $"AssetLibrary_{assetLibraryID}";
            await SetValueAsync(key, pidToTID, ct);
        }

        protected virtual async Task RegisterAssetLibraryAsync(int assetLibraryID, AssetLibraryAsset assetLib, bool addToAssetDB, CancellationToken ct)
        {
            if(addToAssetDB)
            {
                m_loadedAssetLibraries[assetLibraryID] = assetLib;
            }

            MappingInfo mappingInfo = new MappingInfo();
            assetLib.Ordinal = assetLibraryID;
            assetLib.LoadIDMappingTo(mappingInfo, true, true);

            Dictionary<int, TID> pidToTID = await LoadIDsForAssetLibraryAsync(assetLibraryID, assetLib, ct);
            foreach (int pid in pidToTID.Keys.ToArray())
            {
                if (!mappingInfo.PersistentIDtoObj.ContainsKey(pid))
                {
                    pidToTID.Remove(pid);
                }
            }


            List<int> newIds = new List<int>();
            foreach (int id in mappingInfo.PersistentIDtoObj.Keys)
            {
                if (!pidToTID.ContainsKey(id))
                {
                    newIds.Add(id);
                }
            }

            if (newIds.Count > 0)
            {
                TID[] ids = await GenerateIdentifiersAsync(newIds.Count, ct);
                ct.ThrowIfCancellationRequested();

                for (int i = 0; i < ids.Length; ++i)
                {
                    pidToTID.Add(newIds[i], ids[i]);
                }
            }

            await SaveIDsForAssetLibraryAsync(assetLibraryID, pidToTID, ct);

            if(addToAssetDB)
            {
                IAssetDB<TID> assetDB = AssetDB;
                foreach (KeyValuePair<int, TID> kvp in pidToTID)
                {
                    assetDB.RegisterStaticResource(assetLibraryID, kvp.Value, mappingInfo.PersistentIDtoObj[kvp.Key]);
                }
            }
            
        }

        private Task<AssetLibraryAsset> LoadAssetLibraryAsync(string assetLibrary, int assetLibraryID, CancellationToken ct)
        {
            if (m_loadedAssetLibraries.ContainsKey(assetLibraryID))
            {
                throw new ArgumentException($"AssetLibrary with the same id {assetLibraryID} already loaded");
            }

            TaskCompletionSource<AssetLibraryAsset> tcs = new TaskCompletionSource<AssetLibraryAsset>();
            ResourceRequest request = Resources.LoadAsync<AssetLibraryAsset>(assetLibrary);
            Action<AsyncOperation> completed = null;
            completed = ao =>
            {
                request.completed -= completed;

                using (ct.Register(() => tcs.TrySetCanceled(ct)))
                {
                    if (!ct.IsCancellationRequested)
                    {
                        AssetLibraryAsset assetLib = (AssetLibraryAsset)request.asset;
                        if (assetLib == null)
                        {
                            if (IsBuiltinLibrary(assetLibraryID))
                            {
                                if (assetLibraryID - AssetLibraryInfo.BUILTIN_FIRST == 0)
                                {
                                    Debug.LogWarningFormat("Asset Library was not found : {0}. Click Tools->Runtime SaveLoad->Update Libraries.", assetLibrary);
                                }
                            }
                            else if (IsSceneLibrary(assetLibraryID))
                            {
                                if (assetLibraryID - AssetLibraryInfo.SCENELIB_FIRST == 0)
                                {
                                    Debug.LogWarningFormat("Asset Library was not found : {0}. Click Tools->Runtime SaveLoad->Update Libraries.", assetLibrary);
                                }
                            }
                            else
                            {
                                Debug.LogWarningFormat("Asset Library was not found : {0}", assetLibrary);
                            }

                            tcs.TrySetResult(null);
                            return;
                        }

                        tcs.TrySetResult(assetLib);
                    }
                }
            };
            request.completed += completed;
            return tcs.Task;
        }
 
        private async Task LoadBuiltinLibraryAsync(string name, int assetLibraryID, CancellationToken ct)
        {
            string libraryName = assetLibraryID == AssetLibraryInfo.BUILTIN_FIRST ? name : name + (assetLibraryID - AssetLibraryInfo.BUILTIN_FIRST + 1);
            AssetLibraryAsset assetLib = await LoadAssetLibraryAsync(libraryName, assetLibraryID, ct);
            if (assetLib == null)
            {
                if (assetLibraryID == AssetLibraryInfo.BUILTIN_FIRST)
                {
                    Debug.LogWarning("Builtin library was not loaded");
                }
            }
            else
            {
                await RegisterAssetLibraryAsync(assetLibraryID, assetLib, true, ct);

                assetLibraryID++;
                await LoadBuiltinLibraryAsync(name, assetLibraryID, ct);
            }
        }

        private async Task LoadBuiltinLibraryAsync(CancellationToken ct)
        {
            if (!m_loadedAssetLibraries.ContainsKey(AssetLibraryInfo.BUILTIN_FIRST))
            {
                await LoadBuiltinLibraryAsync(BuiltInLibraryName, AssetLibraryInfo.BUILTIN_FIRST, ct);
            }
        }

        private async Task LoadLibraryWithSceneDependenciesAsync(string name, int assetLibraryID, CancellationToken ct)
        {
            string libraryName = assetLibraryID == AssetLibraryInfo.SCENELIB_FIRST ? name : name + (assetLibraryID - AssetLibraryInfo.SCENELIB_FIRST + 1);
            AssetLibraryAsset assetLib = await LoadAssetLibraryAsync(libraryName, assetLibraryID, ct);
            if (assetLib == null)
            {
                if (assetLibraryID == AssetLibraryInfo.SCENELIB_FIRST)
                {
                    Debug.LogWarning("Library with scene dependencies was not loaded");
                }
            }
            else
            {
                await RegisterAssetLibraryAsync(assetLibraryID, assetLib, true, ct);

                assetLibraryID++;
                await LoadLibraryWithSceneDependenciesAsync(name, assetLibraryID, ct);
            }
        }

        protected override async Task LoadLibraryWithSceneDependenciesAsync(CancellationToken ct)
        {
            await LoadBuiltinLibraryAsync(ct);
            if (!m_loadedAssetLibraries.ContainsKey(AssetLibraryInfo.SCENELIB_FIRST))
            {
                await LoadLibraryWithSceneDependenciesAsync(SceneDepsAssetLibraryName, AssetLibraryInfo.SCENELIB_FIRST, ct);
            }
        }

        protected override async Task ResolveDependenciesAsync(ProjectItem[] projectItems, HashSet<TID> unresolvedDependencies, CancellationToken ct)
        {
            HashSet<int> assetLibrariesToLoad = new HashSet<int>();
            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
                int[] assetLibraryIDs = projectItem.GetAssetLibraryIDs();
                if (assetLibraryIDs != null)
                {
                    for(int j = 0; j < assetLibraryIDs.Length; ++j)
                    {
                        int assetLibraryID = assetLibraryIDs[j];
                        if (!assetLibrariesToLoad.Contains(assetLibraryID) && !IsLibraryLoaded(assetLibraryID))
                        {
                            assetLibrariesToLoad.Add(assetLibraryID);
                        }
                    }
                    
                }
            }

            await LoadAssetLibrariesAsync(assetLibrariesToLoad, ct);

            foreach (TID unresolvedDependency in unresolvedDependencies)
            {
                UnityObject obj = AssetDB.FromID<UnityObject>(unresolvedDependency);
                if (obj != null)
                {
                    Guid typeGuid = TypeMap.ToGuid(obj.GetType());
                    if (typeGuid != Guid.Empty)
                    {
                        InternalState.AddProjectItemToCache(unresolvedDependency, m_storage.CreateAssetItem(unresolvedDependency, typeGuid, obj.name, GetExt(obj), null));
                    }
                }
            }
        }

        private async Task LoadAssetLibrariesAsync(HashSet<int> assetLibrariesToLoad, CancellationToken ct)
        {
            if (assetLibrariesToLoad.Count == 0)
            {
                return;
            }
            else
            {
                Dictionary<int, string> assetLibraryIDToName = GetStaticAssetLibraries();

                int loadedLibrariesCount = 0;
                foreach (int assetLibraryID in assetLibrariesToLoad)
                {
                    string assetLibraryName = null;
                    if (assetLibraryIDToName.ContainsKey(assetLibraryID))
                    {
                        assetLibraryName = assetLibraryIDToName[assetLibraryID];
                    }
                    else
                    {
                        if (IsBuiltinLibrary(assetLibraryID))
                        {
                            if (assetLibraryID != AssetLibraryInfo.BUILTIN_FIRST)
                            {
                                assetLibraryName = BuiltInLibraryName + ((assetLibraryID - AssetLibraryInfo.BUILTIN_FIRST) + 1);
                            }
                            else
                            {
                                assetLibraryName = BuiltInLibraryName;
                            }
                        }
                        else if (IsSceneLibrary(assetLibraryID))
                        {
                            if (assetLibraryID != AssetLibraryInfo.SCENELIB_FIRST)
                            {
                                assetLibraryName = SceneDepsAssetLibraryName + ((assetLibraryID - AssetLibraryInfo.SCENELIB_FIRST) + 1);
                            }
                            else
                            {
                                assetLibraryName = SceneDepsAssetLibraryName;
                            }
                        }
                        else if (IsBundledLibrary(assetLibraryID))
                        {
                            AssetBundleInfo assetBundleInfo = m_assetLibraryIDToAssetBundleInfo[assetLibraryID];
                            assetLibraryName = assetBundleInfo.UniqueName;
                        }
                    }

                    if (!string.IsNullOrEmpty(assetLibraryName))
                    {
                        bool done = await LoadLibraryAsync(assetLibraryID, ct);
                        if (!done)
                        {
                            Debug.LogWarning("Asset Library '" + assetLibraryName + "' was not loaded");
                        }
                        loadedLibrariesCount++;
                        if (assetLibrariesToLoad.Count == loadedLibrariesCount)
                        {
                            return;
                        }
                    }
                    else
                    {
                        loadedLibrariesCount++;
                        if (assetLibrariesToLoad.Count == loadedLibrariesCount)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private async Task<bool> LoadLibraryAsync(int assetLibraryID, CancellationToken ct)
        {
            if (IsLibraryLoaded(assetLibraryID))
            {
                Debug.LogError("Already loaded");
                return false;
            }

            if (IsStaticLibrary(assetLibraryID))
            {
                Dictionary<int, string> staticAssetLibraries = GetStaticAssetLibraries();
                if (!staticAssetLibraries.ContainsKey(assetLibraryID))
                {
                    Debug.LogError("Unable to load static library " + assetLibraryID);
                    return false;
                }
                AssetLibraryAsset assetLib = await LoadAssetLibraryAsync(staticAssetLibraries[assetLibraryID], assetLibraryID, ct);
                if (assetLib != null)
                {
                    await RegisterAssetLibraryAsync(assetLibraryID, assetLib, true, ct);
                }
            }
            else if (IsBuiltinLibrary(assetLibraryID))
            {
                int num = assetLibraryID - AssetLibraryInfo.BUILTIN_FIRST;
                string builtinLibraryName = BuiltInLibraryName;
                if (num > 0)
                {
                    builtinLibraryName += (num + 1);
                }
                AssetLibraryAsset assetLib = await LoadAssetLibraryAsync(builtinLibraryName, assetLibraryID, ct);
                if (assetLib != null)
                {
                    await RegisterAssetLibraryAsync(assetLibraryID, assetLib, true, ct);
                }
            }
            else if (IsSceneLibrary(assetLibraryID))
            {
                int num = assetLibraryID - AssetLibraryInfo.SCENELIB_FIRST;
                string sceneLibraryName = SceneDepsAssetLibraryName;
                if (num > 0)
                {
                    sceneLibraryName += (num + 1);
                }
                AssetLibraryAsset assetLib = await LoadAssetLibraryAsync(sceneLibraryName, assetLibraryID, ct);
                if(assetLib != null)
                {
                    await RegisterAssetLibraryAsync(assetLibraryID, assetLib, true, ct);
                }
            }
            else if (IsBundledLibrary(assetLibraryID))
            {
                AssetBundleInfo assetBundleInfo;
                if (!m_assetLibraryIDToAssetBundleInfo.TryGetValue(assetLibraryID, out assetBundleInfo))
                {
                    throw new InvalidOperationException("asset bundle with asset library ID = " + assetLibraryID + " was not imported");
                }

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                IAssetBundleLoader loader = IOC.Resolve<IAssetBundleLoader>();
                loader.Load(assetBundleInfo.UniqueName, async assetBundle =>
                {
                    if(ct.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled(ct);
                        return;
                    }

                    if (assetBundle == null)
                    {
                        Debug.LogError("Unable to load asset bundle " + assetBundleInfo.UniqueName);
                        tcs.TrySetResult(false);
                        return;
                    }

                    using(ct.Register(() => tcs.TrySetCanceled(ct)))
                    {
                        try
                        {
                            m_assetLibraryIDToAssetBundle.Add(assetLibraryID, assetBundle);

                            AssetLibraryAsset assetLib = ToAssetLibraryAsset(assetBundle, assetBundleInfo);

                            await RegisterAssetLibraryAsync(assetLibraryID, assetLib, true, ct);
                            tcs.TrySetResult(true);
                        }
                        catch(OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            tcs.TrySetException(e);
                        }
                    }
                });

                return await tcs.Task;
            }
            else
            {
                throw new ArgumentException("could load static or bundled library", "assetLibraryID");
            }

            return true;
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

        private async Task UnloadAssetLibraryAsync(int assetLibraryID, CancellationToken ct)
        {
            AssetLibraryAsset assetLib;
            if (m_loadedAssetLibraries.TryGetValue(assetLibraryID, out assetLib))
            {
                Dictionary<int, TID> ids = await LoadIDsForAssetLibraryAsync(assetLibraryID, assetLib, ct);
                foreach(TID id in ids.Values)
                {
                    AssetDB.UnregisterStaticResource(id);
                }    

                m_loadedAssetLibraries.Remove(assetLibraryID);
                
                if (IsBundledLibrary(assetLibraryID))
                {
                    AssetBundle assetBundle = m_assetLibraryIDToAssetBundle[assetLibraryID];
                    m_assetLibraryIDToAssetBundle.Remove(assetLibraryID);
                    assetBundle.Unload(true);
                }
                else
                {
                    Resources.UnloadAsset(assetLib);
                }
            }
        }

        protected override void OnAssetBundleInfoLoaded(AssetBundleInfo[] assetBundleInfo)
        {
            m_assetLibraryIDToAssetBundleInfo = assetBundleInfo.ToDictionary(info => info.Ordinal);
            base.OnAssetBundleInfoLoaded(assetBundleInfo);
        }

        public override async Task<string[]> GetStaticAssetLibrariesAsync(CancellationToken ct)
        {
            await Task.Yield();
            CreateLinkedToken(ct).ThrowIfCancellationRequested();
            return GetStaticAssetLibraries().Values.ToArray();
        }


        #region LoadImportItemsAsync

        public override async Task<ProjectItem> LoadImportItemsAsync(string libraryName, bool isBuiltIn, CancellationToken ct)
        {
            if (!IsOpened)
            {
                throw new InvalidOperationException("Unable to load asset library. Open project first");
            }

            ct = CreateLinkedToken(ct);

            await LoadLibraryWithSceneDependenciesAsync(ct);

            if (isBuiltIn)
            {
                int assetLibraryID = -1;

                Dictionary<int, string> assetLibraryIDToName = GetStaticAssetLibraries();
                foreach (KeyValuePair<int, string> kvp in AssetLibraryIDToLibraryName)
                {
                    if (kvp.Value == libraryName)
                    {
                        assetLibraryID = kvp.Key;
                        break;
                    }
                }

                if (assetLibraryID < 0)
                {
                    throw new ArgumentException($"Asset library {libraryName} was not found");
                }

                TaskCompletionSource<AssetLibraryAsset> tcs = new TaskCompletionSource<AssetLibraryAsset>();
                ResourceRequest request = Resources.LoadAsync<AssetLibraryAsset>(libraryName);
                Action<AsyncOperation> completed = null;
                completed = op =>
                {
                    using (ct.Register(() => tcs.TrySetCanceled(ct)))
                    {
                        request.completed -= completed;
                        if(!ct.IsCancellationRequested)
                        {
                            tcs.TrySetResult((AssetLibraryAsset)request.asset);
                        }
                    }
                };
                request.completed += completed;
                AssetLibraryAsset asset = await tcs.Task;

                if(!IsLibraryLoaded(assetLibraryID))
                {
                    await RegisterAssetLibraryAsync(assetLibraryID, asset, false, ct);
                }

                return await LoadImportItemsFromAssetLibraryAsync(assetLibraryID, asset, ct);
            }
            else
            {
                if (ProjectInfo.BundleIdentifier >= AssetLibraryInfo.MAX_BUNDLEDLIBS - 1)
                {
                    throw new InvalidOperationException("Unable to load asset bundle. Bundle identifiers exhausted");
                }
                AssetBundleInfo assetBundleInfo = null;
                try
                {
                    assetBundleInfo = await m_storage.LoadAsync(InternalState.ProjectPath, libraryName, ct);
                    ct.ThrowIfCancellationRequested();

                }
                catch(StorageException e)
                {
                    if(e.ErrorCode != Error.E_NotFound)
                    {
                        throw;
                    }
                }
                 
                if (assetBundleInfo == null)
                {
                    assetBundleInfo = new AssetBundleInfo();
                    assetBundleInfo.UniqueName = libraryName;
                    assetBundleInfo.Ordinal = AssetLibraryInfo.BUNDLEDLIB_FIRST + ProjectInfo.BundleIdentifier;
                    ProjectInfo.BundleIdentifier++;
                    m_assetLibraryIDToAssetBundleInfo.Add(assetBundleInfo.Ordinal, assetBundleInfo);
                }

                AssetBundle loadedAssetBundle;
                if (m_assetLibraryIDToAssetBundle.TryGetValue(assetBundleInfo.Ordinal, out loadedAssetBundle))
                {
                    Debug.Assert(IsLibraryLoaded(assetBundleInfo.Ordinal));
                }
                else
                {
                    TaskCompletionSource<AssetBundle> tcs = new TaskCompletionSource<AssetBundle>();
                    IAssetBundleLoader loader = IOC.Resolve<IAssetBundleLoader>();
                    loader.Load(libraryName, assetBundle =>
                    {
                        using (ct.Register(() => tcs.TrySetCanceled(ct)))
                        {
                            if(!ct.IsCancellationRequested)
                            {
                                tcs.TrySetResult(assetBundle);
                            }
                        }
                    });

                    loadedAssetBundle = await tcs.Task;
                }

                return await LoadImportItemsFromAssetBundleAsync(assetBundleInfo, loadedAssetBundle, ct);
            }
        }

        private async Task<ProjectItem> LoadImportItemsFromAssetBundleAsync(AssetBundleInfo assetBundleInfo, AssetBundle assetBundle, CancellationToken ct)
        {
            if (assetBundle == null)
            {
                throw new ArgumentNullException("assetBundle");
            }

            GenerateIdentifiers(assetBundle, assetBundleInfo);

            if (m_assetLibraryIDToAssetBundleInfo.ContainsKey(assetBundleInfo.Ordinal))
            {
                m_assetLibraryIDToAssetBundleInfo[assetBundleInfo.Ordinal] = assetBundleInfo;
            }

            if (assetBundleInfo.Identifier >= AssetLibraryInfo.MAX_ASSETS)
            {
                throw new InvalidOperationException("Unable to load asset bundle. Asset identifier exhausted");
            }

            await m_storage.SaveAsync(InternalState.ProjectPath, assetBundleInfo, ProjectInfo, ct);
            ct.ThrowIfCancellationRequested();

            AssetLibraryAsset asset = ToAssetLibraryAsset(assetBundle, assetBundleInfo);

            await RegisterAssetLibraryAsync(assetBundleInfo.Ordinal, asset, false, ct);

            if (!IsLibraryLoaded(assetBundleInfo.Ordinal))
            {
                assetBundle.Unload(false);
            }

            return await LoadImportItemsFromAssetLibraryAsync(assetBundleInfo.Ordinal, asset, ct);
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

        private async Task<ProjectItem> LoadImportItemsFromAssetLibraryAsync(int assetLibraryID, AssetLibraryAsset asset, CancellationToken ct)
        {
            ProjectItem result = new ProjectItem();
            if (asset == null)
            {
                throw new ArgumentNullException("asset");
            }

            Dictionary<int, TID> pidToTID = await LoadIDsForAssetLibraryAsync(assetLibraryID, asset, ct);
            TreeModel<AssetFolderInfo> model = new TreeModel<AssetFolderInfo>(asset.AssetLibrary.Folders);
            BuildImportItemsTree(result, (AssetFolderInfo)model.root.children[0], assetLibraryID, pidToTID);

            if (!IsLibraryLoaded(assetLibraryID))
            {
                if (!IsBundledLibrary(asset.Ordinal))
                {
                    Resources.UnloadAsset(asset);
                }
            }

            return result;
        }

        private void BuildImportItemsTree(ProjectItem projectItem, AssetFolderInfo folder, int assetLibraryID, Dictionary<int, TID> pidToTID)
        {
            projectItem.Name = folder.name;

            if (folder.hasChildren)
            {
                projectItem.Children = new List<ProjectItem>();
                for (int i = 0; i < folder.children.Count; ++i)
                {
                    ProjectItem child = new ProjectItem();
                    projectItem.AddChild(child);
                    BuildImportItemsTree(child, (AssetFolderInfo)folder.children[i], assetLibraryID, pidToTID);
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

                        TID itemID = pidToTID[ToPersistentID(assetLibraryID, assetInfo.PersistentID)];
                      
                        Guid typeGuid = TypeMap.ToGuid(assetInfo.Object.GetType());
                        if (typeGuid == Guid.Empty)
                        {
                            continue;
                        }

                        ImportAssetItem importItem = new ImportAssetItem
                        {
                            Name = name,
                            Ext = ext,
                            Object = assetInfo.Object,
                            AssetLibraryID = assetLibraryID,
                            AssetInfo = assetInfo
                        };

                        importItem.SetTypeGuid(typeGuid);

                        if (assetInfo.PrefabParts != null)
                        {
                            for (int j = 0; j < assetInfo.PrefabParts.Count; ++j)
                            {
                                PrefabPartInfo partInfo = assetInfo.PrefabParts[j];

                                if (partInfo.Object != null)
                                {
                                    Guid partTypeGuid = TypeMap.ToGuid(partInfo.Object.GetType());
                                    if (partTypeGuid == Guid.Empty)
                                    {
                                        continue;
                                    }

                                    TID partID;
                                    if(!pidToTID.TryGetValue(ToPersistentID(assetLibraryID, partInfo.PersistentID), out partID))
                                    {
                                        continue;
                                    }

                                    ProjectItem partAssetItem;
                                    if (InternalState.TryGetProjectItemFromCache(partID, out partAssetItem))
                                    {
                                        if (!m_storage.GetID(partAssetItem).Equals(itemID) || partAssetItem.GetTypeGuid() != typeGuid)
                                        {
                                            status = ImportStatus.Conflict;
                                        }
                                    }
                                }
                            }
                        }

                        if (status != ImportStatus.Conflict)
                        {
                            ProjectItem exisitingItem;
                            if (InternalState.TryGetProjectItemFromCache(itemID, out exisitingItem))
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
                        if (!doNotOverwriteItems || status != ImportStatus.Overwrite)
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
            if (item.Children != null)
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
        #endregion

        #region UnloadImportItems

        public override void UnloadImportItems(ProjectItem importItemsRoot)
        {
            if (importItemsRoot == null)
            {
                throw new ArgumentNullException("importItemsRoot");
            }

            ImportAssetItem[] importItems = importItemsRoot.Flatten(true).OfType<ImportAssetItem>().ToArray();
            for (int i = 0; i < importItems.Length; ++i)
            {
                if (importItems[i].Object != null)
                {
                    int assetLibraryID = importItems[i].AssetLibraryID;
                    if (!IsLibraryLoaded(assetLibraryID))
                    {
                        if (IsBundledLibrary(assetLibraryID))
                        {
                            UnityObject.DestroyImmediate(importItems[i].Object, true);
                            importItems[i].Object = null;
                        }
                        else if (IsBuiltinLibrary(assetLibraryID) || IsSceneLibrary(assetLibraryID) || IsStaticLibrary(assetLibraryID))
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

        #endregion

        #region ImportAsync

        public override async Task<ProjectItem[]> ImportAsync(ProjectItem[] importItems, CancellationToken ct)
        {
            if (!IsOpened)
            {
                throw new InvalidOperationException("Unable to import assets. Open project first");
            }

            ct = CreateLinkedToken(ct);

            HashSet<int> assetLibraryIds = new HashSet<int>();
            for (int i = 0; i < importItems.Length; ++i)
            {
                ImportAssetItem importItem = importItems[i] as ImportAssetItem;
                if(importItem == null)
                {
                    continue;
                }

                if (TypeMap.ToType(importItem.GetTypeGuid()) == null)
                {
                    throw new InvalidOperationException("Type of ImportItem is invalid");
                }

                assetLibraryIds.Add(importItem.AssetLibraryID);
            }

            if (assetLibraryIds.Count == 0)
            {
                return new ProjectItem[0];
            }

            if (assetLibraryIds.Count > 1)
            {
                throw new InvalidOperationException("Unabled to import more than one AssetLibrary");
            }

            int assetLibraryID = assetLibraryIds.First();
            if (IsLibraryLoaded(assetLibraryID))
            {
               return await CompleteImportAssetsAsync(importItems, assetLibraryID, false, ct);
            }

            bool loaded = await LoadLibraryAsync(assetLibraryID, ct);
            if (!loaded)
            {
                throw new InvalidOperationException($"Unabled to load AssetLibrary with {assetLibraryID} id");
            }

            return await CompleteImportAssetsAsync(importItems, assetLibraryID, true, ct);

        }
    
        private async Task<ProjectItem[]> CompleteImportAssetsAsync(ProjectItem[] importItems, int assetLibraryID, bool unloadWhenDone, CancellationToken ct)
        {
            ProjectItem[] assetItems = new ProjectItem[importItems.Length];
            object[] objects = new object[importItems.Length];

            Dictionary<int, TID> pidToTID = await LoadIDsForAssetLibraryAsync(assetLibraryID, m_loadedAssetLibraries[assetLibraryID], ct);
 
            HashSet<string> removePathHs = new HashSet<string>();
            for (int i = 0; i < importItems.Length; ++i)
            {
                ImportAssetItem importItem = (ImportAssetItem)importItems[i];
                ProjectItem parent = null;
                ProjectItem assetItem;

                TID id = pidToTID[ToPersistentID(assetLibraryID, importItem.AssetInfo.PersistentID)];
                if (InternalState.TryGetProjectItemFromCache(id, out assetItem))
                {
                    parent = assetItem.Parent;

                    string path = assetItem.ToString();
                    if (!removePathHs.Contains(path))
                    {
                        removePathHs.Add(path);
                    }
                }

                if (assetItem == null)
                {
                    assetItem = m_storage.CreateAssetItem(id, importItem.GetTypeGuid(), null, null, null);
                    InternalState.AddProjectItemToCache(id, assetItem);
                }
                else
                {
                    Debug.Assert(m_storage.GetID(assetItem).Equals(id));
                }

                TID[] embeddedIds = null;
                List<PrefabPartInfo> prefabParts = importItem.AssetInfo.PrefabParts;
                if(prefabParts != null)
                {
                    embeddedIds = new TID[prefabParts.Count];
                    for(int p = 0; p < prefabParts.Count; ++p)
                    {
                        int partID = ToPersistentID(assetLibraryID, prefabParts[p].PersistentID);
                        if(pidToTID.TryGetValue(partID, out TID embeddedId))
                        {
                            embeddedIds[p] = embeddedId;
                        }
                    }
                }

                assetItem.Name = PathHelper.GetUniqueName(importItem.Name, importItem.Ext, importItem.Parent.Children.Where(child => child != importItem).Select(child => child.NameExt).ToList());
                assetItem.Ext = importItem.Ext;
                assetItem.SetTypeGuid(importItem.GetTypeGuid());
                assetItem.SetPreview(importItem.GetPreview());
                m_storage.SetEmbeddedIDs(assetItem, embeddedIds);

                if (embeddedIds != null)
                {
                    for (int p = 0; p < embeddedIds.Length; ++p)
                    {
                        if (!InternalState.ContainsProjectItemWithKey(id))
                        {
                            InternalState.AddProjectItemToCache(id, assetItem);
                        }
                    }
                }

                if (parent == null)
                {
                    parent = RootFolder.GetOrCreateFolder(importItem.Parent.ToString());
                }

                parent.AddChild(assetItem);
                assetItems[i] = assetItem;

                UnityObject obj = FromProjectItem<UnityObject>(assetItem);
                objects[i] = obj;
                if (obj != null)
                {
                    if (AssetDB.TryToReplaceID(obj, m_storage.GetID(assetItem)))
                    {
                        Debug.Log("Object  " + obj + " is present in asset db. This means that it was already loaded from different asset library (SceneAssetLibrary for example). -> PersistentID replaced with " + m_storage.GetID(assetItem));
                    }
                }
            }

            await m_storage.DeleteAsync(InternalState.ProjectPath, removePathHs.ToArray(), ct);
            ct.ThrowIfCancellationRequested();

            assetItems = await SaveAsync(assetItems, null, null, objects, null, ct);
            if (unloadWhenDone)
            {
                await UnloadAssetLibraryAsync(assetLibraryID, ct);
            }

            RaiseImportCompleted(assetItems);
            return assetItems;
        }

        private int ToPersistentID(int assetLibraryID, int id)
        {
            return (assetLibraryID << AssetLibraryInfo.ORDINAL_OFFSET) + id;
        }

        #endregion

        protected override void UnloadUnregister()
        {
            base.UnloadUnregister();

            if (m_assetLibraryIDToLibraryName != null)
            {
                m_assetLibraryIDToLibraryName = null;
            }

            foreach (AssetBundle assetBundle in LoadedAssetBundles)
            {
                assetBundle.Unload(true);
            }

            m_assetLibraryIDToAssetBundle.Clear();
            m_assetLibraryIDToAssetBundleInfo.Clear();

            foreach (AssetLibraryAsset assetLibrary in m_loadedAssetLibraries.Values)
            {
                if (!IsBundledLibrary(assetLibrary.Ordinal))
                {
                    Resources.UnloadAsset(assetLibrary);
                }
            }

            m_loadedAssetLibraries.Clear();
        }
    }

    public class ProjectAsyncImpl<TID> : IProjectAsync, IProjectEvents, IProjectState, IProjectUtils, IDisposable where TID : IEquatable<TID>
    {
        public event EventHandler Locked;
        public event EventHandler Released;

        public event EventHandler<ProjectEventArgs<ProjectInfo[]>> GetProjectsCompleted;
        public event EventHandler<ProjectEventArgs<ProjectInfo>> CreateProjectCompleted;
        public event EventHandler<ProjectEventArgs<(string Project, string TargetProject)>> CopyProjectCompleted;
        public event EventHandler<ProjectEventArgs<string>> DeleteProjectCompleted;
        public event EventHandler<ProjectEventArgs<(string Project, string TargetPath)>> ExportProjectCompleted;
        public event EventHandler<ProjectEventArgs<(string Project, string SourcePath)>> ImportProjectCompleted;
        public event EventHandler<ProjectEventArgs<ProjectInfo>> OpenProjectCompleted;
        public event EventHandler<ProjectEventArgs<string>> CloseProjectCompleted;

        public event EventHandler<ProjectEventArgs<object[]>> BeginSave;
        public event EventHandler<ProjectEventArgs<(ProjectItem[] SavedItems, bool IsUserAction)>> SaveCompleted;
        public event EventHandler<ProjectEventArgs<ProjectItem[]>> BeginLoad;
        public event EventHandler<ProjectEventArgs<(ProjectItem[] LoadedItems, UnityObject[] LoadedObjects)>> LoadCompleted;
        public event EventHandler<ProjectEventArgs<(ProjectItem[] OriginalItems, ProjectItem[] DuplicatedItems)>> DuplicateCompleted;
        public event EventHandler<ProjectEventArgs<ProjectItem[]>> UnloadCompleted;
        public event EventHandler<ProjectEventArgs> UnloadAllCompleted;

        public event EventHandler<ProjectEventArgs<ProjectItem[]>> CreatePrefabsCompleted;
        public event EventHandler<ProjectEventArgs<ProjectItem[]>> CreateFoldersCompleted;
        public event EventHandler<ProjectEventArgs<(ProjectItem[] OriginalParentItems, ProjectItem[] MovedItems)>> MoveCompleted;
        public event EventHandler<ProjectEventArgs<ProjectItem[]>> RenameCompleted;
        public event EventHandler<ProjectEventArgs<ProjectItem[]>> DeleteCompleted;
        public event EventHandler<ProjectEventArgs<ProjectItem[]>> ImportCompleted;

        protected void RaiseImportCompleted(ProjectItem[] projectItems)
        {
            ImportCompleted?.Invoke(this, new ProjectEventArgs<ProjectItem[]>(projectItems));
        }
        //private IProjectToProjectAsyncEvents m_eventForwarder;

        private IAssetDB<TID> m_assetDB;
        protected IAssetDB<TID> AssetDB
        {
            get { return m_assetDB != null ? m_assetDB : m_assetDB = IOC.Resolve<IAssetDB<TID>>(); }
        }

        private ITypeMap m_typeMap;
        protected ITypeMap TypeMap
        {
            get { return m_typeMap != null ? m_typeMap : m_typeMap = IOC.Resolve<ITypeMap>(); }
        }

        private IRuntimeSceneManager m_sceneManager;
        protected IStorageAsync<TID> m_storage;
        private IIDGenerator<TID> m_idGen;
        private IUnityObjectFactory m_factory;
        private IProjectAsyncState<TID> m_state;
        protected IProjectAsyncState<TID> InternalState
        {
            get { return m_state; }
        }

        public IProjectEvents Events
        {
            get { return this; }
        }

        public IProjectState State
        {
            get { return this; }
        }

        public IProjectUtils Utils
        {
            get { return this; }
        }

        private ProjectAsyncSafe m_safe;
        public IProjectAsyncSafe Safe
        {
            get { return m_safe; }
        }

        public IProjectAsync Project
        {
            get { return this; }
        }

        public bool IsOpened
        {
            get { return ProjectInfo != null; }
        }

        public ProjectInfo ProjectInfo
        {
            get { return m_state.ProjectInfo; }
        }

        public ProjectItem RootFolder
        {
            get { return m_state.RootFolder; }
        }

        public ProjectItem LoadedScene
        {
            get { return m_state.LoadedScene; }
            set { m_state.LoadedScene = value; }
        }

        public virtual AssetBundle[] LoadedAssetBundles
        {
            get { return new AssetBundle[0]; }
        }

        public ProjectAsyncImpl()
        {
            m_storage = IOC.Resolve<IStorageAsync<TID>>();
            m_idGen = IOC.Resolve<IIDGenerator<TID>>();
            m_factory = IOC.Resolve<IUnityObjectFactory>();
            m_state = IOC.Resolve<IProjectAsyncState<TID>>();

            m_sceneManager = IOC.Resolve<IRuntimeSceneManager>();
            if (m_sceneManager != null)
            {
                m_sceneManager.NewSceneCreating += OnNewSceneCreating;
            }
            
            m_safe = new ProjectAsyncSafe(this);
        }

        public virtual void Dispose()
        {
            UnloadUnregisterDestroy();

            m_state.Cancel();

            if (m_sceneManager != null)
            {
                m_sceneManager.NewSceneCreating -= OnNewSceneCreating;
                m_sceneManager = null;
            }
            m_state = null;
            m_storage = null;
            m_idGen = null;
            m_assetDB = null;
            m_typeMap = null;
            m_factory = null;
            m_safe = null;
        }

        private void OnNewSceneCreating(object sender, EventArgs e)
        {
            m_state.LoadedScene = null;
        }

        protected CancellationToken CreateLinkedToken(CancellationToken ct)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(ct, m_state.CancellationToken).Token;
        }

        protected virtual Task<TID[]> GenerateIdentifiersAsync(int count, CancellationToken ct)
        {
            return m_idGen.GenerateAsync(count, ct);
        }

        protected virtual void OnAssetBundleInfoLoaded(AssetBundleInfo[] assetBundleInfo)
        {
            //#warning TODO: use assetBundleInfo
        }

        protected virtual async Task ResolveDependenciesAsync(ProjectItem[] projectItems, HashSet<TID> unresolvedDependencies, CancellationToken ct)
        {
            //#warning TODO: load asset libraries
            await Task.Yield();
            ct.ThrowIfCancellationRequested();
        }

        protected virtual async Task LoadLibraryWithSceneDependenciesAsync(CancellationToken ct)
        {
            //#warning TODO: add ability to use asset libraries with TID
            await Task.Yield();
            ct.ThrowIfCancellationRequested();
        }

        protected virtual async Task LoadAllAssetLibrariesAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            //#warning TODO: load all asset libraries
            await Task.Yield();
            ct.ThrowIfCancellationRequested();
        }

        public virtual async Task<ProjectItem> LoadImportItemsAsync(string path, bool isBuiltIn, CancellationToken ct)
        {
            await Task.Yield();
            CreateLinkedToken(ct).ThrowIfCancellationRequested();
            return null;
        }
        public virtual void UnloadImportItems(ProjectItem importItemsRoot)
        {
        }

        public virtual async Task<ProjectItem[]> ImportAsync(ProjectItem[] importItems, CancellationToken ct)
        {
            await Task.Yield();
            CreateLinkedToken(ct).ThrowIfCancellationRequested();

            ProjectItem[] result = new ProjectItem[0];
            ImportCompleted?.Invoke(this, new ProjectEventArgs<ProjectItem[]>(result));
            return result;
        }

        public virtual Task<string[]> GetAssetBundlesAsync(CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            var tcs = new TaskCompletionSource<string[]>();

            IOC.Resolve<IAssetBundleLoader>().GetAssetBundles(result =>
            {
                using (ct.Register(() => tcs.TrySetCanceled(ct)))
                {
                    if (!ct.IsCancellationRequested)
                    {
                        tcs.TrySetResult(result);
                    }
                }
            });

            return tcs.Task;
        }

        public virtual async Task<string[]> GetStaticAssetLibrariesAsync(CancellationToken ct)
        {
            await Task.Yield();
            CreateLinkedToken(ct).ThrowIfCancellationRequested();
            return new string[0];
        }

        public bool IsStatic(ProjectItem projectItem)
        {
            if (projectItem.IsFolder)
            {
                return false;
            }

            return AssetDB.IsStaticResourceID(m_storage.GetID(projectItem));
        }

        public bool IsScene(ProjectItem projectItem)
        {
            if (projectItem.IsFolder)
            {
                return false;
            }

            return ToType(projectItem) == typeof(Scene);
        }

        public bool IsUnityObject(ProjectItem projectItem)
        {
            if(projectItem.IsFolder)
            {
                return false;
            }

            Type type = ToType(projectItem);
            if(type == null)
            {
                return false;
            }

            return type.IsSubclassOf(typeof(UnityObject));
        }

        public Type ToType(ProjectItem assetItem)
        {
            return TypeMap.ToType(assetItem.GetTypeGuid());
        }

        public Guid ToGuid(Type type)
        {
            return TypeMap.ToGuid(type);
        }
        public object ToPersistentID(UnityObject obj)
        {
            return AssetDB.ToID(obj);
        }

        public object ToPersistentID(ProjectItem projectItem)
        {
            return m_storage.GetID(projectItem);
        }

        public T FromPersistentID<T>(object id) where T : UnityObject
        {
            return AssetDB.FromID<T>((TID)id);
        }

        public T FromPersistentID<T>(ProjectItem projectItem) where T : UnityObject
        {
            return AssetDB.FromID<T>(m_storage.GetID(projectItem));
        }

        public void SetPersistentID(ProjectItem projectItem, object id)
        {
            m_storage.SetID(projectItem, (TID)id);
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

        public ProjectItem CreateAssetItem(Guid typeGuid, string name, string ext, ProjectItem parent = null)
        {
            return m_storage.CreateAssetItem(default(TID), typeGuid, name, ext, parent);
        }

        public ProjectItem ToProjectItem(UnityObject obj)
        {
            ProjectItem result;

            TID id = AssetDB.ToID(obj);
            if (!m_state.TryGetProjectItemFromCache(id, out result))
            {
                return null;
            }
            return result;
        }

        public T FromProjectItem<T>(ProjectItem projectItem) where T : UnityObject
        {
            return m_assetDB.FromID<T>(m_storage.GetID(projectItem));
        }

        public ProjectItem[] GetProjectItemsDependentOn(ProjectItem[] projectItems)
        {
            HashSet<ProjectItem> resultHs = new HashSet<ProjectItem>();
            Queue<ProjectItem> queue = new Queue<ProjectItem>();
            for (int i = 0; i < projectItems.Length; ++i)
            {
                queue.Enqueue(projectItems[i]);
            }

            while (queue.Count > 0)
            {
                ProjectItem projectItem = queue.Dequeue();
                if (!resultHs.Add(projectItem))
                {
                    continue;
                }

                TID projectItemID = m_storage.GetID(projectItem);
                foreach (ProjectItem cachedItem in m_state.ProjectItemsCache)
                {
                    TID[] dependencies = m_storage.GetDependencyIDs(cachedItem);
                    if (dependencies != null)
                    {
                        for (int i = 0; i < dependencies.Length; ++i)
                        {
                            if (projectItemID.Equals(dependencies[i]))
                            {
                                queue.Enqueue(cachedItem);
                                break;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
                resultHs.Remove(projectItem);
            }
            return resultHs.ToArray();
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

        public Task<Lock.LockReleaser> LockAsync(CancellationToken ct)
        {
            return m_state.LockAsync(
                () => Locked?.Invoke(this, EventArgs.Empty),
                () => Released?.Invoke(this, EventArgs.Empty),
                CreateLinkedToken(ct));
        }

        public async Task<ProjectInfo[]> GetProjectsAsync(CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            ProjectInfo[] result = await m_storage.GetProjectsAsync(ct);
            ct.ThrowIfCancellationRequested();

            GetProjectsCompleted?.Invoke(this, new ProjectEventArgs<ProjectInfo[]>(result));
            return result;
        }

        public async Task<ProjectInfo> CreateProjectAsync(string project, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            ProjectInfo projectInfo = await m_storage.CreateProjectAsync(project, ct);
            ct.ThrowIfCancellationRequested();

            CreateProjectCompleted?.Invoke(this, new ProjectEventArgs<ProjectInfo>(projectInfo));
            return projectInfo;
        }

        public async Task CopyProjectAsync(string project, string targetProject, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            await m_storage.CopyProjectAsync(project, targetProject, ct);
            ct.ThrowIfCancellationRequested();

            CopyProjectCompleted?.Invoke(this, new ProjectEventArgs<(string, string)>((project, targetProject)));
        }

        public async Task DeleteProjectAsync(string project, CancellationToken ct)
        {
            if(string.IsNullOrEmpty(project))
            {
                return;
            }

            ct = CreateLinkedToken(ct);

            await m_storage.DeleteProjectAsync(project, ct);
            ct.ThrowIfCancellationRequested();

            DeleteProjectCompleted?.Invoke(this, new ProjectEventArgs<string>(project));
        }

        public async Task ExportProjectAsync(string project, string targetPath, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            await m_storage.ExportProjectAsync(project, targetPath, ct);
            ct.ThrowIfCancellationRequested();

            ExportProjectCompleted?.Invoke(this, new ProjectEventArgs<(string, string)>((project, targetPath)));
        }

        public async Task ImportProjectAsync(string project, string sourcePath, bool overwrite, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            if (overwrite)
            {
                await m_storage.DeleteProjectAsync(project, ct);
                ct.ThrowIfCancellationRequested();
            }

            await m_storage.ImportProjectAsync(project, sourcePath, ct);
            ct.ThrowIfCancellationRequested();

            ImportProjectCompleted?.Invoke(this, new ProjectEventArgs<(string, string)>((project, sourcePath)));
        }

        public async Task<ProjectInfo> OpenProjectAsync(string project, OpenProjectFlags flags, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

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
                    m_sceneManager.CreateNewScene();
                }
                else
                {
                    if ((flags & OpenProjectFlags.ClearScene) != 0)
                    {
                        m_sceneManager.ClearScene();
                    }
                    m_state.LoadedScene = null;
                }
            }

            var (projectInfo, assetBundleInfo) = await m_storage.GetOrCreateProjectAsync(project, ct);
            ct.ThrowIfCancellationRequested();

            OnAssetBundleInfoLoaded(assetBundleInfo);

            m_state.RootFolder = await m_storage.GetProjectTreeAsync(project, ct);
            ct.ThrowIfCancellationRequested();

            m_state.ProjectInfo = projectInfo;
            m_state.ProjectPath = project;
            m_state.ClearProjectItemsCache();

            ProjectItem[] projectItems = m_state.FlattenHierarchy(true);
            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];

                m_state.AddProjectItemToCache(m_storage.GetID(projectItem), projectItem);

                TID[] embeddedObjectIDs = m_storage.GetEmbeddedIDs(projectItem);
                if (embeddedObjectIDs != null)
                {
                    for (int j = 0; j < embeddedObjectIDs.Length; ++j)
                    {
                        m_state[embeddedObjectIDs[j]] = projectItem;
                    }
                }
            }

            OpenProjectCompleted?.Invoke(this, new ProjectEventArgs<ProjectInfo>(projectInfo));
            return m_state.ProjectInfo;
        }

        public async Task CloseProjectAsync(CancellationToken ct)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            if (m_state.ProjectInfo != null)
            {
                UnloadUnregisterDestroy();
            }

            string projectPath = m_state.ProjectPath;

            m_state.ProjectPath = null;
            m_state.ProjectInfo = null;
            m_state.RootFolder = null;
            m_state.LoadedScene = null;

            m_sceneManager.ClearScene();

            CloseProjectCompleted?.Invoke(this, new ProjectEventArgs<string>(projectPath));
        }

        public async Task<byte[][]> GetPreviewsAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            Preview<TID>[] previews = await m_storage.GetPreviewsAsync(m_state.ProjectPath, projectItems.Select(f => f.ToString()).ToArray(), ct);
            ct.ThrowIfCancellationRequested();

            return previews.Select(preview => preview.PreviewData).ToArray();
        }

        public async Task<ProjectItem[]> GetAssetItemsAsync(ProjectItem[] folders, string searchPattern, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            Preview<TID>[][] previewsPerFolder = await m_storage.GetPreviewsPerFolderAsync(m_state.ProjectPath, folders.Select(f => f.ToString()).ToArray(), searchPattern, ct);
            ct.ThrowIfCancellationRequested();

            for (int i = 0; i < previewsPerFolder.Length; ++i)
            {
                ProjectItem folder = folders[i];
                Preview<TID>[] previews = previewsPerFolder[i];
                if (previews != null && previews.Length > 0)
                {
                    for (int j = 0; j < previews.Length; ++j)
                    {
                        Preview<TID> preview = previews[j];
                        ProjectItem projectItem;

                        TID id = preview.ID;
                        if (m_state.TryGetProjectItemFromCache(id, out projectItem))
                        {
                            if (projectItem.Parent == null)
                            {
                                Debug.LogErrorFormat("asset item {0} parent is null", projectItem.ToString());
                                continue;
                            }

                            if (projectItem.Parent != folder)
                            {
                                Debug.LogErrorFormat("asset item {0} with wrong parent selected. Expected parent {1}. Actual parent {2}", folder.ToString(), projectItem.Parent.ToString());
                                continue;
                            }

                            projectItem.SetPreview(preview.PreviewData);
                        }
                        else
                        {
                            Debug.LogWarningFormat("AssetItem with ItemID {0} does not exist", id);
                        }
                    }
                }
            }

            if (searchPattern == null)
            {
                searchPattern = string.Empty;
            }

            return folders.Where(f => f.Children != null).SelectMany(f => f.Children).Where(item => item.Name.ToLower().Contains(searchPattern.ToLower())).ToArray();
        }

        public async Task<object[]> GetDependenciesAsync(object obj, bool exceptMappedObjects, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            Type objType = obj.GetType();
            Type persistentType = TypeMap.ToPersistentType(objType);
            if (persistentType == null)
            {
                Debug.LogWarningFormat(string.Format("PersistentClass for {0} does not exist. To create or edit persistent classes click Tools->Runtime SaveLoad->Persistent Classes->Edit.", obj.GetType()), "obj");
                return new object[0];
            }

            await LoadLibraryWithSceneDependenciesAsync(ct);
            return await FindDeepDependenciesAsync(obj, exceptMappedObjects);
        }

        public Task SaveAsync(ProjectItem[] projectItems, object[] obj, bool isUserAction, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            return SaveAsync(projectItems, null, null, obj, null, isUserAction, ct);
        }

        public Task SaveAsync(ProjectItem[] projectItems, object[] obj, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            return SaveAsync(projectItems, null, null, obj, null, true, ct);
        }

        public Task<ProjectItem[]> SaveAsync(ProjectItem[] folders, byte[][] previewData, object[] obj, string[] nameOverrides, bool isUserAction, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            return SaveAsync(null, folders, previewData, obj, nameOverrides, isUserAction, ct);
        }

        public async Task SavePreviewsAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            BeginSave?.Invoke(this, new ProjectEventArgs<object[]>(new object[0]));
            try
            {
                projectItems = projectItems.Where(projectItem => projectItem.Parent != null).ToArray();
                await m_storage.SaveAsync(m_state.ProjectPath, projectItems.Select(item => item.Parent.ToString()).ToArray(), projectItems, null, m_state.ProjectInfo, true, ct);
                ct.ThrowIfCancellationRequested();

                SaveCompleted?.Invoke(this, new ProjectEventArgs<(ProjectItem[], bool)>((projectItems, false)));
            }
            catch (Exception)
            {
                SaveCompleted?.Invoke(this, new ProjectEventArgs<(ProjectItem[], bool)>((new ProjectItem[0], true), true));
                throw;
            }
        }

        private async Task<ProjectItem[]> SaveAsync(ProjectItem[] existingAssetItems, ProjectItem[] folders, byte[][] previewData, object[] objects, string[] nameOverrides, bool isUserAction, CancellationToken ct)
        {
            BeginSave?.Invoke(this, new ProjectEventArgs<object[]>(objects));
            try
            {
                ProjectItem[] savedItems = await SaveAsync(existingAssetItems, folders, previewData, objects, nameOverrides, ct);
                SaveCompleted?.Invoke(this, new ProjectEventArgs<(ProjectItem[], bool)>((savedItems, isUserAction)));
                return savedItems;
            }
            catch (Exception)
            {
                SaveCompleted?.Invoke(this, new ProjectEventArgs<(ProjectItem[], bool)>((new ProjectItem[0], isUserAction), true));
                throw;
            }
        }

        protected async Task<ProjectItem[]> SaveAsync(ProjectItem[] existingAssetItems, ProjectItem[] folders, byte[][] previewData, object[] objects, string[] nameOverrides, CancellationToken ct)
        {
            if (!IsOpened)
            {
                throw new InvalidOperationException("Project is not opened. Use OpenProjectAsync method");
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
                        IEquatable<TID> id = m_storage.GetID(existingAssetItems[i]);
                        if (!id.Equals(AssetDB.ToID((UnityObject)obj)))
                        {
                            throw new ArgumentException("Unable to override item with different object: assetItemID != obj.PersistentID. Either delete the AssetItem, or load the object before updating.");
                        }
                    }
                }
            }

            await LoadLibraryWithSceneDependenciesAsync(ct);

            if (folders == null)
            {
                if (existingAssetItems == null)
                {
                    folders = new ProjectItem[objects.Length];
                    for (int i = 0; i < folders.Length; ++i)
                    {
                        folders[i] = m_state.RootFolder;
                    }
                }
            }
            else
            {
                for (int i = 0; i < folders.Length; ++i)
                {
                    if (!folders[i].IsFolder)
                    {
                        throw new ArgumentException("parent is not folder", "parent");
                    }
                }

                if (folders.Length == 0)
                {
                    return new ProjectItem[0];
                }

                int parentIndex = folders.Length - 1;
                ProjectItem lastParent = folders[parentIndex];
                Array.Resize(ref folders, objects.Length);
                for (int i = parentIndex + 1; i < folders.Length; ++i)
                {
                    folders[i] = lastParent;
                }
            }

            List<UnityObject> notMapped = new List<UnityObject>();
            for (int o = 0; o < objects.Length; ++o)
            {
                object obj = objects[o];
                Type objType = obj.GetType();
                Type persistentType = TypeMap.ToPersistentType(objType);
                if (persistentType == null)
                {
                    Debug.LogWarningFormat(string.Format("PersistentClass for {0} does not exist. Tools->Runtime SaveLoad->Persistent Classes->Edit", obj.GetType()), "obj");
                    continue;
                }

                ProjectItem existingAssetItem = existingAssetItems != null ? existingAssetItems[o] : null;
                if (existingAssetItem == null || AssetDB.IsDynamicResourceID(m_storage.GetID(existingAssetItem)))
                {
                    if (existingAssetItem != null && obj is UnityObject)
                    {
                        IEquatable<TID> id = m_storage.GetID(existingAssetItem);
                        /* object with correct identifier already exists. Only new prefab parts should be processed */
                        Debug.Assert(id.Equals(AssetDB.ToID((UnityObject)obj)));
                    }

                    if (obj is GameObject)
                    {
                        GetUnmappedObjects((GameObject)obj, notMapped);
                    }
                    else if (obj is UnityObject)
                    {
                        if (!AssetDB.IsMapped((UnityObject)obj))
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

            TID[] ids = await GenerateIdentifiersAsync(notMapped.Count, ct);
            ct.ThrowIfCancellationRequested();

            for (int i = 0; i < notMapped.Count; ++i)
            {
                AssetDB.RegisterDynamicResource(ids[i], notMapped[i]);
            }

            ProjectItem[] assetItems = existingAssetItems == null ? new ProjectItem[objects.Length] : existingAssetItems;
            PersistentObject<TID>[] persistentObjects = new PersistentObject<TID>[objects.Length];
            Dictionary<ProjectItem, List<ProjectItem>> parentToPotentialChildren = null;
            if (folders != null)
            {
                parentToPotentialChildren = new Dictionary<ProjectItem, List<ProjectItem>>();
                for (int i = 0; i < folders.Length; ++i)
                {
                    if (!parentToPotentialChildren.ContainsKey(folders[i]))
                    {
                        if (folders[i].Children == null)
                        {
                            folders[i].Children = new List<ProjectItem>();
                        }
                        parentToPotentialChildren.Add(folders[i], folders[i].Children.ToList());
                    }
                }
            }

            HashSet<int> assetLibraryIds = new HashSet<int>();
            for (int objIndex = 0; objIndex < objects.Length; ++objIndex)
            {
                object obj = objects[objIndex];
                Type objType = obj.GetType();
                Type persistentType = TypeMap.ToPersistentType(objType);
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

                if(persistentObject is PersistentRuntimePrefab<TID>)
                {
                    PersistentRuntimePrefab<TID> persistentPrefab = (PersistentRuntimePrefab<TID>)persistentObject;
                    if(persistentPrefab != null)
                    {
                        await persistentPrefab.ReadFromAsync(obj);
                    }
                }
                else
                {
                    persistentObject.ReadFrom(obj);
                }
                

                if (objIndex % RTSLSettings.ProjectReadsPerBatch == RTSLSettings.ProjectReadsPerBatch - 1)
                {
                    await Task.Yield();
                }

                ProjectItem assetItem = assetItems[objIndex];
                if (assetItem == null)
                {
                    if (!string.IsNullOrEmpty(nameOverride))
                    {
                        persistentObject.name = nameOverride;
                    }

                    List<ProjectItem> potentialChildren = parentToPotentialChildren[folders[objIndex]];
                    persistentObject.name = PathHelper.RemoveInvalidFileNameCharacters(persistentObject.name);
                    persistentObject.name = PathHelper.GetUniqueName(persistentObject.name, GetExt(obj), potentialChildren.Select(c => c.NameExt).ToList());

                    TID id;
                    if (obj is Scene)
                    {
                        id = AssetDB.ToID(notMapped[objIndex]);
                        UnityObject.Destroy(notMapped[objIndex]);
                    }
                    else
                    {
                        id = AssetDB.ToID((UnityObject)obj);
                    }

                    assetItem = m_storage.CreateAssetItem(id, TypeMap.ToGuid(obj.GetType()), persistentObject.name, GetExt(obj), folders[objIndex]);
                    potentialChildren.Add(assetItem);
                }

                if (previewData != null)
                {
                    assetItem.SetPreview(previewData[objIndex]);
                }


                if (persistentObject is PersistentRuntimePrefab<TID> && !(persistentObject is PersistentRuntimeScene<TID>))
                {
                    PersistentRuntimePrefab<TID> persistentPrefab = (PersistentRuntimePrefab<TID>)persistentObject;
                    if (persistentPrefab.Descriptors != null)
                    {
                        List<TID> embeddedIds = new List<TID>();
                        PersitentDescriptorToEmbeddedObjectIDs(persistentPrefab.Descriptors, embeddedIds);
                        m_storage.SetEmbeddedIDs(assetItem, embeddedIds.ToArray());
                    }
                }

                GetDepsContext<TID> getDepsCtx = new GetDepsContext<TID>();
                persistentObject.GetDeps(getDepsCtx);
                m_storage.SetDependencyIDs(assetItem, getDepsCtx.Dependencies.ToArray());

                assetLibraryIds.Clear();
                foreach (TID id in getDepsCtx.Dependencies)
                {
                    int assetLibraryID = m_assetDB.GetAssetLibraryID(id);
                    if (assetLibraryID != -1)
                    {
                        assetLibraryIds.Add(assetLibraryID);
                    }
                }

                if (assetLibraryIds.Count > 0)
                {
                    assetItem.SetAssetLibraryIDs(assetLibraryIds.ToArray());
                }
                else
                {
                    assetItem.SetAssetLibraryIDs(null);
                }

                persistentObjects[objIndex] = persistentObject;
                assetItems[objIndex] = assetItem;
            }

            for (int i = 0; i < notMapped.Count; ++i)
            {
                AssetDB.UnregisterDynamicResource(ids[i]);
            }

            assetItems = assetItems.Where(ai => ai != null).ToArray();
            persistentObjects = persistentObjects.Where(p => p != null).ToArray();

            string[] path = assetItems.Select(ai => ai.Parent.ToString()).ToArray();
            await m_storage.SaveAsync(m_state.ProjectPath, path, assetItems, persistentObjects, m_state.ProjectInfo, false, ct);
            ct.ThrowIfCancellationRequested();

            for (int objIndex = 0; objIndex < assetItems.Length; ++objIndex)
            {
                ProjectItem assetItem = assetItems[objIndex];
                TID[] embeddedIds = m_storage.GetEmbeddedIDs(assetItem);
                if (embeddedIds != null)
                {
                    for (int i = 0; i < embeddedIds.Length; ++i)
                    {
                        TID embeddedObjectID = embeddedIds[i];
                        if (!m_state.ContainsProjectItemWithKey(embeddedObjectID))
                        {
                            m_state.AddProjectItemToCache(embeddedObjectID, assetItem);
                        }
                    }
                }

                TID assetItemID = m_storage.GetID(assetItem);
                if (!m_state.ContainsProjectItemWithKey(assetItemID))
                {
                    m_state.AddProjectItemToCache(assetItemID, assetItem);
                }

                if (folders != null)
                {
                    folders[objIndex].AddChild(assetItem);
                }
            }

            return assetItems;
        }

        [Obsolete]
        public object[] FindDeepDependencies(object obj)
        {
            Task<object[]> task = FindDeepDependenciesAsync(obj);
            while(true)
            {
                if(task.IsCanceled)
                {
                    throw new TaskCanceledException();
                }

                if (task.IsFaulted)
                {
                    throw task.Exception;
                }

                if(task.IsCompleted)
                {
                    break;
                }
            }

            return task.Result;
        }

        public Task<object[]> FindDeepDependenciesAsync(object obj)
        {
            return FindDeepDependenciesAsync(obj, false);
        }

        private async Task<object[]> FindDeepDependenciesAsync(object obj, bool exceptMappedObject)
        {
            Type objType = obj.GetType();
            Type persistentType = TypeMap.ToPersistentType(objType);
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
                    if (((UnityObject)dep) == null || exceptMappedObject && AssetDB.IsMapped((UnityObject)dep))
                    {
                        continue;
                    }
                    ctx.Dependencies.Add(dep);
                }
            }

            Queue<UnityObject> depsQueue = new Queue<UnityObject>(deps.OfType<UnityObject>());
            await FindDeepDependenciesAsync(depsQueue, exceptMappedObject, ctx);

            object[] dependencies;
            if (exceptMappedObject)
            {
                dependencies = ctx.Dependencies.Where(d => d is UnityObject && !AssetDB.IsMapped((UnityObject)d)).ToArray();
            }
            else
            {
                dependencies = ctx.Dependencies.ToArray();
            }

            return dependencies;
        }

        private async Task FindDeepDependenciesAsync(Queue<UnityObject> depsQueue, bool exceptMappedObject, GetDepsFromContext ctx)
        {
            GetDepsFromContext getDepsCtx = new GetDepsFromContext();
            int iter = 0;
            while (depsQueue.Count > 0)
            {
                UnityObject uo = depsQueue.Dequeue();
                if (!uo)
                {
                    continue;
                }

                if (exceptMappedObject && AssetDB.IsMapped(uo))
                {
                    continue;
                }

                if (!(uo is GameObject) && !(uo is Component))
                {
                    Type persistentType = TypeMap.ToPersistentType(uo.GetType());
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

                        if (iter % RTSLSettings.ProjectFindDeepDependenciesPerBatch == RTSLSettings.ProjectFindDeepDependenciesPerBatch - 1)
                        {
                            await Task.Yield();
                        }
                        iter++;
                    }
                }
            }
        }

        private void GetUnmappedObjects(GameObject go, List<UnityObject> notMapped)
        {
            if (go.GetComponent<RTSLIgnore>())
            {
                return;
            }

            if (!AssetDB.IsMapped(go))
            {
                notMapped.Add(go);
            }

            Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; ++i)
            {
                Transform tf = transforms[i];
                if (tf.gameObject != go && !AssetDB.IsMapped(tf.gameObject))
                {
                    notMapped.Add(tf.gameObject);
                }

                Component[] components = tf.GetComponents<Component>();
                for (int j = 0; j < components.Length; ++j)
                {
                    Component comp = components[j];
                    if (!AssetDB.IsMapped(comp))
                    {
                        notMapped.Add(comp);
                    }
                }
            }
        }

        private void PersitentDescriptorToEmbeddedObjectIDs(PersistentDescriptor<TID>[] descriptors, List<TID> embeddedObjectIds, bool includeRoot = false)
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
                    Type persistentType = TypeMap.ToType(descriptor.PersistentTypeGuid);
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
                            type = TypeMap.ToUnityType(persistentType);
                        }
                        else
                        {
                            type = TypeMap.ToType(descriptor.RuntimeTypeGuid);
                        }

                        if (type == null)
                        {
                            Debug.LogWarningFormat("Unable to get unity type from persistent type {1}", persistentType.FullName);
                            checkPassed = false;
                        }
                        else
                        {
                            Guid typeGuid = TypeMap.ToGuid(type);
                            if (typeGuid == Guid.Empty)
                            {
                                Debug.LogWarningFormat("Unable convert type {0} to guid", type.FullName);
                                checkPassed = false;
                            }
                        }
                    }

                    if (checkPassed && includeRoot)
                    {
                        embeddedObjectIds.Add(descriptor.PersistentID);
                    }

                    PersitentDescriptorToEmbeddedObjectIDs(descriptor.Children, embeddedObjectIds, true);
                    PersitentDescriptorToEmbeddedObjectIDs(descriptor.Components, embeddedObjectIds, true);
                }
            }
        }

        public async Task<UnityObject[]> LoadAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            BeginLoad?.Invoke(this, new ProjectEventArgs<ProjectItem[]>(projectItems));
            try
            {
                UnityObject[] objects = await _LoadAsync(projectItems, ct);
                LoadCompleted?.Invoke(this, new ProjectEventArgs<(ProjectItem[], UnityObject[])>((projectItems, objects)));
                return objects;
            }
            catch (Exception)
            {
                LoadCompleted?.Invoke(this, new ProjectEventArgs<(ProjectItem[], UnityObject[])>((projectItems, null), true));
                throw;
            }
        }

        private async Task<UnityObject[]> _LoadAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            HashSet<ProjectItem> loadProjectItemsHs = new HashSet<ProjectItem>();
            HashSet<TID> unresolvedDependencies = new HashSet<TID>();
            GetProjectItemsToLoad(projectItems, loadProjectItemsHs, unresolvedDependencies);

            ProjectItem[] rootAssetItems = projectItems;
            projectItems = loadProjectItemsHs.ToArray();

            await ResolveDependenciesAsync(projectItems, unresolvedDependencies, ct);

            Type[] persistentTypes = projectItems.Select(item => TypeMap.ToPersistentType(TypeMap.ToType(item.GetTypeGuid()))).ToArray();
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

            object[] persistentObjects = await m_storage.LoadAsync(m_state.ProjectPath, projectItems, persistentTypes, ct);
            ct.ThrowIfCancellationRequested();

            await LoadAllAssetLibrariesAsync(projectItems, ct);

            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem assetItem = projectItems[i];
                TID assetItemId = m_storage.GetID(assetItem);
                if (!AssetDB.IsMapped(assetItemId))
                {
                    if (AssetDB.IsDynamicResourceID(assetItemId))
                    {
                        PersistentObject<TID> persistentObject = persistentObjects[i] as PersistentObject<TID>;
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
                                persistentPrefab.CreateGameObjectWithComponents(TypeMap, persistentPrefab.Descriptors[0], idToObj, m_state.RuntimePrefabsRoot, createdGameObjects);
                                AssetDB.RegisterDynamicResources(idToObj);
                                for (int j = 0; j < createdGameObjects.Count; ++j)
                                {
                                    GameObject createdGO = createdGameObjects[j];
                                    createdGO.hideFlags = HideFlags.HideAndDontSave;
                                }
                            }
                            else
                            {
                                Type type = TypeMap.ToType(assetItem.GetTypeGuid());
                                if (type != null)
                                {
                                    if (m_factory.CanCreateInstance(type, persistentObject))
                                    {
                                        UnityObject instance = m_factory.CreateInstance(type, persistentObject);
                                        if (instance != null)
                                        {
                                            AssetDB.RegisterDynamicResource(m_storage.GetID(assetItem), instance);
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarning("Unable to create object of type " + type.ToString());
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning("Unable to resolve type by guid " + assetItem.GetTypeGuid());
                                }
                            }
                        }
                    }
                }
            }

            PersistentRuntimeScene<TID> persistentScene = null;
            for (int i = 0; i < persistentObjects.Length; ++i)
            {
                ProjectItem assetItem = projectItems[i];
                IPersistentSurrogate persistentObject = persistentObjects[i] as PersistentObject<TID>;
                if (persistentObject != null)
                {
                    if (persistentObject is PersistentRuntimeScene<TID>)
                    {
                        persistentScene = (PersistentRuntimeScene<TID>)persistentObject;
                        m_state.LoadedScene = assetItem;
                    }
                    else
                    {
                        TID assetItemID = m_storage.GetID(assetItem);
                        UnityObject obj = AssetDB.FromID<UnityObject>(assetItemID);
                        if (obj != null)
                        {
                            if (persistentObject is PersistentRuntimePrefab<TID>)
                            {
                                var persistentRuntimePrefab = (PersistentRuntimePrefab<TID>)persistentObject;
                                await persistentRuntimePrefab.WriteToAsync(obj);
                            }
                            else
                            {
                                persistentObject.WriteTo(obj);
                            }

                            obj.name = assetItem.Name;
                            if (AssetDB.IsDynamicResourceID(assetItemID))
                            {
                                obj.hideFlags = HideFlags.HideAndDontSave;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Unable to find UnityEngine.Object for " + projectItems[i].ToString() + " with id: " + m_storage.GetID(projectItems[i]) + ". This typically means that asset or corresponding asset library was removed (or its asset library id was changed).");
                        }
                    }
                }
            }

            if (persistentScene != null)
            {
                await persistentScene.WriteToAsync(SceneManager.GetActiveScene());
            }

            return rootAssetItems.Select(rootItem => AssetDB.FromID<UnityObject>(m_storage.GetID(rootItem))).ToArray();
        }

        private void GetProjectItemsToLoad(ProjectItem[] projectItems, HashSet<ProjectItem> loadHs, HashSet<TID> unresolvedDependencies)
        {
            for (int a = 0; a < projectItems.Length; ++a)
            {
                ProjectItem projectItem = projectItems[a];
                Type type = TypeMap.ToType(projectItem.GetTypeGuid());
                if (type == null)
                {
                    continue;
                }

                Type persistentType = TypeMap.ToPersistentType(type);
                if (persistentType == null)
                {
                    continue;
                }

                if (!loadHs.Contains(projectItem) && !AssetDB.IsMapped(m_storage.GetID(projectItem)))
                {
                    loadHs.Add(projectItem);
                    TID[] dependencies = m_storage.GetDependencyIDs(projectItem);
                    if (dependencies != null)
                    {
                        List<ProjectItem> depsList = new List<ProjectItem>();
                        for (int i = 0; i < dependencies.Length; ++i)
                        {
                            TID dependencyID = dependencies[i];
                            ProjectItem dependencyItem;
                            if (m_state.TryGetProjectItemFromCache(dependencyID, out dependencyItem))
                            {
                                depsList.Add(dependencyItem);
                            }
                            else
                            {
                                if (!unresolvedDependencies.Contains(dependencyID))
                                {
                                    unresolvedDependencies.Add(dependencyID);
                                }
                            }
                        }

                        if (depsList.Count > 0)
                        {
                            GetProjectItemsToLoad(depsList.ToArray(), loadHs, unresolvedDependencies);
                        }
                    }
                }
            }
        }

        public async Task UnloadAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem assetItem = projectItems[i];
                TID assetItemID = m_storage.GetID(assetItem);
                if (!AssetDB.IsDynamicResourceID(assetItemID))
                {
                    Debug.LogWarning("Unable to unload " + assetItem.ToString() + ". It is possible to unload dynamic resources only"); ;
                    continue;
                }

                TID[] embeddedIDs = m_storage.GetEmbeddedIDs(assetItem);
                if (embeddedIDs != null)
                {
                    for (int j = 0; j < embeddedIDs.Length; ++j)
                    {
                        TID embeddedID = embeddedIDs[j];
                        if (AssetDB.IsDynamicResourceID(embeddedID))
                        {
                            UnityObject obj = AssetDB.FromID<UnityObject>(embeddedID);
                            AssetDB.UnregisterDynamicResource(embeddedID);

                            if (obj != null && !(obj is GameObject) && !(obj is Component))
                            {
                                UnityObject.Destroy(obj);
                            }
                        }
                    }
                }

                if (AssetDB.IsDynamicResourceID(assetItemID))
                {
                    UnityObject obj = AssetDB.FromID<UnityObject>(assetItemID);
                    AssetDB.UnregisterDynamicResource(assetItemID);

                    if (obj != null)
                    {
                        UnityObject.Destroy(obj);
                    }
                }
            }

            UnloadCompleted?.Invoke(this, new ProjectEventArgs<ProjectItem[]>(projectItems));
        }

        public Task UnloadAllAsync(CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            UnloadUnregisterDestroy();

            var tcs = new TaskCompletionSource<object>();
            AssetDB.UnloadUnusedAssets(op =>
            {
                using (ct.Register(() => tcs.TrySetCanceled(ct)))
                {
                    if (!ct.IsCancellationRequested)
                    {
                        tcs.TrySetResult(null);
                        UnloadAllCompleted?.Invoke(this, ProjectEventArgs.Empty);
                    }
                }
            });

            return tcs.Task;
        }

        public async Task<ProjectItem[]> DuplicateAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            List<ProjectItem> extendedProjectItems = new List<ProjectItem>();
            ProjectItem[] folders = projectItems.Where(item => item.IsFolder).ToArray();
            Dictionary<ProjectItem, List<string>> toSubfolderNames = new Dictionary<ProjectItem, List<string>>();
            foreach (ProjectItem folder in folders)
            {
                if (folder.Parent == null)
                {
                    Debug.LogWarning($"Unable to duplicate folder '{folder.Name}'. Parent folder is null.");
                    continue;
                }

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

            ProjectItem[] folderDuplicates = await DuplicateFoldersAsync(folders, toSubfolderNames, ct);
            Dictionary<ProjectItem, ProjectItem> folderToDuplicate = new Dictionary<ProjectItem, ProjectItem>();
            for (int i = 0; i < folders.Length; ++i)
            {
                folderToDuplicate.Add(folders[i], folderDuplicates[i]);
            }

            ProjectItem[] scenes = extendedProjectItems.Where(item => IsScene(item)).ToArray();
            int[] sceneIndices = new int[scenes.Length];
            for (int i = 0; i < scenes.Length; ++i)
            {
                sceneIndices[i] = extendedProjectItems.IndexOf(scenes[i]);
            }

            ProjectItem[] sceneDuplicates = await DuplicateScenesAsync(scenes, folderToDuplicate, ct);
            ProjectItem[] nonScenes = extendedProjectItems.Where(item => !item.IsFolder && !IsScene(item)).ToArray();
            Preview<TID>[] previews = await m_storage.GetPreviewsAsync(m_state.ProjectPath, nonScenes.Select(item => item.ToString()).ToArray(), ct);
            ct.ThrowIfCancellationRequested();

            int[] nonSceneIndices = new int[nonScenes.Length];
            for (int i = 0; i < nonScenes.Length; ++i)
            {
                nonScenes[i].SetPreview(previews[i].PreviewData);
                nonSceneIndices[i] = extendedProjectItems.IndexOf(nonScenes[i]);
            }

            UnityObject[] loadedObjects = await _LoadAsync(nonScenes, ct);
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

                loadedObjects[i] = UnityObject.Instantiate(loadedObjects[i]);
                loadedObjects[i].name = name;

                if (go != null)
                {
                    go.SetActive(wasActive);
                }
            }

            ProjectItem[] nonSceneParents = nonScenes.Select(ai => (folderToDuplicate.ContainsKey(ai.Parent) ? folderToDuplicate[ai.Parent] : ai.Parent)).ToArray();
            ProjectItem[] nonSceneDuplicates = await SaveAsync(null, nonSceneParents, nonScenes.Select(item => item.GetPreview()).ToArray(), loadedObjects, null, false, ct);

            for (int i = 0; i < loadedObjects.Length; ++i)
            {
                UnityObject.Destroy(loadedObjects[i]);
            }

            ProjectItem[] result = new ProjectItem[folders.Length + sceneDuplicates.Length + nonSceneDuplicates.Length];
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

            DuplicateCompleted?.Invoke(this, new ProjectEventArgs<(ProjectItem[], ProjectItem[])>((projectItems, result)));
            return result;
        }

        private async Task<ProjectItem[]> DuplicateFoldersAsync(ProjectItem[] folders, Dictionary<ProjectItem, List<string>> toSubfolderName, CancellationToken ct)
        {
            if (folders.Length == 0)
            {
                return new ProjectItem[0];
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

            await m_storage.CreateFoldersAsync(m_state.ProjectPath, paths, names, ct);
            ct.ThrowIfCancellationRequested();

            for (int i = 0; i < folders.Length; ++i)
            {
                ProjectItem duplicatedFolder = result[i];
                ProjectItem parentFolder = duplicatedFolder.Parent;

                parentFolder.Children.Add(duplicatedFolder);
                parentFolder.Children.Sort((item0, item1) => item0.Name.ToUpper().CompareTo(item1.Name.ToUpper()));
            }

            return result;
        }

        private ProjectItem DuplicateScene(ProjectItem assetItem, TID copyID)
        {
            ProjectItem copy = m_storage.CreateAssetItem(copyID, assetItem.GetTypeGuid(), assetItem.Name, assetItem.Ext, null);

            TID[] dependencyIDs = m_storage.GetDependencyIDs(assetItem);
            if (dependencyIDs != null)
            {
                m_storage.SetDependencyIDs(copy, dependencyIDs.ToArray());
            }

            byte[] preview = assetItem.GetPreview();
            copy.SetPreview(preview);

            if (!m_state.ContainsProjectItemWithKey(copyID))
            {
                m_state.AddProjectItemToCache(copyID, copy);
            }

            return copy;
        }

        private async Task<ProjectItem[]> DuplicateScenesAsync(ProjectItem[] scenes, Dictionary<ProjectItem, ProjectItem> folderToDuplicate, CancellationToken ct)
        {
            if (scenes.Length == 0)
            {
                return new ProjectItem[0];
            }

            TID[] ids = await GenerateIdentifiersAsync(scenes.Length, ct);
            ct.ThrowIfCancellationRequested();

            ProjectItem[] duplicates = new ProjectItem[scenes.Length];
            List<string>[] names = scenes.Select(s => s.Parent.Children.Where(c => IsScene(c)).Select(c => c.Name).ToList()).ToArray();
            for (int i = 0; i < duplicates.Length; ++i)
            {
                ProjectItem copy = DuplicateScene(scenes[i], ids[i]);
                if (copy == null)
                {
                    return new ProjectItem[0];
                }

                if (!folderToDuplicate.ContainsKey(scenes[i].Parent))
                {
                    copy.Name = PathHelper.GetUniqueName(copy.Name, names[i]);
                    names[i].Add(copy.Name);
                }

                duplicates[i] = copy;
            }

            object[] persistentObjects = await m_storage.LoadAsync(m_state.ProjectPath, scenes, scenes.Select(s => typeof(PersistentRuntimeScene<TID>)).ToArray(), ct);
            ct.ThrowIfCancellationRequested();

            await m_storage.SaveAsync(m_state.ProjectPath, scenes.Select(ai => (folderToDuplicate.ContainsKey(ai.Parent) ? folderToDuplicate[ai.Parent] : ai.Parent).ToString()).ToArray(), duplicates, persistentObjects, m_state.ProjectInfo, false, ct);
            ct.ThrowIfCancellationRequested();

            for (int i = 0; i < scenes.Length; ++i)
            {
                (folderToDuplicate.ContainsKey(scenes[i].Parent) ? folderToDuplicate[scenes[i].Parent] : scenes[i].Parent).AddChild(duplicates[i]);
            }

            return duplicates;
        }

        public async Task<ProjectItem[]> CreatePrefabsAsync(ProjectItem[] parentFolders, GameObject[] prefabs, bool includeDeps, Func<UnityObject, byte[]> createPreview, CancellationToken ct)
        {
            ProjectItem[] result;
            if (includeDeps)
            {
                List<ProjectItem> folders = new List<ProjectItem>();
                List<object> objects = new List<object>();
                List<byte[]> previewData = new List<byte[]>();

                for (int i = 0; i < prefabs.Length; ++i)
                {
                    ProjectItem parentFolder = parentFolders[i];
                    GameObject prefab = prefabs[i];

                    object[] deps = await GetDependenciesAsync(prefab, true, ct);

                    for (int j = 0; j < deps.Length; ++j)
                    {
                        object dep = deps[j];
                        if (m_factory.CanCreateInstance(dep.GetType()) && !objects.Contains(dep))
                        {
                            objects.Add(dep);
                            previewData.Add(createPreview != null && dep is UnityObject ? createPreview((UnityObject)dep) : null);
                            folders.Add(parentFolder);
                        }
                    }

                    if (!objects.Contains(prefab))
                    {
                        objects.Add(prefab);
                        previewData.Add(createPreview != null ? createPreview(prefab) : null);
                        folders.Add(parentFolder);
                    }
                }

                result = await SaveAsync(folders.ToArray(), previewData.ToArray(), objects.ToArray(), null, true, ct);
            }
            else
            {
                byte[][] previewData = new byte[prefabs.Length][];
                if (createPreview != null)
                {
                    for (int i = 0; i < previewData.Length; ++i)
                    {
                        previewData[i] = createPreview(prefabs[i]);
                    }
                }
                result = await SaveAsync(parentFolders.ToArray(), previewData, prefabs.ToArray(), null, true, ct);
            }

            CreatePrefabsCompleted?.Invoke(this, new ProjectEventArgs<ProjectItem[]>(result));
            return result;
        }

        public async Task<ProjectItem[]> CreateFoldersAsync(ProjectItem[] parentFolders, string[] folderNames, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            if (parentFolders.Any(prentFolder => !prentFolder.IsFolder))
            {
                throw new InvalidOperationException("is not a folder");
            }

            string[] paths = parentFolders.Select(prentFolder => prentFolder.ToString()).ToArray();

            ProjectItem[] folders = new ProjectItem[folderNames.Length];
            for (int i = 0; i < folders.Length; ++i)
            {
                ProjectItem folder = new ProjectItem
                {
                    Name = folderNames[i]
                };

                parentFolders[i].AddChild(folder);
                folders[i] = folder;
            }

            await m_storage.CreateFoldersAsync(m_state.ProjectPath, paths, folderNames, ct);
            ct.ThrowIfCancellationRequested();

            CreateFoldersCompleted?.Invoke(this, new ProjectEventArgs<ProjectItem[]>(folders));
            return folders;
        }

        public async Task RenameAsync(ProjectItem[] projectItems, string[] newNames, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            string[] paths = projectItems.Select(p => p.Parent.ToString()).ToArray();
            string[] oldNames = projectItems.Select(p => p.NameExt).ToArray();
            string[] names = newNames.Zip(projectItems, (newName, projectItem) => newName + projectItem.Ext).ToArray();

            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
                projectItem.Name = newNames[i];

                if (!projectItem.IsFolder)
                {
                    UnityObject obj = AssetDB.FromID<UnityObject>(m_storage.GetID(projectItem));
                    if (obj != null)
                    {
                        obj.name = projectItem.Name;
                    }
                }
            }

            await m_storage.RenameAsync(m_state.ProjectPath, paths, oldNames, names, ct);
            ct.ThrowIfCancellationRequested();

            RenameCompleted?.Invoke(this, new ProjectEventArgs<ProjectItem[]>(projectItems));
        }

        public async Task MoveAsync(ProjectItem[] projectItems, ProjectItem target, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            ProjectItem[] parents = projectItems.Select(item => item.Parent).ToArray();

            string[] paths = parents.Select(p => p.ToString()).ToArray();
            string[] names = projectItems.Select(p => p.NameExt).ToArray();
            string targetPath = target.ToString();

            ProjectItem targetFolder = m_state.RootFolder.Get(target.ToString());
            foreach (ProjectItem item in projectItems)
            {
                targetFolder.AddChild(item);
            }

            await m_storage.MoveAsync(m_state.ProjectPath, paths, names, targetPath, ct);
            ct.ThrowIfCancellationRequested();

            MoveCompleted?.Invoke(this, new ProjectEventArgs<(ProjectItem[], ProjectItem[])>((parents, projectItems)));
        }

        public async Task DeleteAsync(ProjectItem[] projectItems, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            if(projectItems.Any(p => p.Parent == null))
            {
                throw new ArgumentException("Unable to remove root folder", "projectItems");
            }

            string[] paths = projectItems.Select(p => p.ToString()).ToArray();

            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
                if (projectItem.IsFolder)
                {
                    RemoveFolder(projectItem);
                }
                else
                {
                    Remove(projectItem);
                }
            }

            await m_storage.DeleteAsync(m_state.ProjectPath, paths, ct);
            ct.ThrowIfCancellationRequested();

            DeleteCompleted?.Invoke(this, new ProjectEventArgs<ProjectItem[]>(projectItems));
        }

        protected void Remove(ProjectItem projectItem)
        {
            if (projectItem.Parent != null)
            {
                projectItem.Parent.RemoveChild(projectItem);
            }
            TID id = m_storage.GetID(projectItem);
            m_state.RemoveProjectItemFromCache(id);

            TID[] embeddedIDs = m_storage.GetEmbeddedIDs(projectItem);
            if (embeddedIDs != null)
            {
                for (int i = 0; i < embeddedIDs.Length; ++i)
                {
                    TID embeddedID = embeddedIDs[i];
                    AssetDB.UnregisterDynamicResource(embeddedID);

                    ProjectItem embeddedItem;
                    if (m_state.TryGetProjectItemFromCache(embeddedID, out embeddedItem))
                    {
                        Debug.Assert(projectItem == embeddedItem);
                        m_state.RemoveProjectItemFromCache(embeddedID);
                    }
                }
            }

            if (AssetDB.IsDynamicResourceID(id))
            {
                UnityObject obj = AssetDB.FromID<UnityObject>(id);
                AssetDB.UnregisterDynamicResource(id);
                if (obj != null)
                {
                    UnityObject.Destroy(obj);
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
                    if (child.IsFolder)
                    {
                        RemoveFolder(child);
                    }
                    else
                    {
                        Remove(child);
                    }
                }
            }

            if (projectItem.Parent != null)
            {
                projectItem.Parent.RemoveChild(projectItem);
            }
        }

        public async Task<T[]> GetValuesAsync<T>(string searchPattern, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            Type objType = typeof(T);
            Type persistentType = TypeMap.ToPersistentType(objType);
            Type type = persistentType != null ? persistentType : objType;

            object[] values = await m_storage.GetValuesAsync(m_state.ProjectPath, searchPattern, type, ct);
            ct.ThrowIfCancellationRequested();

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
            return result;
        }

        public async Task<T> GetValueAsync<T>(string key, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            Type objType = typeof(T);
            Type persistentType = TypeMap.ToPersistentType(objType);
            if (persistentType == null)
            {
                persistentType = objType;
            }

            object value = await m_storage.GetValueAsync(m_state.ProjectPath, key + GetExt(objType), persistentType, ct);
            ct.ThrowIfCancellationRequested();

            object result;
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

            return (T)result;
        }

        public async Task SetValueAsync<T>(string key, T obj, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            object value = obj;

            Type objType = obj.GetType();
            Type persistentType = TypeMap.ToPersistentType(objType);
            if (persistentType != null)
            {
                IPersistentSurrogate persistentObj = (IPersistentSurrogate)Activator.CreateInstance(persistentType);
                persistentObj.ReadFrom(obj);
                value = persistentObj;
            }

            await m_storage.SetValueAsync(m_state.ProjectPath, key + GetExt(objType), value, ct);
            ct.ThrowIfCancellationRequested();
        }

        public async Task DeleteValueAsync<T>(string key, CancellationToken ct)
        {
            ct = CreateLinkedToken(ct);

            Type objType = typeof(T);

            await m_storage.DeleteValueAsync(m_state.ProjectPath, key + GetExt(objType), ct);
            ct.ThrowIfCancellationRequested();
        }


        protected virtual void UnloadUnregister()
        {
            IAssetDB<TID> assetDB = AssetDB;
            if (assetDB != null)
            {
                assetDB.UnregisterSceneObjects();
                assetDB.UnregisterDynamicResources();
                assetDB.UnregisterStaticResources();
            }
            m_state.ClearProjectItemsCache();
        }

        private void UnloadUnregisterDestroy()
        {
            IAssetDB<TID> assetDB = AssetDB;
            UnityObject[] dynamicResources = null;
            if (assetDB != null)
            {
                dynamicResources = assetDB.GetDynamicResources();
            }

            AssetBundle[] loadedAssetBundles = LoadedAssetBundles;

            UnloadUnregister();

            if (dynamicResources != null)
            {
                foreach (UnityObject dynamicResource in dynamicResources)
                {
                    if (dynamicResource is Transform)
                    {
                        continue;
                    }
                    UnityObject.Destroy(dynamicResource);
                }
            }


            foreach (AssetBundle assetBundle in loadedAssetBundles)
            {
                assetBundle.Unload(true);
            }
        }
    }
}


