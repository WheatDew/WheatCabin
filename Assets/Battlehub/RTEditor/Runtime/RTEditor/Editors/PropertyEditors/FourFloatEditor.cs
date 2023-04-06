using UnityEngine;
using TMPro;
using System.Globalization;
using UnityEngine.UI;
using System.Reflection;
using System;

namespace Battlehub.RTEditor
{
    public abstract class FourFloatEditor<T> : PropertyEditor<T>
    {
        [SerializeField]
        protected TMP_InputField m_xInput = null;
        [SerializeField]
        protected TMP_InputField m_yInput = null;
        [SerializeField]
        protected TMP_InputField m_zInput = null;
        [SerializeField]
        protected TMP_InputField m_wInput = null;
        [SerializeField]
        private RectTransform m_expander = null;
        [SerializeField]
        private Toggle m_expanderToggle = null;
        [SerializeField]
        private RectTransform m_xLabel = null;
        [SerializeField]
        private RectTransform m_yLabel = null;
        [SerializeField]
        private RectTransform m_zLabel = null;
        [SerializeField]
        private RectTransform m_wLabel = null;
        [SerializeField]
        protected DragField[] m_dragFields = null;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();

            m_xInput?.onValueChanged.AddListener(OnXValueChanged);
            m_yInput?.onValueChanged.AddListener(OnYValueChanged);
            m_zInput?.onValueChanged.AddListener(OnZValueChanged);
            m_wInput?.onValueChanged.AddListener(OnWValueChanged);

            if(m_xLabel != null)
            {
                m_xLabel.offsetMin = new Vector2(Indent, m_xLabel.offsetMin.y);
            }
            if(m_yLabel != null)
            {
                m_yLabel.offsetMin = new Vector2(Indent, m_yLabel.offsetMin.y);
            }
            if(m_zLabel != null)
            {
                m_zLabel.offsetMin = new Vector2(Indent, m_zLabel.offsetMin.y);
            }
            if(m_wLabel != null)
            {
                m_wLabel.offsetMin = new Vector2(Indent, m_wLabel.offsetMin.y);
            }
            
            m_xInput?.onEndEdit.AddListener(OnEndEdit);
            m_yInput?.onEndEdit.AddListener(OnEndEdit);
            m_zInput?.onEndEdit.AddListener(OnEndEdit);
            m_wInput?.onEndEdit.AddListener(OnEndEdit);

            for (int i = 0; i < m_dragFields.Length; ++i)
            {
                if (m_dragFields[i])
                {
                    m_dragFields[i].EndDrag.AddListener(OnEndDrag);
                }
            }
        }

        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback = null, string label = null)
        {
            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);
            if(m_expanderToggle != null)
            {
                m_expanderToggle.isOn = PlayerPrefs.GetInt($"{GetType().FullName}.{MemberInfo.DeclaringType.FullName}.{MemberInfo.Name}", 0) == 1;
                m_expanderToggle?.onValueChanged.AddListener(OnToggle);
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

            if (m_zInput != null)
            {
                m_zInput.onValueChanged.RemoveListener(OnZValueChanged);
                m_zInput.onEndEdit.RemoveListener(OnEndEdit);
            }

            if (m_wInput != null)
            {
                m_wInput.onValueChanged.RemoveListener(OnWValueChanged);
                m_wInput.onEndEdit.RemoveListener(OnEndEdit);
            }

            for (int i = 0; i < m_dragFields.Length; ++i)
            {
                if (m_dragFields[i])
                {
                    m_dragFields[i].EndDrag.RemoveListener(OnEndDrag);
                }
            }

            m_expanderToggle?.onValueChanged.RemoveListener(OnToggle);
        }

        protected override void SetIndent(float indent)
        {
            m_expander.offsetMin = new Vector2(indent, m_expander.offsetMin.y);
            if(m_xLabel != null)
            {
                m_xLabel.offsetMin = new Vector2(indent + Indent, m_xLabel.offsetMin.y);
            }
            if(m_yLabel != null)
            {
                m_yLabel.offsetMin = new Vector2(indent + Indent, m_yLabel.offsetMin.y);
            }
            if(m_zLabel != null)
            {
                m_zLabel.offsetMin = new Vector2(indent + Indent, m_zLabel.offsetMin.y);
            }
            
            if(m_wLabel != null)
            {
                m_wLabel.offsetMin = new Vector2(indent + Indent, m_wLabel.offsetMin.y);
            }
        }

        protected override void SetInputField(T v)
        {
            if(HasMixedValues())
            {
                if(m_xInput != null)
                {
                    m_xInput.text = HasMixedValues((target, accessor) => GetX(GetValue(target, accessor)), (v1, v2) => v1.Equals(v2)) ? null : GetX(v).ToString(FormatProvider);
                }
                
                if(m_yLabel != null)
                {
                    m_yInput.text = HasMixedValues((target, accessor) => GetY(GetValue(target, accessor)), (v1, v2) => v1.Equals(v2)) ? null : GetY(v).ToString(FormatProvider);
                }

                if(m_zInput != null)
                {
                    m_zInput.text = HasMixedValues((target, accessor) => GetZ(GetValue(target, accessor)), (v1, v2) => v1.Equals(v2)) ? null : GetZ(v).ToString(FormatProvider);
                }
                
                if(m_wInput != null)
                {
                    m_wInput.text = HasMixedValues((target, accessor) => GetW(GetValue(target, accessor)), (v1, v2) => v1.Equals(v2)) ? null : GetW(v).ToString(FormatProvider);
                }
            }
            else
            {
                if(m_xInput != null)
                {
                    m_xInput.text = GetX(v).ToString(FormatProvider);
                }
                
                if(m_yLabel != null)
                {
                    m_yInput.text = GetY(v).ToString(FormatProvider);
                }
                
                if(m_zInput != null)
                {
                    m_zInput.text = GetZ(v).ToString(FormatProvider);
                }
                
                if(m_wInput != null)
                {
                    m_wInput.text = GetW(v).ToString(FormatProvider);
                }
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
                    T v = SetX(GetValue(targets[i], accessors[i]), val);
                    SetValue(v, i);
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
                    T v = SetY(GetValue(targets[i], accessors[i]), val);
                    SetValue(v, i);
                }   
            }
        }

        private void OnZValueChanged(string value)
        {
            float val;
            if (float.TryParse(value, NumberStyles.Any, FormatProvider, out val) && Target != null)
            {
                BeginEdit();

                object[] targets = Targets;
                object[] accessors = Accessors;
                for (int i = 0; i < targets.Length; ++i)
                {
                    T v = SetZ(GetValue(targets[i], accessors[i]), val);
                    SetValue(v, i);
                }
            }
        }

        private void OnWValueChanged(string value)
        {
            float val;
            if (float.TryParse(value, NumberStyles.Any, FormatProvider, out val) && Target != null)
            {
                BeginEdit();

                object[] targets = Targets;
                object[] accessors = Accessors;
                for (int i = 0; i < targets.Length; ++i)
                {
                    T v = SetW(GetValue(targets[i], accessors[i]), val);
                    SetValue(v, i);
                }
            }
        }

        protected virtual void OnEndEdit(string value)
        {
            T v = GetValue();
            SetInputField(v);
            EndEdit();
        }

        protected void OnEndDrag()
        {
            EndEdit();
        }

        private void OnToggle(bool value)
        {
            PlayerPrefs.SetInt($"{GetType().FullName}.{MemberInfo.DeclaringType.FullName}.{MemberInfo.Name}", value ? 1 : 0);
        }

        protected abstract T SetX(T v, float x);
        protected abstract T SetY(T v, float y);
        protected abstract T SetZ(T v, float z);
        protected abstract T SetW(T v, float w);
        protected abstract float GetX(T v);
        protected abstract float GetY(T v);
        protected abstract float GetZ(T v);
        protected abstract float GetW(T v);


    }
}

