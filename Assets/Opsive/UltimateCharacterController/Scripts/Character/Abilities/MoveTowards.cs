/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character.Abilities.AI;
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Moves the character to the specified start location. This ability will be called manually by the controller and should not be started by the user.
    /// </summary>
    [DefaultStartType(AbilityStartType.Manual)]
    [DefaultAllowPositionalInput(false)]
    [DefaultAllowRotationalInput(false)]
    [DefaultState("MoveTowards")]
    public class MoveTowards : Ability
    {
        [Tooltip("The multiplier to apply to the input vector. Allows the character to move towards the destination faster.")]
        [SerializeField] protected float m_InputMultiplier = 1;
        [Tooltip("The amount of time it takes that the character has to be stuck before teleporting the character to the start location.")]
        [SerializeField] protected float m_InactiveTimeout = 1;
        [Tooltip("Specifies the maximum distance that the target position can move before the ability stops.")]
        [SerializeField] protected float m_MovingTargetDistanceTimeout = float.MaxValue;
        [Tooltip("Should the character be teleported after the timeout or max moving distance has elapsed? If false the character will stop.")]
        [SerializeField] protected bool m_TeleportOnEarlyStop = true;
        [Tooltip("Should the OnEnableGameplayInpt event be sent to disable the input when the ability is active?")]
        [SerializeField] protected bool m_DisableGameplayInput;
        [Tooltip("The location that the Move Towards ability should move towards if the ability is not started by another ability.")]
        [SerializeField] protected MoveTowardsLocation m_IndependentMoveTowardsLocation;

        public float InputMultiplier { get => m_InputMultiplier; set => m_InputMultiplier = value; }
        public float InactiveTimeout { get => m_InactiveTimeout; set => m_InactiveTimeout = value; } 
        public float MovingTargetDistanceTimeout { get => m_MovingTargetDistanceTimeout; set => m_MovingTargetDistanceTimeout = value; }
        public bool TeleportOnEarlyStop { get => m_TeleportOnEarlyStop; set => m_TeleportOnEarlyStop = value; }
        public bool DisableGameplayInput { get => m_DisableGameplayInput; set => m_DisableGameplayInput = value; }
        [Shared.Utility.NonSerialized] public MoveTowardsLocation IndependentMoveTowardsLocation { get => m_IndependentMoveTowardsLocation; set => m_IndependentMoveTowardsLocation = value; }

        public override bool IsConcurrent { get => true; }
        public override bool ImmediateStartItemVerifier { get => true; }

        private MoveTowardsLocation m_MoveTowardsLocation;
        private Ability m_OnArriveAbility;

        private PathfindingMovement m_PathfindingMovement;
        private SpeedChange[] m_SpeedChangeAbilities;

        private Vector3 m_StartMoveTowardsPosition;
        private float m_MovementMultiplier;
        private Vector3 m_TargetDirection;
        private bool m_Arrived;
        private int m_PrecisionStartWaitFrame;

        private ScheduledEventBase m_ForceStartEvent;

        public MoveTowardsLocation StartLocation { get => m_MoveTowardsLocation; }
        public Ability OnArriveAbility { get => m_OnArriveAbility; }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            m_PathfindingMovement = m_CharacterLocomotion.GetAbility<PathfindingMovement>();
            if (m_PathfindingMovement != null && m_PathfindingMovement.Index > Index) {
                Debug.LogWarning("Warning: The Pathfinding Movement ability should be ordered above the Move Towards ability.");
            }
            m_SpeedChangeAbilities = m_CharacterLocomotion.GetAbilities<SpeedChange>();
        }

        /// <summary>
        /// Moves the character to the specified position. Will create a MoveTowardsLocation if one is not already created.
        /// </summary>
        /// <param name="position">The position to move towards.</param>
        public void MoveTowardsLocation(Vector3 position)
        {
            InitializeMoveTowardsLocation();
            m_IndependentMoveTowardsLocation.transform.position = position;
            m_IndependentMoveTowardsLocation.Angle = 360; // Any arriving location is valid.
            // The position can be updated while the ability is active.
            if (IsActive) {
                if (m_PathfindingMovement != null && m_PathfindingMovement.IsActive) {
                    m_PathfindingMovement.SetDestination(position);
                }
            } else {
                StartAbility();
            }
        }

        /// <summary>
        /// Moves the character to the specified location. Will create a MoveTowardsLocation if one is not already created.
        /// </summary>
        /// <param name="position">The position to move towards.</param>
        /// <param name="rotation">The rotation to move towards.</param>
        public void MoveTowardsLocation(Vector3 position, Quaternion rotation)
        {
            InitializeMoveTowardsLocation();
            m_IndependentMoveTowardsLocation.transform.SetPositionAndRotation(position, rotation);
            // The position can be updated while the ability is active.
            if (IsActive) {
                if (m_PathfindingMovement != null && m_PathfindingMovement.IsActive) {
                    m_PathfindingMovement.SetDestination(position);
                }
            } else {
                StartAbility();
            }
        }

        /// <summary>
        /// Initialize a new MoveTowardsLocation.
        /// </summary>
        private void InitializeMoveTowardsLocation()
        {
            if (m_IndependentMoveTowardsLocation != null) {
                return;
            }
            m_IndependentMoveTowardsLocation = new GameObject("MoveTowardsLocation").AddComponent<MoveTowardsLocation>();
            m_IndependentMoveTowardsLocation.Offset = Vector3.zero;
            m_IndependentMoveTowardsLocation.YawOffset = 0;
            m_IndependentMoveTowardsLocation.PrecisionStart = false;
            m_IndependentMoveTowardsLocation.Distance = 1;
        }

        /// <summary>
        /// Starts moving to the specified start location.
        /// </summary>
        /// <param name="startLocations">The locations the character can move towards. If multiple locations are possible then the closest valid location will be used.</param>
        /// <param name="onArriveAbility">The ability that should be started as soon as the character arrives at the location.</param>
        /// <returns>True if the MoveTowards ability is started.</returns>
        public bool StartMoving(MoveTowardsLocation[] startLocations, Ability onArriveAbility)
        {
            // MoveTowards doesn't need to start if there is no start location.
            if (startLocations == null || startLocations.Length == 0) {
                return false;
            }

            // The arrive ability must exist and be unique. If the ability is already set then StartMoving may have been triggered because the arrive ability
            // should start.
            if (onArriveAbility == null || onArriveAbility == m_OnArriveAbility) {
                return false;
            }

            // No reason to start if the character is already in a valid start location.
            for (int i = 0; i < startLocations.Length; ++i) {
                if (startLocations[i].IsPositionValid(m_Transform.position, m_Transform.rotation, m_CharacterLocomotion.Grounded) && startLocations[i].IsRotationValid(m_Transform.rotation)) {
                    return false;
                }
            }

            // The character needs to move - start the ability.
            m_OnArriveAbility = onArriveAbility;
            if (m_OnArriveAbility.Index < Index) {
                Debug.LogWarning($"Warning: {m_OnArriveAbility.GetType().Name} has a higher priority then the MoveTowards ability. This will cause unintended behavior.");
            }

            m_MoveTowardsLocation = GetClosestStartLocation(startLocations);

            StartAbility();

            // MoveTowards may be starting when all of the inputs are being checked. If it has a lower index then the update loop won't run initially
            // which will prevent the TargetDirection from having a valid value. Run the Update loop immediately so TargetDirection is correct.
            if (Index < onArriveAbility.Index) {
                Update();
            }

            return true;
        }

        /// <summary>
        /// Returns the closest start location out of the possible MoveTowardsLocations.
        /// </summary>
        /// <param name="startLocations">The locations the character can move towards.</param>
        /// <returns>The best location out of the possible MoveTowardsLocations.</returns>
        private MoveTowardsLocation GetClosestStartLocation(MoveTowardsLocation[] startLocations)
        {
            // If only one location is available then it is the closest.
            if (startLocations.Length == 1) {
                return startLocations[0];
            }

            // Multiple locations are available. Choose the closest location.
            MoveTowardsLocation startLocation = null;
            var closestDistance = float.MaxValue;
            float distance;
            for (int i = 0; i < startLocations.Length; ++i) {
                if ((distance = startLocations[i].GetTargetDirection(m_Transform.position, m_Transform.rotation).sqrMagnitude) < closestDistance) {
                    closestDistance = distance;
                    startLocation = startLocations[i];
                }
            }

            return startLocation;
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

            if (m_MoveTowardsLocation == null && m_IndependentMoveTowardsLocation == null) {
                return false;
            }

            // Don't start if the character has already arrived.
            var moveTowardsLocation = (m_MoveTowardsLocation != null ? m_MoveTowardsLocation : m_IndependentMoveTowardsLocation);
            if (moveTowardsLocation.IsRotationValid(m_Transform.rotation) && moveTowardsLocation.IsPositionValid(m_Transform.position, m_Transform.rotation, m_CharacterLocomotion.Grounded)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            if (m_OnArriveAbility != null) {
                m_DetectHorizontalCollisions = m_OnArriveAbility.DetectHorizontalCollisions;
                m_AllowEquippedSlotsMask = m_OnArriveAbility.AllowEquippedSlotsMask;
                m_OnArriveAbility.AbilityMessageCanStart = false;
            }

            base.AbilityStarted();
            m_Arrived = false;
            if (m_DisableGameplayInput) {
                EventHandler.ExecuteEvent(m_GameObject, "OnEnableGameplayInput", false);
            }

            // The MoveTowardsLocation may already be set by the starting ability within StartMoving.
            if (m_MoveTowardsLocation == null) {
                m_MoveTowardsLocation = m_IndependentMoveTowardsLocation;
            }
            m_StartMoveTowardsPosition = m_MoveTowardsLocation.TargetPosition;
            // The movement speed will depend on the current speed the character is moving.
            m_MovementMultiplier = m_MoveTowardsLocation.MovementMultiplier;
            if (m_SpeedChangeAbilities != null) {
                for (int i = 0; i < m_SpeedChangeAbilities.Length; ++i) {
                    if (m_SpeedChangeAbilities[i].IsActive) {
                        m_MovementMultiplier = m_SpeedChangeAbilities[i].SpeedChangeMultiplier;
                        break;
                    }
                }
            }
            // Use the pathfinding ability if the destination is a valid pathfinding destination.
            if (m_PathfindingMovement != null && m_PathfindingMovement.Index < Index) {
                m_PathfindingMovement.SetDestination(m_MoveTowardsLocation.TargetPosition);
            }

            // Force independent look so the ability will have complete control over the rotation.
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterForceIndependentLook", true);
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            // ItemEquipVerifier and EquipUnequip should never be blocked.
            if (startingAbility is ItemEquipVerifier || startingAbility is Items.EquipUnequip) {
                return false;
            }

            // Block the ability if it has a lower priority (higher index) then the MoveTowards ability. ItemAbilities have a different priority list.
            if (startingAbility.Index > Index || startingAbility is StoredInputAbilityBase) {
                return true;
            }

            // The arrive ability can determine if an ability should be blocked.
            if (m_OnArriveAbility != null) {
                return m_OnArriveAbility.ShouldBlockAbilityStart(startingAbility);
            }
            return false;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            if (activeAbility is StoredInputAbilityBase) {
                return true;
            }

            // The arrive ability can determine if an ability should be stopped.
            if (m_OnArriveAbility != null) {
                return m_OnArriveAbility.ShouldStopActiveAbility(activeAbility);
            }
            return false;
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // Stop moving if the target has moved too far away.
            if (Vector3.Distance(m_StartMoveTowardsPosition, m_MoveTowardsLocation.TargetPosition) > m_MovingTargetDistanceTimeout) {
                MoveTimeout();
                return;
            }

            // Keep the MoveTowards ability active until the character has arrived at the destination and the ItemEquipVerifier ability isn't active.
            // This will prevent the character from sliding when ItemEquipVerifier is active and MoveTowards is not active.
            if (m_Arrived && (m_CharacterLocomotion.ItemEquipVerifierAbility == null || !m_CharacterLocomotion.ItemEquipVerifierAbility.IsActive)) {
                if (!m_MoveTowardsLocation.PrecisionStart || Time.frameCount > m_PrecisionStartWaitFrame + 1) {
                    StopAbility();
                    return;
                } else {
                    // After the character is no longer in transition the arrive ability can start. This will ensure the character always starts in the correct location.
                    // For some abilities it doesn't matter if the character is in a precise position and in that case the precision start field can be disabled.
                    if (m_MoveTowardsLocation.PrecisionStart &&
                        (m_AnimatorMonitor != null && (m_AnimatorMonitor.IsInTransition(0) || m_AnimatorMonitor.Moving))) {
                        m_PrecisionStartWaitFrame = Time.frameCount + 1;
                    }
                }
            }

            // The input values should move towards the target.
            var arrived = m_MoveTowardsLocation.IsRotationValid(m_Transform.rotation);
            if (m_PathfindingMovement == null || !m_PathfindingMovement.IsActive || m_PathfindingMovement.HasArrived) {
                m_TargetDirection = m_MoveTowardsLocation.GetTargetDirection(m_Transform.position, m_Transform.rotation);
                if (!m_MoveTowardsLocation.IsPositionValid(m_Transform.position, m_Transform.rotation, m_CharacterLocomotion.Grounded)) {
                    m_CharacterLocomotion.InputVector = GetInputVector(m_TargetDirection);
                    arrived = false;
                } else if (!m_MoveTowardsLocation.PrecisionStart && (m_PathfindingMovement == null || !m_PathfindingMovement.IsActive) &&
                                                                (m_OnArriveAbility == null || m_OnArriveAbility.AllowPositionalInput)) {
                    m_CharacterLocomotion.InputVector = m_CharacterLocomotion.RawInputVector;
                }
            } else {
                // The character hasn't arrived if the pathfinding movement is active.
                arrived = false;
            }

            if (arrived && !m_Arrived) {
                m_Arrived = true;
                // The character should completely stop moving when they have arrived when using a precision start. Return early to allow the animator
                // to start transitioning to the next frame.
                if (m_MoveTowardsLocation.PrecisionStart) {
                    m_CharacterLocomotion.ResetRotationPosition();
                    m_PrecisionStartWaitFrame = Time.frameCount + 1;
                    return;
                }
            }

            // If the character isn't making any progress teleport them to the starting location and start the arrive ability.
            if (!m_Arrived) {
                if (m_CharacterLocomotion.Velocity.sqrMagnitude <= 0.0001f && m_CharacterLocomotion.Torque.eulerAngles.sqrMagnitude <= 0.0001f) {
                    if (m_ForceStartEvent == null) {
                        m_ForceStartEvent = Scheduler.Schedule(m_InactiveTimeout, MoveTimeout);
                    }
                } else if (m_ForceStartEvent != null) {
                    Scheduler.Cancel(m_ForceStartEvent);
                    m_ForceStartEvent = null;
                }
            }
        }

        /// <summary>
        /// Returns the rotation that the character should rotate towards.
        /// </summary>
        /// <returns>The rotation that the character should rotate towards.</returns>
        protected virtual Quaternion GetTargetRotation()
        {
            return Quaternion.LookRotation(m_MoveTowardsLocation.TargetRotation * Vector3.forward, m_Transform.up);
        }

        /// <summary>
        /// Returns the input vector that the character should move with.
        /// </summary>
        /// <param name="direction">The direction that the character should move towards.</param>
        /// <returns>The input vector that the character should move with.</returns>
        protected virtual Vector2 GetInputVector(Vector3 direction)
        {
            var inputVector = Vector2.zero;
            inputVector.x = direction.x;
            inputVector.y = direction.z;
            return m_InputMultiplier * m_MovementMultiplier * inputVector.normalized;
        }

        /// <summary>
        /// Update the controller's rotation values.
        /// </summary>
        public override void ApplyRotation()
        {
            if ((m_PathfindingMovement != null && m_PathfindingMovement.IsActive && !m_PathfindingMovement.HasArrived) || m_MoveTowardsLocation.IsRotationValid(m_Transform.rotation)) {
                return;
            }

            var rotation = (GetTargetRotation() * Quaternion.Inverse(m_Transform.rotation)).eulerAngles;
            rotation = new Vector3(MathUtility.ClampInnerAngle(rotation.x), MathUtility.ClampInnerAngle(rotation.y), MathUtility.ClampInnerAngle(rotation.z));
            m_CharacterLocomotion.DesiredRotation = Quaternion.RotateTowards(Quaternion.identity, Quaternion.Euler(rotation), m_CharacterLocomotion.MotorRotationSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale);
        }

        /// <summary>
        /// Update the controller's position values.
        /// </summary>
        public override void ApplyPosition()
        {
            if (m_PathfindingMovement != null && m_PathfindingMovement.IsActive) {
                return;
            }

            // Prevent the character from jittering back and forth to land precisely on the target.
            var moveDirection = m_Rigidbody.InverseTransformDirection(m_CharacterLocomotion.DesiredMovement);
            if (Mathf.Abs(moveDirection.x) > Mathf.Abs(m_TargetDirection.x)) {
                moveDirection.x = m_TargetDirection.x;
            }
            if (Mathf.Abs(moveDirection.z) > Mathf.Abs(m_TargetDirection.z)) {
                moveDirection.z = m_TargetDirection.z;
            }
            m_CharacterLocomotion.DesiredMovement = m_Rigidbody.TransformDirection(moveDirection);
        }

        /// <summary>
        /// The character has not moved after the timeout duration. Teleport or stop the ability.
        /// </summary>
        private void MoveTimeout()
        {
            if (!m_TeleportOnEarlyStop || !m_GameObject.activeInHierarchy) {
                StopAbility(true);
                return;
            }

            // Teleport the character.
            var onArriveAbility = m_OnArriveAbility;
            var position = m_MoveTowardsLocation.TargetPosition;
            var rotation = m_MoveTowardsLocation.TargetRotation;
            // Stop the ability before setting the location to allow the ability to reset the parameters (such as vertical/horizontal collision detection).
            StopAbility(true);
            m_CharacterLocomotion.SetPositionAndRotation(position, rotation, true, false);
            // The character is in location. Start the arrive ability.
            m_CharacterLocomotion.TryStartAbility(onArriveAbility, true, true);
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            m_MoveTowardsLocation = null;
            if (force) {
                m_OnArriveAbility = null;
            }
            if (m_ForceStartEvent != null) {
                Scheduler.Cancel(m_ForceStartEvent);
                m_ForceStartEvent = null;
            }
            if (m_DisableGameplayInput) {
                EventHandler.ExecuteEvent(m_GameObject, "OnEnableGameplayInput", true);
            }
            if (m_PathfindingMovement != null && m_PathfindingMovement.IsActive) {
                m_PathfindingMovement.StopAbility(true);
            }

            // Reset the force independet look parameter set within StartAbility.
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterForceIndependentLook", false);

            // Start the OnArriveAbility after MoveTowards has stopped to prevent MoveTowards from affecting the arrive ability.
            if (m_OnArriveAbility != null) {
                m_CharacterLocomotion.TryStartAbility(m_OnArriveAbility, true, true);
                m_OnArriveAbility = null;
            }
        }
    }
}