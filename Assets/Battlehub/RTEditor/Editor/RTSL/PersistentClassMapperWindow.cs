using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using System.IO;
using Battlehub.RTSL.Internal;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using ProtoBuf.Meta;
using Battlehub.Utils;

namespace Battlehub.RTSL
{
    public class PersistentTemplateInfo
    {
        public string Usings;
        public string Interfaces;
        public string Body;
        public HashSet<string> FieldNames;
        public HashSet<Type> RequiredTypes;
        public string Path;
    }

    public class PersistentClassMapperGUI
    {
        private int m_uniqueId;
        public const string k_SessionStatePrefix = "PersistentClassMapperGUI";

        public static readonly HashSet<Type> HideMustHaveTypes = new HashSet<Type>
        {
            typeof(UnityObject),
            typeof(Component),
            typeof(Transform),
            typeof(GameObject),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Quaternion),
            typeof(Color),
            typeof(RuntimePrefab),
            typeof(RuntimeScene),
            typeof(UnityEventBase),
            typeof(UnityEvent),
         //   typeof(RuntimeSerializableObject),
        };

        public event Action<Type> TypeLocked;
        public event Action<Type> TypeUnlocked;

        private Type m_baseType;
        private Func<Type, string, bool> m_groupFilter;
        private string[] m_groupNames;
        private string m_groupLabel;
        private int m_selectedGroupIndex;
        private bool m_useGroupFilterText;
        private string m_groupFilterText = string.Empty;
        private string m_filterText = string.Empty;
        private string m_namespaceFilterText = string.Empty;

        private Vector2 m_scrollViewPosition;
        private Type[] m_types;
        private Dictionary<Type, int> m_typeToIndex;
        private string[] m_customImplementationOptions;

        private bool IsAllOn
        {
            get { return m_mappings.Count(m => m.IsOn) == m_mappings.Length; }
        }

        private bool IsAllOff
        {
            get { return m_mappings.Count(m => m.IsOn) == 0; }
        }


        [Serializable]
        public class ClassMapperGUIState
        {
            public ClassMappingInfoState[] State;
            public string FilterText;
            public int GroupIndex;
            public string GroupFilterText;
        }
        [Serializable]
        public class ClassMappingInfoState
        {
            public bool IsExpanded;
            public bool IsPlatformsExpanded;
            public bool[] IsParentExpanded;
            public string TypeName;
        }

        private int[] m_filteredTypeIndices;
        private class ClassMappingInfo
        {
            public string Version;

            public ObsoleteAttribute ObsoleteAttribute;
            public int PersistentPropertyTag;
            public int PersistentSubclassTag;

            public bool IsSelected
            {
                get;
                set;
            }

            public bool IsLocked
            {
                get;
                set;
            }

            public bool IsOn
            {
                get { return IsLocked || IsSelected; }
            }

            public bool IsExpanded;
            public bool[] IsParentExpanded;
            public int ExpandedCounter;
            public PersistentPropertyMapping[] PropertyMappings;
            public PersistentSubclass[] Subclasses;
            public bool[] IsPropertySelected;
            public int[] PropertyMappingSelection;

            public bool IsPropertyOn(int index)
            {
                return IsPropertySelected[index] && PropertyMappingSelection[index] >= 0;
            }

            public string[][] PropertyMappingNames; //per property
            public GUIContent[][] PropertyMappingsDisplayNames;
            public bool[][] PropertyIsObsolete;
            public Type[][] PropertyMappingTypes;
            public string[][] PropertyMappingTypeNames; //per property
            public string[][] PropertyMappingNamespaces;
            public string[][] PropertyMappingAssemblyNames;


            public bool IsSupportedPlaftormsSectionExpanded;
            public HashSet<RuntimePlatform> UnsupportedPlatforms;

            public bool HasCustomImplementation;
            public bool CreateCustomImplementation;
            public bool HasTemplate;
            public bool UseTemplate;
        }

        private Dictionary<Type, PersistentTemplateInfo> m_templates;
        private Dictionary<Type, int> m_dependencyTypes;
        private ClassMappingInfo[] m_mappings;
        private string m_mappingStoragePath;
        private string[] m_mappingTemplateStoragePath;

        private Dictionary<string, UnityObject> m_typeToScriptObject;
        private CodeGen m_codeGen;

        private GUIStyle m_deprecatedPopupStyle;
        private GUIStyle m_deprecatedFoldoutStyle;

        private PersistentClassMappingsStorage m_storage;
        public PersistentClassMappingsStorage Storage
        {
            get { return m_storage; }
        }

        public string EditorGUILayoutTextField(string header, string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(header, GUILayout.ExpandWidth(false), GUILayout.MinWidth(145));
            text = GUILayout.TextField(text, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            return text;
        }

        public PersistentClassMapperGUI(
            int uniqueId,
            CodeGen codeGen,
            string mappingStorage,
            string[] mappingTemplateStorage,
            FilePathStorage filePathStorage,
            Type baseType,
            Type[] types,
            Dictionary<Type, PersistentTemplateInfo> templates,
            string[] groupNames,
            string groupLabel,
            bool useGroupTextFilter,
            Func<Type, string, bool> groupFilter,
            bool userAction = true)
        {
            m_uniqueId = uniqueId;
            m_mappingStoragePath = mappingStorage;
            m_mappingTemplateStoragePath = mappingTemplateStorage;

            m_typeToScriptObject = new Dictionary<string, UnityObject>();
            if (filePathStorage != null)
            {
                FilePathRecord[] records = filePathStorage.PathRecords;
                if (records != null)
                {
                    for (int i = 0; i < records.Length; ++i)
                    {
                        FilePathRecord record = records[i];
                        if (!m_typeToScriptObject.ContainsKey(record.PeristentTypeName))
                        {
                            m_typeToScriptObject.Add(record.PeristentTypeName, record.File);
                        }
                    }
                }
            }

            m_codeGen = codeGen;
            m_baseType = baseType;
            m_types = types;
            m_groupNames = groupNames;
            m_groupLabel = groupLabel;
            m_useGroupFilterText = useGroupTextFilter;
            m_groupFilter = groupFilter;
            m_templates = templates;

            if (userAction)
            {
                m_deprecatedPopupStyle = new GUIStyle(EditorStyles.popup);
                m_deprecatedPopupStyle.normal.textColor = Color.red;
                m_deprecatedPopupStyle.focused.textColor = Color.red;
                m_deprecatedFoldoutStyle = new GUIStyle(EditorStyles.foldout);
                m_deprecatedFoldoutStyle.normal.textColor = Color.red;
                m_deprecatedFoldoutStyle.focused.textColor = Color.red;
            }
            else
            {
                m_deprecatedPopupStyle = new GUIStyle();
            }

            m_customImplementationOptions = new[] { "Create new", "Create from template" };
        }

        public bool InitializeAndLoadMappings()
        {
            if (m_mappings == null)
            {
                Initialize();
                LoadMappings();
                return true;
            }
            return false;
        }

        public void LockTypes()
        {
            for (int i = 0; i < m_mappings.Length; ++i)
            {
                ClassMappingInfo mappingInfo = m_mappings[i];
                if (mappingInfo.IsSelected)
                {
                    OnTypeLock(i);
                }
            }
        }

        public void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            if (m_useGroupFilterText)
            {
                m_groupFilterText = EditorGUILayoutTextField(m_groupLabel, m_groupFilterText);
            }
            else
            {
                m_selectedGroupIndex = EditorGUILayout.Popup(m_groupLabel, m_selectedGroupIndex, m_groupNames);
            }

            m_namespaceFilterText = EditorGUILayoutTextField("Namespace Filter:", m_namespaceFilterText);
            m_filterText = EditorGUILayoutTextField("Type Filter:", m_filterText);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyFilter();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            bool allowSelectAll = false;
            if (allowSelectAll)
            {
                if (IsAllOn)
                {
                    EditorGUILayout.Toggle(true, GUILayout.MaxWidth(20));
                    EditorGUILayout.LabelField("Select All Types", GUILayout.MaxWidth(230));
                }
                else if (IsAllOff)
                {
                    EditorGUILayout.Toggle(false, GUILayout.MaxWidth(20));
                    EditorGUILayout.LabelField("Select All Types", GUILayout.MaxWidth(230));

                }
                else
                {
                    EditorGUILayout.Toggle(false, "ToggleMixed", GUILayout.MaxWidth(20));
                    EditorGUILayout.LabelField("Select All Types", GUILayout.MaxWidth(230));
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (IsAllOn)
                    {
                        UnselectAll();
                    }
                    else
                    {
                        SelectAll();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Collapse All", GUILayout.Width(80)))
            {
                for (int i = 0; i < m_mappings.Length; ++i)
                {
                    m_mappings[i].IsExpanded = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            m_scrollViewPosition = EditorGUILayout.BeginScrollView(m_scrollViewPosition);
            EditorGUILayout.BeginVertical();
            {
                for (int i = 0; i < m_filteredTypeIndices.Length; ++i)
                {
                    int typeIndex = m_filteredTypeIndices[i];
                    DrawTypeEditor(typeIndex, typeIndex);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        public void OnEnable()
        {

        }

        public void OnDisable()
        {
            if (m_mappings == null)
            {
                return;
            }
            ClassMappingInfoState[] sessionState = new ClassMappingInfoState[m_mappings.Length];
            for (int i = 0; i < m_mappings.Length; ++i)
            {
                ClassMappingInfo mapping = m_mappings[i];
                if (mapping != null)
                {
                    ClassMappingInfoState state = new ClassMappingInfoState();
                    state.IsExpanded = mapping.IsExpanded;
                    state.IsPlatformsExpanded = mapping.IsSupportedPlaftormsSectionExpanded;
                    state.IsParentExpanded = mapping.IsParentExpanded;
                    state.TypeName = m_types[i].AssemblyQualifiedName;
                    sessionState[i] = state;
                }
            }

            string jsonState = JsonUtility.ToJson(
                new ClassMapperGUIState
                {
                    State = sessionState,
                    FilterText = m_filterText,
                    GroupIndex = m_selectedGroupIndex,
                    GroupFilterText = m_groupFilterText
                });

            SessionState.SetString(k_SessionStatePrefix + m_uniqueId, jsonState);
        }

        private void Initialize()
        {
            m_mappings = new ClassMappingInfo[m_types.Length];
            for (int i = 0; i < m_types.Length; ++i)
            {
                m_mappings[i] = new ClassMappingInfo();
                m_mappings[i].ObsoleteAttribute = m_types[i].GetCustomAttributes(false).OfType<ObsoleteAttribute>().FirstOrDefault();
                m_mappings[i].HasTemplate = m_templates.ContainsKey(m_types[i]);
                m_mappings[i].UseTemplate = true;

                Type persistentType = m_codeGen.GetPersistentType(PersistentClassMapping.ToPersistentFullName(CodeGen.Namespace(m_types[i]), m_types[i].Name));
                m_mappings[i].HasCustomImplementation = persistentType != null && persistentType.GetCustomAttributes(typeof(CustomImplementationAttribute), false).Length > 0;
            }

            m_dependencyTypes = new Dictionary<Type, int>();
            m_typeToIndex = new Dictionary<Type, int>();
            m_filteredTypeIndices = new int[m_types.Length];
            for (int i = 0; i < m_filteredTypeIndices.Length; ++i)
            {
                m_filteredTypeIndices[i] = i;
                m_typeToIndex.Add(m_types[i], i);
                m_mappings[i].IsParentExpanded = new bool[GetAncestorsCount(m_types[i])];
            }


            var jsonState = SessionState.GetString(k_SessionStatePrefix + m_uniqueId, "");
            if (!string.IsNullOrEmpty(jsonState))
            {
                ClassMapperGUIState guiState = JsonUtility.FromJson<ClassMapperGUIState>(jsonState);
                if (guiState.State != null)
                {
                    Dictionary<string, ClassMappingInfoState> typeToState = guiState.State.ToDictionary(s => s.TypeName);
                    for (int i = 0; i < m_types.Length; ++i)
                    {
                        string type = m_types[i].AssemblyQualifiedName;
                        ClassMappingInfoState state;
                        if (typeToState.TryGetValue(type, out state))
                        {
                            ClassMappingInfo mapping = m_mappings[i];
                            mapping.IsExpanded = state.IsExpanded;
                            ExpandType(i);
                            mapping.IsSupportedPlaftormsSectionExpanded = state.IsPlatformsExpanded;
                            mapping.IsParentExpanded = state.IsParentExpanded;
                        }
                    }
                }
                m_filterText = guiState.FilterText;
                m_selectedGroupIndex = guiState.GroupIndex;
                m_groupFilterText = guiState.GroupFilterText;
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            List<int> filteredTypeIndices = new List<int>();
            for (int i = 0; i < m_types.Length; ++i)
            {
                Type type = m_types[i];

                bool matchNs = string.IsNullOrEmpty(CodeGen.Namespace(type)) && string.IsNullOrEmpty(m_namespaceFilterText) || !string.IsNullOrEmpty(CodeGen.Namespace(type)) && CodeGen.Namespace(type).ToLower().Contains(m_namespaceFilterText.ToLower());

                bool groupFilterPassed = m_useGroupFilterText ?
                    (string.IsNullOrWhiteSpace(m_groupFilterText) || m_groupFilter(type, m_groupFilterText)) :
                    (m_selectedGroupIndex == 0 || m_selectedGroupIndex >= 0 && m_selectedGroupIndex < m_groupNames.Length && m_groupFilter(type, m_groupNames[m_selectedGroupIndex]));

                if (matchNs && CodeGen.TypeName(type).ToLower().Contains(m_filterText.ToLower()) && groupFilterPassed)
                {
                    filteredTypeIndices.Add(i);
                }
            }
            m_filteredTypeIndices = filteredTypeIndices.ToArray();
        }

        private void SelectAll()
        {
        }

        private void UnselectAll()
        {
        }

        private Type MappedType(Type mappedType)
        {
            if (mappedType.IsArray)
            {
                mappedType = mappedType.GetElementType();
            }
            else if (CodeGen.IsGenericList(mappedType) || CodeGen.IsHashSet(mappedType))
            {
                mappedType = mappedType.GetGenericArguments()[0];
            }
            return mappedType;
        }

        /// <summary>
        /// Returns true if locked
        /// </summary>
        /// <param name="typeIndex"></param>
        /// <returns></returns>
        private bool LockType(int typeIndex)
        {
            bool locked = false;
            Type mappedType = m_types[typeIndex];
            if (!m_dependencyTypes.ContainsKey(mappedType))
            {
                m_dependencyTypes.Add(mappedType, 0);
                m_mappings[typeIndex].IsLocked = true;
                ExpandType(typeIndex);
                locked = true;
            }
            m_dependencyTypes[mappedType]++;
            return locked;
        }

        /// <summary>
        /// Returns true if unlocked
        /// </summary>
        /// <param name="typeIndex"></param>
        /// <returns></returns>
        private bool UnlockType(int typeIndex)
        {
            bool unlocked = false;
            Type mappedType = m_types[typeIndex];
            if (m_dependencyTypes.ContainsKey(mappedType))
            {
                m_dependencyTypes[mappedType]--;
                if (m_dependencyTypes[mappedType] <= 0)
                {
                    m_mappings[typeIndex].IsLocked = false;
                    m_dependencyTypes.Remove(mappedType);
                    unlocked = true;
                }
            }
            return unlocked;
        }

        private void ToPropertyTypeIndex(ClassMappingInfo mappingInfo, int propIndex, int selection, Action<int> callback, Action<Type> missingCallback)
        {
            Type propertyType = mappingInfo.PropertyMappingTypes[propIndex][selection];
            if (CodeGen.IsDictionary(propertyType))
            {
                Type[] args = propertyType.GetGenericArguments();
                ToTypeIndex(callback, missingCallback, args[0]);
                ToTypeIndex(callback, missingCallback, args[1]);
            }
            else
            {
                propertyType = MappedType(propertyType);
                ToTypeIndex(callback, missingCallback, propertyType);
            }
        }

        private void ToTypeIndex(Action<int> callback, Action<Type> missingCallback, Type propertyType)
        {
            int propertyTypeIndex;
            if (m_typeToIndex.TryGetValue(propertyType, out propertyTypeIndex))
            {
                if (callback != null)
                {
                    callback(propertyTypeIndex);
                }
            }
            else
            {
                missingCallback(propertyType);
            }
        }

        private void ForEachEnabledProperty(int typeIndex, Action<int> callback, Action<Type> missingCallback)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (mappingInfo == null || mappingInfo.PropertyMappings == null)
            {
                Debug.LogWarning("mappingInfo == null || mappingInfo.PropertyMappings == null : " + (mappingInfo == null) + " " + (mappingInfo.PropertyMappings == null));
                return;
            }
            for (int propIndex = 0; propIndex < mappingInfo.PropertyMappings.Length; ++propIndex)
            {
                if (mappingInfo.IsPropertySelected[propIndex])
                {
                    int selection = mappingInfo.PropertyMappingSelection[propIndex];
                    if (selection > -1)
                    {
                        ToPropertyTypeIndex(mappingInfo, propIndex, selection, callback, missingCallback);
                    }
                }
            }
        }

        private void ForEachRequiredType(int typeIndex, Action<int> callback, Action<Type> missingCallback)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (mappingInfo.HasTemplate)
            {
                PersistentTemplateInfo templateInfo;
                if (m_templates.TryGetValue(m_types[typeIndex], out templateInfo))
                {
                    if (templateInfo.RequiredTypes != null)
                    {
                        foreach (Type requiredType in templateInfo.RequiredTypes)
                        {
                            int requiredTypeIndex;
                            if (m_typeToIndex.TryGetValue(requiredType, out requiredTypeIndex))
                            {
                                callback(requiredTypeIndex);
                            }
                            else
                            {
                                missingCallback(requiredType);
                            }
                        }
                    }
                }
            }
        }

        private void OnTypeLock(int typeIndex)
        {
            ForEachRequiredType(typeIndex, requiredTypeIndex =>
            {
                if (LockType(requiredTypeIndex))
                {
                    OnTypeLock(requiredTypeIndex);
                }
            },
            TypeLocked);

            ForEachEnabledProperty(typeIndex, propertyTypeIndex =>
            {
                if (LockType(propertyTypeIndex))
                {
                    OnTypeLock(propertyTypeIndex);
                }
            },
            TypeLocked);
        }

        private void OnTypeUnlock(int typeIndex)
        {
            ForEachRequiredType(typeIndex, requiredTypeIndex =>
            {
                if (UnlockType(requiredTypeIndex))
                {
                    OnTypeUnlock(requiredTypeIndex);
                }
            },
            TypeUnlocked);

            ForEachEnabledProperty(typeIndex, propertyTypeIndex =>
            {
                if (UnlockType(propertyTypeIndex))
                {
                    OnTypeUnlock(propertyTypeIndex);
                }
            },
            TypeUnlocked);
        }

        public void LockType(Type type)
        {
            int typeIndex;
            if (m_typeToIndex.TryGetValue(type, out typeIndex))
            {
                if (LockType(typeIndex))
                {
                    OnTypeLock(typeIndex);
                }
            }
        }

        public void UnlockType(Type type)
        {
            int typeIndex;
            if (m_typeToIndex.TryGetValue(type, out typeIndex))
            {
                if (UnlockType(typeIndex))
                {
                    OnTypeUnlock(typeIndex);
                }
            }
        }

        private void Select(int typeIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (!mappingInfo.IsSelected)
            {
                mappingInfo.IsSelected = true;
                OnTypeLock(typeIndex);
            }
        }

        private void Unselect(int typeIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (mappingInfo.IsSelected)
            {
                mappingInfo.IsSelected = false;
                OnTypeUnlock(typeIndex);
            }
        }

        private void SelectProperty(int typeIndex, int propIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (!mappingInfo.IsPropertySelected[propIndex])
            {
                mappingInfo.IsPropertySelected[propIndex] = true;

                if (mappingInfo.IsOn && mappingInfo.IsPropertyOn(propIndex))
                {
                    int propMappingIndex = mappingInfo.PropertyMappingSelection[propIndex];
                    if (propMappingIndex >= 0)
                    {
                        ToPropertyTypeIndex(mappingInfo, propIndex, propMappingIndex, propTypeIndex =>
                        {
                            if (LockType(propTypeIndex))
                            {
                                OnTypeLock(propTypeIndex);
                            }
                        },
                        TypeLocked);
                    }
                }
            }
        }

        private void UnselectProperty(int typeIndex, int propIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (mappingInfo.IsPropertySelected[propIndex])
            {
                mappingInfo.IsPropertySelected[propIndex] = false;

                if (mappingInfo.IsOn && !mappingInfo.IsPropertyOn(propIndex))
                {
                    int propMappingIndex = mappingInfo.PropertyMappingSelection[propIndex];
                    if (propMappingIndex >= 0)
                    {
                        ToPropertyTypeIndex(mappingInfo, propIndex, propMappingIndex, propTypeIndex =>
                        {
                            if (UnlockType(propTypeIndex))
                            {
                                OnTypeUnlock(propTypeIndex);
                            }
                        },
                        TypeUnlocked);
                    }
                }
            }
        }

        private void ChangePropertyMapping(int typeIndex, int propIndex, int newSelectedIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (mappingInfo.IsOn && mappingInfo.PropertyMappingSelection[propIndex] != newSelectedIndex)
            {
                if (mappingInfo.IsPropertyOn(propIndex) && mappingInfo.PropertyMappingSelection[propIndex] >= 0)
                {
                    ToPropertyTypeIndex(mappingInfo, propIndex, mappingInfo.PropertyMappingSelection[propIndex], propTypeIndex =>
                    {
                        if (UnlockType(propTypeIndex))
                        {
                            OnTypeUnlock(propTypeIndex);
                        }
                    },
                    TypeUnlocked);
                }

                mappingInfo.PropertyMappingSelection[propIndex] = newSelectedIndex;
                if (mappingInfo.IsPropertyOn(propIndex) && newSelectedIndex >= 0)
                {
                    ToPropertyTypeIndex(mappingInfo, propIndex, newSelectedIndex, propTypeIndex =>
                    {
                        if (LockType(propTypeIndex))
                        {
                            OnTypeLock(propTypeIndex);
                        }
                    },
                    TypeLocked);
                }
            }
        }

        private void TryLockRequiredTypes(int typeIndex, ClassMappingInfo mappingInfo)
        {
            if (mappingInfo.IsOn && mappingInfo.CreateCustomImplementation && mappingInfo.UseTemplate)
            {
                ForEachRequiredType(typeIndex, requiredTypeIndex =>
                {
                    if (LockType(requiredTypeIndex))
                    {
                        OnTypeLock(requiredTypeIndex);
                    }
                },
                TypeLocked);
            }
        }

        private void TryUnlockRequiredTypes(int typeIndex, ClassMappingInfo mappingInfo)
        {
            if (mappingInfo.IsOn && mappingInfo.CreateCustomImplementation && !mappingInfo.UseTemplate)
            {
                ForEachRequiredType(typeIndex, requiredTypeIndex =>
                {
                    if (UnlockType(requiredTypeIndex))
                    {
                        OnTypeUnlock(requiredTypeIndex);
                    }
                },
                TypeUnlocked);
            }
        }

        private void UseTemplate(int typeIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (mappingInfo.UseTemplate)
            {
                return;
            }
            mappingInfo.UseTemplate = true;
            TryLockRequiredTypes(typeIndex, mappingInfo);
        }

        private void DisuseTemplate(int typeIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (!mappingInfo.UseTemplate)
            {
                return;
            }

            mappingInfo.UseTemplate = false;
            TryUnlockRequiredTypes(typeIndex, mappingInfo);
        }

        private void UseCustomImplementation(int typeIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (mappingInfo.CreateCustomImplementation)
            {
                return;
            }
            mappingInfo.CreateCustomImplementation = true;
            TryLockRequiredTypes(typeIndex, mappingInfo);
        }

        private void DisuseCustomImplementation(int typeIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (!mappingInfo.CreateCustomImplementation)
            {
                return;
            }
            mappingInfo.CreateCustomImplementation = false;
            TryUnlockRequiredTypes(typeIndex, mappingInfo);
        }

        public void Reset()
        {
            //for (int typeIndex = 0; typeIndex < m_mappings.Length; ++typeIndex)
            //{
            //    UnselectAllProperties(typeIndex);
            //}
            //UnselectAll();
            LoadMappings();
        }

        public void ClearDependencies()
        {
            m_dependencyTypes.Clear();
        }


        private void LoadMappings()
        {
            PersistentClassMapping[] mappings = GetMappings(out m_storage);

            for (int i = 0; i < mappings.Length; ++i)
            {
                PersistentClassMapping classMapping = mappings[i];
                if (string.IsNullOrEmpty(classMapping.Version))
                {
                    classMapping.MappedTypeName = MappingsUtility.FixTypeName(classMapping.MappedTypeName);
                    classMapping.PersistentTypeName = MappingsUtility.FixTypeName(classMapping.PersistentTypeName);
                    classMapping.PersistentBaseTypeName = MappingsUtility.FixTypeName(classMapping.PersistentBaseTypeName);
                }

                Type type = Type.GetType(classMapping.MappedAssemblyQualifiedName);
                int typeIndex;
                if (type != null && m_typeToIndex.TryGetValue(type, out typeIndex))
                {
                    PersistentPropertyMapping[] pMappings = classMapping.PropertyMappings;
                    PersistentSubclass[] subclasses = classMapping.Subclasses;

                    if (string.IsNullOrEmpty(classMapping.Version))
                    {
                        for (int p = 0; p < pMappings.Length; ++p)
                        {
                            PersistentPropertyMapping pMapping = pMappings[p];
                            pMapping.MappedTypeName = MappingsUtility.FixTypeName(pMapping.MappedTypeName);
                            pMapping.PersistentTypeName = MappingsUtility.FixTypeName(pMapping.PersistentTypeName);
                        }

                        if (classMapping.Subclasses != null)
                        {
                            for (int s = 0; s < classMapping.Subclasses.Length; ++s)
                            {
                                PersistentSubclass subclass = classMapping.Subclasses[s];
                                subclass.TypeName = MappingsUtility.FixTypeName(subclass.TypeName);
                            }
                        }
                    }

                    ClassMappingInfo mappingInfo = m_mappings[typeIndex];
                    mappingInfo.Version = RTSLVersion.Version.ToString();
                    mappingInfo.PropertyMappings = pMappings;
                    mappingInfo.Subclasses = subclasses;
                    mappingInfo.IsLocked = classMapping.IsLocked;
                    mappingInfo.IsSelected = classMapping.IsSelected;
                    mappingInfo.PersistentPropertyTag = classMapping.PersistentPropertyTag;
                    mappingInfo.PersistentSubclassTag = classMapping.PersistentSubclassTag;
                    mappingInfo.CreateCustomImplementation = classMapping.CreateCustomImplementation;
                    mappingInfo.UseTemplate = classMapping.UseTemplate;

                    ExpandType(typeIndex);
                }


            }

            ExpandType(0);
        }

        public Dictionary<Type, PersistentTemplateInfo> GetTemplates()
        {
            return m_templates;
        }

        public PersistentClassMapping[] GetMappings(out PersistentClassMappingsStorage storage)
        {
            return MappingsUtility.GetMappings(m_mappingStoragePath, m_mappingTemplateStoragePath, out storage);
        }

        public void SaveMappings()
        {
            PersistentClassMappingsStorage mappingsStorage;
            Dictionary<string, PersistentClassMapping> exisitingMappingsFromAllSources = GetMappings(out mappingsStorage).ToDictionary(m => m.name);

            GameObject storageGO;
            Dictionary<string, PersistentClassMapping> existingMappings;
            if (mappingsStorage != null)
            {
                storageGO = UnityObject.Instantiate(mappingsStorage.gameObject);
                existingMappings = storageGO.GetComponentsInChildren<PersistentClassMapping>(true).ToDictionary(m => m.name);
            }
            else
            {
                storageGO = new GameObject();
                existingMappings = new Dictionary<string, PersistentClassMapping>();
            }

            Dictionary<int, Dictionary<string, PersistentSubclass>> typeIndexToSubclasses = new Dictionary<int, Dictionary<string, PersistentSubclass>>();
            for (int typeIndex = 0; typeIndex < m_mappings.Length; ++typeIndex)
            {
                ClassMappingInfo mappingInfo = m_mappings[typeIndex];
                Dictionary<string, PersistentSubclass> subclassDictionary;
                if (mappingInfo.Subclasses == null)
                {
                    subclassDictionary = new Dictionary<string, PersistentSubclass>();
                }
                else
                {
                    for (int i = 0; i < mappingInfo.Subclasses.Length; ++i)
                    {
                        PersistentSubclass subclass = mappingInfo.Subclasses[i];
                        subclass.IsEnabled = false;
                    }

                    subclassDictionary = new Dictionary<string, PersistentSubclass>();
                    for (int i = 0; i < mappingInfo.Subclasses.Length; ++i)
                    {
                        PersistentSubclass subclass = mappingInfo.Subclasses[i];
                        if (subclass != null)
                        {
                            if (subclassDictionary.ContainsKey(subclass.FullTypeName))
                            {
                                Debug.LogWarningFormat("Subclass dictionary already contains {0}, BaseType {1}", subclass.FullTypeName, FullName(m_types[typeIndex]));
                            }
                            else
                            {
                                subclassDictionary.Add(subclass.FullTypeName, subclass);
                            }
                        }
                    }
                }

                typeIndexToSubclasses.Add(typeIndex, subclassDictionary);
            }

            for (int typeIndex = 0; typeIndex < m_mappings.Length; ++typeIndex)
            {
                ClassMappingInfo mappingInfo = m_mappings[typeIndex];
                Type type = m_types[typeIndex];
                if (HideMustHaveTypes.Contains(type))
                {
                    mappingInfo.IsSelected = true;
                    mappingInfo.IsLocked = false;
                }

                if (!mappingInfo.IsOn)
                {
                    continue;
                }

                Type baseType = GetEnabledBaseType(typeIndex);
                if (baseType == null)
                {
                    continue;
                }

                int baseTypeIndex;
                if (m_typeToIndex.TryGetValue(baseType, out baseTypeIndex))
                {
                    ClassMappingInfo baseClassMapping = m_mappings[baseTypeIndex];
                    string ns = PersistentClassMapping.ToPersistentNamespace(CodeGen.Namespace(m_types[typeIndex]));
                    string typeName = PersistentClassMapping.ToPersistentName(CodeGen.TypeName(m_types[typeIndex]));
                    string fullTypeName = string.Format("{0}.{1}", ns, typeName);

                    Dictionary<string, PersistentSubclass> subclassDictionary = typeIndexToSubclasses[baseTypeIndex];
                    if (!subclassDictionary.ContainsKey(fullTypeName))
                    {
                        PersistentSubclass subclass = new PersistentSubclass();
                        subclass.IsEnabled = true;
                        subclass.Namespace = PersistentClassMapping.ToPersistentNamespace(CodeGen.Namespace(type));
                        subclass.TypeName = PersistentClassMapping.ToPersistentName(CodeGen.TypeName(type));
                        baseClassMapping.PersistentSubclassTag++;
                        subclass.PersistentTag = baseClassMapping.PersistentSubclassTag;
                        subclass.MappedAssemblyQualifiedName = MappingsUtility.FixTypeName(type.AssemblyQualifiedName);
                        subclassDictionary.Add(fullTypeName, subclass);
                    }
                    else
                    {
                        PersistentSubclass subclass = subclassDictionary[fullTypeName];
                        subclass.MappedAssemblyQualifiedName = MappingsUtility.FixTypeName(type.AssemblyQualifiedName);
                        subclass.IsEnabled = true;
                    }
                }
            }

            for (int typeIndex = 0; typeIndex < m_mappings.Length; ++typeIndex)
            {
                if (m_types[typeIndex].BaseType == null)
                {
                    continue;
                }
                ClassMappingInfo mappingInfo = m_mappings[typeIndex];
                PersistentClassMapping classMapping;
                if (!existingMappings.TryGetValue(FullName(m_types[typeIndex]), out classMapping))
                {
                    GameObject typeStorageGO = new GameObject();
                    typeStorageGO.transform.SetParent(storageGO.transform, false);

                    typeStorageGO.name = FullName(m_types[typeIndex]);
                    classMapping = typeStorageGO.AddComponent<PersistentClassMapping>();

                    PersistentClassMapping existingMapping;
                    if (exisitingMappingsFromAllSources.TryGetValue(FullName(m_types[typeIndex]), out existingMapping))
                    {
                        classMapping.PersistentTypeGUID = existingMapping.PersistentTypeGUID;
                        classMapping.MappedTypeGUID = existingMapping.MappedTypeGUID;
                    }
                    else
                    {
                        classMapping.PersistentTypeGUID = Guid.NewGuid().ToString();
                        classMapping.MappedTypeGUID = Guid.NewGuid().ToString();
                    }
                }

                PersistentTemplateInfo templateInfo;
                if (!classMapping.CreateCustomImplementation || !classMapping.UseTemplate || !m_templates.TryGetValue(m_types[typeIndex], out templateInfo))
                {
                    templateInfo = null;
                }

                List<PersistentPropertyMapping> selectedPropertyMappings = new List<PersistentPropertyMapping>();
                PersistentPropertyMapping[] propertyMappings = mappingInfo.PropertyMappings;
                if (propertyMappings != null)
                {
                    int[] propertyMappingsSelection = mappingInfo.PropertyMappingSelection;
                    for (int propIndex = 0; propIndex < propertyMappings.Length; ++propIndex)
                    {
                        PersistentPropertyMapping propertyMapping = propertyMappings[propIndex];
                        bool hasPropertyInTemplate = templateInfo != null && templateInfo.FieldNames.Contains(propertyMapping.PersistentName);
                        propertyMapping.IsEnabled = mappingInfo.IsPropertySelected[propIndex];
                        propertyMapping.HasPropertyInTemplate = hasPropertyInTemplate;
                        if (propertyMappingsSelection[propIndex] >= 0 && !hasPropertyInTemplate)
                        {
                            propertyMapping.MappedName = mappingInfo.PropertyMappingNames[propIndex][propertyMappingsSelection[propIndex]];
                            propertyMapping.MappedTypeName = mappingInfo.PropertyMappingTypeNames[propIndex][propertyMappingsSelection[propIndex]];
                            propertyMapping.MappedNamespace = mappingInfo.PropertyMappingNamespaces[propIndex][propertyMappingsSelection[propIndex]];
                            propertyMapping.MappedAssemblyName = mappingInfo.PropertyMappingAssemblyNames[propIndex][propertyMappingsSelection[propIndex]];
                            if (propertyMapping.PersistentTag == 0)
                            {
                                mappingInfo.PersistentPropertyTag++;
                                propertyMapping.PersistentTag = mappingInfo.PersistentPropertyTag;
                            }

                            selectedPropertyMappings.Add(propertyMapping);
                        }
                    }
                }


                mappingInfo.PropertyMappings = selectedPropertyMappings.ToArray();
                ExpandType(typeIndex);

                classMapping.Version = mappingInfo.Version;
                classMapping.IsSelected = mappingInfo.IsSelected;
                classMapping.IsLocked = mappingInfo.IsLocked;
                classMapping.PersistentPropertyTag = mappingInfo.PersistentPropertyTag;
                classMapping.PersistentSubclassTag = mappingInfo.PersistentSubclassTag;
                classMapping.PropertyMappings = selectedPropertyMappings.ToArray();
                if (typeIndexToSubclasses.ContainsKey(typeIndex))
                {
                    classMapping.Subclasses = typeIndexToSubclasses[typeIndex].Values.ToArray();
                }
                classMapping.MappedAssemblyName = m_types[typeIndex].Assembly.FullName.Split(',')[0];
                classMapping.MappedNamespace = CodeGen.Namespace(m_types[typeIndex]);
                classMapping.MappedTypeName = CodeGen.TypeName(m_types[typeIndex]);

                classMapping.PersistentNamespace = PersistentClassMapping.ToPersistentNamespace(classMapping.MappedNamespace);
                classMapping.PersistentTypeName = PersistentClassMapping.ToPersistentName(CodeGen.TypeName(m_types[typeIndex]));

                Type baseType = GetEnabledBaseType(typeIndex);
                if (baseType == null || baseType == typeof(object))
                {
                    classMapping.PersistentBaseNamespace = typeof(PersistentSurrogate<>).Namespace;
                    classMapping.PersistentBaseTypeName = typeof(PersistentSurrogate<>).Name;
                }
                else
                {
                    classMapping.PersistentBaseNamespace = PersistentClassMapping.ToPersistentNamespace(CodeGen.Namespace(baseType));
                    classMapping.PersistentBaseTypeName = PersistentClassMapping.ToPersistentName(CodeGen.TypeName(baseType));
                }

                classMapping.CreateCustomImplementation = mappingInfo.CreateCustomImplementation;
                classMapping.UseTemplate = mappingInfo.UseTemplate;
            }

            PersistentClassMappingsStorage storage = storageGO.GetComponent<PersistentClassMappingsStorage>();
            if (mappingsStorage == null)
            {
                mappingsStorage = storageGO.AddComponent<PersistentClassMappingsStorage>();
            }
            mappingsStorage.Version = RTSLVersion.Version.ToString();

            EditorUtility.SetDirty(storageGO);

            if (!Directory.Exists(Path.GetFullPath(RTSLPath.UserPrefabsPath)))
            {
                Directory.CreateDirectory(Path.GetFullPath(RTSLPath.UserPrefabsPath));
            }
            PrefabUtility.SaveAsPrefabAsset(storageGO, m_mappingStoragePath);
            UnityObject.DestroyImmediate(storageGO);
        }

        private Type GetEnabledBaseType(int typeIndex)
        {
            Type baseType = null;
            Type type = m_types[typeIndex];
            while (true)
            {
                type = type.BaseType;
                if (type == m_baseType)
                {
                    baseType = type;
                    break;
                }

                if (type == null)
                {
                    break;
                }

                int baseIndex;
                if (m_typeToIndex.TryGetValue(type, out baseIndex))
                {
                    if (m_mappings[baseIndex].IsOn)
                    {
                        baseType = type;
                        break;
                    }
                }
            }

            return baseType;
        }

        private int GetAncestorsCount(Type type)
        {
            int count = 0;
            while (type != null && type.BaseType != m_baseType)
            {
                count++;
                type = type.BaseType;
            }
            return count;
        }

        private GUIContent m_guiContent = new GUIContent();
        private void DrawTypeEditor(int rootTypeIndex, int typeIndex, int indent = 1)
        {
            Type type = m_types[typeIndex];
            if (type == m_baseType || HideMustHaveTypes.Contains(type))
            {
                return;
            }

            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            GUIStyle foldOutStyle;
            if (mappingInfo.ObsoleteAttribute != null)
            {
                foldOutStyle = m_deprecatedFoldoutStyle;
                m_guiContent.tooltip = mappingInfo.ObsoleteAttribute.Message;
                m_guiContent.text = type.Name + " [Deprecated]";
            }
            else
            {
                foldOutStyle = EditorStyles.foldout;
                m_guiContent.tooltip = null;
                m_guiContent.text = type.Name;
            }


            bool isExpandedChanged;
            bool isExpanded;
            bool isSelectionChanged;

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            {
                GUILayout.Space(5 + 18 * (indent - 1));
                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginDisabledGroup(mappingInfo.IsLocked);
                bool isEnabled;
                EditorGUI.BeginChangeCheck();
                isEnabled = EditorGUILayout.Toggle(mappingInfo.IsOn, GUILayout.MaxWidth(15));
                if (EditorGUI.EndChangeCheck())
                {
                    if (isEnabled)
                    {
                        Select(typeIndex);
                    }
                    else
                    {
                        Unselect(typeIndex);
                    }
                }

                EditorGUI.EndDisabledGroup();

                isSelectionChanged = EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
                if (indent == 1)
                {
                    mappingInfo.IsExpanded = EditorGUILayout.Foldout(mappingInfo.IsExpanded, m_guiContent, true, foldOutStyle);
                    isExpanded = mappingInfo.IsExpanded;
                }
                else
                {
                    m_mappings[rootTypeIndex].IsParentExpanded[indent - 2] = EditorGUILayout.Foldout(m_mappings[rootTypeIndex].IsParentExpanded[indent - 2], m_guiContent, true, foldOutStyle);
                    isExpanded = m_mappings[rootTypeIndex].IsParentExpanded[indent - 2];
                }
                isExpandedChanged = EditorGUI.EndChangeCheck();
            }
            EditorGUILayout.EndHorizontal();

            if (isExpandedChanged || isSelectionChanged)
            {
                if (isExpandedChanged)
                {
                    mappingInfo.ExpandedCounter = isExpanded ?
                        mappingInfo.ExpandedCounter + 1 :
                        mappingInfo.ExpandedCounter - 1;
                }

                TryExpandType(typeIndex);
            }

            if (isExpanded)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5 + 18 * indent);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("Full Name: " + FullName(m_types[typeIndex]));

                    mappingInfo.IsSupportedPlaftormsSectionExpanded = EditorGUILayout.Foldout(mappingInfo.IsSupportedPlaftormsSectionExpanded, "Supported Platforms");
                    if (mappingInfo.IsSupportedPlaftormsSectionExpanded)
                    {
                        string[] platformNames = Enum.GetNames(typeof(RuntimePlatform));
                        RuntimePlatform[] platforms = (RuntimePlatform[])Enum.GetValues(typeof(RuntimePlatform));

                        for (int i = 0; i < platformNames.Length; ++i)
                        {
                            EditorGUI.BeginChangeCheck();
                            bool platformChecked = EditorGUILayout.Toggle(platformNames[i], mappingInfo.UnsupportedPlatforms == null || !mappingInfo.UnsupportedPlatforms.Contains(platforms[i]));
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (mappingInfo.UnsupportedPlatforms == null)
                                {
                                    mappingInfo.UnsupportedPlatforms = new HashSet<RuntimePlatform>();
                                }
                                if (platformChecked)
                                {
                                    mappingInfo.UnsupportedPlatforms.Remove(platforms[i]);
                                }
                                else
                                {
                                    mappingInfo.UnsupportedPlatforms.Add(platforms[i]);
                                }
                            }

                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginVertical();
                {
                    if (!mappingInfo.HasCustomImplementation)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(5 + 18 * indent);

                            EditorGUI.BeginChangeCheck();
                            bool createCustomImplementation = EditorGUILayout.Toggle(mappingInfo.CreateCustomImplementation, GUILayout.MaxWidth(20));
                            EditorGUILayout.LabelField("Custom Implementation", GUILayout.MaxWidth(230));
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (createCustomImplementation)
                                {
                                    UseCustomImplementation(typeIndex);
                                }
                                else
                                {
                                    DisuseCustomImplementation(typeIndex);
                                }
                            }
                            else
                            {
                                if (mappingInfo.HasTemplate)
                                {
                                    EditorGUI.BeginChangeCheck();
                                    bool useTemplate = EditorGUILayout.Popup(mappingInfo.UseTemplate ? 1 : 0, m_customImplementationOptions, GUILayout.MaxWidth(230)) == 1;
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        if (useTemplate)
                                        {
                                            UseTemplate(typeIndex);
                                        }
                                        else
                                        {
                                            DisuseTemplate(typeIndex);
                                        }
                                    }
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(5 + 18 * indent);

                            if (GUILayout.Button("Edit Custom Implementation", GUILayout.Width(215)))
                            {
                                Type t = m_types[typeIndex];
                                string fullTypeName = PersistentClassMapping.ToPersistentFullName(CodeGen.Namespace(t), t.Name);
                                UnityObject scriptFile;
                                if (m_typeToScriptObject.TryGetValue(fullTypeName, out scriptFile))
                                {
                                    AssetDatabase.OpenAsset(scriptFile);
                                }
                                else
                                {
                                    if (EditorApplication.isCompiling)
                                    {
                                        EditorUtility.DisplayDialog("Unable to open file for editing", "Try again when script compilation will be finished", "OK");
                                    }
                                    else
                                    {
                                        EditorUtility.DisplayDialog("Unable to open file for editing", "Open file manually using project window", "OK");
                                    }
                                }
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(5 + 18 * indent);

                        int selectedPropertiesCount = mappingInfo.IsPropertySelected.Count(enabled => enabled);
                        bool isAllPropertiesSelected = selectedPropertiesCount == mappingInfo.IsPropertySelected.Length;
                        if (isAllPropertiesSelected)
                        {
                            EditorGUILayout.Toggle(true, GUILayout.MaxWidth(20));
                            EditorGUILayout.LabelField("Select All Properties", GUILayout.MaxWidth(230));
                        }
                        else if (selectedPropertiesCount == 0)
                        {
                            EditorGUILayout.Toggle(false, GUILayout.MaxWidth(20));
                            EditorGUILayout.LabelField("Select All Properties", GUILayout.MaxWidth(230));
                        }
                        else
                        {
                            EditorGUILayout.Toggle(false, "ToggleMixed", GUILayout.MaxWidth(20));
                            EditorGUILayout.LabelField("Select All Properties", GUILayout.MaxWidth(230));
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (isAllPropertiesSelected)
                            {
                                UnselectAllProperties(typeIndex);
                            }
                            else
                            {
                                SelectAllProperties(typeIndex);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    PersistentTemplateInfo templateInfo;
                    if (!mappingInfo.CreateCustomImplementation || !mappingInfo.UseTemplate || !m_templates.TryGetValue(m_types[typeIndex], out templateInfo))
                    {
                        templateInfo = null;
                    }

                    for (int propIndex = 0; propIndex < mappingInfo.PropertyMappings.Length; ++propIndex)
                    {
                        PersistentPropertyMapping pMapping = mappingInfo.PropertyMappings[propIndex];
                        bool hasPropertyInTemplate = templateInfo != null && templateInfo.FieldNames.Contains(pMapping.PersistentName);
                        if (hasPropertyInTemplate)
                        {
                            continue;
                        }

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(5 + 18 * indent);

                            EditorGUI.BeginChangeCheck();
                            bool isPropertySelected = EditorGUILayout.Toggle(mappingInfo.IsPropertySelected[propIndex], GUILayout.MaxWidth(20));
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (isPropertySelected)
                                {
                                    SelectProperty(typeIndex, propIndex);
                                }
                                else
                                {
                                    UnselectProperty(typeIndex, propIndex);
                                }
                            }

                            int selectedIndex = mappingInfo.PropertyMappingSelection[propIndex];
                            bool isEnum = false;
                            if (selectedIndex >= 0)
                            {
                                Type mappedType = mappingInfo.PropertyMappingTypes[propIndex][selectedIndex];
                                if (mappedType != null && mappedType.IsEnum)
                                {
                                    isEnum = true;
                                }
                            }

                            m_guiContent.text = pMapping.PersistentName + (pMapping.IsNonPublic ? " (non-public)" : "");
                            m_guiContent.tooltip = pMapping.MappedFullTypeName + (isEnum ? " Enum" : "");
                            EditorGUILayout.LabelField(m_guiContent, GUILayout.MaxWidth(230));

                            int newSelectedIndex = selectedIndex >= 0 && mappingInfo.PropertyIsObsolete[propIndex][selectedIndex] ?
                                EditorGUILayout.Popup(selectedIndex, mappingInfo.PropertyMappingsDisplayNames[propIndex], m_deprecatedPopupStyle) :
                                EditorGUILayout.Popup(selectedIndex, mappingInfo.PropertyMappingsDisplayNames[propIndex]);

                            if (selectedIndex != newSelectedIndex)
                            {
                                ChangePropertyMapping(typeIndex, propIndex, newSelectedIndex);
                            }

                            EditorGUI.BeginChangeCheck();
                            GUILayout.Button("X", GUILayout.Width(20));
                            if (EditorGUI.EndChangeCheck())
                            {
                                ChangePropertyMapping(typeIndex, propIndex, -1);
                            }

                            EditorGUILayout.LabelField("Slot: " + pMapping.PersistentTag, GUILayout.Width(60));
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    Type baseType = type.BaseType;
                    while (baseType != m_baseType)
                    {
                        int parentIndex;
                        if (m_typeToIndex.TryGetValue(baseType, out parentIndex))
                        {
                            DrawTypeEditor(rootTypeIndex, parentIndex, indent + 1);
                            break;
                        }
                        baseType = baseType.BaseType;
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Separator();
            }
        }

        private void SelectAllProperties(int typeIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (mappingInfo.IsPropertySelected == null)
            {
                return;
            }
            for (int propIndex = 0; propIndex < mappingInfo.IsPropertySelected.Length; ++propIndex)
            {
                SelectProperty(typeIndex, propIndex);
            }
        }

        private void UnselectAllProperties(int typeIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (mappingInfo.IsPropertySelected == null)
            {
                return;
            }
            for (int propIndex = 0; propIndex < mappingInfo.IsPropertySelected.Length; ++propIndex)
            {
                UnselectProperty(typeIndex, propIndex);
            }
        }

        private void TryExpandType(int typeIndex)
        {
            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            if (mappingInfo.PropertyMappings != null)
            {
                return;
            }
            if (mappingInfo.ExpandedCounter > 0 || mappingInfo.IsOn)
            {
                ExpandType(typeIndex);
            }
        }

        private void ExpandType(int typeIndex)
        {
            Type type = m_types[typeIndex];

            Dictionary<PersistentPropertyMapping, bool> pMappingsEnabled = new Dictionary<PersistentPropertyMapping, bool>();

            ClassMappingInfo mappingInfo = m_mappings[typeIndex];
            PersistentPropertyMapping[] fieldMappings = mappingInfo.PropertyMappings != null ?
                mappingInfo.PropertyMappings.Where(p => !p.IsProperty).ToArray() :
                new PersistentPropertyMapping[0];

            HashSet<string> fieldMappingsHs = new HashSet<string>();
            IEnumerable<string> fmapKeys = fieldMappings.Select(fMap => fMap.PersistentFullTypeName + " " + fMap.PersistentName);
            foreach (string key in fmapKeys)
            {
                if (!fieldMappingsHs.Contains(key))
                {
                    fieldMappingsHs.Add(key);
                }
            }

            PersistentPropertyMapping[] propertyMappings = mappingInfo.PropertyMappings != null ?
                mappingInfo.PropertyMappings.Where(p => p.IsProperty).ToArray() :
                new PersistentPropertyMapping[0];

            HashSet<string> propertyMappingsHs = new HashSet<string>();
            IEnumerable<string> pmapKeys = propertyMappings.Select(pMap => pMap.PersistentFullTypeName + " " + pMap.PersistentName);
            foreach (string key in pmapKeys)
            {
                if (!propertyMappingsHs.Contains(key))
                {
                    propertyMappingsHs.Add(key);
                }
            }

            FieldInfo[] fields = CodeGen.GetFields(type);
            HashSet<string> fieldHs = new HashSet<string>(fields.Select(fInfo => FullName(fInfo.FieldType) + " " + fInfo.Name));

            PropertyInfo[] properties = CodeGen.GetProperties(type);
            HashSet<string> propertyHs = new HashSet<string>(properties.Select(pInfo => FullName(pInfo.PropertyType) + " " + pInfo.Name));

            for (int i = 0; i < fieldMappings.Length; ++i)
            {
                PersistentPropertyMapping mapping = fieldMappings[i];
                string key = mapping.MappedFullTypeName + " " + mapping.MappedName;

                Type mappedType = mapping.MappedType;
                if (!fieldHs.Contains(key) || mappedType == null)
                {
                    mapping.MappedName = null;
                    mapping.MappedTypeName = null;
                    mapping.MappedNamespace = null;
                    mapping.MappedAssemblyName = null;

                    mapping.UseSurrogate = false;
                    mapping.UseSurrogate2 = false;
                    mapping.HasDependenciesOrIsDependencyItself = false;
                    pMappingsEnabled.Add(mapping, false);
                }
                else
                {
                    mapping.IsNonPublic = type.GetField(mapping.MappedName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null;
                    mapping.UseSurrogate = CodeGen.GetSurrogateType(mappedType, 0) != null;
                    mapping.UseSurrogate2 = CodeGen.GetSurrogateType(mappedType, 1) != null;
                    mapping.HasDependenciesOrIsDependencyItself = m_codeGen.HasDependencies(mappedType);
                    pMappingsEnabled.Add(mapping, mapping.IsEnabled);
                }
            }

            for (int i = 0; i < propertyMappings.Length; ++i)
            {
                PersistentPropertyMapping mapping = propertyMappings[i];
                string key = mapping.MappedFullTypeName + " " + mapping.MappedName;

                Type mappedType = mapping.MappedType;
                if (!propertyHs.Contains(key) || mappedType == null)
                {
                    mapping.MappedName = null;
                    mapping.MappedTypeName = null;
                    mapping.MappedNamespace = null;
                    mapping.MappedAssemblyName = null;

                    mapping.HasDependenciesOrIsDependencyItself = false;
                    mapping.UseSurrogate = false;
                    mapping.UseSurrogate2 = false;

                    pMappingsEnabled.Add(mapping, false);
                }
                else
                {
                    mapping.UseSurrogate = CodeGen.GetSurrogateType(mappedType, 0) != null;
                    mapping.UseSurrogate2 = CodeGen.GetSurrogateType(mappedType, 1) != null;
                    mapping.HasDependenciesOrIsDependencyItself = m_codeGen.HasDependencies(mappedType);

                    pMappingsEnabled.Add(mapping, mapping.IsEnabled);
                }
            }

            List<PersistentPropertyMapping> pMappings = mappingInfo.PropertyMappings != null ? mappingInfo.PropertyMappings.ToList() : new List<PersistentPropertyMapping>();
            for (int f = 0; f < fields.Length; ++f)
            {
                FieldInfo fInfo = fields[f];

                string key = string.Format("{0}.{1}",
                    PersistentClassMapping.ToPersistentNamespace(CodeGen.Namespace(fInfo.FieldType)),
                    CodeGen.TypeName(fInfo.FieldType)) + " " + fInfo.Name;

                if (fieldMappingsHs.Contains(key))
                {
                    continue;
                }

                PersistentPropertyMapping pMapping = new PersistentPropertyMapping();
                pMapping.PersistentName = fInfo.Name;
                pMapping.PersistentTypeName = CodeGen.TypeName(fInfo.FieldType);
                pMapping.PersistentNamespace = PersistentClassMapping.ToPersistentNamespace(CodeGen.Namespace(fInfo.FieldType));

                pMapping.MappedName = fInfo.Name;
                pMapping.MappedTypeName = CodeGen.TypeName(fInfo.FieldType);
                pMapping.MappedNamespace = CodeGen.Namespace(fInfo.FieldType);
                pMapping.MappedAssemblyName = fInfo.FieldType.Assembly.FullName.Split(',')[0];
                pMapping.IsProperty = false;

                pMapping.UseSurrogate = CodeGen.GetSurrogateType(fInfo.FieldType, 0) != null;
                pMapping.UseSurrogate2 = CodeGen.GetSurrogateType(fInfo.FieldType, 1) != null;
                pMapping.HasDependenciesOrIsDependencyItself = m_codeGen.HasDependencies(fInfo.FieldType);
                pMapping.IsNonPublic = !fInfo.IsPublic;

                pMappingsEnabled.Add(pMapping, false);
                pMappings.Add(pMapping);
            }

            for (int p = 0; p < properties.Length; ++p)
            {
                PropertyInfo pInfo = properties[p];

                string key = string.Format("{0}.{1}",
                    PersistentClassMapping.ToPersistentNamespace(CodeGen.Namespace(pInfo.PropertyType)),
                    CodeGen.TypeName(pInfo.PropertyType)) + " " + pInfo.Name;

                if (propertyMappingsHs.Contains(key))
                {
                    continue;
                }

                PersistentPropertyMapping pMapping = new PersistentPropertyMapping();

                pMapping.PersistentName = pInfo.Name;       //property name of mapping
                pMapping.PersistentTypeName = CodeGen.TypeName(pInfo.PropertyType);
                pMapping.PersistentNamespace = PersistentClassMapping.ToPersistentNamespace(CodeGen.Namespace(pInfo.PropertyType));

                pMapping.MappedName = pInfo.Name;           //property name of unity type
                pMapping.MappedTypeName = CodeGen.TypeName(pInfo.PropertyType);
                pMapping.MappedNamespace = CodeGen.Namespace(pInfo.PropertyType);
                pMapping.MappedAssemblyName = pInfo.PropertyType.Assembly.FullName.Split(',')[0];
                pMapping.IsProperty = true;

                pMapping.UseSurrogate = CodeGen.GetSurrogateType(pInfo.PropertyType, 0) != null;
                pMapping.UseSurrogate2 = CodeGen.GetSurrogateType(pInfo.PropertyType, 1) != null;
                pMapping.HasDependenciesOrIsDependencyItself = m_codeGen.HasDependencies(pInfo.PropertyType);

                pMappingsEnabled.Add(pMapping, false);
                pMappings.Add(pMapping);
            }

            mappingInfo.PropertyMappings = pMappings.ToArray();
            mappingInfo.IsPropertySelected = new bool[pMappings.Count];
            for (int i = 0; i < mappingInfo.PropertyMappings.Length; ++i)
            {
                mappingInfo.IsPropertySelected[i] = pMappingsEnabled[mappingInfo.PropertyMappings[i]];
            }
            mappingInfo.PropertyMappingNames = new string[pMappings.Count][];
            mappingInfo.PropertyMappingsDisplayNames = new GUIContent[pMappings.Count][];
            mappingInfo.PropertyIsObsolete = new bool[pMappings.Count][];
            mappingInfo.PropertyMappingTypes = new Type[pMappings.Count][];
            mappingInfo.PropertyMappingTypeNames = new string[pMappings.Count][];
            mappingInfo.PropertyMappingNamespaces = new string[pMappings.Count][];
            mappingInfo.PropertyMappingAssemblyNames = new string[pMappings.Count][];
            mappingInfo.PropertyMappingSelection = new int[pMappings.Count];

            string[][] mappedKeys = new string[pMappings.Count][];
            for (int propIndex = 0; propIndex < pMappings.Count; ++propIndex)
            {
                PersistentPropertyMapping pMapping = pMappings[propIndex];

                string ns = PersistentClassMapping.ToMappedNamespace(pMapping.PersistentNamespace);
                if (!string.IsNullOrEmpty(ns))
                {
                    ns += ".";
                }
                var propertyInfo = GetSuitableFields(fields, ns + pMapping.PersistentTypeName)
                    .Select(f => new {
                        Name = f.Name,
                        ObsoleteAttribute = f.GetCustomAttributes(false).OfType<ObsoleteAttribute>().FirstOrDefault(),
                        Type = f.FieldType,
                        TypeName = CodeGen.TypeName(f.FieldType),
                        Namespace = CodeGen.Namespace(f.FieldType),
                        Assembly = f.FieldType.Assembly.FullName.Split(',')[0]
                    })
                    .Union(GetSuitableProperties(properties, ns + pMapping.PersistentTypeName)
                    .Select(p => new {
                        Name = p.Name,
                        ObsoleteAttribute = p.GetCustomAttributes(false).OfType<ObsoleteAttribute>().FirstOrDefault(),
                        Type = p.PropertyType,
                        TypeName = CodeGen.TypeName(p.PropertyType),
                        Namespace = CodeGen.Namespace(p.PropertyType),
                        Assembly = p.PropertyType.Assembly.FullName.Split(',')[0]
                    }))
                    .OrderBy(p => p.Name)
                    .ToArray();

                mappingInfo.PropertyMappingNames[propIndex] = propertyInfo.Select(p => p.Name).ToArray();
                mappingInfo.PropertyIsObsolete[propIndex] = propertyInfo.Select(p => p.ObsoleteAttribute != null).ToArray();
                mappingInfo.PropertyMappingsDisplayNames[propIndex] = propertyInfo.Select(p => new GUIContent(p.ObsoleteAttribute != null ? p.Name + " [Deprecated]" : p.Name, p.ObsoleteAttribute != null ? p.ObsoleteAttribute.Message : "")).ToArray();
                mappingInfo.PropertyMappingTypeNames[propIndex] = propertyInfo.Select(p => p.TypeName).ToArray();
                mappingInfo.PropertyMappingTypes[propIndex] = propertyInfo.Select(p => p.Type).ToArray();
                mappingInfo.PropertyMappingNamespaces[propIndex] = propertyInfo.Select(p => p.Namespace).ToArray();
                mappingInfo.PropertyMappingAssemblyNames[propIndex] = propertyInfo.Select(p => p.Assembly).ToArray();
                mappedKeys[propIndex] = propertyInfo.Select(m => (string.IsNullOrEmpty(m.Namespace) ? m.TypeName : m.Namespace + "." + m.TypeName) + " " + m.Name).ToArray();
            }

            for (int propIndex = 0; propIndex < mappingInfo.PropertyMappingSelection.Length; ++propIndex)
            {
                PersistentPropertyMapping mapping = mappingInfo.PropertyMappings[propIndex];

                mappingInfo.PropertyMappingSelection[propIndex] = Array.IndexOf(mappedKeys[propIndex], mapping.MappedFullTypeName + " " + mapping.MappedName);
            }
        }

        private IEnumerable<PropertyInfo> GetSuitableProperties(PropertyInfo[] properties, string persistentType)
        {
            return properties.Where(pInfo => FullName(pInfo.PropertyType) == persistentType);
        }

        private IEnumerable<FieldInfo> GetSuitableFields(FieldInfo[] fields, string persistentType)
        {
            return fields.Where(fInfo => FullName(fInfo.FieldType) == persistentType);
        }

        public static string FullName(Type type)
        {
            string name = type.FullName;
            name = Regex.Replace(name, @", Version=\d+.\d+.\d+.\d+", string.Empty);
            name = Regex.Replace(name, @", Culture=\w+", string.Empty);
            name = Regex.Replace(name, @", PublicKeyToken=\w+", string.Empty);
            return name;
        }
    }

    public class PersistentClassMapperWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            PersistentClassMapperWindow prevWindow = GetWindow<PersistentClassMapperWindow>();
            if (prevWindow != null)
            {
                prevWindow.Close();
            }

            PersistentClassMapperWindow window = CreateInstance<PersistentClassMapperWindow>();
            window.titleContent = new GUIContent("Persistent Classes");
            window.Show();
            window.position = new Rect(20, 40, 1280, 768);
        }

        private static readonly Type[] m_mostImportantUOTypes =
        {
            typeof(UnityObject),
            typeof(GameObject),
            typeof(Renderer),
            typeof(MeshRenderer),
            typeof(MeshFilter),
            typeof(SkinnedMeshRenderer),
            typeof(Mesh),
            typeof(Material),
            typeof(Rigidbody),
            typeof(BoxCollider),
            typeof(SphereCollider),
            typeof(CapsuleCollider),
            typeof(MeshCollider),
            typeof(Camera),
            typeof(AudioClip),
            typeof(AudioSource),
            typeof(Light),
        };

        private static readonly Type[] m_mostImportantSurrogateTypes =
        {
            typeof(object),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Vector2Int),
            typeof(Vector3Int),
            typeof(Color),
            typeof(Color32),
            typeof(Matrix4x4),
        };

        private Type[] m_uoTypes;
        private PersistentClassMapperGUI m_uoMapperGUI;
        private PersistentClassMapperGUI m_surrogatesMapperGUI;
        private CodeGen m_codeGen = new CodeGen();

        float m_resizerPosition;
        bool m_resize = false;

        private void Resizer(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = Separator(color, thickness, padding);
            EditorGUIUtility.AddCursorRect(r, MouseCursor.ResizeVertical);

            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
            {
                m_resize = true;
            }

            if (m_resize && Event.current.type == EventType.MouseDrag)
            {
                float newPosition = Event.current.mousePosition.y - (padding + thickness);

                if (newPosition > position.height - 180)
                {
                    m_resizerPosition = position.height - 180;
                }
                else
                {
                    m_resizerPosition = newPosition;
                }

                Repaint();
            }

            if (Event.current.type == EventType.MouseUp)
            {
                m_resize = false;
            }
        }

        private static Rect Separator(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
            return r;
        }

        private void OnEnable()
        {
            m_resizerPosition = SessionState.GetFloat(PersistentClassMapperGUI.k_SessionStatePrefix + "ResizerPosition", position.height / 2);
        }

        private void OnDisable()
        {
            if (m_uoMapperGUI != null)
            {
                m_uoMapperGUI.OnDisable();
            }

            if (m_surrogatesMapperGUI != null)
            {
                m_surrogatesMapperGUI.OnDisable();
            }

            SessionState.SetFloat(PersistentClassMapperGUI.k_SessionStatePrefix + "ResizerPosition", m_resizerPosition);
        }

        private void OnGUI()
        {
            Initialize(true);

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginVertical(GUILayout.Height(m_resizerPosition));
            m_uoMapperGUI.OnGUI();
            EditorGUILayout.EndVertical();

            Resizer(Color.gray);

            EditorGUILayout.BeginVertical();
            m_surrogatesMapperGUI.OnGUI();
            EditorGUILayout.EndVertical();

            Separator(Color.gray, 2, 10);

            EditorGUILayout.EndVertical();

            Buttons();
        }

        public static void CreateOrPatchMappings()
        {
            if (!File.Exists(Path.GetFullPath(RTSLPath.UserPrefabsPath + @"/ClassMappingsStorage.prefab")))
            {
                SaveMappings();
            }
            else
            {
                PatchUtility.PatchPersistentClassMappings();
            }
        }

        public static void SaveMappings()
        {
            //TODO: Replace with appropriate code (without creating PersistentClassMapperWindow...)
            PersistentClassMapperWindow window = CreateInstance<PersistentClassMapperWindow>();
            window.Initialize(false);
            window.m_uoMapperGUI.SaveMappings();
            window.m_surrogatesMapperGUI.SaveMappings();
            DestroyImmediate(window);
        }

        public static void CreatePersistentClasses()
        {
            PersistentClassMapping[] uoMappings = MappingsUtility.GetClassMappings();
            PersistentClassMapping[] surrogateMappings = MappingsUtility.GetSurrogateMappings();
            Dictionary<Type, PersistentTemplateInfo> templates = GetPersistentTemplates(GetAllTypes());
            CreatePersistentClasses(uoMappings, templates, surrogateMappings, templates);
        }

        private void Initialize(bool userAction)
        {
            Assembly[] assemblies = null;
            Type[] uoTypes = null;
            Type[] types = null;
            Dictionary<string, HashSet<Type>> declaredIn = null;
            Dictionary<Type, PersistentTemplateInfo> templates = null;
            FilePathStorage filePathStorage = null;
            if (m_uoMapperGUI == null || m_surrogatesMapperGUI == null)
            {
                GetUOAssembliesAndTypes(out assemblies, out m_uoTypes);
                uoTypes = m_uoTypes.Union(new[] { typeof(RuntimePrefab), typeof(RuntimeScene) }).ToArray();
                GetSurrogateAssembliesAndTypes(assemblies, m_uoTypes, out declaredIn, out types);
                Type[] allTypes = uoTypes.Union(types).ToArray();

                templates = GetPersistentTemplates(allTypes);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(RTSLPath.FilePathStoragePath);
                if (go != null)
                {
                    filePathStorage = go.GetComponent<FilePathStorage>();
                }
            }

            if (m_uoMapperGUI == null)
            {
                m_uoMapperGUI = CreateUOMapperGUI(assemblies, uoTypes, templates, filePathStorage, userAction);
                m_uoMapperGUI.TypeLocked += OnUOTypeLocked;
                m_uoMapperGUI.TypeUnlocked += OnUOTypeUnlocked;
            }

            if (m_surrogatesMapperGUI == null)
            {
                m_surrogatesMapperGUI = CreateSurrogatesMapperGUI(types, declaredIn, templates, filePathStorage, userAction);
                m_surrogatesMapperGUI.TypeLocked += OnSurrogateTypeLocked;
                m_surrogatesMapperGUI.TypeUnlocked += OnSurrogateTypeUnlocked;
            }

            bool uoInitialized = m_uoMapperGUI.InitializeAndLoadMappings();
            bool surrInitialized = m_surrogatesMapperGUI.InitializeAndLoadMappings();

            if (uoInitialized)
            {
                m_uoMapperGUI.LockTypes();
            }
            if (surrInitialized)
            {
                m_surrogatesMapperGUI.LockTypes();
            }
        }

        private PersistentClassMapperGUI CreateSurrogatesMapperGUI(Type[] types, Dictionary<string, HashSet<Type>> declaredIn, Dictionary<Type, PersistentTemplateInfo> templates, FilePathStorage filePathStorage, bool userAction)
        {
            return new PersistentClassMapperGUI(/*GetInstanceID() + 1*/1,
                 m_codeGen,
                 RTSLPath.SurrogatesMappingsStoragePath,
                 RTSLPath.SurrogatesMappingsTemplatePath.ToArray(),
                 filePathStorage,
                 typeof(object),
                 types,
                 templates,
                 new[] { "All" }.Union(declaredIn.Where(t => t.Value.Count > 0).Select(t => t.Key)).ToArray(),
                 "Declaring Type:",
                 true,
                 (type, groupName) => declaredIn.Any(kvp => kvp.Key.Contains(groupName) && kvp.Value.Contains(type)), userAction);
        }

        private PersistentClassMapperGUI CreateUOMapperGUI(Assembly[] assemblies, Type[] uoTypes, Dictionary<Type, PersistentTemplateInfo> templates, FilePathStorage filePathStorage, bool userAction)
        {
            return new PersistentClassMapperGUI(/*GetInstanceID()*/0,
                m_codeGen,
                RTSLPath.ClassMappingsStoragePath,
                RTSLPath.ClassMappingsTemplatePath.ToArray(),
                filePathStorage,
                typeof(object),
                uoTypes,
                templates,
                assemblies.Select(a => a == null ? "All" : a.GetName().Name).ToArray(),
                "Assembly",
                false,
                (type, groupName) => type.Assembly.GetName().Name == groupName, userAction);
        }

        private void Buttons()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();

            bool patchRequired = m_uoMapperGUI != null && (m_uoMapperGUI.Storage == null || m_uoMapperGUI.Storage.PatchCounter < 4) || m_surrogatesMapperGUI != null && (m_surrogatesMapperGUI.Storage == null || m_surrogatesMapperGUI.Storage.PatchCounter < 4);

            if (patchRequired)
            {
                if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
                {
                    EditorGUI.BeginChangeCheck();
                    GUILayout.Button("Update Mappings", GUILayout.Height(20));
                    if (EditorGUI.EndChangeCheck())
                    {
                        PatchUtility.PatchPersistentClassMappings();

                        m_uoMapperGUI.ClearDependencies();
                        m_surrogatesMapperGUI.ClearDependencies();
                        m_uoMapperGUI.Reset();
                        m_surrogatesMapperGUI.Reset();
                        m_uoMapperGUI.LockTypes();
                        m_surrogatesMapperGUI.LockTypes();
                    }
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Button("Undo & Reload", GUILayout.Height(20));
                if (EditorGUI.EndChangeCheck())
                {
                    m_uoMapperGUI.ClearDependencies();
                    m_surrogatesMapperGUI.ClearDependencies();
                    m_uoMapperGUI.Reset();
                    m_surrogatesMapperGUI.Reset();
                    m_uoMapperGUI.LockTypes();
                    m_surrogatesMapperGUI.LockTypes();
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.Button("Build All", GUILayout.Height(20));
                if (EditorGUI.EndChangeCheck())
                {
                    try
                    {
                        Selection.activeObject = null;
                        EditorUtility.DisplayProgressBar("Build All", "Creating persistent classes", 0.33f);
                        EditorPrefs.SetBool("RTSL_BuildAll", true);
                        SaveMappingsAndCreatePersistentClasses();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                        EditorPrefs.SetBool("RTSL_BuildAll", false);
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                    }
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.Button("Create Persistent Classes", GUILayout.Height(20));
                if (EditorGUI.EndChangeCheck())
                {
                    SaveMappingsAndCreatePersistentClasses();
                }
            }

            EditorGUILayout.EndHorizontal();
            if (patchRequired)
            {
                EditorGUILayout.HelpBox("Persistent Class Mappings must be updated. Please click \"Update Mappings\" button", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Please note that most of the data are stored and restored using public properties which may cause undesired side effects. For example accessing renderer.material or meshfilter.mesh will instantiate new objects.", MessageType.Info);
            }
            EditorGUILayout.Separator();
        }

        private void SaveMappingsAndCreatePersistentClasses()
        {
            if (!Directory.Exists(Path.GetFullPath(RTSLPath.UserPrefabsPath)))
            {
                Directory.CreateDirectory(Path.GetFullPath(RTSLPath.UserPrefabsPath));
            }

            m_uoMapperGUI.SaveMappings();
            m_surrogatesMapperGUI.SaveMappings();
            CreatePersistentClasses();
        }

        private static Type[] GetAllTypes()
        {
            Type[] uoTypes;
            Assembly[] assemblies;
            Dictionary<string, HashSet<Type>> declaredIn;
            Type[] types;
            GetUOAssembliesAndTypes(out assemblies, out uoTypes);
            uoTypes = uoTypes.Union(new[] { typeof(RuntimePrefab), typeof(RuntimeScene) }).ToArray();
            GetSurrogateAssembliesAndTypes(assemblies, uoTypes, out declaredIn, out types);
            return uoTypes.Union(types).ToArray();
        }

        public static void GetUOAssembliesAndTypes(out Assembly[] assemblies, out Type[] types)
        {
            ReflectionUtils.GetUOAssembliesAndTypes(out assemblies, out types);

            List<Type> allUOTypes = new List<Type>(types);
            HashSet<Type> mustHaveTypes = PersistentClassMapperGUI.HideMustHaveTypes;
            foreach (Type mustHaveType in mustHaveTypes)
            {
                if (mustHaveType.IsSubclassOf(typeof(UnityObject)) && !allUOTypes.Contains(mustHaveType))
                {
                    allUOTypes.Add(mustHaveType);
                }
            }

            for (int i = 0; i < m_mostImportantUOTypes.Length; ++i)
            {
                allUOTypes.Remove(m_mostImportantUOTypes[i]);
            }

            types = m_mostImportantUOTypes.Union(allUOTypes.OrderBy(t => CodeGen.TypeName(t))).ToArray();
        }

        public static void GetSurrogateAssembliesAndTypes(Assembly[] assembiles, Type[] uoTypes, out Dictionary<string, HashSet<Type>> declaredIn, out Type[] types)
        {
            CodeGen.GetSurrogateAssembliesAndTypes(assembiles, uoTypes, out declaredIn, out types);

            List<Type> allTypes = new List<Type>(types);
            HashSet<Type> mustHaveTypes = PersistentClassMapperGUI.HideMustHaveTypes;
            foreach (Type mustHaveType in mustHaveTypes)
            {
                if (mustHaveType == typeof(UnityObject))
                {
                    continue;
                }
                if (!mustHaveType.IsSubclassOf(typeof(UnityObject)) && !allTypes.Contains(mustHaveType))
                {
                    allTypes.Add(mustHaveType);
                }
            }

            for (int i = 0; i < m_mostImportantSurrogateTypes.Length; ++i)
            {
                allTypes.Remove(m_mostImportantSurrogateTypes[i]);
            }

            allTypes.Add(typeof(UnityEventBase));
            types = m_mostImportantSurrogateTypes.Union(allTypes.OrderBy(t => t.Name)).ToArray();
        }

        private static void CreatePersistentClasses(PersistentClassMapping[] uoMappings, Dictionary<Type, PersistentTemplateInfo> uoTemplates, PersistentClassMapping[] surrogateMappings, Dictionary<Type, PersistentTemplateInfo> surrogateTemplates)
        {
            Dictionary<string, string> persistentFileTypeToPath = new Dictionary<string, string>();
            string scriptsAutoPath = Path.GetFullPath(RTSLPath.UserRoot);
            scriptsAutoPath = scriptsAutoPath + "/" + RTSLPath.ScriptsAutoFolder;
            if (Directory.Exists(scriptsAutoPath))
            {
                Directory.Delete(scriptsAutoPath, true);
            }

            Directory.CreateDirectory(scriptsAutoPath);
            string persistentClassesPath = scriptsAutoPath + "/" + RTSLPath.PersistentClassesFolder;
            if (!Directory.Exists(persistentClassesPath))
            {
                Directory.CreateDirectory(persistentClassesPath);
            }

            string scriptsPath = Path.GetFullPath(RTSLPath.UserRoot);
            string myPersistentClassesPath = scriptsPath + "/" + RTSLPath.PersistentCustomImplementationClasessFolder;
            if (!Directory.Exists(myPersistentClassesPath))
            {
                Directory.CreateDirectory(myPersistentClassesPath);
            }

            uoMappings = uoMappings.Where(m => Type.GetType(m.MappedAssemblyQualifiedName) != null).ToArray();
            surrogateMappings = surrogateMappings.Where(m => Type.GetType(m.MappedAssemblyQualifiedName) != null).ToArray();

            GameObject storageGO = (GameObject)AssetDatabase.LoadAssetAtPath(RTSLPath.FilePathStoragePath, typeof(GameObject));
            if (storageGO == null)
            {
                storageGO = new GameObject();
            }
            FilePathStorage filePathStorage = storageGO.GetComponent<FilePathStorage>();
            if (filePathStorage == null)
            {
                filePathStorage = storageGO.AddComponent<FilePathStorage>();
            }

            Dictionary<string, UnityObject> typeToScript = new Dictionary<string, UnityObject>();
            if (filePathStorage.PathRecords != null)
            {
                for (int i = 0; i < filePathStorage.PathRecords.Length; ++i)
                {
                    FilePathRecord record = filePathStorage.PathRecords[i];
                    if (record != null)
                    {
                        if (record.File != null && record.PeristentTypeName != null && !typeToScript.ContainsKey(record.PeristentTypeName))
                        {
                            typeToScript.Add(record.PeristentTypeName, record.File);
                        }
                    }
                }
            }

            HashSet<string> hideMustHaveTypes = new HashSet<string>(PersistentClassMapperGUI.HideMustHaveTypes.Select(t => PersistentClassMapperGUI.FullName(t)));
            CodeGen codeGen = new CodeGen();
            for (int i = 0; i < uoMappings.Length; ++i)
            {
                PersistentClassMapping mapping = uoMappings[i];
                if (mapping != null)
                {
                    if (!mapping.IsOn)
                    {
                        continue;
                    }

                    if (hideMustHaveTypes.Contains(mapping.MappedFullTypeName))
                    {
                        continue;
                    }

                    if (mapping.CreateCustomImplementation)
                    {
                        if (HasCustomImplementation(codeGen, mapping))
                        {
                            persistentFileTypeToPath.Add(mapping.PersistentFullTypeName, null);
                        }
                        else
                        {
                            persistentFileTypeToPath.Add(mapping.PersistentFullTypeName, GetCSFilePath(myPersistentClassesPath, mapping, typeToScript));
                        }
                    }
                    CreateCSFiles(persistentClassesPath, myPersistentClassesPath, codeGen, mapping, uoTemplates, typeToScript);

                }
            }

            for (int i = 0; i < surrogateMappings.Length; ++i)
            {
                PersistentClassMapping mapping = surrogateMappings[i];
                if (mapping != null)
                {
                    if (!mapping.IsOn)
                    {
                        continue;
                    }

                    if (hideMustHaveTypes.Contains(mapping.MappedFullTypeName))
                    {
                        continue;
                    }

                    if (mapping.CreateCustomImplementation)
                    {
                        if (HasCustomImplementation(codeGen, mapping))
                        {
                            persistentFileTypeToPath.Add(mapping.PersistentFullTypeName, null);
                        }
                        else
                        {
                            persistentFileTypeToPath.Add(mapping.PersistentFullTypeName, GetCSFilePath(myPersistentClassesPath, mapping, typeToScript));
                        }
                    }

                    CreateCSFiles(persistentClassesPath, myPersistentClassesPath, codeGen, mapping, surrogateTemplates, typeToScript);
                }
            }

            string typeModelCreatorCode = codeGen.CreateTypeModelCreator(uoMappings.Union(surrogateMappings).ToArray());
            File.WriteAllText(scriptsAutoPath + "/TypeModelCreator.cs", typeModelCreatorCode);

            string typeMapCode = codeGen.CreateTypeMapCreator(uoMappings.Union(surrogateMappings).ToArray());
            File.WriteAllText(scriptsAutoPath + "/TypeMapCreator.cs", typeMapCode);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Dictionary<string, FilePathRecord> typeNameToExistingRecord = filePathStorage.PathRecords != null ?
                filePathStorage.PathRecords.ToDictionary(r => r.PeristentTypeName) :
                new Dictionary<string, FilePathRecord>();

            List<FilePathRecord> records = new List<FilePathRecord>();
            foreach (string fullTypeName in persistentFileTypeToPath.Keys)
            {
                string filePath = persistentFileTypeToPath[fullTypeName];
                if (filePath != null)
                {
                    int indexOfAssets = filePath.IndexOf("Assets");

                    filePath = filePath.Substring(indexOfAssets);

                    UnityObject asset = AssetDatabase.LoadAssetAtPath<UnityObject>(filePath);

                    FilePathRecord record = new FilePathRecord
                    {
                        File = asset,
                        PeristentTypeName = fullTypeName
                    };
                    records.Add(record);
                }
                else
                {
                    FilePathRecord record;
                    if (typeNameToExistingRecord.TryGetValue(fullTypeName, out record))
                    {
                        if (record.File == null)
                        {
                            Debug.LogWarningFormat("FilePathRecord for type {0} is broken", fullTypeName);
                        }

                        records.Add(record);
                    }
                }
            }

            string storageGOPath = AssetDatabase.GetAssetPath(storageGO);
            if (string.IsNullOrEmpty(storageGOPath))
            {
                DestroyImmediate(storageGO, true);
            }
            else
            {
                AssetDatabase.DeleteAsset(storageGOPath);
            }

            storageGO = new GameObject();
            filePathStorage = storageGO.AddComponent<FilePathStorage>();
            filePathStorage.PathRecords = records.ToArray();

            if (!Directory.Exists(Path.GetFullPath(RTSLPath.UserPrefabsPath)))
            {
                Directory.CreateDirectory(Path.GetFullPath(RTSLPath.UserPrefabsPath));
            }

            PrefabUtility.SaveAsPrefabAsset(storageGO, RTSLPath.FilePathStoragePath);
            DestroyImmediate(storageGO);
        }

        private static Dictionary<Type, PersistentTemplateInfo> GetPersistentTemplates(Type[] allTypes)
        {
            Dictionary<Type, PersistentTemplateInfo> templates = new Dictionary<Type, PersistentTemplateInfo>();
            Dictionary<string, Type> typeNameToType = allTypes.ToDictionary(t => PersistentClassMapperGUI.FullName(t));
            Type[] templateTypes = Assembly.GetAssembly(typeof(PersistentClassMapperWindow)).GetTypes().Where(t => t.GetCustomAttributes(typeof(PersistentTemplateAttribute), false).Length != 0).ToArray();
            for (int i = 0; i < templateTypes.Length; ++i)
            {
                Type templateType = templateTypes[i];
                PersistentTemplateAttribute templateAttrib = (PersistentTemplateAttribute)templateType.GetCustomAttributes(typeof(PersistentTemplateAttribute), false)[0];

                Type mappedType;
                if (typeNameToType.TryGetValue(templateAttrib.ForType, out mappedType))
                {
                    MonoScript monoScript = MonoScript.FromScriptableObject(CreateInstance(templateType));
                    if (monoScript == null)
                    {
                        Debug.LogWarning("Unable to find MonoScript for " + templateAttrib.ForType + ". Make sure file and type have same names");
                        continue;
                    }
                    string contents = monoScript.text;
                    string usings = contents.ToString();
                    string interfaces = contents.ToString();
                    if (CodeGen.TryGetTemplateBody(contents, out contents) && CodeGen.TryGetTemplateUsings(usings, out usings))
                    {
                        CodeGen.TryGetTemplateInterfaces(interfaces, out interfaces);
                        PersistentTemplateInfo templateInfo = new PersistentTemplateInfo
                        {
                            Body = contents,
                            FieldNames = new HashSet<string>(),
                            RequiredTypes = new HashSet<Type>(),
                            Usings = usings,
                            Interfaces = interfaces,
                            Path = AssetDatabase.GetAssetPath(monoScript),
                        };

                        if (templateAttrib.FieldNames != null)
                        {
                            for (int n = 0; n < templateAttrib.FieldNames.Length; ++n)
                            {
                                string fieldName = templateAttrib.FieldNames[n];
                                if (!templateInfo.FieldNames.Contains(fieldName))
                                {
                                    templateInfo.FieldNames.Add(fieldName);
                                }
                            }
                        }

                        if (templateAttrib.RequiredTypes != null)
                        {
                            for (int n = 0; n < templateAttrib.RequiredTypes.Length; ++n)
                            {
                                Type requiredType;
                                if (typeNameToType.TryGetValue(templateAttrib.RequiredTypes[n], out requiredType) && !templateInfo.RequiredTypes.Contains(requiredType))
                                {
                                    templateInfo.RequiredTypes.Add(requiredType);
                                }
                            }
                        }

                        if (templates.ContainsKey(mappedType))
                        {
                            Debug.LogWarning("m_templates dictionary already contains " + PersistentClassMapperGUI.FullName(mappedType));
                        }
                        else
                        {
                            templates.Add(mappedType, templateInfo);
                        }
                    }
                    else
                    {
                        string path = AssetDatabase.GetAssetPath(monoScript);
                        Debug.LogWarningFormat("Template {0} has invalid format", path);
                    }
                }
            }

            return templates;
        }

        private static bool HasCustomImplementation(CodeGen codeGen, PersistentClassMapping mapping)
        {
            Type persistentType = codeGen.GetPersistentType(mapping.PersistentFullTypeName);
            return persistentType != null && persistentType.GetCustomAttributes(typeof(CustomImplementationAttribute), false).Length > 0;
        }

        private static void CreateCSFiles(string persistentClassesPath, string myPersistentClassesPath, CodeGen codeGen, PersistentClassMapping mapping, Dictionary<Type, PersistentTemplateInfo> templates, Dictionary<string, UnityObject> typeToScript)
        {
            string code = codeGen.CreatePersistentClass(mapping);
            CreateCSFile(persistentClassesPath, mapping, code, null);

            if (mapping.CreateCustomImplementation)
            {
                if (!HasCustomImplementation(codeGen, mapping))
                {
                    PersistentTemplateInfo template;
                    Type mappedType = Type.GetType(mapping.MappedAssemblyQualifiedName);
                    if (mappedType == null || !templates.TryGetValue(mappedType, out template))
                    {
                        template = null;
                    }
                    string customCode = codeGen.CreatePersistentClassCustomImplementation(mapping.PersistentNamespace, mapping.PersistentTypeName, template);
                    CreateCSFile(myPersistentClassesPath, mapping, customCode, typeToScript);
                }
            }
        }

        private void OnUOTypeLocked(Type obj)
        {
            m_surrogatesMapperGUI.LockType(obj);
        }

        private void OnUOTypeUnlocked(Type obj)
        {
            m_surrogatesMapperGUI.UnlockType(obj);
        }

        private void OnSurrogateTypeLocked(Type obj)
        {
            m_uoMapperGUI.LockType(obj);
        }

        private void OnSurrogateTypeUnlocked(Type obj)
        {
            m_uoMapperGUI.UnlockType(obj);
        }

        private static void CreateCSFile(string persistentClassesPath, PersistentClassMapping mapping, string code, Dictionary<string, UnityObject> typeToScript)
        {
            File.WriteAllText(GetCSFilePath(persistentClassesPath, mapping, typeToScript), code);
        }

        private static string GetCSFilePath(string persistentClassesPath, PersistentClassMapping mapping, Dictionary<string, UnityObject> typeToScript)
        {
            UnityObject file;
            if (typeToScript != null && typeToScript.TryGetValue(mapping.PersistentFullTypeName, out file))
            {
                return AssetDatabase.GetAssetPath(file);
            }

            string path = persistentClassesPath + "/" + mapping.PersistentFullTypeName.Replace(".", "_").Replace("+", "_") + ".cs";
            return path;
        }
    }
}
