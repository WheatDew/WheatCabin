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
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;
    using Object = UnityEngine.Object;

    /// <summary>
    /// The shootable projectile data contains information about the projectile to fire.
    /// </summary>
    public class ShootableProjectileData
    {
        private ShootableProjectileModule m_ProjectileModule;
        private ShootableAmmoData m_AmmoData;
        private GameObject m_ProjectilePrefab;
        private GameObject m_SpawnedProjectile;
        private bool m_WasPrespawnedProjectile;

        public ShootableProjectileModule ProjectileModule { get => m_ProjectileModule; set => m_ProjectileModule = value; }
        public ShootableAmmoData AmmoData { get => m_AmmoData; set => m_AmmoData = value; }
        public GameObject ProjectilePrefab { get => m_ProjectilePrefab; set => m_ProjectilePrefab = value; }
        public GameObject SpawnedProjectile { get => m_SpawnedProjectile; set => m_SpawnedProjectile = value; }
        public bool WasPrespawnedProjectile { get => m_WasPrespawnedProjectile; set => m_WasPrespawnedProjectile = value; }
    }
    
    /// <summary>
    /// This module defines what projectile object gets spawned and when.
    /// </summary>
    [Serializable]
    public abstract class ShootableProjectileModule : ShootableActionModule, IModuleReloadItem, IModuleItemReloadComplete
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;
        
        /// <summary>
        /// Get the preview of the projectile data to fire next.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="firePoint">The origin fire position.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <param name="index">The projectile index.</param>
        /// <returns>The preview of the shootable projectile data to fire next.</returns>
        public abstract ShootableProjectileData GetPreviewProjectileData(ShootableUseDataStream dataStream, Vector3 firePoint, Vector3 fireDirection, int index);
        
        /// <summary>
        /// Get the projectile data to fire next.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="firePoint">The origin fire position.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <param name="ammoData">The ammo data of the projectile to fire.</param>
        /// <param name="remove">Remove the projectile?</param>
        /// <param name="destroy">Destroy the projectile?</param>
        /// <returns>The shootable projectile data to fire next.</returns>
        public abstract ShootableProjectileData GetProjectileDataToFire(ShootableUseDataStream dataStream, Vector3 firePoint,
            Vector3 fireDirection, ShootableAmmoData ammoData, bool remove, bool destroy);

        /// <summary>
        /// Reloads the item.
        /// </summary>
        /// <param name="fullClip">Should the full clip be force reloaded?</param>
        public abstract void ReloadItem(bool fullClip);

        /// <summary>
        /// The item has finished reloading.
        /// </summary>
        /// <param name="success">Was the item reloaded successfully?</param>
        /// <param name="immediateReload">Should the item be reloaded immediately?</param>
        public abstract void ItemReloadComplete(bool success, bool immediateReload);
    }
    
    /// <summary>
    /// This module defines what projectile object gets spawned and when.
    /// </summary>
    [Serializable]
    public class BasicProjectile : ShootableProjectileModule
    {
        protected ShootableProjectileData m_ShootableProjectileData;

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
        /// Create the projectile data.
        /// </summary>
        protected virtual void CreateProjectileData()
        {
            m_ShootableProjectileData = new ShootableProjectileData();
            m_ShootableProjectileData.ProjectileModule = this;
        }

        /// <summary>
        /// Get the preview of the projectile data to fire next.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="firePoint">The origin fire position.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <param name="index">The projectile index.</param>
        /// <returns>The preview of the shootable projectile data to fire next.</returns>
        public override ShootableProjectileData GetPreviewProjectileData(ShootableUseDataStream dataStream, Vector3 firePoint, Vector3 fireDirection, int index)
        {
            m_ShootableProjectileData.WasPrespawnedProjectile = false;
            m_ShootableProjectileData.AmmoData = ShootableAction.GetAmmoDataInClip(0);

            return m_ShootableProjectileData;
        }

        /// <summary>
        /// Get the projectile data to fire next.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="firePoint">The origin fire position.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <param name="ammoData">The ammo data of the projectile to fire.</param>
        /// <param name="remove">Remove the projectile?</param>
        /// <param name="destroy">Destroy the projectile?</param>
        /// <returns>The shootable projectile data to fire next.</returns>
        public override ShootableProjectileData GetProjectileDataToFire(ShootableUseDataStream dataStream, Vector3 firePoint,
            Vector3 fireDirection, ShootableAmmoData ammoData, bool remove, bool destroy)
        {
            m_ShootableProjectileData.WasPrespawnedProjectile = false;
            m_ShootableProjectileData.AmmoData = ammoData;

            return m_ShootableProjectileData;
        }

        /// <summary>
        /// Reloads the item.
        /// </summary>
        /// <param name="fullClip">Should the full clip be force reloaded?</param>
        public override void ReloadItem(bool fullClip) { }

        /// <summary>
        /// The item has finished reloading.
        /// </summary>
        /// <param name="success">Was the item reloaded successfully?</param>
        /// <param name="immediateReload">Should the item be reloaded immediately?</param>
        public override void ItemReloadComplete(bool success, bool immediateReload) { }
    }
    
    /// <summary>
    /// Specifies when the projectile should become visible.
    /// </summary>
    public enum ProjectileVisiblityType
    {
        OnFire,     // The projectile is only visible when the weapon is being fired.
        OnAim,      // The projectile is visible when the character is aiming.
        OnReload,   // The projectile is only visible when reloading.
        Always      // The projectile is always visible when the item is equipped.
    }

    /// <summary>
    /// Specifies the current status of the shown projectile.
    /// </summary>
    public enum ShowProjectileStatus
    {
        NotShown,           // The projectile is not currently shown.
        AttachmentLocation, // The projectile is parented to the attachment location.
        FirePointLocation   // The projectile is parented to the fire point location.
    }

    /// <summary>
    /// Spawns a projectile.
    /// </summary>
    [Serializable]
    public class SpawnProjectile : ShootableProjectileModule, IModuleOnChangePerspectives, IModuleOnAim, IModuleStartItemUse, IModuleUseItem
    {
        [Tooltip("Optionally specify a projectile that the weapon should use.")]
        [SerializeField] protected GameObject m_Projectile;
        [Tooltip("The layer that the projectile should occupy when initially spawned.")]
        [Shared.Utility.Layer] [SerializeField] protected int m_ProjectileStartLayer = LayerManager.IgnoreRaycast;
        [Tooltip("Specifies when the projectile should become visible.")]
        [SerializeField] protected ProjectileVisiblityType m_ProjectileVisibility = ProjectileVisiblityType.OnFire;
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemReloadShowProjectile animation event or wait for the specified duration before showing the projectile.")]
        [SerializeField] protected AnimationEventTrigger m_StartVisibleProjectileEvent = new AnimationEventTrigger(false, 0.4f);
        [Tooltip("The attachment to which the projectile is attached to while reloading the item (if the projectile visibility allows it).")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_ReloadProjectileAttachment;
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemReloadShowProjectile animation event or wait for the specified duration before showing the projectile.")]
        [SerializeField] protected AnimationEventTrigger m_ReloadShowProjectileEvent = new AnimationEventTrigger(false, 0.4f);
        [Tooltip("Specifies if the item should wait for the OnAnimatorItemReloadAttachProjectile animation event or wait for the specified duration before parenting the projectile to the fire point.")]
        [SerializeField] protected AnimationEventTrigger m_ReloadAttachProjectileEvent = new AnimationEventTrigger(false, 0.6f);

        public GameObject Projectile { get { return m_Projectile; } set { m_Projectile = value; } }
        public AnimationEventTrigger StartVisibleProjectileEvent { get { return m_StartVisibleProjectileEvent; } set { m_StartVisibleProjectileEvent.CopyFrom(value); } }
        public AnimationEventTrigger ReloadShowProjectileEvent { get { return m_ReloadShowProjectileEvent; } set { m_ReloadShowProjectileEvent.CopyFrom(value); } }
        public AnimationEventTrigger ReloadAttachProjectileEvent { get { return m_ReloadAttachProjectileEvent; } set { m_ReloadAttachProjectileEvent.CopyFrom(value); } }
        public ProjectileVisiblityType ProjectileVisiblity { get { return m_ProjectileVisibility; } set { m_ProjectileVisibility = value; } }
        
        protected ILookSource LookSource => ShootableAction.LookSource;

        public int ProjectileStartLayer
        {
            get { return m_ProjectileStartLayer; }
            set
            {
                m_ProjectileStartLayer = value;
                if (m_SpawnedProjectile != null) {
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
                    if (NetworkInfo != null && !NetworkInfo.IsLocalPlayer()) {
                        return;
                    }
#endif
                    m_SpawnedProjectile.layer = m_ProjectileStartLayer;
                }
            }
        }
        
        protected ShowProjectileStatus m_ShowReloadProjectile;

        protected ShootableProjectileData m_ShootableProjectileData;
        private GameObject m_SpawnedProjectile;
        private int m_ProjectileLayer;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            CreateProjectileData();
            m_ReloadProjectileAttachment.Initialize(itemAction);
        }

        /// <summary>
        /// Create and cache the projectile data.
        /// </summary>
        protected virtual void CreateProjectileData()
        {
            m_ShootableProjectileData = new ShootableProjectileData();
            m_ShootableProjectileData.ProjectileModule = this;
            m_ShootableProjectileData.SpawnedProjectile = m_SpawnedProjectile;
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register)
        {
            base.UpdateRegisteredEventsInternal(register);
            
            m_StartVisibleProjectileEvent.RegisterUnregisterAnimationEvent(register, Character, "OnAnimatorStartVisibleProjectile", OnStartVisibleProjectile);
            m_ReloadShowProjectileEvent.RegisterUnregisterAnimationEvent(register, Character, "OnAnimatorItemReloadShowProjectile", OnShowReloadProjectile);
            m_ReloadAttachProjectileEvent.RegisterUnregisterAnimationEvent(register, Character, "OnAnimatorItemReloadAttachProjectile", OnAttachReloadProjectile);

            Shared.Events.EventHandler.RegisterUnregisterEvent(register, Character, "OnStartReload", StartItemReload);
        }

        /// <summary>
        /// The item was equipped.
        /// </summary>
        public override void Equip()
        {
            base.Equip();
            
            DetermineVisibleProjectile(false);
        }

        /// <summary>
        /// The item will start unequipping.
        /// </summary>
        public override void StartUnequip()
        {
            base.StartUnequip();

            if (!ShootableAction.Aiming) {
                DetermineVisibleProjectile(true);
            }
        }

        /// <summary>
        /// The item was unequipped.
        /// </summary>
        public override void Unequip()
        {
            base.Unequip();
            
            DetermineVisibleProjectile(true);
        }

        /// <summary>
        /// Start using the item.
        /// </summary>
        /// <param name="useAbility">The use ability that starts the item use.</param>
        public void StartItemUse(Use useAbility)
        {
            m_StartVisibleProjectileEvent.WaitForEvent();
        }

        /// <summary>
        /// Use the item.
        /// </summary>
        public void UseItem()
        {
            DetermineVisibleProjectile(true);
        }

        /// <summary>
        /// The animation invoked that the projectile has started to be visible.
        /// </summary>
        protected virtual void OnStartVisibleProjectile()
        {
            DetermineVisibleProjectile(false);
        }

        /// <summary>
        /// Shows the reload projectile at the attachment location.
        /// </summary>
        private void OnShowReloadProjectile()
        {
            m_ShowReloadProjectile = ShowProjectileStatus.AttachmentLocation;
            DetermineVisibleProjectile(false);

            // The projectile will be attached to the fire point in the future.
            ReloadAttachProjectileEvent.WaitForEvent(false);
        }

        /// <summary>
        /// Attaches the reload projectile to the fire point.
        /// </summary>
        private void OnAttachReloadProjectile()
        {
            m_ShowReloadProjectile = ShowProjectileStatus.FirePointLocation;
            DetermineVisibleProjectile(false);
        }

        /// <summary>
        /// Get the preview of the projectile data to fire next.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="firePoint">The origin fire position.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <param name="index">The projectile index.</param>
        /// <returns>The preview of the shootable projectile data to fire next.</returns>
        public override ShootableProjectileData GetPreviewProjectileData(ShootableUseDataStream dataStream, Vector3 firePoint, Vector3 fireDirection, int index)
        {
            // Index is the projectile index within the clip, index 0 is the next projectile to fire.
            // If remove is set to true remove the projectile ammo from the clip.
            var ammoData = ShootableAction.GetAmmoDataInClip(index);
            var projectileData = GetProjectileDataToFire(dataStream, firePoint, fireDirection, ammoData);
            projectileData.AmmoData = ammoData;

            return m_ShootableProjectileData;
        }

        /// <summary>
        /// Get the projectile data to fire next.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="firePoint">The origin fire position.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <param name="ammoData">The ammo data of the projectile to fire.</param>
        /// <param name="remove">Remove the projectile?</param>
        /// <param name="destroy">Destroy the projectile?</param>
        /// <returns>The shootable projectile data to fire next.</returns>
        public override ShootableProjectileData GetProjectileDataToFire(ShootableUseDataStream dataStream, Vector3 firePoint,
                                                                        Vector3 fireDirection, ShootableAmmoData ammoData, bool remove, bool destroy)
        {
            // Index is the projectile index within the clip, index 0 is the next projectile to fire.
            // If remove is set to true remove the projectile ammo from the clip.
            var projectileData = GetProjectileDataToFire(dataStream, firePoint, fireDirection, ammoData);
            projectileData.AmmoData = ammoData;

            if (remove) {
                RemoveProjectileToFire(destroy);
                DetermineVisibleProjectile(false);
            }
            
            return m_ShootableProjectileData;
        }

        /// <summary>
        /// Starts to reload the item.
        /// </summary>
        private void StartItemReload()
        {
            // The projectile may become visible when the item is reloaded.
            if (m_ProjectileVisibility == ProjectileVisiblityType.OnReload || m_ProjectileVisibility == ProjectileVisiblityType.Always) {
                m_ReloadShowProjectileEvent.WaitForEvent(false);
            }
        }

        /// <summary>
        /// Reloads the item.
        /// </summary>
        /// <param name="fullClip">Should the full clip be force reloaded?</param>
        public override void ReloadItem(bool fullClip)
        {
            if (m_ProjectileVisibility == ProjectileVisiblityType.OnReload) {
                m_ShowReloadProjectile = ShowProjectileStatus.NotShown;
                DetermineVisibleProjectile(false);
            } else {
                m_ShowReloadProjectile = ShowProjectileStatus.FirePointLocation;
                DetermineVisibleProjectile(false);
            }
        }

        /// <summary>
        /// The item has finished reloading.
        /// </summary>
        /// <param name="success">Was the item reloaded successfully?</param>
        /// <param name="immediateReload">Should the item be reloaded immediately?</param>
        public override void ItemReloadComplete(bool success, bool immediateReload)
        {
            if (m_ShowReloadProjectile != ShowProjectileStatus.NotShown) {
                m_ShowReloadProjectile = ShowProjectileStatus.FirePointLocation;
                DetermineVisibleProjectile(false);
            }
            
            m_ShowReloadProjectile = ShowProjectileStatus.NotShown;

            if (!success) {
                m_ReloadAttachProjectileEvent.CancelWaitForEvent();
                m_ReloadShowProjectileEvent.CancelWaitForEvent();
            }
            
            DetermineVisibleProjectile(false);
        }

        /// <summary>
        /// Enables or disables the visibile projectile.
        /// </summary>
        /// <param name="forceDisable">Should the projectile be disabled?</param>
        private void DetermineVisibleProjectile(bool forceDisable)
        {
            if (m_Projectile == null || m_ProjectileVisibility == ProjectileVisiblityType.OnFire) {
                return;
            }

            // The projectile should be visible if:
            // - The item isn't being unequipped and is active.
            // - The projectile is always visible.
            // - The projectile is visible upon aim and the aim ability is active.
            // - The projectile is visible when reloading and the character is reloading.
            var enable = !forceDisable && CharacterItem.IsActive() && ((m_ProjectileVisibility == ProjectileVisiblityType.Always) || 
                (ShootableAction.Aiming && m_ProjectileVisibility == ProjectileVisiblityType.OnAim) || 
                ((m_ShowReloadProjectile != ShowProjectileStatus.NotShown) && m_ProjectileVisibility == ProjectileVisiblityType.OnReload));

            // The projectile can't be shown if there are no projectiles remaining.
            if (enable && ShootableAction.ClipRemainingCount == 0 && m_ShowReloadProjectile == ShowProjectileStatus.NotShown) {
                return;
            }

            if (enable) {
                if (m_SpawnedProjectile == null) {
                    // Spawn the projectile if it does not exist.
                    m_SpawnedProjectile = ObjectPoolBase.Instantiate(m_Projectile);
                    m_SpawnedProjectile.transform.SetLayerRecursively(m_ProjectileStartLayer);

                    // Place it inside the fire location if not in the attachement location.
                    if (m_ShowReloadProjectile != ShowProjectileStatus.AttachmentLocation) {
                        var firePointLocation = ShootableAction.ShooterModuleGroup.FirstEnabledModule.GetFirePointLocation();
                        m_SpawnedProjectile.transform.SetParentOrigin(firePointLocation);
                        m_SpawnedProjectile.transform.SetLayerRecursively(firePointLocation.gameObject.layer);
                    }
                } else if (m_ShowReloadProjectile == ShowProjectileStatus.FirePointLocation) {
                    var firePointLocation = ShootableAction.ShooterModuleGroup.FirstEnabledModule.GetFirePointLocation();
                    m_SpawnedProjectile.transform.parent = firePointLocation;
                }
                
                // Set the projectile in the attachment location. 
                if (m_ShowReloadProjectile == ShowProjectileStatus.AttachmentLocation) {
                    var reloadProjectileAttachment = m_ReloadProjectileAttachment.GetValue();
                    m_SpawnedProjectile.transform.SetParentOrigin(reloadProjectileAttachment);
                    m_SpawnedProjectile.transform.SetLayerRecursively(reloadProjectileAttachment.gameObject.layer);
                }
                var spawnedProjectileParticles = m_SpawnedProjectile.GetComponentInChildren<ParticleSystem>();
                if (spawnedProjectileParticles != null) {
                    spawnedProjectileParticles.Stop(true);
                }
                var projectile = m_SpawnedProjectile.GetComponent<ProjectileBase>();
                if (projectile) {
                    projectile.enabled = false;
                }

                m_ProjectileLayer = m_SpawnedProjectile.layer;
                EventHandler.ExecuteEvent(Character, "OnShootableWeaponShowProjectile", m_SpawnedProjectile, true);
            } else if (m_SpawnedProjectile != null) {
                m_ProjectileLayer = m_SpawnedProjectile.layer;
                EventHandler.ExecuteEvent(Character, "OnShootableWeaponShowProjectile", m_SpawnedProjectile, false);
                
                ObjectPoolBase.Destroy(m_SpawnedProjectile);
                m_SpawnedProjectile = null;
            }
        }
        
        /// <summary>
        /// Get the projectile data to fire.
        /// </summary>
        /// <param name="dataStream">The current shootable data stream.</param>
        /// <param name="firePoint">The fire point.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <param name="ammoData">The ammo data of the projectile.</param>
        /// <returns>The shootable projectile data to fire.</returns>
        protected ShootableProjectileData GetProjectileDataToFire(ShootableUseDataStream dataStream, Vector3 firePoint, Vector3 fireDirection, ShootableAmmoData ammoData)
        {
            var rotation = Quaternion.LookRotation(fireDirection);

            if (!ammoData.Valid) {
                m_ShootableProjectileData.SpawnedProjectile = null;
                m_ShootableProjectileData.AmmoData = ammoData;

                if (m_SpawnedProjectile != null) {
                    RemoveProjectileToFire(false);
                }
                
                return m_ShootableProjectileData;
            }

            if (m_Projectile == null) {
                Debug.LogError("Error: The projectile is empty on the Projectile Module.", CharacterItemAction);
                return m_ShootableProjectileData;
            }

            // The projectile will already be spawned if it is always visible.
            if (m_SpawnedProjectile == null) {
                m_ShootableProjectileData.WasPrespawnedProjectile = false;
                m_SpawnedProjectile = ObjectPoolBase.Instantiate(m_Projectile, firePoint, rotation * m_Projectile.transform.rotation);
                m_SpawnedProjectile.transform.SetLayerRecursively(m_ProjectileLayer);
            } else {
                m_ShootableProjectileData.WasPrespawnedProjectile = true;
                m_SpawnedProjectile.transform.parent = null;
            }

            var projectile = m_SpawnedProjectile.GetComponent<ProjectileBase>();
            if (projectile) {
                projectile.InitializeProjectileProperties();
            }

            m_ShootableProjectileData.ProjectilePrefab = m_Projectile;
            m_ShootableProjectileData.SpawnedProjectile = m_SpawnedProjectile;
            m_ShootableProjectileData.AmmoData = ammoData;

            return m_ShootableProjectileData;
        }

        /// <summary>
        /// Remove the projectile to fire.
        /// </summary>
        /// <param name="destroyProjectile">Destroy the projectile?</param>
        /// <returns>The projectile to remove.</returns>
        public ShootableProjectileData RemoveProjectileToFire(bool destroyProjectile)
        {
            m_ShootableProjectileData.SpawnedProjectile = m_SpawnedProjectile;

            if (destroyProjectile) {
                if (ObjectPool.IsPooledObject(m_SpawnedProjectile)) {
                    ObjectPool.Destroy(m_SpawnedProjectile);
                } else {
                    Object.Destroy(m_SpawnedProjectile);
                }
            }

            m_SpawnedProjectile = null;
            return m_ShootableProjectileData;
        }
        
        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="firstPersonPerspective">Changed to first person?</param>
        public void OnChangePerspectives(bool firstPersonPerspective)
        {
            if (m_SpawnedProjectile == null) { return; }

            var firePointLocation = ShootableAction.MainShooterModule.GetFirePointLocation();
            m_SpawnedProjectile.transform.SetLayerRecursively(CharacterItem.ActivePerspectiveItem.GetVisibleObject().layer);

            if (m_ShowReloadProjectile != ShowProjectileStatus.NotShown) {
                if (m_ShowReloadProjectile == ShowProjectileStatus.AttachmentLocation) {
                    var reloadProjectileAttachment = m_ReloadProjectileAttachment.GetValue();
                    m_SpawnedProjectile.transform.SetParentOrigin(reloadProjectileAttachment);
                    m_SpawnedProjectile.transform.SetLayerRecursively(reloadProjectileAttachment.gameObject.layer);
                } else {
                    // Keep the projectile in the same relative location.
                    var localPosition = m_SpawnedProjectile.transform.localPosition;
                    var localRotation = m_SpawnedProjectile.transform.localRotation;
                    m_SpawnedProjectile.transform.parent = firePointLocation;
                    m_SpawnedProjectile.transform.localPosition = localPosition;
                    m_SpawnedProjectile.transform.localRotation = localRotation;
                }
            } else {
                m_SpawnedProjectile.transform.SetParentOrigin(firePointLocation);
            }
        }

        /// <summary>
        /// The Aim ability has started or stopped.
        /// </summary>
        /// <param name="aim">Has the Aim ability started?</param>
        /// <param name="inputStart">Was the ability started from input?</param>
        public void OnAim(bool aim, bool inputStart)
        {
            DetermineVisibleProjectile(false);
        }

        /// <summary>
        /// Callback when the StateManager has changed the active state on the current object.
        /// </summary>
        public override void StateChange()
        {
            base.StateChange();
            
            DetermineVisibleProjectile(false);
        }
    }
}