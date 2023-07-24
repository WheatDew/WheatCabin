/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Camera
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateCharacterController.Camera;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shows a custom inspector for the LegacyCameraController.
    /// </summary>
    [CustomEditor(typeof(LegacyCameraController), true)]
    public class LegacyCameraControllerInspector : UIElementsInspector
    {
        protected override bool ExcludeAllFields => true;

        /// <summary>
        /// Adds the custom UIElements to the bottom of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            var label = new Label("The Camera Controller needs to be updated to version 3.\nThe camera can be updated through the Migration Manager.");
            label.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            label.style.whiteSpace = WhiteSpace.Normal;
            container.Add(label);

            var button = new Button();
            button.text = "Open Migration Manager";
            button.clicked += () =>
            {
                Managers.CharacterMainWindow.ShowMigrationManagerWindow();
            };
            container.Add(button);
        }
    }
}