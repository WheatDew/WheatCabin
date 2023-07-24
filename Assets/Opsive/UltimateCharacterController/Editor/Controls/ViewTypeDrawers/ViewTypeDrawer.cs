/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Editor.Controls.Types.ViewTypeDrawers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.UltimateCharacterController.Camera.ViewTypes;
    using System;
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom inspector for the view type class.
    /// </summary>
    public abstract class ViewTypeDrawer
    {
        /// <summary>
        /// Returns the control that should be used for the specified ControlType.
        /// </summary>
        /// <param name="unityObject">A reference to the owning Unity Object.</param>
        /// <param name="target">The object that should have its fields displayed.</param>
        /// <param name="container">The container that the UIElements should be added to.</param>
        /// <param name="onValidateChange">Event callback which validates if a field can be changed.</param>
        /// <param name="onChangeEvent">An event that is sent when the value changes. Returns false if the control cannot be changed.</param>
        public abstract void CreateDrawer(UnityEngine.Object unityObject, object target, VisualElement container, System.Func<System.Reflection.FieldInfo, object, bool> onValidateChange, System.Action<object> onChangeEvent);

        /// <summary>
        /// The view type has been added to the Camera Controller. Perform any initialization.
        /// </summary>
        /// <param name="viewType">The view type that has been added.</param>
        /// <param name="parent">The parent of the added ability.</param>
        public virtual void ViewTypeAdded(ViewType viewType, UnityEngine.Object parent) { }

        /// <summary>
        /// The view type has been removed from the Camera Controller. Perform any destruction.
        /// </summary>
        /// <param name="viewType">The view type that has been removed.</param>
        /// <param name="parent">The parent of the removed ability.</param>
        public virtual void ViewTypeRemoved(ViewType viewType, UnityEngine.Object parent) { }
    }

    /// <summary>
    /// Helper class which will get an ViewTypeDrawer for the specified type.
    /// </summary>
    public static class ViewTypeDrawerUtility
    {
        private static Dictionary<Type, Type> s_ViewTypeDrawerMap;

        /// <summary>
        /// Searches for a ViewTypeDrawer object of the specified type.
        /// </summary>
        /// <param name="type">The type of control to search for.</param>
        /// <param name="searchBaseTypes">Should base types be searched?</param>
        /// <returns>The found TypeControlBase (can be null).</returns>
        public static ViewTypeDrawer FindViewTypeDrawer(Type type, bool searchBaseTypes)
        {
            if (type == null) {
                return null;
            }

            if (s_ViewTypeDrawerMap == null) {
                PopulateControlObjects();
            }

            Type abilityDrawerType;
            do {
                if (type.IsGenericType) {
                    type = type.GetGenericTypeDefinition();
                }
                if (s_ViewTypeDrawerMap.TryGetValue(type, out abilityDrawerType)) {
                    return Activator.CreateInstance(abilityDrawerType) as ViewTypeDrawer;
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
            s_ViewTypeDrawerMap = new Dictionary<Type, Type>();
            var types = UnitOptions.GetAllEditorTypes();
            for (int i = 0; i < types.Length; ++i) {
                var controlTypeAttributes = types[i].GetCustomAttributes<ControlType>();
                if (controlTypeAttributes != null) {
                    foreach (var attribute in controlTypeAttributes) {
                        if (typeof(ViewTypeDrawer).IsAssignableFrom(types[i])) {
                            if (s_ViewTypeDrawerMap.ContainsKey(attribute.Type)) {
                                UnityEngine.Debug.LogError($"Error: Unable to add the view type drawer {attribute.Type} because it has already been added.");
                                continue;
                            }
                            s_ViewTypeDrawerMap.Add(attribute.Type, types[i]);
                        }
                    }
                }
            }
        }
    }
}