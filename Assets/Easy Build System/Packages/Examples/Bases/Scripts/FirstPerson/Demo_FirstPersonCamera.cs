/// <summary>
/// Project : Easy Build System
/// Class : Demo_FirstPersonCamera.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.FirstPerson
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Examples.Bases.Scripts.FirstPerson
{
    public class Demo_FirstPersonCamera : MonoBehaviour
    {
        [Header("Look Settings")]
        [SerializeField] bool m_LockCursor = true;
        [SerializeField] float m_XSensitivity = 2f;
        [SerializeField] float m_YSensitivity = 2f;
        [SerializeField] bool m_ClampVerticalRotation = true;
        [SerializeField] float m_MinimumX = -90F;
        [SerializeField] float m_MaximumX = 90F;

        Quaternion m_CharacterTargetRot;
        Quaternion m_CameraTargetRot;

        Camera m_Camera;

        void Awake()
        {
            m_Camera = Camera.main;

            m_CharacterTargetRot = transform.localRotation;
            m_CameraTargetRot = m_Camera.transform.localRotation;

            if (m_LockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void LateUpdate()
        {
            float yRot = Demo_InputHandler.Instance.Look.x * m_XSensitivity;
            float xRot = -Demo_InputHandler.Instance.Look.y * m_YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);

            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (m_ClampVerticalRotation)
            {
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
            }

            transform.localRotation = m_CharacterTargetRot;

            m_Camera.transform.localRotation = m_CameraTargetRot;
        }

        Quaternion ClampRotationAroundXAxis(Quaternion quaternion)
        {
            quaternion.x /= quaternion.w;
            quaternion.y /= quaternion.w;
            quaternion.z /= quaternion.w;
            quaternion.w = 1f;

            float angleX = 2f * Mathf.Rad2Deg * Mathf.Atan(quaternion.x);

            angleX = Mathf.Clamp(angleX, m_MinimumX, m_MaximumX);

            quaternion.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return quaternion;
        }
    }
}