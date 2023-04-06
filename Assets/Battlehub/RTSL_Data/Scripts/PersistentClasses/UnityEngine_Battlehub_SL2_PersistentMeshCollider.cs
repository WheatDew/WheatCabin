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
    public partial class PersistentMeshCollider<TID> : PersistentCollider<TID>
    {
        [ProtoMember(256)]
        public TID sharedMesh;

        [ProtoMember(257)]
        public bool convex;

        [ProtoMember(259)]
        public MeshColliderCookingOptions cookingOptions;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            MeshCollider uo = (MeshCollider)obj;
            sharedMesh = ToID(uo.sharedMesh);
            convex = uo.convex;
            cookingOptions = uo.cookingOptions;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            MeshCollider uo = (MeshCollider)obj;
            uo.sharedMesh = FromID(sharedMesh, uo.sharedMesh);
            uo.convex = convex;
            uo.cookingOptions = cookingOptions;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(sharedMesh, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            MeshCollider uo = (MeshCollider)obj;
            AddDep(uo.sharedMesh, context);
        }
    }
}

