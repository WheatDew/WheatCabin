using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine.EventSystems.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentMask<TID> : PersistentUIBehaviour<TID>
    {
        [ProtoMember(257)]
        public bool showMaskGraphic;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Mask uo = (Mask)obj;
            showMaskGraphic = uo.showMaskGraphic;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Mask uo = (Mask)obj;
            uo.showMaskGraphic = showMaskGraphic;
            return uo;
        }
    }
}

