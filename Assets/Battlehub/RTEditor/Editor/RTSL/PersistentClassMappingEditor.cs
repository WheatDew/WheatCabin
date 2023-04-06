using Battlehub.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Battlehub.RTSL
{
    [CustomEditor(typeof(PersistentClassMapping))]
    public class PersistentClassMappingEditor : Editor
    {
        private ReorderableList list;

        private string m_pathName = Strong.MemberInfo((PersistentPropertyMapping p) => p.MappedName).Name;

        private void OnEnable()
        {
            list = new ReorderableList(serializedObject,
                    serializedObject.FindProperty("PropertyMappings"),
                    true, true, true, true);

            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Properties Order");
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);

                var path = element.FindPropertyRelative(m_pathName);
                if(path != null)
                {
                    EditorGUI.LabelField(rect, path.stringValue);
                }
                
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


