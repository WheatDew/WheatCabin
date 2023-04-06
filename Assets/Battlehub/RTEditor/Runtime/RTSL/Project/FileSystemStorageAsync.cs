using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

namespace Battlehub.RTSL
{
    public class FileSystemStorageAsync<TID> : IStorageAsync<TID> where TID : IEquatable<TID>
    {
        private const string MetaExt = ".rtmeta";
        private const string PreviewExt = ".rtview";
        private const string KeyValueStorage = "Values";
        //private const string TempFolder = "Temp";
        private const string AssetsRootFolder = "Assets";
        private string m_rootPath;
        private ISerializer m_serializer;

        public FileSystemStorageAsync()
        {
            m_rootPath = Application.persistentDataPath + "/";
            m_serializer = IOC.Resolve<ISerializer>();

            Debug.LogFormat("RootPath : {0}", m_rootPath);

#pragma warning disable CS0618
            ZipConstants.DefaultCodePage = System.Text.Encoding.UTF8.CodePage;
#pragma warning restore CS0618
        }

        private string FullPath(string path)
        {
            return m_rootPath + path;
        }

        private string AssetsFolderPath(string path)
        {
            return m_rootPath + path + "/" + AssetsRootFolder;
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

        public async Task<string> GetRootPathAsync()
        {
            await Task.Yield();
            return m_rootPath;
        }
        public async Task SetRootPathAsync(string rootPath)
        {
            await Task.Yield();
            m_rootPath = rootPath;
        }

        public Task<ProjectInfo> CreateProjectAsync(string projectPath, CancellationToken ct)
        {
            string projectDir = FullPath(projectPath);
            if (Directory.Exists(projectDir))
            {
                throw new StorageException(Error.E_AlreadyExist, "Project with the same name already exists " + projectPath);
            }

            return Run(() =>
            {
                Directory.CreateDirectory(projectDir);
                ProjectInfo projectInfo = null;
                using (FileStream fs = File.OpenWrite(projectDir + "/Project.rtmeta"))
                {
                    projectInfo = new ProjectInfo
                    {
                        Name = Path.GetFileNameWithoutExtension(projectPath),
                        LastWriteTime = DateTime.UtcNow
                    };

                    m_serializer.Serialize(projectInfo, fs);
                }
                return projectInfo;
            });
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public Task CopyProjectAsync(string projectPath, string targetPath, CancellationToken ct)
        {
            return Run(() =>
            {
                string projectFullPath = FullPath(projectPath);
                string projectTargetPath = FullPath(targetPath);

                DirectoryInfo diSource = new DirectoryInfo(projectFullPath);
                DirectoryInfo diTarget = new DirectoryInfo(projectTargetPath);

                CopyAll(diSource, diTarget);

                ProjectInfo projectInfo;
                using (FileStream fs = File.OpenRead(projectTargetPath + "/Project.rtmeta"))
                {
                    projectInfo = m_serializer.Deserialize<ProjectInfo>(fs);
                }

                projectInfo.Name = Path.GetFileNameWithoutExtension(targetPath);
                projectInfo.LastWriteTime = File.GetLastWriteTimeUtc(projectTargetPath + "/Project.rtmeta");

                using (FileStream fs = File.OpenWrite(projectTargetPath + "/Project.rtmeta"))
                {
                    m_serializer.Serialize(projectInfo, fs);
                }
            });
        }

        public Task ExportProjectAsync(string projectPath, string targetPath, CancellationToken ct)
        {
            return Run(() =>
            {
                string projectFullPath = FullPath(projectPath);
                FastZip fastZip = new FastZip();
                //fastZip.CompressionLevel = Deflater.CompressionLevel.NO_COMPRESSION;
                fastZip.CreateZip(targetPath, projectFullPath, true, null);
            });
        }

        public Task ImportProjectAsync(string projectPath, string sourcePath, CancellationToken ct)
        {
            return Run(() =>
            {
                string projectFullPath = FullPath(projectPath);
                if (Directory.Exists(projectFullPath))
                {
                    throw new StorageException(Error.E_AlreadyExist, "Project with the same name already exists " + projectPath);
                }
                else
                {
                    FastZip fastZip = new FastZip();
                    fastZip.ExtractZip(sourcePath, projectFullPath, null);

                    ProjectInfo projectInfo;
                    using (FileStream fs = File.OpenRead(projectFullPath + "/Project.rtmeta"))
                    {
                        projectInfo = m_serializer.Deserialize<ProjectInfo>(fs);
                    }

                    projectInfo.Name = Path.GetFileNameWithoutExtension(projectPath);
                    projectInfo.LastWriteTime = File.GetLastWriteTimeUtc(projectFullPath + "/Project.rtmeta");

                    using (FileStream fs = File.OpenWrite(projectFullPath + "/Project.rtmeta"))
                    {
                        m_serializer.Serialize(projectInfo, fs);
                    }
                }
            });
        }
        public Task DeleteProjectAsync(string projectPath, CancellationToken ct)
        {
            return Run(() =>
            {
                string projectDir = FullPath(projectPath);
                if (Directory.Exists(projectDir))
                {
                    Directory.Delete(projectDir, true);
                }
            });
        }

        public Task<ProjectInfo[]> GetProjectsAsync(CancellationToken ct)
        {
            return Run(() =>
            {
                string projectsRoot = FullPath(string.Empty);
                string[] projectDirs = Directory.GetDirectories(projectsRoot);
                List<ProjectInfo> result = new List<ProjectInfo>();

                for (int i = 0; i < projectDirs.Length; ++i)
                {
                    string projectDir = projectDirs[i];
                    if (File.Exists(projectDir + "/Project.rtmeta"))
                    {
                        ProjectInfo projectInfo;
                        using (FileStream fs = File.OpenRead(projectDir + "/Project.rtmeta"))
                        {
                            projectInfo = m_serializer.Deserialize<ProjectInfo>(fs);
                        }
                        projectInfo.Name = Path.GetFileName(projectDir);
                        projectInfo.LastWriteTime = File.GetLastWriteTimeUtc(projectDir + "/Project.rtmeta");
                        result.Add(projectInfo);
                    }
                }

                return result.ToArray();
            });
        }

        public Task<(ProjectInfo, AssetBundleInfo[])> GetOrCreateProjectAsync(string projectPath, CancellationToken ct)
        {
            return Run(() =>
            {
                string projectDir = FullPath(projectPath);
                string projectMetaPath = projectDir + "/Project.rtmeta";
                ProjectInfo projectInfo;
                Error error = new Error();
                AssetBundleInfo[] result = new AssetBundleInfo[0];
                if (!File.Exists(projectMetaPath))
                {
                    Directory.CreateDirectory(projectDir);
                    using (FileStream fs = File.OpenWrite(projectDir + "/Project.rtmeta"))
                    {
                        projectInfo = new ProjectInfo
                        {
                            Name = projectPath,
                            LastWriteTime = DateTime.UtcNow
                        };

                        m_serializer.Serialize(projectInfo, fs);
                    }
                    return (projectInfo, result);
                }

                using (FileStream fs = File.OpenRead(projectMetaPath))
                {
                    projectInfo = m_serializer.Deserialize<ProjectInfo>(fs);
                }
                projectInfo.Name = projectPath;
                projectInfo.LastWriteTime = File.GetLastWriteTimeUtc(projectMetaPath);

                string[] files = Directory.GetFiles(projectDir).Where(fn => fn.EndsWith(".rtbundle")).ToArray();
                result = new AssetBundleInfo[files.Length];

                for (int i = 0; i < result.Length; ++i)
                {
                    using (FileStream fs = File.OpenRead(files[i]))
                    {
                        result[i] = m_serializer.Deserialize<AssetBundleInfo>(fs);
                    }
                }

                return (projectInfo, result);
            });
        }

        private static T LoadItem<T>(ISerializer serializer, string path) where T : ProjectItem, new()
        {
            T item = Load<T>(serializer, path);

            string fileNameWithoutMetaExt = Path.GetFileNameWithoutExtension(path);
            item.Name = Path.GetFileNameWithoutExtension(fileNameWithoutMetaExt);
            item.Ext = Path.GetExtension(fileNameWithoutMetaExt);

            return item;
        }

        private static T Load<T>(ISerializer serializer, string path) where T : new()
        {
            string metaFile = path;
            T item;
            if (File.Exists(metaFile))
            {
                FileStream fs = null;
                try
                {
                    fs = File.OpenRead(metaFile);
                    item = serializer.Deserialize<T>(fs);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Unable to read meta file: {0} -> got exception: {1} ", metaFile, e.ToString());
                    item = new T();
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Dispose();
                    }
                }
            }
            else
            {
                item = new T();
            }

            return item;
        }

        private void GetProjectTree(string path, ProjectItem parent)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            string[] dirs = Directory.GetDirectories(path);
            for (int i = 0; i < dirs.Length; ++i)
            {
                string dir = dirs[i];
                ProjectItem projectItem = LoadItem<ProjectItem>(m_serializer, dir + MetaExt);

                projectItem.Parent = parent;
                projectItem.Children = new List<ProjectItem>();
                parent.Children.Add(projectItem);

                GetProjectTree(dir, projectItem);
            }

            string[] files = Directory.GetFiles(path, "*" + MetaExt);
            for (int i = 0; i < files.Length; ++i)
            {
                string file = files[i];
                if (!File.Exists(file.Replace(MetaExt, string.Empty)))
                {
                    continue;
                }

                ProjectItem assetItem = LegacyAssetItem.ConvertToGenericAssetItem(LoadItem<ProjectItem>(m_serializer, file));
                assetItem.Parent = parent;
                parent.Children.Add(assetItem);
            }
        }

        public Task<ProjectItem> GetProjectTreeAsync(string projectPath, CancellationToken ct)
        {
            return Run(() =>
            {
                projectPath = AssetsFolderPath(projectPath);
                if (!Directory.Exists(projectPath))
                {
                    Directory.CreateDirectory(projectPath);
                }
                ProjectItem assets = new ProjectItem();
                assets.ItemID = 0;
                assets.Children = new List<ProjectItem>();
                assets.Name = "Assets";

                GetProjectTree(projectPath, assets);
                return assets;
            });
        }

        public Task<Preview<TID>[]> GetPreviewsAsync(string projectPath, string[] assetPath, CancellationToken ct)
        {
            return Run(() =>
            {
                projectPath = FullPath(projectPath);

                Preview<TID>[] result = new Preview<TID>[assetPath.Length];
                for (int i = 0; i < assetPath.Length; ++i)
                {
                    string path = projectPath + assetPath[i] + PreviewExt;
                    if (File.Exists(path))
                    {
                        result[i] = Load<Preview<TID>>(m_serializer, path);
                    }
                }

                return result.ToArray();
            });
        }

        public Task<Preview<TID>[][]> GetPreviewsPerFolderAsync(string projectPath, string[] folderPath, CancellationToken ct)
        {
            return GetPreviewsPerFolderAsync(projectPath, folderPath, null, ct);
        }

        public Task<Preview<TID>[][]> GetPreviewsPerFolderAsync(string projectPath, string[] folderPath, string searchPattern, CancellationToken ct)
        {
            return Run(() =>
            {
                projectPath = FullPath(projectPath);

                Preview<TID>[][] result = new Preview<TID>[folderPath.Length][];
                for (int i = 0; i < folderPath.Length; ++i)
                {
                    string path = projectPath + folderPath[i];
                    if (!Directory.Exists(path))
                    {
                        continue;
                    }

                    if (searchPattern == null)
                    {
                        searchPattern = string.Empty;
                    }
                    else
                    {
                        searchPattern = searchPattern.Replace("..", ".");
                    }

                    string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    searchPattern = r.Replace(searchPattern, "");

                    string[] files = Directory.GetFiles(path, string.Format("*{0}*{1}", searchPattern, PreviewExt));
                    Preview<TID>[] previews = new Preview<TID>[files.Length];
                    for (int j = 0; j < files.Length; ++j)
                    {
                        previews[j] = Load<Preview<TID>>(m_serializer, files[j]);
                    }

                    result[i] = previews;
                }
                return result;
            });
        }

        public Task SaveAsync(string projectPath, string[] folderPaths, ProjectItem[] projectItems, object[] persistentObjects, ProjectInfo projectInfo, bool previewOnly, CancellationToken ct)
        {
            return Run(() =>
            {
                if (!previewOnly)
                {
                    if (projectItems.Length != persistentObjects.Length)
                    {
                        throw new ArgumentException("projectItems");
                    }
                }

                if (projectItems.Length > folderPaths.Length)
                {
                    int l = folderPaths.Length;
                    Array.Resize(ref folderPaths, projectItems.Length);
                    for (int i = l; i < folderPaths.Length; ++i)
                    {
                        folderPaths[i] = folderPaths[l - 1];
                    }
                }

                projectPath = FullPath(projectPath);
                if (!Directory.Exists(projectPath))
                {
                    Directory.CreateDirectory(projectPath);
                }

                string projectInfoPath = projectPath + "/Project.rtmeta";
                for (int i = 0; i < projectItems.Length; ++i)
                {
                    string folderPath = folderPaths[i];
                    ProjectItem projectItem = projectItems[i];
                    try
                    {
                        string path = projectPath + folderPath;
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        string previewPath = path + "/" + projectItem.NameExt + PreviewExt;
                        File.Delete(previewPath);

                        byte[] previewData = projectItem.GetPreview();
                        if (previewData != null)
                        {
                            using (FileStream fs = File.Create(previewPath))
                            {
                                m_serializer.Serialize(new Preview<TID> { ID = GetID(projectItem), PreviewData = previewData }, fs);
                            }
                        }

                        if (!previewOnly)
                        {
                            File.Delete(path + "/" + projectItem.NameExt);

                            object persistentObject = persistentObjects[i];
                            if (persistentObject is PersistentRuntimeTextAsset<TID>)
                            {
                                PersistentRuntimeTextAsset<TID> textAsset = (PersistentRuntimeTextAsset<TID>)persistentObject;
                                File.WriteAllText(path + "/" + projectItem.NameExt, textAsset.Text);
                            }
                            else if (persistentObject is PersistentRuntimeBinaryAsset<TID>)
                            {
                                PersistentRuntimeBinaryAsset<TID> binAsset = (PersistentRuntimeBinaryAsset<TID>)persistentObject;
                                File.WriteAllBytes(path + "/" + projectItem.NameExt, binAsset.Data);
                            }
                            else
                            {
                                using (FileStream fs = File.Create(path + "/" + projectItem.NameExt))
                                {
                                    if (RTSLSettings.IsCustomSerializationEnabled && persistentObject is ICustomSerialization)
                                    {
                                        ICustomSerialization customSerialization = (ICustomSerialization)persistentObject;
                                        if (customSerialization.AllowStandardSerialization)
                                        {
                                            m_serializer.Serialize(persistentObject, fs);
                                        }

                                        projectItem.SetCustomDataOffset(fs.Position);
                                        using (BinaryWriter writer = new BinaryWriter(fs))
                                        {
                                            writer.Write(CustomSerializationHeader.Default);
                                            customSerialization.Serialize(fs, writer);
                                        }
                                    }
                                    else
                                    {
                                        m_serializer.Serialize(persistentObject, fs);
                                        projectItem.SetCustomDataOffset(fs.Position);
                                    }
                                }
                            }

                            File.Delete(path + "/" + projectItem.NameExt + MetaExt);
                            using (FileStream fs = File.Create(path + "/" + projectItem.NameExt + MetaExt))
                            {
                                m_serializer.Serialize(projectItem, fs);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new StorageException($"Unable to create asset: {projectItem.NameExt}", e);
                    }
                }

                File.Delete(projectInfoPath);
                using (FileStream fs = File.Create(projectInfoPath))
                {
                    m_serializer.Serialize(projectInfo, fs);
                }
            });
        }
        public Task SaveAsync(string projectPath, AssetBundleInfo assetBundleInfo, ProjectInfo projectInfo, CancellationToken ct)
        {
            return Run(() =>
            {
                projectPath = FullPath(projectPath);

                string projectInfoPath = projectPath + "/Project.rtmeta";
                string assetBundlePath = assetBundleInfo.UniqueName.Replace("/", "_").Replace("\\", "_");

                assetBundlePath += ".rtbundle";
                assetBundlePath = projectPath + "/" + assetBundlePath;

                using (FileStream fs = File.OpenWrite(assetBundlePath))
                {
                    m_serializer.Serialize(assetBundleInfo, fs);
                }

                using (FileStream fs = File.OpenWrite(projectInfoPath))
                {
                    m_serializer.Serialize(projectInfo, fs);
                }
            });
        }

        public Task<object[]> LoadAsync(string projectPath, ProjectItem[] assetItems, Type[] types, CancellationToken ct)
        {
            string[] assetPaths = assetItems.Select(item => item.ToString()).ToArray();
            long[] customDataOffsets = assetItems.Select(item => item.GetCustomDataOffset()).ToArray();
            return Run(() =>
            {
                object[] result = new object[assetPaths.Length];
                for (int i = 0; i < assetPaths.Length; ++i)
                {
                    ct.ThrowIfCancellationRequested();

                    string assetPath = assetPaths[i];
                    assetPath = FullPath(projectPath) + assetPath;
                    try
                    {
                        if (File.Exists(assetPath))
                        {
                            if (types[i] == typeof(PersistentRuntimeTextAsset<TID>))
                            {
                                PersistentRuntimeTextAsset<TID> textAsset = new PersistentRuntimeTextAsset<TID>();
                                textAsset.name = Path.GetFileName(assetPath);
                                textAsset.Text = File.ReadAllText(assetPath);
                                textAsset.Ext = Path.GetExtension(assetPath);
                                result[i] = textAsset;
                            }
                            else if (types[i] == typeof(PersistentRuntimeBinaryAsset<TID>))
                            {
                                PersistentRuntimeBinaryAsset<TID> binAsset = new PersistentRuntimeBinaryAsset<TID>();
                                binAsset.name = Path.GetFileName(assetPath);
                                binAsset.Data = File.ReadAllBytes(assetPath);
                                binAsset.Ext = Path.GetExtension(assetPath);
                                result[i] = binAsset;
                            }
                            else
                            {
                                using (FileStream fs = File.OpenRead(assetPath))
                                {
                                    long customDataOffset = customDataOffsets[i];
                                    if (customDataOffset == -1)
                                    {
                                        result[i] = (PersistentObject<TID>)m_serializer.Deserialize(fs, types[i]);
                                    }
                                    else
                                    {
                                        if (customDataOffset > 0)
                                        {
                                            result[i] = (PersistentObject<TID>)m_serializer.Deserialize(fs, types[i], customDataOffset);
                                        }
                                        else
                                        {
                                            result[i] = (PersistentObject<TID>)Activator.CreateInstance(types[i]);
                                        }

                                        if (fs.Position < fs.Length)
                                        {
                                            using (BinaryReader reader = new BinaryReader(fs))
                                            {
                                                CustomSerializationHeader header = reader.ReadHeader();
                                                if (header.IsValid)
                                                {
                                                    ICustomSerialization customSerialization = (ICustomSerialization)result[i];
                                                    customSerialization.Deserialize(fs, reader);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new StorageException(Error.E_NotFound, "Not Found");
                        }
                    }
                    catch (Exception e)
                    {
                        throw new StorageException(Error.E_Exception, $"Unable to load asset: {assetPath}", e);
                    }
                }
                return result;
            });
        }

        public Task<AssetBundleInfo> LoadAsync(string projectPath, string bundleName, CancellationToken ct)
        {
            return Run(() =>
            {
                string assetBundleInfoPath = bundleName.Replace("/", "_").Replace("\\", "_");
                assetBundleInfoPath += ".rtbundle";
                assetBundleInfoPath = FullPath(projectPath) + "/" + assetBundleInfoPath;

                if (!File.Exists(assetBundleInfoPath))
                {
                    throw new StorageException(Error.E_NotFound, "Not Found");
                }

                AssetBundleInfo result = null;
                using (FileStream fs = File.OpenRead(assetBundleInfoPath))
                {
                    result = m_serializer.Deserialize<AssetBundleInfo>(fs);
                }
                return result;
            });
        }

        public Task DeleteAsync(string projectPath, string[] paths, CancellationToken ct)
        {
            return Run(() =>
            {
                string fullPath = FullPath(projectPath);
                for (int i = 0; i < paths.Length; ++i)
                {
                    string path = fullPath + paths[i];
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        if (File.Exists(path + MetaExt))
                        {
                            File.Delete(path + MetaExt);
                        }
                        if (File.Exists(path + PreviewExt))
                        {
                            File.Delete(path + PreviewExt);
                        }
                    }
                    else if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
            });
        }

        public Task MoveAsync(string projectPath, string[] paths, string[] names, string targetPath, CancellationToken ct)
        {
            return Run(() =>
            {
                string fullPath = FullPath(projectPath);
                for (int i = 0; i < paths.Length; ++i)
                {
                    string path = fullPath + paths[i] + "/" + names[i];
                    if (File.Exists(path))
                    {
                        File.Move(path, fullPath + targetPath + "/" + names[i]);
                        if (File.Exists(path + MetaExt))
                        {
                            File.Move(path + MetaExt, fullPath + targetPath + "/" + names[i] + MetaExt);
                        }
                        if (File.Exists(path + PreviewExt))
                        {
                            File.Move(path + PreviewExt, fullPath + targetPath + "/" + names[i] + PreviewExt);
                        }
                    }
                    else if (Directory.Exists(path))
                    {
                        Directory.Move(path, fullPath + targetPath + "/" + names[i]);
                    }
                }
            });
        }

        public Task RenameAsync(string projectPath, string[] paths, string[] oldNames, string[] names, CancellationToken ct)
        {
            return Run(() =>
            {
                string fullPath = FullPath(projectPath);
                for (int i = 0; i < paths.Length; ++i)
                {
                    string path = fullPath + paths[i] + "/" + oldNames[i];
                    if (File.Exists(path))
                    {
                        File.Move(path, fullPath + paths[i] + "/" + names[i]);
                        if (File.Exists(path + MetaExt))
                        {
                            File.Move(path + MetaExt, fullPath + paths[i] + "/" + names[i] + MetaExt);
                        }
                        if (File.Exists(path + PreviewExt))
                        {
                            File.Move(path + PreviewExt, fullPath + paths[i] + "/" + names[i] + PreviewExt);
                        }
                    }
                    else if (Directory.Exists(path))
                    {
                        if (string.Equals(Path.GetFullPath(path), Path.GetFullPath(fullPath + paths[i] + "/" + names[i]), StringComparison.OrdinalIgnoreCase))
                        {
                            string tempDirName = Guid.NewGuid().ToString();

                            var dir = new DirectoryInfo(path);
                            dir.MoveTo(fullPath + "/" + tempDirName);
                            dir.MoveTo(fullPath + paths[i] + "/" + names[i]);
                        }
                        else
                        {
                            Directory.Move(path, fullPath + paths[i] + "/" + names[i]);
                        }
                    }
                }
            });
        }

        public async Task CreateFoldersAsync(string projectPath, string[] paths, string[] names, CancellationToken ct)
        {
            await Task.Yield();
            string fullPath = FullPath(projectPath);
            for (int i = 0; i < paths.Length; ++i)
            {
                string path = fullPath + paths[i] + "/" + names[i];
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        public Task<object> GetValueAsync(string projectPath, string key, Type type, CancellationToken ct)
        {
            return Run(() =>
            {
                string fullPath = FullPath(projectPath);
                string path = fullPath + "/" + KeyValueStorage;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = path + "/" + key;
                if (!File.Exists(path))
                {
                    throw new StorageException(Error.E_NotFound, $"Value {key} was not found");
                }
                object result = null;
                if (type == typeof(PersistentRuntimeTextAsset<TID>))
                {
                    PersistentRuntimeTextAsset<TID> textAsset = new PersistentRuntimeTextAsset<TID>();
                    textAsset.name = Path.GetFileName(path);
                    textAsset.Text = File.ReadAllText(path);
                    textAsset.Ext = Path.GetExtension(path);
                    result = textAsset;
                }
                else if (type == typeof(PersistentRuntimeBinaryAsset<TID>))
                {
                    PersistentRuntimeBinaryAsset<TID> binaryAsset = new PersistentRuntimeBinaryAsset<TID>();
                    binaryAsset.name = Path.GetFileName(path);
                    binaryAsset.Data = File.ReadAllBytes(path);
                    binaryAsset.Ext = Path.GetExtension(path);
                    result = binaryAsset;
                }
                else
                {
                    using (FileStream fs = File.OpenRead(path))
                    {
                        result = m_serializer.Deserialize(fs, type);
                    }
                }
                return result;
            });
        }

        public Task<object[]> GetValuesAsync(string projectPath, string searchPattern, Type type, CancellationToken ct)
        {
            return Run(() =>
            {
                string fullPath = FullPath(projectPath);
                string path = fullPath + "/" + KeyValueStorage;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string[] files = Directory.GetFiles(path, searchPattern);
                object[] result = new object[files.Length];

                for (int i = 0; i < files.Length; ++i)
                {
                    if (type == typeof(PersistentRuntimeTextAsset<TID>))
                    {
                        PersistentRuntimeTextAsset<TID> textAsset = new PersistentRuntimeTextAsset<TID>();
                        textAsset.name = Path.GetFileName(files[i]);
                        textAsset.Text = File.ReadAllText(files[i]);
                        textAsset.Ext = Path.GetExtension(files[i]);
                        result[i] = textAsset;
                    }
                    else if (type == typeof(PersistentRuntimeBinaryAsset<TID>))
                    {
                        PersistentRuntimeBinaryAsset<TID> binaryAsset = new PersistentRuntimeBinaryAsset<TID>();
                        binaryAsset.name = Path.GetFileName(files[i]);
                        binaryAsset.Data = File.ReadAllBytes(files[i]);
                        binaryAsset.Ext = Path.GetExtension(files[i]);
                        result[i] = binaryAsset;
                    }
                    else
                    {
                        using (FileStream fs = File.OpenRead(files[i]))
                        {
                            result[i] = m_serializer.Deserialize(fs, type);
                        }
                    }
                }

                return result;
            });
        }

        public Task SetValueAsync(string projectPath, string key, object persistentObject, CancellationToken ct)
        {
            return Run(() =>
            {
                string fullPath = FullPath(projectPath);
                string path = fullPath + "/" + KeyValueStorage;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = path + "/" + key;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (persistentObject is PersistentRuntimeTextAsset<TID>)
                {
                    PersistentRuntimeTextAsset<TID> textAsset = (PersistentRuntimeTextAsset<TID>)persistentObject;
                    File.WriteAllText(path, textAsset.Text);
                }
                else if (persistentObject is PersistentRuntimeBinaryAsset<TID>)
                {
                    PersistentRuntimeBinaryAsset<TID> binaryAsset = (PersistentRuntimeBinaryAsset<TID>)persistentObject;
                    File.WriteAllBytes(path, binaryAsset.Data);
                }
                else
                {
                    using (FileStream fs = File.Create(path))
                    {
                        m_serializer.Serialize(persistentObject, fs);
                    }
                    m_serializer.Serialize(persistentObject);
                }
            });
        }

        public Task DeleteValueAsync(string projectPath, string key, CancellationToken ct)
        {
            return Run(() =>
            {
                string fullPath = FullPath(projectPath);
                string path = fullPath + "/" + KeyValueStorage + "/" + key;
                File.Delete(path);
            });
        }

#if UNITY_WEBGL
        public async Task<T> Run<T>(Func<T> action)
        {
            await Task.Yield();
            return action();
        }

        public async Task Run(Action action)
        {
            await Task.Yield();
            action();
        }
#else
        public Task<T> Run<T>(Func<T> action)
        {
            return Task.Run(action);
        }

        public Task Run(Action action)
        {
            return Task.Run(action);
        }
#endif
    }

}

