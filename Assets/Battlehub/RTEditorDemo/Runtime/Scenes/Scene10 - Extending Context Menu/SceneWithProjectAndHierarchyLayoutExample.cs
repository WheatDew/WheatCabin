using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;

namespace Battlehub.RTEditor.Examples.Scene10
{
    /// <summary>
    /// Creates default layout for this example ((scene, hierarchy), project )
    /// </summary>
    public class SceneWithProjectAndHierarchyLayoutExample : LayoutExtension
    {
        protected override void OnBeforeBuildLayout(IWindowManager wm)
        {
            //Hide header toolbar
            wm.OverrideTools(null);

            //Hide the main menu, since it is not used in this example
            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            appearance.IsMainMenuActive = false;
        }

        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            LayoutInfo scene = wm.CreateLayoutInfo(BuiltInWindowNames.Scene);
            scene.IsHeaderVisible = false;

            LayoutInfo hierarchy = wm.CreateLayoutInfo(BuiltInWindowNames.Hierarchy);
            LayoutInfo project = wm.CreateLayoutInfo(BuiltInWindowNames.Project);

            //Defines a region divided into two parts (2/3 of space for scene and 1/3 for hierarchy)
            LayoutInfo sceneAndHierarchy = LayoutInfo.Horizontal(scene, hierarchy, ratio: 2 / 3.0f);

            //Defines a region divided into two parts (2/3 for the scene and hierarchy and 1/3 for project)
            LayoutInfo layoutRoot = LayoutInfo.Vertical(sceneAndHierarchy, project, ratio: 2 / 3.0f);

            return layoutRoot;
        }
    }

}

