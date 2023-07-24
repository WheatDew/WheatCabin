/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// ItemAbility which will start using the IUsableItem.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultStopType(AbilityStopType.ButtonUp)]
    [DefaultInputName("Fire1")]
    [DefaultItemStateIndex(2)]
    [DefaultState("Use")]
    [AllowDuplicateTypes]
    public class Use : ItemAbility
    {
        [Tooltip("The slot that should be used. -1 will use all of the slots.")]
        [SerializeField] protected int m_SlotID = -1;
        [Tooltip("The ID of the ItemAction component that can be used.")]
        [SerializeField] protected int m_ActionID;
        [Tooltip("Should the ability rotate the character to face the look source target?")]
        [SerializeField] protected bool m_RotateTowardsLookSourceTarget = true;
        [Tooltip("Should the use be blocked when the cursor is over a UI object?")]
        [SerializeField] protected bool m_BlockOverUI = true;

        public override int SlotID { get { return m_SlotID; } set { m_SlotID = value; } }
        public override int ActionID { get { return m_ActionID; } set { m_ActionID = value; } }
        public bool RotateTowardsLookSourceTarget { get { return m_RotateTowardsLookSourceTarget; } set { m_RotateTowardsLookSourceTarget = value; } }
        public bool BlockOverUI { get { return m_BlockOverUI; } set { m_BlockOverUI = value; } }

        private ILookSource m_LookSource;
        protected IUsableItem[] m_UsableItems;
        private IPlayerInput m_PlayerInput;
        private bool[] m_CanStopAbility;
        private bool m_Started;
        private bool m_AIAgent;

        public IUsableItem[] UsableItems { get { return m_UsableItems; } }
        public bool AIAgent => m_AIAgent;

        public virtual CharacterItem FaceTargetCharacterItem
        {
            get
            {
                for (int i = 0; i < m_UsableItems.Length; i++) {
                    if (m_UsableItems[i] != null && m_UsableItems[i].FaceTargetCharacterItem != null) {
                        return m_UsableItems[i].FaceTargetCharacterItem;
                    }
                }

                return null;
            }
        }

        public override bool CanReceiveMultipleStarts { get { return true; } }
#if UNITY_EDITOR
        public override string AbilityDescription {
            get {
                var description = string.Empty;
                if (m_SlotID != -1) {
                    description += "Slot " + m_SlotID;
                }
                if (m_ActionID != 0) {
                    if (!string.IsNullOrEmpty(description)) {
                        description += ", ";
                    }
                    description += "Action " + m_ActionID;
                }
                return description;
            } }
#endif

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_PlayerInput = m_GameObject.GetCachedComponent<IPlayerInput>();
            var count = m_SlotID == -1 ? m_Inventory.SlotCount : 1;
            m_UsableItems = new IUsableItem[count];
            m_CanStopAbility = new bool[count];
            // The look source may have already been assigned if the ability was added to the character after the look source was assigned.
            m_LookSource = m_CharacterLocomotion.LookSource;
            // AIAgents will have the LocalLookSource.
            m_AIAgent = m_LookSource is LocalLookSource;

            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.RegisterEvent<bool>(m_GameObject, "OnEnableGameplayInput", OnEnableGameplayInput);
            EventHandler.RegisterEvent<IUsableItem>(m_GameObject, "OnItemUseComplete", OnItemUseComplete);
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            m_LookSource = lookSource;
            m_AIAgent = m_LookSource is LocalLookSource;
        }

        /// <summary>
        /// Returns the Item State Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item State Index.</param>
        /// <returns>The Item State Index which corresponds to the slot ID.</returns>
        public override int GetItemStateIndex(int slotID)
        {
            // Return the ItemStateIndex if the SlotID matches the requested slotID.
            if (m_SlotID == -1) {
                if (m_UsableItems[slotID] != null) {
                    return m_ItemStateIndex;
                }
            } else if (m_SlotID == slotID && m_UsableItems[0] != null) {
                return m_ItemStateIndex;
            }
            return -1;
        }

        /// <summary>
        /// Returns the Item Substate Index which corresponds to the slot ID.
        /// </summary>
        /// <param name="slotID">The ID of the slot that corresponds to the Item Substate Index.</param>
        /// <returns>The Item Substate Index which corresponds to the slot ID.</returns>
        public override int GetItemSubstateIndex(int slotID)
        {
            if (m_SlotID == -1) {
                if (m_UsableItems[slotID] != null) {
                    return m_UsableItems[slotID].GetUseItemSubstateIndex();
                }
            } else if (m_SlotID == slotID && m_UsableItems[0] != null) {
                return m_UsableItems[0].GetUseItemSubstateIndex();
            }
            return -1;
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartAbility()
        {
            // An attribute may prevent the ability from starting.
            if (!base.CanStartAbility()) {
                return false;
            }

            // Don't use the item if the cursor is over any UI.
            if (m_BlockOverUI && m_PlayerInput != null && m_PlayerInput.IsPointerOverUI()) {
                return false;
            }

            // A look source must exist.
            if (m_LookSource == null) {
                return false;
            }

            // If the SlotID is -1 then the ability should use every equipped item at the same time. If only one slot has a UsableItem then the 
            // ability can start. If the SlotID is not -1 then the ability should use the item in the specified slot.
            var canUse = false;
            if (m_SlotID == -1) {
                for (int i = 0; i < m_UsableItems.Length; ++i) {
                    var item = m_Inventory.GetActiveCharacterItem(i);
                    if (item == null) {
                        continue;
                    }

                    var itemAction = item.GetItemAction(m_ActionID);
                    if (itemAction == null) {
                        continue;
                    }

                    m_UsableItems[i] = itemAction as IUsableItem;

                    // The item can't be used if it isn't a usable item.
                    if (m_UsableItems[i] != null) {
                        if (m_UsableItems[i].UseCompleted && !m_CanStopAbility[i] && m_UsableItems[i].IsItemInUse() && m_UsableItems[i].CanStopItemUse()) {
                            m_UsableItems[i].StopItemUse();
                        }

                        if (!m_UsableItems[i].CanStartUseItem(this, UsableAction.UseAbilityState.Start)) {
                            continue;
                        }
                        canUse = true;
                    }
                }
            } else {
                
                var item = m_Inventory.GetActiveCharacterItem(m_SlotID);
                if (item != null) {
                    var itemAction = item.GetItemAction(m_ActionID);
                    if (itemAction != null) {
                        m_UsableItems[0] = itemAction as IUsableItem;
                        // The item can't be used if it isn't a usable item.
                        if (m_UsableItems[0] != null) {
                            // If the item has completed use and is waiting on the CanStop event then it should reset so it can be used again.
                            if (m_UsableItems[0].UseCompleted && !m_CanStopAbility[0] && m_UsableItems[0].IsItemInUse() && m_UsableItems[0].CanStopItemUse()) {
                                m_UsableItems[0].StopItemUse();
                            }
                            if (m_UsableItems[0].CanStartUseItem(this, UsableAction.UseAbilityState.Start)) {
                                canUse = true;
                            }
                        }
                    }
                }
            }

            return canUse;
        }

        /// <summary>
        /// Does the ability use the specified ItemAction type?
        /// </summary>
        /// <param name="itemActionType">The ItemAction type to compare against.</param>
        /// <returns>True if the ability uses the specified ItemAction type.</returns>
        public bool UsesItemActionType(System.Type itemActionType)
        {
            // If the SlotID is -1 then the ability should can every equipped item at the same time. If only one slot has an action which is of the specified type
            // then the entire method will return true. If the SlotID is not -1 then the ability will only check against the single ItemAction.
            if (m_SlotID == -1) {
                for (int i = 0; i < m_UsableItems.Length; ++i) {
                    var item = m_Inventory.GetActiveCharacterItem(i);
                    if (item == null) {
                        continue;
                    }

                    var itemAction = item.GetItemAction(m_ActionID);
                    if (itemAction == null) {
                        return false;
                    }
                    // It only takes one ItemAction for the ability to use the specified ItemAction.
                    if (itemAction.GetType().IsAssignableFrom(itemActionType)) {
                        return true;
                    }
                }
            } else {
                var item = m_Inventory.GetActiveCharacterItem(m_SlotID);
                if (item != null) {
                    var itemAction = item.GetItemAction(m_ActionID);
                    if (itemAction == null) {
                        return false;
                    }
                    return itemAction.GetType().IsAssignableFrom(itemActionType);
                }
            }

            return false;
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            if (base.ShouldBlockAbilityStart(startingAbility)) {
                return true;
            }

            if (startingAbility is Use && startingAbility != this) {
                // The same item should not be able to be used by multiple use abilities at the same time. Different items can be used at the same time, such as
                // a primary item and a secondary grenade throw or dual pistols.
                var startingUseAbility = startingAbility as Use;
                for (int i = 0; i < m_UsableItems.Length; ++i) {
                    if (m_UsableItems[i] == null) {
                        continue;
                    }

                    for (int j = 0; j < startingUseAbility.UsableItems.Length; ++j) {
                        if (startingUseAbility.UsableItems[j] == null) {
                            continue;
                        }

                        if (m_UsableItems[i].CharacterItem == startingUseAbility.UsableItems[j].CharacterItem) {
                            return true;
                        }
                    }
                }
            }
            
            // Active items can block starting abilities.
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] == null) {
                    continue;
                }
                if (!m_UsableItems[i].CanStartAbility(startingAbility)) {
                    return true;
                }
            }

            if (startingAbility is Reload) {
                // The Use ability has priority over the Reload ability. Prevent the reload ability from starting if the use ability is active.
                if (startingAbility.InputIndex != -1) {
                    // If the item isn't actively being used then it shouldn't block reload.
                    var shouldBlock = false;
                    for (int i = 0; i < m_UsableItems.Length; ++i) {
                        if (m_UsableItems[i] != null && !m_UsableItems[i].UseCompleted) {
                            shouldBlock = true;
                            break;
                        }
                    }
                    if (!shouldBlock) {
                        return false;
                    }

                    var reloadAbility = startingAbility as Reload;
                    StopItemReload(reloadAbility);

                    // The ability should only be blocked if there aren't any items left to reload. An item may still be reloaded if it's parented to a different
                    // slot from what is being used.
                    shouldBlock = true;
                    for (int i = 0; i < reloadAbility.ReloadableItems.Length; ++i) {
                        if (reloadAbility.ReloadableItems[i] != null) {
                            shouldBlock = false;
                        }
                    }
                    return shouldBlock;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            // If Use starts while EquipUnequip is active then EquipUnequip should stop.
            if (activeAbility is EquipUnequip) {
                return true;
            }
            if (activeAbility is Reload) {
                // The Use ability has priority over the Reload ability. Stop Reload if it is currently reloading the item.
                StopItemReload(activeAbility as Reload);
            }
            return base.ShouldStopActiveAbility(activeAbility);
        }

        /// <summary>
        /// Stops any item that is trying to reload while it is being used.
        /// </summary>
        /// <param name="reloadAbility">A reference to the reload ability.</param>
        /// <returns>True if the same item is trying to be used and reloaded.</returns>
        private void StopItemReload(Reload reloadAbility)
        {
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] == null) {
                    continue;
                }

                for (int j = 0; j < reloadAbility.ReloadableItems.Length; ++j) {
                    if (reloadAbility.ReloadableItems[j] == null) {
                        continue;
                    }

                    if (m_UsableItems[i].CharacterItem == reloadAbility.ReloadableItems[j].CharacterItem) {
                        reloadAbility.StopItemReload(j);
                    }
                }
            }
        }

        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            // Shootable weapons will deduct the attribute on each use.
            var enableAttributeModifier = true;
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (enableAttributeModifier && m_UsableItems[i] != null && m_UsableItems[i] is ShootableAction) {
                    enableAttributeModifier = false;
                    break;
                }
            }
            base.AbilityStarted(enableAttributeModifier);

            // The item may require root motion to prevent sliding. It may also require the character to face the target before it can actually be used.
            var itemStartedUse = false;
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] == null) {
                    m_CanStopAbility[i] = true;
                    continue;
                }

                m_UsableItems[i].UseItemAbilityStarted(this);

                if (m_CanStopAbility[i]) {
                    continue;
                }

                itemStartedUse = true;
            }

            // The ability can start multiple times. Ensure the events are only subscribed to once.
            if (itemStartedUse && !m_Started) {
                EventHandler.ExecuteEvent(m_GameObject, "OnUseAbilityStart", true, this);
                m_Started = true;
            } else if (!itemStartedUse) {
                // The ability should be stopped if no items are being used.
                var stopAbility = true;
                for (int i = 0; i < m_UsableItems.Length; ++i) {
                    if (m_UsableItems[i] == null) {
                        continue;
                    }

                    if (!m_UsableItems[i].UseCompleted || !m_CanStopAbility[i]) {
                        stopAbility = false;
                        break;
                    }
                }
                if (stopAbility) {
                    StopAbility();
                }
            }
        }

        /// <summary>
        /// Updates the ability.
        /// </summary>
        public override void Update()
        {
            // Do not call the base method to prevent an attribute from stopping the use.
        }

        /// <summary>
        /// Updates the ability after the controller has updated. This will ensure the character is in the most up to date position.
        /// </summary>
        public override void LateUpdate()
        {
            // Enable the collision layer so the weapons can apply damage the originating character.
            var collisionEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            m_CharacterLocomotion.EnableColliderCollisionLayer(true);

            // Tries to use the item. This is done within Update because the item can be used multiple times when the input button is held down.
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] != null) {

                    // Allow the items currently in use to be updated.
                    m_UsableItems[i].UseItemUpdate(this);
                }
            }

            m_CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);
        }

        /// <summary>
        /// Is the Use Input trying to stop the ability.
        /// </summary>
        /// <returns>Return true if the input is trying to stop the ability.</returns>
        public bool IsUseInputTryingToStop()
        {
            // The CanInputStartAbility might only happen on a single frame. But the CanInputStopAbility is continuous.
            if (CanInputStopAbility(m_PlayerInput)) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the usable item index.
        /// </summary>
        /// <param name="usableItem">The usable item to get the index for.</param>
        /// <returns>The usable item index (-1 if none found).</returns>
        private int GetUsableItemIndex(IUsableItem usableItem)
        {
            for (int i = 0; i < m_UsableItems.Length; i++) {
                if (m_UsableItems[i] == usableItem) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Set that the usable item can or cannot be stopped by the ability.
        /// </summary>
        /// <param name="usableItem">The usable item to stop or continue using.</param>
        /// <param name="canStop">can the item be stopped or not?</param>
        public void SetCanStopAbility(IUsableItem usableItem, bool canStop)
        {
            var index = GetUsableItemIndex(usableItem);
            if (index == -1) {
                return;
            }
            
            m_CanStopAbility[index] = canStop;

            if (canStop == false) {
                return;
            }
            
            // The ability should be stopped if all items have finished being used.
            var stopAbility = true;
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if(m_UsableItems[i] == null){ continue; }
                
                if (!m_UsableItems[i].UseCompleted) {
                    stopAbility = false;
                    break;
                }
            }

            if (stopAbility) {
                StopAbility();
            }
        }

        /// <summary>
        /// Update the controller's rotation values.
        /// </summary>
        public override void UpdateRotation()
        {
            for (int i = 0; i < m_UsableItems.Length; i++) {
                if (m_UsableItems[i] == null) { continue; }

                if (m_UsableItems[i].IsItemInUse()) {
                    m_UsableItems[i].UpdateRotation(this, m_RotateTowardsLookSourceTarget);
                }
            }
        }

        /// <summary>
        /// An item completed its use.
        /// </summary>
        /// <param name="usableItem">The item that completed its use.</param>
        protected virtual void OnItemUseComplete(IUsableItem usableItem)
        {
            for (int i = 0; i < m_UsableItems.Length; i++) {
                if (m_UsableItems[i] == usableItem) {
                    UseCompleteItem(i);
                }
            }
        }
        
        /// <summary>
        /// The animator has finished playing the use animation.
        /// </summary>
        /// <param name="slotID">The id of the slot that was used.</param>
        protected virtual void UseCompleteItem(int slotID)
        {
            var usableItem = m_UsableItems[slotID];
            if (usableItem == null || !m_UsableItems[slotID].WaitingForUseCompleteEvent) {
                return;
            }

            // The ability should stop when all the items have been used.
            var stopAbility = true;
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] == null) {
                    continue;
                }

                if (!m_UsableItems[i].UseCompleted || !m_CanStopAbility[i]) {
                    stopAbility = false;
                    break;
                }
            }
            if (stopAbility) {
                StopAbility();
            }
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <param name="force">Should the ability be force stopped?</param>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility(bool force)
        {
            if (force) { return true; }

            for (int i = 0; i < m_UsableItems.Length; ++i) {
                // If the item is currently being used and it cannot be stopped then the ability cannot stop either.
                if (m_UsableItems[i] != null && m_UsableItems[i].IsItemInUse()) {
                    if (m_UsableItems[i].TryStopItemUse() == false) {
                        return false;
                    }
                }
                // Don't stop if CanStopAbility is false. This will allow hip firing to keep the item held up momentarily after being used. The ability should always
                // be able to stop during a reload.
                if (!m_CanStopAbility[i] && !m_CharacterLocomotion.IsAbilityTypeActive<Reload>()) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force);

            // The item may require root motion to prevent sliding.
            for (int i = 0; i < m_UsableItems.Length; ++i) {
                if (m_UsableItems[i] != null) {
                   
                    m_UsableItems[i].ItemAbilityStopped(this);
                    EventHandler.ExecuteEvent(m_GameObject, "OnItemStartUse", m_UsableItems[i], false);
                    m_UsableItems[i] = null;
                }
            }

            m_Started = false;
            EventHandler.ExecuteEvent(m_GameObject, "OnUseAbilityStart", false, this);
        }

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        private void OnEnableGameplayInput(bool enable)
        {
            // Force stop the ability if the character no longer has input.
            if (!enable && IsActive) {
                StopAbility(true);
            }
        }

        /// <summary>
        /// Called when the character is destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnEnableGameplayInput", OnEnableGameplayInput);
            EventHandler.UnregisterEvent<IUsableItem>(m_GameObject, "OnItemUseComplete", OnItemUseComplete);
        }
    }
}