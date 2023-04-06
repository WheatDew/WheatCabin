using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentParticleSystemNestedTextureSheetAnimationModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public ParticleSystemAnimationMode mode;

        [ProtoMember(258)]
        public int numTilesX;

        [ProtoMember(259)]
        public int numTilesY;

        [ProtoMember(260)]
        public ParticleSystemAnimationType animation;

        [ProtoMember(262)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> frameOverTime;

        [ProtoMember(263)]
        public float frameOverTimeMultiplier;

        [ProtoMember(264)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> startFrame;

        [ProtoMember(265)]
        public float startFrameMultiplier;

        [ProtoMember(266)]
        public int cycleCount;

        [ProtoMember(267)]
        public int rowIndex;

        [ProtoMember(268)]
        public UVChannelFlags uvChannelMask;

        [ProtoMember(271)]
        public ParticleSystemAnimationTimeMode timeMode;

        [ProtoMember(272)]
        public float fps;

        [ProtoMember(273)]
        public PersistentVector2<TID> speedRange;

        [ProtoMember(275)]
        public ParticleSystemAnimationRowMode rowMode;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.TextureSheetAnimationModule uo = (ParticleSystem.TextureSheetAnimationModule)obj;
            enabled = uo.enabled;
            mode = uo.mode;
            numTilesX = uo.numTilesX;
            numTilesY = uo.numTilesY;
            animation = uo.animation;
            frameOverTime = uo.frameOverTime;
            frameOverTimeMultiplier = uo.frameOverTimeMultiplier;
            startFrame = uo.startFrame;
            startFrameMultiplier = uo.startFrameMultiplier;
            cycleCount = uo.cycleCount;
            rowIndex = uo.rowIndex;
            uvChannelMask = uo.uvChannelMask;
            timeMode = uo.timeMode;
            fps = uo.fps;
            speedRange = uo.speedRange;
            rowMode = uo.rowMode;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.TextureSheetAnimationModule uo = (ParticleSystem.TextureSheetAnimationModule)obj;
            uo.enabled = enabled;
            uo.mode = mode;
            uo.numTilesX = numTilesX;
            uo.numTilesY = numTilesY;
            uo.animation = animation;
            uo.frameOverTime = frameOverTime;
            uo.frameOverTimeMultiplier = frameOverTimeMultiplier;
            uo.startFrame = startFrame;
            uo.startFrameMultiplier = startFrameMultiplier;
            uo.cycleCount = cycleCount;
            uo.rowIndex = rowIndex;
            uo.uvChannelMask = uvChannelMask;
            uo.timeMode = timeMode;
            uo.fps = fps;
            uo.speedRange = speedRange;
            uo.rowMode = rowMode;
            return uo;
        }

        public static implicit operator ParticleSystem.TextureSheetAnimationModule(PersistentParticleSystemNestedTextureSheetAnimationModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.TextureSheetAnimationModule);
            return (ParticleSystem.TextureSheetAnimationModule)surrogate.WriteTo(new ParticleSystem.TextureSheetAnimationModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedTextureSheetAnimationModule<TID>(ParticleSystem.TextureSheetAnimationModule obj)
        {
            PersistentParticleSystemNestedTextureSheetAnimationModule<TID> surrogate = new PersistentParticleSystemNestedTextureSheetAnimationModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

