/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Effect
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Invoke a Unity Event.
    /// </summary>
    [Serializable]
    public class InvokeUnityEvent : ItemEffect
    {
        [Tooltip("The event to invoke.")]
        [SerializeField] protected UnityEvent m_OnInvokeEffect;

        public UnityEvent OnInvokeEffect { get => m_OnInvokeEffect; set => m_OnInvokeEffect = value; }

        /// <summary>
        /// Can the effect be started?
        /// </summary>
        /// <returns>True if the effect can be started.</returns>
        public override bool CanInvokeEffect()
        {
            return true;
        }

        /// <summary>
        /// Invoke the effect.
        /// </summary>
        protected override void InvokeEffectInternal()
        {
            base.InvokeEffectInternal();
            m_OnInvokeEffect?.Invoke();
        }
    }
    
    /// <summary>
    /// Invoke a Unity Event.
    /// </summary>
    [Serializable]
    public class DebugItemEffect : ItemEffect
    {
        [Tooltip("The Event to Invoke.")]
        [SerializeField] protected string m_Message;

        public string Message { get => m_Message; set => m_Message = value; }

        /// <summary>
        /// Can the effect be started?
        /// </summary>
        /// <returns>True if the effect can be started.</returns>
        public override bool CanInvokeEffect()
        {
            return true;
        }

        /// <summary>
        /// Invoke the effect.
        /// </summary>
        protected override void InvokeEffectInternal()
        {
            base.InvokeEffectInternal();
            Debug.Log(m_Message+" "+m_CharacterItemAction, m_CharacterItemAction);
        }
    }
}