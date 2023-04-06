using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls.MenuControl;
using System;
using UnityEngine;
namespace Battlehub.RTBuilder
{
    [MenuDefinition(-1)]
    public class ProBuilderInit : EditorExtension
    {
        [SerializeField]
        private GameObject m_proBuilderWindow = null;

        [SerializeField]
        private GameObject[] m_prefabs = null;

        [Serializable]
        public class CustomTool
        {
            [SerializeField]
            private GameObject m_prefab = null;
            public GameObject Prefab
            {
                get { return m_prefab; }
            }

            [SerializeField]
            private GameObject m_uiPrefab;
            public GameObject UIPrefab
            {
                get { return m_uiPrefab; }
            }
        }

        [SerializeField]
        private CustomTool[] m_customTools = null;

        protected override void OnInit()
        {
            base.OnInit();
        
            IRTE editor = IOC.Resolve<IRTE>();

            IProBuilderTool proBuilderTool = IOC.Resolve<IProBuilderTool>();
            if (proBuilderTool == null)
            {
                GameObject proBuilderToolGO = new GameObject("ProBuilderTool");
                proBuilderToolGO.transform.SetParent(editor.Root, false);
                proBuilderTool = proBuilderToolGO.AddComponent<ProBuilderTool>();
                proBuilderToolGO.AddComponent<ManualUVEditor>();
            }

            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            for (int i = 0; i < m_customTools.Length; ++i)
            {
                CustomTool tool = m_customTools[i];

                IProBuilderCustomTool customTool = tool.Prefab.GetComponentInChildren<IProBuilderCustomTool>();
                string name = customTool != null ? customTool.Name : tool.Prefab.name;

                proBuilderTool.RegisterCustomTool(name, tool.Prefab, tool.UIPrefab);

                if(appearance != null)
                {
                    appearance.RegisterPrefab(tool.UIPrefab);
                }
            }

            Register();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            IProBuilderTool proBuilderTool = IOC.Resolve<IProBuilderTool>();
            if(proBuilderTool != null)
            {
                for (int i = 0; i < m_customTools.Length; ++i)
                {
                    CustomTool tool = m_customTools[i];

                    IProBuilderCustomTool customTool = tool.Prefab.GetComponentInChildren<IProBuilderCustomTool>();
                    string name = customTool != null ? customTool.Name : tool.Prefab.name;

                    proBuilderTool.UnregisterCustomTool(name);
                }
            }

            //TODO: Unregister IProBuilderTool
            //TODO: Unregister windows
            //TODO: Unregister prefabs
        }


        private void Register()
        {
            ILocalization lc = IOC.Resolve<ILocalization>();
            lc.LoadStringResources("RTBuilder.StringResources");

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            if (m_proBuilderWindow != null)
            {
                RegisterWindow(wm, "ProBuilder", lc.GetString("ID_RTBuilder_WM_Header_Builder", "Builder"),
                    Resources.Load<Sprite>("hammer-24"), m_proBuilderWindow, false);

                appearance.RegisterPrefab(m_proBuilderWindow);
            }

            foreach(GameObject prefab in m_prefabs)
            {
                if(prefab != null)
                {
                    appearance.RegisterPrefab(prefab);
                }
            }
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

        [MenuCommand("MenuWindow/ID_RTBuilder_WM_Header_Builder", "", priority: 100)]
        public void OpenProBuilder()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow("ProBuilder");
        }
    }
}


