using Battlehub.RTCommon;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Battlehub.RTSL.Demo
{
    public class LockProjectExample : MonoBehaviour
    {
        private IProject m_project;
        private IProjectAsync m_projectAsync;

        private void Start()
        {
            m_project = IOC.Resolve<IProject>();
            m_projectAsync = IOC.Resolve<IProjectAsync>();

            StartCoroutine(Coroutine());
            AsyncProc();
        }

        private async void AsyncProc()
        {
            using (await m_projectAsync.LockAsync())
            {
                Debug.Log("AsyncProc Lock acquired");

                await Task.Delay(3000);

                Debug.Log("AsyncProc Delay 3 Second");

                await Task.Delay(3000);

                Debug.Log("AsyncProc Delay 6 Seconds");
            }
        }

        private IEnumerator Coroutine()
        {
            using (YieldLock yeildLock = m_project.Lock())
            {
                yield return yeildLock;

                Debug.Log("Coroutine Lock acquired");

                yield return new WaitForSeconds(3);

                Debug.Log("CoRoutine Wait For 3 Seconds ");

                yield return new WaitForSeconds(3);

                Debug.Log("CoRoutine Wait For 3 Seconds");
            }
        }
    }

}

