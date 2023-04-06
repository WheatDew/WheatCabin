using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentHorizontalOrVerticalLayoutGroup<TID> : PersistentLayoutGroup<TID>
    {
        [ProtoMember(264)]
        public float spacing;

        [ProtoMember(265)]
        public bool childForceExpandWidth;

        [ProtoMember(266)]
        public bool childForceExpandHeight;

        [ProtoMember(267)]
        public bool childControlWidth;

        [ProtoMember(268)]
        public bool childControlHeight;

        [ProtoMember(269)]
        public bool childScaleWidth;

        [ProtoMember(270)]
        public bool childScaleHeight;

        [ProtoMember(271)]
        public bool reverseArrangement;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            HorizontalOrVerticalLayoutGroup uo = (HorizontalOrVerticalLayoutGroup)obj;
            spacing = uo.spacing;
            childForceExpandWidth = uo.childForceExpandWidth;
            childForceExpandHeight = uo.childForceExpandHeight;
            childControlWidth = uo.childControlWidth;
            childControlHeight = uo.childControlHeight;
            childScaleWidth = uo.childScaleWidth;
            childScaleHeight = uo.childScaleHeight;
            reverseArrangement = uo.reverseArrangement;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            HorizontalOrVerticalLayoutGroup uo = (HorizontalOrVerticalLayoutGroup)obj;
            uo.spacing = spacing;
            uo.childForceExpandWidth = childForceExpandWidth;
            uo.childForceExpandHeight = childForceExpandHeight;
            uo.childControlWidth = childControlWidth;
            uo.childControlHeight = childControlHeight;
            uo.childScaleWidth = childScaleWidth;
            uo.childScaleHeight = childScaleHeight;
            uo.reverseArrangement = reverseArrangement;
            return uo;
        }
    }
}

