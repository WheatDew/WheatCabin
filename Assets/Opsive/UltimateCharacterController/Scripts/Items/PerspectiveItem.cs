/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Events;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Objects;
    using UnityEngine;

    /// <summary>
    /// Component which represents the item object actually rendererd. The main responsibility is to determine the location that the object should be rendered at.
    /// </summary>
    public abstract class PerspectiveItem : StateBehavior
    {
        [Tooltip("The GameObject of the object rendered. Can be a prefab or a GameObject that is a child of the character.")]
        [SerializeField] protected GameObject m_Object;
        [Tooltip("The parent that the object spawns under. This parent should be a child of the ItemSlot.")]
        [SerializeField] protected IDObject<Transform> m_SpawnParent;
        [Tooltip("If Object is a prefab, specifies the local position of the spawned object.")]
        [SerializeField] protected Vector3 m_LocalSpawnPosition;
        [Tooltip("If Object is a prefab, specifies the local rotation of the spawned object.")]
        [SerializeField] protected Vector3 m_LocalSpawnRotation;
        [Tooltip("If Object is a prefab, specifies the local scale of the spawned object.")]
        [SerializeField] protected Vector3 m_LocalSpawnScale = Vector3.one;

        [NonSerialized] public GameObject Object { get { return m_Object; } set { m_Object = value; } }
        [NonSerialized] public Vector3 LocalSpawnPosition { get { return m_LocalSpawnPosition; } set { m_LocalSpawnPosition = value; } }
        [NonSerialized] public Vector3 LocalSpawnRotation { get { return m_LocalSpawnRotation; } set { m_LocalSpawnRotation = value; } }
        [NonSerialized] public Vector3 LocalSpawnScale { get { return m_LocalSpawnScale; } set { m_LocalSpawnScale = value; } }

        public abstract bool FirstPersonItem { get; }

        protected bool m_Initialized;
        protected GameObject m_Character;
        protected CharacterItem m_CharacterItem;
        protected GameObject m_CharacterModel;

        /// <summary>
        /// Initialize the perspective item.
        /// </summary>
        /// <param name="character">The character GameObject that the item is parented to.</param>
        /// <returns>True if the item was initialized successfully.</returns>
        public virtual bool Initialize(GameObject character)
        {
            if (m_Initialized) { return true; }

            m_Character = character;
            var modelManager = m_Character.GetCachedComponent<ModelManager>();
            if (modelManager != null) {
                m_CharacterModel = modelManager.ActiveModel;
            } else {
                m_CharacterModel = m_Character;
            }
            if (m_Object != null) {
                var item = m_Object.GetComponentInParent<CharacterItem>(true);
                // If the item is not null then it is being spawned at runtime.
                if (item != null) {
                    var localScale = m_Object.transform.localScale;
                    var parent = GetSpawnParent(m_CharacterModel, item.SlotID, false);
                    if (parent == null) {
                        return false;
                    }
                    m_Object.transform.parent = parent;
                    m_Object.transform.localScale = localScale;
                    m_Object.transform.localPosition = m_LocalSpawnPosition;
                    m_Object.transform.localRotation = Quaternion.Euler(m_LocalSpawnRotation);
                    m_Object.transform.localScale = m_LocalSpawnScale;
                }

                // Layer sanity check.
                if (m_Object.layer == m_Character.layer) {
                    Debug.LogWarning($"Warning: The item {name} has the same layer as the character. This will likely cause collision problems and should be changed.");
                }
            }
            m_CharacterItem = gameObject.GetCachedComponent<CharacterItem>();

            EventHandler.RegisterEvent<GameObject>(m_Character, "OnCharacterSwitchModels", OnCharacterSwitchModels);

            m_Initialized = true;
            return true;
        }

        /// <summary>
        /// Returns the parent that the VisibleItem object should spawn at.
        /// </summary>
        /// <param name="character">The character that the item should spawn under.</param>
        /// <param name="slotID">The character slot that the VisibleItem object should spawn under.</param>
        /// <param name="parentToItemSlotID">Should the object be parented to the item slot ID?</param>
        /// <returns>The parent that the VisibleItem object should spawn at.</returns>
        protected abstract Transform GetSpawnParent(GameObject character, int slotID, bool parentToItemSlotID);

        /// <summary>
        /// Starts the perspective item. Will be called after the item has been started.
        /// </summary>
        public virtual void ItemStarted() { }

        /// <summary>
        /// Is the VisibleItem active?
        /// </summary>
        /// <returns>True if the VisibleItem is active.</returns>
        public virtual bool IsActive() { return m_Object.activeSelf; }

        /// <summary>
        /// Activates or deactivates the VisibleItem.
        /// </summary>
        /// <param name="active">Should the VisibleItem be activated?</param>
        /// <param name="hasItem">Does the inventory contain the item?</param>
        public void SetActive(bool active, bool hasItem) { SetActive(active, hasItem, true); }

        /// <summary>
        /// Activates or deactivates the VisibleItem.
        /// </summary>
        /// <param name="active">Should the VisibleItem be activated?</param>
        /// <param name="hasItem">Does the inventory contain the item?</param>
        /// <param name="setIKTargets">Should the IK targets be set?</param>
        public virtual void SetActive(bool active, bool hasItem, bool setIKTargets) { m_Object.SetActive(active); }

        /// <summary>
        /// Returns the current VisibleItem object.
        /// </summary>
        /// <returns>The current VisibleItem object.</returns>
        public virtual GameObject GetVisibleObject() { return m_Object; }

        /// <summary>
        /// The VisibleItem has been picked up by the character.
        /// </summary>
        public virtual void Pickup() { }

        /// <summary>
        /// The item has started to be equipped.
        /// </summary>
        /// <param name="immediateEquip">Is the item being equipped immediately? Immediate equips will occur from the default loadout or quickly switching to the item.</param>
        public virtual void StartEquip(bool immediateEquip) { }

        /// <summary>
        /// Moves the item according to the horizontal and vertical movement, as well as the character velocity.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="verticalMovement">-1 to 1 value specifying the amount of vertical movement.</param>
        public virtual void Move(float horizontalMovement, float verticalMovement) { }

        /// <summary>
        /// The item has started to be unequipped.
        /// </summary>
        public virtual void StartUnequip() { }

        /// <summary>
        /// The item has been unequipped.
        /// </summary>
        public virtual void Unequip() { }

        /// <summary>
        /// The item has been removed.
        /// </summary>
        public virtual void Remove()
        {
            if (m_Object != null) {
                m_Object.SetActive(false);
            }
        }

        /// <summary>
        /// Resets the PerspectiveItem back to the initial values.
        /// </summary>
        public virtual void ResetInitialization()
        {
            if (!m_Initialized) { 
                return;
            }
            m_Initialized = false;

            var visibleObject = GetVisibleObject();
            if (visibleObject != null) {
                visibleObject.transform.SetParent(m_CharacterItem.transform);
            }
        }

        /// <summary>
        /// The character's model has switched.
        /// </summary>
        /// <param name="activeModel">The active character model.</param>
        public abstract void OnCharacterSwitchModels(GameObject activeModel);

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_Character != null) {
                EventHandler.UnregisterEvent<GameObject>(m_Character, "OnCharacterSwitchModels", OnCharacterSwitchModels);
            }
        }
    }
}