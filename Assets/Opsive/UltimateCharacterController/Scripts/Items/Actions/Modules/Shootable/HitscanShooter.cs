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
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// A shooter that detects collision using a hitscan.
    /// </summary>
    [Serializable]
    public class HitscanShooter : ShootableShooterModule
    {
        [Tooltip("Fire in the look direction, or fire from the Fire point direction?")]
        [SerializeField] protected bool m_FireInLookSourceDirection = true;
        [Tooltip("The delay until the hitscan is performed after being fired. Set to 0 to fire immediately.")]
        [SerializeField] protected float m_HitscanFireDelay;
        [Tooltip("The maximum distance in which the hitscan fire can reach.")]
        [SerializeField] protected float m_HitscanFireRange = float.MaxValue;
        [Tooltip("The maximum number of objects the hitscan cast can collide with.")]
        [SerializeField] protected int m_MaxHitscanCollisionCount = 15;
        [Tooltip("A LayerMask of the layers that can be hit when fired at.")]
        [SerializeField] protected LayerMask m_ImpactLayers = ~(1 << LayerManager.IgnoreRaycast |
                                                                1 << LayerManager.TransparentFX | 1 << LayerManager.UI |
                                                                1 << LayerManager.Overlay);
        [Tooltip("Specifies if the hitscan can detect triggers.")]
        [SerializeField] protected QueryTriggerInteraction m_HitscanTriggerInteraction = QueryTriggerInteraction.Ignore;
        [Tooltip("Optionally specify a tracer that should should appear when the hitscan weapon is fired.")]
        [SerializeField] protected GameObject m_Tracer;
        [Tooltip("The length of the tracer if no target is hit.")]
        [SerializeField] protected float m_TracerDefaultLength = 100;
        [Tooltip("Spawn the tracer after the specified delay.")]
        [SerializeField] protected float m_TracerSpawnDelay;
        [Tooltip("The random spread of the bullets once they are fired.")]
        [Range(0, 360)] [SerializeField] protected float m_Spread = 0.01f;
        [Tooltip("The number of rounds to fire in a single shot.")]
        [SerializeField] protected int m_FireCount = 1;
        [Tooltip("The location that the weapon fires from.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_FirePointLocation;
        [Tooltip("The location of the tracer (optional).")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_TracerLocation;

        public override bool FireInLookSourceDirection
        {
            get => m_FireInLookSourceDirection;
            set => m_FireInLookSourceDirection = value;
        }

        public float HitscanFireDelay { get => m_HitscanFireDelay; set => m_HitscanFireDelay = value; }
        public float HitscanFireRange { get => m_HitscanFireRange; set => m_HitscanFireRange = value; }

        public int MaxHitscanCollisionCount
        {
            get => m_MaxHitscanCollisionCount;
            set => m_MaxHitscanCollisionCount = value;
        }

        public LayerMask ImpactLayers { get => m_ImpactLayers; set => m_ImpactLayers = value; }

        public QueryTriggerInteraction HitscanTriggerInteraction
        {
            get => m_HitscanTriggerInteraction;
            set => m_HitscanTriggerInteraction = value;
        }

        public GameObject Tracer { get => m_Tracer; set => m_Tracer = value; }
        public float TracerDefaultLength { get => m_TracerDefaultLength; set => m_TracerDefaultLength = value; }
        public float TracerSpawnDelay { get => m_TracerSpawnDelay; set => m_TracerSpawnDelay = value; }

        public float Spread { get => m_Spread; set => m_Spread = value; }
        public int FireCount { get => m_FireCount; set => m_FireCount = value; }

        public ItemPerspectiveIDObjectProperty<Transform> FirePointLocation
        {
            get => m_FirePointLocation;
            set => m_FirePointLocation = value;
        }

        public ItemPerspectiveIDObjectProperty<Transform> TracerLocation
        {
            get => m_TracerLocation;
            set => m_TracerLocation = value;
        }

        public ILookSource LookSource => ShootableAction.LookSource;

        private RaycastHit[] m_HitscanRaycastHits;
        private UnityEngineUtility.RaycastHitComparer m_RaycastHitComparer = new UnityEngineUtility.RaycastHitComparer();
        protected ShootableImpactCallbackContext m_ShootableImpactCallbackContext;

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="itemAction">The parent Item Action.</param>
        protected override void Initialize(CharacterItemAction itemAction)
        {
            base.Initialize(itemAction);

            m_HitscanRaycastHits = new RaycastHit[m_MaxHitscanCollisionCount];
            m_ShootableImpactCallbackContext = new ShootableImpactCallbackContext();
            m_ShootableImpactCallbackContext.ShootableAction = ShootableAction;
            m_ShootableImpactCallbackContext.ImpactCollisionData = new ImpactCollisionData();

            m_FirePointLocation.Initialize(itemAction);
            m_TracerLocation.Initialize(itemAction);
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
            // Do nothing.
        }

        /// <summary>
        /// Get the projectile data to dire frm the Shootable Action.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <param name="firePoint">The fire point.</param>
        /// <param name="fireDirection">The fire direction.</param>
        /// <param name="ammoData">The ammo data.</param>
        /// <returns>Returns the projectile to fire.</returns>
        public override ShootableProjectileData GetProjectileDataToFire(ShootableUseDataStream dataStream, Vector3 firePoint, Vector3 fireDirection, ShootableAmmoData ammoData)
        {
            // Destroy the projectiles when getting the data to fire.
            return ShootableAction.GetProjectileDataToFire(dataStream, firePoint, fireDirection, ammoData, true, true);
        }

        /// <summary>
        /// Fire the shootable weapon.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        public override void Fire(ShootableUseDataStream dataStream)
        {
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
                Scheduler.Schedule(m_HitscanFireDelay, HitscanFire, dataStream, ammoData);
            }

            // Notify that the shooter fired.
            ShootableAction.OnFire(m_ShootableFireData);
        }

        /// <summary>
        /// Get the fire point.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        /// <returns>The fire point.</returns>
        public virtual Vector3 GetFirePoint(ShootableUseDataStream dataStream)
        {
            var fireInLookSourceDirection = m_FireInLookSourceDirection;
            var useLookPosition = fireInLookSourceDirection && !CharacterLocomotion.ActiveMovementType.UseIndependentLook(false);

            if (useLookPosition) { return LookSource.LookPosition(true); }

            return m_FirePointLocation.GetValue().position;
        }

        /// <summary>
        /// Determines the direction to fire.
        /// </summary>
        /// <returns>The direction to fire.</returns>
        public virtual Vector3 GetFireDirection(Vector3 firePoint, ShootableUseDataStream dataStream)
        {
            var fireInLookSourceDirection = m_FireInLookSourceDirection;
            var direction = fireInLookSourceDirection ? LookSource.LookDirection(firePoint, false, m_ImpactLayers, true, true)
                                                        : m_FirePointLocation.GetValue().forward;

            // Add the spread in a random direction.
            if (m_Spread > 0) {
                direction += Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), direction) * CharacterTransform.up *
                             UnityEngine.Random.Range(0, m_Spread / 360);
            }

            return direction;
        }

        /// <summary>
        /// Fire by casting a ray in the specified direction. If an object was hit then apply damage, apply a force, add a decal, etc.
        /// </summary>
        /// <param name="dataStream">The use data stream.</param>
        /// <param name="ammoData">The shootable ammo data.</param>
        protected virtual void HitscanFire(ShootableUseDataStream dataStream, ShootableAmmoData ammoData)
        {
            // The hitscan should be fired from the center of the camera so the hitscan will always hit the correct crosshairs location.
            var firePoint = GetFirePoint(dataStream);
            var fireDirection = GetFireDirection(firePoint, dataStream);

            m_ShootableFireData.FirePoint = firePoint;
            m_ShootableFireData.FireDirection = fireDirection;

            var projectileData = GetProjectileDataToFire(dataStream, firePoint, fireDirection, ammoData);
            if (!projectileData.AmmoData.Valid) {
                // Dry fire.
                return;
            }

            // Prevent the ray between the character and the look source from causing a false collision.
            var useLookPosition = m_FireInLookSourceDirection && !CharacterLocomotion.ActiveMovementType.UseIndependentLook(false);
            var fireRay = new Ray(firePoint, fireDirection);
            if (useLookPosition && !CharacterLocomotion.FirstPersonPerspective) {
                var direction = CharacterTransform.InverseTransformPoint(firePoint);
                direction.y = 0;
                fireRay.origin = fireRay.GetPoint(direction.magnitude);
            }

            var strength = dataStream.TriggerData.Force;
            var hitCount = Physics.RaycastNonAlloc(fireRay, m_HitscanRaycastHits, m_HitscanFireRange * strength, m_ImpactLayers.value, m_HitscanTriggerInteraction);
            var hasHit = false;

#if UNITY_EDITOR
            if (hitCount == m_MaxHitscanCollisionCount) {
                Debug.LogWarning(
                    $"Warning: The maximum number of colliders have been hit by {GameObject.name}. Consider increasing the Max Hitscan Collision Count value.",
                    ShootableAction);
            }
#endif

            for (int i = 0; i < hitCount; ++i) {
                var closestRaycastHit = QuickSelect.SmallestK(m_HitscanRaycastHits, hitCount, i, m_RaycastHitComparer);
                var hitGameObject = closestRaycastHit.transform.gameObject;
                // The character can't shoot themself.
                if (hitGameObject.transform.IsChildOf(CharacterTransform)
#if FIRST_PERSON_CONTROLLER
                    // The cast should not hit any colliders who are a child of the camera.
                    || hitGameObject.GetCachedParentComponent<FirstPersonController.Character.FirstPersonObjects>() !=
                    null
#endif
                   ) { continue; }

                var impactData = m_ShootableImpactCallbackContext.ImpactCollisionData;
                impactData.Reset();
                impactData.Initialize();
                impactData.SetRaycast(closestRaycastHit);
                impactData.SetImpactSource(ShootableAction);
                {
                    impactData.ImpactDirection = fireDirection;
                    impactData.ImpactStrength = strength;
                }

                ShootableAction.OnFireImpact(m_ShootableImpactCallbackContext);

                // Spawn a tracer which moves to the hit point.
                if (m_Tracer != null) {
                    Scheduler.ScheduleFixed(m_TracerSpawnDelay, AddHitscanTracer, closestRaycastHit.point);
                }

                hasHit = true;
                break;
            }

            // A tracer should still be spawned if no object was hit.
            if (!hasHit && m_Tracer != null) {
                Scheduler.ScheduleFixed(m_TracerSpawnDelay, AddHitscanTracer, MathUtility.TransformPoint(firePoint, Quaternion.LookRotation(fireDirection),
                                            new Vector3(0, 0, m_TracerDefaultLength)));
            }
        }

        /// <summary>
        /// Adds a tracer to the hitscan weapon.
        /// </summary>
        /// <param name="position">The position that the tracer should move towards.</param>
        protected virtual void AddHitscanTracer(Vector3 position)
        {
            var tracerLocation = m_TracerLocation.GetValue();
            var tracerObject = ObjectPoolBase.Instantiate(m_Tracer, tracerLocation.position, tracerLocation.rotation);
            var tracer = tracerObject.GetCachedComponent<Tracer>();
            if (tracer != null) { tracer.Initialize(position); }
        }

        /// <summary>
        /// Stop the item use.
        /// </summary>
        public override void StopItemUse()
        {

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