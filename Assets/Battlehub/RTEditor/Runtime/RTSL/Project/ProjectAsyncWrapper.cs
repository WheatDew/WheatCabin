using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Battlehub.RTCommon;
using Battlehub.RTSL;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL
{
    public class TaskBasedProjectAsyncOperation : ProjectAsyncOperation
    {
        private Task m_task;
        public static Error TaskToError(Task task)
        {
            if (task.IsFaulted)
            {
                StorageException storageException = task.Exception.InnerException as StorageException;
                if (storageException != null)
                {
                    return new Error(
                        storageException.ErrorCode != Error.OK ?
                        storageException.ErrorCode :
                        Error.E_Exception)
                    {
                        ErrorText = storageException.Message
                    };
                }

                return new Error(Error.E_Exception) { ErrorText = task.Exception.Message };
            }

            return Error.NoError;
        }

        public override bool IsCompleted
        {
            get
            {
                if (!m_task.IsCompleted)
                {
                    return false;
                }

                Error = TaskToError(m_task);
                return true;
            }
            set => throw new NotSupportedException();
        }

        public TaskBasedProjectAsyncOperation(Task task)
        {
            m_task = task;
        }
    }

    public class TaskBasedProjectAsyncOperation<T> : ProjectAsyncOperation<T>
    {
        private Task m_task;
        private T m_result;
        private Func<T> m_getResult;

        public override bool IsCompleted
        {
            get
            {
                if (!m_task.IsCompleted)
                {
                    return false;
                }

                Error = TaskBasedProjectAsyncOperation.TaskToError(m_task);
                if(m_task is Task<T>)
                {
                    Result = ((Task<T>)m_task).Result;
                }
                else if(m_getResult != null)
                {
                    Result = m_getResult();
                }
                else
                {
                    Result = m_result;
                }
              
                return true;
            }
            set => throw new NotSupportedException();
        }

        public TaskBasedProjectAsyncOperation(Task<T> task)
        {
            m_task = task;
        }

        public TaskBasedProjectAsyncOperation(Task task, T result)
        {
            m_task = task;
            m_result = result;
        }

        public TaskBasedProjectAsyncOperation(Task task, Func<T> getResult)
        {
            m_task = task;
            m_getResult = getResult;
        }
    }

    public class TaskBasedProjectAsyncOperation<T1, T2> : ProjectAsyncOperation<T1, T2>
    {
        private Task m_task;
        private T1 m_result1;
        private T2 m_result2;
        private Func<T1> m_getResult1;
        private Func<T2> m_getResult2;

        public override bool IsCompleted
        {
            get
            {
                if (!m_task.IsCompleted)
                {
                    return false;
                }

                Error = TaskBasedProjectAsyncOperation.TaskToError(m_task);
                if (m_task is Task<(T1, T2)>)
                {
                    Result = ((Task<(T1, T2)>)m_task).Result.Item1;
                    Result2 = ((Task<(T1, T2)>)m_task).Result.Item2;
                }
                else if (m_getResult1 != null)
                {
                    Result = m_getResult1();
                    Result2 = m_getResult2();
                }
                else
                {
                    Result = m_result1;
                    Result2 = m_result2;
                }

                return true;
            }
            set => throw new NotSupportedException();
        }

        public TaskBasedProjectAsyncOperation(Task<(T1, T2)> task)
        {
            m_task = task;
        }

        public TaskBasedProjectAsyncOperation(Task task, T1 result1, T2 result2)
        {
            m_task = task;
            m_result1 = result1;
            m_result2 = result2;
        }

        public TaskBasedProjectAsyncOperation(Task task, Func<T1> getResult1, Func<T2> getResult2)
        {
            m_task = task;
            m_getResult1 = getResult1;
            m_getResult2 = getResult2;
        }
    }

    public class ProjectAsyncWrapper<TID> : IProject, IDisposable where TID : IEquatable<TID>
    {
        public event ProjectEventHandler NewSceneCreating;
        public event ProjectEventHandler NewSceneCreated;
        public event ProjectEventHandler<ProjectInfo> CreateProjectCompleted;
        public event ProjectEventHandler<ProjectInfo> OpenProjectCompleted;
        public event ProjectEventHandler<string> CopyProjectCompleted;
        public event ProjectEventHandler<string> ExportProjectCompleted;
        public event ProjectEventHandler<string> ImportProjectCompleted;
        public event ProjectEventHandler<string> DeleteProjectCompleted;
        public event ProjectEventHandler<ProjectInfo[]> ListProjectsCompleted;
        public event ProjectEventHandler CloseProjectCompleted;
        public event ProjectEventHandler<ProjectItem[]> GetAssetItemsCompleted;
        public event ProjectEventHandler<object[]> BeginSave;
        public event ProjectEventHandler<AssetItem[], bool> SaveCompleted;
        public event ProjectEventHandler<AssetItem[]> BeginLoad;
        public event ProjectEventHandler<AssetItem[], UnityObject[]> LoadCompleted;
        public event ProjectEventHandler<AssetItem[]> DuplicateCompleted;
        public event ProjectEventHandler<ProjectItem[]> DuplicateItemsCompleted;
        public event ProjectEventHandler UnloadCompleted;
        public event ProjectEventHandler<AssetItem[]> ImportCompleted;
        public event ProjectEventHandler<ProjectItem[]> BeforeDeleteCompleted;
        public event ProjectEventHandler<ProjectItem[]> DeleteCompleted;
        public event ProjectEventHandler<ProjectItem[], ProjectItem[]> MoveCompleted;
        public event ProjectEventHandler<ProjectItem> RenameCompleted;
        public event ProjectEventHandler<ProjectItem[]> CreateCompleted;

        private bool m_isBusy;
        public bool IsBusy
        {
            get => m_isBusy;
            private set => m_isBusy = value;
        }

        public bool IsOpened => m_projectAsync.State.IsOpened;

        public ProjectInfo ProjectInfo => m_projectAsync.State.ProjectInfo;

        public ProjectItem Root => m_projectAsync.State.RootFolder;

        public AssetItem LoadedScene
        {
            get => m_projectAsync.State.LoadedScene as AssetItem;
            set => m_projectAsync.LoadedScene = value;
        }

        public AssetBundle[] LoadedAssetBundles => m_projectAsync.LoadedAssetBundles;

        private ProjectAsyncWithAssetLibraries<TID> m_projectAsync;
        private IRuntimeSceneManager m_sceneManager;
        private CancellationTokenSource m_cts;
        
        public ProjectAsyncWrapper(ProjectAsyncWithAssetLibraries<TID> projectAsync, IRuntimeSceneManager sceneManager)
        {
            m_projectAsync = projectAsync;
            m_sceneManager = sceneManager;

            if(m_sceneManager != null)
            {
                m_sceneManager.NewSceneCreating += OnNewSceneCreating;
                m_sceneManager.NewSceneCreated += OnNewSceneCreated;
            }
            
            m_projectAsync.Events.GetProjectsCompleted += OnGetProjectsCompleted;
            m_projectAsync.Events.CreateProjectCompleted += OnCreateProjectCompleted;
            m_projectAsync.Events.CopyProjectCompleted += OnCopyProjectCompleted;
            m_projectAsync.Events.DeleteProjectCompleted += OnDeleteProjectCompleted;
            m_projectAsync.Events.ExportProjectCompleted += OnExportProjectCompleted;
            m_projectAsync.Events.ImportProjectCompleted += OnImportProjectCompleted;
            m_projectAsync.Events.OpenProjectCompleted += OnOpenProjectCompleted;
            m_projectAsync.Events.CloseProjectCompleted += OnCloseProjectCompleted;
            m_projectAsync.Events.BeginSave += OnBeginSave;
            m_projectAsync.Events.SaveCompleted += OnSaveCompleted;
            m_projectAsync.Events.BeginLoad += OnBeginLoad;
            m_projectAsync.Events.LoadCompleted += OnLoadCompleted;
            m_projectAsync.Events.UnloadAllCompleted += OnUnloadAllCompleted;
            m_projectAsync.Events.DuplicateCompleted += OnDuplicateCompleted;
            m_projectAsync.Events.CreatePrefabsCompleted += OnCreatePrefabsCompleted;
            m_projectAsync.Events.CreateFoldersCompleted += OnCreateFoldersCompleted;
            m_projectAsync.Events.MoveCompleted += OnMoveCompleted;
            m_projectAsync.Events.RenameCompleted += OnRenameCompleted;
            m_projectAsync.Events.DeleteCompleted += OnDeleteCompleted;
            m_projectAsync.Events.ImportCompleted += OnImportCompleted;

            m_cts = new CancellationTokenSource();
        }

  
        public void Dispose()
        {
            if(m_projectAsync == null)
            {
                return;
            }

            if(m_sceneManager != null)
            {
                m_sceneManager.NewSceneCreating -= OnNewSceneCreating;
                m_sceneManager.NewSceneCreated -= OnNewSceneCreated;
            }
            
            m_projectAsync.Events.GetProjectsCompleted -= OnGetProjectsCompleted;
            m_projectAsync.Events.CreateProjectCompleted -= OnCreateProjectCompleted;
            m_projectAsync.Events.CopyProjectCompleted -= OnCopyProjectCompleted;
            m_projectAsync.Events.DeleteProjectCompleted -= OnDeleteProjectCompleted;
            m_projectAsync.Events.ExportProjectCompleted -= OnExportProjectCompleted;
            m_projectAsync.Events.ImportProjectCompleted -= OnImportProjectCompleted;
            m_projectAsync.Events.OpenProjectCompleted -= OnOpenProjectCompleted;
            m_projectAsync.Events.CloseProjectCompleted -= OnCloseProjectCompleted;
            m_projectAsync.Events.BeginSave -= OnBeginSave;
            m_projectAsync.Events.SaveCompleted -= OnSaveCompleted;
            m_projectAsync.Events.BeginLoad -= OnBeginLoad;
            m_projectAsync.Events.LoadCompleted -= OnLoadCompleted;
            m_projectAsync.Events.UnloadAllCompleted -= OnUnloadAllCompleted;
            m_projectAsync.Events.DuplicateCompleted -= OnDuplicateCompleted;
            m_projectAsync.Events.CreatePrefabsCompleted -= OnCreatePrefabsCompleted;
            m_projectAsync.Events.CreateFoldersCompleted -= OnCreateFoldersCompleted;
            m_projectAsync.Events.MoveCompleted -= OnMoveCompleted;
            m_projectAsync.Events.RenameCompleted -= OnRenameCompleted;
            m_projectAsync.Events.DeleteCompleted -= OnDeleteCompleted;
            m_projectAsync.Events.ImportCompleted -= OnImportCompleted;

            m_projectAsync = null;
            m_sceneManager = null;

            m_cts.Cancel();
        }

        private void OnNewSceneCreating(object sender, EventArgs e)
        {
            NewSceneCreating?.Invoke(Error.NoError);
        }

        private void OnNewSceneCreated(object sender, EventArgs e)
        {
            NewSceneCreated?.Invoke(Error.NoError);
        }

        private void OnGetProjectsCompleted(object sender, ProjectEventArgs<ProjectInfo[]> e)
        {
            ListProjectsCompleted?.Invoke(Error.NoError, e.Payload);
        }

        private void OnCreateProjectCompleted(object sender, ProjectEventArgs<ProjectInfo> e)
        {
            CreateProjectCompleted?.Invoke(Error.NoError, e.Payload);
        }

        private void OnCopyProjectCompleted(object sender, ProjectEventArgs<(string Project, string TargetProject)> e)
        {
            CopyProjectCompleted?.Invoke(Error.NoError, e.Payload.TargetProject);
        }

        private void OnDeleteProjectCompleted(object sender, ProjectEventArgs<string> e)
        {
            DeleteProjectCompleted?.Invoke(Error.NoError, e.Payload);
        }

        private void OnExportProjectCompleted(object sender, ProjectEventArgs<(string Project, string TargetPath)> e)
        {
            ExportProjectCompleted?.Invoke(Error.NoError, e.Payload.Project);
        }

        private void OnImportProjectCompleted(object sender, ProjectEventArgs<(string Project, string SourcePath)> e)
        {
            ImportProjectCompleted?.Invoke(Error.NoError, e.Payload.Project);
        }

        private void OnOpenProjectCompleted(object sender, ProjectEventArgs<ProjectInfo> e)
        {
            OpenProjectCompleted?.Invoke(Error.NoError, e.Payload);
        }

        private void OnCloseProjectCompleted(object sender, ProjectEventArgs<string> e)
        {
            CloseProjectCompleted?.Invoke(Error.NoError);
        }

        private void OnBeginSave(object sender, ProjectEventArgs<object[]> e)
        {
            BeginSave?.Invoke(Error.NoError, e.Payload);
        }

        private void OnSaveCompleted(object sender, ProjectEventArgs<(ProjectItem[] SavedItems, bool IsUserAction)> e)
        {
            SaveCompleted?.Invoke(
                e.HasError ? new Error(Error.E_Failed) : Error.NoError,
                e.Payload.SavedItems?.Cast<AssetItem>().ToArray(),
                e.Payload.IsUserAction);
        }

        private void OnBeginLoad(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            BeginLoad?.Invoke(Error.NoError, e.Payload.Cast<AssetItem>().ToArray());
        }

        private void OnLoadCompleted(object sender, ProjectEventArgs<(ProjectItem[] LoadedItems, UnityObject[] LoadedObjects)> e)
        {
            LoadCompleted?.Invoke(
                e.HasError ? new Error(Error.E_Failed) : Error.NoError,
                e.Payload.LoadedItems?.Cast<AssetItem>().ToArray(),
                e.Payload.LoadedObjects);
        }

        private void OnUnloadAllCompleted(object sender, ProjectEventArgs e)
        {
            UnloadCompleted?.Invoke(Error.NoError);
        }

        private void OnDuplicateCompleted(object sender, ProjectEventArgs<(ProjectItem[] OriginalItems, ProjectItem[] DuplicatedItems)> e)
        {
            DuplicateItemsCompleted?.Invoke(Error.NoError, e.Payload.DuplicatedItems);
            DuplicateCompleted?.Invoke(Error.NoError, e.Payload.DuplicatedItems?.OfType<AssetItem>().ToArray());
        }

        private void OnCreatePrefabsCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            //No IProject event
        }

        private void OnCreateFoldersCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            CreateCompleted?.Invoke(Error.NoError, e.Payload);
        }

        private void OnMoveCompleted(object sender, ProjectEventArgs<(ProjectItem[] OriginalParentItems, ProjectItem[] MovedItems)> e)
        {
            MoveCompleted?.Invoke(Error.NoError, e.Payload.MovedItems, e.Payload.OriginalParentItems);
        }

        private void OnRenameCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            RenameCompleted?.Invoke(Error.NoError, e.Payload?.FirstOrDefault());
        }

        private void OnDeleteCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            BeforeDeleteCompleted?.Invoke(Error.NoError, e.Payload);
            DeleteCompleted?.Invoke(Error.NoError, e.Payload);
        }

        private void OnImportCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            ImportCompleted?.Invoke(Error.NoError, e.Payload?.OfType<AssetItem>().ToArray());
        }

        public bool IsStatic(ProjectItem projectItem)
        {
            return m_projectAsync.Utils.IsStatic(projectItem);
        }

        public bool IsScene(ProjectItem projectItem)
        {
            return m_projectAsync.Utils.IsScene(projectItem);
        }

        public Type ToType(AssetItem assetItem)
        {
            return m_projectAsync.Utils.ToType(assetItem);
        }

        public Guid ToGuid(Type type)
        {
            return m_projectAsync.Utils.ToGuid(type);
        }

        public object ToPersistentID(UnityObject obj)
        {
            return m_projectAsync.Utils.ToPersistentID(obj);
        }

        public object ToPersistentID(ProjectItem projectItem)
        {
            return m_projectAsync.Utils.ToPersistentID(projectItem);
        }

        public void SetPersistentID(ProjectItem projectItem, object id)
        {
            m_projectAsync.Utils.SetPersistentID(projectItem, id);
        }

        public object ToPersistentID(Preview preview)
        {
            return preview.ItemID;
        }

        public void SetPersistentID(Preview preview, object id)
        {
            preview.ItemID = (long)id;
        }

        public T FromPersistentID<T>(object id) where T : UnityObject
        {
            return m_projectAsync.Utils.FromPersistentID<T>(id);
        }

        public T FromPersistentID<T>(ProjectItem projectItem) where T : UnityObject
        {
            return m_projectAsync.Utils.FromProjectItem<T>(projectItem);
        }

        public long ToID(UnityObject obj)
        {
            return (long)ToPersistentID(obj);
        }

        public T FromID<T>(long id) where T : UnityObject
        {
            return FromPersistentID<T>(id);
        }

        public AssetItem ToAssetItem(UnityObject obj)
        {
            return m_projectAsync.Utils.ToProjectItem(obj) as AssetItem;
        }

        public string GetExt(object obj)
        {
            return m_projectAsync.Utils.GetExt(obj);
        }

        public string GetExt(Type type)
        {
            return m_projectAsync.Utils.GetExt(type);
        }

        public string GetUniqueName(string name, string[] names)
        {
            return m_projectAsync.Utils.GetUniqueName(name, names);
        }

        public string GetUniqueName(string name, Type type, ProjectItem folder, bool noSpace = false)
        {
            return m_projectAsync.Utils.GetUniqueName(name, type, folder, noSpace);
        }

        public string GetUniqueName(string name, string ext, ProjectItem folder, bool noSpace = false)
        {
            return m_projectAsync.Utils.GetUniqueName(name, ext, folder, noSpace);
        }

        public string GetUniquePath(string path, Type type, ProjectItem folder, bool noSpace = false)
        {
            return m_projectAsync.Utils.GetUniquePath(path, type, folder, noSpace);
        }

        [Obsolete]
        public object[] FindDeepDependencies(object obj)
        {
            return m_projectAsync.Utils.FindDeepDependencies(obj);
        }

        public AssetItem[] GetDependantAssetItems(AssetItem[] assetItems)
        {
            return m_projectAsync.Utils.GetProjectItemsDependentOn(assetItems).OfType<AssetItem>().ToArray();
        }

        public YieldLock Lock()
        {
            return new YieldLock(null);
        }

        public void CreateNewScene()
        {
            m_sceneManager.CreateNewScene();
        }

        public void ClearScene()
        {
            m_sceneManager.ClearScene();
        }

        private async void RunTaskAsync(Task task, ProjectEventHandler callback)
        {
            try
            {
                IsBusy = true;
                await task;
                IsBusy = false;
                callback?.Invoke(Error.NoError);
            }
            catch (StorageException e)
            {
                IsBusy = false;
                callback?.Invoke(
                    new Error(e.ErrorCode != Error.OK ? e.ErrorCode : Error.E_Exception) { ErrorText = e.ToString() });
            }
            catch (Exception e)
            {
                IsBusy = false;
                callback?.Invoke(new Error(Error.E_Exception) { ErrorText = e.ToString() });
            }
        }

        private async void RunTaskAsync<T>(Task task, T result, ProjectEventHandler<T> callback)
        {
            try
            {
                IsBusy = true;
                await task;
                IsBusy = false;
                callback?.Invoke(Error.NoError, result);
            }
            catch (StorageException e)
            {
                IsBusy = false;
                callback?.Invoke(
                    new Error(e.ErrorCode != Error.OK ? e.ErrorCode : Error.E_Exception) { ErrorText = e.ToString() },
                    result);
            }
            catch (Exception e)
            {
                IsBusy = false;
                callback?.Invoke(
                    new Error(Error.E_Exception) { ErrorText = e.ToString() },
                    result);
            }
        }

        private async void RunTaskAsync<T>(Task<T> task, ProjectEventHandler<T> callback, T defaultValue = default)
        {
            try
            {
                IsBusy = true;
                T result = await task;
                IsBusy = false;
                callback?.Invoke(Error.NoError, result);
            }
            catch (StorageException e)
            {
                IsBusy = false;
                callback?.Invoke(
                    new Error(e.ErrorCode != Error.OK ? e.ErrorCode : Error.E_Exception) { ErrorText = e.ToString() },
                    defaultValue);
            }
            catch (Exception e)
            {
                IsBusy = false;
                callback?.Invoke(new Error(Error.E_Exception) { ErrorText = e.ToString() }, defaultValue);
            }
        }

        public ProjectAsyncOperation<ProjectInfo[]> GetProjects(ProjectEventHandler<ProjectInfo[]> callback = null)
        {
            Task<ProjectInfo[]> task = m_projectAsync.Safe.GetProjectsAsync(m_cts.Token);
            RunTaskAsync(task, callback, new ProjectInfo[0]);
            return new TaskBasedProjectAsyncOperation<ProjectInfo[]>(task);
        }

        public ProjectAsyncOperation<ProjectInfo> CreateProject(string project, ProjectEventHandler<ProjectInfo> callback = null)
        {
            Task<ProjectInfo> task = m_projectAsync.Safe.CreateProjectAsync(project, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<ProjectInfo>(task);
        }

        public ProjectAsyncOperation<ProjectInfo> OpenProject(string project, ProjectEventHandler<ProjectInfo> callback = null)
        {
            Task<ProjectInfo> task = m_projectAsync.Safe.OpenProjectAsync(project, OpenProjectFlags.Default, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<ProjectInfo>(task);
        }

        public ProjectAsyncOperation<ProjectInfo> OpenProject(string project, OpenProjectFlags flags, ProjectEventHandler<ProjectInfo> callback = null)
        {
            Task<ProjectInfo> task = m_projectAsync.Safe.OpenProjectAsync(project, flags, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<ProjectInfo>(task);
        }

        public void CloseProject()
        {
            Task task = m_projectAsync.Safe.CloseProjectAsync(m_cts.Token);
            RunTaskAsync(task, error => { });
        }

        public ProjectAsyncOperation<string> CopyProject(string project, string targetProject, ProjectEventHandler<string> callback = null)
        {
            Task task = m_projectAsync.Safe.CopyProjectAsync(project, targetProject, m_cts.Token);
            RunTaskAsync(task, targetProject, callback);
            return new TaskBasedProjectAsyncOperation<string>(task, targetProject);
        }

        public ProjectAsyncOperation<string> DeleteProject(string project, ProjectEventHandler<string> callback = null)
        {
            Task task = m_projectAsync.Safe.DeleteProjectAsync(project, m_cts.Token);
            RunTaskAsync(task, project, callback);
            return new TaskBasedProjectAsyncOperation<string>(task, project);
        }

        public ProjectAsyncOperation<string> ExportProject(string project, string targetPath, ProjectEventHandler<string> callback = null)
        {
            Task task = m_projectAsync.Safe.ExportProjectAsync(project, targetPath, m_cts.Token);
            RunTaskAsync(task, project, callback);
            return new TaskBasedProjectAsyncOperation<string>(task, project);
        }

        public ProjectAsyncOperation<string> ImportProject(string projectName, string sourcePath, bool overwrite = false, ProjectEventHandler<string> callback = null)
        {
            Task task = m_projectAsync.Safe.ImportProjectAsync(projectName, sourcePath, overwrite, m_cts.Token);
            RunTaskAsync(task, projectName, callback);
            return new TaskBasedProjectAsyncOperation<string>(task, projectName);
        }

        public ProjectAsyncOperation<ProjectItem> CreateFolder(ProjectItem projectItem, ProjectEventHandler<ProjectItem> callback = null)
        {
            Task<ProjectItem[]> task = m_projectAsync.Safe.CreateFoldersAsync(new[] { projectItem.Parent }, new[] { projectItem.Name }, m_cts.Token);
            RunTaskAsync(task, (error, result) => callback?.Invoke(error, (result != null && result.Length > 0) ? result[0] : null)); 
            return new TaskBasedProjectAsyncOperation<ProjectItem>(task, () => (task.Result != null && task.Result.Length > 0) ? task.Result[0] : null);
        }

        public ProjectAsyncOperation<ProjectItem[]> CreateFolders(ProjectItem[] projectItem, ProjectEventHandler<ProjectItem[]> callback = null)
        {
            Task<ProjectItem[]> task = m_projectAsync.Safe.CreateFoldersAsync(projectItem.Select(p => p.Parent).ToArray(), projectItem.Select(p => p.Name).ToArray(), m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<ProjectItem[]>(task);
        }

        private async Task CreatePrefabAsync(string folderPath, GameObject prefab, bool includeDeps, Func<UnityEngine.Object, byte[]> createPreview = null)
        {
            ProjectItem folder;
            using (await m_projectAsync.LockAsync(m_cts.Token))
            {
                folder = Root.GetOrCreateFolder(folderPath);
                if (folder is AssetItem || folder == null)
                {
                    throw new ArgumentException("folderPath");
                }
            }
             
            Task task = m_projectAsync.Safe.CreatePrefabsAsync(new[] { folder }, new[] { prefab }, includeDeps, createPreview, m_cts.Token);
            RunTaskAsync(task, error => { });
        }

        public ProjectAsyncOperation CreatePrefab(string folderPath, GameObject prefab, bool includeDeps, Func<UnityEngine.Object, byte[]> createPreview = null)
        {
            Task task = CreatePrefabAsync(folderPath, prefab, includeDeps, createPreview);
            return new TaskBasedProjectAsyncOperation(task);
        }

        public ProjectAsyncOperation<AssetItem[]> Duplicate(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null)
        {
            Task<ProjectItem[]> task = m_projectAsync.Safe.DuplicateAsync(assetItems, m_cts.Token);
            RunTaskAsync(task, (error, result) => callback?.Invoke(error, result != null ? result.OfType<AssetItem>().ToArray() : null));
            return new TaskBasedProjectAsyncOperation<AssetItem[]>(task, () => task.Result != null ? task.Result.OfType<AssetItem>().ToArray() : new AssetItem[0]);
        }

        public ProjectAsyncOperation<ProjectItem[]> Duplicate(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback = null)
        {
            Task<ProjectItem[]> task = m_projectAsync.Safe.DuplicateAsync(projectItems, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<ProjectItem[]>(task);
        }

        public ProjectAsyncOperation<ProjectItem[], ProjectItem[]> Move(ProjectItem[] projectItems, ProjectItem target, ProjectEventHandler<ProjectItem[], ProjectItem[]> callback = null)
        {
            ProjectItem[] oldParents = projectItems.Select(item => item.Parent).ToArray();
            Task task = m_projectAsync.Safe.MoveAsync(projectItems, target, m_cts.Token);
            RunTaskAsync(task, error => callback?.Invoke(error, projectItems.Select(item => item.Parent).ToArray(), oldParents));
            return new TaskBasedProjectAsyncOperation<ProjectItem[], ProjectItem[]>(task, () => projectItems.Select(item => item.Parent).ToArray(), () => oldParents);
        }

        public ProjectAsyncOperation<ProjectItem> Rename(ProjectItem projectItem, string oldName, ProjectEventHandler<ProjectItem> callback = null)
        {
            string name = projectItem.Name;
            projectItem.Name = oldName;

            Task task = m_projectAsync.Safe.RenameAsync(new[] { projectItem }, new[] { name }, m_cts.Token);
            RunTaskAsync(task, error => callback?.Invoke(error, projectItem));

            return new TaskBasedProjectAsyncOperation<ProjectItem>(task, projectItem);
        }

        public ProjectAsyncOperation<ProjectItem[]> Delete(ProjectItem[] projectItems, ProjectEventHandler<ProjectItem[]> callback = null)
        {
            Task task = m_projectAsync.Safe.DeleteAsync(projectItems, m_cts.Token);
            RunTaskAsync(task, error => callback?.Invoke(error, projectItems));
            return new TaskBasedProjectAsyncOperation<ProjectItem[]>(task, projectItems);
        }

        public ProjectAsyncOperation Delete(string projectPath, string[] files, ProjectEventHandler callback = null)
        {
            Task task = DeleteAsyncSafe(projectPath, files, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation(task);
        }

        private async Task DeleteAsyncSafe(string projectPath, string[] files, CancellationToken ct)
        {
            IStorageAsync<long> storage = IOC.Resolve<IStorageAsync<long>>();

            using(await m_projectAsync.LockAsync(ct))
            {
                await storage.DeleteAsync(projectPath, files, m_cts.Token);
            }
        }

        public ProjectAsyncOperation<string[]> GetAssetBundles(ProjectEventHandler<string[]> callback = null)
        {
            Task<string[]> task = m_projectAsync.Safe.GetAssetBundlesAsync(m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<string[]>(task);
        }

        public ProjectAsyncOperation<AssetItem[]> GetAssetItems(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null)
        {
            throw new NotSupportedException();
        }

        public ProjectAsyncOperation<ProjectItem[]> GetAssetItems(ProjectItem[] folders, ProjectEventHandler<ProjectItem[]> callback = null)
        {
            Task<ProjectItem[]> task = m_projectAsync.Safe.GetAssetItemsAsync(folders, null, m_cts.Token);
            RunTaskAsync(task, (error, projectItems) =>
            {
                callback?.Invoke(error, projectItems);
                GetAssetItemsCompleted?.Invoke(error, projectItems);
            });
            return new TaskBasedProjectAsyncOperation<ProjectItem[]>(task);
        }

        public ProjectAsyncOperation<ProjectItem[]> GetAssetItems(ProjectItem[] folders, string searchPattern, ProjectEventHandler<ProjectItem[]> callback = null)
        {
            Task<ProjectItem[]> task = m_projectAsync.Safe.GetAssetItemsAsync(folders, searchPattern, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<ProjectItem[]>(task);
        }

        private static Preview[] GetPreviews(AssetItem[] assetItems, byte[][] previewData)
        {
            Preview[] previews;
            if (previewData != null && assetItems.Length == previewData.Length)
            {
                previews = new Preview[assetItems.Length];
                for (int i = 0; i < previews.Length; ++i)
                {
                    previews[i] = new Preview
                    {
                        ItemID = assetItems[i].ItemID,
                        PreviewData = previewData[i]
                    };
                }
            }
            else
            {
                previews = new Preview[0];
            }
            return previews;
        }

        public ProjectAsyncOperation<Preview[]> GetPreviews(AssetItem[] assetItems, ProjectEventHandler<Preview[]> callback = null)
        {
            Task<byte[][]> task = m_projectAsync.Safe.GetPreviewsAsync(assetItems, m_cts.Token);
            RunTaskAsync(task, (error, previewData) => callback?.Invoke(error, GetPreviews(assetItems, previewData)));
            return new TaskBasedProjectAsyncOperation<Preview[]>(task, () => GetPreviews(assetItems, task.Result));
        }

        public ProjectAsyncOperation<object[]> GetDependencies(object obj, bool exceptMappedObject = false, ProjectEventHandler<object[]> callback = null)
        {
            Task<object[]> task = m_projectAsync.Safe.GetDependenciesAsync(obj, exceptMappedObject, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<object[]>(task);

        }

        public ProjectAsyncOperation<AssetItem[]> Save(AssetItem[] assetItems, object[] obj, ProjectEventHandler<AssetItem[]> callback = null)
        {
            Task task = m_projectAsync.Safe.SaveAsync(assetItems, obj, m_cts.Token);
            RunTaskAsync(task, error => callback?.Invoke(error, assetItems));
            return new TaskBasedProjectAsyncOperation<AssetItem[]>(task, assetItems);
        }

        public ProjectAsyncOperation<AssetItem[]> Save(ProjectItem[] parents, byte[][] previewData, object[] obj, string[] nameOverrides, ProjectEventHandler<AssetItem[]> callback = null)
        {
            return Save(parents, previewData, obj, nameOverrides, true, callback);
        }

        public ProjectAsyncOperation<AssetItem[]> Save(ProjectItem[] parents, byte[][] previewData, object[] obj, string[] nameOverrides, bool isUserAction, ProjectEventHandler<AssetItem[]> callback = null)
        {
            Task<ProjectItem[]> task = m_projectAsync.Safe.SaveAsync(parents, previewData, obj, nameOverrides, isUserAction, m_cts.Token);
            RunTaskAsync(task, (error, projectItems) => callback?.Invoke(error, projectItems != null ? projectItems.OfType<AssetItem>().ToArray() : new AssetItem[0]));
            return new TaskBasedProjectAsyncOperation<AssetItem[]>(task, () => task.Result != null ? task.Result.OfType<AssetItem>().ToArray() : null);
        }

        public ProjectAsyncOperation<AssetItem[]> SavePreview(AssetItem[] assetItems, ProjectEventHandler<AssetItem[]> callback = null)
        {
            Task task = m_projectAsync.Safe.SavePreviewsAsync(assetItems, m_cts.Token);
            RunTaskAsync(task, error => callback?.Invoke(error, assetItems));
            return new TaskBasedProjectAsyncOperation<AssetItem[]>(task, assetItems);
        }

        public ProjectAsyncOperation<UnityObject[]> Load(AssetItem[] assetItems, ProjectEventHandler<UnityEngine.Object[]> callback = null)
        {
            Task<UnityObject[]> task = m_projectAsync.Safe.LoadAsync(assetItems, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<UnityObject[]>(task);
        }

        public void Unload(AssetItem[] assetItems)
        {
            Task task = m_projectAsync.Safe.UnloadAsync(assetItems, m_cts.Token);
            RunTaskAsync(task, error => { });
        }

        public ProjectAsyncOperation Unload(ProjectEventHandler completedCallback = null)
        {
            Task task = m_projectAsync.Safe.UnloadAllAsync(m_cts.Token);
            RunTaskAsync(task, completedCallback);
            return new TaskBasedProjectAsyncOperation(task);
        }

        public Dictionary<int, string> GetStaticAssetLibraries()
        {
            return m_projectAsync.AssetLibraryIDToLibraryName;
        }

        public ProjectAsyncOperation<ProjectItem> LoadImportItems(string path, bool isBuiltIn, ProjectEventHandler<ProjectItem> callback = null)
        {
            Task<ProjectItem> task = m_projectAsync.Safe.LoadImportItemsAsync(path, isBuiltIn, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<ProjectItem>(task);
        }

        public void UnloadImportItems(ProjectItem importItemsRoot)
        {
            m_projectAsync.UnloadImportItems(importItemsRoot);
        }

        public ProjectAsyncOperation<AssetItem[]> Import(ProjectItem[] importItems, ProjectEventHandler<AssetItem[]> callback = null)
        {
            Task<ProjectItem[]> task = m_projectAsync.Safe.ImportAsync(importItems, m_cts.Token);
            RunTaskAsync(task, (error, projectItems) => callback?.Invoke(error, projectItems != null ? projectItems.OfType<AssetItem>().ToArray() : new AssetItem[0]));
            return new TaskBasedProjectAsyncOperation<AssetItem[]>(task, () => task.Result != null ? task.Result.OfType<AssetItem>().ToArray() : new AssetItem[0]);
        }

        public ProjectAsyncOperation<T> GetValue<T>(string key, ProjectEventHandler<T> callback = null)
        {
            Task<T> task = m_projectAsync.Safe.GetValueAsync<T>(key, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<T>(task);
        }

        public ProjectAsyncOperation<T[]> GetValues<T>(string searchPattern, ProjectEventHandler<T[]> callback = null)
        {
            Task<T[]> task = m_projectAsync.Safe.GetValuesAsync<T>(searchPattern, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation<T[]>(task);
        }

        public ProjectAsyncOperation SetValue<T>(string key, T obj, ProjectEventHandler callback = null)
        {
            Task task = m_projectAsync.Safe.SetValueAsync(key, obj, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation(task);
        }

        public ProjectAsyncOperation DeleteValue<T>(string key, ProjectEventHandler callback = null)
        {
            Task task = m_projectAsync.Safe.DeleteValueAsync<T>(key, m_cts.Token);
            RunTaskAsync(task, callback);
            return new TaskBasedProjectAsyncOperation(task);
        }
    }
}

