/// <summary>
/// Project : Easy Build System
/// Class : BuildingPlacerEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Editor.Extensions;

using EasyBuildSystem.Features.Runtime.Buildings.Placer.InputHandler;

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer.Editor
{
    [CustomEditor(typeof(BuildingPlacer), true)]
    public class BuildingPlacerEditor : UnityEditor.Editor
    {
        #region Fields

        bool m_InputFoldout;
        bool m_RaycastFoldout;
        bool m_SnappingFoldout;
        bool m_PreviewFoldout;
        bool m_AudioFoldout;

        List<Type> m_InputHandlers = new List<Type>();
        string[] m_Handlers;
        int m_InputHandlerIndex;

        UnityEditor.Editor m_InputHandlerEditor;

        BuildingPlacer Target
        {
            get
            {
                return (BuildingPlacer)target;
            }
        }

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            List<Type> types = GetAllDerivedTypes(AppDomain.CurrentDomain, typeof(BaseInputHandler)).ToList();

            m_InputHandlers = types;

            m_Handlers = new string[m_InputHandlers.Count];

            for (int i = 0; i < m_InputHandlers.Count; i++)
            {
                m_Handlers[i] = Regex.Replace(m_InputHandlers[i].Name, "([a-z])([A-Z])", "$1 $2");
            }

            BaseInputHandler[] inputHandlers = Target.GetComponents<BaseInputHandler>();

            if (inputHandlers.Length > 1)
            {
                for (int i = 1; i < inputHandlers.Length; i++)
                {
                    DestroyImmediate(inputHandlers[i]);
                }
            }

            if (Target.GetInputHandler != null)
            {
                m_InputHandlerIndex = m_InputHandlers.IndexOf(Target.GetInputHandler.GetType());
                Target.GetInputHandler.hideFlags = HideFlags.None;
            }
            else
            {
                m_InputHandlerIndex = m_InputHandlers.IndexOf(typeof(StandaloneInputHandler));
            }
        }

        void OnDisable()
        {
            //DestroyImmediate(m_InputHandlerEditor);
        }

        public Type[] GetAllDerivedTypes(AppDomain appDomain, Type targetType)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = appDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (type != typeof(BaseInputHandler))
                    {
                        if (type == targetType || type.IsSubclassOf(targetType))
                        {
                            result.Add(type);
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Placer", "Manages all aspects of building modes (Placement, Destruction, Edition).\n" +
                "You can find more information on the Building Placer component in the documentation.");

            m_InputFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Input Settings"), m_InputFoldout);

            if (m_InputFoldout)
            {
                EditorGUI.BeginChangeCheck();
                m_InputHandlerIndex = EditorGUILayout.Popup("Input Handler", m_InputHandlerIndex, m_Handlers);
                if (EditorGUI.EndChangeCheck())
                {
                    if (Target.GetInputHandler != null)
                    {
                        DestroyImmediate(Target.GetInputHandler);
                    }

                    if (m_InputHandlerEditor != null)
                    {
                        DestroyImmediate(m_InputHandlerEditor);
                        m_InputHandlerEditor = null;
                    }

                    Target.GetInputHandler = (BaseInputHandler)Target.gameObject.AddComponent(m_InputHandlers[m_InputHandlerIndex]);
                    Target.GetInputHandler.hideFlags = HideFlags.HideInInspector;

                    Repaint();

                    m_InputFoldout = true;
                }

                if (m_InputHandlerEditor == null)
                {
                    m_InputHandlerEditor = CreateEditor(Target.GetInputHandler);
                }
                else
                {
                    m_InputHandlerEditor.OnInspectorGUI();
                }

                EditorUtility.SetDirty(target);
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_RaycastFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Raycast Settings"), m_RaycastFoldout);

            if (m_RaycastFoldout)
            {
                if (serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_Camera").objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("This component require a camera to work!", MessageType.Warning);

                    GUI.color = Color.yellow;
                }
                else
                {
                    GUI.color = Color.white;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_Camera"),
                    new GUIContent("Raycast Camera", "Raycast camera."));
                GUI.color = Color.white;

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_ViewType"),
                    new GUIContent("Raycast View Type", "Raycast view type.\n" +
                    "First Person View : The raycast originates from the camera center and goes forward.\n" +
                    "Third Person View : The raycast originates from a custom transform and goes forward.\n" +
                    "Top Down View : The raycast originates from the camera center and goes to the mouse position."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_FromTransform"),
                    new GUIContent("Raycast From Transform", "Transform on which the raycast will start."));
#if EBS_XR
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_RaycastFromXRInteractor"));
#endif
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_LayerMask"),
                    new GUIContent("Raycast Layers", "Raycast layers."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_Through"),
                    new GUIContent("Raycast Passing Through", "Raycast passes through the colliders."));
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_Distance"),
                    new GUIContent("Raycast Distance", "Raycast distance."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_MaxDistance"),
                    new GUIContent("Raycast Max Distance", "Raycast max distance."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_OffsetPosition"),
                    new GUIContent("Raycast Offset Position", "Cast a ray from an offset position, useful for adjusting the position of the origin raycast."));
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_SnappingFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Snapping Settings"), m_SnappingFoldout);

            if (m_SnappingFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_Type"),
                    new GUIContent("Snapping Type", "Snapping detection type."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_LayerMask"),
                    new GUIContent("Snapping Layers", "Layers for snapping detection."));

                serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_MaxAngle").floatValue =
                    EditorGUILayout.Slider(new GUIContent("Snapping Max Angle", "Max angle to detect the Building Sockets."), 
                    serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_MaxAngle").floatValue, 0, 360);
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_PreviewFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Preview Settings"), m_PreviewFoldout);

            if (m_PreviewFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Type"),
                    new GUIContent("Preview Movement Type", "Preview movement type."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_MovementGridSize"),
                    new GUIContent("Preview Movement Grid Size", "Grid size to which the preview will be moved."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_MovementGridOffset"),
                    new GUIContent("Preview Movement Grid Offset Position", "Grid offset for the preview position."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_MovementSmoothTime"),
                    new GUIContent("Preview Movement Smooth Time", "Preview movement smooth time."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_LockRotation"),
                    new GUIContent("Preview Lock Rotation", "Lock the preview with the camera rotation."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ResetRotation"),
                    new GUIContent("Preview Reset Rotation", "Reset the preview rotation after placing or canceling."));
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_AudioFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Audio Settings"), m_AudioFoldout);

            if (m_AudioFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSettings").FindPropertyRelative("m_AudioSource"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSettings").FindPropertyRelative("m_PlacingAudioClips"),
                    new GUIContent("Audio Placing Clips"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSettings").FindPropertyRelative("m_EditingAudioClips"),
                    new GUIContent("Audio Editing Clips"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSettings").FindPropertyRelative("m_DestroyAudioClips"),
                    new GUIContent("Audio Destroying Clips"));
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