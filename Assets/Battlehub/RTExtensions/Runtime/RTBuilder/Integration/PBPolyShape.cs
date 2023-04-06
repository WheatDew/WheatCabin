namespace Battlehub.ProBuilderIntegration
{
    public class PBPolyShape : PBComplexShape
    {
        protected override void CreateShape()
        {
            Target.CreateShapeFromPolygon(Selection.Positions, 0.001f, false);
        }

        protected override PBComplexShapeSelection CreateSelectionObject()
        {
            return gameObject.AddComponent<PBComplexShapeSelection>();
        }

    }
}

