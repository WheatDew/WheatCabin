using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public enum PBShapeType
    {
        Cube = 0,
        Stair = 1,
        CurvedStair = 2,
        Prism = 3,
        Cylinder = 4,
        Plane = 5,
        Door = 6,
        Pipe = 7,
        Cone = 8,
        Sprite = 9,
        Arch = 10,
        Sphere = 11,
        Torus = 12
    }

    public static class PBShapeGenerator 
    {
        public static GameObject CreateShape(PBShapeType shapeType)
        {
            Material defaultMaterial;
            //try
            //{
            //    defaultMaterial = BuiltinMaterials.defaultMaterial;
            //}
            //catch
            //{
            //}

            defaultMaterial = PBBuiltinMaterials.DefaultMaterial;
            GameObject shape = ShapeGenerator.CreateShape((ShapeType)shapeType, PivotLocation.Center).gameObject;
            Renderer renderer = shape.GetComponent<Renderer>();
            renderer.sharedMaterial = defaultMaterial;
            return shape;
        }
    }
}


