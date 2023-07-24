/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Effect
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Items.Actions.Bindings;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using System;
    using UnityEngine;

    /// <summary>
    /// An Item Effect is a simple action that can be Invoked at any time only using the character item action as context. 
    /// </summary>
    [Serializable]
    public abstract class ItemEffect : BoundStateObject
    {
        [Tooltip("Is the effect enabled?")]
        [SerializeField] protected bool m_Enabled = true;
        [Tooltip("Should the invoke effect after a certain delay.")]
        [SerializeField] protected float m_Delay = 0;

        public bool Enabled { get => m_Enabled; set => m_Enabled = value; }
        public float Delay { get => m_Delay; set => m_Delay = value; }

        protected override GameObject BoundGameObject => m_CharacterItemAction?.gameObject ?? m_StateBoundGameObject;

        protected CharacterItemAction m_CharacterItemAction;

        protected Action m_CachedInvokeInternalAction;
        protected ScheduledEventBase m_DelayScheduledEvent;

        /// <summary>
        /// Initializes the ImpactAction.
        /// </summary>
        /// <param name="characterItemAction">The item action.</param>
        public virtual void Initialize(CharacterItemAction characterItemAction)
        {
            m_CharacterItemAction = characterItemAction;
            base.Initialize(characterItemAction.Character);
            InitializeInternal();
        }

        /// <summary>
        /// Initialize the effect.
        /// </summary>
        protected virtual void InitializeInternal()
        {
            // To be overriden.
        }

        /// <summary>
        /// Can the effect be invoked?
        /// </summary>
        /// <returns>True if it can be invoked.</returns>
        public virtual bool CanInvokeEffect()
        {
            return m_Enabled;
        }

        /// <summary>
        /// Try Invoke the effect.
        /// </summary>
        /// <returns>True if it was invoked.</returns>
        public virtual bool TryInvokeEffect()
        {
            if (CanInvokeEffect()) {
                InvokeEffect();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Invoke the effect.
        /// </summary>
        protected virtual void InvokeEffect()
        {
            if (m_Delay <= 0) {
                InvokeEffectInternal();
            } else {
                if (m_CachedInvokeInternalAction == null) {
                    m_CachedInvokeInternalAction = InvokeEffectInternal;
                }
                m_DelayScheduledEvent = Scheduler.Schedule(m_Delay, m_CachedInvokeInternalAction);
            }
        }

        /// <summary>
        /// Invoke the effect.
        /// </summary>
        protected virtual void InvokeEffectInternal()
        {
            m_DelayScheduledEvent = null;
        }

        /// <summary>
        /// The action has been destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
            if (m_DelayScheduledEvent != null) {
                Scheduler.Cancel(m_DelayScheduledEvent);
            }
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The type name.</returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// An Item Effect group is an array of effects which has a custom inspector.
    /// </summary>
    [Serializable]
    public class ItemEffectGroup
    {
        [Tooltip("The array of Item Effects.")]
        [SerializeReference] protected ItemEffect[] m_Effects;

        protected CharacterItemAction m_CharacterItemAction;

        public ItemEffect[] Effects
        {
            get => m_Effects;
            set => m_Effects = value;
        }

        /// <summary>
        /// Initialize the item effects.
        /// </summary>
        /// <param name="actionModule">The module with the character item action to link to the effects.</param>
        public void Initialize(ActionModule actionModule)
        {
            Initialize(actionModule.CharacterItemAction);
        }

        /// <summary>
        /// Initialize the item effects.
        /// </summary>
        /// <param name="characterItemAction">The character item action to link to the effects.</param>
        public void Initialize(CharacterItemAction characterItemAction)
        {
            m_CharacterItemAction = characterItemAction;

            if (m_Effects == null) { return; }

            for (int i = 0; i < m_Effects.Length; i++) {
                if (m_Effects[i] == null) { continue; }

                m_Effects[i].Initialize(m_CharacterItemAction);
            }
        }

        /// <summary>
        /// Can all the effects be invoked.
        /// </summary>
        /// <returns>True if all the effects can be invoked.</returns>
        public bool CanInvokeEffects()
        {
            for (int i = 0; i < m_Effects.Length; i++) {
                if (m_Effects[i] == null) { continue; }

                if (!m_Effects[i].CanInvokeEffect()) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Invoke all the effects.
        /// </summary>
        public void InvokeEffects()
        {
            for (int i = 0; i < m_Effects.Length; i++) {
                if (m_Effects[i] == null) { continue; }
                m_Effects[i].TryInvokeEffect();
            }
        }

        /// <summary>
        /// Destroy all the effects.
        /// </summary>
        public void OnDestroy()
        {
            if (m_Effects == null) { return; }

            for (int i = 0; i < m_Effects.Length; i++) {
                if (m_Effects[i] == null) { continue; }
                m_Effects[i].OnDestroy();
            }
        }
    }
}