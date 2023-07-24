/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using UnityEngine;

    /// <summary>
    /// Interface for components that use trigger to change states.
    /// </summary>
    public interface IZoneTrigger
    {
        /// <summary>
        /// Resets the zone after the character exits.
        /// </summary>
        /// <param name="character">The character that exited the zone.</param>
        void ExitZone(GameObject character);
    }
}