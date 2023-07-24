/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Traits.Damage;
    using UnityEngine;

    /// <summary>
    /// The Projectile component moves a Destructible object along the specified path. Can apply damage at the collision point.
    /// </summary>
    public class Grenade : ProjectileBase
    {
        [Tooltip("The length of time before the grenade destructs.")]
        [SerializeField] protected float m_Lifespan = 5;
        [Tooltip("A reference to the pin that is removed.")]
        [SerializeField] protected Transform m_Pin;

        public float Lifespan { get { return m_Lifespan; } set { m_Lifespan = value; } }
        public Transform Pin { get { return m_Pin; } set { m_Pin = value; } }

        protected ScheduledEventBase m_ScheduledDeactivation;
        private Transform m_PinParent;
        private Vector3 m_PinLocalPosition;
        private Quaternion m_PinLocalRotation;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Remember the pin location so it can be reattached.
            if (m_Pin != null) {
                m_PinParent = m_Pin.parent;
                m_PinLocalPosition = m_Pin.localPosition;
                m_PinLocalRotation = m_Pin.localRotation;
            }
        }

        /// <summary>
        /// The grenade has been enabled.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            DetachAttachPin(null);
        }

        /// <summary>
        /// The grenade should start to cook.
        /// </summary>
        /// <param name="originator">The object that instantiated the trajectory object.</param>
        /// <param name="ownerSource">The owner damage source in case it is nested.</param>
        public void StartCooking(GameObject originator, IDamageSource ownerSource = null)
        {
            SetOwner(originator, ownerSource, Vector3.up);

            // The grenade should destruct after a specified amount of time.
            m_ScheduledDeactivation = Scheduler.Schedule(m_Lifespan, Deactivate);
        }

        /// <summary>
        /// Detaches or attach the pin.
        /// </summary>
        /// <param name="attachTransform">The transform that the pin should be attached to. If null the pin will move back to the starting location.</param>
        public void DetachAttachPin(Transform attachTransform)
        {
            if (m_Pin == null || m_PinParent == null) {
                return;
            }

            if (attachTransform != null) {
                m_Pin.parent = attachTransform;
            } else { // Attach the pin back to the original transform.
                m_Pin.parent = m_PinParent;
                m_Pin.localPosition = m_PinLocalPosition;
                m_Pin.localRotation = m_PinLocalRotation;
            }
        }

        /// <summary>
        /// The grenade has reached its lifespan.
        /// </summary>
        protected void Deactivate()
        {
            Scheduler.Cancel(m_ScheduledDeactivation);
            m_ScheduledDeactivation = null;

            InitializeComponentReferences(); // The grenade may explode before Awake is called.

            // Change the layer of the GameObject so the explosion doesn't detect the grenade when performing its overlap check.
            var prevLayer = m_GameObject.layer;
            m_GameObject.layer = LayerManager.IgnoreRaycast;
            Destruct(null);
            m_GameObject.layer = prevLayer;
        }
    }
}