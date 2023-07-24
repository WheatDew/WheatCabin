/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.Shared.Editor.UIElements.Controls.Types;
    using Opsive.UltimateCharacterController.Editor.Utility;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Utility;
    using System;
    using System.Reflection;
    using Opsive.Shared.Editor.Utility;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Implements TypeControlBase for the IDObjectBase ControlType.
    /// </summary>
    [ControlType(typeof(IDObjectBase))]
    public class IDObjectControl : TypeControlBase
    {
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
            var idObject = input.Value as IDObjectBase;
            if (idObject == null) {
                return new Label("(null)");
            }

            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");

            var idField = new IntegerField();
            var nameIDMap = ObjectIdentifierSearchableWindow.GetNameIDMap();
            idField.style.flexGrow = 1;
            idField.style.width = idField.style.maxWidth = idField.style.minWidth = 75;
            idField.value = idObject.ID;
            idField.RegisterValueChangedCallback(c =>
            {
                idObject.ID = c.newValue;
                input.OnChangeEvent?.Invoke(idObject);
                Shared.Editor.Utility.EditorUtility.SetDirty(input.UnityObject);
            });
            horizontalLayout.Add(idField);

            var idSearchButton = new Button();
            idSearchButton.AddToClassList(DataMapInspector<NameID>.StyleClassName + "_button");
            idSearchButton.text = "▼";
            idSearchButton.clicked += () => {
                var nameID = new NameID((uint)idObject.ID, null);
                if (nameIDMap != null) {
                    nameID = new NameID((uint)idObject.ID, nameIDMap.GetName((uint)idObject.ID));
                }
                ObjectIdentifierSearchableWindow.OpenWindow("Object Identifiers", nameID.Name, (newValue) =>
                {
                    idObject.ID = (int)newValue.ID;
                    idField.SetValueWithoutNotify((int)newValue.ID);
                    input.OnChangeEvent?.Invoke(idObject);
                    Shared.Editor.Utility.EditorUtility.SetDirty(input.UnityObject);
                }, true);
            };
            idField.Add(idSearchButton);

            var objectField = new ObjectField();
            objectField.objectType = idObject.ObjectType;
            objectField.style.flexGrow = 1;
            objectField.RegisterValueChangedCallback(evt =>
            {
                idObject.BaseObject = evt.newValue;
                input.OnChangeEvent?.Invoke(idObject);
                Shared.Editor.Utility.EditorUtility.SetDirty(input.UnityObject);
            });
            objectField.SetValueWithoutNotify(idObject.BaseObject);
            horizontalLayout.Add(objectField);

            if (input.Field != null) {
                System.Action<object> onBindingUpdateEvent = (object newValue) =>
                {
                    var newIDObject = newValue as IDObjectBase;
                    objectField.SetValueWithoutNotify(newIDObject.BaseObject);
                };
                idField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(input.Field, input.ArrayIndex, input.Target, onBindingUpdateEvent);
                });
                idField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
                objectField.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    BindingUpdater.AddBinding(input.Field, input.ArrayIndex, input.Target, onBindingUpdateEvent);
                });
                objectField.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    BindingUpdater.RemoveBinding(onBindingUpdateEvent);
                });
            }

            // Add fields to the container.
            return horizontalLayout;
        }
    }
    
    /// <summary>
    /// Implements PropertyDrawer for the IDObjectBase allowing default inspectors to have a dropdown menu of id names.
    /// </summary>
    [CustomPropertyDrawer(typeof(IDObjectBase), true)]
    public class IDObjectDrawer : PropertyDrawer
    {
        /// <summary>
        /// Override this method to make your own UIElements based GUI for the property.
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <returns>The element containing the custom GUI.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var targetObject = property.serializedObject.targetObject;
            var targetGameObject = (targetObject as Component)?.gameObject;
            var field = property.GetFieldInfo();
            var attributeModifier = (IDObjectBase)ControlUtility.GetTargetObjectOfProperty(property);

            var attributeModifierControl = new IDObjectControl();
            var control = attributeModifierControl.GetTypeControl(
                new TypeControlBase.TypeControlInput {
                    UnityObject = targetGameObject,
                    Target = targetObject,
                    Field = field,
                    SerializedProperty = property,
                    ArrayIndex = -1,
                    Type = attributeModifier?.GetType() ?? typeof(IDObjectBase),
                    Value = attributeModifier,
                    OnChangeEvent = (newValue) =>
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        return true;
                    }
                });

            return control;
        }

        /// <summary>
        ///   <para>Override this method to make your own IMGUI based GUI for the property.</para>
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            // Calculate rects
            var idRect = new Rect(position.x, position.y, 90, position.height);
            var buttonRect = new Rect(position.x + 90, position.y, 15, position.height);
            var objectRect = new Rect(position.x + 90 + 15 + 5, position.y, position.width - 90 - 15 - 5, position.height);
            
            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(idRect, property.FindPropertyRelative("m_ID"),GUIContent.none);
            if (GUI.Button(buttonRect, "▼")) {
                var nameIDMap = ObjectIdentifierSearchableWindow.GetNameIDMap();
                var idObjectID = (uint)property.FindPropertyRelative("m_ID").intValue;
                var nameID = new NameID((uint)idObjectID, null);
                if (nameIDMap != null) {
                    nameID = new NameID((uint)idObjectID, nameIDMap.GetName((uint)idObjectID));
                }
                ObjectIdentifierSearchableWindow.OpenWindow("Object Identifiers", nameID.Name, (newValue) =>
                {
                    property.FindPropertyRelative("m_ID").intValue = (int)newValue.ID;
                    property.serializedObject.ApplyModifiedProperties();
                }, true);
            }
            EditorGUI.PropertyField(objectRect, property.FindPropertyRelative("m_Object"),GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}