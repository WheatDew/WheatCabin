using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Reflection;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class ToggleGroupComponentDescriptor : ComponentDescriptorBase<ToggleGroup>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo allowSwitchOffInfo = Strong.MemberInfo((ToggleGroup x) => x.allowSwitchOff);

            ILocalization lc = IOC.Resolve<ILocalization>();

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_ToggleGroup_AllowSwitchOff", "Allow Switch Off"), editor.Components, allowSwitchOffInfo)
            };
        }
    }
}

