using Battlehub.UIControls.DockPanels;

namespace Battlehub.RTEditor.Examples.Scene8
{
    public class NullLayoutExample : LayoutExtension
    {
        protected override void OnBeforeBuildLayout(IWindowManager wm)
        {
            base.OnBeforeBuildLayout(wm);
            wm.OverrideTools(null);
        }

        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            return null;
        }
    }
}

