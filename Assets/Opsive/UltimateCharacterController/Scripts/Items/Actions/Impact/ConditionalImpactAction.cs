/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Impact
{
    using System;
    using UnityEngine;

    /// <summary>
    /// This impact action allow you to nest impact actions and or stop 
    /// </summary>
    [Serializable]
    public class ConditionalImpactAction : ImpactAction
    {
        [SerializeField] protected ImpactActionConditionGroup m_Conditions;
        [SerializeField] protected ImpactActionGroup m_ImpactActionsOnPass;
        [SerializeField] protected ImpactActionGroup m_ImpactActionsOnFail;
        [SerializeField] protected bool m_ResetDamageDataOnFail;

        /// <summary>
        /// Initialize the effect.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            
            m_Conditions.Initialize(BoundGameObject, m_CharacterItemAction);
            m_ImpactActionsOnPass.Initialize(BoundGameObject, m_CharacterItemAction);
            m_ImpactActionsOnFail.Initialize(BoundGameObject, m_CharacterItemAction);
        }

        /// <summary>
        /// Internal method which performs the impact action.
        /// </summary>
        /// <param name="ctx">Context about the hit.</param>
        protected override void OnImpactInternal(ImpactCallbackContext ctx)
        {
            if (m_Conditions.CanImpact(ctx)) {
                m_ImpactActionsOnPass.OnImpact(ctx, false);
            } else {
                m_ImpactActionsOnFail.OnImpact(ctx, false);
                if (m_ResetDamageDataOnFail) {
                    ctx.ImpactDamageData?.Reset();
                }
            }
        }

        /// <summary>
        /// Resets the impact action.
        /// </summary>
        /// <param name="sourceID">The ID of the cast to reset.</param>
        public override void Reset(uint sourceID)
        {
            base.Reset(sourceID);
            m_ImpactActionsOnPass.Reset(sourceID);
            m_ImpactActionsOnFail.Reset(sourceID);
        }

        /// <summary>
        /// The action has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_Conditions.OnDestroy();
            m_ImpactActionsOnPass.OnDestroy();
            m_ImpactActionsOnFail.OnDestroy();
        }
    }
}