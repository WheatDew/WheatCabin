/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Melee
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Effect;
    using System;
    using UnityEngine;

    /// <summary>
    /// Base class for the melee attack effects.
    /// </summary>
    [Serializable]
    public abstract class MeleeAttackEffectModule : MeleeActionModule
    {
        /// <summary>
        /// Called when the attack starts its active state.
        /// </summary>
        /// <param name="meleeUseDataStream">The use data stream.</param>
        public override void OnActiveAttackStart(MeleeUseDataStream meleeUseDataStream)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_CharacterItemAction.NetworkInfo != null && m_CharacterItemAction.NetworkInfo.HasAuthority()) {
                m_CharacterItemAction.NetworkCharacter.InvokeMeleeAttackEffectModule(this, meleeUseDataStream);
            }
#endif

            StartEffects();
        }

        /// <summary>
        /// Starts the attack effects.
        /// </summary>
        public abstract void StartEffects();
    }

    /// <summary>
    /// A generic item effect module for melee attacks.
    /// </summary>
    [Serializable]
    public class GenericItemEffects : MeleeAttackEffectModule
    {
        [Tooltip("Invoked effects on start attack.")]
        [SerializeField] private bool m_OnStartAttack = true;
        [Tooltip("Invoked effects on complete attack.")]
        [SerializeField] private bool m_OnCompleteAttack;
        [Tooltip("The attack must match the substate index to invoke (Ignore if less than 0).")]
        [SerializeField] private int m_SubstateIndex = -1;
        [Tooltip("The attack ID must match to invoke (Ignore if less than 0).")]
        [SerializeField] private int m_AttackID = -1;
        [Tooltip("The effects to invoke.")]
        [SerializeField] protected ItemEffectGroup m_EffectGroup;

        public bool OnStartAttack { get => m_OnStartAttack; set => m_OnStartAttack = value; }
        public bool OnCompleteAttack { get => m_OnCompleteAttack; set => m_OnCompleteAttack = value; }
        public int SubstateIndex { get => m_SubstateIndex; set => m_SubstateIndex = value; }
        public int AttackID { get => m_AttackID; set => m_AttackID = value; }
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
        /// Clean up the module when it is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_EffectGroup.OnDestroy();
        }

        /// <summary>
        /// Called when the attack starts its active state.
        /// </summary>
        /// <param name="meleeUseDataStream">The use data stream.</param>
        public override void OnActiveAttackStart(MeleeUseDataStream meleeUseDataStream)
        {
            if (!m_OnStartAttack) { return; }
            if (!CanInvoke(meleeUseDataStream)) { return; }

            base.OnActiveAttackStart(meleeUseDataStream);
        }

        /// <summary>
        /// The item has completed its active attack state.
        /// </summary>
        /// <param name="meleeUseDataStream">The use data stream.</param>
        public override void OnActiveAttackComplete(MeleeUseDataStream meleeUseDataStream)
        {
            if (!m_OnCompleteAttack) { return; }
            if (!CanInvoke(meleeUseDataStream)) { return; }

            StartEffects();
        }

        /// <summary>
        /// Starts the attack effect.
        /// </summary>
        public override void StartEffects()
        {
            m_EffectGroup.InvokeEffects();
        }

        /// <summary>
        /// Can the effect be invoked?
        /// </summary>
        /// <param name="meleeUseDataStream">The use data stream.</param>
        /// <returns>True if the conditions pass.</returns>
        private bool CanInvoke(MeleeUseDataStream meleeUseDataStream)
        {
            if (m_AttackID >= 0 && m_AttackID != meleeUseDataStream.AttackData.AttackID) { return false; }

            if (m_SubstateIndex >= 0 && m_SubstateIndex != MeleeAction.GetUseItemSubstateIndex()) { return false; }

            return true;
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

    /// <summary>
    /// This class is used to enable or disable objects when attacks start and stop an spawn slash effects based on the 
    /// </summary>
    [Serializable]
    public class EnableDisableEffects : MeleeAttackEffectModule
    {
        [Tooltip("The effect must match the substate index to invoke (Ignore if less than 0).")]
        [SerializeField] private int m_SubstateIndex = -1;
        [Tooltip("The attack ID must match to invoke (Ignore if less than 0).")]
        [SerializeField] private int m_AttackID = -1;
        [Tooltip("The object to enable while attacking.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<GameObject> m_Enable;
        [Tooltip("The object to enable while attacking.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<GameObject> m_Disable;
        
        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            
            m_Enable.Initialize(m_CharacterItemAction);
            m_Disable.Initialize(m_CharacterItemAction);
        }
        
        /// <summary>
        /// Can the effect be invoked?
        /// </summary>
        /// <param name="meleeUseDataStream">The use data stream.</param>
        /// <returns>True if the conditions pass.</returns>
        private bool CanInvoke(MeleeUseDataStream meleeUseDataStream)
        {
            if (m_AttackID >= 0 && m_AttackID != meleeUseDataStream.AttackData.AttackID) { return false; }

            if (m_SubstateIndex >= 0 && m_SubstateIndex != MeleeAction.GetUseItemSubstateIndex()) { return false; }

            return true;
        }

        /// <summary>
        /// Called when the attack starts its active state.
        /// </summary>
        /// <param name="meleeUseDataStream">The use data stream.</param>
        public override void OnActiveAttackStart(MeleeUseDataStream meleeUseDataStream)
        {
            if (@CanInvoke(meleeUseDataStream)) { return; }

            base.OnActiveAttackStart(meleeUseDataStream);
        }

        /// <summary>
        /// Starts the attack effect.
        /// </summary>
        public override void StartEffects()
        {
            var enableObject = m_Enable.GetValue();
            if (enableObject != null) {
                enableObject.SetActive(true);
            }
            var disableObject = m_Disable.GetValue();
            if (disableObject != null) {
                disableObject.SetActive(false);
            }
        }

        /// <summary>
        /// The item has completed its active attack state.
        /// </summary>
        /// <param name="meleeUseDataStream">The use data stream.</param>
        public override void OnActiveAttackComplete(MeleeUseDataStream meleeUseDataStream)
        {
            if (!CanInvoke(meleeUseDataStream)) { return; }

            var enableObject = m_Enable.GetValue();
            if (enableObject != null) {
                enableObject.SetActive(false);
            }
            var disableObject = m_Disable.GetValue();
            if (disableObject != null) {
                disableObject.SetActive(true);
            }
        }
    }
}