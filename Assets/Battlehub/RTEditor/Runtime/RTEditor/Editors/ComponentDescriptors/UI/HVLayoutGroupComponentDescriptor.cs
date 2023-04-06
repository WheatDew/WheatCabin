
using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class HorizontalLayoutGroupComponentDescriptor : HVLayoutGroupComponentDescriptor<HorizontalLayoutGroup>
    {
    }

    [BuiltInDescriptor]
    public class VerticalLayoutGroupComponentDescriptor : HVLayoutGroupComponentDescriptor<VerticalLayoutGroup>
    {
    }


    public abstract class HVLayoutGroupComponentDescriptor<TComponent> : LayoutGroupComponentDescriptor<TComponent>
    {
        protected override void AfterBaseClassProperties(ComponentEditor editor, object converter, List<PropertyDescriptor> properties)
        {
            base.AfterBaseClassProperties(editor, converter, properties);

            MemberInfo spacingInfo = Strong.MemberInfo((HorizontalOrVerticalLayoutGroup x) => x.spacing);
            MemberInfo childAlignmentInfo = Strong.MemberInfo((HorizontalOrVerticalLayoutGroup x) => x.childAlignment);
            #if UNITY_2020_1_OR_NEWER
            MemberInfo reverseArrangementInfo = Strong.MemberInfo((HorizontalOrVerticalLayoutGroup x) => x.reverseArrangement);
            #endif
            MemberInfo controlChildWidthInfo = Strong.MemberInfo((HorizontalOrVerticalLayoutGroup x) => x.childControlWidth);
            MemberInfo controlChildHeightInfo = Strong.MemberInfo((HorizontalOrVerticalLayoutGroup x) => x.childControlHeight);
            MemberInfo useChildScaleXInfo = Strong.MemberInfo((HorizontalOrVerticalLayoutGroup x) => x.childScaleWidth);
            MemberInfo useChildScaleYInfo = Strong.MemberInfo((HorizontalOrVerticalLayoutGroup x) => x.childScaleHeight);
            MemberInfo forceExpandChildWidthInfo = Strong.MemberInfo((HorizontalOrVerticalLayoutGroup x) => x.childForceExpandWidth);
            MemberInfo forceExpandChildHeightInfo = Strong.MemberInfo((HorizontalOrVerticalLayoutGroup x) => x.childForceExpandHeight);

            ILocalization lc = IOC.Resolve<ILocalization>();

            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HVLayoutGroup_Spacing", "Spacing"), editor.Components, spacingInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HVLayoutGroup_ChildAlignment", "Child Alignment"), editor.Components, childAlignmentInfo));
            #if UNITY_2020_1_OR_NEWER
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HVLayoutGroup_ReverseArrangement", "Reverse Arrangement"), editor.Components, reverseArrangementInfo));
            #endif
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HVLayoutGroup_ControlChildWidth", "Control Child Width"), editor.Components, controlChildWidthInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HVLayoutGroup_ControlChildHeight", "Control Child Height"), editor.Components, controlChildHeightInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HVLayoutGroup_UseChildScaleX", "Use Child Scale X"), editor.Components, useChildScaleXInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HVLayoutGroup_UseChildScaleY", "Use Child Scale Y"), editor.Components, useChildScaleYInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HVLayoutGroup_ChildForceExpandWidth", "Child Force Expand Width"), editor.Components, forceExpandChildWidthInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_HVLayoutGroup_ChildForceExpandHeight", "Child Force Expand Height"), editor.Components, forceExpandChildHeightInfo));
        }
    }
}

