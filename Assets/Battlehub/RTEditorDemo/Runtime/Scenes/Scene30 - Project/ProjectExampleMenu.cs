using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene31
{
    /// <summary>
    /// This is example on how to create, open, close and delete projects using IProjectAsync interface
    /// </summary>
    [MenuDefinition]
    public class ProjectExampleMenu : EditorExtension
    {
        private IWindowManager m_wm;
        private IProjectAsync m_project;

        protected override void OnInit()
        {
            base.OnInit();

            m_wm = IOC.Resolve<IWindowManager>();
            m_project = IOC.Resolve<IProjectAsync>();
            m_project.Events.CreateProjectCompleted += OnCreateProjectCompleted;
            m_project.Events.OpenProjectCompleted += OnOpenProjectCompleted;
            m_project.Events.CloseProjectCompleted += OnCloseProjectCompleted;
            m_project.Events.DeleteProjectCompleted += OnDeleteProjectCompleted;
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
            if(m_project != null)
            {
                m_project.Events.CreateProjectCompleted -= OnCreateProjectCompleted;
                m_project.Events.OpenProjectCompleted -= OnOpenProjectCompleted;
                m_project.Events.CloseProjectCompleted -= OnCloseProjectCompleted;
                m_project.Events.DeleteProjectCompleted -= OnDeleteProjectCompleted;
            }
            m_wm = null;
        }

        [MenuCommand("Example/Manage Projects")]
        public void ManageProjects()
        {
            m_wm.CreateWindow(BuiltInWindowNames.OpenProject);
        }

        [MenuCommand("Example/Create Project")]
        public void CreateProject()
        {
            m_wm.Prompt("Enter Project Name", "My Project", async (sender, args) =>
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;

                IProjectAsync project = m_project;
                try
                {
                    using(await project.LockAsync())
                    {
                        await project.CreateProjectAsync(args.Text);
                    }
                }
                catch(StorageException e)
                {
                    if(e.ErrorCode == Error.E_AlreadyExist)
                    {
                        m_wm.MessageBox("Unable to create project", "A project with the same name already exists");
                    }
                    else
                    {
                        throw;
                    }
                }
                
                editor.IsBusy = false;
            });
        }

        [MenuCommand("Example/Open Project")]
        public void OpenProject()
        {
            m_wm.Prompt("Enter Project Name", "My Project", async (sender, args) =>
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;

                IProjectAsync project = m_project;
                using (await project.LockAsync())
                {
                    await project.OpenProjectAsync(args.Text);
                }

                editor.IsBusy = false;
            });
        }

        [MenuCommand("Example/Delete Project")]
        public void DeleteProject()
        {
            m_wm.Prompt("Enter Project Name", "My Project", async (sender, args) =>
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;

                IProjectAsync project = m_project;
                using (await project.LockAsync())
                {
                    await project.DeleteProjectAsync(args.Text);
                }

                editor.IsBusy = false;
            });
        }

        [MenuCommand("Example/Close Project")]
        public async void CloseProject()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            editor.IsBusy = true;

            IProjectAsync project = m_project;
            using (await project.LockAsync())
            {
                await project.CloseProjectAsync();
            }

            editor.IsBusy = false;
        }

        private void OnCreateProjectCompleted(object sender, ProjectEventArgs<ProjectInfo> e)
        {
            Debug.Log($"On {e.Payload.Name} Created");
        }

        private void OnOpenProjectCompleted(object sender, ProjectEventArgs<ProjectInfo> e)
        {
            Debug.Log($"On {e.Payload.Name} Opened");
        }

        private void OnCloseProjectCompleted(object sender, ProjectEventArgs<string> e)
        {
            Debug.Log($"On {e.Payload} Closed");
        }
        private void OnDeleteProjectCompleted(object sender, ProjectEventArgs<string> e)
        {
            Debug.Log($"On {e.Payload} Deleted");
        }

    }
}
