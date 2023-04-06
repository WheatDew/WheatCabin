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
    public partial class PersistentParticleSystemRenderer<TID> : PersistentRenderer<TID>
    {
        [ProtoMember(256)]
        public TID mesh;

        [ProtoMember(257)]
        public ParticleSystemRenderSpace alignment;

        [ProtoMember(258)]
        public ParticleSystemRenderMode renderMode;

        [ProtoMember(259)]
        public ParticleSystemSortMode sortMode;

        [ProtoMember(260)]
        public float lengthScale;

        [ProtoMember(261)]
        public float velocityScale;

        [ProtoMember(262)]
        public float cameraVelocityScale;

        [ProtoMember(263)]
        public float normalDirection;

        [ProtoMember(264)]
        public float sortingFudge;

        [ProtoMember(265)]
        public float minParticleSize;

        [ProtoMember(266)]
        public float maxParticleSize;

        [ProtoMember(267)]
        public PersistentVector3<TID> pivot;

        [ProtoMember(268)]
        public SpriteMaskInteraction maskInteraction;

        [ProtoMember(269)]
        public TID trailMaterial;

        [ProtoMember(270)]
        public bool enableGPUInstancing;

        [ProtoMember(271)]
        public float shadowBias;

        [ProtoMember(272)]
        public PersistentVector3<TID> flip;

        [ProtoMember(273)]
        public bool allowRoll;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystemRenderer uo = (ParticleSystemRenderer)obj;
            mesh = ToID(uo.mesh);
            alignment = uo.alignment;
            renderMode = uo.renderMode;
            sortMode = uo.sortMode;
            lengthScale = uo.lengthScale;
            velocityScale = uo.velocityScale;
            cameraVelocityScale = uo.cameraVelocityScale;
            normalDirection = uo.normalDirection;
            sortingFudge = uo.sortingFudge;
            minParticleSize = uo.minParticleSize;
            maxParticleSize = uo.maxParticleSize;
            pivot = uo.pivot;
            maskInteraction = uo.maskInteraction;
            trailMaterial = ToID(uo.trailMaterial);
            enableGPUInstancing = uo.enableGPUInstancing;
            shadowBias = uo.shadowBias;
            flip = uo.flip;
            allowRoll = uo.allowRoll;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystemRenderer uo = (ParticleSystemRenderer)obj;
            uo.mesh = FromID(mesh, uo.mesh);
            uo.alignment = alignment;
            uo.renderMode = renderMode;
            uo.sortMode = sortMode;
            uo.lengthScale = lengthScale;
            uo.velocityScale = velocityScale;
            uo.cameraVelocityScale = cameraVelocityScale;
            uo.normalDirection = normalDirection;
            uo.sortingFudge = sortingFudge;
            uo.minParticleSize = minParticleSize;
            uo.maxParticleSize = maxParticleSize;
            uo.pivot = pivot;
            uo.maskInteraction = maskInteraction;
            uo.trailMaterial = FromID(trailMaterial, uo.trailMaterial);
            uo.enableGPUInstancing = enableGPUInstancing;
            uo.shadowBias = shadowBias;
            uo.flip = flip;
            uo.allowRoll = allowRoll;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(mesh, context);
            AddDep(trailMaterial, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            ParticleSystemRenderer uo = (ParticleSystemRenderer)obj;
            AddDep(uo.mesh, context);
            AddDep(uo.trailMaterial, context);
        }
    }
}

