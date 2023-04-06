using Battlehub.RTCommon;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class FloatEditor : ConvertablePropertyEditor<float>
    {
        [SerializeField]
        protected TMP_InputField m_input;
        [SerializeField]
        protected DragField m_dragField;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_input.onValueChanged.AddListener(OnValueChanged);
            m_input.onEndEdit.AddListener(OnEndEdit);
            if(m_dragField != null)
            {
                m_dragField.EndDrag.AddListener(OnEndDrag);
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if(m_input != null)
            {
                m_input.onValueChanged.RemoveListener(OnValueChanged);
                m_input.onEndEdit.RemoveListener(OnEndEdit);
            }

            if(m_dragField != null)
            {
                m_dragField.EndDrag.RemoveListener(OnEndDrag);
            }
        }

        protected override void SetInputField(float value)
        {
            m_input.text = HasMixedValues() ? null : FromMeters(value).ToString(FormatProvider);
        }

        protected virtual void OnValueChanged(string value)
        {
            float val;
            if(float.TryParse(value, NumberStyles.Any, FormatProvider, out val))
            {
                BeginEdit();
                SetValue(ToMeters(val));
            }
        }

        protected virtual void OnEndEdit(string value)
        {
            SetInputField(GetValue());
            EndEdit();
        }

        protected virtual void OnEndDrag()
        {
            EndEdit();
        }
    }
}
