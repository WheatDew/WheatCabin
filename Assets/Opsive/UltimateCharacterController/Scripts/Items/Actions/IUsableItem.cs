/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions
{
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Utility;

    /// <summary>
    /// Interface for an item that can be used (fired, swung, thrown, etc).
    /// </summary>
    public interface IUsableItem
    {
        /// <summary>
        /// Returns the next time the item can be used.
        /// </summary>
        float NextAllowedUseTime { get; }

        /// <summary>
        /// The last time the item was used.
        /// </summary>
        float LastUseTime { get; }

        /// <summary>
        /// Is the item in use?
        /// </summary>
        bool InUse { get; }

        /// <summary>
        /// Is the item waiting for the use event?
        /// </summary>
        bool WaitingForUseEvent { get; }

        /// <summary>
        /// Is the item waiting for the use complete event?
        /// </summary>
        bool WaitingForUseCompleteEvent { get; }

        /// <summary>
        /// Has the use completed?
        /// </summary>
        bool UseCompleted { get; }
        
        /// <summary>
        /// Returns the character Item to face.
        /// </summary>
        CharacterItem FaceTargetCharacterItem { get; }

        /// <summary>
        /// Returns the item that the UsableItem is attached to.
        /// </summary>
        /// <returns>The item that the UsableItem is attached to.</returns>
        CharacterItem CharacterItem { get; }

        /// <summary>
        /// Returns true if the inventory can equip this item.
        /// </summary>
        /// <returns>True if the inventory can equip this item.</returns>
        bool CanEquip();

        /// <summary>
        /// Returns true if the character should turn to face the target.
        /// </summary>
        /// <returns>True if the character should turn to face the target.</returns>
        bool FaceTarget { get; }

        /// <summary>
        /// Does the item require root motion position during use?
        /// </summary>
        /// <returns>True if the item requires root motion position during use.</returns>
        bool ForceRootMotionPosition { get; }

        /// <summary>
        /// Does the item require root motion rotation during use?
        /// </summary>
        /// <returns>True if the item requires root motion rotation during use.</returns>
        bool ForceRootMotionRotation { get; }

        /// <summary>
        /// Specifies if the item should wait for the OnAnimatorItemUse animation event or wait for the specified duration before reloading.
        /// </summary>
        /// <returns>Value of if the item should use the OnAnimatorItemUse animation event or wait the specified duration.</returns>
        AnimationSlotEventTrigger UseEvent { get; }

        /// <summary>
        /// Specifies if the item should wait for the OnAnimatorItemUseComplete animation event or wait for the specified duration before reloading.
        /// </summary>
        /// <returns>Value of if the item should use the OnAnimatorItemUseComplete animation event or wait the specified duration.</returns>
        AnimationSlotEventTrigger UseCompleteEvent { get; }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState);

        /// <summary>
        /// Can the ability be started?
        /// </summary>
        /// <param name="ability">The ability that is trying to start.</param>
        /// <returns>True if the ability can be started.</returns>
        bool CanStartAbility(Ability ability);

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <returns></returns>
        bool CanUseItem();

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        void UseItemAbilityStarted(Use useItemAbility);

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        void StartItemUse(Use useItemAbility);

        /// <summary>
        /// Update the player rotation.
        /// </summary>
        void UpdateRotation(ItemAbility itemAbility, bool rotateTowardLookAtTarget);

        /// <summary>
        /// Uses the item.
        /// </summary>
        void UseItem();

        /// <summary>
        /// Returns the substate index that the item should be in.
        /// </summary>
        /// <returns>Returns the substate index that the item should be in.</returns>
        int GetUseItemSubstateIndex();

        /// <summary>
        /// Is the item in use?
        /// </summary>
        /// <returns>Returns true if the item is in use.</returns>
        bool IsItemInUse();

        /// <summary>
        /// Is the item waiting to be used? This will return true if the item is waiting to be charged or pulled back.
        /// </summary>
        /// <returns>Returns true if the item is waiting to be used.</returns>
        bool IsItemUsePending();

        /// <summary>
        /// Allows the item to update while it is being used.
        /// </summary>
        void UseItemUpdate(Use useItemAbility);

        /// <summary>
        /// The item has been used.
        /// </summary>
        void ItemUseComplete();

        /// <summary>
        /// Tries to stop the item use.
        /// </summary>
        bool TryStopItemUse();

        /// <summary>
        /// Can the item use be stopped?
        /// </summary>
        /// <returns>True if the item use can be stopped.</returns>
        bool CanStopItemUse();

        /// <summary>
        /// Stops the item use.
        /// </summary>
        void StopItemUse();
        
        /// <summary>
        /// Stops the item use.
        /// </summary>
        void ItemAbilityStopped(ItemAbility itemAbility);

        /// <summary>
        /// Should the item be unequipped?
        /// </summary>
        /// <returns>True if the item should be unequipped.</returns>
        bool ShouldUnequip();
    }
}