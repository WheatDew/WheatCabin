using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene23
{
    /// <summary>
    /// This is an example of how to lock the position, rotation and scale axes.
    /// </summary>
    public class LockAxisExample : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_target = null;
        private LockAxes m_lockAxes;
        
        private IRTE m_editor;
        private void Start()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Tools.LockAxes = new LockObject();

            m_lockAxes = m_target.gameObject.GetComponent<LockAxes>();
            if(m_lockAxes == null)
            {
                m_lockAxes = m_target.gameObject.AddComponent<LockAxes>();
            }   
        }

        // Update is called once per frame
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                LockPositionX();
            }
            else if(Input.GetKeyDown(KeyCode.Alpha2))
            {
                LockPositionY();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                LockPositionZ();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                LockRotationX();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                LockRotationY();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                LockRotationZ();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                LockScaleX();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                LockScaleY();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                LockScaleZ();
            }
        }

        public void LockPositionX()
        {
            m_lockAxes.Reset();
            m_lockAxes.PositionX = true;

            SetSelectionAndTool(RuntimeTool.Move);
        }

        public void LockPositionY()
        {
            m_lockAxes.Reset();
            m_lockAxes.PositionY = true;

            SetSelectionAndTool(RuntimeTool.Move);
        }

        public void LockPositionZ()
        {
            m_lockAxes.Reset();
            m_lockAxes.PositionZ = true;

            SetSelectionAndTool(RuntimeTool.Move);
        }

        public void LockRotationX()
        {
            m_lockAxes.Reset();
            m_lockAxes.RotationX = true;

            SetSelectionAndTool(RuntimeTool.Rotate);
        }

        public void LockRotationY()
        {
            m_lockAxes.Reset();
            m_lockAxes.RotationY = true;

            SetSelectionAndTool(RuntimeTool.Rotate);
        }

        public void LockRotationZ()
        {
            m_lockAxes.Reset();
            m_lockAxes.RotationZ = true;

            SetSelectionAndTool(RuntimeTool.Rotate);
        }

        public void LockRotationFree()
        {
            m_lockAxes.Reset();
            m_lockAxes.RotationFree = true;

            SetSelectionAndTool(RuntimeTool.Rotate);
        }

        public void LockRotationScreen()
        {
            m_lockAxes.Reset();
            m_lockAxes.RotationScreen = true;

            SetSelectionAndTool(RuntimeTool.Rotate);
        }

        public void LockScaleX()
        {
            m_lockAxes.Reset();
            m_lockAxes.ScaleX = true;

            SetSelectionAndTool(RuntimeTool.Scale);
        }

        public void LockScaleY()
        {
            m_lockAxes.Reset();
            m_lockAxes.ScaleY = true;

            SetSelectionAndTool(RuntimeTool.Scale);
        }

        public void LockScaleZ()
        {
            m_lockAxes.Reset();
            m_lockAxes.ScaleZ = true;

            SetSelectionAndTool(RuntimeTool.Scale);
        }

        public void LockRectXY()
        {
            m_lockAxes.Reset();
            m_lockAxes.RectXY = true;

            SetSelectionAndTool(RuntimeTool.Rect);
        }

        public void LockRectXZ()
        {
            m_lockAxes.Reset();
            m_lockAxes.RectXZ = true;

            SetSelectionAndTool(RuntimeTool.Rect);
        }

        public void LockRectYZ()
        {
            m_lockAxes.Reset();
            m_lockAxes.RectYZ = true;

            SetSelectionAndTool(RuntimeTool.Rect);
        }

        public void LockPivotMode()
        {
            m_lockAxes.Reset();
            m_lockAxes.PivotMode = true;
            m_lockAxes.PivotModeValue = RuntimePivotMode.Pivot;

            SetSelectionAndTool(RuntimeTool.Move);
        }

        public void LockPivotRotation()
        {
            m_lockAxes.Reset();
            m_lockAxes.PivotRotation = true;
            m_lockAxes.PivotRotationValue = RuntimePivotRotation.Global;

            SetSelectionAndTool(RuntimeTool.Move);
        }

        public void ResetLock()
        {
            m_lockAxes.Reset();
            SetSelectionAndTool(m_editor.Tools.Current);
        }


        public void LockGlobalPositionX()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.PositionX = true;

            SetSelectionAndTool(RuntimeTool.Move);
        }

        public void LockGlobalPositionY()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.PositionY = true;

            SetSelectionAndTool(RuntimeTool.Move);
        }

        public void LockGlobalPositionZ()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.PositionZ = true;

            SetSelectionAndTool(RuntimeTool.Move);
        }

        public void LockGlobalRotationX()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.RotationX = true;

            SetSelectionAndTool(RuntimeTool.Rotate);
        }

        public void LockGlobalRotationY()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.RotationY = true;

            SetSelectionAndTool(RuntimeTool.Rotate);
        }

        public void LockGlobalRotationZ()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.RotationZ = true;

            SetSelectionAndTool(RuntimeTool.Rotate);
        }

        public void LockGlobalRotationFree()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.RotationFree = true;

            SetSelectionAndTool(RuntimeTool.Rotate);
        }

        public void LockGlobalRotationScreen()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.RotationScreen = true;

            SetSelectionAndTool(RuntimeTool.Rotate);
        }

        public void LockGlobalScaleX()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.ScaleX = true;

            SetSelectionAndTool(RuntimeTool.Scale);
        }

        public void LockGlobalScaleY()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.ScaleY = true;

            SetSelectionAndTool(RuntimeTool.Scale);
        }

        public void LockGlobalScaleZ()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.ScaleZ = true;

            SetSelectionAndTool(RuntimeTool.Scale);
        }

        public void LockGlobalRectXY()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.RectXY = true;

            SetSelectionAndTool(RuntimeTool.Rect);
        }

        public void LockGlobalRectXZ()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.RectXZ = true;

            SetSelectionAndTool(RuntimeTool.Rect);
        }

        public void LockGlobalRectYZ()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.RectYZ = true;

            SetSelectionAndTool(RuntimeTool.Rect);
        }

        public void LockGlobalPivotMode()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.PivotMode = RuntimePivotMode.Pivot;
   
            SetSelectionAndTool(RuntimeTool.Move);
        }

        public void LockGlobalPivotRotation()
        {
            m_editor.Tools.LockAxes.Reset();
            m_editor.Tools.LockAxes.PivotRotation = RuntimePivotRotation.Global;

            SetSelectionAndTool(RuntimeTool.Move);
        }

        public void ResetGlobalLock()
        {
            m_editor.Tools.LockAxes = null;
            SetSelectionAndTool(m_editor.Tools.Current);
        }


        private void SetSelectionAndTool(RuntimeTool tool)
        {
            m_editor.Selection.EnableUndo = false;
            m_editor.Selection.activeGameObject = null;
            m_editor.Selection.EnableUndo = true;
            m_editor.Selection.activeObject = m_target;
            m_editor.Tools.Current = tool;
        }
    }
}

