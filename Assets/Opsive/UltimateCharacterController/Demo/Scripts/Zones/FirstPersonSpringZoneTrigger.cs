/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Trigger for when the character enters a spring zone.
    /// </summary>
    public class FirstPersonSpringZoneTrigger : MonoBehaviour
    {
        [Tooltip("The LayerMask that the trigger can set the state of.")]
        [SerializeField] protected LayerMask m_LayerMask = 1 << LayerManager.Character;
        [Tooltip("The type of spring that should be activated.")]
        [SerializeField] protected FirstPersonSpringZone.SpringType m_SpringType = FirstPersonSpringZone.SpringType.Modern;

        private FirstPersonSpringZone m_SpringZone;

        /// <summary>
        /// Initailizes teh default values.
        /// </summary>
        private void Awake()
        {
            m_SpringZone = GetComponentInParent<FirstPersonSpringZone>();
        }

        /// <summary>
        /// The other collider has entered the trigger.
        /// </summary>
        /// <param name="other">The collider which entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }

            m_SpringZone.ActivateSpring(m_SpringType);
        }

        /// <summary>
        /// The other collider has exited the trigger.
        /// </summary>
        /// <param name="other">The collider which exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }

            m_SpringZone.ActivateSpring(FirstPersonSpringZone.SpringType.None);
        }
    }
}