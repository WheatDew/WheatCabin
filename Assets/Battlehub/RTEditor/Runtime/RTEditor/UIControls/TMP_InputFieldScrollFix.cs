using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public class TMP_InputFieldScrollFix : MonoBehaviour, IScrollHandler
    {
        public void OnScroll(PointerEventData eventData)
        {
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.scrollHandler);
        }
    }
}

