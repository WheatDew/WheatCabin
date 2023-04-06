//#define RTSL_COMPILE_TEMPLATES
#if RTSL_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using Battlehub.RTCommon;
using Battlehub.Utils;
using ProtoBuf;
using System.Collections.Generic;
//<TEMPLATE_USINGS_END>
#else
using UnityEngine;
#endif

namespace Battlehub.RTSL.Internal
{
    [PersistentTemplate("UnityEngine.Material", 
        new[] { "color", "mainTexture", "mainTextureOffset", "mainTextureScale", "shader" },
        new[] { "UnityEngine.Vector4", "UnityEngine.Color", "UnityEngine.Vector2" })]
    public class PersistentMaterial_RTSL_Template : PersistentSurrogateTemplate
    {
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>

        [ProtoMember(1)]
        public RTShaderPropertyType[] m_propertyTypes;

        [ProtoMember(2)]
        public string[] m_propertyNames;

        [ProtoMember(3)]
        public PrimitiveContract[] m_propertyValues;

        [ProtoMember(4)]
        public string[] m_keywords;

        [ProtoMember(7)]
        public TID shader;

        [ProtoMember(8)]
        public string m_shaderName;

        [ProtoMember(9)]
        public List<Vector2> m_textureOffset;

        [ProtoMember(10)]
        public List<Vector2> m_textureScale;

        [ProtoMember(11)]
        public int m_renderQueue;

        public override object WriteTo(object obj)
        {
            obj = base.WriteTo(obj);
            if (obj == null)
            {
                return null;
            }

            Material o = (Material)obj;
            if (m_assetDB.IsMapped(shader))
            {
                o.shader = FromID<Shader>(shader);
            }
            else
            {
                o.shader = Shader.Find(m_shaderName);
            }

            if (m_keywords != null)
            {
                foreach (string keyword in m_keywords)
                {
                    o.EnableKeyword(keyword);
                }
            }

            if (m_propertyNames != null)
            {
                int textureIndex = 0;
                for (int i = 0; i < m_propertyNames.Length; ++i)
                {
                    string name = m_propertyNames[i];
                    RTShaderPropertyType type = m_propertyTypes[i];
                    switch (type)
                    {
                        case RTShaderPropertyType.Color:
                            if (m_propertyValues[i].ValueBase is PersistentColor<TID>)
                            {
                                o.SetColor(name, (PersistentColor<TID>)m_propertyValues[i].ValueBase);
                            }
                            break;
                        case RTShaderPropertyType.Float:
                            if (m_propertyValues[i].ValueBase is float)
                            {
                                o.SetFloat(name, (float)m_propertyValues[i].ValueBase);
                            }
                            break;
                        case RTShaderPropertyType.Range:
                            if (m_propertyValues[i].ValueBase is float)
                            {
                                o.SetFloat(name, (float)m_propertyValues[i].ValueBase);
                            }
                            break;
                        case RTShaderPropertyType.TexEnv:
                            if (m_propertyValues[i].ValueBase is TID)
                            {
                                o.SetTexture(name, FromID<Texture>((TID)m_propertyValues[i].ValueBase));
                                if(m_textureOffset != null)
                                {
                                    o.SetTextureOffset(name, m_textureOffset[textureIndex]);
                                }
                                if(m_textureScale != null)
                                {
                                    o.SetTextureScale(name, m_textureScale[textureIndex]);
                                }
                                textureIndex++;
                            }
                            break;
                        case RTShaderPropertyType.Vector:
                            if (m_propertyValues[i].ValueBase is PersistentVector4<TID>)
                            {
                                o.SetVector(name, (PersistentVector4<TID>)m_propertyValues[i].ValueBase);
                            }
                            break;
                        case RTShaderPropertyType.Unknown:
                            break;
                    }
                }
            }

            IMaterialUtil util = IOC.Resolve<IMaterialUtil>();
            util.SetMaterialKeywords(o);
            o.renderQueue = m_renderQueue;

            return obj;
        }

        public override void GetDeps(GetDepsContext<TID> context)
        {
            base.GetDeps(context);

            AddDep(shader, context);

            if (m_propertyValues != null)
            {
                for (int i = 0; i < m_propertyValues.Length; ++i)
                {
                    RTShaderPropertyType type = m_propertyTypes[i];
                    switch (type)
                    {
                        case RTShaderPropertyType.TexEnv:
                            if (m_propertyValues[i].ValueBase is TID)
                            {
                                AddDep((TID)m_propertyValues[i].ValueBase, context);
                            }
                            break;
                    }
                }
            }
        }

        public override void GetDepsFrom(object obj, GetDepsFromContext context)
        {
            base.GetDepsFrom(obj, context);
            if (obj == null)
            {
                return;
            }

            Material o = (Material)obj;
            AddDep(o.shader, context);

            RuntimeShaderInfo shaderInfo = null;
            IRuntimeShaderUtil shaderUtil = IOC.Resolve<IRuntimeShaderUtil>();
            if (shaderUtil != null)
            {
                shaderInfo = shaderUtil.GetShaderInfo(o.shader);
            }

            if (shaderInfo == null)
            {
                return;
            }

            for (int i = 0; i < shaderInfo.PropertyCount; ++i)
            {
                string name = shaderInfo.PropertyNames[i];
                RTShaderPropertyType type = shaderInfo.PropertyTypes[i];
                switch (type)
                {
                    case RTShaderPropertyType.TexEnv:
                        AddDep(o.GetTexture(name), context);
                        break;
                }
            }
        }

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }

            Material o = (Material)obj;
            if (o.shader == null)
            {
                shader = m_assetDB.NullID;
                m_shaderName = null;
                return;
            }

            shader = ToID(o.shader);
            m_shaderName = o.shader.name;

            RuntimeShaderInfo shaderInfo = null;
            IRuntimeShaderUtil shaderUtil = IOC.Resolve<IRuntimeShaderUtil>();
            if (shaderUtil != null)
            {
                shaderInfo = shaderUtil.GetShaderInfo(o.shader);
            }
            if (shaderInfo == null)
            {
                return;
            }

            m_propertyNames = new string[shaderInfo.PropertyCount];
            m_propertyTypes = new RTShaderPropertyType[shaderInfo.PropertyCount];
            m_propertyValues = new PrimitiveContract[shaderInfo.PropertyCount];
            m_textureOffset = new List<Vector2>();
            m_textureScale = new List<Vector2>();
            for (int i = 0; i < shaderInfo.PropertyCount; ++i)
            {
                string name = shaderInfo.PropertyNames[i];
                RTShaderPropertyType type = shaderInfo.PropertyTypes[i];
                m_propertyNames[i] = name;
                m_propertyTypes[i] = type;
                switch (type)
                {
                    case RTShaderPropertyType.Color:
                        m_propertyValues[i] = PrimitiveContract.Create((PersistentColor<TID>)o.GetColor(name));
                        break;
                    case RTShaderPropertyType.Float:
                        m_propertyValues[i] = PrimitiveContract.Create(o.GetFloat(name));
                        break;
                    case RTShaderPropertyType.Range:
                        m_propertyValues[i] = PrimitiveContract.Create(o.GetFloat(name));
                        break;
                    case RTShaderPropertyType.TexEnv:
                        Texture texture = o.GetTexture(name);
                        if (texture == null)
                        {
                            m_propertyValues[i] = PrimitiveContract.Create(m_assetDB.NullID);
                        }
                        else
                        {
                            m_propertyValues[i] = PrimitiveContract.Create(ToID(texture));
                        }
                        m_textureOffset.Add(o.GetTextureOffset(name));
                        m_textureScale.Add(o.GetTextureScale(name));
                        break;
                    case RTShaderPropertyType.Vector:
                        m_propertyValues[i] = PrimitiveContract.Create((PersistentVector4<TID>)o.GetVector(name));
                        break;
                    case RTShaderPropertyType.Unknown:
                        m_propertyValues[i] = null;
                        break;
                }
            }

            m_keywords = o.shaderKeywords;
            m_renderQueue = o.renderQueue;
        }
        //<TEMPLATE_BODY_END>
#endif
    }
}


