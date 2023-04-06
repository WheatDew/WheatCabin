using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Battlehub.SL2;
using UnityObject = UnityEngine.Object;
using UnityEngine;
using System.IO;
using System.Collections;

namespace Battlehub.RTSL.Battlehub.SL2
{
    [ProtoContract]
    public class PersistentRuntimeScene<TID> : PersistentRuntimePrefab<TID>, ICustomSerialization
    {
        //For compatibility purposes
        [ProtoMember(1), Obsolete("Use EmbeddedAssets from base class")]
        public PersistentObject<TID>[] Assets;

        //For compatibity with previous versions
        [ProtoMember(2), Obsolete("Use EmbeddedAssetIds from base class")]
        public int[] AssetIdentifiers;

        //For compatibity with previous versions
        [ProtoMember(3), Obsolete("Use EmbeddedAssetIds from base class")]
        public TID[] AssetIds;

        protected override void ReadDescriptorsAndData(object obj, List<Tuple<PersistentObject<TID>, UnityObject>> data, List<TID> identifiers, GetDepsFromContext getDepsCtx)
        {
            Scene scene = (Scene)obj;
            GameObject[] rootGameObjects;
            if (scene.IsValid())
            {
                rootGameObjects = scene.GetRootGameObjects();
            }
            else
            {
                rootGameObjects = new GameObject[0];
            }

            if (RTSLSettings.SaveIncludedObjectsOnly)
            {
                rootGameObjects = rootGameObjects.Where(go => go.GetComponent<RTSLInclude>() != null).ToArray();
            }

            List<PersistentDescriptor<TID>> descriptors = new List<PersistentDescriptor<TID>>(rootGameObjects.Length);
            for (int i = 0; i < rootGameObjects.Length; ++i)
            {
                GameObject rootGO = rootGameObjects[i];
                PersistentDescriptor<TID> descriptor = CreateDescriptorAndData(rootGO, data, identifiers, getDepsCtx);
                if (descriptor != null)
                {
                    descriptors.Add(descriptor);
                }
            }
            //Dependencies = getDepsCtx.Dependencies.OfType<UnityObject>().Where(uo => m_assetDB.IsMapped(uo)).Select(uo => ToID(uo)).ToArray();
            Descriptors = descriptors.ToArray();
            Identifiers = identifiers.ToArray();
        }

        protected override IEnumerator CoWriteTo(object obj)
        {
            Scene scene = (Scene)obj;

            if (Descriptors == null && Data == null)
            {
                DestroyGameObjects(scene);
                yield break;
            }

            if (Descriptors == null && Data != null || Data != null && Descriptors == null)
            {
                throw new ArgumentException("data is corrupted", "scene");
            }

            if (Descriptors.Length == 0)
            {
                DestroyGameObjects(scene);
                yield break;
            }

            if (Identifiers == null || Identifiers.Length != Data.Length)
            {
                throw new ArgumentException("data is corrupted", "scene");
            }

            DestroyGameObjects(scene);

            Dictionary<TID, UnityObject> idToUnityObj = new Dictionary<TID, UnityObject>();
            for (int i = 0; i < Descriptors.Length; ++i)
            {
                PersistentDescriptor<TID> descriptor = Descriptors[i];
                if (descriptor != null)
                {
                    CreateGameObjectWithComponents(m_typeMap, descriptor, idToUnityObj, null);
                }
            }

            yield return CoRestoreDataAndResolveDependencies(idToUnityObj);
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            if (!(obj is Scene))
            {
                return;
            }

            Scene scene = (Scene)obj;
            GameObject[] gameObjects = scene.GetRootGameObjects();

            for (int i = 0; i < gameObjects.Length; ++i)
            {
                base.GetDepsFromImpl(gameObjects[i], context);
            }
        }

        protected override void GetDependenciesFrom(GameObject go, List<object> prefabParts, GetDepsFromContext context)
        {
            if ((go.hideFlags & HideFlags.DontSave) != 0)
            {
                //Do not save persistent ignore objects
                return;
            }
            base.GetDependenciesFrom(go, prefabParts, context);
        }

        protected override PersistentDescriptor<TID> CreateDescriptorAndData(GameObject go, List<Tuple<PersistentObject<TID>, UnityObject>> persistentData, List<TID> persistentIdentifiers, GetDepsFromContext getDepsFromCtx, PersistentDescriptor<TID> parentDescriptor = null)
        {
            if ((go.hideFlags & HideFlags.DontSave) != 0)
            {
                return null;
            }
            return base.CreateDescriptorAndData(go, persistentData, persistentIdentifiers, getDepsFromCtx, parentDescriptor);
        }

        private void DestroyGameObjects(Scene scene)
        {
            if (UnityObject.FindObjectOfType<RTSLAdditive>())
            {
                return;
            }

            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; ++i)
            {
                GameObject rootGO = rootGameObjects[i];
                if (rootGO.GetComponent<RTSLIgnore>() || (rootGO.hideFlags & HideFlags.DontSave) != 0)
                {
                    continue;
                }

                if (RTSLSettings.SaveIncludedObjectsOnly)
                {
                    if (rootGO.GetComponent<RTSLInclude>() == null)
                    {
                        continue;
                    }
                }

                UnityObject.DestroyImmediate(rootGO);
            }
        }


        public bool AllowStandardSerialization
        {
            get { return true; }
        }

        private List<ICustomSerialization> m_customSerializationAssets;
        private List<int> m_customSerializationAssetIndices;

        [ProtoBeforeSerialization]
        public void OnBeforeSerialization()
        {
            if (!RTSLSettings.IsCustomSerializationEnabled)
            {
                return;
            }
            m_customSerializationAssets = new List<ICustomSerialization>();
            m_customSerializationAssetIndices = new List<int>();

            for (int i = 0; i < EmbeddedAssets.Length; ++i)
            {
                ICustomSerialization asset = EmbeddedAssets[i] as ICustomSerialization;
                if (asset != null)
                {
                    m_customSerializationAssets.Add(asset);
                    m_customSerializationAssetIndices.Add(i);
                    if (!asset.AllowStandardSerialization)
                    {
                        EmbeddedAssets[i] = null;
                    }
                }
            }
        }

        public void Serialize(Stream stream, BinaryWriter writer)
        {
            writer.Write(m_customSerializationAssets.Count);
            for (int i = 0; i < m_customSerializationAssets.Count; ++i)
            {
                ICustomSerialization asset = m_customSerializationAssets[i];
                writer.Write(asset.AllowStandardSerialization);
                writer.Write(m_customSerializationAssetIndices[i]);
                writer.Write(m_typeMap.ToGuid(asset.GetType()).ToByteArray());
                asset.Serialize(stream, writer);
            }
            m_customSerializationAssets = null;
            m_customSerializationAssetIndices = null;
        }

        public void Deserialize(Stream stream, BinaryReader reader)
        {
#pragma warning disable CS0618
            if (Assets != null)
            {
                EmbeddedAssets = Assets;
                Assets = null;
            }
#pragma warning restore CS0618

            List<PersistentObject<TID>> assets = EmbeddedAssets != null ? EmbeddedAssets.ToList() : new List<PersistentObject<TID>>();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                bool allowStandardSerialization = reader.ReadBoolean();
                int index = reader.ReadInt32();
                Guid typeGuid = new Guid(reader.ReadBytes(16));
                Type type = m_typeMap.ToType(typeGuid);
                if (type == null)
                {
                    //Type removal is not allowed
                    throw new InvalidOperationException("Unknown type guid " + typeGuid);
                }

                ICustomSerialization customSerializationAsset;
                if (!allowStandardSerialization)
                {
                    PersistentObject<TID> asset = (PersistentObject<TID>)Activator.CreateInstance(type);
                    assets.Insert(index, asset);
                }

                customSerializationAsset = (ICustomSerialization)assets[index];
                customSerializationAsset.Deserialize(stream, reader);
            }

            EmbeddedAssets = assets.ToArray();
        }

        [ProtoAfterDeserialization]
        public void OnDeserialized()
        {
#pragma warning disable CS0618
            if (AssetIdentifiers != null && AssetIds == null)
            {
                AssetIds = new TID[AssetIdentifiers.Length];
                for (int i = 0; i < AssetIdentifiers.Length; ++i)
                {
                    const long m_instanceIDMask = 1L << 33;

                    AssetIds[i] = Cast(m_instanceIDMask | (0x00000000FFFFFFFFL & AssetIdentifiers[i]));
                }
                AssetIdentifiers = null;
            }

            if (AssetIds != null && EmbeddedAssetIds == null)
            {
                EmbeddedAssetIds = AssetIds;
                AssetIds = null;
            }

            if (Assets != null && EmbeddedAssets == null)
            {
                EmbeddedAssets = Assets;
                Assets = null;
            }
#pragma warning restore CS0618
        }

        private TID Cast(object value)
        {
            if (value is TID)
            {
                return (TID)value;
            }

            return (TID)Convert.ChangeType(value, typeof(TID));
        }
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentRuntimeScene : PersistentRuntimeScene<long>
    {
    }
}


