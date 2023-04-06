using System;
using UnityEngine;

namespace Battlehub.RTSL.Battlehub.SL2
{
}


namespace Battlehub.RTSL
{

    [Serializable]
    public class PersistentPropertyMapping
    {
        //CodeGen will ignore this PersistentPropertyMapping if IsEnabled is false
        public bool IsEnabled;        
        public bool IsProperty;
        public int PersistentTag;

        public string PersistentFullTypeName
        {
            get
            {
                if(string.IsNullOrEmpty(PersistentNamespace))
                {
                    return PersistentTypeName;
                }
                return PersistentNamespace + "." + PersistentTypeName;
            }
        }

        public string MappedFullTypeName
        {
            get
            {
                if(string.IsNullOrEmpty(MappedNamespace))
                {
                    return MappedTypeName;
                }

                return MappedNamespace + "." + MappedTypeName;
            }
        }

        public string MappedAssemblyQualifiedName
        {
            get { return MappedFullTypeName + "," + MappedAssemblyName; }
        }

        public Type MappedType
        {
            get { return Type.GetType(MappedAssemblyQualifiedName); }
        }

        //Namespace, typename and persistent field name
        public string PersistentNamespace;
        public string PersistentTypeName;
        public string PersistentName;        

        //Namespace, typename and name of the property (name of the field) which is member of mapped type
        public string MappedAssemblyName;
        public string MappedNamespace;
        public string MappedTypeName;
        public string MappedName;
        

        //True if property (or field) is non-unityobject persistent class
        public bool UseSurrogate;

        //True if property (or field) is non-unityobject persistent class (used for second generic argument of dictionary)
        public bool UseSurrogate2; 


        //True if property (or field) is unity object or non-unity object with dependencies
        public bool HasDependenciesOrIsDependencyItself;
        public bool IsNonPublic;

        public bool HasPropertyInTemplate;
    }

    [Serializable]
    public class PersistentSubclass
    {
        public string MappedAssemblyQualifiedName;

        public int PersistentTag;
        public string FullTypeName
        {
            get
            {
                if(string.IsNullOrEmpty(Namespace))
                {
                    return TypeName;
                }

                return Namespace + "." + TypeName;
            }
        }
        public string Namespace;
        public string TypeName;
        public bool IsEnabled;
    }

    public class PersistentClassMapping : MonoBehaviour
    {
        public string Version;

        public string MappedFullTypeName
        {
            get
            {
                if(string.IsNullOrEmpty(MappedNamespace))
                {
                    return MappedTypeName;
                }

                return MappedNamespace + "." + MappedTypeName;
            }
        }

        public string PersistentFullTypeName
        {
            get
            {
                if(string.IsNullOrEmpty(PersistentNamespace))
                {
                    return PersistentTypeName;
                }

                return PersistentNamespace + "." + PersistentTypeName;
            }
        }

        public string MappedAssemblyQualifiedName
        {
            get { return MappedFullTypeName + "," + MappedAssemblyName; }
        }

        public bool IsSelected;
        public bool IsLocked;
        public bool IsOn
        {
            get { return IsSelected || IsLocked; }
        }
        
        public int PersistentPropertyTag;
        public int PersistentSubclassTag;
        public string MappedAssemblyName;
        public string MappedNamespace;
        public string MappedTypeName;
        public string MappedTypeGUID;

        public string PersistentNamespace;
        public string PersistentTypeName;
        public string PersistentTypeGUID;

        public string PersistentBaseNamespace;
        public string PersistentBaseTypeName;

        public bool CreateCustomImplementation;
        public bool UseTemplate;
        /// <summary>
        /// Array of subclasses which is used by CodeGen to generate code of TypeModelCreator
        /// </summary>
        public PersistentSubclass[] Subclasses;
        public PersistentPropertyMapping[] PropertyMappings;

        public static string ToPersistentNamespace(string mappedNamespace)
        {
            if(string.IsNullOrEmpty(mappedNamespace))
            {
                return "Battlehub.SL2";
            }
            return mappedNamespace + ".Battlehub.SL2";
        }

        public static string ToMappedNamespace(string persistentNamespace)
        {
            if(persistentNamespace == "Battlehub.SL2")
            {
                return string.Empty;
            }

            return persistentNamespace.Replace(".Battlehub.SL2", "");
        }

        public static string ToPersistentName(string typeName)
        {
            return "Persistent" + typeName;
        }

        public static string ToPersistentFullName(string ns, string typeName)
        {
            ns = ToPersistentNamespace(ns);
            if(string.IsNullOrEmpty(ns))
            {
                return ToPersistentName(typeName);
            }

            return ns + "." + ToPersistentName(typeName);
        }
    }

    
}


