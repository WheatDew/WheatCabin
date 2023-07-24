/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    using UnityEngine;

    /// <summary>
    /// Starts the animator when the specified object enters the trigger.
    /// </summary>
    public class StartAnimatorTrigger : MonoBehaviour
    {
        [Tooltip("The name of the bool that should move the train.")]
        [SerializeField] protected string m_StartBoolParameterName;

        private Animator m_Animator;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Animator = GetComponentInParent<Animator>();
        }

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            var characterLocomotion = other.GetComponentInParent<Character.UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            m_Animator.SetBool(m_StartBoolParameterName, true);
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The object that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            var characterLocomotion = other.GetComponentInParent<Character.UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            m_Animator.SetBool(m_StartBoolParameterName, false);
        }
    }
}