using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene2
{
    /// <summary>
    /// This script fills the top and bottom bars with a panels containing some useful controls.
    /// </summary>
    public class TopAndBottomBarInitExample : EditorExtension
    {
        [SerializeField]
        private RectTransform m_topBar = null;

        [SerializeField]
        private RectTransform m_bottomBar = null;

        protected override void OnInit()
        {
            base.OnInit();

            if(m_bottomBar != null)
            {
                IWindowManager windowManager = IOC.Resolve<IWindowManager>();
                windowManager.SetTopBar(Instantiate(m_topBar));
                windowManager.SetBottomBar(Instantiate(m_bottomBar));
                
                //The left and right panels can be filled in the same way
                //windowManager.SetLeftBar(null);
                //windowManager.SetRightBar(null);
            }
        }

    }
}
