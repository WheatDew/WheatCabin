using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace Battlehub.RTNavigation
{
    public class NavMeshSurfaceGizmo : RTEComponent
    {
        private Mesh m_facesMesh;
        private Mesh m_edgesMesh;
        private Mesh m_verticesMesh;

        private Material m_facesMaterial;
        private Material m_edgesMaterial;
        private Material m_verticesMaterial;

        private IRTECamera m_rteCamera;

        protected override void Awake()
        {
            base.Awake();
            if (m_rteCamera == null)
            {
                IRTEGraphics graphics = IOC.Resolve<IRTEGraphics>();
                if (graphics != null)
                {
                    m_rteCamera = graphics.GetOrCreateCamera(Window.Camera, CameraEvent.AfterForwardAlpha);
                }

                

                if (m_rteCamera == null)
                {
                    m_rteCamera = Window.Camera.gameObject.AddComponent<RTECamera>();
                    m_rteCamera.Event = CameraEvent.AfterForwardAlpha;
                }
            }

            m_rteCamera.CommandBufferRefresh += OnCommandBufferRefresh;
            
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            m_facesMaterial = new Material(Shader.Find("Battlehub/RTNavigation/UnlitColor"));
            m_facesMaterial.color = new Color(1, 1, 1, 0.5f);
            m_facesMaterial.SetInt("_ZTest", (int)CompareFunction.LessEqual);
            m_facesMaterial.SetFloat("_ZWrite", 0);

            m_edgesMaterial = new Material(Shader.Find("Battlehub/RTCommon/LineBillboard"));
            m_edgesMaterial.color = new Color(0, 0, 0, 0.15f);
            m_edgesMaterial.SetFloat("_Scale", 1.0f);
            m_edgesMaterial.SetInt("_HandleZTest", (int)CompareFunction.LessEqual);
            
            m_verticesMaterial = new Material(Shader.Find("Hidden/RTHandles/PointBillboard"));
            m_verticesMaterial.color = new Color(0, 0, 0, 0.3f);
            m_verticesMaterial.SetFloat("_Scale", 1.25f);
            m_verticesMaterial.SetInt("_HandleZTest", (int)CompareFunction.LessEqual);

            Vector3[] verts = triangulation.vertices;
            int[] tris = triangulation.indices;

            m_facesMesh = new Mesh { name = "Faces" };
            m_facesMesh.vertices = verts;
            m_facesMesh.triangles = tris;

            int[] areas = triangulation.areas;
            Color32[] colors = new Color32[triangulation.vertices.Length];
            for(int i = 0; i < areas.Length; i++)
            {
                int area = areas[i];
                Color32 color = Colors.Kellys[(area + 3) % Colors.Kellys.Length];
                colors[tris[i * 3]] = color;
                colors[tris[i * 3 + 1]] = color;
                colors[tris[i * 3 + 2]] = color;
            }
            m_facesMesh.colors32 = colors;
  
            m_edgesMesh = new Mesh { name = "Edges" };
            m_edgesMesh.vertices = verts;
            int[] indices = new int[tris.Length * 2];
            for (int i = 0; i < tris.Length; i += 3)
            {
                indices[i * 2] = tris[i];
                indices[i * 2 + 1] = tris[i + 1];

                indices[i * 2 + 2] = tris[i + 1];
                indices[i * 2 + 3] = tris[i + 2];

                indices[i * 2 + 4] = tris[i + 2];
                indices[i * 2 + 5] = tris[i];
            }
            m_edgesMesh.SetIndices(indices, MeshTopology.Lines, 0);

            m_verticesMesh = new Mesh { name = "Vertices" };
            m_verticesMesh.vertices = verts;
            m_verticesMesh.indexFormat = IndexFormat.UInt32;
            m_verticesMesh.SetIndices(Enumerable.Range(0, verts.Length).ToArray(), MeshTopology.Points, 0);

            m_rteCamera.RefreshCommandBuffer();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(m_facesMesh != null)
            {
                Destroy(m_facesMesh);
            }

            if(m_facesMaterial != null)
            {
                Destroy(m_facesMaterial);
            }

            if(m_edgesMesh != null)
            {
                Destroy(m_edgesMesh);
            }

            if(m_edgesMaterial != null)
            {
                Destroy(m_edgesMaterial);
            }

            if(m_verticesMesh != null)
            {
                Destroy(m_verticesMesh);
            }

            if(m_verticesMaterial != null)
            {
                Destroy(m_verticesMaterial);
            }

            if(m_rteCamera != null)
            {
                m_rteCamera.CommandBufferRefresh -= OnCommandBufferRefresh;
            }
        }

        protected virtual void OnCommandBufferRefresh(IRTECamera camera)
        {
            if(m_facesMesh != null)
            {
                camera.CommandBuffer.DrawMesh(m_facesMesh, Matrix4x4.identity, m_facesMaterial);
                camera.CommandBuffer.DrawMesh(m_edgesMesh, Matrix4x4.identity, m_edgesMaterial);
                camera.CommandBuffer.DrawMesh(m_verticesMesh, Matrix4x4.identity, m_verticesMaterial);
            }
        }

    }

}
