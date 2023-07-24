/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Items;
    using Opsive.UltimateCharacterController.Traits;
    using UnityEngine;

    /// <summary>
    /// Binds an ItemMonitor and an AttributeMonitor together to show the Attributes that a Character equips at runtime.
    /// This component requires a ItemMonitor and a Attribute Monitor to work.
    /// </summary>
    public class ItemAttributeMonitorBinding : MonoBehaviour
    {
        [Tooltip("The Item Monitor that monitors the tem that should be boudn to the Attribute Monitor.")]
        [SerializeField] protected ItemMonitor m_ItemMonitor;
        [Tooltip("The Attribute Monitors that will monitor the attributes of the monitored item.")]
        [SerializeField] protected AttributeMonitor[] m_AttributeMonitors;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            if (m_ItemMonitor == null) {
                m_ItemMonitor = GetComponent<ItemMonitor>();
                if (m_ItemMonitor == null) {
                    Debug.LogError("Error: The ItemAttributeMonitorBinding component must be defined or exist on the same GameObject as the ItemMonitor.");
                    return;
                }
            }

            for (int i = 0; i < m_AttributeMonitors.Length; i++) {
                if (m_AttributeMonitors[i] == null) {
                    Debug.LogWarning("An Attribute Monitor in the ItemAttributeMonitorBinding is null.", gameObject);
                }
            }
            
            EventHandler.RegisterEvent<CharacterItem, CharacterItem>(m_ItemMonitor.gameObject, "OnMonitoredCharacterItemChanged", OnMonitoredCharacterItemChanged);
        }

        /// <summary>
        /// The inventory has added the specified item.
        /// </summary>
        /// <param name="previousItem">The previous monitored item.</param>
        /// <param name="newItem">The new monitored item.</param>
        private void OnMonitoredCharacterItemChanged(CharacterItem previousItem, CharacterItem newItem)
        {
            var attributeManager = newItem?.gameObject?.GetCachedComponent<AttributeManager>();
            
            for (int i = 0; i < m_AttributeMonitors.Length; i++) {
                if (m_AttributeMonitors[i] == null) {
                    Debug.LogWarning("An Attribute Monitor in the ItemAttributeMonitorBinding is null.", gameObject);
                    continue;
                }
                m_AttributeMonitors[i].AttributeManager = attributeManager;
            }
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_ItemMonitor == null) {
                return;
            }

            EventHandler.UnregisterEvent<CharacterItem, CharacterItem>(m_ItemMonitor.gameObject, "OnMonitoredCharacterItemChanged", OnMonitoredCharacterItemChanged);
        }
    }
}