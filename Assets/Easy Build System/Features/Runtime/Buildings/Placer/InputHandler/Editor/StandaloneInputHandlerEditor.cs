/// <summary>
/// Project : Easy Build System
/// Class : StandaloneInputHandlerEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;
using EasyBuildSystem.Features.Editor.Window;

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler.Editor
{
    [CustomEditor(typeof(StandaloneInputHandler), true)]
    public class StandaloneInputHandlerEditor : UnityEditor.Editor
    {
        #region Unity Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

#if !EBS_INPUT_SYSTEM_SUPPORT

            EditorGUILayout.HelpBox("The new Input System Package has been detected on this project.\n" +
                "Please, import the Unity Input System support via the Package Importer to able use this.", MessageType.Warning);

            if (GUILayout.Button("Open Package Importer..."))
            {
                PackageImporter.Init();
            }

#else

#if ENABLE_INPUT_SYSTEM
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
#endif

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_BlockWhenCursorOverUI"),
                new GUIContent("Block When Pointer Is Over UI Element", "Prevent action keys from being used when the cursor is over a UI element.\n"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CanRotateBuildingPart"),
                new GUIContent("Rotate With Mouse Wheel", "Can rotate the preview with mouse wheel?"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CanSelectBuildingPart"),
                    new GUIContent("Can Select Building Part", "Can select Building Part with action key?"));

#if !ENABLE_INPUT_SYSTEM
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_ValidateActionKey"),
                    new GUIContent("Validation Action Key", "Action key to validate the current action."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_CancelActionKey"),
                    new GUIContent("Cancel Action Key", "Action key to cancel the current action."));
#endif

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UsePlacingModeShortcut"),
                    new GUIContent("Use Placing Mode Shortcut", "Action key to select Placing build mode."));

            if (serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UsePlacingModeShortcut").boolValue)
            {
#if !ENABLE_INPUT_SYSTEM
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_PlacingModeKey"),
                    new GUIContent("Placing Mode Key Shortcut", "Action key for placing the preview."));
#endif
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_ResetModeAfterPlacing"),
                    new GUIContent("Reset Mode After Placing", "Reset build mode to NONE after placing."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UseEditingModeShortcut"),
                    new GUIContent("Use Editing Mode Shortcut", "Action key to select Editing build mode."));

            if (serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UseEditingModeShortcut").boolValue)
            {
#if !ENABLE_INPUT_SYSTEM
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_EditingModeKey"),
                    new GUIContent("Editing Mode Key Shortcut", "Action key for editing the preview."));
#endif
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_ResetModeAfterEditing"),
                    new GUIContent("Reset Mode After Editing", "Reset build mode to NONE after editing."));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UseDestroyingModeShortcut"),
                    new GUIContent("Use Destroying Mode Shortcut", "Action key to select Destroy build mode."));

            if (serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_UseDestroyingModeShortcut").boolValue)
            {
#if !ENABLE_INPUT_SYSTEM
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_DestroyingModeKey"),
                    new GUIContent("Destroying Mode Key Shortcut", "Action key for destroy the preview."));
#endif
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_InputSettings").FindPropertyRelative("m_ResetModeAfterDestroying"),
                    new GUIContent("Reset Mode After Destroying", "Reset build mode to NONE after destroying."));
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