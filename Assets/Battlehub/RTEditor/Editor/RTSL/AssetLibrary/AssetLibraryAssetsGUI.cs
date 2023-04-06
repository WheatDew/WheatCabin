using Battlehub.RTCommon.EditorTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSL
{
    public class AssetLibraryAssetsGUI 
    {
        [NonSerialized]
        private bool m_Initialized;
        //[SerializeField]
        private TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
        //[SerializeField]
        private MultiColumnHeaderState m_MultiColumnHeaderState;
        private SearchField m_SearchField;
        private AssetFolderInfo[] m_folders;
        private AssetLibraryAsset m_asset;
        private const string kSessionStateKeyPrefix = "AssetLibraryAssetsTVS";

        public AssetLibraryAssetsGUI()
        {
        }

        internal AssetTreeView TreeView { get; private set; }

        public void SetTreeAsset(AssetLibraryAsset asset)
        {
            m_asset = asset;
            m_Initialized = false;
        }

        public bool IsFolderSelected(AssetFolderInfo folder)
        {
            return m_folders != null && m_folders.Contains(folder);
        }

        public void SetSelectedFolders(AssetFolderInfo[] folders)
        {
            m_folders = folders;
            m_Initialized = false;
        }

        public void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();

                //var jsonState = SessionState.GetString(kSessionStateKeyPrefix + m_asset.GetInstanceID().ToString(), "");
                //if (!string.IsNullOrEmpty(jsonState))
                //    JsonUtility.FromJsonOverwrite(jsonState, m_TreeViewState);
              
                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = AssetTreeView.CreateDefaultMultiColumnHeaderState(0);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new MultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new TreeModel<AssetInfo>(GetData());

                TreeView = new AssetTreeView(
                    m_TreeViewState,
                    multiColumnHeader,
                    treeModel,
                    ExternalDropInside,
                    ExternalDropOutside);
                TreeView.Reload();

                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += TreeView.SetFocusAndEnsureSelectedItem;

                m_Initialized = true;
            }
        }

        public void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            if(TreeView != null && m_asset != null && TreeView.state != null)
            {
               // SessionState.SetString(kSessionStateKeyPrefix + m_asset.GetInstanceID().ToString(), JsonUtility.ToJson(TreeView.state));
            }
            
        }

        private void OnUndoRedoPerformed()
        {
            if (TreeView != null)
            {
                TreeView.treeModel.SetData(GetData());
                TreeView.Reload();
            }
        }

        private bool CanDrop(UnityObject obj)
        {
            if (obj is GameObject)
            {
                GameObject go = (GameObject)obj;
                if (go.transform.parent != null || !File.Exists(AssetDatabase.GetAssetPath(go)))
                {
                    return false;
                }
            }

            return obj != null && (!File.Exists(AssetDatabase.GetAssetPath(obj)) || File.Exists(AssetDatabase.GetAssetPath(obj)) && (!obj.GetType().Assembly.FullName.Contains("UnityEditor") || obj.GetType() == typeof(AnimatorController)));
        }

        private DragAndDropVisualMode CanDrop(TreeViewItem parent, int insertIndex)
        {
            if(m_folders == null || m_folders.Length != 1)
            {
                return DragAndDropVisualMode.None;
            }

            AssetInfo parentAssetInfo = GetAssetInfo(parent);
            if (parentAssetInfo != TreeView.treeModel.root)
            {
                return DragAndDropVisualMode.None;
            }

            if(m_folders == null || m_folders.Length != 1)
            {
                return DragAndDropVisualMode.None;
            }

            bool allFolders = true;
            bool cantDropAnything = DragAndDrop.objectReferences.All(o => !CanDrop(o));
            foreach (UnityObject dragged_object in DragAndDrop.objectReferences)
            {
                string path = AssetDatabase.GetAssetPath(dragged_object);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    allFolders = false;
                    break;
                }
            }

            if(allFolders || cantDropAnything)
            {
                return DragAndDropVisualMode.None;
            }

            AssetInfo parentAsset;
            if (parent == null)
            {
                parentAsset = TreeView.treeModel.root;
            }
            else
            {
                parentAsset = ((TreeViewItem<AssetInfo>)parent).data;
            }

            if (parentAsset.hasChildren)
            {
                var names = parentAsset.children.Select(c => c.name);
                if (DragAndDrop.objectReferences.Any(item => names.Contains(item.name)))
                {
                    return DragAndDropVisualMode.None;
                }
            }
            return DragAndDropVisualMode.Copy;
        }

        private DragAndDropVisualMode PerformDrop(TreeViewItem parent, int insertIndex)
        {
            DragAndDrop.AcceptDrag();

            List<UnityObject> assets = new List<UnityObject>();
            foreach (UnityObject dragged_object in DragAndDrop.objectReferences)
            {
                string path = AssetDatabase.GetAssetPath(dragged_object);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    assets.Add(dragged_object);
                }
            }


            AssetFolderInfo folder = m_folders[0];
            UnityObject[] assetsArray = assets.ToArray();
            bool moveToNewLocation = MoveToNewLocationDialog(assetsArray, folder);

            AddAssetToFolder(parent, insertIndex, assetsArray, folder, moveToNewLocation);
            return DragAndDropVisualMode.Copy;
        }

        public void AddAssetToFolder(UnityObject[] objects, AssetFolderInfo folder, bool moveToNewLocation)
        {
            AddAssetToFolder(null, -1, objects, folder, moveToNewLocation);
        }

        private void AddAssetToFolder(TreeViewItem parent, int insertIndex, UnityObject[] objects, AssetFolderInfo folder, bool moveToNewLocation)
        {
            for (int i = 0; i < objects.Length; ++i)
            {
                UnityObject obj = objects[i];
                if (obj == null || obj.GetType().Assembly.FullName.Contains("UnityEditor") && obj.GetType() != typeof(AnimatorController))
                {
                    continue;
                }
                AssetInfo parentAssetInfo = GetAssetInfo(parent);
                AssetInfo assetInfo;
                AssetFolderInfo existingFolder;
                AssetInfo existingAsset;
                if(m_asset.AssetLibrary.TryGetAssetInfo(obj, out existingFolder, out existingAsset))
                {
                    assetInfo = existingAsset;
                    if(!moveToNewLocation)
                    {
                        continue;
                    }

                    AddAssetToFolder(assetInfo, folder);

                    TreeView.treeModel.SetData(GetData());
                    TreeView.Reload();
                }
                else
                {
                    if(m_asset.AssetLibrary.Identity >= AssetLibraryInfo.MAX_ASSETS)
                    {
                        EditorUtility.DisplayDialog("Unable to add asset", string.Format("Max 'Indentity' value reached. 'Identity' ==  {0}", AssetLibraryInfo.MAX_ASSETS), "OK");
                        return;
                    }

                    if(!CreateAsset(obj, parentAssetInfo, insertIndex, folder))
                    {
                        EditorUtility.DisplayDialog("Unable to add asset", string.Format("Max 'Indentity' value reached. 'Identity' ==  {0}", AssetLibraryInfo.MAX_ASSETS), "OK");
                    }
                }
            }
        }

        private bool CreateAsset(UnityObject obj, AssetInfo parentAssetInfo, int insertIndex = -1, AssetFolderInfo folder = null)
        {        
            if (obj is GameObject)
            {
                GameObject go = (GameObject)obj;

                int identity = m_asset.AssetLibrary.Identity + 1;
                List<PrefabPartInfo> prefabParts = new List<PrefabPartInfo>();
                CreatePefabParts(go, ref identity, prefabParts);


                string prefabPath = AssetDatabase.GetAssetPath(go);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    UnityObject[] assetRepresentations = AssetDatabase.LoadAllAssetRepresentationsAtPath(prefabPath);
                    foreach (UnityObject assetRepresentation in assetRepresentations)
                    {
                        //Add avatar or mesh as prefab part
                        if (assetRepresentation is Avatar || assetRepresentation is Mesh || assetRepresentation is Material || assetRepresentation is AnimationClip)
                        {
                            PrefabPartInfo prefabPart = new PrefabPartInfo();
                            prefabPart.ParentPersistentID = -1;
                            prefabPart.PersistentID = identity;
                            prefabPart.Object = assetRepresentation;
                            prefabPart.Depth = 0;
                            identity++;
                            prefabParts.Add(prefabPart);
                        }
                    }
                }

                if (identity >= AssetLibraryInfo.MAX_ASSETS)
                {
                    return false;
                }

                AssetInfo assetInfo = CreateAsset(obj.name, parentAssetInfo, insertIndex, folder);
                assetInfo.Object = obj;
                AddAssetToFolder(assetInfo, folder);

                assetInfo.PrefabParts = prefabParts;
                m_asset.AssetLibrary.Identity = identity;

                if (folder != null)
                {
                    if (IsFolderSelected(folder))
                    {
                        if (assetInfo.PrefabParts != null)
                        {
                            Dictionary<int, AssetInfo> assets = new Dictionary<int, AssetInfo>();
                            assets.Add(-1, assetInfo);

                            for (int i = 0; i < assetInfo.PrefabParts.Count; ++i)
                            {
                                PrefabPartInfo prefabPart = assetInfo.PrefabParts[i];
                                string name;
                                if(prefabPart.Object == null)
                                {
                                    name = "<Null>";
                                }
                                else
                                {
                                    if(prefabPart.Object is Component)
                                    {
                                        name = prefabPart.Object.GetType().Name;
                                    }
                                    else
                                    {
                                        name = prefabPart.Object.name;
                                    }
                                }

                                AssetInfo prefabPartAssetInfo = new AssetInfo(name, assetInfo.depth + prefabPart.Depth, prefabPart.PersistentID);
                                prefabPartAssetInfo.Object = prefabPart.Object;

                                assets.Add(prefabPartAssetInfo.id, prefabPartAssetInfo);

                                TreeElement parent = assets[prefabPart.ParentPersistentID];
                                TreeView.treeModel.AddElement(prefabPartAssetInfo, parent, parent.children != null ? parent.children.Count : 0);
                            }
                        }
                    }
                }
            } 
            else
            {
                AssetInfo assetInfo = CreateAsset(obj.name, parentAssetInfo, insertIndex, folder);
                assetInfo.Object = obj;
                AddAssetToFolder(assetInfo, folder);
            }
            return true;
        }

        public static void CreatePefabParts(GameObject go, ref int identity, List<PrefabPartInfo> prefabParts, int parentId = -1, int depth = 0)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (Component component in components)
            {
                PrefabPartInfo componentPart = new PrefabPartInfo();
                componentPart.ParentPersistentID = parentId;
                componentPart.PersistentID = identity;
                componentPart.Object = component;
                componentPart.Depth = depth;
                identity++;
                prefabParts.Add(componentPart);
            }

            if (go.transform.childCount > 0)
            {
                foreach (Transform child in go.transform)
                {
                    PrefabPartInfo childPart = new PrefabPartInfo();
                    childPart.ParentPersistentID = parentId;
                    childPart.PersistentID = identity;
                    childPart.Object = child.gameObject;
                    childPart.Depth = depth;
                    identity++;
                    prefabParts.Add(childPart);
                    CreatePefabParts(child.gameObject, ref identity, prefabParts, childPart.PersistentID,  depth + 1);
                }
            }
        }

        public static void CreatePefabParts(GameObject go, ref int identity, Dictionary<UnityObject, int> objToId, List<PrefabPartInfo> prefabParts, int parentId = -1, int depth = 0)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (Component component in components)
            {
                PrefabPartInfo componentPart = new PrefabPartInfo();
                componentPart.ParentPersistentID = parentId;
                if(objToId.ContainsKey(component))
                {
                    componentPart.PersistentID = objToId[component];
                }
                else
                {
                    componentPart.PersistentID = identity;
                    identity++;
                }
                
                componentPart.Object = component;
                componentPart.Depth = depth;
                prefabParts.Add(componentPart);
            }

            if (go.transform.childCount > 0)
            {
                foreach (Transform child in go.transform)
                {
                    PrefabPartInfo childPart = new PrefabPartInfo();
                    childPart.ParentPersistentID = parentId;
                    if (objToId.ContainsKey(child.gameObject))
                    {
                        childPart.PersistentID = objToId[child.gameObject];
                    }
                    else
                    {
                        childPart.PersistentID = identity;
                        identity++;
                    }
                    
                    childPart.Object = child.gameObject;
                    childPart.Depth = depth;
                    prefabParts.Add(childPart);
                    CreatePefabParts(child.gameObject, ref identity, objToId, prefabParts, childPart.PersistentID, depth + 1);
                }
            }
        }

        public void AddAssetToFolder(AssetInfo assetInfo, AssetFolderInfo folder)
        {
            if (folder.Assets == null)
            {
                folder.Assets = new List<AssetInfo>();
            }

            if(assetInfo.Folder != null)
            {
                if(!m_folders.Contains(folder))
                {
                    TreeView.treeModel.RemoveElements(new[] { assetInfo.id });
                }
                else if(TreeView.treeModel.Find(assetInfo.id) == null)
                {
                    AssetInfo parent = (AssetInfo)assetInfo.parent;
                    if(parent == null)
                    {
                        parent = TreeView.treeModel.root;
                    }

                    TreeView.treeModel.AddElement(assetInfo, parent, parent.hasChildren ? parent.children.Count : 0);
                }
                assetInfo.Folder.Assets.Remove(assetInfo);
            }

            assetInfo.Folder = folder;
            folder.Assets.Add(assetInfo);
        }

        private AssetInfo CreateAsset(string name, TreeElement parent, int insertIndex = -1, AssetFolderInfo folder = null)
        {
            int depth = parent != null ? parent.depth + 1 : 0;
            int id = m_asset.AssetLibrary.Identity;
            m_asset.AssetLibrary.Identity++;
            var assetInfo = new AssetInfo(name, depth, id);
            if(folder != null)
            {
                if (IsFolderSelected(folder))
                {
                    TreeView.treeModel.AddElement(assetInfo, parent, insertIndex == -1 ?
                        parent.hasChildren ?
                        parent.children.Count
                        : 0
                        : insertIndex);

                    // Select newly created element

                    if(depth == 0)
                    {
                        TreeView.SetSelection(new[] { id }, TreeViewSelectionOptions.RevealAndFrame);
                    }
                }
            }
            
            return assetInfo;
        }

        private AssetInfo GetAssetInfo(TreeViewItem treeViewItem)
        {
            return treeViewItem != null ? ((TreeViewItem<AssetInfo>)treeViewItem).data : TreeView.treeModel.root;
        }

        private DragAndDropVisualMode ExternalDropInside(TreeViewItem parent, int insertIndex, bool performDrop)
        {
            if (performDrop)
            {
                return PerformDrop(parent, insertIndex);
            }
            return CanDrop(parent, insertIndex);
        }

        private DragAndDropVisualMode ExternalDropOutside(TreeViewItem parent, int insertIndex, bool performDrop)
        {
            if (performDrop)
            {
                return PerformDrop(parent, insertIndex);
            }
            return CanDrop(parent, insertIndex);
        }

        private IList<AssetInfo> GetData()
        {
            if (m_folders != null)
            {
                List<AssetInfo> result = new List<AssetInfo>();

                AssetInfo root = new AssetInfo
                {
                    id = 0,
                    name = "Root",
                    IsEnabled = false,
                    depth = -1,
                };

                result.Add(root);
                foreach (AssetInfo assetInfo in m_folders.Where(folder => folder.Assets != null).SelectMany(folder => folder.Assets))
                {
                    assetInfo.parent = root;
                    result.Add(assetInfo);
                 
                    if (assetInfo.PrefabParts != null)
                    {
                        Dictionary<int, AssetInfo> assets = new Dictionary<int, AssetInfo>();
                        assets.Add(-1, assetInfo);

                        for (int i = 0; i < assetInfo.PrefabParts.Count; ++i)
                        {
                            PrefabPartInfo prefabPart = assetInfo.PrefabParts[i];
                            string name;
                            if (prefabPart.Object == null)
                            {
                                name = "<Null>";
                            }
                            else
                            {
                                if (prefabPart.Object is Component)
                                {
                                    name = prefabPart.Object.GetType().Name;
                                }
                                else
                                {
                                    name = prefabPart.Object.name;
                                }
                            }

                            AssetInfo prefabPartAssetInfo = new AssetInfo(name, assetInfo.depth + prefabPart.Depth + 1, prefabPart.PersistentID);
                            prefabPartAssetInfo.Object = prefabPart.Object;
                            
                            assets.Add(prefabPartAssetInfo.id, prefabPartAssetInfo);

                            if(assets.ContainsKey(prefabPart.ParentPersistentID))
                            {
                                TreeElement parent = assets[prefabPart.ParentPersistentID];
                                prefabPartAssetInfo.parent = parent;
                                result.Add(prefabPartAssetInfo);
                            }
                        }
                    }

                }
                return result;
            }

            return new List<AssetInfo>
            {
                new AssetInfo
                {
                    id = 0,
                    name = "Root",
                    IsEnabled = false,
                    depth = -1,
                }
            };
        }

        private void SearchBar()
        {
            Rect rect = EditorGUILayout.GetControlRect();
            TreeView.searchString = m_SearchField.OnGUI(rect, TreeView.searchString);
        }

        private void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 0, GUILayout.MaxHeight((Screen.height - 300) * 0.5f));
            TreeView.OnGUI(rect);
        }

        private bool m_hasSelectedRootItems;
        private int m_selectedRootItemsCount;
        private void DoCommands()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == KeyCode.Delete)
                        {
                            if (TreeView.HasFocus())
                            {
                                RemoveAsset();
                            }
                        }
                        break;
                    }
            }

            EditorGUILayout.BeginHorizontal();

            int selectedRootItemsCount = TreeView.SelectedRootItemsCount;
            bool hasSelectedRootItems = TreeView.HasSelectedRootItems;

            if(m_folders != null && m_folders.Length == 1)
            {
                if (GUILayout.Button("Add Asset"))
                {
                    PickObject();
                }
            }

            if (m_selectedRootItemsCount == 1)
            {
                if (GUILayout.Button("Rename Asset"))
                {
                    RenameAsset();
                }

                if(GUILayout.Button("Ping Asset"))
                {
                    PingAsset();
                }
            }
            m_selectedRootItemsCount = TreeView.SelectedRootItemsCount;
                        
            if (m_hasSelectedRootItems)
            {
                if (GUILayout.Button("Remove Asset"))
                {
                    RemoveAsset();
                }
            }
            m_hasSelectedRootItems = TreeView.HasSelectedRootItems;

            EditorGUILayout.EndHorizontal();

            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == m_currentPickerWindow)
            {
                m_pickedObject = EditorGUIUtility.GetObjectPickerObject();
            }
            else
            {
                if (Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == m_currentPickerWindow)
                {
                    m_currentPickerWindow = -1;
                    if (m_pickedObject != null)
                    {
                        if (m_folders[0].Assets == null || !m_folders[0].Assets.Any(a => a.Object == m_pickedObject || a.Object != null && a.Object.name == m_pickedObject.name && a.Object.GetType() == m_pickedObject.GetType()))
                        {
                            if (m_pickedObject == null || m_pickedObject.GetType().Assembly.FullName.Contains("UnityEditor") && m_pickedObject.GetType() != typeof(AnimatorController))
                            {
                                EditorUtility.DisplayDialog("Unable to add asset",
                                   string.Format("Unable to add asset {0} from assembly {1}", m_pickedObject.GetType().Name, m_pickedObject.GetType().Assembly.GetName()), "OK");
                            }
                            else
                            {
                                bool moveToNewLocation = MoveToNewLocationDialog(new[] { m_pickedObject }, m_folders[0]);


                                AddAssetToFolder(new[] { m_pickedObject }, m_folders[0], moveToNewLocation);
                            }
                        }
                        m_pickedObject = null;
                    }
                }
            }
        }


        private bool MoveToNewLocationDialog(UnityObject[] assets, AssetFolderInfo folder)
        {
            bool moveToNewLocation = true;
            bool moveDialogDisplayed = false;
            foreach (UnityObject asset in assets)
            {
                if (!moveDialogDisplayed)
                {
                    AssetFolderInfo existingFolder;
                    AssetInfo existingAsset;
                    if (m_asset.AssetLibrary.TryGetAssetInfo(asset, out existingFolder, out existingAsset))
                    {
                        if(existingFolder != folder)
                        {
                            moveToNewLocation = EditorUtility.DisplayDialog(
                                                       "Same asset already added",
                                                       "Same asset already added to asset library. Do you want to move it to new location?", "Yes", "No");
                            moveDialogDisplayed = true;
                        }  
                    }
                }
            }

            return moveToNewLocation;
        }

        private UnityObject m_pickedObject;
        private int m_currentPickerWindow;
        private void PickObject()
        {
            m_currentPickerWindow = GUIUtility.GetControlID(FocusType.Passive) + 100;
            EditorGUIUtility.ShowObjectPicker<UnityObject>(null, false, string.Empty, m_currentPickerWindow);
        }

        private void PingAsset()
        {
            var selection = TreeView.GetSelection();
            if(selection != null && selection.Count > 0)
            {
                AssetInfo assetInfo = TreeView.treeModel.Find(selection[0]);
                if(assetInfo != null && assetInfo.Object != null)
                {
                    EditorGUIUtility.PingObject(assetInfo.Object);
                }
            }
        }

        private void RenameAsset()
        {
            var selection = TreeView.GetSelection();
            if (selection != null && selection.Count > 0)
            {
                TreeView.BeginRename(selection[0]);
            }
        }

        private void FlattenItems(AssetInfo assetInfo, List<AssetInfo> assets)
        {
            if (assetInfo == null)
            {
                return;
            }

            if (assetInfo.children != null)
            {
                for (int i = 0; i < assetInfo.children.Count; ++i)
                {
                    FlattenItems((AssetInfo)assetInfo.children[i], assets);
                }
            }

            assets.Add(assetInfo);
        }

        private void RemoveFromFolder(AssetInfo assetInfo, AssetFolderInfo folder)
        {
            List<AssetInfo> assetsToRemove = new List<AssetInfo>();
            FlattenItems(assetInfo, assetsToRemove);
            DoRemove(assetsToRemove.ToArray());
        }

        private void DoRemove(AssetInfo[] assetInfo)
        {
            for(int i = 0; i < assetInfo.Length; ++i)
            {
                if(assetInfo[i].Folder != null)
                {
                    assetInfo[i].Folder.Assets.Remove(assetInfo[i]);
                    assetInfo[i].Folder = null;
                }
            }

            TreeElement parent = assetInfo[0].parent;
            int index = parent.children.IndexOf(assetInfo[0]);

            TreeView.treeModel.RemoveElements(assetInfo.Select( a => a.id).ToArray());

            if (index >= parent.children.Count)
            {
                index = parent.children.Count - 1;
            }

            if (index >= 0)
            {
                TreeView.SetSelection(new int[] { parent.children[index].id }, TreeViewSelectionOptions.FireSelectionChanged);
            }
            else
            {
                if (parent != null)
                {
                    TreeView.SetSelection(new int[] { parent.id }, TreeViewSelectionOptions.FireSelectionChanged);
                }
                else
                {
                    TreeView.SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
                }

            }
        }

        private void RemoveAsset()
        {
            Undo.RecordObject(m_asset, "Remove Asset");
            IList<int> selection = TreeView.GetSelection();
          
            foreach(int selectedId in selection)
            {
                AssetInfo assetInfo = TreeView.treeModel.Find(selectedId);
                if(assetInfo != null)
                {
                    if(assetInfo.Folder != null && assetInfo.depth == 0)
                    {
                        RemoveFromFolder(assetInfo, assetInfo.Folder);
                    }
                }   
            }

            
            TreeView.Reload();
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            InitIfNeeded();
            EditorGUILayout.Space();
            SearchBar();
            EditorGUILayout.Space();
            DoTreeView();
            EditorGUILayout.Space();
            DoCommands();
            EditorGUILayout.EndVertical();
        }
    }
}
