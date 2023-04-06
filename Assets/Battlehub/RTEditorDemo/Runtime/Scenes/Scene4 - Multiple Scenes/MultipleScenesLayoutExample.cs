using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene4
{
    /// <summary>
    /// These are the arguments passed to each scene window so that it can initialize itself correctly (even when restoring layout from PlayerPrefs or json file)
    /// </summary>
    public static class SceneArgsExample
    {
        public const string ThreeD = "3D";
        public const string XY = "XY";
        public const string XZ = "XZ";
        public const string YZ = "YZ";
    }

    /// <summary>
    /// Initializes runtime editor with 4 scene windows - (3D Perspective, XZ Ortho, XY Ortho, YZ Ortho).
    /// </summary>
    public class MultipleScenesLayoutExample : LayoutExtension
    {
        [SerializeField]
        private GameObject m_sceneWindow = null;

        protected override void OnInit()
        {
            base.OnInit();

            //Unique name
            PersistentLayoutName = typeof(MultipleScenesLayoutExample).FullName;

            //Tells the extension to save changes made by the user.
            PersistentLayout = true;

            if (!PersistentLayout)
            {
                DeleteLayout();
            }

            //Disable foreground ui layer.
            //Better in terms of performance, but does not allow to switch SceneWindow and GameWindow to "floating" mode when using UnversalRP or HDRP
            RenderPipelineInfo.UseForegroundLayerForUI = false;

            //Hide main menu and footer
            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            appearance.IsMainMenuActive = false;
            appearance.IsFooterActive = false;
            appearance.IsUIBackgroundActive = false;
        }

        protected override void OnRegisterWindows(IWindowManager wm)
        {
            if(m_sceneWindow != null)
            {
                //Override scene window with variant
                wm.OverrideWindow(BuiltInWindowNames.Scene, new WindowDescriptor { ContentPrefab = m_sceneWindow, MaxWindows = 4 });
            }
            
        }

        protected override void OnBeforeBuildLayout(IWindowManager wm)
        {
            //Hide header toolbar
            wm.OverrideTools(null);
        }

        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            //Initializing a layout with four scene windows.
            //A parameter is passed to each scene that tells it how it should be initialized

            LayoutInfo scene0 = wm.CreateLayoutInfo(BuiltInWindowNames.Scene, SceneArgsExample.ThreeD);
            scene0.IsHeaderVisible = false;

            LayoutInfo scene1 = wm.CreateLayoutInfo(BuiltInWindowNames.Scene, SceneArgsExample.YZ);
            scene1.IsHeaderVisible = false;

            LayoutInfo scene2 = wm.CreateLayoutInfo(BuiltInWindowNames.Scene, SceneArgsExample.XY);
            scene2.IsHeaderVisible = false;

            LayoutInfo scene3 = wm.CreateLayoutInfo(BuiltInWindowNames.Scene, SceneArgsExample.XZ);
            scene3.IsHeaderVisible = false;

            //First row is divided into equal areas with scene0 and scene1
            LayoutInfo row0 = LayoutInfo.Horizontal(scene0, scene1, 0.5f);

            //Second row is divided into equal areas with scene2 and scene3
            LayoutInfo row1 = LayoutInfo.Horizontal(scene2, scene3, 0.5f);

            //The first and second rows are stacked to form the final layout
            LayoutInfo layout = LayoutInfo.Vertical(row0, row1, 0.5f);

            return layout;
        }
    }

}
