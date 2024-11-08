using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

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

    //״ָ̬ʾ
    [HideInInspector] public bool isBattling = false;
    AnimatorStateInfo stateInfo;

    NavMeshAgent agent;
    public CharacterEntity entity;
    public HashSet<WDEntity> inRangeEntities;

    //ָ���
    public string commandBuffer;
    public string commandExecuting;

    //֡������
    public int frameTimer = 0;

    public IEnumerator action;
    public Dictionary<string, IEnumerator> actmap = new Dictionary<string, IEnumerator>();
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
        actmap.Add("Move",Move());
        actmap.Add("Equip", Equip());

        //���ȼ��趨
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

    public void Run(string name)
    {
        StopAllCoroutines();
        StartCoroutine(actmap[name]);
    }

    IEnumerator Equip()
    {
        currentAction = "Equip";
        anim.SetTrigger("Equip");
        while (stateInfo.normalizedTime < 1)
        {
            yield return null;
        }
        Debug.Log("����");
        currentAction = "";
    }


    IEnumerator Move()
    {
        currentAction = "Move";
        RaycastHit result;
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out result))
        {
            //Debug.LogFormat("���õ�{0}", result.point);
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
        Debug.Log("����");
        anim.SetFloat("Speed", 0);
        agent.isStopped = true;
        currentAction = "";
    }

}

