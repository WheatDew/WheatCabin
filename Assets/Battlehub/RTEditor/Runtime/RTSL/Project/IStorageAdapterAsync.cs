using Battlehub.RTSL.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Battlehub.SL2;

namespace Battlehub.RTSL
{
    public class IStorageAdapterAsync<TID> : IStorageAsync<TID> where TID : IEquatable<TID>
    {
        private IStorage<TID> m_storage;
        public IStorageAdapterAsync(IStorage<TID> storage)
        {
            m_storage = storage;
        }

        public ProjectItem CreateAssetItem(TID id, Guid typeGuid, string name, string ext, ProjectItem parent)
        {
            AssetItem<TID> assetItem = new AssetItem<TID>
            {
                ID = id,
                TypeGuid = typeGuid,
                Name = name,
                Ext = ext,
            };

            if (parent != null)
            {
                parent.AddChild(assetItem);
            }
            return assetItem;
        }

        public TID GetID(ProjectItem projectItem)
        {
            AssetItem<TID> assetItem = (AssetItem<TID>)projectItem;
            return assetItem.ID;
        }
        public void SetID(ProjectItem projectItem, TID id)
        {
            AssetItem<TID> assetItem = (AssetItem<TID>)projectItem;
            assetItem.ID = id;
        }

        public TID[] GetDependencyIDs(ProjectItem projectItem)
        {
            AssetItem<TID> assetItem = (AssetItem<TID>)projectItem;
            return assetItem.DependencyIDs;
        }

        public void SetDependencyIDs(ProjectItem projectItem, TID[] ids)
        {
            AssetItem<TID> assetItem = (AssetItem<TID>)projectItem;
            assetItem.DependencyIDs = ids;
        }

        public TID[] GetEmbeddedIDs(ProjectItem projectItem)
        {
            AssetItem<TID> assetItem = (AssetItem<TID>)projectItem;
            return assetItem.EmbeddedIDs;
        }

        public void SetEmbeddedIDs(ProjectItem projectItem, TID[] ids)
        {
            AssetItem<TID> assetItem = (AssetItem<TID>)projectItem;
            assetItem.EmbeddedIDs = ids;
        }

        public Task<string> GetRootPathAsync()
        {
            var tcs = new TaskCompletionSource<string>();
            tcs.SetResult(m_storage.RootPath);
            return tcs.Task;
        }
        public Task SetRootPathAsync(string rootPath)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.RootPath = rootPath;
            tcs.SetResult(null);
            return tcs.Task;
        }

        private static void TrySetResultOrException<T>(TaskCompletionSource<T> tcs, CancellationToken ct, Error error, T result = default(T))
        {
            ct.ThrowIfCancellationRequested();

            if (error.HasError)
            {
                tcs.TrySetException(new StorageException(error.ErrorCode, error.ErrorText));
            }
            else
            {
                tcs.TrySetResult(result);
            }
        }

        public Task<ProjectInfo> CreateProjectAsync(string projectPath, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<ProjectInfo>();
            m_storage.CreateProject(projectPath, (error, projectInfo) => TrySetResultOrException(tcs, ct, error, projectInfo));
            return tcs.Task;
        }

        public Task CopyProjectAsync(string projectPath, string targetPath, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.CopyProject(projectPath, targetPath, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }
        public Task ExportProjectAsync(string projectPath, string targetPath, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.ExportProject(projectPath, targetPath, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }
        public Task ImportProjectAsync(string projectPath, string sourcePath, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.ImportProject(projectPath, sourcePath, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }
        public Task DeleteProjectAsync(string projectPath, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.DeleteProject(projectPath, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }

        public Task<ProjectInfo[]> GetProjectsAsync(CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<ProjectInfo[]>();
            m_storage.GetProjects((error, result) => TrySetResultOrException(tcs, ct, error, result));
            return tcs.Task;
        }

        public Task<(ProjectInfo, AssetBundleInfo[])> GetOrCreateProjectAsync(string projectPath, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<(ProjectInfo, AssetBundleInfo[])>();
            m_storage.GetProject(projectPath, (error, result1, result2) => TrySetResultOrException(tcs, ct, error, (result1, result2)));
            return tcs.Task;
        }

        public Task<ProjectItem> GetProjectTreeAsync(string projectPath, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<ProjectItem>();
            m_storage.GetProjectTree(projectPath, (error, result) => TrySetResultOrException(tcs, ct, error, result));
            return tcs.Task;
        }

        public Task<Preview<TID>[]> GetPreviewsAsync(string projectPath, string[] assetPath, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<Preview<TID>[]>();
            m_storage.GetPreviews(projectPath, assetPath, (Error error, Preview[] result) => TrySetResultOrException(tcs, ct, error, result.Select(p => p.ConvertToGenericPreview<TID>()).ToArray()));
            return tcs.Task;
        }

        public Task<Preview<TID>[][]> GetPreviewsPerFolderAsync(string projectPath, string[] folderPath, CancellationToken ct)
        {
            return GetPreviewsPerFolderAsync(projectPath, folderPath, null, ct);
        }

        public Task<Preview<TID>[][]> GetPreviewsPerFolderAsync(string projectPath, string[] folderPath, string searchPattern, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<Preview<TID>[][]>();
            if (searchPattern == null)
            {
                m_storage.GetPreviews(projectPath, folderPath, (error, result) => TrySetResultOrException(tcs, ct, error, result.Select(r => r != null ? r.Select(p => p.ConvertToGenericPreview<TID>()).ToArray() : null).ToArray()));
            }
            else
            {
                m_storage.GetPreviews(projectPath, folderPath, searchPattern, (error, result) => TrySetResultOrException(tcs, ct, error, result.Select(r => r != null ? r.Select(p => p.ConvertToGenericPreview<TID>()).ToArray() : null).ToArray()));
            }
            return tcs.Task;
        }

        public Task SaveAsync(string projectPath, string[] folderPaths, ProjectItem[] projectItems, object[] persistentObjects, ProjectInfo projectInfo, bool previewOnly, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.Save(projectPath, folderPaths, projectItems.OfType<AssetItem>().ToArray(), persistentObjects != null ? persistentObjects.OfType<PersistentObject<TID>>().ToArray() : null, projectInfo, previewOnly, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }
        public Task SaveAsync(string projectPath, AssetBundleInfo assetBundleInfo, ProjectInfo projectInfo, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.Save(projectPath, assetBundleInfo, projectInfo, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }

        public Task<object[]> LoadAsync(string projectPath, ProjectItem[] projectItems, Type[] types, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object[]>();
            m_storage.Load(projectPath, projectItems.OfType<AssetItem>().ToArray(), types, (error, result) => TrySetResultOrException(tcs, ct, error, result));
            return tcs.Task;
        }

        public Task<AssetBundleInfo> LoadAsync(string projectPath, string bundleName, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<AssetBundleInfo>();
            m_storage.Load(projectPath, bundleName, (error, result) => TrySetResultOrException(tcs, ct, error, result));
            return tcs.Task;
        }

        public Task DeleteAsync(string projectPath, string[] paths, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.Delete(projectPath, paths, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }

        public Task MoveAsync(string projectPath, string[] paths, string[] names, string targetPath, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.Move(projectPath, paths, names, targetPath, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }

        public Task RenameAsync(string projectPath, string[] paths, string[] oldNames, string[] names, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.Rename(projectPath, paths, oldNames, names, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }

        public Task CreateFoldersAsync(string projectPath, string[] paths, string[] names, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.Create(projectPath, paths, names, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }

        public Task<object> GetValueAsync(string projectPath, string key, Type type, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.GetValue(projectPath, key, type, (error, result) => TrySetResultOrException(tcs, ct, error, result));
            return tcs.Task;
        }

        public Task<object[]> GetValuesAsync(string projectPath, string searchPattern, Type type, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object[]>();
            m_storage.GetValues(projectPath, searchPattern, type, (error, result) => TrySetResultOrException(tcs, ct, error, result));
            return tcs.Task;
        }

        public Task SetValueAsync(string projectPath, string key, object persistentObject, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.SetValue(projectPath, key, persistentObject, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }

        public Task DeleteValueAsync(string projectPath, string key, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            m_storage.DeleteValue(projectPath, key, error => TrySetResultOrException(tcs, ct, error));
            return tcs.Task;
        }
    }
}

