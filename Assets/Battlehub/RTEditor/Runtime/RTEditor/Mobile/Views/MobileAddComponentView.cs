using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.Events;
using Battlehub.RTCommon;
using TMPro;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor.Mobile.Views
{
    public class MobileAddComponentView : MonoBehaviour
    {
        public UnityEvent AddComponent;

        [SerializeField]
        private TMP_InputField m_inputField = null;

        [SerializeField]
        private VirtualizingTreeView m_treeView = null;

        private IRTE m_rte;
        private RuntimeWindow m_window;

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
                    AddComponent?.Invoke();
                    DestroyWindow();
                }
            }
        }

        private void OnTreeViewItemClick(object sender, ItemArgs e)
        {
            AddComponent?.Invoke();
            DestroyWindow();
        }

        private void DestroyWindow()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.DestroyWindow(m_window.transform);
        }

    }
}

