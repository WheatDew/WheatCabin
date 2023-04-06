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
    public partial class PersistentToggleGroup<TID> : PersistentUIBehaviour<TID>
    {
        [ProtoMember(257)]
        public bool allowSwitchOff;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ToggleGroup uo = (ToggleGroup)obj;
            allowSwitchOff = uo.allowSwitchOff;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ToggleGroup uo = (ToggleGroup)obj;
            uo.allowSwitchOff = allowSwitchOff;
            return uo;
        }
    }
}

