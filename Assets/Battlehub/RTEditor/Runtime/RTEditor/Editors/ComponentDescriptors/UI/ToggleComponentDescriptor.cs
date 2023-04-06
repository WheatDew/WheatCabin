using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class TogglePropertyConverter
    {
        public Toggle Toggle
        {
            get;
            set;
        }

        public bool IsOn
        {
            get { return Toggle.isOn; }
            set { Toggle.SetIsOnWithoutNotify(value); }
        }
    }

    [BuiltInDescriptor]
    public class ToggleComponentDescriptor : SelectableComponentDescriptor<Toggle>
    {
        public override object CreateConverter(ComponentEditor editor)
        {
            object[] converters = new object[editor.Components.Length];
            Component[] components = editor.Components;
            for (int i = 0; i < components.Length; ++i)
            {
                Toggle toggle = (Toggle)components[i];

                converters[i] = new TogglePropertyConverter
                {
                    Toggle = toggle
                };
            }
            return converters;
        }

        protected override void AfterBaseClassProperties(ComponentEditor editor, object converter, List<PropertyDescriptor> properties)
        {
            base.AfterBaseClassProperties(editor, converter, properties);

            ILocalization lc = IOC.Resolve<ILocalization>();

            object[] converters = (object[])converter;

            MemberInfo isOnInfoConverted = Strong.MemberInfo((TogglePropertyConverter x) => x.IsOn);
            MemberInfo isOnInfo = Strong.MemberInfo((Toggle x) => x.isOn);
            MemberInfo toggleTransitionInfo = Strong.MemberInfo((Toggle x) => x.toggleTransition);
            MemberInfo graphicInfo = Strong.MemberInfo((Toggle x) => x.graphic);
            MemberInfo groupInfo = Strong.MemberInfo((Toggle x) => x.group);
            MemberInfo onValueChangedInfo = Strong.MemberInfo((Toggle x) => x.onValueChanged);

            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Toggle_IsOn", "Is On"), converters, isOnInfoConverted, isOnInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Toggle_ToggleTransition", "Toggle Transition"), editor.Components, toggleTransitionInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Toggle_Graphic", "Graphic"), editor.Components, graphicInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Toggle_Group", "Group"), editor.Components, groupInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Toggle_OnValueChanged", "On Value Changed (Boolean)"), editor.Components, onValueChangedInfo));
        }
    }
}


