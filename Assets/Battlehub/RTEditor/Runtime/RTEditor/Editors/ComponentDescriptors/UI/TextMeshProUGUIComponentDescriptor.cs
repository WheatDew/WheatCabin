using Battlehub.RTCommon;
using Battlehub.Utils;
using System.Reflection;
using TMPro;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class TextMeshProUGUIComponentDescriptor : ComponentDescriptorBase<TextMeshProUGUI>
    {
        private HeaderText HeaderText = new HeaderText();

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo headerInfo = Strong.MemberInfo((TextMeshProUGUIComponentDescriptor x) => x.HeaderText);

            MemberInfo textInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.text);
            MemberInfo fontInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.font);
            MemberInfo fontStyleInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.fontStyle);
            MemberInfo fontSizeInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.fontSize);
            MemberInfo colorInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.color);
            MemberInfo characterSpacingInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.characterSpacing);
            MemberInfo wordSpacingInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.wordSpacing);
            MemberInfo lineSpacingInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.lineSpacing);
            MemberInfo paragraphSpacingInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.paragraphSpacing);
            #if UNITY_2020_1_OR_NEWER
            MemberInfo horizontalAlignmentInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.horizontalAlignment);
            MemberInfo verticalAlignmentInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.verticalAlignment);
            #else
            MemberInfo alignmentInfo = Strong.MemberInfo((TextMeshPro x) => x.alignment);
            #endif
            MemberInfo enableWordWrapping = Strong.MemberInfo((TextMeshProUGUI x) => x.enableWordWrapping);
            MemberInfo overflowModeInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.overflowMode);
            MemberInfo horizontalMappingInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.horizontalMapping);
            MemberInfo verticalMappingInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.verticalMapping);

            MemberInfo marginsInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.margin);
            #if UNITY_2020_1_OR_NEWER
            MemberInfo isScaleStaticInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.isTextObjectScaleStatic);
            #endif
            MemberInfo richTextInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.richText);
            MemberInfo extraPaddingInfo = Strong.MemberInfo((TextMeshProUGUI x) => x.extraPadding);

            ILocalization lc = IOC.Resolve<ILocalization>();

            return new[]
            {
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_Text", "Text"), editor.Components, textInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_MainSettings", "Main Settings"), null, headerInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_Font", "Font"), editor.Components, fontInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_FontStyle", "Font Style"), editor.Components, fontStyleInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_FontSize", "Font Size"), editor.Components, fontSizeInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_Color", "Color"), editor.Components, colorInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_CharSpacing", "Character Spacing"), editor.Components, characterSpacingInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_WordSpacing", "Word Spacing"), editor.Components, wordSpacingInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_LineSpacing", "Line Spacing"), editor.Components, lineSpacingInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_ParagraphSpacing", "Paragraph Spacing"), editor.Components, paragraphSpacingInfo),
                #if UNITY_2020_1_OR_NEWER
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_HorizontalAlignment", "Horizontal Alignment"), editor.Components, horizontalAlignmentInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_VerticalAlignment", "Vertical Alignment"), editor.Components, verticalAlignmentInfo),
                #else
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_Alignment", "Alignment"), editor.Components, alignmentInfo),
                #endif
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_EnableWordWrapping", "Wrapping"), editor.Components, enableWordWrapping),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_Overflow", "Overflow"), editor.Components, overflowModeInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_HorizontalMapping", "Horizontal Mapping"), editor.Components, horizontalMappingInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_VerticalMapping", "Vertical Mapping"), editor.Components, verticalMappingInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_ExtraSettings", "Extra Settings"), null, headerInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_Margins", "Margin"), editor.Components, marginsInfo),
                #if UNITY_2020_1_OR_NEWER
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_IsScaleStatic", "Is Scale Static"), editor.Components, isScaleStaticInfo),
                #endif
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_RichText", "Rich Text"), editor.Components, richTextInfo),
                new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_TextTMP_ExtraPadding", "Extra Padding"), editor.Components, extraPaddingInfo)
            };
        }
    }
}
