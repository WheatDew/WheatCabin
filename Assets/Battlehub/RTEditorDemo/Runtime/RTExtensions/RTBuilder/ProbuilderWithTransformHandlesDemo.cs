
using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTHandles;
using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTBuilder.Demo
{
    public class ProbuilderWithTransformHandlesDemo : MonoBehaviour
    {
        [SerializeField]
        private Button m_createPrimitiveButton = null;

        [SerializeField]
        private Button m_createPolyshapeButton = null;

        private void Awake()
        {
            UnityEventHelper.AddListener(m_createPrimitiveButton, btn => btn.onClick, OnCreatePrimitive);
            UnityEventHelper.AddListener(m_createPolyshapeButton, btn => btn.onClick, OnCreatePolyshape);
        }

        private void OnDestroy()
        {
            UnityEventHelper.RemoveListener(m_createPrimitiveButton, btn => btn.onClick, OnCreatePrimitive);
            UnityEventHelper.RemoveListener(m_createPolyshapeButton, btn => btn.onClick, OnCreatePolyshape);
        }

        private void OnCreatePrimitive()
        {
            PBShapeType type = PBShapeType.Stair;
            GameObject go;
            ExposeToEditor exposeToEditor;
            CreateShape(type, out go, out exposeToEditor);

            IRTE rte = IOC.Resolve<IRTE>();
            RuntimeWindow scene = rte.GetWindow(RuntimeWindowType.Scene);
            Vector3 position;
            Quaternion rotation;
            GetPositionAndRotation(scene, out position, out rotation);

            go.transform.position = position + rotation * Vector3.up * exposeToEditor.Bounds.extents.y;
            go.transform.rotation = rotation;
        }

        private void OnCreatePolyshape()
        {
            GameObject go;
            ExposeToEditor exposeToEditor;
            CreateShape(PBShapeType.Cube, out go, out exposeToEditor);
            exposeToEditor.SetName("Poly Shape");

            IRTE rte = IOC.Resolve<IRTE>();
            RuntimeWindow scene = rte.GetWindow(RuntimeWindowType.Scene);

            Vector3 position;
            Quaternion rotation;
            GetPositionAndRotation(scene, out position, out rotation);
            go.transform.position = position;
            go.transform.rotation = rotation;

            PBMesh pbMesh = go.GetComponent<PBMesh>();
            pbMesh.Clear();

            PBPolyShape polyShape = go.AddComponent<PBPolyShape>();
            polyShape.IsEditing = true;

            rte.Selection.activeGameObject = go;

            IProBuilderTool tool = IOC.Resolve<IProBuilderTool>();
            tool.CustomTool = CustomToolNames.PolyShape;
        }

        private static void CreateShape(PBShapeType type, out GameObject go, out ExposeToEditor exposeToEditor)
        {
            go = PBShapeGenerator.CreateShape(type);
            go.AddComponent<PBMesh>();

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterials.Length == 1 && renderer.sharedMaterials[0] == PBBuiltinMaterials.DefaultMaterial)
            {
                IMaterialPaletteManager paletteManager = IOC.Resolve<IMaterialPaletteManager>();
                if (paletteManager.Palette.Materials.Count > 0)
                {
                    renderer.sharedMaterial = paletteManager.Palette.Materials[0];
                }
            }

            exposeToEditor = go.AddComponent<ExposeToEditor>();
        }

        private void GetPositionAndRotation(RuntimeWindow window, out Vector3 position, out Quaternion rotation, bool rotateToTerrain = false)
        {
            Ray ray = window != null ?
                new Ray(window.Camera.transform.position, window.Camera.transform.forward) :
                new Ray(Vector3.up * 100000, Vector3.down);

            RaycastHit[] hits = Physics.RaycastAll(ray);
            for (int i = 0; i < hits.Length; ++i)
            {
                RaycastHit hit = hits[i];
                if (hit.collider is TerrainCollider)
                {
                    position = hit.point;
                    if (rotateToTerrain)
                    {
                        rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    }
                    else
                    {
                        rotation = Quaternion.identity;
                    }
                    return;
                }
            }

            Vector3 up = Vector3.up;
            Vector3 pivot = Vector3.zero;
            if (window != null)
            {
                IScenePivot scenePivot = window.IOCContainer.Resolve<IScenePivot>();
                if (Mathf.Abs(Vector3.Dot(window.Camera.transform.up, Vector3.up)) > Mathf.Cos(Mathf.Deg2Rad))
                {
                    up = Vector3.Cross(window.Camera.transform.right, Vector3.up);
                }

                pivot = scenePivot.SecondaryPivot;
            }

            Plane dragPlane = new Plane(up, pivot);
            rotation = Quaternion.identity;
            if (!GetPointOnDragPlane(ray, dragPlane, out position))
            {
                position = window.Camera.transform.position + window.Camera.transform.forward * 10.0f;
            }
        }

        private bool GetPointOnDragPlane(Ray ray, Plane dragPlane, out Vector3 point)
        {
            float distance;
            if (dragPlane.Raycast(ray, out distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            point = Vector3.zero;
            return false;
        }

    }

}
