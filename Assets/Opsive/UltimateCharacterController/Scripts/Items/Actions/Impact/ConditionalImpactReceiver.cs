/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Impact
{
    using System.Collections.Generic;
    using Opsive.Shared.Events;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// An interface for an object that can decide if it should be impacted.
    /// </summary>
    public interface IImpactCondition
    {
        /// <summary>
        /// Should the object impact event be called?
        /// </summary>
        /// <param name="ctx">The impact context.</param>
        /// <returns>True if the object was impacted.</returns>
        bool CanImpact(ImpactCallbackContext ctx);
    }
    
    /// <summary>
    /// A component that detects impact from an weapon attack.
    /// </summary>
    public class ConditionalImpactReceiver : MonoBehaviour
    {
        [Tooltip("Search on this gameobject for Impact Conditions.")]
        [SerializeField] private bool m_GetImpactConditionsOnObject = true;
        [Tooltip("Additional impact conditions.")]
        [SerializeField] private ImpactConditionBehaviourBase[] m_ImpactConditionsBehaviours;
        [Tooltip("The impact actions.")]
        [SerializeField] private ImpactActionConditionGroup m_ImpactActionConditions;
        [Tooltip("The impact actions on passing the condition.")]
        [SerializeField] protected ImpactActionGroup m_ImpactActionsOnPass;
        [Tooltip("The even that gets invoked when the object was impacted passing the conditions.")]
        [SerializeField] protected UnityEvent<ImpactCallbackContext> m_OnObjectImpactSuccess;
        [Tooltip("The impact actions on failing the condition.")]
        [SerializeField] protected ImpactActionGroup m_ImpactActionsOnFail;
        [Tooltip("The even that gets invoked when the object was impacted failing the conditions.")]
        [SerializeField] protected UnityEvent<ImpactCallbackContext> m_OnObjectImpactFail;

        private List<IImpactCondition> m_ImpactConditions;
        private bool m_Initialized = false;
        
        public List<IImpactCondition> ImpactConditions { get => m_ImpactConditions; set => m_ImpactConditions = value; }
        public UnityEvent<ImpactCallbackContext> OnObjectImpactSuccess { get => m_OnObjectImpactSuccess; set => m_OnObjectImpactSuccess = value; }
        public UnityEvent<ImpactCallbackContext> OnObjectImpactFail { get => m_OnObjectImpactFail; set => m_OnObjectImpactFail = value; }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public virtual void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize the class.
        /// </summary>
        /// <param name="force">force initialize?</param>
        private void Initialize(bool force)
        {
            if (m_Initialized && !force) {
                return;
            }

            m_Initialized = true;
            
            Shared.Events.EventHandler.RegisterEvent<ImpactCallbackContext>(gameObject, "OnObjectImpact", OnImpact);
            m_ImpactActionConditions.Initialize(null);
            m_ImpactActionsOnPass.Initialize(null);
            m_ImpactActionsOnFail.Initialize(null);
            
            if (m_ImpactConditions == null) { m_ImpactConditions = new List<IImpactCondition>(); }

            if (m_GetImpactConditionsOnObject) {
                m_ImpactConditions.AddRange(GetComponents<IImpactCondition>());
            }
            
            m_ImpactConditions.AddRange(m_ImpactConditionsBehaviours);
        }

        /// <summary>
        /// The object has been impacted with another object.
        /// </summary>
        /// <param name="ctx">Impact callback context.</param>
        private void OnImpact(ImpactCallbackContext ctx)
        {
            if (CanObjectImpact(ctx) == false) {
                m_OnObjectImpactFail.Invoke(ctx);
                m_ImpactActionsOnFail.OnImpact(ctx, false);
                return;
            }
            
            m_OnObjectImpactSuccess.Invoke(ctx);
            m_ImpactActionsOnPass.OnImpact(ctx, false);
        }

        /// <summary>
        /// Should the object impact event be called?
        /// </summary>
        /// <param name="ctx">The impact context.</param>
        /// <returns>True if the object was impacted.</returns>
        public virtual bool CanObjectImpact(ImpactCallbackContext ctx)
        {
            for (int i = 0; i < m_ImpactConditions.Count; i++) {
                if (m_ImpactConditions[i].CanImpact(ctx) == false) {
                    return false;
                }
            }

            if (m_ImpactActionConditions.CanImpact(ctx) == false) {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Reset the impact with the source id.
        /// </summary>
        /// <param name="sourceID">The source id of the impact to reset.</param>
        public void DoReset(uint sourceID)
        {
            m_ImpactActionsOnPass.Reset(sourceID);
            m_ImpactActionsOnFail.Reset(sourceID);
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
            EventHandler.UnregisterEvent<ImpactCallbackContext>(gameObject, "OnObjectImpact", OnImpact);
            m_ImpactActionsOnPass.OnDestroy();
            m_ImpactActionsOnFail.OnDestroy();
            m_ImpactActionConditions.OnDestroy();
        }
    }
}