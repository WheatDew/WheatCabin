/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Traits
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Extends the Respawner by listening/executing character related events.
    /// </summary>
    public class CharacterRespawner : Respawner
    {
        [Tooltip("Should a check be performed to determine if there are any objects obstructing the respawn?")]
        [SerializeField] protected bool m_CheckForObstruction;

        public bool CheckForObstruction
        {
            get { return m_CheckForObstruction; }
            set {
                m_CheckForObstruction = value;
                if (m_CheckForObstruction) {
                    InitializeObstructingColliders();
                }
            }
        }

        private Rigidbody m_Rigidbody;
        private bool m_Active;
        private Collider[] m_Colliders;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            if (!Game.CharacterInitializer.AutoInitialization) {
                Game.CharacterInitializer.Instance.OnAwake += AwakeInternal;
                return;
            }

            AwakeInternal();
        }

        /// <summary>
        /// Internal method which initializes the default values.
        /// </summary>
        private void AwakeInternal()
        {
            if (Game.CharacterInitializer.Instance) {
                Game.CharacterInitializer.Instance.OnAwake -= AwakeInternal;
            }

            base.Awake();

            if (m_CheckForObstruction) {
                InitializeObstructingColliders();
            }
            m_Active = true;

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCharacterActivate", OnActivate);
        }

        /// <summary>
        /// Initializes the obstructing colliders.
        /// </summary>
        private void InitializeObstructingColliders()
        {
            m_Rigidbody = gameObject.GetCachedComponent<Rigidbody>();

            var characterLayerManager = gameObject.GetCachedComponent<CharacterLayerManager>();
            var colliders = GetComponentsInChildren<Collider>(true);
            m_Colliders = new Collider[colliders.Length];
            var colliderCount = 0;
            for (int i = 0; i < colliders.Length; ++i) {
                // There are a variety of colliders which should be ignored.
                if (!colliders[i].enabled || colliders[i].isTrigger) {
                    continue;
                }

                // Sub colliders are parented to the object but they are not used for collision detection.
                if (!MathUtility.InLayerMask(colliders[i].gameObject.layer, characterLayerManager.SolidObjectLayers)) {
                    continue;
                }

                // Only certain collider types are supported.
                if (!(colliders[i] is CapsuleCollider || colliders[i] is SphereCollider || colliders[i] is BoxCollider)) {
                    continue;
                }

                m_Colliders[colliderCount] = colliders[i];
                colliderCount++;
            }
            if (colliderCount != m_Colliders.Length) {
                System.Array.Resize(ref m_Colliders, colliderCount);
            }
        }

        /// <summary>
        /// Does the respawn by setting the position and rotation to the specified values.
        /// Enable the GameObject and let all of the listening objects know that the object has been respawned.
        /// </summary>
        /// <param name="position">The respawn position.</param>
        /// <param name="rotation">The respawn rotation.</param>
        /// <param name="transformChange">Was the position or rotation changed?</param>
        public override void Respawn(Vector3 position, Quaternion rotation, bool transformChange)
        {
            // The character can't respawn if the GameObject isn't active.
            if (!m_GameObject.activeSelf) {
                return;
            }

            // The character can't respawn if another object is obstructing the respawn.
            if (m_CheckForObstruction) {
                var obstruction = false;
                var collisionLayerEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
                m_CharacterLocomotion.EnableColliderCollisionLayer(false);
                for (int i = 0; i < m_Colliders.Length; ++i) {
                    if (!m_Colliders[i].gameObject.activeInHierarchy) {
                        continue;
                    }

                    if (m_CharacterLocomotion.OverlapColliders(m_Colliders[i], m_Rigidbody.position + m_CharacterLocomotion.Up * m_CharacterLocomotion.ColliderSpacing, m_Rigidbody.rotation) > 0) {
                        obstruction = true;
                        break;
                    }
                }
                m_CharacterLocomotion.EnableColliderCollisionLayer(collisionLayerEnabled);

                // Wait to respawn if there is an object obstructing the respawn.
                if (obstruction) {
                    m_ScheduledRespawnEvent = Scheduler.Schedule(Random.Range(m_MinRespawnTime, m_MaxRespawnTime), Respawn);
                    return;
                }
            }

            base.Respawn(position, rotation, transformChange);

            // Execute OnCharacterImmediateTransformChange after OnRespawn to ensure all of the interested components are using the new position/rotation.
            if (transformChange) {
                EventHandler.ExecuteEvent(m_GameObject, "OnCharacterImmediateTransformChange", true);
            }
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        protected override void OnDisable()
        {
            // If the GameObject was deactivated then the respawner shouldn't respawn.
            if (m_Active) {
                base.OnDisable();
            }
        }

        /// <summary>
        /// The character has been activated or deactivated.
        /// </summary>
        /// <param name="activate">Was the character activated?</param>
        private void OnActivate(bool activate)
        {
            m_Active = activate;
            if (!m_Active) {
                CancelRespawn();
            }
        }

        /// <summary>
        /// The GameObject has been destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCharacterActivate", OnActivate);
        }
    }
}