/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using Opsive.UltimateCharacterController.Motion;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Looks at the specified target. If no target is specified the camera will look in the character's direction.
    /// </summary>
    public class LookAt : ViewType
    {
        [Tooltip("The object to look at. If null then the character's transform will be used.")]
        [SerializeField] protected Transform m_Target;
        [Tooltip("The offset relative to the target.")]
        [SerializeField] protected Vector3 m_Offset = new Vector3(0, 3, -2);
        [Tooltip("The minimum and maximum distance from the target that the camera should move towards.")]
        [SerializeField] protected MinMaxFloat m_LookDistanceLimit = new MinMaxFloat(1, 5);
        [Tooltip("The amount of smoothing to apply to the position. Can be zero.")]
        [SerializeField] protected float m_PositionSmoothing = 0.08f;
        [Tooltip("The radius of the camera's collision sphere to prevent it from clipping with other objects.")]
        [SerializeField] protected float m_CollisionRadius = 0.05f;
        [Tooltip("The speed at which the view type should rotate towards the target rotation.")]
        [Range(0, 1)] [SerializeField] protected float m_RotationalLerpSpeed = 0.9f;
        [Tooltip("The spring used for applying a rotation to the camera.")]
        [SerializeField] protected Spring m_RotationSpring = new Spring();

        public Transform Target { get { return m_Target; } set { m_Target = value; } }
        public Vector3 Offset { get { return m_Offset; } set { m_Offset = value; } }
        public MinMaxFloat LookDistanceLimit { get { return m_LookDistanceLimit; } set { m_LookDistanceLimit = value; } }
        public float PositionSmoothing { get { return m_PositionSmoothing; } set { m_PositionSmoothing = value; } }
        public float CollisionRadius { get { return m_CollisionRadius; } set { m_CollisionRadius = value; } }
        public float RotationalLerpSpeed { get { return m_RotationalLerpSpeed; } set { m_RotationalLerpSpeed = value; } }
        public Spring RotationSpring
        {
            get { return m_RotationSpring; }
            set
            {
                m_RotationSpring = value;
                if (m_RotationSpring != null) { m_RotationSpring.Initialize(true, true); }
            }
        }

        public override Quaternion BaseCharacterRotation { get { return CharacterRotation; } }
        public override bool FirstPersonPerspective { get { return m_CharacterLocomotion != null ? m_CharacterLocomotion.FirstPersonPerspective : false; } }
        public override float LookDirectionDistance { get { return m_Offset.magnitude; } }
        public override float Pitch { get { return 0; } }
        public override float Yaw { get { return 0; } }

        private Vector3 m_PrevRotationSpringValue;
        private Vector3 m_PrevRotationSpringVelocity;
        private RaycastHit m_RaycastHit;
        private Vector3 m_SmoothPositionVelocity;

        /// <summary>
        /// Initializes the view type to the specified camera controller.
        /// </summary>
        /// <param name="cameraController">The camera controller to initialize the view type to.</param>
        public override void Initialize(CameraController cameraController)
        {
            base.Initialize(cameraController);

            m_RotationSpring.Initialize(true, false);
        }

        /// <summary>
        /// Attaches the view type to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            base.AttachCharacter(character);

            if (m_Character != null && m_Target == null) {
                Animator characterAnimator = null;
                var modelManager = m_Character.GetCachedComponent<Opsive.UltimateCharacterController.Character.ModelManager>();
                if (modelManager != null) {
                    characterAnimator = modelManager.ActiveModel.GetComponent<Animator>();
                } else {
                    var animatorMonitor = m_Character.GetComponentInChildren<Opsive.UltimateCharacterController.Character.AnimatorMonitor>();
                    if (animatorMonitor != null) {
                        characterAnimator = animatorMonitor.GetComponent<Animator>();
                    }
                }
                if (characterAnimator != null) {
                    m_Target = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
                } else {
                    m_Target = m_Transform;
                }
            }
        }

        /// <summary>
        /// Rotates the camera to look at the target.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediateUpdate)
        {
            var rotation = Quaternion.LookRotation((m_Target.position - m_Transform.position).normalized, m_CharacterLocomotion.Up);
            if (!immediateUpdate) {
                rotation = Quaternion.Slerp(m_Transform.rotation, rotation, m_RotationalLerpSpeed);
            }

            // Add the rotational spring value.
            var anchorRotation = m_CameraController.Anchor.rotation;
            var localEulerAngles = MathUtility.InverseTransformQuaternion(anchorRotation, rotation).eulerAngles;
            localEulerAngles += m_RotationSpring.Value;
            rotation = MathUtility.TransformQuaternion(anchorRotation, Quaternion.Euler(localEulerAngles));

            return rotation;
        }

        /// <summary>
        /// Moves the camera to look at the target.
        /// </summary>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated position.</returns>
        public override Vector3 Move(bool immediateUpdate)
        {
            var targetTransformPosition = m_Target.position;
            var cameraPosition = MathUtility.TransformPoint(targetTransformPosition, CharacterRotation, m_Offset);
            // Move towards the target if the target is too far away.
            var transformPosition = m_Transform.position;
            var distance = (targetTransformPosition - transformPosition).magnitude;
            Vector3 targetPosition;
            if (distance > m_LookDistanceLimit.MaxValue) {
                targetPosition = Vector3.SmoothDamp(transformPosition, cameraPosition, ref m_SmoothPositionVelocity, immediateUpdate ? 0 : m_PositionSmoothing);
            } else if (distance < m_LookDistanceLimit.MinValue) {
                targetPosition = Vector3.SmoothDamp(transformPosition, cameraPosition - (m_Target.position - m_Transform.position).normalized * (m_LookDistanceLimit.MaxValue - distance), ref m_SmoothPositionVelocity,
                                                        immediateUpdate ? 0 : m_PositionSmoothing);
            } else {
                targetPosition = m_Transform.position;
            }

            var collisionEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            // Fire a sphere to prevent the camera from colliding with other objects.
            var direction = targetPosition - targetTransformPosition;
            if (Physics.SphereCast(targetTransformPosition, m_CollisionRadius, direction.normalized, out m_RaycastHit, Mathf.Max(direction.magnitude - m_LookDistanceLimit.MinValue, 0.01f), m_CharacterLayerManager.IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore)) {
                // Move the camera in if the character isn't in view.
                targetPosition = m_RaycastHit.point + m_RaycastHit.normal * m_CollisionRadius;
            }
            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);

            return targetPosition;
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
            return CharacterRotation * Vector3.forward;
        }

        /// <summary>
        /// Adds a rotational force to the ViewType.
        /// </summary>
        /// <param name="force">The force to add.</param>
        public override void AddRotationalForce(Vector3 force)
        {
            m_RotationSpring.AddForce(force);
        }

        /// <summary>
        /// Callback when the StateManager will change the active state on the current object.
        /// </summary>
        public override void StateWillChange()
        {
            // Remember the interal spring values so they can be restored if a new spring is applied during the state change.
            m_PrevRotationSpringValue = m_RotationSpring.Value;
            m_PrevRotationSpringVelocity = m_RotationSpring.Velocity;
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            m_RotationSpring.Value = m_PrevRotationSpringValue;
            m_RotationSpring.Velocity = m_PrevRotationSpringVelocity;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            m_RotationSpring.Destroy();
        }
    }
}