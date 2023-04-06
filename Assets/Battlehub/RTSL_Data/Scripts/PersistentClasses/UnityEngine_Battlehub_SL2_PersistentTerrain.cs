using System.Collections.Generic;
using ProtoBuf;
using Battlehub.RTSL;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace UnityEngine.Battlehub.SL2
{
    [ProtoContract]
    public partial class PersistentTerrain<TID> : PersistentBehaviour<TID>
    {
        [ProtoMember(256)]
        public TID terrainData;

        [ProtoMember(257)]
        public float treeDistance;

        [ProtoMember(258)]
        public float treeBillboardDistance;

        [ProtoMember(259)]
        public float treeCrossFadeLength;

        [ProtoMember(260)]
        public int treeMaximumFullLODCount;

        [ProtoMember(261)]
        public float detailObjectDistance;

        [ProtoMember(262)]
        public float detailObjectDensity;

        [ProtoMember(263)]
        public float heightmapPixelError;

        [ProtoMember(264)]
        public int heightmapMaximumLOD;

        [ProtoMember(265)]
        public float basemapDistance;

        [ProtoMember(267)]
        public int lightmapIndex;

        [ProtoMember(268)]
        public int realtimeLightmapIndex;

        [ProtoMember(269)]
        public PersistentVector4<TID> lightmapScaleOffset;

        [ProtoMember(270)]
        public PersistentVector4<TID> realtimeLightmapScaleOffset;

        [ProtoMember(273)]
        public ReflectionProbeUsage reflectionProbeUsage;

        [ProtoMember(275)]
        public TID materialTemplate;

        [ProtoMember(278)]
        public bool drawHeightmap;

        [ProtoMember(279)]
        public bool drawTreesAndFoliage;

        [ProtoMember(280)]
        public PersistentVector3<TID> patchBoundsMultiplier;

        [ProtoMember(281)]
        public float treeLODBiasMultiplier;

        [ProtoMember(282)]
        public bool collectDetailPatches;

        [ProtoMember(283)]
        public TerrainRenderFlags editorRenderFlags;

        [ProtoMember(285)]
        public bool preserveTreePrototypeLayers;

        [ProtoMember(286)]
        public bool allowAutoConnect;

        [ProtoMember(287)]
        public int groupingID;

        [ProtoMember(288)]
        public bool drawInstanced;

        [ProtoMember(290)]
        public ShadowCastingMode shadowCastingMode;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Terrain uo = (Terrain)obj;
            terrainData = ToID(uo.terrainData);
            treeDistance = uo.treeDistance;
            treeBillboardDistance = uo.treeBillboardDistance;
            treeCrossFadeLength = uo.treeCrossFadeLength;
            treeMaximumFullLODCount = uo.treeMaximumFullLODCount;
            detailObjectDistance = uo.detailObjectDistance;
            detailObjectDensity = uo.detailObjectDensity;
            heightmapPixelError = uo.heightmapPixelError;
            heightmapMaximumLOD = uo.heightmapMaximumLOD;
            basemapDistance = uo.basemapDistance;
            lightmapIndex = uo.lightmapIndex;
            realtimeLightmapIndex = uo.realtimeLightmapIndex;
            lightmapScaleOffset = uo.lightmapScaleOffset;
            realtimeLightmapScaleOffset = uo.realtimeLightmapScaleOffset;
            reflectionProbeUsage = uo.reflectionProbeUsage;
            materialTemplate = ToID(uo.materialTemplate);
            drawHeightmap = uo.drawHeightmap;
            drawTreesAndFoliage = uo.drawTreesAndFoliage;
            patchBoundsMultiplier = uo.patchBoundsMultiplier;
            treeLODBiasMultiplier = uo.treeLODBiasMultiplier;
            collectDetailPatches = uo.collectDetailPatches;
            editorRenderFlags = uo.editorRenderFlags;
            preserveTreePrototypeLayers = uo.preserveTreePrototypeLayers;
            allowAutoConnect = uo.allowAutoConnect;
            groupingID = uo.groupingID;
            drawInstanced = uo.drawInstanced;
            shadowCastingMode = uo.shadowCastingMode;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Terrain uo = (Terrain)obj;
            uo.terrainData = FromID(terrainData, uo.terrainData);
            uo.treeDistance = treeDistance;
            uo.treeBillboardDistance = treeBillboardDistance;
            uo.treeCrossFadeLength = treeCrossFadeLength;
            uo.treeMaximumFullLODCount = treeMaximumFullLODCount;
            uo.detailObjectDistance = detailObjectDistance;
            uo.detailObjectDensity = detailObjectDensity;
            uo.heightmapPixelError = heightmapPixelError;
            uo.heightmapMaximumLOD = heightmapMaximumLOD;
            uo.basemapDistance = basemapDistance;
            uo.lightmapIndex = lightmapIndex;
            uo.realtimeLightmapIndex = realtimeLightmapIndex;
            uo.lightmapScaleOffset = lightmapScaleOffset;
            uo.realtimeLightmapScaleOffset = realtimeLightmapScaleOffset;
            uo.reflectionProbeUsage = reflectionProbeUsage;
            uo.materialTemplate = FromID(materialTemplate, uo.materialTemplate);
            uo.drawHeightmap = drawHeightmap;
            uo.drawTreesAndFoliage = drawTreesAndFoliage;
            uo.patchBoundsMultiplier = patchBoundsMultiplier;
            uo.treeLODBiasMultiplier = treeLODBiasMultiplier;
            uo.collectDetailPatches = collectDetailPatches;
            uo.editorRenderFlags = editorRenderFlags;
            uo.preserveTreePrototypeLayers = preserveTreePrototypeLayers;
            uo.allowAutoConnect = allowAutoConnect;
            uo.groupingID = groupingID;
            uo.drawInstanced = drawInstanced;
            uo.shadowCastingMode = shadowCastingMode;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(terrainData, context);
            AddDep(materialTemplate, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Terrain uo = (Terrain)obj;
            AddDep(uo.terrainData, context);
            AddDep(uo.materialTemplate, context);
        }
    }
}

