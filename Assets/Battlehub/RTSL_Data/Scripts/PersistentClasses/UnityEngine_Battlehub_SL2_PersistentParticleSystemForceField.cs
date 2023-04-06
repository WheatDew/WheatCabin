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
    public partial class PersistentParticleSystemForceField<TID> : PersistentBehaviour<TID>
    {
        [ProtoMember(256)]
        public ParticleSystemForceFieldShape shape;

        [ProtoMember(257)]
        public float startRange;

        [ProtoMember(258)]
        public float endRange;

        [ProtoMember(259)]
        public float length;

        [ProtoMember(260)]
        public float gravityFocus;

        [ProtoMember(261)]
        public PersistentVector2<TID> rotationRandomness;

        [ProtoMember(262)]
        public bool multiplyDragByParticleSize;

        [ProtoMember(263)]
        public bool multiplyDragByParticleVelocity;

        [ProtoMember(264)]
        public TID vectorField;

        [ProtoMember(265)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> directionX;

        [ProtoMember(266)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> directionY;

        [ProtoMember(267)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> directionZ;

        [ProtoMember(268)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> gravity;

        [ProtoMember(269)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> rotationSpeed;

        [ProtoMember(270)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> rotationAttraction;

        [ProtoMember(271)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> drag;

        [ProtoMember(272)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> vectorFieldSpeed;

        [ProtoMember(273)]
        public PersistentParticleSystemNestedMinMaxCurve<TID> vectorFieldAttraction;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            ParticleSystemForceField uo = (ParticleSystemForceField)obj;
            shape = uo.shape;
            startRange = uo.startRange;
            endRange = uo.endRange;
            length = uo.length;
            gravityFocus = uo.gravityFocus;
            rotationRandomness = uo.rotationRandomness;
            multiplyDragByParticleSize = uo.multiplyDragByParticleSize;
            multiplyDragByParticleVelocity = uo.multiplyDragByParticleVelocity;
            vectorField = ToID(uo.vectorField);
            directionX = uo.directionX;
            directionY = uo.directionY;
            directionZ = uo.directionZ;
            gravity = uo.gravity;
            rotationSpeed = uo.rotationSpeed;
            rotationAttraction = uo.rotationAttraction;
            drag = uo.drag;
            vectorFieldSpeed = uo.vectorFieldSpeed;
            vectorFieldAttraction = uo.vectorFieldAttraction;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            ParticleSystemForceField uo = (ParticleSystemForceField)obj;
            uo.shape = shape;
            uo.startRange = startRange;
            uo.endRange = endRange;
            uo.length = length;
            uo.gravityFocus = gravityFocus;
            uo.rotationRandomness = rotationRandomness;
            uo.multiplyDragByParticleSize = multiplyDragByParticleSize;
            uo.multiplyDragByParticleVelocity = multiplyDragByParticleVelocity;
            uo.vectorField = FromID(vectorField, uo.vectorField);
            uo.directionX = directionX;
            uo.directionY = directionY;
            uo.directionZ = directionZ;
            uo.gravity = gravity;
            uo.rotationSpeed = rotationSpeed;
            uo.rotationAttraction = rotationAttraction;
            uo.drag = drag;
            uo.vectorFieldSpeed = vectorFieldSpeed;
            uo.vectorFieldAttraction = vectorFieldAttraction;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(vectorField, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            ParticleSystemForceField uo = (ParticleSystemForceField)obj;
            AddDep(uo.vectorField, context);
        }
    }
}

