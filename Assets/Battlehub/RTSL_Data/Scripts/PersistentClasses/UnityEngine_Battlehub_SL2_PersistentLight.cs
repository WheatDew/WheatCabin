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
    public partial class PersistentLight<TID> : PersistentBehaviour<TID>
    {
        [ProtoMember(256)]
        public LightShadows shadows;

        [ProtoMember(257)]
        public float shadowStrength;

        [ProtoMember(258)]
        public LightShadowResolution shadowResolution;

        [ProtoMember(261)]
        public float[] layerShadowCullDistances;

        [ProtoMember(262)]
        public float cookieSize;

        [ProtoMember(263)]
        public TID cookie;

        [ProtoMember(264)]
        public LightRenderMode renderMode;

        [ProtoMember(271)]
        public LightType type;

        [ProtoMember(272)]
        public float spotAngle;

        [ProtoMember(273)]
        public PersistentColor<TID> color;

        [ProtoMember(274)]
        public float colorTemperature;

        [ProtoMember(275)]
        public float intensity;

        [ProtoMember(276)]
        public float bounceIntensity;

        [ProtoMember(277)]
        public int shadowCustomResolution;

        [ProtoMember(278)]
        public float shadowBias;

        [ProtoMember(279)]
        public float shadowNormalBias;

        [ProtoMember(280)]
        public float shadowNearPlane;

        [ProtoMember(281)]
        public float range;

        [ProtoMember(282)]
        public TID flare;

        [ProtoMember(283)]
        public PersistentLightBakingOutput<TID> bakingOutput;

        [ProtoMember(284)]
        public int cullingMask;

        [ProtoMember(285)]
        public LightShadowCasterMode lightShadowCasterMode;

        protected override void ReadFromImpl(object obj)
        {
            base.ReadFromImpl(obj);
            Light uo = (Light)obj;
            shadows = uo.shadows;
            shadowStrength = uo.shadowStrength;
            shadowResolution = uo.shadowResolution;
            layerShadowCullDistances = uo.layerShadowCullDistances;
            cookieSize = uo.cookieSize;
            cookie = ToID(uo.cookie);
            renderMode = uo.renderMode;
            type = uo.type;
            spotAngle = uo.spotAngle;
            color = uo.color;
            colorTemperature = uo.colorTemperature;
            intensity = uo.intensity;
            bounceIntensity = uo.bounceIntensity;
            shadowCustomResolution = uo.shadowCustomResolution;
            shadowBias = uo.shadowBias;
            shadowNormalBias = uo.shadowNormalBias;
            shadowNearPlane = uo.shadowNearPlane;
            range = uo.range;
            flare = ToID(uo.flare);
            bakingOutput = uo.bakingOutput;
            cullingMask = uo.cullingMask;
            lightShadowCasterMode = uo.lightShadowCasterMode;
        }

        protected override object WriteToImpl(object obj)
        {
            obj = base.WriteToImpl(obj);
            Light uo = (Light)obj;
            uo.shadows = shadows;
            uo.shadowStrength = shadowStrength;
            uo.shadowResolution = shadowResolution;
            uo.layerShadowCullDistances = layerShadowCullDistances;
            uo.cookieSize = cookieSize;
            uo.cookie = FromID(cookie, uo.cookie);
            uo.renderMode = renderMode;
            uo.type = type;
            uo.spotAngle = spotAngle;
            uo.color = color;
            uo.colorTemperature = colorTemperature;
            uo.intensity = intensity;
            uo.bounceIntensity = bounceIntensity;
            uo.shadowCustomResolution = shadowCustomResolution;
            uo.shadowBias = shadowBias;
            uo.shadowNormalBias = shadowNormalBias;
            uo.shadowNearPlane = shadowNearPlane;
            uo.range = range;
            uo.flare = FromID(flare, uo.flare);
            uo.bakingOutput = bakingOutput;
            uo.cullingMask = cullingMask;
            uo.lightShadowCasterMode = lightShadowCasterMode;
            return uo;
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            AddDep(cookie, context);
            AddDep(flare, context);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            Light uo = (Light)obj;
            AddDep(uo.cookie, context);
            AddDep(uo.flare, context);
        }
    }
}

