/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.UI
{
    using Opsive.Shared.StateSystem;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// A component used to toggle character states on and off
    /// </summary>
    public class CharacterStateToggle : MonoBehaviour
    {
        [Tooltip("Toggled on or off.")]
        [SerializeField] private bool m_On;
        [Tooltip("The toggle UI object.")]
        [SerializeField] private Toggle m_Toggle;
        [Tooltip("The character witht he state to enable/disable.")]
        [SerializeField] private GameObject m_Character;
        [Tooltip("States to enable.")]
        [StateName]
        [SerializeField] private string[] m_EnableStates;
        [Tooltip("States to disable")]
        [StateName]
        [SerializeField] private string[] m_DisableStates;

        /// <summary>
        /// Initailize the default values.
        /// </summary>
        private void Start()
        {
            if (m_Character == null) {
                var demoManager = Object.FindObjectOfType<DemoManager>();
                m_Character = demoManager.Character;
            }

            if (m_Toggle != null) {
                m_Toggle.isOn = m_On;
                m_Toggle.onValueChanged.AddListener(ToggleValueChanged);
            }

            Refresh();
        }

        /// <summary>
        /// Refresh on enable to keep the UI in sync.
        /// </summary>
        private void OnEnable()
        {
            if (m_Toggle != null) {
                m_Toggle.isOn = m_On;
                m_Toggle.onValueChanged.AddListener(ToggleValueChanged);
            }
            Refresh();
        }

        /// <summary>
        /// The toggle UI value changed.
        /// </summary>
        /// <param name="on">The new value.</param>
        private void ToggleValueChanged(bool on)
        {
            m_On = on;
            Refresh();
        }

        /// <summary>
        /// Refresh the states.
        /// </summary>
        public void Refresh()
        {
            if (m_Character == null) { return; }

            for (int i = 0; i < m_EnableStates.Length; i++) {
                StateManager.SetState(m_Character, m_EnableStates[i], m_On);
            }
            for (int i = 0; i < m_DisableStates.Length; i++) {
                StateManager.SetState(m_Character, m_DisableStates[i], !m_On);
            }
        }

        /// <summary>
        /// toggle the state.
        /// </summary>
        public void Toggle()
        {
            if (m_Toggle != null) {
                m_Toggle.isOn = !m_Toggle.isOn;
            } else {
                m_On = !m_On;
                Refresh();
            }
        }

        /// <summary>
        /// Toggle the state on or off.
        /// </summary>
        /// <param name="on">Toggle it on or off?</param>
        public void Toggle(bool on)
        {
            if (m_Toggle != null) {
                m_Toggle.isOn = on;
            } else {
                m_On = on;
                Refresh();
            }
        }
    }
}
