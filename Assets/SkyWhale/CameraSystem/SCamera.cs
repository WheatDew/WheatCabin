using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCamera : MonoBehaviour
{
    public ThirdPersonCameraGroup thirdPersonCameraGroup;



    public void SetFirstPerson()
    {
        thirdPersonCameraGroup.gameObject.SetActive(true);
    }
}
