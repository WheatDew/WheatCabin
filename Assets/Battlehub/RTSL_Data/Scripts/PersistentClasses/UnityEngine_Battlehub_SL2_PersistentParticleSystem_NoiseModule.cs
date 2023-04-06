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
    public partial class PersistentParticleSystemNestedNoiseModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public bool separateAxes;

        [ProtoMember(258)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> strength;

        [ProtoMember(259)]
        public float strengthMultiplier;

        [ProtoMember(260)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> strengthX;

        [ProtoMember(261)]
        public float strengthXMultiplier;

        [ProtoMember(262)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> strengthY;

        [ProtoMember(263)]
        public float strengthYMultiplier;

        [ProtoMember(264)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> strengthZ;

        [ProtoMember(265)]
        public float strengthZMultiplier;

        [ProtoMember(266)]
        public float frequency;

        [ProtoMember(267)]
        public bool damping;

        [ProtoMember(268)]
        public int octaveCount;

        [ProtoMember(269)]
        public float octaveMultiplier;

        [ProtoMember(270)]
        public float octaveScale;

        [ProtoMember(271)]
        public ParticleSystemNoiseQuality quality;

        [ProtoMember(272)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> scrollSpeed;

        [ProtoMember(273)]
        public float scrollSpeedMultiplier;

        [ProtoMember(274)]
        public bool remapEnabled;

        [ProtoMember(275)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> remap;

        [ProtoMember(276)]
        public float remapMultiplier;

        [ProtoMember(277)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> remapX;

        [ProtoMember(278)]
        public float remapXMultiplier;

        [ProtoMember(279)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> remapY;

        [ProtoMember(280)]
        public float remapYMultiplier;

        [ProtoMember(281)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> remapZ;

        [ProtoMember(282)]
        public float remapZMultiplier;

        [ProtoMember(283)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> positionAmount;

        [ProtoMember(284)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> rotationAmount;

        [ProtoMember(285)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> sizeAmount;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.NoiseModule uo = (ParticleSystem.NoiseModule)obj;
            enabled = uo.enabled;
            separateAxes = uo.separateAxes;
            strength = uo.strength;
            strengthMultiplier = uo.strengthMultiplier;
            strengthX = uo.strengthX;
            strengthXMultiplier = uo.strengthXMultiplier;
            strengthY = uo.strengthY;
            strengthYMultiplier = uo.strengthYMultiplier;
            strengthZ = uo.strengthZ;
            strengthZMultiplier = uo.strengthZMultiplier;
            frequency = uo.frequency;
            damping = uo.damping;
            octaveCount = uo.octaveCount;
            octaveMultiplier = uo.octaveMultiplier;
            octaveScale = uo.octaveScale;
            quality = uo.quality;
            scrollSpeed = uo.scrollSpeed;
            scrollSpeedMultiplier = uo.scrollSpeedMultiplier;
            remapEnabled = uo.remapEnabled;
            remap = uo.remap;
            remapMultiplier = uo.remapMultiplier;
            remapX = uo.remapX;
            remapXMultiplier = uo.remapXMultiplier;
            remapY = uo.remapY;
            remapYMultiplier = uo.remapYMultiplier;
            remapZ = uo.remapZ;
            remapZMultiplier = uo.remapZMultiplier;
            positionAmount = uo.positionAmount;
            rotationAmount = uo.rotationAmount;
            sizeAmount = uo.sizeAmount;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.NoiseModule uo = (ParticleSystem.NoiseModule)obj;
            uo.enabled = enabled;
            uo.separateAxes = separateAxes;
            uo.strength = strength;
            uo.strengthMultiplier = strengthMultiplier;
            uo.strengthX = strengthX;
            uo.strengthXMultiplier = strengthXMultiplier;
            uo.strengthY = strengthY;
            uo.strengthYMultiplier = strengthYMultiplier;
            uo.strengthZ = strengthZ;
            uo.strengthZMultiplier = strengthZMultiplier;
            uo.frequency = frequency;
            uo.damping = damping;
            uo.octaveCount = octaveCount;
            uo.octaveMultiplier = octaveMultiplier;
            uo.octaveScale = octaveScale;
            uo.quality = quality;
            uo.scrollSpeed = scrollSpeed;
            uo.scrollSpeedMultiplier = scrollSpeedMultiplier;
            uo.remapEnabled = remapEnabled;
            uo.remap = remap;
            uo.remapMultiplier = remapMultiplier;
            uo.remapX = remapX;
            uo.remapXMultiplier = remapXMultiplier;
            uo.remapY = remapY;
            uo.remapYMultiplier = remapYMultiplier;
            uo.remapZ = remapZ;
            uo.remapZMultiplier = remapZMultiplier;
            uo.positionAmount = positionAmount;
            uo.rotationAmount = rotationAmount;
            uo.sizeAmount = sizeAmount;
            return uo;
        }

        public static implicit operator ParticleSystem.NoiseModule(PersistentParticleSystemNestedNoiseModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.NoiseModule);
            return (ParticleSystem.NoiseModule)surrogate.WriteTo(new ParticleSystem.NoiseModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedNoiseModule<TID>(ParticleSystem.NoiseModule obj)
        {
            PersistentParticleSystemNestedNoiseModule<TID> surrogate = new PersistentParticleSystemNestedNoiseModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

