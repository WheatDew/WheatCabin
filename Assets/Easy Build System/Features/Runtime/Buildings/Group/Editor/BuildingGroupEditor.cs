/// <summary>
/// Project : Easy Build System
/// Class : BuildingGroupEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Group.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

namespace EasyBuildSystem.Features.Runtime.Buildings.Group.Editor
{
    [CustomEditor(typeof(BuildingGroup))]
    public class BuildingGroupEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingGroup Target
        {
            get
            {
                return ((BuildingGroup)target);
            }
        }

        #endregion

        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.TextField(new GUIContent("Group Identifier", "Allows to identify the Building Group."), Target.Identifier);
            GUI.enabled = true;

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}