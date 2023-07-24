/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.Inventory;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Items;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    
    /// <summary>
    /// Represents a set of ItemIdentifiers that can be equipped at the same time.
    /// </summary>
    [Serializable]
    public class ItemSet : StateObject
    {
        [Tooltip("The state to change to when the ItemSet is active Use {0} to add the item names in order.")]
        [SerializeField] [StateName] protected string m_State;
        [Tooltip("Is the ItemSet enabled?")]
        [SerializeField] protected bool m_Enabled = true;
        [Tooltip("Can the ItemSet be switched to by the EquipNext/EquipPrevious abilities?")]
        [SerializeField] protected bool m_CanSwitchTo = true;
        [Tooltip("The ItemSet index that should be activated when the current ItemSet is active and disabled.")]
        [SerializeField] protected int m_DisabledIndex = -1;

        [Shared.Utility.NonSerialized] public string State { get { return m_State; } set { m_State = value; } }
        public bool Enabled { get { return m_Enabled; } set { m_Enabled = value; } }
        public bool CanSwitchTo { get { return m_CanSwitchTo; } set { m_CanSwitchTo = value; } }
        public int DisabledIndex { get { return m_DisabledIndex; } set { m_DisabledIndex = value; } }

        [System.NonSerialized] private ItemSetManagerBase m_ItemSetManager;
        [System.NonSerialized] private ItemSetGroup m_ItemSetGroup;

        [System.NonSerialized] private IItemIdentifier[] m_ItemIdentifiers;
        [System.NonSerialized] private bool m_Active;
        [System.NonSerialized] private bool m_EmptyItemSet;

        [Shared.Utility.NonSerialized] public bool Active { get => m_Active; set { m_Active = value; } }
        [Shared.Utility.NonSerialized] public IItemIdentifier[] ItemIdentifiers { get { return m_ItemIdentifiers; } set { m_ItemIdentifiers = value; } }
        
        [Shared.Utility.NonSerialized] public EquipUnequip EquipUnequip => m_ItemSetGroup.EquipUnequip;
        [Shared.Utility.NonSerialized] public int Index => m_ItemSetGroup.GetItemSetIndex(this);
        [Shared.Utility.NonSerialized] public int GroupIndex => ItemSetGroup.GroupIndex;
        [Shared.Utility.NonSerialized] public ItemSetGroup ItemSetGroup => m_ItemSetGroup;
        [Shared.Utility.NonSerialized] public bool Default => ItemSetGroup?.GetDefaultItemSet() == this;
        [Shared.Utility.NonSerialized] public IItemSetRule ItemSetRule => ItemSetGroup?.GetItemSetRule(this);
        [Shared.Utility.NonSerialized] public int SlotCount => m_ItemIdentifiers.Length;
        [Shared.Utility.NonSerialized] public int ItemSetRuleIndex => ItemSetGroup?.GetItemSetRuleIndex(this) ?? 0;
        [Shared.Utility.NonSerialized] public bool IsValid => ItemSetGroup?.IsItemSetValid(this, false) ?? false;

        /// <summary>
        /// Default ItemSet constructor. 
        /// </summary>
        public ItemSet()
        {
            m_Enabled = true;
        }
        
        /// <summary>
        /// ItemSet constructor with the state name. 
        /// </summary>
        public ItemSet(string stateName)
        {
            m_State = stateName;
            m_Enabled = true;
        }

        /// <summary>
        /// ItemSet constructor which copies the parameters from an existing ItemSet. 
        /// </summary>
        /// <param name="itemSet">The ItemSet to copy the values of.</param>
        public ItemSet(ItemSet itemSet)
        {
            m_State = itemSet.State;
            m_Enabled = itemSet.Enabled;
            m_CanSwitchTo = itemSet.CanSwitchTo;
            m_DisabledIndex = itemSet.DisabledIndex;
            if (itemSet.ItemIdentifiers != null && itemSet.ItemIdentifiers.Length > 0) {
                m_ItemIdentifiers = new IItemIdentifier[itemSet.ItemIdentifiers.Length];
                System.Array.Copy(itemSet.ItemIdentifiers, m_ItemIdentifiers, itemSet.ItemIdentifiers.Length);
            } else {
                m_ItemIdentifiers = Array.Empty<IItemIdentifier>();
            }
            // Deep copy of the states
            if (itemSet.States != null && itemSet.States.Length > 1) {
                m_States = new State[itemSet.States.Length];
                for (int i = 0; i < m_States.Length; i++) {
                    var originalState = itemSet.States[i];
                    if (originalState.Default) {
                        m_States[i] = new State(originalState.Name, true);
                    } else {
                        m_States[i] = new State(originalState.Name, originalState.Preset, originalState.BlockList);
                    }
                }
            } else {
                m_States = new State[] { new State("Default", true) };
            }
        }

        /// <summary>
        /// Four parameter ItemSet constructor. 
        /// </summary>
        /// <param name="slotCount">The number of slots used by the ItemSet.</param>
        /// <param name="slotID">The ID of the slot that will use the ItemSet.</param>
        /// <param name="itemDefinition">The ItemDefinition of the ItemSet.</param>
        /// <param name="itemIdentifier">The ItemIdentifier of the ItemSet.</param>
        /// <param name="state">The state to change to when the ItemSet is active.</param>
        public ItemSet(int slotCount, int slotID, ItemDefinitionBase itemDefinition, IItemIdentifier itemIdentifier, string state)
        {
            m_State = state;
            m_Enabled = true;
            m_ItemIdentifiers = new IItemIdentifier[slotCount];
            m_ItemIdentifiers[slotID] = itemIdentifier;
        }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        /// <param name="gameObject">The GameObject this object is attached to.</param>
        /// <param name="itemSetManager">The ItemSetManager which owns the ItemSet.</param>
        /// <param name="categoryID">The ID of the category that the ItemSet belongs to.</param>
        /// <param name="categoryIndex">The index of the category that the ItemSet belongs to.</param>
        /// <param name="index">The index of the ItemSet.</param>
        public void Initialize(GameObject gameObject, ItemSetManagerBase itemSetManager)
        {
            // The ItemSet may have already been initialized.
            if (m_ItemSetManager != null) {
                return;
            }

            base.Initialize(gameObject);

            m_ItemSetManager = itemSetManager;

            if (m_ItemIdentifiers == null) {
                m_ItemIdentifiers = new IItemIdentifier[itemSetManager.SlotCount];
            }
        }

        /// <summary>
        /// When the item set is popped from the pool, call the item set manager to update.
        /// </summary>
        public void OnPopFromPool()
        {
            EventHandler.RegisterEvent<int, int>(m_ItemSetManager.gameObject, "OnItemSetManagerUpdateItemSet", OnUpdateItemSet);
        }

        /// <summary>
        /// Set the item set group for this item set.
        /// </summary>
        /// <param name="itemSetGroup">The item set group.</param>
        public void SetItemSetGroup(ItemSetGroup itemSetGroup)
        {
            m_ItemSetGroup = itemSetGroup;
        }

        /// <summary>
        /// Set the item identifier per slots.
        /// </summary>
        /// <param name="items">The items per slot.</param>
        public void SetItemIdentifiers(ListSlice<IItemIdentifier> items)
        {
            m_EmptyItemSet = true;
            for (int i = 0; i < m_ItemIdentifiers.Length; i++) {
                if (items.Count > i) {
                    m_EmptyItemSet = false;
                    m_ItemIdentifiers[i] = items[i];
                } else {
                    m_ItemIdentifiers[i] = null;
                }
            }
        }

        /// <summary>
        /// Get the character item in a slot.
        /// </summary>
        /// <param name="slotID">The slot id where the item is set.</param>
        /// <returns>The character item in the slot.</returns>
        public CharacterItem GetCharacterItem(int slotID)
        {
            return ItemSetGroup.ItemSetManager.CharacterInventory.GetCharacterItem(m_ItemIdentifiers[slotID], slotID);
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            base.StateChange();

            var itemSetIndex = m_ItemSetGroup.GetItemSetIndex(this);

            var forceEquipUnequip = !m_ItemSetManager.gameObject.activeInHierarchy;
            // If the character is disabled the equip/unequip must be immediate.
            var immediateEquipUnequip = !m_ItemSetManager.gameObject.activeInHierarchy;

            // The item set is active and the enabled state changed then the item set should be activated or deactivated. This is done through the Equip Unequip ability.
            if (m_Active) {
                if (m_Enabled) {
                    var targetItemSetIndex = EquipUnequip.IsActive ? EquipUnequip.ActiveItemSetIndex : ItemSetGroup.ActiveItemSetIndex;
                    if ((targetItemSetIndex == -1 || targetItemSetIndex == ItemSetGroup.DefaultItemSetIndex && targetItemSetIndex != itemSetIndex)) {
                        EquipUnequip.StartEquipUnequip(itemSetIndex, forceEquipUnequip, immediateEquipUnequip);
                    }
                } else {
                    if (m_DisabledIndex == -1) {
                        var defaultItemSetIndex = ItemSetGroup.DefaultItemSetIndex;
                        if (itemSetIndex == defaultItemSetIndex || !m_ItemSetManager.IsItemSetValid(GroupIndex, defaultItemSetIndex, false)) {
                            // The current item set is equal to the ItemSet being disabled. Equip an empty item set.
                            EquipUnequip.StartEquipUnequip(-1, forceEquipUnequip, immediateEquipUnequip);
                        } else {
                            EquipUnequip.StartEquipUnequip(defaultItemSetIndex, forceEquipUnequip, immediateEquipUnequip);
                        }
                    } else {
                        if (m_ItemSetManager.IsItemSetValid(GroupIndex, m_DisabledIndex, false)) {
                            EquipUnequip.StartEquipUnequip(m_DisabledIndex, forceEquipUnequip, immediateEquipUnequip);
                        } else {
                            EquipUnequip.StartEquipUnequip(-1, forceEquipUnequip, immediateEquipUnequip);
                        }
                    }
                }
            } else if (m_Enabled && (m_EmptyItemSet || m_ItemSetManager.IsItemSetValid(GroupIndex, itemSetIndex, false))) {
                // If the item set is not active and it is enabled then the item set should be enabled if it can be.
                var targetItemSetIndex = EquipUnequip.IsActive ? EquipUnequip.ActiveItemSetIndex : m_ItemSetManager.GetActiveItemSetIndex(GroupIndex);
                if (targetItemSetIndex == -1) {
                    EquipUnequip.StartEquipUnequip(itemSetIndex, forceEquipUnequip, immediateEquipUnequip);
                }
            }
        }

        /// <summary>
        /// The ItemSet has changed.
        /// </summary>
        /// <param name="groupIndex">The index of the changed category.</param>
        /// <param name="itemSetIndex">The index of the changed ItemSet.</param>
        private void OnUpdateItemSet(int groupIndex, int itemSetIndex)
        {
            if (groupIndex == GroupIndex || !m_Enabled) {
                return;
            }

            var activeItemSetIndex = m_ItemSetManager.GetActiveItemSetIndex(GroupIndex);
            if (activeItemSetIndex != -1) {
                return;
            }

            if (IsValid == false) {
                return;
            }

            // The ItemSet may need to be enabled.
            if (!EquipUnequip.IsActive) {
                EquipUnequip.StartEquipUnequip(Index);
            }
        }

        /// <summary>
        /// Return the item set to the pool of item sets to be reused later.
        /// </summary>
        public void ReturnToPool()
        {
            if (m_ItemSetManager == null) {
                return;
            }
            m_ItemSetManager.ReturnItemSetToPool(this);
        }

        /// <summary>
        /// The ItemSet has been destroyed (returned to the pool). Reset everything and update the item set manager.
        /// </summary>
        public void OnReturnToPool()
        {
            // Do not reset the States because it has to persist.
            
            for (int i = 0; i < ItemIdentifiers.Length; i++) {
                ItemIdentifiers[i] = null;
            }
            Enabled = false;
            Active = false;
            CanSwitchTo = false;
            DisabledIndex = -1;
            m_Active = false;
            m_Enabled = false;
            if (m_ItemSetManager == null) {
                return;
            }
            EventHandler.UnregisterEvent<int, int>(m_ItemSetManager.gameObject, "OnItemSetManagerUpdateItemSet", OnUpdateItemSet);
        }
    }
}