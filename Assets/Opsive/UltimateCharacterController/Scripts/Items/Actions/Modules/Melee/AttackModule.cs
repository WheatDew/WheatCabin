/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Melee
{
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// Contains data about the attack. The AttackID is used to define what effect or impact is used. 
    /// </summary>
    [Serializable]
    public class MeleeAttackData
    {
        public int AttackID = 0;
        [StateName] public string StateName;
        public float StrengthMultiplier = 1;
        public bool SingleHit;
    }

    /// <summary>
    /// The base class for melee attack modules, used to define how attacks are performed.
    /// </summary>
    [Serializable]
    public abstract class MeleeAttackModule : MeleeActionModule,
        IModuleCanStartUseItem, IModuleStartItemUse, IModuleStopItemUse, IModuleUseItemUpdate
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;

        protected MeleeAttackData m_MeleeAttackData;

        [Shared.Utility.NonSerialized] public MeleeAttackData AttackData { get { return m_MeleeAttackData; } set => m_MeleeAttackData = value; }


        public abstract bool IsActiveAttacking { get; }

        /// <summary>
        /// Create a new attack data which can be cached.
        /// </summary>
        /// <returns>The new attack data.</returns>
        public virtual MeleeAttackData CreateAttackData()
        {
            return new MeleeAttackData();
        }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_MeleeAttackData = CreateAttackData();
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public abstract bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState);

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public abstract void StartItemUse(Use useAbility);

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public abstract void StopItemUse();

        /// <summary>
        /// Start the attack.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public abstract void AttackStart(MeleeUseDataStream dataStream);

        /// <summary>
        /// Use item update when the update ticks.
        /// </summary>
        public abstract void UseItemUpdate();

        /// <summary>
        /// Complete the attack.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public abstract void AttackComplete(MeleeUseDataStream dataStream);

        /// <summary>
        /// Cancel the attack.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public abstract void AttackCanceled(MeleeUseDataStream dataStream);
    }

    /// <summary>
    /// A basic module that deals with attacking.
    /// </summary>
    [Serializable]
    public class SimpleAttack : MeleeAttackModule
    {
        [Tooltip("Animation event for Attack start.")]
        [SerializeField] protected AnimationSlotEventTrigger m_ActiveAttackStartEventTrigger = new AnimationSlotEventTrigger(false, 0);
        [Tooltip("Animation event for Attack complete.")]
        [SerializeField] protected AnimationSlotEventTrigger m_ActiveAttackCompleteEventTrigger = new AnimationSlotEventTrigger(false, 0.2f);
        [Tooltip("Animation event for when the attack can chain with another attack without waiting until complete.")]
        [SerializeField] protected AnimationSlotEventTrigger m_AllowChainAttackEventTrigger = new AnimationSlotEventTrigger(false, 0.2f);
        [Tooltip("When the weapon attacks should only one hit be registered per use?")]
        [SerializeField] protected bool m_SingleHit;

        protected bool m_CanceledAttack;
        protected bool m_ActiveAttacking;

        [Shared.Utility.NonSerialized] public AnimationSlotEventTrigger ActiveAttackStartEventTrigger { get => m_ActiveAttackStartEventTrigger; set => m_ActiveAttackStartEventTrigger.CopyFrom(value); }
        [Shared.Utility.NonSerialized] public AnimationSlotEventTrigger ActiveAttackCompleteEventTrigger { get => m_ActiveAttackCompleteEventTrigger; set => m_ActiveAttackCompleteEventTrigger.CopyFrom(value); }
        [Shared.Utility.NonSerialized] public AnimationSlotEventTrigger AllowChainAttackEventTrigger { get => m_AllowChainAttackEventTrigger; set => m_AllowChainAttackEventTrigger.CopyFrom(value); }
        public bool SingleHit { get { return m_SingleHit; } set { m_SingleHit = value; } }

        public override bool IsActiveAttacking => m_ActiveAttacking;

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);

            m_ActiveAttackStartEventTrigger.RegisterUnregisterEvent(register, Character, "OnAnimatorActiveAttackStart", SlotID, HandleActiveAttackStartAnimationEvent);
            m_ActiveAttackCompleteEventTrigger.RegisterUnregisterEvent(register, Character, "OnAnimatorMeleeAttackComplete", SlotID, HandleActiveAttackCompleteAnimationEvent);
            m_AllowChainAttackEventTrigger.RegisterUnregisterEvent(register, Character, "OnAnimatorAllowChainAttack", SlotID, HandleAllowChainAttackAnimationEvent);
        }

        /// <summary>
        /// Handle the animation event.
        /// </summary>
        protected virtual void HandleAllowChainAttackAnimationEvent()
        {
            StartAllowChainAttack();
        }

        /// <summary>
        /// Handle the animation event.
        /// </summary>
        protected virtual void HandleActiveAttackStartAnimationEvent()
        {
            ActiveAttackStart();
        }

        /// <summary>
        /// Handle the animation event.
        /// </summary>
        protected virtual void HandleActiveAttackCompleteAnimationEvent()
        {
            ActiveAttackComplete();
        }

        /// <summary>
        /// The attack can now transition into a combo.
        /// </summary>
        protected virtual void StartAllowChainAttack()
        {
            // Do nothing.
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            if (m_ActiveAttackStartEventTrigger.IsWaiting) {
                return false;
            }
            //Don't allow start with waiting for chain attack. Requires a trigger with AllowAttackCombos.
            if (m_AllowChainAttackEventTrigger.IsWaiting) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public override void StartItemUse(Use useAbility)
        {
            m_CanceledAttack = false;
        }

        /// <summary>
        /// Start the attack.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void AttackStart(MeleeUseDataStream dataStream)
        {
            CharacterItemAction.DebugLogger.Log(this, "Attack Start");

            m_ActiveAttackStartEventTrigger.WaitForEvent(false);
        }

        /// <summary>
        /// Active attact starts
        /// </summary>
        public virtual void ActiveAttackStart()
        {
            m_ActiveAttacking = true;

            MeleeAction.OnActiveAttackStart(m_MeleeAttackData);

            // start Wait for chain attack and active attack complete at the same time, such that chain attack can cancel active attack.
            m_AllowChainAttackEventTrigger.WaitForEvent(false);
            m_ActiveAttackCompleteEventTrigger.WaitForEvent(false);
        }

        /// <summary>
        /// Use item update when the update ticks.
        /// </summary>
        public override void UseItemUpdate()
        {
            // No need to update if the weapon has already hit an object and it can only hit a single object or a solid object (such as a shield) was hit.
            if (m_CanceledAttack || !m_ActiveAttacking) { return; }

            m_MeleeAttackData.SingleHit = m_SingleHit;
            MeleeAction.CheckForCollision(m_MeleeAttackData);
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {
            m_AllowChainAttackEventTrigger.CancelWaitForEvent();
            m_ActiveAttackStartEventTrigger.CancelWaitForEvent();
            m_ActiveAttackCompleteEventTrigger.CancelWaitForEvent();
            m_ActiveAttacking = false;
            m_CanceledAttack = false;
        }

        /// <summary>
        /// Complete the attack.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void AttackComplete(MeleeUseDataStream dataStream)
        {
            ActiveAttackComplete();
        }

        /// <summary>
        /// Active attack complete.
        /// </summary>
        public void ActiveAttackComplete()
        {
            if (m_ActiveAttacking == false) {
                return;
            }

            m_AllowChainAttackEventTrigger.CancelWaitForEvent();
            m_ActiveAttackCompleteEventTrigger.CancelWaitForEvent();
            m_ActiveAttacking = false;

            m_MeleeAttackData.SingleHit = m_SingleHit;
            MeleeAction.OnActiveAttackComplete(m_MeleeAttackData);
        }

        /// <summary>
        /// Cancel the attack.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void AttackCanceled(MeleeUseDataStream dataStream)
        {
            ActiveAttackComplete();
            m_CanceledAttack = true;
            m_ActiveAttackStartEventTrigger.CancelWaitForEvent();
            m_AllowChainAttackEventTrigger.CancelWaitForEvent();
            m_ActiveAttackCompleteEventTrigger.CancelWaitForEvent();
            m_ActiveAttacking = false;
        }

    }

    /// <summary>
    /// A basic module that deals with attacking.
    /// </summary>
    [Serializable]
    public class MultiAttack : MeleeAttackModule
    {
        [Serializable]
        public class Attack
        {
            [Tooltip("The Melee attack data.")]
            [SerializeField] private MeleeAttackData m_MeleeData;
            [Tooltip("Animation event for Attack start.")]
            [SerializeField] private AnimationSlotEventTrigger m_ActiveAttackStartEventTrigger = new AnimationSlotEventTrigger(false, 0);
            [Tooltip("Animation event for Attack complete.")]
            [SerializeField] private AnimationSlotEventTrigger m_ActiveAttackCompleteEventTrigger = new AnimationSlotEventTrigger(false, 0.2f);

            public MeleeAttackData MeleeData { get => m_MeleeData; set => m_MeleeData = value; }
            public AnimationSlotEventTrigger ActiveAttackStartEventTrigger { get => m_ActiveAttackStartEventTrigger; set => m_ActiveAttackStartEventTrigger.CopyFrom(value); }
            public AnimationSlotEventTrigger ActiveAttackCompleteEventTrigger { get => m_ActiveAttackCompleteEventTrigger; set => m_ActiveAttackCompleteEventTrigger.CopyFrom(value); }
        }

        [Tooltip("The Attacks that should be performed in order.")]
        [SerializeField] protected Attack[] m_Attacks;
        [Tooltip("Animation event for when the attack can chain with another attack without waiting until complete.")]
        [SerializeField] protected AnimationSlotEventTrigger m_AllowChainAttackEventTrigger = new AnimationSlotEventTrigger(false, 2f);

        private int m_AttackIndex = -1;
        protected bool m_CanceledAttack;
        protected bool m_ActiveAttacking;

        [Shared.Utility.NonSerialized] public Attack[] Attacks { get => m_Attacks; set => m_Attacks = value; }
        [Shared.Utility.NonSerialized] public AnimationSlotEventTrigger AllowChainAttackEventTrigger { get => m_AllowChainAttackEventTrigger; set => m_AllowChainAttackEventTrigger.CopyFrom(value); }

        public override bool IsActiveAttacking => m_ActiveAttacking;

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);

            m_AttackIndex = -1;

            for (int i = 0; i < m_Attacks.Length; i++) {
                var localI = i;

                var startTrigger = m_Attacks[i].ActiveAttackStartEventTrigger;
                var completeTrigger = m_Attacks[i].ActiveAttackCompleteEventTrigger;

                startTrigger.RegisterUnregisterEvent(register, Character, "OnAnimatorActiveAttackStart", SlotID, () => HandleActiveAttackStartAnimationEvent(localI));
                completeTrigger.RegisterUnregisterEvent(register, Character, "OnAnimatorMeleeAttackComplete", SlotID, () => HandleActiveAttackCompleteAnimationEvent(localI));
            }

            m_AllowChainAttackEventTrigger.RegisterUnregisterEvent(register, Character, "OnAnimatorAllowChainAttack", SlotID, HandleAllowChainAttackAnimationEvent);
        }

        /// <summary>
        /// Handle the animation event.
        /// </summary>
        protected virtual void HandleAllowChainAttackAnimationEvent()
        {
            StartAllowChainAttack();
        }

        /// <summary>
        /// Handle the animation event.
        /// </summary>
        /// <param name="attackIndex">The attack index of the event.</param>
        protected virtual void HandleActiveAttackStartAnimationEvent(int attackIndex)
        {
            if (attackIndex != m_AttackIndex) {
                Debug.LogWarning("The event does not match the attack index.");
            }
            ActiveAttackStart();
        }

        /// <summary>
        /// Handle the animation event.
        /// </summary>
        /// <param name="attackIndex">The attack index of the event.</param>
        protected virtual void HandleActiveAttackCompleteAnimationEvent(int attackIndex)
        {
            if (attackIndex != m_AttackIndex) {
                Debug.LogWarning("The event does not match the attack index.");
            }
            ActiveAttackComplete();
        }

        /// <summary>
        /// The attack can now transition into a combo.
        /// </summary>
        protected virtual void StartAllowChainAttack()
        {
            // Do nothing.
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            // All attacks must start first.
            if (m_AttackIndex >= 0) {
                if (m_Attacks.Length - 1 > m_AttackIndex) {
                    return false;
                }
                if (m_Attacks[m_AttackIndex].ActiveAttackStartEventTrigger.IsWaiting) {
                    return false;
                }
            }

            //Don't allow start with waiting for chain attack. Requires a trigger with AllowAttackCombos.
            if (m_AllowChainAttackEventTrigger.IsWaiting) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public override void StartItemUse(Use useAbility)
        {
            if (m_AttackIndex >= 0) {
                m_Attacks[m_AttackIndex].ActiveAttackStartEventTrigger.CancelWaitForEvent();
                m_Attacks[m_AttackIndex].ActiveAttackCompleteEventTrigger.CancelWaitForEvent();
            }

            m_AttackIndex = -1;
            m_CanceledAttack = false;
        }

        /// <summary>
        /// Start the attack.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void AttackStart(MeleeUseDataStream dataStream)
        {
            CharacterItemAction.DebugLogger.Log(this, "Attack Start");

            // Start at index 0 for a new attack.
            m_AttackIndex = 0;
            m_Attacks[m_AttackIndex].ActiveAttackStartEventTrigger.WaitForEvent(false);
        }

        /// <summary>
        /// Active attact starts
        /// </summary>
        public virtual void ActiveAttackStart()
        {
            m_ActiveAttacking = true;

            MeleeAction.OnActiveAttackStart(m_Attacks[m_AttackIndex].MeleeData);

            // start Wait for chain attack and active attack complete at the same time, such that chain attack can cancel active attack.
            m_AllowChainAttackEventTrigger.WaitForEvent(false);
            m_Attacks[m_AttackIndex].ActiveAttackCompleteEventTrigger.WaitForEvent(false);
        }

        /// <summary>
        /// Use item update when the update ticks.
        /// </summary>
        public override void UseItemUpdate()
        {
            // No need to update if the weapon has already hit an object and it can only hit a single object or a solid object (such as a shield) was hit.
            if (m_CanceledAttack || !m_ActiveAttacking) { return; }

            MeleeAction.CheckForCollision(m_Attacks[m_AttackIndex].MeleeData);
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {
            if (m_AttackIndex < 0 || m_AttackIndex >= m_Attacks.Length) {
                return;
            }
            m_Attacks[m_AttackIndex].ActiveAttackStartEventTrigger.CancelWaitForEvent();
            m_Attacks[m_AttackIndex].ActiveAttackCompleteEventTrigger.CancelWaitForEvent();
            m_AllowChainAttackEventTrigger.CancelWaitForEvent();
            m_ActiveAttacking = false;
            m_CanceledAttack = false;
            m_AttackIndex = -1;
        }

        /// <summary>
        /// Complete the attack.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void AttackComplete(MeleeUseDataStream dataStream)
        {
            ActiveAttackComplete();
        }

        /// <summary>
        /// Active attack complete.
        /// </summary>
        public void ActiveAttackComplete()
        {
            if (m_ActiveAttacking == false) {
                return;
            }

            m_ActiveAttacking = false;
            MeleeAction.OnActiveAttackComplete(m_Attacks[m_AttackIndex].MeleeData);

            m_Attacks[m_AttackIndex].ActiveAttackStartEventTrigger.CancelWaitForEvent();
            m_Attacks[m_AttackIndex].ActiveAttackCompleteEventTrigger.CancelWaitForEvent();
            if (m_AttackIndex >= m_Attacks.Length - 1) {
                // Finished the multi attack.
            } else {
                // Continue attacking.
                m_AttackIndex++;
                m_Attacks[m_AttackIndex].ActiveAttackStartEventTrigger.WaitForEvent(false);
            }
        }

        /// <summary>
        /// Cancel the attack.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void AttackCanceled(MeleeUseDataStream dataStream)
        {
            ActiveAttackComplete();
            m_CanceledAttack = true;
            if (m_AttackIndex != -1) {
                m_Attacks[m_AttackIndex].ActiveAttackStartEventTrigger.CancelWaitForEvent();
                m_Attacks[m_AttackIndex].ActiveAttackCompleteEventTrigger.CancelWaitForEvent();
            }
            m_AllowChainAttackEventTrigger.CancelWaitForEvent();
            m_ActiveAttacking = false;
        }

    }
}