using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarterView : MonoBehaviour
{
    public Camera targetCamera;
    [HideInInspector] public Transform target;

    private void Update()
    {
        transform.position = target.position;
    }
}
