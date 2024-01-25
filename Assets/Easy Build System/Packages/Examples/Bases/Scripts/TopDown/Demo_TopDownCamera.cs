/// <summary>
/// Project : Easy Build System
/// Class : Demo_TopDownCamera.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.TopDown
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Examples.Bases.Scripts.TopDown
{
    public class Demo_TopDownCamera : MonoBehaviour
    {
        [SerializeField] Transform m_Target;
        [SerializeField] Vector3 m_Offset;

        void Start()
        {
            transform.parent = null;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            if (m_Target == null)
            {
                return;
            }

            transform.position = Vector3.Lerp(transform.position, m_Offset + m_Target.position, 5f * Time.deltaTime);
        }
    }
}