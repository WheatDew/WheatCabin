namespace Opsive.UltimateCharacterController.Items.Actions.Impact
{
    using System;
    using UnityEngine;

    public abstract class ImpactConditionBehaviourBase : MonoBehaviour, IImpactCondition
    {
        public abstract bool CanImpact(ImpactCallbackContext ctx);
    }
    
    public class ImpactConditionBehaviour : ImpactConditionBehaviourBase
    {
        [SerializeField] protected ImpactActionConditionGroup m_ImpactActionConditions;
        
        private void Awake()
        {
            m_ImpactActionConditions.Initialize(null, null);
        }

        public override bool CanImpact(ImpactCallbackContext ctx)
        {
            return m_ImpactActionConditions.CanImpact(ctx);
        }

        private void OnDestroy()
        {
            m_ImpactActionConditions.OnDestroy();
        }
    }
}