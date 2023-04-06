using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;

namespace Battlehub.RTEditor.Examples.Scene11
{
    public class ExtendSceneDropTargetWindowExample : RuntimeWindowExtension
    {
        public override string WindowTypeName => BuiltInWindowNames.Scene;

        protected override void Extend(RuntimeWindow window)
        {
            ViewModelBase.ReplaceWith<SceneDropTargetViewModelExample>(window);
        }
    }
}
