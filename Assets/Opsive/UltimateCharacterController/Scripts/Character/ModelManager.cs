/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Character
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Events;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
    using Opsive.Shared.Networking;
    using Opsive.UltimateCharacterController.Networking.Character;
#endif
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Allows the character model to be switched at runtime.
    /// </summary>
    [Utility.IgnoreTemplateCopy]
    public class ModelManager : MonoBehaviour
    {
        [Tooltip("The list of models that can be switched to.")]
        [SerializeField] protected GameObject[] m_AvailableModels;
        [Tooltip("The GameObject of the model that is currently active.")]
        [SerializeField] protected GameObject m_ActiveModel;

        public GameObject[] AvailableModels { get => m_AvailableModels; set => m_AvailableModels = value; }
        public GameObject ActiveModel { get => m_ActiveModel; set => ChangeModels(value); }
        public int ActiveModelIndex { get => m_ModelIndexMap[m_ActiveModel]; }

        [System.NonSerialized] private GameObject m_GameObject;
        private Dictionary<GameObject, int> m_ModelIndexMap;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
        private INetworkInfo m_NetworkInfo;
        private INetworkCharacter m_NetworkCharacter;
#endif

        public Dictionary<GameObject, int> ModelIndexMap { get => m_ModelIndexMap; }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_GameObject = gameObject;
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            m_NetworkInfo = m_GameObject.GetCachedComponent<INetworkInfo>();
            m_NetworkCharacter = m_GameObject.GetCachedComponent<INetworkCharacter>();
#endif

            m_ModelIndexMap = new Dictionary<GameObject, int>();
            if (m_AvailableModels != null) {
                if (m_ActiveModel == null) {
                    m_ActiveModel = m_AvailableModels[0].gameObject;
                }

                for (int i = 0; i < m_AvailableModels.Length; ++i) {
                    m_ModelIndexMap.Add(m_AvailableModels[i], i);
                }
            }
        }

        /// <summary>
        /// Deactivates all but the active model.
        /// </summary>
        private void Start()
        {
            for (int i = 0; i < m_AvailableModels.Length; ++i) {
                if (m_AvailableModels[i] == m_ActiveModel) {
                    continue;
                }

                m_AvailableModels[i].SetActive(false);
            }
        }

        /// <summary>
        /// Changes the character model to the target model.
        /// </summary>
        /// <param name="targetModel">The GameObject of the model that should be swiched to.</param>
        public void ChangeModels(GameObject targetModel
#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            , bool fromServer = false
#endif
            )
        {
            if (m_ActiveModel == targetModel) {
                return;
            }

#if ULTIMATE_CHARACTER_CONTROLLER_MULTIPLAYER
            if (m_NetworkInfo != null) {
                if (!m_NetworkInfo.HasAuthority() && !fromServer) {
                    return;
                } else if (m_NetworkInfo.HasAuthority()) {
                    m_NetworkCharacter.ChangeModels(m_ModelIndexMap[targetModel]);
                }
            }
#endif

            var originalAniatorMonitor = m_ActiveModel.GetCachedComponent<AnimatorMonitor>();
            var targetAnimatorMonitor = targetModel.GetCachedComponent<AnimatorMonitor>();

#if FIRST_PERSON_CONTROLLER
            var targetFirstPersonObjects = GetFirstPersonObjects(targetModel);
#endif

            m_ActiveModel.SetActive(false);
            m_ActiveModel = targetModel;
            m_ActiveModel.SetActive(true);

            targetAnimatorMonitor.CopyParameters(originalAniatorMonitor);

#if FIRST_PERSON_CONTROLLER
            if (targetFirstPersonObjects != null) {
                var targetChildAnimatorMonitors = targetFirstPersonObjects.GetComponentsInChildren<ChildAnimatorMonitor>();
                for (int i = 0; i < targetChildAnimatorMonitors.Length; ++i) {
                    targetChildAnimatorMonitors[i].CopyParameters(originalAniatorMonitor);
                }
            }
#endif
            EventHandler.ExecuteEvent(m_GameObject, "OnCharacterSwitchModels", m_ActiveModel);
        }

#if FIRST_PERSON_CONTROLLER
        /// <summary>
        /// Returns the FirstPersonObjects component that is used by the specified character model.
        /// </summary>
        /// <param name="characterModel">The target character model.</param>
        /// <returns>The FirstPersonObjects component that is used by the specified character model.</returns>
        public FirstPersonController.Character.FirstPersonObjects GetFirstPersonObjects(GameObject characterModel)
        {
            var firstPersonObjects = characterModel.GetComponentInChildren<FirstPersonController.Character.FirstPersonObjects>(true);
            if (firstPersonObjects != null) { return firstPersonObjects; }

            // The objects have moved to the camera.
            var characterCamera = Shared.Camera.CameraUtility.FindCamera(gameObject);
            if (characterCamera != null) {
                var allFirstPersonObjects = characterCamera.GetComponentsInChildren<FirstPersonController.Character.FirstPersonObjects>(true);
                for (int i = 0; i < allFirstPersonObjects.Length; ++i) {
                    if (allFirstPersonObjects[i].CharacterModel == characterModel) {
                        firstPersonObjects = allFirstPersonObjects[i];
                        break;
                    }
                }
            }
            return firstPersonObjects;
        }
#endif
    }
}