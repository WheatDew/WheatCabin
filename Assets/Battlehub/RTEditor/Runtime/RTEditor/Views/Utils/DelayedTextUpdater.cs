using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Battlehub.RTEditor.Views
{
    public class DelayedTextUpdater : MonoBehaviour
    {
        [SerializeField]
        private float m_delay = 0.3f;

        public UnityEvent TextChanged = new UnityEvent();

        private string m_text;
        public string Text
        {
            get { return m_text; }
            set 
            {
                if(m_text != value)
                {
                    m_text = value;
                    m_inputField.SetTextWithoutNotify(m_text);
                }
            }
        }

        private TMP_InputField m_inputField;
        private IEnumerator m_coUpdateText;
        private WaitForSeconds m_waitForSeconds;
        private void Awake()
        {
            m_inputField = GetComponent<TMP_InputField>();            
            m_inputField.onValueChanged.AddListener(OnValueChanged);

            m_waitForSeconds = new WaitForSeconds(m_delay);

            Text = m_inputField.text;
            TextChanged?.Invoke();
        }

        private void OnDestroy()
        {
            if(m_inputField != null)
            {
                m_inputField.onValueChanged.RemoveListener(OnValueChanged);
            }

            if(m_coUpdateText != null)
            {
                StopCoroutine(m_coUpdateText);
                m_coUpdateText = null;
            }
        }

        private void OnValueChanged(string value)
        {
            if(m_coUpdateText != null)
            {
                StopCoroutine(m_coUpdateText);
            }

            m_coUpdateText = CoUpdateText();
            StartCoroutine(m_coUpdateText);
        }


        private IEnumerator CoUpdateText()
        {
            yield return m_waitForSeconds;

            Text = m_inputField.text;
            TextChanged?.Invoke();

            m_coUpdateText = null;
        }
    }

}
