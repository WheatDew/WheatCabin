using UnityEngine;

namespace Opsive.UltimateCharacterController.Networking.Utility
{
    /// <summary>
    /// Contains small network compression functions.
    /// </summary>
    public static class NetworkCompression
    {
        // Specifies the max range that the float value can be when converting the movement value to a short.
        private const float c_MaxMovementFloatValue = 2;
        // Specifies the max range that the float value can be when converting the float value to a short.
        private const float c_MaxGenericFloatValue = 1000;

        /// <summary>
        /// Converts the float movement value into a short.
        /// </summary>
        /// <param name="value">The float movement value (range -c_MaxMovementFloatValue to c_MaxMovementFloatValue).</param>
        /// <returns>The short value.</returns>
        public static short FloatToShortMovement(float value)
        {
            return FloatToShort(value, c_MaxMovementFloatValue);
        }

        /// <summary>
        /// Converts the float value into a short.
        /// </summary>
        /// <param name="value">The float value.</param>
        /// <returns>The short value.</returns>
        public static short FloatToShort(float value)
        {
            if (value > c_MaxGenericFloatValue || value < -c_MaxGenericFloatValue) {
                Debug.LogWarning($"Warning: Float {value} is outside the MaxGenericFloatValue range. Consider increasing this value.");
            }
            return FloatToShort(value, c_MaxGenericFloatValue);
        }

        /// <summary>
        /// Converts the float value into a short.
        /// </summary>
        /// <param name="value">The float value.</param>
        /// <param name="maxValue">The maximum float value.</param>
        /// <returns>The short value.</returns>
        private static short FloatToShort(float value, float maxFloatValue)
        {
            value = Mathf.Clamp(value, -maxFloatValue, maxFloatValue);
            return (short)(value * (short.MaxValue / maxFloatValue));
        }

        /// <summary>
        /// Converts the short movement value into a float.
        /// </summary>
        /// <param name="value">The short movement value.</param>
        /// <returns>The float value (range -c_MaxMovementFloatValue to c_MaxMovementFloatValue).</returns>
        public static float ShortToFloatMovement(short value)
        {
            return ShortToFloat(value, c_MaxMovementFloatValue);
        }

        /// <summary>
        /// Converts the short value into a float.
        /// </summary>
        /// <param name="value">The short value.</param>
        /// <returns>The float value (range -c_MaxGenericFloatValue to c_MaxGenericFloatValue).</returns>
        public static float ShortToFloat(short value)
        {
            return ShortToFloat(value, c_MaxGenericFloatValue);
        }

        /// <summary>
        /// Converts the short value into a float.
        /// </summary>
        /// <param name="value">The short value.</param>
        /// <param name="maxValue">The maximum float value.</param>
        /// <returns>The float value.</returns>
        private static float ShortToFloat(short value, float maxValue)
        {
            return (value / (short.MaxValue / maxValue));
        }
    }
}