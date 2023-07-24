/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules
{
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Inventory;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The base Character Item Action Module.
    /// </summary>
    public interface IActionModule
    {
        public int ID { get; }

        /// <summary>
        /// Initialize the Character Item Action Module use the Item Action and the module Group. 
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        /// <param name="moduleGroup">The parent Module Group.</param>
        void Initialize(CharacterItemAction itemAction, ActionModuleGroupBase moduleGroup);
    }

    /// <summary>
    /// A Character Item Action Module for Aiming.
    /// </summary>
    public interface IModuleOnAim : IActionModule
    {
        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="aim">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        public void OnAim(bool aim, bool inputStart);
    }
    
    /// <summary>
    /// A Character Item Action Module for Changing perspective.
    /// </summary>
    public interface IModuleOnChangePerspectives: IActionModule
    {
        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="firstPersonPerspective">Changed to first person?</param>
        public void OnChangePerspectives(bool firstPersonPerspective);
    }

    /// <summary>
    /// A Character Item Action Module for knowing if the item can be equipped.
    /// </summary>
    public interface IModuleCanEquip : IActionModule
    {
        /// <summary>
        /// Can the item be equipped.
        /// </summary>
        /// <returns>True if the item can be equipped.</returns>
        public bool CanEquip();
    }
    
    /// <summary>
    /// A Character Item Action Module for Changing perspective.
    /// </summary>
    public interface IModuleGetItemsToDrop: IActionModule
    {
        /// <summary>
        /// Get the items to drop by adding it to the list.
        /// </summary>
        /// <param name="itemsToDrop">The list of items to drop, the item to drop will be added to this list.</param>
        public void GetItemsToDrop(List<ItemIdentifierAmount> itemsToDrop);
    }
    
    /// <summary>
    /// A Character Item Action Module for knowing if the visible object can be activated.
    /// </summary>
    public interface IModuleCanActivateVisibleObject : IActionModule
    {
        /// <summary>
        /// Can the visible object the activated.
        /// </summary>
        /// <returns>True if the visible object can be activated.</returns>
        public bool CanActivateVisibleObject();
    }
    
    /// <summary>
    /// A Character Item Action Module for knowing if the item can start being used.
    /// </summary>
    public interface IModuleCanStartUseItem : IActionModule
    {
        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState);
    }

    /// <summary>
    /// A Character Item Action Module for knowing if the item can be used.
    /// </summary>
    public interface IModuleCanUseItem : IActionModule
    {
        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <returns>True if the item can be used.</returns>
        public bool CanUseItem();
    }
    
    /// <summary>
    /// A Character Item Action Module for knowing if the item can start the ability.
    /// </summary>
    public interface IModuleCanStartAbility : IActionModule
    {
        /// <summary>
        /// Can the ability start?
        /// </summary>
        /// <param name="ability">The ability trying to start.</param>
        /// <returns>True if it can start.</returns>
        public bool CanStartAbility(Ability ability);
    }
    
    /// <summary>
    /// A Character Item Action Module for when the use ability has started.
    /// </summary>
    public interface IModuleUseItemAbilityStarted : IActionModule
    {
        /// <summary>
        /// The use item ability has started.
        /// </summary>
        /// <param name="useAbility">The use item ability that has started.</param>
        public void UseItemAbilityStarted(Use useAbility);
    }
    
    /// <summary>
    /// A Character Item Action Module for when the item has started to be used.
    /// </summary>
    public interface IModuleStartItemUse : IActionModule
    {
        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public void StartItemUse(Use useAbility);
    }

    /// <summary>
    /// A Character Item Action Module for when the item is used.
    /// </summary>
    public interface IModuleUseItem : IActionModule
    {
        /// <summary>
        /// Use the item.
        /// </summary>
        public void UseItem();
    }
    
    /// <summary>
    /// A Character Item Action Module for knowing if the item is use pending.
    /// </summary>
    public interface IModuleIsItemUsePending : IActionModule
    {
        /// <summary>
        /// Is the item use pending, meaning it has started but isn't ready to be used just yet.
        /// </summary>
        /// <returns>True if the item is use pending.</returns>
        public bool IsItemUsePending();
    }

    /// <summary>
    /// A Character Item Action Module for getting the Use Item Substate, which is used to animate the character.
    /// </summary>
    public interface IModuleGetUseItemSubstateIndex : IActionModule
    {
        /// <summary>
        /// Get the Item Substate Index.
        /// </summary>
        /// <param name="streamData">The stream data containing the other module data which affect the substate index.</param>
        public void GetUseItemSubstateIndex(ItemSubstateIndexStreamData streamData);
    }
    
    /// <summary>
    /// A Character Item Action Module for knowing when the Use Item Update ticks.
    /// </summary>
    public interface IModuleUseItemUpdate : IActionModule
    {
        /// <summary>
        /// Use item update when the update ticks.
        /// </summary>
        public void UseItemUpdate();
    }
    
    /// <summary>
    /// A Character Item Action Module for knowing when the Item is use complete.
    /// </summary>
    public interface IModuleItemUseComplete : IActionModule
    {
        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public void ItemUseComplete();
    }
    
    /// <summary>
    /// A Character Item Action Module for when the ability try to stop the item use.
    /// </summary>
    public interface IModuleTryStopItemUse : IActionModule
    {
        /// <summary>
        /// The item is trying to stop the use.
        /// </summary>
        public void TryStopItemUse();
    }
    
    /// <summary>
    /// A Character Item Action Module for knowing if the item can be stopped from being used.
    /// </summary>
    public interface IModuleCanStopItemUse : IActionModule
    {
        /// <summary>
        /// Can the item be stopped?
        /// </summary>
        /// <returns>True if the item can be stopped.</returns>
        public bool CanStopItemUse();
    }
    
    /// <summary>
    /// A Character Item Action Module for knowing when the item has stopped being used.
    /// </summary>
    public interface IModuleStopItemUse : IActionModule
    {
        /// <summary>
        /// Stop the item use.
        /// </summary>
        public void StopItemUse();
    }
    
    /// <summary>
    /// A Character Item Action Module for knowing when the Item Ability has stopped.
    /// </summary>
    public interface IModuleItemAbilityStopped : IActionModule
    {
        /// <summary>
        /// The item ability has stopped.
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        public void ItemAbilityStopped(ItemAbility useAbility);
    }

    /// <summary>
    /// A Character Item Action Module used by Trigger modules. It contains all the basic interfaces required for good trigger.
    /// </summary>
    public interface IModuleTrigger : IModuleCanUseItem, IModuleCanStartUseItem, IModuleCanStopItemUse,
        IModuleStartItemUse, IModuleUseItem, IModuleUseItemUpdate, IModuleIsItemUsePending, IModuleItemUseComplete,
        IModuleStopItemUse, IModuleTryStopItemUse, IModuleGetUseItemSubstateIndex
    {
        
    }

    /// <summary>
    /// A Character Item Action Module used to define what information to show in the Slot Item Monitor.
    /// </summary>
    public interface IModuleSlotItemMonitor : IActionModule
    {
        public int Priority { get; }
        
        /// <summary>
        /// Try get the loaded number of ammo in the clip.
        /// </summary>
        /// <param name="loadedCount">The loaded count in the clip.</param>
        /// <returns>True if the loaded count exists.</returns>
        public bool TryGetLoadedCount(out string loadedCount);
        
        /// <summary>
        /// Try get the unloaded count.
        /// </summary>
        /// <param name="unloadedCount">The unloaded count.</param>
        /// <returns>True if there is an unloaded count.</returns>
        public bool TryGetUnLoadedCount(out string unloadedCount);
        
        /// <summary>
        /// Try get the item icon.
        /// </summary>
        /// <param name="itemIcon">The item icon.</param>
        /// <returns>True if the item icon exists.</returns>
        public bool TryGetItemIcon(out Sprite itemIcon);
    }

    /// <summary>
    /// A Character Item Action Module used to switch which modules are active/enabled.
    /// </summary>
    public interface IModuleSwitcher : IActionModule
    {
        GameObject gameObject { get; }

        /// <summary>
        /// The name to display for the currently selected index.
        /// </summary>
        /// <returns>The name for the current index.</returns>
        string GetIndexName();
        
        /// <summary>
        /// The icon to display for the currently selected index.
        /// </summary>
        /// <returns>The icon for the current index.</returns>
        Sprite GetIndexIcon();

        /// <summary>
        /// Switch to a specific index.
        /// </summary>
        /// <param name="index">The index to switch to.</param>
        void SwitchTo(int index);

        /// <summary>
        /// Switch to the previous index.
        /// </summary>
        void SwitchToPrevious();

        /// <summary>
        /// Switch to the next index.
        /// </summary>
        void SwitchToNext();
    }

    /// <summary>
    /// A Character Item Action Module interface specifying that the module consumes an Item Definition.
    /// </summary>
    public interface IModuleItemDefinitionConsumer
    {
        /// <summary>
        /// The Item Definition used by the module.
        /// </summary>
        Shared.Inventory.ItemDefinitionBase ItemDefinition { get; set; }

        /// <summary>
        /// Returns the remaining Item Definition count.
        /// </summary>
        /// <returns>The remaining Item Definition count.</returns>
        int GetItemDefinitionRemainingCount();

        /// <summary>
        /// Sets the remaining Item Definition count.
        /// </summary>
        /// <param name="count">The amount to set.</param>
        void SetItemDefinitionRemainingCount(int count);
    }
}