/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Throwable
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;
    using Vector3 = UnityEngine.Vector3;

    /// <summary>
    /// The data associated with the object being thrown.
    /// </summary>
    [Serializable]
    public class ThrowableThrowData
    {
        protected Vector3 m_FirePoint;
        protected Vector3 m_FireDirection;
        protected ThrowableProjectileData m_ProjectileData;
        
        public Vector3 FirePoint { get => m_FirePoint; set => m_FirePoint = value; }
        public Vector3 FireDirection { get => m_FireDirection; set => m_FireDirection = value; }
        public ThrowableProjectileData ProjectileData { get => m_ProjectileData; set => m_ProjectileData = value; }
        public Transform TrajectoryTransform { get; set; }
        public Transform ThrowTransform { get; set; }
        public Vector3 TrajectoryOffset { get; set; }
        public int ImpactLayers { get; set; }
        public Vector3 Velocity { get; set; }
    }
    
    /// <summary>
    /// The base class for the module that deals with throwing the projectile.
    /// </summary>
    [Serializable]
    public abstract class ThrowableThrowerModule : ThrowableActionModule, IModuleCanStartUseItem, IModuleStartItemUse, IModuleStopItemUse
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;
           
        protected ThrowableThrowData m_ThrowableThrowData;
        public ThrowableThrowData ThrowData { get { return m_ThrowableThrowData; } set => m_ThrowableThrowData = value; }

        /// <summary>
        /// Create the throw data, to be cached.
        /// </summary>
        /// <returns></returns>
        public virtual ThrowableThrowData CreateThrowData()
        {
            return new ThrowableThrowData();
        }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_ThrowableThrowData = CreateThrowData();
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public abstract bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState);

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public abstract void StartItemUse(Use useAbility);
        
        /// <summary>
        /// Stop the item use.
        /// </summary>
        public abstract void StopItemUse();
        
        /// <summary>
        /// Throw the throwable item.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public abstract void Throw(ThrowableUseDataStream dataStream);

        /// <summary>
        /// Get the throw location.
        /// </summary>
        /// <returns>The throw location.</returns>
        public abstract Transform GetThrowLocation();
        
        /// <summary>
        /// Get the trajectory object location.
        /// </summary>
        /// <returns>The trajectory object location.</returns>
        public abstract Transform GetTrajectoryLocation();
        
        /// <summary>
        /// Get the preview data for the throw before it even starts.
        /// </summary>
        /// <returns>The preview data of the throw.</returns>
        public abstract ThrowableThrowData GetThrowPreviewData();
    }

    /// <summary>
    /// Throw projectile objects using this throwable thrower module.
    /// </summary>
    [Serializable]
    public class ProjectileThrower : ThrowableThrowerModule
    {
        [Tooltip("A LayerMask of the layers that can be hit when fired at.")]
        [SerializeField] protected LayerMask m_ImpactLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.TransparentFX | 1 << LayerManager.UI | 1 << LayerManager.Overlay);
        [Tooltip("The layer that the projectile object should change to after being fired.")]
        [Shared.Utility.Layer] [SerializeField] protected int m_ProjectileThrownLayer = LayerManager.VisualEffect;
        [Tooltip("The amount of time after the object has been fired to change the layer.")]
        [SerializeField] protected float m_LayerChangeDelay = 0.1f;
        [Tooltip("Throw in the look source direction.")]
        [SerializeField] protected bool m_ThrowInLookSourceDirection;
        [Tooltip("The location at which the throw should occur.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_ThrowLocation;
        [Tooltip("The location of the throw Trajectory.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_TrajectoryLocation;
        [Tooltip("The offset of the trajectory visualization relative to the trajectory transform set on the Throwable Item Properties.")]
        [SerializeField] protected Vector3 m_TrajectoryOffset;
        [Tooltip("The starting velocity of the thrown object.")]
        [SerializeField] protected Vector3 m_Velocity = new Vector3(0, 5, 10);

        public LayerMask ImpactLayers { get => m_ImpactLayers; set => m_ImpactLayers = value; }
        public int ProjectileThrownLayer { get => m_ProjectileThrownLayer; set => m_ProjectileThrownLayer = value; }
        public float LayerChangeDelay { get => m_LayerChangeDelay; set => m_LayerChangeDelay = value; }
        public bool ThrowInLookSourceDirection { get => m_ThrowInLookSourceDirection; set => m_ThrowInLookSourceDirection = value; }
        public ItemPerspectiveIDObjectProperty<Transform> ThrowLocation { get => m_ThrowLocation; set => m_ThrowLocation = value; }
        public ItemPerspectiveIDObjectProperty<Transform> TrajectoryLocation { get => m_TrajectoryLocation; set => m_TrajectoryLocation = value; }
        public Vector3 TrajectoryOffset { get => m_TrajectoryOffset; set => m_TrajectoryOffset = value; }
        public Vector3 Velocity { get => m_Velocity; set => m_Velocity = value; }

        private RaycastHit m_RaycastHit;
        protected ImpactCallbackContext m_ThrowableImpactCallbackContext;
        
        public ILookSource LookSource => ThrowableAction.LookSource;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            m_ThrowableImpactCallbackContext = new ImpactCallbackContext(ThrowableAction);
            m_ThrowLocation.Initialize(m_CharacterItemAction);
            m_TrajectoryLocation.Initialize(m_CharacterItemAction);
        }

        /// <summary>
        /// Can the item be used?
        /// </summary>
        /// <param name="useAbility">A reference to the Use ability.</param>
        /// <param name="abilityState">The state of the Use ability when calling CanUseItem.</param>
        /// <returns>True if the item can be used.</returns>
        public override bool CanStartUseItem(Use useAbility, UsableAction.UseAbilityState abilityState)
        {
            return true;
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public override void StartItemUse(Use useAbility)
        {
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {
        }

        /// <summary>
        /// Get the throw location.
        /// </summary>
        /// <returns>The throw location.</returns>
        public override Transform GetThrowLocation()
        {
            return m_ThrowLocation.GetValue();
        }

        /// <summary>
        /// Get the trajectory object location.
        /// </summary>
        /// <returns>The trajectory object location.</returns>
        public override Transform GetTrajectoryLocation()
        {
            //m_ThrowableItemPerpectiveProperties.TrajectoryLocation != null ? m_ThrowableItemPerpectiveProperties.TrajectoryLocation : m_CharacterTransform;
            var value = m_TrajectoryLocation.GetValue();
            if (value == null) {
                return CharacterTransform;
            }

            return value;
        }

        /// <summary>
        /// Get the preview data for the throw before it even starts.
        /// </summary>
        /// <returns>The preview data of the throw.</returns>
        public override ThrowableThrowData GetThrowPreviewData()
        {
            var dataStream = ThrowableAction.ThrowableUseDataStream;
            
            var firePoint = GetThrowPoint(dataStream);
            var fireDirection = GetThrowDirection(firePoint, dataStream);
            var projectileData = ThrowableAction.GetProjectileDataToThrow(dataStream, firePoint, fireDirection, 0, false);
            m_ThrowableThrowData.FirePoint = firePoint;
            m_ThrowableThrowData.FireDirection = fireDirection;
            m_ThrowableThrowData.ProjectileData = projectileData;
            m_ThrowableThrowData.Velocity = m_Velocity;
            m_ThrowableThrowData.ImpactLayers = m_ImpactLayers;
            m_ThrowableThrowData.TrajectoryOffset = m_TrajectoryOffset;
            m_ThrowableThrowData.ThrowTransform = GetThrowLocation();
            m_ThrowableThrowData.TrajectoryTransform = GetTrajectoryLocation();
            
            return m_ThrowableThrowData;
        }

        /// <summary>
        /// Get the throw point in world space.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        /// <returns>The throw point.</returns>
        public virtual Vector3 GetThrowPoint(ThrowableUseDataStream dataStream)
        {
            var useLookPosition = m_ThrowInLookSourceDirection && !CharacterLocomotion.ActiveMovementType.UseIndependentLook(false);
            if (useLookPosition) {
                return LookSource.LookPosition(true);
            }

            var fireLocation = m_ThrowLocation.GetValue();
            if (fireLocation == null) {
                Debug.LogError($"The Throw Location transform is missing in '{this}', please set it in the inspector, or use the fireInLookSourceDirection option", m_CharacterItemAction);
                return CharacterTransform.position;
            }

            return fireLocation.position;
        }
        
        /// <summary>
        /// Get the projectile data to throw.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        /// <param name="firePoint">The fire point.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <returns>The throwable projectile data.</returns>
        public virtual ThrowableProjectileData GetProjectileDataToThrow(ThrowableUseDataStream dataStream, Vector3 firePoint, Vector3 fireDirection)
        {
            return ThrowableAction.GetProjectileDataToThrow(dataStream, firePoint, fireDirection, 0, true);
        }

        /// <summary>
        /// Determines the direction to fire.
        /// </summary>
        /// <returns>The direction to fire.</returns>
        public virtual Vector3 GetThrowDirection(Vector3 throwPoint, ThrowableUseDataStream dataStream)
        {
            var direction = m_ThrowInLookSourceDirection ? 
                LookSource.LookDirection(throwPoint, false, m_ImpactLayers, true, true) : m_ThrowLocation.GetValue().forward;

            return direction;
        }

        /// <summary>
        /// Throw the throwable item.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void Throw(ThrowableUseDataStream dataStream)
        {
            CharacterItemAction.DebugLogger.Log(this,"Throw");

            ThrowInternal(dataStream);
            
            ThrowableAction.OnThrow(m_ThrowableThrowData);
        }

        /// <summary>
        /// Throw the item.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        protected virtual void ThrowInternal(ThrowableUseDataStream dataStream)
        {
            var strength = dataStream.TriggerData.Force;
            var fireInLookSourceDirection = m_ThrowInLookSourceDirection;

            // Get the data to initialize the ThrowableThrowData.
            GetThrowPreviewData();
            var projectileData = GetProjectileDataToThrow(dataStream, m_ThrowableThrowData.FirePoint, m_ThrowableThrowData.FireDirection);
            m_ThrowableThrowData.ProjectileData = projectileData;

            if (projectileData.AmmoData.Valid == false) {
                CharacterItemAction.DebugLogger.Log(this, "No ammo to throw.");
                return;
            }

            var spawnedThrownObject = projectileData.SpawnedProjectile;
            var spawnedTrajectoryObject = projectileData.SpawnedTrajectoryObject;

            spawnedThrownObject.transform.parent = null;
            // The collider was previously disabled. Enable it again when it is thrown.
            var thrownCollider = spawnedThrownObject.GetCachedComponent<Collider>();
            thrownCollider.enabled = true;

            // When the item is used the trajectory object should start moving on its own.
            // The throwable item may be on the other side of an object (especially in the case of separate arms for the first person perspective). Perform a linecast
            // to ensure the throwable item doesn't move through any objects.
            var collisionEnabled = CharacterLocomotion.CollisionLayerEnabled;
            CharacterLocomotion.EnableColliderCollisionLayer(false);
            if (!CharacterLocomotion.ActiveMovementType.UseIndependentLook(false) &&
                Physics.Linecast(CharacterLocomotion.LookSource.LookPosition(true), spawnedTrajectoryObject.transform.position,
                    out m_RaycastHit,
                    m_ImpactLayers, QueryTriggerInteraction.Ignore)) {
                spawnedTrajectoryObject.transform.position = m_RaycastHit.point;
            }
            CharacterLocomotion.EnableColliderCollisionLayer(collisionEnabled);

            var trajectoryTransform = GetTrajectoryLocation();
            var lookDirection = LookSource.LookDirection(trajectoryTransform.TransformPoint(m_TrajectoryOffset), false, m_ImpactLayers, true, true);
            var velocity = MathUtility.TransformDirection(m_Velocity, Quaternion.LookRotation(lookDirection, CharacterTransform.up));
            // Prevent the item from being thrown behind the character. This can happen if the character is looking straight up and there is a positive
            // y velocity. Gravity will cause the thrown object to go in the opposite direction.
            if (Vector3.Dot(velocity.normalized, CharacterTransform.forward) < 0 &&
                CharacterTransform.InverseTransformDirection(velocity.normalized).y > 0) {
                velocity = CharacterTransform.up * velocity.magnitude;
            }

            if (spawnedTrajectoryObject is ProjectileBase projectile) {
                projectile.InitializeProjectileProperties();
            }
            
            spawnedTrajectoryObject.Initialize(
                CharacterLocomotion.Alive
                    ? (velocity + (CharacterTransform.forward * CharacterLocomotion.LocalVelocity.z))
                    : Vector3.zero, Vector3.zero, Character, ThrowableAction, false, Vector3.down);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (NetworkInfo != null) {
                Networking.Game.NetworkObjectPool.NetworkSpawn(projectileData.m_ProjectilePrefab, projectileData.SpawnedProjectile, true);
                // The server will manage the projectile.
                if (!NetworkInfo.IsServer()) {
                    ObjectPoolBase.Destroy(spawnedThrownObject);
                    projectileData.SpawnedProjectile = null;
                    return;
                }
            }
#endif

            // Optionally change the layer after the object has been fired. This allows the object to change from the first person Overlay layer
            // to the Default layer after it has cleared the first person weapon.
            if (spawnedThrownObject.layer != m_ProjectileThrownLayer) {
                Scheduler.ScheduleFixed(m_LayerChangeDelay, ChangeThrownLayer, spawnedThrownObject);
            }
        }
        
        /// <summary>
        /// Changes the fired projectile to the fired layer.
        /// </summary>
        /// <param name="projectileObject">The projectile that was fired.</param>
        private void ChangeThrownLayer(GameObject projectileObject)
        {
            projectileObject.transform.SetLayerRecursively(m_ProjectileThrownLayer);
        }

        /// <summary>
        /// The module has been added to the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was added to.</param>
        public override void OnEditorModuleAdded(GameObject gameObject)
        {
            base.OnEditorModuleAdded(gameObject);

            m_ThrowLocation = new ItemPerspectiveIDObjectProperty<Transform>();
            m_TrajectoryLocation = new ItemPerspectiveIDObjectProperty<Transform>();

#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(gameObject)) {
                return;
            }
#endif

#if FIRST_PERSON_CONTROLLER
            var firstPersonPerspectiveItem = gameObject.GetComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
            if (firstPersonPerspectiveItem != null && firstPersonPerspectiveItem.GetVisibleObject() != null) {
                var location = new GameObject("ThrowLocation").transform;
                location.transform.SetParentOrigin(firstPersonPerspectiveItem.GetVisibleObject().transform);
                m_ThrowLocation.SetFirstPersonValue(location);

                location = new GameObject("TrajectoryLocation").transform;
                location.SetParentOrigin(firstPersonPerspectiveItem.GetVisibleObject().transform);
                m_TrajectoryLocation.SetFirstPersonValue(location);
            }
#endif
            var thirdPersonPerspectiveItem = gameObject.GetComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
            if (thirdPersonPerspectiveItem != null && thirdPersonPerspectiveItem.GetVisibleObject() != null) {
                var location = new GameObject("ThrowLocation").transform;
                location.SetParentOrigin(thirdPersonPerspectiveItem.GetVisibleObject().transform);
                m_ThrowLocation.SetThirdPersonValue(location);

                location = new GameObject("TrajectoryLocation").transform;
                location.SetParentOrigin(thirdPersonPerspectiveItem.GetVisibleObject().transform);
                m_TrajectoryLocation.SetThirdPersonValue(location);
            }
        }

        /// <summary>
        /// The module has been removed from the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was removed from.</param>
        public override void OnEditorModuleRemoved(GameObject gameObject)
        {
            base.OnEditorModuleRemoved(gameObject);
            
            m_ThrowLocation.OnEditorDestroyObjectCleanup(gameObject);
            m_TrajectoryLocation.OnEditorDestroyObjectCleanup(gameObject);
        }
    }
}