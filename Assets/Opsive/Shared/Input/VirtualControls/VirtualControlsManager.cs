/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Input.VirtualControls
{
    using Opsive.Shared.Events;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// Coordinates all of the virtual controls. All of the virtual controls must be a child of the VirtualControlsManager GameObject.
    /// </summary>
    public class VirtualControlsManager : MonoBehaviour
    {
        [Tooltip("The character used by the virtual input. Can be null.")]
        [SerializeField] protected GameObject m_Character;
        [Tooltip("Should the controls respond to axis input using the Unity Input Manager?")]
        [SerializeField] protected bool m_AllowAxisInput;
        [Tooltip("Should the controls respond to button input using the Unity Input Manager?")]
        [SerializeField] protected bool m_AllowButtonInput;

        public GameObject Character { get { return m_Character; } set { OnAttachCharacter(value);  } }
        public bool AllowAxisInput { get { return m_AllowAxisInput; } set { m_AllowAxisInput = value; } }
        public bool AllowButtonInput { get { return m_AllowButtonInput; } set { m_AllowButtonInput = value; } }

        [System.NonSerialized] private GameObject m_GameObject;
        private GameObject m_CameraGameObject;
        private Dictionary<string, VirtualControl> m_NameVirtualControlsMap = new Dictionary<string, VirtualControl>();

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected virtual void Awake()
        {
            m_GameObject = gameObject;
            if (m_Character == null) {
                var foundCamera = Shared.Camera.CameraUtility.FindCamera(null);
                if (foundCamera != null) {
                    m_CameraGameObject = foundCamera.gameObject;
                    EventHandler.RegisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter", OnAttachCharacter);
                }
                m_GameObject.SetActive(false); // Wait for a character in order to activate.
            } else {
                var character = m_Character;
                m_Character = null; // Set the character to null so the assignment will occur.
                OnAttachCharacter(character);
            }
        }

        /// <summary>
        /// Attaches the component to the specified character.
        /// </summary>
        /// <param name="character">The handler to attach the camera to.</param>
        private void OnAttachCharacter(GameObject character)
        {
            if (character == m_Character) {
                return;
            }

            if (m_Character != null) {
                var playerInput = m_Character.GetComponent<IPlayerInput>();
                if (playerInput is PlayerInputProxy) {
                    playerInput = (playerInput as PlayerInputProxy).PlayerInput;
                }
                var unityInput = playerInput as UnityInput;
                if (unityInput == null) {
                    m_GameObject.SetActive(false);
                    return;
                }

                unityInput.UnegisterVirtualControlsManager();
                EventHandler.UnregisterEvent<UnityInput, bool>(unityInput.gameObject, "OnUnityInputTypeChanged", OnInputTypeChange);
            }

            m_Character = character;

            var activateGameObject = false;
            if (character != null) {
                var playerInput = m_Character.GetComponent<IPlayerInput>();
                if (playerInput is PlayerInputProxy) {
                    playerInput = (playerInput as PlayerInputProxy).PlayerInput;
                }
                var unityInput = playerInput as UnityInput;
                if (unityInput == null) {
                    Debug.LogError($"Error: The character {m_Character.name} has no UnityInput component.");
                    m_GameObject.SetActive(false);
                    return;
                }

                // If the virtual controls weren't registered then the virtual input type isn't selected.
                activateGameObject = unityInput.RegisterVirtualControlsManager(this);
                EventHandler.RegisterEvent<UnityInput, bool>(unityInput.gameObject, "OnUnityInputTypeChanged", OnInputTypeChange);
            }

            m_GameObject.SetActive(activateGameObject);
        }

        /// <summary>
        /// Associates the input name with the virtual control object.
        /// </summary>
        /// <param name="inputName">The name to associate the virtual control object with.</param>
        /// <param name="virtualControl">The object to associate with the name.</param>
        public void RegisterVirtualControl(string inputName, VirtualControl virtualControl)
        {
            m_NameVirtualControlsMap.Add(inputName, virtualControl);
        }

        /// <summary>
        /// Callback when the UnityInput.ForceInput value has changed.
        /// </summary>
        /// <param name="unityInput">A reference to the UnityInput component.</param>
        /// <param name="useVirtualContols">Are the virtual controls now being used?</param>
        private void OnInputTypeChange(UnityInput unityInput, bool useVirtualContols)
        {
            if (useVirtualContols) {
                unityInput.RegisterVirtualControlsManager(this);
            } else {
                unityInput.UnegisterVirtualControlsManager();
            }
            m_GameObject.SetActive(useVirtualContols);
        }

        /// <summary>
        /// Returns if the button is true with the specified ButtonAction.
        /// </summary>
        /// <param name="buttonName">The name of the button.</param>
        /// <param name="action">The type of action to check.</param>
        /// <returns>The status of the action.</returns>
        public bool GetButton(string buttonName, InputBase.ButtonAction action)
        {
            VirtualControl virtualControl;
            if (!m_NameVirtualControlsMap.TryGetValue(buttonName, out virtualControl)) {
                return false;
            }

            var value = virtualControl.GetButton(action);
            if (value || !m_AllowButtonInput || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())) {
                return value;
            }
            if (action == InputBase.ButtonAction.GetButton) {
                return Input.GetButton(buttonName);
            }
            if (action == InputBase.ButtonAction.GetButtonDown) {
                return Input.GetButtonDown(buttonName);
            }
            return Input.GetButtonUp(buttonName);
        }

        /// <summary>
        /// Returns the axis of the specified button.
        /// </summary>
        /// <param name="axisName">The name of the axis.</param>
        /// <returns>The axis value.</returns>
        public float GetAxis(string axisName)
        {
            VirtualControl virtualControl;
            if (!m_NameVirtualControlsMap.TryGetValue(axisName, out virtualControl)) {
                return 0;
            }

            var value = virtualControl.GetAxis(axisName);
            if (value != 0 || !m_AllowAxisInput || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())) {
                return value;
            }
            return Input.GetAxis(axisName);
        }

        /// <summary>
        /// Removes the association with the object specified by the input name.
        /// </summary>
        /// <param name="inputName">The name of the object to remove association with.</param>
        public void UnregisterVirtualControl(string inputName)
        {
            m_NameVirtualControlsMap.Remove(inputName);
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_CameraGameObject != null) {
                EventHandler.UnregisterEvent<GameObject>(m_CameraGameObject, "OnCameraAttachCharacter", OnAttachCharacter);
            }
        }
    }
}