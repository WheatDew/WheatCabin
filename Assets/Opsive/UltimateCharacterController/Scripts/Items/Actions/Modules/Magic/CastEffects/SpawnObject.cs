/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Modules.Magic.CastEffects
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Spawns an object when the cast is performed.
    /// </summary>
    [System.Serializable]
    public class SpawnObject : MagicMultiTargetCastEffectModule, IMagicObjectAction
    {
        [Tooltip("The object that should be spawned.")]
        [SerializeField] protected GameObject m_Object;
        [Tooltip("The positional offset that the object should be spawned.")]
        [SerializeField] protected Vector3 m_PositionOffset;
        [Tooltip("The rotational offset that the object should be spawned.")]
        [SerializeField] protected Vector3 m_RotationOffset;
        [Tooltip("Should the object be parented to the origin?")]
        [SerializeField] protected bool m_ParentToOrigin;

        public GameObject Object { get { return m_Object; } set { m_Object = value; } }
        public Vector3 PositionOffset { get { return m_PositionOffset; } set { m_PositionOffset = value; } }
        public Vector3 RotationOffset { get { return m_RotationOffset; } set { m_RotationOffset = value; } }
        public bool ParentToOrigin { get { return m_ParentToOrigin; } set { m_ParentToOrigin = value; } }

        private GameObject m_SpawnedObject;

        [Shared.Utility.NonSerialized] public GameObject SpawnedGameObject { get => m_SpawnedObject; set => m_SpawnedObject = value; }

        /// <summary>
        /// Performs the cast.
        /// </summary>
        /// <param name="useDataStream">The use data stream, contains the cast data.</param>
        protected override void DoCastInternal(MagicUseDataStream useDataStream)
        {
            m_CastID = (uint)useDataStream.CastData.CastID;
            
            if (m_SpawnedObject != null) {
                return;
            }

            if (m_Object == null) {
                Debug.LogError("Error: An Object must be specified.", MagicAction);
                return;
            }

            var origin = useDataStream.CastData.CastOrigin;
            var direction = useDataStream.CastData.Direction;
            var targetPosition = useDataStream.CastData.CastTargetPosition;
            var position = MathUtility.TransformPoint(origin.position, CharacterTransform.rotation, m_PositionOffset);
            if (targetPosition != position) {
                direction = (targetPosition - position).normalized;
            }
            m_SpawnedObject = ObjectPoolBase.Instantiate(m_Object, position, 
                Quaternion.LookRotation(direction, CharacterLocomotion.Up) * Quaternion.Euler(m_RotationOffset), m_ParentToOrigin ? origin : null);
        }

        /// <summary>
        /// Stops the cast.
        /// </summary>
        public override void StopCast()
        {
            if (m_SpawnedObject != null) {
                ObjectPoolBase.Destroy(m_SpawnedObject);
                m_SpawnedObject = null;
            }

            base.StopCast();
        }

        /// <summary>
        /// The character has changed perspectives.
        /// </summary>
        /// <param name="firstPerson">Changed to first person?</param>
        public override void OnChangePerspectives(bool firstPerson)
        {
            var origin = MagicAction?.MagicUseDataStream?.CastData?.CastOrigin;
            if (m_SpawnedObject == null || m_SpawnedObject.transform.parent == origin) {
                return;
            }

            var spawnedTransform = m_SpawnedObject.transform;
            var localRotation = spawnedTransform.localRotation;
            var localScale = spawnedTransform.localScale;
            spawnedTransform.parent = origin;
            spawnedTransform.position = MathUtility.TransformPoint(origin.position, CharacterTransform.rotation, m_PositionOffset);
            spawnedTransform.localRotation = localRotation;
            spawnedTransform.localScale = localScale;
        }
    }
}