using UnityEngine;

namespace Battlehub.RTTerrain
{
    public abstract class TerrainPaintEditor : MonoBehaviour
    {
        [SerializeField]
        private TerrainBrushEditor m_terrainBrushEditor = null;
        protected TerrainBrushEditor TerrainBrushEditor
        {
            get { return m_terrainBrushEditor; }
        }
        private TerrainEditor m_terrainEditor;
        protected TerrainEditor TerrainEditor
        {
            get { return m_terrainEditor; }
        }
        protected virtual void Awake()
        {
            m_terrainEditor = GetComponentInParent<TerrainEditor>();
            m_terrainEditor.TerrainChanged += OnTerrainChanged;

            m_terrainBrushEditor.BrushOpacity = PlayerPrefs.GetInt(GetType().FullName + ".BrushOpacity", m_terrainBrushEditor.BrushOpacity);
            m_terrainBrushEditor.BrushSize = PlayerPrefs.GetFloat(GetType().FullName + ".BrushSize", m_terrainBrushEditor.BrushSize);

            if (m_terrainBrushEditor != null)
            {
                m_terrainBrushEditor.SelectedBrushChanged += OnSelectedBrushChanged;
                m_terrainBrushEditor.BrushParamsChanged += OnBrushParamsChanged;
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_terrainEditor != null)
            {
                m_terrainEditor.TerrainChanged -= OnTerrainChanged;
            }

            if (m_terrainBrushEditor != null)
            {
                m_terrainBrushEditor.SelectedBrushChanged -= OnSelectedBrushChanged;
                m_terrainBrushEditor.BrushParamsChanged -= OnBrushParamsChanged;
            }
        }

        protected virtual void OnEnable()
        {
            if (m_terrainBrushEditor.SelectedBrush != null)
            {
                OnSelectedBrushChanged(this, System.EventArgs.Empty);
            }
            OnBrushParamsChanged(this, System.EventArgs.Empty);
        }

        protected virtual void OnDisable()
        {
            m_terrainEditor.Projector.TerrainBrush = null;
        }

        protected virtual void OnTerrainChanged()
        {

        }

        protected virtual void OnSelectedBrushChanged(object sender, System.EventArgs e)
        {
            if (m_terrainEditor.Projector.TerrainBrush == null ||
                m_terrainEditor.Projector.TerrainBrush != m_terrainEditor.Projector.TerrainBrush)
            {
                InitializeTerrainBrush();
            }
            else
            {
                m_terrainEditor.Projector.Brush = m_terrainBrushEditor.SelectedBrush.texture;
            }
        }

        protected virtual void OnBrushParamsChanged(object sender, System.EventArgs e)
        {
            if (m_terrainEditor.Projector.TerrainBrush == null ||
                m_terrainEditor.Projector.TerrainBrush != m_terrainEditor.Projector.TerrainBrush)
            {
                InitializeTerrainBrush();
            }
            else
            {
                m_terrainEditor.Projector.Size = m_terrainBrushEditor.BrushSize;
                m_terrainEditor.Projector.Opacity = m_terrainBrushEditor.BrushOpacity;
            }

            PlayerPrefs.SetInt(GetType().FullName + ".BrushOpacity", m_terrainBrushEditor.BrushOpacity);
            PlayerPrefs.SetFloat(GetType().FullName + ".BrushSize", m_terrainBrushEditor.BrushSize);
        }

        protected virtual void InitializeTerrainBrush()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            m_terrainEditor.Projector.TerrainBrush  = CreateBrush();
            if(m_terrainBrushEditor.SelectedBrush != null)
            {
                m_terrainEditor.Projector.Brush = m_terrainBrushEditor.SelectedBrush.texture;
            }

            m_terrainEditor.Projector.Size = m_terrainBrushEditor.BrushSize;
            m_terrainEditor.Projector.Opacity = m_terrainBrushEditor.BrushOpacity;
        }

        protected abstract Brush CreateBrush();
    }

}
