using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class ConsoleFilteringExample : EditorExtension
    {
        private IRuntimeConsole m_console;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            m_console = IOC.Resolve<IRuntimeConsole>();
            if(m_console != null)
            {
                m_console.BeforeMessageAdded += OnBeforeMessageAdded;
            }
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            if (m_console != null)
            {
                m_console.BeforeMessageAdded -= OnBeforeMessageAdded;
            }
        }

        private void OnBeforeMessageAdded(IRuntimeConsole console, ConsoleLogCancelArgs arg)
        {
            if(arg.LogEntry.LogType == LogType.Log)
            {
                arg.Cancel = true;
            }
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha0))
            {
                Debug.Log("Log");
            }
            else if(Input.GetKeyDown(KeyCode.Alpha9))
            {
                Debug.LogWarning("Warning");
            }
        }

    }
}

