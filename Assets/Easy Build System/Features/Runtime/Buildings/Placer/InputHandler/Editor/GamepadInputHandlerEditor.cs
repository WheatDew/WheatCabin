/// <summary>
/// Project : Easy Build System
/// Class : GamepadInputHandlerEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;
using UnityEditor;

using EasyBuildSystem.Features.Editor.Window;

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler.Editor
{
    [CustomEditor(typeof(GamepadInputHandler), true)]
    public class GamepadInputHandlerEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

#if EBS_INPUT_SYSTEM_SUPPORT
            EditorGUILayout.HelpBox("New Input System is detecting, you can change the inputs directly in the Input Action file.\n" +
                "You read the documentation to have more information about the New Input System support.", MessageType.Info);

            if (GUILayout.Button("Edit Input Action Settings..."))
            {
                if (Resources.Load<UnityEngine.InputSystem.InputActionAsset>("Packages/Supports/Input System Support/Input Actions") != null)
                {
                    Selection.activeObject = Resources.Load<UnityEngine.InputSystem.InputActionAsset>("Packages/Supports/Input System Support/Input Actions");
                }
                else
                {
                    Debug.LogWarning("The default input action file <b>Input Actions</b> could be not found, the file not existing or has been renamed.");
                }
            }
#else
            EditorGUILayout.HelpBox("GamepadInputHandler requires New Input System support to work.\n" +
                "You can import the support package through the Package Importer by clicking below.", MessageType.Warning);

            if (GUILayout.Button("Open Package Importer..."))
            {
                PackageImporter.Init();
            }

#endif

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}