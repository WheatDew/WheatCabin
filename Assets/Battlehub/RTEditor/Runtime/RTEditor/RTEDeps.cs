using Battlehub.RTCommon;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [DefaultExecutionOrder(-100)]
    public class RTEDeps : MonoBehaviour
    {
        private IRuntimeConsole m_console;
        private IResourcePreviewUtility m_resourcePreview;
        private IRTEAppearance m_rteAppearance;
        private IRuntimeHandlesComponent m_runtimeHandlesComponent;
        private IRuntimeEditor m_rte;
        private IWindowManager m_windowManager;
        private IContextMenu m_contextMenu;
        private ISettingsComponent m_settingsComponent;
        
        protected virtual IResourcePreviewUtility ResourcePreview
        {
            get
            {
                IResourcePreviewUtility resourcePreviewUtility = FindObjectOfType<ResourcePreviewUtility>();
                if (resourcePreviewUtility == null)
                {
                    resourcePreviewUtility = gameObject.AddComponent<ResourcePreviewUtility>();
                }
                return resourcePreviewUtility;
            }
        }

        protected virtual IRTEAppearance RTEAppearance
        {
            get
            {
                IRTEAppearance rteAppearance = FindObjectOfType<RTEAppearance>();
                if (rteAppearance == null)
                {
                    rteAppearance = gameObject.AddComponent<RTEAppearance>();
                }
                return rteAppearance;
            }
        }

        protected virtual IRuntimeHandlesComponent RuntimeHandlesComponent
        {
            get
            {
                IRuntimeHandlesComponent runtimeHandles = FindObjectOfType<RuntimeHandlesComponent>();
                if(runtimeHandles == null)
                {
                    runtimeHandles = gameObject.AddComponent<RuntimeHandlesComponent>();
                }
                return runtimeHandles;
            }
        }

        protected virtual IRuntimeEditor RTE
        {
            get
            {
                IRuntimeEditor rte = FindObjectOfType<RuntimeEditor>();
                if (rte == null)
                {
                    rte = gameObject.AddComponent<RuntimeEditor>();
                }
                return rte;
            }
        }

        protected virtual IWindowManager WindowManager
        {
            get
            {
                IWindowManager windowManager = FindObjectOfType<WindowManager>();
                if (windowManager == null)
                {
                    windowManager = gameObject.AddComponent<WindowManager>();
                }
                return windowManager;
            }
        }

        protected virtual IRuntimeConsole RuntimeConsole
        {
            get
            {
                IRuntimeConsole console = FindObjectOfType<RuntimeConsole>();
                if (console == null)
                {
                    console = gameObject.AddComponent<RuntimeConsole>();
                }
                return console;
            }
        }

#pragma warning disable CS0618
        protected virtual IGameObjectCmd GameObjectCmd
        {
            get
            {
                return null;
            }
        }

        protected virtual IEditCmd EditCmd
        {
            get
            {
                return null;
            }
        }
#pragma warning restore CS0618

        protected virtual IContextMenu ContextMenu
        {
            get
            {
                return FindObjectOfType<ContextMenu>();
            }
        }

        protected virtual ISettingsComponent SettingsComponent
        {
            get
            {
                return FindObjectOfType<SettingsComponent>();
            }
        }


        private void Awake()
        {
            if (m_instance != null)
            {
                Debug.LogWarning("AnotherInstance of RTEDeps exists");
            }
            m_instance = this;
            AwakeOverride();
        }

        protected virtual void AwakeOverride()
        {
            m_rte = RTE;
            IOC.Register<IRTE>(m_rte);
            IOC.Register(m_rte);

            m_resourcePreview = ResourcePreview;
            m_rteAppearance = RTEAppearance;
            m_windowManager = WindowManager;
            m_console = RuntimeConsole;
            
            m_contextMenu = ContextMenu;
            m_runtimeHandlesComponent = RuntimeHandlesComponent;
            m_settingsComponent = SettingsComponent;
        }

        private void OnDestroy()
        {
            if (m_instance == this)
            {
                m_instance = null;
            }

            OnDestroyOverride();

            IOC.Unregister<IRTE>(m_rte);
            IOC.Unregister(m_rte);

            m_resourcePreview = null;
            m_rteAppearance = null;
            m_windowManager = null;
            m_console = null;
            
            m_contextMenu = null;
            m_runtimeHandlesComponent = null;
            m_settingsComponent = null;
        }

        protected virtual void OnDestroyOverride()
        {

        }

        private static RTEDeps m_instance;
        private static RTEDeps Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<RTEDeps>();
                }
                return m_instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            IOC.RegisterFallback(() => Instance != null ? Instance.m_console : null);
            IOC.RegisterFallback(() => Instance != null ? Instance.m_resourcePreview : null);
            IOC.RegisterFallback(() => Instance != null ? Instance.m_rteAppearance : null);
            IOC.RegisterFallback(() => Instance != null ? Instance.m_windowManager : null);
            IOC.RegisterFallback(() => Instance != null ? Instance.m_contextMenu : null);
            IOC.RegisterFallback(() => Instance != null ? Instance.m_runtimeHandlesComponent : null);
            IOC.RegisterFallback(() => Instance != null ? Instance.m_settingsComponent : null);
        }
    }
}

