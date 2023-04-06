using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSL.Internal
{
    public class PersistentMappingsExporterWindow : EditorWindow
    {
        private string m_filterText;
        private Vector2 m_scrollViewPosition;
        private List<PersistentClassMapping> m_mappings;
        private bool[] m_selection;
        private int[] m_indices;

        private void Initialize()
        {
            if (m_mappings != null)
            {
                return;
            }

            m_mappings = new List<PersistentClassMapping>();

            PersistentClassMapping[] mappings = MappingsUtility.GetAllMappings();
            for (int i = 0; i < mappings.Length; ++i)
            {
                PersistentClassMapping classMapping = mappings[i];
                if (string.IsNullOrEmpty(classMapping.Version))
                {
                    classMapping.MappedTypeName = MappingsUtility.FixTypeName(classMapping.MappedTypeName);
                    classMapping.PersistentTypeName = MappingsUtility.FixTypeName(classMapping.PersistentTypeName);
                    classMapping.PersistentBaseTypeName = MappingsUtility.FixTypeName(classMapping.PersistentBaseTypeName);
                }

                if (classMapping.IsOn && Type.GetType(classMapping.MappedAssemblyQualifiedName) != null)
                {
                    m_mappings.Add(classMapping);
                }
            }

            m_mappings.Sort((m1, m2) => m1.MappedFullTypeName.CompareTo(m2.MappedFullTypeName));
            m_selection = new bool[m_mappings.Count];
            m_indices = new int[m_mappings.Count];
            for (int i = 0; i < m_mappings.Count; ++i)
            {
                m_indices[i] = i;
            }
        }

        private void ApplyFilter()
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < m_mappings.Count; ++i)
            {
                PersistentClassMapping mapping = m_mappings[i];

                if (string.IsNullOrWhiteSpace(m_filterText) || mapping.MappedFullTypeName.ToLower().Contains(m_filterText.ToLower()))
                {
                    indices.Add(i);
                }
            }
            m_indices = indices.ToArray();
        }

        public string EditorGUILayoutTextField(string header, string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(header, GUILayout.ExpandWidth(false), GUILayout.MinWidth(145));
            text = GUILayout.TextField(text, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            return text;
        }

        private void OnGUI()
        {
            Initialize();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Separator();

            m_filterText = EditorGUILayoutTextField("Filter:", m_filterText);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyFilter();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            m_scrollViewPosition = EditorGUILayout.BeginScrollView(m_scrollViewPosition);
            EditorGUILayout.BeginVertical();
            {
                for (int i = 0; i < m_indices.Length; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        int index = m_indices[i];
                        m_selection[index] = EditorGUILayout.Toggle(m_selection[index], GUILayout.MaxWidth(15));
                        EditorGUILayout.LabelField(m_mappings[index].MappedFullTypeName);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            DropAreaGUI();
            Buttons();
            EditorGUILayout.Separator();
        }

        private void Buttons()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            GUILayout.Button("Select All", GUILayout.Height(20));
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < m_indices.Length; ++i)
                {
                    m_selection[m_indices[i]] = true;
                }
            }
            EditorGUI.BeginChangeCheck();
            GUILayout.Button("Unselect All", GUILayout.Height(20));
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < m_indices.Length; ++i)
                {
                    m_selection[m_indices[i]] = false;
                }
            }
            EditorGUI.BeginChangeCheck();
            GUILayout.Button("Export", GUILayout.Height(20));
            if (EditorGUI.EndChangeCheck())
            {
                string packagePath = EditorUtility.SaveFilePanel("Export Mappings", "", "Mappings", "unitypackage");
                if (!string.IsNullOrEmpty(packagePath))
                {
                    Debug.Log("Export to " + packagePath);

                    List<PersistentClassMapping> classMappings = new List<PersistentClassMapping>();
                    List<PersistentClassMapping> surrogateMappings = new List<PersistentClassMapping>();
                    for (int i = 0; i < m_selection.Length; ++i)
                    {
                        if (m_selection[i])
                        {
                            if (typeof(UnityEngine.Object).IsAssignableFrom(Type.GetType(m_mappings[i].MappedAssemblyQualifiedName)))
                            {
                                classMappings.Add(m_mappings[i]);
                            }
                            else
                            {
                                surrogateMappings.Add(m_mappings[i]);
                            }
                        }
                    }

                    string mappingsPath = Application.dataPath + "/Mappings.txt";
                    MappingsUtility.Export(mappingsPath, classMappings.ToArray(), surrogateMappings.ToArray());
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                    m_assetsPath.Add("Assets/Mappings.txt");

                    AssetDatabase.ExportPackage(m_assetsPath.ToArray(), packagePath, ExportPackageOptions.Default);
                    AssetDatabase.DeleteAsset("Assets/Mappings.txt");

                    m_dropAreaText = string.Empty;
                    m_assetsPath.Clear();

                }
            }

            EditorGUILayout.EndHorizontal();
        }


        private HashSet<string> m_assetsPath = new HashSet<string>();
        private string m_dropAreaText = string.Empty;
        public void DropAreaGUI()
        {
            Event evt = Event.current;

            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));

            if(m_assetsPath.Count == 0)
            {
                GUI.Box(drop_area, "drag & drop additional .cs files to export");
            }
            else
            {
                GUI.Box(drop_area, m_dropAreaText);
            }

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            string path = AssetDatabase.GetAssetPath(dragged_object);
                            if(path.EndsWith(".cs") && m_assetsPath.Add(path))
                            {
                                m_dropAreaText += Path.GetFileName(path) + "; ";
                            }
                        }
                    }
                    break;
            }

        }
    }

}
