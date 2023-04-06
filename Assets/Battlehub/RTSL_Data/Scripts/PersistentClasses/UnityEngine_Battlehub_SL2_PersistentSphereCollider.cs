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
    public partial class PersistentSphereCollider<TID> : PersistentCollider<TID>
    {
        [ProtoMember(256)]
        public PersistentVector3<TID> center;

        [ProtoMember(257)]
        public float radius;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            SphereCollider uo = (SphereCollider)obj;
            center = uo.center;
            radius = uo.radius;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            SphereCollider uo = (SphereCollider)obj;
            uo.center = center;
            uo.radius = radius;
            return uo;
        }
    }
}

