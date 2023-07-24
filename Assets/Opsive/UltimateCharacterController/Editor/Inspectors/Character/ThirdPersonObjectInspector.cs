/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character
{
    using Opsive.Shared.Editor.Inspectors.StateSystem;
    using Opsive.UltimateCharacterController.Character.Identifiers;
    using System;
    using UnityEditor;

    /// <summary>
    /// Shows a custom inspector for the Third Person Object inspector
    /// </summary>
    [CustomEditor(typeof(ThirdPersonObject), true)]
    public class ThirdPersonobjectInspector : StateBehaviorInspector
    {
        /// <summary>
        /// Returns the actions to draw before the State list is drawn.
        /// </summary>
        /// <returns>The actions to draw before the State list is drawn.</returns>
        protected override Action GetDrawCallback()
        {
            var baseCallback = base.GetDrawCallback();

            baseCallback += () =>
            {
                var forceVisibleProperty = PropertyFromName("m_ForceVisible");
                var forceVisibleValue = forceVisibleProperty.boolValue;
                EditorGUILayout.PropertyField(forceVisibleProperty);
                if (forceVisibleValue != forceVisibleProperty.boolValue) {
                    // Set the property so the PerspectiveMonitor is aware of the change.
                    (target as ThirdPersonObject).ForceVisible = !forceVisibleValue;
                }
#if !THIRD_PERSON_CONTROLLER
                EditorGUILayout.PropertyField(PropertyFromName("m_VisibleMaterials"));
#endif
                EditorGUILayout.PropertyField(PropertyFromName("m_FirstPersonVisibleOnDeath"));
            };

            return baseCallback;
        }
    }
}