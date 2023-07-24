/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Effect;
    using System;
    using UnityEngine;

    /// <summary>
    /// The base class for effects when a throwable item is thrown.
    /// </summary>
    [Serializable]
    public abstract class ThrowableThrowEffectModule : ThrowableActionModule
    {
        /// <summary>
        /// Adds any effects to the throw.
        /// </summary>
        public abstract void InvokeEffect(ThrowableUseDataStream dataStream);
    }
    
    /// <summary>
    /// Invoke som effects when a throwable item is thrown.
    /// </summary>
    [Serializable]
    public class GenericItemEffects : ThrowableThrowEffectModule
    {
        [Tooltip("The list of Effects to Invoke.")]
        [SerializeField] protected ItemEffectGroup m_EffectGroup;

        public ItemEffectGroup EffectGroup { get => m_EffectGroup; set => m_EffectGroup = value; }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            
            m_EffectGroup.Initialize(this);
        }

        /// <summary>
        /// Adds any effects to the throw.
        /// </summary>
        public override void InvokeEffect(ThrowableUseDataStream dataStream)
        {
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
            return GetToStringPrefix()+$"Generic ({m_EffectGroup.Effects.Length}): " + ListUtility.ToStringDeep(m_EffectGroup.Effects, true);
        }
    }
}