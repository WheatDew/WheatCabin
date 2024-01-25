/// <summary>
/// Project : Easy Build System
/// Class : Demo_AbstractTargetFollower.cs
/// Namespace : EasyBuildSystem.Examples.Bases.Scripts.ThirdPerson
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

namespace EasyBuildSystem.Examples.Bases.Scripts.ThirdPerson
{
    public abstract class Demo_AbstractTargetFollower : MonoBehaviour
    {
        public enum UpdateType
        {
            FixedUpdate,
            LateUpdate,
            ManualUpdate
        }

        [Header("Target Follower Settings")]

        [SerializeField] Transform m_Target;
        public Transform Target { get { return m_Target; } }

        [SerializeField] bool m_AutoTargetPlayer = true;
        [SerializeField] UpdateType m_UpdateMode;

        public virtual void Start()
        {
            if (m_AutoTargetPlayer)
            {
                FindAndTargetPlayer();
            }

            if (m_Target == null)
            {
                return;
            }
        }

        void FixedUpdate()
        {
            if (m_AutoTargetPlayer && (m_Target == null || !m_Target.gameObject.activeSelf))
            {
                FindAndTargetPlayer();
            }

            if (m_UpdateMode == UpdateType.FixedUpdate)
            {
                FollowTarget(Time.deltaTime);
            }
        }

        void LateUpdate()
        {
            if (m_AutoTargetPlayer && (m_Target == null || !m_Target.gameObject.activeSelf))
            {
                FindAndTargetPlayer();
            }

            if (m_UpdateMode == UpdateType.LateUpdate)
            {
                FollowTarget(Time.deltaTime);
            }
        }

        public void ManualUpdate()
        {
            if (m_AutoTargetPlayer && (m_Target == null || !m_Target.gameObject.activeSelf))
            {
                FindAndTargetPlayer();
            }

            if (m_UpdateMode == UpdateType.ManualUpdate)
            {
                FollowTarget(Time.deltaTime);
            }
        }

        protected abstract void FollowTarget(float deltaTime);

        public void FindAndTargetPlayer()
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag("Player");

            if (targetObj)
            {
                SetTarget(targetObj.transform);
            }
        }

        public virtual void SetTarget(Transform newTransform)
        {
            m_Target = newTransform;
        }
    }
}