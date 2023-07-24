/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.AnimatorAudioStates
{
    using Opsive.Shared.Audio;
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Character;
    using UnityEngine;

    /// <summary>
    /// The AnimatorAudioStateSet represets a set of animation parameters and audio clips that should be played together.
    /// </summary>
    [System.Serializable]
    public class AnimatorAudioStateSet
    {
        /// <summary>
        /// Contains a single animator and audio clip state.
        /// </summary>
        [System.Serializable]
        public class AnimatorAudioState : StateObject
        {
            [Tooltip("Is the AnimatorAudioState enabled?")]
            [SerializeField] protected bool m_Enabled = true;
            [Tooltip("Can the state be selected when the character is moving?")]
            [SerializeField] protected bool m_AllowDuringMovement = true;
            [Tooltip("Does the state require the character to be grounded?")]
            [SerializeField] protected bool m_RequireGrounded;
            [Tooltip("The name of the state that should be active when the animation is playing.")]
            [SerializeField] [StateName] protected string m_StateName;
            [Tooltip("The value of the animator's Item Substate Index parameter.")]
            [SerializeField] protected int m_ItemSubstateIndex;
            [Tooltip("Contains an array of AudioClips.")]
            [SerializeField] protected AudioClipSet m_AudioClipSet = new AudioClipSet();

            public bool Enabled { get { return m_Enabled; } set { m_Enabled = value; } }
            public bool AllowDuringMovement { get { return m_AllowDuringMovement; } set { m_AllowDuringMovement = value; } }
            public bool RequireGrounded { get { return m_RequireGrounded; } set { m_RequireGrounded = value; } }
            public string StateName { get { return m_StateName; } set { m_StateName = value; } }
            public int ItemSubstateIndex { get { return m_ItemSubstateIndex; } set { m_ItemSubstateIndex = value; } }
            public AudioClipSet AudioClipSet { get { return m_AudioClipSet; } set { m_AudioClipSet = value; } }

            /// <summary>
            /// Default constructor.
            /// </summary>
            public AnimatorAudioState() { }

            /// <summary>
            /// Constructor with one parameter.
            /// </summary>
            /// <param name="itemSubstateIndex">The value of the animator's Item Substate Index parameter.</param>
            public AnimatorAudioState(int itemSubstateIndex)
            {
                m_ItemSubstateIndex = itemSubstateIndex;
            }

            /// <summary>
            /// Plays the audio clip with a random set index.
            /// </summary>
            /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
            public void PlayAudioClip(GameObject gameObject)
            {
                m_AudioClipSet.PlayAudioClip(gameObject);
            }
        }

        [Tooltip("The selector used for determining the next state.")]
        [SerializeReference] protected AnimatorAudioStateSelector m_AnimatorAudioStateSelector;
        [Tooltip("An array of possible states for the animator parameter and audio clip.")]
        [SerializeField] protected AnimatorAudioState[] m_States;

        public AnimatorAudioStateSelector AnimatorAudioStateSelector { get => m_AnimatorAudioStateSelector; set => m_AnimatorAudioStateSelector = value; }
        public AnimatorAudioState[] States { get => m_States; set => m_States = value; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AnimatorAudioStateSet()
        {
            m_AnimatorAudioStateSelector = System.Activator.CreateInstance(typeof(Sequence)) as AnimatorAudioStateSelector;
        }

        /// <summary>
        /// Constructor with one parameter.
        /// </summary>
        /// <param name="animatorAudioStateSelector">The animator audio state selector.</param>
        public AnimatorAudioStateSet(AnimatorAudioStateSelector animatorAudioStateSelector)
        {
            if (animatorAudioStateSelector == null) {
                m_AnimatorAudioStateSelector = System.Activator.CreateInstance(typeof(Sequence)) as AnimatorAudioStateSelector;
            } else {
                m_AnimatorAudioStateSelector = animatorAudioStateSelector;
            }
        }
        
        /// <summary>
        /// Constructor with one parameter.
        /// </summary>
        /// <param name="itemSubstateParameter">The value of the animator's Item Substate Index parameter.</param>
        /// <param name="animatorAudioStateSelector">The animator audio state selector.</param>
        public AnimatorAudioStateSet(int itemSubstateParameter, AnimatorAudioStateSelector animatorAudioStateSelector = null) : this(animatorAudioStateSelector)
        {
            if (m_States == null) {
                m_States = new AnimatorAudioState[] { new AnimatorAudioState(itemSubstateParameter) };
            }
        }
        
        /// <summary>
        /// Constructor with one parameter.
        /// </summary>
        /// <param name="animatorAudioStates">The starting animator audio states.</param>
        /// <param name="animatorAudioStateSelector">The animator audio state selector.</param>
        public AnimatorAudioStateSet(AnimatorAudioState[] animatorAudioStates, AnimatorAudioStateSelector animatorAudioStateSelector = null) : this(animatorAudioStateSelector)
        {
            if (m_States == null) {
                m_States = animatorAudioStates;
            }
        }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        /// <param name="characterItem">A reference to the item that the state belongs to.</param>
        /// <param name="characterLocomotion">A reference to the character that the state belongs to.</param>
        public void Awake(CharacterItem characterItem, UltimateCharacterLocomotion characterLocomotion)
        {
            m_AnimatorAudioStateSelector.Initialize(characterItem.gameObject, characterLocomotion, characterItem, m_States);

            for (int i = 0; i < m_States.Length; ++i) {
                m_States[i].Initialize(characterItem.gameObject);
            }
        }

        /// <summary>
        /// Returns the current state index of the selector. -1 indicates this index is not set by the class.
        /// </summary>
        /// <returns>The current state index.</returns>
        public int GetStateIndex()
        {
            if (m_AnimatorAudioStateSelector == null || m_States.Length == 0) {
                return -1;
            }
            return m_AnimatorAudioStateSelector.GetStateIndex();
        }

        /// <summary>
        /// Starts or stops the state selection. Will activate or deactivate the state with the name specified within the AnimatorAudioState.
        /// </summary>
        /// <param name="start">Is the object starting?</param>
        public void StartStopStateSelection(bool start)
        {
            m_AnimatorAudioStateSelector.StartStopStateSelection(start);
        }

        /// <summary>
        /// Returns the Item Substate Index parameter value. -1 indicates this value is not set by the class.
        /// </summary>
        /// <returns>The current Item Substate Index parameter value.</returns>
        public int GetItemSubstateIndex()
        {
            var stateIndex = GetStateIndex();
            if (stateIndex == -1 || stateIndex >= m_States.Length) {
                return -1;
            }
            return m_States[stateIndex].ItemSubstateIndex + m_AnimatorAudioStateSelector.GetAdditionalItemSubstateIndex();
        }

        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        /// <param name="gameObject">The GameObject that is playing the audio clip.</param>
        public void PlayAudioClip(GameObject gameObject)
        {
            if (m_AnimatorAudioStateSelector == null || m_States.Length == 0) {
                return;
            }
            var stateIndex = m_AnimatorAudioStateSelector.GetStateIndex();
            if (stateIndex == -1 || stateIndex >= m_States.Length) {
                return;
            }
            m_States[stateIndex].PlayAudioClip(gameObject);
        }

        /// <summary>
        /// Moves to the next state of the selector.
        /// </summary>
        public bool NextState()
        {
            if (m_AnimatorAudioStateSelector == null) {
                return false;
            }

            return m_AnimatorAudioStateSelector.NextState();
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        public void OnDestroy()
        {
            if (m_AnimatorAudioStateSelector == null) {
                return;
            }

            m_AnimatorAudioStateSelector.OnDestroy();
        }
    }
}