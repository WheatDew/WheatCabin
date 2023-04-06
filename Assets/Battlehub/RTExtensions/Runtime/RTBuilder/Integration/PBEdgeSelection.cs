using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

namespace Battlehub.ProBuilderIntegration
{
    public struct Vector3Tuple
    {
        public Vector3 a;
        public Vector3 b;
        
        public Vector3Tuple(Vector3 a, Vector3 b)
        {
            this.a = a;
            this.b = b;
        }

        public bool Equals(Vector3Tuple other)
        {
            return (a == other.a && b == other.b) ||
                   (a == other.b && b == other.a);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector3Tuple && Equals((Vector3Tuple)obj);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (a.GetHashCode() + b.GetHashCode());
                return hash;
            }
        }
    }

    public class PBEdgeSelection : MonoBehaviour
    {
        [SerializeField]
        private Color m_color = new Color(0.33f, 0.33f, 0.33f);
        [SerializeField]
        private Color m_selectedColor = Color.yellow;
        [SerializeField]
        private Color m_hoverColor = new Color(1, 1, 0, 0.75f);

        [SerializeField]
        private float m_scale = 1.0f;

        [SerializeField]
        private CompareFunction m_zTest = CompareFunction.LessEqual;
        private Material m_material;
        private readonly Dictionary<ProBuilderMesh, MeshFilter> m_meshToSelection = new Dictionary<ProBuilderMesh, MeshFilter>();
        private readonly Dictionary<ProBuilderMesh, HashSet<Edge>> m_meshToEdges = new Dictionary<ProBuilderMesh, HashSet<Edge>>();
        private readonly Dictionary<ProBuilderMesh, List<Edge>> m_meshToEdgesList = new Dictionary<ProBuilderMesh, List<Edge>>();
        private readonly List<ProBuilderMesh> m_meshes = new List<ProBuilderMesh>();
        private readonly List<PBMesh> m_pbMeshes = new List<PBMesh>();
        private readonly Dictionary<Edge, List<int>> m_edgeToSelection = new Dictionary<Edge, List<int>>();
        private readonly Dictionary<Edge, HashSet<Edge>> m_coincidentEdges = new Dictionary<Edge, HashSet<Edge>>();

        private Vector3 m_lastPosition;
        public Vector3 LastPosition
        {
            get { return LastMesh != null ? LastMesh.transform.TransformPoint(m_lastPosition) : Vector3.zero; }
        }

     
        public Vector3 m_lastNormal = Vector3.forward;
        public Vector3 LastNormal
        {
            get { return LastMesh != null ? LastMesh.transform.TransformDirection(m_lastNormal.normalized) : Vector3.zero; }
        }

        public ProBuilderMesh LastMesh
        {
            get
            {
                if (m_meshes.Count == 0)
                {
                    return null;
                }

                return m_meshes.Last();
            }
        }

        private Vector3 m_centerOfMass;
        public Vector3 CenterOfMass
        {
            get { return m_centerOfMass; }
        }

        private int m_selectedEdgesCount;
        public int EdgesCount
        {
            get { return m_selectedEdgesCount; }
        }

        public int MeshesCount
        {
            get { return m_meshes.Count; }
        }

        public IEnumerable<ProBuilderMesh> Meshes
        {
            get { return m_meshes; }
        }

        public IEnumerable<PBMesh> PBMeshes
        {
            get { return m_pbMeshes; }
        }

        private bool IsGeometryShadersSupported
        {
            get { return PBBuiltinMaterials.geometryShadersSupported; }
        }

        private PBBaseEditor m_editor;

        private void Awake()
        {
            m_editor = GetComponent<PBBaseEditor>();

            m_material = new Material(PBBuiltinMaterials.LinesMaterial);
            m_material.SetColor("_Color", Color.white);
            m_material.SetInt("_HandleZTest", (int)m_zTest);
            m_material.SetFloat("_Scale", m_scale);
        }

        private void OnDestroy()
        {
            if (m_material != null)
            {
                Destroy(m_material);
            }

            for(int i = 0; i < m_pbMeshes.Count; ++i)
            {
                PBMesh pbMesh = m_pbMeshes[i];
                if (pbMesh != null)
                {
                    pbMesh.RaiseUnselected();
                }
            }

            m_meshToSelection.Clear();
            m_meshToEdges.Clear();
            m_meshToEdgesList.Clear();
            m_meshes.Clear();
            m_pbMeshes.Clear();
            m_edgeToSelection.Clear();
            m_coincidentEdges.Clear();
        }

        public bool IsSelected(ProBuilderMesh mesh, Edge edge)
        {
            HashSet<Edge> edgeHs;
            if (m_meshToEdges.TryGetValue(mesh, out edgeHs))
            {
                return edgeHs.Contains(edge);
            }
            return false;
        }

        public int GetEdgesCount(ProBuilderMesh mesh)
        {
            return m_meshToEdgesList[mesh].Count;
        }

        public IList<Edge> GetEdges(ProBuilderMesh mesh)
        {
            List<Edge> edges;
            if(m_meshToEdgesList.TryGetValue(mesh, out edges))
            {
                return edges;
            }
            return null;
        }

        public IList<Edge> GetCoincidentEdges(IEnumerable<Edge> edges)
        {
            HashSet<Edge> result = new HashSet<Edge>();
            if(edges == null)
            {
                return null;
            }
            foreach(Edge edge in edges)
            {
                if(m_coincidentEdges.ContainsKey(edge))
                {
                    HashSet<Edge> coincidentEdges = m_coincidentEdges[edge];
                    foreach (Edge coincident in coincidentEdges)
                    {
                        if (!result.Contains(coincident))
                        {
                            result.Add(coincident);
                        }
                    }
                }
                else
                {
                    result.Add(edge);
                }   
            }
            return result.ToArray();
        }

        public void Clear()
        {
            for (int i = 0; i < m_meshes.Count; ++i)
            {
                ProBuilderMesh mesh = m_meshes[i];
                MeshFilter filter = m_meshToSelection[mesh];
                if(filter != null)
                {
                    Destroy(filter.gameObject);
                }
                
                PBMesh pbMesh = m_pbMeshes[i];
                if(pbMesh != null)
                {
                    pbMesh.RaiseUnselected();
                }
            }

            m_meshToSelection.Clear();
            m_meshToEdges.Clear();
            m_meshToEdgesList.Clear();
            m_meshes.Clear();
            m_pbMeshes.Clear();
            m_edgeToSelection.Clear();
            m_coincidentEdges.Clear();

            m_selectedEdgesCount = 0;
            m_centerOfMass = Vector3.zero;
            m_lastNormal = Vector3.forward;
            m_lastPosition = Vector3.zero;
        }

        public void Add(ProBuilderMesh mesh, IEnumerable<Edge> edges)
        {
            HashSet<Edge> edgesHs;
            List<Edge> edgesList;
            MeshFilter edgesSelection;

            m_meshToSelection.TryGetValue(mesh, out edgesSelection);
            m_meshToEdgesList.TryGetValue(mesh, out edgesList);
            if (!m_meshToEdges.TryGetValue(mesh, out edgesHs))
            {
                edgesSelection = CreateEdgesGameObject(mesh);
                edgesSelection.transform.SetParent(mesh.transform, false);

                edgesHs = new HashSet<Edge>();
                edgesList = new List<Edge>();

                m_meshToSelection.Add(mesh, edgesSelection);
                m_meshToEdges.Add(mesh, edgesHs);
                m_meshToEdgesList.Add(mesh, edgesList);
                m_meshes.Add(mesh);

                PBMesh pbMesh = mesh.GetComponent<PBMesh>();
                if (pbMesh != null)
                {
                    pbMesh.RaiseSelected(true);
                }
                m_pbMeshes.Add(pbMesh);
            }

            int vertexCount = mesh.vertexCount;
            Edge[] notSelectedEdges = edges.Where(edge => !edgesHs.Contains(edge) && vertexCount > edge.a && vertexCount > edge.b).ToArray();
            SetEdgesColor(mesh, edgesSelection, m_selectedColor, notSelectedEdges);
            for (int i = 0; i < notSelectedEdges.Length; ++i)
            {
                edgesHs.Add(notSelectedEdges[i]);
                edgesList.Add(notSelectedEdges[i]);
            }

            if (notSelectedEdges.Length > 0)
            {
                m_lastPosition = GetPosition(mesh, notSelectedEdges.Last());
                m_lastNormal = GetNormal(mesh, notSelectedEdges.Last());

                for (int i = 0; i < notSelectedEdges.Length; i++)
                {
                    m_selectedEdgesCount++;
                    if (m_selectedEdgesCount == 1)
                    {
                        m_centerOfMass = mesh.transform.TransformPoint(GetPosition(mesh, notSelectedEdges[i]));
                    }
                    else
                    {
                        m_centerOfMass *= (m_selectedEdgesCount - 1) / (float)m_selectedEdgesCount;
                        m_centerOfMass += mesh.transform.TransformPoint(GetPosition(mesh, notSelectedEdges[i])) / m_selectedEdgesCount;
                    }
                }
            }
        }

        public void Remove(ProBuilderMesh mesh, IEnumerable<Edge> edges = null)
        {
            HashSet<Edge> edgesHs;
            if (m_meshToEdges.TryGetValue(mesh, out edgesHs))
            {
                MeshFilter edgesSelection = m_meshToSelection[mesh];
                List<Edge> edgesList = m_meshToEdgesList[mesh];
                if (edges != null)
                {
                    Edge[] selectedEdges = edges.Where(i => edgesHs.Contains(i)).ToArray();
                    SetEdgesColor(mesh, edgesSelection, m_color, selectedEdges);
                    for (int i = 0; i < selectedEdges.Length; ++i)
                    {
                        edgesHs.Remove(selectedEdges[i]);
                        edgesList.Remove(selectedEdges[i]);
                    }

                    UpdateCenterOfMassOnRemove(mesh, selectedEdges);
                }
                else
                {
                    UpdateCenterOfMassOnRemove(mesh, edgesList);

                    edgesHs.Clear();
                    edgesList.Clear();
                }

                if (edgesHs.Count == 0)
                {
                    m_meshToEdges.Remove(mesh);
                    m_meshToEdgesList.Remove(mesh);
                    m_meshToSelection.Remove(mesh);

                    Destroy(edgesSelection.gameObject);

                    int meshIndex = m_meshes.IndexOf(mesh);
                    if(meshIndex != -1)
                    {
                        m_meshes.RemoveAt(meshIndex);

                        PBMesh pbMesh = m_pbMeshes[meshIndex];
                        if (pbMesh != null)
                        {
                            pbMesh.RaiseUnselected();
                        }
                        m_pbMeshes.RemoveAt(meshIndex);
                    }
                }

                if (edgesList.Count > 0)
                {
                    m_lastPosition = GetPosition(mesh, edgesList.Last());
                    m_lastNormal = GetNormal(mesh, edgesList.Last());
                }
                else
                {
                    if (LastMesh != null)
                    {
                        m_lastPosition = GetPosition(LastMesh, m_meshToEdgesList[LastMesh].Last());
                        m_lastNormal = GetNormal(LastMesh, m_meshToEdgesList[LastMesh].Last());
                    }
                    else
                    {
                        m_lastPosition = Vector3.zero;
                        m_lastPosition = Vector3.forward;
                    }
                }
            }
        }

        private void UpdateCenterOfMassOnRemove(ProBuilderMesh mesh,  IList<Edge> selectedEdges)
        {
            for (int i = selectedEdges.Count - 1; i >= 0; --i)
            {
                m_selectedEdgesCount--;
                if (m_selectedEdgesCount == 0)
                {
                    m_centerOfMass = mesh.transform.position;
                }
                else if (m_selectedEdgesCount == 1)
                {
                    m_centerOfMass = mesh.transform.TransformPoint(GetPosition(mesh, selectedEdges[0]));
                }
                else
                {
                    m_centerOfMass -= mesh.transform.TransformPoint(GetPosition(mesh, selectedEdges[i])) / (m_selectedEdgesCount + 1);
                    m_centerOfMass *= (m_selectedEdgesCount + 1) / (float)m_selectedEdgesCount;
                }
            }
        }

        public Vector3 GetPosition(ProBuilderMesh mesh, Edge edge)
        {
            Vertex[] vertices = mesh.GetVertices(new[] { edge.a, edge.b });
            return (vertices[0].position + vertices[1].position) * 0.5f;
        }

        public Vector3 GetNormal(ProBuilderMesh mesh, Edge edge)
        {
            Vertex[] vertices = mesh.GetVertices(new[] { edge.a, edge.b });
            return (vertices[0].normal + vertices[1].normal) * 0.5f;
        }

        private void SetEdgesColor(ProBuilderMesh mesh, MeshFilter selection, Color color, IEnumerable<Edge> edges)
        {
            Color[] colors = selection.sharedMesh.colors;
            foreach (Edge edge in edges)
            {
                if(m_coincidentEdges.ContainsKey(edge))
                {
                    HashSet<Edge> coincidentEdges = m_coincidentEdges[edge];
                    foreach (Edge coincidentEdge in coincidentEdges)
                    {
                        List<int> indices;
                        if(m_edgeToSelection.TryGetValue(coincidentEdge, out indices))
                        {
                            for (int i = 0; i < indices.Count; ++i)
                            {
                                colors[indices[i]] = color;
                            }
                        }
                    }
                }
                else
                {
                    if(m_edgeToSelection.ContainsKey(edge))
                    {
                        List<int> indices = m_edgeToSelection[edge];
                        for (int i = 0; i < indices.Count; ++i)
                        {
                            colors[indices[i]] = color;
                        }
                    }
                }
            }
            selection.sharedMesh.colors = colors;
        }

        private MeshFilter CreateEdgesGameObject(ProBuilderMesh mesh)
        {
            GameObject edgesSelection = new GameObject("Edges");
            edgesSelection.hideFlags = HideFlags.DontSave;
            edgesSelection.layer = m_editor.GraphicsLayer;

            MeshFilter meshFilter = edgesSelection.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = edgesSelection.AddComponent<MeshFilter>();
            }
            meshFilter.sharedMesh = new Mesh();

            MeshRenderer renderer = edgesSelection.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = edgesSelection.AddComponent<MeshRenderer>();
            }
            renderer.sharedMaterial = m_material;
            FindCoincidentEdges(mesh);
            BuildEdgeMesh(mesh, meshFilter.sharedMesh, false);
            CopyTransform(mesh.transform, meshFilter.transform);
            return meshFilter;
        }

        public void FindCoincidentEdges(ProBuilderMesh mesh)
        {
            if (m_coincidentEdges.Count != 0)
            {
                return;
            }

            Dictionary<Vector3Tuple, HashSet<Edge>> coordinateToEdges = new Dictionary<Vector3Tuple, HashSet<Edge>>();
            IList<Face> faces = mesh.faces;
            IList<Vector3> positions = mesh.positions;
            int faceCount = mesh.faceCount;
            for (int i = 0; i < faceCount; i++)
            {
                ReadOnlyCollection<Edge> edges = faces[i].edges;
                for (int n = 0; n < edges.Count; n++)
                {
                    Edge edge = edges[n];
                    Vector3Tuple tuple = new Vector3Tuple(positions[edge.a], positions[edge.b]);
                    HashSet<Edge> hs;
                    if (!coordinateToEdges.TryGetValue(tuple, out hs))
                    {
                        hs = new HashSet<Edge>();
                        coordinateToEdges.Add(tuple, hs);
                    }
                    if (!hs.Contains(edge))
                    {
                        hs.Add(edge);
                    }
                }
            }

            m_coincidentEdges.Clear();
            foreach (HashSet<Edge> edges in coordinateToEdges.Values)
            {
                foreach (Edge edge in edges)
                {
                    m_coincidentEdges.Add(edge, edges);
                }
            }
        }

        private void BuildEdgeMesh(ProBuilderMesh mesh, Mesh target, bool positionsOnly)
        {
            IList<Vector3> positions = mesh.positions;

            int edgeIndex = 0;
            int edgeCount = 0;
            int faceCount = mesh.faceCount;

            IList<Face> faces = mesh.faces;
            for (int i = 0; i < faceCount; i++)
            {
                edgeCount += faces[i].edges.Count;
            }
            edgeCount = System.Math.Min(edgeCount, int.MaxValue / 2 - 1);

            int[] tris;
            Vector3[] vertices;
            if(positionsOnly)
            {
                vertices = target.vertices;
                tris = null;
            }
            else
            {
                tris = new int[edgeCount * 2];
                vertices = new Vector3[edgeCount * 2];
                m_edgeToSelection.Clear();
            }

            for (int i = 0; i < faceCount && edgeIndex < edgeCount; i++)
            {
                ReadOnlyCollection<Edge> edges = faces[i].edges;
                for (int n = 0; n < edges.Count && edgeIndex < edgeCount; n++)
                {
                    Edge edge = edges[n];

                    int positionIndex = edgeIndex * 2;

                    vertices[positionIndex + 0] = positions[edge.a];
                    vertices[positionIndex + 1] = positions[edge.b];

                    if(!positionsOnly)
                    {
                        tris[positionIndex + 0] = positionIndex + 0;
                        tris[positionIndex + 1] = positionIndex + 1;

                        List<int> list;
                        if (!m_edgeToSelection.TryGetValue(edge, out list))
                        {
                            list = new List<int>();
                            m_edgeToSelection.Add(edge, list);
                        }
                        list.Add(positionIndex + 0);
                        list.Add(positionIndex + 1);
                    }
                 
                    edgeIndex++;
                }
            }

            if(!positionsOnly)
            {
                target.Clear();
                target.name = "EdgeMesh" + target.GetInstanceID();
                target.vertices = vertices.ToArray();
                Color[] colors = new Color[target.vertexCount];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i] = m_color;
                }
                target.colors = colors;
                target.subMeshCount = 1;
                target.SetIndices(tris, MeshTopology.Lines, 0);
            }
            else
            {
                target.vertices = vertices.ToArray();
            }  
        }

        private static void CopyTransform(Transform src, Transform dst)
        {
            //dst.position = src.position;
            //dst.rotation = src.rotation;
            //dst.localScale = src.localScale;
        }

        public void Synchronize(Vector3 centerOfMass, Vector3 lastPosition, Vector3 lastNormal)
        {
            foreach (KeyValuePair<ProBuilderMesh, HashSet<Edge>> kvp in m_meshToEdges)
            {
                ProBuilderMesh mesh = kvp.Key;

                MeshFilter meshFilter = m_meshToSelection[mesh];
                BuildEdgeMesh(mesh, meshFilter.sharedMesh, true);

                meshFilter.transform.position = mesh.transform.position;
            }

            m_centerOfMass = centerOfMass;
            m_lastNormal = LastMesh.transform.InverseTransformDirection(lastNormal.normalized);
            m_lastPosition = LastMesh.transform.InverseTransformPoint(lastPosition);

        }
    }
}


