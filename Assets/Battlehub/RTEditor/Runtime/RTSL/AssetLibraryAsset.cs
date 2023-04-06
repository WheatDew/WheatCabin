using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL
{
    public class MappingInfo
    {
        public class IDLists
        {
            public readonly List<int> InstanceIDs = new List<int>();
            public readonly List<int> PersistentIDs = new List<int>();
            
            public IDLists(List<int> instanceIDs, List<int> persistentIDs)
            {
                InstanceIDs = instanceIDs;
                PersistentIDs = persistentIDs;
            }
        }
        
        public readonly Dictionary<int, int> InstanceIDtoPID = new Dictionary<int, int>();
        public readonly Dictionary<int, UnityObject> PersistentIDtoObj = new Dictionary<int, UnityObject>();
        public readonly Dictionary<AssetLibraryAsset, IDLists> LibToIDs = new Dictionary<AssetLibraryAsset, IDLists>();

        public void Add(AssetLibraryAsset lib, List<int> instanceIDs, List<int> persistentIDs)
        {
            try
            {
                LibToIDs.Add(lib, new IDLists(instanceIDs, persistentIDs));
            }
            catch (ArgumentException)
            {
                Debug.LogWarningFormat("Asset library {0} ordinal {1} already added.", lib.name, lib.Ordinal);
                throw;
            }
        }
        
        public void Add(int instanceID, int persistentID)
        {
            try
            {
                InstanceIDtoPID.Add(instanceID, persistentID);
            }
            catch(ArgumentException)
            {
                Debug.LogWarningFormat("An element with instanceId = {0} already exists. mappedId = {1}", instanceID, persistentID);
                throw;
            }
        }

        public void Add(int persistentID, UnityObject obj)
        {
            try
            {
                PersistentIDtoObj.Add(persistentID, obj);
            }
            catch (ArgumentException)
            {
                UnityObject existingObj = PersistentIDtoObj[persistentID];
                string existingObjStr = existingObj != null ? existingObj.GetType().Name + ", name: " + existingObj.name + ", InstanceID: " + existingObj.GetInstanceID() : "null";
                string newObjStr = obj != null ? obj.GetType().Name + ", name: " + obj.name + ", InstanceID: " + obj.GetInstanceID() : "null";
                Debug.LogWarningFormat("An element with mappedId = {0} already exists. existing obj = {1}; new obj = {2}", persistentID, existingObjStr, newObjStr);
                throw;
            }
        }
    }

    public class AssetLibraryAsset : ScriptableObject
    {
        [SerializeField]
        private int m_offset;
        [SerializeField]
        private int m_ordinal;
        public int Ordinal
        {
            get { return m_ordinal; }
            set
            {
                m_ordinal = value;
                m_offset = m_ordinal << AssetLibraryInfo.ORDINAL_OFFSET;
            }
        }
        
        [SerializeField]
        private AssetLibraryInfo m_assetLibrary;

        public AssetLibraryInfo AssetLibrary
        {
            get { return m_assetLibrary; }
            set { m_assetLibrary = value; }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Event.current != null && Event.current.commandName == "Duplicate")
            {
                int identity = AssetLibrariesListGen.GetIdentity();
                Ordinal = identity;
                EditorCoroutine.Start(CoUpdateList(identity));
            }
        }

        private IEnumerator CoUpdateList(int identity)
        {
            yield return null;
            AssetLibrariesListGen.UpdateList(identity + 1);
        }
#endif

        public void UnloadIDMappingFrom(MappingInfo mapping)
        {
            MappingInfo.IDLists idLists;
            if (mapping.LibToIDs.TryGetValue(this, out idLists))
            {
                for(int i = 0; i < idLists.InstanceIDs.Count; ++i)
                {
                    int instanceID = idLists.InstanceIDs[i];
                    mapping.InstanceIDtoPID.Remove(instanceID);
                }

                for(int i = 0; i < idLists.PersistentIDs.Count; ++i)
                {
                    int persistentID = idLists.PersistentIDs[i];
                    mapping.PersistentIDtoObj.Remove(persistentID);
                }

                mapping.LibToIDs.Remove(this);
            }
        }

        public void LoadIDMappingTo(MappingInfo mapping, bool IIDtoPID, bool PIDtoObj)
        {
            if(!IIDtoPID && !PIDtoObj)
            {
                return;
            }

            if(m_assetLibrary == null || m_assetLibrary.Folders == null || m_assetLibrary.Folders.Count == 0)
            {
                return;
            }

            List<int> instanceIDs = new List<int>();
            List<int> persistentIDs = new List<int>();

            for (int i = 0; i < m_assetLibrary.Folders.Count; ++i)
            {
                AssetFolderInfo folder = m_assetLibrary.Folders[i];
                if(folder != null)
                {
                    if (folder.Assets != null && folder.Assets.Count > 0)
                    {
                        for (int j = 0; j < folder.Assets.Count; ++j)
                        {
                            AssetInfo asset = folder.Assets[j];
                            if (asset.Object != null)
                            {
                                LoadIDMappingTo(asset, mapping, instanceIDs, persistentIDs, IIDtoPID, PIDtoObj);
                            }
                        }
                    }
                }
            }

            mapping.Add(this, instanceIDs, persistentIDs);
        }

        private void LoadIDMappingTo(AssetInfo asset,
            MappingInfo mapping,
            List<int> instanceIDs,
            List<int> persistentIDs,
            bool IIDtoPID, bool PIDtoObj)
        {

            int offset = m_ordinal << AssetLibraryInfo.ORDINAL_OFFSET;
            if (IIDtoPID)
            {
                int instanceID = asset.Object.GetInstanceID();

                //Following if statement required because object can be added to multiple libraries and 
                //have multiple persistetnt identifiers. Only first persistent identifier is added to dictionary.
                //Any persistent id can be converted to "first" persistent id using following approach:
                //(PersistenID -> Obj) -> (InstanceID -> First PersistentID)
                if (!mapping.InstanceIDtoPID.ContainsKey(instanceID)) 
                {
                    mapping.Add(instanceID, offset + asset.PersistentID);
                    instanceIDs.Add(instanceID);
                }
            }

            if (PIDtoObj)
            {
                int persistentID = offset + asset.PersistentID;
                mapping.Add(persistentID, asset.Object);
                persistentIDs.Add(persistentID);
            }

            if (asset.PrefabParts != null)
            {
                for (int i = 0; i < asset.PrefabParts.Count; ++i)
                {
                    PrefabPartInfo prefabPart = asset.PrefabParts[i];
                    if(prefabPart != null && prefabPart.Object != null)
                    {
                        if (IIDtoPID)
                        {
                            int instanceID = prefabPart.Object.GetInstanceID();
                            if(!mapping.InstanceIDtoPID.ContainsKey(instanceID))
                            {
                                mapping.Add(instanceID, offset + prefabPart.PersistentID);
                                instanceIDs.Add(instanceID);
                            }
                        }

                        if (PIDtoObj)
                        {
                            int persistentID = offset + prefabPart.PersistentID;
                            mapping.Add(persistentID, prefabPart.Object);
                            persistentIDs.Add(persistentID);
                        }
                    }
                }
            }
        }

        public bool IsSyncRequired()
        {
            bool syncIsNotRequired = Foreach(assetInfo =>
            {
                return !assetInfo.IsSyncRequired();
            });

            return !syncIsNotRequired;
        }

        public void Sync()
        {
            Foreach(assetInfo =>
            {
                assetInfo.Sync(m_assetLibrary);
                return true;
            });
        }

        public bool Foreach(Func<AssetInfo, bool> callback)
        {
            if(m_assetLibrary.Folders != null)
            {
                for(int i = 0; i < m_assetLibrary.Folders.Count; ++i)
                {
                    AssetFolderInfo folder = m_assetLibrary.Folders[i];
                    if(!Foreach(folder, callback))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool Foreach(AssetFolderInfo folder, Func<AssetInfo, bool> callback)
        {
            if(folder.Assets != null)
            {
                for(int i = 0; i < folder.Assets.Count; ++i)
                {
                    AssetInfo assetInfo = folder.Assets[i];
                    if(assetInfo != null)
                    {
                        if(!callback(assetInfo))
                        {
                            return false;
                        }
                    }
                }
            }

            if(folder.hasChildren)
            {
                for(int i = 0; i < folder.children.Count; ++i)
                {
                    AssetFolderInfo childFolder = folder.children[i] as AssetFolderInfo;
                    if(childFolder != null)
                    {
                        if(!Foreach(childFolder, callback))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void Awake()
        {
            if (m_assetLibrary == null || m_assetLibrary.Folders.Count == 0)
            {
                AssetFolderInfo rootFolder = new AssetFolderInfo
                {
                    name = "Root",
                    depth = -1,
                };

                AssetFolderInfo assetsFolder = new AssetFolderInfo("Assets", 0, 3);
                assetsFolder.IsEnabled = true;
                
                m_assetLibrary = new AssetLibraryInfo
                {
                    name = "Root",
                    depth = -1,
                    Folders = new List<AssetFolderInfo>
                    {
                        rootFolder,
                        assetsFolder
                    },
                };
            }
          
        }
    }
}
