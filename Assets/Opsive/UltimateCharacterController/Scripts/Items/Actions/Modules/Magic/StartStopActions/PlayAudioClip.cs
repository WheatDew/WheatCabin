/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.StartStopActions
{
    using Opsive.Shared.Audio;
    using UnityEngine;

    /// <summary>
    /// Plays an audio clip.
    /// </summary>
    [System.Serializable]
    public class PlayAudioClip : MagicStartStopModule
    {
        [Tooltip("The AudioClip that should be played. A random AudioClip will be selected.")]
        [SerializeField] protected AudioClipSet m_AudioClipSet;
        [Tooltip("Plays the AudioClip at the origin. If the value is false the character position will be used.")]
        [SerializeField] protected bool m_PlayAtOrigin = true;
        [Tooltip("Should the AudioClip loop? This is not used if an AudioConfig is specified.")]
        [SerializeField] protected bool m_Loop;

        public AudioClipSet AudioClipSet { get { return m_AudioClipSet; } set { m_AudioClipSet = value; } }
        public bool PlayAtOrigin { get { return m_PlayAtOrigin; } set { m_PlayAtOrigin = value; } }
        public bool Loop { get { return m_Loop; } set { m_Loop = value; } }
        
        private AudioSource m_AudioSource;

        /// <summary>
        /// The action has started.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void Start(MagicUseDataStream useDataStream)
        {
            var origin = useDataStream.CastData.CastOrigin;
            
            if (m_AudioSource != null) {
                return;
            }
            
            var playPosition = m_PlayAtOrigin ? origin.position : CharacterTransform.position;
            m_AudioSource = m_AudioClipSet.PlayAtPosition(playPosition).AudioSource;
            
            if (m_AudioSource != null) {
                m_AudioSource.loop = m_Loop;
            }
        }

        /// <summary>
        /// The action has stopped.
        /// </summary>
        public override void Stop(MagicUseDataStream useDataStream)
        {
            if (m_AudioSource != null) {
                m_AudioSource.Stop();
                m_AudioSource = null;
            }
        }
    }
}