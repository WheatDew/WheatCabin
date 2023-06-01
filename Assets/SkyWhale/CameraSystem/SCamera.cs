using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCamera : MonoBehaviour
{
    public GameObject firstPersonCamera;



    public void SetFirstPerson()
    {
        firstPersonCamera.SetActive(true);
    }
}
