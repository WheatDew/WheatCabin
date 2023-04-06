using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene22
{
    public class ToolsExample : MonoBehaviour
    {
        private IRTE m_editor;
        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();

            m_editor.Tools.ToolChanged += OnToolChanged;
            m_editor.Tools.ActiveToolChanged += OnActiveToolChanged;
            m_editor.Tools.PivotModeChanged += OnPivotModeChanged;
            m_editor.Tools.PivotRotationChanged += OnPivotRotationChanged;
            m_editor.Tools.IsBoxSelectionEnabledChanged += OnIsBoxSelectionEnabledChanged;
            
        }

        private void OnDestroy()
        {
            if(m_editor.Tools != null)
            {
                m_editor.Tools.ToolChanged -= OnToolChanged;
                m_editor.Tools.ActiveToolChanged -= OnActiveToolChanged;
                m_editor.Tools.PivotModeChanged -= OnPivotModeChanged;
                m_editor.Tools.PivotRotationChanged -= OnPivotRotationChanged;
                m_editor.Tools.IsBoxSelectionEnabledChanged -= OnIsBoxSelectionEnabledChanged;
            }
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                m_editor.Tools.Current = RuntimeTool.Move;
            }
            else if(Input.GetKeyDown(KeyCode.Alpha2))
            {
                m_editor.Tools.Current = RuntimeTool.Rotate;
            }
            else if(Input.GetKeyDown(KeyCode.Alpha3))
            {
                m_editor.Tools.Current = RuntimeTool.Scale;
            }
            else if(Input.GetKeyDown(KeyCode.Alpha4))
            {
                m_editor.Tools.Current = RuntimeTool.Rect;
            }
            else if(Input.GetKeyDown(KeyCode.Alpha5))
            {
                m_editor.Tools.PivotMode = (m_editor.Tools.PivotMode == RuntimePivotMode.Center) ? RuntimePivotMode.Pivot : RuntimePivotMode.Center;
            }
            else if(Input.GetKeyDown(KeyCode.Alpha6))
            {
                m_editor.Tools.PivotRotation = (m_editor.Tools.PivotRotation == RuntimePivotRotation.Global) ? RuntimePivotRotation.Local : RuntimePivotRotation.Global;
            }
            else if(Input.GetKeyDown(KeyCode.Alpha7))
            {
                m_editor.Tools.IsBoxSelectionEnabled = !m_editor.Tools.IsBoxSelectionEnabled;
            }
        }

        private void OnToolChanged()
        {
            Debug.Log($"Current Tool: {m_editor.Tools.Current}");
        }

        private void OnActiveToolChanged()
        {
            if (m_editor.Tools.ActiveTool != null)
            {
                Debug.Log($"Active Tool: {m_editor.Tools.ActiveTool}");
            }
            else
            {
                Debug.Log("Tool Deactivated");
            }
        }

        private void OnPivotModeChanged()
        {
            Debug.Log($"Pivot Mode: {m_editor.Tools.PivotMode}");
        }

        private void OnPivotRotationChanged()
        {
            Debug.Log($"Pivot Rotation: {m_editor.Tools.PivotRotation}");
        }

        private void OnIsBoxSelectionEnabledChanged()
        {
            Debug.Log($"Is Box Selection Enabled: {m_editor.Tools.IsBoxSelectionEnabled}");
        }
    }
}
