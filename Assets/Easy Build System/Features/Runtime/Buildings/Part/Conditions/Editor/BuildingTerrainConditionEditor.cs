/// <summary>
/// Project : Easy Build System
/// Class : BuildingTerrainConditionEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions.Editor
{
    [CustomEditor(typeof(BuildingTerrainCondition))]
    public class BuildingTerrainConditionEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CheckTreesCollision"),
                new GUIContent("Building Terrain Trees Collision", "Check the collision with the Unity Terrain's trees."));

            if (serializedObject.FindProperty("m_CheckTreesCollision").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CheckTreesCollisionDistance"), 
                    new GUIContent("Building Terrain Trees Collision Radius", "Check trees collision distance."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ClearGrassDetails"),
                new GUIContent("Building Terrain Clear Grass", "Clear terrain details at placement."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ClearGrassBounds"),
                new GUIContent("Building Terrain Clear Grass Bounds"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowDebugs"), new GUIContent("Show Debugs"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowGizmos"), new GUIContent("Show Gizmos"));

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}