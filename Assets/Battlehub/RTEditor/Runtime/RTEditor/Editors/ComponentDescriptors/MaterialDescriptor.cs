
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

using Battlehub.RTSL;
using Battlehub.Utils;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public class MaterialPropertyAccessor
    {
        private int m_propertyId;
        private string m_propertyName;
        private Material m_material;

        public Material Material
        {
            get { return m_material; }
            set { m_material = value; }
        }

        public Color Color
        {
            get { return m_material.GetColor(m_propertyId); }
            set { m_material.SetColor(m_propertyId, value); }
        }

        public float Float
        {
            get { return m_material.GetFloat(m_propertyId); }
            set { m_material.SetFloat(m_propertyId, value); }
        }

        public Vector4 Vector
        {
            get { return m_material.GetVector(m_propertyId); }
            set { m_material.SetVector(m_propertyId, value); }
        }

        public Texture Texture
        {
            get { return m_material.GetTexture(m_propertyId); }
            set { m_material.SetTexture(m_propertyId, value); }
        }

        public Texture2D Texture2D
        {
            get { return (Texture2D)m_material.GetTexture(m_propertyId); }
            set { m_material.SetTexture(m_propertyId, value); }
        }

        public Texture3D Texture3D
        {
            get { return (Texture3D)m_material.GetTexture(m_propertyId); }
            set { m_material.SetTexture(m_propertyId, value); }
        }

        public Cubemap Cubemap
        {
            get { return (Cubemap)m_material.GetTexture(m_propertyId); }
            set { m_material.SetTexture(m_propertyId, value); }
        }

        public Texture2DArray Texture2DArray
        {
            get { return (Texture2DArray)m_material.GetTexture(m_propertyId); }
            set { m_material.SetTexture(m_propertyId, value); }
        }

        public Vector2 TextureOffset
        {
            get { return m_material.GetTextureOffset(m_propertyName); }
            set { m_material.SetTextureOffset(m_propertyName, value); }
        }

        public Vector2 TextureScale
        {
            get { return m_material.GetTextureScale(m_propertyName); }
            set { m_material.SetTextureScale(m_propertyName, value); }
        }

        public MaterialPropertyAccessor(Material material, string propertyName)
        {
            m_material = material;
            m_propertyName = propertyName;
            m_propertyId = Shader.PropertyToID(m_propertyName);
        }
    }


    [BuiltInDescriptor]
    public class MaterialDescriptor : IMaterialDescriptor
    {
        public string ShaderName
        {
            get { return "Battlehub.RTEditor.MaterialPropertySelector"; }
        }

        public object CreateConverter(MaterialEditor editor)
        {
            return null;
        }

        public object CreateConverter(Material material)
        {
            return null;
        }

        public virtual MaterialPropertyDescriptor[] GetProperties(MaterialEditor editor, object converter)
        {
            return GetProperties(editor.Materials);
        }

        public static MaterialPropertyDescriptor[] GetProperties(Material[] materials)
        {
            RuntimeShaderInfo shaderInfo = null;
            IRuntimeShaderUtil shaderUtil = IOC.Resolve<IRuntimeShaderUtil>();
            if (shaderUtil != null)
            {
                shaderInfo = shaderUtil.GetShaderInfo(materials[0].shader);
            }

            if (shaderInfo == null)
            {
                return null;
            }

            List<MaterialPropertyDescriptor> descriptors = new List<MaterialPropertyDescriptor>();
            if (shaderInfo != null)
            {
                for (int i = 0; i < shaderInfo.PropertyCount; ++i)
                {
                    bool isHidden = shaderInfo.IsHidden[i];
                    if (isHidden)
                    {
                        continue;
                    }

                    string propertyDescr = shaderInfo.PropertyDescriptions[i];
                    string propertyName = shaderInfo.PropertyNames[i];
                    RTShaderPropertyType propertyType = shaderInfo.PropertyTypes[i];
                    RuntimeShaderInfo.RangeLimits limits = shaderInfo.PropertyRangeLimits[i];
                    TextureDimension dim = shaderInfo.PropertyTexDims[i];
                    PropertyInfo propertyInfo = GetPropertyInfo(propertyType, dim);

                    if (propertyInfo == null)
                    {
                        continue;
                    }

                    MaterialPropertyDescriptor propertyDescriptor = CreatePropertyDescriptor(materials, propertyInfo, propertyDescr, propertyName, propertyType, dim, limits);
                    descriptors.Add(propertyDescriptor);
                }
            }
            return descriptors.ToArray();
        }

        public static PropertyInfo GetPropertyInfo(RTShaderPropertyType propertyType, TextureDimension dim)
        {
            PropertyInfo propertyInfo = null;
            switch (propertyType)
            {
                case RTShaderPropertyType.Color:
                    propertyInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Color, "Color");
                    break;
                case RTShaderPropertyType.Float:
                    propertyInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Float, "Float");
                    break;
                case RTShaderPropertyType.Range:
                    propertyInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Float, "Float");
                    break;
                case RTShaderPropertyType.TexEnv:
                    switch (dim)
                    {
                        case TextureDimension.Any:
                            propertyInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Texture, "Texture");
                            break;
                        case TextureDimension.Cube:
                            propertyInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Cubemap, "Cubemap");
                            break;
                        case TextureDimension.None:
                            propertyInfo = null;
                            break;
                        case TextureDimension.Tex2D:
                            propertyInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Texture2D, "Texture2D");
                            break;
                        case TextureDimension.Tex2DArray:
                            propertyInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Texture2DArray, "Texture2DArray");
                            break;
                        case TextureDimension.Tex3D:
                            propertyInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Texture3D, "Texture3D");
                            break;
                        case TextureDimension.Unknown:
                            propertyInfo = null;
                            break;
                    }

                    break;
                case RTShaderPropertyType.Vector:
                    propertyInfo = Strong.PropertyInfo((MaterialPropertyAccessor x) => x.Vector, "Vector");
                    break;
            }

            return propertyInfo;
        }

        private static MaterialPropertyAccessor[] CreateAccessors(Material[] materials, string propertyName)
        {
            MaterialPropertyAccessor[] accessors = new MaterialPropertyAccessor[materials.Length];
            for (int i = 0; i < materials.Length; ++i)
            {
                accessors[i] = new MaterialPropertyAccessor(materials[i], propertyName);
            }
            return accessors;
        }

        public static MaterialPropertyDescriptor CreatePropertyDescriptor(Material[] materials, PropertyInfo propertyInfo, string propertyDescr, string propertyName, RTShaderPropertyType propertyType)
        {
            return CreatePropertyDescriptor(materials, propertyInfo, propertyDescr, propertyName, propertyType, TextureDimension.Tex2D, new RuntimeShaderInfo.RangeLimits());
        }

        public static MaterialPropertyDescriptor CreatePropertyDescriptor(Material[] materials, PropertyInfo propertyInfo, string propertyDescr, string propertyName, RTShaderPropertyType propertyType, TextureDimension dim, RuntimeShaderInfo.RangeLimits limits)
        {
            return new MaterialPropertyDescriptor(
                materials,
                CreateAccessors(materials, propertyName),
                propertyDescr, propertyType, propertyInfo, limits, dim, null,
                (accessorRef, newTarget) =>
                {
                    MaterialPropertyAccessor accessor = (MaterialPropertyAccessor)accessorRef;
                    accessor.Material = newTarget as Material;
                });
        }
    }

}
