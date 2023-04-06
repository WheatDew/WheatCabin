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
    public partial class PersistentRigidbody<TID> : PersistentComponent<TID>
    {
        [ProtoMember(256)]
        public PersistentVector3<TID> velocity;

        [ProtoMember(257)]
        public PersistentVector3<TID> angularVelocity;

        [ProtoMember(258)]
        public float drag;

        [ProtoMember(259)]
        public float angularDrag;

        [ProtoMember(260)]
        public float mass;

        [ProtoMember(261)]
        public bool useGravity;

        [ProtoMember(262)]
        public float maxDepenetrationVelocity;

        [ProtoMember(263)]
        public bool isKinematic;

        [ProtoMember(264)]
        public bool freezeRotation;

        [ProtoMember(265)]
        public RigidbodyConstraints constraints;

        [ProtoMember(266)]
        public CollisionDetectionMode collisionDetectionMode;

        [ProtoMember(267)]
        public PersistentVector3<TID> centerOfMass;

        [ProtoMember(270)]
        public bool detectCollisions;

        [ProtoMember(271)]
        public PersistentVector3<TID> position;

        [ProtoMember(272)]
        public PersistentQuaternion<TID> rotation;

        [ProtoMember(273)]
        public RigidbodyInterpolation interpolation;

        [ProtoMember(274)]
        public int solverIterations;

        [ProtoMember(275)]
        public float sleepThreshold;

        [ProtoMember(276)]
        public float maxAngularVelocity;

        [ProtoMember(277)]
        public int solverVelocityIterations;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Rigidbody uo = (Rigidbody)obj;
            velocity = uo.velocity;
            angularVelocity = uo.angularVelocity;
            drag = uo.drag;
            angularDrag = uo.angularDrag;
            mass = uo.mass;
            useGravity = uo.useGravity;
            maxDepenetrationVelocity = uo.maxDepenetrationVelocity;
            isKinematic = uo.isKinematic;
            freezeRotation = uo.freezeRotation;
            constraints = uo.constraints;
            collisionDetectionMode = uo.collisionDetectionMode;
            centerOfMass = uo.centerOfMass;
            detectCollisions = uo.detectCollisions;
            position = uo.position;
            rotation = uo.rotation;
            interpolation = uo.interpolation;
            solverIterations = uo.solverIterations;
            sleepThreshold = uo.sleepThreshold;
            maxAngularVelocity = uo.maxAngularVelocity;
            solverVelocityIterations = uo.solverVelocityIterations;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Rigidbody uo = (Rigidbody)obj;
            uo.velocity = velocity;
            uo.angularVelocity = angularVelocity;
            uo.drag = drag;
            uo.angularDrag = angularDrag;
            uo.mass = mass;
            uo.useGravity = useGravity;
            uo.maxDepenetrationVelocity = maxDepenetrationVelocity;
            uo.isKinematic = isKinematic;
            uo.freezeRotation = freezeRotation;
            uo.constraints = constraints;
            uo.collisionDetectionMode = collisionDetectionMode;
            uo.centerOfMass = centerOfMass;
            uo.detectCollisions = detectCollisions;
            uo.position = position;
            uo.rotation = rotation;
            uo.interpolation = interpolation;
            uo.solverIterations = solverIterations;
            uo.sleepThreshold = sleepThreshold;
            uo.maxAngularVelocity = maxAngularVelocity;
            uo.solverVelocityIterations = solverVelocityIterations;
            return uo;
        }
    }
}

