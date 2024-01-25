/// <summary>
/// Project : Easy Build System
/// Class : BuildingManagerEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Manager.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using UnityEditor;

using EasyBuildSystem.Features.Runtime.Buildings.Part;
using EasyBuildSystem.Features.Runtime.Buildings.Manager.Collection;

using EasyBuildSystem.Features.Editor.Extensions;
using EasyBuildSystem.Features.Editor;

namespace EasyBuildSystem.Features.Runtime.Buildings.Manager.Editor
{
    [CustomEditor(typeof(BuildingManager))]
    public class BuildingManagerEditor : UnityEditor.Editor
    {
        #region Fields

        bool m_GeneralFoldout = true;
        bool m_AreaOfInterestFoldout;
        bool m_BuildingBatchingFoldout;

        List<BuildingCollection> m_BuildingCollections = new List<BuildingCollection>();
        string[] m_Collections;
        int m_CollectionIndex;

        readonly List<UnityEditor.Editor> BuildingPartPreviews = new List<UnityEditor.Editor>();

        BuildingManager Target 
        { 
            get 
            { 
                return (BuildingManager)target; 
            } 
        }

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            m_BuildingCollections = FindAssetsByType<BuildingCollection>();

            m_Collections = new string[m_BuildingCollections.Count + 1];
            m_Collections[0] = "Select Building Collection...";
            for (int i = 0; i < m_BuildingCollections.Count; i++)
            {
                m_Collections[i + 1] = m_BuildingCollections[i].name;
            }
        }

        void OnDisable()
        {
            for (int i = 0; i < BuildingPartPreviews.Count; i++)
            {
                DestroyImmediate(BuildingPartPreviews[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Manager",
                "Contains Building Parts reference list as well as the optimization features.\n" +
                "You can find more information on the Building Manager component in the documentation.");

            m_GeneralFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("General Settings"), m_GeneralFoldout);

            if (m_GeneralFoldout)
            {
                EditorGUILayout.Separator();
                GUILayout.Label("Building Parts Settings", EditorStyles.boldLabel);
                GUILayout.Space(1);
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BuildingPartReferences"), 
                    new GUIContent("Building Parts", "List of Building Parts which will be used by the system."));
                GUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();

                m_CollectionIndex = EditorGUILayout.Popup("Building Collection", m_CollectionIndex, m_Collections);

                if (EditorGUI.EndChangeCheck())
                {
                    if (m_CollectionIndex - 1 != -1)
                    {
                        Undo.RecordObject(target, "Cancel Push Collection");

                        for (int i = 0; i < m_BuildingCollections[m_CollectionIndex - 1].BuildingParts.Count; i++)
                        {
                            BuildingPart buildingPart = m_BuildingCollections[m_CollectionIndex - 1].BuildingParts[i];

                            if (buildingPart != null)
                            {
                                if (Target.BuildingPartReferences.Find(x => x != null &&
                                    x.GetGeneralSettings.Identifier == buildingPart.GetGeneralSettings.Identifier) == null)
                                {
                                    Target.BuildingPartReferences.Add(buildingPart);
                                }
                                else
                                {
                                    Debug.LogWarning("<b>Easy Build System</b> : <b>" + buildingPart.GetGeneralSettings.Name + "</b> already exists!");
                                }
                            }
                        }

                        Debug.Log("<b>Easy Build System</b> : <b>" + m_BuildingCollections[m_CollectionIndex - 1].name + "</b> has been added!");
                        EditorUtility.SetDirty(target);
                        Repaint();
                        m_CollectionIndex = 0;
                    }
                }

                if (GUILayout.Button("Create Building Collection..."))
                {
                    MenuComponent.CreateBuildingCollection();
                }

                EditorGUILayout.Separator();
                
                GUILayout.Label("Building Types Settings", EditorStyles.boldLabel);

                if (GUILayout.Button("Building Type Editor..."))
                {
                    BuildingTypeEditor.Init();
                }

                EditorGUILayout.Separator();
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_AreaOfInterestFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Area Of Interest Settings"), m_AreaOfInterestFoldout);

            if (m_AreaOfInterestFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AreaOfInterestSettings").FindPropertyRelative("m_AreaOfInterest"),
                    new GUIContent("Use Area Of Interest",
                    "Allows disabling all Building Areas and Building Sockets that are far from the camera to prevent reaching the colliders limit in your scene."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AreaOfInterestSettings").FindPropertyRelative("m_AffectBuildingAreas"),
                    new GUIContent("Area Of Interest Affect Areas", "Affect Building Areas?"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AreaOfInterestSettings").FindPropertyRelative("m_AffectBuildingSockets"),
                    new GUIContent("Area Of Interest Affect Sockets", "Affect Building Building Sockets?"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AreaOfInterestSettings").FindPropertyRelative("m_RefreshInterval"),
                    new GUIContent("Area Of Interest Refresh Interval",
                    "How many times per second the update will be called. It is recommended to use a minimum value of 0.5f by default."));
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_BuildingBatchingFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Building Batching Settings"), m_BuildingBatchingFoldout);

            if (m_BuildingBatchingFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BuildingBatchingSettings").FindPropertyRelative("m_UseBuildingBatching"), 
                    new GUIContent("Use Building Batching",
                    "Allows to batch all Building Parts in the scene. Building Batching will combine all meshes that share the same Materials." +
                    " This can significantly improve performance at runtime, since less draw calls are required."));
            }

            EditorGUIUtilityExtension.EndFoldout();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        List<T> FindAssetsByType<T>() where T : Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        #endregion
    }
}