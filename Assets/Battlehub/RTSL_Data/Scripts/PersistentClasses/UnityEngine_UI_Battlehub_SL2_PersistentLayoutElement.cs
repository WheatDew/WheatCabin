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
    public partial class PersistentLayoutElement<TID> : PersistentUIBehaviour<TID>
    {
        [ProtoMember(264)]
        public bool ignoreLayout;

        [ProtoMember(265)]
        public float minWidth;

        [ProtoMember(266)]
        public float minHeight;

        [ProtoMember(267)]
        public float preferredWidth;

        [ProtoMember(268)]
        public float preferredHeight;

        [ProtoMember(269)]
        public float flexibleWidth;

        [ProtoMember(270)]
        public float flexibleHeight;

        [ProtoMember(271)]
        public int layoutPriority;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            LayoutElement uo = (LayoutElement)obj;
            ignoreLayout = uo.ignoreLayout;
            minWidth = uo.minWidth;
            minHeight = uo.minHeight;
            preferredWidth = uo.preferredWidth;
            preferredHeight = uo.preferredHeight;
            flexibleWidth = uo.flexibleWidth;
            flexibleHeight = uo.flexibleHeight;
            layoutPriority = uo.layoutPriority;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            LayoutElement uo = (LayoutElement)obj;
            uo.ignoreLayout = ignoreLayout;
            uo.minWidth = minWidth;
            uo.minHeight = minHeight;
            uo.preferredWidth = preferredWidth;
            uo.preferredHeight = preferredHeight;
            uo.flexibleWidth = flexibleWidth;
            uo.flexibleHeight = flexibleHeight;
            uo.layoutPriority = layoutPriority;
            return uo;
        }
    }
}

