using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using Battlehub.Utils;

namespace Battlehub.RTGizmos
{
    public class RectTransformGizmo : UIBehaviour, IRTEComponent
    {
        private Mesh m_mesh;
        private Mesh m_parentMesh;
        private Mesh m_canvasMesh;
        private Material m_material;
        private Material m_parentMaterial;

        private IRTECamera m_rteCamera;
        private TransformChangesTracker m_tracker;
        private readonly Vector3[] m_corners = new Vector3[4];

        private RectTransform m_rt;
        private RectTransform m_parentRT;
        private RectTransform m_canvasRT;
        private bool m_refesh;

        public RuntimeWindow Window 
        {
            get;
            set;
        }

        protected override void Start()
        {
            base.Start();

            if(Window == null)
            {
                Destroy(this);
                return;
            }

            m_tracker = new TransformChangesTracker(transform);

            IRTEGraphics graphics = IOC.Resolve<IRTEGraphics>();
            m_rteCamera = graphics.CreateCamera(Window.Camera, CameraEvent.AfterForwardAlpha, false);
            m_rteCamera.CommandBufferRefresh += OnCommandBufferRefresh;

            m_material = new Material(Shader.Find("Battlehub/RTCommon/LineBillboard"));
            m_material.color = new Color(1, 1, 1, 0.5f);
            m_material.SetFloat("_Scale", 0.8f);
            m_material.SetInt("_HandleZTest", (int)CompareFunction.LessEqual);

            m_parentMaterial = new Material(Shader.Find("Battlehub/RTCommon/LineBillboard"));
            m_parentMaterial.color = new Color(1, 1, 1, 0.2f);
            m_parentMaterial.SetFloat("_Scale", 0.5f);
            m_parentMaterial.SetInt("_HandleZTest", (int)CompareFunction.LessEqual);

            m_mesh = GraphicsUtility.CreateWireQuad();
            m_parentMesh = GraphicsUtility.CreateWireQuad();
            m_canvasMesh = GraphicsUtility.CreateWireQuad();
            
            CacheTransforms();

            m_rteCamera.RefreshCommandBuffer();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
         
            if (m_rteCamera != null)
            {
                m_rteCamera.Destroy();
                m_rteCamera.CommandBufferRefresh -= OnCommandBufferRefresh;
            }

            Destroy(m_material);
            Destroy(m_parentMaterial);
            Destroy(m_mesh);
            Destroy(m_parentMesh);
            Destroy(m_canvasMesh);
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            CacheTransforms();
            m_refesh = true;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            m_refesh = true;
        }

        private void LateUpdate()
        {
            m_refesh |= m_tracker.HasChanged;
            if (m_tracker.HasChanged)
            {
                m_tracker.Reset();
            }

            if (m_refesh)
            {
                m_rteCamera.RefreshCommandBuffer();
                m_refesh = false;
            }
        }

        protected virtual void OnCommandBufferRefresh(IRTECamera camera)
        {
            if(m_canvasRT != null)
            {
                m_canvasRT.GetWorldCorners(m_corners);
                m_canvasMesh.vertices = m_corners;
                camera.CommandBuffer.DrawMesh(m_canvasMesh, Matrix4x4.identity, m_parentMaterial);
            }

            if (m_parentRT != null)
            {
                m_parentRT.GetWorldCorners(m_corners);
                m_parentMesh.vertices = m_corners;
                camera.CommandBuffer.DrawMesh(m_parentMesh, Matrix4x4.identity, m_parentMaterial);
            }

            if (m_rt != null)
            {
                m_rt.GetWorldCorners(m_corners);
                m_mesh.vertices = m_corners;
                camera.CommandBuffer.DrawMesh(m_mesh, Matrix4x4.identity, m_material);
            }
        }

        private void CacheTransforms()
        {
            m_rt = transform as RectTransform;
            if (m_rt == null)
            {
                m_parentRT = null;
                m_canvasRT = null;
            }
            else
            {
                Canvas canvas = m_rt.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    m_canvasRT = canvas.transform as RectTransform;
                }

                if (m_canvasRT == m_rt)
                {
                    m_canvasRT = null;
                    m_parentRT = null;
                }
                else
                {
                    m_parentRT = m_rt.parent as RectTransform;
                    if (m_canvasRT == m_parentRT)
                    {
                        m_parentRT = null;
                    }
                }
            }   
        }
    }
}
