using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Battlehub.RTCommon;
using System.Linq;

namespace Battlehub.RTEditor
{
    public class EnumEditor : PropertyEditor<Enum>
    {
        [SerializeField]
        private TMP_Dropdown m_input = null;

        [SerializeField]
        private TextMeshProUGUI m_mixedValuesIndicator = null;

        [SerializeField]
        private string[] m_hiddenValues;

        private string[] GetEnumNames(Type enumType)
        {
            if (m_hiddenValues.Length == 0)
            {
                return Enum.GetNames(enumType);
            }

            return Enum.GetNames(enumType).Where(n => Array.IndexOf(m_hiddenValues, n) < 0).ToArray();
        }
        private Array GetEnumValues(Type enumType)
        {
            if(m_hiddenValues.Length == 0)
            {
                return Enum.GetValues(enumType);
            }

            return GetEnumNames(enumType).Select(n => Enum.Parse(enumType, n)).ToArray();
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

        public Type GetEnumType(object target)
        {
            CustomTypeFieldAccessor[] fieldAccessors = target as CustomTypeFieldAccessor[];
            if (fieldAccessors != null && fieldAccessors.Length > 0 && fieldAccessors[0] != null)
            {
                return fieldAccessors[0].Type;
            }
            else
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
        }

        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback = null, string label = null)
        {
            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            ILocalization localization = IOC.Resolve<ILocalization>();

            Type enumType = GetEnumType(accessor);
            string[] names = GetEnumNames(enumType);

            for (int i = 0; i < names.Length; ++i)
            {
                string name = localization.GetString("ID_" + enumType.FullName + "." + names[i], names[i]);
                options.Add(new TMP_Dropdown.OptionData(name.Replace('_', ' ')));
            }

            m_input.options = options;
        }

        protected override void SetInputField(Enum value)
        {
            if (HasMixedValues())
            {
                m_mixedValuesIndicator.text = "-";
            }
            else
            {
                Type enumType = GetEnumType(Accessor);
                int index = Array.IndexOf(GetEnumValues(enumType), value);
                if(index >= 0 && index < m_input.options.Count)
                {
                    m_input.value = index;
                    m_mixedValuesIndicator.text = m_input.options[index].text;
                }   
            }
        }

        private void OnValueChanged(int index)
        {
            Type enumType = GetEnumType(Accessor);
            Enum value = (Enum)GetEnumValues(enumType).GetValue(index);
            BeginEdit();
            SetValue(value);
            SetInputField(value);
            EndEdit();
        }
    }
}
