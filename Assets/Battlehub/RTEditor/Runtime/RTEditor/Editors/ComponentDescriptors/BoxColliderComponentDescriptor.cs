using UnityEngine;
using System.Reflection;
using System;
using Battlehub.Utils;
using Battlehub.RTGizmos;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class BoxColliderComponentDescriptor : ComponentDescriptorBase<BoxCollider, BoxColliderGizmo>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            MemberInfo isTriggerInfo = Strong.PropertyInfo((BoxCollider x) => x.isTrigger, "isTrigger");
            MemberInfo materialInfo = Strong.PropertyInfo((BoxCollider x) => x.sharedMaterial, "sharedMaterial");
            MemberInfo centerInfo = Strong.PropertyInfo((BoxCollider x) => x.center, "center");
            MemberInfo sizeInfo = Strong.PropertyInfo((BoxCollider x) => x.size, "size");

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_BoxCollider_IsTrigger", "Is Trigger"), editor.Components, isTriggerInfo, "m_IsTrigger"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_BoxCollider_Material",  "Material"), editor.Components, materialInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_BoxCollider_Center", "Center"), editor.Components, centerInfo, "m_Center"),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_BoxCollider_Size", "Size"), editor.Components, sizeInfo, "m_Size"),
            };
        }
    }
}

