using Battlehub.RTSL.Interface;
using System;
using UnityEngine.Battlehub.SL2;

namespace Battlehub.RTSL
{
    public delegate void StorageEventHandler(Error error);
    public delegate void StorageEventHandler<T>(Error error, T data);
    public delegate void StorageEventHandler<T, T2>(Error error, T data, T2 data2);

    public interface IStorage<TID>
    {
        string RootPath
        {
            get;
            set;
        }

        void CreateProject(string projectPath, StorageEventHandler<ProjectInfo> callback);
        void CopyProject(string projectPath, string targetPath, StorageEventHandler callback);
        void ExportProject(string projectPath, string targetPath, StorageEventHandler callback);
        void ImportProject(string projectPath, string sourcePath, StorageEventHandler callback);
        void DeleteProject(string projectPath, StorageEventHandler callback);
        void GetProjects(StorageEventHandler<ProjectInfo[]> callback);
        void GetProject(string projectPath, StorageEventHandler<ProjectInfo, AssetBundleInfo[]> callback);
        void GetProjectTree(string projectPath, StorageEventHandler<ProjectItem> callback);
        void GetPreviews(string projectPath, string[] assetPath, StorageEventHandler<Preview[]> callback);
        void GetPreviews(string projectPath, string[] folderPath, StorageEventHandler<Preview[][]> callback);
        void GetPreviews(string projectPath, string[] folderPath, string searchPattern, StorageEventHandler<Preview[][]> callback);
        void Save(string projectPath, string[] folderPaths, AssetItem[] assetItems, PersistentObject<TID>[] persistentObjects, ProjectInfo projectInfo, bool previewOnly, StorageEventHandler callback);
        void Save(string projectPath, AssetBundleInfo assetBundleInfo, ProjectInfo project, StorageEventHandler callback);
        void Load(string projectPath, AssetItem[] assetItems, Type[] types, StorageEventHandler<PersistentObject<TID>[]> callback);
        void Load(string projectPath, string bundleName, StorageEventHandler<AssetBundleInfo> callback);
        void Delete(string projectPath, string[] paths, StorageEventHandler callback);
        void Move(string projectPath, string[] paths, string[] names, string targetPath, StorageEventHandler callback);
        void Rename(string projectPath, string[] paths, string[] oldNames, string[] names, StorageEventHandler callback);
        void Create(string projectPath, string[] paths, string[] names, StorageEventHandler callback);
        void GetValue(string projectPath, string key, Type type, StorageEventHandler<object> callback);
        void GetValues(string projectPath, string searchPattern, Type type, StorageEventHandler<object[]> callback);
        void SetValue(string projectPath, string key, object persistentObject, StorageEventHandler callback);
        void DeleteValue(string projectPath, string key, StorageEventHandler callback);
    }
}