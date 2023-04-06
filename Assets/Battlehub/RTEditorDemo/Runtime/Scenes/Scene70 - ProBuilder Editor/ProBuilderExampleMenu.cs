using Battlehub.ProBuilderIntegration;
using Battlehub.RTBuilder;
using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene70
{
    /// <summary>
    /// Here is an example of using the IProBuilderTool 
    /// </summary>
    [MenuDefinition]
    public class ProBuilderExampleMenu : MonoBehaviour
    {
        [MenuCommand("Example/Create And Probuilderize Object")]
        public void CreateAndProbuilderizeObject()
        {
            GameObject go = CreatePrimitive();
            go.AddComponent<PBMesh>();    
        }

        [MenuCommand("Example/Create And Select Face")]
        public void SelectFace()
        {
            GameObject go = CreatePrimitive();

            PBMesh mesh = go.AddComponent<PBMesh>();

            IProBuilderTool pbTool = IOC.Resolve<IProBuilderTool>();

            //Set 'Face' mode
            pbTool.Mode = ProBuilderToolMode.Face;

            //Select face with index 1
            MeshSelection selection = new MeshSelection();
            selection.SetFaces(mesh, new[] { 1 });
            pbTool.SetSelection(selection);
        }

        [MenuCommand("Example/Create And Select Edge")]
        public void SelectEdge()
        {
            GameObject go = CreatePrimitive();

            PBMesh mesh = go.AddComponent<PBMesh>();

            IProBuilderTool pbTool = IOC.Resolve<IProBuilderTool>();

            //Set 'Edge' mode
            pbTool.Mode = ProBuilderToolMode.Edge;

            //Select 0, 1 edge
            MeshSelection selection = new MeshSelection();
            selection.SetEdges(mesh, new[] { new PBEdge(0, 1) });
            pbTool.SetSelection(selection);
        }

        [MenuCommand("Example/Create And Select Vertex")]
        public void SelectVertex()
        {
            GameObject go = CreatePrimitive();

            PBMesh mesh = go.AddComponent<PBMesh>();

            IProBuilderTool pbTool = IOC.Resolve<IProBuilderTool>();

            //Set 'Vertex' mode
            pbTool.Mode = ProBuilderToolMode.Vertex;

            //Select vertex with index 5 
            MeshSelection selection = new MeshSelection();
            selection.SetIndices(mesh, new[] { 5 });
            pbTool.SetSelection(selection);
        }

        [MenuCommand("Example/Extrude Face")]
        public void ExtrudeFace()
        {
            SelectFace();

            IProBuilderTool pbTool = IOC.Resolve<IProBuilderTool>();

            //Extrude 5 units in face normal direction
            pbTool.Extrude(5);
        }

        [MenuCommand("Example/Move Face")]
        public void MoveFace()
        {
            SelectFace();

            IProBuilderTool pbTool = IOC.Resolve<IProBuilderTool>();

            IMeshEditor meshEditor = pbTool.GetEditor();
            meshEditor.BeginMove();
            meshEditor.Position += new Vector3(1, 1, 1);
            meshEditor.EndMove();
        }


        [MenuCommand("Example/Rotate Face")]
        public void RotateFace()
        {
            SelectFace();

            IProBuilderTool pbTool = IOC.Resolve<IProBuilderTool>();

            IMeshEditor meshEditor = pbTool.GetEditor();
            meshEditor.BeginRotate(Quaternion.identity);
            meshEditor.Rotate(Quaternion.AngleAxis(45, Vector3.up));
            meshEditor.EndRotate();
        }

        [MenuCommand("Example/Scale Face")]
        public void Scale()
        {
            SelectFace();

            IProBuilderTool pbTool = IOC.Resolve<IProBuilderTool>();

            IMeshEditor meshEditor = pbTool.GetEditor();
            meshEditor.BeginScale();
            meshEditor.Scale(Vector3.one * 2, Quaternion.identity);
            meshEditor.EndScale();
        }

        [MenuCommand("Example/Apply Material")]
        public void ApplyMaterial()
        {
            SelectFace();

            IProBuilderTool pbTool = IOC.Resolve<IProBuilderTool>();

            Material material = new Material(Shader.Find(RenderPipelineInfo.DefaultShaderName));
            material.Color(Color.red);

            pbTool.ApplyMaterial(material);
        }

        private static GameObject CreatePrimitive()
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = Random.onUnitSphere * 3;
            Destroy(go.GetComponent<Collider>());
            go.AddComponent<ExposeToEditor>();
            return go;
        }
    }
}
