/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Motion;
    using Opsive.UltimateCharacterController.Utility;
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
    using Opsive.UltimateCharacterController.VR;
#endif
    using UnityEngine;

    /// <summary>
    /// The Third Person View Type will orbit around the character while always having the character in view.
    /// </summary>
    public abstract class ThirdPerson : ViewType
    {
        [Tooltip("The distance that the character should look ahead.")]
        [SerializeField] protected float m_LookDirectionDistance = 100;
        [Tooltip("The forward axis that the camera should adjust towards.")]
        [SerializeField] protected Vector3 m_ForwardAxis = Vector3.forward;
        [Tooltip("The offset between the anchor and the camera.")]
        [SerializeField] protected Vector3 m_LookOffset = new Vector3(0.5f, 0, -2.5f);
        [Tooltip("The amount of smoothing to apply to the look offset. Can be zero.")]
        [SerializeField] protected float m_LookOffsetSmoothing = 0.05f;
        [Tooltip("The radius of the camera's collision sphere to prevent it from clipping with other objects.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("The offset from the anchor position when determining if there is a collision.")]
        [SerializeField] protected Vector3 m_CollisionAnchorOffset;
        [Tooltip("The lerping speed of the camera rotation.")]
        [Range(0, 1)] [SerializeField] protected float m_RotationSpeed = 0.6f;
        [Tooltip("The lerping speed when determining the override or align to gravity character rotation.")]
        [Range(0, 1)] [SerializeField] protected float m_SecondaryRotationSpeed = 0.8f;
        [Tooltip("The amount of freedom the character has on the horizontal axis before the camera starts to follow the character.")]
        [SerializeField] protected float m_HorizontalPivotFreedom = 0f;
        [Tooltip("The amount of smoothing to apply to the position when an object is obstructing the target position. Can be zero.")]
        [SerializeField] protected float m_ObstructionPositionSmoothing = 0.04f;
        [Tooltip("The positional spring used for regular movement.")]
        [SerializeField] protected Spring m_PositionSpring = new Spring();
        [Tooltip("The rotational spring used for regular movement.")]
        [SerializeField] protected Spring m_RotationSpring = new Spring();
        [Tooltip("The positional spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryPositionSpring = new Spring();
        [Tooltip("The rotational spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryRotationSpring = new Spring();
        [Tooltip("The name of the step zoom input mapping.")]
        [SerializeField] protected string m_StepZoomInputName = "Mouse ScrollWheel";
        [Tooltip("Specifies how quickly the camera zooms when step zooming.")]
        [SerializeField] protected float m_StepZoomSensitivity;
        [Tooltip("The minimum and maximum distance that the step zoom can zoom.")]
        [SerializeField] protected MinMaxFloat m_StepZoomLimit = new MinMaxFloat(0, 1);
        [Tooltip("The minimum and maximum pitch angle (in degrees).")]
        [MinMaxRange(-90, 90)] [SerializeField] protected MinMaxFloat m_PitchLimit = new MinMaxFloat(-72, 72);

        public override float LookDirectionDistance { get { return m_LookDirectionDistance; } }
        public Vector3 ForwardAxis { get { return m_ForwardAxis; } set { m_ForwardAxis = value; } }
        public Vector3 LookOffset { get { return m_LookOffset; } set { m_LookOffset = value; } }
        public float LookOffsetSmoothing { get { return m_LookOffsetSmoothing; } set { m_LookOffsetSmoothing = value; } }
        public float FieldOfView { get { return m_FieldOfView; } set { m_FieldOfView = value; } }
        public float FieldOfViewDamping { get { return m_FieldOfViewDamping; } set { m_FieldOfViewDamping = value; } }
        public float CollisionRadius { get { return m_CollisionRadius; } set { m_CollisionRadius = value; } }
        public Vector3 CollisionAnchorOffset { get { return m_CollisionAnchorOffset; } set { m_CollisionAnchorOffset = value; } }
        public float RotationSpeed { get { return m_RotationSpeed; } set { m_RotationSpeed = value; } }
        public float SecondaryRotationSpeed { get { return m_SecondaryRotationSpeed; } set { m_SecondaryRotationSpeed = value; } }
        public float HorizontalPivotFreedom { get { return m_HorizontalPivotFreedom; } set { m_HorizontalPivotFreedom = value; } }
        public float ObstructionPositionSmoothing { get { return m_ObstructionPositionSmoothing; } set {m_ObstructionPositionSmoothing = value; } }
        public Spring PositionSpring { get { return m_PositionSpring;
            } set {
                m_PositionSpring = value;
                if (m_PositionSpring != null) { m_PositionSpring.Initialize(false, false); }
            }
        }
        public Spring RotationSpring { get { return m_RotationSpring;
            } set {
                m_RotationSpring = value;
                if (m_RotationSpring != null) { m_RotationSpring.Initialize(true, false); }
            }
        }
        public Spring SecondaryPositionSpring { get { return m_SecondaryPositionSpring;
            } set {
                m_SecondaryPositionSpring = value;
                if (m_SecondaryPositionSpring != null) { m_SecondaryPositionSpring.Initialize(false, false); }
            }
        }
        public Spring SecondaryRotationSpring { get { return m_SecondaryRotationSpring;
            } set {
                m_SecondaryRotationSpring = value;
                if (m_SecondaryRotationSpring != null) { m_SecondaryRotationSpring.Initialize(true, false); }
            }
        }
        public string StepZoomInputName { get { return m_StepZoomInputName; } set { m_StepZoomInputName = value; } }
        public float StepZoomSensitivity { get { return m_StepZoomSensitivity; } set { m_StepZoomSensitivity = value; } }
        public MinMaxFloat StepZoomLimit { get { return m_StepZoomLimit; } set { m_StepZoomLimit = value; } }
        public MinMaxFloat PitchLimit { get { return m_PitchLimit; } set { m_PitchLimit = value; } }

        private Transform m_CrosshairsTransform;
        private Canvas m_CrosshairsCanvas;
        private UI.CrosshairsMonitor m_CrosshairsMonitor;

        protected float m_Pitch;
        protected float m_Yaw;
        protected Quaternion m_BaseRotation;
        private bool m_AppendingZoomState;
        private Quaternion m_CrosshairsDeltaRotation;
        private Vector3 m_CrosshairsPosition;

        private Vector3 m_CurrentLookOffset;
        private RaycastHit m_RaycastHit;
        private Vector3 m_ObstructionSmoothPositionVelocity;
        private Vector3 m_SmoothLookOffsetVelocity;

        protected CameraControllerHandler m_Handler;
        private ActiveInputEvent m_StepZoomInputEvent;
        private float m_StepZoom;

        private System.Func<Vector3, Quaternion, Quaternion> m_RotationalOverride;

        private Vector3 m_PrevPositionSpringValue;
        private Vector3 m_PrevPositionSpringVelocity;
        private Vector3 m_PrevRotationSpringValue;
        private Vector3 m_PrevRotationSpringVelocity;
        private float m_PrevFieldOfViewDamping;
        private int m_StateChangeFrame = -1;
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
        private bool m_VREnabled;
#endif

        public override float Pitch { get => m_Pitch; }
        public override float Yaw { get => m_Yaw; }
        public override Quaternion BaseCharacterRotation { get => m_BaseRotation; }
        public override bool FirstPersonPerspective { get => false; }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
            VRCameraIdentifier vrCamera;
            if ((vrCamera = m_GameObject.GetComponentInChildren<VRCameraIdentifier>()) != null) {
                // The VR camera will be used as the main camera.
                m_Camera.enabled = false;
                m_Camera = vrCamera.GetComponent<UnityEngine.Camera>();
                m_VREnabled = true;
            }
#endif
            m_Handler = m_GameObject.GetCachedComponent<CameraControllerHandler>();
            m_CurrentLookOffset = m_LookOffset;

            // Initialize the springs.
            m_PositionSpring.Initialize(false, false);
            m_RotationSpring.Initialize(true, false);
            m_SecondaryPositionSpring.Initialize(false, false);
            m_SecondaryRotationSpring.Initialize(true, false);
        }

        /// <summary>
        /// The view type has changed.
        /// </summary>
        /// <param name="activate">Should the current view type be activated?</param>
        /// <param name="pitch">The pitch of the camera (in degrees).</param>
        /// <param name="yaw">The yaw of the camera (in degrees).</param>
        /// <param name="baseCharacterRotation">The rotation of the character.</param>
        public override void ChangeViewType(bool activate, float pitch, float yaw, Quaternion baseCharacterRotation)
        {
            base.ChangeViewType(activate, pitch, yaw, baseCharacterRotation);

            if (activate) {
                ResetRotation(pitch, yaw, baseCharacterRotation);
                m_CurrentLookOffset = m_LookOffset;
                if (m_StepZoomSensitivity > 0) {
                    if (m_Handler != null) {
                        m_StepZoomInputEvent = GenericObjectPool.Get<ActiveInputEvent>();
                        m_StepZoomInputEvent.Initialize(ActiveInputEvent.Type.Axis, m_StepZoomInputName, "OnThirdPersonViewTypeStepZoom");
                        m_Handler.RegisterInputEvent(m_StepZoomInputEvent);
                    }
                    EventHandler.RegisterEvent<float>(m_GameObject, "OnThirdPersonViewTypeStepZoom", OnStepZoom);
                }
            } else {
                if (m_StepZoomSensitivity > 0) {
                    if (m_Handler != null) {
                        m_StepZoomInputEvent = GenericObjectPool.Get<ActiveInputEvent>();
                        m_Handler.UnregisterAbilityInputEvent(m_StepZoomInputEvent);
                        GenericObjectPool.Return(m_StepZoomInputEvent);
                    }
                    EventHandler.UnregisterEvent<float>(m_GameObject, "OnThirdPersonViewTypeStepZoom", OnStepZoom);
                }
            }
        }

        /// <summary>
        /// Reset the ViewType's variables.
        /// </summary>
        /// <param name="characterRotation">The rotation of the character.</param>
        public override void Reset(Quaternion characterRotation)
        {
            m_Pitch = 0;
            m_Yaw = 0;
            m_BaseRotation = characterRotation;
            m_CurrentLookOffset = m_LookOffset;

            m_PositionSpring.Reset();
            m_RotationSpring.Reset();
            m_SecondaryPositionSpring.Reset();
            m_SecondaryRotationSpring.Reset();
            m_ObstructionSmoothPositionVelocity = Vector3.zero;
        }

        /// <summary>
        /// Resets the View Type rotation parameters to the specified values.
        /// </summary>
        /// <param name="pitch">The pitch of the camera (in degrees).</param>
        /// <param name="yaw">The yaw of the camera (in degrees).</param>
        /// <param name="baseCharacterRotation">The rotation of the character.</param>
        public override void ResetRotation(float pitch, float yaw, Quaternion baseCharacterRotation)
        {
            m_Pitch = pitch;
            m_Yaw = yaw;
            m_BaseRotation = baseCharacterRotation;
        }

        /// <summary>
        /// Sets the crosshairs to the specified transform.
        /// </summary>
        /// <param name="crosshairs">The transform of the crosshairs.</param>
        public override void SetCrosshairs(Transform crosshairs)
        {
            m_CrosshairsTransform = crosshairs;

            if (m_CrosshairsTransform != null) {
                m_CrosshairsCanvas = crosshairs.GetComponentInParent<Canvas>();
                m_CrosshairsMonitor = crosshairs.gameObject.GetCachedComponent<UI.CrosshairsMonitor>();
                m_CrosshairsPosition = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            }
        }

        /// <summary>
        /// Returns the delta rotation caused by the crosshairs.
        /// </summary>
        /// <returns>The delta rotation caused by the crosshairs.</returns>
        public override Quaternion GetCrosshairsDeltaRotation()
        {
            if (m_CrosshairsTransform == null) {
                return Quaternion.identity;
            }

            // The crosshairs direction should only be updated when it changes.
            if (m_CrosshairsPosition != m_CrosshairsTransform.position) {
                Vector3 direction;
                if (m_CrosshairsCanvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                    var screenPoint = RectTransformUtility.WorldToScreenPoint(null, m_CrosshairsTransform.position);
                    direction = m_Camera.ScreenPointToRay(screenPoint).direction;
                } else {
                    direction = (m_CrosshairsTransform.position - m_Camera.transform.position).normalized;
                }
                m_CrosshairsDeltaRotation = Quaternion.LookRotation(direction, m_Transform.up) * Quaternion.Inverse(m_Transform.rotation);
                m_CrosshairsPosition = m_CrosshairsTransform.position;
            }

            return m_CrosshairsDeltaRotation;
        }

        /// <summary>
        /// Updates the camera field of view.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        public override void UpdateFieldOfView(bool immediateUpdate)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
            if (m_VREnabled) {
                return;
            }
#endif
            base.UpdateFieldOfView(immediateUpdate);
        }

        /// <summary>
        /// Rotates the camera according to the horizontal and vertical movement values.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediateUpdate)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
            if (m_VREnabled && immediateUpdate) {
                m_BaseRotation = CharacterRotation;
                EventHandler.ExecuteEvent("OnTryRecenterTracking");
            }
#endif
            // Rotate with the moving platform.
            m_BaseRotation *= m_CharacterLocomotion.MovingPlatformRotation;

            // The camera should always stay aligned to the character's up direction.
            if (m_CharacterLocomotion.AlignToUpDirection) {
                var proj = (m_BaseRotation * Vector3.forward) - Vector3.Dot(m_BaseRotation * Vector3.forward, m_CharacterLocomotion.Up) * m_CharacterLocomotion.Up;
                if (proj.sqrMagnitude > 0.0001f) {
                    if (m_CharacterLocomotion.MovingPlatform == null) {
                        m_BaseRotation = Quaternion.Slerp(m_BaseRotation, Quaternion.LookRotation(proj, m_CharacterLocomotion.Up), m_SecondaryRotationSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale);
                    } else {
                        m_BaseRotation = Quaternion.LookRotation(proj, m_CharacterLocomotion.Up);
                    }
                } else {
                    // Prevents locking the rotation if proj magnitude is close to 0 when character forward is close or equal to the up vector.
                    var right = m_BaseRotation * Vector3.right;
                    var forward = Vector3.Cross(right, m_CharacterLocomotion.Up);
                    m_BaseRotation = Quaternion.LookRotation(forward, m_CharacterLocomotion.Up);
                }
            }

            if (m_RotationalOverride != null) {
                var currentRotation = MathUtility.TransformQuaternion(m_BaseRotation, Quaternion.Euler(m_Pitch, m_Yaw, 0));
                var targetRotation = Quaternion.Slerp(currentRotation, m_RotationalOverride(m_Transform.position, currentRotation), m_SecondaryRotationSpeed);

                // Set the pitch and yaw so when the override is reset the view type won't snap back to the previous rotation value.
                var localRotation = MathUtility.InverseTransformQuaternion(m_BaseRotation, targetRotation).eulerAngles;
                m_Pitch = localRotation.x;
                m_Yaw = localRotation.y;
            }

            // Update the rotation. The pitch may have a limit.
            if (Mathf.Abs(m_PitchLimit.MinValue - m_PitchLimit.MaxValue) < 180) {
                m_Pitch = MathUtility.ClampAngle(m_Pitch, -verticalMovement, m_PitchLimit.MinValue, m_PitchLimit.MaxValue);
            } else {
                m_Pitch -= verticalMovement;
            }

            // Prevent the values from getting too large.
            m_Pitch = MathUtility.ClampInnerAngle(m_Pitch);
            m_Yaw = MathUtility.ClampInnerAngle(m_Yaw);

            // Return the rotation.
            var rotation = MathUtility.TransformQuaternion(m_BaseRotation, Quaternion.Euler(m_Pitch, m_Yaw, 0)) * Quaternion.LookRotation(m_ForwardAxis) * Quaternion.Euler(m_RotationSpring.Value) * Quaternion.Euler(m_SecondaryRotationSpring.Value);
            return immediateUpdate ? rotation : Quaternion.Slerp(m_Transform.rotation * m_CharacterLocomotion.MovingPlatformRotation, rotation, m_RotationSpeed);
        }

        /// <summary>
        /// Moves the camera according to the current pitch and yaw values.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediateUpdate)
        {
            var characterRotation = CharacterRotation;
            var characterPosition = CharacterPosition;

            // Prevent obstruction from other objects. Check for obstruction against character player position rather than the look position because the character should always be visible. It doesn't
            // matter as much if the look position isn't directly visible.
            var anchorPosition = GetAnchorPosition();
            var localAnchorPosition = MathUtility.InverseTransformPoint(anchorPosition, Quaternion.LookRotation(m_Transform.forward, characterRotation * Vector3.up), m_Transform.position);
            m_CurrentLookOffset = immediateUpdate ? m_LookOffset : Vector3.SmoothDamp(m_CurrentLookOffset, m_LookOffset, ref m_SmoothLookOffsetVelocity, m_LookOffsetSmoothing);
            var lookPosition = anchorPosition + (m_CurrentLookOffset.y * (characterRotation * Vector3.up)) + ((m_CurrentLookOffset.z + m_StepZoom) * m_Transform.forward);

            // The position spring is already smoothed so it doesn't need to be included in SmoothDamp.
            lookPosition += m_Transform.TransformDirection(m_PositionSpring.Value + m_SecondaryPositionSpring.Value);
            
            // Allow for some freedom before the camera moves with the character.
            if (immediateUpdate) {
                lookPosition += m_LookOffset.x * m_Transform.right;
                m_CurrentLookOffset.x = m_LookOffset.x;
            } else if (Mathf.Abs(m_CurrentLookOffset.x - localAnchorPosition.x) >= m_HorizontalPivotFreedom) {
                lookPosition += (m_CurrentLookOffset.x + m_HorizontalPivotFreedom * Mathf.Sign(localAnchorPosition.x)) * m_Transform.right;
            } else {
                m_CurrentLookOffset.x = localAnchorPosition.x;
                lookPosition += localAnchorPosition.x * m_Transform.right;
            }

            // Smoothly move into position.
            var targetPosition = lookPosition;
            var collisionEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            var direction = lookPosition - (anchorPosition + m_CollisionAnchorOffset);
            var normalizedDirection = direction.normalized;
            var directionMagnitude = direction.magnitude;
            // Fire a sphere to prevent the camera from colliding with other objects.
            if (Physics.SphereCast(anchorPosition + m_CollisionAnchorOffset - normalizedDirection * m_CollisionRadius, m_CollisionRadius, normalizedDirection, out m_RaycastHit, directionMagnitude,
                                m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                // Move the camera in if the character isn't in view.
                targetPosition = m_RaycastHit.point + m_RaycastHit.normal * m_CollisionRadius;
                if (!immediateUpdate) {
                    targetPosition = Vector3.SmoothDamp(m_Transform.position + m_CharacterLocomotion.MovingPlatformMovement, targetPosition, ref m_ObstructionSmoothPositionVelocity, m_ObstructionPositionSmoothing);
                }

                // Keep a constant height if there is nothing getting in the way of that position.
                var localDirection = MathUtility.TransformDirection(direction, characterRotation);
                if (localDirection.y > 0) {
                    // Account for local y values.
                    var constantHeightPosition = MathUtility.InverseTransformPoint(characterPosition, m_BaseRotation, targetPosition);
                    constantHeightPosition.y = MathUtility.InverseTransformPoint(characterPosition, m_BaseRotation, lookPosition).y;
                    constantHeightPosition = MathUtility.TransformPoint(characterPosition, m_BaseRotation, constantHeightPosition);
                    if (!Physics.SphereCast(anchorPosition + m_CollisionAnchorOffset - normalizedDirection * m_CollisionRadius, m_CollisionRadius, normalizedDirection,
                            out m_RaycastHit, directionMagnitude - m_CollisionRadius, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                        targetPosition = constantHeightPosition;
                    }
                }
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);

            // Prevent the camera from clipping with the character.
            Collider containsCollider;
            if ((containsCollider = m_CharacterLocomotion.BoundsCountains(targetPosition)) != null) {
                targetPosition = containsCollider.ClosestPointOnBounds(targetPosition);
            }

            // The target position should never be lower than the character's position. This may happen if the camera is trying to be positioned below water.
            var localTargetPosition = MathUtility.InverseTransformPoint(characterPosition, characterRotation, targetPosition);
            if (localTargetPosition.y < 0) {
                localTargetPosition.y = 0;
                targetPosition = MathUtility.TransformPoint(characterPosition, characterRotation, localTargetPosition);
            }

            return targetPosition;
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(bool characterLookDirection)
        {
            var crosshairsDeltaRotation = characterLookDirection ? Quaternion.identity : GetCrosshairsDeltaRotation();
            var movingPlatformRotation = characterLookDirection ? Quaternion.Inverse(m_CharacterLocomotion.MovingPlatformRotation) : Quaternion.identity;
            return (m_Transform.rotation * crosshairsDeltaRotation * Quaternion.Inverse(movingPlatformRotation)) * Vector3.forward;
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="lookPosition">The position that the character is looking from.</param>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <param name="layerMask">The LayerMask value of the objects that the look direction can hit.</param>
        /// <param name="includeRecoil">Should recoil be included in the look direction?</param>
        /// <param name="includeMovementSpread">Should the movement spread be included in the look direction?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(Vector3 lookPosition, bool characterLookDirection, int layerMask, bool includeRecoil, bool includeMovementSpread)
        {
            var collisionEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);

            // If a crosshairs is specified then the character should look at the crosshairs. Do not use the crosshairs delta for character look directions to prevent
            // the character's rotation from being affected by the crosshairs.
            var crosshairsDeltaRotation = characterLookDirection ? Quaternion.identity : GetCrosshairsDeltaRotation();
            var platformRotation = characterLookDirection ? Quaternion.Inverse(m_CharacterLocomotion.MovingPlatformRotation) : Quaternion.identity;

            // Cast a ray from the camera point in the forward direction. The look direction is then the vector from the look position to the hit point.
            var rotation = (includeRecoil ? m_Transform.rotation : 
                                MathUtility.TransformQuaternion(m_BaseRotation, Quaternion.Euler(m_Pitch, m_Yaw, 0)) * Quaternion.LookRotation(m_ForwardAxis)) *
                                crosshairsDeltaRotation * Quaternion.Inverse(platformRotation);
            Vector3 hitPoint;
            if (Physics.Raycast(m_Transform.position, rotation * Vector3.forward, out var hit, m_LookDirectionDistance, layerMask, QueryTriggerInteraction.Ignore)) {
                hitPoint = hit.point;
            } else {
                var offset = Vector3.zero;
                offset.Set(0, 0, m_LookDirectionDistance);
                hitPoint = MathUtility.TransformPoint(m_Transform.position, rotation, offset);
            }

            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);
            var direction = (hitPoint - lookPosition).normalized;
            if (includeMovementSpread && m_CrosshairsMonitor != null) {
                var spread = m_CrosshairsMonitor.GetMovementSpread() * 180;
                if (spread > 0) {
                    direction = (Quaternion.AngleAxis(Random.Range(-spread, spread), (CharacterRotation * Vector3.up)) *
                        Quaternion.AngleAxis(Random.Range(-spread, spread), CharacterRotation * Vector3.right)) * m_Transform.forward;
                }
            }
            return direction;
        }

        /// <summary>
        /// Adds a positional force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public override void AddPositionalForce(Vector3 force)
        {
            m_PositionSpring.AddForce(force);
        }

        /// <summary>
        /// Adds a secondary force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public override void AddRotationalForce(Vector3 force)
        {
            m_RotationSpring.AddForce(force);
        }

        /// <summary>
        /// Adds a secondary positional force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="restAccumulation">The percent of the force to accumulate to the rest value.</param>
        public override void AddSecondaryPositionalForce(Vector3 force, float restAccumulation)
        {
            if (restAccumulation > 0 && m_RotationalOverride == null) {
                m_SecondaryPositionSpring.RestValue += force * restAccumulation;
            }
            m_SecondaryPositionSpring.AddForce(force);
        }

        /// <summary>
        /// Adds a delayed rotational force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="restAccumulation">The percent of the force to accumulate to the rest value.</param>
        public override void AddSecondaryRotationalForce(Vector3 force, float restAccumulation)
        {
            if (restAccumulation > 0 && m_RotationalOverride == null) {
                m_Pitch += force.x * restAccumulation;
                m_Yaw += force.y * restAccumulation;
                var springRest = m_SecondaryRotationSpring.RestValue;
                springRest.z += force.z * restAccumulation;
                m_SecondaryRotationSpring.RestValue = springRest;
            }
            m_SecondaryRotationSpring.AddForce(force);
        }

        /// <summary>
        /// The camera should zoom in or out.
        /// </summary>
        /// <param name="amount">The amount to zoom.</param>
        private void OnStepZoom(float amount)
        {
            m_StepZoom = Mathf.Clamp(m_StepZoom + m_StepZoomSensitivity * amount * Time.deltaTime, m_StepZoomLimit.MinValue, m_StepZoomLimit.MaxValue);
        }

        /// <summary>
        /// Sets the function that should override the rotation.
        /// </summary>
        /// <param name="overrideFunc">The function override.</param>
        public override void SetRotationalOverride(System.Func<Vector3, Quaternion, Quaternion> overrideFunc)
        {
            m_RotationalOverride = overrideFunc;
        }

        /// <summary>
        /// Callback when the StateManager will change the active state on the current object.
        /// </summary>
        public override void StateWillChange()
        {
            // Remember the interal spring values so they can be restored if a new spring is applied during the state change.
            m_PrevPositionSpringValue = m_PositionSpring.Value;
            m_PrevPositionSpringVelocity = m_PositionSpring.Velocity;
            m_PrevRotationSpringValue = m_RotationSpring.Value;
            m_PrevRotationSpringVelocity = m_RotationSpring.Velocity;
            // Multiple state changes can occur within the same frame. Only remember the first damping value.
            if (m_StateChangeFrame != Time.frameCount) {
                m_PrevFieldOfViewDamping = m_FieldOfViewDamping;
            }
            m_StateChangeFrame = Time.frameCount;
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            if (m_Character == null) {
                return;
            }

            // Append the zoom state name so the combination of state names will be called, such as "CrouchZoom".
            if (!string.IsNullOrEmpty(m_CameraController.ZoomState) && !m_AppendingZoomState) {
                m_AppendingZoomState = true;
                for (int i = 0; i < m_States.Length; ++i) {
                    StateManager.SetState(m_GameObject, m_States[i].Name + m_CameraController.ZoomState, m_States[i].Active && m_CameraController.ZoomInput);
                }
                m_AppendingZoomState = false;
            }

            if (m_Camera.fieldOfView != m_FieldOfView
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
                && !m_VREnabled
#endif
                ) {
                m_FieldOfViewChangeTime = Time.time;
                if (m_CameraController.ActiveViewType == this) {
                    // The field of view and location should get a head start if the damping was previously 0. This will allow the field of view and location
                    // to move back to the original value when the state is no longer active.
                    if (m_PrevFieldOfViewDamping == 0) {
                        m_Camera.fieldOfView = (m_Camera.fieldOfView + m_FieldOfView) * 0.5f;
                    }

                    m_Transform.position = (m_Transform.position + Move(true)) * 0.5f;
                }
            }

            m_PositionSpring.Value = m_PrevPositionSpringValue;
            m_PositionSpring.Velocity = m_PrevPositionSpringVelocity;
            m_RotationSpring.Value = m_PrevRotationSpringValue;
            m_RotationSpring.Velocity = m_PrevRotationSpringVelocity;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            m_PositionSpring.Destroy();
            m_RotationSpring.Destroy();
            m_SecondaryPositionSpring.Destroy();
            m_SecondaryRotationSpring.Destroy();
        }
    }
}