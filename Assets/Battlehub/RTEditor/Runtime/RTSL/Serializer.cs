using ProtoBuf.Meta;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events.Battlehub.SL2;
using UnityEngine.Battlehub.SL2;
using Battlehub.RTSL.Battlehub.SL2;
using Battlehub.RTSL.Interface;
using Battlehub.RTCommon;
using System.Reflection;
using System.Linq;

namespace Battlehub.RTSL
{
    public static class RuntimeTypeModelExtensions
    {
        public static void Add(this RuntimeTypeModel model, Type genericType, Type[] genericTypeParameters, bool applyDefaultBehaviour)
        {
            for (int i = 0; i < genericTypeParameters.Length; ++i)
            {
                model.Add(genericType.MakeGenericType(genericTypeParameters[i]), applyDefaultBehaviour);
            }
        }

        public static void AddSubType(this MetaType metaType, int startingFieldNumber, Type genericType, Type[] genericTypeParameters)
        {
            for (int i = 0; i < genericTypeParameters.Length; ++i)
            {
                metaType.AddSubType(startingFieldNumber + i, genericType.MakeGenericType(genericTypeParameters[i]));
            }
        }

        public static void SetSurrogate(this MetaType metaType, Type genericType, Type[] genericTypeParameters)
        {
            for (int i = 0; i < genericTypeParameters.Length; ++i)
            {
                metaType.SetSurrogate(genericType.MakeGenericType(genericTypeParameters[i]));
            }
        }

        public static void AddSubtype(this RuntimeTypeModel model, int fieldNumber, Type genericType, Type genericSubType, Type[] genericTypeParameters)
        {
            for (int i = 0; i < genericTypeParameters.Length; ++i)
            {
                model[genericType.MakeGenericType(genericTypeParameters[i])].AddSubType(fieldNumber, genericSubType.MakeGenericType(genericTypeParameters[i]));
            }
        }
    }

    [ProtoBuf.ProtoContract]
    public class NilContainer { }

    public class ProtobufSerializer : ISerializer
    {
        private static TypeModel model;

        private static TypeModel GetTypeModel()
        {
            Assembly typeModelAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm.FullName.Contains("RTSLTypeModel")).FirstOrDefault();
            Type type = null;
            if (typeModelAssembly != null)
            {
                type = typeModelAssembly.GetTypes().Where(t => t.Name.Contains("RTSLTypeModel")).FirstOrDefault();
            }

            return type != null ? Activator.CreateInstance(type) as TypeModel : null;
        }

        static ProtobufSerializer()
        {
#if !UNITY_EDITOR
            model = GetTypeModel();
            if (model == null)
            {
                UnityEngine.Debug.LogError("RTSLTypeModel.dll was not found. Please build type model using the Build All button available through the Tools->Runtime SaveLoad->Config menu item in Unity Editor.");
            }
#endif
            if (model == null)
            {
                model = CreateTypeModel();
            }

            model.DynamicTypeFormatting += (sender, args) =>
            {
                if (args.FormattedName == null)
                {
                    return;
                }

                if (Type.GetType(args.FormattedName) == null)
                {
                    args.Type = typeof(NilContainer);
                }
            };

#if UNITY_EDITOR

            if (GetTypeModel() == null)
            {
                Debug.LogWarning("RTSLTypeModel.dll was not found. Please build type model using the Build All button available through the Tools->Runtime SaveLoad->Config menu item");
            }
            
            RuntimeTypeModel runtimeTypeModel = model as RuntimeTypeModel;
            if (runtimeTypeModel != null)
            {
                runtimeTypeModel.CompileInPlace();
            }
#endif
        }

        public TData DeepClone<TData>(TData data)
        {
            return (TData)model.DeepClone(data);
        }

        public TData Deserialize<TData>(Stream stream)
        {
            return (TData) model.Deserialize(stream, null, typeof(TData));
        }

        public object Deserialize(byte[] b, Type type)
        {
            using (var stream = new MemoryStream(b))
            {
                return model.Deserialize(stream, null, type);
            }
        }

        public object Deserialize(Stream stream, Type type, long length = -1)
        {
            if (length <= 0)
            {
                return model.Deserialize(stream, null, type);
            }
            return model.Deserialize(stream, null, type, (int)length);
        }

        public TData Deserialize<TData>(byte[] b)
        {
            using (var stream = new MemoryStream(b))
            {
                TData deserialized = (TData)model.Deserialize(stream, null, typeof(TData));
                return deserialized;
            }
        }

        public TData Deserialize<TData>(byte[] b, TData obj)
        {
            using (var stream = new MemoryStream(b))
            {
                return (TData)model.Deserialize(stream, obj, typeof(TData));
            }
        }

        public void Serialize<TData>(TData data, Stream stream)
        {
            model.Serialize(stream, data);
        }

        public byte[] Serialize<TData>(TData data)
        {
            using (var stream = new MemoryStream())
            {
                model.Serialize(stream, data);
                stream.Flush();
                stream.Position = 0;
                return stream.ToArray();
            }
        }

        public static RuntimeTypeModel CreateTypeModel()
        {
            RuntimeTypeModel model = TypeModel.Create();

            Type[] idTypes = RTSLSettings.IDTypes;

            model.Add(typeof(IntArray), true);
            model.Add(typeof(ProjectItem), true)
                .AddSubType(1, typeof(LegacyAssetItem))
                .AddSubType(1025, typeof(LegacyAssetItem)) //compatibility with v2.26 save files 
                .AddSubType(1027, typeof(AssetItem<>), idTypes);
            model.Add(typeof(LegacyAssetItem), true);
            model.Add(typeof(AssetItem<>), idTypes, true);
            model.Add(typeof(AssetBundleItemInfo), true);
            model.Add(typeof(AssetBundleInfo), true);
            model.Add(typeof(ProjectInfo), true);
            model.Add(typeof(PrefabPart), true);
            model.Add(typeof(Preview), true);
            model.Add(typeof(Preview<>), idTypes, true);
            model.Add(typeof(PersistentDescriptor<>), idTypes, true);
            model.Add(typeof(PersistentPersistentCall<>), idTypes, true);
            model.Add(typeof(PersistentArgumentCache<>), idTypes, true);
            model.Add(typeof(PersistentBlendShapeFrame<>), idTypes, true);

            for (int i = 0; i < idTypes.Length; ++i)
            {
                Type idType = idTypes[i];
                if(idType.GetCustomAttribute<ProtoBuf.ProtoContractAttribute>() != null)
                {
                    model.Add(idType, true);
                }
            }
         
            ITypeModelCreator typeModelCreator = IOC.Resolve<ITypeModelCreator>();
            if (typeModelCreator == null)
            {
                DefaultTypeModel(model);
            }
            else
            {
                typeModelCreator.Create(model);
            }

            MetaType primitiveContract = model.Add(typeof(PrimitiveContract), false);
            int startingFieldNumber = 16;

            //NOTE: Items should be added to TypeModel in exactly the same order!!!
            //It is allowed to append new types, but not to insert new types in the middle.
            Dictionary<int, Type> types = new Dictionary<int, Type> {
                { 1, typeof(bool) }, 
                { 3, typeof(char) }, 
                { 5, typeof(byte) }, 
                { 7, typeof(short) },
                { 9,  typeof(int) }, 
                { 11,  typeof(long) }, 
                { 13,  typeof(ushort) }, 
                { 15,  typeof(uint) }, 
                { 17, typeof(ulong) }, 
                { 19, typeof(string) },
                { 21, typeof(float) }, 
                { 23, typeof(double) }, 
                { 25, typeof(decimal) }, 
                { 27, typeof(PersistentColor<long>) }, 
                { 29, typeof(PersistentVector4<long>) }, 
                { 31, typeof(Guid) },
                { 33, typeof(PersistentColor<Guid>) }, 
                { 35, typeof(PersistentVector4<Guid>) } 
            };

            int customTypesOffset = 1025;
            for(int i = 0; i < idTypes.Length; i++)
            {
                Type idType = idTypes[i];
                if(idType == typeof(Guid) || idType == typeof(int))
                {
                    continue;
                }
                types.Add(customTypesOffset, idType);
                customTypesOffset += 2;
                types.Add(customTypesOffset, typeof(PersistentColor<>).MakeGenericType(idType));
                customTypesOffset += 2;
                types.Add(customTypesOffset, typeof(PersistentVector4<>).MakeGenericType(idType));
                customTypesOffset += 2;
            }

            //NOTE: PrimitiveContract used by PersistentMaterial to store material properties.
            foreach (KeyValuePair<int, Type> kvp in types)
            {
                Type type = kvp.Value;
                int offset = kvp.Key;

                Type derivedType = typeof(PrimitiveContract<>).MakeGenericType(type.MakeArrayType());
                primitiveContract.AddSubType(startingFieldNumber + offset - 1, derivedType);
                model.Add(derivedType, true);

                derivedType = typeof(PrimitiveContract<>).MakeGenericType(type);
                primitiveContract.AddSubType(startingFieldNumber + offset, derivedType);
                model.Add(derivedType, true);

                model.Add(typeof(List<>).MakeGenericType(type), true);
            }

            //This is special kind of peristent object which can be used to serialize types using reflection. (This is required to serialize objects of type created at runtime for example)
            model.Add(typeof(PersistentRuntimeSerializableObject<>), idTypes, true);
            for (int i = 0; i < idTypes.Length; ++i)
            {
                model[typeof(PersistentObject<>).MakeGenericType(idTypes[i])].AddSubType(1024, typeof(PersistentRuntimeSerializableObject<>).MakeGenericType(idTypes[i])); 
            }

            model.AutoAddMissingTypes = false;
            return model;
        }

        private static void DefaultTypeModel(RuntimeTypeModel model)
        {
            Type[] idTypes = RTSLSettings.IDTypes;
            DefaultTypeModel(model, idTypes);

            model.Add(typeof(PersistentColor<>), idTypes, true);
            model.Add(typeof(Color), false).SetSurrogate(typeof(PersistentColor<>), idTypes);
            model.Add(typeof(PersistentVector3<>), idTypes, true);
            model.Add(typeof(Vector3), false).SetSurrogate(typeof(PersistentVector3<>), idTypes);
            model.Add(typeof(PersistentVector4<>), idTypes, true);
            model.Add(typeof(Vector4), false).SetSurrogate(typeof(PersistentVector4<>), idTypes);
            model.Add(typeof(PersistentQuaternion<>), idTypes, true);
            model.Add(typeof(Quaternion), false).SetSurrogate(typeof(PersistentQuaternion<>), idTypes);
        }

        private static void DefaultTypeModel(RuntimeTypeModel model, Type[] idTypes)
        {
            model.Add(typeof(PersistentRuntimeScene<>), idTypes,  true);
            model.Add(typeof(PersistentRuntimePrefab<>), idTypes, true);
            model.AddSubtype(1025, typeof(PersistentRuntimePrefab<>), typeof(PersistentRuntimeScene<>), idTypes);

            model.Add(typeof(PersistentGameObject<>), idTypes, true);
            model.Add(typeof(PersistentTransform<>), idTypes, true);
            model.Add(typeof(PersistentObject<>), idTypes, true);

            model.AddSubtype(1025, typeof(PersistentObject<>), typeof(PersistentGameObject<>), idTypes);
            model.AddSubtype(1029, typeof(PersistentObject<>), typeof(PersistentComponent<>), idTypes);
            model.AddSubtype(1045, typeof(PersistentObject<>), typeof(PersistentRuntimePrefab<>), idTypes);
       
            model.Add(typeof(PersistentComponent<>), idTypes, true);
            model.AddSubtype(1026, typeof(PersistentComponent<>), typeof(PersistentTransform<>), idTypes);
                
            model.Add(typeof(PersistentUnityEvent<>), idTypes, true);
            model.Add(typeof(PersistentUnityEventBase<>), idTypes, true);
            model.AddSubtype(1025, typeof(PersistentUnityEventBase<>), typeof(PersistentUnityEvent<>), idTypes);
        }
    }
}
