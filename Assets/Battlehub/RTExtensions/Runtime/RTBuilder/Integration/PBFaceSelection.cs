using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

namespace Battlehub.ProBuilderIntegration
{
    public class FaceList
    {
        public readonly Dictionary<int, int> Indexes;
        public readonly List<int> Faces;
        public readonly List<int[]> FaceIndexes;
        public readonly List<Face> SelectionFaces;

        public FaceList()
        {
            Indexes = new Dictionary<int, int>();
            Faces = new List<int>();
            FaceIndexes = new List<int[]>();
            SelectionFaces = new List<Face>();
        }
    }

    public class PBFaceSelection : MonoBehaviour
    {
        private ProBuilderMesh m_selectionMesh;
        private readonly Dictionary<ProBuilderMesh, Dictionary<int, Face>> m_faceToSelectionFace = new Dictionary<ProBuilderMesh, Dictionary<int, Face>>();
        private readonly Dictionary<Face, ProBuilderMesh> m_selectionFaceToMesh = new Dictionary<Face, ProBuilderMesh>();
        private readonly List<Vector3> m_selectionVertices = new List<Vector3>();
        private readonly List<Face> m_selectionFaces = new List<Face>();
        private readonly Dictionary<ProBuilderMesh, FaceList> m_meshToFaces = new Dictionary<ProBuilderMesh, FaceList>();
        private readonly List<PBMesh> m_pbMeshes = new List<PBMesh>();

        private bool m_isChanging;

        [SerializeField]
        private Color m_color = Color.yellow;
        private Material m_material;

        private Vector3 m_lastPosition;
        public Vector3 LastPosition
        {
            get { return transform.TransformPoint(m_lastPosition); }
        }

        public Vector3 m_lastNormal = Vector3.forward;
        public Vector3 LastNormal
        {
            get { return transform.TransformDirection(m_lastNormal.normalized); }
        }

        private ProBuilderMesh m_lastMesh;
        public ProBuilderMesh LastMesh
        {
            get { return m_lastMesh; }
        }

        private Vector3 m_centerOfMass;
        public Vector3 CenterOfMass
        {
            get { return transform.TransformPoint(m_centerOfMass); }
        }

        public int FacesCount
        {
            get { return m_selectionFaces.Count; }
        }

        public int MeshesCount
        {
            get { return m_meshToFaces.Count; }
        }

        public IEnumerable<ProBuilderMesh> Meshes
        {
            get { return m_meshToFaces.Keys; }
        } 

        public IEnumerable<PBMesh> PBMeshes
        {
            get { return m_pbMeshes; }
        }

        private PBBaseEditor m_editor;

        private MeshRenderer m_renderer;
        public bool IsRendererEnabled
        {
            get { return m_renderer.enabled; }
            set { m_renderer.enabled = value; }
        }
        
        private void Awake()
        {
            m_editor = GetComponent<PBBaseEditor>();

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if(meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
            meshFilter.sharedMesh = new Mesh();

            m_renderer = gameObject.GetComponent<MeshRenderer>();
            if(m_renderer == null)
            {
                m_renderer = gameObject.AddComponent<MeshRenderer>();
            }

            m_material = new Material(Shader.Find("Battlehub/RTBuilder/FaceHighlight"));
            m_material.SetFloat("_Dither", 0.0f);
            m_material.SetInt("_HandleZTest", (int)CompareFunction.LessEqual);
            m_material.color = new Color(m_color.r, m_color.g, m_color.b, 0.5f);

            m_renderer.sharedMaterial = m_material;

            gameObject.AddComponent<PBMesh>();
            gameObject.layer = m_editor.GraphicsLayer;
            m_selectionMesh = GetComponent<ProBuilderMesh>();            
        }

        private void OnDestroy()
        {
            if(m_material != null)
            {
                Destroy(m_material);
            }
            m_selectionMesh = null;
            Clear();
        }

        public bool IsSelected(ProBuilderMesh mesh, int face)
        {
            Dictionary<int, Face> faceToSelection;
            if(!m_faceToSelectionFace.TryGetValue(mesh, out faceToSelection))
            {
                return false;
            }

            return faceToSelection.ContainsKey(face);
        }

        public void BeginChange()
        {
            m_isChanging = true;
        }

        public void EndChange()
        {
            if(m_isChanging)
            {
                RebuildSelectionMesh();
                m_isChanging = false;
            }
        }

        public void Add(ProBuilderMesh mesh, int faceIndex)
        {
            Face face = mesh.faces[faceIndex];

            Dictionary<int, Face> faceToSelection;
            if (m_faceToSelectionFace.TryGetValue(mesh, out faceToSelection))
            {
                if (faceToSelection.ContainsKey(faceIndex))
                {
                    return;
                }
            }
            else
            {
                faceToSelection = new Dictionary<int, Face>();
                m_faceToSelectionFace.Add(mesh, faceToSelection);
            }
            

            int[] indices = new int[face.indexes.Count];
            for(int i = 0; i < indices.Length; ++i)
            {
                indices[i] = m_selectionVertices.Count + i;
            }

            Face selectionFace = new Face();
            selectionFace.SetIndexes(indices);
            faceToSelection.Add(faceIndex, selectionFace);
            m_selectionFaceToMesh.Add(selectionFace, mesh);

            IList<int> indexes = face.indexes;
            Vertex[] vertices = mesh.GetVertices(indexes);
            for(int i = 0; i < vertices.Length; ++i)
            {
                m_selectionVertices.Add(transform.InverseTransformPoint(mesh.transform.TransformPoint(vertices[i].position)));
            }

            m_selectionFaces.Add(selectionFace);
            if(!m_isChanging)
            {
                RebuildSelectionMesh();
            }
            
            FaceList faceList;
            if(!m_meshToFaces.TryGetValue(mesh, out faceList))
            {
                faceList = new FaceList();
                m_meshToFaces.Add(mesh, faceList);

                PBMesh pbMesh = mesh.GetComponent<PBMesh>();
                if(pbMesh != null)
                {
                    pbMesh.RaiseSelected(false);
                }
                m_pbMeshes.Add(pbMesh);
            }

            for (int i = 0; i < indexes.Count; ++i)
            {
                int index = indexes[i];
                if (!faceList.Indexes.ContainsKey(index))
                {
                    faceList.Indexes.Add(index, 1);
                }
                else
                {
                    faceList.Indexes[index]++;
                }
            }

            faceList.Faces.Add(faceIndex);
            faceList.FaceIndexes.Add(face.indexes.ToArray());
            faceList.SelectionFaces.Add(selectionFace);

            m_lastMesh = mesh;
            m_lastPosition = GetCenterOfMass(selectionFace);
            m_lastNormal = GetNormal(selectionFace);

            if (m_selectionFaces.Count == 1)
            {
                m_centerOfMass = m_lastPosition;
            }
            else
            {
                m_centerOfMass *= (m_selectionFaces.Count - 1) / (float)m_selectionFaces.Count;
                m_centerOfMass += m_lastPosition / m_selectionFaces.Count;
            }
        }

        public void Remove(ProBuilderMesh mesh, int faceIndex)
        {
            Face selectionFace;
            Dictionary<int, Face> faceToSelection;
            if (m_faceToSelectionFace.TryGetValue(mesh, out faceToSelection))
            {
                if (!faceToSelection.TryGetValue(faceIndex, out selectionFace))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            Face face = mesh.faces[faceIndex];

            faceToSelection.Remove(faceIndex);
            if(faceToSelection.Count == 0)
            {
                m_faceToSelectionFace.Remove(mesh);
            }

            m_selectionFaceToMesh.Remove(selectionFace);

            FaceList faceList = m_meshToFaces[mesh];
            IList<int> indexes = face.indexes;
            for (int i = 0; i < indexes.Count; ++i)
            {
                int index = indexes[i];
                if (faceList.Indexes.ContainsKey(index))
                {
                    faceList.Indexes[index]--;
                    if (faceList.Indexes[index] == 0)
                    {
                        faceList.Indexes.Remove(index);
                    }
                }
            }

            if(faceList.Faces.Count == 0)
            {
                m_meshToFaces.Remove(mesh);

                PBMesh pbMesh = mesh.GetComponent<PBMesh>();
                if(pbMesh != null)
                {
                    pbMesh.RaiseUnselected();
                }
                
                m_pbMeshes.Remove(pbMesh);
            }

            int flidx = faceList.Faces.IndexOf(faceIndex);
            faceList.Faces.RemoveAt(flidx);
            faceList.FaceIndexes.RemoveAt(flidx);
            faceList.SelectionFaces.Remove(selectionFace);

            Vector3 removedFaceCenterOfMass = GetCenterOfMass(selectionFace);
            int[] indices = selectionFace.distinctIndexes.OrderByDescending(i => i).ToArray();
            for(int i = 0; i < indices.Length; ++i)
            {
                m_selectionVertices.RemoveAt(indices[i]);
            }

            int selectionFaceIndex = m_selectionFaces.IndexOf(selectionFace);
            int count = selectionFace.indexes.Count;

            m_selectionFaces.RemoveAt(selectionFaceIndex);
            for(int i = selectionFaceIndex; i < m_selectionFaces.Count; ++i)
            {
                m_selectionFaces[i].ShiftIndexes(-count);
            }

            if(!m_isChanging)
            {
                RebuildSelectionMesh();
            }

            if(m_selectionFaces.Count == 0)
            {
                m_centerOfMass = Vector3.zero;
                m_lastPosition = Vector3.zero;
                m_lastNormal = Vector3.forward;
                m_lastMesh = null;
            }
            else if (m_selectionFaces.Count == 1)
            {
                m_centerOfMass = GetCenterOfMass(m_selectionFaces[0]);
                m_lastPosition = m_centerOfMass;
                m_lastNormal = GetNormal(m_selectionFaces[0]);
                m_lastMesh = m_selectionFaceToMesh[m_selectionFaces[0]];
            }
            else
            {
                m_centerOfMass -= removedFaceCenterOfMass / (m_selectionFaces.Count + 1);
                m_centerOfMass *= (m_selectionFaces.Count + 1) / (float)m_selectionFaces.Count;
                m_lastPosition = GetCenterOfMass(m_selectionFaces.Last());
                m_lastNormal = GetNormal(m_selectionFaces.Last());
                m_lastMesh = m_selectionFaceToMesh[m_selectionFaces.Last()];
            }
        }

        public void Clear()
        {
            for(int i = 0; i < m_pbMeshes.Count; ++i)
            {
                PBMesh pbMesh = m_pbMeshes[i];
                if(pbMesh != null)
                {
                    pbMesh.RaiseUnselected();
                }
            }

            m_selectionVertices.Clear();
            m_selectionFaces.Clear();
            m_faceToSelectionFace.Clear();
            m_selectionFaceToMesh.Clear();
            m_meshToFaces.Clear();
            m_pbMeshes.Clear();
            RebuildSelectionMesh();
        }

        public Vector3 GetCenterOfMass()
        {
            Vector3 centerOfMass = GetCenterOfMass(m_selectionFaces[0]);
            for(int i = 1; i < m_selectionFaces.Count; ++i)
            {
                Face face = m_selectionFaces[i];
                centerOfMass += GetCenterOfMass(face);
            }

            return centerOfMass / m_selectionFaces.Count;
        }

        private Vector3 GetCenterOfMass(Face face)
        {
            IList<int> indexes = face.indexes;
            Vector3 result = m_selectionVertices[indexes[0]];
            for(int i = 1; i < indexes.Count; ++i)
            {
                result += m_selectionVertices[indexes[i]];
            }
            result /= indexes.Count;
            return result;
        }

        private Vector3 GetNormal(Face face)
        {
            IList<int> indexes = face.indexes;

            return Math.Normal(
                m_selectionVertices[indexes[0]],
                m_selectionVertices[indexes[1]],
                m_selectionVertices[indexes[2]]);
        }

        public IList<int> GetFaces(ProBuilderMesh mesh)
        {
            FaceList faces;
            if(m_meshToFaces.TryGetValue(mesh, out faces))
            {
                return faces.Faces;
            }

            return new int[0];
        }

        public IEnumerable<int> GetIndexes(ProBuilderMesh mesh)
        {
            return m_meshToFaces[mesh].Indexes.Keys;
        }

        public void Synchronize(Vector3 centerOfMass, Vector3 lastPosition)
        {
            foreach (KeyValuePair<ProBuilderMesh, FaceList> kvp in m_meshToFaces)
            {
                ProBuilderMesh mesh = kvp.Key;
                FaceList faces = kvp.Value;

                for (int f = 0; f < faces.Faces.Count; ++f)
                {
                    int[] faceIndexes = faces.FaceIndexes[f];
                    Face selectionFace = faces.SelectionFaces[f];

                    Vertex[] vertices = mesh.GetVertices(faceIndexes);
                    IList<int> selectionIndexes = selectionFace.indexes;
                    for (int i = 0; i < vertices.Length; ++i)
                    {
                        int selectionIndex = selectionIndexes[i];
                        m_selectionVertices[selectionIndex] = transform.InverseTransformPoint(mesh.transform.TransformPoint(vertices[i].position));
                    }
                }
            }

            m_centerOfMass = transform.InverseTransformPoint(centerOfMass);
            m_lastPosition = transform.InverseTransformPoint(lastPosition);
            m_lastNormal = m_selectionFaces.Count == 0 ? Vector3.forward : GetNormal(m_selectionFaces[m_selectionFaces.Count - 1]); // transform.InverseTransformDirection(lastNormal.normalized);
            RebuildSelectionMesh();
        }

        private void RebuildSelectionMesh()
        {
            if(m_selectionMesh == null)
            {
                return;
            }
            m_selectionMesh.RebuildWithPositionsAndFaces(m_selectionVertices, m_selectionFaces);
            m_selectionMesh.ToMesh();
            m_selectionMesh.Refresh();
        }
    }
}


