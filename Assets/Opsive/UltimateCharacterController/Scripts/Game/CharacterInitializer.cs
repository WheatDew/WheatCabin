/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Game
{
    using UnityEngine;
    using System;

    /// <summary>
    /// Singleton which allows the character to be initialized at a later time through script.
    /// </summary>
    public class CharacterInitializer : MonoBehaviour
    {
        private static CharacterInitializer s_Instance;
        public static CharacterInitializer Instance { get => s_Instance; }

        [Tooltip("Should the Unity callbacks Awake and Start be executed?")]
        [SerializeField] protected bool m_AutoInitialization = true;

        public Action OnAwake;
        public Action OnEnable;
        public Action OnStart;

        public static bool AutoInitialization
        {
            get { if (s_Instance == null) { return true; } return Instance.m_AutoInitialization; }
            set {
                if (s_Instance == null) {
                    var parent = new GameObject();
                    s_Instance = parent.AddComponent<CharacterInitializer>();
                }
                s_Instance.m_AutoInitialization = value;
            }
        }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            s_Instance = this;
        }
    }
}