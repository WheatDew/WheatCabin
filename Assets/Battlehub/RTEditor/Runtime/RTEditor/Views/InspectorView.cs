using Battlehub.RTCommon;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.Views
{
    public class InspectorView : View
    {
        [NonSerialized]
        public UnityEvent ObjectEditorParentChanged = new UnityEvent();
        [SerializeField]
        private Transform m_objectEditorParent = null;
        public Transform ObjectEditorParent
        { 
            get { return m_objectEditorParent; }
            set { }
        }

        [NonSerialized]
        public UnityEvent SelectedObjectsChanged = new UnityEvent();
        private UnityObject[] m_selectedObjects = null;
        public UnityObject[] SelectedObjects
        {
            get { return m_selectedObjects; }
            set {  }
        }

        private IRuntimeSelection m_selectionOverride;
        protected override void Start()
        {
            base.Start();

            m_selectionOverride = Window.IOCContainer.Resolve<IRuntimeSelection>();
            if (m_selectionOverride != null)
            {
                m_selectionOverride.SelectionChanged += OnSelectionChanged;
            }
            Editor.Selection.SelectionChanged += OnEditorSelectionChanged;

            UpdateObjectEditorParent();
            UpdateSelectedObjects();
        }

        protected override void OnDestroy()
        {
            if (Editor != null)
            {
                Editor.Selection.SelectionChanged -= OnEditorSelectionChanged;
            }

            if (m_selectionOverride != null)
            {
                m_selectionOverride.SelectionChanged -= OnSelectionChanged;
                m_selectionOverride = null;
            }

            base.OnDestroy();
        }

        private void OnEditorSelectionChanged(UnityObject[] unselectedObjects)
        {
            if (m_selectionOverride == null || m_selectionOverride.activeObject == null)
            {
                m_selectedObjects = Editor.Selection.objects;
                SelectedObjectsChanged?.Invoke();
            }
        }

        private void OnSelectionChanged(UnityObject[] unselectedObjects)
        {
            UpdateSelectedObjects();
        }

        private void UpdateSelectedObjects()
        {
            if (m_selectionOverride != null && m_selectionOverride.activeObject != null)
            {
                m_selectedObjects = m_selectionOverride.objects;
            }
            else
            {
                m_selectedObjects = Editor.Selection.objects;
            }

            SelectedObjectsChanged?.Invoke();
        }

        private void UpdateObjectEditorParent()
        {
            ObjectEditorParentChanged?.Invoke();
        }
    }
}

