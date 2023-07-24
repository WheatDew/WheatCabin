/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The Fall ability allows the character to play a falling animation when the character has a negative y velocity.
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    [DefaultAbilityIndex(2)]
    [DefaultUseRootMotionPosition(AbilityBoolOverride.False)]
    [DefaultUseRootMotionRotation(AbilityBoolOverride.False)]
    public class Fall : Ability
    {
        [Tooltip("The minimum height between the ground and character that the ability can start. Set to 0 to start at any height.")]
        [SerializeField] protected float m_MinFallHeight = 0.2f;
        [Tooltip("A reference to the Surface Impact triggered when the character hits the ground.")]
        [SerializeField] protected SurfaceImpact m_LandSurfaceImpact;
        [Tooltip("The minimum velocity required for the Surface Impact to play.")]
        [SerializeField] protected float m_MinSurfaceImpactVelocity = -4f;
        [Tooltip("Specifies if the ability should wait for the OnAnimatorFallComplete animation event or wait for the specified duration before ending the fall.")]
        [SerializeField] protected AnimationEventTrigger m_LandEvent = new AnimationEventTrigger(true, 0f);

        public float MinFallHeight { get => m_MinFallHeight; set => m_MinFallHeight = value; }
        public SurfaceImpact LandSurfaceImpact { get => m_LandSurfaceImpact; set => m_LandSurfaceImpact = value; }
        public float MinSurfaceImpactVelocity { get => m_MinSurfaceImpactVelocity; set => m_MinSurfaceImpactVelocity = value; }
        public AnimationEventTrigger LandEvent { get => m_LandEvent; set => m_LandEvent.CopyFrom(value); }

        public override int AbilityIntData => m_StateIndex;
        public override float AbilityFloatData => m_CharacterLocomotion.Velocity.y;

        private int m_StateIndex;
        private bool m_Landed;

        [Snapshot] protected bool Landed { get => m_Landed; set => m_Landed = value; }

        /// <summary>
        /// Can the ability be started?
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            // Fall can't be started if the character is on the ground.
            if (m_CharacterLocomotion.Grounded) {
                return false;
            }

            // The ground distance must be greater then the minimum fall height if a value is set.
            RaycastHit hit;
            if (m_MinFallHeight != 0 && Physics.Raycast(m_Transform.position, -m_Transform.up, out hit, m_MinFallHeight, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();
            m_StateIndex = 0;
            m_Landed = false;

            m_LandEvent.RegisterUnregisterAnimationEvent(true, m_GameObject, "OnAnimatorFallComplete", Land);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            return startingAbility is HeightChange;
        }

        /// <summary>
        /// The character has changed grounded states. 
        /// </summary>
        /// <param name="grounded">Is the character on the ground?</param>
        private void OnGrounded(bool grounded)
        {
            if (grounded) {
                // Allow the SurfaceManager to play an effect when the character hits the ground.
                if (m_LandSurfaceImpact != null && m_CharacterLocomotion.Velocity.y < m_MinSurfaceImpactVelocity) {
                    SurfaceManager.SpawnEffect(m_CharacterLocomotion.GroundedRaycastHit, m_LandSurfaceImpact, m_CharacterLocomotion.GravityDirection, m_CharacterLocomotion.TimeScale, m_GameObject, m_Transform.forward, false);
                }

                // Move to the fall end state when the character lands.
                m_StateIndex = 1;
                SetAbilityIntDataParameter(m_StateIndex);
                m_LandEvent.WaitForEvent();
            } else {
                m_StateIndex = 0;
                SetAbilityIntDataParameter(m_StateIndex);
            }
        }

        /// <summary>
        /// The character has landed.
        /// </summary>
        private void Land()
        {
            m_LandEvent.CancelWaitForEvent();
            m_Landed = true;
            // The controller may no longer be grounded during the time that it takes for the land animation to send the OnAnimatorFallComplete event.
            if (m_CharacterLocomotion.Grounded) {
                StopAbility();
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

            return m_Landed;
        }

        /// <summary>
        /// The character's position or rotation has been teleported.
        /// </summary>
        /// <param name="snapAnimator">Should the animator be snapped?</param>
        private void OnImmediateTransformChange(bool snapAnimator)
        {
            if (!m_CharacterLocomotion.Grounded && !snapAnimator) {
                return;
            }

            // The character is on the ground but fall is still active. Stop the fall.
            m_Landed = true;
            StopAbility();
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_LandEvent.RegisterUnregisterAnimationEvent(false, m_GameObject, "OnAnimatorFallComplete", Land);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterImmediateTransformChange", OnImmediateTransformChange);
        }
    }
}