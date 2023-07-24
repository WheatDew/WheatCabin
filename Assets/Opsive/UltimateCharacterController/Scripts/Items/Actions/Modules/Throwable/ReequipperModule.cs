/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable
{
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// The base Re-equipper module used to re-equip a throwable item after one was thrown.
    /// </summary>
    [Serializable]
    public abstract class ThrowableReequipperModule : ThrowableActionModule,
        IModuleCanStartUseItem, IModuleGetUseItemSubstateIndex, IModuleStartItemUse, IModuleItemUseComplete, IModuleCanStopItemUse,
        IModuleStopItemUse, IModuleCanEquip
    {
        /// <summary>
        /// Is the item currently being re-equipped?
        /// </summary>
        public abstract bool IsReequipping { get; }
        
        /// <summary>
        /// Was the item Reequipped?
        /// </summary>
        public abstract bool WasReequipped { get; }
        
        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public abstract bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState);
        
        /// <summary>
        /// Get the Item Substate Index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public abstract void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData);

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public abstract void StartItemUse(Use useAbility);

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public abstract void ItemUseComplete();

        /// <summary>
        /// Can the item be stopped?
        /// </summary>
        /// <returns>True if the item can be stopped.</returns>
        public abstract bool CanStopItemUse();

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public abstract void StopItemUse();

        /// <summary>
        /// Can the item be equipped.
        /// </summary>
        /// <returns>True if the item can be equipped.</returns>
        public abstract bool CanEquip();
    }
    
    /// <summary>
    /// The basic Re-equipper module used to re-equip a throwable item after one was thrown.
    /// </summary>
    [Serializable]
    public class SimpleReequipper : ThrowableReequipperModule
    {
        [Tooltip("Can the item be equipped when there are no more projectiles to throw?")]
        [SerializeField] protected bool m_CanEquipEmptyItem;
        [Tooltip("The value of the Item Substate Animator parameter when the item is being reequipped.")]
        [SerializeField] protected ItemSubstateIndexData m_SubstateIndexData = new ItemSubstateIndexData(10, 150);
        [Tooltip("Specifies if the item should wait for the OnAnimatorReequipThrowableItem animation event or wait for the specified duration before requipping.")]
        [SerializeField] protected AnimationSlotEventTrigger m_ReequipEvent = new AnimationSlotEventTrigger(false, 0.5f);

        public bool CanEquipEmptyItem { get => m_CanEquipEmptyItem; set => m_CanEquipEmptyItem = value; }
        public ItemSubstateIndexData SubstateIndexData { get => m_SubstateIndexData; set => m_SubstateIndexData = value; }
        public AnimationSlotEventTrigger ReequipEvent { get => m_ReequipEvent; set => m_ReequipEvent.CopyFrom(value); }
        
        private bool m_Reequipping;
        private bool m_WasReequipped;
        private int m_ReequipFrame;
        private bool m_NextItemSet;

        public override bool IsReequipping => m_Reequipping || m_NextItemSet;

        public override bool WasReequipped => m_WasReequipped;

        /// <summary>
        /// Can the item be equipped.
        /// </summary>
        /// <returns>True if the item can be equipped.</returns>
        public override bool CanEquip()
        {
            if(m_CanEquipEmptyItem) {
                return true;
            }
            return ThrowableAction.GetNextAmmoData().Valid;
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);
            
            m_ReequipEvent.RegisterUnregisterEvent(register, Character, "OnAnimatorReequipThrowableItem", SlotID, HandleReequipeAnimationEvent);
        }

        /// <summary>
        /// Handle the animation event for re-equipping.
        /// </summary>
        private void HandleReequipeAnimationEvent()
        {
            ReequipThrowableItem();
        }

        /// <summary>
        /// Get the Item Substate Index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public override void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData)
        {
            if (!m_Reequipping) { return; }
            
            streamData.TryAddSubstateData(this, m_SubstateIndexData);
        }

        /// <summary>
        /// The item was equipped.
        /// </summary>
        public override void Equip()
        {
            base.Equip();
            m_NextItemSet = false;
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            // The item can't be used if it is already being used.
            if (abilityState == UsableAction.UseAbilityState.Start && m_Reequipping) {
                return false;
            }
            
            // The item can't be used if it hasn't been started yet, is reequipping the throwable item, or has been requipped.
            if (abilityState == UsableAction.UseAbilityState.Update && (m_Reequipping || m_WasReequipped)) {
                return false;
            }

            // Give the item a frame to recover from the reequip.
            if (Time.frameCount == m_ReequipFrame) {
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
            m_Reequipping = false;
            m_WasReequipped = false;
            UpdateItemAbilityAnimatorParameters();
        }

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public override void ItemUseComplete()
        {
            var nextAmmoData = ThrowableAction.GetNextAmmoData();
            if (nextAmmoData.Valid) {
                m_Reequipping = true;
                m_ReequipEvent.WaitForEvent(false);
            }
        }

        /// <summary>
        /// Can the item be stopped?
        /// </summary>
        /// <returns>True if the item can be stopped.</returns>
        public override bool CanStopItemUse()
        {
            // The item can't be stopped until the object has been thrown and is not reequipping the object.
            if (m_Reequipping) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {
            // If there are no items remaining then the next item should be equipped. Wait until the item use is stopped so the use animation will complete.
            if (!ThrowableAction.GetNextAmmoData().Valid) {
                Shared.Events.EventHandler.ExecuteEvent(Character, "OnNextItemSet", CharacterItem, true);
                m_NextItemSet = true;
            }
            
            m_Reequipping = m_WasReequipped = false;
        }

        /// <summary>
        /// The ThrowableItem has been reequipped.
        /// </summary>
        private void ReequipThrowableItem()
        {
            if (!m_Reequipping) {
                return;
            }

            m_ReequipEvent.CancelWaitForEvent();
            m_Reequipping = false;
            m_WasReequipped = true;
            m_ReequipFrame = Time.frameCount;
            
            UpdateItemAbilityAnimatorParameters();

            ThrowableAction.OnReequipThrowableItem();
        }

        /// <summary>
        /// The item was removed from the character.
        /// </summary>
        public override void RemoveItem()
        {
            base.RemoveItem();
            m_Reequipping = m_WasReequipped = false;
        }

        /// <summary>
        /// Reset the module after the item has been unequipped or removed.
        /// </summary>
        /// <param name="force">Force the reset.</param>
        public override void ResetModule(bool force)
        {
            base.ResetModule(force);
            if (force) {
                m_Reequipping = m_WasReequipped = false;
            }

            m_ReequipFrame = 0;
            m_NextItemSet = false;
        }
    }
}