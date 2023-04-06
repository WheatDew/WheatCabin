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
    public partial class PersistentParticleSystemNestedTrailModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public ParticleSystemTrailMode mode;

        [ProtoMember(258)]
        public float ratio;

        [ProtoMember(259)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> lifetime;

        [ProtoMember(260)]
        public float lifetimeMultiplier;

        [ProtoMember(261)]
        public float minVertexDistance;

        [ProtoMember(262)]
        public ParticleSystemTrailTextureMode textureMode;

        [ProtoMember(263)]
        public bool worldSpace;

        [ProtoMember(264)]
        public bool dieWithParticles;

        [ProtoMember(265)]
        public bool sizeAffectsWidth;

        [ProtoMember(266)]
        public bool sizeAffectsLifetime;

        [ProtoMember(267)]
        public bool inheritParticleColor;

        [ProtoMember(268)]
        public PersistentParticleSystemNestedMinMaxGradient<TID> colorOverLifetime;

        [ProtoMember(269)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> widthOverTrail;

        [ProtoMember(270)]
        public float widthOverTrailMultiplier;

        [ProtoMember(271)]
        public PersistentParticleSystemNestedMinMaxGradient<TID> colorOverTrail;

        [ProtoMember(272)]
        public bool generateLightingData;

        [ProtoMember(273)]
        public int ribbonCount;

        [ProtoMember(274)]
        public float shadowBias;

        [ProtoMember(275)]
        public bool splitSubEmitterRibbons;

        [ProtoMember(276)]
        public bool attachRibbonsToTransform;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.TrailModule uo = (ParticleSystem.TrailModule)obj;
            enabled = uo.enabled;
            mode = uo.mode;
            ratio = uo.ratio;
            lifetime = uo.lifetime;
            lifetimeMultiplier = uo.lifetimeMultiplier;
            minVertexDistance = uo.minVertexDistance;
            textureMode = uo.textureMode;
            worldSpace = uo.worldSpace;
            dieWithParticles = uo.dieWithParticles;
            sizeAffectsWidth = uo.sizeAffectsWidth;
            sizeAffectsLifetime = uo.sizeAffectsLifetime;
            inheritParticleColor = uo.inheritParticleColor;
            colorOverLifetime = uo.colorOverLifetime;
            widthOverTrail = uo.widthOverTrail;
            widthOverTrailMultiplier = uo.widthOverTrailMultiplier;
            colorOverTrail = uo.colorOverTrail;
            generateLightingData = uo.generateLightingData;
            ribbonCount = uo.ribbonCount;
            shadowBias = uo.shadowBias;
            splitSubEmitterRibbons = uo.splitSubEmitterRibbons;
            attachRibbonsToTransform = uo.attachRibbonsToTransform;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.TrailModule uo = (ParticleSystem.TrailModule)obj;
            uo.enabled = enabled;
            uo.mode = mode;
            uo.ratio = ratio;
            uo.lifetime = lifetime;
            uo.lifetimeMultiplier = lifetimeMultiplier;
            uo.minVertexDistance = minVertexDistance;
            uo.textureMode = textureMode;
            uo.worldSpace = worldSpace;
            uo.dieWithParticles = dieWithParticles;
            uo.sizeAffectsWidth = sizeAffectsWidth;
            uo.sizeAffectsLifetime = sizeAffectsLifetime;
            uo.inheritParticleColor = inheritParticleColor;
            uo.colorOverLifetime = colorOverLifetime;
            uo.widthOverTrail = widthOverTrail;
            uo.widthOverTrailMultiplier = widthOverTrailMultiplier;
            uo.colorOverTrail = colorOverTrail;
            uo.generateLightingData = generateLightingData;
            uo.ribbonCount = ribbonCount;
            uo.shadowBias = shadowBias;
            uo.splitSubEmitterRibbons = splitSubEmitterRibbons;
            uo.attachRibbonsToTransform = attachRibbonsToTransform;
            return uo;
        }

        public static implicit operator ParticleSystem.TrailModule(PersistentParticleSystemNestedTrailModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.TrailModule);
            return (ParticleSystem.TrailModule)surrogate.WriteTo(new ParticleSystem.TrailModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedTrailModule<TID>(ParticleSystem.TrailModule obj)
        {
            PersistentParticleSystemNestedTrailModule<TID> surrogate = new PersistentParticleSystemNestedTrailModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

