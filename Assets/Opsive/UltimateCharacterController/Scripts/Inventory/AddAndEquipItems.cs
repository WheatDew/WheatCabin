/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Inventory
{
    using Opsive.Shared.StateSystem;
    using UnityEngine;

    /// <summary>
    /// A simple component that allows adding and equipping items.
    /// </summary>
    public class AddAndEquipItems : MonoBehaviour
    {
        [Tooltip("The Inventory to add the items to.")]
        [SerializeField] protected InventoryBase m_Inventory;
        [Tooltip("The items to add to the ivnentory.")]
        [SerializeField] protected ItemIdentifierAmount[] m_ItemAmounts;
        [Tooltip("If an item set exist with the matching state name, that one will be equipped.")]
        [StateName][SerializeField] protected string m_ItemSetName;
        [Tooltip("Add and equip the items on start?")]
        [SerializeField] protected bool m_OnStart;
        [Tooltip("The Item Set group to add the items to.")]
        [SerializeField] protected int m_ItemSetGroup = -1;
        [Tooltip("Force equip the item set?")]
        [SerializeField] protected bool m_ForceEquip;
        [Tooltip("Immediatly equip the item or wait for the animation?")]
        [SerializeField] protected bool m_ImmediateEquip;

        protected ItemSetManager m_ItemSetManager;
        protected bool m_Initialized = false;
        
        /// <summary>
        /// Initialize the component.
        /// </summary>
        private void Start()
        {
            Initialize(false);

            if (!m_OnStart) { return; }

            DoAddAndEquipItems();
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        /// <param name="force">Force the initialization?</param>
        private void Initialize(bool force)
        {
            if (m_Initialized && !force) {
                return;
            }
            
            if (m_Inventory == null) { m_Inventory = GameObject.FindGameObjectWithTag("Player")?.GetComponent<InventoryBase>(); }

            if (m_Inventory == null) {
                return;
            }
            
            m_ItemSetManager = m_Inventory.GetComponent<ItemSetManager>();
            m_Initialized = true;
        }

        /// <summary>
        /// Do add and equip items specified in the fields.
        /// </summary>
        public void DoAddAndEquipItems()
        {
            Initialize(false);
            
            if (m_Inventory == null ||m_ItemSetManager == null) {
                return;
            }

            if (m_ItemSetManager.Initialized == false) {
                return;
            }
            
            for (int i = 0; i < m_ItemAmounts.Length; i++) {
                var itemIdentifierAmount = m_ItemAmounts[i];
                if (itemIdentifierAmount.ItemDefinition == null || itemIdentifierAmount.Amount == 0) {
                    continue;
                }

                m_Inventory.AddItemIdentifierAmount(itemIdentifierAmount.ItemIdentifier, itemIdentifierAmount.Amount);
            }
            
            m_ItemSetManager.UpdateItemSets();

            if (string.IsNullOrWhiteSpace(m_ItemSetName) == false) {
                if (m_ItemSetManager.TryEquipItemSet(m_ItemSetName, m_ItemSetGroup, m_ForceEquip, m_ImmediateEquip)) {
                    return;
                } else {
                    Debug.LogWarning($"Cannot equip item set '{m_ItemSetName}' it might be invalid, disabled or might not exist.");
                }
            }

            for (int i = 0; i < m_ItemAmounts.Length; i++) {
                var itemIdentifierAmount = m_ItemAmounts[i];
                for (int j = 0; j < m_Inventory.SlotCount; j++) {
                    var characterItem = m_Inventory.GetCharacterItem(itemIdentifierAmount.ItemIdentifier, j);
                    if(characterItem == null){ continue; }
                    
                    m_ItemSetManager.EquipItem(characterItem.ItemIdentifier, m_ItemSetGroup, m_ForceEquip, m_ImmediateEquip);
                }
                
            }
        }
    }
}