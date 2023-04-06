using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentImage<TID> : PersistentMaskableGraphic<TID>
    {
        [ProtoMember(276)]
        public TID sprite;

        [ProtoMember(277)]
        public TID overrideSprite;

        [ProtoMember(278)]
        public Image.Type type;

        [ProtoMember(279)]
        public bool preserveAspect;

        [ProtoMember(280)]
        public bool fillCenter;

        [ProtoMember(281)]
        public Image.FillMethod fillMethod;

        [ProtoMember(282)]
        public float fillAmount;

        [ProtoMember(283)]
        public bool fillClockwise;

        [ProtoMember(284)]
        public int fillOrigin;

        [ProtoMember(286)]
        public float alphaHitTestMinimumThreshold;

        [ProtoMember(289)]
        public bool useSpriteMesh;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Image uo = (Image)obj;
            sprite = ToID(uo.sprite);
            overrideSprite = ToID(uo.overrideSprite);
            type = uo.type;
            preserveAspect = uo.preserveAspect;
            fillCenter = uo.fillCenter;
            fillMethod = uo.fillMethod;
            fillAmount = uo.fillAmount;
            fillClockwise = uo.fillClockwise;
            fillOrigin = uo.fillOrigin;
            alphaHitTestMinimumThreshold = uo.alphaHitTestMinimumThreshold;
            useSpriteMesh = uo.useSpriteMesh;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Image uo = (Image)obj;
            uo.sprite = FromID(sprite, uo.sprite);
            uo.overrideSprite = FromID(overrideSprite, uo.overrideSprite);
            uo.type = type;
            uo.preserveAspect = preserveAspect;
            uo.fillCenter = fillCenter;
            uo.fillMethod = fillMethod;
            uo.fillAmount = fillAmount;
            uo.fillClockwise = fillClockwise;
            uo.fillOrigin = fillOrigin;
            uo.alphaHitTestMinimumThreshold = alphaHitTestMinimumThreshold;
            uo.useSpriteMesh = useSpriteMesh;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(sprite, context);
            AddDep(overrideSprite, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Image uo = (Image)obj;
            AddDep(uo.sprite, context);
            AddDep(uo.overrideSprite, context);
        }
    }
}

