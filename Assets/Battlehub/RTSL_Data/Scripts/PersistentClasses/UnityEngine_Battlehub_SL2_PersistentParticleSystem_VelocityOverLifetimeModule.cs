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
    public partial class PersistentParticleSystemNestedVelocityOverLifetimeModule<TID> : PersistentSurrogate<TID>
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
        public PersistentParticleSystemNestedMinMaxCurve<TID> orbitalX;

        [ProtoMember(264)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> orbitalY;

        [ProtoMember(265)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> orbitalZ;

        [ProtoMember(266)]
        public float orbitalXMultiplier;

        [ProtoMember(267)]
        public float orbitalYMultiplier;

        [ProtoMember(268)]
        public float orbitalZMultiplier;

        [ProtoMember(269)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> orbitalOffsetX;

        [ProtoMember(270)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> orbitalOffsetY;

        [ProtoMember(271)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> orbitalOffsetZ;

        [ProtoMember(272)]
        public float orbitalOffsetXMultiplier;

        [ProtoMember(273)]
        public float orbitalOffsetYMultiplier;

        [ProtoMember(274)]
        public float orbitalOffsetZMultiplier;

        [ProtoMember(275)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> radial;

        [ProtoMember(276)]
        public float radialMultiplier;

        [ProtoMember(277)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> speedModifier;

        [ProtoMember(278)]
        public float speedModifierMultiplier;

        [ProtoMember(279)]
        public ParticleSystemSimulationSpace space;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.VelocityOverLifetimeModule uo = (ParticleSystem.VelocityOverLifetimeModule)obj;
            enabled = uo.enabled;
            x = uo.x;
            y = uo.y;
            z = uo.z;
            xMultiplier = uo.xMultiplier;
            yMultiplier = uo.yMultiplier;
            zMultiplier = uo.zMultiplier;
            orbitalX = uo.orbitalX;
            orbitalY = uo.orbitalY;
            orbitalZ = uo.orbitalZ;
            orbitalXMultiplier = uo.orbitalXMultiplier;
            orbitalYMultiplier = uo.orbitalYMultiplier;
            orbitalZMultiplier = uo.orbitalZMultiplier;
            orbitalOffsetX = uo.orbitalOffsetX;
            orbitalOffsetY = uo.orbitalOffsetY;
            orbitalOffsetZ = uo.orbitalOffsetZ;
            orbitalOffsetXMultiplier = uo.orbitalOffsetXMultiplier;
            orbitalOffsetYMultiplier = uo.orbitalOffsetYMultiplier;
            orbitalOffsetZMultiplier = uo.orbitalOffsetZMultiplier;
            radial = uo.radial;
            radialMultiplier = uo.radialMultiplier;
            speedModifier = uo.speedModifier;
            speedModifierMultiplier = uo.speedModifierMultiplier;
            space = uo.space;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.VelocityOverLifetimeModule uo = (ParticleSystem.VelocityOverLifetimeModule)obj;
            uo.enabled = enabled;
            uo.x = x;
            uo.y = y;
            uo.z = z;
            uo.xMultiplier = xMultiplier;
            uo.yMultiplier = yMultiplier;
            uo.zMultiplier = zMultiplier;
            uo.orbitalX = orbitalX;
            uo.orbitalY = orbitalY;
            uo.orbitalZ = orbitalZ;
            uo.orbitalXMultiplier = orbitalXMultiplier;
            uo.orbitalYMultiplier = orbitalYMultiplier;
            uo.orbitalZMultiplier = orbitalZMultiplier;
            uo.orbitalOffsetX = orbitalOffsetX;
            uo.orbitalOffsetY = orbitalOffsetY;
            uo.orbitalOffsetZ = orbitalOffsetZ;
            uo.orbitalOffsetXMultiplier = orbitalOffsetXMultiplier;
            uo.orbitalOffsetYMultiplier = orbitalOffsetYMultiplier;
            uo.orbitalOffsetZMultiplier = orbitalOffsetZMultiplier;
            uo.radial = radial;
            uo.radialMultiplier = radialMultiplier;
            uo.speedModifier = speedModifier;
            uo.speedModifierMultiplier = speedModifierMultiplier;
            uo.space = space;
            return uo;
        }

        public static implicit operator ParticleSystem.VelocityOverLifetimeModule(PersistentParticleSystemNestedVelocityOverLifetimeModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.VelocityOverLifetimeModule);
            return (ParticleSystem.VelocityOverLifetimeModule)surrogate.WriteTo(new ParticleSystem.VelocityOverLifetimeModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedVelocityOverLifetimeModule<TID>(ParticleSystem.VelocityOverLifetimeModule obj)
        {
            PersistentParticleSystemNestedVelocityOverLifetimeModule<TID> surrogate = new PersistentParticleSystemNestedVelocityOverLifetimeModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

