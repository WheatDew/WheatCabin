/// <summary>
/// Project : Easy Build System
/// Class : IntegrationManager.cs
/// Namespace : EasyBuildSystem.Features.Editor.Window
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Editor.Extensions;

namespace EasyBuildSystem.Features.Editor.Window
{
    public class IntegrationManager : EditorWindow
    {
        static List<BuildTargetGroup> m_Targets;
        static readonly List<string> m_Integrations = new List<string>();

        Vector2 m_ScrollPos;

        void OnGUI()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(5f);

            GUILayout.BeginVertical();

            GUILayout.Space(5f);

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Integration Manager",
                "Integration Manager allows you to import integrations that work with Easy Build System.\n" +
                "You can consult the documentation for more information about each integration available here.");

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

            GUILayout.Label("Asset Store Integrations", EditorStyles.boldLabel);

            EditorGUILayout.Separator();

            AddIntegration("GameCreator V2", "Catsoft Works",
                "https://assetstore.unity.com/packages/tools/game-toolkits/game-creator-2-203069", "EBS_GAMECREATORV2", "GameCreator V2 Integration", "2.9.36", null, null);

            AddIntegration("Mirror", "Vis2k",
                "https://assetstore.unity.com/packages/tools/network/mirror-129321", "EBS_MIRROR", "Mirror Integration", "78.3.0", null, null);

            AddIntegration("PUN V2", "Exit Games",
                "https://assetstore.unity.com/packages/tools/network/pun-2-free-119922", "EBS_PUNV2", "PUN V2 Integration", "2.41", null, null);

            AddIntegration("FishNet", "FirstGearGames",
                "https://assetstore.unity.com/packages/tools/network/fish-net-networking-evolved-207815", "EBS_FISHNET", "FishNet Integration", "3.5.8hf0", null, null);

            //AddIntegration("(Planned) Playmaker", "PLAYMAKER",
            //    "https://assetstore.unity.com/packages/tools/visual-scripting/playmaker-368", "Playmaker Integration", null, null);

            AddIntegration("uSurvival", "Vis2k",
                "https://assetstore.unity.com/packages/templates/systems/usurvival-multiplayer-survival-95015", "EBS_USURVIVAL", "uSurvival Integration", "1.86", null, null);

            AddIntegration("RPGBuilder", "Blink",
                "https://assetstore.unity.com/packages/tools/game-toolkits/rpg-builder-177657", "EBS_RPG_BUILDER", "RPGBuilder Integration", "2.0.7.1", null, null);

            EditorGUILayout.EndScrollView();

            GUILayout.Space(5f);

            GUILayout.EndVertical();

            GUILayout.Space(5f);

            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        public static void Init()
        {
            EditorWindow window = GetWindow(typeof(IntegrationManager), false, "Integration Manager", true);

            window.titleContent.image = EditorGUIUtility.IconContent("d_SceneViewFx").image;
            window.autoRepaintOnSceneChange = true;

            int width = 600;
            int height = 427;
            int x = (Screen.currentResolution.width - width) / 2;
            int y = (Screen.currentResolution.height - height) / 2;

            window.minSize = new Vector2(width, height);
            window.position = new Rect(x, y, width, height);

            window.Show(true);
        }

        void AddIntegration(string name, string author, string link, string defName, string path, string version, Action onEnable, Action onDisable)
        {
            EditorGUIUtilityExtension.BeginVertical();

            GUILayout.BeginHorizontal();

            GUILayout.Label(name + " by " + author, EditorStyles.whiteLargeLabel);
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.Space(6f);
            GUI.color = Color.yellow;
            GUILayout.Label("Version " + version, EditorStyles.miniLabel);
            GUI.color = Color.white;
            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            GUILayout.Space(3f);

            GUILayout.BeginHorizontal();

            if (!HasSymbol(defName))
            {
                if (GUILayout.Button("Enable Integration", GUILayout.Width(130)))
                {
                    EnableIntegration(defName, onEnable);

                    if (GetRelativePath(path) != string.Empty)
                    {
                        AssetDatabase.ImportPackage(GetRelativePath(path), true);
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Disable Integration", GUILayout.Width(130)))
                {
                    DisableIntegration(defName, onDisable);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(3f);

            EditorGUIUtilityExtension.LinkLabel(Truncate(link, 64), link);

            GUILayout.Space(5f);

            EditorGUIUtilityExtension.EndVertical();
        }

        string GetRelativePath(string packageName)
        {
            string[] allPaths = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);

            for (int i = 0; i < allPaths.Length; i++)
                if (allPaths[i].Contains(packageName))
                    return allPaths[i];

            return string.Empty;
        }

        string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        static bool HasSymbol(string name)
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains(name);
        }

        public static void DisableIntegration(string name, Action onDisable)
        {
            if (HasSymbol(name) == false)
            {
                return;
            }

            if (onDisable != null)
            {
                onDisable.Invoke();
            }

            m_Targets = new List<BuildTargetGroup>
            {
                BuildTargetGroup.iOS,

                BuildTargetGroup.WebGL,

                BuildTargetGroup.Standalone,

                BuildTargetGroup.Android
            };

            foreach (BuildTargetGroup target in m_Targets)
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                string[] splitArray = symbols.Split(';');

                List<string> array = new List<string>(splitArray);

                array.Remove(name);

                if (target != BuildTargetGroup.Unknown)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", array.ToArray()));
                }
            }
        }

        public static void EnableIntegration(string name, Action onEnable)
        {
            if (HasSymbol(name))
            {
                return;
            }

            m_Targets = new List<BuildTargetGroup>
            {
                BuildTargetGroup.iOS,

                BuildTargetGroup.WebGL,

                BuildTargetGroup.Standalone,

                BuildTargetGroup.Android,
            };

            foreach (BuildTargetGroup target in m_Targets)
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                string[] splitArray = symbols.Split(';');

                List<string> array = new List<string>(splitArray)
            {
                name
            };

                if (target != BuildTargetGroup.Unknown)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", array.ToArray()));
                }
            }

            if (onEnable != null)
            {
                m_Integrations.Add(onEnable.Method.Name);

                onEnable.Invoke();
            }
        }
    }
}