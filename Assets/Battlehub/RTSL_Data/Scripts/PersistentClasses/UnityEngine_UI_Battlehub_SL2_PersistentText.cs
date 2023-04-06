using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine;
using System;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentText<TID> : PersistentMaskableGraphic<TID>
    {
        [ProtoMember(271)]
        public TID font;

        [ProtoMember(272)]
        public string text;

        [ProtoMember(273)]
        public bool supportRichText;

        [ProtoMember(274)]
        public bool resizeTextForBestFit;

        [ProtoMember(275)]
        public int resizeTextMinSize;

        [ProtoMember(276)]
        public int resizeTextMaxSize;

        [ProtoMember(277)]
        public TextAnchor alignment;

        [ProtoMember(278)]
        public bool alignByGeometry;

        [ProtoMember(279)]
        public int fontSize;

        [ProtoMember(280)]
        public HorizontalWrapMode horizontalOverflow;

        [ProtoMember(281)]
        public VerticalWrapMode verticalOverflow;

        [ProtoMember(282)]
        public float lineSpacing;

        [ProtoMember(283)]
        public FontStyle fontStyle;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Text uo = (Text)obj;
            font = ToID(uo.font);
            text = uo.text;
            supportRichText = uo.supportRichText;
            resizeTextForBestFit = uo.resizeTextForBestFit;
            resizeTextMinSize = uo.resizeTextMinSize;
            resizeTextMaxSize = uo.resizeTextMaxSize;
            alignment = uo.alignment;
            alignByGeometry = uo.alignByGeometry;
            fontSize = uo.fontSize;
            horizontalOverflow = uo.horizontalOverflow;
            verticalOverflow = uo.verticalOverflow;
            lineSpacing = uo.lineSpacing;
            fontStyle = uo.fontStyle;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Text uo = (Text)obj;
            uo.font = FromID(font, uo.font);
            uo.text = text;
            uo.supportRichText = supportRichText;
            uo.resizeTextForBestFit = resizeTextForBestFit;
            uo.resizeTextMinSize = resizeTextMinSize;
            uo.resizeTextMaxSize = resizeTextMaxSize;
            uo.alignment = alignment;
            uo.alignByGeometry = alignByGeometry;
            uo.fontSize = fontSize;
            uo.horizontalOverflow = horizontalOverflow;
            uo.verticalOverflow = verticalOverflow;
            uo.lineSpacing = lineSpacing;
            uo.fontStyle = fontStyle;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(font, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Text uo = (Text)obj;
            AddDep(uo.font, context);
        }
    }
}

