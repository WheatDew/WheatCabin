using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Battlehub
{
    public class SerializeIgnore : System.Attribute
    {
    }


    public static class Reflection
    {
        public static string GetAssemblyQualifiedName(Type type)
        {
            return CleanAssemblyName(type.AssemblyQualifiedName);
        }

        public static string CleanAssemblyName(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                return name;
            }

            name = Regex.Replace(name, @", Version=\d+.\d+.\d+.\d+", string.Empty);
            name = Regex.Replace(name, @", Culture=\w+", string.Empty);
            name = Regex.Replace(name, @", PublicKeyToken=\w+", string.Empty);
            return name;
        }

        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        public static IEnumerable<KeyValuePair<Type, object>> GetTypesWithAttribute(this Assembly assembly, Type attribute)
        {
            foreach (Type type in assembly.GetTypes())
            {
                object[] attributes = type.GetCustomAttributes(attribute, true);
                if (attributes.Length > 0)
                {
                    yield return new KeyValuePair<Type, object>(type, attributes[0]);
                }
            }
        }

        public static T GetCustomAttribute<T>(Type type, out Type typeWithAttribute) where T : Attribute
        {
            typeWithAttribute = null;
            while(type != null)
            {
                T attribute = type.GetCustomAttribute<T>();
                if(attribute != null)
                {
                    typeWithAttribute = type;
                    return attribute;
                }
                type = type.BaseType;
            }
            return null;
        }

        public static bool TryConvert(this string input, Type type, out object result)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(type);
                if (converter != null)
                {
                    result = converter.ConvertFromString(null, CultureInfo.InvariantCulture, input);
                    return true;
                }
                result = GetDefault(type);
                return false;
            }
            catch (NotSupportedException)
            {
                result = GetDefault(type);
                return false;
            }
        }

        public static object GetDefault(Type type)
        {
            if(type == typeof(string))
            {
                return string.Empty;
            }
            if (type.IsValueType())
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static bool IsDelegate(Type type)
        {
            return typeof(MulticastDelegate).IsAssignableFrom(type.BaseType);
        }

        public static bool HasParameterlessConstructor(Type type)
        {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsScript(this Type type)
        {
            return type.IsSubclassOf(typeof(MonoBehaviour));
        }

        public static PropertyInfo PropertyInfo<T>(string name)
        {
            return typeof(T).GetProperty(name);
        }

        public static FieldInfo FieldInfo<T>(string name)
        {
            return typeof(T).GetField(name);
        }

        public static PropertyInfo[] GetSerializableProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).
                Where(p => IsSerializable(p)).ToArray();
        }


        public static FieldInfo[] GetSerializableFields(this Type type, bool declaredOnly = true)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            if (declaredOnly)
            {
                flags |= BindingFlags.DeclaredOnly;
            }
            return type.GetFields(flags).Where(f => IsSerializable(f)).ToArray();
        }

        private static bool IsSerializable(this FieldInfo field)
        {
            return (field.IsPublic || field.IsDefined(typeof(SerializeField), false)) && !field.IsDefined(typeof(SerializeIgnore), false);
        }

        private static bool IsSerializable(this PropertyInfo property)
        {
            return property.CanWrite && property.GetSetMethod(/*nonPublic*/ true).IsPublic &&
                   property.CanRead && property.GetGetMethod(true).IsPublic &&
                   !property.IsDefined(typeof(SerializeIgnore), false) &&
                   property.GetIndexParameters().Length == 0;
        }

        public static Type[] GetAllFromCurrentAssembly()
        {
#if !UNITY_WINRT || UNITY_EDITOR
            var types = typeof(Reflection).Assembly.GetTypes();
#else
            var types = typeof(Reflection).GetTypeInfo().Assembly.GetTypes();
#endif
            return types.ToArray();
        }


        public static Type[] GetAssignableFromTypes(Type type)
        {
#if !UNITY_WINRT || UNITY_EDITOR
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.IsClass);
#else
            var types = type.GetTypeInfo().Assembly.GetTypes().
                Where(p => type.IsAssignableFrom(p) && p.GetTypeInfo().IsClass);
#endif
            return types.ToArray();
        }


#if UNITY_WINRT && !UNITY_EDITOR
        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static bool IsValueType(this Type type)
        {
              return type.GetTypeInfo().IsValueType;
        }

        public static bool IsPrimitive(this Type type)
        {
              return type.GetTypeInfo().IsPrimitive;
        }

         public static bool IsArray(this Type type)
        {
            return type.GetTypeInfo().IsArray;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsAssignableFrom(this Type type, Type fromType)
        {
            return type.GetTypeInfo().IsAssignableFrom(fromType.GetTypeInfo());
        }

        public static bool IsSubclassOf(this Type type, Type ofType)
        {
            return type.GetTypeInfo().IsSubclassOf(ofType);
        }

        public static bool IsDefined(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().IsDefined(attributeType, inherit);
        }

        public static object[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }

        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        public static ConstructorInfo GetConstructor(this Type type, Type[] types)
        {
             return type.GetTypeInfo().GetConstructor(types);
        }
#else
        public static Type BaseType(this Type type)
        {
            return type.BaseType;      
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.IsPrimitive;
        }

        public static bool IsArray(this Type type)
        {
            return type.IsArray;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        public static bool IsEnum(this Type type)
        {
            return type.IsEnum;
        }

        public static bool IsClass(this Type type)
        {
            return type.IsClass;
        }

#endif

        public static T GetValue<T>(this FieldInfo fieldInfo, object obj)
        {
            return (T)fieldInfo.GetValue(obj);
        }
    }

}
