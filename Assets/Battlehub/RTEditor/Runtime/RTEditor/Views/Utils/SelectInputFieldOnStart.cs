using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor.Views
{
    public class SelectInputFieldOnStart : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField m_inputField;

        [SerializeField]
        private bool m_ignoreIfTouchInputSupported = true;

        private void Start()
        {
            if(!Input.touchSupported || !m_ignoreIfTouchInputSupported)
            {
                if (m_inputField == null)
                {
                    m_inputField = GetComponent<TMP_InputField>();
                }
                if (m_inputField != null)
                {
                    m_inputField.Select();
                }
            }

            
            Destroy(this);
        }
    }

}
