/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo
{
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using Opsive.UltimateCharacterController.SurfaceSystem;
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Continuously applies damage to the character while the character is within the trigger.
    /// </summary>
    public class ImpactZone : MonoBehaviour
    {
        [Tooltip("Only impact object with a health component.")]
        [SerializeField] protected bool m_OnlyWithHealth;
        [Tooltip("The delay until the damage is started to be applied.")]
        [SerializeField] protected float m_InitialImpactDelay = 0.5f;
        [Tooltip("The delay until the damage is started to be applied.")]
        [SerializeField] protected float m_ImpactInterval = 0.2f;
        [Tooltip("The interval between damage events.")]
        [SerializeField] protected LayerMask m_LayerMask = (1 << LayerManager.Character | 1 << LayerManager.Default | 1 << LayerManager.Enemy);
        [Tooltip("The surface impact.")]
        [SerializeField] protected SurfaceImpact m_SurfaceImpact;
        [Tooltip("The impact damage data")]
        [SerializeField] protected ImpactDamageData m_DefaultImpactDamageData;
        [Tooltip("The impact damage data")]
        [SerializeField] protected ImpactActionGroup m_ImpactActionGroup = ImpactActionGroup.DefaultDamageGroup(true);

        private ImpactCallbackContext m_ImpactCallbackContext;
        private Dictionary<Collider, ScheduledEventBase> m_Targets = new Dictionary<Collider, ScheduledEventBase>();

        private void Start()
        {
            m_ImpactCallbackContext = new ImpactCallbackContext();
            m_ImpactCallbackContext.ImpactCollisionData = new ImpactCollisionData();
            m_ImpactCallbackContext.ImpactDamageData = m_DefaultImpactDamageData;
        }

        /// <summary>
        /// An object has entered the trigger.
        /// </summary>
        /// <param name="other">The object that entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == gameObject) {
                return;
            }

            if (other.attachedRigidbody != null && transform.IsChildOf(other.attachedRigidbody.transform)) {
                return;
            }
            
            if (m_Targets.ContainsKey(other)) {
                return;
            }
            
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }

            var schedule =Scheduler.Schedule(m_InitialImpactDelay, Impact, other);
            m_Targets.Add(other, schedule);
        }

        /// <summary>
        /// Apply damage to the health component.
        /// </summary>
        private void Impact(Collider collider)
        {
            var health = collider.GetComponentInParent<Health>();
            if (m_OnlyWithHealth && health == null) {
                return;
            }

            var impactCollisionData = m_ImpactCallbackContext.ImpactCollisionData;
            impactCollisionData.Reset();
            impactCollisionData.Initialize();
            impactCollisionData.SetImpactSource(gameObject,null);
            impactCollisionData.SetImpactTarget(collider);
            impactCollisionData.ImpactDirection = Vector3.zero;
            impactCollisionData.ImpactPosition = collider.transform.position + Random.insideUnitSphere;
            impactCollisionData.ImpactStrength = 1;
            impactCollisionData.SurfaceImpact = m_SurfaceImpact;
            
            m_ImpactActionGroup.OnImpact(m_ImpactCallbackContext, true);

            // Apply the damage again if the object still has health remaining.
            if (health == null || health.Value > 0) {
                m_Targets[collider] = Scheduler.Schedule(m_ImpactInterval, Impact, collider);
            }

            if (collider.gameObject.activeInHierarchy == false) {
                OnTriggerExit(collider);
            }
        }

        /// <summary>
        /// An object has exited the trigger.
        /// </summary>
        /// <param name="other">The collider that exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (m_Targets.TryGetValue(other, out var scheduledEvent) == false) {
                return;
            }
            
            // The object has left the trigger - stop applying damage.
            Scheduler.Cancel(scheduledEvent);
            m_Targets.Remove(other);
        }

        /// <summary>
        /// Draw a gizmo showing the damage zone.
        /// </summary>
        private void OnDrawGizmos()
        {
            var meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null) {
                var color = Color.red;
                color.a = 0.5f;
                Gizmos.color = color;
                var meshTransform = meshCollider.transform;
                Gizmos.DrawMesh(meshCollider.sharedMesh, meshTransform.position, meshTransform.rotation);
            }
        }
    }
}