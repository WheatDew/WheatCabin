using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene9
{
    /// <summary>
    /// This is an example of how to add menu items to the existing main menu of the runtime editor.
    /// </summary>
    [MenuDefinition]
    public class AddMenuItemsToBuiltInMenuExample : MonoBehaviour
    {
        /// <summary>
        /// Adds a menu item to the File menu with the text "My File Command" and the "RTE_Settings" icon from the Resources folder.
        /// </summary>
        [MenuCommand("MenuFile/My File Command", "RTE_Settings")]
        public void MenuFileCommand()
        {
            Debug.Log("My File Command");
        }

        /// <summary>
        /// Adds a menu item with priority 0 to the top of the menu. The lower the priority value, the higher in the menu the menu item will be displayed.
        /// </summary>
        [MenuCommand("MenuFile/My File Command 0", priority:0)]
        public void MenuFileCommandWithPriority0()
        {
            Debug.Log("My File Command 0");
        }

        [MenuCommand("MenuFile/My File Command 15", priority: 15)]
        public void MenuFileCommandWithPriority15()
        {
            Debug.Log("My File Command 15");
        }

        [MenuCommand("MenuEdit/My Edit Command")]
        public void MenuEditCommand()
        {
            Debug.Log("My Edit Command");
        }

        [MenuCommand("MenuGameObject/My Game Object Command")]
        public void MenuGameObjectCommand()
        {
            Debug.Log("My Game Object Command");
        }

        [MenuCommand("MenuWindow/My Window Command")]
        public void MenuWindowCommand()
        {
            Debug.Log("My Window Command");
        }

        [MenuCommand("MenuWindow/General/My Window Command")]
        public void MenuWindowGeneralSubmenuCommand()
        {
            Debug.Log("My Window Command");
        }

        [MenuCommand("MenuHelp/My Help Command")]
        public void MenuHelpCommand()
        {
            Debug.Log("My Help Command");
        }
    }
}

