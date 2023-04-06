using Battlehub.UIControls.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ScrollbarClampValues : MonoBehaviour
    {
        private DragAndDropListener m_listener;
        private Scrollbar m_scrollbar;
        
        private void Awake()
        {
            m_scrollbar = GetComponent<Scrollbar>();
            m_listener = gameObject.AddComponent<DragAndDropListener>();
            m_listener.Drop += OnDrop;
            m_listener.EndDrag += OnEndDrag;
        }
        private void OnDestroy()
        {
            if(m_listener != null)
            {
                m_listener.Drop -= OnDrop;
                m_listener.EndDrag -= OnEndDrag;
            }
        }

        private void OnEndDrag(PointerEventData eventData)
        {
            m_scrollbar.value = Mathf.Clamp(m_scrollbar.value, ScrollbarResizer.k_minValue, ScrollbarResizer.k_maxValue);
        }

        private void OnDrop(PointerEventData eventData)
        {
            m_scrollbar.value = Mathf.Clamp(m_scrollbar.value, ScrollbarResizer.k_minValue, ScrollbarResizer.k_maxValue);
        }
    }

}
