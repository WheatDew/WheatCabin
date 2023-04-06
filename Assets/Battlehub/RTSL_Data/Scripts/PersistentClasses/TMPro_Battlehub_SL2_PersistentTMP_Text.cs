using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using TMPro;
using TMPro.Battlehub.SL2;
using UnityEngine.UI.Battlehub.SL2;
using System;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace TMPro.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentTMP_Text<TID> : PersistentMaskableGraphic<TID>
    {
        [ProtoMember(315)]
        public string text;

        [ProtoMember(317)]
        public bool isRightToLeftText;

        [ProtoMember(318)]
        public TID font;

        [ProtoMember(324)]
        public float alpha;

        [ProtoMember(325)]
        public bool enableVertexGradient;

        [ProtoMember(326)]
        public PersistentVertexGradient<TID> colorGradient;

        [ProtoMember(327)]
        public TID colorGradientPreset;

        [ProtoMember(328)]
        public TID spriteAsset;

        [ProtoMember(330)]
        public TID styleSheet;

        [ProtoMember(336)]
        public float fontSize;

        [ProtoMember(337)]
        public FontWeight fontWeight;

        [ProtoMember(338)]
        public bool enableAutoSizing;

        [ProtoMember(339)]
        public float fontSizeMin;

        [ProtoMember(340)]
        public float fontSizeMax;

        [ProtoMember(341)]
        public FontStyles fontStyle;

        [ProtoMember(342)]
        public HorizontalAlignmentOptions horizontalAlignment;

        [ProtoMember(343)]
        public VerticalAlignmentOptions verticalAlignment;

        [ProtoMember(344)]
        public TextAlignmentOptions alignment;

        [ProtoMember(345)]
        public float characterSpacing;

        [ProtoMember(346)]
        public float wordSpacing;

        [ProtoMember(347)]
        public float lineSpacing;

        [ProtoMember(349)]
        public float paragraphSpacing;

        [ProtoMember(351)]
        public bool enableWordWrapping;

        [ProtoMember(353)]
        public TextOverflowModes overflowMode;

        [ProtoMember(355)]
        public bool enableKerning;

        [ProtoMember(356)]
        public bool extraPadding;

        [ProtoMember(357)]
        public bool richText;

        [ProtoMember(358)]
        public bool parseCtrlCharacters;

        [ProtoMember(363)]
        public TextureMappingOptions horizontalMapping;

        [ProtoMember(364)]
        public TextureMappingOptions verticalMapping;

        [ProtoMember(366)]
        public TextRenderFlags renderMode;

        [ProtoMember(367)]
        public VertexSortingOrder geometrySortingOrder;

        [ProtoMember(368)]
        public bool isTextObjectScaleStatic;

        [ProtoMember(376)]
        public PersistentVector4<TID> margin;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TMP_Text uo = (TMP_Text)obj;
            text = uo.text;
            isRightToLeftText = uo.isRightToLeftText;
            font = ToID(uo.font);
            alpha = uo.alpha;
            enableVertexGradient = uo.enableVertexGradient;
            colorGradient = uo.colorGradient;
            colorGradientPreset = ToID(uo.colorGradientPreset);
            spriteAsset = ToID(uo.spriteAsset);
            styleSheet = ToID(uo.styleSheet);
            fontSize = uo.fontSize;
            fontWeight = uo.fontWeight;
            enableAutoSizing = uo.enableAutoSizing;
            fontSizeMin = uo.fontSizeMin;
            fontSizeMax = uo.fontSizeMax;
            fontStyle = uo.fontStyle;
            horizontalAlignment = uo.horizontalAlignment;
            verticalAlignment = uo.verticalAlignment;
            alignment = uo.alignment;
            characterSpacing = uo.characterSpacing;
            wordSpacing = uo.wordSpacing;
            lineSpacing = uo.lineSpacing;
            paragraphSpacing = uo.paragraphSpacing;
            enableWordWrapping = uo.enableWordWrapping;
            overflowMode = uo.overflowMode;
            enableKerning = uo.enableKerning;
            extraPadding = uo.extraPadding;
            richText = uo.richText;
            parseCtrlCharacters = uo.parseCtrlCharacters;
            horizontalMapping = uo.horizontalMapping;
            verticalMapping = uo.verticalMapping;
            renderMode = uo.renderMode;
            geometrySortingOrder = uo.geometrySortingOrder;
            isTextObjectScaleStatic = uo.isTextObjectScaleStatic;
            margin = uo.margin;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TMP_Text uo = (TMP_Text)obj;
            uo.text = text;
            uo.isRightToLeftText = isRightToLeftText;
            uo.font = FromID(font, uo.font);
            uo.alpha = alpha;
            uo.enableVertexGradient = enableVertexGradient;
            uo.colorGradient = colorGradient;
            uo.colorGradientPreset = FromID(colorGradientPreset, uo.colorGradientPreset);
            uo.spriteAsset = FromID(spriteAsset, uo.spriteAsset);
            uo.styleSheet = FromID(styleSheet, uo.styleSheet);
            uo.fontSize = fontSize;
            uo.fontWeight = fontWeight;
            uo.enableAutoSizing = enableAutoSizing;
            uo.fontSizeMin = fontSizeMin;
            uo.fontSizeMax = fontSizeMax;
            uo.fontStyle = fontStyle;
            uo.horizontalAlignment = horizontalAlignment;
            uo.verticalAlignment = verticalAlignment;
            uo.alignment = alignment;
            uo.characterSpacing = characterSpacing;
            uo.wordSpacing = wordSpacing;
            uo.lineSpacing = lineSpacing;
            uo.paragraphSpacing = paragraphSpacing;
            uo.enableWordWrapping = enableWordWrapping;
            uo.overflowMode = overflowMode;
            uo.enableKerning = enableKerning;
            uo.extraPadding = extraPadding;
            uo.richText = richText;
            uo.parseCtrlCharacters = parseCtrlCharacters;
            uo.horizontalMapping = horizontalMapping;
            uo.verticalMapping = verticalMapping;
            uo.renderMode = renderMode;
            uo.geometrySortingOrder = geometrySortingOrder;
            uo.isTextObjectScaleStatic = isTextObjectScaleStatic;
            uo.margin = margin;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(font, context);
            AddDep(colorGradientPreset, context);
            AddDep(spriteAsset, context);
            AddDep(styleSheet, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            TMP_Text uo = (TMP_Text)obj;
            AddDep(uo.font, context);
            AddDep(uo.colorGradientPreset, context);
            AddDep(uo.spriteAsset, context);
            AddDep(uo.styleSheet, context);
        }
    }
}

