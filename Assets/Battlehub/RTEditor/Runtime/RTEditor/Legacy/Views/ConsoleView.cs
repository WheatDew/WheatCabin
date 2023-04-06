using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.DockPanels;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    [AddComponentMenu(""), /*System.Obsolete*/]
    public class ConsoleView : RuntimeWindow
    {
        [SerializeField]
        private Button m_btnClear = null;

        [SerializeField]
        private Toggle m_togInfo = null;

        [SerializeField]
        private Toggle m_togWarning = null;

        [SerializeField]
        private Toggle m_togError = null;

        private TextMeshProUGUI m_txtInfoCount;
        private TextMeshProUGUI m_txtWarningCount;
        private TextMeshProUGUI m_txtErrorCount;

        [SerializeField]
        private Sprite m_infoIcon = null;

        [SerializeField]
        private Sprite m_warningIcon = null;

        [SerializeField]
        private Sprite m_errorIcon = null;

        [SerializeField]
        private Color m_infoColor = Color.white;

        [SerializeField]
        private Color m_warningColor = Color.yellow;

        [SerializeField]
        private Color m_errorColor = Color.red;

        [SerializeField]
        private VirtualizingTreeView m_treeView = null;

        [SerializeField]
        private TMP_InputField m_stackTrace = null;

        private IRuntimeConsole m_console;

        private VirtualizingScrollRect m_scrollRect;

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Console;

            base.AwakeOverride();

            DockPanel dockPanelsRoot = GetComponent<DockPanel>();
            if(dockPanelsRoot != null)
            {
                dockPanelsRoot.CursorHelper = Editor.CursorHelper;
            }

            m_treeView.CanDrag = false;
            m_treeView.CanReorder = false;
            m_treeView.CanRemove = false;
            m_treeView.CanEdit = false;
            
            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.SelectionChanged += OnSelectionChanged;

            m_scrollRect = m_treeView.GetComponentInChildren<VirtualizingScrollRect>();

            if (m_btnClear != null)
            {
                m_btnClear.onClick.AddListener(OnClearClick);
            }

            if(m_togInfo != null)
            {
                m_txtInfoCount = m_togInfo.GetComponentInChildren<TextMeshProUGUI>();                
                m_togInfo.onValueChanged.AddListener(OnTogInfoValueChanged);
            }

            if(m_togWarning != null)
            {
                m_txtWarningCount = m_togWarning.GetComponentInChildren<TextMeshProUGUI>();
                m_togWarning.onValueChanged.AddListener(OnTogWarningValueChange);
            }

            if(m_togError != null)
            {
                m_txtErrorCount = m_togError.GetComponentInChildren<TextMeshProUGUI>();
                m_togError.onValueChanged.AddListener(OnTogErrorValueChanged);
            }            
        }

        protected virtual IEnumerator Start()
        {
            //Waiting several frames to prevent writes to console during initial layout 
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if(m_stackTrace != null)
            {
                m_stackTrace.scrollSensitivity = 0;
            }

            m_console = IOC.Resolve<IRuntimeConsole>();
            m_console.MessageAdded += OnMessageAdded;
            m_console.MessagesRemoved += OnMessageRemoved;
            DataBind();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if(m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.SelectionChanged -= OnSelectionChanged;
            }

            if (m_btnClear != null)
            {
                m_btnClear.onClick.RemoveListener(OnClearClick);
            }

            if (m_togInfo != null)
            {
                m_togInfo.onValueChanged.RemoveListener(OnTogInfoValueChanged);
            }

            if (m_togWarning != null)
            {
                m_togWarning.onValueChanged.RemoveListener(OnTogWarningValueChange);
            }

            if (m_togError != null)
            {
                m_togError.onValueChanged.RemoveListener(OnTogErrorValueChanged);
            }

            if (m_console != null)
            {
                m_console.MessageAdded -= OnMessageAdded;
                m_console.MessagesRemoved -= OnMessageRemoved;
            }
        }

        private bool CanAdd(LogType type)
        {
            switch (type)
            {
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    return m_togError.isOn;
                case LogType.Warning:
                    return m_togWarning.isOn;
                case LogType.Log:
                    return m_togInfo.isOn;
            }
            return false;
        }

        private void OnMessageAdded(IRuntimeConsole console, ConsoleLogEntry logEntry)
        {
            bool scroll = false;
            if(m_scrollRect.Index + m_scrollRect.VisibleItemsCount == m_scrollRect.ItemsCount)
            {
                scroll = true;
            }

            if (CanAdd(logEntry.LogType))
            {
                m_treeView.Add(logEntry);
            }
            UpdateCounters();

            if(scroll)
            {
                m_scrollRect.verticalNormalizedPosition = 0;
            }
        }

        private void OnMessageRemoved(IRuntimeConsole console, ConsoleLogEntry[] arg)
        {
            bool scroll = false;
            if (m_scrollRect.Index + m_scrollRect.VisibleItemsCount == m_scrollRect.ItemsCount)
            {
                scroll = true;
            }
            DataBind();
            if (scroll)
            {
                m_scrollRect.verticalNormalizedPosition = 0;
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ConsoleLogEntry logEntry = (ConsoleLogEntry)e.Item;

            TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
            text.text = logEntry.Condition;

            Image icon = e.ItemPresenter.GetComponentsInChildren<Image>(true)[2];
            icon.gameObject.SetActive(true);
            switch (logEntry.LogType)
            {
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    icon.sprite = m_errorIcon;
                    icon.color = m_errorColor;
                    break;
                case LogType.Warning:
                    icon.sprite = m_warningIcon;
                    icon.color = m_warningColor;
                    break;
                case LogType.Log:
                    icon.sprite = m_infoIcon;
                    icon.color = m_infoColor;
                    break;
            }
            e.HasChildren = false;
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            if(e.NewItem != null)
            {
                ConsoleLogEntry logEntry = (ConsoleLogEntry)e.NewItem;
                m_stackTrace.text = logEntry.Condition + " " + logEntry.StackTrace;
            }
            else
            {
                m_stackTrace.text = null;
            }
        }

        private void OnClearClick()
        {
            m_console.Clear();
            m_stackTrace.text = null;
        }

        private void OnTogInfoValueChanged(bool value)
        {
            DataBind();
        }

        private void OnTogWarningValueChange(bool value)
        {
            DataBind();
        }

        private void OnTogErrorValueChanged(bool value)
        {
            DataBind();
        }

        private void DataBind()
        {
            m_treeView.Items = m_console.Log.Where(entry => CanAdd(entry.LogType)).ToArray();
            UpdateCounters();
        }

        private void UpdateCounters()
        {
            if (m_txtInfoCount != null)
            {
                if(m_console.InfoCount > 99)
                {
                    m_txtInfoCount.text = "99+";
                }
                else
                {
                    m_txtInfoCount.text = m_console.InfoCount.ToString();
                }
            }

            if(m_txtWarningCount != null)
            {
                if(m_console.WarningsCount > 99)
                {
                    m_txtWarningCount.text = "99+";
                }
                else
                {
                    m_txtWarningCount.text = m_console.WarningsCount.ToString();
                }
            }

            if(m_txtErrorCount != null)
            {
                if(m_console.ErrorsCount > 99)
                {
                    m_txtErrorCount.text = "99+";
                }
                else
                {
                    m_txtErrorCount.text = m_console.ErrorsCount.ToString();
                }
            }
        }
    }
}
