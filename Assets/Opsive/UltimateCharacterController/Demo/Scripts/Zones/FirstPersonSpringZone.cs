/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEngine;

    /// <summary>
    /// Manages the first person springs zone. Allows switching between various spring types.
    /// </summary>
    public class FirstPersonSpringZone : MonoBehaviour, IZoneTrigger
    {
        public enum SpringType { Modern, OldSchool, CrazyCowboy, Astronaut, DrunkPerson, Giant, SpringsOff, None }

        [Tooltip("A reference to the character used for Astronaut and Drunk Person.")]
        [SerializeField] protected GameObject m_DrunkAstronautCharacter;
        [Tooltip("A reference to the character used for Giant.")]
        [SerializeField] protected GameObject m_GiantCharacter;
        [Tooltip("The ItemDefinitions that the character should have before entering the room.")]
        [SerializeField] protected Shared.Inventory.ItemDefinitionBase[] m_ItemDefinitions;
        [Tooltip("The name of the ItemSet that should be equipped.")]
        [StateName][SerializeField] protected string m_ItemSetName;

        private GameObject m_Character;
        private CameraController m_CameraController;
        private SpringType m_ActiveSpring = SpringType.None;
        private bool m_InTrigger;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            var demoManager = Object.FindObjectOfType<DemoManager>();
            m_Character = demoManager.Character;
            m_CameraController = Shared.Camera.CameraUtility.FindCamera(m_Character).GetComponent<CameraController>();
        }

        /// <summary>
        /// Sets the item if it has not already been equipped.
        /// </summary>
        private void Start()
        {
            if (!m_InTrigger) {
                return;
            }
            InitializeItemSet();
        }

        /// <summary>
        /// Activates the specified spring.
        /// </summary>
        /// <param name="springType">The spring that should be activated.</param>
        public void ActivateSpring(SpringType springType)
        {
            if (springType == m_ActiveSpring) {
                return;
            }

            var prevCharacter = GetCharacter();
            if (m_ActiveSpring != SpringType.None) {
                StateManager.SetState(prevCharacter, m_ActiveSpring.ToString(), false);
            }

            m_ActiveSpring = springType;
            var activeCharacter = GetCharacter();
            if (activeCharacter != prevCharacter) {
                prevCharacter.SetActive(false);
                activeCharacter.SetActive(true);
                activeCharacter.GetCachedComponent<UltimateCharacterLocomotion>().SetPositionAndRotation(prevCharacter.transform.position, prevCharacter.transform.rotation);
                m_CameraController.Character = activeCharacter;
            }
            StateManager.SetState(activeCharacter, m_ActiveSpring.ToString(), true);
            EventHandler.ExecuteEvent(activeCharacter, "OnShowUI", springType == SpringType.None);
        }

        /// <summary>
        /// Returns the active character.
        /// </summary>
        /// <returns>The active character.</returns>
        private GameObject GetCharacter()
        {
            if (m_ActiveSpring == SpringType.Giant) {
                return m_GiantCharacter;
            }
            if (m_ActiveSpring == SpringType.Astronaut || m_ActiveSpring == SpringType.DrunkPerson) {
                return m_DrunkAstronautCharacter;
            }
            return m_Character;
        }

        /// <summary>
        /// The other collider has entered the trigger.
        /// </summary>
        /// <param name="other">The collider which entered the trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!other.transform.IsChildOf(m_Character.transform)) {
                return;
            }
            m_InTrigger = true;

            // The character must have the primary item in order for it to be equipped.
            var inventory = m_Character.GetCachedComponent<InventoryBase>();
            for (int i = 0; i < m_ItemDefinitions.Length; ++i) {
                if (m_ItemDefinitions[i] == null) {
                    continue;
                }
                inventory.AddItemIdentifierAmount(m_ItemDefinitions[i].CreateItemIdentifier(), 1);
            }

            InitializeItemSet();

            // First person perspective is required.
            m_CameraController.SetPerspective(true);

            ActivateSpring(SpringType.None);
        }

        /// <summary>
        /// Initializes the primary weapon ItemSet.
        /// </summary>
        private void InitializeItemSet()
        {
            // Ensure the primary weapon is equipped.
            var itemSetManager = m_Character.GetCachedComponent<ItemSetManagerBase>();
            if (itemSetManager.TryEquipItemSet(m_ItemSetName, -1, true, true)) {
                EventHandler.ExecuteEvent(m_Character, "OnCharacterSnapAnimator", false);
                StateManager.SetState(m_Character, "FirstPersonSpringZone", true);
            }
        }

        /// <summary>
        /// The other collider has exited the trigger.
        /// </summary>
        /// <param name="other">The collider which exited the trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!other.transform.IsChildOf(m_Character.transform)) {
                return;
            }

            ExitZone(m_Character);
        }

        /// <summary>
        /// Resets the zone after the character exits.
        /// </summary>
        /// <param name="character">The character that exited the zone.</param>
        public void ExitZone(GameObject character)
        {
            ActivateSpring(SpringType.None);
            m_InTrigger = false;
            StateManager.SetState(m_Character, "FirstPersonSpringZone", false);
        }
    }
}