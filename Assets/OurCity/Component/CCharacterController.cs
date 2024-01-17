using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CCharacterController : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        
    }

    private void Move()
    {
        Vector3 direct = transform.forward;
        direct.y = 0;
        _rigidbody.velocity = direct * Input.GetAxisRaw("Horizontal");

    }
}
