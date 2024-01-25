/// <summary>
/// Project : Easy Build System
/// Class : BuildingSocketEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Socket.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Linq;

using UnityEngine;

using UnityEditor;
using UnityEditor.Experimental.SceneManagement;

#if UNITY_2020_1_OR_NEWER
using UnityEditor.SceneManagement;
#endif

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using EasyBuildSystem.Features.Editor.Extensions;

namespace EasyBuildSystem.Features.Runtime.Buildings.Socket.Editor
{
    [CustomEditor(typeof(BuildingSocket))]
    public class BuildingSocketEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingSocket Target
        {
            get
            {
                return ((BuildingSocket)target);
            }
        }

        static BuildingSocket.SnappingPointSettings m_CurrentOffset;

        static bool m_GeneralFoldout = true;
        static bool m_SnappingFoldout;

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            PrefabStage.prefabStageDirtied += (PrefabStage stage) =>
            {
                EditorUtility.SetDirty(Target);
            };

            PrefabStage.prefabStageClosing += (PrefabStage stage) =>
            {
                Target.ClearPreview();
            };
        }

        void OnDisable()
        {
            Target.ClearPreview();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Target.Preview != null)
            {
                if (m_CurrentOffset != null)
                {
                    Target.Snap(Target.Preview, m_CurrentOffset, Vector3.zero);
                }
            }

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Socket", "Handles the snapping of Building Parts according offset positions.\n" +
                "You can find more information on the Building Socket component in the documentation.");

            m_GeneralFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("General Settings"), m_GeneralFoldout);

            if (m_GeneralFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SocketRadius"),
                    new GUIContent("Socket Radius", "Radius of the socket."));
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_SnappingFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Snapping Settings"), m_SnappingFoldout);

            if (m_SnappingFoldout)
            {
                EditorGUILayout.Separator();

                if (serializedObject.FindProperty("m_SnappingPoints").arraySize == 0)
                {
                    GUILayout.Label("Contains no snapping points...", EditorStyles.miniLabel);
                }
                else
                {
                    int index = 0;

                    foreach (BuildingSocket.SnappingPointSettings offset in Target.SnappingPoints.ToList())
                    {
                        if (offset == null)
                        {
                            return;
                        }

                        EditorGUIUtilityExtension.BeginVertical();

                        GUILayout.Space(3f);

                        GUILayout.BeginHorizontal();

                        GUILayout.Space(2f);

                        GUILayout.Label("Snapping Point #" + index, EditorStyles.whiteLargeLabel);

                        GUILayout.EndHorizontal();
                       
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_MatchBy"),
                            new GUIContent("Snapping Match By :", "Snapping match by type."));

                        EditorGUI.indentLevel++;

                        if ((BuildingSocket.SnappingPointSettings.MatchType)serializedObject.FindProperty("m_SnappingPoints").
                            GetArrayElementAtIndex(index).FindPropertyRelative("m_MatchBy").enumValueIndex == BuildingSocket.SnappingPointSettings.MatchType.BUILDING_PART_TYPE)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_Type"),
                                new GUIContent("Building Part Type", "Building Part type required to snap on this socket."));
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_BuildingPart"),
                                new GUIContent("Building Part Reference", "Building Part reference required to snap on this socket."));
                        }

                        EditorGUI.indentLevel--;

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_Position"),
                            new GUIContent("Snapping Position", "Position to which the Building Part will be snapped (relative to transform parent)."));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_Rotation"),
                            new GUIContent("Snapping Rotation", "Rotation to which the Building Part will be snapped (relative to transform parent)."));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SnappingPoints").GetArrayElementAtIndex(index).FindPropertyRelative("m_Scale"),
                            new GUIContent("Snapping Scale", "Scale to which the Building Part will be snapped (relative to transform parent)."));
                        
                        if (Target.Preview != null &&
                            (offset != null && offset.BuildingPart != null ? 
                            Target.Preview.GetGeneralSettings.Identifier == offset.BuildingPart.GetGeneralSettings.Identifier :
                            Target.Preview.GetGeneralSettings.Type == offset.Type))
                        {
                            GUI.color = Color.yellow;
                            if (GUILayout.Button("Hide Preview"))
                            {
                                for (int x = 0; x < Selection.gameObjects.Length; x++)
                                {
                                    BuildingSocket buildingSocket = Selection.gameObjects[x].GetComponent<BuildingSocket>();

                                    if (buildingSocket != null)
                                    {
                                        buildingSocket.ClearPreview();
                                    }

                                    EditorUtility.SetDirty(target);
                                }

                                return;
                            }
                            GUI.color = Color.white;

                            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                            {
                                GUI.enabled = !Application.isPlaying && Selection.gameObjects.Length <= 1;
                            }

                            if (GUILayout.Button("Instantiate Preview"))
                            {
                                m_CurrentOffset = Target.SnappingPoints[index];

                                for (int i = 0; i < Selection.gameObjects.Length; i++)
                                {
                                    BuildingSocket buildingSocket = Selection.gameObjects[i].GetComponent<BuildingSocket>();

                                    if (buildingSocket != null)
                                    {
                                        buildingSocket.ClearPreview();

                                        BuildingPart buildingPart = buildingSocket.GetOffsetBuildingPart(m_CurrentOffset);

                                        if (buildingPart != null)
                                        {
                                            BuildingSocket.SnappingPointSettings offsetSettings = buildingSocket.GetOffset(buildingPart);
                                            BuildingPart instancedBuildingPart = BuildingManager.Instance.PlaceBuildingPart(buildingPart,
                                                offsetSettings.Position, offsetSettings.Rotation, offsetSettings.Scale, false);
                                            buildingSocket.Snap(instancedBuildingPart, offsetSettings, Vector3.zero);
                                        }
                                    }
                                }
                            }
                            GUI.enabled = true;
                        }
                        else
                        {
                            if (m_CurrentOffset != null)
                            {
                                if (m_CurrentOffset.MatchBy == BuildingSocket.SnappingPointSettings.MatchType.BUILDING_PART_TYPE &&
                                    m_CurrentOffset.Type != string.Empty)
                                {
                                    m_CurrentOffset.BuildingPart = null;
                                }
                            }

                            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                            {
                                GUI.enabled = !Application.isPlaying && Selection.gameObjects.Length <= 1;
                            }

                            if (GUILayout.Button("Show Preview"))
                            {
                                m_CurrentOffset = Target.SnappingPoints[index];

                                for (int i = 0; i < Selection.gameObjects.Length; i++)
                                {
                                    BuildingSocket buildingSocket = Selection.gameObjects[i].GetComponent<BuildingSocket>();

                                    if (buildingSocket != null)
                                    {
                                        buildingSocket.ClearPreview();

                                        BuildingPart buildingPart = buildingSocket.GetOffsetBuildingPart(m_CurrentOffset);

                                        if (buildingPart != null)
                                        {
                                            buildingSocket.ShowPreview(buildingSocket.GetOffset(buildingPart));
                                        }
                                    }
                                }
                            }

                            GUI.enabled = true;
                        }

                        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                        {
                            GUI.enabled = Selection.gameObjects.Length <= 1;
                        }

                        EditorGUILayout.Separator();

                        if (GUILayout.Button("Duplicate Snapping Point"))
                        {
                            Undo.RecordObject(target, "Cancel duplicate offset");

                            BuildingSocket.SnappingPointSettings offsetSettings = Target.SnappingPoints[index];

                            Target.SnappingPoints.Add(new BuildingSocket.SnappingPointSettings() {
                                MatchBy = offsetSettings.MatchBy,
                                BuildingPart = offsetSettings.BuildingPart,
                                Type = offsetSettings.Type, 
                                Position = offsetSettings.Position,
                                Rotation = offsetSettings.Rotation,
                                Scale = offsetSettings.Scale 
                                });

                            EditorUtility.SetDirty(target);

                            m_CurrentOffset = Target.SnappingPoints[index];

                            return;
                        }

                        if (GUILayout.Button("Remove Snapping Point"))
                        {
                            Undo.RecordObject(target, "Cancel remove offset");
                            Target.SnappingPoints.Remove(Target.SnappingPoints[index]);
                            EditorUtility.SetDirty(target);
                            return;
                        }

                        GUI.enabled = true;

                        GUILayout.Space(1f);

                        EditorGUIUtilityExtension.EndVertical();

                        EditorGUILayout.Separator();

                        index++;
                    }
                }

                EditorGUILayout.Separator();

                EditorGUIUtilityExtension.BeginVertical();

                Rect dropRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));

                GUI.Box(dropRect, "Drag & Drop your Building Parts here to snapping it...", EditorStyles.centeredGreyMiniLabel);

                if (dropRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.DragUpdated)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        Event.current.Use();
                    }
                    else if (Event.current.type == EventType.DragPerform)
                    {
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                        {
                            GameObject draggedObject = DragAndDrop.objectReferences[i] as GameObject;

                            if (!PrefabUtility.IsPartOfPrefabAsset(draggedObject))
                            {
                                draggedObject = PrefabUtility.GetCorrespondingObjectFromSource(draggedObject);

                                if (draggedObject == null)
                                {
                                    Debug.LogError("<b>Easy Build System</b> : Object have not Building Part component or the prefab is not the original.");
                                    return;
                                }
                            }

                            BuildingPart draggedBuildingPart = draggedObject.GetComponent<BuildingPart>();

                            if (draggedBuildingPart == null)
                            {
                                Debug.LogError("<b>Easy Build System</b> : Missing Building Part component!");
                                return;
                            }

                            if (!BuildingManager.Instance.BuildingPartReferences.Contains(draggedBuildingPart))
                            {
                                Debug.LogError("<b>Easy Build System</b> : This Building Part does not exist in the Building Manager!");
                                return;
                            }

                            Target.ClearPreview();

                            BuildingSocket.SnappingPointSettings Offset = new BuildingSocket.SnappingPointSettings() { BuildingPart = draggedBuildingPart };

                            Target.SnappingPoints.Insert(Target.SnappingPoints.Count, Offset);
                            Target.SnappingPoints = Target.SnappingPoints.OrderBy(x => i).ToList();

                            m_CurrentOffset = Offset;

                            Target.ShowPreview(m_CurrentOffset);

                            SceneView.FrameLastActiveSceneView();

                            Repaint();

                            EditorUtility.SetDirty(target);
                        }

                        Event.current.Use();
                    }
                }

                EditorGUIUtilityExtension.EndVertical();

                if (GUILayout.Button("Create New Snapping Point..."))
                {
                    Target.SnappingPoints.Add(new BuildingSocket.SnappingPointSettings());
                }

                EditorGUILayout.Separator();
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