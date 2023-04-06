using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.MeshDeformer3
{
    [MenuDefinition(-1)]
    public class MeshDeformerInit : EditorExtension
    {
        [SerializeField]
        private GameObject m_meshDeformerWindow = null;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            if(FindObjectOfType<MeshDeformerTool>() == null)
            {
                gameObject.AddComponent<MeshDeformerTool>();
            }

            Register();
        }

        private void Register()
        {
            ILocalization lc = IOC.Resolve<ILocalization>();
            lc.LoadStringResources("RTDeformer.StringResources");

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            if (m_meshDeformerWindow != null)
            {
                RegisterWindow(wm, "MeshDeformer", lc.GetString("ID_RTDeformer_WM_Header_Deformer", "Deformer"),
                    Resources.Load<Sprite>("meshdeformer-24"), m_meshDeformerWindow, false);

                IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
                appearance.ApplyColors(m_meshDeformerWindow);
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

        [MenuCommand("MenuWindow/ID_RTDeformer_WM_Header_Deformer", "", priority: 100)]
        public void OpenMeshDeformer()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow("MeshDeformer");
        }
    }
}


