/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using Opsive.UltimateCharacterController.Traits;
    using System;
    using UnityEngine;

    /// <summary>
    /// A target used in the shooter training module.
    /// </summary>
    public class ShooterTrainingModuleTarget : MonoBehaviour
    {
        public Action<ShooterTrainingModuleTarget> OnDeath;

        [Tooltip("The health component to monitor.")]
        [SerializeField] private Health m_Health;
        
        public int Index { get; set; }

        /// <summary>
        /// Listen to the death event.
        /// </summary>
        private void Start()
        {
            Shared.Events.EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(gameObject, "OnDeath", HandleDeath);
        }

        /// <summary>
        /// Respawn the object.
        /// </summary>
        public void Respawn()
        {
            Shared.Events.EventHandler.ExecuteEvent(gameObject, "OnRespawn");
        }

        /// <summary>
        /// The object has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the object.</param>
        private void HandleDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            OnDeath?.Invoke(this);
        }
    }
}