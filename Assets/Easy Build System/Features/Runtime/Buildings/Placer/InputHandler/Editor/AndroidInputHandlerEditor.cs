/// <summary>
/// Project : Easy Build System
/// Class : AndroidInputHandlerEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEditor;

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler.Editor
{
    [CustomEditor(typeof(AndroidInputHandler), true)]
    public class AndroidInputHandlerEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}