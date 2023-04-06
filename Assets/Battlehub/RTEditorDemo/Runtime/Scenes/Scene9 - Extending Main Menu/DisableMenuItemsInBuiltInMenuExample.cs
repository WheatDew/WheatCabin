using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene9
{
    /// <summary>
    /// This is an example on how to disable existing main menu items
    /// </summary>
    [MenuDefinition]
    public class DisableMenuItemsInBuiltInMenuExample : MonoBehaviour
    {
        [MenuCommand("MenuFile/New Scene", validate:true, priority:10)]
        public bool IsNewSceneEnabled() 
        {
            return false;
        }
    }
}
