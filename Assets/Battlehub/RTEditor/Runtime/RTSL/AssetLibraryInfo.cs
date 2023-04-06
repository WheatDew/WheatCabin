using Battlehub.RTCommon.EditorTreeView;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSL
{
    [Serializable]
    public class PrefabPartInfo 
    {
        public int ParentPersistentID;
        public int PersistentID;
        public UnityObject Object;
        public int Depth;
    }

    
    [Serializable]
    public class AssetInfo : TreeElement
    {
        [NonSerialized]
        [HideInInspector]
        public AssetFolderInfo Folder;
        public UnityObject Object;
        
        public int PersistentID
        {
            get { return id; }
        }
        public bool IsEnabled;

        public List<PrefabPartInfo> PrefabParts;

        public AssetInfo()
        {

        }

        public AssetInfo(string name, int depth, int id) : base(name, depth, id)
        {
        }

        public void Sync(AssetLibraryInfo assetLibraryInfo)
        {
            if (Object == null)
            {
                return;
            }

            GameObject go = Object as GameObject;
            if (go == null)
            {
                return;
            }

            List<PrefabPartInfo> newPrefabParts = new List<PrefabPartInfo>();

            Dictionary<UnityObject, PrefabPartInfo> objToParts = PrefabParts != null ?
                PrefabParts.Where(part => part.Object != null).ToDictionary(part => part.Object) :
                new Dictionary<UnityObject, PrefabPartInfo>();

            Sync(assetLibraryInfo, -1, 0, go, objToParts, newPrefabParts);

            PrefabParts = newPrefabParts;
        }

        private void Sync(
           AssetLibraryInfo assetLibraryInfo,
           int parentPersistentID,
           int depth,
           GameObject go,
           Dictionary<UnityObject, PrefabPartInfo> objToParts,
           List<PrefabPartInfo> newPrefabs)
        {
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i)
            {
                Component component = components[i];
                if(component == null)
                {
                    continue;
                }
                PrefabPartInfo part;
                if (objToParts.TryGetValue(component, out part))
                {
                    newPrefabs.Add(part);
                }
                else
                {
                    part = new PrefabPartInfo
                    {
                        Depth = depth,
                        Object = component,
                        PersistentID = assetLibraryInfo.Identity,
                        ParentPersistentID = parentPersistentID
                    };
                    assetLibraryInfo.Identity++;
                    newPrefabs.Add(part);
                }
            }

            foreach (Transform child in go.transform)
            {
                GameObject childGo = child.gameObject;
                PrefabPartInfo part;
                if (objToParts.TryGetValue(childGo, out part))
                {
                    newPrefabs.Add(part);
                }
                else
                {
                    part = new PrefabPartInfo
                    {
                        Depth = depth,
                        Object = childGo,
                        PersistentID = assetLibraryInfo.Identity,
                        ParentPersistentID = parentPersistentID
                    };
                    assetLibraryInfo.Identity++;
                    newPrefabs.Add(part);
                }

                Sync(assetLibraryInfo, part.PersistentID, depth + 1, childGo, objToParts, newPrefabs);  
            }
        }

        public bool IsSyncRequired()
        {
            if(Object == null)
            {
                return false;
            }

            GameObject go = Object as GameObject;
            if(go == null)
            {
                return false;
            }

            PrefabPartInfo rootPart = new PrefabPartInfo
            {
                Object = go,
                Depth = -1,
                ParentPersistentID = -1,
                PersistentID = -1,
            };

            Dictionary<UnityObject, PrefabPartInfo> objToParts = PrefabParts != null ?
                PrefabParts.Where(part => part.Object != null).ToDictionary(part => part.Object) :
                new Dictionary<UnityObject, PrefabPartInfo>();

            objToParts.Add(go, rootPart);

            Dictionary<int, PrefabPartInfo> idToParts = PrefabParts != null ?
                PrefabParts.Where(part => part.Object != null).ToDictionary(part => part.PersistentID) :
                new Dictionary<int, PrefabPartInfo>();

            idToParts.Add(rootPart.PersistentID, rootPart);

            return IsSyncRequired(go, objToParts, idToParts);
        }

        private bool IsSyncRequired(GameObject go, 
            Dictionary<UnityObject, PrefabPartInfo> objToParts,
            Dictionary<int, PrefabPartInfo> idToParts)
        {
            Component[] components = go.GetComponents<Component>();
            for(int i = 0; i < components.Length; ++i)
            {
                Component component = components[i];
                if(component == null)
                {
                    return true;
                }
                PrefabPartInfo part;
                if(!objToParts.TryGetValue(component, out part))
                {
                    return true;
                }

                PrefabPartInfo parentPart;
                if(!idToParts.TryGetValue(part.ParentPersistentID, out parentPart))
                {
                    return true;
                }

                if(parentPart.Object != go)
                {
                    return true;
                }
            }

            foreach(Transform child in go.transform)
            {
                GameObject childGo = child.gameObject;
                PrefabPartInfo part;
                if (!objToParts.TryGetValue(childGo, out part))
                {
                    return true;
                }

                PrefabPartInfo parentPart;
                if (!idToParts.TryGetValue(part.ParentPersistentID, out parentPart))
                {
                    return true;
                }

                if (parentPart.Object != go)
                {
                    return true;
                }

                if(IsSyncRequired(childGo, objToParts, idToParts))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public class AssetFolderInfo : TreeElement, ISerializationCallbackReceiver
    {
        public List<AssetInfo> Assets;
        public bool IsEnabled;

        public AssetFolderInfo()
        {

        }

        public AssetFolderInfo(string name, int depth, int id) : base(name, depth, id)
        {
        }

        public void OnAfterDeserialize()
        {
            for(int i = 0; i < Assets.Count; ++i)
            {
                AssetInfo assetInfo = Assets[i];
                if(assetInfo != null)
                {
                    assetInfo.Folder = this;
                }
            }
        }

        public void OnBeforeSerialize()
        {
           
        }
    }

    [Serializable]
    public class AssetLibraryInfo : TreeElement
    {
        public const int ORDINAL_OFFSET = 16;
        public const int MAX_ASSETS = 1 << ORDINAL_OFFSET - 1;
        public const int ORDINAL_MASK = 0x0000FFFF;
        public const int MAX_FOLDERS = 1 << ORDINAL_OFFSET - 1;
        public const int INITIAL_ID = 4;

        public const int STATICLIB_FIRST = 0;
        public const int STATICLIB_LAST = (ORDINAL_MASK - 2535) / 3 - 1;
        public const int MAX_STATICLIBS = (STATICLIB_LAST - STATICLIB_FIRST) + 1;

        public const int BUILTIN_FIRST = STATICLIB_LAST + 1;
        public const int BUILTIN_LAST = BUILTIN_FIRST + 35 - 1;
        public const int MAX_BUILTINLIBS = (BUILTIN_LAST - BUILTIN_FIRST) + 1;

        public const int SCENELIB_FIRST = BUILTIN_LAST + 1;
        public const int SCENELIB_LAST = SCENELIB_FIRST + 2500 - 1;
        public const int MAX_SCENELIBS = (SCENELIB_LAST - SCENELIB_FIRST) + 1;

        public const int BUNDLEDLIB_FIRST = SCENELIB_LAST + 1;
        public const int BUNDLEDLIB_LAST = BUNDLEDLIB_FIRST + (ORDINAL_MASK - 2535) / 3 - 1;
        public const int MAX_BUNDLEDLIBS = (BUNDLEDLIB_LAST - BUNDLEDLIB_FIRST) + 1;

        public const int DYNAMICLIB_FIRST = BUNDLEDLIB_LAST + 1;
        public const int DYNAMICLIB_LAST = DYNAMICLIB_FIRST + (ORDINAL_MASK - 2535) / 3 - 1;
        public const int MAX_DYNAMICLIBS = (DYNAMICLIB_LAST - DYNAMICLIB_FIRST) + 1;

        public int Identity = INITIAL_ID;
        public int FolderIdentity = INITIAL_ID;

        public List<AssetFolderInfo> Folders;

        public bool Contains(UnityObject obj)
        {
            AssetFolderInfo folder;
            AssetInfo asset;
            return TryGetAssetInfo(obj, out folder, out asset);
        }

        public bool TryGetAssetInfo(UnityObject obj, out AssetFolderInfo resultFolder, out AssetInfo resultAsset)
        {
            for(int i = 0; i < Folders.Count; ++i)
            {
                AssetFolderInfo folder = Folders[i];
                if(folder != null && TryGetAssetInfo(folder, obj, out resultFolder, out resultAsset))
                {
                    return true;
                }
            }
            resultAsset = null;
            resultFolder = null;
            return false;
        }

        private bool TryGetAssetInfo(AssetFolderInfo folder, UnityObject obj, out AssetFolderInfo resultFolder, out AssetInfo resultAsset)
        {
            if(folder.Assets != null)
            {
                for(int i = 0; i < folder.Assets.Count; ++i)
                {
                    AssetInfo asset = folder.Assets[i];
                    if(asset.Object == obj)
                    {
                        resultFolder = folder;
                        resultAsset = asset;
                        return true;
                    }
                }
            }

            if(folder.hasChildren)
            {
                for(int i = 0; i < folder.children.Count; ++i)
                {
                    AssetFolderInfo subfolder = (AssetFolderInfo)folder.children[i];
                    if(TryGetAssetInfo(subfolder, obj, out resultFolder, out resultAsset))
                    {
                        return true;
                    }   
                }
            }

            resultAsset = null;
            resultFolder = null;
            return false;
        }

        public void BuildTree()
        {
            if(Folders == null || Folders.Count == 0)
            {
                return;
            }
            AssetFolderInfo root = Folders[0];
            if(root.depth != -1)
            {
                throw new InvalidOperationException("Unable to build AssetLibraryInfo tree -> root.depth != -1");
            }
            BuildSubtree(root, 1);
        }

        private int BuildSubtree(AssetFolderInfo parent, int startIndex)
        {
            parent.children = new List<TreeElement>();
            for (int i = startIndex; i < Folders.Count; ++i)
            {
                AssetFolderInfo folder = Folders[i];
                if (folder == null)
                {
                    continue;
                }

                if(folder.depth == parent.depth + 1)
                {
                    parent.children.Add(folder);
                }
                else if(folder.depth == parent.depth + 2)
                {
                    i = BuildSubtree(Folders[i - 1], i);
                }
                else if(folder.depth > parent.depth + 2)
                {
                    throw new InvalidOperationException("Unable to build AssetLibraryInfo tree -> folder.depth > parent.depth + 2");
                }
                else
                {
                    return i - 1;
                }
            }

            return Folders.Count;
        }

    }
}
