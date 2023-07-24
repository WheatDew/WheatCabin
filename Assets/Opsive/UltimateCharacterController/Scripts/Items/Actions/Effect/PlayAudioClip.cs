/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Effect
{
    using Opsive.Shared.Audio;
    using System;
    using UnityEngine;
    
    /// <summary>
    /// Plays an AudioClip when the effect starts.
    /// </summary>
    [Serializable]
    public class PlayAudioClip : ItemEffect
    {
        [Tooltip("A set of AudioClips that can be played when the effect is started.")]
        [SerializeField] protected AudioClipSet m_AudioClipSet = new AudioClipSet();

        public AudioClipSet AudioClipSet { get { return m_AudioClipSet; } set { m_AudioClipSet = value; } }

        /// <summary>
        /// Can the effect be started?
        /// </summary>
        /// <returns>True if the effect can be started.</returns>
        public override bool CanInvokeEffect()
        {
            return true;
        }

        /// <summary>
        /// Get the game object on which to play the audio clip.
        /// </summary>
        /// <returns>The game object on which to play the audio.</returns>
        protected virtual GameObject GetPlayGameObject()
        {
            return m_CharacterItemAction.GameObject;
        }

        /// <summary>
        /// The effect has been started.
        /// </summary>
        protected override void InvokeEffectInternal()
        {
            base.InvokeEffectInternal();

            m_AudioClipSet.PlayAudioClip(GetPlayGameObject());
        }
    }
}