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
    using Opsive.UltimateCharacterController.Editor.Utility;
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the ability class.
    /// </summary>
    public abstract class AbilityDrawer
    {
        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        public virtual void CreateDrawer(UnityEngine.Object unityObject, object target, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent)
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
        public virtual void CreateEditorDrawer(UnityEngine.Object unityObject, object target, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent) { }

        /// <summary>
        /// The ability has been added to the Ultimate Character Locomotion. Perform any initialization.
        /// </summary>
        /// <param name="ability">The ability that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public virtual void AbilityAdded(Ability ability, UnityEngine.Object parent) { }

        /// <summary>
        /// The ability has been removed from the Ultimate Character Locomotion. Perform any destruction.
        /// </summary>
        /// <param name="ability">The ability that has been removed.</param>
        /// <param name="parent">The parent of the removed ability.</param>
        public virtual void AbilityRemoved(Ability ability, UnityEngine.Object parent) { }

        /// <summary>
        /// Returns true if the ability can build to the animator.
        /// </summary>
        public virtual bool CanBuildAnimator { get => true; }

        /// <summary>
        /// Adds the abilities states/transitions to the animator. 
        /// </summary>
        /// <param name="animatorControllers">The Animator Controllers to add the states to.</param>
        /// <param name="firstPersonAnimatorControllers">The first person Animator Controllers to add the states to.</param>
        public virtual void BuildAnimator(UnityEditor.Animations.AnimatorController[] animatorControllers, UnityEditor.Animations.AnimatorController[] firstPersonAnimatorControllers) { }
    }

    /// <summary>
    /// Helper class which will get an AbilityDrawer for the specified type.
    /// </summary>
    public static class AbilityDrawerUtility
    {
        private static Dictionary<Type, Type> s_AbilityDrawerMap;

        /// <summary>
        /// Searches for a AbilityDrawer object of the specified type.
        /// </summary>
        /// <param name="type">The type of control to search for.</param>
        /// <param name="searchBaseTypes">Should base types be searched?</param>
        /// <returns>The found TypeControlBase (can be null).</returns>
        public static AbilityDrawer FindAbilityDrawer(Type type, bool searchBaseTypes)
        {
            if (type == null) {
                return null;
            }

            if (s_AbilityDrawerMap == null) {
                PopulateControlObjects();
            }

            Type abilityDrawerType;
            do {
                if (type.IsGenericType) {
                    type = type.GetGenericTypeDefinition();
                }
                if (s_AbilityDrawerMap.TryGetValue(type, out abilityDrawerType)) {
                    return Activator.CreateInstance(abilityDrawerType) as AbilityDrawer;
                }
                type = type.BaseType;
            } while (searchBaseTypes && type != typeof(object));

            return null;
        }

        /// <summary>
        /// Populates the control objects from the available types.
        /// </summary>
        private static void PopulateControlObjects()
        {
            s_AbilityDrawerMap = new Dictionary<Type, Type>();
            var types = UnitOptions.GetAllEditorTypes();
            for (int i = 0; i < types.Length; ++i) {
                var controlTypeAttributes = types[i].GetCustomAttributes<ControlType>();
                if (controlTypeAttributes != null) {
                    foreach (var attribute in controlTypeAttributes) {
                        if (typeof(AbilityDrawer).IsAssignableFrom(types[i])) {
                            if (s_AbilityDrawerMap.ContainsKey(attribute.Type)) {
                                UnityEngine.Debug.LogError($"Error: Unable to add the ability drawer {attribute.Type} because it has already been added.");
                                continue;
                            }
                            s_AbilityDrawerMap.Add(attribute.Type, types[i]);
                        }
                    }
                }
            }
        }
    }
}