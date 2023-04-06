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
    public partial class PersistentKeyframe<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(258)]
        public float inTangent;

        [ProtoMember(259)]
        public float outTangent;

        [ProtoMember(260)]
        public float inWeight;

        [ProtoMember(261)]
        public float outWeight;

        [ProtoMember(262)]
        public WeightedMode weightedMode;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Keyframe uo = (Keyframe)obj;
            inTangent = uo.inTangent;
            outTangent = uo.outTangent;
            inWeight = uo.inWeight;
            outWeight = uo.outWeight;
            weightedMode = uo.weightedMode;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Keyframe uo = (Keyframe)obj;
            uo.inTangent = inTangent;
            uo.outTangent = outTangent;
            uo.inWeight = inWeight;
            uo.outWeight = outWeight;
            uo.weightedMode = weightedMode;
            return uo;
        }

        public static implicit operator Keyframe(PersistentKeyframe<TID> surrogate)
        {
            if(surrogate == null) return default(Keyframe);
            return (Keyframe)surrogate.WriteTo(new Keyframe());
        }
        
        public static implicit operator PersistentKeyframe<TID>(Keyframe obj)
        {
            PersistentKeyframe<TID> surrogate = new PersistentKeyframe<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

