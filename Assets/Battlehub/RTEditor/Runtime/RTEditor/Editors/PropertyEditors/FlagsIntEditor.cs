using UnityEngine;

using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Battlehub.UIControls;

namespace Battlehub.RTEditor
{
    public class FlagsIntEditor : FlagsEditor<int>
    {
        protected override void ResetCurrentValue()
        {
            CurrentValue = -1;
        }

        protected override int ToValue()
        {
            return ToValueImpl();
        }

        protected override int[] FromValue(int value)
        {
            return FromValueImpl(value);
        }
    }

    public abstract class FlagsEditor<T> : PropertyEditor<T>
    {
        [SerializeField]
        protected MultiselectDropdown m_input = null;

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
                if (m_options != null)
                {
                    for (int i = 0; i < m_options.Length; ++i)
                    {
                        RangeOptions.Option option = m_options[i];
                        if (option.Value == null)
                        {
                            if (!m_valueToIndex.ContainsKey(i))
                            {
                                m_valueToIndex.Add(i, i);
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


        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_input.onSelected.AddListener(OnSelected);
            m_input.onUnselected.AddListener(OnUnselected);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_input != null)
            {
                m_input.onSelected.RemoveListener(OnSelected);
                m_input.onUnselected.RemoveListener(OnUnselected);
            }
        }

        protected override void SetInputField(T value)
        {
            if (HasMixedValues())
            {
                m_mixedValuesIndicator.text = "-";
            }
            else
            {
                m_mixedValuesIndicator.text = m_input.displayText;
                m_input.SelectWithoutNotify(FromValue(value));
            }
        }

        public Type GetEnumType(object target)
        {
            CustomTypeFieldAccessor fieldAccessor = target as CustomTypeFieldAccessor;
            if (fieldAccessor != null)
            {
                return fieldAccessor.Type;
            }
            else
            {
                return MemberInfoType;
            }
        }

        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback, string label = null)
        {
            List<MultiselectDropdown.OptionData> options = new List<MultiselectDropdown.OptionData>();

            for (int i = 0; i < Options.Length; ++i)
            {
                options.Add(new MultiselectDropdown.OptionData { Text = Options[i].Text });
            }

            m_input.options = options;

            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);

            //m_currentValue = -1;
            ResetCurrentValue();
        }

        protected abstract void ResetCurrentValue();
        protected abstract T ToValue();
        protected abstract int[] FromValue(T value);
           
        protected int ToValueImpl()
        {
            int value = 0;
            foreach (int index in m_input.selectedIndexes)
            {
                value |= 1 << (int)m_options[index].Value;
            }
            return value;
        }

        protected int[] FromValueImpl(int value)
        {
            List<int> selectedIndexes = new List<int>();
            for (int i = 0; i < 32; i++)
            {
                int index;
                if (m_valueToIndex.TryGetValue(i, out index))
                {
                    if ((value & (1 << i)) != 0)
                    {
                        selectedIndexes.Add(index);
                    }
                }
            }
            return selectedIndexes.ToArray();
        }


        protected virtual void OnSelected(int index)
        {
            T value = ToValue();
            BeginEdit();
            SetValue(value);
            SetInputField(value);
            EndEdit();
        }

        protected virtual void OnUnselected(int index)
        {
            T value = ToValue();
            BeginEdit();
            SetValue(value);
            SetInputField(value);
            EndEdit();
        }
    }
}
