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
    /// A struct containing an ID and a name.
    /// </summary>
    [Serializable]
    public struct NameID : System.IComparable<NameID>
    {
        [Tooltip("The ID matched to the name.")]
        [SerializeField] private uint m_ID;
        [Tooltip("The name matched to the ID.")]
        [SerializeField] private string m_Name;

        public uint ID => m_ID;
        public string Name => m_Name;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="name">The name.</param>
        public NameID(uint id, string name)
        {
            m_ID = id;
            m_Name = name;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="id">The ID.</param>
        public NameID(string name, uint id)
        {
            m_ID = id;
            m_Name = name;
        }

        /// <summary>
        /// Compares the current NameID to the other NameID.
        /// </summary>
        /// <param name="other">The other NameID.</param>
        /// <returns>The difference between the current NameID and the other NameID.</returns>
        public int CompareTo(NameID other)
        {
            return (int)ID - (int)other.ID;
        }

        /// <summary>
        /// Overrides the ToString method.
        /// </summary>
        /// <returns>The returned string value.</returns>
        public override string ToString()
        {
            // The name must be included, such that the searchable window can allow users to search by name too.
            return ID.ToString() + $" ({m_Name})";
        }
    }

    /// <summary>
    /// An object containing an array of NameIDs. Used with the StringMapper Editor window to organize IDs.
    /// Both names and IDs should be unique. The class does not prevent duplicate names or IDs but some functions are available to find duplicate names and IDs.
    /// </summary>
    [CreateAssetMenu(menuName = "Opsive/Ultimate Character Controller/Utility/NameIDMap", fileName = "NameIDMap", order = 31)]
    public class NameIDMap : DataMap<NameID>
    {
        /// <summary>
        /// Specifies how the list should be sorted.
        /// </summary>
        public enum SortOption
        {
            ByIDAscending,      // Sort by ID from 0 to n.
            ByIDDescending,     // Sort by ID from n to 0.
            ByNameAscending,    // Sort by name from A to Z.
            ByNameDescending,   // Sort by name from Z to A.
        }

        [Tooltip("Specifies the sorting order.")]
        [HideInInspector] [SerializeField] protected SortOption m_SortOrder;

        private HashSet<uint> m_IDSet = new HashSet<uint>();
        private HashSet<string> m_NameSet = new HashSet<string>();

        public SortOption SortOrder
        {
            get => m_SortOrder;
            set {
                m_SortOrder = value;
                OnValidate();
            }
        }

        /// <summary>
        /// Sorts the name IDs by the specified sorting order.
        /// </summary>
        protected override void SortAllObjects()
        {
            switch (m_SortOrder) {
                case SortOption.ByIDAscending:
                    Array.Sort(EditableObjects, IDComparison(true));
                    Array.Sort(ReadOnlyObjects, IDComparison(true));
                    break;
                case SortOption.ByIDDescending:
                    Array.Sort(EditableObjects, IDComparison(false));
                    Array.Sort(ReadOnlyObjects, IDComparison(false));
                    break;
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
        protected override NameID ParseLine(string line)
        {
            var rawLineArray = line.Split(',');

            if (rawLineArray.Length == 0) {
                return new NameID();
            }

            if (uint.TryParse(rawLineArray[0], out var id) == false) {
                id = 0;
            }

            var newNameID = new NameID();
            if (rawLineArray.Length == 1) {
                newNameID = new NameID(id, "");
            } else if (rawLineArray.Length > 1) {
                newNameID = new NameID(id, rawLineArray[1].Trim());
            }
            return newNameID;
        }

        /// <summary>
        /// Returns the name matching the ID.
        /// </summary>
        /// <param name="id">The ID to search for.</param>
        /// <returns>The name found (null if not found).</returns>
        public string GetName(uint id)
        {
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (m_AllObjects[i].ID == id) {
                    return m_AllObjects[i].Name;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the ID matching the name.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>The ID matching the name (-1 if not found).</returns>
        public int GetID(string name)
        {
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (m_AllObjects[i].Name == name) {
                    return (int)m_AllObjects[i].ID;
                }
            }

            return -1;
        }

        /// <summary>
        /// Adds a new object with the string value.
        /// </summary>
        /// <param name="name">The name of the string.</param>
        public override void AddName(string name)
        {
            // Get a new valid ID.
            Add(NextNewValidNameID(name));
        }

        /// <summary>
        /// Checks if the name and IDs in the array are all unique.
        /// </summary>
        /// <returns>Returns true if all names and IDs are unique.</returns>
        public override bool IsDataValid()
        {
            var idHashSet = GetEmptyIDHashSet();
            var nameHashSet = GetEmptyNameHashSet();
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (!idHashSet.Add(m_AllObjects[i].ID)) {
                    return false;
                }
                if (!nameHashSet.Add(m_AllObjects[i].Name)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if the specified name is valid within the mapping.
        /// </summary>
        /// <returns>True if the specified name is valid within the mapping.</returns>
        public override bool IsNameValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Contains(',')) { // The mapping is saved with a csv so the name cannot contain a comma.
                return false;
            }

            name = name.ToLowerInvariant();
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (m_AllObjects[i].Name.ToLowerInvariant() == name) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the specified id is valid within the mapping.
        /// </summary>
        /// <returns>True if the specified id is valid within the mapping.</returns>
        public bool IsIDValid(uint id)
        {
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (m_AllObjects[i].ID == id) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a empty ID hash set.
        /// </summary>
        /// <returns>A empty ID hash set.</returns>
        private HashSet<uint> GetEmptyIDHashSet()
        {
            if (m_IDSet == null) {
                m_IDSet = new HashSet<uint>();
            } else {
                m_IDSet.Clear();
            }
            return m_IDSet;
        }

        /// <summary>
        /// Returns a empty name hash set.
        /// </summary>
        /// <returns>A empty name hash set.</returns>
        private HashSet<string> GetEmptyNameHashSet()
        {
            if (m_NameSet == null) {
                m_NameSet = new HashSet<string>();
            } else {
                m_NameSet.Clear();
            }
            return m_NameSet;
        }

        /// <summary>
        /// Get the names and IDs that have duplicates in the map array.
        /// </summary>
        /// <param name="duplicateIDs">The result of duplicate IDs found.</param>
        /// <param name="duplicateNames">The result of duplicate names found.</param>
        public void GetDuplicateIDsAndNames(List<uint> duplicateIDs, List<string> duplicateNames)
        {
            duplicateIDs.Clear();
            duplicateNames.Clear();
            var idHashSet = GetEmptyIDHashSet();
            var nameHashSet = GetEmptyNameHashSet();
            for (int i = 0; i < m_AllObjects.Length; i++) {
                var id = m_AllObjects[i].ID;
                if (idHashSet.Add(id) == false) {
                    if (!duplicateIDs.Contains(id)) {
                        duplicateIDs.Add(id);
                    }
                }

                var name = m_AllObjects[i].Name;
                if (nameHashSet.Add(name) == false) {
                    if (!duplicateNames.Contains(name)) {
                        duplicateNames.Add(name);
                    }
                }
            }
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
        /// Order the mapping by ID.
        /// </summary>
        /// <param name="ascending">Is the sort ascending?</param>
        public void OrderEditableById(bool ascending)
        {
            Array.Sort(m_EditableObjects, IDComparison(ascending));
            WriteEditableArrayToFile();
        }

        /// <summary>
        /// Compare the IDs.
        /// </summary>
        /// <param name="ascending">An ascending comparison?</param>
        /// <returns>The comparison for IDs.</returns>
        public Comparison<NameID> IDComparison(bool ascending)
        {
            return ascending ? (x, y) => x.ID.CompareTo(y.ID) : (x, y) => y.ID.CompareTo(x.ID);
        }

        /// <summary>
        /// Compare the names.
        /// </summary>
        /// <param name="ascending">Is the comparison ascending?</param>
        /// <returns>The comparison for names.</returns>
        public Comparison<NameID> NameComparison(bool ascending)
        {
            return ascending
                ? (x, y) => String.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase)
                : (x, y) => String.Compare(y.Name, x.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the string value of the name.
        /// </summary>
        /// <param name="obj">The interested object.</param>
        /// <param name="csvString">Should the csv string be returned?</param>
        /// <returns>The string value of the name.</returns>
        public override string GetStringValue(NameID obj, bool csvString)
        {
            if (csvString) {
                return obj.ID + "," + obj.Name;
            }
            return String.Format("{0} ({1})", obj.ID, obj.Name);
        }

        /// <summary>
        /// Does the Id match a name ids in the map.
        /// </summary>
        /// <param name="id">The id to search for.</param>
        /// <returns>True if the id matches a name ids in the map.</returns>
        public bool HasID(uint id)
        {
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (id == m_AllObjects[i].ID) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the name match a name IDs in the map.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>True if the name matches a name IDs in the map.</returns>
        public bool HasName(string name)
        {
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (name == m_AllObjects[i].Name) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the ID match two or more name IDs in the map.
        /// </summary>
        /// <param name="id">The ID to search for.</param>
        /// <returns>True if the ID matches two or more name IDs in the map.</returns>
        public bool HasDuplicateID(uint id)
        {
            var count = 0;
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (id == m_AllObjects[i].ID) {
                    if (count == 1) {
                        return true;
                    }

                    count++;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the name match two or more name IDs in the map.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <returns>True if the name matches two or more name IDs in the map.</returns>
        public bool HasDuplicateName(string name)
        {
            var count = 0;
            for (int i = 0; i < m_AllObjects.Length; i++) {
                if (name == m_AllObjects[i].Name) {
                    if (count == 1) {
                        return true;
                    }

                    count++;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a new Name ID that is valid such that it can be added in the map without creating a duplicate.
        /// </summary>
        /// <param name="baseName">The base name to start with, then a counter is added to it.</param>
        /// <param name="startID">The counter starter (default 10000, everything under is used by Opsive demos).</param>
        /// <returns>The new and valid name ID.</returns>
        public NameID NextNewValidNameID(string baseName = "New ID", uint startID = 10000)
        {
            // Starting at 10000, everything under is used by Opsive demos.
            var id = NextNewValidID(startID);
            var name = NextNewValidName(baseName);
            return new NameID(name, id);
        }

        /// <summary>
        /// Get a name that does not exist in the map.
        /// </summary>
        /// <param name="baseName">The base name to start with, then a counter is added to it.</param>
        /// <returns>A valid name.</returns>
        public string NextNewValidName(string baseName = "New ID")
        {
            var name = baseName;
            var count = 0;
            while (HasName(name)) {
                count++;
                name = baseName + " " + count;
            }
            return name;
        }

        /// <summary>
        /// Get an ID that does not exist in the map.
        /// </summary>
        /// <param name="startID">The counter starter (default 10000, everything under is used by Opsive demos).</param>
        /// <returns>A valid ID.</returns>
        public uint NextNewValidID(uint startID = 10000)
        {
            var id = startID;
            while (HasID(id)) { id++; }
            return id;
        }
    }
}