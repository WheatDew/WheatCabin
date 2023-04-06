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
    public partial class PersistentParticleSystemNestedLightsModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public float ratio;

        [ProtoMember(258)]
        public bool useRandomDistribution;

        [ProtoMember(259)]
        public TID light;

        [ProtoMember(260)]
        public bool useParticleColor;

        [ProtoMember(261)]
        public bool sizeAffectsRange;

        [ProtoMember(262)]
        public bool alphaAffectsIntensity;

        [ProtoMember(263)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> range;

        [ProtoMember(264)]
        public float rangeMultiplier;

        [ProtoMember(265)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> intensity;

        [ProtoMember(266)]
        public float intensityMultiplier;

        [ProtoMember(267)]
        public int maxLights;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.LightsModule uo = (ParticleSystem.LightsModule)obj;
            enabled = uo.enabled;
            ratio = uo.ratio;
            useRandomDistribution = uo.useRandomDistribution;
            light = ToID(uo.light);
            useParticleColor = uo.useParticleColor;
            sizeAffectsRange = uo.sizeAffectsRange;
            alphaAffectsIntensity = uo.alphaAffectsIntensity;
            range = uo.range;
            rangeMultiplier = uo.rangeMultiplier;
            intensity = uo.intensity;
            intensityMultiplier = uo.intensityMultiplier;
            maxLights = uo.maxLights;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.LightsModule uo = (ParticleSystem.LightsModule)obj;
            uo.enabled = enabled;
            uo.ratio = ratio;
            uo.useRandomDistribution = useRandomDistribution;
            uo.light = FromID(light, uo.light);
            uo.useParticleColor = useParticleColor;
            uo.sizeAffectsRange = sizeAffectsRange;
            uo.alphaAffectsIntensity = alphaAffectsIntensity;
            uo.range = range;
            uo.rangeMultiplier = rangeMultiplier;
            uo.intensity = intensity;
            uo.intensityMultiplier = intensityMultiplier;
            uo.maxLights = maxLights;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(light, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            ParticleSystem.LightsModule uo = (ParticleSystem.LightsModule)obj;
            AddDep(uo.light, context);
        }

        public static implicit operator ParticleSystem.LightsModule(PersistentParticleSystemNestedLightsModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.LightsModule);
            return (ParticleSystem.LightsModule)surrogate.WriteTo(new ParticleSystem.LightsModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedLightsModule<TID>(ParticleSystem.LightsModule obj)
        {
            PersistentParticleSystemNestedLightsModule<TID> surrogate = new PersistentParticleSystemNestedLightsModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

