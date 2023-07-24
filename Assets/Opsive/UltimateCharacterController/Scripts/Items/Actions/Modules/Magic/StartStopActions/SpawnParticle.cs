/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.StartStopActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Spawns a particle.
    /// </summary>
    [System.Serializable]
    public class SpawnParticle : MagicStartStopModule
    {
        [Tooltip("The particle prefab that should be spawned.")]
        [SerializeField] protected GameObject m_ParticlePrefab;
        [Tooltip("The positional offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The rotational offset that the particle should be spawned.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("Should the particle be parented to the origin?")]
        [SerializeField] protected bool m_ParentToOrigin;

        public GameObject ParticlePrefab { get { return m_ParticlePrefab; } set { m_ParticlePrefab = value; } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; } }
        public bool ParentToOrigin { get { return m_ParentToOrigin; } set { m_ParentToOrigin = value; } }

        private Transform m_SpawnedTransform;

        /// <summary>
        /// The action has started.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void Start(MagicUseDataStream useDataStream)
        {
            Spawn(useDataStream.CastData.CastOrigin);
        }

        /// <summary>
        /// The action has stopped.
        /// </summary>
        public override void Stop(MagicUseDataStream useDataStream)
        {
            m_SpawnedTransform = null;
        }

        /// <summary>
        /// Spawns the particle.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        private void Spawn(Transform origin)
        {
            if (m_ParticlePrefab == null) {
                Debug.LogError("Error: A Particle Prefab must be specified.", MagicAction);
                return;
            }

            var position = MathUtility.TransformPoint(origin.position, CharacterTransform.rotation, m_PositionOffset);
            var rotation = origin.rotation * Quaternion.Euler(m_RotationOffset);
            var obj = ObjectPoolBase.Instantiate(m_ParticlePrefab, position, 
                rotation, m_ParentToOrigin ? origin : null);
            m_SpawnedTransform = obj.transform;
            var particleSystem = obj.GetCachedComponent<ParticleSystem>();
            if (particleSystem == null) {
                Debug.LogError($"Error: A Particle System must be specified on the particle {m_ParticlePrefab}.", MagicAction);
                return;
            }

            particleSystem.Clear(true);
        }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="origin">The location that the cast originates from.</param>
        public override void OnChangePerspectives(bool firstPerson)
        {
            var origin = MagicAction.MagicUseDataStream?.CastData?.CastOrigin;
            
            if (m_SpawnedTransform == null || m_SpawnedTransform.parent == origin) {
                return;
            }

            var localRotation = m_SpawnedTransform.localRotation;
            var localScale = m_SpawnedTransform.localScale;
            m_SpawnedTransform.parent = origin;
            m_SpawnedTransform.position = MathUtility.TransformPoint(origin?.position ?? Vector2.zero, CharacterTransform.rotation, m_PositionOffset);
            m_SpawnedTransform.localRotation = localRotation;
            m_SpawnedTransform.localScale = localScale;
        }
    }
}