using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor.Views
{
    public class BoolToImageAlphaConverter : MonoBehaviour
    {
        [SerializeField]
        private float m_trueValue = 1.0f;

        [SerializeField]
        private float m_falseValue = 0.5f;

        private bool m_value;
        public bool Value
        {
            get { return m_value; }
            set
            {
                if(m_value != value)
                {
                    SetValue(value);
                }
            }
        }

        private void SetValue(bool value)
        {
            m_value = value;

            Color color = m_targetImage.color;
            color.a = m_value ? m_trueValue : m_falseValue;
            m_targetImage.color = color;
        }

        [SerializeField]
        private Image m_targetImage = null;

        private void Awake()
        {
            if(m_targetImage == null)
            {
                m_targetImage = GetComponent<Image>();
            }
        }

        private void Start()
        {
            SetValue(Value);
        }
    }
}
