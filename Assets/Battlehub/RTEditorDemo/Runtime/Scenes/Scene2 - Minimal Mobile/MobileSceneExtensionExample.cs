using Battlehub.RTCommon;
using Battlehub.RTHandles;

namespace Battlehub.RTEditor.Examples.Scene2
{
    /// <summary>
    /// Extends Scene Window with mobile input hander
    /// </summary>
    public class MobileSceneExtensionExample : RuntimeWindowExtension
    {
        public override string WindowTypeName => BuiltInWindowNames.Scene;

        protected override void Extend(RuntimeWindow window)
        {
            //Extend if touch input supported
            if(window.Editor.TouchInput.IsTouchSupported)
            {
                IRuntimeSceneComponent sceneComponent = window.IOCContainer.Resolve<IRuntimeSceneComponent>();
                sceneComponent.GameObject.AddComponent<MobileSceneInput>();
                sceneComponent.IsBoxSelectionEnabled = false;
            }
        }
    }
}
