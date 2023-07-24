/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// A base component used for enabling and disabling and restarting the state of a module
    /// It is used for the shooting training modules for example.
    /// </summary>
    public class ZoneModule : MonoBehaviour
    {
        [Tooltip("The name to display in the UI.")]
        [SerializeField] private string m_DisplayName;
        [Tooltip("The description to display in the UI.")]
        [TextArea(3,10)]
        [SerializeField] private string m_DisplayDescription;
        [Tooltip("The Event invoked when the gameobject is enabled.")]
        [SerializeField] private UnityEvent m_OnEnableEvent;
        [Tooltip("The Event invoked when the gameobject is disabled.")]
        [SerializeField] private UnityEvent m_OnDisableEvent;
        
        private ZoneModule m_ParentModule;
        private bool m_Initialized = false;
        
        public ZoneModule ParentModule { get => m_ParentModule; set => m_ParentModule = value; }
        public string DisplayName { get => m_DisplayName; set => m_DisplayName = value; }
        public string DisplayDescription { get => m_DisplayDescription; set => m_DisplayDescription = value; }
        public UnityEvent OnEnableEvent { get => m_OnEnableEvent; set => m_OnEnableEvent = value; }
        public UnityEvent OnDisableEvent { get => m_OnDisableEvent; set => m_OnDisableEvent = value; }

        /// <summary>
        /// Awk
        /// </summary>
        private void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize the parent module.
        /// </summary>
        /// <param name="force">Force initialize?</param>
        public void Initialize(bool force)
        {
            if (m_Initialized && !force) {
                return;
            }

            var parents =GetComponentsInParent<ZoneModule>(true);
            for (int i = 0; i < parents.Length; i++) {
                if(parents[i] == this){ continue;}

                m_ParentModule = parents[i];
                break;
            }
            
            
            m_Initialized = true;
        }

        /// <summary>
        /// Restart the module.
        /// </summary>
        public void Restart()
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// On Enable.
        /// </summary>
        private void OnEnable()
        {
            m_OnEnableEvent?.Invoke();
        }

        /// <summary>
        /// On Disable.
        /// </summary>
        private void OnDisable()
        {
            m_OnDisableEvent?.Invoke();
        }

        /// <summary>
        /// Toggle on the this zone module.
        /// </summary>
        [ContextMenu("Switch to zone module.")]
        public void SwitchToZoneModule()
        {
            var group = GetComponentInParent<ZoneModuleGroup>(true);
            if (group == null) {
                Debug.LogWarning("Zone module group not found in parent.");
                return;
            }
            group.ToggleOnZoneModule(this);
        }
        
        /// <summary>
        /// Enable all the modules that are children of this component.
        /// </summary>
        [ContextMenu("Enable All Modules")]
        public void EnableAllModules()
        {
            var group = GetComponentInParent<ZoneModuleGroup>(true);
            if (group == null) {
                Debug.LogWarning("Zone module group not found in parent.");
                return;
            }
            group.EnableAllModules();
        }
        
        /// <summary>
        /// Disable all the modules that are children of this component.
        /// </summary>
        [ContextMenu("Disable All Modules")]
        public void DisableAllModules()
        {
            var group = GetComponentInParent<ZoneModuleGroup>(true);
            if (group == null) {
                Debug.LogWarning("Zone module group not found in parent.");
                return;
            }
            group.DisableAllModules();
        }
    }
}
