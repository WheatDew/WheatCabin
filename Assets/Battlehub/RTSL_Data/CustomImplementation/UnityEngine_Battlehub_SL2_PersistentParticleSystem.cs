#if !RTSL_MAINTENANCE
using Battlehub.RTSL;
using ProtoBuf;
using UnityEngine;

namespace UnityEngine.Battlehub.SL2
{
    [CustomImplementation]
    public partial class PersistentParticleSystem<TID> 
    {        
        [ProtoMember(1)]
        public PersistentParticleSystemNestedCollisionModule<TID> m_collisionModule;

        [ProtoMember(2)]
        public PersistentParticleSystemNestedColorBySpeedModule<TID> m_colorBySpeedModule;

        [ProtoMember(3)]
        public PersistentParticleSystemNestedColorOverLifetimeModule<TID> m_colorOverLifetimeModule;

        [ProtoMember(4)]
        public PersistentParticleSystemNestedCustomDataModule<TID> m_customDataModule;

        [ProtoMember(5)]
        public PersistentParticleSystemNestedEmissionModule<TID> m_emissonModule;

        [ProtoMember(6)]
        public PersistentParticleSystemNestedExternalForcesModule<TID> m_externalForcesModule;

        [ProtoMember(7)]
        public PersistentParticleSystemNestedForceOverLifetimeModule<TID> m_forceOverlifeTimeModule;

        [ProtoMember(8)]
        public PersistentParticleSystemNestedInheritVelocityModule<TID> m_inheritVelocityModule;

        [ProtoMember(9)]
        public PersistentParticleSystemNestedLightsModule<TID> m_lightsModule;

        [ProtoMember(10)]
        public PersistentParticleSystemNestedLimitVelocityOverLifetimeModule<TID> m_limitVelocityOverLifetimeModule;

        [ProtoMember(11)]
        public PersistentParticleSystemNestedMainModule<TID> m_mainModule;

        [ProtoMember(12)]
        public PersistentParticleSystemNestedNoiseModule<TID> m_noiseModule;

        [ProtoMember(13)]
        public PersistentParticleSystemNestedRotationBySpeedModule<TID> m_rotationBySpeedModule;

        [ProtoMember(14)]
        public PersistentParticleSystemNestedRotationOverLifetimeModule<TID> m_rotationOverlifetimeModule;

        [ProtoMember(15)]
        public PersistentParticleSystemNestedShapeModule<TID> m_shapeModule;

        [ProtoMember(16)]
        public PersistentParticleSystemNestedSizeBySpeedModule<TID> m_sizeBySpeedModule;

        [ProtoMember(17)]
        public PersistentParticleSystemNestedSizeOverLifetimeModule<TID> m_sizeOverlifeTimeModule;

        [ProtoMember(18)]
        public PersistentParticleSystemNestedSubEmittersModule<TID> m_subEmittersModule;

        [ProtoMember(19)]
        public PersistentParticleSystemNestedTextureSheetAnimationModule<TID> m_textureSheetAnimationModule;

        [ProtoMember(20)]
        public PersistentParticleSystemNestedTrailModule<TID> m_trialModule;

        [ProtoMember(21)]
        public PersistentParticleSystemNestedTriggerModule<TID> m_triggerModule;

        [ProtoMember(22)]
        public PersistentParticleSystemNestedVelocityOverLifetimeModule<TID> m_velocityOverLifetimeModule;

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            ParticleSystem o = (ParticleSystem)obj;
            WriteSurrogateTo(m_collisionModule, o.collision);
            WriteSurrogateTo(m_colorBySpeedModule, o.colorBySpeed);
            WriteSurrogateTo(m_colorOverLifetimeModule, o.colorOverLifetime);
            WriteSurrogateTo(m_customDataModule, o.customData);
            WriteSurrogateTo(m_emissonModule, o.emission);
            WriteSurrogateTo(m_externalForcesModule, o.externalForces);
            WriteSurrogateTo(m_forceOverlifeTimeModule, o.forceOverLifetime);
            WriteSurrogateTo(m_inheritVelocityModule, o.inheritVelocity);
            WriteSurrogateTo(m_lightsModule, o.lights);
            WriteSurrogateTo(m_limitVelocityOverLifetimeModule, o.limitVelocityOverLifetime);
            WriteSurrogateTo(m_mainModule, o.main);
            WriteSurrogateTo(m_noiseModule, o.noise);
            WriteSurrogateTo(m_rotationBySpeedModule, o.rotationBySpeed);
            WriteSurrogateTo(m_rotationOverlifetimeModule, o.rotationOverLifetime);
            WriteSurrogateTo(m_shapeModule, o.shape);
            WriteSurrogateTo(m_sizeBySpeedModule, o.sizeBySpeed);
            WriteSurrogateTo(m_sizeOverlifeTimeModule, o.sizeOverLifetime);
            WriteSurrogateTo(m_subEmittersModule, o.subEmitters);
            WriteSurrogateTo(m_textureSheetAnimationModule, o.textureSheetAnimation);
            WriteSurrogateTo(m_trialModule, o.trails);
            WriteSurrogateTo(m_triggerModule, o.trigger);
            WriteSurrogateTo(m_velocityOverLifetimeModule, o.velocityOverLifetime);
            return obj;
        }

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }

            ParticleSystem o = (ParticleSystem)obj;
            m_collisionModule = ReadSurrogateFrom<PersistentParticleSystemNestedCollisionModule<TID>>(o.collision);
            m_colorBySpeedModule = ReadSurrogateFrom<PersistentParticleSystemNestedColorBySpeedModule<TID>>(o.colorBySpeed);
            m_colorOverLifetimeModule = ReadSurrogateFrom<PersistentParticleSystemNestedColorOverLifetimeModule<TID>>(o.colorOverLifetime);
            m_customDataModule = ReadSurrogateFrom<PersistentParticleSystemNestedCustomDataModule<TID>>(o.customData);
            m_emissonModule = ReadSurrogateFrom<PersistentParticleSystemNestedEmissionModule<TID>>(o.emission);
            m_externalForcesModule = ReadSurrogateFrom<PersistentParticleSystemNestedExternalForcesModule<TID>>(o.externalForces);
            m_forceOverlifeTimeModule = ReadSurrogateFrom<PersistentParticleSystemNestedForceOverLifetimeModule<TID>>(o.forceOverLifetime);
            m_inheritVelocityModule = ReadSurrogateFrom<PersistentParticleSystemNestedInheritVelocityModule<TID>>(o.inheritVelocity);
            m_lightsModule = ReadSurrogateFrom<PersistentParticleSystemNestedLightsModule<TID>>(o.lights);
            m_limitVelocityOverLifetimeModule = ReadSurrogateFrom<PersistentParticleSystemNestedLimitVelocityOverLifetimeModule<TID>>(o.limitVelocityOverLifetime);
            m_mainModule = ReadSurrogateFrom<PersistentParticleSystemNestedMainModule<TID>>(o.main);
            m_noiseModule = ReadSurrogateFrom<PersistentParticleSystemNestedNoiseModule<TID>>(o.noise);
            m_rotationBySpeedModule = ReadSurrogateFrom<PersistentParticleSystemNestedRotationBySpeedModule<TID>>(o.rotationBySpeed);
            m_rotationOverlifetimeModule = ReadSurrogateFrom<PersistentParticleSystemNestedRotationOverLifetimeModule<TID>>(o.rotationOverLifetime);
            m_shapeModule = ReadSurrogateFrom<PersistentParticleSystemNestedShapeModule<TID>>(o.shape);
            m_sizeBySpeedModule = ReadSurrogateFrom<PersistentParticleSystemNestedSizeBySpeedModule<TID>>(o.sizeBySpeed);
            m_sizeOverlifeTimeModule = ReadSurrogateFrom<PersistentParticleSystemNestedSizeOverLifetimeModule<TID>>(o.sizeOverLifetime);
            m_subEmittersModule = ReadSurrogateFrom<PersistentParticleSystemNestedSubEmittersModule<TID>>(o.subEmitters);
            m_textureSheetAnimationModule = ReadSurrogateFrom<PersistentParticleSystemNestedTextureSheetAnimationModule<TID>>(o.textureSheetAnimation);
            m_trialModule = ReadSurrogateFrom<PersistentParticleSystemNestedTrailModule<TID>>(o.trails);
            m_triggerModule = ReadSurrogateFrom<PersistentParticleSystemNestedTriggerModule<TID>>(o.trigger);
            m_velocityOverLifetimeModule = ReadSurrogateFrom<PersistentParticleSystemNestedVelocityOverLifetimeModule<TID>>(o.velocityOverLifetime);
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);
            AddSurrogateDeps(m_collisionModule, context);
            AddSurrogateDeps(m_colorBySpeedModule, context);
            AddSurrogateDeps(m_colorOverLifetimeModule, context);
            AddSurrogateDeps(m_customDataModule, context);
            AddSurrogateDeps(m_emissonModule, context);
            AddSurrogateDeps(m_externalForcesModule, context);
            AddSurrogateDeps(m_forceOverlifeTimeModule, context);
            AddSurrogateDeps(m_inheritVelocityModule, context);
            AddSurrogateDeps(m_lightsModule, context);
            AddSurrogateDeps(m_limitVelocityOverLifetimeModule, context);
            AddSurrogateDeps(m_mainModule, context);
            AddSurrogateDeps(m_noiseModule, context);
            AddSurrogateDeps(m_rotationBySpeedModule, context);
            AddSurrogateDeps(m_rotationOverlifetimeModule, context);
            AddSurrogateDeps(m_shapeModule, context);
            AddSurrogateDeps(m_sizeBySpeedModule, context);
            AddSurrogateDeps(m_sizeOverlifeTimeModule, context);
            AddSurrogateDeps(m_subEmittersModule, context);
            AddSurrogateDeps(m_textureSheetAnimationModule, context);
            AddSurrogateDeps(m_trialModule, context);
            AddSurrogateDeps(m_triggerModule, context);
            AddSurrogateDeps(m_velocityOverLifetimeModule, context);
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            if (obj == null)
            {
                return;
            }

            ParticleSystem o = (ParticleSystem)obj;
            AddSurrogateDeps(o.collision, v_ => (PersistentParticleSystemNestedCollisionModule<TID>)v_, context);
            AddSurrogateDeps(o.colorBySpeed, v_ => (PersistentParticleSystemNestedColorBySpeedModule<TID>)v_, context);
            AddSurrogateDeps(o.colorOverLifetime, v_ => (PersistentParticleSystemNestedColorOverLifetimeModule<TID>)v_, context);
            AddSurrogateDeps(o.customData, v_ => (PersistentParticleSystemNestedCustomDataModule<TID>)v_, context);
            AddSurrogateDeps(o.emission, v_ => (PersistentParticleSystemNestedEmissionModule<TID>)v_, context);
            AddSurrogateDeps(o.externalForces, v_ => (PersistentParticleSystemNestedExternalForcesModule<TID>)v_, context);
            AddSurrogateDeps(o.forceOverLifetime, v_ => (PersistentParticleSystemNestedForceOverLifetimeModule<TID>)v_, context);
            AddSurrogateDeps(o.inheritVelocity, v_ => (PersistentParticleSystemNestedInheritVelocityModule<TID>)v_, context);
            AddSurrogateDeps(o.lights, v_ => (PersistentParticleSystemNestedLightsModule<TID>)v_, context);
            AddSurrogateDeps(o.limitVelocityOverLifetime, v_ => (PersistentParticleSystemNestedLimitVelocityOverLifetimeModule<TID>)v_, context);
            AddSurrogateDeps(o.main, v_ => (PersistentParticleSystemNestedMainModule<TID>)v_, context);
            AddSurrogateDeps(o.noise, v_ => (PersistentParticleSystemNestedNoiseModule<TID>)v_, context);
            AddSurrogateDeps(o.rotationBySpeed, v_ => (PersistentParticleSystemNestedRotationBySpeedModule<TID>)v_, context);
            AddSurrogateDeps(o.rotationOverLifetime, v_ => (PersistentParticleSystemNestedRotationOverLifetimeModule<TID>)v_, context);
            AddSurrogateDeps(o.shape, v_ => (PersistentParticleSystemNestedShapeModule<TID>)v_, context);
            AddSurrogateDeps(o.sizeBySpeed, v_ => (PersistentParticleSystemNestedSizeBySpeedModule<TID>)v_, context);
            AddSurrogateDeps(o.sizeOverLifetime, v_ => (PersistentParticleSystemNestedSizeOverLifetimeModule<TID>)v_, context);
            AddSurrogateDeps(o.subEmitters, v_ => (PersistentParticleSystemNestedSubEmittersModule<TID>)v_, context);
            AddSurrogateDeps(o.textureSheetAnimation, v_ => (PersistentParticleSystemNestedTextureSheetAnimationModule<TID>)v_, context);
            AddSurrogateDeps(o.trails, v_ => (PersistentParticleSystemNestedTrailModule<TID>)v_, context);
            AddSurrogateDeps(o.trigger, v_ => (PersistentParticleSystemNestedTriggerModule<TID>)v_, context);
            AddSurrogateDeps(o.velocityOverLifetime, v_ => (PersistentParticleSystemNestedVelocityOverLifetimeModule<TID>)v_, context);
        }
    }
}
#endif

