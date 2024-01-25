/// <summary>
/// Project : Easy Build System
/// Class : Demo_FreeCamera.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

#if EBS_INPUT_SYSTEM_SUPPORT
using UnityEngine.InputSystem;
#endif

namespace EasyBuildSystem.Examples.Bases.Scripts
{
    [RequireComponent(typeof(Demo_InputHandler))]
    public class Demo_FreeCamera : MonoBehaviour
    {
        [SerializeField] float m_Speed = 10f;
        [SerializeField] float m_FastSpeed = 100f;
        [SerializeField] float m_LookSensitivity = 3f;

        void Awake()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
#if EBS_INPUT_SYSTEM_SUPPORT
            
            if (Cursor.lockState != CursorLockMode.Locked)
            { 
                return;
            }

            bool fastMode = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            float movementSpeed = fastMode ? m_FastSpeed : m_Speed;

            transform.position += transform.TransformDirection(new Vector3(Demo_InputHandler.Instance.Move.x, 0, Demo_InputHandler.Instance.Move.y)
                * movementSpeed * Time.deltaTime);

            float newRotationX = transform.localEulerAngles.y + Demo_InputHandler.Instance.Look.x * m_LookSensitivity / 2;
            float newRotationY = transform.localEulerAngles.x - -Demo_InputHandler.Instance.Look.y * m_LookSensitivity / 2;

            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
#else

            if (Cursor.lockState != CursorLockMode.Locked)
            { 
                return;
            }

            bool fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float movementSpeed = fastMode ? m_FastSpeed : m_Speed;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position = transform.position + (-transform.right * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                transform.position = transform.position + (transform.right * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                transform.position = transform.position + (transform.forward * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                transform.position = transform.position + (-transform.forward * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.Space))
            {
                transform.position = transform.position + (transform.up * m_Speed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                transform.position = transform.position + (-transform.up * m_Speed * Time.deltaTime);
            }

            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * m_LookSensitivity;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * m_LookSensitivity;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
#endif
        }
    }
}