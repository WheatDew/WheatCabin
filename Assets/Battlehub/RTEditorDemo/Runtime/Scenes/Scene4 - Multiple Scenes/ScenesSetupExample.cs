using Battlehub.RTCommon;
using Battlehub.RTHandles;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene4
{
    /// <summary>
    /// This extension initializes the scene window according to the parameters passed from MultipleScenesLayoutExample.cs
    /// </summary>
    public class ScenesSetupExample : RuntimeWindowExtension
    {
        /// <summary>
        /// Type of window to be extended
        /// </summary>
        public override string WindowTypeName => BuiltInWindowNames.Scene;

        protected override void Extend(RuntimeWindow window)
        {
            //If the window is meant to be 3D Perspective, do not change any settings
            if (window.Args == SceneArgsExample.ThreeD)
            {
                return;
            }

            //Otherwise get a reference to the IRuntimeSceneComponent of the window
            IRuntimeSceneComponent sceneComponent = window.IOCContainer.Resolve<IRuntimeSceneComponent>();

            //This is the point the camera looks at and orbiting around
            sceneComponent.Pivot = Vector3.zero;

            //Switch the scene component and scene camera to orthographic mode
            sceneComponent.IsOrthographic = true;

            //Disable scene gizmo
            sceneComponent.IsSceneGizmoEnabled = false;

            //Disable rotation
            sceneComponent.CanRotate = false;

            //Disable free move
            sceneComponent.CanFreeMove = false;

            //Prevent camera position changes when zooming in and out
            sceneComponent.ChangeOrthographicSizeOnly = true;

            //Set initial orthographic size
            sceneComponent.OrthographicSize = 5.0f;

            //Set camera position according to window.Args
            const float distance = 100;
            switch (window.Args)
            {
                case SceneArgsExample.XY:
                    sceneComponent.CameraPosition = -Vector3.forward * distance;
                    break;
                case SceneArgsExample.XZ:
                    sceneComponent.CameraPosition = Vector3.up * distance;
                    break;
                case SceneArgsExample.YZ:
                    sceneComponent.CameraPosition = -Vector3.right * distance;
                    break;
            }
        }
    }
}

