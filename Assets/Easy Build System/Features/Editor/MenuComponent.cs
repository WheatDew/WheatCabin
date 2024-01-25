/// <summary>
/// Project : Easy Build System
/// Class : MenuComponent.cs
/// Namespace : EasyBuildSystem.Features.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;

using UnityEngine;

using UnityEditor;
using UnityEditor.SceneManagement;

using EasyBuildSystem.Features.Runtime.Buildings.Area;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions;

using EasyBuildSystem.Features.Runtime.Buildings.Socket;

using EasyBuildSystem.Features.Runtime.Buildings.Placer;
using EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver;
using EasyBuildSystem.Features.Runtime.Buildings.Manager.Collection;

using EasyBuildSystem.Features.Runtime.Buildings.Manager.Editor;

namespace EasyBuildSystem.Features.Editor
{
    public class MenuComponent
    {
        [MenuItem(@"Tools/Easy Build System/Scene Setup...", priority = -10001)]
        public static void SceneSetup()
        {
            if (Camera.main == null)
            {
                Debug.LogWarning("<b>Easy Build System</b> : Could not find the Main Camera.\n" +
                    "Make sure you have a camera with the tag <b>MainCamera</b>.");
                return;
            }

            if (MonoBehaviour.FindObjectOfType<BuildingManager>() != null)
            {
                Debug.LogWarning("<b>Easy Build System</b> : This scene was already setup!");
                return;
            }

            ProjectIntegrity.CheckMissingLayers(new string[1] { "Socket" });

            Camera.main.gameObject.AddComponent<BuildingPlacer>();
            Camera.main.gameObject.AddComponent<StandaloneInputHandler>().hideFlags = HideFlags.HideInInspector;

            BuildingManager buildingManager = new GameObject("Building Manager").AddComponent<BuildingManager>();

            EditorApplication.delayCall += () =>
            {
                BuildingPart defaultPart = Resources.Load<BuildingPart>("Prefabs/Default_Cube");

                if (defaultPart != null)
                {
                    buildingManager.BuildingPartReferences.Add(defaultPart);
                }
            };

            EditorSceneManager.MarkAllScenesDirty();

            Debug.Log("<b>Easy Build System</b> : This scene has been setup with success!");
        }

        [MenuItem(@"Tools/Easy Build System/Package Importer...", priority = -10000)]
        public static void PackageImporter()
        {
            Window.PackageImporter.Init();
        }

        [MenuItem(@"Tools/Easy Build System/Integration Manager...", priority = -10000)]
        public static void IntegrationManager()
        {
            Window.IntegrationManager.Init();
        }

        [MenuItem("GameObject/Easy Build System/Tools/Components/Create Building Manager...", priority = -1000)]
        [MenuItem(@"Tools/Easy Build System/Tools/Components/Create Building Manager...", priority = -1000)]
        public static void CreateBuildingManager()
        {
            GameObject buildingManagerInstance = new GameObject("Building Manager");
            buildingManagerInstance.AddComponent<BuildingManager>();
        }

        [MenuItem("GameObject/Easy Build System/Tools/Components/Create Building Saver...", priority = -1000)]
        [MenuItem(@"Tools/Easy Build System/Tools/Components/Create Building Saver...", priority = -1000)]
        public static void CreateBuildingSaver()
        {
            GameObject buildingSaverInstance = new GameObject("Building Saver");
            buildingSaverInstance.AddComponent<BuildingSaver>();
        }

        [MenuItem("GameObject/Easy Build System/Tools/Components/Create Building Area...", priority = -1000)]
        [MenuItem(@"Tools/Easy Build System/Tools/Components/Create Building Area...", priority = -1000)]
        public static void CreateBuildingArea()
        {
            GameObject buildingAreaInstance = new GameObject("Building Area");
            buildingAreaInstance.AddComponent<BuildingArea>();

            Selection.activeGameObject = buildingAreaInstance;
            SceneView.lastActiveSceneView.FrameSelected();
        }

        [MenuItem("GameObject/Easy Build System/Tools/Components/Create Building Part...", priority = -1000)]
        [MenuItem(@"Tools/Easy Build System/Tools/Components/Create Building Part...", priority = -1000)]
        public static void CreateBuildingPart()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("<b>Easy Build System</b> : Please select the gameObject to create a new Building Part.");
                return;
            }

            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                if (Selection.gameObjects[i].GetComponentInParent<BuildingPart>() == null)
                {
                    string localPath =
                        EditorUtility.SaveFilePanelInProject("Easy Build System - Define a save path...",
                        Selection.gameObjects[i].name, "prefab", "");

                    if (localPath == string.Empty)
                    {
                        return;
                    }

                    if (localPath != string.Empty)
                    {
                        GameObject parentInstance = new GameObject(localPath);

                        Selection.gameObjects[i].transform.position = Vector3.zero;

                        parentInstance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                        Selection.gameObjects[i].transform.SetParent(parentInstance.transform, false);

                        BuildingPart instancedBuildingPart = parentInstance.AddComponent<BuildingPart>();

                        instancedBuildingPart.GetGeneralSettings.Identifier = Guid.NewGuid().ToString("N");
                        instancedBuildingPart.GetGeneralSettings.Name = Selection.gameObjects[i].name;

                        instancedBuildingPart.GetModelSettings.Models.Add(Selection.gameObjects[i]);

                        instancedBuildingPart.gameObject.name = instancedBuildingPart.GetGeneralSettings.Name;

                        instancedBuildingPart.UpdateModelBounds();

                        if (instancedBuildingPart.TryGetBasicsCondition == null)
                        {
                            instancedBuildingPart.gameObject.AddComponent<BuildingBasicsCondition>();
                        }

#if UNITY_2018_3 || UNITY_2019
                        UnityEngine.Object AssetPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(instancedBuildingPart.gameObject, localPath, InteractionMode.UserAction);
                        EditorGUIUtility.PingObject(parentInstance);
                        parentInstance.name = AssetPrefab.name;
#else
                        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
                        GameObject AssetPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(instancedBuildingPart.gameObject, localPath, InteractionMode.UserAction);
                        EditorGUIUtility.PingObject(AssetPrefab);
#endif
                        AssetDatabase.SetLabels(AssetPrefab, new string[1] { "BuildingPart" });
                        AssetDatabase.Refresh();

                        if (BuildingManager.Instance != null)
                        {
                            BuildingPart buildingPart = ((GameObject)AssetPrefab).GetComponent<BuildingPart>();
                            Debug.Log("<b>Easy Build System</b> : The Building Part '" + buildingPart.name + "' has been added to Building Manger.");
                            BuildingManager.Instance.BuildingPartReferences.Add(buildingPart);
                        }

                        if (i >= Selection.gameObjects.Length - 1)
                        {
                            Selection.activeGameObject = parentInstance;
                        }

                        SceneView.lastActiveSceneView.FrameSelected();
                    }
                }
            }

            EditorSceneManager.MarkAllScenesDirty();
        }

        [MenuItem("GameObject/Easy Build System/Tools/Components/Create Building Socket...", priority = -1000)]
        [MenuItem(@"Tools/Easy Build System/Tools/Components/Create Building Socket...", priority = -1000)]
        public static void CreateBuildingSocket()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject buildingSocketInstance = new GameObject("New Building Socket");
                buildingSocketInstance.transform.SetParent(Selection.activeGameObject.transform, false);
                buildingSocketInstance.transform.position = Selection.activeGameObject.transform.position;
                buildingSocketInstance.AddComponent<BuildingSocket>();

                Selection.activeGameObject = buildingSocketInstance;
                SceneView.lastActiveSceneView.FrameSelected();
            }
            else
            {
                GameObject buildingSocketInstance = new GameObject("New Building Socket");
                buildingSocketInstance.AddComponent<BuildingSocket>();

                Selection.activeGameObject = buildingSocketInstance;
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        [MenuItem("GameObject/Easy Build System/Tools/Scriptable Objects/Create Building Collection...", false, priority = -200)]
        [MenuItem(@"Tools/Easy Build System/Tools/Scriptable Objects/Create Building Collection...", priority = -200)]
        public static void CreateBuildingCollection()
        {
            BuildingCollection asset = ScriptableObject.CreateInstance<BuildingCollection>();

            AssetDatabase.CreateAsset(asset, "Assets/New Building Collection.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

        [MenuItem("GameObject/Easy Build System/Tools/Building Type Editor...", false, priority = -100)]
        [MenuItem(@"Tools/Easy Build System/Tools/Building Type Editor...", priority = -100)]
        public static void ManageBuildingTypes()
        {
            BuildingTypeEditor.Init();
        }

        [MenuItem("Tools/Easy Build System/Tools/Building Placer Editor...", priority = -100)]
        public static void EditorBuildingPlacer()
        {
            if (BuildingManager.Instance == null)
            {
                Debug.LogWarning("<b>Easy Build System</b> : The system is not setup on this scene!");
                return;
            }

            if (Application.isPlaying)
            {
                Debug.LogWarning("<b>Easy Build System</b> : You can't use the Editor Building Placer during the runtime!");
                return;
            }

            GameObject obj = new GameObject("Editor - Building Placer");
            obj.AddComponent<InspectorBuildingPlacer>();
            obj.GetComponent<StandaloneInputHandler>().hideFlags = HideFlags.HideInInspector;
            Selection.activeObject = obj;
        }

        [MenuItem(@"Tools/Easy Build System/Support...")]
        public static void SupportLink()
        {
            Application.OpenURL("https://form.jotform.com/202960719544359");
        }

        [MenuItem(@"Tools/Easy Build System/Documentation...")]
        public static void DocumentationLink()
        {
            Application.OpenURL("https://polarinteractive.gitbook.io/easy-build-system/");
        }
    }
}