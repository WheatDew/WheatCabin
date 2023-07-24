/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Motion;
    using Opsive.UltimateCharacterController.Utility;
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
    using Opsive.UltimateCharacterController.VR;
#endif
    using UnityEngine;

    /// <summary>
    /// The Top Down View Type allows the camera to be placed in a top down perspective with the character in view with any pitch (as long as top down) or rotation.
    /// </summary>
    [RecommendedMovementType(typeof(Character.MovementTypes.TopDown))]
    [RecommendedMovementType(typeof(Character.MovementTypes.FourLegged))]
    [RecommendedMovementType(typeof(Character.MovementTypes.PointClick))]
    public class TopDown : ViewType
    {
        [Tooltip("The distance that the character should look ahead.")]
        [SerializeField] protected float m_LookDirectionDistance = 100;
        [Tooltip("The forward axis that the camera should adjust towards.")]
        [SerializeField] protected Vector3 m_ForwardAxis = -Vector3.forward;
        [Tooltip("The up axis that the camera should adjust towards.")]
        [SerializeField] protected Vector3 m_UpAxis = Vector3.up;
        [Tooltip("The speed at which the camera rotates to face the character.")]
        [SerializeField] protected float m_RotationSpeed = 1.5f;
        [Tooltip("The minimum and maximum pitch angle (in degrees).")]
        [MinMaxRange(0, 89.99f)] [SerializeField] protected MinMaxFloat m_PitchLimit = new MinMaxFloat(70,89);
        [Tooltip("The radius of the camera's collision sphere to prevent it from clipping with other objects.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("The distance to position the camera away from the anchor.")]
        [SerializeField] protected float m_ViewDistance = 10;
        [Tooltip("The number of degrees to adjust if the anchor is obstructed by an object.")]
        [SerializeField] protected float m_ViewStep = 2;
        [Tooltip("The amount of smoothing to apply to the movement. Can be zero.")]
        [SerializeField] protected float m_MoveSmoothing = 0.1f;
        [Tooltip("Should the look direction account for vertical offsets? This is only used when the mouse is visible.")]
        [SerializeField] protected bool m_VerticalLookDirection;
        [Tooltip("Can the camera dynamically rotate when a state has been changed?")]
        [SerializeField] protected bool m_AllowDynamicCameraRotation;
        [Tooltip("The desired angle if the camera is dynamically rotating.")]
        [SerializeField] protected float m_DesiredAngle;
        [Tooltip("The speed at which the camera rotates if it is dynamically rotating.")]
        [SerializeField] protected float m_ChangeAngleSpeed = 1;
        [Tooltip("The curve used by the dynamic camera rotation.")]
        [SerializeField] protected AnimationCurve m_RotationTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Tooltip("Can the camera dynamically change pitch when a state has been changed?")]
        [SerializeField] protected bool m_AllowDynamicPitchAdjustment;
        [Tooltip("The desired pitch if the camera is dynamically rotating.")]
        [SerializeField] protected float m_DesiredPitch;
        [Tooltip("The speed at which the camera pitches if it is dynamically changing the pitch.")]
        [SerializeField] protected float m_ChangePitchSpeed = 1;
        [Tooltip("Is a separate Animation curve required for pitch adjustment, if not, the rotation transition will be used.")]
        [SerializeField] protected bool m_UseIndependentPitchTransition;
        [Tooltip("The curve used by the dynamic camera pitch change.")]
        [SerializeField] protected AnimationCurve m_PitchTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Tooltip("Can the camera dynamically change distance when a state has been changed?")]
        [SerializeField] protected bool m_AllowDynamicDistanceAdjustment;
        [Tooltip("The desired distance if the camera is dynamically adjusting distance.")]
        [SerializeField] protected float m_DesiredDistance;
        [Tooltip("The speed at which the camera changes distance if it is dynamically changing the distance.")]
        [SerializeField] protected float m_ChangeDistanceSpeed;
        [Tooltip("Is a separate Animation curve required for distance adjustment, if not, the rotation transition will be used.")]
        [SerializeField] protected bool m_UseIndependentDistanceTransition;
        [Tooltip("The curve used by the dynamic camera distance change.")]
        [SerializeField] protected AnimationCurve m_DistanceTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Tooltip("The positional spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryPositionSpring = new Spring(0, 0, 0);
        [Tooltip("The rotational spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryRotationSpring = new Spring(0, 0, 0);

        public Vector3 ForwardAxis { get { return m_ForwardAxis; } set { m_ForwardAxis = value; } }
        public Vector3 UpAxis { get { return m_UpAxis; } set { m_UpAxis = value; } }
        public float FieldOfView { get { return m_FieldOfView; } set { m_FieldOfView = value; } }
        public float FieldOfViewDamping { get { return m_FieldOfViewDamping; } set { m_FieldOfViewDamping = value; } }
        public float RotationSpeed { get { return m_RotationSpeed; } set { m_RotationSpeed = value; } }
        public MinMaxFloat PitchLimit { get { return m_PitchLimit; } set { m_PitchLimit = value; } }
        public float CollisionRadius { get { return m_CollisionRadius; } set { m_CollisionRadius = value; } }
        public float ViewDistance { get { return m_ViewDistance; } set { m_ViewDistance = value; } }
        public float ViewStep { get { return m_ViewStep; } set { m_ViewStep = value; } }
        public float MoveSmoothing { get { return m_MoveSmoothing; } set { m_MoveSmoothing = value; } }
        public bool VerticalLookDirection { get { return m_VerticalLookDirection; } set { m_VerticalLookDirection = value; } }
        public bool AllowDynamicCameraRotation { get { return m_AllowDynamicCameraRotation; } set { m_AllowDynamicCameraRotation = value; } }
        public float DesiredAngle { get { return m_DesiredAngle; } set { m_DesiredAngle = value; } }
        public float ChangeAngleSpeed { get { return m_ChangeAngleSpeed; } set { m_ChangeAngleSpeed = value; } }
        public AnimationCurve RotationTransitionCurve { get { return m_RotationTransitionCurve; } set { m_RotationTransitionCurve = value; } }
        public bool AllowDynamicPitchAdjustment { get { return m_AllowDynamicPitchAdjustment; } set { m_AllowDynamicPitchAdjustment = value; } }
        public float DesiredPitch { get { return m_DesiredPitch; } set { m_DesiredPitch = value; } }
        public float ChangePitchSpeed { get { return m_ChangePitchSpeed; } set { m_ChangePitchSpeed = value; } }
        public bool UseIndependentPitchTransition { get { return m_UseIndependentPitchTransition; } set { m_UseIndependentPitchTransition = value; } }
        public AnimationCurve PitchTransitionCurve { get { return m_PitchTransitionCurve; } set { m_PitchTransitionCurve = value; } }
        public bool AllowDynamicDistanceAdjustment { get { return m_AllowDynamicDistanceAdjustment; } set { m_AllowDynamicDistanceAdjustment = value; } }
        public float DesiredDistance { get { return m_DesiredDistance; } set { m_DesiredDistance = value; } }
        public float ChangeDistanceSpeed { get { return m_ChangeDistanceSpeed; } set { m_ChangeDistanceSpeed = value; } }
        public bool UseIndependentDistanceTransition { get { return m_UseIndependentDistanceTransition; } set { m_UseIndependentDistanceTransition = value; } }
        public AnimationCurve DistanceTransitionCurve { get { return m_DistanceTransitionCurve; } set { m_DistanceTransitionCurve = value; } }
        public Spring SecondaryPositionSpring
        {
            get { return m_SecondaryPositionSpring; }
            set
            {
                m_SecondaryPositionSpring = value;
                if (m_SecondaryPositionSpring != null) { m_SecondaryPositionSpring.Initialize(false, false); }
            }
        }
        public Spring SecondaryRotationSpring
        {
            get { return m_SecondaryRotationSpring; }
            set
            {
                m_SecondaryRotationSpring = value;
                if (m_SecondaryRotationSpring != null) { m_SecondaryRotationSpring.Initialize(true, false); }
            }
        }

        public override float Pitch { get { return 0; } }
        public override float Yaw { get { return 0; } }
        public override Quaternion BaseCharacterRotation { get { return CharacterRotation; } }
        public override bool FirstPersonPerspective { get { return false; } }
        public override float LookDirectionDistance { get { return m_LookDistance; } }

        private Ray m_Ray;
        private IPlayerInput m_PlayerInput;
        private Plane m_HitPlane;
        private RaycastHit m_RaycastHit;
        private ObjectFader m_ObjectFader;
        private Character.MovementTypes.TopDown m_TopDownMovementType;

        private int m_DeactivateFrame = -1;
        private Vector3 m_LookDirection;
        private Vector3 m_SmoothPositionVelocity;
        private float m_LookDistance;
        private float m_PrevFieldOfViewDamping;
        private int m_StateChangeFrame = -1;

        private float m_CameraAngle;
        private float m_CameraRotationLerp;
        private bool m_RotateCamera;
        private float m_CameraRotateSpeedCorrection;
        private bool m_AdjustPitch;
        private float m_CameraPitch;
        private float m_CameraPitchLerp;
        private float m_CameraPitchSpeedCorrection;
        private bool m_AdjustDistance;
        private float m_CameraDistance;
        private float m_CameraDistanceLerp;


#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
        private bool m_VREnabled;
#endif

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            m_Camera = m_CameraController.gameObject.GetCachedComponent<UnityEngine.Camera>();
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
            VRCameraIdentifier vrCamera;
            if ((vrCamera = m_GameObject.GetComponentInChildren<VRCameraIdentifier>()) != null) {
                // The VR camera will be used as the main camera.
                m_Camera.enabled = false;
                m_Camera = vrCamera.GetComponent<UnityEngine.Camera>();
                m_VREnabled = true;
            }
#endif
            m_ObjectFader = m_CameraController.gameObject.GetComponent<ObjectFader>();
            m_LookDistance = m_LookDirectionDistance;

            // Initialize the springs.
            m_SecondaryPositionSpring.Initialize(false, false);
            m_SecondaryRotationSpring.Initialize(true, false);
        }

        /// <summary>
        /// Attaches the view type to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                m_PlayerInput = null;
            }

            base.AttachCharacter(character);

            if (m_Character != null) {
                m_PlayerInput = m_Character.GetCachedComponent<IPlayerInput>();
                m_LookDirection = m_CharacterRigidbody.rotation * Vector3.forward;
                var characterLocomotion = m_Character.GetCachedComponent<Opsive.UltimateCharacterController.Character.UltimateCharacterLocomotion>();
                var movementTypes = characterLocomotion.MovementTypes;
                for (int i = 0; i < movementTypes.Length; ++i) {
                    if (movementTypes[i] is Character.MovementTypes.TopDown) {
                        m_TopDownMovementType = movementTypes[i] as Character.MovementTypes.TopDown;
                        break;
                    }
                }
            }
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
            if (activate) {
                if (Time.frameCount != m_DeactivateFrame) {
                    m_Ray.direction = GetAnchorPosition() - m_Transform.position;
                }
            } else {
                m_DeactivateFrame = Time.frameCount;
            }
        }

        /// <summary>
        /// Reset the ViewType's variables.
        /// </summary>
        /// <param name="characterRotation">The rotation of the character.</param>
        public override void Reset(Quaternion characterRotation)
        {
            m_SmoothPositionVelocity = Vector3.zero;
            m_SecondaryPositionSpring.Reset();
            m_SecondaryRotationSpring.Reset();
        }

        /// <summary>
        /// Rotates the camera to face the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediateUpdate)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
            if (m_VREnabled && immediateUpdate) {
                EventHandler.ExecuteEvent("OnTryRecenterTracking");
            }
#endif
            if (m_RotateCamera) {
                RotateCamera();
            }
            if (m_AdjustPitch) {
                AdjustCameraPitch();
            }
            if (m_AdjustDistance) {
                AdjustDistance();
            }
            var up = m_CharacterLocomotion.AlignToUpDirection ? m_CharacterLocomotion.Up : m_UpAxis;
            var rotation = Quaternion.LookRotation(-m_Ray.direction, up);
            return (immediateUpdate ? rotation : Quaternion.Slerp(m_Transform.rotation, rotation, m_RotationSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime))
                        * Quaternion.Euler(m_SecondaryRotationSpring.Value);
        }

        /// <summary>
        /// Moves the camera to face the character.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediateUpdate)
        {
            m_Ray.origin = GetAnchorPosition();
            var lookRotation = Quaternion.LookRotation(-m_ForwardAxis, m_UpAxis);
            var up = m_CharacterLocomotion.AlignToUpDirection ? m_CharacterLocomotion.Up : m_UpAxis;
            m_Ray.direction = MathUtility.TransformQuaternion(lookRotation, Quaternion.Euler(90 - m_PitchLimit.MinValue, 0, 0)) * up;

            var collisionEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            var step = 0f;
            // Prevent the character from being obstructed by adjusting the pitch of the camera and testing for an obstruction free path.
            while (Physics.SphereCast(m_Ray, m_CollisionRadius, out m_RaycastHit, m_ViewDistance, m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                if (m_ObjectFader != null) {
                    var canFade = true;
                    // If the object can be faded then the view does not need to readjust.
                    var renderers = m_RaycastHit.collider.gameObject.GetCachedComponents<Renderer>();
                    for (int i = 0; i < renderers.Length; ++i) {
                        var materials = renderers[i].materials;
                        for (int j = 0; j < materials.Length; ++j) {
                            if (!m_ObjectFader.CanMaterialFade(materials[j])) {
                                canFade = false;
                                break;
                            }
                        }
                        if (!canFade) {
                            break;
                        }
                    }
                    // If the material will fade then the view does not need to readjust.
                    if (canFade) {
                        break;
                    }
                }
                if (m_PitchLimit.MinValue + step >= m_PitchLimit.MaxValue) {
                    m_Ray.direction = MathUtility.TransformQuaternion(lookRotation, Quaternion.Euler(90 - m_PitchLimit.MaxValue, 0, 0)) * up;
                    break;
                }
                step += m_ViewStep;
                m_Ray.direction = MathUtility.TransformQuaternion(lookRotation, Quaternion.Euler(90 - m_PitchLimit.MinValue - step, 0, 0)) * up;
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);
            var targetPosition = m_Ray.origin + m_Ray.direction * m_ViewDistance;
            return (immediateUpdate ? targetPosition : Vector3.SmoothDamp(m_Transform.position, targetPosition, ref m_SmoothPositionVelocity, m_MoveSmoothing)) + m_SecondaryPositionSpring.Value;
        }

        /// <summary>
        /// Returns the position of the look source.
        /// </summary>
        /// <param name="characterLookPosition">Is the character look position being retrieved?</param>
        /// <returns>The position of the look source.</returns>
        public override Vector3 LookPosition(bool characterLookPosition)
        {
            if (!characterLookPosition && Cursor.visible) {
                return m_Camera.ScreenPointToRay(m_PlayerInput.GetMousePosition()).origin;
            }
            return m_CharacterRigidbody.position;
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(bool characterLookDirection)
        {
            if (!characterLookDirection && Cursor.visible) {
                return m_Camera.ScreenPointToRay(m_PlayerInput.GetMousePosition()).direction;
            }
            return CharacterRotation * Vector3.forward;
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// As topdown aiming relies on raycasting planes the plane position must match the IK position. Meaning the Movement Type is requesting the plane
        /// at the head's position (to match CharacterIK and prevent rotation glitches with the mouse and IK that occur because of angle rays to parallel offset planes).
        /// As a result the VerticalLookDirection is limited to above head height. Also, to cover all angles, the Anchor offset should be above head height.
        /// </summary>
        /// <param name="lookPosition">The position that the character is looking from.</param>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <param name="layerMask">The LayerMask value of the objects that the look direction can hit.</param>
        /// <param name="includeRecoil">Should recoil be included in the look direction?</param>
        /// <param name="includeMovementSpread">Should the movement spread be included in the look direction?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(Vector3 lookPosition, bool characterLookDirection, int layerMask, bool includeRecoil, bool includeMovementSpread)
        {
            if (m_TopDownMovementType != null && m_TopDownMovementType.LookInMoveDirection) {
                m_LookDirection = CharacterRotation * Vector3.forward;
            } else {
                // The character should look towards the cursor or Mouse X/Y direction.
                if (m_PlayerInput.IsCursorVisible() || !m_PlayerInput.ControllerConnected) {
                    var ray = m_Camera.ScreenPointToRay(m_PlayerInput.GetMousePosition());
                    var planeRaycast = true;
                    var hitPointValid = false;
                    var hitPoint = Vector3.zero;
                    var lookDirection = m_LookDirection;
                    if (m_VerticalLookDirection) {
                        // If vertical look direction is enabled then the top down character should be able to aim along the relative y axis. The hit plane should be based
                        // off of the hit object's relative y position instead of the look position. This allows the character to look up/down while ensuring the direction
                        // will move through the mouse position.
                        if (Physics.Raycast(ray, out m_RaycastHit, m_LookDirectionDistance, layerMask, QueryTriggerInteraction.Ignore)) {
                            lookDirection = m_RaycastHit.point - lookPosition;
                            hitPoint = m_RaycastHit.point;
                            planeRaycast = false;
                            hitPointValid = true;
                        }
                    }
                    if (planeRaycast) {
                        // The first cast is only for high angle top down. On lower angled views the cursor may miss the plane as it is above the horizon.
                        float distance;
                        m_HitPlane.SetNormalAndPosition(CharacterRotation * Vector3.up, lookPosition);
                        if (m_HitPlane.Raycast(ray, out distance)) {
                            hitPoint = ray.GetPoint(distance);
                            hitPoint.y = lookPosition.y;
                            lookDirection = hitPoint - lookPosition;
                            hitPointValid = true;
                        }
                        // If the raycast misses the horizon use a second ray as fallback. This is an approximate but should hit a plane.
                        if (!hitPointValid) {
                            m_HitPlane.SetNormalAndPosition(-m_Transform.forward, lookPosition + (m_Transform.forward * m_LookDirectionDistance));
                            if (m_HitPlane.Raycast(ray, out distance)) {
                                hitPoint = ray.GetPoint(distance);
                                hitPoint.y = lookPosition.y;
                                lookDirection = hitPoint - lookPosition;
                                hitPointValid = true;
                            }
                        }
                    }

                    if (hitPointValid) {
                        // The hit point may be located within the look position. Do not set the look direction as this is an impossible direction.
                        if (characterLookDirection || ((CharacterPosition - hitPoint).sqrMagnitude >= (CharacterPosition - lookPosition).sqrMagnitude * 1.5f &&
                                Vector3.Dot(lookDirection, CharacterRotation * Vector3.forward) >= 0f)) {
                            m_LookDistance = lookDirection.magnitude;
                            m_LookDirection = lookDirection.normalized;
                        }
                    }
                } else {
                    // Set the look direction relative to the camera direction for gamepads.
                    var forward = m_Transform.up; // Use camera up because in a Top Down setting it is always forward facing.
                    forward.y = 0;
                    forward = forward.normalized;
                    var right = m_Transform.right;
                    right.y = 0;
                    var direction = (m_PlayerInput.GetAxis(m_PlayerInput.ActiveVerticalLookInputName) * forward) + (m_PlayerInput.GetAxis(m_PlayerInput.ActiveHorizontalLookInputName) * right);
                    if (direction.sqrMagnitude > 0.1f) {
                        m_LookDirection = Quaternion.LookRotation(direction.normalized, m_Transform.up) * Vector3.forward;
                    }
                }
            }
            return m_LookDirection;
        }

        /// <summary>
        /// Adds a secondary positional force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        /// <param name="restAccumulation">The percent of the force to accumulate to the rest value.</param>
        public override void AddSecondaryPositionalForce(Vector3 force, float restAccumulation)
        {
            if (restAccumulation > 0) {
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
            if (restAccumulation > 0) {
                var springRest = m_SecondaryRotationSpring.RestValue;
                springRest.z += force.z * restAccumulation;
                m_SecondaryRotationSpring.RestValue = springRest;
            }
            m_SecondaryRotationSpring.AddForce(force);
        }

        /// <summary>
        /// Callback when the StateManager will change the active state on the current object.
        /// </summary>
        public override void StateWillChange()
        {
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
                }
            }
            // On state change check if a camera rotation is wanted and if so reset the lerp in case one is current active when the state is changed. 
            if (m_AllowDynamicCameraRotation) {
                m_RotateCamera = true;
                m_CameraAngle = GetForwardAngle();
                m_CameraRotationLerp = 0;
                // Set an arbitrary rotation factor for each target angle to ensure rotation speed is the same.
                var angle = MathUtility.ClampAngle(m_CameraAngle);
                var desiredAngle = MathUtility.ClampAngle(m_DesiredAngle);
                m_CameraRotateSpeedCorrection = 0.2f / ((Mathf.Max(angle, desiredAngle) - Mathf.Min(angle, desiredAngle)) / 360);
            }
            if (m_AllowDynamicPitchAdjustment) {
                m_AdjustPitch = true;
                m_CameraPitch = 90 - Vector3.Angle(m_UpAxis, -m_Transform.forward);
                m_CameraPitchLerp = 0;
                // Set an arbitrary rotation factor for the pitch to ensure rotation speed is the same and matches rotation, if required.
                m_CameraPitchSpeedCorrection = 0.2f / ((Mathf.Max(m_CameraPitch, m_DesiredPitch) - Mathf.Min(m_CameraPitch, m_DesiredPitch)) / 360);
            }
            if (m_AllowDynamicDistanceAdjustment) {
                m_AdjustDistance = true;
                m_CameraDistance = m_ViewDistance;
                m_CameraDistanceLerp = 0;
            }
        }

        /// <summary>
        /// Rotates the camera to match the desired angle.
        /// </summary>
        private void RotateCamera()
        {
            m_CameraRotationLerp += m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime * m_ChangeAngleSpeed * m_CameraRotateSpeedCorrection;
            if (m_CameraRotationLerp >= 1) {
                m_CameraRotationLerp = 1;
                m_RotateCamera = false;
            }

            var angle = Mathf.LerpAngle(m_CameraAngle, m_DesiredAngle, m_RotationTransitionCurve.Evaluate(m_CameraRotationLerp));
            if (!m_RotateCamera) {
                m_CameraAngle = GetForwardAngle();
            }
            m_ForwardAxis = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), m_ForwardAxis.y, Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        /// <summary>
        /// Adjust the pitch of the camera to match the desired pitch.
        /// Since forward vector.y is held at 0 adjusting minimum pitch will adjust the angle.
        /// </summary>
        private void AdjustCameraPitch()
        {
            m_CameraPitchLerp += m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime * m_ChangePitchSpeed * m_CameraPitchSpeedCorrection;
            if (m_CameraPitchLerp >= 1) {
                m_CameraPitchLerp = 1;
                m_AdjustPitch = false;
            }
            var animationCurve = m_UseIndependentPitchTransition ? m_PitchTransitionCurve : m_RotationTransitionCurve;
            m_PitchLimit.MinValue = Mathf.LerpAngle(m_CameraPitch, m_DesiredPitch, animationCurve.Evaluate(m_CameraPitchLerp));
            if (!m_AdjustPitch) {
                m_CameraPitch = m_PitchLimit.MinValue;
            }
        }

        /// <summary>
        /// Adjust the distance of the camera to match the desired distance.
        /// </summary>
        private void AdjustDistance()
        {
            m_CameraDistanceLerp += m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime * m_ChangeDistanceSpeed;
            if (m_CameraDistanceLerp >= 1) {
                m_CameraDistanceLerp = 1;
                m_AdjustDistance = false;
            }
            var animationCurve = m_UseIndependentDistanceTransition ? m_DistanceTransitionCurve : m_RotationTransitionCurve;
            m_ViewDistance = Mathf.Lerp(m_CameraDistance, m_DesiredDistance, animationCurve.Evaluate(m_CameraDistanceLerp));
            if (!m_AdjustDistance) {
                m_CameraDistance = m_ViewDistance;
            }
        }

        /// <summary>
        /// Returns the clamped forward angle of the camera.
        /// </summary>
        /// <returns>The clamped forward angle of the camera.</returns>
        private float GetForwardAngle()
        {
            var forward = m_ForwardAxis;
            forward.y = 0;
            return MathUtility.ClampAngle(Vector3.SignedAngle(forward, Vector3.right, m_Transform.up));
        }
    }
}