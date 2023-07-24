/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo
{
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Enables the objects within the Objects array when the character enters the trigger.
    /// </summary>
    public class ObjectEnabler : MonoBehaviour
    {
        [Tooltip("Specifies the objects that should be enabled when in the trigger zone.")]
        [SerializeField] protected GameObject[] m_Objects;
        [Tooltip("Disable on start")]
        [SerializeField] protected bool m_DisableOnStart;

        private GameObject m_ActiveObject;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Start()
        {
            if(m_DisableOnStart == false){ return; } 
            
            for (int i = 0; i < m_Objects.Length; ++i) {
                m_Objects[i].SetActive(false);
            }
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

            m_ActiveObject = other.gameObject;

            DoEnableObject(true);
        }

        /// <summary>
        /// Enable the gameobjects in the list.
        /// </summary>
        public void DoEnableObject(bool enable)
        {
            for (int i = 0; i < m_Objects.Length; ++i) {
                EventHandler.ExecuteEvent(m_Objects[i], "OnRespawn");
                m_Objects[i].SetActive(enable);
            }
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (m_ActiveObject != other.gameObject) {
                return;
            }
            
            m_ActiveObject = null;
            
            DoEnableObject(false);
        }
    }
}