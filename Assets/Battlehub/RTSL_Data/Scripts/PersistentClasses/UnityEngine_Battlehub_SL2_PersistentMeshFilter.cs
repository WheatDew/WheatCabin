using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentMeshFilter<TID> : PersistentComponent<TID>
    {
        [ProtoMember(256)]
        public TID sharedMesh;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            MeshFilter uo = (MeshFilter)obj;
            sharedMesh = ToID(uo.sharedMesh);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            MeshFilter uo = (MeshFilter)obj;
            uo.sharedMesh = FromID(sharedMesh, uo.sharedMesh);
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
            MeshFilter uo = (MeshFilter)obj;
            AddDep(uo.sharedMesh, context);
        }
    }
}

