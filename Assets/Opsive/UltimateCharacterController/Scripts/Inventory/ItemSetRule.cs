/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using System;
    using Opsive.Shared.Inventory;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An Item Set Rule is used to define how ItemSets are created at runtime depending on the state of the Inventory.
    /// </summary>
    public abstract class ItemSetRuleBase : ScriptableObject, IItemSetRule
    {
        /// <summary>
        /// From the Item Set Rule Stream Data return the next item set state info.
        /// </summary>
        /// <param name="itemSetRuleStreamData">The item set rule stream data.</param>
        /// <returns>Return the item set state info.</returns>
        public abstract ListSlice<ItemSetStateInfo> GetNextItemSetsStateInfo(ItemSetRuleStreamData itemSetRuleStreamData);
        
        /// <summary>
        /// Returns if an item set is valid for the allowed slots mask.
        /// </summary>
        /// <param name="itemSet">The item set to check.</param>
        /// <param name="allowedSlotsMask">The allowed slots mask.</param>
        /// <returns>Returns true if the item set is valid.</returns>
        public abstract bool IsItemSetValid(ItemSet itemSet, int allowedSlotsMask);
    }

    /// <summary>
    /// An Item Set Rule is used to define how ItemSets are created at runtime depending on the state of the Inventory.
    /// </summary>
    public interface IItemSetRule
    {
        string name { get; }
        
        /// <summary>
        /// From the Item Set Rule Stream Data return the next item set state info.
        /// </summary>
        /// <param name="itemSetRuleStreamData">The item set rule stream data.</param>
        /// <returns>Return the item set state info.</returns>
        ListSlice<ItemSetStateInfo> GetNextItemSetsStateInfo(ItemSetRuleStreamData itemSetRuleStreamData);
        
        /// <summary>
        /// Returns if an item set is valid for the allowed slots mask.
        /// </summary>
        /// <param name="itemSet">The item set to check.</param>
        /// <param name="allowedSlotsMask">The allowed slots mask.</param>
        /// <returns>Returns true if the item set is valid.</returns>
        bool IsItemSetValid(ItemSet itemSet, int allowedSlotsMask);
    }

    /// <summary>
    /// An Item Set Rule is used to define how ItemSets are created at runtime depending on the state of the Inventory.
    /// </summary>
    public abstract class ItemSetRule : ItemSetRuleBase
    {
        
        [Tooltip("The default Item Set that is used to create the runtime item sets.")]
        [SerializeField] protected ItemSet m_DefaultItemSet = new ItemSet("{0}");
        [Tooltip("Is the ItemSet the default ItemSet?")]
        [SerializeField] protected bool m_Default = false;

        protected List<ItemSetStateInfo> m_TemporaryItemSetStateInfos;
        protected PooledItemPermutationList m_PooledItemSlotPermutations;
        
        [Shared.Utility.NonSerialized] public string State { get { return m_DefaultItemSet.State; } set { m_DefaultItemSet.State = value; } }
        public bool Default { get { return m_Default; } set { m_Default = value; } }
        
        public ItemSet DefaultItemSet { get { return m_DefaultItemSet; } set { m_DefaultItemSet = value; } }

        /// <summary>
        /// From the Item Set Rule Stream Data return the next item set state info.
        /// </summary>
        /// <param name="itemSetRuleStreamData">The item set rule stream data.</param>
        /// <returns>Return the item set state info.</returns>
        public override ListSlice<ItemSetStateInfo> GetNextItemSetsStateInfo(ItemSetRuleStreamData itemSetRuleStreamData)
        {
            var itemSetManager = itemSetRuleStreamData.ItemSetManager;
            var groupIndex = itemSetRuleStreamData.GroupIndex;
            var slotCount = itemSetManager.SlotCount;
            var itemSetGroup = itemSetManager.ItemSetGroups[groupIndex];
            var currentItemSets = itemSetGroup.GetRuleItemSetList(this);
            var itemSetRuleInfo = new ItemSetRuleInfo(itemSetGroup, this);
            
            // Initialize the lists
            if (m_TemporaryItemSetStateInfos == null) {
                m_TemporaryItemSetStateInfos = new List<ItemSetStateInfo>();
            } else {
                m_TemporaryItemSetStateInfos.Clear();
            }

            if (m_PooledItemSlotPermutations == null) {
                m_PooledItemSlotPermutations = new PooledItemPermutationList(slotCount);
            } else {
                m_PooledItemSlotPermutations.Clear();
            }

            // Set all the possible valid permutations. 
            var validSlotPermutations = GetValidSlotPermutations(itemSetRuleStreamData);

            //Set what item set are to keep, add or remove.
            for (int i = 0; i < currentItemSets.Count; i++) {

                var currentItemSet = currentItemSets[i];
                var foundSetMatch = false;
                for (int j = validSlotPermutations.Count - 1; j >= 0; j--) {
                    var itemPermutation = validSlotPermutations[j];

                    var allSlotMatch = true;
                    for (int k = 0; k < slotCount; k++) {
                        if (currentItemSet.ItemIdentifiers[k] != itemPermutation[k]) {
                            allSlotMatch = false;
                            break;
                        }
                    }

                    if (!allSlotMatch) { continue; }

                    foundSetMatch = true;
                    m_TemporaryItemSetStateInfos.Add(
                        new ItemSetStateInfo(itemSetRuleInfo, currentItemSet,ItemSetStateInfo.SetState.Keep, m_Default));
                    validSlotPermutations.RemoveAt(j);
                    break;
                }

                if (foundSetMatch == false) {
                    m_TemporaryItemSetStateInfos.Add(
                        new ItemSetStateInfo(itemSetRuleInfo, currentItemSet,ItemSetStateInfo.SetState.Remove, m_Default));
                }
            }
            

            for (int i = 0; i < validSlotPermutations.Count; i++) {
                //New item set index set to -1 it will be updated once it is really set.
                var newItemSet = CreateItemSet(itemSetManager, validSlotPermutations[i]);
                newItemSet.SetItemSetGroup(itemSetRuleStreamData.ItemSetGroup);
                
                m_TemporaryItemSetStateInfos.Add(
                    new ItemSetStateInfo(itemSetRuleInfo, newItemSet,ItemSetStateInfo.SetState.Add, m_Default));
            }
            
            validSlotPermutations.Clear();

            return m_TemporaryItemSetStateInfos;
        }

        /// <summary>
        /// Does the character item match this rule.
        /// </summary>
        /// <param name="itemSetRuleStreamData">The item set rule stream data.</param>
        /// <param name="currentPermutation">The current item permutation so far.</param>
        /// <param name="characterItem">The character item to check.</param>
        /// <returns>True if the character item matches this rule.</returns>
        public abstract bool DoesCharacterItemMatchRule(ItemSetRuleStreamData itemSetRuleStreamData, ListSlice<IItemIdentifier> currentPermutation,
            CharacterItem characterItem);

        /// <summary>
        /// Can the slot be empty for this rule.
        /// </summary>
        /// <param name="slotID">The slot ID to check.</param>
        /// <returns>True if it can be empty.</returns>
        protected abstract bool CanSlotBeNull(int slotID);

        /// <summary>
        /// Get the valid slot permutations for the item set rule stream data.
        /// </summary>
        /// <param name="itemSetRuleStreamData">Th item set rules stream data.</param>
        /// <returns>Returns a pooled item permutation list which can be used to create ItemSets.</returns>
        public virtual PooledItemPermutationList GetValidSlotPermutations(ItemSetRuleStreamData itemSetRuleStreamData)
        {
           
            m_PooledItemSlotPermutations.Clear();
            m_PooledItemSlotPermutations.SlotCount = itemSetRuleStreamData.SlotCount;

            var slotPermutation = m_PooledItemSlotPermutations.Add();
            GetAllValidSlotPermutations(itemSetRuleStreamData,0, slotPermutation, m_PooledItemSlotPermutations);

            return m_PooledItemSlotPermutations;
        }

        /// <summary>
        /// Get all the valid slot permutations for the provided item set rule stream data. can be used recursively.
        /// </summary>
        /// <param name="itemSetRuleStreamData">The item set rules stream data containing information about the item sets.</param>
        /// <param name="startSlotID">The index in which to start the permutations.</param>
        /// <param name="currentSlotPermutation">The current permutation to complete.</param>
        /// <param name="result">The pooled item permutation list result with all the valid permutations so far.</param>
        public virtual void GetAllValidSlotPermutations(ItemSetRuleStreamData itemSetRuleStreamData, int startSlotID,
            ResizableArray<IItemIdentifier> currentSlotPermutation, PooledItemPermutationList result)
        {
            var characterItemsBySlot = itemSetRuleStreamData.CharacterItemsBySlot;
            
            var slotCount = itemSetRuleStreamData.SlotCount;
            for (int slotID = startSlotID; slotID < slotCount; slotID++) {

                if (CanSlotBeNull(slotID)) {
                    //Other match must create new set.
                    var nextSlotID = slotID + 1;
                    var newPermutation = result.Add();
                    currentSlotPermutation.CopyTo(newPermutation, 0, nextSlotID);
                    newPermutation[slotID] = null;

                    GetAllValidSlotPermutations(itemSetRuleStreamData, nextSlotID, newPermutation,result);
                }
                
                var foundMatch = false;
                IItemIdentifier itemFound = null;
                for (int j = 0; j < characterItemsBySlot[slotID].Count; j++) {
                    var characterItem = characterItemsBySlot[slotID][j];
                    
                    // Setting to null to not confuse the DoesCharacterItemMatchRule currentSlotPermutation. This is the slot that the item is being added to.
                    currentSlotPermutation[slotID] = null;
                    
                    if (DoesCharacterItemMatchRule(itemSetRuleStreamData, currentSlotPermutation, characterItem) == false) {
                        continue;
                    }

                    if (!foundMatch) {
                        //First match add to current.
                        itemFound = characterItem.ItemIdentifier;
                        currentSlotPermutation[slotID] = characterItem.ItemIdentifier;
                        foundMatch = true;
                        continue;
                    }

                    //Other match must create new set.
                    var nextSlotID = slotID + 1;
                    var newPermutation = result.Add();
                    currentSlotPermutation.CopyTo(newPermutation, 0, nextSlotID);
                    newPermutation[slotID] = characterItem.ItemIdentifier;

                    GetAllValidSlotPermutations(itemSetRuleStreamData, nextSlotID, newPermutation, result);
                }

                if (foundMatch) {
                    currentSlotPermutation[slotID] = itemFound;
                    continue;
                }

                //No match was found, no need to look further.
                result.Remove(currentSlotPermutation);
                break;
            }
        }

        /// <summary>
        /// Create an item set.
        /// </summary>
        /// <param name="itemSetManager">The item set manager.</param>
        /// <param name="itemsInSet">The items within the item set.</param>
        /// <returns>The item set.</returns>
        protected virtual ItemSet CreateItemSet(ItemSetManagerBase itemSetManager, ListSlice<IItemIdentifier> itemsInSet)
        {
            var slotCount = itemSetManager.SlotCount;
            var itemSet = itemSetManager.PopItemSetFromPool(this, DefaultItemSet);

            if (itemSet.ItemIdentifiers == null || itemSet.ItemIdentifiers.Length != slotCount) {
                itemSet.ItemIdentifiers = new IItemIdentifier[slotCount];
            }

            for (int i = 0; i < slotCount; i++) {
                itemSet.ItemIdentifiers[i] = itemsInSet[i];
            }
            
            itemSet.State = GetMainStateNameForItemSet(itemSet, itemSetManager, itemsInSet);

            return itemSet;
        }

        /// <summary>
        /// Get the state name for the item set.
        /// </summary>
        /// <param name="itemSet">The item set to get the state name from.</param>
        /// <param name="itemSetManager">The item set manager.</param>
        /// <param name="itemsInSet">The items in the set.</param>
        /// <returns>The state name for the item set.</returns>
        protected virtual string GetMainStateNameForItemSet(ItemSet itemSet, ItemSetManagerBase itemSetManager, ListSlice<IItemIdentifier> itemsInSet)
        {
            if (State.Contains("{0}") == false) {
                return State;
            }

            var itemNames = "";
            for (int i = 0; i < itemSetManager.SlotCount; i++) {
                itemNames += itemsInSet[i]?.GetItemDefinition()?.name;
            }
            return string.Format(State, itemNames);
        }
    }
}