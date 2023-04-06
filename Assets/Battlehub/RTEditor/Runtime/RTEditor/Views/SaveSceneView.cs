
using Battlehub.RTCommon;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor.Views
{
    public class SaveSceneView : View
    {
        [SerializeField]
        private TMP_InputField m_inputField = null;

        public bool ActivateInputField
        {
            get { return false; }
            set
            {
                if (IOC.Resolve<IRTE>().TouchInput.IsTouchSupported)
                {
                    return;
                }

                if (value)
                {
                    if (gameObject.activeInHierarchy)
                    {
                        StartCoroutine(CoActivateInputField());
                    }
                    else
                    {
                        m_inputField.ActivateInputField();
                    }
                }
                else
                {
                    m_inputField.DeactivateInputField();
                }
            }
        }

        private System.Collections.IEnumerator CoActivateInputField()
        {
            yield return new WaitForEndOfFrame();
            if (m_inputField != null)
            {
                m_inputField.ActivateInputField();
            }
        }

    }
}

