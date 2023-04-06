using System.Globalization;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class Vector2Editor : ConvertablePropertyEditor<Vector2>
    {
        [SerializeField]
        private TMP_InputField m_xInput = null;
        [SerializeField]
        private TMP_InputField m_yInput = null;
        [SerializeField]
        protected DragField[] m_dragFields = null;

        public bool? IsInteractable
        {
            get
            {
                if (IsXInteractable == IsYInteractable)
                {
                    return IsXInteractable;
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    IsXInteractable = value.Value;
                    IsYInteractable = value.Value;
                }
            }
        }

        public bool IsXInteractable
        {
            get { return m_xInput.interactable; }
            set
            {
                m_xInput.interactable = value;
                m_dragFields[0].enabled = value;
            }
        }
        public bool IsYInteractable
        {
            get { return m_yInput.interactable; }
            set
            {
                m_yInput.interactable = value;
                m_dragFields[1].enabled = value; 
            }
        }
    
        protected override void AwakeOverride()
        {
            base.AwakeOverride();

            m_xInput.onValueChanged.AddListener(OnXValueChanged);
            m_yInput.onValueChanged.AddListener(OnYValueChanged);
            m_xInput.onEndEdit.AddListener(OnEndEdit);
            m_yInput.onEndEdit.AddListener(OnEndEdit);

            for (int i = 0; i < m_dragFields.Length; ++i)
            {
                if (m_dragFields[i])
                {
                    m_dragFields[i].EndDrag.AddListener(OnEndDrag);
                }
            }

        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_xInput != null)
            {
                m_xInput.onValueChanged.RemoveListener(OnXValueChanged);
                m_xInput.onEndEdit.RemoveListener(OnEndEdit);   
            }

            if (m_yInput != null)
            {
                m_yInput.onValueChanged.RemoveListener(OnYValueChanged);
                m_yInput.onEndEdit.RemoveListener(OnEndEdit);
            }

            for (int i = 0; i < m_dragFields.Length; ++i)
            {
                if (m_dragFields[i])
                {
                    m_dragFields[i].EndDrag.RemoveListener(OnEndDrag);
                }
            }
        }

        protected override void SetInputField(Vector2 value)
        {
            if (HasMixedValues())
            {
                m_xInput.text = HasMixedValues((target, accessor) => GetValue(target, accessor).x, (v1, v2) => v1.Equals(v2)) ? null : FromMeters(value.x).ToString(FormatProvider);
                m_yInput.text = HasMixedValues((target, accessor) => GetValue(target, accessor).y, (v1, v2) => v1.Equals(v2)) ? null : FromMeters(value.y).ToString(FormatProvider);
            }
            else
            {
                m_xInput.text = FromMeters(value.x).ToString(FormatProvider);
                m_yInput.text = FromMeters(value.y).ToString(FormatProvider);
            }
        }

        private void OnXValueChanged(string value)
        {
            float val;
            if (float.TryParse(value, NumberStyles.Any, FormatProvider, out val) && Target != null)
            {
                BeginEdit();

                object[] targets = Targets;
                object[] accessors = Accessors;
                for (int i = 0; i < targets.Length; ++i)
                {
                    Vector3 vector = GetValue(targets[i], accessors[i]);
                    vector.x = ToMeters(val);
                    SetValue(vector, i);
                }
            }
        }

        private void OnYValueChanged(string value)
        {
            float val;
            if (float.TryParse(value, NumberStyles.Any, FormatProvider, out val) && Target != null)
            {
                BeginEdit();

                object[] targets = Targets;
                object[] accessors = Accessors;
                for (int i = 0; i < targets.Length; ++i)
                {
                    Vector3 vector = GetValue(targets[i], accessors[i]);
                    vector.y = ToMeters(val);
                    SetValue(vector, i);
                }
            }
        }

        private void OnEndEdit(string value)
        {
            Vector2 vector = GetValue();
            SetInputField(vector);

            EndEdit();
        }

        protected void OnEndDrag()
        {
            EndEdit();
        } 
    }
}
