/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.UIElements.Controls.Types
{
    using Opsive.Shared.UI;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the UnityEngine.Object ControlType.
    /// </summary>
    [ControlType(typeof(Text))]
    public class TextControl : TypeControlBase
    {
        protected bool m_UsingTMP;
        
        /// <summary>
        /// Does the control use a label?
        /// </summary>
        public override bool UseLabel { get { return true; } }

        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="input">The input to the control.</param>
        /// <returns>The created control.</returns>
        protected override VisualElement GetControl(TypeControlInput input)
        {
            var sharedText = (Text)input.Value;
#if TEXTMESH_PRO_PRESENT
            m_UsingTMP = SharedTextDropdown.IsDefaultUseTMP(sharedText);
#else
            m_UsingTMP = false;
#endif
            
            var objectField = new ObjectField();
            objectField.style.flexShrink = objectField.style.flexGrow = 1;
            objectField.style.flexDirection = FlexDirection.Row;

#if TEXTMESH_PRO_PRESENT
            if (m_UsingTMP) {
                objectField.objectType = typeof(TMPro.TMP_Text);
                objectField.SetValueWithoutNotify(sharedText.TextMeshProText);
            } else {
#endif
                objectField.objectType = typeof(UnityEngine.UI.Text);
                objectField.SetValueWithoutNotify(sharedText.UnityText);
#if TEXTMESH_PRO_PRESENT
            }
#endif

            // Ensure the control is kept up to date as the value changes.
            if (input.Field != null) {
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    var newSharedText = (Text)newValue;
#if TEXTMESH_PRO_PRESENT
                    if (m_UsingTMP) {
                        objectField.SetValueWithoutNotify(newSharedText.TextMeshProText);
                    } else {
#endif
                        objectField.SetValueWithoutNotify(newSharedText.UnityText);
#if TEXTMESH_PRO_PRESENT
                    }
#endif
                };
                objectField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(input.Field, input.ArrayIndex, input.Target, onBindingUpdateEvent);
                });
                objectField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
            }
            objectField.RegisterValueChangedCallback(c =>
            {
                var text = (Text)input.Value;
#if TEXTMESH_PRO_PRESENT
                if (m_UsingTMP) {
                    text.TextMeshProText = (TMPro.TMP_Text)c.newValue;
                } else {
#endif
                    text.UnityText= (UnityEngine.UI.Text)c.newValue;
#if TEXTMESH_PRO_PRESENT
                }
#endif
                if (!input.OnChangeEvent(text)) {
                    objectField.SetValueWithoutNotify(c.previousValue);
#if TEXTMESH_PRO_PRESENT
                    if (m_UsingTMP) {
                        text.TextMeshProText = (TMPro.TMP_Text)c.previousValue;
                    } else {
#endif
                        text.UnityText = (UnityEngine.UI.Text)c.previousValue;
#if TEXTMESH_PRO_PRESENT
                    }
#endif
                }
                c.StopPropagation();
            });
            
#if TEXTMESH_PRO_PRESENT
            var dropdown = new SharedTextDropdown(sharedText);
            dropdown.OnUseTMPChange += useTMP =>
            {
                objectField.objectType = useTMP ? typeof(TMPro.TMP_Text) : typeof(UnityEngine.UI.Text);
                objectField.SetValueWithoutNotify(useTMP ? (Object)sharedText.TextMeshProText : (Object)sharedText.UnityText);
                m_UsingTMP = useTMP;
            };
            
            objectField.Insert(0,dropdown);
#endif
            
            return objectField;
        }
    }

#if TEXTMESH_PRO_PRESENT
    /// <summary>
    /// VisualElement for showing the TextMeshPro or Text dropdown.
    /// </summary>
    public class SharedTextDropdown : VisualElement
    {
        public event System.Action<bool> OnUseTMPChange;
        
        private static string c_EditorPrefKey = "Opsive.Shared.Editor.UIElements.Controls.Types.TextControl.UsingTMP";

        public static bool EditorPrefKey
        {
            get => EditorPrefs.GetBool(c_EditorPrefKey, true);
            set => EditorPrefs.SetBool(c_EditorPrefKey, value);
        }
        
        /// <summary>
        /// Is the TextMeshPro dropdown being used?
        /// </summary>
        /// <param name="sharedText">A reference to the text struct.</param>
        /// <returns>True if the TextMeshPro dropdown is being used.</returns>
        public static bool IsDefaultUseTMP(Text sharedText)
        {
            return sharedText.TextMeshProText != null || (sharedText.UnityText == null && EditorPrefKey);
        }
        
        /// <summary>
        /// Single parameter constructor.
        /// </summary>
        /// <param name="sharedText">A reference to the text struct.</param>
        public SharedTextDropdown(Text sharedText)
        {
            var usingTMP = IsDefaultUseTMP(sharedText);
            
            var dropDownList = new System.Collections.Generic.List<string>() { "TMP", "Text" };
            var defaultIndex = usingTMP ? 0 : 1;
            var dropdown = new PopupField<string>(dropDownList, defaultIndex, (formatValue) =>
            {
                return formatValue;
            },(formatValue) =>
            {
                return formatValue;
            });
            dropdown.RegisterValueChangedCallback(evt =>
            {
                var useTMP = dropDownList.IndexOf(evt.newValue) == 0;
                EditorPrefKey = useTMP;
                OnUseTMPChange?.Invoke(useTMP);
            });
            
            Add(dropdown);
        }
    }
#endif

    /// <summary>
    /// Draws the text using the PropertyDrawer system.
    /// </summary>
    [CustomPropertyDrawer(typeof(Text))]
    public class TextDrawer : PropertyDrawer
    {
#if TEXTMESH_PRO_PRESENT
        private int m_PreviousDropdownIndex = -1;
#endif

        /// <summary>
        /// Creates a new VisualElement using the specified property.
        /// </summary>
        /// <param name="property">The property that is being drawn.</param>
        /// <returns>The new VisualElement using the specified property.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var sharedText = (Text) ControlUtility.GetTargetObjectOfProperty(property);

            // Create property container element.
            var objectField = new ObjectField();
            objectField.style.flexShrink = objectField.style.flexGrow = 1;
            objectField.style.flexDirection = FlexDirection.Row;
            
            var labelControl = new LabelControl(property.name, property.tooltip, objectField);
            labelControl.Add(objectField);

            var usingTMP = false;
#if TEXTMESH_PRO_PRESENT
            usingTMP = SharedTextDropdown.IsDefaultUseTMP(sharedText);
#endif

            if (usingTMP) {
#if TEXTMESH_PRO_PRESENT
                objectField.objectType = typeof(TMPro.TMP_Text);
                objectField.SetValueWithoutNotify(sharedText.TextMeshProText);
#endif
            } else {
                objectField.objectType = typeof(UnityEngine.UI.Text);
                objectField.SetValueWithoutNotify(sharedText.UnityText);
            }
            
            objectField.RegisterValueChangedCallback(c =>
            {
                if (usingTMP) {
                    property.FindPropertyRelative("m_TextMeshProText").objectReferenceValue = c.newValue;
                } else {
                    property.FindPropertyRelative("m_UnityText").objectReferenceValue = c.newValue;
                }
                
                property.serializedObject.ApplyModifiedProperties();
                
            });
            
#if TEXTMESH_PRO_PRESENT
            var dropdown = new SharedTextDropdown(sharedText);
            dropdown.OnUseTMPChange += useTMP =>
            {
                objectField.objectType = useTMP ? typeof(TMPro.TMP_Text) : typeof(UnityEngine.UI.Text);
                objectField.SetValueWithoutNotify(useTMP ? (Object)sharedText.TextMeshProText : (Object)sharedText.UnityText);
            };
            
            objectField.Insert(0,dropdown);
#endif

            // Add fields to the container.
            return labelControl;
        }

        /// <summary>
        /// Draws the control within the specified rect.
        /// </summary>
        /// <param name="rect">The text bounds.</param>
        /// <param name="property">The property that should be drawn.</param>
        /// <param name="label">The label of the control</param>
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var sharedText = (Text) ControlUtility.GetTargetObjectOfProperty(property);
            
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(rect, label, property);

            // Draw the label first.
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented.
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

#if !TEXTMESH_PRO_PRESENT
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("m_UnityText"), GUIContent.none);
#else
            var dropdownRect = new Rect(rect.x, rect.y, 60, rect.height);
            var objectRect = new Rect(rect.x + 60, rect.y, rect.width - 60, rect.height);
            var usingTMP = SharedTextDropdown.IsDefaultUseTMP(sharedText);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels.
            var selectedIndex = m_PreviousDropdownIndex == -1 ? (usingTMP ? 0 : 1) : m_PreviousDropdownIndex;
            var value = EditorGUI.Popup(dropdownRect, selectedIndex, new string[] { "TMP", "Text" });

            if (value == 0 && !usingTMP) {
                usingTMP = true;
                SharedTextDropdown.EditorPrefKey = usingTMP;
            }else if (value == 1 && usingTMP) {
                usingTMP = false;
                SharedTextDropdown.EditorPrefKey = usingTMP;
            }

            m_PreviousDropdownIndex = value;
            
            if (usingTMP) {
                EditorGUI.PropertyField(objectRect, property.FindPropertyRelative("m_TextMeshProText"), GUIContent.none);
            } else {
                EditorGUI.PropertyField(objectRect, property.FindPropertyRelative("m_UnityText"), GUIContent.none);
            }
#endif

            // Set indent back to the original value.
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}