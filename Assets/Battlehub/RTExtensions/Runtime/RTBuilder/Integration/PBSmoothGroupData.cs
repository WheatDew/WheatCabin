using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;

namespace Battlehub.ProBuilderIntegration
{
    public class PBSmoothGroupData
    {
        private Dictionary<int, List<Face>> m_groups;
        private Dictionary<int, Color> groupColors;
       
        public IEnumerable<int> Groups
        {
            get
            {
                if(m_groups == null)
                {
                    yield break;
                }
                foreach(int key in m_groups.Keys)
                {
                    yield return key;
                }
            }
        }

        private Mesh m_previewMesh;
        private Mesh m_normalsMesh;
        private GameObject m_preview;
        private GameObject m_normals;

        private static Material m_previewMaterial;
        private static Material m_normalsMaterial;

        public static float PreviewOpacity
        {
            get
            {
                if(m_previewMaterial == null)
                {
                    return 0;
                }
                return m_previewMaterial.GetFloat("_Opacity");
            }
            set
            {
                CreateMaterialsIfRequired();
                m_previewMaterial.SetFloat("_Opacity", value);
            }
        }

        public static float NormalsScale
        {
            get
            {
                if(m_normalsMaterial == null)
                {
                    return 0;
                }
                return m_normalsMaterial.GetFloat("_Scale");
            }
            set
            {
                CreateMaterialsIfRequired();
                m_normalsMaterial.SetFloat("_Scale", value);
            }
        }

        public PBMesh PBMesh
        {
            get;
            private set;
        }

        public PBSmoothGroupData(PBMesh pb)
        {
            CreateMaterialsIfRequired();

            m_groups = new Dictionary<int, List<Face>>();
            //selected = new HashSet<int>();
            groupColors = new Dictionary<int, Color>();

            m_previewMesh = new Mesh()
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = pb.name + "_SmoothingPreview"
            };

            m_normalsMesh = new Mesh()
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = pb.name + "_SmoothingNormals"
            };

            m_preview = new GameObject("SmoothingPreview");
            m_preview.AddComponent<MeshFilter>().sharedMesh = m_previewMesh;
            m_preview.AddComponent<MeshRenderer>().sharedMaterial = m_previewMaterial;

            m_normals = new GameObject("SmoothingNormals");
            m_normals.AddComponent<MeshFilter>().sharedMesh = m_normalsMesh;
            m_normals.AddComponent<MeshRenderer>().sharedMaterial = m_normalsMaterial;

            m_preview.transform.SetParent(pb.transform, false);
            m_normals.transform.SetParent(pb.transform, false);

            PBMesh = pb;

            Rebuild(pb);
        }

        private static void CreateMaterialsIfRequired()
        {
            if (m_previewMaterial == null)
            {
                m_previewMaterial = new Material(Shader.Find("Battlehub/RTBuilder/SmoothingPreview"));
                m_previewMaterial.SetFloat("_Dither", 0.0f);
                m_previewMaterial.SetInt("_HandleZTest", (int)CompareFunction.LessEqual);
                m_previewMaterial.SetColor("_Color", Color.white);
            }

            if (m_normalsMaterial == null)
            {
                m_normalsMaterial = new Material(Shader.Find("Battlehub/RTBuilder/NormalPreview"));
                m_normalsMaterial.SetColor("_Color", Color.white);
                m_normalsMaterial.SetInt("_HandleZTest", (int)CompareFunction.LessEqual);
                m_normalsMaterial.SetFloat("_Scale", 1.0f);
            }
        }

        public void Rebuild(PBMesh pb)
        {
            CacheGroups(pb.ProBuilderMesh);
            RebuildPreviewMesh(pb.ProBuilderMesh);
            RebuildNormalsMesh(pb);
        }

        public void CacheGroups(ProBuilderMesh pb)
        {
            m_groups.Clear();

            foreach (Face face in pb.faces)
            {
                List<Face> affected;

                if (!m_groups.TryGetValue(face.smoothingGroup, out affected))
                {
                    m_groups.Add(face.smoothingGroup, new List<Face>() { face });
                }
                else
                {
                    affected.Add(face);
                }
            }
        }

        private void RebuildPreviewMesh(ProBuilderMesh pb)
        {
            List<int> indexes = new List<int>();
            Color32[] colors = new Color32[pb.vertexCount];
            groupColors.Clear();

            foreach (KeyValuePair<int, List<Face>> smoothGroup in m_groups)
            {
                if (smoothGroup.Key > PBSmoothing.smoothingGroupNone)
                {
                    Color32 color = GetColor(smoothGroup.Key);
                    groupColors.Add(smoothGroup.Key, color);
                    var groupIndexes = smoothGroup.Value.SelectMany(y => y.indexes);
                    indexes.AddRange(groupIndexes);
                    foreach (int i in groupIndexes)
                    {
                        colors[i] = color;
                    }
                }
            }

            m_previewMesh.Clear();
            m_previewMesh.vertices = pb.positions.ToArray();
            m_previewMesh.colors32 = colors;
            m_previewMesh.triangles = indexes.ToArray();
        }

        public void RebuildNormalsMesh(PBMesh pb)
        {
            m_normalsMesh.Clear();
            Mesh mesh = pb.Mesh;
            Vector3[] srcPositions = mesh.vertices;
            Vector3[] srcNormals = mesh.normals;
            int vertexCount = System.Math.Min(ushort.MaxValue / 2, mesh.vertexCount);
            Vector3[] positions = new Vector3[vertexCount * 2];
            Vector4[] tangents = new Vector4[vertexCount * 2];
            int[] indexes = new int[vertexCount * 2];
            for (int i = 0; i < vertexCount; i++)
            {
                int a = i * 2, b = i * 2 + 1;

                positions[a] = srcPositions[i];
                positions[b] = srcPositions[i];
                tangents[a] = new Vector4(srcNormals[i].x, srcNormals[i].y, srcNormals[i].z, 0f);
                tangents[b] = new Vector4(srcNormals[i].x, srcNormals[i].y, srcNormals[i].z, 1f);
                indexes[a] = a;
                indexes[b] = b;
            }
            m_normalsMesh.vertices = positions;
            m_normalsMesh.tangents = tangents;
            m_normalsMesh.subMeshCount = 1;
            m_normalsMesh.SetIndices(indexes, MeshTopology.Lines, 0);
        }

        public void Clear()
        {
            if (m_previewMesh)
            {
                UnityEngine.Object.DestroyImmediate(m_previewMesh);
            }

            if (m_normalsMesh)
            {
                UnityEngine.Object.DestroyImmediate(m_normalsMesh);
            }

            if (m_preview)
            {
                UnityEngine.Object.Destroy(m_preview);
            }

            if (m_normals)
            {
                UnityEngine.Object.DestroyImmediate(m_normals);
            }
        }

        private Color32 GetColor(int index)
        {
            return Colors.Kellys[index % Colors.Kellys.Length];
        }
    }
}
