using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.Spline3
{
    public class SplineRenderer : MonoBehaviour
    {
        [SerializeField]
        protected float m_step = 0.05f;
        [SerializeField]
        protected float m_normalLength = 0.0f;
        [SerializeField]
        protected Color m_lineColor = Color.green;
        [SerializeField]
        protected Color m_controlPointColor = Color.gray;
        [SerializeField]
        private int m_layer;
        public int Layer
        {
            get { return m_layer; }
            set
            {
                if(m_layer != value)
                {
                    m_layer = value;
                    OnLayerChanged();
                }   
            }
        }

        private BaseSpline m_spline;
        protected BaseSpline Spline
        {
            get { return m_spline; }
        }
        private int m_segCount;
        private int m_perSegCount;
        private int[] m_indexes;
        private MeshFilter m_lineMeshFilter;
        private MeshFilter m_normalMeshFilter;
        private MeshFilter m_pointMeshFilter;

        private Renderer m_lineRenderer;
        private Renderer m_normalRenderer;
        private Renderer m_pointRenderer;

        protected static Material m_lineMaterial;
        protected static Material m_normalMaterial;
        protected static Material m_controlPointMaterial;

        protected virtual void Awake()
        {            
            if(m_lineMaterial == null)
            {
                m_lineMaterial = new Material(Shader.Find("Battlehub/RTCommon/LineBillboard"));
                m_lineMaterial.SetFloat("_Scale", 0.9f);
                m_lineMaterial.SetColor("_Color", Color.white);
                m_lineMaterial.SetInt("_HandleZTest", (int)CompareFunction.Always);
            }

            if(m_normalMaterial == null)
            {
                m_normalMaterial = new Material(Shader.Find("Battlehub/RTCommon/LineBillboard"));
                m_normalMaterial.SetFloat("_Scale", 0.9f);
                m_normalMaterial.SetColor("_Color", Color.white);
                m_normalMaterial.SetInt("_HandleZTest", (int)CompareFunction.Always);
            }

            if(m_controlPointMaterial == null)
            {
                m_controlPointMaterial = new Material(Shader.Find("Hidden/RTHandles/PointBillboard"));
                m_controlPointMaterial.SetFloat("_Scale", 4.5f);
                m_controlPointMaterial.SetColor("_Color", m_controlPointColor);
                m_controlPointMaterial.SetInt("_HandleZTest", (int)CompareFunction.Always);
            }

            InitDrawing("Line", m_lineMaterial, out m_lineMeshFilter, out m_lineRenderer);
            GameObject normals = InitDrawing("Normals", m_normalMaterial, out m_normalMeshFilter, out m_normalRenderer);
            normals.hideFlags = HideFlags.HideAndDontSave;
            GameObject points = InitDrawing("Points", m_controlPointMaterial, out m_pointMeshFilter, out m_pointRenderer);
            points.hideFlags = HideFlags.HideAndDontSave;
        }

        protected GameObject InitDrawing(string name, Material material, out MeshFilter meshFilter, out Renderer renderer)
        {
            GameObject go = new GameObject();
            go.hideFlags = HideFlags.DontSave;
            go.name = name;
            go.transform.SetParent(transform, false);

            meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();
            meshFilter.sharedMesh.MarkDynamic();

            renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;

            return go;
        }

        protected virtual void Start()
        {
            m_spline = GetComponent<BaseSpline>();
            Refresh();
            OnLayerChanged();
        }

        protected virtual void OnDestroy()
        {
            if (m_lineRenderer != null)
            {
                Destroy(m_lineRenderer.gameObject);
            }

            if (m_pointRenderer != null)
            {
                Destroy(m_pointRenderer.gameObject);
            }

            if (m_normalRenderer != null)
            {
                Destroy(m_normalRenderer.gameObject);
            }
        }

        protected virtual void OnEnable()
        {
            if(m_lineRenderer != null)
            {
                m_lineRenderer.enabled = true;
            }
            
            if(m_pointRenderer != null)
            {
                m_pointRenderer.enabled = true;
            }
            
            if(m_normalRenderer != null)
            {
                m_normalRenderer.enabled = true;
            }
        }

        protected virtual void OnDisable()
        {
            if(m_lineRenderer != null)
            {
                m_lineRenderer.enabled = false;
            }
            
            if(m_pointRenderer != null)
            {
                m_pointRenderer.enabled = false;
            }
            
            if(m_normalRenderer != null)
            {
                m_normalRenderer.enabled = false;
            }
        }

        protected virtual void OnLayerChanged()
        {
            if (m_lineMeshFilter != null)
            {
                m_lineMeshFilter.gameObject.layer = m_layer;
            }
            if (m_normalMeshFilter != null)
            {
                m_normalMeshFilter.gameObject.layer = m_layer;
            }
            if (m_pointMeshFilter != null)
            {
                m_pointMeshFilter.gameObject.layer = m_layer;
            }
        }

        public virtual void Refresh(bool positionsOnly = false)
        {
            if(m_spline == null)
            {
                return;
            }

            BuildLineMesh(m_lineMeshFilter.sharedMesh, positionsOnly);
            if(m_normalLength > 0)
            {
                m_normalMeshFilter.gameObject.SetActive(true);
                BuildNormalMesh(m_normalMeshFilter.sharedMesh, positionsOnly);
            }
            else
            {
                m_normalMeshFilter.gameObject.SetActive(false);
            }

            if(!positionsOnly)
            {
                m_indexes = null;
            }
            
            BuildPointsMesh(m_pointMeshFilter.sharedMesh);
        }

        protected void BuildLineMesh(Mesh target, bool positionsOnly)
        {
            int segCount = m_spline.SegmentsCount;
            int perSegCount = Mathf.RoundToInt(1.0f / Mathf.Max(0.0001f, m_step));
            
            if(m_segCount != segCount || m_perSegCount != perSegCount)
            {
                m_segCount = segCount;
                m_perSegCount = perSegCount;
                positionsOnly = false;
            }
            
            if(positionsOnly)
            {
                Vector3[] vertices = target.vertices;
                UpdateLineVertices(segCount, perSegCount, vertices);
                target.vertices = vertices;
                target.RecalculateBounds();
            }
            else
            {
                Vector3[] vertices = new Vector3[segCount * (perSegCount + 1)];
                int[] indexes = new int[(vertices.Length - (m_spline.IsLooping ? 0 : 1)) * 2];
                UpdateLineVertices(segCount, perSegCount, vertices);
                int index = 0;
                for(int i = 0; i < indexes.Length; i+= 2)
                {
                    indexes[i] = index;
                    indexes[i + 1] = (index + 1) % vertices.Length;
                    index++;
                }

                target.Clear();
                target.subMeshCount = 1;

                target.name = "SplineMesh" + target.GetInstanceID();
                Color[] colors = new Color[vertices.Length];
                UpdateColors(segCount, perSegCount, colors);
                
                target.vertices = vertices;
                target.SetIndices(indexes, MeshTopology.Lines, 0);
                target.colors = colors;
                target.RecalculateBounds();
            }
        }

        protected virtual void UpdateColors(int segCount, int perSegCount, Color[] colors)
        {
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = m_lineColor;
            }
        }

        protected void UpdateLineVertices(int segCount, int perSegCount, Vector3[] vertices)
        {
            for (int segIndex = 0; segIndex < segCount; segIndex++)
            {
                for (int offset = 0; offset <= perSegCount; offset++)
                {
                    vertices[segIndex * (perSegCount + 1) + offset] = m_spline.GetLocalPosition(segIndex, (float)offset / perSegCount);
                }
            }
            vertices[vertices.Length - 1] = m_spline.GetLocalPosition(segCount - 1, 1.0f);
        }

        private void BuildNormalMesh(Mesh target, bool positionsOnly)
        {
            int segCount = m_spline.SegmentsCount;
            int perSegCount = Mathf.RoundToInt(1.0f / Mathf.Max(0.0001f, m_step));

            if (m_segCount != segCount || m_perSegCount != perSegCount)
            {
                m_segCount = segCount;
                m_perSegCount = perSegCount;
                positionsOnly = false;
            }

            if (positionsOnly)
            {
                Vector3[] vertices = target.vertices;
                UpdateNormalVertices(segCount, perSegCount, vertices);
                target.vertices = vertices;
                target.RecalculateBounds();
            }
            else
            {
                Vector3[] vertices = new Vector3[segCount * (perSegCount + 1) * 2];
                int[] indexes = new int[vertices.Length];
                UpdateNormalVertices(segCount, perSegCount, vertices);
                for (int i = 0; i < indexes.Length; i++)
                {
                    indexes[i] = i;
                }

                target.Clear();
                target.subMeshCount = 1;

                target.name = "SplineMesh" + target.GetInstanceID();
                Color[] colors = new Color[vertices.Length];

                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i] = m_lineColor;
                }

                target.vertices = vertices;
                target.SetIndices(indexes, MeshTopology.Lines, 0);
                target.colors = colors;
                target.RecalculateBounds();
            }
        }

        private void UpdateNormalVertices(int segCount, int perSegCount, Vector3[] vertices)
        {
            for (int segIndex = 0; segIndex < segCount; segIndex++)
            {
                for (int offset = 0; offset <= perSegCount; offset++)
                {
                    Vector3 position = m_spline.GetLocalPosition(segIndex, (float)offset / perSegCount);
                    Vector3 tangent = m_spline.GetLocalTangent(segIndex, (float)offset / perSegCount).normalized;
                    int index = (segIndex * (perSegCount + 1) + offset) * 2;
                    vertices[index] = position;
                    vertices[index + 1] = position + Vector3.Cross(tangent, Vector3.up) * m_normalLength;
                }
            }

            Vector3 lastPosition = m_spline.GetLocalPosition(segCount - 1, 1.0f);
            Vector3 lastTangent = m_spline.GetLocalTangent(segCount - 1, 1.0f).normalized;
            vertices[vertices.Length - 2] = lastPosition;
            vertices[vertices.Length - 1] = lastPosition + Vector3.Cross(lastTangent, Vector3.up) * m_normalLength;
        }

        private void BuildPointsMesh(Mesh target)
        {
            Vector3[] vertices = m_spline.LocalControlPoints.ToArray();
            if(m_indexes == null)
            {
                m_indexes = new int[0];
            }
            if (m_indexes.Length != vertices.Length)
            {
                System.Array.Resize(ref m_indexes, vertices.Length);
                for(int i = 0; i < m_indexes.Length; ++i)
                {
                    m_indexes[i] = i;
                }

                target.Clear();
                target.subMeshCount = 1;

                Color[] colors = new Color[vertices.Length];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i] = m_controlPointColor;
                }

                if(!m_spline.ShowTerminalPoints && !m_spline.IsLooping)
                {
                    colors[colors.Length - 1] = colors[0] = new Color(0, 0, 0, 0);
                    vertices[0] = vertices[1];
                    vertices[vertices.Length - 1] = vertices[vertices.Length - 2];
                }

                target.vertices = vertices;
                target.SetIndices(m_indexes, MeshTopology.Points, 0);
                target.colors = colors;
                target.RecalculateBounds();
            }
            else
            {
                if (!m_spline.ShowTerminalPoints && !m_spline.IsLooping)
                {
                    vertices[0] = vertices[1];
                    vertices[vertices.Length - 1] = vertices[vertices.Length - 2];
                }

                target.vertices = vertices;
                target.RecalculateBounds();
            }

           
        }
    }
}

