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
    public partial class PersistentParticleSystemNestedMainModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public float duration;

        [ProtoMember(257)]
        public bool loop;

        [ProtoMember(258)]
        public bool prewarm;

        [ProtoMember(259)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startDelay;

        [ProtoMember(260)]
        public float startDelayMultiplier;

        [ProtoMember(261)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startLifetime;

        [ProtoMember(262)]
        public float startLifetimeMultiplier;

        [ProtoMember(263)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startSpeed;

        [ProtoMember(264)]
        public float startSpeedMultiplier;

        [ProtoMember(265)]
        public bool startSize3D;

        [ProtoMember(266)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startSize;

        [ProtoMember(267)]
        public float startSizeMultiplier;

        [ProtoMember(268)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startSizeX;

        [ProtoMember(269)]
        public float startSizeXMultiplier;

        [ProtoMember(270)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startSizeY;

        [ProtoMember(271)]
        public float startSizeYMultiplier;

        [ProtoMember(272)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startSizeZ;

        [ProtoMember(273)]
        public float startSizeZMultiplier;

        [ProtoMember(274)]
        public bool startRotation3D;

        [ProtoMember(275)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startRotation;

        [ProtoMember(276)]
        public float startRotationMultiplier;

        [ProtoMember(277)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startRotationX;

        [ProtoMember(278)]
        public float startRotationXMultiplier;

        [ProtoMember(279)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startRotationY;

        [ProtoMember(280)]
        public float startRotationYMultiplier;

        [ProtoMember(281)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startRotationZ;

        [ProtoMember(282)]
        public float startRotationZMultiplier;

        [ProtoMember(283)]
        public float flipRotation;

        [ProtoMember(284)]
        public PersistentParticleSystemNestedMinMaxGradient<TID> startColor;

        [ProtoMember(285)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> gravityModifier;

        [ProtoMember(286)]
        public float gravityModifierMultiplier;

        [ProtoMember(287)]
        public ParticleSystemSimulationSpace simulationSpace;

        [ProtoMember(288)]
        public TID customSimulationSpace;

        [ProtoMember(289)]
        public float simulationSpeed;

        [ProtoMember(290)]
        public bool useUnscaledTime;

        [ProtoMember(291)]
        public ParticleSystemScalingMode scalingMode;

        [ProtoMember(292)]
        public bool playOnAwake;

        [ProtoMember(293)]
        public int maxParticles;

        [ProtoMember(294)]
        public ParticleSystemEmitterVelocityMode emitterVelocityMode;

        [ProtoMember(295)]
        public ParticleSystemStopAction stopAction;

        [ProtoMember(297)]
        public ParticleSystemCullingMode cullingMode;

        [ProtoMember(298)]
        public ParticleSystemRingBufferMode ringBufferMode;

        [ProtoMember(299)]
        public PersistentVector2<TID> ringBufferLoopRange;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.MainModule uo = (ParticleSystem.MainModule)obj;
            duration = uo.duration;
            loop = uo.loop;
            prewarm = uo.prewarm;
            startDelay = uo.startDelay;
            startDelayMultiplier = uo.startDelayMultiplier;
            startLifetime = uo.startLifetime;
            startLifetimeMultiplier = uo.startLifetimeMultiplier;
            startSpeed = uo.startSpeed;
            startSpeedMultiplier = uo.startSpeedMultiplier;
            startSize3D = uo.startSize3D;
            startSize = uo.startSize;
            startSizeMultiplier = uo.startSizeMultiplier;
            startSizeX = uo.startSizeX;
            startSizeXMultiplier = uo.startSizeXMultiplier;
            startSizeY = uo.startSizeY;
            startSizeYMultiplier = uo.startSizeYMultiplier;
            startSizeZ = uo.startSizeZ;
            startSizeZMultiplier = uo.startSizeZMultiplier;
            startRotation3D = uo.startRotation3D;
            startRotation = uo.startRotation;
            startRotationMultiplier = uo.startRotationMultiplier;
            startRotationX = uo.startRotationX;
            startRotationXMultiplier = uo.startRotationXMultiplier;
            startRotationY = uo.startRotationY;
            startRotationYMultiplier = uo.startRotationYMultiplier;
            startRotationZ = uo.startRotationZ;
            startRotationZMultiplier = uo.startRotationZMultiplier;
            flipRotation = uo.flipRotation;
            startColor = uo.startColor;
            gravityModifier = uo.gravityModifier;
            gravityModifierMultiplier = uo.gravityModifierMultiplier;
            simulationSpace = uo.simulationSpace;
            customSimulationSpace = ToID(uo.customSimulationSpace);
            simulationSpeed = uo.simulationSpeed;
            useUnscaledTime = uo.useUnscaledTime;
            scalingMode = uo.scalingMode;
            playOnAwake = uo.playOnAwake;
            maxParticles = uo.maxParticles;
            emitterVelocityMode = uo.emitterVelocityMode;
            stopAction = uo.stopAction;
            cullingMode = uo.cullingMode;
            ringBufferMode = uo.ringBufferMode;
            ringBufferLoopRange = uo.ringBufferLoopRange;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.MainModule uo = (ParticleSystem.MainModule)obj;
            uo.duration = duration;
            uo.loop = loop;
            uo.prewarm = prewarm;
            uo.startDelay = startDelay;
            uo.startDelayMultiplier = startDelayMultiplier;
            uo.startLifetime = startLifetime;
            uo.startLifetimeMultiplier = startLifetimeMultiplier;
            uo.startSpeed = startSpeed;
            uo.startSpeedMultiplier = startSpeedMultiplier;
            uo.startSize3D = startSize3D;
            uo.startSize = startSize;
            uo.startSizeMultiplier = startSizeMultiplier;
            uo.startSizeX = startSizeX;
            uo.startSizeXMultiplier = startSizeXMultiplier;
            uo.startSizeY = startSizeY;
            uo.startSizeYMultiplier = startSizeYMultiplier;
            uo.startSizeZ = startSizeZ;
            uo.startSizeZMultiplier = startSizeZMultiplier;
            uo.startRotation3D = startRotation3D;
            uo.startRotation = startRotation;
            uo.startRotationMultiplier = startRotationMultiplier;
            uo.startRotationX = startRotationX;
            uo.startRotationXMultiplier = startRotationXMultiplier;
            uo.startRotationY = startRotationY;
            uo.startRotationYMultiplier = startRotationYMultiplier;
            uo.startRotationZ = startRotationZ;
            uo.startRotationZMultiplier = startRotationZMultiplier;
            uo.flipRotation = flipRotation;
            uo.startColor = startColor;
            uo.gravityModifier = gravityModifier;
            uo.gravityModifierMultiplier = gravityModifierMultiplier;
            uo.simulationSpace = simulationSpace;
            uo.customSimulationSpace = FromID(customSimulationSpace, uo.customSimulationSpace);
            uo.simulationSpeed = simulationSpeed;
            uo.useUnscaledTime = useUnscaledTime;
            uo.scalingMode = scalingMode;
            uo.playOnAwake = playOnAwake;
            uo.maxParticles = maxParticles;
            uo.emitterVelocityMode = emitterVelocityMode;
            uo.stopAction = stopAction;
            uo.cullingMode = cullingMode;
            uo.ringBufferMode = ringBufferMode;
            uo.ringBufferLoopRange = ringBufferLoopRange;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(customSimulationSpace, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            ParticleSystem.MainModule uo = (ParticleSystem.MainModule)obj;
            AddDep(uo.customSimulationSpace, context);
        }

        public static implicit operator ParticleSystem.MainModule(PersistentParticleSystemNestedMainModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.MainModule);
            return (ParticleSystem.MainModule)surrogate.WriteTo(new ParticleSystem.MainModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedMainModule<TID>(ParticleSystem.MainModule obj)
        {
            PersistentParticleSystemNestedMainModule<TID> surrogate = new PersistentParticleSystemNestedMainModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

