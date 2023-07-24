/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors.StateSystem
{
    using Opsive.Shared.Editor.Inspectors;
    using Opsive.Shared.StateSystem;
    using Opsive.Shared.Utility;
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Contains all of the editor logic for presets. A preset is a set of presaved values that can be applied at runtime.
    /// </summary>
    [CustomEditor(typeof(PersistablePreset))]
    public class PersistablePresetInspector : InspectorBase
    {
        private MemberVisibility m_Visiblity = MemberVisibility.Public;
        private string[] m_AvailablePropertyNames;
        private List<PropertyInfo> m_AvailableProperies = new List<PropertyInfo>();

        /// <summary>
        /// Initializes the available property array.
        /// </summary>
        private void OnEnable()
        {
            // If the preset is a child of a StateConfiguration asset then the AllPublic MemberVisiblity type should be shown.
            if (!AssetDatabase.IsMainAsset(target)) {
                var mainAsset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(target), TypeUtility.GetType("Opsive.UltimateCharacterController.StateSystem.StateConfiguration"));
                if (mainAsset != null) {
                    m_Visiblity = MemberVisibility.AllPublic;
                }
            }

            InitializeAvailablePropertyArray();
        }

        /// <summary>
        /// Initializes the avialable property array. Can be called at any point if the array may have changed.
        /// </summary>
        private void InitializeAvailablePropertyArray()
        {
            m_AvailableProperies.Clear();
            var availablePropertyNames = new List<string>();
            // The properties name list should always show "Add Property..." first.
            availablePropertyNames.Add("Add Property...");

            // Get a list of all available property types on the current preset.
            var preset = target as PersistablePreset;
            if (preset == null || preset.Data == null) {
                return;
            }
            var objType = TypeUtility.GetType(preset.Data.ObjectType);
            if (objType != null) {
                Dictionary<long, int> valuePositionMap;
                if (preset.Data.LongValueHashes != null && preset.Data.LongValueHashes.Length > 0) {
                    valuePositionMap = new Dictionary<long, int>(preset.Data.LongValueHashes.Length);
                    for (int i = 0; i < preset.Data.LongValueHashes.Length; ++i) {
                        valuePositionMap.Add(preset.Data.LongValueHashes[i], i);
                    }
                } else if (preset.Data.ValueHashes != null) {
                    valuePositionMap = new Dictionary<long, int>(preset.Data.ValueHashes.Length);
                    for (int i = 0; i < preset.Data.ValueHashes.Length; ++i) {
                        valuePositionMap.Add(preset.Data.ValueHashes[i], i);
                    }
                } else {
                    valuePositionMap = new Dictionary<long, int>();
                }
                
                var properties = Serialization.GetSerializedProperties(objType, m_Visiblity);
                // Add to the available property names list based on the property types that have not already been added.
                var hashType = Serialization.GetHashType(preset.Data.Version);
                for (int i = 0; i < properties.Length; ++i) {
                    long hash;
                    if (hashType == Serialization.HashType.BitwiseLong) {
                        hash = Serialization.StringHash(properties[i].PropertyType.FullName) + Serialization.StringHash(properties[i].Name);
                    } else {
                        hash = Serialization.StringIntHash(properties[i].PropertyType.FullName) + Serialization.StringIntHash(properties[i].Name);
                    }
                    // The property is not currently being serialized.
                    if (!valuePositionMap.ContainsKey(hash)) {
                        // The property may not be valid.
                        if (Serialization.GetValidGetMethod(properties[i], m_Visiblity) == null) {
                            continue;
                        }

                        // The property is valid. Add it to the list.
                        availablePropertyNames.Add(ObjectNames.NicifyVariableName(properties[i].Name));
                        m_AvailableProperies.Add(properties[i]);
                    }
                }
            }

            // Save the list to an array.
            m_AvailablePropertyNames = availablePropertyNames.ToArray();
        }

        /// <summary>
        /// Draws the preset values within the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Show all of the fields.
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var preset = target as PersistablePreset;
            if (preset == null || preset.Data == null) {
                return;
            }
            var fullName = preset.Data.ObjectType;
            var splitName = fullName.Split('.');
            GUILayout.Label(splitName[splitName.Length - 1] + " Preset", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Show the property values within a table.
            var objType = TypeUtility.GetType(preset.Data.ObjectType);
            if (objType != null) {
                // Populate the position map so ObjectInspector.DrawProperties to know which properties to draw.
                Dictionary<long, int> valuePositionMap;
                if (preset.Data.LongValueHashes != null && preset.Data.LongValueHashes.Length > 0) {
                    valuePositionMap = new Dictionary<long, int>(preset.Data.LongValueHashes.Length);
                    for (int i = 0; i < preset.Data.LongValueHashes.Length; ++i) {
                        valuePositionMap.Add(preset.Data.LongValueHashes[i], i);
                    }
                } else if (preset.Data.ValueHashes != null){
                    valuePositionMap = new Dictionary<long, int>(preset.Data.ValueHashes.Length);
                    for (int i = 0; i < preset.Data.ValueHashes.Length; ++i) {
                        valuePositionMap.Add(preset.Data.ValueHashes[i], i);
                    }
                } else {
                    valuePositionMap = new Dictionary<long, int>();
                }

                // Draw all of the serialized properties. Implement the start and end callbacks so the delete button can be drawn next to a foldout in the case of a list, class, or struct.
                Utility.ObjectInspector.DrawProperties(objType, null, 0, valuePositionMap, preset.Data, m_Visiblity, () => { GUILayout.BeginHorizontal(); }, (index, unityObjectIndexes) =>
                {
                    Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(preset, "Change Value");
                    var removed = false;
                    if (GUILayout.Button(Utility.InspectorStyles.DeleteIcon, Utility.InspectorStyles.NoPaddingButtonStyle, GUILayout.Width(16))) {
                        RemoveElement(index, unityObjectIndexes);
                        removed = true;
                    }
                    serializedObject.ApplyModifiedProperties();
                    GUILayout.EndHorizontal();
                    return removed;
                });
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = m_AvailableProperies.Count > 0 && !Application.isPlaying; // Only allow the popup if properties can be selected.
            var selectedPropertyIndex = EditorGUILayout.Popup(0, m_AvailablePropertyNames, GUILayout.MaxWidth(150));
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            // If the selected property index isn't 0 then a property should be added.
            if (selectedPropertyIndex != 0) {
                var property = m_AvailableProperies[selectedPropertyIndex - 1];
                if (property != null) {
                    // Add the new property to the serialization.
                    object value = null;
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(property.PropertyType)) {
                        // Lists require special handling.
                        if (typeof(IList).IsAssignableFrom(property.PropertyType)) {
                            if (property.PropertyType.IsArray) {
                                var elementType = property.PropertyType.GetElementType();
                                value = Array.CreateInstance(elementType, 0);
                            } else {
                                var baseType = property.PropertyType;
                                while (!baseType.IsGenericType) {
                                    baseType = baseType.BaseType;
                                }
                                var elementType = baseType.GetGenericArguments()[0];
                                if (property.PropertyType.IsGenericType) {
                                    value = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                                } else {
                                    value = Activator.CreateInstance(property.PropertyType) as IList;
                                }
                            }
                        } else {
                            var getMethod = property.GetGetMethod();
                            if (getMethod != null) {
                                // A new GameObject must be created so the component can be added to it. MonoBehaviours cannot use Activator.CreateInstance.
                                GameObject gameObject = null;
                                object obj;
                                var objectType = TypeUtility.GetType(preset.Data.ObjectType);
                                if (typeof(MonoBehaviour).IsAssignableFrom(objectType)) {
                                    gameObject = new GameObject();
                                    obj = gameObject.AddComponent(objectType);
                                } else {
                                    obj = Activator.CreateInstance(objectType);
                                }
                                value = getMethod.Invoke(obj, null);
                                if (value == null) {
                                    if (getMethod.ReturnType == typeof(string)) {
                                        value = string.Empty;
                                    } else {
                                        value = Activator.CreateInstance(getMethod.ReturnType);
                                    }
                                }
                                if (gameObject != null) {
                                    DestroyImmediate(gameObject);
                                }
                            }
                        }
                    }
                    Serialization.AddProperty(property, value, null, preset.Data, m_Visiblity);
                    InitializeAvailablePropertyArray();
                }
            }

            if (EditorGUI.EndChangeCheck()) {
                Shared.Editor.Utility.EditorUtility.RecordUndoDirtyObject(preset, "Change Value");
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Removes the property element at the specified index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        /// <param name="unityObjectIndexes">The list of indexes that correspond to the Unity objects which are being removed.</param>
        private void RemoveElement(int index, List<int> unityObjectIndexes)
        {
            Serialization.RemoveProperty(index, unityObjectIndexes, (target as PersistablePreset).Data, m_Visiblity, Serialization.GetHashType((target as PersistablePreset).Data.Version));
            InitializeAvailablePropertyArray();
        }
    }
}