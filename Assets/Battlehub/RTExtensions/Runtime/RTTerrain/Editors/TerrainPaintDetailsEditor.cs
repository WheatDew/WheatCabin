
using System;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainPaintDetailsEditor : TerrainPaintEditor
    {
        [SerializeField]
        private TerrainDetailEditor m_terrainDetailEditor = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_terrainDetailEditor != null)
            {
                m_terrainDetailEditor.SelectedDetailChanged += OnSelectedDetailChanged;
            }

            if (TerrainEditor.Terrain != null)
            {
                m_terrainDetailEditor.Terrain = TerrainEditor.Terrain;
            }

            if (TerrainEditor.Projector != null)
            {
                TerrainEditor.Projector.gameObject.SetActive(GetTerrainDetailIndex() >= 0);
            }

        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_terrainDetailEditor != null)
            {
                m_terrainDetailEditor.SelectedDetailChanged -= OnSelectedDetailChanged;
            }
        }

        private void OnSelectedDetailChanged(object sender, EventArgs e)
        {
            InitializeTerrainBrush();
            TerrainDetailsBrush brush = (TerrainDetailsBrush)TerrainEditor.Projector.TerrainBrush;
            brush.TerrainDetailIndex = GetTerrainDetailIndex();
            TerrainEditor.Projector.gameObject.SetActive(brush.TerrainDetailIndex >= 0);
        }

        private int GetTerrainDetailIndex()
        {
            return m_terrainDetailEditor.SelectedDetail != null ? Array.IndexOf(m_terrainDetailEditor.Terrain.terrainData.detailPrototypes, m_terrainDetailEditor.SelectedDetail) : -1;
        }

        protected override void OnTerrainChanged()
        {
            if (m_terrainDetailEditor != null)
            {
                if (TerrainEditor.Terrain == null || TerrainEditor.Terrain.terrainData == null)
                {
                    m_terrainDetailEditor.Terrain = null;
                }
                else
                {
                    m_terrainDetailEditor.Terrain = TerrainEditor.Terrain;
                }
            }
        }

        protected override Brush CreateBrush()
        {
            TerrainEditor.Projector.gameObject.SetActive(GetTerrainDetailIndex() >= 0);
            return new TerrainDetailsBrush()
            {
                TerrainDetailIndex = GetTerrainDetailIndex()
            };

        }
    }
}

