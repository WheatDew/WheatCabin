using System;
using System.Reflection;
using UnityEditor;

namespace Battlehub.RTEditor.Examples
{
    public static class RuntimeEditorMenu
    {
        [MenuItem("Tools/Runtime Editor/Show me examples", priority = -1000)]
        public static void ShowMeExamples()
        {
            EditorUtility.FocusProjectWindow();

            const string path = "Assets/Battlehub/RTEditorDemo/Scenes/Scene1 - Minimal.unity";

            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);

          
        }
    }
}
