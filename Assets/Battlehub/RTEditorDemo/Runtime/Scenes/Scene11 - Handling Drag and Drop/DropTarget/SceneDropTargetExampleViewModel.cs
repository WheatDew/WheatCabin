using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene11
{
    public class SceneDropTargetViewModelExample : SceneViewModel
    {
        public override void OnExternalObjectEnter()
        {
            base.OnExternalObjectEnter();

            object dragObject = ExternalDragObjects.FirstOrDefault();

            if(dragObject is string)
            {
                CanDropExternalObjects = true;
            }

            Debug.Log("On External Drag Enter");
        }

        public override void OnExternalObjectLeave()
        {
            base.OnExternalObjectLeave();

            Debug.Log("On External Drag Leave");
        }

        public override void OnExternalObjectDrag()
        {
            base.OnExternalObjectDrag();

            Debug.Log("On External Drag");
        }

        public override void OnExternalObjectDrop()
        {
            base.OnExternalObjectDrop();

            CreateDragPlane();
            if (!GetPointOnDragPlane(out Vector3 position))
            {
                position = Vector3.zero;
            }

            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            instance.AddComponent<ExposeToEditor>();
            instance.transform.position = position;
            instance.name = ExternalDragObjects.FirstOrDefault().ToString();
          
            Editor.AddGameObjectToHierarchy(instance);
        }
    }
}
