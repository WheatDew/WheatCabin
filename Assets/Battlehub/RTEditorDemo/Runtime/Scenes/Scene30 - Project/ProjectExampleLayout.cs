using Battlehub.UIControls.DockPanels;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene31
{
    public class ProjectExampleLayout : LayoutExtension
    {
        [SerializeField]
        private bool m_hideTools = true;

        protected override void OnBeforeBuildLayout(IWindowManager wm)
        {
            base.OnBeforeBuildLayout(wm);

            if (m_hideTools)
            {
                wm.OverrideTools(null);
            }
        }

        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            return LayoutInfo.Vertical
            (
                LayoutInfo.Horizontal
                (
                    wm.CreateLayoutInfo(BuiltInWindowNames.Inspector),
                    LayoutInfo.Horizontal
                    (
                        wm.CreateLayoutInfo(BuiltInWindowNames.Scene),
                        wm.CreateLayoutInfo(BuiltInWindowNames.Hierarchy),
                        ratio: 1 / 2.0f
                    ),
                    ratio: 1 / 3.0f
                ),
                LayoutInfo.Group
                (
                    wm.CreateLayoutInfo(BuiltInWindowNames.Console),
                    wm.CreateLayoutInfo(BuiltInWindowNames.Project)
                ),
                ratio: 1.0f
            );      
        }
    }

}
