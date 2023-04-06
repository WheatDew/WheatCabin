using UnityEngine;

namespace Battlehub.RTEditor
{
    public class RangeInt : Range
    {
        public RangeInt(int min, int max) : base(min, max)
        {

        }
    }

    public class RangeIntEditor : IntEditor
    {
        [SerializeField]
        private SliderOverride m_slider = null;

        public int Min = 0;
        public int Max = 1;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_slider.onValueChanged.AddListener(OnSliderValueChanged);
            m_slider.onEndEdit.AddListener(OnSliderEndEdit);
            m_slider.wholeNumbers = true;
        }

        protected override void StartOverride()
        {
            base.StartOverride();
            m_slider.minValue = Min;
            m_slider.maxValue = Max;
        }

        protected override void SetInputField(int value)
        {
            base.SetInputField(value);
            m_slider.minValue = Min;
            m_slider.maxValue = Max;
            m_slider.value = value;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_slider != null)
            {
                m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
                m_slider.onEndEdit.RemoveListener(OnSliderEndEdit);
            }
        }

        private void OnSliderValueChanged(float value)
        {
            m_input.text = value.ToString(FormatProvider);
        }

        protected override void OnValueChanged(string value)
        {
            int val;
            if (int.TryParse(value, out val))
            {
                if (Min <= val && val <= Max)
                {
                    BeginEdit();
                    SetValue(val);
                    m_slider.value = val;
                }
            }
        }

        private void OnSliderEndEdit()
        {
            EndEdit();
        }
    }
}

