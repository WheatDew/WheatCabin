/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.CastEffects
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Items.Actions.Impact;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using Opsive.UltimateCharacterController.Traits.Damage;
    using UnityEngine;

    /// <summary>
    /// Spawns a projectile when the cast is performed.
    /// </summary>
    [System.Serializable]
    public class SpawnProjectile : MagicMultiTargetCastEffectModule, IProjectileOwner
    {
        [Tooltip("The projectile that should be spawned.")]
        [SerializeField] protected GameObject m_ProjectilePrefab;
        [Tooltip("The positional offset that the projectile should be spawned.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The rotational offset that the projectile should be spawned.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("The speed that the projectile should be initialized to.")]
        [SerializeField] protected float m_Speed = 1;
        [Tooltip("Should the projecitle be parented to the origin?")]
        [SerializeField] protected bool m_ParentToOrigin;

        public GameObject ProjectilePrefab { get { return m_ProjectilePrefab; } set { m_ProjectilePrefab = value; } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; } }
        public float Speed { get { return m_Speed; } set { m_Speed = value; } }
        public bool ParentToOrigin { get { return m_ParentToOrigin; } set { m_ParentToOrigin = value; } }

        GameObject IProjectileOwner.Owner => Character;
        Component IProjectileOwner.SourceComponent => m_CharacterItemAction;
        IDamageSource IProjectileOwner.DamageSource => MagicAction;

        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="useDataStream">The use data stream, contains the cast data.</param>
        protected override void DoCastInternal(MagicUseDataStream useDataStream)
        {
            var origin = useDataStream.CastData.CastOrigin;
            var castPosition = useDataStream.CastData.CastPosition;
            var direction = useDataStream.CastData.Direction;
            var targetPosition = useDataStream.CastData.CastTargetPosition;
            m_CastID = (uint)useDataStream.CastData.CastID;

            if (m_CharacterItemAction.IsDebugging) {
                Debug.DrawRay(castPosition, direction * 5, Color.red, 0.1f);
            }

            if (m_ProjectilePrefab == null) {
                Debug.LogError("Error: A Projectile Prefab must be specified.", MagicAction);
                return;
            }

            var position = Utility.MathUtility.TransformPoint(castPosition, Transform.rotation, m_PositionOffset);
            var obj = ObjectPoolBase.Instantiate(m_ProjectilePrefab, position,
                Quaternion.LookRotation(direction, Transform.up) * Quaternion.Euler(m_RotationOffset),
                m_ParentToOrigin ? origin : null);
            var projectile = obj.GetComponent<Projectile>();
            if (projectile != null) {
                projectile.Initialize(m_CastID, direction * m_Speed, Vector3.zero, this, null);
            } else {
                Debug.LogWarning($"Warning: The projectile {m_ProjectilePrefab.name} does not have the MagicProjectile component attached.");
            }

            var magicParticle = obj.GetComponent<MagicParticle>();
            if (magicParticle != null) { magicParticle.Initialize(MagicAction, m_CastID); }

            base.DoCastInternal(useDataStream);

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (NetworkInfo != null && NetworkInfo.HasAuthority()) {
                Networking.Game.NetworkObjectPool.NetworkSpawn(m_ProjectilePrefab, projectile.gameObject, true);
                // The server will manage the projectile.
                if (!NetworkInfo.IsServer()) {
                    ObjectPoolBase.Destroy(obj);
                }
            }
#endif
        }

        /// <summary>
        /// Handle the projectile being destroyed.
        /// </summary>
        /// <param name="projectile">The projectile that was destroyed.</param>
        /// <param name="hitPosition">The position of the destruction.</param>
        /// <param name="hitNormal">The normal direction of the destruction.</param>
        public void OnProjectileDestruct(ProjectileBase projectile, Vector3 hitPosition, Vector3 hitNormal)
        {
            // Do nothing.
        }

        /// <summary>
        /// Handle the projectile once it impacts on something.
        /// </summary>
        /// <param name="projectile">The projectile object.</param>
        /// <param name="impactContext">The impact data.</param>
        public void OnProjectileImpact(ProjectileBase projectile, ImpactCallbackContext impactContext)
        {
            MagicAction.PerformImpact(projectile.ID, impactContext);
        }
    }
}