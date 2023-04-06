using Battlehub.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTCommon
{
    public delegate void DragDropEventHander(PointerEventData pointerEventData);

    public interface IDragDrop
    {
        event DragDropEventHander BeginDrag;
        event DragDropEventHander Drag;
        event DragDropEventHander Drop;

        object[] DragObjects
        {
            get;
        }

        object Source
        {
            get;
        }

        bool InProgress
        {
            get;
        }

        void Reset();

        void SetCursor(KnownCursor cursorType);
        
        void RaiseBeginDrag(object source, object[] dragItems, PointerEventData pointerEventData, KnownCursor initialCursor = KnownCursor.DropNotAllowed);

        void RaiseDrag(PointerEventData eventData);

        void RaiseDrop(PointerEventData pointerEventData);
    }

    public class DragDrop : IDragDrop
    {
        private object m_cursorLocker = new object();
        private KnownCursor m_currentCursorType;

        public event DragDropEventHander BeginDrag;
        public event DragDropEventHander Drag;
        public event DragDropEventHander Drop;

        public object[] DragObjects
        {
            get;
            private set;
        }

        public bool InProgress
        {
            get { return DragObjects != null && DragObjects.Length > 0; }
        }

        private object m_source;
        public object Source
        {
            get { return m_source; }
        }

        private IRTE m_editor;
        public DragDrop(IRTE rte)
        {
            m_editor = rte;
        }

        public void Reset()
        {
            DragObjects = null;
            ResetCursor();
        }

        public object DragObject
        {
            get
            {
                if (DragObjects == null || DragObjects.Length == 0)
                {
                    return null;
                }

                return DragObjects[0];
            }
        }

        public void SetCursor(KnownCursor cursorType)
        {
            if (m_currentCursorType != cursorType && m_editor.CursorHelper.SetCursor(m_cursorLocker, cursorType))
            {
                m_currentCursorType = cursorType;
            }
            
        }

        public void ResetCursor()
        {
            m_editor.CursorHelper.ResetCursor(m_cursorLocker);
            m_currentCursorType = KnownCursor.None;
        }

        public void RaiseBeginDrag(object source, object[] dragItems, PointerEventData pointerEventData, KnownCursor initialCursor = KnownCursor.DropNotAllowed)
        {
            if(dragItems == null)
            {
                return;
            }

            if(m_editor.IsBusy)
            {
                return;
            }

            m_currentCursorType = KnownCursor.None;
            m_source = source;
            DragObjects = dragItems;
            if(initialCursor != KnownCursor.None)
            {
                SetCursor(initialCursor);
            }
            if (BeginDrag != null)
            {
                BeginDrag(pointerEventData);
            }
        }

        public void RaiseDrag(PointerEventData eventData)
        {
            if(InProgress)
            {
                if (Drag != null)
                {
                    Drag(eventData);
                }
            }
        }

        public void RaiseDrop(PointerEventData pointerEventData)
        {
            if(InProgress)
            {
                if (Drop != null)
                {
                    Drop(pointerEventData);
                }

                ResetCursor();
                DragObjects = null;
                m_source = null;
            }
        }
    }

}
