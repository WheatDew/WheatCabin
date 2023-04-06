using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene9
{
    /// <summary>
    /// This is an example of how to override existing main menu items
    /// </summary>
    [MenuDefinition]
    public class OverrideMenuItemsInBuiltInMenuExample : MonoBehaviour
    {
        [MenuCommand("MenuFile/Close")]
        public void OverridenCloseButton()
        {
            Debug.Log("Close");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

        }
    }
}
