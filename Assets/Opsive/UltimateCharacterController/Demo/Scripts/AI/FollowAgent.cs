/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.AI
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.AI;
    using UnityEngine;

    /// <summary>
    /// Uses the PathfindingMovement ability to follow the specified target.
    /// </summary>
    public class FollowAgent : MonoBehaviour
    {
        [Tooltip("The target that the character should follow.")]
        [SerializeField] protected Transform m_Target;
        [Tooltip("The distance from which the character should follow from.")]
        [SerializeField] protected float m_Distance = 5;

        public Transform Target { get { return m_Target; } set { m_Target = value; } }
        public float Distance { get { return m_Distance; } set { m_Distance = value; } }

        private Transform m_Transform;
        private PathfindingMovement m_PathfindingMovement;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            var characterLocomotion = GetComponent<UltimateCharacterLocomotion>();
            m_PathfindingMovement = characterLocomotion.GetAbility<PathfindingMovement>();
        }

        /// <summary>
        /// Follow the target.
        /// </summary>
        private void Update()
        {
            var direction = m_Target.position - m_Transform.position;
            m_PathfindingMovement.SetDestination(m_Target.position - direction.normalized * m_Distance);
        }
    }
}