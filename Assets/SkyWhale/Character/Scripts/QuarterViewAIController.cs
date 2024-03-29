using Battlehub.RTTerrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class QuarterViewAIController : MonoBehaviour
{

    Rigidbody _rigidbody;
    Animator _animator;
    float _animationSpeed = 4;
    AnimatorStateInfo _animatorStateInfo;
    NavMeshAgent agent;
    Terrain terrain;

    public float timer = 0;

    [HideInInspector] public Vector3 target=Vector3.zero;

    private UnityAction update;

    

    void Start()
    {
        terrain = FindObjectOfType<Terrain>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        agent = gameObject.AddComponent<NavMeshAgent>();
        agent.angularSpeed = 720;
        _rigidbody.drag = 10;
        //agent.stoppingDistance = 1;
        SetWander();
    }

    private void Update()
    {
        update.Invoke();

        //Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        Vector3 velocity = agent.velocity;
        velocity.y = 0;
        

        _animator.SetFloat("DV", Vector3.Magnitude(velocity));
        //_animator.SetFloat("DH", localVelocity.x);
    }

    public bool isMoving;
    public float wanderTimer = 0;

    //随机漫游
    public void WanderAction()
    {


        if (!isMoving)
        {
            target = new Vector3(transform.position.x + Random.Range(-5, 5), transform.position.y, transform.position.z + Random.Range(-5, 5));

            // 获取地形高度
            float terrainHeight = terrain.SampleHeight(target);

            target.y = terrainHeight;

            agent.SetDestination(target);

            isMoving = true;

            Debug.DrawLine(transform.position, target, Color.red, 5);
        }

        Vector3 target_xz = target - target.y * Vector3.up;
        Vector3 position_xz = transform.position - transform.position.y * Vector3.up;
        print(Vector3.Distance(target_xz, position_xz));
        if (isMoving&&wanderTimer>3)
        {
            isMoving = false;
            wanderTimer = 0;
            
        }
        else if (isMoving)
        {
            wanderTimer += Time.deltaTime;
        }
        else if (isMoving&&Vector3.Distance(target_xz,position_xz)<2)
        {
            //target = transform.position;
            agent.isStopped = true;
        }
    }

    public void SetWander()
    {
        update += WanderAction;
    }
}
