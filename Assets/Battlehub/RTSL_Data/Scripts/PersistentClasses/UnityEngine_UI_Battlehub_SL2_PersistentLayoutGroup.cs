using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.UI;
using UnityEngine.UI.Battlehub.SL2;
using UnityEngine.EventSystems.Battlehub.SL2;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.UI.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentLayoutGroup<TID> : PersistentUIBehaviour<TID>
    {
        [ProtoMember(258)]
        public PersistentRectOffset<TID> padding;

        [ProtoMember(259)]
        public TextAnchor childAlignment;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            LayoutGroup uo = (LayoutGroup)obj;
            padding = uo.padding;
            childAlignment = uo.childAlignment;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            LayoutGroup uo = (LayoutGroup)obj;
            uo.padding = padding;
            uo.childAlignment = childAlignment;
            return uo;
        }
    }
}

