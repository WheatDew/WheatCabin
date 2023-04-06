using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Battlehub.Utils.Lock;

namespace Battlehub.Utils
{
    #if UNITY_WEBGL
    public class Lock
    {
        private long m_lock;
        
        public async Task<LockReleaser> Wait(Action lockedCallback = null, Action releasedCallback = null, CancellationToken ct = default)
        {
            while (Interlocked.CompareExchange(ref m_lock, 1L, 0L) == 1L)
            {
                await Task.Yield();
            }

            lockedCallback?.Invoke();
            return new LockReleaser(this, releasedCallback);
        }

        public struct LockReleaser : IDisposable
        {
            private readonly Lock m_toRelease;
            private Action m_callback;

            public LockReleaser(Lock toRelease, Action callback = null)
            {
                m_toRelease = toRelease;
                m_callback = callback;
            }
            public void Dispose()
            {
                m_callback?.Invoke();
                if (m_toRelease != null)
                {
                    Interlocked.Exchange(ref m_toRelease.m_lock, 0L);
                }
            }
        }
    }
    #else
    public class Lock
    {
        private readonly SemaphoreSlim toLock;

        public Lock()
        {
            toLock = new SemaphoreSlim(1, 1);
        }

        public async Task<LockReleaser> Wait(Action lockedCallback = null, Action releasedCallback = null, CancellationToken ct = default)
        {
            await toLock.WaitAsync(ct);

            lockedCallback?.Invoke();
            return new LockReleaser(toLock, releasedCallback);
        }

        public struct LockReleaser : IDisposable
        {
            private readonly SemaphoreSlim m_toRelease;
            private Action m_callback;

            public LockReleaser(SemaphoreSlim toRelease, Action callback = null)
            {
                m_toRelease = toRelease;
                m_callback = callback;
            }
            public void Dispose()
            {
                m_callback?.Invoke();
                if (m_toRelease != null)
                {
                    m_toRelease.Release();
                }
            }
        }
    }
    #endif

    public class YieldLock : CustomYieldInstruction, IDisposable
    {
        private LockReleaser m_releaser;
        private Task<LockReleaser> m_task;

        public override bool keepWaiting
        {
            get
            {
                if(m_task == null)
                {
                    return false;
                }

                if (!m_task.IsCompleted)
                {
                    return true;
                }

                if (m_task.IsFaulted)
                {
                    throw m_task.Exception;
                }

                m_releaser = m_task.Result;
                return false;
            }
        }

        public YieldLock(Task<LockReleaser> task)
        {
            m_task = task;
        }

        public void Dispose()
        {
            m_releaser.Dispose();
        }
    }

    public class WaitForTask : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                if (m_task == null)
                {
                    return false;
                }

                if (!m_task.IsCompleted)
                {
                    return true;
                }

                if (m_task.IsFaulted)
                {
                    throw m_task.Exception;
                }

                return false;
            }
        }

        private Task m_task;
        public WaitForTask(Task task)
        {
            m_task = task;
        }
    }

}
