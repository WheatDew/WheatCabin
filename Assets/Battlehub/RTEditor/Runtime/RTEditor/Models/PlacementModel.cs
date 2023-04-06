using Battlehub.RTCommon;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{

    [DefaultExecutionOrder(-100)]
    public class PlacementModel : MonoBehaviour, IPlacementModel
    {
        protected virtual void Awake()
        {
            if(!IOC.IsFallbackRegistered<IPlacementModel>())
            {
                IOC.RegisterFallback<IPlacementModel>(this);
            }
        }
        protected virtual void OnDestroy()
        {
            IOC.UnregisterFallback<IPlacementModel>(this);
        }

        public virtual Plane GetDragPlane(Transform cameraTransform)
        {
            Vector3 up;
            if (Mathf.Abs(Vector3.Dot(cameraTransform.up, Vector3.up)) > Mathf.Cos(Mathf.Deg2Rad))
            {
                up = Vector3.Cross(cameraTransform.right, Vector3.up);
            }
            else
            {
                up = Vector3.up;
            }

            IScenePivot pivot = GetSelectionComponent();
            return GetDragPlane(up, pivot.SecondaryPivot);
        }

        public virtual Plane GetDragPlane(Vector3 normal, Vector3 point)
        {
            return new Plane(normal, point);
        }

        public virtual bool GetPointOnDragPlane(Plane plane, Ray ray, out Vector3 point)
        {
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                point = ray.GetPoint(distance);
                return true;
            }
            point = Vector3.zero;
            return false;
        }

        public virtual bool GetHitPoint(Ray ray, out Vector3 point)
        {
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                point = hit.point;
                return true;
            }

            point = Vector3.zero; 
            return false;
        }

        public virtual IRuntimeSelectionComponent GetSelectionComponent()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            if (editor.ActiveWindow != null)
            {
                IRuntimeSceneComponent scenePivot = editor.ActiveWindow.IOCContainer.Resolve<IRuntimeSceneComponent>();
                if (scenePivot != null)
                {
                    return scenePivot;
                }
            }

            RuntimeWindow sceneWindow = editor.GetWindow(RuntimeWindowType.Scene);
            if (sceneWindow != null)
            {
                IRuntimeSelectionComponent scenePivot = sceneWindow.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                if (scenePivot != null)
                {
                    return scenePivot;
                }
            }

            return null;
        }

        public virtual void AddGameObjectToScene(GameObject go, bool select = true)
        {
            Vector3 pivot = Vector3.zero;
            IRuntimeSelectionComponent selectionComponent = GetSelectionComponent();
            if (selectionComponent != null)
            {
                pivot = selectionComponent.SecondaryPivot;
            }

            AddGameObjectToScene(go, pivot, select);
        }

        public virtual void AddGameObjectToScene(GameObject go, Vector3 pivot, bool select = true)
        {
            IRuntimeSelectionComponent selectionComponent = GetSelectionComponent();
            IRTE editor = IOC.Resolve<IRTE>();

            editor.AddGameObjectToHierarchy(go);

            go.transform.position = pivot;
            if (go.GetComponent<ExposeToEditor>() == null)
            {
                go.AddComponent<ExposeToEditor>();
            }

            go.transform.SetSiblingIndex(int.MaxValue);
            go.SetActive(true);
            editor.RegisterCreatedObjects(new[] { go }, select && (selectionComponent != null ? selectionComponent.CanSelect : true));
        }
    }
}
