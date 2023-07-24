/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Networking
{
    /// <summary>
    /// Contains information about the object on the network.
    /// </summary>
    public interface INetworkInfo
    {
        /// <summary>
        /// Is the networking implementation server authoritative?
        /// </summary>
        /// <returns>True if the network transform is server authoritative.</returns>
        bool IsServerAuthoritative();

        /// <summary>
        /// Is the game instance on the server?
        /// </summary>
        /// <returns>True if the game instance is on the server.</returns>
        bool IsServer();

        /// <summary>
        /// Does the network instance have authority?
        /// </summary>
        /// <returns>True if the instance has authority.</returns>
        bool HasAuthority();

        /// <summary>
        /// Is the character the local player?
        /// </summary>
        /// <returns>True if the character is the local player.</returns>
        bool IsLocalPlayer();
    }
}