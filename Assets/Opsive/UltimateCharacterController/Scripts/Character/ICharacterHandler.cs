/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    public interface ICharacterHandler
    {
        /// <summary>
        /// Returns the position input for the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        void GetPositionInput(out float horizontalMovement, out float forwardMovement);

        /// <summary>
        /// Returns the rotation input for the character.
        /// </summary>
        /// <param name="horizontalMovement">-1 to 1 value specifying the amount of horizontal movement.</param>
        /// <param name="forwardMovement">-1 to 1 value specifying the amount of forward movement.</param>
        /// <param name="deltaYawRotation">Value specifying the number of degrees changed on the local yaw axis.</param>
        void GetRotationInput(float horizontalMovement, float forwardMovement, out float deltaYawRotation);
    }
}