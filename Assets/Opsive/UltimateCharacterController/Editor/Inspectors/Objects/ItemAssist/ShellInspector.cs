/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects.ItemAssist
{
    using Opsive.UltimateCharacterController.Objects.ItemAssist;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the Shell component.
    /// </summary>
    [CustomEditor(typeof(Shell))]
    public class ShellInspector : TrajectoryObjectInspector
    {
        /// <summary>
        /// Draws the inspector fields for the object.
        /// </summary>
        protected override void DrawObjectFields(VisualElement container)
        {
            base.DrawObjectFields(container);
            
            var foldout = PropertiesFoldout(container, "Shell",m_ExcludeFields, new[]
            {
                "m_Lifespan",
                "m_Persistence",
            });
        }
    }
}
