/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Utility
{
    using Opsive.Shared.Editor.Inspectors.Input;
    using UnityEditor;

    /// <summary>
    /// Updates the Unity Input Builder to create the correct button bindings.
    /// </summary>
    public class CharacterInputBuilder
    {
        /// <summary>
        /// Update the Input Manager to add all of the correct controls.
        /// </summary>
        public static void UpdateInputManager()
        {
            var serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axisProperty = serializedObject.FindProperty("m_Axes");

            // Cleanup from the default axis.
            UnityInputBuilder.RemoveAxisProperty(axisProperty, "Mouse X", string.Empty, UnityInputBuilder.AxisType.Joystick);
            UnityInputBuilder.RemoveAxisProperty(axisProperty, "Mouse Y", string.Empty, UnityInputBuilder.AxisType.Joystick);
            UnityInputBuilder.RemoveAxisProperty(axisProperty, "Fire1", "joystick button 0", UnityInputBuilder.AxisType.KeyMouseButton);
            UnityInputBuilder.RemoveAxisProperty(axisProperty, "Fire2", "joystick button 1", UnityInputBuilder.AxisType.KeyMouseButton);
            UnityInputBuilder.RemoveAxisProperty(axisProperty, "Fire3", string.Empty, UnityInputBuilder.AxisType.KeyMouseButton);
            UnityInputBuilder.RemoveAxisProperty(axisProperty, "Jump", "joystick button 3", UnityInputBuilder.AxisType.KeyMouseButton);

            // Unity defined axis:
            UnityInputBuilder.AddInputAxis(axisProperty, "Horizontal", "left", "right", "a", "d", 1000, 0.001f, 3, true, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Vertical", "down", "up", "s", "w", 1000, 0.001f, 3, true, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Fire1", "", "left ctrl", "", "mouse 0", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Fire2", "", "", "", "mouse 1", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Fire3", "", "", "", "mouse 2", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Jump", "", "space", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Mouse X", "", "", "", "", 0, 0, 0.1f, false, false, UnityInputBuilder.AxisType.Mouse, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Mouse Y", "", "", "", "", 0, 0, 0.1f, false, false, UnityInputBuilder.AxisType.Mouse, UnityInputBuilder.AxisNumber.Y);
            UnityInputBuilder.AddInputAxis(axisProperty, "Mouse ScrollWheel", "", "", "", "", 0, 0, 0.1f, false, false, UnityInputBuilder.AxisType.Mouse, UnityInputBuilder.AxisNumber.Three);
            UnityInputBuilder.AddInputAxis(axisProperty, "Horizontal", "", "", "", "", 1000, 0.19f, 1, false, false, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Vertical", "", "", "", "", 1000, 0.19f, 1, false, true, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.Y);
            UnityInputBuilder.AddInputAxis(axisProperty, "Fire1", "", "", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.Ten);
            UnityInputBuilder.AddInputAxis(axisProperty, "Fire2", "", "", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.Nine);
            UnityInputBuilder.AddInputAxis(axisProperty, "Fire3", "", "joystick button 2", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Jump", "", "joystick button 0", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);

            // New axis:
            UnityInputBuilder.AddInputAxis(axisProperty, "Controller X", "", "", "", "", 0, 0.19f, 1, false, false, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.Four);
            UnityInputBuilder.AddInputAxis(axisProperty, "Controller Y", "", "", "", "", 0, 0.19f, 1, false, true, UnityInputBuilder.AxisType.Joystick, UnityInputBuilder.AxisNumber.Five);
            UnityInputBuilder.AddInputAxis(axisProperty, "Alt Horizontal", "q", "e", "", "", 1000, 0.19f, 3, true, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Change Speeds", "", "left shift", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Crouch", "", "c", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Crouch", "", "joystick button 9", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Toggle Perspective", "", "v", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Toggle Perspective", "", "joystick button 8", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Toggle Item Equip", "", "t", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Next Item", "", "e", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Next Item", "", "joystick button 3", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Previous Item", "", "q", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip First Item", "", "1", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Second Item", "", "2", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Third Item", "", "3", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Fourth Item", "", "4", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Fifth Item", "", "5", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Sixth Item", "", "6", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Seventh Item", "", "7", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Eighth Item", "", "8", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Ninth Item", "", "9", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Equip Tenth Item", "", "0", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Reload", "", "r", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Reload", "", "joystick button 2", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Drop", "", "y", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Grenade", "", "g", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Grenade", "", "joystick button 5", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Lean", "x", "z", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "SecondaryUse", "", "b", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "SecondaryUse", "", "joystick button 4", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Action", "", "f", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "Action", "", "joystick button 1", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "PreviousModuleSwitcherGroup", "", "h", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "NextModuleSwitcherGroup", "", "j", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "PreviousModuleSwitchState", "", "n", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            UnityInputBuilder.AddInputAxis(axisProperty, "NextModuleSwitchState", "", "m", "", "", 1000, 0.001f, 1000, false, false, UnityInputBuilder.AxisType.KeyMouseButton, UnityInputBuilder.AxisNumber.X);
            
            serializedObject.ApplyModifiedProperties();

            UnityEngine.Debug.Log("The input manager has been updated.");
        }
    }
}