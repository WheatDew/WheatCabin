using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Origin
{
    public class SurroundCamera : MonoBehaviour
    {
        public Transform targetCamera;
        public bool isControlled = true;
        public bool isMouseStartUIControlled = true;

        private bool m_isMouseStartUI = false;
        private void Update()
        {
            if (isMouseStartUIControlled)//判断鼠标是否从UI起始处点击
            {
                if (EventSystem.current.IsPointerOverGameObject() && (
                    Input.GetMouseButton(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
                    m_isMouseStartUI = true;

                if (m_isMouseStartUI && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)))
                    m_isMouseStartUI = false;
            }


            if (isControlled && !m_isMouseStartUI)//控制是否可控
            {
                if (Input.GetMouseButton(2))
                {
                    Vector3 settingPosition = transform.localPosition;
                    settingPosition.x += Input.GetAxis("Mouse X");
                    settingPosition.z += Input.GetAxis("Mouse Y");
                    transform.localPosition = settingPosition;
                }
                if (Input.GetMouseButton(1))
                {
                    transform.localRotation *= Quaternion.Euler(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
                    Vector3 settingRotation = transform.localRotation.eulerAngles;
                    settingRotation.z = 0;
                    transform.localRotation = Quaternion.Euler(settingRotation);
                }
                if (Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                    Vector3 settingPositionZ = targetCamera.localPosition;
                    settingPositionZ.z += Input.GetAxis("Mouse ScrollWheel");
                    targetCamera.localPosition = settingPositionZ;
                }
            }
        }
    }
}

