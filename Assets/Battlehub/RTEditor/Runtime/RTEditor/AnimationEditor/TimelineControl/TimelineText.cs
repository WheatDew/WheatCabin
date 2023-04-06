using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class TimelineText : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_text;

        public string Text
        {
            set { m_text.text = value; }
        }

        private bool m_isSecondary;
        public bool IsSecondary
        {
            get { return m_isSecondary; }
            set { m_isSecondary = value; }
        }

        private bool m_isPrimary;
        public bool IsPrimary
        {
            get { return m_isPrimary; }
            set { m_isPrimary = value; }
        }

        public bool IsVisible
        {
            set
            {
                m_text.alpha = value ? 1 : 0;
            }
        }

        private void Awake()
        {
            if(m_text == null)
            {
                m_text = GetComponent<TextMeshProUGUI>();
            }
        }

        public void ForceMeshUpdate()
        {
            m_text.ForceMeshUpdate();
        }
            
        public void Refresh(float primarySpace, float secondarySpace, string text)
        {
            float charWidth = m_text.GetPreferredValues("0").x;

            if (m_isSecondary)
            {
                if (secondarySpace < text.Length * charWidth + 10)
                {
                    m_text.alpha = 0;
                }
                else
                {
                    m_text.alpha = 1;
                }
            }
            else if(m_isPrimary)
            {
                if (primarySpace < text.Length * charWidth + 10)
                {
                    m_text.alpha = 0;
                }
                else
                {
                    m_text.alpha = 1;
                }
            }
        }
    }

}
