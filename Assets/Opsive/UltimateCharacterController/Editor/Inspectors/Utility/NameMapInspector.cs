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
    using UnityEngine.UIElements;

    /// <summary>
    /// The NameIDMapInspector implements the DataMapInspector for the name object.
    /// </summary>
    public class NameMapInspector : DataMapInspector<string>
    {
        private static List<string> s_OrderByList = new List<string>() {  "Name Ascending", "Name Descending" };

        private NameMap m_NameMap;
        private List<string> m_DuplicateNames;

        protected override string ReorderableListTitle => "Names";

        /// <summary>
        /// Performs any initialization.
        /// </summary>
        protected override void InitializeInspector()
        {
            base.InitializeInspector();

            m_NameMap = m_DataMap as NameMap;
        }

        /// <summary>
        /// Returns a new ListElement.
        /// </summary>
        /// <param name="onChange">Callback when the element has changed.</param>
        /// <returns>A new ListElement.</returns>
        protected override ListElement GetListElement(Action<int, string> onChange)
        {
            return new NameMapListElement(this, onChange);
        }

        /// <summary>
        /// Shows the sorting elements.
        /// </summary>
        /// <param name="container">The parent VisualElement.</param>
        protected override void ShowSortingElements(VisualElement container)
        {
            var orderByDropDown = new DropdownField("Order By", s_OrderByList, (int)m_NameMap.SortOrder, (choice) =>
            {
                return s_OrderByList[(int)m_NameMap.SortOrder];
            });
            orderByDropDown.RegisterValueChangedCallback(evt =>
            {
                var index = s_OrderByList.IndexOf(evt.newValue);
                m_NameMap.SortOrder = (NameMap.SortOption)index;  // Specifying a new sort order will sort the objects.
                orderByDropDown.SetValueWithoutNotify(s_OrderByList[index]);
                Refresh();
            });

            m_DuplicateNames = new List<string>();
            container.Add(orderByDropDown);
        }

        /// <summary>
        /// Refreshes the objects.
        /// </summary>
        protected override void DoRefresh()
        {
            m_DuplicateNames.Clear();

            if (!m_NameMap.IsDataValid()) {
                m_NameMap.GetDuplicateNames(m_DuplicateNames);
            }
        }

        /// <summary>
        /// VisualElement for a row within the ReorderableList.
        /// </summary>
        public class NameMapListElement : ListElement
        {
            private TextField m_NameField;

            /// <summary>
            /// Two parameter constructor.
            /// </summary>
            /// <param name="inspector">A reference to the DataMapInspector.</param>
            /// <param name="onChange">Callback when the fields have changed.</param>
            public NameMapListElement(NameMapInspector inspector, Action<int, string> onChange) : base(inspector, onChange)
            {
                m_NameField = new TextField();
                m_NameField.AddToClassList(c_StyleClassName + "_name");
                m_NameField.isDelayed = true;
                m_NameField.RegisterValueChangedCallback(evt =>
                {
                    if (!inspector.m_NameMap.IsNameValid(m_NameField.value)) {
                        m_NameField.SetValueWithoutNotify(evt.previousValue);
                        return;
                    }
                    onChange(m_Index, m_NameField.value);
                });
                Add(m_NameField);
            }

            /// <summary>
            /// Binds the rwo to the specified object.
            /// </summary>
            /// <param name="obj">The object that is being bound.</param>
            /// <param name="index">The index of the row.</param>
            public override void Bind(string obj, int index)
            {
                base.Bind(obj, index);

                var nameIDInspector = m_Inspector as NameMapInspector;
                if (nameIDInspector.m_DuplicateNames.Contains(obj)) {
                    m_WarningIndicator.style.display = DisplayStyle.Flex;
                    m_WarningIndicator.tooltip += "\nThe name is already used.";
                }

                m_NameField.SetValueWithoutNotify(obj);
            }
        }
    }
}