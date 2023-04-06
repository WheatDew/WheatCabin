using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene9
{
    [MenuDefinition]
    public class CreateYourOwnMenuExample : MonoBehaviour
    {
        private bool m_isMyMenuCommandEnabled;

        [MenuCommand("My Menu/Enable Command")]
        public void EnableMyMenuCommaand()
        {
            m_isMyMenuCommandEnabled = true;
        }

        [MenuCommand("My Menu/My Menu Command", validate: true)]
        public bool MyMenuCommandValidate()
        {
            return m_isMyMenuCommandEnabled;
        }

        [MenuCommand("My Menu/My Menu Command")]
        public void MyMenuCommand()
        {
            Debug.Log("My Menu Command");
        }
    }
}
