using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.ProBuilderIntegration
{
    public class PBComplexShapeSelection : MonoBehaviour
    {
        [SerializeField]
        private Color m_edgesColor = Color.yellow;
        [SerializeField]
        private Color m_vertexColor = new Color(0.33f, 0.33f, 0.33f);
        [SerializeField]
        private Color m_vertexSelectedColor = Color.yellow;
        [SerializeField]
        private Color m_vertexHoverColor = new Color(1, 1, 0, 0.75f);
        [SerializeField]
        private float m_vertexScale = 3.3f;
        [SerializeField]
        private float m_edgeScale = 1.0f;

        private readonly List<Vector3> m_positions = new List<Vector3>();
        public IList<Vector3> Positions
        {
            get { return m_positions; }
        }

        public Transform Transform
        {
            get { return m_polyShapeVertices.transform; }
        }

        private int m_hoveredIndex = -1;
        private int m_selectedIndex = -1;
        public int SelectedIndex
        {
            get { return m_selectedIndex; }
        }

        private MeshFilter m_polyShapeVertices;
        private MeshFilter m_polyShapeEdges;

        private void Awake()
        {
            GameObject verticesGo;
            GameObject edgesGo;

            verticesGo = new GameObject("polyshapeVertices");
            verticesGo.transform.SetParent(transform, false);
            verticesGo.AddComponent<MeshFilter>();
            verticesGo.AddComponent<MeshRenderer>();
            verticesGo.hideFlags = HideFlags.HideInHierarchy;

            edgesGo = new GameObject("polyshapeEdges");
            edgesGo.transform.SetParent(transform, false);
            edgesGo.AddComponent<MeshFilter>();
            edgesGo.AddComponent<MeshRenderer>();
            edgesGo.hideFlags = HideFlags.HideInHierarchy;

            m_polyShapeVertices = verticesGo.GetComponent<MeshFilter>();
            m_polyShapeVertices.mesh = new Mesh();

            m_polyShapeEdges = edgesGo.GetComponent<MeshFilter>();
            m_polyShapeEdges.mesh = new Mesh();

            Renderer renderer = verticesGo.GetComponent<MeshRenderer>();
            string vertShader = PBBuiltinMaterials.geometryShadersSupported ?
               PBBuiltinMaterials.pointShader :
               PBBuiltinMaterials.dotShader;
            renderer.sharedMaterial = new Material(Shader.Find(vertShader));
            renderer.sharedMaterial.SetFloat("_Scale", m_vertexScale);

            renderer = edgesGo.GetComponent<MeshRenderer>();
            string edgeShader = PBBuiltinMaterials.lineShader;
            renderer.sharedMaterial = new Material(Shader.Find(edgeShader));
            renderer.sharedMaterial.SetFloat("_Scale", m_edgeScale);
        }

        private void OnDestroy()
        {
            Destroy(m_polyShapeVertices.gameObject);
            Destroy(m_polyShapeEdges.gameObject);
        }

        private void OnEnable()
        {
            m_polyShapeVertices.gameObject.SetActive(true);
            m_polyShapeEdges.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (m_polyShapeVertices != null)
            {
                m_polyShapeVertices.gameObject.SetActive(false);
            }

            if (m_polyShapeEdges != null)
            {
                m_polyShapeEdges.gameObject.SetActive(false);
            }
        }

        public void Add(Vector3 position)
        {
            m_positions.Add(position);

            BuildVertexMesh(m_positions, m_polyShapeVertices.sharedMesh);
            BuildEdgeMesh(m_positions, m_polyShapeEdges.sharedMesh, false);
            SetVertexColors();
        }

        public void Insert(int index, Vector3 position)
        {
            m_positions.Insert(index, position);
            BuildVertexMesh(m_positions, m_polyShapeVertices.sharedMesh);
            BuildEdgeMesh(m_positions, m_polyShapeEdges.sharedMesh, false);

            if (m_hoveredIndex >= index)
            {
                m_hoveredIndex++;
            }

            if (m_selectedIndex >= index)
            {
                m_selectedIndex++;
            }
            SetVertexColors();
        }

        public void RemoveAt(int index)
        {
            if (index < m_hoveredIndex)
            {
                m_hoveredIndex--;
            }

            if (index < m_selectedIndex)
            {
                m_selectedIndex--;
            }

            m_positions.RemoveAt(index);
            BuildVertexMesh(m_positions, m_polyShapeVertices.sharedMesh);
            BuildEdgeMesh(m_positions, m_polyShapeEdges.sharedMesh, false);

            SetVertexColors();
        }

        private void SetVertexColors()
        {
            if (m_hoveredIndex >= 0 && m_hoveredIndex < m_polyShapeVertices.sharedMesh.vertexCount)
            {
                SetVerticesColor(m_polyShapeVertices, m_vertexHoverColor, new[] { m_hoveredIndex });
            }

            if (m_selectedIndex >= 0 && m_selectedIndex < m_polyShapeVertices.sharedMesh.vertexCount)
            {
                SetVerticesColor(m_polyShapeVertices, m_vertexSelectedColor, new[] { m_selectedIndex });
            }
        }


        public void Hover(int index)
        {
            Leave();
            SetVerticesColor(m_polyShapeVertices, m_vertexHoverColor, new[] { index });
        }

        public void Leave()
        {
            if (m_hoveredIndex >= 0)
            {
                if (m_selectedIndex == m_hoveredIndex)
                {
                    SetVerticesColor(m_polyShapeVertices, m_vertexSelectedColor, new[] { m_hoveredIndex });
                }
                else
                {
                    SetVerticesColor(m_polyShapeVertices, m_vertexColor, new[] { m_hoveredIndex });
                }

                m_hoveredIndex = -1;
            }
        }

        public void Select(int index)
        {
            Unselect();
            SetVerticesColor(m_polyShapeVertices, m_vertexSelectedColor, new[] { index });
            m_selectedIndex = index;
        }

        public void Unselect()
        {
            if (m_selectedIndex >= 0)
            {
                if (m_selectedIndex == m_hoveredIndex)
                {
                    SetVerticesColor(m_polyShapeVertices, m_vertexHoverColor, new[] { m_selectedIndex });
                }
                else
                {
                    SetVerticesColor(m_polyShapeVertices, m_vertexColor, new[] { m_selectedIndex });
                }

                m_selectedIndex = -1;
            }
        }

        public void Refersh()
        {
            BuildVertexMesh(m_positions, m_polyShapeVertices.sharedMesh);
            BuildEdgeMesh(m_positions, m_polyShapeEdges.sharedMesh, false);
            if (m_selectedIndex >= 0)
            {
                SetVerticesColor(m_polyShapeVertices, m_vertexSelectedColor, new[] { m_selectedIndex });
            }
        }

        public void Clear()
        {
            m_positions.Clear();
            BuildVertexMesh(m_positions, m_polyShapeVertices.sharedMesh);
            BuildEdgeMesh(m_positions, m_polyShapeEdges.sharedMesh, false);
        }

        private void SetVerticesColor(MeshFilter vertices, Color color, IEnumerable<int> indices)
        {
            if (PBBuiltinMaterials.geometryShadersSupported)
            {
                foreach (int index in indices)
                {
                    Color[] colors = vertices.sharedMesh.colors;
                    colors[index] = color;
                    vertices.sharedMesh.colors = colors;
                }
            }
            else
            {
                foreach (int index in indices)
                {
                    Color[] colors = vertices.sharedMesh.colors;

                    colors[index * 4] = color;
                    colors[index * 4 + 1] = color;
                    colors[index * 4 + 2] = color;
                    colors[index * 4 + 3] = color;

                    vertices.sharedMesh.colors = colors;
                }
            }
        }

        private void BuildVertexMesh(IList<Vector3> positions, Mesh target)
        {
            PBUtility.BuildVertexMesh(positions, m_vertexColor, target);
        }

        private void BuildEdgeMesh(IList<Vector3> positions, Mesh target, bool positionsOnly)
        {
            int edgeCount = positions.Count;

            int[] tris;
            if (positionsOnly)
            {
                tris = null;
            }
            else
            {
                tris = new int[edgeCount * 2];
            }

            if (!positionsOnly)
            {
                for (int i = 0; i < edgeCount; ++i)
                {
                    tris[i * 2 + 0] = i + 0;
                    tris[i * 2 + 1] = (i + 1) % edgeCount;
                }

                target.Clear();
                target.name = "EdgeMesh" + target.GetInstanceID();
                target.vertices = positions.ToArray();
                Color[] colors = new Color[target.vertexCount];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i] = m_edgesColor;
                }
                target.colors = colors;
                target.subMeshCount = 1;
                target.SetIndices(tris, MeshTopology.Lines, 0);
            }
            else
            {
                target.vertices = positions.ToArray();
            }
        }
    }
}