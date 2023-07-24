/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Utility
{
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    /// <summary>
    /// The NameIDMapInspector implements the DataMapInspector for the NameID object.
    /// </summary>
    public class NameIDMapInspector : DataMapInspector<NameID>
    {
        private static List<string> s_OrderByList = new List<string>() { "ID Ascending", "ID Descending", "Name Ascending", "Name Descending" };

        private NameIDMap m_NameIDMap;
        private List<uint> m_DuplicateIDs;
        private List<string> m_DuplicateNames;

        protected override string ReorderableListTitle => "Name IDs";

        /// <summary>
        /// Performs any initialization.
        /// </summary>
        protected override void InitializeInspector()
        {
            base.InitializeInspector();

            m_NameIDMap = m_DataMap as NameIDMap;
        }

        /// <summary>
        /// Returns a new ListElement.
        /// </summary>
        /// <param name="onChange">Callback when the element has changed.</param>
        /// <returns>A new ListElement.</returns>
        protected override ListElement GetListElement(Action<int, NameID> onChange)
        {
            return new NameIDMapListElement(this, onChange);
        }

        /// <summary>
        /// Shows the sorting elements.
        /// </summary>
        /// <param name="container">The parent VisualElement.</param>
        protected override void ShowSortingElements(VisualElement container)
        {
            var orderByDropDown = new DropdownField("Order By", s_OrderByList, (int)m_NameIDMap.SortOrder, (choice) =>
            {
                return s_OrderByList[(int)m_NameIDMap.SortOrder];
            });
            orderByDropDown.RegisterValueChangedCallback(evt =>
            {
                var index = s_OrderByList.IndexOf(evt.newValue);
                m_NameIDMap.SortOrder = (NameIDMap.SortOption)index; // Specifying a new sort order will sort the objects.
                orderByDropDown.SetValueWithoutNotify(s_OrderByList[index]);
                Refresh();
            });

            m_DuplicateIDs = new List<uint>();
            m_DuplicateNames = new List<string>();
            container.Add(orderByDropDown);
        }

        /// <summary>
        /// Refreshes the objects.
        /// </summary>
        protected override void DoRefresh()
        {
            m_DuplicateIDs.Clear();
            m_DuplicateNames.Clear();

            if (!m_NameIDMap.IsDataValid()) {
                m_NameIDMap.GetDuplicateIDsAndNames(m_DuplicateIDs, m_DuplicateNames);
            }
        }

        /// <summary>
        /// VisualElement for a row within the ReorderableList.
        /// </summary>
        public class NameIDMapListElement : ListElement
        {
            private IntegerField m_IDField;
            private TextField m_NameField;

            /// <summary>
            /// Two parameter constructor.
            /// </summary>
            /// <param name="inspector">A reference to the DataMapInspector.</param>
            /// <param name="onChange">Callback when the fields have changed.</param>
            public NameIDMapListElement(NameIDMapInspector inspector, Action<int, NameID> onChange) : base(inspector, onChange)
            {
                m_IDField = new IntegerField();
                m_IDField.AddToClassList(c_StyleClassName + "_id");
                m_IDField.isDelayed = true;
                m_IDField.RegisterValueChangedCallback(evt =>
                {
                    if (!inspector.m_NameIDMap.IsIDValid((uint)evt.newValue)) {
                        m_IDField.SetValueWithoutNotify(evt.previousValue);
                        return;
                    }
                    onChange(m_Index, new NameID((uint)evt.newValue, m_NameField.value));
                });
                Add(m_IDField);

                m_NameField = new TextField();
                m_NameField.AddToClassList(c_StyleClassName + "_name");
                m_NameField.isDelayed = true;
                m_NameField.RegisterValueChangedCallback(evt =>
                {
                    if (!inspector.m_NameIDMap.IsNameValid(evt.newValue)) {
                        m_NameField.SetValueWithoutNotify(evt.previousValue);
                        return;
                    }
                    onChange(m_Index, new NameID((uint)m_IDField.value, evt.newValue));
                });
                Add(m_NameField);
            }

            /// <summary>
            /// Binds the rwo to the specified object.
            /// </summary>
            /// <param name="obj">The object that is being bound.</param>
            /// <param name="index">The index of the row.</param>
            public override void Bind(NameID obj, int index)
            {
                base.Bind(obj, index);

                var nameIDInspector = m_Inspector as NameIDMapInspector;
                if (nameIDInspector.m_DuplicateIDs.Contains(obj.ID)) {
                    m_WarningIndicator.style.display = DisplayStyle.Flex;
                    m_WarningIndicator.tooltip += "The ID is already used.";
                }

                if (nameIDInspector.m_DuplicateNames.Contains(obj.Name)) {
                    m_WarningIndicator.style.display = DisplayStyle.Flex;
                    m_WarningIndicator.tooltip += "\nThe name is already used.";
                }

                m_IDField.SetValueWithoutNotify((int)obj.ID);
                m_NameField.SetValueWithoutNotify(obj.Name);
            }
        }
    }
}