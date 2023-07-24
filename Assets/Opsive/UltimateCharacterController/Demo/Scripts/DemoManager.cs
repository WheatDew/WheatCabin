/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Demo.UI;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Inventory;
    using Opsive.UltimateCharacterController.Traits;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    /// <summary>
    /// The DemoManager will control the objects in the demo scene as well as the text shown.
    /// </summary>
    public class DemoManager : MonoBehaviour
    {
        /// <summary>
        /// Container for each zone within the demo scene.
        /// </summary>
        [System.Serializable]
        public class DemoZone
        {
            [Tooltip("The zone in the scene.")]
            [SerializeField] protected GameObject m_Zone;
            [Tooltip("The header text.")]
            [SerializeField] protected string m_Header;
            [Tooltip("The image representing the zone.")]
            [SerializeField] protected Sprite m_Sprite;
            [Tooltip("The description text.")]
            [SerializeField] protected string m_Description;

            public GameObject Zone { get => m_Zone; }
            public string Header { get => m_Header; }
            public Sprite Sprite { get => m_Sprite; }
            public string Description { get => m_Description; }

            private int m_Index;
            private ZoneElement m_ZoneElement;
            private Zones.IZoneTrigger[] m_ZoneTriggers;
            private StateSystem.StateTrigger[] m_StateTriggers;

            public int Index { get => m_Index; }
            public ZoneElement ZoneElement { get => m_ZoneElement; set => m_ZoneElement = value; }
            public Zones.IZoneTrigger[] ZoneTriggers { get => m_ZoneTriggers; }
            public StateSystem.StateTrigger[] StateTriggers { get => m_StateTriggers; }

            /// <summary>
            /// Initializes the zone.
            /// </summary>
            /// <param name="index">The index of the DemoZone.</param>
            /// <param name="deactivate">Should the zone be deactivated?</param>
            public void Initialize(int index, bool deactivate)
            {
                m_Index = index;

                // Assign the spawn point so the character will know where to spawn upon death.
                var spawnPoint = m_Zone.GetComponentInChildren<SpawnPoint>();
                if (spawnPoint != null) {
                    spawnPoint.Grouping = index;
                }

                m_ZoneTriggers = m_Zone.GetComponentsInChildren<Zones.IZoneTrigger>(true);
                m_StateTriggers = m_Zone.GetComponentsInChildren<StateSystem.StateTrigger>(true);

                if (deactivate) {
                    m_Zone.SetActive(false);
                }
            }
        }

        [Tooltip("A reference to the character.")]
        [SerializeField] protected GameObject m_Character;
        [Tooltip("Is the character allowed to free roam the scene at the very start?")]
        [SerializeField] protected bool m_FreeRoam;
        [Tooltip("The location the character should spawn.")]
        [SerializeField] protected Transform m_FreeRoamSpawnLocation;
        [Tooltip("A reference to the ZoneSelection component.")]
        [SerializeField] protected ZoneSelection m_ZoneSelection;
        [Tooltip("A reference to the first or third person perspective toggle.")]
        [SerializeField] protected Toggle m_PerspectiveToggle;
        [Tooltip("A reference to the Text component which shows the demo header text.")]
        [SerializeField] protected Shared.UI.Text m_Header;
        [Tooltip("A reference to the Text component which shows the demo description text.")]
        [SerializeField] protected Shared.UI.Text m_Description;
        [Tooltip("A list of all of the zones within the scene.")]
        [SerializeField] protected DemoZone[] m_DemoZones;
        [Tooltip("A reference to the controls UI parent.")]
        [SerializeField] protected GameObject m_ControlsParent;
        [Tooltip("A reference to the keyboard mapping.")]
        [SerializeField] protected GameObject m_KeyboardMapping;
        [Tooltip("A reference to the controller mapping.")]
        [SerializeField] protected GameObject m_ControllerMapping;
        [Tooltip("A reference to the container of the in game UI content.")]
        [SerializeField] protected GameObject m_InGameZoneContent;
        [Tooltip("A reference to the panel which shows the demo text.")]
        [SerializeField] protected GameObject m_InGameZoneDescription;
        [Tooltip("Is this manager part of an add-on?")]
        [SerializeField] protected bool m_AddAllItemsToCharacter;
        [Tooltip("Is this manager part of an add-on?")]
        [SerializeField] protected bool m_AddOnDemoManager;
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
        [Tooltip("Specifies the perspective that the character should start in if there is no perspective selection GameObject.")]
        [SerializeField] protected bool m_DefaultFirstPersonStart = true;
#endif
        [Tooltip("Is this manager part of an add-on?")]
        [SerializeField] protected UnityEvent m_OnCharacterInitialized;

        public bool FreeRoam { get => m_FreeRoam; set => m_FreeRoam = value; }
        public Transform FreeRoamSpawnLocation { get => m_FreeRoamSpawnLocation; set => m_FreeRoamSpawnLocation = value; }
        public Toggle PerspectiveToggle { get => m_PerspectiveToggle; set => m_PerspectiveToggle = value; }
        public DemoZone[] DemoZones { get => m_DemoZones; }

        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private Health m_CharacterHealth;
        private Respawner m_CharacterRespawner;
        private Shared.Input.PlayerInput m_PlayerInput;
        private Dictionary<DemoZoneTrigger, DemoZone> m_DemoZoneTriggerDemoZoneMap = new Dictionary<DemoZoneTrigger, DemoZone>();
        private List<int> m_ActiveZoneIndices = new List<int>();
        private bool m_SelectPerspective = true;
        private int m_LastZoneIndex = -1;

        public GameObject Character { get => m_Character; }
        public int LastZoneIndex { get => m_LastZoneIndex; set 
                {
                if (m_LastZoneIndex != -1) {
                    if (value == -1) {
                        m_DemoZones[m_LastZoneIndex].Zone.SetActive(false);
                    }
                    var zoneTriggers = m_DemoZones[m_LastZoneIndex].ZoneTriggers;
                    if (zoneTriggers != null) {
                        for (int i = 0; i < zoneTriggers.Length; ++i) {
                            zoneTriggers[i].ExitZone(m_Character);
                        }
                    }
                    var stateTriggers = m_DemoZones[m_LastZoneIndex].StateTriggers;
                    if (stateTriggers != null) {
                        for (int i = 0; i < stateTriggers.Length; ++i) {
                            stateTriggers[i].TriggerExit(m_Character);
                        }
                    }
                }
                m_LastZoneIndex = value;
            }
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected virtual void Awake()
        {
#if !FIRST_PERSON_CONTROLLER || !THIRD_PERSON_CONTROLLER
            var demoZones = new List<DemoZone>(m_DemoZones);
            for (int i = demoZones.Count - 1; i > -1; --i) {
                // The demo zone may belong to the other perspective.
                if (demoZones[i].Zone == null) {
                    demoZones.RemoveAt(i);
                }
            }
            m_DemoZones = demoZones.ToArray();
#endif

            for (int i = 0; i < m_DemoZones.Length; ++i) {
                if (m_DemoZones[i].Zone == null) {
                    continue;
                }

                m_DemoZones[i].Initialize(i, !m_AddOnDemoManager);
                var demoZoneTrigger = m_DemoZones[i].Zone.GetComponent<DemoZoneTrigger>();
                if (demoZoneTrigger != null) {
                    m_DemoZoneTriggerDemoZoneMap.Add(m_DemoZones[i].Zone.GetComponent<DemoZoneTrigger>(), m_DemoZones[i]);
                }
            }

            // Enable the UI after the character has spawned.
            if (m_InGameZoneDescription != null) {
                m_InGameZoneDescription?.SetActive(false);
                m_InGameZoneContent?.SetActive(false);
                m_ControlsParent?.SetActive(false);
            }
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
            if (m_PerspectiveToggle != null) {
                m_PerspectiveToggle.isOn = m_DefaultFirstPersonStart;
            }
#endif
        }

        /// <summary>
        /// Initializes the character.
        /// </summary>
        protected virtual void Start()
        {
            var spawnCharacter = true;
            var allCharacters = UnityEngine.Object.FindObjectsOfType<UltimateCharacterLocomotion>(true);
            for (int i = 0; i < allCharacters.Length; ++i) {
                if (allCharacters[i].gameObject == m_Character) {
                    spawnCharacter = false;
                    break;
                }
            }
            if (spawnCharacter) {
                m_Character = Instantiate(m_Character);
            }
            InitializeCharacter(m_Character, !m_FreeRoam, true);

            if (m_FreeRoam) {
                m_SelectPerspective = false;
                if (spawnCharacter) {
                    var characterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
                    characterLocomotion.SetPositionAndRotation(m_FreeRoamSpawnLocation.position, m_FreeRoamSpawnLocation.rotation);
                }
                m_Character.SetActive(true);

                var foundCamera = Shared.Camera.CameraUtility.FindCamera(null);
                var cameraController = foundCamera.GetComponent<UltimateCharacterController.Camera.CameraController>();
                // Ensure the camera starts with the correct view type.
                cameraController.FirstPersonViewTypeFullName = GetViewTypeFullName(true);
                cameraController.ThirdPersonViewTypeFullName = GetViewTypeFullName(false);
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
                cameraController.SetPerspective(m_DefaultFirstPersonStart, true);
#elif FIRST_PERSON_CONTROLLER
                cameraController.SetPerspective(true, true);
#elif THIRD_PERSON_CONTROLLER
                cameraController.SetPerspective(false, true);
#endif
                cameraController.Character = m_Character;

                // Enable the zone that the free roam character starts in.
                var characterLayer = m_Character.GetComponent<CharacterLayerManager>().CharacterLayer;
                for (int i = 0; i < m_DemoZones.Length; ++i) {
                    var boxCollider = m_DemoZones[i].Zone.GetComponent<BoxCollider>();
                    if (Physics.CheckBox(m_DemoZones[i].Zone.transform.TransformPoint(boxCollider.center), boxCollider.size / 2, boxCollider.transform.rotation, characterLayer, QueryTriggerInteraction.Ignore)) {
                        ActivateDemoZone(m_DemoZones[i], false);
                        break;
                    }
                }
                m_ZoneSelection.Container.SetActive(false);
            } else {
                m_Character.SetActive(false);
            }

            m_InGameZoneContent?.SetActive(true);

            // Wait at least a frame for the character to initialize properly.
            Scheduler.Schedule(0.1f, () => m_OnCharacterInitialized?.Invoke());
            
            EventHandler.RegisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
        }

        /// <summary>
        /// Initializes the Demo Manager with the specified character.
        /// </summary>
        /// <param name="character">The character that should be initialized/</param>
        /// <param name="showZoneSelection">Should the zone selection menu be shown?</param>
        /// <param name="teleport">Should the character be teleported to the first demo zone?</param>
        protected void InitializeCharacter(GameObject character, bool showZoneSelection, bool teleport)
        {
            m_Character = character;

            if (m_Character == null) {
                return;
            }

            m_CharacterLocomotion = m_Character.GetComponent<UltimateCharacterLocomotion>();
            m_CharacterHealth = m_Character.GetComponent<Health>();
            m_CharacterRespawner = m_Character.GetComponent<Respawner>();
            m_PlayerInput = m_Character.GetComponent<Shared.Input.PlayerInputProxy>().PlayerInput;

            // Some ViewTypes need a reference to the character bones.
            var cameraController = Shared.Camera.CameraUtility.FindCamera(character).GetComponent<UltimateCharacterController.Camera.CameraController>();
            Animator characterAnimator;
            var modelManager = character.GetComponent<ModelManager>();
            if (modelManager != null) {
                characterAnimator = modelManager.ActiveModel.GetComponent<Animator>();
            } else {
                characterAnimator = character.GetComponentInChildren<AnimatorMonitor>(true).GetComponent<Animator>();
            }
#if FIRST_PERSON_CONTROLLER
            var transformLookViewType = cameraController.GetViewType<FirstPersonController.Camera.ViewTypes.TransformLook>();
            if (transformLookViewType != null) {
                transformLookViewType.MoveTarget = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
                transformLookViewType.RotationTarget = characterAnimator.GetBoneTransform(HumanBodyBones.Hips);
            }
#endif
#if THIRD_PERSON_CONTROLLER
            var lookAtViewType = cameraController.GetViewType<ThirdPersonController.Camera.ViewTypes.LookAt>();
            if (lookAtViewType != null) {
                lookAtViewType.Target = characterAnimator.GetBoneTransform(HumanBodyBones.Head);
            }
            
            // The path is located within the scene. Set it to the spawned character.
            var pseudo3DPath = GameObject.FindObjectOfType<Opsive.UltimateCharacterController.Motion.Path>(true);
            if (pseudo3DPath != null) {
                for (int i = 0; i < m_CharacterLocomotion.MovementTypes.Length; ++i) {
                    if (m_CharacterLocomotion.MovementTypes[i] is ThirdPersonController.Character.MovementTypes.Pseudo3D) {
                        var pseudo3DMovementType = m_CharacterLocomotion.MovementTypes[i] as ThirdPersonController.Character.MovementTypes.Pseudo3D;
                        pseudo3DMovementType.Path = pseudo3DPath;
                        break;
                    }
                }
            }
#endif
            // Optionally add all the items to the character for debugging.
            if (m_AddAllItemsToCharacter) {
                var inventory = m_Character.GetCachedComponent<Inventory>();
                var itemSetManager = m_Character.GetCachedComponent<ItemSetManager>();
                var itemCollection = itemSetManager.ItemCollection;

                for (int i = 0; i < itemCollection.ItemTypes.Length; i++) {
                    var itemType = itemCollection.ItemTypes[i];
                    // Add 15 units of all items.
                    inventory.AddItemIdentifierAmount(itemType, 15);
                }
            }
            
            
            // Rhea should follow the spawned character.
            var followAgent = UnityEngine.Object.FindObjectOfType<Demo.AI.FollowAgent>(true);
            if (followAgent != null) {
                followAgent.Target = character.transform;
            }

            // Disable the demo components if the character is null. This allows for free roaming within the demo scene.
            if (m_FreeRoam) {
                if (m_PerspectiveToggle != null) {
                    m_PerspectiveToggle.gameObject.SetActive(false);
                }

                // The character needs to be assigned to the camera.
                cameraController.SetPerspective(m_CharacterLocomotion.FirstPersonPerspective, true);
                cameraController.Character = m_Character;
                if (m_Character.activeInHierarchy) {
                    EventHandler.ExecuteEvent(m_Character, "OnCharacterSnapAnimator", true);
                }
                return;
            }

            // The cursor needs to be visible.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!showZoneSelection) {
                return;
            }

            m_ZoneSelection.FocusZone(0);
            m_ZoneSelection.Container.SetActive(true);
            if (m_PerspectiveToggle != null) {
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
                m_PerspectiveToggle.gameObject.SetActive(true);
#else
                m_PerspectiveToggle.gameObject.SetActive(false);
#endif
            }
        }

        /// <summary>
        /// Updates the cursor and action text.
        /// </summary>
        private void Update()
        {
            if (!m_AddOnDemoManager) {
                if (m_LastZoneIndex == -1) {
                    // Keep the mouse visible when the selection screen is active.
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                } else if (Input.GetKeyDown(KeyCode.Escape) && !m_ZoneSelection.isActiveAndEnabled) {
                    m_ZoneSelection.ShowMenu(true, false);
                    return;
                }
            }
        }

        /// <summary>
        /// The character has entered a trigger zone.
        /// </summary>
        /// <param name="demoZoneTrigger">The trigger zone that the character entered.</param>
        /// <param name="other">The GameObject that entered the trigger.</param>
        /// <returns>Did the character successfully enter the trigger?</returns>
        public bool EnteredTriggerZone(DemoZoneTrigger demoZoneTrigger, GameObject other)
        {
            var characterLocomotion = other.GetComponentInParent<UltimateCharacterLocomotion>();
            if (characterLocomotion == null || characterLocomotion.gameObject != m_Character) {
                return false;
            }

            if (!m_DemoZoneTriggerDemoZoneMap.TryGetValue(demoZoneTrigger, out var demoZone)) {
                return false;
            }

            if (m_CharacterHealth != null && m_CharacterHealth.Value == 0) {
                return false;
            }

            return ActivateDemoZone(demoZone, false);
        }

        /// <summary>
        /// Activates the specified demo zone.
        /// </summary>
        /// <param name="demoZone">The demo zone to active.</param>
        /// <param name="teleport">Should the character be teleported to the demo zone?</param>
        /// <returns>Did the character successfully enter the trigger?</returns>
        private bool ActivateDemoZone(DemoZone demoZone, bool teleport)
        {
            if (m_LastZoneIndex == demoZone.Index) {
                return false;
            }

            if (m_SelectPerspective) {
                m_SelectPerspective = false;
#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
                SelectStartingPerspective(m_DefaultFirstPersonStart, false);
#elif FIRST_PERSON_CONTROLLER
                SelectStartingPerspective(true, false);
#else
                SelectStartingPerspective(false, false);
#endif
            }

            if (m_ActiveZoneIndices.Count == 0 || m_ActiveZoneIndices[m_ActiveZoneIndices.Count - 1] != demoZone.Index) {
                m_ActiveZoneIndices.Add(demoZone.Index);
            }
            if (m_LastZoneIndex != -1 && !m_AddOnDemoManager) {
                m_DemoZones[m_LastZoneIndex].Zone.SetActive(false);
            }
            LastZoneIndex = demoZone.Index;
            if (m_AddOnDemoManager) {
                ShowText(demoZone.Header, demoZone.Description, true);
            } else {
                ShowText(string.Empty, string.Empty, false);
                demoZone.Zone.SetActive(true);
            }

            if (teleport) {
                var position = Vector3.zero;
                var rotation = Quaternion.identity;
                SpawnPointManager.GetPlacement(m_Character, demoZone.Index, ref position, ref rotation);
                m_CharacterLocomotion.SetPositionAndRotation(position, rotation, true);
            }

            // Set the group after the state so the default state doesn't override the grouping value.
            m_CharacterRespawner.Grouping = demoZone.Index;
            return true;
        }

        /// <summary>
        /// The character has exited a trigger zone.
        /// </summary>
        /// <param name="demoZoneTrigger">The trigger zone that the character exited.</param>
        public void ExitedTriggerZone(DemoZoneTrigger demoZoneTrigger)
        {
            DemoZone demoZone;
            if (!m_DemoZoneTriggerDemoZoneMap.TryGetValue(demoZoneTrigger, out demoZone)) {
                return;
            }
            m_ActiveZoneIndices.Remove(demoZone.Index);
            if (m_ActiveZoneIndices.Count > 0 && m_LastZoneIndex != m_ActiveZoneIndices[m_ActiveZoneIndices.Count - 1]) {
                ActivateDemoZone(m_DemoZones[m_ActiveZoneIndices[m_ActiveZoneIndices.Count - 1]], false);
            }
            if (m_AddOnDemoManager) {
                ShowText(string.Empty, string.Empty, false);
            }
        }

        /// <summary>
        /// Teleports the character to the specified zone index.
        /// </summary>
        /// <param name="index">The index of the zone.</param>
        /// <returns>True if the character was teleported.</returns>
        public bool Teleport(int index)
        {
            return ActivateDemoZone(m_DemoZones[index], true);
        }

        /// <summary>
        /// Sets the starting perspective on the character.
        /// </summary>
        /// <param name="firstPersonPerspective">Should the character start in a first person perspective?</param>
        /// <param name="teleport">Should the character be teleported to the demo zone?</param>
        protected void SelectStartingPerspective(bool firstPersonPerspective, bool teleport)
        {
            if (m_DemoZones.Length > 0) {
                // The cursor should be hidden to start the demo.
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Set the starting position.
                m_LastZoneIndex = -1;
                ActivateDemoZone(m_DemoZones[m_ZoneSelection.SelectedZoneIndex], teleport);
                // The character should be activated after positioned so the fall surface impacts don't play.
                m_Character.SetActive(true);
                EventHandler.ExecuteEvent(m_Character, "OnEnableGameplayInput", true);
            }

            // Set the perspective on the camera.
            var foundCamera = Shared.Camera.CameraUtility.FindCamera(null);
            var cameraController = foundCamera.GetComponent<UltimateCharacterController.Camera.CameraController>();
            // Ensure the camera starts with the correct view type.
            cameraController.FirstPersonViewTypeFullName = GetViewTypeFullName(true);
            cameraController.ThirdPersonViewTypeFullName = GetViewTypeFullName(false);
            cameraController.SetPerspective(firstPersonPerspective, true);
            cameraController.Character = m_Character;

            if (m_DemoZones.Length > 0) {
                if (m_PerspectiveToggle != null) {
                    m_PerspectiveToggle.gameObject.SetActive(false);
                }
                m_ZoneSelection.ShowMenu(false, false);
                m_InGameZoneContent.SetActive(true);
            }
        }

        /// <summary>
        /// Returns the full name of the view type for the specified perspective.
        /// </summary>
        /// <param name="firstPersonPerspective">Should the first person perspective be returned?</param>
        /// <returns>The full name of the view type for the specified perspective.</returns>
        protected virtual string GetViewTypeFullName(bool firstPersonPerspective)
        {
            return firstPersonPerspective ? "Opsive.UltimateCharacterController.FirstPersonController.Camera.ViewTypes.Combat" :
                                            "Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes.Adventure";
        }

        /// <summary>
        /// The zone selection menu has been opened or closed.
        /// </summary>
        /// <param name="open">Was the menu opened?</param>
        public void ZoneSelectionChange(bool open)
        {
            m_InGameZoneContent?.SetActive(!open);
        }

#if FIRST_PERSON_CONTROLLER && THIRD_PERSON_CONTROLLER
        /// <summary>
        /// The perspective toggle has changed.
        /// </summary>
        /// <param name="value">The new value of the perspective toggle.</param>
        public void PerspectiveChanged(bool value)
        {
            m_DefaultFirstPersonStart = value;
        }
#endif

        /// <summary>
        /// Shows the text in the UI with the specified header and description.
        /// </summary>
        /// <param name="header">The header that should be shown.</param>
        /// <param name="description">The description that should be shown.</param>
        /// <param name="show">Should the text be shown?</param>
        public void ShowText(string header, string description, bool show)
        {
            if (m_InGameZoneDescription == null) {
                return;
            }

            if (!show && !string.IsNullOrEmpty(m_Header.text) && !string.IsNullOrEmpty(header) && m_Header.text != header) {
                return;
            }

            if (!show) {
                m_InGameZoneDescription.SetActive(false);
                m_Header.text = string.Empty;
                return;
            }

            m_InGameZoneDescription.SetActive(true);
            m_Header.text = header;
            m_Description.text = description.Replace("{AssetName}", AssetInfo.Name);
        }

        /// <summary>
        /// Shows or hides the controls.
        /// </summary>
        /// <param name="show">Should the controls be shown?</param>
        public void ShowHideControls(bool show)
        {
            m_ControlsParent.SetActive(show);
            m_ZoneSelection.gameObject.SetActive(!show);
            if (!show) {
                return;
            }
            m_KeyboardMapping.SetActive(!m_PlayerInput.ControllerConnected);
            m_ControllerMapping.SetActive(m_PlayerInput.ControllerConnected);
        }

        /// <summary>
        /// The character has died.
        /// </summary>
        /// <param name="position">The position of the force.</param>
        /// <param name="force">The amount of force which killed the character.</param>
        /// <param name="attacker">The GameObject that killed the character.</param>
        private void OnDeath(Vector3 position, Vector3 force, GameObject attacker)
        {
            if (m_LastZoneIndex != -1) {
                var zoneTriggers = m_DemoZones[m_LastZoneIndex].ZoneTriggers;
                if (zoneTriggers != null) {
                    for (int i = 0; i < zoneTriggers.Length; ++i) {
                        zoneTriggers[i].ExitZone(m_Character);
                    }
                }
                var stateTriggers = m_DemoZones[m_LastZoneIndex].StateTriggers;
                if (stateTriggers != null) {
                    for (int i = 0; i < stateTriggers.Length; ++i) {
                        stateTriggers[i].TriggerExit(m_Character);
                    }
                }
            }
        }

        /// <summary>
        /// Quits the project.
        /// </summary>
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// The manager has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<Vector3, Vector3, GameObject>(m_Character, "OnDeath", OnDeath);
        }
    }
}