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
    public partial class PersistentPhysicMaterial<TID> : PersistentObject<TID>
    {
        [ProtoMember(256)]
        public float bounciness;

        [ProtoMember(257)]
        public float dynamicFriction;

        [ProtoMember(258)]
        public float staticFriction;

        [ProtoMember(259)]
        public PhysicMaterialCombine frictionCombine;

        [ProtoMember(260)]
        public PhysicMaterialCombine bounceCombine;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            PhysicMaterial uo = (PhysicMaterial)obj;
            bounciness = uo.bounciness;
            dynamicFriction = uo.dynamicFriction;
            staticFriction = uo.staticFriction;
            frictionCombine = uo.frictionCombine;
            bounceCombine = uo.bounceCombine;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            PhysicMaterial uo = (PhysicMaterial)obj;
            uo.bounciness = bounciness;
            uo.dynamicFriction = dynamicFriction;
            uo.staticFriction = staticFriction;
            uo.frictionCombine = frictionCombine;
            uo.bounceCombine = bounceCombine;
            return uo;
        }
    }
}

