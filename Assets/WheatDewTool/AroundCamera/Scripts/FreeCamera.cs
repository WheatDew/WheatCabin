using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButton(2))
        {
            Vector3 settingPosition = transform.localPosition;
            settingPosition += new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
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
            Vector3 settingPositionZ = transform.localPosition;
            settingPositionZ.z += Input.GetAxis("Mouse ScrollWheel");
            transform.localPosition = settingPositionZ;
        }
    }
}
