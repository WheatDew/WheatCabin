/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Objects
{
    using Opsive.UltimateCharacterController.Objects;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the Projectile component.
    /// </summary>
    [CustomEditor(typeof(Projectile), true)]
    public class ProjectileInspector : ProjectileBaseInspector
    {
        /// <summary>
        /// Draws the inspector fields for the object.
        /// </summary>
        protected override void DrawObjectFields(VisualElement container)
        {
            base.DrawObjectFields(container);

            PropertiesFoldout(container, "Projectile", m_ExcludeFields, new[]
            {
                "m_Lifespan",
            });
        }
    }
}
