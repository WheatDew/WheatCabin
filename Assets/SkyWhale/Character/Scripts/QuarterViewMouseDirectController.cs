using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class QuarterViewMouseDirectController : MonoBehaviour
{
    public KeyCode sprintKeyboard = KeyCode.LeftShift;
    public KeyCode attackKeyboard = KeyCode.Mouse0;
    public KeyCode skillKeyboard = KeyCode.Mouse1;
    public KeyCode equipKeyboard = KeyCode.F;

    Rigidbody _rigidbody;
    Animator _animator;
    float _animationSpeed=4;
    AnimatorStateInfo _animatorStateInfo;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        _animatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (_animatorStateInfo.IsTag("Move"))
        {
            if (_animator.applyRootMotion)
                _animator.applyRootMotion = false;
            Vector3 dir_forward = transform.forward;
            Vector3 dir_right = transform.right;
            Vector3 velocity = Vector3.right * Input.GetAxisRaw("Horizontal") * _animationSpeed + Vector3.forward * Input.GetAxisRaw("Vertical") * _animationSpeed;
            velocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = velocity;

            _animator.SetFloat("DH", Input.GetAxisRaw("Horizontal"));
            _animator.SetFloat("DV", Input.GetAxisRaw("Vertical"));

            Vector3 mousePosition = Input.mousePosition;

            // �������λ���������Ļ���ĵ�ƫ������
            Vector3 offset = mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);

            // ����ƫ�������ĽǶ�
            float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.down);
        }
        else
        {
            if (!_animator.applyRootMotion)
                _animator.applyRootMotion = true;
        }

        
        if (Input.GetKeyDown(attackKeyboard))
        {
            _animator.SetTrigger("Attack");
        }

        if (Input.GetKeyDown(skillKeyboard))
        {
            _animator.SetTrigger("Skill");
        }
    }


}
