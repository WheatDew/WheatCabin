/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo
{
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using UnityEngine;

    /// <summary>
    /// A component that enables or disables ItemSets using the rules.
    /// </summary>
    public class ActivateDeactivateCharacterStates : MonoBehaviour
    {
        [Tooltip("Do the state Transition on Enable?")]
        [SerializeField] private bool m_OnEnable = true;
        [Tooltip("Do the state Transition on Disable?")]
        [SerializeField] private bool m_OnDisable = true;
        [Tooltip("Do switch activate and deactivate on Disable?")]
        [SerializeField] private bool m_InverseActivationOnDisable = true;
        [Tooltip("The StateObject, defaults to the character.")]
        [SerializeField] private GameObject m_Character;
        [Tooltip("The states to activate on the character.")]
        [StateName] [SerializeField] protected string[] m_StateToActivate;
        [Tooltip("The states to deactivate on the character.")]
        [StateName] [SerializeField] protected string[] m_StateToDeactivate;

        protected bool m_Initialized = false;
        
        /// <summary>
        /// Initialize the component.
        /// </summary>
        private void Start()
        {
            Initialize(false);
            
            // This ensures the character is initialized before the state is set.
            Scheduler.Schedule(0.1f, OnEnable);
        }

        /// <summary>
        /// Invoke the function if OnEnable is true.
        /// </summary>
        private void OnEnable()
        {
            if (!m_OnEnable) { return; }

            DoEnableDisableStates(true);
        }

        /// <summary>
        /// Invoke the function if OnDisable is true.
        /// </summary>
        private void OnDisable()
        {
            if (!m_OnDisable) { return; }

            DoEnableDisableStates(!m_InverseActivationOnDisable);
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        /// <param name="force">Force the initialization?</param>
        private void Initialize(bool force)
        {
            if (m_Initialized && !force) {
                return;
            }

            if (m_Character == null) {
                m_Character = FindObjectOfType<DemoManager>()?.Character;
            }

            if (m_Character == null) {
                return;
            }
            
            m_Initialized = true;
        }
        
        /// <summary>
        /// Do add and equip items specified in the fields.
        /// </summary>
        public void DoEnableDisableStates(bool enable)
        {
            Initialize(false);
            
            if (m_Character == null) {
                return;
            }

            for (int i = 0; i < m_StateToActivate.Length; i++) {
                StateManager.SetState(m_Character, m_StateToActivate[i], enable);
            }

            for (int i = 0; i < m_StateToDeactivate.Length; i++) {
                StateManager.SetState(m_Character, m_StateToDeactivate[i], !enable);
            }
        }
    }
}