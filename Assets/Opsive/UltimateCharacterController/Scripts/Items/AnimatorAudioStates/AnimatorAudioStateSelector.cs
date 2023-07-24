/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Character;
    using UnityEngine;

    /// <summary>
    /// The AnimatorAudioState will return a Item Substate Index parameter based on the object's state. 
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public abstract class AnimatorAudioStateSelector
    {
        protected CharacterItem m_CharacterItem;
        protected GameObject m_Character;
        protected UltimateCharacterLocomotion m_CharacterLocomotion;
        protected AnimatorAudioStateSet.AnimatorAudioState[] m_States;

        /// <summary>
        /// Initializes the selector.
        /// </summary>
        /// <param name="gameObject">The GameObject that the state belongs to.</param>
        /// <param name="characterLocomotion">The character that the state belongs to.</param>
        /// <param name="characterItem">The item that the state belongs to.</param>
        /// <param name="states">The states which are being selected.</param>
        public virtual void Initialize(GameObject gameObject, UltimateCharacterLocomotion characterLocomotion, CharacterItem characterItem, AnimatorAudioStateSet.AnimatorAudioState[] states)
        {
            m_CharacterItem = characterItem;
            m_Character = characterLocomotion.gameObject;
            m_CharacterLocomotion = characterLocomotion;
            m_States = states;
        }

        /// <summary>
        /// Starts or stops the state selection.
        /// </summary>
        /// <param name="start">Is the object starting?</param>
        public virtual void StartStopStateSelection(bool start) 
        {
            // Activate or deactivate the state.
            var stateIndex = GetStateIndex();
            if (stateIndex == -1 || stateIndex >= m_States.Length) {
                return;
            }

            ChangeStates(start ? -1 : stateIndex, start ? stateIndex : -1);
        }

        /// <summary>
        /// Returns the current state index. -1 indicates this index is not set by the class.
        /// </summary>
        /// <returns>The current state index.</returns>
        public virtual int GetStateIndex()
        {
            return -1;
        }

        /// <summary>
        /// Moves to the next state.
        /// </summary>
        /// <returns>Was the state changed successfully?</returns>
        public virtual bool NextState() { return true; }

        /// <summary>
        /// Changes states from the fromIndex to the toIndex.
        /// </summary>
        /// <param name="fromIndex">The original state index.</param>
        /// <param name="toIndex">The new state index.</param>
        protected void ChangeStates(int fromIndex, int toIndex)
        {
            if (fromIndex != -1) {
                var stateName = m_States[fromIndex].StateName;
                if (!string.IsNullOrEmpty(stateName)) {
                    StateManager.SetState(m_Character, stateName, false);
                }
            }
            if (toIndex != -1) {
                var stateName = m_States[toIndex].StateName;
                if (!string.IsNullOrEmpty(stateName)) {
                    StateManager.SetState(m_Character, stateName, true);
                }
            }
        }

        /// <summary>
        /// Is the state at the specified index valid?
        /// </summary>
        /// <param name="index">The index to check the state of.</param>
        /// <returns>True if the state at the specified index is valid.</returns>
        protected bool IsStateValid(int index) { 
            return (m_States[index].AllowDuringMovement || !m_CharacterLocomotion.Moving)
                   && (!m_States[index].RequireGrounded || m_CharacterLocomotion.Grounded); 
        }

        /// <summary>
        /// Returns an additional value that should be added to the Item Substate Index.
        /// </summary>
        /// <returns>An additional value that should be added to the Item Substate Index.</returns>
        public virtual int GetAdditionalItemSubstateIndex() { return 0; }

        /// <summary>
        /// The selector has been destroyed.
        /// </summary>
        public virtual void OnDestroy() { }
    }
}