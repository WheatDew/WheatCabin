using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using UnityEngine.UI;
using System.IO;

namespace Battlehub.RTSL.Demo
{
    public class RTSLDemo : MonoBehaviour
    {
        [SerializeField]
        private Button m_btnSave = null;

        [SerializeField]
        private Button m_btnLoad = null;

        [SerializeField]
        private Button m_btnDestroy = null;

        [SerializeField]
        private CanvasGroup m_progress = null;

        [SerializeField]
        private Text m_progressText = null;

        private IProject m_project;

        [SerializeField]
        private string m_scenePath = "Scenes/Demo/MyScene";

        [SerializeField]
        private string m_projectName = "My Project";

        [SerializeField]
        private string m_builtInAssetLibrary = null;

        [SerializeField]
        private string m_sceneAssetLibrary = null;

        IEnumerator Start()
        {
            IProjectAsyncWithAssetLibraries projectAsync = IOC.Resolve<IProjectAsyncWithAssetLibraries>();
            projectAsync.BuiltInLibraryName = m_builtInAssetLibrary;
            projectAsync.SceneDepsAssetLibraryName = m_sceneAssetLibrary;

            m_project = IOC.Resolve<IProject>();
            
            yield return m_project.OpenProject(m_projectName);

            if(!string.IsNullOrEmpty(Path.GetDirectoryName(m_scenePath)))
            {
                yield return m_project.CreateFolder(Path.GetDirectoryName(m_scenePath).Replace(@"\", "/"));
            }

            
            
            if (m_btnSave != null)
            {
                m_btnSave.onClick.AddListener(OnSaveClick);
            }

            if(m_btnLoad != null)
            {
                m_btnLoad.interactable = m_project.Exist<Scene>(m_scenePath);
                m_btnLoad.onClick.AddListener(OnLoadClick);
            }

            if(m_btnDestroy != null)
            {
                m_btnDestroy.interactable = m_project.Exist<Scene>(m_scenePath);
                m_btnDestroy.onClick.AddListener(OnDestroyClick);
            }
        }

        private void OnDestroy()
        {
            if (m_btnSave != null)
            {
                m_btnSave.onClick.RemoveListener(OnSaveClick);
            }

            if (m_btnLoad != null)
            {
                m_btnLoad.onClick.RemoveListener(OnLoadClick);
            }

            if(m_btnDestroy != null)
            {
                m_btnDestroy.onClick.RemoveListener(OnDestroyClick);
            }

            StopAllCoroutines();
        }

        private void OnSaveClick()
        {
            StartCoroutine(SaveScene());
        }

        private void OnLoadClick()
        {
            if (m_project.Exist<Scene>(m_scenePath))
            {
                StartCoroutine(LoadScene());
            }
        }

        private void OnDestroyClick()
        {
            if(m_project.Exist<Scene>(m_scenePath))
            {
                m_project.CreateNewScene();

                GameObject go = new GameObject();
                go.name = "Camera";
                go.AddComponent<Camera>();
            }
        }

        private WaitForEndOfFrame m_waitForEndOfFrame = new WaitForEndOfFrame();

        IEnumerator FadeInProgress()
        {
            if (m_progress != null)
            {
                m_progress.gameObject.SetActive(true);
                while (m_progress.alpha < 1)
                {
                    m_progress.alpha += Time.deltaTime;
                    yield return m_waitForEndOfFrame;
                }
            }
        }

        IEnumerator FadeOutProgress()
        {
            if (m_progress != null)
            {
                while (m_progress.alpha > 0)
                {
                    m_progress.alpha -= Time.deltaTime;
                    yield return m_waitForEndOfFrame;
                }
                m_progress.gameObject.SetActive(false);
            }
        }

        IEnumerator SaveScene()
        {
            if(m_progressText != null) { m_progressText.text = "Saving ..."; }

            yield return FadeInProgress();
            ProjectAsyncOperation ao = m_project.Save(m_scenePath, SceneManager.GetActiveScene());
            yield return ao;
            yield return FadeOutProgress();

            if (ao.Error.HasError)
            {
                Debug.LogError(ao.Error.ToString());
            }
            else
            {
                if(m_btnLoad != null)
                {
                    m_btnLoad.interactable = m_project.Exist<Scene>(m_scenePath);
                }

                if(m_btnDestroy != null)
                {
                    m_btnDestroy.interactable = m_project.Exist<Scene>(m_scenePath);
                }
            }
        }

        IEnumerator LoadScene()
        {
            if (m_progressText != null) { m_progressText.text = "Loading ..."; }

           
            yield return FadeInProgress();
            ProjectAsyncOperation ao = m_project.Load<Scene>(m_scenePath);
            yield return ao;
            yield return FadeOutProgress();
          
            if (m_progress != null) { m_progress.gameObject.SetActive(false); }

            if (ao.Error.HasError)
            {
                Debug.LogError(ao.Error.ToString());
            }
        }
    }
}

