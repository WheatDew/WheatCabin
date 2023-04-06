using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.Events;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor.Mobile.Views
{
    public class MobileMenuView : MonoBehaviour
    {
        public UnityEvent ItemClick;

        [SerializeField]
        private VirtualizingTreeView m_treeView = null;

        private IRTE m_rte;
        private RuntimeWindow m_window;

        private bool m_isActionExecuted;
        public bool IsActionExecuted
        {
            get { return m_isActionExecuted; }
            set
            {
                if (m_isActionExecuted != value)
                {
                    m_isActionExecuted = value;

                    if (m_isActionExecuted)
                    {
                        DestroyWindow();
                    }
                }
            }
        }

        private void Awake()
        {
            m_rte = IOC.Resolve<IRTE>();
        }

        private void Start()
        {
            m_window = GetComponent<RuntimeWindow>();
            if (m_treeView != null)
            {
                m_treeView.ItemClick += OnTreeViewItemClick;
            }
        }


        private void OnDestroy()
        {
            if (m_treeView != null)
            {
                m_treeView.ItemClick -= OnTreeViewItemClick;
            }
        }

        private void LateUpdate()
        {
            IInput input = m_rte.Input;
            if (input.GetKeyDown(KeyCode.DownArrow))
            {
                m_treeView.Select();
                m_treeView.IsFocused = true;
            }
            else if (input.GetKeyDown(KeyCode.Return))
            {
                if (m_treeView.SelectedItem != null)
                {
                    ItemClick?.Invoke();
                    if (!IsActionExecuted)
                    {
                        ToggleSelectedItem();
                    }
                }
            }
        }

        private void OnTreeViewItemClick(object sender, ItemArgs e)
        {
            ItemClick?.Invoke();
            if (!IsActionExecuted)
            {
                ToggleSelectedItem();
            }
        }

        private void DestroyWindow()
        {
            if (m_window == null)
            {
                return;
            }

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            if (wm != null)
            {
                wm.DestroyWindow(m_window.transform);
                m_window = null;
            }
        }

        private void ToggleSelectedItem()
        {
            if (m_treeView.IsExpanded(m_treeView.SelectedItem))
            {
                m_treeView.Collapse(m_treeView.SelectedItem);
            }
            else
            {
                m_treeView.Expand(m_treeView.SelectedItem);
            }
        }
    }
}

