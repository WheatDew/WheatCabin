using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene9
{
    /// <summary>
    /// This is how to hide main menu completely
    /// </summary>
    public class HideMainMenuExample : EditorExtension
    {
        public bool m_hide = false;

        protected override void OnInit()
        {
            base.OnInit();

            if(m_hide)
            {
                IRTEAppearance appearance = IOC.Resolve<IRTEAppearance>();
                appearance.IsMainMenuActive = false;
            }
        }
    }

}

