using Battlehub.RTCommon;
using Battlehub.RTSL.Battlehub.SL2;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL
{
    public static class Menu
    {
        public static string TypeModelDll
        {
            get { return string.Format("{0}.dll", RTSLPath.TypeModel); }
        }

        private const string LinkFileTemplate = "<linker>{0}</linker>";

        private static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }


        [MenuItem("Tools/Runtime SaveLoad/Misc/Convert to asset")]
        public static async void ConvertToAsset()
        {
            //TODO: Update this method to use IProjectAsync

            const string ext = ";*.rtscene;*.rtprefab;*.rtmat;*.rttex;*.rtmesh;";

            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Unable to load runtime asset", "Unable to load runtime asset in play mode", "OK");
                return;
            }

            string path = EditorUtility.OpenFilePanel("Select Runtime Asset", Application.persistentDataPath, ext);
            if (path.Length != 0)
            {
                GameObject projGo = new GameObject();
                IAssetBundleLoader bundleLoader;
#if USE_GOOGLE_DRIVE
                if (File.Exists(Application.streamingAssetsPath + "/credentials.json"))
                {
                    bundleLoader = new GoogleDriveAssetBundleLoader();
                }
                else
#endif
                {
                    bundleLoader = new AssetBundleLoader();
                }

                IOC.Register(bundleLoader);

                ITypeMap typeMap = new TypeMap<long>();
                IOC.Register(typeMap);

                IUnityObjectFactory objFactory = new UnityObjectFactory();
                IOC.Register(objFactory);

                ISerializer serializer = new ProtobufSerializer();
                IOC.Register(serializer);

                IStorageAsync<long> storage = new FileSystemStorageAsync<long>();
                IOC.Register(storage);

                IRuntimeShaderUtil shaderUtil = new RuntimeShaderUtil();
                IOC.Register(shaderUtil);

                IMaterialUtil materialUtil = new StandardMaterialUtils();
                IOC.Register(materialUtil);

                IAssetDB<long> assetDB = new AssetDB<long>(obj =>
                {
                    const long instanceIDMask = 1L << 33;
                    return instanceIDMask | (0x00000000FFFFFFFFL & obj.GetInstanceID());
                });

                IOC.Register<IIDMap<long>>(assetDB);
                IOC.Register(assetDB);

                IProjectAsyncState<long> asyncState = new ProjectAsyncState<long>(projGo);
                IOC.Register(asyncState);

#pragma warning disable 0612
                IProject project = new ProjectAsyncWrapper<long>(new ProjectAsyncImpl(), new RuntimeSceneManager());
#pragma warning restore 0612

                DirectoryInfo root = new DirectoryInfo(Application.persistentDataPath);
                string rootPath = root.ToString().ToLower();

                DirectoryInfo parent = Directory.GetParent(path);
                while (true)
                {
                    if (parent == null)
                    {
                        EditorUtility.DisplayDialog("Unable to load runtime asset", "Project.rtmeta was not found", "OK");
                        UnityObject.DestroyImmediate(projGo);
                        ClearIOC();
                        return;
                    }

                    string projectPath = parent.FullName.ToLower();
                    if (rootPath == projectPath)
                    {
                        EditorUtility.DisplayDialog("Unable to load runtime asset", "Project.rtmeta was not found", "OK");
                        UnityObject.DestroyImmediate(projGo);
                        ClearIOC();
                        return;
                    }

                    string projectFile = Path.Combine(projectPath, "Project.rtmeta");
                    if (File.Exists(projectFile))
                    {
                        await storage.SetRootPathAsync(Path.GetDirectoryName(projectPath).Replace('\\', '/') + "/");

                        string projectName = Path.GetFileNameWithoutExtension(projectPath);
                        project.OpenProject(projectName, (error, result) =>
                        {
                            if (error.HasError)
                            {
                                EditorUtility.DisplayDialog("Unable to load runtime asset", "Project " + projectName + " can not be loaded", "OK");
                                UnityObject.DestroyImmediate(projGo);
                                ClearIOC();
                                return;
                            }

                            string relativePath = GetRelativePath(path, projectPath + "/Assets");
                            relativePath = relativePath.Replace('\\', '/');
                            AssetItem assetItem = (AssetItem)project.Root.Get(relativePath);

                            project.Load(new[] { assetItem }, (loadError, loadedObjects) =>
                            {
                                if (loadError.HasError)
                                {
                                    EditorUtility.DisplayDialog("Unable to load runtime asset", loadError.ToString(), "OK");
                                }
                                else
                                {
                                    if (!project.IsScene(assetItem))
                                    {
                                        foreach (UnityObject asset in assetDB.GetDynamicResources())
                                        {
                                            asset.hideFlags = HideFlags.None;
                                        }

                                        UnityObject loadedObj = loadedObjects[0];
                                        if (loadedObj == null)
                                        {
                                            EditorUtility.DisplayDialog("Unable to load runtime asset", assetItem.Name, "OK");
                                        }
                                        else
                                        {
                                            GameObject loadedGo = loadedObj as GameObject;
                                            if (loadedGo != null)
                                            {
                                                string savePath = EditorUtility.SaveFilePanelInProject("Save " + loadedGo.name, loadedGo.name, "prefab", "Save prefab");

                                                if (!string.IsNullOrWhiteSpace(savePath))
                                                {
                                                    PersistentRuntimePrefab<long> runtimePrefab = new PersistentRuntimePrefab<long>();

                                                    GetDepsFromContext ctx = new GetDepsFromContext();
                                                    runtimePrefab.GetDepsFrom(loadedGo, ctx);

                                                    SaveDependencies(ctx, typeMap, assetDB, Path.GetDirectoryName(savePath));
                                                    PrefabUtility.SaveAsPrefabAsset(loadedGo, savePath);
                                                }
                                            }
                                            else
                                            {
                                                string savePath = EditorUtility.SaveFilePanelInProject("Save " + loadedObj.name, loadedObj.name, "asset", "Save asset");
                                                if (!string.IsNullOrWhiteSpace(savePath))
                                                {
                                                    AssetDatabase.CreateAsset(loadedObj, savePath);
                                                }
                                            }
                                        }
                                    }
                                }

                                ClearIOC();
                                UnityObject.DestroyImmediate(projGo);

                            });
                        });

                        return;
                    }

                    parent = parent.Parent;
                }
            }
        }

        private static void ClearIOC()
        {
            IOC.Unregister<IAssetBundleLoader>();
            IOC.Unregister<ITypeMap>();
            IOC.Unregister<IUnityObjectFactory>();
            IOC.Unregister<ISerializer>();
            IOC.Unregister<IStorageAsync<long>>();
            IOC.Unregister<IRuntimeShaderUtil>();
            IOC.Unregister<IMaterialUtil>();
            IOC.Unregister<IAssetDB<long>>();
            IOC.Unregister<IIDMap<long>>();
            IOC.Unregister<IProjectAsyncState<long>>();
        }

        private static void SaveDependencies(GetDepsFromContext context, ITypeMap typeMap, IAssetDB<long> assetDB, string path)
        {
            object[] dependencies = context.Dependencies.ToArray();
            foreach (UnityObject dep in dependencies)
            {
                Type persistentType = typeMap.ToPersistentType(dep.GetType());
                if (persistentType != null)
                {
                    context.Dependencies.Clear();

                    IPersistentSurrogate persistentObject = (IPersistentSurrogate)Activator.CreateInstance(persistentType);
                    persistentObject.GetDepsFrom(dep, context);

                    SaveDependencies(context, typeMap, assetDB, path);
                }
            }

            foreach (UnityObject dep in dependencies)
            {
                if (dep is Component || dep is GameObject || !assetDB.IsDynamicResourceID(assetDB.ToID(dep)))
                {
                    continue;
                }

                string name = dep.name;
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = dep.GetType().Name;
                }

                string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", path, name));
                try
                {
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(dep)))
                    {
                        AssetDatabase.CreateAsset(dep, uniqueAssetPath);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

            }
        }

        [MenuItem("Tools/Runtime SaveLoad/Create RTSL", priority = 0)]
        private static void CreateRTSL()
        {
            GameObject rtsl = GameObject.Find("RTSLRTSL");
            if (rtsl != null)
            {
                Selection.activeGameObject = rtsl;
                EditorGUIUtility.PingObject(rtsl);
            }
            else
            {
                rtsl = new GameObject("RTSL");
                rtsl.AddComponent<RTSLDeps>();

                Undo.RegisterCreatedObjectUndo(rtsl, "Battlehub.RTSL.Create");
            }
        }

        //[MenuItem("Tools/Runtime SaveLoad/Persistent Classes/Create")]
        private static void CreatePersistentClasses()
        {
            PersistentClassMapperWindow.CreateOrPatchMappings();
            PersistentClassMapperWindow.CreatePersistentClasses();
        }

        [MenuItem("Tools/Runtime SaveLoad/Persistent Classes/Edit")]
        public static void EditPersistentClasses()
        {
            PersistentClassMapperWindow.ShowWindow();
        }

        [MenuItem("Tools/Runtime SaveLoad/Persistent Classes/Clean")]
        public static void CleanPersistentClasses()
        {
            if (EditorUtility.DisplayDialog("Clean", "Do you want to remove persistent classes and type model?", "Yes", "No"))
            {
                if (EditorUtility.DisplayDialog("Clean", "Do you want to remove files from " + RTSLPath.UserRoot + "/CustomImplementation ?", "Yes", "No"))
                {
                    try
                    {
                        AssetDatabase.StartAssetEditing();
                        AssetDatabase.DeleteAsset(RTSLPath.UserRoot + "/CustomImplementation");
                        AssetDatabase.DeleteAsset(RTSLPath.UserRoot + "/Mappings/Editor/FilePathStorage.prefab");
                        AssetDatabase.DeleteAsset(RTSLPath.UserRoot + "/Scripts");
                        AssetDatabase.DeleteAsset(RTSLPath.UserRoot + "/RTSLTypeModel.dll");
                    }
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                    }
                }
                else
                {
                    try
                    {
                        AssetDatabase.StartAssetEditing();
                        AssetDatabase.DeleteAsset(RTSLPath.UserRoot + "/Scripts");
                        AssetDatabase.DeleteAsset(RTSLPath.UserRoot + "/RTSLTypeModel.dll");
                    }
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                    }
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        private static void BuildTypeModel()
        {
            string dir = Path.GetFullPath(RTSLPath.UserRoot);
            if (!Directory.Exists(Path.GetFullPath(dir)))
            {
                AssetDatabase.CreateFolder(Path.GetDirectoryName(RTSLPath.UserRoot), Path.GetFileName(RTSLPath.UserRoot));
            }

            string[] typeModels = Directory.GetFiles(dir, RTSLPath.TypeModel + "*");
            foreach (string typeModel in typeModels)
            {
                File.Delete(typeModel);
            }

            RuntimeTypeModel model = ProtobufSerializer.CreateTypeModel();
            model.Compile(new RuntimeTypeModel.CompilerOptions() { OutputPath = TypeModelDll, TypeName = RTSLPath.TypeModel });

            string srcPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("Assets")) + TypeModelDll;
            string dstPath = Path.GetFullPath(RTSLPath.UserRoot + "/" + TypeModelDll);
            Debug.LogFormat("Done! Move {0} to {1} ...", srcPath, dstPath);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<assembly fullname=\"RTSLTypeModel\" preserve=\"all\"/>");

            File.WriteAllText(Path.GetFullPath(RTSLPath.UserRoot + "/link.xml"), string.Format(LinkFileTemplate, sb.ToString()));
            File.Delete(dstPath);
            File.Move(srcPath, dstPath);

            AssetDatabase.Refresh();
        }

        //[MenuItem("Tools/Runtime SaveLoad/Libraries/Collect Scene Dependencies")]
        private static void CreateAssetLibraryForActiveScene()
        {
            CreateBuiltInAssetLibrary();

            Scene scene = SceneManager.GetActiveScene();
            if (scene == null || string.IsNullOrEmpty(scene.name))
            {
                Debug.Log("Unable to create AssetLibrary for scene with no name");
                return;
            }

            int index;
            AssetLibraryAsset asset;
            AssetFolderInfo folder;
            HashSet<UnityObject> hs = ReadFromBuiltInAssetLibraries(out index, out asset, out folder);
            HashSet<UnityObject> hs2 = ReadFromSceneAssetLibraries(scene, out index, out asset, out folder);

            foreach (UnityObject obj in hs)
            {
                if (!hs2.Contains(obj))
                {
                    hs2.Add(obj);
                }
            }

            CreateAssetLibraryForScene(scene, index, asset, folder, hs2);
        }

        [MenuItem("Tools/Runtime SaveLoad/Update Libraries")]
        private static void UpdateLibraries()
        {
            CreateAssetLibraryForActiveScene();
            CreateBuiltInAssetLibrary();
            CreateShaderProfiles();
            CreateAssetLibrariesList();
        }

        //[MenuItem("Tools/Runtime SaveLoad/Libraries/Update Built-In Assets Library")]
        private static void CreateBuiltInAssetLibrary()
        {
            int index;
            AssetLibraryAsset asset;
            AssetFolderInfo folder;
            HashSet<UnityObject> hs = ReadFromBuiltInAssetLibraries(out index, out asset, out folder);
            CreateBuiltInAssetLibrary(index, asset, folder, hs);
        }

        //[MenuItem("Tools/Runtime SaveLoad/Libraries/Update Shader Profiles")]
        private static void CreateShaderProfiles()
        {
            RuntimeShaderProfilesGen.CreateProfile();

            /*
            RuntimeShaderProfilesAsset asset = RuntimeShaderProfilesGen.CreateProfile();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            */
        }

        //[MenuItem("Tools/Runtime SaveLoad/Libraries/Update Asset Libraries List")]
        private static void CreateAssetLibrariesList()
        {
            AssetLibrariesListAsset asset = AssetLibrariesListGen.UpdateList();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        [MenuItem("Assets/Create/Runtime SaveLoad/Runtime Asset Library", priority = 0)]
        private static void CreateAssetLibrary()
        {
            CreateAssetLibrary(null);
        }

        [MenuItem("Tools/Runtime SaveLoad/Create Runtime Asset Library", priority = 0)]
        private static void SelectPathAndCreateAssetLibrary()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Runtime Asset Library", "AssetLibrary", "asset", "Select the \"Resources\" folder to save the Runtime Asset Library");
            if (!string.IsNullOrEmpty(path))
            {
                CreateAssetLibrary(path);
            }
        }

        private static void CreateAssetLibrary(string path)
        {
            int identity = AssetLibrariesListGen.GetIdentity();
            if (string.IsNullOrEmpty(path))
            {
                path = AssetDatabase.GetAssetPath(Selection.activeObject);
                string name = "/AssetLibrary" + ((identity == 0) ? "" : identity.ToString());
                path = AssetDatabase.GenerateUniqueAssetPath(path + name + ".asset");
            }
            else
            {
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }

            if (!path.Contains("Resources"))
            {
                EditorUtility.DisplayDialog("Incorrect location selected", "Please select the \"Resources\" folder to save the Runtime Asset Library", "OK");
                SelectPathAndCreateAssetLibrary();
                return;
            }

            AssetLibraryAsset asset = ScriptableObject.CreateInstance<AssetLibraryAsset>();
            asset.Ordinal = identity;

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssetLibrariesListGen.UpdateList(identity + 1);
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (EditorPrefs.GetBool("RTSL_BuildAll"))
            {
                EditorPrefs.SetBool("RTSL_BuildAll", false);
                try
                {
                    AssetDatabase.StartAssetEditing();
                    EditorUtility.DisplayProgressBar("Build All", "Updating asset libraries and shader profiles", 0.33f);

                    CreateAssetLibraryForActiveScene();
                    Debug.Log("Asset Libraries Updated");

                    CreateAssetLibrariesList();
                    Debug.Log("Asset Libraries List Updated");

                    CreateShaderProfiles();
                    Debug.Log("Shader Profiles Updated");
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.StopAssetEditing();
                }

                try
                {
                    EditorUtility.DisplayProgressBar("Build", "Building Type Model...", 0.66f);
                    BuildTypeModel();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                }
            }
        }

        //[MenuItem("Tools/Runtime SaveLoad/Build All")]
        public static void BuildAll()
        {
            Selection.activeObject = null;
            EditorUtility.DisplayProgressBar("Build All", "Creating persistent classes", 0.0f);
            try
            {
                CreatePersistentClasses();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                Debug.Log("Persistent Classes Created");

                Selection.activeObject = AssetDatabase.LoadAssetAtPath(RTSLPath.UserRoot + "/" + RTSLPath.ScriptsAutoFolder, typeof(UnityObject));
                EditorGUIUtility.PingObject(Selection.activeObject);

                EditorPrefs.SetBool("RTSL_BuildAll", true);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static HashSet<UnityObject> ReadFromAssetLibraries(string[] guids, out int index, out AssetLibraryAsset asset, out AssetFolderInfo folder)
        {
            HashSet<UnityObject> hs = new HashSet<UnityObject>();

            List<AssetLibraryAsset> assetLibraries = new List<AssetLibraryAsset>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                AssetLibraryAsset assetLibrary = AssetDatabase.LoadAssetAtPath<AssetLibraryAsset>(path);
                if (assetLibrary != null)
                {
                    assetLibrary.Foreach(assetInfo =>
                    {
                        if (assetInfo.Object != null)
                        {
                            if (!hs.Contains(assetInfo.Object))
                            {
                                hs.Add(assetInfo.Object);
                            }

                            if (assetInfo.PrefabParts != null)
                            {
                                foreach (PrefabPartInfo prefabPart in assetInfo.PrefabParts)
                                {
                                    if (prefabPart.Object != null)
                                    {
                                        if (!hs.Contains(prefabPart.Object))
                                        {
                                            hs.Add(prefabPart.Object);
                                        }
                                    }
                                }
                            }
                        }
                        return true;
                    });

                    assetLibraries.Add(assetLibrary);
                }
            }

            if (assetLibraries.Count == 0)
            {
                asset = ScriptableObject.CreateInstance<AssetLibraryAsset>();
                index = 0;
            }
            else
            {
                asset = assetLibraries.OrderBy(a => a.AssetLibrary.Identity).FirstOrDefault();
                index = assetLibraries.Count - 1;
            }

            folder = asset.AssetLibrary.Folders.Where(f => f.depth == 0).First();
            if (folder.Assets == null)
            {
                folder.Assets = new List<AssetInfo>();
            }
            return hs;
        }

        private static void CreateAssetLibrary(object[] objects, string folderName, string assetLibraryName, int index, AssetLibraryAsset asset, AssetFolderInfo folder, HashSet<UnityObject> hs)
        {
            int identity = asset.AssetLibrary.Identity;

            foreach (object o in objects)
            {
                UnityObject obj = o as UnityObject;
                if (!obj)
                {
                    if (o != null)
                    {
                        Debug.Log(o.GetType() + " is not a UnityEngine.Object");
                    }
                    continue;
                }

                if (hs.Contains(obj))
                {
                    continue;
                }

                if (!AssetDatabase.Contains(obj))
                {
                    continue;
                }

                if (obj is GameObject)
                {
                    GameObject go = (GameObject)obj;
                    AssetInfo assetInfo = new AssetInfo(go.name, 0, identity);
                    assetInfo.Object = go;
                    hs.Add(go);

                    identity++;

                    List<PrefabPartInfo> prefabParts = new List<PrefabPartInfo>();
                    AssetLibraryAssetsGUI.CreatePefabParts(go, ref identity, prefabParts);
                    for (int i = prefabParts.Count - 1; i >= 0; --i)
                    {
                        PrefabPartInfo prefabPart = prefabParts[i];
                        if (hs.Contains(prefabPart.Object))
                        {
                            prefabParts.Remove(prefabPart);
                        }
                        else
                        {
                            hs.Add(prefabPart.Object);
                        }
                    }

                    if (prefabParts.Count >= AssetLibraryInfo.MAX_ASSETS - AssetLibraryInfo.INITIAL_ID)
                    {
                        EditorUtility.DisplayDialog("Unable Create AssetLibrary", string.Format("Max 'Indentity' value reached. 'Identity' ==  {0}", AssetLibraryInfo.MAX_ASSETS), "OK");
                        return;
                    }

                    if (identity >= AssetLibraryInfo.MAX_ASSETS)
                    {
                        SaveAssetLibrary(asset, folderName, assetLibraryName, index);
                        index++;

                        asset = ScriptableObject.CreateInstance<AssetLibraryAsset>();
                        folder = asset.AssetLibrary.Folders.Where(f => f.depth == 0).First();
                        if (folder.Assets == null)
                        {
                            folder.Assets = new List<AssetInfo>();
                        }
                        identity = asset.AssetLibrary.Identity;
                    }

                    assetInfo.PrefabParts = prefabParts;
                    asset.AssetLibrary.Identity = identity;
                    folder.Assets.Add(assetInfo);
                    assetInfo.Folder = folder;
                }
                else
                {
                    AssetInfo assetInfo = new AssetInfo(obj.name, 0, identity);
                    assetInfo.Object = obj;
                    identity++;

                    if (identity >= AssetLibraryInfo.MAX_ASSETS)
                    {
                        SaveAssetLibrary(asset, folderName, assetLibraryName, index);
                        index++;

                        asset = ScriptableObject.CreateInstance<AssetLibraryAsset>();
                        folder = asset.AssetLibrary.Folders.Where(f => f.depth == 0).First();
                        if (folder.Assets == null)
                        {
                            folder.Assets = new List<AssetInfo>();
                        }
                        identity = asset.AssetLibrary.Identity;
                    }

                    asset.AssetLibrary.Identity = identity;
                    folder.Assets.Add(assetInfo);
                    assetInfo.Folder = folder;
                }
            }

            SaveAssetLibrary(asset, folderName, assetLibraryName, index);
            index++;

            //Selection.activeObject = asset;
            //EditorGUIUtility.PingObject(asset);
        }

        private static void SaveAssetLibrary(AssetLibraryAsset asset, string folderName, string assetLibraryName, int index)
        {
            string dir = RTSLPath.UserRoot;

            if (!Directory.Exists(Path.GetFullPath(dir)))
            {
                Directory.CreateDirectory(Path.GetFullPath(dir));
            }

            if (!Directory.Exists(Path.GetFullPath(dir + "/" + RTSLPath.LibrariesFolder)))
            {
                AssetDatabase.CreateFolder(dir, RTSLPath.LibrariesFolder);
            }

            dir = dir + "/" + RTSLPath.LibrariesFolder;
            if (!Directory.Exists(Path.GetFullPath(dir + "/Resources")))
            {
                AssetDatabase.CreateFolder(dir, "Resources");
            }

            dir = dir + "/Resources";

            string[] folderNameParts = folderName.Split('/');
            for (int i = 0; i < folderNameParts.Length; ++i)
            {
                string folderNamePart = folderNameParts[i];

                if (!Directory.Exists(Path.GetFullPath(dir + "/" + folderNamePart)))
                {
                    AssetDatabase.CreateFolder(dir, folderNamePart);
                }

                dir = dir + "/" + folderNamePart;
            }

            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset)))
            {
                if (index == 0)
                {
                    AssetDatabase.CreateAsset(asset, dir + "/" + assetLibraryName + ".asset");
                }
                else
                {
                    AssetDatabase.CreateAsset(asset, dir + "/" + assetLibraryName + (index + 1) + ".asset");
                }
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        private static HashSet<UnityObject> ReadFromSceneAssetLibraries(Scene scene, out int index, out AssetLibraryAsset asset, out AssetFolderInfo folder)
        {
            if (!Directory.Exists(Path.GetFullPath(RTSLPath.UserRoot + "/" + RTSLPath.LibrariesFolder + "/Resources/Scenes/" + scene.name)))
            {
                return ReadFromAssetLibraries(new string[0], out index, out asset, out folder);
            }
            string[] guids = AssetDatabase.FindAssets("", new[] { RTSLPath.UserRoot + "/" + RTSLPath.LibrariesFolder + "/Resources/Scenes/" + scene.name });
            return ReadFromAssetLibraries(guids, out index, out asset, out folder);
        }

        private static void CreateAssetLibraryForScene(Scene scene, int index, AssetLibraryAsset asset, AssetFolderInfo folder, HashSet<UnityObject> hs)
        {
            TypeMap<long> typeMap = new TypeMap<long>();
            AssetDB assetDB = new AssetDB();
            RuntimeShaderUtil shaderUtil = new RuntimeShaderUtil(Resources.Load<RuntimeShaderProfilesAsset>("Lists/ShaderProfiles"));

            IOC.Register<ITypeMap>(typeMap);
            IOC.Register<IAssetDB>(assetDB);
            IOC.Register<IAssetDB<long>>(assetDB);
            IOC.Register<IRuntimeShaderUtil>(shaderUtil);

            PersistentRuntimeScene<long> rtScene = new PersistentRuntimeScene<long>();

            GetDepsFromContext ctx = new GetDepsFromContext();
            rtScene.GetDepsFrom(scene, ctx);

            Queue<UnityObject> depsQueue = new Queue<UnityObject>(ctx.Dependencies.OfType<UnityObject>());
            GetDepsFromContext getDepsCtx = new GetDepsFromContext();
            while (depsQueue.Count > 0)
            {
                UnityObject uo = depsQueue.Dequeue();
                if (!uo)
                {
                    continue;
                }

                Type persistentType = typeMap.ToPersistentType(uo.GetType());
                if (persistentType != null)
                {
                    getDepsCtx.Clear();

                    try
                    {
                        IPersistentSurrogate persistentObject = (IPersistentSurrogate)Activator.CreateInstance(persistentType);
                        persistentObject.GetDepsFrom(uo, getDepsCtx);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }

                    foreach (UnityObject dep in getDepsCtx.Dependencies)
                    {
                        if (!ctx.Dependencies.Contains(dep))
                        {
                            ctx.Dependencies.Add(dep);
                            depsQueue.Enqueue(dep);
                        }
                    }
                }
            }

            IOC.Unregister<IRuntimeShaderUtil>(shaderUtil);
            IOC.Unregister<ITypeMap>(typeMap);
            IOC.Unregister<IAssetDB>(assetDB);
            IOC.Unregister<IAssetDB<long>>(assetDB);

            CreateAssetLibrary(ctx.Dependencies.ToArray(), "Scenes/" + scene.name, "SceneAssetLibrary", index, asset, folder, hs);
        }

        private static HashSet<UnityObject> ReadFromBuiltInAssetLibraries(out int index, out AssetLibraryAsset asset, out AssetFolderInfo folder)
        {
            if (!Directory.Exists(Path.GetFullPath(RTSLPath.UserRoot + "/" + RTSLPath.LibrariesFolder + "/Resources/BuiltInAssets")))
            {
                return ReadFromAssetLibraries(new string[0], out index, out asset, out folder);
            }
            string[] guids = AssetDatabase.FindAssets("", new[] { RTSLPath.UserRoot + "/" + RTSLPath.LibrariesFolder + "/Resources/BuiltInAssets" });
            return ReadFromAssetLibraries(guids, out index, out asset, out folder);
        }

        private static void CreateBuiltInAssetLibrary(int index, AssetLibraryAsset asset, AssetFolderInfo folder, HashSet<UnityObject> hs)
        {
            Dictionary<string, Type> builtInExtra = new Dictionary<string, Type>
            {
                {  "Default-Line.mat", typeof(Material) },
                {  "Default-Material.mat", typeof(Material) },
                {  "Default-Particle.mat", typeof(Material) },
                {  "Default-Skybox.mat", typeof(Material) },
                {  "Sprites-Default.mat", typeof(Material) },
                {  "Sprites-Mask.mat", typeof(Material) },
                {  "UI/Skin/Background.psd", typeof(Sprite) },
                {  "UI/Skin/Checkmark.psd", typeof(Sprite) },
                {  "UI/Skin/DropdownArrow.psd", typeof(Sprite) },
                {  "UI/Skin/InputFieldBackground.psd", typeof(Sprite) },
                {  "UI/Skin/Knob.psd", typeof(Sprite) },
                {  "UI/Skin/UIMask.psd", typeof(Sprite) },
                {  "UI/Skin/UISprite.psd", typeof(Sprite) },
                {  "Default-Terrain-Standard.mat", typeof(Material) },
                {  "Default-Particle.psd", typeof(Texture2D) },
            };

            Dictionary<string, Type> builtIn = new Dictionary<string, Type>
            {
               { "New-Sphere.fbx", typeof(Mesh) },
               { "New-Capsule.fbx", typeof(Mesh) },
               { "New-Cylinder.fbx", typeof(Mesh) },
               { "Cube.fbx", typeof(Mesh) },
               { "New-Plane.fbx", typeof(Mesh) },
               { "Quad.fbx", typeof(Mesh) },
               { "Arial.ttf", typeof(Font) }
            };


            List<object> builtInAssets = new List<object>();
            foreach (KeyValuePair<string, Type> kvp in builtInExtra)
            {
                UnityObject obj = AssetDatabase.GetBuiltinExtraResource(kvp.Value, kvp.Key);
                if (obj != null)
                {
                    builtInAssets.Add(obj);
                }
            }

            foreach (KeyValuePair<string, Type> kvp in builtIn)
            {
                UnityObject obj = Resources.GetBuiltinResource(kvp.Value, kvp.Key);
                if (obj != null)
                {
                    builtInAssets.Add(obj);
                }
            }

            GameObject defaultTree = Resources.Load<GameObject>("Tree/RTT_DefaultTree");
            if (defaultTree != null)
            {
                builtInAssets.Add(defaultTree);
                Material barkMaterial = Resources.Load<Material>("Tree/Materials/RTT_DefaultTreeBark");
                if (barkMaterial != null)
                {
                    builtInAssets.Add(barkMaterial);
                }
                Material branchesMaterial = Resources.Load<Material>("Tree/Materials/RTT_DefaultTreeBranches");
                if (branchesMaterial != null)
                {
                    builtInAssets.Add(branchesMaterial);
                }
            }

            CreateAssetLibrary(builtInAssets.ToArray(), "BuiltInAssets", "BuiltInAssetLibrary", index, asset, folder, hs);
        }
    }
}

