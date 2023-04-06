using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityWeld.Binding;
using UnityWeld.Binding.Exceptions;
using UnityWeld.Binding.Internal;

namespace Battlehub.UIControls.Binding
{
    public enum HierarchicalDataChangedAction
    {
        Add,
        Insert,
        Remove,
        RemoveSelected,
        SetNextSibling,
        SetPrevSibling,
        ChangeParent,
        Expand,
        Collapse,
        Select,
        Reset
    }

    public class HierarchicalDataChangedEventArgs : EventArgs
    {
        public HierarchicalDataChangedEventArgs() { }
        public HierarchicalDataChangedAction Action { get; private set; }
        public object Item { get; private set; }
        public object TargetItem { get; private set; }

        public static HierarchicalDataChangedEventArgs ItemAdded(object parentItem, object item)
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.Add,
                Item = item,
                TargetItem = parentItem
            };
        }

        public static HierarchicalDataChangedEventArgs ItemInserted(int index, object item)
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.Insert,
                Item = item,
                TargetItem = index
            };
        }

        public static HierarchicalDataChangedEventArgs ItemRemoved(object parentItem, object item)
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.Remove,
                Item = item,
                TargetItem = parentItem
            };
        }

        public static HierarchicalDataChangedEventArgs RemoveSelected()
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.Remove,
                Item = null,
                TargetItem = null
            };
        }


        public static HierarchicalDataChangedEventArgs NextSiblingsChanged(object nextSibling, object item)
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.SetNextSibling,
                Item = item,
                TargetItem = nextSibling
            };
        }

        public static HierarchicalDataChangedEventArgs PrevSiblingsChanged(object prevSibling, object item)
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.SetPrevSibling,
                Item = item,
                TargetItem = prevSibling
            };
        }

        public static HierarchicalDataChangedEventArgs ParentChanged(object oldParent, object item)
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.ChangeParent,
                Item = item,
                TargetItem = oldParent
            };
        }

        public static HierarchicalDataChangedEventArgs Expand(object item)
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.Expand,
                Item = item,
            };
        }

        public static HierarchicalDataChangedEventArgs Collapse(object item)
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.Collapse,
                Item = item,
            };
        }

        public static HierarchicalDataChangedEventArgs Select(IEnumerable items, object scrollIntoView = null)
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.Select,
                TargetItem = scrollIntoView,
                Item = items,
            };
        }

        public static HierarchicalDataChangedEventArgs Reset(object item = null)
        {
            return new HierarchicalDataChangedEventArgs
            {
                Action = HierarchicalDataChangedAction.Reset,
                Item = item,
                TargetItem = item
            };
        }
    }

    public interface INotifyHierarchicalDataChanged
    {
        event EventHandler<HierarchicalDataChangedEventArgs> HierarchicalDataChanged;
    }

    /// <summary>
    /// Global flags (restrictive, which means that if something is disabled it cannot be re-enabled at the item level using per item flags)
    /// </summary>
    [Flags]
    public enum HierarchicalDataFlags
    {
        None = 0,
        Default = -1,

        CanMultiSelect = 1,
        CanSelectAll = 1 << 1,
        CanUnselectAll = 1 << 2,

        CanEdit = 1 << 4,
        CanRemove = 1 << 5,

        CanReorder = 1 << 6,
        CanChangeParent = 1 << 7,

        CanDrag = CanReorder | CanChangeParent,
    }


    /// <summary>
    /// Per item flags
    /// </summary>
    [Flags]
    public enum HierarchicalDataItemFlags
    {
        None = 0,
        Default = -1,

        CanSelect = 1 << 3,
        CanEdit = 1 << 4,
        CanRemove = 1 << 5,

        CanChangeParent = 1 << 7,
        CanBeParent = 1 << 8,

        CanDrag = CanChangeParent,
    }

    public class HierarchicalDataCollection<T> : IHierarchicalData<T> where T : class
    {
        public event EventHandler<HierarchicalDataChangedEventArgs> HierarchicalDataChanged;

        private Dictionary<T, T> m_itemToParent;
        private Dictionary<T, List<T>> m_parentToItems;
        private List<T> m_rootItems;

        private HierarchicalDataFlags m_flags;
        private HierarchicalDataItemFlags m_itemFlags;

        public bool ScrollIntoViewSelected
        {
            get;
            set;
        }

        public HierarchicalDataCollection() : this(HierarchicalDataFlags.None, HierarchicalDataItemFlags.CanSelect)
        {
        }

        public HierarchicalDataCollection(HierarchicalDataFlags flags = HierarchicalDataFlags.None, HierarchicalDataItemFlags itemFlags = HierarchicalDataItemFlags.CanSelect)
        {
            m_flags = flags;
            m_itemFlags = itemFlags;

            m_itemToParent = new Dictionary<T, T>();
            m_parentToItems = new Dictionary<T, List<T>>();
            m_rootItems = new List<T>();
        }

        public HierarchicalDataCollection(IEnumerable<T> items, HierarchicalDataFlags flags = HierarchicalDataFlags.None, HierarchicalDataItemFlags itemFlags = HierarchicalDataItemFlags.CanSelect)
            : this(items.ToDictionary(item => item, item => (T)null), flags, itemFlags)
        {
        }

        public HierarchicalDataCollection(IDictionary<T, T> itemToParent, HierarchicalDataFlags flags = HierarchicalDataFlags.None, HierarchicalDataItemFlags itemFlags = HierarchicalDataItemFlags.CanSelect)
        {
            m_flags = flags;
            m_itemFlags = itemFlags;

            m_itemToParent = itemToParent.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            m_rootItems = new List<T>();
            m_parentToItems = new Dictionary<T, List<T>>();

            foreach (KeyValuePair<T, T> kvp in m_itemToParent)
            {
                T item = kvp.Key;
                T parent = kvp.Value;

                if (parent == null)
                {
                    if (!m_parentToItems.TryGetValue(parent, out List<T> items))
                    {
                        m_parentToItems[parent] = items = new List<T>();
                    }

                    items.Add(item);
                }
                else
                {
                    m_rootItems.Add(item);
                }
            }
        }

        public virtual IEnumerable<T> GetChildren(T parent)
        {
            if (parent == null)
            {
                return m_rootItems;
            }

            return m_parentToItems[parent];
        }

        public virtual HierarchicalDataFlags GetFlags()
        {
            return m_flags;
        }

        public virtual HierarchicalDataItemFlags GetItemFlags(T item)
        {
            return m_itemFlags;
        }

        public virtual T GetParent(T item)
        {
            return m_itemToParent[item];
        }

        public virtual bool HasChildren(T parent)
        {
            if (parent == null)
            {
                return m_rootItems.Count > 0;
            }

            return m_parentToItems.ContainsKey(parent);
        }

        public virtual int IndexOf(T parent, T item)
        {
            if (parent == null)
            {
                return m_rootItems.IndexOf(item);
            }

            return m_parentToItems[parent].IndexOf(item);
        }

        public virtual void Add(T parent, T item)
        {
            if (parent == null)
            {
                m_rootItems.Add(item);
            }
            else
            {
                m_itemToParent.Add(parent, item);

                if (!m_parentToItems.TryGetValue(parent, out List<T> items))
                {
                    m_parentToItems[parent] = items = new List<T>();
                }

                items.Add(item);
            }


            HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.ItemAdded(parent, item));
        }

        public virtual void Insert(T parent, T item, int index)
        {
            T sibling = null;
            if (parent == null)
            {
                m_rootItems.Insert(index, item);
                if (m_rootItems.Count > 1)
                {
                    if (index > 0)
                    {
                        sibling = m_rootItems[index - 1];
                    }
                    else
                    {
                        sibling = m_rootItems[1];
                    }
                }
            }
            else
            {
                m_itemToParent.Add(parent, item);

                if (!m_parentToItems.TryGetValue(parent, out List<T> items))
                {
                    m_parentToItems[parent] = items = new List<T>();
                }

                items.Insert(index, item);
                if (items.Count > 1)
                {
                    if (index > 0)
                    {
                        sibling = items[index - 1];
                    }
                    else if (m_rootItems.Count > 1)
                    {
                        sibling = items[1];
                    }
                }
            }


            HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.ItemAdded(parent, item));

            if (sibling != null)
            {
                if (index == 0)
                {
                    HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.PrevSiblingsChanged(item, sibling));
                }
                else
                {
                    HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.NextSiblingsChanged(item, sibling));
                }
            }

        }

        internal object Where(Func<object, object> p)
        {
            throw new NotImplementedException();
        }

        public virtual void Remove(T parent, T item)
        {
            if (parent == null)
            {
                m_rootItems.Remove(item);
            }
            else
            {
                m_itemToParent.Remove(item);

                if (m_parentToItems.ContainsKey(parent))
                {
                    m_parentToItems[parent].Remove(item);
                }
            }

            HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.ItemRemoved(parent, item));
        }

        public virtual void RemoveAt(T parent, int index)
        {
            T item = null;
            if (parent == null)
            {
                item = m_rootItems[index];
                m_rootItems.RemoveAt(index);
            }
            else
            {
                if (m_parentToItems.ContainsKey(parent))
                {
                    item = m_parentToItems[parent][index];
                    m_itemToParent.Remove(item);
                    m_parentToItems[parent].RemoveAt(index);
                }
            }

            if (item != null)
            {
                HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.ItemRemoved(parent, item));
            }
        }

        public virtual void Expand(T item)
        {
            HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.Expand(item));
        }

        public virtual void Collapse(T item)
        {
            HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.Collapse(item));
        }

        public virtual void Select(IEnumerable<T> items)
        {
            HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.Select(items, ScrollIntoViewSelected && items != null ? items.FirstOrDefault() : null));
        }

        public virtual void ResetAt(T parent, int index)
        {
            T item = null;
            if (parent == null)
            {
                item = m_rootItems[index];
            }
            else
            {
                if (m_parentToItems.ContainsKey(parent))
                {
                    item = m_parentToItems[parent][index];
                }
            }

            HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.Reset(item));
        }

        public virtual void Reset(T item)
        {
            HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.Reset(item));
        }

        public virtual void Reset()
        {
            HierarchicalDataChanged?.Invoke(this, HierarchicalDataChangedEventArgs.Reset());
        }
    }



    public interface IHierarchicalData<T> : INotifyHierarchicalDataChanged
    {
        HierarchicalDataFlags GetFlags();
        HierarchicalDataItemFlags GetItemFlags(T item);
        T GetParent(T item);
        bool HasChildren(T parent);
        IEnumerable<T> GetChildren(T parent);
        int IndexOf(T parent, T item);

        void Add(T parent, T item);
        void Insert(T parent, T item, int index);
        void Remove(T parent, T item);

        void Expand(T item);
        void Collapse(T item);

        void Select(IEnumerable<T> items);
    }

    public class VirtualizingTreeViewBinding : ControlBinding
    {
        public UnityEvent SelectionChanged;
        public UnityEvent ItemsBeginDrag;
        public UnityEvent ItemsDrag;
        public UnityEvent ItemsSetLastChild;
        public UnityEvent ItemsSetNextSibling;
        public UnityEvent ItemsSetPrevSibling;
        public UnityEvent ItemsCancelDrop;
        public UnityEvent ItemsBeginDrop;
        public UnityEvent ItemsDrop;
        public UnityEvent ItemsEndDrag;

        public UnityEvent ExternalBeginDrag;
        public UnityEvent ExternalDrag;
        public UnityEvent ExternalDrop;

        public UnityEvent ItemsRemoved;

        public UnityEvent ItemDragEnter;
        public UnityEvent ItemDragExit;

        public UnityEvent ItemBeginEdit;
        public UnityEvent ItemEndEdit;

        public UnityEvent ItemHold;
        public UnityEvent ItemLeftClick;
        public UnityEvent ItemRightClick;
        public UnityEvent ItemDoubleLeftClick;
        public UnityEvent ItemDoubleRightClick;

        public UnityEvent Hold;
        public UnityEvent LeftClick;
        public UnityEvent RightClick;

        #region HierarchicalDataAccessor
        private class HierarchicalDataAccessor : INotifyHierarchicalDataChanged
        {
            private object m_viewModel;
            public event EventHandler<HierarchicalDataChangedEventArgs> HierarchicalDataChanged
            {
                add { ((INotifyHierarchicalDataChanged)m_viewModel).HierarchicalDataChanged += value; }
                remove { ((INotifyHierarchicalDataChanged)m_viewModel).HierarchicalDataChanged -= value; }
            }

            private MethodInfo m_getFlags;
            private MethodInfo m_getItemFlags;
            private MethodInfo m_getParent;
            private MethodInfo m_hasChildren;
            private MethodInfo m_getChildren;
            private MethodInfo m_indexOf;
            private MethodInfo m_add;
            private MethodInfo m_insert;
            private MethodInfo m_remove;
            private MethodInfo m_expand;
            private MethodInfo m_collapse;
            private MethodInfo m_select;

            private Type m_dataType;
            private Type MakeGenericType(Type type)
            {
                return typeof(IHierarchicalData<>).MakeGenericType(type);
            }

            public HierarchicalDataAccessor(object viewModel)
            {
                m_viewModel = viewModel;
                m_dataType = m_viewModel.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHierarchicalData<>)).First().GetGenericArguments()[0];

                Type genericType = MakeGenericType(m_dataType);

                m_getParent = genericType.GetMethod(nameof(IHierarchicalData<object>.GetParent));
                m_hasChildren = genericType.GetMethod(nameof(IHierarchicalData<object>.HasChildren));
                m_getChildren = genericType.GetMethod(nameof(IHierarchicalData<object>.GetChildren));
                m_indexOf = genericType.GetMethod(nameof(IHierarchicalData<object>.IndexOf));
                m_add = genericType.GetMethod(nameof(IHierarchicalData<object>.Add));
                m_insert = genericType.GetMethod(nameof(IHierarchicalData<object>.Insert));
                m_getFlags = genericType.GetMethod(nameof(IHierarchicalData<object>.GetFlags));
                m_getItemFlags = genericType.GetMethod(nameof(IHierarchicalData<object>.GetItemFlags));
                m_remove = genericType.GetMethod(nameof(IHierarchicalData<object>.Remove));
                m_expand = genericType.GetMethod(nameof(IHierarchicalData<object>.Expand));
                m_collapse = genericType.GetMethod(nameof(IHierarchicalData<object>.Collapse));
                m_select = genericType.GetMethod(nameof(IHierarchicalData<object>.Select));
            }

            public HierarchicalDataFlags GetFlags()
            {
                return (HierarchicalDataFlags)m_getFlags.Invoke(m_viewModel, null);
            }

            public HierarchicalDataItemFlags GetItemFlags(object item)
            {
                return (HierarchicalDataItemFlags)m_getItemFlags.Invoke(m_viewModel, new[] { item });
            }

            public object GetParent(object item)
            {
                return m_getParent.Invoke(m_viewModel, new[] { item });
            }

            public bool HasChildren(object parent)
            {
                return (bool)m_hasChildren.Invoke(m_viewModel, new[] { parent });
            }

            public IEnumerable GetChildren(object parent)
            {
                return (IEnumerable)m_getChildren.Invoke(m_viewModel, new[] { parent });
            }

            public int IndexOf(object parent, object item)
            {
                return (int)m_indexOf.Invoke(m_viewModel, new[] { parent, item });
            }

            public void Add(object parent, object item)
            {
                m_add.Invoke(m_viewModel, new[] { parent, item });
            }

            public void Insert(object parent, object item, int index)
            {
                m_insert.Invoke(m_viewModel, new[] { parent, item, index });
            }

            public void Remove(object parent, object item)
            {
                m_remove.Invoke(m_viewModel, new[] { parent, item });
            }

            public void Expand(object item)
            {
                m_expand.Invoke(m_viewModel, new[] { item });
            }

            public void Collapse(object item)
            {
                m_collapse.Invoke(m_viewModel, new[] { item });
            }

            public void Select(IEnumerable items)
            {
                m_select.Invoke(m_viewModel, new[] { MakeArray(items, m_dataType) });
            }

            private Array MakeArray(IEnumerable parm, Type t)
            {
                if (parm == null)
                {
                    return null;
                }

                int size;
                if (parm is IList)
                {
                    size = ((IList)parm).Count;
                }
                else
                {
                    size = 0;
                    foreach (object nextMember in parm)
                    {
                        ++size;
                    }
                }

                Array result = Array.CreateInstance(t, size);
                int index = 0;
                foreach (object nextMember in parm)
                {
                    result.SetValue(nextMember, index);
                    ++index;
                }
                return result;
            }
        }

        #endregion

        /// <summary>
        /// Data that we have bound to.
        /// </summary>
        private HierarchicalDataAccessor m_hierarchicalData;

        /// <summary>
        /// The view-model, cached during connection.
        /// </summary>
        protected object m_viewModel;

        [SerializeField]
        private string m_viewModelPropertyName = string.Empty;

        /// <summary>
        /// The name of the property we are binding to on the view model.
        /// </summary>
        public string ViewModelPropertyName
        {
            get { return m_viewModelPropertyName; }
            set { m_viewModelPropertyName = value; }
        }

        /// <summary>
        /// Watches the view-model property for changes.
        /// </summary>
        protected PropertyWatcher m_viewModelPropertyWatcher;

        #region SourceItems

        [SerializeField]
        private string m_viewModelSourceItemsPropertyName = string.Empty;

        public string ViewModelSourceItemsPropertyName
        {
            get { return m_viewModelSourceItemsPropertyName; }
            set { m_viewModelSourceItemsPropertyName = value; }
        }

        [SerializeField]
        private string m_sourceItemsUIToViewModelAdapter;
        public string SourceItemsUIToViewModelAdapter
        {
            get { return m_sourceItemsUIToViewModelAdapter; }
            set { m_sourceItemsUIToViewModelAdapter = value; }
        }

        private PropertySync m_sourceItemsPropertySync;

        private IEnumerable m_sourceItems;
        public IEnumerable SourceItems
        {
            get { return m_sourceItems; }
            set
            {
                if (m_sourceItems != value)
                {
                    m_sourceItems = value;
                    if (m_sourceItemsPropertySync != null)
                    {
                        m_sourceItemsPropertySync.SyncFromDest();
                    }
                }
            }
        }

        #endregion SourceItems

        #region Target
        [SerializeField]
        private string m_viewModelTargetPropertyName = string.Empty;
        public string ViewModelTargetPropertyName
        {
            get { return m_viewModelTargetPropertyName; }
            set { m_viewModelTargetPropertyName = value; }
        }

        private PropertySync m_TargetPropertySync;

        private object m_target;
        public object Target
        {
            get { return m_target; }
            set
            {
                if (m_target != value)
                {
                    m_target = value;
                    if (m_TargetPropertySync != null)
                    {
                        m_TargetPropertySync.SyncFromDest();
                    }
                }
            }
        }

        #endregion Target

        private VirtualizingTreeView m_treeView;
        public override Component TargetControl
        {
            get { return m_treeView; }
        }

        private ItemDropAction m_lastDropAction;
        private ItemDropAction LastDropAction
        {
            get { return m_lastDropAction; }
            set
            {
                if (m_lastDropAction != value)
                {
                    m_lastDropAction = value;
                    switch (m_lastDropAction)
                    {
                        case ItemDropAction.SetLastChild:
                            ItemsSetLastChild?.Invoke();
                            break;
                        case ItemDropAction.SetNextSibling:
                            ItemsSetNextSibling?.Invoke();
                            break;
                        case ItemDropAction.SetPrevSibling:
                            ItemsSetPrevSibling?.Invoke();
                            break;
                        case ItemDropAction.None:
                            ItemsCancelDrop?.Invoke();
                            break;
                    }
                }
            }
        }

        public override void Connect()
        {
            base.Connect();

            m_lastDropAction = ItemDropAction.None;

            m_treeView = GetComponent<VirtualizingTreeView>();

            m_treeView.ItemDataBinding += OnTreeViewItemDataBinding;
            m_treeView.ItemExpanding += OnTreeViewItemExpanding;

            m_treeView.SelectionChanged += OnTreeViewSelectionChanged;

            m_treeView.ItemsRemoving += OnTreeViewItemsRemoving;
            m_treeView.ItemsRemoved += OnTreeViewItemsRemoved;

            m_treeView.ItemBeginDrag += OnTreeViewItemBeginDrag;
            m_treeView.ItemDrag += OnTreeViewItemDrag;
            m_treeView.ItemBeginDrop += OnTreeViewItemBeginDrop;
            m_treeView.ItemDrop += OnTreeViewItemDrop;
            m_treeView.ItemEndDrag += OnTreeViewItemEndDrag;

            m_treeView.BeginDragExternal += OnTreeViewBeginDragExternal;
            m_treeView.DragExternal += OnTreeViewDragExternal;
            m_treeView.DropExternal += OnTreeViewDropExternal;

            m_treeView.ItemDragEnter += OnTreeViewItemDragEnter;
            m_treeView.ItemDragExit += OnTreeViewItemDragExit;

            m_treeView.ItemBeginEdit += OnTreeViewItemBeginEdit;
            m_treeView.ItemEndEdit += OnTreeViewItemEndEdit;

            m_treeView.ItemHold += OnTreeViewItemHold;
            m_treeView.ItemClick += OnTreeViewItemClick;
            m_treeView.ItemDoubleClick += OnTreeViewItemDoubleClick;

            m_treeView.ItemExpanded += OnTreeViewItemExpanded;
            m_treeView.ItemCollapsed += OnTreeViewItemCollapsed;

            m_treeView.Click += OnTreeViewClick;
            m_treeView.Hold += OnTreeViewHold;

            string propertyName;
            object newViewModel;
            ParseViewModelEndPointReference(
                ViewModelPropertyName,
                out propertyName,
                out newViewModel
            );

            m_viewModel = newViewModel;

            m_viewModelPropertyWatcher = new PropertyWatcher(
                newViewModel,
                propertyName,
                OnDataPropertyChanged
            );

            BindData();

            if (!string.IsNullOrEmpty(m_viewModelSourceItemsPropertyName))
            {
                PropertyEndPoint dragItemsPropertyEndPoint = MakeViewModelEndPoint(m_viewModelSourceItemsPropertyName, m_sourceItemsUIToViewModelAdapter, null);
                m_sourceItemsPropertySync = new PropertySync(
                    dragItemsPropertyEndPoint,
                    new PropertyEndPoint(
                        this,
                        nameof(SourceItems),
                        null,
                        null,
                        "view",
                        this),
                    null,
                    this);
            }

            if (!string.IsNullOrEmpty(m_viewModelTargetPropertyName))
            {
                PropertyEndPoint dropTargetPropertyEndPoint = MakeViewModelEndPoint(m_viewModelTargetPropertyName, null, null);
                m_TargetPropertySync = new PropertySync(
                    dropTargetPropertyEndPoint,
                    new PropertyEndPoint(
                        this,
                        nameof(Target),
                        null,
                        null,
                        "view",
                        this),
                    null,
                    this);
            }
        }

        public override void Disconnect()
        {
            base.Disconnect();

            UnbindData();

            m_viewModel = null;

            if (m_viewModelPropertyWatcher != null)
            {
                m_viewModelPropertyWatcher.Dispose();
                m_viewModelPropertyWatcher = null;
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnTreeViewItemDataBinding;
                m_treeView.ItemExpanding -= OnTreeViewItemExpanding;

                m_treeView.SelectionChanged -= OnTreeViewSelectionChanged;

                m_treeView.ItemsRemoving -= OnTreeViewItemsRemoving;
                m_treeView.ItemsRemoved -= OnTreeViewItemsRemoved;

                m_treeView.ItemBeginDrag -= OnTreeViewItemBeginDrag;
                m_treeView.ItemDrag -= OnTreeViewItemDrag;
                m_treeView.ItemBeginDrop -= OnTreeViewItemBeginDrop;
                m_treeView.ItemDrop -= OnTreeViewItemDrop;
                m_treeView.ItemEndDrag -= OnTreeViewItemEndDrag;

                m_treeView.BeginDragExternal -= OnTreeViewBeginDragExternal;
                m_treeView.DragExternal -= OnTreeViewDragExternal;
                m_treeView.DropExternal -= OnTreeViewDropExternal;

                m_treeView.ItemDragEnter -= OnTreeViewItemDragEnter;
                m_treeView.ItemDragExit -= OnTreeViewItemDragExit;

                m_treeView.ItemBeginEdit -= OnTreeViewItemBeginEdit;
                m_treeView.ItemEndEdit -= OnTreeViewItemEndEdit;

                m_treeView.ItemHold -= OnTreeViewItemHold;
                m_treeView.ItemClick -= OnTreeViewItemClick;
                m_treeView.ItemDoubleClick -= OnTreeViewItemDoubleClick;

                m_treeView.ItemExpanded -= OnTreeViewItemExpanded;
                m_treeView.ItemCollapsed -= OnTreeViewItemCollapsed;

                m_treeView.Click -= OnTreeViewClick;
                m_treeView.Hold -= OnTreeViewHold;

                m_treeView = null;
            }
        }

        private void BindData()
        {
            // Bind view model.
            var viewModelType = m_viewModel.GetType();

            string propertyName;
            string viewModelName;
            ParseEndPointReference(
                ViewModelPropertyName,
                out propertyName,
                out viewModelName
            );

            var viewModelCollectionProperty = viewModelType.GetProperty(propertyName);
            if (viewModelCollectionProperty == null)
            {
                throw new MemberNotFoundException(
                    "Expected property "
                    + ViewModelPropertyName + ", but it wasn't found on type "
                    + viewModelType + "."
                );
            }

            // Get value from view model.
            var viewModelValue = viewModelCollectionProperty.GetValue(m_viewModel, null);
            if (viewModelValue == null)
            {
                m_treeView.Items = null;
            }
            else
            {
                bool isHierarchicalData = viewModelValue.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHierarchicalData<>));
                if (!isHierarchicalData)
                {
                    throw new InvalidTypeException(
                        "Property "
                        + ViewModelPropertyName
                        + " is not a IHierarchicalData and cannot be used."
                    );
                }

                m_hierarchicalData = new HierarchicalDataAccessor(viewModelValue);
                m_hierarchicalData.HierarchicalDataChanged += OnHierarchicalDataChanged;

                HierarchicalDataFlags flags = m_hierarchicalData.GetFlags();
                m_treeView.CanSelectAll = (flags & HierarchicalDataFlags.CanSelectAll) != 0;
                m_treeView.CanUnselectAll = (flags & HierarchicalDataFlags.CanUnselectAll) != 0;
                m_treeView.CanMultiSelect = (flags & HierarchicalDataFlags.CanMultiSelect) != 0;
                m_treeView.CanEdit = (flags & HierarchicalDataFlags.CanEdit) != 0;
                m_treeView.CanRemove = (flags & HierarchicalDataFlags.CanRemove) != 0;
                m_treeView.CanDrag = (flags & HierarchicalDataFlags.CanDrag) != 0;
                m_treeView.CanReorder = (flags & HierarchicalDataFlags.CanReorder) == HierarchicalDataFlags.CanReorder;
                m_treeView.CanReparent = (flags & HierarchicalDataFlags.CanChangeParent) == HierarchicalDataFlags.CanChangeParent;

                m_treeView.SetItems(m_hierarchicalData.GetChildren(null), false);
            }
        }

        private void UnbindData()
        {
            if (m_hierarchicalData != null)
            {
                m_hierarchicalData.HierarchicalDataChanged -= OnHierarchicalDataChanged;
                m_hierarchicalData = null;
            }
        }

        private void RebindData()
        {
            UnbindData();
            BindData();
        }

        private void OnDataPropertyChanged()
        {
            RebindData();
        }

        private void AddChild(object parent, object item)
        {
            if(parent != null && m_treeView.IndexOf(parent) < 0)
            {
                AddChild(m_hierarchicalData.GetParent(parent), parent);
            }

            if (m_treeView.IndexOf(item) < 0)
            {
                m_treeView.AddChild(parent, item);
            }
        }

        private void OnHierarchicalDataChanged(object sender, HierarchicalDataChangedEventArgs e)
        {
            switch (e.Action)
            {
                case HierarchicalDataChangedAction.Add:
                    {
                        AddChild(e.TargetItem, e.Item);
                    }
                    break;
                case HierarchicalDataChangedAction.Insert:
                    {
                        if (m_treeView.IndexOf(e.Item) < 0)
                        {
                            m_treeView.Insert((int)e.TargetItem, e.Item);
                        }
                    }
                    break;
                case HierarchicalDataChangedAction.Remove:
                    {
                        if (e.Item == null && e.TargetItem == null)
                        {
                            m_treeView.RemoveSelectedItems();
                        }
                        else
                        {
                            try
                            {
                                m_treeView.ItemsRemoving -= OnTreeViewItemsRemoving;
                                m_treeView.ItemsRemoved -= OnTreeViewItemsRemoved;
                                m_treeView.RemoveChild(e.TargetItem, e.Item);
                            }
                            finally
                            {
                                m_treeView.ItemsRemoving += OnTreeViewItemsRemoving;
                                m_treeView.ItemsRemoved += OnTreeViewItemsRemoved;
                            }
                        }
                    }
                    break;
                case HierarchicalDataChangedAction.ChangeParent:
                    {
                        object obj = e.Item;
                        object newParent = m_hierarchicalData.GetParent(e.Item);
                        object oldParent = e.TargetItem;

                        bool isNewParentExpanded = true;
                        bool isOldParentExpanded = true;
                        bool isLastChild = false;
                        if (newParent != null)
                        {
                            isNewParentExpanded = m_treeView.IsExpanded(newParent);
                        }

                        if (oldParent != null)
                        {
                            isLastChild = !m_hierarchicalData.HasChildren(oldParent);
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

                            try
                            {
                                m_treeView.ItemsRemoving -= OnTreeViewItemsRemoving;
                                m_treeView.ItemsRemoved -= OnTreeViewItemsRemoved;
                                m_treeView.RemoveChild(oldParent, obj);
                            }
                            finally
                            {
                                m_treeView.ItemsRemoving += OnTreeViewItemsRemoving;
                                m_treeView.ItemsRemoved += OnTreeViewItemsRemoved;
                            }
                        }
                    }
                    break;
                case HierarchicalDataChangedAction.SetNextSibling:
                    {
                        m_treeView.SetNextSibling(e.Item, e.TargetItem);
                    }
                    break;
                case HierarchicalDataChangedAction.SetPrevSibling:
                    {
                        m_treeView.SetPrevSibling(e.Item, e.TargetItem);
                    }
                    break;
                case HierarchicalDataChangedAction.Expand:
                    try
                    {
                        m_treeView.ItemExpanded -= OnTreeViewItemExpanded;
                        m_treeView.Expand(e.Item);
                    }
                    finally
                    {
                        m_treeView.ItemExpanded += OnTreeViewItemExpanded;
                    }
                    break;
                case HierarchicalDataChangedAction.Collapse:
                    try
                    {
                        m_treeView.ItemCollapsed -= OnTreeViewItemCollapsed;
                        m_treeView.Collapse(e.Item);
                    }
                    finally
                    {
                        m_treeView.ItemCollapsed += OnTreeViewItemCollapsed;
                    }
                    break;
                case HierarchicalDataChangedAction.Select:
                    try
                    {
                        m_treeView.SelectionChanged -= OnTreeViewSelectionChanged;
                        m_treeView.SelectedItems = (IEnumerable)e.Item;
                        if (e.TargetItem != null)
                        {
                            if (!m_treeView.IsInViewport(e.TargetItem))
                            {
                                m_treeView.ScrollIntoView(e.TargetItem);
                            }
                        }
                    }
                    finally
                    {
                        m_treeView.SelectionChanged += OnTreeViewSelectionChanged;
                    }
                    break;
                case HierarchicalDataChangedAction.Reset:
                    {
                        if (e.Item != null)
                        {
                            m_treeView.DataBindItem(e.Item);
                        }
                        else
                        {
                            RebindData();
                        }
                    }
                    break;
            }
        }

        private void OnTreeViewItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            Template template = e.ItemPresenter.GetComponent<Template>();
            if (template != null)
            {
                template.InitChildBindings(e.Item);
            }
            else
            {
                Debug.LogWarning($"add {nameof(Template)} component to ItemPresenter");
            }

            if (e.ItemPresenter != e.EditorPresenter)
            {
                template = e.EditorPresenter.GetComponent<Template>();
                if (template != null)
                {
                    template.InitChildBindings(e.Item);
                }
                else
                {
                    Debug.LogWarning($"add {nameof(Template)} component to EditorPresenter");
                }
            }

            HierarchicalDataItemFlags flags = m_hierarchicalData.GetItemFlags(e.Item);
            e.CanSelect = (flags & HierarchicalDataItemFlags.CanSelect) != 0;
            e.CanEdit = (flags & HierarchicalDataItemFlags.CanEdit) != 0;
            e.CanDrag = (flags & HierarchicalDataItemFlags.CanDrag) != 0;
            e.CanBeParent = (flags & HierarchicalDataItemFlags.CanBeParent) != 0;
            e.CanChangeParent = (flags & HierarchicalDataItemFlags.CanChangeParent) == HierarchicalDataItemFlags.CanChangeParent;
            e.HasChildren = m_hierarchicalData.HasChildren(e.Item);
        }

        private void OnTreeViewItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            e.Children = m_hierarchicalData.GetChildren(e.Item);
        }

        private void OnTreeViewSelectionChanged(object sender, SelectionChangedArgs e)
        {
            m_hierarchicalData.Select(m_treeView.SelectedItems);

            SelectionChanged?.Invoke();
        }

        private void OnTreeViewItemsRemoving(object sender, ItemsCancelArgs e)
        {
            if (e.Items == null)
            {
                return;
            }

            for (int i = e.Items.Count - 1; i >= 0; i--)
            {
                if ((m_hierarchicalData.GetItemFlags(e.Items[i]) & HierarchicalDataItemFlags.CanRemove) == 0)
                {
                    e.Items.RemoveAt(i);
                }
            }
        }

        private void OnTreeViewItemsRemoved(object sender, ItemsRemovedArgs e)
        {
            SourceItems = e.Items;
            Target = null;

            ItemsRemoved?.Invoke();

            m_hierarchicalData.Select(m_treeView.SelectedItems);

            SourceItems = null;
        }

        private void OnTreeViewItemDragEnter(object sender, ItemDropCancelArgs e)
        {
            Target = e.DropTarget;
            ItemDragEnter?.Invoke();

            m_TargetPropertySync.SyncFromSource();
            e.Cancel = Target == null;
        }

        private void OnTreeViewItemDragExit(object sender, EventArgs e)
        {
            ItemDragExit?.Invoke();
            Target = null;
        }

        private void OnTreeViewItemBeginDrag(object sender, ItemArgs e)
        {
            LastDropAction = ItemDropAction.None;

            SourceItems = m_treeView.DragItems;
            Target = m_treeView.DropTarget;

            ItemsBeginDrag?.Invoke();
        }

        private void OnTreeViewItemDrag(object sender, ItemArgs e)
        {
            SourceItems = m_treeView.DragItems;
            Target = m_treeView.DropTarget;

            LastDropAction = m_treeView.DropAction;

            ItemsDrag?.Invoke();
        }

        private void OnTreeViewItemBeginDrop(object sender, ItemDropCancelArgs e)
        {
            if (e.IsExternal)
            {
                return;
            }

            SourceItems = m_treeView.DragItems;
            Target = m_treeView.DropTarget;

            ItemsBeginDrop?.Invoke();

            m_TargetPropertySync.SyncFromSource();
            e.Cancel = Target == null;
        }

        private async void OnTreeViewItemDrop(object sender, ItemDropArgs e)
        {
            if (e.IsExternal)
            {
                return;
            }

            SourceItems = m_treeView.DragItems;
            Target = m_treeView.DropTarget;

            try
            {
                if (m_hierarchicalData != null)
                {
                    m_hierarchicalData.HierarchicalDataChanged -= OnHierarchicalDataChanged;
                }

                m_treeView.ItemDropStdHandler<object>(e,
                    (item) => m_hierarchicalData.GetParent(item),
                    (item, parent) => { },
                    (item, parent) => m_hierarchicalData.IndexOf(parent, item),
                    (item, parent) => m_hierarchicalData.Remove(parent, item),
                    (item, parent, i) => m_hierarchicalData.Insert(parent, item, i),
                    (item, parent) => m_hierarchicalData.Add(parent, item));
            }
            finally
            {
                if (m_hierarchicalData != null)
                {
                    m_hierarchicalData.HierarchicalDataChanged += OnHierarchicalDataChanged;
                }
            }

            ItemsDrop?.Invoke();

            await Task.Yield();
            SourceItems = null;
            Target = null;
        }

        private async void OnTreeViewItemEndDrag(object sender, ItemArgs e)
        {
            SourceItems = m_treeView.DragItems;
            Target = m_treeView.DropTarget;
            ItemsEndDrag?.Invoke();

            await Task.Yield();
            SourceItems = null;
            Target = null;
        }

        private void OnTreeViewBeginDragExternal(object sender, EventArgs e)
        {
            ExternalBeginDrag?.Invoke();
        }

        private void OnTreeViewDragExternal(object sender, EventArgs e)
        {
            Target = m_treeView.DropTarget;
            LastDropAction = m_treeView.DropAction;
            ExternalDrag?.Invoke();
        }

        private async void OnTreeViewDropExternal(object sender, EventArgs e)
        {
            Target = m_treeView.DropTarget;
            ExternalDrop?.Invoke();

            await Task.Yield();
            Target = null;
        }

        private void OnTreeViewItemBeginEdit(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            Target = e.Item;
            ItemBeginEdit?.Invoke();
            Target = null;
        }

        private void OnTreeViewItemEndEdit(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            Target = e.Item;
            ItemEndEdit?.Invoke();
            Target = null;
        }

        private void OnTreeViewItemHold(object sender, ItemArgs e)
        {
            if (e.PointerEventData.pointerId == 0)
            {
                Target = e.Items[0];
                ItemHold?.Invoke();
                Target = null;
            }
        }

        private async void OnTreeViewItemClick(object sender, ItemArgs e)
        {
            if (e.PointerEventData.button == PointerEventData.InputButton.Left)
            {
                Target = e.Items[0];
                ItemLeftClick?.Invoke();
                await Task.Yield();
                Target = null;
            }
            else if (e.PointerEventData.button == PointerEventData.InputButton.Right)
            {
                Target = e.Items[0];
                ItemRightClick?.Invoke();
                await Task.Yield();
                Target = null;
            }
        }

        private void OnTreeViewItemDoubleClick(object sender, ItemArgs e)
        {
            if (e.PointerEventData.button == PointerEventData.InputButton.Left)
            {
                Target = e.Items[0];
                ItemDoubleLeftClick?.Invoke();
                Target = null;
            }
            else if (e.PointerEventData.button == PointerEventData.InputButton.Right)
            {
                Target = e.Items[0];
                ItemDoubleRightClick?.Invoke();
                Target = null;
            }
        }

        private void OnTreeViewItemExpanded(object sender, VirtualizingItemExpandingArgs e)
        {
            m_hierarchicalData.Expand(e.Item);
        }

        private void OnTreeViewItemCollapsed(object sender, VirtualizingItemCollapsedArgs e)
        {
            m_hierarchicalData.Collapse(e.Item);
        }

        private void OnTreeViewClick(object sender, PointerEventArgs e)
        {
            if (e.Data.button == PointerEventData.InputButton.Left)
            {
                LeftClick?.Invoke();
            }
            else if (e.Data.button == PointerEventData.InputButton.Right)
            {
                RightClick?.Invoke();
            }
        }

        private void OnTreeViewHold(object sender, PointerEventArgs e)
        {
            Hold?.Invoke();
        }
    }
}
