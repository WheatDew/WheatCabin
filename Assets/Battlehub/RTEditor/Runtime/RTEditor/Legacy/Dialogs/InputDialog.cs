using Battlehub.RTCommon;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*System.Obsolete*/]
    public class InputDialog : RuntimeWindow
    {
        [SerializeField]
        private TMP_InputField m_inputField = null;

        public string Text
        {
            get { return m_inputField.text; }
            private set
            {
                m_inputField.text = value;
            }
        }

        private void Start()
        {
            m_inputField.Select();
        }
    }
}

