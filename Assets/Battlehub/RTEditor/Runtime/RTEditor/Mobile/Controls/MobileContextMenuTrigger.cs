using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor.Mobile.Controls
{
    public class MobileContextMenuTrigger : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent OnContextMenu;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            OnContextMenu?.Invoke();
        }
    }

}
