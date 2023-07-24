/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.AI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using UnityEngine;
    using UnityEngine.AI;

    /// <summary>
    /// Moves the character according to the NavMeshAgent desired velocity.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshAgentMovement : PathfindingMovement
    {
        /// <summary>
        /// Specifies if the rotation should be overridden.
        /// </summary>
        public enum RotationOverrideMode
        {
            NoOverride, // Does not override the rotation. Uses the NavMesh updateRotation property.
            NavMesh,    // Forces the rotation according to the NavMesh path.
            Character   // Forces the rotation according to the character's rotation.
        }

        [Tooltip("Should the NavMesh automatically be enabled when SetDestination is called?")]
        [SerializeField] protected bool m_AutoEnable = true;
        [Tooltip("Specifies if the rotation should be overridden.")]
        [SerializeField] protected RotationOverrideMode m_RotationOverride;
        [Tooltip("The agent has arrived at the destination when the remaining distance is less than the arrived distance.")]
        [SerializeField] protected float m_ArrivedDistance = 0.2f;
        [Tooltip("The name of the manual offmesh link that the character can traverse across.")]
        [SerializeField] protected string m_ManualOffMeshLinkName = "Jump";
        [Tooltip("Should the jump ability be started on the manual offmesh link?")]
        [SerializeField] protected bool m_JumpAcrossManualOffMeshLink = true;

        public bool AutoEnable { get => m_AutoEnable; set => m_AutoEnable = value; }
        public RotationOverrideMode RotationOverride { get => m_RotationOverride; set => m_RotationOverride = value; }
        public float ArrivedDistance { get => m_ArrivedDistance; set => m_ArrivedDistance = value; }
        public string ManualOffMeshLinkName { get => m_ManualOffMeshLinkName; set => m_ManualOffMeshLinkName = value; }
        public bool JumpAcrossManualOffMeshLink { get => m_JumpAcrossManualOffMeshLink; set => m_JumpAcrossManualOffMeshLink = value; }

        private NavMeshAgent m_NavMeshAgent;
        private Jump m_JumpAbility;
        private Fall m_FallAbility;

        private bool m_PrevEnabled = true;
        private Vector2 m_InputVector;
        private Vector3 m_DeltaRotation;
        private bool m_UpdateRotation;
        private int m_ManualOffMeshLinkIndex;
        private Quaternion m_DestinationRotation = Quaternion.identity;

        public override Vector2 InputVector { get { return m_InputVector; } }
        public override Vector3 DeltaRotation { get { return m_DeltaRotation; } }
        public override bool HasArrived { get { return !m_NavMeshAgent.pathPending && m_NavMeshAgent.remainingDistance <= m_ArrivedDistance; } }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_NavMeshAgent.autoTraverseOffMeshLink = false;
            m_NavMeshAgent.updatePosition = false;
            m_ManualOffMeshLinkIndex = NavMesh.GetAreaFromName(m_ManualOffMeshLinkName);

            m_JumpAbility = m_CharacterLocomotion.GetAbility<Jump>();
            m_FallAbility = m_CharacterLocomotion.GetAbility<Fall>();

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);

            if (!Enabled) {
                m_NavMeshAgent.enabled = false;
            }
        }

        /// <summary>
        /// Sets the destination of the pathfinding agent.
        /// </summary>
        /// <param name="target">The position to move towards.</param>
        /// <returns>True if the destination was set.</returns>
        public override bool SetDestination(Vector3 target)
        {
            if (!m_AutoEnable && !Enabled) {
                return false;
            }

            m_DestinationRotation = Quaternion.identity;

            // Set the new destination if the ability is already active.
            if (m_NavMeshAgent.hasPath && IsActive) {
                return m_NavMeshAgent.SetDestination(target);
            }

            // The NavMeshAgent must be enabled in order to set the destination.
            m_PrevEnabled = Enabled;
            Enabled = true;
            // Move towards the destination.
            if (m_NavMeshAgent.isOnNavMesh && m_NavMeshAgent.SetDestination(target)) {
                StartAbility();
                return true;
            }
            Enabled = m_PrevEnabled;
            return false;
        }

        /// <summary>
        /// Returns the destination of the pathfinding agent.
        /// </summary>
        /// <returns>The destination of the pathfinding agent.</returns>
        public override Vector3 GetDestination()
        {
            return m_NavMeshAgent.destination;
        }

        /// <summary>
        /// Teleports the agent to the specified position.
        /// </summary>
        /// <param name="position">The position that the agent should teleport to.</param>
        public override void Teleport(Vector3 position)
        {
            m_NavMeshAgent.Warp(position);
        }

        /// <summary>
        /// Sets the rotation of the agent after they have arrived at the destination.
        /// </summary>
        /// <param name="rotation">The destination rotation.</param>
        public override void SetDestinationRotation(Quaternion rotation)
        {
            m_DestinationRotation = rotation;
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            if (!base.CanStartAbility()) {
                return false;
            }

            return m_NavMeshAgent.isOnNavMesh;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_NavMeshAgent.isStopped = false;
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            m_InputVector = Vector2.zero;
            var lookRotation = m_Rigidbody.rotation;
            var updateInput = true;
            if (m_NavMeshAgent.isOnOffMeshLink && UpdateOffMeshLink()) {
                updateInput = false;
            }

            if (updateInput && m_NavMeshAgent.hasPath && !m_NavMeshAgent.isStopped) {
                var direction = (m_NavMeshAgent.pathPending || m_NavMeshAgent.desiredVelocity.sqrMagnitude < 0.01f) ? m_NavMeshAgent.velocity : m_NavMeshAgent.desiredVelocity;
                // Only move if a path exists.
                if (m_NavMeshAgent.remainingDistance > 0.01f) {
                    // A path can exist but the velocity returns 0 (??). Move in the direction of the destination.
                    if (direction.sqrMagnitude == 0) {
                        direction = m_NavMeshAgent.destination - m_Rigidbody.position;
                    }
                    Vector3 velocity;
                    if (direction.sqrMagnitude > 0 && 
                        ((m_NavMeshAgent.updateRotation && m_RotationOverride == RotationOverrideMode.NoOverride) || m_RotationOverride == RotationOverrideMode.NavMesh)) {
                        lookRotation = Quaternion.LookRotation(direction.normalized, m_CharacterLocomotion.Up);
                        // The normalized velocity should be relative to the target rotation.
                        velocity = Quaternion.Inverse(lookRotation) * direction.normalized;
                    } else {
                        velocity = m_Rigidbody.InverseTransformDirection(direction);
                    }
                    // Only normalize if the magnitude is greater than 1. This will allow the character to walk.
                    if (velocity.sqrMagnitude > 1) {
                        velocity.Normalize();
                    }
                    m_InputVector.x = velocity.x;
                    m_InputVector.y = velocity.z;
                } else if (m_DestinationRotation != Quaternion.identity) {
                    lookRotation = m_DestinationRotation;
                }
            }
            var rotation = lookRotation * Quaternion.Inverse(m_Rigidbody.rotation);
            m_DeltaRotation.y = Utility.MathUtility.ClampInnerAngle(rotation.eulerAngles.y);

            base.Update();
        }

        /// <summary>
        /// Ensure the move direction is valid.
        /// </summary>
        public override void ApplyPosition()
        {
            if (m_NavMeshAgent.remainingDistance <= m_CharacterLocomotion.DesiredMovement.magnitude) {
                // Prevent the character from jittering back and forth to land precisely on the target.
                var direction = m_Rigidbody.InverseTransformPoint(m_NavMeshAgent.destination);
                var desiredMovement = m_Rigidbody.InverseTransformDirection(m_CharacterLocomotion.DesiredMovement);
                if (Mathf.Abs(desiredMovement.x) > Mathf.Abs(direction.x)) {
                    desiredMovement.x = direction.x;
                }
                if (Mathf.Abs(desiredMovement.z) > Mathf.Abs(direction.z)) {
                    desiredMovement.z = direction.z;
                }
                m_CharacterLocomotion.DesiredMovement = m_Rigidbody.TransformDirection(desiredMovement);
            }

            // NavMeshAgents require the transforms to be synced.
            if (!Physics.autoSyncTransforms) {
                Physics.SyncTransforms();
            }
        }

        /// <summary>
        /// Synchronizes the NavMeshAgent with the current position.
        /// </summary>
        public override void LateUpdate()
        {
            base.LateUpdate();

            m_NavMeshAgent.nextPosition = m_Rigidbody.position;
        }

        /// <summary>
        /// Updates the velocity and look rotation using the off mesh link.
        /// </summary>
        /// <returns>True if the off mesh link was handled.</returns>
        protected virtual bool UpdateOffMeshLink()
        {
            if (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross ||
                (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeManual && m_NavMeshAgent.currentOffMeshLinkData.offMeshLink.area == m_ManualOffMeshLinkIndex)) {
                // Ignore the y difference when determining a look direction and velocity.
                // This will give XZ distances a greater impact when normalized.
                var direction = m_NavMeshAgent.currentOffMeshLinkData.endPos - m_Rigidbody.position;
                direction.y = 0;
                if (direction.sqrMagnitude > 0.1f || m_CharacterLocomotion.Grounded) {
                    var nextPositionDirection = m_Rigidbody.InverseTransformPoint(m_NavMeshAgent.currentOffMeshLinkData.endPos);
                    nextPositionDirection.y = 0;
                    nextPositionDirection.Normalize();

                    m_InputVector.x = nextPositionDirection.x;
                    m_InputVector.y = nextPositionDirection.z;
                }

                // Jump if the agent hasn't jumped yet.
                if (m_JumpAbility != null && (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross ||
                    (m_JumpAcrossManualOffMeshLink && m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeManual && 
                        m_NavMeshAgent.currentOffMeshLinkData.offMeshLink.area == m_ManualOffMeshLinkIndex))) {
                    if (!m_JumpAbility.IsActive && (m_FallAbility == null || !m_FallAbility.IsActive)) {
                        m_CharacterLocomotion.TryStartAbility(m_JumpAbility);
                    }
                }
                return true;
            } else if (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeDropDown && m_CharacterLocomotion.Grounded) {
                m_NavMeshAgent.CompleteOffMeshLink();
            }
            return false;
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <param name="force">Should the ability be force stopped?</param>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility(bool force)
        {
            if (!Enabled || force) {
                return true;
            }

            if (!base.CanStopAbility(force)) {
                return false;
            }

            return m_NavMeshAgent.hasPath && !m_NavMeshAgent.pathPending && m_NavMeshAgent.remainingDistance <= m_ArrivedDistance;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            if (m_NavMeshAgent.hasPath && m_NavMeshAgent.pathPending) {
                m_NavMeshAgent.isStopped = true;
            }

            if (!m_PrevEnabled) {
                Enabled = false;
            }
        }

        /// <summary>
        /// The character has changed grounded state. 
        /// </summary>
        /// <param name="grounded">Is the character on the ground?</param>
        protected virtual void OnGrounded(bool grounded)
        {
            if (grounded && m_NavMeshAgent.enabled) {
                // The agent is no longer on an off mesh link if they just landed.
                if (m_NavMeshAgent.isOnOffMeshLink && (m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeDropDown ||
                                                       m_NavMeshAgent.currentOffMeshLinkData.linkType == OffMeshLinkType.LinkTypeJumpAcross)) {
                    m_NavMeshAgent.CompleteOffMeshLink();
                }
                // Warp the NavMeshAgent just in case the navmesh position doesn't match the transform position.
                var destination = m_NavMeshAgent.destination;
                m_NavMeshAgent.Warp(m_Rigidbody.position);
                // Warp can change the destination so make sure that doesn't happen.
                if (m_NavMeshAgent.destination != destination) {
                    m_NavMeshAgent.SetDestination(destination);
                }
            }
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            m_UpdateRotation = m_NavMeshAgent.updateRotation;
            m_NavMeshAgent.updateRotation = false;
        }

        /// <summary>
        /// The character has respawned. Start moving again.
        /// </summary>
        private void OnRespawn()
        {
            // Reset the NavMeshAgent to the new position.
            m_NavMeshAgent.Warp(m_Rigidbody.position);
            if (m_NavMeshAgent.isOnOffMeshLink) {
                m_NavMeshAgent.ActivateCurrentOffMeshLink(false);
            }
            m_NavMeshAgent.updateRotation = m_UpdateRotation;
        }

        /// <summary>
        /// Called when the ability is enabled or disabled.
        /// </summary>
        /// <param name="enabled">Is the ability enabled?</param>
        protected override void SetEnabled(bool enabled)
        {
            if (m_NavMeshAgent != null) {
                m_NavMeshAgent.enabled = enabled;
            }
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterGrounded", OnGrounded);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }
    }
}