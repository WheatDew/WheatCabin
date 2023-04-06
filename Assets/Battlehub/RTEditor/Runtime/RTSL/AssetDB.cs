using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL
{   
    public interface IIDMap<TID>
    {
        TID CreateID(UnityObject obj);

        TID NullID { get; }
        bool IsNullID(TID id);

        bool IsStaticResourceID(TID id);
        bool IsDynamicResourceID(TID id);

        bool IsMapped(TID id);
        bool IsMapped(UnityObject uo);

        TID ToID(UnityObject uo);
        T FromID<T>(TID id) where T : UnityObject;


        bool TryToReplaceID(UnityObject uo, TID id);

    }

    public interface IAssetDB<TID> : IIDMap<TID>
    {
        void RegisterSceneObject(TID id, UnityObject obj);
        void RegisterSceneObjects(Dictionary<TID, UnityObject> idToObj);
        void UnregisterSceneObjects();

        void RegisterDynamicResource(TID persistentID, UnityObject obj);
        void UnregisterDynamicResource(TID persistentID);
        void RegisterDynamicResources(Dictionary<TID, UnityObject> idToObj);
        void UnregisterDynamicResources(Dictionary<TID, UnityObject> idToObj);
        void UnregisterDynamicResources();
        UnityObject[] GetDynamicResources();
        AsyncOperation UnloadUnusedAssets(Action<AsyncOperation> completedCallback = null);

        int GetAssetLibraryID(TID persistentID);
        void RegisterStaticResource(int assetLibraryID, TID persistentID, UnityObject obj);
        void UnregisterStaticResource(TID persistentID);
        void UnregisterStaticResources();
    }

    public class AssetDB<TID> : IAssetDB<TID> where TID : struct
    {
        public TID NullID => default;

        private readonly Dictionary<TID, UnityObject> m_idToStaticObj = new Dictionary<TID, UnityObject>();
        private readonly Dictionary<int, TID> m_staticIDToPersistentID = new Dictionary<int, TID>();
        private readonly Dictionary<TID, int> m_persistentIDToAssetLibraryID = new Dictionary<TID, int>();

        private readonly Dictionary<TID, UnityObject> m_idToDynamicObj = new Dictionary<TID, UnityObject>();
        private readonly Dictionary<int, TID> m_dynamicIDToPersistentID = new Dictionary<int, TID>();
        
        private readonly HashSet<int> m_sceneObjects = new HashSet<int>();

        private Func<UnityObject, TID> m_createIDFunc;

        public AssetDB(Func<UnityObject, TID> createIDFunc)
        {
            m_createIDFunc = createIDFunc;
        }

        public virtual TID CreateID(UnityObject obj)
        {
            return m_createIDFunc(obj);
        }

        public virtual bool IsNullID(TID id)
        {
            return id.Equals(NullID);
        }

        public virtual bool IsStaticResourceID(TID id)
        {
            return m_idToStaticObj.ContainsKey(id);
        }

        public virtual bool IsDynamicResourceID(TID id)
        {
            return !IsStaticResourceID(id);
        }

        public virtual bool IsMapped(UnityObject uo)
        {
            if (uo == null)
            {
                return false;
            }
            int instanceId = uo.GetInstanceID();
            return m_staticIDToPersistentID.ContainsKey(instanceId) || m_dynamicIDToPersistentID.ContainsKey(instanceId) && !m_sceneObjects.Contains(instanceId);
        }

        public virtual bool IsMapped(TID id)
        {
            UnityObject obj;
            if(m_idToDynamicObj.TryGetValue(id, out obj))
            {
                return !m_sceneObjects.Contains(obj.GetInstanceID());
            }
            
            return m_idToStaticObj.ContainsKey(id);
        }


        public virtual TID ToID(UnityObject uo)
        {
            if (uo == null)
            {
                return NullID;
            }

            TID id;
            int instanceId = uo.GetInstanceID();
            if (m_dynamicIDToPersistentID.TryGetValue(instanceId, out id))
            {
                return id;
            }

            if(m_staticIDToPersistentID.TryGetValue(instanceId, out id))
            {
                return id;
            }

            return NullID;
        }

        public virtual T FromID<T>(TID id) where T : UnityObject
        {
            UnityObject result;
            if (m_idToDynamicObj.TryGetValue(id, out result))
            {
                return (T)result;
            }
            
            if(m_idToStaticObj.TryGetValue(id, out result))
            {
                return (T)result;
            }

            return default;
        }

        public virtual void RegisterSceneObject(TID id, UnityObject obj)
        {
            if(obj == null)
            {
                return;
            }

            int instanceId = obj.GetInstanceID();
            m_dynamicIDToPersistentID.Add(instanceId, id);
            m_idToDynamicObj.Add(id, obj);
            m_sceneObjects.Add(instanceId);
        }

        public virtual void RegisterSceneObjects(Dictionary<TID, UnityObject> idToObj)
        {
            foreach (KeyValuePair<TID, UnityObject> kvp in idToObj)
            {
                RegisterSceneObject(kvp.Key, kvp.Value);
            }
        }

        public void UnregisterSceneObjects()
        {
            foreach (int id in m_sceneObjects)
            {
                TID persistentID;
                if (m_dynamicIDToPersistentID.TryGetValue(id, out persistentID))
                {
                    m_dynamicIDToPersistentID.Remove(id);
                    m_idToDynamicObj.Remove(persistentID);
                }
            }
            m_sceneObjects.Clear();
        }

        public virtual void RegisterDynamicResource(TID persistentID, UnityObject obj)
        {
            m_idToDynamicObj.Add(persistentID, obj);
            m_dynamicIDToPersistentID.Add(obj.GetInstanceID(), persistentID);
        }

        public virtual void RegisterDynamicResources(Dictionary<TID, UnityObject> idToObj)
        {
            foreach(KeyValuePair<TID, UnityObject> kvp in idToObj)
            {
                RegisterDynamicResource(kvp.Key, kvp.Value);
            }
        }

        public void UnregisterDynamicResource(TID persistentID)
        {
            UnityObject obj;
            if (m_idToDynamicObj.TryGetValue(persistentID, out obj))
            {
                m_idToDynamicObj.Remove(persistentID);
                if (((object)obj) != null)
                {
                    m_dynamicIDToPersistentID.Remove(obj.GetInstanceID());
                }
                else
                {
                    Debug.LogWarning("obj with persistent id " + persistentID + " is null");
                }
            }
        }

        public void UnregisterDynamicResources(Dictionary<TID, UnityObject> idToObj)
        {
            foreach (KeyValuePair<TID, UnityObject> kvp in idToObj)
            {
                m_idToDynamicObj.Remove(kvp.Key);
                if (kvp.Value != null)
                {
                    m_dynamicIDToPersistentID.Remove(kvp.Value.GetInstanceID());
                }
            }
        }

        public void UnregisterDynamicResources()
        {
            m_dynamicIDToPersistentID.Clear();
            m_idToDynamicObj.Clear();
        }

        public virtual UnityObject[] GetDynamicResources()
        {
            return m_idToDynamicObj.Values.ToArray();
        }

        public virtual bool TryToReplaceID(UnityObject uo, TID id)
        {
            if (uo == null)
            {
                return false;
            }

            int instanceID = uo.GetInstanceID();
            if (m_dynamicIDToPersistentID.ContainsKey(instanceID))
            {
                if (!id.Equals(m_dynamicIDToPersistentID[instanceID]))
                {
                    m_dynamicIDToPersistentID[instanceID] = id;
                    return true;
                }
            }
            else if(m_staticIDToPersistentID.ContainsKey(instanceID))
            {
                if (!id.Equals(m_staticIDToPersistentID[instanceID]))
                {
                    m_staticIDToPersistentID[instanceID] = id;
                    return true;
                }
            }

            return false;
        }

        public AsyncOperation UnloadUnusedAssets(Action<AsyncOperation> completedCallback = null)
        {
            AsyncOperation operation = Resources.UnloadUnusedAssets();

            if (completedCallback != null)
            {
                if (operation.isDone)
                {
                    completedCallback(operation);
                }
                else
                {
                    Action<AsyncOperation> onCompleted = null;
                    onCompleted = ao =>
                    {
                        operation.completed -= onCompleted;
                        completedCallback(operation);
                    };
                    operation.completed += onCompleted;
                }
            }

            return operation;
        }

        public int GetAssetLibraryID(TID persistentID)
        {
            if (!m_persistentIDToAssetLibraryID.TryGetValue(persistentID, out int id))
            {
                return -1;
            }
            return id;
        }

        public void RegisterStaticResource(int assetLibraryId, TID persistentID, UnityObject obj)
        {
            m_idToStaticObj.Add(persistentID, obj);
            int instanceID = obj.GetInstanceID();
            if(!m_staticIDToPersistentID.ContainsKey(instanceID))
            {
                m_staticIDToPersistentID.Add(instanceID, persistentID);
            }
            m_persistentIDToAssetLibraryID.Add(persistentID, assetLibraryId);
        }
        public void UnregisterStaticResource(TID persistentID)
        {
            UnityObject obj;
            if (m_idToStaticObj.TryGetValue(persistentID, out obj))
            {
                m_idToStaticObj.Remove(persistentID);
                if (((object)obj) != null)
                {
                    m_staticIDToPersistentID.Remove(obj.GetInstanceID());
                }
                else
                {
                    Debug.LogWarning("obj with persistent id " + persistentID + " is null");
                }
            }

            m_persistentIDToAssetLibraryID.Remove(persistentID);
        }

        public void UnregisterStaticResources()
        {
            m_staticIDToPersistentID.Clear();
            m_idToStaticObj.Clear();
            m_persistentIDToAssetLibraryID.Clear();
        }
    }

    public interface IIDMap : IIDMap<long>
    {
        bool IsInstanceID(long id);

        int ToOrdinal(long id);
        int ToOrdinal(int id);

        int ToInt(long id);

        long ToStaticResourceID(int ordinal, int id);
        long ToDynamicResourceID(int ordinal, int id);


        [Obsolete]
        long[] ToID(UnityObject[] uo);
        [Obsolete]
        long[] ToID<T>(List<T> uo) where T : UnityObject;
        [Obsolete]
        long[] ToID<T>(IEnumerable<T> uo) where T : UnityObject;
        [Obsolete]
        T[] FromID<T>(long[] id) where T : UnityObject;

        [Obsolete]
        bool IsStaticFolderID(long id);

        [Obsolete]
        bool IsDynamicFolderID(long id);

        [Obsolete]
        bool IsSceneID(long id);
        [Obsolete]
        bool IsResourceID(long id);

        [Obsolete]
        long ToStaticFolderID(int ordinal, int id);

        [Obsolete]
        long ToDynamicFolderID(int ordinal, int id);

        [Obsolete]
        long ToSceneID(int ordinal, int id);
    }

    public interface IAssetDB : IAssetDB<long>, IIDMap
    {
        bool IsStaticLibrary(int ordinal);
        bool IsSceneLibrary(int ordinal);
        bool IsBuiltinLibrary(int ordinal);
        bool IsBundledLibrary(int ordinal);
        bool IsDynamicLibrary(int ordinal);

        [Obsolete]
        void RegisterDynamicResource(int persistentID, UnityObject obj);
        [Obsolete]
        void UnregisterDynamicResource(int persistentID);
        [Obsolete]
        void RegisterDynamicResources(Dictionary<int, UnityObject> idToObj);
        [Obsolete]
        void UnregisterDynamicResources(Dictionary<int, UnityObject> idToObj);

        bool IsLibraryLoaded(int ordinal);
        bool AddLibrary(AssetLibraryAsset assetLib, int ordinal, bool IIDtoObj, bool PIDtoObj);
        void LoadLibrary(string assetLibrary, int ordinal, bool loadIIDtoPID, bool loadPIDtoObj, Action<bool> callback);
        void UnloadLibrary(int ordinal);
        void UnloadLibraries();
    }

    public class AssetDB : IAssetDB
    {
        private readonly HashSet<AssetLibraryAsset> m_loadedLibraries = new HashSet<AssetLibraryAsset>();
        private readonly Dictionary<int, AssetLibraryAsset> m_ordinalToLib = new Dictionary<int, AssetLibraryAsset>();

        private MappingInfo m_mapping = new MappingInfo();

        private readonly Dictionary<int, UnityObject> m_persistentIDToSceneObject = new Dictionary<int, UnityObject>();
        private readonly Dictionary<int, int> m_sceneObjectIDToPersistentID = new Dictionary<int, int>();

        private readonly Dictionary<int, UnityObject> m_persistentIDToDynamicResource = new Dictionary<int, UnityObject>();
        private readonly Dictionary<int, int> m_dynamicResourceIDToPersistentID = new Dictionary<int, int>();

        public bool IsStaticLibrary(int ordinal)
        {
            return AssetLibraryInfo.STATICLIB_FIRST <= ordinal && ordinal <= AssetLibraryInfo.STATICLIB_LAST;
        }

        public bool IsSceneLibrary(int ordinal)
        {
            return AssetLibraryInfo.SCENELIB_FIRST <= ordinal && ordinal <= AssetLibraryInfo.SCENELIB_LAST;
        }

        public bool IsBuiltinLibrary(int ordinal)
        {
            return AssetLibraryInfo.BUILTIN_FIRST <= ordinal && ordinal <= AssetLibraryInfo.BUILTIN_LAST;
        }

        public bool IsBundledLibrary(int ordinal)
        {
            return AssetLibraryInfo.BUNDLEDLIB_FIRST <= ordinal && ordinal <= AssetLibraryInfo.BUNDLEDLIB_LAST;
        }

        public bool IsDynamicLibrary(int ordinal)
        {
            return AssetLibraryInfo.DYNAMICLIB_FIRST <= ordinal && ordinal <= AssetLibraryInfo.DYNAMICLIB_LAST;
        }

        public void RegisterSceneObject(long id, UnityObject obj)
        {
            _RegisterSceneObject(id, obj);
        }

        private void _RegisterSceneObject(long id, UnityObject obj)
        {
            int intId = ToInt(id);
            if (!m_persistentIDToSceneObject.ContainsKey(intId))
            {
                m_persistentIDToSceneObject.Add(intId, obj);
            }

            if (obj != null)
            {
                int instanceId = obj.GetInstanceID();
                if (!m_sceneObjectIDToPersistentID.ContainsKey(instanceId))
                {
                    m_sceneObjectIDToPersistentID.Add(instanceId, intId);
                }
            }
        }

        public void RegisterSceneObjects(Dictionary<long, UnityObject> idToObj)
        {
            if (m_persistentIDToSceneObject.Count != 0)
            {
                Debug.LogWarning("scene objects were not unregistered");
            }

            foreach (KeyValuePair<long, UnityObject> kvp in idToObj)
            {
                _RegisterSceneObject(kvp.Key, kvp.Value);
            }
        }

        public void UnregisterSceneObjects()
        {
            m_persistentIDToSceneObject.Clear();
            m_sceneObjectIDToPersistentID.Clear();
        }

        public void RegisterDynamicResource(int persistentID, UnityObject obj)
        {
            m_persistentIDToDynamicResource[persistentID] = obj;
            if (obj != null)
            {
                m_dynamicResourceIDToPersistentID[obj.GetInstanceID()] = persistentID;
            }
        }
        public void RegisterDynamicResources(Dictionary<int, UnityObject> idToObj)
        {
            foreach (KeyValuePair<int, UnityObject> kvp in idToObj)
            {
                m_persistentIDToDynamicResource[kvp.Key] = kvp.Value;
                if (kvp.Value != null)
                {
                    m_dynamicResourceIDToPersistentID[kvp.Value.GetInstanceID()] = kvp.Key;
                }
            }
        }

        public void UnregisterDynamicResources(Dictionary<int, UnityObject> idToObj)
        {
            foreach (KeyValuePair<int, UnityObject> kvp in idToObj)
            {
                m_persistentIDToDynamicResource.Remove(kvp.Key);
                if (kvp.Value != null)
                {
                    m_dynamicResourceIDToPersistentID.Remove(kvp.Value.GetInstanceID());
                }
            }
        }

        public void UnregisterDynamicResource(int persistentID)
        {
            UnityObject obj;
            if(m_persistentIDToDynamicResource.TryGetValue(persistentID, out obj))
            {
                m_persistentIDToDynamicResource.Remove(persistentID);
                if(((object)obj) != null)
                {
                    m_dynamicResourceIDToPersistentID.Remove(obj.GetInstanceID());
                }
                else
                {
                    Debug.LogWarning("obj with persistent id " + persistentID + " is null");
                }   
            }
        }

        public void UnregisterDynamicResources()
        {
            m_persistentIDToDynamicResource.Clear();
            m_dynamicResourceIDToPersistentID.Clear();
        }

        public UnityObject[] GetDynamicResources()
        {
            return m_persistentIDToDynamicResource.Values.Where(o => o != null).ToArray();
        }

        public bool IsLibraryLoaded(int ordinal)
        {
            return m_ordinalToLib.ContainsKey(ordinal);
        }

        public bool AddLibrary(AssetLibraryAsset assetLib, int ordinal, bool IIDtoPID, bool PIDtoObj)
        {
            if (m_ordinalToLib.ContainsKey(ordinal))
            {
                Debug.LogWarningFormat("Asset Library with ordinal {0} already loadeded", assetLib.Ordinal);
                return false;
            }

            if (m_loadedLibraries.Contains(assetLib))
            {
                Debug.LogWarning("Asset Library already added");
                return false;
            }

            assetLib.Ordinal = ordinal;
            m_loadedLibraries.Add(assetLib);
            m_ordinalToLib.Add(ordinal, assetLib);
            LoadMapping(ordinal, IIDtoPID, PIDtoObj);

            return true;
        }

        public void LoadLibrary(string assetLibrary, int ordinal, bool loadIIDtoPID, bool loadPIDtoObj, Action<bool> callback)
         {
            if (m_ordinalToLib.ContainsKey(ordinal))
            {
                Debug.LogWarningFormat("Asset Library {0} with this same ordinal {1} already loaded", m_ordinalToLib[ordinal].name, ordinal);
                callback(false);
                return;
            }

            ResourceRequest request = Resources.LoadAsync<AssetLibraryAsset>(assetLibrary);
            Action<AsyncOperation> completed = null;
            completed = ao =>
            {
                AssetLibraryAsset assetLib = (AssetLibraryAsset)request.asset;
                if (assetLib == null)
                {
                    if(IsBuiltinLibrary(ordinal))
                    {
                        if (ordinal - AssetLibraryInfo.BUILTIN_FIRST == 0)
                        {
                            Debug.LogWarningFormat("Asset Library was not found : {0}. Click Tools->Runtime SaveLoad->Update Libraries.", assetLibrary);
                        }
                    }
                    else if(IsSceneLibrary(ordinal))
                    {
                        if (ordinal - AssetLibraryInfo.SCENELIB_FIRST == 0)
                        {
                            Debug.LogWarningFormat("Asset Library was not found : {0}. Click Tools->Runtime SaveLoad->Update Libraries.", assetLibrary);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("Asset Library was not found : {0}", assetLibrary);
                    }
                    
                    callback(false);
                    return;
                }
                AddLibrary(assetLib, ordinal, loadIIDtoPID, loadPIDtoObj);
                callback(true);
                request.completed -= completed;
            };
            request.completed += completed;
        } 

        public void UnloadLibrary(int ordinal)
        {
            AssetLibraryAsset assetLib;
            if(m_ordinalToLib.TryGetValue(ordinal, out assetLib))
            {
                UnloadMapping(ordinal);
                m_loadedLibraries.Remove(assetLib);
                m_ordinalToLib.Remove(ordinal);
                if(!IsBundledLibrary(assetLib.Ordinal))
                {
                    Resources.UnloadAsset(assetLib);
                }
            }
        }

        public void UnloadLibraries()
        {
            foreach (AssetLibraryAsset assetLibrary in m_loadedLibraries)
            {
                if(!IsBundledLibrary(assetLibrary.Ordinal))
                {
                    Resources.UnloadAsset(assetLibrary);
                }
            }
            
            m_ordinalToLib.Clear();
            m_loadedLibraries.Clear();
            UnloadMappings();
        }

        public AsyncOperation UnloadUnusedAssets(Action<AsyncOperation> completedCallback = null)
        {
            AsyncOperation operation = Resources.UnloadUnusedAssets();

            if(completedCallback != null)
            {
                if(operation.isDone)
                {
                    completedCallback(operation);
                }
                else
                {
                    Action<AsyncOperation> onCompleted = null;
                    onCompleted = ao =>
                    {
                        operation.completed -= onCompleted;
                        completedCallback(operation);
                    };
                    operation.completed += onCompleted;
                }
            }
           
            return operation;
        }

        private void LoadMapping(int ordinal, bool IIDtoPID, bool PIDtoObj)
        {
            AssetLibraryAsset assetLib;
            if(m_ordinalToLib.TryGetValue(ordinal, out assetLib))
            {
                assetLib.LoadIDMappingTo(m_mapping, IIDtoPID, PIDtoObj);
            }
            else
            {
                throw new ArgumentException(string.Format("Unable to find assetLibrary with ordinal = {0}", ordinal), "ordinal");
            }
        }

        private void UnloadMapping(int ordinal)
        {
            AssetLibraryAsset assetLib;
            if (m_ordinalToLib.TryGetValue(ordinal, out assetLib))
            {
                assetLib.UnloadIDMappingFrom(m_mapping);
            }
            else
            {
                throw new ArgumentException(string.Format("Unable to find assetLibrary with ordinal = {0}", ordinal), "ordinal");
            }
        }


        private void UnloadMappings()
        {
            m_mapping = new MappingInfo();
        }

        private const long m_nullID = 1L << 32;
        private const long m_instanceIDMask = 1L << 33;
        private const long m_staticResourceIDMask = 1L << 34;
        private const long m_dynamicResourceIDMask = 1L << 36;

        public long NullID { get { return m_nullID; } }

        public virtual long CreateID(UnityObject obj)
        {
            return m_instanceIDMask | (0x00000000FFFFFFFFL & obj.GetInstanceID());
        }

        public bool IsNullID(long id)
        {
            return (id & m_nullID) != 0 || id <= 0;
        }

        public bool IsInstanceID(long id)
        {
            return (id & m_instanceIDMask) != 0;
        }

        public bool IsStaticResourceID(long id)
        {
            return (id & m_staticResourceIDMask) != 0;
        }

        public bool IsDynamicResourceID(long id)
        {
            return (id & m_dynamicResourceIDMask) != 0;
        }

        public long ToStaticResourceID(int ordinal, int id)
        {
            return ToID(ordinal, id, m_staticResourceIDMask);
        }

        public long ToDynamicResourceID(int ordinal, int id)
        {
            return ToID(ordinal, id, m_dynamicResourceIDMask);
        }

        private static long ToID(int ordinal, int id, long mask)
        {
            if (id > AssetLibraryInfo.ORDINAL_MASK)
            {
                throw new ArgumentException("id > AssetLibraryInfo.ORDINAL_MASK");
            }

            id = (ordinal << AssetLibraryInfo.ORDINAL_OFFSET) | (AssetLibraryInfo.ORDINAL_MASK & id);
            return mask | (0x00000000FFFFFFFFL & id);
        }

        public int ToOrdinal(long id)
        {
            int intId = (int)(0x00000000FFFFFFFFL & id);
            return (intId >> AssetLibraryInfo.ORDINAL_OFFSET) & AssetLibraryInfo.ORDINAL_MASK;
            
        }
        public int ToOrdinal(int id)
        {
            return (id >> AssetLibraryInfo.ORDINAL_OFFSET) & AssetLibraryInfo.ORDINAL_MASK;
        }

        public bool IsMapped(long id)
        {
            if (IsNullID(id))
            {
                return true;
            }
            //if (IsStaticFolderID(id))
            //{
            //    return true;
            //}
            //if (IsDynamicFolderID(id))
            //{
            //    return true;
            //}
            if (IsInstanceID(id))
            {
                int persistentID = ToInt(id);
                return m_persistentIDToSceneObject != null && m_persistentIDToSceneObject.ContainsKey(persistentID);
            }
            if (IsStaticResourceID(id))
            {
                int persistentID = ToInt(id);
                return m_mapping.PersistentIDtoObj.ContainsKey(persistentID);
            }
            if (IsDynamicResourceID(id))
            {
                int persistentID = ToInt(id);
                return m_persistentIDToDynamicResource.ContainsKey(persistentID);
            }
            return false;
        }


        public bool IsMapped(UnityObject uo)
        {
            if (uo == null)
            {
                return false;
            }

            int instanceID = uo.GetInstanceID();
            int persistentID;
            if (m_mapping.InstanceIDtoPID.TryGetValue(instanceID, out persistentID))
            {
                return true;
            }

            //if (m_sceneObjectIDToPersistentID != null && m_sceneObjectIDToPersistentID.TryGetValue(instanceID, out persistentID))
            //{
            //    return true;
            //}

            if (m_dynamicResourceIDToPersistentID.TryGetValue(instanceID, out persistentID))
            {
                return true;
            }

            return false;
        }

        public long ToID(UnityObject uo)
        {
            if(uo == null)
            {
                return m_nullID;
            }

            int instanceID = uo.GetInstanceID();
            int persistentID;
            if(m_mapping.InstanceIDtoPID.TryGetValue(instanceID, out persistentID))
            {
                return m_staticResourceIDMask | (0x00000000FFFFFFFFL & persistentID);
            }
            
            //if(m_sceneObjectIDToPersistentID != null && m_sceneObjectIDToPersistentID.TryGetValue(instanceID, out persistentID))
            //{
            //    return m_instanceIDMask | (0x00000000FFFFFFFFL & persistentID);
            //}

            if(m_dynamicResourceIDToPersistentID.TryGetValue(instanceID, out persistentID))
            {
                return m_dynamicResourceIDMask | (0x00000000FFFFFFFFL & persistentID);
            }

            return m_instanceIDMask | (0x00000000FFFFFFFFL & instanceID);
        }

        [Obsolete]
        public long[] ToID(UnityObject[] uo)
        {
            if(uo == null)
            {
                return null;
            }
            long[] ids = new long[uo.Length];
            for(int i = 0; i < uo.Length; ++i)
            {
                ids[i] = ToID(uo[i]);
            }
            return ids;
        }


        [Obsolete]
        public long[] ToID<T>(List<T> uo) where T : UnityObject
        {
            if (uo == null)
            {
                return null;
            }
            long[] ids = new long[uo.Count];
            for (int i = 0; i < uo.Count; ++i)
            {
                ids[i] = ToID(uo[i]);
            }
            return ids;
        }

        [Obsolete]
        public long[] ToID<T>(IEnumerable<T> uo) where T : UnityObject
        {
            if (uo == null)
            {
                return null;
            }
            List<long> ids = new List<long>();
            foreach(T obj in uo)
            {
                ids.Add(ToID(obj));
            }
            return ids.ToArray();
        }

        public int ToInt(long id)
        {
            return (int)(0x00000000FFFFFFFFL & id);
        }

        public T FromID<T>(long id) where T : UnityObject
        {
            if(IsNullID(id))
            {
                return null;
            }

            if(IsStaticResourceID(id))
            {
                UnityObject obj;
                int persistentID = ToInt(id);
                if (m_mapping.PersistentIDtoObj.TryGetValue(persistentID, out obj))
                {
                    return obj as T;
                }
            }
            else if (IsInstanceID(id))
            {
                UnityObject obj;
                int persistentID = ToInt(id);
                if (m_persistentIDToSceneObject != null && m_persistentIDToSceneObject.TryGetValue(persistentID, out obj))
                {
                    return obj as T;
                }
            }
            else if(IsDynamicResourceID(id))
            {
                UnityObject obj;
                int persistentID = ToInt(id);
                if (m_persistentIDToDynamicResource.TryGetValue(persistentID, out obj))
                {
                    return obj as T;
                }
            }
            return null;
        }

        [Obsolete]
        public T[] FromID<T>(long[] id) where T : UnityObject
        {
            if(id == null)
            {
                return null;
            }

            T[] objs = new T[id.Length];
            for(int i = 0; i < id.Length; ++i)
            {
                objs[i] = FromID<T>(id[i]);
            }
            return objs;
        }

        public bool TryToReplaceID(UnityObject uo, long persistentID)
        {
            if (uo == null)
            {
                return false;
            }

            int instanceID = uo.GetInstanceID();
            if (m_mapping.InstanceIDtoPID.ContainsKey(instanceID))
            {
                int id = ToInt(persistentID);
                if (id != m_mapping.InstanceIDtoPID[instanceID])
                {
                    m_mapping.InstanceIDtoPID[instanceID] = id;
                    return true;
                }
            }

            return false;
        }

        public void RegisterDynamicResource(long persistentID, UnityObject obj)
        {
            RegisterDynamicResource(ToInt(persistentID), obj);
        }

        public void UnregisterDynamicResource(long persistentID)
        {
            UnregisterDynamicResource(ToInt(persistentID));
        }

        public void RegisterDynamicResources(Dictionary<long, UnityObject> idToObj)
        {
            RegisterDynamicResources(idToObj.ToDictionary(kvp => ToInt(kvp.Key), kvp => kvp.Value));
        }

        public void UnregisterDynamicResources(Dictionary<long, UnityObject> idToObj)
        {
            UnregisterDynamicResources(idToObj.ToDictionary(kvp => ToInt(kvp.Key), kvp => kvp.Value));
        }


        [Obsolete]
        private const long m_dynamicFolderIDMask = 1L << 37;
        [Obsolete]
        private const long m_staticFolderIDMask = 1L << 35;
        [Obsolete]
        private const long m_sceneIDMask = 1L << 38;

        [Obsolete]
        public bool IsStaticFolderID(long id)
        {
            return (id & m_staticFolderIDMask) != 0;
        }

        [Obsolete]
        public bool IsDynamicFolderID(long id)
        {
            return (id & m_dynamicFolderIDMask) != 0;
        }

        [Obsolete]
        public bool IsSceneID(long id)
        {
            return (id & m_sceneIDMask) != 0;
        }

        [Obsolete]
        public bool IsResourceID(long id)
        {
            return IsStaticResourceID(id) || IsDynamicResourceID(id);
        }

        [Obsolete]
        public long ToStaticFolderID(int ordinal, int id)
        {
            return ToID(ordinal, id, m_staticFolderIDMask);
        }


        [Obsolete]
        public long ToDynamicFolderID(int ordinal, int id)
        {
            return ToID(ordinal, id, m_dynamicFolderIDMask);
        }

        [Obsolete]
        public long ToSceneID(int ordinal, int id)
        {
            return ToID(ordinal, id, m_sceneIDMask);
        }

        
        public int GetAssetLibraryID(long persistentID)
        {
            return -1;
        }

        public void RegisterStaticResource(int assetLibraryID, long persistentID, UnityObject obj)
        {
          
        }

        public void UnregisterStaticResource(long persistentID)
        {
            //Do nothing
        }

        public void UnregisterStaticResources()
        {
            //Do nothing
        }
    }
}
