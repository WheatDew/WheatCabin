using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class StringEditor : PropertyEditor<string>
    {
        [SerializeField]
        protected TMP_InputField m_input;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_input.onValueChanged.AddListener(OnValueChanged);
            m_input.onEndEdit.AddListener(OnEndEdit);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_input != null)
            {
                m_input.onValueChanged.RemoveListener(OnValueChanged);
                m_input.onEndEdit.RemoveListener(OnEndEdit);
            }
        }

        protected override void SetInputField(string value)
        {
            bool hasMixedValues = HasMixedValues();
            m_input.placeholder.gameObject.SetActive(hasMixedValues);
            m_input.SetTextWithoutNotify(hasMixedValues ? null : value);
        }

        protected virtual void OnValueChanged(string value)
        {
            BeginEdit();
            SetValue(value);
            SetInputField(value);
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
