using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.Events;
using Battlehub.RTCommon;
using TMPro;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor.Mobile.Views
{
    public class MobileCreatorView : MonoBehaviour
    {
        public UnityEvent CreateGameObject;

        [SerializeField]
        private TMP_InputField m_inputField = null;

        [SerializeField]
        private VirtualizingTreeView m_treeView = null;

        private IRTE m_rte;
        private RuntimeWindow m_window;

        private bool m_isObjectCreated;
        public bool IsObjectCreated
        {
            get { return m_isObjectCreated; }
            set
            {
                if(m_isObjectCreated != value)
                {
                    m_isObjectCreated = value;

                    if(m_isObjectCreated)
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
            if(m_treeView != null)
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
            else if (input.GetKeyUp(KeyCode.UpArrow))
            {
                if (m_treeView.SelectedItem == null || m_treeView.SelectedIndex == 0)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(m_inputField.gameObject);
                }
            }
            else if (input.GetKeyDown(KeyCode.Return))
            {
                if (m_treeView.SelectedItem != null)
                {
                    CreateGameObject?.Invoke();
                    if (!IsObjectCreated)
                    {
                        ToggleSelectedItem();
                    }
                }
            }
        }

        private void OnTreeViewItemClick(object sender, ItemArgs e)
        {
            CreateGameObject?.Invoke();
            if (!IsObjectCreated)
            {
                ToggleSelectedItem();
            }
        }

        private void DestroyWindow()
        {
            if(m_window == null)
            {
                return;
            }

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            if(wm != null)
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

