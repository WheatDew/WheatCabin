/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using System;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    /// <summary>
    /// The inspectable Object field with a button to open a property window showing the UnityObject (example: scriptable object)
    /// </summary>
    public class InspectableObjectField<T> : VisualElement where T : UnityEngine.Object
    {
        public event Action<int, T> OnValueChanged;

        private Label m_Label;
        private Button m_Button;
        private ObjectField m_ObjectField;

        public string LabelText { 
            get => m_Label.text; 
            set {
                m_Label.text = value;
                m_Label.style.display = string.IsNullOrEmpty(m_Label.text) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }
        public float LabelMinWidth
        {
            get => m_Label.style.minWidth.value.value;
            set => m_Label.style.minWidth = value;
        }
        public int Index { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public InspectableObjectField()
        {
            AddToClassList("inspectable-object-field");

            m_Label = new Label();
            m_Label.style.display = DisplayStyle.None;
            Add(m_Label);

            m_ObjectField = new ObjectField();
            m_ObjectField.objectType = typeof(T);
            m_ObjectField.RegisterValueChangedCallback(evt =>
            {
                OnValueChanged.Invoke(Index, evt.newValue as T);
            });
            m_ObjectField.style.flexGrow = 1;
            Add(m_ObjectField);

            m_Button = new Button();
            m_Button.AddToClassList(UnityEditor.EditorGUIUtility.isProSkin ? "open-dark-icon" : "open-light-icon");
            m_Button.clickable.clicked += () =>
            {
                if (m_ObjectField.value == null) {
                    return;
                }

                Shared.Editor.Utility.EditorUtility.OpenInPropertyEditor(m_ObjectField.value);
            };
            Add(m_Button);
        }

        /// <summary>
        /// Refresh the field to show the updated data.
        /// </summary>
        /// <param name="obj">The object to set in the field.</param>
        public virtual void Refresh(T obj)
        {
            m_ObjectField.SetValueWithoutNotify(obj);
        }
    }
}