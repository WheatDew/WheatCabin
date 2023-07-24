/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Motion;
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
    using Opsive.UltimateCharacterController.VR;
#endif
    using UnityEngine;

    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The Pseudo3D View Type places the camera in a 2.5D view - allowing the camera to look at the character from the side.
    /// </summary>
    [RecommendedMovementType(typeof(Character.MovementTypes.Pseudo3D))]
    [RecommendedMovementType(typeof(Character.MovementTypes.FourLegged))]
    public class Pseudo3D : ViewType
    {
        [Tooltip("The distance that the character should look ahead.")]
        [SerializeField] protected float m_LookDirectionDistance = 100;
        [Tooltip("The forward axis that the camera should adjust towards.")]
        [SerializeField] protected Vector3 m_ForwardAxis = -Vector3.forward;
        [Tooltip("The distance to position the camera away from the anchor.")]
        [SerializeField] protected float m_ViewDistance = 10;
        [Tooltip("The camera will readjust the position/rotation if the character moves outside of this vertical dead zone.")]
        [SerializeField] protected float m_VerticalDeadZone = 1;
        [Tooltip("The amount of smoothing to apply to the movement. Can be zero.")]
        [SerializeField] protected float m_MoveSmoothing = 0.1f;
        [Tooltip("The amount of smoothing to apply to the rotation. Can be zero.")]
        [SerializeField] protected float m_RotationSmoothing = 0.1f;
        [Tooltip("Should the look direction account for depth offsets? This is only used when the mouse is visible.")]
        [SerializeField] protected bool m_DepthLookDirection;
        [Tooltip("The positional spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryPositionSpring = new Spring(0, 0, 0);
        [Tooltip("The rotational spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryRotationSpring = new Spring(0, 0, 0);

        public Vector3 ForwardAxis { get { return m_ForwardAxis; } set { m_ForwardAxis = value; } }
        public float FieldOfView { get { return m_FieldOfView; } set { m_FieldOfView = value; } }
        public float FieldOfViewDamping { get { return m_FieldOfViewDamping; } set { m_FieldOfViewDamping = value; } }
        public float ViewDistance { get { return m_ViewDistance; } set { m_ViewDistance = value; } }
        public float MoveSmoothing { get { return m_MoveSmoothing; } set { m_MoveSmoothing = value; } }
        public float RotationSmoothing { get { return m_RotationSmoothing; } set { m_RotationSmoothing = value; } }
        public bool DepthLookDirection { get { return m_DepthLookDirection; } set { m_DepthLookDirection = value; } }
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

        private IPlayerInput m_PlayerInput;
        private Plane m_HitPlane = new Plane();
        private RaycastHit m_RaycastHit;
        private Vector3 m_LookDirection;
        private Vector3 m_SmoothPositionVelocity;
        private float m_LookDistance;
        private float m_PrevFieldOfViewDamping;
        private int m_StateChangeFrame = -1;
        private float m_CharacterStartPosition;
        private int m_PathIndex;

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
            base.AttachCharacter(character);

            if (m_Character == null) {
                m_PlayerInput = null;
            } else {
                m_PlayerInput = m_Character.GetCachedComponent<IPlayerInput>();
                m_LookDirection = m_CharacterRigidbody.rotation * Vector3.forward;
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
            // Immediately position the camera to face the player.
            if (activate) {
                m_CharacterStartPosition = m_CharacterRigidbody.InverseTransformPoint(Vector3.zero).y; 
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
            if (m_VREnabled) {
                EventHandler.ExecuteEvent("OnTryRecenterTracking");
            }
#endif
            Quaternion targetRotation;
            var activeMovementType = m_CharacterLocomotion.ActiveMovementType as Character.MovementTypes.Pseudo3D;
            if (activeMovementType != null && activeMovementType.Path != null) {
                targetRotation = Quaternion.LookRotation(Vector3.Cross(activeMovementType.Path.GetTangent(GetAnchorPosition(), ref m_PathIndex), m_Transform.up)) * Quaternion.LookRotation(m_ForwardAxis);
            } else {
                targetRotation = Quaternion.LookRotation(m_ForwardAxis);
            }

            return (immediateUpdate ? targetRotation : Quaternion.Slerp(m_Transform.rotation, targetRotation, m_RotationSmoothing)) * Quaternion.Euler(m_SecondaryRotationSpring.Value);
        }

        /// <summary>
        /// Moves the camera to face the character.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediateUpdate)
        {
            // A vertical offset can be applied to prevent the character from changing positions when the character jumps.
            var offset = m_CharacterStartPosition - Utility.MathUtility.InverseTransformPoint(CharacterPosition, CharacterRotation, Vector3.zero).y;
            if (Mathf.Abs(offset) < m_VerticalDeadZone) {
                offset = -offset;
            } else {
                offset = -m_VerticalDeadZone * Mathf.Sign(offset);
            }
            var targetPosition = GetAnchorPosition() + (CharacterRotation * Vector3.up) * offset - m_Transform.forward * m_ViewDistance;
            return (immediateUpdate ? targetPosition : Vector3.SmoothDamp(m_Transform.position, targetPosition, ref m_SmoothPositionVelocity, m_MoveSmoothing)) + m_SecondaryPositionSpring.Value;
        }

        /// <summary>
        /// Returns the position of the look source.
        /// </summary>
        /// <param name="characterLookPosition">Is the character look position being retrieved?</param>
        /// <returns>The position of the look source.</returns>
        public override Vector3 LookPosition(bool characterLookPosition)
        {
            return CharacterPosition;
        }

        /// <summary>
        /// Returns the direction that the character is looking.
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>The direction that the character is looking.</returns>
        public override Vector3 LookDirection(bool characterLookDirection)
        {
            return CharacterRotation * Vector3.forward;
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
            // The character should look towards the cursor or Mouse X/Y direction.
            if (m_PlayerInput.IsCursorVisible()) {
                var ray = m_Camera.ScreenPointToRay(m_PlayerInput.GetMousePosition());
                var planeRaycast = true;
                var hitPointValid = false;
                var hitPoint = Vector3.zero;
                var lookDirection = m_LookDirection;
                if (m_DepthLookDirection) {
                    // If depth look direction is enabled then the 2.5D character should be able to aim along the relative z axis. The hit plane should be based
                    // off of the hit object's relative z position instead of the look position. This allows the character to look forward/back while ensuring the direction
                    // will move through the mouse position.
                    if (Physics.Raycast(ray, out m_RaycastHit, m_LookDirectionDistance, layerMask, QueryTriggerInteraction.Ignore)) {
                        lookDirection = m_RaycastHit.point - lookPosition;
                        planeRaycast = false;
                        hitPointValid = true;
                    }
                }

                if (planeRaycast) {
                    // Cast a ray from the mouse position to an invisible plane to determine the direction that the character should look.
                    var localLookDirection = m_CharacterRigidbody.InverseTransformDirection(m_Transform.forward);
                    // The vertical look direction can be ignored.
                    localLookDirection.y = 0;
                    m_HitPlane.SetNormalAndPosition(m_CharacterRigidbody.TransformDirection(localLookDirection), lookPosition);
                    float distance;
                    if (m_HitPlane.Raycast(ray, out distance)) {
                        hitPoint = ray.GetPoint(distance);
                        lookDirection = (hitPoint - lookPosition);
                        hitPointValid = true;
                    }
                }

                if (hitPointValid) {
                    // The hit point may be located within the look position. Do not set the look direction as this is an impossible direction.
                    if (characterLookDirection || ((CharacterPosition - hitPoint).sqrMagnitude >= (CharacterPosition - lookPosition).sqrMagnitude * 1.5f &&
                            Vector3.Dot(lookDirection, (CharacterRotation * Vector3.forward)) >= 0f)) {
                        m_LookDistance = lookDirection.magnitude;
                        m_LookDirection = lookDirection.normalized;
                    }
                }
            } else {
                // If the cursor isn't visible then get the axis to determine a look rotation. This will be used for controllers and virtual input.
                var direction = Vector3.zero; 
                direction.x = m_PlayerInput.GetAxis(m_PlayerInput.ActiveHorizontalLookInputName);
                direction.y = m_PlayerInput.GetAxis(m_PlayerInput.ActiveVerticalLookInputName);
                if (direction.sqrMagnitude > 0.1f) {
                    m_LookDirection = Quaternion.LookRotation(direction.normalized, m_Transform.up) * Vector3.forward;
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
        }
    }
}