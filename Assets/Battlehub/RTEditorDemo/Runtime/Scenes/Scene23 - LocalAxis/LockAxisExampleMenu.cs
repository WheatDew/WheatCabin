using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor.Examples.Scene23
{
    [MenuDefinition]
    [RequireComponent(typeof(LockAxisExample))]
    public class LockAxisExampleMenu : MonoBehaviour
    {
        private LockAxisExample m_example;

        private void Start()
        {
            m_example = GetComponent<LockAxisExample>();
        }

        [MenuCommand("Lock/Reset", priority:0)]
        public void ResetLock()
        {
            m_example.ResetLock();
        }

        [MenuCommand("Lock/Position/X")]
        public void LockPositionX()
        {
            m_example.LockPositionX();
        }

        [MenuCommand("Lock/Position/Y")]
        public void LockPositionY()
        {
            m_example.LockPositionY();
        }

        [MenuCommand("Lock/Position/Z")]
        public void LockPositionZ()
        {
            m_example.LockPositionZ();
        }

        [MenuCommand("Lock/Rotation/X")]
        public void LockRotationX()
        {
            m_example.LockRotationX();
        }

        [MenuCommand("Lock/Rotation/Y")]
        public void LockRotationY()
        {
            m_example.LockRotationY();
        }

        [MenuCommand("Lock/Rotation/Z")]
        public void LockRotationZ()
        {
            m_example.LockRotationZ();
        }

        [MenuCommand("Lock/Rotation/Free")]
        public void LockRotationFree()
        {
            m_example.LockRotationFree();
        }

        [MenuCommand("Lock/Rotation/Screen")]
        public void LockRotationScreen()
        {
            m_example.LockRotationScreen();
        }

        [MenuCommand("Lock/Scale/X")]
        public void LockScaleX()
        {
            m_example.LockScaleX();
        }

        [MenuCommand("Lock/Scale/Y")]
        public void LockScaleY()
        {
            m_example.LockScaleY();
        }

        [MenuCommand("Lock/Scale/Z")]
        public void LockScaleZ()
        {
            m_example.LockScaleZ();
        }

        [MenuCommand("Lock/Rect/XY")]
        public void LockRectXY()
        {
            m_example.LockRectXY();
        }

        [MenuCommand("Lock/Rect/XZ")]
        public void LockRectXZ()
        {
            m_example.LockRectXZ();
        }

        [MenuCommand("Lock/Rect/YZ")]
        public void LockRectYZ()
        {
            m_example.LockRectYZ();
        }

        [MenuCommand("Lock/Pivot Mode")]
        public void LockPivotMode()
        {
            m_example.LockPivotMode();
        }

        [MenuCommand("Lock/Pivot Rotation")]
        public void LockPivotRotation()
        {
            m_example.LockPivotRotation();
        }


        [MenuCommand("Global Lock/Reset", priority: 0)]
        public void ResetGlobalLock()
        {
            m_example.ResetGlobalLock();
        }

        [MenuCommand("Global Lock/Position/X")]
        public void LockGlobalPositionX()
        {
            m_example.LockGlobalPositionX();
        }

        [MenuCommand("Global Lock/Position/Y")]
        public void LockGlobalPositionY()
        {
            m_example.LockGlobalPositionY();
        }

        [MenuCommand("Global Lock/Position/Z")]
        public void LockGlobalPositionZ()
        {
            m_example.LockGlobalPositionZ();
        }

        [MenuCommand("Global Lock/Rotation/X")]
        public void LockGlobalRotationX()
        {
            m_example.LockGlobalRotationX();
        }

        [MenuCommand("Global Lock/Rotation/Y")]
        public void LockGlobalRotationY()
        {
            m_example.LockGlobalRotationY();
        }

        [MenuCommand("Global Lock/Rotation/Z")]
        public void LockGlobalRotationZ()
        {
            m_example.LockGlobalRotationZ();
        }

        [MenuCommand("Global Lock/Rotation/Free")]
        public void LockGlobalRotationFree()
        {
            m_example.LockGlobalRotationFree();
        }

        [MenuCommand("Global Lock/Rotation/Screen")]
        public void LockGlobalRotationScreen()
        {
            m_example.LockGlobalRotationScreen();
        }

        [MenuCommand("Global Lock/Scale/X")]
        public void LockGlobalScaleX()
        {
            m_example.LockGlobalScaleX();
        }

        [MenuCommand("Global Lock/Scale/Y")]
        public void LockGlobalScaleY()
        {
            m_example.LockGlobalScaleY();
        }

        [MenuCommand("Global Lock/Scale/Z")]
        public void LockGlobalScaleZ()
        {
            m_example.LockGlobalScaleZ();
        }

        [MenuCommand("Global Lock/Rect/XY")]
        public void LockGlobalRectXY()
        {
            m_example.LockGlobalRectXY();
        }

        [MenuCommand("Global Lock/Rect/XZ")]
        public void LockGlobalRectXZ()
        {
            m_example.LockGlobalRectXZ();
        }

        [MenuCommand("Global Lock/Rect/YZ")]
        public void LockGlobalRectYZ()
        {
            m_example.LockGlobalRectYZ();
        }

        [MenuCommand("Global Lock/Pivot Mode")]
        public void LockGlobalPivotMode()
        {
            m_example.LockGlobalPivotMode();
        }

        [MenuCommand("Global Lock/Pivot Rotation")]
        public void LockGlobalPivotRotation()
        {
            m_example.LockGlobalPivotRotation();
        }

    }

}
