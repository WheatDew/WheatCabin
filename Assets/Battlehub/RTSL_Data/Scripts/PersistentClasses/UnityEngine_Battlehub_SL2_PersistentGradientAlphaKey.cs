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
    public partial class PersistentGradientAlphaKey<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public float alpha;

        [ProtoMember(257)]
        public float time;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            GradientAlphaKey uo = (GradientAlphaKey)obj;
            alpha = uo.alpha;
            time = uo.time;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            GradientAlphaKey uo = (GradientAlphaKey)obj;
            uo.alpha = alpha;
            uo.time = time;
            return uo;
        }

        public static implicit operator GradientAlphaKey(PersistentGradientAlphaKey<TID> surrogate)
        {
            if(surrogate == null) return default(GradientAlphaKey);
            return (GradientAlphaKey)surrogate.WriteTo(new GradientAlphaKey());
        }
        
        public static implicit operator PersistentGradientAlphaKey<TID>(GradientAlphaKey obj)
        {
            PersistentGradientAlphaKey<TID> surrogate = new PersistentGradientAlphaKey<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

