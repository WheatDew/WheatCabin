using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.UIControls.DockPanels
{
    public class EatDragEvent : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            eventData.Use();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            eventData.Use();
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            eventData.Use();
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.Use();

            IPointerDownHandler pointerDown = GetComponentInParent<Region>();
            pointerDown.OnPointerDown(eventData);
        }
    }

}
