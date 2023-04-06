using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using UnityEngine.Battlehub.SL2;
using Battlehub.RTSL.Interface;
using Battlehub.RTCommon;
using System.Linq;
using System.Threading;
using Battlehub.Utils;
using ProtoBuf;
using ICSharpCode.SharpZipLib.Zip;
//using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Battlehub.RTSL
{
    public class FileSystemStorage<TID> : IStorage<TID>
    {
        private const string MetaExt = ".rtmeta";
        private const string PreviewExt = ".rtview";
        private const string KeyValueStorage = "Values";
        private const string TempFolder = "Temp";
        private const string AssetsRootFolder = "Assets";
     
        public string RootPath
        {
            get;
            set;
        }

        private string FullPath(string path)
        {
            return RootPath + path;
        }

        private string AssetsFolderPath(string path)
        {
            return RootPath + path + "/" + AssetsRootFolder;
        }

        public FileSystemStorage()
        {
            RootPath = Application.persistentDataPath + "/";

            string tempPath = RootPath + "/" + TempFolder;
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);
            Debug.LogFormat("RootPath : {0}", RootPath);
#pragma warning disable CS0618
            ZipConstants.DefaultCodePage = System.Text.Encoding.UTF8.CodePage;
#pragma warning restore CS0618
        }

        public void CreateProject(string projectName, StorageEventHandler<ProjectInfo> callback)
        {
            string projectDir = FullPath(projectName);
            if (Directory.Exists(projectDir))
            {
                Error error = new Error(Error.E_AlreadyExist);
                error.ErrorText = "Project with the same name already exists " + projectName;
                callback(error, null);
            }
            else
            {
                ISerializer serializer = IOC.Resolve<ISerializer>();
                Directory.CreateDirectory(projectDir);
                ProjectInfo projectInfo = null;
                using (FileStream fs = File.OpenWrite(projectDir + "/Project.rtmeta"))
                {
                    projectInfo = new ProjectInfo
                    {
                        Name = projectName,
                        LastWriteTime = DateTime.UtcNow
                    };

                    serializer.Serialize(projectInfo, fs);
                }
                callback(new Error(Error.OK), projectInfo);
            }
        }

        public void CopyProject(string projectPath, string targetPath, StorageEventHandler callback)
        {
            QueueUserWorkItem(() =>
            {
                try
                {
                    string projectFullPath = FullPath(projectPath);
                    string projectTargetPath = FullPath(targetPath);

                    DirectoryInfo diSource = new DirectoryInfo(projectFullPath);
                    DirectoryInfo diTarget = new DirectoryInfo(projectTargetPath);

                    CopyAll(diSource, diTarget);

                    ProjectInfo projectInfo;
                    using (FileStream fs = File.OpenRead(projectTargetPath + "/Project.rtmeta"))
                    {
                        projectInfo = Serializer.Deserialize<ProjectInfo>(fs);
                    }

                    projectInfo.Name = Path.GetFileNameWithoutExtension(targetPath);
                    projectInfo.LastWriteTime = File.GetLastWriteTimeUtc(projectTargetPath + "/Project.rtmeta");

                    using (FileStream fs = File.OpenWrite(projectTargetPath + "/Project.rtmeta"))
                    {
                        Serializer.Serialize(fs, projectInfo);
                    }

                    Callback(() => callback(new Error(Error.OK)));
                }
                catch (Exception e)
                {
                    Callback(() => callback(new Error(Error.E_Exception) { ErrorText = e.ToString() }));
                }
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

        public void ExportProject(string projectPath, string targetPath, StorageEventHandler callback)
        {
            QueueUserWorkItem(() =>
            {
                try
                {
                    string projectFullPath = FullPath(projectPath);

                    FastZip fastZip = new FastZip();
                   // fastZip.CompressionLevel = Deflater.CompressionLevel.NO_COMPRESSION;
                    fastZip.CreateZip(targetPath, projectFullPath, true, null);
                    

                    Callback(() =>
                    {
                        callback(new Error(Error.OK));
                    });
                }
                catch (Exception e)
                {
                    Callback(() =>
                    {
                        callback(new Error(Error.E_Exception) { ErrorText = e.ToString() });
                    });
                }
            });
        }

        public void ImportProject(string projectPath, string sourcePath, StorageEventHandler callback)
        {
            QueueUserWorkItem(() =>
            {
                try
                {
                    string projectFullPath = FullPath(projectPath);
                    if (Directory.Exists(projectFullPath))
                    {
                        callback(new Error(Error.E_AlreadyExist) { ErrorText = string.Format("Project {0} already exist", projectPath) });
                    }
                    else
                    {
                        FastZip fastZip = new FastZip();
                        fastZip.ExtractZip(sourcePath, projectFullPath, null);

                        ProjectInfo projectInfo;
                        using (FileStream fs = File.OpenRead(projectFullPath + "/Project.rtmeta"))
                        {
                            projectInfo = Serializer.Deserialize<ProjectInfo>(fs);
                        }

                        projectInfo.Name = Path.GetFileNameWithoutExtension(projectPath);
                        projectInfo.LastWriteTime = File.GetLastWriteTimeUtc(projectFullPath + "/Project.rtmeta");

                        using (FileStream fs = File.OpenWrite(projectFullPath + "/Project.rtmeta"))
                        {
                            Serializer.Serialize(fs, projectInfo);
                        }

                        Callback(() =>
                        {
                            callback(new Error(Error.OK));
                        });
                    }

                }
                catch (Exception e)
                {
                    Callback(() =>
                    {
                        callback(new Error(Error.E_Exception) { ErrorText = e.ToString() });
                    });
                }
            });
        }

        public void DeleteProject(string projectPath, StorageEventHandler callback)
        {
            QueueUserWorkItem(() =>
            {
                string projectDir = FullPath(projectPath);
                if (Directory.Exists(projectDir))
                {
                    Directory.Delete(projectDir, true);
                }
                Callback(() =>
                {
                    callback(new Error(Error.OK));
                });
            });

        }

        public void GetProjects(StorageEventHandler<ProjectInfo[]> callback)
        {
            string projectsRoot = FullPath(string.Empty);
            string[] projectDirs = Directory.GetDirectories(projectsRoot);
            List<ProjectInfo> result = new List<ProjectInfo>();
            ISerializer serializer = IOC.Resolve<ISerializer>();
            for (int i = 0; i < projectDirs.Length; ++i)
            {
                string projectDir = projectDirs[i];
                if (File.Exists(projectDir + "/Project.rtmeta"))
                {
                    ProjectInfo projectInfo;
                    using (FileStream fs = File.OpenRead(projectDir + "/Project.rtmeta"))
                    {
                        projectInfo = serializer.Deserialize<ProjectInfo>(fs);
                    }
                    projectInfo.Name = Path.GetFileName(projectDir);
                    projectInfo.LastWriteTime = File.GetLastWriteTimeUtc(projectDir + "/Project.rtmeta");
                    result.Add(projectInfo);
                }
            }
            callback(new Error(Error.OK), result.ToArray());
        }

        public void GetProject(string projectName, StorageEventHandler<ProjectInfo, AssetBundleInfo[]> callback)
        {
            string projectDir = FullPath(projectName);
            string projectPath = projectDir + "/Project.rtmeta";
            ProjectInfo projectInfo;
            Error error = new Error();
            ISerializer serializer = IOC.Resolve<ISerializer>();
            AssetBundleInfo[] result = new AssetBundleInfo[0];
            if (!File.Exists(projectPath))
            {
                Directory.CreateDirectory(projectDir);
                using (FileStream fs = File.OpenWrite(projectDir + "/Project.rtmeta"))
                {
                    projectInfo = new ProjectInfo
                    {
                        Name = projectName,
                        LastWriteTime = DateTime.UtcNow
                    };

                    serializer.Serialize(projectInfo, fs);
                }
            }
            else
            {
                try
                {
                    using (FileStream fs = File.OpenRead(projectPath))
                    {
                        projectInfo = serializer.Deserialize<ProjectInfo>(fs);
                    }
                    projectInfo.Name = projectName;
                    projectInfo.LastWriteTime = File.GetLastWriteTimeUtc(projectPath);

                    string[] files = Directory.GetFiles(projectDir).Where(fn => fn.EndsWith(".rtbundle")).ToArray();
                    result = new AssetBundleInfo[files.Length];

                    for (int i = 0; i < result.Length; ++i)
                    {
                        using (FileStream fs = File.OpenRead(files[i]))
                        {
                            result[i] = serializer.Deserialize<AssetBundleInfo>(fs);
                        }
                    }
                }
                catch (Exception e)
                {
                    projectInfo = new ProjectInfo();
                    error.ErrorCode = Error.E_Exception;
                    error.ErrorText = e.ToString();
                }
            }

            callback(error, projectInfo, result);
        }

        public void GetProjectTree(string projectPath, StorageEventHandler<ProjectItem> callback)
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

            callback(new Error(), assets);
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
                try
                {
                    using (FileStream fs = File.OpenRead(metaFile))
                    {
                        item = serializer.Deserialize<T>(fs);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Unable to read meta file: {0} -> got exception: {1} ", metaFile, e.ToString());
                    item = new T();
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

            ISerializer serializer = IOC.Resolve<ISerializer>();
            string[] dirs = Directory.GetDirectories(path);
            for (int i = 0; i < dirs.Length; ++i)
            {
                string dir = dirs[i];
                ProjectItem projectItem = LoadItem<ProjectItem>(serializer, dir + MetaExt);

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

                ProjectItem assetItem = LegacyAssetItem.ConvertToGenericAssetItem(LoadItem<ProjectItem>(serializer, file));
                assetItem.Parent = parent;
                parent.Children.Add(assetItem);
            }
        }

        public void GetPreviews(string projectPath, string[] assetPath, StorageEventHandler<Preview[]> callback)
        {
            projectPath = FullPath(projectPath);

            ISerializer serializer = IOC.Resolve<ISerializer>();
            Preview[] result = new Preview[assetPath.Length];
            for (int i = 0; i < assetPath.Length; ++i)
            {
                string path = projectPath + assetPath[i] + PreviewExt;
                if (File.Exists(path))
                {
                    result[i] = Load<Preview>(serializer, path);
                }
            }

            callback(new Error(), result);
        }

        public void GetPreviews(string projectPath, string[] folderPath, StorageEventHandler<Preview[][]> callback)
        {
            GetPreviews(projectPath, folderPath, string.Empty, callback);
        }

        public void GetPreviews(string projectPath, string[] folderPath, string searchPattern, StorageEventHandler<Preview[][]> callback)
        {
            projectPath = FullPath(projectPath);

            ISerializer serializer = IOC.Resolve<ISerializer>();
            Preview[][] result = new Preview[folderPath.Length][];
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

                string[] files = Directory.GetFiles(path, string.Format("*{0}*{1}", searchPattern, PreviewExt));
                Preview[] previews = new Preview[files.Length];
                for (int j = 0; j < files.Length; ++j)
                {
                    previews[j] = Load<Preview>(serializer, files[j]);
                }

                result[i] = previews;
            }

            callback(new Error(), result);
        }

        public void Save(string projectPath, string[] folderPaths, AssetItem[] assetItems, PersistentObject<TID>[] persistentObjects, ProjectInfo projectInfo, bool previewOnly, StorageEventHandler callback)
        {
            QueueUserWorkItem(() =>
            {
                if (!previewOnly)
                {
                    if (assetItems.Length != persistentObjects.Length)
                    {
                        throw new ArgumentException("assetItems");
                    }
                }

                if (assetItems.Length > folderPaths.Length)
                {
                    int l = folderPaths.Length;
                    Array.Resize(ref folderPaths, assetItems.Length);
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
                ISerializer serializer = IOC.Resolve<ISerializer>();
                Error error = new Error(Error.OK);
                for (int i = 0; i < assetItems.Length; ++i)
                {
                    string folderPath = folderPaths[i];
                    AssetItem assetItem = assetItems[i];

                    try
                    {
                        string path = projectPath + folderPath;
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        string previewPath = path + "/" + assetItem.NameExt + PreviewExt;
                        if (assetItem.Preview == null)
                        {
                            File.Delete(previewPath);
                        }
                        else
                        {
                            File.Delete(previewPath);
                            using (FileStream fs = File.Create(previewPath))
                            {
                                serializer.Serialize(assetItem.Preview, fs);
                            }
                        }

                        if (!previewOnly)
                        {
                            PersistentObject<TID> persistentObject = persistentObjects[i];
                          
                            File.Delete(path + "/" + assetItem.NameExt);

                            if (persistentObject is PersistentRuntimeTextAsset<TID>)
                            {
                                PersistentRuntimeTextAsset<TID> textAsset = (PersistentRuntimeTextAsset<TID>)persistentObject;
                                File.WriteAllText(path + "/" + assetItem.NameExt, textAsset.Text);
                            }
                            else if (persistentObject is PersistentRuntimeBinaryAsset<TID>)
                            {
                                PersistentRuntimeBinaryAsset<TID> binAsset = (PersistentRuntimeBinaryAsset<TID>)persistentObject;
                                File.WriteAllBytes(path + "/" + assetItem.NameExt, binAsset.Data);
                            }
                            else
                            {
                                using (FileStream fs = File.Create(path + "/" + assetItem.NameExt))
                                {
                                    if (RTSLSettings.IsCustomSerializationEnabled && persistentObject is ICustomSerialization)
                                    {
                                        ICustomSerialization customSerialization = (ICustomSerialization)persistentObject;
                                        if(customSerialization.AllowStandardSerialization)
                                        {
                                            serializer.Serialize(persistentObject, fs);
                                        }
                                        assetItem.CustomDataOffset = fs.Position;
                                        using (BinaryWriter writer = new BinaryWriter(fs))
                                        {
                                            writer.Write(CustomSerializationHeader.Default);
                                            customSerialization.Serialize(fs, writer);
                                        }
                                    }
                                    else
                                    {
                                        serializer.Serialize(persistentObject, fs);
                                        assetItem.CustomDataOffset = fs.Position;
                                    }   
                                }
                            }

                            File.Delete(path + "/" + assetItem.NameExt + MetaExt);
                            using (FileStream fs = File.Create(path + "/" + assetItem.NameExt + MetaExt))
                            {
                                serializer.Serialize(assetItem, fs);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Unable to create asset: {0} -> got exception: {1} ", assetItem.NameExt, e.ToString());
                        error.ErrorCode = Error.E_Exception;
                        error.ErrorText = e.ToString();
                        break;
                    }
                }

                File.Delete(projectInfoPath);
                using (FileStream fs = File.Create(projectInfoPath))
                {
                    serializer.Serialize(projectInfo, fs);
                }

                Callback(() => callback(error));
            });
        }

        public void Save(string projectPath, AssetBundleInfo assetBundleInfo, ProjectInfo projectInfo, StorageEventHandler callback)
        {
            QueueUserWorkItem(() =>
            {
                projectPath = FullPath(projectPath);
                string projectInfoPath = projectPath + "/Project.rtmeta";

                string assetBundlePath = assetBundleInfo.UniqueName.Replace("/", "_").Replace("\\", "_");
                assetBundlePath += ".rtbundle";
                assetBundlePath = projectPath + "/" + assetBundlePath;

                ISerializer serializer = IOC.Resolve<ISerializer>();

                using (FileStream fs = File.OpenWrite(assetBundlePath))
                {
                    serializer.Serialize(assetBundleInfo, fs);
                }

                using (FileStream fs = File.OpenWrite(projectInfoPath))
                {
                    serializer.Serialize(projectInfo, fs);
                }

                Callback(() => callback(new Error(Error.OK)));
            });
        }

        public void Load(string projectPath, AssetItem[] assetItems, Type[] types, StorageEventHandler<PersistentObject<TID>[]> callback)
        {
            string[] assetPaths = assetItems.Select(item => item.ToString()).ToArray();
            long[] customDataOffsets = assetItems.Select(item => item.CustomDataOffset).ToArray();
            QueueUserWorkItem(() =>
            {
                PersistentObject<TID>[] result = new PersistentObject<TID>[assetPaths.Length];
                for (int i = 0; i < assetPaths.Length; ++i)
                {
                    string assetPath = assetPaths[i];
                    assetPath = FullPath(projectPath) + assetPath;
                    ISerializer serializer = IOC.Resolve<ISerializer>();
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
                                    if(customDataOffset == -1)
                                    {
                                        result[i] = (PersistentObject<TID>)serializer.Deserialize(fs, types[i]);
                                    }
                                    else
                                    {
                                        if(customDataOffset > 0)
                                        {
                                            result[i] = (PersistentObject<TID>)serializer.Deserialize(fs, types[i], customDataOffset);
                                        }
                                        else
                                        {
                                            result[i] = (PersistentObject<TID>)Activator.CreateInstance(types[i]);
                                        }

                                        if(fs.Position < fs.Length)
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
                            Callback(() => callback(new Error(Error.E_NotFound), new PersistentObject<TID>[0]));
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Unable to load asset: {0} -> got exception: {1} ", assetPath, e.ToString());
                        Callback(() => callback(new Error(Error.E_Exception) { ErrorText = e.ToString() }, new PersistentObject<TID>[0]));
                        return;
                    }
                }

                Callback(() => callback(new Error(Error.OK), result));
            });
        }

        public void Load(string projectPath, string bundleName, StorageEventHandler<AssetBundleInfo> callback)
        {
            QueueUserWorkItem(() =>
            {
                string assetBundleInfoPath = bundleName.Replace("/", "_").Replace("\\", "_");
                assetBundleInfoPath += ".rtbundle";
                assetBundleInfoPath = FullPath(projectPath) + "/" + assetBundleInfoPath;

                ISerializer serializer = IOC.Resolve<ISerializer>();
                if (File.Exists(assetBundleInfoPath))
                {
                    AssetBundleInfo result = null;
                    using (FileStream fs = File.OpenRead(assetBundleInfoPath))
                    {
                        result = serializer.Deserialize<AssetBundleInfo>(fs);
                    }

                    Callback(() => callback(new Error(Error.OK), result));
                }
                else
                {
                    Callback(() => callback(new Error(Error.E_NotFound), null));
                }
            });
        }

        public void Delete(string projectPath, string[] paths, StorageEventHandler callback)
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

            callback(new Error(Error.OK));
        }

        public void Rename(string projectPath, string[] paths, string[] oldNames, string[] names, StorageEventHandler callback)
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

            callback(new Error(Error.OK));
        }

        public void Move(string projectPath, string[] paths, string[] names, string targetPath, StorageEventHandler callback)
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

            callback(new Error(Error.OK));
        }

        public void Create(string projectPath, string[] paths, string[] names, StorageEventHandler callback)
        {
            string fullPath = FullPath(projectPath);
            for (int i = 0; i < paths.Length; ++i)
            {
                string path = fullPath + paths[i] + "/" + names[i];
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            callback(new Error(Error.OK));
        }


        public void GetValue(string projectPath, string key, Type type, StorageEventHandler<object> callback)
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
                    ISerializer serializer = IOC.Resolve<ISerializer>();
                    using (FileStream fs = File.OpenRead(path))
                    {
                        result = serializer.Deserialize(fs, type);
                    }
                }

                callback(new Error(Error.OK), result);
            }
            else
            {
                callback(new Error(Error.E_NotFound), null);
                return;
            }
        }

        public void GetValues(string projectPath, string searchPattern, Type type, StorageEventHandler<object[]> callback)
        {
            QueueUserWorkItem(() =>
            {
                string fullPath = FullPath(projectPath);
                string path = fullPath + "/" + KeyValueStorage;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string[] files = Directory.GetFiles(path, searchPattern);
                object[] result = new object[files.Length];
                ISerializer serializer = IOC.Resolve<ISerializer>();
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
                            result[i] = serializer.Deserialize(fs, type);
                        }
                    }
                }

                Callback(() =>
                {
                    callback(Error.NoError, result);
                });
            });
        }

        public void SetValue(string projectPath, string key, object persistentObject, StorageEventHandler callback)
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
                ISerializer serializer = IOC.Resolve<ISerializer>();
                using (FileStream fs = File.Create(path))
                {
                    serializer.Serialize(persistentObject, fs);
                }
            }

            callback(new Error(Error.OK));
        }

        public void DeleteValue(string projectPath, string key, StorageEventHandler callback)
        {
            string fullPath = FullPath(projectPath);
            string path = fullPath + "/" + KeyValueStorage + "/" + key;
            File.Delete(path);
            callback(Error.NoError);
        }

        /*
        public void CreatePackage(string projectPath, string[] assetsPath, string packagePath, StorageEventHandler callback)
        {
            QueueUserWorkItem(() =>
            {
                try
                {
                    string fullProjectPath = FullPath(projectPath);
                    string tempPath = RootPath + "/" + TempFolder;
                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }

                    tempPath = tempPath + "/" + Path.GetFileNameWithoutExtension(packagePath) + "/" + AssetsRootFolder;

                    for (int i = 0; i < assetsPath.Length; ++i)
                    {
                        string sourceDataPath = fullProjectPath + "/" + assetsPath[i];

                        if (File.Exists(sourceDataPath) && File.Exists(sourceDataPath + MetaExt))
                        {
                            string targetDataPath = tempPath + "/" + assetsPath[i];
                            string targetDir = Path.GetDirectoryName(targetDataPath);
                            Directory.CreateDirectory(targetDir);

                            File.Copy(sourceDataPath, targetDataPath);
                            File.Copy(sourceDataPath + MetaExt, targetDataPath + MetaExt);

                            if (File.Exists(sourceDataPath + PreviewExt))
                            {
                                File.Copy(sourceDataPath + PreviewExt, targetDataPath + PreviewExt);
                            }
                        }
                    }

                    FastZip fastZip = new FastZip();
                    fastZip.CompressionLevel = Deflater.CompressionLevel.NO_COMPRESSION;
                    fastZip.CreateZip(packagePath, tempPath, true, null);
                    Directory.Delete(tempPath, true);

                    Callback(() => callback(Error.NoError));
                }
                catch (Exception e)
                {
                    Callback(() => callback(new Error(Error.E_Exception) { ErrorText = e.ToString() }));
                }
            });
        }
        
        
        public void OpenPackage(string projectPath, string packagePath, StorageEventHandler<AssetItem[]> callback)
        {
            QueueUserWorkItem(() =>
            {
                try
                {
                    if (!File.Exists(packagePath))
                    {
                        Callback(() => callback(new Error(Error.E_NotFound) { ErrorText = "File was not found: " + packagePath }, new AssetItem[0]));
                        return;
                    }

                    string tempPath = RootPath + "/" + TempFolder;
                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }

                    tempPath = tempPath + "/" + Path.GetFileNameWithoutExtension(packagePath);
                    Directory.CreateDirectory(tempPath);

                    FastZip fastZip = new FastZip();
                    fastZip.ExtractZip(packagePath, tempPath, null);

                    tempPath = tempPath + "/" + AssetsRootFolder;

                    ProjectItem assets = new ProjectItem();
                    assets.ItemID = 0;
                    assets.Children = new List<ProjectItem>();
                    assets.Name = "Assets";

                    GetProjectTree(tempPath, assets);

                    AssetItem[] assetItems = assets.Flatten(true).Cast<AssetItem>().ToArray();

                    Callback(() => callback(Error.NoError, assetItems));
                }
                catch (Exception e)
                {
                    Callback(() => callback(new Error(Error.E_Exception) { ErrorText = e.ToString() }, new AssetItem[0]));
                }
            });
        }
        
        */
        public void QueueUserWorkItem(Action action)
        {
#if UNITY_WEBGL
            action();
#else
            if (Dispatcher.Current != null)
            {
                ThreadPool.QueueUserWorkItem(arg =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        Dispatcher.BeginInvoke(() => Debug.LogError(e));
                    }

                });
            }
            else
            {
                action();
            }
#endif
        }

        public void Callback(Action callback)
        {
            Dispatcher.BeginInvoke(callback);
        }
    }
}
