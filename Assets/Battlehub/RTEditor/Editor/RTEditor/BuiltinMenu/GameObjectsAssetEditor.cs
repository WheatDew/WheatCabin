using Battlehub.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [CustomEditor(typeof(GameObjectsAsset))]
    public class GameObjectsAssetEditor : Editor
    {
        private ReorderableList list;

        private string m_pathName = Strong.MemberInfo((GameObjectsAsset.Prefab mi) => mi.MenuPath).Name;

        private void OnEnable()
        {
            list = new ReorderableList(serializedObject,
                    serializedObject.FindProperty("m_prefabs"),
                    true, true, true, true);

            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Items Order");
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);

                var path = element.FindPropertyRelative(m_pathName);
                EditorGUI.LabelField(rect, path.stringValue);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }

}


