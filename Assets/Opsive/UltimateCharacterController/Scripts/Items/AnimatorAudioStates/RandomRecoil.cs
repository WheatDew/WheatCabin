/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    using Opsive.UltimateCharacterController.Character;
    using UnityEngine;

    /// <summary>
    /// The RandomRecoil state will move from one state to another in a random order.
    /// </summary>
    public class RandomRecoil : RecoilAnimatorAudioStateSelector
    {
        private int m_CurrentIndex;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RandomRecoil() : base() { }
        
        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="blockedRecoilItemSubstateIndex">The blocked recoil item substate index.</param>
        public RandomRecoil(int blockedRecoilItemSubstateIndex) : base(blockedRecoilItemSubstateIndex) { }
        
        /// <summary>
        /// Initializes the selector.
        /// </summary>
        /// <param name="gameObject">The GameObject that the state belongs to.</param>
        /// <param name="characterLocomotion">The character that the state bleongs to.</param>
        /// <param name="characterItem">The item that the state belongs to.</param>
        /// <param name="states">The states which are being selected.</param>
        public override void Initialize(GameObject gameObject, UltimateCharacterLocomotion characterLocomotion, CharacterItem characterItem, AnimatorAudioStateSet.AnimatorAudioState[] states)
        {
            base.Initialize(gameObject, characterLocomotion, characterItem, states);

            // Call next state so the index will be initialized to a random value.
            NextState();
        }

        /// <summary>
        /// Returns the current state index. -1 indicates this index is not set by the class.
        /// </summary>
        /// <returns>The current state index.</returns>
        public override int GetStateIndex()
        {
            return m_CurrentIndex;
        }

        /// <summary>
        /// Moves to the next state.
        /// </summary>
        /// <returns>Was the state changed successfully?</returns>
        public override bool NextState()
        {
            var lastIndex = m_CurrentIndex;
            var count = 0;
            var size = m_States.Length;
            if (size == 0) {
                return false;
            }
            do {
                m_CurrentIndex = UnityEngine.Random.Range(0, size);
                ++count;
            } while ((!IsStateValid(m_CurrentIndex) || !m_States[m_CurrentIndex].Enabled) && count <= size);
            var stateChange = count <= size;
            if (stateChange) {
                ChangeStates(lastIndex, m_CurrentIndex);
            }
            return stateChange;
        }
    }
}