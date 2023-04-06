using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTMeasurement
{
    public class LineStripRenderer : MonoBehaviour
    {
        [SerializeField]
        private Color m_lineColor = new Color32(0x37, 0x73, 0xF4, 0xFF);
        [SerializeField]
        private Color m_controlPointColor = new Color32(0x37, 0x73, 0xF4, 0xFF);

        private int[] m_indexes;
        private MeshFilter m_lineMeshFilter;
        private MeshFilter m_pointMeshFilter;

        private Renderer m_lineRenderer;
        private Renderer m_pointRenderer;

        private static Material m_lineMaterial;
        private static Material m_controlPointMaterial;

        private Vector3[] m_vertices = new Vector3[0];
        public Vector3[] Vertices
        {
            get { return m_vertices; }
            set { m_vertices = value; }
        }

        public bool IsLoop
        {
            get;
            set;
        }

        private void Start()
        {
            if (m_lineMaterial == null)
            {
                m_lineMaterial = new Material(Shader.Find("Battlehub/RTCommon/LineBillboard"));
                m_lineMaterial.SetFloat("_Scale", 1.0f);
                m_lineMaterial.SetColor("_Color", Color.white);
                m_lineMaterial.SetInt("_HandleZTest", (int)CompareFunction.Always);
            }

            if (m_controlPointMaterial == null)
            {
                m_controlPointMaterial = new Material(Shader.Find("Hidden/RTHandles/PointBillboard"));
                m_controlPointMaterial.SetFloat("_Scale", 3.5f);
                m_controlPointMaterial.SetColor("_Color", Color.white);
                m_controlPointMaterial.SetInt("_HandleZTest", (int)CompareFunction.Always);
            }

            GameObject lineGo = new GameObject();
            lineGo.name = "Line";
            lineGo.transform.SetParent(transform, false);
            lineGo.layer = gameObject.layer;

            m_lineMeshFilter = lineGo.AddComponent<MeshFilter>();
            m_lineMeshFilter.sharedMesh = new Mesh();
            m_lineMeshFilter.sharedMesh.MarkDynamic();

            m_lineRenderer = lineGo.AddComponent<MeshRenderer>();
            m_lineRenderer.sharedMaterial = m_lineMaterial;

            GameObject pointsGo = new GameObject();
            pointsGo.name = "Points";
            pointsGo.transform.SetParent(transform, false);
            pointsGo.layer = gameObject.layer;

            m_pointMeshFilter = pointsGo.AddComponent<MeshFilter>();
            m_pointMeshFilter.sharedMesh = new Mesh();
            m_pointMeshFilter.sharedMesh.MarkDynamic();

            m_pointRenderer = pointsGo.AddComponent<MeshRenderer>();
            m_pointRenderer.sharedMaterial = m_controlPointMaterial;

            Refresh();
        }

        private void OnDestroy()
        {
            if (m_lineRenderer != null)
            {
                Destroy(m_lineRenderer.gameObject);
            }

            if (m_pointRenderer != null)
            {
                Destroy(m_pointRenderer.gameObject);
            }
        }

        private void OnEnable()
        {
            if (m_lineRenderer != null)
            {
                m_lineRenderer.enabled = true;
            }

            if (m_pointRenderer != null)
            {
                m_pointRenderer.enabled = true;
            }
        }

        private void OnDisable()
        {
            if (m_lineRenderer != null)
            {
                m_lineRenderer.enabled = false;
            }

            if (m_pointRenderer != null)
            {
                m_pointRenderer.enabled = false;
            }
        }

        public void Refresh(bool positionsOnly = false)
        {
            BuildLineMesh(m_lineMeshFilter.sharedMesh, m_lineColor, positionsOnly);
            BuildPointsMesh(m_pointMeshFilter.sharedMesh, m_controlPointColor);
        }

        private void BuildLineMesh(Mesh target, Color color, bool positionsOnly)
        {
            Vector3[] vertices = Vertices;
            if(vertices == null)
            {
                vertices = new Vector3[0];
            }

            if(target.vertexCount != vertices.Length)
            {
                positionsOnly = false;
            }
            
            if (positionsOnly)
            {
                target.vertices = Vertices;
                target.RecalculateBounds();
            }
            else
            {
                int[] indexes = new int[Mathf.Max(0, (vertices.Length - (IsLoop ? 0 : 1)) * 2)];
                int index = 0;
                for (int i = 0; i < indexes.Length; i += 2)
                {
                    indexes[i] = index;
                    indexes[i + 1] = (index + 1) % vertices.Length;
                    index++;
                }

                target.Clear();
                target.subMeshCount = 1;

                target.name = "SplineMesh" + target.GetInstanceID();
                Color[] colors = new Color[vertices.Length];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i] = color;
                }

                target.vertices = vertices;
                target.SetIndices(indexes, MeshTopology.Lines, 0);
                target.colors = colors;
                target.RecalculateBounds();
            }
        }

        private void BuildPointsMesh(Mesh target, Color color)
        {
            Vector3[] vertices = Vertices;
            if(vertices == null)
            {
                vertices = new Vector3[0];
            }

            if (m_indexes == null)
            {
                m_indexes = new int[0];
            }
            if (m_indexes.Length != vertices.Length)
            {
                System.Array.Resize(ref m_indexes, vertices.Length);
                for (int i = 0; i < m_indexes.Length; ++i)
                {
                    m_indexes[i] = i;
                }

                target.Clear();
                target.subMeshCount = 1;

                Color[] colors = new Color[vertices.Length];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i] = color;
                }

                target.vertices = vertices;
                target.SetIndices(m_indexes, MeshTopology.Points, 0);
                target.colors = colors;
                target.RecalculateBounds();

            }
            else
            {
                target.vertices = vertices;
                target.RecalculateBounds();
            }
        }
    }
}