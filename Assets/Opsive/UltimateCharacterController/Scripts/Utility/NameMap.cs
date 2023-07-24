/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Utility
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An object containing an array of strings. Used with the StringMapper Editor window to organize IDs.
    /// Both names and IDs should be unique. The class does not prevent duplicate names or IDs but some functions are available to find duplicate names and IDs.
    /// </summary>
    [CreateAssetMenu(menuName = "Opsive/Ultimate Character Controller/Utility/NameMap", fileName = "NameMap", order = 30)]
    public class NameMap : DataMap<string>
    {
        /// <summary>
        /// Specifies how the list should be sorted.
        /// </summary>
        public enum SortOption
        {
            ByNameAscending,    // Sort by name from A to Z.
            ByNameDescending,   // Sort by name from Z to A.
        }

        [Tooltip("Specifies the sorting order.")]
        [HideInInspector] [SerializeField] protected SortOption m_SortOrder;

        public SortOption SortOrder
        {
            get => m_SortOrder;
            set {
                m_SortOrder = value;
                OnValidate();
            }
        }

        /// <summary>
        /// Sorts the strings by the specified sorting order.
        /// </summary>
        protected override void SortAllObjects()
        {
            switch (m_SortOrder) {
                case SortOption.ByNameAscending:
                    Array.Sort(EditableObjects, NameComparison(true));
                    Array.Sort(ReadOnlyObjects, NameComparison(true));
                    break;
                case SortOption.ByNameDescending:
                    Array.Sort(EditableObjects, NameComparison(false));
                    Array.Sort(ReadOnlyObjects, NameComparison(false));
                    break;
            }
        }

        /// <summary>
        /// Parses the input line to the template type.
        /// </summary>
        /// <param name="line">The line that should be parsed.</param>
        /// <returns>The resulting object.</returns>
        protected override string ParseLine(string line)
        {
            var rawLineArray = line.Split(',');

            if (rawLineArray.Length == 0) {
                return String.Empty;
            }

            return rawLineArray[0];
        }

        /// <summary>
        /// Adds a new object with the string value.
        /// </summary>
        /// <param name="name">The name of the string.</param>
        public override void AddName(string name)
        {
            Add(NextNewValidName(name));
        }

        /// <summary>
        /// Checks if the objects in the array are all unique.
        /// </summary>
        /// <returns>Returns true if all objects are unique.</returns>
        public override bool IsDataValid()
        {
            var nameHashSet = new HashSet<string>();
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (!nameHashSet.Add(m_AllObjects[i])) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the names that have duplicates in the map array.
        /// </summary>
        /// <param name="duplicateNames">The result of duplicate names found.</param>
        public void GetDuplicateNames(List<string> duplicateNames)
        {
            duplicateNames.Clear();
            var nameHashSet = new HashSet<string>();
            for (int i = 0; i < m_AllObjects.Length; i++) {
                var name = m_AllObjects[i];
                if (!nameHashSet.Add(name)) {
                    if (!duplicateNames.Contains(name)) {
                        duplicateNames.Add(name);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the name is valid if it was added to the map array.
        /// </summary>
        /// <param name="name">The name to check if it is valid.</param>
        /// <returns>True if no existing objects have the same name.</returns>
        public override bool IsNameValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Contains(',')) { // The mapping is saved with a csv so the name cannot contain a comma.
                return false;
            }

            name = name.ToLowerInvariant();
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (m_AllObjects[i].ToLowerInvariant() == name) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Order the mapping by name.
        /// </summary>
        /// <param name="ascending">Is the sort ascending?</param>
        public void OrderEditableByName(bool ascending)
        {
            Array.Sort(m_EditableObjects, NameComparison(ascending));
            WriteEditableArrayToFile();
        }

        /// <summary>
        /// Compare the names.
        /// </summary>
        /// <param name="ascending">Is the comparison ascending?</param>
        /// <returns>The comparison for names.</returns>
        public Comparison<string> NameComparison(bool ascending)
        {
            return ascending
                ? (x, y) => String.Compare(x, y, StringComparison.OrdinalIgnoreCase)
                : (x, y) => String.Compare(y, x, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the string value of the name.
        /// </summary>
        /// <param name="obj">The interested object.</param>
        /// <param name="csvString">Should the csv string be returned?</param>
        /// <returns>The string value of the name.</returns>
        public override string GetStringValue(string obj, bool csvString)
        {
            return obj;
        }

        /// <summary>
        /// Get a name that does not exist in the map.
        /// </summary>
        /// <param name="baseName">The base name to start with, then a counter is added to it.</param>
        /// <returns>A valid name.</returns>
        public string NextNewValidName(string baseName = "New Name")
        {
            var name = baseName;
            var count = 0;
            while (!IsNameValid(name)) {
                count++;
                name = baseName + " " + count;
            }
            return name;
        }
    }
}