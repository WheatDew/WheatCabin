/// <summary>
/// Project : Easy Build System
/// Class : BuildingTypeEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Manager.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using EasyBuildSystem.Features.Editor.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Manager.Editor
{
    public class BuildingTypeEditor : EditorWindow
    {
        #region Fields

        Vector2 m_ScrollPos;

        #endregion

        #region Unity Methods

        public static void Init()
        {
            EditorWindow window = CreateInstance<BuildingTypeEditor>();
            window.titleContent = new GUIContent("Easy Build System - Building Types");
            window.minSize = new Vector2(530, 300);
            window.maxSize = new Vector2(530, 300);
            window.ShowUtility();
        }

        void OnGUI()
        {
            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Types",
                    "Adding, removing, and editing the Building Types here.\n" +
                    "You can find more information on the Building Manager component in the documentation.");

            m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

            List<string> m_AllBuildingTypes = BuildingType.Instance.AllBuildingTypes;

            for (int i = 0; i < m_AllBuildingTypes.Count; i++)
            {
                GUILayout.BeginHorizontal();

                if (m_AllBuildingTypes[i] == string.Empty)
                {
                    m_AllBuildingTypes.RemoveAt(i);
                    return;
                }

                m_AllBuildingTypes[i] = EditorGUILayout.TextField(m_AllBuildingTypes[i]);

                if (GUILayout.Button("Remove"))
                {
                    m_AllBuildingTypes.RemoveAt(i);
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Create Building Type..."))
            {
                m_AllBuildingTypes.Add("New Type");

                EditorUtility.SetDirty(BuildingType.Instance);
            }

            if (GUILayout.Button("Close..."))
            {
                Close();
            }
        }

        #endregion
    }
}