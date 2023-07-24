/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Editor.Inspectors
{
    using Opsive.Shared.StateSystem;
    using Shared.Editor.UIElements;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shows a custom inspector for the StateBehavior component.
    /// </summary>
    [CustomEditor(typeof(StateBehavior), true)]
    public class UIStateBehaviorInspector : UIElementsInspector
    {
        /// <summary>
        /// Add the styles to the container.
        /// </summary>
        /// <param name="container">The container to add styles to.</param>
        protected override void AddStyleSheets(VisualElement container)
        {
            base.AddStyleSheets(container);

            container.styleSheets.Add(Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("a243b2a2fb9cc0d45a2aac464a7a3ba3")); // ReorderableStateList stylesheet.
        }

        /// <summary>
        /// Draws the custom UIElements to the bottom of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            FieldInspectorView.AddField(target, target, "m_States", container, (object obj) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
            }, null);

            base.ShowFooterElements(container);
        }
    }
}