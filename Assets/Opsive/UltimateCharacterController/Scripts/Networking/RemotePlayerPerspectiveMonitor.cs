/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Networking.Character
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Game;
    using Opsive.Shared.Networking;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Identifiers;
    using Opsive.UltimateCharacterController.Items;
    using UnityEngine;

    /// <summary>
    /// Manages the perspective switch for a networked remote player. This is a lightweight version of the Third Person Controller Perspective Monitor.
    /// </summary>
    public class RemotePlayerPerspectiveMonitor : MonoBehaviour
    {
        [Tooltip("The material used to make the object invisible but still cast shadows.")]
        [SerializeField] protected Material m_InvisibleMaterial;

        public Material InvisibleMaterial { get { return m_InvisibleMaterial; } set { m_InvisibleMaterial = value; } }

        private GameObject m_GameObject;
        private UltimateCharacterLocomotion m_CharacterLocomotion;
        private INetworkInfo m_NetworkInfo;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
#if THIRD_PERSON_CONTROLLER
            // If the third person Perspective Monitor exists then that component will manage the remote player's perspective.
            var perspectiveMonitor = gameObject.GetComponent<ThirdPersonController.Character.PerspectiveMonitor>();
            if (perspectiveMonitor != null) {
                Destroy(this);
                return;
            }
#endif
            m_GameObject = gameObject;
            m_NetworkInfo = m_GameObject.GetCachedComponent<INetworkInfo>();
            if (m_NetworkInfo== null) {
                Debug.LogError("Error: The character must have a NetworkInfo object.");
                return;
            }
            m_CharacterLocomotion = m_GameObject.GetCachedComponent<UltimateCharacterLocomotion>();

            EventHandler.RegisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
            EventHandler.RegisterEvent<CharacterItem>(m_GameObject, "OnInventoryAddItem", OnAddItem);
        }

        /// <summary>
        /// A new ILookSource object has been attached to the character.
        /// </summary>
        /// <param name="lookSource">The ILookSource object attached to the character.</param>
        private void OnAttachLookSource(ILookSource lookSource)
        {
            var firstPersonPerspective = false;
            if (lookSource != null && m_NetworkInfo.IsLocalPlayer()) {
                var cameraController = lookSource as UltimateCharacterController.Camera.CameraController;
                if (cameraController != null) {
                    firstPersonPerspective = cameraController.ActiveViewType.FirstPersonPerspective;
                }
            }

            // The character is a first person character. Set the third person objects to the invisible shadow castor material.
            if (firstPersonPerspective) {
                var thirdPersonObjects = gameObject.GetComponentsInChildren<ThirdPersonObject>(true);
                for (int i = 0; i < thirdPersonObjects.Length; ++i) {
                    var renderers = thirdPersonObjects[i].GetComponentsInChildren<Renderer>(true);
                    for (int j = 0; j < renderers.Length; ++j) {
                        var materials = renderers[j].materials;
                        for (int k = 0; k < materials.Length; ++k) {
                            materials[k] = m_InvisibleMaterial;
                        }
                        renderers[j].materials = materials;
                    }
                }
            }

            Destroy(this);
        }

        /// <summary>
        /// The inventory has added the specified item.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        private void OnAddItem(CharacterItem item)
        {
            if (!m_CharacterLocomotion.FirstPersonPerspective) {
                return;
            }

            // The Third Person's PerspectiveItem object will contain a reference to the ThirdPersonObject component.
            var perspectiveItems = item.GetComponents<PerspectiveItem>();
            PerspectiveItem thirdPersonPerspectiveItem = null;
            for (int i = 0; i < perspectiveItems.Length; ++i) {
                if (!perspectiveItems[i].FirstPersonItem) {
                    thirdPersonPerspectiveItem = perspectiveItems[i];
                    break;
                }
            }

            if (thirdPersonPerspectiveItem != null && thirdPersonPerspectiveItem.Object != null) {
                var thirdPersonObject = thirdPersonPerspectiveItem.Object.GetComponent<ThirdPersonObject>();
                if (thirdPersonObject != null) {
                    var renderers = thirdPersonObject.GetComponentsInChildren<Renderer>(true);
                    for (int i = 0; i < renderers.Length; ++i) {
                        var materials = renderers[i].materials;
                        for (int j = 0; j < materials.Length; ++j) {
                            materials[j] = m_InvisibleMaterial;
                        }
                        renderers[i].materials = materials;
                    }
                }
            }
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_GameObject != null) {
                EventHandler.UnregisterEvent<ILookSource>(m_GameObject, "OnCharacterAttachLookSource", OnAttachLookSource);
                EventHandler.UnregisterEvent<CharacterItem>(m_GameObject, "OnInventoryAddItem", OnAddItem);
            }
        }
    }
}