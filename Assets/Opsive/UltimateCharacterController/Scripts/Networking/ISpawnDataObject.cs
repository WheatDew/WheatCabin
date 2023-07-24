/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Objects
{
    /// <summary>
    /// Interface which indicates that the object has initialization data that should be sent when the object is spawned.
    /// </summary>
    public interface ISpawnDataObject
    {
        object[] InstantiationData { get; set; }

        /// <summary>
        /// Returns the initialization data that is required when the object spawns. This allows the remote players to initialize the object correctly.
        /// </summary>
        object[] SpawnData();

        /// <summary>
        /// Callback after the object has been spawned.
        /// </summary>
        void ObjectSpawned();
    }
}