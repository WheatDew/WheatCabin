namespace Battlehub.RTTerrain
{
    public class TerrainPaintHolesEditor : TerrainPaintEditor
    {
        protected override void Awake()
        {
            TerrainBrushEditor.BrushOpacity = 50;
            base.Awake();
        }

        protected override Brush CreateBrush()
        {
            return new TerrainHolesBrush();
        }
    }
}

