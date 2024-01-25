/// <summary>
/// Project : Easy Build System
/// Class : BuildingPhysicsConditionEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions.Editor
{
    [CustomEditor(typeof(BuildingPhysicsCondition))]
    public class BuildingPhysicsConditionEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingPhysicsCondition Target
        {
            get
            {
                return ((BuildingPhysicsCondition)target);
            }
        }

        #endregion

        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IsSleeping"),
                new GUIContent("Building Physics Is Sleeping", "If the physics condition is sleeping?"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FallingTime"),
                new GUIContent("Building Physics Falling Time", "Falling time before destroying the gameObject after being affected by physics."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanPlaceOnlyIfStable"),
                new GUIContent("Building Physics Can Placing When Stable", "Can only be placed if the Building Part is stable."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CheckStabilityInterval"),
                new GUIContent("Building Physics Check Stability Interval",
                "How many times per second will the CheckStability method be called? This can cause frame rate drops, so it is recommended not to set the value too low."));

            GUILayout.BeginHorizontal();
            GUILayout.Space(13);
            GUILayout.BeginVertical();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Points"), 
                new GUIContent("Building Physics Points",
                "Physics points that will need to hit a collider for the condition to return a stable value."));
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowDebugs"), new GUIContent("Show Debugs"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ShowGizmos"), new GUIContent("Show Gizmos"));

            GUI.enabled = !serializedObject.FindProperty("m_IsSleeping").boolValue && (Target.Points != null && Target.Points.Length != 0);
            if (GUILayout.Button("Check Physics Stability..."))
            {
                if (!Target.CheckStability())
                {
                    Debug.Log("<b>Easy Build System</b> : The Building Part is not stable.");
                }
                else
                {
                    Debug.Log("<b>Easy Build System</b> : The Building Part is stable.");
                }
            }
            GUI.enabled = true;

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