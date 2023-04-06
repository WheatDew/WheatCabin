using Battlehub.RTCommon;
using System.Collections.Generic;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class ViewModel : ViewModelBase
    {
        private bool m_canDropExternalDragObjects;
        [Binding]
        public bool CanDropExternalObjects
        {
            get { return m_canDropExternalDragObjects; }
            set
            {
                if (m_canDropExternalDragObjects != value)
                {
                    m_canDropExternalDragObjects = value;
                    RaisePropertyChanged(nameof(CanDropExternalObjects));
                }
            }
        }

        private IEnumerable<object> m_externalDragObjects;
        [Binding]
        public IEnumerable<object> ExternalDragObjects
        {
            get { return m_externalDragObjects; }
            set { m_externalDragObjects = value; }
        }

        private IRuntimeEditor m_editor;
        protected IRuntimeEditor Editor
        {
            get { return m_editor; }
        }

        private ILocalization m_localization;
        protected ILocalization Localization
        {
            get { return m_localization; }
        }

        private IWindowManager m_windowManager;
        protected IWindowManager WindowManager
        {
            get { return m_windowManager; }
        }

        protected IRuntimeSelection Selection
        {
            get;
            private set;
        }

        protected IRuntimeUndo Undo
        {
            get;
            private set;
        }

        public IRuntimeSelection SelectionOverride
        {
            private get;
            set;
        }

        public IRuntimeUndo UndoOverride
        {
            private get;
            set;
        }

        protected virtual void Awake()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();
            m_localization = IOC.Resolve<ILocalization>();
            m_windowManager = IOC.Resolve<IWindowManager>();
        }
    
        protected virtual void OnEnable()
        {
            Selection = SelectionOverride != null ? SelectionOverride : m_editor.Selection;
            Undo = UndoOverride != null ? UndoOverride : m_editor.Undo;
        }

        protected virtual void Start()
        {

        }

        protected virtual void OnDisable()
        {
           
        }

        protected virtual void OnDestroy()
        {
            m_editor = null;
            m_localization = null;
            m_windowManager = null;

            Selection = null;
            Undo = null;
        }

        [Binding]
        public virtual void OnActivated()
        {

        }

        [Binding]
        public virtual void OnDeactivated()
        {

        }

        [Binding]
        public virtual void OnSelectAll()
        {
        }

        [Binding]
        public virtual void OnDelete()
        {
            
        }

        [Binding]
        public virtual void OnDuplicate()
        {
            
        }


        [Binding]
        public virtual void OnExternalObjectEnter()
        {
            
        }

        [Binding]
        public virtual void OnExternalObjectLeave()
        {
            
        }

        [Binding]
        public virtual void OnExternalObjectDrag()
        {
        }

        [Binding]
        public virtual void OnExternalObjectDrop()
        {
            
        }

    }

}


