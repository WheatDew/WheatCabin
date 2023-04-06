
using Battlehub.UIControls.DockPanels;

namespace Battlehub.RTEditor.Views
{
    public class ProjectView : View
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            DockPanel dockPanelsRoot = GetComponent<DockPanel>();
            if (dockPanelsRoot != null)
            {
                dockPanelsRoot.CursorHelper = Editor.CursorHelper;
            }
        }
    }

}
