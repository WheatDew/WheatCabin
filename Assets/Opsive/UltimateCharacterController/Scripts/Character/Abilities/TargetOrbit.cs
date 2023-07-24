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
    /// Orbits around the target when the character moves.
    /// </summary>
    [DefaultStartType(AbilityStartType.Automatic)]
    [DefaultStopType(AbilityStopType.Automatic)]
    public class TargetOrbit : Ability
    {
        [Tooltip("Should the ability use the assist aim target?")]
        [SerializeField] protected bool m_UseAssistAimTarget;
        [Tooltip("Specifies the target transform if the aim assist target is not used.")]
        [SerializeField] protected Transform m_Target;

        public bool UseAssistAimTarget { get { return m_UseAssistAimTarget; } set { m_UseAssistAimTarget = value; } }

        private AssistAim m_AssistAim;

        public override bool IsConcurrent { get { return true; } }

        private Transform Target
        {
            get
            {
                Transform target = null;
                if (m_UseAssistAimTarget && m_AssistAim != null && m_AssistAim.Target != null) {
                    target = m_AssistAim.Target;
                } else {
                    target = m_Target;
                }
                return target;
            }
        }
        
        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_AssistAim = m_CharacterLocomotion.GetAbility<AssistAim>();
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

            return Target != null;
        }

        /// <summary>
        /// Stops the ability if the target is null.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (Target == null) {
                StopAbility();
            }
        }

        /// <summary>
        /// Verify the position values. Called immediately before the position is applied.
        /// </summary>
        public override void ApplyPosition()
        {
            // The character's z relative direction can change when input is applied to the y input vector. It can also change when there
            // is no input, during this time the velocity is changing the position.
            if (Mathf.Abs(m_CharacterLocomotion.InputVector.y) > 0.0001f || m_CharacterLocomotion.RawInputVector.sqrMagnitude == 0) {
                return;
            }

            // The character's z relative direction should not change when the character is orbiting around the target.
            var targetPosition = m_Transform.position + m_CharacterLocomotion.DesiredMovement;
            var rotation = Quaternion.LookRotation((Target.position - targetPosition).normalized, m_CharacterLocomotion.Up);
            var direction = MathUtility.InverseTransformDirection(m_CharacterLocomotion.DesiredMovement, rotation);
            direction.z = 0;
            m_CharacterLocomotion.DesiredMovement = MathUtility.TransformDirection(direction, rotation);
        }
    }
}