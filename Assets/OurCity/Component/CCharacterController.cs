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

    public float forwardMultiple,backMutiple,rightMultiple;


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
        else
            MoveStop();

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
            //var velocity = verticalInput>0 ?
            //    transform.forward.normalized * verticalInput* forwardMultiple: transform.forward.normalized * verticalInput * backMutiple + transform.right.normalized*horizontalInput*rightMultiple;

            var velocity =
                transform.forward.normalized * verticalInput * forwardMultiple + transform.right.normalized * horizontalInput * rightMultiple;

            _rigidbody.velocity = velocity;

            //Debug.LogFormat("{0} {1}", velocity, velocity.magnitude);
        }
    }

    public void MoveStop()
    {
        _rigidbody.velocity = Vector3.zero;
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
