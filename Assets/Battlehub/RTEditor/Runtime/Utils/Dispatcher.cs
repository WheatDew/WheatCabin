using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

namespace Battlehub.Utils
{
    public class Dispatcher : MonoBehaviour
    {
        private static Dispatcher m_current;
        public static Dispatcher Current
        {
            get { return m_current; }
            private set { m_current = value; }
        }

        private int m_lock;
        private bool m_run;
        private Queue<Action> m_wait;

        private void _BeginInvoke(Action action)
        {
            while (true)
            {
                if (0 == Interlocked.Exchange(ref m_lock, 1))
                {
                    m_wait.Enqueue(action);
                    m_run = true;
                    Interlocked.Exchange(ref m_lock, 0);
                    break;
                }
            }
        }

        public static void BeginInvoke(Action action)
        {
#if UNITY_WEBGL
            action();
#else
            if (m_current != null)
            {
                m_current._BeginInvoke(action);
            }
            else
            {
                action();
            }
#endif
        }

        private void Awake()
        {
            if (Current != null)
            {
                Destroy(Current);
            }

            Current = this;
            m_wait = new Queue<Action>();
        }

        private void Update()
        {
            if (m_run)
            {
                Queue<Action> execute = null;
                if (0 == Interlocked.Exchange(ref m_lock, 1))
                {
                    execute = new Queue<Action>(m_wait.Count);
                    while (m_wait.Count != 0)
                    {
                        Action action = m_wait.Dequeue();
                        execute.Enqueue(action);
                    }
                    m_run = false;
                    Interlocked.Exchange(ref m_lock, 0);
                }

                if (execute != null)
                {
                    while (execute.Count != 0)
                    {
                        Action action = execute.Dequeue();
                        action();
                    }
                }
            }

        }
    }

}
