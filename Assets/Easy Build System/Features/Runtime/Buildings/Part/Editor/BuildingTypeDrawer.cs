/// <summary>
/// Project : Easy Build System
/// Class : BuildingTypeDrawer.cs
/// Namespace : EasyBuildSystem.Features.Runtime.Buildings.Part.Editor
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using EasyBuildSystem.Features.Runtime.Buildings.Manager;

using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Runtime.Buildings.Part.Editor
{
    [CustomPropertyDrawer(typeof(BuildingTypeAttribute), true)]
    public class BuildingTypeDrawer : PropertyDrawer
    {
        #region Unity Methods

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "The attribute runs just on strings.", MessageType.Error);
            }

            if (BuildingType.Instance.AllBuildingTypes != null)
            {
                string[] allTypesToArray = BuildingType.Instance.AllBuildingTypes.ToArray();

                int selectedItem = IndexOfString(property.stringValue, allTypesToArray);
                selectedItem = EditorGUI.Popup(position, label.text, selectedItem, allTypesToArray);
                property.stringValue = StringAtIndex(selectedItem, allTypesToArray);
            }
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return false;
        }

        #endregion

        #region Internal Methods

        int IndexOfString(string str, string[] allStrings)
        {
            for (int i = 0; i < allStrings.Length; i++)
            {
                if (allStrings[i] == str)
                {
                    return i;
                }
            }

            return 0;
        }

        string StringAtIndex(int i, string[] allStrings)
        {
            return allStrings.Length > i ? allStrings[i] : "";
        }

        #endregion
    }
}