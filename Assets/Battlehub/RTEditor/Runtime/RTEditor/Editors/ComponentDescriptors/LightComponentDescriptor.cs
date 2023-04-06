using UnityEngine;
using System.Reflection;
using System;
using Battlehub.Utils;
using System.Collections.Generic;
using Battlehub.RTGizmos;
using Battlehub.RTCommon;
using System.Linq;

namespace Battlehub.RTEditor
{
    [BuiltInDescriptor]
    public class LightComponentDescriptor : ComponentDescriptorBase<Light, LightGizmo>
    {
        private LightType GetLightType(ComponentEditor editor, out bool? hasMixedValues)
        {
            hasMixedValues = null;
            LightType lightType = LightType.Directional;
            Light[] lights = editor.NotNullComponents.OfType<Light>().ToArray();
            if (lights.Length > 0)
            {
                hasMixedValues = false;
                lightType = lights[0].type;
                for (int i = 1; i < lights.Length; ++i)
                {
                    if (lights[i].type != lightType)
                    {
                        hasMixedValues = true;
                        break;
                    }
                }
            }
            return lightType;
        }

        private LightShadows GetLightShadows(ComponentEditor editor, out bool? hasMixedValues)
        {
            hasMixedValues = null;
            LightShadows lightShadows = LightShadows.None;
            Light[] lights = editor.NotNullComponents.OfType<Light>().ToArray();
            if (lights.Length > 0)
            {
                hasMixedValues = false;
                lightShadows = lights[0].shadows;
                for (int i = 1; i < lights.Length; ++i)
                {
                    if (lights[i].shadows != lightShadows)
                    {
                        hasMixedValues = true;
                        break;
                    }
                }
            }
            return lightShadows;
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            PropertyEditorCallback valueChanged = () => editor.BuildEditor();

            MemberInfo enabledInfo = Strong.PropertyInfo((Light x) => x.enabled, "enabled");
            MemberInfo lightTypeInfo = Strong.PropertyInfo((Light x) => x.type, "type");
            MemberInfo colorInfo = Strong.PropertyInfo((Light x) => x.color, "color");
            MemberInfo intensityInfo = Strong.PropertyInfo((Light x) => x.intensity, "intensity");
            MemberInfo bounceIntensityInfo = Strong.PropertyInfo((Light x) => x.bounceIntensity, "bounceIntensity");
            MemberInfo shadowTypeInfo = Strong.PropertyInfo((Light x) => x.shadows, "shadows");
            MemberInfo cookieInfo = Strong.PropertyInfo((Light x) => x.cookie, "cookie");
            MemberInfo cookieSizeInfo = Strong.PropertyInfo((Light x) => x.cookieSize, "cookieSize");
            MemberInfo flareInfo = Strong.PropertyInfo((Light x) => x.flare, "flare");
            MemberInfo renderModeInfo = Strong.PropertyInfo((Light x) => x.renderMode, "renderMode");

            bool? hasMixedLightTypes;
            LightType lightType = GetLightType(editor, out hasMixedLightTypes);

            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Enabled", "Enabled"), editor.Components, enabledInfo, "m_Enabled"));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Type", "Type"), editor.Components, lightTypeInfo, lightTypeInfo, valueChanged));

            if(hasMixedLightTypes == false)
            {
                if (lightType == LightType.Point)
                {
                    MemberInfo rangeInfo = Strong.PropertyInfo((Light x) => x.range, "range");
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Range", "Range"), editor.Components, rangeInfo, "m_Range"));
                }
                else if (lightType == LightType.Spot)
                {
                    MemberInfo rangeInfo = Strong.PropertyInfo((Light x) => x.range, "range");
                    MemberInfo spotAngleInfo = Strong.PropertyInfo((Light x) => x.spotAngle, "spotAngle");
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Range", "Range"), editor.Components, rangeInfo, "m_Range"));
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_SpotAngle", "Spot Angle"), editor.Components, spotAngleInfo, spotAngleInfo, null, new Range(1, 179)) { AnimationPropertyName = "m_SpotAngle" });
                }
            }

            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Color", "Color"), editor.Components, colorInfo, "m_Color"));

            Range lightIntensityRange = RenderPipelineInfo.Type == RPType.HDRP ? new Range(0, 128000) : new Range(0, 8);
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Intensity", "Intensity"), editor.Components, intensityInfo, intensityInfo, null, lightIntensityRange) { AnimationPropertyName = "m_Intensity" });
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_BounceIntensity", "Bounce Intensity"), editor.Components, bounceIntensityInfo, bounceIntensityInfo, null, new Range(0, 8)) { AnimationPropertyName = "m_BounceIntensity" });

            if(hasMixedLightTypes == false)
            {
                if (lightType != LightType.Area)
                {
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_ShadowType", "Shadow Type"), editor.Components, shadowTypeInfo, shadowTypeInfo, valueChanged));

                    bool? hasMixedLightShadows;
                    LightShadows lightShadows = GetLightShadows(editor, out hasMixedLightShadows);

                    if (hasMixedLightShadows == false && (lightShadows == LightShadows.Soft || lightShadows == LightShadows.Hard))
                    {
                        MemberInfo shadowStrengthInfo = Strong.PropertyInfo((Light x) => x.shadowStrength, "shadowStrength");
                        MemberInfo shadowResolutionInfo = Strong.PropertyInfo((Light x) => x.shadowResolution, "shadowResolution");
                        MemberInfo shadowBiasInfo = Strong.PropertyInfo((Light x) => x.shadowBias, "shadowBias");
                        MemberInfo shadowNormalBiasInfo = Strong.PropertyInfo((Light x) => x.shadowNormalBias, "shadowNormalBias");
                        MemberInfo shadowNearPlaneInfo = Strong.PropertyInfo((Light x) => x.shadowNearPlane, "shadowNearPlane");

                        descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Strength", "Strength"), editor.Components, shadowStrengthInfo, shadowStrengthInfo, null, new Range(0, 1)) { AnimationPropertyName = "m_Strength" });
                        descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Resolution", "Resoultion"), editor.Components, shadowResolutionInfo, shadowResolutionInfo));
                        descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Bias", "Bias"), editor.Components, shadowBiasInfo, shadowBiasInfo, null, new Range(0, 2)) { AnimationPropertyName = "m_Bias" });
                        descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_NormalBias", "Normal Bias"), editor.Components, shadowNormalBiasInfo, shadowNormalBiasInfo, null, new Range(0, 3)) { AnimationPropertyName = "m_NormalBias" });
                        descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_ShadowNearPlane", "Shadow Near Plane"), editor.Components, shadowNearPlaneInfo, shadowNearPlaneInfo, null, new Range(0, 10)) { AnimationPropertyName = "m_NearPlane" });
                    }

                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Cookie", "Cookie"), editor.Components, cookieInfo, cookieInfo));
                    descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_CookieSize", "Cookie Size"), editor.Components, cookieSizeInfo, cookieSizeInfo));
                }
            }
           
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_Flare", "Flare"), editor.Components, flareInfo, flareInfo));
            descriptors.Add(new PropertyDescriptor(lc.GetString("ID_RTEditor_CD_Light_RenderMode", "Render Mode"), editor.Components, renderModeInfo, renderModeInfo));

            return descriptors.ToArray();
        }
    }
}

