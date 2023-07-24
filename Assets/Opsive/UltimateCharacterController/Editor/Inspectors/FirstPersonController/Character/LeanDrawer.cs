/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Inspectors.FirstPersonController.Character.Abilities
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.Editor.Controls.Types.AbilityDrawers;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Character.Abilities;
    using Opsive.UltimateCharacterController.Character.Identifiers;
    using Opsive.UltimateCharacterController.FirstPersonController.Character.Abilities;
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.UIElements;
    using System.Reflection;

    /// <summary>
    /// Draws a custom drawer for the Lean Ability.
    /// </summary>
    [ControlType(typeof(Lean))]
    public class LeanDrawer : AbilityDrawer
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

        }

        /// <summary>
        /// Returns the editor control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        public override void CreateEditorDrawer(Object unityObject, object target, VisualElement container, System.Func<FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
        {
            base.CreateEditorDrawer(unityObject, target, container, onValidateChange, onChangeEvent);

            var horizontalContainer = new VisualElement();
            horizontalContainer.AddToClassList("horizontal-layout");
            container.Add(horizontalContainer);
            var addCollidersButton = new Button(() =>
            {
                AbilityAdded(target as Lean, (unityObject as Component));
            });
            addCollidersButton.text = "Add Colliders";
            addCollidersButton.style.flexGrow = 1;
            horizontalContainer.Add(addCollidersButton);

            var removeCollidersButton = new Button(() =>
            {
                RemoveColliders(target as Lean, (unityObject as Component).gameObject);
            });
            removeCollidersButton.text = "Remove Colliders";
            removeCollidersButton.style.flexGrow = 1;
            horizontalContainer.Add(removeCollidersButton);
        }

        /// <summary>
        /// The ability has been added to the Ultimate Character Locomotion. Perform any initialization.
        /// </summary>
        /// <param name="ability">The ability that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public override void AbilityAdded(Ability ability, UnityEngine.Object parent)
        {
#if UNITY_EDITOR
            var character = (parent as Component).gameObject;
            var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(character);
            if (!string.IsNullOrEmpty(prefabPath)) {
                PrefabUtility.UnpackPrefabInstance(character, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }
#endif
            var lean = ability as Lean;
            var animatorMonitors = (parent as Component).GetComponentsInChildren<AnimatorMonitor>(true);
            if (animatorMonitors == null || animatorMonitors.Length == 0) {
                lean.Colliders = new Collider[] { AddLeanCollider((parent as Component).gameObject) };
            } else {
                lean.Colliders = new Collider[animatorMonitors.Length];
                for (int i = 0; i < animatorMonitors.Length; ++i) {
                    lean.Colliders[i] = AddLeanCollider(animatorMonitors[i].gameObject);
                }
            }
            Opsive.Shared.Editor.Utility.EditorUtility.SetDirty(parent);

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(prefabPath)) {
                PrefabUtility.SaveAsPrefabAssetAndConnect(character, prefabPath, InteractionMode.AutomatedAction);
            }
#endif
        }

        /// <summary>
        /// The ability has been removed from the Ultimate Character Locomotion. Perform any destruction.
        /// </summary>
        /// <param name="ability">The ability that has been removed.</param>
        /// <param name="parent">The parent of the removed ability.</param>
        public override void AbilityRemoved(Ability ability, UnityEngine.Object parent)
        {
            var leanAbility = ability as Lean;
            RemoveColliders(leanAbility, (parent as Component).gameObject);
        }

        /// <summary>
        /// Adds the collider to the lean ability.
        /// </summary>
        /// <param name="leanAbility">The ability to add the collider to.</param>
        /// <param name="parent">The parent of the lean ability.</param>
        /// <returns>The added collider</returns>
        private Collider AddLeanCollider(GameObject parent)
        {
            // Position the collider under the Colliders GameObject if it exists.
            var collidersTransform = parent.transform.GetComponentInChildren<CharacterColliderBaseIdentifier>()?.transform;
            var leanCollider = new GameObject("LeanCollider");
            leanCollider.layer = LayerManager.IgnoreRaycast;
            leanCollider.transform.SetParentOrigin(collidersTransform != null ? collidersTransform : parent.transform);
            leanCollider.transform.localPosition = new Vector3(0, 1.5f, 0);
            var leanCapsuleCollider = leanCollider.AddComponent<CapsuleCollider>();
            leanCapsuleCollider.radius = 0.3f;
            leanCapsuleCollider.height = 1;
            leanCapsuleCollider.enabled = false;
            return leanCapsuleCollider;
        }

        /// <summary>
        /// Removes the collider from the lean ability.
        /// </summary>
        /// <param name="leanAbility">The ability to remove the collider from.</param>
        /// <param name="parent">The parent of the lean ability.</param>
        private void RemoveColliders(Lean leanAbility, GameObject parent)
        {
#if UNITY_EDITOR
            var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(parent);
            if (!string.IsNullOrEmpty(prefabPath)) {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(parent)) {
                    PrefabUtility.UnpackPrefabInstance(parent, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                } else {
                    prefabPath = string.Empty;
                }
            }
#endif

            for (int i = leanAbility.Colliders.Length - 1; i > -1; --i) {
                UnityEngine.Object.DestroyImmediate(leanAbility.Colliders[i].gameObject, true);
            }
            leanAbility.Colliders = null;

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(prefabPath)) {
                PrefabUtility.SaveAsPrefabAssetAndConnect(parent, prefabPath, InteractionMode.AutomatedAction);
            }
#endif
        }
    }
}