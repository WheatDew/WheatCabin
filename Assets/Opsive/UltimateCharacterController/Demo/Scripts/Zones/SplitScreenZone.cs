/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;

    /// <summary>
    /// Sets up the demo zone to be in split screen mode when the character enters the trigger.
    /// </summary>
    public class SplitScreenZone : MonoBehaviour, IZoneTrigger
    {
        [Tooltip("The LayerMask that the trigger can set the state of.")]
        [SerializeField] protected LayerMask m_LayerMask = 1 << LayerManager.Character;
        [Tooltip("The cameras that should change into split screen mode.")]
        [SerializeField] protected Camera[] m_Cameras;
        [Tooltip("A reference to the UI Monitors used by the main character.")]
        [SerializeField] protected GameObject m_UIMonitors;
        [Tooltip("The objects that should be disabled when the character is not in the trigger.")]
        [SerializeField] protected GameObject[] m_DisableObjects;
        [Tooltip("The name of the ItemSet that should be equipped.")]
        [SerializeField] protected string m_ItemSetName;

        private Transform m_Transform;

        /// <summary>
        /// Inititalize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;

            for (int i = 0; i < m_DisableObjects.Length; ++i) {
                m_DisableObjects[i].SetActive(false);
            }
        }

        /// <summary>
        /// The other collider has entered the trigger.
        /// </summary>
        /// <param name="other">The collider which entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }

            for (int i = 0; i < m_Transform.childCount; ++i) {
                m_Transform.GetChild(i).gameObject.SetActive(true);
            }
            for (int i = 0; i < m_Cameras.Length; ++i) {
                if (m_Cameras[i] == null) {
                    continue;
                }
                m_Cameras[i].rect = new Rect(0, 0, 0.5f, 1);
            }
            m_UIMonitors.SetActive(false);

            // Items are not setup with the correct offsets for a split screen perspective. Prevent any items from being equipped.
            var character = other.gameObject.GetComponentInParent<Character.UltimateCharacterLocomotion>().gameObject;
            var itemSetManager = character.GetCachedComponent<ItemSetManagerBase>();
            itemSetManager.TryEquipItemSet(m_ItemSetName, -1, true, false);
        }

        /// <summary>
        /// The other collider has exited the trigger.
        /// </summary>
        /// <param name="other">The collider which exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!MathUtility.InLayerMask(other.gameObject.layer, m_LayerMask)) {
                return;
            }

            ExitZone(null);
        }

        /// <summary>
        /// Resets the zone after the character exits.
        /// </summary>
        /// <param name="character">The character that exited the zone.</param>
        public void ExitZone(GameObject character)
        {
            for (int i = 0; i < m_DisableObjects.Length; ++i) {
                m_DisableObjects[i].SetActive(false);
            }
            for (int i = 0; i < m_Cameras.Length; ++i) {
                if (m_Cameras[i] == null) {
                    continue;
                }
                m_Cameras[i].rect = new Rect(0, 0, 1, 1);
            }
            m_UIMonitors.SetActive(true);
        }
    }
}