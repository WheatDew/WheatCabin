using Battlehub.RTCommon;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTMeasurement
{
    public class MeasureTool : RTEComponent
    {
        [SerializeField]
        private LineStripRenderer m_renderer;

        [SerializeField]
        private LineStripRenderer m_pointerRenderer;

        [SerializeField]
        private TextMeshPro m_output = null;

        protected LineStripRenderer Renderer
        {
            get { return m_renderer; }
        }

        protected LineStripRenderer PointerRenderer
        {
            get { return m_pointerRenderer; }
        }

        public TextMeshPro Output
        {
            get { return m_output; }
        }

        public override RuntimeWindow Window
        {
            get;
            set;
        }

        private IRTECamera m_rteCamera;

        protected override void Awake()
        {
            base.Awake();
        
            if (m_renderer == null)
            {
                m_renderer = gameObject.AddComponent<LineStripRenderer>();
            }

            if(m_output != null)
            {
                m_output.GetComponent<Renderer>().enabled = false;
            }

            if(Window != null)
            {
                m_rteCamera = Window.Camera.gameObject.AddComponent<RTECamera>();
                m_rteCamera.Event = CameraEvent.AfterImageEffects;
                m_rteCamera.CommandBufferRefresh += OnCommandBufferRefresh;
                m_rteCamera.RefreshCommandBuffer();
            }
            
            if (m_pointerRenderer == null)
            {
                GameObject pointerGo = new GameObject("Pointer");
                pointerGo.transform.SetParent(transform, false);
                pointerGo.transform.position = Vector3.one * 10000;
                m_pointerRenderer = pointerGo.AddComponent<LineStripRenderer>();
            }
            m_pointerRenderer.Vertices = new[] { Vector3.zero };

            Transform[] transforms = m_renderer.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; ++i)
            {
                transforms[i].gameObject.layer = Editor.CameraLayerSettings.AllScenesLayer;
            }
        }

        protected virtual void OnDisable()
        {
            if (m_pointerRenderer != null)
            {
                m_pointerRenderer.transform.position = Vector3.one * 10000;
            }

            if(m_renderer != null)
            {
                m_renderer.Vertices = new Vector3[0];
                m_renderer.Refresh();
            }

            if(Output != null)
            {
                Output.text = "";
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_rteCamera != null)
            {
                m_rteCamera.CommandBufferRefresh -= OnCommandBufferRefresh;
                m_rteCamera.Destroy();
                m_rteCamera = null;
            }
        }

        protected virtual void Update()
        {
            if (Window == null)
            {
                return;
            }

            if(Editor.ActiveWindow != Window)
            {
                return;
            }

            if(CancelAction())
            {
                Editor.Tools.Current = RuntimeTool.Move;
                return;
            }
            UpdateOverride();
        }

        protected virtual void UpdateOverride()
        {
            if (BeginVertexSnappingAction())
            {
                BeginVertexSnapping();
            }
            else if (EndVertexSnappingAction())
            {
                EndVertexSnapping();
            }
        }

        protected override void OnActiveWindowChanged(RuntimeWindow deactivatedWindow)
        {
            base.OnActiveWindowChanged(deactivatedWindow);

            if (m_rteCamera != null)
            {
                m_rteCamera.CommandBufferRefresh -= OnCommandBufferRefresh;
                m_rteCamera.Destroy();
                m_rteCamera = null;
            }

            if (Editor.ActiveWindow == null || Editor.ActiveWindow.WindowType != RuntimeWindowType.Scene)
            {
                Window = null;
            }
            else
            {
                Window = Editor.ActiveWindow;

                m_rteCamera = Window.Camera.gameObject.AddComponent<RTECamera>();
                m_rteCamera.Event = CameraEvent.AfterImageEffects;
                m_rteCamera.CommandBufferRefresh += OnCommandBufferRefresh;
                m_rteCamera.RefreshCommandBuffer();
            }
        }

        protected virtual bool BeginVertexSnappingAction()
        {
            return Editor.Input.GetKeyDown(KeyCode.V);
        }

        protected virtual bool VertexSnappingAction()
        {
            return Editor.Input.GetKey(KeyCode.V);
        }

        protected virtual bool EndVertexSnappingAction()
        {
            return Editor.Input.GetKeyUp(KeyCode.V);
        }

        protected virtual bool CancelAction()
        {
            return Editor.Input.GetKeyUp(KeyCode.Escape);
        }

        private bool m_isInVertexSnappingMode;
        private ExposeToEditor[] m_vertexSnappingTargets;
        protected void BeginVertexSnapping()
        {
            m_isInVertexSnappingMode = true;
            m_vertexSnappingTargets = Editor.Object.Get(false).ToArray();
        }

        protected Vector3 SnapToVertex(Vector3 position)
        {
            if(!m_isInVertexSnappingMode)
            {
                return position;
            }

            Vector3 result;
            if(GetClosestVertex(position, m_vertexSnappingTargets, out result))
            {
                return result;
            }
            return position;
        }

        protected void EndVertexSnapping()
        {
            m_isInVertexSnappingMode = false;
            m_vertexSnappingTargets = null;
        }

        private Vector3[] m_boundingBoxCorners = new Vector3[0];
        protected bool GetClosestVertex(Vector3 position, ExposeToEditor[] targets, out Vector3 result)
        {
            Ray ray = Window.Pointer;
            RaycastHit hitInfo;

            GameObject closestObject = null;
            if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity))
            {
                closestObject = hitInfo.collider.gameObject;
            }
            else
            {
                Pointer pointer = Window.Pointer;
                Vector2 screenPosition;
                if (pointer.WorldToScreenPoint(Vector3.zero, position, out screenPosition))
                {

                    float minDistance = float.MaxValue;
                    for (int i = 0; i < targets.Length; ++i)
                    {
                        ExposeToEditor exposedToEditor = targets[i];
                        Bounds bounds = exposedToEditor.Bounds;

                        m_boundingBoxCorners[0] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                        m_boundingBoxCorners[1] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
                        m_boundingBoxCorners[2] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
                        m_boundingBoxCorners[3] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
                        m_boundingBoxCorners[4] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);
                        m_boundingBoxCorners[5] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
                        m_boundingBoxCorners[6] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);
                        m_boundingBoxCorners[7] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);

                        for (int j = 0; j < m_boundingBoxCorners.Length; ++j)
                        {
                            Vector2 boundsScreenPoint;

                            if (pointer.WorldToScreenPoint(Vector3.zero, exposedToEditor.BoundsObject.transform.TransformPoint(m_boundingBoxCorners[j]), out boundsScreenPoint))
                            {
                                float distance = (boundsScreenPoint - screenPosition).magnitude;
                                if (distance < minDistance)
                                {
                                    closestObject = exposedToEditor.gameObject;
                                    minDistance = distance;
                                }
                            }
                        }
                    }
                }
            }


            if (closestObject != null)
            {
                float minDistance = float.MaxValue;
                Vector3 minPoint = Vector3.zero;
                bool minPointFound = false;
                Transform meshTransform;
                Mesh mesh = GetMesh(closestObject, out meshTransform);
                GetMinPoint(meshTransform, ref minDistance, ref minPoint, ref minPointFound, mesh);

                if (minPointFound)
                {
                    result = minPoint;
                    return true;
                }
            }

            result = Vector3.zero;
            return false;
        }

        private void GetMinPoint(Transform meshTransform, ref float minDistance, ref Vector3 minPoint, ref bool minPointFound, Mesh mesh)
        {
            if (mesh != null && mesh.isReadable)
            {
                IRTE editor = Editor;
                Vector3[] vertices = mesh.vertices;
                for (int i = 0; i < vertices.Length; ++i)
                {
                    Vector3 vert = vertices[i];
                    vert = meshTransform.TransformPoint(vert);

                    Vector2 screenPoint;
                    if (Window.Pointer.WorldToScreenPoint(Vector3.zero, vert, out screenPoint))
                    {
                        Vector2 mousePoint;
                        if (Window.Pointer.XY(Vector3.zero, out mousePoint))
                        {
                            float distance = (screenPoint - mousePoint).magnitude;
                            if (distance < minDistance)
                            {
                                minPointFound = true;
                                minDistance = distance;
                                minPoint = vert;
                            }
                        }
                    }
                }
            }
        }

        private static Mesh GetMesh(GameObject go, out Transform meshTransform)
        {
            Mesh mesh = null;
            meshTransform = null;
            MeshFilter filter = go.GetComponentInChildren<MeshFilter>();
            if (filter != null)
            {
                mesh = filter.sharedMesh;
                meshTransform = filter.transform;
            }
            else
            {
                SkinnedMeshRenderer skinnedMeshRender = go.GetComponentInChildren<SkinnedMeshRenderer>();
                if (skinnedMeshRender != null)
                {
                    mesh = skinnedMeshRender.sharedMesh;
                    meshTransform = skinnedMeshRender.transform;
                }
                else
                {
                    MeshCollider collider = go.GetComponentInChildren<MeshCollider>();
                    if (collider != null)
                    {
                        mesh = collider.sharedMesh;
                        meshTransform = collider.transform;
                    }
                }
            }

            return mesh;
        }

        protected virtual void OnCommandBufferRefresh(IRTECamera camera)
        {
              
        }
    }
}

