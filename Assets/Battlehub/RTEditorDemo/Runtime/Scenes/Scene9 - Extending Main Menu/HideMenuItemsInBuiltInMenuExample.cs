using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene9
{
    /// <summary>
    /// This is an example of how to hide existing main menu items
    /// </summary>
    [MenuDefinition]
    public class HideMenuItemsInBuiltInMenuExample : MonoBehaviour
    {
        [MenuCommand("MenuHelp/About RTE", hide: true)]
        public void HideAbout() {  }
        
        [MenuCommand("MenuWindow/General/Animation", hide:true)]
        public void HideAnimation() {  }

    }
}
