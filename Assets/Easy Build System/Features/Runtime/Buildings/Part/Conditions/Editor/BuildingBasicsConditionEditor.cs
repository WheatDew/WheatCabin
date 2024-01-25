/// <summary>
/// Project : Easy Build System
/// Class : BuildingBasicsConditionEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions.Editor
{
    [CustomEditor(typeof(BuildingBasicsCondition))]
    public class BuildingBasicsConditionEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanPlacing"), 
                new GUIContent("Building Can Placing", "Can place the Building Part?"));

            if (serializedObject.FindProperty("m_CanPlacing").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RequireArea"),
                    new GUIContent("Building Can Placing Only Area", "Require a Building Area to be placed?"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RequireSocket"),
                    new GUIContent("Building Can Placing Only Socket", "Needs snapped on a Building Socket to be placed?"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanEditing"),
                new GUIContent("Building Can Editing", "Can edit the Building Part?"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanDestroying"),
                new GUIContent("Building Can Destroying", "Can destroy the Building Part?"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IgnoreSocket"),
                new GUIContent("Building Ignore Snapping", "Ignore the snapping with all the Building Sockets."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowDebugs"), 
                new GUIContent("Show Debugs"));

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

        #region Internal Methods

        #endregion
    }
}