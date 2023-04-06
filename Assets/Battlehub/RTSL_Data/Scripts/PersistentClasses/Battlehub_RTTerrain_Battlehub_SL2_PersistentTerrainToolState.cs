using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using Battlehub.RTTerrain;
using Battlehub.RTTerrain.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTTerrain.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentTerrainToolState<TID> : PersistentMonoBehaviour<TID>
    {
        [ProtoMember(257)]
        public float Height;

        [ProtoMember(258)]
        public float ZSize;

        [ProtoMember(259)]
        public float XSize;

        [ProtoMember(261)]
        public float ZSpacing;

        [ProtoMember(262)]
        public float XSpacing;

        [ProtoMember(264)]
        public float[] Grid;

        [ProtoMember(265)]
        public float[] HeightMap;

        [ProtoMember(266)]
        public TID CutoutTexture;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            TerrainToolState uo = (TerrainToolState)obj;
            Height = uo.Height;
            ZSize = uo.ZSize;
            XSize = uo.XSize;
            ZSpacing = uo.ZSpacing;
            XSpacing = uo.XSpacing;
            Grid = uo.Grid;
            HeightMap = uo.HeightMap;
            CutoutTexture = ToID(uo.CutoutTexture);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            TerrainToolState uo = (TerrainToolState)obj;
            uo.Height = Height;
            uo.ZSize = ZSize;
            uo.XSize = XSize;
            uo.ZSpacing = ZSpacing;
            uo.XSpacing = XSpacing;
            uo.Grid = Grid;
            uo.HeightMap = HeightMap;
            uo.CutoutTexture = FromID(CutoutTexture, uo.CutoutTexture);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(CutoutTexture, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            TerrainToolState uo = (TerrainToolState)obj;
            AddDep(uo.CutoutTexture, context);
        }
    }
}

