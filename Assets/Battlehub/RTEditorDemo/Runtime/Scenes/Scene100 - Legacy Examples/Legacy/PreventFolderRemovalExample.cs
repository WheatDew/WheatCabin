using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Battlehub.RTEditor.Demo
{
    [Obsolete]
    public class PreventFolderRemovalExample : EditorExtension
    {
        private IWindowManager m_wm;
        private IProjectFolder m_projectFolder;
        private IProjectTree m_projectTree;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            m_wm = IOC.Resolve<IWindowManager>();
            Subscribe();
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            Unsubscribe();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Unsubscribe();
        }

        private void Subscribe()
        {
            if(m_wm != null)
            {
                m_wm.AfterLayout += OnAfterLayout;
                m_wm.WindowCreated += OnWindowCreated;
            }
        }

        private void Unsubscribe()
        {
            if (m_wm != null)
            {
                m_wm.AfterLayout -= OnAfterLayout;
                m_wm.WindowCreated -= OnWindowCreated;
            }
        }

        private void OnAfterLayout(IWindowManager obj)
        {
            Init();
        }

        private void OnWindowCreated(UnityEngine.Transform obj)
        {
            Init();
        }

        private async void Init()
        {
            await Task.Yield();

            IProjectFolder projectFolder = IOC.Resolve<IProjectFolder>();
            IProjectTree projectTree = IOC.Resolve<IProjectTree>();
            if (m_projectFolder != projectFolder)
            {
                m_projectFolder = projectFolder;

                if (m_projectFolder != null)
                {
                    m_projectFolder.ItemsDeleting += OnItemsDeleting;
                    m_projectFolder.ItemDataBinding += OnItemDataBinding;
                    m_projectFolder.Destroyed += OnDestroyed;
                }
            }

            if (m_projectTree != projectTree)
            {
                m_projectTree = projectTree;

                if (m_projectTree != null)
                {
                    m_projectTree.ItemsDeleting += OnItemsDeleting;
                    m_projectTree.ItemDataBinding += OnItemDataBinding;
                    m_projectTree.Destroyed += OnDestroyed;
                }
            }
        }

        private void OnDestroyed(object sender, System.EventArgs e)
        {
            IProjectFolder projectFolder = sender as IProjectFolder;
            if(projectFolder != null)
            {
                projectFolder.ItemsDeleting -= OnItemsDeleting;
                projectFolder.ItemDataBinding -= OnItemDataBinding;
                projectFolder.Destroyed -= OnDestroyed;
            }

            IProjectTree projectTree = sender as IProjectTree;
            if (projectTree != null)
            {
                projectTree.ItemsDeleting -= OnItemsDeleting;
                projectTree.ItemDataBinding -= OnItemDataBinding;
                projectTree.Destroyed -= OnDestroyed;
            }
        }

        private void OnItemDataBinding(object sender, UIControls.ItemDataBindingArgs e)
        {
            ProjectItem item = e.Item as ProjectItem;
            if(item != null && item.Name.StartsWith("DoNotRemove"))
            {
                e.CanEdit = false;
            }
        }

        private void OnItemsDeleting(object sender, ProjectTreeCancelEventArgs e)
        {
            if(e.ProjectItems != null && e.ProjectItems.Any(item => item.IsFolder && item.Name.StartsWith("DoNotRemove")))
            {
                e.Cancel = true;
                IOC.Resolve<IWindowManager>().MessageBox("Warning", "Unable to remove folder");
            }
        }
    }

}
