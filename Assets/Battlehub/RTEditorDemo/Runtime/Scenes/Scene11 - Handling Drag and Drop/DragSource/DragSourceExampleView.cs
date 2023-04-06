using Battlehub.RTEditor.Views;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor.Examples.Scene11.Views
{
    public class DragSourceExampleView : View, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public UnityEvent BeginDrag;
        public UnityEvent EndDrag;

        public object[] DragItems
        {
            get;
            set;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            BeginDrag?.Invoke();

            Editor.DragDrop.RaiseBeginDrag(this, DragItems, eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            Editor.DragDrop.RaiseDrag(eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            Editor.DragDrop.RaiseDrop(eventData);

            EndDrag?.Invoke();
        }
    }
}


