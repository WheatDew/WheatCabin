/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An Item Action Monitor is used to show Item Action Module Groups in the UI.
    /// </summary>
    public class ModuleSwitcherGroupMonitor : ItemMonitor
    {
        [Tooltip("The ID that UI represents.")]
        [SerializeField] protected int m_ID;
        [Tooltip("The action ID that the UI represents.")]
        [SerializeField] protected int m_ItemActionID;
        [Tooltip("The content that is disabled when there is nothing to show.")]
        [SerializeField] protected GameObject m_Content;
        [Tooltip("Should the monitor selection loop indexes when the limits are reached? 0 -> Max, Max -> 0")]
        [SerializeField] protected bool m_Loop = true;
        [Tooltip("The action ID that the UI represents.")]
        [SerializeField] protected ModuleSwitcherMonitor[] m_ModuleSwitcherMonitors;
        
        [System.NonSerialized] private GameObject m_GameObject;
        
        private CharacterItem m_EquippedCharacterItem;
        private List<IModuleSwitcher> m_CachedSwitcherModules;
        private int m_SelectedIndex;

        private List<ModuleSwitcherMonitor> m_ActiveSwitchers;
        
        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_CachedSwitcherModules = new List<IModuleSwitcher>();
            m_ActiveSwitchers = new List<ModuleSwitcherMonitor>();
        }

        /// <summary>
        /// Starts the UI.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            for (int i = 0; i < m_ModuleSwitcherMonitors.Length; i++) {
                m_ModuleSwitcherMonitors[i].OnSelect += HandleMonitorSelected;
            }
        }

        /// <summary>
        /// Handle a swticher monitor being selected, it can be selected externally.
        /// </summary>
        /// <param name="monitor">The monitor that was selected or deselected.</param>
        /// <param name="selected">Is the monitor selected?</param>
        private void HandleMonitorSelected(ModuleSwitcherMonitor monitor, bool selected)
        {
            var monitorIndex = m_ActiveSwitchers.IndexOf(monitor);
            if(monitorIndex == -1){ return; }
            if(selected == false){ return; }
            if(monitorIndex == m_SelectedIndex){ return; }
            
            SelectMonitor(monitorIndex);
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected override void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<CharacterItem>(m_Character, "OnRefreshSlotItemMonitor", OnRefresh);
                EventHandler.UnregisterEvent<CharacterItem, int>(m_Character, "OnAbilityWillEquipItem", OnEquipItem);
                EventHandler.UnregisterEvent<CharacterItem, int>(m_Character, "OnAbilityUnequipItemComplete", OnUnequipItem);
                EventHandler.UnregisterEvent<CharacterItem, int>(m_Character, "OnInventoryRemoveItem", OnUnequipItem);
                ResetMonitor();
            }

            base.OnAttachCharacter(character);

            if (m_Character == null || m_CharacterInventory == null) {
                return;
            }

            gameObject.SetActive(CanShowUI());
            EventHandler.RegisterEvent<CharacterItem>(m_Character, "OnRefreshSlotItemMonitor", OnRefresh);
            EventHandler.RegisterEvent<CharacterItem, int>(m_Character, "OnAbilityWillEquipItem", OnEquipItem);
            EventHandler.RegisterEvent<CharacterItem, int>(m_Character, "OnAbilityUnequipItemComplete", OnUnequipItem);
            EventHandler.RegisterEvent<CharacterItem, int>(m_Character, "OnInventoryRemoveItem", OnUnequipItem);
            // An item may already be equipped.
            for (int i = 0; i < m_CharacterInventory.SlotCount; ++i) {
                var item = m_CharacterInventory.GetActiveCharacterItem(i);
                if (item != null) {
                    OnEquipItem(item, i);
                }
            }
        }

        /// <summary>
        /// An item has been equipped.
        /// </summary>
        /// <param name="characterItem">The equipped item.</param>
        /// <param name="slotID">The slot that the item now occupies.</param>
        private void OnEquipItem(CharacterItem characterItem, int slotID)
        {
            if (!characterItem.DominantItem || characterItem.UIMonitorID != m_ID) {
                return;
            }
            
            m_EquippedCharacterItem = characterItem;

            OnMonitoredCharacterItemChanged(m_EquippedCharacterItem);

            OnRefresh(characterItem);
            
            // Select 0 each time a new item is equipped.
            SelectMonitor(0);
        }
        
        /// <summary>
        /// Refresh the Monitor by drawing the character item.
        /// </summary>
        /// <param name="characterItem">The character item to draw.</param>
        protected virtual void OnRefresh(CharacterItem characterItem)
        {
            if (!characterItem.DominantItem || characterItem.UIMonitorID != m_ID) {
                return;
            }
            
            m_GameObject.SetActive(CanShowUI());

            // Multiple item actions can be attached to the same item.
            CharacterItemAction itemAction = null;
            if (m_ItemActionID < characterItem.ItemActions.Length) {
                itemAction = characterItem.ItemActions[m_ItemActionID];
            }

            if (itemAction == null) {
                DisableMonitorContent();
                return;
            }
            
            m_CachedSwitcherModules.Clear();
            var allSwitchers = itemAction.GetActiveModules(m_CachedSwitcherModules);

            m_ActiveSwitchers.Clear();
            for (int i = 0; i < m_ModuleSwitcherMonitors.Length; i++) {
                var switcherMonitor = m_ModuleSwitcherMonitors[i];
                var foundMatch = false;
                for (int j = 0; j < allSwitchers.Count; j++) {
                    var moduleSwitcher = allSwitchers[j];
                    if (switcherMonitor.DoesSwitcherMatch(moduleSwitcher)) {
                        switcherMonitor.SetSwitcher(moduleSwitcher);
                        foundMatch = true;
                        m_ActiveSwitchers.Add(switcherMonitor);
                        break;
                    }
                }

                if (foundMatch == false) {
                    switcherMonitor.SetSwitcher(null);
                }
            }

            if (m_ActiveSwitchers.Count > 0) {
                EnableMonitorContent();
            } else {
                DisableMonitorContent();
            }
        }
        
        /// <summary>
        /// Enables the content of the monitor.
        /// </summary>
        private void EnableMonitorContent()
        {
            m_Content.SetActive(true);
        }

        /// <summary>
        /// Disables the content of the monitor.
        /// </summary>
        private void DisableMonitorContent()
        {
            m_Content.SetActive(false);
        }

        /// <summary>
        /// Select the monitor at the index provided.
        /// </summary>
        /// <param name="index">The index of the monitor to select.</param>
        public void SelectMonitor(int index)
        {
            m_SelectedIndex = index;
            for (int i = 0; i < m_ActiveSwitchers.Count; i++) {
                var select = m_SelectedIndex == i;
                m_ActiveSwitchers[i].Select(select);
            }
        }
        
        /// <summary>
        /// Select the previous index.
        /// </summary>
        public void SelectPreviousMonitor()
        {
            if (m_SelectedIndex <= 0 && m_Loop) {
                SelectMonitor(m_ActiveSwitchers.Count-1);
            } else {
                SelectMonitor(m_SelectedIndex - 1);
            }
        }

        /// <summary>
        /// Select to the next index.
        /// </summary>
        public void SelectNextMonitor()
        {
            if (m_SelectedIndex >= m_ActiveSwitchers.Count - 1 && m_Loop) {
                SelectMonitor(0);
            } else {
                SelectMonitor(m_SelectedIndex + 1);
            }
        }

        /// <summary>
        /// Get the selected switcher monitor.
        /// </summary>
        /// <returns></returns>
        public ModuleSwitcherMonitor GetSelectedSwitcherMonitor()
        {
            if (m_SelectedIndex < 0 || m_SelectedIndex >= m_ActiveSwitchers.Count) {
                return null;
            }

            return m_ActiveSwitchers[m_SelectedIndex];
        }

        /// <summary>
        /// The DominantItem field has been updated for the specified item.
        /// </summary>
        /// <param name="characterItem">The Item whose DominantItem field was updated.</param>
        /// <param name="dominantItem">True if the item is now a dominant item.</param>
        protected override void OnUpdateDominantItem(CharacterItem characterItem, bool dominantItem)
        {
            if ((m_EquippedCharacterItem != null && characterItem != m_EquippedCharacterItem) 
                || m_CharacterInventory.GetItemIdentifierAmount(characterItem.ItemIdentifier) == 0 
                || m_CharacterInventory.GetActiveCharacterItem(characterItem.SlotID) != characterItem) {
                return;
            }

            if (characterItem.DominantItem) {
                OnEquipItem(characterItem, characterItem.SlotID);
            } else {
                ResetMonitor();
            }
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="characterItem">The unequipped item.</param>
        /// <param name="slotID">The slot that the item previously occupied.</param>
        private void OnUnequipItem(CharacterItem characterItem, int slotID)
        {
            CharacterItem equippedCharacterItem = null;
            if (!characterItem.DominantItem 
                || characterItem.UIMonitorID != m_ID 
                || ((equippedCharacterItem = m_CharacterInventory.GetActiveCharacterItem(slotID)) != null 
                    && equippedCharacterItem.DominantItem && equippedCharacterItem != characterItem)) {
                return;
            }

            ResetMonitor();
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return base.CanShowUI() 
                   && m_EquippedCharacterItem != null 
                   && m_ModuleSwitcherMonitors != null 
                   && m_ModuleSwitcherMonitors.Length != 0;
        }

        /// <summary>
        /// Resets the monitor back to the default state.
        /// </summary>
        private void ResetMonitor()
        {
            m_EquippedCharacterItem = null;
            OnMonitoredCharacterItemChanged(null);
            gameObject.SetActive(false);
        }
    }
}