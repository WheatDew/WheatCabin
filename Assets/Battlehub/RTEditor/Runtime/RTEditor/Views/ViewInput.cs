using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Views
{
    public class ViewInput : MonoBehaviour
    {
        private KeyCode ModifierKey
        {
            get
            {
#if UNITY_EDITOR
                return KeyCode.LeftShift;
#else
                return KeyCode.LeftControl;
#endif
            }
        }

        public virtual bool SelectAllAction()
        {
            return m_editor.Input.GetKeyDown(KeyCode.A) && Input.GetKey(ModifierKey);
        }
        public virtual bool DuplicateAction()
        {
            return m_editor.Input.GetKeyDown(KeyCode.D) && Input.GetKey(ModifierKey);
        }
        public virtual bool DeleteAction()
        {
            return m_editor.Input.GetKeyDown(KeyCode.Delete);
        }

        private IRTE m_editor;
        protected IRTE Editor
        {
            get { return m_editor; }
        }

        private IWindowManager m_wm;
        protected IWindowManager WindowManager
        {
            get { return m_wm; }
        }

        private RuntimeWindow m_window;
        protected RuntimeWindow Window
        {
            get { return m_window; }
        }

        private View m_view;
        protected View View
        {
            get { return m_view; }
        }

        protected virtual void Awake()
        {
            m_window = GetComponent<RuntimeWindow>();
            m_editor = m_window.Editor;
            m_wm = IOC.Resolve<IWindowManager>();
            m_view = GetComponent<View>();
        }

        public virtual bool HandleInput()
        {
            if (m_window.Editor.ActiveWindow != m_window || m_editor.IsInputFieldActive || (m_wm != null && m_wm.IsDialogOpened))
            {
                return false;
            }

            if (SelectAllAction())
            {
                View.SelectAll?.Invoke();
            }

            if (DuplicateAction())
            {
                View.Duplicate?.Invoke();
            }

            if (DeleteAction())
            {
                View.Delete?.Invoke();
            }

            return true;
        }
    }
}

