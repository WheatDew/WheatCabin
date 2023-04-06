using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.UIControls.DockPanels;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class SceneParametersOverrideExample : EditorExtension
    {
        private IWindowManager m_wm;
        protected override void OnEditorExist()
        {
            m_wm = IOC.Resolve<IWindowManager>();
            m_wm.AfterLayout += OnAfterLayout;
            m_wm.WindowCreated += OnWindowCreated;
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            if(m_wm != null)
            {
                m_wm.AfterLayout -= OnAfterLayout;
                m_wm.WindowCreated -= OnWindowCreated;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_wm != null)
            {
                m_wm.AfterLayout -= OnAfterLayout;
                m_wm.WindowCreated -= OnWindowCreated;
            }
        }

        private void OnWindowCreated(Transform window)
        {
            SetupScene(window);
        }

        private void OnAfterLayout(IWindowManager wm)
        {
            Transform[] windows = wm.GetWindows(RuntimeWindowType.Scene.ToString());
            for(int i = 0; i < windows.Length; ++i)
            {
                SetupScene(windows[i]);
            }
        }

        private void SetupScene(Transform w)
        {
            RuntimeWindow window = w.GetComponent<RuntimeWindow>();
            if(window == null)
            {
                return;
            }

            IRuntimeSceneComponent scene = window.IOCContainer.Resolve<IRuntimeSceneComponent>();
            if(scene == null)
            {
                return;
            }

            scene.Pivot = new Vector3(5, 0, 0);
            scene.CameraPosition = Vector3.right * 20;
            scene.IsOrthographic = true;

            scene.PositionHandle.GridSize = 2;
            scene.RotationHandle.GridSize = 5;
            scene.SizeOfGrid = 2;

            scene.IsScaleHandleEnabled = false;
            scene.IsSceneGizmoEnabled = true;
            scene.IsBoxSelectionEnabled = false;

            scene.CanSelect = true;
            scene.CanSelectAll = true;

            scene.CanRotate = true;
            scene.CanPan = false;
            scene.CanZoom = true;

            Tab tab = Region.FindTab(window.transform);
            tab.CanClose = false;

            scene.SceneGizmo.Anchor = new Vector2(1, 1);
            scene.SceneGizmo.PivotPoint = new Vector2(1, 1);
            scene.SceneGizmoTransform.anchorMax = new Vector2(1, 0);
            scene.SceneGizmoTransform.anchorMin = new Vector2(1, 0);
            scene.SceneGizmoTransform.pivot = new Vector2(1, 0);
        }
    }
}
