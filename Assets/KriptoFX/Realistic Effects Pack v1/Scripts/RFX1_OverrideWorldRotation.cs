using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RFX1_OverrideWorldRotation : MonoBehaviour
{
    public Vector3 WorldRotation;
    Transform t;
    void Start()
    {
        t = transform;
    }

    // Update is called once per frame
    void Update()
    {
        t.rotation = Quaternion.Euler(WorldRotation);
    }
}
