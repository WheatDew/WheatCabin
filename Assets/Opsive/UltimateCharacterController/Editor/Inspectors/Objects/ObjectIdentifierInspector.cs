/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Editor.Utility;
    using Opsive.UltimateCharacterController.Objects;
    using Opsive.UltimateCharacterController.Utility;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shows a custom inspector for the ObjectIdentifier component.
    /// </summary>
    [CustomEditor(typeof(ObjectIdentifier), true)]
    public class ObjectIdentifierInspector : UIElementsInspector
    {
        protected override List<string> ExcludedFields { get => new List<string>() { "m_ID" }; }
        
        protected ObjectIdentifier m_ObjectIdentifier;

        /// <summary>
        /// Initializes the inspector fields.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ObjectIdentifier = target as ObjectIdentifier;
            base.InitializeInspector();
        }

        /// <summary>
        /// Adds the custom UIElements to the bottom of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList("horizontal-layout");
            container.Add(horizontalLayout);

            var labelField = new Label("ID");
            labelField.style.width = 150;
            horizontalLayout.Add(labelField);

            var idField = new IntegerField();
            var nameField = new TextField();
            var nameIDMap = ObjectIdentifierSearchableWindow.GetNameIDMap();
            idField.style.flexGrow = 1;
            idField.style.maxWidth = 100;
            idField.value = (int)m_ObjectIdentifier.ID;
            idField.RegisterValueChangedCallback(c =>
            {
                m_ObjectIdentifier.ID = (uint)c.newValue;
                if (nameIDMap != null) {
                    nameField.SetValueWithoutNotify(nameIDMap.GetName(m_ObjectIdentifier.ID));
                }
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
            });
            horizontalLayout.Add(idField);

            var idSearchButton = new Button();
            idSearchButton.AddToClassList(DataMapInspector<NameID>.StyleClassName + "_button");
            idSearchButton.text = "▼";
            idSearchButton.clicked += () => {
                var nameID = new NameID(m_ObjectIdentifier.ID, null);
                if (nameIDMap != null) {
                    nameID = new NameID(m_ObjectIdentifier.ID, nameIDMap.GetName(m_ObjectIdentifier.ID));
                }
                ObjectIdentifierSearchableWindow.OpenWindow("Object Identifiers", nameID.Name, (newValue) =>
                {
                    m_ObjectIdentifier.ID = newValue.ID;
                    idField.SetValueWithoutNotify((int)newValue.ID);
                    nameField.SetValueWithoutNotify(nameIDMap.GetName(m_ObjectIdentifier.ID));
                    Shared.Editor.Utility.EditorUtility.SetDirty(target);
                }, true);
            };
            idField.Add(idSearchButton);

            nameField.name = "name-field";
            nameField.SetEnabled(false); // The name cannot be directly set.
            if (nameIDMap != null) {
                nameField.SetValueWithoutNotify(nameIDMap.GetName(m_ObjectIdentifier.ID));
            }
            horizontalLayout.Add(nameField);

            base.ShowFooterElements(container);
        }
    }
}