using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class QuarterViewPlayerController : MonoBehaviour
{
    private enum BeadType { arrow, bead, noTarget, buff, target };

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
    private GameObject bead;
    private BeadType currentBead;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentBead = BeadType.arrow;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentBead = BeadType.bead;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentBead = BeadType.noTarget;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentBead = BeadType.buff;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            currentBead = BeadType.target;
        }


        MouseDirection();
        timer += Time.deltaTime;
        _animatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (spin)
            return;

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
            if (currentBead == BeadType.arrow)
            {
                skillPrepare = true;
                bead = EffectSystem.s.CreateEffect("arrow");
            }
            else if(currentBead == BeadType.bead)
            {
                skillPrepare = true;
                bead = EffectSystem.s.CreateEffect("bead");
            }
            else if (currentBead == BeadType.noTarget)
            {
                skillPrepare = true;
                bead = EffectSystem.s.CreateEffect("notarget");
                bead.transform.localScale = Vector3.one*2;
            }
            else if (currentBead == BeadType.buff)
            {
                skillPrepare = true;
                bead = EffectSystem.s.CreateEffect("notarget");
                bead.transform.localScale = Vector3.one*0.25f;
            }
            else if (currentBead == BeadType.target)
            {
                skillPrepare = true;
                bead = EffectSystem.s.CreateEffect("notarget");
                bead.transform.localScale = Vector3.one * 0.2f;
            }
        }
        else if (skillPrepare)
        {
            if (currentBead == BeadType.arrow)
            {
                bead.transform.position = transform.position;
                bead.transform.rotation = Quaternion.LookRotation(mouseDirection);

                if (Input.GetKeyUp(skillKeyboard))
                {
                    StartCoroutine(SpinAnimation(bead, mouseDirection, 0.1f));
                }
            }
            else if (currentBead == BeadType.bead)
            {
                if (mousePoisition != Vector3.zero)
                    bead.transform.position = mousePoisition;

                if (Input.GetKeyUp(skillKeyboard))
                {
                    StartCoroutine(BeadAnimation(bead, 6));
                }
            }
            else if (currentBead == BeadType.noTarget)
            {
                bead.transform.position = transform.position;
                if (Input.GetKeyUp(skillKeyboard))
                {
                    StartCoroutine(NoTargetAnimation(bead, 6));
                }
            }
            else if (currentBead == BeadType.buff)
            {
                bead.transform.position = transform.position;
                if (Input.GetKeyUp(skillKeyboard))
                {
                    StartCoroutine(BuffAnimation(bead, 6));
                }
            }
            else if (currentBead == BeadType.target)
            {
                if (mousePoisition != Vector3.zero)
                {
                    if (mouseTarget.tag != "Character")
                        bead.transform.position = Vector3.zero;
                    else
                        bead.transform.position = mouseTarget.transform.position;
                    if(mouseTarget.tag =="Character" && mouseTarget != gameObject)
                    {
                        if (Input.GetKeyUp(skillKeyboard))
                        {
                            StartCoroutine(TargetAnimation(bead, mouseTarget.transform,0.1f));
                        }
                    }
                }


            }


        }
    }

    private float attackTimer = 0;
    private Vector3 mouseDirection = Vector3.zero;
    private Vector3 mousePoisition = Vector3.zero;
    private GameObject mouseTarget;

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
            mousePoisition = result.point;
            mouseTarget = result.collider.gameObject;
        }
        else
        {
            mouseDirection = Vector3.zero;
            mousePoisition = Vector3.zero;
            mouseTarget = null;
        }
    }

    //spin arrow
    private IEnumerator SpinAnimation(GameObject arrow,Vector3 direct, float time)
    {
        spin = true;
        Quaternion target = Quaternion.LookRotation(mouseDirection);
        Quaternion origin = transform.rotation;


        float timer = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(origin, target, timer / time);


            yield return null;
        }
        _animator.SetTrigger("Skill");
        yield return new WaitForSeconds(0.1f);
        EffectSystem.s.CreateDirectivityEffect("fire", transform.position + Vector3.up, mouseDirection, 3, 3);
        Destroy(arrow);
        skillPrepare = false;
        spin = false;
    }

    //bead
    private IEnumerator BeadAnimation(GameObject bead, float time)
    {
        _animator.SetTrigger("Skill");
        yield return new WaitForSeconds(0.1f);
        EffectSystem.s.CreateBeadEffect("stone", mousePoisition, time);
        Destroy(bead);
        skillPrepare = false;
        spin = false;
    }

    //spin notarget
    private IEnumerator NoTargetAnimation(GameObject ring, float time)
    {
        spin = true;

        yield return new WaitForSeconds(0.1f);
        EffectSystem.s.CreateBeadEffect("ring", transform.position, time);
        Destroy(ring);
        skillPrepare = false;
        spin = false;
    }

    //spin buff
    private IEnumerator BuffAnimation(GameObject ring, float time)
    {
        spin = true;

        yield return new WaitForSeconds(0.1f);
        EffectSystem.s.CreateBuffEffect("heal", transform, time);
        Destroy(ring);
        skillPrepare = false;
        spin = false;
    }

    //target
    private IEnumerator TargetAnimation(GameObject bead, Transform follow,float spinTime)
    {
        spin = true;
        Quaternion target = Quaternion.LookRotation(mouseDirection);
        Quaternion origin = transform.rotation;


        float timer = 0;
        while (timer < spinTime)
        {
            timer += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(origin, target, timer / spinTime);


            yield return null;
        }
        _animator.SetTrigger("Skill");
        yield return new WaitForSeconds(0.1f);
        EffectSystem.s.CreateTargetEffect("fire", transform.position+Vector3.up, follow, 3,"fire_collision");
        Destroy(bead);
        skillPrepare = false;
        spin = false;
    }
}
