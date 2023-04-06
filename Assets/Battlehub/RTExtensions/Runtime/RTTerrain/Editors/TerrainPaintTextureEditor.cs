using System;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainPaintTextureEditor : TerrainPaintEditor
    {
        [SerializeField]
        private TerrainLayerEditor m_terrainLayerEditor = null;

        private static int m_defaultLayerIndex;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_terrainLayerEditor != null)
            {
                m_terrainLayerEditor.DefaultSelectedIndex = m_defaultLayerIndex;
                m_terrainLayerEditor.SelectedLayerChanged += OnSelectedLayerChanged;
            }

            if (TerrainEditor.Terrain != null)
            {
                m_terrainLayerEditor.Terrain = TerrainEditor.Terrain;
            }

            if (TerrainEditor.Projector != null)
            {
                TerrainEditor.Projector.gameObject.SetActive(GetTerrainLayerIndex() >= 0);
            }
   
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        
            if (m_terrainLayerEditor != null)
            {
                m_terrainLayerEditor.SelectedLayerChanged -= OnSelectedLayerChanged;
            }
        }

        private void OnSelectedLayerChanged(object sender, EventArgs e)
        {
            InitializeTerrainBrush();
            TerrainTextureBrush brush = (TerrainTextureBrush)TerrainEditor.Projector.TerrainBrush;
            brush.TerrainLayerIndex = GetTerrainLayerIndex();
            TerrainEditor.Projector.gameObject.SetActive(brush.TerrainLayerIndex >= 0);

            int layerIndex = GetTerrainLayerIndex();
            if(layerIndex != -1)
            {
                m_defaultLayerIndex = layerIndex;
            }
        }

    
        private int GetTerrainLayerIndex()
        {
            return m_terrainLayerEditor.SelectedLayer != null ? Array.IndexOf(m_terrainLayerEditor.Terrain.terrainData.terrainLayers, m_terrainLayerEditor.SelectedLayer) : -1;
        }

        protected override void OnTerrainChanged()
        {
            if (m_terrainLayerEditor != null)
            {
                if (TerrainEditor.Terrain == null || TerrainEditor.Terrain.terrainData == null)
                {
                    m_terrainLayerEditor.Terrain = null;
                }
                else
                {
                    m_terrainLayerEditor.Terrain = TerrainEditor.Terrain;
                }
            }
        }

        protected override Brush CreateBrush()
        {
            TerrainEditor.Projector.gameObject.SetActive(GetTerrainLayerIndex() >= 0);
            return new TerrainTextureBrush()
            {                
                TerrainLayerIndex = GetTerrainLayerIndex()
            };
        }
    }
}
