/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Items.Actions.Effect
{
    using Opsive.Shared.Game;
    using System;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// Invoke a Unity Event.
    /// </summary>
    [Serializable]
    public class SpawnPrefab : ItemEffect
    {
        [Tooltip("The prefab to spawn.")]
        [SerializeField] protected ItemPerspectiveProperty<GameObject> m_Prefab;
        [FormerlySerializedAs("m_Parent")]
        [Tooltip("The origin of the spawn, set the the character position if none is specified.")]
        [SerializeField] protected ItemPerspectiveIDObjectProperty<Transform> m_Origin;
        [Tooltip("Parent the instantiated gameobject to the origin?")]
        [SerializeField] protected bool m_ParentToOrigin = true;
        [Tooltip("The positional offset that the object should be spawned.")]
        [SerializeField] protected Vector3 m_LocalPosition;
        [Tooltip("The rotational offset that the object should be spawned.")]
        [SerializeField] protected Vector3 m_LocalRotation;
        [Tooltip("The positional offset that the object should be spawned.")]
        [SerializeField] protected Vector3 m_LocalScale = Vector3.one;
        [Tooltip("Destroy or return the object the the pool after some time. (Ignored if 0 or less).")]
        [SerializeField] protected float m_LifeTime = -1;
        
        public ItemPerspectiveProperty<GameObject> Prefab { get => m_Prefab; set => m_Prefab = value; }
        public Vector3 LocalPosition { get => m_LocalPosition; set => m_LocalPosition = value; }
        public Vector3 LocalRotation { get => m_LocalRotation; set => m_LocalRotation = value; }
        public Vector3 LocalScale { get => m_LocalScale; set => m_LocalScale = value; }
        public ItemPerspectiveIDObjectProperty<Transform> Parent { get => m_Origin; set => m_Origin = value; }
        public float LifeTime { get => m_LifeTime; set => m_LifeTime = value; }

        private GameObject m_SpawnedObject;

        [Shared.Utility.NonSerialized] public GameObject SpawnedGameObject { get => m_SpawnedObject; set => m_SpawnedObject = value; }
        
        protected Action<GameObject> m_DoDestroyObject;
        
        /// <summary>
        /// Initialize the effect.
        /// </summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            m_Prefab.Initialize(m_CharacterItemAction);
            m_Origin.Initialize(m_CharacterItemAction);
            m_DoDestroyObject = DoDestroyObject;
        }

        /// <summary>
        /// Invoke the effect.
        /// </summary>
        protected override void InvokeEffectInternal()
        {
            base.InvokeEffectInternal();
            DoSpawn();
        }

        /// <summary>
        /// Spawn the prefab gameobject.
        /// </summary>
        private void DoSpawn()
        {
            var prefab = m_Prefab.GetValue();
            if (prefab == null) {
                return;
            }

            var origin = m_Origin.GetValue();
            if (origin == null) {
                origin = m_CharacterItemAction.Character.transform;
            }

            m_SpawnedObject = ObjectPoolBase.Instantiate(prefab, origin);
            var spawnedTransform = m_SpawnedObject.transform;
            spawnedTransform.localPosition = m_LocalPosition;
            spawnedTransform.localRotation = Quaternion.Euler(m_LocalRotation);

            if (m_ParentToOrigin == false) {
                m_SpawnedObject.transform.SetParent(null);
            } 
            
            // Scale afterwards, to prevent taking the parent scale if not parented.
            spawnedTransform.localScale = m_LocalScale;
            
            if (m_LifeTime > 0) {
                Scheduler.Schedule(m_LifeTime, m_DoDestroyObject, m_SpawnedObject);
            }
        }

        /// <summary>
        /// Destroy the game object.
        /// </summary>
        /// <param name="obj">The object to destroy.</param>
        private void DoDestroyObject(GameObject obj)
        {
            if (obj == null) { return; }

            if (ObjectPoolBase.IsPooledObject(obj)) {
                ObjectPoolBase.Destroy(obj);
            } else {
                GameObject.Destroy(obj);
            }
        }
    }
}