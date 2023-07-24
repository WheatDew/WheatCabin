/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.FirstPersonController.Character
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Utility;
    using Opsive.UltimateCharacterController.FirstPersonController.Character.Identifiers;
    using Opsive.UltimateCharacterController.FirstPersonController.Items;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Manages the location of the first person objects while in first or third person view.
    /// </summary>
    public class FirstPersonObjects : StateBehavior
    {
        [Tooltip("The minimum and maximum pitch angle (in degrees).")]
        [MinMaxRange(-90, 90)] [SerializeField] protected MinMaxFloat m_PitchLimit = new MinMaxFloat(-90, 90);
        [Tooltip("Should the object's pitch be locked to the character's rotation?")]
        [SerializeField] protected bool m_LockPitch;
        [Tooltip("The minimum and maximum yaw angle (in degrees).")]
        [MinMaxRange(-180, 180)] [SerializeField] protected MinMaxFloat m_YawLimit = new MinMaxFloat(-180, 180);
        [Tooltip("Should the object's yaw be locked to the character's rotation?")]
        [SerializeField] protected bool m_LockYaw;
        [Tooltip("Should the object rotate with a change in crosshairs rotation?")]
        [SerializeField] protected bool m_RotateWithCrosshairs = true;
        [Tooltip("The speed at which the object rotates towards the target position.")]
        [SerializeField] protected float m_RotationSpeed = 15;
        [Tooltip("Should the objects be positioned according to the target position of the camera ignorning the look offset?")]
        [SerializeField] protected bool m_IgnorePositionalLookOffset;
        [Tooltip("If ignoring the look offset, specifies the offset from the target position that the first person objects should move towards.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("If ignoring the look offset, specifies the speed that the first person objects should move towards the target position.")]
        [SerializeField] protected float m_MoveSpeed;

        public MinMaxFloat PitchLimit
        {
            get => m_PitchLimit;
            set
            {
                m_PitchLimit = value;
                if (Application.isPlaying) {
                    enabled = IsActive();
                    if (m_LockPitch) { UpdateLockedPitchAngle(); }
                }
            }
        }
        public bool LockPitch {
            get => m_LockPitch;
            set
            {
                m_LockPitch = value;
                if (Application.isPlaying) {
                    enabled = IsActive();
                    if (m_LockPitch) { UpdateLockedPitchAngle(); }
                }
            }
        }
        public MinMaxFloat YawLimit
        {
            get => m_YawLimit;
            set
            {
                m_YawLimit = value;
                if (Application.isPlaying) {
                    enabled = IsActive();
                    if (m_LockYaw) { UpdateLockedYawAngle(); }
                }
            }
        }
        public bool LockYaw
        {
            get => m_LockYaw;
            set
            {
                m_LockYaw = value;
                if (Application.isPlaying) {
                    if (m_LockYaw) { UpdateLockedYawAngle(); }
                    enabled = IsActive();
                }
            }
        }
        public bool RotateWithCrosshairs
        {
            get => m_RotateWithCrosshairs;
            set
            {
                m_RotateWithCrosshairs = value;
                if (Application.isPlaying) {
                    enabled = IsActive();
                }
            }
        }
        public float RotationSpeed { get => m_RotationSpeed; set => m_RotationSpeed = value; }
        public bool IgnorePositionalLookOffset { get => m_IgnorePositionalLookOffset; set { m_IgnorePositionalLookOffset = value; if (Application.isPlaying) { enabled = IsActive(); } } }
        public Vector3 PositionOffset { get => m_PositionOffset; set => m_PositionOffset = value; }
        public float MoveSpeed { get => m_MoveSpeed; set => m_MoveSpeed = value; }

        private Transform m_Transform;
        [System.NonSerialized] private GameObject m_GameObject;
        private GameObject m_Character;
        private Transform m_CharacterTransform;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private GameObject m_CharacterModel;
        private CameraController m_CameraController;
        private Transform m_CameraTransform;
        private GameObject[] m_FirstPersonBaseObjects;
        private HashSet<GameObject> m_ShouldActivateObject = new HashSet<GameObject>();
        private Dictionary<CharacterItem, GameObject[]> m_ItemBaseObjectMap = new Dictionary<CharacterItem, GameObject[]>();
        private CharacterItem[] m_EquippedItems;

        private float m_Pitch;
        private float m_Yaw;
        public GameObject Character => m_Character;
        public GameObject CharacterModel => m_CharacterModel;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_GameObject = gameObject;
            m_Transform = transform;
            m_CharacterLocomotion = gameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
            m_CharacterTransform = m_CharacterLocomotion.transform;
            m_Character = m_CharacterTransform.gameObject;
            var characterAnimatorMonitor = gameObject.GetCachedParentComponent<AnimatorMonitor>();
            if (characterAnimatorMonitor != null) {
                m_CharacterModel = characterAnimatorMonitor.gameObject;
            } else {
                m_CharacterModel = m_Character;
            }
            var baseObjects = GetComponentsInChildren<FirstPersonBaseObject>();
            var count = 0;
            m_FirstPersonBaseObjects = new GameObject[baseObjects.Length];
            for (int i = 0; i < baseObjects.Length; ++i) {
                if (baseObjects[i].AlwaysActive) {
                    continue;
                }
                m_FirstPersonBaseObjects[count] = baseObjects[i].gameObject;
                m_FirstPersonBaseObjects[count].SetActive(false);
                count++;
            }
            if (count != baseObjects.Length) {
                System.Array.Resize(ref m_FirstPersonBaseObjects, count);
            }
            var inventory = m_Character.GetCachedComponent<Inventory.InventoryBase>();
            m_EquippedItems = new CharacterItem[inventory != null ? inventory.SlotCount : 0];

            StateManager.LinkGameObjects(m_CharacterLocomotion.gameObject, m_GameObject, true);

            EventHandler.RegisterEvent<CameraController>(m_Character, "OnCharacterAttachCamera", OnAttachCamera);
            EventHandler.RegisterEvent<CharacterItem>(m_Character, "OnInventoryAddItem", OnAddItem);
            EventHandler.RegisterEvent<CharacterItem>(m_Character, "OnFirstPersonPerspectiveItemStartEquip", OnStartEquipItem);
            EventHandler.RegisterEvent<CharacterItem>(m_Character, "OnFirstPersonPerspectiveItemUnequip", OnUnequipItem);
            EventHandler.RegisterEvent<GameObject>(m_Character, "OnCharacterSwitchModels", OnSwitchModels);
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.RegisterEvent(m_Character, "OnRespawn", OnRespawn);
            EventHandler.RegisterEvent<bool>(m_Character, "OnCharacterActivate", OnActivate);
            EventHandler.RegisterEvent(m_Character, "OnCharacterDestroyed", OnCharacterDestroyed);

            enabled = false;
            
            // Some items might have already been added before this component is initialized.
            if (inventory != null) {
                var allCharacterItems = inventory.GetAllCharacterItems();
                for (int i = 0; i < allCharacterItems.Count; i++) {
                    OnAddItem(allCharacterItems[i]);
                }
            }
        }

        /// <summary>
        /// Deactivates the GameObject if the FirstPersonObjects don't belong to the active character model.
        /// </summary>
        private void Start()
        {
            var modelManager = m_Character.GetComponent<ModelManager>();
            if (modelManager != null && modelManager.ActiveModel != m_CharacterModel) {
                m_GameObject.SetActive(false);
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // Remote players should never see the first person objects.
            var networkInfo = m_Character.GetComponentInParent<Shared.Networking.INetworkInfo>();
            if (networkInfo != null && !networkInfo.IsLocalPlayer()) {
                m_GameObject.SetActive(false);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterActivate", OnActivate);
            }
#endif
        }

        /// <summary>
        /// The character has been attached to the camera. Initialze the camera-related values.
        /// </summary>
        /// <param name="cameraController">The camera controller attached to the character. Can be null.</param>
        private void OnAttachCamera(CameraController cameraController)
        {
            m_CameraController = cameraController;
            m_CameraTransform = (m_CameraController != null ? m_CameraController.Transform : null);
            // Delay the parent being set to prevent the transform from changing when the application is shutting down.
            Scheduler.Schedule(0.001f, () => { 
                m_Transform.parent = (m_CameraController != null ? m_CameraTransform : (m_CharacterTransform != null ? m_CharacterTransform.parent : null));
                m_Transform.localPosition = Vector3.zero;
                m_Transform.localRotation = Quaternion.identity;
            });
            m_Pitch = m_Yaw = 0;
            enabled = IsActive();
        }

        /// <summary>
        /// Is the component active?
        /// </summary>
        /// <returns>True if the component is active.</returns>
        private bool IsActive()
        {
            // The component should be active if any values can update the rotation.
            return m_CameraTransform != null && (Mathf.Abs(m_PitchLimit.MinValue - m_PitchLimit.MaxValue) < 180 || m_LockPitch || 
                Mathf.Abs(m_YawLimit.MinValue - m_YawLimit.MaxValue) < 360 || m_LockYaw || m_RotateWithCrosshairs || m_IgnorePositionalLookOffset || 
                m_Transform.localPosition != Vector3.zero);
        }

        /// <summary>
        /// Updates the internal pitch angle while ensuring it is within the pitch limits.
        /// </summary>
        private void UpdateLockedPitchAngle()
        {
            if (!enabled) {
                return;
            }

            var localRotation = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_CameraTransform.rotation).eulerAngles;
            if (Mathf.Abs(m_PitchLimit.MinValue - m_PitchLimit.MaxValue) < 180) {
                m_Pitch = MathUtility.ClampAngle(localRotation.x, m_PitchLimit.MinValue, m_PitchLimit.MaxValue);
            } else {
                m_Pitch = localRotation.x;
            }
        }

        /// <summary>
        /// Updates the internal yaw angle while ensuring it is within the yaw limits.
        /// </summary>
        private void UpdateLockedYawAngle()
        {
            if (!enabled) {
                return;
            }

            var localRotation = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_CameraTransform.rotation).eulerAngles;
            if (Mathf.Abs(m_YawLimit.MinValue - m_YawLimit.MaxValue) < 360) {
                m_Yaw = MathUtility.ClampAngle(localRotation.y, m_YawLimit.MinValue, m_YawLimit.MaxValue);
            } else {
                m_Yaw = localRotation.y;
            }
        }

        /// <summary>
        /// Adjusts the location of the transform according to the enabled toggles.
        /// </summary>
        private void LateUpdate()
        {
            var localRotation = MathUtility.InverseTransformQuaternion(m_CharacterTransform.rotation, m_CameraTransform.rotation).eulerAngles;
            if (m_LockPitch) {
                localRotation.x = m_Pitch;
            } else if (Mathf.Abs(m_PitchLimit.MinValue - m_PitchLimit.MaxValue) < 180) {
                localRotation.x = MathUtility.ClampAngle(localRotation.x, m_PitchLimit.MinValue, m_PitchLimit.MaxValue);
            }
            if (m_LockYaw) {
                localRotation.y = m_Yaw;
            } else if (Mathf.Abs(m_YawLimit.MinValue - m_YawLimit.MaxValue) < 360) {
                localRotation.y = MathUtility.ClampAngle(localRotation.y, m_YawLimit.MinValue, m_YawLimit.MaxValue);
            }
            var rotation = MathUtility.TransformQuaternion(m_CharacterTransform.rotation, Quaternion.Euler(localRotation));
            if (m_RotateWithCrosshairs) {
                rotation = m_CameraController.GetCrosshairsDeltaRotation() * rotation;
            }
            m_Transform.rotation = Quaternion.Slerp(m_Transform.rotation, rotation, m_RotationSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime);

            if (m_IgnorePositionalLookOffset && m_CameraController.ActiveViewType is FirstPersonController.Camera.ViewTypes.FirstPerson) {
                var firstPersonViewType = m_CameraController.ActiveViewType as FirstPersonController.Camera.ViewTypes.FirstPerson;
                var targetPosition = firstPersonViewType.GetTargetPosition() + m_CharacterTransform.TransformDirection(m_PositionOffset);
                m_Transform.position = Vector3.MoveTowards(m_Transform.position, targetPosition, m_MoveSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime);
            } else if (m_Transform.localPosition != Vector3.zero) {
                m_Transform.localPosition = Vector3.MoveTowards(m_Transform.localPosition, Vector3.zero, m_MoveSpeed * m_CharacterLocomotion.TimeScale * Time.timeScale * Time.deltaTime);
            }
        }

        /// <summary>
        /// The inventory has added the specified item.
        /// </summary>
        /// <param name="characterItem">The item that was added.</param>
        private void OnAddItem(CharacterItem characterItem)
        {
            if (m_ItemBaseObjectMap.ContainsKey(characterItem)) {
                return;
            }

            var firstPersonPerspective = characterItem.gameObject.GetCachedComponent<Items.FirstPersonPerspectiveItem>();
            RefreshItemBaseObjectMap(characterItem, firstPersonPerspective);
        }

        /// <summary>
        /// Update the item base Object map when something changes in the first person perspective.
        /// </summary>
        /// <param name="characterItem">The character item that is changing.</param>
        /// <param name="firstPersonPerspective">The first person perspective.</param>
        public virtual void RefreshItemBaseObjectMap(CharacterItem characterItem, FirstPersonPerspectiveItem firstPersonPerspective)
        {
            if (firstPersonPerspective != null && firstPersonPerspective.Object != null) {
                // If the item contains a first person object then the item will have a parent FirstPersonBaseObject. This object should be enabled/disabled depending on
                // the item active status.
                var firstPersonBaseObject = firstPersonPerspective.Object.transform.GetComponentInParentIncludeInactive<FirstPersonBaseObject>();
                // A base object may not exist in VR.
                if (firstPersonBaseObject == null) { return; }

                if (m_ItemBaseObjectMap.TryGetValue(characterItem, out var baseObjects) == false 
                    || baseObjects == null 
                    || (firstPersonPerspective.AdditionalControlObjectsTransforms != null && baseObjects.Length != firstPersonPerspective.AdditionalControlObjectsTransforms.Length + 1)) {
                    baseObjects = new GameObject[firstPersonPerspective.AdditionalControlObjectsTransforms.Length + 1];
                }
                
                baseObjects[0] = firstPersonBaseObject.gameObject; // Element 0 will always be the FirstPersonBaseObject from the FirstPersonPerspectiveItem.
                if (firstPersonPerspective.AdditionalControlObjectsTransforms != null) {
                    for (int i = 0; i < firstPersonPerspective.AdditionalControlObjectsTransforms.Length; ++i) {
                        baseObjects[i + 1] = firstPersonPerspective.AdditionalControlObjectsTransforms[i].gameObject;
                    }
                }

                m_ItemBaseObjectMap[characterItem] = baseObjects;
            }
        }

        /// <summary>
        /// The FirstPersonPerspectiveItem.Object value has been updated on the specified Item.
        /// </summary>
        /// <param name="characterItem">The Item that has been updated.</param>
        public void ItemObjectUpdated(CharacterItem characterItem)
        {
            if (!m_ItemBaseObjectMap.TryGetValue(characterItem, out var baseObjects)) {
                return;
            }

            var firstPersonPerspective = characterItem.gameObject.GetCachedComponent<Items.FirstPersonPerspectiveItem>();
            if (firstPersonPerspective == null || firstPersonPerspective.Object == null) {
                return;
            }

            var firstPersonBaseObject = firstPersonPerspective.Object.transform.GetComponentInParentIncludeInactive<FirstPersonBaseObject>();
            if (firstPersonBaseObject == null) {
                return;
            }
            
            RefreshItemBaseObjectMap(characterItem, firstPersonPerspective);
            CheckActiveBaseObjects();
        }

        /// <summary>
        /// The specified item will be equipped.
        /// </summary>
        /// <param name="characterItem">The item that will be equipped.</param>
        private void OnStartEquipItem(CharacterItem characterItem)
        {
            if (!characterItem.DominantItem) {
                return;
            }

            m_EquippedItems[characterItem.SlotID] = characterItem;

            CheckActiveBaseObjects();
        }

        /// <summary>
        /// An item has been unequipped.
        /// </summary>
        /// <param name="characterItem">The item that has been unequipped.</param>
        private void OnUnequipItem(CharacterItem characterItem)
        {
            if (characterItem != m_EquippedItems[characterItem.SlotID]) {
                return;
            }

            m_EquippedItems[characterItem.SlotID] = null;

            CheckActiveBaseObjects();
        }

        /// <summary>
        /// Loops through the base objects determining if it should be active.
        /// </summary>
        private void CheckActiveBaseObjects()
        {
            // Loop through the equipped items to determine which base objects should be activated.
            // Once the loop is complete do the activation/deactivation based on the equipped items.
            m_ShouldActivateObject.Clear();
            
            for (int i = 0; i < m_EquippedItems.Length; ++i) {
                if (m_EquippedItems[i] != null) {
                    if (!m_ItemBaseObjectMap.TryGetValue(m_EquippedItems[i], out var baseObjects)) {
                        Debug.LogError($"Error: Unable to find the base object for item {m_EquippedItems[i].name}. Ensure the item specifies a base object under the First Person Perspective Item component.");
                        continue;
                    }
                    for (int j = 0; j < baseObjects.Length; ++j) {
                        m_ShouldActivateObject.Add(baseObjects[j]);
                    }
                }
            }

            for (int i = 0; i < m_FirstPersonBaseObjects.Length; ++i) {
                m_FirstPersonBaseObjects[i].SetActive(m_ShouldActivateObject.Contains(m_FirstPersonBaseObjects[i]));
            }
        }

        /// <summary>
        /// The character's model has switched.
        /// </summary>
        /// <param name="activeModel">The active character model.</param>
        private void OnSwitchModels(GameObject activeModel)
        {
            m_GameObject.SetActive(m_CharacterModel == activeModel);
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            enabled = false;
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        private void OnRespawn()
        {
            enabled = m_CameraTransform != null && (m_LockPitch || m_LockYaw || m_RotateWithCrosshairs);
        }

        /// <summary>
        /// The character has been activated or deactivated.
        /// </summary>
        /// <param name="activate">Was the character activated?</param>
        private void OnActivate(bool activate)
        {
            m_GameObject.SetActive(activate);
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        private void OnCharacterDestroyed()
        {
            Destroy(m_GameObject);
        }

        /// <summary>
        /// The GameObject was destroyed. Unregister for any registered events.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<CameraController>(m_Character, "OnCharacterAttachCamera", OnAttachCamera);
            EventHandler.UnregisterEvent<CharacterItem>(m_Character, "OnInventoryAddItem", OnAddItem);
            EventHandler.UnregisterEvent<CharacterItem>(m_Character, "OnFirstPersonPerspectiveItemStartEquip", OnStartEquipItem);
            EventHandler.UnregisterEvent<CharacterItem>(m_Character, "OnFirstPersonPerspectiveItemUnequip", OnUnequipItem);
            EventHandler.UnregisterEvent<GameObject>(m_Character, "OnCharacterSwitchModels", OnSwitchModels);
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
            EventHandler.UnregisterEvent(m_Character, "OnRespawn", OnRespawn);
            EventHandler.UnregisterEvent<bool>(m_Character, "OnCharacterActivate", OnActivate);
            EventHandler.UnregisterEvent(m_Character, "OnCharacterDestroyed", OnCharacterDestroyed);
        }
    }
}