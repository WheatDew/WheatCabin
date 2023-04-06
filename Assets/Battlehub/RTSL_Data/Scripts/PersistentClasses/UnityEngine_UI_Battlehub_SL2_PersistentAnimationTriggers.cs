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
    public partial class PersistentAnimationTriggers<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public string normalTrigger;

        [ProtoMember(257)]
        public string highlightedTrigger;

        [ProtoMember(258)]
        public string pressedTrigger;

        [ProtoMember(259)]
        public string disabledTrigger;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            AnimationTriggers uo = (AnimationTriggers)obj;
            normalTrigger = uo.normalTrigger;
            highlightedTrigger = uo.highlightedTrigger;
            pressedTrigger = uo.pressedTrigger;
            disabledTrigger = uo.disabledTrigger;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            AnimationTriggers uo = (AnimationTriggers)obj;
            uo.normalTrigger = normalTrigger;
            uo.highlightedTrigger = highlightedTrigger;
            uo.pressedTrigger = pressedTrigger;
            uo.disabledTrigger = disabledTrigger;
            return uo;
        }

        public static implicit operator AnimationTriggers(PersistentAnimationTriggers<TID> surrogate)
        {
            if(surrogate == null) return default(AnimationTriggers);
            return (AnimationTriggers)surrogate.WriteTo(new AnimationTriggers());
        }
        
        public static implicit operator PersistentAnimationTriggers<TID>(AnimationTriggers obj)
        {
            PersistentAnimationTriggers<TID> surrogate = new PersistentAnimationTriggers<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

