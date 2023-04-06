using UnityEngine;

namespace Battlehub.RTCommon
{
    /*
    public class VRPointer : Pointer
    {
        private IVRTracker m_vrTracker;

        public override Ray Ray
        {
            get
            {
                if(m_vrTracker.RightHand.IsTracking)
                {
                    Vector3 pos = m_vrTracker.Origin + m_vrTracker.RightHand.Postion;
                    Ray ray = new Ray(pos, m_vrTracker.RightHand.Rotation * Quaternion.AngleAxis(45, Vector3.right) * Vector3.forward);
                    Debug.DrawRay(ray.origin, ray.direction, Color.green, 5);
                    return ray;
                }

                Transform transform = m_window.Camera.transform;
                return new Ray(transform.position, transform.forward);
            }
        }

        public override Vector2 ScreenPoint
        {
            get { throw new System.NotSupportedException(); }
        }

        public override bool XY(Vector3 worldPos, out Vector2 result)
        {
            return GetPointOnPlane(worldPos, Ray, out result);
        }

        protected override void Init()
        {
            base.Init();

            m_vrTracker = IOC.Resolve<IVRTracker>();
        }

        public override bool WorldToScreenPoint(Vector3 worldPos, Vector3 point, out Vector2 result)
        {
            Vector3 camPos = m_window.Camera.transform.position;
            Ray ray = new Ray(camPos, (point - camPos).normalized);
            return GetPointOnPlane(worldPos, ray, out result); 
        }

        private bool GetPointOnPlane(Vector3 worldPos, Ray ray, out Vector2 result)
        {
            Vector3 camPos = m_window.Camera.transform.position;
            Vector3 toCam = (camPos - worldPos).normalized;
            float halfFov = Mathf.Cos(m_window.Camera.fieldOfView / 2 * Mathf.Rad2Deg);

            if (Vector3.Dot(-toCam, m_window.Camera.transform.forward) < halfFov)
            {
                result = Vector3.zero;
                return false;
            }

            ray.origin = camPos;

            Vector3 planePos = camPos - toCam * m_window.Camera.nearClipPlane;
            Matrix4x4 planeMatrix = Matrix4x4.TRS(planePos, Quaternion.LookRotation(-toCam), Vector3.one);

            Plane plane = new Plane(toCam, planePos);

            float distance;
            if (!plane.Raycast(ray, out distance))
            {
                result = Vector3.zero;
                return false;
            }

            Vector3 pointerPos = ray.GetPoint(distance);
            result = planeMatrix.inverse.MultiplyPoint(pointerPos) * 5000;
            return true;
        }

        public override bool ToWorldMatrix(Vector3 worldPos, out Matrix4x4 matrix)
        {
            Vector3 camPos = m_window.Camera.transform.position;
            Vector3 toCam = (camPos - worldPos).normalized;
            float halfFov = Mathf.Cos(m_window.Camera.fieldOfView / 2 * Mathf.Rad2Deg);
            if (Vector3.Dot(-toCam, m_window.Camera.transform.forward) < halfFov)
            {
                matrix = Matrix4x4.identity;
                return false;
            }

            Vector3 planePos = camPos - toCam * m_window.Camera.nearClipPlane;
            matrix = Matrix4x4.TRS(planePos, Quaternion.LookRotation(-toCam), Vector3.one);
            return true;
        }
    }
    */
}
