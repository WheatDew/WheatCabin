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
    public partial class PersistentParticleSystemNestedExternalForcesModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public float multiplier;

        [ProtoMember(258)]
        public ParticleSystemGameObjectFilter influenceFilter;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.ExternalForcesModule uo = (ParticleSystem.ExternalForcesModule)obj;
            enabled = uo.enabled;
            multiplier = uo.multiplier;
            influenceFilter = uo.influenceFilter;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.ExternalForcesModule uo = (ParticleSystem.ExternalForcesModule)obj;
            uo.enabled = enabled;
            uo.multiplier = multiplier;
            uo.influenceFilter = influenceFilter;
            return uo;
        }

        public static implicit operator ParticleSystem.ExternalForcesModule(PersistentParticleSystemNestedExternalForcesModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.ExternalForcesModule);
            return (ParticleSystem.ExternalForcesModule)surrogate.WriteTo(new ParticleSystem.ExternalForcesModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedExternalForcesModule<TID>(ParticleSystem.ExternalForcesModule obj)
        {
            PersistentParticleSystemNestedExternalForcesModule<TID> surrogate = new PersistentParticleSystemNestedExternalForcesModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

