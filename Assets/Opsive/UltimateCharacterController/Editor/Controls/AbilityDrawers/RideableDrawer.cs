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
    using Opsive.UltimateCharacterController.Game;
    using Opsive.UltimateCharacterController.Utility;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements AbilityDrawer for the Rideable ControlType.
    /// </summary>
    [ControlType(typeof(Rideable))]
    public class RideableDrawer : AbilityDrawer
    {
        /// <summary>
        /// The ability has been added to the Ultimate Character Locomotion. Perform any initialization.
        /// </summary>
        /// <param name="ability">The ability that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public override void AbilityAdded(Ability ability, UnityEngine.Object parent)
        {
            AddDismountColliders(ability as Rideable, (parent as Component).gameObject);
        }

        /// <summary>
        /// The ability has been removed from the Ultimate Character Locomotion. Perform any destruction.
        /// </summary>
        /// <param name="ability">The ability that has been removed.</param>
        /// <param name="parent">The parent of the removed ability.</param>
        public override void AbilityRemoved(Ability ability, UnityEngine.Object parent)
        {
            RemoveDismountColliders(ability as Rideable);
        }

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

            var rideableAbility = target as Rideable;
            var horizontalContainer = new VisualElement();
            horizontalContainer.AddToClassList("horizontal-layout");
            container.Add(horizontalContainer);
            var addCollidersButton = new Button(() =>
            {
                AddDismountColliders(rideableAbility, (unityObject as Component).gameObject);
            });
            addCollidersButton.text = "Add Dismount Colliders";
            addCollidersButton.style.flexGrow = 1;
            horizontalContainer.Add(addCollidersButton);

            var removeCollidersButton = new Button(() =>
            {
                RemoveDismountColliders(rideableAbility);
            });
            removeCollidersButton.text = "Remove Dismount Colliders";
            removeCollidersButton.style.flexGrow = 1;
            horizontalContainer.Add(removeCollidersButton);
        }

        /// <summary>
        /// Adds the colliders to the rideable ability.
        /// </summary>
        /// <param name="rideableAbility">The ability to add the colliders to.</param>
        /// <param name="parent">The parent of the rideable ability.</param>
        private void AddDismountColliders(Rideable rideableAbility, GameObject parent)
        {
            // Position the collider under the Colliders GameObject if it exists.
            Transform collidersTransform;
            if ((collidersTransform = parent.transform.Find("Colliders")) != null) {
                parent = collidersTransform.gameObject;
            }
            rideableAbility.LeftDismountCollider = CreateCollider(parent, "Left Dismount Collider", new Vector3(-0.9f, 1, 0));
            rideableAbility.RightDismountCollider = CreateCollider(parent, "Right Dismount Collider", new Vector3(0.9f, 1, 0));
        }

        /// <summary>
        /// Creates the dismount collider
        /// </summary>
        /// <param name="parent">The parent of the rideable ability.</param>
        /// <param name="name">The name of the collider.</param>
        /// <param name="position">The local poistion of the collider.</param>
        /// <returns>The created collider.</returns>
        private Collider CreateCollider(GameObject parent, string name, Vector3 position)
        {
            var collider = new GameObject(name);
            collider.layer = LayerManager.SubCharacter;
            collider.transform.SetParentOrigin(parent.transform);
            collider.transform.localPosition = position;
            var capsuleCollider = collider.AddComponent<CapsuleCollider>();
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.5f;
            return capsuleCollider;
        }

        /// <summary>
        /// Removes the collider from the rideable ability.
        /// </summary>
        /// <param name="rideableAbility">The ability to remove the colliders from.</param>
        private void RemoveDismountColliders(Rideable rideableAbility)
        {
            if (rideableAbility.LeftDismountCollider != null) {
                UnityEngine.Object.DestroyImmediate(rideableAbility.LeftDismountCollider.gameObject, true);
                rideableAbility.LeftDismountCollider = null;
            }
            if (rideableAbility.RightDismountCollider != null) {
                UnityEngine.Object.DestroyImmediate(rideableAbility.RightDismountCollider.gameObject, true);
                rideableAbility.RightDismountCollider = null;
            }
        }
    }
}