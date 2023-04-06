using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.MeshDeformer3
{
    [Serializable]
    public class Slice
    {
        [SerializeField]
        public Vector3 m_center;

        [SerializeField]
        public int m_curveIndex;

        [SerializeField]
        public float m_t; //[0, 1] within curve

        [SerializeField]
        public int[] m_indices;

        public Vector3 Center
        {
            get { return m_center; }
        }

        public int SegmentIndex
        {
            get { return m_curveIndex; }
            set { m_curveIndex = value; }
        }

        public float T
        {
            get { return m_t; }
        }

        public int[] Indices
        {
            get { return m_indices; }
        }

        public Slice()
        {

        }

        public Slice(Vector3 center, int curveIndex, float t, int[] vertexIndices)
        {
            m_center = center;
            m_curveIndex = curveIndex;
            m_t = Mathf.Clamp01(t);
            m_indices = vertexIndices;
        }
    }

    public class Segment : MonoBehaviour
    {
        public static event Action<Deformer, Segment> Deformed;

        private Vector3 m_up;
        private Quaternion m_axisRotation;
        private MeshFilter m_meshFilter;
        private MeshCollider m_meshCollider;
        private Slice[] m_slices;
        private Slice[] m_colliderSlices;

        public int[] CurveIndices
        {
            get;
            private set;
        }

        public Mesh Mesh
        {
            get { return m_meshFilter != null ? m_meshFilter.sharedMesh : null; }
        }

        public Mesh ColliderMesh
        {
            get { return m_meshCollider != null ? m_meshCollider.sharedMesh : null; }
        }

        public void Wrap(MeshFilter meshFilter, MeshCollider meshCollider, Axis axis, int[] curveIndices, int sliceCount)
        {
            if (curveIndices.Length < 1)
            {
                throw new ArgumentException("at least one curveIndex required", "curveIndices");
            }

            CurveIndices = curveIndices;
            m_up = Up(axis);
            if (axis == Axis.Z)
            {
                m_axisRotation = Quaternion.identity;
            }
            else if (axis == Axis.X)
            {
                m_axisRotation = Quaternion.AngleAxis(-90.0f, Vector3.up);
            }
            else
            {
                m_axisRotation = Quaternion.AngleAxis(90.0f, Vector3.right);
            }

            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                m_meshFilter = null;
                m_meshCollider = null;
                m_slices = new Slice[curveIndices.Length * (sliceCount + 1)];
                m_colliderSlices = new Slice[curveIndices.Length * (sliceCount + 1)];
                for (int i = 0; i < m_slices.Length; ++i)
                {
                    m_slices[i] = new Slice(Vector3.zero, -1, 0, new int[0]);
                    m_colliderSlices[i] = new Slice(Vector3.zero, -1, 0, new int[0]);
                }
                return;
            }

            sliceCount = Math.Max(sliceCount / curveIndices.Length, 1);

            Vector3 boundsFrom;
            Vector3 boundsTo;
            meshFilter.sharedMesh.GetBounds(axis, out boundsFrom, out boundsTo);

            m_meshFilter = meshFilter;
            m_slices = CreateSlices(m_meshFilter.sharedMesh, boundsFrom, boundsTo, axis, curveIndices, sliceCount);

            if (meshCollider == null || meshCollider.sharedMesh == null)
            {
                m_meshCollider = null;
                m_colliderSlices = new Slice[curveIndices.Length * (sliceCount + 1)];
                for (int i = 0; i < m_colliderSlices.Length; ++i)
                {
                    m_colliderSlices[i] = new Slice(Vector3.zero, -1, 0, new int[0]);
                }
            }
            else
            {
                m_meshCollider = meshCollider;
                m_colliderSlices = CreateSlices(m_meshCollider.sharedMesh, boundsFrom, boundsTo, axis, curveIndices, sliceCount);
            }
        }

        private Slice[] CreateSlices(Mesh mesh, Vector3 boundsFrom, Vector3 boundsTo, Axis axis, int[] curveIndices, int sliceCount)
        {
            Slice[] result = new Slice[curveIndices.Length * (sliceCount + 1)];
            Vector3[] vertices = mesh.vertices;
            List<int>[,] slices = new List<int>[curveIndices.Length, sliceCount + 1];
            for (int i = 0; i < curveIndices.Length; ++i)
            {
                for (int s = 0; s <= sliceCount; ++s)
                {
                    slices[i, s] = new List<int>(vertices.Length / result.Length);
                }
            }

            Vector3 delta = (boundsTo - boundsFrom) / curveIndices.Length;
            for (int v = 0; v < vertices.Length; ++v)
            {
                Vector3 vertex = vertices[v];

                int minI = -1;
                int minS = -1;
                float minMag = float.MaxValue;

                Vector3 offset = boundsFrom;
                for (int i = 0; i < curveIndices.Length; ++i)
                {
                    float t = 0.0f;
                    for (int s = 0; s <= sliceCount; ++s)
                    {
                        Vector3 point = Vector3.Lerp(offset, offset + delta, t);
                        float sqrMag = (vertex - point).sqrMagnitude;
                        if (sqrMag < minMag)
                        {
                            minMag = sqrMag;
                            minI = i;
                            minS = s;
                        }
                        t += 1.0f / sliceCount;
                    }
                    offset += delta;
                }
                slices[minI, minS].Add(v);
            }

            {
                Vector3 offset = boundsFrom;
                for (int i = 0; i < curveIndices.Length; ++i)
                {

                    int curveIndex = curveIndices[i];
                    float t = 0.0f;
                    for (int s = 0; s <= sliceCount; ++s)
                    {
                        result[i * (sliceCount + 1) + s] = new Slice(Vector3.Lerp(offset, offset + delta, t), curveIndex, t, slices[i, s].ToArray());
                        t += 1.0f / sliceCount;
                    }
                    offset += delta;

                }
            }

            return result;
        }

        public void SlerpContacts(Deformer deformer, Mesh original, Mesh colliderOriginal, Segment prev, Segment next, bool isRigid)
        {
            if (isRigid)
            {
                return;
            }

            Mesh prevMesh = null;
            Mesh nextMesh = null;
            if (prev != null)
            {
                prevMesh = prev.Mesh;
            }

            if (next != null)
            {
                nextMesh = next.Mesh;
            }
            SlerpContacts(deformer, Mesh, deformer.Contacts, prev, prevMesh, next, nextMesh);


            if (colliderOriginal == null)
            {
                return;
            }
            if (prev != null)
            {
                prevMesh = prev.ColliderMesh;
            }

            if (next != null)
            {
                nextMesh = next.ColliderMesh;
            }
            SlerpContacts(deformer, ColliderMesh, deformer.ColliderContacts, prev, prevMesh, next, nextMesh);

            if (m_meshCollider != null)
            {
                Mesh colliderMesh = ColliderMesh;

                m_meshCollider.sharedMesh = null;
                m_meshCollider.sharedMesh = colliderMesh;
            }
        }

        private void SlerpContacts(Deformer deformer, Mesh mesh, Contact[] contacts, Segment prev, Mesh prevMesh, Segment next, Mesh nextMesh)
        {
            Vector3[] normals = null;
            Vector3[] prevNormals = null;
            Vector3[] nextNormals = null;
            if (mesh == null)
            {
                return;
            }

            if (prev != null || next != null)
            {
                normals = mesh.normals;
            }

            if (prevMesh != null && prev != null && (prev != this || deformer.SegmentsCount == 1 && deformer.IsLooping))
            {
                prevNormals = prevMesh.normals;
                for (int i = 0; i < contacts.Length; ++i)
                {
                    Contact contact = contacts[i];
                    Vector3 prevNormal = prevNormals[contact.Index2];
                    Vector3 normal = normals[contact.Index1];
                    Vector3 slerped = Vector3.Slerp(prevNormal, normal, 0.5f);
                    prevNormals[contact.Index2] = slerped;
                    normals[contact.Index1] = slerped;
                }
            }

            if (nextMesh != null && next != null && (next != this || deformer.SegmentsCount == 1 && deformer.IsLooping))
            {

                nextNormals = nextMesh.normals;
                for (int i = 0; i < contacts.Length; ++i)
                {
                    Contact contact = contacts[i];
                    Vector3 normal = normals[contact.Index2];
                    Vector3 nextNormal = nextNormals[contact.Index1];
                    Vector3 slerped = Vector3.Slerp(normal, nextNormal, 0.5f);

                    normals[contact.Index2] = slerped;
                    nextNormals[contact.Index1] = slerped;
                }
            }

            if (prev != null)
            {
                if (mesh != null)
                {
                    mesh.normals = normals;
                }

                if (this != prev)
                {
                    if (prevMesh != null)
                    {
                        prevMesh.normals = prevNormals;
                    }

                }

                if (next != null && next != prev)
                {
                    if (nextMesh != null)
                    {
                        nextMesh.normals = nextNormals;
                    }
                }
            }
            else if (next != null)
            {
                if (mesh != null)
                {
                    mesh.normals = normals;
                }

                if (prev != null && prev != next)
                {
                    if (prevMesh != null)
                    {
                        prevMesh.normals = prevNormals;
                    }
                }

                if (this != next)
                {
                    if (nextMesh != null)
                    {
                        nextMesh.normals = nextNormals;
                    }
                }
            }
        }

        public void Deform(Deformer deformer, Mesh original, Mesh colliderOriginal, bool isRigid)
        {
            if (original != null)
            {
                Mesh mesh = Mesh;
                if (mesh != null)
                {
                    mesh.vertices = Deform(m_slices, original, deformer, isRigid);
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();
                }
            }

            if (colliderOriginal != null && m_meshCollider != null)
            {
                Mesh colliderMesh = ColliderMesh;
                if (colliderMesh != null)
                {
                    colliderMesh.vertices = Deform(m_colliderSlices, colliderOriginal, deformer, isRigid);
                    colliderMesh.RecalculateBounds();
                    colliderMesh.RecalculateNormals();
                    m_meshCollider.sharedMesh = null;
                    m_meshCollider.sharedMesh = colliderMesh;
                }
            }

            if (Deformed != null)
            {
                Deformed(deformer, this);
            }
        }

        private Vector3[] Deform(Slice[] slices, Mesh mesh, Deformer deformer, bool isRigid)
        {
            Vector3[] vertices = mesh.vertices;
            for (int s = 0; s < slices.Length; ++s)
            {
                Slice slice = slices[s];

                Vector3 center = deformer.GetPosition(slice.SegmentIndex, slice.T);
                center = deformer.transform.InverseTransformPoint(center);

                Vector3 dir = deformer.transform.InverseTransformVector(deformer.GetDirection(slice.SegmentIndex, slice.T));
                float t = slice.T;
                if (isRigid)
                {
                    t = 1.0f;
                }

                if (dir == Vector3.zero)
                {
                    // continue;
                }

                float twistAngle = deformer.GetTwist(slice.SegmentIndex, t);
                Vector3 thickness = deformer.GetThickness(slice.SegmentIndex, t);
                float wrapCurvature = deformer.GetWrap(slice.SegmentIndex, t);

                Quaternion rotation = Quaternion.AngleAxis(twistAngle, dir) * Quaternion.LookRotation(dir, m_up) * m_axisRotation;

                Matrix4x4 matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

                //#warning Continue here
                /*
                Vector3 angle = rotation.eulerAngles;
                Matrix4x4 skew = Matrix4x4.identity;
                skew.m01 = Mathf.Tan(angle.x);
                skew.m02 = Mathf.Tan(angle.y);
                skew.m12 = Mathf.Tan(angle.z);
                matrix = skew;
                */

                int[] indices = slice.Indices;
                if (indices != null)
                {
                    for (int i = 0; i < indices.Length; ++i)
                    {
                        int index = indices[i];
                        Vector3 vertex = vertices[index];
                        vertex = AxisTransform(deformer, vertex, slice.Center, thickness);
                        vertex = WrapAroundAxisTransform(deformer, vertex, slice.Center, wrapCurvature);
                        vertex = matrix.MultiplyPoint(vertex);
                        vertices[index] = vertex;
                    }
                }
            }
            return vertices;
        }

        private static Vector3 WrapAroundAxisTransform(Deformer deformer, Vector3 vertex, Vector3 center, float curvature = 0.1f)
        {
            Vector3 result = vertex;
            if (deformer.Axis == Axis.X)
            {
                result.y += Mathf.Pow((vertex.z - center.z), 2) * curvature * 0.01f;
            }
            else if (deformer.Axis == Axis.Y)
            {
                result.z += Mathf.Pow((vertex.x - center.x), 2) * curvature * 0.01f;
            }
            else
            {
                result.y += Mathf.Pow((vertex.x - center.x), 2) * curvature * 0.01f;
            }

            return result;
        }

        private static Vector3 AxisTransform(Deformer deformer, Vector3 vertex, Vector3 center, Vector3 scale)
        {
            Vector3 toVertex = vertex - center;
            if (deformer.Axis == Axis.X)
            {
                toVertex.x = 0;
                center.x = vertex.x - center.x;
            }
            else if (deformer.Axis == Axis.Y)
            {
                toVertex.y = 0;
                center.y = vertex.y - center.y;
            }
            else
            {
                toVertex.z = 0;
                center.z = vertex.z - center.z;
            }

            return center + Vector3.Scale(toVertex, scale);
        }

        private static Vector3 Up(Axis axis)
        {
            if (axis == Axis.Z)
            {
                return Vector3.up;
            }
            else if (axis == Axis.X)
            {
                return Vector3.up;
            }
            else
            {
                return Vector3.back;
            }
        }
    }
}
