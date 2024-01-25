/// <summary>
/// Project : Easy Build System
/// Class : Demo_FreeLookCam.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.ThirdPerson
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Examples.Bases.Scripts.ThirdPerson
{
    public class Demo_FreeLookCam : Demo_PivotBasedCameraRig
    {
        [Header("Free Look Settings")]
        [SerializeField] float m_MoveSpeed = 1f;

        [Range(0f, 10f)] public float m_TurnSpeed = 1.5f;
        [SerializeField] float m_TurnSmoothing = 0.0f;
        [SerializeField] float m_TiltMax = 75f;
        [SerializeField] float m_TiltMin = 45f;

        [SerializeField] bool m_VerticalAutoReturn = false;

        float m_LookAngle;
        float m_TiltAngle;
        Vector3 m_PivotEulers;
        Quaternion m_PivotTargetRot;
        Quaternion m_TransformTargetRot;

        void Awake()
        {
            m_PivotEulers = Pivot.rotation.eulerAngles;

            m_PivotTargetRot = Pivot.transform.localRotation;
            m_TransformTargetRot = transform.localRotation;

            transform.parent = null;
        }

        public override void Start()
        {
            base.Start();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        protected void Update()
        {
            HandleRotationMovement();
        }

        protected override void FollowTarget(float deltaTime)
        {
            if (Target == null)
            {
                return;
            }

            transform.position = Vector3.Lerp(transform.position, Target.position, deltaTime * m_MoveSpeed);
        }

        void HandleRotationMovement()
        {
            if (Time.timeScale < float.Epsilon)
            {
                return;
            }

            float x = Demo_InputHandler.Instance.Look.x;
            float y = -Demo_InputHandler.Instance.Look.y;

            m_LookAngle += x * m_TurnSpeed;

            m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

            if (m_VerticalAutoReturn)
            {
                m_TiltAngle = y > 0 ? Mathf.Lerp(0, -m_TiltMin, y) : Mathf.Lerp(0, m_TiltMax, -y);
            }
            else
            {
                m_TiltAngle -= y * m_TurnSpeed;
                m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);
            }

            m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_PivotEulers.y, m_PivotEulers.z);

            if (m_TurnSmoothing > 0)
            {
                Pivot.localRotation = Quaternion.Slerp(Pivot.localRotation, m_PivotTargetRot, m_TurnSmoothing * Time.deltaTime);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, m_TurnSmoothing * Time.deltaTime);
            }
            else
            {
                Pivot.localRotation = m_PivotTargetRot;
                transform.localRotation = m_TransformTargetRot;
            }
        }
    }
}