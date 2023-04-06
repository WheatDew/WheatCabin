using Battlehub.RTCommon;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTTerrain.Demo
{
    public class TerrainEditorDemo : MonoBehaviour
    {
        [SerializeField]
        private TerrainEditor m_terrainEditor = null;

        private IRTE m_editor;
        private void Start()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Selection.SelectionChanged += OnSelectionChanged;

            TerrainInit terrainInit = FindObjectOfType<TerrainInit>(); 
            terrainInit.CreateTerrain();
        }

        private void OnDestroy()
        {
            if(m_editor != null)
            {
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            if(m_editor.Selection.activeGameObject != null)
            {
                m_terrainEditor.Terrain = m_editor.Selection.gameObjects.Select(go => go.GetComponent<Terrain>()).Where(t => t != null).FirstOrDefault();
            }
            else
            {
                m_terrainEditor.Terrain = null;
            }   
        }

    }

}
