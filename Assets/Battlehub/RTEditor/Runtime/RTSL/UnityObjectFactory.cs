using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using Battlehub.RTSL.Interface;
using Battlehub.RTCommon;

namespace Battlehub.RTSL
{
    public class UnityObjectFactory : IUnityObjectFactory
    {
        private ITypeMap m_typeMap;
        public UnityObjectFactory()
        {
            m_typeMap = IOC.Resolve<ITypeMap>();
        }

        public bool CanCreateInstance(Type type)
        {
            Type persistentType = m_typeMap.ToPersistentType(type);
            IPersistentSurrogate surrogate = null;
            if(persistentType != null)
            {
                surrogate = (IPersistentSurrogate)Activator.CreateInstance(persistentType);
            }
            return CanCreateInstance(type, surrogate);
        }

        public bool CanCreateInstance(Type type, IPersistentSurrogate surrogate)
        {
            return type == typeof(Material) ||
                type == typeof(Texture2D) ||
                type == typeof(Mesh) ||
                type == typeof(PhysicMaterial) ||
                type.IsSubclassOf(typeof(ScriptableObject)) ||
                type == typeof(GameObject) ||
                surrogate != null && surrogate.CanInstantiate(type);
        }

        public UnityObject CreateInstance(Type type)
        {
            Type persistentType = m_typeMap.ToPersistentType(type);
            IPersistentSurrogate surrogate = null;
            if (persistentType != null)
            {
                surrogate = (IPersistentSurrogate)Activator.CreateInstance(persistentType);
            }
            return CreateInstance(type, surrogate);
        }

        public UnityObject CreateInstance(Type type, IPersistentSurrogate surrogate)
        {
            if(type == null)
            {
                Debug.LogError("type is null");
                return null;
            }

            if (type == typeof(Material))
            {
                Material material = new Material(RenderPipelineInfo.DefaultMaterial);
                return material;
            }
            else if (type == typeof(Texture2D))
            {
                if (surrogate != null && surrogate.CanInstantiate(typeof(Texture2D)))
                {
                    return (UnityObject)surrogate.Instantiate(type);
                }

                Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
                return texture;
            }
            else if(type == typeof(Shader))
            {
                Debug.LogWarning("Unable to instantiate Shader");
                return null;
            }
            else if(type.IsSubclassOf(typeof(ScriptableObject)))
            {
                ScriptableObject obj = ScriptableObject.CreateInstance(type);
                obj.name = type.Name;
                return obj;
            }
               
            try
            {
                if (surrogate != null)
                {
                    return (UnityObject)surrogate.Instantiate(type);
                }

                return (UnityObject)Activator.CreateInstance(type);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                Debug.LogWarning("Collecting scene dependencies could fix this exeption. Tools->Runtime Save Load->Collect Scene Dependencies"); 
                return null;
            }
            
        }
    }

}

