/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Events;
    using UnityEngine;

    /// <summary>
    /// The Slide ability will apply a force to the character if the character is on a steep slope.
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    public class Slide : Ability
    {
        [Tooltip("Steepness (in degrees) in which the character can slide.")]
        [Shared.Utility.MinMaxRange(0, 89)] [SerializeField] protected Shared.Utility.MinMaxFloat m_SlideLimit = new Shared.Utility.MinMaxFloat(50, 89);
        [Tooltip("Steepness (in degrees) in which the character can slide when on the edge of a platform.")]
        [SerializeField] protected float m_EdgeSlideLimit = 30;
        [Tooltip("Acceleration of the ground's slide value. The slide value is determined by (1 - dynamicFriction) of the ground's physic material.")]
        [SerializeField] protected float m_Acceleration = 0.14f;
        [Tooltip("The maximum speed that the character can slide.")]
        [SerializeField] protected float m_MaxSlideSpeed = 0.54f;
        [Tooltip("The rate at which the slide speed decelerates.")]
        [SerializeField] protected float m_SlideDamping = 0.08f;
        [Tooltip("Optionally specifies the up direction that should override the character's up direction.")]
        [SerializeField] protected Vector3 m_OverrideUpDirection;

        public Shared.Utility.MinMaxFloat SlideLimit { get { return m_SlideLimit; } set { m_SlideLimit = value; } }
        public float EdgeSlideLimit { get { return m_EdgeSlideLimit; } set { m_EdgeSlideLimit = value; } }
        public float Acceleration { get { return m_Acceleration; } set { m_Acceleration = value; } }
        public float MaxSlideSpeed { get { return m_MaxSlideSpeed; } set { m_MaxSlideSpeed = value; } }
        public float SlideDamping { get { return m_SlideDamping; } set { m_SlideDamping = value; } }
        public Vector3 OverrideUpDirection { get { return m_OverrideUpDirection; } set { m_OverrideUpDirection = value; } }

        private float m_OriginalStickToGroundDistance;
        private float m_SlideSpeed;
        private Vector3 m_SlideDirection;
        private Vector3 m_Momentum;

        public override bool IsConcurrent { get { return true; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            return CanSlide();
        }

        /// <summary>
        /// Returns true if the character can slide on the ground.
        /// </summary>
        /// <returns>True if the character can slide on the ground.</returns>
        private bool CanSlide()
        {
            // The character cannot slide in the air.
            if (!m_CharacterLocomotion.Grounded) {
                return false;
            }

            // If the character is on an edge then the slope limit is different.
            var upDirection = m_OverrideUpDirection.sqrMagnitude > 0 ? m_OverrideUpDirection : m_Transform.up;
            var slopeAngle = Vector3.Angle(m_CharacterLocomotion.GroundedRaycastHit.normal, upDirection);
            var ray = new Ray(m_Rigidbody.position + m_CharacterLocomotion.DesiredMovement + m_CharacterLocomotion.Up * m_CharacterLocomotion.ColliderSpacing * 2, -upDirection);
            if (!Physics.Raycast(ray, m_CharacterLocomotion.MaxStepHeight, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore)) {
                return slopeAngle >= m_EdgeSlideLimit - 0.001f;
            }

            ray = new Ray(m_CharacterLocomotion.GroundedRaycastHit.point + m_CharacterLocomotion.GroundedRaycastHit.normal * m_CharacterLocomotion.ColliderSpacing, -m_CharacterLocomotion.GroundedRaycastHit.normal);
            if (!Physics.Raycast(ray, out var slopeRaycastHit, m_CharacterLocomotion.ColliderSpacing * 2, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore)) {
                return false;
            }

            // The character cannot slide if the slope isn't steep enough or is too steep.
            slopeAngle = Vector3.Angle(slopeRaycastHit.normal, upDirection);
            if (slopeAngle < m_SlideLimit.MinValue + 0.001f || slopeAngle > m_SlideLimit.MaxValue - 0.001f) {
                return false;
            }

            // The character can slide.
            return true;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();
            m_SlideSpeed = 0;
            m_SlideDirection = Vector3.zero;
            m_CharacterLocomotion.ForceStickToGround = true;
            m_OriginalStickToGroundDistance = m_CharacterLocomotion.StickToGroundDistance;
        }

        /// <summary>
        /// Update the controller's position values.
        /// </summary>
        public override void UpdatePosition()
        {
            base.UpdatePosition();

            m_SlideSpeed /= (1 + m_SlideDamping * m_CharacterLocomotion.TimeScale * Time.timeScale);

            // The slide value uses the ground's physic material to get the amount of friction of the material.
            var upDirection = m_OverrideUpDirection.sqrMagnitude > 0 ? m_OverrideUpDirection : m_Transform.up;
            var slopeAngle = Vector3.Angle(m_CharacterLocomotion.GroundedRaycastHit.normal, upDirection);
            var direction = Vector3.Cross(Vector3.Cross(m_CharacterLocomotion.GroundedRaycastHit.normal, -upDirection), m_CharacterLocomotion.GroundedRaycastHit.normal).normalized;
            var directionDot = Vector3.Dot(m_Momentum.normalized, direction);

            // The slope may not be within the range but m_SlopeSpeed is still greater than 0 so the ability hasn't stopped yet. 
            var increaseSlideSpeed = slopeAngle >= m_SlideLimit.MinValue - 0.001f && slopeAngle <= m_SlideLimit.MaxValue + 0.001f;
            var minSlideValue = m_SlideLimit.MinValue;
            if (!increaseSlideSpeed) {
                // The character may be on the edge.
                var ray = new Ray(m_Rigidbody.position + m_CharacterLocomotion.DesiredMovement + m_CharacterLocomotion.Up * m_CharacterLocomotion.ColliderSpacing * 2, -upDirection);
                if (!Physics.Raycast(ray, m_CharacterLocomotion.MaxStepHeight, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore)) {
                    increaseSlideSpeed = slopeAngle >= m_EdgeSlideLimit - 0.001f;
                    minSlideValue = m_EdgeSlideLimit;
                }
            }

            if (increaseSlideSpeed) {
                // Increase the slide speed if the slope is in the direction of the character's momentum, otherwise decrease the slide speed.
                var slide = m_Acceleration * (1 - m_CharacterLocomotion.GroundedRaycastHit.collider.material.dynamicFriction) * ((slopeAngle - minSlideValue) / (m_SlideLimit.MaxValue - minSlideValue)) * (directionDot >= 0 ? 1 : -1);
                m_SlideSpeed = Mathf.Max(0, Mathf.Min(m_SlideSpeed + slide, m_MaxSlideSpeed));
            }

            if (m_SlideSpeed > 0) {
                // If the character isn't on a flat surface then they should move in the direction of the slope. The inverse direction will be used if the slope is facing the 
                // oppsite direction of the momentum.
                if (direction.sqrMagnitude > 0) {
                    if (directionDot >= 0) {
                        m_Momentum = m_SlideDirection = direction;
                    } else {
                        m_SlideDirection = -direction;
                    }
                }

                m_CharacterLocomotion.DesiredMovement += m_SlideSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * m_SlideDirection;
            } else if (direction.sqrMagnitude > 0) {
                // The slope is changing directions.
                m_Momentum = direction.normalized;
            }
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <param name="force">Should the ability be force stopped?</param>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility(bool force)
        {
            if (force) { return true; }

            return !CanSlide() && m_SlideSpeed <= m_CharacterLocomotion.ColliderSpacing;
        }

        /// <summary>
        /// The character has changed grounded state. 
        /// </summary>
        /// <param name="grounded">Is the character on the ground?</param>
        private void OnGrounded(bool grounded)
        {
            if (grounded) {
                if (!CanSlide()) {
                    m_SlideSpeed = 0;
                }
            } else {
                StopAbility(true);
            }
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            if (m_SlideSpeed > 0) {
                AddForce((m_SlideSpeed / Time.deltaTime) * m_CharacterLocomotion.TimeScale * Time.timeScale * m_SlideDirection, 1, false);
            }
            m_CharacterLocomotion.ForceStickToGround = false;
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
        }
    }
}