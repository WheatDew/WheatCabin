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
    public partial class PersistentParticleSystemNestedCustomDataModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.CustomDataModule uo = (ParticleSystem.CustomDataModule)obj;
            enabled = uo.enabled;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.CustomDataModule uo = (ParticleSystem.CustomDataModule)obj;
            uo.enabled = enabled;
            return uo;
        }

        public static implicit operator ParticleSystem.CustomDataModule(PersistentParticleSystemNestedCustomDataModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.CustomDataModule);
            return (ParticleSystem.CustomDataModule)surrogate.WriteTo(new ParticleSystem.CustomDataModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedCustomDataModule<TID>(ParticleSystem.CustomDataModule obj)
        {
            PersistentParticleSystemNestedCustomDataModule<TID> surrogate = new PersistentParticleSystemNestedCustomDataModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

