using NPOI.SS.Formula.Functions;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum BeadType { arrow, bead, noTarget, buff, target };
public class QuarterViewPlayerController : MonoBehaviour
{


    public KeyCode sprintKeyboard = KeyCode.LeftShift;
    public KeyCode attackKeyboard = KeyCode.Mouse0;
    public KeyCode skillKeyboard = KeyCode.Mouse1;
    public KeyCode skillKeyboard1 = KeyCode.Q;
    public KeyCode skillKeyboard2 = KeyCode.E;
    public KeyCode equipKeyboard = KeyCode.F;

    Rigidbody _rigidbody;
    Animator _animator;
    float _animationSpeed=4;
    AnimatorStateInfo _animatorStateInfo;

    public float timer=0;

    private bool skillPrepare=false;
    private bool spin = false;
    private GameObject bead;
    //private BeadType currentBead;
    private Dictionary<string, SkillInfo> skillData = new Dictionary<string, SkillInfo>();
    private string currentElemental;


    //事件组
    public UnityEvent elemental1ClickEvent = new();
    public UnityEvent elemental2ClickEvent = new();
    public UnityEvent elemental3ClickEvent = new();
    public UnityEvent elemental4ClickEvent = new();
    public UnityEvent skill1ClickEvent = new();
    public UnityEvent skill2ClickEvent = new();

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        elemental1ClickEvent.AddListener(() => SetElemental("fire", "fire_buff"));
        elemental2ClickEvent.AddListener(() => SetElemental("ice", "ice_buff"));
        skill1ClickEvent.AddListener(() => ExcuteSkillStart());
        skillData.Add("fire1", new SkillInfo("fire_buff", BeadType.arrow, "fire"));
    }

    private void Update()
    {
        if (!skillPrepare)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                elemental1ClickEvent.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                elemental2ClickEvent.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                elemental3ClickEvent.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                elemental4ClickEvent.Invoke();
            }
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

        if (Input.GetKeyDown(skillKeyboard1))
        {
            skill1ClickEvent.Invoke();
        }

        if (Input.GetKeyDown(skillKeyboard2))
        {
            skill2ClickEvent.Invoke();
        }

        //if (Input.GetKeyDown(skillKeyboard2)&&!skillPrepare)
        //{
        //    if (currentBead == BeadType.target)
        //    {
        //        skillPrepare = true;
        //        bead = EffectSystem.s.CreateEffect("notarget");
        //        bead.transform.localScale = Vector3.one * 0.2f;
        //    }
        //}
        //else if (skillPrepare)
        //{
        //    if (currentBead == BeadType.target)
        //    {
        //        if (mousePoisition != Vector3.zero)
        //        {
        //            if (mouseTarget.tag != "Character")
        //                bead.transform.position = Vector3.zero;
        //            else
        //                bead.transform.position = mouseTarget.transform.position;
        //            if(mouseTarget.tag =="Character" && mouseTarget != gameObject)
        //            {
        //                if (Input.GetKeyUp(skillKeyboard1))
        //                {
        //                    StartCoroutine(TargetAnimation(bead, mouseTarget.transform,0.1f));
        //                }
        //            }
        //        }
        //    }


        //}

        if (Input.GetKeyDown(skillKeyboard))
        {
            skillPrepare = false;
            Destroy(bead);
        }
    }

    #region 主状态相关

    private GameObject currentBuff=null;

    public void SetMainBuff(string buff)
    {
        currentBuff = EffectSystem.s.SetMainBuff(buff, transform,currentBuff);
    }

    #endregion


    private float attackTimer = 0;
    private Vector3 mouseDirection = Vector3.zero;
    private Vector3 mousePoisition = Vector3.zero;
    private GameObject mouseTarget;

    /// <summary>
    /// 判断鼠标方向
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

    public SkillInfo currentSkill;

    void SetElemental(string elemental,string effect)
    {
        if (!skillPrepare)
        {
            SetMainBuff(effect);
            currentElemental = elemental;
        }
    }

    IEnumerator executeSkillPrepare;

    private void ExcuteSkillStart()
    {
        currentSkill = skillData["fire"];

        skillPrepare = true;
        bead = EffectSystem.s.CreateEffect("arrow");
        
        if (executeSkillPrepare != null)
        {
            StopCoroutine(executeSkillPrepare);

        }
        executeSkillPrepare = ExecuteSkillPrepare();
        StartCoroutine(executeSkillPrepare);
    }

    IEnumerator ExecuteSkillPrepare()
    {
        while (skillPrepare)
        {
            if (currentSkill.beadType == BeadType.arrow)
            {
                bead.transform.position = transform.position;
                bead.transform.rotation = Quaternion.LookRotation(mouseDirection);
            }

            yield return null;
        }

    }

    IEnumerator ExecuteSkill(SkillInfo info)
    {
        //判断所属元素组
        yield return null;
    }
}

public class SkillInfo
{
    public string type;
    public BeadType beadType;
    public string effect;

    public SkillInfo(string type,BeadType beadType,string effect)
    {
        this.type = type;
        this.beadType = beadType;
        this.effect = effect;
    }
}
