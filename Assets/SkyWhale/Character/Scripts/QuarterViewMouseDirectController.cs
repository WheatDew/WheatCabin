using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
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

    private bool skillPrepare=false;
    private bool spin = false;
    private GameObject arrow;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        MouseDirection();
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

        if (Input.GetKeyUp(skillKeyboard)&&!skillPrepare)
        {
            skillPrepare = true;
            arrow = EffectSystem.s.CreateEffect("arrow");
        }
        else if (skillPrepare)
        {

            arrow.transform.position = transform.position;
            arrow.transform.rotation = Quaternion.LookRotation(mouseDirection);

            if (Input.GetKeyUp(skillKeyboard))
            {
                transform.rotation = Quaternion.LookRotation(mouseDirection);
                _animator.SetTrigger("Skill");
                EffectSystem.s.CreateDirectivityEffect("fire", transform.position + Vector3.up, mouseDirection, 3, 3);
                Destroy(arrow);
                skillPrepare = false;
            }
        }
    }

    private float attackTimer = 0;
    private Vector3 mouseDirection = Vector3.zero;

    /// <summary>
    /// ≈–∂œ Û±Í∑ΩœÚ
    /// </summary>
    private void MouseDirection()
    {
        RaycastHit result;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out result))
        {
            Vector3 pos = transform.position;
            Vector3 mpos = result.point;
            pos.y = 0;
            mpos.y = 0;
            mouseDirection = mpos - pos;
        }
        else
        {
            mouseDirection = Vector3.zero;
        }
    }

    //spin
    private IEnumerator SpinAnimation(float time)
    {
        float timer = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
