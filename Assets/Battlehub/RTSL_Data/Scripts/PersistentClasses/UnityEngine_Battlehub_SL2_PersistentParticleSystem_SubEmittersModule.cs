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
    public partial class PersistentParticleSystemNestedSubEmittersModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.SubEmittersModule uo = (ParticleSystem.SubEmittersModule)obj;
            enabled = uo.enabled;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.SubEmittersModule uo = (ParticleSystem.SubEmittersModule)obj;
            uo.enabled = enabled;
            return uo;
        }

        public static implicit operator ParticleSystem.SubEmittersModule(PersistentParticleSystemNestedSubEmittersModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.SubEmittersModule);
            return (ParticleSystem.SubEmittersModule)surrogate.WriteTo(new ParticleSystem.SubEmittersModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedSubEmittersModule<TID>(ParticleSystem.SubEmittersModule obj)
        {
            PersistentParticleSystemNestedSubEmittersModule<TID> surrogate = new PersistentParticleSystemNestedSubEmittersModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

