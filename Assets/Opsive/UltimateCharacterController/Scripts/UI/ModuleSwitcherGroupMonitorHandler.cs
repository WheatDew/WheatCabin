/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using UnityEngine;

    /// <summary>
    /// The handler used to control the ModuleSwitcherGroupMonitor with inputs.
    /// </summary>
    public class ModuleSwitcherGroupMonitorHandler : CharacterMonitor
    {
        [Tooltip("The module switcher group monitor to control with the input.")]
        [SerializeField] private ModuleSwitcherGroupMonitor m_ModuleSwitcherGroupMonitor;
        [Tooltip("The input to select the previous ModuleSwitchMonitor.")]
        [SerializeField] private string m_SelectPreviousMonitorInput = "PreviousModuleSwitcherGroup";
        [Tooltip("The input to select the next ModuleSwitchMonitor.")]
        [SerializeField] private string m_SelectNextMonitorInput = "NextModuleSwitcherGroup";
        [Tooltip("The input to switch to the previous switch state.")]
        [SerializeField] private string m_SwitchToPreviousInput = "PreviousModuleSwitchState";
        [Tooltip("The input to switch to the next switch state.")]
        [SerializeField] private string m_SwitchToNextInput = "NextModuleSwitchState";

        public ModuleSwitcherGroupMonitor ModuleSwitcherGroupMonitor { get => m_ModuleSwitcherGroupMonitor; set => m_ModuleSwitcherGroupMonitor = value; }
        public string SelectPreviousMonitorInput { get => m_SelectPreviousMonitorInput; set => m_SelectPreviousMonitorInput = value; }
        public string SelectNextMonitorInput { get => m_SelectNextMonitorInput; set => m_SelectNextMonitorInput = value; }
        public string SwitchToPreviousInput { get => m_SwitchToPreviousInput; set => m_SwitchToPreviousInput = value; }
        public string SwitchToNextInput { get => m_SwitchToNextInput; set => m_SwitchToNextInput = value; }
        public IPlayerInput PlayerInput { get => m_PlayerInput; set => m_PlayerInput = value; }

        private IPlayerInput m_PlayerInput;
        private bool m_AllowGameplayInput;

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character == character) {
                return;
            }
            
            enabled = character != null;

            if (m_Character != null) {
                EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
                EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnEnableGameplayInput", OnEnableGameplayInput);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterActivate", OnActivate);
            }
            
            base.OnAttachCharacter(character);
            
            if (character == null) {
                return;
            }
            
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(character, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(character, "OnRespawn", OnRespawn);
            EventHandler.RegisterEvent<bool>(character, "OnEnableGameplayInput", OnEnableGameplayInput);
            EventHandler.RegisterEvent<bool>(character, "OnCharacterActivate", OnActivate);
            m_AllowGameplayInput = true;
            m_PlayerInput = character.GetCachedComponent<IPlayerInput>();
            enabled = character.activeInHierarchy;
        }
        
        /// <summary>
        /// Handles the player input.
        /// </summary>
        private void Update()
        {
            if(m_AllowGameplayInput == false || m_PlayerInput == null ){ return;}

            if (m_PlayerInput.GetButtonDown(m_SelectPreviousMonitorInput)) {
                m_ModuleSwitcherGroupMonitor.SelectPreviousMonitor();
            }
            
            if (m_PlayerInput.GetButtonDown(m_SelectNextMonitorInput)) {
                m_ModuleSwitcherGroupMonitor.SelectNextMonitor();
            }
            
            if (m_PlayerInput.GetButtonDown(m_SwitchToPreviousInput)) {
                m_ModuleSwitcherGroupMonitor.GetSelectedSwitcherMonitor()?.DoSwitchPrevious();
            }
            
            if (m_PlayerInput.GetButtonDown(m_SwitchToNextInput)) {
                m_ModuleSwitcherGroupMonitor.GetSelectedSwitcherMonitor()?.DoSwitchNext();
            }
        }
        
        /// <summary>
        /// The character has died. Disable the component.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            enabled = false;
        }

        /// <summary>
        /// The character has respawned. Enable the component.
        /// </summary>
        private void OnRespawn()
        {
            enabled = true;
        }

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        private void OnEnableGameplayInput(bool enable)
        {
            m_AllowGameplayInput = enable;
            enabled = m_AllowGameplayInput && m_Character != null && m_Character.activeInHierarchy;
        }

        /// <summary>
        /// The character has been activated or deactivated.
        /// </summary>
        /// <param name="activate">True if the character has been activated.</param>
        private void OnActivate(bool activate)
        {
            enabled = m_AllowGameplayInput && m_Character != null && activate;
        }
    }
    
    
}