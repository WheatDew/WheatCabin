using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCamera : MonoBehaviour
{
    public Transform target;

    private void OnEnable()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    private void Update()
    {
        if (target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, 0.05f);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, 0.05f);
        }
    }
}
