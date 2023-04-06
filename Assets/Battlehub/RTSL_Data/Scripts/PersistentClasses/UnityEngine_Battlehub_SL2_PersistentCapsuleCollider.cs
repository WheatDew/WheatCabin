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
    public partial class PersistentCapsuleCollider<TID> : PersistentCollider<TID>
    {
        [ProtoMember(256)]
        public PersistentVector3<TID> center;

        [ProtoMember(257)]
        public float radius;

        [ProtoMember(258)]
        public float height;

        [ProtoMember(259)]
        public int direction;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            CapsuleCollider uo = (CapsuleCollider)obj;
            center = uo.center;
            radius = uo.radius;
            height = uo.height;
            direction = uo.direction;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            CapsuleCollider uo = (CapsuleCollider)obj;
            uo.center = center;
            uo.radius = radius;
            uo.height = height;
            uo.direction = direction;
            return uo;
        }
    }
}

