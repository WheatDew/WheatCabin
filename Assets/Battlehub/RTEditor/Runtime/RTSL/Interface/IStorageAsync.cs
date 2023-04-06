using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Battlehub.SL2;

namespace Battlehub.RTSL.Interface
{
    [Serializable]
    public class StorageException : Exception
    {
        public int ErrorCode { get; private set; } = Error.E_Exception;
        public StorageException() { }
        public StorageException(string message) : base(message) { }
        public StorageException(string message, Exception inner) : base(message, inner) { }
        public StorageException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
        public StorageException(int errorCode, string message, Exception inner) : base(message, inner)
        {
            ErrorCode = errorCode;
        }

        protected StorageException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public interface IIDGenerator<TID>
    {
        Task<TID[]> GenerateAsync(int count, CancellationToken ct);
    }

    public interface IStorageAsync<TID> where TID : IEquatable<TID>
    {
        ProjectItem CreateAssetItem(TID id, Guid typeGuid, string name, string ext, ProjectItem parent);
        
        TID GetID(ProjectItem projectItem);
        void SetID(ProjectItem projectItem, TID id);
        TID[] GetDependencyIDs(ProjectItem projectItem);
        void SetDependencyIDs(ProjectItem projectItem, TID[] ids);
        TID[] GetEmbeddedIDs(ProjectItem projectItem);
        void SetEmbeddedIDs(ProjectItem projectItem, TID[] ids);
        
        Task<string> GetRootPathAsync();
        Task SetRootPathAsync(string rootPath);
        
        Task<ProjectInfo[]> GetProjectsAsync(CancellationToken ct);
        Task<ProjectInfo> CreateProjectAsync(string projectPath, CancellationToken ct);
        Task<(ProjectInfo, AssetBundleInfo[])> GetOrCreateProjectAsync(string projectPath, CancellationToken ct);
        Task CopyProjectAsync(string projectPath, string targetPath, CancellationToken ct);
        Task ExportProjectAsync(string projectPath, string targetPath, CancellationToken ct);
        Task ImportProjectAsync(string projectPath, string sourcePath, CancellationToken ct);
        Task DeleteProjectAsync(string projectPath, CancellationToken ct);
        
        Task<ProjectItem> GetProjectTreeAsync(string projectPath, CancellationToken ct);
        Task<Preview<TID>[]> GetPreviewsAsync(string projectPath, string[] assetPath, CancellationToken ct);
        Task<Preview<TID>[][]> GetPreviewsPerFolderAsync(string projectPath, string[] folderPath, CancellationToken ct);
        Task<Preview<TID>[][]> GetPreviewsPerFolderAsync(string projectPath, string[] folderPath, string searchPattern, CancellationToken ct);

        Task SaveAsync(string projectPath, string[] folderPaths, ProjectItem[] projectItems, object[] persistentObjects, ProjectInfo projectInfo, bool previewOnly, CancellationToken ct);
        Task SaveAsync(string projectPath, AssetBundleInfo assetBundleInfo, ProjectInfo projectInfo, CancellationToken ct);

        Task<object[]> LoadAsync(string projectPath, ProjectItem[] projectItems, Type[] types, CancellationToken ct);
        Task<AssetBundleInfo> LoadAsync(string projectPath, string bundleName, CancellationToken ct);

        Task MoveAsync(string projectPath, string[] paths, string[] names, string targetPath, CancellationToken ct);
        Task DeleteAsync(string projectPath, string[] paths, CancellationToken ct);
        Task RenameAsync(string projectPath, string[] paths, string[] oldNames, string[] names, CancellationToken ct);
        Task CreateFoldersAsync(string projectPath, string[] paths, string[] names, CancellationToken ct);

        Task<object> GetValueAsync(string projectPath, string key, Type type, CancellationToken ct);
        Task<object[]> GetValuesAsync(string projectPath, string searchPattern, Type type, CancellationToken ct);
        Task SetValueAsync(string projectPath, string key, object persistentObject, CancellationToken ct);
        Task DeleteValueAsync(string projectPath, string key, CancellationToken ct);
    }
}
