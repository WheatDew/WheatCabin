using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentTerrainCollider<TID> : PersistentCollider<TID>
    {
        [ProtoMember(256)]
        public TID terrainData;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TerrainCollider uo = (TerrainCollider)obj;
            terrainData = ToID(uo.terrainData);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TerrainCollider uo = (TerrainCollider)obj;
            uo.terrainData = FromID(terrainData, uo.terrainData);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(terrainData, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            TerrainCollider uo = (TerrainCollider)obj;
            AddDep(uo.terrainData, context);
        }
    }
}

