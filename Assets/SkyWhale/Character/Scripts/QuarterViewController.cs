using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static UnityEditor.Sprites.Packer;

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

    public IEnumerator action;
    public Dictionary<string, List<IEnumerator>> actmap = new Dictionary<string, List<IEnumerator>>();
    public HashSet<string> blockList = new HashSet<string>();

    public string currentGroup = "", currentAction = "";
    // Use this for initialization
    void Start()
    {
        entity = GetComponent<CharacterEntity>();
        anim = GetComponent<Animator>();
        mainCamera = SCamera.s.currentCamera;
        agent = gameObject.AddComponent<NavMeshAgent>();
        
        agent.angularSpeed = 1080;
        agent.speed = 2;
        actmap.Add("Move",new List<IEnumerator> { Move() });
        actmap.Add("Equip", new List<IEnumerator> { Equip()});

        //优先级设定
        blockList.Add("Equip");
    }

    private void Update()
    {
        stateInfo = anim.GetCurrentAnimatorStateInfo(0);


        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(1))
            {
                Run("Move");
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
                    Run("Equip");
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


    public void Run(string key)
    {
        StopAllCoroutines();
        StartCoroutine(RunActions(key));
    }

    public IEnumerator RunActions(string key)
    {
        currentGroup = key;
        var list = actmap[key];
        for(int i = 0; i < list.Count; i++)
        {
            yield return list[i];
        }
        currentGroup = "";
    }

    IEnumerator Equip()
    {
        currentAction = "Equip";
        anim.SetTrigger("Equip");
        while (stateInfo.normalizedTime < 1)
        {
            yield return null;
        }
        Debug.Log("结束");
        currentAction = "";
    }

    IEnumerator Move()
    {
        currentAction = "Move";
        RaycastHit result;
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out result))
        {
            //Debug.LogFormat("设置点{0}", result.point);
            agent.SetDestination(result.point);
        }
        agent.isStopped = false;
        while (true)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                break;
            }
            yield return null;
        }
        Debug.Log("结束");
        anim.SetFloat("Speed", 0);
        agent.isStopped = true;
        currentAction = "";
    }

    ////更新目标方向
    //public virtual void UpdateTargetDirection()
    //{
    //    if (!useCharacterForward)
    //    {
    //        turnSpeedMultiplier = 1f;
    //        var forward = mainCamera.transform.TransformDirection(Vector3.forward);
    //        forward.y = 0;

    //        //get the right-facing direction of the referenceTransform
    //        var right = mainCamera.transform.TransformDirection(Vector3.right);

    //        // determine the direction the player will face based on input and the referenceTransform's right and forward directions
    //        targetDirection = input.x * right + input.y * forward;
    //    }
    //    else
    //    {
    //        turnSpeedMultiplier = 0.2f;
    //        var forward = transform.TransformDirection(Vector3.forward);
    //        forward.y = 0;

    //        //get the right-facing direction of the referenceTransform
    //        var right = transform.TransformDirection(Vector3.right);
    //        targetDirection = input.x * right + Mathf.Abs(input.y) * forward;
            
    //    }
    //}
}

