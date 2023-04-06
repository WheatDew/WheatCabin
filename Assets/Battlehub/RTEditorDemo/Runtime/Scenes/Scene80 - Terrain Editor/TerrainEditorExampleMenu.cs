using Battlehub.RTCommon;
using Battlehub.RTTerrain;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene80
{
    [MenuDefinition]
    public class TerrainEditorExampleMenu : MonoBehaviour
    {
        [MenuCommand("Example/Create Terrain")]  
        public void CreateTerrain()
        {
            IRTE edtior = IOC.Resolve<IRTE>();

            TerrainInit terrainInit = FindObjectOfType<TerrainInit>();
            edtior.Selection.activeGameObject = terrainInit.CreateTerrain().gameObject;
        }

        [MenuCommand("Example/Import Texture")]
        public void ImportTexture()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow(BuiltInWindowNames.ImportFile);
        }
    }
}
