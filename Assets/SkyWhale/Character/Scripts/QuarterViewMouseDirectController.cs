using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

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

    public float timer=0;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        _animatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (_animatorStateInfo.IsTag("Attack"))
        {
            if (!_animator.applyRootMotion)
                _animator.applyRootMotion = true;
        }

        if (_animatorStateInfo.IsTag("Move"))
        {

            if (_animator.applyRootMotion)
                _animator.applyRootMotion = false;
            Vector3 dir_forward = transform.forward;
            Vector3 dir_right = transform.right;
            Vector3 velocity = Vector3.right * Input.GetAxisRaw("Horizontal") * _animationSpeed + Vector3.forward * Input.GetAxisRaw("Vertical") * _animationSpeed;
            var angle = Vector3.Angle(transform.forward, velocity);
            var vv = velocity;
            transform.forward = Vector3.Lerp(transform.forward, velocity.normalized, 0.1f);

            velocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = velocity;

            if(angle<90)
                _animator.SetFloat("DV", velocity.magnitude);
            else if(angle>=90)
                _animator.SetFloat("DV", -velocity.magnitude);

            //_animator.SetFloat("DH", Mathf.Sin(angle) * velocity.magnitude);
            //_animator.SetFloat("DV", Mathf.Cos(angle) * velocity.magnitude);
            

            //_animator.SetFloat("DH", _rigidbody.velocity.sqrMagnitude);

            //_animator.SetFloat("DV", velocity.magnitude);
            //_animator.SetFloat("DV", velocity.normalized.z);

            //Vector3 mousePosition = Input.mousePosition;

            //// 计算鼠标位置相对于屏幕中心的偏移向量
            //Vector3 offset = mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);

            //// 计算偏移向量的角度
            //float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.down);
        }
        else
        {
            if (!_animator.applyRootMotion)
                _animator.applyRootMotion = true;
        }

        if (Input.GetKeyDown(attackKeyboard))
        {
            attackTimer = timer;
        }

        if (Input.GetKeyUp(attackKeyboard))
        {
            if(timer > attackTimer + 0.5f)
            {
                _animator.SetTrigger("SpAttack");
            }
            else
            {
                _animator.SetTrigger("Attack");
            }

        }

        if (Input.GetKeyUp(skillKeyboard))
        {
            _animator.SetTrigger("Skill");
        }
    }

    private float attackTimer = 0;

}
