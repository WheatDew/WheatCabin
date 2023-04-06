using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class BoolFloat 
    {
        public float DisabledValue = -1;
        public float EnabledValue = 0;
        public BoolFloat(float disabledValue = -1, float enabledValue = 0) 
        {
            DisabledValue = disabledValue;
            EnabledValue = enabledValue;
        }
    }

    public class BoolFloatEditor : FloatEditor
    {
        [SerializeField]
        protected Toggle m_toggle;

        [SerializeField]
        private TextMeshProUGUI m_mixedValuesIndicator = null;

        public float DisabledValue = -1;
        public float EnabledValue = 0;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_toggle.onValueChanged.AddListener(OnValueChanged);
            m_input.gameObject.SetActive(m_toggle.isOn);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_input != null)
            {
                m_toggle.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        protected override void SetInputField(float value)
        {
            base.SetInputField(value);

            bool wasOn = m_toggle.isOn;

            if (GetValues().Any(v => v < EnabledValue) && GetValues().Any(v => v >= EnabledValue))
            {
                m_toggle.SetIsOnWithoutNotify(false);
                if (m_mixedValuesIndicator != null)
                {
                    m_mixedValuesIndicator.text = "-";
                }
            }
            else
            {
                m_toggle.SetIsOnWithoutNotify(value >= EnabledValue);
                if (m_mixedValuesIndicator != null)
                {
                    m_mixedValuesIndicator.text = null;
                }
            }

            if(wasOn != m_toggle.isOn)
            {
                m_input.gameObject.SetActive(m_toggle.isOn);
            }
        }

        private void OnValueChanged(bool value)
        {
            m_input.gameObject.SetActive(value);
            BeginEdit();
            if (value)
            {
                SetValue(EnabledValue);
            }
            else
            {
                SetValue(DisabledValue);
            }
            EndEdit();

            SetInputField(GetValue());
        }

        protected override void OnValueChanged(string value)
        {
            base.OnValueChanged(value);
            float val;
            if (float.TryParse(value, NumberStyles.Any, FormatProvider, out val))
            {
                bool wasOn = m_toggle.isOn;

                m_toggle.SetIsOnWithoutNotify(val >= EnabledValue);
                if (m_mixedValuesIndicator != null)
                {
                    m_mixedValuesIndicator.text = null;
                }

                if (wasOn != m_toggle.isOn)
                {
                    m_input.gameObject.SetActive(m_toggle.isOn);
                }
            }
        }
    }
}
