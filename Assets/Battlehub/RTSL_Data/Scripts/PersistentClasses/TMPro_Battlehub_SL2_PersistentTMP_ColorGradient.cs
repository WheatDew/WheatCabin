using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using TMPro;
using TMPro.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace TMPro.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentTMP_ColorGradient<TID> : PersistentObject<TID>
    {
        [ProtoMember(256)]
        public ColorMode colorMode;

        [ProtoMember(257)]
        public PersistentColor<TID> topLeft;

        [ProtoMember(258)]
        public PersistentColor<TID> topRight;

        [ProtoMember(259)]
        public PersistentColor<TID> bottomLeft;

        [ProtoMember(260)]
        public PersistentColor<TID> bottomRight;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TMP_ColorGradient uo = (TMP_ColorGradient)obj;
            colorMode = uo.colorMode;
            topLeft = uo.topLeft;
            topRight = uo.topRight;
            bottomLeft = uo.bottomLeft;
            bottomRight = uo.bottomRight;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TMP_ColorGradient uo = (TMP_ColorGradient)obj;
            uo.colorMode = colorMode;
            uo.topLeft = topLeft;
            uo.topRight = topRight;
            uo.bottomLeft = bottomLeft;
            uo.bottomRight = bottomRight;
            return uo;
        }
    }
}

