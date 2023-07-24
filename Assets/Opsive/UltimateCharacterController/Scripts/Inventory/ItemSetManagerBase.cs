/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

//#define DEBUG_ITEM

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The ItemSetManager manages the ItemSets belonging to the character.
    /// </summary>
    public abstract class ItemSetManagerBase : MonoBehaviour
    {
        /// <summary>
        /// Sepcifies when the item sets should be updated after an item is added.
        /// </summary>
        public enum OnAddItemUpdateItemSetsOptions
        {
            Immediately,            // The ItemSet should be updated immediately.
            ScheduleToLateUpdate,   // The ItemSets should be updated within LateUpdate.
            Manual                  // The ItemSets should be updated manually with ItemSetManagerBase.UpdateItemSets.
        }

        [Tooltip("Sepcifies the order that the items can be equipped.")]
        [SerializeField] protected ItemSetGroup[] m_ItemSetGroups;
        [Tooltip("Update ItemSets options when an Item is added.")]
        [SerializeField] protected OnAddItemUpdateItemSetsOptions m_OnAddItemUpdateItemSetsOption;

        public ItemSetGroup[] ItemSetGroups { get { return m_ItemSetGroups; } set { m_ItemSetGroups = value; } }

        [System.NonSerialized] protected bool m_Initialized;
        protected GameObject m_GameObject;
        protected InventoryBase m_Inventory;
        protected bool m_ItemSetDirty;

        protected Dictionary<uint, int> m_CategoryIndexMap;
        protected Dictionary<IItemSetRule, Stack<ItemSet>> m_ItemSetPool;
        protected List<CharacterItem>[] m_CachedCharacterItemsBySlot;

        public virtual int CategoryCount => m_ItemSetGroups.Length;
        public virtual int SlotCount => m_Inventory.SlotCount;
        public InventoryBase CharacterInventory => m_Inventory;
        public bool Initialized => m_Initialized;

        /// <summary>
        /// Initializes the ItemCollection and ItemSet.
        /// </summary>
        private void Awake()
        {
            if (!Game.CharacterInitializer.AutoInitialization) {
                Game.CharacterInitializer.Instance.OnAwake += AwakeInternal;
                return;
            }

            AwakeInternal();
        }

        /// <summary>
        /// Internal method which initializes the ItemCollection and ItemSet.
        /// </summary>
        protected virtual void AwakeInternal()
        {
            if (Game.CharacterInitializer.Instance != null) {
                Game.CharacterInitializer.Instance.OnAwake -= AwakeInternal;
            }

            Initialize(false);
        }

        /// <summary>
        /// Initializes the ItemSetManager.
        /// </summary>
        /// <param name="force">Should the ItemSet be force initialized?</param>
        public virtual void Initialize(bool force)
        {
            if (m_Initialized && !force) {
                return;
            }
            m_Initialized = true;

            m_GameObject = gameObject;
            m_Inventory = m_GameObject.GetCachedComponent<InventoryBase>();
            if (m_Inventory == null) {
                Debug.LogError("An Inventory component is required for an ItemSetManager.");
            }

            m_ItemSetPool = new Dictionary<IItemSetRule, Stack<ItemSet>>();
            m_CachedCharacterItemsBySlot = new List<CharacterItem>[SlotCount];

            if (m_CategoryIndexMap == null) {
                m_CategoryIndexMap = new Dictionary<uint, int>();
            } else {
                m_CategoryIndexMap.Clear();
            }

            for (int i = 0; i < m_ItemSetGroups.Length; ++i) {
                var itemSetGroup = m_ItemSetGroups[i];
                itemSetGroup.Initialize(gameObject, this);

                // Create a mapping between the category and index.
                m_CategoryIndexMap.Add(itemSetGroup.CategoryID, i);
            }

            EventHandler.RegisterEvent<CharacterItem>(m_GameObject, "OnInventoryAddItem", OnAddItem);
            EventHandler.RegisterEvent<CharacterItem>(m_GameObject, "OnInventoryDestroyItem", OnDestroyItem);
            EventHandler.RegisterEvent(m_GameObject, "OnInventoryLoadDefaultLoadoutComplete", OnInventoryLoadDefaultLoadoutComplete);
        }

        /// <summary>
        /// Update the item sets in late update if an update was scheduled. avoiding updating it multiple times in a single frame.
        /// </summary>
        private void LateUpdate()
        {
            if (!m_ItemSetDirty) { return; }

            UpdateItemSets();
        }

        /// <summary>
        /// Schedule the Item Sets to be updated at the end of the frame.
        /// </summary>
        public void ScheduleItemSetUpdate()
        {
            m_ItemSetDirty = true;
        }

        /// <summary>
        /// Get the default ItemCategory to use when an ItemSetGroup does not have a category assigned.
        /// </summary>
        /// <returns>The default ItemCategory.</returns>
        public abstract CategoryBase GetDefaultCategory();

        /// <summary>
        /// Try to equip the item set with the matching item set state name.
        /// </summary>
        /// <param name="itemSetName">The item set state name.</param>
        /// <param name="groupIndex">The group in which to look for the item set. -1 to search in all groups.</param>
        /// <param name="forceEquipUnequip">Force equip the item?</param>
        /// <param name="immediateEquipUnequip">Immediately equip the item?</param>
        /// <returns>Return true if an item set exist with that name.</returns>
        public bool TryEquipItemSet(string itemSetName, int groupIndex, bool forceEquipUnequip, bool immediateEquipUnequip)
        {
            var itemSet = GetItemSet(itemSetName, groupIndex);
            if (itemSet == null) {
                return false;
            }

            return TryEquipItemSet(itemSet, forceEquipUnequip, immediateEquipUnequip);
        }

        /// <summary>
        /// Try to equip the item set with the matching item set state name.
        /// </summary>
        /// <param name="itemSetIndex">The item set state name.</param>
        /// <param name="groupIndex">The group in which to look for the item set. -1 to search in all groups.</param>
        /// <param name="forceEquipUnequip">Force equip the item?</param>
        /// <param name="immediateEquipUnequip">Immediately equip the item?</param>
        /// <returns>Return true if an item set exist with that name.</returns>
        public bool TryEquipItemSet(int itemSetIndex, int groupIndex, bool forceEquipUnequip, bool immediateEquipUnequip)
        {
            var itemSet = GetItemSet(itemSetIndex, groupIndex);
            if (itemSet == null) {
                return false;
            }

            return TryEquipItemSet(itemSet, forceEquipUnequip, immediateEquipUnequip);
        }

        /// <summary>
        /// Try to equip the item set.
        /// </summary>
        /// <param name="itemSet">The item set to equip.</param>
        /// <param name="forceEquipUnequip">Force equip the item?</param>
        /// <param name="immediateEquipUnequip">Immediately equip the item?</param>
        /// <returns>Return true if an item set exist and is valid.</returns>
        public bool TryEquipItemSet(ItemSet itemSet, bool forceEquipUnequip, bool immediateEquipUnequip)
        {
            if (itemSet == null) {
                return false;
            }

            if (itemSet.Active) {
                return true;
            }

            if (itemSet.Enabled == false || itemSet.IsValid == false) {
                return false;
            }

            itemSet.EquipUnequip.StartEquipUnequip(itemSet.Index, forceEquipUnequip, immediateEquipUnequip);
            return true;
        }

        /// <summary>
        /// Equip the item set which contains the item.
        /// </summary>
        /// <param name="item">The item to equip.</param>
        /// <param name="equip">Equip or unequip.</param>
        /// <param name="groupIndex">The group in which to look for the item set. -1 to search in all groups.</param>
        /// <param name="forceEquipUnequip">Force equip the item?</param>
        /// <param name="immediateEquipUnequip">Immediately equip the item?</param>
        public void EquipUnequipItem(IItemIdentifier item, bool equip, int groupIndex, bool forceEquipUnequip, bool immediateEquipUnequip)
        {
            var itemSet = GetItemSet(item, groupIndex, true);
            if (itemSet == null) { return; }

            //Don't equip or unequip if it is already equipped or unequipped.
            if (itemSet.Active == equip) { return; }

            if (itemSet.EquipUnequip == null) {
                Debug.LogError($"The Equip Unequip Ability for the ItemSet '{itemSet.State}' of group '{itemSet.ItemSetGroup.CategoryName}' is null");
                return;
            }

            if (equip) {
                itemSet.EquipUnequip.StartEquipUnequip(itemSet.Index, forceEquipUnequip, immediateEquipUnequip);
            } else {
                itemSet.EquipUnequip.StartEquipUnequip(-1, forceEquipUnequip, immediateEquipUnequip);
            }
        }

        /// <summary>
        /// Equip the item set which contains the item.
        /// </summary>
        /// <param name="item">The item to equip.</param>
        /// <param name="groupIndex">The group in which to look for the item set. -1 to search in all groups.</param>
        /// <param name="forceEquipUnequip">Force equip the item?</param>
        /// <param name="immediateEquipUnequip">Immediately equip the item?</param>
        public void EquipItem(IItemIdentifier item, int groupIndex, bool forceEquipUnequip, bool immediateEquipUnequip)
        {
            EquipUnequipItem(item, true, groupIndex, forceEquipUnequip, immediateEquipUnequip);
        }

        /// <summary>
        /// Unequip the item set which contains the item.
        /// </summary>
        /// <param name="item">The item to unequip.</param>
        /// <param name="groupIndex">The group in which to look for the item set. -1 to search in all groups.</param>
        /// <param name="forceEquipUnequip">Force unequip the item?</param>
        /// <param name="immediateEquipUnequip">Immediately unequip the item?</param>
        public void UnEquipItem(IItemIdentifier item, int groupIndex, bool forceEquipUnequip, bool immediateEquipUnequip)
        {
            EquipUnequipItem(item, false, groupIndex, forceEquipUnequip, immediateEquipUnequip);
        }

        /// <summary>
        /// Returns the corresponding category index which maps to the category.
        /// </summary>
        /// <param name="category">The interested category.</param>
        /// <returns>The corresponding category index which maps to the category.</returns>
        public int CategoryToIndex(IItemCategoryIdentifier category)
        {
            if (category == null) {
                return -1;
            }

            if (m_CategoryIndexMap.TryGetValue(category.ID, out var index)) {
                return index;
            }

            return -1;
        }

        /// <summary>
        /// Returns the corresponding category index which maps to the ID.
        /// </summary>
        /// <param name="categoryID">The ID of the category to get.</param>
        /// <returns>The corresponding category index which maps to the ID.</returns>
        public int CategoryIDToIndex(uint categoryID)
        {
            for (int i = 0; i < m_ItemSetGroups.Length; ++i) {
                if (m_ItemSetGroups[i].CategoryID == categoryID) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns true if the ItemDefinition belongs to the category with the specified index.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition to determine if it belongs to the category.</param>
        /// <param name="groupIndex">The index of the category which the ItemIdentifier may belong to.</param>
        /// <returns>True if the ItemDefinition belongs to the category with the specified index.</returns>
        public bool IsCategoryMember(ItemDefinitionBase itemDefinition, int groupIndex)
        {
            // If an ItemDefinition doesn't have a category it is a member of every category.
            if (itemDefinition.GetItemCategory() == null) {
                return true;
            }

            return IsCategoryMember(itemDefinition.GetItemCategory(), groupIndex);
        }

        /// <summary>
        /// Returns true if the CategoryIdentifier belongs to the category with the specified index.
        /// </summary>
        /// <param name="itemCategory">The CategoryIdentifier to determine if it belongs to the category.</param>
        /// <param name="groupIndex">The index of the category which the ItemIdentifier may belong to.</param>
        /// <returns>True if the ItemIdentifier belongs to the category with the specified index.</returns>
        private bool IsCategoryMember(IItemCategoryIdentifier itemCategory, int groupIndex)
        {
            if (groupIndex >= m_ItemSetGroups.Length || groupIndex < 0) {
                return false;
            }

            var category = m_ItemSetGroups[groupIndex].ItemCategory;
            return category.InherentlyContains(itemCategory);
        }

        /// <summary>
        /// Get the index of the ItemSetGroup.
        /// </summary>
        /// <param name="itemSetGroup">The </param>
        /// <returns></returns>
        public int IndexOf(ItemSetGroup itemSetGroup)
        {
            return m_ItemSetGroups.IndexOf(itemSetGroup);
        }

        /// <summary>
        /// Returns true if the ItemDefinitionrepresents the default ItemCategory.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition to determine if it is the default ItemCategory.</param>
        /// <returns>True if the ItemDefinition represents the default ItemCategory.</returns>
        public bool IsDefaultItemCategory(ItemDefinitionBase itemDefinition)
        {
            if (itemDefinition == null) {
                return false;
            }
            var category = itemDefinition.GetItemCategory();
            if (category == null) {
                return false;
            }

            return IsDefaultItemCategory(itemDefinition, category);
        }

        /// <summary>
        /// Returns true if the ItemCategory represents the default ItemCategory.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition to determine if it is the default ItemCategory.</param>
        /// <param name="itemCategory">The ItemCategory to determine if it is the default ItemCategory.</param>
        /// <returns>True if the ItemCategory represents the default ItemCategory.</returns>
        private bool IsDefaultItemCategory(ItemDefinitionBase itemDefinition, IItemCategoryIdentifier itemCategory)
        {
            for (int i = 0; i < m_ItemSetGroups.Length; i++) {
                var itemSetGroup = m_ItemSetGroups[i];
                // The default category does not match the active category. Return false.
                if (itemSetGroup.IsDefaultItemSetActive() == false) {
                    continue;
                }

                if (itemSetGroup.ItemCategory.InherentlyContains(itemCategory) == false) {
                    continue;
                }

                var defaultItemSet = itemSetGroup.GetActiveItemSet();
                if (defaultItemSet != null) {
                    for (int j = 0; j < defaultItemSet.ItemIdentifiers.Length; j++) {
                        if (itemDefinition.InherentlyContains(defaultItemSet.ItemIdentifiers[j])) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// The inventory has added the specified item.
        /// </summary>
        /// <param name="characterItem">The item that was added.</param>
        protected virtual void OnAddItem(CharacterItem characterItem)
        {
            OnEventToUpdateItemSets();
        }

        /// <summary>
        /// The inventory has destroyed the specified item.
        /// </summary>
        /// <param name="characterItem">The item that was destroyed.</param>
        protected virtual void OnDestroyItem(CharacterItem characterItem)
        {
            OnEventToUpdateItemSets();
        }

        /// <summary>
        /// Update or schedule the update for the item sets.
        /// </summary>
        protected virtual void OnEventToUpdateItemSets()
        {
            if (m_OnAddItemUpdateItemSetsOption == OnAddItemUpdateItemSetsOptions.Immediately) {
                UpdateItemSets();
            } else if (m_OnAddItemUpdateItemSetsOption == OnAddItemUpdateItemSetsOptions.ScheduleToLateUpdate) {
                ScheduleItemSetUpdate();
            }
        }

        /// <summary>
        /// Refresh the item sets on loadout complete.
        /// </summary>
        protected virtual void OnInventoryLoadDefaultLoadoutComplete()
        {
            UpdateItemSets();
        }

        /// <summary>
        /// Returns the ItemSet that the item belongs to.
        /// </summary>
        /// <param name="characterItem">The item to get the ItemSet of.</param>
        /// <param name="groupIndex">The index of the ItemSet group.</param>
        /// <param name="checkIfValid">Should the ItemSet be checked to see if it is valid?.</param>
        /// <returns>The ItemSet that the item belongs to.</returns>
        public int GetItemSetIndex(CharacterItem characterItem, int groupIndex, bool checkIfValid)
        {
            if (groupIndex == -1) {
                return -1;
            }

            var itemSetGroup = m_ItemSetGroups[groupIndex];
            return itemSetGroup.GetItemSetIndex(characterItem.ItemIdentifier, characterItem.SlotID, checkIfValid);
        }

        /// <summary>
        /// Returns the default ItemSet index for the specified group index.
        /// </summary>
        /// <param name="groupIndex">The index of the cateogry to get the default ItemSet index of.</param>
        /// <returns>The default ItemSet index for the specified group index.</returns>
        public int GetDefaultItemSetIndex(int groupIndex)
        {
            if (groupIndex == -1) {
                return -1;
            }
            return m_ItemSetGroups[groupIndex].DefaultItemSetIndex;
        }

        /// <summary>
        /// Returns the target ItemSet index for the specified group index based on the allowed slots bitwise mask.
        /// </summary>
        /// <param name="groupIndex">The index of the group to get the target ItemSet index of.</param>
        /// <param name="allowedMask">The bitwise mask indicating which slots are allowed.</param>
        /// <returns>The target ItemSet index for the specified group index.</returns>
        public int GetTargetItemSetIndex(int groupIndex, int allowedMask)
        {
            if (groupIndex == -1) {
                return -1;
            }

            var itemSetGroup = m_ItemSetGroups[groupIndex];
            var itemSetIndex = itemSetGroup.ActiveItemSetIndex;

            if (IsItemSetValid(groupIndex, itemSetIndex, false, allowedMask)) {
                return itemSetIndex;
            }

            // Check the special cases before looping through the entire item set list.
            // Determine if the previous item set is similar to the current item set.
            var itemSetListCount = itemSetGroup.ItemSetList.Count;
            var prevItemSetIndex = itemSetIndex - 1;
            if (prevItemSetIndex < 0) {
                prevItemSetIndex = itemSetListCount - 1;
            }
            if (itemSetIndex != -1 && prevItemSetIndex != itemSetIndex && IsItemSetValid(groupIndex, prevItemSetIndex, false, allowedMask)) {
                for (int i = 0; i < itemSetGroup.ItemSetList[prevItemSetIndex].ItemIdentifiers.Length; ++i) {
                    var prevSlots = itemSetGroup.ItemSetList[prevItemSetIndex].ItemIdentifiers;
                    if (itemSetGroup.ItemSetList[itemSetIndex].ItemIdentifiers[i] == prevSlots[i] && prevSlots[i] != null) {
                        // At least one definition matches. Switch to that ItemSet.
                        return prevItemSetIndex;
                    }
                }
            }
            // Check the default item set.
            if (IsItemSetValid(groupIndex, itemSetGroup.DefaultItemSetIndex, false, allowedMask)) {
                return itemSetGroup.DefaultItemSetIndex;
            }

            // Keep checking the ItemSets until a valid item set exists.
            var iterCount = 0;
            do {
                if (iterCount == itemSetListCount) {
                    // No valid ItemSet was found.
                    return -1;
                }
                iterCount++;
                itemSetIndex = (itemSetIndex + 1) % itemSetListCount;
            } while (itemSetIndex == prevItemSetIndex || itemSetIndex == itemSetGroup.DefaultItemSetIndex || !IsItemSetValid(groupIndex, itemSetIndex, false, allowedMask));

            return itemSetIndex;
        }

        /// <summary>
        /// Returns true if the specified ItemSet is valid. A valid ItemSet means the character has all of the items specified in the inventory.
        /// </summary>
        /// <param name="groupIndex">The index of the ItemSet group.</param>
        /// <param name="itemSetIndex">The ItemSet within the group.</param>
        /// <param name="checkIfCanSwitchTo">Should the ItemSet be checked if it can be switched to?</param>
        /// <param name="allowedSlotsMask">The bitwise mask indicating which slots are allowed.</param>
        /// <returns>True if the specified ItemSet is valid.</returns>
        public bool IsItemSetValid(int groupIndex, int itemSetIndex, bool checkIfCanSwitchTo, int allowedSlotsMask = -1)
        {
            return m_ItemSetGroups[groupIndex].IsItemSetValid(itemSetIndex, checkIfCanSwitchTo, allowedSlotsMask);
        }

        /// <summary>
        /// Returns true if the specified ItemSet is valid. A valid ItemSet means the character has all of the items specified in the inventory.
        /// </summary>
        /// <param name="itemSet">The ItemSet within the group.</param>
        /// <param name="checkIfCanSwitchTo">Should the ItemSet be checked if it can be switched to?</param>
        /// <param name="allowedSlotsMask">The bitwise mask indicating which slots are allowed.</param>
        /// <returns>True if the specified ItemSet is valid.</returns>
        public bool IsItemSetValid(ItemSet itemSet, bool checkIfCanSwitchTo, int allowedSlotsMask = -1)
        {
            if (itemSet == null || itemSet.ItemSetGroup == null) {
                return false;
            }
            return itemSet.ItemSetGroup.IsItemSetValid(itemSet, checkIfCanSwitchTo, allowedSlotsMask);
        }

        /// <summary>
        /// Returns the index of the ItemSet that is next or previous in the list.
        /// </summary>
        /// <param name="groupIndex">The group of ItemSets to get.</param>
        /// <param name="currentItemSetIndex">The current ItemSet index.</param>
        /// <param name="next">Should the next ItemSet be retrieved? If false the previous ItemSet will be retrieved.</param>
        /// <returns>The index of the ItemSet that is next or previous in the list.</returns>
        public int NextActiveItemSetIndex(int groupIndex, int currentItemSetIndex, bool next)
        {
            if (currentItemSetIndex == -1) {
                return -1;
            }
            var itemSetListCount = m_ItemSetGroups[groupIndex].ItemSetList.Count;
            // The ItemSet can't be switched if there are zero or only one ItemSets.
            if (itemSetListCount <= 1) {
                return -1;
            }
            var itemSetIndex = currentItemSetIndex;
            do {
                itemSetIndex = (itemSetIndex + (next ? 1 : -1)) % itemSetListCount;
                if (itemSetIndex < 0) {
                    itemSetIndex = itemSetListCount - 1;
                }
            } while (itemSetIndex != currentItemSetIndex && !IsItemSetValid(groupIndex, itemSetIndex, true));

            return itemSetIndex;
        }

        /// <summary>
        /// Updates the next ItemSet to the specified value.
        /// </summary>
        /// <param name="groupIndex">The group to update the ItemSet within.</param>
        /// <param name="itemSetIndex">The ItemSet to set.</param>
        public void UpdateNextItemSet(int groupIndex, int itemSetIndex)
        {
            var itemSetGroup = m_ItemSetGroups[groupIndex];
            itemSetGroup.UpdateNextItemSet(itemSetIndex);
        }

        /// <summary>
        /// Updates the active ItemSet to the specified value.
        /// </summary>
        /// <param name="groupIndex">The group to update the ItemSet within.</param>
        /// <param name="itemSetIndex">The ItemSet to set.</param>
        public void UpdateActiveItemSet(int groupIndex, int itemSetIndex)
        {
            var itemSetGroup = m_ItemSetGroups[groupIndex];
            var activeItemSetIndex = itemSetGroup.ActiveItemSetIndex;

#if DEBUG_ITEM
            Debug.Log($"Update Active Item Set: group index {groupIndex}, active item set {activeItemSetIndex}, new item set index {itemSetIndex}");
#endif

            itemSetGroup.UpdateActiveItemSet(itemSetIndex);
        }

        /// <summary>
        /// Sets the default ItemSet for the specified group.
        /// </summary>
        /// <param name="groupIndex">The group to set the default itemset of.</param>
        public void SetDefaultItemSet(int groupIndex)
        {
            var itemSetIndex = GetDefaultItemSetIndex(groupIndex);
            if (IsItemSetValid(groupIndex, itemSetIndex, false)) {
                UpdateActiveItemSet(groupIndex, itemSetIndex);
            }
        }

        /// <summary>
        /// Returns the ItemIdentifier which should be equipped for the specified slot.
        /// </summary>
        /// <param name="slot">The slot to get the ItemIdentifier of.</param>
        /// <returns>The ItemIdentifier which should be equipped for the specified slot. Can be null.</returns>
        public IItemIdentifier GetEquipItemIdentifier(int slot)
        {
            if (slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }

            for (int i = 0; i < m_ItemSetGroups.Length; ++i) {
                var itemSetGroup = m_ItemSetGroups[i];
                if (itemSetGroup.ActiveItemSetIndex != -1) {
                    var itemSet = itemSetGroup.GetActiveItemSet();
                    if (itemSet.Enabled && itemSet.ItemIdentifiers[slot] != null) {
                        return itemSet.ItemIdentifiers[slot];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the ItemIdentifier which should be equipped for the specified groupIndex and slot.
        /// </summary>
        /// <param name="groupIndex">The group to get the ItemIdentifier of.</param>
        /// <param name="slot">The slot to get the ItemIdentifier of.</param>
        /// <returns>The ItemIdentifier which should be equipped for the specified slot. Can be null.</returns>
        public IItemIdentifier GetEquipItemIdentifier(int groupIndex, int slot)
        {
            if (groupIndex == -1 || groupIndex >= m_ItemSetGroups.Length || slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }
            var itemSetGroup = m_ItemSetGroups[groupIndex];
            if (itemSetGroup.ActiveItemSetIndex != -1) {
                var itemSet = itemSetGroup.GetActiveItemSet();
                return itemSet.ItemIdentifiers[slot];
            }
            return null;
        }

        /// <summary>
        /// Returns the ItemIdentifier which should be equipped for the specified groupIndex, ItemSet, and slot.
        /// </summary>
        /// <param name="groupIndex">The group to get the ItemIdentifier of.</param>
        /// <param name="targetItemSetIndex">The ItemSet to get the ItemIdentifier of.</param>
        /// <param name="slot">The slot to get the ItemIdentifier of.</param>
        /// <returns>The ItemIdentifier which should be equipped for the specified groupIndex, ItemIdentifier, and slot. Can be null.</returns>
        public IItemIdentifier GetEquipItemIdentifier(int groupIndex, int targetItemSetIndex, int slot)
        {
            if (groupIndex == -1 || groupIndex >= m_ItemSetGroups.Length ||
                targetItemSetIndex == -1 || targetItemSetIndex >= m_ItemSetGroups[groupIndex].ItemSetList.Count ||
                slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }

            return m_ItemSetGroups[groupIndex].ItemSetList[targetItemSetIndex].ItemIdentifiers[slot];
        }

        /// <summary>
        /// Returns the ItemIdentifier which is going to be equipped next.
        /// </summary>
        /// <param name="slot">The slot to get the ItemIdentifier of.</param>
        /// <param name="groupIndex">The group index of the found ItemIdentifier.</param>
        /// <returns>The ItemIdentifier which is going to be equipped next. Can be null.</returns>
        public IItemIdentifier GetNextItemIdentifier(int slot, out int groupIndex)
        {
            groupIndex = -1;
            if (slot == -1 || slot >= m_Inventory.SlotCount) {
                return null;
            }

            for (int i = 0; i < m_ItemSetGroups.Length; ++i) {
                var itemSetGroup = m_ItemSetGroups[i];
                var index = itemSetGroup.NextItemSetIndex != -1 ? itemSetGroup.NextItemSetIndex : itemSetGroup.ActiveItemSetIndex;
                if (index == -1) {
                    continue;
                }
                groupIndex = i;
                return itemSetGroup.ItemSetList[index].ItemIdentifiers[slot];
            }
            return null;
        }

        /// <summary>
        /// Get the item set group in which the item set is contained.
        /// </summary>
        /// <param name="itemSet">The item set to get the group of.</param>
        /// <returns>The item set group.</returns>
        public ItemSetGroup GetItemSetGroup(ItemSet itemSet)
        {
            if (itemSet == null) { return null; }

            for (int i = 0; i < m_ItemSetGroups.Length; i++) {
                var itemSetGroup = m_ItemSetGroups[i];
                if (itemSetGroup.ItemSetList.Contains(itemSet)) {
                    return itemSetGroup;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the Item Set Rule which created the item set.
        /// </summary>
        /// <param name="itemSet">The item set which was created by the item set rule.</param>
        /// <returns>The item set rule which created the item set.</returns>
        public IItemSetRule GetItemSetRule(ItemSet itemSet)
        {
            var itemSetGroup = GetItemSetGroup(itemSet);
            if (itemSetGroup == null) { return null; }

            return itemSetGroup.GetItemSetRule(itemSet);
        }

        /// <summary>
        /// Update the Item Sets using the rules and the state of the Inventory character items.
        /// </summary>
        public virtual void UpdateItemSets()
        {
            if (!m_Initialized) {
                return;
            }
            UpdateItemSets(m_Inventory.GetAllCharacterItems());
        }

        /// <summary>
        /// Update the item sets using the item set rules.
        /// </summary>
        /// <param name="characterItems">The items used to create the new item sets.</param>
        public virtual void UpdateItemSets(ListSlice<CharacterItem> characterItems)
        {
            for (int i = 0; i < m_CachedCharacterItemsBySlot.Length; i++) {
                if (m_CachedCharacterItemsBySlot[i] == null) {
                    m_CachedCharacterItemsBySlot[i] = new List<CharacterItem>();
                } else {
                    m_CachedCharacterItemsBySlot[i].Clear();
                }
            }

            for (int i = 0; i < characterItems.Count; i++) {
                var characterItem = characterItems[i];
                m_CachedCharacterItemsBySlot[characterItem.SlotID].Add(characterItem);
            }

            UpdateItemSets(m_CachedCharacterItemsBySlot);
        }

        /// <summary>
        /// Update the item sets using the item set rules.
        /// </summary>
        /// <param name="characterItemsBySlot">The items used to create the new item sets.</param>
        public virtual void UpdateItemSets(List<CharacterItem>[] characterItemsBySlot)
        {
            for (int i = 0; i < m_ItemSetGroups.Length; i++) {
                m_ItemSetGroups[i].UpdateItemSets(characterItemsBySlot);
            }

            m_ItemSetDirty = false;
        }

        /// <summary>
        /// Get the active item set.
        /// </summary>
        /// <param name="groupIndex">The group index in which to get the active item set.</param>
        /// <returns>The active item set (can be null).</returns>
        public ItemSet GetActiveItemSet(int groupIndex)
        {
            if (groupIndex < 0 || groupIndex >= m_ItemSetGroups.Length) {
                return null;
            }

            return m_ItemSetGroups[groupIndex].GetActiveItemSet();
        }

        /// <summary>
        /// Get the next item set.
        /// </summary>
        /// <param name="groupIndex">The group index in which to search for the item set.</param>
        /// <returns>The next item set.</returns>
        public ItemSet GetNextItemSet(int groupIndex)
        {
            if (groupIndex < 0 || groupIndex >= m_ItemSetGroups.Length) {
                return null;
            }

            return m_ItemSetGroups[groupIndex].GetNextItemSet();
        }

        /// <summary>
        /// Get the active item set index.
        /// </summary>
        /// <param name="groupIndex">The group index in which to get the active item set.</param>
        /// <returns>The active item set index (can be -1).</returns>
        public int GetActiveItemSetIndex(int groupIndex)
        {
            if (groupIndex < 0 || groupIndex >= m_ItemSetGroups.Length) {
                return -1;
            }

            return m_ItemSetGroups[groupIndex].ActiveItemSetIndex;
        }

        /// <summary>
        /// Get the next item set index.
        /// </summary>
        /// <param name="groupIndex">The group index in which to search for the item set.</param>
        /// <returns>The next item set index.</returns>
        public int GetNextItemSetIndex(int groupIndex)
        {
            if (groupIndex < 0 || groupIndex >= m_ItemSetGroups.Length) {
                return -1;
            }

            return m_ItemSetGroups[groupIndex].NextItemSetIndex;
        }

        /// <summary>
        /// Get an Item Set that contains the item in any slot.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <param name="groupIndex">The group in which to search the Item Set. -1 to search in all groups.</param>
        /// <param name="checkIfValid">Do not return invalid Item Sets?</param>
        /// <returns>The item set that matches the item.</returns>
        public ItemSet GetItemSet(IItemIdentifier item, int groupIndex, bool checkIfValid)
        {
            if (groupIndex >= 0) {
                var itemSetGroup = m_ItemSetGroups[groupIndex];
                return itemSetGroup.GetItemSet(item, checkIfValid);
            }

            //Check in all item set groups
            for (int i = 0; i < m_ItemSetGroups.Length; i++) {
                var itemSet = m_ItemSetGroups[i].GetItemSet(item, checkIfValid);
                if (itemSet != null) {
                    return itemSet;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the item set which contains the list of items.
        /// </summary>
        /// <param name="items">The list of items to look for.</param>
        /// <param name="groupIndex">The group index (-1 to look in all groups).</param>
        /// <returns>The item set taht matches the list of items.</returns>
        public ItemSet GetItemSet(ListSlice<IItemIdentifier> items, int groupIndex)
        {
            if (groupIndex >= 0) {
                var itemSetGroup = m_ItemSetGroups[groupIndex];
                return itemSetGroup.GetItemSet(items);
            }

            //Check in all item set groups
            for (int i = 0; i < m_ItemSetGroups.Length; i++) {
                var itemSet = m_ItemSetGroups[i].GetItemSet(items);
                if (itemSet != null) {
                    return itemSet;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the item set that matches the items set state name.
        /// </summary>
        /// <param name="itemSetName">The item set state name.</param>
        /// <param name="groupIndex">The group index (-1 to look in all groups).</param>
        /// <returns>The item set taht matches the list of items.</returns>
        public ItemSet GetItemSet(string itemSetName, int groupIndex)
        {
            if (groupIndex >= 0) {
                var itemSetGroup = m_ItemSetGroups[groupIndex];
                return itemSetGroup.GetItemSet(itemSetName);
            }

            //Check in all item set groups
            for (int i = 0; i < m_ItemSetGroups.Length; i++) {
                var itemSet = m_ItemSetGroups[i].GetItemSet(itemSetName);
                if (itemSet != null) {
                    return itemSet;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the item set that matches the items set index.
        /// </summary>
        /// <param name="itemSetIndex">The item set index.</param>
        /// <param name="groupIndex">The group index (-1 to look in all groups).</param>
        /// <returns>The item set that matches the item set index.</returns>
        public ItemSet GetItemSet(int itemSetIndex, int groupIndex)
        {
            if (groupIndex >= 0) {
                var itemSetGroup = m_ItemSetGroups[groupIndex];
                return itemSetGroup.GetItemSet(itemSetIndex);
            }

            //Check in all item set groups
            for (int i = 0; i < m_ItemSetGroups.Length; i++) {
                var itemSet = m_ItemSetGroups[i].GetItemSet(itemSetIndex);
                if (itemSet != null) {
                    return itemSet;
                }
            }

            return null;
        }

        /// <summary>
        /// Is the item contained within the active ItemSet.
        /// </summary>
        /// <param name="groupIndex">The group index (-1 searches in all groups).</param>
        /// <param name="item">The item to match (checks exact slot).</param>
        /// <returns>True if the active Item Set contains the item</returns>
        public bool IsItemContainedInActiveItemSet(int groupIndex, CharacterItem item)
        {
            // If the group index is -1, search in all groups.
            if (groupIndex == -1) {
                for (int i = 0; i < ItemSetGroups.Length; i++) {
                    var itemSet = GetActiveItemSet(i);
                    if (itemSet != null && itemSet.ItemIdentifiers[item.SlotID] == item.ItemIdentifier) {
                        return true;
                    }
                }
            } else {
                var itemSet = GetActiveItemSet(groupIndex);
                if (itemSet != null && itemSet.ItemIdentifiers[item.SlotID] == item.ItemIdentifier) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Is the item contained within the active ItemSet.
        /// </summary>
        /// <param name="groupIndex">The group index (-1 searches in all groups).</param>
        /// <param name="item">The item to match (in any slot).</param>
        /// <returns>True if the active Item Set contains the item</returns>
        public bool IsItemContainedInActiveItemSet(int groupIndex, IItemIdentifier item)
        {
            // If the group index is -1, search in all groups.
            if (groupIndex == -1) {
                for (int i = 0; i < ItemSetGroups.Length; i++) {
                    var itemSet = GetActiveItemSet(i);
                    if (itemSet != null && itemSet.ItemIdentifiers.Contains(item)) {
                        return true;
                    }
                }
            } else {
                var itemSet = GetActiveItemSet(groupIndex);
                if (itemSet != null && itemSet.ItemIdentifiers.Contains(item)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Is the item contained within the next ItemSet.
        /// </summary>
        /// <param name="groupIndex">The group index (-1 searches in all groups).</param>
        /// <param name="item">The item to match (checks exact slot).</param>
        /// <returns>True if the next Item Set contains the item</returns>
        public bool IsItemContainedInNextItemSet(int groupIndex, CharacterItem item)
        {
            // If the group index is -1, search in all groups.
            if (groupIndex == -1) {
                for (int i = 0; i < ItemSetGroups.Length; i++) {
                    var itemSet = GetNextItemSet(i);
                    if (itemSet != null && itemSet.ItemIdentifiers[item.SlotID] == item.ItemIdentifier) {
                        return true;
                    }
                }
            } else {
                var itemSet = GetNextItemSet(groupIndex);
                if (itemSet != null && itemSet.ItemIdentifiers[item.SlotID] == item.ItemIdentifier) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Is the item contained within the next ItemSet.
        /// </summary>
        /// <param name="groupIndex">The group index (-1 searches in all groups).</param>
        /// <param name="item">The item to match (in any slot).</param>
        /// <returns>True if the next Item Set contains the item</returns>
        public bool IsItemContainedInNextItemSet(int groupIndex, IItemIdentifier item)
        {
            // If the group index is -1, search in all groups.
            if (groupIndex == -1) {
                for (int i = 0; i < ItemSetGroups.Length; i++) {
                    var itemSet = GetNextItemSet(i);
                    if (itemSet != null && itemSet.ItemIdentifiers.Contains(item)) {
                        return true;
                    }
                }
            } else {
                var itemSet = GetNextItemSet(groupIndex);
                if (itemSet != null && itemSet.ItemIdentifiers.Contains(item)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get an Item Set from the pool.
        /// </summary>
        /// <returns>A clean item set.</returns>
        /// <param name="itemSetRule">The item set rule associated to the item set to create.</param>
        /// <param name="originalItemSet">The original ItemSet which is copied if not item set exist in the pool.</param>
        /// <returns></returns>
        public virtual ItemSet PopItemSetFromPool(IItemSetRule itemSetRule, ItemSet originalItemSet)
        {
            if (m_ItemSetPool.TryGetValue(itemSetRule, out var itemSetStack) == false
                || itemSetStack.TryPop(out var itemSet) == false) {
                itemSet = new ItemSet(originalItemSet);
                itemSet.Active = false;
                itemSet.Initialize(m_GameObject, this);
            }

            itemSet.OnPopFromPool();
            return itemSet;
        }

        /// <summary>
        /// Return an item set to the pool.
        /// </summary>
        /// <param name="itemSet">The item set to return.</param>
        public virtual void ReturnItemSetToPool(ItemSet itemSet)
        {
            var itemSetRule = itemSet.ItemSetRule;
            if (itemSetRule == null) { return; }

            if (m_ItemSetPool.TryGetValue(itemSetRule, out var itemSetStack) == false) {
                itemSetStack = new Stack<ItemSet>();
                m_ItemSetPool.Add(itemSetRule, itemSetStack);
            }

            itemSetStack.Push(itemSet);

            itemSet.OnReturnToPool();
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            for (int i = 0; i < m_ItemSetGroups.Length; ++i) {
                for (int j = 0; j < m_ItemSetGroups[i].ItemSetList.Count; ++j) {
                    m_ItemSetGroups[i].ItemSetList[j].OnReturnToPool();
                }
            }

            if (m_GameObject != null) {
                EventHandler.UnregisterEvent<CharacterItem>(m_GameObject, "OnInventoryAddItem", OnAddItem);
                EventHandler.UnregisterEvent<CharacterItem>(m_GameObject, "OnInventoryDestroyItem", OnDestroyItem);
                EventHandler.UnregisterEvent(m_GameObject, "OnInventoryLoadDefaultLoadoutComplete", OnInventoryLoadDefaultLoadoutComplete);
            }
        }

        /// <summary>
        /// Unequip all equipped items
        /// </summary>
        /// <param name="forceEquipUnequip">Force unequip the item?</param>
        /// <param name="immediateEquipUnequip">Immediately unequip the item?</param>
        public void UnEquipAllItems(bool forceEquipUnequip, bool immediateEquipUnequip)
        {
            for (int i = 0; i < m_ItemSetGroups.Length; i++) {

                var activeItemSet = m_ItemSetGroups[i].GetActiveItemSet();
                if (activeItemSet == null) { return; }

                if (activeItemSet.EquipUnequip == null) {
                    Debug.LogError($"The Equip Unequip Ability for the ItemSet '{activeItemSet.State}' of group '{activeItemSet.ItemSetGroup.CategoryName}' is null");
                    return;
                }
                
                activeItemSet.EquipUnequip.StartEquipUnequip(-1, forceEquipUnequip, immediateEquipUnequip);
                
            }
        }
    }
}