/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.UI
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Shows the specified text when the character has entered the tigger.
    /// </summary>
    public class DemoTextTrigger : MonoBehaviour
    {
        [Tooltip("The header text.")]
        [SerializeField] protected string m_HeaderText;
        [Tooltip("The description text.")]
        [Multiline] [SerializeField] protected string m_DescriptionText;

        private DemoManager m_DemoManager;
        private GameObject m_ActiveObject;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_DemoManager = Object.FindObjectOfType<DemoManager>(true);
        }

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (m_ActiveObject != null || !MathUtility.InLayerMask(other.gameObject.layer, 1 << LayerManager.Character)) {
                return;
            }

            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null) {
                return;
            }

            m_DemoManager.ShowText(m_HeaderText, m_DescriptionText, true);
            m_ActiveObject = characterLocomotion.gameObject;
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>(true);
            if (characterLocomotion == null) {
                return;
            }

            m_ActiveObject = null;
            m_DemoManager.ShowText(m_HeaderText, m_DescriptionText, false);
        }
    }
}