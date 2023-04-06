using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.MeshTools
{
    public class EditableMesh : MonoBehaviour
    {
        private FaceSelection m_faceSelection;

        private MeshFilter m_meshFilter;

        public ICollection<Face> SelectedFaces
        {
            get { return m_faceSelection.SelectedFaces; }
            set { m_faceSelection.SelectedFaces = value; }
        }

        public bool IsSelectionVisible
        {
            get { return m_faceSelection.gameObject.activeSelf; }
            set { m_faceSelection.gameObject.SetActive(value); }
        }

        private int[] m_sharedVertices;
        private bool IsSharedVertex(int index)
        {
            if(0 <= index && index < m_sharedVertices.Length)
            {
                return m_sharedVertices[index] > 1;
            }

            return false;
        }

        public Mesh Mesh
        {
            get { return m_meshFilter.sharedMesh; }
        }

        private void Awake()
        {
            m_meshFilter = GetComponent<MeshFilter>();

            Mesh mesh = m_meshFilter.mesh;
            m_meshFilter.sharedMesh = mesh; //copy mesh;
            m_sharedVertices = new int[mesh.vertexCount];

            int[] tris = mesh.triangles; 
            for(int i = 0; i < tris.Length; ++i)
            {
                m_sharedVertices[tris[i]]++;
            }

            if (mesh.uv.Length == 0)
            {
                mesh.uv = UvCalculator.CalculateUVs(tris, mesh.vertices, 1);
            }

            GameObject faceSelection = new GameObject("FaceSelection");
            faceSelection.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideAndDontSave;
            faceSelection.transform.SetParent(transform, false);
            m_faceSelection = faceSelection.AddComponent<FaceSelection>();
        }

        public bool SelectFace(Ray ray, out Face face)
        {
            Mesh mesh = m_meshFilter.sharedMesh;
            face = new Face();

            Utils.RaycastHit hit;
            bool selected = Utils.FaceRaycast(ray, mesh, m_meshFilter.transform, out hit);
            if(selected)
            {
                Vector3[] v = mesh.vertices;
                Vector2[] uv = mesh.uv;
                int[] tri = mesh.triangles;
              
                face = new Face(hit.Index, v[tri[hit.Index]], v[tri[hit.Index + 1]], v[tri[hit.Index + 2]], uv[tri[hit.Index]], uv[tri[hit.Index + 1]], uv[tri[hit.Index + 2]]);
                m_faceSelection.Add(hit.Index, face);
                m_faceSelection.Rebuild();
            }
            return selected;
        }

        public void UnselectFaces()
        {
            m_faceSelection.Clear();
            m_faceSelection.Rebuild();
        }

        public void RecacluateNormals(float angle = 60)
        {
            NormalSolver.RecalculateNormals(m_meshFilter.sharedMesh, angle);
        }

        public void RefreshUVs(IEnumerable<Face> faces)
        {
            Mesh mesh = m_meshFilter.sharedMesh;
            Vector2[] uv = mesh.uv;
            int[] tri = mesh.triangles;

            foreach(Face face in faces)
            {
                uv[tri[face.Index]] = face.UV0;
                uv[tri[face.Index + 1]] = face.UV1;
                uv[tri[face.Index + 2]] = face.UV2;
            }

            mesh.uv = uv;
        }

        public void Separate(IEnumerable<Face> faces)
        {
            Mesh mesh = m_meshFilter.sharedMesh;
            int[] triangles = mesh.triangles;

            List<Vector3> vertices = null;
            List<Vector3> normals = null;
            List<Vector2> uv = null;

            foreach(Face face in faces)
            {
                int i0 = triangles[face.Index];
                int i1 = triangles[face.Index + 1];
                int i2 = triangles[face.Index + 2];

                bool is0Shared = IsSharedVertex(i0);
                bool is1Shared = IsSharedVertex(i1);
                bool is2Shared = IsSharedVertex(i2);

                if(is0Shared || is1Shared || is2Shared)
                {
                    if(vertices == null)
                    {
                        vertices = mesh.vertices.ToList();
                        normals = mesh.normals.ToList();
                        uv = mesh.uv.ToList();
                    }

                    if(is0Shared)
                    {
                        SeparateVertex(face.Index, triangles, vertices, normals, uv);
                    }

                    if(is1Shared)
                    {
                        SeparateVertex(face.Index + 1, triangles, vertices, normals, uv);
                    }

                    if (is2Shared)
                    {
                        SeparateVertex(face.Index + 2, triangles, vertices, normals, uv);
                    }
                }
            }

            if(vertices != null)
            {
                mesh.vertices = vertices.ToArray();
                mesh.normals = normals.ToArray();
                mesh.uv = uv.ToArray();

                for(int i = 0; i < mesh.subMeshCount; ++i)
                {
                    var desc = mesh.GetSubMesh(i);
                    mesh.SetTriangles(triangles, desc.indexStart, desc.indexCount, i);
                }
            }
        }

        private void SeparateVertex(int faceIndex, int[] triangles, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv)
        {
            int index = triangles[faceIndex];
            triangles[faceIndex] = vertices.Count;
            vertices.Add(vertices[index]);
            normals.Add(normals[index]);
            uv.Add(uv[index]);
            m_sharedVertices[index]--;
        }
    }

    public static class UvCalculator
    {
        private enum Facing { Up, Forward, Right };

        public static Vector2[] CalculateUVs(int[] tris, Vector3[] v/*vertices*/, float scale)
        {
            var uvs = new Vector2[v.Length];

            for (int i = 0; i < tris.Length; i += 3)
            {
                int i0 = tris[i];
                int i1 = tris[i + 1];
                int i2 = tris[i + 2];

                Vector3 v0 = v[i0];
                Vector3 v1 = v[i1];
                Vector3 v2 = v[i2];

                Vector3 side1 = v1 - v0;
                Vector3 side2 = v2 - v0;
                var direction = Vector3.Cross(side1, side2);
                var facing = FacingDirection(direction);
                switch (facing)
                {
                    case Facing.Forward:
                        uvs[i0] = ScaledUV(v0.x, v0.y, scale);
                        uvs[i1] = ScaledUV(v1.x, v1.y, scale);
                        uvs[i2] = ScaledUV(v2.x, v2.y, scale);
                        break;
                    case Facing.Up:
                        uvs[i0] = ScaledUV(v0.x, v0.z, scale);
                        uvs[i1] = ScaledUV(v1.x, v1.z, scale);
                        uvs[i2] = ScaledUV(v2.x, v2.z, scale);
                        break;
                    case Facing.Right:
                        uvs[i0] = ScaledUV(v0.y, v0.z, scale);
                        uvs[i1] = ScaledUV(v1.y, v1.z, scale);
                        uvs[i2] = ScaledUV(v2.y, v2.z, scale);
                        break;
                }
            }
            return uvs;
        }

        private static bool FacesThisWay(Vector3 v, Vector3 dir, Facing p, ref float maxDot, ref Facing ret)
        {
            float t = Vector3.Dot(v, dir);
            if (t > maxDot)
            {
                ret = p;
                maxDot = t;
                return true;
            }
            return false;
        }

        private static Facing FacingDirection(Vector3 v)
        {
            var ret = Facing.Up;
            float maxDot = Mathf.NegativeInfinity;

            FacesThisWay(v, Vector3.right, Facing.Right, ref maxDot, ref ret);
            FacesThisWay(v, Vector3.left, Facing.Right, ref maxDot, ref ret);

            FacesThisWay(v, Vector3.forward, Facing.Forward, ref maxDot, ref ret);
            FacesThisWay(v, Vector3.back, Facing.Forward, ref maxDot, ref ret);

            FacesThisWay(v, Vector3.up, Facing.Up, ref maxDot, ref ret);
            FacesThisWay(v, Vector3.down, Facing.Up, ref maxDot, ref ret);

            return ret;
        }

        private static Vector2 ScaledUV(float uv1, float uv2, float scale)
        {
            return new Vector2(uv1 / scale, uv2 / scale);
        }
    }
}
