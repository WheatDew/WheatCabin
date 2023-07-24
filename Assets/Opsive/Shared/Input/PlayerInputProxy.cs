/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Input
{
    using Opsive.Shared.Events;
    using UnityEngine;

    /// <summary>
    /// Acts as a proxy for the PlayerInput class.
    /// </summary>
    public class PlayerInputProxy : MonoBehaviour, IPlayerInput
    {
        [Tooltip("A reference to the PlayerInput component.")]
        [SerializeField] protected PlayerInput m_PlayerInput;

        public PlayerInput PlayerInput { get => m_PlayerInput; set => m_PlayerInput = value; }

        public string HorizontalLookInputName { get => m_PlayerInput.HorizontalLookInputName; set => m_PlayerInput.HorizontalLookInputName = value; }
        public string VerticalLookInputName { get => m_PlayerInput.VerticalLookInputName; set => m_PlayerInput.VerticalLookInputName = value; }
        public string ControllerHorizontalLookInputName { get => m_PlayerInput.ControllerHorizontalLookInputName; set => m_PlayerInput.ControllerHorizontalLookInputName = value; }
        public string ControllerVerticalLookInputName { get => m_PlayerInput.ControllerVerticalLookInputName; set => m_PlayerInput.ControllerVerticalLookInputName = value; }
        public string ActiveHorizontalLookInputName { get => m_PlayerInput.ActiveHorizontalLookInputName; }
        public string ActiveVerticalLookInputName { get => m_PlayerInput.ActiveVerticalLookInputName; }
        public PlayerInput.LookVectorMode LookMode { get => m_PlayerInput.LookMode; set => m_PlayerInput.LookMode = value; }
        public Vector2 LookSensitivity { get => m_PlayerInput.LookSensitivity; set => m_PlayerInput.LookSensitivity = value; }
        public float LookSensitivityMultiplier { get => m_PlayerInput.LookSensitivityMultiplier; set => m_PlayerInput.LookSensitivityMultiplier = value; }
        public int SmoothLookSteps { get => m_PlayerInput.SmoothLookSteps; set => m_PlayerInput.SmoothLookSteps = value; }
        public float SmoothLookWeight { get => m_PlayerInput.SmoothLookWeight; set => m_PlayerInput.SmoothLookWeight = value; }
        public float SmoothExponent { get => m_PlayerInput.SmoothExponent; set => m_PlayerInput.SmoothExponent = value; }
        public float LookAccelerationThreshold { get => m_PlayerInput.LookAccelerationThreshold; set => m_PlayerInput.LookAccelerationThreshold = value; }
        public float DoublePressTapTimeout { get => m_PlayerInput.DoublePressTapTimeout; set => m_PlayerInput.DoublePressTapTimeout = value; }
        public float ControllerConnectedCheckRate { get => m_PlayerInput.ControllerConnectedCheckRate; set => m_PlayerInput.ControllerConnectedCheckRate = value; }
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
        public string ConnectedControllerState { get => m_PlayerInput.ConnectedControllerState; set => m_PlayerInput.ConnectedControllerState = value; }
#endif
        public UnityBoolEvent EnableGameplayInputEvent { get => m_PlayerInput.EnableGameplayInputEvent; set => m_PlayerInput.EnableGameplayInputEvent = value; }

        public Vector2 RawLookVector { set => m_PlayerInput.RawLookVector = value; }
        public Vector2 CurrentLookVector { set => m_PlayerInput.CurrentLookVector = value; }
        public bool ControllerConnected { get => m_PlayerInput.ControllerConnected; }
        public bool DisableOnDeath { get => m_PlayerInput.DisableOnDeath; }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            if (m_PlayerInput == null) {
                m_PlayerInput = GetComponentInChildren<PlayerInput>();

                if (m_PlayerInput == null) {
                    Debug.LogError("Error: Unable to find the PlayerInput component.");
                    enabled = false;
                    return;
                }
            }

            Transform parent;
            // Move the PlayerInput GameObject to a GameObject that will never be disabled.
            var scheduler = FindObjectOfType<Game.SchedulerBase>();
            if (scheduler != null) {
                parent = scheduler.transform;
            } else {
                parent = new GameObject(gameObject.name + "Input").transform;
            }
            m_PlayerInput.transform.parent = parent;
            m_PlayerInput.transform.localPosition = Vector3.zero;
            m_PlayerInput.transform.localRotation = Quaternion.identity;
            m_PlayerInput.transform.localPosition = Vector3.zero;
            m_PlayerInput.RegisterEvents(gameObject);

#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
            StateSystem.StateManager.LinkGameObjects(gameObject, m_PlayerInput.gameObject, true);
#endif
        }

        /// <summary>
        /// Returns true if the button is being pressed.
        /// </summary>
        /// <param name="buttonName">The name of the button.</param>
        /// <returns>True of the button is being pressed.</returns>
        public bool GetButton(string buttonName)
        {
            return m_PlayerInput.GetButton(buttonName);
        }

        /// <summary>
        /// Returns true if the button was pressed this frame.
        /// </summary>
        /// <param name="buttonName">The name of the button.</param>
        /// <returns>True if the button is pressed this frame.</returns>
        public bool GetButtonDown(string buttonName)
        {
            return m_PlayerInput.GetButtonDown(buttonName);
        }

        /// <summary>
        /// Returns true if the button is up.
        /// </summary>
        /// <param name="buttonName">The name of the button.</param>
        /// <returns>True if the button is up.</returns>
        public bool GetButtonUp(string buttonName)
        {
            return m_PlayerInput.GetButtonUp(buttonName);
        }

        /// <summary>
        /// Returns true if a double press occurred (double click or double tap).
        /// </summary>
        /// <param name="buttonName">The button name to check for a double press.</param>
        /// <returns>True if a double press occurred (double click or double tap).</returns>
        public bool GetDoublePress(string buttonName)
        {
            return m_PlayerInput.GetDoublePress(buttonName);
        }

        /// <summary>
        /// Returns true if a tap occurred.
        /// </summary>
        /// <param name="buttonName">The button name to check for a tap.</param>
        /// <returns>True if a tap occurred.</returns>
        public bool GetTap(string buttonName)
        {
            return m_PlayerInput.GetTap(buttonName);
        }

        /// <summary>
        /// Returns true if a long press occurred.
        /// </summary>
        /// <param name="buttonName">The button name to check for a long press.</param>
        /// <param name="duration">The duration of a long press.</param>
        /// <param name="waitForRelease">Indicates if the long press should occur after the button has been released (true) or after the duration (false).</param>
        /// <returns>True if a long press occurred.</returns>
        public bool GetLongPress(string buttonName, float duration, bool waitForRelease)
        {
            return m_PlayerInput.GetLongPress(buttonName, duration, waitForRelease);
        }

        /// <summary>
        /// Returns the value of the axis with the specified name.
        /// </summary>
        /// <param name="buttonName">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        public float GetAxis(string buttonName)
        {
            return m_PlayerInput.GetAxis(buttonName);
        }

        /// <summary>
        /// Returns the value of the raw axis with the specified name.
        /// </summary>
        /// <param name="buttonName">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        public float GetAxisRaw(string buttonName)
        {
            return m_PlayerInput.GetAxisRaw(buttonName);
        }

        /// <summary>
        /// Is a controller connected?
        /// </summary>
        /// <returns>True if a controller is connected.</returns>
        public bool IsControllerConnected()
        {
            return m_PlayerInput.IsControllerConnected();
        }

        /// <summary>
        /// Is the cursor visible?
        /// </summary>
        /// <returns>True if the cursor is visible.</returns>
        public virtual bool IsCursorVisible()
        {
            return m_PlayerInput.IsCursorVisible();
        }

        /// <summary>
        /// Returns the position of the mouse.
        /// </summary>
        /// <returns>The mouse position.</returns>
        public virtual Vector2 GetMousePosition()
        { 
            return m_PlayerInput.GetMousePosition();
        }

        /// <summary>
        /// Returns the look vector. Will apply smoothing if specified otherwise will return the GetAxis value.
        /// </summary>
        /// <param name="smoothed">Should the smoothing value be returned? If false the raw look vector will be returned.</param>
        /// <returns>The current look vector.</returns>
        public virtual Vector2 GetLookVector(bool smoothed)
        {
            return m_PlayerInput.GetLookVector(smoothed);
        }

        /// <summary>
        /// Returns true if the pointer is over a UI element.
        /// </summary>
        /// <returns>True if the pointer is over a UI element.</returns>
        public virtual bool IsPointerOverUI()
        {
            return m_PlayerInput.IsPointerOverUI();
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_PlayerInput != null) {
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
                StateSystem.StateManager.LinkGameObjects(gameObject, m_PlayerInput.gameObject, false);
#endif
                GameObject.Destroy(m_PlayerInput.gameObject);
            }
        }
    }
}