using Battlehub.UIControls;
using Battlehub.Utils;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor.Views
{
    public class HierarchicalDataView : View
    {
        private VirtualizingTreeView m_treeView;
        protected VirtualizingTreeView TreeView
        {
            get { return m_treeView; }
        }

        public override bool CanDropExternalObjects
        {
            get { return base.CanDropExternalObjects; }
            set
            {
                
                base.CanDropExternalObjects = value;
                if (m_treeView == null)
                {
                    return;
                }

                if (DragObjects != null)
                {
                    if (base.CanDropExternalObjects)
                    {
                        Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
                    }
                    else
                    {
                        Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
                        m_treeView.ClearTarget();
                    }
                }
                else
                {
                    if (!CanDropExternalObjects)
                    {
                        m_treeView.ExternalDrop();
                    }
                }
            }
        }

        private bool m_canDropItems;
        public virtual bool CanDropItems
        {
            get { return m_canDropItems; }
            set
            {
                if (m_canDropItems != value)
                {
                    if (m_treeView.DragItems != null)
                    {
                        m_canDropItems = value;

                        if (m_canDropItems)
                        {
                            Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
                        }
                        else
                        {
                            Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
                        }
                    }
                }
            }
        }

        public virtual bool IsEditing
        {
            get
            {
                VirtualizingTreeViewItem treeViewItem = m_treeView != null ? m_treeView.GetTreeViewItem(m_treeView.SelectedItem) : null;
                if (treeViewItem != null)
                {
                    return treeViewItem.IsEditing;
                }
                return false;
            }
            set
            {
                if (m_treeView == null)
                {
                    return;
                }

                VirtualizingTreeViewItem treeViewItem = m_treeView.GetTreeViewItem(m_treeView.SelectedItem);
                if (treeViewItem != null && treeViewItem.CanEdit)
                {
                    treeViewItem.IsEditing = true;
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            m_treeView = GetComponentInChildren<VirtualizingTreeView>(true);
            m_treeView.ItemBeginDrag += OnTreeViewItemBeginDrag;
            m_treeView.ItemDrag += OnTreeViewItemDrag;
            m_treeView.ItemDrop += OnTreeViewItemDrop;
            m_treeView.ItemEndDrag += OnTreeViewItemEndDrag;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_treeView != null)
            {
                m_treeView.ItemBeginDrag -= OnTreeViewItemBeginDrag;
                m_treeView.ItemDrag -= OnTreeViewItemDrag;
                m_treeView.ItemDrop -= OnTreeViewItemDrop;
                m_treeView.ItemEndDrag -= OnTreeViewItemEndDrag;
            }
        }

        protected virtual void OnTreeViewItemBeginDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseBeginDrag(Window, e.Items, e.PointerEventData, KnownCursor.None);
            if (!CanDropItems)
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            }
        }

        protected virtual void OnTreeViewItemDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrag(e.PointerEventData);
        }

        protected virtual void OnTreeViewItemDrop(object sender, ItemDropArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);
        }

        protected virtual void OnTreeViewItemEndDrag(object sender, ItemArgs e)
        {
            if (Editor.DragDrop.InProgress)
            {
                Editor.DragDrop.RaiseDrop(e.PointerEventData);
            }
        }

        protected override void OnDragEnter(PointerEventData pointerEventData)
        {
            base.OnDragEnter(pointerEventData);
            m_treeView.ExternalBeginDrag(pointerEventData.position);
        }

        protected override void OnDragLeave(PointerEventData pointerEventData)
        {
            base.OnDragLeave(pointerEventData);
            if (m_treeView.IsExternalDragInProgress)
            {
                m_treeView.ExternalDrop();
            }
        }

        protected override void OnDrag(PointerEventData pointerEventData)
        {
            base.OnDrag(pointerEventData);
            m_treeView.ExternalDrag(pointerEventData.position);
        }

        protected override void OnDrop(PointerEventData pointerEventData)
        {
            base.OnDrop(pointerEventData);
            if (!CanDropExternalObjects)
            {
                m_treeView.ExternalDrop();
            }
            //CanDropObject = false by corresponding ViewModel
        }
    }
}

