/// ---------------------------------------------
/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

    /// <summary>
    /// Base class for files that contain a mapping and write to a csv file.
    /// </summary>
    public abstract class DataMap<T> : ScriptableObject 
    {
        [Tooltip("The file that contains the mappings that can be edited.")]
        [SerializeField] protected TextAsset m_EditableFile;
        [Tooltip("The file that contains the mappings that cannot be edited.")]
        [SerializeField] protected TextAsset[] m_ReadOnlyFiles = Array.Empty<TextAsset>();

        protected T[] m_EditableObjects;
        protected T[] m_ReadOnlyObjects;
        protected T[] m_AllObjects;

        public T[] EditableObjects
        {
            get => m_EditableObjects;
            set {
                m_EditableObjects = value;

                if (m_EditableObjects == null) {
                    m_EditableObjects = Array.Empty<T>();
                }

                WriteEditableArrayToFile();
            }
        }
        public T[] ReadOnlyObjects { get => m_ReadOnlyObjects; }
        public T[] AllObjects
        {
            get {
                if (m_AllObjects == null) {
                    OnValidate();
                }
                return m_AllObjects;
            }
        }

        public event Action OnValidateEvent;

        /// <summary>
        /// Sorts the objects by the specified sorting order.
        /// </summary>
        protected abstract void SortAllObjects();

        /// <summary>
        /// Parses the input line to the template type.
        /// </summary>
        /// <param name="line">The line that should be parsed.</param>
        /// <returns>The resulting object.</returns>
        protected abstract T ParseLine(string line);

        /// <summary>
        /// Adds a new object with the string value.
        /// </summary>
        /// <param name="name">The name of the string.</param>
        public abstract void AddName(string name);

        /// <summary>
        /// Checks if the objects in the array are all unique.
        /// </summary>
        /// <returns>Returns true if all objects are unique.</returns>
        public abstract bool IsDataValid();

        /// <summary>
        /// Returns true if the specified name is valid within the mapping.
        /// </summary>
        /// <returns>True if the specified name is valid within the mapping.</returns>
        public abstract bool IsNameValid(string name);

        /// <summary>
        /// Returns the string value of the object.
        /// </summary>
        /// <param name="obj">The interested object.</param>
        /// <param name="csvString">Should the csv string be returned?</param>
        /// <returns>The string value of the object.</returns>
        public abstract string GetStringValue(T obj, bool csvString);

        /// <summary>
        /// Validates the objects.
        /// </summary>
        public virtual void OnValidate()
        {
            if (m_AllObjects == null) {
                m_EditableObjects = Array.Empty<T>();
            }

            if (m_EditableObjects == null) {
                m_EditableObjects = Array.Empty<T>();
            }

            if (m_ReadOnlyObjects == null) {
                m_ReadOnlyObjects = Array.Empty<T>();
            }

            LoadObjectsFromFile(m_EditableFile, ref m_EditableObjects);
            LoadObjectsFromFiles(m_ReadOnlyFiles, ref m_ReadOnlyObjects);

            SortAllObjects();

            m_AllObjects = new T[m_EditableObjects.Length + m_ReadOnlyObjects.Length];
            for (int i = 0; i < m_EditableObjects.Length; i++) {
                m_AllObjects[i] = m_EditableObjects[i];
            }
            for (int i = 0; i < m_ReadOnlyObjects.Length; i++) {
                m_AllObjects[i + m_EditableObjects.Length] = m_ReadOnlyObjects[i];
            }

            OnValidateEvent?.Invoke();
        }

        /// <summary>
        /// Loads the objects from multiple files.
        /// </summary>
        /// <param name="files">The files to read.</param>
        /// <param name="array">The array of objects resulting from those files.</param>
        public void LoadObjectsFromFiles(IReadOnlyList<TextAsset> files, ref T[] array)
        {
            if (files == null) { return; }

            var list = new List<T>();
            for (int i = 0; i < files.Count; i++) {
                GetObjectsFromFile(files[i], list);
            }

            array = list.ToArray();
        }

        /// <summary>
        /// Loads the objects from a single file.
        /// </summary>
        /// <param name="file">The file to get the objects from.</param>
        /// <param name="array">The array of objects retrieved from the file.</param>
        public void LoadObjectsFromFile(TextAsset file, ref T[] array)
        {
            var list = new List<T>();
            GetObjectsFromFile(file, list);
            array = list.ToArray();
        }

        /// <summary>
        /// Gets the objects from a file.
        /// </summary>
        /// <param name="file">The file to get the objects from.</param>
        /// <param name="objects">The list to add the objects to.</param>
        /// <returns>The number of objects added.</returns>
        public int GetObjectsFromFile(TextAsset file, List<T> objects)
        {
            if (file == null) { return 0; }

            var count = 0;
            var lines = file.text.Split(new[] { Environment.NewLine, "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++) {
                var obj = ParseLine(lines[i]);
                if (obj != null) {
                    objects.Add(obj);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Adds the objects to the map.
        /// </summary>
        /// <param name="obj">The value to add.</param>
        public void Add(T obj)
        {
#if UNITY_EDITOR
            if (m_EditableFile == null) {
                CreateEditableFile();
                if (m_EditableFile == null) {
                    return;
                }
            }

            var newLine = Environment.NewLine + GetStringValue(obj, true);
            var assetPath = AssetDatabase.GetAssetPath(m_EditableFile);
            File.WriteAllText(assetPath, m_EditableFile.text + newLine);
            EditorUtility.SetDirty(m_EditableFile);
            AssetDatabase.SaveAssetIfDirty(m_EditableFile);
            AssetDatabase.ImportAsset(assetPath);

            OnValidate();
#else
            Debug.LogError("Error: DataMaps cannot be created at runtime.");
#endif
        }

        /// <summary>
        /// Removes the object at the specified index within the editable file.
        /// </summary>
        /// <param name="index">The index of the object that should be removed.</param>
        public void Remove(int index)
        {
#if UNITY_EDITOR
            if (m_EditableObjects == null || index >= m_EditableObjects.Length) {
                return;
            }

            ArrayUtility.RemoveAt(ref m_EditableObjects, index);
            var editableText = string.Empty;
            for (int i = 0; i < m_EditableObjects.Length; ++i) {
                editableText += (i > 0 ? Environment.NewLine : string.Empty) + GetStringValue(m_EditableObjects[i], true);
            }
            var assetPath = AssetDatabase.GetAssetPath(m_EditableFile);
            File.WriteAllText(assetPath, editableText);
            EditorUtility.SetDirty(m_EditableFile);
            AssetDatabase.SaveAssetIfDirty(m_EditableFile);
            AssetDatabase.ImportAsset(assetPath);
            OnValidate();
#else
            Debug.LogError("Error: DataMaps cannot be removed at runtime.");
#endif
        }

        /// <summary>
        /// Saves a new csv file with the editable objects.
        /// </summary>
        protected void CreateEditableFile()
        {
#if UNITY_EDITOR
            var path = EditorUtility.SaveFilePanelInProject("Save File", this.name + "_Editable", "csv",
                                                                "Please enter a file name to save the mapping to.");
            if (path.Length != 0) {
                var streamWriter = File.CreateText(path);
                streamWriter.Close();

                AssetDatabase.Refresh();
                m_EditableFile = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
                EditorUtility.SetDirty(m_EditableFile);
                EditorUtility.SetDirty(this);
            }
#else
            Debug.LogError("Error: DataMaps cannot be created at runtime.");
#endif
        }

        /// <summary>
        /// Write the editable objects to an editable file.
        /// </summary>
        public void WriteEditableArrayToFile()
        {
#if UNITY_EDITOR
            if (m_EditableFile == null) {
                CreateEditableFile();
                if (m_EditableFile == null) {
                    return;
                }
            }

            var lines = string.Empty;
            for (int i = 0; i < m_EditableObjects.Length; i++) {
                lines += GetStringValue(m_EditableObjects[i], true) + Environment.NewLine;
            }

            var assetPath = AssetDatabase.GetAssetPath(m_EditableFile);
            File.WriteAllText(assetPath, lines);
            EditorUtility.SetDirty(m_EditableFile);
            AssetDatabase.ImportAsset(assetPath);
#else
            Debug.LogError("Error: DataMaps cannot be created at runtime.");
#endif
        }
    }
}