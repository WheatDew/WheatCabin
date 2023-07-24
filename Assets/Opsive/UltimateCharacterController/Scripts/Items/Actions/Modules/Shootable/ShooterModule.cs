/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Shootable
{
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using System;
    using UnityEngine;

    /// <summary>
    /// The fire data contains information about how the shooter should fire the projectile.
    /// </summary>
    public class ShootableFireData
    {
        protected Vector3 m_FirePoint;
        protected Vector3 m_FireDirection;
        
        public Vector3 FirePoint { get => m_FirePoint; set => m_FirePoint = value; }
        public Vector3 FireDirection { get => m_FireDirection; set => m_FireDirection = value; }
    }
    
    /// <summary>
    /// The base class for the shooter modules, used to fire projectiles, hitscan, etc with a shootable weapon.
    /// </summary>
    [Serializable]
    public abstract class ShootableShooterModule : ShootableActionModule, IModuleCanStartUseItem, IModuleStartItemUse, IModuleStopItemUse
    {
        public override bool IsActiveOnlyIfFirstEnabled => true;
       
        protected ShootableFireData m_ShootableFireData;
        public ShootableFireData FireData { get { return m_ShootableFireData; } set => m_ShootableFireData = value; }
        public abstract bool FireInLookSourceDirection { get; set; }

        /// <summary>
        /// Create the fire data so that it can be cached.
        /// </summary>
        /// <returns>The dire data to cache.</returns>
        public virtual ShootableFireData CreateFireData()
        {
            return new ShootableFireData();
        }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            m_ShootableFireData = CreateFireData();
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
        /// Fire the shootable weapon.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public abstract void Fire(ShootableUseDataStream dataStream);

        /// <summary>
        /// Get the fire point location.
        /// </summary>
        /// <returns>The fire point location.</returns>
        public abstract Transform GetFirePointLocation();
        
        /// <summary>
        /// Get the fire preview data, to give information about the next fire.
        /// </summary>
        /// <returns>The preview data.</returns>
        public abstract ShootableFireData GetFirePreviewData();
        
        /// <summary>
        /// Get the projectile data to dire frm the Shootable Action.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="firePoint">The fire point.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <param name="ammoData">The ammo data.</param>
        /// <returns>Returns the projectile to fire.</returns>
        public virtual ShootableProjectileData GetProjectileDataToFire(ShootableUseDataStream dataStream, Vector3 firePoint, Vector3 fireDirection, ShootableAmmoData ammoData)
        {
            return ShootableAction.GetProjectileDataToFire(dataStream, firePoint, fireDirection, ammoData, true, false);
        }
    }
}