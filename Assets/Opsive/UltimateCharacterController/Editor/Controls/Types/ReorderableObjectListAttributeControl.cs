/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Attributes;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Implements AttributeControlBase for the DropdownEffect attribute.
    /// </summary>
    [ControlType(typeof(ReorderableObjectListAttribute))]
    public class ReorderableObjectListAttributeControl : AttributeControlBase
    {
        private IList m_List;
        private Func<object, bool> m_OnChangeEvent;
        private FieldInfo m_FieldInfo;
        private Type m_ElementType;

        /// <summary>
        /// Does the attribute override the type control?
        /// </summary>
        public override bool OverrideTypeControl { get { return true; } }

        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get { return false; } }

        /// <summary>
        /// Returns the attribute control that should be used for the specified AttributeControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(AttributeControlInput input)
        {
            m_FieldInfo = input.Field;
            var elementType = m_FieldInfo.FieldType.GetElementType();
            if (elementType == null) {
                return null;
            }

            m_OnChangeEvent = input.OnChangeEvent;
            m_ElementType = elementType;

            var listType = typeof(List<>).MakeGenericType(m_ElementType);
            m_List = Activator.CreateInstance(listType) as IList;

            var valueList = input.Value as IList;
            if (valueList != null) {
                for (int i = 0; i < valueList.Count; i++) {
                    m_List.Add(valueList[i]);
                }
            }

            ReorderableList reorderableList = null;
            reorderableList = new ReorderableList(m_List, (VisualElement container, int index) => // Add Row.
            {
                var stateElementContainer = new GenericListElement(
                    input.Field.FieldType.GetElementType(),
                    input.UnityObject, (index, newValue) =>
                    {
                        m_List[index] = newValue;
                        HandleValueChanged();
                    });
                container.Add(stateElementContainer);
            }, (VisualElement container, int index) => // Bind.
            {
                var obj = m_List[index];
                var elementContainer = container.Q<GenericListElement>();
                elementContainer.Refresh(input.Target, input.Field, index, obj);
            }, (VisualElement container) => // Header.
            {
                container.Add(new Label(ObjectNames.NicifyVariableName(input.Field.Name)));
            }, (int index) => // Select.
            {
            }, () => // Add
            {
                var listElementType = input.Field.FieldType.GetElementType();
                m_List.Add(default);
                HandleValueChanged();
            }, (int index) => // Remove.
            {
                m_List.RemoveAt(index);
                HandleValueChanged();
            }, (int fromIndex, int toIndex) =>
            {
                HandleValueChanged();
            });
            return reorderableList;
        }

        /// <summary>
        /// The list value has changed.
        /// </summary>
        private void HandleValueChanged()
        {
            if (m_FieldInfo.FieldType.IsArray) {
                var array = Array.CreateInstance(m_ElementType, m_List.Count) as IList;
                for (int i = 0; i < array.Count; i++) {
                    array[i] = m_List[i];
                }
                m_OnChangeEvent?.Invoke(array);
            } else {
                // If its not an array its a list.
                m_OnChangeEvent?.Invoke(m_List);
            }
        }

        /// <summary>
        /// The visual elment for the objects showed inside the list.
        /// </summary>
        public class GenericListElement : VisualElement
        {
            private Type m_ElementType;
            private Object m_UnityObject;
            private object m_Element;
            private object m_Target;
            private FieldInfo m_FieldInfo;
            private Action<int, object> m_OnChange;
            private VisualElement m_OtherFieldsContainer;

            public int Index { get; set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public GenericListElement(Type elementType, Object unityObject, Action<int, object> onChange)
            {
                m_ElementType = elementType;
                m_UnityObject = unityObject;
                m_OnChange = onChange;

                AddToClassList("horizontal-layout");

                m_OtherFieldsContainer = new VisualElement();
                m_OtherFieldsContainer.style.flexGrow = 1;
                m_OtherFieldsContainer.style.marginRight = 4;
                m_OtherFieldsContainer.AddToClassList("horizontal-layout");
                Add(m_OtherFieldsContainer);
            }

            /// <summary>
            /// Refresh the view to show the updated data.
            /// </summary>
            /// <param name="index">The index of the element in the list.</param>
            /// <param name="element">The element at the index.</param>
            public virtual void Refresh(object target, FieldInfo field, int index, object element)
            {
                Index = index;
                m_Element = element;
                m_Target = target;
                m_FieldInfo = field;
                Refresh();
            }

            /// <summary>
            /// Show the updated data.
            /// </summary>
            public virtual void Refresh()
            {
                m_OtherFieldsContainer.Clear();

                FieldInspectorView.AddField(m_UnityObject, m_Target, null, null, Index, m_ElementType,
                    null, null, false, m_Element,
                    m_OtherFieldsContainer, (newValue) =>
                    {
                        if (m_UnityObject != null) {
                            Undo.RecordObject(m_UnityObject, "Change " + ObjectNames.NicifyVariableName(m_FieldInfo.Name));
                        }

                        m_Element = newValue;
                        InvokeChange(newValue);
                    });
            }

            /// <summary>
            /// Invoke that the field was changed.
            /// </summary>
            /// <param name="obj">The object that changed.</param>
            private void InvokeChange(object obj)
            {
                m_OnChange?.Invoke(Index, m_Element);
            }
        }
    }
}