/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Car
{
    using Opsive.Shared.Events;
    using Opsive.UltimateCharacterController.Demo.UnityStandardAssets.Vehicles.Car;
    using Opsive.UltimateCharacterController.Objects.CharacterAssist;
    using UnityEngine;

    /// <summary>
    /// Provides a sample implementation of the IDriveSource.
    /// </summary>
    public class SkyCar : MonoBehaviour, IDriveSource
    {
        [Tooltip("A reference to the headlights that should turn on when the character enters the car.")]
        [SerializeField] protected GameObject[] m_Headlights;
        [Tooltip("A reference to the colliders that should be disabled when the character enters the car.")]
        [SerializeField] protected GameObject[] m_DisableColliders;
        [Tooltip("The location that the character drives from.")]
        [SerializeField] protected Transform m_DriverLocation;

        private static int s_OpenCloseDoorParameter = Animator.StringToHash("OpenCloseDoor");

        [System.NonSerialized] private GameObject m_GameObject;
        private Transform m_Transform;
        private Animator m_Animator;
        private Rigidbody m_Rigidbody;
        private Collider[] m_Colliders;
        private CarUserControl m_UserControl;
        private CarAudio m_Audio;
        private UltimateCharacterController.Character.Abilities.Drive m_CharacterDriveAbility;
        private int m_HorizontalInputID;
        private bool m_OpenedDoor;

        public GameObject GameObject { get => m_GameObject; }
        public Transform Transform { get => m_Transform; }
        public Transform DriverLocation { get => m_DriverLocation; }
        public Collider[] Colliders { get => m_Colliders; }
        public int AnimatorID { get => 0; }
        public bool PhysicsUpdate { get => true; }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
            m_Transform = transform;
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Colliders = m_GameObject.GetComponentsInChildren<Collider>();
            m_UserControl = GetComponent<CarUserControl>();
            m_Audio = GetComponent<CarAudio>();
            m_HorizontalInputID = Animator.StringToHash("HorizontalInput");
            EnableDisableCar(false);
        }

        /// <summary>
        /// Enables or disables the car components.
        /// </summary>
        /// <param name="enable">Should the car be enabled?</param>
        private void EnableDisableCar(bool enable)
        {
            enabled = m_Audio.enabled = enable;
            if (m_UserControl != null) {
                m_UserControl.enabled = enable;
            }
            m_Rigidbody.isKinematic = !enable;
            for (int i = 0; i < m_Headlights.Length; ++i) {
                m_Headlights[i].SetActive(enable);
            }
            for (int i = 0; i < m_DisableColliders.Length; ++i) {
                m_DisableColliders[i].SetActive(!enable);
            }
        }

        /// <summary>
        /// The character has started to enter the vehicle.
        /// </summary>
        /// <param name="driveAbility">The ability of the character that is entering the vehicle.</param>
        public void EnterVehicle(UltimateCharacterController.Character.Abilities.Drive driveAbility)
        {
            EventHandler.RegisterEvent(driveAbility.GameObject, "OnAnimatorOpenCloseDoor", OpenCloseDoor);
        }

        /// <summary>
        /// Triggers the OpenCloseDoor parameter.
        /// </summary>
        private void OpenCloseDoor()
        {
            m_OpenedDoor = !m_OpenedDoor;
            m_Animator.SetTrigger(s_OpenCloseDoorParameter);
        }

        /// <summary>
        /// The character has entered the vehicle.
        /// </summary>
        /// <param name="driveAbility">The ability of the character that entered the vehicle.</param>
        public void EnteredVehicle(UltimateCharacterController.Character.Abilities.Drive driveAbility)
        {
            m_CharacterDriveAbility = driveAbility;
            EnableDisableCar(true);
        }

        /// <summary>
        /// Updates the animator.
        /// </summary>
        public void Update()
        {
            m_Animator.SetFloat(m_HorizontalInputID, m_CharacterDriveAbility.CharacterAnimatorMonitor.AbilityFloatData, 0, 0);
        }

        /// <summary>
        /// The character has started to exit the vehicle.
        /// </summary>
        /// <param name="driveAbility">The ability of the character that will exit the vehicle.</param>
        public void ExitVehicle(UltimateCharacterController.Character.Abilities.Drive driveAbility)
        {
            EnableDisableCar(false);
        }

        /// <summary>
        /// The character has exited the vehicle.
        /// </summary>
        /// <param name="driveAbility">The ability of the character that exited the vehicle.</param>
        public void ExitedVehicle(UltimateCharacterController.Character.Abilities.Drive driveAbility)
        {
            EventHandler.UnregisterEvent(driveAbility.GameObject, "OnAnimatorOpenCloseDoor", OpenCloseDoor);

            if (m_OpenedDoor) {
                OpenCloseDoor();
            }
        }
    }
}