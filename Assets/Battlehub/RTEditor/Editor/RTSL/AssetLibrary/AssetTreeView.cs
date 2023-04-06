using Battlehub.RTCommon.EditorTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;


namespace Battlehub.RTSL
{
    internal class AssetTreeView : TreeViewWithTreeModel<AssetInfo>
    {
        private const float kRowHeights = 20f;
        private const float kIconWidth = 18f;

        //private static Texture2D[] s_icons =
        //{
        //    EditorGUIUtility.FindTexture ("Folder Icon")
        //};

        private enum Columns
        {
            Name,
            PersistentID,
           // ExposeToEditor,
        }

        private Func<TreeViewItem, int, bool, DragAndDropVisualMode> m_externalDropInside;
        private Func<TreeViewItem, int, bool, DragAndDropVisualMode> m_externalDropOutside;

        private readonly Dictionary<int, AssetInfo> m_selection = new Dictionary<int, AssetInfo>();
        private readonly Dictionary<int, AssetInfo> m_selectedRootItems = new Dictionary<int, AssetInfo>();
      
        public bool HasSelectedRootItems
        {
            get { return m_selectedRootItems.Count > 0; }
        }

        public int SelectedRootItemsCount
        {
            get { return m_selectedRootItems.Count; }
        }
        
        public AssetTreeView(
            TreeViewState state,
            MultiColumnHeader multiColumnHeader,
            TreeModel<AssetInfo> model,
            Func<TreeViewItem, int, bool, DragAndDropVisualMode> externalDropInside,
            Func<TreeViewItem, int, bool, DragAndDropVisualMode> externalDropOutside) : base(state, multiColumnHeader, model)
        {
            m_externalDropInside = externalDropInside;
            m_externalDropOutside = externalDropOutside;

            rowHeight = kRowHeights;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;

            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = kIconWidth;

            Reload();
        }

        
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<AssetInfo>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
            }
        }


        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            HandleSelectionChanged(selectedIds);
        }

        private void HandleSelectionChanged(IList<int> selectedIds)
        {
            HashSet<int> idsHs = new HashSet<int>(selectedIds);
            foreach (int id in m_selection.Keys.ToArray())
            {
                if (!idsHs.Contains(id))
                {
                    m_selection.Remove(id);
                    m_selectedRootItems.Remove(id);
                }
            }

            for (int i = 0; i < selectedIds.Count; ++i)
            {
                int id = selectedIds[i];
                if (!m_selection.ContainsKey(id))
                {
                    AssetInfo assetInfo = treeModel.Find(id);
                    if(assetInfo != null)
                    {
                        m_selection.Add(id, assetInfo);
                        if (assetInfo.depth == 0)
                        {
                            m_selectedRootItems.Add(id, assetInfo);
                        }
                    }
                }
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem buildRoot =  base.BuildRoot();

            HandleSelectionChanged(GetSelection());

            return buildRoot;
        }

        private void CellGUI(Rect cellRect, TreeViewItem<AssetInfo> item, Columns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {

                case Columns.Name:
                    {
                        // Do toggle
                        Rect iconRect = cellRect;
                        iconRect.x += GetContentIndent(item);
                        iconRect.width = kIconWidth;
                        if (iconRect.xMax < cellRect.xMax)
                        {
                            if(item.data.Object != null)
                            {
                                GUI.DrawTexture(iconRect, AssetPreview.GetMiniThumbnail(item.data.Object), ScaleMode.ScaleToFit);
                            }
                        }

                        // Default icon and label
                        args.rowRect = cellRect;
                        base.RowGUI(args);
                    }
                    break;
                case Columns.PersistentID:
                    {
                        EditorGUI.LabelField(cellRect, item.data.PersistentID.ToString());
                    }
                    break;
                //case Columns.ExposeToEditor:
                //    {
                //        if(item.data.depth == 0)
                //        {
                //            item.data.IsEnabled = EditorGUI.Toggle(cellRect, item.data.IsEnabled);
                //        }
                //    }
                    //break;
            }
        }


        protected override bool CanRename(TreeViewItem item)
        {
            if(item.depth != 0)
            {
                return false;
            }

            // Only allow rename if we can show the rename overlay with a certain width (label might be clipped by other columns)
            Rect renameRect = GetRenameRect(treeViewRect, 0, item);
            return renameRect.width > 30;
        }


        public bool BeginRename(int id)
        {
            TreeViewItem item = FindItem(id, rootItem);
            if (item == null)
            {
                return false;
            }
            return BeginRename(item);
        }

        public TreeViewItem FindItem(int id)
        {
            return FindItem(id, rootItem);
        }

        public string GetUniqueName(string desiredName, TreeElement parent, TreeElement except)
        {
            if (!parent.hasChildren)
            {
                return desiredName;
            }

            var childen = parent.children.Where(c => c.id != except.id);

            var names = childen.Select(c => c.name);

            if (names.Contains(desiredName))
            {
                string[] parts = desiredName.Split(' ');
                if (parts.Length > 1)
                {
                    int val;
                    if (int.TryParse(parts.Last(), out val))
                    {
                        desiredName = string.Join(" ", parts, 0, parts.Length - 1);
                    }
                }

                for (int i = 0; i < int.MaxValue; ++i)
                {
                    bool unique = true;
                    foreach (string name in names)
                    {
                        parts = name.Split(' ');
                        if (parts.Length > 1)
                        {
                            int val;
                            if (int.TryParse(parts.Last(), out val))
                            {
                                if (val == i)
                                {
                                    unique = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (unique)
                    {
                        desiredName += " " + i;
                        break;
                    }
                }
            }

            return desiredName;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            // Set the backend name and reload the tree to reflect the new model
            if (args.acceptedRename)
            {
                var element = treeModel.Find(args.itemID);
                element.name = GetUniqueName(args.newName, element.parent, element);
                Reload();
            }
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
            CenterRectUsingSingleLineHeight(ref cellRect);
            return base.GetRenameRect(cellRect, row, item);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            AssetInfo assetInfo = ((TreeViewItem<AssetInfo>)args.draggedItem).data;
            if(assetInfo.parent != treeModel.root)
            {
                return false;
            }
            return base.CanStartDrag(args);
        }

        protected override DragAndDropVisualMode OnExternalDragDropBetweenItems(DragAndDropArgs args)
        {
            return m_externalDropInside(args.parentItem, args.insertAtIndex, args.performDrop);
        }

        protected override DragAndDropVisualMode OnExternalDragDropOutsideItems(DragAndDropArgs args)
        {
            return m_externalDropOutside(args.parentItem, args.insertAtIndex, args.performDrop);
        }

        protected override bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
        {
            if (base.ValidDrag(parent, draggedItems))
            {
                //if (parent.hasChildren)
                //{
                //    var names = parent.children.OfType<TreeViewItem<AssetInfo>>().Select(c => c.data.name);
                //    if (draggedItems.OfType<TreeViewItem<AssetInfo>>().Any(item => names.Contains(item.data.name)))
                //    {
                //        return false;
                //    }
                //}
                //return true;
            }
            return false;
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Asset", ""),
                    contextMenuText = "Asset",
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 200,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = false
                },


                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Persistent ID"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 85,
                    minWidth = 85,
                    autoResize = false,
                    allowToggleVisibility = false
                },

                //new MultiColumnHeaderState.Column
                //{
                //    headerContent = new GUIContent("Visible"),
                //    headerTextAlignment = TextAlignment.Left,
                //    sortedAscending = true,
                //    sortingArrowAlignment = TextAlignment.Center,
                //    width = 70,
                //    minWidth = 70,
                //    autoResize = false,
                //    allowToggleVisibility = false
                //},

            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(Columns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }
    }
}
