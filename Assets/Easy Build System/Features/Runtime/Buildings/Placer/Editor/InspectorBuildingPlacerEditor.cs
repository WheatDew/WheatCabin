/// <summary>
/// Project : Easy Build System
/// Class : InspectorBuildingPlacerEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Editor.Extensions;


namespace EasyBuildSystem.Features.Runtime.Buildings.Placer.Editor
{
    [CustomEditor(typeof(InspectorBuildingPlacer))]
    public class InspectorBuildingPlacerEditor : UnityEditor.Editor
    {
        #region Fields

        static BuildingPlacer m_Builder;
        static BuildingPlacer Builder
        {
            get
            {
                if (m_Builder == null)
                {
                    if (FindObjectOfType<InspectorBuildingPlacer>() != null)
                    {
                        m_Builder = FindObjectOfType<InspectorBuildingPlacer>();
                    }
                    else
                    {
                        m_Builder = new GameObject("(Instance) Building Placer Editor").AddComponent<InspectorBuildingPlacer>();
                    }

                    m_Builder.GetRaycastSettings.ViewType = BuildingPlacer.RaycastSettings.RaycastType.TOP_DOWN_VIEW;
                    m_Builder.GetRaycastSettings.Distance = 100f;

                    m_Builder.GetSnappingSettings.MaxAngles = 5f;
                }

                return m_Builder;
            }
        }

        int m_BuildingSelectionIndex = 0;
        string[] m_BuildingCategory;

        Vector2 m_ScrollPosition;

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            List<string> category = new List<string>();

            for (int i = 0; i < BuildingManager.Instance.BuildingPartReferences.Count; i++)
            {
                BuildingPart partReference = BuildingManager.Instance.BuildingPartReferences[i];

                if (partReference != null)
                {
                    if (!category.Contains(partReference.GetGeneralSettings.Type))
                    {
                        category.Add(partReference.GetGeneralSettings.Type);
                    }
                }
            }

            m_BuildingCategory = category.ToArray();

            serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_MaxAngle").floatValue = 15f;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Placer", "Place, destroy, and edit building parts directly from the Unity Editor.\n" +
                "You can find more information on the Building Placer component in the documentation.");

            EditorGUIUtilityExtension.BeginVertical();

            if (serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_Camera").objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("This component require a camera to work!", MessageType.Warning);

                GUI.color = Color.yellow;
            }
            else
            {
                GUI.color = Color.white;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Type"),
                new GUIContent("Placing Type"));

            if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Type").enumValueIndex == (int)BuildingPlacer.PreviewSettings.MovementType.GRID)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_MovementGridSize"),
                    new GUIContent("Placing Grid Size"));
            }

            if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Type").enumValueIndex == (int)BuildingPlacer.PreviewSettings.MovementType.SMOOTH)
            {
                serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Type").enumValueIndex = (int)BuildingPlacer.PreviewSettings.MovementType.NORMAL;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastSettings").FindPropertyRelative("m_LayerMask"),
                new GUIContent("Raycast Layers", "Preview Raycast layers."));

            serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_MaxAngle").floatValue =
                EditorGUILayout.Slider(new GUIContent("Snapping Max Angle", "Max angle to detect the Building Sockets."),
                serializedObject.FindProperty("m_SnappingSettings").FindPropertyRelative("m_MaxAngle").floatValue, 0, 360);

            EditorGUIUtilityExtension.EndVertical();

            EditorGUIUtilityExtension.BeginVertical();

            GUILayout.Space(3);

            GUI.color = Color.white / 1.15f;
            GUILayout.Label("Buildings", EditorStyles.whiteLargeLabel);
            GUI.color = Color.white;

            GUILayout.Space(3);

            if (Builder.GetSelectedBuildingPart != null)
            {
                EditorGUIUtilityExtension.BeginVertical();

                GUILayout.BeginHorizontal();

                GUILayout.Button(Builder.GetSelectedBuildingPart.GetGeneralSettings.Thumbnail != null ?
                            new GUIContent(Builder.GetSelectedBuildingPart.GetGeneralSettings.Thumbnail) : EditorGUIUtility.IconContent("d__Help@2x"),
                               GUILayout.Width(80), GUILayout.Height(80));

                GUILayout.BeginVertical();

                EditorGUILayout.Separator();

                GUI.enabled = false;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Identifier :", GUILayout.Width(65));
                Builder.GetSelectedBuildingPart.GetGeneralSettings.Identifier =
                    GUILayout.TextField(Builder.GetSelectedBuildingPart.GetGeneralSettings.Identifier);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Name :", GUILayout.Width(65));
                Builder.GetSelectedBuildingPart.GetGeneralSettings.Name =
                    GUILayout.TextField(Builder.GetSelectedBuildingPart.GetGeneralSettings.Name);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Type :", GUILayout.Width(65));
                Builder.GetSelectedBuildingPart.GetGeneralSettings.Type =
                    GUILayout.TextField(Builder.GetSelectedBuildingPart.GetGeneralSettings.Type);
                GUILayout.EndHorizontal();

                GUI.enabled = true;

                if (GUILayout.Button("Edit Building Settings..."))
                {
                    EditorGUIUtility.PingObject(Builder.GetSelectedBuildingPart.gameObject);
                    Selection.activeObject = Builder.GetSelectedBuildingPart.gameObject;
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                EditorGUIUtilityExtension.EndVertical();
            }

            m_BuildingSelectionIndex = GUILayout.Toolbar(m_BuildingSelectionIndex, m_BuildingCategory);

            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, false, false);

            GUILayout.BeginHorizontal();
            for (int i = 0; i < BuildingManager.Instance.BuildingPartReferences.Count; i++)
            {
                if (BuildingManager.Instance.BuildingPartReferences[i].GetGeneralSettings.Type == m_BuildingCategory[m_BuildingSelectionIndex])
                {
                    if (GUILayout.Button(BuildingManager.Instance.BuildingPartReferences[i].GetGeneralSettings.Thumbnail != null ?
                        new GUIContent(BuildingManager.Instance.BuildingPartReferences[i].GetGeneralSettings.Thumbnail) : EditorGUIUtility.IconContent("d__Help@2x"),
                        GUILayout.Width(60), GUILayout.Height(60)))
                    {
                        Builder.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                        Builder.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
                        Builder.SelectBuildingPart(BuildingManager.Instance.BuildingPartReferences[i]);
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            if (Builder.GetBuildMode != BuildingPlacer.BuildMode.NONE)
            {
                GUILayout.Label("Shortcuts : Left Mouse = Validate | R = Rotate Preview", EditorStyles.centeredGreyMiniLabel);

                GUILayout.Space(5f);
            }

            GUILayout.BeginHorizontal();

            if (Builder.GetBuildMode != BuildingPlacer.BuildMode.NONE)
            {
                GUI.color = Color.red / 2f + Color.white / 1.3f;
                if (GUILayout.Button("Exit " + Builder.GetBuildMode.ToString() + " Mode"))
                {
                    Builder.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                }
                GUI.color = Color.white;
            }
            else
            {
                if (GUILayout.Button("Place Mode"))
                {
                    Builder.ChangeBuildMode(BuildingPlacer.BuildMode.PLACE);
                }

                if (GUILayout.Button("Destroy Mode"))
                {
                    Builder.ChangeBuildMode(BuildingPlacer.BuildMode.DESTROY);
                }

                if (GUILayout.Button("Edit Mode"))
                {
                    Builder.ChangeBuildMode(BuildingPlacer.BuildMode.EDIT);
                }
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Close Building Placer..."))
            {
                Builder.ChangeBuildMode(BuildingPlacer.BuildMode.NONE);
                DestroyImmediate(((InspectorBuildingPlacer)target).gameObject);
                return;
            }

            EditorGUIUtilityExtension.EndVertical();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion
    }
}