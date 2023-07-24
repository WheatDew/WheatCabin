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
    /// Interface which represents a component that acts as player input.
    /// </summary>
    public interface IPlayerInput
    {
        GameObject gameObject { get; }
        string HorizontalLookInputName { get; set; }
        string VerticalLookInputName { get; set; }
        string ControllerHorizontalLookInputName { get; set; }
        string ControllerVerticalLookInputName { get; set; }
        string ActiveHorizontalLookInputName { get; }
        string ActiveVerticalLookInputName { get; }
        PlayerInput.LookVectorMode LookMode { get; set; }
        Vector2 LookSensitivity { get; set; }
        float LookSensitivityMultiplier { get; set; }
        int SmoothLookSteps { get; set; }
        float SmoothLookWeight { get; set; }
        float SmoothExponent { get; set; }
        float LookAccelerationThreshold { get; set; }
        float DoublePressTapTimeout { get; set; }
        float ControllerConnectedCheckRate { get; set; }
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
        string ConnectedControllerState { get; set; }
#endif
        UnityBoolEvent EnableGameplayInputEvent { get; set; }

        Vector2 RawLookVector { set; }
        Vector2 CurrentLookVector { set; }
        bool ControllerConnected { get; }
        bool DisableOnDeath { get; }

        /// <summary>
        /// Returns true if the button is being pressed.
        /// </summary>
        /// <param name="buttonName">The name of the button.</param>
        /// <returns>True of the button is being pressed.</returns>
        bool GetButton(string buttonName);

        /// <summary>
        /// Returns true if the button was pressed this frame.
        /// </summary>
        /// <param name="buttonName">The name of the button.</param>
        /// <returns>True if the button is pressed this frame.</returns>
        bool GetButtonDown(string buttonName);

        /// <summary>
        /// Returns true if the button is up.
        /// </summary>
        /// <param name="buttonName">The name of the button.</param>
        /// <returns>True if the button is up.</returns>
        bool GetButtonUp(string buttonName);

        /// <summary>
        /// Returns true if a double press occurred (double click or double tap).
        /// </summary>
        /// <param name="buttonName">The button name to check for a double press.</param>
        /// <returns>True if a double press occurred (double click or double tap).</returns>
        bool GetDoublePress(string buttonName);

        /// <summary>
        /// Returns true if a tap occurred.
        /// </summary>
        /// <param name="buttonName">The button name to check for a tap.</param>
        /// <returns>True if a tap occurred.</returns>
        bool GetTap(string buttonName);

        /// <summary>
        /// Returns true if a long press occurred.
        /// </summary>
        /// <param name="buttonName">The button name to check for a long press.</param>
        /// <param name="duration">The duration of a long press.</param>
        /// <param name="waitForRelease">Indicates if the long press should occur after the button has been released (true) or after the duration (false).</param>
        /// <returns>True if a long press occurred.</returns>
        bool GetLongPress(string buttonName, float duration, bool waitForRelease);

        /// <summary>
        /// Returns the value of the axis with the specified name.
        /// </summary>
        /// <param name="buttonName">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        float GetAxis(string buttonName);

        /// <summary>
        /// Returns the value of the raw axis with the specified name.
        /// </summary>
        /// <param name="buttonName">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        float GetAxisRaw(string buttonName);

        /// <summary>
        /// Is a controller connected?
        /// </summary>
        /// <returns>True if a controller is connected.</returns>
        bool IsControllerConnected();

        /// <summary>
        /// Is the cursor visible?
        /// </summary>
        /// <returns>True if the cursor is visible.</returns>
        bool IsCursorVisible();

        /// <summary>
        /// Returns the position of the mouse.
        /// </summary>
        /// <returns>The mouse position.</returns>
        Vector2 GetMousePosition();

        /// <summary>
        /// Returns the look vector. Will apply smoothing if specified otherwise will return the GetAxis value.
        /// </summary>
        /// <param name="smoothed">Should the smoothing value be returned? If false the raw look vector will be returned.</param>
        /// <returns>The current look vector.</returns>
        Vector2 GetLookVector(bool smoothed);

        /// <summary>
        /// Returns true if the pointer is over a UI element.
        /// </summary>
        /// <returns>True if the pointer is over a UI element.</returns>
        bool IsPointerOverUI();
    }
}