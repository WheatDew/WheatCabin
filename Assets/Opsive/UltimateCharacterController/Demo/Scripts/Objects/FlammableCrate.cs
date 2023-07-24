/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Objects
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// A flammable crate will play a flame particle if it gets hit by a fireball.
    /// </summary>
    public class FlammableCrate : MonoBehaviour
    {
        [Tooltip("The SurfaceImpact that causes the flame to start.")]
        [SerializeField] protected SurfaceImpact m_FlameImpact;
        [Tooltip("A reference to the flame particle that should start when the fireball collides with the crate.")]
        [SerializeField] protected GameObject m_FlamePrefab;
        [Tooltip("The crate that is spawned with the wood shreds.")]
        [SerializeField] protected GameObject m_DestroyedCrate;
        [Tooltip("The interval that the object should have its health reduced.")]
        [SerializeField] protected MinMaxFloat m_HealthReductionInterval = new MinMaxFloat(0.2f, 0.8f);
        [Tooltip("The amount that the object should be damaged on each interval.")]
        [SerializeField] protected MinMaxFloat m_DamageAmount = new MinMaxFloat(4, 10);
        [Tooltip("The amount of time it takes for the wood shreds to be removed.")]
        [SerializeField] protected MinMaxFloat m_WoodShreadRemovalTime = new MinMaxFloat(5, 7);
        [Tooltip("The amount to fade out the AudioSource.")]
        [SerializeField] protected float m_AudioSourceFadeAmount = 0.05f;
        [Tooltip("The flame started.")]
        [SerializeField] protected UnityEvent m_OnStartFlame;
        [Tooltip("The flame stopped.")]
        [SerializeField] protected UnityEvent m_OnStopFlame;

        private Health m_Health;
        private GameObject m_SpawnedParticle;
        private ParticleSystem m_FlameParticle;
        private AudioSource m_FlameParticleAudioSource;
        private GameObject m_SpawnedCrate;
        private ScheduledEventBase m_StopEvent;

        private float m_StartHealth;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Health = GetComponent<Health>();
            m_StartHealth = m_Health.HealthValue;

            EventHandler.RegisterEvent<ImpactCallbackContext>(gameObject, "OnObjectImpact", OnObjectImpact);
        }

        /// <summary>
        /// The crate has been enabled.
        /// </summary>
        private void OnEnable()
        {
            StopParticles();
            if (m_Health.HealthValue != m_StartHealth) {
                m_Health.Heal(m_StartHealth - m_Health.HealthValue);
            }
        }

        /// <summary>
        /// The magic cast has collided with another object.
        /// </summary>
        /// <param name="ctx">The Impact callback context.</param>
        private void OnObjectImpact(ImpactCallbackContext ctx)
        {
            var surfaceImpact = ctx.ImpactCollisionData.SurfaceImpact;
            if (m_FlameParticle != null || (m_FlameImpact != null && m_FlameImpact != surfaceImpact)) {
                return;
            }

            // A fireball has collided with the crate. Start the flame.
            var crateTransform = transform;
            m_SpawnedParticle = ObjectPoolBase.Instantiate(m_FlamePrefab, crateTransform.position, crateTransform.rotation);
            m_FlameParticle = m_SpawnedParticle.GetComponent<ParticleSystem>();
            m_FlameParticleAudioSource = m_SpawnedParticle.GetCachedComponent<AudioSource>();
            m_FlameParticleAudioSource.volume = 1;
            
            m_OnStartFlame?.Invoke();

            // The crate should be destroyed by the flame.
            ReduceHealth();
        }

        /// <summary>
        /// Reduces the health by the damage amount.
        /// </summary>
        private void ReduceHealth()
        {
            m_Health.Damage(m_DamageAmount.RandomValue);
            if (m_Health.IsAlive()) {
                // Keep reducing the object's health until is is no longer alive.
                Scheduler.Schedule(m_HealthReductionInterval.RandomValue, ReduceHealth);
            } else {
                // After the object is no longer alive spawn some wood shreds. These shreds should be cleaned up after a random
                // amount of time.
                var crateTransform = transform;
                m_SpawnedCrate = ObjectPoolBase.Instantiate(m_DestroyedCrate, crateTransform.position, crateTransform.rotation);
                var maxDestroyTime = 0f;
                for (int i = 0; i < m_SpawnedCrate.transform.childCount; ++i) {
                    var destroyTime = m_WoodShreadRemovalTime.RandomValue;
                    if (destroyTime > maxDestroyTime) {
                        maxDestroyTime = destroyTime;
                    }

                    Scheduler.Schedule(destroyTime, x => x.SetActive(false), m_SpawnedCrate.transform.GetChild(i).gameObject);
                }

                m_StopEvent = Scheduler.Schedule(maxDestroyTime, StopParticles);
            }
        }

        /// <summary>
        /// The crate has been destroyed. Stop the particles.
        /// </summary>
        private void StopParticles()
        {
            if (m_StopEvent == null) {
                return;
            }

            ObjectPool.Destroy(m_SpawnedCrate);
            for (int i = 0; i < m_SpawnedCrate.transform.childCount; ++i) {
                m_SpawnedCrate.transform.GetChild(i).gameObject.SetActive(true);
            }
            m_SpawnedCrate = null;

            Scheduler.Cancel(m_StopEvent);
            m_StopEvent = null;
            m_FlameParticle.Stop(true);

            m_OnStopFlame?.Invoke();
            Scheduler.Schedule(0.2f, FadeAudioSource);
            
            var duration = m_FlameParticle.main.duration;
            duration = Mathf.Max(duration, 0.1f + 0.2f * (1/m_AudioSourceFadeAmount));
            Scheduler.Schedule(duration, DestroyParticle);
        }

        /// <summary>
        /// Fades the flame audio source.
        /// </summary>
        private void FadeAudioSource()
        {
            m_FlameParticleAudioSource.volume -= m_AudioSourceFadeAmount;
            if (m_FlameParticleAudioSource.volume > 0) {
                Scheduler.Schedule(0.2f, FadeAudioSource);
            }
        }

        /// <summary>
        /// Destroy the particle object.
        /// </summary>
        private void DestroyParticle()
        {
            // It might not be pooled any more if the prefab instance can destroy itself.
            if (ObjectPoolBase.IsPooledObject(m_SpawnedParticle)) {
                ObjectPoolBase.Destroy(m_SpawnedParticle);
            }
            
            m_SpawnedParticle = null;
            m_FlameParticle = null;
            m_FlameParticleAudioSource = null;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<ImpactCallbackContext>(gameObject, "OnObjectImpact", OnObjectImpact);
        }
    }
}