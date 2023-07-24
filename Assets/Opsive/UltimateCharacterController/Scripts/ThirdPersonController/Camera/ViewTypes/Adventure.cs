/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// The Adventure View Type will inherit the functionality from the Third Person View Type while allowing the camera yaw to rotate freely.
    /// </summary>
    [UltimateCharacterController.Camera.ViewTypes.RecommendedMovementType(typeof(Character.MovementTypes.Adventure))]
    [UltimateCharacterController.Camera.ViewTypes.RecommendedMovementType(typeof(Character.MovementTypes.FourLegged))]
    [Opsive.Shared.StateSystem.AddState("Zoom", "da67cc4518129ec40bc4e49daeff5c3a")]
    public class Adventure : ThirdPerson
    {
        [Tooltip("The minimum and maximum yaw angle (in degrees).")]
        [MinMaxRange(-180, 180)] [SerializeField] protected MinMaxFloat m_YawLimit = new MinMaxFloat(-180, 180);
        [Tooltip("The speed in which the camera should rotate towards the yaw limit when out of bounds.")]
        [Range(0, 1)] [SerializeField] protected float m_YawLimitLerpSpeed = 0.7f;
        [Tooltip("Should the view type rotate with the character's rotation?")]
        [SerializeField] protected bool m_RotateWithCharacter;

        public MinMaxFloat YawLimit { get { return m_YawLimit; } set { m_YawLimit = value; } }
        public float YawLimitLerpSpeed { get { return m_YawLimitLerpSpeed; } set { m_YawLimitLerpSpeed = value; } }
        public bool RotateWithCharacter { get { return m_RotateWithCharacter; } set { m_RotateWithCharacter = value; } }

        /// <summary>
        /// Rotates the camera according to the horizontal and vertical movement values.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        /// <param name="immediateUpdate">Should the camera be updated immediately?</param>
        /// <returns>The updated rotation.</returns>
        public override Quaternion Rotate(float horizontalMovement, float verticalMovement, bool immediateUpdate)
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

            return base.Rotate(horizontalMovement, verticalMovement, immediateUpdate);
        }
    }
}