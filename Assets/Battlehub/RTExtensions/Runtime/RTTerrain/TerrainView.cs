using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainView : RuntimeWindow
    {
        [SerializeField]
        private TerrainEditor m_terrainEditorPrefab = null;
        private TerrainEditor m_terrainEditor;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Custom;
            base.AwakeOverride();

            TryRefreshTerrainEditor();
            Editor.Selection.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(Editor != null)
            {
                Editor.Selection.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            TryRefreshTerrainEditor();
        }

        private void TryRefreshTerrainEditor()
        {
            Terrain terrain = (Editor.Selection.activeGameObject != null && Editor.Selection.objects.Length == 1) ? Editor.Selection.activeGameObject.GetComponent<Terrain>() : null;
            if (m_terrainEditor != null)
            {
                m_terrainEditor.Terrain = null;
                Destroy(m_terrainEditor.gameObject);
                m_terrainEditor = null;
            }

            if(terrain != null)
            {
                m_terrainEditor = Instantiate(m_terrainEditorPrefab, transform);
                m_terrainEditor.Terrain = terrain;
                m_terrainEditor.gameObject.SetActive(m_terrainEditor.Terrain != null);
            }
        }
    }
}