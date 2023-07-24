/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.ThirdPersonController.Character.Abilities.Items
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Abilities.Items;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Items.Actions;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Pulls back the item if the character gets too close to a wall. This will prevent the item from clipping with the wall.
    /// </summary>
    [DefaultStopType(AbilityStopType.Automatic)]
    public class ItemPullback : ItemAbility
    {
        [Tooltip("The collider used to detect when the character is near an object and should pull back the items.")]
        [SerializeField] protected Collider[] m_Colliders;
        [Tooltip("The layers that the collider can collide with.")]
        [SerializeField] protected LayerMask m_CollisionLayers = ~(1 << LayerManager.IgnoreRaycast | 1 << LayerManager.TransparentFX | 1 << LayerManager.SubCharacter |
                                                                1 << LayerManager.Overlay | 1 << LayerManager.VisualEffect | 1 << LayerManager.Water);
        [Tooltip("The maximum number of collisions that should be detected by the collider.")]
        [SerializeField] protected int m_MaxCollisionCount = 5;

        public Collider[] Colliders { get { return m_Colliders; } set { m_Colliders = value; } }
        public LayerMask CollisionLayers { get { return m_CollisionLayers; } set { m_CollisionLayers = value; } }

        private Transform[] m_ColliderTransforms;
        private Collider[] m_HitColliders;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            if (m_Colliders == null) {
                Enabled = false;
                return;
            }

            m_HitColliders = new Collider[m_MaxCollisionCount];
            m_ColliderTransforms = new Transform[m_Colliders.Length];
            for (int i = 0; i < m_Colliders.Length; ++i) {
                if (m_Colliders[i] == null || (!(m_Colliders[i] is CapsuleCollider) && !(m_Colliders[i] is SphereCollider))) {
                    Debug.LogError("Error: Only Capsule and Sphere Colliders are supported by the Item Pullback ability.");
                    continue;
                }
                m_ColliderTransforms[i] = m_Colliders[i].transform;
            }

            EventHandler.RegisterEvent<bool>(m_GameObject, "OnCameraWillChangePerspectives", OnChangePerspectives);
        }

        /// <summary>
        /// Called when the ablity is tried to be started. If false is returned then the ability will not be started.
        /// </summary>
        /// <returns>True if the ability can be started.</returns>
        public override bool CanStartAbility()
        {
            return HasCollision();
        }

        /// <summary>
        /// Is there a collision between the item pullback collider and another object?
        /// </summary>
        /// <returns>True if there is a collision with the item pullback collider.</returns>
        private bool HasCollision()
        {
            for (int i = 0; i < m_Colliders.Length; ++i) {
                // The model may not be active.
                if (m_Colliders[i] == null || !m_Colliders[i].gameObject.activeInHierarchy) {
                    continue;
                }

                int hitCount;
                if (m_Colliders[i] is CapsuleCollider) {
                    var capsuleCollider = m_Colliders[i] as CapsuleCollider;
                    if (capsuleCollider.radius == 0) {
                        return false;
                    }
                    Vector3 startEndCap, endEndCap;
                    MathUtility.CapsuleColliderEndCaps(capsuleCollider, m_ColliderTransforms[i].position, m_ColliderTransforms[i].rotation, out startEndCap, out endEndCap);
                    hitCount = Physics.OverlapCapsuleNonAlloc(startEndCap, endEndCap, capsuleCollider.radius * MathUtility.ColliderScaleMultiplier(capsuleCollider), m_HitColliders, m_CollisionLayers, QueryTriggerInteraction.Ignore);
                } else { // SphereCollider.
                    var sphereCollider = m_Colliders[i] as SphereCollider;
                    if (sphereCollider.radius == 0) {
                        return false;
                    }
                    hitCount = Physics.OverlapSphereNonAlloc(m_ColliderTransforms[i].position, sphereCollider.radius * MathUtility.ColliderScaleMultiplier(sphereCollider), m_HitColliders, m_CollisionLayers, QueryTriggerInteraction.Ignore);
                }

                for (int j = 0; j < hitCount; ++j) {
                    // Objects which are children of the character aren't considered a collision.
                    if (m_HitColliders[j].transform.IsChildOf(m_Transform)) {
                        continue;
                    }

                    // Projectiles shouldn't prevent the pullback ability.
                    if (m_HitColliders[j].gameObject.GetCachedComponent<Objects.Projectile>() != null) {
                        continue;
                    }

                    // It only takes one object for the ability to be in a collision state.
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Called when another ability is attempting to start and the current ability is active.
        /// Returns true or false depending on if the new ability should be blocked from starting.
        /// </summary>
        /// <param name="startingAbility">The ability that is starting.</param>
        /// <returns>True if the ability should be blocked.</returns>
        public override bool ShouldBlockAbilityStart(Ability startingAbility)
        {
            return CanStopAbility(startingAbility);
        }

        /// <summary>
        /// Called when the current ability is attempting to start and another ability is active.
        /// Returns true or false depending on if the active ability should be stopped.
        /// </summary>
        /// <param name="activeAbility">The ability that is currently active.</param>
        /// <returns>True if the ability should be stopped.</returns>
        public override bool ShouldStopActiveAbility(Ability activeAbility)
        {
            return CanStopAbility(activeAbility);
        }

        /// <summary>
        /// Can the Item Pullback ability stop the specified ability?
        /// </summary>
        /// <param name="ability">The ability that may be able to be stopped.</param>
        /// <returns>True if the ability can be stopped.</returns>
        private bool CanStopAbility(Ability ability)
        {
            if (ability is Use) {
                var useAbility = ability as Use;
                return useAbility.UsesItemActionType(typeof(ShootableAction));
            }
            if (ability is Reload) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Can the ability be stopped?
        /// </summary>
        /// <param name="force">Should the ability be force stopped?</param>
        /// <returns>True if the ability can be stopped.</returns>
        public override bool CanStopAbility(bool force)
        {
            if (force) { return true; }

            return !HasCollision();
        }

        /// <summary>
        /// The character perspective between first and third person has changed.
        /// </summary>
        /// <param name="firstPersonPerspective">Is the character in a first person perspective?</param>
        private void OnChangePerspectives(bool firstPersonPerspective)
        {
            // Item Pullback does not work in first person mode.
            Enabled = !firstPersonPerspective;
        }

        /// <summary>
        /// The character has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            EventHandler.UnregisterEvent<bool>(m_GameObject, "OnCameraWillChangePerspectives", OnChangePerspectives);
        }
    }
}