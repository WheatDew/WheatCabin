using Battlehub.RTCommon;
using Battlehub.RTEditor.ViewModels;
using Battlehub.RTEditor.Views;

namespace Battlehub.RTEditor.Demo
{
    public class ReplaceSceneViewAndViewModelExample : RuntimeWindowExtension
    {
        public override string WindowTypeName
        {
            get { return RuntimeWindowType.Scene.ToString(); }
        }

        protected override void Extend(RuntimeWindow window)
        {
            View.ReplaceWith<SceneViewOverrideExample>(window, false);
            ViewModel.ReplaceWith<SceneViewModelOverrideExample>(window);
        }
    }

}
