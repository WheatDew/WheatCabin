using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSL.Interface
{
    public delegate void ProjectEventHandler(Error error);
    public delegate void ProjectEventHandler<T>(Error error, T result);
    public delegate void ProjectEventHandler<T, T2>(Error error, T result, T2 result2);


    //[Obsolete]
    public interface IProject
    {
        event ProjectEventHandler NewSceneCreating;
        event ProjectEventHandler NewSceneCreated;
        event ProjectEventHandler<ProjectInfo> CreateProjectCompleted;
        event ProjectEventHandler<ProjectInfo> OpenProjectCompleted;
        event ProjectEventHandler<string> CopyProjectCompleted;
        event ProjectEventHandler<string> ExportProjectCompleted;
        event ProjectEventHandler<string> ImportProjectCompleted;
        event ProjectEventHandler<string> DeleteProjectCompleted;
        event ProjectEventHandler<ProjectInfo[]> ListProjectsCompleted;
        event ProjectEventHandler CloseProjectCompleted;

        event ProjectEventHandler<ProjectItem[]> GetAssetItemsCompleted;
        event ProjectEventHandler<object[]> BeginSave;
        event ProjectEventHandler<AssetItem[], bool> SaveCompleted;
        event ProjectEventHandler<AssetItem[]> BeginLoad;
        event ProjectEventHandler<AssetItem[], UnityObject[]> LoadCompleted;
        [Obsolete("Use DuplicateItemsCompleted instead")]
        event ProjectEventHandler<AssetItem[]> DuplicateCompleted;
        event ProjectEventHandler<ProjectItem[]> DuplicateItemsCompleted;
        event ProjectEventHandler UnloadCompleted;
        event ProjectEventHandler<AssetItem[]> ImportCompleted;
        event ProjectEventHandler<ProjectItem[]> BeforeDeleteCompleted;
        event ProjectEventHandler<ProjectItem[]> DeleteCompleted;
        event ProjectEventHandler<ProjectItem[], ProjectItem[]> MoveCompleted;
        event ProjectEventHandler<ProjectItem> RenameCompleted;
        event ProjectEventHandler<ProjectItem[]> CreateCompleted;

        bool IsBusy
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

        ProjectItem Root
        {
            get;
        }

        AssetItem LoadedScene
        {
            get;
            set;
        }

        AssetBundle[] LoadedAssetBundles
        {
            get;
        }

        bool IsStatic(ProjectItem projectItem);
        bool IsScene(ProjectItem projectItem);
        Type ToType(AssetItem assetItem);
        Guid ToGuid(Type type);

        object ToPersistentID(UnityObject obj);
        object ToPersistentID(ProjectItem projectItem);
        void SetPersistentID(ProjectItem projectItem, object id);
        object ToPersistentID(Preview preview);
        void SetPersistentID(Preview preview, object id);
        T FromPersistentID<T>(object id) where T : UnityObject;
        T FromPersistentID<T>(ProjectItem projectItem) where T : UnityObject;

        [Obsolete("Use ToPersistentID instead")]
        long ToID(UnityObject obj);
        [Obsolete("Use FromPersistentID instead")]
        T FromID<T>(long id) where T : UnityObject;

        AssetItem ToAssetItem(UnityObject obj);
        AssetItem[] GetDependantAssetItems(AssetItem[] assetItems);
        object[] FindDeepDependencies(object obj);

        string GetExt(object obj);
        string GetExt(Type type);
        
        string GetUniqueName(string name, string[] names);
        string GetUniqueName(string name, Type type, ProjectItem folder, bool noSpace = false);
        string GetUniqueName(string name, string ext, ProjectItem folder, bool noSpace = false);
        string GetUniquePath(string path, Type type, ProjectItem folder, bool noSpace = false);

        void ClearScene();
        void CreateNewScene();

        YieldLock Lock();
        ProjectAsyncOperation<ProjectInfo[]> GetProjects(ProjectEventHandler<ProjectInfo[]> callback = null);
        ProjectAsyncOperation<ProjectInfo> CreateProject(string project, ProjectEventHandler<ProjectInfo> callback = null);
        ProjectAsyncOperation<ProjectInfo> OpenProject(string project, ProjectEventHandler<ProjectInfo> callback = null);
        ProjectAsyncOperation<ProjectInfo> OpenProject(string project, OpenProjectFlags flags, ProjectEventHandler<ProjectInfo> callback = null);
        ProjectAsyncOperation<string> CopyProject(string project, string targetProject, ProjectEventHandler<string> callback = null);
        ProjectAsyncOperation<string> DeleteProject(string project, ProjectEventHandler<string> callback = null);
        ProjectAsyncOperation<string> ExportProject(string project, string targetPath, ProjectEventHandler<string> callback = null);
        ProjectAsyncOperation<string> ImportProject(string projectName, string sourcePath, bool overwrite = false, ProjectEventHandler<string> callback = null);
        void CloseProject();

        ProjectAsyncOperation<Preview[]> GetPreviews(AssetItem[] assetItems, ProjectEventHandler<Preview[]> callback = null);
        [Obsolete("Use GetPreviews instead")]
        ProjectAsyncOperation<AssetItem[]> GetAssetItems(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null); /*no events raised*/
        ProjectAsyncOperation<ProjectItem[]> GetAssetItems(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback = null); /*GetAssetItemsCompleted raised*/
        ProjectAsyncOperation<ProjectItem[]> GetAssetItems(ProjectItem[] folders, string searchPattern, ProjectEventHandler<ProjectItem[]> callback = null);

        ProjectAsyncOperation<object[]> GetDependencies(object obj, bool exceptMappedObject = false, ProjectEventHandler<object[]> callback = null); /*no events raised*/

        ProjectAsyncOperation<AssetItem[]> Save(AssetItem[] assetItems, object[] obj, ProjectEventHandler<AssetItem[]> callback = null);
        ProjectAsyncOperation<AssetItem[]> Save(ProjectItem[] parents, byte[][] previewData, object[] obj, string[] nameOverrides, ProjectEventHandler<AssetItem[]> callback = null);
        ProjectAsyncOperation<AssetItem[]> Save(ProjectItem[] parents, byte[][] previewData, object[] obj, string[] nameOverrides, bool isUserAction, ProjectEventHandler<AssetItem[]> callback = null);
        ProjectAsyncOperation<AssetItem[]> SavePreview(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null);

        [Obsolete("Use Duplicate(ProjectItem[], ProjectEventHandler<ProjectItem[]>) overload")]
        ProjectAsyncOperation<AssetItem[]> Duplicate(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null);
        ProjectAsyncOperation<ProjectItem[]> Duplicate(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback = null);

        ProjectAsyncOperation<UnityObject[]> Load(AssetItem[] assetItems, ProjectEventHandler<UnityObject[]> callback = null);
        ProjectAsyncOperation Unload(ProjectEventHandler completedCallback = null);
        void Unload(AssetItem[] assetItems);

        ProjectAsyncOperation<ProjectItem> LoadImportItems(string path, bool isBuiltIn, ProjectEventHandler<ProjectItem> callback = null);
        void UnloadImportItems(ProjectItem importItemsRoot);
        ProjectAsyncOperation<AssetItem[]> Import(ProjectItem[] importItems, ProjectEventHandler<AssetItem[]> callback = null);

        ProjectAsyncOperation CreatePrefab(string folderPath, GameObject prefab, bool includeDeps, Func<UnityObject, byte[]> createPreview = null);
        ProjectAsyncOperation<ProjectItem> CreateFolder(ProjectItem projectItem, ProjectEventHandler<ProjectItem> callback = null);
        ProjectAsyncOperation<ProjectItem[]> CreateFolders(ProjectItem[] projectItem, ProjectEventHandler<ProjectItem[]> callback = null);
        ProjectAsyncOperation<ProjectItem> Rename(ProjectItem projectItem, string oldName, ProjectEventHandler<ProjectItem> callback = null);
        ProjectAsyncOperation<ProjectItem[], ProjectItem[]> Move(ProjectItem[] projectItems, ProjectItem target, ProjectEventHandler<ProjectItem[], ProjectItem[]> callback = null);
        ProjectAsyncOperation<ProjectItem[]> Delete(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback = null);
        ProjectAsyncOperation Delete(string projectPath, string[] files, ProjectEventHandler callback = null);

        ProjectAsyncOperation<string[]> GetAssetBundles(ProjectEventHandler<string[]> callback = null);
        Dictionary<int, string> GetStaticAssetLibraries();

        ProjectAsyncOperation<T[]> GetValues<T>(string searchPattern, ProjectEventHandler<T[]> callback = null);
        ProjectAsyncOperation<T> GetValue<T>(string key, ProjectEventHandler<T> callback = null);
        ProjectAsyncOperation SetValue<T>(string key, T obj, ProjectEventHandler callback = null);
        ProjectAsyncOperation DeleteValue<T>(string key, ProjectEventHandler callback = null);
    }


    public class ProjectAsyncOperation : CustomYieldInstruction
    {
        public bool HasError
        {
            get { return Error.HasError; }
        }

        public Error Error
        {
            get;
            set;
        }

        public virtual bool IsCompleted
        {
            get;
            set;
        }

        public override bool keepWaiting
        {
            get { return !IsCompleted; }
        }
    }

    public class ProjectAsyncOperation<T> : ProjectAsyncOperation
    {
        public T Result
        {
            get;
            set;
        }
    }

    public class ProjectAsyncOperation<T, T2> : ProjectAsyncOperation<T>
    {
        public T2 Result2
        {
            get;
            set;
        }
    }

    [Obsolete("Use IProjectExtensionsAsync")]
    public static class IProjectExtensions
    {
        public static string GetUniqueName(this IProject project, string path, Type type)
        {
            ProjectItem folder = project.GetFolder(Path.GetDirectoryName(path));
            return Path.GetFileName(project.GetUniquePath(path, type, folder));
        }

        public static string GetUniquePath(this IProject project, string path, Type type)
        {
            ProjectItem folder = project.GetFolder(Path.GetDirectoryName(path));
            return project.GetUniquePath(path, type, folder);
        }

        public static string[] Find<T>(this IProject project, string filter = null, bool allowSubclasses = false)
        {
            Type typeofT = typeof(T);
            return Find(project, filter, allowSubclasses, typeofT);
        }

        public static string[] Find(this IProject project, string filter, bool allowSubclasses, Type typeofT)
        {
            return project.FindAssetItems(filter, allowSubclasses, typeofT).Select(item => item.RelativePath(allowSubclasses)).ToArray();
        }

        public static AssetItem[] FindAssetItems(this IProject project, string filter, bool allowSubclasses, Type typeofT)
        {
            List<AssetItem> result = new List<AssetItem>();
            ProjectItem[] projectItems = project.Root.Flatten(true);
            for (int i = 0; i < projectItems.Length; ++i)
            {
                AssetItem assetItem = (AssetItem)projectItems[i];
                Type type = project.ToType(assetItem);
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

                if (!string.IsNullOrEmpty(filter) && !assetItem.Name.Contains(filter))
                {
                    continue;
                }

                result.Add(assetItem);
            }
            return result.ToArray();
        }

        public static string[] FindFolders(this IProject project, string filter = null)
        {
            List<string> result = new List<string>();
            ProjectItem[] projectItems = project.Root.Flatten(false, true);
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

        public static ProjectItem Get<T>(this IProject project, string path)
        {
            Type type = typeof(T);
            return Get(project, path, type);
        }
        
        public static ProjectItem Get(this IProject project, string path, Type type)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            return project.Root.Get(string.Format("{0}{1}", path, project.GetExt(type)));
        }

        public static ProjectItem GetFolder(this IProject project, string path = null, bool forceCreate = false)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            if(string.IsNullOrEmpty(path))
            {
                return project.Root;
            }

            if(forceCreate)
            {
                return project.Root.GetOrCreateFolder(path);
            }

            return project.Root.Get(path);
        }

        public static bool FolderExist(this IProject project, string path)
        {
            ProjectItem projectItem = project.GetFolder(path);
            return projectItem != null && projectItem.ToString().ToLower() == ("/" + project.Root.Name + "/" + path).ToLower();
        }

        public static bool Exist<T>(this IProject project, string path)
        {
            ProjectItem projectItem = project.Get<T>(path);
            return projectItem != null && projectItem.ToString().ToLower() == ("/" + project.Root.Name + "/" + path + projectItem.Ext).ToLower();
        }

        public static ProjectAsyncOperation CreateFolder(this IProject project, string path)
        {
            ProjectItem folder = project.Root.GetOrCreateFolder(path);
            return project.CreateFolder(folder);
        }

        public static ProjectAsyncOperation CreateFolders(this IProject project, string[] path)
        {
            ProjectItem[] folders = path.Select(p => project.Root.GetOrCreateFolder(p)).ToArray();
            return project.CreateFolders(folders);
        }

        public static ProjectAsyncOperation RenameFolder(this IProject project, string path, string newName)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            ProjectItem projectItem = project.Root.Get(path);
            if (projectItem == null)
            {
                throw new ArgumentException("not found", "path");
            }

            string oldName = projectItem.Name;
            projectItem.Name = newName;
            return project.Rename(projectItem, oldName);
        }

        public static ProjectAsyncOperation DeleteFolder(this IProject project, string path)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            ProjectItem projectItem = project.Root.Get(path);
            if (projectItem == null)
            {
                throw new ArgumentException("not found", "path");
            }

            return project.Delete(new[] { projectItem });
        }

        public static ProjectAsyncOperation<AssetItem[]> Save(this IProject project, string path, object obj, ProjectEventHandler<AssetItem[]> callback = null)
        {
            return project.Save(path, obj, null, callback);
        }

        public static ProjectAsyncOperation<AssetItem[]> Save(this IProject project, string path, object obj, byte[] preview, ProjectEventHandler<AssetItem[]> callback = null)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            string name = Path.GetFileName(path);
            path = Path.GetDirectoryName(path).Replace(@"\", "/");
            path = !string.IsNullOrEmpty(path) ? path : "/" + project.Root;

            string ext = project.GetExt(obj.GetType());
            ProjectItem item = project.Root.Get(path + "/" + name + ext);
            if (item is AssetItem)
            {
                AssetItem assetItem = (AssetItem)item;
                return project.Save(new[] { assetItem }, new[] { obj }, callback);
            }

            ProjectItem folder = project.Root.GetOrCreateFolder(path);
            if (folder == null || !folder.IsFolder)
            {
                throw new ArgumentException("directory cannot be found", "path");
            }

            if(preview == null)
            {
                preview = new byte[0];
            }

            return project.Save(new[] { folder }, new[] { preview }, new[] { obj }, new[] { name }, callback);
        }

        public static ProjectAsyncOperation<AssetItem[]> Save(this IProject project, string[] path, object[] objects,  ProjectEventHandler<AssetItem[]> callback = null)
        {
            return project.Save(path, objects, null, callback);
        }

        public static ProjectAsyncOperation<AssetItem[]> Save(this IProject project, string[] path, object[] objects, byte[][] previews, ProjectEventHandler<AssetItem[]> callback = null)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            if (previews == null)
            {
                previews = new byte[objects.Length][];
            }

            List<AssetItem> existingAssetItems = new List<AssetItem>();
            List<ProjectItem> folders = new List<ProjectItem>();
            List<string> names = new List<string>();
            for (int i = 0; i < objects.Length; ++i)
            {
                string name = Path.GetFileName(path[i]);
                path[i] = Path.GetDirectoryName(path[i]).Replace(@"\", "/");
                path[i] = !string.IsNullOrEmpty(path[i]) && path[i] != "/" ? path[i] : "/" + project.Root.Name;

                object obj = objects[i];
                string ext = project.GetExt(obj.GetType());
                ProjectItem item = project.Root.Get(path[i] + "/" + name + ext);
                if (item is AssetItem)
                {
                    AssetItem assetItem = (AssetItem)item;
                    existingAssetItems.Add(assetItem);
                }
                else
                {
                    ProjectItem folder = project.Root.Get(path[i]);
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

                return project.Save(existingAssetItems.ToArray(), objects, callback);
            }

            return project.Save(folders.ToArray(), previews, objects, names.ToArray(), callback);
        }

        public static ProjectAsyncOperation<UnityObject[]> Load<T>(this IProject project, string path, ProjectEventHandler<UnityObject[]> callback = null)
        {
            Type type = typeof(T);
            return Load(project, path, type, callback);
        }

        public static ProjectAsyncOperation<UnityObject[]> Load(this IProject project, string path, Type type, ProjectEventHandler<UnityObject[]> callback = null)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            AssetItem assetItem = project.Root.Get(path + project.GetExt(type)) as AssetItem;
            if (assetItem == null)
            {
                throw new ArgumentException("not found", "path");
            }

            return project.Load(new[] { assetItem }, callback);
        }

        public static ProjectAsyncOperation<ProjectItem> Rename<T>(this IProject project, string path, string newName)
        {
            Type type = typeof(T);
            return Rename(project, path, newName, type);
        }

        public static ProjectAsyncOperation<ProjectItem> Rename(this IProject project, string path, string newName, Type type)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            AssetItem projectItem = project.Root.Get(path + project.GetExt(type)) as AssetItem;
            if (projectItem == null)
            {
                throw new ArgumentException("not found", "path");
            }
            string oldName = projectItem.Name;
            projectItem.Name = newName;
            return project.Rename(projectItem, oldName);
        }

        public static ProjectAsyncOperation Delete<T>(this IProject project, string path)
        {
            Type type = typeof(T);
            return Delete(project, path, type);
        }

        public static ProjectAsyncOperation Delete(this IProject project, string path, Type type)
        {
            if (!project.IsOpened)
            {
                throw new InvalidOperationException("OpenProject first");
            }

            AssetItem projectItem = project.Root.Get(path + project.GetExt(type)) as AssetItem;
            if (projectItem == null)
            {
                throw new ArgumentException("not found", "path");
            }

            return project.Delete(new[] { projectItem });
        }

        public static void Unload<T>(this IProject project, string path)
        {
            project.Unload(path, typeof(T));
        }

        public static void Unload(this IProject project, string path, Type type)
        {
            AssetItem unloadItem = (AssetItem)project.Get(path, type);
            if(unloadItem == null)
            {
                Debug.Log("Unable to unload. Item was not found " + path);
                return;
            }
            project.Unload(new[] { unloadItem });
        }
    }
}
