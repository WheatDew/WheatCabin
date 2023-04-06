using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using System;
using UnityEngine;

namespace Battlehub.RTSL
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(RTSLIgnore))]
    public class RTSLDeps : RTSLDeps<long>
    {
        private IAssetDB<long> m_assetDB;
        protected override IAssetDB<long> AssetDB
        {
            get
            {
                if (m_assetDB == null)
                {
                    m_assetDB = new AssetDB<long>(obj =>
                    {
                        const long instanceIDMask = 1L << 33;
                        return instanceIDMask | (0x00000000FFFFFFFFL & obj.GetInstanceID());
                    });
                }
                return m_assetDB;
            }
        }

        private IAssetDB m_legacyAssetDB;
        private IAssetDB LegacyAssetDB
        {
            get
            {
                if (m_legacyAssetDB == null)
                {
                    m_legacyAssetDB = new AssetDB();
                }
                return m_legacyAssetDB;
            }
        }

        private ProjectAsyncImpl m_projectAsync;
        protected override IProjectAsync ProjectAsync
        {
            get
            {
                if(m_projectAsync == null)
                {
                    m_projectAsync = new ProjectAsyncImpl();
                }
                return m_projectAsync;
            }
        }


        private Func<IAssetDB> m_registerAssetDB;
        private Func<IIDMap> m_registerIDMap;
        

        protected override void AwakeOverride()
        {
            base.AwakeOverride();

            m_registerAssetDB = () => LegacyAssetDB;
            m_registerIDMap = () => LegacyAssetDB;

            IOC.RegisterFallback(m_registerAssetDB);
            IOC.RegisterFallback(m_registerIDMap);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();


            if (m_projectAsync != null)
            {
                m_projectAsync.Dispose();
            }
         

            IOC.UnregisterFallback(m_registerAssetDB);
            IOC.UnregisterFallback(m_registerIDMap);

            Init();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (!RTSLSettings.RuntimeInitializeOnLoad)
            {
                return;
            }

            IProjectAsync projectAsync = null;
            IOC.RegisterFallback(() =>
            {
                if (!RTSLSettings.RuntimeInitializeOnLoad)
                {
                    return null;
                }

                if (projectAsync == null)
                {
                    RTSLDeps deps = FindObjectOfType<RTSLDeps>();
                    if (deps == null)
                    {
                        GameObject go = new GameObject("RTSL");
                        go.transform.SetSiblingIndex(0);
                        deps = go.AddComponent<RTSLDeps>();
                        projectAsync = deps.ProjectAsync;
                    }
                }
                return projectAsync;
            });

            IProject project = null;
            IOC.RegisterFallback(() =>
            {
                if (!RTSLSettings.RuntimeInitializeOnLoad)
                {
                    return null;
                }

                if (project == null)
                {
                    RTSLDeps deps = FindObjectOfType<RTSLDeps>();
                    if (deps == null)
                    {
                        GameObject go = new GameObject("RTSL");
                        go.transform.SetSiblingIndex(0);
                        deps = go.AddComponent<RTSLDeps>();
                        project = deps.Project;
                    }
                }
                return project;
            });
        }
    }

    public abstract class RTSLDeps<TID> : MonoBehaviour where TID : IEquatable<TID>
    {
        private IAssetBundleLoader m_assetBundleLoader;
        protected virtual IAssetBundleLoader AssetBundleLoader
        {
            get
            {
                if (m_assetBundleLoader == null)
                {
                    m_assetBundleLoader = new AssetBundleLoader();
                }
                return m_assetBundleLoader;

            }
        }

        private IMaterialUtil materialUtil;
        protected virtual IMaterialUtil MaterialUtil
        {
            get
            {
                if (materialUtil == null)
                {
                    materialUtil = new StandardMaterialUtils();
                }
                return materialUtil;
            }
        }

        private IRuntimeShaderUtil m_shaderUtil;
        protected virtual IRuntimeShaderUtil ShaderUtil
        {
            get
            {
                if (m_shaderUtil == null)
                {
                    m_shaderUtil = new RuntimeShaderUtil();
                }
                return m_shaderUtil;
            }
        }

        private ITypeMap m_typeMap;
        protected virtual ITypeMap TypeMap
        {
            get
            {
                if (m_typeMap == null)
                {
                    m_typeMap = new TypeMap<TID>();
                }

                return m_typeMap;
            }
        }

        private IUnityObjectFactory m_objectFactory;
        protected virtual IUnityObjectFactory ObjectFactory
        {
            get
            {
                if (m_objectFactory == null)
                {
                    m_objectFactory = new UnityObjectFactory();
                }

                return m_objectFactory;
            }
        }

        private ISerializer m_serializer;
        protected virtual ISerializer Serializer
        {
            get
            {
                if (m_serializer == null)
                {
                    m_serializer = new ProtobufSerializer();
                }
                return m_serializer;
            }
        }

        private IStorage<TID> m_storage;
        protected virtual IStorage<TID> Storage
        {
            get
            {
                if (m_storage == null)
                {
                    m_storage = new FileSystemStorage<TID>();
                }
                return m_storage;
            }
        }

        private IStorageAsync<TID> m_storageAsync;
        protected virtual IStorageAsync<TID> StorageAsync
        {
            get
            {
                if (m_storageAsync == null)
                {
                    m_storageAsync = new FileSystemStorageAsync<TID>();
                }
                return m_storageAsync;
            }
        }

        protected virtual IIDGenerator<TID> IDGen
        {
            get;
        }

        private IPlayerPrefsStorage m_playerPrefs;
        protected virtual IPlayerPrefsStorage PlayerPrefs
        {
            get
            {
                if (m_playerPrefs == null)
                {
                    m_playerPrefs = new PlayerPrefsStorage();
                }
                return m_playerPrefs;
            }
        }

        protected abstract IAssetDB<TID> AssetDB
        {
            get;
        }

        private IProjectAsyncState<TID> m_projectAsyncState;
        internal virtual IProjectAsyncState<TID> ProjectAsyncState
        {
            get
            {
                if(m_projectAsyncState == null)
                {
                    m_projectAsyncState = new ProjectAsyncState<TID>(gameObject);
                }
                return m_projectAsyncState;
            }
        }

        private IProject m_project;
        protected virtual IProject Project
        {
            get
            {
                if (m_project == null)
                {
                    ProjectAsyncWithAssetLibraries<TID> projectAsync = ProjectAsync as ProjectAsyncWithAssetLibraries<TID>;
                    if (projectAsync != null)
                    {
                        m_project = new ProjectAsyncWrapper<TID>(projectAsync, SceneManager);
                    }
                }
                return m_project;
            }
        }

        private IRuntimeSceneManager m_sceneManager;
        protected virtual IRuntimeSceneManager SceneManager
        {
            get 
            {
                if (m_sceneManager == null)
                {
                    m_sceneManager = new RuntimeSceneManager();
                }
                return m_sceneManager; 
            }
        }

        private ProjectAsyncImpl<TID> m_projectAsync;
        protected virtual IProjectAsync ProjectAsync
        {
            get
            {
                if (m_projectAsync == null)
                {
                    m_projectAsync = new ProjectAsyncImpl<TID>();
                }
                return m_projectAsync;
            }
        }

        private Func<IAssetBundleLoader> m_registerBundleLoader;
        private Func<ITypeMap> m_registerTypeMap;
        private Func<IUnityObjectFactory> m_registerObjectFactory;
        private Func<ISerializer> m_registerSerializer;
        private Func<IStorage<TID>> m_registerStorage;
        private Func<IStorageAsync<TID>> m_registerStorageAsync;
        private Func<IIDGenerator<TID>> m_registerIDGen;
        private Func<IAssetDB<TID>> m_registerAssetDB;
        private Func<IIDMap<TID>> m_registerIDMap;
        private Func<IProjectAsyncState<TID>> m_registerProjectAsyncState;
        private Func<IProject> m_registerProject;
        private Func<IRuntimeSceneManager> m_registerSceneManager;
        private Func<IProjectAsync> m_registerProjectAsync;
        private Func<IProjectAsyncWithAssetLibraries> m_registerProjectAsyncWithAssetLibraries;
        
        private Func<IRuntimeShaderUtil> m_registerShaderUtil;
        private Func<IMaterialUtil> m_registerMaterialUtil;
        private Func<IPlayerPrefsStorage> m_registerPlayerPrefs;

        protected virtual void Awake()
        {
            AwakeOverride();
        }

        protected virtual void AwakeOverride()
        {
            if (gameObject.GetComponent<Dispatcher>() == null)
            {
                gameObject.AddComponent<Dispatcher>();
            }

            m_registerBundleLoader = () => AssetBundleLoader;
            m_registerTypeMap = () => TypeMap;
            m_registerObjectFactory = () => ObjectFactory;
            m_registerSerializer = () => Serializer;
            m_registerStorage = () => Storage;
            m_registerStorageAsync = () => StorageAsync;
            m_registerIDGen = () => IDGen;
            m_registerAssetDB = () => AssetDB;
            m_registerIDMap = () => AssetDB;

            m_registerProjectAsyncState = () => ProjectAsyncState;
            m_registerSceneManager = () => SceneManager;
            m_registerProjectAsync = () => ProjectAsync;
            m_registerProjectAsyncWithAssetLibraries = () => ProjectAsync as IProjectAsyncWithAssetLibraries;
            m_registerProject = () => Project;
            
            m_registerShaderUtil = () => ShaderUtil;
            m_registerMaterialUtil = () => MaterialUtil;
            m_registerPlayerPrefs = () => PlayerPrefs;
     
            IOC.UnregisterFallback<IProject>();
            IOC.UnregisterFallback<IProjectAsync>();

            IOC.RegisterFallback(m_registerBundleLoader);
            IOC.RegisterFallback(m_registerTypeMap);
            IOC.RegisterFallback(m_registerObjectFactory);
            IOC.RegisterFallback(m_registerSerializer);
            IOC.RegisterFallback(m_registerStorage);
            IOC.RegisterFallback(m_registerStorageAsync);
            IOC.RegisterFallback(m_registerIDGen);
            IOC.RegisterFallback(m_registerAssetDB);
            IOC.RegisterFallback(m_registerIDMap);
            IOC.RegisterFallback(m_registerProjectAsyncState);
            IOC.RegisterFallback(m_registerSceneManager);
            IOC.RegisterFallback(m_registerProjectAsync);
            IOC.RegisterFallback(m_registerProjectAsyncWithAssetLibraries);
            IOC.RegisterFallback(m_registerProject);
            IOC.RegisterFallback(m_registerShaderUtil);
            IOC.RegisterFallback(m_registerMaterialUtil);
            IOC.RegisterFallback(m_registerPlayerPrefs);
        }

        protected virtual void OnDestroy()
        {
            OnDestroyOverride();
        }

        protected virtual void OnDestroyOverride()
        {
            if (m_projectAsync != null)
            {
                m_projectAsync.Dispose();
            }

            if(m_projectAsyncState != null)
            {
                m_projectAsyncState.Dispose();
            }
            
            IOC.UnregisterFallback(m_registerBundleLoader);
            IOC.UnregisterFallback(m_registerTypeMap);
            IOC.UnregisterFallback(m_registerObjectFactory);
            IOC.UnregisterFallback(m_registerSerializer);
            IOC.UnregisterFallback(m_registerStorage);
            IOC.UnregisterFallback(m_registerStorageAsync);
            IOC.UnregisterFallback(m_registerIDGen);
            IOC.UnregisterFallback(m_registerAssetDB);
            IOC.UnregisterFallback(m_registerIDMap);
            IOC.UnregisterFallback(m_registerProjectAsyncState);
            IOC.UnregisterFallback(m_registerProject);
            IOC.UnregisterFallback(m_registerSceneManager);
            IOC.UnregisterFallback(m_registerProjectAsync);
            IOC.UnregisterFallback(m_registerProjectAsyncWithAssetLibraries);
            IOC.UnregisterFallback(m_registerShaderUtil);
            IOC.UnregisterFallback(m_registerMaterialUtil);
            IOC.UnregisterFallback(m_registerPlayerPrefs);
        }
    }
}

