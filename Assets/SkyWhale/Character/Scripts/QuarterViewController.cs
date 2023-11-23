using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static UnityEditor.Sprites.Packer;

//[RequireComponent(typeof(AnimatorAddon))]
public class QuarterViewController : MonoBehaviour
{

    public bool useCharacterForward = false;
    public bool lockToCameraForward = false;
    public float turnSpeed = 10f;
    public KeyCode sprintJoystick = KeyCode.JoystickButton2;
    public KeyCode sprintKeyboard = KeyCode.LeftShift;
    public KeyCode attackKeyboard = KeyCode.Mouse0;
    public KeyCode equipKeyboard = KeyCode.F;

    private float turnSpeedMultiplier;
    private float speed = 0f;
    private float direction = 0f;
    private bool isSprinting = false;
    private Animator anim;
    private Vector3 targetDirection;
    private Vector2 input;
    private Quaternion freeRotation;
    private Camera mainCamera;
    private float velocity;

    public float rotationSpeed = 100f;
    private bool isTurnAround = false;

    //状态指示
    [HideInInspector] public bool isBattling = false;
    AnimatorStateInfo stateInfo;

    NavMeshAgent agent;
    public CharacterEntity entity;
    public HashSet<Entity> inRangeEntities;

    //指令缓存
    public string commandBuffer;
    public string commandExecuting;

    //帧计数器
    public int frameTimer = 0;

    // Use this for initialization
    void Start()
    {
        entity = GetComponent<CharacterEntity>();
        anim = GetComponent<Animator>();
        mainCamera = SCamera.s.currentCamera;
        agent = gameObject.AddComponent<NavMeshAgent>();
        
        agent.angularSpeed = 1080;
        agent.speed = 2;
    }

    private void Update()
    {
        stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        CommandCircle(ref commandBuffer,ref commandExecuting,ref stateInfo);

        

        //Debug.Log(stateInfo.IsTag("Turn")+" "+stateInfo.IsTag("Walk"));

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(1))
            {
                commandBuffer = "Move";
            }

            if (Input.GetKeyDown(equipKeyboard))
            {
                if (isBattling)
                {
                    anim.SetTrigger("Unarm");
                    isBattling = false;
                }
                else
                {
                    anim.SetTrigger("Equip");
                    isBattling = true;
                }
            }
            if (Input.GetKeyDown(attackKeyboard))
            {
                if (isBattling)
                {
                    anim.SetTrigger("Attack");
                }
            }


        }

    }

    //命令循环
    private void CommandCircle(ref string buffer,ref string executing,ref AnimatorStateInfo stateInfo)
    {


        //状态更新
        if (executing != null)
        {
            if (executing == "Move")
            {
                CheckMoveEnd(ref executing);
                SetSpeed();
            }

        }

        if (frameTimer > 0)
        {
            frameTimer--;
            return;
        }

        //无覆盖指令
        if (buffer != null&& executing == null)
        {
            if (buffer == "Attack"&& isBattling&&stateInfo.normalizedTime>0.6f)
            {
                AttackPrepare();
            }
        }

        //可覆盖指令
        if(buffer != null)
        {
            if (buffer == "Move"&& (executing == null||executing=="Move"))
            {
                MoveToPoint(ref executing);
                buffer = null;
                //frameTimer = 0;
            }
        }


    }

    public void AttackPrepare()
    {
        RaycastHit result;
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out result))
        {
            if (PropertyMap.s.entityMap[result.transform.GetInstanceID()]&&!isBattling)
            {
                anim.SetTrigger("Equip");
                isBattling = true;
            }

        }
    }

    public void AttackAction()
    {
        inRangeEntities = RangeCalculateSystem.s.Calculate(entity, 2);
        if (inRangeEntities.Count > 0)
        {
            transform.LookAt(inRangeEntities.First().transform);
            anim.SetTrigger("Attack");
        }
    }

    //移动到鼠标点击位置
    public void MoveToPoint(ref string executing)
    {
        RaycastHit result;
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out result))
        {
            //Debug.LogFormat("设置点{0}", result.point);
            agent.SetDestination(result.point);
            executing = "Move";
            
        }
    } 

    private void CheckMoveEnd(ref string executing)
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
            executing = null;
        else
            executing = "Move";
    }

    public void SetSpeed()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);
        //Debug.Log(agent.velocity.magnitude);

    }

    public void  StopAgentOnAnimationPlay(AnimatorStateInfo stateInfo)
    {
        if (!stateInfo.IsTag("Move"))
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }
    }

    //更新目标方向
    public virtual void UpdateTargetDirection()
    {
        if (!useCharacterForward)
        {
            turnSpeedMultiplier = 1f;
            var forward = mainCamera.transform.TransformDirection(Vector3.forward);
            forward.y = 0;

            //get the right-facing direction of the referenceTransform
            var right = mainCamera.transform.TransformDirection(Vector3.right);

            // determine the direction the player will face based on input and the referenceTransform's right and forward directions
            targetDirection = input.x * right + input.y * forward;
        }
        else
        {
            turnSpeedMultiplier = 0.2f;
            var forward = transform.TransformDirection(Vector3.forward);
            forward.y = 0;

            //get the right-facing direction of the referenceTransform
            var right = transform.TransformDirection(Vector3.right);
            targetDirection = input.x * right + Mathf.Abs(input.y) * forward;
        }
    }
}

