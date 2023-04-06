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
    public partial class PersistentParticleSystemNestedForceOverLifetimeModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> x;

        [ProtoMember(258)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> y;

        [ProtoMember(259)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> z;

        [ProtoMember(260)]
        public float xMultiplier;

        [ProtoMember(261)]
        public float yMultiplier;

        [ProtoMember(262)]
        public float zMultiplier;

        [ProtoMember(263)]
        public ParticleSystemSimulationSpace space;

        [ProtoMember(264)]
        public bool randomized;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.ForceOverLifetimeModule uo = (ParticleSystem.ForceOverLifetimeModule)obj;
            enabled = uo.enabled;
            x = uo.x;
            y = uo.y;
            z = uo.z;
            xMultiplier = uo.xMultiplier;
            yMultiplier = uo.yMultiplier;
            zMultiplier = uo.zMultiplier;
            space = uo.space;
            randomized = uo.randomized;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.ForceOverLifetimeModule uo = (ParticleSystem.ForceOverLifetimeModule)obj;
            uo.enabled = enabled;
            uo.x = x;
            uo.y = y;
            uo.z = z;
            uo.xMultiplier = xMultiplier;
            uo.yMultiplier = yMultiplier;
            uo.zMultiplier = zMultiplier;
            uo.space = space;
            uo.randomized = randomized;
            return uo;
        }

        public static implicit operator ParticleSystem.ForceOverLifetimeModule(PersistentParticleSystemNestedForceOverLifetimeModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.ForceOverLifetimeModule);
            return (ParticleSystem.ForceOverLifetimeModule)surrogate.WriteTo(new ParticleSystem.ForceOverLifetimeModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedForceOverLifetimeModule<TID>(ParticleSystem.ForceOverLifetimeModule obj)
        {
            PersistentParticleSystemNestedForceOverLifetimeModule<TID> surrogate = new PersistentParticleSystemNestedForceOverLifetimeModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

