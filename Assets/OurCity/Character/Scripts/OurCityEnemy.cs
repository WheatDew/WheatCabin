using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace OurCity
{
    public class OurCityEnemy : MonoBehaviour
    {

        Rigidbody _rigidbody;
        Animator _animator;
        float _animationSpeed = 4;
        AnimatorStateInfo _animatorStateInfo;
        NavMeshAgent agent;
        Terrain terrain;
        public float timer = 0;

        [HideInInspector] public Vector3 target = Vector3.zero;

        private UnityAction update;

        private bool isDeath = false;
        public EnemyRange range;
        public EnemyRange attackRange;
        private GameObject targetEntity;
        private HashSet<string> currentStatus = new HashSet<string>();

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
            SetSearch();
        }

        private void Update()
        {
            _animatorStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            update?.Invoke();

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
            //print(Vector3.Distance(target_xz, position_xz));
            if (isMoving && wanderTimer > 3)
            {
                isMoving = false;
                wanderTimer = 0;

            }
            else if (isMoving)
            {
                wanderTimer += Time.deltaTime;
            }
            else if (isMoving && Vector3.Distance(target_xz, position_xz) < 2)
            {
                //target = transform.position;
                agent.isStopped = true;
            }
        }

        private void SearchAction()
        {
            if (range.entities.Count > 0&&targetEntity==null)
            {
                foreach (var item in range.entities)
                {
                    targetEntity = item;
                }
                update -= WanderAction;
                
            }

            if (targetEntity != null)
            {
                agent.SetDestination(targetEntity.transform.position);
            }


            if (targetEntity != null && targetEntity.layer == 0)
            {
                targetEntity = null;
                update += WanderAction;
            }

        }

        private float attackInterval = 6;
        private bool isAttack = false;
        private void AttackAction()
        {
            attackInterval += Time.deltaTime;
            if (targetEntity != null&&Vector3.Distance(targetEntity.transform.position,transform.position)<1.5f)
            {
                if (attackInterval > 1)
                {

                    Vector3 directionToTarget = targetEntity.transform.position - transform.position;  // 计算目标方向
                    directionToTarget.y = 0;  // 移除Y轴分量，确保只在XZ平面内旋转

                    // 使用Quaternion.LookRotation创建旋转，使物体的前方朝向计算出的方向
                    Quaternion rotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = rotation;  // 应用旋转
                    _animator.SetTrigger("Attack");
                    isAttack = true;
                    attackInterval = 0;
                    agent.isStopped = true;
                }
            }

            if (targetEntity != null && _animatorStateInfo.IsTag("Move") && isAttack&&attackInterval>2)
            {
                isAttack = false;
                agent.isStopped = false;
                attackInterval = 0;
            }

            if (targetEntity!=null&&attackRange.entities.Contains(targetEntity))
            {
                targetEntity.GetComponent<PlayerController>().Death();
                targetEntity = null;
            }


        }

        public void SetWander()
        {
            update += WanderAction;
        }

        public void SetSearch()
        {
            update += SearchAction;
            update += AttackAction;
        }

        public void Death()
        {
            if (!isDeath)
            {
                _animator.SetTrigger("Death");
                isDeath = true;
                agent.isStopped = true;
                update -= WanderAction;
            }

        }

        

    }
}


