using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class HandleDragEventExample : SceneComponentExtension 
    {
        protected override void OnSceneActivated(IRuntimeSceneComponent sceneComponent)
        {
            base.OnSceneActivated(sceneComponent);

            sceneComponent.PositionHandle.Drag.AddListener(OnDrag);
        }

        protected override void OnSceneDeactivated(IRuntimeSceneComponent sceneComponent)
        {
            base.OnSceneDeactivated(sceneComponent);

            sceneComponent.PositionHandle.Drag.RemoveListener(OnDrag);
        }

        private void OnDrag(BaseHandle handle)
        {
            foreach(Transform target in handle.ActiveTargets)
            {
                Vector3 p = target.position;
                target.position = new Vector3(Mathf.Round(p.x), Mathf.Round(p.y), Mathf.Round(p.z));
            }
        }
    }
}
