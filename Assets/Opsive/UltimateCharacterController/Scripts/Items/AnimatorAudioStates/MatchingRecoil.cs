/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    using UnityEngine;

    /// <summary>
    /// The MatchingRecoil state will return the same state index as what was retrieved by the use state selector.
    /// </summary>
    public class MatchingRecoil : RecoilAnimatorAudioStateSelector
    {
        /// <summary>
        /// Returns the current state index. -1 indicates this index is not set by the class.
        /// </summary>
        /// <returns>The current state index.</returns>
        public override int GetStateIndex()
        {
            return m_UseStateIndex;
        }
    }
}