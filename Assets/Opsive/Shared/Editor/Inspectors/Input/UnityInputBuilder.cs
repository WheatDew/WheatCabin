/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors.Input
{
    using UnityEditor;

    /// <summary>
    /// Updates the Unity input manager with the correct button bindings.
    /// </summary>
    public class UnityInputBuilder
    {
        /// <summary>
        /// The elements axis type within the InputManager.
        /// </summary>
        public enum AxisType
        {
            KeyMouseButton, Mouse, Joystick
        }
        /// <summary>
        /// The element's axis number within the InputManager.
        /// </summary>
        public enum AxisNumber
        {
            X, Y, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Eleven, Twelve, Thirteen, Fourteen, Fifteen, Sixteen, Seventeen, Eighteen, Nineteen, Twenty
        }

        /// <summary>
        /// Adds a new axis to the InputManager.
        /// </summary>
        /// <param name="axisProperty">The array of all of the axes.</param>
        /// <param name="name">The name of the new axis.</param>
        /// <param name="negativeButton">The name of the negative button of the new axis.</param>
        /// <param name="positiveButton">The name of the positive button of the new axis.</param>
        /// <param name="altNegativeButton">The name of the alternative negative button of the new axis.</param>
        /// <param name="altPositiveButton">The name of the alternative positive button of the new axis.</param>
        /// <param name="sensitivity">The sensitivity of the new axis.</param>
        /// <param name="gravity">The gravity of the new axis.</param>
        /// <param name="dead">The dead value of the new axis.</param>
        /// <param name="snap">Does the new axis snap?</param>
        /// <param name="invert">Is the axis inverted?</param>
        /// <param name="axisType">The type of axis to add.</param>
        /// <param name="axisNumber">The index of the axis.</param>
        public static void AddInputAxis(SerializedProperty axisProperty, string name, string negativeButton, string positiveButton,
                                string altNegativeButton, string altPositiveButton, float gravity, float dead, float sensitivity, bool snap, bool invert, AxisType axisType, AxisNumber axisNumber)
        {
            var property = FindAxisProperty(axisProperty, name, positiveButton, altPositiveButton, axisType);
            property.FindPropertyRelative("m_Name").stringValue = name;
            property.FindPropertyRelative("negativeButton").stringValue = negativeButton;
            property.FindPropertyRelative("positiveButton").stringValue = positiveButton;
            property.FindPropertyRelative("altNegativeButton").stringValue = altNegativeButton;
            property.FindPropertyRelative("altPositiveButton").stringValue = altPositiveButton;
            property.FindPropertyRelative("gravity").floatValue = gravity;
            property.FindPropertyRelative("dead").floatValue = dead;
            property.FindPropertyRelative("sensitivity").floatValue = sensitivity;
            property.FindPropertyRelative("snap").boolValue = snap;
            property.FindPropertyRelative("invert").boolValue = invert;
            property.FindPropertyRelative("type").intValue = (int)axisType;
            property.FindPropertyRelative("axis").intValue = (int)axisNumber;
        }

        /// <summary>
        /// Removes the axis properties with the specified name.
        /// </summary>
        /// <param name="axisProperty">The array of all of the axes.</param>
        /// <param name="name">The name of the axis.</param>
        /// <param name="positiveButton">The name of the positive button of the new axis.</param>
        /// <param name="axisType">The type of axis.</param>
        public static void RemoveAxisProperty(SerializedProperty axisProperty, string name, string positiveButton, AxisType axisType)
        {
            for (int i = axisProperty.arraySize - 1; i > -1; --i) {
                var property = axisProperty.GetArrayElementAtIndex(i);
                if (property.FindPropertyRelative("m_Name").stringValue.Equals(name) && property.FindPropertyRelative("type").intValue == (int)axisType && 
                    (string.IsNullOrEmpty(positiveButton) || property.FindPropertyRelative("positiveButton").stringValue.Equals(positiveButton) && axisType == AxisType.KeyMouseButton)) {
                    axisProperty.DeleteArrayElementAtIndex(i);
                }
            }
        }

        /// <summary>
        /// Searches for a property with the given name and axis type within the axes property array. If no property is found then a new one will be created.
        /// </summary>
        /// <param name="axisProperty">The array to search through.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="positiveButton">The name of the positive button of the new axis.</param>
        /// <param name="altPositiveButton">The name of the alternative positive button of the new axis.</param>
        /// <param name="axisType">The type of axis that should be found.</param>
        /// <param name="autoCreate">Should a property be automatically created if it does not exist?</param>
        /// <returns>The found axis property.</returns>
        public static SerializedProperty FindAxisProperty(SerializedProperty axisProperty, string name, string positiveButton, string altPositiveButton, AxisType axisType, bool autoCreate = true)
        {
            SerializedProperty foundProperty = null;
            for (int i = 0; i < axisProperty.arraySize; ++i) {
                var property = axisProperty.GetArrayElementAtIndex(i);
                if (property.FindPropertyRelative("m_Name").stringValue.Equals(name) && property.FindPropertyRelative("type").intValue == (int)axisType && 
                    ((string.IsNullOrEmpty(positiveButton) && string.IsNullOrEmpty(altPositiveButton)) || 
                    (!string.IsNullOrEmpty(positiveButton) && property.FindPropertyRelative("positiveButton").stringValue.Equals(positiveButton) && axisType == AxisType.KeyMouseButton) ||
                    (!string.IsNullOrEmpty(altPositiveButton) && property.FindPropertyRelative("altPositiveButton").stringValue.Equals(altPositiveButton) && axisType == AxisType.KeyMouseButton))) {
                    foundProperty = property;
                }
            }

            // If no property was found then create a new one.
            if (autoCreate && foundProperty == null) {
                axisProperty.InsertArrayElementAtIndex(axisProperty.arraySize);
                foundProperty = axisProperty.GetArrayElementAtIndex(axisProperty.arraySize - 1);
            }

            return foundProperty;
        }
    }
}