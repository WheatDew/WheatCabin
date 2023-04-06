using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace Battlehub.UIControls.Binding
{   
    [RequireComponent(typeof(VirtualizingListBox))]
    public class VirtualizingListBoxBinding : ControlBinding
    {
        public UnityEvent ItemHold;
        public UnityEvent ItemLeftClick;
        public UnityEvent ItemRightClick;
        public UnityEvent ItemDoubleLeftClick;
        public UnityEvent ItemDoubleRightClick;

        public override Component TargetControl
        {
            get { return this; }
        }

        #region Items
        [SerializeField]
        private string m_viewModelItemsPropertyName = string.Empty;

        public string ViewModelItemsPropertyName
        {
            get { return m_viewModelItemsPropertyName; }
            set { m_viewModelItemsPropertyName = value; }
        }

        [SerializeField]
        private string m_itemsViewModelToUIAdapter;

        public string ItemsViewModelToUIAdapter
        {
            get { return m_itemsViewModelToUIAdapter; }
            set { m_itemsViewModelToUIAdapter = value; }
        }

        [SerializeField]
        private string m_itemsUIToViewModelAdapter;
        public string ItemsUIToViewModelAdapter
        {
            get { return m_itemsUIToViewModelAdapter; }
            set { m_itemsUIToViewModelAdapter = value; }
        }

        /// <summary>
        /// Watches the items property in the view-model for changes.
        /// </summary>
        private PropertyWatcher m_itemsPropertyWatcher;

        /// <summary>
        /// Syncrhonises the value between two properties using reflection.
        /// </summary>
        private PropertySync m_itemsPropertySync;
        #endregion

        #region SelectedItems
        [SerializeField]
        private string m_viewModelSelectedItemsPropertyName = string.Empty;

        /// <summary>
        /// Name of the property in the view model to bind for the current selection.
        /// </summary>
        public string ViewModelSelectedItemsPropertyName
        {
            get { return m_viewModelSelectedItemsPropertyName; }
            set { m_viewModelSelectedItemsPropertyName = value; }
        }

        /// <summary>
        /// Type name of the adapter for converting a selection value in the 
        /// view model to what the UI expects (which should be a string).
        /// </summary>
        [SerializeField]
        private string m_selectedItemsViewModelToUIAdapter;

        public string SelectedItemsViewModelToUIAdapter
        {
            get { return m_selectedItemsViewModelToUIAdapter; }
            set { m_selectedItemsViewModelToUIAdapter = value; }
        }

        /// <summary>
        /// Type name of the adapter for converting a selection value in the 
        /// UI back to the type needed by the view model.
        /// </summary>
        [SerializeField]
        private string m_selectedItemsUIToViewModelAdapter;

        public string SelectedItemsUIToViewModelAdapter
        {
            get { return m_selectedItemsUIToViewModelAdapter; }
            set { m_selectedItemsUIToViewModelAdapter = value; }
        }

        /// <summary>
        /// Watches the selection property in the view-model for changes.
        /// </summary>
        private PropertyWatcher m_selectedItemsPropertyWatcher;

        /// <summary>
        /// Syncrhonises the value between two properties using reflection.
        /// </summary>
        private PropertySync m_selectedItemsPropertySync;

        /// <summary>
        /// Used to remember the selection if it gets set before the options list is set.
        /// </summary>
        private IEnumerable m_selectedItems = null;

        /// <summary>
        /// String of the text of the currently selected option.
        /// </summary>
        public IEnumerable SelectedItems
        {
            get
            {
                return m_selectedItems;
            }
            set
            {
                if (m_selectedItems == value)
                {
                    return;
                }

                m_selectedItems = value;
                UpdateSelectedItems();
            }
        }

        /// <summary>
        /// Update the selected option.
        /// </summary>
        private void UpdateSelectedItems()
        {
            if (m_virtualizingListBox == null)
            {
                return; // Not connected.
            }

            m_virtualizingListBox.SelectedItems = m_selectedItems;

            if(ScrollSelectedIntoView)
            {
                if (m_virtualizingListBox.SelectedItem != null)
                {
                    if(!m_virtualizingListBox.IsInViewport(m_virtualizingListBox.SelectedItem))
                    {
                        m_virtualizingListBox.ScrollIntoView(m_virtualizingListBox.SelectedItem);
                    }
                }
            }
        }
        #endregion

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

        #region Options

        [SerializeField]
        private bool m_canSelect = true;
        public bool CanSelect
        {
            get { return m_canSelect; }
            set { m_canSelect = value; }
        }

        [SerializeField]
        private bool m_canReorder = true;
        public bool CanReorder
        { 
            get { return m_canReorder; }
            set 
            {
                if(m_canReorder != value)
                {
                    m_canReorder = value;
                    if(m_virtualizingListBox != null)
                    {
                        m_virtualizingListBox.CanDrag = value;
                    }
                }
            }
        }

        [SerializeField]
        private bool m_canRemove = true;
        public bool CanRemove
        {
            get { return m_canRemove; }
            set
            {
                if(m_canRemove != value)
                {
                    m_canRemove = value;
                    if(m_virtualizingListBox != null)
                    {
                        m_virtualizingListBox.CanRemove = value;
                    }
                }
            }
        }

        [SerializeField]
        private bool m_selectOnPointerUp = true;
        public bool SelectOnPointerUp
        {
            get { return m_selectOnPointerUp; }
            set
            {
                if (m_selectOnPointerUp != value)
                {
                    m_selectOnPointerUp = value;
                    if (m_virtualizingListBox != null)
                    {
                        m_virtualizingListBox.SelectOnPointerUp = value;
                    }
                }
            }
        }

        [SerializeField]
        private bool m_canUnselectAll = true;
        public bool CanUnselectAll
        {
            get { return m_canUnselectAll; }
            set
            {
                if(m_canUnselectAll != value)
                {
                    m_canUnselectAll = value;
                    if(m_virtualizingListBox != null)
                    {
                        m_virtualizingListBox.CanUnselectAll = value;
                    }
                }
            }
        }

        [SerializeField]
        private bool m_scrollSelectedIntoView = false;
        public bool ScrollSelectedIntoView
        {
            get { return m_scrollSelectedIntoView; }
            set { m_scrollSelectedIntoView = value; }
        }

        #endregion

        private VirtualizingListBox m_virtualizingListBox;

        public override void Connect()
        {
            base.Connect();

            m_virtualizingListBox = GetComponent<VirtualizingListBox>();
            m_virtualizingListBox.CanDrag = CanReorder;
            m_virtualizingListBox.CanReorder = CanReorder;
            m_virtualizingListBox.CanRemove = CanRemove;
            m_virtualizingListBox.SelectOnPointerUp = SelectOnPointerUp;
            m_virtualizingListBox.CanMultiSelect = false;
            m_virtualizingListBox.CanSelectAll = false;
            m_virtualizingListBox.CanUnselectAll = CanUnselectAll;
            
            m_virtualizingListBox.ItemsChanged += OnListBoxItemsChanged;
            m_virtualizingListBox.ItemDataBinding += OnListBoxItemDataBinding;
            m_virtualizingListBox.SelectionChanged += OnListBoxSelectionChanged;
            m_virtualizingListBox.ItemsRemoved += OnListBoxItemsRemoved;
            m_virtualizingListBox.ItemHold += OnItemHold;
            m_virtualizingListBox.ItemClick += OnItemClick;
            m_virtualizingListBox.ItemDoubleClick += OnItemDoubleClick;
            
            PropertyEndPoint itemsPropertyEndPoint = MakeViewModelEndPoint(m_viewModelItemsPropertyName, m_itemsUIToViewModelAdapter, null);
            m_itemsPropertySync = new PropertySync(
               // Source
               itemsPropertyEndPoint,
               // Dest
               new PropertyEndPoint(
                    m_virtualizingListBox,
                    "Items",
                    CreateAdapter(m_itemsViewModelToUIAdapter),
                    null,
                    "view",
                    this),
               null,
               this
            );

            m_itemsPropertyWatcher = itemsPropertyEndPoint
                .Watch(() => m_itemsPropertySync.SyncFromSource());

            // Copy the initial value from view-model to view.
            m_itemsPropertySync.SyncFromSource();


            if (!string.IsNullOrWhiteSpace(m_viewModelSelectedItemsPropertyName))
            {
                PropertyEndPoint selectedItemsPropertyEndPoint = MakeViewModelEndPoint(m_viewModelSelectedItemsPropertyName, m_selectedItemsUIToViewModelAdapter, null);
                m_selectedItemsPropertySync = new PropertySync(
                   // Source
                   selectedItemsPropertyEndPoint,
                   // Dest
                   new PropertyEndPoint(
                        this,
                        "SelectedItems",
                        CreateAdapter(m_selectedItemsViewModelToUIAdapter),
                        null,
                        "view",
                        this),
                   null,
                   this
                );

                m_selectedItemsPropertyWatcher = selectedItemsPropertyEndPoint
                    .Watch(() => m_selectedItemsPropertySync.SyncFromSource());

                // Copy the initial value from view-model to view.
                m_selectedItemsPropertySync.SyncFromSource();
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

            if (m_itemsPropertyWatcher != null)
            {
                m_itemsPropertyWatcher.Dispose();
                m_itemsPropertyWatcher = null;
            }

            if (m_selectedItemsPropertyWatcher != null)
            {
                m_selectedItemsPropertyWatcher.Dispose();
                m_selectedItemsPropertyWatcher = null;
            }

            if (m_virtualizingListBox != null)
            {
                m_virtualizingListBox.ItemsChanged -= OnListBoxItemsChanged;
                m_virtualizingListBox.ItemDataBinding -= OnListBoxItemDataBinding;
                m_virtualizingListBox.SelectionChanged -= OnListBoxSelectionChanged;
                m_virtualizingListBox.ItemsRemoved -= OnListBoxItemsRemoved;
                m_virtualizingListBox.ItemHold -= OnItemHold;
                m_virtualizingListBox.ItemClick -= OnItemClick;
                m_virtualizingListBox.ItemDoubleClick -= OnItemDoubleClick;
                m_virtualizingListBox = null;
            }
        }

        private void OnListBoxItemDataBinding(object sender, ItemDataBindingArgs e)
        {
            e.CanDrag = CanReorder;
            e.CanSelect = CanSelect;

            Template template = e.ItemPresenter.GetComponent<Template>();
            if(template != null)
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
                if(template != null)
                {
                    template.InitChildBindings(e.Item);
                }
                else
                {
                    Debug.LogWarning($"add {nameof(Template)} component to EditorPresenter");
                }
            }
        }

        private void OnListBoxSelectionChanged(object sender, SelectionChangedArgs e)
        {
            m_selectedItems = m_virtualizingListBox.SelectedItems;
            m_selectedItemsPropertySync.SyncFromDest();
        }

        private void OnListBoxItemsRemoved(object sender, ItemsRemovedArgs e)
        {
            m_selectedItems = m_virtualizingListBox.SelectedItems;
            m_selectedItemsPropertySync.SyncFromDest();
        }

        private void OnListBoxItemsChanged(object sender, System.EventArgs e)
        {
            m_itemsPropertySync.SyncFromDest();
        }

        private void OnItemHold(object sender, ItemArgs e)
        {
            if (e.PointerEventData.pointerId == 0)
            {
                Target = e.Items[0];
                ItemHold?.Invoke();
                Target = null;
            }
        }

        private void OnItemClick(object sender, ItemArgs e)
        {
            var button = e.PointerEventData.button;
            if (button == PointerEventData.InputButton.Left)
            {
                Target = e.Items[0];
                ItemLeftClick?.Invoke();
                Target = null;
            }
            else if (button == PointerEventData.InputButton.Right)
            {
                Target = e.Items[0];
                ItemRightClick?.Invoke();
                Target = null;
            }
        }

        private void OnItemDoubleClick(object sender, ItemArgs e)
        {
            var button = e.PointerEventData.button;
            if (button == PointerEventData.InputButton.Left)
            {
                Target = e.Items[0];
                ItemDoubleLeftClick?.Invoke();
                Target = null;
            }
            else if (button == PointerEventData.InputButton.Right)
            {
                Target = e.Items[0];
                ItemDoubleLeftClick?.Invoke();
                Target = null;
            }
        }
    }

}
