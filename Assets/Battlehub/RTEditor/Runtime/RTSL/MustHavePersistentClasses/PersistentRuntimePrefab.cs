using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Battlehub.SL2;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSL.Battlehub.SL2
{
    [ProtoContract]
    public class PersistentRuntimePrefab<TID> : PersistentObject<TID>
    {
        [ProtoMember(1)]
        public PersistentDescriptor<TID>[] Descriptors;
        [ProtoMember(2)]
        public PersistentObject<TID>[] Data;
        [ProtoMember(3)]
        public TID[] Identifiers;

        //Identifiers of assets PersistentPrefab depends on
        [ProtoMember(4)]
        public TID[] Dependencies;

        [ProtoMember(5)]
        public PersistentObject<TID>[] EmbeddedAssets;
        [ProtoMember(6)]
        public TID[] EmbeddedAssetIds;

        protected readonly ITypeMap m_typeMap;

        public PersistentRuntimePrefab()
        {
            m_typeMap = IOC.Resolve<ITypeMap>();
        }

        protected override TID ToID(UnityObject uo)
        {
            TID persistentID;
            if (m_assetDB.IsMapped(uo))
            {
                persistentID = m_assetDB.ToID(uo);
            }
            else
            {
                persistentID = m_assetDB.CreateID(uo);
                m_assetDB.RegisterSceneObject(persistentID, uo);
            }

            return persistentID;
        }
        protected void RunCoroutineSync(IEnumerator coroutine)
        {
            while (coroutine.MoveNext())
            {
                IEnumerator subroutine = coroutine.Current as IEnumerator;
                if (subroutine != null)
                {
                    RunCoroutineSync(subroutine);
                }
            };
        }

        protected async Task RunCoroutineAsync(IEnumerator coroutine)
        {
            while (coroutine.MoveNext())
            {
                IEnumerator subroutine = coroutine.Current as IEnumerator;
                if (subroutine != null)
                {
                    await RunCoroutineAsync(subroutine);
                }
                else
                {
                    await Task.Yield();
                }
            };
        }

        protected override void ReadFromImpl(object obj)
        {
            RunCoroutineSync(CoReadFrom(obj));
        }

        public virtual Task ReadFromAsync(object obj)
        {
            return RunCoroutineAsync(CoReadFrom(obj));
          
        }
        protected virtual IEnumerator CoReadFrom(object obj)
        {
            base.ReadFromImpl(obj);

            ClearReferencesCache();

            List<Tuple<PersistentObject<TID>, UnityObject>> data = new List<Tuple<PersistentObject<TID>, UnityObject>>();
            List<TID> identifiers = new List<TID>();
            GetDepsFromContext getDepsCtx = new GetDepsFromContext();

            ReadDescriptorsAndData(obj, data, identifiers, getDepsCtx);
            yield return CoReadEmbeddedAssetsAndData(data, getDepsCtx);

            m_assetDB.UnregisterSceneObjects();
            ClearReferencesCache();
        }

        protected virtual void ReadDescriptorsAndData(object obj, List<Tuple<PersistentObject<TID>, UnityObject>> data, List<TID> identifiers, GetDepsFromContext getDepsCtx)
        {
            GameObject go = (GameObject)obj;
            Descriptors = new[] { CreateDescriptorAndData(go, data, identifiers, getDepsCtx) };
            //Dependencies = getDepsCtx.Dependencies.OfType<UnityObject>().Where(uo => m_assetDB.IsMapped(uo)).Select(uo => ToID(uo)).ToArray();
            Identifiers = identifiers.ToArray();
        }

        protected IEnumerator CoReadEmbeddedAssetsAndData(List<Tuple<PersistentObject<TID>, UnityObject>> data, GetDepsFromContext getDepsCtx)
        {
            HashSet<object> allDeps = getDepsCtx.Dependencies;
            Queue<UnityObject> depsQueue = new Queue<UnityObject>(allDeps.OfType<UnityObject>());
            List<Tuple<PersistentObject<TID>, UnityObject>> assets = new List<Tuple<PersistentObject<TID>, UnityObject>>();
            List<TID> assetIds = new List<TID>();

            GetDepsFromContext getDeepDepsCtx = new GetDepsFromContext();
            while (depsQueue.Count > 0)
            {
                UnityObject uo = depsQueue.Dequeue();
                if (!uo)
                {
                    continue;
                }

                Type persistentType = m_typeMap.ToPersistentType(uo.GetType());
                if (persistentType != null)
                {
                    getDeepDepsCtx.Clear();

                    try
                    {
                        PersistentObject<TID> persistentObject = (PersistentObject<TID>)Activator.CreateInstance(persistentType);
                        if (!(uo is GameObject) && !(uo is Component) && (uo.hideFlags & HideFlags.DontSave) == 0)
                        {
                            if (!m_assetDB.IsMapped(uo))
                            {
                                assets.Add(new Tuple<PersistentObject<TID>, UnityObject>(persistentObject, uo));
                                assetIds.Add(ToID(uo));
                                persistentObject.GetDepsFrom(uo, getDeepDepsCtx);
                            }
                            else
                            {
                                persistentObject.GetDepsFrom(uo, getDeepDepsCtx);
                            }
                        }
                        else
                        {
                            persistentObject.GetDepsFrom(uo, getDeepDepsCtx);
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                    }

                    foreach (UnityObject dep in getDeepDepsCtx.Dependencies)
                    {
                        if (!allDeps.Contains(dep))
                        {
                            allDeps.Add(dep);
                            depsQueue.Enqueue(dep);
                        }
                    }
                }
            }

            //This is wrong to store deep dependencies, this is solution for Sprite loading issue.
            //TODO: Fix scene/prefab save and load procedure to prevent this....
            Dependencies = allDeps.OfType<UnityObject>().Where(uo => m_assetDB.IsMapped(uo)).Select(uo => ToID(uo)).ToArray();

            Data = new PersistentObject<TID>[data.Count];
            for(int i = 0; i < Data.Length; ++i)
            {
                PersistentObject<TID> persistentObject = data[i].Item1;
                persistentObject.ReadFrom(data[i].Item2);
                Data[i] = persistentObject;

                if (i % RTSLSettings.PersistentPrefabReadsPerBatch == RTSLSettings.PersistentPrefabReadsPerBatch - 1)
                {
                    yield return null;
                }
            }

            EmbeddedAssets = new PersistentObject<TID>[assets.Count];
            for (int i = 0; i < EmbeddedAssets.Length; ++i)
            {
                PersistentObject<TID> persistentObject = assets[i].Item1;
                persistentObject.ReadFrom(assets[i].Item2);
                EmbeddedAssets[i] = persistentObject;

                if (i % RTSLSettings.PersistentPrefabReadsPerBatch == RTSLSettings.PersistentPrefabReadsPerBatch - 1)
                {
                    yield return null;
                }
            }

            EmbeddedAssetIds = assetIds.ToArray();
        }

        protected override object WriteToImpl(object obj)
        {
            RunCoroutineSync(CoWriteTo(obj));
            return obj;
        }

        public virtual async Task<object> WriteToAsync(object obj)
        {
            await RunCoroutineAsync(CoWriteTo(obj));
            return Task.FromResult(obj);
        }
        protected virtual IEnumerator CoWriteTo(object obj)
        {
            base.WriteToImpl(obj);
            yield return CoRestoreDataAndResolveDependencies(null);
        }

        protected override void GetDepsImpl(GetDepsContext<TID> context)
        {
            base.GetDepsImpl(context);
            if (Dependencies != null)
            {
                for (int i = 0; i < Dependencies.Length; ++i)
                {
                    context.Dependencies.Add(Dependencies[i]);
                }
            }
        }

        protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)
        {
            base.GetDepsFromImpl(obj, context);
            if (!(obj is GameObject))
            {
                return;
            }

            //Prefab parts should not be considered as external dependencies. This list required to remove prefab parts (children and components) from dependencies collection. 
            List<object> prefabParts = new List<object>();

            GetDependenciesFrom((GameObject)obj, prefabParts, context);

            for (int i = 0; i < prefabParts.Count; ++i)
            {
                context.Dependencies.Remove(prefabParts[i]);
            }
        }

        protected virtual void GetDependenciesFrom(GameObject go, List<object> prefabParts, GetDepsFromContext context)
        {
            if (go.GetComponent<RTSLIgnore>())
            {
                //Do not save persistent ignore objects
                return;
            }

            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            if (exposeToEditor != null && exposeToEditor.MarkAsDestroyed)
            {
                return;
            }

            Type persistentType = m_typeMap.ToPersistentType(go.GetType());
            if (persistentType == null)
            {
                return;
            }

            prefabParts.Add(go);

            PersistentObject<TID> goData = (PersistentObject<TID>)Activator.CreateInstance(persistentType);
            goData.GetDepsFrom(go, context);

            Component[] components = go.GetComponents<Component>().Where(c => c != null).ToArray();
            if (components.Length > 0)
            {
                for (int i = 0; i < components.Length; ++i)
                {
                    Component component = components[i];
                    Type persistentComponentType = m_typeMap.ToPersistentType(component.GetType());
                    if (persistentComponentType == null)
                    {
                        continue;
                    }

                    prefabParts.Add(component);

                    PersistentObject<TID> componentData = (PersistentObject<TID>)Activator.CreateInstance(persistentComponentType);
                    componentData.GetDepsFrom(component, context);
                }
            }

            Transform transform = go.transform;
            if (transform.childCount > 0)
            {
                foreach (Transform child in transform)
                {
                    GetDependenciesFrom(child.gameObject, prefabParts, context);
                }
            }
        }


        /// <summary>
        /// Create GameObjects hierarchy and Add Components recursively
        /// </summary>
        /// <param name="descriptor">PersistentObject descriptor (initially root descriptor)</param>
        /// <param name="idToObj">Dictionary instanceId->UnityObject which will be populated with GameObjects and Components</param>
        public void CreateGameObjectWithComponents(ITypeMap typeMap, PersistentDescriptor<TID> descriptor, Dictionary<TID, UnityObject> idToObj, Transform parent, List<GameObject> createdGameObjects = null, Dictionary<TID, UnityObject> decomposition = null)
        {
            UnityObject objGo;
            GameObject go;
            if (idToObj.TryGetValue(descriptor.PersistentID, out objGo))
            {
                throw new ArgumentException(string.Format("duplicate object descriptor found in descriptors hierarchy. {0}", descriptor.ToString()), "descriptor");
            }
            else
            {
                go = new GameObject();
                if (parent != null)
                {
                    go.transform.SetParent(parent, false);
                }
                idToObj.Add(descriptor.PersistentID, go);
            }

            if (decomposition != null)
            {
                if (!decomposition.ContainsKey(descriptor.PersistentID))
                {
                    decomposition.Add(descriptor.PersistentID, go);
                }
            }

            if (createdGameObjects != null)
            {
                createdGameObjects.Add(go);
            }

            go.SetActive(false);

            if (descriptor.Parent != null)
            {
                UnityObject parentGO;
                if (!idToObj.TryGetValue(descriptor.Parent.PersistentID, out parentGO))
                {
                    throw new ArgumentException(string.Format("objects dictionary is supposed to have object with PersistentID {0} at this stage. Descriptor {1}", descriptor.Parent.PersistentID, descriptor, "descriptor"));
                }

                if (parentGO == null)
                {
                    throw new ArgumentException(string.Format("object with PersistentID {0} should have GameObject type. Descriptor {1}", descriptor.Parent.PersistentID, descriptor, "descriptor"));
                }
                go.transform.SetParent(((GameObject)parentGO).transform, false);
            }

            if (descriptor.Components != null)
            {
                Dictionary<Type, bool> requirements = new Dictionary<Type, bool>();
                for (int i = 0; i < descriptor.Components.Length; ++i)
                {
                    PersistentDescriptor<TID> componentDescriptor = descriptor.Components[i];

                    Type persistentComponentType = m_typeMap.ToType(componentDescriptor.PersistentTypeGuid);
                    Type componentType;
                    if (persistentComponentType != typeof(PersistentRuntimeSerializableObject<TID>))
                    {
                        if (persistentComponentType == null)
                        {
                            Debug.LogWarningFormat("Unknown type {0} associated with component Descriptor {1}", componentDescriptor.PersistentTypeGuid, componentDescriptor.ToString());
                            idToObj.Add(componentDescriptor.PersistentID, null);
                            continue;
                        }

                        componentType = typeMap.ToUnityType(persistentComponentType);
                        if (componentType == null)
                        {
                            Debug.LogWarningFormat("There is no mapped type for " + persistentComponentType.FullName + " in TypeMap");
                            idToObj.Add(componentDescriptor.PersistentID, null);
                            continue;
                        }
                    }
                    else
                    {
                        componentType = typeMap.ToType(componentDescriptor.RuntimeTypeGuid);
                        if (componentType == null)
                        {
                            Debug.LogWarningFormat("There is no runtime type with guid {0}", componentDescriptor.RuntimeTypeGuid);
                            idToObj.Add(componentDescriptor.PersistentID, null);
                            continue;
                        }
                    }

                    if (!componentType.IsSubclassOf(typeof(Component)))
                    {
                        Debug.LogErrorFormat("{0} is not subclass of {1}", componentType.FullName, typeof(Component).FullName);
                        idToObj.Add(componentDescriptor.PersistentID, null);
                        continue;
                    }

                    UnityObject obj;
                    if (idToObj.TryGetValue(componentDescriptor.PersistentID, out obj))
                    {
                        if (obj != null && !(obj is Component))
                        {
                            Debug.LogError("Invalid Type. Component " + obj.name + " " + obj.GetType() + " " + obj.GetInstanceID() + " " + descriptor.PersistentTypeGuid + " " + componentDescriptor.PersistentTypeGuid);
                        }
                    }
                    else
                    {
                        obj = AddComponent(idToObj, go, requirements, componentDescriptor, componentType);
                    }

                    if (decomposition != null)
                    {
                        if (!decomposition.ContainsKey(componentDescriptor.PersistentID))
                        {
                            decomposition.Add(componentDescriptor.PersistentID, obj);
                        }
                    }
                }
            }

            if (descriptor.Children != null)
            {
                for (int i = 0; i < descriptor.Children.Length; ++i)
                {
                    PersistentDescriptor<TID> childDescriptor = descriptor.Children[i];
                    CreateGameObjectWithComponents(typeMap, childDescriptor, idToObj, null, createdGameObjects, decomposition);
                }
            }
        }

        protected IEnumerator CoRestoreDataAndResolveDependencies(Dictionary<TID, UnityObject> idToUnityObj)
        {
            ClearReferencesCache();

            UnityObject[] assetInstances = null;
            if (EmbeddedAssetIds != null)
            {
                if (idToUnityObj == null)
                {
                    idToUnityObj = new Dictionary<TID, UnityObject>();
                }

                IUnityObjectFactory factory = IOC.Resolve<IUnityObjectFactory>();
                assetInstances = new UnityObject[EmbeddedAssetIds.Length];
                for (int i = 0; i < EmbeddedAssetIds.Length; ++i)
                {
                    Type uoType;
                    PersistentObject<TID> asset = EmbeddedAssets[i];
                    if (asset is PersistentRuntimeSerializableObject<TID>)
                    {
                        PersistentRuntimeSerializableObject<TID> runtimeSerializableObject = (PersistentRuntimeSerializableObject<TID>)asset;
                        uoType = runtimeSerializableObject.ObjectType;
                    }
                    else
                    {
                        uoType = m_typeMap.ToUnityType(asset.GetType());
                    }

                    if (uoType != null)
                    {
                        if (factory.CanCreateInstance(uoType, asset))
                        {
                            UnityObject assetInstance = factory.CreateInstance(uoType, asset);
                            if (assetInstance != null)
                            {
                                assetInstances[i] = assetInstance;
                                idToUnityObj.Add(EmbeddedAssetIds[i], assetInstance);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Unable to create object of type " + uoType.ToString());
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Unable to resolve unity type for " + asset.GetType().FullName);
                    }
                }
            }

            if (idToUnityObj != null)
            {
                m_assetDB.RegisterSceneObjects(idToUnityObj);
            }

            if (assetInstances != null)
            {
                for (int i = 0; i < assetInstances.Length; ++i)
                {
                    UnityObject assetInstance = assetInstances[i];
                    if (assetInstance != null)
                    {
                        PersistentObject<TID> asset = EmbeddedAssets[i];
                        asset.WriteTo(assetInstance);

                        if (i % RTSLSettings.PersistentPrefabWritesPerBatch == RTSLSettings.PersistentPrefabWritesPerBatch - 1)
                        {
                            yield return null;
                        }
                    }
                }
            }

            List<GameObject> goList = new List<GameObject>();
            List<bool> goActivationList = new List<bool>();

            //This list is necessary to deal with edge cases where Avatar for some Animators must be created using AvatarBuilder.
            //AvatarBuilder requires a hierarchy of children with names.
            //TODO: Implement a common mechanism to handle such situations. For example, add the OnAfterWrite method.
            List<int> deferredIndices = new List<int>();

            for (int i = 0; i < Data.Length; ++i)
            {
                PersistentObject<TID> data = Data[i];
                TID id = Identifiers[i];

                UnityObject obj = FromID<UnityObject>(id);
                if (obj == null)
                {
                    Debug.LogWarningFormat("objects does not have object with instance id {0} however PersistentData of type {1} is present", id, data.GetType());
                    continue;
                }

                if (obj is Animator)
                {
                    deferredIndices.Add(i);
                    continue;
                }

                data.WriteTo(obj);

                if (i % RTSLSettings.PersistentPrefabWritesPerBatch == RTSLSettings.PersistentPrefabWritesPerBatch - 1)
                {
                    yield return null;
                }

                if (obj is GameObject)
                {
                    goList.Add((GameObject)obj);
                    PersistentGameObject<TID> goData = (PersistentGameObject<TID>)data;
                    goActivationList.Add(goData.ActiveSelf);
                }
            }

            for (int i = 0; i < deferredIndices.Count; ++i)
            {
                int index = deferredIndices[i];

                PersistentObject<TID> data = Data[index];
                TID id = Identifiers[index];

                UnityObject obj = FromID<UnityObject>(id);
                data.WriteTo(obj);

                if (i % RTSLSettings.PersistentPrefabWritesPerBatch == RTSLSettings.PersistentPrefabWritesPerBatch - 1)
                {
                    yield return null;
                }
            }

            for (int i = 0; i < goList.Count; ++i)
            {
                bool activeSelf = goActivationList[i];
                GameObject go = goList[i];
                if (go != null)
                {
                    go.SetActive(activeSelf);
                }
            }

            m_assetDB.UnregisterSceneObjects();
            ClearReferencesCache();
        }

        /// <summary>
        /// Add  dependencies here to let AddComponent method to figure out which components automatically added
        /// for example ParticleSystemRenderer should be added automatically if ParticleSystem component exists 
        /// </summary>
        public readonly static Dictionary<Type, HashSet<Type>> ComponentDependencies = new Dictionary<Type, HashSet<Type>>
            {
                //type depends on <- { types }
                { typeof(ParticleSystemRenderer), new HashSet<Type> { typeof(ParticleSystem) } }
            };

        private UnityObject AddComponent(Dictionary<TID, UnityObject> idToObj, GameObject go, Dictionary<Type, bool> requirements, PersistentDescriptor<TID> componentDescriptor, Type componentType)
        {
            Component component;
            bool isReqFulfilled = requirements.ContainsKey(componentType) && requirements[componentType];
            bool maybeComponentAlreadyAdded =
                !isReqFulfilled ||
                componentType.IsSubclassOf(typeof(Transform)) ||
                componentType == typeof(Transform) ||
                componentType.IsDefined(typeof(DisallowMultipleComponent), true) ||
                ComponentDependencies.ContainsKey(componentType) && ComponentDependencies[componentType].Any(d => go.GetComponent(d) != null);

            if (maybeComponentAlreadyAdded)
            {
                component = go.GetComponent(componentType);
                if (component == null)
                {
                    component = go.AddComponent(componentType);
                }
                if (!isReqFulfilled)
                {
                    requirements[componentType] = true;
                }
            }
            else
            {
                component = go.AddComponent(componentType);
                if (component == null)
                {
                    component = go.GetComponent(componentType);
                }
            }
            if (component == null)
            {
                Debug.LogErrorFormat("Unable to add or get component of type {0}", componentType);
            }
            else
            {
                object[] requireComponents = component.GetType().GetCustomAttributes(typeof(RequireComponent), true);
                for (int j = 0; j < requireComponents.Length; ++j)
                {
                    RequireComponent requireComponent = requireComponents[j] as RequireComponent;
                    if (requireComponent != null)
                    {
                        if (requireComponent.m_Type0 != null && !requirements.ContainsKey(requireComponent.m_Type0))
                        {
                            bool fulfilled = go.GetComponent(requireComponent.m_Type0);
                            requirements.Add(requireComponent.m_Type0, fulfilled);
                        }
                        if (requireComponent.m_Type1 != null && !requirements.ContainsKey(requireComponent.m_Type1))
                        {
                            bool fulfilled = go.GetComponent(requireComponent.m_Type1);
                            requirements.Add(requireComponent.m_Type1, fulfilled);
                        }
                        if (requireComponent.m_Type2 != null && !requirements.ContainsKey(requireComponent.m_Type2))
                        {
                            bool fulfilled = go.GetComponent(requireComponent.m_Type2);
                            requirements.Add(requireComponent.m_Type2, fulfilled);
                        }
                    }
                }
                idToObj.Add(componentDescriptor.PersistentID, component);
            }

            return component;
        }


        protected virtual PersistentDescriptor<TID> CreateDescriptorAndData(GameObject go, List<Tuple<PersistentObject<TID>, UnityObject>> persistentData, List<TID> persistentIdentifiers, GetDepsFromContext getDepsFromCtx, PersistentDescriptor<TID> parentDescriptor = null)
        {
            if (go.GetComponent<RTSLIgnore>())
            {
                //Do not save persistent ignore objects
                return null;
            }
            Type type = go.GetType();
            Type persistentType = m_typeMap.ToPersistentType(type);
            if (persistentType == null)
            {
                return null;
            }

            TID persistentID = ToID(go);

            PersistentDescriptor<TID> descriptor = new PersistentDescriptor<TID>(m_typeMap.ToGuid(persistentType), persistentID, go.name, m_typeMap.ToGuid(type));
            descriptor.Parent = parentDescriptor;

            PersistentObject<TID> goData = (PersistentObject<TID>)Activator.CreateInstance(persistentType);
            goData.GetDepsFrom(go, getDepsFromCtx);
            persistentData.Add(new Tuple<PersistentObject<TID>, UnityObject>(goData, go));
            persistentIdentifiers.Add(persistentID);

            Component[] components = go.GetComponents<Component>().Where(c => c != null).ToArray();
            if (components.Length > 0)
            {
                List<PersistentDescriptor<TID>> componentDescriptors = new List<PersistentDescriptor<TID>>();
                for (int i = 0; i < components.Length; ++i)
                {
                    Component component = components[i];
                    Type componentType = component.GetType();
                    Type persistentComponentType = m_typeMap.ToPersistentType(componentType);
                    if (persistentComponentType == null)
                    {
                        continue;
                    }

                    TID componentID = ToID(component);

                    PersistentDescriptor<TID> componentDescriptor = new PersistentDescriptor<TID>(m_typeMap.ToGuid(persistentComponentType), componentID, component.name, m_typeMap.ToGuid(componentType));
                    componentDescriptor.Parent = descriptor;
                    componentDescriptors.Add(componentDescriptor);

                    PersistentObject<TID> componentData = (PersistentObject<TID>)Activator.CreateInstance(persistentComponentType);
                    componentData.GetDepsFrom(component, getDepsFromCtx);
                    persistentData.Add(new Tuple<PersistentObject<TID>, UnityObject>(componentData, component));
                    persistentIdentifiers.Add(componentID);
                }

                if (componentDescriptors.Count > 0)
                {
                    descriptor.Components = componentDescriptors.ToArray();
                }
            }

            Transform transform = go.transform;
            if (transform.childCount > 0)
            {
                List<PersistentDescriptor<TID>> children = new List<PersistentDescriptor<TID>>();
                foreach (Transform child in transform)
                {
                    PersistentDescriptor<TID> childDescriptor = CreateDescriptorAndData(child.gameObject, persistentData, persistentIdentifiers, getDepsFromCtx, descriptor);
                    if (childDescriptor != null)
                    {
                        children.Add(childDescriptor);
                    }
                }

                descriptor.Children = children.ToArray();
            }

            return descriptor;
        }
    }

    [Obsolete("Use generic version")]
    [ProtoContract]
    public class PersistentRuntimePrefab : PersistentRuntimePrefab<long>
    {
    }
}


