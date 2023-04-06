using Battlehub.Utils;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class ConsoleLogEntry
    {
        public LogType LogType
        {
            get;
            set;
        }
        public string Condition
        {
            get;
            set;
        }

        public string StackTrace
        {
            get;
            set;
        }

        public ConsoleLogEntry(LogType logType, string condition, string stackTrace)
        {
            LogType = logType;
            Condition = condition;
            StackTrace = stackTrace;
        }
    }

    public class ConsoleLogCancelArgs
    {
        public bool Cancel
        {
            get;
            set;
        }

        public ConsoleLogEntry LogEntry
        {
            get;
            set;
        }

        public ConsoleLogCancelArgs(ConsoleLogEntry logEntry)
        {
            LogEntry = logEntry;
        }
    }

    public delegate void RuntimeConsoleEventHandler<T>(IRuntimeConsole console, T arg);
    public delegate void RuntimeConsoleEventHandler(IRuntimeConsole console);
    public interface IRuntimeConsole
    {
        event RuntimeConsoleEventHandler<ConsoleLogCancelArgs> BeforeMessageAdded;
        event RuntimeConsoleEventHandler<ConsoleLogEntry> MessageAdded;
        event RuntimeConsoleEventHandler<ConsoleLogEntry[]> MessagesRemoved;
        event RuntimeConsoleEventHandler Cleared;

        bool Store
        {
            get;
            set;
        }

        int MaxItems
        {
            get;
        }

        IEnumerable<ConsoleLogEntry> Log
        {
            get;
        }

        int InfoCount
        {
            get;
        }

        int WarningsCount
        {
            get;
        }

        int ErrorsCount
        {
            get;
        }

        void Clear();
        
    }


    public class RuntimeConsole : MonoBehaviour, IRuntimeConsole
    {
        private static object m_syncRoot = new object();
        private int m_mainThreadId;

        public event RuntimeConsoleEventHandler<ConsoleLogCancelArgs> BeforeMessageAdded;
        public event RuntimeConsoleEventHandler<ConsoleLogEntry> MessageAdded;
        public event RuntimeConsoleEventHandler<ConsoleLogEntry[]> MessagesRemoved;
        public event RuntimeConsoleEventHandler Cleared;

        [SerializeField]
        private bool m_store = false;
        public virtual bool Store
        {
            get { return m_store; }
            set { m_store = value; }
        }

        [SerializeField]
        private int m_maxItems = 300;
        public virtual int MaxItems
        {
            get { return m_maxItems; }
        }

        [SerializeField]
        private int m_clearThreshold = 600;

        private Queue<ConsoleLogEntry> m_log;
        public virtual IEnumerable<ConsoleLogEntry> Log
        {
            get { return m_log; }
        }

        private int m_infoCount;
        public virtual int InfoCount
        {
            get { return m_infoCount; }
        }

        private int m_warningsCount;
        public virtual int WarningsCount
        {
            get { return m_warningsCount; }
        }

        private int m_errorsCount;
        public virtual int ErrorsCount
        {
            get { return m_errorsCount; }
        }

        protected virtual void Awake()
        {
            m_log = new Queue<ConsoleLogEntry>();
            if(m_clearThreshold <= m_maxItems)
            {
                m_clearThreshold = m_maxItems + 50;
            }

            m_mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        protected virtual void OnEnable()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
        }

        protected virtual void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
        }

        protected virtual void UpdateCounters(LogType type, int delta)
        {
            switch(type)
            {
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    m_errorsCount += delta;
                    break;
                case LogType.Warning:
                    m_warningsCount += delta;
                    break;
                case LogType.Log:
                    m_infoCount += delta;
                    break;
            }
        }

        protected virtual bool AcceptMessage(string condition, string stackTrace, LogType type)
        {
            return true;
        }

        private void OnLogMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {
            lock(m_syncRoot)
            {
                if(m_mainThreadId != Thread.CurrentThread.ManagedThreadId)
                {
                    if (Dispatcher.Current != null)
                    {
                        Dispatcher.BeginInvoke(() => OnLogMessageReceived(condition, stackTrace, type));
                    }
                }
            }
        }

        protected virtual void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if(!AcceptMessage(condition, stackTrace, type))
            {
                return;
            }

            ConsoleLogEntry logEntry = null;
            if(MessageAdded != null || m_store)
            {
                logEntry = new ConsoleLogEntry(type, condition, stackTrace);
                if (BeforeMessageAdded != null)
                {
                    ConsoleLogCancelArgs args = new ConsoleLogCancelArgs(logEntry);
                    BeforeMessageAdded(this, args);
                    if(args.Cancel)
                    {
                        return;
                    }
                }

                m_log.Enqueue(logEntry);
                UpdateCounters(type, 1);
                if(m_log.Count > m_clearThreshold)
                {
                    ConsoleLogEntry[] removedItems = new ConsoleLogEntry[m_clearThreshold - m_maxItems];
                    for(int i = 0; i < removedItems.Length; ++i)
                    {
                        ConsoleLogEntry removedLogEntry = m_log.Dequeue();
                        removedItems[i] = removedLogEntry;
                        UpdateCounters(removedLogEntry.LogType, -1);
                    }

                    if (MessagesRemoved != null)
                    {
                        MessagesRemoved(this, removedItems);
                    }
                }
            }

            

            if(MessageAdded != null)
            {
                MessageAdded(this, logEntry);
            }
        }

        public virtual void Clear()
        {
            m_infoCount = 0;
            m_warningsCount = 0;
            m_errorsCount = 0;

            ConsoleLogEntry[] logEntries = m_log.ToArray();

            m_log.Clear();

            if(MessagesRemoved != null)
            {
                MessagesRemoved(this, logEntries);
            }

            if(Cleared != null)
            {
                Cleared(this);
            }
        }
    }
}



