/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Audio;
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The Jump ability allows the character to jump into the air. Jump is only active when the character has a positive y velocity.
    /// </summary>
    [DefaultInputName("Jump")]
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultStopType(AbilityStopType.Automatic)]
    [DefaultAbilityIndex(1)]
    [DefaultUseRootMotionPosition(AbilityBoolOverride.False)]
    [DefaultUseRootMotionRotation(AbilityBoolOverride.False)]
    public class Jump : Ability
    {
        [Tooltip("Prevents the jump ability from starting if there is a flat object above the character within the specified distance. Set to -1 to disable.")]
        [SerializeField] protected float m_MinCeilingJumpHeight = 0.05f;
        [Tooltip("The amount of time after the character is airborne that the character can still jump. Also known as coyote time. " +
                 "Set to -1 to allow for the character to jump at any time after being airborne.")]
        [SerializeField] protected float m_GroundedGracePeriod = 0.05f;
        [Tooltip("Should the jump be prevented when the character is on a slope greater than the slope limit?")]
        [SerializeField] protected bool m_PreventSlopeLimitJump = true;
        [Tooltip("The amount of force that should be applied when the character jumps.")]
        [SerializeField] protected float m_Force = 0.2f;
        [Tooltip("A multiplier applied to the force while moving sideways.")]
        [SerializeField] protected float m_SidewaysForceMultiplier = 0.8f;
        [Tooltip("A multiplier applied to the force while moving backwards.")]
        [SerializeField] protected float m_BackwardsForceMultiplier = 0.7f;
        [Tooltip("The number of frames that the force is applied in.")]
        [SerializeField] protected int m_Frames = 1;
        [Tooltip("Determines how quickly the jump force wears off.")]
        [SerializeField] protected float m_ForceDamping = 0.18f;
        [Tooltip("Specifies if the ability should wait for the OnAnimatorJump animation event or wait for the specified duration before applying the jump force.")]
        [SerializeField] protected AnimationEventTrigger m_JumpEvent = new AnimationEventTrigger(true, 0f);
        [Tooltip("The Surface Impact triggered when the character jumps.")]
        [SerializeField] protected SurfaceImpact m_JumpSurfaceImpact;
        [Tooltip("The amount of force to add per frame if the jump button is being held down continuously. This is a common feature for providing increased jump control in platform games.")]
        [SerializeField] protected float m_ForceHold = 0.003f;
        [Tooltip("Determines how quickly the jump hold force wears off.")]
        [SerializeField] protected float m_ForceDampingHold = 0.5f;
        [Tooltip("Specifies the number of times the character can perform a airborne jump (double jump, triple jump, etc). Set to -1 to allow an infinite number of airborne jumps.")]
        [SerializeField] protected int m_MaxAirborneJumpCount;
        [Tooltip("The amount of force that applied when the character performs an airborne jump.")]
        [SerializeField] protected float m_AirborneJumpForce = 0.6f;
        [Tooltip("The number of frames that the repeated jump force is applied in.")]
        [SerializeField] protected int m_AirborneJumpFrames = 10;
        [Tooltip("Contains an array of AudioClips toat can be played when a repeated jump occurs.")]
        [SerializeField] protected AudioClipSet m_AirborneJumpAudioClipSet = new AudioClipSet();
        [Tooltip("A vertical velocity value below the specified amount will stop the ability.")]
        [SerializeField] protected float m_VerticalVelocityStopThreshold = 0.01f;
        [Tooltip("The number of seconds that the jump ability has to wait after it can start again (includes repeated jumps).")]
        [SerializeField] protected float m_RecurrenceDelay = 0.2f;

        public float MinCeilingJumpHeight { get { return m_MinCeilingJumpHeight; } set { m_MinCeilingJumpHeight = value; } }
        public float GroundedGracePeriod { get { return m_GroundedGracePeriod; } set { m_GroundedGracePeriod = value; } }
        public bool PrevntSlopeLimitJump { get { return m_PreventSlopeLimitJump; } set { m_PreventSlopeLimitJump = value; } }
        public float Force { get { return m_Force; } set { m_Force = value; } }
        public float SidewaysForceMultiplier { get { return m_SidewaysForceMultiplier; } set { m_SidewaysForceMultiplier = value; } }
        public float BackwardsForceMultiplier { get { return m_BackwardsForceMultiplier; } set { m_BackwardsForceMultiplier = value; } }
        public int Frames { get { return m_Frames; } set { m_Frames = value; } }
        public float ForceDamping { get { return m_ForceDamping; } set { m_ForceDamping = value; } }
        public AnimationEventTrigger JumpEvent { get { return m_JumpEvent; } set { m_JumpEvent.CopyFrom(value); } }
        public SurfaceImpact JumpSurfaceImpact { get { return m_JumpSurfaceImpact; } set { m_JumpSurfaceImpact = value; } }
        public float ForceHold { get { return m_ForceHold; } set { m_ForceHold = value; } }
        public float ForceDampingHold { get { return m_ForceDampingHold; } set { m_ForceDampingHold = value; } }
        public int MaxAirborneJumpCount { get { return m_MaxAirborneJumpCount; } set { m_MaxAirborneJumpCount = value; } }
        public float AirborneJumpForce { get { return m_AirborneJumpForce; } set { m_AirborneJumpForce = value; } }
        public int AirborneJumpFrames { get { return m_AirborneJumpFrames; } set { m_AirborneJumpFrames = value; } }
        public AudioClipSet AirborneJumpAudioClipSet { get { return m_AirborneJumpAudioClipSet; } set { m_AirborneJumpAudioClipSet = value; } }
        public float VerticalVelocityStopThreshold { get { return m_VerticalVelocityStopThreshold; } set { m_VerticalVelocityStopThreshold = value; } }
        public float RecurrenceDelay { get { return m_RecurrenceDelay; } set { m_RecurrenceDelay = value; } }

        private UltimateCharacterLocomotionHandler m_Handler;
        private ActiveInputEvent m_HoldInput;
        private ActiveInputEvent m_AirborneJumpInput;

        private float m_ActiveForce;
        private float[] m_ActiveSoftForceFrames;
        private RaycastHit m_RaycastResult;
        private bool m_ImmediateJump;
        private bool m_Jumping;
        private bool m_ApplyHoldForce;
        private float m_HoldForce;
        private int m_AirborneJumpCount;
        private bool m_AirborneJumpApplied;
        private float m_JumpTime = -1;
        private float m_LandTime = -1;
        private float m_InAirTime = -1;
        private bool m_AirborneJumpRegistered;

        [Snapshot] protected float HoldForce { get { return m_HoldForce; } set { m_HoldForce = value; } }
        [Snapshot] public bool Jumping { get { return m_Jumping; } set { m_Jumping = value; } }
        public bool ImmediateJump { set { m_ImmediateJump = value; } }

        public override float AbilityFloatData { get { if (m_Jumping) { return m_CharacterLocomotion.Velocity.y; } return -1; } }
        public override int AbilityIntData { get { return (m_AirborneJumpCount > 0 ? 2 : (m_Jumping ? 0 : 1)); } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Handler = m_GameObject.GetCachedComponent<UltimateCharacterLocomotionHandler>();
            m_ActiveSoftForceFrames = new float[m_CharacterLocomotion.MaxSoftForceFrames];

            m_JumpEvent.RegisterUnregisterAnimationEvent(true, m_GameObject, "OnAnimatorJump", ApplyJumpForce);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
        }

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

            if (m_MinCeilingJumpHeight != -1) {
                // Ensure the space above is clear to get off of the ground.
                if (m_CharacterLocomotion.SingleCast(m_CharacterLocomotion.Up, Vector3.zero,
                                                     (m_MinCeilingJumpHeight + m_CharacterLocomotion.SkinWidth + m_CharacterLocomotion.ColliderSpacing), m_CharacterLayerManager.SolidObjectLayers, ref m_RaycastResult)) {
                    var ray = new Ray(m_RaycastResult.point + m_RaycastResult.normal * m_CharacterLocomotion.ColliderSpacing, -m_RaycastResult.normal);
                    if (!Physics.Raycast(ray, out var ceilingRaycastHit, m_CharacterLocomotion.ColliderSpacing * 2, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore)) {
                        ceilingRaycastHit = m_RaycastResult;
                    }

                    if (Vector3.Angle(-m_CharacterLocomotion.Up, ceilingRaycastHit.normal) == 0) {
                        return false;
                    }
                }
            }

            if (m_CharacterLocomotion.Grounded) {
                // The character can't jump if they aren't on the ground nor if they recently landed.
                if (m_LandTime + m_RecurrenceDelay >= Time.time) {
                    return false;
                }

                // The character can't jump if the slope is too steep.
                if (m_PreventSlopeLimitJump) {
                    // Do not use the m_CharacterLocomotion.GroundedRaycastHit result as it uses a character cast instead of casting against the object directly beneath
                    // the character.
                    var ray = new Ray(m_Transform.position + m_CharacterLocomotion.Up * (m_CharacterLocomotion.MaxStepHeight - m_CharacterLocomotion.ColliderSpacing), -m_CharacterLocomotion.Up);
                    Physics.Raycast(ray, out var raycastHit, m_CharacterLocomotion.MaxStepHeight + m_CharacterLocomotion.ColliderSpacing, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore);
                    var slope = Vector3.Angle(m_Transform.up, raycastHit.normal);
                    if (slope > m_CharacterLocomotion.SlopeLimit) {
                        return false;
                    }
                }
            } else {
                // The airborne jump should play if the character walks off a ledge and did not initially jump.
                if (m_AirborneJumpCount < m_MaxAirborneJumpCount) {
                    m_Jumping = true;
                    return true;
                }

                // Allow the ability to start if the character is in the air before the grounded grace period. This allows the character to run off a ledge but still
                // be able to jump. The character may also be able to do a repeated jump even if the character isn't grounded.
                if ((!m_Jumping && m_GroundedGracePeriod != -1 && m_InAirTime + m_GroundedGracePeriod <= Time.time) || 
                    (m_AirborneJumpCount != -1 && m_AirborneJumpCount > m_MaxAirborneJumpCount) || (m_JumpTime != -1 && m_JumpTime + m_RecurrenceDelay > Time.time)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            if (activeAbility is Fall) {
                return true;
            }
            return base.ShouldStopActiveAbility(activeAbility);
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            m_ApplyHoldForce = InputIndex != -1;
            m_HoldForce = 0;

            m_ActiveForce = 0;
            for (int i = 0; i < m_ActiveSoftForceFrames.Length; ++i) {
                if (m_ActiveSoftForceFrames[i] == 0) {
                    break;
                }
                m_ActiveSoftForceFrames[i] = 0;
            }

            // If the jump has already been applied then it is a repeated jump.
            if (m_Jumping) {
                OnAirborneJump();
            } else {
                if (m_ImmediateJump) {
                    ApplyJumpForce();
                } else {
                    m_JumpEvent.WaitForEvent();
                }
            }

            if (m_ForceHold > 0) {
                if (m_Handler != null && InputIndex != -1) {
                    m_HoldInput = GenericObjectPool.Get<ActiveInputEvent>();
                    m_HoldInput.Initialize(ActiveInputEvent.Type.ButtonUp, InputNames[InputIndex], "OnJumpAbilityReleaseHold");
                    m_Handler.RegisterInputEvent(m_HoldInput);
                }
                EventHandler.RegisterEvent(m_GameObject, "OnJumpAbilityReleaseHold", OnReleaseHold);
            }

            // The character can do a repeated jump after the character is already in the air.
            if (!m_AirborneJumpRegistered) {
                if (m_Handler != null && InputIndex != -1) {
                    m_AirborneJumpInput = GenericObjectPool.Get<ActiveInputEvent>();
                    m_AirborneJumpInput.Initialize(ActiveInputEvent.Type.ButtonDown, InputNames[InputIndex], "OnJumpAbilityAirborneJump");
                    m_Handler.RegisterInputEvent(m_AirborneJumpInput);
                }
                EventHandler.RegisterEvent(m_GameObject, "OnJumpAbilityAirborneJump", OnPerformAirborneJump);
                m_AirborneJumpRegistered = true;
            }
            m_ImmediateJump = false;

            base.AbilityStarted();
        }

        /// <summary>
        /// The character has either landed or just left the ground.
        /// </summary>
        /// <param name="grounded">Is the character on the ground?</param>
        private void OnGrounded(bool grounded)
        {
            if (grounded) {
                if (IsActive) {
                    StopAbility(true);
                }
                m_AirborneJumpCount = 0;
                m_Jumping = false;

                // Remember the land time to prevent jumping more than the JumpReoccuranceDelay.
                // Add the deltaTime to prevent the ability from starting during the same frame. This can happen if the character is updated within FixedUpdate
                // but the input is updated within Update.
                m_LandTime = Time.time + Time.deltaTime;
                m_InAirTime = -1;

                // Unregister the jump input within OnGrounded so a repeated jump can be applied when fall is active.
                if (m_AirborneJumpInput != null) {
                    m_Handler.UnregisterInputEvent(m_AirborneJumpInput);
                    GenericObjectPool.Return(m_AirborneJumpInput);
                    m_AirborneJumpInput = null;
                }
                EventHandler.UnregisterEvent(m_GameObject, "OnJumpAbilityAirborneJump", OnPerformAirborneJump);
                m_AirborneJumpRegistered = false;
            } else {
                m_InAirTime = Time.time;
            }
        }

        /// <summary>
        /// The character should start the jump.
        /// </summary>
        private void ApplyJumpForce()
        {
            m_JumpEvent.CancelWaitForEvent();
            if (IsActive && !m_Jumping) {
                // A surface effect can optionally play when the character leaves the ground.
                if (m_JumpSurfaceImpact != null) {
                    SurfaceManager.SpawnEffect(m_CharacterLocomotion.GroundedRaycastHit, m_JumpSurfaceImpact, m_CharacterLocomotion.GravityDirection, m_CharacterLocomotion.TimeScale, m_GameObject);
                }
                // Do not set the Jumping variable because the ability should be active for at least one frame. If Jumping was set there is a chance
                // the ability could stop right away if the character jumps while moving down a slope.
                m_Jumping = true;

                m_JumpTime = Time.time;
                var force = m_Force;
                // Prevent the character from jumping as high when moving backwards or sideways.
                if (m_CharacterLocomotion.InputVector.y < 0) {
                    force *= Mathf.Lerp(1, m_BackwardsForceMultiplier, Mathf.Abs(m_CharacterLocomotion.InputVector.y));
                } else {
                    // The character's forward movement will contribute to a full jump force.
                    force *= Mathf.Lerp(1, m_SidewaysForceMultiplier, Mathf.Abs(m_CharacterLocomotion.InputVector.x) - Mathf.Abs(m_CharacterLocomotion.InputVector.y));
                }
                AddJumpForce(force, m_Frames);

                // Ensure the character is in the air after jumping.
                Scheduler.ScheduleFixed(Time.deltaTime * 10, EnsureAirborne); 
            }
        }

        /// <summary>
        /// Adds the force to the active force.
        /// </summary>
        /// <param name="force">The amount of force that should be added.</param>
        /// <param name="frames">The number of fames that the force should be applied in.</param>
        private void AddJumpForce(float force, int frames = 1)
        {
            if (frames == 1) {
                m_ActiveForce += force;
            } else {
                frames = Mathf.Clamp(frames, 1, m_CharacterLocomotion.MaxSoftForceFrames);
                AddJumpForce(force / frames);
                for (int i = 0; i < (Mathf.RoundToInt(frames) - 1); i++) {
                    m_ActiveSoftForceFrames[i] += (force / frames);
                }
            }
        }

        /// <summary>
        /// After jumping the character should be in the air. If the character is not in the air then another object prevented the character from jumping and the
        /// jump ability should be stopped.
        /// </summary>
        private void EnsureAirborne()
        {
            if (!m_CharacterLocomotion.Grounded) {
                return;
            }

            StopAbility(true);
        }

        /// <summary>
        /// The user is no longer holding the jump button down.
        /// </summary>
        private void OnReleaseHold()
        {
            m_ApplyHoldForce = false;
        }

        /// <summary>
        /// Start the ability to perform an airborne jump.
        /// </summary>
        private void OnPerformAirborneJump()
        {
            if (IsActive) {
                OnAirborneJump();
            } else {
                StartAbility();
            }
        }

        /// <summary>
        /// The ability should perform a airborne jump.
        /// </summary>
        private void OnAirborneJump()
        {
            if (m_MaxAirborneJumpCount != -1 && m_AirborneJumpCount >= m_MaxAirborneJumpCount) {
                return;
            }

            // Reset the accumulated gravity to allow for a full airborne jump.
            m_CharacterLocomotion.GravityAccumulation = 0;
            AddJumpForce(m_AirborneJumpForce, m_AirborneJumpFrames);
            m_AirborneJumpCount++;
            m_JumpTime = Time.time;
            // The repeated jump may be applied just as the fall ability is about to start. Prevent the jump ability from stopping immediately after starting a repeated jump.
            m_AirborneJumpApplied = true;
            m_AirborneJumpAudioClipSet.PlayAudioClip(m_GameObject);
            SetAbilityIntDataParameter(AbilityIntData);
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // Set the Float Data parameter for the blend tree.
            if (m_Jumping) {
                SetAbilityFloatDataParameter(m_CharacterLocomotion.Velocity.y);
            }
        }

        /// <summary>
        /// Allows for the Jump ability to add an extra force.
        /// </summary>
        public override void UpdatePosition()
        {
            if (!m_Jumping) {
                return;
            }

            if (m_AirborneJumpApplied) {
                m_AirborneJumpApplied = false;
            } 

            // Allow a force and damping to be applied when the input button is held down.
            if (m_ForceHold > 0 && m_ApplyHoldForce) {
                m_HoldForce += m_ForceHold;
                m_HoldForce /= (1 + m_ForceDampingHold);
                AddJumpForce(m_HoldForce, 1);
            }

            var desiredMovement = m_CharacterLocomotion.LocalDesiredMovement;
            desiredMovement.y += m_ActiveForce * (m_CharacterLocomotion.TimeScale * Time.timeScale);
            m_CharacterLocomotion.LocalDesiredMovement = desiredMovement;

            // Apply a soft force (forces applied over several frames).
            if (m_ActiveSoftForceFrames[0] != 0) {
                AddJumpForce(m_ActiveSoftForceFrames[0], 1);
                for (int i = 0; i < m_ActiveSoftForceFrames.Length; ++i) {
                    m_ActiveSoftForceFrames[i] = (i < m_ActiveSoftForceFrames.Length - 1) ? m_ActiveSoftForceFrames[i + 1] : 0;
                    if (m_ActiveSoftForceFrames[i] == 0) {
                        break;
                    }
                }
            }

            // Dampen external forces.
            m_ActiveForce /= (1 + m_ForceDamping * (m_CharacterLocomotion.TimeScale * Time.timeScale));
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <param name="force">Should the ability be force stopped?</param>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility(bool force)
        {
            if (force) { return true; }

            // The Jump ability is done if the velocity is less than a the specified value.
            if (m_Jumping && m_InAirTime >= m_JumpTime && m_CharacterLocomotion.LocalVelocity.y <= m_VerticalVelocityStopThreshold && !m_AirborneJumpApplied) {
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

            m_Jumping = false;

            // Unregister for the ability input events.
            if (m_HoldInput != null) {
                m_Handler.UnregisterInputEvent(m_HoldInput);
                GenericObjectPool.Return(m_HoldInput);
            }
            EventHandler.UnregisterEvent(m_GameObject, "OnJumpAbilityReleaseHold", OnReleaseHold);
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
        /// The GameObject has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            m_JumpEvent.RegisterUnregisterAnimationEvent(false, m_GameObject, "OnAnimatorJump", ApplyJumpForce);
            EventHandler.UnregisterEvent(m_GameObject, "OnAnimatorJump", ApplyJumpForce);
        }
    }
}