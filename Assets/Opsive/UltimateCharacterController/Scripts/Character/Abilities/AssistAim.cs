/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Items.Actions;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// This ability is used to move and rotate the character towards a target when attacking/using an item.
    /// </summary>
    [DefaultState("AssistAim")]
    [Serializable]
    [Shared.StateSystem.AddState("Aim", "7d9977608fa141a4da838a951f95f825")]
    [Shared.StateSystem.AddState("Use", "077e49e285060a14c83cf6f65d01d266")]
    public class AssistAim : Ability
    {
        [Tooltip("Should the ability auto select the target?")]
        [SerializeField] protected bool m_AutoSelectTarget = true;
        [Tooltip("The radius around the player used to search targets.")]
        [SerializeField] protected float m_Radius = 10;
        [Tooltip("The angle offset from the aim direction to find targets, within the radius.")]
        [SerializeField] protected float m_Angle = 90;
        [Tooltip("Is the target required to be within sight?")]
        [SerializeField] protected bool m_RequireLineOfSight = true;
        [Tooltip("The offset from the characters center.")]
        [SerializeField] protected Vector3 m_CenterOffset;
        [Tooltip("Evaluated between time 0 and 1, defines how the distance affects the selected target.")]
        [SerializeField] protected AnimationCurve m_DistanceInfluence = AnimationCurve.EaseInOut(0, 50, 1, 1);
        [Tooltip("Evaluated between time 0 and 1, defines how the angle difference affects the selected target.")]
        [SerializeField] protected AnimationCurve m_AngleInfluence = AnimationCurve.EaseInOut(0, 100, 1, 1);
        [Tooltip("The stickiness angle keeps the current target even if it goes out the default angle.")]
        [SerializeField] protected float m_StickinessAngle = 180;
        [Tooltip("The stickiness score is applied on the current target when no input direction is being pressed.")]
        [SerializeField] protected float m_StickinessScore = 10;

        [Tooltip("If the target is a humanoid should a bone from the humanoid be targeted?")]
        [SerializeField] protected bool m_TargetHumanoidBone;
        [Tooltip("Specifies which bone to target if targeting a humanoid bone.")]
        [SerializeField] protected HumanBodyBones m_HumanoidBoneTarget = HumanBodyBones.Chest;

        [Tooltip("Should the camera focus on the target? If false it will look at the point between the target and character.")]
        [SerializeField] protected bool m_LookAtTarget;
        [Tooltip("Specifies an offset to apply to the target.")]
        [SerializeField] protected Vector3 m_TargetOffset;
        [Tooltip("Should the character rotate towards the target?")]
        [SerializeField] protected bool m_RotateCharacterTowardsTarget = true;
        [Tooltip("Should the camera rotate towards the target?")]
        [SerializeField] protected bool m_RotateCameraTowardsTarget = true;
        [Tooltip("Should the character move towards the target?")]
        [SerializeField] protected bool m_MoveCharacterTowardsTarget = true;
        [Tooltip("The minimum distance between the character and the target when moving towards the target.")]
        [SerializeField] protected float m_MinDistance = 2;
        [Tooltip("Evaluated between time 0 and 1, defines how the movement is affected depending on the distance of from the character to the target (max distance is time 1).")]
        [SerializeField] protected AnimationCurve m_MotorDistanceMultiplier = AnimationCurve.EaseInOut(0, 1, 1, 2);

        [Tooltip("Can the targets be switched?")]
        [SerializeField] protected bool m_CanSwitchTargets;
        [Tooltip("The name of the button mapping for switching targets.")]
        [SerializeField] protected string m_SwitchTargetInputName = "Horizontal";
        [Tooltip("The minimum magnitude required to switch targets.")]
        [SerializeField] protected float m_SwitchTargetMagnitude = 0.8f;

        [Tooltip("The magnitude required in order to break the current target lock. Set to -1 to disable.")]
        [SerializeField] protected float m_BreakForce = 1.5f;
        [Tooltip("The input name for the horizontal break force axis mapping.")]
        [SerializeField] protected string m_HorizontalBreakForceInputName = "Mouse X";
        [Tooltip("The input name for the vertical break force axis mapping.")]
        [SerializeField] protected string m_VerticalBreakForceInputName = "Mouse Y";

        [Tooltip("When the Assist Aim ability is activated should it stop the speed change ability?")]
        [SerializeField] protected bool m_StopSpeedChange = true;

        public bool AutoSelectTarget { get => m_AutoSelectTarget; set => m_AutoSelectTarget = value;  }
        public float Radius { get => m_Radius; set => m_Radius = value; }
        public float Angle { get => m_Angle; set => m_Angle = value; }
        public bool RequireLineOfSight { get => m_RequireLineOfSight; set => m_RequireLineOfSight = value; }
        public Vector3 CenterOffset { get => m_CenterOffset; set => m_CenterOffset = value; }
        public AnimationCurve DistanceInfluence { get => m_DistanceInfluence; set => m_DistanceInfluence = value; }
        public AnimationCurve AngleInfluence { get => m_AngleInfluence; set => m_AngleInfluence = value; }
        public float StickinessAngle { get => m_StickinessAngle; set => m_StickinessAngle = value;  }
        public float StickinessScore { get => m_StickinessScore; set => m_StickinessScore = value;  }

        public bool TargetHumanoidBone { get => m_TargetHumanoidBone; set => m_TargetHumanoidBone = value;  }
        public HumanBodyBones HumanoidBoneTarget { get => m_HumanoidBoneTarget; set => m_HumanoidBoneTarget = value;  }
        public bool LookAtTarget { get => m_LookAtTarget; set => m_LookAtTarget = value; }
        public Vector3 TargetOffset { get => m_TargetOffset; set => m_TargetOffset = value; }
        public bool RotateCharacterTowardsTarget { get => m_RotateCharacterTowardsTarget; set => m_RotateCharacterTowardsTarget = value; }
        public bool RotateCameraTowardsTarget { get => m_RotateCameraTowardsTarget; set => m_RotateCameraTowardsTarget = value; }
        public bool MoveTowardsTarget { get { return m_MoveCharacterTowardsTarget; } set { m_MoveCharacterTowardsTarget = value; } }
        public float MinDistance { get { return m_MinDistance; } set { m_MinDistance = value; } }
        public AnimationCurve MotorDistanceMultiplier { get { return m_MotorDistanceMultiplier; } set { m_MotorDistanceMultiplier = value; } }

        public bool CanSwitchTargets { get { return m_CanSwitchTargets; } set { m_CanSwitchTargets = value; } }
        public float SwitchTargetMagnitude { get { return m_SwitchTargetMagnitude; } set { m_SwitchTargetMagnitude = value; } }
        public float BreakForce { get { return m_BreakForce; } set { m_BreakForce = value; } }

        public bool StopSpeedChange { get { return m_StopSpeedChange; } set { m_StopSpeedChange = value; } }

        private float RadiusSquared => m_Radius * m_Radius;
        public override bool IsConcurrent => true;

        private UltimateCharacterLocomotionHandler m_Handler;
        private ActiveInputEvent m_SwitchInput;
        private ActiveInputEvent m_HorizontalBreakForceInput;
        private ActiveInputEvent m_VerticalBreakForceInput;
        private Collider[] m_ColliderResults;
        private CameraController m_CameraController;

        private Transform m_Target;
        private Objects.ItemAssist.PivotOffset m_TargetPivotOffset;
        private bool m_UseActive;
        private bool m_AllowTargetSwitch = true;

        public Transform Target { 
            get => m_Target; 
            set {
                if (m_Target == value) {
                    return;
                }

                if (m_Target != null) {
                    EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Target.gameObject, "OnDeath", OnTargetDeath);
                }
                m_Target = value;
                if (m_Target != null) {
                    m_TargetPivotOffset = m_Target.gameObject.GetCachedComponent<Objects.ItemAssist.PivotOffset>();
                    EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Target.gameObject, "OnDeath", OnTargetDeath);

                    // If the target is a humanoid then a specific bone can be targeted.
                    if (m_TargetHumanoidBone) {
                        var animator = m_Target.gameObject.GetCachedComponent<Animator>();
                        if (animator != null && animator.isHuman) {
                            m_Target = animator.GetBoneTransform(m_HumanoidBoneTarget);
                        }
                    }
                } else if (IsActive) {
                    StopAbility();
                }
                EventHandler.ExecuteEvent<int, Transform, bool>(m_GameObject, "OnAimTargetChange", -1, m_Target, m_Aiming);
            }
        }

        private bool m_Aiming;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Handler = m_GameObject.GetCachedComponent<UltimateCharacterLocomotionHandler>();
            m_ColliderResults = new Collider[m_CharacterLocomotion.MaxCollisionCount];

            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.RegisterEvent<IUsableItem, bool>(m_GameObject, "OnItemStartUse", OnItemStartUse);
            EventHandler.RegisterEvent<bool, bool>(m_GameObject, "OnAimAbilityStart", OnAim);
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            if (lookSource != null && lookSource is CameraController) {
                m_CameraController = lookSource as CameraController;
            } else {
                m_CameraController = null;
                if (IsActive) {
                    StopAbility(true);
                }
            }
        }

        /// <summary>
        /// Can the ability be started?
        /// </summary>
        /// <returns></returns>
        public override bool CanStartAbility()
        {
            if (!base.CanStartAbility() || m_CameraController == null) {
                return false;
            }

            return m_Target != null;
        }

        /// <summary>
        /// Optionally search for a target while the ability is inactive.
        /// </summary>
        public override void InactiveUpdate()
        {
            base.InactiveUpdate();

            if (m_AutoSelectTarget) {
                Target = SearchForTarget(false);
            }
        }

        /// <summary>
        /// Searches for a new target.
        /// </summary>
        /// <param name="stickyTarget">Should the current selected target receive a better score than the non-selected targets?</param>
        /// <returns>The target (can be null).</returns>
        private Transform SearchForTarget(bool stickyTarget)
        {
            if (m_Target != null && m_RequireLineOfSight) {
                var direction = m_Target.TransformPoint(m_TargetOffset + (m_TargetPivotOffset != null ? m_TargetPivotOffset.Offset : Vector3.zero)) - m_Transform.TransformPoint(m_CenterOffset);
                if (!Physics.Raycast(m_Transform.TransformPoint(m_CenterOffset), direction, out var hit, direction.magnitude, m_CharacterLayerManager.SolidObjectLayers, QueryTriggerInteraction.Ignore) ||
                    (!hit.transform.IsChildOf(m_Target) && !m_Target.IsChildOf(hit.transform))) {
                    return null;
                }
            }

            if ((m_Aiming || m_UseActive) && m_Target != null) {
                return m_Target;
            }

            // Determine which collider should be switched to next based upon the radius of the current target's transform.
            var overlapCount = Physics.OverlapSphereNonAlloc(m_Transform.TransformPoint(m_CenterOffset), m_Radius, m_ColliderResults, m_CharacterLayerManager.EnemyLayers, QueryTriggerInteraction.Ignore);
            if (overlapCount == 0) {
                return null;
            }

            Transform target = null;
            var targetScore = 0f;
            for (int i = 0; i < overlapCount; i++) {
                var potentialTarget = m_ColliderResults[i].transform;
                if (!IsTargetValid(potentialTarget)) {
                    continue; 
                }

                var score = GetTargetScore(potentialTarget, stickyTarget);
                if (score > targetScore) {
                    targetScore = score;
                    target = potentialTarget;
                }
            }

            return target;
        }

        /// <summary>
        /// Is the target valid?
        /// </summary>
        /// <param name="target">The GameObject to determine if it is a valid target.</param>
        /// <returns>True if the GameObject is a valid target.</returns>
        protected virtual bool IsTargetValid(Transform target)
        {
            return target != null && target.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Returns the score of the target. The higher the score the more likely it is to be selected.
        /// </summary>
        /// <param name="potentialTarget">The potential target.</param>
        /// <param name="stickyTarget">Should the current selected target receive a better score than the non-selected targets?</param>
        /// <returns>The target score.</returns>
        protected virtual float GetTargetScore(Transform potentialTarget, bool stickyTarget)
        {
            if (m_CameraController == null) {
                return 0;
            }

            var direction = Vector3.ProjectOnPlane(potentialTarget.position - m_Transform.position, m_CharacterLocomotion.Up);
            direction.y = 0;
            var angle = Vector3.Angle(direction, m_CameraController.LookDirection(m_CameraController.LookPosition(true), true, m_CharacterLayerManager.IgnoreInvisibleCharacterLayers, false, false));
            var halfAngle = (stickyTarget && potentialTarget == m_Target) ? (m_StickinessAngle / 2) : (m_Angle / 2);
            if (angle <= halfAngle) {
                var angleScore = m_AngleInfluence.Evaluate(angle / halfAngle);
                var distanceScore = m_DistanceInfluence.Evaluate(direction.sqrMagnitude / RadiusSquared);
                return angleScore * distanceScore;
            }

            return 0;
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();

            m_CameraController.SetRotationalOverride(GetTargetCameraRotation);

            if (m_CanSwitchTargets) {
                if (m_Handler != null) {
                    m_SwitchInput = GenericObjectPool.Get<ActiveInputEvent>();
                    m_SwitchInput.Initialize(ActiveInputEvent.Type.Axis, m_SwitchTargetInputName, "OnAssistAimSwitchInput");
                    m_Handler.RegisterInputEvent(m_SwitchInput);
                }
                EventHandler.RegisterEvent<float>(m_GameObject, "OnAssistAimSwitchInput", OnSwitchInput);
            }
            if (m_Handler != null) {
                m_HorizontalBreakForceInput = GenericObjectPool.Get<ActiveInputEvent>();
                m_HorizontalBreakForceInput.Initialize(ActiveInputEvent.Type.Axis, m_HorizontalBreakForceInputName, "OnAssistAimUpdateBreakForce");
                m_Handler.RegisterInputEvent(m_HorizontalBreakForceInput);

                m_VerticalBreakForceInput = GenericObjectPool.Get<ActiveInputEvent>();
                m_VerticalBreakForceInput.Initialize(ActiveInputEvent.Type.Axis, m_VerticalBreakForceInputName, "OnAssistAimUpdateBreakForce");
                m_Handler.RegisterInputEvent(m_VerticalBreakForceInput);
            }
            EventHandler.RegisterEvent<float>(m_GameObject, "OnAssistAimUpdateBreakForce", OnUpdateBreakForce);
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            if (base.ShouldBlockAbilityStart(startingAbility)) {
                return true;
            }
            return PreventAbility(startingAbility);
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            if (base.ShouldStopActiveAbility(activeAbility)) {
                return true;
            }
            return PreventAbility(activeAbility);
        }

        /// <summary>
        /// Can the specified ability be active while the Assist Aim ability is active?
        /// </summary>
        /// <param name="activeAbility">The ability to check if it can be active.</param>
        /// <returns>True if the specified ability can be active while the Assist Aim ability is active.</returns>
        private bool PreventAbility(Ability activeAbility)
        {
            // InputStart isn't set until AbilityStarted so the InputIndex should be used as well.
            if (m_StopSpeedChange && InputIndex != -1 && activeAbility is SpeedChange) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Selects the most appropriate target.
        /// </summary>
        public override void Update()
        {
            base.Update();

            Target = SearchForTarget(true);
        }
        
        /// <summary>
        /// The target has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the target.</param>
        /// <param name="attacker">The GameObject that killed the target.</param>
        protected virtual void OnTargetDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            Target = null;
        }

        /// <summary>
        /// Update the controller's rotation values.
        /// </summary>
        public override void UpdateRotation()
        {
            if (!m_RotateCharacterTowardsTarget || (!m_Aiming && m_CharacterLocomotion.Moving)) {
                return;
            }

            // Determine the direction that the character should be facing.
            var direction = Vector3.ProjectOnPlane(m_Target.TransformPoint(m_TargetOffset + (m_TargetPivotOffset != null ? m_TargetPivotOffset.Offset : Vector3.zero)) - m_Rigidbody.TransformPoint(m_CenterOffset), m_CharacterLocomotion.Up);
            var targetRotation = Quaternion.LookRotation(direction, m_CharacterLocomotion.Up);
            targetRotation = Quaternion.Slerp(m_Rigidbody.rotation, targetRotation, m_CharacterLocomotion.MotorRotationSpeed);
            Debug.DrawRay(m_Rigidbody.position, targetRotation * Vector3.forward);
            m_CharacterLocomotion.DesiredRotation = Quaternion.Inverse(m_Rigidbody.rotation) * targetRotation;
        }

        /// <summary>
        /// Apply the character position to move towards the target.
        /// </summary>
        public override void UpdateDesiredMovement()
        {
            if (!m_MoveCharacterTowardsTarget || !m_UseActive) {
                return;
            }

            var distance = Vector3.Distance(m_Transform.position, m_Target.position);
            if (distance < m_MinDistance) {
                m_CharacterLocomotion.DesiredMovement = Vector3.zero;
                return;
            }

            var motorMultiplier = m_MotorDistanceMultiplier.Evaluate(Mathf.Clamp(distance / m_Radius, 0, 1));
            if (float.IsNaN(motorMultiplier)) {
                Debug.LogWarning("The 'Motor Distance Multiplier' animation curve in the combat aim assist should be between 0 and 1.");
                motorMultiplier = 1;
            }

            var direction = m_Target.TransformPoint(m_TargetOffset + (m_TargetPivotOffset != null ? m_TargetPivotOffset.Offset : Vector3.zero)) - m_Transform.position;
            m_CharacterLocomotion.DesiredMovement = direction.normalized * motorMultiplier;
        }

        /// <summary>
        /// An event has indicated that the target may need to be switched.
        /// </summary>
        /// <param name="value">The value of the axis.</param>
        private void OnSwitchInput(float value)
        {
            if (Mathf.Abs(value) > m_SwitchTargetMagnitude) {
                TrySwitchTargets(value > 0);
                m_AllowTargetSwitch = false;
            } else if (!m_AllowTargetSwitch && Mathf.Abs(value) < 0.01f) {
                // Don't allow another target switch until the value is reset. This will prevent the target from quickly switching.
                m_AllowTargetSwitch = true;
            }
        }

        /// <summary>
        /// Tries to switch to the next target. The target may not be able to be switched if there is only one collider overlapping in the specified radius.
        /// </summary>
        /// <param name="rightTarget">Specifies if the next target should be to the right relative to the camera transform.</param>
        public void TrySwitchTargets(bool rightTarget)
        {
            // The targets can't be switched if there isn't a target to begin with.
            if (m_Target == null || !m_AllowTargetSwitch) {
                return;
            }

            // Determine which collider should be switched to next based upon the radius of the current target's transform.
            var overlapCount = Physics.OverlapSphereNonAlloc(m_Target.position, m_Radius, m_ColliderResults, m_CharacterLayerManager.EnemyLayers, QueryTriggerInteraction.Ignore);
            if (overlapCount > 1) {
                var nextTarget = DetermineNextTarget(rightTarget, true, overlapCount);

                // If no target was found then there is no overlapping colliders in the direction specified by rightTarget. The furtherst target in the opposite
                // direction of rightTarget should then be found. This will allow the targets to be cycled through linearly. 
                if (nextTarget == null) {
                    nextTarget = DetermineNextTarget(!rightTarget, false, overlapCount);
                }

                if (nextTarget != null) {
                    // Allow the target to be switched multiple times while an existing switch is taking place.
                    Target = nextTarget;
                }
            }
        }

        /// <summary>
        /// Returns the next valid target within the colliders array.
        /// </summary>
        /// <param name="rightTarget">Specifies if the next target should be to the right relative to the camera transform.</param>
        /// <param name="closestTarget">Should the closest target be found? If false the furthest away target will be found with the specified direction.</param>
        /// <param name="overlapCount">The number of colliders that are overlapping in the colliders array.</param>
        /// <returns>The next valid  target within the colliders array.</returns>
        private Transform DetermineNextTarget(bool rightTarget, bool closestTarget, int overlapCount)
        {
            var interestedOffset = Vector3.zero;
            var interestedDistance = (closestTarget ? float.MaxValue : 0);
            Transform nextTarget = null;
            var relativeTargetPosition = m_Transform.InverseTransformPoint(m_Target.position);

            for (int i = 0; i < overlapCount; ++i) {
                var overlapTransform = m_ColliderResults[i].transform;

                // The target can't switch to itself.
                if (overlapTransform.IsChildOf(m_Target)) {
                    continue;
                }

                var distance = (m_Target.position - overlapTransform.position).sqrMagnitude;
                // If the closest target is being found then the distance needs to be less than the previously-least distance amount.
                // If the closest target is not being found then the furtherst target will be used and in that case the greatest distance will be found.
                if ((closestTarget && distance < interestedDistance) || (!closestTarget && distance > interestedDistance)) {
                    // Use the relative direction so "right" and "left" is relative to the camera rather than to the world space position.
                    var relativePosition = m_Transform.InverseTransformPoint(overlapTransform.position);
                    var offset = relativePosition - relativeTargetPosition;
                    // If the closest target is being found then the offset should be the least value in the specified direction.
                    // If the closest target is not being found then the offset should be the greatest value in the specified direction.
                    if ((closestTarget && ((rightTarget && offset.x > 0 && (nextTarget == null || offset.x < interestedOffset.x)) ||
                        (!rightTarget && offset.x < 0 && (nextTarget == null || offset.x > interestedOffset.x)))) ||
                        (!closestTarget && ((rightTarget && offset.x > 0 && (nextTarget == null || offset.x > interestedOffset.x)) ||
                        (!rightTarget && offset.x < 0 && (nextTarget == null || offset.x < interestedOffset.x))))) {
                        // The transform is at an extreme - save the values so they can be compared against for the next iteration.
                        interestedOffset = offset;
                        interestedDistance = distance;
                        nextTarget = overlapTransform;
                    }
                }
            }

            return nextTarget;
        }

        /// <summary>
        /// Updates the break force axis value to determine if the abiluty should stop.
        /// </summary>
        /// <param name="value">The value of the axis.</param>
        private void OnUpdateBreakForce(float value)
        {
            if (m_BreakForce != -1 && Mathf.Abs(value) > m_BreakForce && !m_UseActive) {
                Target = null;
            }
        }

        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="aim">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        private void OnAim(bool aim, bool inputStart)
        {
            if (!inputStart) {
                return;
            }

            m_Aiming = aim;
            EventHandler.ExecuteEvent<int, Transform, bool>(m_GameObject, "OnAimTargetChange", -1, m_Target, m_Aiming);
        }

        /// <summary>
        /// Start the rotating/moving the character towards the target when an item is being used.
        /// </summary>
        /// <param name="usableItem">The item being used.</param>
        /// <param name="active">The item is being used?</param>
        private void OnItemStartUse(IUsableItem usableItem, bool active)
        {
            m_UseActive = active;
        }

        /// <summary>
        /// Callback for overriding the Camera Controller rotation.
        /// </summary>
        /// <param name="cameraRotation">The current Camera Controller rotation.</param>
        /// <returns>The overridden rotation.</returns>
        private Quaternion GetTargetCameraRotation(Vector3 cameraPosition, Quaternion cameraRotation)
        {
            if (!m_RotateCameraTowardsTarget) {
                return cameraRotation;
            }

            // Keep both the camera and the character within view.
            var direction = m_Target.TransformPoint(m_TargetOffset + (m_TargetPivotOffset != null ? m_TargetPivotOffset.Offset : Vector3.zero)) - cameraPosition;
            if (!m_CharacterLocomotion.FirstPersonPerspective && !m_LookAtTarget) {
                var lookPosition = cameraPosition + direction / 2;
                direction = lookPosition - m_CameraController.LookPosition(false);
            }

            return Quaternion.LookRotation(direction, cameraRotation * m_CharacterLocomotion.Up);
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            Target = null;
            if (m_CameraController != null) {
                m_CameraController.SetRotationalOverride(null);
            }
            if (m_SwitchInput != null) {
                m_Handler.UnregisterInputEvent(m_SwitchInput);
                GenericObjectPool.Return(m_SwitchInput);
            }
            EventHandler.UnregisterEvent<float>(m_GameObject, "OnAssistAimSwitchInput", OnSwitchInput);

            if (m_HorizontalBreakForceInput != null) {
                m_Handler.UnregisterInputEvent(m_HorizontalBreakForceInput);
                GenericObjectPool.Return(m_HorizontalBreakForceInput);
            }
            if (m_VerticalBreakForceInput != null) {
                m_Handler.UnregisterInputEvent(m_VerticalBreakForceInput);
                GenericObjectPool.Return(m_VerticalBreakForceInput);
            }
            EventHandler.UnregisterEvent<float>(m_GameObject, "OnAssistAimUpdateBreakForce", OnUpdateBreakForce);
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.UnregisterEvent<bool, bool>(m_GameObject, "OnAimAbilityStart", OnAim);
        }

        /// <summary>
        /// Draw the gizmo to show the target and the field of view (FOV) radius and angle.
        /// </summary>
        public override void OnDrawGizmos()
        {
            if (m_Target == null) { return; }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_Target.position, 1);
        }
    }
}