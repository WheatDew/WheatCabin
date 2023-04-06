#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor
{
    public static class RTExtensionsMenu
    {
        [MenuItem("Tools/Runtime Editor/Create RTExtensions", priority = 1)]
        public static void CreateExtensions()
        {
            GameObject editorExtensions = GameObject.Find("RTExtensions");
            if(editorExtensions != null)
            {
                Selection.activeGameObject = editorExtensions;
                EditorGUIUtility.PingObject(editorExtensions);
            }
            else
            {
                editorExtensions = InstantiateEditorExtensions();
                editorExtensions.name = "RTExtensions";
                Undo.RegisterCreatedObjectUndo(editorExtensions, "Battlehub.RTExtensions.Create");
            }
        }

        public static GameObject InstantiateEditorExtensions()
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath(BHRoot.PackageRuntimeContentPath + @"/EditorExtensions.prefab", typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }
    }
}
#endif