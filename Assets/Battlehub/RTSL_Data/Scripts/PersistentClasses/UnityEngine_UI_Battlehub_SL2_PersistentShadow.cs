using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentShadow<TID> : PersistentBaseMeshEffect<TID>
    {
        [ProtoMember(262)]
        public PersistentColor<TID> effectColor;

        [ProtoMember(263)]
        public PersistentVector2<TID> effectDistance;

        [ProtoMember(264)]
        public bool useGraphicAlpha;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Shadow uo = (Shadow)obj;
            effectColor = uo.effectColor;
            effectDistance = uo.effectDistance;
            useGraphicAlpha = uo.useGraphicAlpha;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Shadow uo = (Shadow)obj;
            uo.effectColor = effectColor;
            uo.effectDistance = effectDistance;
            uo.useGraphicAlpha = useGraphicAlpha;
            return uo;
        }
    }
}

