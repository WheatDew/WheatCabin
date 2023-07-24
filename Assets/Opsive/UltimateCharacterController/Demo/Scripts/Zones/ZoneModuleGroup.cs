/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.Zones
{
    using UnityEngine;

    /// <summary>
    /// A component set as a parent to multiple modules. Used to quickly toggle modules on and off in the Inspector.
    /// </summary>
    public class ZoneModuleGroup : MonoBehaviour
    {
        /// <summary>
        /// Enable all the modules that are children of this component.
        /// </summary>
        [ContextMenu("Enable All Modules")]
        public void EnableAllModules()
        {
            var modules = GetComponentsInChildren<ZoneModule>(true);
            for (int i = 0; i < modules.Length; i++) {
                modules[i].gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// Disable all the modules that are children of this component.
        /// </summary>
        [ContextMenu("Disable All Modules")]
        public void DisableAllModules()
        {
            var modules = GetComponentsInChildren<ZoneModule>(true);
            for (int i = 0; i < modules.Length; i++) {
                modules[i].gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Toggle on the zone module provided.
        /// </summary>
        /// <param name="zoneModule">The zone module index.</param>
        public void ToggleOnZoneModule(ZoneModule zoneModule)
        {
            DisableAllModules();

            var parentTransform = zoneModule.transform;
            while (parentTransform != transform && parentTransform != null) {
                parentTransform.gameObject.SetActive(true);
                parentTransform = parentTransform.parent;
            }
        }
    }
}