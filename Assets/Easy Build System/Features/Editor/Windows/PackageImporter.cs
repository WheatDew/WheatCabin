/// <summary>
/// Project : Easy Build System
/// Class : PackageImporter.cs
/// Namespace : EasyBuildSystem.Features.Editor.Window
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections;

using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

using UnityEngine;
using UnityEngine.Rendering;

using EasyBuildSystem.Features.Editor.Extensions;

namespace EasyBuildSystem.Features.Editor.Window
{
    public class PackageImporter : EditorWindow
    {
        const string VERSION = "6.1";

        static bool m_ImportedPackageSupport;
        static bool m_ImportedPackageXRSupport;
        static bool m_ImportedInputSystem;
        static bool m_ImportedXRManagement;
        static bool m_ImportedXRInteractionToolkit;

        static bool m_CancelImport;

        static AddRequest m_AddRequest;

        public static IEnumerator CurrentDownload;

        public static void Init()
        {
            EditorWindow window = GetWindow(typeof(PackageImporter), false, "Package Importer", true);
            window.titleContent.image = EditorGUIUtility.IconContent("d__Popup").image;
            window.autoRepaintOnSceneChange = true;

            int width = 600;
            int height = 240;
            int x = (Screen.currentResolution.width - width) / 2;
            int y = (Screen.currentResolution.height - height) / 2;

            window.minSize = new Vector2(width, height);
            window.position = new Rect(x, y, width, height);

            window.Show(true);

            EditorApplication.UnlockReloadAssemblies();
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(3f);

            GUILayout.BeginVertical();

            GUILayout.Space(3f);

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Package Importer",
                "Package Importer allows you to import some specific packages that work with Easy Build System.\n" +
                "You can consult the documentation for more information about each support available here.");

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Unity Version :"), GUILayout.Width(90));
            GUI.color = Color.green;
            GUILayout.BeginVertical();
            GUILayout.Label(Application.unityVersion, GUILayout.Width(150));
            GUILayout.EndVertical();
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Render Pipeline :"), GUILayout.Width(100));
            if (GraphicsSettings.currentRenderPipeline)
            {
                if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                {
                    GUI.color = Color.green;
                    GUILayout.BeginVertical();
                    GUILayout.Label("High Definition Render Pipeline", GUILayout.Width(350));
                    GUILayout.EndVertical();
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.green;
                    GUILayout.BeginVertical();
                    GUILayout.Label("Universal Render Pipeline", GUILayout.Width(350));
                    GUILayout.EndVertical();
                    GUI.color = Color.white;
                }
            }
            else
            {
                GUI.color = Color.green;
                GUILayout.BeginVertical();
                GUILayout.Label("Built-in Render Pipeline", GUILayout.Width(350));
                GUILayout.EndVertical();
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Package Version :"), GUILayout.Width(110));
            GUI.color = Color.green;
            GUILayout.BeginVertical();
            GUILayout.Label("Release " + VERSION, GUILayout.Width(150));
            GUILayout.EndVertical();
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Internet Reachability :"), GUILayout.Width(130));

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                GUI.color = Color.yellow;
                GUILayout.BeginVertical();
                GUILayout.Label("UnReachable", GUILayout.Width(150));
                GUILayout.EndVertical();
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.green;
                GUILayout.BeginVertical();
                GUILayout.Label("Reachable", GUILayout.Width(150));
                GUILayout.EndVertical();
                GUI.color = Color.white;
            }

            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            string manifestPath;

            GUI.enabled = GraphicsSettings.currentRenderPipeline;

            if (GUILayout.Button("Import HDRP & URP Support..."))
            {
                if (GraphicsSettings.currentRenderPipeline)
                {
                    if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                    {
                        manifestPath = AssetDatabase.GetAssetPath(Resources.Load("Packages/Supports/manifest"));
                        AssetDatabase.ImportPackage(manifestPath.Replace("manifest.prefab", "HDRP Support.unitypackage"), true);
                    }
                    else
                    {
                        manifestPath = AssetDatabase.GetAssetPath(Resources.Load("Packages/Supports/manifest"));
                        AssetDatabase.ImportPackage(manifestPath.Replace("manifest.prefab", "URP Support.unitypackage"), true);
                    }
                }
            }

            GUI.enabled = true;

#if EBS_INPUT_SYSTEM_SUPPORT
            GUI.enabled = false;
#else
            GUI.enabled = true;
#endif

            if (GUILayout.Button("Import Unity Input System Support..."))
            {
                if (EditorUtility.DisplayDialog("Easy Build System - Import Package",
                       "This will download and import the Unity Input System (1.4.3) from the Package Manager and the required support package for working with the system.", "Import", "Cancel"))
                {
                    EditorApplication.update += ProcessDownload;
                    CurrentDownload = InstallSupports(false);
                }
            }

            GUI.enabled = true;

#if EBS_XR
            GUI.enabled = false;
#else
            GUI.enabled = true;
#endif

            if (GUILayout.Button("Import XR Interaction Toolkit Support..."))
            {
                if (EditorUtility.DisplayDialog("Easy Build System - Import Package",
                       "This will download and import the Unity Input System (1.4.3) and XR Interaction Toolkit (2.1.1) from the Package Manager and the required support package for working with the system.", "Import", "Cancel"))
                {
                    EditorApplication.update += ProcessDownload;
                    CurrentDownload = InstallSupports(true);
                }
            }

            GUI.enabled = true;

            if (GUILayout.Button("Import NavMesh Components Support..."))
            {
                if (EditorUtility.DisplayDialog("Easy Build System - Import Package",
                       "This will import the NavMeshComponents support for working with the system.", "Import", "Cancel"))
                {
                    manifestPath = AssetDatabase.GetAssetPath(Resources.Load("Packages/Supports/manifest"));
                    AssetDatabase.ImportPackage(manifestPath.Replace("manifest.prefab", "NavMesh Components Support.unitypackage"), true);
                }
            }

            if (GUILayout.Button("(2021.1) Import AI Navigation Support..."))
            {
                if (EditorUtility.DisplayDialog("Easy Build System - Import Package",
                       "This will import the new AI Navigation support for working with the system.", "Import", "Cancel"))
                {
                    manifestPath = AssetDatabase.GetAssetPath(Resources.Load("Packages/Supports/manifest"));
                    AssetDatabase.ImportPackage(manifestPath.Replace("manifest.prefab", "AI Navigation Support.unitypackage"), true);

                    IntegrationManager.EnableIntegration("ENABLE_AI_NAVIGATION", () =>
                    {

                    });
                }
            }

            GUILayout.Space(3f);

            GUILayout.EndVertical();

            GUILayout.Space(3f);

            GUILayout.EndHorizontal();
        }

        public static void ProcessDownload()
        {
            if (CurrentDownload != null)
            {
                CurrentDownload.MoveNext();
            }
        }

        static void DownloadInputSystem()
        {
            if (m_AddRequest.IsCompleted)
            {
                EditorApplication.update -= DownloadInputSystem;
                m_ImportedInputSystem = true;
            }
        }

        static void DownloadXRManagement()
        {
            if (m_AddRequest.IsCompleted)
            {
                EditorApplication.update -= DownloadXRManagement;
                m_ImportedXRManagement = true;
            }
        }

        static void DownloadXRInteractionToolkit()
        {
            if (m_AddRequest.IsCompleted)
            {
                EditorApplication.update -= DownloadXRInteractionToolkit;
                m_ImportedXRInteractionToolkit = true;
            }
        }

        public static IEnumerator InstallSupports(bool installXR)
        {
            string manifestPath = "";

            EditorApplication.LockReloadAssemblies();

            if (installXR)
            {
                m_AddRequest = Client.Add("com.unity.xr.management@4.2.1");

                EditorApplication.update += DownloadXRManagement;

                while (!m_ImportedXRManagement)
                {
                    yield return new WaitForEndOfFrame();
                }

                m_AddRequest = Client.Add("com.unity.xr.interaction.toolkit@2.1.1");

                EditorApplication.update += DownloadXRInteractionToolkit;

                while (!m_ImportedXRInteractionToolkit)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            m_AddRequest = Client.Add("com.unity.inputsystem@1.4.3");

            EditorApplication.update += DownloadInputSystem;

            while (!m_ImportedInputSystem)
            {
                yield return new WaitForEndOfFrame();
            }

            manifestPath = AssetDatabase.GetAssetPath(Resources.Load("Packages/Supports/manifest"));
            AssetDatabase.ImportPackage(manifestPath.Replace("manifest.prefab", "Input System Support.unitypackage"), true);

            AssetDatabase.importPackageCompleted += (string packageName) => { m_ImportedPackageSupport = true; };
            AssetDatabase.importPackageCancelled += (string packageName) => { m_CancelImport = true; EditorApplication.UnlockReloadAssemblies(); };

            if (m_CancelImport)
            {
                m_CancelImport = false;
                yield break;
            }

            while (!m_ImportedPackageSupport)
            {
                yield return new WaitForEndOfFrame();
            }

            IntegrationManager.EnableIntegration("ENABLE_INPUT_SYSTEM", () =>
            {

            });

            IntegrationManager.EnableIntegration("EBS_INPUT_SYSTEM_SUPPORT", () =>
            {
                Debug.Log("<b>Easy Build System</b> : New Input System support has been installed!");
            });

            if (installXR)
            {
                manifestPath = AssetDatabase.GetAssetPath(Resources.Load("Packages/Supports/manifest"));
                AssetDatabase.ImportPackage(manifestPath.Replace("manifest.prefab", "XR Interaction Toolkit Support.unitypackage"), true);

                AssetDatabase.importPackageCompleted += (string packageName) => { m_ImportedPackageXRSupport = true; };
                AssetDatabase.importPackageCancelled += (string packageName) => { m_CancelImport = true; EditorApplication.UnlockReloadAssemblies(); };

                if (m_CancelImport)
                {
                    m_CancelImport = false;
                    yield break;
                }

                while (!m_ImportedPackageXRSupport)
                {
                    yield return new WaitForEndOfFrame();
                }

                IntegrationManager.EnableIntegration("EBS_XR", () =>
                {
                    Debug.Log("<b>Easy Build System</b> : XR Interaction Toolkit support has been installed!");
                });
            }

            EditorApplication.UnlockReloadAssemblies();

            yield break;
        }
    }
}