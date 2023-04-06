using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainGridHandle : MonoBehaviour
    {
        [SerializeField]
        private Color m_pointerOverColor = Color.yellow;

        [SerializeField]
        private Color m_selectedColor = Color.yellow;

        [SerializeField]
        private Color m_normalColor = Color.white;

        private bool m_isPointerOver;
        public bool IsPointerOver
        {
            get { return m_isPointerOver; }
            set
            {
                m_isPointerOver = value;
                UpdateVisualState();
            }
        }

        private bool m_isSelected;
        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                m_isSelected = value;
                UpdateVisualState();
            }
        }

        private bool m_zTest;
        public bool ZTest
        {
            get
            {
                return m_zTest;
            }
            set
            {
                if(ZTest != value)
                {
                    m_zTest = value;
                    if(m_renderer != null && m_renderer.sharedMaterial != null)
                    {
                        UpdateZTest();
                    }
                }
            }
        }

        private void UpdateZTest()
        {
            m_renderer.sharedMaterial.SetFloat("_ZTest", m_zTest ? 2 : 0);
        }

        private void UpdateVisualState()
        {
            if(m_isSelected)
            {
                m_renderer.sharedMaterial.color = m_selectedColor;
            }
            else if(m_isPointerOver)
            {
                m_renderer.sharedMaterial.color = m_pointerOverColor;
            }
            else
            {
                m_renderer.sharedMaterial.color = m_normalColor;
            }
        }

        private float m_terrainHeight;
        public void SetTerrainHeight(float terrainHeight)
        {
            m_terrainHeight = terrainHeight;
            
            Vector3 lp = transform.localPosition;
            lp.y = m_terrainHeight;
            transform.localPosition = lp;
        }

        public Vector3 GetLocalPosition()
        {
            return transform.localPosition - Vector3.up * m_terrainHeight;
        }

        public void SetLocalPostion(Vector3 position)
        {
            transform.localPosition = position + Vector3.up * m_terrainHeight;
        }

        private Renderer m_renderer;
        private void Awake()
        {
            m_renderer = GetComponent<Renderer>();
            m_renderer.sharedMaterial = m_renderer.material;
            UpdateVisualState();
            UpdateZTest();
        }
    }
}
