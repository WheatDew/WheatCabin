/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.AI
{
    using Opsive.Shared.Events;
    using UnityEngine;

    /// <summary>
    /// Base class for moving the character with a pathfinding implementation.
    /// </summary>
    public abstract class PathfindingMovement : Ability
    {
        [Tooltip("Can the character apply new accelerations while in the air?")]
        [SerializeField] protected bool m_AllowMovementInAir = true;

        public bool AllowMovementInAir { get => m_AllowMovementInAir; set => m_AllowMovementInAir = value; }

        public override bool IsConcurrent { get => true; }

        /// <summary>
        /// Returns the desired input vector value. This will be used by the Ultimate Character Locomotion componnet.
        /// </summary>
        public abstract Vector2 InputVector { get; }
        /// <summary>
        /// Returns the desired rotation value. This will be used by the Ultimate Character Locomotion component.
        /// </summary>
        public abstract Vector3 DeltaRotation { get; }
        /// <summary>
        /// Returns if the agent has arrived at the destination.
        /// </summary>
        public abstract bool HasArrived { get; }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
        }

        /// <summary>
        /// Sets the destination of the pathfinding agent.
        /// </summary>
        /// <param name="target">The position to move towards.</param>
        /// <returns>True if the destination was set.</returns>
        public abstract bool SetDestination(Vector3 target);

        /// <summary>
        /// Returns the destination of the pathfinding agent.
        /// </summary>
        /// <returns>The destination of the pathfinding agent.</returns>
        public abstract Vector3 GetDestination();

        /// <summary>
        /// Teleports the agent to the specified position.
        /// </summary>
        /// <param name="position">The position that the agent should teleport to.</param>
        public abstract void Teleport(Vector3 position);

        /// <summary>
        /// Sets the rotation of the agent after they have arrived at the destination.
        /// </summary>
        /// <param name="rotation">The destination rotation.</param>
        public virtual void SetDestinationRotation(Quaternion rotation) { }

        /// <summary>
        /// Updates the character's input values.
        /// </summary>
        public override void Update()
        {
            if (!m_AllowMovementInAir && !m_CharacterLocomotion.Grounded) {
                m_CharacterLocomotion.InputVector = Vector2.zero;
                m_CharacterLocomotion.DeltaRotation = Vector3.zero;
                return;
            }
            m_CharacterLocomotion.InputVector = InputVector;
            m_CharacterLocomotion.DeltaRotation = DeltaRotation;
        }

        /// <summary>
        /// The character's position or rotation has been teleported.
        /// </summary>
        /// <param name="snapAnimator">Should the animator be snapped?</param>
        private void OnImmediateTransformChange(bool snapAnimator)
        {
            Teleport(m_Transform.position);
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
        }
    }
}