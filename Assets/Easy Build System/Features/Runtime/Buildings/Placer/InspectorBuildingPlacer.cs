/// <summary>
/// Project : Easy Build System
/// Class : InspectorBuildingPlacer.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Placer
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

namespace EasyBuildSystem.Features.Runtime.Buildings.Placer
{
    [ExecuteInEditMode]
    public class InspectorBuildingPlacer : BuildingPlacer
    {
        #region Unity Methods

#if UNITY_EDITOR
        void OnEnable()
        {
            SceneView.duringSceneGui += OnScene;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnScene;
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                SceneView.duringSceneGui -= OnScene;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

                DestroyImmediate(gameObject);
            }
        }

        void OnScene(SceneView sceneview)
        {
            if (GetBuildMode != BuildingPlacer.BuildMode.NONE)
            {
                if (Event.current.type == EventType.Layout)
                {
                    HandleUtility.AddDefaultControl(0);
                }
            }
            
            base.Update();

            if (Selection.activeObject != gameObject)
            {
                ChangeBuildMode(BuildMode.NONE);
                DestroyImmediate(gameObject);
                return;
            }

            if (Event.current != null)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    if (GetBuildMode == BuildMode.PLACE)
                    {
                        PlacingBuildingPart();
                    }
                    else if (GetBuildMode == BuildMode.DESTROY)
                    {
                        DestroyBuildingPart();
                    }
                    else if (GetBuildMode == BuildMode.EDIT)
                    {
                        EditingBuildingPart();
                    }
                }

                if (Event.current.keyCode == KeyCode.R && Event.current.type == EventType.KeyUp)
                {
                    RotatePreview();
                }
            }
        }
#endif

        #endregion
    }
}