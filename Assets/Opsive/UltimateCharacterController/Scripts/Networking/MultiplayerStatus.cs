/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking
{
    using UnityEngine;

    /// <summary>
    /// Small ScriptableObject which shows the import window if it has not been shown.
    /// </summary>
    public class MultiplayerStatus : ScriptableObject
    {
        [Tooltip("Does the character controller support multiplayer?")]
        [SerializeField] protected bool m_SupportsMultiplayer;

        public bool SupportsMultiplayer { get { return m_SupportsMultiplayer; } set { m_SupportsMultiplayer = value; } }
    }
}