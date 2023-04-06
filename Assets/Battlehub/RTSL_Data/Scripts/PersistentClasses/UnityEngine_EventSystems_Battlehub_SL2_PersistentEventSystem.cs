using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine.EventSystems;
using UnityEngine.EventSystems.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.EventSystems.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentEventSystem<TID> : PersistentUIBehaviour<TID>
    {
        [ProtoMember(262)]
        public bool sendNavigationEvents;

        [ProtoMember(263)]
        public int pixelDragThreshold;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            EventSystem uo = (EventSystem)obj;
            sendNavigationEvents = uo.sendNavigationEvents;
            pixelDragThreshold = uo.pixelDragThreshold;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            EventSystem uo = (EventSystem)obj;
            uo.sendNavigationEvents = sendNavigationEvents;
            uo.pixelDragThreshold = pixelDragThreshold;
            return uo;
        }
    }
}

