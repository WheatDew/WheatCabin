using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class SetWindowSizeExample : EditorExtension
    {
        private IWindowManager m_wm;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            m_wm = IOC.Resolve<IWindowManager>();
            Subscribe();
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            Unsubscribe();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Unsubscribe();
        }

        private void Subscribe()
        {
            m_wm.WindowCreated += OnWindowCreated;
        }

        private void Unsubscribe()
        {
            if (m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
            }
        }

        private void OnWindowCreated(Transform windowTransform)
        {
            var region = windowTransform.GetComponentInParent<UIControls.DockPanels.Region>();
            region.MinHeight = 300;
            region.MinWidth = 300;

            RectTransform rt = (RectTransform)region.transform;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 400);
        }
    }
}
