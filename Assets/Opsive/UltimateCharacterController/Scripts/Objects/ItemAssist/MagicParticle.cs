/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.ItemAssist
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Items.Actions;
    using UnityEngine;

    /// <summary>
    /// The MagicParticle will perform a MagicItem impact when it collides with an object. In order for this to work correctly the ParticleSystem must
    /// have collisions enabled and the "Send Collision Event" parameter enabled. See this page for more information:
    /// https://docs.unity3d.com/Manual/PartSysCollisionModule.html.
    /// </summary>
    public class MagicParticle : MonoBehaviour
    {
        [Tooltip("Can the particle collide with the originator?")]
        [SerializeField] protected bool m_CanCollideWithOriginator;

        [System.NonSerialized] private GameObject m_GameObject;
        private Transform m_Transform;
        private MagicAction m_MagicAction;
        private uint m_CastID;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_Transform = transform;

            var magicParticleSystem = GetComponent<ParticleSystem>();
            if (magicParticleSystem == null) {
                Debug.LogError($"Error: The MagicProjectile {m_GameObject.name} does not have a ParticleSystem attached.");
                return;
            }

            if (!magicParticleSystem.collision.enabled) {
                Debug.LogError($"Error: The collision module on the MagicProjectile {m_GameObject.name} is disabled. This should be enabled in order to receive collision events.");
                return;
            }

            if (!magicParticleSystem.collision.sendCollisionMessages) {
                Debug.LogError($"Error: Send Collision Messages on the the MagicProjectile {m_GameObject.name} is disabled. This should be enabled in order to receive collision events.");
            }
        }

        /// <summary>
        /// Initializes the particle to the specified MagicItem.
        /// </summary>
        /// <param name="magicAction">The MagicItem that casted the particle.</param>
        /// <param name="castID">The ID of the MagicItem cast.</param>
        public void Initialize(MagicAction magicAction, uint castID)
        {
            m_MagicAction = magicAction;
            m_CastID = castID;
        }

        /// <summary>
        /// A particle has collided with another object.
        /// </summary>
        /// <param name="other">The object that the particle collided with.</param>
        public void OnParticleCollision(GameObject other)
        {
            // If the transform is null the particle hasn't been initialized yet.
            if (m_Transform == null) {
                return;
            }

            // Prevent the particle from colliding with the originator.
            var characterLocomotion = other.GetCachedComponent<Character.UltimateCharacterLocomotion>();
            if (!m_CanCollideWithOriginator && characterLocomotion != null && m_MagicAction.Character == characterLocomotion.gameObject) {
                return;
            }

            // PerformImpact requires a RaycastHit.
            var colliders = characterLocomotion != null ? characterLocomotion.Colliders : other.GetCachedComponents<Collider>();
            if (colliders == null) {
                return;
            }
            for (int i = 0; i < colliders.Length; ++i) {
                if (colliders[i].isTrigger) {
                    continue;
                }
                Vector3 closestPoint;
                if (colliders[i] is BoxCollider || colliders[i] is SphereCollider || colliders[i] is CapsuleCollider || (colliders[i] is MeshCollider && (colliders[i] as MeshCollider).convex)) {
                    closestPoint = colliders[i].ClosestPoint(m_Transform.position);
                } else {
                    closestPoint = m_Transform.position;
                }
                var direction = other.transform.position - closestPoint;
                if (Physics.Raycast(closestPoint - direction.normalized * 0.1f, direction.normalized, out var hit, direction.magnitude + 0.1f, 1 << other.layer)) {
                    m_MagicAction.PerformImpact(m_CastID, m_GameObject, other, hit);
                    break;
                }
            }
        }

        /// <summary>
        /// The particle has been disabled.
        /// </summary>
        private void OnDisable()
        {
            // All of the impact actions should be reset for the particle spawn id.
            if (m_MagicAction != null) {
                var magicImpactCharacterItemActionModules = m_MagicAction.ImpactModuleGroup.EnabledModules;
                for (int i = 0; i < magicImpactCharacterItemActionModules.Count; ++i) {
                    magicImpactCharacterItemActionModules[i].Reset(m_CastID);
                }
            }
        }
    }
}