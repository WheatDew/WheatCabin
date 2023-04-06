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
    public partial class PersistentParticleSystemNestedShapeModule<TID> : PersistentSurrogate<TID>
    {
        [ProtoMember(256)]
        public bool enabled;

        [ProtoMember(257)]
        public ParticleSystemShapeType shapeType;

        [ProtoMember(258)]
        public float randomDirectionAmount;

        [ProtoMember(259)]
        public float sphericalDirectionAmount;

        [ProtoMember(260)]
        public float randomPositionAmount;

        [ProtoMember(261)]
        public bool alignToDirection;

        [ProtoMember(262)]
        public float radius;

        [ProtoMember(263)]
        public ParticleSystemShapeMultiModeValue radiusMode;

        [ProtoMember(264)]
        public float radiusSpread;

        [ProtoMember(265)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> radiusSpeed;

        [ProtoMember(266)]
        public float radiusSpeedMultiplier;

        [ProtoMember(267)]
        public float radiusThickness;

        [ProtoMember(268)]
        public float angle;

        [ProtoMember(269)]
        public float length;

        [ProtoMember(270)]
        public PersistentVector3<TID> boxThickness;

        [ProtoMember(271)]
        public ParticleSystemMeshShapeType meshShapeType;

        [ProtoMember(272)]
        public TID mesh;

        [ProtoMember(273)]
        public TID meshRenderer;

        [ProtoMember(274)]
        public TID skinnedMeshRenderer;

        [ProtoMember(275)]
        public TID sprite;

        [ProtoMember(276)]
        public TID spriteRenderer;

        [ProtoMember(277)]
        public bool useMeshMaterialIndex;

        [ProtoMember(278)]
        public int meshMaterialIndex;

        [ProtoMember(279)]
        public bool useMeshColors;

        [ProtoMember(280)]
        public float normalOffset;

        [ProtoMember(281)]
        public float arc;

        [ProtoMember(282)]
        public ParticleSystemShapeMultiModeValue arcMode;

        [ProtoMember(283)]
        public float arcSpread;

        [ProtoMember(284)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> arcSpeed;

        [ProtoMember(285)]
        public float arcSpeedMultiplier;

        [ProtoMember(286)]
        public float donutRadius;

        [ProtoMember(287)]
        public PersistentVector3<TID> position;

        [ProtoMember(288)]
        public PersistentVector3<TID> rotation;

        [ProtoMember(289)]
        public PersistentVector3<TID> scale;

        [ProtoMember(290)]
        public TID texture;

        [ProtoMember(291)]
        public ParticleSystemShapeTextureChannel textureClipChannel;

        [ProtoMember(292)]
        public float textureClipThreshold;

        [ProtoMember(293)]
        public bool textureColorAffectsParticles;

        [ProtoMember(294)]
        public bool textureAlphaAffectsParticles;

        [ProtoMember(295)]
        public bool textureBilinearFiltering;

        [ProtoMember(296)]
        public int textureUVChannel;

        [ProtoMember(300)]
        public ParticleSystemShapeMultiModeValue meshSpawnMode;

        [ProtoMember(301)]
        public float meshSpawnSpread;

        [ProtoMember(302)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> meshSpawnSpeed;

        [ProtoMember(303)]
        public float meshSpawnSpeedMultiplier;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystem.ShapeModule uo = (ParticleSystem.ShapeModule)obj;
            enabled = uo.enabled;
            shapeType = uo.shapeType;
            randomDirectionAmount = uo.randomDirectionAmount;
            sphericalDirectionAmount = uo.sphericalDirectionAmount;
            randomPositionAmount = uo.randomPositionAmount;
            alignToDirection = uo.alignToDirection;
            radius = uo.radius;
            radiusMode = uo.radiusMode;
            radiusSpread = uo.radiusSpread;
            radiusSpeed = uo.radiusSpeed;
            radiusSpeedMultiplier = uo.radiusSpeedMultiplier;
            radiusThickness = uo.radiusThickness;
            angle = uo.angle;
            length = uo.length;
            boxThickness = uo.boxThickness;
            meshShapeType = uo.meshShapeType;
            mesh = ToID(uo.mesh);
            meshRenderer = ToID(uo.meshRenderer);
            skinnedMeshRenderer = ToID(uo.skinnedMeshRenderer);
            sprite = ToID(uo.sprite);
            spriteRenderer = ToID(uo.spriteRenderer);
            useMeshMaterialIndex = uo.useMeshMaterialIndex;
            meshMaterialIndex = uo.meshMaterialIndex;
            useMeshColors = uo.useMeshColors;
            normalOffset = uo.normalOffset;
            arc = uo.arc;
            arcMode = uo.arcMode;
            arcSpread = uo.arcSpread;
            arcSpeed = uo.arcSpeed;
            arcSpeedMultiplier = uo.arcSpeedMultiplier;
            donutRadius = uo.donutRadius;
            position = uo.position;
            rotation = uo.rotation;
            scale = uo.scale;
            texture = ToID(uo.texture);
            textureClipChannel = uo.textureClipChannel;
            textureClipThreshold = uo.textureClipThreshold;
            textureColorAffectsParticles = uo.textureColorAffectsParticles;
            textureAlphaAffectsParticles = uo.textureAlphaAffectsParticles;
            textureBilinearFiltering = uo.textureBilinearFiltering;
            textureUVChannel = uo.textureUVChannel;
            meshSpawnMode = uo.meshSpawnMode;
            meshSpawnSpread = uo.meshSpawnSpread;
            meshSpawnSpeed = uo.meshSpawnSpeed;
            meshSpawnSpeedMultiplier = uo.meshSpawnSpeedMultiplier;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystem.ShapeModule uo = (ParticleSystem.ShapeModule)obj;
            uo.enabled = enabled;
            uo.shapeType = shapeType;
            uo.randomDirectionAmount = randomDirectionAmount;
            uo.sphericalDirectionAmount = sphericalDirectionAmount;
            uo.randomPositionAmount = randomPositionAmount;
            uo.alignToDirection = alignToDirection;
            uo.radius = radius;
            uo.radiusMode = radiusMode;
            uo.radiusSpread = radiusSpread;
            uo.radiusSpeed = radiusSpeed;
            uo.radiusSpeedMultiplier = radiusSpeedMultiplier;
            uo.radiusThickness = radiusThickness;
            uo.angle = angle;
            uo.length = length;
            uo.boxThickness = boxThickness;
            uo.meshShapeType = meshShapeType;
            uo.mesh = FromID(mesh, uo.mesh);
            uo.meshRenderer = FromID(meshRenderer, uo.meshRenderer);
            uo.skinnedMeshRenderer = FromID(skinnedMeshRenderer, uo.skinnedMeshRenderer);
            uo.sprite = FromID(sprite, uo.sprite);
            uo.spriteRenderer = FromID(spriteRenderer, uo.spriteRenderer);
            uo.useMeshMaterialIndex = useMeshMaterialIndex;
            uo.meshMaterialIndex = meshMaterialIndex;
            uo.useMeshColors = useMeshColors;
            uo.normalOffset = normalOffset;
            uo.arc = arc;
            uo.arcMode = arcMode;
            uo.arcSpread = arcSpread;
            uo.arcSpeed = arcSpeed;
            uo.arcSpeedMultiplier = arcSpeedMultiplier;
            uo.donutRadius = donutRadius;
            uo.position = position;
            uo.rotation = rotation;
            uo.scale = scale;
            uo.texture = FromID(texture, uo.texture);
            uo.textureClipChannel = textureClipChannel;
            uo.textureClipThreshold = textureClipThreshold;
            uo.textureColorAffectsParticles = textureColorAffectsParticles;
            uo.textureAlphaAffectsParticles = textureAlphaAffectsParticles;
            uo.textureBilinearFiltering = textureBilinearFiltering;
            uo.textureUVChannel = textureUVChannel;
            uo.meshSpawnMode = meshSpawnMode;
            uo.meshSpawnSpread = meshSpawnSpread;
            uo.meshSpawnSpeed = meshSpawnSpeed;
            uo.meshSpawnSpeedMultiplier = meshSpawnSpeedMultiplier;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(mesh, context);
            AddDep(meshRenderer, context);
            AddDep(skinnedMeshRenderer, context);
            AddDep(sprite, context);
            AddDep(spriteRenderer, context);
            AddDep(texture, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            ParticleSystem.ShapeModule uo = (ParticleSystem.ShapeModule)obj;
            AddDep(uo.mesh, context);
            AddDep(uo.meshRenderer, context);
            AddDep(uo.skinnedMeshRenderer, context);
            AddDep(uo.sprite, context);
            AddDep(uo.spriteRenderer, context);
            AddDep(uo.texture, context);
        }

        public static implicit operator ParticleSystem.ShapeModule(PersistentParticleSystemNestedShapeModule<TID> surrogate)
        {
            if(surrogate == null) return default(ParticleSystem.ShapeModule);
            return (ParticleSystem.ShapeModule)surrogate.WriteTo(new ParticleSystem.ShapeModule());
        }
        
        public static implicit operator PersistentParticleSystemNestedShapeModule<TID>(ParticleSystem.ShapeModule obj)
        {
            PersistentParticleSystemNestedShapeModule<TID> surrogate = new PersistentParticleSystemNestedShapeModule<TID>();
            surrogate.ReadFrom(obj);
            return surrogate;
        }
    }
}

