/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Objects.CharacterAssist
{
    using UnityEngine;

    /// <summary>
    /// Interface for any object that can be driven.
    /// </summary>
    public interface IDriveSource
    {
        /// <summary>
        /// The GameObject of the vehicle.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// The Transform of the vehicle.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// The location that the character drives the vehicle from.
        /// </summary>
        Transform DriverLocation { get; }

        /// <summary>
        /// The vehicle colliders.
        /// </summary>
        Collider[] Colliders { get; }

        /// <summary>
        /// The unique identifier of the object. This value is used within the AbilityIntData parameter of the character's animator.
        /// </summary>
        int AnimatorID { get; }

        /// <summary>
        /// Does the vehicle update during the physics update loop?
        /// </summary>
        bool PhysicsUpdate { get; }

        /// <summary>
        /// The character has started to enter the vehicle.
        /// </summary>
        /// <param name="driveAbility">The ability of the character that entered the vehicle.</param>
        void EnterVehicle(Character.Abilities.Drive driveAbility);

        /// <summary>
        /// The character has entered the vehicle.
        /// </summary>
        /// <param name="driveAbility">The ability of the character that exited the vehicle.</param>
        void EnteredVehicle(Character.Abilities.Drive driveAbility);

        /// <summary>
        /// The character has started to exit the vehicle.
        /// </summary>
        /// <param name="driveAbility">The ability of the  character that is exiting the vehicle.</param>
        void ExitVehicle(Character.Abilities.Drive driveAbility);

        /// <summary>
        /// The character has exited the vehicle.
        /// </summary>
        /// <param name="driveAbility">The ability of the character that exited the vehicle.</param>
        void ExitedVehicle(Character.Abilities.Drive driveAbility);
    }
}