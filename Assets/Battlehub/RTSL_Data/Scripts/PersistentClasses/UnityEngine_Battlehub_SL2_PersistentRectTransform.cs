using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentRectTransform<TID> : PersistentTransform<TID>
    {
        [ProtoMember(256)]
        public PersistentVector2<TID> anchorMin;

        [ProtoMember(257)]
        public PersistentVector2<TID> anchorMax;

        [ProtoMember(258)]
        public PersistentVector2<TID> anchoredPosition;

        [ProtoMember(259)]
        public PersistentVector2<TID> sizeDelta;

        [ProtoMember(260)]
        public PersistentVector2<TID> pivot;

        [ProtoMember(261)]
        public PersistentVector3<TID> anchoredPosition3D;

        [ProtoMember(262)]
        public PersistentVector2<TID> offsetMin;

        [ProtoMember(263)]
        public PersistentVector2<TID> offsetMax;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            RectTransform uo = (RectTransform)obj;
            anchorMin = uo.anchorMin;
            anchorMax = uo.anchorMax;
            anchoredPosition = uo.anchoredPosition;
            sizeDelta = uo.sizeDelta;
            pivot = uo.pivot;
            anchoredPosition3D = uo.anchoredPosition3D;
            offsetMin = uo.offsetMin;
            offsetMax = uo.offsetMax;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            RectTransform uo = (RectTransform)obj;
            uo.anchorMin = anchorMin;
            uo.anchorMax = anchorMax;
            uo.anchoredPosition = anchoredPosition;
            uo.sizeDelta = sizeDelta;
            uo.pivot = pivot;
            uo.anchoredPosition3D = anchoredPosition3D;
            uo.offsetMin = offsetMin;
            uo.offsetMax = offsetMax;
            return uo;
        }
    }
}

