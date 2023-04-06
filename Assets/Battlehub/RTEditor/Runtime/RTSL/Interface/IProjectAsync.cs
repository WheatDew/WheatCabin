using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL.Interface
{
    public enum OpenProjectFlags
    {
        None = 0,
        ClearScene = 1,
        CreateNewScene = 3,
        DestroyObjects = 4,
        Default = CreateNewScene | DestroyObjects
    }

    public class ProjectEventArgs : EventArgs
    {
        public static readonly new ProjectEventArgs Empty = new ProjectEventArgs();

        public bool HasError
        {
            get;
            private set;
        }

        public ProjectEventArgs()
        {
            HasError = false;
        }

        public ProjectEventArgs(bool hasError)
        {
            HasError = hasError;
        }
    }

    public class ProjectEventArgs<T> : ProjectEventArgs
    {
        public T Payload
        {
            get;
            private set;
        }

        public ProjectEventArgs(T payload)
           : base()
        {
            Payload = payload;
        }

        public ProjectEventArgs(T payload, bool hasError)
            : base(hasError)
        {
            Payload = payload;
        }
    }

    public interface IProjectEvents
    {
        IProjectAsync Project
        {
            get;
        }

        event EventHandler Locked;
        event EventHandler Released;

        event EventHandler<ProjectEventArgs<ProjectInfo[]>> GetProjectsCompleted;
        event EventHandler<ProjectEventArgs<ProjectInfo>> CreateProjectCompleted;
        event EventHandler<ProjectEventArgs<(string Project, string TargetProject)>> CopyProjectCompleted;
        event EventHandler<ProjectEventArgs<string>> DeleteProjectCompleted;
        event EventHandler<ProjectEventArgs<(string Project, string TargetPath)>> ExportProjectCompleted;
        event EventHandler<ProjectEventArgs<(string Project, string SourcePath)>> ImportProjectCompleted;
        event EventHandler<ProjectEventArgs<ProjectInfo>> OpenProjectCompleted;
        event EventHandler<ProjectEventArgs<string>> CloseProjectCompleted;

        event EventHandler<ProjectEventArgs<object[]>> BeginSave;
        event EventHandler<ProjectEventArgs<(ProjectItem[] SavedItems, bool IsUserAction)>> SaveCompleted;
        event EventHandler<ProjectEventArgs<ProjectItem[]>> BeginLoad;
        event EventHandler<ProjectEventArgs<(ProjectItem[] LoadedItems, UnityObject[] LoadedObjects)>> LoadCompleted;
        event EventHandler<ProjectEventArgs<ProjectItem[]>> UnloadCompleted;
        event EventHandler<ProjectEventArgs> UnloadAllCompleted;
        event EventHandler<ProjectEventArgs<(ProjectItem[] OriginalItems, ProjectItem[] DuplicatedItems)>> DuplicateCompleted;

        event EventHandler<ProjectEventArgs<ProjectItem[]>> CreatePrefabsCompleted;
        event EventHandler<ProjectEventArgs<ProjectItem[]>> CreateFoldersCompleted;
        event EventHandler<ProjectEventArgs<(ProjectItem[] OriginalParentItems, ProjectItem[] MovedItems)>> MoveCompleted;
        event EventHandler<ProjectEventArgs<ProjectItem[]>> RenameCompleted;
        event EventHandler<ProjectEventArgs<ProjectItem[]>> DeleteCompleted;

        event EventHandler<ProjectEventArgs<ProjectItem[]>> ImportCompleted;
    }

    public interface IProjectState
    {
        IProjectAsync Project
        {
            get;
        }

        bool IsOpened
        {
            get;
        }

        ProjectInfo ProjectInfo
        {
            get;
        }

        ProjectItem RootFolder
        {
            get;
        }

        ProjectItem LoadedScene
        {
            get;
            set;
        }
    }

    public interface IProjectUtils
    {
        IProjectAsync Project
        {
            get;
        }

        bool IsStatic(ProjectItem projectItem);
        bool IsScene(ProjectItem projectItem);
        bool IsUnityObject(ProjectItem projectItem);
        Type ToType(ProjectItem assetItem);
        Guid ToGuid(Type type);

        object ToPersistentID(UnityObject obj);
        object ToPersistentID(ProjectItem projectItem);
        void SetPersistentID(ProjectItem projectItem, object id);

        ProjectItem CreateAssetItem(Guid typeGuid, string name, string ext = null, ProjectItem parent = null);
        ProjectItem ToProjectItem(UnityObject obj);
        T FromProjectItem<T>(ProjectItem projectItem) where T : UnityObject;
        T FromPersistentID<T>(object id) where T : UnityObject;
        ProjectItem[] GetProjectItemsDependentOn(ProjectItem[] projectItems);

        [Obsolete("Use FindDeepDependenciesAsync instead")]
        object[] FindDeepDependencies(object obj);
        Task<object[]> FindDeepDependenciesAsync(object obj);

        string GetExt(object obj);
        string GetExt(Type type);

        string GetUniqueName(string name, string[] names);
        string GetUniqueName(string name, Type type, ProjectItem folder, bool noSpace = false);
        string GetUniqueName(string name, string ext, ProjectItem folder, bool noSpace = false);
        string GetUniquePath(string path, Type type, ProjectItem folder, bool noSpace = false);
    }

    public interface IProjectAsyncSafe
    {
        IProjectEvents Events
        {
            get;
        }

        Task<ProjectInfo[]> GetProjectsAsync(CancellationToken ct = default);
        Task<ProjectInfo> CreateProjectAsync(string project, CancellationToken ct = default);
        Task CopyProjectAsync(string project, string targetProject, CancellationToken ct = default);
        Task DeleteProjectAsync(string project, CancellationToken ct = default);
        Task ExportProjectAsync(string project, string targetPath, CancellationToken ct = default);
        Task ImportProjectAsync(string project, string sourcePath, bool overwrite = false, CancellationToken ct = default);
        Task<ProjectInfo> OpenProjectAsync(string project, OpenProjectFlags flags = OpenProjectFlags.Default, CancellationToken ct = default);
        Task CloseProjectAsync(CancellationToken ct = default);

        Task<byte[][]> GetPreviewsAsync(ProjectItem[] projectItems, CancellationToken ct = default);
        Task<ProjectItem[]> GetAssetItemsAsync(ProjectItem[] folders, string searchPattern = null, CancellationToken ct = default);
        Task<object[]> GetDependenciesAsync(object obj, bool exceptMappedObjects = false, CancellationToken ct = default);
        Task SaveAsync(ProjectItem[] projectItems, object[] obj, bool isUserAction, CancellationToken ct = default);
        Task SaveAsync(ProjectItem[] projectItems, object[] obj, CancellationToken ct = default);
        Task<ProjectItem[]> SaveAsync(ProjectItem[] folders, byte[][] previewData, object[] obj, string[] nameOverrides = null, bool isUserAction = true, CancellationToken ct = default);
        Task SavePreviewsAsync(ProjectItem[] projectItems, CancellationToken ct = default);

        Task<UnityObject[]> LoadAsync(ProjectItem[] projectItems, CancellationToken ct = default);
        Task UnloadAsync(ProjectItem[] projectItems, CancellationToken ct = default);
        Task UnloadAllAsync(CancellationToken ct = default);

        Task<ProjectItem[]> DuplicateAsync(ProjectItem[] projectItems, CancellationToken ct = default);

        Task<ProjectItem> LoadImportItemsAsync(string path, bool isBuiltIn, CancellationToken ct = default);
        void UnloadImportItems(ProjectItem importItemsRoot);
        Task<ProjectItem[]> ImportAsync(ProjectItem[] importItems, CancellationToken ct = default);

        Task<ProjectItem[]> CreatePrefabsAsync(ProjectItem[] parentFolders, GameObject[] prefabs, bool includeDeps, Func<UnityObject, byte[]> createPreview = null, CancellationToken ct = default);
        Task<ProjectItem[]> CreateFoldersAsync(ProjectItem[] parentFolders, string[] names, CancellationToken ct = default);
        Task RenameAsync(ProjectItem[] projectItem, string[] newNames, CancellationToken ct = default);
        Task MoveAsync(ProjectItem[] projectItems, ProjectItem targetFolder, CancellationToken ct = default);
        Task DeleteAsync(ProjectItem[] projectItems, CancellationToken ct = default);

        Task<string[]> GetAssetBundlesAsync(CancellationToken ct = default);
        Task<string[]> GetStaticAssetLibrariesAsync(CancellationToken ct = default);

        Task<T[]> GetValuesAsync<T>(string searchPattern, CancellationToken ct = default);
        Task<T> GetValueAsync<T>(string key, CancellationToken ct = default);
        Task SetValueAsync<T>(string key, T obj, CancellationToken ct = default);
        Task DeleteValueAsync<T>(string key, CancellationToken ct = default);
    }

    public interface IProjectAsyncWithAssetLibraries : IProjectAsync
    {
        string BuiltInLibraryName
        {
            get;
            set;
        }

        string SceneDepsAssetLibraryName
        {
            get;
            set;
        }
    }

    public interface IProjectAsync : IProjectAsyncSafe
    {

        IProjectState State
        {
            get;
        }

        IProjectUtils Utils
        {
            get;
        }

        IProjectAsyncSafe Safe
        {
            get;
        }

        Task<Lock.LockReleaser> LockAsync(CancellationToken ct = default);
    }

    public static class IProjectAsyncExtensions
    {
        public static string GetUniqueName(this IProjectUtils projectUtils, string path, Type type)
        {
            ProjectItem folder = projectUtils.GetFolder(Path.GetDirectoryName(path));
            return Path.GetFileName(projectUtils.GetUniquePath(path, type, folder));
        }

        public static string GetUniquePath(this IProjectUtils projectUtils, string path, Type type)
        {
            ProjectItem folder = projectUtils.GetFolder(Path.GetDirectoryName(path));
            return projectUtils.GetUniquePath(path, type, folder);
        }

        public static string[] Find<T>(this IProjectUtils projectUtils, string filter = null, bool allowSubclasses = false)
        {
            Type typeofT = typeof(T);
            return Find(projectUtils, filter, allowSubclasses, typeofT);
        }

        public static string[] Find(this IProjectUtils projectUtils, string filter, bool allowSubclasses, Type typeofT)
        {
            return projectUtils.FindAssetItems(filter, allowSubclasses, typeofT).Select(item => item.RelativePath(allowSubclasses)).ToArray();
        }

        public static ProjectItem[] FindAssetItems(this IProjectUtils projectUtils, string filter, bool allowSubclasses, Type typeofT)
        {
            IProjectState state = projectUtils.Project.State;
            List<ProjectItem> result = new List<ProjectItem>();
            ProjectItem[] projectItems = state.RootFolder.Flatten(true);
            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
                Type type = projectUtils.ToType(projectItem);
                if (type == null)
                {
                    continue;
                }

                if (type != typeofT)
                {
                    if (!allowSubclasses || !type.IsSubclassOf(typeofT))
                    {
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(filter) && !projectItem.Name.Contains(filter))
                {
                    continue;
                }

                result.Add(projectItem);
            }
            return result.ToArray();
        }

        public static string[] FindFolders(this IProjectUtils projectUtils, string filter = null)
        {
            IProjectState state = projectUtils.Project.State;
            List<string> result = new List<string>();
            ProjectItem[] projectItems = state.RootFolder.Flatten(false, true);
            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
                Debug.Assert(projectItem.IsFolder);

                if (!string.IsNullOrEmpty(filter) && !projectItem.Name.Contains(filter))
                {
                    continue;
                }

                result.Add(projectItem.RelativePath(false));
            }
            return result.ToArray();
        }

        public static ProjectItem Get<T>(this IProjectUtils projectUtils, string path)
        {
            Type type = typeof(T);
            return Get(projectUtils, path, type);
        }

        public static ProjectItem Get(this IProjectUtils projectUtils, string path, Type type)
        {
            IProjectState state = projectUtils.Project.State;
            if (!state.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            return state.RootFolder.Get(string.Format("{0}{1}", path, projectUtils.GetExt(type)));
        }

        public static ProjectItem GetFolder(this IProjectUtils projectUtils, string path = null, bool forceCreate = false)
        {
            IProjectState state = projectUtils.Project.State;

            if (!state.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            if (string.IsNullOrEmpty(path))
            {
                return state.RootFolder;
            }

            if (forceCreate)
            {
                return state.RootFolder.GetOrCreateFolder(path);
            }

            return state.RootFolder.Get(path);
        }
        public static bool FolderExist(this IProjectUtils projectUtils, string path)
        {
            ProjectItem projectItem = projectUtils.GetFolder(path);
            return projectItem != null && projectItem.ToString().ToLower() == ("/" + projectUtils.Project.State.RootFolder.Name + "/" + path).ToLower();
        }

        public static bool Exist<T>(this IProjectUtils projectUtils, string path)
        {
            ProjectItem projectItem = projectUtils.Get<T>(path);
            return projectItem != null && projectItem.ToString().ToLower() == ("/" + projectUtils.Project.State.RootFolder.Name + "/" + path + projectItem.Ext).ToLower();
        }

        public static Task<ProjectItem[]> CreateFolderAsync(this IProjectAsync project, string path)
        {
            IProjectState state = project.State;

            ProjectItem folder = state.RootFolder.GetOrCreateFolder(path);
            ProjectItem parentFolder = folder.Parent;
            parentFolder.RemoveChild(folder);

            return project.CreateFoldersAsync(new[] { parentFolder }, new[] { folder.Name });
        }

        public static Task<ProjectItem[]> CreateFoldersAsync(this IProjectAsync project, string[] path)
        {
            ProjectItem[] folders = path.Select(p => project.State.RootFolder.GetOrCreateFolder(p)).ToArray();
            ProjectItem[] parentFolders = folders.Select(p => p.Parent).ToArray();
            for (int i = 0; i < parentFolders.Length; ++i)
            {
                parentFolders[i].RemoveChild(folders[i]);
            }
            return project.CreateFoldersAsync(parentFolders, folders.Select(f => f.Name).ToArray());
        }

        public static Task RenameFolderAsync(this IProjectAsync project, string path, string newName)
        {
            if (!project.State.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            ProjectItem projectItem = project.State.RootFolder.Get(path);
            if (projectItem == null)
            {
                throw new ArgumentException("not found", "path");
            }

            return project.RenameAsync(new[] { projectItem }, new[] { newName });
        }
        
        [Obsolete("Use DeleteFolderAsync")]
        public static Task DeleteFolder(this IProjectAsync project, string path)
        {
            return project.DeleteFolderAsync(path);
        }

        public static async Task DeleteFolderAsync(this IProjectAsync project, string path)
        {
            if (!project.State.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            ProjectItem projectItem = project.State.RootFolder.Get(path);
            if (projectItem == null)
            {
                return;
            }

            await project.DeleteAsync(new[] { projectItem });
        }


        public static Task SaveAsync(this IProjectAsync project, string path, object obj)
        {
            return project.SaveAsync(path, obj, null);
        }

        public static async Task<ProjectItem[]> SaveAsync(this IProjectAsync project, string path, object obj, byte[] preview)
        {
            if (!project.State.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            string name = Path.GetFileName(path);
            path = Path.GetDirectoryName(path).Replace(@"\", "/");
            path = !string.IsNullOrEmpty(path) && path != "/" ? path : "/" + project.State.RootFolder.Name;

            string ext = project.Utils.GetExt(obj.GetType());
            ProjectItem item = project.State.RootFolder.Get(path + "/" + name + ext);
            if (item != null && !item.IsFolder)
            {
                await project.SaveAsync(new[] { item }, new[] { obj });
                return new[] { item };
            }

            ProjectItem folder = project.State.RootFolder.Get(path);
            if (folder == null || !folder.IsFolder)
            {
                throw new ArgumentException("directory cannot be found", "path");
            }

            if (preview == null)
            {
                preview = new byte[0];
            }

            return await project.SaveAsync(new[] { folder }, new[] { preview }, new[] { obj }, new[] { name });
        }

        public static Task<ProjectItem[]> SaveAsync(this IProjectAsync project, string[] path, object[] objects)
        {
            return project.SaveAsync(path, objects, null);
        }

        public static async Task<ProjectItem[]> SaveAsync(this IProjectAsync project, string[] path, object[] objects, byte[][] previews)
        {
            if (!project.State.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            if (previews == null)
            {
                previews = new byte[objects.Length][];
            }

            List<ProjectItem> existingAssetItems = new List<ProjectItem>();
            List<ProjectItem> folders = new List<ProjectItem>();
            List<string> names = new List<string>();
            for (int i = 0; i < objects.Length; ++i)
            {
                string name = Path.GetFileName(path[i]);
                path[i] = Path.GetDirectoryName(path[i]).Replace(@"\", "/");
                path[i] = !string.IsNullOrEmpty(path[i]) && path[i] != "/" ? path[i] : "/" + project.State.RootFolder.Name;

                object obj = objects[i];
                string ext = project.Utils.GetExt(obj.GetType());
                ProjectItem item = project.State.RootFolder.Get(path[i] + "/" + name + ext);
                if (item != null && !item.IsFolder)
                {
                    existingAssetItems.Add(item);
                }
                else
                {
                    ProjectItem folder = project.State.RootFolder.Get(path[i]);
                    if (folder == null || !folder.IsFolder)
                    {
                        throw new ArgumentException("directory cannot be found", "path");
                    }

                    if (previews[i] == null)
                    {
                        previews[i] = new byte[0];
                    }
                    folders.Add(folder);
                    names.Add(name);
                }
            }

            if (existingAssetItems.Count > 0)
            {
                if (existingAssetItems.Count != objects.Length)
                {
                    throw new InvalidOperationException("You are trying to save mixed collection of new and existing objects. This is not supported");
                }

                await project.SaveAsync(existingAssetItems.ToArray(), objects);
                return existingAssetItems.ToArray();
            }

            return await project.SaveAsync(folders.ToArray(), previews, objects, names.ToArray());
        }

        //[Obsolete("Use Load_Async instead")]
        public static Task<UnityObject> LoadAsync<T>(this IProjectAsync project, string path)
        {
            Type type = typeof(T);
            return LoadAsync(project, path, type);
        }

        public static async Task<T> Load_Async<T>(this IProjectAsync project, string path) where T : UnityObject
        {
            Type type = typeof(T);
            return (T)await LoadAsync(project, path, type);
        }

        public static async Task<UnityObject> LoadAsync(this IProjectAsync project, string path, Type type)
        {
            if (!project.State.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            ProjectItem assetItem = project.State.RootFolder.Get(path + project.Utils.GetExt(type));
            if (assetItem == null || assetItem.IsFolder)
            {
                throw new ArgumentException("not found", "path");
            }

            UnityObject[] result = await project.LoadAsync(new[] { assetItem });
            return result[0];
        }

        public static Task RenameAsync<T>(this IProjectAsync project, string path, string newName)
        {
            Type type = typeof(T);
            return RenameAsync(project, path, newName, type);
        }

        public static async Task RenameAsync(this IProjectAsync project, string path, string newName, Type type)
        {
            if (!project.State.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            ProjectItem projectItem = project.State.RootFolder.Get(path + project.Utils.GetExt(type));
            if (projectItem == null || projectItem.IsFolder)
            {
                throw new ArgumentException("not found", "path");
            }
            await project.RenameAsync(new[] { projectItem }, new string[] { newName });
        }

        public static Task DeleteAsync<T>(this IProjectAsync project, string path)
        {
            Type type = typeof(T);
            return DeleteAsync(project, path, type);
        }

        public static async Task DeleteAsync(this IProjectAsync project, string path, Type type)
        {
            if (!project.State.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            ProjectItem projectItem = project.State.RootFolder.Get(path + project.Utils.GetExt(type));
            if (projectItem == null)
            {
                return;
            }
            if (projectItem.IsFolder)
            {
                throw new ArgumentException("not found", "path");
            }
            await project.DeleteAsync(new[] { projectItem });
        }

        public static Task UnloadAsync<T>(this IProjectAsync project, string path)
        {
            return project.UnloadAsync(path, typeof(T));
        }

        public static async Task UnloadAsync(this IProjectAsync project, string path, Type type)
        {
            ProjectItem projectItem = project.Utils.Get(path, type);
            if (projectItem == null || projectItem.IsFolder)
            {
                Debug.Log("Unable to unload. Item was not found " + path);
                return;
            }
            await project.UnloadAsync(new[] { projectItem });
        }

        public static async Task<(bool Exists, T Value)> TryGetValueAsync<T>(this IProjectAsyncSafe safe, string key, CancellationToken ct = default)
        {
            try
            {
                T value = await safe.GetValueAsync<T>(key, ct);
                return (true, value);
            }
            catch (StorageException e)
            {
                if (e.ErrorCode == Error.E_NotFound)
                {
                    return (false, default);
                }
                throw;
            }
        }

        public static async Task<(bool Exists, T Value)> TryGetValueAsync<T>(this IProjectAsync project, string key, CancellationToken ct = default)
        {
            try
            {
                T value = await project.GetValueAsync<T>(key, ct);
                return (true, value);
            }
            catch (StorageException e)
            {
                if (e.ErrorCode == Error.E_NotFound)
                {
                    return (false, default);
                }
                throw;
            }
        }
    }
}

