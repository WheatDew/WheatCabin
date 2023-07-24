/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Traits.Damage;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// A shooter module which fires a projectile object.
    /// </summary>
    [Serializable]
    public class ProjectileShooter : ShootableShooterModule , IProjectileOwner
    {
        [Tooltip("Fire in the look direction, or fire from the Fire point direction?")]
        [SerializeField] protected bool m_FireInLookSourceDirection;
        [Tooltip("Fire from the look source position?")]
        [SerializeField] protected bool m_UseLookSourcePosition;
        [Tooltip("The magnitude of the projectile velocity when fired. The direction is determined by the fire direction.")]
        [SerializeField] protected float m_ProjectileFireVelocityMagnitude = 10;
        [Tooltip("The layer that the projectile object should change to after being fired.")]
        [Shared.Utility.Layer] [SerializeField] protected int m_ProjectileFiredLayer = LayerManager.VisualEffect;
        [Tooltip("The amount of time after the object has been fired to change the layer.")]
        [SerializeField] protected float m_LayerChangeDelay = 0.1f;
        [Tooltip("The amount of time after another item has been used that the projectile should be enabled again.")]
        [SerializeField] protected float m_ProjectileEnableDelayAfterOtherUse = 0.4f;
        [Tooltip("The number of rounds to fire in a single shot.")]
        [SerializeField] protected int m_FireCount = 1;
        [Tooltip("The fire point location.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_FirePointLocation;
        [Tooltip("The random spread of the bullets once they are fired.")]
        [Range(0, 360)] [SerializeField] protected float m_Spread = 0.01f;
        [Tooltip("A LayerMask of the layers that can be hit when fired at.")]
        [SerializeField] protected LayerMask m_ImpactLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.TransparentFX | 1 << LayerManager.UI | 1 << LayerManager.Overlay);
        [Tooltip("Should the projectile inherit the character's forward velocity?")]
        [SerializeField] protected bool m_InheritCharacterVelocity;
        
        public bool UseLookSourcePosition { get => m_UseLookSourcePosition; set => m_UseLookSourcePosition = value; }
        public override bool FireInLookSourceDirection { get => m_FireInLookSourceDirection; set => m_FireInLookSourceDirection = value; }
        public float ProjectileFireVelocityMagnitude { get { return m_ProjectileFireVelocityMagnitude; } set { m_ProjectileFireVelocityMagnitude = value; } }
        public LayerMask ProjectileFiredLayer { get { return m_ProjectileFiredLayer; } set { m_ProjectileFiredLayer = value; } }
        public float LayerChangeDelay { get { return m_LayerChangeDelay; } set { m_LayerChangeDelay = value; } }
        public float ProjectileEnableDelayAfterOtherUse { get { return m_ProjectileEnableDelayAfterOtherUse; } set { m_ProjectileEnableDelayAfterOtherUse = value; } }
        public int FireCount { get { return m_FireCount; } set { m_FireCount = value; } }
        public float Spread { get { return m_Spread; } set { m_Spread = value; } }
        public ItemPerspectiveIDObjectProperty<Transform> FirePointLocation { get => m_FirePointLocation; set => m_FirePointLocation = value; }
        public LayerMask ImpactLayers { get { return m_ImpactLayers; } set { m_ImpactLayers = value; } }
        public bool InheritCharacterVelocity { get { return m_InheritCharacterVelocity; } set { m_InheritCharacterVelocity = value; } }
        
        public ILookSource LookSource => ShootableAction.LookSource;

        private RaycastHit m_RaycastHit;
        protected ShootableImpactCallbackContext m_ShootableImpactCallbackContext;

        GameObject IProjectileOwner.Owner => Character;
        Component IProjectileOwner.SourceComponent => m_CharacterItemAction;
        IDamageSource IProjectileOwner.DamageSource => ShootableAction;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            
            m_ShootableImpactCallbackContext = new ShootableImpactCallbackContext();
            
            m_FirePointLocation.Initialize(itemAction);
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
        /// Get the fire preview data, to give information about the next fire.
        /// </summary>
        /// <returns>The preview data.</returns>
        public override ShootableFireData GetFirePreviewData()
        {
            var dataStream = ShootableAction.ShootableUseDataStream;
            m_ShootableFireData.FirePoint = GetFirePoint(dataStream);
            m_ShootableFireData.FireDirection = GetFireDirection(m_ShootableFireData.FirePoint, dataStream);
            
            return m_ShootableFireData;
        }
        
        /// <summary>
        /// Get the fire point.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        /// <returns>The fire point.</returns>
        public virtual Vector3 GetFirePoint(ShootableUseDataStream dataStream)
        {
            var useLookPosition = m_UseLookSourcePosition && !CharacterLocomotion.ActiveMovementType.UseIndependentLook(false);

            if (useLookPosition) {
                return LookSource.LookPosition(true);
            }

            var fireLocation = m_FirePointLocation.GetValue();
            if (fireLocation == null) {
                Debug.LogError($"The Fire Location transform is missing in '{this}', please set it in the inspector, or use the fireInLookSourceDirection option", ShootableAction);
                return CharacterTransform.position;
            }

            return fireLocation.position;
        }
        
        /// <summary>
        /// Determines the direction to fire.
        /// </summary>
        /// <returns>The direction to fire.</returns>
        public virtual Vector3 GetFireDirection(Vector3 firePoint, ShootableUseDataStream dataStream)
        {
            var direction = Vector3.zero;
            if (m_FireInLookSourceDirection) {
                direction = LookSource.LookDirection(firePoint, false, m_ImpactLayers, true, true);
            } else {
                direction = m_FirePointLocation.GetValue().forward;
            }

            // Add the spread in a random direction.
            if (m_Spread > 0) {
                direction += Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), direction) * CharacterTransform.up * UnityEngine.Random.Range(0, m_Spread / 360);
            }

            return direction;
        }

        /// <summary>
        /// Fire the shootable weapon.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void Fire(ShootableUseDataStream dataStream)
        {
            CharacterItemAction.DebugLogger.Log(this,"Fire");
            
            var ammoIndex = 0;
            var ammoData = ShootableAction.GetAmmoDataInClip(ammoIndex);

            if (ammoData.Valid == false) {
                ShootableAction.OnDryFire(m_ShootableFireData);
                return;
            }
            
            // Get the preview data to set things up.
            GetFirePreviewData();
            
            // Remove the ammo before it is fired.
            ShootableAction.ClipModuleGroup.FirstEnabledModule.AmmoUsed(1, ammoIndex);
            
            // Fire as many projectiles or hitscan bullets as the fire count specifies.
            for (int i = 0; i < m_FireCount; ++i) {
                ProjectileFire(dataStream, ammoData);
            }
            
            // Notify that the shooter fired.
            ShootableAction.OnFire(m_ShootableFireData);
        }

        /// <summary>
        /// Get the fire point location.
        /// </summary>
        /// <returns>The fire point location.</returns>
        public override Transform GetFirePointLocation()
        {
            return m_FirePointLocation.GetValue();
        }

        /// <summary>
        /// Spawns a projectile which will move in the firing direction.
        /// </summary>
        protected virtual void ProjectileFire(ShootableUseDataStream dataStream, ShootableAmmoData ammoData)
        {
            var firePoint = GetFirePoint(dataStream);
            var fireDirection = GetFireDirection(firePoint, dataStream);
            var projectileData = GetProjectileDataToFire(dataStream, firePoint, fireDirection, ammoData);

            m_ShootableFireData.FirePoint = firePoint;
            m_ShootableFireData.FireDirection = fireDirection;
            
            if (!projectileData.AmmoData.Valid) {
                CharacterItemAction.DebugLogger.Log(this,"No Ammo to fire!");
                return;
            }
            
            var spawnedProjectile = projectileData.SpawnedProjectile;
            if (projectileData.WasPrespawnedProjectile) {
                CharacterItemAction.DebugLogger.Log(this,"Prespawned");

                // The projectile may be on the other side of an object (especially in the case of separate arms for the first person perspective). Perform a linecast
                // to ensure the projectile doesn't go through any objects.
                var fireInLookSourceDirection = m_FireInLookSourceDirection || m_UseLookSourcePosition;
                if (fireInLookSourceDirection && !CharacterLocomotion.ActiveMovementType.UseIndependentLook(false) &&
                    Physics.Linecast(LookSource.LookPosition(true), spawnedProjectile.transform.position, out m_RaycastHit, m_ImpactLayers, QueryTriggerInteraction.Ignore)) {
                    // The cast should not hit the character that it belongs to.
                    var updatePosition = true;
                    var hitGameObject = m_RaycastHit.transform.gameObject;
                    var parentCharacterLocomotion = hitGameObject.GetCachedParentComponent<UltimateCharacterLocomotion>();
                    if (parentCharacterLocomotion != null && parentCharacterLocomotion == CharacterLocomotion) {
                        updatePosition = false;
                    }
#if FIRST_PERSON_CONTROLLER
                    // The cast should not hit any colliders who are a child of the camera.
                    if (updatePosition && hitGameObject.GetCachedParentComponent<FirstPersonController.Character.FirstPersonObjects>() != null) { 
                        updatePosition = false; 
                    }
#endif
                    if (updatePosition) { spawnedProjectile.transform.position = m_RaycastHit.point; }

                    EventHandler.ExecuteEvent(Character, "OnShootableWeaponShowProjectile", spawnedProjectile, false);
                }
            } else {
                CharacterItemAction.DebugLogger.Log("Not Prespawned");
            }
            
            // Optionally change the layer after the object has been fired. This allows the object to change from the first person Overlay layer
            // to the Default layer after it has cleared the first person weapon.
            if (spawnedProjectile.layer != m_ProjectileFiredLayer) {
                Scheduler.ScheduleFixed(m_LayerChangeDelay, ChangeFiredLayer, spawnedProjectile);
            }

            var projectile = spawnedProjectile.GetCachedComponent<Projectile>();
            var rotation = Quaternion.LookRotation(fireDirection);
            var projectileVelocity = (CharacterTransform.forward * (m_InheritCharacterVelocity ? CharacterLocomotion.LocalVelocity.z : 0)) + 
                                        rotation * (m_ProjectileFireVelocityMagnitude * dataStream.TriggerData.Force * Vector3.forward);

            CharacterItemAction.DebugLogger.DrawRay(this, firePoint, fireDirection * 10, Color.red, 1);
            CharacterItemAction.DebugLogger.DrawRay(this, firePoint, projectileVelocity, Color.blue, 1);
            
            projectile.Initialize(0, projectileVelocity, Vector3.zero, this, null);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (NetworkInfo != null && NetworkInfo.HasAuthority()) {
                Networking.Game.NetworkObjectPool.NetworkSpawn(projectileData.ProjectilePrefab, projectile.gameObject, true);
                // The server will manage the projectile.
                if (!NetworkInfo.IsServer()) {
                    ObjectPoolBase.Destroy(projectileData.SpawnedProjectile);
                    projectileData.SpawnedProjectile = null;
                }
            }
#endif
            spawnedProjectile = null;
        }


        /// <summary>
        /// Handle the projectile once it impacts on something.
        /// </summary>
        /// <param name="projectile">The projectile object.</param>
        /// <param name="impactContext">The impact data.</param>
        public virtual void OnProjectileImpact(ProjectileBase projectile, ImpactCallbackContext impactContext)
        {
            m_ShootableImpactCallbackContext.Reset();
            
            impactContext.ImpactCollisionData.SourceItemAction = ShootableAction;
            m_ShootableImpactCallbackContext.ImpactCollisionData = impactContext.ImpactCollisionData;
            m_ShootableImpactCallbackContext.ImpactDamageData = impactContext.ImpactDamageData;
            
            ShootableAction.OnFireImpact(m_ShootableImpactCallbackContext);
        }

        /// <summary>
        /// Handle the projectile being destroyed.
        /// </summary>
        /// <param name="projectile">The projectile that was destroyed.</param>
        /// <param name="hitPosition">The position of the destruction.</param>
        /// <param name="hitNormal">The normal direction of the destruction.</param>
        public void OnProjectileDestruct(ProjectileBase projectile, Vector3 hitPosition, Vector3 hitNormal)
        {
            // Do nothing.
        }

        /// <summary>
        /// Changes the fired projectile to the fired layer.
        /// </summary>
        /// <param name="projectileObject">The projectile that was fired.</param>
        private void ChangeFiredLayer(GameObject projectileObject)
        {
            projectileObject.transform.SetLayerRecursively(m_ProjectileFiredLayer);
        }

        /// <summary>
        /// The module has been added to the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was added to.</param>
        public override void OnEditorModuleAdded(GameObject gameObject)
        {
            base.OnEditorModuleAdded(gameObject);

            m_FirePointLocation = new ItemPerspectiveIDObjectProperty<Transform>();

#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(gameObject)) {
                return;
            }
#endif

#if FIRST_PERSON_CONTROLLER
            var firstPersonPerspectiveItem = gameObject.GetComponent<FirstPersonController.Items.FirstPersonPerspectiveItem>();
            if (firstPersonPerspectiveItem != null && firstPersonPerspectiveItem.GetVisibleObject() != null) {
                var location = new GameObject("FirePointLocation").transform;
                location.SetParentOrigin(firstPersonPerspectiveItem.GetVisibleObject().transform);
                m_FirePointLocation.SetFirstPersonValue(location);
            }
#endif
            var thirdPersonPerspectiveItem = gameObject.GetComponent<ThirdPersonController.Items.ThirdPersonPerspectiveItem>();
            if (thirdPersonPerspectiveItem != null && thirdPersonPerspectiveItem.GetVisibleObject() != null) {
                var location = new GameObject("FirePointLocation").transform;
                location.SetParentOrigin(thirdPersonPerspectiveItem.GetVisibleObject().transform);
                m_FirePointLocation.SetThirdPersonValue(location);
            }
        }

        /// <summary>
        /// The module has been removed from the item action.
        /// </summary>
        /// <param name="gameObject">The GameObject that the item was removed from.</param>
        public override void OnEditorModuleRemoved(GameObject gameObject)
        {
            base.OnEditorModuleRemoved(gameObject);

            m_FirePointLocation.OnEditorDestroyObjectCleanup(gameObject);
        }
    }
}