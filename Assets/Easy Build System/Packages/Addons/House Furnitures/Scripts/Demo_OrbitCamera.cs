/// <summary>
/// Project : Easy Build System
/// Class : Demo_OrbitCamera.cs
/// Namespace : EasyBuildSystem.Packages.Addons.BuildingMenu
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
#if EBS_INPUT_SYSTEM_SUPPORT
using UnityEngine.InputSystem;
#endif

namespace EasyBuildSystem.Packages.Addons.BuildingMenu
{
    public class Demo_OrbitCamera : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] float m_MovementSpeed = 8f;

        [Header("Rotation Settings")]
        [SerializeField] float m_RotateSpeed = 15;

        Vector3 m_PanMovement;
        Vector3 m_LastMousePosition;

        void Update()
        {
            m_PanMovement = Vector3.zero;

#if EBS_INPUT_SYSTEM_SUPPORT

            if (Keyboard.current.wKey.isPressed)
            {
                m_PanMovement += Vector3.forward * m_MovementSpeed * Time.deltaTime;
            }

            if (Keyboard.current.sKey.isPressed)
            {
                m_PanMovement -= Vector3.forward * m_MovementSpeed * Time.deltaTime;
            }

            if (Keyboard.current.aKey.isPressed)
            {
                m_PanMovement += Vector3.left * m_MovementSpeed * Time.deltaTime;
            }

            if (Keyboard.current.dKey.isPressed)
            {
                m_PanMovement += Vector3.right * m_MovementSpeed * Time.deltaTime;
            }

            transform.Translate(transform.TransformDirection(m_PanMovement), Space.World);

            Vector2 inputAxis = Mouse.current.position.ReadValue();

            if (Mouse.current.rightButton.isPressed)
            {
                Vector3 mouseDelta;

                if (m_LastMousePosition.x >= 0 &&
                    m_LastMousePosition.y >= 0 &&
                    m_LastMousePosition.x <= Screen.width &&
                    m_LastMousePosition.y <= Screen.height)
                    mouseDelta = new Vector3(inputAxis.x, inputAxis.y, 0) - m_LastMousePosition;
                else
                {
                    mouseDelta = Vector3.zero;
                }

                Vector3 rotation = Vector3.up * Time.deltaTime * m_RotateSpeed * mouseDelta.x;

                transform.Rotate(rotation, Space.World);

                rotation = transform.rotation.eulerAngles;
                rotation.z = 0;

                transform.rotation = Quaternion.Euler(rotation);
            }

            m_LastMousePosition = new Vector3(inputAxis.x, inputAxis.y, 0);
#else
            if (Input.GetKey(KeyCode.W))
            {
                m_PanMovement += Vector3.forward * m_MovementSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.S))
            {
                m_PanMovement -= Vector3.forward * m_MovementSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.A))
            {
                m_PanMovement += Vector3.left * m_MovementSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.D))
            {
                m_PanMovement += Vector3.right * m_MovementSpeed * Time.deltaTime;
            }

            transform.Translate(transform.TransformDirection(m_PanMovement), Space.World);

            if (Input.GetMouseButton(1))
            {
                Vector3 mouseDelta;

                if (m_LastMousePosition.x >= 0 &&
                    m_LastMousePosition.y >= 0 &&
                    m_LastMousePosition.x <= Screen.width &&
                    m_LastMousePosition.y <= Screen.height)
                    mouseDelta = Input.mousePosition - m_LastMousePosition;
                else
                {
                    mouseDelta = Vector3.zero;
                }

                Vector3 rotation = Vector3.up * Time.deltaTime * m_RotateSpeed * mouseDelta.x;

                transform.Rotate(rotation, Space.World);

                rotation = transform.rotation.eulerAngles;
                rotation.z = 0;

                transform.rotation = Quaternion.Euler(rotation);
            }

            m_LastMousePosition = Input.mousePosition;
#endif
        }
    }
}