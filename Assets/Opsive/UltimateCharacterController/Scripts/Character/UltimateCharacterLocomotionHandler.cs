/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Manages input handling for the locomotion component.
    /// </summary>
    public class UltimateCharacterLocomotionHandler : CharacterHandler
    {
        protected UltimateCharacterLocomotion m_CharacterLocomotion;
        private List<ActiveInputEvent> m_ActiveInputList;
        private bool m_InputEnabled;

        /// <summary>
        /// Internal method which initializes the default values.
        /// </summary>
        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            m_GameObject = gameObject;
            m_CharacterLocomotion = m_GameObject.GetCachedComponent<UltimateCharacterLocomotion>();
            m_PlayerInput = m_GameObject.GetCachedComponent<IPlayerInput>();

            if (m_PlayerInput != null) {
                if (m_PlayerInput is PlayerInputProxy) {
                    var proxy = m_PlayerInput as PlayerInputProxy;
                    m_PlayerInput = proxy.PlayerInput;
                }

                EventHandler.RegisterEvent<Ability, bool>(m_GameObject, "OnCharacterAbilityActive", OnAbilityActive);
                EventHandler.RegisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
                EventHandler.RegisterEvent<int, int>(m_GameObject, "OnItemSetManagerUpdateItemSet", OnUpdateItemSet);
                EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
                EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);
                EventHandler.RegisterEvent<bool>(m_GameObject, "OnEnableGameplayInput", OnEnableGameplayInput);
                m_InputEnabled = true;
            }
        }

        /// <summary>
        /// Returns the rotation input for the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        /// <param name="deltaYawRotation">Value specifying the number of degrees changed on the local yaw axis.</param>
        public override void GetRotationInput(float horizontalMovement, float forwardMovement, out float deltaYawRotation)
        {
            if (!enabled) {
                deltaYawRotation = 0;
                return;
            }

            var lookVector = m_PlayerInput.GetLookVector(true);
            deltaYawRotation = m_CharacterLocomotion.ActiveMovementType.GetDeltaYawRotation(horizontalMovement, forwardMovement, lookVector.x, lookVector.y);
        }

        /// <summary>
        /// Updates the input.
        /// </summary>
        private void Update()
        {
            UpdateAbilityInput();
        }

        /// <summary>
        /// Updates the input for the abilities and input events. Will start/stop abilities if the input is enabled.
        /// </summary>
        private void UpdateAbilityInput()
        {
            if (!m_InputEnabled) {
                return;
            }

            // Abilities can listen for their own input.
            if (m_ActiveInputList != null) {
                for (int i = 0; i < m_ActiveInputList.Count; ++i) {
                    // Execute the event as soon as the input type becomes true.
                    if (m_ActiveInputList[i].HasButtonEvent(m_PlayerInput)) {
                        ExecuteInputEvent(m_ActiveInputList[i].EventName);
                    } else if (m_ActiveInputList[i].HasAxisEvent()) {
                        ExecuteInputEvent(m_ActiveInputList[i].EventName, m_ActiveInputList[i].GetAxisValue(m_PlayerInput));
                    }
                }
            }

            // Update the input for both the regular abilities and item abilities.
            UpdateAbilityInput(m_CharacterLocomotion.Abilities);
            UpdateAbilityInput(m_CharacterLocomotion.ItemAbilities);
        }

        /// <summary>
        /// Updates the input for the specified abilities. Will start/stop abilities as necessary.
        /// </summary>
        private void UpdateAbilityInput(Ability[] abilities)
        {
            if (abilities != null) {
                // Try to start or stop the ability.
                for (int i = 0; i < abilities.Length; ++i) {
                    // The ability has to be enabled in order for it to be able to be stopped/started.
                    if (!abilities[i].Enabled) {
                        if (abilities[i].IsActive) {
                            TryStopAbility(abilities[i]);
                        }
                        continue;
                    }

                    var abilityStopped = false;
                    if (abilities[i].IsActive) {
                        // Stop the ability if it is already started and the input says to stop. 
                        if (abilities[i].CanInputStopAbility(m_PlayerInput)) {
                            abilityStopped = TryStopAbility(abilities[i]);
                        }
                    }
                    // Use a separate if statement because the ability may be stopping while able to receive multiple starts.
                    if (!abilityStopped || abilities[i].CanReceiveMultipleStarts) {
                        // Start the ability if it is not started and the input says to start. The ability may also be started if it is
                        // currently active and can receive multiple starts. This is useful for item abilities that can be activated again while
                        // they are still running (such as toggling an equipped item while waiting for the animation to do the equip).
                        if (abilities[i].CanInputStartAbility(m_PlayerInput)) {
                            TryStartAbility(abilities[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// An Ability has been activated or deactivated.
        /// </summary>
        /// <param name="ability">The Ability activated or deactivated.</param>
        /// <param name="active">Was the Ability activated?</param>
        private void OnAbilityActive(Ability ability, bool active)
        {
            // When an ability starts it may share the same input as another ability. Update the input on all of the other abilities so they don't respond to input
            // when they shouldn't.
            if (active) {
                CheckAbilityInput(m_CharacterLocomotion.Abilities);
            }
        }

        /// <summary>
        /// An ItemAbility has been activated or deactivated.
        /// </summary>
        /// <param name="itemAbility">The ItemAbility activated or deactivated.</param>
        /// <param name="active">Was the ItemAbility activated?</param>
        private void OnItemAbilityActive(ItemAbility itemAbility, bool active)
        {
            // When an ability starts it may share the same input as another ability. Update the input on all of the other abilities so they don't respond to input
            // when they shouldn't.
            if (active) {
                CheckAbilityInput(m_CharacterLocomotion.ItemAbilities);
            }
        }

        /// <summary>
        /// The ItemSet has changed.
        /// </summary>
        /// <param name="categoryIndex">The index of the changed category.</param>
        /// <param name="itemSetIndex">The index of the changed ItemSet.</param>
        private void OnUpdateItemSet(int categoryIndex, int itemSetIndex)
        {
            // The input may update after the ItemSet has been changed.
            CheckAbilityInput(m_CharacterLocomotion.ItemAbilities);
        }

        /// <summary>
        /// Ensures the ability input is up to date with the latest player input state.
        /// </summary>
        /// <param name="abilities">An array of abilities to check.</param>
        private void CheckAbilityInput(Ability[] abilities)
        {
            for (int i = 0; i < abilities.Length; ++i) {
                if (abilities[i].IsActive) {
                    continue;
                }

                abilities[i].CheckInput(m_PlayerInput);
            }
        }

        /// <summary>
        /// Register an input event which allows the ability to receive button callbacks while it is active.
        /// </summary>
        /// <param name="inputEvent">The input event object to register.</param>
        public virtual void RegisterInputEvent(ActiveInputEvent inputEvent)
        {
            if (m_ActiveInputList == null) {
                m_ActiveInputList = new List<ActiveInputEvent>();
            }
            m_ActiveInputList.Add(inputEvent);
        }

        /// <summary>
        /// Unregister the specified input event.
        /// </summary>
        /// <param name="inputEvent">The input event object to unregister.</param>
        public void UnregisterInputEvent(ActiveInputEvent inputEvent)
        {
            // The input list may be null when the object is being destroyed.
            if (m_ActiveInputList == null || inputEvent == null) {
                return;
            }
            m_ActiveInputList.Remove(inputEvent);
        }

        /// <summary>
        /// Tries to start the specified ability.
        /// </summary>
        /// <param name="ability">The ability to try to start.</param>
        /// <returns>True if the ability was started.</returns>
        protected virtual bool TryStartAbility(Ability ability)
        {
            return m_CharacterLocomotion.TryStartAbility(ability);
        }

        /// <summary>
        /// Tries to stop the specified ability.
        /// </summary>
        /// <param name="ability">The ability to try to stop.</param>
        /// <returns>True if the ability was stopped.</returns>
        protected virtual bool TryStopAbility(Ability ability)
        {
            return m_CharacterLocomotion.TryStopAbility(ability);
        }

        /// <summary>
        /// Executes the input event.
        /// </summary>
        /// <param name="eventName">The input event name.</param>
        protected virtual void ExecuteInputEvent(string eventName)
        {
            EventHandler.ExecuteEvent(m_GameObject, eventName);
        }

        /// <summary>
        /// Executes the axis input event.
        /// </summary>
        /// <param name="eventName">The input event name.</param>
        /// <param name="axisValue">The value of the axis.</param>
        protected virtual void ExecuteInputEvent(string eventName, float axisValue)
        {
            EventHandler.ExecuteEvent<float>(m_GameObject, eventName, axisValue);
        }

        /// <summary>
        /// The character has died. Disable the handler.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            enabled = !m_PlayerInput.DisableOnDeath;
        }

        /// <summary>
        /// The character has respawned. Enable the handler.
        /// </summary>
        protected virtual void OnRespawn()
        {
            enabled = true;
        }

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        private void OnEnableGameplayInput(bool enable)
        {
            enabled = m_InputEnabled = enable;
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_PlayerInput != null) {
                EventHandler.UnregisterEvent<Ability, bool>(m_GameObject, "OnCharacterAbilityActive", OnAbilityActive);
                EventHandler.UnregisterEvent<ItemAbility, bool>(m_GameObject, "OnCharacterItemAbilityActive", OnItemAbilityActive);
                EventHandler.UnregisterEvent<int, int>(m_GameObject, "OnItemSetManagerUpdateItemSet", OnUpdateItemSet);
                EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
                EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
                EventHandler.UnregisterEvent<bool>(m_GameObject, "OnEnableGameplayInput", OnEnableGameplayInput);
            }
        }
    }
}