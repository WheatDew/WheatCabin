using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTBuilder
{
    public class ObjectEditorEventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event EventHandler<PointerEventData> Click;
        public event EventHandler<PointerEventData> PointerDown;
        public event EventHandler<PointerEventData> PointerUp;

        public void OnPointerClick(PointerEventData eventData)
        {
            if(Click != null)
            {
                Click(this, eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(PointerDown != null)
            {
                PointerDown(this, eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(PointerUp != null)
            {
                PointerUp(this, eventData);
            }
        }
    }
}

