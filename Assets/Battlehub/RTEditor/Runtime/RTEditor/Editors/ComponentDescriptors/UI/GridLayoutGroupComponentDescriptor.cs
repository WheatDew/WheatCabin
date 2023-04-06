using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class GridLayoutGroupComponentDescriptor : LayoutGroupComponentDescriptor<GridLayoutGroup>
    {
        protected override void AfterBaseClassProperties(ComponentEditor editor, object converter, List<PropertyDescriptor> properties)
        {
            base.AfterBaseClassProperties(editor, converter, properties);
            MemberInfo spacingInfo = Strong.MemberInfo((GridLayoutGroup x) => x.spacing);
            MemberInfo cellSizeInfo = Strong.MemberInfo((GridLayoutGroup x) => x.cellSize);
            
            MemberInfo startCornerInfo = Strong.MemberInfo((GridLayoutGroup x) => x.startCorner);
            MemberInfo startAxisInfo = Strong.MemberInfo((GridLayoutGroup x) => x.startAxis);
            MemberInfo childAlignmentInfo = Strong.MemberInfo((GridLayoutGroup x) => x.childAlignment);
            MemberInfo constraintInfo = Strong.MemberInfo((GridLayoutGroup x) => x.constraint);

            ILocalization lc = IOC.Resolve<ILocalization>();

            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_GridLayoutGroup_Spacing", "Spacing"), editor.Components, spacingInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_GridLayoutGroup_CellSize", "Cell Size"), editor.Components, cellSizeInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_GridLayoutGroup_StartCorner", "Start Corner"), editor.Components, startCornerInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_GridLayoutGroup_StartAxis", "Start Axis"), editor.Components, startAxisInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_GridLayoutGroup_ChildAlignment", "Child Alignment"), editor.Components, childAlignmentInfo));
            properties.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_GridLayoutGroup_Constraint", "Constraint"), editor.Components, constraintInfo));
        }
    }

}

