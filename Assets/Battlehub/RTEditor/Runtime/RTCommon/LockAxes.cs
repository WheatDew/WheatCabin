using UnityEngine;
using System.Linq;

namespace Battlehub.RTCommon
{
    public class LockObject
    {
        private bool m_positionX;
        private bool m_positionY;
        private bool m_positionZ;
        private bool m_rotationX;
        private bool m_rotationY;
        private bool m_rotationZ;
        private bool m_rotationFree;
        private bool m_rotationScreen;
        private bool m_scaleX;
        private bool m_scaleY;
        private bool m_scaleZ;
        private bool m_rectXY;
        private bool m_rectYZ;
        private bool m_rectXZ;

        public bool PositionX { get { return m_positionX || (m_globalLock != null ? m_globalLock.m_positionX : false); } set { m_positionX = value; } }
        public bool PositionY { get { return m_positionY || (m_globalLock != null ? m_globalLock.m_positionY : false); } set { m_positionY = value; } }
        public bool PositionZ { get { return m_positionZ || (m_globalLock != null ? m_globalLock.m_positionZ : false); } set { m_positionZ = value; } }
        public bool RotationX { get { return m_rotationX || (m_globalLock != null ? m_globalLock.m_rotationX : false); } set { m_rotationX = value; } }
        public bool RotationY { get { return m_rotationY || (m_globalLock != null ? m_globalLock.m_rotationY : false); } set { m_rotationY = value; } }
        public bool RotationZ { get { return m_rotationZ || (m_globalLock != null ? m_globalLock.m_rotationZ : false); } set { m_rotationZ = value; } }
        public bool RotationFree { get { return m_rotationFree || (m_globalLock != null ? m_globalLock.m_rotationFree : false); } set { m_rotationFree = value; } }
        public bool RotationScreen { get { return m_rotationScreen || (m_globalLock != null ? m_globalLock.m_rotationScreen : false); } set { m_rotationScreen = value; } }
        public bool ScaleX { get { return m_scaleX || (m_globalLock != null ? m_globalLock.m_scaleX : false); } set { m_scaleX = value; } }
        public bool ScaleY { get { return m_scaleY || (m_globalLock != null ? m_globalLock.m_scaleY : false); } set { m_scaleY = value; } }
        public bool ScaleZ { get { return m_scaleZ || (m_globalLock != null ? m_globalLock.m_scaleZ : false); } set { m_scaleZ = value; } }
        public bool RectXY { get { return m_rectXY || (m_globalLock != null ? m_globalLock.m_rectXY : false); } set { m_rectXY = value; } }
        public bool RectYZ { get { return m_rectYZ || (m_globalLock != null ? m_globalLock.m_rectYZ : false); } set { m_rectYZ = value; } }
        public bool RectXZ { get { return m_rectXZ || (m_globalLock != null ? m_globalLock.m_rectXZ : false); } set { m_rectXZ = value; } }

        public RuntimePivotMode? PivotMode { get; set; }
        public RuntimePivotRotation? PivotRotation { get; set; }

        public bool IsPositionLocked
        {
            get { return PositionX && PositionY && PositionZ; }
        }

        public bool IsRotationLocked
        {
            get { return RotationX && RotationY && RotationZ && RotationFree && RotationScreen; }
        }

        public bool IsScaleLocked
        {
            get { return ScaleX && ScaleY && ScaleZ; }
        }

        public bool IsRectLocked
        {
            get { return RectXY && RectYZ && RectXZ; }
        }

        private LockObject m_globalLock;
        public void SetGlobalLock(LockObject gLock)
        {
            m_globalLock = gLock;
        }

        public LockObject()
        {
        }

        public LockObject(LockObject obj)
        {
            m_positionX = obj.m_positionX;
            m_positionY = obj.m_positionY;
            m_positionZ = obj.m_positionZ;
            m_rotationX = obj.m_rotationX;
            m_rotationY = obj.m_rotationY;
            m_rotationZ = obj.m_rotationZ;
            m_rotationFree = obj.m_rotationFree;
            m_rotationScreen = obj.m_rotationScreen;
            m_scaleX = obj.m_scaleX;
            m_scaleY = obj.m_scaleY;
            m_scaleZ = obj.m_scaleZ;
            m_rectXY = obj.m_rectXY;
            m_rectYZ = obj.m_rectYZ;
            m_rectXZ = obj.m_rectXZ;
            m_globalLock = obj.m_globalLock;
        }

        public void Reset()
        {
            m_positionX = false;
            m_positionY = false;
            m_positionZ = false;
            m_rotationX = false;
            m_rotationY = false;
            m_rotationZ = false;
            m_rotationFree = false;
            m_rotationScreen = false;
            m_scaleX = false;
            m_scaleY = false;
            m_scaleZ = false;
            m_rectXY = false;
            m_rectYZ = false;
            m_rectXZ = false;
            m_globalLock = null;
        }
    }

    public class LockAxes : MonoBehaviour
    {
        public bool PositionX;
        public bool PositionY;
        public bool PositionZ;
        public bool RotationX;
        public bool RotationY;
        public bool RotationZ;
        public bool RotationFree;
        public bool RotationScreen;
        public bool ScaleX;
        public bool ScaleY;
        public bool ScaleZ;
        public bool RectXY;
        public bool RectYZ;
        public bool RectXZ;

        public bool PivotMode;
        public RuntimePivotMode PivotModeValue;
        public bool PivotRotation;
        public RuntimePivotRotation PivotRotationValue;

        public void Reset()
        {
            PositionX = PositionY = PositionZ = false;
            RotationX = RotationY = RotationZ = RotationFree = RotationScreen =  false;
            ScaleX = ScaleY = ScaleZ = false;
            RectXY = RectXZ = RectYZ = false;
            PivotMode = false;
            PivotModeValue = RuntimePivotMode.Center;
            PivotRotation = false;
            PivotRotationValue = RuntimePivotRotation.Local;
        }

        public static LockObject Eval(LockAxes[] lockAxes)
        {
            LockObject lockObject = new LockObject();
            if(lockAxes != null)
            {
                lockObject.PositionX = lockAxes.Any(la => la.PositionX);
                lockObject.PositionY = lockAxes.Any(la => la.PositionY);
                lockObject.PositionZ = lockAxes.Any(la => la.PositionZ);

                lockObject.RotationX = lockAxes.Any(la => la.RotationX);
                lockObject.RotationY = lockAxes.Any(la => la.RotationY);
                lockObject.RotationZ = lockAxes.Any(la => la.RotationZ);
                lockObject.RotationFree = lockAxes.Any(la => la.RotationFree);
                lockObject.RotationScreen = lockAxes.Any(la => la.RotationScreen);

                lockObject.ScaleX = lockAxes.Any(la => la.ScaleX);
                lockObject.ScaleY = lockAxes.Any(la => la.ScaleY);
                lockObject.ScaleZ = lockAxes.Any(la => la.ScaleZ);

                lockObject.RectXY = lockAxes.Any(la => la.RectXY);
                lockObject.RectYZ = lockAxes.Any(la => la.RectYZ);
                lockObject.RectXZ = lockAxes.Any(la => la.RectXZ);

                lockObject.PivotMode = null;
                if(lockAxes.Any(la => la.PivotMode))
                {
                    if(lockAxes.All(la => la.PivotModeValue == RuntimePivotMode.Center))
                    {
                        lockObject.PivotMode = RuntimePivotMode.Center;
                    }
                    else if(lockAxes.All(la => la.PivotModeValue == RuntimePivotMode.Pivot))
                    {
                        lockObject.PivotMode = RuntimePivotMode.Pivot;
                    }
                }

                lockObject.PivotRotation = null;
                if(lockAxes.Any(la => la.PivotRotation))
                {
                    if (lockAxes.All(la => la.PivotRotationValue == RuntimePivotRotation.Global))
                    {
                        lockObject.PivotRotation = RuntimePivotRotation.Global;
                    }
                    else if (lockAxes.All(la => la.PivotRotationValue == RuntimePivotRotation.Local))
                    {
                        lockObject.PivotRotation = RuntimePivotRotation.Local;
                    }
                }
            }

            return lockObject;
        }
    }

}
