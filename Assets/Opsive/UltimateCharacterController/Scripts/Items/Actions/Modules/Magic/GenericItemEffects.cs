/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Effect;
    using System;
    using UnityEngine;

    /// <summary>
    /// This magic start stop module allow generic item effects when starting and/or stopping a spell.
    /// </summary>
    [Serializable]
    public class GenericItemEffects : MagicStartStopModule
    {
        [Tooltip("Invoke the item effects on start.")]
        [SerializeField] private bool m_OnStart = true;
        [Tooltip("Invoked the item effects on stop.")]
        [SerializeField] private bool m_OnStop;
        [Tooltip("The item effects to invoke.")]
        [SerializeField] protected ItemEffectGroup m_EffectGroup;

        public bool OnStart { get => m_OnStart; set => m_OnStart = value; }
        public bool OnStop { get => m_OnStop; set => m_OnStop = value; }
        public ItemEffectGroup EffectGroup { get => m_EffectGroup; set => m_EffectGroup = value; }

        /// <summary>
        /// Initialize to check if this is a begin or end action.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            m_EffectGroup.Initialize(this);
        }
        
        /// <summary>
        /// The action has started.
        /// </summary>
        /// <param name="useDataStream">The data stream with information about the magic cast.</param>
        public override void Start(MagicUseDataStream useDataStream)
        {
            if (!m_OnStart) { return; }
            m_EffectGroup.InvokeEffects();
        }

        /// <summary>
        /// The action has stopped.
        /// </summary>
        /// <param name="useDataStream">The data stream with information about the magic cast.</param>
        public override void Stop(MagicUseDataStream useDataStream)
        {
            if (!m_OnStop) { return; }
            m_EffectGroup.InvokeEffects();
        }

        /// <summary>
        /// Clean up the module when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_EffectGroup.OnDestroy();
        }
        
        /// <summary>
        /// Write the module name in an easy to read format for debugging.
        /// </summary>
        /// <returns>The string representation of the module.</returns>
        public override string ToString()
        {
            if (m_EffectGroup == null || m_EffectGroup.Effects == null) {
                return base.ToString();
            }
            return GetToStringPrefix() + $"Generic ({m_EffectGroup.Effects.Length}): " + ListUtility.ToStringDeep(m_EffectGroup.Effects, true);
        }
    }
}