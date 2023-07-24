/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Rotates the character towards the target. If the character is using root motion then the speed is determined by the Animator.
    /// If the character is not using root motion then the speed is determined by the Ultimate Character Locomotion's Motor Rotation Speed field.
    /// </summary>
    public class RotateTowards : Ability
    {
        [Tooltip("The object that the character should rotate towards.")]
        [SerializeField] protected Transform m_Target;

        [Shared.Utility.NonSerialized] public Transform Target { get { return m_Target; } set { m_Target = value; if (IsActive && m_Target == null) StopAbility(); } }

        /// <summary>
        /// Can the Ability start.
        /// </summary>
        /// <returns>True if the ability can start.</returns>
        public override bool CanStartAbility()
        {
            return m_Target != null;
        }

        /// <summary>
        /// Update the controller's rotation values.
        /// </summary>
        public override void UpdateRotation()
        {
            // Rotate towards the target.
            var lookDirection = m_Target.position - m_Transform.position;
            var rotation = m_Transform.rotation;
            var localLookDirection = MathUtility.InverseTransformDirection(lookDirection, rotation);
            localLookDirection.y = 0;
            lookDirection = MathUtility.TransformDirection(localLookDirection, rotation);
            var targetRotation = Quaternion.LookRotation(lookDirection, rotation * Vector3.up);
            targetRotation = Quaternion.Slerp(rotation, targetRotation, m_CharacterLocomotion.MotorRotationSpeed * Time.deltaTime);
            m_CharacterLocomotion.DesiredRotation = MathUtility.InverseTransformQuaternion(m_Transform.rotation, targetRotation);
        }
    }
}