using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Reflection;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class TextComponentDescriptor : ComponentDescriptorBase<Text>
    {
        private HeaderText HeaderText = new HeaderText();

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo headerInfo = Strong.MemberInfo((TextComponentDescriptor x) => x.HeaderText);

            MemberInfo textInfo = Strong.MemberInfo((Text x) => x.text);
            MemberInfo fontInfo = Strong.MemberInfo((Text x) => x.font);
            MemberInfo fontStyleInfo = Strong.MemberInfo((Text x) => x.fontStyle);
            MemberInfo fontSizeInfo = Strong.MemberInfo((Text x) => x.fontSize);
            MemberInfo lineSpacingInfo = Strong.MemberInfo((Text x) => x.lineSpacing);
            MemberInfo richTextInfo = Strong.MemberInfo((Text x) => x.supportRichText);

            MemberInfo alignmentInfo = Strong.MemberInfo((Text x) => x.alignment);
            MemberInfo alignByGeometryInfo = Strong.MemberInfo((Text x) => x.alignByGeometry);
            MemberInfo horizontalOverflowInfo = Strong.MemberInfo((Text x) => x.horizontalOverflow);
            MemberInfo verticalOverflowInfo = Strong.MemberInfo((Text x) => x.verticalOverflow);
            MemberInfo bestFitInfo = Strong.MemberInfo((Text x) => x.resizeTextForBestFit);

            MemberInfo colorInfo = Strong.MemberInfo((Text x) => x.color);
            MemberInfo materialInfo = Strong.MemberInfo((Text x) => x.material);

            ILocalization lc = IOC.Resolve<ILocalization>();

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text", "Text"), editor.Components, textInfo),
                
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_Character", "Character"), null, headerInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_Font", "Font"), editor.Components, fontInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_FontStyle", "Font Style"), editor.Components, fontStyleInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_FontSize", "Font Size"), editor.Components, fontSizeInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_LineSpacing", "Line Spacing"), editor.Components, lineSpacingInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_RichText", "Rich Text"), editor.Components, richTextInfo),

                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_Paragraph", "Paragraph"), null, headerInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_Alignment", "Alignment"), editor.Components, alignmentInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_AlignByGeometry", "Align By Geometry"), editor.Components, alignByGeometryInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_HorizontalOverflow", "Horizontal Overflow"), editor.Components, horizontalOverflowInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_VerticalOverflow", "Vertical Overflow"), editor.Components, verticalOverflowInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_BestFit", "Best Fit"), editor.Components, bestFitInfo),

                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_Rendering", "Rendering"), null, headerInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_Color", "Color"), editor.Components, colorInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Text_Material", "Material"), editor.Components, materialInfo),
            };
        }
    }
}
