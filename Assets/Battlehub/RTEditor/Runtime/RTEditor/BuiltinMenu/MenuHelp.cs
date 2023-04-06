using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor
{

    [MenuDefinition(order: -50)]
    public class MenuHelp : MonoBehaviour
    {
        private IRuntimeEditor Editor
        {
            get { return IOC.Resolve<IRuntimeEditor>(); }
        }

        [MenuCommand("MenuHelp/About RTE", priority:10)]
        public void Help()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.About.ToString());
        }
    }
}
