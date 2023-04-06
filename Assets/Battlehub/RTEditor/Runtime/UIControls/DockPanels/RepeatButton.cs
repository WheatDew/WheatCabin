using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class RepeatButton : Button
    {
        public UnityEvent onPointerDown;
        public UnityEvent onPointerUp;

        private bool m_isPressed;
        public new bool IsPressed
        {
            get { return m_isPressed; }
            set { m_isPressed = value; }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if(eventData != null)
            {
                base.OnPointerDown(eventData);
            }
            
            onPointerDown.Invoke();
            IsPressed = true;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if(eventData != null)
            {
                base.OnPointerUp(eventData);
            }
            
            onPointerUp.Invoke();
            IsPressed = false;
        }
    }
}
