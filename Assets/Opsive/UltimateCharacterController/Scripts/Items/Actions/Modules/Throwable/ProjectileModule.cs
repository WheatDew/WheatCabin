/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// The Throwable Projectile data contains information about the projectile that was thrown.
    /// </summary>
    [Serializable]
    public class ThrowableProjectileData
    {
        public GameObject m_ProjectilePrefab;
        public ThrowableProjectileModule m_ProjectileModule;
        public ThrowableAmmoData m_AmmoData;
        public GameObject m_SpawnedProjectile;
        public bool m_WasPrespawnedProjectile;
        public TrajectoryObject m_SpawnedTrajectoryObject;

        public GameObject ProjectilePrefab { get => m_ProjectilePrefab; set => m_ProjectilePrefab = value; }
        public ThrowableProjectileModule ProjectileModule { get => m_ProjectileModule; set => m_ProjectileModule = value; }
        public ThrowableAmmoData AmmoData { get => m_AmmoData; set => m_AmmoData = value; }
        public GameObject SpawnedProjectile { get => m_SpawnedProjectile; set => m_SpawnedProjectile = value; }
        public bool WasPrespawnedProjectile { get => m_WasPrespawnedProjectile; set => m_WasPrespawnedProjectile = value; }
        public TrajectoryObject SpawnedTrajectoryObject { get => m_SpawnedTrajectoryObject; set => m_SpawnedTrajectoryObject = value; }
    }

    /// <summary>
    /// The base class for defining the projectile to be thrown by a the throwable action.
    /// </summary>
    [Serializable]
    public abstract class ThrowableProjectileModule : ThrowableActionModule,
        IModuleCanActivateVisibleObject, IModuleStartItemUse, IModuleItemUseComplete, IModuleStopItemUse,
        IModuleOnChangePerspectives
    {
        protected ThrowableProjectileData m_ThrowableProjectileData;

        public abstract TrajectoryObject InstantiatedTrajectoryObject { get; }
        public abstract GameObject InstantiatedThrownObject { get; }
        public abstract bool ObjectIsVisible { get; }
        
        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            CreateProjectileData();
        }
        
        /// <summary>
        /// Creat the projectile data.
        /// </summary>
        protected virtual void CreateProjectileData()
        {
            m_ThrowableProjectileData = new ThrowableProjectileData();
            m_ThrowableProjectileData.ProjectileModule = this;
        }
        
        /// <summary>
        /// Get the projectile data of the item to throw.
        /// </summary>
        /// <param name="dataStream">The throwable data stream.</param>
        /// <param name="throwPoint">The throw point.</param>
        /// <param name="throwDirection">The throw direction.</param>
        /// <param name="index">The projectile index.</param>
        /// <param name="remove">Should the item be removed when thrown?</param>
        /// <returns>The throwable projectile data.</returns>
        public abstract ThrowableProjectileData GetProjectileDataToThrow(ThrowableUseDataStream dataStream, Vector3 throwPoint, Vector3 throwDirection, int index, bool remove);
        
        /// <summary>
        /// Can the visible object be activated? An example of when it shouldn't be activated is when a grenade can be thrown but it is not the primary item
        /// so it shouldn't be thrown until after the throw action has started.
        /// </summary>
        /// <returns>True if the visible object can be activated.</returns>
        public abstract bool CanActivateVisibleObject();

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public abstract void StartItemUse(Use useAbility);

        /// <summary>
        /// The item has complete its use.
        /// </summary>
        public abstract void ItemUseComplete();

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public abstract void StopItemUse();

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="firstPersonPerspective">Changed to first person?</param>
        public abstract void OnChangePerspectives(bool firstPersonPerspective);
    }
    
    /// <summary>
    /// Spawns a projectile to preview the projectile before it is thrown.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceClassName : "SpawnedProjectile", sourceNamespace: "Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable")]
    public class SpawnProjectile : ThrowableProjectileModule
    {
        [Tooltip("The object that is thrown.")]
        [SerializeField] protected GameObject m_ThrownObject;
        [Tooltip("Should the visible object be disabled?")]
        [SerializeField] protected bool m_DisableVisibleObject;
        [Tooltip("The layer that the item should occupy when initially spawned.")]
        [Shared.Utility.Layer] [SerializeField] protected int m_StartLayer = LayerManager.IgnoreRaycast;
        [Tooltip("Specifies if the item should wait for the OnAnimatorActivateThrowableObject animation event or wait for the specified duration before activating the throwable object.")]
        [SerializeField] protected AnimationEventTrigger m_ActivateThrowableObjectEvent;

        public override TrajectoryObject InstantiatedTrajectoryObject => m_InstantiatedTrajectoryObject;
        public override GameObject InstantiatedThrownObject => m_InstantiatedThrownObject;

        public override bool ObjectIsVisible { get; }
        
        public GameObject ThrownObject { get { return m_ThrownObject; } set { m_ThrownObject = value; } }
        public bool DisableVisibleObject { get { return m_DisableVisibleObject; } set {
                m_DisableVisibleObject = value;
                if (CharacterItem != null) {
                    CharacterItem.SetVisibleObjectActive(CharacterItem.VisibleObjectActive, Inventory.GetItemIdentifierAmount(CharacterItem.ItemIdentifier) > 0);
                    EnableObjectMeshRenderers(CanActivateVisibleObject());
                }
            }
        }
        public AnimationEventTrigger ActivateThrowableObjectEvent { get { return m_ActivateThrowableObjectEvent; } set { m_ActivateThrowableObjectEvent.CopyFrom(value); } }
        public int StartLayer { get { return m_StartLayer; }
            set
            {
                m_StartLayer = value;
                if (m_InstantiatedThrownObject != null && !ThrowableAction.WasThrown) {
                    m_InstantiatedThrownObject.layer = m_StartLayer;
                }
            }
        }
        
        private GameObject m_Object;
        private Transform m_ObjectTransform;
        private Renderer[] m_FirstPersonObjectRenderers;
        private Renderer[] m_ThirdPersonObjectRenderers;
        private GameObject m_InstantiatedThrownObject;
        protected TrajectoryObject m_InstantiatedTrajectoryObject;
        private bool m_ActivateVisibleObject;
        protected Action m_OnReequipThrowableItemAction;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            
            var perspectiveItem = CharacterLocomotion.FirstPersonPerspective ? CharacterItem.FirstPersonPerspectiveItem : CharacterItem.ThirdPersonPerspectiveItem;
            m_Object = perspectiveItem.GetVisibleObject();
            m_ObjectTransform = m_Object.transform;
            var firstPersonPerspectiveItem = CharacterItem.FirstPersonPerspectiveItem;
            if (firstPersonPerspectiveItem != null) {
                var visibleObject = firstPersonPerspectiveItem.GetVisibleObject();
                if (visibleObject != null) {
                    m_FirstPersonObjectRenderers = visibleObject.GetComponentsInChildren<Renderer>(true);
                }
            }
            var thirdPersonPerspectiveItem = CharacterItem.ThirdPersonPerspectiveItem;
            if (thirdPersonPerspectiveItem != null) {
                var visibleObject = thirdPersonPerspectiveItem.GetVisibleObject();
                if (visibleObject != null) {
                    m_ThirdPersonObjectRenderers = visibleObject.GetComponentsInChildren<Renderer>(true);
                }
            }
            
            m_Initialized = true;
            EnableObjectMeshRenderers(CanActivateVisibleObject());
            m_OnReequipThrowableItemAction = OnReequipThrowableItem;
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);
            m_ActivateThrowableObjectEvent.RegisterUnregisterAnimationEvent(register, Character, "OnAnimatorActivateThrowableObject", ActivateThrowableObject);
            
            if (register) {
                ThrowableAction.OnReequipThrowableItemE += m_OnReequipThrowableItemAction;
            } else {
                ThrowableAction.OnReequipThrowableItemE -= m_OnReequipThrowableItemAction;
            }
            
            EnableObjectMeshRenderers(CanActivateVisibleObject());
        }

        /// <summary>
        /// Reequip the throwable item.
        /// </summary>
        private void OnReequipThrowableItem()
        {
            var nextAmmoData = ThrowableAction.GetNextAmmoData();
            // The item shouldn't be reequipped if it is out of ammo.
            if (nextAmmoData.Valid == false) {
                return;
            }

            if (!m_DisableVisibleObject) {
                EnableObjectMeshRenderers(true);
            }
        }

        /// <summary>
        /// Activates the throwable object.
        /// </summary>
        private void ActivateThrowableObject()
        {
            m_ActivateThrowableObjectEvent.CancelWaitForEvent();
            
            m_InstantiatedThrownObject.SetActive(true);
            m_ActivateVisibleObject = true;
            if (!CharacterItem.IsActive()) {
                CharacterItem.SetVisibleObjectActive(true, true);
            }
        }

        /// <summary>
        /// Get the projectile data of the item to throw.
        /// </summary>
        /// <param name="dataStream">The throwable data stream.</param>
        /// <param name="throwPoint">The throw point.</param>
        /// <param name="throwDirection">The throw direction.</param>
        /// <param name="index">The projectile index.</param>
        /// <param name="remove">Should the item be removed when thrown?</param>
        /// <returns>The throwable projectile data.</returns>
        public override ThrowableProjectileData GetProjectileDataToThrow(ThrowableUseDataStream dataStream, Vector3 throwPoint,
            Vector3 throwDirection, int index, bool remove)
        {
            var ammoData = ThrowableAction.GetNextAmmoData();

            m_ThrowableProjectileData.ProjectilePrefab = m_ThrownObject;
            m_ThrowableProjectileData.SpawnedProjectile = m_InstantiatedThrownObject;
            m_ThrowableProjectileData.WasPrespawnedProjectile = true;
            m_ThrowableProjectileData.AmmoData = ammoData;
            m_ThrowableProjectileData.SpawnedTrajectoryObject = m_InstantiatedTrajectoryObject;
            
            if (remove) {
                ThrowableAction.AmmoModuleGroup.FirstEnabledModule.LoadNextAmmoData();
            }
            
            return m_ThrowableProjectileData;
        }

        /// <summary>
        /// Enables or disables the object mesh renderers for the current perspective.
        /// </summary>
        /// <param name="enable">Should the renderers be enabled?</param>
        public void EnableObjectMeshRenderers(bool enable)
        {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (NetworkInfo != null && NetworkInfo.HasAuthority()) {
                NetworkCharacter.EnableThrowableObjectMeshRenderers(this, enable);
            }
#endif
            var renderers = CharacterLocomotion.FirstPersonPerspective ? m_FirstPersonObjectRenderers : m_ThirdPersonObjectRenderers;
            if (renderers != null) {
                for (int i = 0; i < renderers.Length; ++i) {
                    renderers[i].enabled = enable;
                }
            }
        }

        /// <summary>
        /// Can the visible object be activated? An example of when it shouldn't be activated is when a grenade can be thrown but it is not the primary item
        /// so it shouldn't be thrown until after the throw action has started.
        /// </summary>
        /// <returns>True if the visible object can be activated.</returns>
        public override bool CanActivateVisibleObject()
        {
            return !m_Initialized || !m_DisableVisibleObject || m_ActivateVisibleObject;
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public override void StartItemUse(Use useAbility)
        {
            // An Animator Audio State Set may prevent the item from being used.
            if (!ThrowableAction.IsItemInUse()) {
                return;
            }

            SpawnThrowableObject();
        }

        /// <summary>
        /// Spawn the throwable object. Instantiate the object that will actually be thrown.
        /// </summary>
        protected virtual void SpawnThrowableObject()
        {
            if (m_ThrownObject == null) {
                Debug.LogError("Error: The thrown object is empty on the Projectile Module.", CharacterItemAction);
                return;
            }

            // Instantiate the object that will actually be thrown.
            var previewData = ThrowableAction.GetThrowPreviewData();

            var location = previewData.ThrowTransform;
            m_InstantiatedThrownObject = ObjectPoolBase.Instantiate(m_ThrownObject, location.position, location.rotation, m_ObjectTransform.parent);
            m_InstantiatedThrownObject.transform.localScale = location.localScale;
            m_InstantiatedThrownObject.transform.SetLayerRecursively(m_StartLayer);
            m_InstantiatedTrajectoryObject = m_InstantiatedThrownObject.GetCachedComponent<TrajectoryObject>();
            if (m_InstantiatedTrajectoryObject == null) {
                Debug.LogError($"Error: {m_InstantiatedThrownObject.name} must contain the TrajectoryObject component.", m_InstantiatedThrownObject);
                return;
            }
            
            // Initialize the projectile properties here in case the object is not thrown but dropped.
            if (m_InstantiatedTrajectoryObject is ProjectileBase projectile) {
                projectile.InitializeProjectileProperties();
            }

            // The trajectory object will be enabled when the object is thrown.
            m_InstantiatedTrajectoryObject.enabled = false;

            // Hide the object that isn't thrown.
            EnableObjectMeshRenderers(false);

            // The instantiated object may not immediately be visible.
            if (m_DisableVisibleObject) {
                m_InstantiatedThrownObject.SetActive(false);
                m_ActivateVisibleObject = false;
                CharacterItem.SetVisibleObjectActive(false, true);
                m_ActivateThrowableObjectEvent.WaitForEvent(false);
            }
        }

        /// <summary>
        /// The item has been used.
        /// </summary>
        public override void ItemUseComplete()
        {
            // Intentionally left blank.
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {
            m_ActivateVisibleObject = false;
            m_InstantiatedThrownObject = null;
            if (m_DisableVisibleObject) {
                CharacterItem.SetVisibleObjectActive(false, true);
            }
        }
        
        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        public override void OnChangePerspectives(bool firstPersonPerspective)
        {
            // A new object is used for each perspective.
            var perspectiveItem = firstPersonPerspective ? CharacterItem.FirstPersonPerspectiveItem : CharacterItem.ThirdPersonPerspectiveItem;
            m_Object = perspectiveItem.GetVisibleObject();
            m_ObjectTransform = m_Object.transform;

            // OnChangePerspective will be called whether or not the throwable item is equipped. Only set the mesh renderer status if the item is equipped.
            if (CharacterItem.IsActive()) {
                // If the object has already been thrown then the mesh renderer should be disabled.
                if (m_InstantiatedThrownObject != null) {
                    EnableObjectMeshRenderers(m_InstantiatedThrownObject.activeSelf);
                } else {
                    EnableObjectMeshRenderers(!ThrowableAction.WasThrown && !m_DisableVisibleObject);
                }
                if (ThrowableAction.IsThrowing && !ThrowableAction.WasThrown) {
                    // Setup the thrown object if the item is in the process of being thrown.
                    var thrownObjectTransform = m_InstantiatedThrownObject.transform;
                    thrownObjectTransform.parent = m_ObjectTransform.parent;
                    var location = ThrowableAction.GetThrowPreviewData().ThrowTransform;
                    thrownObjectTransform.SetPositionAndRotation(location.position, location.rotation);
                    thrownObjectTransform.localScale = location.localScale;
                    m_InstantiatedThrownObject.transform.SetLayerRecursively(m_StartLayer);
                }
            }
        }
    }
}