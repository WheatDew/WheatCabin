/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects
{
    using Opsive.Shared.Game;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.Shared.Networking;
#endif
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.UltimateCharacterController.Networking.Game;
    using Opsive.UltimateCharacterController.Networking.Objects;
#endif
    using Opsive.UltimateCharacterController.Traits.Damage;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using UnityEngine;

    /// <summary>
    /// An interface used to get a callback when a projectile was destructed or had an impact.
    /// </summary>
    public interface IProjectileOwner
    {
        public GameObject Owner { get; }
        Component SourceComponent { get; }
        
        IDamageSource DamageSource { get; }

        /// <summary>
        /// Handle the projectile being destroyed.
        /// </summary>
        /// <param name="projectile">The projectile that was destroyed.</param>
        /// <param name="hitPosition">The position of the destruction.</param>
        /// <param name="hitNormal">The normal direction of the destruction.</param>
        public void OnProjectileDestruct(ProjectileBase projectile, Vector3 hitPosition, Vector3 hitNormal);

        /// <summary>
        /// Handle the projectile once it impacts on something.
        /// </summary>
        /// <param name="projectile">The projectile object.</param>
        /// <param name="impactContext">The impact data.</param>
        public void OnProjectileImpact(ProjectileBase projectile, ImpactCallbackContext impactContext);
    }
    
    /// <summary>
    /// The Destructible class is an abstract class which acts as the base class for any object that destroys itself and applies a damange.
    /// Primary uses include projectiles and grenades.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public abstract class ProjectileBase : TrajectoryObject
    {
        public event Action<ProjectileBase, ImpactCallbackContext> OnImpact; 
        public event Action<ProjectileBase, Vector3, Vector3> OnDestruct;

        [Tooltip("Disable the collider on impact?")]
        [SerializeField] protected bool m_DisableColliderOnImpact = true;
        [Tooltip("The layers that the object can stick to.")]
        [SerializeField] protected LayerMask m_StickyLayers = ~((1 << LayerManager.IgnoreRaycast) | (1 << LayerManager.Water) | (1 << LayerManager.UI) | (1 << LayerManager.VisualEffect) |
                                                                (1 << LayerManager.Overlay) | (1 << LayerManager.Character) | (1 << LayerManager.SubCharacter));
        [Tooltip("Should the projectile be destroyed when it collides with another object?")]
        [SerializeField] protected bool m_DestroyOnCollision = true;
        [Tooltip("Should the projectile be destroyed after the particle has stopped emitting?")]
        [SerializeField] protected bool m_WaitForParticleStop;
        [Tooltip("The amount of time after a collision that the object should be destroyed.")]
        [SerializeField] protected float m_DestructionDelay;
        [Tooltip("The objects which should spawn when the object is destroyed.")]
        [SerializeField] protected ObjectSpawnInfo[] m_SpawnedObjectsOnDestruction;
        [Tooltip("The impact damage data to deal on collision (can be bypassed when the damage data is specified on initialize).")]
        [SerializeField] protected ImpactDamageData m_DefaultImpactDamageData;
        [Tooltip("Should the local Impact Actions be called when there is an impact?")]
        [SerializeField] protected bool m_InternalImpact = true;
        [Tooltip("A list of actions when the object impacts with something.")]
        [SerializeField] protected ImpactActionGroup m_ImpactActionGroup = ImpactActionGroup.DefaultDamageGroup(true);
        
        public LayerMask StickyLayers { get { return m_StickyLayers; } set { m_StickyLayers = value; } }
        public bool DestroyOnCollision { get { return m_DestroyOnCollision; } set { m_DestroyOnCollision = value; } }
        public bool WaitForParticleStop { get { return m_WaitForParticleStop; } set { m_WaitForParticleStop = value; } }
        public float DestructionDelay { get { return m_DestructionDelay; } set { m_DestructionDelay = value; } }
        public ObjectSpawnInfo[] SpawnedObjectsOnDestruction { get { return m_SpawnedObjectsOnDestruction; } set { m_SpawnedObjectsOnDestruction = value; } }
        public ImpactDamageData DefaultImpactDamageData { get { return m_DefaultImpactDamageData; } set { m_DefaultImpactDamageData = value; } }
        public bool InternalImpact { get { return m_InternalImpact; } set { m_InternalImpact = value; } }
        public ImpactActionGroup ImpactActionGroup { get { return m_ImpactActionGroup; } set { m_ImpactActionGroup = value; } }

        protected uint m_ID;
        protected IProjectileOwner m_ProjectileOwner;
        protected IImpactDamageData m_ImpactDamageData;
        protected ImpactCollisionData m_CachedImpactCollisionData;
        protected ImpactCallbackContext m_CachedImpactCallbackContext;
        protected TrailRenderer m_TrailRenderer;
        protected ParticleSystem m_ParticleSystem;
        protected ScheduledEventBase m_DestroyEvent;
        protected bool m_Destroyed;

        protected UltimateCharacterLocomotion m_StickyCharacterLocomotion;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private INetworkInfo m_NetworkInfo;
        private IDestructibleMonitor m_DestructibleMonitor;
#endif
        
        public uint ID { get { return m_ID; } set { m_ID = value; } }

        /// <summary>
        /// Initialize the defualt values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_CachedImpactCallbackContext = new ImpactCallbackContext();
            m_CachedImpactCollisionData = new ImpactCollisionData();

            m_TrailRenderer = GetComponent<TrailRenderer>();
            if (m_TrailRenderer != null) {
                m_TrailRenderer.enabled = false;
            }
            m_ParticleSystem = GetComponent<ParticleSystem>();
            if (m_ParticleSystem != null) {
                m_ParticleSystem.Stop();
            }

            // The Rigidbody is only used to notify Unity that the object isn't static. The Rigidbody doesn't control any movement.
            var destructableRigidbody = GetComponent<Rigidbody>();
            if (destructableRigidbody != null) {
                destructableRigidbody.mass = m_Mass;
                destructableRigidbody.isKinematic = true;
                destructableRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = GetComponent<INetworkInfo>();
            m_DestructibleMonitor = GetComponent<IDestructibleMonitor>();
#endif

            if (m_DestroyOnCollision && m_CollisionMode != CollisionMode.Collide) {
                Debug.LogWarning($"Warning: The Destructible {name} will be destroyed on collision but does not have a Collision Mode set to Collide.");
                m_CollisionMode = CollisionMode.Collide;
            }
        }

        /// <summary>
        /// Initializes the object. This will be called from an object creating the projectile (such as a weapon).
        /// </summary>
        /// <param name="id">The id used to differentiate this projectile from others.</param>
        /// <param name="velocity">The velocity to apply.</param>
        /// <param name="torque">The torque to apply.</param>
        /// <param name="impactDamageData">Processes the damage dealt to a damage target.</param>
        /// <param name="owner">The object that instantiated the trajectory object.</param>
        public virtual void Initialize(uint id, Vector3 velocity, Vector3 torque, GameObject owner, IImpactDamageData impactDamageData)
        {
            InitializeProjectileProperties(id, impactDamageData);
            m_ProjectileOwner = null;

            base.Initialize(velocity, torque, owner);
        }
        
        /// <summary>
        /// Initializes the object. This will be called from an object creating the projectile (such as a weapon).
        /// </summary>
        /// <param name="id">The id used to differentiate this projectile from others.</param>
        /// <param name="velocity">The velocity to apply.</param>
        /// <param name="torque">The torque to apply.</param>
        /// <param name="impactDamageData">Processes the damage dealt to a Damage Target.</param>
        /// <param name="owner">The object that instantiated the trajectory object.</param>
        public virtual void Initialize(uint id, Vector3 velocity, Vector3 torque, IProjectileOwner owner, IImpactDamageData impactDamageData)
        {
            if (impactDamageData == null) {
                InitializeProjectileProperties(id, m_DefaultImpactDamageData);
            } else {
                InitializeProjectileProperties(id, impactDamageData);
            }
            
            m_ProjectileOwner = owner;

            base.Initialize(velocity, torque, m_ProjectileOwner.DamageSource);
        }

        /// <summary>
        /// Initializes the destructible properties.
        /// </summary>
        public virtual void InitializeProjectileProperties()
        {
            InitializeProjectileProperties(0, m_DefaultImpactDamageData);
        }

        /// <summary>
        /// Initializes the destructible properties.
        /// </summary>
        /// <param name="id">The id used to differentiate this projectile from others.</param>
        /// <param name="impactDamageData">Processes the damage dealt to a Damage Target.</param>
        /// <param name="useObjectImpactLayerAndSurface">Keep the Impact Layer and Surface impact from the prefab.</param>
        public void InitializeProjectileProperties(uint id, IImpactDamageData impactDamageData, bool useObjectImpactLayerAndSurface = false)
        {
            m_ID = id;
            
            if (impactDamageData == null) {
                Debug.LogError("The impact damage data cannot be null.");
                return;
            }
            
            m_Destroyed = false;
            if (m_DestroyEvent != null) {
                Scheduler.Cancel(m_DestroyEvent);
                m_DestroyEvent = null;
            }

            m_ImpactDamageData = impactDamageData;

            if (useObjectImpactLayerAndSurface == false) {
                // The Impact layers can be set directly on the destructible prefab.
                m_ImpactLayers = impactDamageData.LayerMask;

                // The SurfaceImpact may be set directly on the destructible prefab.
                m_SurfaceImpact = impactDamageData.SurfaceImpact;
            }

            if (m_TrailRenderer != null) {
                m_TrailRenderer.Clear();
                m_TrailRenderer.enabled = true;
            }
            if (m_ParticleSystem != null) {
                m_ParticleSystem.Play();
            }
            if (m_Collider != null) {
                m_Collider.enabled = false;
            }
            // The object may be reused and was previously stuck to a character.
            if (m_StickyCharacterLocomotion != null) {
                m_StickyCharacterLocomotion.RemoveIgnoredCollider(m_Collider);
                m_StickyCharacterLocomotion = null;
            }
            enabled = true;
        }

        /// <summary>
        /// Get the impact data for a raycast hit.
        /// </summary>
        /// <param name="hit">The raycast hit.</param>
        /// <returns>The impact data with the raycast hit.</returns>
        public virtual ImpactCollisionData GetImpactData(RaycastHit hit)
        {
            var impactData = m_CachedImpactCollisionData;
            impactData.Reset();
            impactData.Initialize();
            impactData.SetRaycast(hit);
            impactData.SetImpactSource(this);
            impactData.SourceID = m_ID;
            impactData.ImpactDirection = m_Velocity.normalized;
            impactData.ImpactStrength = 1;
            impactData.SurfaceImpact = m_SurfaceImpact;
            impactData.DetectLayers = m_ImpactLayers;

            return impactData;
        }

        /// <summary>
        /// The object has collided with another object.
        /// </summary>
        /// <param name="hit">The RaycastHit of the object. Can be null.</param>
        protected override void OnCollision(RaycastHit? hit)
        {
            // The object may not have been initialized before it collides.
            if (m_GameObject == null) {
                InitializeComponentReferences();
            }
            
            base.OnCollision(hit);

            var forceDestruct = false;
            if (m_CollisionMode == CollisionMode.Collide) {
                // When there is a collision the object should move to the position that was hit so if it's not destroyed then it looks like it
                // is penetrating the hit object.
                if (hit != null && hit.HasValue && m_Collider != null) {
                    var closestPoint = m_Collider.ClosestPoint(hit.Value.point);
                    m_Transform.position += (hit.Value.point - closestPoint);
                    // Only set the parent to the hit transform on uniform objects to prevent stretching.
                    if (MathUtility.IsUniform(hit.Value.transform.localScale)) {
                        // The parent layer must be within the sticky layer mask.
                        if (MathUtility.InLayerMask(hit.Value.transform.gameObject.layer, m_StickyLayers)) {
                            m_Transform.parent = hit.Value.transform;

                            // If the destructible sticks to a character then the object should be added as a sub collider so collisions will be ignored.
                            m_StickyCharacterLocomotion = hit.Value.transform.gameObject.GetCachedComponent<UltimateCharacterLocomotion>();
                            if (m_StickyCharacterLocomotion != null) {
                                m_StickyCharacterLocomotion.AddIgnoredCollider(m_Collider);
                            }
                        } else {
                            forceDestruct = true;
                        }
                    }
                }
                if (m_TrailRenderer != null) {
                    m_TrailRenderer.enabled = false;
                }
            }

            var destructionDelay = m_DestructionDelay;
            if (m_ParticleSystem != null && m_WaitForParticleStop) {
                destructionDelay = m_ParticleSystem.main.duration;
                m_ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Stop();
            }

            if (hit != null && hit.HasValue) {
                var impactData = GetImpactData(hit.Value);
                var impactCallback = m_CachedImpactCallbackContext;
                impactCallback.ImpactCollisionData = impactData;
                impactCallback.ImpactDamageData = m_ImpactDamageData;
                
                OnImpact?.Invoke(this, impactCallback);
                m_ProjectileOwner?.OnProjectileImpact(this, impactCallback);

                if (m_InternalImpact) {
                    m_ImpactActionGroup.OnImpact(impactCallback, true);
                }
            }
            
            if (m_DisableColliderOnImpact) {
                if (m_Collider != null) {
                    m_Collider.enabled = false;
                }
            }

            // The object can destroy itself after a small delay.
            if (m_DestroyEvent == null && (m_DestroyOnCollision || forceDestruct || destructionDelay > 0)) {
                m_DestroyEvent = Scheduler.ScheduleFixed(destructionDelay, Destruct, hit);
            }
        }

        /// <summary>
        /// Destroys the object.
        /// </summary>
        /// <param name="hit">The RaycastHit of the object. Can be null.</param>
        protected virtual void Destruct(RaycastHit? hit)
        {
            if (m_Destroyed) {
                return;
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            // The object can only explode on the server.
            if (m_NetworkInfo != null && !m_NetworkInfo.IsServer()) {
                return;
            }
#endif
            m_DestroyEvent = null;

            // The RaycastHit will be null if the destruction happens with no collision.
            var hitPosition = (hit != null && hit.HasValue) ? hit.Value.point : m_Transform.position;
            var hitNormal = (hit != null && hit.HasValue) ? hit.Value.normal : m_Transform.up;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null && m_NetworkInfo.IsServer()) {
                m_DestructibleMonitor.Destruct(hitPosition, hitNormal);
            }
#endif
            Destruct(hitPosition, hitNormal);
        }

        /// <summary>
        /// Destroys the object.
        /// </summary>
        /// <param name="hitPosition">The position of the destruction.</param>
        /// <param name="hitNormal">The normal direction of the destruction.</param>
        public void Destruct(Vector3 hitPosition, Vector3 hitNormal)
        {
            OnDestruct?.Invoke(this, hitPosition, hitNormal);
            m_ProjectileOwner?.OnProjectileDestruct(this, hitPosition, hitNormal);
            
            for (int i = 0; i < m_SpawnedObjectsOnDestruction.Length; ++i) {
                if (m_SpawnedObjectsOnDestruction[i] == null) {
                    continue;
                }

                var spawnedObject = m_SpawnedObjectsOnDestruction[i].Instantiate(hitPosition, hitNormal, m_NormalizedGravity);
                if (spawnedObject == null) {
                    continue;
                }
                var explosion = spawnedObject.GetCachedComponent<Explosion>();
                if (explosion != null) {
                    explosion.Explode(m_ImpactDamageData, m_Owner, m_OwnerDamageSource);
                }
            }

            // The component and collider no longer need to be enabled after the object has been destroyed.
            if (m_Collider != null) {
                m_Collider.enabled = false;
            }
            if (m_ParticleSystem != null) {
                m_ParticleSystem.Stop();
            }
            m_Destroyed = true;
            m_DestroyEvent = null;
            enabled = false;

            // The destructible should be destroyed.
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (NetworkObjectPool.IsNetworkActive()) {
                // The object may have already been destroyed over the network.
                if (!m_GameObject.activeSelf) {
                    return;
                }
                NetworkObjectPool.Destroy(m_GameObject);
                return;
            }
#endif
            ObjectPoolBase.Destroy(m_GameObject);
        }

        /// <summary>
        /// The component has been disabled.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_TrailRenderer != null) {
                m_TrailRenderer.enabled = false;
            }

            if (m_DestroyOnCollision && m_StickyCharacterLocomotion != null) {
                m_StickyCharacterLocomotion.RemoveIgnoredCollider(m_Collider);
                m_StickyCharacterLocomotion = null;
            }
        }
    }
}