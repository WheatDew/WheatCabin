#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [CustomEditor(typeof(WindowManager))]
    public class WindowManagerEditor : Editor
    {
        private readonly string[] m_legacyProperties =
        {
            "m_Script",

            "m_sceneWindow",
            "m_gameWindow",
            "m_hierarchyWindow",
            "m_inspectorWindow",
            "m_projectWindow",
            "m_consoleWindow",
            "m_animationWindow",
            "m_saveSceneDialog",
            "m_saveAssetDialog",
            "m_openProjectDialog",
            "m_selectAssetLibraryDialog",
            "m_toolsWindow",
            "m_importAssetsDialog",
            "m_aboutDialog",
            "m_selectObjectDialog",
            "m_selectColorDialog",
            "m_selectAnimationPropertiesDialog",
            "m_saveFileDialog",
            "m_openFileDialog",
            "m_emptyDialog",
            "m_empty",
            "m_customWindows",
            
            "m_dialogManager",
            "m_dockPanels",
            "m_componentsRoot",
            "m_toolsRoot",
            "m_topBar",
            "m_bottomBar",
            "m_leftBar",
            "m_rightBar",
        };

        private SerializedProperty m_useLegacyBuiltInWindowsProperty;

        private void OnEnable()
        {
            m_useLegacyBuiltInWindowsProperty = serializedObject.FindProperty("m_useLegacyBuiltInWindows");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if(m_useLegacyBuiltInWindowsProperty.boolValue)
            {
                DrawDefaultInspector();
            }
            else
            {
                DrawPropertiesExcluding(serializedObject, m_legacyProperties);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

