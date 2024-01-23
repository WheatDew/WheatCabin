using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CCharacterController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Animator _animator;

    private AnimatorStateInfo currentStateInfo;
    private float verticalInput,horizontalInput;

    public Camera playerCamera;

    public Vector3 verticalMultiple;


    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (verticalInput != 0 || horizontalInput != 0)
            Move();

        Animation();
    }

    private void Move()
    {

        Vector3 cameraDirect = playerCamera.transform.forward;

        cameraDirect.y = 0;

        transform.forward = cameraDirect;

        if (currentStateInfo.IsTag("Move"))
        {
            if (_animator.applyRootMotion)
                _animator.applyRootMotion = false;
            var velocity = transform.forward.normalized * verticalInput* verticalMultiple.z+ transform.right.normalized*horizontalInput*verticalMultiple.x;
            
            _rigidbody.velocity = velocity;

            Debug.LogFormat("{0} {1}", velocity, velocity.magnitude);
        }
    }

    public void Animation()
    {
        if (currentStateInfo.IsTag("Move"))
        {
            _animator.SetFloat("DV", verticalInput);
            _animator.SetFloat("DH", horizontalInput);
        }
    }
}
