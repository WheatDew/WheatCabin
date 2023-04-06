using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.MenuControl;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.RTEditor.Examples.Scene32
{
    /// <summary>
    /// This is example on how to create, save, load, delete scene using IProjectAsync interface
    /// </summary>
    [MenuDefinition]
    public class SceneExampleMenu : EditorExtension
    {
        private IRuntimeSceneManager m_sceneManager;
        private IProjectAsync m_project;
        private IWindowManager m_wm;

        protected override void OnInit()
        {
            base.OnInit();

            m_wm = IOC.Resolve<IWindowManager>();

            m_sceneManager = IOC.Resolve<IRuntimeSceneManager>();
            m_sceneManager.NewSceneCreating += OnNewSceneCreating;
            m_sceneManager.NewSceneCreated += OnNewSceneCreated;

            m_project = IOC.Resolve<IProjectAsync>();
            m_project.Events.BeginSave += OnBeginSave;
            m_project.Events.SaveCompleted += OnSaveCompleted;
            m_project.Events.BeginLoad += OnBeginLoad;
            m_project.Events.LoadCompleted += OnLoadCompleted;
            m_project.Events.DeleteCompleted += OnDeleteCompleted;
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();

            if (m_project != null)
            {
                m_project.Events.BeginSave -= OnBeginSave;
                m_project.Events.SaveCompleted -= OnSaveCompleted;
                m_project.Events.BeginLoad -= OnBeginLoad;
                m_project.Events.LoadCompleted -= OnLoadCompleted;
                m_project.Events.DeleteCompleted -= OnDeleteCompleted;

                m_project = null;
            }

            if (m_sceneManager != null)
            {
                m_sceneManager.NewSceneCreating -= OnNewSceneCreating;
                m_sceneManager.NewSceneCreated -= OnNewSceneCreated;
                m_sceneManager = null;
            }

            m_wm = null;

        }

        [MenuCommand("Example/New Scene")]
        public void CreateNewScene()
        {
            m_sceneManager.CreateNewScene();
        }

        [MenuCommand("Example/Save Scene")]
        public void SaveScene()
        {
            m_wm.Prompt("Enter Scene Name", "My Scene", async (sender, args) =>
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;

                IProjectAsync project = m_project;
                using (await project.LockAsync())
                {
                    await project.SaveAsync(args.Text, SceneManager.GetActiveScene());
                }

                editor.IsBusy = false;
            });
        }

        [MenuCommand("Example/Load Scene")]
        public void LoadScene()
        {
            m_wm.Prompt("Enter Scene Name", "My Scene", async (sender, args) =>
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;

                IProjectAsync project = m_project;
                using (await project.LockAsync())
                {
                    if (!project.Utils.Exist<Scene>(args.Text))
                    {
                        await Task.Yield();
                        m_wm.MessageBox("Unable to load scene", $"{args.Text} was not found");
                    }
                    else
                    {
                        await project.LoadAsync(args.Text, typeof(Scene));
                    }
                }

                editor.IsBusy = false;
            });
        }

        [MenuCommand("Example/Delete Scene")]
        public void Delete()
        {
            m_wm.Prompt("Enter Scene Name", "My Scene", async (sender, args) =>
            {
                IRTE editor = IOC.Resolve<IRTE>();
                editor.IsBusy = true;

                IProjectAsync project = m_project;
                using (await project.LockAsync())
                {
                    await project.DeleteAsync(args.Text, typeof(Scene));
                }

                editor.IsBusy = false;
            });
        }

        private void OnNewSceneCreating(object sender, System.EventArgs e)
        {
            Debug.Log("New Scene Creating");
        }

        private void OnNewSceneCreated(object sender, System.EventArgs e)
        {
            Debug.Log("New Scene Created");
        }


        private void OnBeginSave(object sender, ProjectEventArgs<object[]> e)
        {
            bool isScene = e.Payload[0] is Scene;
            if (isScene)
            {
                Debug.Log("Scene saving");
            }
        }

        private void OnSaveCompleted(object sender, ProjectEventArgs<(ProjectItem[] SavedItems, bool IsUserAction)> e)
        {
            bool isScene = m_project.Utils.IsScene(e.Payload.SavedItems[0]);
            if (isScene)
            {
                Debug.Log($"{e.Payload.SavedItems[0].Name} saved");
            }
        }

        private void OnBeginLoad(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            bool isScene = m_project.Utils.IsScene(e.Payload[0]);
            if (isScene)
            {
                Debug.Log($"{e.Payload[0].Name} loading");
            }
        }

        private void OnLoadCompleted(object sender, ProjectEventArgs<(ProjectItem[] LoadedItems, Object[] LoadedObjects)> e)
        {
            bool isScene = m_project.Utils.IsScene(e.Payload.LoadedItems[0]);
            if (isScene)
            {
                Debug.Log($"{e.Payload.LoadedItems[0].Name} loaded");
            }
        }

        private void OnDeleteCompleted(object sender, ProjectEventArgs<ProjectItem[]> e)
        {
            bool isScene = m_project.Utils.IsScene(e.Payload[0]);
            if (isScene)
            {
                Debug.Log($"{e.Payload[0].Name} deleted");
            }
        }
    }
}
