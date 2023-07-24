/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Character
{
    using Opsive.Shared.Networking;
    using Opsive.UltimateCharacterController.Character;
    using UnityEngine;

    /// <summary>
    /// Enables the UltimateCharacterLocomotionHandler to be aware of the character on the network.
    /// </summary>
    public class NetworkCharacterLocomotionHandler : UltimateCharacterLocomotionHandler
    {
        [Tooltip("Should the camera be attached to the local player?")]
        [SerializeField] protected bool m_AttachCamera = true;

        private INetworkInfo m_NetworkInfo;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            m_NetworkInfo = GetComponent<INetworkInfo>();
            m_CharacterLocomotion.enabled = false;
        }

        /// <summary>
        /// Determines if the handler should be enabled.
        /// </summary>
        private void Start()
        {
            if (m_NetworkInfo.IsLocalPlayer()) {
                if (m_AttachCamera) {
                    var camera = Shared.Camera.CameraUtility.FindCamera(gameObject);
                    if (camera != null) {
                        camera.GetComponent<UltimateCharacterController.Camera.CameraController>().Character = gameObject;
                        m_CharacterLocomotion.enabled = true;
                    }
                }
            } else {
                // Non-local players will not be controlled with player input.
                enabled = false;
                m_PlayerInput.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The character has respawned.
        /// </summary>
        protected override void OnRespawn()
        {
            if (!m_NetworkInfo.HasAuthority()) {
                return;
            }

            base.OnRespawn();
        }
    }
}