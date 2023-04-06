using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class ToolsPanelOverrideExample : EditorExtension
    {
        [SerializeField]
        private Transform m_toolsPrefab = null;

        protected override void OnEditorCreated(object obj)
        {
            OverrideTools();
        }

        protected override void OnEditorExist()
        {
            OverrideTools();

            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            if (editor.IsOpened)
            {
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                if (m_toolsPrefab != null)
                {
                    wm.SetTools(Instantiate(m_toolsPrefab));
                }
                else
                {
                    wm.SetTools(null);
                }
            }
        }

        private void OverrideTools()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.OverrideTools(m_toolsPrefab);
        }
    }
}

