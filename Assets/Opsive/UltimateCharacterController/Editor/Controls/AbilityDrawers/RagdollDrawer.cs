/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types.AbilityDrawers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Editor.Inspectors.Utility;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Items;
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements AbilityDrawer for the Ragdoll ControlType.
    /// </summary>
    [ControlType(typeof(Ragdoll))]
    public class RagdollInspectorDrawer : AbilityDrawer
    {
        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        public override void CreateDrawer(UnityEngine.Object unityObject, object target, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
        {
            FieldInspectorView.AddFields(unityObject, target, Shared.Utility.MemberVisibility.Public, container, onChangeEvent, null, onValidateChange, false, null, true);

            var horizontalContainer = new VisualElement();
            horizontalContainer.AddToClassList("horizontal-layout");
            container.Add(horizontalContainer);
            var addCollidersButton = new Button(() =>
            {
                AddRagdollColliders((unityObject as Component).gameObject);
            });
            addCollidersButton.text = "Add Ragdoll Colliders";
            addCollidersButton.style.flexGrow = 1;
            horizontalContainer.Add(addCollidersButton);

            var removeCollidersButton = new Button(() =>
            {
                RemoveRagdollColliders((unityObject as Component).gameObject);
            });
            removeCollidersButton.text = "Remove Ragdoll Colliders";
            removeCollidersButton.style.flexGrow = 1;
            horizontalContainer.Add(removeCollidersButton);
        }

        /// <summary>
        /// Uses Unity's Ragdoll Builder to create the ragdoll.
        /// </summary>
        /// <param name="character">The character to add the ragdoll to.</param>
        /// <param name="autoCreate">Should the ragdoll be created automatically without any UI?</param>
        public static void AddRagdollColliders(GameObject character, bool autoCreate = false)
        {
            var ragdollBuilderType = Type.GetType("UnityEditor.RagdollBuilder, UnityEditor");
            var windows = Resources.FindObjectsOfTypeAll(ragdollBuilderType);
            // Open the Ragdoll Builder if it isn't already opened.
            if (windows == null || windows.Length == 0) {
                EditorApplication.ExecuteMenuItem("GameObject/3D Object/Ragdoll...");
                windows = Resources.FindObjectsOfTypeAll(ragdollBuilderType);
            }

            if (windows != null && windows.Length > 0) {
                var ragdollWindow = windows[0] as ScriptableWizard;
                var animatorMonitor = character.GetComponentInChildren<Character.AnimatorMonitor>();
                if (animatorMonitor == null) {
                    return;
                }
                var animator = animatorMonitor.GetComponent<Animator>();
                if (animator == null) {
                    return;
                }

                InspectorUtility.SetFieldValue(ragdollWindow, "pelvis", animator.GetBoneTransform(HumanBodyBones.Hips));
                InspectorUtility.SetFieldValue(ragdollWindow, "leftHips", animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
                InspectorUtility.SetFieldValue(ragdollWindow, "leftKnee", animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
                InspectorUtility.SetFieldValue(ragdollWindow, "leftFoot", animator.GetBoneTransform(HumanBodyBones.LeftFoot));
                InspectorUtility.SetFieldValue(ragdollWindow, "rightHips", animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
                InspectorUtility.SetFieldValue(ragdollWindow, "rightKnee", animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
                InspectorUtility.SetFieldValue(ragdollWindow, "rightFoot", animator.GetBoneTransform(HumanBodyBones.RightFoot));
                InspectorUtility.SetFieldValue(ragdollWindow, "leftArm", animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
                InspectorUtility.SetFieldValue(ragdollWindow, "leftElbow", animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
                InspectorUtility.SetFieldValue(ragdollWindow, "rightArm", animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
                InspectorUtility.SetFieldValue(ragdollWindow, "rightElbow", animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
                InspectorUtility.SetFieldValue(ragdollWindow, "middleSpine", animator.GetBoneTransform(HumanBodyBones.Spine));
                InspectorUtility.SetFieldValue(ragdollWindow, "head", animator.GetBoneTransform(HumanBodyBones.Head));

                var consistencyMethod = ragdollWindow.GetType().GetMethod("CheckConsistency", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (consistencyMethod != null) {
                    ragdollWindow.errorString = (string)consistencyMethod.Invoke(ragdollWindow, null);
                    ragdollWindow.isValid = string.IsNullOrEmpty(ragdollWindow.errorString);
                }

                if (autoCreate) {
                    var createMethod = ragdollWindow.GetType().GetMethod("OnWizardCreate", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (createMethod != null) {
                        try {
                            createMethod.Invoke(ragdollWindow, null);
                            ragdollWindow.Close();
                        } catch (Exception /*e*/) {
                            Debug.LogError("Error: The Ragdoll Builder is not able to automatically create the ragdoll.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the ragdoll colliders from the specified character.
        /// </summary>
        /// <param name="character">The character to remove the ragdoll colliders from.</param>
        public static void RemoveRagdollColliders(GameObject character)
        {
            // If the character is a humanoid then the ragdoll colliders are known ahead of time. Generic characters are required to be searched recursively.
            var animatorMonitor = character.GetComponentInChildren<Character.AnimatorMonitor>();
            if (animatorMonitor == null) {
                return;
            }
            var animator = animatorMonitor.GetComponent<Animator>();
            if (animator != null && animator.GetBoneTransform(HumanBodyBones.Head) != null) {
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.Hips), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.LeftFoot), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.RightFoot), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.Spine), false);
                RemoveRagdollColliders(animator.GetBoneTransform(HumanBodyBones.Head), false);
            } else {
                RemoveRagdollColliders(character.transform, true);
            }
        }

        /// <summary>
        /// Removes the ragdoll colliders from the transform. If removeChildColliders is true then the method will be called recursively.
        /// </summary>
        /// <param name="transform">The transform to remove the colliders from.</param>
        /// <param name="removeChildColliders">True if the colliders should be searched for recursively.</param>
        private static void RemoveRagdollColliders(Transform transform, bool removeChildColliders)
        {
            if (transform == null) {
                return;
            }

            if (removeChildColliders) {
                for (int i = transform.childCount - 1; i >= 0; --i) {
                    var child = transform.GetChild(i);
                    // No ragdoll colliders exist under the Character layer GameObjects no under the item GameObjects.
                    if (child.gameObject.layer == LayerManager.Character || child.GetComponent<ItemPlacement>() != null || child.GetComponent<CharacterItemSlot>() != null) {
                        continue;
                    }

#if FIRST_PERSON_CONTROLLER
                    // First person objects do not contain any ragdoll colliders.
                    if (child.GetComponent<UltimateCharacterController.FirstPersonController.Character.FirstPersonObjects>() != null) {
                        continue;
                    }
#endif
                    // Remove the ragdoll from the transform and recursively check the children.
                    RemoveRagdollCollider(child);
                    RemoveRagdollColliders(child, true);
                }
            } else {
                RemoveRagdollCollider(transform);
            }
        }

        /// <summary>
        /// Removes the ragdoll colliders from the specified transform.
        /// </summary>
        /// <param name="transform">The transform to remove the ragdoll colliders from.</param>
        private static void RemoveRagdollCollider(Transform transform)
        {
            var collider = transform.GetComponent<Collider>();
            var rigidbody = transform.GetComponent<Rigidbody>();
            // If the object doesn't have a collider and a rigidbody then it isn't a ragdoll collider.
            if (collider == null || rigidbody == null) {
                return;
            }
            UnityEngine.Object.DestroyImmediate(collider, true);
            var characterJoint = transform.GetComponent<CharacterJoint>();
            if (characterJoint != null) {
                UnityEngine.Object.DestroyImmediate(characterJoint, true);
            }
            // The rigidbody must be removed last to prevent conflicts.
            UnityEngine.Object.DestroyImmediate(rigidbody, true);
        }
    }
}