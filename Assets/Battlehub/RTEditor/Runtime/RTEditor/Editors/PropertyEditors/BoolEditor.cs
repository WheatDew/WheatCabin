using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class BoolEditor : PropertyEditor<bool>
    {
        [SerializeField]
        private Toggle m_input = null;

        [SerializeField]
        private TextMeshProUGUI m_mixedValuesIndicator = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_input.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_input != null)
            {
                m_input.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        protected override void SetInputField(bool value)
        {
            if(HasMixedValues())
            {
                m_input.SetIsOnWithoutNotify(false);
                if(m_mixedValuesIndicator != null)
                {
                    m_mixedValuesIndicator.text = "-";
                }
            }
            else
            {
                m_input.SetIsOnWithoutNotify(value);
                if(m_mixedValuesIndicator != null)
                {
                    m_mixedValuesIndicator.text = null;
                }
            }
        }

        private void OnValueChanged(bool value)
        {
            BeginEdit();
            SetValue(value);
            SetInputField(value);
            EndEdit();
        }
    }
}

