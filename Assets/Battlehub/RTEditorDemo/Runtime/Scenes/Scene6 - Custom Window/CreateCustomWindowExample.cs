using Battlehub.UIControls.DockPanels;

namespace Battlehub.RTEditor.Examples.Scene6
{
    /// <summary>
    /// This script creates a custom floating window after the completion of the initial layout
    /// </summary>
    public class CreateCustomWindowExample : LayoutExtension
    {
        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            return LayoutInfo.Horizontal(
                wm.CreateLayoutInfo(BuiltInWindowNames.Scene),
                wm.CreateLayoutInfo("CustomExample"));
        }

        protected override void OnAfterBuildLayout(IWindowManager wm)
        {
            base.OnAfterBuildLayout(wm);
            wm.CreateWindow("CustomExample", true);
        }
    }

}
