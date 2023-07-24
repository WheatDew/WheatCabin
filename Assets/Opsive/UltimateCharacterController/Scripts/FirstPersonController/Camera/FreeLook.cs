/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes
{
    using Opsive.UltimateCharacterController.FirstPersonController.Character;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// The FreeLook ViewType is a first person view type that allows the camera to rotate independently of the character's direction.
    /// </summary>
    [UltimateCharacterController.Camera.ViewTypes.RecommendedMovementType(typeof(Character.MovementTypes.FreeLook))]
    [Shared.StateSystem.AddState("Zoom", "538aa537a9f445e40b8a2c2758627962")]
    public class FreeLook : FirstPerson
    {
        [Tooltip("The minimum yaw angle (in degrees).")]
        [MinMaxRange(-180, 180)] [SerializeField] protected MinMaxFloat m_YawLimit = new MinMaxFloat(-60, 60);
        [Tooltip("The speed in which the camera should rotate towards the yaw limit when out of bounds.")]
        [Range(0, 1)] [SerializeField] protected float m_YawLimitLerpSpeed = 0.7f;
        [Tooltip("Should the view type rotate with the character's rotation?")]
        [SerializeField] protected bool m_RotateWithCharacter;

        public MinMaxFloat YawLimit { get { return m_YawLimit; } set { m_YawLimit = value; } }
        public float YawLimitLerpSpeed { get { return m_YawLimitLerpSpeed; } set { m_YawLimitLerpSpeed = value; } }
        public bool RotateWithCharacter { get { return m_RotateWithCharacter; } set { m_RotateWithCharacter = value; } }

        private Transform m_FirstPersonObjectsTransform;
        private Vector3 m_LookDirection;

        /// <summary>
        /// Attaches the camera to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the camera to.</param>
        public override void AttachCharacter(GameObject character)
        {
            m_FirstPersonObjectsTransform = null;

            base.AttachCharacter(character);

            if (m_Character != null) {
                var firstPersonObjects = m_Character.GetComponentInChildren<FirstPersonObjects>(true);
                if (firstPersonObjects == null) {
                    // The component may have already been changed to be a child of the camera.
                    firstPersonObjects = m_GameObject.GetComponentInChildren<FirstPersonObjects>(true);
                }
                // FirstPersonObjects won't exist if the character carries no items.
                if (firstPersonObjects != null) {
                    m_FirstPersonObjectsTransform = firstPersonObjects.transform;
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
                baseCharacterRotation = m_CharacterRigidbody.rotation;
                var localRotation = MathUtility.InverseTransformQuaternion(baseCharacterRotation, m_Transform.rotation);
                yaw = localRotation.eulerAngles.y;
            }
            base.ChangeViewType(activate, pitch, yaw, baseCharacterRotation);
        }

        /// <summary>
        /// Rotates the camera according to the horizontal and vertical movement values.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediatePosition">Should the camera be positioned immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediatePosition)
        {
            // Update the rotation. The yaw may have a limit.
            if (Mathf.Abs(m_YawLimit.MinValue - m_YawLimit.MaxValue) < 360) {
                // The rotation shouldn't extend beyond the min and max yaw limit.
                m_Yaw = Mathf.LerpAngle(m_Yaw + horizontalMovement, MathUtility.ClampAngle(m_Yaw, horizontalMovement, m_YawLimit.MinValue, m_YawLimit.MaxValue), m_YawLimitLerpSpeed);
            } else {
                m_Yaw += horizontalMovement;
            }

            // The ViewType can move with the character.
            if (m_RotateWithCharacter) {
                m_BaseRotation = m_CharacterRigidbody.rotation;
            }

            // Return the rotation.
            return base.Rotate(horizontalMovement, verticalMovement, immediatePosition);
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
            var rotation = m_FirstPersonObjectsTransform != null ? m_FirstPersonObjectsTransform.rotation : m_CameraController.Anchor.rotation;

            // Cast a ray from the camera point in the forward direction. The look direction is then the vector from the look position to the hit point.
            RaycastHit hit;
            Vector3 hitPoint;
            if (Physics.Raycast(m_Transform.position, rotation * Vector3.forward, out hit, m_LookDirectionDistance, layerMask, QueryTriggerInteraction.Ignore)) {
                hitPoint = hit.point;
            } else {
                Vector3 position;
                if (includeRecoil) {
                    position = GetAnchorTransformPoint(m_PositionSpring.Value + m_SecondaryPositionSpring.Value);
                } else {
                    position = lookPosition;
                }
                m_LookDirection.Set(0, 0, m_LookDirectionDistance);
                hitPoint = MathUtility.TransformPoint(position, rotation, m_LookDirection);
            }

            return (hitPoint - lookPosition).normalized;
        }
    }
}