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
    public partial class PersistentRenderer<TID> : PersistentComponent<TID>
    {
        [ProtoMember(261)]
        public bool enabled;

        [ProtoMember(262)]
        public ShadowCastingMode shadowCastingMode;

        [ProtoMember(263)]
        public bool receiveShadows;

        [ProtoMember(264)]
        public MotionVectorGenerationMode motionVectorGenerationMode;

        [ProtoMember(265)]
        public LightProbeUsage lightProbeUsage;

        [ProtoMember(266)]
        public ReflectionProbeUsage reflectionProbeUsage;

        [ProtoMember(267)]
        public uint renderingLayerMask;

        [ProtoMember(268)]
        public string sortingLayerName;

        [ProtoMember(269)]
        public int sortingLayerID;

        [ProtoMember(270)]
        public int sortingOrder;

        [ProtoMember(271)]
        public bool allowOcclusionWhenDynamic;

        [ProtoMember(272)]
        public TID lightProbeProxyVolumeOverride;

        [ProtoMember(273)]
        public TID probeAnchor;

        [ProtoMember(274)]
        public int lightmapIndex;

        [ProtoMember(275)]
        public int realtimeLightmapIndex;

        [ProtoMember(276)]
        public PersistentVector4<TID> lightmapScaleOffset;

        [ProtoMember(277)]
        public PersistentVector4<TID> realtimeLightmapScaleOffset;

        [ProtoMember(281)]
        public TID[] sharedMaterials;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Renderer uo = (Renderer)obj;
            enabled = uo.enabled;
            shadowCastingMode = uo.shadowCastingMode;
            receiveShadows = uo.receiveShadows;
            motionVectorGenerationMode = uo.motionVectorGenerationMode;
            lightProbeUsage = uo.lightProbeUsage;
            reflectionProbeUsage = uo.reflectionProbeUsage;
            renderingLayerMask = uo.renderingLayerMask;
            sortingLayerName = uo.sortingLayerName;
            sortingLayerID = uo.sortingLayerID;
            sortingOrder = uo.sortingOrder;
            allowOcclusionWhenDynamic = uo.allowOcclusionWhenDynamic;
            lightProbeProxyVolumeOverride = ToID(uo.lightProbeProxyVolumeOverride);
            probeAnchor = ToID(uo.probeAnchor);
            lightmapIndex = uo.lightmapIndex;
            realtimeLightmapIndex = uo.realtimeLightmapIndex;
            lightmapScaleOffset = uo.lightmapScaleOffset;
            realtimeLightmapScaleOffset = uo.realtimeLightmapScaleOffset;
            sharedMaterials = ToID(uo.sharedMaterials);
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Renderer uo = (Renderer)obj;
            uo.enabled = enabled;
            uo.shadowCastingMode = shadowCastingMode;
            uo.receiveShadows = receiveShadows;
            uo.motionVectorGenerationMode = motionVectorGenerationMode;
            uo.lightProbeUsage = lightProbeUsage;
            uo.reflectionProbeUsage = reflectionProbeUsage;
            uo.renderingLayerMask = renderingLayerMask;
            uo.sortingLayerName = sortingLayerName;
            uo.sortingLayerID = sortingLayerID;
            uo.sortingOrder = sortingOrder;
            uo.allowOcclusionWhenDynamic = allowOcclusionWhenDynamic;
            uo.lightProbeProxyVolumeOverride = FromID(lightProbeProxyVolumeOverride, uo.lightProbeProxyVolumeOverride);
            uo.probeAnchor = FromID(probeAnchor, uo.probeAnchor);
            uo.lightmapIndex = lightmapIndex;
            uo.realtimeLightmapIndex = realtimeLightmapIndex;
            uo.lightmapScaleOffset = lightmapScaleOffset;
            uo.realtimeLightmapScaleOffset = realtimeLightmapScaleOffset;
            uo.sharedMaterials = FromID(sharedMaterials, uo.sharedMaterials);
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(lightProbeProxyVolumeOverride, context);
            AddDep(probeAnchor, context);
            AddDep(sharedMaterials, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Renderer uo = (Renderer)obj;
            AddDep(uo.lightProbeProxyVolumeOverride, context);
            AddDep(uo.probeAnchor, context);
            AddDep(uo.sharedMaterials, context);
        }
    }
}

