/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Game
{
    using Opsive.Shared.Inventory;
    using Opsive.UltimateCharacterController.Inventory;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Maps the Item Type ID to the Item Type over the network.
    /// </summary>
    public class ItemIdentifierTracker : MonoBehaviour
    {
        private static ItemIdentifierTracker s_Instance;
        private static ItemIdentifierTracker Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("ItemIdentifierTracker").AddComponent<ItemIdentifierTracker>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        [Tooltip("A reference to the available ItemIdentifiers.")]
        [SerializeField] protected ItemCollection m_ItemCollection;

        public ItemCollection ItemCollection { get { return m_ItemCollection; } set { m_ItemCollection = value; } }
        public static ItemCollection ItemCollectionInstance { get { return Instance.m_ItemCollection; } }

        private Dictionary<uint, IItemIdentifier> m_IDItemIdentifierMap = new Dictionary<uint, IItemIdentifier>();

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            if (m_ItemCollection == null || m_ItemCollection.ItemTypes == null) {
                return;
            }

            for (int i = 0; i < m_ItemCollection.ItemTypes.Length; ++i) {
                m_IDItemIdentifierMap.Add(m_ItemCollection.ItemTypes[i].ID, m_ItemCollection.ItemTypes[i]);
            }
        }

        /// <summary>
        /// Returns the ItemIdentifier that belongs to the specified ID.
        /// </summary>
        /// <param name="id">The ID of the ItemIdentifier to retrieve.</param>
        /// <returns>The ItemIdentifier that belongs to the specified ID.</returns>
        public static IItemIdentifier GetItemIdentifier(uint id)
        {
            return Instance.GetItemIdentifierInternal(id);
        }

        /// <summary>
        /// Internal method which returns the ItemIdentifier that belongs to the specified ID.
        /// </summary>
        /// <param name="id">The ID of the ItemIdentifier to retrieve.</param>
        /// <returns>The ItemIdentifier that belongs to the specified ID.</returns>
        private IItemIdentifier GetItemIdentifierInternal(uint id)
        {
            if (m_IDItemIdentifierMap.TryGetValue(id, out var itemIdentifier)) {
                return itemIdentifier;
            }
            return null;
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }
    }
}