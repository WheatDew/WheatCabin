using Battlehub.UIControls.DockPanels;

namespace Battlehub.RTEditor.Examples.Scene5
{
    /// <summary>
    /// Creates default layout for this example (inspector, (scene, hierarchy))
    /// </summary>
    public class ThreeColumsLayoutExample : LayoutExtension
    { 
        protected override void OnBeforeBuildLayout(IWindowManager wm)
        {
            //Hide header toolbar
            wm.OverrideTools(null);
        }

        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            LayoutInfo scene = wm.CreateLayoutInfo(BuiltInWindowNames.Scene);
            scene.IsHeaderVisible = true;

            LayoutInfo hierarchy = wm.CreateLayoutInfo(BuiltInWindowNames.Hierarchy);
            LayoutInfo inspector = wm.CreateLayoutInfo(BuiltInWindowNames.Inspector);

            //Defines a region divided into two equal parts (ratio 1 / 2)
            LayoutInfo sceneAndHierarchy = LayoutInfo.Horizontal(scene, hierarchy, ratio: 1 / 2.0f);

            //Defines a region divided into two parts (1/3 for the inspector and 2/3 for the scene and hierarchy)
            LayoutInfo layoutRoot = LayoutInfo.Horizontal(inspector, sceneAndHierarchy, ratio: 1 / 3.0f);
            
            return layoutRoot;
        }
    }

}
