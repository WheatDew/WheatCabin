using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTBuilder
{
    public class ManualUVRenderer : RTEComponent
    {
        public enum RenderMode
        {
            Vertex,
            Edge,
            Face,
        }

        
        public const float Scale = 10;

        [SerializeField]
        private float m_edgesScale = 0.5f;

        [SerializeField]
        private float m_verticesScale = 3.3f;

        [SerializeField]
        private CompareFunction m_zTest = CompareFunction.LessEqual;

        [SerializeField]
        private Color m_autoUVColor = Color.red;


        private Material m_edgesMaterial;
        private Material m_verticesMaterial;
        private Mesh m_edgesMesh;
        private Mesh m_verticesMesh;
        private IRTECamera m_graphicsCamera;

        private Color[] m_vertexColors = new Color[0];
        public Color[] VertexColors
        {
            get { return m_vertexColors; }
        }

        private Color[] m_edgeColors = new Color[0];
        public Color[] EdgeColors
        {
            get { return m_edgeColors; }
        }

        private Vector2[] m_uv = new Vector2[0];
        public Vector2[] UV
        {
            get { return m_uv; }
            set
            {
                m_uv = value;
                if(m_uv == null)
                {
                    m_uv = new Vector2[0];
                }
            }
        }

        private bool[] m_isSelected = new bool[0];
        public bool[] IsSelected
        {
            get { return m_isSelected; }
            set
            {
                m_isSelected = value;
                if(m_isSelected == null)
                {
                    m_isSelected = new bool[0];
                }
            }
        }
        
        private PBEdge[] m_edges;
        public PBEdge[] Edges
        {
            get { return m_edges; }
            set
            {
                m_edges = value;
                if(m_edges == null)
                {
                    m_edges = new PBEdge[0];
                }
            }
        }

        private Dictionary<int, List<int>> m_faceToEdge;
        public Dictionary<int, List<int>> FaceToEdges
        {
            get { return m_faceToEdge; }
            set
            {
                m_faceToEdge = value;
                if(m_faceToEdge == null)
                {
                    m_faceToEdge = new Dictionary<int, List<int>>();
                }
            }
        }

        private PBFace[] m_faces;
        public PBFace[] Faces
        {
            get { return m_faces; }
            set { m_faces = value; }
        }

        private RenderMode m_mode;
        public RenderMode Mode
        {
            get { return m_mode; }
            set
            {
                m_mode = value;
                switch (m_mode)
                {
                    case RenderMode.Vertex:
                        m_edgesMaterial.SetFloat("_Scale", m_edgesScale);
                        break;
                    case RenderMode.Face:
                        m_edgesMaterial.SetFloat("_Scale", m_edgesScale);
                        break;
                    default:
                        m_edgesMaterial.SetFloat("_Scale", m_edgesScale);
                        break;
                }

            }
        }

        protected override void Awake()
        {
            base.Awake();
        
            string vertShader = PBBuiltinMaterials.geometryShadersSupported ?
                PBBuiltinMaterials.pointShader :
                PBBuiltinMaterials.dotShader;

            m_verticesMaterial = new Material(Shader.Find(vertShader));
            m_verticesMaterial.SetColor("_Color", Color.white);
            m_verticesMaterial.SetInt("_HandleZTest", (int)m_zTest);
            m_verticesMaterial.SetFloat("_Scale", m_verticesScale);

            m_edgesMaterial = new Material(PBBuiltinMaterials.LinesMaterial);
            m_edgesMaterial.SetColor("_Color", Color.white);
            m_edgesMaterial.SetInt("_HandleZTest", (int)m_zTest);
            m_edgesMaterial.SetFloat("_Scale", m_edgesScale);

            m_verticesMesh = new Mesh { name = "UVVertices" };
            m_edgesMesh = new Mesh { name = "UVEdges" };

            RuntimeCameraWindow cameraWindow = (RuntimeCameraWindow)Window;

            m_graphicsCamera = cameraWindow.GetGraphicsCamera();
            if(m_graphicsCamera != null)
            {
                m_graphicsCamera.MeshesCache.RefreshMode = CacheRefreshMode.Always;
            }
        }        
        protected override void OnDestroy()
        {
            if (m_graphicsCamera != null && m_graphicsCamera.MeshesCache != null)
            {
                m_graphicsCamera.MeshesCache.RemoveBatch(m_verticesMesh);
                m_graphicsCamera.MeshesCache.RemoveBatch(m_edgesMesh);
            }

            Destroy(m_verticesMesh);
            Destroy(m_edgesMesh);

            Destroy(m_verticesMaterial);
            Destroy(m_edgesMaterial);
        }

        public void Refresh(bool verticesOnly, bool colorsOnly)
        {
            if (!verticesOnly && !colorsOnly)
            {
                m_verticesMesh.Clear();
                m_edgesMesh.Clear();

                int[] indices;
                if (Mode == RenderMode.Vertex)
                {
                    indices = new int[m_uv.Length];
                    m_vertexColors = new Color[m_uv.Length];
                    m_verticesMesh.vertices = new Vector3[m_uv.Length];
                    for (int i = 0; i < indices.Length; ++i)
                    {
                        indices[i] = i;
                        m_vertexColors[i] = Color.white;
                    }
                    m_verticesMesh.SetIndices(indices, MeshTopology.Points, 0);
                }
                else if(Mode == RenderMode.Face)
                {
                    indices = new int[m_faces.Length];
                    m_vertexColors = new Color[m_faces.Length];
                    m_verticesMesh.vertices = new Vector3[m_faces.Length];
                    for (int i = 0; i < indices.Length; ++i)
                    {
                        indices[i] = i;
                        //m_vertexColors[i] = m_faces[i].IsManualUV ? Color.white : m_autoUVColor;
                        m_vertexColors[i] = Color.white;
                    }

                    m_verticesMesh.colors = m_vertexColors;
                    m_verticesMesh.SetIndices(indices, MeshTopology.Points, 0);
                }
                
                indices = new int[m_edges.Length * 2];
                m_edgesMesh.vertices = new Vector3[m_edges.Length * 2];
                m_edgeColors = new Color[m_edges.Length * 2];
                
                for (int i = 0; i < m_edges.Length; ++i)
                {
                    indices[i * 2] = i * 2;
                    indices[i * 2 + 1] = i * 2 + 1;

                    PBFace face = m_faces[m_edges[i].FaceIndex];

                    m_edgeColors[i * 2] = face.IsManualUV ? Color.white : m_autoUVColor;
                    m_edgeColors[i * 2 + 1] = face.IsManualUV ? Color.white : m_autoUVColor;
                }

                m_edgesMesh.SetIndices(indices, MeshTopology.Lines, 0);

                RefreshVertices(Scale);
                RefreshColors();
            }
            else
            {
                if(verticesOnly)
                {
                    RefreshVertices(Scale);
                }

                if(colorsOnly)
                {
                    RefreshColors();
                }
            }

            if(m_graphicsCamera != null)
            {
                m_graphicsCamera.MeshesCache.RemoveBatch(m_verticesMesh);
                m_graphicsCamera.MeshesCache.RemoveBatch(m_edgesMesh);

                m_graphicsCamera.MeshesCache.AddBatch(m_verticesMesh, m_verticesMaterial, new[] { Matrix4x4.identity });
                m_graphicsCamera.MeshesCache.AddBatch(m_edgesMesh, m_edgesMaterial, new[] { Matrix4x4.identity });
                m_graphicsCamera.MeshesCache.Refresh();
            }
            else
            {
                Debug.LogWarning("m_graphicsCamera is null");
            }
            
        }

        private void RefreshColors()
        {
            if(Mode != RenderMode.Edge)
            {
                m_verticesMesh.colors = m_vertexColors;
            }
            
            m_edgesMesh.colors = m_edgeColors;
        }

        private void RefreshVertices(float scale)
        {
            Vector3[] vertices;
            if (Mode == RenderMode.Vertex)
            {
                vertices = m_verticesMesh.vertices;
                for (int i = 0; i < m_uv.Length; ++i)
                {
                    Vector2 uv = m_uv[i];
                    vertices[i] = new Vector3(uv.x * scale, uv.y * scale, m_isSelected[i] ? -1 : 0);
                }
                m_verticesMesh.vertices = vertices;
            } 
            else if(Mode == RenderMode.Face)
            {
                vertices = m_verticesMesh.vertices;
                for (int i = 0; i < Faces.Length; ++i)
                {
                    int[] face = Faces[i].Indexes;
                    Vector2 uv = m_uv[face[0]];
                    for(int j = 1; j < face.Length; ++j)
                    {
                        uv += m_uv[face[j]];
                    }
                    uv /= face.Length;
                    vertices[i] = new Vector3(uv.x * scale, uv.y * scale, m_isSelected[i] ? -1 : 0);
                }
                m_verticesMesh.vertices = vertices;
            }
            
            m_verticesMesh.RecalculateBounds();

            vertices = m_edgesMesh.vertices;
            for (int i = 0; i < m_edges.Length; ++i)
            {
                Vector2 uv0 = m_uv[m_edges[i].A];
                Vector2 uv1 = m_uv[m_edges[i].B];

                bool isManualUV = m_faces[m_edges[i].FaceIndex].IsManualUV;

                vertices[i * 2 + 0] = new Vector3(uv0.x * scale, uv0.y * scale, isManualUV && m_isSelected[m_edges[i].A] ? -1 : 0);
                vertices[i * 2 + 1] = new Vector3(uv1.x * scale, uv1.y * scale, isManualUV && m_isSelected[m_edges[i].B] ? -1 : 0);
            }

            m_edgesMesh.vertices = vertices;
            m_edgesMesh.RecalculateBounds();
        }

        public static Vector2 WorldToUV(Vector3 point)
        {
            Vector2 uv;
            uv.x = point.x / Scale;
            uv.y = point.y / Scale;
            return uv;
        }
    }
}

