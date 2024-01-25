/// <summary>
/// Project : Easy Build System
/// Class : DontDrawIfAttributeEditor.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Bases.Drawers.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using System.IO;
using System.Collections;

using UnityEngine;
using UnityEditor;

namespace EasyBuildSystem.Features.Runtime.Bases.Drawers.Editor
{
    [CustomPropertyDrawer(typeof(DontDrawIfAttribute))]
    public class DontDrawIfAttributeEditor : PropertyDrawer
    {
        DontDrawIfAttribute m_DrawAttribute;
        SerializedProperty m_ComparedField;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!ShowProperty(property) && m_DrawAttribute.Disabling == DontDrawIfAttribute.DisablingType.DONT_DRAW)
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                if (property.propertyType == SerializedPropertyType.Generic)
                {
                    int childCount = 0;
                    float height = 0.0f;

                    IEnumerator children = property.GetEnumerator();

                    while (children.MoveNext())
                    {
                        SerializedProperty child = children.Current as SerializedProperty;

                        GUIContent childLabel = new GUIContent(child.displayName);

                        height += EditorGUI.GetPropertyHeight(child, childLabel) + EditorGUIUtility.standardVerticalSpacing;
                        childCount++;
                    }

                    height -= EditorGUIUtility.standardVerticalSpacing;

                    return height;
                }

                return EditorGUI.GetPropertyHeight(property, label);
            }
        }

        bool ShowProperty(SerializedProperty property)
        {
            m_DrawAttribute = attribute as DontDrawIfAttribute;

            string path = property.propertyPath.Contains(".") ?
                Path.ChangeExtension(property.propertyPath, m_DrawAttribute.ComparedPropertyName) : m_DrawAttribute.ComparedPropertyName;

            m_ComparedField = property.serializedObject.FindProperty(path);

            if (m_ComparedField == null)
            {
                return true;
            }

            switch (m_ComparedField.type)
            {
                case "bool":
                    return m_ComparedField.boolValue.Equals(m_DrawAttribute.ComparedValue);
                case "Enum":
                    return m_ComparedField.enumValueIndex.Equals((int)m_DrawAttribute.ComparedValue);
                default:
                    return false;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShowProperty(property))
            {
                if (property.propertyType == SerializedPropertyType.Generic)
                {
                    IEnumerator children = property.GetEnumerator();

                    Rect offsetPosition = position;

                    while (children.MoveNext())
                    {
                        SerializedProperty child = children.Current as SerializedProperty;

                        GUIContent childLabel = new GUIContent(child.displayName);

                        float childHeight = EditorGUI.GetPropertyHeight(child, childLabel);

                        offsetPosition.height = childHeight;

                        EditorGUI.PropertyField(offsetPosition, child, childLabel);

                        offsetPosition.y += childHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label);
                }
            }
            else if (m_DrawAttribute.Disabling == DontDrawIfAttribute.DisablingType.READ_ONLY)
            {
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label);
                GUI.enabled = true;
            }
        }
    }
}