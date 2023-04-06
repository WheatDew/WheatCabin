using Battlehub.RTCommon;
using Battlehub.RTEditor.Binding;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor.Views
{
    public class View : MonoBehaviour
    {
        [NonSerialized]
        public UnityEvent SelectAll = new UnityEvent();
        
        [NonSerialized]
        public UnityEvent Duplicate = new UnityEvent();

        [NonSerialized]
        public UnityEvent Delete = new UnityEvent();

        [NonSerialized]
        public UnityEvent DragEnter = new UnityEvent();

        [NonSerialized]
        public UnityEvent DragLeave = new UnityEvent();

        [NonSerialized]
        public UnityEvent Drag = new UnityEvent();

        [NonSerialized]
        public UnityEvent Drop = new UnityEvent();

        [NonSerialized]
        public UnityEvent DragObjectsChanged = new UnityEvent();

        [NonSerialized]
        public UnityEvent Activated = new UnityEvent();

        [NonSerialized]
        public UnityEvent Deactivated = new UnityEvent();

        private IEnumerable<object> m_dragObjects;
        public IEnumerable<object> DragObjects
        {
            get { return m_dragObjects; }
            set
            {
                if(m_dragObjects != value)
                {
                    m_dragObjects = value;
                    DragObjectsChanged?.Invoke();
                }
            }
        }

        private bool m_canDropObjects;
        public virtual bool CanDropExternalObjects
        {
            get { return m_canDropObjects; }
            set { m_canDropObjects = value; }
        }

        private bool m_isDragging;
        protected bool IsDraggingOver
        {
            get { return m_isDragging; }
            set { m_isDragging = value; }
        }


        [SerializeField]
        private ViewInput m_baseViewInput;
        protected ViewInput ViewInput
        {
            get { return m_baseViewInput; }
        }

        [SerializeField]
        private RuntimeWindow m_window;
        protected RuntimeWindow Window
        {
            get { return m_window; }
        }
        protected IRTE Editor
        {
            get { return m_window.Editor; }
        }
        protected virtual void Awake()
        {
            if(m_window == null)
            {
                m_window = GetComponent<RuntimeWindow>();
            }

            if (m_window != null)
            {
                m_window.DragEnterEvent += OnDragEnter;
                m_window.DragLeaveEvent += OnDragLeave;
                m_window.DragEvent += OnDrag;
                m_window.DropEvent += OnDrop;

                m_window.Activated += OnActivated;
                m_window.Deactivated += OnDeactivated;
            }
            else
            {
                Debug.LogWarning("Window is null");
            }
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void Start()
        {
            if(m_baseViewInput == null)
            {
                m_baseViewInput = GetComponent<ViewInput>();
            }
            
            if (m_baseViewInput == null)
            {
                m_baseViewInput = gameObject.AddComponent<ViewInput>();
            }   
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
            if(m_window != null)
            {
                m_window.DragEnterEvent -= OnDragEnter;
                m_window.DragLeaveEvent -= OnDragLeave;
                m_window.DragEvent -= OnDrag;
                m_window.DropEvent -= OnDrop;

                m_window.Activated -= OnActivated;
                m_window.Deactivated -= OnDeactivated;
            }

            m_baseViewInput = null;
            m_window = null;
        }


        protected virtual void OnDragEnter(PointerEventData pointerEventData)
        {
            DragObjects = m_window.Editor.DragDrop.DragObjects;
            DragObjectsChanged?.Invoke();

            DragEnter?.Invoke();

            if(!CanDropExternalObjects)
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
            }
        }

        protected virtual void OnDragLeave(PointerEventData pointerEventData)
        {
            DragLeave?.Invoke();

            DragObjects = null;
            DragObjectsChanged?.Invoke();

            Editor.DragDrop.SetCursor(KnownCursor.DropNotAllowed);
        }

        protected virtual void OnDrag(PointerEventData pointerEventData)
        {
            Drag?.Invoke();
        }

        protected virtual void OnDrop(PointerEventData pointerEventData)
        {
            Drop?.Invoke();

            DragObjects = null;
            DragObjectsChanged?.Invoke();
        }


        protected virtual void OnActivated(object sender, EventArgs e)
        {
            Activated?.Invoke();
        }

        protected virtual void OnDeactivated(object sender, EventArgs e)
        {
            Deactivated?.Invoke();
        }

        public static void ReplaceWith<T>(Component component, bool initViewBinding) where T : View
        {
            ReplaceWith(typeof(T), component.gameObject, initViewBinding);
        }

        public static void ReplaceWith<T>(GameObject go, bool initViewBinding) where T : View
        {
            ReplaceWith(typeof(T), go, initViewBinding);
        }

        public static void ReplaceWith(Type type, Component component, bool initViewBinding) 
        {
            ReplaceWith(type, component.gameObject, initViewBinding);
        }

        public static void ReplaceWith(Type type, GameObject go, bool initViewBinding)
        {
            View oldView = go.GetComponent<View>();
            if (oldView == null)
            {
                Debug.Log($"View was not found");
                return;
            }

            if (initViewBinding)
            {
                ViewBinding viewBinding = go.GetComponent<ViewBinding>();

                go.AddComponent(type);

                DestroyImmediate(oldView);

                if (viewBinding != null)
                {
                    viewBinding.Init();
                }
            }
            else
            {
                go.AddComponent(type);
                DestroyImmediate(oldView);
            }
        }
    }
}
