/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using UnityEngine;
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.Shared.StateSystem;

    /// <summary>
    /// Manages input handling for the locomotion component.
    /// </summary>
    public class CharacterHandler : StateBehavior, ICharacterHandler
    {
        [Tooltip("The name of the horizontal input mapping.")]
        [SerializeField] protected string m_HorizontalInputName = "Horizontal";
        [Tooltip("The name of the forward input mapping.")]
        [SerializeField] protected string m_ForwardInputName = "Vertical";

        public string HorizontalInputName { get { return m_HorizontalInputName; } set { m_HorizontalInputName = value; } }
        public string ForwardInputName { get { return m_ForwardInputName; } set { m_ForwardInputName = value; } }

        protected GameObject m_GameObject;
        protected IPlayerInput m_PlayerInput;

        protected float m_HorizontalMovement;
        protected float m_ForwardMovement;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        protected override void Awake()
        {
            if (!Game.CharacterInitializer.AutoInitialization) {
                Game.CharacterInitializer.Instance.OnAwake += AwakeInternal;
                return;
            }

            AwakeInternal();
        }

        /// <summary>
        /// Internal method which initializes the default values.
        /// </summary>
        protected virtual void AwakeInternal()
        {
            if (Game.CharacterInitializer.Instance != null) {
                Game.CharacterInitializer.Instance.OnAwake -= AwakeInternal;
            }

            base.Awake();

            m_PlayerInput = gameObject.GetCachedComponent<IPlayerInput>();
        }

        /// <summary>
        /// Returns the input for the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        /// <param name="deltaYawRotation">Value specifying the number of degrees changed on the local yaw axis.</param>
        public virtual void GetPositionInput(out float horizontalMovement, out float forwardMovement)
        {
            if (!enabled) {
                horizontalMovement = forwardMovement = 0;
                return;
            }
            horizontalMovement = m_PlayerInput.GetAxisRaw(m_HorizontalInputName);
            forwardMovement = m_PlayerInput.GetAxisRaw(m_ForwardInputName);
        }

        /// <summary>
        /// Returns the rotation input for the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        /// <param name="deltaYawRotation">Value specifying the number of degrees changed on the local yaw axis.</param>
        public virtual void GetRotationInput(float horizontalMovement, float forwardMovement, out float deltaYawRotation)
        {
            deltaYawRotation = 0;
        }
    }
}