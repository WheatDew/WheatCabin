/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.Shared.Networking;
#endif
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Events;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking.Character;
#endif
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Utility;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Provides a common base class for any character Inventory.
    /// </summary>
    public abstract class InventoryBase : MonoBehaviour
    {
        [Tooltip("Should Character Item instance be automatically spawned or destoryed when the Item Identifier is added or removed using the linked prefabs on the ItemDefinition?")]
        [SerializeField] protected bool m_AutoSpawnDestroyRuntimeCharacterItems = true;
        [Tooltip("Should Character Items be automatically removed when the item identifier is removed? (removed Character Items can optionally be destroyed).")]
        [SerializeField] protected bool m_AutoRemoveCharacterItems = true;
        [Tooltip("Unequip all the items on death")]
        [SerializeField] protected bool m_UnequipAllOnDeath;
        [Tooltip("Should the default loadout be loaded when the character respawns?")]
        [SerializeField] protected bool m_LoadDefaultLoadoutOnRespawn = false;
        [Tooltip("The name of the state when the inventory is unequipped.")]
        [SerializeField] protected string m_UnequippedStateName = "Unequipped";
        [Tooltip("Should the inventory prevent item pickups if the Inventory component is disabled?")]
        [SerializeField] protected bool m_PreventPickupIfDisabled = false;
        [Tooltip("The default Drop prefab to use for item identifiers that do not have a drop prefab.")]
        [SerializeField] protected GameObject m_DropPrefab;
        [Tooltip("The offset of the dropped prefab.")]
        [SerializeField] protected Vector3 m_DropOffset;
        [Tooltip("Unity event that is invoked when an item is initially added to the inventory.")]
        [SerializeField] protected UnityItemEvent m_OnAddItemEvent;
        [Tooltip("Unity event that is invoked when an IItemIdentifier is picked up.")]
        [SerializeField] protected UnityItemIdentifierFloatBoolBoolEvent m_OnPickupItemIdentifierEvent;
        [Tooltip("Unity event that is invoked when an item is picked up.")]
        [SerializeField] protected UnityItemFloatBoolBoolEvent m_OnPickupItemEvent;
        [Tooltip("Unity event that is invoked when an item is equipped.")]
        [SerializeField] protected UnityItemIntEvent m_OnEquipItemEvent;
        [Tooltip("Unity event that is invoked when an IItemIdentifier is adjusted.")]
        [SerializeField] protected UnityItemIdentifierFloatEvent m_OnAdjustItemIdentifierAmountEvent;
        [Tooltip("Unity event that is invoked when an item is unequipped.")]
        [SerializeField] protected UnityItemIntEvent m_OnUnequipItemEvent;
        [Tooltip("Unity event that is invoked when an item is removed.")]
        [SerializeField] protected UnityItemIntEvent m_OnRemoveItemEvent;

        public bool AutoSpawnDestroyRuntimeCharacterItems { get { return m_AutoSpawnDestroyRuntimeCharacterItems; } set { m_AutoSpawnDestroyRuntimeCharacterItems = value; } }
        public bool AutoRemoveCharacterItems { get { return m_AutoRemoveCharacterItems; } set { m_AutoRemoveCharacterItems = value; } }

        public bool UnequipAllOnDeath{ get { return m_UnequipAllOnDeath; } set { m_UnequipAllOnDeath = value; } }
        public bool LoadDefaultLoadoutOnRespawn { get { return m_LoadDefaultLoadoutOnRespawn; } set { m_LoadDefaultLoadoutOnRespawn = value; } }
        public string UnequippedStateName { get { return m_UnequippedStateName; } set { m_UnequippedStateName = value; } }
        public GameObject DropPrefab { get { return m_DropPrefab; } set { m_DropPrefab = value; } }
        public Vector3 DropOffset { get { return m_DropOffset; } set { m_DropOffset = value; } }
        public UnityItemEvent OnAddItemEvent { get { return m_OnAddItemEvent; } set { m_OnAddItemEvent = value; } }
        public UnityItemIdentifierFloatBoolBoolEvent OnPickupItemIdentifierEvent { get { return m_OnPickupItemIdentifierEvent; } set { m_OnPickupItemIdentifierEvent = value; } }
        public UnityItemFloatBoolBoolEvent OnPickupItemEvent { get { return m_OnPickupItemEvent; } set { m_OnPickupItemEvent = value; } }
        public UnityItemIntEvent OnEquipItemEvent { get { return m_OnEquipItemEvent; } set { m_OnEquipItemEvent = value; } }
        public UnityItemIdentifierFloatEvent OnAdjustItemIdentifierAmountEvent { get { return m_OnAdjustItemIdentifierAmountEvent; } set { m_OnAdjustItemIdentifierAmountEvent = value; } }
        public UnityItemIntEvent OnUnequipItemEvent { get { return m_OnUnequipItemEvent; } set { m_OnUnequipItemEvent = value; } }
        public UnityItemIntEvent OnRemoveItemEvent { get { return m_OnRemoveItemEvent; } set { m_OnRemoveItemEvent = value; } }

        protected GameObject m_GameObject;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        protected INetworkInfo m_NetworkInfo;
        protected INetworkCharacter m_NetworkCharacter;
#endif

        protected int m_SlotCount = 1;
        // All character Items include all character items which are currently within the Character hierarchy.
        protected List<CharacterItem> m_AllCharacterItems = new List<CharacterItem>();
        // Valid Character Items include character items on the character which have been picked up and not removed.
        protected List<CharacterItem> m_ValidCharacterItems = new List<CharacterItem>();
        protected List<IItemIdentifier> m_AllItemIdentifiers = new List<IItemIdentifier>();
        protected List<CharacterItem>[] m_CharacterItemsBySlot;
        protected Dictionary<CharacterItem, bool> m_CharacterItemsWaitingToBeRemoved = new Dictionary<CharacterItem, bool>();

        protected ItemPlacement m_ItemPlacement;
        protected bool m_InitializedSlotCount = false;

        public int SlotCount
        {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) { DetermineSlotCount(); }
#endif
                if (!m_InitializedSlotCount) { DetermineSlotCount(); }
                return m_SlotCount;
            }
        }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            if (!Game.CharacterInitializer.AutoInitialization) {
                Game.CharacterInitializer.Instance.OnAwake += AwakeInternal;
                Game.CharacterInitializer.Instance.OnStart += StartInternal;
                return;
            }

            AwakeInternal();
        }

        /// <summary>
        /// Internal method which initializes the default values.
        /// </summary>
        protected virtual void AwakeInternal()
        {
            if (Game.CharacterInitializer.Instance != null) {
                Game.CharacterInitializer.Instance.OnAwake -= AwakeInternal;
            }

            m_GameObject = gameObject;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = m_GameObject.GetCachedComponent<INetworkInfo>();
            m_NetworkCharacter = m_GameObject.GetCachedComponent<INetworkCharacter>();
#endif

            DetermineSlotCount();

            m_ItemPlacement = m_GameObject.GetComponentInChildren<ItemPlacement>(true);

            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }

        /// <summary>
        /// Determines the number of slots on the character.
        /// </summary>
        public virtual void DetermineSlotCount()
        {
            var previousAmount = m_SlotCount;
            // The number of slots depends on the maximum number of ItemSlot IDs.
            var itemSlots = GetComponentsInChildren<CharacterItemSlot>(true);
            for (int i = 0; i < itemSlots.Length; ++i) {
                if (m_SlotCount <= itemSlots[i].ID) {
                    m_SlotCount = itemSlots[i].ID + 1;
                }
            }
            m_InitializedSlotCount = true;

            OnSlotCountChange(previousAmount, m_SlotCount);
        }

        /// <summary>
        /// Refresh the Character Items arrays and dictionary when the slot count changes.
        /// </summary>
        /// <param name="previousAmount">The previous slot count.</param>
        /// <param name="newAmount">The new slot count.</param>
        protected virtual void OnSlotCountChange(int previousAmount, int newAmount)
        {
            if (m_CharacterItemsBySlot == null) {
                m_CharacterItemsBySlot = new List<CharacterItem>[newAmount];
                for (int i = 0; i < m_CharacterItemsBySlot.Length; i++) {
                    m_CharacterItemsBySlot[i] = new List<CharacterItem>();
                }
                return;
            }

            Array.Resize(ref m_CharacterItemsBySlot, newAmount);
            for (int i = 0; i < m_CharacterItemsBySlot.Length; i++) {
                if (m_CharacterItemsBySlot[i] != null) { continue; }

                m_CharacterItemsBySlot[i] = new List<CharacterItem>();
            }
        }

        /// <summary>
        /// Loads the default loadout.
        /// </summary>
        private void Start()
        {
            if (!Game.CharacterInitializer.AutoInitialization) {
                return;
            }

            StartInternal();
        }

        /// <summary>
        /// Internal method which loads the default loadout.
        /// </summary>
        protected virtual void StartInternal()
        {
            if (Game.CharacterInitializer.Instance != null) {
                Game.CharacterInitializer.Instance.OnStart -= StartInternal;
            }

            // The character starts out unequipped.
            if (!string.IsNullOrEmpty(m_UnequippedStateName)) {
                StateManager.SetState(m_GameObject, m_UnequippedStateName, true);
            }

            InitializePreAddedCharacterItems();

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo == null || m_NetworkInfo.IsLocalPlayer() || m_NetworkInfo.HasAuthority()) {
#endif
                LoadDefaultLoadout();
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            }
#endif

            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterSnapAnimator", true);
        }

        /// <summary>
        /// This function will scan the Items in the ItemPlacement component and initialize all the CharacterItems.
        /// </summary>
        protected virtual void InitializePreAddedCharacterItems()
        {
            if (m_ItemPlacement == null) {
                Debug.LogError($"Error: ItemPlacement doesn't exist under the character {gameObject.name}.");
            }
            var characterItems = m_ItemPlacement.GetComponentsInChildren<CharacterItem>();
            for (int i = 0; i < characterItems.Length; i++) {
                characterItems[i].Initialize(false);
                OnCharacterItemSpawned(characterItems[i]);
            }
        }

        /// <summary>
        /// Pick up each ItemIdentifier within the DefaultLoadout.
        /// </summary>
        public virtual void LoadDefaultLoadout()
        {
            LoadDefaultLoadoutInternal();
            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryLoadDefaultLoadoutComplete");
        }

        /// <summary>
        /// Pick up each ItemIdentifier within the DefaultLoadout.
        /// </summary>
        public abstract void LoadDefaultLoadoutInternal();


        /// <summary>
        /// Determines if the character has the specified item.
        /// </summary>
        /// <param name="characterItem">The item to check against.</param>
        /// <returns>True if the character has the item.</returns>
        public bool HasCharacterItem(CharacterItem characterItem) { return HasCharacterItemInternal(characterItem); }

        /// <summary>
        /// Internal method which determines if the character has the specified item.
        /// </summary>
        /// <param name="characterItem">The item to check against.</param>
        /// <returns>True if the character has the item.</returns>
        protected abstract bool HasCharacterItemInternal(CharacterItem characterItem);

        /// <summary>
        /// Returns the active item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The active item which occupies the specified slot. Can be null.</returns>
        public CharacterItem GetActiveCharacterItem(int slotID) { return GetActiveCharacterItemInternal(slotID); }

        /// <summary>
        /// Internal method which returns the active item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <returns>The active item which occupies the specified slot. Can be null.</returns>
        protected abstract CharacterItem GetActiveCharacterItemInternal(int slotID);

        /// <summary>
        /// Try to get a character item that matches the item identifier and slot ID.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier of the character item.</param>
        /// <param name="slotID">The slot ID of the character item.</param>
        /// <param name="characterItem">The character item which matches the item identifier and slot ID.</param>
        /// <returns>Return true only if a character item was found with the matching item identifier and slot ID.</returns>
        public bool TryGetCharacterItem(IItemIdentifier itemIdentifier, int slotID, out CharacterItem characterItem)
        {
            return TryGetCharacterItemInternal(itemIdentifier, slotID, out characterItem);
        }

        /// <summary>
        /// Try to get a character item that matches the item identifier and slot ID.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier of the character item.</param>
        /// <param name="slotID">The slot ID of the character item.</param>
        /// <param name="characterItem">The character item which matches the item identifier and slot ID.</param>
        /// <returns>Return true only if a character item was found with the matching item identifier and slot ID.</returns>
        protected abstract bool TryGetCharacterItemInternal(IItemIdentifier itemIdentifier, int slotID, out CharacterItem characterItem);

        /// <summary>
        /// Returns the item that corresponds to the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier of the item.</param>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        public CharacterItem GetCharacterItem(IItemIdentifier itemIdentifier, int slotID)
        {
            TryGetCharacterItemInternal(itemIdentifier, slotID, out var characterItem);
            return characterItem;
        }

        /// <summary>
        /// Returns any item that corresponds to the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier of the item.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        public CharacterItem GetCharacterItem(IItemIdentifier itemIdentifier)
        {
            for (int i = 0; i < m_SlotCount; i++) {
                if (TryGetCharacterItem(itemIdentifier, i, out var characterItem)) {
                    return characterItem;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all the character items that are linked to the specified itemdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <param name="characterItemListResult">A list of character Item List where the character items will be added to, used to prevent creating garbage.</param>
        /// <returns>The item which occupies the specified slot. Can be null.</returns>
        public ListSlice<CharacterItem> GetCharacterItems(IItemIdentifier itemIdentifier, List<CharacterItem> characterItemListResult)
        {
            if (characterItemListResult == null) {
                characterItemListResult = new List<CharacterItem>();
            }

            var startIndex = characterItemListResult.Count;
            var count = 0;
            for (int i = 0; i < m_SlotCount; i++) {
                if (TryGetCharacterItem(itemIdentifier, i, out var characterItem)) {
                    characterItemListResult.Add(characterItem);
                    count++;
                }
            }

            return (characterItemListResult, startIndex, startIndex + count);
        }

        /// <summary>
        /// Returns a list of all of the items in the inventory.
        /// </summary>
        /// <returns>A list of all of the items in the inventory.</returns>
        public ListSlice<CharacterItem> GetAllCharacterItems() { return m_AllCharacterItems; }

        /// <summary>
        /// Returns a list of all of the items in the inventory.
        /// </summary>
        /// <returns>A list of all of the items in the inventory.</returns>
        public ListSlice<CharacterItem> GetValidCharacterItems() { return m_ValidCharacterItems; }

        /// <summary>
        /// Returns a list of all of the items in the inventory.
        /// </summary>
        /// <returns>A list of all of the items in the inventory.</returns>
        public List<CharacterItem>[] GetAllCharacterItemsBySlot() { return m_CharacterItemsBySlot; }

        /// <summary>
        /// Returns a list of all of the ItemIdentifier in the inventory. Only used by the editor for the inventory inspector.
        /// </summary>
        /// <returns>A list of all of the ItemIdentifier in the inventory.</returns>
        public virtual List<IItemIdentifier> GetAllItemIdentifiers() { return m_AllItemIdentifiers; }

        /// <summary>
        /// Equips the ItemIdentifier in the specified slot.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to equip.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <param name="immediateEquip">Is the item being equipped immediately? Immediate equips will occur from the default loadout or quickly switching to the item.</param>
        public void EquipItem(IItemIdentifier itemIdentifier, int slotID, bool immediateEquip)
        {
            if (itemIdentifier == null) {
                return;
            }

            var currentItem = GetActiveCharacterItem(slotID);
            if (currentItem != null && currentItem.ItemIdentifier != itemIdentifier) {
                UnequipItem(slotID);
            }

            var item = EquipItemInternal(itemIdentifier, slotID);
            if (item != null) {
                item.Equip(immediateEquip);

                // Notify those interested that an item has been equipped.
                EventHandler.ExecuteEvent(m_GameObject, "OnInventoryEquipItem", item, slotID);
                if (m_OnEquipItemEvent != null) {
                    m_OnEquipItemEvent.Invoke(item, slotID);
                }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                    m_NetworkCharacter.EquipUnequipItem(itemIdentifier.ID, slotID, true);
                }
#endif

                if (!string.IsNullOrEmpty(m_UnequippedStateName)) {
                    StateManager.SetState(m_GameObject, m_UnequippedStateName, false);
                }
            }
        }

        /// <summary>
        /// Internal method which equips the ItemIdentifier in the specified slot.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to equip.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item which corresponds to the ItemIdentifier. Can be null.</returns>
        protected abstract CharacterItem EquipItemInternal(IItemIdentifier itemIdentifier, int slotID);

        /// <summary>
        /// Unequips the specified ItemIdentifier in the specified slot.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to unequip. If the ItemIdentifier isn't currently equipped then no changes will be made.</param>
        /// <param name="slotID">The ID of the slot.</param>
        public void UnequipItem(IItemIdentifier itemIdentifier, int slotID)
        {
            // No need to unequip if the item is already unequipped or the ItemIdentifier don't match.
            var currentItem = GetActiveCharacterItem(slotID);
            if (currentItem == null || currentItem.ItemIdentifier != itemIdentifier) {
                return;
            }

            UnequipItem(slotID);
        }

        /// <summary>
        /// Unequips the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot.</param>
        public void UnequipItem(int slotID)
        {
            // No need to unequip if the item is already unequipped.
            var currentItem = GetActiveCharacterItem(slotID);
            if (currentItem == null) {
                return;
            }

            var item = UnequipItemInternal(slotID);
            if (item == null) { return; }

            item.Unequip();

            // Notify those interested that an item has been unequipped.
            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryUnequipItem", item, slotID);
            if (m_OnUnequipItemEvent != null) {
                m_OnUnequipItemEvent.Invoke(item, slotID);
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.EquipUnequipItem(item.ItemIdentifier.ID, slotID, false);
            }
#endif

            // Optionally enable a state when the inventory is unequipped.
            if (!string.IsNullOrEmpty(m_UnequippedStateName)) {
                var unequipped = true;
                for (int i = 0; i < m_SlotCount; ++i) {
                    if (i == slotID) {
                        continue;
                    }

                    if (GetActiveCharacterItem(i) != null) {
                        unequipped = false;
                    }
                }
                if (unequipped) {
                    StateManager.SetState(m_GameObject, m_UnequippedStateName, true);
                }
            }

            // The unequipped item might be in a list of items to remove.
            if (m_CharacterItemsWaitingToBeRemoved.TryGetValue(item, out var destroyCharacterItem)) {
                RemoveCharacterItem(item, destroyCharacterItem);
            }
        }

        /// <summary>
        /// Internal method which unequips the item in the specified slot.
        /// </summary>
        /// <param name="slotID">The ID of the slot.</param>
        /// <returns>The item that was unequipped.</returns>
        protected abstract CharacterItem UnequipItemInternal(int slotID);

        /// <summary>
        /// Returns the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to get the amount of.</param>
        /// <param name="includeExternalItems">Include items that might not be part of the inventory directly?</param>
        /// <returns>The amount of the specified ItemIdentifier.</returns>
        public int GetItemIdentifierAmount(IItemIdentifier itemIdentifier, bool includeExternalItems = false)
        {
            if (itemIdentifier == null) { return 0; }
            return GetItemIdentifierAmountInternal(itemIdentifier, includeExternalItems);
        }

        /// <summary>
        /// Internal method which returns the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to get the amount of.</param>
        /// <param name="includeExternalItems">Include items that might not be part of the inventory directly?</param>
        /// <returns>The amount of the specified ItemIdentifier.</returns>
        protected abstract int GetItemIdentifierAmountInternal(IItemIdentifier itemIdentifier, bool includeExternalItems);

        /// <summary>
        /// Adjusts the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        public int AdjustItemIdentifierAmount(IItemIdentifier itemIdentifier, int amount)
        {
            if (itemIdentifier == null || amount == 0) {
                return 0;
            }

            if (amount > 0) {
                return AddItemIdentifierAmount(itemIdentifier, amount);
            } else {
                return -RemoveItemIdentifierAmount(itemIdentifier, -amount);
            }
        }

        /// <summary>
        /// Notify with an event that the item identifier amount has changed.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier that changed amount.</param>
        /// <param name="previousAmount">The previous amount.</param>
        /// <param name="newAmount">The new amount.</param>
        protected virtual void SendItemIdentifierAdjustAmountEvent(IItemIdentifier itemIdentifier, int previousAmount, int newAmount)
        {
            // Notify those interested that an item has been adjusted.
            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryAdjustItemIdentifierAmount", itemIdentifier, previousAmount, newAmount);
            if (m_OnAdjustItemIdentifierAmountEvent != null) {
                m_OnAdjustItemIdentifierAmountEvent.Invoke(itemIdentifier, newAmount);
            }
        }

        /// <summary>
        /// Adjusts the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        public virtual int AddItemIdentifierAmount(IItemIdentifier itemIdentifier, int amount)
        {
            if (m_AutoSpawnDestroyRuntimeCharacterItems) {
                return PickupItem(itemIdentifier, -1, amount, GetCharacterItem(itemIdentifier) == null, false, true, m_AutoSpawnDestroyRuntimeCharacterItems);
            } else {
                return AddItemIdentifierAmount(itemIdentifier, amount, m_AutoSpawnDestroyRuntimeCharacterItems);
            }
        }

        /// <summary>
        /// Adjusts the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to adjust.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        /// <param name="spawnCharacterItems">Should the character item be spawned?</param>
        /// <param name="slotID">The slot id for the character item. -1 will automatically spawn all available character items.</param>
        public virtual int AddItemIdentifierAmount(IItemIdentifier itemIdentifier, int amount, bool spawnCharacterItems, int slotID = -1)
        {
            if (itemIdentifier == null || amount <= 0) {
                return 0;
            }

            var previousAmount = GetItemIdentifierAmount(itemIdentifier);
            AddItemIdentifierAmountInternal(itemIdentifier, amount);
            var newAmount = GetItemIdentifierAmount(itemIdentifier);
            if (newAmount > 0 && !HasItem(itemIdentifier)) {
                AddItemIdentifierInternal(itemIdentifier);
            }

            // If the item is added when it wasn't part of the inventory before.
            if (spawnCharacterItems && newAmount > 0) {
                SpawnItemIdentifiersCharacterItem(itemIdentifier, slotID);
            }

            SendItemIdentifierAdjustAmountEvent(itemIdentifier, previousAmount, newAmount);

            return newAmount - previousAmount;
        }

        /// <summary>
        /// Add an item identifier amount.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier to add.</param>
        /// <param name="amount">The amount to add.</param>
        protected abstract void AddItemIdentifierAmountInternal(IItemIdentifier itemIdentifier, int amount);

        /// <summary>
        /// Adds the specified amount of the ItemIdentifier to the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to add.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="amount">The amount of ItemIdentifier to add.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <returns>The amount of item that was actually picked up.</returns>
        public int PickupItem(IItemIdentifier itemIdentifier, int slotID, int amount, bool immediatePickup, bool forceEquip)
        {
            return PickupItem(itemIdentifier, slotID, amount, immediatePickup, forceEquip, true, m_AutoSpawnDestroyRuntimeCharacterItems);
        }

        /// <summary>
        /// Adds the specified amount of the ItemIdentifier to the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to add.</param>
        /// <param name="slotID">The slot ID that picked up the item. A -1 value will indicate no specified slot.</param>
        /// <param name="amount">The amount of ItemIdentifier to add.</param>
        /// <param name="immediatePickup">Should the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        /// <param name="notifyOnPickup">Should other objects be notified that the ItemIdentifier was picked up?</param>
        /// <param name="spawnCharacterItems">Should the character item be spawned?</param>
        /// <returns>The amount of item that was actually picked up.</returns>
        public int PickupItem(IItemIdentifier itemIdentifier, int slotID, int amount, bool immediatePickup, bool forceEquip, bool notifyOnPickup, bool spawnCharacterItems)
        {
            if (itemIdentifier == null || !enabled || amount == 0) {
                return 0;
            }

            // Prevent pickup when the inventory isn't enabled.
            if (m_PreventPickupIfDisabled && !enabled) {
                return 0;
            }

            var addedAmount = AddItemIdentifierAmount(itemIdentifier, amount, spawnCharacterItems, slotID);
            var pickedUp = addedAmount != 0;

            // Notify those interested that an item has been picked up.
            if (pickedUp && notifyOnPickup) {
                if (slotID == -1) {
                    // Find the slot that the item belongs to (if any).
                    for (int i = 0; i < m_SlotCount; ++i) {
                        if (GetCharacterItem(itemIdentifier, i) != null) {
                            OnItemIdentifierPickedUp(itemIdentifier, i, amount, immediatePickup, forceEquip);
                            slotID = i;
                        }
                    }
                    if (slotID == -1) {
                        // The ItemIdentifier doesn't correspond to an item so execute the event once.
                        OnItemIdentifierPickedUp(itemIdentifier, -1, amount, immediatePickup, forceEquip);
                    }
                } else {
                    OnItemIdentifierPickedUp(itemIdentifier, slotID, amount, immediatePickup, forceEquip);
                }

                // If the slot ID isn't -1 then AddItem has already run. Add the item if it hasn't already been added. This will occur if the item is removed
                // and then later added again.
                if (slotID != -1) {
                    var item = GetCharacterItem(itemIdentifier, slotID);
                    if (item != null) {
                        if (!m_AllCharacterItems.Contains(item)) {
                            m_AllCharacterItems.Add(item);
                            m_CharacterItemsBySlot[item.SlotID].Add(item);
                        }

                        if (!m_ValidCharacterItems.Contains(item)) {
                            m_ValidCharacterItems.Add(item);
                        }
                    }
                }
            }

            return addedAmount;
        }

        /// <summary>
        /// The ItemIdentifier has been picked up. Notify interested objects.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier that was picked up.</param>
        /// <param name="slotID">The ID of the slot which the item belongs to.</param>
        /// <param name="amount">The number of ItemIdentifier picked up.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        protected virtual void OnItemIdentifierPickedUp(IItemIdentifier itemIdentifier, int slotID, int amount, bool immediatePickup, bool forceEquip)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.ItemIdentifierPickup(itemIdentifier.ID, slotID, amount, immediatePickup, forceEquip);
            }
#endif

            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryPickupItemIdentifier", itemIdentifier, amount, immediatePickup, forceEquip);
            if (m_OnPickupItemIdentifierEvent != null) {
                m_OnPickupItemIdentifierEvent.Invoke(itemIdentifier, amount, immediatePickup, forceEquip);
            }
            
            // Pickup the item.
            if (slotID != -1) {
                var item = GetCharacterItem(itemIdentifier, slotID);
                if (item != null) {
                    item.Pickup();

                    EventHandler.ExecuteEvent(m_GameObject, "OnInventoryPickupItem", item, amount, immediatePickup, forceEquip);
                    if (m_OnPickupItemEvent != null) {
                        m_OnPickupItemEvent.Invoke(item, amount, immediatePickup, forceEquip);
                    }
                }
            }
            
            // Add the item if it does not exist.
            if (!HasItem(itemIdentifier)) {
                AddItemIdentifierInternal(itemIdentifier);
            }
            
        }

        /// <summary>
        /// Spawn the Character item(s) for the item identifier.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier for which the matching character items should be spawned.</param>
        /// <param name="addToSlotID">The slots for which the character items should be spawned. -1 will automatically spawn all available character items.</param>
        /// <returns></returns>
        public virtual bool SpawnItemIdentifiersCharacterItem(IItemIdentifier itemIdentifier, int addToSlotID = -1)
        {
            // Prevent pickup when the inventory isn't enabled.
            if (itemIdentifier == null || !enabled) {
                return false;
            }

            var characterItemPrefabs = GetItemIdentifierCharacterItemPrefabs(itemIdentifier);
            if (characterItemPrefabs.Count == 0) {
                return true;
            }

            var atLeastOneItemAdded = false;
            for (int i = 0; i < characterItemPrefabs.Count; i++) {
                if (characterItemPrefabs[i] == null) {
                    continue;
                }

                var characterItemPrefab = characterItemPrefabs[i].GetCachedComponent<CharacterItem>();
                if (characterItemPrefab == null) {
                    Debug.LogError($"The Character Item prefab at index {i} is null for {itemIdentifier}.");
                    continue;
                }

                // If slot ID is -1 add the item for all the possible slots.
                var characterItemSlotID = characterItemPrefab.SlotID;
                if (addToSlotID != -1 && addToSlotID != characterItemSlotID) {
                    continue;
                }

                // The Character Item already exists no need to add it.
                if (TryGetCharacterItem(itemIdentifier, characterItemSlotID, out var existingCharacterItem)) {
                    continue;
                }

                // A unused Character Item might be available.
                var foundAvailableCharacterItem = false;
                var characterItems = m_CharacterItemsBySlot[characterItemSlotID];
                for (int j = 0; j < characterItems.Count; j++) {
                    var existingAvailableCharacterItem = characterItems[j];
                    if (m_ValidCharacterItems.Contains(existingAvailableCharacterItem)) { continue; }

                    if (ObjectPoolBase.GetOriginalObject(existingAvailableCharacterItem.gameObject) == characterItemPrefab.gameObject) {
                        // Found an invalid character item, it can be used instead of spawning a new CharacterItem.
                        foundAvailableCharacterItem = true;
                        existingAvailableCharacterItem.Initialize(itemIdentifier);
                        OnCharacterItemSpawned(existingAvailableCharacterItem);
                    }
                }

                if (!foundAvailableCharacterItem) {
                    SpawnCharacterItem(characterItemPrefab, itemIdentifier);
                }

                atLeastOneItemAdded = true;
            }

            return atLeastOneItemAdded;
        }

        /// <summary>
        /// When a character item is spawned send events to notify objects outside the inventory.
        /// </summary>
        /// <param name="characterItem">The character Item that was added.</param>
        public virtual void OnCharacterItemSpawned(CharacterItem characterItem)
        {
            if (!m_ValidCharacterItems.Contains(characterItem)) {
                m_ValidCharacterItems.Add(characterItem);
            }

            if (m_AllCharacterItems.Contains(characterItem)) {
                return;
            }

            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryWillAddItem", characterItem);

            m_AllCharacterItems.Add(characterItem);
            m_CharacterItemsBySlot[characterItem.SlotID].Add(characterItem);

            // Notify those interested that an item has been added.
            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryAddItem", characterItem);
            if (m_OnAddItemEvent != null) {
                m_OnAddItemEvent.Invoke(characterItem);
            }

            // The ItemIdentifier event should also be called in cases where the amount is greater than 0.
            // This allows the ItemIdentifier to be picked up before the item has been added.
            if (GetItemIdentifierAmount(characterItem.ItemIdentifier) > 0) {
                OnItemIdentifierPickedUp(characterItem.ItemIdentifier, characterItem.SlotID, 1, false, false);
            }
        }

        /// <summary>
        /// The character item started to initialize.
        /// </summary>
        /// <param name="characterItem">The character item that is starting to initialize.</param>
        public virtual void OnCharacterItemStartInitializing(CharacterItem characterItem)
        {
            // Do nothing, to be overriden. 
        }

        /// <summary>
        /// The character item finished initialization.
        /// </summary>
        /// <param name="characterItem">The character item that finished initializing.</param>
        public virtual void OnCharacterItemStopInitializing(CharacterItem characterItem)
        {
            // Do nothing, to be overriden. 
        }

        /// <summary>
        /// Get the Character Item Prefabs for the item identifier.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier from which the character item prefabs should be retrieved.</param>
        /// <returns>A list slice of character item prefabs as GameObjects.</returns>
        public abstract ListSlice<GameObject> GetItemIdentifierCharacterItemPrefabs(IItemIdentifier itemIdentifier);

        /// <summary>
        /// Spawns the item under the specified character.
        /// </summary>
        /// <param name="characterItemPrefab">The item that should be spawned.</param>
        /// <param name="itemIdentifier">The item identifier matching the character item prefab.</param>
        /// <returns>The spawned item GameObject.</returns>
        public virtual CharacterItem SpawnCharacterItem(CharacterItem characterItemPrefab, IItemIdentifier itemIdentifier)
        {
            var character = m_GameObject;
            // Spawn the item under the character's ItemPlacement GameObject.
            if (m_ItemPlacement == null) {
                Debug.LogError($"Error: ItemPlacement doesn't exist under the character {character.name}.");
                return null;
            }

            var additionalPoolKey = character.GetInstanceID();
            var previousActiveState = characterItemPrefab.gameObject.activeSelf;
            characterItemPrefab.gameObject.SetActive(false);
            var itemGameObject = ObjectPoolBase.Instantiate(characterItemPrefab.gameObject, additionalPoolKey, Vector3.zero, Quaternion.identity, m_ItemPlacement.transform);
            itemGameObject.name = characterItemPrefab.name;
            itemGameObject.transform.localPosition = Vector3.zero;
            itemGameObject.transform.localRotation = Quaternion.identity;

            var instancedCharacterItem = itemGameObject.GetComponent<CharacterItem>();
            instancedCharacterItem.Initialize(itemIdentifier);

            characterItemPrefab.gameObject.SetActive(previousActiveState);
            itemGameObject.SetActive(true);

            OnCharacterItemSpawned(instancedCharacterItem);

            return instancedCharacterItem;
        }

        /// <summary>
        /// Remove the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="amount">The amount of ItemIdentifier to remove.</param>
        /// <returns>Returns the actually amount of items removed.</returns>
        public int RemoveItemIdentifierAmount(IItemIdentifier itemIdentifier, int amount)
        {
            return RemoveItemIdentifierAmount(itemIdentifier, -1, amount, false).amountRemoved;
        }

        /// <summary>
        /// Removes the ItemIdentifier from the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="slotID">The ID of the slot.</param>
        /// <param name="amount">The amount of the ItemIdentifier that should be removed.</param>
        /// <param name="drop">Should the item be dropped when removed?</param>
        /// <returns>The instance of the dropped item (can be null).</returns>
        public GameObject RemoveItemIdentifier(IItemIdentifier itemIdentifier, int slotID, int amount, bool drop)
        {
            return RemoveItemIdentifierAmount(itemIdentifier, slotID, amount, drop).dropInstance;
        }

        /// <summary>
        /// Remove an item amount from the inventory..
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="slotID">The slot id in which to remove the item from.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        /// <param name="drop">Should the item be dropped?</param>
        public (int amountRemoved, GameObject dropInstance) RemoveItemIdentifierAmount(IItemIdentifier itemIdentifier, int slotID, int amount, bool drop)
        {
            return RemoveItemIdentifierAmount(itemIdentifier, slotID, amount, drop, m_AutoRemoveCharacterItems, m_AutoSpawnDestroyRuntimeCharacterItems);
        }
        
        /// <summary>
        /// Remove an item amount from the inventory..
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="slotID">The slot id in which to remove the item from.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        /// <param name="drop">Should the item be dropped?</param>
        /// <param name="removeCharacterItem">Should the character item be removed?</param>
        /// <returns>Returns a tuple of the actual amount removed and the dropped item instance.</returns>
        public (int amountRemoved, GameObject dropInstance) RemoveItemIdentifierAmount(IItemIdentifier itemIdentifier, int slotID, int amount, bool drop, bool removeCharacterItem)
        {
            return RemoveItemIdentifierAmountInternal(itemIdentifier, slotID, amount, drop, removeCharacterItem, m_AutoSpawnDestroyRuntimeCharacterItems);
        }

        /// <summary>
        /// Remove an item amount from the inventory..
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="slotID">The slot id in which to remove the item from.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        /// <param name="drop">Should the item be dropped?</param>
        /// <param name="removeCharacterItem">Should the character item be removed?</param>
        /// <param name="destroyCharacterItem">Should the character item be destroyed?</param>
        /// <returns>Returns a tuple of the actual amount removed and the dropped item instance.</returns>
        public (int amountRemoved, GameObject dropInstance) RemoveItemIdentifierAmount(IItemIdentifier itemIdentifier, int slotID, int amount, bool drop, bool removeCharacterItem,
                                                                                            bool destroyCharacterItem)
        {
            return RemoveItemIdentifierAmountInternal(itemIdentifier, slotID, amount, drop, removeCharacterItem, destroyCharacterItem);
        }

        /// <summary>
        /// Remove an item amount from the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="slotID">The slot id in which to remove the item from.</param>
        /// <param name="amount">The amount of ItemIdentifier to adjust.</param>
        /// <param name="drop">Should the item be dropped?</param>
        /// <param name="removeCharacterItem">Should the character item be removed?</param>
        /// <param name="destroyCharacterItem">Should the character item be destroyed?</param>
        /// <returns>Returns a tuple of the actual amount removed and the dropped item instance.</returns>
        protected virtual (int amountRemoved, GameObject dropInstance) RemoveItemIdentifierAmountInternal(IItemIdentifier itemIdentifier, int slotID, int amount, bool drop, bool removeCharacterItem, bool destroyCharacterItem)
        {
            GameObject dropInstance = null;
            if (itemIdentifier == null || amount <= 0) {
                return (0, dropInstance);
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.HasAuthority()) {
                m_NetworkCharacter.RemoveItemIdentifierAmount(itemIdentifier.ID, slotID, amount, drop, removeCharacterItem, destroyCharacterItem);
            }
#endif

            if (drop) {
                dropInstance = TryDropItemInstance(itemIdentifier, slotID, amount);
            }

            if (!CanRemoveItemIdentifier(itemIdentifier)) {
                return (0, dropInstance);
            }

            var previousAmount = GetItemIdentifierAmount(itemIdentifier);
            RemoveItemIdentifierAmountInternal(itemIdentifier, amount);
            var newAmount = GetItemIdentifierAmount(itemIdentifier);
            if (newAmount == 0 && HasItem(itemIdentifier)) {
                RemoveItemIdentifierInternal(itemIdentifier);
            }

            // If the item is removed completely auto remove the character item.
            // If a slot is specified it should be removed. If not remove all slots if the new amount is 0.
            if (removeCharacterItem && HasCharacterItem(itemIdentifier)) {
                // Remove all character items if the slot ID is -1 and new amount is 0.
                if (slotID == -1 && newAmount == 0) {
                    for (int i = 0; i < SlotCount; i++) {
                        if (TryGetCharacterItem(itemIdentifier, i, out var characterItem)) {
                            RemoveCharacterItem(characterItem, destroyCharacterItem);
                        }
                    }
                } else if (slotID != -1) {
                    if (TryGetCharacterItem(itemIdentifier, slotID, out var characterItem)) {
                        RemoveCharacterItem(characterItem, destroyCharacterItem);
                    }
                }
            }

            SendItemIdentifierAdjustAmountEvent(itemIdentifier, previousAmount, newAmount);

            return (previousAmount - newAmount, dropInstance);
        }

        /// <summary>
        /// Try to drop an item.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier to drop.</param>
        /// <param name="slotID">The slot id of the item to drop.</param>
        /// <param name="amount">The amount to drop.</param>
        /// <returns>The dropped instance.</returns>
        protected virtual GameObject TryDropItemInstance(IItemIdentifier itemIdentifier, int slotID, int amount)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && !m_NetworkInfo.HasAuthority()) {
                return null;
            }
#endif
            GameObject dropInstance = null;
            if (TryGetCharacterItem(itemIdentifier, slotID, out var characterItem)) {
                dropInstance = DropCharacterItem(characterItem, amount, false, false);
            } else if (m_DropPrefab != null) {
                var dropAmount = new ItemIdentifierAmount(itemIdentifier, amount);
                dropInstance = DropItemIdentifiers(m_DropPrefab, transform.position + transform.TransformDirection(m_DropOffset), transform.rotation, dropAmount, false, false);
            }

            return dropInstance;
        }

        /// <summary>
        /// Remove the amount of the specified ItemIdentifier.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier to remove.</param>
        /// <param name="amount">The amount of ItemIdentifier to remove.</param>
        protected abstract void RemoveItemIdentifierAmountInternal(IItemIdentifier itemIdentifier, int amount);

        /// <summary>
        /// Remove the Character Item.
        /// </summary>
        /// <param name="characterItem">The character item to remove.</param>
        /// <param name="destroyCharacterItem">Should the character item be destroyed.</param>
        /// <returns>True if the character item was removed or destroyed correctly.</returns>
        public virtual bool RemoveCharacterItem(CharacterItem characterItem, bool destroyCharacterItem)
        {
            if (characterItem == null) {
                return false;
            }

            // Remove the item from the valid character items, not from the all character item list or character items by slot list.
            // This list contains the character items that are spawned which are updated only when the item is destroyed and not when removed.
            m_ValidCharacterItems.Remove(characterItem);

            // Notify those interested that the item will be removed.
            // If the item isn't dropped then it is removed immediately.
            characterItem.Remove();

            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryRemoveItem", characterItem, characterItem.SlotID);
            if (m_OnRemoveItemEvent != null) {
                m_OnRemoveItemEvent.Invoke(characterItem, characterItem.SlotID);
            }

            // The character item might be equipped. It must be unequipped before destroyed.
            if (characterItem.IsActive()) {
                // The Character Item cannot be removed if it isn't unequipped.
                // Unequip will happen normally as the ItemSets update with the OnInventoryRemoveItem event.
                m_CharacterItemsWaitingToBeRemoved[characterItem] = destroyCharacterItem;
                return false;
            }

            // Remove it from the waiting list.
            m_CharacterItemsWaitingToBeRemoved.Remove(characterItem);

            // If the item isn't dropped then it is removed immediately.
            // Destroy the character item if it was added automatically.
            if (destroyCharacterItem) {
                return DestroyCharacterItem(characterItem);
            }

            return true;
        }

        /// <summary>
        /// Can the item be removed?
        /// </summary>
        /// <param name="itemIdentifier">The item to remove.</param>
        /// <returns>True if it can be removed.</returns>
        protected abstract bool CanRemoveItemIdentifier(IItemIdentifier itemIdentifier);

        /// <summary>
        /// Destroy the character item matching the item identifier and slot id. (Pre-Runtime character items cannot be destroyed.)
        /// </summary>
        /// <param name="itemIdentifier">The item identifier for which the matching character item should be removed.</param>
        /// <param name="removeFromSlotID">The slot ID from which the item should be removed. -1 for destroying all of them.</param>
        /// <returns>True if the character item was destroyed successfully.</returns>
        public virtual bool DestroyItemIdentifiersCharacterItem(IItemIdentifier itemIdentifier, int removeFromSlotID = -1)
        {
            // Prevent remove when the inventory isn't enabled.
            if (itemIdentifier == null || !enabled) {
                return false;
            }

            var characterItemPrefabs = GetItemIdentifierCharacterItemPrefabs(itemIdentifier);
            if (characterItemPrefabs.Count == 0) {
                return true;
            }

            var atLeastOneItemRemoved = false;
            for (int i = 0; i < characterItemPrefabs.Count; i++) {
                var characterItemPrefab = characterItemPrefabs[i].GetCachedComponent<CharacterItem>();
                if (characterItemPrefab == null) {
                    Debug.LogError($"The Character Item prefab at index {i} is null for {itemIdentifier}.");
                    continue;
                }

                // If slot ID is -1 add the item for all the possible slots.
                var characterItemSlotID = characterItemPrefab.SlotID;
                if (removeFromSlotID != -1 && removeFromSlotID != characterItemSlotID) {
                    continue;
                }

                // The Character Item does not exist on the character no need to remove it.
                if (TryGetCharacterItem(itemIdentifier, characterItemSlotID, out var existingCharacterItem) == false) {
                    continue;
                }

                DestroyCharacterItem(existingCharacterItem);
                atLeastOneItemRemoved = true;
            }

            return atLeastOneItemRemoved;
        }

        /// <summary>
        /// Is the Character Item a runtime added object or a pre-runtime object. Pre-runtime items cannot be destroyed.
        /// </summary>
        /// <param name="characterItem">A reference to the item.</param>
        /// <returns></returns>
        protected virtual bool IsRuntimeCharacterItem(CharacterItem characterItem)
        {
            if (characterItem == null) { return false; }
            return ObjectPoolBase.IsPooledObject(characterItem.gameObject);
        }

        /// <summary>
        /// Destroy the character item. Pre-Runtime character items can only be destroyed if forced.
        /// </summary>
        /// <param name="characterItem">The character item to destroy.</param>
        /// <param name="forceDestroy">Force destroy even if the character item is a pre-runtime character item.</param>
        /// <returns>Returns true if the character item was destroyed successfully.</returns>
        public virtual bool DestroyCharacterItem(CharacterItem characterItem, bool forceDestroy = false)
        {
            if (characterItem == null) {
                return false;
            }

            // Make sure to remove the character item before destroying it.
            if (m_ValidCharacterItems.Contains(characterItem)) {
                return RemoveCharacterItem(characterItem, true);
            }

            // Runtime Character items can be returned to the pool.
            if (forceDestroy == false && IsRuntimeCharacterItem(characterItem) == false) {
                // Only runtime added character items can be destroyed.
                return true;
            }

            var isItemPooled = ObjectPoolBase.IsPooledObject(characterItem.gameObject);
            if (isItemPooled == false && forceDestroy == false) {
                Debug.LogError($"The Character Item {characterItem} cannot be removed because it was not added at runtime.", characterItem);
                return false;
            }

            OnCharacterItemWillBeDestroyed(characterItem);
            characterItem.ResetInitialization();

            if (isItemPooled == false) {
                // The item is not pooled but was forced to be destroyed.
                Destroy(characterItem.gameObject);
                return true;
            }

            ObjectPoolBase.Destroy(characterItem.gameObject);

            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryDestroyItem", characterItem);

            return true;
        }

        /// <summary>
        /// A Character item will be destroyed, remove it from the dictionaries and lists.
        /// </summary>
        /// <param name="characterItem">The character item that will be destroyed.</param>
        protected virtual void OnCharacterItemWillBeDestroyed(CharacterItem characterItem)
        {
            // Remove the character item before it is destroyed.
            if (!m_AllCharacterItems.Contains(characterItem)) { return; }

            m_AllCharacterItems.Remove(characterItem);
            m_CharacterItemsBySlot[characterItem.SlotID].Remove(characterItem);
        }

        /// <summary>
        /// Check if an item identifier has a Character item.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier.</param>
        /// <returns>True if the item identifier has a character item.</returns>
        public bool HasCharacterItem(IItemIdentifier itemIdentifier)
        {
            for (int i = 0; i < SlotCount; i++) {
                var match = HasCharacterItem(itemIdentifier, i);
                if (match == true) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the Inventory contains the item identifier.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier to check for.</param>
        /// <returns>Return true if the inventory contains the item identifier.</returns>
        public virtual bool HasItem(IItemIdentifier itemIdentifier)
        {
            return m_AllItemIdentifiers.Contains(itemIdentifier);
        }

        /// <summary>
        /// Remove the Item Identifier from the list of ItemIdentifiers.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier to check for.</param>
        protected virtual void AddItemIdentifierInternal(IItemIdentifier itemIdentifier)
        {
            if (!HasItem(itemIdentifier)) {
                m_AllItemIdentifiers.Add(itemIdentifier);
            }
        }

        /// <summary>
        /// Remove the Item Identifier from the list of ItemIdentifiers.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier to check for.</param>
        protected virtual void RemoveItemIdentifierInternal(IItemIdentifier itemIdentifier)
        {
            m_AllItemIdentifiers.Remove(itemIdentifier);
        }

        /// <summary>
        /// Does the inventory have a character item matching the item identifier and slot ID.
        /// </summary>
        /// <param name="itemIdentifier">The item identifier matching the character item.</param>
        /// <param name="slotID">The slot ID to check for.</param>
        /// <returns>True if the inventory has the matching character item.</returns>
        public bool HasCharacterItem(IItemIdentifier itemIdentifier, int slotID)
        {
            return TryGetCharacterItem(itemIdentifier, slotID, out var characterItem);
        }

        /// <summary>
        /// Drop the Item identifiers from the character.
        /// </summary>
        /// <param name="dropPrefab">The drop prefab to use, if null the default drop prefab will be used.</param>
        /// <param name="dropPosition">The drop position in world space.</param>
        /// <param name="dropRotation">The drop rotation in world space.</param>
        /// <param name="dropItemAmounts">The list of item amounts to drop.</param>
        /// <param name="forceDrop">Should the item be dropped even if the inventory doesn't contain any count for the item?</param>
        /// <param name="remove">Should the items be removed after it is dropped?</param>
        /// <returns>The instance of the dropped item.</returns>
        public virtual GameObject DropItemIdentifiers(GameObject dropPrefab, Vector3 dropPosition, Quaternion dropRotation, ListSlice<ItemIdentifierAmount> dropItemAmounts, bool forceDrop, bool remove)
        {
            if (dropPrefab == null) {
                dropPrefab = m_DropPrefab;
            }

            GameObject spawnedObject = null;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo == null || m_NetworkInfo.HasAuthority()) {
#endif
            spawnedObject = ObjectPoolBase.Instantiate(dropPrefab, dropPosition, dropRotation);
                // The ItemPickup component is responsible for allowing characters to pick up the item. Save the ItemIdentifier count
                // to the ItemIdentifierAmount array so that same amount can be picked up again.
                var itemPickup = spawnedObject.GetCachedComponent<ItemPickupBase>();
                if (itemPickup != null) {
                    // Return the old.
                    var itemDefinitionAmounts = itemPickup.GetItemDefinitionAmounts();
                    if (itemDefinitionAmounts.Length != dropItemAmounts.Count) {
                        itemDefinitionAmounts = new ItemIdentifierAmount[dropItemAmounts.Count];
                    }

                    for (int i = 0; i < dropItemAmounts.Count; i++) {
                        var dropItemAmount = dropItemAmounts[i];
                        var dropItemIdentifier = dropItemAmount.ItemIdentifier;

                        // With force drop, drop the amount even if the inventory doesn't have it.
                        if (forceDrop) {
                            itemDefinitionAmounts[i] = dropItemAmount;
                        } else {
                            var dropAmount = Mathf.Min(dropItemAmount.Amount, GetItemIdentifierAmount(dropItemIdentifier));
                            itemDefinitionAmounts[i] = new ItemIdentifierAmount(dropItemIdentifier, dropAmount);
                        }
                    }

                    // Enable the ItemPickup.
                    itemPickup.SetItemDefinitionAmounts(itemDefinitionAmounts);
                    itemPickup.Initialize(true);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                    if (m_NetworkInfo != null) {
                        Networking.Game.NetworkObjectPool.NetworkSpawn(dropPrefab, spawnedObject, true);

                        // The server will manage the object.
                        if (!m_NetworkInfo.IsServer()) {
                            ObjectPoolBase.Destroy(spawnedObject);
                            spawnedObject = null;
                        }
                    }
#endif
                }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            }
#endif

            if (remove) {
                for (int i = 0; i < dropItemAmounts.Count; i++) {
                    RemoveItemIdentifierAmount(dropItemAmounts[i].ItemIdentifier, dropItemAmounts[i].Amount);
                }
            }

            return spawnedObject;
        }

        /// <summary>
        /// Drop an Item from the character.
        /// </summary>
        /// <param name="characterItem">The item to drop.</param>
        /// <param name="forceDrop">Should the item be dropped even if the inventory doesn't contain any count for the item?</param>
        /// <param name="amount">The amount of ItemIdentifier that should be dropped.</param>
        /// <param name="remove">Should the item be removed after it is dropped?</param>
        /// <returns>The instance of the dropped item (can be null).</returns>
        public virtual GameObject DropCharacterItem(CharacterItem characterItem, int amount, bool forceDrop, bool remove)
        {
            // The item needs to first be unequipped before it can be dropped.
            if (characterItem.VisibleObjectActive && characterItem.CharacterLocomotion.FirstPersonPerspective && remove) {
                characterItem.UnequipDropAmount = amount;
                var itemObject = characterItem.GetVisibleObject().transform;
                characterItem.UnequpDropPosition = itemObject.position;
                characterItem.UnequipDropRotation = itemObject.rotation;
                return null;
            }

            GameObject spawnedObject = null;
            ItemPickupBase itemPickup = null;
            // If a drop prefab exists then the character should drop a prefab of the item so it can later be picked up.
            if (characterItem.DropPrefab != null) {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                if (m_NetworkInfo == null || m_NetworkInfo.HasAuthority()) {
#endif
                    var existingAmount = GetItemIdentifierAmount(characterItem.ItemIdentifier);

                    // The prefab can be dropped if the inventory contains the item or is force dropped.
                    if (existingAmount > 0 || forceDrop) {
                        Vector3 dropPosition;
                        Quaternion dropRotation;
                        // If the item is unequipped before it is dropped then it could be holstered so the current transform should not be used.
                        if (characterItem.UnequipDropAmount > 0) {
                            dropPosition = characterItem.UnequpDropPosition;
                            dropRotation = characterItem.UnequipDropRotation;
                        } else {
                            var itemObject = characterItem.GetVisibleObject().transform;
                            dropPosition = itemObject.position;
                            dropRotation = itemObject.rotation;
                        }
                        spawnedObject = ObjectPoolBase.Instantiate(characterItem.DropPrefab, dropPosition, dropRotation);

                        // The ItemPickup component is responsible for allowing characters to pick up the item. Save the ItemIdentifier count
                        // to the ItemIdentifierAmount array so that same amount can be picked up again.
                        itemPickup = spawnedObject.GetCachedComponent<ItemPickupBase>();
                        if (itemPickup != null) {
                            // Return the old.
                            var itemDefinitionAmounts = itemPickup.GetItemDefinitionAmounts();
                            var itemDefinitionAmount = new ItemIdentifierAmount(characterItem.ItemIdentifier.GetItemDefinition(), Mathf.Min(existingAmount, amount));

                            // The character item can have other items which can be dropped simultaneously like ammo item. 
                            var otherItemsToDrop = characterItem.GetAdditionalItemsToDrop();

                            // If the dropped Item is a usable item then the array should be larger to be able to pick up the usable ItemIdentifier.
                            // Save the main ItemIdentifier.
                            var length = otherItemsToDrop.Count + 1;
                            if (itemDefinitionAmounts.Length != length) {
                                itemDefinitionAmounts = new ItemIdentifierAmount[length];
                            }
                            itemDefinitionAmounts[0] = itemDefinitionAmount;

                            for (int i = 0; i < otherItemsToDrop.Count; i++) {
                                var otherItemToDrop = otherItemsToDrop[i];
                                itemDefinitionAmounts[i + 1] = otherItemToDrop;
                            }

                            // Enable the ItemPickup.
                            itemPickup.SetItemDefinitionAmounts(itemDefinitionAmounts);
                            itemPickup.Initialize(true);
                        }

                        // The ItemPickup may have a TrajectoryObject attached instead of a Rigidbody.
                        var trajectoryObject = spawnedObject.GetCachedComponent<Objects.TrajectoryObject>();
                        if (trajectoryObject != null) {
                            var velocity = characterItem.CharacterLocomotion.Velocity;
#if ULTIMATE_CHARACTER_CONTROLLER_VERSION_2_VR
                            if (characterItem.HandHandler != null) {
                                velocity += characterItem.HandHandler.GetVelocity(characterItem.SlotID) * characterItem.DropVelocityMultiplier;
                            }
#endif
                            trajectoryObject.Initialize(velocity, characterItem.CharacterLocomotion.Torque.eulerAngles, characterItem.Character);
                        }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                        if (m_NetworkInfo != null) {
                            Networking.Game.NetworkObjectPool.NetworkSpawn(characterItem.DropPrefab, spawnedObject, true);

                            // The server will manage the object.
                            if (!m_NetworkInfo.IsServer()) {
                                ObjectPoolBase.Destroy(spawnedObject);
                                spawnedObject = null;
                            }
                        }
#endif
                    }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                }
#endif
            }
            if (remove) {
                characterItem.UnequipDropAmount = 0;
                RemoveItemIdentifier(characterItem.ItemIdentifier, characterItem.SlotID, GetItemIdentifierAmount(characterItem.ItemIdentifier), false);
            }

            if (characterItem.DropItemEvent != null) {
                characterItem.DropItemEvent.Invoke();
            }
            
            EventHandler.ExecuteEvent<CharacterItem, int, GameObject>(m_GameObject, "OnInventoryDropItem", characterItem, amount, spawnedObject);
            
            return spawnedObject;
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        protected virtual void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            enabled = false;
            
            if (m_UnequipAllOnDeath) {
                for (int i = 0; i < m_SlotCount; ++i) {
                    UnequipItem(i);
                }
            }
        }

        /// <summary>
        /// Removes all of the items from the inventory.
        /// </summary>
        /// <param name="drop">Should the item be dropped when removed?</param>
        public virtual void RemoveAllItems(bool drop)
        {
            // All items should be unequiped before they are removed.
            for (int i = 0; i < m_SlotCount; ++i) {
                UnequipItem(i);
            }

            var allItems = GetAllCharacterItems();
            for (int i = allItems.Count - 1; i >= 0; --i) {
                // Multiple items may be dropped at the same time.
                if (allItems.Count <= i) {
                    continue;
                }

                var characterItem = allItems[i];
                var itemIdentifier = characterItem.ItemIdentifier;
                var slotID = characterItem.SlotID;
                while (GetItemIdentifierAmount(itemIdentifier) > 0 && CanRemoveItemIdentifier(itemIdentifier)) {
                    RemoveItemIdentifier(itemIdentifier, slotID, 1, drop);
                }
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        protected virtual void OnRespawn()
        {
            enabled = true;
            if (m_LoadDefaultLoadoutOnRespawn) {
                LoadDefaultLoadout();
            }

            // Notify others that the inventory has respawned - allows EquipUnequip to equip any previously equipped items.
            EventHandler.ExecuteEvent(m_GameObject, "OnInventoryRespawned");
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_GameObject, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_GameObject, "OnRespawn", OnRespawn);
        }
    }
}
