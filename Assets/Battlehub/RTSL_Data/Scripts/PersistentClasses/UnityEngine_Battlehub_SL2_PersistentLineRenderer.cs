using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentLineRenderer<TID> : PersistentRenderer<TID>
    {
        [ProtoMember(257)]
        public float startWidth;

        [ProtoMember(258)]
        public float endWidth;

        [ProtoMember(259)]
        public float widthMultiplier;

        [ProtoMember(260)]
        public int numCornerVertices;

        [ProtoMember(261)]
        public int numCapVertices;

        [ProtoMember(262)]
        public bool useWorldSpace;

        [ProtoMember(263)]
        public bool loop;

        [ProtoMember(264)]
        public PersistentColor<TID> startColor;

        [ProtoMember(265)]
        public PersistentColor<TID> endColor;

        [ProtoMember(266)]
        public int positionCount;

        [ProtoMember(267)]
        public bool generateLightingData;

        [ProtoMember(268)]
        public LineTextureMode textureMode;

        [ProtoMember(269)]
        public LineAlignment alignment;

        [ProtoMember(270)]
        public PersistentAnimationCurve<TID> widthCurve;

        [ProtoMember(271)]
        public PersistentGradient<TID> colorGradient;

        [ProtoMember(272)]
        public float shadowBias;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            LineRenderer uo = (LineRenderer)obj;
            startWidth = uo.startWidth;
            endWidth = uo.endWidth;
            widthMultiplier = uo.widthMultiplier;
            numCornerVertices = uo.numCornerVertices;
            numCapVertices = uo.numCapVertices;
            useWorldSpace = uo.useWorldSpace;
            loop = uo.loop;
            startColor = uo.startColor;
            endColor = uo.endColor;
            positionCount = uo.positionCount;
            generateLightingData = uo.generateLightingData;
            textureMode = uo.textureMode;
            alignment = uo.alignment;
            widthCurve = uo.widthCurve;
            colorGradient = uo.colorGradient;
            shadowBias = uo.shadowBias;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            LineRenderer uo = (LineRenderer)obj;
            uo.startWidth = startWidth;
            uo.endWidth = endWidth;
            uo.widthMultiplier = widthMultiplier;
            uo.numCornerVertices = numCornerVertices;
            uo.numCapVertices = numCapVertices;
            uo.useWorldSpace = useWorldSpace;
            uo.loop = loop;
            uo.startColor = startColor;
            uo.endColor = endColor;
            uo.positionCount = positionCount;
            uo.generateLightingData = generateLightingData;
            uo.textureMode = textureMode;
            uo.alignment = alignment;
            uo.widthCurve = widthCurve;
            uo.colorGradient = colorGradient;
            uo.shadowBias = shadowBias;
            return uo;
        }
    }
}

