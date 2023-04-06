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
    public partial class PersistentParticleSystemNestedColorBySpeedModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public PersistentParticleSystemNestedMinMaxGradient<TID> color;

        [ProtoMember(258)]
        public PersistentVector2<TID> range;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.ColorBySpeedModule uo = (ParticleSystem.ColorBySpeedModule)obj;
            enabled = uo.enabled;
            color = uo.color;
            range = uo.range;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.ColorBySpeedModule uo = (ParticleSystem.ColorBySpeedModule)obj;
            uo.enabled = enabled;
            uo.color = color;
            uo.range = range;
            return uo;
        }

        public static implicit operator ParticleSystem.ColorBySpeedModule(PersistentParticleSystemNestedColorBySpeedModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.ColorBySpeedModule);
            return (ParticleSystem.ColorBySpeedModule)surrogate.WriteTo(new ParticleSystem.ColorBySpeedModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedColorBySpeedModule<TID>(ParticleSystem.ColorBySpeedModule obj)
        {
            PersistentParticleSystemNestedColorBySpeedModule<TID> surrogate = new PersistentParticleSystemNestedColorBySpeedModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

