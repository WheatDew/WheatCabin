/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.UI
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.StateSystem;
    using UnityEngine;

    /// <summary>
    /// The CharacterMonitor component allows for UI elements to mapped to a specific character (allowing for split screen and coop).
    /// </summary>
    public abstract class CharacterMonitor : StateBehavior
    {
        [Tooltip("The character that uses the UI represents. Can be null.")]
        [SerializeField] protected GameObject m_Character;
        [Tooltip("Should this monitor listen to the OnCameraAttachCharacter event?")]
        [SerializeField] protected bool m_AttachToCamera = true;
        [Tooltip("Is the UI visible?")]
        [SerializeField] protected bool m_Visible = true;

        public GameObject Character { get { return m_Character; } set { OnAttachCharacter(value); } }
        public bool Visible { get { return m_Visible; } set { m_Visible = value; ShowUI(m_ShowUI); } }

        protected bool m_StartedActive;
        protected bool m_ShowUI = true;
        protected GameObject m_CameraGameObject;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            AssignCamera();
            
            m_StartedActive = gameObject.activeSelf;
            
            if (m_Character != null) {
                var character = m_Character;
                OnAttachCharacter(null);
                OnAttachCharacter(character);
            } else {
                gameObject.SetActive(false); // Wait for a character in order to activate.
            }
        }

        /// <summary>
        /// Attaches the monitor to the specified character.
        /// </summary>
        /// <param name="character">The character to attach the monitor to.</param>
        protected virtual void OnAttachCharacter(GameObject character)
        {
            if (m_Character != null) {
                StateManager.LinkGameObjects(m_Character, gameObject, false);
                EventHandler.UnregisterEvent<bool>(m_Character, "OnShowUI", ShowUI);
            }

            m_Character = character;

            if (m_Character != null) {
                StateManager.LinkGameObjects(m_Character, gameObject, true);
                EventHandler.RegisterEvent<bool>(m_Character, "OnShowUI", ShowUI);
            }

            // The monitor may be in the process of being destroyed.
            enabled = m_Character != null;
            if (m_StartedActive) {
                ShowUI(m_Character != null);
            }
        }

        /// <summary>
        /// Assigns the camera to the UI Monitor.
        /// </summary>
        private void AssignCamera()
        {
            var foundCamera = Shared.Camera.CameraUtility.FindCamera(m_Character);
            if (foundCamera != null) {
                m_CameraGameObject = foundCamera.gameObject;
                if (m_Character == null) {
                    m_Character = m_CameraGameObject.GetCachedComponent<UltimateCharacterController.Camera.CameraController>().Character;
                }
                EventHandler.RegisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter", OnCameraAttachCharacter);
            }
        }

        /// <summary>
        /// The assigned camera has changed the attached character.
        /// </summary>
        /// <param name="character">The new assigned character for the camera.</param>
        private void OnCameraAttachCharacter(GameObject character)
        {
            if (m_AttachToCamera) {
                OnAttachCharacter(character);
            }
        }

        /// <summary>
        /// Starts the UI.
        /// </summary>
        protected virtual void Start()
        {
            // If the camera GameObject is null then the camera may have been spawned after Awake.
            if (m_CameraGameObject == null) {
                AssignCamera();
            }

            // Disable the UI if it shouldn't be shown.
            if (!CanShowUI()) {
                enabled = false;
            }
        }

        /// <summary>
        /// Shows or hides the UI.
        /// </summary>
        /// <param name="show">Should the UI be shown?</param>
        protected virtual void ShowUI(bool show)
        {
            m_ShowUI = show;
            var canShow = CanShowUI();
            if (canShow != gameObject.activeSelf) {
                gameObject.SetActive(canShow);
            }
        }

        /// <summary>
        /// Can the UI be shown?
        /// </summary>
        /// <returns>True if the UI can be shown.</returns>
        protected virtual bool CanShowUI()
        {
            return m_ShowUI && m_Visible && m_Character != null;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_CameraGameObject != null) {
                EventHandler.UnregisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter", OnCameraAttachCharacter);
            }
            OnAttachCharacter(null);
        }
    }
}