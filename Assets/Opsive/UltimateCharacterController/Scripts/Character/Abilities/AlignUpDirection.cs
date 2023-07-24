/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using UnityEngine;

    /// <summary>
    /// The AlignUpDirection ability provides a base class for any abilities that want to change the character's up rotation.
    /// </summary>
    public abstract class AlignUpDirection : Ability
    {
        [Tooltip("The smoothing that should be applied to the align direction.")]
        [SerializeField] protected float m_AlignSmoothing = 0.2f;
        [Tooltip("Should the gravity direction also be aligned?")]
        [SerializeField] protected bool m_AlignGravityDirection = true;
        [Tooltip("The direction of that should be set when the ability stops. Set to Vector3.zero to disable.")]
        [SerializeField] protected Vector3 m_StopDirection = Vector3.zero;

        public float AlignSmoothing { get => m_AlignSmoothing; set => m_AlignSmoothing = value; }
        public bool AlignGravityDirection { get => m_AlignGravityDirection; set => m_AlignGravityDirection = value; }
        public Vector3 StopDirection { get => m_StopDirection; set => m_StopDirection = value; }

        public override bool IsConcurrent { get => true; }
        public override bool CanStayActivatedOnDeath { get => true; }

        private Vector3 m_UpVelocity;
        private Vector3 m_GravityVelocity;
        protected bool m_Stopping;
        private bool m_StoppingFromUpdate;
        private float m_Epsilon = 1f - Mathf.Epsilon;

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_CharacterLocomotion.AlignToUpDirection = true;
        }

        /// <summary>
        /// Rotates the character to be oriented with the specified normal.
        /// </summary>
        /// <param name="targetNormal">The direction that the character should be oriented towards on the vertical axis.</param>
        protected void Rotate(Vector3 targetNormal)
        {
            m_CharacterLocomotion.Up = m_Stopping ? targetNormal : Vector3.SmoothDamp(m_CharacterLocomotion.Up, targetNormal, ref m_UpVelocity, m_AlignSmoothing).normalized;
            if (m_AlignGravityDirection) {
                m_CharacterLocomotion.GravityDirection = m_Stopping ? -targetNormal : Vector3.SmoothDamp(m_CharacterLocomotion.GravityDirection, -targetNormal, ref m_GravityVelocity, m_AlignSmoothing).normalized;
            }
        }

        /// <summary>
        /// Stops the ability if it needs to be stopped.
        /// </summary>
        public override void LateUpdate()
        {
            base.LateUpdate();

            // The ability should be stopped within LateUpdate so the character has a chance to be rotated.
            if (m_Stopping) {
                m_StoppingFromUpdate = true;
                StopAbility();
                m_StoppingFromUpdate = false;
            }
        }

        /// <summary>
        /// The ability is trying to stop. Ensure the character ends at the correct orientation.
        /// </summary>
        public override void WillTryStopAbility()
        {
            base.WillTryStopAbility();

            m_Stopping = true;
            if (m_StopDirection.sqrMagnitude > 0) {
                Rotate(m_StopDirection);
            }
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <param name="force">Should the ability be force stopped?</param>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility(bool force)
        {
            if (!base.CanStopAbility(force)) {
                return false;
            }

            // Don't stop until the character is oriented in the correct direction.
            if ((force || m_StoppingFromUpdate) && (m_StopDirection.sqrMagnitude == 0 || Vector3.Dot(m_Transform.rotation * Vector3.up, m_StopDirection) >= m_Epsilon)) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_Stopping = m_StoppingFromUpdate = false;
            // Another AlignUpDirection may still be active.
            if (!m_CharacterLocomotion.IsAbilityTypeActive<AlignUpDirection>()) {
                m_CharacterLocomotion.AlignToUpDirection = false;
            }
        }
    }
}