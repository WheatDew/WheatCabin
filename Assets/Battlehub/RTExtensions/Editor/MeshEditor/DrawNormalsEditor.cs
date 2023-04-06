#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace Battlehub.MeshTools
{
    [CustomEditor(typeof(DrawNormals))]
    public class DrawNormalsEditor : Editor
    {
        private Vector3[] m_vertices;
        private Vector3[] m_normals;
        private MeshFilter m_meshFilter;


        private void OnSceneGUI()
        {
            DrawNormals showNormals = (DrawNormals)target;

            if (showNormals.Show)
            {
                if (m_meshFilter == null)
                {
                    m_meshFilter = showNormals.GetComponent<MeshFilter>();
                    if (m_meshFilter == null)
                    {
                        m_meshFilter = showNormals.gameObject.AddComponent<MeshFilter>();
                    }

                    Mesh mesh = m_meshFilter.sharedMesh;
                    m_vertices = mesh.vertices;
                    m_normals = mesh.normals;
                }

                Handles.color = showNormals.Color;
                for (int i = 0; i < m_vertices.Length; i++)
                {
                    Vector3 norm = m_normals[i];

                    Vector3 start = m_vertices[i];
                    start = showNormals.transform.TransformPoint(start);

                    Vector3 end = start + showNormals.transform.TransformDirection(norm) * showNormals.Length;
                    Handles.DrawLine(start, end);
                }
            }
        }
    }
}
#endif
