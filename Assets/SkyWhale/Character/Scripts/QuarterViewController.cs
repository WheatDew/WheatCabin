using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

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
    // Use this for initialization
    void Start()
    {

        anim = GetComponent<Animator>();
        mainCamera = SCamera.s.currentCamera;
        agent = gameObject.AddComponent<NavMeshAgent>();
        
        agent.angularSpeed = 1080;
        agent.speed = 2;
    }

    private void Update()
    {
        stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        SetSpeed();
        

        if (isBattling && RangeCalculateSystem.s.Calculate(transform.position, 2).Count > 1 && stateInfo.normalizedTime > 0.6f)
        {
            anim.SetTrigger("Attack");
        }

        //Debug.Log(stateInfo.IsTag("Turn")+" "+stateInfo.IsTag("Walk"));

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(1))
            {
                MoveToPoint();
            }
            

            //// 获取用户的输入
            //float horizontalInput = Input.GetAxis("Horizontal");
            //float verticalInput = Input.GetAxis("Vertical");

            //// 计算移动向量
            //Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);

            //// 如果有输入，则改变朝向
            //if (movement != Vector3.zero)
            //{
            //    // 设置目标旋转角度
            //    Quaternion targetRotation = Quaternion.LookRotation(movement);

            //    // 平滑插值旋转角度
            //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            //}

            //anim.SetFloat("Speed", movement.magnitude);

            //stateInfo = anim.GetCurrentAnimatorStateInfo(0);

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

    public void InitQuarterViewController()
    {
        
    }

    //移动到鼠标点击位置
    public void MoveToPoint()
    {
        RaycastHit result;
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out result))
        {
            //Debug.LogFormat("设置点{0}", result.point);
            agent.SetDestination(result.point);
        }
    } 

    public void SetSpeed()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);
        Debug.Log(agent.velocity.magnitude);

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

