using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class RegisterProjectViewOverrideExample : EditorExtension
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
            ProjectView projectView = window.GetComponentInChildren<ProjectView>(true);
            if (projectView != null)
            {
                projectView.ProjectTree.gameObject.AddComponent<ProjectTreeViewOverrideExample>();
                projectView.ProjectFolder.gameObject.AddComponent<ProjectFolderViewOverrideExample>();
            }
        }

        private void OnAfterLayout(IWindowManager wm)
        {
            Transform window = wm.GetWindow(RuntimeWindowType.Project.ToString());
            if (window != null)
            {
                ProjectView projectView = window.GetComponentInChildren<ProjectView>(true);
                if(projectView != null)
                {
                    projectView.ProjectTree.gameObject.AddComponent<ProjectTreeViewOverrideExample>();
                    projectView.ProjectFolder.gameObject.AddComponent<ProjectFolderViewOverrideExample>();
                }
            }
        }
    }

}
