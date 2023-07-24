/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Contains a list of ItemSets which belong in the same grouping.
    /// </summary>
    [Serializable]
    public class ItemSetGroup
    {
        [Tooltip("The item category assigned to this item set group.")]
        [SerializeField] protected CategoryBase m_SerializedItemCategory;
        [Tooltip("The starting item set rules (item set rules can be added at runtime).")]
        [SerializeField] protected ItemSetRuleBase[] m_StartingItemSetRules;

        protected GameObject m_GameObject;
        protected ItemSetManagerBase m_ItemSetManager;
        protected int m_DefaultItemSetIndex = -1;
        protected List<ItemSet> m_ItemSetList;
        protected ItemSetRuleStreamData m_ItemSetRuleStreamData;

        protected List<IItemSetRule> m_ItemSetRules;
        protected Dictionary<ItemSet, IItemSetRule> m_RulesByItemSet;
        protected Dictionary<IItemSetRule, List<ItemSet>> m_ItemSetListByRule;
        protected int m_ActiveItemSetIndex;
        protected int m_NextItemSetIndex;

        protected List<ItemSetStateInfo> m_ItemSetStateList;
        private EquipUnequip m_EquipUnequip;
        
        private HashSet<IItemIdentifier> m_CheckedItemIdentifiers = new HashSet<IItemIdentifier>();
        private IItemCategoryIdentifier m_ItemCategoryIdentifier;

        public virtual CategoryBase SerializedItemCategory { get { return m_SerializedItemCategory; } set { m_SerializedItemCategory = value; } }
        public virtual IItemCategoryIdentifier ItemCategory { get { return m_ItemCategoryIdentifier; } set { m_ItemCategoryIdentifier = value; } }
        public ItemSetRuleBase[] StartingItemSetRules { get { return m_StartingItemSetRules; } set { m_StartingItemSetRules = value; } }

        public GameObject GameObject => m_GameObject;
        public ItemSetManagerBase ItemSetManager => m_ItemSetManager;
        public List<IItemSetRule> ItemSetRules { get { return m_ItemSetRules; } set { m_ItemSetRules = value; } }


        public int ActiveItemSetIndex { get => m_ActiveItemSetIndex; set { m_ActiveItemSetIndex = value; } }

        public int NextItemSetIndex { get => m_NextItemSetIndex; set => m_NextItemSetIndex = value; }

        public int GroupIndex => m_ItemSetManager.IndexOf(this);
        public int SlotCount => m_ItemSetManager.SlotCount;

        public uint CategoryID => ItemCategory?.ID ?? 0;

        public string CategoryName => ItemCategory.name;

        public int DefaultItemSetIndex { get { return m_DefaultItemSetIndex; } set { m_DefaultItemSetIndex = value; } }
        
        public List<ItemSet> ItemSetList { get { return m_ItemSetList; } set { m_ItemSetList = value; } }
        public EquipUnequip EquipUnequip => m_EquipUnequip;

        /// <summary>
        /// CategoryItemSet default constructor.
        /// </summary>
        public ItemSetGroup()
        {
            m_StartingItemSetRules = new ItemSetRuleBase[0];
            m_ItemSetRules = new List<IItemSetRule>();
            m_ActiveItemSetIndex = -1;
            m_NextItemSetIndex = -1;
        }
        
        /// <summary>
        /// CategoryItemSet overload constructor.
        /// </summary>
        /// <param name="itemCategory">The item category.</param>
        /// <param name="startingItemSetRules">The starting item set rule.</param>
        public ItemSetGroup(IItemCategoryIdentifier itemCategory, ItemSetRuleBase[] startingItemSetRules)
        {
            m_ItemCategoryIdentifier = itemCategory;
            m_StartingItemSetRules = startingItemSetRules;
            m_ItemSetRules = new List<IItemSetRule>();
            m_ActiveItemSetIndex = -1;
            m_NextItemSetIndex = -1;
        }

        /// <summary>
        /// Initialize the Item Set Group.
        /// </summary>
        /// <param name="gameObject">The game object linked to the group.</param>
        /// <param name="itemSetManager">The item set manager.</param>
        public void Initialize(GameObject gameObject, ItemSetManagerBase itemSetManager)
        {
            if (m_ItemCategoryIdentifier == null) {
                // Use the default ItemCategory if the serialized Item Category is null.
                if (m_SerializedItemCategory == null) {
                    m_SerializedItemCategory = itemSetManager.GetDefaultCategory();
                }
                
                m_ItemCategoryIdentifier = m_SerializedItemCategory as IItemCategoryIdentifier;

                if (m_SerializedItemCategory != null && m_ItemCategoryIdentifier == null) {
                    Debug.LogError($"The serialized ItemCategory {m_SerializedItemCategory} isn't of type {typeof(IItemCategoryIdentifier)}.", gameObject);
                }

                if (m_ItemCategoryIdentifier == null) {
                    Debug.LogError($"The Item Set Group has a null ItemCategory on {gameObject}, A category must be assigned.", gameObject);
                    
                    // Assign a "fake" category to prevent further errors.
                    m_ItemCategoryIdentifier = ScriptableObject.CreateInstance<Category>();
                }
            }

            m_GameObject = gameObject;
            m_ItemSetManager = itemSetManager;
            
            m_ItemSetRules.Clear();
            m_ItemSetRules.AddRange(m_StartingItemSetRules);
            m_ItemSetList = new List<ItemSet>();
            m_RulesByItemSet = new Dictionary<ItemSet, IItemSetRule>();
            m_ItemSetListByRule = new Dictionary<IItemSetRule, List<ItemSet>>();
            m_ItemSetStateList = new List<ItemSetStateInfo>();
            m_CheckedItemIdentifiers = new HashSet<IItemIdentifier>();
            m_ItemSetRuleStreamData = new ItemSetRuleStreamData(this);

            var ultimateCharacterLocomotion = gameObject.GetCachedComponent<UltimateCharacterLocomotion>();
            var equipUnequipAbilities = ultimateCharacterLocomotion.GetAbilities<EquipUnequip>();
            EquipUnequip firstEquipUnequipAbility = null;
            EquipUnequip genericEquipUnequipAbility = null;
            if (equipUnequipAbilities != null) {
                for (int i = 0; i < equipUnequipAbilities.Length; ++i) {
                    var equipUnequipAbility = equipUnequipAbilities[i];
                    
                    if (i == 0) {
                        firstEquipUnequipAbility = equipUnequipAbility;
                    }

                    if (equipUnequipAbility.ItemSetCategoryID <= 0) {
                        genericEquipUnequipAbility = equipUnequipAbility;
                    }
                    
                    if (equipUnequipAbility.ItemSetCategoryID == CategoryID) {
                        m_EquipUnequip = equipUnequipAbility;
                        break;
                    }
                }
            }

            if (m_EquipUnequip == null) {
                m_EquipUnequip = genericEquipUnequipAbility;
                if (m_EquipUnequip == null && Application.isPlaying) {
                    if (firstEquipUnequipAbility == null) {
                        Debug.LogError($"No Equip/Unequip ability was foundmatching the ItemSetGroup category ID '{CategoryID}'. Please add at least one Equip/Unequip ability.");
                    } else {
                        Debug.LogWarning($"No Equip/Unequip ability was found matching the ItemSetGroup category ID '{CategoryID}'. " +
                                         $"The first ability found will be assigned instead. Add a matching ability to avoid errors.");
                        m_EquipUnequip = firstEquipUnequipAbility;
                    }
                }
            }
        }

        /// <summary>
        /// Get the item set at the index provided.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item set located at the index provided.</returns>
        public ItemSet GetItemSetAt(int index)
        {
            if (index < 0 || index >= m_ItemSetList.Count) {
                return null;
            }
            return m_ItemSetList[index];
        }

        /// <summary>
        /// Get the active item set.
        /// </summary>
        /// <returns>The active item set.</returns>
        public ItemSet GetActiveItemSet()
        {
            return GetItemSetAt(m_ActiveItemSetIndex);
        }

        /// <summary>
        /// Get the next item set in the group.
        /// </summary>
        /// <returns>The next item set.</returns>
        public ItemSet GetNextItemSet()
        {
            return GetItemSetAt(m_NextItemSetIndex);
        }
        
        /// <summary>
        /// Get the default item set.
        /// </summary>
        /// <returns>The default item set.</returns>
        public ItemSet GetDefaultItemSet()
        {
            return GetItemSetAt(m_DefaultItemSetIndex);
        }

        /// <summary>
        /// Is the default item set active?
        /// </summary>
        /// <returns>True if the default item set is active.</returns>
        public bool IsDefaultItemSetActive()
        {
            return m_DefaultItemSetIndex == m_ActiveItemSetIndex;
        }
        
        /// <summary>
        /// Get the item set rule for the item set.
        /// </summary>
        /// <param name="itemSet">The item set to get the rule from.</param>
        /// <returns>The item set rule for the provided item set.</returns>
        public IItemSetRule GetItemSetRule(ItemSet itemSet)
        {
            m_RulesByItemSet.TryGetValue(itemSet, out var itemSetRule);
            return itemSetRule;
        }
        
        /// <summary>
        /// Get all the item sets which where created by the provided rule.
        /// </summary>
        /// <param name="itemSetRule">The item set rule to get the item sets for.</param>
        /// <returns>A list of item sets which where created by the provided rule.</returns>
        public ListSlice<ItemSet> GetRuleItemSetList(IItemSetRule itemSetRule)
        {
            m_ItemSetListByRule.TryGetValue(itemSetRule, out var itemSetCreator);
            return itemSetCreator;
        }

        /// <summary>
        /// Get the item set index from the item set.
        /// </summary>
        /// <param name="itemSet">The item set to get the index for.</param>
        /// <returns>The index of the item set within the group.</returns>
        public int GetItemSetIndex(ItemSet itemSet)
        {
            return m_ItemSetList.IndexOf(itemSet);
        }

        /// <summary>
        /// Insert an item set rule within the group.
        /// </summary>
        /// <param name="index">The index in which to insert the item set rule.</param>
        /// <param name="itemSetRule">The item set rule to insert.</param>
        public void InsertItemSetRule(int index, IItemSetRule itemSetRule)
        {
            m_ItemSetRules.Insert(index, itemSetRule);
            m_ItemSetManager.ScheduleItemSetUpdate();
        }

        /// <summary>
        /// Add an item set rule in the group.
        /// </summary>
        /// <param name="itemSetRule">The item set rule to add.</param>
        public void AddItemSetRule(IItemSetRule itemSetRule)
        {
            m_ItemSetRules.Add(itemSetRule);
            m_ItemSetManager.ScheduleItemSetUpdate();
        }
        
        /// <summary>
        /// Remove an item set rule from the group.
        /// </summary>
        /// <param name="itemSetRule">The item set rule to remove.</param>
        public void RemoveItemSetRule(IItemSetRule itemSetRule)
        {
            m_ItemSetRules.Remove(itemSetRule);
            m_ItemSetManager.ScheduleItemSetUpdate();
        }
        
        /// <summary>
        /// Set the item set rules, replaces the existing ones.
        /// </summary>
        /// <param name="itemSetRules">The item set rules.</param>
        public void SetItemSetRules(ListSlice<IItemSetRule> itemSetRules)
        {
            m_ItemSetRules.Clear();
            m_ItemSetRules.AddRange(itemSetRules);
            m_ItemSetManager.ScheduleItemSetUpdate();
        }

        /// <summary>
        /// Get the first item set index for a specific item.
        /// </summary>
        /// <param name="item">The item to look for.</param>
        /// <param name="slotID">The slot ID in which the item should be in (-1 if any).</param>
        /// <param name="checkIfValid">Only return the index for a valid item set?</param>
        /// <returns>The item set index which contains the item provided.</returns>
        public int GetItemSetIndex(IItemIdentifier item, int slotID, bool checkIfValid)
        {
            // The ItemSet may be in the process of being changed. Test the next item set first to determine if this item set should be returned.
            var nextItemSetIndex = m_NextItemSetIndex;
            if (nextItemSetIndex != -1) {
                var itemSet = m_ItemSetList[nextItemSetIndex];
                if (slotID == -1) {
                    for (int i = 0; i < itemSet.ItemIdentifiers.Length; i++) {
                        if (itemSet.ItemIdentifiers[i] != item) {
                            continue;
                        }
                        if (!checkIfValid || IsItemSetValid(nextItemSetIndex, false)) {
                            return nextItemSetIndex;
                        }
                    }
                } else {
                    if (itemSet.ItemIdentifiers[slotID] == item) {
                        if (!checkIfValid || IsItemSetValid(nextItemSetIndex, false)) {
                            return nextItemSetIndex;
                        }
                    }
                }
            }

            var itemCount = m_ItemSetManager.CharacterInventory.GetItemIdentifierAmount(item);
            // Search through all of the ItemSets for one that contains the specified item.
            var validItemSet = -1;
            for (int i = 0; i < m_ItemSetList.Count; ++i) {
                // The ItemSet is valid, but do not return it immediately if the ItemSet uses more than one ItemDefinitions. This will prevent a dual wield ItemSet from equipping
                // when a single item was picked up.
                var validSlotCount = 1;
                var itemSet = m_ItemSetList[i];

                var slotValid = slotID == -1;
                for (int j = 0; j < itemSet.ItemIdentifiers.Length; ++j) {
                    if (item == itemSet.ItemIdentifiers[j]) {
                        validSlotCount++;
                        if (j == slotID) {
                            slotValid = true;
                        }
                    }
                }
                
                // The slot specified must match or be -1.
                if (!slotValid) { continue; }

                if (itemCount == validSlotCount) {
                    if (!checkIfValid || IsItemSetValid(i, false)) {
                        return i;
                    }
                } else if (validItemSet == -1) {
                    validItemSet = i;
                }
            }
            
            if (!checkIfValid || IsItemSetValid(validItemSet, false)) {
                return validItemSet;
            }
            
            return -1;
        }

        /// <summary>
        /// Returns true if the specified ItemSet is valid. A valid ItemSet means the character has all of the items specified in the inventory.
        /// </summary>
        /// <param name="itemSetIndex">The ItemSet within the category.</param>
        /// <param name="checkIfCanSwitchTo">Should the ItemSet be checked if it can be switched to?</param>
        /// <param name="allowedSlotsMask">The bitwise mask indicating which slots are allowed.</param>
        /// <returns>True if the specified ItemSet is valid.</returns>
        public bool IsItemSetValid(int itemSetIndex, bool checkIfCanSwitchTo, int allowedSlotsMask = -1)
        {
            if (itemSetIndex == -1 || itemSetIndex >= ItemSetList.Count) {
                return false;
            }
            
            var itemSet = ItemSetList[itemSetIndex];
            return IsItemSetValid(itemSet, checkIfCanSwitchTo, allowedSlotsMask);
        }

        /// <summary>
        /// Returns true if the specified ItemSet is valid. A valid ItemSet means the character has all of the items specified in the inventory.
        /// </summary>
        /// <param name="itemSet">The ItemSet within the category.</param>
        /// <param name="checkIfCanSwitchTo">Should the ItemSet be checked if it can be switched to?</param>
        /// <param name="allowedSlotsMask">The bitwise mask indicating which slots are allowed.</param>
        /// <returns>True if the specified ItemSet is valid.</returns>
        public bool IsItemSetValid(ItemSet itemSet, bool checkIfCanSwitchTo, int allowedSlotsMask = -1)
        {
            if (itemSet.ItemSetGroup != this) {
                return false;
            }
            
            // The ItemSet isn't valid if it isn't enabled.
            if (!itemSet.Enabled) {
                return false;
            }

            // The ItemSet may not be able to be switched to.
            if (checkIfCanSwitchTo && !itemSet.CanSwitchTo) {
                return false;
            }
            
            // The ItemSet may not be valid according to the ItemSetRule.
            var itemSetRule = GetItemSetRule(itemSet);
            if (itemSetRule != null) {
                if (!itemSetRule.IsItemSetValid(itemSet, allowedSlotsMask)) {
                    return false;
                }
            }
            
            var requiredCount = 0;
            var availableCount = 0;
            m_CheckedItemIdentifiers.Clear();
            for (int i = 0; i < itemSet.ItemIdentifiers.Length; ++i) {
                if (itemSet.ItemIdentifiers[i] == null) {
                    continue;
                }

                // If the ItemIdentifier is null then the item hasn't been added yet.
                if (itemSet.ItemIdentifiers[i] == null) {
                    return false;
                }

                // The item may not be in the allowed layer mask.
                if (allowedSlotsMask != -1 && DefaultItemSetIndex != itemSet.Index && !MathUtility.InLayerMask(i, allowedSlotsMask)) {
                    return false;
                }

                // It only takes one item for the ItemSet not to be valid.
                var item = m_ItemSetManager.CharacterInventory.GetCharacterItem(itemSet.ItemIdentifiers[i], i);
                if (item == null) {
                    return false;
                }

                // Usable items may not be able to be equipped if they don't have any consumable ItemIdentifiers left.
                for (int j = 0; j < item.ItemActions.Length; ++j) {
                    var usableItem = item.ItemActions[j] as IUsableItem;
                    if (usableItem != null) {
                        if (!usableItem.CanEquip()) {
                            return false;
                        }
                    }
                }

                // Remember the count to ensure the correct number of items exist within the inventory.
                requiredCount++;
                if (!m_CheckedItemIdentifiers.Contains(item.ItemIdentifier)) {
                    availableCount += m_ItemSetManager.CharacterInventory.GetItemIdentifierAmount(item.ItemIdentifier);
                    m_CheckedItemIdentifiers.Add(item.ItemIdentifier);
                }
            }

            // Ensure the inventory has the number of items required for the current ItemSet.
            if (availableCount < requiredCount) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Update the item sets using the item set rules.
        /// </summary>
        /// <param name="characterItemsBySlot">The items used to create the new item sets.</param>
        public virtual void UpdateItemSets(List<CharacterItem>[] characterItemsBySlot)
        {
            var pooledItemCategorySetList = GenericObjectPool.Get<List<ItemSetRuleInfo>>();
            var pooledItemSets = GenericObjectPool.Get<List<ItemSet>>();
            pooledItemCategorySetList.Clear();
            pooledItemSets.Clear();
            
            m_ItemSetStateList.Clear();
            
            // Determine which item sets should be added, remove and kept.
            for (int j = 0; j < ItemSetRules.Count; j++) {
                var itemSetRule = ItemSetRules[j];
                if (!m_ItemSetListByRule.ContainsKey(itemSetRule)) {
                    m_ItemSetListByRule[itemSetRule] = new List<ItemSet>();
                }

                m_ItemSetRuleStreamData.CharacterItemsBySlot = characterItemsBySlot;
                var itemSetStateInfoListSlice = itemSetRule.GetNextItemSetsStateInfo(m_ItemSetRuleStreamData);
                m_ItemSetStateList.AddRange(itemSetStateInfoListSlice);
            }

            EventHandler.ExecuteEvent<ItemSetGroup, List<ItemSetStateInfo>>(m_GameObject,"OnItemSetGroupWillUpdate", this, m_ItemSetStateList);

            // Reset the index.
            m_DefaultItemSetIndex = -1;
            var temporarySetCount = 0;
            var temporaryAssignedActiveItemSet = -1;
            var temporaryAssignedNextItemSet = -1;
            var activeItemSet = GetActiveItemSet();
            var nextItemSet = GetNextItemSet();

            for (int i = 0; i < m_ItemSetStateList.Count; i++) {
                var itemSetStateInfo = m_ItemSetStateList[i];
                var itemSet = itemSetStateInfo.ItemSet;
                var setCount = temporarySetCount;
                var itemSetRule = itemSetStateInfo.RuleInfo.ItemSetRule;

                // The item set rule might not be the same as the item set rule info (such as for MultiItemSetRule). A new ItemSet list must be created.
                if (!m_ItemSetListByRule.ContainsKey(itemSetRule)) {
                    m_ItemSetListByRule[itemSetRule] = new List<ItemSet>();
                }

                // Add/Remove the ItemSets and update index, for default, active, next, etc.
                switch (itemSetStateInfo.State) {
                    case ItemSetStateInfo.SetState.Keep: {
                        // Update and move Index.
                        m_ItemSetList.Remove(itemSet);
                        m_ItemSetList.Insert(setCount, itemSet);

                        if (temporaryAssignedActiveItemSet == -1 &&
                            (activeItemSet == itemSet)){
                            temporaryAssignedActiveItemSet = setCount;
                        }
                        
                        if (temporaryAssignedNextItemSet == -1 &&
                            (nextItemSet == itemSet )){
                            temporaryAssignedNextItemSet = setCount;
                        }

                        setCount++;
                        break;
                    }
                    case ItemSetStateInfo.SetState.Add: {
                        itemSet.SetItemSetGroup(this);
                        m_ItemSetList.Insert(setCount, itemSet);
                        m_ItemSetListByRule[itemSetRule].Add(itemSet);
                        m_RulesByItemSet[itemSet] = itemSetRule;

                        setCount++;
                        break;
                    }
                    case ItemSetStateInfo.SetState.Remove: {
                        m_ItemSetList.Remove(itemSet);
                        m_ItemSetListByRule[itemSetRule].Remove(itemSet);
                        m_RulesByItemSet.Remove(itemSet);
                        
                        if (activeItemSet == itemSet) {
                            SetStateForItemSet(itemSet, null);
                        }

                        DestroyItemSet(itemSet);
                        break;
                    }
                    default: {
                        throw new ArgumentOutOfRangeException();
                    }
                }
                temporarySetCount = setCount;
                
                if (itemSetStateInfo.Default) {
                    m_DefaultItemSetIndex = setCount - 1;
                }
            }

            var newActiveIndex = IsItemSetValid(temporaryAssignedActiveItemSet, false, -1) ? temporaryAssignedActiveItemSet : -1;
            var newNextIndex = IsItemSetValid(temporaryAssignedNextItemSet, false, -1) ? temporaryAssignedNextItemSet : -1;

            // The active index needs to be set to the next index if it is -1.
            newActiveIndex = newActiveIndex != -1 ? newActiveIndex : newNextIndex;
            
            // Force the update to happen by setting the index to minus one.
            ActiveItemSetIndex = -1;
            NextItemSetIndex = -1;
            
            UpdateActiveItemSet(newActiveIndex);
            UpdateNextItemSet(newNextIndex);

            EventHandler.ExecuteEvent(m_GameObject, "OnItemSetIndexChange", GroupIndex, ActiveItemSetIndex);
            
            GenericObjectPool.Return(pooledItemSets);
            GenericObjectPool.Return(pooledItemCategorySetList);

            if (ActiveItemSetIndex == -1 && NextItemSetIndex == -1 && m_DefaultItemSetIndex != -1) {
                var defaultItemSet = GetDefaultItemSet();
                if (defaultItemSet != null && defaultItemSet.IsValid) {
                    m_EquipUnequip.StartEquipUnequip(m_DefaultItemSetIndex, true, false);
                }
            }
            EventHandler.ExecuteEvent<ItemSetGroup>(m_GameObject, "OnItemSetGroupUpdated", this);
        }

        /// <summary>
        /// Update the next item set index.
        /// </summary>
        /// <param name="itemSetIndex">The new next item set index.</param>
        public void UpdateNextItemSet(int itemSetIndex)
        {
            // No updates are necessary if the indicies are the same.
            if (m_NextItemSetIndex == itemSetIndex) {
                return;
            }

            var prevItemSetIndex = m_NextItemSetIndex != -1 ? m_NextItemSetIndex : ActiveItemSetIndex;
            m_NextItemSetIndex = itemSetIndex;

            EventHandler.ExecuteEvent<int,int,int>(m_GameObject, "OnItemSetManagerUpdateNextItemSet", GroupIndex, prevItemSetIndex, itemSetIndex);
        }

        /// <summary>
        /// Update the active item set index.
        /// </summary>
        /// <param name="newActiveIndex">The new active item set index.</param>
        public virtual void UpdateActiveItemSet(int newActiveIndex)
        {
            // No updates are necessary if the indicies are the same.
            if (m_ActiveItemSetIndex == newActiveIndex) {
                return;
            }

            ItemSet newItemSet = null;
            if (newActiveIndex != -1) {
                // In some cases the ItemSet that had to be activated was disabled during the equip/unequip animation.
                // If that's the case set the newActive index to -1.
                newItemSet = ItemSetList[newActiveIndex];
                if (newItemSet.Enabled == false) {
                    newActiveIndex = -1;
                }
            }

            ItemSet previousItemSet = null;
            var previousIndex = ActiveItemSetIndex;
            // If the active Item Set was removed it might be out of range.
            if (previousIndex != -1 && previousIndex < ItemSetList.Count) {
                previousItemSet = ItemSetList[previousIndex];
                previousItemSet.Active = false;
            }

            m_ActiveItemSetIndex = newActiveIndex;
            m_NextItemSetIndex = -1;

            
            if (newActiveIndex != -1) {
                newItemSet = ItemSetList[newActiveIndex];
                newItemSet.Active = true;
#if UNITY_EDITOR
                if (newItemSet.Enabled == false) {
                    Debug.LogError($"The Item Set [{newActiveIndex}]{newItemSet.State} in the group [{GroupIndex}]{ItemCategory} was set active when it is actually disabled. This should not happen.");
                }
#endif
            }

            SetStateForItemSet(previousItemSet, newItemSet);
            
            EventHandler.ExecuteEvent<int,ItemSet,ItemSet>(m_GameObject, "OnActiveItemSetChange", GroupIndex, previousItemSet, newItemSet);
            EventHandler.ExecuteEvent<int,int>(m_GameObject, "OnItemSetManagerUpdateItemSet", GroupIndex, newActiveIndex);
        }

        /// <summary>
        /// Set the state for the active Item Set.
        /// </summary>
        /// <param name="previousItemSet">The previous active Item Set.</param>
        /// <param name="newItemSet">The new active Item Set.</param>
        protected virtual void SetStateForItemSet(ItemSet previousItemSet, ItemSet newItemSet)
        {
            // No need to make a change if they have the same state.
            var sameState = previousItemSet?.State == newItemSet?.State;
            if (sameState) { return; }

            if (newItemSet != null) {
                var newState = newItemSet.State;
                if (!string.IsNullOrWhiteSpace(newState)) {
                    // Activate the state.
                    StateManager.SetState(m_GameObject, newState, true);
                }
            }
            
            if (previousItemSet != null) {
                var previousState = previousItemSet.State;
                if (!string.IsNullOrWhiteSpace(previousState)) {
                    // No longer active.
                    StateManager.SetState(m_GameObject, previousState, false);
                }
            }
        }

        /// <summary>
        /// Destroy the item set, and remove it from the mappings.
        /// </summary>
        /// <param name="itemSet">The item set to destroy.</param>
        protected void DestroyItemSet(ItemSet itemSet)
        {
            m_ItemSetManager.ReturnItemSetToPool(itemSet);
        }

        /// <summary>
        /// Get the first item set with the provided item.
        /// </summary>
        /// <param name="item">The item to look for.</param>
        /// <param name="checkIfValid">Only return a valid item set?</param>
        /// <returns>The item set which contains the item.</returns>
        public ItemSet GetItemSet(IItemIdentifier item, bool checkIfValid)
        {
            for (int i = 0; i < m_ItemSetList.Count; i++) {
                var itemSet = m_ItemSetList[i];
                for (int j = 0; j < itemSet.ItemIdentifiers.Length; j++) {
                    var otherItem = itemSet.ItemIdentifiers[j];

                    if (otherItem != item) { continue; }

                    if (checkIfValid && !IsItemSetValid(i, false)) {
                        continue;
                    }
                        
                    return itemSet;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Get the first item set that matches the list of items exactly.
        /// </summary>
        /// <param name="items">The list of items per slot.</param>
        /// <returns>The item set that matches the item list.</returns>
        public ItemSet GetItemSet(ListSlice<IItemIdentifier> items)
        {
            for (int i = 0; i < m_ItemSetList.Count; i++) {
                var itemSet = m_ItemSetList[i];

                var matchAll = true;
                for (int j = 0; j < itemSet.ItemIdentifiers.Length; j++) {
                    var otherItem = itemSet.ItemIdentifiers[j];
                    IItemIdentifier item;
                    if (j >= items.Count) {
                        item = null;
                    } else {
                        item = items[j];
                    }

                    if (otherItem != item) {
                        matchAll = false;
                        break;
                    }
                }

                if (matchAll) {
                    return itemSet;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Get the item set that matches the items set state name.
        /// </summary>
        /// <param name="itemSetName">The item set state name.</param>
        /// <returns>The item set that matches the item set state name.</returns>
        public ItemSet GetItemSet(string itemSetName)
        {
            for (int i = 0; i < m_ItemSetList.Count; i++) {
                if (ItemSetList[i].State == itemSetName) { return ItemSetList[i]; }
            }

            return null;
        }
        
        /// <summary>
        /// Get the item set that matches the items set index.
        /// </summary>
        /// <param name="itemSetIndex">The item set index.</param>
        /// <returns>The item set that matches the item set index.</returns>
        public ItemSet GetItemSet(int itemSetIndex)
        {
            if (itemSetIndex < 0 || itemSetIndex >= m_ItemSetList.Count) {
                return null;
            }

            return ItemSetList[itemSetIndex];
        }

        /// <summary>
        /// Get the item set rule index for thr provided item set.
        /// </summary>
        /// <param name="itemSet">The item set that was created by the item set rule.</param>
        /// <returns>The item set rule index.</returns>
        public int GetItemSetRuleIndex(ItemSet itemSet)
        {
            var itemSetRule = GetItemSetRule(itemSet);
            if (itemSetRule == null) {
                return -1;
            }

            return m_ItemSetRules.IndexOf(itemSetRule);
        }
    }
}