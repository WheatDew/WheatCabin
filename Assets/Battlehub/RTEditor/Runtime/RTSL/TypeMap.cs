using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Battlehub.SL2;
using Battlehub.RTSL.Interface;
using Battlehub.RTSL.Battlehub.SL2;
using Battlehub.RTCommon;

namespace Battlehub.RTSL
{
  
    public partial class TypeMap<TID> : ITypeMap<TID>
    {
        protected readonly Dictionary<Type, Type> m_toPeristentType = new Dictionary<Type, Type>();
        protected readonly Dictionary<Type, Type> m_toUnityType = new Dictionary<Type, Type>();
        protected readonly Dictionary<Type, Guid> m_toGuid = new Dictionary<Type, Guid>();
        protected readonly Dictionary<Guid, Type> m_toType = new Dictionary<Guid, Type>();

        protected readonly Guid m_persistentRuntimeSerializableObjectGuid = new Guid("be030c1e-582e-4ffa-8bc9-5c3a3018b033");

        public TypeMap()
        {
            ITypeMapCreator typeMapCreator = IOC.Resolve<ITypeMapCreator>();
            if(typeMapCreator == null)
            {
                typeMapCreator = new DefaultTypeMapCreator();
            }

            typeMapCreator.Create(this);
            
            m_toPeristentType[typeof(Scene)] = typeof(PersistentRuntimeScene<>);
            m_toUnityType[typeof(PersistentRuntimeScene<>)] = typeof(Scene);
            Guid sceneGuid = new Guid("d144fbe0-d2c0-4bcf-aa9f-251376262202");
            m_toGuid[typeof(Scene)] = sceneGuid;
            m_toType[sceneGuid] = typeof(Scene);

            m_toPeristentType[typeof(RuntimeTextAsset)] = typeof(PersistentRuntimeTextAsset<>);
            m_toUnityType[typeof(PersistentRuntimeTextAsset<>)] = typeof(RuntimeTextAsset);
            m_toGuid[typeof(PersistentRuntimeTextAsset<>)] = new Guid("82b9517d-9b1e-494d-a398-f20a084e7f14");
            m_toGuid[typeof(RuntimeTextAsset)] = new Guid("cbe0eb96-429b-4231-85e4-e25644b6c145");
            m_toType[new Guid("82b9517d-9b1e-494d-a398-f20a084e7f14")] = typeof(PersistentRuntimeTextAsset<>);
            m_toType[new Guid("cbe0eb96-429b-4231-85e4-e25644b6c145")] = typeof(RuntimeTextAsset);

            m_toPeristentType[typeof(RuntimeBinaryAsset)] = typeof(PersistentRuntimeBinaryAsset<>);
            m_toUnityType[typeof(PersistentRuntimeBinaryAsset<>)] = typeof(RuntimeBinaryAsset);
            m_toGuid[typeof(PersistentRuntimeBinaryAsset<>)] = new Guid("22af8a02-307a-4a7f-97e4-139efce7006a");
            m_toGuid[typeof(RuntimeBinaryAsset)] = new Guid("4be06fec-1084-4e90-a179-1bf0e8c0e970");
            m_toType[new Guid("22af8a02-307a-4a7f-97e4-139efce7006a")] = typeof(PersistentRuntimeBinaryAsset<>);
            m_toType[new Guid("4be06fec-1084-4e90-a179-1bf0e8c0e970")] = typeof(RuntimeBinaryAsset);

            m_toType[m_persistentRuntimeSerializableObjectGuid] = typeof(PersistentRuntimeSerializableObject<>);
            m_toGuid[typeof(PersistentRuntimeSerializableObject<>)] = m_persistentRuntimeSerializableObjectGuid;
        }
        
        public Type ToPersistentType(Type unityType)
        {
            Type persistentType;
            if(m_toPeristentType.TryGetValue(unityType, out persistentType))
            {
                if(persistentType.IsGenericTypeDefinition)
                {
                    return persistentType.MakeGenericType(typeof(TID));
                }

                return persistentType;
            }

            return null;
        }

        public Type ToUnityType(Type persistentType)
        {
            if(persistentType.IsGenericType)
            {
                persistentType = persistentType.GetGenericTypeDefinition();
            }

            Type unityType;
            if(m_toUnityType.TryGetValue(persistentType, out unityType))
            {
                return unityType;
            }

            if(persistentType == typeof(PersistentRuntimeSerializableObject<>))
            {
                throw new InvalidOperationException(string.Format("Unable to resolve type. PersistentType: {0}", persistentType.Name));
            }

            return null;
        }

        public Type ToType(Guid typeGuid)
        {
            Type type;
            if (m_toType.TryGetValue(typeGuid, out type))
            {
                if(typeof(IPersistentSurrogate).IsAssignableFrom(type) && type.IsGenericTypeDefinition)
                {
                    return type.MakeGenericType(typeof(TID));
                }

                return type;
            }
            return null;
        }

        public Guid ToGuid(Type type)
        {
            if(typeof(IPersistentSurrogate).IsAssignableFrom(type) && type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            Guid guid;
            if(m_toGuid.TryGetValue(type, out guid))
            {
                return guid;
            }

            return Guid.Empty;
        }

        public void RegisterRuntimeSerializableType(Type type, Guid typeGuid)
        {
            if(m_toPeristentType.ContainsKey(type))
            {
                return;
            }
            m_toPeristentType.Add(type, typeof(PersistentRuntimeSerializableObject<>));
            m_toGuid.Add(type, typeGuid);
            m_toType.Add(typeGuid, type);
        }

        public void UnregisterRuntimeSerialzableType(Type type)
        {
            Guid typeGuid;
            if(m_toGuid.TryGetValue(type, out typeGuid))
            {
                m_toGuid.Remove(type);
                m_toType.Remove(typeGuid);
                m_toPeristentType.Remove(type);
            }
        }

        public void Register(Type type, Type persistentType, Guid unityTypeGuid, Guid persistentTypeGuid)
        {
            m_toPeristentType.Add(type, persistentType);
            m_toUnityType.Add(persistentType, type);
            m_toGuid.Add(persistentType, persistentTypeGuid);
            m_toGuid.Add(type, unityTypeGuid);
            m_toType.Add(persistentTypeGuid, persistentType);
            m_toType.Add(unityTypeGuid, type);
        }

        public void Unregister(Type type, Type persistentType)
        {
            m_toPeristentType.Remove(type);
            m_toUnityType.Remove(persistentType);

            Guid typeGuid;
            if(m_toGuid.TryGetValue(type, out typeGuid))
            {
                m_toType.Remove(typeGuid);
            }
            Guid persistentTypeGuid;
            if(m_toGuid.TryGetValue(persistentType, out persistentTypeGuid))
            {
                m_toType.Remove(persistentTypeGuid);
            }

            m_toGuid.Remove(type);
            m_toGuid.Remove(persistentType);
        }
    }
}


