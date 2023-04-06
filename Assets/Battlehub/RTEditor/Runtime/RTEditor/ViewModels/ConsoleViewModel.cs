using Battlehub.RTCommon;
using Battlehub.UIControls.Binding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityWeld.Binding;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class ConsoleViewModel : HierarchicalDataViewModel<ConsoleLogEntry>
    {
        #region ConsoleLogEntryViewModel
        /// <summary>
        /// This class is never instantiated. 
        /// It is used in the Template to specify the binding properties of ConsoleLogEntry without modifying the ConsoleLogEntry itself.
        /// </summary>
        [Binding]
        internal class ConsoleLogEntryViewModel
        {
            [Binding]
            public LogType LogType
            {
                get;
                set;
            }

            [Binding]
            public string Condition
            {
                get;
                set;
            }

            private ConsoleLogEntryViewModel() { Debug.Assert(false); }
        }
        #endregion
        private bool m_isInfoVisible = true;
        [Binding]
        public bool IsInfoVisible
        {
            get { return m_isInfoVisible; }
            set
            {
                if(m_isInfoVisible != value)
                {
                    m_isInfoVisible = value;
                    RaisePropertyChanged(nameof(IsInfoVisible));
                    OnDataBind();
                }
            }
        }

        private bool m_isWarningsVisible = true;
        [Binding]
        public bool IsWarningsVisible
        {
            get { return m_isWarningsVisible; }
            set
            {
                if(m_isWarningsVisible != value)
                {
                    m_isWarningsVisible = value;
                    RaisePropertyChanged(nameof(IsWarningsVisible));
                    OnDataBind();
                }
            }
        }

        private bool m_isErrorsVisible = true;
        [Binding]
        public bool IsErrorsVisible
        {
            get { return m_isErrorsVisible; }
            set
            {
                if(m_isErrorsVisible != value)
                {
                    m_isErrorsVisible = value;
                    RaisePropertyChanged(nameof(IsErrorsVisible));
                    OnDataBind();
                }
            }
        }
           
        private int m_infoCount;
        [Binding]
        public int InfoCount
        {
            get { return m_infoCount; }
            set 
            {
                if(m_infoCount != value)
                {
                    m_infoCount = value;
                    RaisePropertyChanged(nameof(InfoCount));
                }
            }
        }

        private int m_warningsCount;
        [Binding]
        public int WarningsCount
        {
            get { return m_warningsCount; }
            set
            {
                if(m_warningsCount != value)
                {
                    m_warningsCount = value;
                    RaisePropertyChanged(nameof(WarningsCount));
                }
            }
        }

        private int m_errorsCount;
        [Binding]
        public int ErrorsCount
        {
            get { return m_errorsCount; }
            set
            {
                if(m_errorsCount != value)
                {
                    m_errorsCount = value;
                    RaisePropertyChanged(nameof(ErrorsCount));
                }
            }
        }

        private string m_stackTrace;
        [Binding]
        public string StackTrace
        {
            get { return m_stackTrace; }
            set
            {
                if(m_stackTrace != value)
                {
                    m_stackTrace = value;
                    RaisePropertyChanged(nameof(StackTrace));
                }
            }
        }

        private bool m_canAutoScroll;
        [Binding]
        public bool CanAutoScroll
        {
            get { return m_canAutoScroll; }
            set
            {
                if(m_canAutoScroll != value)
                {
                    m_canAutoScroll = value;
                    RaisePropertyChanged(nameof(CanAutoScroll));
                }
            }
        }

        private IRuntimeConsole m_console;
        
        protected override void Awake()
        {
            base.Awake();
            m_console = IOC.Resolve<IRuntimeConsole>();
            if (m_console != null)
            {
                m_console.Store = true;
            }
        }

        private IEnumerator m_coStart;
        protected override void Start()
        {
            m_coStart = CoStart();
            StartCoroutine(m_coStart);
        }

        protected virtual IEnumerator CoStart()
        {
            //Waiting several frames to prevent writes to console during initial layout 
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            m_console.Store = false;
            BindData();
            UpdateCounters();

            m_console.MessageAdded += OnMessageAdded;
            m_console.MessagesRemoved += OnMessageRemoved;

            m_coStart = null;
        }

        protected override void OnDestroy()
        {
            if (m_console != null)
            {
                m_console.MessageAdded -= OnMessageAdded;
                m_console.MessagesRemoved -= OnMessageRemoved;
            }
            m_console = null;

            if(m_coStart != null)
            {
                StopCoroutine(m_coStart);
                m_coStart = null;
            }
        }

        #region IRuntimeConsole EventHandlers
        private void OnMessageAdded(IRuntimeConsole console, ConsoleLogEntry logEntry)
        {
            CanAutoScroll = true;
            if(CanAdd(logEntry.LogType))
            {
                RaiseHierarchicalDataChanged(HierarchicalDataChangedEventArgs.ItemAdded(null, logEntry));
            }

            UpdateCounters();
            CanAutoScroll = false;
        }

        private void OnMessageRemoved(IRuntimeConsole console, ConsoleLogEntry[] arg)
        {
            CanAutoScroll = true;
            OnDataBind();
            CanAutoScroll = false;
        }

        #endregion

        #region IHierarchicalData<T>

        public override IEnumerable<ConsoleLogEntry> GetChildren(ConsoleLogEntry parent)
        {
            return m_console.Log.Where(entry => CanAdd(entry.LogType)).ToArray();
        }

        public override HierarchicalDataItemFlags GetItemFlags(ConsoleLogEntry item)
        {
            return HierarchicalDataItemFlags.CanSelect;
        }

        #endregion

        #region Bound UnityEvent Handlers

        [Binding]
        public virtual void OnDataBind()
        {
            BindData();
            UpdateCounters();
        }

        [Binding]
        public virtual void OnClear()
        {
            m_console.Clear();
            StackTrace = null;
        }

        #endregion

        #region Methods
        protected override void OnSelectedItemsChanged(IEnumerable<ConsoleLogEntry> unselectedObjects, IEnumerable<ConsoleLogEntry> selectedObjects)
        {
            if(selectedObjects != null && selectedObjects.Any())
            {
                ConsoleLogEntry logEntry = selectedObjects.First();
                StackTrace = $"{logEntry.Condition} {logEntry.StackTrace}";
            }
            else
            {
                StackTrace = null;
            }
        }
        private bool CanAdd(LogType type)
        {
            switch (type)
            {
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    return IsErrorsVisible;
                case LogType.Warning:
                    return IsWarningsVisible;
                case LogType.Log:
                    return IsInfoVisible;
            }
            return false;
        }

        private void UpdateCounters()
        {
            InfoCount = m_console.InfoCount;
            WarningsCount = m_console.WarningsCount;
            ErrorsCount = m_console.ErrorsCount;
        }

        #endregion
    }
}
