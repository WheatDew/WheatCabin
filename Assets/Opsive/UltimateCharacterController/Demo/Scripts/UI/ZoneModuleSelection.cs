/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Demo.UI
{
    using Opsive.Shared.UI;
    using Opsive.UltimateCharacterController.Demo.Zones;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// A UI for selecting the zone module to start.
    /// </summary>
    public class ZoneModuleSelection : MonoBehaviour
    {
        [Tooltip("The Toggle button prefab used for selecting a module.")]
        [SerializeField] private GameObject m_TogglePrefab;
        [Tooltip("The parent for the category toggles (toggles will be spawned inside).")]
        [SerializeField] private RectTransform m_CategoryContent;
        [Tooltip("The parent for the module toggles (toggles will be spawned inside).")]
        [SerializeField] private RectTransform m_ModuleContent;
        [Tooltip("The description text of the selected module.")]
        [SerializeField] private Text m_Description;
        [Tooltip("The parent component of the modules to display in the UI.")]
        [SerializeField] private Transform m_ZoneModulesParent;
        [Tooltip("Event invoked when the category changes.")]
        [SerializeField] private UnityEvent m_OnCategoryChange;
        [Tooltip("Event invoked when the module changes.")]
        [SerializeField] private UnityEvent m_OnModuleChange;

        private ZoneModule[] m_AllZoneModules;
        private List<ZoneModule> m_AllCategories;
        private List<Toggle> m_CategoryToggles;
        private Dictionary<ZoneModule,Toggle> m_ToggleByCategory;
        private List<Toggle> m_ModuleToggles;
        private Dictionary<ZoneModule, List<ZoneModule>> m_ModulesByCategory;

        private ZoneModule m_ActiveCategory;
        private ZoneModule m_ActiveZoneModule;
        
        public GameObject TogglePrefab { get => m_TogglePrefab; set => m_TogglePrefab = value; }
        public RectTransform CategoryContent { get => m_CategoryContent; set => m_CategoryContent = value; }
        public RectTransform ModuleContent { get => m_ModuleContent; set => m_ModuleContent = value; }
        public Text Description { get => m_Description; set => m_Description = value; }
        public Transform ZoneModulesParent { get => m_ZoneModulesParent; set => m_ZoneModulesParent = value; }
        public UnityEvent OnCategoryChange { get => m_OnCategoryChange; set => m_OnCategoryChange = value; }
        public UnityEvent OnModuleChange { get => m_OnModuleChange; set => m_OnModuleChange = value; }
        public ZoneModule[] AllZoneModules { get => m_AllZoneModules; set => m_AllZoneModules = value; }
        public List<Toggle> CategoryToggles { get => m_CategoryToggles; set => m_CategoryToggles = value; }
        public Dictionary<ZoneModule, Toggle> ToggleByCategory { get => m_ToggleByCategory; set => m_ToggleByCategory = value; }
        public List<Toggle> ModuleToggles { get => m_ModuleToggles; set => m_ModuleToggles = value; }
        public Dictionary<ZoneModule, List<ZoneModule>> ModulesByCategory { get => m_ModulesByCategory; set => m_ModulesByCategory = value; }
        public ZoneModule ActiveCategory { get => m_ActiveCategory; set => m_ActiveCategory = value; }
        public ZoneModule ActiveZoneModule { get => m_ActiveZoneModule; set => m_ActiveZoneModule = value; }


        /// <summary>
        /// Initialize the Ui to show the zone category modules.
        /// </summary>
        private void Awake()
        {
            m_AllZoneModules = m_ZoneModulesParent.GetComponentsInChildren<ZoneModule>(true);
            m_AllCategories = new List<ZoneModule>();
            m_ModulesByCategory = new Dictionary<ZoneModule, List<ZoneModule>>();
            m_ToggleByCategory = new Dictionary<ZoneModule, Toggle>();
            m_CategoryToggles = new List<Toggle>();
            m_ModuleToggles = new List<Toggle>();
            
            // Cleanup the toggle parents as they might have toggles for previewing
            for (int i = m_CategoryContent.childCount - 1; i >= 0; i--) {
                var child = m_CategoryContent.GetChild(i);
                Destroy(child.gameObject);
            }
            for (int i = m_ModuleContent.childCount - 1; i >= 0; i--) {
                var child = m_ModuleContent.GetChild(i);
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < m_AllZoneModules.Length; i++) {
                var zoneModule = m_AllZoneModules[i];
                zoneModule.Initialize(false);
                var category = zoneModule.ParentModule;

                if (category == null) {
                    // If the category is null, then the zone module is the category.
                    category = zoneModule;
                }

                if (m_ModulesByCategory.TryGetValue(category, out var zoneModules) == false) {
                    zoneModules = new List<ZoneModule>();
                    m_ModulesByCategory.Add(category,zoneModules);
                    m_AllCategories.Add(category);
                    
                    //New category create a toggle for it
                    var newToggleGO = Instantiate(m_TogglePrefab, m_CategoryContent);
                    var categoryToggle = newToggleGO.GetComponent<Toggle>();

                    var sharedComponent = categoryToggle.GetComponentInChildren<TextComponent>();
                    sharedComponent.text = category.DisplayName; //last element in the array.
                    m_CategoryToggles.Add(categoryToggle);
                    
                    m_ToggleByCategory.Add(category, categoryToggle);
                    
                    categoryToggle.onValueChanged.AddListener((toggleOn) =>
                    {
                        if (toggleOn) {
                            ToggleOnCategory(category);
                        }
                    });
                }

                // Add the modules lin the list of category modules if it is not the category itself.
                if (category != zoneModule) {
                    zoneModules.Add(zoneModule);
                }
                
            }
            
            var firstCategory = m_AllCategories[0];
            ToggleOnCategory(firstCategory);
        }

        /// <summary>
        /// Toggle on a zone category.
        /// </summary>
        /// <param name="category">The zone category toggled on.</param>
        public void ToggleOnCategory(ZoneModule category)
        {
            if (m_ActiveCategory == category && m_ActiveCategory != null) {
                m_ActiveCategory.Restart();
                return;
            }
            
            m_ActiveCategory = category;
            var zoneModules = m_ModulesByCategory[category];
            var toggle = m_ToggleByCategory[category];

            // Make sure only one toggle is active
            for (int i = 0; i < m_CategoryToggles.Count; i++) {
                var otherToggles = m_CategoryToggles[i];
                otherToggles.SetIsOnWithoutNotify(otherToggles == toggle);
            }
            
            // Make sure only one toggle is active
            for (int i = 0; i < m_AllCategories.Count; i++) {
                var otherCategories = m_AllCategories[i];
                otherCategories.gameObject.SetActive(otherCategories == m_ActiveCategory);
            }

            // Set the zone modules within the category.
            for (int i = 0; i < zoneModules.Count; i++) {
                var zoneModule = zoneModules[i];
                Toggle moduleToggle = null;
                if (i >= m_ModuleToggles.Count) {
                    // A new module toggle must be spawned.
                    var newToggleGO = Instantiate(m_TogglePrefab, m_ModuleContent);
                    moduleToggle = newToggleGO.GetComponent<Toggle>();

                    var localIndex = i;
                    moduleToggle.onValueChanged.AddListener((toggleOn) =>
                    {
                        if (toggleOn) {
                            ToggleOnZoneModule(localIndex);
                        }
                    });
                    
                    m_ModuleToggles.Add(moduleToggle);
                } else {
                    moduleToggle = m_ModuleToggles[i];
                }

                moduleToggle.gameObject.SetActive(true);
                var sharedText = moduleToggle.GetComponentInChildren<TextComponent>();
                sharedText.text = zoneModule.DisplayName;
            }

            // Disable toggles that aren't used.
            for (int i = zoneModules.Count; i < m_ModuleToggles.Count; i++) {
                m_ModuleToggles[i].gameObject.SetActive(false);
            }

            // Toggle on the first module.
            ToggleOnZoneModule(0);
            
            m_OnCategoryChange?.Invoke();
        }

        /// <summary>
        /// Toggle on the zone module at the index provided.
        /// </summary>
        /// <param name="zoneModuleIndex">The zone module index.</param>
        public void ToggleOnZoneModule(int zoneModuleIndex)
        {
            var zoneModules = m_ModulesByCategory[m_ActiveCategory];
            var zoneModule = zoneModules[zoneModuleIndex];

            // Restart the module if it was already active.
            if (m_ActiveZoneModule == zoneModule) {
                zoneModule?.Restart();
                return;
            }
            
            // Make sure only one toggle is active
            for (int i = 0; i < m_ModuleToggles.Count; i++) {
                var otherToggles = m_ModuleToggles[i];
                otherToggles.SetIsOnWithoutNotify(i == zoneModuleIndex);
            }

            // Only allow one module to be active.
            for (int i = 0; i < m_AllZoneModules.Length; i++) {
                var otherZoneModule = m_AllZoneModules[i];
                otherZoneModule.gameObject.SetActive(otherZoneModule == zoneModule || otherZoneModule == m_ActiveCategory);
            }
            
            m_ActiveZoneModule = zoneModule;
            m_Description.text = zoneModule.DisplayDescription;
            
            m_OnModuleChange?.Invoke();
        }
    }
}
