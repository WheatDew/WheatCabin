/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.Character
{
    using Opsive.UltimateCharacterController.Character;
    using Opsive.Shared.Editor.Inspectors;
    using Opsive.Shared.Editor.UIElements;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shows a custom inspector for the CharacterLocomotion component.
    /// </summary>
    [CustomEditor(typeof(CharacterLocomotion), true)]
    public class CharacterLocomotionInspector : UIStateBehaviorInspector
    {
        protected override bool ExcludeAllFields => true;

        /// <summary>
        /// Add the styles to the container.
        /// </summary>
        /// <param name="container">The container to add styles to.</param>
        protected override void AddStyleSheets(VisualElement container)
        {
            base.AddStyleSheets(container);

            container.styleSheets.Add(Shared.Editor.Utility.EditorUtility.LoadAsset<StyleSheet>("a46bc2a887de04846a522116cc71dd3b")); // Controller stylesheet.
        }

        /// <summary>
        /// Draws the custom UIElements to the top of the container.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowHeaderElements(VisualElement container)
        {
            base.ShowHeaderElements(container);

            var characterLocomotion = target as CharacterLocomotion;

            var foldout = PersistentFoldout("Movement");
            FieldInspectorView.AddField(target, "m_Interpolate", foldout);
            var rootMotionPositionFields = new VisualElement();
            var nonRootMotionPositionFields = new VisualElement();
            rootMotionPositionFields.AddToClassList("indent");
            nonRootMotionPositionFields.AddToClassList("indent");
            FieldInspectorView.AddField(target, target, "m_UseRootMotionPosition", foldout, (object o) =>
            {
                EditorUtility.SetDirty(target);
                rootMotionPositionFields.style.display = characterLocomotion.UseRootMotionPosition ? DisplayStyle.Flex : DisplayStyle.None;
                nonRootMotionPositionFields.style.display = characterLocomotion.UseRootMotionPosition ? DisplayStyle.None : DisplayStyle.Flex;
            });
            foldout.Add(rootMotionPositionFields);
            foldout.Add(nonRootMotionPositionFields);
            rootMotionPositionFields.style.display = characterLocomotion.UseRootMotionPosition ? DisplayStyle.Flex : DisplayStyle.None;
            nonRootMotionPositionFields.style.display = characterLocomotion.UseRootMotionPosition ? DisplayStyle.None : DisplayStyle.Flex;

            FieldInspectorView.AddField(target, "m_RootMotionSpeedMultiplier", rootMotionPositionFields);
            FieldInspectorView.AddField(target, "m_MotorAcceleration", nonRootMotionPositionFields);
            FieldInspectorView.AddField(target, "m_MotorDamping", nonRootMotionPositionFields);
            FieldInspectorView.AddField(target, "m_MotorBackwardsMultiplier", nonRootMotionPositionFields);
            FieldInspectorView.AddField(target, "m_PreviousAccelerationInfluence", nonRootMotionPositionFields);
            FieldInspectorView.AddField(target, "m_AdjustMotorForceOnSlope", foldout);
            FieldInspectorView.AddField(target, "m_MotorSlopeForceUp", foldout);
            FieldInspectorView.AddField(target, "m_MotorSlopeForceDown", foldout);

            var rootMotionRotationFields = new VisualElement();
            var nonRootMotionRotationFields = new VisualElement();
            rootMotionRotationFields.AddToClassList("indent");
            nonRootMotionRotationFields.AddToClassList("indent");
            FieldInspectorView.AddField(target, target, "m_UseRootMotionRotation", foldout, (object o) =>
            {
                EditorUtility.SetDirty(target);
                rootMotionRotationFields.style.display = characterLocomotion.UseRootMotionRotation ? DisplayStyle.Flex : DisplayStyle.None;
                nonRootMotionRotationFields.style.display = characterLocomotion.UseRootMotionRotation ? DisplayStyle.None : DisplayStyle.Flex;
            });
            foldout.Add(rootMotionRotationFields);
            foldout.Add(nonRootMotionRotationFields);
            rootMotionRotationFields.style.display = characterLocomotion.UseRootMotionRotation ? DisplayStyle.Flex : DisplayStyle.None;
            nonRootMotionRotationFields.style.display = characterLocomotion.UseRootMotionRotation ? DisplayStyle.None : DisplayStyle.Flex;

            FieldInspectorView.AddField(target, "m_RootMotionRotationMultiplier", rootMotionRotationFields);
            FieldInspectorView.AddField(target, "m_MotorRotationSpeed", nonRootMotionRotationFields);
            FieldInspectorView.AddField(target, "m_UpAlignmentRotationSpeed", foldout);
            container.Add(foldout);

            foldout = PersistentFoldout("Physics");
            FieldInspectorView.AddField(target, "m_Up", foldout);
            FieldInspectorView.AddField(target, "m_UseGravity", foldout);
            FieldInspectorView.AddField(target, "m_GravityAmount", foldout);
            FieldInspectorView.AddField(target, "m_GravityDirection", foldout);
            FieldInspectorView.AddField(target, "m_SkinWidth", foldout);
            FieldInspectorView.AddField(target, "m_StickToGround", foldout);
            FieldInspectorView.AddField(target, "m_StickToGroundDistance", foldout);
            FieldInspectorView.AddField(target, "m_ExternalForceDamping", foldout);
            FieldInspectorView.AddField(target, "m_MaxSoftForceFrames", foldout);
            FieldInspectorView.AddField(target, "m_SlopeLimit", foldout);
            FieldInspectorView.AddField(target, "m_MaxStepHeight", foldout);
            FieldInspectorView.AddField(target, "m_TimeScale", foldout);
            FieldInspectorView.AddField(target, "m_WallGlideCurve", foldout);
            FieldInspectorView.AddField(target, "m_WallFrictionModifier", foldout);
            FieldInspectorView.AddField(target, "m_WallBounceModifier", foldout);
            FieldInspectorView.AddField(target, "m_GroundFrictionModifier", foldout);
            FieldInspectorView.AddField(target, "m_GroundBounceModifier", foldout);
            container.Add(foldout);
            foldout = PersistentFoldout("Collision");
            FieldInspectorView.AddField(target, "m_DetectHorizontalCollisions", foldout);
            FieldInspectorView.AddField(target, "m_DetectVerticalCollisions", foldout);
            FieldInspectorView.AddField(target, "m_DetectGround", foldout);
            FieldInspectorView.AddField(target, "m_ContinuousCollisionDetection", foldout);
            FieldInspectorView.AddField(target, "m_ColliderLayerMask", foldout);
            FieldInspectorView.AddField(target, "m_MaxCollisionCount", foldout);
            FieldInspectorView.AddField(target, "m_MaxMovementCollisionChecks", foldout);
            FieldInspectorView.AddField(target, "m_MaxPenetrationChecks", foldout);
            FieldInspectorView.AddField(target, "m_MaxRotationCollisionChecks", foldout);
            container.Add(foldout);
            foldout = PersistentFoldout("Moving Platforms");
            FieldInspectorView.AddField(target, "m_StickToMovingPlatform", foldout);
            FieldInspectorView.AddField(target, "m_MovingPlatformSeperationVelocity", foldout);
            FieldInspectorView.AddField(target, "m_MovingPlatformForceDamping", foldout);
            container.Add(foldout);
        }
    }
}