/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    using UnityEngine;

    /// <summary>
    /// The RandomRecoil state will move from one state to another in a sequence order.
    /// </summary>
    public class SequenceRecoil : RecoilAnimatorAudioStateSelector
    {
        [Tooltip("Resets the index back to the start after the specified delay. Set to -1 to never reset.")]
        [SerializeField] protected float m_ResetDelay = -1;

        public float ResetDelay { get { return m_ResetDelay; } set { m_ResetDelay = value; } }

        private int m_CurrentIndex = -1;
        private float m_LastUsedTime = -1;

        /// <summary>
        /// Starts or stops the state selection.
        /// </summary>
        /// <param name="start">Is the object starting?</param>
        public override void StartStopStateSelection(bool start)
        {
            base.StartStopStateSelection(start);

            // The Sequence task can reset which index is returned if the next state is selected too slowly. 
            if (start && m_ResetDelay != -1 && m_LastUsedTime != -1 && m_LastUsedTime + m_ResetDelay < Time.time) {
                m_CurrentIndex = -1;
            }
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
            m_LastUsedTime = Time.time;
            var count = 0;
            var size = m_States.Length;
            if (size == 0) {
                return false;
            }
            do {
                m_CurrentIndex = (m_CurrentIndex + 1) % size;
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