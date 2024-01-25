/// <summary>
/// Project : Easy Build System
/// Class : BuildingSaverEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEditor;

using EasyBuildSystem.Features.Editor.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Manager.Saver.Editor
{
    [CustomEditor(typeof(BuildingSaver), true)]
    public class BuildingSaverEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingSaver Target
        {
            get
            {
                return ((BuildingSaver)target);
            }
        }

        bool m_GeneralFoldout = true;

        #endregion

        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Saver", "Save and load the position, rotation, and scale of all Building Parts in a scene.\n" +
                "You can find more information on the Building Saver component in the documentation.");

            m_GeneralFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("General Settings"), m_GeneralFoldout);

            if (m_GeneralFoldout)
            {
                GUI.enabled = false;
                EditorGUILayout.TextField("Scene Identifier", "data_" + SceneManager.GetActiveScene().name.Replace(" ", "") + "_save.txt");
                GUI.enabled = true;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UseAutoSaver"), new GUIContent("Use Auto Saver",
                    "Allows to save all the Building Parts at each interval. Useful for avoiding losing or corrupting your save file if crashing."));

                if (serializedObject.FindProperty("m_UseAutoSaver").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AutoSaverInterval"), new GUIContent("Auto Saver Interval",
                        "How often will the save be called per second."));
                }

                string loadPath = string.Empty;

                if (Application.platform == RuntimePlatform.Android)
                {
                    loadPath = SceneManager.GetActiveScene().name + "_save";
                }
                else
                {
                    loadPath = Application.persistentDataPath + "/data_" + SceneManager.GetActiveScene().name.Replace(" ", "") + "_save.txt";
                }

                GUI.enabled = System.IO.File.Exists(loadPath);

                if (GUILayout.Button("Load Saving File..."))
                {
                    Target.ForceLoad(loadPath);
                }

                if (GUILayout.Button("Delete Saving File..."))
                {
                    if (EditorUtility.DisplayDialog("Easy Build System - Delete Saving File...",
                        "This deletes the saving file that may contain your Building Parts data from the current scene, do you want to continue?", "Yes", "Cancel"))
                    {
                        System.IO.File.Delete(loadPath);

                        Debug.Log("<b>Easy Build System</b> : Saving file has been deleted.");
                    }
                }

                GUI.enabled = true;
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