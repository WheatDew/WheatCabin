/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using UnityEngine;

    public class AgentHandler : MonoBehaviour, ICharacterHandler
    {
        [Tooltip("The horizontal input value of the agent.")]
        [SerializeField] protected float m_HorizontalMovement;
        [Tooltip("The forward input value of the agent.")]
        [SerializeField] protected float m_ForwardMovement;
        [Tooltip("The delta yaw input value of the agent.")]
        [SerializeField] protected float m_DeltaYawRotation;

        public float HorizontalMovement { get => m_HorizontalMovement; set => m_HorizontalMovement = value; }
        public float ForwardMovement { get => m_ForwardMovement; set => m_ForwardMovement = value; }
        public float DeltaYawRotation { get => m_DeltaYawRotation; set => m_DeltaYawRotation = value; }

        /// <summary>
        /// Returns the position input for the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        public void GetPositionInput(out float horizontalMovement, out float forwardMovement)
        {
            horizontalMovement = m_HorizontalMovement;
            forwardMovement = m_ForwardMovement;
        }

        /// <summary>
        /// Returns the rotation input for the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        /// <param name="deltaYawRotation">Value specifying the number of degrees changed on the local yaw axis.</param>
        public virtual void GetRotationInput(float horizontalMovement, float forwardMovement, out float deltaYawRotation)
        {
            deltaYawRotation = m_DeltaYawRotation;
        }
    }
}