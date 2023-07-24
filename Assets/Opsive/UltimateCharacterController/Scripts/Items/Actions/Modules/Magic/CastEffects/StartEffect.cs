/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.CastEffects
{
    using Opsive.UltimateCharacterController.Character.Effects;
    using UnityEngine;

    /// <summary>
    /// Starts an effect on the character.
    /// </summary>
    [System.Serializable]
    public class StartEffect : MagicMultiTargetCastEffectModule
    {
        [Tooltip("The effect that should be started when the ability starts.")]
        [HideInInspector] [SerializeField] protected string m_EffectName;
        [Tooltip("The index of the effect that should be started when the ability starts.")]
        [HideInInspector] [SerializeField] protected int m_EffectIndex = -1;
        [Tooltip("Should the effect be stopped when the cast is stopped?")]
        [SerializeField] protected bool m_StopEffect;

        public string EffectName { get { return m_EffectName; } set { m_EffectName = value; } }
        public int EffectIndex { get { return m_EffectIndex; } set { m_EffectIndex = value; } }
        public bool StopEffect { get { return m_StopEffect; } set { m_StopEffect = value; } }

        private Effect m_Effect;

        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="useDataStream">The use data stream, contains the cast data.</param>
        protected override void DoCastInternal(MagicUseDataStream useDataStream)
        {
            if (!string.IsNullOrEmpty(m_EffectName)) {
                m_Effect = CharacterLocomotion.GetEffect(Shared.Utility.TypeUtility.GetType(m_EffectName), m_EffectIndex);
            }
            if (m_Effect == null) {
                return;
            }

            m_CastID = (uint)useDataStream.CastData.CastID;
            CharacterLocomotion.TryStartEffect(m_Effect);
        }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void StopCast()
        {
            if (m_StopEffect && m_Effect != null) {
                CharacterLocomotion.TryStopEffect(m_Effect);
            } 

            base.StopCast();
        }
    }
}