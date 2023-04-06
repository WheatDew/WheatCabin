using UnityEngine;

using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using System.Linq;

namespace Battlehub.RTEditor
{
    public class RangeFlags : RangeOptions
    {
        public RangeFlags(Option[] options) : base(options)
        {
            Options = options;
        }
    }

    public class RangeOptions 
    {
        public Option[] Options;

        public class Option
        {
            public string Text;
            public object Value;
            public Option(string text, object value = null)
            {
                Text = text;
                Value = value;
            }
        }
        

        [Obsolete]
        public RangeOptions(params string[] options)
        {
            Options = options.Select(opt => new Option(opt)).ToArray();
        }

        public RangeOptions(Option[] options)
        {
            Options = options;
        }
    }

    public class OptionsEditor : OptionsEditor<int>
    {
        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback, string label = null)
        {
            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);
            CurrentValue = -1;
        }
    }

    public class OptionsEditor<T> : PropertyEditor<T>
    {
        [SerializeField]
        protected TMP_Dropdown m_input = null;

        [SerializeField]
        protected TextMeshProUGUI m_mixedValuesIndicator = null;

        private RangeOptions.Option[] m_options = new RangeOptions.Option[0];
        private Dictionary<object, int> m_valueToIndex = new Dictionary<object, int>();
        public RangeOptions.Option[] Options
        {
            get { return m_options; }
            set
            { 
                m_options = value;
                m_valueToIndex.Clear();
                if(m_options != null)
                {
                    for(int i = 0; i < m_options.Length; ++i)
                    {
                        RangeOptions.Option option = m_options[i];
                        if(option.Value == null)
                        {
                            if (!m_valueToIndex.ContainsKey(i))
                            {
                                m_valueToIndex.Add(Convert.ChangeType(i, typeof(T)), i);
                            }
                        }
                        else
                        {
                            if (!m_valueToIndex.ContainsKey(option.Value))
                            {
                                m_valueToIndex.Add(option.Value, i);
                            }
                        }
                    }
                }
            }
        }

        protected bool TryGetIndex(T value, out int index)
        {
            return m_valueToIndex.TryGetValue(value, out index);
        }

        protected T ToValue(int index)
        {
            var val = Options[index].Value;
            if (val == null)
            {
                return (T)Convert.ChangeType(index, typeof(T));
            }
            return (T)val;
        }


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

        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback, string label = null)
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            for (int i = 0; i < Options.Length; ++i)
            {
                options.Add(new TMP_Dropdown.OptionData(Options[i].Text));
            }

            m_input.options = options;

            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);
        }

        protected virtual void OnValueChanged(int index)
        {
            BeginEdit();
            SetValue(ToValue(index));
            SetInputField(ToValue(index));
            EndEdit();
        }

        protected override void SetInputField(T value)
        {
            if (HasMixedValues())
            {
                m_mixedValuesIndicator.text = "-";
            }
            else
            {
                int index;
                if (TryGetIndex(value, out index))
                {
                    m_input.value = index;
                    m_mixedValuesIndicator.text = m_input.options[index].text;
                }
                else
                {
                    m_mixedValuesIndicator.text = "";
                }
            }
        }
    }
}
