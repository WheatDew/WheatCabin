using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class InputViewModel : MonoBehaviour
    {
        private string m_text = string.Empty;
        [Binding]
        public string Text
        {
            get { return m_text; }
            set { m_text = value; }
        }

    }
}
