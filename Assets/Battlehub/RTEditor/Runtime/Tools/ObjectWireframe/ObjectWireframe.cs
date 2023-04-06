using UnityEngine;

namespace Battlehub.Wireframe
{
    [RequireComponent(typeof(MeshFilter))]
    public class ObjectWireframe : MonoBehaviour
    {
        private static Material m_wMaterial;
        private MeshFilter m_filter;
        private GameObject m_wireframe;
        private MeshRenderer m_wireframeRenderer;
        private MaterialPropertyBlock m_props;
        private int m_colorPropID = Shader.PropertyToID("_Color");

        [SerializeField]
        private int m_wireframeLayer = 0;
        public int WireframeLayer
        {
            get { return m_wireframeLayer; }
            set
            {
                m_wireframeLayer = value;
                if(m_wireframe != null)
                {
                    m_wireframe.layer = m_wireframeLayer;
                }
            }
        }

        [SerializeField]
        private Color m_color = Color.yellow;
        public Color Color
        {
            get { return m_color; }
            set
            {
                m_color = value;
                ApplyColor();
            }
        }

        [SerializeField]
        private Color m_selectionColor = new Color(1, 0.35f, 0, 1);
        public Color SelectionColor
        {
            get { return m_selectionColor; }
            set
            {
                m_selectionColor = value;
                ApplyColor();
            }
        }

        [SerializeField]
        private bool m_showWireframeOnly = false;
        [SerializeField]
        private bool m_isOn = true;
        public bool IsOn
        {
            get { return m_isOn; }
            set
            {
                if (m_isOn != value)
                {
                    m_isOn = value;

                    if (m_showWireframeOnly)
                    {
                        MeshRenderer renderer = GetComponent<MeshRenderer>();
                        if (renderer != null)
                        {
                            renderer.enabled = !m_isOn;
                        }
                    }

                    if (m_wireframe != null)
                    {
                        m_wireframe.SetActive(m_isOn);
                    }
                }
            }
        }

        private bool m_isSelected;
        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    ApplyColor();
                }
            }
        }

        private void ApplyColor()
        {
            if (m_wireframeRenderer != null)
            {
                m_props.SetColor(m_colorPropID, IsSelected ? SelectionColor : Color);
                m_wireframeRenderer.SetPropertyBlock(m_props);
            }
        }

 
        private void Awake()
        {
            if(m_wMaterial == null)
            {
                m_wMaterial = Resources.Load<Material>("ObjectWireframe");
            }

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh != null)
            {
                m_wireframe = new GameObject("Wireframe");
                m_wireframe.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideAndDontSave;
                m_wireframe.transform.SetParent(transform, false);
                //m_wireframe.gameObject.AddComponent<RTSLIgnore>();
                m_wireframe.layer = m_wireframeLayer;

                m_filter = m_wireframe.AddComponent<MeshFilter>();
                m_filter.sharedMesh = CreateWirefameMesh(mesh);

                m_wireframeRenderer = m_wireframe.AddComponent<MeshRenderer>();
                m_wireframeRenderer.sharedMaterial = m_wMaterial;
                m_props = new MaterialPropertyBlock();
                ApplyColor();

                m_wireframe.SetActive(m_isOn);
                if (m_showWireframeOnly)
                {
                    MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        meshRenderer.enabled = !m_isOn;
                    }
                }
            }
        }

        private void OnEnable()
        {
            if(m_wireframe != null)
            {
                m_wireframe.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if(m_wireframe != null)
            {
                m_wireframe.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (m_wireframe)
            {
                Destroy(m_wireframe);
            }
        }

        public void Refresh(Mesh mesh)
        {
            Mesh wireframe = m_filter.sharedMesh;
            wireframe.vertices = mesh.vertices;
        }

        private Mesh CreateWirefameMesh(Mesh mesh)
        {
            int[] triangles = mesh.triangles;
            int[] wIndices = new int[triangles.Length * 2];

            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v0 = triangles[i];
                int v1 = triangles[i + 1];
                int v2 = triangles[i + 2];

                wIndices[i * 2] = v0;
                wIndices[i * 2 + 1] = v1;

                wIndices[(i + 1) * 2] = v1;
                wIndices[(i + 1) * 2 + 1] = v2;

                wIndices[(i + 2) * 2] = v2;
                wIndices[(i + 2) * 2 + 1] = v0;
            }

            Mesh wMesh = new Mesh();
            wMesh.name = mesh.name + " Wireframe";
            wMesh.vertices = vertices;
            wMesh.subMeshCount = 1;
            wMesh.SetIndices(wIndices, MeshTopology.Lines, 0);

            return wMesh;
        }
    }
}

