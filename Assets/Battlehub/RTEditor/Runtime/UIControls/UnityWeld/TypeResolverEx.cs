using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityWeld.Binding;
using UnityWeld.Binding.Exceptions;
using UnityWeld.Binding.Internal;

namespace Battlehub.UIControls.Binding
{
    public class TypeResolverEx
    {
        public static string BindableEventToString(BindableEvent evt)
        {
            return string.Concat(evt.ComponentType.ToString(), ".", evt.Name);
        }

        public static BindableMember<PropertyInfo>[] FindBindableIHierarchicalDataProperties(VirtualizingTreeViewBinding target)
        {
            Func<Type, bool> IsHierarchycalData = x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHierarchicalData<>);

            return TypeResolver.FindBindableProperties(target)
                .Where(p => IsHierarchycalData(p.Member.PropertyType) || p.Member.PropertyType.GetInterfaces().Any(IsHierarchycalData))
                .ToArray();
        }

        public static BindableMember<PropertyInfo>[] FindBindableIEnumerableProperties(VirtualizingTreeViewBinding target)
        {
            return TypeResolver.FindBindableProperties(target)
                .Where(p => typeof(IEnumerable).IsAssignableFrom(p.Member.PropertyType))
                .Where(p => !typeof(string).IsAssignableFrom(p.Member.PropertyType))
                .ToArray();
        }

        public static BindableEvent GetBoundEvent(string boundEventName, Component component)
        {
            Assert.IsNotNull(component);
            Assert.IsFalse(string.IsNullOrEmpty(boundEventName));

            var componentType = component.GetType();
            var boundEvent = GetBindableEvents(component)
                .FirstOrDefault(e => e.Name.Equals(boundEventName));

            if (boundEvent == null)
            {
                throw new InvalidEventException(string.Format("Could not bind to event \"{0}\" on component \"{1}\".", boundEventName, componentType));
            }

            return boundEvent;
        }

        public static IEnumerable<BindableEvent> GetBindableEvents(Component component)
        {
            Assert.IsNotNull(component, "Cannot get bindinable events of a null component.");

            var type = component.GetType();

            var bindableEventsFromProperties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(propertyInfo => propertyInfo.PropertyType.IsSubclassOf(typeof(UnityEventBase)))
                .Where(propertyInfo => !propertyInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any())
                .Select(propertyInfo => new BindableEvent()
                {
                    UnityEvent = (UnityEventBase)propertyInfo.GetValue(component, null),
                    Name = propertyInfo.Name,
                    DeclaringType = propertyInfo.DeclaringType,
                    ComponentType = component.GetType()
                });

            var bindableEventsFromFields = type
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(fieldInfo => fieldInfo.FieldType.IsSubclassOf(typeof(UnityEventBase)))
                .Where(fieldInfo => !fieldInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any())
                .Select(fieldInfo => new BindableEvent
                {
                    UnityEvent = (UnityEventBase)fieldInfo.GetValue(component),
                    Name = fieldInfo.Name,
                    DeclaringType = fieldInfo.DeclaringType,
                    ComponentType = type
                });

            return bindableEventsFromFields.Concat(bindableEventsFromProperties);
        }

        public static BindableMember<MethodInfo>[] FindBindableMethods(AbstractMemberBinding targetScript)
        {
            return FindAvailableViewModelTypes(targetScript)
                .SelectMany(type => GetPublicMethods(type)
                    .Select(m => new BindableMember<MethodInfo>(m, type))
                )
                .Where(m => m.Member.GetParameters().Length == 0)
                .Where(m => 
                    {
                        object[] attributes = Attribute.GetCustomAttributes(m.Member.GetBaseDefinition(), true);
                        return attributes.Any(a => a.GetType() == typeof(BindingAttribute)) && !m.MemberName.StartsWith("get_");
                    }) // Exclude property getters, since we aren't doing anything with the return value of the bound method anyway.
                .ToArray();
        }

        /// <summary>
        /// Get all the declared and inherited public methods from a class or interface.
        /// </summary>
        private static IEnumerable<MethodInfo> GetPublicMethods(Type type)
        {
            IEnumerable<MethodInfo> result;
            if (!type.IsInterface)
            {
                result = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            }
            else
            {
                result = (new[] { type })
                    .Concat(type.GetInterfaces())
                    .SelectMany(i => i.GetMethods(BindingFlags.Public | BindingFlags.Instance));
            }
            return result;
        }

        /// <summary>
        /// Scan up the hierarchy and find all the types that can be bound to 
        /// a specified MemberBinding.
        /// </summary>
        private static IEnumerable<Type> FindAvailableViewModelTypes(AbstractMemberBinding memberBinding)
        {
            var foundAtLeastOneBinding = false;

            var trans = memberBinding.transform;
            while (trans != null)
            {
                var components = trans.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    // Can't bind to self or null
                    if (component == null || component == memberBinding)
                    {
                        continue;
                    }

                    // Case where a ViewModelBinding is used to bind a non-MonoBehaviour class.
                    var viewModelBinding = component as IViewModelProvider;
                    if (viewModelBinding != null)
                    {
                        var viewModelTypeName = viewModelBinding.GetViewModelTypeName();
                        // Ignore view model bindings that haven't been set up yet.
                        if (string.IsNullOrEmpty(viewModelTypeName))
                        {
                            continue;
                        }

                        foundAtLeastOneBinding = true;

                        yield return GetViewModelType(viewModelBinding.GetViewModelTypeName());
                    }
                    else if (component.GetType().GetCustomAttributes(typeof(BindingAttribute), false).Any())
                    {
                        // Case where we are binding to an existing MonoBehaviour.
                        foundAtLeastOneBinding = true;

                        yield return component.GetType();
                    }
                }

                trans = trans.parent;
            }

            if (!foundAtLeastOneBinding)
            {
                Debug.LogError("UI binding " + memberBinding.gameObject.name + " must be placed underneath at least one bindable component.", memberBinding);
            }
        }

        private static Type GetViewModelType(string viewModelTypeName)
        {
            var type = TypeResolver.TypesWithBindingAttribute
                .FirstOrDefault(t => t.ToString() == viewModelTypeName);

            if (type == null)
            {
                throw new ViewModelNotFoundException("Could not find the specified view model \"" + viewModelTypeName + "\"");
            }

            return type;
        }

    }

}
