using Battlehub.RTCommon;
using System;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainAreaEditor : MonoBehaviour
    {
        private ITerrainAreaTool m_tool;

        [SerializeField]
        private TerrainBrushEditor m_terrainBrushEditor = null;

        private void Awake()
        {
            m_terrainBrushEditor.SelectedBrushChanged += OnSelectedBrushChanged;
            m_terrainBrushEditor.BrushParamsChanged += OnBrushParamsChanged;
            m_tool = IOC.Resolve<ITerrainAreaTool>();
        }


        private void OnDestroy()
        {
            m_tool = null;

            if(m_terrainBrushEditor != null)
            {
                m_terrainBrushEditor.SelectedBrushChanged -= OnSelectedBrushChanged;
                m_terrainBrushEditor.BrushParamsChanged -= OnBrushParamsChanged;
            }
        }

        private void OnEnable()
        {
            m_tool.IsActive = true;

            if(m_terrainBrushEditor.SelectedBrush != null)
            {
                m_tool.Brush = m_terrainBrushEditor.SelectedBrush.texture;
                m_tool.BrushOpacity = m_terrainBrushEditor.BrushOpacity;
            }
        }

        private void OnDisable()
        {
            m_tool.IsActive = false;
        }

        private void OnSelectedBrushChanged(object sender, EventArgs e)
        {
            m_tool.Brush = m_terrainBrushEditor.SelectedBrush.texture;
            m_tool.BrushOpacity = m_terrainBrushEditor.BrushOpacity;
        }

        private void OnBrushParamsChanged(object sender, EventArgs e)
        {
            m_tool.BrushOpacity = m_terrainBrushEditor.BrushOpacity;
        }

    }
}
