using UnityEngine;
using UnityEditor;

namespace Battlehub.Utils
{
    [CustomPropertyDrawer(typeof(FieldDisplayNameAttribute))]
    public class FieldDisplayNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, new GUIContent(((FieldDisplayNameAttribute)attribute).NewName));
        }
    }

}


