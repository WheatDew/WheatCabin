using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [MenuDefinition]
    public class HideBuiltinMenu : MonoBehaviour
    {
        [MenuCommand("MenuFile", requiresInstance: true, hide: true, priority: int.MinValue)]
        public void HideMenuFile() { }

        [MenuCommand("MenuEdit", requiresInstance: true, hide: true, priority: int.MinValue)]
        public void HideMenuEdit() { }

        [MenuCommand("MenuGameObject", requiresInstance: true, hide: true, priority: int.MinValue)]
        public void HideMenuGameObject() { }

        [MenuCommand("MenuWindow", requiresInstance: true, hide: true, priority: int.MinValue)]
        public void HideMenuWindow() { }

        [MenuCommand("MenuHelp", requiresInstance: true, hide: true, priority: int.MinValue)]
        public void HideMenuAbout() { }
    }
}
