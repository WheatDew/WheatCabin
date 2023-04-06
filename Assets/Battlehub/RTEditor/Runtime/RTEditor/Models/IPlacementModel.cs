using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public interface IPlacementModel
    {
        Plane GetDragPlane(Transform cameraTransform);

        Plane GetDragPlane(Vector3 normal, Vector3 point);

        bool GetPointOnDragPlane(Plane plane, Ray ray, out Vector3 point);

        bool GetHitPoint(Ray ray, out Vector3 point);

        IRuntimeSelectionComponent GetSelectionComponent();

        void AddGameObjectToScene(GameObject go, bool select = true);

        void AddGameObjectToScene(GameObject go, Vector3 pivot, bool select = true);
    }
}
