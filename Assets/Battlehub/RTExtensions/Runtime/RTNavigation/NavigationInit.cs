using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls.MenuControl;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Battlehub.RTNavigation
{
    [MenuDefinition]
    public class NavigationInit : EditorExtension
    {
        [SerializeField]
        private GameObject m_prefab = null;
        private ISpriteGizmoManager m_gizmoManager;
        private IEditorsMap m_editorsMap;
        
        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            ILocalization lc = IOC.Resolve<ILocalization>();
            lc.LoadStringResources("RTNavigation.StringResources");

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            Sprite icon = Resources.Load<Sprite>("RTN_Header");
            bool isDialog = false;
            RegisterWindow(wm, "NavigationView", "ID_RTNavigation_WM_Header_Navigation", icon, m_prefab, isDialog);

            m_editorsMap = IOC.Resolve<IEditorsMap>();
            
            TryToAddEditorMapping(typeof(NavMeshSurface));
            TryToAddEditorMapping(typeof(NavMeshModifierVolume));
            TryToAddEditorMapping(typeof(NavMeshLink));
            TryToAddEditorMapping(typeof(NavMeshModifier));
            TryToAddEditorMapping(typeof(NavMeshObstacle));
            TryToAddEditorMapping(typeof(NavMeshAgent));
            TryToAddEditorMapping(typeof(NavMeshDebugController));

            m_gizmoManager = IOC.Resolve<ISpriteGizmoManager>();

            Material surfaceIcon = Resources.Load<Material>("RTNavigationMeshSurfaceIcon");
            Material volumeIcon = Resources.Load<Material>("RTNavigationMeshModifierVolumeIcon");
            Material linkIcon = Resources.Load<Material>("RTNavigationMeshLinkIcon");
            
            m_gizmoManager.Register(typeof(NavMeshSurface), surfaceIcon);
            m_gizmoManager.Register(typeof(NavMeshModifierVolume), volumeIcon);
            m_gizmoManager.Register(typeof(NavMeshLink), linkIcon);
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            Cleanup();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cleanup();
        }

        private void Cleanup()
        {
            if(m_gizmoManager != null)
            {
                m_gizmoManager.Unregister(typeof(NavMeshSurface));
                m_gizmoManager.Unregister(typeof(NavMeshModifierVolume));
                m_gizmoManager.Unregister(typeof(NavMeshLink));
            }

            if(m_editorsMap != null)
            {
                m_editorsMap.RemoveMapping(typeof(NavMeshSurface));
                m_editorsMap.RemoveMapping(typeof(NavMeshModifierVolume));
                m_editorsMap.RemoveMapping(typeof(NavMeshLink));
                m_editorsMap.RemoveMapping(typeof(NavMeshModifier));
                m_editorsMap.RemoveMapping(typeof(NavMeshObstacle));
                m_editorsMap.RemoveMapping(typeof(NavMeshAgent));
                m_editorsMap.RemoveMapping(typeof(NavMeshDebugController));
            }
        }

        private void TryToAddEditorMapping(Type type)
        {
            if(m_editorsMap.HasMapping(type))
            {
                return;
            }

            m_editorsMap.AddMapping(type, typeof(ComponentEditor), true, false);
        }

        private void RegisterWindow(IWindowManager wm, string typeName, string header, Sprite icon, GameObject prefab, bool isDialog)
        {
            wm.RegisterWindow(new CustomWindowDescriptor
            {
                IsDialog = isDialog,
                TypeName = typeName,
                Descriptor = new WindowDescriptor
                {
                    Header = header,
                    Icon = icon,
                    MaxWindows = 1,
                    ContentPrefab = prefab
                }
            });
        }

        [MenuCommand("MenuWindow/ID_RTNavigation_WM_Header_Navigation", "", priority: 100)]
        public void Open()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow("NavigationView");
        }
    }
}
