/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Items.Actions.Modules;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The ItemMonitor will update the UI for the character's items.
    /// </summary>
    public class SlotItemMonitor : ItemMonitor
    {
        [Tooltip("A reference to the text used for primary ItemType count.")]
        [SerializeField] protected Shared.UI.Text m_PrimaryCount;
        [Tooltip("The parent transform containing the count texts, can be disabled if there is no count.")]
        [SerializeField] protected GameObject m_CountParent;
        [Tooltip("A reference to the text used for the usable item loaded count.")]
        [SerializeField] protected Shared.UI.Text m_LoadedCount;
        [Tooltip("A reference to the text used for the usable item unloaded count.")]
        [SerializeField] protected Shared.UI.Text m_UnloadedCount;
        [Tooltip("The ID that UI represents.")]
        [SerializeField] protected int m_ID;
        [Tooltip("A reference to the image used for the item's icon.")]
        [SerializeField] protected Image m_ItemIcon;
        [Tooltip("The action ID that the UI represents.")]
        [SerializeField] protected int m_ItemActionID;

        public Image ItemIcon { get { return m_ItemIcon; } }

        [System.NonSerialized] private GameObject m_GameObject;

        private RectTransform m_ItemRectTransform;
        private CharacterItem m_EquippedCharacterItem;
        private IItemIdentifier m_ConsumableItemIdentifier;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            if (m_ItemIcon == null) {
                m_ItemIcon = GetComponent<Image>();
            }
            m_ItemRectTransform = m_ItemIcon.GetComponent<RectTransform>();
            m_ItemIcon.sprite = null;
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
                EventHandler.UnregisterEvent<CharacterItem, int>(m_Character, "OnInventoryRemoveItem", OnRemoveItem);
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
            EventHandler.RegisterEvent<CharacterItem, int>(m_Character, "OnInventoryRemoveItem", OnRemoveItem);
            // An item may already be equipped.
            for (int i = 0; i < m_CharacterInventory.SlotCount; ++i) {
                var item = m_CharacterInventory.GetActiveCharacterItem(i);
                if (item != null) {
                    OnEquipItem(item, i);
                }
            }
        }

        /// <summary>
        /// An ItemIdentifier has been picked up within the inventory.
        /// </summary>
        /// <param name="itemIdentifier">The ItemIdentifier that has been picked up.</param>
        /// <param name="amount">The amount of item picked up.</param>
        /// <param name="immediatePickup">Was the item be picked up immediately?</param>
        /// <param name="forceEquip">Should the item be force equipped?</param>
        protected override void OnPickupItemIdentifier(IItemIdentifier itemIdentifier, int amount, bool immediatePickup, bool forceEquip)
        {
            if (itemIdentifier != m_ConsumableItemIdentifier) {
                return;
            }

            var countString = m_CharacterInventory.GetItemIdentifierAmount(m_ConsumableItemIdentifier).ToString();
            if (m_PrimaryCount.enabled) {
                m_PrimaryCount.text = countString;
            } else {
                m_UnloadedCount.text = countString;
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
            
            m_ItemIcon.sprite = characterItem.Icon;
            UnityEngineUtility.SizeSprite(m_ItemIcon.sprite, m_ItemRectTransform);
            m_GameObject.SetActive(CanShowUI());

            // Multiple item actions can be attached to the same item.
            CharacterItemAction itemAction = null;
            if (m_ItemActionID < characterItem.ItemActions.Length) {
                itemAction = characterItem.ItemActions[m_ItemActionID];
            }

            if (itemAction == null) {
                DisableCountText();
                return;
            }

            var slotItemMonitorModule = itemAction.GetFirstActiveModule<IModuleSlotItemMonitor>();
            if (slotItemMonitorModule == null) {
                DisableCountText();
                return;
            }

            var hasLoadedCount = slotItemMonitorModule.TryGetLoadedCount(out var loadedCount);
            var hasUnloadedCount = slotItemMonitorModule.TryGetUnLoadedCount(out var unloadedCount);
            var hasItemIcon = slotItemMonitorModule.TryGetItemIcon(out var itemIcon);

            if (hasLoadedCount || hasUnloadedCount) {

                if (hasLoadedCount && hasUnloadedCount) {
                    m_LoadedCount.text = loadedCount.ToString();
                    m_UnloadedCount.text = unloadedCount.ToString();
                    m_LoadedCount.enabled = m_UnloadedCount.enabled = true;
                    m_PrimaryCount.enabled = false;
                } else {
                    m_PrimaryCount.text = (hasLoadedCount ? loadedCount : unloadedCount).ToString();
                    m_PrimaryCount.enabled = true;
                    m_LoadedCount.enabled = m_UnloadedCount.enabled = false;
                }
                
                
                if (m_CountParent != null) {
                    m_CountParent.SetActive(true);
                }
            }else {
                DisableCountText();
            }
        }

        /// <summary>
        /// Disables the text objects.
        /// </summary>
        private void DisableCountText()
        {
            if (m_CountParent != null) {
                m_CountParent.SetActive(false);
            }
            if (m_PrimaryCount.gameObject != null) {
                m_PrimaryCount.enabled = false;
            }
            if (m_LoadedCount.gameObject != null) {
                m_LoadedCount.enabled = false;
            }
            if (m_UnloadedCount.gameObject != null) {
                m_UnloadedCount.enabled = false;
            }
        }

        /// <summary>
        /// The DominantItem field has been updated for the specified item.
        /// </summary>
        /// <param name="characterItem">The Item whose DominantItem field was updated.</param>
        /// <param name="dominantItem">True if the item is now a dominant item.</param>
        protected override void OnUpdateDominantItem(CharacterItem characterItem, bool dominantItem)
        {
            if ((m_EquippedCharacterItem != null && characterItem != m_EquippedCharacterItem) || m_CharacterInventory.GetItemIdentifierAmount(characterItem.ItemIdentifier) == 0 || m_CharacterInventory.GetActiveCharacterItem(characterItem.SlotID) != characterItem) {
                return;
            }

            if (characterItem.DominantItem) {
                OnEquipItem(characterItem, characterItem.SlotID);
            } else {
                ResetMonitor();
            }
        }

        /// <summary>
        /// An item has been removed.
        /// </summary>
        /// <param name="characterItem">The unequipped item.</param>
        /// <param name="slotID">The slot that the item previously occupied.</param>
        private void OnRemoveItem(CharacterItem characterItem, int slotID)
        {
            TryResetMonitor(characterItem, slotID);
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="characterItem">The unequipped item.</param>
        /// <param name="slotID">The slot that the item previously occupied.</param>
        private void OnUnequipItem(CharacterItem characterItem, int slotID)
        {
            TryResetMonitor(characterItem, slotID);
        }

        private void TryResetMonitor(CharacterItem characterItem, int slotID)
        {
            CharacterItem equippedCharacterItem = null;
            if (!characterItem.DominantItem || characterItem.UIMonitorID != m_ID ||
                ((equippedCharacterItem = m_CharacterInventory.GetActiveCharacterItem(slotID)) != null &&
                 equippedCharacterItem.DominantItem && equippedCharacterItem != characterItem)) { return; }

            ResetMonitor();
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected override bool CanShowUI()
        {
            return base.CanShowUI() && m_EquippedCharacterItem != null && m_ItemIcon.sprite != null;
        }

        /// <summary>
        /// Resets the monitor back to the default state.
        /// </summary>
        private void ResetMonitor()
        {
            m_EquippedCharacterItem = null;
            OnMonitoredCharacterItemChanged(m_EquippedCharacterItem);
            m_ConsumableItemIdentifier = null;

            if (m_GameObject != null) {
                m_GameObject.SetActive(false);
            }
        }
    }
}
