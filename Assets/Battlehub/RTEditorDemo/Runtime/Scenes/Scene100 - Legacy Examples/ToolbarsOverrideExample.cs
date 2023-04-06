using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class ToolbarsOverrideExample : EditorExtension
    {
        [SerializeField]
        private Transform m_leftBarPrefab = null;

        [SerializeField]
        private Transform m_rightBarPrefab = null;

        [SerializeField]
        private Transform m_topBarPrefab = null;

        [SerializeField]
        private Transform m_bottomBarPrefab = null;

        protected override void OnEditorExist()
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            if (editor.IsOpened)
            {
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                wm.SetTopBar(m_topBarPrefab != null ? Instantiate(m_topBarPrefab) : null);
                wm.SetBottomBar(m_bottomBarPrefab != null ? Instantiate(m_bottomBarPrefab) : null);
                wm.SetLeftBar(m_leftBarPrefab != null ? Instantiate(m_leftBarPrefab) : null);
                wm.SetRightBar(m_rightBarPrefab != null ? Instantiate(m_rightBarPrefab) : null);
            }
        }
    }
}

