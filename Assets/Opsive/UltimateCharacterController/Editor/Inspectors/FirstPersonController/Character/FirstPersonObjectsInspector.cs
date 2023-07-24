/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.FirstPersonController.Character
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.FirstPersonController.Character;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shows a custom inspector for the FirstPersonObjects.
    /// </summary>
    [CustomEditor(typeof(FirstPersonObjects))]
    public class FirstPersonObjectsInspector : Shared.Editor.Inspectors.UIStateBehaviorInspector
    {
        protected override bool ExcludeAllFields => true;

        /// <summary>
        /// Adds the custom UIElements to the top of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowHeaderElements(VisualElement container)
        {
            FieldInspectorView.AddField(target, target, "m_PitchLimit", container, (object obj) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
                // Force refresh the value with the property setter.
                (target as FirstPersonObjects).PitchLimit = (obj as FirstPersonObjects).PitchLimit;
            });
            FieldInspectorView.AddField(target, target, "m_LockPitch", container, (object obj) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
                (target as FirstPersonObjects).LockPitch = (obj as FirstPersonObjects).LockPitch;
            });
            FieldInspectorView.AddField(target, target, "m_YawLimit", container, (object obj) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
                (target as FirstPersonObjects).YawLimit = (obj as FirstPersonObjects).YawLimit;
            });
            FieldInspectorView.AddField(target, target, "m_LockYaw", container, (object obj) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
                (target as FirstPersonObjects).LockYaw = (obj as FirstPersonObjects).LockYaw;
            });
            FieldInspectorView.AddField(target, target, "m_RotateWithCrosshairs", container, (object obj) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
                (target as FirstPersonObjects).RotateWithCrosshairs = (obj as FirstPersonObjects).RotateWithCrosshairs;
            });
            FieldInspectorView.AddField(target, target, "m_RotationSpeed", container);
            FieldInspectorView.AddField(target, target, "m_IgnorePositionalLookOffset", container, (object obj) =>
            {
                Shared.Editor.Utility.EditorUtility.SetDirty(target);
                (target as FirstPersonObjects).IgnorePositionalLookOffset = (obj as FirstPersonObjects).IgnorePositionalLookOffset;
            });
            FieldInspectorView.AddField(target, target, "m_PositionOffset", container);
            FieldInspectorView.AddField(target, target, "m_MoveSpeed", container);
        }
    }
}