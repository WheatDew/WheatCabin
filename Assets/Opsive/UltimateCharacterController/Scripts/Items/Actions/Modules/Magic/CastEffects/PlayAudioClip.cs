/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.CastEffects
{
    using Opsive.Shared.Audio;
    using Opsive.Shared.Game;
    using UnityEngine;

    /// <summary>
    /// Plays an audio clip when the cast is performed.
    /// </summary>
    [System.Serializable]
    public class PlayAudioClip : MagicMultiTargetCastEffectModule
    {
        [Tooltip("The AudioClip that should be played. A random AudioClip will be selected.")]
        [SerializeField] protected AudioClipSet m_AudioClipSet;
        [Tooltip("Plays the AudioClip at the origin. If the value is false the character position will be used.")]
        [SerializeField] protected bool m_PlayAtOrigin = true;
        [Tooltip("Should the AudioClip loop? This is not used if an AudioConfig is specified.")]
        [SerializeField] protected bool m_Loop;
        [Tooltip("The duration of the AudioSource fade. Set to 0 to disable fading out.")]
        [SerializeField] protected float m_FadeOutDuration = 0.1f;
        [Tooltip("The amount to fade out the AudioSource.")]
        [SerializeField] protected float m_FadeStep = 0.05f;

        public AudioClipSet AudioClipSet { get { return m_AudioClipSet; } set { m_AudioClipSet = value; } }
        public bool PlayAtOrigin { get { return m_PlayAtOrigin; } set { m_PlayAtOrigin = value; } }
        public bool Loop { get { return m_Loop; } set { m_Loop = value; } }
        public float FadeOutDuration { get { return m_FadeOutDuration; } set { m_FadeOutDuration = value; } }
        public float FadeStep { get { return m_FadeStep; } set { m_FadeStep = value; } }

        private AudioSource m_AudioSource;
        private ScheduledEventBase m_FadeEvent;

        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="useDataStream">The use data stream, contains the cast data.</param>
        protected override void DoCastInternal(MagicUseDataStream useDataStream)
        {
            Transform origin = useDataStream.CastData.CastOrigin;
            Vector3 direction = useDataStream.CastData.Direction;
            Vector3 targetPosition = useDataStream.CastData.CastTargetPosition;
            m_CastID = (uint)useDataStream.CastData.CastID;
            
            if (m_AudioSource != null && m_FadeEvent == null) {
                return;
            }
            
            var playPosition = m_PlayAtOrigin ? origin.position : GameObject.transform.position;
            
            m_AudioSource = m_AudioClipSet.PlayAtPosition(playPosition).AudioSource;
            m_AudioSource.loop = m_Loop;
            m_AudioSource.volume = 1;

            if (m_FadeEvent != null) {
                Scheduler.Cancel(m_FadeEvent);
                m_FadeEvent = null;
            }
        }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void StopCast()
        {
            base.StopCast();

            if (m_AudioSource == null || m_FadeEvent != null) { return; }

            if (m_FadeOutDuration > 0) {
                FadeAudio(m_FadeOutDuration / (1 / m_FadeStep));
            } else {
                m_AudioSource.Stop();
                m_AudioSource = null;
            }
        }

        /// <summary>
        /// Fades the audio volume.
        /// </summary>
        /// <param name="interval">The interval of the fade.</param>
        private void FadeAudio(float interval)
        {
            m_AudioSource.volume -= m_FadeStep;
            if (m_AudioSource.volume > 0) {
                m_FadeEvent = Scheduler.Schedule(interval, FadeAudio, interval);
            } else {
                m_AudioSource.Stop();
                m_AudioSource = null;
                m_FadeEvent = null;
            }
        }
    }
}