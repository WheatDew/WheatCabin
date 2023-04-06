using Battlehub.UIControls;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Battlehub.RTEditor.Views
{
    public class ToolsView : View
    {
        [SerializeField]
        private Toggle m_viewToggle = null;
        [SerializeField]
        private Toggle m_moveToggle = null;
        [SerializeField]
        private Toggle m_rotateToggle = null;
        [SerializeField]
        private Toggle m_scaleToggle = null;
        [SerializeField]
        private Toggle m_rectToggle = null;

        [SerializeField]
        private Toggle m_pivotRotationToggle = null;
        [SerializeField]
        private Toggle m_pivotModeToggle = null;
        [SerializeField]
        private Toggle m_playToggle = null;

        [SerializeField]
        private Button m_undoButton = null;
        [SerializeField]
        private Button m_redoButton = null;

        [NonSerialized]
        public UnityEvent ViewChanged = new UnityEvent();
        public bool IsView
        {
            get { return m_viewToggle.isOn; }
            set { m_viewToggle.isOn = value; }
        }

        [NonSerialized]
        public UnityEvent MoveChanged = new UnityEvent();
        public bool IsMove
        {
            get { return m_moveToggle.isOn; }
            set { m_moveToggle.isOn = value; }
        }

        [NonSerialized]
        public UnityEvent RotateChanged = new UnityEvent();
        public bool IsRotate
        {
            get { return m_rotateToggle.isOn; }
            set { m_rotateToggle.isOn = value; }
        }

        [NonSerialized]
        public UnityEvent ScaleChanged = new UnityEvent();
        public bool IsScale
        {
            get { return m_scaleToggle.isOn; }
            set { m_scaleToggle.isOn = value; }
        }

        [NonSerialized]
        public UnityEvent RectChanged = new UnityEvent();
        public bool IsRect
        {
            get { return m_rectToggle.isOn; }
            set { m_rectToggle.isOn = value; }
        }

        [NonSerialized]
        public UnityEvent PivotRotationChanged = new UnityEvent();
        public bool IsPivotRotation
        {
            get { return m_pivotRotationToggle.isOn; }
            set { m_pivotRotationToggle.isOn = value; }
        }
        
        [NonSerialized]
        public UnityEvent PivotModeChanged = new UnityEvent();
        public bool IsPivotMode
        {
            get { return m_pivotModeToggle.isOn; }
            set { m_pivotModeToggle.isOn = value; }
        }

        [NonSerialized]
        public UnityEvent PlayChanged = new UnityEvent();
        public bool IsPlay
        {
            get { return m_playToggle.isOn; }
            set { m_playToggle.isOn = value; }
        }

        [NonSerialized]
        public UnityEvent Undo = new UnityEvent();
        public bool IsUndoInteractable
        {
            get { return m_undoButton.interactable; }
            set { m_undoButton.interactable = value; }
        }

        [NonSerialized]
        public UnityEvent Redo = new UnityEvent();
        public bool IsRedoInteractable
        {
            get { return m_redoButton.interactable; }
            set { m_redoButton.interactable = value; }
        }

        protected override void Awake()
        {
            base.Awake();
            UnityEventHelper.AddListener(m_viewToggle, tog => tog.onValueChanged, OnViewChanged);
            UnityEventHelper.AddListener(m_moveToggle, tog => tog.onValueChanged, OnMoveChanged);
            UnityEventHelper.AddListener(m_rotateToggle, tog => tog.onValueChanged, OnRotateChanged);
            UnityEventHelper.AddListener(m_scaleToggle, tog => tog.onValueChanged, OnScaleChanged);
            UnityEventHelper.AddListener(m_rectToggle, tog => tog.onValueChanged, OnRectChanged);

            UnityEventHelper.AddListener(m_pivotModeToggle, tog => tog.onValueChanged, OnPivotModeChanged);
            UnityEventHelper.AddListener(m_pivotRotationToggle, tog => tog.onValueChanged, OnPivotRotationChanged);
            UnityEventHelper.AddListener(m_playToggle, tog => tog.onValueChanged, OnPlayChanged);

            UnityEventHelper.AddListener(m_undoButton, tog => tog.onClick, OnUndoClick);
            UnityEventHelper.AddListener(m_redoButton, tog => tog.onClick, OnRedoClick);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            UnityEventHelper.RemoveListener(m_viewToggle, tog => tog.onValueChanged, OnViewChanged);
            UnityEventHelper.RemoveListener(m_moveToggle, tog => tog.onValueChanged, OnMoveChanged);
            UnityEventHelper.RemoveListener(m_rotateToggle, tog => tog.onValueChanged, OnRotateChanged);
            UnityEventHelper.RemoveListener(m_scaleToggle, tog => tog.onValueChanged, OnScaleChanged);
            UnityEventHelper.RemoveListener(m_rectToggle, tog => tog.onValueChanged, OnRectChanged);

            UnityEventHelper.RemoveListener(m_pivotModeToggle, tog => tog.onValueChanged, OnPivotModeChanged);
            UnityEventHelper.RemoveListener(m_pivotRotationToggle, tog => tog.onValueChanged, OnPivotRotationChanged);
            UnityEventHelper.RemoveListener(m_playToggle, tog => tog.onValueChanged, OnPlayChanged);

            UnityEventHelper.RemoveListener(m_undoButton, tog => tog.onClick, OnUndoClick);
            UnityEventHelper.RemoveListener(m_redoButton, tog => tog.onClick, OnRedoClick);
        }

        private void OnViewChanged(bool value)
        {
            ViewChanged?.Invoke();
        }

        private void OnMoveChanged(bool value)
        {
            MoveChanged?.Invoke();
        }

        private void OnRotateChanged(bool value)
        {
            RotateChanged?.Invoke();
        }

        private void OnScaleChanged(bool value)
        {
            ScaleChanged?.Invoke();
        }

        private void OnRectChanged(bool value)
        {
            RectChanged?.Invoke();
        }

        private void OnPivotRotationChanged(bool value)
        {
            PivotRotationChanged?.Invoke();
        }

        private void OnPivotModeChanged(bool value)
        {
            PivotModeChanged?.Invoke();
        }

        private void OnPlayChanged(bool value)
        {
            PlayChanged?.Invoke();
        }
        
        private void OnUndoClick()
        {
            Undo?.Invoke();
        }

        private void OnRedoClick()
        {
            Redo?.Invoke();
        }
    }
}
