/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Traits.Damage;
    using UnityEngine;

    /// <summary>
    /// The Projectile component moves a Destructible object along the specified path. Can apply damage at the collision point.
    /// </summary>
    public class Projectile : ProjectileBase
    {
        [Tooltip("The length of time the projectile should exist before it deactivates if no collision occurs.")]
        [SerializeField] protected float m_Lifespan = 10;

        public float Lifespan { get { return m_Lifespan; } set { m_Lifespan = value; } }
        
        private ScheduledEventBase m_ScheduledDeactivation;

        /// <summary>
        /// Initializes the object with the specified velocity and torque.
        /// </summary>
        /// <param name="velocity">The starting velocity.</param>
        /// <param name="torque">The starting torque.</param>
        /// <param name="owner">The object that instantiated the trajectory object.</param>
        /// <param name="ownerSource">The owner damage source in case it is nested.</param>
        /// <param name="ownerCollisionCheck">Should a collision check against the owner be performed?</param>
        /// <param name="defaultNormalizedGravity">The normalized gravity direction if a character isn't specified for the owner.</param>
        public override void Initialize(Vector3 velocity, Vector3 torque, GameObject owner, IDamageSource ownerSource, bool ownerCollisionCheck, Vector3 defaultNormalizedGravity)
        {
            // The projectile can deactivate after it comes in contact with another object or after a specified amount of time. Do the scheduling here to allow
            // it to activate after a set amount of time.
            if (m_Lifespan > 0) {
                m_ScheduledDeactivation = Scheduler.Schedule(m_Lifespan, Deactivate);
            }

            base.Initialize(velocity, torque, owner, ownerSource, ownerCollisionCheck, defaultNormalizedGravity);
        }

        /// <summary>
        /// The projectile has reached its lifespan.
        /// </summary>
        private void Deactivate()
        {
            if (m_ScheduledDeactivation != null) {
                Scheduler.Cancel(m_ScheduledDeactivation);
                m_ScheduledDeactivation = null;
            }
            OnCollision(null);
        }

        /// <summary>
        /// The object has collided with another object.
        /// </summary>
        /// <param name="hit">The RaycastHit of the object. Can be null.</param>
        protected override void OnCollision(RaycastHit? hit)
        {
            if (m_ScheduledDeactivation != null) {
                Scheduler.Cancel(m_ScheduledDeactivation);
                m_ScheduledDeactivation = null;
            }
            base.OnCollision(hit);
        }

        /// <summary>
        /// The component has been disabled.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_ScheduledDeactivation != null) {
                Scheduler.Cancel(m_ScheduledDeactivation);
                m_ScheduledDeactivation = null;
            }
        }
    }
}