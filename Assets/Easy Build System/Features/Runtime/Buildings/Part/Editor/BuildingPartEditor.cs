/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;
using UnityEditorInternal;

using EasyBuildSystem.Features.Runtime.Buildings.Manager;
using EasyBuildSystem.Features.Runtime.Buildings.Socket;
using EasyBuildSystem.Features.Runtime.Buildings.Part.Conditions;

using EasyBuildSystem.Features.Runtime.Buildings.Manager.Editor;

using EasyBuildSystem.Features.Editor.Extensions;


namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingPart), true)]
    public class BuildingPartEditor : UnityEditor.Editor
    {
        #region Fields

        BuildingPart Target
        {
            get
            {
                return ((BuildingPart)target);
            }
        }

        static bool m_GeneralFoldout = true;
        static bool m_ModelFoldout;
        static bool m_PreviewFoldout;
        static bool m_ConditionFoldout;

        UnityEditor.Editor m_ModelEditor = null;

        static readonly bool[] m_ConditionsFoldout = new bool[999];

        List<BuildingConditionAttribute> m_BuildingConditions = new List<BuildingConditionAttribute>();

        Dictionary<int, UnityEditor.Editor> m_CachedEditors = new Dictionary<int, UnityEditor.Editor>();

        #endregion

        #region Unity Methods

        void OnEnable()
        {
            m_BuildingConditions = LoadBuildingConditions();

            for (int i = 0; i < m_BuildingConditions.Count; i++)
            {
                if (Target.GetComponent(m_BuildingConditions[i].Type) != null)
                {
                    Target.GetComponent(m_BuildingConditions[i].Type).hideFlags = HideFlags.HideInInspector;
                }
            }
        }

        void OnDisable()
        {
            Target.HidePreviewIndicator();

            if (m_ModelEditor != null)
            {
                DestroyImmediate(m_ModelEditor);
            }

            m_CachedEditors.Clear();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Building Part",
                "Contains all data concerning the building, such as the model, preview, and conditions.\n" +
                "You can find more information on the Building Part component in the documentation.");

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("None of changes on this component during the runtime will be saved.\n" +
                    "You can apply changes by clicking below. This will apply the change to all buildings of the same identifier.", MessageType.Warning);

                if (GUILayout.Button("Apply All Changes..."))
                {
                    BuildingPart buildingPart = BuildingManager.Instance.GetBuildingPartByIdentifier(Target.GetGeneralSettings.Identifier);

                    if (buildingPart != null)
                    {
                        EditorUtility.CopySerialized(Target, buildingPart);

                        for (int i = 0; i < Target.Conditions.Count; i++)
                        {
                            if (Target.Conditions[i] != null)
                            {
                                EditorUtility.CopySerialized(Target.Conditions[i], buildingPart.GetComponents<BuildingCondition>()[i]);
                            }
                        }

                        Debug.Log("<b>Easy Build System</b> : All changes on the Building Part : " + buildingPart.GetGeneralSettings.Name + " have applied.");
                    }
                }
            }

            if (HasMissingColliders())
            {
                if (Target.GetModelSettings.Models != null)
                {
                    EditorGUILayout.HelpBox("No collider has been found in the children's transforms.", MessageType.Warning);

                    if (GUILayout.Button("Add MeshCollider..."))
                    {
                        foreach (Renderer renderer in Target.gameObject.GetComponentsInChildren<Renderer>())
                        {
                            renderer.gameObject.AddComponent<MeshCollider>();

                            Target.Colliders = null;
                            _ = Target.Colliders;

                            Debug.Log("<b>Easy Build System</b> : The missing colliders have been added to the renderers.");
                        }
                    }
                }
            }

            m_GeneralFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("General Settings"), m_GeneralFoldout);

            if (m_GeneralFoldout)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField(new GUIContent("Building Identifier",
                    "Generated UID that allows the system to easily find the Building Part."), Target.GetGeneralSettings.Identifier);
                if (GUILayout.Button("Generate ID", GUILayout.Width(90)))
                {
                    Target.GetGeneralSettings.Identifier = Guid.NewGuid().ToString("N");
                    EditorUtility.SetDirty(target);
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GeneralSettings").FindPropertyRelative("m_Name"),
                    new GUIContent("Building Name", "Building name."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GeneralSettings").FindPropertyRelative("m_Type"),
                    new GUIContent("Building Type", "Building type."));

                if (GUILayout.Button("Building Type Editor..."))
                {
                    BuildingTypeEditor.Init();
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GeneralSettings").FindPropertyRelative("m_Thumbnail"),
                    new GUIContent("Building Thumbnail", "Building thumbnail."));

                if (GUILayout.Button("Generate Building Thumbnail..."))
                {
                    EditorApplication.delayCall += () =>
                    {
                        GenerateThumbnail();
                    };
                }
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_ModelFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Model Settings"), m_ModelFoldout);

            if (m_ModelFoldout)
            {
                EditorGUILayout.Separator();

                if (Target.GetModelSettings.GetModel != null)
                {
                    if (m_ModelEditor == null)
                    {
                        m_ModelEditor = CreateEditor(Target.GetModelSettings.GetModel);
                    }

                    EditorGUILayout.Separator();

                    m_ModelEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(128f, 128f), EditorStyles.whiteLabel);

                    EditorGUILayout.Separator();
                }

                for (int i = 0; i < Target.GetModelSettings.Models.Count; i++)
                {
                    if (Target.GetModelSettings.Models[i] != null)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Space(1f);

                        GUILayout.Label(Target.GetModelSettings.Models[i].name);

                        GUILayout.FlexibleSpace();

                        GUI.enabled = Target.GetModelSettings.ModelIndex != i;

                        if (GUILayout.Button("Set Model As Default...", GUILayout.Width(200)))
                        {
                            Target.GetModelSettings.ModelIndex = i;

                            Target.OnValidate();

                            DestroyImmediate(m_ModelEditor);

                            EditorUtility.SetDirty(target);
                        }

                        GUI.enabled = true;

                        GUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ModelSettings").FindPropertyRelative("m_ModelBounds"),
                    new GUIContent("Building Model Bounds", "Model bounds."));

                GUI.enabled = Target.GetModelSettings.Models != null;
                if (GUILayout.Button("Generate Building Model Bounds..."))
                {
                    Target.UpdateModelBounds();
                    EditorUtility.SetDirty(target);
                }

                GUI.enabled = Target.transform != null;
                if (GUILayout.Button("Edit Building Model Offset..."))
                {
                    ModelOffsetEditor.Init(Target.transform, Target);
                }
                GUI.enabled = true;

                GUILayout.Space(5f);

                EditorGUIUtilityExtension.BeginVertical();

                GUI.enabled = Target.gameObject.scene.IsValid();

                Rect dropRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));

                GUI.Box(dropRect, "Drag & Drop your 3D model here to add it...", EditorStyles.centeredGreyMiniLabel);

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

                            if (draggedObject == null)
                            {
                                Debug.LogError("<b>Easy Build System</b> : Cannot add empty object!");
                                return;
                            }

                            ModelChanger.ChangeModel(Target, Target.GetModelSettings.GetModel, draggedObject);

                            DestroyImmediate(m_ModelEditor, true);

                            Target.UpdateModelBounds();
                        }

                        Event.current.Use();
                    }
                }

                GUI.enabled = true;

                EditorGUIUtilityExtension.EndVertical();
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_PreviewFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Preview Settings"), m_PreviewFoldout);

            if (m_PreviewFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Indicator"),
                    new GUIContent("Preview Indicator", "Shows an indicator during the preview state, which can be useful for visualizing the rotation preview."));

                if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Indicator").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_IndicatorSettings"),
                        new GUIContent("Preview Indicator Settings"), true);

                    if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_IndicatorSettings").isExpanded)
                    {
                        GUI.enabled = Target.gameObject.scene.IsValid();

                        if (Target.InstancedIndicatorObject == null)
                        {
                            GUI.color = Color.white;
                            if (GUILayout.Button("Show Preview Indicator"))
                            {
                                Target.ShowPreviewIndicator();
                            }
                            GUI.color = Color.white;
                        }
                        else
                        {
                            GUI.color = Color.yellow;
                            if (GUILayout.Button("Cancel Preview Indicator"))
                            {
                                Target.HidePreviewIndicator();
                            }
                            GUI.color = Color.white;
                        }

                        GUI.enabled = true;
                    }
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_RotateAxis"),
                    new GUIContent("Preview Rotate Axis", "Rotation axis on which the preview can be rotated."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_RotateAccordingAngle"),
                    new GUIContent("Preview Rotate According Angle", "Rotate preview according to the surface angle."));

                if (serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_RotateAccordingAngle").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_RotateAccordingAxis"),
                        new GUIContent("Preview Rotate According Axis", "Rotate preview according to the surface axis."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_CanRotateOnSocket"),
                    new GUIContent("Preview Can Rotate On Socket", "Can rotate the preview when snapped on a Building Socket."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_CanMovingIfPlaceable"),
                    new GUIContent("Preview Can Move If Placeable", "Move the preview onbly when placeable."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_OffsetPosition"),
                    new GUIContent("Preview Offset Position", "Preview offset position."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampPosition"),
                    new GUIContent("Preview Clamp Position", "Clamp the position of the preview."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampMinPosition"),
                    new GUIContent("Preview Clamp Min Position", "Clamp min position of the preview."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampMaxPosition"),
                    new GUIContent("Preview Clamp Max Position", "Clamp max position of the preview."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampRotation"),
                    new GUIContent("Preview Clamp Rotation", "Clamp the rotation of the preview."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampMinRotation"),
                    new GUIContent("Preview Clamp Min Rotation", "Clamp min rotation of the preview."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_ClampMaxRotation"),
                    new GUIContent("Preview Clamp Max Rotation", "Clamp max rotation of the preview."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_Material"),
                    new GUIContent("Preview Material", "Preview material."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_PlacingColor"),
                    new GUIContent("Preview Placing Color", "Placing material color of the preview."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_EditingColor"),
                    new GUIContent("Preview Editing Color", "Editing material color of the preview."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_DestroyingColor"),
                    new GUIContent("Preview Destroying Color", "Destroying material color of the preview."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_IgnoreRenderers"),
                        new GUIContent("Preview Ignore Renderers", "Prevents the preview material from being applied to specific renderers."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_DisableGameObjects"),
                        new GUIContent("Preview Disable GameObjects", "Disable specific GameObjects during the preview state."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PreviewSettings").FindPropertyRelative("m_DisableMonoBehaviours"),
                        new GUIContent("Preview Disable MonoBehaviours", "Disable specific MonoBehaviours during the preview state."), true);
            }

            EditorGUIUtilityExtension.EndFoldout();

            m_ConditionFoldout = EditorGUIUtilityExtension.BeginFoldout(new GUIContent("Conditions Settings"), m_ConditionFoldout, false);

            if (m_ConditionFoldout)
            {
                GUILayout.Space(3f);

                if (m_BuildingConditions.Count == 0)
                {
                    EditorGUIUtilityExtension.BeginVertical();
                    GUILayout.Space(5f);
                    GUILayout.Label("No conditions was found for this component.", EditorStyles.centeredGreyMiniLabel);
                    GUILayout.Space(5f);
                    EditorGUIUtilityExtension.EndVertical();
                }

                int index = 0;

                foreach (BuildingConditionAttribute condition in m_BuildingConditions)
                {
                    m_ConditionsFoldout[index] = EditorGUIUtilityExtension.BeginFoldout(new GUIContent(condition.Name), m_ConditionsFoldout[index], false);

                    GUILayout.Space(-21);

                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();

                    if (Target.GetComponent(condition.Type) != null)
                    {
                        GUILayout.BeginVertical();

                        GUILayout.Space(3f);

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("Copy Settings", GUILayout.Width(120)))
                        {
                            ComponentUtility.CopyComponent(Target.GetComponent(condition.Type));
                        }

                        if (GUILayout.Button("Paste Settings", GUILayout.Width(120)))
                        {
                            ComponentUtility.PasteComponentValues(Target.GetComponent(condition.Type));
                            EditorUtility.SetDirty(target);
                        }

                        if (!condition.Type.Equals(typeof(BuildingBasicsCondition)))
                        {
                            if (GUILayout.Button("Disable Condition", GUILayout.Width(120)))
                            {
                                try
                                {
                                    DestroyImmediate(Target.gameObject.GetComponent(condition.Type), true);
                                    break;
                                }
                                catch { }
                            }
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();
                    }
                    else
                    {
                        GUILayout.BeginVertical();

                        GUILayout.Space(3f);

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("Enable Condition", GUILayout.Width(120)))
                        {
                            if (Target.GetComponent(condition.Type) != null)
                            {
                                return;
                            }

                            Component component = Target.gameObject.AddComponent(condition.Type);
                            component.hideFlags = HideFlags.HideInInspector;
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();
                    }

                    GUILayout.EndHorizontal();

                    if (m_ConditionsFoldout[index])
                    {
                        GUILayout.BeginHorizontal();

                        if (Target.GetComponent(condition.Type) != null)
                        {
                            GUILayout.BeginVertical();

                            if (Selection.gameObjects.Length > 1)
                            {
                                EditorGUILayout.HelpBox("Multiple-editing not yet supported.", MessageType.Warning);
                            }
                            else
                            {
                                EditorGUILayout.Space();
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(15f);
                                GUI.enabled = false;
                                GUILayout.Label(condition.Description, EditorStyles.miniLabel);
                                GUI.enabled = true;
                                GUILayout.EndHorizontal();
                                EditorGUILayout.Space();

                                Component component = Target.GetComponent(condition.Type);

                                if (m_CachedEditors.ContainsKey(component.GetInstanceID()))
                                {
                                    m_CachedEditors.TryGetValue(component.GetInstanceID(), out UnityEditor.Editor editor);

                                    EditorGUI.indentLevel++;
                                    editor.OnInspectorGUI();
                                    EditorGUI.indentLevel--;
                                }
                                else
                                {
                                    UnityEditor.Editor conditionEditor = CreateEditor(Target.GetComponent(condition.Type));
                                    m_CachedEditors.Add(component.GetInstanceID(), conditionEditor);

                                    EditorGUI.indentLevel++;
                                    conditionEditor.OnInspectorGUI();
                                    EditorGUI.indentLevel--;
                                    //Repaint();
                                    //return;
                                }
                            }

                            GUILayout.EndVertical();
                        }

                        GUILayout.EndHorizontal();
                    }

                    EditorGUIUtilityExtension.EndFoldout(false);

                    index++;
                }
            }

            EditorGUIUtilityExtension.EndFoldout(false);

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

        #region Internal Methods

        void GenerateThumbnail()
        {
            try
            {
                string path = EditorUtility.SaveFilePanelInProject(
                           "Save As Thumbnail...",
                           "New Thumbnail.png",
                           "png",
                           "");

                if (path.Length != 0)
                {
                    Texture2D thumbnailTexture = AssetPreview.GetAssetPreview(Target.gameObject);
                    File.WriteAllBytes(path, thumbnailTexture.EncodeToPNG());

                    AssetDatabase.Refresh();

                    Target.GetGeneralSettings.Thumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
            }
            catch
            { }
        }

        bool HasMissingColliders()
        {
            List<Transform> missingColliders = Target.GetComponentsInParent<Transform>(true).ToList();

            missingColliders.AddRange(Target.GetComponentsInChildren<Transform>(true));

            for (int i = 0; i < missingColliders.Count; i++)
            {
                if (missingColliders[i].GetComponent<BuildingSocket>() == null)
                {
                    if (missingColliders[i].GetComponent<Collider>() != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        List<BuildingConditionAttribute> LoadBuildingConditions()
        {
            List<BuildingConditionAttribute> conditions = new List<BuildingConditionAttribute>();

            Type[] activeBehaviours = GetAllSubTypes(typeof(MonoBehaviour));

            foreach (Type type in activeBehaviours)
            {
                object[] attributes = type.GetCustomAttributes(typeof(BuildingConditionAttribute), false);

                if (attributes != null)
                {
                    for (int i = 0; i < attributes.Length; i++)
                    {
                        if ((BuildingConditionAttribute)attributes[i] != null)
                        {
                            ((BuildingConditionAttribute)attributes[i]).Type = type;
                            conditions.Add((BuildingConditionAttribute)attributes[i]);
                        }
                    }
                }
            }

            conditions = conditions.OrderBy(x => x.Order).ToList();

            return conditions;
        }

        Type[] GetAllSubTypes(Type baseType)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type T in types)
                {
                    if (T.IsSubclassOf(baseType))
                    {
                        result.Add(T);
                    }
                }
            }

            return result.ToArray();
        }

        #endregion
    }

    public class ModelChanger
    {
        #region Internal Methods

        public static void ChangeModel(BuildingPart target, GameObject lastReference, GameObject newReference)
        {
            if (newReference != null)
            {
                bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(target.gameObject);

                string prefabAsset = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target.gameObject);

                if (isPrefabInstance)
                {
                    PrefabUtility.UnpackPrefabInstance(target.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                bool inChildren = target.transform.Find(newReference.name) != null;
                GameObject instancedObject;

                if (inChildren)
                {
                    instancedObject = newReference;
                }
                else
                {
                    instancedObject = MonoBehaviour.Instantiate(newReference, target.transform);
                }

                instancedObject.name = instancedObject.name.Replace("(Clone)", "");

                target.GetModelSettings.Models.Add(instancedObject);
                target.GetModelSettings.ModelIndex = target.GetModelSettings.Models.Count - 1;
                target.OnValidate();

                if (lastReference != null)
                {
                    instancedObject.transform.localPosition = lastReference.transform.localPosition;
                }
                else
                {
                    instancedObject.transform.localPosition = Vector3.zero;
                    instancedObject.transform.localEulerAngles = Vector3.zero;
                }

                target.Colliders = null;
                _ = target.Colliders;

                if (isPrefabInstance)
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(target.gameObject, prefabAsset, InteractionMode.UserAction);
                }

                Selection.activeObject = target.transform;
                SceneView.FrameLastActiveSceneView();
            }
        }

        #endregion
    }

    public class ModelOffsetEditor : EditorWindow
    {
        #region Fields

        static Rect m_WindowRect = new Rect(10, 30, 400, 200);

        static Transform m_TransformParent;

        static Transform m_SelectedTransform;
        static Transform GetSelectedTransform
        {
            get
            {
                if (Selection.activeGameObject != null)
                {
                    Transform selectedTransform = Selection.activeGameObject.transform;

                    if (selectedTransform != m_TransformParent)
                    {
                        if (m_TransformParent.Find(selectedTransform.name) != null)
                        {
                            m_SelectedTransform = selectedTransform;
                        }
                        else
                        {
                            m_SelectedTransform = null;
                        }
                    }
                    else
                    {
                        m_SelectedTransform = null;
                    }
                }
                else
                {
                    m_SelectedTransform = null;
                }

                return m_SelectedTransform;
            }
        }

        static BuildingPart m_BuildingPart;

        static Vector3 m_LastPosition;
        static Vector3 m_LastRotation;
        static Vector3 m_LastScale;

        static Vector3 m_DefaultPosition;
        static Vector3 m_DefaultRotation;
        static Vector3 m_DefaultScale;

        #endregion

        #region Unity Methods

        public static void Init(Transform transformParent, BuildingPart buildingPart)
        {
            m_BuildingPart = buildingPart;

            SceneView.duringSceneGui += OnScene;

            m_TransformParent = transformParent;

            if (buildingPart.GetModelSettings.Models != null)
            {
                Selection.activeGameObject = buildingPart.GetModelSettings.GetModel;
            }

            m_DefaultPosition = GetSelectedTransform.localPosition;
            m_DefaultRotation = GetSelectedTransform.localEulerAngles;
            m_DefaultScale = GetSelectedTransform.localScale;
        }

        static void OnScene(SceneView sceneview)
        {
            if (Selection.activeGameObject == null)
            {
                SceneView.duringSceneGui -= OnScene;
            }

            if (GetSelectedTransform != null)
            {
                if (m_LastPosition != m_SelectedTransform.localPosition || m_LastRotation != m_SelectedTransform.localEulerAngles ||
                    m_LastScale != m_SelectedTransform.localScale)
                {
                    m_LastPosition = m_SelectedTransform.localPosition;
                    m_LastRotation = m_SelectedTransform.localEulerAngles;
                    m_LastScale = m_SelectedTransform.localScale;
                }
            }

            Handles.BeginGUI();

            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            GUI.backgroundColor = new Color(1, 1, 1, 0f);
            m_WindowRect = GUILayout.Window(controlId, m_WindowRect, WindowContent, "");
            GUI.backgroundColor = new Color(1, 1, 1, 1f);

            Handles.EndGUI();
        }

        static void WindowContent(int id)
        {
            GUILayout.Space(-20);

            GUILayout.BeginVertical("window");

            GUILayout.Space(-20);

            EditorGUIUtilityExtension.DrawHeader("Easy Build System - Model Offset Editor",
                "You can edit here the offset positions of your model...");

            GUI.enabled = GetSelectedTransform != null;

            if (GetSelectedTransform != null)
            {
                GetSelectedTransform.localPosition = EditorGUILayout.Vector3Field("Offset Position", GetSelectedTransform.localPosition);
                GetSelectedTransform.localEulerAngles = EditorGUILayout.Vector3Field("Offset Rotation", GetSelectedTransform.localEulerAngles);
                GetSelectedTransform.localScale = EditorGUILayout.Vector3Field("Offset Scale", GetSelectedTransform.localScale);
            }
            else
            {
                GUILayout.Label("Select a transform child to edit offset...");
            }

            GUI.enabled = true;

            EditorGUILayout.Separator();

            if (GUILayout.Button("Save & Close"))
            {
                SceneView.duringSceneGui -= OnScene;
                Selection.activeObject = m_TransformParent;
                m_BuildingPart.UpdateModelBounds();
                SceneView.FrameLastActiveSceneView();

                EditorUtility.SetDirty(m_BuildingPart);
            }

            if (GUILayout.Button("Cancel"))
            {
                SceneView.duringSceneGui -= OnScene;

                if (GetSelectedTransform != null)
                {
                    GetSelectedTransform.localPosition = m_DefaultPosition;
                    GetSelectedTransform.localEulerAngles = m_DefaultRotation;
                    GetSelectedTransform.localScale = m_DefaultScale;
                }

                Selection.activeObject = m_TransformParent;

                SceneView.FrameLastActiveSceneView();
            }

            GUI.DragWindow();

            GUILayout.EndVertical();
        }

        #endregion
    }
}