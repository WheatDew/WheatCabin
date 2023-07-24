/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.MovementTypes
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.MovementTypes;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The TopDown MovementType can move the character relative to a top down camera Controlled by TopDownAnyAngle ViewType.
    /// </summary>
    public class TopDown : MovementType
    {
        [Tooltip("Should the character move relative to the camera's direction?")]
        [SerializeField] protected bool m_RelativeCameraMovement = true;
        [Tooltip("Should the character look in the direction of the movement?")]
        [SerializeField] protected bool m_LookInMoveDirection;
        [Tooltip("A reference to the character's head. This value will be retrieved automatically if using a humanoid.")]
        [SerializeField] protected Transform m_Head;

        public bool RelativeCameraMovement { get { return m_RelativeCameraMovement; } set { m_RelativeCameraMovement = value; } }
        public bool LookInMoveDirection { get { return m_LookInMoveDirection; } set { m_LookInMoveDirection = value; } }
        public override bool FirstPersonPerspective { get { return false; } }

        private IPlayerInput m_PlayerInput;
        private Animator m_Animator;

        /// <summary>
        /// Initializes the MovementType.
        /// </summary>
        /// <param name="CharacterLocomotion">The reference to the character motor component.</param>
        public override void Initialize(UltimateCharacterLocomotion characterLocomotion)
        {
            base.Initialize(characterLocomotion);

            m_PlayerInput = m_GameObject.GetCachedComponent<IPlayerInput>();
            if (m_Head == null) {
                m_Animator = m_GameObject.GetCachedComponent<Animator>();
                if (m_Animator != null) {
                    m_Head = m_Animator.GetBoneTransform(HumanBodyBones.Head);
                }
            }
            if (m_Head == null) {
                m_Head = m_GameObject.transform;
            }
        }

        /// <summary>
        /// Returns the delta yaw rotation of the character.
        /// </summary>
        /// <param name="characterHorizontalMovement">The character's horizontal movement.</param>
        /// <param name="characterForwardMovement">The character's forward movement.</param>
        /// <param name="cameraHorizontalMovement">The camera's horizontal movement.</param>
        /// <param name="cameraVerticalMovement">The camera's vertical movement.</param>
        /// <returns>The delta yaw rotation of the character.</returns>
        public override float GetDeltaYawRotation(float characterHorizontalMovement, float characterForwardMovement, float cameraHorizontalMovement, float cameraVerticalMovement)
        {
#if UNITY_EDITOR
            if (m_LookSource == null) {
                Debug.LogError($"Error: There is no look source attached to the character {m_GameObject.name}. Ensure the character has a look source attached. For player characters the look source is the Camera Controller, and AI agents use the Local Look Source.");
                return 0;
            }
#endif
            if (m_LookInMoveDirection) {
                if (characterHorizontalMovement != 0 || characterForwardMovement != 0) {
                    var inputVector = new Vector3(characterHorizontalMovement, 0, characterForwardMovement);
                    // Create a new rotation instead of using the look source rotation. The look source rotation is unreliable when the camera is facing vertically down.
                    var lookRotation = Quaternion.LookRotation(Quaternion.LookRotation(m_LookSource.Transform.up, Vector3.up) * inputVector.normalized);
                    return MathUtility.ClampInnerAngle(MathUtility.InverseTransformQuaternion(m_Rigidbody.rotation, lookRotation).eulerAngles.y);
                }
            } else {
                // Rotate towards the look direction. Use the head position to prevent anomalies when using the mouse.
                var direction = m_LookSource.LookDirection(m_Head.position, true, 0, false, false);
                if (direction.sqrMagnitude > 0.1f) {
                    var rotation = Quaternion.LookRotation(direction.normalized, m_CharacterLocomotion.Up);
                    return MathUtility.ClampInnerAngle(MathUtility.InverseTransformQuaternion(m_Rigidbody.rotation, rotation).eulerAngles.y);
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the controller's input vector relative to the movement type.
        /// </summary>
        /// <param name="inputVector">The current input vector.</param>
        /// <returns>The updated input vector.</returns>
        public override Vector2 GetInputVector(Vector2 inputVector)
        {
            if (!m_RelativeCameraMovement) {
                return inputVector;
            }

            var rotation = m_Rigidbody.rotation;
            // The camera may not exist (in the case of an AI agent) but if it does move relative to the camera position.
            if (m_LookSource != null) {
                var localEuler = MathUtility.InverseTransformQuaternion(Quaternion.LookRotation(Vector3.forward, m_CharacterLocomotion.Up), m_LookSource.Transform.rotation).eulerAngles;
                localEuler.x = localEuler.z = 0;
                localEuler.y = 360 - localEuler.y;
                rotation *= Quaternion.Euler(localEuler);
            }
            // Convert to a local input vector. Vector3s are required for the correct calculation.
            var localInputVector = Vector3.zero;
            localInputVector.Set(inputVector.x, 0, inputVector.y);
            localInputVector = Quaternion.Inverse(rotation) * localInputVector;

            // Store the max input vector value so it can be normalized before being returned.
            inputVector.x = localInputVector.x;
            inputVector.y = localInputVector.z;
            // Normalize the input vector to prevent the diagonals from moving faster.
            inputVector = Vector2.ClampMagnitude(inputVector, 1f);

            return inputVector;
        }

        /// <summary>
        /// Can the character look independently of the transform rotation?
        /// </summary>
        /// <param name="characterLookDirection">Is the character look direction being retrieved?</param>
        /// <returns>True if the character should look independently of the transform rotation.</returns>
        public override bool UseIndependentLook(bool characterLookDirection)
        {
            if (m_LookInMoveDirection || base.UseIndependentLook(characterLookDirection)) {
                return true;
            }
            return !characterLookDirection || m_PlayerInput.IsControllerConnected();
        }
    }
}