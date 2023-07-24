/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.UI
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    /// <summary>
    /// Adds events to the base Unity Toggle.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class ToggleExtension : MonoBehaviour
    {
        [Tooltip("Event when toggled on.")]
        [SerializeField] private UnityEvent m_OnToggleOn;
        [Tooltip("Event when toggled off.")]
        [SerializeField] private UnityEvent m_OnToggleOff;
        
        private Toggle m_Toggle;
        
        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public void Awake()
        {
            m_Toggle = GetComponent<Toggle>();
            m_Toggle.onValueChanged.AddListener(HandleOnValueChanged);
        }

        /// <summary>
        /// Handles the toggle changing value.
        /// </summary>
        /// <param name="on">Was the toggle turned on?</param>
        private void HandleOnValueChanged(bool on)
        {
            if (on) {
                m_OnToggleOn?.Invoke();
            } else {
                m_OnToggleOff?.Invoke();
            }
        }
    }
}