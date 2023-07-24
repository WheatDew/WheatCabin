/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using System;
    using UnityEngine;

    /// <summary>
    /// The base class for all magic impact modules.
    /// </summary>
    [Serializable]
    public abstract class MagicImpactModule : MagicActionModule
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
    /// Invoke some generic impact actions during a magic impact.
    /// </summary>
    [Serializable]
    public class GenericMagicImpactModule : MagicImpactModule
    {
        [Tooltip("The impact actions.")]
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
        /// Function called when an impact happens.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback data.</param>
        public override void OnImpact(ImpactCallbackContext impactCallbackContext)
        {
            m_ImpactActions.OnImpact(impactCallbackContext, false);
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

    /// <summary>
    /// The Ricochet action will cast a new CastAction for nearby objects, creating a ricochet effect.
    /// </summary>
    [Serializable]
    public class RicochetImpact : MagicImpactModule
    {
        protected const string InfoKey_RicochetCount = "Magic/RicochetCount";
    
        [Tooltip("The Ricochet data.")]
        [SerializeField] protected Ricochet m_RicochetImpact;

        public Ricochet Ricochet { get => m_RicochetImpact; set => m_RicochetImpact = value; }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            
            m_RicochetImpact.Initialize(Character, CharacterItemAction);
            m_RicochetImpact.OnRicochet += HandleRicochet;
            
        }

        /// <summary>
        /// Function called when an impact happens.
        /// </summary>
        /// <param name="impactCallbackContext">The impact callback data.</param>
        public override void OnImpact(ImpactCallbackContext impactCallbackContext)
        {
            m_RicochetImpact.TryInvokeOnImpact(impactCallbackContext,false);
        }

        /// <summary>
        /// Handle the ricochet using the ricochet data.
        /// </summary>
        /// <param name="ricochetData">The ricochet data.</param>
        private void HandleRicochet(Ricochet.RicochetData ricochetData)
        {
            var magicUseDataStream = MagicAction.MagicUseDataStream;
            
            // If the target index was -1 then the next target index must be 1 and not 0.
            var castDataTargetIndex = magicUseDataStream.CastData.TargetIndex;
            if(castDataTargetIndex <= -1) {
                magicUseDataStream.CastData.TargetIndex = 1;
                magicUseDataStream.CastData.Targets = magicUseDataStream.CastData.Targets.NewSlice(0,2);
            } else {
                magicUseDataStream.CastData.TargetIndex++;
                magicUseDataStream.CastData.Targets = magicUseDataStream.CastData.Targets.NewSlice(0,magicUseDataStream.CastData.Targets.Count+1);
            }

            magicUseDataStream.CastData.Direction = ricochetData.Direction;
            magicUseDataStream.CastData.CastPosition = ricochetData.SourcePosition;
            magicUseDataStream.CastData.CastTargetPosition = ricochetData.TargetPosition;
            MagicAction.DebugLogger.DrawRay(this, ricochetData.SourcePosition, ricochetData.Direction, Color.magenta, 1);

            MagicAction.DebugLogger.SetInfo(InfoKey_RicochetCount, m_RicochetImpact.GetChainCountFor(magicUseDataStream.CastData.CastID).ToString());
            
            MagicAction.CasterModuleGroup.FirstEnabledModule.ImmediateCastEffects(
                MagicAction.MagicUseDataStream);
        }

        /// <summary>
        /// Reset the impact with the source id.
        /// </summary>
        /// <param name="sourceID">The source id of the impact to reset.</param>
        public override void Reset(uint sourceID)
        {
            m_RicochetImpact.Reset(sourceID);
        }

        /// <summary>
        /// Clean up the module when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_RicochetImpact.OnDestroy();
        }
    }
}