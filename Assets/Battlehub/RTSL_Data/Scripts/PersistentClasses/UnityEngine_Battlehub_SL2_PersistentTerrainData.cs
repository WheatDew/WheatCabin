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
    public partial class PersistentTerrainData<TID> : PersistentObject<TID>
    {
        [ProtoMember(259)]
        public float wavingGrassStrength;

        [ProtoMember(260)]
        public float wavingGrassAmount;

        [ProtoMember(261)]
        public float wavingGrassSpeed;

        [ProtoMember(262)]
        public PersistentColor<TID> wavingGrassTint;

        [ProtoMember(266)]
        public int alphamapResolution;

        [ProtoMember(267)]
        public int baseMapResolution;

        [ProtoMember(269)]
        public TID[] terrainLayers;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TerrainData uo = (TerrainData)obj;
            wavingGrassStrength = uo.wavingGrassStrength;
            wavingGrassAmount = uo.wavingGrassAmount;
            wavingGrassSpeed = uo.wavingGrassSpeed;
            wavingGrassTint = uo.wavingGrassTint;
            alphamapResolution = uo.alphamapResolution;
            baseMapResolution = uo.baseMapResolution;
            terrainLayers = ToID(uo.terrainLayers);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TerrainData uo = (TerrainData)obj;
            uo.wavingGrassStrength = wavingGrassStrength;
            uo.wavingGrassAmount = wavingGrassAmount;
            uo.wavingGrassSpeed = wavingGrassSpeed;
            uo.wavingGrassTint = wavingGrassTint;
            uo.alphamapResolution = alphamapResolution;
            uo.baseMapResolution = baseMapResolution;
            uo.terrainLayers = FromID(terrainLayers, uo.terrainLayers);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(terrainLayers, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            TerrainData uo = (TerrainData)obj;
            AddDep(uo.terrainLayers, context);
        }
    }
}

