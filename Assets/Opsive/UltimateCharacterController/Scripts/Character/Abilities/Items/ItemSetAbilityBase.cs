/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities.Items
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;

    /// <summary>
    /// The ItemSetAbilityBase ability acts as a base class for common ItemSet operations such as equipping the previous or next item.
    /// </summary>
    public abstract class ItemSetAbilityBase : ItemAbility
    {
        [Tooltip("The category that the ability should respond to.")]
        [SerializeField] protected CategoryBase m_ItemCategory;
        [Tooltip("If the Use or Reload abilities are active should the ability not be able to start?")]
        [SerializeField] protected bool m_PreventStartUseReloadActive = true;

        protected uint m_ItemSetCategoryID;
        
        public uint ItemSetCategoryID
        {
            get
            {
                if (m_Initialized) {
                    return m_ItemSetCategoryID;
                }
                
                if (m_ItemCategory != null) {
                    m_ItemSetCategoryID = m_ItemCategory.ID;
                }
                
                return m_ItemSetCategoryID;
            }
        }
        
        public CategoryBase ItemCategory
        {
            get {return m_ItemCategory; }
            set
            {
                m_ItemCategory = value;
                if (m_ItemCategory == null) {
                    m_ItemSetCategoryID = 0;
                } else {
                    m_ItemSetCategoryID = m_ItemCategory.ID;
                }
            }
        }

        public bool PreventStartUseReloadActive { get { return m_PreventStartUseReloadActive; } set { m_PreventStartUseReloadActive = value; } }

        protected bool m_Initialized = false;
        protected EquipUnequip m_EquipUnequipItemAbility;
        protected ItemSetManagerBase m_ItemSetManager;
        protected int m_ItemSetGroupIndex;

        public int ItemSetGroupIndex { get { return m_ItemSetGroupIndex; } }

        /// <summary>
        /// Register for any interested events.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_Initialized = true;
            
            if (m_ItemCategory != null) {
                m_ItemSetCategoryID = m_ItemCategory.ID;
            }

            m_ItemSetManager = m_GameObject.GetCachedComponent<ItemSetManagerBase>();
            m_ItemSetManager.Initialize(false);
            // If the CategoryID is empty then the category hasn't been initialized. Use the first category index.
            if (RandomID.IsIDEmpty(m_ItemSetCategoryID) && m_ItemSetManager.ItemSetGroups.Length > 0) {
                m_ItemSetCategoryID = m_ItemSetManager.ItemSetGroups[0].CategoryID;
            }
            m_ItemSetGroupIndex = m_ItemSetManager.CategoryIDToIndex(m_ItemSetCategoryID);

            if (m_ItemSetGroupIndex < 0) {
                Debug.LogError($"The item ability '{this}' requires an Item Set Manager with at least one Item Set Group with a matching Item Category: '{m_ItemCategory}'.");
                return;
            }
            
            var equipUnequipAbilities = GetAbilities<EquipUnequip>();
            if (equipUnequipAbilities != null) {
                // The ItemSet CategoryID must match for the ToggleEquip ability to be able to use the EquipUnequip ability.
                for (int i = 0; i < equipUnequipAbilities.Length; ++i) {
                    if (equipUnequipAbilities[i].ItemSetCategoryID == m_ItemSetCategoryID) {
                        m_EquipUnequipItemAbility = equipUnequipAbilities[i];
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            // Use and Reload can prevent the ability from equipping or unequipping items.
            if (m_PreventStartUseReloadActive && (m_CharacterLocomotion.IsAbilityTypeActive<Use>() || m_CharacterLocomotion.IsAbilityTypeActive<Reload>())) {
                return false;
            }
            return base.CanStartAbility();
        }
    }
}