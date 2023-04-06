using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface ICustomSelectionComponent
    {
        event Action<IRuntimeSelectionComponent> CreateCustomHandle;
        event Action<IRuntimeSelectionComponent> DestroyCustomHandle;

        bool Enabled
        {
            get;
            set;
        }

        IRuntimeSelection Selection
        {
            get;
        }
    }

    [DefaultExecutionOrder(-50)]
    public class CustomSelectionComponent : MonoBehaviour, ICustomSelectionComponent
    {
        public event Action<IRuntimeSelectionComponent> CreateCustomHandle;
        public event Action<IRuntimeSelectionComponent> DestroyCustomHandle;

        private IRTE m_editor;
        private IWindowManager m_wm;
        
        private IRuntimeSelection m_selection;
        public IRuntimeSelection Selection
        {
            get { return m_selection; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if(this)
                {
                    enabled = value;
                }
            }
        }

        protected void Awake()
        {  
            m_wm = IOC.Resolve<IWindowManager>();
            m_editor = IOC.Resolve<IRTE>();
            m_selection = new RuntimeSelection(m_editor);
            m_selection.EnableUndo = false;
            IOC.RegisterFallback<ICustomSelectionComponent>(this);
            Enabled = false;
        }

        protected void OnDestroy()
        {
            IOC.UnregisterFallback<ICustomSelectionComponent>(this);
        }

        private void OnEnable()
        {
            if (m_wm != null)
            {
                foreach (Transform windowTransform in m_wm.GetWindows(RuntimeWindowType.Scene.ToString()))
                {
                    TryToEnableCustomSelection(windowTransform);
                }

                m_wm.WindowCreated += OnWindowCreated;
                m_wm.WindowDestroyed += OnWindowDestroyed;
            }
        }

        private void OnDisable()
        {
            if (m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
                m_wm.WindowDestroyed -= OnWindowDestroyed;

                m_selection.activeObject = null;
                foreach (Transform windowTransform in m_wm.GetWindows(RuntimeWindowType.Scene.ToString()))
                {
                    TryToDisableCustomSelection(windowTransform);
                }
            }
        }

        private void OnWindowCreated(Transform windowTransform)
        {
            TryToEnableCustomSelection(windowTransform);
        }

        private void OnWindowDestroyed(Transform windowTransform)
        {
            TryToDisableCustomSelection(windowTransform);
        }

        private void TryToEnableCustomSelection(Transform windowTransform)
        {
            RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
            if (window != null && window.WindowType == RuntimeWindowType.Scene)
            {
                IRuntimeSelectionComponent selectionComponent = window.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                if (selectionComponent != null)
                {
                    selectionComponent.Selection = m_selection;
                    if(CreateCustomHandle != null)
                    {
                        CreateCustomHandle(selectionComponent);
                    }
                }
            }
        }

        private void TryToDisableCustomSelection(Transform windowTransform)
        {
            RuntimeWindow window = windowTransform.GetComponent<RuntimeWindow>();
            if (window != null && window.WindowType == RuntimeWindowType.Scene)
            {
                IRuntimeSelectionComponent selectionComponent = window.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                if (selectionComponent != null)
                {
                    selectionComponent.Selection = null;
                    if (DestroyCustomHandle != null)
                    {
                        DestroyCustomHandle(selectionComponent);
                    }
                }
            }
        }
    }

}


