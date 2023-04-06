using Battlehub.UIControls.DockPanels;

namespace Battlehub.RTEditor.Examples.Scene1
{
    public class OnlySceneLayoutExample : LayoutExtension
    {
        protected override void OnBeforeBuildLayout(IWindowManager wm)
        {
            //Hide header toolbar
            wm.OverrideTools(null);
        }

        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            //Initializing a layout with one window - Scene
            LayoutInfo layoutInfo = wm.CreateLayoutInfo(BuiltInWindowNames.Scene);
            layoutInfo.IsHeaderVisible = false;

            return layoutInfo;
        }
    }

}
