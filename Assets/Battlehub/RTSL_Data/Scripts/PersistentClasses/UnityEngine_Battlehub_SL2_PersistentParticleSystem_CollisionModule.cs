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
    public partial class PersistentParticleSystemNestedCollisionModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public ParticleSystemCollisionType type;

        [ProtoMember(258)]
        public ParticleSystemCollisionMode mode;

        [ProtoMember(259)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> dampen;

        [ProtoMember(260)]
        public float dampenMultiplier;

        [ProtoMember(261)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> bounce;

        [ProtoMember(262)]
        public float bounceMultiplier;

        [ProtoMember(263)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> lifetimeLoss;

        [ProtoMember(264)]
        public float lifetimeLossMultiplier;

        [ProtoMember(265)]
        public float minKillSpeed;

        [ProtoMember(266)]
        public float maxKillSpeed;

        [ProtoMember(267)]
        public PersistentLayerMask<TID> collidesWith;

        [ProtoMember(268)]
        public bool enableDynamicColliders;

        [ProtoMember(269)]
        public int maxCollisionShapes;

        [ProtoMember(270)]
        public ParticleSystemCollisionQuality quality;

        [ProtoMember(271)]
        public float voxelSize;

        [ProtoMember(272)]
        public float radiusScale;

        [ProtoMember(273)]
        public bool sendCollisionMessages;

        [ProtoMember(274)]
        public float colliderForce;

        [ProtoMember(275)]
        public bool multiplyColliderForceByCollisionAngle;

        [ProtoMember(276)]
        public bool multiplyColliderForceByParticleSpeed;

        [ProtoMember(277)]
        public bool multiplyColliderForceByParticleSize;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.CollisionModule uo = (ParticleSystem.CollisionModule)obj;
            enabled = uo.enabled;
            type = uo.type;
            mode = uo.mode;
            dampen = uo.dampen;
            dampenMultiplier = uo.dampenMultiplier;
            bounce = uo.bounce;
            bounceMultiplier = uo.bounceMultiplier;
            lifetimeLoss = uo.lifetimeLoss;
            lifetimeLossMultiplier = uo.lifetimeLossMultiplier;
            minKillSpeed = uo.minKillSpeed;
            maxKillSpeed = uo.maxKillSpeed;
            collidesWith = uo.collidesWith;
            enableDynamicColliders = uo.enableDynamicColliders;
            maxCollisionShapes = uo.maxCollisionShapes;
            quality = uo.quality;
            voxelSize = uo.voxelSize;
            radiusScale = uo.radiusScale;
            sendCollisionMessages = uo.sendCollisionMessages;
            colliderForce = uo.colliderForce;
            multiplyColliderForceByCollisionAngle = uo.multiplyColliderForceByCollisionAngle;
            multiplyColliderForceByParticleSpeed = uo.multiplyColliderForceByParticleSpeed;
            multiplyColliderForceByParticleSize = uo.multiplyColliderForceByParticleSize;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.CollisionModule uo = (ParticleSystem.CollisionModule)obj;
            uo.enabled = enabled;
            uo.type = type;
            uo.mode = mode;
            uo.dampen = dampen;
            uo.dampenMultiplier = dampenMultiplier;
            uo.bounce = bounce;
            uo.bounceMultiplier = bounceMultiplier;
            uo.lifetimeLoss = lifetimeLoss;
            uo.lifetimeLossMultiplier = lifetimeLossMultiplier;
            uo.minKillSpeed = minKillSpeed;
            uo.maxKillSpeed = maxKillSpeed;
            uo.collidesWith = collidesWith;
            uo.enableDynamicColliders = enableDynamicColliders;
            uo.maxCollisionShapes = maxCollisionShapes;
            uo.quality = quality;
            uo.voxelSize = voxelSize;
            uo.radiusScale = radiusScale;
            uo.sendCollisionMessages = sendCollisionMessages;
            uo.colliderForce = colliderForce;
            uo.multiplyColliderForceByCollisionAngle = multiplyColliderForceByCollisionAngle;
            uo.multiplyColliderForceByParticleSpeed = multiplyColliderForceByParticleSpeed;
            uo.multiplyColliderForceByParticleSize = multiplyColliderForceByParticleSize;
            return uo;
        }

        public static implicit operator ParticleSystem.CollisionModule(PersistentParticleSystemNestedCollisionModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.CollisionModule);
            return (ParticleSystem.CollisionModule)surrogate.WriteTo(new ParticleSystem.CollisionModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedCollisionModule<TID>(ParticleSystem.CollisionModule obj)
        {
            PersistentParticleSystemNestedCollisionModule<TID> surrogate = new PersistentParticleSystemNestedCollisionModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

