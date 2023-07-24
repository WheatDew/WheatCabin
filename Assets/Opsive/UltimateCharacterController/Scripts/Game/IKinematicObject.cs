
using UnityEngine;
/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Game
{
    /// <summary>
    /// Interface for any kinematic object that can be moved.
    /// </summary>
    public interface IKinematicObject
    {
        /// <summary>
        /// The index of the object within the Simulation Manager.
        /// </summary>
        int SimulationIndex { set; }

        /// <summary>
        /// The Transform of the object.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Moves the object.
        /// </summary>
        void Move();
    }
}