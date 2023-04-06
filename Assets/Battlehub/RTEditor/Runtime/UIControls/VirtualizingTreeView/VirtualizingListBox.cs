using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls
{
    public class VirtualizingListBox : VirtualizingItemsControl<ItemDataBindingArgs>
    {
        [SerializeField]
        private ScrollMode m_scrollRectMode = ScrollMode.Continuous;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            ScrollRect.ScrollMode = m_scrollRectMode;
        }

        public override void SetItems(IEnumerable value, bool resetSelection)
        {
            if(ScrollRect.ItemsCount > 0)
            {
                ScrollRect.Index = 0;
            }
            
            base.SetItems(value, resetSelection);
        }

        public override void DataBindItem(object item, VirtualizingItemContainer itemContainer)
        {
            base.DataBindItem(item, itemContainer);
        }

        private protected override void OnItemPointerEnter(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            base.OnItemPointerEnter(sender, eventData);
            if (DragItems != null && DragItems.Length == 1)
            {
                DropMarker.SetTarget(null);
            }
        }

        private protected override void OnItemDrag(VirtualizingItemContainer sender, PointerEventData eventData)
        {
            if (!CanHandleEvent(sender))
            {
                return;
            }

            base.OnItemDrag(sender, eventData);
            if (DragItems != null && DragItems.Length == 1)
            {
                object dropTarget = DropTarget;
                ItemContainerData containerData = GetItemContainerData(dropTarget);

                if (containerData != null)
                {
                    if (IndexOf(dropTarget) < IndexOf(DragItems[0]))
                    {
                        DropMarker.Action = ItemDropAction.SetPrevSibling;
                    }
                    else
                    {
                        DropMarker.Action = ItemDropAction.SetNextSibling;
                    }

                    Drop(m_dragItems, containerData, DropMarker.Action);
                    DropMarker.Action = ItemDropAction.None;
                    DropMarker.SetTarget(null);
                    DataBindVisible();
                }
            }
        }

        public void ScrollIntoView(object obj)
        {
            int index = IndexOf(obj);
            if (index < 0)
            {
                throw new InvalidOperationException(string.Format("item {0} does not exist or not visible", obj));
            }
            VirtualizingScrollRect scrollRect = GetComponentInChildren<VirtualizingScrollRect>();
            scrollRect.Index = index;
        }
    }
}
