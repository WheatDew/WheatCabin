/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Inventory
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Editor.Managers;
    using Opsive.UltimateCharacterController.Inventory;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the Category ScriptableObject.
    /// </summary>
    [CustomEditor(typeof(Category), true)]
    public class CategoryInspector : UIElementsInspector
    {
        protected override bool ExcludeAllFields => true;

        /// <summary>
        /// Adds the custom UIElements to the bottom of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            var label = new Label("The Category can be managed within the Item Type Manager.");
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.whiteSpace = WhiteSpace.Normal;
            container.Add(label);

            var button = new Button();
            button.text = "Open Item Type Manager";
            button.clicked += () =>
            {
                CharacterMainWindow.ShowItemTypeManagerWindow();
            };
            container.Add(button);
        }
    }
}