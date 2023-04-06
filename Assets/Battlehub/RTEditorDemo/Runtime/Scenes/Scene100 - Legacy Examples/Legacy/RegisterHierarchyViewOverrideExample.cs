using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class RegisterHierarchyViewOverrideExample : EditorExtension
    {
        private IWindowManager m_wm;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.WindowCreated += OnWindowCreated;
            m_wm.AfterLayout += OnAfterLayout;
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            if (m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
                m_wm.AfterLayout -= OnAfterLayout;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
                m_wm.AfterLayout -= OnAfterLayout;
            }
        }

        private void OnWindowCreated(Transform window)
        {
            RuntimeWindow runtimeWindow = window.GetComponentInChildren<RuntimeWindow>(true);
            if (runtimeWindow.WindowType == RuntimeWindowType.Hierarchy)
            {
                runtimeWindow.gameObject.AddComponent<HierarchyViewOverrideExample>();
            }
        }

        private void OnAfterLayout(IWindowManager wm)
        {
            Transform window = wm.GetWindow(RuntimeWindowType.Hierarchy.ToString());
            if (window != null)
            {
                RuntimeWindow runtimeWindow = window.GetComponentInChildren<RuntimeWindow>(true);
                runtimeWindow.gameObject.AddComponent<HierarchyViewOverrideExample>();
            }
        }
    }

}
