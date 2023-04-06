using UnityEngine;

namespace Battlehub.RTTerrain
{
    public class TerrainRaiseOrLowerEditor : TerrainPaintEditor
    {
        protected override Brush CreateBrush()
        {
            return new TerrainBrush();
        }
    }
}

