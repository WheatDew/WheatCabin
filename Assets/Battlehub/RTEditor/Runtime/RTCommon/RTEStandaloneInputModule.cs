using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTCommon
{
    public interface IRTEInputModule
    {
        event Action Update;
    }

    [DefaultExecutionOrder(-100)]
    public class RTEStandaloneInputModule : StandaloneInputModule, IRTEInputModule
    {
        public event Action Update;

        public bool UseMouse = true;

        protected override void Awake()
        {
            base.Awake();
            IOC.RegisterFallback<IRTEInputModule>(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            IOC.UnregisterFallback<IRTEInputModule>(this);
        }

        public override void UpdateModule()
        {
            base.UpdateModule();
            Update?.Invoke();
        }

        public override void Process()
        {
            bool selectedObject = SendUpdateEventToSelectedObject();
            if (eventSystem.sendNavigationEvents)
            {
                if (!selectedObject)
                    selectedObject |= SendMoveEventToSelectedObject();
                if (!selectedObject)
                    SendSubmitEventToSelectedObject();
            }
            if (ProcessTouchEvents())
            {
                return;
            }
                
            if(UseMouse)
            {
                if (Input.mousePresent)
                {
                    ProcessMouseEvent();
                }
                
            }
            else
            {
                if(Input.GetMouseButtonDown(0))
                {
                    Debug.LogWarning("Processing of touch events only. To enable processing of mouse events set RTEStandaloneInputModule.UseMouse = true");
                }
            }
        }

        private bool ProcessTouchEvents()
        {
            for (int index = 0; index < Input.touchCount; ++index)
            {
                Touch touch = Input.GetTouch(index);
                if (touch.type != TouchType.Indirect)
                {
                    bool pressed;
                    bool released;
                    PointerEventData pointerEventData = GetTouchPointerEventData(touch, out pressed, out released);
                    ProcessTouchPress(pointerEventData, pressed, released);
                    if (!released)
                    {
                        ProcessMove(pointerEventData);
                        ProcessDrag(pointerEventData);
                    }
                    else
                    {
                        RemovePointerData(pointerEventData);
                    }
                }
            }
            return Input.touchCount > 0;
        }

    }
}
