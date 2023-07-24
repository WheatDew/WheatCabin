/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Melee
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// The base class for melee extra modules, used to add more generic functionality to the melee item action.
    /// </summary>
    [Serializable]
    public abstract class MeleeExtraModule : MeleeActionModule
    {
    }
    
    /// <summary>
    /// A trail effect module for melee attacks.
    /// </summary>
    [Serializable]
    public class TrailEffect : MeleeExtraModule,
        IModuleStartItemUse, IModuleStopItemUse, IModuleOnChangePerspectives
    {
        /// <summary>
        /// Specifies when the melee weapon trail should be shown.
        /// </summary>
        public enum TrailVisibilityType
        {
            Attack, // The trail is only visible while attacking.
            Always  // The trail is always visible.
        }
        
        [Tooltip("A reference to the trail prefab that should be spawned.")]
        [SerializeField] protected GameObject m_Trail;
        [Tooltip("Specifies when the melee weapon trail should be shown.")]
        [SerializeField] protected TrailVisibilityType m_TrailVisibility = TrailVisibilityType.Attack;
        [Tooltip("The delay until the trail should be spawned after it is visible.")]
        [SerializeField] protected float m_TrailSpawnDelay;
        [Tooltip("Specifies if the item should wait for the OnAnimatorStopTrail animation event or wait for the specified duration before stopping the trail during an attack.")]
        [SerializeField] protected AnimationEventTrigger m_AttackStopTrailEvent = new AnimationEventTrigger(false, 0.5f);
        [Tooltip("The location that the melee weapon trail is spawned at.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_TrailLocation;
        
        public GameObject Trail { get { return m_Trail; } set { m_Trail = value; } }
        public TrailVisibilityType TrailVisibility { get { return m_TrailVisibility; } set { m_TrailVisibility = value; } }
        public float TrailSpawnDelay { get { return m_TrailSpawnDelay; } set { m_TrailSpawnDelay = value; } }
        public AnimationEventTrigger AttackStopTrailEvent { get { return m_AttackStopTrailEvent; } set { m_AttackStopTrailEvent.CopyFrom(value); } }

        private ScheduledEventBase m_TrailSpawnEvent;
        private ScheduledEventBase m_TrailStopEvent;
        private Trail m_ActiveTrail;

        [Shared.Utility.NonSerialized] public Transform TrailLocation { get { return m_TrailLocation.GetValue(); } set { m_TrailLocation.SetValue(value); } }

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent item action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);
            
            m_TrailLocation.Initialize(itemAction);
        }

        /// <summary>
        /// Updates the registered events when the item is equipped and the module is enabled.
        /// </summary>
        protected override void UpdateRegisteredEventsInternal(bool register) {
            base.UpdateRegisteredEventsInternal(register);
            m_AttackStopTrailEvent.RegisterUnregisterAnimationEvent(register,Character,"OnAnimatorStopTrail", HandleStopTrail);
        }

        /// <summary>
        /// The item has been equipped by the character.
        /// </summary>
        public override void Equip()
        {
            base.Equip();

            if (m_Trail != null && m_TrailVisibility == TrailVisibilityType.Always) {
                m_TrailSpawnEvent = Scheduler.Schedule(m_TrailSpawnDelay, SpawnTrail);
            }
        }
        
        /// <summary>
        /// Spawns a weapon trail prefab.
        /// </summary>
        private void SpawnTrail()
        {
            Transform trailLocation = m_TrailLocation.GetValue();
            if (trailLocation != null) {
                var trailObject = ObjectPoolBase.Instantiate(m_Trail);
                trailObject.transform.SetParentOrigin(trailLocation);
                trailObject.layer = trailLocation.gameObject.layer;
                m_ActiveTrail = trailObject.GetCachedComponent<Trail>();
            }
            m_TrailSpawnEvent = null;

            if (m_TrailVisibility == TrailVisibilityType.Attack && !m_AttackStopTrailEvent.WaitForAnimationEvent) {
                m_TrailStopEvent = Scheduler.ScheduleFixed(m_AttackStopTrailEvent.Duration, HandleStopTrail);
            }
        }

        /// <summary>
        /// Starts the item use.
        /// </summary>
        /// <param name="itemAbility">The item ability that is using the item.</param>
        public void StartItemUse(Use itemAbility)
        {
            if (m_TrailVisibility == TrailVisibilityType.Attack) {
                StopTrail();
            }

            if (m_Trail != null && m_TrailVisibility == TrailVisibilityType.Attack) {
                m_TrailSpawnEvent = Scheduler.Schedule(m_TrailSpawnDelay, SpawnTrail);
            }
        }
        
        /// <summary>
        /// Stops the item use.
        /// </summary>
        public void StopItemUse()
        {
            if (m_TrailVisibility == TrailVisibilityType.Attack) {
                StopTrail();
            }
        }

        /// <summary>
        /// The item has been unequipped by the character.
        /// </summary>
        public override void Unequip()
        {
            base.Unequip();
            
            StopTrail();
        }

        /// <summary>
        /// Stop the trail.
        /// </summary>
        private void HandleStopTrail()
        {
            if (m_TrailVisibility != TrailVisibilityType.Attack || !MeleeAction.IsAttacking) {
                return;
            }
            m_TrailStopEvent = null;

            StopTrail();
        }
        
        /// <summary>
        /// Stops the weapon trail.
        /// </summary>
        private void StopTrail()
        {
            if (m_Trail == null) {
                return;
            }

            if (m_TrailSpawnEvent != null) {
                Scheduler.Cancel(m_TrailSpawnEvent);
                m_TrailSpawnEvent = null;
            }
            if (m_TrailStopEvent != null) {
                Scheduler.Cancel(m_TrailStopEvent);
                m_TrailStopEvent = null;
            }
            if (m_ActiveTrail != null) {
                m_ActiveTrail.StopGeneration();
                m_ActiveTrail = null;
            }
        }
        
        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        public void OnChangePerspectives(bool firstPersonPerspective)
        {

            if (m_ActiveTrail != null) {
                Transform trailLocation = m_TrailLocation?.GetValue(firstPersonPerspective);
                if (trailLocation != null) {
                    m_ActiveTrail.transform.SetParentOrigin(trailLocation);
                    m_ActiveTrail.gameObject.layer = trailLocation.gameObject.layer;
                }
            }
        }
    }
}