using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class ButtonComponentDescriptor : SelectableComponentDescriptor<Button>
    {
        protected override void AfterBaseClassProperties(ComponentEditor editor, object converter, List<PropertyDescriptor> properties)
        {
            base.AfterBaseClassProperties(editor, converter, properties);

            ILocalization lc = IOC.Resolve<ILocalization>();

            PropertyInfo onClickPropertyInfo = Strong.PropertyInfo((Button x) => x.onClick);
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_OnClick", "On Click()"), editor.Components, onClickPropertyInfo));
        }
    }
}

