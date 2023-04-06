using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class CanvasComponentDescriptor : ComponentDescriptorBase<Canvas>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();
            MemberInfo memberInfo = Strong.MemberInfo((Canvas x) => x.worldCamera);
            if(editor.NotNullComponents.OfType<Canvas>().Any(c => c.renderMode == RenderMode.ScreenSpaceCamera))
            {
                return new PropertyDescriptor[0];
            }

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Canvas_Camera", "Camera"), editor.Components, memberInfo)
            };
        }
    }
}

