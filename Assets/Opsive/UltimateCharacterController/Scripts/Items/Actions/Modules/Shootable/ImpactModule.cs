/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using System;
    using UnityEngine;

    public class ShootableImpactCallbackContext : ImpactCallbackContext
    {
        private ShootableAction m_ShootableAction;

        public ShootableAction ShootableAction { get => m_ShootableAction; set => m_ShootableAction = value; }
        public override CharacterItemAction CharacterItemAction => ShootableAction;

        /// <summary>
        /// Reset the data.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            m_ShootableAction = null;
        }
        
        /// <summary>
        /// Set the character item action that will receive callbacks.
        /// </summary>
        /// <param name="characterItemAction">The character item action that will receive callbacks.</param>
        public override void SetCharacterItemAction(CharacterItemAction characterItemAction)
        {
            base.SetCharacterItemAction(characterItemAction);
            ShootableAction = characterItemAction as ShootableAction;
        }
        
        /// <summary>
        /// Get a duplicate version of this object that is pooled pooled.
        /// </summary>
        /// <returns>The pooled duplicate.</returns>
        public override ImpactCallbackContext GetPooledDuplicate()
        {
            var duplicate = GenericObjectPool.Get<ShootableImpactCallbackContext>();
            duplicate.PooledCopy(this);
            return duplicate;
        }

        /// <summary>
        /// Copy the contents of another impact callback context.
        /// </summary>
        /// <param name="other">The other callback context to copy the data from.</param>
        protected override void PooledCopy(ImpactCallbackContext other)
        {
            base.PooledCopy(other);
            if (other is ShootableImpactCallbackContext otherShootable) {
                m_ShootableAction = otherShootable.m_ShootableAction;
            }
        }
    }
    
    /// <summary>
    /// The base class for shootable impacts.
    /// </summary>
    [Serializable]
    public abstract class ShootableImpactModule : ShootableActionModule
    {
        /// <summary>
        /// On fire impact.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback.</param>
        public abstract void OnImpact(ImpactCallbackContext impactCallbackContext);
        
        /// <summary>
        /// Reset the impact with the source id specified.
        /// </summary>
        /// <param name="sourceID">The source id.</param>
        public abstract void Reset(uint sourceID);
    }
    
    /// <summary>
    /// Invoke generic impact actions when impacting a shot.
    /// </summary>
    [Serializable]
    public class GenericShootableImpactModule : ShootableImpactModule
    {
        [Tooltip("The impact actions to invoke on impact.")]
        [SerializeField] protected ImpactActionGroup m_ImpactActions  = ImpactActionGroup.DefaultDamageGroup(true);

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
        /// On fire impact.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback.</param>
        public override void OnImpact(ImpactCallbackContext impactCallbackContext)
        {
            m_ImpactActions.OnImpact(impactCallbackContext, true);
        }

        /// <summary>
        /// Reset the impact with the source id specified.
        /// </summary>
        /// <param name="sourceID">The source id.</param>
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