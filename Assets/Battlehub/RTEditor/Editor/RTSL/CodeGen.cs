using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityObject = UnityEngine.Object;

namespace Battlehub.RTSL
{
    /// <summary>
    /// This class is responsible for code generation of persistent objects (surrogates) 
    /// </summary>
    public class CodeGen
    {
        public static bool InitializeLists = false;
        public static bool InitializeDictionaries = false;
        public static bool InitializeHs = false;

        /// <summary>
        /// Automatically generated fields have ProtoMember tag offset = 256. 1 - 256 is reserved for user defined fields.
        /// User defined fields should be located in auto-generated partial class.
        /// </summary>
        private const int AutoFieldTagOffset = 256;

        /// <summary>
        /// Subclass offset which is used in TypeModel creator code. 
        /// (1024 value means, that there is 1024 - 256 - 1 = 767 slots available for auto-generated fields
        /// </summary>
        private const int SubclassOffset = 1024;

        /// <summary>
        /// skip backing fields generated for auto properties
        /// </summary>
        private const string k__BackingField = "k__BackingField";

        /// <summary>
        /// For text formatting
        /// </summary>
        private static readonly string BR = Environment.NewLine;
        private static readonly string END = BR + BR;
        private static readonly string TAB = "    ";
        private static readonly string TAB2 = "        ";
        private static readonly string TAB3 = "            ";
        private static readonly string SEMICOLON = ";";

        /// <summary>
        /// Default namespaces which will be included in all auto-generated classes
        /// </summary>
        private static string[] DefaultNamespaces =
        {
            "System.Collections.Generic",
            "ProtoBuf",
            "Battlehub.RTSL"
        };

        //Templates
        private static readonly string PersistentClassTemplate =
            "{0}" + BR +
            "using UnityObject = UnityEngine.Object;" + BR +
            "namespace {1}" + BR +
            "{{" + BR +
            "    [ProtoContract]" + BR +
            "    public partial class {2} : {3}" + BR +
            "    {{" + BR +
            "        {4}" +
            "    }}" + BR +
            "}}" + END;

        private static readonly string UserDefinedClassTemplate =
           "#if !RTSL_MAINTENANCE" + BR +
           "{0}" + BR +
           "namespace {1}" + BR +
           "{{" + BR +
           "    [CustomImplementation]" + BR +
           "    public partial class {2} {3}" + BR +
           "    {{" +
           "        {4}" + BR +
           "    }}" + BR +
           "}}" + BR +
           "#endif" + END;

        private static readonly string UserDefinedEmptyClassTemplate =
            "#if !RTSL_MAINTENANCE" + BR +
            "{0}" + BR +
            "namespace {1}" + BR +
            "{{" + BR +
            "    [CustomImplementation]" + BR +
            "    public partial class {2}" + BR +
            "    {{" + BR +
            "        /*" + BR +
            "        public override void ReadFrom(object obj)" + BR +
            "        {{" + BR +
            "            base.ReadFrom(obj);" + BR +
            "        }}" + BR + BR +
            "        public override object WriteTo(object obj)" + BR +
            "        {{" + BR +
            "            return base.WriteTo(obj);" + BR +
            "        }}" + BR + BR +
            "        public override void GetDeps(GetDepsContext<TID> context)" + BR +
            "        {{" + BR +
            "            base.GetDeps(context);" + BR +
            "        }}" + BR + BR +
            "        public override void GetDepsFrom(object obj, GetDepsFromContext context)" + BR +
            "        {{" + BR +
            "            base.GetDepsFrom(obj, context);" + BR +
            "        }}" + BR +
            "        */" + BR +
            "    }}" + BR +
            "}}" + BR +
            "#endif" + END;

        private static readonly string FieldTemplate =
            "[ProtoMember({0})]" + BR + TAB2 +
            "public {1} {2};" + END + TAB2;

        private static readonly string FieldInitializationTemplate =
            "[ProtoMember({0})]" + BR + TAB2 +
            "public {1} {2} = new {1}();" + END + TAB2;

        private static readonly string ReplacementFieldInitializationTemplate =
            "[ProtoMember({0})]" + BR + TAB2 +
            "public TID[] {1} = new TID[0];" + END + TAB2;

        private static readonly string ReadFromMethodTemplate =
            "protected override void ReadFromImpl(object obj)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    base.ReadFromImpl(obj);" + BR + TAB2 +
            "    {1} uo = ({1})obj;" + BR + TAB2 +
            "{0}" +
            "}}" + BR;

        private static readonly string WriteToMethodTemplate =
            "protected override object WriteToImpl(object obj)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    obj = base.WriteToImpl(obj);" + BR + TAB2 +
            "    {1} uo = ({1})obj;" + BR + TAB2 +
            "{0}" +
            "    return uo;" + BR + TAB2 +
            "}}" + BR;

        private static readonly string GetDepsMethodTemplate =
            "protected override void GetDepsImpl(GetDepsContext<TID> context)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    base.GetDepsImpl(context);" + BR + TAB2 +
            "{0}" +
            "}}" + BR;

        private static readonly string GetDepsFromMethodTemplate =
            "protected override void GetDepsFromImpl(object obj, GetDepsFromContext context)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    base.GetDepsFromImpl(obj, context);" + BR + TAB2 +
            "    {1} uo = ({1})obj;" + BR + TAB2 +
            "{0}" +
            "}}" + BR;


        private static readonly string ImplicitOperatorsTemplate =
            "public static implicit operator {0}({1} surrogate)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    if(surrogate == null) return default({0});" + BR + TAB2 +
            "    return ({0})surrogate.WriteTo(new {0}());" + BR + TAB2 +
            "}}" + BR + TAB2 +
            BR + TAB2 +
            "public static implicit operator {1}({0} obj)" + BR + TAB2 +
            "{{" + BR + TAB2 +
            "    {1} surrogate = new {1}();" + BR + TAB2 +
            "    surrogate.ReadFrom(obj);" + BR + TAB2 +
            "    return surrogate;" + BR + TAB2 +
            "}}" + BR;

        private static readonly string TypeModelCreatorTemplate =
            "using ProtoBuf.Meta;" + BR +
            "using UnityEngine;" + BR +
            "using Battlehub.RTCommon;" + BR +
            "using Battlehub.RTSL.Interface;" + BR +
            "{0}" + BR +
            "namespace Battlehub.RTSL" + BR +
            "{{" + BR +
            "    public partial class TypeModelCreator : ITypeModelCreator" + BR +
            "    {{" + BR +
            "        #if UNITY_EDITOR" + BR +
            "        [UnityEditor.InitializeOnLoadMethod]" + BR +
            "        #endif" + BR +
            "        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]" + BR +
            "        static void Register()" + BR +
            "        {{" + BR +
            "            IOC.UnregisterFallback<ITypeModelCreator>();" + BR +
            "            IOC.RegisterFallback<ITypeModelCreator>(() => new TypeModelCreator());" + BR +
            "        }}" + BR +
            "        static partial void RegisterUserDefinedTypes(RuntimeTypeModel model);" + BR +
            "        void ITypeModelCreator.Create(RuntimeTypeModel model)" + BR +
            "        {{" + BR +
            "            {1}" + BR +
            "            RegisterUserDefinedTypes(model);" + BR +
            "        }}" + BR +
            "    }}" + BR +
            "}}" + BR +
            "{2}" + END;

        private static readonly string NamespaceDefinitionTemplate =
            "namespace {0} {{}}";

        private static readonly string AddTypeTemplate =
            "model.Add(typeof({0}), {1}){2}";

        private static readonly string AddSubtypeTemplate =
            ".AddSubType({1}, typeof({0}))";

        private static readonly string SetSerializationSurrogate =
            ".SetSurrogate(typeof({0}))";

        private static readonly string TypeMapCreatorTemplate =
            "using Battlehub.RTCommon;" + BR +
            "using Battlehub.RTSL.Interface;" + BR +
            "{0}" + BR +
            "namespace Battlehub.RTSL" + BR +
            "{{" + BR +
            "    public class TypeMapCreator : ITypeMapCreator" + BR +
            "    {{" + BR +
            "        #if UNITY_EDITOR" + BR +
            "        [UnityEditor.InitializeOnLoadMethod]" + BR +
            "        #endif" + BR +
            "        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]" + BR +
            "        static void Register()" + BR +
            "        {{" + BR +
            "            IOC.UnregisterFallback<ITypeMapCreator>();" + BR +
            "            IOC.RegisterFallback<ITypeMapCreator>(() => new TypeMapCreator());" + BR +
            "        }}" + BR +
            "        void ITypeMapCreator.Create(ITypeMap typeMap)" + BR +
            "        {{" + BR +
            "            {1}" +
            "        }}" + BR +
            "    }}" + BR +
            "}}" + END;

        private static readonly string RegisterTypeMappingTemplate =
            "typeMap.Register(typeof({0}), typeof({1}), new System.Guid(\"{2}\"), new System.Guid(\"{3}\"));" + BR;

        private struct _TID { }

        /// <summary>
        /// Short names for primitive types
        /// </summary>
        private static Dictionary<Type, string> m_primitiveNames = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(short), "short" },
            { typeof(byte), "byte" },
            { typeof(ulong), "ulong" },
            { typeof(uint), "uint" },
            { typeof(ushort), "ushort" },
            { typeof(char), "char" },
            { typeof(object), "object" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(bool), "bool" },
            { typeof(string[]), "string[]" },
            { typeof(long[]), "long[]" },
            { typeof(int[]), "int[]" },
            { typeof(short[]), "short[]" },
            { typeof(byte[]), "byte[]" },
            { typeof(ulong[]), "ulong[]" },
            { typeof(uint[]), "uint[]" },
            { typeof(ushort[]), "ushort[]" },
            { typeof(char[]), "char[]" },
            { typeof(object[]), "object[]" },
            { typeof(float[]), "float[]" },
            { typeof(double[]), "double[]" },
            { typeof(bool[]), "bool[]" },
            { typeof(_TID), "TID" },
            { typeof(_TID[]), "TID[]" },
        };


        /// <summary>
        /// Get all public properties with getter and setter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(Type type)
        {
            return GetAllProperties(type).Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null).ToArray();
        }

        /// <summary>
        /// Get all public instance declared only properties (excluding indexers)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetAllProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => IsValidPropertyOrFieldType(p.PropertyType) && p.GetIndexParameters().Length == 0).ToArray();
        }

        /// <summary>
        /// Get all public instance declared only fields
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldInfo[] GetFields(Type type)
        {
            Func<FieldInfo, bool> filter = f => IsValidPropertyOrFieldType(f.FieldType) && !f.Name.Contains(k__BackingField);
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                return type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Union(
                    type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(f => (f.FieldType.IsPublic || f.FieldType.IsNestedPublic) && f.GetCustomAttributes(typeof(SerializeField), true).Length > 0)).Where(filter).ToArray();
            }

            return type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Union(
                    type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(f => (f.FieldType.IsPublic || f.FieldType.IsNestedPublic))).Where(filter).ToArray();
        }

        private static bool IsValidPropertyOrFieldType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (type.IsArray)
            {
                return _IsValidPropertyOrFieldType(type.GetElementType());
            }
            else if (IsGenericList(type) || IsHashSet(type))
            {
                return _IsValidPropertyOrFieldType(type.GetGenericArguments()[0]);
            }
            else if (IsDictionary(type))
            {
                Type[] genericArgs = type.GetGenericArguments();
                return _IsValidPropertyOrFieldType(genericArgs[0]) && _IsValidPropertyOrFieldType(genericArgs[1]);
            }

            return _IsValidPropertyOrFieldType(type);
        }

        private static bool _IsValidPropertyOrFieldType(Type type)
        {
            return (type.IsSubclassOf(typeof(UnityObject)) || type == typeof(UnityObject) || type == typeof(string) || Reflection.HasParameterlessConstructor(type)) &&
                !type.IsGenericType &&
                !type.IsArray &&
                !IsGenericList(type) &&
                !IsHashSet(type) &&
                !IsDictionary(type) &&
                (type.IsPublic || type.IsNestedPublic) &&
                !Reflection.IsDelegate(type) &&
                !type.FullName.Contains("<");
        }

        public static MethodInfo[] GetMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public static bool IsGenericList(Type type)
        {
            bool isList = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
            return isList;
        }

        public static bool IsHashSet(Type type)
        {
            bool isHs = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);
            return isHs;
        }

        public static bool IsDictionary(Type type)
        {
            bool isDict = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            return isDict;
        }

        /// <summary>
        /// Get type which is not subclass of UnityObject and "suitable" to be persistent object
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetSurrogateType(Type type, int index)
        {
            if (type == null)
            {
                return null;
            }

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (IsGenericList(type) || IsHashSet(type))
            {
                type = type.GetGenericArguments()[0];
            }
            else if (IsDictionary(type))
            {
                type = type.GetGenericArguments()[index];
            }

            if (!type.IsSubclassOf(typeof(UnityObject)) &&
                 type != typeof(UnityObject) &&
                !type.IsEnum &&
                !type.IsGenericType &&
                !type.IsArray &&
                !IsGenericList(type) &&
                !IsHashSet(type) &&
                !IsDictionary(type) &&
                !type.IsPrimitive &&
                (type.IsPublic || type.IsNestedPublic) &&
                Reflection.HasParameterlessConstructor(type) &&
                type != typeof(string))
            {
                return type;
            }
            return null;
        }

        public Type GetPersistentType(string fullTypeName)
        {
            string assemblyName = KnownAssemblies.Names[0];//  typeof(PersistentSurrogate<>).Assembly.FullName;
            string assemblyQualifiedName = Assembly.CreateQualifiedName(assemblyName, fullTypeName + "`1");
            Type persistentType = Type.GetType(assemblyQualifiedName);
            return persistentType;
        }


        /// <summary>
        /// Returns true if type has fields or properties referencing UnityObjects. Search is done recursively.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasDependencies(Type type)
        {
            return HasDependenciesRecursive(type, new HashSet<Type>());
        }

        private bool HasDependencies(Type type, HashSet<Type> inspectedTypes)
        {
            if (type.IsArray)
            {
                return HasDependencies(inspectedTypes, type.GetElementType());
            }
            else if (IsGenericList(type) || IsHashSet(type))
            {
                return HasDependencies(inspectedTypes, type.GetGenericArguments()[0]);
            }
            else if (IsDictionary(type))
            {
                Type[] args = type.GetGenericArguments();
                return HasDependencies(inspectedTypes, args[0]) || HasDependencies(inspectedTypes, args[1]);
            }

            return HasDependencies(inspectedTypes, type);
        }

        private bool HasDependencies(HashSet<Type> inspectedTypes, Type type)
        {
            if (inspectedTypes.Contains(type))
            {
                return false;
            }

            inspectedTypes.Add(type);

            if (type.IsSubclassOf(typeof(UnityEventBase)))
            {
                return true;
            }

            PropertyInfo[] properties = GetProperties(type);
            for (int i = 0; i < properties.Length; ++i)
            {
                PropertyInfo property = properties[i];
                if (HasDependenciesRecursive(property.PropertyType, inspectedTypes))
                {
                    return true;
                }
            }

            FieldInfo[] fields = GetFields(type);
            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo field = fields[i];
                if (HasDependenciesRecursive(field.FieldType, inspectedTypes))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasDependenciesRecursive(Type type, HashSet<Type> inspectedTypes)
        {
            if (type.IsArray)
            {
                return HasDependenciesRecursive(inspectedTypes, type.GetElementType());
            }
            else if (IsGenericList(type) || IsHashSet(type))
            {
                return HasDependenciesRecursive(inspectedTypes, type.GetGenericArguments()[0]);
            }
            else if (IsDictionary(type))
            {
                Type[] args = type.GetGenericArguments();
                return HasDependenciesRecursive(inspectedTypes, args[0]) || HasDependenciesRecursive(inspectedTypes, args[1]);
            }

            return HasDependenciesRecursive(inspectedTypes, type);
        }

        private bool HasDependenciesRecursive(HashSet<Type> inspectedTypes, Type type)
        {
            if (type.IsSubclassOf(typeof(UnityObject)))
            {
                return true;
            }

            if (type.IsSubclassOf(typeof(UnityEventBase)))
            {
                return true;
            }

            if (IsDictionary(type))
            {
                Type surrogateType0 = GetSurrogateType(type, 0);
                Type surrogateType1 = GetSurrogateType(type, 1);

                return surrogateType0 != null && HasDependencies(surrogateType0, inspectedTypes) ||
                       surrogateType1 != null && HasDependencies(surrogateType1, inspectedTypes);
            }
            else
            {
                Type surrogateType = GetSurrogateType(type, 0);
                if (surrogateType != null)
                {
                    return HasDependencies(surrogateType, inspectedTypes);
                }
            }

            return false;
        }

        /// <summary>
        /// Returns type name (including names of nested types)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TypeName(Type type)
        {
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                if (elementType.DeclaringType == null)
                {
                    return _TypeName(type);
                }

                return TypeName(elementType.DeclaringType) + "+" + _TypeName(type);
            }

            if (type.DeclaringType == null)
            {
                return _TypeName(type);
            }

            return TypeName(type.DeclaringType) + "+" + _TypeName(type);
        }

        private static string _TypeName(Type type)
        {
            if (type.IsGenericType)
            {
                string name = type.FullName;
                name = Regex.Replace(name, @", Version=\d+.\d+.\d+.\d+", string.Empty);
                name = Regex.Replace(name, @", Culture=\w+", string.Empty);
                name = Regex.Replace(name, @", PublicKeyToken=\w+", string.Empty);

                if (!string.IsNullOrEmpty(Namespace(type)))
                {
                    name = name.Remove(0, Namespace(type).Length + 1);
                }
                return name;
            }
            return type.Name;
        }

        public static string FullTypeName(Type type)
        {
            if (type.DeclaringType == null)
            {
                string name = type.FullName;
                name = Regex.Replace(name, @", Version=\d+.\d+.\d+.\d+", string.Empty);
                name = Regex.Replace(name, @", Culture=\w+", string.Empty);
                name = Regex.Replace(name, @", PublicKeyToken=\w+", string.Empty);
                return name;
            }

            return FullTypeName(type.DeclaringType) + "+" + _TypeName(type);
        }

        private string DictionaryPersistentArgTypeName(Type type)
        {
            string argTypeName;
            if (type.IsSubclassOf(typeof(UnityObject)))
            {
                argTypeName = PrepareMappedTypeName(TypeName(type));
            }
            else
            {
                bool isGenericList = IsGenericList(type);
                if (isGenericList || IsHashSet(type))
                {
                    Type argType = type.GetGenericArguments()[0];
                    string genericArgTypeName;
                    if (!m_primitiveNames.TryGetValue(argType, out genericArgTypeName))
                    {
                        genericArgTypeName = "NotImplemented";
                    }

                    if (isGenericList)
                    {
                        argTypeName = string.Format("List<{0}>", genericArgTypeName);
                    }
                    else
                    {
                        argTypeName = string.Format("HashSet<{0}>", genericArgTypeName);
                    }
                }
                else if (!m_primitiveNames.TryGetValue(type, out argTypeName))
                {
                    argTypeName = "Persistent" + PreparePersistentTypeName(TypeName(type), "TID");
                }
            }

            return argTypeName;
        }

        private string DictionaryMappedArgTypeName(Type type)
        {
            string argTypeName;
            bool isGenericList = IsGenericList(type);
            if (isGenericList || IsHashSet(type))
            {
                Type argType = type.GetGenericArguments()[0];
                string genericArgTypeName;
                if (!m_primitiveNames.TryGetValue(argType, out genericArgTypeName))
                {
                    genericArgTypeName = "NotImplemented";
                }

                if (isGenericList)
                {
                    argTypeName = string.Format("List<{0}>", genericArgTypeName);
                }
                else
                {
                    argTypeName = string.Format("HashSet<{0}>", genericArgTypeName);
                }
            }
            else if (!m_primitiveNames.TryGetValue(type, out argTypeName))
            {
                argTypeName = PrepareMappedTypeName(TypeName(type));
            }
            return argTypeName;
        }

        public string PreparePersistentTypeName(string typeName, string genericArg)
        {
            if (typeName.StartsWith("Battlehub."))
            {
                typeName = typeName.Remove(0, 10);
            }

            typeName = typeName.Replace("+", "Nested");
            if (genericArg == null)
            {
                return typeName;
            }

            typeName = string.Format("{0}<{1}>", typeName, genericArg);
            typeName = typeName.Replace("`1", "");
            return typeName.Replace(string.Format("[]<{0}>", genericArg), string.Format("<{0}>[]", genericArg));
        }

        public string PrepareMappedTypeName(string typeName)
        {
            if (typeName.StartsWith("Battlehub."))
            {
                typeName = typeName.Remove(0, 10);
            }

            return typeName.Replace("+", ".");
        }

        //Generate C# code of TypeMapCreator for selected mappings
        public string CreateTypeMapCreator(PersistentClassMapping[] mappings)
        {
            string usings = "";// CreateUsings(mappings);
            string body = CreateTypeMapCreatorBody(mappings);

            return string.Format(TypeMapCreatorTemplate, usings, body);
        }

        private string CreateTypeMapCreatorBody(PersistentClassMapping[] mappings)
        {
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < mappings.Length; ++m)
            {
                PersistentClassMapping mapping = mappings[m];
                if (mapping == null)
                {
                    continue;
                }

                if (!mapping.IsOn)
                {
                    continue;
                }

                Type mappingType = Type.GetType(mapping.MappedAssemblyQualifiedName);
                if (mappingType == null)
                {
                    continue;
                }

                string mappedTypeName = PrepareMappedTypeName(FullTypeName(mappingType));
                string persistentTypeName = PreparePersistentTypeName(mapping.PersistentFullTypeName, "");
                if (mappedTypeName == "Object")
                {
                    mappedTypeName = "UnityObject";
                }
                sb.AppendFormat(RegisterTypeMappingTemplate, mappedTypeName, persistentTypeName, mapping.MappedTypeGUID, mapping.PersistentTypeGUID);
                sb.Append(TAB3);
            }
            sb.Append(BR);
            return sb.ToString();
        }

        //Generate C# code of TypeModelCreator for selected mappings
        public string CreateTypeModelCreator(PersistentClassMapping[] mappings)
        {
            Type[] idTypes = RTSLSettings.IDTypes;
            if (idTypes == null || idTypes.Length == 0)
            {
                return string.Empty;
            }

            HashSet<string> usingsHs = new HashSet<string>();
            StringBuilder usingsBuilder = new StringBuilder();
            for (int i = 0; i < idTypes.Length; ++i)
            {
                Type idType = idTypes[i];
                if (!usingsHs.Contains(idType.Namespace) && !m_primitiveNames.ContainsKey(idType))
                {
                    usingsHs.Add(idType.Namespace);
                    usingsBuilder.Append($"using {idType.Namespace};");
                    usingsBuilder.Append(BR);
                }
            }

            StringBuilder bodyBuilder = new StringBuilder();
            for (int i = 0; i < idTypes.Length - 1; ++i)
            {
                CreateTypeModelCreator(mappings, bodyBuilder, idTypes[i]);
                bodyBuilder.Append(BR + TAB3);
            }
            CreateTypeModelCreator(mappings, bodyBuilder, idTypes[idTypes.Length - 1]);

            string usings = usingsBuilder.ToString();
            string body = bodyBuilder.ToString();
            string nsDefinitions = CreateNamespaceDefinitions(mappings);
            return string.Format(TypeModelCreatorTemplate, usings, body, nsDefinitions);
        }

        private void CreateTypeModelCreator(PersistentClassMapping[] mappings, StringBuilder bodyBuilder, Type idType)
        {
            string typeName;
            if (!m_primitiveNames.TryGetValue(idType, out typeName))
            {
                typeName = idType.Name;
            }

            bodyBuilder.Append(CreateTypeModelCreatorBody(mappings, typeName));
        }

        private string CreateNamespaceDefinitions(PersistentClassMapping[] mappings)
        {
            StringBuilder sb = new StringBuilder();

            HashSet<string> nsHs = new HashSet<string>();
            for (int m = 0; m < mappings.Length; ++m)
            {
                PersistentClassMapping mapping = mappings[m];
                if (!mapping.IsOn)
                {
                    continue;
                }

                if (!nsHs.Contains(mapping.PersistentNamespace))
                {
                    nsHs.Add(mapping.PersistentNamespace);
                }

                for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
                {
                    PersistentPropertyMapping pMapping = mapping.PropertyMappings[i];
                    if (!pMapping.IsEnabled || pMapping.HasPropertyInTemplate)
                    {
                        continue;
                    }
                    if (!nsHs.Contains(pMapping.PersistentNamespace))
                    {
                        nsHs.Add(pMapping.PersistentNamespace);
                    }
                }
            }

            foreach (string ns in nsHs)
            {
                sb.AppendFormat(NamespaceDefinitionTemplate, ns);
                sb.Append(BR);
            }

            return sb.ToString();
        }

        private string CreateTypeModelCreatorBody(PersistentClassMapping[] mappings, string genericArg)
        {
            StringBuilder sb = new StringBuilder();
            for (int m = 0; m < mappings.Length; ++m)
            {
                PersistentClassMapping mapping = mappings[m];
                if (mapping == null)
                {
                    continue;
                }

                if (!mapping.IsOn)
                {
                    continue;
                }
                string endOfLine = string.Empty;
                if (mapping.Subclasses != null && mapping.Subclasses.Where(s => s.IsEnabled).Count() > 0)
                {
                    endOfLine = CreateAddSubtypesBody(mapping, genericArg);
                }

                Type mappingType = Type.GetType(mapping.MappedAssemblyQualifiedName);
                if (mappingType == null)
                {
                    Debug.LogWarning("Type " + mapping.MappedAssemblyQualifiedName + " was not found");
                }
                else
                {
                    sb.AppendFormat(AddTypeTemplate, PreparePersistentTypeName(mapping.PersistentFullTypeName, genericArg), "true", endOfLine + SEMICOLON + BR + TAB3);

                    if (GetSurrogateType(mappingType, 0) != null)
                    {
                        if (!mappingType.IsSubclassOf(typeof(UnityEventBase)))
                        {
                            endOfLine = string.Format(SetSerializationSurrogate, PreparePersistentTypeName(mapping.PersistentFullTypeName, genericArg));
                            sb.AppendFormat(AddTypeTemplate, PrepareMappedTypeName(mapping.MappedFullTypeName), "false", endOfLine + SEMICOLON + BR + TAB3);
                        }
                    }

                }
            }

            return sb.ToString();
        }

        private string CreateAddSubtypesBody(PersistentClassMapping mapping, string genericArg)
        {
            StringBuilder sb = new StringBuilder();
            PersistentSubclass[] subclasses = mapping.Subclasses.Where(sc => sc.IsEnabled).ToArray();
            for (int i = 0; i < subclasses.Length - 1; ++i)
            {
                PersistentSubclass subclass = subclasses[i];

                Type mappingType = Type.GetType(subclass.MappedAssemblyQualifiedName);
                if (mappingType == null)
                {
                    Debug.LogWarning("Type " + subclass.MappedAssemblyQualifiedName + " was not found");
                }
                else
                {
                    sb.Append(BR + TAB3 + TAB);
                    sb.AppendFormat(AddSubtypeTemplate, PreparePersistentTypeName(subclass.FullTypeName, genericArg), subclass.PersistentTag + SubclassOffset);
                }
            }

            if (subclasses.Length > 0)
            {
                if (subclasses[subclasses.Length - 1].IsEnabled)
                {
                    PersistentSubclass subclass = subclasses[subclasses.Length - 1];

                    Type mappingType = Type.GetType(subclass.MappedAssemblyQualifiedName);
                    if (mappingType == null)
                    {
                        Debug.LogWarning("Type " + subclass.MappedAssemblyQualifiedName + " was not found");
                    }
                    else
                    {
                        sb.Append(BR + TAB3 + TAB);
                        sb.AppendFormat(AddSubtypeTemplate, PreparePersistentTypeName(subclass.FullTypeName, genericArg), subclass.PersistentTag + SubclassOffset);
                    }
                }
            }

            return sb.ToString();
        }

        public static bool TryGetTemplateUsings(string template, out string result)
        {
            result = string.Empty;

            int startIndex = template.IndexOf("//<TEMPLATE_USINGS_START>");
            int endIndex = template.IndexOf("//<TEMPLATE_USINGS_END>");

            if (startIndex < 0 || endIndex < 0 || startIndex >= endIndex)
            {
                return false;
            }

            template = template.Substring(startIndex, endIndex - startIndex);
            template = template.Replace("//<TEMPLATE_USINGS_START>", string.Empty);

            result = template;
            return true;
        }

        public static bool TryGetTemplateInterfaces(string template, out string result)
        {
            result = string.Empty;

            int startIndex = template.IndexOf("//<TEMPLATE_INTERFACES_START>");
            int endIndex = template.IndexOf("//<TEMPLATE_INTERFACES_END>");

            if (startIndex < 0 || endIndex < 0 || startIndex >= endIndex)
            {
                return false;
            }

            template = template.Substring(startIndex, endIndex - startIndex);
            template = template.Replace("//<TEMPLATE_INTERFACES_START>", string.Empty);

            result = " : " + template.Trim(Environment.NewLine.ToCharArray()).Trim(' ');

            return true;
        }

        public static bool TryGetTemplateBody(string template, out string result)
        {
            result = string.Empty;

            int startIndex = template.IndexOf("//<TEMPLATE_BODY_START>");
            int endIndex = template.IndexOf("//<TEMPLATE_BODY_END>");

            if (startIndex < 0 || endIndex < 0 || startIndex >= endIndex)
            {
                return false;
            }

            template = template.Substring(startIndex, endIndex - startIndex);
            template = template.Replace("//<TEMPLATE_BODY_START>", string.Empty);
            template = template.Replace("#if RTSL_COMPILE_TEMPLATES", string.Empty);
            template = template.Replace("#endif", string.Empty);
            template = template.Replace("_RTSL_Template", "<TID>");

            result = template;
            return true;
        }

        public string CreatePersistentClassCustomImplementation(string ns, string persistentTypeName, PersistentTemplateInfo template = null)
        {
            string usings = "using Battlehub.RTSL;";
            string className = PreparePersistentTypeName(persistentTypeName, "TID");
            if (template != null)
            {
                usings += template.Usings;
                return string.Format(UserDefinedClassTemplate, usings, ns, className, template.Interfaces, template.Body.TrimEnd());
            }

            return string.Format(UserDefinedEmptyClassTemplate, usings, ns, className);
        }


        /// <summary>
        /// Generate C# code of persistent class using persistent class mapping
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public string CreatePersistentClass(PersistentClassMapping mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }
            string usings = CreateUsings(mapping);
            string ns = mapping.PersistentNamespace;
            string className = PreparePersistentTypeName(mapping.PersistentTypeName, "TID");
            string baseClassName = mapping.PersistentBaseTypeName != null ?
                 PreparePersistentTypeName(mapping.PersistentBaseTypeName, "TID") : null;
            string body = mapping.IsOn ? CreatePersistentClassBody(mapping) : string.Empty;
            return string.Format(PersistentClassTemplate, usings, ns, className, baseClassName, body);
        }

        private string CreatePersistentClassBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
            {
                PersistentPropertyMapping prop = mapping.PropertyMappings[i];
                if (!prop.IsEnabled || prop.HasPropertyInTemplate)
                {
                    continue;
                }

                if (prop.MappedType == null)
                {
                    continue;
                }

                string typeName = GetTypeName(prop);
                int tag = prop.PersistentTag - 1;
                if (InitializeLists && IsGenericList(prop.MappedType) || InitializeHs && IsHashSet(prop.MappedType))
                {
                    Type replacementType = GetReplacementType(prop.MappedType);
                    if (replacementType == null)
                    {
                        sb.AppendFormat(
                            FieldInitializationTemplate, tag + AutoFieldTagOffset,
                            typeName,
                            prop.PersistentName);
                    }
                    else if (replacementType == typeof(_TID[]))
                    {
                        sb.AppendFormat(
                            ReplacementFieldInitializationTemplate, tag + AutoFieldTagOffset,
                            prop.PersistentName);
                    }
                }
                else
                {
                    if (IsDictionary(prop.MappedType))
                    {
                        string fieldTemplate;
                        if (InitializeDictionaries)
                        {
                            fieldTemplate = FieldInitializationTemplate;
                        }
                        else
                        {
                            fieldTemplate = FieldTemplate;
                        }


                        Type[] args = prop.MappedType.GetGenericArguments();
                        Type t0 = GetReplacementType(args[0]);
                        Type t1 = GetReplacementType(args[1]);
                        if (t0 != null && t1 != null)
                        {
                            sb.AppendFormat(
                                fieldTemplate, tag + AutoFieldTagOffset,
                                "Dictionary<TID, TID>",
                                prop.PersistentName);
                        }
                        else if (t0 != null)
                        {

                            sb.AppendFormat(
                                fieldTemplate, tag + AutoFieldTagOffset,
                                string.Format("Dictionary<TID, {0}>", DictionaryPersistentArgTypeName(args[1])),
                                prop.PersistentName);
                        }
                        else if (t1 != null)
                        {
                            sb.AppendFormat(
                                fieldTemplate, tag + AutoFieldTagOffset,
                                string.Format("Dictionary<{0}, TID>", DictionaryPersistentArgTypeName(args[0])),
                                prop.PersistentName);
                        }
                        else
                        {
                            sb.AppendFormat(
                                fieldTemplate, tag + AutoFieldTagOffset,
                                string.Format("Dictionary<{0}, {1}>", DictionaryPersistentArgTypeName(args[0]), DictionaryPersistentArgTypeName(args[1])),
                                prop.PersistentName);
                        }
                    }
                    else
                    {
                        sb.AppendFormat(
                            FieldTemplate, tag + AutoFieldTagOffset,
                            typeName,
                            prop.PersistentName);
                    }
                }
            }

            string readMethodBody = CreateReadMethodBody(mapping);
            string writeMethodBody = CreateWriteMethodBody(mapping);
            string getDepsMethodBody = CreateDepsMethodBody(mapping);
            string getDepsFromMethodBody = CreateDepsFromMethodBody(mapping);

            string mappedTypeName = ToMappedTypeName(mapping);

            if (!string.IsNullOrEmpty(readMethodBody))
            {
                sb.AppendFormat(ReadFromMethodTemplate, readMethodBody, mappedTypeName);
            }

            if (!string.IsNullOrEmpty(writeMethodBody))
            {
                sb.Append(BR + TAB2);
                sb.AppendFormat(WriteToMethodTemplate, writeMethodBody, mappedTypeName);
            }

            if (!string.IsNullOrEmpty(getDepsMethodBody))
            {
                sb.Append(BR + TAB2);
                sb.AppendFormat(GetDepsMethodTemplate, getDepsMethodBody);
            }
            if (!string.IsNullOrEmpty(getDepsFromMethodBody))
            {
                sb.Append(BR + TAB2);
                sb.AppendFormat(GetDepsFromMethodTemplate, getDepsFromMethodBody, mappedTypeName);
            }

            Type mappingType = Type.GetType(mapping.MappedAssemblyQualifiedName);
            if (mappingType != null)
            {
                if (Reflection.HasParameterlessConstructor(mappingType))
                {
                    if (!mappingType.IsSubclassOf(typeof(UnityObject)))
                    {
                        sb.Append(BR + TAB2);
                        sb.AppendFormat(ImplicitOperatorsTemplate, mappedTypeName, PreparePersistentTypeName(mapping.PersistentTypeName, "TID"));
                    }
                }
            }

            return sb.ToString();
        }

        private string ToMappedTypeName(PersistentClassMapping mapping)
        {
            string mappedTypeName = PrepareMappedTypeName(mapping.MappedTypeName);
            if (mappedTypeName == "Object")
            {
                mappedTypeName = "UnityObject";
            }

            return mappedTypeName;
        }


        private string GetTypeName(PersistentPropertyMapping prop, bool useReplacementType = true, bool forceMappedTypeName = false)
        {
            string typeName;
            Type repacementType = GetReplacementType(prop.MappedType);
            if (repacementType != null && useReplacementType)
            {
                string primitiveTypeName;
                if (m_primitiveNames.TryGetValue(repacementType, out primitiveTypeName))
                {
                    typeName = primitiveTypeName;
                }
                else
                {
                    typeName = PrepareMappedTypeName(repacementType.Name);
                }
            }
            else
            {
                string primitiveTypeName;
                if (m_primitiveNames.TryGetValue(prop.MappedType, out primitiveTypeName))
                {
                    typeName = primitiveTypeName;
                }
                else
                {
                    if (IsGenericList(prop.MappedType))
                    {
                        Type argType = prop.MappedType.GetGenericArguments()[0];
                        typeName = string.Format("List<{0}>", GetArgName(prop, argType, forceMappedTypeName));
                    }
                    else if (IsHashSet(prop.MappedType))
                    {
                        Type argType = prop.MappedType.GetGenericArguments()[0];
                        typeName = string.Format("HashSet<{0}>", GetArgName(prop, argType, forceMappedTypeName));
                    }
                    else if (IsDictionary(prop.MappedType))
                    {
                        Type[] args = prop.MappedType.GetGenericArguments();
                        typeName = string.Format("Dictionary<{0},{1}>",
                            GetArgName(prop, args[0], forceMappedTypeName),
                            GetArgName(prop, args[1], forceMappedTypeName));
                    }
                    else
                    {
                        if (prop.UseSurrogate && !forceMappedTypeName)
                        {
                            typeName = "Persistent" + PreparePersistentTypeName(prop.PersistentTypeName, "TID");
                        }
                        else
                        {
                            typeName = PrepareMappedTypeName(prop.MappedTypeName);
                        }
                    }
                }
            }

            return typeName;
        }

        private string GetArgName(PersistentPropertyMapping prop, Type type, bool forceMappedTypeName)
        {
            string primitiveTypeName;
            if (m_primitiveNames.TryGetValue(type, out primitiveTypeName))
            {
                return primitiveTypeName;
            }

            if (prop.UseSurrogate && !forceMappedTypeName)
            {
                return string.Format("Persistent{0}", PreparePersistentTypeName(TypeName(type), "TID"));
            }

            return PrepareMappedTypeName(FullTypeName(type));
        }

        private string CreateReadMethodBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();
            string mappedTypeName = ToMappedTypeName(mapping);
            for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
            {
                PersistentPropertyMapping prop = mapping.PropertyMappings[i];
                if (!prop.IsEnabled || prop.HasPropertyInTemplate)
                {
                    continue;
                }

                if (prop.MappedType == null)
                {
                    continue;
                }

                if (prop.MappedName == null || prop.MappedName.EndsWith(k__BackingField))
                {
                    continue;
                }

                sb.Append(TAB);

                string get = "uo.{1}";
                if (prop.IsNonPublic)
                {
                    get = "GetPrivate<" + mappedTypeName + "," + GetTypeName(prop, false, true) + ">(uo, \"{1}\")";
                }

                if (IsDictionary(prop.MappedType))
                {
                    Type[] args = prop.MappedType.GetGenericArguments();
                    bool isArg0UnityObj = args[0].IsSubclassOf(typeof(UnityObject));
                    bool isArg1UnityObj = args[1].IsSubclassOf(typeof(UnityObject));
                    if (isArg0UnityObj || isArg1UnityObj)
                    {
                        if (isArg0UnityObj && isArg1UnityObj)
                        {
                            sb.AppendFormat("{0} = ToID(" + get + ");", prop.PersistentName, prop.MappedName);
                        }
                        else if (isArg0UnityObj)
                        {
                            sb.AppendFormat("{0} = ToID(" + get + ", v_ => ({2})v_);", prop.PersistentName, prop.MappedName,
                                DictionaryPersistentArgTypeName(args[1]));
                        }
                        else
                        {
                            sb.AppendFormat("{0} = ToID(" + get + ", k_ => ({2})k_);", prop.PersistentName, prop.MappedName,
                                DictionaryPersistentArgTypeName(args[0]));
                        }
                    }
                    else
                    {
                        sb.AppendFormat("{0} = Assign(" + get + ", k_ => ({2})k_, v_ => ({3})v_);",
                            prop.PersistentName, prop.MappedName,
                            DictionaryPersistentArgTypeName(args[0]),
                            DictionaryPersistentArgTypeName(args[1]));
                    }
                }
                else
                {
                    if (prop.MappedType.IsSubclassOf(typeof(UnityObject)) ||
                        prop.MappedType.IsArray && prop.MappedType.GetElementType().IsSubclassOf(typeof(UnityObject)) ||
                        (IsGenericList(prop.MappedType) || IsHashSet(prop.MappedType)) && prop.MappedType.GetGenericArguments()[0].IsSubclassOf(typeof(UnityObject)))
                    {
                        //generate code which will convert unity object to identifier
                        sb.AppendFormat("{0} = ToID(" + get + ");", prop.PersistentName, prop.MappedName);
                    }
                    else
                    {
                        if (prop.UseSurrogate)
                        {
                            if (IsGenericList(prop.MappedType) || IsHashSet(prop.MappedType))
                            {
                                sb.AppendFormat("{0} = Assign(" + get + ", v_ => ({2})v_);", prop.PersistentName, prop.MappedName, PreparePersistentTypeName("Persistent" + TypeName(prop.MappedType.GetGenericArguments()[0]), "TID"));
                            }
                            else if (prop.MappedType.IsArray)
                            {
                                sb.AppendFormat("{0} = Assign(" + get + ", v_ => ({2})v_);", prop.PersistentName, prop.MappedName, PreparePersistentTypeName("Persistent" + TypeName(prop.MappedType.GetElementType()), "TID"));
                            }
                            else
                            {
                                sb.AppendFormat("{0} = " + get + ";", prop.PersistentName, prop.MappedName);
                            }
                        }
                        else
                        {
                            sb.AppendFormat("{0} = " + get + ";", prop.PersistentName, prop.MappedName);
                        }
                    }
                }

                sb.Append(BR + TAB2);
            }

            return sb.ToString();
        }

        private string CreateWriteMethodBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();
            string mappedTypeName = ToMappedTypeName(mapping);

            for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
            {
                PersistentPropertyMapping prop = mapping.PropertyMappings[i];
                if (!prop.IsEnabled || prop.HasPropertyInTemplate)
                {
                    continue;
                }
                if (prop.MappedType == null)
                {
                    continue;
                }
                if (prop.MappedName == null || prop.MappedName.EndsWith(k__BackingField))
                {
                    continue;
                }

                sb.Append(TAB);

                string get = "uo.{0}";
                if (prop.IsNonPublic)
                {
                    get = "GetPrivate<" + mappedTypeName + "," + GetTypeName(prop, false, true) + ">(uo, \"{0}\")";
                }

                if (IsDictionary(prop.MappedType))
                {
                    Type[] args = prop.MappedType.GetGenericArguments();
                    bool isArg0UnityObj = args[0].IsSubclassOf(typeof(UnityObject));
                    bool isArg1UnityObj = args[1].IsSubclassOf(typeof(UnityObject));
                    if (isArg0UnityObj || isArg1UnityObj)
                    {
                        if (isArg0UnityObj && isArg1UnityObj)
                        {
                            if (prop.IsNonPublic)
                            {
                                sb.AppendFormat("SetPrivate(uo, \"{0}\", FromID({1}, " + get + "));", prop.MappedName, prop.PersistentName);
                            }
                            else
                            {
                                sb.AppendFormat("uo.{0} = FromID({1}, " + get + ");", prop.MappedName, prop.PersistentName);
                            }
                        }
                        else if (isArg0UnityObj)
                        {
                            if (prop.IsNonPublic)
                            {
                                sb.AppendFormat("SetPrivate(uo, \"{0}\", FromID({1}, v_ => ({2})v_, " + get + "));",
                                    prop.MappedName, prop.PersistentName,
                                    DictionaryMappedArgTypeName(args[1]));
                            }
                            else
                            {
                                sb.AppendFormat("uo.{0} = FromID({1}, v_ => ({2})v_, " + get + ");",
                                    prop.MappedName, prop.PersistentName,
                                    DictionaryMappedArgTypeName(args[1]));
                            }
                        }
                        else
                        {
                            if (prop.IsNonPublic)
                            {
                                sb.AppendFormat("SetPrivate(uo, \"{0}\", FromID({1}, k_ => ({2})k_, " + get + "));",
                                    prop.MappedName, prop.PersistentName,
                                    DictionaryMappedArgTypeName(args[0]));
                            }
                            else
                            {
                                sb.AppendFormat("uo.{0} = FromID({1}, k_ => ({2})k_, " + get + ");",
                                    prop.MappedName, prop.PersistentName,
                                    DictionaryMappedArgTypeName(args[0]));
                            }
                        }
                    }
                    else
                    {
                        if (prop.IsNonPublic)
                        {
                            sb.AppendFormat("SetPrivate(uo, \"{0}\", Assign({1}, k_ => ({2})k_, v_ => ({3})v_));",
                                prop.MappedName, prop.PersistentName,
                                DictionaryMappedArgTypeName(args[0]),
                                DictionaryMappedArgTypeName(args[1]));
                        }
                        else
                        {
                            sb.AppendFormat("uo.{0} = Assign({1}, k_ => ({2})k_, v_ => ({3})v_);",
                                prop.MappedName, prop.PersistentName,
                                DictionaryMappedArgTypeName(args[0]),
                                DictionaryMappedArgTypeName(args[1]));
                        }
                    }
                }
                else
                {
                    if (prop.MappedType.IsSubclassOf(typeof(UnityObject)) ||
                        prop.MappedType.IsArray && prop.MappedType.GetElementType().IsSubclassOf(typeof(UnityObject)) ||
                        (IsGenericList(prop.MappedType) || IsHashSet(prop.MappedType)) && prop.MappedType.GetGenericArguments()[0].IsSubclassOf(typeof(UnityObject)))
                    {
                        if (prop.IsNonPublic)
                        {
                            sb.AppendFormat("SetPrivate(uo, \"{0}\", FromID({1}, " + get + "));", prop.MappedName, prop.PersistentName);
                        }
                        else
                        {
                            sb.AppendFormat("uo.{0} = FromID({1}, " + get + ");", prop.MappedName, prop.PersistentName);
                        }
                    }
                    else
                    {
                        if (prop.UseSurrogate)
                        {
                            if (IsGenericList(prop.MappedType) || IsHashSet(prop.MappedType))
                            {
                                if (prop.IsNonPublic)
                                {
                                    sb.AppendFormat("SetPrivate(uo, \"{0}\", Assign({1}, v_ => ({2})v_));", prop.MappedName, prop.PersistentName, PrepareMappedTypeName(TypeName(prop.MappedType.GetGenericArguments()[0])));
                                }
                                else
                                {
                                    sb.AppendFormat("uo.{0} = Assign({1}, v_ => ({2})v_);", prop.MappedName, prop.PersistentName, PrepareMappedTypeName(TypeName(prop.MappedType.GetGenericArguments()[0])));
                                }
                            }
                            else if (prop.MappedType.IsArray)
                            {
                                if (prop.IsNonPublic)
                                {
                                    sb.AppendFormat("SetPrivate(uo, \"{0}\", Assign({1}, v_ => ({2})v_));", prop.MappedName, prop.PersistentName, PrepareMappedTypeName(TypeName(prop.MappedType.GetElementType())));
                                }
                                else
                                {
                                    sb.AppendFormat("uo.{0} = Assign({1}, v_ => ({2})v_);", prop.MappedName, prop.PersistentName, PrepareMappedTypeName(TypeName(prop.MappedType.GetElementType())));
                                }
                            }
                            else
                            {
                                if (prop.IsNonPublic)
                                {
                                    sb.AppendFormat("SetPrivate<{0}, {1}>(uo, \"{2}\", {3});", mappedTypeName, GetTypeName(prop, false, true), prop.MappedName, prop.PersistentName);
                                }
                                else
                                {
                                    sb.AppendFormat("uo.{0} = {1};", prop.MappedName, prop.PersistentName);
                                }
                            }
                        }
                        else
                        {
                            if (prop.IsNonPublic)
                            {
                                sb.AppendFormat("SetPrivate(uo, \"{0}\", {1});", prop.MappedName, prop.PersistentName);
                            }
                            else
                            {
                                sb.AppendFormat("uo.{0} = {1};", prop.MappedName, prop.PersistentName);
                            }
                        }
                    }
                }

                sb.Append(BR + TAB2);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generate method which will populate context with dependencies (referenced unity object identifiers)
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private string CreateDepsMethodBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
            {
                PersistentPropertyMapping prop = mapping.PropertyMappings[i];
                if (!prop.IsEnabled || prop.HasPropertyInTemplate)
                {
                    continue;
                }
                if (prop.MappedType == null)
                {
                    continue;
                }

                if (prop.MappedName == null || prop.MappedName.EndsWith(k__BackingField))
                {
                    continue;
                }

                if (prop.HasDependenciesOrIsDependencyItself)
                {
                    if (IsDictionary(prop.MappedType))
                    {
                        Type[] args = prop.MappedType.GetGenericArguments();
                        bool isArg0UnityObj = args[0].IsSubclassOf(typeof(UnityObject));
                        bool isArg1UnityObj = args[1].IsSubclassOf(typeof(UnityObject));
                        if (isArg0UnityObj && isArg1UnityObj)
                        {
                            sb.Append(TAB);
                            sb.AppendFormat("AddDep({0}, context);", prop.PersistentName);
                            sb.Append(BR + TAB2);
                        }
                        else
                        {
                            sb.Append(TAB);
                            sb.AppendFormat("AddSurrogateDeps({0}, context);", prop.PersistentName);
                            sb.Append(BR + TAB2);
                        }
                    }
                    else
                    {
                        if (prop.UseSurrogate)
                        {
                            sb.Append(TAB);
                            sb.AppendFormat("AddSurrogateDeps({0}, context);", prop.PersistentName);
                            sb.Append(BR + TAB2);
                        }
                        else if (prop.MappedType.IsSubclassOf(typeof(UnityObject)) ||
                            prop.MappedType.IsArray && prop.MappedType.GetElementType().IsSubclassOf(typeof(UnityObject)) ||
                            (IsGenericList(prop.MappedType) || IsHashSet(prop.MappedType)) && prop.MappedType.GetGenericArguments()[0].IsSubclassOf(typeof(UnityObject)))
                        {
                            sb.Append(TAB);
                            sb.AppendFormat("AddDep({0}, context);", prop.PersistentName);
                            sb.Append(BR + TAB2);
                        }
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generate method which will extract and populate context with dependencies (referenced unity objects)
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private string CreateDepsFromMethodBody(PersistentClassMapping mapping)
        {
            StringBuilder sb = new StringBuilder();
            string mappedTypeName = ToMappedTypeName(mapping);

            for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
            {
                PersistentPropertyMapping prop = mapping.PropertyMappings[i];
                if (!prop.IsEnabled || prop.HasPropertyInTemplate)
                {
                    continue;
                }
                if (prop.MappedType == null)
                {
                    continue;
                }

                if (prop.MappedName == null || prop.MappedName.EndsWith(k__BackingField))
                {
                    continue;
                }


                if (prop.HasDependenciesOrIsDependencyItself)
                {
                    string get = "uo.{0}";
                    if (prop.IsNonPublic)
                    {
                        get = "GetPrivate<" + mappedTypeName + "," + GetTypeName(prop, false, true) + ">(uo, \"{0}\")";
                    }

                    if (IsDictionary(prop.MappedType))
                    {
                        Type[] args = prop.MappedType.GetGenericArguments();
                        bool isArg0UnityObj = args[0].IsSubclassOf(typeof(UnityObject));
                        bool isArg1UnityObj = args[1].IsSubclassOf(typeof(UnityObject));
                        if (isArg0UnityObj && isArg1UnityObj)
                        {
                            sb.Append(TAB);
                            sb.AppendFormat("AddDep(" + get + ", context);", prop.MappedName);
                            sb.Append(BR + TAB2);
                        }
                        else
                        {
                            sb.Append(TAB);

                            sb.AppendFormat("AddSurrogateDeps(" + get + ", k_ => ({1})k_, v_ => ({2})v_, context);", prop.MappedName,
                                DictionaryPersistentArgTypeName(args[0]),
                                DictionaryPersistentArgTypeName(args[1]));

                            sb.Append(BR + TAB2);
                        }
                    }
                    else
                    {
                        if (prop.UseSurrogate)
                        {
                            sb.Append(TAB);

                            string persistentTypeName;
                            if (prop.MappedType != null && (IsGenericList(prop.MappedType) || IsHashSet(prop.MappedType)))
                            {
                                Type type = prop.MappedType.GetGenericArguments()[0];
                                persistentTypeName = PreparePersistentTypeName("Persistent" + TypeName(type), "TID");
                            }
                            else if (prop.MappedType != null && prop.MappedType.IsArray)
                            {
                                Type type = prop.MappedType.GetElementType();
                                persistentTypeName = PreparePersistentTypeName("Persistent" + TypeName(type), "TID");
                            }
                            else
                            {
                                persistentTypeName = PreparePersistentTypeName("Persistent" + prop.PersistentTypeName, "TID");
                            }

                            sb.AppendFormat("AddSurrogateDeps(" + get + ", v_ => ({1})v_, context);", prop.MappedName, persistentTypeName);
                            sb.Append(BR + TAB2);
                        }
                        if (prop.MappedType.IsSubclassOf(typeof(UnityObject)) ||
                            prop.MappedType.IsArray && prop.MappedType.GetElementType().IsSubclassOf(typeof(UnityObject)) ||
                            (IsGenericList(prop.MappedType) || IsHashSet(prop.MappedType)) && prop.MappedType.GetGenericArguments()[0].IsSubclassOf(typeof(UnityObject)))
                        {
                            sb.Append(TAB);
                            sb.AppendFormat("AddDep(" + get + ", context);", prop.MappedName);
                            sb.Append(BR + TAB2);
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private string CreateUsings(params PersistentClassMapping[] mappings)
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> namespaces = new HashSet<string>();
            for (int i = 0; i < DefaultNamespaces.Length; ++i)
            {
                namespaces.Add(DefaultNamespaces[i]);
            }

            for (int m = 0; m < mappings.Length; ++m)
            {
                PersistentClassMapping mapping = mappings[m];
                if (mapping == null)
                {
                    continue;
                }

                if (mapping.IsOn)
                {
                    if (!namespaces.Contains(mapping.MappedNamespace) && !string.IsNullOrEmpty(mapping.MappedNamespace))
                    {
                        namespaces.Add(mapping.MappedNamespace);
                    }

                    if (!namespaces.Contains(mapping.PersistentNamespace))
                    {
                        namespaces.Add(mapping.PersistentNamespace);
                    }

                    if (!namespaces.Contains(mapping.PersistentBaseNamespace))
                    {
                        namespaces.Add(mapping.PersistentBaseNamespace);
                    }

                    for (int i = 0; i < mapping.PropertyMappings.Length; ++i)
                    {
                        PersistentPropertyMapping propertyMapping = mapping.PropertyMappings[i];
                        if (!propertyMapping.IsEnabled || propertyMapping.HasPropertyInTemplate)
                        {
                            continue;
                        }
                        if (propertyMapping.MappedName == null || propertyMapping.MappedName.EndsWith(k__BackingField))
                        {
                            continue;
                        }

                        if (!namespaces.Contains(propertyMapping.MappedNamespace) && !string.IsNullOrEmpty(propertyMapping.MappedNamespace))
                        {
                            namespaces.Add(propertyMapping.MappedNamespace);
                        }

                        Type type = propertyMapping.MappedType;
                        if (type != null)
                        {
                            AddNamespace(type, namespaces, propertyMapping.PersistentNamespace);

                            if (IsDictionary(type))
                            {
                                Type[] args = type.GetGenericArguments();
                                if (!namespaces.Contains(Namespace(args[0])) && !string.IsNullOrEmpty(Namespace(args[0])))
                                {
                                    namespaces.Add(Namespace(args[0]));
                                }

                                AddNamespace(args[0], namespaces, PersistentClassMapping.ToPersistentNamespace(Namespace(args[0])));

                                if (!namespaces.Contains(Namespace(args[1])) && !string.IsNullOrEmpty(Namespace(args[1])))
                                {
                                    namespaces.Add(Namespace(args[1]));
                                }

                                AddNamespace(args[1], namespaces, PersistentClassMapping.ToPersistentNamespace(Namespace(args[1])));

                            }
                            else if (IsGenericList(type) || IsHashSet(type))
                            {
                                type = type.GetGenericArguments()[0];
                                if (!namespaces.Contains(Namespace(type)) && !string.IsNullOrEmpty(Namespace(type)))
                                {
                                    namespaces.Add(Namespace(type));
                                }

                                AddNamespace(type, namespaces, PersistentClassMapping.ToPersistentNamespace(Namespace(type)));
                            }
                            else if (type.IsArray)
                            {
                                type = type.GetElementType();
                                if (!namespaces.Contains(Namespace(type)) && !string.IsNullOrEmpty(Namespace(type)))
                                {
                                    namespaces.Add(Namespace(type));
                                }

                                AddNamespace(type, namespaces, PersistentClassMapping.ToPersistentNamespace(Namespace(type)));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Unable to resolve type: " + propertyMapping.MappedAssemblyQualifiedName);
                        }
                    }
                }
            }
            foreach (string ns in namespaces)
            {
                sb.Append("using " + ns + ";" + BR);
            }

            return sb.ToString();
        }

        public void AddNamespace(Type type, HashSet<string> namespaces, string persistentNamespace)
        {
            Type replacementType = GetReplacementType(type);
            if (replacementType != null)
            {
                if (!namespaces.Contains(Namespace(replacementType)))
                {
                    namespaces.Add(Namespace(replacementType));
                }
            }
            else
            {
                if (!type.FullName.StartsWith("System"))
                {
                    if (!namespaces.Contains(persistentNamespace))
                    {
                        namespaces.Add(persistentNamespace);
                    }
                }
            }
        }

        private Type GetReplacementType(Type type)
        {
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                if (elementType.IsSubclassOf(typeof(UnityObject)))
                {
                    return typeof(_TID[]);
                }
            }

            if (IsGenericList(type) || IsHashSet(type))
            {
                Type elementType = type.GetGenericArguments()[0];
                if (elementType.IsSubclassOf(typeof(UnityObject)))
                {
                    return typeof(_TID[]);
                }
            }

            if (type.IsSubclassOf(typeof(UnityObject)))
            {
                return typeof(_TID);
            }
            return null;
        }

        private static void GetTypesRecursive(Type type, HashSet<Type> typesHS)
        {
            PropertyInfo[] properties = GetAllProperties(type);
            FieldInfo[] fields = GetFields(type);
            MethodInfo[] methods = GetMethods(type);

            for (int p = 0; p < properties.Length; ++p)
            {
                PropertyInfo pInfo = properties[p];
                if (!typesHS.Contains(pInfo.PropertyType))
                {
                    Type surrogateType = GetSurrogateType(pInfo.PropertyType, 0);
                    if (surrogateType != null && !typesHS.Contains(surrogateType))
                    {
                        typesHS.Add(surrogateType);
                        GetTypesRecursive(surrogateType, typesHS);
                    }

                    if (IsDictionary(pInfo.PropertyType))
                    {
                        surrogateType = GetSurrogateType(pInfo.PropertyType, 1);
                        if (surrogateType != null && !typesHS.Contains(surrogateType))
                        {
                            typesHS.Add(surrogateType);
                            GetTypesRecursive(surrogateType, typesHS);
                        }
                    }
                }
            }

            for (int f = 0; f < fields.Length; ++f)
            {
                FieldInfo fInfo = fields[f];
                if (!typesHS.Contains(fInfo.FieldType))
                {
                    Type surrogateType = GetSurrogateType(fInfo.FieldType, 0);
                    if (surrogateType != null && !typesHS.Contains(surrogateType))
                    {
                        typesHS.Add(surrogateType);
                        GetTypesRecursive(surrogateType, typesHS);
                    }

                    if (IsDictionary(fInfo.FieldType))
                    {
                        surrogateType = GetSurrogateType(fInfo.FieldType, 1);
                        if (surrogateType != null && !typesHS.Contains(surrogateType))
                        {
                            typesHS.Add(surrogateType);
                            GetTypesRecursive(surrogateType, typesHS);
                        }
                    }
                }
            }

            for (int m = 0; m < methods.Length; ++m)
            {
                MethodInfo mInfo = methods[m];
                ParameterInfo[] parameters = mInfo.GetParameters();
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        ParameterInfo pInfo = parameters[i];
                        if (pInfo != null && pInfo.ParameterType != null)
                        {
                            Type surrogateType = GetSurrogateType(pInfo.ParameterType, 0);
                            if (surrogateType != null && !string.IsNullOrEmpty(surrogateType.FullName) && surrogateType != typeof(object) && !typesHS.Contains(surrogateType))
                            {
                                typesHS.Add(surrogateType);
                                GetTypesRecursive(surrogateType, typesHS);
                            }

                            if (IsDictionary(pInfo.ParameterType))
                            {
                                surrogateType = GetSurrogateType(pInfo.ParameterType, 1);
                                if (surrogateType != null && !string.IsNullOrEmpty(surrogateType.FullName) && surrogateType != typeof(object) && !typesHS.Contains(surrogateType))
                                {
                                    typesHS.Add(surrogateType);
                                    GetTypesRecursive(surrogateType, typesHS);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void GetSurrogateAssembliesAndTypes(Assembly[] assembiles, Type[] uoTypes, out Dictionary<string, HashSet<Type>> declaredIn, out Type[] result)
        {
            HashSet<Type> allTypesHS = new HashSet<Type>();
            declaredIn = new Dictionary<string, HashSet<Type>>();

            for (int typeIndex = 0; typeIndex < uoTypes.Length; ++typeIndex)
            {
                Type uoType = uoTypes[typeIndex];

                HashSet<Type> typesHs = new HashSet<Type>();
                GetTypesRecursive(uoType, typesHs);
                declaredIn.Add(uoType.FullName, typesHs);

                foreach (Type type in typesHs)
                {
                    if (!allTypesHS.Contains(type))
                    {
                        allTypesHS.Add(type);
                    }
                }
            }

            //Add System.Serializable types
            for (int i = 0; i < assembiles.Length; ++i)
            {
                Assembly assembly = assembiles[i];
                if (assembly == null)
                {
                    continue;
                }

                Type[] types = assembly.GetTypes();
                for (int typeIndex = 0; typeIndex < types.Length; ++typeIndex)
                {
                    Type type = GetSurrogateType(types[typeIndex], 0);
                    if (type != null && type.GetCustomAttribute<SerializableAttribute>() != null)
                    {
                        if (!allTypesHS.Contains(type))
                        {
                            allTypesHS.Add(type);
                        }
                    }
                }
            }

            result = allTypesHS.ToArray();
        }

        public static string Namespace(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType().Namespace;
            }
            return type.Namespace;
        }

        #region Deprecated
        [Obsolete] //10.02.2021
        public static void GetUOAssembliesAndTypes(out Assembly[] assemblies, out Type[] types)
        {
            ReflectionUtils.GetUOAssembliesAndTypes(out assemblies, out types);
        }
        #endregion
    }
}
