/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.Shared.Game;
    using Opsive.Shared.Editor.Inspectors.Input;
    using Opsive.Shared.StateSystem;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility.Builders;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The SetupManager shows any project or scene related setup options.
    /// </summary>
    [OrderedEditorItem("Setup", 1)]
    public class SetupManager : Manager
    {
        private const string c_MonitorsPrefabGUID = "b5bf2e4077598914b83fc5e4ca20f2f4";
        private const string c_VirtualControlsPrefabGUID = "33d3d57ba5fc7484c8d09150e45066a4";
        private const string c_3DAudioManagerModuleGUID = "7c2f6e9d4d7571042964493904b06c50";
        private const string c_ObjectFaderAimStateGUID = "5c1fe60fde7c54e48ad118439bf49b9b";

        /// <summary>
        /// Specifies the perspective that the ViewType can change into.
        /// </summary>
        private enum Perspective
        {
            First,  // The ViewType can only be in first person perspective.
            Third,  // The ViewType can only be in third person perspective.
            Both,   // The ViewType can be in first or third person perspective.
            None    // Default value.
        }

        private string[] m_ToolbarStrings = { "Scene", "Project" };
        [SerializeField] private bool m_DrawSceneSetup = true;

        [SerializeField] private Perspective m_Perspective = Perspective.None;
        [SerializeField] private string m_FirstPersonViewType;
        [SerializeField] private string m_ThirdPersonViewType;
        [SerializeField] private bool m_StartFirstPersonPerspective;
        [SerializeField] private bool m_CanSetupCamera;

        private List<Type> m_FirstPersonViewTypes = new List<Type>();
        private List<string> m_FirstPersonViewTypeStrings = new List<string>();
        private List<Type> m_ThirdPersonViewTypes = new List<Type>();
        private List<string> m_ThirdPersonViewTypeStrings = new List<string>();
        private List<string> m_PerspectiveNames = new List<string>() { "First", "Third", "Both" };
        private VisualElement m_SceneProjectContainer;

        /// <summary>
        /// Initializes the manager after deserialization.
        /// </summary>
        /// <param name="mainManagerWindow">A reference to the Main Manager Window.</param>
        public override void Initialize(MainManagerWindow mainManagerWindow)
        {
            base.Initialize(mainManagerWindow);

            // Set the default perspective based on what asset is installed.
            if (m_Perspective == Perspective.None) {
#if FIRST_PERSON_CONTROLLER
                m_Perspective = Perspective.First;
#elif THIRD_PERSON_CONTROLLER
                m_Perspective = Perspective.Third;
#endif
            }

            // Get a list of the available view types.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i) {
                try {
                    var assemblyTypes = assemblies[i].GetTypes();
                    for (int j = 0; j < assemblyTypes.Length; ++j) {
                        // Must derive from ViewType.
                        if (!typeof(UltimateCharacterController.Camera.ViewTypes.ViewType).IsAssignableFrom(assemblyTypes[j])) {
                            continue;
                        }

                        // Ignore abstract classes.
                        if (assemblyTypes[j].IsAbstract) {
                            continue;
                        }

                        if (assemblyTypes[j].FullName.Contains("FirstPersonController")) {
                            m_FirstPersonViewTypes.Add(assemblyTypes[j]);
                        } else if (assemblyTypes[j].FullName.Contains("ThirdPersonController")) {
                            m_ThirdPersonViewTypes.Add(assemblyTypes[j]);
                        }
                    }
                } catch (Exception) {
                    continue;
                }
            }

            // Create an array of display names for the popup.
            for (int i = 0; i < m_FirstPersonViewTypes.Count; ++i) {
                m_FirstPersonViewTypeStrings.Add(InspectorUtility.DisplayTypeName(m_FirstPersonViewTypes[i], true));
            }
            for (int i = 0; i < m_ThirdPersonViewTypes.Count; ++i) {
                m_ThirdPersonViewTypeStrings.Add(InspectorUtility.DisplayTypeName(m_ThirdPersonViewTypes[i], true));
            }
        }

        /// <summary>
        /// Opens the Project Setup tab.
        /// </summary>
        public void OpenProjectSetup()
        {
            m_SceneProjectContainer.Clear();
            m_DrawSceneSetup = false;
            ShowProjectSetup();
        }

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            var tabToolbar = new TabToolbar(m_ToolbarStrings, m_DrawSceneSetup ? 0 : 1, (int selected) =>
            {
                m_DrawSceneSetup = selected == 0;
                m_SceneProjectContainer.Clear();
                if (m_DrawSceneSetup) {
                    ShowSceneSetup();
                } else {
                    ShowProjectSetup();
                }
            }, true);
            m_ManagerContentContainer.Add(tabToolbar);

            m_SceneProjectContainer = new VisualElement();
            m_ManagerContentContainer.Add(m_SceneProjectContainer);

            if (m_DrawSceneSetup) {
                ShowSceneSetup();
            } else {
                ShowProjectSetup();
            }
        }

        /// <summary>
        /// Shows the scene setup control boxes.
        /// </summary>
        private void ShowSceneSetup()
        {
            ManagerUtility.ShowControlBox("Manager Setup", "Adds the scene-level manager components to the scene.", null, "Add Managers", AddManagers, m_SceneProjectContainer, true);
            ManagerUtility.ShowControlBox("Camera Setup", "Sets up the camera within the scene to use the Ultimate Character Controller Camera Controller component.", ShowCameraSetup,
                                    "Setup Camera", SetupCamera, m_SceneProjectContainer, true);
            ManagerUtility.ShowControlBox("UI Setup", "Adds the UI monitors to the scene.", null, "Add UI", AddUI, m_SceneProjectContainer, true);
            ManagerUtility.ShowControlBox("Virtual Controls Setup", "Adds the virtual controls to the scene.", null, "Add Virtual Controls", AddVirtualControls, m_SceneProjectContainer, true);
        }

        /// <summary>
        /// Shows the project setup control boxes.
        /// </summary>
        private void ShowProjectSetup()
        {
            // Show a warning if the button mappings or layers have not been updated.
            var serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            var axisProperty = serializedObject.FindProperty("m_Axes");
            var hasInputs = UnityInputBuilder.FindAxisProperty(axisProperty, "Action", string.Empty, string.Empty, UnityInputBuilder.AxisType.KeyMouseButton, false) != null && 
                            UnityInputBuilder.FindAxisProperty(axisProperty, "Crouch", string.Empty, string.Empty, UnityInputBuilder.AxisType.KeyMouseButton, false) != null;

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProperty = tagManager.FindProperty("layers");
            var hasLayers = layersProperty.GetArrayElementAtIndex(LayerManager.Character).stringValue == "Character";

            if (!hasInputs || !hasLayers) {
                var helpBox = new HelpBox();
                helpBox.messageType = HelpBoxMessageType.Warning;
                helpBox.text = "The default button mappings or layers have not been added. If you are just getting started you should update the button mappings and layers with the button below. " +
                    "This can be changed later.";
                helpBox.style.marginTop = 20;
                m_SceneProjectContainer.Add(helpBox);

                var updateButton = new Button();
                updateButton.text = "Update Buttons and Layers";
                updateButton.clicked += () => {
                    Utility.CharacterInputBuilder.UpdateInputManager();
                    UpdateLayers();
                    helpBox.style.display = DisplayStyle.None;
                    updateButton.style.display = DisplayStyle.None;
                };
                updateButton.AddToClassList("sub-menu-button");
                m_SceneProjectContainer.Add(updateButton);
            }

            ManagerUtility.ShowControlBox("Button Mappings", "Add the default button mappings to the Unity Input Manager. If you are using a custom button mapping or " +
                            "an input integration then you do not need to update the Unity button mappings.", null, "Update Buttons",
                            Utility.CharacterInputBuilder.UpdateInputManager, m_SceneProjectContainer, true);

            ManagerUtility.ShowControlBox("Layers", "Update the project layers to the default character controller layers. The layers do not need to be updated " +
                            "if you have already setup a custom set of layers.", null, "Update Layers", UpdateLayers, m_SceneProjectContainer, true);
        }

        /// <summary>
        /// Shows the camera setup fields.
        /// </summary>
        /// <param name="container">The VisualElement that contains the setup fields.</param>
        private void ShowCameraSetup(VisualElement container)
        {
            // Draw the perspective.
            var selectedPerspectivePopup = new PopupField<string>("Perspective", m_PerspectiveNames, (int)m_Perspective);
            selectedPerspectivePopup.RegisterValueChangedCallback(c =>
            {
                m_SceneProjectContainer.Clear();
                m_Perspective = (Perspective)selectedPerspectivePopup.index;
                if (m_DrawSceneSetup) {
                    ShowSceneSetup();
                } else {
                    ShowProjectSetup();
                }
            });
            container.Add(selectedPerspectivePopup);
            m_CanSetupCamera = true;
            // Determine if the selected perspective is supported.
#if !FIRST_PERSON_CONTROLLER
            if (m_Perspective == Perspective.First || m_Perspective == Perspective.Both) {
                var helpBox = new HelpBox();
                helpBox.messageType = HelpBoxMessageType.Error;
                helpBox.text = "Unable to select the First Person Controller perspective. If you'd like to use a first person perspective ensure the " +
                                        "First Person Controller is imported.";
                helpBox.Q<Label>().style.fontSize = 12;
                container.Add(helpBox);
                m_CanSetupCamera = false;
            }
#endif
#if !THIRD_PERSON_CONTROLLER
            if (m_Perspective == Perspective.Third || m_Perspective == Perspective.Both) {
                var helpBox = new HelpBox();
                helpBox.messageType = HelpBoxMessageType.Error;
                helpBox.text = "Unable to select the Third Person Controller perspective. If you'd like to use a third person perspective ensure the " +
                                        "Third Person Controller is imported.";
                helpBox.Q<Label>().style.fontSize = 12;
                container.Add(helpBox);
                m_CanSetupCamera = false;
            }
#endif
            if (!m_CanSetupCamera) {
                return;
            }

            // Show the available first person ViewTypes.
            if (m_Perspective == Perspective.First || m_Perspective == Perspective.Both) {
                var selectedViewType = -1;
                for (int i = 0; i < m_FirstPersonViewTypes.Count; ++i) {
                    if (m_FirstPersonViewTypes[i].FullName == m_FirstPersonViewType) {
                        selectedViewType = i;
                        break;
                    }
                }
                if (selectedViewType == -1) {
                    selectedViewType = 0;
                    m_FirstPersonViewType = m_FirstPersonViewTypes[0].FullName;
                }
                var selectedViewTypePopup = new PopupField<string>("First Person View Type", m_FirstPersonViewTypeStrings, selectedViewType);
                selectedViewTypePopup.RegisterValueChangedCallback(c =>
                {
                    m_FirstPersonViewType = m_FirstPersonViewTypes[selectedViewTypePopup.index].FullName;

                });
                container.Add(selectedViewTypePopup);
            }
            // Show the available third person ViewTypes.
            if (m_Perspective == Perspective.Third || m_Perspective == Perspective.Both) {
                var selectedViewType = -1;
                for (int i = 0; i < m_ThirdPersonViewTypes.Count; ++i) {
                    if (m_ThirdPersonViewTypes[i].FullName == m_ThirdPersonViewType) {
                        selectedViewType = i;
                        break;
                    }
                }
                if (selectedViewType == -1) {
                    selectedViewType = 0;
                    m_ThirdPersonViewType = m_ThirdPersonViewTypes[0].FullName;
                }
                var selectedViewTypePopup = new PopupField<string>("Third Person View Type", m_ThirdPersonViewTypeStrings, selectedViewType);
                selectedViewTypePopup.RegisterValueChangedCallback(c =>
                {
                    m_ThirdPersonViewType = m_ThirdPersonViewTypes[selectedViewTypePopup.index].FullName;

                });
                container.Add(selectedViewTypePopup);
            }
            if (m_Perspective == Perspective.Both) {
                var startPerspectivePopup = new PopupField<string>("Start Perspective", new List<string>() { "First Person", "Third Person" }, m_StartFirstPersonPerspective ? 0 : 1);
                startPerspectivePopup.RegisterValueChangedCallback(c =>
                {
                    m_StartFirstPersonPerspective = startPerspectivePopup.index == 0;
                });
                container.Add(startPerspectivePopup);
            } else {
                m_StartFirstPersonPerspective = (m_Perspective == Perspective.First);
            }
        }

        /// <summary>
        /// Sets up the camera if it hasn't already been setup.
        /// </summary>
        private void SetupCamera()
        {
            if (!m_CanSetupCamera) {
                return;
            }

            // Setup the camera.
            GameObject cameraGameObject;
            var addedCameraController = false;
            var camera = UnityEngine.Camera.main;
            if (camera == null) {
                // If the main camera can't be found then use the first available camera.
                var cameras = UnityEngine.Camera.allCameras;
                if (cameras != null && cameras.Length > 0) {
                    // Prefer cameras that are at the root level.
                    for (int i = 0; i < cameras.Length; ++i) {
                        if (cameras[i].transform.parent == null) {
                            camera = cameras[i];
                            break;
                        }
                    }
                    // No cameras are at the root level. Set the first available camera.
                    if (camera == null) {
                        camera = cameras[0];
                    }
                }

                // A new camera should be created if there isn't a valid camera.
                if (camera == null) {
                    cameraGameObject = new GameObject("Camera");
                    cameraGameObject.tag = "MainCamera";
                    camera = cameraGameObject.AddComponent<UnityEngine.Camera>();
                    cameraGameObject.AddComponent<AudioListener>();
                }
            }

            // The near clip plane should adjusted for viewing close objects.
            camera.nearClipPlane = 0.01f;

            // Add the CameraController if it isn't already added.
            cameraGameObject = camera.gameObject;
            if (cameraGameObject.GetComponent<CameraController>() == null) {
                var cameraController = cameraGameObject.AddComponent<CameraController>();
                if (m_Perspective == Perspective.Both) {
                    ViewTypeBuilder.AddViewType(cameraController, typeof(UltimateCharacterController.Camera.ViewTypes.Transition));
                }
                if (m_StartFirstPersonPerspective) {
                    if (m_Perspective != Perspective.First && !string.IsNullOrEmpty(m_ThirdPersonViewType)) {
                        ViewTypeBuilder.AddViewType(cameraController, Shared.Utility.TypeUtility.GetType(m_ThirdPersonViewType));
                    }
                    if (m_Perspective != Perspective.Third && !string.IsNullOrEmpty(m_FirstPersonViewType)) {
                        ViewTypeBuilder.AddViewType(cameraController, Shared.Utility.TypeUtility.GetType(m_FirstPersonViewType));
                    }
                } else {
                    if (m_Perspective != Perspective.Third && !string.IsNullOrEmpty(m_FirstPersonViewType)) {
                        ViewTypeBuilder.AddViewType(cameraController, Shared.Utility.TypeUtility.GetType(m_FirstPersonViewType));
                    }
                    if (m_Perspective != Perspective.First && !string.IsNullOrEmpty(m_ThirdPersonViewType)) {
                        ViewTypeBuilder.AddViewType(cameraController, Shared.Utility.TypeUtility.GetType(m_ThirdPersonViewType));
                    }
                }

                // Detect if a character exists in the scene. Automatically add the character if it does.
                var characters = UnityEngine.Object.FindObjectsOfType<UltimateCharacterController.Character.CharacterLocomotion>();
                if (characters != null && characters.Length == 1) {
                    cameraController.InitCharacterOnAwake = true;
                    cameraController.Character = characters[0].gameObject;
                }

                // Setup the components which help the Camera Controller.
                Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<CameraControllerHandler>(cameraGameObject);
#if THIRD_PERSON_CONTROLLER
                if (m_Perspective != Perspective.First) {
                    var objectFader = Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<ThirdPersonController.Camera.ObjectFader>(cameraGameObject);

                    if (!Application.isPlaying) {
                        // The Moving and Move Towards states should automatically be added.
                        var aimPresetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(c_ObjectFaderAimStateGUID);
                        if (!string.IsNullOrEmpty(aimPresetPath)) {
                            var aimPreset = UnityEditor.AssetDatabase.LoadAssetAtPath(aimPresetPath, typeof(PersistablePreset)) as PersistablePreset;
                            if (aimPreset != null) {
                                var states = objectFader.States;
                                System.Array.Resize(ref states, states.Length + 1);
                                // Default must always be at the end.
                                states[states.Length - 1] = states[0];
                                states[states.Length - 2] = new State("Aim", aimPreset, null);
                                objectFader.States = states;
                            }
                        }
                    }
                }
#endif
                addedCameraController = true;
            }

            if (addedCameraController) {
                Debug.Log("The Camera Controller has been added.");
            } else {
                Debug.LogWarning("Warning: No action was performed, the Camera Controller component has already been added.");
            }
        }

        /// <summary>
        /// Adds the singleton manager components.
        /// </summary>
        public static void AddManagers()
        {
            // Create the "Game" components if it doesn't already exists.
            SchedulerBase scheduler;
            GameObject gameGameObject;
            if ((scheduler = UnityEngine.Object.FindObjectOfType<SchedulerBase>()) == null) {
                gameGameObject = new GameObject("Game");
            } else {
                gameGameObject = scheduler.gameObject;
            }

            // Add the Singletons.
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<SurfaceSystem.SurfaceManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<SurfaceSystem.DecalManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<SimulationManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<ObjectPool>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<Scheduler>(gameGameObject);
            var audiomanager = Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<Shared.Audio.AudioManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<SpawnPointManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<StateManager>(gameGameObject);
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<LayerManager>(gameGameObject);

            if (audiomanager.AudioManagerModule == null) {
                var defaultAudioManagerModulePath = AssetDatabase.GUIDToAssetPath(c_3DAudioManagerModuleGUID);
                if (!string.IsNullOrEmpty(defaultAudioManagerModulePath)) {
                    var audioManagerModule = AssetDatabase.LoadAssetAtPath(defaultAudioManagerModulePath, typeof(Shared.Audio.AudioManagerModule)) as Shared.Audio.AudioManagerModule;
                    audiomanager.AudioManagerModule = audioManagerModule;
                }
            }

            Debug.Log("The managers have been added.");
        }

        /// <summary>
        /// Adds the UI to the scene.
        /// </summary>
        private void AddUI()
        {
            var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (canvas == null) {
                EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
                canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            }

            // Look up based on guid.
            GameObject uiPrefab = null;
            var monitorsPath = AssetDatabase.GUIDToAssetPath(c_MonitorsPrefabGUID);
            if (!string.IsNullOrEmpty(monitorsPath)) {
                uiPrefab = AssetDatabase.LoadAssetAtPath(monitorsPath, typeof(GameObject)) as GameObject;
            }

            // If the guid wasn't found try the path.
            if (uiPrefab == null) {
                var baseDirectory = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(m_MainManagerWindow))).Replace("\\", "/").Replace("Editor/Managers", "");
                uiPrefab = AssetDatabase.LoadAssetAtPath(baseDirectory + "Demo/Prefabs/UI/Monitors.prefab", typeof(GameObject)) as GameObject;
            }

            if (uiPrefab == null) {
                Debug.LogError("Error: Unable to find the UI Monitors prefab.");
                return;
            }

            // Instantiate the Monitors prefab.
            var uiGameObject = PrefabUtility.InstantiatePrefab(uiPrefab) as GameObject;
            uiGameObject.name = "Monitors";
            uiGameObject.GetComponent<RectTransform>().SetParent(canvas.transform, false);

            // The GlobalDictionary is used by the UI. Add it to the "Game" GameObject.
            SchedulerBase scheduler;
            GameObject gameGameObject;
            if ((scheduler = UnityEngine.Object.FindObjectOfType<SchedulerBase>()) == null) {
                gameGameObject = new GameObject("Game");
            } else {
                gameGameObject = scheduler.gameObject;
            }
            Shared.Editor.Inspectors.Utility.InspectorUtility.AddComponent<Shared.Utility.GlobalDictionary>(gameGameObject);
        }

        /// <summary>
        /// Adds the UI to the scene.
        /// </summary>
        private void AddVirtualControls()
        {
            var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (canvas == null) {
                EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
                canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            }

            // Look up based on guid.
            GameObject virtualControlsPrefab = null;
            var virtualControlsPath = AssetDatabase.GUIDToAssetPath(c_VirtualControlsPrefabGUID);
            if (!string.IsNullOrEmpty(virtualControlsPath)) {
                virtualControlsPrefab = AssetDatabase.LoadAssetAtPath(virtualControlsPath, typeof(GameObject)) as GameObject;
            }

            // If the guid wasn't found try the path.
            if (virtualControlsPrefab == null) {
                var baseDirectory = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(m_MainManagerWindow))).Replace("\\", "/").Replace("Editor/Managers", "");
                virtualControlsPrefab = AssetDatabase.LoadAssetAtPath(baseDirectory + "Demo/Prefabs/UI/VirtualControls.prefab", typeof(GameObject)) as GameObject;
            }

            if (virtualControlsPrefab == null) {
                Debug.LogError("Error: Unable to find the UI Virtual Controls prefab.");
                return;
            }

            // Instantiate the Virtual Controls prefab.
            var virtualControls = PrefabUtility.InstantiatePrefab(virtualControlsPrefab) as GameObject;
            virtualControls.name = "VirtualControls";
            virtualControls.GetComponent<RectTransform>().SetParent(canvas.transform, false);
        }

        /// <summary>
        /// Updates all of the layers to the Ultimate Character Controller defaults.
        /// </summary>
        public static void UpdateLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProperty = tagManager.FindProperty("layers");

            // Add the layers.
            AddLayer(layersProperty, LayerManager.Enemy, "Enemy");
            AddLayer(layersProperty, LayerManager.MovingPlatform, "MovingPlatform");
            AddLayer(layersProperty, LayerManager.VisualEffect, "VisualEffect");
            AddLayer(layersProperty, LayerManager.Overlay, "Overlay");
            AddLayer(layersProperty, LayerManager.SubCharacter, "SubCharacter");
            AddLayer(layersProperty, LayerManager.Character, "Character");

            tagManager.ApplyModifiedProperties();

            Debug.Log("The layers were successfully updated.");
        }

        /// <summary>
        /// Sets the layer index to the specified name if the string value is empty.
        /// </summary>
        public static void AddLayer(SerializedProperty layersProperty, int index, string name)
        {
            var layerElement = layersProperty.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(layerElement.stringValue)) {
                layerElement.stringValue = name;
            }
        }
    }
}