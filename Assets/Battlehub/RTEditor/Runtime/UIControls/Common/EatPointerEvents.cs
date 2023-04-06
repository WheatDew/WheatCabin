using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls.Common
{
    public class EatPointerEvents : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            eventData.Use();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            eventData.Use();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            eventData.Use();
        }
    }
}
