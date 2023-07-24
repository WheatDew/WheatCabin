/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character.Abilities
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Plays an animation which picks up the object.
    /// </summary>
    [DefaultStartType(AbilityStartType.ButtonDown)]
    [DefaultInputName("Action")]
    [DefaultAbilityIndex(11)]
    [DefaultObjectDetection(ObjectDetectionMode.Trigger)]
    [DefaultReequipSlots(false)]
    public class Pickup : DetectObjectAbilityBase
    {
        /// <summary>
        /// Specifies the type of objects that can be picked up.
        /// </summary>
        [System.Flags]
        public enum AllowedPickups
        {
            Item = 1,   // Item pickups.
            Health = 2, // Health packups.
            Other = 4,  // Any other object type.
        }

        [Tooltip("Specifies the type of objects that can be picked up.")]
        [SerializeField] protected AllowedPickups m_AllowedPickups = AllowedPickups.Item;
        [Tooltip("The slot ID to pick up. A value of -1 indicates any slot.")]
        [SerializeField] protected int m_SlotID = -1;
        [Tooltip("Specifies a list of ItemIdentifiers that should be picked up. If the list is empty any ItemIdentifier will trigger the animation.")]
        [SerializeField] protected ItemDefinitionBase[] m_PickupItemDefinitions;
        [Tooltip("Specifies if the ability should wait for the OnAnimatorPickup animation event or wait for the specified duration before picking up the item.")]
        [SerializeField] protected AnimationEventTrigger m_PickupEvent = new AnimationEventTrigger(true, 0.2f);
        [Tooltip("Specifies if the ability should wait for the OnAnimatorPickupComplete animation event or wait for the specified duration before stopping the ability.")]
        [SerializeField] protected AnimationEventTrigger m_PickupCompleteEvent = new AnimationEventTrigger(false, 0.4f);

        public AllowedPickups AllowedPickup { get { return m_AllowedPickups; } set { m_AllowedPickups = value; } }
        public int SlotID { get { return m_SlotID; } set { m_SlotID = value; } }
        public ItemDefinitionBase[] PickupItemDefinitions { get { return m_PickupItemDefinitions; } set { m_PickupItemDefinitions = value; } }
        public AnimationEventTrigger PickupEvent { get { return m_PickupEvent; } set { m_PickupEvent.CopyFrom(value); } }
        public AnimationEventTrigger PickupCompleteEvent { get { return m_PickupCompleteEvent; } set { m_PickupCompleteEvent.CopyFrom(value); } }

        public override bool CanReceiveMultipleStarts { get { return true; } }
        public override bool ImmediateStartItemVerifier { get { return true; } }
        public override string AbilityMessageText {
            get {
                var message = m_AbilityMessageText;
                if (m_AvailableItemPickupCount > 0) {
                    message = string.Format(message, m_AvailableItemPickups[0].PickupMessageText);
                }
                return message;
            }
            set { base.AbilityMessageText = value; }
        }

        private ItemPickupBase m_ItemPickup;
        private ItemPickupBase[] m_AvailableItemPickups;
        private int m_AvailableItemPickupCount;
        private EquipUnequip[] m_EquipUnequipAbilities;
        private ItemSetManagerBase m_ItemSetManager;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            m_AvailableItemPickups = new ItemPickupBase[m_MaxTriggerObjectCount];
            m_ItemSetManager = m_GameObject.GetCachedComponent<ItemSetManagerBase>();
            m_PickupEvent.RegisterUnregisterAnimationEvent(true, m_GameObject, "OnAnimatorPickup", DoPickup);
            m_PickupCompleteEvent.RegisterUnregisterAnimationEvent(true, m_GameObject, "OnAnimatorPickupComplete", PickupComplete);
        }

        /// <summary>
        /// Cache the abilities.
        /// </summary>
        public override void Start()
        {
            m_EquipUnequipAbilities = m_CharacterLocomotion.GetAbilities<EquipUnequip>();

            // Assume every ItemIdentifier can be picked up. The local mask should be set so the ability doesn't block the pickup with EquipUnequip.ShouldEquip.
            m_AllowEquippedSlotsMask = (1 << m_Inventory.SlotCount) - 1;
        }

        /// <summary>
        /// Returns true if the ItemPickup component should pickup the item.
        /// </summary>
        /// <returns>True if the ItemPickup component should pickup the item.</returns>
        public bool CanPickupItem()
        {
            // CanPickupItem will be called when a trigger is entered. The item can only be picked up if the start type is automatic.
            if (m_StartType != AbilityStartType.Automatic) {
                return false;
            }
            // Item pickups may not be allowed.
            if ((m_AllowedPickups & AllowedPickups.Item) == 0) {
                return false;
            }
            // The ItemPickup component should always pickup the item if the Ride ability is active.
            if (m_CharacterLocomotion.IsAbilityTypeActive<Ride>()) {
                return true;
            }
            // The ItemPickup component should always pickup the item if there are any active higher priority abilities active.
            for (int i = 0; i < m_CharacterLocomotion.ActiveAbilityCount; ++i) {
                if (m_CharacterLocomotion.ActiveAbilities[i].Index > Index) {
                    break;
                }
                if (!m_CharacterLocomotion.ActiveAbilities[i].IsConcurrent) {
                    return true;
                }
            }
            return !Enabled;
        }

        /// <summary>
        /// Validates the object to ensure it is valid for the current ability.
        /// </summary>
        /// <param name="obj">The object being validated.</param>
        /// <param name="raycastHit">The raycast hit of the detected object. Will be null for trigger detections.</param>
        /// <returns>True if the object is valid. The object may not be valid if it doesn't have an ability-specific component attached.</returns>
        protected override bool ValidateObject(GameObject obj, RaycastHit? raycastHit)
        {
            if (!base.ValidateObject(obj, raycastHit)) {
                return false;
            }

            if ((m_AllowedPickups & AllowedPickups.Item) != 0) {
                if (m_AvailableItemPickupCount > 0) {
                    for (int i = 0; i < m_AvailableItemPickupCount; ++i) {
                        if (obj == m_AvailableItemPickups[i].gameObject) {
                            return true;
                        }
                    }
                }

                ItemPickupBase itemPickup;
                if ((itemPickup = obj.GetCachedComponent<ItemPickupBase>()) != null && !itemPickup.PickupOnTriggerEnter && !itemPickup.IsDepleted) {
                    if (m_AvailableItemPickups.Length == m_AvailableItemPickupCount) {
                        Debug.LogWarning("Warning: Unable to pickup " + itemPickup.name + ". Ensure the MaxTriggerObject count is at least " +
                                                (m_AvailableItemPickupCount + 1) + " on the PickupItem ability.");
                        return false;
                    }
                    m_AvailableItemPickups[m_AvailableItemPickupCount] = itemPickup;
                    m_AvailableItemPickupCount++;
                    return true;
                }
            }

            if ((m_AllowedPickups & AllowedPickups.Health) != 0) {
                var healthpack = obj.GetCachedComponent<HealthPickup>();
                if (healthpack != null) {
                    return true;
                }
            }

            if ((m_AllowedPickups & AllowedPickups.Other) != 0) {
                var objectPickup = obj.GetCachedComponent<IObjectPickup>();
                if (objectPickup != null) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            if ((m_AllowedPickups & AllowedPickups.Item) != 0) {
                if (m_EquipUnequipAbilities == null) {
                    return false;
                }

                // An attribute may prevent the ability from starting.
                if (!base.CanStartAbility()) {
                    if (m_AvailableItemPickupCount > 0) {
                        for (int i = 0; i < m_AvailableItemPickupCount; ++i) {
                            m_AvailableItemPickups[i] = null;
                        }
                        m_AvailableItemPickupCount = 0;
                    }
                    return false;
                }

                if (m_AvailableItemPickupCount == 0) {
                    return false;
                }
            }

            return base.CanStartAbility();
        }
        
        /// <summary>
        /// The ability has started.
        /// </summary>
        protected override void AbilityStarted()
        {
            base.AbilityStarted();
            
            if ((m_AllowedPickups & AllowedPickups.Item) != 0 && m_AvailableItemPickupCount > 0 && !PrepareItemPickup()) {
                return;
            }
            
            // If the ability index is -1 then an animation will not play and the item should be picked up immediately.
            if (m_AbilityIndexParameter == -1) {
                DoPickup();
            } else {
                m_PickupEvent.WaitForEvent();
            }
        }

        /// <summary>
        /// Prepare the item pickup.
        /// </summary>
        /// <returns>Return false if the item does not need to be picked up.</returns>
        private bool PrepareItemPickup()
        {
            // If the item pickup isn't null then the ability is currently working on equipping another item.
            if (m_ItemPickup != null && m_ItemPickup != m_AvailableItemPickups[0]) {
                m_ItemPickup.DoItemIdentifierPickup(m_GameObject, m_Inventory, m_SlotID, true, false);
            }

            m_ItemPickup = m_AvailableItemPickups[0];
            m_AvailableItemPickupCount--;
            for (int i = 0; i < m_AvailableItemPickupCount; ++i) {
                m_AvailableItemPickups[i] = m_AvailableItemPickups[i + 1];
            }

            var itemDefinitionAmounts = m_ItemPickup.GetItemDefinitionAmounts();

            // If the PickupItemIdentifier array contains any ItemIdentifiers then the PickupItem ability should only start if the PickupItem object contains one of the ItemIdentifiers
            // within the array. If it doesn't contain the ItemIdentifier then that ItemIdentifier should be equipped as if the PickupItem ability doesn't exist.
            var immediatePickup = false;
            if (m_PickupItemDefinitions != null && m_PickupItemDefinitions.Length > 0) {
                immediatePickup = true;
                for (int i = 0; i < m_PickupItemDefinitions.Length; ++i) {
                    for (int j = 0; j < itemDefinitionAmounts.Length; ++j) {
                        if (m_PickupItemDefinitions[i] == itemDefinitionAmounts[j].ItemIdentifier.GetItemDefinition()) {
                            immediatePickup = false;
                            break;
                        }
                    }

                    if (immediatePickup) {
                        break;
                    }
                }
            }

            m_ItemPickup.DoItemPickup(m_GameObject, m_Inventory, m_SlotID, !immediatePickup, immediatePickup);

            // The ability shouldn't start if the ItemIdentifier has already been picked up.
            if (immediatePickup) {
                StopAbility();
                return false;
            }

            // Before the item can be picked up the currently equipped items need to be unequipped.
            for (int i = 0; i < m_EquipUnequipAbilities.Length; ++i) {
                m_EquipUnequipAbilities[i].WillStartPickup();
            }

            var allowedEquippedSlotsMask = 0;
            for (int i = 0; i < itemDefinitionAmounts.Length; ++i) {
                var itemDefinition = itemDefinitionAmounts[i].ItemDefinition;
                var itemIdentifier = itemDefinitionAmounts[i].ItemIdentifier;

                for (int j = 0; j < m_Inventory.SlotCount; ++j) {
                    // Determine if the item should be equipped. The current item needs to be unequipped if it doesn't match the item being picked up.
                    var shouldEquip = false;
                    for (int k = 0; k < m_EquipUnequipAbilities.Length; ++k) {
                        if (m_ItemSetManager.IsCategoryMember(itemDefinition,
                                m_EquipUnequipAbilities[k].ItemSetGroupIndex) &&
                            m_EquipUnequipAbilities[k].ShouldEquip(itemIdentifier, j, itemDefinitionAmounts[i].Amount)) {
                            shouldEquip = true;
                            break;
                        }
                    }

                    if (!shouldEquip) {
                        continue;
                    }

                    // The item should be equipped.
                    var equippedItem = m_Inventory.GetActiveCharacterItem(j);
                    if (equippedItem == null || itemIdentifier != equippedItem.ItemIdentifier) {
                        allowedEquippedSlotsMask |= (1 << j);
                    }

                    break;
                }
            }

            // If the item doesn't need to be equipped then it should still be picked up.
            if (allowedEquippedSlotsMask == 0 && (m_AllowedPickups & AllowedPickups.Other) == 0) {
                m_ItemPickup.DoItemPickup(m_GameObject, m_Inventory, m_SlotID, false, true);
                StopAbility();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Picks up the object.
        /// </summary>
        private void DoPickup()
        {

            m_PickupEvent.CancelWaitForEvent();

            if ((m_AllowedPickups & AllowedPickups.Item) != 0) {
                if (m_ItemPickup == null) {
                    return;
                }
                
                m_ItemPickup.DoItemIdentifierPickup(m_GameObject, m_Inventory, m_SlotID, true, true);
            } else {
                var objectPickup = m_DetectedObject.GetCachedComponent<IObjectPickup>();
                if (objectPickup == null) {
                    return;
                }

                objectPickup.DoPickup(m_GameObject);
            }

            m_PickupCompleteEvent.WaitForEvent();
        }

        /// <summary>
        /// Completes the ability.
        /// </summary>
        private void PickupComplete()
        {
            m_PickupCompleteEvent.CancelWaitForEvent();

            StopAbility();
        }

        /// <summary>
        /// The ability has stopped running.
        /// </summary>
        /// <param name="force">Was the ability force stopped?</param>
        protected override void AbilityStopped(bool force)
        {
            base.AbilityStopped(force, true);

            m_ItemPickup = null;
        }

        /// <summary>
        /// The character has exited a trigger.
        /// </summary>
        /// <param name="other">The GameObject that the character exited.</param>
        /// <returns>Returns true if the entered object leaves the trigger.</returns>
        protected override bool TriggerExit(GameObject other)
        {
            if (base.TriggerExit(other) && !IsActive) {
                var index = -1;
                for (int i = 0; i < m_AvailableItemPickupCount; ++i) {
                    if (m_AvailableItemPickups[i].gameObject == other) {
                        m_AvailableItemPickups[i] = null;
                        index = i;
                        break;
                    }
                }
                if (index != -1) {
                    m_AvailableItemPickupCount--;
                    for (int i = index; i < m_AvailableItemPickupCount; ++i) {
                        m_AvailableItemPickups[i] = m_AvailableItemPickups[i + 1];
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            m_PickupEvent.RegisterUnregisterAnimationEvent(false, m_GameObject, "OnAnimatorPickup", DoPickup);
            m_PickupCompleteEvent.RegisterUnregisterAnimationEvent(false, m_GameObject, "OnAnimatorPickupComplete", PickupComplete);
        }
    }
}