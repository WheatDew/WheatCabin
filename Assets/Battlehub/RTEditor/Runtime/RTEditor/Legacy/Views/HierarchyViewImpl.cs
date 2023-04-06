using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTHandles;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls;
using Battlehub.UIControls.MenuControl;
using Battlehub.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*System.Obsolete*/]
    public class HierarchyViewImpl : MonoBehaviour
    {
        private bool m_lockSelection;
        private bool m_isSpawningPrefab;
        private ItemDropAction m_lastDropAction;
        protected ItemDropAction LastDropAction
        {
            get { return m_lastDropAction; }
        }

        private VirtualizingTreeView m_treeView;
        protected VirtualizingTreeView TreeView
        {
            get { return m_treeView; }
        }

        private List<GameObject> m_rootGameObjects;
        protected List<GameObject> RootGameObjects
        {
            get { return m_rootGameObjects; }
        }

        private IProjectAsync m_project;
        protected IProjectAsync Project
        {
            get { return m_project; }
        }

        private IRuntimeEditor m_editor;
        protected IRuntimeEditor Editor
        {
            get { return m_editor; }
        }

        private HierarchyView m_hierarchyView;
        protected RuntimeWindow Window
        {
            get { return m_hierarchyView; }
        }

      
        private TMP_InputField m_filterInput;
        protected TMP_InputField FilterInput
        {
            get { return m_filterInput; }
        }

        protected bool IsFilterEmpty
        {
            get { return string.IsNullOrWhiteSpace(m_filter); }
        }
        
        protected bool Filter(ExposeToEditor go)
        {
            return go.name.ToLower().Contains(m_filter.ToLower());
        }

        private float m_applyFilterTime;
        private string m_filter = string.Empty;

        private ILocalization m_localization;
        private IRuntimeSelectionComponent m_selectionComponent;
        
        protected virtual void Awake()
        {
            m_localization = IOC.Resolve<ILocalization>();
            m_hierarchyView = GetComponent<HierarchyView>();
            if (!m_hierarchyView.TreeViewPrefab)
            {
                Debug.LogError("Set TreeViewPrefab field");
                return;
            }

            m_project = IOC.Resolve<IProjectAsync>();
            m_editor = IOC.Resolve<IRuntimeEditor>();

            m_filterInput = m_hierarchyView.FilterInput;
            if(m_filterInput != null)
            {
                m_filterInput.onValueChanged.AddListener(OnFiltering);
            }
            
            Transform parent = m_hierarchyView.TreePanel != null ? m_hierarchyView.TreePanel : transform;
            m_treeView = Instantiate(m_hierarchyView.TreeViewPrefab, parent).GetComponent<VirtualizingTreeView>();
            m_treeView.name = "HierarchyTreeView";
            m_treeView.CanSelectAll = false;
            m_treeView.SelectOnPointerUp = true;
            
            RectTransform rt = (RectTransform)m_treeView.transform;
            rt.Stretch();

            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.SelectionChanged += OnSelectionChanged;
            m_treeView.ItemsRemoving += OnItemsRemoving;
            m_treeView.ItemsRemoved += OnItemsRemoved;
            m_treeView.ItemExpanding += OnItemExpanding;
            m_treeView.ItemBeginDrag += OnItemBeginDrag;
            m_treeView.ItemBeginDrop += OnItemBeginDrop;
            m_treeView.ItemDrag += OnItemDrag;
            m_treeView.ItemDrop += OnItemDrop;
            m_treeView.ItemEndDrag += OnItemEndDrag;
            m_treeView.ItemDragEnter += OnItemDragEnter;
            m_treeView.ItemDragExit += OnItemDragExit;
            m_treeView.ItemDoubleClick += OnItemDoubleClicked;
            m_treeView.ItemHold += OnItemHold;
            m_treeView.ItemClick += OnItemClick;
            m_treeView.ItemBeginEdit += OnItemBeginEdit;
            m_treeView.ItemEndEdit += OnItemEndEdit;
            m_treeView.PointerEnter += OnTreeViewPointerEnter;
            m_treeView.PointerExit += OnTreeViewPointerExit;

            m_hierarchyView.DragEnterEvent += OnDragEnter;
            m_hierarchyView.DragLeaveEvent += OnDragLeave;
            m_hierarchyView.DragEvent += OnDrag;
            m_hierarchyView.DropEvent += OnDrop;
        }

        protected virtual void OnEnable()
        {
            if (m_editor != null)
            {
                m_editor.SceneLoading += OnSceneLoading;
                m_editor.SceneLoaded += OnSceneLoaded;
            }

            EnableHierarchy();
        }

        protected virtual void OnDisable()
        {
            if (m_editor != null)
            {
                m_editor.SceneLoading -= OnSceneLoading;
                m_editor.SceneLoaded -= OnSceneLoaded;
            }

            DisableHierarchy();
        }

        protected virtual void OnDestroy()
        {
            if (m_filterInput != null)
            {
                m_filterInput.onValueChanged.RemoveListener(OnFiltering);
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.SelectionChanged -= OnSelectionChanged;
                m_treeView.ItemsRemoving -= OnItemsRemoving;
                m_treeView.ItemsRemoved -= OnItemsRemoved;
                m_treeView.ItemExpanding -= OnItemExpanding;
                m_treeView.ItemBeginDrag -= OnItemBeginDrag;
                m_treeView.ItemBeginDrop -= OnItemBeginDrop;
                m_treeView.ItemDrag -= OnItemDrag;
                m_treeView.ItemDrop -= OnItemDrop;
                m_treeView.ItemEndDrag -= OnItemEndDrag;
                m_treeView.ItemDragEnter -= OnItemDragEnter;
                m_treeView.ItemDragExit -= OnItemDragExit;
                m_treeView.ItemDoubleClick -= OnItemDoubleClicked;
                m_treeView.ItemHold -= OnItemHold;
                m_treeView.ItemClick -= OnItemClick;
                m_treeView.ItemBeginEdit -= OnItemBeginEdit;
                m_treeView.ItemEndEdit -= OnItemEndEdit;
                m_treeView.PointerEnter -= OnTreeViewPointerEnter;
                m_treeView.PointerExit -= OnTreeViewPointerExit;
            }

            if (m_hierarchyView != null)
            {
                m_hierarchyView.DragEnterEvent -= OnDragEnter;
                m_hierarchyView.DragLeaveEvent -= OnDragLeave;
                m_hierarchyView.DragEvent -= OnDrag;
                m_hierarchyView.DropEvent -= OnDrop;
            }
        }

        protected virtual void LateUpdate()
        {
            m_rootGameObjects = null;

            if (Time.time > m_applyFilterTime)
            {
                m_applyFilterTime = float.PositiveInfinity;
                BindGameObjects(true, true);
            }
        }

        protected virtual void OnFiltering(string value)
        {
            m_filter = value;
            m_applyFilterTime = Time.time + 0.3f;
        }

        public virtual void SelectAll()
        {
            m_treeView.SelectedItems = m_treeView.Items;
        }

        protected virtual void EnableHierarchy()
        {
            BindGameObjects();
            m_lockSelection = true;
            m_treeView.SelectedItems = Editor.Selection.gameObjects != null ? Editor.Selection.gameObjects.Select(g => g.GetComponent<ExposeToEditor>()).Where(e => e != null).ToArray() : null;
            m_lockSelection = false;

            Editor.Selection.SelectionChanged += OnRuntimeSelectionChanged;

            Editor.Object.Awaked += OnObjectAwaked;
            Editor.Object.Started += OnObjectStarted;
            Editor.Object.Enabled += OnObjectEnabled;
            Editor.Object.Disabled += OnObjectDisabled;
            Editor.Object.Destroying += OnObjectDestroying;
            Editor.Object.Destroyed += OnObjectDestroyed;
            Editor.Object.MarkAsDestroyedChanging += OnObjectMarkAsDestoryedChanging;
            Editor.Object.MarkAsDestroyedChanged += OnObjectMarkAsDestroyedChanged;
            Editor.Object.ParentChanged += OnParentChanged;
            Editor.Object.NameChanged += OnNameChanged;

            Editor.PlaymodeStateChanged += OnPlaymodeStateChanged;
        }

        protected virtual void DisableHierarchy()
        {
            if (Editor != null)
            {
                if (Editor.Selection != null)
                {
                    Editor.Selection.SelectionChanged -= OnRuntimeSelectionChanged;
                }

                if (Editor.Object != null)
                {
                    Editor.Object.Awaked -= OnObjectAwaked;
                    Editor.Object.Started -= OnObjectStarted;
                    Editor.Object.Enabled -= OnObjectEnabled;
                    Editor.Object.Disabled -= OnObjectDisabled;
                    Editor.Object.Destroying -= OnObjectDestroying;
                    Editor.Object.Destroyed -= OnObjectDestroyed;
                    Editor.Object.MarkAsDestroyedChanging -= OnObjectMarkAsDestoryedChanging;
                    Editor.Object.MarkAsDestroyedChanged -= OnObjectMarkAsDestroyedChanged;
                    Editor.Object.ParentChanged -= OnParentChanged;
                    Editor.Object.NameChanged -= OnNameChanged;
                }

                Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;
            }
        }

        protected virtual void OnSceneLoading()
        {
            DisableHierarchy();
        }

        protected virtual void OnSceneLoaded()
        {
            EnableHierarchy();
        }

        protected virtual void BindGameObjects(bool forceUseCache = false, bool updateSelection = true)
        {
            bool useCache = Editor.IsPlaying;

            IEnumerable<ExposeToEditor> objects = Editor.Object.Get(IsFilterEmpty, useCache || forceUseCache);
            if(IsFilterEmpty)
            {
                if (objects.Any())
                {
                    Transform commonParent = objects.First().transform.parent;
                    foreach (ExposeToEditor obj in objects)
                    {
                        if (obj.transform.parent != commonParent)
                        {
                            Debug.LogWarning("ExposeToEditor objects have different parents, hierarchy may not work correctly.");
                            break;
                        }
                    }
                }

                m_treeView.SetItems(objects.OrderBy(g => g.transform.GetSiblingIndex()), updateSelection);
            }
            else
            {
                objects = objects.Where(Filter);
                m_treeView.SetItems(objects.OrderBy(g => g.name), updateSelection);
            }
        }

        protected virtual void OnPlaymodeStateChanged()
        {
            BindGameObjects();
        }

        protected virtual void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            ExposeToEditor exposeToEditor = (ExposeToEditor)e.Item;

            if (exposeToEditor.HasChildren())
            {
                e.Children = exposeToEditor.GetChildren().Where(obj => !obj.MarkAsDestroyed);

                //This line is required to syncronize selection, runtime selection and treeview selection
                OnTreeViewSelectionChanged(m_treeView.SelectedItems, m_treeView.SelectedItems);
            }
            else
            {
                e.Children = new ExposeToEditor[0];
            }
        }

        protected virtual void OnRuntimeSelectionChanged(Object[] unselected)
        {
            if (m_lockSelection)
            {
                return;
            }
            m_lockSelection = true;

            if (Editor.Selection.gameObjects == null)
            {
                m_treeView.SelectedItems = new ExposeToEditor[0];
            }
            else
            {
                m_treeView.SelectedItems = Editor.Selection.gameObjects.Select(g => g.GetComponent<ExposeToEditor>()).Where(e => e != null && !e.gameObject.IsPrefab() && (e.gameObject.hideFlags & HideFlags.HideInHierarchy) == 0).ToArray();
            }

            m_lockSelection = false;
        }

        protected virtual void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            OnTreeViewSelectionChanged(e.OldItems, e.NewItems);
        }

        protected virtual void OnTreeViewSelectionChanged(IEnumerable oldItems, IEnumerable newItems)
        {
            if (m_lockSelection)
            {
                return;
            }

            m_lockSelection = true;

            if (newItems == null)
            {
                newItems = new ExposeToEditor[0];
            }
            ExposeToEditor[] selectableObjects = newItems.OfType<ExposeToEditor>().ToArray();
            Editor.Selection.objects = selectableObjects.Select(o => o.gameObject).ToArray();

            //sync with RunitimeSelectiom.objects because of OnBeforeSelectionChanged event
            m_treeView.SelectedItems = selectableObjects;

            m_lockSelection = false;
        }

        protected virtual void OnItemsRemoving(object sender, ItemsCancelArgs e)
        {
            if (e.Items == null)
            {
                return;
            }

            for (int i = e.Items.Count - 1; i >= 0; i--)
            {
                ExposeToEditor item = (ExposeToEditor)e.Items[i];
                if (!item.CanDelete)
                {
                    e.Items.RemoveAt(i);
                }
            }
        }

        protected virtual void OnItemsRemoved(object sender, ItemsRemovedArgs e)
        {
        }

        protected virtual void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ExposeToEditor dataItem = (ExposeToEditor)e.Item;
            if (dataItem != null)
            {
                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = dataItem.name;
                if (dataItem.gameObject.activeInHierarchy)
                {
                    text.color = m_hierarchyView.EnabledItemColor;
                }
                else
                {
                    text.color = m_hierarchyView.DisabledItemColor;
                }

                bool isFilterEmpty = IsFilterEmpty;

                e.CanEdit = dataItem.CanRename && isFilterEmpty;
                e.HasChildren = isFilterEmpty && dataItem.HasChildren();
                e.CanDrag = isFilterEmpty;
            }
        }

        protected virtual void OnItemDoubleClicked(object sender, ItemArgs e)
        {
            ExposeToEditor exposeToEditor = (ExposeToEditor)e.Items[0];
            Editor.Selection.activeObject = exposeToEditor.gameObject;
        }

        private void OnItemHold(object sender, ItemArgs e)
        {
            if (e.PointerEventData.pointerId == 0)
            {
                ExposeToEditor exposeToEditor = (ExposeToEditor)e.Items[0];
                Editor.Selection.activeObject = exposeToEditor.gameObject;
                OpenContextMenu();
            }
        }

        protected virtual void OnItemClick(object sender, ItemArgs e)
        {
            if (e.PointerEventData.button == PointerEventData.InputButton.Right)
            {
                OpenContextMenu();
            }
        }

        private void OpenContextMenu()
        {
            IContextMenu menu = IOC.Resolve<IContextMenu>();
            List<MenuItemInfo> menuItems = new List<MenuItemInfo>();
            OnContextMenu(menuItems);
            menu.Open(menuItems.ToArray());
        }

        protected virtual void OnContextMenu(List<MenuItemInfo> menuItems)
        {
            MenuItemInfo duplicate = new MenuItemInfo { Path = m_localization.GetString("ID_RTEditor_HierarchyViewImpl_Duplicate", "Duplicate") };
            duplicate.Action = new MenuItemEvent();
            duplicate.Action.AddListener(DuplicateContextMenuCmd);
            duplicate.Validate = new MenuItemValidationEvent();
            duplicate.Validate.AddListener(DuplicateValidateContextMenuCmd);
            menuItems.Add(duplicate);

            MenuItemInfo delete = new MenuItemInfo { Path = m_localization.GetString("ID_RTEditor_HierarchyViewImpl_Delete", "Delete") };
            delete.Action = new MenuItemEvent();
            delete.Action.AddListener(DeleteContextMenuCmd);
            delete.Validate = new MenuItemValidationEvent();
            delete.Validate.AddListener(DeleteValidateContextMenuCmd);
            menuItems.Add(delete);

            MenuItemInfo rename = new MenuItemInfo { Path = m_localization.GetString("ID_RTEditor_HierarchyViewImpl_Rename", "Rename") };
            rename.Action = new MenuItemEvent();
            rename.Action.AddListener(RenameContextMenuCmd);
            rename.Validate = new MenuItemValidationEvent();
            rename.Validate.AddListener(RenameValidateContextMenuCmd);
            menuItems.Add(rename);
        }

        protected virtual void RenameValidateContextMenuCmd(MenuItemValidationArgs args)
        {
            if (m_treeView.SelectedItem == null || !((ExposeToEditor)m_treeView.SelectedItem).CanRename) 
            {
                args.IsValid = false;
            }
        }

        protected virtual void RenameContextMenuCmd(string arg)
        {
            VirtualizingTreeViewItem treeViewItem = m_treeView.GetTreeViewItem(m_treeView.SelectedItem);
            if (treeViewItem != null && treeViewItem.CanEdit)
            {
                treeViewItem.IsEditing = true;
            }
        }

        protected virtual void DuplicateValidateContextMenuCmd(MenuItemValidationArgs args)
        {
            if (m_treeView.SelectedItem == null || !m_treeView.SelectedItems.OfType<ExposeToEditor>().Any(o => o.CanDuplicate))
            {
                args.IsValid = false;
            }
        }

        protected virtual void DuplicateContextMenuCmd(string arg)
        {
            GameObject[] gameObjects = m_treeView.SelectedItems.OfType<ExposeToEditor>().Select(o => o.gameObject).ToArray();
            Editor.Duplicate(gameObjects);
        }

        protected virtual void DeleteValidateContextMenuCmd(MenuItemValidationArgs args)
        {
            if (m_treeView.SelectedItem == null || !m_treeView.SelectedItems.OfType<ExposeToEditor>().Any(o => o.CanDelete))
            {
                args.IsValid = false;
            }
        }

        protected virtual void DeleteContextMenuCmd(string arg)
        {
            GameObject[] gameObjects = m_treeView.SelectedItems.OfType<ExposeToEditor>().Select(o => o.gameObject).ToArray();
            Editor.Delete(gameObjects);
        }

        protected virtual void OnItemBeginEdit(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ExposeToEditor dataItem = (ExposeToEditor)e.Item;
            if (dataItem != null)
            {
                TMP_InputField inputField = e.EditorPresenter.GetComponentInChildren<TMP_InputField>(true);
                inputField.text = dataItem.name;
                inputField.ActivateInputField();
                inputField.Select();
                LayoutElement layout = inputField.GetComponent<LayoutElement>();

                TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = dataItem.name;

                RectTransform rt = text.GetComponent<RectTransform>();
                layout.preferredWidth = rt.rect.width;
            }
        }

        protected virtual void OnItemEndEdit(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ExposeToEditor dataItem = (ExposeToEditor)e.Item;
            if (dataItem != null)
            {
                TMP_InputField inputField = e.EditorPresenter.GetComponentInChildren<TMP_InputField>(true);
                if (!string.IsNullOrEmpty(inputField.text))
                {
                    dataItem.SetName(inputField.text);
                    TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
                    text.text = dataItem.name;
                }
                else
                {
                    inputField.text = dataItem.name;
                }
            }

            //Following code is required to unfocus inputfield if focused and release InputManager
            if (EventSystem.current != null && !EventSystem.current.alreadySelecting)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        protected virtual void OnItemDragEnter(object sender, ItemDropCancelArgs e)
        {
            Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
        }

        protected virtual void OnItemDragExit(object sender, System.EventArgs e)
        {
        }

        protected virtual void OnTreeViewPointerEnter(object sender, PointerEventArgs e)
        {
            if (m_treeView.DragItems != null)
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
        }

        protected virtual void OnTreeViewPointerExit(object sender, PointerEventArgs e)
        {
            if (m_treeView.DragItems != null)
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            }
        }

        protected virtual void OnItemBeginDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseBeginDrag(m_hierarchyView, e.Items, e.PointerEventData);
        }

        protected virtual void OnItemDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrag(e.PointerEventData);
        }

        protected virtual void OnItemBeginDrop(object sender, ItemDropCancelArgs e)
        {
            if (e.IsExternal)
            {
                return;
            }

            Editor.Undo.BeginRecord();
            Editor.Undo.CreateRecord(null, null, false,
                record => RefreshTree(record, true),
                record => RefreshTree(record, false));

            IEnumerable<ExposeToEditor> dragItems = e.DragItems.OfType<ExposeToEditor>();

            if (e.Action == ItemDropAction.SetLastChild || dragItems.Any(d => (object)d.GetParent() != e.DropTarget))
            {
                foreach (ExposeToEditor exposed in dragItems.Reverse())
                {
                    Transform dragT = exposed.transform;
                    int siblingIndex = dragT.GetSiblingIndex();
                    Editor.Undo.BeginRecordTransform(dragT, dragT.parent, siblingIndex);
                }
            }
            else
            {
                Transform dropT = ((ExposeToEditor)e.DropTarget).transform;
                int dropTIndex = dropT.GetSiblingIndex();

                foreach (ExposeToEditor exposed in dragItems
                    .Where(o => o.transform.GetSiblingIndex() > dropTIndex)
                    .OrderBy(o => o.transform.GetSiblingIndex())
                    .Union(dragItems
                        .Where(o => o.transform.GetSiblingIndex() < dropTIndex)
                        .OrderByDescending(o => o.transform.GetSiblingIndex())))
                {
                    Transform dragT = exposed.transform;
                    int siblingIndex = dragT.GetSiblingIndex();
                    Editor.Undo.BeginRecordTransform(dragT, dragT.parent, siblingIndex);
                }
            }

            Editor.Undo.EndRecord();
        }

        protected virtual void OnItemDrop(object sender, ItemDropArgs e)
        {
            if (e.IsExternal)
            {
                return;
            }
            Transform dropT = ((ExposeToEditor)e.DropTarget).transform;
            if (e.Action == ItemDropAction.SetLastChild)
            {
                Editor.Undo.BeginRecord();
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    ExposeToEditor exposed = (ExposeToEditor)e.DragItems[i];
                    Transform dragT = exposed.transform;
                    dragT.SetParent(dropT, true);
                    dragT.SetAsLastSibling();

                    Editor.Undo.EndRecordTransform(dragT, dropT, dragT.GetSiblingIndex());
                }
                Editor.Undo.CreateRecord(null, null, true,
                   record => RefreshTree(record, true),
                   record => RefreshTree(record, false));
                Editor.Undo.EndRecord();
            }
            else if (e.Action == ItemDropAction.SetNextSibling)
            {
                Editor.Undo.BeginRecord();

                for (int i = e.DragItems.Length - 1; i >= 0; --i)
                {
                    ExposeToEditor exposed = (ExposeToEditor)e.DragItems[i];
                    Transform dragT = exposed.transform;

                    int dropTIndex = dropT.GetSiblingIndex();
                    if (dragT.parent != dropT.parent)
                    {
                        dragT.SetParent(dropT.parent, true);
                        dragT.SetSiblingIndex(dropTIndex + 1);
                    }
                    else
                    {
                        int dragTIndex = dragT.GetSiblingIndex();
                        if (dropTIndex < dragTIndex)
                        {
                            dragT.SetSiblingIndex(dropTIndex + 1);
                        }
                        else
                        {
                            dragT.SetSiblingIndex(dropTIndex);
                        }
                    }
                    Editor.Undo.EndRecordTransform(dragT, dragT.parent, dragT.GetSiblingIndex());
                }
                Editor.Undo.CreateRecord(null, null, true,
                    record => RefreshTree(record, true),
                    record => RefreshTree(record, false));
                Editor.Undo.EndRecord();

            }
            else if (e.Action == ItemDropAction.SetPrevSibling)
            {
                Editor.Undo.BeginRecord();
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    ExposeToEditor exposed = (ExposeToEditor)e.DragItems[i];
                    Transform dragT = exposed.transform;
                    if (dragT.parent != dropT.parent)
                    {
                        dragT.SetParent(dropT.parent, true);
                    }

                    int dropTIndex = dropT.GetSiblingIndex();
                    int dragTIndex = dragT.GetSiblingIndex();
                    if (dropTIndex > dragTIndex)
                    {
                        dragT.SetSiblingIndex(dropTIndex - 1);
                    }
                    else
                    {
                        dragT.SetSiblingIndex(dropTIndex);
                    }

                    Editor.Undo.EndRecordTransform(dragT, dragT.parent, dragT.GetSiblingIndex());
                }
                Editor.Undo.CreateRecord(null, null, true,
                    record => RefreshTree(record, true),
                    record => RefreshTree(record, false));
                Editor.Undo.EndRecord();
            }

            Editor.DragDrop.RaiseDrop(e.PointerEventData);
        }

        protected virtual void OnItemEndDrag(object sender, ItemArgs e)
        {
            if (Editor.DragDrop.InProgress)
            {
                Editor.DragDrop.RaiseDrop(e.PointerEventData);
            }
        }

        protected virtual bool RefreshTree(Record record, bool isRedo)
        {
            bool applyOnRedo = (bool)record.OldState;
            if (applyOnRedo != isRedo)
            {
                return false;
            }

            BindGameObjects(true, false);

            if (m_treeView.SelectedItems != null)
            {
                foreach (ExposeToEditor obj in m_treeView.SelectedItems.OfType<ExposeToEditor>().OrderBy(o => o.transform.GetSiblingIndex()))
                {
                    Expand(obj);
                }
            }

            return false;
        }

        protected virtual void Expand(ExposeToEditor item)
        {
            if (item == null || !IsFilterEmpty)
            {
                return;
            }

            ExposeToEditor parent = item.GetParent();
            if (parent != null && !m_treeView.IsExpanded(parent))
            {
                Expand(parent);
            }

            if (item.HasChildren())
            {
                m_treeView.Expand(item);
            }
        }

        protected virtual void OnObjectAwaked(ExposeToEditor obj)
        {
            if (!m_isSpawningPrefab)
            {
                if (!obj.MarkAsDestroyed && m_treeView.IndexOf(obj) == -1)
                {
                    if(IsFilterEmpty)
                    {
                        ExposeToEditor parent = obj.GetParent();
                        m_treeView.AddChild(parent, obj);
                    }
                    else
                    {
                        if(Filter(obj))
                        {
                            m_treeView.Add(obj);
                        }
                    }
                }
            }
        }

        protected virtual void OnObjectStarted(ExposeToEditor obj)
        {
        }

        protected virtual void OnObjectEnabled(ExposeToEditor obj)
        {
            VirtualizingTreeViewItem tvItem = m_treeView.GetTreeViewItem(obj);
            if (tvItem == null)
            {
                return;
            }
            TextMeshProUGUI text = tvItem.GetComponentInChildren<TextMeshProUGUI>();
            text.color = m_hierarchyView.EnabledItemColor;
        }

        protected virtual void OnObjectDisabled(ExposeToEditor obj)
        {
            VirtualizingTreeViewItem tvItem = m_treeView.GetTreeViewItem(obj);
            if (tvItem == null)
            {
                return;
            }
            TextMeshProUGUI text = tvItem.GetComponentInChildren<TextMeshProUGUI>();
            text.color = m_hierarchyView.DisabledItemColor;
        }

        protected virtual void OnObjectDestroying(ExposeToEditor o)
        {
            try
            {
                m_treeView.ItemsRemoving -= OnItemsRemoving;
                m_treeView.ItemsRemoved -= OnItemsRemoved;
                if (IsFilterEmpty)
                {
                    ExposeToEditor parent = o.GetParent();
                    m_treeView.RemoveChild(parent, o);
                }
                else
                {
                    m_treeView.RemoveChild(null, o);
                }
            }
            finally
            {
                m_treeView.ItemsRemoving += OnItemsRemoving;
                m_treeView.ItemsRemoved += OnItemsRemoved;
            }
        }

        protected virtual void OnObjectDestroyed(ExposeToEditor o)
        {
        }

        protected virtual void OnObjectMarkAsDestoryedChanging(ExposeToEditor o)
        {
        }

        protected virtual void OnObjectMarkAsDestroyedChanged(ExposeToEditor obj)
        {
            if (obj.MarkAsDestroyed)
            {
                m_treeView.ItemsRemoving -= OnItemsRemoving;
                m_treeView.ItemsRemoved -= OnItemsRemoved;
                m_treeView.RemoveChild(obj.GetParent(), obj);
                m_treeView.ItemsRemoving += OnItemsRemoving;
                m_treeView.ItemsRemoved += OnItemsRemoved;
            }
            else
            {
                if (IsFilterEmpty)
                {
                    ExposeToEditor parent = obj.GetParent();
                    m_treeView.AddChild(parent, obj);
                    SetSiblingIndex(obj);
                }
                else
                {
                    if (Filter(obj))
                    {
                        AddSortedByName(obj);
                    }
                }
            }
        }

        private void AddSortedByName(ExposeToEditor obj)
        {
            string[] names = m_treeView.Items.OfType<ExposeToEditor>().Select(go => go.name).Union(new[] { obj.name }).OrderBy(k => k).ToArray();
            int index = System.Array.IndexOf(names, obj.name);
            ExposeToEditor sibling;
            if (index == 0)
            {
                sibling = m_treeView.Items.OfType<ExposeToEditor>().FirstOrDefault();
                m_treeView.Add(obj);
                if (sibling != null)
                {
                    m_treeView.SetPrevSibling(sibling, obj);
                }
            }
            else
            {
                sibling = m_treeView.Items.OfType<ExposeToEditor>().ElementAt(index - 1);
                m_treeView.Add(obj);
                m_treeView.SetNextSibling(sibling, obj);
            }
        }

        protected virtual void SetSiblingIndex(ExposeToEditor obj)
        {
            if (obj.transform.parent == null && m_rootGameObjects == null)
            {
                m_rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects().OrderBy(g => g.transform.GetSiblingIndex()).ToList();
            }

            ExposeToEditor nextSibling = obj.NextSibling(m_rootGameObjects);
            if (nextSibling != null)
            {
                m_treeView.SetPrevSibling(nextSibling, obj);
            }
        }

        protected virtual void OnParentChanged(ExposeToEditor obj, ExposeToEditor oldParent, ExposeToEditor newParent)
        {
            if (Editor.IsPlaymodeStateChanging)
            {
                return;
            }

            if(!IsFilterEmpty)
            {
                return;
            }

            bool isNewParentExpanded = true;
            bool isOldParentExpanded = true;
            bool isLastChild = false;
            if (newParent != null)
            {
                isNewParentExpanded = m_treeView.IsExpanded(newParent);
            }

            if (oldParent != null)
            {
                //TreeViewItemContainerData itemContainerData = (TreeViewItemContainerData)m_treeView.GetItemContainerData(oldParent);

                isLastChild = !oldParent.HasChildren(); //!itemContainerData.HasChildren(m_treeView);

                isOldParentExpanded = m_treeView.IsExpanded(oldParent);
            }

            if (isNewParentExpanded)
            {
                m_treeView.ChangeParent(newParent, obj);

                if (!isOldParentExpanded)
                {
                    if (isLastChild)
                    {
                        VirtualizingTreeViewItem oldParentContainer = m_treeView.GetTreeViewItem(oldParent);
                        if (oldParentContainer)
                        {
                            oldParentContainer.CanExpand = false;
                        }
                    }
                }
            }
            else
            {
                if (newParent != null)
                {
                    VirtualizingTreeViewItem newParentTreeViewItem = m_treeView.GetTreeViewItem(newParent);
                    if (newParentTreeViewItem != null)
                    {
                        newParentTreeViewItem.CanExpand = true;
                    }
                }

                m_treeView.ItemsRemoving -= OnItemsRemoving;
                m_treeView.ItemsRemoved -= OnItemsRemoved;
                m_treeView.RemoveChild(oldParent, obj);
                m_treeView.ItemsRemoving += OnItemsRemoving;
                m_treeView.ItemsRemoved += OnItemsRemoved;
            }
        }

        protected virtual void OnNameChanged(ExposeToEditor obj)
        {
            if(!IsFilterEmpty)
            {
                m_treeView.ItemsRemoving -= OnItemsRemoving;
                m_treeView.ItemsRemoved -= OnItemsRemoved;

                if(Filter(obj))
                {
                    if(m_treeView.GetItemContainerData(obj) == null)
                    {
                        AddSortedByName(obj);
                        m_treeView.SelectedItems = Editor.Selection.gameObjects.Select(go => go.GetComponent<ExposeToEditor>()).Where(exposed => exposed != null);
                    }
                }
                else
                {
                    m_treeView.RemoveChild(null, obj);
                }
                
                m_treeView.ItemsRemoving += OnItemsRemoving;
                m_treeView.ItemsRemoved += OnItemsRemoved;
            }

            VirtualizingTreeViewItem tvItem = m_treeView.GetTreeViewItem(obj);
            if (tvItem == null)
            {
                return;
            }

            TextMeshProUGUI text = tvItem.GetComponentInChildren<TextMeshProUGUI>();
            text.text = obj.name;
        }

        protected virtual bool CanDrop(object[] dragObjects)
        {
            IEnumerable<ProjectItem> projectItems = dragObjects.OfType<ProjectItem>();
            return projectItems.Count() > 0 && projectItems.Any(projectItem => !projectItem.IsFolder && m_project.Utils.ToType(projectItem) == typeof(GameObject));
        }

        protected virtual void OnDragEnter(PointerEventData pointerEventData)
        {
            if(IsFilterEmpty)
            {
                m_treeView.ExternalBeginDrag(pointerEventData.position);
            }
            else
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            }
        }

        protected virtual void OnDragLeave(PointerEventData pointerEventData)
        {
            if(IsFilterEmpty)
            {
                m_treeView.ExternalDrop();
            }
            
            Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
        }

        protected virtual void OnDrag(PointerEventData pointerEventData)
        {
            if(!IsFilterEmpty)
            {
                return;
            }

            object[] dragObjects = Editor.DragDrop.DragObjects;
            m_treeView.ExternalDrag(pointerEventData.position);
            m_lastDropAction = m_treeView.DropAction;
            if (CanDrop(dragObjects))
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
            else
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
                m_treeView.ClearTarget();
            }
        }

        protected async virtual void OnDrop(PointerEventData pointerEventData)
        {
            if (!IsFilterEmpty)
            {
                return;
            }

            object[] dragObjects = Editor.DragDrop.DragObjects;
            if (CanDrop(dragObjects))
            {
                ExposeToEditor dropTarget = (ExposeToEditor)m_treeView.DropTarget;
                VirtualizingTreeViewItem treeViewItem = null;
                if (dropTarget != null)
                {
                    treeViewItem = m_treeView.GetTreeViewItem(m_treeView.DropTarget);
                }

                ProjectItem[] loadAssetItems = dragObjects.Where(o => o is ProjectItem && m_project.Utils.ToType((ProjectItem)o) == typeof(GameObject)).Select(o => (ProjectItem)o).ToArray();
                if (loadAssetItems.Length > 0)
                {
                    m_isSpawningPrefab = true;
                    Editor.IsBusy = true;
                    Object[] objects;
                    try
                    {
                        objects = await m_project.Safe.LoadAsync(loadAssetItems);
                    }
                    catch(System.Exception e)
                    {
                        IWindowManager wm = IOC.Resolve<IWindowManager>();
                        wm.MessageBox(m_localization.GetString("ID_RTEditor_HierarchyView_UnableToLoadAssetItems", "Unable to load asset items"), e.Message);
                        Debug.LogException(e);
                        return;
                    }
                    finally
                    {
                        Editor.IsBusy = false;
                    }

                    OnAssetItemsLoaded(objects, dropTarget, treeViewItem);
                }
                else
                {
                    m_treeView.ExternalDrop();
                }
            }
            else
            {
                m_treeView.ExternalDrop();
            }
        }

        protected void OnAssetItemsLoaded(Object[] objects, ExposeToEditor dropTarget, VirtualizingTreeViewItem treeViewItem)
        {
            IPlacementModel placementModel = IOC.Resolve<IPlacementModel>();
            m_selectionComponent = placementModel.GetSelectionComponent();

            GameObject[] createdObjects = new GameObject[objects.Length];
            for (int i = 0; i < objects.Length; ++i)
            {
                GameObject prefab = (GameObject)objects[i];
                bool wasPrefabEnabled = prefab.activeSelf;
                prefab.SetActive(false);
                GameObject prefabInstance = InstantiatePrefab(prefab);
                Editor.AddGameObjectToHierarchy(prefabInstance);
                prefab.SetActive(wasPrefabEnabled);

                ExposeToEditor exposeToEditor = ExposePrefabInstance(prefabInstance);
                exposeToEditor.SetName(prefab.name);

                if (dropTarget == null)
                {
                    exposeToEditor.transform.SetParent(null);
                    m_treeView.Add(exposeToEditor);
                }
                else
                {
                    if (m_lastDropAction == ItemDropAction.SetLastChild)
                    {
                        exposeToEditor.transform.SetParent(dropTarget.transform);
                        m_treeView.AddChild(dropTarget, exposeToEditor);
                        treeViewItem.CanExpand = true;
                        treeViewItem.IsExpanded = true;
                    }
                    if (m_lastDropAction != ItemDropAction.None && m_lastDropAction != ItemDropAction.SetLastChild)
                    {
                        int index;
                        int siblingIndex;
                        if (m_lastDropAction == ItemDropAction.SetNextSibling)
                        {
                            index = m_treeView.IndexOf(dropTarget) + 1;
                            siblingIndex = dropTarget.transform.GetSiblingIndex() + 1;
                        }
                        else
                        {
                            index = m_treeView.IndexOf(dropTarget);
                            siblingIndex = dropTarget.transform.GetSiblingIndex();
                        }

                        exposeToEditor.transform.SetParent(dropTarget.transform.parent != null ? dropTarget.transform.parent : null);
                        exposeToEditor.transform.SetSiblingIndex(siblingIndex);

                        TreeViewItemContainerData newTreeViewItemData = (TreeViewItemContainerData)m_treeView.Insert(index, exposeToEditor);
                        VirtualizingTreeViewItem newTreeViewItem = m_treeView.GetTreeViewItem(exposeToEditor);
                        if (newTreeViewItem != null)
                        {
                            newTreeViewItem.Parent = treeViewItem.Parent;
                        }
                        else
                        {
                            newTreeViewItemData.Parent = treeViewItem.Parent;
                        }
                    }
                }

                OnActivatePrefabInstance(prefabInstance);
                createdObjects[i] = prefabInstance;
            }

            if (createdObjects.Length > 0)
            {
                IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
                editor.RegisterCreatedObjects(createdObjects, m_selectionComponent != null ? m_selectionComponent.CanSelect : true);
            }

            m_treeView.ExternalDrop();
            m_isSpawningPrefab = false;
        }

        protected virtual ExposeToEditor ExposePrefabInstance(GameObject prefabInstance)
        {
            Transform[] transforms = prefabInstance.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in transforms)
            {
                if (transform.GetComponent<ExposeToEditor>() == null)
                {
                    transform.gameObject.AddComponent<ExposeToEditor>();
                }
            }

            ExposeToEditor exposeToEditor = prefabInstance.GetComponent<ExposeToEditor>();
            if (exposeToEditor == null)
            {
                exposeToEditor = prefabInstance.AddComponent<ExposeToEditor>();
            }

            return exposeToEditor;
        }

        protected virtual void OnActivatePrefabInstance(GameObject prefabInstance)
        {
            prefabInstance.SetActive(true);
        }

        protected virtual GameObject InstantiatePrefab(GameObject prefab)
        {
            Vector3 pivot = Vector3.zero;
            if (m_selectionComponent != null)
            {
                pivot = m_selectionComponent.SecondaryPivot;
            }

            return Instantiate(prefab, pivot, Quaternion.identity);
        }

    }

}


