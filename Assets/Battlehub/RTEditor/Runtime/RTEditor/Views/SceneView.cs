using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor.Views
{
    public class SceneView : View
    {
        [NonSerialized]
        public UnityEvent PointerChanged = new UnityEvent();
        public Ray Pointer
        {
            get;
            set;
        }

        [NonSerialized]
        public UnityEvent CameraTransformChanged = new UnityEvent();
        public Transform CameraTransform
        {
            get;
            set;
        }        
        protected override void Awake()
        {
            base.Awake();
            Window.ActivateOnAnyKey = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CameraTransform = null;
        }

        protected override void OnDragEnter(PointerEventData pointerEventData)
        {
            Pointer = Window.Pointer;
            PointerChanged?.Invoke();

            CameraTransform = Window.Camera.transform;
            CameraTransformChanged?.Invoke();

            base.OnDragEnter(pointerEventData);

            if (CanDropExternalObjects)
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
        }

        protected override void OnDrag(PointerEventData pointerEventData)
        {
            Pointer = Window.Pointer;
            PointerChanged?.Invoke();

            base.OnDrag(pointerEventData);

            if (CanDropExternalObjects)
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
            else
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            }
        }

        protected virtual void Update()
        {
            ViewInput.HandleInput();
        }
        


    }

}
