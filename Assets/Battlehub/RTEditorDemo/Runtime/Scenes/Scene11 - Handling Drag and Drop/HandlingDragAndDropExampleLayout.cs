using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene11
{
    public class HandlingDragAndDropExampleLayout : LayoutExtension
    {
        protected override void OnInit()
        {
            base.OnInit();

            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            appearance.IsMainMenuActive = false;
            appearance.IsFooterActive = false;
        }

        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            LayoutInfo scene = wm.CreateLayoutInfo(BuiltInWindowNames.Scene);
            LayoutInfo dragSource = wm.CreateLayoutInfo("DragSourceExample");

            return LayoutInfo.Horizontal(dragSource, scene);
        }
    }

}
