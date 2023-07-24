/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using System;
    using UnityEngine;

    /// <summary>
    /// The base class for throwable item impacts.
    /// </summary>
    [Serializable]
    public abstract class ThrowableImpactModule : ThrowableActionModule
    {
        /// <summary>
        /// Function called when an impact happens.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback data.</param>
        public abstract void OnImpact(ImpactCallbackContext impactCallbackContext);
        
        /// <summary>
        /// Reset the impact with the source id.
        /// </summary>
        /// <param name="sourceID">The source id of the impact to reset.</param>
        public abstract void Reset(uint sourceID);
    }
    
    /// <summary>
    /// Invoke generic impact actions when a throwable item is thrown.
    /// </summary>
    [Serializable]
    public class GenericThrowableImpactModule : ThrowableImpactModule
    {
        [Tooltip("The impact actions to invoked on impact.")]
        [SerializeField] protected ImpactActionGroup m_ImpactActions = ImpactActionGroup.DefaultDamageGroup(true);

        public ImpactActionGroup ImpactActions { get => m_ImpactActions; set => m_ImpactActions = value; }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            
            m_ImpactActions.Initialize(this);
        }

        /// <summary>
        /// Function called when an impact happens.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback data.</param>
        public override void OnImpact(ImpactCallbackContext impactCallbackContext)
        {
            m_ImpactActions.OnImpact(impactCallbackContext, true);
        }

        /// <summary>
        /// Reset the impact with the source id.
        /// </summary>
        /// <param name="sourceID">The source id of the impact to reset.</param>
        public override void Reset(uint sourceID)
        {
            m_ImpactActions.Reset(sourceID);
        }

        /// <summary>
        /// Clean up the module when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_ImpactActions.OnDestroy();
        }
        
        /// <summary>
        /// Write the module name in an easy to read format for debugging.
        /// </summary>
        /// <returns>The string representation of the module.</returns>
        public override string ToString()
        {
            if (m_ImpactActions == null || m_ImpactActions.ImpactActions == null) {
                return base.ToString();
            }
            return GetToStringPrefix()+$"Generic ({m_ImpactActions.Count}): " + ListUtility.ToStringDeep(m_ImpactActions.ImpactActions, true);
            
        }
    }
}