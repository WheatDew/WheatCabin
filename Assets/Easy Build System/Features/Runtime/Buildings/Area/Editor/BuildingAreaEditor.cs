/// <summary>
/// Project : Easy Build System
/// Class : BuildingAreaEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Area.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Editor.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Area.Editor
{
    [CustomEditor(typeof(BuildingArea))]
    public class BuildingAreaEditor : UnityEditor.Editor
    {
        #region Fields

        bool m_GeneralFoldout = true;
        bool m_RestrictionFoldout;

        #endregion

        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Area", "Limit the building actions (Placement, Destruction, Edition) of Building Parts in the area.\n" +
                "You can find more information on the Building Area component in the documentation.");

            m_GeneralFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("General Settings"), m_GeneralFoldout);

            if (m_GeneralFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Shape"), new GUIContent("Area Shape", "Area shape (sphere, bounds)."));

                if (serializedObject.FindProperty("m_Shape").enumValueIndex == (int)BuildingArea.ShapeType.BOUNDS)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Bounds"),
                        new GUIContent("Area Bounds", "Area bounds."));
                }
                else
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Radius"),
                        new GUIContent("Area Radius", "Area sphere radius."));
                }
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_RestrictionFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Restriction Settings"), m_RestrictionFoldout);

            if (m_RestrictionFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanPlacingAnyBuildingParts"),
                    new GUIContent("Can Placing Any Building Parts", "Can placing any Building Parts in the area."));

                if (serializedObject.FindProperty("m_CanPlacingAnyBuildingParts").boolValue == false)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanPlacingSpecificBuildingParts"),
                        new GUIContent("Can Placing Specific Building Parts", "Can placing only specific Building Parts in this area."), true);
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanEditingAnyBuildingParts"),
                    new GUIContent("Can Editing Any Building Parts", "Can editing any Building Parts in the area."));

                if (serializedObject.FindProperty("m_CanEditingAnyBuildingParts").boolValue == false)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanEditingSpecificBuildingParts"),
                        new GUIContent("Can Editing Specific Building Parts", "Can editing only specific Building Parts in this area."), true);
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanDestroyingAnyBuildingParts"),
                    new GUIContent("Can Destroying Any Building Parts", "Can destroying any Building Parts in the area."));

                if (serializedObject.FindProperty("m_CanDestroyingAnyBuildingParts").boolValue == false)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanDestroyingSpecificBuildingParts"),
                        new GUIContent("Can Destroying Specific Building Parts", "Can destroying only specific Building Parts in this area."), true);
                }
            }

            EditorGUIUtilityExtension.EndFoldout();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}