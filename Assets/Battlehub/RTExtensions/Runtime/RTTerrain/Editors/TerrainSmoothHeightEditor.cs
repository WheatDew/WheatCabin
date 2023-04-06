namespace Battlehub.RTTerrain
{
    public class TerrainSmoothHeightEditor : TerrainPaintEditor
    {
        protected override Brush CreateBrush()
        {
            TerrainBrush terrainBrush = new TerrainBrush();
            terrainBrush.Blend = Brush.BlendFunction.Smooth;
            terrainBrush.AllowNegativeValue = false;
            return terrainBrush;
        }
    }
}

