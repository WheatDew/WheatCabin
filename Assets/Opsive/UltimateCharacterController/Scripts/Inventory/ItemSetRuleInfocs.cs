/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items;
    using System.Collections.Generic;

    /// <summary>
    /// The item set rule stream data contains a list of potential item set by having a list of character items per slot.
    /// </summary>
    public class ItemSetRuleStreamData
    {
        protected ItemSetGroup m_ItemSetGroup;
        protected List<CharacterItem>[] m_CharacterItemsBySlot;


        public List<CharacterItem>[]CharacterItemsBySlot { get => m_CharacterItemsBySlot; set => m_CharacterItemsBySlot = value; }

        public ItemSetGroup ItemSetGroup { get => m_ItemSetGroup; set => m_ItemSetGroup = value; }

        public ItemSetManagerBase ItemSetManager => m_ItemSetGroup.ItemSetManager;

        public int GroupIndex => m_ItemSetGroup.GroupIndex;

        public int SlotCount => ItemSetManager.SlotCount;

        public InventoryBase CharacterInventory => ItemSetManager.CharacterInventory;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public ItemSetRuleStreamData()
        {
            // Nothing.
        }

        /// <summary>
        /// The item set group constructor.
        /// </summary>
        /// <param name="itemSetGroup">The ItemSetGroup represented by the rule.</param>
        public ItemSetRuleStreamData(ItemSetGroup itemSetGroup)
        {
            m_ItemSetGroup = itemSetGroup;
        }
    }

    /// <summary>
    /// The pooled item permutation list contains a list of potential item sets by having a list of items per slot.
    /// </summary>
    public class PooledItemPermutationList
    {
        protected List<ResizableArray<IItemIdentifier>> m_ItemSlotPermutations;
        
        protected int m_SlotCount;

        public int SlotCount
        {
            get => m_SlotCount;
            set
            {
                m_SlotCount = value;
                for (int i = 0; i < m_ItemSlotPermutations.Count; i++) {
                    m_ItemSlotPermutations[i].Resize(m_SlotCount);
                }
            }
        }

        public int Count => m_ItemSlotPermutations.Count;
        public ListSlice<IItemIdentifier> this[int index] { get => m_ItemSlotPermutations[index]; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PooledItemPermutationList()
        {
            m_ItemSlotPermutations = new List<ResizableArray<IItemIdentifier>>();
        }
        
        /// <summary>
        /// Constructor with the slot count.
        /// </summary>
        /// <param name="slotCount">The slot count.</param>
        public PooledItemPermutationList(int slotCount)
        {
            m_ItemSlotPermutations = new List<ResizableArray<IItemIdentifier>>();
            m_SlotCount = slotCount;
        }
        
        /// <summary>
        /// Add an entry in the list.
        /// </summary>
        /// <returns>The new entry in the list of permutations.</returns>
        public ResizableArray<IItemIdentifier> Add()
        {
            var itemIdentifiers = GenericObjectPool.Get<ResizableArray<IItemIdentifier>>();
            itemIdentifiers.Resize(m_SlotCount);
            for (int i = 0; i < itemIdentifiers.Count; i++) {
                itemIdentifiers[i] = null;
            }
            m_ItemSlotPermutations.Add(itemIdentifiers);
            return itemIdentifiers;
        }

        /// <summary>
        /// Remove a permutation.
        /// </summary>
        /// <param name="itemIdentifiers">the permutation to remove.</param>
        public void Remove(ResizableArray<IItemIdentifier> itemIdentifiers)
        {
            m_ItemSlotPermutations.Remove(itemIdentifiers);
            GenericObjectPool.Return(itemIdentifiers);
        }
        
        /// <summary>
        /// Remove the permutation at the index.
        /// </summary>
        /// <param name="index">The index of the permutation to remove.</param>
        public void RemoveAt(int index)
        {
            var permutation = m_ItemSlotPermutations[index];
            m_ItemSlotPermutations.RemoveAt(index);
            GenericObjectPool.Return(permutation);
        }

        /// <summary>
        /// Clear all the permutations.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < m_ItemSlotPermutations.Count; i++) {
                GenericObjectPool.Return(m_ItemSlotPermutations[i]);
            }
            m_ItemSlotPermutations.Clear();
        }
    }
    
    /// <summary>
    /// a struct with infomration about an item set rule.
    /// </summary>
    public struct ItemSetRuleInfo
    {
        private ItemSetGroup m_ItemSetGroup;
        private ItemSetRuleBase m_ItemSetRule;

        public ItemSetGroup ItemSetGroup => m_ItemSetGroup;
        public ItemSetRuleBase ItemSetRule => m_ItemSetRule;

        /// <summary>
        /// Constructor of the item set rule info.
        /// </summary>
        /// <param name="itemSetGroup">The item set group.</param>
        /// <param name="itemSetRule">The item set rule.</param>
        public ItemSetRuleInfo(ItemSetGroup itemSetGroup, ItemSetRuleBase itemSetRule)
        {
            m_ItemSetGroup = itemSetGroup;
            m_ItemSetRule = itemSetRule;
        }
    }

    /// <summary>
    /// The item set state info contains information about whether an item set is to be added, kept or removed.
    /// </summary>
    public struct ItemSetStateInfo
    {
        public enum SetState{
            Keep,
            Add,
            Remove
        }

        private ItemSetRuleInfo m_RuleInfo;
        private ItemSet m_ItemSet;
        private SetState m_State;
        private bool m_Default;

        public ItemSetRuleInfo RuleInfo => m_RuleInfo;
        public ItemSet ItemSet => m_ItemSet;
        public SetState State => m_State;
        public bool Default => m_Default;

        /// <summary>
        /// The constructor of the info.
        /// </summary>
        /// <param name="ruleInfo">The item set rule info.</param>
        /// <param name="itemSet">The item set.</param>
        /// <param name="state">The item set state.</param>
        /// <param name="default">Is this the default item set?</param>
        public ItemSetStateInfo(ItemSetRuleInfo ruleInfo, ItemSet itemSet, SetState state, bool @default = false)
        {
            m_RuleInfo = ruleInfo;
            m_ItemSet = itemSet;
            m_State = state;
            m_Default = @default;
        }
    }
   
}