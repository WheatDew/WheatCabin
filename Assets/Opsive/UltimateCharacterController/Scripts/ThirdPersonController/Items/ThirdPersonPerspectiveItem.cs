/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Items
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Objects;
    using UnityEngine;

    /// <summary>
    /// Component which represents the item object actually rendererd.
    /// </summary>
    public class ThirdPersonPerspectiveItem : PerspectiveItem
    {
        [Tooltip("The location of the non-dominant hand which should be placed by the IK implementation.")]
        [SerializeField] protected Transform m_NonDominantHandIKTarget;
        [Tooltip("The location of the non-dominant hand hint which should be placed by the IK implementation.")]
        [SerializeField] protected Transform m_NonDominantHandIKTargetHint;
        [Tooltip("The transform that the item should be holstered to when unequipped.")]
        [SerializeField] protected IDObject<Transform> m_HolsterTarget;

        [NonSerialized] public Transform NonDominantHandIKTarget { get { return m_NonDominantHandIKTarget; } set { m_NonDominantHandIKTarget = value; } }
        [NonSerialized] public Transform NonDominantHandIKTargetHint { get { return m_NonDominantHandIKTargetHint; } set { m_NonDominantHandIKTargetHint = value; } }
        [NonSerialized] public Transform HolsterTarget { get { return m_HolsterTarget.GetObject(m_CharacterModel, true); } set { m_HolsterTarget.Obj = value; } }
        [NonSerialized] public IDObject<Transform> HolsterTargetIDObject { get { return m_HolsterTarget; } set { m_HolsterTarget = value; } }

        private CharacterIKBase m_CharacterIK;
        private Transform m_ParentBone;
        private Transform m_ObjectTransform;
        private Transform m_StartParentTransform;
        private Vector3 m_StartLocalPosition;
        private Quaternion m_StartLocalRotation;
        private bool m_PickedUp;

        public override bool FirstPersonItem { get { return false; } }

        /// <summary>
        /// Initialize the perspective item.
        /// </summary>
        /// <param name="character">The character GameObject that the item is parented to.</param>
        /// <returns>True if the item was initialized successfully.</returns>
        public override bool Initialize(GameObject character)
        {
            if (m_Initialized) { return true; }

            if (!base.Initialize(character)) {
                return false;
            }

            var modelManager = m_Character.GetCachedComponent<ModelManager>();
            if (modelManager == null) {
                m_CharacterIK = m_Character.GetComponentInChildren<CharacterIKBase>();
            } else {
                m_CharacterIK = modelManager.ActiveModel.GetCachedComponent<CharacterIKBase>();
            }

            if (m_Object != null) {
                m_ObjectTransform = m_Object.transform;
                m_StartParentTransform = m_ObjectTransform.parent; // Represents the Items GameObject.
                m_StartLocalPosition = m_ObjectTransform.localPosition;
                m_StartLocalRotation = m_ObjectTransform.localRotation;
                m_ParentBone = m_StartParentTransform.parent; // Represents the bone that the item is equipped to.
            }

            if (HolsterTarget != null) {
                // The holster target will be enabled when the item is picked up.
                HolsterTarget.gameObject.SetActive(false);
            }

            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);
            return true;
        }

        /// <summary>
        /// Returns the parent that the VisibleItem object should spawn at.
        /// </summary>
        /// <param name="character">The character that the item should spawn under.</param>
        /// <param name="slotID">The character slot that the VisibleItem object should spawn under.</param>
        /// <param name="parentToItemSlotID">Should the object be parented to the item slot ID?</param>
        /// <returns>The parent that the VisibleItem object should spawn at.</returns>
        protected override Transform GetSpawnParent(GameObject character, int slotID, bool parentToItemSlotID)
        {
            Transform itemSlotTransform = null;
            var itemSlots = character.GetComponentsInChildren<CharacterItemSlot>(true);
            for (int i = 0; i < itemSlots.Length; ++i) {
#if FIRST_PERSON_CONTROLLER
                if (itemSlots[i].GetComponentInParent<FirstPersonController.Character.Identifiers.FirstPersonBaseObject>(true) != null) {
                    continue;
                }
#endif
                
                if (itemSlots[i].ID == slotID) {
                    itemSlotTransform = itemSlots[i].transform;
                    break;
                }
            }

            if (itemSlotTransform == null) {
                Debug.LogError($"The Character Item Slot Transform with SlotID '{slotID}' could not be found in the Character hierarchy '{character}'", character);
                return null;
            }

            // Search for the spawn parent inside the ItemSlot, it is optional so it might not exist.
            // Force the search because the item might move to another model.
            if (m_SpawnParent != null && m_SpawnParent.TryGetObjectInChildren(itemSlotTransform.gameObject, out var spawnParent, true)) {
                if (spawnParent != null) {
                    return spawnParent;
                }
            }

            return itemSlotTransform;
        }

        /// <summary>
        /// Is the VisibleItem active?
        /// </summary>
        /// <returns>True if the VisibleItem is active.</returns>
        public override bool IsActive()
        {
            if (m_Object == null) {
                return m_CharacterItem.VisibleObjectActive;
            }
            // If a holster target is specified then the VisibleItem will never completely deactivate. Determine if it is active by the Transform parent.
            if (m_ObjectTransform.parent == HolsterTarget) {
                return false;
            } else {
                return base.IsActive();
            }
        }

        /// <summary>
        /// Activates or deactivates the VisibleItem.
        /// </summary>
        /// <param name="active">Should the VisibleItem be activated?</param>
        /// <param name="hasItem">Does the inventory contain the item?</param>
        /// <param name="setIKTargets">Should the IK targets be set?</param>
        public override void SetActive(bool active, bool hasItem, bool setIKTargets)
        {
            // If a holster target is specified then deactivating the VisibleItem will mean setting the parent transform of the object to that holster target.
            if (HolsterTarget != null && hasItem) {
                if (active) {
                    m_ObjectTransform.parent = m_StartParentTransform;
                    m_ObjectTransform.localPosition = m_StartLocalPosition;
                    m_ObjectTransform.localRotation = m_StartLocalRotation;
                    m_ObjectTransform.localScale = m_LocalSpawnScale;
                } else {
                    m_ObjectTransform.parent = HolsterTarget;
                    m_ObjectTransform.localPosition = Vector3.zero;
                    m_ObjectTransform.localRotation = Quaternion.identity;
                    m_ObjectTransform.localScale = m_LocalSpawnScale;
                }
                // If the item is holstered it should always be active when it exists in the inventory.
                if (m_Object != null) {
                    base.SetActive(true, true, setIKTargets);
                }
            } else if (m_Object != null) {
                // Allow the base object to activate or deactivate the actual object.
                base.SetActive(active, hasItem, setIKTargets);
            }

            // When the item activates or deactivates it should specify the IK target of the non-dominant hand (if any).
            if (m_CharacterIK != null && setIKTargets) {
                m_CharacterIK.SetItemIKTargets(active ? m_ObjectTransform : null, m_ParentBone, active ? NonDominantHandIKTarget : null, active ? NonDominantHandIKTargetHint : null);
            }
        }

        /// <summary>
        /// The VisibleItem has been picked up by the character.
        /// </summary>
        public override void Pickup()
        {
            
            base.Pickup();

            m_PickedUp = true;

            // The object should always be active if it is holstered.
            if (HolsterTarget != null) {
                HolsterTarget.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// The item has been removed.
        /// </summary>
        public override void Remove()
        {
            base.Remove();

            m_PickedUp = false;
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            // When the character dies disable the holster. Do not disable the item's object because that may not be activated again
            // when the character respawns, whereas the holster target should always be activated since it's an empty GameObject.
            if (HolsterTarget != null && m_PickedUp) {
                SetActive(false, true);
                HolsterTarget.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            if (HolsterTarget != null && m_PickedUp) {
                HolsterTarget.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Resets the PerspectiveItem back to the initial values.
        /// </summary>
        public override void ResetInitialization()
        {
            if (!m_Initialized) {
                return;
            }

            base.ResetInitialization();

            OnDestroy();
        }

        /// <summary>
        /// The character's model has switched.
        /// </summary>
        /// <param name="activeModel">The active character model.</param>
        public override void OnCharacterSwitchModels(GameObject activeModel)
        {
            var visibleObject = GetVisibleObject();
            if (visibleObject == null) {
                return;
            }

            m_CharacterModel = activeModel;
            m_CharacterIK = m_CharacterModel.GetCachedComponent<CharacterIKBase>();

            // Rest the values such that they can be retrieved from the new model
            m_HolsterTarget.ResetValue();
            
            // No need to change m_Object because it is the same object.
            m_StartParentTransform = GetSpawnParent(activeModel, m_CharacterItem.SlotID, false);
            
            // Reposition the item.
            m_ObjectTransform.parent = m_StartParentTransform;
            m_ObjectTransform.localPosition = m_StartLocalPosition;
            m_ObjectTransform.localRotation = m_StartLocalRotation;
            m_ObjectTransform.localScale = m_LocalSpawnScale;
            m_ParentBone = m_StartParentTransform.parent; // Represents the bone that the item is equipped to.
            m_CharacterItem.SetPerspectiveAnimatorMonitors(this);

            // Call set active to make sure that the IK or holster are positioned correctly.
            var hasItem = m_CharacterItem.HasItemInInventory();
            if (hasItem) {
                var active = m_CharacterItem.IsItemActiveInInventory();
                SetActive(active, hasItem, active);
            }
        }

        /// <summary>
        /// Called when the item is destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_Character == null) {
                return;
            }

            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
        }
    }
}