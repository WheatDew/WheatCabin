using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.MeshTools
{
    public struct TBNBasis
    {
        public Vector3 Tangent;
        public Vector3 Binormal;
        public Vector3 Normal;
    }

    public class Face
    {
        public int Index;

        public Vector3 V0;
        public Vector3 V1;
        public Vector3 V2;
        public Vector3 Position;

        public Vector2 UV0;
        public Vector2 UV1;
        public Vector2 UV2;

        public Face() { }
                
        public Face(int index, Vector3 v0, Vector3 v1, Vector3 v2, Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            Index = index;
            V0 = v0;
            V1 = v1;
            V2 = v2;
            UV0 = uv0;
            UV1 = uv1;
            UV2 = uv2;
            Position = (v0 + v1 + v2) / 3;
        }

        public Face(Face face)
        {
            Index = face.Index;
            V0 = face.V0;
            V1 = face.V1;
            V2 = face.V2;
            UV0 = face.UV0;
            UV1 = face.UV1;
            UV2 = face.UV2;
            Position = face.Position;
        }

        public void GetFaceTBNBasis(out TBNBasis basis)
        {
            Vector3 p21 = V1 - V0;  //p2-p1
            Vector3 p31 = V2 - V0;  //p3-p1
            Vector2 uv21 = UV1 - UV0; //uv2-uv1
            Vector2 uv31 = UV2 - UV0; //uv3-uv1

            float f = uv21.x * uv31.y - uv21.y * uv31.x;
            f = (f == 0) ? 1 : 1 / f;

            basis = new TBNBasis();
            /*
            basis.Tangent = ((p21 * uv31.y - p31 * uv21.y) * f).normalized;
            basis.Binormal = ((p31 * uv21.x - p21 * uv31.x) * f).normalized;
            */
            basis.Binormal = ((p21 * uv31.y - p31 * uv21.y) * f);//.normalized;
            basis.Tangent = ((p31 * uv21.x - p21 * uv31.x) * f);//.normalized;
            basis.Normal = Vector3.Cross(basis.Tangent, basis.Binormal);//.normalized;
            
            /*
            //Gram-Schmidt orthogonalization
            basis.Tangent -= (basis.Normal * Vector3.Dot(basis.Normal, basis.Tangent));
            basis.Tangent.Normalize();

            //Right handed TBN space ?
            bool rigthHanded = Vector3.Dot(Vector3.Cross(basis.Tangent, basis.Binormal), basis.Normal) >= 0;
            basis.Binormal = Vector3.Cross(basis.Normal, basis.Tangent);
            if (!rigthHanded)
            {
                basis.Binormal *= -1;
            }*/
        }
    }

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class FaceSelection : MonoBehaviour
    {
        private MeshFilter m_meshFilter;
        private MeshRenderer m_meshRenderer;
        private Dictionary<int, Face> m_indexToFace = new Dictionary<int, Face>();

        public ICollection<Face> SelectedFaces
        {
            get { return m_indexToFace.Values; }
            set
            {
                m_indexToFace.Clear();
                if(value != null)
                {
                    foreach(Face face in value)
                    {
                        if(!m_indexToFace.ContainsKey(face.Index))
                        {
                            m_indexToFace.Add(face.Index, face);
                        }
                    }
                }

                Rebuild();
            }
        }
        
        private void Awake()
        {
            m_meshFilter = GetComponent<MeshFilter>();
            m_meshFilter.sharedMesh = new Mesh();
            m_meshFilter.sharedMesh.name = "FaceSelection";

            m_meshRenderer = GetComponent<MeshRenderer>();
            if(m_meshRenderer.sharedMaterial == null)
            {
                m_meshRenderer.sharedMaterial = Resources.Load<Material>("MeshEditor.FaceHighlight");
            }
        }

        public bool Contains(int index)
        {
            return m_indexToFace.ContainsKey(index);
        }

        public bool Add(int index, Face face)
        {
            if(m_indexToFace.ContainsKey(index))
            {
                return false;
            }
            m_indexToFace.Add(index, face);
            return true;
        }

        public void Remove(int index)
        {
            m_indexToFace.Remove(index);
        }

        public bool IsSelected(int index)
        {
            return m_indexToFace.ContainsKey(index);
        }

        public void Clear()
        {
            m_indexToFace.Clear();
        }

        public void Rebuild()
        {
            Mesh mesh = m_meshFilter.sharedMesh;
            mesh.Clear();

            Vector3[] vertices = new Vector3[m_indexToFace.Count * 3];
            int[] tris = new int[m_indexToFace.Count * 3];
            int index = 0;
            foreach(Face face in m_indexToFace.Values)
            {
                vertices[index] = face.V0;
                vertices[index + 1] = face.V1;
                vertices[index + 2] = face.V2;

                tris[index] = index;
                tris[index + 1] = index + 1;
                tris[index + 2] = index + 2;

                index += 3;
            }

            mesh.vertices = vertices;
            mesh.triangles = tris;
        }
        
    }
}
