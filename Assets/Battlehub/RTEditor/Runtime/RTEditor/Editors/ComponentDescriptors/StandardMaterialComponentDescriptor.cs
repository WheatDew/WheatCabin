using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

using Battlehub.Utils;
using Battlehub.RTSL;
using Battlehub.RTCommon;
using System;
using System.Linq;

namespace Battlehub.RTEditor
{
    public class StandardMaterialValueConverter 
    {
        public StandardMaterialUtils.BlendMode Mode
        {
            get { return (StandardMaterialUtils.BlendMode)Material.GetFloat("_Mode"); }
            set
            {
                if (Mode != value)
                {
                    Material.SetFloat("_Mode", (float)value);
                    StandardMaterialUtils.SetupMaterialWithBlendMode(Material,Mode);
                    StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
                }
            }
        }

        public float Cutoff
        {
            get { return Material.GetFloat("_Cutoff"); }
            set
            {
                Material.SetFloat("_Cutoff", value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
                StandardMaterialUtils.SetupMaterialWithBlendMode(Material, Mode);
            }
        }

        public Texture MetallicGlossMap
        {
            get { return Material.GetTexture("_MetallicGlossMap"); }
            set
            {
                Material.SetTexture("_MetallicGlossMap", value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
                StandardMaterialUtils.SetupMaterialWithBlendMode(Material, Mode);
            }
        }

        public Texture BumpMap
        {
            get { return Material.GetTexture("_BumpMap"); }
            set
            {
                Material.SetTexture("_BumpMap", value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
                StandardMaterialUtils.SetupMaterialWithBlendMode(Material, Mode);
            }
        }

        public Texture ParallaxMap
        {
            get { return Material.GetTexture("_ParallaxMap"); }
            set
            {
                Material.SetTexture("_ParallaxMap", value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
                StandardMaterialUtils.SetupMaterialWithBlendMode(Material, Mode);
            }
        }

        public Texture OcclusionMap
        {
            get { return Material.GetTexture("_OcclusionMap"); }
            set
            {
                Material.SetTexture("_OcclusionMap", value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
                StandardMaterialUtils.SetupMaterialWithBlendMode(Material, Mode);
            }
        }

        public Texture EmissionMap
        {
            get { return Material.GetTexture("_EmissionMap"); }
            set
            {
                Material.SetTexture("_EmissionMap", value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
                StandardMaterialUtils.SetupMaterialWithBlendMode(Material, Mode);
            }
        }

        public Color EmissionColor
        {
            get { return Material.GetColor("_EmissionColor"); }
            set
            {
                Material.SetColor("_EmissionColor", value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
                StandardMaterialUtils.SetupMaterialWithBlendMode(Material, Mode);
            }
        }

        public Texture DetailMask
        {
            get { return Material.GetTexture("_DetailMask"); }
            set
            {
                Material.SetTexture("_DetailMask", value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
            }
        }

        public Texture DetailAlbedoMap
        {
            get { return Material.GetTexture("_DetailAlbedoMap"); }
            set
            {
                Material.SetTexture("_DetailAlbedoMap", value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
            }
        }

        public Texture DetailNormalMap
        {
            get { return Material.GetTexture("_DetailNormalMap"); }
            set
            {
                Material.SetTexture("_DetailNormalMap", value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
            }
        }

        public StandardMaterialUtils.UVSec UVSecondary
        {
            get { return (StandardMaterialUtils.UVSec)Material.GetFloat("_UVSec"); }
            set
            {
                Material.SetFloat("_UVSec", (float)value);
                StandardMaterialUtils.SetMaterialKeywords(Material, StandardMaterialUtils.m_workflow);
            }
        }

        public Vector2 Tiling
        {
            get { return Material.mainTextureScale; }
            set { Material.mainTextureScale = value; }
        }

        public Vector2 Offset
        {
            get { return Material.mainTextureOffset; }
            set { Material.mainTextureOffset = value; }
        }

      
        public Material Material
        {
            get;
            set;
        }
    }

    [BuiltInDescriptor]
    public class StandardMaterialDescriptor : IMaterialDescriptor
    {
        const string _Mode = "_Mode";
        const string _MainTex = "_MainTex";
        const string _Color = "_Color";
        const string _Cutoff = "_Cutoff";
        const string _MetallicGlossMap = "_MetallicGlossMap";
        const string _Metallic = "_Metallic";
        const string _Glossiness = "_Glossiness";
        const string _GlossMapScale = "_GlossMapScale";
        const string _BumpScale = "_BumpScale";
        const string _Parallax = "_Parallax";
        const string _OcclusionStrength = "_OcclusionStrength";
        const string _DetailNormalMapScale = "_DetailNormalMapScale";

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,       // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }

        public object GetValue(Material[] materials, Func<Material, object> getter)
        {
            if(materials == null || materials.Length == 0 || materials[0] == null)
            {
                return null;
            }

            object val = getter(materials[0]);
            for(int i = 1; i < materials.Length; ++i)
            {
                Material material = materials[i];
                if(material == null)
                {
                    return null;
                }

                object val2 = getter(material);
                if(!Equals(val, val2))
                {
                    return null;
                }
            }

            return val;
        }

        public object GetValue(StandardMaterialValueConverter[] converters, Func<StandardMaterialValueConverter, object> getter)
        {
            if (converters == null || converters.Length == 0 || converters[0] == null)
            {
                return null;
            }

            object val = getter(converters[0]);
            for (int i = 1; i < converters.Length; ++i)
            {
                StandardMaterialValueConverter converter = converters[i];
                if (converter == null)
                {
                    return null;
                }

                object val2 = getter(converter);
                if (!Equals(val, val2))
                {
                    return null;
                }
            }

            return val;
        }

        private object GetBlendMode(Material material)
        {
            return (BlendMode) material.GetFloat(_Mode);
        }

        private object IsMetallicGlossMapSet(Material material)
        {
            Texture texture = material.GetTexture(_MetallicGlossMap);

            return texture != null;
        }

        public string ShaderName
        {
            get { return "Standard"; }
        }

        public object CreateConverter(MaterialEditor editor)
        {
            object[] converters = new object[editor.Materials.Length];
            Material[] materials = editor.Materials;
            for (int i = 0; i < materials.Length; ++i)
            {
                Material material = materials[i];
                if (material != null)
                {
                    converters[i] = new StandardMaterialValueConverter
                    {
                        Material = material
                    };
                }
            }
            return converters;
        }

        public void EraseAccessorTarget(object accessorRef, object target)
        {
            if(accessorRef is StandardMaterialValueConverter)
            {
                StandardMaterialValueConverter accessor = (StandardMaterialValueConverter)accessorRef;
                accessor.Material = target as Material;
            }
            else if(accessorRef is MaterialPropertyAccessor)
            {
                MaterialPropertyAccessor accessor = (MaterialPropertyAccessor)accessorRef;
                accessor.Material = target as Material;
            }
        }

        private MaterialPropertyAccessor[] CreateAccessors(MaterialEditor editor, string propertyName)
        {
            Material[] materials = editor.Materials;
            MaterialPropertyAccessor[] accessors = new MaterialPropertyAccessor[materials.Length];
            for(int i = 0; i < materials.Length; ++i)
            {
                accessors[i] = new MaterialPropertyAccessor(materials[i], propertyName);
            }
            return accessors;
        }

        public MaterialPropertyDescriptor[] GetProperties(MaterialEditor editor, object converterObject)
        {
            ILocalization lc = IOC.Resolve<ILocalization>();

            PropertyEditorCallback valueChangedCallback = () => editor.BuildEditor();

            StandardMaterialValueConverter[] converters = ((object[])converterObject).Cast<StandardMaterialValueConverter>().ToArray();

            PropertyInfo modeInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.Mode, "Mode");
            PropertyInfo cutoffInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.Cutoff, "Cutoff");
            PropertyInfo metallicMapInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.MetallicGlossMap, "MetallicGlossMap");
            PropertyInfo bumpMapInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.BumpMap, "BumpMap");
            PropertyInfo parallaxMapInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.ParallaxMap, "ParallaxMap");
            PropertyInfo occlusionMapInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.OcclusionMap, "OcclusionMap");
            PropertyInfo emissionMapInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.EmissionMap, "EmissionMap");
            PropertyInfo emissionColorInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.EmissionColor, "EmissionColor");
            PropertyInfo detailMaskInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.DetailMask, "DetailMask");
            PropertyInfo detailAlbedoMap = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.DetailAlbedoMap, "DetailAlbedoMap");
            PropertyInfo detailNormalMap = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.DetailNormalMap, "DetailNormalMap");
            
            PropertyInfo texInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Texture, "Texture");
            PropertyInfo colorInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Color, "Color");
            PropertyInfo floatInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Float, "Float");

            BlendMode? mode = (BlendMode?)GetValue(editor.Materials, GetBlendMode);
            List<MaterialPropertyDescriptor> properties = new List<MaterialPropertyDescriptor>();
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_RenderingMode", "Rendering Mode"), RTShaderPropertyType.Float, modeInfo, new RuntimeShaderInfo.RangeLimits(), TextureDimension.None, valueChangedCallback, EraseAccessorTarget));
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, CreateAccessors(editor, _MainTex), lc.GetString("ID_RTEditor_CD_StandardMaterial_Albedo", "Albedo"), RTShaderPropertyType.TexEnv, texInfo, new RuntimeShaderInfo.RangeLimits(), TextureDimension.Tex2D, null, EraseAccessorTarget));
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, CreateAccessors(editor, _Color), lc.GetString("ID_RTEditor_CD_StandardMaterial_AlbedoColor", "Albedo Color"), RTShaderPropertyType.Color, colorInfo, new RuntimeShaderInfo.RangeLimits(), TextureDimension.None, null, EraseAccessorTarget));
            if (mode == BlendMode.Cutout)
            {
                properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_AlphaCutoff", "Alpha Cutoff"), RTShaderPropertyType.Range, cutoffInfo, new RuntimeShaderInfo.RangeLimits(0.5f, 0.0f, 1.0f), TextureDimension.None, null, EraseAccessorTarget));
            }

            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_Metallic", "Metallic"), RTShaderPropertyType.TexEnv, metallicMapInfo, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 0.0f), TextureDimension.Tex2D, valueChangedCallback, EraseAccessorTarget));
            bool? hasGlossMap = (bool ?)GetValue(editor.Materials, IsMetallicGlossMapSet);
            if(hasGlossMap != null)
            {
                if (!hasGlossMap.Value)
                {
                    properties.Add(new MaterialPropertyDescriptor(editor.Materials, CreateAccessors(editor, _Metallic), lc.GetString("ID_RTEditor_CD_StandardMaterial_Metallic", "Metallic"), RTShaderPropertyType.Range, floatInfo, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 1.0f), TextureDimension.None, null, EraseAccessorTarget));
                    var smoothness = (StandardMaterialUtils.SmoothnessMapChannel?)GetValue(editor.Materials, material => StandardMaterialUtils.GetSmoothnessMapChannel(material));
                    if(smoothness != null)
                    {
                        if (smoothness.Value == StandardMaterialUtils.SmoothnessMapChannel.SpecularMetallicAlpha)
                        {
                            properties.Add(new MaterialPropertyDescriptor(editor.Materials, CreateAccessors(editor, _Glossiness), lc.GetString("ID_RTEditor_CD_StandardMaterial_Smoothness", "Smoothness"), RTShaderPropertyType.Range, floatInfo, new RuntimeShaderInfo.RangeLimits(0.5f, 0.0f, 1.0f), TextureDimension.None, null, EraseAccessorTarget));
                        }
                        else
                        {
                            properties.Add(new MaterialPropertyDescriptor(editor.Materials, CreateAccessors(editor, _GlossMapScale), lc.GetString("ID_RTEditor_CD_StandardMaterial_Smoothness", "Smoothness"), RTShaderPropertyType.Range, floatInfo, new RuntimeShaderInfo.RangeLimits(1.0f, 0.0f, 1.0f), TextureDimension.None, null, EraseAccessorTarget));
                        }
                    }
                }
                else
                {
                    properties.Add(new MaterialPropertyDescriptor(editor.Materials, CreateAccessors(editor, _GlossMapScale), lc.GetString("ID_RTEditor_CD_StandardMaterial_Smoothness", "Smoothness"), RTShaderPropertyType.Range, floatInfo, new RuntimeShaderInfo.RangeLimits(1.0f, 0.0f, 1.0f), TextureDimension.None, null, EraseAccessorTarget));
                }
            }
            
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_NormalMap", "Normal Map"), RTShaderPropertyType.TexEnv, bumpMapInfo, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 0.0f), TextureDimension.Tex2D, valueChangedCallback, EraseAccessorTarget));

            bool? hasBumpMap = (bool?)GetValue(converters, conv => conv.BumpMap != null);
            if (hasBumpMap != null && hasBumpMap.Value)
            {
                properties.Add(new MaterialPropertyDescriptor(editor.Materials, CreateAccessors(editor, _BumpScale), lc.GetString("ID_RTEditor_CD_StandardMaterial_NormalMapScale", "Normal Map Scale"), RTShaderPropertyType.Float, floatInfo, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 0.0f), TextureDimension.None, null, EraseAccessorTarget));
            }

            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_HeightMap", "Height Map"), RTShaderPropertyType.TexEnv, parallaxMapInfo, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 0.0f), TextureDimension.Tex2D, valueChangedCallback, EraseAccessorTarget));
            bool? hasParallaxMap = (bool?)GetValue(converters, conv => conv.ParallaxMap != null);
            if (hasParallaxMap != null && hasParallaxMap.Value)
            {
                properties.Add(new MaterialPropertyDescriptor(editor.Materials, CreateAccessors(editor, _Parallax), lc.GetString("ID_RTEditor_CD_StandardMaterial_HeightMapScale", "Height Map Scale"), RTShaderPropertyType.Range, floatInfo, new RuntimeShaderInfo.RangeLimits(0.02f, 0.005f, 0.08f), TextureDimension.None, null, EraseAccessorTarget));
            }

            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_OcclusionMap", "Occlusion Map"), RTShaderPropertyType.TexEnv, occlusionMapInfo, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 0.0f), TextureDimension.Tex2D, valueChangedCallback, EraseAccessorTarget));
            bool? occlusionMap = (bool?)GetValue(converters, conv => conv.OcclusionMap != null);
            if (occlusionMap != null && occlusionMap.Value)
            {
                properties.Add(new MaterialPropertyDescriptor(editor.Materials, CreateAccessors(editor, _OcclusionStrength), lc.GetString("ID_RTEditor_CD_StandardMaterial_OcclusionStrength", "Occlusion Strength"), RTShaderPropertyType.Range, floatInfo, new RuntimeShaderInfo.RangeLimits(1.0f, 0, 1.0f), TextureDimension.None, null, EraseAccessorTarget));
            }

            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_EmissionMap", "Emission Map"), RTShaderPropertyType.TexEnv, emissionMapInfo, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 0.0f), TextureDimension.Tex2D, null, EraseAccessorTarget));
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_EmissionMap", "Emission Color"), RTShaderPropertyType.Color, emissionColorInfo, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 0.0f), TextureDimension.None, null, EraseAccessorTarget));
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_DetailMask", "Detail Mask"), RTShaderPropertyType.TexEnv, detailMaskInfo, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 0.0f), TextureDimension.Tex2D, null, EraseAccessorTarget));
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_DetailAlbedoMap", "Detail Albedo Map"), RTShaderPropertyType.TexEnv, detailAlbedoMap, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 0.0f), TextureDimension.Tex2D, null, EraseAccessorTarget));
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_DetailNormalMap", "Detail Normal Map"), RTShaderPropertyType.TexEnv, detailNormalMap, new RuntimeShaderInfo.RangeLimits(0.0f, 0.0f, 0.0f), TextureDimension.Tex2D, null, EraseAccessorTarget));
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, CreateAccessors(editor, _DetailNormalMapScale), lc.GetString("ID_RTEditor_CD_StandardMaterial_DetailScale", "Detail Scale"), RTShaderPropertyType.Float, floatInfo, new RuntimeShaderInfo.RangeLimits(0, 0, 0), TextureDimension.None, null, EraseAccessorTarget));

            PropertyInfo tilingInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.Tiling, "Tiling");
            PropertyInfo offsetInfo = Strong.PropertyInfo((StandardMaterialValueConverter x) => x.Offset, "Offset");
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_Tiling", "Tiling"), RTShaderPropertyType.Vector, tilingInfo, new RuntimeShaderInfo.RangeLimits(), TextureDimension.None, null, EraseAccessorTarget));
            properties.Add(new MaterialPropertyDescriptor(editor.Materials, converters, lc.GetString("ID_RTEditor_CD_StandardMaterial_Offset", "Offset"), RTShaderPropertyType.Vector, offsetInfo, new RuntimeShaderInfo.RangeLimits(), TextureDimension.None, null, EraseAccessorTarget));

            return properties.ToArray();
        }
    }
}
