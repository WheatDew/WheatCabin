/// <summary>
/// Project : Easy Build System
/// Class : Demo_UIVirtualButton.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.UI.Mobile
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace EasyBuildSystem.Examples.Bases.Scripts.UI.Mobile
{
    public class Demo_UIVirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Output")]
        public UnityEvent<bool> buttonStateOutputEvent;
        public UnityEvent buttonClickOutputEvent;

        public void OnPointerDown(PointerEventData eventData)
        {
            OutputButtonStateValue(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OutputButtonStateValue(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OutputButtonClickEvent();
        }

        void OutputButtonStateValue(bool buttonState)
        {
            buttonStateOutputEvent?.Invoke(buttonState);
        }

        void OutputButtonClickEvent()
        {
            buttonClickOutputEvent.Invoke();
        }
    }
}