using Battlehub.RTCommon;
using Battlehub.RTHandles;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor.Demo
{
    public class CustomHandleExtension<T> : EditorExtension where T: BaseHandle
    {
        private IWindowManager m_wm;
        private readonly Dictionary<Transform, T> m_windowToHandle = new Dictionary<Transform, T>();
        private bool m_isActive;
        public virtual bool IsActive
        {
            get { return m_isActive; }
            set
            {
                if(m_isActive != value)
                {
                    m_isActive = value;
                    if(m_isActive)
                    {
                        Activate();
                    }
                    else
                    {
                        Deactivate();
                    }
                }
            }
        }

        public IEnumerable<T> Handles
        {
            get { return m_windowToHandle.Values; }
        }

        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            m_wm = IOC.Resolve<IWindowManager>();
            Subscribe();
            if (m_wm == null)
            {
                RuntimeWindow scene = IOC.Resolve<IRTE>().GetWindow(RuntimeWindowType.Scene);
                if (scene != null)
                {
                    CreateHandle(scene);
                }
            }
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            Unsubscribe();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (m_wm != null)
            {
                m_wm.WindowCreated += OnWindowCreated;
                m_wm.AfterLayout += OnAfterLayout;
                m_wm.WindowDestroyed += OnWindowDestroyed;
            }
        }

        private void Unsubscribe()
        {
            if (m_wm != null)
            {
                m_wm.WindowCreated -= OnWindowCreated;
                m_wm.AfterLayout -= OnAfterLayout;
                m_wm.WindowDestroyed -= OnWindowDestroyed;
            }
        }

        private void OnAfterLayout(IWindowManager wm)
        {
            foreach (Transform windowTransform in wm.GetWindows())
            {
                CreateHandle(windowTransform);
            }
        }

        private void OnWindowCreated(Transform windowTransform)
        {
            CreateHandle(windowTransform);
        }

        private void OnWindowDestroyed(Transform windowTransform)
        {
            T handle;
            if (m_windowToHandle.TryGetValue(windowTransform, out handle))
            {
                Destroy(handle.gameObject);
                m_windowToHandle.Remove(windowTransform);
            }
        }

        private void CreateHandle(Transform windowTransform)
        {
            RuntimeWindow scene = windowTransform.GetComponentInChildren<RuntimeWindow>();
            if (scene != null && scene.WindowType == RuntimeWindowType.Scene)
            {
                T handle = CreateHandle(scene);
                m_windowToHandle.Add(windowTransform, handle);
            }
        }

        protected virtual T CreateHandle(RuntimeWindow scene)
        {
            GameObject go = new GameObject(typeof(T).Name);
            go.transform.SetParent(transform, false);
            go.gameObject.SetActive(false);

            T handle = go.AddComponent<T>();
            handle.Window = scene;
            return handle;
        }

        protected virtual void Activate()
        {
            foreach (Transform windowTranform in m_windowToHandle.Keys)
            {
                RuntimeWindow window = windowTranform.GetComponentInChildren<RuntimeWindow>();
                IRuntimeSelectionComponent selectionComponent = window.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                selectionComponent.CustomHandle = m_windowToHandle[windowTranform];
            }

            SetCurrentTool(RuntimeTool.Custom);
        }

        protected virtual void  Deactivate()
        {
            foreach (Transform windowTranform in m_windowToHandle.Keys)
            {
                RuntimeWindow window = windowTranform.GetComponentInChildren<RuntimeWindow>();
                BaseHandle handle = m_windowToHandle[windowTranform];
                if(handle != null)
                {
                    handle.gameObject.SetActive(false);
                }

                IRuntimeSelectionComponent selectionComponent = window.IOCContainer.Resolve<IRuntimeSelectionComponent>();
                selectionComponent.CustomHandle = null;
            }

            SetCurrentTool(RuntimeTool.None);
        }

        protected virtual void SetCurrentTool(RuntimeTool tool)
        {
            IRTE rte = IOC.Resolve<IRTE>();
            rte.Tools.Current = tool;
        }
    }
}
