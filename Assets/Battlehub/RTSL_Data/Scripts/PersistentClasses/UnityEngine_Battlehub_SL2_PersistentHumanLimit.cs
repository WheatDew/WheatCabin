using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentHumanLimit<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(261)]
        public bool useDefaultValues;

        [ProtoMember(262)]
        public PersistentVector3<TID> min;

        [ProtoMember(263)]
        public PersistentVector3<TID> max;

        [ProtoMember(264)]
        public PersistentVector3<TID> center;

        [ProtoMember(265)]
        public float axisLength;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            HumanLimit uo = (HumanLimit)obj;
            useDefaultValues = uo.useDefaultValues;
            min = uo.min;
            max = uo.max;
            center = uo.center;
            axisLength = uo.axisLength;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            HumanLimit uo = (HumanLimit)obj;
            uo.useDefaultValues = useDefaultValues;
            uo.min = min;
            uo.max = max;
            uo.center = center;
            uo.axisLength = axisLength;
            return uo;
        }

        public static implicit operator HumanLimit(PersistentHumanLimit<TID> surrogate)
        {
            if(surrogate == null) return default(HumanLimit);
            return (HumanLimit)surrogate.WriteTo(new HumanLimit());
        }
        
        public static implicit operator PersistentHumanLimit<TID>(HumanLimit obj)
        {
            PersistentHumanLimit<TID> surrogate = new PersistentHumanLimit<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

