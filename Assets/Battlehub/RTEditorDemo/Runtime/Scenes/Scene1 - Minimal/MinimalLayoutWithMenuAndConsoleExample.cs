using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene1
{
    public class MinimalLayoutWithMenuAndConsoleExample : LayoutExtension
    {
        [SerializeField]
        private GameObject m_sceneWindow = null;

        protected override void OnInit()
        {
            base.OnInit();

            //Disable foreground ui layer.
            //Better in terms of performance, but does not allow to switch SceneWindow and GameWindow to "floating" mode when using UnversalRP or HDRP
            RenderPipelineInfo.UseForegroundLayerForUI = false;

            //Hide footer
            IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
            appearance.IsMainMenuActive = true;
            appearance.IsFooterActive = false;
            appearance.IsUIBackgroundActive = true;
        }

        protected override void OnRegisterWindows(IWindowManager wm)
        {
            if(m_sceneWindow != null)
            {
                //Override scene window with borderless variant
                wm.OverrideWindow(BuiltInWindowNames.Scene, m_sceneWindow);
            }
        }

        protected override void OnBeforeBuildLayout(IWindowManager wm)
        {
            //Hide header toolbar
            wm.OverrideTools(null);
        }

        protected override LayoutInfo GetLayoutInfo(IWindowManager wm)
        {
            LayoutInfo scene = wm.CreateLayoutInfo(BuiltInWindowNames.Scene);
            scene.IsHeaderVisible = false;

            LayoutInfo console = wm.CreateLayoutInfo(BuiltInWindowNames.Console);

            return LayoutInfo.Horizontal(scene, console, 0.75f);
        }
    }

}
