/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.UI
{
    using Opsive.UltimateCharacterController.UI;
    using UnityEditor;
    using UnityEditor.UI;

    /// <summary>
    /// The repeat button inherits the Unity Button component, and therefor needs to show the UI correctly
    /// </summary>
    [CustomEditor(typeof(RepeatButton))]
    public class MenuButtonEditor : ButtonEditor
    {
        private SerializedProperty m_InitialDelay;
        private SerializedProperty m_PressPerSecond;
        
        /// <summary>
        /// On editor enable.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            m_InitialDelay = serializedObject.FindProperty("m_InitialDelay");
            m_PressPerSecond = serializedObject.FindProperty("m_PressPerSecond");
        }

        /// <summary>
        /// On inspector GUI to draw the button editor.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Show default inspector property editor
            //DrawDefaultInspector();
            
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_InitialDelay);
            EditorGUILayout.PropertyField(m_PressPerSecond);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
