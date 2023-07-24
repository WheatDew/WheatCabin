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
    /// Custom inspector for the Destructible component.
    /// </summary>
    [CustomEditor(typeof(ProjectileBase))]
    public class ProjectileBaseInspector : TrajectoryObjectInspector
    {
        /// <summary>
        /// Draws the inspector fields for the object.
        /// </summary>
        protected override void DrawObjectFields(VisualElement container)
        {
            base.DrawObjectFields(container);

            PropertiesFoldout(container, "Destruction", m_ExcludeFields, new[]
            {
                "m_DestroyOnCollision",
                "m_WaitForParticleStop",
                "m_DestructionDelay",
                "m_SpawnedObjectsOnDestruction",
            });
        }
    }
}
