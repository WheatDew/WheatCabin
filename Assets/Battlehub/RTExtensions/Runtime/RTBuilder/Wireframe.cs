using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class Wireframe : MonoBehaviour
    {
        private IRTE m_editor;
        private RuntimeWindow m_window;
        private IRuntimeSceneComponent m_sceneComponent;

        protected IRTE Editor
        {
            get { return m_editor; }
        }

        protected RuntimeWindow Window
        {
            get { return m_window; }
        }

        protected IRuntimeSceneComponent SceneComponent
        {
            get { return m_sceneComponent; }
        }
        
        protected virtual void Awake()
        {
            m_window = GetComponent<RuntimeWindow>();
            
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Object.Started += OnObjectStarted;
            m_editor.Selection.SelectionChanged += OnSelectionChanged;

            m_sceneComponent = m_window.IOCContainer.Resolve<IRuntimeSceneComponent>();

            foreach (ExposeToEditor obj in m_editor.Object.Get(false))
            {
                TryCreateWireframe(obj);
            }
        }

        protected virtual void Start()
        {
            SetCullingMask(m_window);
        }

        protected virtual void OnDestroy()
        {
            if(m_editor != null && m_editor.Object != null)
            {
                m_editor.Object.Started -= OnObjectStarted;
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;

                foreach (ExposeToEditor obj in m_editor.Object.Get(false))
                {
                    TryDestroyWireframe(obj);
                }
            }

            if(m_window != null)
            {
                ResetCullingMask(m_window);
            }
        }

        protected virtual void OnObjectStarted(ExposeToEditor obj)
        {
            TryCreateWireframe(obj);
        }

        protected virtual void TryCreateWireframe(ExposeToEditor obj)
        {
            PBMesh pbMesh = obj.GetComponent<PBMesh>();
            if (pbMesh != null && !pbMesh.GetComponentsInChildren<WireframeMesh>().Any(w => !w.IsIndividual))
            {
                CreateWireframeMesh(pbMesh);
            }
        }

        protected virtual void CreateWireframeMesh(PBMesh pbMesh)
        {
            GameObject wireframe = new GameObject("Wireframe");
            wireframe.transform.SetParent(pbMesh.transform, false);
            wireframe.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            wireframe.layer = m_editor.CameraLayerSettings.ExtraLayer;
            WireframeMesh wireframeMesh = wireframe.AddComponent<WireframeMesh>();

            if(IsSelected(pbMesh.gameObject))
            {
                wireframeMesh.IsSelected = true;
            }
        }

        protected virtual void TryDestroyWireframe(ExposeToEditor obj)
        {
            PBMesh pbMesh = obj.GetComponent<PBMesh>();
            if (pbMesh != null)
            {
                WireframeMesh[] wireframeMesh = pbMesh.GetComponentsInChildren<WireframeMesh>(true);
                for (int i = 0; i < wireframeMesh.Length; ++i)
                {
                    WireframeMesh wireframe = wireframeMesh[i];
                    if (!wireframe.IsIndividual)
                    {
                        Destroy(wireframe.gameObject);
                        break;
                    }
                }
            }
        }

        protected virtual void SetCullingMask(RuntimeWindow window)
        {
            window.Camera.cullingMask = (1 << LayerMask.NameToLayer("UI")) | (1 << m_editor.CameraLayerSettings.AllScenesLayer) | (1 << m_editor.CameraLayerSettings.ExtraLayer);
            window.Camera.backgroundColor = Color.white;
            window.Camera.clearFlags = CameraClearFlags.SolidColor;

            if(m_sceneComponent != null && m_sceneComponent.SceneGizmo != null)
            {
                m_sceneComponent.SceneGizmo.TextColor = Color.black;
            }
        }

        protected virtual void ResetCullingMask(RuntimeWindow window)
        {
            CameraLayerSettings settings = m_editor.CameraLayerSettings;
            if (window.Camera != null)
            {
                window.Camera.cullingMask = ~((1 << m_editor.CameraLayerSettings.ExtraLayer) | ((1 << settings.MaxGraphicsLayers) - 1) << settings.RuntimeGraphicsLayer);
                window.Camera.clearFlags = CameraClearFlags.Skybox;
            }
            
            if (m_sceneComponent != null && m_sceneComponent.SceneGizmo != null)
            {
                m_sceneComponent.SceneGizmo.TextColor = Color.white;
            }
        }

        protected virtual void OnSelectionChanged(Object[] unselectedObjects)
        {
            if (unselectedObjects != null)
            {
                WireframeMesh[] wireframes = unselectedObjects.Select(go => go as GameObject).Where(go => go != null).SelectMany(go => go.GetComponentsInChildren<WireframeMesh>(true)).ToArray();
                for(int i = 0; i < wireframes.Length; ++i)
                {
                    wireframes[i].IsSelected = false;
                }
            }

            TryToSelect();
        }

        protected virtual void TryToSelect()
        {
            if (m_editor.Selection.gameObjects != null)
            {
                WireframeMesh[] wireframes = m_editor.Selection.gameObjects.Where(go => go != null).SelectMany(go => go.GetComponentsInChildren<WireframeMesh>(true)).ToArray();
                for (int i = 0; i < wireframes.Length; ++i)
                {
                    wireframes[i].IsSelected = true;
                }
            }
        }

        protected bool IsSelected(GameObject gameObject)
        {
            Transform parent = gameObject.transform;
            while (parent != null)
            {
                if (m_editor.Selection.IsSelected(parent.gameObject))
                {
                    return true;
                }

                parent = parent.parent;
            }
            return false;
        }
    }
}

